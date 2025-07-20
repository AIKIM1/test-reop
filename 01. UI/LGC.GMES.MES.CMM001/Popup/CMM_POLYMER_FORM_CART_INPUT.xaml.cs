/*************************************************************************************
 Created Date : 2018.02.28
      Creator : 정문교
   Decription : 대차 투입
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
using System.Windows.Media;
using DataGridRow = C1.WPF.DataGrid.DataGridRow;
using LGC.GMES.MES.CMM001.Extensions;
using System.Linq;

namespace LGC.GMES.MES.CMM001.Popup
{
    /// <summary>
    /// CMM_POLYMER_FORM_CART_INPUT.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class CMM_POLYMER_FORM_CART_INPUT : C1Window, IWorkArea
    {
        #region Declaration & Constructor

        private string _procID = string.Empty;        // 공정코드
        private string _procName = string.Empty;      // 공정명
        private string _eqptID = string.Empty;        // 설비코드
        private string _prodLotID = string.Empty;       // 생산LOT
        private CheckBoxHeaderType _inBoxHeaderType;

        private readonly BizDataSet _bizDataSet = new BizDataSet();
        private readonly Util _util = new Util();

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

        public CMM_POLYMER_FORM_CART_INPUT()
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
            _procName = tmps[1] as string;
            _eqptID = tmps[2] as string;
            _prodLotID = tmps[4] as string;

            txtProcess.Text = tmps[1] as string;
            txtEquipment.Text = tmps[3] as string;
            txtCartID.Text = tmps[5] as string;
        }
        private void SetCombo()
        {
            SetAssyLotCombo(true);
        }

        #endregion

        #region [조립LOT 변경 cboAssyLot_SelectedValueChanged]
        private void cboAssyLot_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            if (cboAssyLot.SelectedValue == null)
                return;

            InboxSearch();
        }
        #endregion

        #region [투입]
        private void btnInput_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidationInput())
                return;

            // 투입 하시겠습니까?
            Util.MessageConfirm("SFU1248", (result) =>
            {
                if (result == MessageBoxResult.OK)
                {
                    InputProcess();
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

        private void SetAssyLotCombo(bool bLoad = false)
        {
            try
            {
                ShowLoadingIndicator();

                cboAssyLot.SelectedValueChanged -= cboAssyLot_SelectedValueChanged;

                DataTable inTable = new DataTable();
                inTable.Columns.Add("CTNR_ID", typeof(string));

                DataRow newRow = inTable.NewRow();
                newRow["CTNR_ID"] = txtCartID.Text;
                inTable.Rows.Add(newRow);

                new ClientProxy().ExecuteService("DA_PRD_SEL_ASSYLOT_CART_INPUT", "INDATA", "OUTDATA", inTable, (bizResult, bizException) =>
                {
                    try
                    {
                        HiddenLoadingIndicator();

                        if (bizException != null)
                        {
                            Util.MessageException(bizException);
                            this.DialogResult = MessageBoxResult.Cancel;
                            return;
                        }

                        if (bizResult == null || bizResult.Rows.Count == 0)
                        {
                            if (bLoad)
                            {
                                // 대차 정보가 없습니다.
                                Util.MessageValidation("SFU4365");
                            }
                            this.DialogResult = MessageBoxResult.Cancel;
                            return;
                        }

                        cboAssyLot.ItemsSource = null;
                        cboAssyLot.ItemsSource = bizResult.Copy().AsDataView();
                        cboAssyLot.SelectedIndex = 0;

                        cboAssyLot.SelectedValueChanged += cboAssyLot_SelectedValueChanged;

                        InboxSearch();
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
        /// 조립LOT의 Inbox 조회
        /// </summary>
        private void InboxSearch()
        {
            try
            {
                ShowLoadingIndicator();

                _inBoxHeaderType = CheckBoxHeaderType.Two;

                DataTable inTable = new DataTable();
                inTable.Columns.Add("LANGID", typeof(string));
                inTable.Columns.Add("CTNR_ID", typeof(string));
                inTable.Columns.Add("ASSY_LOTID", typeof(string));

                // INDATA SET
                DataRow newRow = inTable.NewRow();
                newRow["LANGID"] = LoginInfo.LANGID;
                newRow["CTNR_ID"] = txtCartID.Text;
                newRow["ASSY_LOTID"] = Util.NVC(cboAssyLot.SelectedValue);
                inTable.Rows.Add(newRow);

                new ClientProxy().ExecuteService("DA_PRD_SEL_INBOX_CART_INPUT", "INDATA", "OUTDATA", inTable, (bizResult, bizException) =>
                {
                    try
                    {
                        HiddenLoadingIndicator();

                        if (bizException != null)
                        {
                            Util.MessageException(bizException);
                            return;
                        }

                        Util.GridSetData(dgList, bizResult, FrameOperation, true);
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
        /// 투입
        /// </summary>
        private void InputProcess()
        {
            try
            {
                ShowLoadingIndicator();

                DataSet inDataSet = new DataSet();

                DataTable inTable = inDataSet.Tables.Add("INDATA");
                inTable.Columns.Add("SRCTYPE", typeof(string));
                inTable.Columns.Add("IFMODE", typeof(string));
                inTable.Columns.Add("EQPTID", typeof(string));
                inTable.Columns.Add("PROD_LOTID", typeof(string));
                inTable.Columns.Add("USERID", typeof(string));

                DataTable inInput = inDataSet.Tables.Add("IN_INPUT");
                inInput.Columns.Add("INPUT_LOTID", typeof(string));

                DataRow newRow = inTable.NewRow();
                newRow["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                newRow["IFMODE"] = IFMODE.IFMODE_OFF;
                newRow["EQPTID"] = Util.NVC(_eqptID);
                newRow["PROD_LOTID"] = _prodLotID;
                newRow["USERID"] = LoginInfo.USERID;
                inTable.Rows.Add(newRow);

                DataRow[] drSelect = DataTableConverter.Convert(dgList.ItemsSource).Select("CHK = 1");

                foreach(DataRow dr in drSelect)
                {
                    newRow = inInput.NewRow();
                    newRow["INPUT_LOTID"] = Util.NVC(dr["LOTID"]);
                    inInput.Rows.Add(newRow);
                }

                new ClientProxy().ExecuteService_Multi("BR_PRD_CHK_INPUT_LOT_INBOX", "INDATA,IN_INPUT", "OUTDATA", (bizResult, bizException) =>
                {
                    try
                    {
                        if (bizException != null)
                        {
                            HiddenLoadingIndicator();
                            Util.MessageException(bizException);
                            return;
                        }

                        //////Util.AlertInfo("정상 처리 되었습니다.");
                        ////Util.MessageInfo("SFU1889");
                        ////this.DialogResult = MessageBoxResult.OK;

                        SetAssyLotCombo();
                        InboxSearch();
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

        #region [Func]

        private bool ValidationInput()
        {
            if (dgList.Rows.Count - dgList.BottomRows.Count <= 0)
            {
                // 조회된 Data가 없습니다.
                Util.MessageValidation("SFU1905");
                return false;
            }

            if (_util.GetDataGridCheckFirstRowIndex(dgList, "CHK") < 0)
            {
                // 선택된 항목이 없습니다.
                Util.MessageValidation("SFU1651");
                return false;
            }

            return true;
        }
  
        private void tbCheckHeaderAll_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            C1DataGrid dg = dgList;
            if (dg?.ItemsSource == null) return;

            foreach (DataGridRow row in dg.Rows)
            {
                switch (_inBoxHeaderType)
                {
                    case CheckBoxHeaderType.One:
                        DataTableConverter.SetValue(row.DataItem, "CHK", true);
                        break;
                    case CheckBoxHeaderType.Two:
                        DataTableConverter.SetValue(row.DataItem, "CHK", false);
                        break;
                }
            }

            switch (_inBoxHeaderType)
            {
                case CheckBoxHeaderType.One:
                    _inBoxHeaderType = CheckBoxHeaderType.Two;
                    break;
                case CheckBoxHeaderType.Two:
                    _inBoxHeaderType = CheckBoxHeaderType.One;
                    break;
            }

            dg.EndEdit();
            dg.EndEditRow(true);
        }

        private enum CheckBoxHeaderType
        {
            Zero,
            One,
            Two,
            Three
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