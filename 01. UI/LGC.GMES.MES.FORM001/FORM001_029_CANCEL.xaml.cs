/*************************************************************************************
 Created Date : 2017.12.07
      Creator : 
   Decription : 파활성화 후공정 파우치 : 작업시작
--------------------------------------------------------------------------------------
 [Change History]

**************************************************************************************/

using System;
using System.Windows;
using System.Data;
using System.Windows.Input;

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
using System.Windows.Media;

namespace LGC.GMES.MES.FORM001
{
    public partial class FORM001_029_CANCEL : C1Window, IWorkArea
    {
        #region Declaration
        Util _Util = new Util();
        BizDataSet _bizRule = new BizDataSet();
        CommonCombo _combo = new CMM001.Class.CommonCombo();

        private string _processCode = string.Empty;
        private string _processName = string.Empty;
        private string _equipmentCode = string.Empty;
        private string _equipmentName = string.Empty;

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

        public FORM001_029_CANCEL()
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
                object[] parameters = C1WindowExtension.GetParameters(this);
                _processCode = parameters[0] as string;
                _processName = parameters[1] as string;
                _equipmentCode = parameters[2] as string;
                _equipmentName = parameters[3] as string;

                // SET 대차 정보
                DataRow prodCart = parameters[4] as DataRow;
                DataTable prodLot = parameters[5] as DataTable;

                InitializeUserControls();
                SetControl();
                SetControlVisibility();

                if (_processName != null) txtProcess.Text = _processName;
                if (_equipmentName != null) txtEquipment.Text = _equipmentName;

                // 대차
                DataTable prodCartBind = new DataTable();
                prodCartBind = prodCart.Table.Clone();
                prodCartBind.ImportRow(prodCart);
                Util.GridSetData(dgCart, prodCartBind, null, true);

                // 생산LOT
                Util.GridSetData(dgLot, prodLot, null, true);

                _load = false;
            }

        }

        private void InitializeUserControls()
        {
        }
        private void SetControl()
        {
        }

        private void SetControlVisibility()
        {
        }

        #endregion

        #region [작업취소]
        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidateCancel())
                return;

            // 대차 작업실적이 취소됩니다. 정말로 작업취소하시겠습니까?
            Util.MessageConfirm("SFU4421", (result) =>
            {
                if (result == MessageBoxResult.OK)
                {
                    CencelProcess();
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
        /// 작업취소
        /// </summary>
        private void CencelProcess()
        {
            try
            {
                ShowLoadingIndicator();

                const string bizRuleName = "BR_PRD_REG_CANCEL_START_PROD_FV";

                DataTable inTable = new DataTable();
                inTable.Columns.Add("SRCTYPE", typeof(string));
                inTable.Columns.Add("IFMODE", typeof(string));
                inTable.Columns.Add("EQPTID", typeof(string));
                inTable.Columns.Add("CTNR_ID", typeof(string));
                inTable.Columns.Add("USERID", typeof(string));

                DataRow newRow = inTable.NewRow();
                newRow["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                newRow["IFMODE"] = IFMODE.IFMODE_OFF;
                newRow["EQPTID"] = _equipmentCode;
                newRow["CTNR_ID"] = Util.NVC(DataTableConverter.GetValue(dgCart.Rows[0].DataItem, "CTNR_ID"));
                newRow["USERID"] = LoginInfo.USERID;
                inTable.Rows.Add(newRow);

                new ClientProxy().ExecuteService(bizRuleName, "INDATA", null, inTable, (bizResult, bizException) =>
                {
                    HiddenLoadingIndicator();

                    if (bizException != null)
                    {
                        Util.MessageException(bizException);
                        return;
                    }

                    ////Util.AlertInfo("정상 처리 되었습니다.");
                    //Util.MessageInfo("SFU1889");

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
        private bool ValidateCancel()
        {
            if (!CommonVerify.HasDataGridRow(dgCart))
            {
                // 대차 정보가 없습니다.
                Util.MessageValidation("SFU4365");
                return false;
            }

            if (Util.NVC_Int(DataTableConverter.GetValue(dgCart.Rows[0].DataItem, "END_CNT")) !=0)
            {
                // 완성LOT이 존재하여 작업취소가 불가합니다.
                Util.MessageValidation("SFU4422");
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
