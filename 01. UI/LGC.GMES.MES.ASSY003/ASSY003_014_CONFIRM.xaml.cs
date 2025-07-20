/*************************************************************************************
 Created Date : 2017.12.07
      Creator : CNS 고현영S
   Decription : 전지 5MEGA-GMES 구축 - C생산 관리 - Folding BOX 재공/출고 - 출고 팝업
--------------------------------------------------------------------------------------
 [Change History]
  2017.12.07  CNS 고현영S : 생성
**************************************************************************************/

using C1.WPF;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace LGC.GMES.MES.ASSY003
{
    /// <summary>
    /// ASSY003_014_HIST_DETAIL.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class ASSY003_014_CONFIRM : C1Window, IWorkArea
    {
        #region Declaration & Constructor 

        private string _EqsgId = string.Empty;
        private string _EqsgName = string.Empty;
        //private string _procID = string.Empty;
        private string _mkType = string.Empty;
        private decimal _Qty = 0;
        private List<string> _OutLotList = null;

        private BizDataSet _Biz = new BizDataSet();
        private Util _Util = new Util();

        Report_CProd_Out rptCProdOut;

        public string EQSGID
        {
            get { return _EqsgId; }
        }

        #endregion

        #region Initialize 
        /// <summary>
        ///  Frame과 상호작용하기 위한 객체
        /// </summary>
        public IFrameOperation FrameOperation
        {
            get;
            set;
        }

        public ASSY003_014_CONFIRM()
        {
            InitializeComponent();
        }

        #endregion

        #region Event
        private void C1Window_Initialized(object sender, EventArgs e)
        {
            try
            {

            }
            catch ( Exception ex )
            {
                Util.MessageException(ex);
            }
        }

        private void C1Window_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                object[] tmps = C1WindowExtension.GetParameters(this);

                if ( tmps != null && tmps.Length >= 5 )
                {
                    _EqsgId = Util.NVC(tmps[0]);
                    _EqsgName = Util.NVC(tmps[1]);
                    _Qty = Util.NVC_Decimal(tmps[2]);
                    _OutLotList = (List<string>)tmps[3];
                    //_procID = "";
                    _mkType = Util.NVC(tmps[4]);
                }
                else
                {
                    _EqsgId = "";
                    _EqsgName = "";
                    _Qty = 0;
                    _OutLotList = null;
                    //_procID = "";
                    _mkType = "";
                }


                CommonCombo _combo = new CommonCombo();

                //라인 Combo
                String[] sFilter1 = { LoginInfo.CFG_AREA_ID, null, Process.PACKAGING };
                _combo.SetCombo(cboLine, CommonCombo.ComboStatus.SELECT, sCase: "cboEquipmentSegmentAssy", sFilter: sFilter1);

                if (cboLine != null && cboLine.Items.Count > 0)
                    cboLine.SelectedValue = _EqsgId;

                //if (_mkType.Equals("P"))    // 패키지 이면 라인 변경 없음.
                //    cboLine.IsEnabled = false;

                LoadTextBox();

                ApplyPermissions();
            }
            catch ( Exception ex )
            {
                Util.MessageException(ex);
            }
        }

        private void C1Window_Closed(object sender, EventArgs e)
        {
            if (this.DialogResult == MessageBoxResult.OK)
            {
                //if (rptCProdOut != null)
                //    rptCProdOut = null;

                //rptCProdOut = new Report_CProd_Out();
                //rptCProdOut.FrameOperation = FrameOperation;

                //if (rptCProdOut != null)
                //{
                //    object[] Parameters = new object[2];

                //    Parameters[0] = "CProd_OutBoxList";
                //    Parameters[1] = _OutLotList;

                //    C1WindowExtension.SetParameters(rptCProdOut, Parameters);

                //    this.Dispatcher.BeginInvoke(new Action(() => rptCProdOut.ShowModal()));
                //}

            }
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = MessageBoxResult.Cancel;
        }

        private void btnOut_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                btnOut.IsEnabled = false;
                ShowLoadingIndicator();

                _EqsgId = Util.NVC(cboLine.SelectedValue);

                this.DialogResult = MessageBoxResult.OK;
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

      
        private void LoadTextBox()
        {
            //tbxLine.Text = _EqsgName;
            tbxQty.Text = string.Format("{0:#,##0}", _Qty);
        }

        #endregion

        #region Method

        #region [BizCall]

        #endregion

        #region [Validation]

        #endregion

        #region [Func]

        private void ApplyPermissions()
        {
            List<Button> listAuth = new List<Button>();

            Util.pageAuth(listAuth, FrameOperation.AUTHORITY);
        }

        private void ShowLoadingIndicator()
        {
            if ( loadingIndicator != null )
                loadingIndicator.Visibility = Visibility.Visible;
        }

        private void HiddenLoadingIndicator()
        {
            if ( loadingIndicator != null )
                loadingIndicator.Visibility = Visibility.Collapsed;
        }

        #endregion

        #endregion

    }
}
