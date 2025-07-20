/*************************************************************************************
 Created Date : 2017.12.26
      Creator : 정문교
   Decription : 대차 보관
--------------------------------------------------------------------------------------
 [Change History]
    
**************************************************************************************/

using C1.WPF;
using C1.WPF.DataGrid;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using System;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace LGC.GMES.MES.CMM001.Popup
{
    /// <summary>
    /// CMM_SHIFT.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class CMM_POLYMER_FORM_CART_STORAGE : C1Window, IWorkArea
    {
        #region Declaration & Constructor

        private string _procID = string.Empty;        // 공정코드
        private string _eqptID = string.Empty;        // 설비코드

        Util _Util = new Util();
        BizDataSet _bizRule = new BizDataSet();

        private bool _load = true;

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

        public CMM_POLYMER_FORM_CART_STORAGE()
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
                SetCombo();
                _load = false;
            }

        }

        private void InitializeUserControls()
        {
        }
        private void SetControl()
        {
            object[] tmps = C1WindowExtension.GetParameters(this);

            _procID = tmps[0] as string;
            _eqptID = tmps[2] as string;

            txtProcess.Text = tmps[1] as string;
            txtEquipment.Text = tmps[3] as string;
            txtCartID.Text = tmps[4] as string;

            SetGridCartList();
            SetGridAssyLotList();
        }
        private void SetCombo()
        {
        }

        #endregion

        #region [보관]
        private void btnStorage_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidationStorage())
                return;

            // 대차를 현재 공정에서 보관하시겠습니까?
            Util.MessageConfirm("SFU4407", (result) =>
            {
                if (result == MessageBoxResult.OK)
                {
                    CartStorage();
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

        #region Mehod

        /// <summary>
        /// Cart List
        /// </summary>
        private void SetGridCartList()
        {
            try
            {
                DataTable inTable = new DataTable();
                inTable.Columns.Add("LANGID", typeof(string));
                inTable.Columns.Add("EQPTID", typeof(string));
                inTable.Columns.Add("PROCID", typeof(string));
                inTable.Columns.Add("CTNR_ID", typeof(string));
                inTable.Columns.Add("CTNR_STAT_CODE", typeof(string));

                // INDATA SET
                DataRow newRow = inTable.NewRow();
                newRow["LANGID"] = LoginInfo.LANGID;
                newRow["EQPTID"] = _eqptID;
                newRow["PROCID"] = _procID;
                newRow["CTNR_ID"] = txtCartID.Text;
                inTable.Rows.Add(newRow);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_TB_SFC_CTNR_PC", "INDATA", "OUTDATA", inTable);
                Util.GridSetData(dgCart, dtResult, FrameOperation, true);

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        /// <summary>
        /// 조립LOT List
        /// </summary>
        private void SetGridAssyLotList()
        {
            try
            {
                string bizRuleName = string.Empty;

                DataTable inTable = new DataTable();
                inTable.Columns.Add("LANGID", typeof(string));
                inTable.Columns.Add("EQPTID", typeof(string));
                inTable.Columns.Add("CART_ID", typeof(string));

                DataRow newRow = inTable.NewRow();
                newRow["LANGID"] = LoginInfo.LANGID;
                newRow["EQPTID"] = _eqptID;
                newRow["CART_ID"] = txtCartID.Text;

                inTable.Rows.Add(newRow);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_TB_SFC_CTNR_ASSYLOT_PC", "INDATA", "OUTDATA", inTable);
                Util.GridSetData(dgAssyLot, dtResult, null, true);

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }

        }

        /// <summary>
        /// 대차 보관
        /// </summary>
        private void CartStorage()
        {
            try
            {
                ShowLoadingIndicator();

                // DATA SET
                DataSet inDataSet = new DataSet();
                DataTable inTable = inDataSet.Tables.Add("INDATA");
                inTable.Columns.Add("IFMODE", typeof(string));
                inTable.Columns.Add("SRCTYPE", typeof(string));
                inTable.Columns.Add("PROCID", typeof(string));
                inTable.Columns.Add("USERID", typeof(string));
                inTable.Columns.Add("LANGID", typeof(string));

                DataTable inCNTR = inDataSet.Tables.Add("INCTNR");
                inCNTR.Columns.Add("CTNR_ID", typeof(string));

                DataRow newRow = inTable.NewRow();
                newRow["IFMODE"] = IFMODE.IFMODE_OFF;
                newRow["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                newRow["PROCID"] = _procID;
                newRow["USERID"] = LoginInfo.USERID;
                newRow["LANGID"] = LoginInfo.LANGID;
                inTable.Rows.Add(newRow);

                newRow = inCNTR.NewRow();
                newRow["CTNR_ID"] = txtCartID.Text;
                inCNTR.Rows.Add(newRow);

                new ClientProxy().ExecuteService_Multi("BR_PRD_REG_STORE_CTNR", "INDATA,INCTNR", null, (bizResult, bizException) =>
                {
                    try
                    {
                        if (bizException != null)
                        {
                            HiddenLoadingIndicator();
                            Util.MessageException(bizException);
                            return;
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
                }, inDataSet);
            }
            catch (Exception ex)
            {
                HiddenLoadingIndicator();
                Util.MessageException(ex);
            }
        }

        private bool ChkCartStatus()
        {
            bool IsInspProc = false;

            try
            {

                DataTable inTable = new DataTable();
                inTable.Columns.Add("LANGID", typeof(string));
                inTable.Columns.Add("AREAID", typeof(string));
                inTable.Columns.Add("CMCDTYPE", typeof(string));

                DataRow newRow = inTable.NewRow();
                newRow["LANGID"] = LoginInfo.LANGID;
                newRow["AREAID"] = LoginInfo.CFG_AREA_ID;
                newRow["CMCDTYPE"] = "FORM_INSP_PROCID";
                inTable.Rows.Add(newRow);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_PROCESS_PC", "INDATA", "OUTDATA", inTable);

                if (dtResult != null && dtResult.Rows.Count > 0)
                {
                    DataRow[] dr = dtResult.Select("CBO_CODE = '" + _procID + "'");

                    if (dr.Length > 0)
                    {
                        IsInspProc = true;
                    }
                }

                return IsInspProc;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return IsInspProc;
            }
        }
        #endregion

        #region [Func]
        private bool ValidationStorage()
        {
            DataTable dt = DataTableConverter.Convert(dgCart.ItemsSource);
            DataRow[] dr = dt.Select("CTNR_ID = '" + txtCartID.Text + "'");

            if (dr.Length == 0)
            {
                // 대차 정보가 없습니다.
                Util.MessageValidation("SFU4365");
                return false;
            }

            // 임시로 주석 처리 : 최종 외관인경우 체크 제외 전체불량인 경우 양품 Inbox가 없다
            //if (Util.NVC_Int(dr[0]["CELL_QTY"]) == 0)
            //{
            //    // 대차에 Inbox 정보가 없습니다.
            //    Util.MessageValidation("SFU4375");
            //    return false;
            //}

            //if (Util.NVC(dr[0]["CART_SHEET_PRT_FLAG"]).Equals("N"))
            //{
            //    // 대차 발행후 보관 처리가 가능 합니다.
            //    Util.MessageValidation("SFU4489");
            //    return false;
            //}
            /////////////////////////////////////////////////////////////////////////////////////////

            // 대차 라벨 발행여부 체크
            //if (string.Equals(_procID, Process.PolymerOffLineCharacteristic) ||
            //    string.Equals(_procID, Process.PolymerFinalExternalDSF) ||
            //    string.Equals(_procID, Process.PolymerFinalExternal))
            //{
            if (ChkCartStatus())
            {
                if (Util.NVC_Int(dr[0]["PROC_COUNT"]) != 0)
                {
                    // 생산중인 대차는 보관할 수 없습니다.
                    Util.MessageValidation("SFU4423");
                    return false;
                }
            }
            else
            {
                if (Util.NVC_Int(dr[0]["CELL_QTY"]) == 0)
                {
                    // 대차에 Inbox 정보가 없습니다.
                    Util.MessageValidation("SFU4375");
                    return false;
                }

                if (Util.NVC(dr[0]["CART_SHEET_PRT_FLAG"]).Equals("N"))
                {
                    // 대차 발행후 보관 처리가 가능 합니다.
                    Util.MessageValidation("SFU4489");
                    return false;
                }

                if (Util.NVC_Int(dr[0]["NO_PRINT_COUNT"]) != 0)
                {
                    // Inbox 태그를 전부 발행해야 보관 처리가 가능 합니다.
                    Util.MessageValidation("SFU4490");
                    return false;
                }
            }

            return true;
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







    }
}