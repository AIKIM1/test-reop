/*************************************************************************************
 Created Date : 2018.08.09
      Creator : 오화백K
   Decription : Pallet 생성 팝업
--------------------------------------------------------------------------------------
 [Change History]
 2022.05.12  C20220509-000185 라인MIX 적용
 2023.08.23  E20230704-000395 Differentiate functions for IM and non-IM according to OUTBOXID  \
 2024.04.19  이홍주    : SI               NFF 경우 OCV 검사결과 확인부분 제외함
 2025.05.15  이홍주    : SI               MES2.0 대응
**************************************************************************************/

using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Data;

using C1.WPF;
using C1.WPF.DataGrid;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.ControlsLibrary;
using System.ComponentModel;
using System.Threading;
using System.Linq;
using C1.WPF.DataGrid.Summaries;
using System.Windows.Media;

namespace LGC.GMES.MES.FCS002
{
    /// <summary>
    /// BOX001_201_RUNSTART.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class FCS002_303_CREATEPALLET : C1Window, IWorkArea
    {
        Util _util = new Util();

        string _AREAID = string.Empty;
        string sUSERID = string.Empty;
        string sSHFTID = string.Empty;

        public IFrameOperation FrameOperation
        {
            get;
            set;
        }

        public string PALLET_ID
        {
            get;
            set;
        }
        #region Initialize
        public FCS002_303_CREATEPALLET()
        {
            InitializeComponent();
        }

        private void C1Window_Loaded(object sender, RoutedEventArgs e)
        {
            object[] tmps = C1WindowExtension.GetParameters(this);
            _AREAID = Util.NVC(tmps[0]);
            sUSERID = Util.NVC(tmps[1]);
            sSHFTID = Util.NVC(tmps[2]);

            InitCombo();
            InitControl();
        }

        /// <summary>
        /// 콤보박스 초기 설정
        /// </summary>
        private void InitCombo()
        {
            CommonCombo _combo = new CommonCombo();
        }

        /// <summary>
        /// 컨트롤 초기값 설정
        /// </summary>
        private void InitControl()
        {
            object[] tmps = C1WindowExtension.GetParameters(this);
        }
        #endregion

        #region 작업 Pallet 색깔표시 : dgPalletList_LoadedCellPresenter()

        private void dgInPallet_LoadedCellPresenter(object sender, C1.WPF.DataGrid.DataGridCellEventArgs e)
        {
            if (sender == null)
                return;

            C1.WPF.DataGrid.C1DataGrid dataGrid = sender as C1.WPF.DataGrid.C1DataGrid;

            dataGrid.Dispatcher.BeginInvoke(new Action(() =>
            {

                if (e.Cell.Presenter == null)
                {
                    return;
                }
                //Grid Data Binding 이용한 Background 색 변경
                if (e.Cell.Row.Type == C1.WPF.DataGrid.DataGridRowType.Item)
                {
                    if (!Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "OCV_SPEC_RESULT")).Equals("OK"))
                    {
                        e.Cell.Presenter.Background = null;

                    }
                    else
                    {
                        e.Cell.Presenter.Background = new SolidColorBrush(Colors.OrangeRed);
                    }
                }
            }));
        }

        #endregion

        #region [EVENT]

        #region 텍스트박스 포커스 : text_GotFocus()
        private void text_GotFocus(object sender, RoutedEventArgs e)
        {
            InputMethod.Current.ImeConversionMode = ImeConversionModeValues.Alphanumeric;
        }
        #endregion

        #region Pallet 생성 : btnSave_Click()
        private void btnCreate_Click(object sender, RoutedEventArgs e)
        {
            //OCV RESULT OK아닐시 생성 차단
            bool sOcvchk = true;

            DataTable dt = DataTableConverter.Convert(dgInPallet.ItemsSource);
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                if (dt.Rows[i]["OUTBOXID"].ToString() != string.Empty)
                {
                    //NFF의 경우 OCV 검사결과 확인부분 제외함. 추후 MMD 공통코드로 사용여부 사용할 경우 공통코드로 제외 해야함.(2024.04.18)
                    //(공통코드 CNJ_AUTOBOX_FCS_OCV_CHK 에 등록되어야함.)

                    //if (!Util.NVC(dgInPallet.GetCell(i, dgInPallet.Columns["OCV_SPEC_RESULT"].Index).Value).Equals("OK"))
                    //{
                    //    //OCV SPEC이 맞지 않아 포장이 불가능합니다.
                    //    Util.MessageValidation("SFU8227");
                    //    sOcvchk = false;
                    //    return;
                    //}
                    //CNSSL19C,CNSSL20C,CNSSL21C

                    /*C20210906-000208 로 수정
                    //C20210305-000498 로 수정. INBOXID 7자리 : 테슬라 재활용 인박스, 8자리 : 테슬라 일회용 인박스. 혼입금지
                    int iInboxID_0_len = dt.Rows[0]["INBOXID"].ToString().Trim().Length;  //0번째 INBOXID 길이
                    int iInboxIDlen = dt.Rows[i]["INBOXID"].ToString().Trim().Length;   //i번째 INBOXID 길이
                    if (iInboxID_0_len != iInboxIDlen)
                    {
                        string sOutBoxID = dt.Rows[i]["OUTBOXID"].ToString();

                        Util.MessageValidation("SFU3776", sOutBoxID);  //유형이 다른 인박스는 하나의 팔레트에 혼입할 수 없습니다. [%1]
                        return;
                    }
                    */

                    //C20210906-000208
                    string sTypeFlag0 = dt.Rows[0]["TYPE_FLAG"].ToString().Trim();  //0번째 TYPE_FLAG
                    string sTypeFlagi = dt.Rows[i]["TYPE_FLAG"].ToString().Trim();  //i번째 TYPE_FLAG
                    if (sTypeFlag0 != sTypeFlagi)
                    {
                        string sOutBoxID = dt.Rows[i]["OUTBOXID"].ToString();

                        Util.MessageValidation("SFU3806", sOutBoxID);  //유형이 다른 박스는 하나의 팔레트에 혼입할 수 없습니다. [%1]
                        return;
                    }
                }
            }

            if (sOcvchk == true)
            {
                DataTable dtSource = DataTableConverter.Convert(dgInPallet.ItemsSource);
                string sPROD_LINE = dtSource.Rows[0]["PROD_LINE"].ToString(); ;

                var query = (from t in dtSource.AsEnumerable()
                             where t.Field<string>("PROD_LINE") != sPROD_LINE
                             select t).Distinct();

                if (query.Any())
                {
                    //Line이 혼합되어 있습니다. Pallet 생성하시겠습니까?
                    Util.MessageConfirm("SFU8494", (result) =>
                    {
                        if (result == MessageBoxResult.OK)
                        {
                            RunStart("Y");
                        }
                    });
                }
                else
                {
                    //Pallet 생성하시겠습니까?
                    Util.MessageConfirm("SFU5009", (result) =>
                    {
                        if (result == MessageBoxResult.OK)
                        {
                            RunStart("N");
                        }
                    });
                }
            }

        }
        #endregion

        #region 닫기 : btnClose_Click()
        /// <summary>
        /// 닫기
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = MessageBoxResult.Cancel;
        }


        #endregion

        #region 삭제 : btnInPalletDelete_Click()
        private void btnInPalletDelete_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                int iCnt = 0;

                DataTable dt = DataTableConverter.Convert(dgInPallet.ItemsSource);

                foreach (DataRow dr in dt.AsEnumerable().ToList())
                {
                    if (dr["CHK"].SafeToBoolean())
                    {
                        dt.Rows.Remove(dr);
                    }
                }

                Util.GridSetData(dgInPallet, dt, FrameOperation, false);

                if (dt.Rows.Count == 0)
                {
                    setShipToPopControl(string.Empty);
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        #endregion

        #region OutBox 체크 : txtInPalletID_KeyDown()
        private void txtInPalletID_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                try
                {
                    DataSet inDataSet = new DataSet();

                    DataTable inDataTable = inDataSet.Tables.Add("INDATA");
                    inDataTable.Columns.Add("LANGID");
                    inDataTable.Columns.Add("PALLETID");

                    DataTable inBoxTable = inDataSet.Tables.Add("INOUTBOX");
                    inBoxTable.Columns.Add("BOXID");

                    DataRow newRow = inDataTable.NewRow();
                    newRow["LANGID"] = LoginInfo.LANGID;
                    newRow["PALLETID"] = null;
                    inDataTable.Rows.Add(newRow);
                    newRow = null;

                    newRow = inBoxTable.NewRow();
                    newRow["BOXID"] = txtInPalletID.Text;
                    //（IM：27 bit；Non_IM：12 bit)
                    string sOutBox = string.Empty;
                    if ((txtInPalletID.Text).Trim().Length == 12)
                    {
                        sOutBox = "NonIM";
                    }
                    else
                    {
                        sOutBox = "IM";
                    }
                    inBoxTable.Rows.Add(newRow);

                    //setShipToPopControl("MCCM348015A1-D14", "CNH016502");

                    string sAssyLotLinemixFlag = "";

                    //OUTBOX 체크
                    DataSet dsResult = new ClientProxy().ExecuteServiceSync_Multi("BR_PRD_GET_INPUT_OUTBOX_MIX_MB", "INDATA,INOUTBOX", "OUTDATA", inDataSet);

                    if (dsResult.Tables["OUTDATA"] != null || dsResult.Tables["OUTDATA"].Rows.Count > 0)
                    {
                        if (string.IsNullOrEmpty(dsResult.Tables["OUTDATA"].Rows[0]["ASSY_LOT_LINE_MIX_NO"].ToString()) ||
                            dsResult.Tables["OUTDATA"].Rows[0]["ASSY_LOT_LINE_MIX_NO"].ToString() == "0")
                        {
                            sAssyLotLinemixFlag = "N";
                        }
                        else
                        {
                            sAssyLotLinemixFlag = "Y";
                        }

                        DataRow[] dr = dsResult.Tables["OUTDATA"].Select();

                        object[] param = new object[] { dr[0]["TOTAL_QTY"] };
                        if (sOutBox.Equals("IM"))
                        {
                            //if ((int)dr[0]["TOTAL_QTY"] < 256)
                            //{
                            //    // BOX 수량은 %1 입니다. 추가 하시겠습니까? 
                            //    Util.MessageConfirm("SFU8207", (msgresult) =>
                            //    {
                            //        if (msgresult == MessageBoxResult.OK)
                            //        {
                            //            GetCompleteOutbox(dsResult.Tables["OUTDATA"].Rows[0]["BOXID"].ToString(), sAssyLotLinemixFlag, sOutBox, "");
                            //        }
                            //        else
                            //        {
                            //            return;
                            //        }
                            //    }, param);
                            //}
                            //else if ((int)dr[0]["TOTAL_QTY"] == 256)
                            //{
                            //    GetCompleteOutbox(dsResult.Tables["OUTDATA"].Rows[0]["BOXID"].ToString(), sAssyLotLinemixFlag, sOutBox, "");
                            //}
                            GetCompleteOutbox(dsResult.Tables["OUTDATA"].Rows[0]["BOXID"].ToString(), sAssyLotLinemixFlag, sOutBox, "");
                        }
                        else if (sOutBox.Equals("NonIM"))
                        {
                            GetCompleteOutbox(dsResult.Tables["OUTDATA"].Rows[0]["BOXID"].ToString(), sAssyLotLinemixFlag, sOutBox, dr[0]["SHIPTO_ID"].ToString());
                        }

                    }
                }
                catch (Exception ex)
                {
                    Util.MessageException(ex);
                }
                finally
                {
                    txtInPalletID.Text = string.Empty;
                }
            }
        }
        #endregion

        #endregion

        #region [Method]

        #region Pallet 생성 : RunStart()
        private void RunStart(string pAssyLotLinemixFlag)
        {
            if (dgInPallet.Rows.Count == 1)
            {
                //Outbox 정보가 없습니다
                Util.MessageValidation("SFU5010");
                return;
            }

            if (String.IsNullOrWhiteSpace(Util.NVC(popShipto.SelectedValue).Trim()))
            {
                //출하처를 선택하세요.
                Util.MessageInfo("SFU4096");
                return;
            }

            try
            {
                DataTable dt = DataTableConverter.Convert(dgInPallet.ItemsSource);

                DataSet inDataSet = new DataSet();

                DataTable inDataTable = inDataSet.Tables.Add("INDATA");
                inDataTable.Columns.Add("SRCTYPE");
                inDataTable.Columns.Add("LANGID");
                inDataTable.Columns.Add("SHFT_ID");
                inDataTable.Columns.Add("USERID");
                inDataTable.Columns.Add("EQSGID");
                inDataTable.Columns.Add("SHIPTO_ID");
                inDataTable.Columns.Add("MULTI_SHIPTO_FLAG");
                inDataTable.Columns.Add("ASSY_LOT_LINE_MIX_NO");
                inDataTable.Columns.Add("LOTTYPE");
                inDataTable.Columns.Add("EXP_DOM_TYPE_CODE");


                DataTable inBoxTable = inDataSet.Tables.Add("INOUTBOX");
                inBoxTable.Columns.Add("BOXID");

                DataRow newRow = inDataTable.NewRow();
                newRow["SRCTYPE"] = "UI";
                newRow["SHFT_ID"] = sSHFTID;
                newRow["USERID"] = sUSERID;
                newRow["SHIPTO_ID"] = popShipto.SelectedValue.ToString().Trim();
                newRow["LANGID"] = LoginInfo.LANGID;

                if (chkMultiShipTo.IsChecked == true)
                {
                    newRow["MULTI_SHIPTO_FLAG"] = "Y";
                }
                else
                {
                    newRow["MULTI_SHIPTO_FLAG"] = "N";
                }

                if (pAssyLotLinemixFlag == "Y")
                {
                    newRow["ASSY_LOT_LINE_MIX_NO"] = dt.Rows[0]["MIX_LINE"].ToString();
                }
                else
                {
                    newRow["ASSY_LOT_LINE_MIX_NO"] = "0";
                }
                newRow["LOTTYPE"] = dt.Rows[0]["LOTTYPE"].ToString();
                newRow["EQSGID"] = dt.Rows[0]["EQSGID"].ToString();
                newRow["EXP_DOM_TYPE_CODE"] = dt.Rows[0]["EXP_DOM_TYPE_CODE"].ToString();



                inDataTable.Rows.Add(newRow);

                newRow = null;
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    if (dt.Rows[i]["OUTBOXID"].ToString() != string.Empty)
                    {
                        newRow = inBoxTable.NewRow();
                        newRow["BOXID"] = Util.NVC(dgInPallet.GetCell(i, dgInPallet.Columns["OUTBOXID"].Index).Value);

                        inBoxTable.Rows.Add(newRow);
                    }
                }
                //DataSet dsResult = new ClientProxy().ExecuteServiceSync_Multi("BR_PRD_REG_2ND_PALLET_NEW_MB", "INDATA,INOUTBOX", "OUTDATA", inDataSet);
                DataSet dsResult = new ClientProxy().ExecuteServiceSync_Multi("BR_PRD_REG_2ND_PALLET_NEW_MB", "INDATA,INOUTBOX", "OUTDATA", inDataSet);
                if (dsResult != null && dsResult.Tables["OUTDATA"] != null)
                {
                    PALLET_ID = dsResult.Tables["OUTDATA"].Rows[0]["PALLETID"].ToString();
                }

                this.DialogResult = MessageBoxResult.OK;

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }

        }
        #endregion

        #region 조회된 OUTBOX 바인드 : GetCompleteOutbox()
        private void GetCompleteOutbox(string BoxID, string pAssyLotLinemixFlag, string OutBox, string ShipTo)
        {
            try
            {
                DataTable inTable = new DataTable();
                inTable.Columns.Add("BOXID", typeof(string));
                inTable.Columns.Add("LANGID", typeof(string));
                inTable.Columns.Add("MULTI_SHIPTO_FLAG", typeof(string));

                DataRow newRow = inTable.NewRow();
                newRow["BOXID"] = BoxID;
                newRow["LANGID"] = LoginInfo.LANGID;
                if (chkMultiShipTo.IsChecked == true)
                {
                    newRow["MULTI_SHIPTO_FLAG"] = "Y";
                }
                else
                {
                    newRow["MULTI_SHIPTO_FLAG"] = "N";
                }
                inTable.Rows.Add(newRow);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_COMPLETE_OUTBOX_PALLET_MB", "INDATA", "OUTDATA", inTable);

                if (dtResult != null && dtResult.Rows.Count > 0)
                {
                    DataTable dtSource = DataTableConverter.Convert(dgInPallet.ItemsSource);
                    var query = (from t in dtSource.AsEnumerable()
                                 where t.Field<string>("OUTBOXID") == BoxID
                                 select t).Distinct();

                    if (query.Any())
                    {
                        //	SFU1781	이미 추가 된 OUTBOX 입니다.
                        Util.MessageValidation("SFU5011");
                        return;
                    }
                    // 출하처 선택방식 분리처리_IM:기존유지, Non_IM:.생성시 등록된 출하처 매핑
                    if (OutBox.Equals("IM"))
                    {
                        // 최초 Outbox 바인딩 시, 출하처 콤보 Set
                        if (dtResult.Rows.Count == 1)
                        {
                            //setShipToPopControl(dtResult.Rows[0]["PRODID"].ToString());

                            /*C20210906-000208 로 변경
                            //C20210305-000490 : INBOXID 7자리 : 테슬라 재활용일 경우 CNH020441 , 8자리 : 테슬라 일회용일 경우 CNH016502
                            if (dtResult.Rows[0]["INBOXID"].ToString().Trim().Length == 7)
                            {
                                setShipToPopControl(dtResult.Rows[0]["PRODID"].ToString(), "CNH020441");
                            }
                            else
                            {
                                setShipToPopControl(dtResult.Rows[0]["PRODID"].ToString(), "CNH016502");
                            }
                            */

                            //C20210906-000208
                            setShipToPopControl(dtResult.Rows[0]["PRODID"].ToString(), dtResult.Rows[0]["SHIP_TO"].ToString());
                        }
                    }
                    else if (OutBox.Equals("NonIM"))
                    {
                        // 최초 Outbox 바인딩 시, 출하처 콤보 Set
                        if (dtResult.Rows.Count == 1)
                        {
                            setShipToPopControl(dtResult.Rows[0]["PRODID"].ToString(), ShipTo);
                        }
                        else
                        {
                            if (!ShipTo.Equals(popShipto.SelectedValue))
                            {
                                Util.MessageValidation("SFU1503");
                                return;
                            }
                        }
                    }

                    if (dtSource != null)
                    {

                        for (int i = 0; i < dtSource.Rows.Count; i++)
                        {
                            if (dtResult.Rows[0]["PRODID"].ToString() != dtSource.Rows[i]["PRODID"].ToString())
                            {
                                //동일 제품이 아닙니다
                                Util.MessageValidation("SFU1502");
                                return;
                            }
                            if (dtResult.Rows[0]["EXP_DOM_TYPE_CODE"].ToString() != dtSource.Rows[i]["EXP_DOM_TYPE_CODE"].ToString())
                            {
                                //동일한 시장유형이 아닙니다.
                                Util.MessageValidation("SFU4271");
                                return;
                            }
                            if (dtResult.Rows[0]["LOTTYPE"].ToString() != dtSource.Rows[i]["LOTTYPE"].ToString())
                            {
                                //동일 LOT 유형이 아닙니다.
                                Util.MessageValidation("SFU4513");
                                return;
                            }
                            if (dtResult.Rows[0]["PROD_MONTH"].ToString() != dtSource.Rows[i]["PROD_MONTH"].ToString())
                            {
                                //동일한 생산월이 아닙니다.
                                Util.MessageValidation("SFU4644");
                                return;
                            }

                            //라인 믹스 적용 후 혼합된 OUTBOX인지 확인한다. 혼합:MIX가능, 혼합아니면 기존 동일 처리 -  2022.05.12
                            //if (dtResult.Rows[0]["PROD_LINE"].ToString() != dtSource.Rows[i]["PROD_LINE"].ToString())

                            if (pAssyLotLinemixFlag == "Y")
                            {

                                if (!string.IsNullOrEmpty(dtResult.Rows[0]["MIX_LINE"].ToString()) || dtResult.Rows[0]["MIX_LINE"].ToString() != "0")   //라인 믹스 적용 케이스인 경우
                                {
                                    if (dtResult.Rows[0]["MIX_LINE"].ToString() != dtSource.Rows[i]["MIX_LINE"].ToString())
                                    {
                                        //동일한 MIX LINE이 아닙니다.
                                        Util.MessageValidation("SFU4968");
                                        return;
                                    }
                                }
                                else //기준정보에 등록이 안되어 있는 경우 또는 /LINE MIX 아닌경우 예전 Logic를 탈 수 있도록(MMD 공통코드 등록: TESLA_LINE_MIX_NJ)
                                {

                                    if (dtResult.Rows[0]["PROD_LINE"].ToString() != dtSource.Rows[i]["PROD_LINE"].ToString())
                                    {
                                        //동일한 라인이 아닙니다.
                                        Util.MessageValidation("SFU4645");
                                        return;
                                    }
                                }
                            }
                            else //LINE MIX 아닌경우 기존과 동일하게 처리
                            {
                                {
                                    if (dtResult.Rows[0]["PROD_LINE"].ToString() != dtSource.Rows[i]["PROD_LINE"].ToString())
                                    {
                                        //동일한 라인이 아닙니다.
                                        Util.MessageValidation("SFU4645");
                                        return;
                                    }
                                }
                            }

                            /* C20210906-000208 로 변경
                            //C20210305-000498 로 수정. INBOXID 7자리 : 테슬라 재활용 인박스, 8자리 : 테슬라 일회용 인박스. 혼입금지
                            if (dtResult.Rows[0]["INBOXID"].ToString().Trim().Length != dtSource.Rows[i]["INBOXID"].ToString().Trim().Length)
                            {
                                string sOutBoxID = dtResult.Rows[0]["OUTBOXID"].ToString();
                                //유형이 다른 인박스는 하나의 팔레트에 혼입할 수 없습니다. [%1]
                                Util.MessageValidation("SFU3776", sOutBoxID);
                                return;
                            }
                            */
                            //C20210906-000208
                            if (dtResult.Rows[0]["TYPE_FLAG"].ToString().Trim() != dtSource.Rows[i]["TYPE_FLAG"].ToString().Trim())
                            {
                                string sOutBoxID = dtResult.Rows[0]["OUTBOXID"].ToString();
                                //유형이 다른 박스는 하나의 팔레트에 혼입할 수 없습니다. [%1]
                                Util.MessageValidation("SFU3806", sOutBoxID);
                                return;
                            }
                        }
                    }

                    dtResult.Merge(dtSource);
                    Util.GridSetData(dgInPallet, dtResult, FrameOperation, false);

                }

                if (dtResult != null && dtResult.Rows.Count > 0)
                {
                    dgInPallet.CurrentCell = dgInPallet.GetCell(0, 1);
                }

                string[] sColumnName = new string[] { "OUTBOXID2", "BOXSEQ", "OUTBOXID", "OUTBOXQTY" };
                if (dgInPallet.Rows.Count > 0)
                {
                    DataGridAggregate.SetAggregateFunctions(dgInPallet.Columns["INBOXQTY"], new DataGridAggregatesCollection { new DataGridAggregateSum() });
                }
                _util.SetDataGridMergeExtensionCol(dgInPallet, sColumnName, DataGridMergeMode.VERTICALHIERARCHI);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        #endregion

        #endregion

        private void setShipToPopControl(string prodID, String ShipToID = null)
        {
            if (prodID != string.Empty)
            {
                const string bizRuleName = "DA_PRD_SEL_SHIPTO_CBO_MB";
                string[] arrColumn = { "SHOPID", "PRODID", "LANGID" };
                string[] arrCondition = { LoginInfo.CFG_SHOP_ID, prodID, LoginInfo.LANGID };
                CommonCombo.SetFindPopupCombo(bizRuleName, popShipto, arrColumn, arrCondition, (string)popShipto.SelectedValuePath, (string)popShipto.DisplayMemberPath);

                //C20210305-000490
                if (!string.IsNullOrEmpty(ShipToID))
                {
                    DataTable dt = DataTableConverter.Convert(popShipto.ItemsSource);
                    if (dt != null && dt.Rows.Count > 0)
                    {
                        for (int inx = 0; inx < dt.Rows.Count; inx++)
                        {
                            if (dt.Rows[inx]["CBO_CODE"].ToString() == ShipToID)
                            {
                                popShipto.SelectedValue = ShipToID;
                                popShipto.SelectedText = dt.Rows[inx]["CBO_NAME"].ToString();
                            }
                        }
                    }
                }
            }
            else
            {
                popShipto.SelectedValue = null;
                popShipto.SelectedText = null;
            }
        }

        private void chkMultiShipTo_Click(object sender, RoutedEventArgs e)
        {
            popShipto.SelectedValue = null;
            popShipto.SelectedText = null;

            Util.gridClear(dgInPallet);
        }
    }
}
