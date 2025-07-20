/*************************************************************************************
 Created Date : 2020.11.24
      Creator : PSM
   Decription : 재공정보현황(Lot별)
--------------------------------------------------------------------------------------
 [Change History]
  2020.11.24  DEVELOPER : Initial Created.
  2021.08.19        KDH : Lot 유형 검색조건 추가
  2023.04.26        홍석원 : Hold 제외 기능 추가
  2023.05.19        박승렬 : CSR : E20230519-001478  / 조회조건에 PKG 생산일자 추가
  2023.11.16        조영대 : LOT ID 8자리, 10자리 구분 추가
  2024.03.06        조영대 : 수치컬럼 너비 고정
  2024.08.06        남형희 : CSR : E20240710-001088 / Lot 자리수 선택에 따라, PKG LOT ID 자리수 Validation 추가
  2024.07.23        Ahmad F, Fauzul A : E20240709-000780, Changing ComboBox of Line and Model to be MultiselectionBox and reducing the models that are not in use (GDC)
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
using System.Linq;

namespace LGC.GMES.MES.FCS001
{
    public partial class FCS001_006 : UserControl, IWorkArea
    {
        #region Declaration & Constructor 

        Util _Util = new Util();

        // 2024.07.23 Ahmad F, Line & Model MultiSelectionBox
        bool lineComboCheckFlag = false;
        int lineTotalCnt = 0;
        DataTable dtResultLine;
        bool modelComboCheckFlag = false;
        int modelTotalCnt = 0;
        DataTable dtResultModel;

        public FCS001_006()
        {
            InitializeComponent();
            InitCombo();
        }
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

        private void InitCombo()
        {
            CommonCombo_Form _combo = new CommonCombo_Form();

            // 2024.07.23 Ahmad F, commenting due to changes 
            //C1ComboBox[] cboLineChild = { cboModel };
            //_combo.SetCombo(cboLine, CommonCombo_Form.ComboStatus.ALL, sCase: "LINE", cbChild: cboLineChild);
            // C1ComboBox[] cboModelChild = { cboRoute };
            //C1ComboBox[] cboModelParent = { cboLine };
            //_combo.SetCombo(cboModel, CommonCombo_Form.ComboStatus.ALL, sCase: "LINEMODEL", cbChild: cboModelChild, cbParent: cboModelParent);

            string[] sFilter = { "ROUT_GR_CODE" };

            // 2024.07.23 Ahmad F, commenting due to changes
            //C1ComboBox[] cboRouteSetChild = { cboRoute };
            //_combo.SetCombo(cboRouteDG, CommonCombo_Form.ComboStatus.ALL, sCase: "COMMCODE", sFilter: sFilter, cbChild: cboRouteSetChild);
            _combo.SetCombo(cboRouteDG, CommonCombo_Form.ComboStatus.ALL, sCase: "COMMCODE", sFilter: sFilter);

            // 2024.07.23 Ahmad F, commenting due to changes 
            // 2021-05-13 Parent Combobox Parameter 매핑 오류로 인하여 null Combobox 추가
            //C1ComboBox nCbo = new C1ComboBox();
            //C1ComboBox[] cboRouteParent = { cboLine, cboModel, nCbo, nCbo, cboRouteDG };
            //_combo.SetCombo(cboRoute, CommonCombo_Form.ComboStatus.ALL, sCase: "ROUTE", cbParent: cboRouteParent);

            string[] sFilter1 = { "COMBO_FORM_SPCL_FLAG" };
            _combo.SetCombo(cboSpecial, CommonCombo_Form.ComboStatus.ALL, sCase: "COMMCODE", sFilter: sFilter1);

            // Lot 유형
            _combo.SetCombo(cboLotType, CommonCombo_Form.ComboStatus.ALL, sCase: "LOTTYPE"); // 2021.08.19 Lot 유형 검색조건 추가
        }
        #endregion

        #region Method

        private void GetList()
        {
            try
            {
                //E20240710-001088 2024.08.06 남형희 CSR : E20240710-001088 / Lot 자리수 선택에 따라, PKG LOT ID 자리수 Validation 추가
                if (chkDigitLot.IsChecked == true && txtLotId.Text.Length != 0 && txtLotId.Text.Length != 8)
                {
                    dgWipbyLot.ClearRows();
                    Util.MessageValidation("SFU10020");
                    return;
                }
                else if (chkDigitPkgLot.IsChecked == true && txtLotId.Text.Length != 0 && txtLotId.Text.Length != 10)
                {
                    dgWipbyLot.ClearRows();
                    Util.MessageValidation("SFU10021");
                    return;
                }

                DataTable dtRqst = new DataTable();

                dtRqst.TableName = "RQSTDT";
                dtRqst.Columns.Add("AREAID", typeof(string));
                dtRqst.Columns.Add("LANGID", typeof(string));
                dtRqst.Columns.Add("EQSGID", typeof(string));
                dtRqst.Columns.Add("MDLLOT_ID", typeof(string));
                dtRqst.Columns.Add("ROUTID", typeof(string));
                dtRqst.Columns.Add("PROD_LOTID", typeof(string));
                dtRqst.Columns.Add("SPCL_TYPE_CODE", typeof(string));
                dtRqst.Columns.Add("ROUT_TYPE_CODE", typeof(string));
                dtRqst.Columns.Add("BY_MODEL", typeof(string));
                dtRqst.Columns.Add("LOTTYPE", typeof(string)); // 2021.08.19 Lot 유형 검색조건 추가    
                dtRqst.Columns.Add("FROM_DATE", typeof(string)); // 2023. 05. 18 생산일자 검색조건 추가
                dtRqst.Columns.Add("TO_DATE", typeof(string)); // 2023. 05. 18 생산일자 검색조건 추가
                dtRqst.Columns.Add("EXCEPT_HOLD", typeof(string));
                dtRqst.Columns.Add("DIGIT_TYPE", typeof(string));

                DataRow dr = dtRqst.NewRow();
                dr["AREAID"] = LoginInfo.CFG_AREA_ID;
                dr["LANGID"] = LoginInfo.LANGID;

                //2023.07.23, Fauzul Azkia, Setting line and model to the row
                //dr["EQSGID"] = Util.GetCondition(cboLine, bAllNull: true);
                //dr["MDLLOT_ID"] = Util.GetCondition(cboModel, bAllNull: true);
                dr["EQSGID"] = (!string.IsNullOrEmpty(cboLine.SelectedItemsToString)) ? cboLine.SelectedItemsToString : null;
                dr["MDLLOT_ID"] = (!string.IsNullOrEmpty(cboModel.SelectedItemsToString)) ? cboModel.SelectedItemsToString : null;

                dr["ROUTID"] = Util.GetCondition(cboRoute, bAllNull: true);
                if (!string.IsNullOrEmpty(txtLotId.Text)) dr["PROD_LOTID"] = txtLotId.Text;
                dr["SPCL_TYPE_CODE"] = Util.GetCondition(cboSpecial, bAllNull: true);
                dr["ROUT_TYPE_CODE"] = Util.GetCondition(cboRouteDG, bAllNull: true);
                dr["BY_MODEL"] = (bool)chkModel.IsChecked ? "Y" : "N";
                dr["LOTTYPE"] = Util.GetCondition(cboLotType, bAllNull: true); // 2021.08.19 Lot 유형 검색조건 추가
                dr["FROM_DATE"] = Util.GetCondition(dtpDateFrom, "yyyy-MM-dd 00:00:00"); // 2023. 05. 18 생산일자 검색조건 추가
                dr["TO_DATE"] = Util.GetCondition(dtpDateTo, "yyyy-MM-dd 23:59:59"); // 2023. 05. 18 생산일자 검색조건 추가
                dr["EXCEPT_HOLD"] = chkHold.IsChecked == true ? "Y" : "N";
                dr["DIGIT_TYPE"] = chkDigitPkgLot.IsChecked.Equals(true) ? "10" : "8";
                dtRqst.Rows.Add(dr);

                btnSearch.IsEnabled = false;

                // 백그라운드 실행
                dgWipbyLot.ExecuteService("DA_SEL_WIP_RETRIEVE_INFO_SPECIAL_LOTID_INDUSTRY_MODEL", "RQSTDT", "RSLTDT", dtRqst, true);

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        #endregion

        #region Event

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            //2024.07.23 Ahmad F, populating combobox 
            SetLineCombo(cboLine);
            SetLineModel(cboModel);
            SetFormRoute(cboRoute);

            // 사용자 컬럼 설정 제외
            dgWipbyLot.UserConfigExceptColumns.Add("DAY_GR_LOTID");
            dgWipbyLot.UserConfigExceptColumns.Add("PROD_LOTID");

            //2024.07.23 Ahmad F, retaining combobox 
            this.Loaded -= UserControl_Loaded;
        }

        //2024.07.23 Ahmad F, adding new method to populate line combo
        private void SetLineCombo(MultiSelectionBox mcb)
        {
            try
            {
                DataTable dtRqstA = new DataTable();
                dtRqstA.TableName = "RQSTDT";
                dtRqstA.Columns.Add("LANGID", typeof(string));
                dtRqstA.Columns.Add("AREAID", typeof(string));

                DataRow drA = dtRqstA.NewRow();
                drA["LANGID"] = LoginInfo.LANGID;
                drA["AREAID"] = LoginInfo.CFG_AREA_ID;
                dtRqstA.Rows.Add(drA);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_COMBO_LINE", "RQSTDT", "RSLTDT", dtRqstA);

                dtResultLine = dtResult;
                lineTotalCnt = dtResult.Rows.Count;

                if (dtResult.Rows.Count != 0)
                {
                    mcb.isAllUsed = false;
                    if (dtResult.Rows.Count == 1)
                    {
                        mcb.ItemsSource = DataTableConverter.Convert(dtResult);
                        mcb.Check(-1);
                    }
                    else
                    {
                        mcb.isAllUsed = true;
                        mcb.ItemsSource = DataTableConverter.Convert(dtResult);
                        mcb.CheckAll();
                    }
                }
                else
                {
                    mcb.ItemsSource = null;
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        //2024.07.23 Ahmad F, adding new method to populate model 
        private void SetLineModel(MultiSelectionBox mcb)
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("EQSGID", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["EQSGID"] = cboLine.SelectedItemsToString;

                RQSTDT.Rows.Add(dr);

                // 2024.07.23 Fauzul Azkia, Using new DA 
                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_COMBO_LINE_MULTI_MODEL_IN_WIP", "RQSTDT", "RSLTDT", RQSTDT);

                mcb.DisplayMemberPath = "CBO_NAME";
                mcb.SelectedValuePath = "CBO_CODE";

                dtResultModel = dtResult;
                modelTotalCnt = dtResult.Rows.Count;

                if (dtResult.Rows.Count != 0)
                {
                    mcb.isAllUsed = false;
                    if (dtResult.Rows.Count == 1)
                    {
                        mcb.ItemsSource = DataTableConverter.Convert(dtResult);
                        mcb.Check(-1);
                    }
                    else
                    {
                        mcb.isAllUsed = true;
                        mcb.ItemsSource = DataTableConverter.Convert(dtResult);
                        mcb.CheckAll();
                    }
                }
                else
                {
                    //2024.07.23 Fauzul Azkia, Storing null data to prevent popup
                    //mcb.ItemsSource = null;
                    mcb.ItemsSource = dtResult.Copy().AsDataView();
                    mcb.CheckAll();
                }
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }

        //2024.07.23 Ahmad F, adding new method to populate route 
        private void SetFormRoute(C1ComboBox cbo)
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("MDLLOT_ID", typeof(string));
                RQSTDT.Columns.Add("EQSGID", typeof(string));
                RQSTDT.Columns.Add("ROUTE_TYPE_DG", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["EQSGID"] = (!string.IsNullOrEmpty(cboLine.SelectedItemsToString)) ? cboLine.SelectedItemsToString : null;   //cboline.getbindvalue()
                dr["MDLLOT_ID"] = (!string.IsNullOrEmpty(cboModel.SelectedItemsToString)) ? cboModel.SelectedItemsToString : null; //cbomodel.getbindvalue()
                dr["ROUTE_TYPE_DG"] = cboRouteDG.GetBindValue();

                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_COMBO_FORM_MULTI_ROUTE", "RQSTDT", "RSLTDT", RQSTDT);

                cbo.DisplayMemberPath = "CBO_NAME";
                cbo.SelectedValuePath = "CBO_CODE";
                cbo.ItemsSource = AddStatus(dtResult, CommonCombo.ComboStatus.ALL, "CBO_CODE", "CBO_NAME").Copy().AsDataView();

                if (cbo.Items.Count > 0)
                    cbo.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }

        //2024.07.23 Ahmad F, adding new method to add status
        private DataTable AddStatus(DataTable dt, CommonCombo.ComboStatus cs, string sValue, string sDisplay)
        {
            DataRow dr = dt.NewRow();

            switch (cs)
            {
                case CommonCombo.ComboStatus.ALL:
                    dr[sDisplay] = "-ALL-";
                    dr[sValue] = "";
                    dt.Rows.InsertAt(dr, 0);
                    break;

                case CommonCombo.ComboStatus.SELECT:
                    dr[sDisplay] = "-SELECT-";
                    dr[sValue] = "SELECT";
                    dt.Rows.InsertAt(dr, 0);
                    break;

                case CommonCombo.ComboStatus.NA:
                    dr[sDisplay] = "-N/A-";
                    dr[sValue] = "";
                    dt.Rows.InsertAt(dr, 0);
                    break;
            }

            return dt;
        }

        //2024.07.23 Ahmad F, adding new method on the selection of line change
        private void cboLine_SelectionChanged(object sender, EventArgs e)
        {
            if (cboLine.SelectedItems.Count == 0)
            {
                cboLine.CheckAll();
            }
        }

        //2024.07.23 Ahmad F, adding new method for dropdown of line closed 
        private void cboLine_DropDownClosed(object sender)
        {
            if (sender == null) return;
            SetLineModel(cboModel);
            SetFormRoute(cboRoute);
        }

        //2024.07.23 Ahmad F, adding new method on the selection of model change 
        private void cboModel_SelectionChanged(object sender, EventArgs e)
        {
            if (cboModel.SelectedItems.Count == 0)
            {
                cboModel.CheckAll();
            }
        }

        //2024.07.23 Ahmad F, adding new method for dropdown of model closed 
        private void cboModel_DropDownClosed(object sender)
        {
            if (sender == null) return;
            SetFormRoute(cboRoute);
        }

        //2024.07.23 Ahmad F, adding new method on the selection of route change 
        private void cboRouteDg_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            if (sender == null) return;
            if (lineComboCheckFlag || modelComboCheckFlag) SetFormRoute(cboRoute);
        }

        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            GetList();
        }

        private void chkModel_Checked(object sender, RoutedEventArgs e)
        {
            dgWipbyLot.Columns["MDLLOT_ID"].Visibility = Visibility.Visible;
            dgWipbyLot.Columns["MODEL_NAME"].Visibility = Visibility.Visible;
            GetList();
        }

        private void chkModel_Unchecked(object sender, RoutedEventArgs e)
        {
            dgWipbyLot.Columns["MDLLOT_ID"].Visibility = Visibility.Collapsed;
            dgWipbyLot.Columns["MODEL_NAME"].Visibility = Visibility.Collapsed;
        }

        private void chkDigitLot_Checked(object sender, RoutedEventArgs e)
        {
            if (chkDigitLot.IsChecked.Equals(true))
            {
                dgWipbyLot.Columns["DAY_GR_LOTID"].Visibility = Visibility.Visible;
                dgWipbyLot.Columns["PROD_LOTID"].Visibility = Visibility.Collapsed;

                GetList();
            }
        }

        private void chkDigitPkgLot_Checked(object sender, RoutedEventArgs e)
        {
            if (chkDigitPkgLot.IsChecked.Equals(true))
            {
                dgWipbyLot.Columns["PROD_LOTID"].Visibility = Visibility.Visible;
                dgWipbyLot.Columns["DAY_GR_LOTID"].Visibility = Visibility.Collapsed;

                GetList();
            }
        }

        private void txtLotId_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                GetList();
            }
        }

        private void dgWipbyLot_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            // 2024.08.22 Ahmad F. null value handle
            if (sender == null) return;
            C1.WPF.DataGrid.C1DataGrid dg = sender as C1.WPF.DataGrid.C1DataGrid;

            //if (dg.CurrentColumn.Index < dgWipbyLot.Columns["WAITTRAY"].Index) return;
            if (dgWipbyLot.CurrentRow != null && (dgWipbyLot.CurrentColumn.Name.Equals("WAITTRAY") || dgWipbyLot.CurrentColumn.Name.Equals("WAITCELL") ||
                dgWipbyLot.CurrentColumn.Name.Equals("WORKTRAY") || dgWipbyLot.CurrentColumn.Name.Equals("WORKCELL") ||
                dgWipbyLot.CurrentColumn.Name.Equals("AGINGENDTRAY") || dgWipbyLot.CurrentColumn.Name.Equals("AGINGENDCELL") ||
                dgWipbyLot.CurrentColumn.Name.Equals("TROUBLETRAY") || dgWipbyLot.CurrentColumn.Name.Equals("TROUBLECELL") ||
                dgWipbyLot.CurrentColumn.Name.Equals("RECHECKTRAY") || dgWipbyLot.CurrentColumn.Name.Equals("RECHECKCELL") ||
                dgWipbyLot.CurrentColumn.Name.Equals("TOTALTRAY") || dgWipbyLot.CurrentColumn.Name.Equals("TOTALINPUTCELL") || dgWipbyLot.CurrentColumn.Name.Equals("TOTALCURRCELL")))
            {
            }
            else
            {
                return;
            }

            if (dg.CurrentCell.Text.Equals("0")) return;
            if (dg.CurrentRow.Index == dg.Rows.Count - 1) return;

            int rowIdx = dg.CurrentRow.Index;

            //2024.07.23 Ahmad F, storing selected CBO line 
            string sLINE_ID = cboLine.SelectedItemsToString;
            string sLINE_NAME = "";

            List<string> LcboLine = new List<string>();
            int arrCountLine = sLINE_ID.Split(',').Length;

            if (lineTotalCnt == arrCountLine)
            {
                sLINE_NAME = "All";
            }
            else
            {
                foreach (DataRow item in dtResultLine.Rows)
                {
                    for (int i = 0; i < arrCountLine; i++)
                    {
                        if (item["CBO_CODE"].ToString() == sLINE_ID.Split(',')[i].ToString())
                        {
                            LcboLine.Add(item["CBO_NAME"].ToString());
                            break;
                        }
                    }
                }
                sLINE_NAME = String.Join(",", LcboLine);
            }

            string sMODEL_ID = cboModel.SelectedItemsToString;
            string sMODEL_NAME = "";

            List<string> LcboModel = new List<string>();
            int arrCountModel = sMODEL_ID.Split(',').Length;

            if (modelTotalCnt == arrCountModel)
            {
                sMODEL_NAME = "-All-";
            }
            else
            {
                foreach (DataRow item in dtResultModel.Rows)
                {
                    for (int i = 0; i < arrCountModel; i++)
                    {
                        if (item["CBO_CODE"].ToString() == sMODEL_ID.Split(',')[i].ToString())
                        {
                            LcboModel.Add(item["CBO_NAME"].ToString());
                        }
                    }
                }
                sMODEL_NAME = String.Join(",", LcboModel);
            }
            // 2024.07.23 Ahmad F, Storing only selected Model name
            if (chkModel.IsChecked.Equals(true))
            {
                sMODEL_ID = Util.NVC(DataTableConverter.GetValue(dg.Rows[rowIdx].DataItem, "MDLLOT_ID"));
                sMODEL_NAME = Util.NVC(DataTableConverter.GetValue(dg.Rows[rowIdx].DataItem, "MODEL_NAME"));
            }

            object[] Parameters = new object[24];
            Parameters[0] = null; //sOPER
            Parameters[1] = null; //sOPER_NAME
            //2024.08.22 Ahmad F. Storing LINE id
            //Parameters[2] = Util.NVC(Util.GetCondition(cboLine, bAllNull: true)); // sLINE_ID
            Parameters[2] = sLINE_ID;
            // 2024.07.23 Ahmad F, Storing the line name
            //Parameters[3] = Util.]NVC(cboLine.Text); // sLINE_NAME
            Parameters[3] = sLINE_NAME;
            Parameters[4] = Util.NVC(Util.GetCondition(cboRoute, bAllNull: true));//sROUTE_ID
            Parameters[5] = Util.NVC(cboRoute.Text); //sROUTE_NAME

            //2024.08.22 Ahmad F. Storing model id
            //Parameters[6] = Util.NVC(Util.GetCondition(cboModel, bAllNull: true)); //sMODEL_ID
            Parameters[6] = sMODEL_ID;
            // 2024.07.23 Ahmad F, Storing the model name
            //Parameters[7] = Util.NVC(cboModel.Text); // sMODEL_NAME
            Parameters[7] = sMODEL_NAME;
            Parameters[8] = null; //sStatus
            Parameters[9] = null; //sStatusNameB
            Parameters[10] = Util.NVC(Util.GetCondition(cboRouteDG, bAllNull: true)); //_sROUTE_TYPE_DG
            Parameters[11] = Util.NVC(cboRouteDG.Text); //sRouteTypeDGName
            if (chkDigitPkgLot.IsChecked.Equals(true))
            {
                Parameters[12] = Util.NVC(DataTableConverter.GetValue(dg.Rows[rowIdx].DataItem, "PROD_LOTID")); //sLotID
                Parameters[23] = null; // DAY_GR_LOTID
            }
            else
            {
                Parameters[12] = null; //PROD_LOTID
                Parameters[23] = Util.NVC(DataTableConverter.GetValue(dg.Rows[rowIdx].DataItem, "DAY_GR_LOTID")); //DAY_GR_LOTID                
            }
            Parameters[13] = Util.NVC(Util.GetCondition(cboSpecial, bAllNull: true)); // sSpecial
                                                                                      // Parameters[14] = Util.NVC(cboSpecial.Text); // sSpecialName
            Parameters[14] = null;
            Parameters[15] = null;
            Parameters[16] = null;
            Parameters[17] = Util.NVC(Util.GetCondition(cboLotType, bAllNull: true));
            Parameters[18] = Util.NVC(cboLotType.Text);

            if (dg.CurrentColumn.Name.Equals("WORKTRAY") || dg.CurrentColumn.Name.Equals("WORKCELL"))
            {
                Parameters[8] = "S"; // sStatus
                Parameters[9] = ObjectDic.Instance.GetObjectName("WORK_TRAY"); // 작업 Tray
            }

            else if (dg.CurrentColumn.Name.Equals("AGINGENDTRAY") || dg.CurrentColumn.Name.Equals("AGINGENDCELL"))
            {
                Parameters[8] = "P"; // sStatus
                Parameters[9] = ObjectDic.Instance.GetObjectName("AGING_END_WAIT"); // Aging 종료대기
            }
            else if (dg.CurrentColumn.Name.Equals("WAITTRAY") || dg.CurrentColumn.Name.Equals("WAITCELL"))
            {
                Parameters[8] = "E"; // sStatus
                Parameters[9] = ObjectDic.Instance.GetObjectName("AFTER_END_WAIT"); // 작업종료 후 대기
            }
            else if (dg.CurrentColumn.Name.Equals("TROUBLETRAY") || dg.CurrentColumn.Name.Equals("TROUBLECELL"))
            {
                Parameters[8] = "T"; // sStatus
                Parameters[9] = ObjectDic.Instance.GetObjectName("WORK_ERR"); // 작업이상
            }
            else if (dg.CurrentColumn.Name.Equals("RECHECKTRAY") || dg.CurrentColumn.Name.Equals("RECHECKCELL"))
            {
                Parameters[8] = "R"; // sStatus
                Parameters[9] = ObjectDic.Instance.GetObjectName("RECHECK"); // RECHECK
            }
            else if (dg.CurrentColumn.Name.Equals("TOTALTRAY") || dg.CurrentColumn.Name.Equals("TOTALINPUTCELL") || dg.CurrentColumn.Name.Equals("TOTALCURRCELL"))
            {
                Parameters[8] = "A"; // sStatus
                Parameters[9] = ObjectDic.Instance.GetObjectName("TOTAL"); // 
            }
            Parameters[19] = "";
            Parameters[20] = chkHold.IsChecked == true ? "Y" : "N";

            this.FrameOperation.OpenMenuFORM("SFU010705052", "FCS001_005_02", "LGC.GMES.MES.FCS001", ObjectDic.Instance.GetObjectName("Tray List"), true, Parameters);
        }

        private void dgWipbyLot_LoadedCellPresenter(object sender, DataGridCellEventArgs e)
        {
            dgWipbyLot.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (e.Cell.Presenter == null) return;

                //Foreground Color
                string spcl = Util.GetCondition(cboSpecial, bAllNull: true);
                int endRow = dgWipbyLot.Rows.Count - 1;
                switch (spcl)
                {
                    case "Y":

                        if (e.Cell.Column.Index >= dgWipbyLot.Columns["WAITTRAY"].Index && e.Cell.Row.Index < endRow)
                        {
                            e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Red);
                            e.Cell.Presenter.FontWeight = FontWeights.Regular;
                        }
                        break;

                    case "I":
                        if (e.Cell.Column.Index >= dgWipbyLot.Columns["WAITTRAY"].Index && e.Cell.Row.Index < endRow)
                        {
                            e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.DarkOrange);
                            e.Cell.Presenter.FontWeight = FontWeights.Regular;
                        }
                        break;

                    default:
                        if (e.Cell.Column.Index >= dgWipbyLot.Columns["WAITTRAY"].Index && e.Cell.Row.Index < endRow)
                        {
                            e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Blue);
                            e.Cell.Presenter.FontWeight = FontWeights.Regular;
                        }
                        break;
                }

                if ((e.Cell.Column.Name.Equals("TROUBLECELL") || e.Cell.Column.Name.Equals("TROUBLETRAY")) && e.Cell.Row.Index < endRow)
                {
                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.DarkViolet);
                    e.Cell.Presenter.FontWeight = FontWeights.Regular;
                }
            }));
        }

        private void dgWipbyLot_UnloadedCellPresenter(object sender, DataGridCellEventArgs e)
        {
            if (sender == null) return;

            C1DataGrid dataGrid = sender as C1DataGrid;
            if (e.Cell.Presenter != null)
            {
                if (e.Cell.Row.Type == DataGridRowType.Item)
                {
                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Black);
                    e.Cell.Presenter.FontWeight = FontWeights.Normal;
                    e.Cell.Presenter.Background = null;
                }
            }
        }

        private void dgWipbyLot_LoadedRowHeaderPresenter(object sender, C1.WPF.DataGrid.DataGridRowEventArgs e)
        {
            if (e.Row.HeaderPresenter == null)
            {
                return;
            }

            e.Row.HeaderPresenter.Content = null;
            C1DataGrid dataGrid = e.Row.DataGrid as C1DataGrid;
            TextBlock tb = new TextBlock();

            if (e.Row.Index + 1 - dataGrid.TopRows.Count > 0 && e.Row.Index != dataGrid.Rows.Count - 1)
            {
                tb.Text = (e.Row.Index + 1 - dataGrid.TopRows.Count).ToString();
                tb.VerticalAlignment = VerticalAlignment.Center;
                tb.HorizontalAlignment = HorizontalAlignment.Center;
                e.Row.HeaderPresenter.Content = tb;
            }
        }

        private void dgWipbyLot_ExecuteDataCompleted(object sender, CMM001.Controls.UcBaseDataGrid.ExecuteDataEventArgs e)
        {
            try
            {
                btnSearch.IsEnabled = true;

                string[] sColumnName = new string[] { "MDLLOT_ID", "MODEL_NAME" };
                _Util.SetDataGridMergeExtensionCol(dgWipbyLot, sColumnName, DataGridMergeMode.VERTICALHIERARCHI);

                if (!dgWipbyLot.IsUserConfigUsing)
                {
                    dgWipbyLot.Columns.Where(w => !w.Width.IsStar).ToList()
                        .ForEach(x => x.Width = x is DataGridNumericColumn ? new C1.WPF.DataGrid.DataGridLength(100) : new C1.WPF.DataGrid.DataGridLength(1, DataGridUnitType.Auto));
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        #endregion

     }
}
