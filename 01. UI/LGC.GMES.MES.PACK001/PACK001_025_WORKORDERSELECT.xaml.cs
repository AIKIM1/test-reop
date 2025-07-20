/*************************************************************************************
 Created Date : 2016.08.01
      Creator : Jeong Hyeon Sik
   Decription : Pack 작업지시 선택 팝업
--------------------------------------------------------------------------------------
 [Change History]
  2016.08.01  Jeong Hyeon Sik : Initial Created.
  2020.01.28  손우석 CSR ID 25026 시스템 개선을 통해 수동 포장시 타 모델 W/O로 오 포장 방지 [요청번호] C20200123-000214
  2020.02.12  손우석 CSR ID 25026 시스템 개선을 통해 수동 포장시 타 모델 W/O로 오 포장 방지 [요청번호] C20200123-000214
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
    public partial class PACK001_025_WORKORDERSELECT : C1Window, IWorkArea
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
        //2020.01.28
        private string sPRODID = string.Empty;

        public PACK001_025_WORKORDERSELECT()
        {
            InitializeComponent();
        }

        #endregion Declaration & Constructor 

        #region Initialize

        #endregion Initialize

        #region Event

        private void C1Window_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                this.dtpDateFrom.SelectedDataTimeChanged -= new System.Windows.Controls.SelectionChangedEventHandler(this.dtpDateFrom_SelectedDataTimeChanged);
                this.dtpDateTo.SelectedDataTimeChanged -= new System.Windows.Controls.SelectionChangedEventHandler(this.dtpDateTo_SelectedDataTimeChanged);

                dtpDateFrom.SelectedDateTime = DateTime.Now.AddDays(-7);
                dtpDateTo.SelectedDateTime = DateTime.Now;

                object[] tmps = C1WindowExtension.GetParameters(this);
                if (tmps != null)
                {
                    DataTable dtText = tmps[0] as DataTable;

                    if (dtText.Rows.Count > 0)
                    {
                        sEqsgid = Util.NVC(dtText.Rows[0]["EQSGID"]);
                        //2020.01.28
                        sPRODID = Util.NVC(dtText.Rows[0]["PRODID"]);

                        setWOInfo();
                    }
                }

                this.dtpDateFrom.SelectedDataTimeChanged += new System.Windows.Controls.SelectionChangedEventHandler(this.dtpDateFrom_SelectedDataTimeChanged);
                this.dtpDateTo.SelectedDataTimeChanged += new System.Windows.Controls.SelectionChangedEventHandler(this.dtpDateTo_SelectedDataTimeChanged);
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
                this.Close();
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

        private void dtpDateFrom_SelectedDataTimeChanged(object sender, SelectionChangedEventArgs e)
        {
            setWOInfo();
        }

        private void dtpDateTo_SelectedDataTimeChanged(object sender, SelectionChangedEventArgs e)
        {
            setWOInfo();
        }
        #endregion

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
                string sWOID = null;
                string sMODELID = null;
                //2020.01.28
                string sPROD_ID = sPRODID;

                //sMODELID = sMODELID == "" ? null : sMODELID;
                sMODELID = null;

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
                //2020.01.28
                RQSTDT.Columns.Add("PRODID", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["STRT_DTTM_FROM"] = sSTRT_DTTM_FROM;
                dr["STRT_DTTM_TO"] = sSTRT_DTTM_TO;
                dr["SHOPID"] = sSHOPID;
                dr["AREAID"] = sAREAID;
                dr["EQSGID"] = sEQSGID;
                dr["PCSGID"] = sPCSGID;
                dr["PRJ_NAME"] = sMODELID;
                //2020.01.28
                dr["PRODID"] = sPROD_ID;
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_WORKORDER_PLAN_LIST_PACK_RW", "RQSTDT", "RSLTDT", RQSTDT);

                //dgPlanWorkorderList.ItemsSource = DataTableConverter.Convert(dtResult);
                Util.GridSetData(dgPlanWorkorderList, dtResult, FrameOperation, true);
            }
            catch (Exception ex)
            {
                //Util.AlertByBiz("DA_PRD_SEL_WORKORDER_PLAN_LIST_PACK", ex.Message, ex.ToString());
                Util.MessageException(ex);
            }
        }

        #endregion
    }
}
