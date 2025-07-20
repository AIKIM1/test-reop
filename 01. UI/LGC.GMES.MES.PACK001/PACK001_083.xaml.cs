/*************************************************************************************
 Created Date : 2021.07.13
      Creator : 김건식
   Decription : 장기재고 의뢰검사

--------------------------------------------------------------------------------------
 [Change History]
 2021.07.13  |  김건식  | 최초작성
 
**************************************************************************************/

using C1.WPF;
using C1.WPF.DataGrid;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using LGC.GMES.MES.PACK001.Class;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace LGC.GMES.MES.PACK001
{
    /// <summary>
    /// PACK001_083.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class PACK001_083 : UserControl, IWorkArea
    {
        #region Declaration & Constructor 

        CommonCombo _combo = new CMM001.Class.CommonCombo();
        LGC.GMES.MES.PACK001.Class.MESSAGE_PARAM ms = new LGC.GMES.MES.PACK001.Class.MESSAGE_PARAM();
        #endregion

        public PACK001_083()
        {
            InitializeComponent();
        }

        public IFrameOperation FrameOperation
        {
            get;
            set;
        }


        #region Initialize

        #endregion

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            dtpHistDateFrom.SelectedDateTime = (DateTime)System.DateTime.Now.AddDays(-7);
            dtpHistDateTo.SelectedDateTime = (DateTime)System.DateTime.Now;

            setComboBox();

            tbPossibility.Text = ObjectDic.Instance.GetObjectName("전체선택") + " : " + ObjectDic.Instance.GetObjectName("대기") + ", " + ObjectDic.Instance.GetObjectName("불합격") + ", " + ObjectDic.Instance.GetObjectName("합격");
        }

        private void setComboBox()
        {
            String[] sFilter = { LoginInfo.CFG_SHOP_ID, Area_Type.PACK };
            string[] Tab1_sFilterJUDGE = { "QMS_PQC_JUDG_VALUE" };
            string[] Tab2_sFilterJUDGE = { "QMS_INSP_JUDGE_VALUE" };

            #region 정보전송조회
            C1ComboBox cboShop = new C1ComboBox();
            cboShop.SelectedValue = LoginInfo.CFG_SHOP_ID;
            C1ComboBox cboAREA_TYPE_CODE = new C1ComboBox();
            cboAREA_TYPE_CODE.SelectedValue = Area_Type.PACK;

            //동           
            C1ComboBox[] cboAreaParent = { cboShop };
            C1ComboBox[] cboAreaChild = { cboEquipmentSegment };            
            _combo.SetCombo(cboArea, CommonCombo.ComboStatus.NONE, cbChild: cboAreaChild);

            //라인            
            C1ComboBox[] cboEquipmentSegmentParent = { cboArea };
            C1ComboBox[] cboEquipmentSegmentChild = { cboProductModel };
            _combo.SetCombo(cboEquipmentSegment, CommonCombo.ComboStatus.NONE, cbParent: cboEquipmentSegmentParent, cbChild: cboEquipmentSegmentChild);

            //모델     
            C1ComboBox[] cboProductModelParent = { cboArea, cboEquipmentSegment };
            C1ComboBox[] cboProductModelChild = { cboProduct };
            _combo.SetCombo(cboProductModel, CommonCombo.ComboStatus.ALL, cbParent: cboProductModelParent, cbChild: cboProductModelChild, sCase: "PRJ_MODEL");

            //제품코드  
            C1ComboBox[] cboProductParent = { cboShop, cboArea, cboEquipmentSegment, cboProductModel };
            _combo.SetCombo(cboProduct, CommonCombo.ComboStatus.ALL, cbParent: cboProductParent, sCase: "PRJ_PRODUCT");

            //판졍상태
            _combo.SetCombo(cboQccode, CommonCombo.ComboStatus.ALL, sFilter: Tab1_sFilterJUDGE, sCase: "COMMCODE");


            #endregion

            #region 검사이력조회

            //동           
            C1ComboBox[] cboHistAreaParent = { cboShop };
            C1ComboBox[] cboHistAreaChild = { cboHistEquipmentSegment };            
            _combo.SetCombo(cboHistArea, CommonCombo.ComboStatus.NONE, cbChild: cboHistAreaChild);

            //라인            
            C1ComboBox[] cboHistEquipmentSegmentParent = { cboHistArea };
            C1ComboBox[] cboHistEquipmentSegmentChild = { cboHistModel };
            _combo.SetCombo(cboHistEquipmentSegment, CommonCombo.ComboStatus.NONE, cbParent: cboHistEquipmentSegmentParent, cbChild: cboHistEquipmentSegmentChild);

            //모델     
            C1ComboBox[] cboHistProductModelParent = { cboHistArea, cboHistEquipmentSegment };
            C1ComboBox[] cboHistProductModelChild = { cboHistProduct };
            _combo.SetCombo(cboHistModel, CommonCombo.ComboStatus.ALL, cbParent: cboHistProductModelParent, cbChild: cboHistProductModelChild, sCase: "PRJ_MODEL");

            //제품코드  
            C1ComboBox[] cboHistProductParent = { cboShop, cboHistArea, cboHistEquipmentSegment, cboHistModel };
            _combo.SetCombo(cboHistProduct, CommonCombo.ComboStatus.ALL, cbParent: cboHistProductParent, sCase: "PRJ_PRODUCT");

            //판졍결과
            _combo.SetCombo(cboHistJudge, CommonCombo.ComboStatus.ALL, sFilter: Tab2_sFilterJUDGE, sCase: "COMMCODE");
            #endregion
        }

        #region Event - [버튼]
        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
           // 날짜 30일 제한
            TimeSpan timeSpan = dtpREQDateTo.SelectedDateTime.Date - dtpREQDateFrom.SelectedDateTime.Date;

            if (timeSpan.Days < 0)
            {
                Util.MessageValidation("SFU3569");  //조회 시작일자는 종료일자를 초과 할 수 없습니다.
                return;
            }

            if (timeSpan.Days > 30)
            {
                Util.MessageValidation("SFU4466");  //조회기간은 30일을 초과 할 수 없습니다.
                return;
            }
            Util.gridClear(dgTargetList);
            Util.gridClear(dgLotInfo);
            GetTargetLotInfo();
        }

        private void btnHistSearch_Click(object sender, RoutedEventArgs e)
        {
            //날짜 30일 제한
            TimeSpan timeSpan = dtpHistDateTo.SelectedDateTime.Date - dtpHistDateFrom.SelectedDateTime.Date;

            if (timeSpan.Days < 0)
            {
                Util.MessageValidation("SFU3569");  //조회 시작일자는 종료일자를 초과 할 수 없습니다.
                return;
            }

            if (timeSpan.Days > 30)
            {
                Util.MessageValidation("SFU4466");  //조회기간은 30일을 초과 할 수 없습니다.
                return;
            }

            GetReqHist();
        }

        private void btnPQCRequest_Click(object sender, RoutedEventArgs e)
        {

            LGC.GMES.MES.ControlsLibrary.MessageBox.Show(ms.AlertRetun("SFU4140"), null, "Information", MessageBoxButton.OKCancel, MessageBoxIcon.None, (result) =>
            // 검사의뢰하시겠습니까?
            {
                if (result == MessageBoxResult.OK)
                {
                    PQCRequest();//선택 체크 팝업 오픈 오픈 close 시 조회및 입고처리.
                }
            }
            );

            
        }

        private void btnDel_Click(object sender, RoutedEventArgs e)
        {
            // 작업자에게 다시 한 번 삭제 여부 묻기
            Button btn = sender as Button;
            int iRow = ((C1.WPF.DataGrid.DataGridCellPresenter)btn.Parent).Row.Index;
            //System.Data.DataRowView row = btn.DataContext as System.Data.DataRowView;

            string SLOTID = Util.NVC(dgTargetList.GetCell(iRow, dgTargetList.Columns["LOTID"].Index).Value);
            //삭제시 Pallet 조회 시트상 해당 PalletID 체크박스 해제
            for (int i = 0; i < dgLotInfo.GetRowCount(); i++)
            {
                if (SLOTID == Util.NVC(dgLotInfo.GetCell(i, dgLotInfo.Columns["LOTID"].Index).Value))
                {
                    DataTableConverter.SetValue(dgLotInfo.Rows[i].DataItem, "CHK", false);
                }
            }
            // 선택된 행 삭제
            dgTargetList.IsReadOnly = false;
            dgTargetList.CanUserRemoveRows = true;
            //dgTargetList.BeginEdit();
            dgTargetList.RemoveRow(iRow);
            dgTargetList.EndEdit();
            dgTargetList.CanUserRemoveRows = false;
            dgTargetList.IsReadOnly = true;

            Util.SetTextBlockText_DataGridRowCount(tbCCount, Util.NVC(dgLotInfo.GetRowCount()));
            Util.SetTextBlockText_DataGridRowCount(tbTargetListCount, Util.NVC(dgTargetList.GetRowCount()));
        }

        private void MoveLOTFromLOTGridToPQCRequestGrid()
        {
            if (this.dgLotInfo.GetRowCount() <= 0)
            {
                return;
            }

            DataTable dtSource = DataTableConverter.Convert(this.dgLotInfo.ItemsSource);
            DataTable dtTaget = new DataTable();
            
            dtSource.Select("CHK = 'False' AND QC_CHECK_STATUS IN('WAIT','FAIL','SUCESS')").ToList<DataRow>().ForEach(r => r["CHK"] = true);
            this.dgLotInfo.ItemsSource = DataTableConverter.Convert(dtSource);
            dtTaget = dtSource;
            dtTaget.Select("CHK = 'False'").ToList<DataRow>().ForEach(row => row.Delete());
            // 전송 정보 Grid에 Insert
            this.dgTargetList.ItemsSource = DataTableConverter.Convert(dtTaget);
        }

        private void btnSelect_All_Click(object sender, RoutedEventArgs e)
        {
            // 2022-04-15 : 전체선택시에 진행상태가 진행중인거나 공란인 것을 제외한 모든 것들이 전송항목으로 이동가능하게 수정.
            try
            {
                this.MoveLOTFromLOTGridToPQCRequestGrid();
                PackCommon.SearchRowCount(ref this.tbTargetListCount, this.dgTargetList.GetRowCount());
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void btnTargetAllCancel_Click(object sender, RoutedEventArgs e)
        {
            for (int iRow = dgTargetList.Rows.Count - 1; iRow > -1; iRow--)
            {
                //System.Data.DataRowView row = btn.DataContext as System.Data.DataRowView;

                string sLOTID = Util.NVC(dgTargetList.GetCell(iRow, dgTargetList.Columns["LOTID"].Index).Value);
                //삭제시 Pallet 조회 시트상 해당 PalletID 체크박스 해제
                for (int i = 0; i < dgLotInfo.GetRowCount(); i++)
                {
                    if (sLOTID == Util.NVC(dgLotInfo.GetCell(i, dgLotInfo.Columns["LOTID"].Index).Value))
                    {
                        DataTableConverter.SetValue(dgLotInfo.Rows[i].DataItem, "CHK", false);
                        break;
                    }
                }
                // 선택된 행 삭제
                dgTargetList.IsReadOnly = false;
                dgTargetList.CanUserRemoveRows = true;
                //dgTargetList.BeginEdit();
                dgTargetList.RemoveRow(iRow);
                dgTargetList.EndEdit();
                dgTargetList.CanUserRemoveRows = false;
                dgTargetList.IsReadOnly = true;
            }
        }

        #endregion

        #region Event - [TEXT 박스]
        private void txtLOTID_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                string sLOTID = txtLOTID.Text.Trim();

                if (string.IsNullOrEmpty(sLOTID))
                {
                    Util.MessageInfo("SFU1190");  // 조회할 LOT ID 를 입력하세요.
                    return;
                }

                GetTargetLotInfo(sLOTID);
            }
        }

        private void txtLOTID_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.V && Keyboard.IsKeyDown(Key.LeftCtrl))
            {
                try
                {
                    string[] stringSeparators = new string[] { "\r\n" };
                    string sPasteString = Clipboard.GetText();
                    string[] sPasteStrings = sPasteString.Split(stringSeparators, StringSplitOptions.None);

                    if (sPasteStrings.Count() > 500)
                    {
                        Util.MessageValidation("SFU8102");   //최대 수량은 500개 입니다.
                        return;
                    }

                    string lotList = string.Empty;
                    for (int i = 0; i < sPasteStrings.Length; i++)
                    {
                        if (i == 0)
                        {
                            lotList = sPasteStrings[i];
                        }
                        else
                        {
                            lotList = lotList + "," + sPasteStrings[i];
                        }
                        System.Windows.Forms.Application.DoEvents();
                    }
                    GetTargetLotInfo(lotList);
                }
                catch (Exception ex)
                {
                    Util.MessageException(ex);
                    return;
                }

                e.Handled = true;
            }
        }

        private void txtHistLOTID_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                string sLOTID = txtHistLOTID.Text.Trim();

                if (string.IsNullOrEmpty(sLOTID))
                {
                    Util.MessageInfo("SFU1190");  // 조회할 LOT ID 를 입력하세요.
                    return;
                }

                GetReqHist(sLOTID);
            }
        }

        private void txtHistLOTID_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.V && Keyboard.IsKeyDown(Key.LeftCtrl))
            {
                try
                {
                    string[] stringSeparators = new string[] { "\r\n" };
                    string sPasteString = Clipboard.GetText();
                    string[] sPasteStrings = sPasteString.Split(stringSeparators, StringSplitOptions.None);

                    if (sPasteStrings.Count() > 500)
                    {
                        Util.MessageValidation("SFU8102");   //최대 수량은 500개 입니다.
                        return;
                    }

                    string lotList = string.Empty;
                    for (int i = 0; i < sPasteStrings.Length; i++)
                    {
                        if (i == 0)
                        {
                            lotList = sPasteStrings[i];
                        }
                        else
                        {
                            lotList = lotList + "," + sPasteStrings[i];
                        }
                        System.Windows.Forms.Application.DoEvents();
                    }
                    GetReqHist(lotList);
                }
                catch (Exception ex)
                {
                    Util.MessageException(ex);
                    return;
                }

                e.Handled = true;
            }
        }
        #endregion



        #region DataGrid
        private void common_LoadedCellPresenter(object sender, DataGridCellEventArgs e)
        {
            try
            {
                C1.WPF.DataGrid.C1DataGrid dataGrid = sender as C1.WPF.DataGrid.C1DataGrid;
                //C1DataGrid dataGrid = (sender as C1DataGrid);

                dataGrid.Dispatcher.BeginInvoke(new Action(() =>
                {
                    if (e.Cell.Presenter == null)
                    {
                        return;
                    }
                    //컬러
                    if (e.Cell.Column.Name != null && e.Cell.Column.Name.Equals("OVER_DAY"))
                    {
                        string sWarningColor = Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "WARNING_COLOR"));
                        if (sWarningColor.Equals("R"))
                        {
                            e.Cell.Presenter.Background = new SolidColorBrush(Colors.Red);
                            e.Cell.Presenter.FontWeight = FontWeights.Bold;
                        }
                        else if (sWarningColor.Equals("Y"))
                        {
                            e.Cell.Presenter.Background = new SolidColorBrush(Colors.Yellow);
                            e.Cell.Presenter.FontWeight = FontWeights.Bold;
                        }
                    }
                }));
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void dgLotInfo_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (dgLotInfo.CurrentRow == null || dgLotInfo.SelectedIndex == -1)
            {
                return;
            }
            else if (e.ChangedButton.ToString().Equals("Left") && dgLotInfo.CurrentColumn.Name == "CHK")
            {
                string sLOTID = string.Empty;
                try
                {
                    // Rows Count가 0보다 클 경우에만 이벤트 발생하도록
                    if (dgLotInfo.GetRowCount() > 0)
                    {
                        string sChkValue = Util.NVC(dgLotInfo.GetCell(dgLotInfo.CurrentRow.Index, dgLotInfo.Columns["CHK"].Index).Value);
                        sLOTID = Util.NVC(dgLotInfo.GetCell(dgLotInfo.CurrentRow.Index, dgLotInfo.Columns["LOTID"].Index).Value);

                        //MouseUp 이벤트 -> 체크 이벤트가 아니라.. 강제 체크
                        if (sChkValue == "True")
                        {
                            DataTableConverter.SetValue(dgLotInfo.Rows[dgLotInfo.CurrentRow.Index].DataItem, "CHK", false);
                            sChkValue = Util.NVC(dgLotInfo.GetCell(dgLotInfo.CurrentRow.Index, dgLotInfo.Columns["CHK"].Index).Value);
                        }
                        else
                        {
                            //string sISS_STAT_YN = Util.NVC(dgLotInfo.GetCell(dgLotInfo.CurrentRow.Index, dgLotInfo.Columns["ISS_STAT_YN"].Index).Value);
                            //if (sISS_STAT_YN.Equals("Y"))
                            //{
                            //    Util.MessageValidation("SFU8373"); // 반품된 LOT은 장기재고 검사 의뢰를 할수 없습니다.
                            //    return;
                            //}
                            DataTableConverter.SetValue(dgLotInfo.Rows[dgLotInfo.CurrentRow.Index].DataItem, "CHK", true);
                            sChkValue = Util.NVC(dgLotInfo.GetCell(dgLotInfo.CurrentRow.Index, dgLotInfo.Columns["CHK"].Index).Value);
                        }


                        if (sChkValue == "True")
                        {
                            int dgLotInfoRows = 0;
                            dgLotInfoRows = dgTargetList.GetRowCount();

                            //전송 정보 중복 여부 체크
                            if (dgLotInfoRows != 0)
                            {
                                for (int i = 0; i < dgLotInfoRows; i++)
                                {
                                    if (Util.NVC(dgTargetList.GetCell(i, dgTargetList.Columns["LOTID"].Index).Value) == sLOTID)
                                    {
                                        Util.MessageValidation("SFU1776"); //이미 전송 정보에 해당 Pallet ID가 존재합니다.
                                        return;
                                    }
                                }
                            }

                            SetDgTargetList(dgLotInfo.CurrentRow.Index);
                        }
                        else
                        {
                            int iwoListIndex = Util.gridFindDataRow(ref dgTargetList, "LOTID", sLOTID, false);
                            if (iwoListIndex >= 0)
                            {
                                DataTableConverter.SetValue(dgLotInfo.Rows[dgLotInfo.CurrentRow.Index].DataItem, "CHK", true);
                                Util.MessageValidation("SFU2015"); //해당 PALLETID를 전송정보 시트에서 삭제해 주세요.
                                return;
                            }
                        }

                        // 스캔된 마지막 셀이 바로 보이도록 스프레드 스크롤 하단으로 이동
                        if (dgTargetList.GetRowCount() > 0)
                            dgTargetList.ScrollIntoView(dgTargetList.GetRowCount() - 1, 0);

                        Util.SetTextBlockText_DataGridRowCount(tbTargetListCount, Util.NVC(dgTargetList.GetRowCount()));
                    }
                }
                catch (Exception ex)
                {
                    Util.AlertInfo(ex.Message);
                    return;
                }
                finally
                {
                    dgLotInfo.CurrentRow = null;
                }
            }
        }
        #endregion

        #region Method             

        private void GetTargetLotInfo(string sLotList = "")
        {
           try
            {

                string sINSP_MED_CLSS_CODE = "PQCM022"; // PACK 장기재고 중분류

                DataTable RQSTDT = new DataTable();
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("AREAID", typeof(string));
                RQSTDT.Columns.Add("EQSGID", typeof(string));
                RQSTDT.Columns.Add("PRODID", typeof(string));
                RQSTDT.Columns.Add("MODLID", typeof(string));
                RQSTDT.Columns.Add("FROM_DATE", typeof(string));
                RQSTDT.Columns.Add("TO_DATE", typeof(string));
                RQSTDT.Columns.Add("LOTID", typeof(string));
                RQSTDT.Columns.Add("INSP_MED_CLSS_CODE", typeof(string));
                RQSTDT.Columns.Add("QC_CODE", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                if (!string.IsNullOrEmpty(sLotList))
                {   
                    dr["LANGID"] = LoginInfo.LANGID;
                    dr["AREAID"] = Util.GetCondition(cboArea, "", true);
                    dr["EQSGID"] = Util.GetCondition(cboEquipmentSegment, "", true);
                    dr["LOTID"] = sLotList;
                    dr["INSP_MED_CLSS_CODE"] = sINSP_MED_CLSS_CODE;
                    dr["QC_CODE"] = Util.GetCondition(cboQccode, "", true);
                }
                else
                {
                    dr["LANGID"] = LoginInfo.LANGID;
                    dr["AREAID"] = Util.GetCondition(cboArea, "", true);
                    dr["EQSGID"] = Util.GetCondition(cboEquipmentSegment, "", true);
                    dr["PRODID"] = Util.GetCondition(cboProduct, "", true);
                    dr["MODLID"] = Util.GetCondition(cboProductModel, "", true);
                    dr["FROM_DATE"] = dtpREQDateFrom.SelectedDateTime.ToString("yyyyMMdd");
                    dr["TO_DATE"] = dtpREQDateTo.SelectedDateTime.ToString("yyyyMMdd");
                    dr["INSP_MED_CLSS_CODE"] = sINSP_MED_CLSS_CODE;
                    dr["QC_CODE"] = Util.GetCondition(cboQccode, "", true);
                }

                RQSTDT.Rows.Add(dr);

                //DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_PQC_LOT_V2", "RQSTDT", "RSLTDT", RQSTDT);
                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_PQC_LOT_V2", "RQSTDT", "RSLTDT", RQSTDT);

                string[] dtLotid = sLotList.Split(',');
                string ErrorLotid = "";                
                DataRow[] dataRowsArray = { };

                if (dtResult.Rows.Count != 0 && !string.IsNullOrEmpty(sLotList))
                {                    
                    foreach (var lot in dtLotid)
                    {
                        if (dtResult.Select("LOTID = '" + lot + "'").Count() == 0)
                        {
                            ErrorLotid += " " + lot + ",";
                        }
                    }

                    if (!ErrorLotid.Equals(""))
                    {
                        Util.MessageValidation("SFU8908", ErrorLotid.Substring(0, ErrorLotid.Length - 3));
                    }
                }

                if (dtResult.Rows.Count == 0)
                {
                    Util.MessageValidation("SFU2951"); //SFU2951 조회결과가 없습니다.
                }


                Util.gridClear(dgTargetList);
                Util.GridSetData(dgLotInfo, dtResult, FrameOperation, true);
                Util.SetTextBlockText_DataGridRowCount(tbCCount, Util.NVC(dtResult.Rows.Count));
            }
            catch(Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void SetDgTargetList(int parmint) //2023-04-07
        {
            DataTable dtTarget = DataTableConverter.Convert(dgTargetList.ItemsSource);
            DataTable dtSource = DataTableConverter.Convert(dgLotInfo.ItemsSource);
            DataRow newRow = null;

            if (dtTarget.Columns.Count == 0)//최초 바인딩이 안된 상태라서 컬럼들이 없음. 추가후 처리
            {
                dtTarget.Columns.Add("CHK", typeof(Boolean));
                dtTarget.Columns.Add("LOTID", typeof(string));
                dtTarget.Columns.Add("MODLID", typeof(string));
                dtTarget.Columns.Add("PRODID", typeof(string));
                dtTarget.Columns.Add("EQSGID", typeof(string));
                dtTarget.Columns.Add("EQSGNAME", typeof(string));
                dtTarget.Columns.Add("INSP_SEQS", typeof(string));
                dtTarget.Columns.Add("STD_DATE", typeof(string));
                dtTarget.Columns.Add("WIPDTTM_ED", typeof(string));
                dtTarget.Columns.Add("OVER_DAY", typeof(string));
                dtTarget.Columns.Add("WARNING_COLOR", typeof(string));
                dtTarget.Columns.Add("AREAID", typeof(string));
            }

            newRow = dtTarget.NewRow();
            newRow["CHK"] = false;
            newRow["LOTID"] = dtSource.Rows[parmint]["LOTID"].ToString();
            newRow["MODLID"] = dtSource.Rows[parmint]["MODLID"].ToString();
            newRow["PRODID"] = dtSource.Rows[parmint]["PRODID"].ToString();
            newRow["EQSGID"] = dtSource.Rows[parmint]["EQSGID"].ToString();
            newRow["EQSGNAME"] = dtSource.Rows[parmint]["EQSGNAME"].ToString();
            newRow["INSP_SEQS"] = dtSource.Rows[parmint]["INSP_SEQS"].ToString();
            newRow["STD_DATE"] = dtSource.Rows[parmint]["STD_DATE"].ToString();
            newRow["WIPDTTM_ED"] = dtSource.Rows[parmint]["WIPDTTM_ED"].ToString();
            newRow["OVER_DAY"] = dtSource.Rows[parmint]["OVER_DAY"].ToString();
            newRow["WARNING_COLOR"] = dtSource.Rows[parmint]["WARNING_COLOR"].ToString();
            newRow["AREAID"] = dtSource.Rows[parmint]["AREAID"].ToString();
            dtTarget.Rows.Add(newRow);

            dgTargetList.ItemsSource = DataTableConverter.Convert(dtTarget);
        }

        private void GetReqHist(string sLotList = "")
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("AREAID", typeof(string));
                RQSTDT.Columns.Add("EQSGID", typeof(string));
                RQSTDT.Columns.Add("FROM_DATE", typeof(string));
                RQSTDT.Columns.Add("TO_DATE", typeof(string));
                RQSTDT.Columns.Add("PRODID", typeof(string));
                RQSTDT.Columns.Add("MODLID", typeof(string));
                RQSTDT.Columns.Add("LOTID", typeof(string));
                RQSTDT.Columns.Add("JUDG_FLAG", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["AREAID"] = Util.GetCondition(cboHistArea);
                dr["EQSGID"] = Util.GetCondition(cboHistEquipmentSegment);

                if (!string.IsNullOrEmpty(sLotList))
                {
                    dr["LOTID"] = sLotList;
                }
                else
                {
                    dr["FROM_DATE"] = dtpHistDateFrom.SelectedDateTime.ToString("yyyyMMdd");
                    dr["TO_DATE"] = dtpHistDateTo.SelectedDateTime.ToString("yyyyMMdd");
                    dr["PRODID"] = Util.GetCondition(cboHistProduct, "", true);
                    dr["MODLID"] = Util.GetCondition(cboHistModel, "", true);
                    dr["JUDG_FLAG"] = Util.GetCondition(cboHistJudge, "", true);
                }
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_PQC_REQ_HIST", "RQSTDT", "RSLTDT", RQSTDT);
                
                if(dtResult.Rows.Count == 0)
                {
                    Util.MessageValidation("SFU2951"); //SFU2951 조회결과가 없습니다.
                }
               

                Util.GridSetData(dgHistList, dtResult, FrameOperation, true);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void PQCRequest()
        {
            try
            {
                if (dgTargetList.GetRowCount() <= 0)
                {
                    Util.MessageInfo("SFU4154"); //의뢰할 데이터가 없습니다.
                    return;
                }
                else if (dgTargetList.GetRowCount() > 500)
                {
                    Util.MessageValidation("SFU8102");   //최대 수량은 500개 입니다.
                    return;
                }

                DataTable dt = DataTableConverter.Convert(dgTargetList.ItemsSource);
                DataTable dtClone = dt.AsEnumerable().CopyToDataTable();

                for (int i = 0; i < dtClone.Rows.Count; i++)
                {

                    DataTable RQSTDT = new DataTable();
                    RQSTDT.TableName = "RQSTDT";
                    RQSTDT.Columns.Add("LANGID", typeof(string));
                    RQSTDT.Columns.Add("SRCTYPE", typeof(string));
                    RQSTDT.Columns.Add("SHOPID", typeof(string));
                    RQSTDT.Columns.Add("AREAID", typeof(string));
                    RQSTDT.Columns.Add("EQSGID", typeof(string));
                    RQSTDT.Columns.Add("PRODID", typeof(string));
                    RQSTDT.Columns.Add("MODLID", typeof(string));
                    RQSTDT.Columns.Add("LOTID", typeof(string));
                    RQSTDT.Columns.Add("USERID", typeof(string));

                    DataRow dr = RQSTDT.NewRow();
                    dr["SRCTYPE"] = LoginInfo.LANGID;
                    dr["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                    dr["SHOPID"] = LoginInfo.CFG_SHOP_ID;
                    dr["AREAID"] = Util.NVC(dtClone.Rows[i]["AREAID"]);
                    dr["EQSGID"] = Util.NVC(dtClone.Rows[i]["EQSGID"]);
                    dr["PRODID"] = Util.NVC(dtClone.Rows[i]["PRODID"]);
                    dr["MODLID"] = Util.NVC(dtClone.Rows[i]["MODLID"]);
                    dr["LOTID"] = Util.NVC(dtClone.Rows[i]["LOTID"]);
                    dr["USERID"] = LoginInfo.USERID;
                    RQSTDT.Rows.Add(dr);

                    DataTable dtResult = new ClientProxy().ExecuteServiceSync("BR_PRD_REG_LONG_INSP_REQUEST_PQC_PACK", "RQSTDT", "RSLTDT", RQSTDT);

                    if (dtResult.Rows.Count > 0)
                    {
                        dt.AcceptChanges();

                        foreach (DataRow drDel in dt.Rows)
                        {
                            if (drDel["LOTID"].ToString() == dtResult.Rows[0]["LOTID"].ToString())
                            {
                                drDel.Delete();
                                break;
                            }
                        }

                        dt.AcceptChanges();

                        Util.GridSetData(dgTargetList, dt, FrameOperation);
                    }
                }
                Util.MessageInfo("FM_ME_0186");
                Util.gridClear(dgLotInfo);
                Util.SetTextBlockText_DataGridRowCount(tbCCount, Util.NVC(dgLotInfo.GetRowCount()));
                Util.SetTextBlockText_DataGridRowCount(tbTargetListCount, Util.NVC(dgTargetList.GetRowCount()));
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

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
        #endregion


    }
}
