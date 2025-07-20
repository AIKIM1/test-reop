/*************************************************************************************
 Created Date : 2022.01.24
      Creator : 이정미
   Decription : 색지도 Loss 현황
--------------------------------------------------------------------------------------
 [Change History]
  2022.01.24  이정미 : Initial Created
  2022.04.07  이정미 : S/C Loss 현황 수정
**************************************************************************************/

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
using LGC.GMES.MES.CMM001.Extensions;
using System.Collections;
using System.Linq;



using System.Configuration;
using C1.WPF.Excel;
using Microsoft.Win32;


namespace LGC.GMES.MES.FCS001
{
    public partial class FCS001_125 : UserControl, IWorkArea
    {
        #region [Declaration & Constructor]
        Util _Util = new Util();
        string strFloor = "";
        string strGrp = "";
        string strUnit = "";
        string strEqptId = "";
        public FCS001_125()
        {
            InitializeComponent();
        }

        public IFrameOperation FrameOperation
        {
            get;
            set;
        }
        #endregion


        #region [Initialize]

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            InitCombo();

            InitControl();

            this.Loaded -= UserControl_Loaded;
        }

        private void InitCombo()
        {

            //S/C Loss 현황
            CommonCombo_Form _combo = new CommonCombo_Form();
            string[] sFilter1 = { "FLOOR_CODE" }; //설비층
            _combo.SetCombo(cboScEqptFloor, CommonCombo_Form.ComboStatus.ALL, sCase: "CMN", sFilter: sFilter1);
            string[] sFilter2 = { "FL_CLM_STK" }; //설비UNIT명
            _combo.SetCombo(cboScEqptUnitName, CommonCombo_Form.ComboStatus.ALL, sCase: "AREA_COMMON_CODE", sFilter: sFilter2);

            //C/V Loss 현황
            string[] sFilter3 = { "FLOOR_CODE" }; //설비층
            _combo.SetCombo(cboCvEqptFloor, CommonCombo_Form.ComboStatus.ALL, sCase: "CMN", sFilter: sFilter3);
            string[] sFilter4 = { "CNVR_PANEL_GR_CODE" }; //설비그룹
            _combo.SetCombo(cboCvEqptGroup, CommonCombo_Form.ComboStatus.ALL, sCase: "AREA_COMMON_CODE", sFilter: sFilter4);
            string[] sFilter5 = { "CNVR_UNIT_EQPT_TYPE_CODE" }; //설비UNIT명
            _combo.SetCombo(cboCvEqptUnitName, CommonCombo_Form.ComboStatus.ALL, sCase: "CMN", sFilter: sFilter5);


        }

        private void InitControl()
        {
            //S/C Loss 현황
            dtpScFromDate.SelectedDateTime = System.DateTime.Now.AddDays(0);
            dtpScFromTime.DateTime = DateTime.Now.AddDays(-1);
            dtpScToDate.SelectedDateTime = System.DateTime.Now.AddDays(0);
            dtpScToTime.DateTime = DateTime.Now.AddDays(-1);

            //C/V Loss 현황
            dtpCvFromDate.SelectedDateTime = System.DateTime.Now.AddDays(0);
            dtpCvFromTime.DateTime = DateTime.Now.AddDays(-1);
            dtpCvToDate.SelectedDateTime = System.DateTime.Now.AddDays(0);
            dtpCvToTime.DateTime = DateTime.Now.AddDays(-1);
        }
        #endregion


        #region [Method]
        /// <summary>
        /// Loss 내역 조회
        /// </summary>
        /// 
        private void GetScLossList()
        {
            try
            {
                Util.gridClear(dgScEqptLossList);
                Util.gridClear(dgScEqptLossDetailList);

                DataTable dtRqst = new DataTable();

                dtRqst.TableName = "RQSTDT";
                dtRqst.Columns.Add("LANGID", typeof(string));
                dtRqst.Columns.Add("FLOOR_CD", typeof(string));
                dtRqst.Columns.Add("UNIT_TYPE", typeof(string));
                dtRqst.Columns.Add("FROM_WRK_DATE", typeof(string));
                dtRqst.Columns.Add("TO_WRK_DATE", typeof(string));

                DataRow dr = dtRqst.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["FLOOR_CD"] = Util.GetCondition(cboScEqptFloor, bAllNull: true);
                dr["UNIT_TYPE"] = Util.GetCondition(cboScEqptUnitName, bAllNull: true);
                dr["FROM_WRK_DATE"] = dtpScFromDate.SelectedDateTime.ToString("yyyy-MM-dd") + " " + dtpScFromTime.DateTime.Value.ToString("HH:mm:00");

                //dtpCvFromDate.SelectedDateTime.ToString("yyyyMMdd") + dtpCvFromTime.DateTime.Value.ToString("HHmmss"); //Util.GetCondition(dtpCvFromDate, dtpCvFromTime, bAllNull: true);
                dr["TO_WRK_DATE"] = dtpScToDate.SelectedDateTime.ToString("yyyy-MM-dd") + " " + dtpScToTime.DateTime.Value.ToString("HH:mm:00");
                //dtpCvToDate.SelectedDateTime.ToString("yyyyMMdd") + dtpCvToTime.DateTime.Value.ToString("HHmmss");//Util.GetCondition(dtpCvToDate, dtpCvToTime,  bAllNull: true);
                dtRqst.Rows.Add(dr);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_SEL_STK_LOSS_SUMMARY_UI", "RQSTDT", "RSLTDT", dtRqst);
                Util.GridSetData(dgScEqptLossList, dtRslt, FrameOperation, true);

                string[] sColumnName = new string[] { "FLOOR_NAME", "UNIT_NAME" };
                _Util.SetDataGridMergeExtensionCol(dgScEqptLossList, sColumnName, DataGridMergeMode.VERTICALHIERARCHI);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        private void GetLossList()
        {
            try
            {
                Util.gridClear(dgCvEqptLossList);

                DataTable dtRqst = new DataTable();

                dtRqst.TableName = "RQSTDT";
                dtRqst.Columns.Add("LANGID", typeof(string));
                dtRqst.Columns.Add("FLOOR_CD", typeof(string));
                dtRqst.Columns.Add("GRP_CD", typeof(string));
                dtRqst.Columns.Add("UNIT_TYPE", typeof(string));
                dtRqst.Columns.Add("FROM_WRK_DATE", typeof(string));
                dtRqst.Columns.Add("TO_WRK_DATE", typeof(string));


                DataRow dr = dtRqst.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["FLOOR_CD"] = Util.GetCondition(cboCvEqptFloor, bAllNull: true);
                dr["GRP_CD"] = Util.GetCondition(cboCvEqptGroup, bAllNull: true);
                dr["UNIT_TYPE"] = Util.GetCondition(cboCvEqptUnitName, bAllNull: true);
                dr["FROM_WRK_DATE"] = dtpCvFromDate.SelectedDateTime.ToString("yyyy-MM-dd") + " " + dtpCvFromTime.DateTime.Value.ToString("HH:mm:00");

                //dtpCvFromDate.SelectedDateTime.ToString("yyyyMMdd") + dtpCvFromTime.DateTime.Value.ToString("HHmmss"); //Util.GetCondition(dtpCvFromDate, dtpCvFromTime, bAllNull: true);
                dr["TO_WRK_DATE"] = dtpCvToDate.SelectedDateTime.ToString("yyyy-MM-dd") + " " + dtpCvToTime.DateTime.Value.ToString("HH:mm:00");
                //dtpCvToDate.SelectedDateTime.ToString("yyyyMMdd") + dtpCvToTime.DateTime.Value.ToString("HHmmss");//Util.GetCondition(dtpCvToDate, dtpCvToTime,  bAllNull: true);
                dtRqst.Rows.Add(dr);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_SEL_COLORMAP_LOSS_SUMMARY", "RQSTDT", "RSLTDT", dtRqst);
                Util.GridSetData(dgCvEqptLossList, dtRslt, FrameOperation, true);

                string[] sColumnName = new string[] { "FLOOR_CD", "GRP_CD" };
                _Util.SetDataGridMergeExtensionCol(dgCvEqptLossList, sColumnName, DataGridMergeMode.VERTICALHIERARCHI);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        /// <summary>
        /// Loss 상세 내역 조회
        /// </summary>
        /// 
        /*      private void GetLossDetl(string strFloor, string strGrp, string strUnit)
              {
                  try
                  {
                      //Show Loading indicator
                      //StartLoader();
                      Util.gridClear(dgCvEqptLossDetailList);

                      DataTable dtRqst = new DataTable();
                      dtRqst.TableName = "RQSTDT";
                      dtRqst.Columns.Add("LANGID", typeof(string));
                      dtRqst.Columns.Add("FLOOR_CD", typeof(string));
                      dtRqst.Columns.Add("GRP_CD", typeof(string));
                      dtRqst.Columns.Add("UNIT_TYPE", typeof(string));
                      dtRqst.Columns.Add("FROM_WRK_DATE", typeof(string));
                      dtRqst.Columns.Add("TO_WRK_DATE", typeof(string));

                      DataRow dr = dtRqst.NewRow();
                      dr["LANGID"] = LoginInfo.LANGID;
                      dr["FLOOR_CD"] = strFloor;
                      dr["GRP_CD"] = strGrp;
                      dr["UNIT_TYPE"] = strUnit;
                      dr["FROM_WRK_DATE"] = dtpCvFromDate.SelectedDateTime.ToString("yyyy-MM-dd") + " " + dtpCvFromTime.DateTime.Value.ToString("HH:mm:00");
                      //dtpCvFromDate.SelectedDateTime.ToString("yyyyMMdd") + dtpCvFromTime.DateTime.Value.ToString("HHmmss");
                      dr["TO_WRK_DATE"] = dtpCvToDate.SelectedDateTime.ToString("yyyy-MM-dd") + " " + dtpCvToTime.DateTime.Value.ToString("HH:mm:00");
                      //dtpCvToDate.SelectedDateTime.ToString("yyyyMMdd") + dtpCvToTime.DateTime.Value.ToString("HHmmss");
                      dtRqst.Rows.Add(dr);

                      DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_SEL_COLORMAP_LOSS_SUMMARY_DETAIL", "RQSTDT", "RSLTDT", dtRqst);
                      Util.GridSetData(dgCvEqptLossDetailList, dtRslt, FrameOperation, true);
                  }
                  catch (Exception ex)
                  {
                      Util.MessageException(ex);
                  }
              }*/
        #endregion


        #region [Event]

        private void ScbtnSearch_Click(object sender, RoutedEventArgs e)
        {
            if (dtpScToDate.SelectedDateTime.Date < dtpScFromDate.SelectedDateTime.Date)
            {
                Util.MessageValidation("FM_ME_0182");  //시작일이 종료일보다 클 수 없습니다. 다시 선택해주세요.
            }
            else if (dtpScToDate.SelectedDateTime.Date == dtpScFromDate.SelectedDateTime.Date)
            {
                if (dtpScToTime.DateTime.Value.TimeOfDay < dtpScFromTime.DateTime.Value.TimeOfDay)
                {
                    Util.MessageValidation("FM_ME_0182");  //시작일이 종료일보다 클 수 없습니다. 다시 선택해주세요.
                }
            }
            GetScLossList();
        }

        private void CvbtnSearch_Click(object sender, RoutedEventArgs e)
        {
            if (dtpCvToDate.SelectedDateTime.Date < dtpCvFromDate.SelectedDateTime.Date)
            {
                Util.MessageValidation("FM_ME_0182");  //시작일이 종료일보다 클 수 없습니다. 다시 선택해주세요.
            }
            else if (dtpCvToDate.SelectedDateTime.Date == dtpCvFromDate.SelectedDateTime.Date)
            {
                if (dtpCvToTime.DateTime.Value.TimeOfDay < dtpCvFromTime.DateTime.Value.TimeOfDay)
                {
                    Util.MessageValidation("FM_ME_0182");  //시작일이 종료일보다 클 수 없습니다. 다시 선택해주세요.
                }
            }
            GetLossList();
        }

        private void dgScEqptLossList_CellDoubleClick(object sender, MouseButtonEventArgs e)
        {
            try
            {
                Point pnt = e.GetPosition(null);
                C1.WPF.DataGrid.DataGridCell cell = dgScEqptLossList.GetCellFromPoint(pnt);


                if (!cell.Column.Name.Equals("LOSSCNT")) //부동수량
                {
                    return;
                }

                strEqptId = Util.NVC(DataTableConverter.GetValue(dgScEqptLossList.Rows[cell.Row.Index].DataItem, "EQPTID")).ToString();

                DataTable dtRqst = new DataTable();
                dtRqst.TableName = "RQSTDT";
                dtRqst.Columns.Add("LANGID", typeof(string));
                dtRqst.Columns.Add("EQP_ID", typeof(string));
                dtRqst.Columns.Add("FROM_WRK_DATE", typeof(string));
                dtRqst.Columns.Add("TO_WRK_DATE", typeof(string));

                DataRow dr = dtRqst.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["EQP_ID"] = strEqptId;
                dr["FROM_WRK_DATE"] = dtpScFromDate.SelectedDateTime.ToString("yyyy-MM-dd") + " " + dtpScFromTime.DateTime.Value.ToString("HH:mm:00");
                dr["TO_WRK_DATE"] = dtpScToDate.SelectedDateTime.ToString("yyyy-MM-dd") + " " + dtpScToTime.DateTime.Value.ToString("HH:mm:00");

                dtRqst.Rows.Add(dr);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_SEL_STK_LOSS_SUMMARY_DETAIL_UI", "RQSTDT", "RSLTDT", dtRqst);
                Util.GridSetData(dgScEqptLossDetailList, dtRslt, FrameOperation, true);

                string[] sColumnName = new string[] { "EQPTNAME" };
                _Util.SetDataGridMergeExtensionCol(dgScEqptLossDetailList, sColumnName, DataGridMergeMode.VERTICALHIERARCHI);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return;
            }
        }

        private void dgCvEqptLossList_CellDoubleClick(object sender, MouseButtonEventArgs e)
        {
            try
            {
                Point pnt = e.GetPosition(null);
                C1.WPF.DataGrid.DataGridCell cell = dgCvEqptLossList.GetCellFromPoint(pnt);


                if (!cell.Column.Name.Equals("LOSSCNT")) //부동수량
                {
                    return;
                }

                strFloor = Util.NVC(DataTableConverter.GetValue(dgCvEqptLossList.Rows[cell.Row.Index].DataItem, "FLOOR_CD")).ToString();
                strGrp = Util.NVC(DataTableConverter.GetValue(dgCvEqptLossList.Rows[cell.Row.Index].DataItem, "GRP_CD")).ToString();
                strUnit = Util.NVC(DataTableConverter.GetValue(dgCvEqptLossList.Rows[cell.Row.Index].DataItem, "UNIT_TYPE")).ToString();

                DataTable dtRqst = new DataTable();
                dtRqst.TableName = "RQSTDT";
                dtRqst.Columns.Add("LANGID", typeof(string));
                dtRqst.Columns.Add("FLOOR_CD", typeof(string));
                dtRqst.Columns.Add("GRP_CD", typeof(string));
                dtRqst.Columns.Add("UNIT_TYPE", typeof(string));
                dtRqst.Columns.Add("FROM_WRK_DATE", typeof(string));
                dtRqst.Columns.Add("TO_WRK_DATE", typeof(string));

                DataRow dr = dtRqst.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["FLOOR_CD"] = strFloor;
                dr["GRP_CD"] = strGrp;
                dr["UNIT_TYPE"] = strUnit;
                dr["FROM_WRK_DATE"] = dtpCvFromDate.SelectedDateTime.ToString("yyyy-MM-dd") + " " + dtpCvFromTime.DateTime.Value.ToString("HH:mm:00");
                dr["TO_WRK_DATE"] = dtpCvToDate.SelectedDateTime.ToString("yyyy-MM-dd") + " " + dtpCvToTime.DateTime.Value.ToString("HH:mm:00");

                dtRqst.Rows.Add(dr);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_SEL_COLORMAP_LOSS_SUMMARY_DETAIL", "RQSTDT", "RSLTDT", dtRqst);
                Util.GridSetData(dgCvEqptLossDetailList, dtRslt, FrameOperation, true);

                string[] sColumnName = new string[] { "EQPTNAME" };
                _Util.SetDataGridMergeExtensionCol(dgCvEqptLossDetailList, sColumnName, DataGridMergeMode.VERTICALHIERARCHI);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return;
            }
        }

        #endregion

    }
}
