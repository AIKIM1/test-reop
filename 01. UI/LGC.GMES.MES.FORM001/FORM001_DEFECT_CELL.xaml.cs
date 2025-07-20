/*************************************************************************************
 Created Date : 2017.12.06
      Creator : 
   Decription : 활성화 후공정 폴리머 - 불량 Cell 등록 팝업
--------------------------------------------------------------------------------------
 [Change History]

**************************************************************************************/

using System;
using System.Windows;
using System.Data;
using System.Windows.Input;
using System.Linq;
using System.Windows.Media;

using C1.WPF;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using LGC.GMES.MES.CMM001.Class;
using C1.WPF.DataGrid;
using System.Collections.Generic;
using System.Windows.Controls;
using LGC.GMES.MES.CMM001;
using LGC.GMES.MES.CMM001.Extensions;

namespace LGC.GMES.MES.FORM001
{
    public partial class FORM001_DEFECT_CELL : C1Window, IWorkArea
    {
        #region Declaration
        Util _Util = new Util();
        BizDataSet _bizRule = new BizDataSet();

        private string _procID = string.Empty;        // 공정코드
        private string _eqptID = string.Empty;        // 설비코드

        private bool _load = true;

        public string ProdLotId { get; set; }

        /// <summary>
        ///  Frame과 상호작용하기 위한 객체
        /// </summary>
        public IFrameOperation FrameOperation
        {
            get;
            set;
        }
        #endregion

        #region Initialize

        public FORM001_DEFECT_CELL()
        {
            InitializeComponent();
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
                _load = false;
            }

            txtCellID.Focus();
        }

        private void InitializeUserControls()
        {
            txtCellID.Text = string.Empty;
            txtSumCellCount.Text = string.Empty;
            Util.gridClear(dgCell);
            txtCellID.Focus();

        }
        private void SetControl()
        {
            object[] tmps = C1WindowExtension.GetParameters(this);

            _procID = tmps[0] as string;
            _eqptID = tmps[2] as string;

            // SET 생산 Lot 정보
            DataRow prodLot = tmps[4] as DataRow;
            // SET 불량 정보
            DataRow Defect = tmps[4] as DataRow;

            // SET Text
            txtProdID.Text = prodLot[""].ToString();
            txtPJT.Text = prodLot[""].ToString();
            txtAssyLot.Text = prodLot[""].ToString();
            txtProdID.Text = prodLot[""].ToString();

            txtDefectTag.Text = Defect[""].ToString();
            txtDefectName.Text = Defect[""].ToString();

        }
        #endregion

        #region [Cell ID Scan]
        private void txtCellID_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                // Grid 중복 체크
                if (!ValidateCellDuplicate())
                    return;

                // 조립 LOT 일치 여부 확인, 동일 생산 LOT에 다른 불량으로 기 등록되었는지 확인
                if (!ValidateCellScan())
                    return;

                // 그리드에 추가
                SetGridCellADD();
            }
        }
        #endregion

        #region [초기화]
        private void btnClear_Click(object sender, RoutedEventArgs e)
        {
            // 초기화 하시겠습니까?
            Util.MessageConfirm("SFU3440", (result) =>
            {
                if (result == MessageBoxResult.OK)
                {
                    InitializeUserControls();
                }
            });
        }
        #endregion

        #region [저장]
        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidateSave())
                return;

            // 저장하시겠습니까?
            Util.MessageConfirm("SFU1241", (result) =>
            {
                if (result == MessageBoxResult.OK)
                {
                    DefectCellSave();
                }
            });
        }
        #endregion

        #region [삭제]
        private void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidateSave())
                return;

            // 삭제 하시겠습니까?
            Util.MessageConfirm("SFU1230", (result) =>
            {
                if (result == MessageBoxResult.OK)
                {
                    DefectCellDelete();
                }
            });
        }
        #endregion

        #region [닫기]
        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = MessageBoxResult.Cancel;
        }
        #endregion


        #endregion

        #region User Method

        #region [BizCall]

        /// <summary>
        /// Scan Cell 체크
        /// </summary>
        private bool ValidateCellScan()
        {
            try
            {
                ShowLoadingIndicator();

                DataTable inTable = new DataTable();
                inTable.Columns.Add("LANGID", typeof(string));
                inTable.Columns.Add("PALLETID", typeof(string));
                inTable.Columns.Add("PROCID", typeof(string));

                DataRow newRow = inTable.NewRow();
                newRow["LANGID"] = LoginInfo.LANGID; ;
                //newRow["PALLETID"] = txtStartTray.Text;
                newRow["PROCID"] = _procID;
                inTable.Rows.Add(newRow);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("BR_PRD_CHK_INPUT_PALLET_ASSY_LOT_INFO_FO", "INDATA", "OUTDATA", inTable);

                return true;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return false;
            }
            finally
            {
                HiddenLoadingIndicator();
            }

        }

        /// <summary>
        /// 불량 Cell 저장
        /// </summary>
        private void DefectCellSave()
        {
            try
            {
                ShowLoadingIndicator();

                const string bizRuleName = "BR_PRD_REG_START_PROD_LOT_RT";

                // DATA Table
                DataTable inTable = new DataTable();
                inTable.Columns.Add("SRCTYPE", typeof(string));
                inTable.Columns.Add("IFMODE", typeof(string));
                inTable.Columns.Add("EQPTID", typeof(string));
                inTable.Columns.Add("PALLETID", typeof(string));
                inTable.Columns.Add("USERID", typeof(string));
                inTable.Columns.Add("WRK_SUPPLIERID", typeof(string));
                inTable.Columns.Add("ASSY_LOTID", typeof(string));
                inTable.Columns.Add("FORM_WRK_TYPE_CODE", typeof(string));             // (*)활성화 작업 유형

                //DataRow newRow = inTable.NewRow();
                //newRow["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                //newRow["IFMODE"] = IFMODE.IFMODE_OFF;
                //newRow["EQPTID"] = Util.NVC(_eqptID);
                //newRow["PALLETID"] = Util.NVC(DataTableConverter.GetValue(dgLot.Rows[0].DataItem, "PALLETID"));
                //newRow["USERID"] = LoginInfo.USERID;
                //newRow["WRK_SUPPLIERID"] = cboWorkSupplier.SelectedValue.ToString();
                //newRow["ASSY_LOTID"] = Util.NVC(DataTableConverter.GetValue(dgLot.Rows[0].DataItem, "ASSY_LOTID"));
                //newRow["FORM_WRK_TYPE_CODE"] = cboFormWorkType.SelectedValue.ToString();

                //inTable.Rows.Add(newRow);

                DataRow[] drSave = DataTableConverter.Convert(dgCell.ItemsSource).Select("CHK = 1");
                DataRow newRow;

                foreach (DataRow Addrow in drSave)
                {
                    newRow = inTable.NewRow();
                    newRow["PALLETID"] = Util.NVC(Addrow[""]);
                    inTable.Rows.Add(newRow);
                }

                new ClientProxy().ExecuteService(bizRuleName, "RQSTDT", "OUTDATA", inTable, (result, bizException) =>
                {
                    try
                    {
                        HiddenLoadingIndicator();

                        if (bizException != null)
                        {
                            Util.MessageException(bizException);
                            return;
                        }

                        if (result != null && result.Rows.Count > 0)
                        {
                            ProdLotId = result.Rows[0]["LOTID"].ToString();
                        }

                        ////Util.AlertInfo("정상 처리 되었습니다.");
                        //Util.MessageInfo("SFU1889");

                        this.DialogResult = MessageBoxResult.OK;
                    }
                    catch (Exception ex)
                    {
                        HiddenLoadingIndicator();
                        Util.MessageException(ex);
                    }
                });
            }
            catch (Exception ex)
            {
                HiddenLoadingIndicator();
                Util.MessageException(ex);
            }
        }

        /// <summary>
        /// 불량 Cell 삭제
        /// </summary>
        private void DefectCellDelete()
        {
            try
            {
                ShowLoadingIndicator();

                const string bizRuleName = "BR_PRD_REG_START_PROD_LOT_RT";

                // DATA Table
                DataTable inTable = new DataTable();
                inTable.Columns.Add("SRCTYPE", typeof(string));
                inTable.Columns.Add("IFMODE", typeof(string));
                inTable.Columns.Add("EQPTID", typeof(string));
                inTable.Columns.Add("PALLETID", typeof(string));
                inTable.Columns.Add("USERID", typeof(string));
                inTable.Columns.Add("WRK_SUPPLIERID", typeof(string));
                inTable.Columns.Add("ASSY_LOTID", typeof(string));
                inTable.Columns.Add("FORM_WRK_TYPE_CODE", typeof(string));             // (*)활성화 작업 유형

                //DataRow newRow = inTable.NewRow();
                //newRow["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                //newRow["IFMODE"] = IFMODE.IFMODE_OFF;
                //newRow["EQPTID"] = Util.NVC(_eqptID);
                //newRow["PALLETID"] = Util.NVC(DataTableConverter.GetValue(dgLot.Rows[0].DataItem, "PALLETID"));
                //newRow["USERID"] = LoginInfo.USERID;
                //newRow["WRK_SUPPLIERID"] = cboWorkSupplier.SelectedValue.ToString();
                //newRow["ASSY_LOTID"] = Util.NVC(DataTableConverter.GetValue(dgLot.Rows[0].DataItem, "ASSY_LOTID"));
                //newRow["FORM_WRK_TYPE_CODE"] = cboFormWorkType.SelectedValue.ToString();

                //inTable.Rows.Add(newRow);

                DataRow[] drSave = DataTableConverter.Convert(dgCell.ItemsSource).Select("CHK = 1");
                DataRow newRow;

                foreach (DataRow Addrow in drSave)
                {
                    newRow = inTable.NewRow();
                    newRow["PALLETID"] = Util.NVC(Addrow[""]);
                    inTable.Rows.Add(newRow);
                }

                new ClientProxy().ExecuteService(bizRuleName, "RQSTDT", "OUTDATA", inTable, (result, bizException) =>
                {
                    try
                    {
                        HiddenLoadingIndicator();

                        if (bizException != null)
                        {
                            Util.MessageException(bizException);
                            return;
                        }

                        if (result != null && result.Rows.Count > 0)
                        {
                            ProdLotId = result.Rows[0]["LOTID"].ToString();
                        }

                        ////Util.AlertInfo("정상 처리 되었습니다.");
                        //Util.MessageInfo("SFU1889");

                        this.DialogResult = MessageBoxResult.OK;
                    }
                    catch (Exception ex)
                    {
                        HiddenLoadingIndicator();
                        Util.MessageException(ex);
                    }
                });
            }
            catch (Exception ex)
            {
                HiddenLoadingIndicator();
                Util.MessageException(ex);
            }
        }
        #endregion

        #region[[Validation]
        private bool ValidateCellDuplicate()
        {
            // 그리드 중복 체크
            DataRow[] dr = DataTableConverter.Convert(dgCell.ItemsSource).Select("CELLID ='" + txtCellID.Text + "'");

            if (dr.Length > 0)
            {
                // 중복된 CellID 정보가 존재합니다.
                Util.MessageValidation("SFU3681");
                return false;
            }

            return true;
        }

        private bool ValidateSave()
        {
            // 체크 여부
            DataRow[] dr = DataTableConverter.Convert(dgCell.ItemsSource).Select("CHK = 1");

            if (dr.Length == 0)
            {
                // 선택된 항목이 없습니다.
                Util.MessageValidation("SFU1651");
                return false;
            }

            return true;
        }

        private bool ValidateDelete()
        {
            // 체크 여부
            DataRow[] dr = DataTableConverter.Convert(dgCell.ItemsSource).Select("CHK = 1");

            if (dr.Length == 0)
            {
                // 선택된 항목이 없습니다.
                Util.MessageValidation("SFU1651");
                return false;
            }

            return true;
        }
        #endregion

        #region [Func]

        private void SetGridCellADD()
        {
            DataTable dt = DataTableConverter.Convert(dgCell.ItemsSource);

            dt.Rows.Add(true, txtCellID.Text);
            dt.AcceptChanges();

            Util.GridSetData(dgCell, dt, null);

            txtSumCellCount.Text = dt.Rows.Count.ToString();
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





        #endregion

        #endregion

    }
}
