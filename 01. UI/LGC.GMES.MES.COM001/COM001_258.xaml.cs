/*************************************************************************************
 Created Date : 2019.04.19
      Creator : 
   Decription : 선감지 모니터링
--------------------------------------------------------------------------------------
 [Change History]
  2019.04.19  오화백 : Initial Created.

**************************************************************************************/
using System;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using C1.WPF.DataGrid;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using System.Configuration;
using System.IO;
using C1.WPF.Excel;
using System.Windows.Media;
using System.Globalization;

using LGC.GMES.MES.CMM001;
using LGC.GMES.MES.CMM001.Extensions;
using System.Windows.Input;
using LGC.GMES.MES.CMM001.Popup;
using System.Collections.Generic;
using System.Linq;

using C1.WPF;

namespace LGC.GMES.MES.COM001
{
    /// <summary>
    /// COM001_081.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class COM001_258 : UserControl
    {
        #region Private 변수
        CommonCombo _combo = new CMM001.Class.CommonCombo();
        DataTable dtHistList = new DataTable();
        #endregion

        #region Form Load & Init Control
        public IFrameOperation FrameOperation
        {
            get;
            set;
        }

        public COM001_258()
        {
            InitializeComponent();
            ComboSeting();
           
        }
 
        private void ComboSeting()
        {
                       
            String[] sFilter1 = { "ERLY_DETT_GR1_CODE" };//선감지그룹
            String[] sFilter2 = { "JUDGE_OK" };//상태
            String[] sFilter3 = { "IUSE" };//사용여부


            #region 선감지현황 Combo Setting
            _combo.SetCombo(cboShop, CommonCombo.ComboStatus.ALL, sCase: "SHOP");  //SHOP
            _combo.SetCombo(cboArea, CommonCombo.ComboStatus.ALL, sCase: "cboArea");  //동

            C1ComboBox[] cboGroupCurrentChild = { cboItemCurrent };
            _combo.SetCombo(cboGroupCurrent, CommonCombo.ComboStatus.ALL, sFilter: sFilter1, sCase: "COMMCODE", cbChild: cboGroupCurrentChild); //선감지그룹코드
            C1ComboBox[] cboItemCurrentParent = { cboGroupCurrent };
            _combo.SetCombo(cboItemCurrent, CommonCombo.ComboStatus.ALL, sCase: "ERLY_DETT_ITEM", cbParent: cboItemCurrentParent);  //아이템

            _combo.SetCombo(cboStatCurrent, CommonCombo.ComboStatus.ALL, sFilter: sFilter2, sCase: "COMMCODE");  //상태
        

            #endregion

            #region 선감지이력 Combo Setting
            _combo.SetCombo(cboShop_history, CommonCombo.ComboStatus.ALL, sCase: "SHOP");
            _combo.SetCombo(cboArea, CommonCombo.ComboStatus.ALL, sCase: "cboArea");

            C1ComboBox[] cboGroupHistoryChild = { cboItem_history };
            _combo.SetCombo(cboGroup_history, CommonCombo.ComboStatus.ALL, sFilter: sFilter1, sCase: "COMMCODE", cbChild: cboGroupHistoryChild); //선감지그룹코드

            C1ComboBox[] cboItemHistoryParent = { cboGroup_history };
            _combo.SetCombo(cboItem_history, CommonCombo.ComboStatus.ALL, sCase: "ERLY_DETT_ITEM", cbParent: cboItemHistoryParent);  //아이템

            _combo.SetCombo(cboStat_history, CommonCombo.ComboStatus.ALL, sFilter: sFilter2, sCase: "COMMCODE");  //상태
         
            _combo.SetCombo(cboUseYN, CommonCombo.ComboStatus.ALL, sFilter: sFilter3, sCase: "COMMCODE");  //사용여부
            #endregion
        }
        #endregion

        #region Events

        #region 선감지현황

        #region 엑셀업로드 : ExcelUpload_Click()
        /// <summary>
        /// 엑셀업로드
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        /// 
        private void ExcelUpload_Click(object sender, RoutedEventArgs e)
        {
            GetExcel();
        }
        #endregion

        #region 선감지현황 조회 : btnSearchHistCurrent_Click()
        private void btnSearchHistCurrent_Click(object sender, RoutedEventArgs e)
        {
            GetCurrent();
        }

        #endregion

        #endregion

        #region 선감지이력

        #region 선감지이력 조회 : btnSearchHist_Click()
        /// <summary>
        /// 이력조회
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnSearchHist_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidationSearchBoxPrint())
                return;

            GetListHist();
        }


        #endregion

        #endregion

        #endregion

        #region Functions

        #region 선감지현황

        #region 엑셀업로드 :  GetExcel(),LoadExcel()

        void GetExcel()
        {
            try
            {
                Microsoft.Win32.OpenFileDialog fd = new Microsoft.Win32.OpenFileDialog();

                if (ConfigurationManager.AppSettings["APP_CONFIG_SBC_MODE"] == "SBC")
                {
                    fd.InitialDirectory = @"\\Client\C$";
                }

                fd.Filter = "Excel Files (.xlsx)|*.xlsx";
                if (fd.ShowDialog() == true)
                {
                    using (Stream stream = fd.OpenFile())
                    {
                        LoadExcel(stream, (int)0);
                    }
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        void LoadExcel(Stream excelFileStream, int sheetNo)
        {
            //            try
            //            {
            //                excelFileStream.Seek(0, SeekOrigin.Begin);
            //                C1XLBook book = new C1XLBook();
            //                book.Load(excelFileStream, FileFormat.OpenXml);
            //                XLSheet sheet = book.Sheets[sheetNo];

            //                if (sheet == null)
            //                {
            //                    //업로드한 엑셀파일의 데이타가 잘못되었습니다. 확인 후 다시 처리하여 주십시오.
            //                    Util.MessageValidation("9017");
            //                    return;
            //                }

            //                // 해더 제외
            //                DataTable dt = DataTableConverter.Convert(dgTransfer.ItemsSource).Clone();

            //                for (int rowInx = 1; rowInx < sheet.Rows.Count; rowInx++)
            //                {
            //                    DataRow dr = dt.NewRow();

            //                    if (sheet.GetCell(rowInx, 0) == null)
            //                        dr["SHOPID"] = "";
            //                    else
            //                        dr["SHOPID"] = Util.NVC(sheet.GetCell(rowInx, 0).Text);

            //                    if (sheet.GetCell(rowInx, 1) == null)
            //                        dr["CALDATE"] = "";
            //                    else
            //                        dr["CALDATE"] = Util.NVC(sheet.GetCell(rowInx, 1).Text);

            //                    if (sheet.GetCell(rowInx, 2) == null)
            //                        dr["PRODID"] = "";
            //                    else
            //                        dr["PRODID"] = Util.NVC(sheet.GetCell(rowInx, 2).Text);

            //                    if (sheet.GetCell(rowInx, 3) == null)
            //                        dr["UNIT_CODE"] = "";
            //                    else
            //                        dr["UNIT_CODE"] = Util.NVC(sheet.GetCell(rowInx, 3).Text);

            //                    if (sheet.GetCell(rowInx, 4) == null)
            //                        dr["CNFM_LOSS_QTY2"] = 0;
            //                    else
            //                        dr["CNFM_LOSS_QTY2"] = Util.NVC(sheet.GetCell(rowInx, 4).Text);

            //                    if (sheet.GetCell(rowInx, 5) == null)
            //                        dr["LENGTH_EXCEED2"] = 0;
            //                    else
            //                        dr["LENGTH_EXCEED2"] = Util.NVC(sheet.GetCell(rowInx, 5).Text);

            //                    if (sheet.GetCell(rowInx, 6) == null)
            //                        dr["SLOC_ID"] = "";
            //                    else
            //                        dr["SLOC_ID"] = Util.NVC(sheet.GetCell(rowInx, 6).Text);

            ////                    if (sheet.GetCell(rowInx, 7) == null)
            ////                        dr["DFCT_SLOC_ID"] = "";
            ////                    else
            ////                        dr["DFCT_SLOC_ID"] = Util.NVC(sheet.GetCell(rowInx, 7).Text);

            //                    //if (!string.IsNullOrWhiteSpace(dr["SHOPID"].ToString()))
            //                    //    CommonCombo.SetDataGridComboItem("DA_BAS_SEL_SLOC_BY_SHOP", new string[] { "SHOPID" }, new string[] { dr["SHOPID"].ToString() }, CommonCombo.ComboStatus.NONE, dgTransfer.Columns["SLOC_ID"], "CBO_CODE", "CBO_NAME");

            //                    dt.Rows.Add(dr);
            //                }

            //                dt.AcceptChanges();
            //                Util.GridSetData(dgTransfer, dt, FrameOperation, true);

            //            }
            //            catch (Exception ex)
            //            {
            //                Util.MessageException(ex);
            //            }

        }

        #endregion

        #region 선감지현황 조회 : GetCurrent
        public void GetCurrent()
        {
            try
            {
                ShowLoadingIndicator();
                DataTable dtRqst = new DataTable();
                dtRqst.Columns.Add("LANGID", typeof(string));
                dtRqst.Columns.Add("SHOPID", typeof(string));
                dtRqst.Columns.Add("AREAID", typeof(string));
                dtRqst.Columns.Add("ERLY_DETT_GR1_CODE", typeof(string));
                dtRqst.Columns.Add("ERLY_DETT_ITEM_ID", typeof(string));
                dtRqst.Columns.Add("ERLY_DETT_STAT_CODE", typeof(string));


                DataRow dr = dtRqst.NewRow();

                dr["LANGID"] = LoginInfo.LANGID;
                dr["SHOPID"] = null;
                dr["AREAID"] = null;
                dr["ERLY_DETT_GR1_CODE"] = cboGroupCurrent.SelectedValue.ToString() == string.Empty ? null : cboGroupCurrent.SelectedValue.ToString();
                dr["ERLY_DETT_ITEM_ID"] = cboItemCurrent.SelectedValue.ToString() == string.Empty ? null : cboItemCurrent.SelectedValue.ToString();
                dr["ERLY_DETT_STAT_CODE"] = cboStatCurrent.SelectedValue.ToString() == string.Empty ? null : cboStatCurrent.SelectedValue.ToString();

                dtRqst.Rows.Add(dr);

                new ClientProxy().ExecuteService("DA_SEL_TB_SOM_ERLY_DETT_EXE_REC", "RQSTDT", "RSLTDT", dtRqst, (bizResult, bizException) =>
                {
                    HiddenLoadingIndicator();
                    if (bizException != null)
                    {
                        Util.MessageException(bizException);
                        return;
                    }

                    //Util.GridSetData(dgInputRank, bizResult, null, true);
                    Util.GridSetData(dgListCurrent, bizResult, null, true);
                });


                //DataTable dtRslt = new DataTable();
                //dtRslt = new ClientProxy().ExecuteServiceSync("DA_SEL_TB_SOM_ERLY_DETT_EXE_REC", "INDATA", "OUTDATA", dtRqst);
                //Util.GridSetData(dgListCurrent, dtRslt, FrameOperation, true);

            }
            catch (Exception ex)
            {
                HiddenLoadingIndicator();
                Util.MessageException(ex);
            }
        }
        #endregion

        #endregion

        #region 선감지 이력 조회

        #region Validation : ValidationSearchBoxPrint()
        private bool ValidationSearchBoxPrint()
        {
            DateTime dtEndTime = new DateTime(dtpDateTo.SelectedDateTime.Year, dtpDateTo.SelectedDateTime.Month, dtpDateTo.SelectedDateTime.Day);
            DateTime dtStartTime = new DateTime(dtpDateFrom.SelectedDateTime.Year, dtpDateFrom.SelectedDateTime.Month, dtpDateFrom.SelectedDateTime.Day);

            if (!(Math.Truncate(dtEndTime.Subtract(dtStartTime).TotalSeconds) >= 0))
            {
                //종료일자가 시작일자보다 빠릅니다.
                Util.MessageValidation("SFU1913");
                return false;
            }
            return true;
        }

        #endregion

        #region 이력 조회

        public void GetListHist()
        {
            try
            {
                ShowLoadingIndicator();
                DataTable dtRqst = new DataTable();
                dtRqst.Columns.Add("LANGID", typeof(string));
                dtRqst.Columns.Add("SHOPID", typeof(string));
                dtRqst.Columns.Add("AREAID", typeof(string));
                dtRqst.Columns.Add("ERLY_DETT_GR1_CODE", typeof(string));
                dtRqst.Columns.Add("ERLY_DETT_ITEM_ID", typeof(string));
                dtRqst.Columns.Add("ERLY_DETT_STAT_CODE", typeof(string));
                dtRqst.Columns.Add("USE_FLAG", typeof(string));
                dtRqst.Columns.Add("FROM_ERLY_DETT_DTTM", typeof(string));
                dtRqst.Columns.Add("TO_ERLY_DETT_DTTM", typeof(string));
                DataRow dr = dtRqst.NewRow();

                dr["LANGID"] = LoginInfo.LANGID;
                dr["SHOPID"] = null;
                dr["AREAID"] = null;
                dr["ERLY_DETT_GR1_CODE"] = cboGroup_history.SelectedValue.ToString() == string.Empty ? null : cboGroup_history.SelectedValue.ToString();
                dr["ERLY_DETT_ITEM_ID"] = cboItem_history.SelectedValue.ToString() == string.Empty ? null : cboItem_history.SelectedValue.ToString();
                dr["ERLY_DETT_STAT_CODE"] = cboStat_history.SelectedValue.ToString() == string.Empty ? null : cboStat_history.SelectedValue.ToString();
                dr["USE_FLAG"] = cboUseYN.SelectedValue.ToString() == string.Empty ? null : cboUseYN.SelectedValue.ToString();
                dr["FROM_ERLY_DETT_DTTM"] = Util.GetCondition(dtpDateFrom);
                dr["TO_ERLY_DETT_DTTM"] = Util.GetCondition(dtpDateTo);
                dtRqst.Rows.Add(dr);

                new ClientProxy().ExecuteService("DA_SEL_TB_SOM_ERLY_DETT_EXE_HIST", "RQSTDT", "RSLTDT", dtRqst, (bizResult, bizException) =>
                {
                    HiddenLoadingIndicator();
                    if (bizException != null)
                    {
                        Util.MessageException(bizException);
                        return;
                    }

                  
                    Util.GridSetData(dgListHist, bizResult, null, true);
                });

              
            }
            catch (Exception ex)
            {
                HiddenLoadingIndicator();
                Util.MessageException(ex);
            }
        }

        #endregion

        #endregion

        #region 공통 
        
        #region LoadingIndicator  :ShowLoadingIndicator(), HiddenLoadingIndicator()

        private void ShowLoadingIndicator()
        {
            if (loadingIndicator != null)
                loadingIndicator.Visibility = Visibility.Visible;
        }
        private void HiddenLoadingIndicator()
        {
            if (loadingIndicator != null)
                loadingIndicator.Visibility = Visibility.Collapsed;
        }
        #endregion LoadingIndicator
       
        #endregion 공통
        
        #endregion Functions
    }
}