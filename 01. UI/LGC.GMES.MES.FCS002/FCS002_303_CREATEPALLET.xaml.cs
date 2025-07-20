/*************************************************************************************
 Created Date : 2018.08.09
      Creator : ��ȭ��K
   Decription : Pallet ���� �˾�
--------------------------------------------------------------------------------------
 [Change History]
 2022.05.12  C20220509-000185 ����MIX ����
 2023.08.23  E20230704-000395 Differentiate functions for IM and non-IM according to OUTBOXID  \
 2024.04.19  ��ȫ��    : SI               NFF ��� OCV �˻��� Ȯ�κκ� ������
 2025.05.15  ��ȫ��    : SI               MES2.0 ����
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
    /// BOX001_201_RUNSTART.xaml�� ���� ��ȣ �ۿ� ��
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
        }
        #endregion

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
                    //(�����ڵ� CNJ_AUTOBOX_FCS_OCV_CHK �� ��ϵǾ����.)

                    //if (!Util.NVC(dgInPallet.GetCell(i, dgInPallet.Columns["OCV_SPEC_RESULT"].Index).Value).Equals("OK"))
                    //{
                    //    //OCV SPEC�� ���� �ʾ� ������ �Ұ����մϴ�.
                    //    Util.MessageValidation("SFU8227");
                    //    sOcvchk = false;
                    //    return;
                    //}
                    //CNSSL19C,CNSSL20C,CNSSL21C

                    /*C20210906-000208 �� ����
                    //C20210305-000498 �� ����. INBOXID 7�ڸ� : �׽��� ��Ȱ�� �ιڽ�, 8�ڸ� : �׽��� ��ȸ�� �ιڽ�. ȥ�Ա���
                    int iInboxID_0_len = dt.Rows[0]["INBOXID"].ToString().Trim().Length;  //0��° INBOXID ����
                    int iInboxIDlen = dt.Rows[i]["INBOXID"].ToString().Trim().Length;   //i��° INBOXID ����
                    if (iInboxID_0_len != iInboxIDlen)
                    {
                        string sOutBoxID = dt.Rows[i]["OUTBOXID"].ToString();

                        Util.MessageValidation("SFU3776", sOutBoxID);  //������ �ٸ� �ιڽ��� �ϳ��� �ȷ�Ʈ�� ȥ���� �� �����ϴ�. [%1]
                        return;
                    }
                    */

                    //C20210906-000208
                    string sTypeFlag0 = dt.Rows[0]["TYPE_FLAG"].ToString().Trim();  //0��° TYPE_FLAG
                    string sTypeFlagi = dt.Rows[i]["TYPE_FLAG"].ToString().Trim();  //i��° TYPE_FLAG
                    if (sTypeFlag0 != sTypeFlagi)
                    {
                        string sOutBoxID = dt.Rows[i]["OUTBOXID"].ToString();

                        Util.MessageValidation("SFU3806", sOutBoxID);  //������ �ٸ� �ڽ��� �ϳ��� �ȷ�Ʈ�� ȥ���� �� �����ϴ�. [%1]
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
                    //Line�� ȥ�յǾ� �ֽ��ϴ�. Pallet �����Ͻðڽ��ϱ�?
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
                    //Pallet �����Ͻðڽ��ϱ�?
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

        #region OutBox üũ : txtInPalletID_KeyDown()
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

                    //setShipToPopControl("MCCM348015A1-D14", "CNH016502");

                    string sAssyLotLinemixFlag = "";

                    //OUTBOX üũ
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
                            //    // BOX ������ %1 �Դϴ�. �߰� �Ͻðڽ��ϱ�? 
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

        #region Pallet ���� : RunStart()
        private void RunStart(string pAssyLotLinemixFlag)
        {
            if (dgInPallet.Rows.Count == 1)
            {
                //Outbox ������ �����ϴ�
                Util.MessageValidation("SFU5010");
                return;
            }

            if (String.IsNullOrWhiteSpace(Util.NVC(popShipto.SelectedValue).Trim()))
            {
                //����ó�� �����ϼ���.
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

        #region ��ȸ�� OUTBOX ���ε� : GetCompleteOutbox()
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
                        //	SFU1781	�̹� �߰� �� OUTBOX �Դϴ�.
                        Util.MessageValidation("SFU5011");
                        return;
                    }
                    // ����ó ���ù�� �и�ó��_IM:��������, Non_IM:.������ ��ϵ� ����ó ����
                    if (OutBox.Equals("IM"))
                    {
                        // ���� Outbox ���ε� ��, ����ó �޺� Set
                        if (dtResult.Rows.Count == 1)
                        {
                            //setShipToPopControl(dtResult.Rows[0]["PRODID"].ToString());

                            /*C20210906-000208 �� ����
                            //C20210305-000490 : INBOXID 7�ڸ� : �׽��� ��Ȱ���� ��� CNH020441 , 8�ڸ� : �׽��� ��ȸ���� ��� CNH016502
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
                        // ���� Outbox ���ε� ��, ����ó �޺� Set
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

                                if (!string.IsNullOrEmpty(dtResult.Rows[0]["MIX_LINE"].ToString()) || dtResult.Rows[0]["MIX_LINE"].ToString() != "0")   //���� �ͽ� ���� ���̽��� ���
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
                                {
                                    if (dtResult.Rows[0]["PROD_LINE"].ToString() != dtSource.Rows[i]["PROD_LINE"].ToString())
                                    {
                                        //������ ������ �ƴմϴ�.
                                        Util.MessageValidation("SFU4645");
                                        return;
                                    }
                                }
                            }

                            /* C20210906-000208 �� ����
                            //C20210305-000498 �� ����. INBOXID 7�ڸ� : �׽��� ��Ȱ�� �ιڽ�, 8�ڸ� : �׽��� ��ȸ�� �ιڽ�. ȥ�Ա���
                            if (dtResult.Rows[0]["INBOXID"].ToString().Trim().Length != dtSource.Rows[i]["INBOXID"].ToString().Trim().Length)
                            {
                                string sOutBoxID = dtResult.Rows[0]["OUTBOXID"].ToString();
                                //������ �ٸ� �ιڽ��� �ϳ��� �ȷ�Ʈ�� ȥ���� �� �����ϴ�. [%1]
                                Util.MessageValidation("SFU3776", sOutBoxID);
                                return;
                            }
                            */
                            //C20210906-000208
                            if (dtResult.Rows[0]["TYPE_FLAG"].ToString().Trim() != dtSource.Rows[i]["TYPE_FLAG"].ToString().Trim())
                            {
                                string sOutBoxID = dtResult.Rows[0]["OUTBOXID"].ToString();
                                //������ �ٸ� �ڽ��� �ϳ��� �ȷ�Ʈ�� ȥ���� �� �����ϴ�. [%1]
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
