/*************************************************************************************
 Created Date : 2017.05.31
      Creator : 이슬아D
   Decription : 전지 5MEGA-GMES 구축 - 특성실적관리 화면 - 작업시작 팝업
--------------------------------------------------------------------------------------
 [Change History]
  2017.05.31  이슬아D : 최초생성
  
**************************************************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using C1.WPF;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.ControlsLibrary;
using System.Data;
using C1.WPF.DataGrid;

namespace LGC.GMES.MES.BOX001{
  
    public partial class BOX001_101_RUNSTART : C1Window, IWorkArea
    {
        #region Declaration & Constructor 

        private string _PROCID = string.Empty; 

        public string BOXID
        {
            get;
            set;
        }

        public BOX001_101_RUNSTART()
        {
            InitializeComponent();
        }

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


        /// <summary>
        /// 화면내 버튼 권한 처리
        /// </summary>
        private void ApplyPermissions()
        {
            List<Button> listAuth = new List<Button>();
            //listAuth.Add(btnInReplace);
            Util.pageAuth(listAuth, FrameOperation.AUTHORITY);
        }

        /// <summary>
        /// 콤보박스 초기 설정
        /// </summary>
        private void InitCombo()
        {
            CommonCombo _combo = new CommonCombo();
            _combo.SetCombo(cboEquipment, CommonCombo.ComboStatus.SELECT, sFilter: new string[] { string.Empty, _PROCID }, sCase: "EQUIPMENT_BY_EQSGID_PROCID");
            _combo.SetCombo(cboInBox, CommonCombo.ComboStatus.SELECT, sFilter: new string[] { "INBOX_TYPE" }, sCase: "COMMCODE_WITHOUT_CODE");
            _combo.SetCombo(cboWorkType, CommonCombo.ComboStatus.SELECT, sFilter: new string[] { "PACK_WRK_TYPE_CODE1" }, sCase: "COMMCODE_WITHOUT_CODE");
        }

        /// <summary>
        /// 컨트롤 초기값 설정
        /// </summary>
        private void InitControl()
        {
            object[] tmps = C1WindowExtension.GetParameters(this);
                        
            cboEquipment.SelectedValue = Util.NVC(tmps[1]);
            //tmps[2] = string.Empty; //작업구분
            //tmps[3] = string.Empty; //inbox종류
            //tmps[4] = string.Empty; //모델lot
            //tmps[5] = string.Empty; //prjt
            //tmps[6] = string.Empty; //prodid
            //tmps[7] = string.Empty; // 출하처id
            //tmps[8] = string.Empty; // 출하처name
            txtShift.Tag = tmps[9]; // 작업조id
            txtShift.Text = Util.NVC(tmps[10]); // 작업조name
            txtWorker.Tag = tmps[11]; // 작업자id
            txtWorker.Text = Util.NVC(tmps[12]); // 작업자name
        }
        #endregion

        #region Event

        private void C1Window_Loaded(object sender, RoutedEventArgs e)
        {
            object[] tmps = C1WindowExtension.GetParameters(this);
            _PROCID = Util.NVC(tmps[0]);

            InitCombo();
            InitControl();
        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                TextRange textRange = new TextRange(txtHoldNote.Document.ContentStart, txtHoldNote.Document.ContentEnd);
                string sNote = textRange.Text.Substring(0, textRange.Text.LastIndexOf(System.Environment.NewLine));

                DataSet indataSet = new DataSet();
                DataTable inDataTable = indataSet.Tables.Add("INDATA");
                inDataTable.Columns.Add("PROCID");
                inDataTable.Columns.Add("EQPTID");
                inDataTable.Columns.Add("SHFT_ID");
                inDataTable.Columns.Add("USERID");
                inDataTable.Columns.Add("USERNAME");

                DataRow newRow = inDataTable.NewRow();
                newRow["PROCID"] = _PROCID;
                newRow["EQPTID"] = Util.NVC(cboEquipment.SelectedValue);
                newRow["SHFT_ID"] = txtShift.Tag;
                newRow["USERID"] = txtWorker.Tag;
                newRow["USERNAME"] = txtWorker.Text;

                inDataTable.Rows.Add(newRow);
                newRow = null;

                DataTable inBoxTable = indataSet.Tables.Add("INBOX");
                inBoxTable.Columns.Add("PACKDTTM");
                inBoxTable.Columns.Add("MDLLOT_ID");
                inBoxTable.Columns.Add("PRODID");
                inBoxTable.Columns.Add("PACK_WRK_TYPE_CODE");
                inBoxTable.Columns.Add("INBOX_TYPE");
                inBoxTable.Columns.Add("PKG_LOTID");
                inBoxTable.Columns.Add("SHIPTO_ID");
                inBoxTable.Columns.Add("PACK_NOTE");

                newRow = inBoxTable.NewRow();
                newRow["PACKDTTM"] = dtpDate.SelectedDateTime.ToString("yyyy-MM-dd HH:mm:ss");
                newRow["MDLLOT_ID"] = txtMDLLOT.Text;
                newRow["PRODID"] = btnPRODID.Tag;
                newRow["PACK_WRK_TYPE_CODE"] = cboWorkType.SelectedValue;
                newRow["INBOX_TYPE"] = cboInBox.SelectedValue;
                newRow["PKG_LOTID"] = txtLotID.Text;
                newRow["SHIPTO_ID"] = btnShip_To.Tag;
                newRow["PACK_NOTE"] = sNote;

                inBoxTable.Rows.Add(newRow);

                new ClientProxy().ExecuteService_Multi("BR_PRD_REG_START_INPALLET_FM", "INDATA,INBOX", "OUTDATA", (bizResult, bizException) =>
                {
                    try
                    {
                        if (bizException != null)
                        {
                            Util.MessageException(bizException);
                            return;
                        }

                        BOXID = Util.NVC(bizResult.Tables["OUTDATA"].Rows[0]["BOXID"]);
                       
                        Util.MessageInfo("SFU1275");    //정상 처리 되었습니다.

                        this.DialogResult = MessageBoxResult.OK;
                    }
                    catch (Exception ex)
                    {
                        Util.MessageException(ex);
                    }
                    finally
                    {

                    }
                }, indataSet);
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

        private void btnSave_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            loadingIndicator.Visibility = Visibility.Visible;
        }

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = MessageBoxResult.Cancel;
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
        #endregion

        #region Mehod

        #endregion

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
    }
}
