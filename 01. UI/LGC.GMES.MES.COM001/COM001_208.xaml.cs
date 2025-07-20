/*************************************************************************************
 Created Date : 2017.12.25
      Creator : 오화백
   Decription : Init
--------------------------------------------------------------------------------------
 [Change History]
  2017.12.23  DEVELOPER : Initial Created.





 
**************************************************************************************/

using C1.WPF;
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

using C1.WPF.DataGrid;
using C1.WPF.DataGrid.Summaries;

namespace LGC.GMES.MES.COM001
{
    public partial class COM001_208 : UserControl, IWorkArea
    {

        #region Declaration & Constructor       
        Util _Util = new Util();
        string _FLOWID = string.Empty;
      
        public COM001_208()
        {
            InitializeComponent();
        }

        public IFrameOperation FrameOperation
        {
            get;
            set;
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            ApplyPermissions();
            InitCombo();
            InitControl();
            SetEvent();
        }

        /// <summary>
        /// 화면내 버튼 권한 처리
        /// </summary>
        private void ApplyPermissions()
        {
            List<Button> listAuth = new List<Button>();
            listAuth.Add(btnOut);
            listAuth.Add(btnReturn);
        
            Util.pageAuth(listAuth, FrameOperation.AUTHORITY);            
        }

        #endregion

        #region Initialize
        /// <summary>
        /// 콤보박스 셋팅
        /// </summary>
        private void InitCombo()
        {
            #region 인계화면
            CommonCombo _combo = new CommonCombo();

            //인수공정
            C1ComboBox[] cboAreaChild = { cboMoveToArea };
            string[] sFilter = { string.Empty, string.Empty };
            _combo.SetCombo(cboMoveProcess, CommonCombo.ComboStatus.SELECT, cbChild: cboAreaChild, sFilter: sFilter, sCase: "POLYMER_PROCESS_ROUTE");

            //인수동
            C1ComboBox[] cboLineParent = { cboMoveProcess };
            C1ComboBox[] cboLineChild = { cboMoveEquipmentSegment };
            _combo.SetCombo(cboMoveToArea, CommonCombo.ComboStatus.SELECT, sCase: "POLYMER_PROCESS_AREA", cbChild: cboLineChild, cbParent: cboLineParent);

            //인수라인
            C1ComboBox[] cboProcessParent = { cboMoveProcess, cboMoveToArea };
            _combo.SetCombo(cboMoveEquipmentSegment, CommonCombo.ComboStatus.SELECT, cbParent: cboProcessParent, sCase: "POLYMER_PROCESS_AREA_EQSG");


            #endregion

            #region 이력조회화면
            //인수동
            C1ComboBox[] cboAreaChild_Hist = { cboHistToArea };
            _combo.SetCombo(cboHistToProcid, CommonCombo.ComboStatus.ALL, cbChild: cboAreaChild_Hist, sCase: "POLYMER_PROCESS");

            //인수동
            C1ComboBox[] cboLineParent_Hist = { cboHistToProcid };
            C1ComboBox[] cboLineChild_Hist = { cboHistToEquipmentSegment };
            _combo.SetCombo(cboHistToArea, CommonCombo.ComboStatus.ALL, sCase: "POLYMER_PROCESS_AREA", cbChild: cboLineChild_Hist, cbParent: cboLineParent_Hist);

            //인수라인
            C1ComboBox[] cboProcessParent_Hist = { cboHistToProcid, cboHistToArea };
            _combo.SetCombo(cboHistToEquipmentSegment, CommonCombo.ComboStatus.ALL, cbParent: cboProcessParent_Hist, sCase: "POLYMER_PROCESS_AREA_EQSG");

            //재공구분
            String[] sFilterQLTY = { "", "WIP_QLTY_TYPE_CODE" };
            _combo.SetCombo(cbowipType, CommonCombo.ComboStatus.ALL, sFilter: sFilterQLTY, sCase: "COMMCODES");


            //상태
            string[] sFilter1 = { "MOVE_ORD_STAT_CODE" };
            _combo.SetCombo(cboHistToStat, CommonCombo.ComboStatus.ALL, sCase: "COMMCODE", sFilter: sFilter1);
            
            //이동유형
            string[] sFilter2 = { "MOVE_TYPE_CODE_MP_FM" };
            _combo.SetCombo(cboHistToTransType, CommonCombo.ComboStatus.ALL, sCase: "COMMCODE", sFilter: sFilter2);

            #endregion
        }

        /// <summary>
        /// 컨트롤 초기값 설정
        /// </summary>
        private void InitControl()
        {
            dtpDateFrom.SelectedDateTime = (DateTime)System.DateTime.Now;
            dtpDateTo.SelectedDateTime = (DateTime)System.DateTime.Now;
            txtFromArea.Text = LoginInfo.CFG_AREA_NAME;
         
            txtCTNRID.Focus();
        }

        #endregion

        #region Event

        /// <summary>
        /// Initializing 이후에 FormLoad시 Event를 생성.
        /// </summary>
        private void SetEvent()
        {
            this.Loaded -= UserControl_Loaded;
            dtpDateFrom.SelectedDataTimeChanged += dtpDateFrom_SelectedDataTimeChanged;
            dtpDateTo.SelectedDataTimeChanged += dtpDateTo_SelectedDataTimeChanged;
          
        }

        #endregion

        #region 동간이동 
        private void txtCTNRID_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                string sLotid = txtCTNRID.Text.Trim();
                if (dgMoveList.GetRowCount() > 0)
                {
                    for (int i = 0; i < dgMoveList.GetRowCount(); i++)
                    {
                        if (DataTableConverter.GetValue(dgMoveList.Rows[i].DataItem, "CTNR_ID").ToString() == sLotid)
                        {
                            dgMoveList.SelectedIndex = i;
                            dgMoveList.ScrollIntoView(i, dgMoveList.Columns["CTNR_ID"].Index);
                            txtCTNRID.Focus();
                            txtCTNRID.Text = string.Empty;
                            return;
                        }
                    }
                }
                GetTransferData();
            }
        }
        private void btnRefresh_Click(object sender, RoutedEventArgs e)
        {
            Clear();
        }
        private void btnOut_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!Validation())
                {
                    return;
                }
                //분할하시겠습니까?
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(
                        "SFU4393"), null, "Information", MessageBoxButton.OKCancel, MessageBoxIcon.None, (result) =>
                        {
                            if (result == MessageBoxResult.OK)
                            {
                               CTNR_TRANSFER();
                            }
                        });

            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }
        private void CTNR_TRANSFER()
        {

            //FLOWID 정보
            _FLOWID = FLOWID();

            DataSet inData = new DataSet();
            //마스터 정보

            DataTable inDataTable = inData.Tables.Add("INDATA");
            inDataTable.Columns.Add("SRCTYPE", typeof(string));
            inDataTable.Columns.Add("IFMODE", typeof(string));
            inDataTable.Columns.Add("TO_PROCID", typeof(string)); // 인수공정
            inDataTable.Columns.Add("TO_ROUTID", typeof(string)); // 인수 ROUTE
            inDataTable.Columns.Add("TO_FLOWID", typeof(string)); // 인수 FLOWID
            inDataTable.Columns.Add("TO_EQSGID", typeof(string)); // 인수 라인
            inDataTable.Columns.Add("MOVE_USERID", typeof(string)); //인계자 ID
            inDataTable.Columns.Add("USERID", typeof(string));
            inDataTable.Columns.Add("NOTE", typeof(string));
            inDataTable.Columns.Add("FROM_EQSGID", typeof(string)); // 인계라인
            inDataTable.Columns.Add("FROM_PROCID", typeof(string)); // 인계공정

            DataRow row = null;

            row = inDataTable.NewRow();
            row["SRCTYPE"] = "UI";
            row["IFMODE"] = "OFF";
            row["TO_PROCID"] = cboMoveProcess.SelectedValue.ToString();
            row["TO_ROUTID"] = Util.NVC(DataTableConverter.GetValue(dgMoveList.CurrentRow.DataItem, "ROUTID"));
            row["TO_FLOWID"] = _FLOWID;
            row["TO_EQSGID"] = cboMoveEquipmentSegment.SelectedValue.ToString();
            row["MOVE_USERID"] = LoginInfo.USERID;
            row["USERID"] = LoginInfo.USERID;
            row["NOTE"] = txtRemark.Text;
            row["FROM_EQSGID"] = Util.NVC(DataTableConverter.GetValue(dgMoveList.CurrentRow.DataItem, "EQSGID"));
            row["FROM_PROCID"] = Util.NVC(DataTableConverter.GetValue(dgMoveList.CurrentRow.DataItem, "PROCID"));

            inDataTable.Rows.Add(row);
            //인계처리 - 대차 ID
            DataTable inCtnr = inData.Tables.Add("INCTNR");
            inCtnr.Columns.Add("CTNR_ID", typeof(string));

            row = inCtnr.NewRow();
            row["CTNR_ID"] = Util.NVC(DataTableConverter.GetValue(dgMoveList.CurrentRow.DataItem, "CTNR_ID"));


            inCtnr.Rows.Add(row);

            ShowLoadingIndicator();

            try
            {
                //인계처리
                new ClientProxy().ExecuteService_Multi("BR_PRD_REG_SEND_CTNR", "INDATA,INCTNR", null, (Result, ex) =>
                {
                    HiddenLoadingIndicator();

                    if (ex != null)
                    {
                        Util.MessageException(ex);
                        return;
                    }
                    Util.MessageInfo("SFU1275");  //정상 처리 되었습니다.
                    Clear();

                }, inData);



            }
            catch (Exception ex)
            {
                HiddenLoadingIndicator();
                Util.AlertByBiz("BR_PRD_REG_SEND_CTNR", ex.Message, ex.ToString());

            }
        }
        private void GetTransferData()
        {
            try
            {
                
                string sBizName = "DA_PRD_SEL_POLYMER_CART_TRANSFER";

                DataTable dtRqst = new DataTable();
                dtRqst.Columns.Add("LANGID", typeof(string));
                dtRqst.Columns.Add("CTNR_ID", typeof(string));
                dtRqst.Columns.Add("AREAID", typeof(string));

                DataRow dr = dtRqst.NewRow();

                dr["LANGID"] = LoginInfo.LANGID;
                dr["CTNR_ID"] = Util.GetCondition(txtCTNRID);
                dr["AREAID"] = LoginInfo.CFG_AREA_ID;
                DataTable dtRslt = new DataTable();

                dtRqst.Rows.Add(dr);
                dtRslt = new ClientProxy().ExecuteServiceSync(sBizName, "INDATA", "OUTDATA", dtRqst);


                if (dtRslt.Rows.Count == 0)
                {
                    //Util.AlertInfo("SFU1905"); //"조회된 Data가 없습니다."
                    LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("SFU1905"), null, "Information", MessageBoxButton.OK, MessageBoxIcon.None, (result) =>
                    {
                        if (result == MessageBoxResult.OK)
                        {
                            txtCTNRID.Focus();
                            txtCTNRID.Text = string.Empty;
                        }
                    });
                    return;
                }
                else
                {
                    //DataTable dtSource = DataTableConverter.Convert(dgMoveList.ItemsSource);
                    //dtSource.Merge(dtRslt);
                    Util.gridClear(dgMoveList);
                    //dgListInput.ItemsSource = DataTableConverter.Convert(dtSource);
                    Util.GridSetData(dgMoveList, dtRslt, FrameOperation, false);
                    txtCTNRID.Focus();
                    txtCTNRID.Text = string.Empty;
                    ComboSetting();
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
        private void Clear()
        {
            Util.gridClear(dgMoveList);
            cboMoveProcess.SelectedIndex = 0;
            cboMoveToArea.SelectedIndex = 0;
            cboMoveEquipmentSegment.SelectedIndex = 0;
            txtRemark.Text = string.Empty;
            _FLOWID = string.Empty;
        }

        private string FLOWID()
        {
            string ReturnValue = string.Empty;

            try
            {
                DataTable dtRqst = new DataTable();
                dtRqst.Columns.Add("ROUTID", typeof(string));
                dtRqst.Columns.Add("PROCID", typeof(string));

                DataRow dr = dtRqst.NewRow();
                dr["ROUTID"] = Util.NVC(DataTableConverter.GetValue(dgMoveList.CurrentRow.DataItem, "ROUTID"));
                dr["PROCID"] = Util.NVC(DataTableConverter.GetValue(dgMoveList.CurrentRow.DataItem, "PROCID"));
                dtRqst.Rows.Add(dr);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_POLYMER_TO_FLOWID", "INDATA", "OUTDATA", dtRqst);

                if (dtRslt.Rows.Count > 0)
                {
                    ReturnValue = dtRslt.Rows[0]["FLOWID"].ToString();
                }
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
            return ReturnValue;

        }


        private bool Validation()
        {

            if(dgMoveList.Rows.Count == 0)
            {
                Util.MessageValidation("SFU3537");  //조회된 데이터가 없습니다.
                return false;
            }

            if (cboMoveProcess.SelectedIndex == 0)
            {
                Util.MessageValidation("SFU4394");  //인계 공정을 선택하세요
                return false;
            }
            if (cboMoveToArea.SelectedIndex == 0)
            {
                Util.MessageValidation("SFU4395");  //인계 동을 선택하세요.
                return false;
            }
            if (cboMoveEquipmentSegment.SelectedIndex == 0)
            {
                Util.MessageValidation("SFU4396");  //인계 라인을 선택하세요.
                return false;
            }
            if(cboMoveToArea.SelectedValue.ToString() == Util.NVC(DataTableConverter.GetValue(dgMoveList.CurrentRow.DataItem, "AREAID")))
            {
                Util.MessageValidation("SFU4397");  //조회된 대차정보의 동정봐 인계동 정보가 동일합니다..
                return false;

            }


            return true;
        }

        private void ComboSetting()
        {
          
            CommonCombo _combo = new CommonCombo();

            //인수공정
            C1ComboBox[] cboAreaChild = { cboMoveToArea };
            string[] sFilter = { string.Empty, Util.NVC(DataTableConverter.GetValue(dgMoveList.CurrentRow.DataItem, "ROUTID")) };
            _combo.SetCombo(cboMoveProcess, CommonCombo.ComboStatus.SELECT, cbChild: cboAreaChild, sFilter: sFilter, sCase: "POLYMER_PROCESS_ROUTE");

            //인수동
            C1ComboBox[] cboLineParent = { cboMoveProcess };
            C1ComboBox[] cboLineChild = { cboMoveEquipmentSegment };
            _combo.SetCombo(cboMoveToArea, CommonCombo.ComboStatus.SELECT, sCase: "POLYMER_PROCESS_AREA", cbChild: cboLineChild, cbParent: cboLineParent);

            //인수라인
            C1ComboBox[] cboProcessParent = { cboMoveProcess, cboMoveToArea };
            _combo.SetCombo(cboMoveEquipmentSegment, CommonCombo.ComboStatus.SELECT, cbParent: cboProcessParent, sCase: "POLYMER_PROCESS_AREA_EQSG");

        }


        #endregion

        #region 이력조회
        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            Search_data();
        }

        private void Search_data()
        {
            try
            {
                DataTable dtRqst = new DataTable();
                dtRqst.Columns.Add("LANGID", typeof(string));
                dtRqst.Columns.Add("TO_AREAID", typeof(string));
                dtRqst.Columns.Add("TO_EQSGID", typeof(string));
                dtRqst.Columns.Add("TO_PROCID", typeof(string));
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

                if(cboHistToArea.SelectedIndex == 0)
                {
                    dr["TO_AREAID"] = null;
                }
                else
                {
                    dr["TO_AREAID"] = cboHistToArea.SelectedValue.ToString();
                }
              
                if(cboHistToEquipmentSegment.SelectedIndex == 0)
                {
                    dr["TO_EQSGID"] = null;
                }
                else
                {
                    dr["TO_EQSGID"] = cboHistToEquipmentSegment.SelectedValue.ToString();
                }
                if(cboHistToProcid.SelectedIndex == 0)
                {
                    dr["TO_PROCID"] = null;
                }
               else
                {
                    dr["TO_PROCID"] = cboHistToProcid.SelectedValue.ToString();
                }
                if (cboHistToTransType.SelectedIndex == 0)
                {
                    dr["MOVE_TYPE_CODE"] = null;
                }
                else
                {
                    dr["MOVE_TYPE_CODE"] = cboHistToTransType.SelectedValue.ToString();
                }
                if (cboHistToStat.SelectedIndex == 0)
                {
                    dr["MOVE_ORD_STAT_CODE"] = null;
                }
                else
                {
                    dr["MOVE_ORD_STAT_CODE"] = cboHistToStat.SelectedValue.ToString();
                }
                               
                dr["PJT_NAME"] = txtHistToPJT.Text;
                dr["PRODID"] = txtProd_ID.Text;
                dr["FROM_DATE"] = Util.GetCondition(dtpDateFrom);
                dr["TO_DATE"] = Util.GetCondition(dtpDateTo);
                dr["CTNR_ID"] = txtCtnr_ID.Text;

                if(cbowipType.SelectedIndex == 0)
                {
                    dr["WIP_QLTY_TYPE_CODE"] = null;
                }
                else
                {
                    dr["WIP_QLTY_TYPE_CODE"] = cbowipType.SelectedValue.ToString();
                }

                dtRqst.Rows.Add(dr);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_POLYMER_CART_TRANSFER_HIST", "INDATA", "OUTDATA", dtRqst);

                if (dtRslt.Rows.Count == 0)
                {
                    //Util.AlertInfo("SFU1905"); //"조회된 Data가 없습니다."
                    dgMove_Master.ItemsSource = DataTableConverter.Convert(dtRslt);
                    return;
                }
            

                Util.GridSetData(dgMove_Master, dtRslt, FrameOperation, true);

               
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(ex.Message, null, "Error", MessageBoxButton.OK, ControlsLibrary.MessageBoxIcon.Warning);
                return;
            }
        }
        
        private void btnReprint_Click(object sender, RoutedEventArgs e)
        {
            try
            {
               
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(ex.Message, null, "Error", MessageBoxButton.OK, ControlsLibrary.MessageBoxIcon.Warning);
                return;
            }
        }

        private void RePrint_Result(object sender, EventArgs e)
        {
            try
            {
                LGC.GMES.MES.COM001.Report_Print wndPopup = sender as LGC.GMES.MES.COM001.Report_Print;
                if (wndPopup.DialogResult == MessageBoxResult.OK)
                {
                    Util.MessageInfo("SFU1275");  //정상 처리 되었습니다.
                }
                else
                {
                    return;
                }
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
                return;
            }
        }

        #region EventHandler
        //이벤트 추가 2016.12.15 이슬아 
        #region [인계이력조회] - 기간 선택시 이벤트
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
        private void btnReturn_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!Validation_Cancel())
                {
                    return;
                }
                //분할하시겠습니까?
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(
                        "SFU4398"), null, "Information", MessageBoxButton.OKCancel, MessageBoxIcon.None, (result) =>
                        {
                            if (result == MessageBoxResult.OK)
                            {
                                Cancel();
                            }
                        });

            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }


        private bool Validation_Cancel()
        {

            if (dgMove_Master.Rows.Count == 2)
            {
                Util.MessageValidation("SFU3537");  //조회된 데이터가 없습니다.
                return false;
            }
            DataRow[] drInfo = Util.gridGetChecked(ref dgMove_Master, "CHK");

            if (drInfo.Count() <= 0)
            {
                Util.MessageValidation("10008");//선택된 데이터가 없습니다.
                return false;
            }



            return true;
        }

        private void Cancel()
        {

            DataSet inData = new DataSet();
            //마스터 정보


            DataTable inDataTable = inData.Tables.Add("INDATA");
            inDataTable.Columns.Add("SRCTYPE", typeof(string));
            inDataTable.Columns.Add("IFMODE", typeof(string));
            inDataTable.Columns.Add("MOVE_ORD_ID", typeof(string)); // 이동지시 ID
            inDataTable.Columns.Add("RCPT_USERID", typeof(string)); // 인수자 ID
            inDataTable.Columns.Add("USERID", typeof(string)); // USERID
            inDataTable.Columns.Add("AREAID", typeof(string)); // 인수동
            inDataTable.Columns.Add("NOTE", typeof(string)); //비고
            inDataTable.Columns.Add("CTNR_ID", typeof(string)); //비고

            DataRow row = null;

            for (int i = 0; i < dgMove_Master.Rows.Count; i++)
            {
                if (Util.NVC(DataTableConverter.GetValue(dgMove_Master.Rows[i].DataItem, "CHK")).Equals("1"))
                {

                    row = inDataTable.NewRow();
                    row["SRCTYPE"] = "UI";
                    row["IFMODE"] = "OFF";
                    row["MOVE_ORD_ID"] = Util.NVC(DataTableConverter.GetValue(dgMove_Master.CurrentRow.DataItem, "MOVE_ORD_ID"));
                    row["RCPT_USERID"] = LoginInfo.USERID;
                    row["USERID"] = LoginInfo.USERID;
                    row["AREAID"] = LoginInfo.CFG_AREA_ID;
                    row["NOTE"] = LoginInfo.USERID;
                    row["CTNR_ID"] = Util.NVC(DataTableConverter.GetValue(dgMove_Master.CurrentRow.DataItem, "CTNR_ID"));
                    inDataTable.Rows.Add(row);
                }
            }

            //인계처리 - 대차 ID
            try
            {
                //인계처리
                new ClientProxy().ExecuteService_Multi("BR_PRD_REG_CANCEL_SEND_CTNR", "INDATA", null, (Result, ex) =>
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
                Util.AlertByBiz("BR_PRD_REG_CANCEL_SEND_CTNR", ex.Message, ex.ToString());

            }
        }





        #endregion

        #endregion

        private void txtCtnr_ID_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                Search_data();
                txtCtnr_ID.Text = string.Empty;
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



    }
}
