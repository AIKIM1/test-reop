/*************************************************************************************
 Created Date : 2018.02.28
      Creator : 오화백
   Decription : 파우치 활성화 불량 대차 폐기 - 불량 Cell 등록
--------------------------------------------------------------------------------------
 [Change History]
    
**************************************************************************************/

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
namespace LGC.GMES.MES.COM001
{
    /// <summary>
    /// CMM_SHIFT.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class COM001_219_DEFECT_CELL_INPUT : C1Window, IWorkArea
    {
        #region Declaration & Constructor
     

        Util _Util = new Util();
        BizDataSet _bizRule = new BizDataSet();
        DataTable DEFC_CELL = null;

        private CheckBoxHeaderType _inBoxHeaderType;

        private bool _load = true;
      
        public string NON_WIP_CHK { get; set; }    //비재공 호출 여부

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

        public COM001_219_DEFECT_CELL_INPUT()
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
            DataTable ScrapList = parameters[0] as DataTable;
            if (ScrapList == null)
                return;

            if (NON_WIP_CHK == "Y") //비재공 호출
            {
                SetGridCartList_NON_WIP(ScrapList);
                SetGridDefectList_NON_WIP();
                if (dgCell.Rows.Count > 0)
                {
                    SetGridLotIdRt();
                }
            }
            else
            {
                SetGridCartList(ScrapList);
                SetGridDefectList();
                if (dgCell.Rows.Count > 0)
                {
                    SetGridLotIdRt();
                }
            }

        }

        /// <summary>
        /// Spread 셋팅
        /// </summary>
        private void SetSpread()
        {
            if (NON_WIP_CHK == "Y") // 비재공 호출일 경우
            {
                dgCtnr.Columns["NON_WIP_ID"].Visibility = Visibility.Visible;
                dgCtnr.Columns["NON_WIP_QTY"].Visibility = Visibility.Visible;

                dgCtnr.Columns["CTNR_ID"].Visibility = Visibility.Collapsed;
                dgCtnr.Columns["WIPQTY"].Visibility = Visibility.Collapsed;
            }
            else
            {
                dgCtnr.Columns["NON_WIP_ID"].Visibility = Visibility.Collapsed;
                dgCtnr.Columns["NON_WIP_QTY"].Visibility = Visibility.Collapsed;

                dgCtnr.Columns["CTNR_ID"].Visibility = Visibility.Visible;
                dgCtnr.Columns["WIPQTY"].Visibility = Visibility.Visible;
            }
        }




        #endregion

        #region Event

        #region [Form Load]
        private void C1Window_Loaded(object sender, RoutedEventArgs e)
        {
            if (_load)
            {
                InitializeUserControls();
                SetControl();
                SetSpread();

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

                                dgCell.SelectedIndex = i;
                                dgCell.ScrollIntoView(i, dgCell.Columns["CHK"].Index);
                                DataTableConverter.SetValue(dgCell.Rows[i].DataItem, "CHK", 1);
                                txtCellID.Focus();
                                txtCellID.Text = string.Empty;
                                return;
                            }
                        }
                    }

                    if (NON_WIP_CHK == "Y") // 비재공 호출일 경우
                    {
                        InputCell_NON_WIP();
                    }
                    else
                    {
                        InputCell();
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

        #region 불량 Cell 삭제  : btnDelete_Click()
        // 불량 Cell 삭제
        private void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            if (!Validation_Delete())
            {
                return;
            }

            if (NON_WIP_CHK == "Y") // 비재공 호출일 경우
            {
                DeleteCell_NON_WIP();
            }
            else
            {
                DeleteCell();
            }

             
        }

        #endregion

        #region [닫기]
        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = MessageBoxResult.Cancel;
        }
        #endregion


        #endregion

        #region Mehod


        #region 비재공 호출

        #region 비재공ID  셋팅 : SetGridCartList_NON_WIP()
        /// <summary>
        ///  비재공 Cart List
        /// </summary>
        private void SetGridCartList_NON_WIP(DataTable dt)
        {
            try
            {
                int SumWipqty = 0;
                int SumSCAN_QTY = 0;
                int SumNON_SCAN_QTY = 0;

                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    SumWipqty = SumWipqty + Convert.ToInt32(dt.Rows[i]["NON_WIP_QTY"].ToString());
                    SumSCAN_QTY = SumSCAN_QTY + Convert.ToInt32(dt.Rows[i]["SCAN_QTY"].ToString());
                    SumNON_SCAN_QTY = SumNON_SCAN_QTY + Convert.ToInt32(dt.Rows[i]["NON_SCAN_QTY"].ToString());
                }
                DataTable dtBindTable = new DataTable();
                dtBindTable = new DataTable();
                dtBindTable.Columns.Add("NON_WIP_ID", typeof(string));
                dtBindTable.Columns.Add("PRJT_NAME", typeof(string));
                dtBindTable.Columns.Add("PRODID", typeof(string));
                dtBindTable.Columns.Add("MKT_TYPE_NAME", typeof(string));
                dtBindTable.Columns.Add("NON_WIP_QTY", typeof(string));
                dtBindTable.Columns.Add("SCAN_QTY", typeof(string));
                dtBindTable.Columns.Add("NON_SCAN_QTY", typeof(string));
                DataRow newRow = null;

                newRow = dtBindTable.NewRow();
                newRow["NON_WIP_ID"] = dt.Rows[0]["NON_WIP_ID"].ToString();
                newRow["PRJT_NAME"] = dt.Rows[0]["PRJT_NAME"].ToString();
                newRow["PRODID"] = dt.Rows[0]["PRODID"].ToString();
                newRow["MKT_TYPE_NAME"] = dt.Rows[0]["MKT_TYPE_NAME"].ToString();
                newRow["NON_WIP_QTY"] = SumWipqty;
                newRow["SCAN_QTY"] = SumSCAN_QTY;
                newRow["NON_SCAN_QTY"] = SumNON_SCAN_QTY;
                dtBindTable.Rows.Add(newRow);
                Util.GridSetData(dgCtnr, dt, FrameOperation);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
            finally
            {

            }
        }
        #endregion

        #region 팝업 로드 시 비재공 불량 Cell List 조회 : SetGridDefectList_NON_WIP
        // <summary>
        /// 팝업 로드시 비재공 불량 Cell List 
        /// </summary>
        private void SetGridDefectList_NON_WIP()
        {
            try
            {
                _inBoxHeaderType = CheckBoxHeaderType.Zero;

                DataTable dtRqst = new DataTable();
                dtRqst.Columns.Add("TRANSFER_ID", typeof(string));
                dtRqst.Columns.Add("TRANSFER_UNIT_CODE", typeof(string));
                dtRqst.Columns.Add("SCRAP_YN", typeof(string));

                DataRow dr = dtRqst.NewRow();
                dr["TRANSFER_ID"] = Util.NVC(DataTableConverter.GetValue(dgCtnr.Rows[0].DataItem, "NON_WIP_ID"));
                dr["TRANSFER_UNIT_CODE"] = "NWIP";
                dr["SCRAP_YN"] = "Y";
                dtRqst.Rows.Add(dr);
                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_POLYMER_DEFC_CELL", "INDATA", "OUTDATA", dtRqst);
                Util.GridSetData(dgCell, dtRslt, FrameOperation, false);

            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }

        #endregion

        #region 비재공 호출 Cell 입력 및 삭제시 Scan 수량, 미 Scan 수량 계산:SumScanQty_NON_WIP()
        //Scan 수량/ 미스캔수량 계산
        private void SumScanQty_NON_WIP()
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

                SumCtnrNonscanQty = Convert.ToInt32(Util.NVC(DataTableConverter.GetValue(dgCtnr.Rows[0].DataItem, "NON_WIP_QTY")).Replace(",", "")) - SumCtnrScanQty;

                DataTableConverter.SetValue(dgCtnr.Rows[0].DataItem, "SCAN_QTY", SumCtnrScanQty);
                DataTableConverter.SetValue(dgCtnr.Rows[0].DataItem, "NON_SCAN_QTY", SumCtnrNonscanQty);


            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }

        }

        #endregion

        #region 불량 Cell 등록 : InputCell_NON_WIP()
        //불량 Cell 등록
        private void InputCell_NON_WIP()
        {
            DataSet inData = new DataSet();

            //마스터 정보
            DataTable inDataTable = inData.Tables.Add("INDATA");
            inDataTable.Columns.Add("USERID", typeof(string));
            inDataTable.Columns.Add("NOTE", typeof(string));
            inDataTable.Columns.Add("TRANSFER_UNIT_CODE", typeof(string));
            inDataTable.Columns.Add("TRANSFER_ID", typeof(string));
            
            DataRow row = null;
            row = inDataTable.NewRow();
            row["USERID"] = LoginInfo.USERID;
            row["NOTE"] = null;
            row["TRANSFER_UNIT_CODE"] = "NWIP";
            row["TRANSFER_ID"] = Util.NVC(DataTableConverter.GetValue(dgCtnr.Rows[0].DataItem, "NON_WIP_ID"));
            inDataTable.Rows.Add(row);

            //INRESN
            DataTable inInresn = inData.Tables.Add("INSUBLOT");
            inInresn.Columns.Add("SUBLOTID", typeof(string));
            row = inInresn.NewRow();
            row["SUBLOTID"] = txtCellID.Text;
            inInresn.Rows.Add(row);
            try
            {
                string DefectCellID = txtCellID.Text;
                txtCellID.Focus();
                txtCellID.Text = string.Empty;


                //Cell 저장
                new ClientProxy().ExecuteService_Multi("BR_ACT_REG_SCRAP_WAIT_SUBLOT", "INDATA,INSUBLOT", null, (Result, ex) =>
                {
                    if (ex != null)
                    {
                        Util.MessageException(ex);
                        return;
                    }
                    //불량 Cell 재조회
                    SetDefcCell(DefectCellID);
                    if (dgCell.Rows.Count == 0)
                    {
                        Util.GridSetData(dgCell, DEFC_CELL, FrameOperation);
                    }
                    else
                    {
                        DataTable dtSource = DataTableConverter.Convert(dgCell.ItemsSource);

                        DataRow newRow = null;
                        newRow = dtSource.NewRow();
                        newRow["CHK"] = 0;
                        newRow["LOTID_RT"] = DEFC_CELL.Rows[0]["LOTID_RT"].ToString();
                        newRow["CELLID"] = DEFC_CELL.Rows[0]["CELLID"].ToString();
                        newRow["SUBLOTQTY"] = DEFC_CELL.Rows[0]["SUBLOTQTY"].ToString();
                        //dtSource.Rows.Add(newRow);
                        dtSource.Rows.InsertAt(newRow, 0);

                        for (int i = 0; i < dtSource.Rows.Count; i++)
                        {
                            dtSource.Rows[i]["CHK"] = 0;
                        }

                        Util.GridSetData(dgCell, dtSource, FrameOperation);
                        //txtCellID.Focus();
                        //txtCellID.Text = string.Empty;


                    }
                    SumScanQty_NON_WIP();

                }, inData);
            }
            catch (Exception ex)
            {
                Util.AlertByBiz("BR_ACT_REG_SCRAP_WAIT_SUBLOT", ex.Message, ex.ToString());

            }
        }
        #endregion

        #region 불량 Cell 삭제 : DeleteCell_NON_WIP()

        //불량Cell 삭제
        private void DeleteCell_NON_WIP()
        {
            DataSet inData = new DataSet();

            //마스터 정보
            DataTable inDataTable = inData.Tables.Add("INDATA");
            inDataTable.Columns.Add("USERID", typeof(string));
            inDataTable.Columns.Add("NOTE", typeof(string));
            inDataTable.Columns.Add("CTNR_ID", typeof(string));


            DataRow row = null;
            row = inDataTable.NewRow();
            row["USERID"] = LoginInfo.USERID;
            row["NOTE"] = null;
            row["CTNR_ID"] = Util.NVC(DataTableConverter.GetValue(dgCtnr.Rows[0].DataItem, "NON_WIP_ID"));
            inDataTable.Rows.Add(row);

            //INRESN
            DataTable inInresn = inData.Tables.Add("INSUBLOT");
            inInresn.Columns.Add("SUBLOTID", typeof(string));

            for (int i = 0; i < dgCell.Rows.Count; i++)
            {
                if (Util.NVC(DataTableConverter.GetValue(dgCell.Rows[i].DataItem, "CHK")) == "1")
                {
                    row = inInresn.NewRow();
                    row["SUBLOTID"] = Util.NVC(DataTableConverter.GetValue(dgCell.Rows[i].DataItem, "CELLID"));
                    inInresn.Rows.Add(row);
                }

            }



            try
            {
                //Cell 삭제
                new ClientProxy().ExecuteService_Multi("BR_ACT_REG_CANCEL_SCRAP_WAIT_SUBLOT", "INDATA,INSUBLOT", null, (Result, ex) =>
                {
                    if (ex != null)
                    {
                        Util.MessageException(ex);
                        return;
                    }
                    txtCellID.Focus();
                    txtCellID.Text = string.Empty;
                    SetGridDefectList_NON_WIP();
                    SumScanQty_NON_WIP();

                }, inData);
            }
            catch (Exception ex)
            {
                Util.AlertByBiz("BR_ACT_REG_SCRAP_WAIT_SUBLOT", ex.Message, ex.ToString());

            }
        }

        #endregion


        #endregion

        #region 폐기 및 Formation 불량 관리 호출

        #region 대차ID  셋팅 : SetGridCartList()
        /// <summary>
        /// Cart List 셋팅
        /// </summary>
        private void SetGridCartList(DataTable dt)
        {
            try
            {
                int SumWipqty = 0;
                int SumSCAN_QTY = 0;
                int SumNON_SCAN_QTY = 0;

                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    SumWipqty = SumWipqty + Convert.ToInt32(dt.Rows[i]["WIPQTY"].ToString());
                    SumSCAN_QTY = SumSCAN_QTY + Convert.ToInt32(dt.Rows[i]["SCAN_QTY"].ToString());
                    SumNON_SCAN_QTY = SumNON_SCAN_QTY + Convert.ToInt32(dt.Rows[i]["NON_SCAN_QTY"].ToString());
                }
                DataTable dtBindTable = new DataTable();
                dtBindTable = new DataTable();
                dtBindTable.Columns.Add("CTNR_ID", typeof(string));
                dtBindTable.Columns.Add("PRJT_NAME", typeof(string));
                dtBindTable.Columns.Add("PRODID", typeof(string));
                dtBindTable.Columns.Add("MKT_TYPE_NAME", typeof(string));
                dtBindTable.Columns.Add("WIPQTY", typeof(string));
                dtBindTable.Columns.Add("SCAN_QTY", typeof(string));
                dtBindTable.Columns.Add("NON_SCAN_QTY", typeof(string));
                DataRow newRow = null;

                newRow = dtBindTable.NewRow();
                newRow["CTNR_ID"] = dt.Rows[0]["CTNR_ID"].ToString();
                newRow["PRJT_NAME"] = dt.Rows[0]["PRJT_NAME"].ToString();
                newRow["PRODID"] = dt.Rows[0]["PRODID"].ToString();
                newRow["MKT_TYPE_NAME"] = dt.Rows[0]["MKT_TYPE_NAME"].ToString();
                newRow["WIPQTY"] = SumWipqty;
                newRow["SCAN_QTY"] = SumSCAN_QTY;
                newRow["NON_SCAN_QTY"] = SumNON_SCAN_QTY;
                dtBindTable.Rows.Add(newRow);
                Util.GridSetData(dgCtnr, dt, FrameOperation);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
            finally
            {

            }
        }
        #endregion

        #region 팝업 로드 시 Cell List 조회 :SetGridDefectList()
        // <summary>
        /// 팝업 로드시 불량 Cell List
        /// </summary>
        private void SetGridDefectList()
        {
            try
            {
                _inBoxHeaderType = CheckBoxHeaderType.Zero;

                DataTable dtRqst = new DataTable();
                dtRqst.Columns.Add("TRANSFER_ID", typeof(string));
                dtRqst.Columns.Add("TRANSFER_UNIT_CODE", typeof(string));
                dtRqst.Columns.Add("SCRAP_YN", typeof(string));

                DataRow dr = dtRqst.NewRow();
                dr["TRANSFER_ID"] = Util.NVC(DataTableConverter.GetValue(dgCtnr.Rows[0].DataItem, "CTNR_ID"));
                dr["TRANSFER_UNIT_CODE"] = "CTNR";
                dr["SCRAP_YN"] = "Y";
                dtRqst.Rows.Add(dr);
                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_POLYMER_DEFC_CELL", "INDATA", "OUTDATA", dtRqst);
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
        private void InputCell()
        {
            DataSet inData = new DataSet();

            //마스터 정보
            DataTable inDataTable = inData.Tables.Add("INDATA");

            inDataTable.Columns.Add("USERID", typeof(string));
            inDataTable.Columns.Add("NOTE", typeof(string));
            inDataTable.Columns.Add("TRANSFER_UNIT_CODE", typeof(string));
            inDataTable.Columns.Add("TRANSFER_ID", typeof(string));


            DataRow row = null;
            row = inDataTable.NewRow();
            row["USERID"] = LoginInfo.USERID;
            row["NOTE"] = null;
            row["TRANSFER_UNIT_CODE"] = "CTNR";
            row["TRANSFER_ID"] = Util.NVC(DataTableConverter.GetValue(dgCtnr.Rows[0].DataItem, "CTNR_ID"));
            inDataTable.Rows.Add(row);

            //INRESN
            DataTable inInresn = inData.Tables.Add("INSUBLOT");
            inInresn.Columns.Add("SUBLOTID", typeof(string));
            row = inInresn.NewRow();
            row["SUBLOTID"] = txtCellID.Text;
            inInresn.Rows.Add(row);
            try
            {
                string DefectCellID = txtCellID.Text;
                txtCellID.Focus();
                txtCellID.Text = string.Empty;
                //Cell 저장
                new ClientProxy().ExecuteService_Multi("BR_ACT_REG_SCRAP_WAIT_SUBLOT", "INDATA,INSUBLOT", null, (Result, ex) =>
                {
                    if (ex != null)
                    {
                        Util.MessageException(ex);
                        return;
                    }
                     //불량 Cell 재조회
                     SetDefcCell(DefectCellID);
                    

                    if (dgCell.Rows.Count == 0)
                    {
                        Util.GridSetData(dgCell, DEFC_CELL, FrameOperation);
                    }
                    else
                    {
                        DataTable dtSource = DataTableConverter.Convert(dgCell.ItemsSource);

                        DataRow newRow = null;
                        newRow = dtSource.NewRow();
                        newRow["CHK"] = 0;
                        newRow["LOTID_RT"] = DEFC_CELL.Rows[0]["LOTID_RT"].ToString();
                        newRow["CELLID"] = DEFC_CELL.Rows[0]["CELLID"].ToString();
                        newRow["SUBLOTQTY"] = DEFC_CELL.Rows[0]["SUBLOTQTY"].ToString();
                        //dtSource.Rows.Add(newRow);
                        dtSource.Rows.InsertAt(newRow, 0);

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
                Util.AlertByBiz("BR_ACT_REG_SCRAP_WAIT_SUBLOT", ex.Message, ex.ToString());

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
            DataSet inData = new DataSet();

            //마스터 정보
            DataTable inDataTable = inData.Tables.Add("INDATA");
            inDataTable.Columns.Add("USERID", typeof(string));
            inDataTable.Columns.Add("NOTE", typeof(string));
            //inDataTable.Columns.Add("CTNR_ID", typeof(string));


            DataRow row = null;
            row = inDataTable.NewRow();
            row["USERID"] = LoginInfo.USERID;
            row["NOTE"] = null;
            //row["CTNR_ID"] = Util.NVC(DataTableConverter.GetValue(dgCtnr.Rows[0].DataItem, "CTNR_ID"));
            inDataTable.Rows.Add(row);

            //INRESN
            DataTable inInresn = inData.Tables.Add("INSUBLOT");
            inInresn.Columns.Add("SUBLOTID", typeof(string));

            for (int i = 0; i < dgCell.Rows.Count; i++)
            {
                if (Util.NVC(DataTableConverter.GetValue(dgCell.Rows[i].DataItem, "CHK")) == "1")
                {
                    row = inInresn.NewRow();
                    row["SUBLOTID"] = Util.NVC(DataTableConverter.GetValue(dgCell.Rows[i].DataItem, "CELLID"));
                    inInresn.Rows.Add(row);
                }

            }



            try
            {
                //Cell 삭제
                new ClientProxy().ExecuteService_Multi("BR_ACT_REG_CANCEL_SCRAP_WAIT_SUBLOT", "INDATA,INSUBLOT", null, (Result, ex) =>
                {
                    if (ex != null)
                    {
                        Util.MessageException(ex);
                        return;
                    }
                    txtCellID.Focus();
                    txtCellID.Text = string.Empty;
                    SetGridDefectList();
                    SumScanQty();

                }, inData);
            }
            catch (Exception ex)
            {
                Util.AlertByBiz("BR_ACT_REG_SCRAP_WAIT_SUBLOT", ex.Message, ex.ToString());

            }
        }

        #endregion

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
                DEFC_CELL = new DataTable();
                DEFC_CELL = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_POLYMER_DEFC_CELL", "INDATA", "OUTDATA", dtRqst);
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


            //SetDefcCell();

            //if (DEFC_CELL.Rows.Count == 0)
            //{
            //    //존재하지 않는 Cell ID 입니다.
            //    LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("SFU4608"), null, "Information", MessageBoxButton.OK, MessageBoxIcon.None, (result) =>
            //    {
            //        if (result == MessageBoxResult.OK)
            //        {
            //            txtCellID.Focus();
            //            txtCellID.Text = string.Empty;
            //        }
            //    });

            //    return false;
            //}
            //데이터가 2건 있을 수도 있음
            //조회된 데이터에서 동일한 제품ID가 존재하는지 체크후 한건만 바인딩시킴
            //DEFC_CELL에서 PRODID가 틀린 데이터는 삭제시킴 ==> 결국 DEFC_CELL은 한건임 (2건이상이면 PRODID가 맞는것이 없는것임)
            //foreach (DataRow rowdel in DEFC_CELL.Rows)
            //{
            //    DEFC_CELL.Select("PRODID <> '" + Util.NVC(DataTableConverter.GetValue(dgCtnr.Rows[0].DataItem, "PRODID")) + "'").ToList<DataRow>().ForEach(row => row.Delete());
            //}
            //DEFC_CELL.AcceptChanges();

            //if (Util.NVC(DataTableConverter.GetValue(dgCtnr.Rows[0].DataItem, "PRODID")) != DEFC_CELL.Rows[0]["PRODID"].ToString())
            // {

            //     LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("SFU1893"), null, "Information", MessageBoxButton.OK, MessageBoxIcon.None, (result) =>
            //     {
            //         if (result == MessageBoxResult.OK)
            //         {
            //             txtCellID.Focus();
            //             txtCellID.Text = string.Empty;
            //         }
            //     });

            //     return false;
            // }

            //if (DEFC_CELL.Rows.Count == 0)
            //{

            //    LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("SFU1893"), null, "Information", MessageBoxButton.OK, MessageBoxIcon.None, (result) =>
            //    {
            //        if (result == MessageBoxResult.OK)
            //        {
            //            txtCellID.Focus();
            //            txtCellID.Text = string.Empty;
            //        }
            //    });

            //    return false;
            //}
            //if (NON_WIP_CHK == "Y") // 비재공 호출일 경우
            //{
                ////"다른 대차에 적재된 Cell 입니다.
                //if (DEFC_CELL.Rows[0]["WIP_PRCS_TYPE_CODE"].ToString() == "SCRAP" && Util.NVC(DataTableConverter.GetValue(dgCtnr.Rows[0].DataItem, "TRANSFER_ID")) != DEFC_CELL.Rows[0]["TRANSFER_ID"].ToString())
                //{
                //    LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("SFU4609"), null, "Information", MessageBoxButton.OK, MessageBoxIcon.None, (result) =>
                //    {
                //        if (result == MessageBoxResult.OK)
                //        {
                //            txtCellID.Focus();
                //            txtCellID.Text = string.Empty;
                //        }
                //    });

                //    return false;
                //}
            //}
            //else
            //{
                ////"다른 대차에 적재된 Cell 입니다.
                //if (DEFC_CELL.Rows[0]["WIP_PRCS_TYPE_CODE"].ToString() == "SCRAP" && Util.NVC(DataTableConverter.GetValue(dgCtnr.Rows[0].DataItem, "CTNR_ID")) != DEFC_CELL.Rows[0]["CTNR_ID"].ToString())
                //{
                //    LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("SFU4609"), null, "Information", MessageBoxButton.OK, MessageBoxIcon.None, (result) =>
                //    {
                //        if (result == MessageBoxResult.OK)
                //        {
                //            txtCellID.Focus();
                //            txtCellID.Text = string.Empty;
                //        }
                //    });

                //    return false;
                //}
            //}
            
            //if (DEFC_CELL.Rows[0]["SUBLOTSTAT"].ToString() == "SCRAPED")
            //{
            //    //폐기된 Cell 입니다
            //    LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("SFU4610"), null, "Information", MessageBoxButton.OK, MessageBoxIcon.None, (result) =>
            //    {
            //        if (result == MessageBoxResult.OK)
            //        {
            //            txtCellID.Focus();
            //            txtCellID.Text = string.Empty;
            //        }
            //    });

            //    return false;
            //}

            return true;
        }


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


        #endregion


        #endregion


        #endregion

        #region [Func]

        private enum CheckBoxHeaderType
        {
            Zero,
            One,
            Two,
            Three
        }



        #endregion





    }
}