/*************************************************************************************
 Created Date : 2017.08.30
       Creator : 
    Decription : X-Ray 재작업 작업시작
--------------------------------------------------------------------------------------
 [Change History]

**************************************************************************************/

using System;
using System.Windows;
using System.Windows.Input;
using System.Data;

using C1.WPF;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using LGC.GMES.MES.CMM001.Class;
using C1.WPF.DataGrid;
using System.Collections.Generic;
using System.Windows.Controls;
using System.Text.RegularExpressions;
using LGC.GMES.MES.CMM001.Extensions;

namespace LGC.GMES.MES.ASSY002
{
    public partial class ASSY002_008_RUNSTART : C1Window, IWorkArea
    {
        #region Declaration

        private readonly Util _util = new Util();
        private string _equipmentCode = string.Empty;
      
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

        public ASSY002_008_RUNSTART()
        {
            InitializeComponent();
        }
        #endregion

        #region Event

        #region [Form Load]
        private void ASSY002_008_RUNSTART_Loaded(object sender, RoutedEventArgs e)
        {
            object[] tmps = C1WindowExtension.GetParameters(this);
            _equipmentCode = tmps[0] as string;
            GetXrayRunList();
        }
        #endregion

        #region [DataGrid] - gdInputItem_CommittedEdit, txtLotId_PreviewKeyDown, btnSearch_Click

        private void rdoChk_Checked(object sender, RoutedEventArgs e)
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
                        dgInputProduct.SelectedIndex = idx;
                    }
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
            finally { }
        }

        #endregion

        #region [LOT 조회]
        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                GetXrayRunList();
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        #endregion

        #region [작업시작]
        private void btnStart_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidateStartRun())
                return;

            Util.MessageConfirm("SFU1240", (result) =>
            {
                if (result == MessageBoxResult.OK)
                {
                    // LOT 착공
                    StartRunProcess();
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

        //GetWashingRunList();
        private void GetXrayRunList()
        {
            try
            {
                ShowLoadingIndicator();
                string bizRuleName = "DA_PRD_SEL_WAIT_PALLET_LIST_XR";
                DataTable inTable = new DataTable("RQSTDT");
                inTable.Columns.Add("EQPTID", typeof(string));
                inTable.Columns.Add("PALLETID", typeof(string));
                inTable.Columns.Add("PRODID", typeof(string));

                DataRow dr = inTable.NewRow();
                dr["EQPTID"] = _equipmentCode;
                dr["PALLETID"] = txtPalletID.Text;
                inTable.Rows.Add(dr);

                new ClientProxy().ExecuteService(bizRuleName, "RQSTDT", "RSLTDT", inTable, (bizResult, bizException) =>
                {
                    HiddenLoadingIndicator();
                    if (bizException != null)
                    {
                        Util.MessageException(bizException);
                        return;
                    }

                    dgInputProduct.ItemsSource = DataTableConverter.Convert(bizResult);

                });
            }
            catch (Exception ex)
            {
                HiddenLoadingIndicator();
                Util.MessageException(ex);
            }
        }

        private void StartRunProcess()
        {
            try
            {
                ShowLoadingIndicator();

                string bizRuleName = "BR_PRD_REG_START_PROD_LOT_ASSY_XR";

                DataTable inTable = new DataTable("INDATA");
                inTable.Columns.Add("SRCTYPE", typeof(string));
                inTable.Columns.Add("IFMODE", typeof(string));
                inTable.Columns.Add("EQPTID", typeof(string));
                inTable.Columns.Add("PALLETID", typeof(string));
                inTable.Columns.Add("USERID", typeof(string));

                DataRow dr = inTable.NewRow();
                dr["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                dr["IFMODE"] = IFMODE.IFMODE_OFF;
                dr["EQPTID"] = _equipmentCode;
                dr["PALLETID"] = _util.GetDataGridFirstRowBycheck(dgInputProduct, "CHK").Field<string>("PALLETID").GetString();
                dr["USERID"] = LoginInfo.USERID;
                inTable.Rows.Add(dr);

                DataSet ds = new DataSet();
                ds.Tables.Add(inTable);
                //string xml = ds.GetXml();

                new ClientProxy().ExecuteService(bizRuleName, "INDATA", null, inTable, (bizResult, bizException) =>
                {
                    HiddenLoadingIndicator();
                    if (bizException != null)
                    {
                        Util.MessageException(bizException);
                        return;
                    }
                    Util.MessageInfo("SFU1889");
                    this.DialogResult = MessageBoxResult.OK;
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
        private bool ValidateStartRun()
        {
            // CHECK INPUT HALFPRODUCT
            if (dgInputProduct.Rows.Count == 0)
            {
                //Util.Alert("투입 반제품이 존재하지 않습니다.");
                Util.MessageValidation("SFU1952");
                return false;
            }

            if (_util.GetDataGridCheckCnt(dgInputProduct, "CHK") == 0)
            {
                //Util.Alert("선택된 투입 반제품이 존재하지 않습니다.");
                Util.MessageValidation("SFU1650");
                return false;
            }

            for (int i = 0; i < dgInputProduct.Rows.Count; i++)
            {
                if (string.IsNullOrEmpty(Util.NVC(DataTableConverter.GetValue(dgInputProduct.Rows[i].DataItem, "ASSY_LOTID"))))
                {
                    //Util.Alert("투입 반제품 정보에 LOT ID가 없습니다.");
                    Util.MessageValidation("SFU1946");
                    dgInputProduct.SelectedIndex = i;
                    return false;
                }

                if (string.IsNullOrEmpty(Util.NVC(DataTableConverter.GetValue(dgInputProduct.Rows[i].DataItem, "PRODID"))))
                {
                    //Util.Alert("투입 반제품 정보에 제품ID가 없습니다.");
                    Util.MessageValidation("SFU1946");
                    dgInputProduct.SelectedIndex = i;
                    return false;
                }
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
