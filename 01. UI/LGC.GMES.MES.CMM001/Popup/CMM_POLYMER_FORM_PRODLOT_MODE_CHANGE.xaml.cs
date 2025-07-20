/*************************************************************************************
 Created Date : 2018.04.28
      Creator : 정문교
   Decription : 대차 재공처리 유형 변경
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
    /// CMM_POLYMER_FORM_PRODLOT_MODE_CHANGE.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class CMM_POLYMER_FORM_PRODLOT_MODE_CHANGE : C1Window, IWorkArea
    {
        #region Declaration & Constructor

        private string _procID = string.Empty;              // 공정코드
        private string _prodLotID = string.Empty;           // 생산Lot
        private string _ctnrTypeCode = string.Empty;        // 작업모드

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

        public CMM_POLYMER_FORM_PRODLOT_MODE_CHANGE()
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
                SetParameters();
                SetControl();
                _load = false;
            }

        }

        private void InitializeUserControls()
        {
        }
        private void SetParameters()
        {
            object[] tmps = C1WindowExtension.GetParameters(this);

            _procID = tmps[0] as string;
            _prodLotID = tmps[1] as string;
            _ctnrTypeCode = tmps[2] as string;

            txtProdLotID.Text = _prodLotID;
        }
        private void SetControl()
        {
            if (_ctnrTypeCode.Equals("B"))
            {
                rdoBeforeBoxMode.IsChecked = true;
            }
            else
            {
                rdoBeforeCartMode.IsChecked = true;
            }
        }
        #endregion

        private void txtWorker_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == System.Windows.Input.Key.Enter)
            {
                GetUser(txtWorker.Text.Trim());
            }
        }

        #region [변경]
        private void btnChange_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidationChange())
                return;

            // 변경하시겠습니까?
            Util.MessageConfirm("SFU2875", (result) =>
            {
                if (result == MessageBoxResult.OK)
                {
                    ChangeProcess();
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
        /// 작업자
        /// </summary>
        private void GetUser(string sUserId)
        {
            try
            {
                DataTable inTable = new DataTable();
                inTable.Columns.Add("LANGID", typeof(string));
                inTable.Columns.Add("USERID", typeof(string));

                DataRow row = inTable.NewRow();
                row["LANGID"] = LoginInfo.LANGID;
                row["USERID"] = sUserId;
                inTable.Rows.Add(row);

                ShowLoadingIndicator();

                new ClientProxy().ExecuteService("DA_BAS_SEL_PERSON_BY_ID", "INDATA", "OUTDATA", inTable, (bizResult, bizException) =>
                {
                    try
                    {
                        HiddenLoadingIndicator();

                        if (bizException != null)
                        {
                            Util.MessageException(bizException);
                            return;
                        }

                        if (bizResult != null && bizResult.Rows.Count > 0)
                        {
                            txtUserName.Text = (string)bizResult.Rows[0]["USERNAME"];
                            txtUserId.Text = (string)bizResult.Rows[0]["USERID"];
                        }
                        else
                        {
                            // 사용자 정보가 없습니다.
                            Util.MessageValidation("SFU1592");

                            txtUserName.Text = string.Empty;
                            txtUserId.Text = string.Empty;
                        }
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
        /// 변경
        /// </summary>
        private void ChangeProcess()
        {
            try
            {
                ShowLoadingIndicator();

                // DATA SET
                DataTable inTable = new DataTable();
                inTable.Columns.Add("PROD_LOTID", typeof(string));
                inTable.Columns.Add("CTNR_TYPE_CODE", typeof(string));
                inTable.Columns.Add("USERID", typeof(string));
                inTable.Columns.Add("PROCID", typeof(string));

                DataRow newRow = inTable.NewRow();
                newRow["PROD_LOTID"] = _prodLotID;
                newRow["CTNR_TYPE_CODE"] = (bool)rdoNewCartMode.IsChecked ? "CART" : "B";
                newRow["USERID"] = txtUserId.Text;
                newRow["PROCID"] = _procID;
                inTable.Rows.Add(newRow);

                new ClientProxy().ExecuteService("BR_PRD_CHG_CTNR_TYPE_CODE", "INDATA", null, inTable, (bizResult, bizException) =>
                {
                    try
                    {
                        HiddenLoadingIndicator();

                        if (bizException != null)
                        {
                            Util.MessageException(bizException);
                            return;
                        }

                        //Util.AlertInfo("정상 처리 되었습니다.");
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

        #region [Func]

        private bool ValidationChange()
        {
            if (_ctnrTypeCode.Equals("B") && (bool)rdoNewBoxMode.IsChecked)
            {
                // 변경내용이 없습니다.
                Util.MessageValidation("SFU1226");
                return false;
            }

            if (_ctnrTypeCode.Equals("CART") && (bool)rdoNewCartMode.IsChecked)
            {
                // 변경내용이 없습니다.
                Util.MessageValidation("SFU1226");
                return false;
            }

            if (!(bool)rdoNewBoxMode.IsChecked && !(bool)rdoNewCartMode.IsChecked)
            {
                // 변경내용이 없습니다.
                Util.MessageValidation("SFU1226");
                return false;
            }

            if (string.IsNullOrWhiteSpace(txtUserId.Text))
            {
                // 작업자 정보를 입력하세요.
                Util.MessageValidation("SFU4201");
                return false;
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