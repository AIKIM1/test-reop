/*************************************************************************************
 Created Date : 2018.3.27
      Creator : 
   Decription : 불량 대차/불량 그룹LOT 등록'
--------------------------------------------------------------------------------------
 [Change History]
 
**************************************************************************************/

using System;
using System.Windows;
using System.Data;

using C1.WPF;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.CMM001.Extensions;
using C1.WPF.DataGrid;
using System.Linq;
using System.Windows.Media;
using System.Windows.Input;
using System.Windows.Controls;
using Application = System.Windows.Application;
using DataGridLength = C1.WPF.DataGrid.DataGridLength;
using DataGridRow = C1.WPF.DataGrid.DataGridRow;
using LGC.GMES.MES.CMM001;


namespace LGC.GMES.MES.COM001
{

    public partial class COM001_221_DEFECT : C1Window, IWorkArea
    {
        #region Declaration
        Util _Util = new Util();
        BizDataSet _bizRule = new BizDataSet();

        private string _LOTID_RT = string.Empty;  //조립LOT
        private string _SHIFT = string.Empty; //작업조
        private string _WRK_USERID = string.Empty; //작업자ID
        private string _WRK_USER_NAME = string.Empty; //작업자명
        private string _areaID = string.Empty;
        private string _procID = string.Empty;
        private string _eqsgID = string.Empty;

        private bool _load = true;
        private readonly Util _util = new Util();
        private readonly BizDataSet _bizDataSet = new BizDataSet();

        public string _PROCESS { get; set; }
        public string DefectCtnr { get; set; }
        /// <summary>
        ///  Frame과 상호작용하기 위한 객체
        /// </summary>
        public IFrameOperation FrameOperation
        {
            get;
            set;
        }
        #endregion

        #region Initialize

        public COM001_221_DEFECT()
        {
            InitializeComponent();
        }
        #endregion

        #region Event

        #region [Form Load]
        private void C1Window_Loaded(object sender, RoutedEventArgs e)
        {
            if (_load)
            {
                if(DefectCtnr=="Y")
                {
                    SetDefectCtnrControl();
                     object[] parameters = C1WindowExtension.GetParameters(this);

                    _areaID = parameters[0] as string;
                    _procID = parameters[1] as string;
                    _eqsgID = parameters[2] as string;
                    _SHIFT = parameters[3] as string; //작업조
                    _WRK_USER_NAME = parameters[4] as string; //작업자명
                    _WRK_USERID = parameters[5] as string; //작업자ID
                }
                else
                {
                    object[] parameters = C1WindowExtension.GetParameters(this);

                    _areaID = parameters[0] as string;
                    _procID = parameters[1] as string;
                    _eqsgID = parameters[2] as string;
                    _SHIFT = parameters[3] as string; //작업조
                    _WRK_USER_NAME = parameters[4] as string; //작업자명
                    _WRK_USERID = parameters[5] as string; //작업자ID
                    DataTable Ctnr_LOT = parameters[6] as DataTable;

                    if (Ctnr_LOT == null)
                        return;
                     SetGridCartList(Ctnr_LOT);

                     txtCtnrID.Text = Ctnr_LOT.Rows[0]["CTNR_ID"].ToString();
                     chkNew.IsEnabled = false;
                    txtLotRt.Text = Ctnr_LOT.Rows[0]["LOTID_RT"].ToString();
                    btnLotRtSearch.IsEnabled = false;
                    AssyLot();


                }
               
                DEFCCODE();

                dtpDate.SelectedDateTime = GetComSelCalDate();


                //InboxInfo();
                _load = false;
            }

        }
    
        
        // 불량대차 등록 셋팅
        private void SetDefectCtnrControl()
        {
            txtCtnrID.Text = "NEW";
            chkNew.IsChecked = true;
            chkNew.IsEnabled = false;
        }
        //불량 대차 셋팅
        private void SetGridCartList(DataTable dt)
        {
            try
            {
               Util.GridSetData(dgCtnr, dt, FrameOperation);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
            finally
            {

            }
        }


        #endregion


        //조립LOT 조회
        private void txtLotRt_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                if (DefectCtnr == "Y")
                {
                    AssyLot();
                }
             
            }
        }
        //조립LOT 버튼 조회
        private void btnLotRtSearch_Click(object sender, RoutedEventArgs e)
        {
            AssyLot();
        }
        //조립LOT 선택
        private void dgLotRT_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            Point pnt = e.GetPosition(null);
            C1.WPF.DataGrid.DataGridCell cell = dgLotRT.GetCellFromPoint(pnt);

            if (cell != null)
            {

                try
                {
                    _LOTID_RT = Util.NVC(DataTableConverter.GetValue(dgLotRT.CurrentRow.DataItem, "LOTID"));
                }
                catch (Exception ex)
                {
                    Util.MessageException(ex);
                }
            }
        }
        //등록
        private void btnInput_Click(object sender, RoutedEventArgs e)
        {
            if (!Validation())
            {
                return;
            }
            // 등록하시겠습니까?
            Util.MessageConfirm("SFU4615", (result) =>
            {
                if (result == MessageBoxResult.OK)
                {
                    InputLot();
                }
            });
        }
        //대차시트 팝업
        private void btnSheetInput_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidationCartRePrint()) return;

            DataTable dt = PrintCartData();

            if (dt == null || dt.Rows.Count == 0)
            {
                // 대차에 Inbox 정보가 없습니다.
                Util.MessageValidation("SFU4375");
                return;
            }
            // Page수 산출
            int PageCount = dt.Rows.Count % 40 != 0 ? (dt.Rows.Count / 40) + 1 : dt.Rows.Count / 40;
            int start = 0;
            int end = 0;
            DataRow[] dr;

            // Page 수만큼 Pallet List를 채운다
            for (int cnt = 0; cnt < PageCount; cnt++)
            {
                start = (cnt * 40) + 1;
                end = ((cnt + 1) * 40);

                dr = dt.Select("ROWNUM >=" + start + "And ROWNUM <=" + end);
                CartRePrint(dr, cnt + 1);
            }
        }
        //대차시트 팝업
        private void CartRePrint(DataRow[] printrow, int pageCnt)
        {


            CMM_POLYMER_FORM_TAG_PRINT popupCartPrint = new CMM_POLYMER_FORM_TAG_PRINT();
            popupCartPrint.FrameOperation = this.FrameOperation;
            popupCartPrint.DefectCartYN = "Y";
            popupCartPrint.PrintCount = pageCnt.ToString();
            popupCartPrint.DataRowCartSheet = printrow;


            object[] parameters = new object[5];
            parameters[0] = _PROCESS;
            parameters[1] = string.Empty;
            parameters[2] = txtCtnrID.Text;
            parameters[3] = "N";      // Direct 출력 여부
            parameters[4] = "N";      // 임시 대차 출력 여부

            C1WindowExtension.SetParameters(popupCartPrint, parameters);

            popupCartPrint.Closed += new EventHandler(popupCartPrint_Closed);

            foreach (Grid tmp in Util.FindVisualChildren<Grid>(Application.Current.MainWindow))
            {
                if (tmp.Name == "grdMain")
                {
                    tmp.Children.Add(popupCartPrint);
                    popupCartPrint.BringToFront();
                    break;
                }
            }
        }
        //대차시트 팝업 닫기
        private void popupCartPrint_Closed(object sender, EventArgs e)
        {
            CMM_POLYMER_FORM_TAG_PRINT popup = sender as CMM_POLYMER_FORM_TAG_PRINT;
            if (popup != null && popup.DialogResult == MessageBoxResult.OK)
            {
            }

            foreach (Grid tmp in Util.FindVisualChildren<Grid>(Application.Current.MainWindow))
            {
                if (tmp.Name == "grdMain")
                {
                    tmp.Children.Remove(popup);
                    break;
                }
            }

        }


        #region [닫기]
        private void btnClose_Click(object sender, RoutedEventArgs e)
        {

            if(btnInput.IsEnabled == true)
            {
                this.DialogResult = MessageBoxResult.Cancel;
            }
            else
            {
                this.DialogResult = MessageBoxResult.OK;
            }
           

        }
        private void C1Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (btnInput.IsEnabled == true)
            {
                this.DialogResult = MessageBoxResult.Cancel;
            }
            else
            {
                this.DialogResult = MessageBoxResult.OK;
            }
        }
        #endregion


        #endregion

        #region User Method

        #region [BizCall]

        private DateTime GetComSelCalDate()
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.Columns.Add("AREAID", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["AREAID"] = _areaID;
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_COM_SEL_CALDATE", "RQSTDT", "RSLTDT", RQSTDT);

                DateTime dCalDate;

                if (dtResult != null && dtResult.Rows.Count > 0)
                    dCalDate = Convert.ToDateTime(Util.NVC(dtResult.Rows[0]["CALDATE"]));
                else
                    dCalDate = DateTime.Now;

                return dCalDate;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return DateTime.Now;
            }
        }

        /// <summary>
        ///  등록
        /// </summary>
        private void InputLot()
        {
            try
            {
                ShowLoadingIndicator();

                DataSet inData = new DataSet();

                //마스터 정보
                DataTable inDataTable = inData.Tables.Add("INDATA");
                inDataTable.Columns.Add("SRCTYPE", typeof(string));
                inDataTable.Columns.Add("IFMODE", typeof(string));
                inDataTable.Columns.Add("AREAID", typeof(string));
                inDataTable.Columns.Add("EQSGID", typeof(string));
                inDataTable.Columns.Add("PROCID", typeof(string));
                inDataTable.Columns.Add("PKG_LOTID", typeof(string));
                inDataTable.Columns.Add("USERID", typeof(string));
                inDataTable.Columns.Add("SHIFT", typeof(string));
                inDataTable.Columns.Add("WRK_USERID", typeof(string));
                inDataTable.Columns.Add("WRK_USER_NAME", typeof(string));
                inDataTable.Columns.Add("CTNR_ID", typeof(string));
                inDataTable.Columns.Add("POSTDATE", typeof(string));

                DataRow row = null;
                row = inDataTable.NewRow();
                row["SRCTYPE"] = "UI";
                row["IFMODE"] = "OFF";
                row["AREAID"] = _areaID;
                row["EQSGID"] = _eqsgID;
                row["PROCID"] = _procID; ;
                row["PKG_LOTID"] = _LOTID_RT;
                row["USERID"] = LoginInfo.USERID;
                row["SHIFT"] = _SHIFT;
                row["WRK_USERID"] = _WRK_USERID;
                row["WRK_USER_NAME"] = _WRK_USER_NAME;
                if (DefectCtnr == "N")
                {
                    row["CTNR_ID"] = txtCtnrID.Text;
                }
                row["POSTDATE"] = dtpDate.SelectedDateTime.ToString("yyyy-MM-dd");

                inDataTable.Rows.Add(row);
                
                //불량그룹정보
                DataTable inResngr = inData.Tables.Add("INRESNGR");
                inResngr.Columns.Add("DFCT_RSN_GR_ID", typeof(string));
                inResngr.Columns.Add("RSN_GR_QTY", typeof(decimal));

                //불량 그룹별로 GROUP BY
                DataTable LinQ = DataTableConverter.Convert(dgProductionDefect.ItemsSource);
               
                var summarydata = from SUMrow in LinQ.AsEnumerable()
                                  group SUMrow by new
                                  {
                                      RESNGRID = SUMrow.Field<string>("RESNGRID")
                                  } into grp
                                  select new
                                  {
                                      RESNGRID = grp.Key.RESNGRID
                                      ,
                                      ALL_RESNQTY = grp.Sum(r => r.Field<Int32>("RESNQTY"))
                                  };

                DataTable SumDT = new DataTable();
                SumDT = LinQ.Clone();
                foreach (var data in summarydata)
                {
                    DataRow nrow = SumDT.NewRow();
                    nrow["RESNGRID"] = data.RESNGRID;
                    nrow["RESNQTY"] = data.ALL_RESNQTY;
                    SumDT.Rows.Add(nrow);
                }

                for (int i = 0; i < SumDT.Rows.Count; i++)
                {

                    if (SumDT.Rows[i]["RESNQTY"].ToString() != "0")
                    {
                        row = inResngr.NewRow();
                        row["DFCT_RSN_GR_ID"] = SumDT.Rows[i]["RESNGRID"].ToString();
                        row["RSN_GR_QTY"] = Convert.ToDecimal(SumDT.Rows[i]["RESNQTY"].ToString());
                        inResngr.Rows.Add(row);
                    }
                   
                }
                //불량코드별 
                DataTable inResn = inData.Tables.Add("INRESN");
                inResn.Columns.Add("DFCT_RSN_GR_ID", typeof(string));
                inResn.Columns.Add("RESNCODE", typeof(string));
                inResn.Columns.Add("RESNQTY", typeof(decimal));

                for(int i=0; i< dgProductionDefect.Rows.Count-1; i++)
                {
                    row = inResn.NewRow();
                    row["DFCT_RSN_GR_ID"] = Util.NVC(DataTableConverter.GetValue(dgProductionDefect.Rows[i].DataItem, "RESNGRID"));
                    row["RESNCODE"] = Util.NVC(DataTableConverter.GetValue(dgProductionDefect.Rows[i].DataItem, "RESNCODE"));
                    row["RESNQTY"] = Convert.ToDecimal(Util.NVC(DataTableConverter.GetValue(dgProductionDefect.Rows[i].DataItem, "RESNQTY")).Replace(",",""));
                    inResn.Rows.Add(row);
                }
                try
                {
                    //
                    new ClientProxy().ExecuteService_Multi("BR_PRD_REG_DEFECT_LOT_PC", "INDATA,INRESNGR,INRESN", "OUTDATA,OUTCTNR", (Result, ex) =>
                    {
                        if (ex != null)
                        {
                            Util.MessageException(ex);
                            HiddenLoadingIndicator();
                            return;
                        }
                        Util.MessageInfo("SFU1275");//정상처리되었습니다.
                        SetGridCartList(Result.Tables["OUTCTNR"].Rows[0]["CTNR_ID"].ToString());
                        btnSheetInput.IsEnabled = true;
                        btnInput.IsEnabled = false;
                        dgProductionDefect.Columns["RESNQTY"].IsReadOnly = true;

                    }, inData);



                }
                catch (Exception ex)
                {
                    HiddenLoadingIndicator();
                    Util.AlertByBiz("BR_PRD_REG_DEFECT_LOT_PC", ex.Message, ex.ToString());

                }
            }
            catch (Exception ex)
            {
                HiddenLoadingIndicator();
                Util.MessageException(ex);
            }
            finally
            {
                HiddenLoadingIndicator();
            }
        }

        //대차 프린트
        private DataTable PrintCartData()
        {
            try
            {
                DataTable inTable = new DataTable();
                inTable.Columns.Add("LANGID", typeof(string));
                inTable.Columns.Add("EQPTID", typeof(string));
                inTable.Columns.Add("CART_ID", typeof(string));
                inTable.Columns.Add("PROCID", typeof(string));

                DataRow newRow = inTable.NewRow();
                newRow["LANGID"] = LoginInfo.LANGID;
                newRow["CART_ID"] = txtCtnrID.Text;
                inTable.Rows.Add(newRow);

                return new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_CART_SHEET_PC", "INDATA", "OUTDATA", inTable);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return null;
            }
        }
        /// <summary>
        ///  조립LOT 조회
        /// </summary>
        private void AssyLot()
        {
            try
            {

                if (txtLotRt.Text.Length < 4)
                {
                    // Lot ID는 4자리 이상 넣어 주세요.
                    Util.MessageValidation("SFU3450");
                    return;
                }

                DataTable dtRqst = new DataTable();
                dtRqst.Columns.Add("ASSYLOT", typeof(string));
                dtRqst.Columns.Add("LANGID", typeof(string));
                DataRow dr = dtRqst.NewRow();
                dr["ASSYLOT"] = txtLotRt.Text;
                dr["LANGID"] = LoginInfo.LANGID;
                dtRqst.Rows.Add(dr);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_ASSYLOT", "INDATA", "OUTDATA", dtRqst);
                if (dtRslt.Rows.Count > 0)
                {
                    Util.GridSetData(dgLotRT, dtRslt, FrameOperation);
                    txtLotRt.Text = string.Empty;

                }
                else
                {
                    Util.AlertInfo("SFU1905"); //"조회된 Data가 없습니다."
                    txtLotRt.Text = string.Empty;
                }
            }
            catch (Exception ex)
            {
                HiddenLoadingIndicator();
                Util.MessageException(ex);
            }
            finally
            {
                HiddenLoadingIndicator();
            }
        }



        /// <summary>
        ///  불량조회
        /// </summary>
        private void DEFCCODE()
        {
            try
            {
                DataTable dtRqst = new DataTable();
                dtRqst.Columns.Add("LANGID", typeof(string));
                dtRqst.Columns.Add("PROCID", typeof(string));
                dtRqst.Columns.Add("AREAID", typeof(string));
                dtRqst.Columns.Add("ACTID", typeof(string));
                DataRow dr = dtRqst.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["PROCID"] = _PROCESS;
                dr["AREAID"] = LoginInfo.CFG_AREA_ID;
                dr["ACTID"] = "DEFECT_LOT";
                dtRqst.Rows.Add(dr);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_QCA_SEL_WIPRESONCOLLECT_INFO_DEFC_CTNR", "INDATA", "OUTDATA", dtRqst);
                if (dtRslt.Rows.Count > 0)
                {
                    Util.GridSetData(dgProductionDefect, dtRslt, FrameOperation);

                }
            }
            catch (Exception ex)
            {
                HiddenLoadingIndicator();
                Util.MessageException(ex);
            }
            finally
            {
                HiddenLoadingIndicator();
            }
        }

        /// <summary>
        /// 대차 재조회
        /// </summary>
        private void SetGridCartList(string CtnrId)
        {
            try
            {
                DataTable dtRqst = new DataTable();
                dtRqst.Columns.Add("CTNR_ID", typeof(string));
                dtRqst.Columns.Add("LANGID", typeof(string));
                dtRqst.Columns.Add("PROCID", typeof(string));
                DataRow dr = dtRqst.NewRow();
                dr["CTNR_ID"] = CtnrId;
                dr["LANGID"] = LoginInfo.LANGID;
                dr["PROCID"] = _PROCESS;
                dtRqst.Rows.Add(dr);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_POLYMER_FORMATION_NEW_DEFC_CTNR", "INDATA", "OUTDATA", dtRqst);

                if (dtRslt.Rows.Count > 0)
                {
                    Util.gridClear(dgCtnr);
                    Util.GridSetData(dgCtnr, dtRslt, FrameOperation, false);

                }

                txtCtnrID.Text = CtnrId;
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }



        #endregion

        #region[[Validation]
        private bool Validation()
        {

            if(dgLotRT.Rows.Count == 1)
            {
                _LOTID_RT = Util.NVC(DataTableConverter.GetValue(dgLotRT.Rows[0].DataItem, "LOTID"));
            }

            if (_LOTID_RT == string.Empty)
            {
                Util.MessageValidation("SFU4613");// 조립LOT을 선택하세요
                return false;
            }

            decimal SumQty = 0;

            for(int i=0; i< dgProductionDefect.Rows.Count -1; i++)
            {
                SumQty = SumQty + Convert.ToDecimal(Util.NVC(DataTableConverter.GetValue(dgProductionDefect.Rows[i].DataItem, "RESNQTY")).Replace(",", ""));
            }

            if (SumQty == 0)
            {
                Util.MessageValidation("SFU4614"); // 불량수량을 입력하세요.
                return false;
            }
            return true;
        }

        //대차 시트 Validation
        private bool ValidationCartRePrint()
        {

            if (dgCtnr.Rows.Count == 0)
            {
                // 대차 정보가 없습니다.
                Util.MessageValidation("SFU4365");
                return false;
            }

            return true;
        }
        #endregion

        #region [Func]
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

        #endregion


      

    }
}
