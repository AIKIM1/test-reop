/*************************************************************************************
 Created Date : 2018.09.20
      Creator : 오화백
   Decription : 포트상태 변경 팝업
--------------------------------------------------------------------------------------
 [Change History]
  2018.09.20  DEVELOPER : Initial Created.
    
**************************************************************************************/

using C1.WPF;
using C1.WPF.DataGrid;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using System;
using System.Collections.Generic;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace LGC.GMES.MES.MCS001
{
    /// <summary>
    /// MCS001_002_CHANGE_PORT.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class MCS001_002_CHANGE_PORT : C1Window, IWorkArea
    {

        #region Initialize
        private string _ProtID = string.Empty;

        private bool _load = true;

        public MCS001_002_CHANGE_PORT()
        {
            InitializeComponent();
        }
      
        public IFrameOperation FrameOperation { get; set; }
        private void C1Window_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {
            if (_load)
            {
                object[] tmps = C1WindowExtension.GetParameters(this);
                if (tmps == null)
                    return;
                _ProtID = Util.NVC(tmps[0]);
                SetControl();
                this.InitCombo();
                _load = false;
            }
          
        }
        private void InitCombo()
        {
            CommonCombo _combo = new CommonCombo();
            String[] sFilter1 = { "MCS_INOUT_TYPE_CODE" };
            _combo.SetCombo(cboPortMode, CommonCombo.ComboStatus.SELECT, sFilter: sFilter1, sCase: "COMMCODE");
        }

        #endregion

        #region Event

        private void btnChange_Click(object sender, System.Windows.RoutedEventArgs e)
        {

            if (!ValidationReturn())
                return;

            // Port ID 상태를 변경하시겠습니까?
            Util.MessageConfirm("Port ID 상태를 변경하시겠습니까?", (result) =>
            {
                if (result == MessageBoxResult.OK)
                {
                    PortChange();
                }
            });


           
        }

        private void btnClose_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            this.DialogResult = MessageBoxResult.Cancel;
        }
        #endregion

        #region Method

        #region Port 변경
        /// <summary>
        /// Port 변경
        /// </summary>
        private void PortChange()
        {
            try
            {
                ShowLoadingIndicator();
                DataTable inTable = new DataTable();
                inTable.Columns.Add("PORT_ID", typeof(string));
                inTable.Columns.Add("UPDUSER", typeof(string));
                inTable.Columns.Add("INOUT_TYPE_CODE", typeof(string));

                // INDATA SET
                DataRow newRow = inTable.NewRow();
                newRow["PORT_ID"] = txtPortID.Text;
                newRow["UPDUSER"] = LoginInfo.USERID;
                newRow["INOUT_TYPE_CODE"] = cboPortMode.SelectedValue.ToString();
                inTable.Rows.Add(newRow);

                new ClientProxy().ExecuteService("BR_MCS_REG_PORT_INOUT", "INDATA", null, inTable, (result, searchException) =>
                {
                    try
                    {
                        HiddenLoadingIndicator();
                        if (searchException != null)
                        {
                            Util.MessageException(searchException);
                            return;
                        }
                        this.DialogResult = MessageBoxResult.OK;
                    }
                    catch (Exception ex)
                    {
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

        #region Control 셋팅
        private void SetControl()
        {
            txtPortID.Text = _ProtID;
        }
        #endregion

        #region Validation
        private bool ValidationReturn()
        {
            if (cboPortMode.SelectedIndex <= 0)
            {
                Util.MessageValidation("SFU5052");  // 포트모드를 선택하세요
                return false;
            }

            return true;
        }

        #endregion

        #region LoadingIndicator : ShowLoadingIndicator(),HiddenLoadingIndicator()

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
