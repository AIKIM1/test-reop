/*************************************************************************************
 Created Date : 2016.08.01
      Creator : Jeong Hyeon Sik
   Decription : Pack 작업지시 선택 팝업
--------------------------------------------------------------------------------------
 [Change History]
  2016.08.01  Jeong Hyeon Sik : Initial Created.
  2018.10.12  손우석 같은 제품 다른 라인인경우 포장 W/O 구분을위해 EQSGID 파라메터 추가
  2019.02.13  손우석 포장 제품코드만 조회되록 수정
  2019.12.11  손우석 SM CMI Pack 메시지 다국어 처리 요청
  2025.07.03  윤주일 HD_OSS_0402 CHK 처리 "1" -> "True"
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
    public partial class PACK001_015_WORKORDERSELECT : C1Window, IWorkArea
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

        public string PRODID
        {
            get
            {
                return sProdID;
            }

            set
            {
                sProdID = value;
            }
        }

        public string PCSGID
        {
            get
            {
                return sPcsgID;
            }

            set
            {
                sPcsgID = value;
            }
        }

        public string WOTYPE
        {
            get
            {
                return sWoType;
            }

            set
            {
                sWoType = value;
            }
        }

        //2018.10.12
        public string EQSGID
        {
            get
            {
                return sEqsgID;
            }

            set
            {
                sEqsgID = value;
            }
        }

        public string EQSGNAME
        {
            get
            {
                return sEQSGNAME;
            }

            set
            {
                sEQSGNAME = value;
            }
        }

        private string sEqsgid = "";
        private DataView dvRootNodes;
        private string sWoID = "";
        private string sProdID = "";
        private string sPcsgID = "";
        private string sProdID_LOT = "";
        private string sPRDT_CLSS_CODE = "";
        private string sWoType = "";
        //2018.10.12
        private string sEqsgID = "";
        private string sEQSGNAME = "";

        public PACK001_015_WORKORDERSELECT()
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
                        sProdID = Util.NVC(dtText.Rows[0]["PRODID"]);
                        sProdID_LOT = Util.NVC(dtText.Rows[0]["PRODID_LOT"]);
                        sPRDT_CLSS_CODE = Util.NVC(dtText.Rows[0]["PROD_CLSS_CODE"]);
                        //2018.10.12
                        sEqsgID = Util.NVC(dtText.Rows[0]["EQSGID"]);

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
                    //DataTableConverter.SetValue(dgPlanWorkorderList.Rows[cell.Row.Index].DataItem, "CHK", "1");
                    DataTableConverter.SetValue(dgPlanWorkorderList.Rows[cell.Row.Index].DataItem, "CHK", "True");
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
                //int iwoPlanListIndex = Util.gridFindDataRow(ref dgPlanWorkorderList, "CHK", "1", false);
                int iwoPlanListIndex = Util.gridFindDataRow(ref dgPlanWorkorderList, "CHK", "True", false);
                if (iwoPlanListIndex == -1)
                {
                    return;
                }

                txtSelectedWO.Text = Util.NVC(DataTableConverter.GetValue(dgPlanWorkorderList.Rows[iwoPlanListIndex].DataItem, "WOID"));                
                PRODID = Util.NVC(DataTableConverter.GetValue(dgPlanWorkorderList.Rows[iwoPlanListIndex].DataItem, "PRODID"));
                PCSGID = Util.NVC(DataTableConverter.GetValue(dgPlanWorkorderList.Rows[iwoPlanListIndex].DataItem, "PCSGID"));
                WOTYPE = Util.NVC(DataTableConverter.GetValue(dgPlanWorkorderList.Rows[iwoPlanListIndex].DataItem, "WOTYPE"));
                //2018.10.12
                EQSGID = Util.NVC(DataTableConverter.GetValue(dgPlanWorkorderList.Rows[iwoPlanListIndex].DataItem, "EQSGID"));
                EQSGNAME = Util.NVC(DataTableConverter.GetValue(dgPlanWorkorderList.Rows[iwoPlanListIndex].DataItem, "EQSGNAME"));
                //PCSGID = "B";
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
                string SPRODID = sProdID;

                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("STRT_DTTM_FROM", typeof(string));
                RQSTDT.Columns.Add("STRT_DTTM_TO", typeof(string));              
                RQSTDT.Columns.Add("PRODID", typeof(string));
                RQSTDT.Columns.Add("PRDT_CLSS_CODE", typeof(string));
                RQSTDT.Columns.Add("LOT_PRODID", typeof(string));
                //2018.10.12
                RQSTDT.Columns.Add("EQSGID", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["STRT_DTTM_FROM"] = sSTRT_DTTM_FROM;
                dr["STRT_DTTM_TO"] = sSTRT_DTTM_TO;
                //2019.02.13
                //dr["PRODID"] = SPRODID=="" ? null : SPRODID;
                if(sPRDT_CLSS_CODE == "CMA")
                {
                    dr["PRODID"] = null;
                }
                else
                {
                    dr["PRODID"] = SPRODID == "" ? null : SPRODID;
                }
                dr["PRDT_CLSS_CODE"] = sPRDT_CLSS_CODE == "" ? null : sPRDT_CLSS_CODE;
                dr["LOT_PRODID"] = sProdID_LOT == "" ? null : sProdID_LOT;
                //2018.10.12
                dr["EQSGID"] = sEqsgID == "" ? null : sEqsgID;

                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_WORKORDERSELECT_PACKING", "RQSTDT", "RSLTDT", RQSTDT);

                //dgPlanWorkorderList.ItemsSource = DataTableConverter.Convert(dtResult);
                Util.GridSetData(dgPlanWorkorderList, dtResult, FrameOperation, true);
                
            }
            catch (Exception ex)
            {
                //2019.12.11
                //Util.AlertByBiz("DA_PRD_SEL_WORKORDERSELECT_PACKING", ex.Message, ex.ToString());
                Util.MessageException(ex);
            }
        }
        #endregion
    }
}
