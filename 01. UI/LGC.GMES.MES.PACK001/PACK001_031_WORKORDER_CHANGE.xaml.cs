/*************************************************************************************
 Created Date : 2019.01.24
      Creator : Jeong Hyeon Sik
   Decription : Pack 작업지시 선택 팝업
--------------------------------------------------------------------------------------
 [Change History]
  2019.01.24 손우석 CSR ID 3859176 GMES W/O 오더 변경 및 공정일괄 변경 기능 개선 요청의 건 [요청번호]C20181130_59176
**************************************************************************************/

using System;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using C1.WPF;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;

namespace LGC.GMES.MES.PACK001
{
    public partial class PACK001_031_WORKORDER_CHANGE : C1Window, IWorkArea
    {
        #region Declaration & Constructor 
        LGC.GMES.MES.PACK001.Class.MESSAGE_PARAM ms = new LGC.GMES.MES.PACK001.Class.MESSAGE_PARAM();
        /// <summary>
        ///  Frame과 상호작용하기 위한 객체
        /// </summary>
        public IFrameOperation FrameOperation
        {
            get;
            set;
        }

        public string WOID
        {
            get
            {
                return sWoID;
            }

            set
            {
                sWoID = value;
            }
        }

        private string sEqsgid = "";
        private DataView dvRootNodes;
        private string sWoID = "";
        private DataTable dtLot;

        public PACK001_031_WORKORDER_CHANGE()
        {
            InitializeComponent();
        }

        #endregion

        #region Initialize

        #endregion

        #region Event

        private void C1Window_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                dtpDateFrom.SelectedDateTime = DateTime.Now.AddDays(-7);
                dtpDateTo.SelectedDateTime = DateTime.Now;

                object[] tmps = C1WindowExtension.GetParameters(this);
                if (tmps != null)
                {
                    DataTable dtText = tmps[0] as DataTable;

                    if (dtText.Rows.Count > 0)
                    {
                        dgLotList.ItemsSource = DataTableConverter.Convert(dtText);

                        dtLot = dtText;

                        sEqsgid = Util.NVC(dtText.Rows[0]["EQSGID"]);
                    }
                }
            }
            catch (Exception ex)
            {
                Util.Alert(ex.ToString());
            }
        }

        #region Button
        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                setWOInfo();
            }
            catch (Exception ex)
            {
                Util.Alert(ex.ToString());
            }
        }

        private void btnSelect_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                WOID = txtSelectedWO.Text;

                if (!(txtSelectedWO.Text.Length > 0))
                {
                    ms.AlertInfo("SFU1441"); //WORK ORDER ID가 선택되지 않았습니다.
                    return;
                }

                this.DialogResult = MessageBoxResult.OK;

                WOID_Change();
            }
            catch (Exception ex)
            {
                Util.Alert(ex.ToString());
            }
        }

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                this.DialogResult = MessageBoxResult.Cancel;
                this.Close();
            }
            catch (Exception ex)
            {
                Util.Alert(ex.ToString());
            }
        }
        
        #endregion Button

        #region Grid
        private void dgPlanWorkorderList_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            try
            {
                Point pnt = e.GetPosition(null);
                C1.WPF.DataGrid.DataGridCell cell = dgPlanWorkorderList.GetCellFromPoint(pnt);
                if (cell != null)
                {
                    DataTableConverter.SetValue(dgPlanWorkorderList.Rows[cell.Row.Index].DataItem, "CHK", "1");
                }
            }
            catch (Exception ex)
            {
                Util.Alert(ex.ToString());
            }
        }

        private void dgPlanWorkOrderListChoice_Checked(object sender, RoutedEventArgs e)
        {
            try
            {
                int iwoPlanListIndex = Util.gridFindDataRow(ref dgPlanWorkorderList, "CHK", "1", false);
                if (iwoPlanListIndex == -1)
                {
                    return;
                }
                txtSelectedWO.Text = Util.NVC(DataTableConverter.GetValue(dgPlanWorkorderList.Rows[iwoPlanListIndex].DataItem, "WOID"));
            }
            catch (Exception ex)
            {
                Util.Alert(ex.ToString());
            }
        }

        #endregion Grid

        #endregion Event

        #region Mehod

        private void setWOInfo()
        {
            try
            {
                if (Convert.ToDecimal(Convert.ToDateTime(dtpDateFrom.SelectedDateTime).ToString("yyyyMMdd")) > Convert.ToDecimal(Convert.ToDateTime(dtpDateTo.SelectedDateTime).ToString("yyyyMMdd")))
                {
                    Util.Alert("SFU1913");      //종료일자가 시작일자보다 빠릅니다.
                    return;
                }
                
                string sSTRT_DTTM_FROM = dtpDateFrom.SelectedDateTime.ToString("yyyyMMdd");
                string sSTRT_DTTM_TO = dtpDateTo.SelectedDateTime.ToString("yyyyMMdd");
                string sSHOPID = LoginInfo.CFG_SHOP_ID;
                string sAREAID = LoginInfo.CFG_AREA_ID;
                string sEQSGID = sEqsgid;
                string sPCSGID = null;
                string sMODELID = null;

                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("STRT_DTTM_FROM", typeof(string));
                RQSTDT.Columns.Add("STRT_DTTM_TO", typeof(string));
                RQSTDT.Columns.Add("SHOPID", typeof(string));
                RQSTDT.Columns.Add("AREAID", typeof(string));
                RQSTDT.Columns.Add("EQSGID", typeof(string));
                RQSTDT.Columns.Add("PCSGID", typeof(string));
                RQSTDT.Columns.Add("PRJ_NAME", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["STRT_DTTM_FROM"] = sSTRT_DTTM_FROM;
                dr["STRT_DTTM_TO"] = sSTRT_DTTM_TO;
                dr["SHOPID"] = sSHOPID;
                dr["AREAID"] = sAREAID;
                dr["EQSGID"] = sEQSGID;
                dr["PCSGID"] = sPCSGID;
                dr["PRJ_NAME"] = sMODELID;
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_WORKORDER_PLAN_LIST_PACK_RW", "RQSTDT", "RSLTDT", RQSTDT);

                Util.GridSetData(dgPlanWorkorderList, dtResult, FrameOperation, true);
                
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void WOID_Change()
        {
            try
            {
                int iCnt = 0;
                DataTable inDataTable = new DataTable();

                inDataTable.Columns.Add("SRCTYPE", typeof(string));
                inDataTable.Columns.Add("ERP_TRNF_SEQNO", typeof(string));
                inDataTable.Columns.Add("LOTID", typeof(string));
                inDataTable.Columns.Add("PROCID", typeof(string));
                inDataTable.Columns.Add("EQPTID", typeof(string));
                inDataTable.Columns.Add("USERID", typeof(string));
                inDataTable.Columns.Add("PRDT_EQPT_TYPE", typeof(string));
                inDataTable.Columns.Add("WOID", typeof(string));

                for (int i = 0; i < dtLot.Rows.Count; i++)
                {
                    DataRow drRow = inDataTable.NewRow();

                    drRow["SRCTYPE"] = "UI";
                    drRow["LOTID"] = dtLot.Rows[i]["ERP_TRNF_SEQNO"].ToString();
                    drRow["LOTID"] = dtLot.Rows[i]["LOTID"].ToString();
                    drRow["PROCID"] = "";
                    drRow["EQPTID"] = "";
                    drRow["USERID"] = LoginInfo.USERID;
                    drRow["PRDT_EQPT_TYPE"] = "";
                    drRow["WOID"] = txtSelectedWO.Text;
                    drRow["ERP_TRNF_SEQNO"] = dtLot.Rows[i]["ERP_TRNF_SEQNO"].ToString();

                    inDataTable.Rows.Add(drRow);

                    DataTable dtResult = new ClientProxy().ExecuteServiceSync("BR_PRD_REG_WO_CHANGE_BMATYPE_WOID", "INDATA", "OUTDATA", inDataTable);

                    if  (dtResult.Rows.Count > 0)
                    {
                        iCnt = iCnt + 1;
                    }
                }             
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        #endregion Method
    }
}