/*********************************************************************************************************************************
 Created Date : 2024.11.05
      Creator : 오화백
   Decription : 원자재 재공현황 조회
-----------------------------------------------------------------------------------------------------------------------------------
 [Change History]
-----------------------------------------------------------------------------------------------------------------------------------
       DATE            CSR NO            DEVELOPER            DESCRIPTION
-----------------------------------------------------------------------------------------------------------------------------------
  2024.11.05                               오화백           Initial Created.
  2025.05.21                               박승민           PLC_ID 로직 변경으로 DA->BR 변경
  2025.06.04                               권준서           원자재 재공조회 - 원자재 저장위치별 재고조회(원자재 삭제 기능 버튼 추가)
***********************************************************************************************************************************/

using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.CMM001.Popup;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using System;
using System.Data;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;
using C1.WPF.DataGrid;
using System.Windows.Media;
using C1.WPF;
using C1.WPF.Excel;
using System.IO;
using System.Configuration;
using LGC.GMES.MES.CMM001;
using C1.WPF.DataGrid.Summaries;
using Microsoft.Win32;

using System.Linq;
using System.Threading;

using LGC.GMES.MES.CMM001.Extensions;

using LGC.GMES.MES.CMM001.UserControls;
using System.Windows.Data;
using System.Windows.Media.Animation;
namespace LGC.GMES.MES.MTRL001
{
    public partial class MTRL001_220 : UserControl, IWorkArea
    {
        #region Declaration
        private CommonCombo combo = new CommonCombo();
        private Util _Util = new Util();

        private string CSTStatus = string.Empty;
        private string sMtrlid = "";

        private readonly DispatcherTimer _monitorTimer = new DispatcherTimer();
        private bool _isSelectedAutoTime = false;
        private bool _isLoaded = false;
        public IFrameOperation FrameOperation
        {
            get;
            set;
        }
        #endregion

        public MTRL001_220()
        {
            InitializeComponent();
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            ApplyPermissions();
            InitCombo();
            dtpEnd.SelectedDateTime = DateTime.Today;
            dtpStart.SelectedDateTime = DateTime.Today.AddDays(-31);

            dtpEndT1.SelectedDateTime = DateTime.Today;
            dtpStartT1.SelectedDateTime = DateTime.Today.AddDays(-31);

            dtpEndT2.SelectedDateTime = DateTime.Today;
            dtpStartT2.SelectedDateTime = DateTime.Today.AddDays(-31);

            TimerSetting();
            this.Loaded -= UserControl_Loaded;
            _isLoaded = true;
        }

        #region [ Event ] - Button
        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            SummaryData();
        }
        #endregion

        #region [ Method ] - Search
        private void InitCombo()
        {
            CommonCombo _combo = new CommonCombo();

            // Area 콤보박스
            SetAreaCombo(cboArea);

            //자재 유형
            SetMtrlType(cboMtrlType);
            SetMtrlType(cboMtrlTypeT1);
            SetMtrlType(cboMtrlTypeT2);

            //저장 위치
            SetOutputWHCombo(cboLoc);
            //자재 정보 조회
            SearchMtrl_Group();
            btnDeleteT2.Visibility = Visibility.Collapsed;

            cboLoc.SelectedItemChanged += CboLoc_SelectedItemChanged;
        }

        private void init()
        {
            Util.gridClear(dgSummary);
            Util.gridClear(dgDetail);
            Util.gridClear(dgHist);
            Util.gridClear(dgListLoc);
        }
         

        private void SummaryData()
        {
            try
            {
                init();
                DataTable dtINDATA = new DataTable();
                dtINDATA.Columns.Add("MTRLID", typeof(string));
              
                DataRow Indata = dtINDATA.NewRow();
                Indata["MTRLID"] = popSearchMtrl.SelectedValue == null ? null : popSearchMtrl.SelectedValue.ToString();


                dtINDATA.Rows.Add(Indata);

                string bizRule = "DA_INV_SEL_MTRL_PROC_SUM";

                loadingIndicator.Visibility = Visibility.Visible;

                new ClientProxy().ExecuteService(bizRule, "RQSTDT", "RSLTDT", dtINDATA, (dtResult, ex) =>
                {
                    loadingIndicator.Visibility = Visibility.Collapsed;
                    if (ex != null)
                    {
                        Util.MessageException(ex); //Util.AlertInfo(ex.Message);
                        return;
                    }

                    if (dtResult == null || dtResult.Rows.Count == 0)
                    {
                        Util.MessageValidation("SFU3537");  //조회된 데이터가 없습니다.
                        return;
                    }

                    Util.GridSetData(dgSummary, dtResult, FrameOperation, true);
                });
            }
            catch (Exception ex)
            {
                loadingIndicator.Visibility = Visibility.Collapsed;
                Util.MessageException(ex);
            }
        }

        private void CboLoc_SelectedItemChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            if (dgListLoc.GetRowCount() > 0)
            {
                if (ChkInputWhLocation().Equals(cboLoc.SelectedValue))
                {
                    foreach (DataRowView drv in (DataView)dgListLoc.ItemsSource)
                    {
                        if (!drv["MATERIAL_LOCATION"].Equals(cboLoc.SelectedValue))
                        {
                            btnDeleteT2.Visibility = Visibility.Collapsed;
                            return;
                        }
                    }
                    btnDeleteT2.Visibility = Visibility.Visible;
                }
                else
                {
                    btnDeleteT2.Visibility = Visibility.Collapsed;
                }
            }
            else
            {
                if (ChkInputWhLocation().Equals(cboLoc.SelectedValue))
                {
                    btnDeleteT2.Visibility = Visibility.Visible;
                }
                else
                {
                    btnDeleteT2.Visibility = Visibility.Collapsed;
                }
            }
        }

        private void DetailData(string MtrlID, string Loc)
        {
            try
            {
                DataTable dtINDATA = new DataTable();
                DataTable dtOUTDATA = new DataTable();

                dtINDATA.Columns.Add("LANGID", typeof(string));
                dtINDATA.Columns.Add("MTRL_CD", typeof(string));
                dtINDATA.Columns.Add("MATERIAL_LOCATION", typeof(string));

                DataRow Indata = dtINDATA.NewRow();
                Indata["LANGID"] = LoginInfo.LANGID;
                Indata["MTRL_CD"] = MtrlID;
                Indata["MATERIAL_LOCATION"] = Loc == string.Empty? null : Loc;


                dtINDATA.Rows.Add(Indata);

                string bizRule = "BR_INV_SEL_MTRL_PROC_DETAIL";

                loadingIndicator.Visibility = Visibility.Visible;

                new ClientProxy().ExecuteService(bizRule, "RQSTDT", "RSLTDT", dtINDATA, (dtResult, ex) =>
                {
                    loadingIndicator.Visibility = Visibility.Collapsed;
                    if (ex != null)
                    {
                        Util.MessageException(ex); //Util.AlertInfo(ex.Message);
                        return;
                    }

                    if (dtResult == null || dtResult.Rows.Count == 0)
                    {
                        Util.MessageValidation("SFU3537");  //조회된 데이터가 없습니다.
                        Util.GridSetData(dgDetail, dtResult, FrameOperation, true);
                        return;
                    }

                    Util.GridSetData(dgDetail, dtResult, FrameOperation,true);
                });
            }
            catch (Exception ex)
            {
                loadingIndicator.Visibility = Visibility.Collapsed;
                Util.MessageException(ex);
            }
        }


        private void HistData()
        {
            try
            {
                DataTable dtINDATA = new DataTable();
                DataTable dtOUTDATA = new DataTable();

                dtINDATA.Columns.Add("MTRL_CD", typeof(string));
                dtINDATA.Columns.Add("CARRIER_ID", typeof(string));
                dtINDATA.Columns.Add("LANGID", typeof(string));
                dtINDATA.Columns.Add("FROM_DATE", typeof(string));
                dtINDATA.Columns.Add("TO_DATE", typeof(string));
                dtINDATA.Columns.Add("MTRL_TYPE", typeof(string));
                dtINDATA.Columns.Add("CANCEL_CHK_N", typeof(string)); //설비투입취소 미포함
                dtINDATA.Columns.Add("CANCEL_CHK_Y", typeof(string)); //설비투입취소 포함

                DataRow Indata = dtINDATA.NewRow();

                if(txtPallet.Text == string.Empty)
                {
                    Indata["MTRL_CD"] = popSearchMtrlT1.SelectedValue == null ? null : popSearchMtrlT1.SelectedValue.ToString();
                    Indata["LANGID"] = LoginInfo.LANGID;
                    Indata["FROM_DATE"] = dtpStart.SelectedDateTime.ToString("yyyyMMdd");
                    Indata["TO_DATE"] = dtpEnd.SelectedDateTime.ToString("yyyyMMdd");
                    Indata["MTRL_TYPE"] = cboMtrlType.GetBindValue();
                }
                else
                {
                  
                    Indata["CARRIER_ID"] = txtPallet.Text == string.Empty ? null : txtPallet.Text;
                    Indata["LANGID"] = LoginInfo.LANGID;
                    Indata["FROM_DATE"] = dtpStart.SelectedDateTime.ToString("yyyyMMdd");
                    Indata["TO_DATE"] = dtpEnd.SelectedDateTime.ToString("yyyyMMdd");
                }
                if(ChkCancel.IsChecked == true)
                {
                    Indata["CANCEL_CHK_Y"] = "Y";
                    Indata["CANCEL_CHK_N"] = null;
                }
                else
                {
                    Indata["CANCEL_CHK_Y"] = null;
                    Indata["CANCEL_CHK_N"] = "Y";
                }

                dtINDATA.Rows.Add(Indata);

                string bizRule = "DA_INV_SEL_MTRL_PROC_HIST";

                loadingIndicator.Visibility = Visibility.Visible;

                new ClientProxy().ExecuteService(bizRule, "RQSTDT", "RSLTDT", dtINDATA, (dtResult, ex) =>
                {
                    loadingIndicator.Visibility = Visibility.Collapsed;
                    if (ex != null)
                    {
                        Util.MessageException(ex); //Util.AlertInfo(ex.Message);
                        return;
                    }

                    if (dtResult == null || dtResult.Rows.Count == 0)
                    {
                        Util.MessageValidation("SFU3537");  //조회된 데이터가 없습니다.
                        Util.GridSetData(dgHist, dtResult, FrameOperation, true);
                        return;
                    }

                    Util.GridSetData(dgHist, dtResult, FrameOperation,true);
                });
            }
            catch (Exception ex)
            {
                loadingIndicator.Visibility = Visibility.Collapsed;
                Util.MessageException(ex);
            }
        }


        private void LocList()
        {
            try
            {
                DataTable dtINDATA = new DataTable();
                DataTable dtOUTDATA = new DataTable();

                dtINDATA.Columns.Add("MTRL_CD", typeof(string));
                dtINDATA.Columns.Add("MATERIAL_LOCATION", typeof(string));
                dtINDATA.Columns.Add("LANGID", typeof(string));
                dtINDATA.Columns.Add("CARRIER_ID", typeof(string));
                dtINDATA.Columns.Add("FROM_DATE", typeof(string));
                dtINDATA.Columns.Add("TO_DATE", typeof(string));
                dtINDATA.Columns.Add("MTRL_TYPE", typeof(string));
                DataRow Indata = dtINDATA.NewRow();
                if(txtPalletT1.Text == string.Empty)
                {
                    Indata["MTRL_CD"] = popSearchMtrlT2.SelectedValue == null ? null : popSearchMtrlT2.SelectedValue.ToString(); ;
                    Indata["MATERIAL_LOCATION"] = cboLoc.SelectedValue == null ? null : cboLoc.SelectedValue.ToString();
                    Indata["LANGID"] = LoginInfo.LANGID;
                    Indata["MTRL_TYPE"] = cboMtrlTypeT1.GetBindValue();

                    if (chkUI_DateT1.IsChecked == true)
                    {
                        Indata["FROM_DATE"] = dtpStartT1.SelectedDateTime.ToString("yyyyMMdd");
                        Indata["TO_DATE"] = dtpEndT1.SelectedDateTime.ToString("yyyyMMdd");
                    }
                }
                else
                {
                    Indata["LANGID"] = LoginInfo.LANGID;
                    Indata["CARRIER_ID"] = txtPalletT1.Text == string.Empty ? null : txtPalletT1.Text;
                }

                dtINDATA.Rows.Add(Indata);

                string bizRule = "BR_INV_SEL_MTRL_PROC";

                loadingIndicator.Visibility = Visibility.Visible;

                new ClientProxy().ExecuteService(bizRule, "RQSTDT", "RSLTDT", dtINDATA, (dtResult, ex) =>
                {
                    loadingIndicator.Visibility = Visibility.Collapsed;
                    if (ex != null)
                    {
                        Util.MessageException(ex); //Util.AlertInfo(ex.Message);
                        return;
                    }

                    if (dtResult == null || dtResult.Rows.Count == 0)
                    {
                        Util.MessageValidation("SFU3537");  //조회된 데이터가 없습니다.
                        Util.GridSetData(dgListLoc, dtResult, FrameOperation, true);
                        return;
                    }

                    Util.GridSetData(dgListLoc, dtResult, FrameOperation,true);
                });

                if (ChkInputWhLocation().Equals(cboLoc.SelectedValue))
                {
                    btnDeleteT2.Visibility = Visibility.Visible;
                }
            }
            catch (Exception ex)
            {
                loadingIndicator.Visibility = Visibility.Collapsed;
                Util.MessageException(ex);
            }
        }


        private void ReturnList()
        {
            try
            {
                DataTable dtINDATA = new DataTable();
                DataTable dtOUTDATA = new DataTable();

                dtINDATA.Columns.Add("MTRL_CD", typeof(string));
                dtINDATA.Columns.Add("LANGID", typeof(string));
                dtINDATA.Columns.Add("CARRIER_ID", typeof(string));
                dtINDATA.Columns.Add("FROM_DATE", typeof(string));
                dtINDATA.Columns.Add("TO_DATE", typeof(string));
                dtINDATA.Columns.Add("MTRL_TYPE", typeof(string));

                DataRow Indata = dtINDATA.NewRow();
                Indata["MTRL_TYPE"] = cboMtrlTypeT2.GetBindValue();

                if (string.IsNullOrEmpty(txtPalletT2.Text))
                {
                    Indata["MTRL_CD"] = popSearchMtrlT3.SelectedValue == null ? null : popSearchMtrlT3.SelectedValue.ToString(); ;
                    Indata["LANGID"] = LoginInfo.LANGID;
                    Indata["MTRL_TYPE"] = cboMtrlTypeT2.GetBindValue();

                    if (chkUI_DateT2.IsChecked == true)
                    {
                        Indata["FROM_DATE"] = dtpStartT2.SelectedDateTime.ToString("yyyyMMdd");
                        Indata["TO_DATE"] = dtpEndT2.SelectedDateTime.ToString("yyyyMMdd");
                    }
                }
                else
                {
                    Indata["LANGID"] = LoginInfo.LANGID;
                    Indata["CARRIER_ID"] = txtPalletT2.Text == string.Empty ? null : txtPalletT2.Text;
                }

                dtINDATA.Rows.Add(Indata);

                string bizRule = "DA_INV_SEL_MTRL_RETURN";

                loadingIndicator.Visibility = Visibility.Visible;

                new ClientProxy().ExecuteService(bizRule, "RQSTDT", "RSLTDT", dtINDATA, (dtResult, ex) =>
                {
                    loadingIndicator.Visibility = Visibility.Collapsed;
                    if (ex != null)
                    {
                        Util.MessageException(ex); //Util.AlertInfo(ex.Message);
                        return;
                    }

                    if (dtResult == null || dtResult.Rows.Count == 0)
                    {
                        Util.MessageValidation("SFU3537");  //조회된 데이터가 없습니다.
                        Util.GridSetData(dgListReturn, dtResult, FrameOperation, true);
                        return;
                    }

                    Util.GridSetData(dgListReturn, dtResult, FrameOperation, true);
                });
            }
            catch (Exception ex)
            {
                loadingIndicator.Visibility = Visibility.Collapsed;
                Util.MessageException(ex);
            }
        }


        #endregion


        #region [ Util ]
        private void ApplyPermissions()
        {
            List<Button> listAuth = new List<Button>();
            Util.pageAuth(listAuth, FrameOperation.AUTHORITY);
        }
        #endregion

        #region 동정보 콤보박스 조회 : SetAreaCombo()

        /// <summary>
        ///// 동정보 조회
        ///// </summary>
        ///// <param name="cbo"></param>
        private static void SetAreaCombo(C1ComboBox cbo)
        {
            const string bizRuleName = "DA_BAS_SEL_AREA_LOGIS_GROUP_MTRL_CBO";
            string[] arrColumn = { "LANGID", "AREAID" };

            string[] arrCondition = { LoginInfo.LANGID, LoginInfo.CFG_AREA_ID };
            string selectedValueText = cbo.SelectedValuePath;
            string displayMemberText = cbo.DisplayMemberPath;

            CommonCombo.CommonBaseCombo(bizRuleName, cbo, arrColumn, arrCondition, CommonCombo.ComboStatus.NONE, selectedValueText, displayMemberText, LoginInfo.CFG_AREA_ID);
        }




        #endregion

        #region 자재 정보 조회 : SearchMtrl()

        /// <summary>
        /// 기본 자재 정보 조회
        /// </summary>
        /// <param name="cbo"></param>
        private void SearchMtrl_Group(object sender = null)
        {
            try
            {
                DataTable inDataTable = new DataTable();
                inDataTable.Columns.Add("FACILITY_CODE", typeof(string));
                inDataTable.Columns.Add("MTRL_TYPE", typeof(string));
                DataRow newRow = inDataTable.NewRow();
                newRow["FACILITY_CODE"] = LoginInfo.CFG_AREA_ID;

                C1ComboBox cbx = sender as C1ComboBox;
                if ( cbx != null)
                    newRow["MTRL_TYPE"] = cbx.GetBindValue();

                inDataTable.Rows.Add(newRow);
                new ClientProxy().ExecuteService("DA_INV_SEL_MTRL_ID", "INDATA", "OUTDATA", inDataTable, (searchResult, searchException) =>
                {
                    try
                    {

                        if (searchException != null)
                        {
                            Util.MessageException(searchException);
                            return;
                        }
                        popSearchMtrl.ItemsSource = DataTableConverter.Convert(searchResult);
                        popSearchMtrlT1.ItemsSource = DataTableConverter.Convert(searchResult);
                        popSearchMtrlT2.ItemsSource = DataTableConverter.Convert(searchResult);
                        popSearchMtrlT3.ItemsSource = DataTableConverter.Convert(searchResult);

                    }
                    catch (Exception ex)
                    {

                        Util.MessageException(ex);
                    }
                });
            }
            catch (Exception ex)
            {

                Util.MessageException(ex);
            }
        }


        /// <summary>
        /// 자재 정보 조회
        /// </summary>

        private void SearchMtrl()
        {
            try
            {
                if (List.IsSelected == true)
                {
                    if (popSearchMtrl.SelectedValue == null || string.IsNullOrWhiteSpace(popSearchMtrl.SelectedValue.ToString()))
                    {
                        popSearchMtrl.SelectedValue = null;
                        popSearchMtrl.SelectedText = null;
                        popSearchMtrl.ItemsSource = null;

                    }
                    DataTable inDataTable = new DataTable();
                    inDataTable.Columns.Add("FACILITY_CODE", typeof(string));
                    inDataTable.Columns.Add("MTRLID", typeof(string));
                    DataRow newRow = inDataTable.NewRow();
                    newRow["FACILITY_CODE"] = LoginInfo.CFG_AREA_ID;
                    newRow["MTRLID"] = popSearchMtrl.SelectedValue == null ? null : popSearchMtrl.SelectedValue.ToString();
                    inDataTable.Rows.Add(newRow);
                    new ClientProxy().ExecuteService("DA_INV_SEL_MTRL_ID", "INDATA", "OUTDATA", inDataTable, (searchResult, searchException) =>
                    {
                        try
                        {

                            if (searchException != null)
                            {
                                Util.MessageException(searchException);
                                return;
                            }
                            popSearchMtrl.ItemsSource = DataTableConverter.Convert(searchResult);
                        }
                        catch (Exception ex)
                        {

                            Util.MessageException(ex);
                        }
                    });
                }
                else if (HISTORY.IsSelected == true)
                {
                    if (popSearchMtrlT1.SelectedValue == null || string.IsNullOrWhiteSpace(popSearchMtrlT1.SelectedValue.ToString()))
                    {
                        popSearchMtrlT1.SelectedValue = null;
                        popSearchMtrlT1.SelectedText = null;
                        popSearchMtrlT1.ItemsSource = null;

                    }
                    DataTable inDataTable = new DataTable();
                    inDataTable.Columns.Add("FACILITY_CODE", typeof(string));
                    inDataTable.Columns.Add("MTRLID", typeof(string));
                    inDataTable.Columns.Add("MTRL_TYPE", typeof(string));
                    DataRow newRow = inDataTable.NewRow();
                    newRow["FACILITY_CODE"] = LoginInfo.CFG_AREA_ID;
                    newRow["MTRLID"] = popSearchMtrlT1.SelectedValue == null ? null : popSearchMtrlT1.SelectedValue.ToString();
                    newRow["MTRL_TYPE"] = cboMtrlType.GetBindValue();
                    inDataTable.Rows.Add(newRow);
                    new ClientProxy().ExecuteService("DA_INV_SEL_MTRL_ID", "INDATA", "OUTDATA", inDataTable, (searchResult, searchException) =>
                    {
                        try
                        {
                            if (searchException != null)
                            {
                                Util.MessageException(searchException);
                                return;
                            }
                            popSearchMtrlT1.ItemsSource = DataTableConverter.Convert(searchResult);
                        }
                        catch (Exception ex)
                        {
                            Util.MessageException(ex);
                        }
                    });
                }
                else if (LOC_LIST.IsSelected == true)
                {
                    if (popSearchMtrlT2.SelectedValue == null || string.IsNullOrWhiteSpace(popSearchMtrlT2.SelectedValue.ToString()))
                    {
                        popSearchMtrlT2.SelectedValue = null;
                        popSearchMtrlT2.SelectedText = null;
                        popSearchMtrlT2.ItemsSource = null;
                    }
                    DataTable inDataTable = new DataTable();
                    inDataTable.Columns.Add("FACILITY_CODE", typeof(string));
                    inDataTable.Columns.Add("MTRLID", typeof(string));
                    inDataTable.Columns.Add("MTRL_TYPE", typeof(string));
                    DataRow newRow = inDataTable.NewRow();
                    newRow["FACILITY_CODE"] = LoginInfo.CFG_AREA_ID;
                    newRow["MTRLID"] = popSearchMtrlT2.SelectedValue == null ? null : popSearchMtrlT2.SelectedValue.ToString();
                    newRow["MTRL_TYPE"] = cboMtrlTypeT1.GetBindValue();
                    inDataTable.Rows.Add(newRow);
                    new ClientProxy().ExecuteService("DA_INV_SEL_MTRL_ID", "INDATA", "OUTDATA", inDataTable, (searchResult, searchException) =>
                    {
                        try
                        {
                            if (searchException != null)
                            {
                                Util.MessageException(searchException);
                                return;
                            }
                            popSearchMtrlT2.ItemsSource = DataTableConverter.Convert(searchResult);
                        }
                        catch (Exception ex)
                        {
                            Util.MessageException(ex);
                        }
                    });
                }
                else if (RETURN_LIST.IsSelected == true)
                {
                    if (popSearchMtrlT3.SelectedValue == null || string.IsNullOrWhiteSpace(popSearchMtrlT3.SelectedValue.ToString()))
                    {
                        popSearchMtrlT3.SelectedValue = null;
                        popSearchMtrlT3.SelectedText = null;
                        popSearchMtrlT3.ItemsSource = null;
                    }
                    DataTable inDataTable = new DataTable();
                    inDataTable.Columns.Add("FACILITY_CODE", typeof(string));
                    inDataTable.Columns.Add("MTRL_TYPE", typeof(string));
                    inDataTable.Columns.Add("MTRLID", typeof(string));
                    DataRow newRow = inDataTable.NewRow();
                    newRow["FACILITY_CODE"] = LoginInfo.CFG_AREA_ID;
                    newRow["MTRLID"] = popSearchMtrlT3.SelectedValue == null ? null : popSearchMtrlT3.SelectedValue.ToString();
                    newRow["MTRL_TYPE"] = cboMtrlTypeT2.GetBindValue();
                    inDataTable.Rows.Add(newRow);
                    new ClientProxy().ExecuteService("DA_INV_SEL_MTRL_ID", "INDATA", "OUTDATA", inDataTable, (searchResult, searchException) =>
                    {
                        try
                        {
                            if (searchException != null)
                            {
                                Util.MessageException(searchException);
                                return;
                            }
                            popSearchMtrlT3.ItemsSource = DataTableConverter.Convert(searchResult);
                        }
                        catch (Exception ex)
                        {
                            Util.MessageException(ex);
                        }
                    });
                }
            }
            catch (Exception ex)
            {

                Util.MessageException(ex);
            }
        }

        #endregion


        #region 자재ID 조회 : popSearchMtrl_ValueChanged()

        private void popSearchMtrl_ValueChanged(object sender, EventArgs e)
        {
            SearchMtrl();
        }
        #endregion

        #region 자재유형 : SetMtrlType()
        private static void SetMtrlType(C1ComboBox cbo)
        {
            const string bizRuleName = "DA_INV_SEL_TB_EGN_CODE_ITEM_INFO_CBO";
            string[] arrColumn = { "LANGID", "FACILITY_CODE", "BUSINESS_USAGE_TYPE_CODE", "USE_FLAG", "ATTRIBUTE11" };
            string[] arrCondition = { LoginInfo.LANGID, LoginInfo.CFG_AREA_ID, "INV_MATERIAL_CATEGORY", "Y", "Y" };
            string selectedValueText = cbo.SelectedValuePath;
            string displayMemberText = cbo.DisplayMemberPath;

            CommonCombo.CommonBaseCombo(bizRuleName, cbo, arrColumn, arrCondition, CommonCombo.ComboStatus.ALL, selectedValueText, displayMemberText, string.Empty);
        }

        #endregion

        private void dgSummary_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            try
            {
                C1.WPF.DataGrid.C1DataGrid dg = sender as C1.WPF.DataGrid.C1DataGrid;
                if (dg.CurrentColumn != null && dg.CurrentColumn.Name.Equals("TOT_QTY") && dg.GetRowCount() > 0 && dg.CurrentRow != null)
                {
                    DetailData(Util.NVC(DataTableConverter.GetValue(dg.CurrentRow.DataItem, "MATERIAL_CD")), string.Empty);
                }
                else if (dg.CurrentColumn != null && dg.CurrentColumn.Name.Equals("MATERIAL_LOC_QTY") && dg.GetRowCount() > 0 && dg.CurrentRow != null)
                {
                    if(Convert.ToDecimal(Util.NVC(DataTableConverter.GetValue(dg.CurrentRow.DataItem, "MATERIAL_LOC_QTY"))) > 0)
                    {
                        DetailData(Util.NVC(DataTableConverter.GetValue(dg.CurrentRow.DataItem, "MATERIAL_CD")), Util.NVC(DataTableConverter.GetValue(dg.CurrentRow.DataItem, "MATERIAL_LOC")));
                    }
                    else
                    {
                        Util.gridClear(dgDetail);
                    }
                }
                else if (dg.CurrentColumn != null && dg.CurrentColumn.Name.Equals("STO_LOC_QTY") && dg.GetRowCount() > 0 && dg.CurrentRow != null)
                {
                    if (Convert.ToDecimal(Util.NVC(DataTableConverter.GetValue(dg.CurrentRow.DataItem, "STO_LOC_QTY"))) > 0)
                    {
                        DetailData(Util.NVC(DataTableConverter.GetValue(dg.CurrentRow.DataItem, "MATERIAL_CD")), Util.NVC(DataTableConverter.GetValue(dg.CurrentRow.DataItem, "STO_LOC")));
                    }
                    else
                    {
                        Util.gridClear(dgDetail);
                    }
                }
                else if (dg.CurrentColumn != null && dg.CurrentColumn.Name.Equals("PROC_LOC_QTY") && dg.GetRowCount() > 0 && dg.CurrentRow != null)
                {
                    if (Convert.ToDecimal(Util.NVC(DataTableConverter.GetValue(dg.CurrentRow.DataItem, "PROC_LOC_QTY"))) > 0)
                    {
                        DetailData(Util.NVC(DataTableConverter.GetValue(dg.CurrentRow.DataItem, "MATERIAL_CD")), Util.NVC(DataTableConverter.GetValue(dg.CurrentRow.DataItem, "PROC_LOC")));
                    }
                    else
                    {
                        Util.gridClear(dgDetail);
                    }
                }
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

        private void dgSummary_LoadedCellPresenter(object sender, DataGridCellEventArgs e)
        {
            dgSummary.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (e.Cell.Presenter == null)
                {
                    return;
                }

                //link 색변경
                if (e.Cell.Column.Name.Equals("MATERIAL_LOC_QTY")|| e.Cell.Column.Name.Equals("STO_LOC_QTY") || e.Cell.Column.Name.Equals("PROC_LOC_QTY") || e.Cell.Column.Name.Equals("TOT_QTY"))
                {
                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Blue);
                }
                else
                {
                    e.Cell.Presenter.Foreground = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#FF333333"));
                }

            

            }));
        }

        private void btnSearchT1_Click(object sender, RoutedEventArgs e)
        {
            HistData();
        }

        private void txtCellID_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                HistData();
            }
        }

        private void SetOutputWHCombo(C1ComboBox cbo)
        {
            const string bizRuleName = "DA_INV_SEL_TB_EGN_CODE_ITEM_INFO_LOC_CBO";
            string[] arrColumn = { "LANGID", "FACILITY_CODE", "BUSINESS_USAGE_TYPE_CODE", "USE_FLAG", "ATTRIBUTE13" };
            string[] arrCondition = { LoginInfo.LANGID, LoginInfo.CFG_AREA_ID, "INV_WH_LOCATE", "Y","Y"};
            string selectedValueText = cbo.SelectedValuePath;
            string displayMemberText = cbo.DisplayMemberPath;

            CommonCombo.CommonBaseCombo(bizRuleName, cbo, arrColumn, arrCondition, CommonCombo.ComboStatus.ALL, selectedValueText, displayMemberText, string.Empty);
        }

        private void btnSearchT2_Click(object sender, RoutedEventArgs e)
        {
            LocList();
        }

        private void dgDetail_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            try
            {
                Point pnt = e.GetPosition(null);
                C1.WPF.DataGrid.DataGridCell cell = dgDetail.GetCellFromPoint(pnt);

                if (cell != null)
                {
                    if (cell.Column.Name == "CARRIER_ID")
                    {
                        if (!string.IsNullOrWhiteSpace(Util.NVC(DataTableConverter.GetValue(dgDetail.Rows[cell.Row.Index].DataItem, cell.Column.Name))))
                        {
                            tabMain.SelectedItem = HISTORY;
                            txtPallet.Text = Util.NVC(DataTableConverter.GetValue(dgDetail.Rows[cell.Row.Index].DataItem, cell.Column.Name));
                            btnSearchT1.PerformClick();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void dgDetail_LoadedCellPresenter(object sender, DataGridCellEventArgs e)
        {
            dgSummary.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (e.Cell.Presenter == null)
                {
                    return;
                }

                //link 색변경
                if (e.Cell.Column.Name.Equals("CARRIER_ID"))
                {
                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Blue);
                }
                else
                {
                    e.Cell.Presenter.Foreground = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#FF333333"));
                }

            }));
        }

        #region 타이머 콤보박스 이벤트  : cboTimer_SelectedValueChanged()
        private void cboTimer_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            if (!_isLoaded) return;
            try
            {
                if (_monitorTimer != null)
                {
                    _monitorTimer.Stop();

                    int second = 0;
                    if (!string.IsNullOrEmpty(cboTimer?.SelectedValue?.GetString()))
                    {
                        second = int.Parse(cboTimer.SelectedValue.ToString());
                        _isSelectedAutoTime = true;
                    }
                    else
                    {
                        _isSelectedAutoTime = false;
                    }

                    if (second == 0 && !_isSelectedAutoTime)
                    {
                        Util.MessageValidation("SFU8310");
                        return;
                    }
                    _monitorTimer.Interval = new TimeSpan(0, 0, second);
                    _monitorTimer.Start();

                    if (_isSelectedAutoTime)
                    {
                        if (cboTimer != null)
                            Util.MessageInfo("SFU8311", Convert.ToString(Convert.ToInt32(cboTimer.SelectedValue) / 60));
                    }
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        #endregion

        #region 타이머 셋팅 : TimerSetting()

        /// <summary>
        /// 타이머 셋팅
        /// </summary>
        private void TimerSetting()
        {
            CommonCombo combo = new CommonCombo();
            string[] filter = { "INTERVAL_MIN" };
            combo.SetCombo(cboTimer, CommonCombo.ComboStatus.NA, sFilter: filter, sCase: "COMMCODE");

            if (cboTimer != null && cboTimer.Items.Count > 0)
                cboTimer.SelectedIndex = 3;

            if (_monitorTimer != null)
            {
                int second = 0;

                if (!string.IsNullOrEmpty(cboTimer?.SelectedValue?.ToString()))
                    second = int.Parse(cboTimer.SelectedValue.ToString());

                _monitorTimer.Tick += _dispatcherTimer_Tick;
                _monitorTimer.Interval = new TimeSpan(0, 0, second);

                _monitorTimer.Start();
            }
        }

        #endregion

        #region Timer 실행 : _dispatcherTimer_Tick()
        /// <summary>
        /// 타이머 실행 : 조회 버튼 호출
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void _dispatcherTimer_Tick(object sender, EventArgs e)
        {
            if (sender == null)
                return;
            if (LOC_LIST.IsSelected == false)
                return;
            DispatcherTimer dpcTmr = sender as DispatcherTimer;
            dpcTmr?.Dispatcher.BeginInvoke(new Action(() =>
            {
                try
                {
                    dpcTmr.Stop();
                    if (Math.Abs(dpcTmr.Interval.TotalSeconds) < 1) return;


                    btnSearchT2_Click(null, null);


                }
                catch (Exception ex)
                {
                    Util.MessageException(ex);
                }
                finally
                {
                    if (dpcTmr.Interval.TotalSeconds > 0)
                        dpcTmr.Start();
                }
            }));
        }

        #endregion

        private void txtCellIDT1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                LocList();
            }
        }

        private void btnSearchT3_Click(object sender, RoutedEventArgs e)
        {
            ReturnList();
        }

        private void cboMtrlType_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            SearchMtrl_Group(sender);
        }

        private void cboMtrlTypeT1_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            SearchMtrl_Group(sender);
        }
        private void cboMtrlTypeT2_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            SearchMtrl_Group(sender);
        }

        private void chkUI_DateT1_Click(object sender, RoutedEventArgs e)
        {
            if (chkUI_DateT1.IsChecked == true)
            {
                dtpStartT1.IsEnabled = true;
                dtpEndT1.IsEnabled = true;
            }
            else
            {
                dtpStartT1.IsEnabled = false;
                dtpEndT1.IsEnabled = false;
            }
        }

        private void chkUI_DateT2_Click(object sender, RoutedEventArgs e)
        {
            if (chkUI_DateT2.IsChecked == true)
            {
                dtpStartT2.IsEnabled = true;
                dtpEndT2.IsEnabled = true;
            }
            else
            {
                dtpStartT2.IsEnabled = false;
                dtpEndT2.IsEnabled = false;
            }
        }

        private void dgHist_LoadedCellPresenter(object sender, DataGridCellEventArgs e)
        {
            dgHist.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (e.Cell.Presenter == null)
                {
                    return;
                }
                if (e.Cell.Row.Type == DataGridRowType.Item)
                {
                    if (Util.NVC(DataTableConverter.GetValue(dgHist.Rows[e.Cell.Row.Index].DataItem, "HISTORY_CANCEL_FLAG")).Equals("C"))
                    {
                      
                        e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Red);
                    }
                    else
                    {
                        e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Black);
                    }
                }              
            }));
        }

        

        private void dgListLoc_MergingCells(object sender, DataGridMergingCellsEventArgs e)
        {
            try
            {
                //STO 그룹에 따라 MAX CAPA 수량 및 설정 재고수량 머지             
                int idxS = 0;
                int idxE = 0;
                bool bStrt = false;
                string sTmpLvCd = string.Empty;
                string sTmpTOTALQTY = string.Empty;

                for (int i = dgListLoc.TopRows.Count; i < dgListLoc.Rows.Count - 1; i++)
                {

                    if (dgListLoc.Rows[i].DataItem.GetType() == typeof(System.Data.DataRowView))
                    {

                        if (!bStrt)
                        {
                            bStrt = true;
                            sTmpLvCd = Util.NVC(DataTableConverter.GetValue(dgListLoc.Rows[i].DataItem, "REP_PROCESSING_GROUP_ID"));
                            idxS = i;

                            if (sTmpLvCd.Equals(""))
                                bStrt = false;
                        }
                        else
                        {
                            if (Util.NVC(DataTableConverter.GetValue(dgListLoc.Rows[i].DataItem, "REP_PROCESSING_GROUP_ID")).Equals(sTmpLvCd))
                            {
                                idxE = i;
                                //마지막 Row 일경우
                                if (i == dgListLoc.Rows.Count - 1)
                                {
                                    if (idxS > idxE)
                                    {
                                        idxE = idxS;
                                    }
                                    e.Merge(new DataGridCellsRange(dgListLoc.GetCell(idxS, dgListLoc.Columns["CHK"].Index), dgListLoc.GetCell(idxE, dgListLoc.Columns["CHK"].Index)));
                                }
                            }
                            else
                            {
                                if (idxS > idxE)
                                {
                                    idxE = idxS;
                                }
                                e.Merge(new DataGridCellsRange(dgListLoc.GetCell(idxS, dgListLoc.Columns["CHK"].Index), dgListLoc.GetCell(idxE, dgListLoc.Columns["CHK"].Index)));


                                bStrt = true;
                                sTmpLvCd = Util.NVC(DataTableConverter.GetValue(dgListLoc.Rows[i].DataItem, "REP_PROCESSING_GROUP_ID"));
                                idxS = i;

                                if (sTmpLvCd.Equals(""))
                                    bStrt = false;
                            }
                        }
                    }

                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }

        }

        private void btnDeleteT2_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                const string bizRule = "BR_INV_DEL_MTRL_LIST";

                DataTable inTable = new DataTable();
                inTable.Columns.Add("GROUP_ID", typeof(string));
                inTable.Columns.Add("USER_ID", typeof(string));
                DataRow newRow = inTable.NewRow();

                for (int i = 0; i < dgListLoc.Rows.Count - dgListLoc.BottomRows.Count; i++)
                {
                    if (!_Util.GetDataGridCheckValue(dgListLoc, "CHK", i)) continue;

                    newRow = inTable.NewRow();
                    newRow["GROUP_ID"] = Util.NVC(DataTableConverter.GetValue(dgListLoc.Rows[i].DataItem, "REP_PROCESSING_GROUP_ID"));
                    newRow["USER_ID"] = LoginInfo.USERID;

                    inTable.Rows.Add(newRow);
                }

                if (inTable == null || inTable.Rows.Count == 0)
                {
                    Util.MessageValidation("SFU1644");  // 선택된 자재가 없습니다
                    return;
                }

                loadingIndicator.Visibility = Visibility.Visible;

                Util.MessageConfirm("SFU9059", (result) =>
                {
                    if (result == MessageBoxResult.OK)
                    {
                        new ClientProxy().ExecuteService(bizRule, "INDATA", null, inTable, (dtResult, ex) =>
                        {
                            loadingIndicator.Visibility = Visibility.Collapsed;
                            if (ex != null)
                            {
                                Util.MessageException(ex);
                                return;
                            }

                            LocList();

                            Util.MessageInfo("SFU1275");    //정상 처리 되었습니다.
                        });
                    }
                });
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                loadingIndicator.Visibility = Visibility.Collapsed;
            }
        }

        private string ChkInputWhLocation()
        {
            string rslt = "N";
            try
            {
                DataTable dt = new DataTable("RQSTDT");
                dt.Columns.Add("BUSINESS_USAGE_TYPE_CODE", typeof(string));
                dt.Columns.Add("USE_FLAG", typeof(string));

                DataRow dr = dt.NewRow();
                dr["BUSINESS_USAGE_TYPE_CODE"] = "INV_INPUT_WH_LOCATE";
                dr["USE_FLAG"] = "Y";
                dt.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_INV_SEL_TB_EGN_CODE_ITEM_INFO", "RQSTDT", "RSLTDT", dt);

                if (dtResult?.Rows?.Count > 0)
                {
                    rslt = dtResult.Rows[0]["BUSINESS_USAGE_CODE_ID"].ToString();
                }

                return rslt;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
            return rslt;
        }
    }
}

