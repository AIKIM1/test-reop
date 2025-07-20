/*************************************************************************************
 Created Date : 2017.03.07
      Creator : 
   Decription : 
--------------------------------------------------------------------------------------
 [Change History]
  2017.03.07  DEVELOPER : Initial Created.
 
**************************************************************************************/

using C1.WPF;
using C1.WPF.DataGrid;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.CMM001.Extensions;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using System;
using System.Collections.Generic;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Linq;
namespace LGC.GMES.MES.COM001
{
    public partial class COM001_211 : UserControl, IWorkArea
    {
        #region Declaration & Constructor 
        private Util _Util = new Util();
        private BizDataSet _Biz = new BizDataSet();
        public COM001_211()
        {
            InitializeComponent();

            InitCombo();
        }

        public IFrameOperation FrameOperation
        {
            get;
            set;
        }

        #endregion

        #region Initialize
        /// <summary>
        /// 화면내 combo 셋팅
        /// </summary>
        private void InitCombo()
        {

            //동,라인,공정,설비 셋팅
            CommonCombo _combo = new CommonCombo();

            //동
            C1ComboBox[] cboAreaChild = { cboProcess, cboJob };
            _combo.SetCombo(cboArea, CommonCombo.ComboStatus.SELECT, cbChild: cboAreaChild);
             //String[] sFilter1 = { LoginInfo.CFG_AREA_ID };
            //C1ComboBox[] cboAreaChild = { cboEquipment };
            //_combo.SetCombo(cboProcid, CommonCombo.ComboStatus.SELECT, sFilter: sFilter1, cbChild: cboAreaChild, sCase: "POLYMER_PROCESS");
            
            //공정
            C1ComboBox[] cboLineParent = { cboArea };
            C1ComboBox[] cboLineChild = { cboEquipmentSegment };
            _combo.SetCombo(cboProcess, CommonCombo.ComboStatus.SELECT, sCase: "POLYMER_PROCESS", cbChild: cboLineChild, cbParent: cboLineParent);

            //라인
            C1ComboBox[] cboProcessParent = { cboArea, cboProcess };
            C1ComboBox[] cboProcessChild = { cboEquipment};
            _combo.SetCombo(cboEquipmentSegment, CommonCombo.ComboStatus.ALL, cbChild: cboProcessChild, cbParent: cboProcessParent, sCase: "PROCESS_EQUIPMENT");

            //설비
            C1ComboBox[] cboEquipmentParent = { cboEquipmentSegment, cboProcess };
            _combo.SetCombo(cboEquipment, CommonCombo.ComboStatus.ALL, cbParent: cboEquipmentParent);


            // 작업구분
            C1ComboBox[] cboWorkTypeParent = { cboArea };
            _combo.SetCombo(cboJob, CommonCombo.ComboStatus.ALL, cbParent: cboWorkTypeParent, sCase: "FORM_WRK_TYPE_CODE");

            //LOTTYPE
            _combo.SetCombo(cboLotType, CommonCombo.ComboStatus.ALL);
           
            //재공구분
            String[] sFilterQLTY = { "", "WIP_QLTY_TYPE_CODE" };
            _combo.SetCombo(cboQlty, CommonCombo.ComboStatus.ALL, sFilter: sFilterQLTY, sCase: "COMMCODES");

            //용량등급
            String[] sFilterCapa = { "", "CAPA_GRD_CODE" };
           _combo.SetCombo(cboCapaGrd, CommonCombo.ComboStatus.ALL, sFilter: sFilterCapa, sCase: "COMMCODES");
         

            //작업조
            String[] sFilterShift = { cboArea.SelectedValue.ToString(), cboEquipmentSegment.SelectedValue.ToString(), cboProcess.SelectedValue.ToString() };
            _combo.SetCombo(cboShift, CommonCombo.ComboStatus.ALL, sFilter: sFilterShift, sCase: "SHIFT");

          

            //내수/수출 구분
            String[] sFilterMKT = { "", "MKT_TYPE_CODE" };
            _combo.SetCombo(cboMkt, CommonCombo.ComboStatus.ALL, sFilter: sFilterMKT, sCase: "COMMCODES");
        }
        #endregion

        #region Event
        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
           dtpDateFrom.SelectedDateTime = DateTime.Now.AddDays(-7);
           dtpDateTo.SelectedDateTime = DateTime.Now;
            
            //사용자 권한별로 버튼 숨기기
            List<Button> listAuth = new List<Button>();
            
            Util.pageAuth(listAuth, FrameOperation.AUTHORITY);
            //여기까지 사용자 권한별로 버튼 숨기기

            this.Loaded -= UserControl_Loaded;
        }

        private void dtpDate_SelectedDataTimeChanged(object sender, SelectionChangedEventArgs e)
        {
            
        }

        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            ClearValue();
            GetResult();
        }


        private void txtCTNR_ID_KeyDown(object sender, KeyEventArgs e)
        {
            try
            {
                if (e.Key == Key.Enter)
                {
                    GetResult();
                }
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }


        private void dgResult_LoadedCellPresenter(object sender, C1.WPF.DataGrid.DataGridCellEventArgs e)
        {
            dgResult.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (e.Cell.Presenter == null)
                {
                    return;
                }

                if (e.Cell.Row.Type == DataGridRowType.Item)
                {
                    //if(e.Cell.Column.Name == "")

                    //음수수량 색변경
                    if (DataTableConverter.GetValue(e.Cell.Row.DataItem, "WIPSEQ").GetDouble() < 0)
                    {
                        e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Red);
                    }
                    else if (DataTableConverter.GetValue(e.Cell.Row.DataItem, "CALDATE") == null)
                    {
                        e.Cell.Presenter.Background = new SolidColorBrush(Colors.OrangeRed);
                        e.Cell.Presenter.Background = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#D8BFD8"));
                    }
                    else
                    {
                        var convertFromString = System.Windows.Media.ColorConverter.ConvertFromString("#FF000000");
                        if (convertFromString != null)
                            e.Cell.Presenter.Foreground = new SolidColorBrush((Color)convertFromString);
                    }
                }
            }));
        }

        private void dgResult_UnloadedCellPresenter(object sender, DataGridCellEventArgs e)
        {
            if (sender == null)
                return;

            C1DataGrid dataGrid = sender as C1DataGrid;

            dataGrid?.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (e.Cell.Presenter != null)
                {
                    if (e.Cell.Row.Type == DataGridRowType.Item)
                    {
                        var convertFromString = System.Windows.Media.ColorConverter.ConvertFromString("#FF000000");
                        if (convertFromString != null)
                            e.Cell.Presenter.Foreground = new SolidColorBrush((Color)convertFromString);
                    }
                }
            }));
        }

        #endregion

        #region Mehod
        public void GetResult()
        {
            try
            {
                ShowLoadingIndicator();
                DataTable dtRqst = new DataTable();

                dtRqst.Columns.Add("AREAID", typeof(string));
                dtRqst.Columns.Add("PROCID", typeof(string));
                dtRqst.Columns.Add("EQSGID", typeof(string));
                dtRqst.Columns.Add("EQPTID", typeof(string));
                dtRqst.Columns.Add("FROM_DATE", typeof(string));
                dtRqst.Columns.Add("TO_DATE", typeof(string));
                dtRqst.Columns.Add("PJT_NAME", typeof(string));
                dtRqst.Columns.Add("PRODID", typeof(string));
                dtRqst.Columns.Add("LOTID_RT", typeof(string));
                dtRqst.Columns.Add("LOTTYPE", typeof(string));
                dtRqst.Columns.Add("FORM_WRK_TYPE_CODE", typeof(string));
                dtRqst.Columns.Add("WIP_QLTY", typeof(string));
                dtRqst.Columns.Add("CAPA_GRD", typeof(string));
                dtRqst.Columns.Add("LANGID", typeof(string));
                dtRqst.Columns.Add("SHIFT", typeof(string));
                dtRqst.Columns.Add("MKT_TYPE_CODE", typeof(string));
                dtRqst.Columns.Add("CTNR_ID", typeof(string));

                DataRow dr = dtRqst.NewRow();
                if(txtCTNR_ID.Text == string.Empty)
                {

                    dr["AREAID"] = Util.GetCondition(cboArea, bAllNull: true);
                    dr["PROCID"] = Util.GetCondition(cboProcess, "SFU1459"); // 공정을 선택하세요
                    if (dr["PROCID"].Equals("")) 

                    {
                        HiddenLoadingIndicator();
                        return;
                    }

                    dr["EQSGID"] = Util.GetCondition(cboEquipmentSegment, bAllNull: true);
                    dr["EQPTID"] = Util.GetCondition(cboEquipment, bAllNull: true);

                    dr["FROM_DATE"] = Util.GetCondition(dtpDateFrom);
                    dr["TO_DATE"] = Util.GetCondition(dtpDateTo);
                    dr["PJT_NAME"] = txtPJT.Text.ToString();
                    dr["PRODID"] = txtProd.Text.ToString();
                    dr["LOTID_RT"] = txtLotRt.Text.ToString();
                    dr["LOTTYPE"] = Util.GetCondition(cboLotType, bAllNull: true);
                    dr["FORM_WRK_TYPE_CODE"] = Util.GetCondition(cboJob, bAllNull: true);

                    dr["WIP_QLTY"] = Util.GetCondition(cboQlty, bAllNull: true);
                    dr["CAPA_GRD"] = Util.GetCondition(cboCapaGrd, bAllNull: true);
                 
                    dr["LANGID"] = LoginInfo.LANGID;
                    dr["SHIFT"] = Util.GetCondition(cboShift, bAllNull: true);
                    dr["MKT_TYPE_CODE"] = Util.GetCondition(cboMkt, bAllNull: true);
                   
                }
                else
                {
                    dr["AREAID"] = Util.GetCondition(cboArea, bAllNull: true);
                    dr["PROCID"] = Util.GetCondition(cboProcess, "SFU1459"); // 공정을 선택하세요

                    if (dr["PROCID"].Equals(""))

                    {
                        HiddenLoadingIndicator();
                        return;
                    }
                    dr["CTNR_ID"] = txtCTNR_ID.Text;
                    dr["LANGID"] = LoginInfo.LANGID;
                }
                dtRqst.Rows.Add(dr);
                // 머지된 상태에서  스프레드의 집계 기능을 사용하면.. 합계수량이 맞지 않게 나와서
                // Linq를 사용하여 DataTable에서 집계하여 데이터를 바인딩 시킴              
                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_LOT_RT_RESULT", "INDATA", "OUTDATA", dtRqst);

                decimal SumGoodQty = 0; //집계 양품수
                decimal SumDfctQty = 0; //집계 불량수
                decimal SumLossQty = 0; //집계 Loss수
                decimal SumReqQty = 0;  //집계 물청수
                decimal SumInboxQty = 0; //집계 INBOX 수
                decimal SumCellQty = 0;  //집계 Cell수
                
                //여러개의 같은데이터를 GROUP BY 
                DataTable LinQ = new DataTable();
                DataRow Linqrow = null;
                LinQ = dtRslt.Clone();

                for (int i = 0; i < dtRslt.Rows.Count; i++)
                {
                    Linqrow = LinQ.NewRow();
                    Linqrow["LOTID"] = dtRslt.Rows[i]["LOTID"];
                    Linqrow["LOTID_RT"] = dtRslt.Rows[i]["LOTID_RT"];
                    Linqrow["GOOD_QTY"] = dtRslt.Rows[i]["GOOD_QTY"];
                    Linqrow["DEFECT_QTY"] = dtRslt.Rows[i]["DEFECT_QTY"];
                    Linqrow["CNFM_LOSS_QTY"] = dtRslt.Rows[i]["CNFM_LOSS_QTY"];
                    Linqrow["CNFM_PRDT_REQ_QTY"] = dtRslt.Rows[i]["CNFM_PRDT_REQ_QTY"];
                    LinQ.Rows.Add(Linqrow);

                    SumInboxQty = SumInboxQty + Convert.ToDecimal(dtRslt.Rows[i]["INBOX_QTY"].ToString());
                    SumCellQty = SumCellQty + Convert.ToDecimal(dtRslt.Rows[i]["CELL_QTY"].ToString());
                }
                var summarydata = from SUMrow in LinQ.AsEnumerable()
                                  group SUMrow by new
                                  {
                                      LOTID = SUMrow.Field<string>("LOTID")
                                      ,
                                      LOTID_RT = SUMrow.Field<string>("LOTID_RT")
                                  } into grp
                                  select new
                                  {
                                      LOTID = grp.Key.LOTID
                                      ,
                                      LOTID_RT = grp.Key.LOTID_RT
                                      ,
                                      GOOD_QTY = grp.Max(r => r.Field<decimal>("GOOD_QTY"))
                                      ,
                                      DEFECT_QTY = grp.Max(r => r.Field<decimal>("DEFECT_QTY"))
                                       ,
                                      CNFM_LOSS_QTY = grp.Max(r => r.Field<decimal>("CNFM_LOSS_QTY"))
                                      ,
                                      CNFM_PRDT_REQ_QTY = grp.Max(r => r.Field<decimal>("CNFM_PRDT_REQ_QTY"))
                                  };

                // 집계한 수량을 집계 DataTable에 적재
                foreach (var data in summarydata)
                {
                    SumGoodQty = SumGoodQty + Convert.ToDecimal(data.GOOD_QTY);
                    SumDfctQty = SumDfctQty + Convert.ToDecimal(data.DEFECT_QTY);
                    SumLossQty = SumLossQty + Convert.ToDecimal(data.CNFM_LOSS_QTY);
                    SumReqQty  = SumReqQty + Convert.ToDecimal(data.CNFM_PRDT_REQ_QTY);
                }
                DataTable SumDT = new DataTable();
                SumDT = dtRslt.Clone();
                DataRow row = null;
                row = SumDT.NewRow();
                row["GOOD_QTY"] = SumGoodQty;
                row["DEFECT_QTY"] = SumDfctQty;
                row["CNFM_LOSS_QTY"] = SumLossQty;
                row["CNFM_PRDT_REQ_QTY"] = SumReqQty;
                row["INBOX_QTY"] = SumInboxQty;
                row["CELL_QTY"] = SumCellQty;
                SumDT.Rows.Add(row);

                //집계한 데이터를 바인딩 데이터에 머지시킴
                dtRslt.Merge(SumDT);

                Util.gridClear(dgResult);
                Util.GridSetData(dgResult, dtRslt, FrameOperation, true);

                dgResult.Columns["CELL_QTY"].Width = new C1.WPF.DataGrid.DataGridLength(120);
                dgResult.Columns["INBOX_QTY"].Width = new C1.WPF.DataGrid.DataGridLength(100);
            }
            catch (Exception ex)
            {
                HiddenLoadingIndicator();
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
            finally
            {
                HiddenLoadingIndicator();
            }
        }

        #region [초기화]
        private void ClearValue()
        {
            Util.gridClear(dgResult);
        }
        #endregion


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
