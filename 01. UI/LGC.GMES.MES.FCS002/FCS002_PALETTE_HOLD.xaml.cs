/*************************************************************************************
 Created Date : 2017.07.27
      Creator : 
   Decription : Pallet Hold
--------------------------------------------------------------------------------------
 [Change History]

 2023.03.10  0.2   LEEHJ    SI               소형활성화 MES 복사
 ***************************************************************************************/

using System;
using System.Windows;
using System.Data;

using C1.WPF;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using LGC.GMES.MES.CMM001.Class;
using C1.WPF.DataGrid;
using System.Collections.Generic;
using System.Windows.Controls;
using LGC.GMES.MES.CMM001;
using LGC.GMES.MES.CMM001.Extensions;
using Application = System.Windows.Application;
using DataGridLength = C1.WPF.DataGrid.DataGridLength;
using DataGridRow = C1.WPF.DataGrid.DataGridRow;
using System.Linq;
using System.Windows.Input;

namespace LGC.GMES.MES.FCS002
{
    public partial class FCS002_PALETTE_HOLD : C1Window, IWorkArea
    {
        #region Declaration

        Util _Util = new Util();
        BizDataSet _bizRule = new BizDataSet();
        CommonCombo _combo = new CMM001.Class.CommonCombo();

        private string _procID = string.Empty;        // 공정코드
        private string _eqptID = string.Empty;        // 설비코드

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

        public FCS002_PALETTE_HOLD()
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
                SetControlVisibility();
                SetControlHeader();
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

            // SET COMMON
            txtProcess.Text = tmps[1] as string;
            txtEquipment.Text = tmps[3] as string;

            // SET 생산 Lot 정보
            DataRow prodLot = tmps[4] as DataRow;
            DataRow prodPallet = tmps[5] as DataRow;

            if (prodLot == null || prodPallet == null)
                return;

            DataTable prodLotBind = new DataTable();
            DataTable prodPalletBind = new DataTable();

            prodLotBind = prodLot.Table.Clone();
            prodLotBind.ImportRow(prodLot);

            prodPalletBind = prodPallet.Table.Clone();
            prodPalletBind.ImportRow(prodPallet);

            Util.GridSetData(dgLot, prodLotBind, null, true);
            Util.GridSetData(dgPallet, prodPalletBind, null);

            //HOLD 사유
            string[] sFilter = { "HOLD_LOT" };
            _combo.SetCombo(cboHoldType, CommonCombo.ComboStatus.SELECT, sCase: "ACTIVITIREASON", sFilter: sFilter);

        }
        private void SetControlVisibility()
        {
            if (string.Equals(_procID, Process.CircularGrader) || string.Equals(_procID, Process.CircularCharacteristic))
            {
                // 원형
                dgLot.Columns["WND_GR_CODE"].Visibility = Visibility.Collapsed;
                dgLot.Columns["WND_EQPTID"].Visibility = Visibility.Collapsed;
                dgPallet.Columns["VLTG_GRD_CODE"].Visibility = Visibility.Collapsed;
            }
            else
            {
                // 초소형
                dgLot.Columns["SOC_VALUE"].Visibility = Visibility.Collapsed;
            }

            // 저항등급
            if (_procID.Equals(Process.CircularCharacteristicGrader) || _procID.Equals(Process.CircularReTubing))
            {
                dgPallet.Columns["RSST_GRD_CODE"].Visibility = Visibility.Visible;
            }
            else
            {
                dgPallet.Columns["RSST_GRD_CODE"].Visibility = Visibility.Collapsed;
            }

        }

        private void SetControlHeader()
        {
            if (string.Equals(_procID, Process.SmallOcv) || string.Equals(_procID, Process.SmallLeak) || string.Equals(_procID, Process.SmallDoubleTab))
            {
                // 초소형 OCV 검사, 초소형 누액검사, 초소형 더블탭
                this.Header = ObjectDic.Instance.GetObjectName("대차Hold");
            }
            else
            {
                this.Header = ObjectDic.Instance.GetObjectName("Pallet Hold");
            }
        }

        #endregion

        #region [Hold]
        /// <summary>
        /// Hold
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnHold_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidatePaletteHold())
                return;

            // HOLD 하시겠습니까?
            Util.MessageConfirm("SFU1345", (result) =>
            {
                if (result == MessageBoxResult.OK)
                {
                    PalletHold();
                }
            });
        }
        #endregion

        #region [닫기]
        /// <summary>
        /// 닫기
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = MessageBoxResult.Cancel;
        }
        #endregion

        #region [예상해제일]
        private void dtExpected_SelectedDataTimeChanged(object sender, SelectionChangedEventArgs e)
        {
            if (Convert.ToDecimal(dtExpected.SelectedDateTime.ToString("yyyyMMdd")) < Convert.ToDecimal(DateTime.Now.ToString("yyyyMMdd")))
            {
                Util.MessageValidation("SFU1740");    //오늘 이후 날짜만 지정 가능합니다.
                dtExpected.SelectedDateTime = DateTime.Now;
            }

        }
        #endregion

        #region [담당자]
        private void txtPerson_GotFocus(object sender, RoutedEventArgs e)
        {
            dgPersonSelect.Visibility = Visibility.Collapsed;
        }

        private void txtPerson_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Return)
            {
                SetPerson();
            }
        }

        private void dgPersonSelect_Checked(object sender, RoutedEventArgs e)
        {
            RadioButton rb = sender as RadioButton;

            txtPersonId.Text = Util.NVC(DataTableConverter.GetValue(rb.DataContext, "USERID").ToString());
            txtPerson.Text = Util.NVC(DataTableConverter.GetValue(rb.DataContext, "USERNAME").ToString());
            txtPersonDept.Text = Util.NVC(DataTableConverter.GetValue(rb.DataContext, "DEPTNAME"));

            dgPersonSelect.Visibility = Visibility.Collapsed;
            this.Focus();

        }
        #endregion

        #endregion

        #region User Method

        #region [BizCall]

        /// <summary>
        /// 담당자 조회
        /// </summary>
        private void SetPerson()
        {
            try
            {
                dgPersonSelect.Visibility = Visibility.Collapsed;

                DataTable inTable = new DataTable();
                inTable.Columns.Add("USERNAME", typeof(string));
                inTable.Columns.Add("LANGID", typeof(string));

                DataRow newRow = inTable.NewRow();
                newRow["USERNAME"] = txtPerson.Text;
                newRow["LANGID"] = LoginInfo.LANGID;

                inTable.Rows.Add(newRow);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_PERSON_BY_NAME", "INDATA", "OUTDATA", inTable);

                if (dtResult == null || dtResult.Rows.Count == 0)
                {
                    //사용자 정보가 없습니다.
                    Util.MessageInfo("SFU1592");
                }
                else if (dtResult.Rows.Count == 1)
                {
                    txtPerson.Text = dtResult.Rows[0]["USERNAME"].ToString();
                    txtPersonId.Text = dtResult.Rows[0]["USERID"].ToString();
                    txtPersonDept.Text = dtResult.Rows[0]["DEPTNAME"].ToString();
                }
                else
                {
                    dgPersonSelect.Visibility = Visibility.Visible;

                    Util.gridClear(dgPersonSelect);

                    dgPersonSelect.ItemsSource = DataTableConverter.Convert(dtResult);
                    //this.Focusable = true;
                    //this.Focus();
                    //this.Focusable = false;
                }

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }

        }

        /// <summary>
        /// Pallet Hold
        /// </summary>
        private void PalletHold()
        {
            try
            {
                ShowLoadingIndicator();

                const string bizRuleName = "BR_PRD_REG_HOLD_LOT";

                // DATA SET
                DataSet inDataSet = new DataSet();

                DataTable inTable = inDataSet.Tables.Add("INDATA");
                inTable.Columns.Add("SRCTYPE", typeof(string));
                inTable.Columns.Add("IFMODE", typeof(string));
                inTable.Columns.Add("USERID", typeof(string));
                inTable.Columns.Add("ACTION_USERID", typeof(string));

                DataRow newRow = inTable.NewRow();
                newRow["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                newRow["IFMODE"] = IFMODE.IFMODE_OFF;
                newRow["USERID"] = txtPersonId.Text;
                newRow["ACTION_USERID"] = LoginInfo.USERID;
                inTable.Rows.Add(newRow);

                DataTable inLot = inDataSet.Tables.Add("INLOT");
                inLot.Columns.Add("LOTID", typeof(string));
                inLot.Columns.Add("HOLD_NOTE", typeof(string));
                inLot.Columns.Add("RESNCODE", typeof(string));
                inLot.Columns.Add("HOLD_CODE", typeof(string));
                inLot.Columns.Add("UNHOLD_SCHD_DATE", typeof(string));

                newRow = inLot.NewRow();
                newRow["LOTID"] = DataTableConverter.GetValue(dgPallet.Rows[0].DataItem, "PALLETE_ID");
                newRow["HOLD_NOTE"] = txtHold.Text;
                newRow["RESNCODE"] = cboHoldType.SelectedValue.ToString();
                newRow["HOLD_CODE"] = cboHoldType.SelectedValue.ToString();
                newRow["UNHOLD_SCHD_DATE"] = Util.GetCondition(dtExpected);
                inLot.Rows.Add(newRow);

                new ClientProxy().ExecuteService_Multi(bizRuleName, "INDATA,INLOT", null, (bizResult, bizException) =>
                {
                    try
                    {
                        HiddenLoadingIndicator();

                        if (bizException != null)
                        {
                            Util.MessageException(bizException);
                            return;
                        }

                        if ((bool)chkTagPrint.IsChecked)
                        {
                            CMM_FORM_TAG_PRINT popupTagPrint = new CMM_FORM_TAG_PRINT();
                            popupTagPrint.FrameOperation = this.FrameOperation;

                            popupTagPrint.HoldPalletYN = "Y";

                            // SET PARAMETER
                            object[] parameters = new object[8];
                            parameters[0] = _procID;
                            parameters[1] = _eqptID;
                            parameters[2] = DataTableConverter.GetValue(dgPallet.Rows[0].DataItem, "PALLETE_ID").GetString();
                            parameters[3] = DataTableConverter.GetValue(dgPallet.Rows[0].DataItem, "WIPSEQ").GetString();
                            parameters[4] = DataTableConverter.GetValue(dgPallet.Rows[0].DataItem, "CELL_QTY").GetString();
                            parameters[5] = DataTableConverter.GetValue(dgPallet.Rows[0].DataItem, "PRINT_YN").GetString().Equals("Y") ? "N" : "Y";      // 디스패치 처리
                            parameters[6] = DataTableConverter.GetValue(dgPallet.Rows[0].DataItem, "PRINT_YN").GetString();
                            parameters[7] = "Y";      // Direct 출력 여부
                            C1WindowExtension.SetParameters(popupTagPrint, parameters);

                            popupTagPrint.Closed += new EventHandler(popupTagPrint_Closed);

                            popupTagPrint.Show();

                        }

                        //// 정상 처리 되었습니다.
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
        #endregion

        #region[[Validation]
        private bool ValidatePaletteHold()
        {
            if (dgPallet.Rows.Count == 0)
            {
                // Hold 대상 Pallet가 없습니다.
                Util.MessageValidation("SFU4010");
                return false;
            }

            if (cboHoldType.SelectedValue == null || cboHoldType.SelectedValue.ToString().Trim().Equals("SELECT"))
            {
                // HOLD 사유를 선택 하세요.
                Util.MessageValidation("SFU1342");
                return false;
            }

            if (string.IsNullOrWhiteSpace(txtPersonId.Text))
            {
                // 담당자를 입력 하세요.
                Util.MessageValidation("SFU4011");
                return false;
            }

            return true;
        }
        #endregion

        #region [Func]
        private void popupTagPrint_Closed(object sender, EventArgs e)
        {
            CMM_FORM_TAG_PRINT window = sender as CMM_FORM_TAG_PRINT;

            if (window.DialogResult == MessageBoxResult.OK)
            {
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

        #endregion

        #endregion

    }
}
