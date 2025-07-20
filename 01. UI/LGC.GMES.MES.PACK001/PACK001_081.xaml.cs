/*************************************************************************************
 Created Date : 2021.05.11
      Creator : 김건식
   Decription : 개발Lot 직행불량 예외처리(Pack)

--------------------------------------------------------------------------------------
 [Change History]
 2021.05.11  |  김건식  | 최초작성
 
**************************************************************************************/

using System;
using System.Data;
using C1.WPF;
using System.Windows;
using System.Windows.Controls;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;
using System.Collections.Generic;
using System.Windows.Input;
using LGC.GMES.MES.ControlsLibrary;
using System.Linq;
using DataGridRow = C1.WPF.DataGrid.DataGridRow;

namespace LGC.GMES.MES.PACK001
{
    /// <summary>
    /// PACK001_081.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class PACK001_081 : UserControl, IWorkArea
    {
        #region region Declaration & Constructor 

        CommonCombo _combo = new CommonCombo();

        private readonly Util _util = new Util();

        public IFrameOperation FrameOperation
        {
            get;
            set;
        }

        private struct CHECK_STAT
        {
            public const string SELECT = "SELECT";
            public const string CANCEL = "CANCEL";
        }
        #endregion

        public PACK001_081()
        {
            InitializeComponent();
        }

        #region Initialize
        /// <summary>
        /// 화면내 combo 셋팅
        /// </summary>
        private void InitCombo()
        {
            C1ComboBox cboSHOPID = new C1ComboBox();
            cboSHOPID.SelectedValue = LoginInfo.CFG_SHOP_ID;

            #region [ 등록 Tab ]
            // 동
            String[] sFiltercboArea = { LoginInfo.CFG_SHOP_ID, Area_Type.PACK };
            _combo.SetCombo(cboReqArea, CommonCombo.ComboStatus.NONE, sFilter: sFiltercboArea, sCase: "AREA_AREATYPE");

            // 제품구분
            cboReqPrdtClass.SelectedValuePath = "Key";
            cboReqPrdtClass.DisplayMemberPath = "Value";
            cboReqPrdtClass.Items.Add(new KeyValuePair<string, string>("", "-ALL-"));
            cboReqPrdtClass.Items.Add(new KeyValuePair<string, string>("CMA", "CMA"));
            cboReqPrdtClass.Items.Add(new KeyValuePair<string, string>("BMA", "BMA"));
            cboReqPrdtClass.SelectedIndex = 0;

            // 모델
            C1ComboBox[] cboReqPJT_AbbrParent = { cboReqArea };
            _combo.SetCombo(cboReqPrdtModel, CommonCombo.ComboStatus.ALL, cbParent: cboReqPJT_AbbrParent, sCase: "cboPRJModelPack");

            // 제품  
            C1ComboBox[] cboProductParent = { cboSHOPID, cboReqArea};
            _combo.SetCombo(cboReqPrdt, CommonCombo.ComboStatus.ALL, cbParent: cboProductParent, sCase: "PRJ_PRODUCT");

            // 불량발생공정
            _combo.SetCombo(cboReqDefectProc, CommonCombo.ComboStatus.ALL, sCase: "SetProcessPack");

            // 사유
            String[] sFilter = { "PACK_FPY_EXCL_RSNCODE" };
            _combo.SetCombo(cboReqReson, CommonCombo.ComboStatus.SELECT, sFilter: sFilter, sCase: "COMMCODE");
            #endregion


            #region [ 이력 Tab ]
            // 동
            _combo.SetCombo(cboHistArea, CommonCombo.ComboStatus.NONE, sFilter: sFiltercboArea, sCase: "AREA_AREATYPE");

            // 제품구분
            cboHistPrdtClass.SelectedValuePath = "Key";
            cboHistPrdtClass.DisplayMemberPath = "Value";
            cboHistPrdtClass.Items.Add(new KeyValuePair<string, string>("", "-ALL-"));
            cboHistPrdtClass.Items.Add(new KeyValuePair<string, string>("CMA", "CMA"));
            cboHistPrdtClass.Items.Add(new KeyValuePair<string, string>("BMA", "BMA"));
            cboHistPrdtClass.SelectedIndex = 0;

            // 모델
            C1ComboBox[] cboHistPJT_AbbrParent = { cboHistArea };
            _combo.SetCombo(cboHistPrdtModel, CommonCombo.ComboStatus.ALL, cbParent: cboHistPJT_AbbrParent, sCase: "cboPRJModelPack");

            // 제품  
            C1ComboBox[] cboHistProductParent = { cboSHOPID, cboHistArea };
            _combo.SetCombo(cboHistPrdt, CommonCombo.ComboStatus.ALL, cbParent: cboHistProductParent, sCase: "PRJ_PRODUCT");
            #endregion
        }

        #endregion

        #region Event
        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                dtpDateFromDefect.SelectedDateTime = (DateTime)System.DateTime.Now.AddDays(-7);
                dtpDateToDefect.SelectedDateTime = (DateTime)System.DateTime.Now;
                dtpDateFromHist.SelectedDateTime = (DateTime)System.DateTime.Now.AddDays(-7);
                dtpDateToHist.SelectedDateTime = (DateTime)System.DateTime.Now;

                SetCboEQSG();
                SetCboHistEQSG();

                InitCombo();

                this.Loaded -= new System.Windows.RoutedEventHandler(this.UserControl_Loaded);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        #region Event - [조회]
        private void btnReqSearch_Click(object sender, RoutedEventArgs e)
        {
            GetRequestSearch();
        }

        private void txtLotId_KeyDown(object sender, KeyEventArgs e)
        {
            try
            {
                if (e.Key == Key.Enter)
                {
                    if (txtLotId.Text.Trim() == string.Empty)
                        return;
                    GetRequestSearch();
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void txtLotId_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.V && Keyboard.IsKeyDown(Key.LeftCtrl))
            {
                try
                {
                    string[] stringSeparators = new string[] { "\r\n" };
                    string sPasteString = Clipboard.GetText();
                    string[] sPasteStrings = sPasteString.Split(stringSeparators, StringSplitOptions.None);

                    if (sPasteStrings.Count() > 100)
                    {
                        Util.MessageValidation("SFU3695");   //최대 100개 까지 가능합니다.
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
                    GetRequestSearch(lotList);
                }
                catch (Exception ex)
                {
                    Util.MessageException(ex);
                    return;
                }

                e.Handled = true;
            }
        }

        private void btnHistSearch_Click(object sender, RoutedEventArgs e)
        {
            GetCancelSearch();
        }

        private void txtHistLotId_KeyDown(object sender, KeyEventArgs e)
        {
            try
            {
                if (e.Key == Key.Enter)
                {
                    if (txtHistLotId.Text.Trim() == string.Empty)
                        return;
                    GetCancelSearch();
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void txtHistLotId_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.V && Keyboard.IsKeyDown(Key.LeftCtrl))
            {
                try
                {
                    string[] stringSeparators = new string[] { "\r\n" };
                    string sPasteString = Clipboard.GetText();
                    string[] sPasteStrings = sPasteString.Split(stringSeparators, StringSplitOptions.None);

                    if (sPasteStrings.Count() > 100)
                    {
                        Util.MessageValidation("SFU3695");   //최대 100개 까지 가능합니다.
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
                    GetCancelSearch(lotList);
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

        #region Event - [등록 / 취소 / 선택]
        private void btnRequest_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (dgReqSelectList.GetRowCount() == 0)
                {
                    Util.Alert("SFU1748");  //요청 목록이 필요합니다.
                    return;
                }

                if (Util.GetCondition(cboReqReson, "SFU1593") == "") //사유는필수입니다. >> 사유를 선택하세요.
                {
                    return;
                }

                //요청하시겠습니까?
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(
                        "SFU2924"), null, "Information", MessageBoxButton.OKCancel, MessageBoxIcon.None, (result) =>
                        {
                            if (result == MessageBoxResult.OK)
                            {
                                Request();
                            }
                        });
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return;
            }
        }

        private void btnCancle_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                int rowIndex = _util.GetDataGridCheckFirstRowIndex(dgHistDefectExcl, "CHK");

                if (rowIndex < 0)
                {
                    //Util.Alert("선택된 항목이 없습니다.");
                    Util.MessageValidation("SFU1651");
                    return;
                }

                // 취소 하시겠습니까?
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(
                        "SFU1243"), null, "Information", MessageBoxButton.OKCancel, MessageBoxIcon.None, (result) =>
                        {
                            if (result == MessageBoxResult.OK)
                            {
                                Cancel();
                            }
                        });
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return;
            }
        }

        private void dgReqDefect_MouseUp(object sender, MouseButtonEventArgs e)
        {
            try
            {
                if (dgReqDefect.CurrentRow == null || dgReqDefect.SelectedIndex == -1)
                {
                    return;
                }

                string sColName = dgReqDefect.CurrentColumn.Name;

                if (!sColName.Contains("CHK"))
                {
                    return;
                }

                if (e.ChangedButton.ToString().Equals("Left"))
                {
                    int indexRow = dgReqDefect.CurrentRow.Index;
                    int indexColumn = dgReqDefect.CurrentColumn.Index;


                    string sChkValue = Util.NVC(dgReqDefect.GetCell(indexRow, indexColumn).Value);
                    string sREQ_PASS_FLAG = Util.NVC(dgReqDefect.GetCell(indexRow, 17).Value);

                    if (string.IsNullOrEmpty(sChkValue) || sChkValue.Equals(""))
                        return;

                    if (sREQ_PASS_FLAG.Equals("Y"))
                    {
                        RequestDataGrid(CHECK_STAT.SELECT);
                    }
                    else
                    {
                        Util.Alert("SFU8339"); // SFU8339 : ERP 생산실적이 마감 되었습니다. \n월초 부터 마감전까지 사용가능합니다.
                    }
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void dgReqSelectList_MouseUp(object sender, MouseButtonEventArgs e)
        {
            try
            {
                if (dgReqSelectList.CurrentRow == null || dgReqSelectList.SelectedIndex == -1)
                {
                    return;
                }

                string sColName = dgReqSelectList.CurrentColumn.Name;

                if (!sColName.Contains("CHK"))
                {
                    return;
                }

                if (e.ChangedButton.ToString().Equals("Left"))
                {
                    int indexRow = dgReqSelectList.CurrentRow.Index;
                    int indexColumn = dgReqSelectList.CurrentColumn.Index;
                    string sChkValue = Util.NVC(dgReqSelectList.GetCell(indexRow, indexColumn).Value);

                    if (string.IsNullOrEmpty(sChkValue) || sChkValue.Equals(""))
                        return;

                    RequestDataGrid(CHECK_STAT.CANCEL);
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void dgHistDefectExcl_MouseUp(object sender, MouseButtonEventArgs e)
        {
            try
            {
                if (dgHistDefectExcl.CurrentRow == null || dgHistDefectExcl.SelectedIndex == -1)
                {
                    return;
                }

                string sColName = dgHistDefectExcl.CurrentColumn.Name;

                if (!sColName.Contains("CHK"))
                {
                    return;
                }

                if (e.ChangedButton.ToString().Equals("Left"))
                {
                    int indexRow = dgHistDefectExcl.CurrentRow.Index;
                    int indexColumn = dgHistDefectExcl.CurrentColumn.Index;


                    string sChkValue = Util.NVC(dgHistDefectExcl.GetCell(indexRow, indexColumn).Value);
                    string sCancel_PASS_FLAG = Util.NVC(dgHistDefectExcl.GetCell(indexRow, 22).Value);

                    if (string.IsNullOrEmpty(sChkValue) || sChkValue.Equals(""))
                        return;

                    if (sCancel_PASS_FLAG.Equals("Y"))
                    {
                        DataTableConverter.SetValue(dgHistDefectExcl.Rows[indexRow].DataItem, sColName, true);
                    }
                    else
                    {
                        Util.Alert("SFU8339"); // SFU8339 : ERP 생산실적이 마감 되었습니다. \n월초 부터 마감전까지 사용가능합니다.
                    }
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        #endregion

        #region Event - [콤보박스]
        private void cboReqArea_SelectedItemChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            this.Dispatcher.BeginInvoke(new System.Action(() =>
            {
                cboReqEquipmentSegment.isAllUsed = true;
                SetCboEQSG();
                
            }));
        }

        private void cboReqEquipmentSegment_SelectionChanged(object sender, EventArgs e)
        {
            this.Dispatcher.BeginInvoke(new System.Action(() =>
            {
                SetcboReqPrdtModel();
                SetcboDefectProcess();

            }));
        }

        private void cboReqPrdtModel_SelectedItemChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            this.Dispatcher.BeginInvoke(new System.Action(() =>
            {
                SetcboProduct();

            }));
        }

        private void cboReqPrdtClass_SelectedItemChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            this.Dispatcher.BeginInvoke(new System.Action(() =>
            {
                SetcboDefectProcess();

            }));
        }


        private void cboHistArea_SelectedItemChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            this.Dispatcher.BeginInvoke(new System.Action(() =>
            {
                cboHistEquipmentSegment.isAllUsed = true;
                SetCboHistEQSG();

            }));
        }

        private void cboHistEquipmentSegment_SelectionChanged(object sender, EventArgs e)
        {
            this.Dispatcher.BeginInvoke(new System.Action(() =>
            {
                SetcboHistPrdtModel();

            }));
        }

        private void cboHistPrdtModel_SelectedItemChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            this.Dispatcher.BeginInvoke(new System.Action(() =>
            {
                SetcboHistProduct();

            }));
        }
        #endregion

        #endregion

        #region Method

        private void SetCboEQSG()
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("AREAID", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["AREAID"] = cboReqArea.SelectedValue;
                RQSTDT.Rows.Add(dr);
                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_EQUIPMENTSEGMENT_MULTI_CBO", "RQSTDT", "RSLTDT", RQSTDT);
                
                cboReqEquipmentSegment.ItemsSource = DataTableConverter.Convert(dtResult);

                for (int i = 0; i < dtResult.Rows.Count; i++)
                {
                    if (Util.NVC(dtResult.Rows[i]["CBO_CODE"]) == LoginInfo.CFG_EQSG_ID)
                    {
                        cboReqEquipmentSegment.Check(i);
                        break;
                    }
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void SetcboReqPrdtModel()
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("AREAID", typeof(string));
                RQSTDT.Columns.Add("EQSGID", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["AREAID"] = Util.GetCondition(cboReqArea);
                dr["EQSGID"] = cboReqEquipmentSegment.SelectedItemsToString;
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_PRJMODEL_MULTI_CBO", "RQSTDT", "RSLTDT", RQSTDT);

                cboReqPrdtModel.ItemsSource = AddStatus(dtResult, CommonCombo.ComboStatus.ALL, "CBO_CODE", "CBO_NAME").Copy().AsDataView();
                cboReqPrdtModel.SelectedIndex = 0;

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void SetcboProduct()
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("SHOPID", typeof(string));
                RQSTDT.Columns.Add("AREAID", typeof(string));
                RQSTDT.Columns.Add("EQSGID", typeof(string));
                RQSTDT.Columns.Add("PROCID", typeof(string));
                RQSTDT.Columns.Add("MODLID", typeof(string));
                RQSTDT.Columns.Add("AREA_TYPE_CODE", typeof(string));
                RQSTDT.Columns.Add("PRDT_CLSS_CODE", typeof(string));


                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["SHOPID"] = LoginInfo.CFG_SHOP_ID;
                dr["AREAID"] = Util.GetCondition(cboReqArea);
                dr["EQSGID"] = cboReqEquipmentSegment.SelectedItemsToString;
                dr["PROCID"] = null;
                dr["MODLID"] = Util.GetCondition(cboReqPrdtModel, "", true);
                dr["AREA_TYPE_CODE"] = Area_Type.PACK;
                dr["PRDT_CLSS_CODE"] = Util.GetCondition(cboReqPrdtClass, "", true);

                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_PRODUCT_MULTI_CBO", "RQSTDT", "RSLTDT", RQSTDT);

                cboReqPrdt.ItemsSource = AddStatus(dtResult, CommonCombo.ComboStatus.ALL, "CBO_CODE", "CBO_NAME").Copy().AsDataView();
                cboReqPrdt.SelectedIndex = 0;

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void SetcboDefectProcess()
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("EQSGID", typeof(string));
                RQSTDT.Columns.Add("AREA_TYPE_CODE", typeof(string));
                RQSTDT.Columns.Add("PCSGID", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["EQSGID"] = cboReqEquipmentSegment.SelectedItemsToString;
                dr["AREA_TYPE_CODE"] = Area_Type.PACK;
                if(string.IsNullOrEmpty(Util.GetCondition(cboReqPrdtClass)))
                {
                    dr["PCSGID"] = null;
                }
                else
                {
                    dr["PCSGID"] = Util.GetCondition(cboReqPrdtClass).Equals("CMA") ? "M" : "P";
                }
                
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_PROCESS_PACK_CBO", "RQSTDT", "RSLTDT", RQSTDT);

                cboReqDefectProc.ItemsSource = AddStatus(dtResult, CommonCombo.ComboStatus.ALL, "CBO_CODE", "CBO_NAME").Copy().AsDataView();
                cboReqDefectProc.SelectedIndex = 0;

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void SetCboHistEQSG()
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("AREAID", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["AREAID"] = cboHistArea.SelectedValue;
                RQSTDT.Rows.Add(dr);
                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_EQUIPMENTSEGMENT_MULTI_CBO", "RQSTDT", "RSLTDT", RQSTDT);

                cboHistEquipmentSegment.ItemsSource = DataTableConverter.Convert(dtResult);

                for (int i = 0; i < dtResult.Rows.Count; i++)
                {
                    if (Util.NVC(dtResult.Rows[i]["CBO_CODE"]) == LoginInfo.CFG_EQSG_ID)
                    {
                        cboHistEquipmentSegment.Check(i);
                        break;
                    }
                }
            }
            catch (Exception ex)
            {
                //Util.MessageException(ex);
            }
        }

        private void SetcboHistPrdtModel()
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("AREAID", typeof(string));
                RQSTDT.Columns.Add("EQSGID", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["AREAID"] = Util.GetCondition(cboHistArea);
                dr["EQSGID"] = cboHistEquipmentSegment.SelectedItemsToString;
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_PRJMODEL_MULTI_CBO", "RQSTDT", "RSLTDT", RQSTDT);

                cboHistPrdtModel.ItemsSource = AddStatus(dtResult, CommonCombo.ComboStatus.ALL, "CBO_CODE", "CBO_NAME").Copy().AsDataView();
                cboHistPrdtModel.SelectedIndex = 0;

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void SetcboHistProduct()
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("SHOPID", typeof(string));
                RQSTDT.Columns.Add("AREAID", typeof(string));
                RQSTDT.Columns.Add("EQSGID", typeof(string));
                RQSTDT.Columns.Add("PROCID", typeof(string));
                RQSTDT.Columns.Add("MODLID", typeof(string));
                RQSTDT.Columns.Add("AREA_TYPE_CODE", typeof(string));
                RQSTDT.Columns.Add("PRDT_CLSS_CODE", typeof(string));


                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["SHOPID"] = LoginInfo.CFG_SHOP_ID;
                dr["AREAID"] = Util.GetCondition(cboHistArea);
                dr["EQSGID"] = cboHistEquipmentSegment.SelectedItemsToString;
                dr["PROCID"] = null;
                dr["MODLID"] = Util.GetCondition(cboHistPrdtModel, "", true);
                dr["AREA_TYPE_CODE"] = Area_Type.PACK;
                dr["PRDT_CLSS_CODE"] = Util.GetCondition(cboHistPrdtClass, "", true);

                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_PRODUCT_MULTI_CBO", "RQSTDT", "RSLTDT", RQSTDT);

                cboHistPrdt.ItemsSource = AddStatus(dtResult, CommonCombo.ComboStatus.ALL, "CBO_CODE", "CBO_NAME").Copy().AsDataView();
                cboHistPrdt.SelectedIndex = 0;

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

        private void GetRequestSearch(string sLotList = "")
        {
            try
            {
                DataTable RQSTDT = new DataTable();

                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("EQSGID", typeof(string));
                RQSTDT.Columns.Add("FROM_DATE", typeof(string));
                RQSTDT.Columns.Add("TO_DATE", typeof(string));
                RQSTDT.Columns.Add("CLASS", typeof(string));
                RQSTDT.Columns.Add("MODEL", typeof(string));
                RQSTDT.Columns.Add("PRODID", typeof(string));
                RQSTDT.Columns.Add("PROCID_CAUSE", typeof(string));
                RQSTDT.Columns.Add("SHOPID", typeof(string));
                RQSTDT.Columns.Add("LOTID", typeof(string));
                RQSTDT.Columns.Add("FPY_EXCL_FLAG", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["SHOPID"] = LoginInfo.CFG_SHOP_ID;
                dr["FPY_EXCL_FLAG"] = 'N';

                if (!string.IsNullOrEmpty(sLotList))
                {
                    dr["LOTID"] = sLotList;
                }
                else if (!string.IsNullOrEmpty(Util.NVC(txtLotId.Text)))
                {
                    dr["LOTID"] = Util.NVC(txtLotId.Text);
                }
                else
                {
                    dr["EQSGID"] = cboReqEquipmentSegment.SelectedItemsToString;
                    dr["FROM_DATE"] = Util.GetCondition(dtpDateFromDefect);
                    dr["TO_DATE"] = Util.GetCondition(dtpDateToDefect);
                    dr["CLASS"] = Util.GetCondition(cboReqPrdtClass, "", true);
                    dr["MODEL"] = Util.GetCondition(cboReqPrdtModel, "", true);
                    dr["PRODID"] = Util.GetCondition(cboReqPrdt, "", true);
                    dr["PROCID_CAUSE"] = Util.GetCondition(cboReqDefectProc, "", true);
                }
                RQSTDT.Rows.Add(dr);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_GDW_LOT_FRST_DFCT", "RQSTDT", "RSLTDT", RQSTDT);

                dgReqDefect.ItemsSource = null;
                dgReqSelectList.ItemsSource = null;

                if (dtRslt.Rows.Count != 0)
                {
                    Util.GridSetData(dgReqDefect, dtRslt, FrameOperation);
                }
                
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void GetCancelSearch(string sLotList = "")
        {
            try
            {
                DataTable RQSTDT = new DataTable();

                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("EQSGID", typeof(string));
                RQSTDT.Columns.Add("HIST_FROM_DATE", typeof(string));
                RQSTDT.Columns.Add("HIST_TO_DATE", typeof(string));
                RQSTDT.Columns.Add("CLASS", typeof(string));
                RQSTDT.Columns.Add("MODEL", typeof(string));
                RQSTDT.Columns.Add("PRODID", typeof(string));
                RQSTDT.Columns.Add("SHOPID", typeof(string));
                RQSTDT.Columns.Add("LOTID", typeof(string));
                RQSTDT.Columns.Add("FPY_EXCL_FLAG", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["SHOPID"] = LoginInfo.CFG_SHOP_ID;
                dr["FPY_EXCL_FLAG"] = 'Y';

                if (!string.IsNullOrEmpty(sLotList))
                {
                    dr["LOTID"] = sLotList;
                }
                else if (!string.IsNullOrEmpty(Util.NVC(txtHistLotId.Text)))
                {
                    dr["LOTID"] = Util.NVC(txtHistLotId.Text);
                }
                else
                {
                    dr["EQSGID"] = cboHistEquipmentSegment.SelectedItemsToString;
                    dr["HIST_FROM_DATE"] = dtpDateFromHist.SelectedDateTime.ToString("yyyyMMdd");
                    dr["HIST_TO_DATE"] = dtpDateToHist.SelectedDateTime.ToString("yyyyMMdd");
                    dr["CLASS"] = Util.GetCondition(cboHistPrdtClass, "", true);
                    dr["MODEL"] = Util.GetCondition(cboHistPrdtModel, "", true);
                    dr["PRODID"] = Util.GetCondition(cboHistPrdt, "", true);
                }
                RQSTDT.Rows.Add(dr);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_GDW_LOT_FRST_DFCT", "RQSTDT", "RSLTDT", RQSTDT);

                dgHistDefectExcl.ItemsSource = null;

                if (dtRslt.Rows.Count != 0)
                {
                    Util.GridSetData(dgHistDefectExcl, dtRslt, FrameOperation);
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void RequestDataGrid(string sCheckStat)
        {

            if (sCheckStat == CHECK_STAT.SELECT)
            {
                if (dgReqDefect.ItemsSource == null) return;
                if (dgReqDefect.GetRowCount() == 0) return;
            }
            else
            {
                if (dgReqSelectList.ItemsSource == null) return;
                if (dgReqSelectList.GetRowCount() == 0) return;
            }

            DataTable dtTarget = DataTableConverter.Convert((sCheckStat == CHECK_STAT.SELECT) ? dgReqSelectList.ItemsSource : dgReqDefect.ItemsSource);
            DataTable dtSource = DataTableConverter.Convert((sCheckStat == CHECK_STAT.SELECT) ? dgReqDefect.ItemsSource : dgReqSelectList.ItemsSource);
            DataRow newRow = null;

            if (dtTarget.Columns.Count == 0)//최초 바인딩이 안된 상태라서 컬럼들이 없음. 추가후 처리
            {
                dtTarget.Columns.Add("CHK", typeof(Boolean));
                dtTarget.Columns.Add("EQSGID", typeof(string));
                dtTarget.Columns.Add("EQSGNAME", typeof(string));
                dtTarget.Columns.Add("CLASS", typeof(string));
                dtTarget.Columns.Add("MODEL", typeof(string));
                dtTarget.Columns.Add("PRODID", typeof(string));
                dtTarget.Columns.Add("LOTTYPE", typeof(string));
                dtTarget.Columns.Add("LOTYDESC", typeof(string));
                dtTarget.Columns.Add("DFCT_WIPSEQ", typeof(string));
                dtTarget.Columns.Add("LOTID", typeof(string));
                dtTarget.Columns.Add("DFCT_PROCID_CAUSE", typeof(string));
                dtTarget.Columns.Add("PROCNAME_CAUSE", typeof(string));
                dtTarget.Columns.Add("DFCT_RESNCODE", typeof(string));
                dtTarget.Columns.Add("RESNNAME", typeof(string));
                dtTarget.Columns.Add("DFCT_ACTDTTM", typeof(string));
                dtTarget.Columns.Add("PROCID_CURR", typeof(string));
                dtTarget.Columns.Add("PROCNAME_CURR", typeof(string));
                dtTarget.Columns.Add("REQ_PASS_FLAG", typeof(string));
            }

            for (int i = dtSource.Rows.Count; i > 0; i--)
            {
                if (string.Equals(dtSource.Rows[i - 1]["CHK"].ToString(), "True"))
                {
                    newRow = dtTarget.NewRow();
                    newRow["CHK"] = false;
                    newRow["EQSGID"] = dtSource.Rows[i - 1]["EQSGID"].ToString();
                    newRow["EQSGNAME"] = dtSource.Rows[i - 1]["EQSGNAME"].ToString();
                    newRow["CLASS"] = dtSource.Rows[i - 1]["CLASS"].ToString();
                    newRow["MODEL"] = dtSource.Rows[i - 1]["MODEL"].ToString();
                    newRow["PRODID"] = dtSource.Rows[i - 1]["PRODID"].ToString();
                    newRow["LOTTYPE"] = dtSource.Rows[i - 1]["LOTTYPE"].ToString();
                    newRow["LOTYDESC"] = dtSource.Rows[i - 1]["LOTYDESC"].ToString();
                    newRow["DFCT_WIPSEQ"] = dtSource.Rows[i - 1]["DFCT_WIPSEQ"].ToString();
                    newRow["LOTID"] = dtSource.Rows[i - 1]["LOTID"].ToString();
                    newRow["DFCT_PROCID_CAUSE"] = dtSource.Rows[i - 1]["DFCT_PROCID_CAUSE"].ToString();
                    newRow["PROCNAME_CAUSE"] = dtSource.Rows[i - 1]["PROCNAME_CAUSE"].ToString();
                    newRow["DFCT_RESNCODE"] = dtSource.Rows[i - 1]["DFCT_RESNCODE"].ToString();
                    newRow["RESNNAME"] = dtSource.Rows[i - 1]["RESNNAME"].ToString();
                    newRow["DFCT_ACTDTTM"] = dtSource.Rows[i - 1]["DFCT_ACTDTTM"].ToString();
                    newRow["PROCID_CURR"] = dtSource.Rows[i - 1]["PROCID_CURR"].ToString();
                    newRow["PROCNAME_CURR"] = dtSource.Rows[i - 1]["PROCNAME_CURR"].ToString();
                    newRow["REQ_PASS_FLAG"] = dtSource.Rows[i - 1]["REQ_PASS_FLAG"].ToString();

                    dtTarget.Rows.Add(newRow);

                    dtSource.Rows[i - 1].Delete();
                }
            }

            dgReqSelectList.ItemsSource = DataTableConverter.Convert((sCheckStat == CHECK_STAT.SELECT) ? dtTarget : dtSource);
            dgReqDefect.ItemsSource = DataTableConverter.Convert((sCheckStat == CHECK_STAT.SELECT) ? dtSource : dtTarget);

        }

        private void Request()
        {
            try
            {
                DataTable RQSTDT = new DataTable();

                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("FPY_EXCL_FLAG", typeof(string));
                RQSTDT.Columns.Add("FPY_EXCL_RESNCODE", typeof(string));
                RQSTDT.Columns.Add("USERID", typeof(string));
                RQSTDT.Columns.Add("FPY_EXCL_RESNNOTE", typeof(string));
                RQSTDT.Columns.Add("LOTID", typeof(string));
                RQSTDT.Columns.Add("DFCT_WIPSEQ", typeof(string));
                RQSTDT.Columns.Add("DFCT_ACTID", typeof(string));
                RQSTDT.Columns.Add("DFCT_RESNCODE", typeof(string));

                for (int i = 0; i < dgReqSelectList.Rows.Count; i++)
                {
                    DataRow dr = RQSTDT.NewRow();

                    dr["FPY_EXCL_FLAG"] = "Y";
                    dr["FPY_EXCL_RESNCODE"] = Util.GetCondition(cboReqReson);
                    dr["USERID"] = LoginInfo.USERID;
                    dr["FPY_EXCL_RESNNOTE"] = Util.GetCondition(txtReqNote);
                    dr["LOTID"] = Util.NVC(DataTableConverter.GetValue(dgReqSelectList.Rows[i].DataItem, "LOTID"));
                    dr["DFCT_WIPSEQ"] = Util.NVC(DataTableConverter.GetValue(dgReqSelectList.Rows[i].DataItem, "DFCT_WIPSEQ"));
                    dr["DFCT_ACTID"] = "DEFECT_LOT";
                    dr["DFCT_RESNCODE"] = Util.NVC(DataTableConverter.GetValue(dgReqSelectList.Rows[i].DataItem, "DFCT_RESNCODE"));

                    RQSTDT.Rows.Add(dr);
                }

                new ClientProxy().ExecuteServiceSync("DA_PRD_UPD_GDW_LOT_FRST_DFCT", "RQSTDT", null, RQSTDT);

                GetRequestSearch();
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void Cancel()
        {
            try
            {
                DataTable RQSTDT = new DataTable();

                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("FPY_EXCL_FLAG", typeof(string));
                RQSTDT.Columns.Add("FPY_EXCL_RESNCODE", typeof(string));
                RQSTDT.Columns.Add("USERID", typeof(string));
                RQSTDT.Columns.Add("FPY_EXCL_RESNNOTE", typeof(string));
                RQSTDT.Columns.Add("LOTID", typeof(string));
                RQSTDT.Columns.Add("DFCT_WIPSEQ", typeof(string));
                RQSTDT.Columns.Add("DFCT_ACTID", typeof(string));
                RQSTDT.Columns.Add("DFCT_RESNCODE", typeof(string));

                foreach (DataGridRow row in dgHistDefectExcl.Rows)
                {
                    if (DataTableConverter.GetValue(row.DataItem, "CHK").ToString() == "True" ||
                         DataTableConverter.GetValue(row.DataItem, "CHK").ToString() == "1")
                    {
                        DataRow dr = RQSTDT.NewRow();

                        dr["FPY_EXCL_FLAG"] = "N";
                        dr["FPY_EXCL_RESNCODE"] = Util.NVC(DataTableConverter.GetValue(dgHistDefectExcl.Rows[row.Index].DataItem, "FPY_EXCL_RESNCODE"));
                        dr["USERID"] = LoginInfo.USERID;
                        dr["FPY_EXCL_RESNNOTE"] = "Canceled By UI";
                        dr["LOTID"] = Util.NVC(DataTableConverter.GetValue(dgHistDefectExcl.Rows[row.Index].DataItem, "LOTID"));
                        dr["DFCT_WIPSEQ"] = Util.NVC(DataTableConverter.GetValue(dgHistDefectExcl.Rows[row.Index].DataItem, "DFCT_WIPSEQ"));
                        dr["DFCT_ACTID"] = "DEFECT_LOT";
                        dr["DFCT_RESNCODE"] = Util.NVC(DataTableConverter.GetValue(dgHistDefectExcl.Rows[row.Index].DataItem, "DFCT_RESNCODE"));

                        RQSTDT.Rows.Add(dr);
                    }
                }

                new ClientProxy().ExecuteServiceSync("DA_PRD_UPD_GDW_LOT_FRST_DFCT", "RQSTDT", null, RQSTDT);

                GetCancelSearch();
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        #endregion
    }
}
