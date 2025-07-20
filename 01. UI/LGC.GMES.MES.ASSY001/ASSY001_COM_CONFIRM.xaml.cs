/*************************************************************************************
 Created Date : 2019.04.11
      Creator : INS 김동일K
   Decription : CWA3동 증설 - 조립 공정 공통 - 실적확정
--------------------------------------------------------------------------------------
 [Change History]
  2019.04.11  INS 김동일K : Initial Created.
  2020.05.12  김동일       : [C20200511-000024] 작업조, 작업자 등록 변경
  2024.01.25  남기운K      : (NERP 대응 프로젝트)실적처리 허용비율 초과 입력 기능추가
**************************************************************************************/

using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using C1.WPF;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.ControlsLibrary;
using System.Data;
using C1.WPF.DataGrid;
using System.Windows.Media;
using LGC.GMES.MES.CMM001;

namespace LGC.GMES.MES.ASSY001
{
    /// <summary>
    /// ASSY001_COM_CONFIRM.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class ASSY001_COM_CONFIRM : C1Window, IWorkArea
    {
        #region Declaration & Constructor 
        private string _ProcID = string.Empty;
        private string _LineID = string.Empty;
        private string _EqptID = string.Empty;
        private string _LotID = string.Empty;
        private string _WipSeq = string.Empty;
        private string _WipStat = string.Empty;

        private string _ShftName = string.Empty;
        private string _ShftID = string.Empty;
        private string _WorkerName = string.Empty;
        private string _WorkerID = string.Empty;
        private string _IRREGL_PROD_LOT_TYPE_CODE = string.Empty;
        private string _PROD_LOT_OPER_MODE = string.Empty;

        private bool _CanChgQty = false;
        private string _IsSTK = string.Empty;

        private bool bShowMsg = false;
        private bool _UNLDR_SHR_FLAG = false;
        private bool bSave = false;
        private System.DateTime dtNow;

        private string sCaldate = string.Empty;
        private System.DateTime dtCaldate;

        private string _LDR_LOT_IDENT_BAS_CODE = string.Empty;
        private string _UNLDR_LOT_IDENT_BAS_CODE = string.Empty;

        // 전체 적용 여부 CCB 결과 없음. 지급 요청 건으로 하드 코딩.
        private List<string> _SELECT_USER_MODE_AREA = new List<string>(new string[] { "A7" });   // 작업조,작업자 등록 화면 변경 요청 건 [C20200511-000024]

        private Util _Util = new Util();
        private BizDataSet _Biz = new BizDataSet();


        //허용비율 초과 사유
        DataTable _dtRet_Data = new DataTable();
        string _sUserID = string.Empty;
        string _sDepID = string.Empty;
        bool _bInputErpRate = false;


        #endregion

        #region Initialize 
        /// <summary>
        ///  Frame과 상호작용하기 위한 객체
        /// </summary>
        public IFrameOperation FrameOperation
        {
            get;
            set;
        }

        public ASSY001_COM_CONFIRM()
        {
            InitializeComponent();
        }

        private void InitializeGrid()
        {
            DataTable dtTmp = new DataTable();

            dtTmp.Columns.Add("LOTID", typeof(string));
            dtTmp.Columns.Add("INPUTQTY", typeof(int));
            dtTmp.Columns.Add("GOODQTY", typeof(int));
            dtTmp.Columns.Add("DTL_DFCT_DEDUCT", typeof(int));
            dtTmp.Columns.Add("AFTER_DEDUCT_GOODQTY", typeof(int));
            dtTmp.Columns.Add("DTL_DEFECT_LOT", typeof(int));
            dtTmp.Columns.Add("DTL_LOSS_LOT", typeof(int));
            dtTmp.Columns.Add("DTL_CHARGE_PROD_LOT", typeof(int));
            dtTmp.Columns.Add("DFCT_SUM", typeof(int));
            dtTmp.Columns.Add("PRE_SECTION_QTY", typeof(int));
            dtTmp.Columns.Add("AFTER_SECTION_QTY", typeof(int));
            dtTmp.Columns.Add("UNIDENTIFIED_QTY", typeof(int));
            dtTmp.Columns.Add("RE_INPUT_QTY", typeof(int));
            //dtTmp.Columns.Add("PICKUP_LOSS_QTY", typeof(int));

            DataRow dtRow = dtTmp.NewRow();
            dtRow["LOTID"] = _LotID;
            dtRow["INPUTQTY"] = 0;
            dtRow["GOODQTY"] = 0;
            dtRow["DTL_DFCT_DEDUCT"] = 0;
            dtRow["AFTER_DEDUCT_GOODQTY"] = 0;
            dtRow["DTL_DEFECT_LOT"] = 0;
            dtRow["DTL_LOSS_LOT"] = 0;
            dtRow["DTL_CHARGE_PROD_LOT"] = 0;
            dtRow["DFCT_SUM"] = 0;
            dtRow["PRE_SECTION_QTY"] = 0;
            dtRow["AFTER_SECTION_QTY"] = 0;
            dtRow["UNIDENTIFIED_QTY"] = 0;
            dtRow["RE_INPUT_QTY"] = 0;
            //dtRow["PICKUP_LOSS_QTY"] = 0;

            dtTmp.Rows.Add(dtRow);

            dgQty.ItemsSource = DataTableConverter.Convert(dtTmp);
        }

        private void InitializeControls()
        {
            dtNow = System.DateTime.Now;

            //if (_UNLDR_LOT_IDENT_BAS_CODE.Equals("CST_ID") || _UNLDR_LOT_IDENT_BAS_CODE.Equals("RF_ID"))
            //{
            //    lblCstID.Visibility = Visibility.Visible;
            //    txtCstID.Visibility = Visibility.Visible;
            //}
            //else
            //{
            //    lblCstID.Visibility = Visibility.Collapsed;
            //    txtCstID.Visibility = Visibility.Collapsed;
            //}

            sumRow.Visibility = Visibility.Collapsed;
            if (_ProcID.Equals(Process.LAMINATION))
            {
                dgQty.Columns["RE_INPUT_QTY"].Visibility = Visibility.Collapsed;
                dgQty.Columns["PRE_SECTION_QTY"].Visibility = Visibility.Collapsed;
                dgQty.Columns["AFTER_SECTION_QTY"].Visibility = Visibility.Collapsed;

                tabMagazine.Visibility = Visibility.Visible;
                tabBox.Visibility = Visibility.Collapsed;
                tabTray.Visibility = Visibility.Collapsed;
            }
            else if (_ProcID.Equals(Process.STACKING_FOLDING) || _ProcID.Equals(Process.CT_INSP))
            {
                dgQty.Columns["RE_INPUT_QTY"].Visibility = Visibility.Collapsed;
                dgQty.Columns["PRE_SECTION_QTY"].Visibility = Visibility.Collapsed;
                dgQty.Columns["AFTER_SECTION_QTY"].Visibility = Visibility.Collapsed;

                tabMagazine.Visibility = Visibility.Collapsed;
                tabBox.Visibility = Visibility.Visible;
                tabTray.Visibility = Visibility.Collapsed;
            }
            else if (_ProcID.Equals(Process.PACKAGING))
            {
                dgQty.Columns["RE_INPUT_QTY"].Visibility = Visibility.Visible;
                dgQty.Columns["PRE_SECTION_QTY"].Visibility = Visibility.Collapsed;
                dgQty.Columns["AFTER_SECTION_QTY"].Visibility = Visibility.Collapsed;

                tabMagazine.Visibility = Visibility.Collapsed;
                tabBox.Visibility = Visibility.Collapsed;
                tabTray.Visibility = Visibility.Visible;
            }
            else
            {
                dgQty.Columns["RE_INPUT_QTY"].Visibility = Visibility.Collapsed;
                dgQty.Columns["PRE_SECTION_QTY"].Visibility = Visibility.Collapsed;
                dgQty.Columns["AFTER_SECTION_QTY"].Visibility = Visibility.Collapsed;

                tabMagazine.Visibility = Visibility.Collapsed;
                tabBox.Visibility = Visibility.Collapsed;
                tabTray.Visibility = Visibility.Collapsed;
            }

            txtShift.Text = _ShftName;
            txtShift.Tag = _ShftID;
            txtWorker.Text = _WorkerName;
            txtWorker.Tag = _WorkerID;

            txtUserName.Text = string.Empty;
            txtUserName.Tag = string.Empty;
            txtReqNote.Text = string.Empty;
        }

        #endregion

        #region Event

        private void C1Window_Loaded(object sender, RoutedEventArgs e)
        {
            object[] tmps = C1WindowExtension.GetParameters(this);

            if (tmps != null && tmps.Length >= 13)
            {
                _ProcID = Util.NVC(tmps[0]);
                _LineID = Util.NVC(tmps[1]);
                _EqptID = Util.NVC(tmps[2]);
                _LotID = Util.NVC(tmps[3]);
                _WipSeq = Util.NVC(tmps[4]);

                _LDR_LOT_IDENT_BAS_CODE = Util.NVC(tmps[5]);
                _UNLDR_LOT_IDENT_BAS_CODE = Util.NVC(tmps[6]);

                _ShftName = Util.NVC(tmps[7]);
                _ShftID = Util.NVC(tmps[8]);
                _WorkerName = Util.NVC(tmps[9]);
                _WorkerID = Util.NVC(tmps[10]);

                _CanChgQty = (bool)tmps[11];
                _IsSTK = Util.NVC(tmps[12]);
            }

            ApplyPermissions();

            InitializeControls();

            GetUnldrSharFlag();

            GetAllData();
        }

        private void C1Window_Initialized(object sender, EventArgs e)
        {
            InitializeGrid();
        }

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            if (bShowMsg)
                return;

            if (bSave)
                this.DialogResult = MessageBoxResult.OK;
            else
                this.DialogResult = MessageBoxResult.Cancel;
        }

        /// <summary>
        /// /////////////////////////////////////
        /// </summary>
        /// <param name="sLot"></param>
        /// <param name="sProcID"></param>
        private void BR_PRD_REG_PERMIT_RATE_OVER_HIST()
        {
            try
            {
                DataTable lotTab = _dtRet_Data.DefaultView.ToTable(true, new string[] { "LOTID", "WIPSEQ" });
                string sLot = "";

                DataSet indataSet = new DataSet();
                DataTable inTable = indataSet.Tables.Add("INDATA");
                inTable.Columns.Add("SRCTYPE", typeof(string));
                inTable.Columns.Add("IFMODE", typeof(string));
                inTable.Columns.Add("EQPTID", typeof(string));
                inTable.Columns.Add("USERID", typeof(string));
                inTable.Columns.Add("LOTID", typeof(string));
                inTable.Columns.Add("WIPSEQ", typeof(string));

                DataTable inRESN = indataSet.Tables.Add("IN_RESN");
                inRESN.Columns.Add("PERMIT_RATE", typeof(decimal));
                inRESN.Columns.Add("ACTID", typeof(string));
                inRESN.Columns.Add("RESNCODE", typeof(string));
                inRESN.Columns.Add("RESNQTY", typeof(decimal));
                inRESN.Columns.Add("OVER_QTY", typeof(decimal));
                inRESN.Columns.Add("REQ_USERID", typeof(string));
                inRESN.Columns.Add("REQ_DEPTID", typeof(string));
                inRESN.Columns.Add("DIFF_RSN_CODE", typeof(string));
                inRESN.Columns.Add("NOTE", typeof(string));

                for (int j = 0; j < lotTab.Rows.Count; j++)
                {

                    inTable.Rows.Clear();
                    inRESN.Rows.Clear();
                    sLot = lotTab.Rows[j]["LOTID"].ToString();

                    DataRow newRow = inTable.NewRow();
                    newRow["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                    newRow["IFMODE"] = IFMODE.IFMODE_OFF;
                    newRow["EQPTID"] = _EqptID;
                    newRow["USERID"] = LoginInfo.USERID;
                    newRow["LOTID"] = sLot;
                    newRow["WIPSEQ"] = lotTab.Rows[j]["WIPSEQ"].ToString();
                    inTable.Rows.Add(newRow);
                    newRow = null;


                    for (int i = 0; i < _dtRet_Data.Rows.Count; i++)
                    {
                        if (sLot.Equals(_dtRet_Data.Rows[i]["LOTID"].ToString()))
                        {
                            newRow = inRESN.NewRow();
                            newRow["PERMIT_RATE"] = Convert.ToDecimal(_dtRet_Data.Rows[i]["PERMIT_RATE"].ToString());
                            newRow["ACTID"] = _dtRet_Data.Rows[i]["ACTID"].ToString();
                            newRow["RESNCODE"] = _dtRet_Data.Rows[i]["RESNCODE"].ToString();
                            newRow["RESNQTY"] = _dtRet_Data.Rows[i]["RESNQTY"].ToString();
                            newRow["OVER_QTY"] = _dtRet_Data.Rows[i]["OVER_QTY"].ToString();
                            newRow["REQ_USERID"] = _sUserID;
                            newRow["REQ_DEPTID"] = _sDepID;
                            newRow["DIFF_RSN_CODE"] = _dtRet_Data.Rows[i]["SPCL_RSNCODE"].ToString();
                            newRow["NOTE"] = _dtRet_Data.Rows[i]["RESNNOTE"].ToString();
                            inRESN.Rows.Add(newRow);
                        }
                    }

                    new ClientProxy().ExecuteService_Multi("BR_PRD_REG_PERMIT_RATE_OVER_HIST", "INDATA,IN_LOT", null, (bizResult, bizException) =>
                    {
                        try
                        {
                            if (bizException != null)
                            {
                                Util.MessageException(bizException);
                                return;
                            }
                        //Util.AlertInfo("정상 처리 되었습니다.");
                        //Util.MessageInfo("SFU1275");
                    }
                        catch (Exception ex)
                        {
                            Util.MessageException(ex);
                        }
                        finally
                        {

                        }
                    }, indataSet);
                }
            }
            catch (Exception ex)
            {

                Util.MessageException(ex);
            }
        }


        private void popRermitRate_Closed(object sender, EventArgs e)
        {
            
            CMM_PERMIT_RATE popRermitRate = sender as CMM_PERMIT_RATE;
            if (popRermitRate != null && popRermitRate.DialogResult == MessageBoxResult.OK)
            {
                _dtRet_Data.Clear();
                _dtRet_Data = popRermitRate.PERMIT_RATE.Copy();
                _sUserID = popRermitRate.UserID;
                _sDepID = popRermitRate.DeptID;
                _bInputErpRate = true;

                //////////////////////////////
                //확정 처리
                Save_Rate();

            }
            

        }

       

        private double GetLotINPUTQTY(string sLotID)
        {
            double dInputQty = 0;

            if (dgQty != null)
            {
                if (dgQty.GetRowCount() > 0)
                {
                    dInputQty = Util.NVC(DataTableConverter.GetValue(dgQty.Rows[0].DataItem, "INPUTQTY")).Equals("") ? 0 : double.Parse(Util.NVC(DataTableConverter.GetValue(dgQty.Rows[0].DataItem, "INPUTQTY")));

                    if (_ProcID.Equals(Process.STACKING_FOLDING))
                    {
                        if (dInputQty == 0)
                        {
                            double dGOODQTY = Util.NVC(DataTableConverter.GetValue(dgQty.Rows[0].DataItem, "GOODQTY")).Equals("") ? 0 : double.Parse(Util.NVC(DataTableConverter.GetValue(dgQty.Rows[0].DataItem, "GOODQTY")));
                            double dDFCT_SUM = Util.NVC(DataTableConverter.GetValue(dgQty.Rows[0].DataItem, "DFCT_SUM")).Equals("") ? 0 : double.Parse(Util.NVC(DataTableConverter.GetValue(dgQty.Rows[0].DataItem, "DFCT_SUM")));
                            dInputQty = dGOODQTY + dDFCT_SUM;
                        }
                    }
                }
            }
            return dInputQty;
        }

        private bool PERMIT_RATE_input(string sLotID, string sWipSeq)
        {
            bool bFlag = false;
            try
            {
                dgDefect.EndEdit();
                //양품 수량을 가지고 온다.
                //double goodQty = GetLotGoodQty(sLotID);
                ////////생산수량을 가지고 온다
                double goodQty = GetLotINPUTQTY(sLotID);
                if(goodQty <= 0)
                    return bFlag;

                DataTable data = new DataTable();
                data.Columns.Add("LOTID", typeof(string));
                data.Columns.Add("WIPSEQ", typeof(string));
                data.Columns.Add("ACTID", typeof(string));
                data.Columns.Add("ACTNAME", typeof(string));
                data.Columns.Add("RESNCODE", typeof(string));

                data.Columns.Add("RESNNAME", typeof(string));
                data.Columns.Add("DFCT_CODE_DETL_NAME", typeof(string));
                data.Columns.Add("RESNQTY", typeof(string));
                data.Columns.Add("PERMIT_RATE", typeof(string));
                data.Columns.Add("OVER_QTY", typeof(string));
                data.Columns.Add("SPCL_RSNCODE", typeof(string));
                data.Columns.Add("SPCL_RSNCODE_NAME", typeof(string));
                data.Columns.Add("RESNNOTE", typeof(string));

                
                decimal dRate = 0;
                decimal dQty = 0;
                decimal dAllowQty = 0;
                decimal OverQty = 0;
                for (int j = 0; j < dgDefect.Rows.Count - dgDefect.BottomRows.Count; j++)
                {
                    dRate = Util.NVC_Decimal(DataTableConverter.GetValue(dgDefect.Rows[j].DataItem, "PERMIT_RATE"));
                    //등록된 Rate가 0보다 큰것인 것만 적용
                    if (dRate > 0)
                    {
                        dQty = Util.NVC_Decimal(DataTableConverter.GetValue(dgDefect.Rows[j].DataItem, "RESNQTY")); //불량수량                        
                                                                                                                //dAllowQty = Math.Truncate(goodQty * dRate / 100); //버림
                        dAllowQty = Convert.ToDecimal(goodQty) * dRate / 100;


                        if (dAllowQty < dQty)
                        {
                            OverQty = dQty - dAllowQty;
                            OverQty = Math.Ceiling(OverQty); //소수점 첫자리 올림

                            DataRow newRow = data.NewRow();
                            newRow["LOTID"] = sLotID; //필수
                            newRow["WIPSEQ"] = sWipSeq; //필수
                            newRow["ACTID"] = Util.NVC(DataTableConverter.GetValue(dgDefect.Rows[j].DataItem, "ACTID")); //필수
                            newRow["ACTNAME"] = Util.NVC(DataTableConverter.GetValue(dgDefect.Rows[j].DataItem, "ACTNAME")); //필수
                            newRow["RESNCODE"] = Util.NVC(DataTableConverter.GetValue(dgDefect.Rows[j].DataItem, "RESNCODE")); //필수
                            newRow["RESNNAME"] = Util.NVC(DataTableConverter.GetValue(dgDefect.Rows[j].DataItem, "RESNNAME")); //필수
                            newRow["DFCT_CODE_DETL_NAME"] = "";

                            newRow["RESNQTY"] = dQty.ToString("G29"); //필수
                            newRow["PERMIT_RATE"] = dRate.ToString("0.00");  //필수 0제거 
                            newRow["OVER_QTY"] = OverQty.ToString("G29"); //필수 (dQty - dAllowQty).ToString("0.000"); //소수점 3자리

                            newRow["SPCL_RSNCODE"] = "";
                            newRow["SPCL_RSNCODE_NAME"] = "";
                            newRow["RESNNOTE"] = "";
                            data.Rows.Add(newRow);
                        }
                    }
                }
                
                //등록 할 정보가 있으면 
                if (data.Rows.Count > 0)
                {

                    CMM_PERMIT_RATE popRermitRate = new CMM_PERMIT_RATE { FrameOperation = FrameOperation };
                    object[] parameters = new object[2];
                    parameters[0] = sLotID;
                    parameters[1] = data;
                    C1WindowExtension.SetParameters(popRermitRate, parameters);

                    popRermitRate.Closed += popRermitRate_Closed;
                    Dispatcher.BeginInvoke(new Action(() => popRermitRate.ShowModal()));

                    bFlag = true;
                }
                
                return bFlag;
                ///////////////////////////////////////////////                  
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return bFlag;
            }

        }


        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            double dUnidentifiedQty = 0;
            
            if (!CanSave(out dUnidentifiedQty))
                return;

            if (!CheckCanConfirm())
                return;

            string messageCode = dUnidentifiedQty < 0 ? "SFU3746" : "SFU2039";
            Util.MessageConfirm(messageCode, (result) =>
            {
                if (result == MessageBoxResult.OK)
                {
                    Save();
                }
            });
        }

        private void dgQty_CommittedEdit(object sender, C1.WPF.DataGrid.DataGridCellEventArgs e)
        {
            try
            {
                if (sender == null) return;

                C1DataGrid grd = sender as C1DataGrid;

                grd.EndEdit();
                grd.EndEditRow(true);

                //CalcSumQty();

                double dGoodQty = Util.NVC(DataTableConverter.GetValue(grd.Rows[grd.TopRows.Count].DataItem, "GOODQTY")).Equals("") ? 0 : double.Parse(Util.NVC(DataTableConverter.GetValue(grd.Rows[grd.TopRows.Count].DataItem, "GOODQTY")));
                double dGoodQtyAfterDeduct = Util.NVC(DataTableConverter.GetValue(grd.Rows[grd.TopRows.Count].DataItem, "AFTER_DEDUCT_GOODQTY")).Equals("") ? 0 : double.Parse(Util.NVC(DataTableConverter.GetValue(grd.Rows[grd.TopRows.Count].DataItem, "AFTER_DEDUCT_GOODQTY")));
                double dDefectQty = Util.NVC(DataTableConverter.GetValue(grd.Rows[grd.TopRows.Count].DataItem, "DFCT_SUM")).Equals("") ? 0 : double.Parse(Util.NVC(DataTableConverter.GetValue(grd.Rows[grd.TopRows.Count].DataItem, "DFCT_SUM")));
                double dInputQty = Util.NVC(DataTableConverter.GetValue(grd.Rows[grd.TopRows.Count].DataItem, "INPUTQTY")).Equals("") ? 0 : double.Parse(Util.NVC(DataTableConverter.GetValue(grd.Rows[grd.TopRows.Count].DataItem, "INPUTQTY")));
                double dPreSectionQty = Util.NVC(DataTableConverter.GetValue(grd.Rows[grd.TopRows.Count].DataItem, "PRE_SECTION_QTY")).Equals("") ? 0 : double.Parse(Util.NVC(DataTableConverter.GetValue(grd.Rows[grd.TopRows.Count].DataItem, "PRE_SECTION_QTY")));
                double dAftSectionQty = Util.NVC(DataTableConverter.GetValue(grd.Rows[grd.TopRows.Count].DataItem, "AFTER_SECTION_QTY")).Equals("") ? 0 : double.Parse(Util.NVC(DataTableConverter.GetValue(grd.Rows[grd.TopRows.Count].DataItem, "AFTER_SECTION_QTY")));
                double dReInputQty = Util.NVC(DataTableConverter.GetValue(grd.Rows[grd.TopRows.Count].DataItem, "RE_INPUT_QTY")).Equals("") ? 0 : double.Parse(Util.NVC(DataTableConverter.GetValue(grd.Rows[grd.TopRows.Count].DataItem, "RE_INPUT_QTY")));

                double dUnidentifiedQty = dInputQty - (_UNLDR_SHR_FLAG ? dGoodQtyAfterDeduct : dGoodQty) - dDefectQty - (dPreSectionQty + dAftSectionQty);// + dReInputQty;

                DataTableConverter.SetValue(grd.Rows[grd.TopRows.Count].DataItem, "UNIDENTIFIED_QTY", dUnidentifiedQty);
                //// 미확인 LOSS 불량 코드 관리로 인한수정                
                //int idx = _Util.GetDataGridRowIndex(dgDefect, "PRCS_ITEM_CODE", "UNIDENTIFIED_QTY");
                //if (idx >= 0)
                //{
                //    DataTableConverter.SetValue(dgDefect.Rows[idx].DataItem, "RESNQTY", dUnidentifiedQty);
                //    dgDefect.UpdateLayout();
                //}

                grd.UpdateLayout();
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void dtpDateFrom_SelectedDataTimeChanged(object sender, SelectionChangedEventArgs e)
        {
            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (sender == null)
                {
                    this.Focus();
                    return;
                }

                LGCDatePicker dtPik = (sender as LGCDatePicker);

                if (sCaldate.Equals(""))
                {
                    this.Focus();
                    return;
                }

                if ((Convert.ToDecimal(dtCaldate.ToString("yyyyMMdd")) - 1 > Convert.ToDecimal(dtPik.SelectedDateTime.ToString("yyyyMMdd"))) ||
                    (Convert.ToDecimal(dtCaldate.ToString("yyyyMMdd")) + 1 < Convert.ToDecimal(dtPik.SelectedDateTime.ToString("yyyyMMdd"))))
                {
                    dtPik.Text = dtCaldate.ToLongDateString();
                    dtPik.SelectedDateTime = dtCaldate;

                    Util.MessageValidation("SFU1669");  // 선택할 수 없습니다.
                    //e.Handled = false;
                    return;
                }

                this.Focus();
            }));
        }

        private void dgQty_LoadedCellPresenter(object sender, C1.WPF.DataGrid.DataGridCellEventArgs e)
        {
            if (sender == null)
                return;

            C1.WPF.DataGrid.C1DataGrid dataGrid = sender as C1.WPF.DataGrid.C1DataGrid;

            dataGrid.Dispatcher.BeginInvoke(new Action(() =>
            {
                try
                {
                    if (e.Cell.Presenter == null)
                    {
                        return;
                    }

                    //Grid Data Binding 이용한 Background 색 변경
                    if (e.Cell.Row.Type == DataGridRowType.Item)
                    {
                        if (_CanChgQty && (e.Cell.Row.Index == 0) &&
                            (e.Cell.Column.Name.Equals("INPUTQTY") || 
                             e.Cell.Column.Name.Equals("PRE_SECTION_QTY") || 
                             e.Cell.Column.Name.Equals("AFTER_SECTION_QTY")))
                        {
                            e.Cell.Presenter.Background = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#FFFF8F"));
                            e.Cell.Presenter.Foreground = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#FF000000"));
                        }
                        else
                        {
                            e.Cell.Presenter.Background = null;
                            e.Cell.Presenter.Foreground = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#FF000000"));
                        }
                    }

                    if (e.Cell.Column.Name.Equals("INPUTQTY"))
                    {
                        if(e.Cell.Row.Index > 0)
                        {
                            e.Cell.Presenter.IsEnabled = false;
                        }
                        else
                        {
                            e.Cell.Presenter.IsEnabled = true;
                        }
                    }

                    //if (LoginInfo.LANGID.ToUpper().Equals("KO-KR"))
                    //{
                    //    if (e.Cell.Column.Name.Equals("INPUTQTY"))
                    //        ToolTipService.SetToolTip(e.Cell.Presenter, "설비보고수량");
                    //    else if (e.Cell.Column.Name.Equals("GOODQTY"))
                    //        ToolTipService.SetToolTip(e.Cell.Presenter, "생산반제품 수량 Sum");
                    //    //else if (e.Cell.Column.Name.Equals("DFCTQTY"))
                    //    //    ToolTipService.SetToolTip(e.Cell.Presenter, "불량수량 Sum");
                    //    else if (e.Cell.Column.Name.Equals("PRE_SECTION_QTY"))
                    //        ToolTipService.SetToolTip(e.Cell.Presenter, "설비보고수량");
                    //    else if (e.Cell.Column.Name.Equals("AFTER_SECTION_QTY"))
                    //        ToolTipService.SetToolTip(e.Cell.Presenter, "설비보고수량");
                    //    else if (e.Cell.Column.Name.Equals("UNIDENTIFIED_QTY"))
                    //        ToolTipService.SetToolTip(e.Cell.Presenter, "투입수 - 양품수 - 불량수 - 구간잔량");
                    //    else if (e.Cell.Column.Name.Equals("RE_INPUT_QTY"))
                    //        ToolTipService.SetToolTip(e.Cell.Presenter, "설비보고수량");
                    //    //else if (e.Cell.Column.Name.Equals("PICKUP_LOSS_QTY"))
                    //    //    ToolTipService.SetToolTip(e.Cell.Presenter, "설비보고수량");
                    //    else
                    //        ;
                    //}

                }
                catch (Exception ex)
                {
                    Util.MessageException(ex);
                }
            }));
        }

        private void dgQty_UnloadedCellPresenter(object sender, C1.WPF.DataGrid.DataGridCellEventArgs e)
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
                        e.Cell.Presenter.Foreground = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#FF000000"));
                    }
                }
            }));
        }

        private void dgQty_IsKeyboardFocusWithinChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (sender == null) return;

            C1DataGrid grd = sender as C1DataGrid;

            if ((bool)e.NewValue == false)
                grd.EndEdit();
        }

        private void btnDefectSave_Click(object sender, RoutedEventArgs e)
        {
            if (!CanSaveDefect())
                return;

            //불량정보를 저장 하시겠습니까?
            Util.MessageConfirm("SFU1587", result =>
            {
                if (result == MessageBoxResult.OK)
                {
                    SetDefect();
                }
            });
        }

        private void dgDefect_CommittedEdit(object sender, DataGridCellEventArgs e)
        {
            try
            {
                //#region 미확인 Loss 음수 여부 Validation
                //if (e == null || e.Cell == null)
                //    return;

                //if (sender == null) return;

                //C1DataGrid grd = sender as C1DataGrid;

                //int idx = _Util.GetDataGridRowIndex(grd, "PRCS_ITEM_CODE", "UNIDENTIFIED_QTY");
                //if (idx >= 0)
                //{
                //    // 미확인 Loss = 투입수(타발수) - 양품수 - 불량Sum(실적제외 및 미확인LOSS 항목 제외) - 구간잔량 + 재투입수
                //    DataTable dtSrc = DataTableConverter.Convert(dgDefect.ItemsSource);
                //    double dDefectQty = dtSrc.Compute("SUM(RESNQTY)", "RSLT_EXCL_FLAG = 'N' AND ISNULL(PRCS_ITEM_CODE, '') <> 'UNIDENTIFIED_QTY'").ToString().Equals("") ? 0 : double.Parse(dtSrc.Compute("SUM(RESNQTY)", "RSLT_EXCL_FLAG = 'N' AND ISNULL(PRCS_ITEM_CODE, '') <> 'UNIDENTIFIED_QTY'").ToString());

                //    double dInputQty = Util.NVC(DataTableConverter.GetValue(dgQty.Rows[dgQty.TopRows.Count].DataItem, "INPUTQTY")).Equals("") ? 0 : double.Parse(Util.NVC(DataTableConverter.GetValue(dgQty.Rows[dgQty.TopRows.Count].DataItem, "INPUTQTY")));
                //    double dGoodQty = Util.NVC(DataTableConverter.GetValue(dgQty.Rows[dgQty.TopRows.Count].DataItem, "GOODQTY")).Equals("") ? 0 : double.Parse(Util.NVC(DataTableConverter.GetValue(dgQty.Rows[dgQty.TopRows.Count].DataItem, "GOODQTY")));
                //    double dPreSecQty = Util.NVC(DataTableConverter.GetValue(dgQty.Rows[dgQty.TopRows.Count].DataItem, "PRE_SECTION_QTY")).Equals("") ? 0 : double.Parse(Util.NVC(DataTableConverter.GetValue(dgQty.Rows[dgQty.TopRows.Count].DataItem, "PRE_SECTION_QTY")));
                //    double dAftSecQty = Util.NVC(DataTableConverter.GetValue(dgQty.Rows[dgQty.TopRows.Count].DataItem, "AFTER_SECTION_QTY")).Equals("") ? 0 : double.Parse(Util.NVC(DataTableConverter.GetValue(dgQty.Rows[dgQty.TopRows.Count].DataItem, "AFTER_SECTION_QTY")));
                //    double dReInputQty = Util.NVC(DataTableConverter.GetValue(dgQty.Rows[dgQty.TopRows.Count].DataItem, "RE_INPUT_QTY")).Equals("") ? 0 : double.Parse(Util.NVC(DataTableConverter.GetValue(dgQty.Rows[dgQty.TopRows.Count].DataItem, "RE_INPUT_QTY")));

                //    double dUnidentifiedQty = dInputQty - dGoodQty - dDefectQty - (dPreSecQty + dAftSecQty);// + dReInputQty;

                //    if (dUnidentifiedQty < 0)
                //    {
                //        // 입력오류 : 입력한 불량으로 인해 미확인LOSS가 음수가 됩니다.
                //        Util.MessageValidation("SFU6035", (action) => { bShowMsg = false; });

                //        grd.EndEdit();
                //        grd.EndEditRow(true);

                //        DataTableConverter.SetValue(grd.Rows[e.Cell.Row.Index].DataItem, "RESNQTY", 0);

                //        grd.UpdateLayout();

                //        bShowMsg = true;
                //    }
                //}
                //#endregion

                SumDefectQty();
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void dgDefect_BeginningEdit(object sender, C1.WPF.DataGrid.DataGridBeginningEditEventArgs e)
        {
            try
            {
                if (e == null || e.Row == null || e.Row.DataItem == null || e.Column == null)
                    return;

                if (e.Column.Name.Equals("RESNQTY"))
                {
                    string sFlag = Util.NVC(DataTableConverter.GetValue(e.Row.DataItem, "DFCT_QTY_CHG_BLOCK_FLAG"));
                    if (sFlag == "Y")
                    {
                        e.Cancel = true;
                    }
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void dgDefect_LoadedCellPresenter(object sender, DataGridCellEventArgs e)
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

                if (e.Cell.Row.Type == DataGridRowType.Item)
                {
                    if (!Util.NVC(e.Cell.Column.Name).Equals("ACTNAME"))
                    {
                        //if (Util.NVC(DataTableConverter.GetValue(dgDefect.Rows[e.Cell.Row.Index].DataItem, "RSLT_EXCL_FLAG")).Equals("Y"))
                        //{
                        //    e.Cell.Presenter.Background = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#F8DAC0"));
                        //    e.Cell.Presenter.Foreground = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#FF000000"));
                        //}
                        //else
                        //{
                            string sFlag = Util.NVC(DataTableConverter.GetValue(dgDefect.Rows[e.Cell.Row.Index].DataItem, "DFCT_QTY_CHG_BLOCK_FLAG"));
                            if (sFlag == "Y")
                            {
                                e.Cell.Presenter.Background = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#D4D4D4"));
                                e.Cell.Presenter.Foreground = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#FF000000"));
                            }
                            else
                            {
                                e.Cell.Presenter.Background = null;// new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#E6F5FB"));
                                e.Cell.Presenter.Foreground = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#FF000000"));
                            }
                        //}
                    }
                }
            }));
        }

        private void dgDefect_UnloadedCellPresenter(object sender, DataGridCellEventArgs e)
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
                        //e.Row.Presenter.Foreground = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#FF000000"));
                    }
                }
            }));
        }
        
        private void dgDefect_IsKeyboardFocusWithinChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (sender == null) return;

            C1DataGrid grd = sender as C1DataGrid;

            if ((bool)e.NewValue == false)
                grd.EndEdit();
        }

        private void dgQty_BeganEdit(object sender, DataGridBeganEditEventArgs e)
        {
            if (dgDefect?.Rows?.Count > 0)
            {
                dgDefect.EndEdit(true);

                int idx = _Util.GetDataGridRowIndex(dgDefect, "PRCS_ITEM_CODE", "UNIDENTIFIED_QTY");
                if (idx >= 0 && dgDefect.CurrentCell.Row.Index == idx)
                {
                    int iTmpRow = dgDefect.GetRowCount() - 2 > idx ? idx + 1 : idx - 1;

                    C1.WPF.DataGrid.DataGridCell dgcTmp = dgDefect.GetCell(iTmpRow, dgDefect.Columns.Count - 1);

                    if (dgcTmp != null)
                        dgDefect.CurrentCell = dgcTmp;
                }
            }
        }

        private void dgDefect_BeganEdit(object sender, DataGridBeganEditEventArgs e)
        {
            if (dgQty?.Rows?.Count > 0)
                dgQty.EndEdit(true);
        }

        private void btnWorker_Click(object sender, RoutedEventArgs e)
        {
            CMM001.Popup.CMM_SHIFT_USER2 wndPopup = new CMM001.Popup.CMM_SHIFT_USER2();
            wndPopup.FrameOperation = this.FrameOperation;

            if (wndPopup != null)
            {
                object[] Parameters = new object[8];
                Parameters[0] = LoginInfo.CFG_SHOP_ID;
                Parameters[1] = LoginInfo.CFG_AREA_ID;
                Parameters[2] = Util.NVC(_LineID);
                Parameters[3] = _ProcID;
                Parameters[4] = Util.NVC(txtShift.Tag);
                Parameters[5] = Util.NVC(txtWorker.Tag);
                Parameters[6] = Util.NVC(_EqptID);
                Parameters[7] = "Y"; // 저장 플로그 "Y" 일때만 저장.

                C1WindowExtension.SetParameters(wndPopup, Parameters);

                wndPopup.Closed += new EventHandler(wndShiftUser_Closed);

                this.Dispatcher.BeginInvoke(new Action(() => wndPopup.ShowModal()));                
            }
        }

        private void txtUserName_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == System.Windows.Input.Key.Enter)
            {
                popupUser();
            }
        }

        private void btnReqUser_Click(object sender, RoutedEventArgs e)
        {
            popupUser();
        }
        #endregion

        #region Mehod

        #region [BizCall]
        private void GetLotInfo()
        {
            try
            {
                ShowLoadingIndicator();

                DataTable inTable = _Biz.GetDA_PRD_SEL_WIP_INFO();

                DataRow newRow = inTable.NewRow();
                newRow["LOTID"] = _LotID;
                newRow["LANGID"] = LoginInfo.LANGID;

                inTable.Rows.Add(newRow);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_CONFIRM_LOT_INFO_L", "INDATA", "OUTDATA", inTable);

                if (dtRslt != null && dtRslt.Rows.Count > 0)
                {
                    if (dgQty.GetRowCount() > 0)
                    {
                        if (dgQty.Columns.Contains("INPUTQTY") && dtRslt.Columns.Contains("INPUTQTY"))
                            DataTableConverter.SetValue(dgQty.Rows[dgQty.TopRows.Count].DataItem, "INPUTQTY", dtRslt.Rows[0]["INPUTQTY"]);
                        //if (dgQty.Columns.Contains("GOODQTY") && searchResult.Columns.Contains("GOODQTY"))
                        //    DataTableConverter.SetValue(dgQty.Rows[dgQty.TopRows.Count].DataItem, "GOODQTY", searchResult.Rows[0]["GOODQTY"]);
                        //if (dgQty.Columns.Contains("DFCTQTY") && searchResult.Columns.Contains("DFCTQTY"))
                        //    DataTableConverter.SetValue(dgQty.Rows[dgQty.TopRows.Count].DataItem, "DFCTQTY", searchResult.Rows[0]["DFCTQTY"]); 
                        if (dgQty.Columns.Contains("PRE_SECTION_QTY") && dtRslt.Columns.Contains("PRE_SECTION_QTY"))
                            DataTableConverter.SetValue(dgQty.Rows[dgQty.TopRows.Count].DataItem, "PRE_SECTION_QTY", dtRslt.Rows[0]["PRE_SECTION_QTY"]);
                        if (dgQty.Columns.Contains("AFTER_SECTION_QTY") && dtRslt.Columns.Contains("AFTER_SECTION_QTY"))
                            DataTableConverter.SetValue(dgQty.Rows[dgQty.TopRows.Count].DataItem, "AFTER_SECTION_QTY", dtRslt.Rows[0]["AFTER_SECTION_QTY"]);
                        if (dgQty.Columns.Contains("UNIDENTIFIED_QTY") && dtRslt.Columns.Contains("UNIDENTIFIED_QTY"))
                            DataTableConverter.SetValue(dgQty.Rows[dgQty.TopRows.Count].DataItem, "UNIDENTIFIED_QTY", dtRslt.Rows[0]["UNIDENTIFIED_QTY"]);
                        if (dgQty.Columns.Contains("RE_INPUT_QTY") && dtRslt.Columns.Contains("RE_INPUT_QTY"))
                            DataTableConverter.SetValue(dgQty.Rows[dgQty.TopRows.Count].DataItem, "RE_INPUT_QTY", dtRslt.Rows[0]["RE_INPUT_QTY"]);
                        //if (dgQty.Columns.Contains("PICKUP_LOSS_QTY") && dtRslt.Columns.Contains("PICKUP_LOSS_QTY"))
                        //    DataTableConverter.SetValue(dgQty.Rows[dgQty.TopRows.Count].DataItem, "PICKUP_LOSS_QTY", dtRslt.Rows[0]["PICKUP_LOSS_QTY"]);
                    }

                    txtLotId.Text = Util.NVC(dtRslt.Rows[0]["LOTID"]);
                    txtProdId.Text = Util.NVC(dtRslt.Rows[0]["PRODID"]);
                    //txtWorkOrder.Text = Util.NVC(searchResult.Rows[0]["WOID"]);
                    txtStartTime.Text = Util.NVC(dtRslt.Rows[0]["WIPDTTM_ST"]);
                    txtEndTime.Text = Util.NVC(dtRslt.Rows[0]["EQPT_END_DTTM"]);
                    txtRemark.Text = Util.NVC(dtRslt.Rows[0]["WIP_NOTE"]);

                    txtAnLotType.Text = Util.NVC(dtRslt.Rows[0]["IRREGL_PROD_LOT_TYPE_NAME"]);
                    txtLotOperMode.Text = Util.NVC(dtRslt.Rows[0]["PROD_LOT_OPER_MODE_NAME"]);

                    _IRREGL_PROD_LOT_TYPE_CODE = Util.NVC(dtRslt.Rows[0]["IRREGL_PROD_LOT_TYPE_CODE"]);
                    //cboAnLotType.SelectedValue = Util.NVC(dtRslt.Rows[0]["IRREGL_PROD_LOT_TYPE_CODE"]);
                    _PROD_LOT_OPER_MODE = Util.NVC(dtRslt.Rows[0]["PROD_LOT_OPER_MODE"]);
                    
                    // Caldate Lot의 Caldate로...
                    if (Util.NVC(dtRslt.Rows[0]["CALDATE_LOT"]).Trim().Equals(""))
                    {
                        //dtpCaldate.Text = Convert.ToDateTime(Util.NVC(dtRslt.Rows[0]["NOW_CALDATE"])).ToLongDateString();
                        //dtpCaldate.SelectedDateTime = Convert.ToDateTime(Util.NVC(dtRslt.Rows[0]["NOW_CALDATE"]));

                        sCaldate = Util.NVC(dtRslt.Rows[0]["NOW_CALDATE_YMD"]);
                        dtCaldate = Convert.ToDateTime(Util.NVC(dtRslt.Rows[0]["NOW_CALDATE"]));
                    }
                    else
                    {
                        //dtpCaldate.Text = Convert.ToDateTime(Util.NVC(dtRslt.Rows[0]["CALDATE_LOT"])).ToLongDateString();
                        //dtpCaldate.SelectedDateTime = Convert.ToDateTime(Util.NVC(dtRslt.Rows[0]["CALDATE_LOT"]));

                        sCaldate = Convert.ToDateTime(Util.NVC(dtRslt.Rows[0]["CALDATE_LOT"])).ToString("yyyyMMdd");
                        dtCaldate = Convert.ToDateTime(Util.NVC(dtRslt.Rows[0]["CALDATE_LOT"]));
                    }
                }
            }
            catch (Exception ex)
            {
                HideLoadingIndicator();
                Util.MessageException(ex);
            }
            finally
            {
                HideLoadingIndicator();
            }
        }

        private DataTable GetLotInfo(string lotID)
        {
            DataTable dtRslt = null;
            try
            {
                ShowLoadingIndicator();

                DataTable inTable = _Biz.GetDA_PRD_SEL_WIP_INFO();

                DataRow newRow = inTable.NewRow();
                newRow["LOTID"] = lotID;
                newRow["LANGID"] = LoginInfo.LANGID;

                inTable.Rows.Add(newRow);

                dtRslt = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_CONFIRM_LOT_INFO_L", "INDATA", "OUTDATA", inTable);
            }
            catch (Exception ex)
            {
                HideLoadingIndicator();
                Util.MessageException(ex);
            }
            finally
            {
                HideLoadingIndicator();
            }
            return dtRslt;
        }

        private void Save()
        {
            try
            {

                ////////////////////////////////////////////////////////////////////////////////
                //Rate 관련 팝업
                //완료 처리 하기 전에 팝업 표시
                _bInputErpRate = false;
                _dtRet_Data.Clear();
                _sUserID = string.Empty;
                _sDepID = string.Empty;
                if (PERMIT_RATE_input(_LotID, _WipSeq))
                {
                    return;
                }
                ///////////////////////////////////////////////////////////////////////////////

                ShowLoadingIndicator();
                string bizName = string.Empty;
                string sInDataName = string.Empty;

                if (_ProcID.Equals(Process.LAMINATION))
                {
                    bizName = "BR_PRD_REG_END_LOT_LM_L";
                    sInDataName = "INDATA,IN_INPUT,IN_OUTPUT,IN_PROC_WRKR";
                }
                else if (_ProcID.Equals(Process.STACKING_FOLDING))
                {
                    bizName = "BR_PRD_REG_END_LOT_FD_L";
                    sInDataName = "INDATA,IN_INPUT,IN_OUTPUT,IN_PROC_WRKR";
                }
                else if (_ProcID.Equals(Process.CT_INSP))
                {
                    bizName = "BR_PRD_REG_END_LOT_CI_L";
                    sInDataName = "INDATA,IN_INPUT,IN_OUTPUT,IN_PROC_WRKR";
                }
                else if (_ProcID.Equals(Process.PACKAGING))
                {
                    bizName = "BR_PRD_REG_END_LOT_CL_L";
                    sInDataName = "INDATA,IN_OUTPUT,IN_PROC_WRKR";
                }


                DataSet indataSet = new DataSet();

                DataTable inDataTable = indataSet.Tables.Add("INDATA");
                inDataTable.Columns.Add("SRCTYPE", typeof(string));
                inDataTable.Columns.Add("IFMODE", typeof(string));
                inDataTable.Columns.Add("EQPTID", typeof(string));
                inDataTable.Columns.Add("PROD_LOTID", typeof(string));
                inDataTable.Columns.Add("USERID", typeof(string));

                if (!_ProcID.Equals(Process.PACKAGING))
                {
                    inDataTable = indataSet.Tables.Add("IN_INPUT");
                    inDataTable.Columns.Add("EQPT_MOUNT_PSTN_ID", typeof(string));
                    inDataTable.Columns.Add("EQPT_MOUNT_PSTN_STATE", typeof(string));
                    inDataTable.Columns.Add("INPUT_LOTID", typeof(string));
                }
               
                inDataTable = indataSet.Tables.Add("IN_OUTPUT");
                inDataTable.Columns.Add("INPUT_QTY", typeof(decimal));
                inDataTable.Columns.Add("DFCT_QTY", typeof(decimal));
                inDataTable.Columns.Add("OUTPUT_QTY", typeof(decimal));
                inDataTable.Columns.Add("BTN_QTY_PRE", typeof(decimal));
                inDataTable.Columns.Add("BTN_QTY_AFT", typeof(decimal));
                inDataTable.Columns.Add("REINPUT_QTY", typeof(decimal));
                inDataTable.Columns.Add("WIPNOTE", typeof(string));
                inDataTable.Columns.Add("CRRT_NOTE", typeof(string));
                inDataTable.Columns.Add("REQ_USERID", typeof(string));

                inDataTable = indataSet.Tables.Add("IN_PROC_WRKR");
                inDataTable.Columns.Add("SHIFT", typeof(string));
                inDataTable.Columns.Add("WIPDTTM_ED", typeof(DateTime));
                inDataTable.Columns.Add("WRK_USERID", typeof(string));
                inDataTable.Columns.Add("WRK_USER_NAME", typeof(string));
                
                DataTable inTable = indataSet.Tables["INDATA"];
                DataRow newRow = inTable.NewRow();
                newRow["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                newRow["IFMODE"] = IFMODE.IFMODE_OFF;
                newRow["EQPTID"] = _EqptID;
                newRow["PROD_LOTID"] = txtLotId.Text;                
                newRow["USERID"] = LoginInfo.USERID;

                inTable.Rows.Add(newRow);
                newRow = null;

                //DataTable inInputTable = indataSet.Tables["IN_INPUT"];

                //newRow = inInputTable.NewRow();
                //newRow["EQPT_MOUNT_PSTN_ID"] = "";
                //newRow["EQPT_MOUNT_PSTN_STATE"] = "A";
                //newRow["INPUT_LOTID"] = "";

                //inInputTable.Rows.Add(newRow);

                newRow = null;

                if (dgQty.GetRowCount() > 0)
                {                    
                    DataTable inOutputTable = indataSet.Tables["IN_OUTPUT"];

                    newRow = inOutputTable.NewRow();
                    newRow["INPUT_QTY"] = Util.NVC(DataTableConverter.GetValue(dgQty.Rows[dgQty.TopRows.Count].DataItem, "INPUTQTY")).Equals("") ? 0 : decimal.Parse(Util.NVC(DataTableConverter.GetValue(dgQty.Rows[dgQty.TopRows.Count].DataItem, "INPUTQTY")));
                    newRow["DFCT_QTY"] = Util.NVC(DataTableConverter.GetValue(dgQty.Rows[dgQty.TopRows.Count].DataItem, "DFCT_SUM")).Equals("") ? 0 : decimal.Parse(Util.NVC(DataTableConverter.GetValue(dgQty.Rows[dgQty.TopRows.Count].DataItem, "DFCT_SUM")));
                    newRow["OUTPUT_QTY"] = 0;// Util.NVC(DataTableConverter.GetValue(dgQty.Rows[dgQty.TopRows.Count].DataItem, "GOODQTY")).Equals("") ? 0 : decimal.Parse(Util.NVC(DataTableConverter.GetValue(dgQty.Rows[dgQty.TopRows.Count].DataItem, "GOODQTY")));

                    if (_ProcID.Equals(Process.PACKAGING))
                    {
                        newRow["REINPUT_QTY"] = Util.NVC(DataTableConverter.GetValue(dgQty.Rows[dgQty.TopRows.Count].DataItem, "RE_INPUT_QTY")).Equals("") ? 0 : decimal.Parse(Util.NVC(DataTableConverter.GetValue(dgQty.Rows[dgQty.TopRows.Count].DataItem, "RE_INPUT_QTY")));
                    }
                    else
                    {
                        newRow["BTN_QTY_PRE"] = Util.NVC(DataTableConverter.GetValue(dgQty.Rows[dgQty.TopRows.Count].DataItem, "PRE_SECTION_QTY")).Equals("") ? 0 : decimal.Parse(Util.NVC(DataTableConverter.GetValue(dgQty.Rows[dgQty.TopRows.Count].DataItem, "PRE_SECTION_QTY")));
                        newRow["BTN_QTY_AFT"] = Util.NVC(DataTableConverter.GetValue(dgQty.Rows[dgQty.TopRows.Count].DataItem, "AFTER_SECTION_QTY")).Equals("") ? 0 : decimal.Parse(Util.NVC(DataTableConverter.GetValue(dgQty.Rows[dgQty.TopRows.Count].DataItem, "AFTER_SECTION_QTY")));
                    }

                    newRow["WIPNOTE"] = txtRemark.Text;
                    newRow["CRRT_NOTE"] = txtReqNote.Text;
                    newRow["REQ_USERID"] = txtUserName.Tag;

                    inOutputTable.Rows.Add(newRow);

                    newRow = null;
                }

                DataTable inWrkTable = indataSet.Tables["IN_PROC_WRKR"];

                newRow = inWrkTable.NewRow();
                newRow["SHIFT"] = txtShift.Tag;
                //newRow["WIPDTTM_ED"] = GetSystemTime();
                newRow["WRK_USERID"] = txtWorker.Tag;
                newRow["WRK_USER_NAME"] = txtWorker.Text;
                
                inWrkTable.Rows.Add(newRow);

                new ClientProxy().ExecuteService_Multi(bizName, sInDataName, null, (bizResult, bizException) =>
                {
                    try
                    {
                        if (bizException != null)
                        {
                            Util.MessageException(bizException);
                            return;
                        }

                        btnSave.IsEnabled = false;

                        Util.MessageInfo("SFU1275");

                        bSave = true;

                        this.DialogResult = MessageBoxResult.OK;
                    }
                    catch (Exception ex)
                    {
                        Util.MessageException(ex);
                    }
                    finally
                    {
                        HideLoadingIndicator();
                    }
                }, indataSet);
            }
            catch (Exception ex)
            {
                HideLoadingIndicator();
                Util.MessageException(ex);
            }
        }


        private void Save_Rate()
        {
            try
            {
                ShowLoadingIndicator();

                string bizName = string.Empty;
                string sInDataName = string.Empty;

               
                if (_ProcID.Equals(Process.LAMINATION))
                {
                    bizName = "BR_PRD_REG_END_LOT_LM_L";
                    sInDataName = "INDATA,IN_INPUT,IN_OUTPUT,IN_PROC_WRKR";
                }
                else if (_ProcID.Equals(Process.STACKING_FOLDING))
                {
                    bizName = "BR_PRD_REG_END_LOT_FD_L";
                    sInDataName = "INDATA,IN_INPUT,IN_OUTPUT,IN_PROC_WRKR";
                }
                else if (_ProcID.Equals(Process.CT_INSP))
                {
                    bizName = "BR_PRD_REG_END_LOT_CI_L";
                    sInDataName = "INDATA,IN_INPUT,IN_OUTPUT,IN_PROC_WRKR";
                }
                else if (_ProcID.Equals(Process.PACKAGING))
                {
                    bizName = "BR_PRD_REG_END_LOT_CL_L";
                    sInDataName = "INDATA,IN_OUTPUT,IN_PROC_WRKR";
                }


                DataSet indataSet = new DataSet();

                DataTable inDataTable = indataSet.Tables.Add("INDATA");
                inDataTable.Columns.Add("SRCTYPE", typeof(string));
                inDataTable.Columns.Add("IFMODE", typeof(string));
                inDataTable.Columns.Add("EQPTID", typeof(string));
                inDataTable.Columns.Add("PROD_LOTID", typeof(string));
                inDataTable.Columns.Add("USERID", typeof(string));

                if (!_ProcID.Equals(Process.PACKAGING))
                {
                    inDataTable = indataSet.Tables.Add("IN_INPUT");
                    inDataTable.Columns.Add("EQPT_MOUNT_PSTN_ID", typeof(string));
                    inDataTable.Columns.Add("EQPT_MOUNT_PSTN_STATE", typeof(string));
                    inDataTable.Columns.Add("INPUT_LOTID", typeof(string));
                }

                inDataTable = indataSet.Tables.Add("IN_OUTPUT");
                inDataTable.Columns.Add("INPUT_QTY", typeof(decimal));
                inDataTable.Columns.Add("DFCT_QTY", typeof(decimal));
                inDataTable.Columns.Add("OUTPUT_QTY", typeof(decimal));
                inDataTable.Columns.Add("BTN_QTY_PRE", typeof(decimal));
                inDataTable.Columns.Add("BTN_QTY_AFT", typeof(decimal));
                inDataTable.Columns.Add("REINPUT_QTY", typeof(decimal));
                inDataTable.Columns.Add("WIPNOTE", typeof(string));
                inDataTable.Columns.Add("CRRT_NOTE", typeof(string));
                inDataTable.Columns.Add("REQ_USERID", typeof(string));

                inDataTable = indataSet.Tables.Add("IN_PROC_WRKR");
                inDataTable.Columns.Add("SHIFT", typeof(string));
                inDataTable.Columns.Add("WIPDTTM_ED", typeof(DateTime));
                inDataTable.Columns.Add("WRK_USERID", typeof(string));
                inDataTable.Columns.Add("WRK_USER_NAME", typeof(string));

                DataTable inTable = indataSet.Tables["INDATA"];
                DataRow newRow = inTable.NewRow();
                newRow["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                newRow["IFMODE"] = IFMODE.IFMODE_OFF;
                newRow["EQPTID"] = _EqptID;
                newRow["PROD_LOTID"] = txtLotId.Text;
                newRow["USERID"] = LoginInfo.USERID;

                inTable.Rows.Add(newRow);
                newRow = null;
                newRow = null;

                if (dgQty.GetRowCount() > 0)
                {
                    DataTable inOutputTable = indataSet.Tables["IN_OUTPUT"];

                    newRow = inOutputTable.NewRow();
                    newRow["INPUT_QTY"] = Util.NVC(DataTableConverter.GetValue(dgQty.Rows[dgQty.TopRows.Count].DataItem, "INPUTQTY")).Equals("") ? 0 : decimal.Parse(Util.NVC(DataTableConverter.GetValue(dgQty.Rows[dgQty.TopRows.Count].DataItem, "INPUTQTY")));
                    newRow["DFCT_QTY"] = Util.NVC(DataTableConverter.GetValue(dgQty.Rows[dgQty.TopRows.Count].DataItem, "DFCT_SUM")).Equals("") ? 0 : decimal.Parse(Util.NVC(DataTableConverter.GetValue(dgQty.Rows[dgQty.TopRows.Count].DataItem, "DFCT_SUM")));
                    newRow["OUTPUT_QTY"] = 0;// Util.NVC(DataTableConverter.GetValue(dgQty.Rows[dgQty.TopRows.Count].DataItem, "GOODQTY")).Equals("") ? 0 : decimal.Parse(Util.NVC(DataTableConverter.GetValue(dgQty.Rows[dgQty.TopRows.Count].DataItem, "GOODQTY")));

                    if (_ProcID.Equals(Process.PACKAGING))
                    {
                        newRow["REINPUT_QTY"] = Util.NVC(DataTableConverter.GetValue(dgQty.Rows[dgQty.TopRows.Count].DataItem, "RE_INPUT_QTY")).Equals("") ? 0 : decimal.Parse(Util.NVC(DataTableConverter.GetValue(dgQty.Rows[dgQty.TopRows.Count].DataItem, "RE_INPUT_QTY")));
                    }
                    else
                    {
                        newRow["BTN_QTY_PRE"] = Util.NVC(DataTableConverter.GetValue(dgQty.Rows[dgQty.TopRows.Count].DataItem, "PRE_SECTION_QTY")).Equals("") ? 0 : decimal.Parse(Util.NVC(DataTableConverter.GetValue(dgQty.Rows[dgQty.TopRows.Count].DataItem, "PRE_SECTION_QTY")));
                        newRow["BTN_QTY_AFT"] = Util.NVC(DataTableConverter.GetValue(dgQty.Rows[dgQty.TopRows.Count].DataItem, "AFTER_SECTION_QTY")).Equals("") ? 0 : decimal.Parse(Util.NVC(DataTableConverter.GetValue(dgQty.Rows[dgQty.TopRows.Count].DataItem, "AFTER_SECTION_QTY")));
                    }

                    newRow["WIPNOTE"] = txtRemark.Text;
                    newRow["CRRT_NOTE"] = txtReqNote.Text;
                    newRow["REQ_USERID"] = txtUserName.Tag;

                    inOutputTable.Rows.Add(newRow);

                    newRow = null;
                }

                DataTable inWrkTable = indataSet.Tables["IN_PROC_WRKR"];

                newRow = inWrkTable.NewRow();
                newRow["SHIFT"] = txtShift.Tag;
                //newRow["WIPDTTM_ED"] = GetSystemTime();
                newRow["WRK_USERID"] = txtWorker.Tag;
                newRow["WRK_USER_NAME"] = txtWorker.Text;

                inWrkTable.Rows.Add(newRow);

                new ClientProxy().ExecuteService_Multi(bizName, sInDataName, null, (bizResult, bizException) =>
                {
                    try
                    {
                        if (bizException != null)
                        {
                            Util.MessageException(bizException);
                            return;
                        }

                        btnSave.IsEnabled = false;


                        // ERP 불량 비율 Rate 저장
                        if (_bInputErpRate && _dtRet_Data.Rows.Count > 0)
                        {
                            BR_PRD_REG_PERMIT_RATE_OVER_HIST();
                        }

                        Util.MessageInfo("SFU1275");

                        bSave = true;

                        this.DialogResult = MessageBoxResult.OK;
                    }
                    catch (Exception ex)
                    {
                        Util.MessageException(ex);
                    }
                 
                }, indataSet);

          

            }
            catch (Exception ex)
            {
                HideLoadingIndicator();
                Util.MessageException(ex);
            }
            finally
            {
                HideLoadingIndicator();
            }
        }

        private void SetDefect(bool bMsgShow = true)
        {
            try
            {
                ShowLoadingIndicator();

                dgDefect.EndEdit();

                DataSet indataSet = _Biz.GetBR_PRD_REG_DEFECT_ALL();
                DataTable inTable = indataSet.Tables["INDATA"];

                DataRow newRow = inTable.NewRow();
                newRow["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                newRow["IFMODE"] = IFMODE.IFMODE_OFF;
                newRow["EQPTID"] = _EqptID;
                newRow["USERID"] = LoginInfo.USERID;

                inTable.Rows.Add(newRow);

                DataTable inDEFECT_LOT = indataSet.Tables["INRESN"];

                for (int i = 0; i < dgDefect.Rows.Count - dgDefect.BottomRows.Count; i++)
                {
                    newRow = null;

                    newRow = inDEFECT_LOT.NewRow();
                    newRow["LOTID"] = txtLotId.Text.Trim();
                    newRow["WIPSEQ"] = _WipSeq;
                    newRow["ACTID"] = Util.NVC(DataTableConverter.GetValue(dgDefect.Rows[i].DataItem, "ACTID"));
                    newRow["RESNCODE"] = Util.NVC(DataTableConverter.GetValue(dgDefect.Rows[i].DataItem, "RESNCODE"));
                    newRow["RESNQTY"] = Util.NVC(DataTableConverter.GetValue(dgDefect.Rows[i].DataItem, "RESNQTY")).Equals("") ? 0 : double.Parse(Util.NVC(DataTableConverter.GetValue(dgDefect.Rows[i].DataItem, "RESNQTY")));
                    newRow["RESNCODE_CAUSE"] = "";
                    newRow["PROCID_CAUSE"] = "";
                    newRow["RESNNOTE"] = "";
                    newRow["DFCT_TAG_QTY"] = 0;
                    newRow["LANE_QTY"] = 1;
                    newRow["LANE_PTN_QTY"] = 1;

                    if (Util.NVC(DataTableConverter.GetValue(dgDefect.Rows[i].DataItem, "ACTID")).Equals("CHARGE_PROD_LOT"))
                    {
                        newRow["COST_CNTR_ID"] = Util.NVC(DataTableConverter.GetValue(dgDefect.Rows[i].DataItem, "COST_CNTR_ID"));
                    }
                    else
                    {
                        newRow["COST_CNTR_ID"] = "";
                    }

                    newRow["A_TYPE_DFCT_QTY"] = 0;
                    newRow["C_TYPE_DFCT_QTY"] = 0;

                    inDEFECT_LOT.Rows.Add(newRow);
                }

                new ClientProxy().ExecuteServiceSync_Multi("BR_QCA_REG_WIPREASONCOLLECT_ALL", "INDATA,INRESN", null, indataSet);

                if (bMsgShow)
                    Util.MessageInfo("SFU1275");    //정상 처리 되었습니다.

                GetDefectInfo();
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
            finally
            {
                HideLoadingIndicator();
            }
        }

        private void GetDefectInfo()
        {
            try
            {
                ShowLoadingIndicator();

                DataTable inTable = new DataTable();
                inTable.Columns.Add("LANGID", typeof(string));
                inTable.Columns.Add("AREAID", typeof(string));
                inTable.Columns.Add("PROCID", typeof(string));
                inTable.Columns.Add("EQPTID", typeof(string));
                inTable.Columns.Add("LOTID", typeof(string));
                inTable.Columns.Add("WIPSEQ", typeof(string));
                inTable.Columns.Add("ACTID", typeof(string));


                DataRow newRow = inTable.NewRow();
                newRow["LANGID"] = LoginInfo.LANGID;
                newRow["AREAID"] = LoginInfo.CFG_AREA_ID;
                newRow["PROCID"] = _ProcID;
                newRow["EQPTID"] = _EqptID;
                newRow["LOTID"] = _LotID;
                newRow["WIPSEQ"] = _WipSeq;
                newRow["ACTID"] = "DEFECT_LOT,LOSS_LOT,CHARGE_PROD_LOT";

                inTable.Rows.Add(newRow);

                DataTable searchResult = new ClientProxy().ExecuteServiceSync("DA_QCA_SEL_WIPRESONCOLLECT_L", "INDATA", "OUTDATA", inTable);

                Util.GridSetData(dgDefect, searchResult, FrameOperation, false);

                SumDefectQty();
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
            finally
            {
                HideLoadingIndicator();
            }
        }

        private DataTable GetDefectInfo(string lotID, string eqptID, string wipseq)
        {
            DataTable searchResult = null;
            try
            {
                ShowLoadingIndicator();

                DataTable inTable = new DataTable();
                inTable.Columns.Add("LANGID", typeof(string));
                inTable.Columns.Add("AREAID", typeof(string));
                inTable.Columns.Add("PROCID", typeof(string));
                inTable.Columns.Add("EQPTID", typeof(string));
                inTable.Columns.Add("LOTID", typeof(string));
                inTable.Columns.Add("WIPSEQ", typeof(string));
                inTable.Columns.Add("ACTID", typeof(string));


                DataRow newRow = inTable.NewRow();
                newRow["LANGID"] = LoginInfo.LANGID;
                newRow["AREAID"] = LoginInfo.CFG_AREA_ID;
                newRow["PROCID"] = _ProcID;
                newRow["EQPTID"] = eqptID;
                newRow["LOTID"] = lotID;
                newRow["WIPSEQ"] = wipseq;
                newRow["ACTID"] = "DEFECT_LOT,LOSS_LOT,CHARGE_PROD_LOT";

                inTable.Rows.Add(newRow);

                searchResult = new ClientProxy().ExecuteServiceSync("DA_QCA_SEL_WIPRESONCOLLECT_L", "INDATA", "OUTDATA", inTable);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
            finally
            {
                HideLoadingIndicator();
            }
            return searchResult;
        }

        private void GetMagzineList()
        {
            try
            {
                ShowLoadingIndicator();

                DataTable inTable = new DataTable();
                inTable.Columns.Add("LANGID", typeof(string));
                inTable.Columns.Add("PR_LOTID", typeof(string));

                DataRow newRow = inTable.NewRow();
                newRow["LANGID"] = LoginInfo.LANGID;
                newRow["PR_LOTID"] = _LotID;

                inTable.Rows.Add(newRow);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_OUT_LOT_LIST_LM", "INDATA", "OUTDATA", inTable);

                Util.GridSetData(dgMagazine, dtRslt, FrameOperation, true);

                SumGoodQty();
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
            finally
            {
                HideLoadingIndicator();
            }
        }

        private void GetBoxList()
        {
            try
            {
                ShowLoadingIndicator();

                DataTable inTable = new DataTable();
                inTable.Columns.Add("LANGID", typeof(string));
                inTable.Columns.Add("PR_LOTID", typeof(string));

                DataRow newRow = inTable.NewRow();
                newRow["LANGID"] = LoginInfo.LANGID;
                newRow["PR_LOTID"] = _LotID;

                inTable.Rows.Add(newRow);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_OUT_LOT_LIST_FD", "INDATA", "OUTDATA", inTable);

                Util.GridSetData(dgBox, dtRslt, FrameOperation, true);

                SumGoodQty();
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
            finally
            {
                HideLoadingIndicator();
            }
        }

        private DataTable GetBoxList(string lotID)
        {
            DataTable dtRslt = null;
            try
            {
                ShowLoadingIndicator();

                DataTable inTable = new DataTable();
                inTable.Columns.Add("LANGID", typeof(string));
                inTable.Columns.Add("PR_LOTID", typeof(string));

                DataRow newRow = inTable.NewRow();
                newRow["LANGID"] = LoginInfo.LANGID;
                newRow["PR_LOTID"] = lotID;

                inTable.Rows.Add(newRow);

                dtRslt = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_OUT_LOT_LIST_FD", "INDATA", "OUTDATA", inTable);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
            finally
            {
                HideLoadingIndicator();
            }
            return dtRslt;

        }

        private void GetTrayList()
        {
            try
            {
                ShowLoadingIndicator();

                DataTable inTable = new DataTable();
                inTable.Columns.Add("LANGID", typeof(string));
                inTable.Columns.Add("AREAID", typeof(string));
                inTable.Columns.Add("PR_LOTID", typeof(string));
                inTable.Columns.Add("PROCID", typeof(string));
                inTable.Columns.Add("EQSGID", typeof(string));
                inTable.Columns.Add("EQPTID", typeof(string));
                inTable.Columns.Add("TRAYID", typeof(string));

                DataRow newRow = inTable.NewRow();
                newRow["LANGID"] = LoginInfo.LANGID;
                newRow["AREAID"] = LoginInfo.CFG_AREA_ID;
                newRow["PR_LOTID"] = _LotID;
                newRow["PROCID"] = Process.PACKAGING;
                newRow["EQSGID"] = _LineID;
                newRow["EQPTID"] = _EqptID;
                newRow["TRAYID"] = null;    // 전체 리스트 조회 처리.

                inTable.Rows.Add(newRow);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_OUT_LOT_LIST_CL", "INDATA", "OUTDATA", inTable);

                Util.GridSetData(dgTray, dtRslt, FrameOperation, true);

                SumGoodQty();
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
            finally
            {
                HideLoadingIndicator();
            }
        }

        private void GetUnldrSharFlag()
        {
            try
            {
                _UNLDR_SHR_FLAG = false;

                if (dgQty.Columns.Contains("DTL_DFCT_DEDUCT"))
                    dgQty.Columns["DTL_DFCT_DEDUCT"].Visibility = Visibility.Collapsed;
                if (dgQty.Columns.Contains("AFTER_DEDUCT_GOODQTY"))
                    dgQty.Columns["AFTER_DEDUCT_GOODQTY"].Visibility = Visibility.Collapsed;
                
                ShowLoadingIndicator();

                DataTable inTable = new DataTable();
                inTable.Columns.Add("EQPTID", typeof(string));

                DataRow newRow = inTable.NewRow();
                newRow["EQPTID"] = _EqptID;

                inTable.Rows.Add(newRow);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_EQUIPMENTATTR", "INDATA", "OUTDATA", inTable);

                if (dtRslt?.Rows?.Count > 0)
                {
                    if (Util.NVC(dtRslt.Rows[0]["UNLDR_SHR_FLAG"]).Equals("Y"))
                    {
                        _UNLDR_SHR_FLAG = true;

                        if (dgQty.Columns.Contains("DTL_DFCT_DEDUCT"))
                        {
                            dgQty.Columns["DTL_DFCT_DEDUCT"].Visibility = Visibility.Visible;
                            dgQty.Columns["DTL_DFCT_DEDUCT"].Width = new C1.WPF.DataGrid.DataGridLength(110);
                        }
                        if (dgQty.Columns.Contains("AFTER_DEDUCT_GOODQTY"))
                        {
                            dgQty.Columns["AFTER_DEDUCT_GOODQTY"].Visibility = Visibility.Visible;
                            dgQty.Columns["AFTER_DEDUCT_GOODQTY"].Width = new C1.WPF.DataGrid.DataGridLength(130);
                        }
                    }
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
        }

        private bool CheckCanConfirm()
        {
            try
            {
                bool bRet = false;
                
                DataTable inDataTable = new DataTable();
                inDataTable.Columns.Add("SRCTYPE", typeof(string));
                inDataTable.Columns.Add("IFMODE", typeof(string));
                inDataTable.Columns.Add("EQPTID", typeof(string));
                inDataTable.Columns.Add("LOTID", typeof(string));
                inDataTable.Columns.Add("USERID", typeof(string));

                DataRow newRow = inDataTable.NewRow();
                newRow["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                newRow["IFMODE"] = IFMODE.IFMODE_OFF;
                newRow["EQPTID"] = _EqptID;
                newRow["LOTID"] = _LotID;
                newRow["USERID"] = LoginInfo.USERID;

                inDataTable.Rows.Add(newRow);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("BR_PRD_CHK_AUTO_CONFIRM_LOT", "INDATA", "OUTDATA", inDataTable);

                if (dtRslt?.Rows?.Count > 0)
                {
                    if (Util.NVC(dtRslt.Rows[0]["RESULT_VALUE"]).Equals("OK"))
                    {
                        bRet = true;
                    }
                    else
                    {
                        Util.MessageValidation(Util.NVC(dtRslt.Rows[0]["RESULT_CODE"]));
                    }
                }

                return bRet;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);

                return false;
            }
        }
        #endregion

        #region [Validation]
        private bool CanSave(out double dUnidentifiedQty)
        {
            dUnidentifiedQty = 0;

            bool bRet = false;

            if (bShowMsg)
                return bRet;

            //if (dgDefect.ItemsSource != null)
            //{
            //    foreach (DataRowView row in DataGridHandler.GetModifiedItems(dgDefect))
            //    {
            //        //Util.Alert("저장하지 않은 불량 정보가 있습니다.");
            //        Util.MessageValidation("SFU1878");
            //        return bRet;
            //    }
            //}

            foreach (C1.WPF.DataGrid.DataGridRow row in dgDefect.Rows)
            {
                double dRsn, dOrgRsn = 0;

                double.TryParse(Util.NVC(DataTableConverter.GetValue(row.DataItem, "RESNQTY")), out dRsn);
                double.TryParse(Util.NVC(DataTableConverter.GetValue(row.DataItem, "ORG_RESNQTY")), out dOrgRsn);

                if (dRsn != dOrgRsn)
                {
                    // 저장하지 않은 불량 정보가 있습니다.
                    Util.MessageValidation("SFU1878", (action) =>
                    {
                        if (tbDefect.Visibility == Visibility.Visible)
                            tbDefect.IsSelected = true;
                    });
                    return bRet;
                }
            }

            // 언로더 공용 설비 차감후 양품수량 음수 체크.
            if (_UNLDR_SHR_FLAG)
            {
                double dGoodQtyAfterDeduct = Util.NVC(DataTableConverter.GetValue(dgQty.Rows[dgQty.TopRows.Count].DataItem, "AFTER_DEDUCT_GOODQTY")).Equals("") ? 0 : double.Parse(Util.NVC(DataTableConverter.GetValue(dgQty.Rows[dgQty.TopRows.Count].DataItem, "AFTER_DEDUCT_GOODQTY")));

                if (dGoodQtyAfterDeduct < 0)
                {
                    // 입력오류 : 차감 후 양품수량이 음수로 처리할 수 없습니다.
                    Util.MessageValidation("SFU6054");
                    return bRet;
                }
            }

            dUnidentifiedQty = Util.NVC(DataTableConverter.GetValue(dgQty.Rows[dgQty.TopRows.Count].DataItem, "UNIDENTIFIED_QTY")).Equals("") ? 0 : double.Parse(Util.NVC(DataTableConverter.GetValue(dgQty.Rows[dgQty.TopRows.Count].DataItem, "UNIDENTIFIED_QTY")));

            //if (dUnidentifiedQty < 0)
            //{
            //    // 입력오류 : 미확인LOSS가 음수로 처리할 수 없습니다.
            //    Util.MessageValidation("SFU6041");
            //    return bRet;
            //}

            #region 작업조,작업자 등록 화면 변경 요청 건 [C20200511-000024]
            if (_SELECT_USER_MODE_AREA.Contains(LoginInfo.CFG_AREA_ID))
            {
                if (txtWorker.Text.Trim().Equals(""))
                {
                    Util.MessageValidation("SFU3752"); // 입력오류 : 입력된 작업자 정보가 없습니다. 월력정보를 등록 하거나 작업자를 선택 하세요.
                    return bRet;
                }

                if (!string.IsNullOrEmpty(txtShiftEndTime.Text))
                {
                    DateTime shiftStartDateTime = Convert.ToDateTime(txtShiftStartTime.Text);
                    DateTime shiftEndDateTime = Convert.ToDateTime(txtShiftEndTime.Text);
                    DateTime systemDateTime = GetSystemTime();

                    int prevCheck = DateTime.Compare(systemDateTime, shiftStartDateTime);
                    int nextCheck = DateTime.Compare(systemDateTime, shiftEndDateTime);

                    if (prevCheck < 0 || nextCheck > 0)
                    {
                        Util.MessageValidation("SFU3752"); // 입력오류 : 입력된 작업자 정보가 없습니다. 월력정보를 등록 하거나 작업자를 선택 하세요.
                        txtWorker.Text = string.Empty;
                        txtWorker.Tag = string.Empty;
                        txtShift.Text = string.Empty;
                        txtShift.Tag = string.Empty;
                        txtShiftStartTime.Text = string.Empty;
                        txtShiftEndTime.Text = string.Empty;
                        //txtShiftDateTime.Text = string.Empty;
                        return bRet;
                    }
                }
            }
            #endregion

            if (_CanChgQty)
            {
                if (string.IsNullOrWhiteSpace(txtUserName.Text) || string.IsNullOrWhiteSpace(txtUserName.Tag.ToString()))
                {
                    // 요청자를 입력 하세요.
                    Util.MessageValidation("SFU3451", (action) => { txtUserName.Focus(); });
                    return bRet;
                }

                // 20.12.16 사유 공란이어도 확정가능하도록 수정 - 한기덕 사원 요청
                //if (string.IsNullOrWhiteSpace(txtReqNote.Text))
                //{
                //    // 사유를 입력하세요.
                //    Util.MessageValidation("SFU1594", (action) => { txtReqNote.Focus(); });
                //    return bRet;
                //}
            }

            bRet = true;
            return bRet;
        }

        private bool CanSaveDefect()
        {
            bool bRet = false;

            if (bShowMsg)
                return bRet;

            if (!CommonVerify.HasDataGridRow(dgDefect))
            {
                //Util.Alert("불량 항목이 없습니다.");
                Util.MessageValidation("SFU1578");
                return bRet;
            }

            if (txtLotId.Text.Trim().Length < 1)
            {
                //Util.Alert("LOT 정보가 없습니다.");
                Util.MessageValidation("SFU1195");
                return bRet;
            }

            bRet = true;
            return bRet;
        }
        #endregion

        #region [Func]
        private void ShowLoadingIndicator()
        {
            if (loadingIndicator != null)
                loadingIndicator.Visibility = Visibility.Visible;
        }

        private void HideLoadingIndicator()
        {
            if (loadingIndicator != null)
                loadingIndicator.Visibility = Visibility.Collapsed;
        }

        private void GetAllData()
        {
            ClearControls();

            InitializeGrid();
            GetLotInfo();

            if (_ProcID.Equals(Process.LAMINATION))
            {
                GetMagzineList();
                GetDefectInfo();
            }
            else if (_ProcID.Equals(Process.STACKING_FOLDING))
            {
                GetBoxList();
                GetDefectInfo();
                if (_UNLDR_SHR_FLAG)
                {
                    DataTable dt = new DataTable();
                    dt.Columns.Add("EQPTID", typeof(string));
                    dt.Columns.Add("AREAID", typeof(string));
                    dt.Columns.Add("LOTID", typeof(string));
                    dt.Columns.Add("WIPSEQ", typeof(decimal));

                    DataRow dr = dt.NewRow();
                    dr["EQPTID"] = _EqptID;
                    dr["AREAID"] = LoginInfo.CFG_AREA_ID;
                    dr["LOTID"] = _LotID;
                    dr["WIPSEQ"] = Convert.ToDecimal(_WipSeq);
                    dt.Rows.Add(dr);
                    DataTable dtOtherProdLotInfo = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_EQPT_SHARING_UNLDR", "INDATA", "OUTDATA", dt);

                    DataTable dtQty = DataTableConverter.Convert(dgQty.ItemsSource);
                    int idx = 1;
                    foreach (DataRowView drv in dtOtherProdLotInfo.DefaultView)
                    {
                        DataTable lotInfo = GetLotInfo(Util.NVC(drv["LOTID"]));

                        DataRow dtRow = dtQty.NewRow();
                        dtRow["LOTID"] = Util.NVC(lotInfo.Rows[0]["LOTID"]);
                        dtRow["INPUTQTY"] = Util.NVC_Decimal(lotInfo.Rows[0]["INPUTQTY"]);
                        dtRow["GOODQTY"] = 0;
                        dtRow["DTL_DFCT_DEDUCT"] = 0;
                        dtRow["AFTER_DEDUCT_GOODQTY"] = 0;
                        dtRow["DTL_DEFECT_LOT"] = 0;
                        dtRow["DTL_LOSS_LOT"] = 0;
                        dtRow["DTL_CHARGE_PROD_LOT"] = 0;
                        dtRow["DFCT_SUM"] = 0;
                        dtRow["PRE_SECTION_QTY"] = Util.NVC_Decimal(lotInfo.Rows[0]["PRE_SECTION_QTY"]);
                        dtRow["AFTER_SECTION_QTY"] = Util.NVC_Decimal(lotInfo.Rows[0]["AFTER_SECTION_QTY"]);
                        dtRow["UNIDENTIFIED_QTY"] = Util.NVC_Decimal(lotInfo.Rows[0]["UNIDENTIFIED_QTY"]);
                        dtRow["RE_INPUT_QTY"] = Util.NVC_Decimal(lotInfo.Rows[0]["RE_INPUT_QTY"]);
                        dtQty.Rows.Add(dtRow);

                        DataTable boxList = GetBoxList(Util.NVC(drv["LOTID"]));
                        SumGoodQty(boxList, dtQty, idx);

                        DataTable defectList = GetDefectInfo(Util.NVC(drv["LOTID"]), Util.NVC(drv["EQPTID"]), Util.NVC(drv["WIPSEQ"]));
                        SumDefectQty(defectList, dtQty, idx);

                        idx += 1;
                    }
                    if(idx > 1)
                    {
                        sumRow.Visibility = Visibility.Visible;
                        dgQty.Columns["LOTID"].Visibility = Visibility.Visible;
                        Util.GridSetData(dgQty, dtQty, this.FrameOperation);
                    }
                }
            }
            else if (_ProcID.Equals(Process.CT_INSP))
            {
                GetBoxList();
                GetDefectInfo();
            }
            else if (_ProcID.Equals(Process.PACKAGING))
            {
                GetTrayList();
                GetDefectInfo();
            }
        }

        private void ClearControls()
        {
            Util.gridClear(dgQty);
            Util.gridClear(dgDefect);
            Util.gridClear(dgMagazine);
            Util.gridClear(dgBox);
            Util.gridClear(dgTray);

            txtLotId.Text = "";
            //txtCstID.Text = "";
            txtProdId.Text = "";
            txtAnLotType.Text = "";
            txtLotOperMode.Text = "";
            txtStartTime.Text = "";
        }

        private void ApplyPermissions()
        {
            List<Button> listAuth = new List<Button>();
            listAuth.Add(btnSave);
            listAuth.Add(btnDefectSave);

            Util.pageAuth(listAuth, FrameOperation.AUTHORITY);

            if (_CanChgQty)
            {
                dgQty.Columns["INPUTQTY"].IsReadOnly = false;
                dgQty.Columns["PRE_SECTION_QTY"].IsReadOnly = false;
                dgQty.Columns["AFTER_SECTION_QTY"].IsReadOnly = false;
            }
            else
            {
                dgQty.Columns["INPUTQTY"].IsReadOnly = true;
                dgQty.Columns["PRE_SECTION_QTY"].IsReadOnly = true;
                dgQty.Columns["AFTER_SECTION_QTY"].IsReadOnly = true;
            }
        }

        private void SumDefectQty()
        {
            try
            {
                DataTable dtSrc = DataTableConverter.Convert(dgDefect.ItemsSource);
                DataTable dtTgt = DataTableConverter.Convert(dgQty.ItemsSource);

                if (dtTgt == null || dtTgt.Rows.Count < 1) return;

                if (dtSrc != null && dtSrc.Rows.Count > 0)
                {
                    dtTgt.Rows[0]["DFCT_SUM"] = dtSrc.Compute("SUM(RESNQTY)", "RSLT_EXCL_FLAG = 'N' AND ISNULL(PRCS_ITEM_CODE, '') <> 'UNIDENTIFIED_QTY'").ToString().Equals("") ? 0 : double.Parse(dtSrc.Compute("SUM(RESNQTY)", "RSLT_EXCL_FLAG = 'N' AND ISNULL(PRCS_ITEM_CODE, '') <> 'UNIDENTIFIED_QTY'").ToString());
                    dtTgt.Rows[0]["DTL_DEFECT_LOT"] = dtSrc.Compute("SUM(RESNQTY)", "ACTID = 'DEFECT_LOT' AND RSLT_EXCL_FLAG = 'N' AND ISNULL(PRCS_ITEM_CODE, '') <> 'UNIDENTIFIED_QTY'").ToString().Equals("") ? 0 : double.Parse(dtSrc.Compute("SUM(RESNQTY)", "ACTID = 'DEFECT_LOT' AND RSLT_EXCL_FLAG = 'N' AND ISNULL(PRCS_ITEM_CODE, '') <> 'UNIDENTIFIED_QTY'").ToString());
                    dtTgt.Rows[0]["DTL_LOSS_LOT"] = dtSrc.Compute("SUM(RESNQTY)", "ACTID = 'LOSS_LOT' AND RSLT_EXCL_FLAG = 'N' AND ISNULL(PRCS_ITEM_CODE, '') <> 'UNIDENTIFIED_QTY'").ToString().Equals("") ? 0 : double.Parse(dtSrc.Compute("SUM(RESNQTY)", "ACTID = 'LOSS_LOT' AND RSLT_EXCL_FLAG = 'N' AND ISNULL(PRCS_ITEM_CODE, '') <> 'UNIDENTIFIED_QTY'").ToString());
                    dtTgt.Rows[0]["DTL_CHARGE_PROD_LOT"] = dtSrc.Compute("SUM(RESNQTY)", "ACTID = 'CHARGE_PROD_LOT' AND RSLT_EXCL_FLAG = 'N' AND ISNULL(PRCS_ITEM_CODE, '') <> 'UNIDENTIFIED_QTY'").ToString().Equals("") ? 0 : double.Parse(dtSrc.Compute("SUM(RESNQTY)", "ACTID = 'CHARGE_PROD_LOT' AND RSLT_EXCL_FLAG = 'N' AND ISNULL(PRCS_ITEM_CODE, '') <> 'UNIDENTIFIED_QTY'").ToString());

                    // 완성LOT 수량차감
                    dtTgt.Rows[0]["DTL_DFCT_DEDUCT"] = dtSrc.Compute("SUM(RESNQTY)", "RSLT_EXCL_FLAG = 'N' AND ISNULL(PRCS_ITEM_CODE, '') = 'OUT_LOT_QTY_DEDUCT'").ToString().Equals("") ? 0 : double.Parse(dtSrc.Compute("SUM(RESNQTY)", "RSLT_EXCL_FLAG = 'N' AND ISNULL(PRCS_ITEM_CODE, '') = 'OUT_LOT_QTY_DEDUCT'").ToString());
                }
                else
                {
                    dtTgt.Rows[0]["DFCT_SUM"] = 0;
                    dtTgt.Rows[0]["DTL_DEFECT_LOT"] = 0;
                    dtTgt.Rows[0]["DTL_LOSS_LOT"] = 0;
                    dtTgt.Rows[0]["DTL_CHARGE_PROD_LOT"] = 0;

                    dtTgt.Rows[0]["DTL_DFCT_DEDUCT"] = 0;
                }

                CalcSumQty(dtTgt);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void SumDefectQty(DataTable src, DataTable trgt, int idx)
        {
            try
            {
                if (trgt == null || trgt.Rows.Count < 1) return;

                if (src != null && src.Rows.Count > 0)
                {
                    trgt.Rows[idx]["DFCT_SUM"] = src.Compute("SUM(RESNQTY)", "RSLT_EXCL_FLAG = 'N' AND ISNULL(PRCS_ITEM_CODE, '') <> 'UNIDENTIFIED_QTY'").ToString().Equals("") ? 0 : double.Parse(src.Compute("SUM(RESNQTY)", "RSLT_EXCL_FLAG = 'N' AND ISNULL(PRCS_ITEM_CODE, '') <> 'UNIDENTIFIED_QTY'").ToString());
                    trgt.Rows[idx]["DTL_DEFECT_LOT"] = src.Compute("SUM(RESNQTY)", "ACTID = 'DEFECT_LOT' AND RSLT_EXCL_FLAG = 'N' AND ISNULL(PRCS_ITEM_CODE, '') <> 'UNIDENTIFIED_QTY'").ToString().Equals("") ? 0 : double.Parse(src.Compute("SUM(RESNQTY)", "ACTID = 'DEFECT_LOT' AND RSLT_EXCL_FLAG = 'N' AND ISNULL(PRCS_ITEM_CODE, '') <> 'UNIDENTIFIED_QTY'").ToString());
                    trgt.Rows[idx]["DTL_LOSS_LOT"] = src.Compute("SUM(RESNQTY)", "ACTID = 'LOSS_LOT' AND RSLT_EXCL_FLAG = 'N' AND ISNULL(PRCS_ITEM_CODE, '') <> 'UNIDENTIFIED_QTY'").ToString().Equals("") ? 0 : double.Parse(src.Compute("SUM(RESNQTY)", "ACTID = 'LOSS_LOT' AND RSLT_EXCL_FLAG = 'N' AND ISNULL(PRCS_ITEM_CODE, '') <> 'UNIDENTIFIED_QTY'").ToString());
                    trgt.Rows[idx]["DTL_CHARGE_PROD_LOT"] = src.Compute("SUM(RESNQTY)", "ACTID = 'CHARGE_PROD_LOT' AND RSLT_EXCL_FLAG = 'N' AND ISNULL(PRCS_ITEM_CODE, '') <> 'UNIDENTIFIED_QTY'").ToString().Equals("") ? 0 : double.Parse(src.Compute("SUM(RESNQTY)", "ACTID = 'CHARGE_PROD_LOT' AND RSLT_EXCL_FLAG = 'N' AND ISNULL(PRCS_ITEM_CODE, '') <> 'UNIDENTIFIED_QTY'").ToString());

                    // 완성LOT 수량차감
                    trgt.Rows[idx]["DTL_DFCT_DEDUCT"] = src.Compute("SUM(RESNQTY)", "RSLT_EXCL_FLAG = 'N' AND ISNULL(PRCS_ITEM_CODE, '') = 'OUT_LOT_QTY_DEDUCT'").ToString().Equals("") ? 0 : double.Parse(src.Compute("SUM(RESNQTY)", "RSLT_EXCL_FLAG = 'N' AND ISNULL(PRCS_ITEM_CODE, '') = 'OUT_LOT_QTY_DEDUCT'").ToString());
                }
                else
                {
                    trgt.Rows[idx]["DFCT_SUM"] = 0;
                    trgt.Rows[idx]["DTL_DEFECT_LOT"] = 0;
                    trgt.Rows[idx]["DTL_LOSS_LOT"] = 0;
                    trgt.Rows[idx]["DTL_CHARGE_PROD_LOT"] = 0;

                    trgt.Rows[idx]["DTL_DFCT_DEDUCT"] = 0;
                }

                CalcSumQty(trgt, idx);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void SumGoodQty()
        {
            try
            {
                DataTable dtSrc;
                DataTable dtTgt;

                if (_ProcID.Equals(Process.LAMINATION))
                    dtSrc = DataTableConverter.Convert(dgMagazine.ItemsSource);
                else if (_ProcID.Equals(Process.STACKING_FOLDING) || _ProcID.Equals(Process.CT_INSP))
                    dtSrc = DataTableConverter.Convert(dgBox.ItemsSource);
                else if (_ProcID.Equals(Process.PACKAGING))
                    dtSrc = DataTableConverter.Convert(dgTray.ItemsSource);
                else
                    dtSrc = null;

                dtTgt = DataTableConverter.Convert(dgQty.ItemsSource);

                if (dtTgt == null || dtTgt.Rows.Count < 1) return;

                if (dtSrc != null && dtSrc.Rows.Count > 0)
                {
                    if (_ProcID.Equals(Process.PACKAGING))
                        dtTgt.Rows[0]["GOODQTY"] = dtSrc.Compute("SUM(CELLQTY)", string.Empty).ToString().Equals("") ? 0 : double.Parse(dtSrc.Compute("SUM(CELLQTY)", string.Empty).ToString());
                    else
                        dtTgt.Rows[0]["GOODQTY"] = dtSrc.Compute("SUM(WIPQTY)", string.Empty).ToString().Equals("") ? 0 : double.Parse(dtSrc.Compute("SUM(WIPQTY)", string.Empty).ToString());

                    //if (_ProcID.Equals(Process.PACKAGING))
                    //    DataTableConverter.SetValue(dgQty.Rows[dgQty.TopRows.Count].DataItem, "GOODQTY", dtSrc.Compute("SUM(CELLQTY)", string.Empty).ToString().Equals("") ? 0 : double.Parse(dtSrc.Compute("SUM(CELLQTY)", string.Empty).ToString()));
                    //else
                    //    DataTableConverter.SetValue(dgQty.Rows[dgQty.TopRows.Count].DataItem, "GOODQTY", dtSrc.Compute("SUM(WIPQTY)", string.Empty).ToString().Equals("") ? 0 : double.Parse(dtSrc.Compute("SUM(WIPQTY)", string.Empty).ToString()));
                }
                else
                {
                    dtTgt.Rows[0]["GOODQTY"] = 0;
                    //DataTableConverter.SetValue(dgQty.Rows[dgQty.TopRows.Count].DataItem, "GOODQTY", 0);
                }

                CalcSumQty(dtTgt);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void SumGoodQty(DataTable src, DataTable trgt, int idx)
        {
            try
            {
                if (trgt == null || trgt.Rows.Count < 1) return;

                if (src != null && src.Rows.Count > 0)
                {
                    if (_ProcID.Equals(Process.PACKAGING))
                        trgt.Rows[idx]["GOODQTY"] = src.Compute("SUM(CELLQTY)", string.Empty).ToString().Equals("") ? 0 : double.Parse(src.Compute("SUM(CELLQTY)", string.Empty).ToString());
                    else
                        trgt.Rows[idx]["GOODQTY"] = src.Compute("SUM(WIPQTY)", string.Empty).ToString().Equals("") ? 0 : double.Parse(src.Compute("SUM(WIPQTY)", string.Empty).ToString());

                    //if (_ProcID.Equals(Process.PACKAGING))
                    //    DataTableConverter.SetValue(dgQty.Rows[dgQty.TopRows.Count].DataItem, "GOODQTY", dtSrc.Compute("SUM(CELLQTY)", string.Empty).ToString().Equals("") ? 0 : double.Parse(dtSrc.Compute("SUM(CELLQTY)", string.Empty).ToString()));
                    //else
                    //    DataTableConverter.SetValue(dgQty.Rows[dgQty.TopRows.Count].DataItem, "GOODQTY", dtSrc.Compute("SUM(WIPQTY)", string.Empty).ToString().Equals("") ? 0 : double.Parse(dtSrc.Compute("SUM(WIPQTY)", string.Empty).ToString()));
                }
                else
                {
                    trgt.Rows[idx]["GOODQTY"] = 0;
                    //DataTableConverter.SetValue(dgQty.Rows[dgQty.TopRows.Count].DataItem, "GOODQTY", 0);
                }

                CalcSumQty(trgt, idx);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void CalcSumQty(DataTable dtTmp)
        {
            try
            {
                if (dtTmp == null || dtTmp.Rows.Count < 1) return;

                double dGoodQty = Util.NVC(dtTmp.Rows[0]["GOODQTY"]).Equals("") ? 0 : double.Parse(Util.NVC(dtTmp.Rows[0]["GOODQTY"]));                
                double dDefectQty = Util.NVC(dtTmp.Rows[0]["DFCT_SUM"]).Equals("") ? 0 : double.Parse(Util.NVC(dtTmp.Rows[0]["DFCT_SUM"]));
                double dInputQty = Util.NVC(dtTmp.Rows[0]["INPUTQTY"]).Equals("") ? 0 : double.Parse(Util.NVC(dtTmp.Rows[0]["INPUTQTY"]));
                double dPreSecQty = Util.NVC(dtTmp.Rows[0]["PRE_SECTION_QTY"]).Equals("") ? 0 : double.Parse(Util.NVC(dtTmp.Rows[0]["PRE_SECTION_QTY"]));
                double dAftSecQty = Util.NVC(dtTmp.Rows[0]["AFTER_SECTION_QTY"]).Equals("") ? 0 : double.Parse(Util.NVC(dtTmp.Rows[0]["AFTER_SECTION_QTY"]));
                double dReInputQty = Util.NVC(dtTmp.Rows[0]["RE_INPUT_QTY"]).Equals("") ? 0 : double.Parse(Util.NVC(dtTmp.Rows[0]["RE_INPUT_QTY"]));

                double dGoodQtyAfterDeduct = 0;
                dGoodQtyAfterDeduct = dGoodQty - (Util.NVC(dtTmp.Rows[0]["DTL_DFCT_DEDUCT"]).Equals("") ? 0 : double.Parse(Util.NVC(dtTmp.Rows[0]["DTL_DFCT_DEDUCT"])));
                dtTmp.Rows[0]["AFTER_DEDUCT_GOODQTY"] = dGoodQtyAfterDeduct;

                double dUnidentifiedQty = dInputQty - (_UNLDR_SHR_FLAG ? dGoodQtyAfterDeduct : dGoodQty) - dDefectQty - (dPreSecQty + dAftSecQty);// + dReInputQty;

                dtTmp.Rows[0]["UNIDENTIFIED_QTY"] = dUnidentifiedQty;
                dgQty.ItemsSource = DataTableConverter.Convert(dtTmp);

                //// 미확인 LOSS 불량 코드 관리로 인한수정
                //int idx = _Util.GetDataGridRowIndex(dgDefect, "PRCS_ITEM_CODE", "UNIDENTIFIED_QTY");
                //if (idx >= 0)
                //    DataTableConverter.SetValue(dgDefect.Rows[idx].DataItem, "RESNQTY", dUnidentifiedQty);                
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void CalcSumQty(DataTable dtTmp, int idx)
        {
            try
            {
                if (dtTmp == null || dtTmp.Rows.Count < 1) return;

                double dGoodQty = Util.NVC(dtTmp.Rows[idx]["GOODQTY"]).Equals("") ? 0 : double.Parse(Util.NVC(dtTmp.Rows[idx]["GOODQTY"]));
                double dDefectQty = Util.NVC(dtTmp.Rows[idx]["DFCT_SUM"]).Equals("") ? 0 : double.Parse(Util.NVC(dtTmp.Rows[idx]["DFCT_SUM"]));
                double dInputQty = Util.NVC(dtTmp.Rows[idx]["INPUTQTY"]).Equals("") ? 0 : double.Parse(Util.NVC(dtTmp.Rows[idx]["INPUTQTY"]));
                double dPreSecQty = Util.NVC(dtTmp.Rows[idx]["PRE_SECTION_QTY"]).Equals("") ? 0 : double.Parse(Util.NVC(dtTmp.Rows[idx]["PRE_SECTION_QTY"]));
                double dAftSecQty = Util.NVC(dtTmp.Rows[idx]["AFTER_SECTION_QTY"]).Equals("") ? 0 : double.Parse(Util.NVC(dtTmp.Rows[idx]["AFTER_SECTION_QTY"]));
                double dReInputQty = Util.NVC(dtTmp.Rows[idx]["RE_INPUT_QTY"]).Equals("") ? 0 : double.Parse(Util.NVC(dtTmp.Rows[idx]["RE_INPUT_QTY"]));

                double dGoodQtyAfterDeduct = 0;
                dGoodQtyAfterDeduct = dGoodQty - (Util.NVC(dtTmp.Rows[idx]["DTL_DFCT_DEDUCT"]).Equals("") ? 0 : double.Parse(Util.NVC(dtTmp.Rows[idx]["DTL_DFCT_DEDUCT"])));
                dtTmp.Rows[idx]["AFTER_DEDUCT_GOODQTY"] = dGoodQtyAfterDeduct;

                double dUnidentifiedQty = dInputQty - (_UNLDR_SHR_FLAG ? dGoodQtyAfterDeduct : dGoodQty) - dDefectQty - (dPreSecQty + dAftSecQty);// + dReInputQty;

                dtTmp.Rows[idx]["UNIDENTIFIED_QTY"] = dUnidentifiedQty;
                dgQty.ItemsSource = DataTableConverter.Convert(dtTmp);

                //// 미확인 LOSS 불량 코드 관리로 인한수정
                //int idx = _Util.GetDataGridRowIndex(dgDefect, "PRCS_ITEM_CODE", "UNIDENTIFIED_QTY");
                //if (idx >= 0)
                //    DataTableConverter.SetValue(dgDefect.Rows[idx].DataItem, "RESNQTY", dUnidentifiedQty);                
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void wndShiftUser_Closed(object sender, EventArgs e)
        {
            CMM001.Popup.CMM_SHIFT_USER2 wndPopup = sender as CMM001.Popup.CMM_SHIFT_USER2;

            if (wndPopup.DialogResult == MessageBoxResult.OK)
            {
                #region 작업조,작업자 등록 화면 변경 요청 건 [C20200511-000024]
                GetEqptWrkInfo();
                #endregion
            }
        }

        private void popupUser()
        {
            CMM001.Popup.CMM_PERSON popUser = new CMM001.Popup.CMM_PERSON();
            popUser.FrameOperation = FrameOperation;

            object[] Parameters = new object[1];
            Parameters[0] = txtUserName.Text;
            C1WindowExtension.SetParameters(popUser, Parameters);

            popUser.Closed += new EventHandler(popUser_Closed);

            this.Dispatcher.BeginInvoke(new Action(() => popUser.ShowModal()));
        }

        private void popUser_Closed(object sender, EventArgs e)
        {
            CMM001.Popup.CMM_PERSON popup = sender as CMM001.Popup.CMM_PERSON;
            if (popup.DialogResult == MessageBoxResult.OK)
            {
                txtUserName.Text = popup.USERNAME;
                txtUserName.Tag = popup.USERID;

                txtReqNote.Focus();
            }
        }
        #endregion

        #endregion

        #region 작업조,작업자 등록 화면 변경 요청 건 [C20200511-000024]
        private DateTime GetSystemTime()
        {
            DateTime systemDateTime = new DateTime();

            const string bizRuleName = "BR_CUS_GET_SYSTIME";
            DataTable inDataTable = new DataTable("INDATA");
            DataTable dtResult = new ClientProxy().ExecuteServiceSync(bizRuleName, "INDATA", "OUTDATA", inDataTable);

            if (CommonVerify.HasTableRow(dtResult))
            {
                systemDateTime = Convert.ToDateTime(dtResult.Rows[0][0]);
            }

            return systemDateTime;
        }

        private void GetEqptWrkInfo()
        {
            try
            {
                //ShowLoadingIndicator();

                DataTable IndataTable = new DataTable("RQSTDT");
                IndataTable.Columns.Add("LANGID", typeof(string));
                IndataTable.Columns.Add("EQPTID", typeof(string));
                IndataTable.Columns.Add("SHOPID", typeof(string));
                IndataTable.Columns.Add("AREAID", typeof(string));
                IndataTable.Columns.Add("EQSGID", typeof(string));
                IndataTable.Columns.Add("PROCID", typeof(string));

                DataRow Indata = IndataTable.NewRow();
                Indata["LANGID"] = LoginInfo.LANGID;
                Indata["EQPTID"] = _EqptID;
                Indata["SHOPID"] = LoginInfo.CFG_SHOP_ID;
                Indata["AREAID"] = LoginInfo.CFG_AREA_ID;
                Indata["EQSGID"] = _LineID;
                Indata["PROCID"] = _ProcID;

                IndataTable.Rows.Add(Indata);

                DataTable result = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_TB_SFC_EQPT_WRK_INFO", "RQSTDT", "RSLTDT", IndataTable);

                if (result.Rows.Count > 0)
                {
                    if (!result.Rows[0].ItemArray[0].ToString().Equals(""))
                    {
                        txtShiftStartTime.Text = Util.NVC(result.Rows[0]["WRK_STRT_DTTM"]);
                    }
                    else
                    {
                        txtShiftStartTime.Text = string.Empty;
                    }

                    if (!result.Rows[0].ItemArray[1].ToString().Equals(""))
                    {
                        txtShiftEndTime.Text = Util.NVC(result.Rows[0]["WRK_END_DTTM"]);
                    }
                    else
                    {
                        txtShiftEndTime.Text = string.Empty;
                    }

                    //if (!string.IsNullOrEmpty(txtShiftStartTime.Text) && !string.IsNullOrEmpty(txtShiftEndTime.Text))
                    //{
                    //    txtShiftDateTime.Text = txtShiftStartTime.Text + " ~ " + txtShiftEndTime.Text;
                    //}
                    //else
                    //{
                    //    txtShiftDateTime.Text = string.Empty;
                    //}

                    if (Util.NVC(result.Rows[0]["WRK_USERID"]).Equals(""))
                    {
                        txtWorker.Text = string.Empty;
                        txtWorker.Tag = string.Empty;
                    }
                    else
                    {
                        txtWorker.Text = Util.NVC(result.Rows[0]["WRK_USERNAME"]);
                        txtWorker.Tag = Util.NVC(result.Rows[0]["WRK_USERID"]);
                    }

                    if (Util.NVC(result.Rows[0]["SHFT_ID"]).Equals(""))
                    {
                        txtShift.Tag = string.Empty;
                        txtShift.Text = string.Empty;
                    }
                    else
                    {
                        txtShift.Text = Util.NVC(result.Rows[0]["SHFT_NAME"]);
                        txtShift.Tag = Util.NVC(result.Rows[0]["SHFT_ID"]);
                    }
                }
                else
                {
                    txtWorker.Text = string.Empty;
                    txtWorker.Tag = string.Empty;
                    txtShift.Text = string.Empty;
                    txtShift.Tag = string.Empty;
                    txtShiftStartTime.Text = string.Empty;
                    txtShiftEndTime.Text = string.Empty;
                    //txtShiftDateTime.Text = string.Empty;
                }
            }
            catch (Exception ex)
            {
                //HiddenLoadingIndicator();
                Util.MessageException(ex);
            }
        }
        #endregion
    }
}
