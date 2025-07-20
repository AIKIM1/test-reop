/*************************************************************************************
 Created Date : 2020.04.22
      Creator : 이제섭
   Decription : CNJ 원형 21700 자동포장기 Pjt - 자동 포장 구성 (원/각형) - Outbox 일괄 추가 팝업
--------------------------------------------------------------------------------------
 [Change History]
  2020.04.22 이제섭 : 최초생성
  2022.05.19 장희만 : C20220509-000185 - MIX LINE 체크 추가
  2023.09.11  이병윤    : E20230704-000395 Differentiate functions for IM and non-IM according to OUTBOXID
  2024.04.19  이홍주    : SI               NFF 경우 OCV 검사결과 확인부분 제외함
**************************************************************************************/

using System;
using System.Windows;
using System.Windows.Input;
using System.Data;
using C1.WPF;
using C1.WPF.DataGrid;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.ControlsLibrary;
using System.Linq;
using C1.WPF.DataGrid.Summaries;
using System.Windows.Media;

namespace LGC.GMES.MES.FCS002
{

    public partial class FCS002_303_OUTBOX_MULTI : C1Window, IWorkArea
    {
        Util _util = new Util();

        string sUSERID = string.Empty;
        string _PalletID = string.Empty;
        string _TypeFlag = string.Empty;
        string _MultiShipToFlag = string.Empty;
        string _LotType = string.Empty;
        string _Editable = string.Empty;
        string _ProdId = string.Empty;
        string _ShipToId = string.Empty;
        string _AssyLotLineMixNo = string.Empty;
        DataTable _dtOutbox = null;

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
        public FCS002_303_OUTBOX_MULTI()
        {
            InitializeComponent();
        }

        private void C1Window_Loaded(object sender, RoutedEventArgs e)
        {
            object[] tmps = C1WindowExtension.GetParameters(this);
            _PalletID = Util.NVC(tmps[0]);
            sUSERID = Util.NVC(tmps[1]);
            _TypeFlag = Util.NVC(tmps[2]);
            _MultiShipToFlag = Util.NVC(tmps[3]);
            _LotType = Util.NVC(tmps[4]);
            _Editable = Util.NVC(tmps[5]);
            _ProdId = Util.NVC(tmps[6]);
            _ShipToId = Util.NVC(tmps[7]);
            _AssyLotLineMixNo = Util.NVC(tmps[8]);
            _dtOutbox = tmps[9] as DataTable;

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

            if (_MultiShipToFlag == "Y")
            {
                chkMultiShipTo.IsChecked = true;
            }
            else
            {
                chkMultiShipTo.IsChecked = false;
            }

            setShipToPopControl(_ProdId, _ShipToId);

            if (_LotType == "N" && _Editable == "Y")
            {
                chkMultiShipTo.IsEnabled = true;
                popShipto.IsEnabled = true;
            }
            else
            {
                chkMultiShipTo.IsEnabled = false;
                popShipto.IsEnabled = false;
            }
        }
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
                    if (Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "OCV_SPEC_RESULT")).Equals("OK"))
                    {
                        e.Cell.Presenter.Background = null;

                    }
                    else
                    {
                        //NFF는 OCV SPEC 체크를 안할수도 있어서 결과에 따른 색상 변경은 제외함.
                        e.Cell.Presenter.Background = null;
                        //e.Cell.Presenter.Background = new SolidColorBrush(Colors.OrangeRed);
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
                    //if (!Util.NVC(dgInPallet.GetCell(i, dgInPallet.Columns["OCV_SPEC_RESULT"].Index).Value).Equals("OK"))
                    //{
                    //    //OCV SPEC이 맞지 않아 포장이 불가능합니다.
                    //    Util.MessageValidation("SFU8227");
                    //    sOcvchk = false;
                    //    return;
                    //}

                    /*C20210906-000208 로 변경
                    //C20210305-000498 로 수정. INBOXID 7자리 : 테슬라 재활용 인박스, 8자리 : 테슬라 일회용 인박스. 혼입금지
                    int iInboxID_0_len = 0;
                    if (_iInboxIDlen == 0)
                    {
                        //파라미터로 받은 인박스ID 길이가 0 이면
                        iInboxID_0_len = dt.Rows[0]["INBOXID"].ToString().Trim().Length;  //0번째 INBOXID 길이 사용
                    }
                    else
                    {
                        //파라미터로 받은 인박스ID 길이가 0 이 아니면
                        iInboxID_0_len = _iInboxIDlen;  // 파라미터로 받은 길이 사용
                    }
                    int iInboxIDlen = dt.Rows[i]["INBOXID"].ToString().Trim().Length;   //i번째 INBOXID 길이
                    if (iInboxID_0_len != iInboxIDlen)
                    {
                        string sOutBoxID = dt.Rows[i]["OUTBOXID"].ToString();

                        Util.MessageValidation("SFU3776", sOutBoxID);  //유형이 다른 인박스는 하나의 팔레트에 혼입할 수 없습니다. [%1]
                        return;
                    }
                    */

                    //C20210906-000208
                    string sTypeCode = string.Empty;
                    if (string.IsNullOrEmpty(_TypeFlag))
                    {
                        //파라미터로 받은 TYPE_FLAG 가 없으면
                        sTypeCode = dt.Rows[0]["TYPE_FLAG"].ToString().Trim();  //0번째 TYPE_FLAG 사용
                    }
                    else
                    {
                        //파라미터로 받은 TYPE_FLAG 있으면
                        sTypeCode = _TypeFlag;  // 파라미터로 받은 TYPE_FLAG 사용
                    }
                    string sTypeCode_i = dt.Rows[i]["TYPE_FLAG"].ToString().Trim();   //i번째 TYPE_FLAG
                    if (sTypeCode_i != sTypeCode)
                    {
                        string sOutBoxID = dt.Rows[i]["OUTBOXID"].ToString();
                        Util.MessageValidation("SFU3806", sOutBoxID);  //유형이 다른 박스는 하나의 팔레트에 혼입할 수 없습니다. [%1]
                        return;
                    }
                }
            }

            if (sOcvchk == true)
            {
                bool bLineMix = false;
                string sPROD_LINE = string.Empty;

                if (_AssyLotLineMixNo == "0" || string.IsNullOrEmpty(_AssyLotLineMixNo))
                {
                    //원래 팔레트에 포함되어 있던 박스 PROD_LINE 확인하여 라인믹스여부 판단
                    if (_dtOutbox != null && _dtOutbox.Rows.Count > 0)
                    {
                        sPROD_LINE = _dtOutbox.Rows[0]["PROD_LINE"].ToString();
                        var query = (from t in _dtOutbox.AsEnumerable()
                                     where t.Field<string>("PROD_LINE") != sPROD_LINE
                                     select t).Distinct();

                        if (query.Any())
                        {
                            bLineMix = true;
                        }
                    }

                    //원래 팔레트에 포함되어 있던 박스 라인 믹스 아니면 추가되는 박스 PROD_LINE 확인하여 라인믹스여부 판단
                    if (bLineMix == false && dgInPallet != null && dgInPallet.Rows.Count > 0)
                    {
                        DataTable dtSource = DataTableConverter.Convert(dgInPallet.ItemsSource);

                        if (string.IsNullOrEmpty(sPROD_LINE))
                        {
                            sPROD_LINE = dtSource.Rows[0]["PROD_LINE"].ToString();
                        }

                        var query = (from t in dtSource.AsEnumerable()
                                     where t.Field<string>("PROD_LINE") != sPROD_LINE
                                     select t).Distinct();

                        if (query.Any())
                        {
                            bLineMix = true;
                        }
                    }
                }

                if (bLineMix)
                {
                    //Line이 혼합됩니다. 추가하시겠습니까?
                    Util.MessageConfirm("SFU3821", (result) =>
                    {
                        if (result == MessageBoxResult.OK)
                        {
                            RunStart();
                        }
                    });
                }
                else
                {
                    //추가하시겠습니까?
                    Util.MessageConfirm("SFU2965", (result) =>
                    {
                        if (result == MessageBoxResult.OK)
                        {
                            RunStart();
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
                    if (dr["CHK"].Equals(1))
                    {
                        dt.Rows.Remove(dr);
                    }
                }

                Util.GridSetData(dgInPallet, dt, FrameOperation, false);

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
                ValidationOutbox();
            }
        }
        #endregion

        #endregion

        #region [Method]

        #region Pallet 생성 : RunStart()
        private void RunStart()
        {
            if (dgInPallet.Rows.Count == 1)
            {
                //Outbox 정보가 없습니다
                Util.MessageValidation("SFU5010");
                return;
            }

            try
            {
                DataSet ds = new DataSet();
                DataTable dtIndata = ds.Tables.Add("INPALLET");
                dtIndata.Columns.Add("SRCTYPE");
                dtIndata.Columns.Add("LANGID");
                dtIndata.Columns.Add("BOXID");
                dtIndata.Columns.Add("USERID");
                dtIndata.Columns.Add("SHIPTO_ID");
                if (_LotType == "N" && _Editable == "Y")
                {
                    dtIndata.Columns.Add("MULTI_SHIPTO_FLAG");
                }

                DataTable dtInbox = ds.Tables.Add("INOUTBOX");
                dtInbox.Columns.Add("BOXID");

                DataRow dr = dtIndata.NewRow();
                dr["SRCTYPE"] = "UI";
                dr["LANGID"] = LoginInfo.LANGID;
                dr["BOXID"] = _PalletID;
                dr["USERID"] = sUSERID;
                dr["SHIPTO_ID"] = popShipto.SelectedValue.ToString().Trim();
                if (_LotType == "N" && _Editable == "Y")
                {
                    if (chkMultiShipTo.IsChecked == true)
                    {
                        dr["MULTI_SHIPTO_FLAG"] = "Y";
                    }
                    else
                    {
                        dr["MULTI_SHIPTO_FLAG"] = "N";
                    }
                }

                dtIndata.Rows.Add(dr);

                for (int i = 0; i < dgInPallet.GetRowCount(); i++)
                {
                    DataRow drS = dtInbox.NewRow();
                    drS["BOXID"] = Util.NVC(dgInPallet.GetCell(i, dgInPallet.Columns["OUTBOXID"].Index).Value);
                    dtInbox.Rows.Add(drS);
                }

                DataSet dsResult = new ClientProxy().ExecuteServiceSync_Multi("BR_PRD_REG_PACK_OUTBOX_MIX_MULTI_MB", "INPALLET,INOUTBOX", null, ds);

                PALLET_ID = _PalletID;

                this.DialogResult = MessageBoxResult.OK;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        #endregion

        #region 조회된 OUTBOX 바인드 : GetCompleteOutbox()
        private void GetCompleteOutbox(string BoxID, string pAssyLotLinemixFlag)
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
                //newRow["MULTI_SHIPTO_FLAG"] = _MultiShipToFlag;
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

                //int iInboxID_0_len = 0; //C20210305-000498 로 수정. INBOXID 7자리 : 테슬라 재활용 인박스, 8자리 : 테슬라 일회용 인박스. 혼입금지
                string sTypeFlag = string.Empty;    //C20210906-000208 로 변경

                if (dtResult != null && dtResult.Rows.Count > 0)
                {
                    DataTable dtSource = DataTableConverter.Convert(dgInPallet.ItemsSource);
                    var query = (from t in dtSource.AsEnumerable()
                                 where t.Field<string>("OUTBOXID") == txtInPalletID.Text
                                 select t).Distinct();
                    if (query.Any())
                    {
                        //	SFU1781	이미 추가 된 OUTBOX 입니다.
                        Util.MessageValidation("SFU5011");
                        return;
                    }

                    if (dtSource != null && dtSource.Rows.Count > 0)
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
                                if (!string.IsNullOrEmpty(dtResult.Rows[0]["MIX_LINE"].ToString()) || dtResult.Rows[0]["MIX_LINE"].ToString() != "0") //라인 믹스 적용 케이스인 경우
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
                                if (dtResult.Rows[0]["PROD_LINE"].ToString() != dtSource.Rows[i]["PROD_LINE"].ToString())
                                {
                                    //동일한 라인이 아닙니다.
                                    Util.MessageValidation("SFU4645");
                                    return;
                                }
                            }
                        }

                        if (string.IsNullOrEmpty(_TypeFlag))
                        {
                            //파라미터로 받은 타입이 없으면
                            sTypeFlag = dtSource.Rows[0]["TYPE_FLAG"].ToString().Trim();  //그리드 0번째 TYPE_FLAG 사용
                        }
                        else
                        {
                            //파라미터로 받은 타입이 있으면
                            sTypeFlag = _TypeFlag;  //파라미터로 받은 TYPE_FLAG 사용
                        }
                    }
                    else
                    {
                        if (string.IsNullOrEmpty(_TypeFlag))
                        {
                            //파라미터로 받은 타입이 없으면
                            sTypeFlag = dtResult.Rows[0]["TYPE_FLAG"].ToString().Trim();  //조회결과 TYPE_FLAG 사용
                        }
                        else
                        {
                            //파라미터로 받은 타입이 있으면
                            sTypeFlag = _TypeFlag;  //파라미터로 받은 TYPE_FLAG 사용
                        }
                    }

                    /*C20210906-000208 로 변경
                    //C20210305-000498 로 수정. INBOXID 7자리 : 테슬라 재활용 인박스, 8자리 : 테슬라 일회용 인박스. 혼입금지
                    if (dtResult.Rows[0]["INBOXID"].ToString().Trim().Length != iInboxID_0_len)
                    {
                        string sOutBoxID = dtResult.Rows[0]["OUTBOXID"].ToString();
                        //유형이 다른 인박스는 하나의 팔레트에 혼입할 수 없습니다. [%1]
                        Util.MessageValidation("SFU3776", sOutBoxID);
                        return;
                    }
                    */
                    //C20210906-000208
                    if (dtResult.Rows[0]["TYPE_FLAG"].ToString().Trim() != sTypeFlag)
                    {
                        string sOutBoxID = dtResult.Rows[0]["OUTBOXID"].ToString();
                        //유형이 다른 박스는 하나의 팔레트에 혼입할 수 없습니다. [%1]
                        Util.MessageValidation("SFU3806", sOutBoxID);
                        return;
                    }

                    dtResult.Merge(dtSource);
                    Util.GridSetData(dgInPallet, dtResult, FrameOperation, false);
                }

                // 최초 Outbox 바인딩 시, 출하처 콤보 Set
                if (dtResult.Rows.Count == 1)
                {
                    if (_LotType == "N" && _Editable == "Y")
                    {
                        setShipToPopControl(dtResult.Rows[0]["PRODID"].ToString(), dtResult.Rows[0]["SHIP_TO"].ToString());
                    }
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

        #region Outbox 투입 가능 여부 체크 : ValidationOutbox()
        private void ValidationOutbox()
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
                newRow["PALLETID"] = _PalletID;

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

                string sAssyLotLinemixFlag = "";

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

                    //GetCompleteOutbox(dsResult.Tables["OUTDATA"].Rows[0]["BOXID"].ToString());
                    DataRow[] dr = dsResult.Tables["OUTDATA"].Select();

                    object[] param = new object[] { (int)dr[0]["TOTAL_QTY"] };
                    if (sOutBox.Equals("IM"))
                    {
                        //if ((int)dr[0]["TOTAL_QTY"] < 256)
                        //{
                        //    // BOX 수량은 %1 입니다. 추가 하시겠습니까? 
                        //    Util.MessageConfirm("SFU8207", (msgresult) =>
                        //    {
                        //        if (msgresult == MessageBoxResult.OK)
                        //        {
                        //            GetCompleteOutbox(dsResult.Tables["OUTDATA"].Rows[0]["BOXID"].ToString(), sAssyLotLinemixFlag);
                        //        }
                        //        else
                        //        {
                        //            return;
                        //        }
                        //    }, param);
                        //}
                        //else if ((int)dr[0]["TOTAL_QTY"] == 256)
                        //{
                        //    GetCompleteOutbox(dsResult.Tables["OUTDATA"].Rows[0]["BOXID"].ToString(), sAssyLotLinemixFlag);
                        //}
                        GetCompleteOutbox(dsResult.Tables["OUTDATA"].Rows[0]["BOXID"].ToString(), sAssyLotLinemixFlag);
                    }
                    else
                    {
                        GetCompleteOutbox(dsResult.Tables["OUTDATA"].Rows[0]["BOXID"].ToString(), sAssyLotLinemixFlag);
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

        #endregion

        #endregion

        private void chkMultiShipTo_Click(object sender, RoutedEventArgs e)
        {
            if (chkMultiShipTo.IsEnabled == true)
            {
                popShipto.SelectedValue = null;
                popShipto.SelectedText = null;

                Util.gridClear(dgInPallet);
            }
        }

    }
}
