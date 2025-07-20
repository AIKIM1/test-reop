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
namespace LGC.GMES.MES.CMM001.Popup
{
    /// <summary>
    /// CMM_SHIFT.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class CMM_POLYMER_CELL_INPUT : C1Window, IWorkArea
    {
        #region Declaration & Constructor


        string _CTNR_OR_DEF_LOT = string.Empty;
        Util _Util = new Util();
        BizDataSet _bizRule = new BizDataSet();
        DataTable DEFC_CELL = null;

        private CheckBoxHeaderType _inBoxHeaderType;


        private bool _load = true;
      
        public string CTNR_DEFC_LOT_CHK { get; set; }    //대차로 넘어올지 불량LOT으로 넘어올지 체크

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

        public CMM_POLYMER_CELL_INPUT()
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
            _CTNR_OR_DEF_LOT = parameters[0] as string;
            if (_CTNR_OR_DEF_LOT == string.Empty)
                return;

            if (CTNR_DEFC_LOT_CHK == "Y") ////대차
            {
                txtCtnr_ID.Text = _CTNR_OR_DEF_LOT;
                SetGridCartList(_CTNR_OR_DEF_LOT);
                SetGridDefectList();
                if (dgCell.Rows.Count > 0)
                {
                    SetGridLotIdRt();
                }
            }
            else //불량LOT
            {
                txtDefc_Lot.Text = _CTNR_OR_DEF_LOT;
                SetGridCartList_Defc_Lot(_CTNR_OR_DEF_LOT);
                SetGridDefectList_DEFC_LOT();
                if (dgCell.Rows.Count > 0)
                {
                    SetGridLotIdRt();
                }
              
            }

        }

        /// <summary>
        /// 텍스트 박스 수정
        /// </summary>
        private void SetSpread()
        {
            if (CTNR_DEFC_LOT_CHK == "Y") // 대차/불량LOT 여부
            {
                Ctnr.Visibility = Visibility.Visible;
                txtCtnr_ID.Visibility = Visibility.Visible;
                Defc_Lot.Visibility = Visibility.Collapsed;
                txtDefc_Lot.Visibility = Visibility.Collapsed;
            }
            else
            {
                Ctnr.Visibility = Visibility.Collapsed;
                txtCtnr_ID.Visibility = Visibility.Collapsed;
                Defc_Lot.Visibility = Visibility.Visible;
                txtDefc_Lot.Visibility = Visibility.Visible;
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

        #region Cell ID 바코드 영문 적용 : txtCellID_GotFocus()
        /// <summary>
        /// 바코드 영문 적용
        /// </summary>
        private void txtCellID_GotFocus(object sender, RoutedEventArgs e)
        {
            InputMethod.Current.ImeConversionMode = ImeConversionModeValues.Alphanumeric;
        }
        #endregion

        #region Cell 정보 조회 및 입력  : txtCellID_KeyDown()
        /// <summary>
        /// Cell 정보 조회 및 입력
        /// </summary>
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
                        //for (int i = 0; i < dgCell.GetRowCount(); i++)
                        //{
                        //    if (DataTableConverter.GetValue(dgCell.Rows[i].DataItem, "CELLID").ToString() == sLotid)
                        //    {

                        //        dgCell.SelectedIndex = i;
                        //        dgCell.ScrollIntoView(i, dgCell.Columns["CHK"].Index);
                        //        DataTableConverter.SetValue(dgCell.Rows[i].DataItem, "CHK", 1);
                        //        txtCellID.Focus();
                        //        txtCellID.Text = string.Empty;
                        //        return;
                        //    }
                        //}

                        DataTable dt = DataTableConverter.Convert(dgCell.ItemsSource);
                        DataRow[] drSelect = dt.Select("CELLID = '" + sLotid + "'");

                        if (drSelect.Length > 0)
                        {
                            int idx = dt.Rows.IndexOf(drSelect[0]);
                            dgCell.SelectedIndex = idx;
                            dgCell.ScrollIntoView(idx, dgCell.Columns["CHK"].Index);
                            DataTableConverter.SetValue(dgCell.Rows[idx].DataItem, "CHK", 1);
                            txtCellID.Focus();
                            txtCellID.Text = string.Empty;
                            return;
                        }

                    }

                    if (CTNR_DEFC_LOT_CHK == "Y") // 대차 ID
                    {
                        InputCell(txtCellID.Text);
                    }
                    else // 불량LOT
                    {
                        InputCell_DEFC_LOT(txtCellID.Text);
                    }

                    txtCellID.Focus();
                    txtCellID.Text = string.Empty;
                }
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
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

            if (CTNR_DEFC_LOT_CHK == "Y") // 대차 ID 호출
            {
                DeleteCell();
               
            }
            else
            {
                DeleteCell_DFEC_LOT();
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


        #region 불량 LOT  호출

        #region 불량 LOT   셋팅 : SetGridCartList_Defc_Lot()
        /// <summary>
        ///  불량 LOT List
        /// </summary>
        private void SetGridCartList_Defc_Lot(string Defc_Lot)
        {
            try
            {
                DataTable dtRqst = new DataTable();
                dtRqst.Columns.Add("LOTID", typeof(string));
                dtRqst.Columns.Add("LANGID", typeof(string));
                DataRow dr = dtRqst.NewRow();
                dr["LOTID"] = Defc_Lot;
                dr["LANGID"] = LoginInfo.LANGID;
                dtRqst.Rows.Add(dr);
                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_POLYMER_FORMATION_INPUT_CELL_DEFC_LOT", "INDATA", "OUTDATA", dtRqst);
                Util.GridSetData(dgCtnr, dtRslt, FrameOperation, false);
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }
        #endregion

        #region 팝업 로드 시 불량 LOT Cell List 조회 : SetGridDefectList_DEFC_LOT
        // <summary>
        /// 팝업 로드시 비재공 불량 Cell List 
        /// </summary>
        private void SetGridDefectList_DEFC_LOT()
        {
            try
            {
                _inBoxHeaderType = CheckBoxHeaderType.Zero;

                DataTable dtRqst = new DataTable();
                dtRqst.Columns.Add("TRANSFER_ID", typeof(string));
                dtRqst.Columns.Add("TRANSFER_UNIT_CODE", typeof(string));
                DataRow dr = dtRqst.NewRow();
                dr["TRANSFER_ID"] = _CTNR_OR_DEF_LOT;
                dr["TRANSFER_UNIT_CODE"] = "LOT";
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

        #region 불량LOT 호출 Cell 입력 및 삭제시 Scan 수량, 미 Scan 수량 계산:SumScanQty_DEFC_LOT()
        //Scan 수량/ 미스캔수량 계산
        private void SumScanQty_DEFC_LOT()
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

        #region 불량 Cell 등록 : InputCell_DEFC_LOT()
        //불량 Cell 등록
        private void InputCell_DEFC_LOT(string SubLotID)
        {

            string sBizName = string.Empty;
            if (Util.NVC(DataTableConverter.GetValue(dgCtnr.CurrentRow.DataItem, "WIP_QLTY_TYPE_CODE")) == "N") //불량이면
            {
                sBizName = "BR_ACT_REG_DEFECT_SUBLOT";
            }
            else
            {
                sBizName = "BR_ACT_REG_GOOD_SUBLOT";
            }

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
            row["TRANSFER_UNIT_CODE"] = "LOT";
            row["TRANSFER_ID"] = txtDefc_Lot.Text;
            inDataTable.Rows.Add(row);

            //INRESN
            DataTable inInresn = inData.Tables.Add("INSUBLOT");
            inInresn.Columns.Add("SUBLOTID", typeof(string));
            row = inInresn.NewRow();
            row["SUBLOTID"] = SubLotID;
            inInresn.Rows.Add(row);
            try
            {
                //Cell 저장
                new ClientProxy().ExecuteService_Multi(sBizName, "INDATA,INSUBLOT", null, (Result, ex) =>
                {
                    if (ex != null)
                    {
                        Util.MessageException(ex);
                        return;
                    }
                    //불량 Cell 재조회
                    SetDefcCell(SubLotID);
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
                        dtSource.Rows.Add(newRow);

                        //for (int i = 0; i < dtSource.Rows.Count; i++)
                        //{
                        //    dtSource.Rows[i]["CHK"] = 0;
                        //}

                        Util.GridSetData(dgCell, dtSource, FrameOperation);
                        //txtCellID.Focus();
                        //txtCellID.Text = string.Empty;


                    }
                    SumScanQty_DEFC_LOT();

                }, inData);
            }
            catch (Exception ex)
            {
                Util.AlertByBiz("BR_ACT_REG_SCRAP_WAIT_SUBLOT", ex.Message, ex.ToString());

            }
        }
        #endregion

        #region 불량 Cell 삭제 : DeleteCell_DFEC_LOT()

        //불량Cell 삭제
        private void DeleteCell_DFEC_LOT()
        {
            string sBizName = string.Empty;
            if (Util.NVC(DataTableConverter.GetValue(dgCtnr.CurrentRow.DataItem, "WIP_QLTY_TYPE_CODE")) == "N") //불량이면
            {
                sBizName = "BR_ACT_REG_CANCEL_DEFECT_SUBLOT";
            }
            else
            {
                sBizName = "BR_ACT_REG_CANCEL_GOOD_SUBLOT";
            }

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
            //row["CTNR_ID"] = _CTNR_OR_DEF_LOT;
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
                new ClientProxy().ExecuteService_Multi(sBizName, "INDATA,INSUBLOT", null, (Result, ex) =>
                {
                    if (ex != null)
                    {
                        Util.MessageException(ex);
                        return;
                    }
                    txtCellID.Focus();
                    txtCellID.Text = string.Empty;
                    SetGridDefectList_DEFC_LOT();
                    SumScanQty_DEFC_LOT();

                }, inData);
            }
            catch (Exception ex)
            {
                Util.AlertByBiz(sBizName, ex.Message, ex.ToString());

            }
        }

        #endregion


        #endregion

        #region 대차 ID 호출

        #region 대차ID  셋팅 : SetGridCartList()
        /// <summary>
        /// Cart List 셋팅
        /// </summary>
        private void SetGridCartList(string ctnr_id)
        {
            try
            {
                DataTable dtRqst = new DataTable();
                dtRqst.Columns.Add("CTNR_ID", typeof(string));
                dtRqst.Columns.Add("LANGID", typeof(string));
                DataRow dr = dtRqst.NewRow();
                dr["CTNR_ID"] = ctnr_id;
                dr["LANGID"] = LoginInfo.LANGID;
                dtRqst.Rows.Add(dr);
                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_POLYMER_FORMATION_INPUT_CELL_CTNR", "INDATA", "OUTDATA", dtRqst);
                Util.GridSetData(dgCtnr, dtRslt, FrameOperation, false);
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
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
                
                    
                DataTable dtRqst = new DataTable();
                dtRqst.Columns.Add("TRANSFER_ID", typeof(string));
                dtRqst.Columns.Add("TRANSFER_UNIT_CODE", typeof(string));
                DataRow dr = dtRqst.NewRow();
                dr["TRANSFER_ID"] = _CTNR_OR_DEF_LOT;
                dr["TRANSFER_UNIT_CODE"] = "CTNR";
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
        private void InputCell(string SubLotID)
        {
            string sBizName = string.Empty;

            if (Util.NVC(DataTableConverter.GetValue(dgCtnr.CurrentRow.DataItem, "WIP_QLTY_TYPE_CODE")) == "N") //불량이면
            {
                sBizName = "BR_ACT_REG_DEFECT_SUBLOT";
            }
            else
            {
                sBizName = "BR_ACT_REG_GOOD_SUBLOT";
            }
            

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
            row["TRANSFER_ID"] = txtCtnr_ID.Text;
            inDataTable.Rows.Add(row);

            //INRESN
            DataTable inInresn = inData.Tables.Add("INSUBLOT");
            inInresn.Columns.Add("SUBLOTID", typeof(string));
            row = inInresn.NewRow();
            row["SUBLOTID"] = SubLotID;
            inInresn.Rows.Add(row);

            try
            {
                //Cell 저장
                new ClientProxy().ExecuteService_Multi(sBizName, "INDATA,INSUBLOT", null, (Result, ex) =>
                {
                    if (ex != null)
                    {
                        Util.MessageException(ex);
                        return;
                    }

                     //불량 Cell 재조회
                     SetDefcCell(SubLotID);

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
                        dtSource.Rows.Add(newRow);

                        //for (int i = 0; i < dtSource.Rows.Count; i++)
                        //{
                        //    dtSource.Rows[i]["CHK"] = 0;
                        //}

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

            string sBizName = string.Empty;
            if (Util.NVC(DataTableConverter.GetValue(dgCtnr.CurrentRow.DataItem, "WIP_QLTY_TYPE_CODE")) == "N") //불량이면
            {
                sBizName = "BR_ACT_REG_CANCEL_DEFECT_SUBLOT";
            }
            else
            {
                sBizName = "BR_ACT_REG_CANCEL_GOOD_SUBLOT";
            }


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
            //row["CTNR_ID"] = _CTNR_OR_DEF_LOT;
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
                new ClientProxy().ExecuteService_Multi(sBizName, "INDATA,INSUBLOT", null, (Result, ex) =>
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
                Util.AlertByBiz(sBizName, ex.Message, ex.ToString());

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

        private void SetDefcCell(string SubLotID)
        {
            try
            {
                DataTable dtRqst = new DataTable();
                dtRqst.Columns.Add("SUBLOTID", typeof(string));
                DataRow dr = dtRqst.NewRow();
                dr["SUBLOTID"] = SubLotID;
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
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("SFU4607"), null, "Information", MessageBoxButton.OK, MessageBoxIcon.None, (result) =>
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