/*************************************************************************************
 Created Date : 2024.08.13
      Creator : 
   Decription : 
--------------------------------------------------------------------------------------
 [Change History]

  2024.08.13  유재홍      신규 생성
**************************************************************************************/

using C1.WPF;
using C1.WPF.DataGrid;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using LGC.GMES.MES.CMM001.Extensions;
using System;
using System.Collections.Generic;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;


namespace LGC.GMES.MES.COM001
{
    public partial class COM001_404 : UserControl, IWorkArea
    {
        #region Declaration & Constructor 

        private Util _Util = new Util();
        private int rowIndex = -1;

        string _AREATYPE = "";

        // Roll Map
        private bool _isRollMapLot;

        private string _lotId = string.Empty;
        private string _wipSeq = string.Empty;
        private string _processCode = string.Empty;
        private string _laneQty = string.Empty;
        private string _wipStat = string.Empty;
        private string _equipmentCode = string.Empty;
        private string _equipmentName = string.Empty;
        private string _equipmentSegmentCode = string.Empty;
        private string _prod_Ver_Code = string.Empty;
        private string _lastProcId = string.Empty;


        public COM001_404()
        {

            InitializeComponent();

        }
        public IFrameOperation FrameOperation
        {
            get;
            set;
        }
        #endregion

        #region Initialize
        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            //사용자 권한별로 버튼 숨기기
            List<Button> listAuth = new List<Button>();
            Util.pageAuth(listAuth, FrameOperation.AUTHORITY);
            //여기까지 사용자 권한별로 버튼 숨기기
            GetSpclMngtInfo();

            object[] parameters = this.FrameOperation.Parameters;





        }


        #endregion

        #region Event


        private void SearchSpclLot(string sID, string sGroup)
        {
            DataSet inData = new DataSet();
            DataTable dtRqst = inData.Tables.Add("INDATA");

            dtRqst.Columns.Add("LANGID", typeof(string));
            dtRqst.Columns.Add("LOTID", typeof(string));
            dtRqst.Columns.Add("SPCL_LOT_GROUP", typeof(string));


            DataRow dr = dtRqst.NewRow();
            dr["LANGID"] = LoginInfo.LANGID;
            dr["LOTID"] = sID;
            dr["SPCL_LOT_GROUP"] = sGroup;

            dtRqst.Rows.Add(dr);

            //WIPACTHISTORY, 

            // CSR : [E20231018-000975] - 전극 재와인딩 공정 추가 Biz 분리 처리 [동별 공통코드 : REWINDING_LOT_INFO_TREE]
            string sBizName = string.Empty;

            sBizName = "DA_PRD_SEL_SPCL_LOT_IN_GROUP";

            /////////////////////////////////////////////////////////////////////
            //DataSet dsRslt = new ClientProxy().ExecuteServiceSync_Multi("BR_PRD_SEL_LOT_INFO_END", "INDATA", "LOTSTATUS,TREEDATA", inData);
            DataSet dsRslt = new ClientProxy().ExecuteServiceSync_Multi(sBizName, "INDATA", "OUTDATA", inData);

        }


        private void Init()
        {
            Util.gridClear(dgLotInfo);
            //Util.gridClear(dgHistory);
            //trvData.ItemsSource = null;

            _lotId = string.Empty;
            _wipSeq = string.Empty;
            _processCode = string.Empty;
            _laneQty = string.Empty;
            _wipStat = string.Empty;
            _equipmentCode = string.Empty;
            _equipmentName = string.Empty;
            _equipmentSegmentCode = string.Empty;
            _prod_Ver_Code = string.Empty;

            _lastProcId = string.Empty;

        }




        #endregion

        #region Meho
        public void GetSpclMngtInfo()
        {
            try
            {
                string Stap = string.Empty;
                string sBizName = "DA_PRD_SEL_SPCL_STCK_MNGT";

                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("AREAID", typeof(string));
                RQSTDT.Columns.Add("USE_FLAG", typeof(string));
                DataRow dr = RQSTDT.NewRow();


                dr["LANGID"] = LoginInfo.LANGID;
                dr["AREAID"] = LoginInfo.CFG_AREA_ID;

                if (USE_FLAG_N_Display.IsChecked == true)
                {
                    dr["USE_FLAG"] = "N";
                }
                else
                {
                    dr["USE_FLAG"] = "Y";
                }
                


                RQSTDT.Rows.Add(dr);



                DataTable dtResult = new ClientProxy().ExecuteServiceSync(sBizName, "RQSTDT", "RSLTDT", RQSTDT);

                Util.gridClear(dgSpclMngtList);

                dgSpclMngtList.ItemsSource = DataTableConverter.Convert(dtResult);

                // dtLotStatus Validation 추가
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }



        #region [동별 전극 등급 관리 여부 정보]  
        private bool EltrGrdCodeColumnVisible()
        {
            try
            {
                DataTable inTable = new DataTable();
                inTable.TableName = "RQSTDT";
                inTable.Columns.Add("LANGID", typeof(string));
                inTable.Columns.Add("AREAID", typeof(string));
                inTable.Columns.Add("COM_TYPE_CODE", typeof(string));
                inTable.Columns.Add("COM_CODE", typeof(string));

                DataRow dr = inTable.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["AREAID"] = LoginInfo.CFG_AREA_ID;
                dr["COM_TYPE_CODE"] = "ELTR_GRD_JUDG_ITEM_CODE";
                inTable.Rows.Add(dr);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_TB_MMD_AREA_COM_CODE", "INDATA", "OUTDATA", inTable);

                if (dtRslt != null && dtRslt.Rows.Count > 0)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return false;
            }
        }
        #endregion




        public void TreeItemExpandAll()
        {
            IList<DependencyObject> items = new List<DependencyObject>();
            // VTreeHelper.GetChildrenOfType(trvData, typeof(C1TreeViewItem), ref items);

            foreach (C1TreeViewItem item in items)
            {
                TreeItemExpandNodes(item);
            }
        }

        public void TreeItemExpandNodes(C1TreeViewItem item)
        {
            item.IsExpanded = true;
            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                IList<DependencyObject> items = new List<DependencyObject>();
                VTreeHelper.GetChildrenOfType(item, typeof(C1TreeViewItem), ref items);
                foreach (C1TreeViewItem childItem in items)
                {
                    TreeItemExpandNodes(childItem);
                }
            }));
        }
        #endregion










        /// <summary>
        ///  2022 05 19 오화백 공통코드에서 슬라이딩 관리동인지 확인
        /// </summary>
        private bool ChkSlid_Matc_Area()
        {

            bool bRet = false;
            DataTable dt = new DataTable();
            dt.Columns.Add("CMCDTYPE", typeof(string));
            dt.Columns.Add("CBO_CODE", typeof(string));

            DataRow dr = dt.NewRow();
            dr["CMCDTYPE"] = "SLID_MATC_GR_AREA";
            dr["CBO_CODE"] = LoginInfo.CFG_AREA_ID;
            dt.Rows.Add(dr);

            DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_COMMONCODE_ATTRIBUTE", "RQSTDT", "RSLTDT", dt);

            if (dtResult.Rows.Count != 0 && dtResult.Rows[0]["ATTRIBUTE1"].ToString() == "Y")
            {
                bRet = true;
            }
            else
            {
                bRet = false;
            }
            return bRet;
        }




        private DataTable GetRollMapInputLotCode(string lotId)
        {
            DataTable inTable = new DataTable { TableName = "RQSTDT" };
            inTable.Columns.Add("LANGID", typeof(string));
            inTable.Columns.Add("LOTID", typeof(string));
            inTable.Columns.Add("WIPSEQ", typeof(decimal));
            inTable.Columns.Add("EQPT_MEASR_PSTN_ID", typeof(string));

            DataRow dr = inTable.NewRow();
            dr["LANGID"] = LoginInfo.LANGID;
            dr["LOTID"] = lotId;
            dr["WIPSEQ"] = _wipSeq;
            dr["EQPT_MEASR_PSTN_ID"] = "UW";
            inTable.Rows.Add(dr);

            return new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_ROLLMAP_COLLECT_INFO", "RQSTDT", "RSLTDT", inTable);
        }
        private DataTable GetEquipmentCode(string lotId, string wipSeq)
        {
            DataTable inTable = new DataTable { TableName = "RQSTDT" };
            inTable.Columns.Add("EQPTID", typeof(string));
            inTable.Columns.Add("LOTID", typeof(string));
            inTable.Columns.Add("WIPSEQ", typeof(decimal));

            DataRow dr = inTable.NewRow();
            //dr["EQPTID"] = equipmentCode;
            dr["LOTID"] = lotId;
            dr["WIPSEQ"] = wipSeq;
            inTable.Rows.Add(dr);

            return new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_VW_WIPHISTORY", "RQSTDT", "RSLTDT", inTable);
        }

        private bool IsElectrodeGradeInfo()
        {
            try
            {
                const string bizRuleName = "DA_BAS_SEL_COMMONCODE_TBL";

                DataTable inTable = new DataTable("RQSTDT");
                inTable.Columns.Add("LANGID", typeof(string));
                inTable.Columns.Add("CMCDTYPE", typeof(string));
                inTable.Columns.Add("CMCODE", typeof(string));

                DataRow newRow = inTable.NewRow();
                newRow["LANGID"] = LoginInfo.LANGID;
                newRow["CMCDTYPE"] = "GRD_JUDG_DISP_AREA";
                newRow["CMCODE"] = LoginInfo.CFG_AREA_ID;
                inTable.Rows.Add(newRow);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync(bizRuleName, "RQSTDT", "RSLTDT", inTable);
                if (CommonVerify.HasTableRow(dtResult))
                    return true;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return false;
            }

            return false;
        }

        private string GetLotUnitCode()
        {
            try
            {
                const string bizRuleName = "DA_PRD_SEL_VW_LOT";

                DataTable inTable = new DataTable { TableName = "RQSTDT" };
                inTable.Columns.Add("LOTID", typeof(string));

                DataRow dr = inTable.NewRow();
                dr["LOTID"] = _lotId;
                inTable.Rows.Add(dr);
                DataTable result = new ClientProxy().ExecuteServiceSync(bizRuleName, "RQSTDT", "RSLTDT", inTable);

                if (CommonVerify.HasTableRow(result))
                {
                    return result.Rows[0]["LOTUID"].GetString();
                }
                else
                {
                    return string.Empty;
                }

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return string.Empty;
            }
        }

        private bool IsAreaCommonCodeUse(string sCodeType, string sCodeName)
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("AREAID", typeof(string));
                RQSTDT.Columns.Add("COM_TYPE_CODE", typeof(string));
                RQSTDT.Columns.Add("COM_CODE", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["AREAID"] = LoginInfo.CFG_AREA_ID;
                dr["COM_TYPE_CODE"] = sCodeType;
                dr["COM_CODE"] = sCodeName == string.Empty ? null : sCodeName;
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_TB_MMD_AREA_COM_CODE_FOR_AREA", "RQSTDT", "RSLTDT", RQSTDT);

                if (dtResult != null && dtResult.Rows.Count > 0)
                    return true;

                return false;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        private void btnAddLOT_Click(object sender, RoutedEventArgs e)
        {
            COM001_404_REGLOT popupReglot = new COM001_404_REGLOT { FrameOperation = FrameOperation };
            popupReglot.FrameOperation = this.FrameOperation;


            if (popupReglot != null)
            {

                int rowIndex = _Util.GetDataGridFirstRowIndexWithTopRow(dgSpclMngtList, "CHK");
                object[] Parameters = new object[2];
                Parameters[0] = Util.NVC(DataTableConverter.GetValue(dgSpclMngtList.Rows[rowIndex].DataItem, "SPCL_STCK_MNGT_NAME")).ToString(); ;
                Parameters[1] = Util.NVC(DataTableConverter.GetValue(dgSpclMngtList.Rows[rowIndex].DataItem, "SPCL_STCK_MNGT_ID")).ToString(); ;


                C1WindowExtension.SetParameters(popupReglot, Parameters);

                popupReglot.Closed += new EventHandler(popupReglot_Closed);

                ;

                //        // 팝업 화면 숨겨지는 문제 수정.               //        //this.Dispatcher.BeginInvoke(new Action(() => wndPopup.ShowModal()));
                //        grdMain.Children.Add(wndPopup);
                //        wndPopup.BringToFront();
                //    }


                

                popupReglot.Closed += new EventHandler(popupReglot_Closed);

                Dispatcher.BeginInvoke(new Action(() => popupReglot.ShowModal()));






            }
        }

        private void popupReglot_Closed(object sender, EventArgs e)
        {


            GetSpclMngtInfo();
            GetSpclMngtLOTInfo();
            COM001_404_REGLOT popup = sender as COM001_404_REGLOT;

        }

        private void popupUPDMNGT_Closed(object sender, EventArgs e)
        {


            GetSpclMngtInfo();
            GetSpclMngtLOTInfo();
            COM001_404_UPD_MNGT popup = sender as COM001_404_UPD_MNGT;

        }


        private void dgChoice_Checked(object sender, RoutedEventArgs e)//선택시
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

                        // row 색 바꾸기

                        dgSpclMngtList.SelectedIndex = idx;

                    }
                }
                SetSpclLotInfo();
                GetSpclMngtLOTInfo();
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void SetSpclLotInfo()


        {
            ;
            int rowIndex = _Util.GetDataGridFirstRowIndexWithTopRow(dgSpclMngtList, "CHK");
            String SPCL_LOT = Util.NVC(DataTableConverter.GetValue(dgSpclMngtList.Rows[rowIndex].DataItem, "SPCL_STCK_MNGT_NAME")).ToString();
            String PlanDate = Util.NVC(DataTableConverter.GetValue(dgSpclMngtList.Rows[rowIndex].DataItem, "PRCS_SCHD_DATE")).ToString();
            String UseUser = Util.NVC(DataTableConverter.GetValue(dgSpclMngtList.Rows[rowIndex].DataItem, "PRCS_USERID")).ToString();
            String NOTE = Util.NVC(DataTableConverter.GetValue(dgSpclMngtList.Rows[rowIndex].DataItem, "NOTE")).ToString();
            String USE_FLAG = Util.NVC(DataTableConverter.GetValue(dgSpclMngtList.Rows[rowIndex].DataItem, "USE_FLAG")).ToString();

            txtSPCL_LOT.Text = SPCL_LOT;
            txtPlanDate.Text = PlanDate;
            txtUseUser.Text = UseUser;
            txtNote.Text = NOTE;
            txtUseFlag.Text = USE_FLAG;
        }

        public void GetSpclMngtLOTInfo()
        {
            try
            {
                string Stap = string.Empty;
                string sBizName = "DA_PRD_SEL_SPCL_STCK_MNGT_LOT";

                int rowIndex = _Util.GetDataGridFirstRowIndexWithTopRow(dgSpclMngtList, "CHK");

                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("SPCL_STCK_MNGT_ID", typeof(string));
                RQSTDT.Columns.Add("LANGID", typeof(string));
                DataRow dr = RQSTDT.NewRow();


                dr["SPCL_STCK_MNGT_ID"] = Util.NVC(DataTableConverter.GetValue(dgSpclMngtList.Rows[rowIndex].DataItem, "SPCL_STCK_MNGT_ID"));
                dr["LANGID"] = LoginInfo.LANGID;


                RQSTDT.Rows.Add(dr);



                DataTable dtResult = new ClientProxy().ExecuteServiceSync(sBizName, "RQSTDT", "RSLTDT", RQSTDT);
                dtResult.Columns.Add("CHK", typeof(Boolean));
                for(int i = 0; i < dtResult.Rows.Count; i++)
                {
                    dtResult.Rows[i]["CHK"] = false;
                }


                Util.gridClear(dgLotInfo);

                dgLotInfo.ItemsSource = DataTableConverter.Convert(dtResult);
                // dtLotStatus Validation 추가
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void btnAdd_Click(object sender, RoutedEventArgs e)
        {
            COM001_404_REG_MNGT popupReglot = new COM001_404_REG_MNGT { FrameOperation = FrameOperation };
            popupReglot.FrameOperation = this.FrameOperation;


            if (popupReglot != null)
            {


                

                popupReglot.Closed += new EventHandler(popupReglot_Closed);

                ;



                popupReglot.Closed += new EventHandler(popupReglot_Closed);

                Dispatcher.BeginInvoke(new Action(() => popupReglot.ShowModal()));


            }
        }

        private void btnEdit_Click(object sender, RoutedEventArgs e)
        {
            COM001_404_UPD_MNGT popupUPDMNGT = new COM001_404_UPD_MNGT { FrameOperation = FrameOperation };
            popupUPDMNGT.FrameOperation = this.FrameOperation;


            if (popupUPDMNGT != null)
            {

                int rowIndex = _Util.GetDataGridFirstRowIndexWithTopRow(dgSpclMngtList, "CHK");
                object[] Parameters = new object[6];
                Parameters[0] = Util.NVC(DataTableConverter.GetValue(dgSpclMngtList.Rows[rowIndex].DataItem, "SPCL_STCK_MNGT_NAME")).ToString(); 
                Parameters[1] = Util.NVC(DataTableConverter.GetValue(dgSpclMngtList.Rows[rowIndex].DataItem, "SPCL_STCK_MNGT_ID")).ToString(); 
                Parameters[2] = txtPlanDate.Text;
                Parameters[3] = txtUseUser.Text;
                Parameters[4] = txtNote.Text;
                Parameters[5] = Util.NVC(DataTableConverter.GetValue(dgSpclMngtList.Rows[rowIndex].DataItem, "USE_FLAG")).ToString();

                C1WindowExtension.SetParameters(popupUPDMNGT, Parameters);

                popupUPDMNGT.Closed += new EventHandler(popupUPDMNGT_Closed);

                ;

                //        // 팝업 화면 숨겨지는 문제 수정.               //        //this.Dispatcher.BeginInvoke(new Action(() => wndPopup.ShowModal()));
                //        grdMain.Children.Add(wndPopup);
                //        wndPopup.BringToFront();
                //    }




                popupUPDMNGT.Closed += new EventHandler(popupUPDMNGT_Closed);

                Dispatcher.BeginInvoke(new Action(() => popupUPDMNGT.ShowModal()));






            }
        }

        private void btnDelLOT_Click(object sender, RoutedEventArgs e)
        {

            DataTable dtLotInfo = DataTableConverter.Convert(dgLotInfo.ItemsSource);

            
            for (int i = 0; i < dtLotInfo.Rows.Count; i++)
            {
                if (dtLotInfo.Rows[i]["CHK"].Equals(true))
                {
                    Util.MessageConfirm("SFU1259", (result) =>//삭제처리하시겠습니까
                    {
                        if (result == MessageBoxResult.OK)
                        {
                            DeleteLOT();
                        }
                        GetSpclMngtInfo();
                        GetSpclMngtLOTInfo();
                    });
                    return;

                }
                Util.MessageValidation("SFU1132");  //삭제할 LOT을 선택하세요
            }




           
        }

        private void DeleteLOT()
        {
            try
            {

                DataSet inDataSet = new DataSet();
                DataTable inLotTable = inDataSet.Tables.Add("INLOT");
                inLotTable.Columns.Add("LOTID", typeof(string));

                DataTable inDataTable = inDataSet.Tables.Add("INDATA");
                inDataTable.Columns.Add("USERID", typeof(string));





                DataTable dtLotInfo = DataTableConverter.Convert(dgLotInfo.ItemsSource);


                for (int i = 0; i < dtLotInfo.Rows.Count; i++)
                {
                    if (dtLotInfo.Rows[i]["CHK"].Equals(true))
                    {
                        DataRow newIDRow = inLotTable.NewRow();
                        newIDRow["LOTID"] = dtLotInfo.Rows[i]["LOTID"];
                        inLotTable.Rows.Add(newIDRow);
                    }
                }

                DataRow newRow = inDataTable.NewRow();
                newRow["USERID"] = LoginInfo.USERID;
                inDataTable.Rows.Add(newRow);
                newRow = null;


                new ClientProxy().ExecuteServiceSync_Multi("BR_PRD_UPD_SPCL_STCK_LOT_DEL", "INLOT,INDATA", null, inDataSet);
                Util.MessageValidation("SFU1275");  //정상 처리 되었습니다.
                
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                loadingIndicator.Visibility = Visibility.Collapsed;
            }


        }



        private void _USE_FLAG_Y_Changed(object sender, RoutedEventArgs e)
        {
            GetSpclMngtInfo();
        }

        private void _USE_FLAG_N_Changed(object sender, RoutedEventArgs e)
        {
            GetSpclMngtInfo();
        }
    }
}
