/*************************************************************************************
 Created Date : 2017.05.22
      Creator : 이영준S
   Decription : 전지 5MEGA-GMES 구축 - 1차 포장 구성 - 작업시작 팝업
--------------------------------------------------------------------------------------
 [Change History]


**************************************************************************************/

using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Data;

using C1.WPF;
using C1.WPF.DataGrid;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.ControlsLibrary;
using System.ComponentModel;
using System.Threading;

namespace LGC.GMES.MES.BOX001
{
    /// <summary>
    /// BOX001_027_RUNSTART.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class BOX001_027_RUNSTART : C1Window, IWorkArea
    {
        string _PROCID = Process.CELL_BOXING;
        string _EQSGID = string.Empty;
        string _EQPTID = string.Empty;
        string sUSERID = string.Empty;
        string sSHFTID = string.Empty;

        public IFrameOperation FrameOperation
        {
            get;
            set;
        }

        public BOX001_027_RUNSTART()
        {
            InitializeComponent();
        }

        #region Initialize

        private void C1Window_Loaded(object sender, RoutedEventArgs e)
        {
            object[] tmps = C1WindowExtension.GetParameters(this);
            _PROCID = Util.NVC(tmps[0]);
            _EQSGID = Util.NVC(tmps[1]);
            _EQPTID = Util.NVC(tmps[2]);

            InitCombo();
            InitControl();
        }

        /// <summary>
        /// 콤보박스 초기 설정
        /// </summary>
        private void InitCombo()
        {
            CommonCombo _combo = new CommonCombo();
            _combo.SetCombo(cboEquipment, CommonCombo.ComboStatus.SELECT, sFilter: new string[] { string.Empty, _PROCID }, sCase: "EQUIPMENT_BY_EQSGID_PROCID");
            _combo.SetCombo(cboExpDomType, CommonCombo.ComboStatus.SELECT, sFilter: new string[] { "EXP_DOM_TYPE_CODE" }, sCase: "COMMCODE_WITHOUT_CODE");
        }

        /// <summary>
        /// 컨트롤 초기값 설정
        /// </summary>
        private void InitControl()
        {
            object[] tmps = C1WindowExtension.GetParameters(this);
            cboEquipment.SelectedValue = Util.NVC(tmps[2]);
            txtShift.Tag = tmps[3]; // 작업조id
            txtShift.Text = Util.NVC(tmps[4]); // 작업조name
            txtWorker.Tag = tmps[5]; // 작업자id
            txtWorker.Text = Util.NVC(tmps[6]); // 작업자name
        }
        #endregion

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {

            Util.MessageConfirm("SFU1240", (result) =>
            {
                if (result == MessageBoxResult.OK)
                {
                    RunStart();
                }
            });
        }

        private void RunStart()
        {
            try
            {
                DataSet inDataSet = new DataSet(); 

                DataTable inDataTable = inDataSet.Tables.Add("INDATA");
                inDataTable.Columns.Add("EQSGID");
                inDataTable.Columns.Add("EQPTID");
                inDataTable.Columns.Add("USERID");

                DataTable inBoxTable = inDataSet.Tables.Add("INBOX");
                inBoxTable.Columns.Add("PRODID");
                inBoxTable.Columns.Add("PKG_LOTID");
                inBoxTable.Columns.Add("EXP_DOM_TYPE_CODE");
                inBoxTable.Columns.Add("TOTAL_QTY");
                inBoxTable.Columns.Add("SHIFT_ID");

                DataTable inTable = inDataSet.Tables["INDATA"];
                DataRow newRow = inDataTable.NewRow();
                newRow["EQSGID"] = _EQSGID;
                newRow["EQPTID"] = _EQPTID;
                newRow["USERID"] = txtWorker.Tag;
                inDataTable.Rows.Add(newRow);
                newRow = null;
                
                newRow = inBoxTable.NewRow();
                newRow["PRODID"] = btnPRODID.Tag;
                newRow["PKG_LOTID"] = txtLotID.Text;
                newRow["EXP_DOM_TYPE_CODE"] = cboExpDomType.SelectedValue;
                newRow["TOTAL_QTY"] = txtTotalQty.Value;
                newRow["SHIFT_ID"] = txtShift.Tag;
                inBoxTable.Rows.Add(newRow);
                new ClientProxy().ExecuteServiceSync_Multi("BR_PRD_REG_PACKING_PALLET_1ST_MP", "INDATA,INBOX", null, inDataSet);

                this.DialogResult = MessageBoxResult.OK;

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
            finally
            {
                loadingIndicator.Visibility = Visibility.Collapsed;
            }
        }
        
        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = MessageBoxResult.Cancel;
        }

        private void btnSave_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            loadingIndicator.Visibility = Visibility.Visible;
        }

        private void btnPRODID_Click(object sender, RoutedEventArgs e)
        {
            CMM001.Popup.CMM_MDLLOT popup = new CMM001.Popup.CMM_MDLLOT();
            popup.FrameOperation = this.FrameOperation;

            if (popup != null)
            {
                object[] Parameters = new object[2];
                Parameters[0] = LoginInfo.CFG_AREA_ID;
                Parameters[1] = txtMDLLOT.Text;
                C1WindowExtension.SetParameters(popup, Parameters);

                popup.Closed += new EventHandler(puProduct_Closed);

                //grdMain.Children.Add(popup);
                //popup.BringToFront();
                this.Dispatcher.BeginInvoke(new Action(() => popup.ShowModal()));
            }
        }

        private void puProduct_Closed(object sender, EventArgs e)
        {
            CMM001.Popup.CMM_MDLLOT popup = sender as CMM001.Popup.CMM_MDLLOT;
            if (popup.DialogResult == MessageBoxResult.OK)
            {
                txtMDLLOT.Text = popup.MDLLOT_ID;
                txtProjectName.Text = popup.PRJT_NAME;
                btnPRODID.Tag = popup.PRODID;
            }
            //this.grdMain.Children.Remove(popup);
        }


        private void btnShip_To_Click(object sender, RoutedEventArgs e)
        {
            CMM001.Popup.CMM_SHIP_TO popup = new CMM001.Popup.CMM_SHIP_TO();
            popup.FrameOperation = this.FrameOperation;

            if (popup != null)
            {
                object[] Parameters = new object[2];
                Parameters[0] = LoginInfo.CFG_SHOP_ID;
                C1WindowExtension.SetParameters(popup, Parameters);

                popup.Closed += new EventHandler(puShipTo_Closed);

                //grdMain.Children.Add(popup);
                //popup.BringToFront();
                this.Dispatcher.BeginInvoke(new Action(() => popup.ShowModal()));
            }
        }

        private void puShipTo_Closed(object sender, EventArgs e)
        {
            CMM001.Popup.CMM_SHIP_TO popup = sender as CMM001.Popup.CMM_SHIP_TO;
            if (popup.DialogResult == MessageBoxResult.OK)
            {
                txtShip_To.Text = popup.SHIPTO_NAME;
                btnShip_To.Tag = popup.SHIPTO_ID;
            }
            //this.grdMain.Children.Remove(popup);
        }
        private void btnShift_Click(object sender, RoutedEventArgs e)
        {
            CMM001.Popup.CMM_SHIFT_USER2 wndPopup = new CMM001.Popup.CMM_SHIFT_USER2();
            wndPopup.FrameOperation = this.FrameOperation;

            if (wndPopup != null)
            {
                object[] Parameters = new object[8];
                Parameters[0] = LoginInfo.CFG_SHOP_ID;
                Parameters[1] = LoginInfo.CFG_AREA_ID;
                Parameters[2] = LoginInfo.CFG_EQSG_ID;
                Parameters[3] = _PROCID;
                Parameters[4] = Util.NVC(txtShift.Tag);
                Parameters[5] = Util.NVC(txtWorker.Tag);
                Parameters[6] = Util.NVC(cboEquipment.SelectedValue);
                Parameters[7] = "Y"; // 저장 플로그 "Y" 일때만 저장.

                C1WindowExtension.SetParameters(wndPopup, Parameters);

                wndPopup.Closed += new EventHandler(wndShift_Closed);

                this.Dispatcher.BeginInvoke(new Action(() => wndPopup.ShowModal()));

                //grdMain.Children.Add(wndPopup);
                //wndPopup.BringToFront();
            }
        }

        private void wndShift_Closed(object sender, EventArgs e)
        {
            CMM001.Popup.CMM_SHIFT_USER2 wndPopup = sender as CMM001.Popup.CMM_SHIFT_USER2;

            if (wndPopup.DialogResult == MessageBoxResult.OK)
            {
                txtShift.Text = Util.NVC(wndPopup.SHIFTNAME);
                txtShift.Tag = Util.NVC(wndPopup.SHIFTCODE);
                txtWorker.Text = Util.NVC(wndPopup.USERNAME);
                txtWorker.Tag = Util.NVC(wndPopup.USERID);
            }
            // this.grdMain.Children.Remove(wndPopup);
        }
    }
}
