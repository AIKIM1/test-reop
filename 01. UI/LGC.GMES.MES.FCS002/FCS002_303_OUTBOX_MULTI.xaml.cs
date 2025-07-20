/*************************************************************************************
 Created Date : 2020.04.22
      Creator : ������
   Decription : CNJ ���� 21700 �ڵ������ Pjt - �ڵ� ���� ���� (��/����) - Outbox �ϰ� �߰� �˾�
--------------------------------------------------------------------------------------
 [Change History]
  2020.04.22 ������ : ���ʻ���
  2022.05.19 ���� : C20220509-000185 - MIX LINE üũ �߰�
  2023.09.11  �̺���    : E20230704-000395 Differentiate functions for IM and non-IM according to OUTBOXID
  2024.04.19  ��ȫ��    : SI               NFF ��� OCV �˻��� Ȯ�κκ� ������
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
        /// �޺��ڽ� �ʱ� ����
        /// </summary>
        private void InitCombo()
        {
            CommonCombo _combo = new CommonCombo();
        }

        /// <summary>
        /// ��Ʈ�� �ʱⰪ ����
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

        #region �۾� Pallet ����ǥ�� : dgPalletList_LoadedCellPresenter()

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
                //Grid Data Binding �̿��� Background �� ����
                if (e.Cell.Row.Type == C1.WPF.DataGrid.DataGridRowType.Item)
                {
                    if (Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "OCV_SPEC_RESULT")).Equals("OK"))
                    {
                        e.Cell.Presenter.Background = null;

                    }
                    else
                    {
                        //NFF�� OCV SPEC üũ�� ���Ҽ��� �־ ����� ���� ���� ������ ������.
                        e.Cell.Presenter.Background = null;
                        //e.Cell.Presenter.Background = new SolidColorBrush(Colors.OrangeRed);
                    }
                }
            }));
        }

        #endregion

        #region [EVENT]

        #region �ؽ�Ʈ�ڽ� ��Ŀ�� : text_GotFocus()
        private void text_GotFocus(object sender, RoutedEventArgs e)
        {
            InputMethod.Current.ImeConversionMode = ImeConversionModeValues.Alphanumeric;
        }
        #endregion

        #region Pallet ���� : btnSave_Click()
        private void btnCreate_Click(object sender, RoutedEventArgs e)
        {
            //OCV RESULT OK�ƴҽ� ���� ����
            bool sOcvchk = true;

            DataTable dt = DataTableConverter.Convert(dgInPallet.ItemsSource);
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                if (dt.Rows[i]["OUTBOXID"].ToString() != string.Empty)
                {
                    //NFF�� ��� OCV �˻��� Ȯ�κκ� ������. ���� MMD �����ڵ�� ��뿩�� ����� ��� �����ڵ�� ���� �ؾ���.(2024.04.18)
                    //if (!Util.NVC(dgInPallet.GetCell(i, dgInPallet.Columns["OCV_SPEC_RESULT"].Index).Value).Equals("OK"))
                    //{
                    //    //OCV SPEC�� ���� �ʾ� ������ �Ұ����մϴ�.
                    //    Util.MessageValidation("SFU8227");
                    //    sOcvchk = false;
                    //    return;
                    //}

                    /*C20210906-000208 �� ����
                    //C20210305-000498 �� ����. INBOXID 7�ڸ� : �׽��� ��Ȱ�� �ιڽ�, 8�ڸ� : �׽��� ��ȸ�� �ιڽ�. ȥ�Ա���
                    int iInboxID_0_len = 0;
                    if (_iInboxIDlen == 0)
                    {
                        //�Ķ���ͷ� ���� �ιڽ�ID ���̰� 0 �̸�
                        iInboxID_0_len = dt.Rows[0]["INBOXID"].ToString().Trim().Length;  //0��° INBOXID ���� ���
                    }
                    else
                    {
                        //�Ķ���ͷ� ���� �ιڽ�ID ���̰� 0 �� �ƴϸ�
                        iInboxID_0_len = _iInboxIDlen;  // �Ķ���ͷ� ���� ���� ���
                    }
                    int iInboxIDlen = dt.Rows[i]["INBOXID"].ToString().Trim().Length;   //i��° INBOXID ����
                    if (iInboxID_0_len != iInboxIDlen)
                    {
                        string sOutBoxID = dt.Rows[i]["OUTBOXID"].ToString();

                        Util.MessageValidation("SFU3776", sOutBoxID);  //������ �ٸ� �ιڽ��� �ϳ��� �ȷ�Ʈ�� ȥ���� �� �����ϴ�. [%1]
                        return;
                    }
                    */

                    //C20210906-000208
                    string sTypeCode = string.Empty;
                    if (string.IsNullOrEmpty(_TypeFlag))
                    {
                        //�Ķ���ͷ� ���� TYPE_FLAG �� ������
                        sTypeCode = dt.Rows[0]["TYPE_FLAG"].ToString().Trim();  //0��° TYPE_FLAG ���
                    }
                    else
                    {
                        //�Ķ���ͷ� ���� TYPE_FLAG ������
                        sTypeCode = _TypeFlag;  // �Ķ���ͷ� ���� TYPE_FLAG ���
                    }
                    string sTypeCode_i = dt.Rows[i]["TYPE_FLAG"].ToString().Trim();   //i��° TYPE_FLAG
                    if (sTypeCode_i != sTypeCode)
                    {
                        string sOutBoxID = dt.Rows[i]["OUTBOXID"].ToString();
                        Util.MessageValidation("SFU3806", sOutBoxID);  //������ �ٸ� �ڽ��� �ϳ��� �ȷ�Ʈ�� ȥ���� �� �����ϴ�. [%1]
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
                    //���� �ȷ�Ʈ�� ���ԵǾ� �ִ� �ڽ� PROD_LINE Ȯ���Ͽ� ���ιͽ����� �Ǵ�
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

                    //���� �ȷ�Ʈ�� ���ԵǾ� �ִ� �ڽ� ���� �ͽ� �ƴϸ� �߰��Ǵ� �ڽ� PROD_LINE Ȯ���Ͽ� ���ιͽ����� �Ǵ�
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
                    //Line�� ȥ�յ˴ϴ�. �߰��Ͻðڽ��ϱ�?
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
                    //�߰��Ͻðڽ��ϱ�?
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

        #region �ݱ� : btnClose_Click()
        /// <summary>
        /// �ݱ�
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = MessageBoxResult.Cancel;
        }


        #endregion

        #region ���� : btnInPalletDelete_Click()
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

        #region OutBox üũ : txtInPalletID_KeyDown()
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

        #region Pallet ���� : RunStart()
        private void RunStart()
        {
            if (dgInPallet.Rows.Count == 1)
            {
                //Outbox ������ �����ϴ�
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

        #region ��ȸ�� OUTBOX ���ε� : GetCompleteOutbox()
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

                //int iInboxID_0_len = 0; //C20210305-000498 �� ����. INBOXID 7�ڸ� : �׽��� ��Ȱ�� �ιڽ�, 8�ڸ� : �׽��� ��ȸ�� �ιڽ�. ȥ�Ա���
                string sTypeFlag = string.Empty;    //C20210906-000208 �� ����

                if (dtResult != null && dtResult.Rows.Count > 0)
                {
                    DataTable dtSource = DataTableConverter.Convert(dgInPallet.ItemsSource);
                    var query = (from t in dtSource.AsEnumerable()
                                 where t.Field<string>("OUTBOXID") == txtInPalletID.Text
                                 select t).Distinct();
                    if (query.Any())
                    {
                        //	SFU1781	�̹� �߰� �� OUTBOX �Դϴ�.
                        Util.MessageValidation("SFU5011");
                        return;
                    }

                    if (dtSource != null && dtSource.Rows.Count > 0)
                    {
                        for (int i = 0; i < dtSource.Rows.Count; i++)
                        {
                            if (dtResult.Rows[0]["PRODID"].ToString() != dtSource.Rows[i]["PRODID"].ToString())
                            {
                                //���� ��ǰ�� �ƴմϴ�
                                Util.MessageValidation("SFU1502");
                                return;
                            }
                            if (dtResult.Rows[0]["EXP_DOM_TYPE_CODE"].ToString() != dtSource.Rows[i]["EXP_DOM_TYPE_CODE"].ToString())
                            {
                                //������ ���������� �ƴմϴ�.
                                Util.MessageValidation("SFU4271");
                                return;
                            }
                            if (dtResult.Rows[0]["LOTTYPE"].ToString() != dtSource.Rows[i]["LOTTYPE"].ToString())
                            {
                                //���� LOT ������ �ƴմϴ�.
                                Util.MessageValidation("SFU4513");
                                return;
                            }
                            if (dtResult.Rows[0]["PROD_MONTH"].ToString() != dtSource.Rows[i]["PROD_MONTH"].ToString())
                            {
                                //������ ������� �ƴմϴ�.
                                Util.MessageValidation("SFU4644");
                                return;
                            }

                            //���� �ͽ� ���� �� ȥ�յ� OUTBOX���� Ȯ���Ѵ�. ȥ��:MIX����, ȥ�վƴϸ� ���� ���� ó�� -  2022.05.12
                            //if (dtResult.Rows[0]["PROD_LINE"].ToString() != dtSource.Rows[i]["PROD_LINE"].ToString())
                            if (pAssyLotLinemixFlag == "Y")
                            {
                                if (!string.IsNullOrEmpty(dtResult.Rows[0]["MIX_LINE"].ToString()) || dtResult.Rows[0]["MIX_LINE"].ToString() != "0") //���� �ͽ� ���� ���̽��� ���
                                {
                                    if (dtResult.Rows[0]["MIX_LINE"].ToString() != dtSource.Rows[i]["MIX_LINE"].ToString())
                                    {
                                        //������ MIX LINE�� �ƴմϴ�.
                                        Util.MessageValidation("SFU4968");
                                        return;
                                    }
                                }
                                else //���������� ����� �ȵǾ� �ִ� ��� �Ǵ� /LINE MIX �ƴѰ�� ���� Logic�� Ż �� �ֵ���(MMD �����ڵ� ���: TESLA_LINE_MIX_NJ)
                                {
                                    if (dtResult.Rows[0]["PROD_LINE"].ToString() != dtSource.Rows[i]["PROD_LINE"].ToString())
                                    {
                                        //������ ������ �ƴմϴ�.
                                        Util.MessageValidation("SFU4645");
                                        return;
                                    }
                                }
                            }
                            else //LINE MIX �ƴѰ�� ������ �����ϰ� ó��
                            {
                                if (dtResult.Rows[0]["PROD_LINE"].ToString() != dtSource.Rows[i]["PROD_LINE"].ToString())
                                {
                                    //������ ������ �ƴմϴ�.
                                    Util.MessageValidation("SFU4645");
                                    return;
                                }
                            }
                        }

                        if (string.IsNullOrEmpty(_TypeFlag))
                        {
                            //�Ķ���ͷ� ���� Ÿ���� ������
                            sTypeFlag = dtSource.Rows[0]["TYPE_FLAG"].ToString().Trim();  //�׸��� 0��° TYPE_FLAG ���
                        }
                        else
                        {
                            //�Ķ���ͷ� ���� Ÿ���� ������
                            sTypeFlag = _TypeFlag;  //�Ķ���ͷ� ���� TYPE_FLAG ���
                        }
                    }
                    else
                    {
                        if (string.IsNullOrEmpty(_TypeFlag))
                        {
                            //�Ķ���ͷ� ���� Ÿ���� ������
                            sTypeFlag = dtResult.Rows[0]["TYPE_FLAG"].ToString().Trim();  //��ȸ��� TYPE_FLAG ���
                        }
                        else
                        {
                            //�Ķ���ͷ� ���� Ÿ���� ������
                            sTypeFlag = _TypeFlag;  //�Ķ���ͷ� ���� TYPE_FLAG ���
                        }
                    }

                    /*C20210906-000208 �� ����
                    //C20210305-000498 �� ����. INBOXID 7�ڸ� : �׽��� ��Ȱ�� �ιڽ�, 8�ڸ� : �׽��� ��ȸ�� �ιڽ�. ȥ�Ա���
                    if (dtResult.Rows[0]["INBOXID"].ToString().Trim().Length != iInboxID_0_len)
                    {
                        string sOutBoxID = dtResult.Rows[0]["OUTBOXID"].ToString();
                        //������ �ٸ� �ιڽ��� �ϳ��� �ȷ�Ʈ�� ȥ���� �� �����ϴ�. [%1]
                        Util.MessageValidation("SFU3776", sOutBoxID);
                        return;
                    }
                    */
                    //C20210906-000208
                    if (dtResult.Rows[0]["TYPE_FLAG"].ToString().Trim() != sTypeFlag)
                    {
                        string sOutBoxID = dtResult.Rows[0]["OUTBOXID"].ToString();
                        //������ �ٸ� �ڽ��� �ϳ��� �ȷ�Ʈ�� ȥ���� �� �����ϴ�. [%1]
                        Util.MessageValidation("SFU3806", sOutBoxID);
                        return;
                    }

                    dtResult.Merge(dtSource);
                    Util.GridSetData(dgInPallet, dtResult, FrameOperation, false);
                }

                // ���� Outbox ���ε� ��, ����ó �޺� Set
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

        #region Outbox ���� ���� ���� üũ : ValidationOutbox()
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
                //��IM��27 bit��Non_IM��12 bit)
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
                        //    // BOX ������ %1 �Դϴ�. �߰� �Ͻðڽ��ϱ�? 
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
