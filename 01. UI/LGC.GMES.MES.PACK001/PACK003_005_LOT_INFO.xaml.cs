/*************************************************************************************
 Created Date : 2020.09.21
      Creator : 김길용
   Decription : 
--------------------------------------------------------------------------------------
 [Change History]
  2020.09.21  담당자   CSRID             Initialize
  2024.09.11  권성혁   E20240822-001209  하단 OCV 측정일자 중앙값, 상당 OCV 측정일자 중앙값, 편차(gap 일수)값들 화면에 추가
  2025.04.15  김선준   SI                GAP_DATE값 없을 경우 체크로직 추가
  2025.04.16  윤주일   SI                GAP_DATE값 숫자형 체크로직 추가
**************************************************************************************/


using System;
using System.Windows;
using C1.WPF;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.CMM001.Class;
using System.Globalization;
using LGC.GMES.MES.ControlsLibrary;
using System.Data;
using System.Windows.Threading;

namespace LGC.GMES.MES.PACK001
{
    public partial class PACK003_005_LOT_INFO : C1Window, IWorkArea
    {
        #region Declaration & Constructor 
        private string strInput = string.Empty;
        private string strType = string.Empty;
        private BizDataSet _Biz = new BizDataSet();
        private Util _Util = new Util();


        private DataTable isListTable = new DataTable();
        #endregion

        #region Initialize 
        public IFrameOperation FrameOperation
        {
            get;
            set;
        }
        public PACK003_005_LOT_INFO()
        {
            InitializeComponent();
        }

        private void C1Window_Initialized(object sender, EventArgs e)
        {

        }
        #endregion

        #region Event
        //최초 Load
        private void C1Window_Loaded(object sender, RoutedEventArgs e)
        {
            object[] tmps = C1WindowExtension.GetParameters(this);
            string[] tmp = null;
            
            if (tmps != null && tmps.Length >= 1)
            {
                strInput = Util.NVC(tmps[0]) == null ? "" : Util.NVC(tmps[0]);
                strType = Util.NVC(tmps[1]);

                SetGridDetailColumnText();

                if (strType == "RACK")
                {
                    bRackList(strInput);
                    txtConfHold.Text = "N";
                }
                if (strType == "PORT")
                {
                    bPortList(strInput);
                    txtConfHold.Text = "N";
                }

                
            }
            else
            {
            }
            
        }
        #endregion

        #region Method
        //Search - RACK상세 이력조회
        public void bRackList(string rack)
        {
            try
            {
                loadingIndicator.Visibility = Visibility.Visible;
                //조회
                DataTable RQSTDT = new DataTable();

                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("RACKID", typeof(string));
                
                DataRow dr = RQSTDT.NewRow();

                dr["LANGID"] = LoginInfo.LANGID;
                dr["RACKID"] = rack;

                RQSTDT.Rows.Add(dr);

                new ClientProxy().ExecuteService("DA_PRD_SEL_PACK_LOGIS_STK_INPUTLOT", "RQSTDT", "RSLTDT", RQSTDT, (dtResult, ex) =>
                {
                    if (ex != null)
                    {
                        Util.MessageException(ex);
                        loadingIndicator.Visibility = Visibility.Collapsed;
                        return;
                    }

                    if (dtResult.Rows.Count != 0)
                    {
                        DataTable dt = new DataTable();
                        dt.Columns.Add("RACK_ID", typeof(string));
                        dt.Columns.Add("EQPTNAME", typeof(string));
                        dt.Columns.Add("RACK_STAT_CODE", typeof(string));
                        dt.Columns.Add("X_PSTN", typeof(string));
                        dt.Columns.Add("Y_PSTN", typeof(string));
                        dt.Columns.Add("Z_PSTN", typeof(string));
                        DataRow newRow = null;
                        newRow = dt.NewRow();
                        newRow["RACK_ID"] = dtResult.Rows[0]["RACK_ID"];
                        newRow["EQPTNAME"] = dtResult.Rows[0]["EQPTNAME"];
                        newRow["RACK_STAT_CODE"] = dtResult.Rows[0]["RACK_STAT_CODE"];
                        newRow["X_PSTN"] = dtResult.Rows[0]["X_PSTN"];
                        newRow["Y_PSTN"] = dtResult.Rows[0]["Y_PSTN"];
                        newRow["Z_PSTN"] = dtResult.Rows[0]["Z_PSTN"];
                        dt.Rows.Add(newRow);

                        DataTable du = new DataTable();
                        du.Columns.Add("LOTID", typeof(string));
                        //du.Columns.Add("PRJT_NAME", typeof(string));
                        du.Columns.Add("PRODID", typeof(string));
                        du.Columns.Add("PRODNAME", typeof(string));
                        du.Columns.Add("WIPHOLD", typeof(string));

                        
                        for (int i = 0; i < dtResult.Rows.Count; i++)
                        {
                            DataRow newRu = null;
                            newRu = du.NewRow();
                            newRu["LOTID"] = dtResult.Rows[i]["LOTID"];
                            //newRu["PRJT_NAME"] = dtResult.Rows[i]["PRJT_NAME"];
                            newRu["PRODID"] = dtResult.Rows[i]["PRODID"];
                            newRu["PRODNAME"] = dtResult.Rows[i]["PRODNAME"];
                            newRu["WIPHOLD"] = dtResult.Rows[i]["WIPHOLD"];
                            
                            du.Rows.Add(newRu);
                            string dthold = dtResult.Rows[i]["WIPHOLD"].ToString();
                            if (dthold == "Y")
                            {
                                txtConfHold.Text = "Y";
                            }
                        }
                        txtConfRack.Text = dtResult.Rows[0]["RACK_ID"].ToString();
                        txtConfPlt.Text = dtResult.Rows[0]["PLTID"].ToString();
                        txtConfProd.Text = dtResult.Rows[0]["PRODID"].ToString();
                        txtConfQty.Text = dtResult.Rows[0]["LOTCNT"].ToString();
                        this.txtCarrierID.Text = dtResult.Rows[0]["CSTID"].ToString();
                        string LOWER_DATE = dtResult.Rows[0]["LOWER_DATE"].ToString();
                        string UPPER_DATE = dtResult.Rows[0]["UPPER_DATE"].ToString();
                        txtLOWER_DATE.Text = !string.IsNullOrWhiteSpace(LOWER_DATE) ? StringToDate(LOWER_DATE).ToString() : LOWER_DATE.ToString(); 
                        txtUPPER_DATE.Text = !string.IsNullOrWhiteSpace(UPPER_DATE) ? StringToDate(UPPER_DATE).ToString() : LOWER_DATE.ToString();

                        if (!string.IsNullOrEmpty(Util.NVC(dtResult.Rows[0]["GAP_DATE"])))
                        {
                            if (Util.isNumber(dtResult.Rows[0]["GAP_DATE"].ToString()))
                            {
                                int GAP_DATE = Convert.ToInt32(dtResult.Rows[0]["GAP_DATE"]) / 24;
                                txtGAP_DATE.Text = GAP_DATE.ToString();
                            }
                        }

                        Util.GridSetData(dgCsthist, dt, FrameOperation);
                        Util.GridSetData(dgPlthist, du, FrameOperation);
                    }

                    loadingIndicator.Visibility = Visibility.Collapsed;
                });

            }
            catch (Exception ex)
            {
                loadingIndicator.Visibility = Visibility.Collapsed;
                Util.MessageException(ex);
            }
        }
        public void bPortList(string port)
        {
            try
            {
                loadingIndicator.Visibility = Visibility.Visible;
                //조회
                DataTable RQSTDT = new DataTable();

                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("PORTID", typeof(string));

                DataRow dr = RQSTDT.NewRow();

                dr["LANGID"] = LoginInfo.LANGID;
                dr["PORTID"] = port;

                RQSTDT.Rows.Add(dr);
                new ClientProxy().ExecuteService("DA_PRD_SEL_PACK_LOGIS_PORT_INPUTLOT", "RQSTDT", "RSLTDT", RQSTDT, (dtResult, ex) =>
                {
                    if (ex != null)
                    {
                        Util.MessageException(ex);
                        loadingIndicator.Visibility = Visibility.Collapsed;
                        return;
                    }

                    if (dtResult.Rows.Count != 0)
                    {
                        DataTable dt = new DataTable();
                        dt.Columns.Add("PORTID", typeof(string));
                        dt.Columns.Add("PORTNAME", typeof(string));
                        DataRow newRow = null;
                        newRow = dt.NewRow();
                        newRow["PORTID"] = dtResult.Rows[0]["PORTID"];
                        newRow["PORTNAME"] = dtResult.Rows[0]["PORTNAME"];
                        dt.Rows.Add(newRow);

                        DataTable du = new DataTable();
                        du.Columns.Add("LOTID", typeof(string));
                        du.Columns.Add("PRODID", typeof(string));
                        du.Columns.Add("PRODNAME", typeof(string));
                        du.Columns.Add("WIPHOLD", typeof(string));


                        for (int i = 0; i < dtResult.Rows.Count; i++)
                        {
                            DataRow newRu = null;
                            newRu = du.NewRow();
                            newRu["LOTID"] = dtResult.Rows[i]["LOTID"];
                            newRu["PRODID"] = dtResult.Rows[i]["PRODID"];
                            newRu["PRODNAME"] = dtResult.Rows[i]["PRODNAME"];
                            newRu["WIPHOLD"] = dtResult.Rows[i]["WIPHOLD"];
                            string dthold = dtResult.Rows[i]["WIPHOLD"].ToString();
                            if (dthold == "Y")
                            {
                                txtConfHold.Text = "Y";
                            }
                            du.Rows.Add(newRu);
                        }
                        txtConfRack.Text = dtResult.Rows[0]["PORTID"].ToString();
                        txtConfPlt.Text = dtResult.Rows[0]["PLTID"].ToString();
                        this.txtCarrierID.Text = dtResult.Rows[0]["CSTID"].ToString();
                        txtConfProd.Text = dtResult.Rows[0]["PRODID"].ToString();
                        txtConfQty.Text = dtResult.Rows[0]["LOTCNT"].ToString();

                        Util.GridSetData(dgCsthist, dt, FrameOperation);
                        Util.GridSetData(dgPlthist, du, FrameOperation);
                    }

                    loadingIndicator.Visibility = Visibility.Collapsed;
                });
            }
            catch (Exception ex)
            {
                loadingIndicator.Visibility = Visibility.Collapsed;
                Util.MessageException(ex);
            }
        }

        private void SetGridDetailColumnText()
        {
            switch (strType)
            {
                case "RACK":
                    dgCsthist.Columns["PORTID"].Visibility = Visibility.Collapsed;
                    dgCsthist.Columns["PORTNAME"].Visibility = Visibility.Collapsed;
                    dgCsthist.Columns["RACK_ID"].Visibility = Visibility.Visible;
                    break;
                case "PORT":
                    dgCsthist.Columns["PORTID"].Visibility = Visibility.Visible;
                    dgCsthist.Columns["PORTNAME"].Visibility = Visibility.Visible;
                    dgCsthist.Columns["RACK_ID"].Visibility = Visibility.Collapsed;
                    dgCsthist.Columns["EQPTNAME"].Visibility = Visibility.Collapsed;
                    dgCsthist.Columns["RACK_STAT_CODE"].Visibility = Visibility.Collapsed;
                    dgCsthist.Columns["X_PSTN"].Visibility = Visibility.Collapsed;
                    dgCsthist.Columns["Y_PSTN"].Visibility = Visibility.Collapsed;
                    dgCsthist.Columns["Z_PSTN"].Visibility = Visibility.Collapsed;
                    break;
                
            }
        }

        private DateTime StringToDate(string date)
        {
            DateTime sDate = DateTime.ParseExact(date, "yyyyMMddHHmmss", CultureInfo.InvariantCulture, DateTimeStyles.None);
            return sDate;
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

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void DoEvents()
        {
            Application.Current.Dispatcher.Invoke(DispatcherPriority.Background, new System.Threading.ThreadStart(delegate { }));
        }
        #endregion
    }
}
