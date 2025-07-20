/*************************************************************************************
 Created Date : 
      Creator : 
   Decription : 
--------------------------------------------------------------------------------------
 [Change History]

**************************************************************************************/

using System;
using System.Windows;
using System.Data;
using C1.WPF;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using LGC.GMES.MES.CMM001.Class;
using System.Windows.Controls;
using LGC.GMES.MES.CMM001;
using LGC.GMES.MES.CMM001.Extensions;
using System.Windows.Input;

namespace LGC.GMES.MES.FORM001
{
    public partial class FORM001_011_CHANGE_PROD_LOT : C1Window, IWorkArea
    {
        #region Declaration

        Util _Util = new Util();
        BizDataSet _bizRule = new BizDataSet();
        CommonCombo _combo = new CMM001.Class.CommonCombo();

        private string _eqptID = string.Empty;          //설비코드
        private string _wipStat = string.Empty;          //공정코드
        private string _procID = string.Empty;          //공정코드
        private string _wipTypeCode = string.Empty;     //WIPTYPECODE
        private string _assyLotID = string.Empty;       //조립LTO

        private bool _load = true;

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

        public FORM001_011_CHANGE_PROD_LOT()
        {
            InitializeComponent();
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

        private void InitializeUserControls()
        {
        }

        private void SetControl()
        {
            object[] tmps = C1WindowExtension.GetParameters(this);

            if(tmps.Length >= 6)
            {
                _eqptID = tmps[0] as string;
                _wipStat = tmps[1] as string;
                _procID = tmps[2] as string;
                _wipTypeCode = tmps[3] as string;
                _assyLotID = tmps[4] as string;


                txtProcess.Text = _procID;
                //txtEquipment.Text = _eqptID;
                txtAssyLot.Text = _assyLotID;


                DataRow[] prodShipPallet = tmps[5] as DataRow[];

                if (prodShipPallet == null)
                {
                    return;
                }

                DataTable prodShipPalletBind = new DataTable();
                prodShipPalletBind = prodShipPallet[0].Table.Clone();

                for (int row = 0; row < prodShipPallet.Length; row++)
                {
                    prodShipPalletBind.ImportRow(prodShipPallet[row]);
                }

                Util.GridSetData(dgProductionShipPallet, prodShipPalletBind, null);
            }

            if (string.IsNullOrEmpty(_eqptID) || string.IsNullOrEmpty(_wipStat) || string.IsNullOrEmpty(_procID) || string.IsNullOrEmpty(_wipTypeCode) || string.IsNullOrEmpty(_assyLotID))
            {
                return;
            }

            SetEquipmentCombo(cboEquipment);
            GetProductLot();
        }

        #endregion

        private void SetEquipmentCombo(C1ComboBox pCbo)
        {
            string bizRuleName = "DA_BAS_SEL_EQUIPMENT_CBO";

            if (LoginInfo.CFG_SHOP_ID == "G182")
            {
                bizRuleName = "DA_BAS_SEL_EQUIPMENT_CBO_NJ";
            }

            string[] arrColumn = { "LANGID", "EQSGID", "PROCID", "COATER_EQPT_TYPE_CODE" };
            string[] arrCondition = { LoginInfo.LANGID, LoginInfo.CFG_EQSG_ID, _procID, null };
            string selectedValueText = pCbo.SelectedValuePath;
            string displayMemberText = pCbo.DisplayMemberPath;

            CommonCombo.CommonBaseCombo(bizRuleName, pCbo, arrColumn, arrCondition, CommonCombo.ComboStatus.NONE, selectedValueText, displayMemberText, _eqptID);

            //////////////////// 설비가 N건인 경우 Select 추가
            if (pCbo.Items.Count > 1 || pCbo.Items.Count == 0)
            {
                DataTable dtEqpt = DataTableConverter.Convert(pCbo.ItemsSource);
                DataRow drEqpt = dtEqpt.NewRow();
                drEqpt[selectedValueText] = "SELECT";
                drEqpt[displayMemberText] = "- SELECT -";
                dtEqpt.Rows.InsertAt(drEqpt, 0);

                pCbo.ItemsSource = null;
                pCbo.ItemsSource = dtEqpt.Copy().AsDataView();

                int Index = 0;

                if (!string.IsNullOrEmpty(_eqptID))
                {
                    DataRow[] drIndex = dtEqpt.Select("CBO_CODE ='" + _eqptID + "'");

                    if (drIndex.Length > 0)
                    {
                        Index = dtEqpt.Rows.IndexOf(drIndex[0]);
                        pCbo.SelectedValue = _eqptID;
                    }
                }

                pCbo.SelectedIndex = Index;
            }
        }

        public void GetProductLot()
        {
            try
            {
                if (cboEquipment.SelectedIndex < 0 || cboEquipment.SelectedValue.GetString().Equals("SELECT"))
                {
                    return;
                }

                if (string.IsNullOrEmpty(_wipStat))
                {
                    return;
                }

                if (string.IsNullOrEmpty(_procID))
                {
                    return;
                }

                if (string.IsNullOrEmpty(_wipTypeCode))
                {
                    return;
                }

                if (string.IsNullOrEmpty(_assyLotID))
                {
                    return;
                }

                ShowLoadingIndicator();

                DataTable inTable = new DataTable();
                inTable.Columns.Add("LANGID", typeof(string));
                inTable.Columns.Add("EQSGID", typeof(string));
                inTable.Columns.Add("EQPTID", typeof(string));
                inTable.Columns.Add("WIPSTAT", typeof(string));
                inTable.Columns.Add("PROCID", typeof(string));
                inTable.Columns.Add("WIPTYPECODE", typeof(string));
                inTable.Columns.Add("ASSY_LOTID", typeof(string));

                DataRow newRow = inTable.NewRow();
                newRow["LANGID"] = LoginInfo.LANGID;
                newRow["EQSGID"] = LoginInfo.CFG_EQSG_ID;
                newRow["EQPTID"] = Util.NVC(cboEquipment.SelectedValue); ;
                newRow["WIPSTAT"] = _wipStat;
                newRow["PROCID"] = _procID;
                newRow["WIPTYPECODE"] = _wipTypeCode;
                newRow["ASSY_LOTID"] = _assyLotID;

                inTable.Rows.Add(newRow);

                new ClientProxy().ExecuteService("DA_PRD_SEL_PRODUCTLOT_FO", "RQSTDT", "RSLTDT", inTable, (result, ex) =>
                {
                    if (ex != null)
                    {
                        Util.MessageException(ex);
                    }
                    else
                    {
                        Util.GridSetData(dgProductLot, result, FrameOperation, true);

                        if (result == null || result.Rows.Count == 0)
                        {
                            return;
                        }
                        else
                        {
                            DataTableConverter.SetValue(dgProductLot.Rows[0].DataItem, "CHK", true);
                        }
                    }
                });

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
            finally
            {
                HiddenLoadingIndicator();
            }
        }

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = MessageBoxResult.Cancel;
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

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidateSave())
            {
                return;
            }

            //저장하시겠습니까?
            Util.MessageConfirm("SFU1241", (result) =>
            {
                if (result == MessageBoxResult.OK)
                {
                    ChangeProdLot();
                }
            });

        }

        private void ChangeProdLot()
        {
            try
            {
                ShowLoadingIndicator();

                const string bizRuleName = "BR_PRD_REG_CHANGE_PROD_LOT_SHIP_PALLET";

                // DATA SET
                DataSet inDataSet = new DataSet();

                DataTable inTable = inDataSet.Tables.Add("INDATA");
                inTable.Columns.Add("LANGID", typeof(string));
                inTable.Columns.Add("USERID", typeof(string));
                inTable.Columns.Add("AREAID", typeof(string));
                inTable.Columns.Add("PROD_LOTID", typeof(string));

                DataRow newRow = inTable.NewRow();
                newRow["LANGID"] = LoginInfo.LANGID;
                newRow["USERID"] = LoginInfo.USERID;
                newRow["AREAID"] = LoginInfo.CFG_AREA_ID;
                newRow["PROD_LOTID"] = Util.NVC(txtProductLot.Text);
                inTable.Rows.Add(newRow);

                DataTable inPallet = inDataSet.Tables.Add("INPALLET");
                inPallet.Columns.Add("BOXID", typeof(string));

                for (int row = 0; row < dgProductionShipPallet.Rows.Count; row++)
                {
                    newRow = inPallet.NewRow();
                    newRow["BOXID"] = DataTableConverter.GetValue(dgProductionShipPallet.Rows[row].DataItem, "BOXID");
                    inPallet.Rows.Add(newRow);
                }

                new ClientProxy().ExecuteService_Multi(bizRuleName, "INDATA,INPALLET", null, (bizResult, bizException) =>
                {
                    try
                    {
                        HiddenLoadingIndicator();

                        if (bizException != null)
                        {
                            Util.MessageException(bizException);
                            return;
                        }

                        Util.MessageInfo("SFU1889");    //정상 처리 되었습니다

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

        private bool ValidateSave()
        {
            if (string.IsNullOrEmpty(Util.NVC(txtProductLot.Text)))
            {
                Util.MessageValidation("SFU4014");  //생산 Lot 정보가 없습니다.
                return false;
            }

            DataRow[] dr = DataTableConverter.Convert(dgProductionShipPallet.ItemsSource).Select("PR_LOTID = '" + txtProductLot.Text + "'");
            if (dr.Length >= 1)
            {
                Util.MessageValidation("SFU3818");  //동일한 생산LOT 으로 변경할 수 없습니다.
                return false;
            }

            return true;
        }

        private void dgProductLotChoice_Checked(object sender, RoutedEventArgs e)
        {
            try
            {
                RadioButton rbt = sender as RadioButton;
                C1.WPF.DataGrid.C1DataGrid dg = ((C1.WPF.DataGrid.DataGridCellPresenter)rbt.Parent).DataGrid;
                dg.SelectedIndex = ((C1.WPF.DataGrid.DataGridCellPresenter)rbt.Parent).Cell.Row.Index;

                txtProductLot.Text = Util.NVC(DataTableConverter.GetValue(dgProductLot.Rows[dg.SelectedIndex].DataItem, "LOTID"));
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void cboEquipment_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            ClearProductLot();
            GetProductLot();
        }

        private void ClearProductLot()
        {
            txtProductLot.Text = string.Empty;
            Util.gridClear(dgProductLot);
        }
    }
}
