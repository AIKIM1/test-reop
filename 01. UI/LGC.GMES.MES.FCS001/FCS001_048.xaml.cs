/*************************************************************************************
 Created Date : 2020-12-19
      Creator : KANG DONG HEE
   Decription : Leak Detector 측정값 조회
--------------------------------------------------------------------------------------
 [Change History]
  2021.04.13  NAME : Initial Created
  2021.04.13  KDH : 화면간 이동 시 초기화 현상 제거
**************************************************************************************/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Net;
using System.Reflection;
using System.Collections;
using System.Globalization;
using System.Data;
using C1.WPF;
using C1.WPF.DataGrid;
using C1.WPF.DataGrid.Summaries;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.ControlsLibrary;

namespace LGC.GMES.MES.FCS001
{
    /// <summary>
    /// FCS001_047.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class FCS001_048 : UserControl, IWorkArea
    {
        #region Declaration & Constructor 
        private string sWorkReSetTime = string.Empty;
        private string sWorkEndTime = string.Empty;

        Util _Util = new Util();

        public FCS001_048()
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
            try
            {
                SetWorkResetTime();
                InitCombo();
                InitControl();

                this.Loaded -= UserControl_Loaded; //2021.04.13 화면간 이동 시 초기화 현상 제거
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        /// <summary>
        /// 화면 내 ComboBox Setting
        /// </summary>
        private void InitCombo()
        {
            CommonCombo_Form ComCombo = new CommonCombo_Form();

            C1ComboBox[] cboLineChild = { cboModel };
            ComCombo.SetCombo(cboLine, CommonCombo_Form.ComboStatus.ALL, sCase: "LINE", cbChild: cboLineChild);

            C1ComboBox[] cboModelParent = { cboLine };
            ComCombo.SetCombo(cboModel, CommonCombo_Form.ComboStatus.ALL, sCase: "LINEMODEL", cbParent: cboModelParent);

            object[] objParent = { "5", cboLine };
            ComCombo.SetComboObjParent(cboEqp, CommonCombo_Form.ComboStatus.ALL, sCase: "EQPDEGASEOL", objParent: objParent);

            cboLine.SelectedIndexChanged += cbo1_SelectedIndexChanged;
            cboModel.SelectedIndexChanged += cbo1_SelectedIndexChanged;
            cboEqp.SelectedIndexChanged += cbo1_SelectedIndexChanged;

            C1ComboBox[] cboLineChild2 = { cboModelHist };
            ComCombo.SetCombo(cboLineHist, CommonCombo_Form.ComboStatus.ALL, sCase: "LINE", cbChild: cboLineChild2);

            C1ComboBox[] cboModelParent2 = { cboLineHist };
            ComCombo.SetCombo(cboModelHist, CommonCombo_Form.ComboStatus.ALL, sCase: "LINEMODEL", cbParent: cboModelParent2);

            object[] objParent2 = { "5", cboLineHist };
            ComCombo.SetComboObjParent(cboEqpHist, CommonCombo_Form.ComboStatus.ALL, sCase: "EQPDEGASEOL", objParent: objParent2);

            cboLineHist.SelectedIndexChanged += cbo2_SelectedIndexChanged;
            cboModelHist.SelectedIndexChanged += cbo2_SelectedIndexChanged;
            cboEqpHist.SelectedIndexChanged += cbo2_SelectedIndexChanged;

        }

        private void InitControl()
        {
            dtpFromDate.SelectedDateTime = GetJobDateFrom();
            dtpToDate.SelectedDateTime = GetJobDateTo();

            dtpFromDateHist.SelectedDateTime = GetJobDateFrom(-1);
            dtpToDateHist.SelectedDateTime = GetJobDateTo(-1);
        }
        #endregion

        #region Event
        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            GetList();
        }

        private void btnSearchHist_Click(object sender, RoutedEventArgs e)
        {
            GetHistList();
        }

        private void txt1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                btnSearch_Click(null, null);
            }
        }

        private void txt2_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                btnSearchHist_Click(null, null);
            }
        }

        private void cbo1_SelectedIndexChanged(object sender, EventArgs e)
        {
            btnSearch_Click(null, null);
        }

        private void cbo2_SelectedIndexChanged(object sender, EventArgs e)
        {
            btnSearchHist_Click(null, null);
        }

        private void chkSPeriod_Checked(object sender, RoutedEventArgs e)
        {
            dtpFromDate.IsEnabled = false;
            dtpToDate.IsEnabled = false;
        }

        private void chkSPeriodHist_Unchecked(object sender, RoutedEventArgs e)
        {
            dtpFromDate.IsEnabled = true;
            dtpToDate.IsEnabled = true;
        }

        private void chkSPeriodHist_Checked(object sender, RoutedEventArgs e)
        {
            dtpFromDate.IsEnabled = false;
            dtpToDate.IsEnabled = false;
        }

        private void chkSPeriod_Unchecked(object sender, RoutedEventArgs e)
        {
            dtpFromDate.IsEnabled = true;
            dtpToDate.IsEnabled = true;
        }
        #endregion

        #region Method
        private void GetList()
        {
            try
            {
                DataTable inDataTable = new DataTable();
                inDataTable.Columns.Add("LANGID", typeof(string));
                inDataTable.Columns.Add("AREAID", typeof(string));
                inDataTable.Columns.Add("MDLLOT_ID", typeof(string));
                inDataTable.Columns.Add("PROD_LOTID", typeof(string));
                inDataTable.Columns.Add("SUBLOTID", typeof(string));
                inDataTable.Columns.Add("EQPTID", typeof(string));
                inDataTable.Columns.Add("FROM_DATE", typeof(string));
                inDataTable.Columns.Add("TO_DATE", typeof(string));

                DataRow newRow = inDataTable.NewRow();
                newRow["LANGID"] = LoginInfo.LANGID;
                newRow["AREAID"] = LoginInfo.CFG_AREA_ID;
                newRow["MDLLOT_ID"] = Util.GetCondition(cboModel, bAllNull: true);
                if (string.IsNullOrEmpty(txtLotId.Text))
                {
                    newRow["PROD_LOTID"] = DBNull.Value;
                }
                else
                {
                    newRow["PROD_LOTID"] = Util.GetCondition(txtLotId, bAllNull: true);
                }

                if (string.IsNullOrEmpty(txtCellId.Text))
                {
                    newRow["SUBLOTID"] = DBNull.Value;
                }
                else
                {
                    newRow["SUBLOTID"] = Util.GetCondition(txtCellId, bAllNull: true);
                }

                newRow["EQPTID"] = Util.GetCondition(cboEqp, bAllNull: true);
                if (chkSPeriod.IsChecked.Equals(true))
                {
                    newRow["FROM_DATE"] = DBNull.Value;
                    newRow["TO_DATE"] = DBNull.Value;
                    if (string.IsNullOrEmpty(txtLotId.Text) && string.IsNullOrEmpty(txtCellId.Text))
                    {
                        Util.Alert("FM_ME_0305"); //조회기간 미 지정시 Lot ID, Cell ID 중 하나는 선택해야합니다.
                        return;
                    }
                }
                else
                {
                    newRow["FROM_DATE"] = dtpFromDate.SelectedDateTime.ToString("yyyyMMdd000000");
                    newRow["TO_DATE"] = dtpToDate.SelectedDateTime.ToString("yyyyMMdd235959");
                }

                inDataTable.Rows.Add(newRow);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_SEL_CELL_LEAK_MEAS_DATA", "INDATA", "OUTDATA", inDataTable);

                Util.GridSetData(dgMeas, dtRslt, FrameOperation, true);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void GetHistList()
        {
            try
            {
                DataTable inDataTable = new DataTable();
                inDataTable.Columns.Add("LANGID", typeof(string));
                inDataTable.Columns.Add("AREAID", typeof(string));
                inDataTable.Columns.Add("MDLLOT_ID", typeof(string));
                inDataTable.Columns.Add("PROD_LOTID", typeof(string));
                inDataTable.Columns.Add("SUBLOTID", typeof(string));
                inDataTable.Columns.Add("EQPTID", typeof(string));
                inDataTable.Columns.Add("FROM_DATE", typeof(string));
                inDataTable.Columns.Add("TO_DATE", typeof(string));

                DataRow newRow = inDataTable.NewRow();
                newRow["LANGID"] = LoginInfo.LANGID;
                newRow["AREAID"] = LoginInfo.CFG_AREA_ID;
                newRow["MDLLOT_ID"] = Util.GetCondition(cboModelHist, bAllNull: true);
                if (string.IsNullOrEmpty(txtLotIdHist.Text))
                {
                    newRow["PROD_LOTID"] = DBNull.Value;
                }
                else
                {
                    newRow["PROD_LOTID"] = Util.GetCondition(txtLotIdHist, bAllNull: true);
                }

                if (string.IsNullOrEmpty(txtCellIdHist.Text))
                {
                    newRow["SUBLOTID"] = DBNull.Value;
                }
                else
                {
                    newRow["SUBLOTID"] = Util.GetCondition(txtCellIdHist, bAllNull: true);
                }

                newRow["EQPTID"] = Util.GetCondition(cboEqpHist, bAllNull: true);

                if (chkSPeriodHist.IsChecked.Equals(true))
                {
                    newRow["FROM_DATE"] = DBNull.Value;
                    newRow["TO_DATE"] = DBNull.Value;
                    if(string.IsNullOrEmpty(txtLotIdHist.Text) && string.IsNullOrEmpty(txtCellIdHist.Text))
                    {
                        Util.Alert("FM_ME_0305"); //조회기간 미 지정시 Lot ID, Cell ID 중 하나는 선택해야합니다.
                        return;
                    }
                }
                else
                {
                    newRow["FROM_DATE"] = dtpFromDateHist.SelectedDateTime.ToString("yyyyMMdd000000");
                    newRow["TO_DATE"] = dtpToDateHist.SelectedDateTime.ToString("yyyyMMdd235959");
                }

                inDataTable.Rows.Add(newRow);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_SEL_CELL_LEAK_MEAS_DATA_HIST", "INDATA", "OUTDATA", inDataTable);

                Util.GridSetData(dgMeasHist, dtRslt, FrameOperation, true);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private DateTime GetJobDateFrom(int iDays = 0)
        {
            string sJobDate = string.Empty;

            if (Convert.ToInt32(DateTime.Now.Hour.ToString() + DateTime.Now.Minute.ToString()) < Convert.ToInt32(sWorkReSetTime.Substring(0, 4)))
            {
                sJobDate = DateTime.Now.AddDays(-1).ToString("yyyyMMdd");
            }
            else
            {
                sJobDate = DateTime.Now.ToString("yyyyMMdd");
            }

            DateTime dJobDate = DateTime.ParseExact(sJobDate + " " + sWorkReSetTime, "yyyyMMdd HHmmss", null);
            return dJobDate;
        }

        private DateTime GetJobDateTo(int iDays = 0)
        {
            string sJobDate = string.Empty;

            if (Convert.ToInt32(DateTime.Now.Hour.ToString() + DateTime.Now.Minute.ToString()) < Convert.ToInt32(sWorkEndTime.Substring(0, 4)))
            {
                sJobDate = DateTime.Now.ToString("yyyyMMdd");
            }
            else
            {
                sJobDate = DateTime.Now.AddDays(1).ToString("yyyyMMdd");
            }

            DateTime dJobDate = DateTime.ParseExact(sJobDate + " " + sWorkEndTime.ToString(), "yyyyMMdd HHmmss", null);

            return dJobDate;
        }

        private void SetWorkResetTime()
        {
            DataTable dtRqst = new DataTable();
            dtRqst.TableName = "RQSTDT";
            dtRqst.Columns.Add("AREAID", typeof(string));

            DataRow dr = dtRqst.NewRow();
            dr["AREAID"] = LoginInfo.CFG_AREA_ID;
            dtRqst.Rows.Add(dr);

            DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_AREAATTR_FOR_OPENTIME", "RQSTDT", "RSLTDT", dtRqst);
            sWorkReSetTime = dtRslt.Rows[0]["OPEN_TIME"].ToString();
            sWorkEndTime = dtRslt.Rows[0]["OPEN_TIME"].ToString();
        }

        #endregion

    }
}
