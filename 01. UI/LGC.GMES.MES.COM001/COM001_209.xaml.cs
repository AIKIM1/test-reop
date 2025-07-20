/*************************************************************************************
 Created Date : 2016.06.16
      Creator : 
   Decription : 
--------------------------------------------------------------------------------------
 [Change History]
  2016.06.16  DEVELOPER : Initial Created.





 
**************************************************************************************/

using C1.WPF;
using C1.WPF.DataGrid;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace LGC.GMES.MES.COM001
{
    public partial class COM001_209 : UserControl, IWorkArea
    {

        Util _Util = new Util();

        #region Declaration & Constructor 
        public COM001_209()
        {
            InitializeComponent();

            Initialize();
        }
        public IFrameOperation FrameOperation
        {
            get;
            set;
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            List<Button> listAuth = new List<Button>();
            listAuth.Add(btnConfrim);
            Util.pageAuth(listAuth, FrameOperation.AUTHORITY);

            SetEvent();
        }

        #endregion

        #region Initialize
        private void Initialize()
        {
            dtpDateFrom.SelectedDateTime = (DateTime)System.DateTime.Now;
            dtpDateTo.SelectedDateTime = (DateTime)System.DateTime.Now;

            CommonCombo _combo = new CommonCombo();

            _combo.SetCombo(cboMoveToArea, CommonCombo.ComboStatus.ALL, sCase: "MOVETOAREA");
            //_combo.SetCombo(cboMoveToArea, CommonCombo.ComboStatus.ALL, sCase: "AREA");


            #region 이력조회화면

            //인수공정
            C1ComboBox[] cboAreaChild_Hist = { cboHistToArea };
            _combo.SetCombo(cboHistToProcid, CommonCombo.ComboStatus.ALL, cbChild: cboAreaChild_Hist, sCase: "POLYMER_PROCESS");

            //인수동
            C1ComboBox[] cboLineParent_Hist = { cboHistToProcid };
            C1ComboBox[] cboLineChild_Hist = { cboHistToEquipmentSegment };
            _combo.SetCombo(cboHistToArea, CommonCombo.ComboStatus.ALL, sCase: "POLYMER_PROCESS_AREA", cbChild: cboLineChild_Hist, cbParent: cboLineParent_Hist);

            //인수라인
            C1ComboBox[] cboProcessParent_Hist = { cboHistToProcid, cboHistToArea };
            _combo.SetCombo(cboHistToEquipmentSegment, CommonCombo.ComboStatus.ALL, cbParent: cboProcessParent_Hist, sCase: "POLYMER_PROCESS_AREA_EQSG");

            string[] sFilter1 = { "MOVE_ORD_STAT_CODE" };
            _combo.SetCombo(cboHistStat, CommonCombo.ComboStatus.ALL, sCase: "COMMCODE", sFilter: sFilter1);

            string[] sFilter2 = { "MOVE_TYPE_CODE_MP_FM" };
            _combo.SetCombo(cboHistToTransType, CommonCombo.ComboStatus.ALL, sCase: "COMMCODE", sFilter: sFilter2);

            //재공구분
            String[] sFilterQLTY = { "", "WIP_QLTY_TYPE_CODE" };
            _combo.SetCombo(cbowipType, CommonCombo.ComboStatus.ALL, sFilter: sFilterQLTY, sCase: "COMMCODES");

            #endregion
        }

        private void SetEvent()
        {
            this.Loaded -= UserControl_Loaded;
           
            dtpDateFrom.SelectedDataTimeChanged += dtpDateFrom_SelectedDataTimeChanged;
            dtpDateTo.SelectedDataTimeChanged += dtpDateTo_SelectedDataTimeChanged;
        }

        #endregion

        #region Event
       
        private void dtpDateFrom_SelectedDataTimeChanged(object sender, SelectionChangedEventArgs e)
        {
            LGCDatePicker dtPik = sender as LGCDatePicker;
            if (Convert.ToDecimal(dtpDateTo.SelectedDateTime.ToString("yyyyMMdd")) < Convert.ToDecimal(dtPik.SelectedDateTime.ToString("yyyyMMdd")))
            {
                dtPik.SelectedDateTime = dtpDateTo.SelectedDateTime;
                return;
            }
        }

        private void dtpDateTo_SelectedDataTimeChanged(object sender, SelectionChangedEventArgs e)
        {
            LGCDatePicker dtPik = sender as LGCDatePicker;
            if (Convert.ToDecimal(dtpDateFrom.SelectedDateTime.ToString("yyyyMMdd")) > Convert.ToDecimal(dtPik.SelectedDateTime.ToString("yyyyMMdd")))
            {
                dtPik.SelectedDateTime = dtpDateFrom.SelectedDateTime;
                return;
            }
        }
        #endregion

        #region 인수처리


        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            Search_data();
        }

        private void Search_data()
        {
            try
            {

                string sBizName = "DA_PRD_SEL_POLYMER_CART_TAKEOVER";

                DataTable dtRqst = new DataTable();

                dtRqst.Columns.Add("LANGID", typeof(string));
                dtRqst.Columns.Add("FROM_AREAID", typeof(string));
                dtRqst.Columns.Add("TO_AREAID", typeof(string));
                dtRqst.Columns.Add("CTNR_ID", typeof(string));
                DataRow dr = dtRqst.NewRow();

                dr["LANGID"] = LoginInfo.LANGID;

                if (cboMoveToArea.SelectedIndex == 0)
                {
                    dr["FROM_AREAID"] = null;
                }
                else
                {
                    dr["FROM_AREAID"] = cboMoveToArea.SelectedValue.ToString();
                }
                dr["TO_AREAID"] = LoginInfo.CFG_AREA_ID;
                dr["CTNR_ID"] = txtCtnr_ID_PR.Text.ToString();
                DataTable dtRslt = new DataTable();

                dtRqst.Rows.Add(dr);
                dtRslt = new ClientProxy().ExecuteServiceSync(sBizName, "INDATA", "OUTDATA", dtRqst);


                if (dtRslt.Rows.Count == 0)
                {
                    //Util.AlertInfo("SFU1905"); //"조회된 Data가 없습니다."
                    dgMoveList.ItemsSource = DataTableConverter.Convert(dtRslt);
                    return;
                }
                else
                {
                    Util.gridClear(dgMoveList);
                    Util.GridSetData(dgMoveList, dtRslt, FrameOperation, true);
        
                }

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
            finally
            {
                loadingIndicator.Visibility = Visibility.Collapsed;
            }
        }

        /// <summary>
        /// 입고처리
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnConfrim_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!Validation())
                {
                    return;
                }
                //인수하시겠습니까?
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("SFU4399"), null, "Information", MessageBoxButton.OKCancel, MessageBoxIcon.None, (result) =>
                        {
                            if (result == MessageBoxResult.OK)
                            {
                                Confrim();
                            }
                        });

            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }

        private void btnInitialize_Click(object sender, RoutedEventArgs e)
        {
            Clear();
        }

        private void Confrim()
        {

            DataSet inData = new DataSet();
            //마스터 정보

            DataTable dt = new DataTable();
            dt = DataTableConverter.Convert(dgMoveList.ItemsSource);
            DataTable inDataTable = inData.Tables.Add("INDATA");
            inDataTable.Columns.Add("SRCTYPE", typeof(string));
            inDataTable.Columns.Add("IFMODE", typeof(string));
            inDataTable.Columns.Add("RCPT_USERID", typeof(string)); // 인수자 ID
            inDataTable.Columns.Add("USERID", typeof(string)); // USERID
            inDataTable.Columns.Add("AREAID", typeof(string)); // 인수동
            inDataTable.Columns.Add("NOTE", typeof(string)); //비고

            DataRow row = null;

            row = inDataTable.NewRow();
            row["SRCTYPE"] = "UI";
            row["IFMODE"] = "OFF";
            row["RCPT_USERID"] = LoginInfo.USERID;
            row["USERID"] = LoginInfo.USERID;
            row["AREAID"] = LoginInfo.CFG_AREA_ID;
            row["NOTE"] = txtRemark.Text;
         
            inDataTable.Rows.Add(row);

            //대차 정보
            DataTable inCtnr = inData.Tables.Add("INCTNR");
            inCtnr.Columns.Add("MOVE_ORD_ID", typeof(string));
            inCtnr.Columns.Add("CTNR_ID", typeof(string));
     
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                if(dt.Rows[i]["CHK"].ToString() == "1")
                {
                    row = inCtnr.NewRow();
                    row["MOVE_ORD_ID"] = dt.Rows[i]["MOVE_ORD_ID"].ToString();
                    row["CTNR_ID"] = dt.Rows[i]["CTNR_ID"].ToString();
                    inCtnr.Rows.Add(row);
                }
                
            }
           
            //인계처리 - 대차 ID
            try
            {
                //인계처리
                new ClientProxy().ExecuteService_Multi("BR_PRD_REG_RECEIVE_CTNR", "INDATA,INCTNR", null, (Result, ex) =>
                {
                    if (ex != null)
                    {
                        Util.MessageException(ex);
                        return;
                    }
                    Util.MessageInfo("SFU1275");  //정상 처리 되었습니다.
                    Search_data();

                }, inData);



            }
            catch (Exception ex)
            {
                Util.AlertByBiz("BR_PRD_REG_RECEIVE_CTNR", ex.Message, ex.ToString());

            }
        }
        private bool Validation()
        {

            if (dgMoveList.Rows.Count == 2)
            {
                Util.MessageValidation("SFU3537");  //조회된 데이터가 없습니다.
                return false;
            }
            DataRow[] drInfo = Util.gridGetChecked(ref dgMoveList, "CHK");

            if (drInfo.Count() <= 0)
            {
                Util.MessageValidation("10008");//선택된 데이터가 없습니다.
                return false;
            }



            return true;
        }
        private void Clear()
        {
            Util.gridClear(dgMoveList);
            cboMoveToArea.SelectedIndex = 0;
            txtRemark.Text = string.Empty;
        }
        private void txtCtnr_ID_PR_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                Search_data();
                txtCtnr_ID_PR.Text = string.Empty;
            }
        }
       
        #endregion

        #region 인수이력

        private void btnSearchHist_Click(object sender, RoutedEventArgs e)
        {
            SearchHist();
        }
        private void SearchHist()
        {
            try
            {
                DataTable dtRqst = new DataTable();
                dtRqst.Columns.Add("LANGID", typeof(string));
                dtRqst.Columns.Add("FROM_AREAID", typeof(string));
                dtRqst.Columns.Add("FROM_EQSGID", typeof(string));
                dtRqst.Columns.Add("FROM_PROCID", typeof(string));
                dtRqst.Columns.Add("MOVE_TYPE_CODE", typeof(string));     //유형
                dtRqst.Columns.Add("MOVE_ORD_STAT_CODE", typeof(string)); // 상태
                dtRqst.Columns.Add("PRODID", typeof(string));
                dtRqst.Columns.Add("PJT_NAME", typeof(string));
                dtRqst.Columns.Add("FROM_DATE", typeof(String));
                dtRqst.Columns.Add("TO_DATE", typeof(String));
                dtRqst.Columns.Add("CTNR_ID", typeof(String));
                dtRqst.Columns.Add("WIP_QLTY_TYPE_CODE", typeof(String));
                //ldpDateFromHist.SelectedDateTime.ToShortDateString();
                DataRow dr = dtRqst.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;

                if (cboHistToArea.SelectedIndex == 0)
                {
                    dr["FROM_AREAID"] = null;
                }
                else
                {
                    dr["FROM_AREAID"] = cboHistToArea.SelectedValue.ToString();
                }

                if (cboHistToEquipmentSegment.SelectedIndex == 0)
                {
                    dr["FROM_EQSGID"] = null;
                }
                else
                {
                    dr["FROM_EQSGID"] = cboHistToEquipmentSegment.SelectedValue.ToString();
                }
                if (cboHistToProcid.SelectedIndex == 0)
                {
                    dr["FROM_PROCID"] = null;
                }
                else
                {
                    dr["FROM_PROCID"] = cboHistToProcid.SelectedValue.ToString();
                }
                if (cboHistToTransType.SelectedIndex == 0)
                {
                    dr["MOVE_TYPE_CODE"] = null;
                }
                else
                {
                    dr["MOVE_TYPE_CODE"] = cboHistToTransType.SelectedValue.ToString();
                }
                if (cboHistStat.SelectedIndex == 0)
                {
                    dr["MOVE_ORD_STAT_CODE"] = null;
                }
                else
                {
                    dr["MOVE_ORD_STAT_CODE"] = cboHistStat.SelectedValue.ToString();
                }
                if (cbowipType.SelectedIndex == 0)
                {
                    dr["WIP_QLTY_TYPE_CODE"] = null;
                }
                else
                {
                    dr["WIP_QLTY_TYPE_CODE"] = cbowipType.SelectedValue.ToString();
                }

                dr["PJT_NAME"] = txtPjt.Text;
                dr["PRODID"] = txtProdIDHist.Text;
                dr["FROM_DATE"] = Util.GetCondition(dtpDateFrom);
                dr["TO_DATE"] = Util.GetCondition(dtpDateTo);
                dr["CTNR_ID"] = txtCtnr_ID.Text;
                dtRqst.Rows.Add(dr);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_POLYMER_CART_TAKEOVER_HIST", "INDATA", "OUTDATA", dtRqst);

                if (dtRslt.Rows.Count == 0)
                {
                    Util.AlertInfo("SFU1905"); //"조회된 Data가 없습니다."
                    dgMove_Info_Hist.ItemsSource = DataTableConverter.Convert(dtRslt);
                    return;
                }


                Util.GridSetData(dgMove_Info_Hist, dtRslt, FrameOperation, true);


            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(ex.Message, null, "Error", MessageBoxButton.OK, ControlsLibrary.MessageBoxIcon.Warning);
                return;
            }
        }

        private void txtCtnr_ID_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                SearchHist();
                txtCtnr_ID.Text = string.Empty;
            }
        }

        #endregion


    }
}
