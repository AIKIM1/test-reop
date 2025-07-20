/*************************************************************************************
 Created Date : 2020.11.23
      Creator : 
   Decription : 충방전기 공정별 현황
--------------------------------------------------------------------------------------
 [Change History]
  2023.12.20  주훈        LGC.GMES.MES.FCS002.FCS002_003 초기 복사
  2024.01.03  주훈        조회 조건을 LINE에서 LANE으로 변경
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
using System.Windows.Media;
using System.Windows.Data;
using System.Linq;

namespace LGC.GMES.MES.FCS002
{
    public partial class FCS002_003_NFF : UserControl, IWorkArea
    {
        #region Declaration & Constructor 

        // 최대 Charge Display Count
        Int32 MAX_DISPLAY_CHARGE = 15;
        Int32 MAX_DISPLAY_DISCHARGE = 15;

        Util _Util = new Util();

        public FCS002_003_NFF()
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
        
      
        #endregion

        #region Event

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            // 2024.01.03 조회 조건을 LINE에서 LANE으로 변경
            //SetCboLineID();
            SetCboLane();

            // 2023.12.20 추가
            InitControl();
            InitSpread();

            this.Loaded -= UserControl_Loaded;
        }

        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            GetList();            
        }

        #endregion

        #region Method

        private void SetCboLineID()
        {
            // 2024.01.03 조회 조건을 LINE에서 LANE으로 변경
            //try
            //{
            //    DataTable dtRQSTDT = new DataTable();
            //    dtRQSTDT.TableName = "RQSTDT";
            //    dtRQSTDT.Columns.Add("LANGID", typeof(string));
            //    dtRQSTDT.Columns.Add("AREAID", typeof(string));

            //    DataRow drnewrow = dtRQSTDT.NewRow();
            //    drnewrow["LANGID"] = LoginInfo.LANGID;
            //    drnewrow["AREAID"] = LoginInfo.CFG_AREA_ID;
            //    dtRQSTDT.Rows.Add(drnewrow);

            //    DataTable result = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_COMBO_LINE_MB", "RQSTDT", "RSLTDT", dtRQSTDT);

            //    cboLineID.ItemsSource = DataTableConverter.Convert(result);
            //    cboLineID.CheckAll();
            //}
            //catch (Exception ex)
            //{

            //}
        }

        private void SetCboLane()
        {
            try
            {
                DataTable dtRQSTDT = new DataTable();
                dtRQSTDT.TableName = "RQSTDT";
                dtRQSTDT.Columns.Add("LANGID", typeof(string));
                dtRQSTDT.Columns.Add("ONLY_X", typeof(string));

                DataRow drnewrow = dtRQSTDT.NewRow();
                drnewrow["LANGID"] = LoginInfo.LANGID;
                drnewrow["ONLY_X"] = "Y";
                dtRQSTDT.Rows.Add(drnewrow);

                DataTable result = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_LANE_CBO", "RQSTDT", "RSLTDT", dtRQSTDT);

                cboLane.ItemsSource = DataTableConverter.Convert(result);
                cboLane.CheckAll();
            }
            catch (Exception ex)
            {

            }
        }

        private void InitControl()
        {
        }

        private void InitSpread()
        {
            Util.gridClear(dgFrmOpStatus); // Grid clear
            int Header_Row_count = 3;

            //칼럼 헤더 행 추가
            for (int i = 0; i < Header_Row_count; i++)
            {
                DataGridColumnHeaderRow HR = new DataGridColumnHeaderRow();
                dgFrmOpStatus.TopRows.Add(HR);
            }

            // FIX
            // 2024.01.03  조회 조건을 LINE에서 LANE으로 변경에 따른 수정
            //FixedMultiHeader("LINE|LINE|LINE", "LINE_NAME", iWidth: 100);
            FixedMultiHeader("LANE_ID|LANE_ID|LANE_ID", "LANE_NAME", iWidth: 100);
            FixedMultiHeader("모델ID|모델ID|모델ID", "MDLLOT_ID", iWidth: 100);
            FixedMultiHeader("경로유형|경로유형|경로유형", "ROUT_TYPE_NAME", iWidth: 100, oHorizonAlign: HorizontalAlignment.Left);
            FixedMultiHeader("공정경로|공정경로|공정경로", "ROUTID", iWidth: 100);

            // 2024.01.03  조회 조건을 LINE에서 LANE으로 변경에 따른 수정
            //string[] sColumnName = new string[] { "LINE_NAME", "LANE_NAME", "MDLLOT_ID", "ROUT_TYPE_NAME" };
            string[] sColumnName = new string[] { "LANE_NAME", "MDLLOT_ID", "ROUT_TYPE_NAME" };
            _Util.SetDataGridMergeExtensionCol(dgFrmOpStatus, sColumnName, DataGridMergeMode.VERTICALHIERARCHI);

            // N차 충전 HEADER
            for (int iDisp = 1; iDisp <= MAX_DISPLAY_CHARGE; iDisp++)
            {
                AddHeaderCharge(true, iDisp, false);
            }

            // N차 방전 HEADER
            for (int iDisp = 1; iDisp <= MAX_DISPLAY_DISCHARGE; iDisp++)
            {
                AddHeaderCharge(false, iDisp, false);
            }

            FixedMultiHeader("충방전 합계|충전|Box 수량", "CNT_CHARGE", oHorizonAlign: HorizontalAlignment.Right);
            FixedMultiHeader("충방전 합계|충전|점유율(%)", "RATE_CHARGE", oHorizonAlign: HorizontalAlignment.Right);
            FixedMultiHeader("충방전 합계|방전|Box 수량", "CNT_DISCHARGE", oHorizonAlign: HorizontalAlignment.Right);
            FixedMultiHeader("충방전 합계|방전|점유율(%)", "RATE_DISCHARGE", oHorizonAlign: HorizontalAlignment.Right);
            FixedMultiHeader("충방전 합계|ETC|Box 수량", "CNT_ETC", oHorizonAlign: HorizontalAlignment.Right);
            FixedMultiHeader("충방전 합계|ETC|점유율(%)", "RATE_ETC", oHorizonAlign: HorizontalAlignment.Right);
        }


        private void AddHeaderCharge(Boolean bCharge, int iChargeNo, bool bVisible = true)
        {
            String sChargeName = String.Empty;
            string sName = String.Empty;
            string sBindName = String.Empty;

            sChargeName = bCharge == true ? "CHARGE" : "DISCHARGE";

            sName = sChargeName + "|[*]" + iChargeNo.ToString() + ObjectDic.Instance.GetObjectName("차") + " " + ObjectDic.Instance.GetObjectName(sChargeName) + "|Box 수량";
            sBindName = "CNT_" + sChargeName + "_" + iChargeNo.ToString("00");
            FixedMultiHeader(sName, sBindName, bVisible: bVisible, oHorizonAlign: HorizontalAlignment.Right);

            sName = sChargeName + "|[*]" + iChargeNo.ToString() + ObjectDic.Instance.GetObjectName("차") + " " + ObjectDic.Instance.GetObjectName(sChargeName) + "|점유율(%)";
            sBindName = "RATE_" + sChargeName  + "_" + iChargeNo.ToString("00");
            FixedMultiHeader(sName, sBindName, bVisible: bVisible, oHorizonAlign: HorizontalAlignment.Right);
        }

        private void FixedMultiHeader(string sName, string sBindName
                                    , int iWidth = 75, bool bPercent = false, bool bVisible = true
                                    , HorizontalAlignment oHorizonAlign = HorizontalAlignment.Center
                                    , VerticalAlignment oVerticalAlign = VerticalAlignment.Center)
        {
            bool bReadOnly = true;
            bool bEditable = false;

            string[] sColNames = sName.Split('|');

            List<string> Multi_Header = new List<string>();
            Multi_Header = sColNames.ToList();

            var column_TEXT = CreateTextColumn(null, Multi_Header, sBindName, sBindName, iWidth
                                             , bPercent: bPercent
                                             , bReadOnly: bReadOnly
                                             , bEditable: bEditable
                                             , bVisible: bVisible
                                             , HorizonAlign: oHorizonAlign
                                             , VerticalAlign: oVerticalAlign);
            dgFrmOpStatus.Columns.Add(column_TEXT);
        }

        private C1.WPF.DataGrid.DataGridTextColumn CreateTextColumn(string Single_Header
                                                                         , List<string> Multi_Header
                                                                         , string sName
                                                                         , string sBinding
                                                                         , int iWidth
                                                                         , bool bPercent = false
                                                                         , bool bReadOnly = false
                                                                         , bool bEditable = true
                                                                         , bool bVisible = true
                                                                         , HorizontalAlignment HorizonAlign = HorizontalAlignment.Center
                                                                         , VerticalAlignment VerticalAlign = VerticalAlignment.Center
                                                        )
        {

            C1.WPF.DataGrid.DataGridTextColumn Col = new C1.WPF.DataGrid.DataGridTextColumn();

            Col.Name = sName;
            Col.Binding = new Binding(sBinding);
            Col.IsReadOnly = bReadOnly;
            Col.EditOnSelection = bEditable;
            Col.Visibility = bVisible.Equals(true) ? Visibility.Visible : Visibility.Collapsed;
            Col.HorizontalAlignment = HorizonAlign;
            Col.VerticalAlignment = VerticalAlign;

            if (iWidth == 0)
                Col.Width = new C1.WPF.DataGrid.DataGridLength(1, DataGridUnitType.Auto);
            else
                Col.Width = new C1.WPF.DataGrid.DataGridLength(iWidth, DataGridUnitType.Pixel);

            if (bPercent)
                Col.Format = "P2";

            if (!string.IsNullOrEmpty(Single_Header))
                Col.Header = Single_Header;
            else
                Col.Header = Multi_Header;

            return Col;
        }

        private void GetList()
        {
            try
            {
                btnSearch.IsEnabled = false;
                GridInit();

                DataTable dtRqst = new DataTable();
                dtRqst.TableName = "INDATA";
                dtRqst.Columns.Add("LANGID", typeof(string));
                dtRqst.Columns.Add("AREAID", typeof(string));

                // 2024.01.03 조회 조건을 LINE에서 LANE으로 변경
                //dtRqst.Columns.Add("LINE_ID", typeof(string));
                dtRqst.Columns.Add("LANE_ID", typeof(string));

                DataRow dr = dtRqst.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["AREAID"] = LoginInfo.CFG_AREA_ID;

                // 2024.01.03 조회 조건을 LINE에서 LANE으로 변경
                //dr["LINE_ID"] = Util.NVC(cboLineID.SelectedItemsToString).Equals("") ? string.Empty : Util.NVC(cboLineID.SelectedItemsToString);
                dr["LANE_ID"] = Util.NVC(cboLane.SelectedItemsToString).Equals("") ? string.Empty : Util.NVC(cboLane.SelectedItemsToString);
                dtRqst.Rows.Add(dr);

                ShowLoadingIndicator();
                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_SEL_FORMATION_RATE_TOTAL_MB_NFF", "RQSTDT", "RSLTDT", dtRqst);
                Util.GridSetData(dgFrmOpStatus, dtRslt, FrameOperation, true);

                Int32 iChargeNo = 0;
                Int32 iMaxChargeNo = 0;

                Int32 iDischargeNo = 0;
                Int32 iMaxDischargeNo = 0;

                foreach (DataRow oRow in dtRslt.Rows)
                {
                    iChargeNo = Convert.ToInt32(oRow["MAX_CHARGE_NO"].ToString());
                    if (iMaxChargeNo < iChargeNo) iMaxChargeNo = iChargeNo;

                    iDischargeNo = Convert.ToInt32(oRow["MAX_DISCHARGE_NO"].ToString());
                    if (iMaxDischargeNo < iDischargeNo) iMaxDischargeNo = iDischargeNo;
                }

                iChargeNo = 0;
                String sMaxDisplayChargName = "CNT_CHARGE_" + MAX_DISPLAY_CHARGE.ToString("00");
                for (Int32 i = dgFrmOpStatus.Columns["CNT_CHARGE_01"].Index; i <= dgFrmOpStatus.Columns[sMaxDisplayChargName].Index;)
                {
                    iChargeNo++;
                    if (iMaxChargeNo >= iChargeNo)
                    {
                        dgFrmOpStatus.Columns[i++].Visibility = Visibility.Visible;     // CHARGE_0
                        dgFrmOpStatus.Columns[i++].Visibility = Visibility.Visible;     // RATE_CHARGE_0
                    }
                    else
                    {
                        dgFrmOpStatus.Columns[i++].Visibility = Visibility.Collapsed;   // CHARGE_0
                        dgFrmOpStatus.Columns[i++].Visibility = Visibility.Collapsed;   // RATE_CHARGE_0
                    }
                }

                iDischargeNo = 0;
                String sMaxDisplayDioschargName = "CNT_DISCHARGE_" + MAX_DISPLAY_DISCHARGE.ToString("00");
                for (Int32 i = dgFrmOpStatus.Columns["CNT_DISCHARGE_01"].Index; i <= dgFrmOpStatus.Columns[sMaxDisplayDioschargName].Index;)
                {
                    iDischargeNo++;
                    if (iMaxDischargeNo >= iDischargeNo)
                    {
                        dgFrmOpStatus.Columns[i++].Visibility = Visibility.Visible;     // DISCHARGE_0
                        dgFrmOpStatus.Columns[i++].Visibility = Visibility.Visible;     // RATE_DISCHARGE_0
                    }
                    else
                    {
                        dgFrmOpStatus.Columns[i++].Visibility = Visibility.Collapsed;   // DISCHARGE_0
                        dgFrmOpStatus.Columns[i++].Visibility = Visibility.Collapsed;   // RATE_DISCHARGE_0
                    }
                }

                // 결과없는 컬럼 숨기기
                DataTable OperDt = DataTableConverter.Convert(dgFrmOpStatus.ItemsSource);

                List<String> ExceptColName = new List<string>
                {
                    "CNT_CHARGE", "CNT_DISCHARGE", "CNT_ETC", "RATE_CHARGE", "RATE_DISCHARGE"
                  , "RATE_ETC",  "MAX_CHARGE_NO", "MAX_DISCHARGE_NO"
                };

                List<String> ExceptGridColName = new List<string>
                {
                    "LINE_ID", "LINE_NAME", "LANE_ID", "ROUT_TYPE_CODE", "CNT_LANE_BOX"
                  , "MAX_CHARGE_NO",  "MAX_DISCHARGE_NO"
                };

                for (int i = 0; i < OperDt.Columns.Count; i++)
                {
                    bool bNull = true;

                    for (int j = 0; j < OperDt.Rows.Count; j++)
                    {
                        if (!OperDt.Rows[j][i].ToString().Equals("-"))
                            bNull = false;
                    }

                    if (ExceptGridColName.Contains(OperDt.Columns[i].ColumnName) == true)
                        continue;
                    //if (sum == 0)
                    //    dgDateOper.Columns[OperDt.Columns[i].ColumnName].Visibility = Visibility.Collapsed;

                    if ((ExceptColName.Contains(OperDt.Columns[i].ColumnName) == false) && (bNull == true))
                        dgFrmOpStatus.Columns[OperDt.Columns[i].ColumnName].Visibility = Visibility.Collapsed;
                    else
                        dgFrmOpStatus.Columns[OperDt.Columns[i].ColumnName].Visibility = Visibility.Visible;
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
            finally
            {
                HiddenLoadingIndicator();
                btnSearch.IsEnabled = true;
            }
        }

        private void GridInit()
        {
            Util.gridClear(dgFrmOpStatus);
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



    }
}
