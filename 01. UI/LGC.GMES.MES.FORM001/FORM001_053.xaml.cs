/*************************************************************************************
 Created Date : 2017.09.15
      Creator : INS 김동일K
   Decription : 전지 5MEGA-GMES 구축 - TCO 공정진척 화면
--------------------------------------------------------------------------------------
 [Change History]
  2017.09.15  INS 김동일K : Initial Created.

**************************************************************************************/

using C1.WPF;
using LGC.GMES.MES.CMM001;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.CMM001.Popup;
using LGC.GMES.MES.CMM001.UserControls;
using LGC.GMES.MES.Common;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace LGC.GMES.MES.FORM001
{
    /// <summary>
    /// FORM001_053.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class FORM001_053 : UserControl, IWorkArea
    {   
        #region Declaration & Constructor        
        private Util _Util = new Util();
        private BizDataSet _Biz = new BizDataSet();

        private UC_WORKORDER_LINE winWorkOrder = new UC_WORKORDER_LINE();

        #region Popup 처리 로직 변경

        #endregion
        #endregion

        #region Initialize        
        /// <summary>
        /// Frame과 상호작용하기 위한 객체
        /// </summary>
        public IFrameOperation FrameOperation
        {
            get;
            set;
        }

        public FORM001_053()
        {
            InitializeComponent();
        }

        private void InitCombo()
        {
            CommonCombo _combo = new CommonCombo();

            String[] sFilter = { LoginInfo.CFG_AREA_ID };
            C1ComboBox[] cboLineChild = { cboEquipment };
            _combo.SetCombo(cboEquipmentSegment, CommonCombo.ComboStatus.SELECT, cbChild: cboLineChild, sFilter: sFilter);
            
        }

        #endregion

        #region Event

        #region [Main Window]

        private void UserControl_Initialized(object sender, EventArgs e)
        {
            InitCombo();
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            #region Popup 처리 로직 변경

            #endregion
            
            ApplyPermissions();

            SetWorkOrderWindow();            
        }
                
        #endregion

        #region [작업대상]
        
        #endregion

        #region [실적확인]
        
        #endregion

        #region [Tabs]

        #endregion

        #endregion

        #region Mehod

        #region [BizCall]
        
        #endregion

        #region [Validation]
        
        #endregion

        #region [PopUp Event]
        
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

        private void ApplyPermissions()
        {
            List<Button> listAuth = new List<Button>();
            listAuth.Add(btnRunStart);
            listAuth.Add(btnRunCancel);
            listAuth.Add(btnConfirm);

            listAuth.Add(btnSaveFaulty);

            Util.pageAuth(listAuth, FrameOperation.AUTHORITY);
        }

        private void SetWorkOrderWindow()
        {
            if (grdWorkOrder.Children.Count == 0)
            {
                winWorkOrder.FrameOperation = FrameOperation;

                winWorkOrder._UCParent = this;
                grdWorkOrder.Children.Add(winWorkOrder);
            }
        }

        private void GetWorkOrder()
        {
            if (winWorkOrder == null)
                return;

            winWorkOrder.EQPTSEGMENT = cboEquipmentSegment.SelectedValue.ToString();
            winWorkOrder.EQPTID = cboEquipment.SelectedValue.ToString();
            winWorkOrder.PROCID = Process.NOTCHING;

            winWorkOrder.GetWorkOrder();
        }

        public bool GetSearchConditions(out string sProc, out string sEqsg, out string sEqpt)
        {
            try
            {
                sProc = Process.NOTCHING;
                sEqsg = cboEquipmentSegment.SelectedIndex >= 0 ? cboEquipmentSegment.SelectedValue.ToString() : "";
                sEqpt = cboEquipment.SelectedIndex >= 0 ? cboEquipment.SelectedValue.ToString() : "";

                return true;
            }
            catch (Exception ex)
            {
                sProc = "";
                sEqsg = "";
                sEqpt = "";
                return false;
                throw ex;
            }

        }
                
        public bool ClearControls()
        {
            bool bRet = false;

            try
            {
                Util.gridClear(dgProductLot);
                

                bRet = true;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                bRet = false;
            }
            return bRet;
        }

        #endregion

        #endregion

        private void btnExtra_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
        {

        }

        private void btnRunStart_Click(object sender, RoutedEventArgs e)
        {

        }

        private void btnRunCancel_Click(object sender, RoutedEventArgs e)
        {

        }

        private void btnRunComplete_Click(object sender, RoutedEventArgs e)
        {

        }

        private void btnRunCompleteCancel_Click(object sender, RoutedEventArgs e)
        {

        }

        private void btnConfirm_Click(object sender, RoutedEventArgs e)
        {

        }

        private void cboEquipment_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {

        }

        private void dgProductLot_CurrentCellChanged(object sender, C1.WPF.DataGrid.DataGridCellEventArgs e)
        {

        }

        private void btnSaveFaulty_Click(object sender, RoutedEventArgs e)
        {

        }

        private void btnSearchEqpFaulty_Click(object sender, RoutedEventArgs e)
        {

        }

        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {

        }

        private void btnShift_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}
