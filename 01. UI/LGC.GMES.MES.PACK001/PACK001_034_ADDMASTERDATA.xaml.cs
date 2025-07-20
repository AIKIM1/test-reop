/*************************************************************************************
 Created Date : 2018.07.20
      Creator : 손홍구
   Decription : Master Lot이력- 데이터 팝업
--------------------------------------------------------------------------------------
 [Change History]
  2018.07.20  손홍구 : Initial Created.
  2019.12.11  손우석 SM CMI Pack 메시지 다국어 처리 요청
**************************************************************************************/

using System;
using System.Collections.Generic;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using C1.WPF;
using C1.WPF.DataGrid;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;

namespace LGC.GMES.MES.PACK001
{
    public partial class PACK001_034_ADDMASTERDATA : C1Window, IWorkArea
    {
        #region Declaration & Constructor 

        /// <summary>
        ///  Frame과 상호작용하기 위한 객체
        /// </summary>
        public IFrameOperation FrameOperation
        {
            get;
            set;
        }

        public PACK001_034_ADDMASTERDATA()
        {
            InitializeComponent();
        }

        private DataTable dtLotINFO = null;
        private string sLotid = string.Empty;

        #endregion

        #region Initialize

        #endregion

        #region Event
        private void C1Window_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                List<Button> listAuth = new List<Button>();
                Util.pageAuth(listAuth, FrameOperation.AUTHORITY);

                object[] tmps = C1WindowExtension.GetParameters(this);
                if (tmps != null)
                {
                    DataTable dtText = tmps[0] as DataTable;

                    if (dtText.Rows.Count > 0)
                    {
                        txtSearchLot.Text = Util.NVC(dtText.Rows[0]["LOTID"]);

                        sLotid = txtSearchLot.Text.Trim();

                        getWipdatacollect_Q(sLotid);
                        getWipdatacollect_E(sLotid);
                        //getLabelHustory(sLotid);
                    }
                }
            }
            catch (Exception ex)
            {
                Util.Alert(ex.ToString());

            }
        }

        private void txtSearchLot_KeyDown(object sender, KeyEventArgs e)
        {
            try
            {
                if(e.Key == Key.Enter)
                {
                    if (txtSearchLot.Text.Trim().Length > 0)
                    {
                        sLotid = txtSearchLot.Text.Trim();

                        getWipdatacollect_Q(sLotid);
                        getWipdatacollect_E(sLotid);
                        //getLabelHustory(sLotid);
                    }
                }
            }
            catch(Exception ex)
            {
                Util.Alert(ex.ToString());
            }
        }

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            //if(this.DialogResult != MessageBoxResult.OK)
            //{
            //    this.DialogResult = MessageBoxResult.OK;
            //}
            this.DialogResult = MessageBoxResult.OK;
            this.Close();
        }

        #endregion

        #region Mehod

        private void getWipdatacollect_Q(string sLOTID)
        {
            try
            {
                //DA_PRD_SEL_WIPDATACOLLECT_CLCTTYPE_Q
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("LOTID", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["LOTID"] = sLOTID;
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_MST_SMPL_DATA_CLCT_TYPE_Q", "RQSTDT", "RSLTDT", RQSTDT);

                //dgInspectionData.ItemsSource = DataTableConverter.Convert(dtResult);
                Util.GridSetData(dgInspectionData, dtResult, FrameOperation, true);
            }
            catch (Exception ex)
            {
                //2019.12.11
                //Util.AlertByBiz("DA_PRD_SEL_MST_SMPL_DATA_CLCT_TYPE_Q", ex.Message, ex.ToString());
                Util.MessageException(ex);
            }
        }

        private void getWipdatacollect_E(string sLOTID)
        {
            try
            {
                //DA_PRD_SEL_WIPDATACOLLECT_CLCTTYPE_E
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("LOTID", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["LOTID"] = sLOTID;
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_MST_SMPL_DATA_CLCT_TYPE_E", "RQSTDT", "RSLTDT", RQSTDT);

                //dgInspectionData.ItemsSource = DataTableConverter.Convert(dtResult);
                Util.GridSetData(dgDetailData, dtResult, FrameOperation, true);
            }
            catch (Exception ex)
            {
                //2019.12.11
                //Util.AlertByBiz("DA_PRD_SEL_MST_SMPL_DATA_CLCT_TYPE_E", ex.Message, ex.ToString());
                Util.MessageException(ex);
            }
        }

        //private void getLabelHustory(string sLOTID)
        //{
        //    try
        //    {
        //        DataTable RQSTDT = new DataTable();
        //        RQSTDT.TableName = "RQSTDT";
        //        RQSTDT.Columns.Add("LOTID", typeof(string));

        //        DataRow dr = RQSTDT.NewRow();
        //        dr["LOTID"] = sLOTID;

        //        RQSTDT.Rows.Add(dr);

        //        DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_PACKLABEL_HIST", "INDATA", "OUTDATA", RQSTDT);

        //        Util.GridSetData(dgLabelHistory, dtResult, FrameOperation, true);
        //    }
        //    catch(Exception ex)
        //    {
        //        Util.AlertByBiz("", ex.Message, ex.ToString());
        //    }
        //}

        private void btnExcelInspectionData_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                new LGC.GMES.MES.Common.ExcelExporter().Export(dgInspectionData);
            }
            catch (Exception ex)
            {
                Util.Alert(ex.ToString());
            }
        }

        private void btnExcelDetailData_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                new LGC.GMES.MES.Common.ExcelExporter().Export(dgDetailData);
            }
            catch (Exception ex)
            {
                Util.Alert(ex.ToString());
            }
        }

        //private void btnExcelLabelHist_Click(object sender, RoutedEventArgs e)
        //{
        //    try
        //    {
        //        new LGC.GMES.MES.Common.ExcelExporter().Export(dgLabelHistory);
        //    }
        //    catch (Exception ex)
        //    {
        //        Util.Alert(ex.ToString());
        //    }
        //}
        #endregion


    }
}
