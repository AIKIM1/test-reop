/*************************************************************************************
 Created Date : 2021.01.11
      Creator : 
   Decription : OCV Alarm View
--------------------------------------------------------------------------------------
 [Change History]
  2021.01.11  DEVELOPER : 
  2021.04.13  KDH : 화면간 이동 시 초기화 현상 제거
  2022.03.17  KDH : 설비리스트 조건 파라미터 변경
**************************************************************************************/
#define SAMPLE_DEV
//#undef SAMPLE_DEV

using C1.WPF;
using C1.WPF.DataGrid;
using C1.WPF.DataGrid.Summaries;
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
using LGC.GMES.MES.CMM001;

namespace LGC.GMES.MES.FCS002
{
    public partial class FCS002_009 : UserControl, IWorkArea
    {
        #region Declaration & Constructor 

        private string _actYN = "N";

        public string ACTYN {
            get {
                return _actYN;
            }
            set {
                _actYN = value;
            }
        }

        Util _Util = new Util();

        public FCS002_009()
        {
            InitializeComponent();
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

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            SetGridCboItem_EqpID(dgOCVAlarm.Columns["EQPID"], "8", null);

            if (_actYN.Equals("Y"))
            {
                GetList();
            }

            this.Loaded -= UserControl_Loaded; //2021.04.13 화면간 이동 시 초기화 현상 제거
        }
        #endregion

        #region Event
        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            GetList();
        }

        private void btnExcel_Click(object sender, RoutedEventArgs e)
        {
            if (_Util.GetDataGridCheckCnt(dgOCVAlarm, "CHK") <= 0)
            {
                Util.MessageValidation("FM_ME_0165");  //선택된 데이터가 없습니다.
            }
            else
            {
                DataTable dtRequestData = new DataTable();

                dtRequestData.TableName = "RQSTDT";
                dtRequestData.Columns.Add("EQPTID", typeof(string));
                dtRequestData.Columns.Add("ALARM_GRADE", typeof(string));

                for (int i = 0; i < dgOCVAlarm.GetRowCount(); i++)
                {
                    if (Util.NVC(DataTableConverter.GetValue(dgOCVAlarm.Rows[i].DataItem, "CHK")).Equals("True"))
                    {
                        DataRow dr = dtRequestData.NewRow();    
                        dr["EQPID"] = Util.NVC(DataTableConverter.GetValue(dgOCVAlarm.Rows[i].DataItem, "EQPID"));
                        dr["ALARM_GRADE"] = Util.NVC(DataTableConverter.GetValue(dgOCVAlarm.Rows[i].DataItem, "ALARM_GRADE"));
                        dtRequestData.Rows.Add(dr);
                    }
                }

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("BR_GET_OCV_ALARM_TRAY_LIST", "RQSTDT", "RSLTDT", dtRequestData);

                if (dtRslt.Rows.Count > 0)
                {
                    Util.GridSetData(dgOCVAlarmInsert, dtRslt, FrameOperation, true);
                    new LGC.GMES.MES.Common.ExcelExporter().Export(dgOCVAlarmInsert);
                }
                else
                {
                    Util.MessageValidation("FM_ME_0232");  //조회된 값이 없습니다.
                }
            }
        }

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
        #endregion

        #region Method
        private void GetList()
        {
            try
            {
                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_SEL_TC_OCV_ALARM_CAL", null, "RSLTDT", null);

                Util.GridSetData(dgOCVAlarm, dtRslt, FrameOperation, true);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private bool SetGridCboItem_EqpID(C1.WPF.DataGrid.DataGridColumn col, string sEqpKindCd, string sEqpIdLast, bool bCodeDisplay = false)
        {
            try
            {
                bool rtn = false;

                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                //RQSTDT.Columns.Add("EQP_KIND_CD", typeof(string)); //20220317_설비리스트 조건 파라미터 변경
                //RQSTDT.Columns.Add("EQP_ID_LAST", typeof(string)); //20220317_설비리스트 조건 파라미터 변경
                RQSTDT.Columns.Add("S70", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                //20220317_설비리스트 조건 파라미터 변경 START
                //dr["EQP_KIND_CD"] = sEqpKindCd;
                //dr["EQP_ID_LAST"] = sEqpIdLast;
                dr["S70"] = sEqpKindCd;
                //20220317_설비리스트 조건 파라미터 변경 END
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_COMBO_EQP", "RQSTDT", "RSLTDT", RQSTDT);

                if (dtResult == null || dtResult.Rows.Count == 0)
                {
                    rtn = false;
                }
                else
                {
                    rtn = true;
                    (col as C1.WPF.DataGrid.DataGridComboBoxColumn).ItemsSource = DataTableConverter.Convert(dtResult);
                }

                return rtn;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        #endregion



        private void GetTestData(ref DataTable dt)
        {
            dt.TableName = "RQSTDT";
            //dt.Columns.Add("INSDTTM", typeof(string));

            #region ROW ADD
            DataRow row1 = dt.NewRow();
            row1["WRK_DATE"] = "01/01/2021 01:00:00";
            row1["WRK_DATE1"] = "20210101";
            row1["EQSGID"] = "X20";
            row1["MDLLOT_ID"] = "UAB";
            row1["SHFT_ID"] = "1";
            row1["WRKLOG_TYPE_CODE"] = "Q";
            row1["PROD_LOTID"] = "UABTL153";
            row1["LOT_TYPE"] = "직행";
            row1["INPUT_QTY"] = "6427";
            row1["GOOD_QTY"] = "6124";
            row1["DFCT_CODE"] = "106";
            row1["DFCT_QTY"] = "1";
            row1["LOT_COMMENT"] = "E51-A02";
            row1["INSUSER"] = "陶顺华";
            row1["LOT_ATTR"] = "양산";
            row1["EQP_NAME"] = "자동차 2호 EOL";
            dt.Rows.Add(row1);
            DataRow row2 = dt.NewRow();
            row2["WRK_DATE"] = "01/01/2021 01:00:00";
            row2["WRK_DATE1"] = "20210101";
            row2["EQSGID"] = "X20";
            row2["MDLLOT_ID"] = "UAB";
            row2["SHFT_ID"] = "1";
            row2["WRKLOG_TYPE_CODE"] = "Q";
            row2["PROD_LOTID"] = "UABTL153";
            row2["LOT_TYPE"] = "직행";
            row2["INPUT_QTY"] = "6427";
            row2["GOOD_QTY"] = "6124";
            row2["DFCT_CODE"] = "119";
            row2["DFCT_QTY"] = "300";
            row2["LOT_COMMENT"] = "E51-A02";
            row2["INSUSER"] = "陶顺华";
            row2["LOT_ATTR"] = "양산";
            row2["EQP_NAME"] = "자동차 2호 EOL";
            dt.Rows.Add(row2);
            DataRow row3 = dt.NewRow();
            row3["WRK_DATE"] = "01/01/2021 01:00:00";
            row3["WRK_DATE1"] = "20210101";
            row3["EQSGID"] = "X20";
            row3["MDLLOT_ID"] = "UAB";
            row3["SHFT_ID"] = "1";
            row3["WRKLOG_TYPE_CODE"] = "Q";
            row3["PROD_LOTID"] = "UABTL153";
            row3["LOT_TYPE"] = "직행";
            row3["INPUT_QTY"] = "6427";
            row3["GOOD_QTY"] = "6124";
            row3["DFCT_CODE"] = "207";
            row3["DFCT_QTY"] = "2";
            row3["LOT_COMMENT"] = "E51-A02";
            row3["INSUSER"] = "陶顺华";
            row3["LOT_ATTR"] = "양산";
            row3["EQP_NAME"] = "자동차 2호 EOL";
            dt.Rows.Add(row3);
            DataRow row4 = dt.NewRow();
            row4["WRK_DATE"] = "01/01/2021 01:00:00";
            row4["WRK_DATE1"] = "20210101";
            row4["EQSGID"] = "X20";
            row4["MDLLOT_ID"] = "UAB";
            row4["SHFT_ID"] = "2";
            row4["WRKLOG_TYPE_CODE"] = "Q";
            row4["PROD_LOTID"] = "UABTL153";
            row4["LOT_TYPE"] = "직행";
            row4["INPUT_QTY"] = "2034";
            row4["GOOD_QTY"] = "2033";
            row4["DFCT_CODE"] = "101";
            row4["DFCT_QTY"] = "1";
            row4["LOT_COMMENT"] = "E51-A02";
            row4["INSUSER"] = "冯凌霏";
            row4["LOT_ATTR"] = "양산";
            row4["EQP_NAME"] = "자동차 2호 EOL";
            dt.Rows.Add(row4);
            DataRow row5 = dt.NewRow();
            row5["WRK_DATE"] = "01/01/2021 01:00:00";
            row5["WRK_DATE1"] = "20210101";
            row5["EQSGID"] = "X20";
            row5["MDLLOT_ID"] = "UAB";
            row5["SHFT_ID"] = "2";
            row5["WRKLOG_TYPE_CODE"] = "Q";
            row5["PROD_LOTID"] = "UABTL163";
            row5["LOT_TYPE"] = "직행";
            row5["INPUT_QTY"] = "7556";
            row5["GOOD_QTY"] = "7546";
            row5["DFCT_CODE"] = "103";
            row5["DFCT_QTY"] = "3";
            row5["LOT_COMMENT"] = "E51-A02";
            row5["INSUSER"] = "冯凌霏";
            row5["LOT_ATTR"] = "양산";
            row5["EQP_NAME"] = "자동차 2호 EOL";
            dt.Rows.Add(row5);
            DataRow row6 = dt.NewRow();
            row6["WRK_DATE"] = "01/01/2021 01:00:00";
            row6["WRK_DATE1"] = "20210101";
            row6["EQSGID"] = "X20";
            row6["MDLLOT_ID"] = "UAB";
            row6["SHFT_ID"] = "2";
            row6["WRKLOG_TYPE_CODE"] = "Q";
            row6["PROD_LOTID"] = "UABTL163";
            row6["LOT_TYPE"] = "직행";
            row6["INPUT_QTY"] = "7556";
            row6["GOOD_QTY"] = "7546";
            row6["DFCT_CODE"] = "105";
            row6["DFCT_QTY"] = "1";
            row6["LOT_COMMENT"] = "E51-A02";
            row6["INSUSER"] = "冯凌霏";
            row6["LOT_ATTR"] = "양산";
            row6["EQP_NAME"] = "자동차 2호 EOL";
            dt.Rows.Add(row6);
            DataRow row7 = dt.NewRow();
            row7["WRK_DATE"] = "01/01/2021 01:00:00";
            row7["WRK_DATE1"] = "20210101";
            row7["EQSGID"] = "X20";
            row7["MDLLOT_ID"] = "UAB";
            row7["SHFT_ID"] = "2";
            row7["WRKLOG_TYPE_CODE"] = "Q";
            row7["PROD_LOTID"] = "UABTL163";
            row7["LOT_TYPE"] = "직행";
            row7["INPUT_QTY"] = "7556";
            row7["GOOD_QTY"] = "7546";
            row7["DFCT_CODE"] = "210";
            row7["DFCT_QTY"] = "1";
            row7["LOT_COMMENT"] = "E51-A02";
            row7["INSUSER"] = "冯凌霏";
            row7["LOT_ATTR"] = "양산";
            row7["EQP_NAME"] = "자동차 2호 EOL";
            dt.Rows.Add(row7);
            DataRow row8 = dt.NewRow();
            row8["WRK_DATE"] = "01/01/2021 01:00:00";
            row8["WRK_DATE1"] = "20210101";
            row8["EQSGID"] = "X20";
            row8["MDLLOT_ID"] = "UAB";
            row8["SHFT_ID"] = "2";
            row8["WRKLOG_TYPE_CODE"] = "Q";
            row8["PROD_LOTID"] = "UABTL163";
            row8["LOT_TYPE"] = "직행";
            row8["INPUT_QTY"] = "7556";
            row8["GOOD_QTY"] = "7546";
            row8["DFCT_CODE"] = "408";
            row8["DFCT_QTY"] = "5";
            row8["LOT_COMMENT"] = "E51-A02";
            row8["INSUSER"] = "冯凌霏";
            row8["LOT_ATTR"] = "양산";
            row8["EQP_NAME"] = "자동차 2호 EOL";
            dt.Rows.Add(row8);
            DataRow row9 = dt.NewRow();
            row9["WRK_DATE"] = "01/02/2021 01:00:00";
            row9["WRK_DATE1"] = "20210102";
            row9["EQSGID"] = "X20";
            row9["MDLLOT_ID"] = "UAB";
            row9["SHFT_ID"] = "1";
            row9["WRKLOG_TYPE_CODE"] = "Q";
            row9["PROD_LOTID"] = "UABTL163";
            row9["LOT_TYPE"] = "직행";
            row9["INPUT_QTY"] = "5061";
            row9["GOOD_QTY"] = "4758";
            row9["DFCT_CODE"] = "103";
            row9["DFCT_QTY"] = "1";
            row9["LOT_COMMENT"] = "E51-A02";
            row9["INSUSER"] = "冯凌霏";
            row9["LOT_ATTR"] = "양산";
            row9["EQP_NAME"] = "자동차 2호 EOL";
            dt.Rows.Add(row9);
            DataRow row10 = dt.NewRow();
            row10["WRK_DATE"] = "01/02/2021 01:00:00";
            row10["WRK_DATE1"] = "20210102";
            row10["EQSGID"] = "X20";
            row10["MDLLOT_ID"] = "UAB";
            row10["SHFT_ID"] = "1";
            row10["WRKLOG_TYPE_CODE"] = "Q";
            row10["PROD_LOTID"] = "UABTL163";
            row10["LOT_TYPE"] = "직행";
            row10["INPUT_QTY"] = "5061";
            row10["GOOD_QTY"] = "4758";
            row10["DFCT_CODE"] = "119";
            row10["DFCT_QTY"] = "300";
            row10["LOT_COMMENT"] = "E51-A02";
            row10["INSUSER"] = "冯凌霏";
            row10["LOT_ATTR"] = "양산";
            row10["EQP_NAME"] = "자동차 2호 EOL";
            dt.Rows.Add(row10);
            DataRow row11 = dt.NewRow();
            row11["WRK_DATE"] = "01/02/2021 01:00:00";
            row11["WRK_DATE1"] = "20210102";
            row11["EQSGID"] = "X20";
            row11["MDLLOT_ID"] = "UAB";
            row11["SHFT_ID"] = "1";
            row11["WRKLOG_TYPE_CODE"] = "Q";
            row11["PROD_LOTID"] = "UABTL163";
            row11["LOT_TYPE"] = "직행";
            row11["INPUT_QTY"] = "5061";
            row11["GOOD_QTY"] = "4758";
            row11["DFCT_CODE"] = "210";
            row11["DFCT_QTY"] = "2";
            row11["LOT_COMMENT"] = "E51-A02";
            row11["INSUSER"] = "冯凌霏";
            row11["LOT_ATTR"] = "양산";
            row11["EQP_NAME"] = "자동차 2호 EOL";
            dt.Rows.Add(row11);
            DataRow row12 = dt.NewRow();
            row12["WRK_DATE"] = "01/02/2021 01:00:00";
            row12["WRK_DATE1"] = "20210102";
            row12["EQSGID"] = "X20";
            row12["MDLLOT_ID"] = "UAB";
            row12["SHFT_ID"] = "1";
            row12["WRKLOG_TYPE_CODE"] = "Q";
            row12["PROD_LOTID"] = "UABTL173";
            row12["LOT_TYPE"] = "직행";
            row12["INPUT_QTY"] = "3539";
            row12["GOOD_QTY"] = "3528";
            row12["DFCT_CODE"] = "101";
            row12["DFCT_QTY"] = "1";
            row12["LOT_COMMENT"] = "E51-A02";
            row12["INSUSER"] = "冯凌霏";
            row12["LOT_ATTR"] = "양산";
            row12["EQP_NAME"] = "자동차 2호 EOL";
            dt.Rows.Add(row12);
            DataRow row13 = dt.NewRow();
            row13["WRK_DATE"] = "01/02/2021 01:00:00";
            row13["WRK_DATE1"] = "20210102";
            row13["EQSGID"] = "X20";
            row13["MDLLOT_ID"] = "UAB";
            row13["SHFT_ID"] = "1";
            row13["WRKLOG_TYPE_CODE"] = "Q";
            row13["PROD_LOTID"] = "UABTL173";
            row13["LOT_TYPE"] = "직행";
            row13["INPUT_QTY"] = "3539";
            row13["GOOD_QTY"] = "3528";
            row13["DFCT_CODE"] = "107";
            row13["DFCT_QTY"] = "2";
            row13["LOT_COMMENT"] = "E51-A02";
            row13["INSUSER"] = "冯凌霏";
            row13["LOT_ATTR"] = "양산";
            row13["EQP_NAME"] = "자동차 2호 EOL";
            dt.Rows.Add(row13);
            DataRow row14 = dt.NewRow();
            row14["WRK_DATE"] = "01/02/2021 01:00:00";
            row14["WRK_DATE1"] = "20210102";
            row14["EQSGID"] = "X20";
            row14["MDLLOT_ID"] = "UAB";
            row14["SHFT_ID"] = "1";
            row14["WRKLOG_TYPE_CODE"] = "Q";
            row14["PROD_LOTID"] = "UABTL173";
            row14["LOT_TYPE"] = "직행";
            row14["INPUT_QTY"] = "3539";
            row14["GOOD_QTY"] = "3528";
            row14["DFCT_CODE"] = "210";
            row14["DFCT_QTY"] = "2";
            row14["LOT_COMMENT"] = "E51-A02";
            row14["INSUSER"] = "冯凌霏";
            row14["LOT_ATTR"] = "양산";
            row14["EQP_NAME"] = "자동차 2호 EOL";
            dt.Rows.Add(row14);
            DataRow row15 = dt.NewRow();
            row15["WRK_DATE"] = "01/02/2021 01:00:00";
            row15["WRK_DATE1"] = "20210102";
            row15["EQSGID"] = "X20";
            row15["MDLLOT_ID"] = "UAB";
            row15["SHFT_ID"] = "1";
            row15["WRKLOG_TYPE_CODE"] = "Q";
            row15["PROD_LOTID"] = "UABTL173";
            row15["LOT_TYPE"] = "직행";
            row15["INPUT_QTY"] = "3539";
            row15["GOOD_QTY"] = "3528";
            row15["DFCT_CODE"] = "408";
            row15["DFCT_QTY"] = "6";
            row15["LOT_COMMENT"] = "E51-A02";
            row15["INSUSER"] = "冯凌霏";
            row15["LOT_ATTR"] = "양산";
            row15["EQP_NAME"] = "자동차 2호 EOL";
            dt.Rows.Add(row15);
            DataRow row16 = dt.NewRow();
            row16["WRK_DATE"] = "01/02/2021 01:00:00";
            row16["WRK_DATE1"] = "20210102";
            row16["EQSGID"] = "X20";
            row16["MDLLOT_ID"] = "UAB";
            row16["SHFT_ID"] = "2";
            row16["WRKLOG_TYPE_CODE"] = "Q";
            row16["PROD_LOTID"] = "UABTL173";
            row16["LOT_TYPE"] = "직행";
            row16["INPUT_QTY"] = "9331";
            row16["GOOD_QTY"] = "9304";
            row16["DFCT_CODE"] = "101";
            row16["DFCT_QTY"] = "2";
            row16["LOT_COMMENT"] = "E51-A02";
            row16["INSUSER"] = "冯凌霏";
            row16["LOT_ATTR"] = "양산";
            row16["EQP_NAME"] = "자동차 2호 EOL";
            dt.Rows.Add(row16);
            DataRow row17 = dt.NewRow();
            row17["WRK_DATE"] = "01/02/2021 01:00:00";
            row17["WRK_DATE1"] = "20210102";
            row17["EQSGID"] = "X20";
            row17["MDLLOT_ID"] = "UAB";
            row17["SHFT_ID"] = "2";
            row17["WRKLOG_TYPE_CODE"] = "Q";
            row17["PROD_LOTID"] = "UABTL173";
            row17["LOT_TYPE"] = "직행";
            row17["INPUT_QTY"] = "9331";
            row17["GOOD_QTY"] = "9304";
            row17["DFCT_CODE"] = "103";
            row17["DFCT_QTY"] = "11";
            row17["LOT_COMMENT"] = "E51-A02";
            row17["INSUSER"] = "冯凌霏";
            row17["LOT_ATTR"] = "양산";
            row17["EQP_NAME"] = "자동차 2호 EOL";
            dt.Rows.Add(row17);
            DataRow row18 = dt.NewRow();
            row18["WRK_DATE"] = "01/02/2021 01:00:00";
            row18["WRK_DATE1"] = "20210102";
            row18["EQSGID"] = "X20";
            row18["MDLLOT_ID"] = "UAB";
            row18["SHFT_ID"] = "2";
            row18["WRKLOG_TYPE_CODE"] = "Q";
            row18["PROD_LOTID"] = "UABTL173";
            row18["LOT_TYPE"] = "직행";
            row18["INPUT_QTY"] = "9331";
            row18["GOOD_QTY"] = "9304";
            row18["DFCT_CODE"] = "105";
            row18["DFCT_QTY"] = "5";
            row18["LOT_COMMENT"] = "E51-A02";
            row18["INSUSER"] = "冯凌霏";
            row18["LOT_ATTR"] = "양산";
            row18["EQP_NAME"] = "자동차 2호 EOL";
            dt.Rows.Add(row18);
            DataRow row19 = dt.NewRow();
            row19["WRK_DATE"] = "01/02/2021 01:00:00";
            row19["WRK_DATE1"] = "20210102";
            row19["EQSGID"] = "X20";
            row19["MDLLOT_ID"] = "UAB";
            row19["SHFT_ID"] = "2";
            row19["WRKLOG_TYPE_CODE"] = "Q";
            row19["PROD_LOTID"] = "UABTL173";
            row19["LOT_TYPE"] = "직행";
            row19["INPUT_QTY"] = "9331";
            row19["GOOD_QTY"] = "9304";
            row19["DFCT_CODE"] = "106";
            row19["DFCT_QTY"] = "1";
            row19["LOT_COMMENT"] = "E51-A02";
            row19["INSUSER"] = "冯凌霏";
            row19["LOT_ATTR"] = "양산";
            row19["EQP_NAME"] = "자동차 2호 EOL";
            dt.Rows.Add(row19);
            DataRow row20 = dt.NewRow();
            row20["WRK_DATE"] = "01/02/2021 01:00:00";
            row20["WRK_DATE1"] = "20210102";
            row20["EQSGID"] = "X20";
            row20["MDLLOT_ID"] = "UAB";
            row20["SHFT_ID"] = "2";
            row20["WRKLOG_TYPE_CODE"] = "Q";
            row20["PROD_LOTID"] = "UABTL173";
            row20["LOT_TYPE"] = "직행";
            row20["INPUT_QTY"] = "9331";
            row20["GOOD_QTY"] = "9304";
            row20["DFCT_CODE"] = "107";
            row20["DFCT_QTY"] = "3";
            row20["LOT_COMMENT"] = "E51-A02";
            row20["INSUSER"] = "冯凌霏";
            row20["LOT_ATTR"] = "양산";
            row20["EQP_NAME"] = "자동차 2호 EOL";
            dt.Rows.Add(row20);
            DataRow row21 = dt.NewRow();
            row21["WRK_DATE"] = "01/02/2021 01:00:00";
            row21["WRK_DATE1"] = "20210102";
            row21["EQSGID"] = "X20";
            row21["MDLLOT_ID"] = "UAB";
            row21["SHFT_ID"] = "2";
            row21["WRKLOG_TYPE_CODE"] = "Q";
            row21["PROD_LOTID"] = "UABTL173";
            row21["LOT_TYPE"] = "직행";
            row21["INPUT_QTY"] = "9331";
            row21["GOOD_QTY"] = "9304";
            row21["DFCT_CODE"] = "108";
            row21["DFCT_QTY"] = "1";
            row21["LOT_COMMENT"] = "E51-A02";
            row21["INSUSER"] = "冯凌霏";
            row21["LOT_ATTR"] = "양산";
            row21["EQP_NAME"] = "자동차 2호 EOL";
            dt.Rows.Add(row21);
            DataRow row22 = dt.NewRow();
            row22["WRK_DATE"] = "01/02/2021 01:00:00";
            row22["WRK_DATE1"] = "20210102";
            row22["EQSGID"] = "X20";
            row22["MDLLOT_ID"] = "UAB";
            row22["SHFT_ID"] = "2";
            row22["WRKLOG_TYPE_CODE"] = "Q";
            row22["PROD_LOTID"] = "UABTL173";
            row22["LOT_TYPE"] = "직행";
            row22["INPUT_QTY"] = "9331";
            row22["GOOD_QTY"] = "9304";
            row22["DFCT_CODE"] = "210";
            row22["DFCT_QTY"] = "4";
            row22["LOT_COMMENT"] = "E51-A02";
            row22["INSUSER"] = "冯凌霏";
            row22["LOT_ATTR"] = "양산";
            row22["EQP_NAME"] = "자동차 2호 EOL";
            dt.Rows.Add(row22);
            DataRow row23 = dt.NewRow();
            row23["WRK_DATE"] = "01/03/2021 01:00:00";
            row23["WRK_DATE1"] = "20210103";
            row23["EQSGID"] = "X20";
            row23["MDLLOT_ID"] = "UAB";
            row23["SHFT_ID"] = "1";
            row23["WRKLOG_TYPE_CODE"] = "Q";
            row23["PROD_LOTID"] = "UABTL173";
            row23["LOT_TYPE"] = "직행";
            row23["INPUT_QTY"] = "1198";
            row23["GOOD_QTY"] = "1196";
            row23["DFCT_CODE"] = "103";
            row23["DFCT_QTY"] = "2";
            row23["LOT_COMMENT"] = "E51-A02";
            row23["INSUSER"] = "冯凌霏";
            row23["LOT_ATTR"] = "양산";
            row23["EQP_NAME"] = "자동차 2호 EOL";
            dt.Rows.Add(row23);
            DataRow row24 = dt.NewRow();
            row24["WRK_DATE"] = "01/03/2021 01:00:00";
            row24["WRK_DATE1"] = "20210103";
            row24["EQSGID"] = "X20";
            row24["MDLLOT_ID"] = "UAB";
            row24["SHFT_ID"] = "1";
            row24["WRKLOG_TYPE_CODE"] = "Q";
            row24["PROD_LOTID"] = "UABTL183";
            row24["LOT_TYPE"] = "직행";
            row24["INPUT_QTY"] = "7232";
            row24["GOOD_QTY"] = "6919";
            row24["DFCT_CODE"] = "101";
            row24["DFCT_QTY"] = "1";
            row24["LOT_COMMENT"] = "E51-A02";
            row24["INSUSER"] = "冯凌霏";
            row24["LOT_ATTR"] = "양산";
            row24["EQP_NAME"] = "자동차 2호 EOL";
            dt.Rows.Add(row24);
            DataRow row25 = dt.NewRow();
            row25["WRK_DATE"] = "01/03/2021 01:00:00";
            row25["WRK_DATE1"] = "20210103";
            row25["EQSGID"] = "X20";
            row25["MDLLOT_ID"] = "UAB";
            row25["SHFT_ID"] = "1";
            row25["WRKLOG_TYPE_CODE"] = "Q";
            row25["PROD_LOTID"] = "UABTL183";
            row25["LOT_TYPE"] = "직행";
            row25["INPUT_QTY"] = "7232";
            row25["GOOD_QTY"] = "6919";
            row25["DFCT_CODE"] = "106";
            row25["DFCT_QTY"] = "1";
            row25["LOT_COMMENT"] = "E51-A02";
            row25["INSUSER"] = "冯凌霏";
            row25["LOT_ATTR"] = "양산";
            row25["EQP_NAME"] = "자동차 2호 EOL";
            dt.Rows.Add(row25);
            DataRow row26 = dt.NewRow();
            row26["WRK_DATE"] = "01/03/2021 01:00:00";
            row26["WRK_DATE1"] = "20210103";
            row26["EQSGID"] = "X20";
            row26["MDLLOT_ID"] = "UAB";
            row26["SHFT_ID"] = "1";
            row26["WRKLOG_TYPE_CODE"] = "Q";
            row26["PROD_LOTID"] = "UABTL183";
            row26["LOT_TYPE"] = "직행";
            row26["INPUT_QTY"] = "7232";
            row26["GOOD_QTY"] = "6919";
            row26["DFCT_CODE"] = "108";
            row26["DFCT_QTY"] = "3";
            row26["LOT_COMMENT"] = "E51-A02";
            row26["INSUSER"] = "冯凌霏";
            row26["LOT_ATTR"] = "양산";
            row26["EQP_NAME"] = "자동차 2호 EOL";
            dt.Rows.Add(row26);
            DataRow row27 = dt.NewRow();
            row27["WRK_DATE"] = "01/03/2021 01:00:00";
            row27["WRK_DATE1"] = "20210103";
            row27["EQSGID"] = "X20";
            row27["MDLLOT_ID"] = "UAB";
            row27["SHFT_ID"] = "1";
            row27["WRKLOG_TYPE_CODE"] = "Q";
            row27["PROD_LOTID"] = "UABTL183";
            row27["LOT_TYPE"] = "직행";
            row27["INPUT_QTY"] = "7232";
            row27["GOOD_QTY"] = "6919";
            row27["DFCT_CODE"] = "119";
            row27["DFCT_QTY"] = "300";
            row27["LOT_COMMENT"] = "E51-A02";
            row27["INSUSER"] = "冯凌霏";
            row27["LOT_ATTR"] = "양산";
            row27["EQP_NAME"] = "자동차 2호 EOL";
            dt.Rows.Add(row27);
            DataRow row28 = dt.NewRow();
            row28["WRK_DATE"] = "01/03/2021 01:00:00";
            row28["WRK_DATE1"] = "20210103";
            row28["EQSGID"] = "X20";
            row28["MDLLOT_ID"] = "UAB";
            row28["SHFT_ID"] = "1";
            row28["WRKLOG_TYPE_CODE"] = "Q";
            row28["PROD_LOTID"] = "UABTL183";
            row28["LOT_TYPE"] = "직행";
            row28["INPUT_QTY"] = "7232";
            row28["GOOD_QTY"] = "6919";
            row28["DFCT_CODE"] = "207";
            row28["DFCT_QTY"] = "1";
            row28["LOT_COMMENT"] = "E51-A02";
            row28["INSUSER"] = "冯凌霏";
            row28["LOT_ATTR"] = "양산";
            row28["EQP_NAME"] = "자동차 2호 EOL";
            dt.Rows.Add(row28);
            DataRow row29 = dt.NewRow();
            row29["WRK_DATE"] = "01/03/2021 01:00:00";
            row29["WRK_DATE1"] = "20210103";
            row29["EQSGID"] = "X20";
            row29["MDLLOT_ID"] = "UAB";
            row29["SHFT_ID"] = "1";
            row29["WRKLOG_TYPE_CODE"] = "Q";
            row29["PROD_LOTID"] = "UABTL183";
            row29["LOT_TYPE"] = "직행";
            row29["INPUT_QTY"] = "7232";
            row29["GOOD_QTY"] = "6919";
            row29["DFCT_CODE"] = "210";
            row29["DFCT_QTY"] = "2";
            row29["LOT_COMMENT"] = "E51-A02";
            row29["INSUSER"] = "冯凌霏";
            row29["LOT_ATTR"] = "양산";
            row29["EQP_NAME"] = "자동차 2호 EOL";
            dt.Rows.Add(row29);
            DataRow row30 = dt.NewRow();
            row30["WRK_DATE"] = "01/03/2021 01:00:00";
            row30["WRK_DATE1"] = "20210103";
            row30["EQSGID"] = "X20";
            row30["MDLLOT_ID"] = "UAB";
            row30["SHFT_ID"] = "1";
            row30["WRKLOG_TYPE_CODE"] = "Q";
            row30["PROD_LOTID"] = "UABTL183";
            row30["LOT_TYPE"] = "직행";
            row30["INPUT_QTY"] = "7232";
            row30["GOOD_QTY"] = "6919";
            row30["DFCT_CODE"] = "408";
            row30["DFCT_QTY"] = "5";
            row30["LOT_COMMENT"] = "E51-A02";
            row30["INSUSER"] = "冯凌霏";
            row30["LOT_ATTR"] = "양산";
            row30["EQP_NAME"] = "자동차 2호 EOL";
            dt.Rows.Add(row30);
            DataRow row31 = dt.NewRow();
            row31["WRK_DATE"] = "01/03/2021 01:00:00";
            row31["WRK_DATE1"] = "20210103";
            row31["EQSGID"] = "X20";
            row31["MDLLOT_ID"] = "UAB";
            row31["SHFT_ID"] = "2";
            row31["WRKLOG_TYPE_CODE"] = "Q";
            row31["PROD_LOTID"] = "UABTL183";
            row31["LOT_TYPE"] = "직행";
            row31["INPUT_QTY"] = "6469";
            row31["GOOD_QTY"] = "6465";
            row31["DFCT_CODE"] = "103";
            row31["DFCT_QTY"] = "1";
            row31["LOT_COMMENT"] = "E51-A02";
            row31["INSUSER"] = "冯凌霏";
            row31["LOT_ATTR"] = "양산";
            row31["EQP_NAME"] = "자동차 2호 EOL";
            dt.Rows.Add(row31);
            DataRow row32 = dt.NewRow();
            row32["WRK_DATE"] = "01/03/2021 01:00:00";
            row32["WRK_DATE1"] = "20210103";
            row32["EQSGID"] = "X20";
            row32["MDLLOT_ID"] = "UAB";
            row32["SHFT_ID"] = "2";
            row32["WRKLOG_TYPE_CODE"] = "Q";
            row32["PROD_LOTID"] = "UABTL183";
            row32["LOT_TYPE"] = "직행";
            row32["INPUT_QTY"] = "6469";
            row32["GOOD_QTY"] = "6465";
            row32["DFCT_CODE"] = "105";
            row32["DFCT_QTY"] = "1";
            row32["LOT_COMMENT"] = "E51-A02";
            row32["INSUSER"] = "冯凌霏";
            row32["LOT_ATTR"] = "양산";
            row32["EQP_NAME"] = "자동차 2호 EOL";
            dt.Rows.Add(row32);
            DataRow row33 = dt.NewRow();
            row33["WRK_DATE"] = "01/03/2021 01:00:00";
            row33["WRK_DATE1"] = "20210103";
            row33["EQSGID"] = "X20";
            row33["MDLLOT_ID"] = "UAB";
            row33["SHFT_ID"] = "2";
            row33["WRKLOG_TYPE_CODE"] = "Q";
            row33["PROD_LOTID"] = "UABTL183";
            row33["LOT_TYPE"] = "직행";
            row33["INPUT_QTY"] = "6469";
            row33["GOOD_QTY"] = "6465";
            row33["DFCT_CODE"] = "108";
            row33["DFCT_QTY"] = "2";
            row33["LOT_COMMENT"] = "E51-A02";
            row33["INSUSER"] = "冯凌霏";
            row33["LOT_ATTR"] = "양산";
            row33["EQP_NAME"] = "자동차 2호 EOL";
            dt.Rows.Add(row33);
            DataRow row34 = dt.NewRow();
            row34["WRK_DATE"] = "01/03/2021 01:00:00";
            row34["WRK_DATE1"] = "20210103";
            row34["EQSGID"] = "X20";
            row34["MDLLOT_ID"] = "UAB";
            row34["SHFT_ID"] = "2";
            row34["WRKLOG_TYPE_CODE"] = "Q";
            row34["PROD_LOTID"] = "UABTL193";
            row34["LOT_TYPE"] = "직행";
            row34["INPUT_QTY"] = "2680";
            row34["GOOD_QTY"] = "2665";
            row34["DFCT_CODE"] = "101";
            row34["DFCT_QTY"] = "2";
            row34["LOT_COMMENT"] = "E51-A02";
            row34["INSUSER"] = "冯凌霏";
            row34["LOT_ATTR"] = "양산";
            row34["EQP_NAME"] = "자동차 2호 EOL";
            dt.Rows.Add(row34);
            DataRow row35 = dt.NewRow();
            row35["WRK_DATE"] = "01/03/2021 01:00:00";
            row35["WRK_DATE1"] = "20210103";
            row35["EQSGID"] = "X20";
            row35["MDLLOT_ID"] = "UAB";
            row35["SHFT_ID"] = "2";
            row35["WRKLOG_TYPE_CODE"] = "Q";
            row35["PROD_LOTID"] = "UABTL193";
            row35["LOT_TYPE"] = "직행";
            row35["INPUT_QTY"] = "2680";
            row35["GOOD_QTY"] = "2665";
            row35["DFCT_CODE"] = "103";
            row35["DFCT_QTY"] = "2";
            row35["LOT_COMMENT"] = "E51-A02";
            row35["INSUSER"] = "冯凌霏";
            row35["LOT_ATTR"] = "양산";
            row35["EQP_NAME"] = "자동차 2호 EOL";
            dt.Rows.Add(row35);
            DataRow row36 = dt.NewRow();
            row36["WRK_DATE"] = "01/03/2021 01:00:00";
            row36["WRK_DATE1"] = "20210103";
            row36["EQSGID"] = "X20";
            row36["MDLLOT_ID"] = "UAB";
            row36["SHFT_ID"] = "2";
            row36["WRKLOG_TYPE_CODE"] = "Q";
            row36["PROD_LOTID"] = "UABTL193";
            row36["LOT_TYPE"] = "직행";
            row36["INPUT_QTY"] = "2680";
            row36["GOOD_QTY"] = "2665";
            row36["DFCT_CODE"] = "105";
            row36["DFCT_QTY"] = "2";
            row36["LOT_COMMENT"] = "E51-A02";
            row36["INSUSER"] = "冯凌霏";
            row36["LOT_ATTR"] = "양산";
            row36["EQP_NAME"] = "자동차 2호 EOL";
            dt.Rows.Add(row36);
            DataRow row37 = dt.NewRow();
            row37["WRK_DATE"] = "01/03/2021 01:00:00";
            row37["WRK_DATE1"] = "20210103";
            row37["EQSGID"] = "X20";
            row37["MDLLOT_ID"] = "UAB";
            row37["SHFT_ID"] = "2";
            row37["WRKLOG_TYPE_CODE"] = "Q";
            row37["PROD_LOTID"] = "UABTL193";
            row37["LOT_TYPE"] = "직행";
            row37["INPUT_QTY"] = "2680";
            row37["GOOD_QTY"] = "2665";
            row37["DFCT_CODE"] = "108";
            row37["DFCT_QTY"] = "3";
            row37["LOT_COMMENT"] = "E51-A02";
            row37["INSUSER"] = "冯凌霏";
            row37["LOT_ATTR"] = "양산";
            row37["EQP_NAME"] = "자동차 2호 EOL";
            dt.Rows.Add(row37);
            DataRow row38 = dt.NewRow();
            row38["WRK_DATE"] = "01/03/2021 01:00:00";
            row38["WRK_DATE1"] = "20210103";
            row38["EQSGID"] = "X20";
            row38["MDLLOT_ID"] = "UAB";
            row38["SHFT_ID"] = "2";
            row38["WRKLOG_TYPE_CODE"] = "Q";
            row38["PROD_LOTID"] = "UABTL193";
            row38["LOT_TYPE"] = "직행";
            row38["INPUT_QTY"] = "2680";
            row38["GOOD_QTY"] = "2665";
            row38["DFCT_CODE"] = "408";
            row38["DFCT_QTY"] = "6";
            row38["LOT_COMMENT"] = "E51-A02";
            row38["INSUSER"] = "冯凌霏";
            row38["LOT_ATTR"] = "양산";
            row38["EQP_NAME"] = "자동차 2호 EOL";
            dt.Rows.Add(row38);
            DataRow row39 = dt.NewRow();
            row39["WRK_DATE"] = "01/04/2021 01:00:00";
            row39["WRK_DATE1"] = "20210104";
            row39["EQSGID"] = "X20";
            row39["MDLLOT_ID"] = "UAB";
            row39["SHFT_ID"] = "1";
            row39["WRKLOG_TYPE_CODE"] = "Q";
            row39["PROD_LOTID"] = "UABTL193";
            row39["LOT_TYPE"] = "직행";
            row39["INPUT_QTY"] = "8007";
            row39["GOOD_QTY"] = "7804";
            row39["DFCT_CODE"] = "105";
            row39["DFCT_QTY"] = "1";
            row39["LOT_COMMENT"] = "E51-A02";
            row39["INSUSER"] = "冯凌霏";
            row39["LOT_ATTR"] = "양산";
            row39["EQP_NAME"] = "자동차 2호 EOL";
            dt.Rows.Add(row39);
            DataRow row40 = dt.NewRow();
            row40["WRK_DATE"] = "01/04/2021 01:00:00";
            row40["WRK_DATE1"] = "20210104";
            row40["EQSGID"] = "X20";
            row40["MDLLOT_ID"] = "UAB";
            row40["SHFT_ID"] = "1";
            row40["WRKLOG_TYPE_CODE"] = "Q";
            row40["PROD_LOTID"] = "UABTL193";
            row40["LOT_TYPE"] = "직행";
            row40["INPUT_QTY"] = "8007";
            row40["GOOD_QTY"] = "7804";
            row40["DFCT_CODE"] = "119";
            row40["DFCT_QTY"] = "200";
            row40["LOT_COMMENT"] = "E51-A02";
            row40["INSUSER"] = "冯凌霏";
            row40["LOT_ATTR"] = "양산";
            row40["EQP_NAME"] = "자동차 2호 EOL";
            dt.Rows.Add(row40);
            DataRow row41 = dt.NewRow();
            row41["WRK_DATE"] = "01/04/2021 01:00:00";
            row41["WRK_DATE1"] = "20210104";
            row41["EQSGID"] = "X20";
            row41["MDLLOT_ID"] = "UAB";
            row41["SHFT_ID"] = "1";
            row41["WRKLOG_TYPE_CODE"] = "Q";
            row41["PROD_LOTID"] = "UABTL193";
            row41["LOT_TYPE"] = "직행";
            row41["INPUT_QTY"] = "8007";
            row41["GOOD_QTY"] = "7804";
            row41["DFCT_CODE"] = "207";
            row41["DFCT_QTY"] = "2";
            row41["LOT_COMMENT"] = "E51-A02";
            row41["INSUSER"] = "冯凌霏";
            row41["LOT_ATTR"] = "양산";
            row41["EQP_NAME"] = "자동차 2호 EOL";
            dt.Rows.Add(row41);
            DataRow row42 = dt.NewRow();
            row42["WRK_DATE"] = "01/04/2021 01:00:00";
            row42["WRK_DATE1"] = "20210104";
            row42["EQSGID"] = "X20";
            row42["MDLLOT_ID"] = "UAB";
            row42["SHFT_ID"] = "2";
            row42["WRKLOG_TYPE_CODE"] = "Q";
            row42["PROD_LOTID"] = "UABTL193";
            row42["LOT_TYPE"] = "직행";
            row42["INPUT_QTY"] = "4494";
            row42["GOOD_QTY"] = "4493";
            row42["DFCT_CODE"] = "106";
            row42["DFCT_QTY"] = "1";
            row42["LOT_COMMENT"] = "E51-A02";
            row42["INSUSER"] = "shendanhua";
            row42["LOT_ATTR"] = "양산";
            row42["EQP_NAME"] = "자동차 2호 EOL";
            dt.Rows.Add(row42);
            DataRow row43 = dt.NewRow();
            row43["WRK_DATE"] = "01/05/2021 01:00:00";
            row43["WRK_DATE1"] = "20210105";
            row43["EQSGID"] = "X20";
            row43["MDLLOT_ID"] = "UAB";
            row43["SHFT_ID"] = "1";
            row43["WRKLOG_TYPE_CODE"] = "Q";
            row43["PROD_LOTID"] = "UABTL203";
            row43["LOT_TYPE"] = "직행";
            row43["INPUT_QTY"] = "2025";
            row43["GOOD_QTY"] = "1820";
            row43["DFCT_CODE"] = "101";
            row43["DFCT_QTY"] = "1";
            row43["LOT_COMMENT"] = "E51-A02";
            row43["INSUSER"] = "冯凌霏";
            row43["LOT_ATTR"] = "양산";
            row43["EQP_NAME"] = "자동차 2호 EOL";
            dt.Rows.Add(row43);
            DataRow row44 = dt.NewRow();
            row44["WRK_DATE"] = "01/05/2021 01:00:00";
            row44["WRK_DATE1"] = "20210105";
            row44["EQSGID"] = "X20";
            row44["MDLLOT_ID"] = "UAB";
            row44["SHFT_ID"] = "1";
            row44["WRKLOG_TYPE_CODE"] = "Q";
            row44["PROD_LOTID"] = "UABTL203";
            row44["LOT_TYPE"] = "직행";
            row44["INPUT_QTY"] = "2025";
            row44["GOOD_QTY"] = "1820";
            row44["DFCT_CODE"] = "108";
            row44["DFCT_QTY"] = "1";
            row44["LOT_COMMENT"] = "E51-A02";
            row44["INSUSER"] = "冯凌霏";
            row44["LOT_ATTR"] = "양산";
            row44["EQP_NAME"] = "자동차 2호 EOL";
            dt.Rows.Add(row44);
            DataRow row45 = dt.NewRow();
            row45["WRK_DATE"] = "01/05/2021 01:00:00";
            row45["WRK_DATE1"] = "20210105";
            row45["EQSGID"] = "X20";
            row45["MDLLOT_ID"] = "UAB";
            row45["SHFT_ID"] = "1";
            row45["WRKLOG_TYPE_CODE"] = "Q";
            row45["PROD_LOTID"] = "UABTL203";
            row45["LOT_TYPE"] = "직행";
            row45["INPUT_QTY"] = "2025";
            row45["GOOD_QTY"] = "1820";
            row45["DFCT_CODE"] = "119";
            row45["DFCT_QTY"] = "200";
            row45["LOT_COMMENT"] = "E51-A02";
            row45["INSUSER"] = "冯凌霏";
            row45["LOT_ATTR"] = "양산";
            row45["EQP_NAME"] = "자동차 2호 EOL";
            dt.Rows.Add(row45);
            DataRow row46 = dt.NewRow();
            row46["WRK_DATE"] = "01/05/2021 01:00:00";
            row46["WRK_DATE1"] = "20210105";
            row46["EQSGID"] = "X20";
            row46["MDLLOT_ID"] = "UAB";
            row46["SHFT_ID"] = "1";
            row46["WRKLOG_TYPE_CODE"] = "Q";
            row46["PROD_LOTID"] = "UABTL203";
            row46["LOT_TYPE"] = "직행";
            row46["INPUT_QTY"] = "2025";
            row46["GOOD_QTY"] = "1820";
            row46["DFCT_CODE"] = "207";
            row46["DFCT_QTY"] = "1";
            row46["LOT_COMMENT"] = "E51-A02";
            row46["INSUSER"] = "冯凌霏";
            row46["LOT_ATTR"] = "양산";
            row46["EQP_NAME"] = "자동차 2호 EOL";
            dt.Rows.Add(row46);
            DataRow row47 = dt.NewRow();
            row47["WRK_DATE"] = "01/05/2021 01:00:00";
            row47["WRK_DATE1"] = "20210105";
            row47["EQSGID"] = "X20";
            row47["MDLLOT_ID"] = "UAB";
            row47["SHFT_ID"] = "1";
            row47["WRKLOG_TYPE_CODE"] = "Q";
            row47["PROD_LOTID"] = "UABTL203";
            row47["LOT_TYPE"] = "직행";
            row47["INPUT_QTY"] = "2025";
            row47["GOOD_QTY"] = "1820";
            row47["DFCT_CODE"] = "210";
            row47["DFCT_QTY"] = "2";
            row47["LOT_COMMENT"] = "E51-A02";
            row47["INSUSER"] = "冯凌霏";
            row47["LOT_ATTR"] = "양산";
            row47["EQP_NAME"] = "자동차 2호 EOL";
            dt.Rows.Add(row47);
            DataRow row48 = dt.NewRow();
            row48["WRK_DATE"] = "01/05/2021 01:00:00";
            row48["WRK_DATE1"] = "20210105";
            row48["EQSGID"] = "X20";
            row48["MDLLOT_ID"] = "UAB";
            row48["SHFT_ID"] = "2";
            row48["WRKLOG_TYPE_CODE"] = "Q";
            row48["PROD_LOTID"] = "UABTL203";
            row48["LOT_TYPE"] = "직행";
            row48["INPUT_QTY"] = "10008";
            row48["GOOD_QTY"] = "9986";
            row48["DFCT_CODE"] = "101";
            row48["DFCT_QTY"] = "2";
            row48["LOT_COMMENT"] = "E51-A02";
            row48["INSUSER"] = "冯凌霏";
            row48["LOT_ATTR"] = "양산";
            row48["EQP_NAME"] = "자동차 2호 EOL";
            dt.Rows.Add(row48);
            DataRow row49 = dt.NewRow();
            row49["WRK_DATE"] = "01/05/2021 01:00:00";
            row49["WRK_DATE1"] = "20210105";
            row49["EQSGID"] = "X20";
            row49["MDLLOT_ID"] = "UAB";
            row49["SHFT_ID"] = "2";
            row49["WRKLOG_TYPE_CODE"] = "Q";
            row49["PROD_LOTID"] = "UABTL203";
            row49["LOT_TYPE"] = "직행";
            row49["INPUT_QTY"] = "10008";
            row49["GOOD_QTY"] = "9986";
            row49["DFCT_CODE"] = "103";
            row49["DFCT_QTY"] = "1";
            row49["LOT_COMMENT"] = "E51-A02";
            row49["INSUSER"] = "冯凌霏";
            row49["LOT_ATTR"] = "양산";
            row49["EQP_NAME"] = "자동차 2호 EOL";
            dt.Rows.Add(row49);
            DataRow row50 = dt.NewRow();
            row50["WRK_DATE"] = "01/05/2021 01:00:00";
            row50["WRK_DATE1"] = "20210105";
            row50["EQSGID"] = "X20";
            row50["MDLLOT_ID"] = "UAB";
            row50["SHFT_ID"] = "2";
            row50["WRKLOG_TYPE_CODE"] = "Q";
            row50["PROD_LOTID"] = "UABTL203";
            row50["LOT_TYPE"] = "직행";
            row50["INPUT_QTY"] = "10008";
            row50["GOOD_QTY"] = "9986";
            row50["DFCT_CODE"] = "106";
            row50["DFCT_QTY"] = "1";
            row50["LOT_COMMENT"] = "E51-A02";
            row50["INSUSER"] = "冯凌霏";
            row50["LOT_ATTR"] = "양산";
            row50["EQP_NAME"] = "자동차 2호 EOL";
            dt.Rows.Add(row50);
            DataRow row51 = dt.NewRow();
            row51["WRK_DATE"] = "01/05/2021 01:00:00";
            row51["WRK_DATE1"] = "20210105";
            row51["EQSGID"] = "X20";
            row51["MDLLOT_ID"] = "UAB";
            row51["SHFT_ID"] = "2";
            row51["WRKLOG_TYPE_CODE"] = "Q";
            row51["PROD_LOTID"] = "UABTL203";
            row51["LOT_TYPE"] = "직행";
            row51["INPUT_QTY"] = "10008";
            row51["GOOD_QTY"] = "9986";
            row51["DFCT_CODE"] = "116";
            row51["DFCT_QTY"] = "1";
            row51["LOT_COMMENT"] = "E51-A02";
            row51["INSUSER"] = "冯凌霏";
            row51["LOT_ATTR"] = "양산";
            row51["EQP_NAME"] = "자동차 2호 EOL";
            dt.Rows.Add(row51);
            DataRow row52 = dt.NewRow();
            row52["WRK_DATE"] = "01/05/2021 01:00:00";
            row52["WRK_DATE1"] = "20210105";
            row52["EQSGID"] = "X20";
            row52["MDLLOT_ID"] = "UAB";
            row52["SHFT_ID"] = "2";
            row52["WRKLOG_TYPE_CODE"] = "Q";
            row52["PROD_LOTID"] = "UABTL203";
            row52["LOT_TYPE"] = "직행";
            row52["INPUT_QTY"] = "10008";
            row52["GOOD_QTY"] = "9986";
            row52["DFCT_CODE"] = "207";
            row52["DFCT_QTY"] = "2";
            row52["LOT_COMMENT"] = "E51-A02";
            row52["INSUSER"] = "冯凌霏";
            row52["LOT_ATTR"] = "양산";
            row52["EQP_NAME"] = "자동차 2호 EOL";
            dt.Rows.Add(row52);
            DataRow row53 = dt.NewRow();
            row53["WRK_DATE"] = "01/05/2021 01:00:00";
            row53["WRK_DATE1"] = "20210105";
            row53["EQSGID"] = "X20";
            row53["MDLLOT_ID"] = "UAB";
            row53["SHFT_ID"] = "2";
            row53["WRKLOG_TYPE_CODE"] = "Q";
            row53["PROD_LOTID"] = "UABTL203";
            row53["LOT_TYPE"] = "직행";
            row53["INPUT_QTY"] = "10008";
            row53["GOOD_QTY"] = "9986";
            row53["DFCT_CODE"] = "210";
            row53["DFCT_QTY"] = "10";
            row53["LOT_COMMENT"] = "E51-A02";
            row53["INSUSER"] = "冯凌霏";
            row53["LOT_ATTR"] = "양산";
            row53["EQP_NAME"] = "자동차 2호 EOL";
            dt.Rows.Add(row53);
            DataRow row54 = dt.NewRow();
            row54["WRK_DATE"] = "01/05/2021 01:00:00";
            row54["WRK_DATE1"] = "20210105";
            row54["EQSGID"] = "X20";
            row54["MDLLOT_ID"] = "UAB";
            row54["SHFT_ID"] = "2";
            row54["WRKLOG_TYPE_CODE"] = "Q";
            row54["PROD_LOTID"] = "UABTL203";
            row54["LOT_TYPE"] = "직행";
            row54["INPUT_QTY"] = "10008";
            row54["GOOD_QTY"] = "9986";
            row54["DFCT_CODE"] = "408";
            row54["DFCT_QTY"] = "5";
            row54["LOT_COMMENT"] = "E51-A02";
            row54["INSUSER"] = "冯凌霏";
            row54["LOT_ATTR"] = "양산";
            row54["EQP_NAME"] = "자동차 2호 EOL";
            dt.Rows.Add(row54);
            DataRow row55 = dt.NewRow();
            row55["WRK_DATE"] = "01/06/2021 01:00:00";
            row55["WRK_DATE1"] = "20210106";
            row55["EQSGID"] = "X20";
            row55["MDLLOT_ID"] = "UAB";
            row55["SHFT_ID"] = "1";
            row55["WRKLOG_TYPE_CODE"] = "Q";
            row55["PROD_LOTID"] = "UABTK223";
            row55["LOT_TYPE"] = "직행";
            row55["INPUT_QTY"] = "1584";
            row55["GOOD_QTY"] = "1581";
            row55["DFCT_CODE"] = "102";
            row55["DFCT_QTY"] = "1";
            row55["LOT_COMMENT"] = "E51-A02";
            row55["INSUSER"] = "冯凌霏";
            row55["LOT_ATTR"] = "양산";
            row55["EQP_NAME"] = "자동차 2호 EOL";
            dt.Rows.Add(row55);
            DataRow row56 = dt.NewRow();
            row56["WRK_DATE"] = "01/06/2021 01:00:00";
            row56["WRK_DATE1"] = "20210106";
            row56["EQSGID"] = "X20";
            row56["MDLLOT_ID"] = "UAB";
            row56["SHFT_ID"] = "1";
            row56["WRKLOG_TYPE_CODE"] = "Q";
            row56["PROD_LOTID"] = "UABTK223";
            row56["LOT_TYPE"] = "직행";
            row56["INPUT_QTY"] = "1584";
            row56["GOOD_QTY"] = "1581";
            row56["DFCT_CODE"] = "207";
            row56["DFCT_QTY"] = "1";
            row56["LOT_COMMENT"] = "E51-A02";
            row56["INSUSER"] = "冯凌霏";
            row56["LOT_ATTR"] = "양산";
            row56["EQP_NAME"] = "자동차 2호 EOL";
            dt.Rows.Add(row56);
            DataRow row57 = dt.NewRow();
            row57["WRK_DATE"] = "01/06/2021 01:00:00";
            row57["WRK_DATE1"] = "20210106";
            row57["EQSGID"] = "X20";
            row57["MDLLOT_ID"] = "UAB";
            row57["SHFT_ID"] = "1";
            row57["WRKLOG_TYPE_CODE"] = "Q";
            row57["PROD_LOTID"] = "UABTK223";
            row57["LOT_TYPE"] = "직행";
            row57["INPUT_QTY"] = "1584";
            row57["GOOD_QTY"] = "1581";
            row57["DFCT_CODE"] = "311";
            row57["DFCT_QTY"] = "1";
            row57["LOT_COMMENT"] = "E51-A02";
            row57["INSUSER"] = "冯凌霏";
            row57["LOT_ATTR"] = "양산";
            row57["EQP_NAME"] = "자동차 2호 EOL";
            dt.Rows.Add(row57);
            DataRow row58 = dt.NewRow();
            row58["WRK_DATE"] = "01/06/2021 01:00:00";
            row58["WRK_DATE1"] = "20210106";
            row58["EQSGID"] = "X20";
            row58["MDLLOT_ID"] = "UAB";
            row58["SHFT_ID"] = "2";
            row58["WRKLOG_TYPE_CODE"] = "Q";
            row58["PROD_LOTID"] = "UABTK301";
            row58["LOT_TYPE"] = "직행";
            row58["INPUT_QTY"] = "885";
            row58["GOOD_QTY"] = "884";
            row58["DFCT_CODE"] = "105";
            row58["DFCT_QTY"] = "1";
            row58["LOT_COMMENT"] = "E51-A02";
            row58["INSUSER"] = "冯凌霏";
            row58["LOT_ATTR"] = "양산";
            row58["EQP_NAME"] = "자동차 2호 EOL";
            dt.Rows.Add(row58);
            DataRow row59 = dt.NewRow();
            row59["WRK_DATE"] = "01/06/2021 01:00:00";
            row59["WRK_DATE1"] = "20210106";
            row59["EQSGID"] = "X20";
            row59["MDLLOT_ID"] = "UAB";
            row59["SHFT_ID"] = "1";
            row59["WRKLOG_TYPE_CODE"] = "Q";
            row59["PROD_LOTID"] = "UABTL103";
            row59["LOT_TYPE"] = "직행";
            row59["INPUT_QTY"] = "1408";
            row59["GOOD_QTY"] = "1407";
            row59["DFCT_CODE"] = "105";
            row59["DFCT_QTY"] = "1";
            row59["LOT_COMMENT"] = "E51-A02";
            row59["INSUSER"] = "冯凌霏";
            row59["LOT_ATTR"] = "양산";
            row59["EQP_NAME"] = "자동차 2호 EOL";
            dt.Rows.Add(row59);
            DataRow row60 = dt.NewRow();
            row60["WRK_DATE"] = "01/06/2021 01:00:00";
            row60["WRK_DATE1"] = "20210106";
            row60["EQSGID"] = "X20";
            row60["MDLLOT_ID"] = "UAB";
            row60["SHFT_ID"] = "1";
            row60["WRKLOG_TYPE_CODE"] = "Q";
            row60["PROD_LOTID"] = "UABTL203";
            row60["LOT_TYPE"] = "직행";
            row60["INPUT_QTY"] = "4439";
            row60["GOOD_QTY"] = "4283";
            row60["DFCT_CODE"] = "103";
            row60["DFCT_QTY"] = "1";
            row60["LOT_COMMENT"] = "E51-A02";
            row60["INSUSER"] = "冯凌霏";
            row60["LOT_ATTR"] = "양산";
            row60["EQP_NAME"] = "자동차 2호 EOL";
            dt.Rows.Add(row60);
            DataRow row61 = dt.NewRow();
            row61["WRK_DATE"] = "01/06/2021 01:00:00";
            row61["WRK_DATE1"] = "20210106";
            row61["EQSGID"] = "X20";
            row61["MDLLOT_ID"] = "UAB";
            row61["SHFT_ID"] = "1";
            row61["WRKLOG_TYPE_CODE"] = "Q";
            row61["PROD_LOTID"] = "UABTL203";
            row61["LOT_TYPE"] = "직행";
            row61["INPUT_QTY"] = "4439";
            row61["GOOD_QTY"] = "4283";
            row61["DFCT_CODE"] = "105";
            row61["DFCT_QTY"] = "1";
            row61["LOT_COMMENT"] = "E51-A02";
            row61["INSUSER"] = "冯凌霏";
            row61["LOT_ATTR"] = "양산";
            row61["EQP_NAME"] = "자동차 2호 EOL";



            #endregion

        }

    }
}
