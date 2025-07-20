/*************************************************************************************
 Created Date : 2020.10.27
      Creator : 
   Decription : 대LOT 삭제
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
using LGC.GMES.MES.CMM001.Extensions;
using LGC.GMES.MES.CMM001.UserControls;
using LGC.GMES.MES.CMM001.Popup;
using C1.WPF.DataGrid;
using System.Linq;
using System.Windows.Media;
using System.Windows.Controls;
using LGC.GMES.MES.CMM001;
using System.Windows.Input;

namespace LGC.GMES.MES.ELEC003
{
    public partial class ELEC003_LARGELOT_DELETE : C1Window, IWorkArea
    {
        #region Declaration

        public bool bSaveConfirm { get; set; }

        private string _processCode = string.Empty;

        Util _Util = new Util();
        BizDataSet _bizRule = new BizDataSet();

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

        public ELEC003_LARGELOT_DELETE()
        {
            InitializeComponent();
        }
        #endregion

        #region Event

        private void C1Window_Loaded(object sender, RoutedEventArgs e)
        {
            InitializeUserControls();
            SetControl();
            SetControlVisibility();

            SelectLargeLot();

            this.Loaded -= C1Window_Loaded;
        }

        private void InitializeUserControls()
        {
        }

        private void SetControl()
        {
            object[] tmps = C1WindowExtension.GetParameters(this);

            _processCode = tmps[0] as string;
            txtEquipmentCode.Text = tmps[1] as string;
            txtEquipmentName.Text = tmps[2] as string;
       }

        private void SetControlVisibility()
        {
        }

        /// <summary>
        /// 대LOT 선택
        /// </summary>
        private void dgLargeLotChoice_Checked(object sender, RoutedEventArgs e)
        {
            try
            {
                RadioButton rb = sender as RadioButton;
                if (rb?.DataContext == null) return;

                if (rb.IsChecked != null)
                {
                    DataRowView drv = rb.DataContext as DataRowView;
                    if (drv != null && ((bool)rb.IsChecked && drv.Row["CHK"].ToString().Equals("0") || Convert.ToBoolean(drv.Row["CHK"]) == false))
                    {
                        int idx = ((DataGridCellPresenter)rb.Parent).Row.Index;

                        for (int i = 0; i < ((DataGridCellPresenter)rb.Parent).DataGrid.Rows.Count; i++)
                        {
                            DataTableConverter.SetValue(((DataGridCellPresenter)rb.Parent).DataGrid.Rows[i].DataItem, "CHK", idx == i);
                        }

                        //row 색 바꾸기
                        dgLargeLot.SelectedIndex = idx;
                    }
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        /// <summary>
        /// 삭제 
        /// </summary>
        private void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidationDelete())
                return;

            //대LOT 정리를 하시겠습니까?
            Util.MessageConfirm("SFU1488", (sresult) =>
            {
                if (sresult == MessageBoxResult.OK)
                {
                    DeleteProcess();
                }
            });
        }

        /// <summary>
        /// 닫기
        /// </summary>
        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = MessageBoxResult.Cancel;
        }

        #endregion

        #region User Method

        #region [BizCall]

        /// <summary>
        /// 조회
        /// </summary>
        private void SelectLargeLot()
        {
            try
            {
                DataTable inTable = new DataTable();
                inTable.Columns.Add("PROCID", typeof(string));
                inTable.Columns.Add("EQPTID", typeof(string));

                DataRow newRow = inTable.NewRow();
                newRow["PROCID"] = _processCode;
                newRow["EQPTID"] = txtEquipmentCode.Text;
                inTable.Rows.Add(newRow);

                ShowLoadingIndicator();

                new ClientProxy().ExecuteService("DA_PRD_SEL_COATMLOT_LOT", "RQSTDT", "RSLTDT", inTable, (bizResult, bizException) =>
                {
                    HiddenLoadingIndicator();

                    if (bizException != null)
                    {
                        Util.MessageException(bizException);
                        return;
                    }

                    Util.GridSetData(dgLargeLot, bizResult, null, true);
                });
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        /// <summary>
        /// 삭제
        /// </summary>
        private void DeleteProcess()
        {
            try
            {
                DataRow[] drSelect = DataTableConverter.Convert(dgLargeLot.ItemsSource).Select("CHK = 1");

                DataTable inTable = new DataTable();
                inTable.Columns.Add("SRCTYPE", typeof(string));
                inTable.Columns.Add("IFMODE", typeof(string));
                inTable.Columns.Add("EQPTID", typeof(string));
                inTable.Columns.Add("LOTID", typeof(string));
                inTable.Columns.Add("USERID", typeof(string));
                ///////////////////////////////////////////////////////////////////////////////

                DataRow newRow = inTable.NewRow();
                newRow["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                newRow["IFMODE"] = IFMODE.IFMODE_OFF;
                newRow["EQPTID"] = txtEquipmentCode.Text;
                newRow["LOTID"] = drSelect[0]["LOTID_LARGE"].ToString();
                newRow["USERID"] = LoginInfo.USERID;
                inTable.Rows.Add(newRow);

                ShowLoadingIndicator();

                new ClientProxy().ExecuteService("BR_PRD_REG_TERM_LARGE_LOT", "INDATA", null, inTable, (result, bizException) =>
                {
                    try
                    {
                        HiddenLoadingIndicator();

                        if (bizException != null)
                        {
                            Util.MessageException(bizException);
                            return;
                        }

                        // 삭제되었습니다.
                        Util.MessageInfo("SFU1273");

                        bSaveConfirm = true;

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
        private bool ValidationDelete()
        {
            DataRow[] drSelect = DataTableConverter.Convert(dgLargeLot.ItemsSource).Select("CHK = 1");

            if (drSelect.Length == 0)
            {
                // 대LOT을 선택하십시오.
                Util.MessageValidation("SFU1490"); 
                return false;
            }

            return true;
        }
        #endregion

        #region [Func]

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
