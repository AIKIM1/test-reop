/*************************************************************************************
 Created Date : 2020.07.23
      Creator : 염규범
   Decription : CST 라벨 발행
--------------------------------------------------------------------------------------
 [Change History]
  2020.09.09  염규범S : Initial Created.
  2022.04.22  정용석  : Recreated.
  2022.05.04  정용석  : 카세트 라벨 발행시에 Mapping된 LOT의 포함여부 확인 Popup 추가.
  2022.05.04  정용석  : UI Operation 오류 수정.
  2025.03.25  윤주일  : MES 2.0 적용에 따른 정보 생성 DA(MSSQL)를 BR(ORACLE)로 변경
**************************************************************************************/
using C1.C1Report;
using C1.WPF;
using C1.WPF.C1Report;
using C1.WPF.DataGrid;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using LGC.GMES.MES.PACK001.Class;
using System;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;

namespace LGC.GMES.MES.PACK001
{
    public partial class PACK001_071 : UserControl, IWorkArea
    {
        #region Member Variable Lists...
        private PACK001_071_DataHelper dataHelper = new PACK001_071_DataHelper();
        private bool isLOTInclude = true;

        public IFrameOperation FrameOperation
        {
            get;
            set;
        }
        #endregion

        #region Declaration & Constructor
        public PACK001_071()
        {
            InitializeComponent();
        }
        #endregion

        #region Member Function Lists...
        private void Initilize()
        {
            this.dtpFromDate.ApplyTemplate();
            this.dtpToDate.ApplyTemplate();
            this.dtpFromDate.SelectedDateTime = DateTime.Now.AddDays(-3);
            this.dtpToDate.SelectedDateTime = DateTime.Now;

            PackCommon.SetC1ComboBox(this.dataHelper.GetAreaInfo(), this.cboAreaID, "ALL");
            PackCommon.SetC1ComboBox(this.dataHelper.GetCommonCodeInfo("USE_FLAG"), this.cboUseFlag, "ALL");
            PackCommon.SearchRowCount(ref this.txtPanCakeGRIDCount, 0);
            PackCommon.SearchRowCount(ref this.txtLotListCount, 0);

            this.isLOTInclude = true;
            this.rdoPrintPanCakeIDWithLOTList.IsChecked = true;
            this.rdoPrintPanCakeIDOnly.IsChecked = false;
        }

        private void InitializePopup()
        {
            this.txtPopupTitle.Text = string.Empty;
            this.popupLabelPrintCheck.IsOpen = false;
            this.popupLabelPrintCheck.HorizontalOffset = 0;
            this.popupLabelPrintCheck.VerticalOffset = 0;
        }

        private void SearchProcess()
        {
            DateTime fromDate = this.dtpFromDate.SelectedDateTime.Date;
            DateTime toDate = this.dtpToDate.SelectedDateTime.Date;

            if (fromDate > toDate)
            {
                Util.MessageValidation("SFU3569");  // 조회 시작일자는 종료일자를 초과 할 수 없습니다.
                this.dtpFromDate.Focus();
                return;
            }

            this.loadingIndicator.Visibility = Visibility.Visible;
            PackCommon.DoEvents();
            try
            {
                this.txtSelectedPanCakeGRID.Text = string.Empty;
                Util.gridClear(this.dgPanCakeGRID);
                Util.gridClear(this.dgLOTList);
                PackCommon.SearchRowCount(ref this.txtPanCakeGRIDCount, 0);
                PackCommon.SearchRowCount(ref this.txtLotListCount, 0);

                string areaID = this.cboAreaID.SelectedValue.ToString().Equals("ALL") ? string.Empty : this.cboAreaID.SelectedValue.ToString();
                string useFlag = this.cboUseFlag.SelectedValue.ToString().Equals("ALL") ? string.Empty : this.cboUseFlag.SelectedValue.ToString();
                string panCakeGroupID = string.IsNullOrEmpty(this.txtPanCakeGRID.Text) ? string.Empty : this.txtPanCakeGRID.Text;

                DataTable dt = this.dataHelper.GetPanCakeGroupIDList(areaID, fromDate, toDate, panCakeGroupID, useFlag);
                if (CommonVerify.HasTableRow(dt))
                {
                    Util.GridSetData(this.dgPanCakeGRID, dt, FrameOperation);
                    PackCommon.SearchRowCount(ref this.txtPanCakeGRIDCount, dt.Rows.Count);
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                this.loadingIndicator.Visibility = Visibility.Collapsed;
            }

            this.loadingIndicator.Visibility = Visibility.Collapsed;
        }

        private void CreateProcess()
        {
            this.dataHelper.CreateNewPanCakeGroupID(LoginInfo.CFG_AREA_ID, LoginInfo.USERID);
            this.SearchProcess();
        }

        private void SaveProcess()
        {
            try
            {
                if (this.dgPanCakeGRID.ItemsSource == null || !CommonVerify.HasTableRow(DataTableConverter.Convert(this.dgPanCakeGRID.ItemsSource)))
                {
                    Util.MessageValidation("SFU1226");
                    return;
                }

                DataTable dt = new DataTable("RQSTDT");
                dt.Columns.Add("PANCAKE_GR_ID", typeof(string));
                dt.Columns.Add("AREAID", typeof(string));
                dt.Columns.Add("USE_FLAG", typeof(string));
                dt.Columns.Add("UPDUSER", typeof(string));

                foreach (object modified in this.dgPanCakeGRID.GetModifiedItems())
                {
                    DataRow dr = dt.NewRow();
                    dr["PANCAKE_GR_ID"] = DataTableConverter.GetValue(modified, "PANCAKE_GR_ID");
                    dr["AREAID"] = DataTableConverter.GetValue(modified, "AREAID");
                    dr["USE_FLAG"] = DataTableConverter.GetValue(modified, "USE_FLAG");
                    dr["UPDUSER"] = LoginInfo.USERID;
                    dt.Rows.Add(dr);
                }

                if (!CommonVerify.HasTableRow(dt))
                {
                    Util.MessageValidation("SFU1226");
                    return;
                }

                if (this.dataHelper.UpdatePanCakeGroupID(dt))
                {
                    Util.MessageInfo("SFU3532");
                    this.SearchProcess();
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void SetSelectedPancakeID(C1DataGrid c1DataGrid)
        {
            try
            {
                int indexRow = c1DataGrid.CurrentRow.Index;
                this.txtSelectedPanCakeGRID.Text = Util.NVC(DataTableConverter.GetValue(c1DataGrid.Rows[indexRow].DataItem, "PANCAKE_GR_ID"));
                this.SearchDetail(this.txtSelectedPanCakeGRID.Text);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void SearchDetail(string selectedPanCakeGroupID)
        {
            this.loadingIndicator.Visibility = Visibility.Visible;
            PackCommon.DoEvents();
            try
            {
                Util.gridClear(this.dgLOTList);
                PackCommon.SearchRowCount(ref this.txtLotListCount, 0);

                DataTable dt = this.dataHelper.GetPanCakeGroupIDMappingLOTList(selectedPanCakeGroupID);
                if (CommonVerify.HasTableRow(dt))
                {
                    Util.GridSetData(this.dgLOTList, dt, FrameOperation);
                    PackCommon.SearchRowCount(ref this.txtLotListCount, dt.Rows.Count);
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                this.loadingIndicator.Visibility = Visibility.Collapsed;
            }

            this.loadingIndicator.Visibility = Visibility.Collapsed;
        }

        private void PreviewProcess()
        {
            try
            {
                CST_Prt frmCasettePrint = new CST_Prt();
                frmCasettePrint.FrameOperation = this.FrameOperation;

                if (frmCasettePrint != null)
                {
                    // 태그 발행 창 화면에 띄움.
                    object[] obj = new object[3];
                    obj[0] = this.isLOTInclude;
                    obj[1] = this.dataHelper.GetPanCakeGroupIDList(this.txtSelectedPanCakeGRID.Text);
                    obj[2] = this.dataHelper.GetPanCakeGroupIDMappingLOTList(this.txtSelectedPanCakeGRID.Text);

                    C1WindowExtension.SetParameters(frmCasettePrint, obj);
                    this.Dispatcher.BeginInvoke(new Action(() => frmCasettePrint.Show()));
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void PrintProcess()
        {
            try
            {
                CST_Prt frmCasettePrint = new CST_Prt();
                frmCasettePrint.FrameOperation = this.FrameOperation;

                if (frmCasettePrint != null)
                {
                    frmCasettePrint.ISLOTINCLUDE = this.isLOTInclude;
                    frmCasettePrint.HEADERDATA = this.dataHelper.GetPanCakeGroupIDList(this.txtSelectedPanCakeGRID.Text);
                    frmCasettePrint.DETAILDATA = this.dataHelper.GetPanCakeGroupIDMappingLOTList(this.txtSelectedPanCakeGRID.Text);

                    C1Report c1Report = frmCasettePrint.PrintBatchProcess();
                    C1DocumentViewer c1DocumentViewer = new C1DocumentViewer();
                    c1DocumentViewer.Document = c1Report.C1Document.FixedDocumentSequence;
                    c1DocumentViewer.Print();
                    frmCasettePrint.Close();
                }

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void HidePopUp()
        {
            this.InitializePopup();
        }
        #endregion

        #region Event Lists...
        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            PackCommon.SetPopupDraggable(this.popupLabelPrintCheck, this.pnlPopUpTitle);
            this.Initilize();
            this.Loaded -= new RoutedEventHandler(this.UserControl_Loaded);
        }

        private void btnCreate_Click(object sender, RoutedEventArgs e)
        {
            this.CreateProcess();
        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            this.SaveProcess();
        }

        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            this.SearchProcess();
        }

        private void dgPanCakeGRID_BeginningNewRow(object sender, DataGridBeginningNewRowEventArgs e)
        {

        }

        private void dgPanCakeGRID_BeginningEdit(object sender, C1.WPF.DataGrid.DataGridBeginningEditEventArgs e)
        {
        }

        private void dgPanCakeGRID_BeganEdit(object sender, DataGridBeganEditEventArgs e)
        {
            C1ComboBox c1ComboBox = (C1ComboBox)e.EditingElement;
            if (c1ComboBox == null)
            {
                return;
            }

            PackCommon.SetC1ComboBox(this.dataHelper.GetCommonCodeInfo("USE_FLAG"), c1ComboBox, string.Empty);
        }

        private void dgPanCakeGRID_CommittedEdit(object sender, DataGridCellEventArgs e)
        {
            C1DataGrid dg = (C1DataGrid)sender;

            if (dg.CurrentCell.IsEditing)
            {
                return;
            }

            switch (dg.CurrentCell.Column.Name)
            {
                case "USE_FLAG":
                    break;
                default:
                    break;
            }

        }

        private void dgPanCakeGRID_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            this.SetSelectedPancakeID((C1DataGrid)sender);
        }

        private void btnLabelPrint_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(this.txtSelectedPanCakeGRID.Text))
            {
                return;
            }

            this.txtPopupTitle.Text = ObjectDic.Instance.GetObjectName("SELECT_LABEL_PRINT_OPTION") + "(" + this.txtSelectedPanCakeGRID.Text + ")"; // Popup 제목 땡기고
            this.popupLabelPrintCheck.Placement = PlacementMode.Center;                                                                             // 화면 정가운데에다가
            this.popupLabelPrintCheck.IsOpen = true;                                                                                                // Popup 표출
        }

        private void Image_MouseLeftButtonUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            this.HidePopUp();
        }

        private void popupBaseInfo_LostFocus(object sender, RoutedEventArgs e)
        {
            this.popupLabelPrintCheck.StaysOpen = true;
        }

        private void btnPreview_Click(object sender, RoutedEventArgs e)
        {
            this.HidePopUp();
            this.PreviewProcess();
        }

        private void btnPrint_Click(object sender, RoutedEventArgs e)
        {
            this.PrintProcess();
            this.HidePopUp();
        }

        private void btnHide_Click(object sender, RoutedEventArgs e)
        {
            this.HidePopUp();
        }

        private void rdoPrintPanCakeIDWithLOTList_Checked(object sender, RoutedEventArgs e)
        {
            // 라디오버튼 이상함.
            RadioButton radioButton = (RadioButton)sender;

            if (radioButton.Name.ToUpper().Equals("RDOPRINTPANCAKEIDWITHLOTLIST") && radioButton.IsChecked == true)
            {
                this.rdoPrintPanCakeIDOnly.IsChecked = false;
                this.isLOTInclude = true;
                return;
            }

            if (radioButton.Name.ToUpper().Equals("RDOPRINTPANCAKEIDONLY") && radioButton.IsChecked == true)
            {
                this.rdoPrintPanCakeIDWithLOTList.IsChecked = false;
                this.isLOTInclude = false;
                return;
            }

        }
        #endregion
    }

    public class PACK001_071_DataHelper
    {
        #region Member Variable Lists...
        #endregion

        #region Constructor
        public PACK001_071_DataHelper()
        {
        }
        #endregion

        #region Member Function Lists...
        // 순서도 호출 - 사용여부
        public DataTable GetCommonCodeInfo(string cmcdType)
        {
            DataTable dtReturn = new DataTable();
            try
            {
                string bizRuleName = "DA_BAS_SEL_COMMONCODE_ATTRIBUTES";
                DataTable dtRQSTDT = new DataTable("RQSTDT");
                DataTable dtRSLTDT = new DataTable("RSLTDT");

                dtRQSTDT.Columns.Add("LANGID", typeof(string));
                dtRQSTDT.Columns.Add("CMCDTYPE", typeof(string));
                dtRQSTDT.Columns.Add("ATTRIBUTE1", typeof(string));
                dtRQSTDT.Columns.Add("ATTRIBUTE2", typeof(string));
                dtRQSTDT.Columns.Add("ATTRIBUTE3", typeof(string));
                dtRQSTDT.Columns.Add("ATTRIBUTE4", typeof(string));
                dtRQSTDT.Columns.Add("ATTRIBUTE5", typeof(string));

                DataRow drRQSTDT = dtRQSTDT.NewRow();
                drRQSTDT["LANGID"] = LoginInfo.LANGID;
                drRQSTDT["CMCDTYPE"] = cmcdType;
                drRQSTDT["ATTRIBUTE1"] = DBNull.Value;
                drRQSTDT["ATTRIBUTE2"] = DBNull.Value;
                drRQSTDT["ATTRIBUTE3"] = DBNull.Value;
                drRQSTDT["ATTRIBUTE4"] = DBNull.Value;
                drRQSTDT["ATTRIBUTE5"] = DBNull.Value;
                dtRQSTDT.Rows.Add(drRQSTDT);

                dtRSLTDT = new ClientProxy().ExecuteServiceSync(bizRuleName, dtRQSTDT.TableName, dtRSLTDT.TableName, dtRQSTDT);
                if (CommonVerify.HasTableRow(dtRSLTDT))
                {
                    dtReturn = dtRSLTDT.Copy();
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }

            return dtReturn;
        }

        // 순서도 호출 - 동
        public DataTable GetAreaInfo()
        {
            DataTable dtReturn = new DataTable();
            try
            {
                string bizRuleName = "DA_BAS_SEL_AREA_BY_AREATYPE_CBO";
                DataTable dtRQSTDT = new DataTable("RQSTDT");
                DataTable dtRSLTDT = new DataTable("RSLTDT");

                dtRQSTDT.Columns.Add("LANGID", typeof(string));
                dtRQSTDT.Columns.Add("SHOPID", typeof(string));
                dtRQSTDT.Columns.Add("AREA_TYPE_CODE", typeof(string));

                DataRow drRQSTDT = dtRQSTDT.NewRow();
                drRQSTDT["LANGID"] = LoginInfo.LANGID;
                drRQSTDT["SHOPID"] = LoginInfo.CFG_SHOP_ID;
                drRQSTDT["AREA_TYPE_CODE"] = Area_Type.PACK;
                dtRQSTDT.Rows.Add(drRQSTDT);

                dtRSLTDT = new ClientProxy().ExecuteServiceSync(bizRuleName, dtRQSTDT.TableName, dtRSLTDT.TableName, dtRQSTDT);
                if (CommonVerify.HasTableRow(dtRSLTDT))
                {
                    dtReturn = dtRSLTDT.Copy();
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }

            return dtReturn;
        }

        // 순서도 호출 - 구루마 조회 2호
        public DataTable GetPanCakeGroupIDList(string panCakeGroupID)
        {
            DataTable dtReturn = new DataTable();

            try
            {
                string bizRuleName = "DA_PRD_SEL_PANCAKE_GR_ID";
                DataTable dtRQSTDT = new DataTable("RQSTDT");
                DataTable dtRSLTDT = new DataTable("RSLTDT");

                dtRQSTDT.Columns.Add("LANGID", typeof(string));
                dtRQSTDT.Columns.Add("AREAID", typeof(string));
                dtRQSTDT.Columns.Add("PANCAKE_GR_ID", typeof(string));
                dtRQSTDT.Columns.Add("USE_FLAG", typeof(string));
                dtRQSTDT.Columns.Add("FROMDATE", typeof(DateTime));
                dtRQSTDT.Columns.Add("TODATE", typeof(DateTime));

                DataRow drRQSTDT = dtRQSTDT.NewRow();
                drRQSTDT["LANGID"] = LoginInfo.LANGID;
                drRQSTDT["AREAID"] = null;
                drRQSTDT["PANCAKE_GR_ID"] = string.IsNullOrEmpty(panCakeGroupID) ? null : panCakeGroupID;
                drRQSTDT["USE_FLAG"] = null;
                drRQSTDT["FROMDATE"] = DBNull.Value;
                drRQSTDT["TODATE"] = DBNull.Value;
                dtRQSTDT.Rows.Add(drRQSTDT);

                dtRSLTDT = new ClientProxy().ExecuteServiceSync(bizRuleName, dtRQSTDT.TableName, dtRSLTDT.TableName, dtRQSTDT);
                if (CommonVerify.HasTableRow(dtRSLTDT))
                {
                    dtReturn = dtRSLTDT.Copy();
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }

            return dtReturn;
        }

        // 순서도 호출 - 구루마 조회 1호
        public DataTable GetPanCakeGroupIDList(string areaID, DateTime dteFromDate, DateTime dteToDate, string panCakeGroupID, string useFlag)
        {
            DataTable dtReturn = new DataTable();

            try
            {
                string bizRuleName = "DA_PRD_SEL_PANCAKE_GR_ID";
                DataTable dtRQSTDT = new DataTable("RQSTDT");
                DataTable dtRSLTDT = new DataTable("RSLTDT");

                dtRQSTDT.Columns.Add("LANGID", typeof(string));
                dtRQSTDT.Columns.Add("AREAID", typeof(string));
                dtRQSTDT.Columns.Add("PANCAKE_GR_ID", typeof(string));
                dtRQSTDT.Columns.Add("USE_FLAG", typeof(string));
                dtRQSTDT.Columns.Add("FROMDATE", typeof(DateTime));
                dtRQSTDT.Columns.Add("TODATE", typeof(DateTime));

                DataRow drRQSTDT = dtRQSTDT.NewRow();
                drRQSTDT["LANGID"] = LoginInfo.LANGID;
                drRQSTDT["AREAID"] = string.IsNullOrEmpty(areaID) ? null : areaID;
                drRQSTDT["PANCAKE_GR_ID"] = string.IsNullOrEmpty(panCakeGroupID) ? null : panCakeGroupID;
                drRQSTDT["USE_FLAG"] = string.IsNullOrEmpty(useFlag) ? null : useFlag;
                drRQSTDT["FROMDATE"] = dteFromDate;
                drRQSTDT["TODATE"] = dteToDate.AddDays(1);
                dtRQSTDT.Rows.Add(drRQSTDT);

                dtRSLTDT = new ClientProxy().ExecuteServiceSync(bizRuleName, dtRQSTDT.TableName, dtRSLTDT.TableName, dtRQSTDT);
                if (CommonVerify.HasTableRow(dtRSLTDT))
                {
                    dtReturn = dtRSLTDT.Copy();
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }

            return dtReturn;
        }

        // 순서도 호출 - 구루마와 매핑된 LOT 조회
        public DataTable GetPanCakeGroupIDMappingLOTList(string panCakeGroupID)
        {
            DataTable dtReturn = new DataTable();

            try
            {
                string bizRuleName = "DA_PRD_SEL_PANCAKE_GR_ID_MAPPING_LOT";
                DataTable dtRQSTDT = new DataTable("RQSTDT");
                DataTable dtRSLTDT = new DataTable("RSLTDT");

                dtRQSTDT.Columns.Add("LANGID", typeof(string));
                dtRQSTDT.Columns.Add("PANCAKE_GR_ID", typeof(string));

                DataRow drRQSTDT = dtRQSTDT.NewRow();
                drRQSTDT["LANGID"] = LoginInfo.LANGID;
                drRQSTDT["PANCAKE_GR_ID"] = string.IsNullOrEmpty(panCakeGroupID) ? null : panCakeGroupID;
                dtRQSTDT.Rows.Add(drRQSTDT);

                dtRSLTDT = new ClientProxy().ExecuteServiceSync(bizRuleName, dtRQSTDT.TableName, dtRSLTDT.TableName, dtRQSTDT);
                if (CommonVerify.HasTableRow(dtRSLTDT))
                {
                    dtReturn = dtRSLTDT.Copy();
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }

            return dtReturn;
        }

        // 순서도 호출 - 구루마 신규 생성
        public DataTable CreateNewPanCakeGroupID(string areaID, string userID)
        {
            DataTable dtReturn = new DataTable();

            try
            {
                //string bizRuleName = "DA_PRD_INS_PANCAKE_GR_ID";
                string bizRuleName = "BR_PRD_INS_PANCAKE_GR_ID"; // mes 2.0 적용에 따른 변경
                DataTable dtRQSTDT = new DataTable("RQSTDT");
                DataTable dtRSLTDT = new DataTable("RSLTDT");

                dtRQSTDT.Columns.Add("AREAID", typeof(string));
                dtRQSTDT.Columns.Add("USERID", typeof(string));

                DataRow drRQSTDT = dtRQSTDT.NewRow();
                drRQSTDT["AREAID"] = areaID;
                drRQSTDT["USERID"] = userID;
                dtRQSTDT.Rows.Add(drRQSTDT);

                dtRSLTDT = new ClientProxy().ExecuteServiceSync(bizRuleName, dtRQSTDT.TableName, dtRSLTDT.TableName, dtRQSTDT);
                if (CommonVerify.HasTableRow(dtRSLTDT))
                {
                    dtReturn = dtRSLTDT.Copy();
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }

            return dtReturn;
        }

        // 순서도 호출 - 구루마 속성값 변경
        public bool UpdatePanCakeGroupID(DataTable dt)
        {
            bool returnValue = false;

            try
            {
                string bizRuleName = "DA_PRD_UPD_SFC_PANCAKE_GR_ID_FOR_PACK";
                DataTable dtRQSTDT = new DataTable("RQSTDT");
                DataTable dtRSLTDT = new DataTable("RSLTDT");

                dtRSLTDT = new ClientProxy().ExecuteServiceSync(bizRuleName, dtRQSTDT.TableName, dtRSLTDT.TableName, dt);
                returnValue = true;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                returnValue = false;
            }

            return returnValue;
        }
        #endregion
    }
}