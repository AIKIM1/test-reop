/*************************************************************************************
 Created Date : 2017.12.14
      Creator : 이진선
   Decription : VD QA 검사 이력조회
--------------------------------------------------------------------------------------
 [Change History]
  2017.10.10  이진선S : Initial Created.
  2019.04.29  정문교  : 폴란드3동 대응 Carrier ID(CSTID) 조회조건, 조회칼럼 추가



 
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
using System.Windows.Input;
using System.Windows.Media;

namespace LGC.GMES.MES.ASSY001
{
    public partial class ASSY001_035 : UserControl, IWorkArea
    {

        CommonCombo combo = new CommonCombo();
        Util util = new Util();
      

        public IFrameOperation FrameOperation
        {
            get;
            set;
        }

        public ASSY001_035()
        {
            InitializeComponent();
        }

        private void UserControl_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {
            Loaded -= UserControl_Loaded;
            ApplyPermissions();

            initcombo();
        }
     
     

        private void txtLOTID_KeyUp(object sender, KeyEventArgs e)
        {

            if (e.Key == Key.Enter)
            {
                SearchData();
            }
        }

        private void txtPRLOTID_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                SearchData();
            }

        }
        private void txtProdId_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                SearchData();
            }
        }
        private void txtPrjtName_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                SearchData();
            }
        }
        private void txtCstID_KeyUp(object sender, KeyEventArgs e)
        {

            if (e.Key == Key.Enter)
            {
                SearchData();
            }
        }

        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            if (sender == null)
                return;

            SearchData();
        }

        private void cboVDArea_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            //라인
            string[] sFilter2 = { Process.VD_LMN, Convert.ToString(cboVDArea.SelectedValue) };
            combo.SetCombo(cboVDEquipmentSegment, CommonCombo.ComboStatus.NONE, sFilter: sFilter2);
        }
        private void cboVDEquipmentSegment_SelectedValueChanged(object sender, C1.WPF.PropertyChangedEventArgs<object> e)
        {
            //공정
            String[] sFilter3 = { cboVDEquipmentSegment.SelectedValue.ToString() };
            combo.SetCombo(cboVDProcess, CommonCombo.ComboStatus.NONE, sFilter: sFilter3);
        }
        private void cboVDProcess_SelectedValueChanged(object sender, C1.WPF.PropertyChangedEventArgs<object> e)
        {
            //설비
            String[] sFilter4 = { cboVDProcess.SelectedValue.ToString(), cboVDEquipmentSegment.SelectedValue.ToString() };
            combo.SetCombo(cboVDEquipment, CommonCombo.ComboStatus.ALL, sFilter: sFilter4);
        }

        private void dgLotInfo_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (sender == null)
                return;

            if (dgLotInfo.CurrentCell.Column.Name.Equals("JUDG_NAME"))
            {
                //팝업 띄우기
                ASSY001_035_LOTINFO wndLot = new ASSY001_035_LOTINFO();
                wndLot.FrameOperation = FrameOperation;

                if (wndLot != null)
                {
                    object[] Parameters = new object[6];

                    Parameters[0] = Util.NVC(DataTableConverter.GetValue(dgLotInfo.CurrentCell.Row.DataItem, "LOTID_RT"));
                    Parameters[1] = Util.NVC(DataTableConverter.GetValue(dgLotInfo.CurrentCell.Row.DataItem, "EQPT_BTCH_WRK_NO"));
                    Parameters[2] = Util.NVC(DataTableConverter.GetValue(dgLotInfo.CurrentCell.Row.DataItem, "VD_QA_INSP_COND_CODE"));
                    Parameters[3] = Util.NVC(DataTableConverter.GetValue(dgLotInfo.CurrentCell.Row.DataItem, "EQPTID"));
                    Parameters[4] = Util.NVC(DataTableConverter.GetValue(dgLotInfo.CurrentCell.Row.DataItem, "QATYPE"));
                    Parameters[5] = Util.NVC(DataTableConverter.GetValue(dgLotInfo.CurrentCell.Row.DataItem, "REWORK_TYPE"));

                    C1WindowExtension.SetParameters(wndLot, Parameters);

                    // wndLot.Closed += new EventHandler(wndLot_Closed);

                    // 팝업 화면 숨겨지는 문제 수정.
                    this.Dispatcher.BeginInvoke(new Action(() => wndLot.ShowModal()));
                    wndLot.BringToFront();

                }
            }
            
        }

        private void wndLot_Closed(object sender, EventArgs e)
        {
            ASSY001_035_LOTINFO window = sender as ASSY001_035_LOTINFO;
            if (window.DialogResult == MessageBoxResult.OK)
            {
                
            }
        }

        private void dgLotInfo_LoadedCellPresenter(object sender, DataGridCellEventArgs e)
        {
            if (sender == null)
                return;

            e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Black);

            if (e.Cell.Column.Name.Equals("JUDG_NAME"))
            {
                e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Blue);
            }
        }


        #region[Method]

        private void initcombo()
        {
            //동
            string[] sFilter1 = { null, LoginInfo.CFG_AREA_ID };
            combo.SetCombo(cboVDArea, CommonCombo.ComboStatus.NONE, sFilter: sFilter1);

            //극성
            string[] sFilter2 = { "ELEC_TYPE" };
            combo.SetCombo(cboElec, CommonCombo.ComboStatus.ALL, sFilter: sFilter2, sCase: "COMMCODE");


        }

        private void ApplyPermissions()
        {
            List<Button> listAuth = new List<Button>();

            Util.pageAuth(listAuth, FrameOperation.AUTHORITY);
        }

        private void SearchData()
        {
            try
            {
                DataTable dt = new DataTable();
                dt.Columns.Add("LANGID", typeof(string));
                dt.Columns.Add("AREAID", typeof(string));
                dt.Columns.Add("EQSGID", typeof(string));
                dt.Columns.Add("EQPTID", typeof(string));
                dt.Columns.Add("FROM_DATE", typeof(string));
                dt.Columns.Add("TO_DATE", typeof(string));
                dt.Columns.Add("PRODID", typeof(string));
                dt.Columns.Add("PRJT_NAME", typeof(string));
                dt.Columns.Add("LOTID", typeof(string));
                dt.Columns.Add("PR_LOTID", typeof(string));
                dt.Columns.Add("PRDT_CLSS_CODE", typeof(string));
                dt.Columns.Add("CSTID", typeof(string));

                DataRow dr = dt.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["AREAID"] = Convert.ToString(cboVDArea.SelectedValue);
                dr["EQSGID"] = Convert.ToString(cboVDEquipmentSegment.SelectedValue);
                dr["EQPTID"] = Convert.ToString(cboVDEquipment.SelectedValue).Equals("") ? null : Convert.ToString(cboVDEquipment.SelectedValue);
                dr["FROM_DATE"] = dtpDateFrom.SelectedDateTime.ToShortDateString();
                dr["TO_DATE"] = dtpDateTo.SelectedDateTime.ToShortDateString();
                dr["PRODID"] = txtProdId.Text.Equals("") ? null : txtProdId.Text;
                dr["PRJT_NAME"] = txtPrjtName.Text.Equals("") ? null : txtPrjtName.Text;
                dr["LOTID"] = txtLotId.Text.Equals("") ? null : txtLotId.Text;
                dr["PR_LOTID"] = txtPRLOTID.Text.Equals("") ? null : txtPRLOTID.Text;
                dr["PRDT_CLSS_CODE"] = Convert.ToString(cboElec.SelectedValue).Equals("") ? null : Convert.ToString(cboElec.SelectedValue);
                dr["CSTID"] = txtCstId.Text.Equals("") ? null : txtCstId.Text;

                dt.Rows.Add(dr);


                loadingIndicator.Visibility = Visibility.Visible;

                new ClientProxy().ExecuteService("DA_PRD_SEL_TB_SFC_VD_QA_INSP_HIST_QA_HIST", "RQST", "RSLT", dt, (bizResult, bizException) =>
                 {
                     try
                     {
                         if (bizException != null)
                         {
                             Util.MessageException(bizException);
                             return;
                         }

                         Util.GridSetData(dgLotInfo, bizResult, FrameOperation, true);
                     }
                     catch (Exception ex)
                     {
                         Util.MessageException(ex);
                     }
                     finally
                     {
                         loadingIndicator.Visibility = Visibility.Collapsed;
                         txtLotId.Text = "";
                         txtPRLOTID.Text = "";
                         txtProdId.Text = "";
                         txtPrjtName.Text = "";
                     }
                 });

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }

        }










        #endregion

      
    }
}
