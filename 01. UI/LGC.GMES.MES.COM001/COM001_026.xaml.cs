/*************************************************************************************
 Created Date : 2016.06.16
      Creator : 
   Decription : 
--------------------------------------------------------------------------------------
 [Change History]
  2016.06.16  DEVELOPER : Initial Created.
  2021.05.11  정재홍 : CSR[C20210409-000075] 공정 멀티 선택 및 LOT 컬럼 필터링 추가
  2022.03.24  서용호 : 슬라이딩 측정값 컬럼 추가
  2023.12.18  김도형 : [E20231123-001377] MES TBOX 출고 화먄 improvement ( SKID_NOTE추가)
  2023.12.21  김도형 : [E20231123-001377] MES TBOX 출고 화먄 improvement ( SKID_NOTE ROW병합기능제거)

**************************************************************************************/

using C1.WPF;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using System;
using System.Collections.Generic;
using System.Data;
using System.Windows;
using System.Windows.Controls;

namespace LGC.GMES.MES.COM001
{
    public partial class COM001_026 : UserControl, IWorkArea
    {
        #region Declaration & Constructor  
        public COM001_026()
        {
            InitializeComponent();
        }
        public IFrameOperation FrameOperation
        {
            get;
            set;
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            List<Button> listAuth = new List<Button>();
            //listAuth.Add(btnInReplace);

            Util.pageAuth(listAuth, FrameOperation.AUTHORITY);

            InitCombo();

            ColVisible();
        }

        #endregion

        #region Initialize
        private void InitCombo()
        {
            CommonCombo _combo = new CommonCombo();

            //동
            C1ComboBox[] cboAreaChild = { cboEquipmentSegment, cboElecWareHouse };
            _combo.SetCombo(cboArea, CommonCombo.ComboStatus.NONE, cbChild: cboAreaChild, sCase: "AREA");

            //라인
            C1ComboBox[] cboEquipmentSegmentParent = { cboArea };
            C1ComboBox[] cboEquipmentSegmentChild = { cboProcess };
            _combo.SetCombo(cboEquipmentSegment, CommonCombo.ComboStatus.SELECT, cbChild: cboEquipmentSegmentChild,                                                               cbParent: cboEquipmentSegmentParent);
            //_combo.SetCombo(cboEquipmentSegment, CommonCombo.ComboStatus.SELECT, cbParent: cboEquipmentSegmentParent);

            //공정
            C1ComboBox[] cbProcessParent = { cboEquipmentSegment };
            _combo.SetCombo(cboProcess, CommonCombo.ComboStatus.ALL, cbParent: cbProcessParent, sCase: "PROCESS");

            // 창고명
            C1ComboBox[] cboWareHouseParent = { cboArea };
            C1ComboBox[] cboareHouseChild = { cboElecRack };
            _combo.SetCombo(cboElecWareHouse, CommonCombo.ComboStatus.ALL, cbChild: cboareHouseChild, cbParent: cboWareHouseParent);

            // RACK
            C1ComboBox[] cboRackParent = { cboElecWareHouse };
            _combo.SetCombo(cboElecRack, CommonCombo.ComboStatus.ALL, cbParent: cboRackParent);

            // 양/음극
            String[] sFilter1 = { "", "ELEC_TYPE" };
            _combo.SetCombo(cboElecType, CommonCombo.ComboStatus.ALL, sFilter: sFilter1, sCase: "COMMCODES");

            // Lot Type  --> Roll/Pancake 통합으로 구분 필요없음
            //_combo.SetCombo(cboElecLotType, CommonCombo.ComboStatus.ALL);

            //// Lot Status
            String[] sFilter2 = { "", "HOLD_STATUS" };
            _combo.SetCombo(cboLotStatus, CommonCombo.ComboStatus.ALL, sFilter: sFilter2, sCase: "COMMCODES");

        }

        #endregion

        #region Event
        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            if(chkReturnLotSearch.IsChecked == true) { GetElecStockReturnLot(); }
            else { GetElecStock(); }            
        }

        private void chkReturnLotSearch_Checked(object sender, RoutedEventArgs e)
        {
            if (chkReturnLotSearch.IsChecked == true)
            {
                dgElecStock.Columns["MOVE_RTN_TYPE_CODE"].Visibility = Visibility.Visible;
                dgElecStock.Columns["MOVE_NOTE"].Visibility = Visibility.Visible;
            }
            else
            {
                dgElecStock.Columns["MOVE_RTN_TYPE_CODE"].Visibility = Visibility.Collapsed;
                dgElecStock.Columns["MOVE_NOTE"].Visibility = Visibility.Collapsed;
            }
        }

        private void chkTagQty_Checked(object sender, RoutedEventArgs e)
        {
            dgElecStock.Columns["DFCT_TAG_QTY"].Visibility = Visibility.Visible;
        }

        private void chkTagQty_UnChecked(object sender, RoutedEventArgs e)
        {
            dgElecStock.Columns["DFCT_TAG_QTY"].Visibility = Visibility.Collapsed;
        }

        private void chkShipTime_Checked(object sender, RoutedEventArgs e)
        {

            dgElecStock.Columns["AVL_DAYS"].Visibility = Visibility.Visible;
        }

        private void chkShipTime_UnChecked(object sender, RoutedEventArgs e)
        {
            dgElecStock.Columns["AVL_DAYS"].Visibility = Visibility.Collapsed;
        }

        private void cboArea_SelectedIndexChanged(object sender, PropertyChangedEventArgs<int> e)
        {
            try
            {
                this.Dispatcher.BeginInvoke(new System.Action(() =>
                {
                    //SetcboProcess();
                }));
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        #endregion

        #region Method
        private void GetElecStock()
        {
            try
            {
                string sProc = cboProcess.SelectedValue.ToString();
                string sWh = cboElecWareHouse.SelectedValue.ToString();
                string sRack = cboElecRack.SelectedValue.ToString();
                string sModel = txtModel.Text;
                string sProd = txtProdCode.Text;
                string sPrdtClss = cboElecType.SelectedValue.ToString();
                string sLot = txtLotID.Text;
                string sPrjt = txtPrjtName.Text;

                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("SHOPID", typeof(string));
                RQSTDT.Columns.Add("AREAID", typeof(string));
                RQSTDT.Columns.Add("PROCID", typeof(string));
                RQSTDT.Columns.Add("WH_ID", typeof(string));
                RQSTDT.Columns.Add("RACK_ID", typeof(string));
                RQSTDT.Columns.Add("MODLID", typeof(string));
                RQSTDT.Columns.Add("PRODID", typeof(string));
                RQSTDT.Columns.Add("PRDT_CLSS_CODE", typeof(string));
                RQSTDT.Columns.Add("LOTID", typeof(string));
                RQSTDT.Columns.Add("HOLD_FLAG", typeof(string));
                RQSTDT.Columns.Add("VLD_DATE_OVER", typeof(string));
                RQSTDT.Columns.Add("PRJT_NAME", typeof(string));
                RQSTDT.Columns.Add("PACK_YN", typeof(string));
                RQSTDT.Columns.Add("PROC_YN", typeof(string));

                if (sProc == "")
                    sProc = null;
                if (sWh == "")
                    sWh = null;
                if (sRack == "")
                    sRack = null;
                if (sModel == "")
                    sModel = null;
                if (sProd == "")
                    sProd = null;
                if (sPrdtClss == "")
                    sPrdtClss = null;
                if (sLot == "")
                    sLot = null;
                //if (sHold == "")
                //    sHold = null;
                //if ((bool)chkHold.IsChecked)
                //{
                //    sHold = "Y";
                //}
                //else
                //{
                //    sHold = null;
                //}

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["SHOPID"] = LoginInfo.CFG_SHOP_ID.ToString();
                dr["AREAID"] = cboArea.SelectedValue.ToString();
                dr["PROCID"] = sProc;
                dr["WH_ID"] = sWh;
                dr["RACK_ID"] = sRack;
                dr["MODLID"] = sModel;
                dr["PRODID"] = sProd;
                dr["PRDT_CLSS_CODE"] = sPrdtClss;
                dr["LOTID"] = sLot;
                dr["HOLD_FLAG"] = cboLotStatus.SelectedValue.ToString() == ""? null: cboLotStatus.SelectedValue.ToString();
                dr["VLD_DATE_OVER"] = ((bool)chkVldDate.IsChecked) ? "Y" : null;
                dr["PRJT_NAME"] = sPrjt;
                dr["PACK_YN"] = ((bool)chkPackYn.IsChecked) ? "Y" : null;
                dr["PROC_YN"] = ((bool)chkProcWip.IsChecked) ? "Y" : "N";

                RQSTDT.Rows.Add(dr);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("BR_PRD_SEL_WH_WIP", "RQSTDT", "RSLTDT", RQSTDT);

                // dgElecStock.ItemsSource = DataTableConverter.Convert(dtRslt);
                Util.GridSetData(dgElecStock, dtRslt, FrameOperation, true);
                
                ColVisible();
                 
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }

        private void GetElecStockReturnLot()
        {
            try
            {
                string sProc = cboProcess.SelectedValue.ToString();
                string sWh = cboElecWareHouse.SelectedValue.ToString();
                string sRack = cboElecRack.SelectedValue.ToString();
                string sModel = txtModel.Text;
                string sProd = txtProdCode.Text;
                string sPrdtClss = cboElecType.SelectedValue.ToString();
                string sLot = txtLotID.Text;
                string sPrjt = txtPrjtName.Text;
                string sHold = cboLotStatus.SelectedValue.ToString();

                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("SHOPID", typeof(string));
                RQSTDT.Columns.Add("AREAID", typeof(string));
                RQSTDT.Columns.Add("PROCID", typeof(string));
                RQSTDT.Columns.Add("WH_ID", typeof(string));
                RQSTDT.Columns.Add("RACK_ID", typeof(string));
                RQSTDT.Columns.Add("MODLID", typeof(string));
                RQSTDT.Columns.Add("PRODID", typeof(string));
                RQSTDT.Columns.Add("PRDT_CLSS_CODE", typeof(string));
                RQSTDT.Columns.Add("LOTID", typeof(string));
                RQSTDT.Columns.Add("HOLD_FLAG", typeof(string));
                RQSTDT.Columns.Add("VLD_DATE_OVER", typeof(string));
                RQSTDT.Columns.Add("PRJT_NAME", typeof(string));
                RQSTDT.Columns.Add("PROC_YN", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["SHOPID"] = LoginInfo.CFG_SHOP_ID.ToString();
                dr["AREAID"] = cboArea.SelectedValue.ToString();

                dr["PROCID"] = Util.ConvertEmptyToNull(sProc);
                dr["WH_ID"] = Util.ConvertEmptyToNull(sWh);
                dr["RACK_ID"] = Util.ConvertEmptyToNull(sRack);
                dr["MODLID"] = Util.ConvertEmptyToNull(sModel);
                dr["PRODID"] = Util.ConvertEmptyToNull(sProd);
                dr["PRDT_CLSS_CODE"] = Util.ConvertEmptyToNull(sPrdtClss);
                dr["LOTID"] = Util.ConvertEmptyToNull(sLot);
                dr["HOLD_FLAG"] = Util.ConvertEmptyToNull(sHold);
                dr["VLD_DATE_OVER"] = ((bool)chkVldDate.IsChecked) ? "Y" : null;
                dr["PRJT_NAME"] = Util.ConvertEmptyToNull(sPrjt);
                dr["PROC_YN"] = ((bool)chkProcWip.IsChecked) ? "Y" : "N";

                RQSTDT.Rows.Add(dr);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_WH_WIP_RETURN_LOT", "RQSTDT", "RSLTDT", RQSTDT);

                // dgElecStock.ItemsSource = DataTableConverter.Convert(dtRslt);
                Util.GridSetData(dgElecStock, dtRslt, FrameOperation, true);
                 
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }

        private void SetcboProcess()
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("AREAID", typeof(string));
                RQSTDT.Columns.Add("EQSGID", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["AREAID"] = cboArea.SelectedValue;
                dr["EQSGID"] = Util.GetCondition(cboEquipmentSegment);
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_PROCESS_CBO", "RQSTDT", "RSLTDT", RQSTDT);

                cboProcess.ItemsSource = DataTableConverter.Convert(dtResult);

                for (int i = 0; i < dtResult.Rows.Count; i++)
                {
                    //cboProcess.Check(i);
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void ColVisible()
        {
            try
            {

                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("CMCDTYPE", typeof(string));
                RQSTDT.Columns.Add("CMCODE", typeof(string));
                RQSTDT.Columns.Add("CMCDIUSE", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["CMCDTYPE"] = "ELEC_SEL_WH_WIP";
                dr["CMCODE"] = LoginInfo.CFG_SHOP_ID;
                dr["CMCDIUSE"] = "Y";
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_COMMONCODE_USE", "RQSTDT", "RSLTDT", RQSTDT);

                if (dtResult.Rows.Count > 0)
                {
                    dgElecStock.Columns["ACTNAME"].Visibility = Visibility.Visible;
                    dgElecStock.Columns["ACTDTTM"].Visibility = Visibility.Visible;
                    //DaileySearch.Visibility = Visibility.Visible;
                }
                else
                {
                    dgElecStock.Columns["ACTNAME"].Visibility = Visibility.Collapsed;
                    dgElecStock.Columns["ACTDTTM"].Visibility = Visibility.Collapsed;
                    //DaileySearch.Visibility = Visibility.Collapsed;
                }
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }
        #endregion


    }
}
