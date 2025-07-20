/*************************************************************************************
 Created Date : 2020-10-17
      Creator : ����
   Decription : �ڵ����� Cell Pallet �籸��
--------------------------------------------------------------------------------------
 [Change History]
    Date         Author      CSR         Description...
  2020.10.17 Create          SI          Initial Created.
  2021.03.05 ����          SI          ��ǰ���� �÷��߰������� ���Ͷ� �߰�(Pallet Cell��ü, ������� ������Ͷ�)
**************************************************************************************/

using System;
using System.Windows.Controls;
using C1.WPF;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.CMM001.Class;
using System.Windows.Input;
using System.Data;
using LGC.GMES.MES.ControlsLibrary;
using System.Windows;
using C1.WPF.DataGrid;
using System.Windows.Threading;
using System.Windows.Media;
using System.Collections.Generic;

namespace LGC.GMES.MES.PACK001
{
    public partial class PACK003_007 : UserControl, IWorkArea
    {
        LGC.GMES.MES.PACK001.Class.MESSAGE_PARAM ms = new LGC.GMES.MES.PACK001.Class.MESSAGE_PARAM();
        private DataTable isCreateTable = new DataTable();

        #region [ Initialize ] 
        public IFrameOperation FrameOperation
        {
            get;
            set;
        }

        public PACK003_007()
        {
            InitializeComponent();
            //Initialize();
        }

        //private void Initialize()
        //{
        //}
        //�޺�ó��
        private void InitCombo()
        {
            CommonCombo _combo = new CommonCombo();

            C1ComboBox cboSHOPID = new C1ComboBox();
            cboSHOPID.SelectedValue = LoginInfo.CFG_SHOP_ID;
            C1ComboBox cboAREA_TYPE_CODE = new C1ComboBox();
            cboAREA_TYPE_CODE.SelectedValue = Area_Type.PACK;
            C1ComboBox cboEquipmentSegment = new C1ComboBox();
            cboEquipmentSegment.SelectedValue = null;
            C1ComboBox cboPrdtClass = new C1ComboBox();
            cboPrdtClass.SelectedValue = "";

        }
        #endregion

        #region [ Global variable ] 
        private void ShowLoadingIndicator()
        {
            if (loadingIndicator != null)
            {
                loadingIndicator.Visibility = Visibility.Visible;
            }
        }

        private void HiddenLoadingIndicator()
        {
            if (loadingIndicator != null)
            {
                loadingIndicator.Visibility = Visibility.Collapsed;
            }
        }

        private void DoEvents()
        {
            Application.Current.Dispatcher.Invoke(DispatcherPriority.Background, new System.Threading.ThreadStart(delegate { }));
        }
        #endregion

        #region [ Event ] 

        private void UserControl_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {
            InitCombo();
            //���� �ʱⰪ ����
            dtpDateFrom.SelectedDateTime = (DateTime)System.DateTime.Now.AddDays(-7); //������ �� ��¥
            dtpDateTo.SelectedDateTime = (DateTime)System.DateTime.Now;//���� ��¥

            List<Button> listAuth = new List<Button>();
            listAuth.Add(btnPack);
            listAuth.Add(btnChgcel);
            listAuth.Add(btnPltLabel);
            listAuth.Add(btnPacCancel);
            Util.pageAuth(listAuth, FrameOperation.AUTHORITY);
        }

        #endregion
        /// <summary>
        /// Carrier Mapping ���� �� Pallet ID ����
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void txtCarid_KeyDown(object sender, KeyEventArgs e)
        {
            try
            {
                if (dgPltcell.SelectedIndex > 0)
                {
                    return;
                }
                if (e.Key == Key.Enter)
                {
                    //string sInput_Flag = "";
                    //if ((bool)rdoReturn.IsChecked) sInput_Flag = "RETURN";
                    //if ((bool)rdoStock.IsChecked) sInput_Flag = "STOCK";

                    DataSet dsInput = new DataSet();

                    DataTable INDATA = new DataTable();
                    INDATA.TableName = "INDATA";
                    INDATA.Columns.Add("SRCTYPE", typeof(string));
                    INDATA.Columns.Add("LANGID", typeof(string));
                    INDATA.Columns.Add("CSTID", typeof(string));
                    INDATA.Columns.Add("RETURN_ID", typeof(string));
                    INDATA.Columns.Add("LOTID", typeof(string));
                    INDATA.Columns.Add("USERID", typeof(string));
                    //INDATA.Columns.Add("INPUT_ID_FLAG", typeof(string));

                    DataRow dr = INDATA.NewRow();
                    dr["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                    dr["LANGID"] = LoginInfo.LANGID;
                    dr["CSTID"] = txtCarid.Text.ToString();
                    dr["RETURN_ID"] = null; //��ǰ��ȣ
                    dr["LOTID"] = null;
                    dr["USERID"] = LoginInfo.USERID;
                    //dr["INPUT_ID_FLAG"] = sInput_Flag;
                    INDATA.Rows.Add(dr);

                    dsInput.Tables.Add(INDATA);

                    new ClientProxy().ExecuteService_Multi("BR_ACT_REG_LOGIS_RETURN_PRODUCT", "INDATA", "OUTDATA", (dsResult, dataException) =>
                    {
                        try
                        {
                            DataTable dt = DataTableConverter.Convert(dgPltcell.ItemsSource);

                            if (dataException != null)
                            {
                                Util.MessageException(dataException);
                                txtCarid.Text = "";
                                txtReason.Text = "";
                                txtCstid.Text = "";
                                txtPalletid.Text = "";
                                txtCellprodid.Text = "";
                                txtRcviss.Text = "";
                                Util.gridClear(dgPltcell);
                                return;
                            }
                            else
                            {
                                if (dsResult == null || dsResult.Tables["OUTDATA"].Rows.Count == 0)
                                {
                                    Util.MessageInfo("SFU1801"); //�Է� �����Ͱ� �������� �ʽ��ϴ�.
                                    txtCarid.Text = "";
                                    txtReason.Text = "";
                                    txtCstid.Text = "";
                                    txtPalletid.Text = "";
                                    txtCellprodid.Text = "";
                                    txtRcviss.Text = "";
                                    Util.gridClear(dgPltcell);
                                    return;
                                }
                                else
                                {
                                    if (!string.IsNullOrEmpty(txtCstid.Text.ToString()) && txtCstid.Text.ToString() != Util.NVC(dsResult.Tables["OUTDATA"].Rows[0]["CSTID"]))
                                    {
                                        LGC.GMES.MES.ControlsLibrary.MessageBox.Show(ms.AlertRetun("SFU5166"), null, "Information", MessageBoxButton.OKCancel, MessageBoxIcon.None, (result) =>
                                        //CSTID�� �����Ͻðڽ��ϱ�?
                                        {
                                            if (result == MessageBoxResult.OK)
                                            {
                                                txtCstid.Text = Util.NVC(dsResult.Tables["OUTDATA"].Rows[0]["CSTID"]);
                                                txtCarid.Text = "";
                                                txtReason.Text = "";
                                                txtPalletid.Text = Util.NVC(dsResult.Tables["OUTDATA"].Rows[0]["RETURN_ID"]);
                                                txtCellprodid.Text = "";
                                                txtRcviss.Text = "";
                                                Util.gridClear(dgPltcell);
                                            }
                                        }
            );
                                    }
                                    else
                                    {
                                        txtCstid.Text = Util.NVC(dsResult.Tables["OUTDATA"].Rows[0]["CSTID"]);
                                        txtCarid.Text = "";
                                        txtReason.Text = "";
                                        txtPalletid.Text = Util.NVC(dsResult.Tables["OUTDATA"].Rows[0]["RETURN_ID"]);
                                        txtCellprodid.Text = "";
                                        txtRcviss.Text = "";
                                        Util.gridClear(dgPltcell);

                                    }
                                    return;
                                }

                            }
                        }
                        catch (Exception ex)
                        {
                            throw ex;
                        }
                    }, dsInput);
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        //Cell �Է� ��
        private void txtCellId_KeyDown(object sender, KeyEventArgs e)
        {
            try
            {
                if (e.Key == Key.Enter)
                {
                    if (!string.IsNullOrEmpty(txtCstid.Text.ToString()))
                    {
                        //string sInput_Flag = "";
                        //if ((bool)rdoReturn.IsChecked) sInput_Flag = "RETURN";
                        //if ((bool)rdoStock.IsChecked) sInput_Flag = "STOCK";

                        DataTable RQSTDT = new DataTable();
                        RQSTDT.TableName = "RQSTDT";
                        RQSTDT.Columns.Add("LANGID", typeof(string));
                        RQSTDT.Columns.Add("LOTID", typeof(string));
                        RQSTDT.Columns.Add("EXIST_FLAG", typeof(string));
                        DataRow dr = RQSTDT.NewRow();
                        dr["LANGID"] = LoginInfo.LANGID;
                        dr["LOTID"] = txtCellId.Text.ToString();
                        dr["EXIST_FLAG"] = "Y";

                        RQSTDT.Rows.Add(dr);
                        DataTable dt = new ClientProxy().ExecuteServiceSync("DA_PRD_CHK_LOGIS_INPUTCELL_PLT", "RQSTDT", "RSLTDT", RQSTDT);
                        if (dt.Rows.Count > 0)
                        {
                            DataTable dtBe = DataTableConverter.Convert(dgPltcell.ItemsSource);
                            if (Util.NVC(dt.Rows[0]["WIPHOLD"]) == "Y")
                            {
                                Util.MessageInfo("SFU1340"); //HOLD �� LOT ID �Դϴ�.
                                txtCellId.Text = "";
                                return;
                            }
                            if (rdoReturn.IsChecked == true)
                            {
                                if (!string.IsNullOrEmpty(txtCellprodid.Text) && dgPltcell.Rows.Count != 0)
                                {
                                    if (txtCellprodid.Text.ToString() != Util.NVC(dt.Rows[0]["PRODID"]))
                                    {
                                        Util.MessageInfo("SFU4338"); //������ ��ǰ�� �۾� �����մϴ�.
                                        txtCellId.Text = "";
                                        return;
                                    }
                                }

                            }

                            for (int i = 0; i < dtBe.Rows.Count; i++)
                            {
                                if (Util.NVC(dt.Rows[0]["LOTID"]) == Util.NVC(dtBe.Rows[i]["LOTID"]))
                                {
                                    Util.MessageInfo("SFU1508"); //������ LOT ID�� �ֽ��ϴ�.
                                    txtCellId.Text = "";
                                    return;
                                }
                                if (rdoReturn.IsChecked == true)
                                {
                                    if (Util.NVC(dt.Rows[0]["S40"]) != Util.NVC(dtBe.Rows[i]["S40"]))
                                    {
                                        Util.MessageInfo("SFU8301"); //Cell ���굿�� �������� �ʽ��ϴ�. 
                                        txtCellId.Text = "";
                                        return;
                                    }
                                }
                            }

                            if (dgPltcell.Rows.Count > 1)
                            {
                                txtCellId.Text = "";
                                DataTable dtBefore = DataTableConverter.Convert(dgPltcell.ItemsSource);

                                dtBefore.Merge(dt);
                                dgPltcell.ItemsSource = DataTableConverter.Convert(dtBefore);
                                Util.SetTextBlockText_DataGridRowCount(txLeftRowCnt, Util.NVC(dtBefore.Rows.Count));
                                return;
                            }
                            else
                            {
                                txtCellId.Text = "";
                                txtCellprodid.Text = Util.NVC(dt.Rows[0]["PRODID"]);
                                txtRcviss.Tag = Util.NVC(dt.Rows[0]["FROM_SLOC_ID"]);
                                txtRcviss.Text = Util.NVC(dt.Rows[0]["FROM_SLOC_NAME"]);
                                dgPltcell.ItemsSource = DataTableConverter.Convert(dt);
                                Util.SetTextBlockText_DataGridRowCount(txLeftRowCnt, Util.NVC(dt.Rows.Count));
                                return;
                            }
                        }
                        if (dt.Rows.Count == 0)
                        {
                            Util.MessageInfo("SFU5167"); //�籸�� ������ CELLID�� �ƴմϴ�.
                            txtCellId.Text = "";
                            return;
                        }

                    }
                    else
                    {
                        Util.MessageInfo("SFU7006"); //Carrier ID�� �Է��ϼ���.
                        return;
                    }
                }
               
                
            }
            catch(Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void btnSelectCacel_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                DataTable dtTempTagetList = DataTableConverter.Convert(dgPltcell.ItemsSource);

                for (int i = (dtTempTagetList.Rows.Count - 1); i >= 0; i--)
                {
                    if (Util.NVC(dtTempTagetList.Rows[i]["CHK"]) == "True" ||
                        Util.NVC(dtTempTagetList.Rows[i]["CHK"]) == "1")
                    {

                        dtTempTagetList.Rows[i].Delete();
                        dtTempTagetList.AcceptChanges();
                    }
                }
                dgPltcell.ItemsSource = DataTableConverter.Convert(dtTempTagetList);
                Util.SetTextBlockText_DataGridRowCount(txLeftRowCnt, Util.NVC(dtTempTagetList.Rows.Count));

                if (!(dtTempTagetList.Rows.Count > 0))
                {
                    dgPltcell.ItemsSource = null;
                    //Refresh();
                }

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void btncancel_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!string.IsNullOrEmpty(txtPalletid.Text))
                {
                    LGC.GMES.MES.ControlsLibrary.MessageBox.Show(ms.AlertRetun("SFU1885"), null, "Information", MessageBoxButton.OKCancel, MessageBoxIcon.None, (result) =>
                    //��ü ��� �Ͻðڽ��ϱ�?
                    {
                        if (result == MessageBoxResult.OK)
                        {
                            Refresh();
                        }
                    }
            );
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        //�׸��� ��ü�ʱ�ȭ
        private void Refresh()
        {
            try
            {
                //�׸��� clear
                Util.gridClear(dgPltcell);
                Util.SetTextBlockText_DataGridRowCount(txLeftRowCnt, "0");

                txtCarid.Text = string.Empty;
                txtCellId.Text = string.Empty;
                txtReason.Text = string.Empty;

                txtCstid.Text = string.Empty;
                txtCstid.Tag = null;
                txtPalletid.Text = string.Empty;
                txtPalletid.Tag = null;

                txtCellprodid.Text = string.Empty;
                txtCellprodid.Tag = null;
                txtRcviss.Text = string.Empty;
                txtRcviss.Tag = null;


            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void btnExcel_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                new ExcelExporter().Export(dgBoxhistory);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        //�̷���ȸ
        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                //Util.gridClear(dgBoxhistory);
                txtPLTH.Text = "";
                //if (!dtDateCompare())
                //{
                //    return;
                //}
                search();


            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        //���� Validation
        private Boolean dtDateCompare()
        {
            try
            {
                TimeSpan timeSpan = dtpDateTo.SelectedDateTime.Date - dtpDateFrom.SelectedDateTime.Date;

                if (timeSpan.Days < 0)
                {
                    //��ȸ �������ڴ� �������ڸ� �ʰ� �� �� �����ϴ�.
                    Util.MessageValidation("SFU3569");
                    return false;
                }

                if (timeSpan.Days > 30)
                {
                    dtpDateTo.SelectedDateTime = dtpDateFrom.SelectedDateTime.AddDays(+30);
                    Util.MessageValidation("SFU4466");
                    return false;

                }

                return true;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return false;
            }
        }
        //������ �̷���ȸ
        private void search()
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("SHOPID", typeof(string));
                RQSTDT.Columns.Add("FROMDATE", typeof(string));
                RQSTDT.Columns.Add("TODATE", typeof(string));
                RQSTDT.Columns.Add("BOXID", typeof(string));
                RQSTDT.Columns.Add("UNPACKYN", typeof(string));

                //if(chkPackYn)

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["SHOPID"] = LoginInfo.CFG_SHOP_ID;
                dr["FROMDATE"] = dtpDateFrom.SelectedDateTime.ToString("yyyyMMdd");
                dr["TODATE"] = dtpDateTo.SelectedDateTime.ToString("yyyyMMdd");
                dr["BOXID"] = txtPltid.Text.ToString() == "" ? null : txtPltid.Text.ToString();
                dr["UNPACKYN"] = chkPackYn.IsChecked.Equals(true) ? "Y" : null;


                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SET_LOGIS_PLTHISTORY_SEARCH", "RQSTDT", "RSLTDT", RQSTDT);

                dgBoxhistory.ItemsSource = null;
                txtPltid.Text = "";
                if (dtResult == null || dtResult.Rows.Count <= 0)
                {
                    Util.SetTextBlockText_DataGridRowCount(txRightRowCnt, "0");
                    Util.Alert("101471");  // ��ȸ�� ����� �����ϴ�.

                    return;
                }
                if (dtResult.Rows.Count != 0)
                {
                    Util.GridSetData(dgBoxhistory, dtResult, FrameOperation);
                    Util.SetTextBlockText_DataGridRowCount(txRightRowCnt, Util.NVC(dtResult.Rows.Count));
                    txtPltid.Text = "";
                }
                
            }
            catch(Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        //�̷�Grid ����Ŭ�� �� POPUP ó��

        private void chkHeaderAllList_Checked(object sender, RoutedEventArgs e)
        {
            foreach (C1.WPF.DataGrid.DataGridRow row in dgBoxhistory.Rows)
            {
                DataTableConverter.SetValue(row.DataItem, "CHK", true);
            }

            dgBoxhistory.EndEdit();
            dgBoxhistory.EndEditRow(true);
        }

        private void chkHeaderAllList_Unchecked(object sender, RoutedEventArgs e)
        {
            foreach (C1.WPF.DataGrid.DataGridRow row in dgBoxhistory.Rows)
            {
                DataTableConverter.SetValue(row.DataItem, "CHK", false);
            }
            dgBoxhistory.EndEdit();
            dgBoxhistory.EndEditRow(true);
        }

        private void dgBoxhistory_LoadedCellPresenter(object sender, DataGridCellEventArgs e)
        {
            try
            {
                C1DataGrid dataGrid = (sender as C1DataGrid);

                Action act = () =>
                {
                    if( e == null)
                    {
                        return;
                    }

                    if (e.Cell.Presenter == null)
                    {
                        return;
                    }
                    //���� ����
                    if (e.Cell.Column.Name.Equals("BOXID"))
                    {

                        dgBoxhistory.GetCell(e.Cell.Row.Index, e.Cell.Column.Index).Presenter.Foreground = new SolidColorBrush(Colors.Red);
                        dgBoxhistory.GetCell(e.Cell.Row.Index, e.Cell.Column.Index).Presenter.FontWeight = FontWeights.Bold;
                        return;

                    }
                };

                dataGrid.Dispatcher.BeginInvoke(act);

            }
            catch(Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        //������
        private void btnPack_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!Validation()) return;
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(ms.AlertRetun("SFU4615"), null, "Information", MessageBoxButton.OKCancel, MessageBoxIcon.None, (result) =>
                {
                    if (result == MessageBoxResult.OK)
                    {
                        Save();
                        DoEvents();
                        //btnSearch_Click(null, null);
                    }
                });
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        //�����Ͽ� ���� ó��
        private void Save()
        {
            ShowLoadingIndicator();
            DoEvents();

            try
            {
                //BOX ���̺� INBOX_TYPE_CODE ����
                string inTypeBox = "";

                if (rdoReturn.IsChecked == true)
                {
                    for (int i = 0; i < dgPltcell.Rows.Count - 1; i ++ )
                    {
                       string Cprodid = DataTableConverter.GetValue(dgPltcell.Rows[i].DataItem, "PRODID").ToString();
                        if (txtCellprodid.Text != Cprodid)
                        {
                            Util.MessageInfo("SFU4338");//������ ��ǰ�� �۾� �����մϴ�.
                            return;
                        }
                    }
                    inTypeBox = "RTN";
                }
                
                if (rdoStock.IsChecked == true) inTypeBox = "WIP";

                isCreateTable = DataTableConverter.Convert(dgPltcell.GetCurrentItems());
                if (!CommonVerify.HasDataGridRow(dgPltcell)) return;

                this.dgPltcell.EndEdit();
                this.dgPltcell.EndEditRow(true);

                int irow = 0;

                DataSet indataSet = new DataSet();

                DataTable INDATA = new DataTable();
                INDATA.TableName = "INDATA";
                INDATA.Columns.Add("LANGID", typeof(string));
                INDATA.Columns.Add("SRCTYPE", typeof(string));
                INDATA.Columns.Add("AREAID", typeof(string));
                INDATA.Columns.Add("CSTID", typeof(string));
                INDATA.Columns.Add("PLTID", typeof(string));
                INDATA.Columns.Add("PLTQTY", typeof(string));
                INDATA.Columns.Add("PRODID", typeof(string));
                INDATA.Columns.Add("INBOX_TYPE", typeof(string));
                INDATA.Columns.Add("USERID", typeof(string));
                INDATA.Columns.Add("RESNNOTE", typeof(string));
                INDATA.Columns.Add("S40", typeof(string));

                DataTable INLOT = new DataTable();
                INLOT.TableName = "INLOT";
                INLOT.Columns.Add("LOTID", typeof(string));
                INLOT.Columns.Add("PRODID", typeof(string));
                INLOT.Columns.Add("WIPQTY", typeof(string));
                INLOT.Columns.Add("PROCID", typeof(string));
                INLOT.Columns.Add("AREAID", typeof(string));
                INLOT.Columns.Add("PROD_EQSGID", typeof(string));

                DataRow drINDATA = INDATA.NewRow();
                drINDATA["LANGID"] = LoginInfo.LANGID;
                drINDATA["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                drINDATA["AREAID"] = LoginInfo.CFG_AREA_ID;
                drINDATA["CSTID"] = txtCstid.Text;
                drINDATA["PLTID"] = txtPalletid.Text;
                drINDATA["PRODID"] = txtCellprodid.Text;
                drINDATA["INBOX_TYPE"] = inTypeBox;

                drINDATA["USERID"] = LoginInfo.USERID;
                drINDATA["RESNNOTE"] = txtReason.Text;
                drINDATA["S40"] = DataTableConverter.GetValue(dgPltcell.Rows[0].DataItem, "S40").ToString(); 


                for (int i=0; i < dgPltcell.Rows.Count-1; i++)
                {
                    if (DataTableConverter.GetValue(dgPltcell.Rows[i].DataItem, "CHK").ToString() == "True")
                    {
                        DataRow drINLOT = INLOT.NewRow();
                        drINLOT["LOTID"] = DataTableConverter.GetValue(dgPltcell.Rows[i].DataItem, "LOTID").ToString();
                        drINLOT["PRODID"] = DataTableConverter.GetValue(dgPltcell.Rows[i].DataItem, "PRODID").ToString();
                        drINLOT["WIPQTY"] = DataTableConverter.GetValue(dgPltcell.Rows[i].DataItem, "WIPQTY").ToString();
                        drINLOT["PROCID"] = DataTableConverter.GetValue(dgPltcell.Rows[i].DataItem, "PROCID").ToString();
                        drINLOT["AREAID"] = DataTableConverter.GetValue(dgPltcell.Rows[i].DataItem, "AREAID").ToString();
                        drINLOT["PROD_EQSGID"] = DataTableConverter.GetValue(dgPltcell.Rows[i].DataItem, "PROD_EQSGID").ToString();
                        irow++;
                        INLOT.Rows.Add(drINLOT);
                    }
                    
                }

                drINDATA["PLTQTY"] = irow;


                INDATA.Rows.Add(drINDATA);

                indataSet.Tables.Add(INDATA);
                indataSet.Tables.Add(INLOT);

                loadingIndicator.Visibility = Visibility.Visible;


                new ClientProxy().ExecuteService_Multi("BR_PRD_REG_LOGIS_PLT_RECONFIG", "INDATA,INLOT", "OUTDATA", (dsResult, dataException) =>
                {
                    try
                    {
                        loadingIndicator.Visibility = Visibility.Collapsed;
                        if (dataException != null)
                        {
                            loadingIndicator.Visibility = Visibility.Collapsed;
                            Util.MessageException(dataException);
                            return;
                        }
                        else
                        {
                            if (dsResult != null && dsResult.Tables.Count > 0)
                            {
                                if ((dsResult.Tables.IndexOf("OUTDATA") > -1))
                                {
                                    if (dsResult.Tables["OUTDATA"].Rows.Count > 0)
                                    {
                                        LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("SFU2056", dsResult.Tables["OUTDATA"].Rows.Count), null, "Info", MessageBoxButton.OK, MessageBoxIcon.None);
                                        btnSearch_Click(null, null);
                                        Refresh();
                                    }
                                }
                            }
                            return;
                        }
                    }
                    catch (Exception ex)
                    {
                        throw ex;
                    }
                }, indataSet);

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
            finally
            {
                HiddenLoadingIndicator();
            }


        }
        private bool Validation()
        {
            int ichk = 0;
            for (int i=0; i < dgPltcell.BottomRows.Count; i++)
            {
               if(DataTableConverter.GetValue(dgPltcell.Rows[i].DataItem, "CHK").ToString() == "True")
                {
                    ichk++;
                }

            }
            if (ichk == 0)
            {
                Util.MessageValidation("SFU1261"); //���õ� LOT�� �����ϴ�.
                return false;
            }
            if (string.IsNullOrEmpty(txtCstid.Text))
            {
                Util.MessageValidation("SFU7006");
                return false;
            }
            if (string.IsNullOrEmpty(txtPalletid.Text))
            {
                Util.MessageValidation("SFU3425"); //���õ� Pallet�� �����ϴ�.
                return false;
            }
            if (dgPltcell.BottomRows.Count == 0)
            {
                Util.MessageValidation("SFU1261");
                return false;
            }
            if (string.IsNullOrEmpty(txtReason.Text))
            {
                Util.MessageValidation("SFU1594"); //������ �Է��ϼ���.
                return false;
            }
            return true;
        }
        //Cell��ü �� ���� ��ư�� ���� POPUPó��
        private void btnChgcel_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (dgBoxhistory.SelectedIndex > -1)
                {
                    if (!string.IsNullOrEmpty(txtPLTH.Text))
                    {
                        DataRowView drv = dgBoxhistory.CurrentRow.DataItem as DataRowView;

                        if (string.IsNullOrEmpty(txtPLTH.Text))
                        {
                            Util.MessageInfo("SFU1636");
                            return;
                        }
                        for (int i=0; i <dgBoxhistory.Rows.Count; i++)
                        {
                            if (DataTableConverter.GetValue(dgBoxhistory.Rows[i].DataItem, "CHK").ToString() == "True")
                            {
                                if (DataTableConverter.GetValue(dgBoxhistory.Rows[i].DataItem, "APPR_YN").ToString() == "Y")
                                {
                                    Util.MessageInfo("SFU3723"); //�۾� ������ ���°� �ƴմϴ�.
                                    return;
                                }
                            }
                        }
                        if (string.IsNullOrEmpty(txtcstidH.Text))
                        {
                            Util.MessageInfo("SFU4564");
                            return;
                        }

                        PACK003_007_LOTCHANGE popup = new PACK003_007_LOTCHANGE();

                        popup.FrameOperation = this.FrameOperation;
                        if (popup != null)
                        {
                            object[] Parameters = new object[2];

                            Parameters[0] = txtcstidH.Text;
                            Parameters[1] = txtboxIdH.Text;

                            C1WindowExtension.SetParameters(popup, Parameters);

                            popup.ShowModal();
                            popup.CenterOnScreen();
                            //txtPLTH.Text = "";
                        }
                    }
                }
            }
            catch(Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        //private void dgBoxhistory_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        //{
        //    try
        //    {
        //        if (dgBoxhistory.Rows.Count == 0 || dgBoxhistory == null)
        //        {
        //            return;
        //        }

        //        Point pnt = e.GetPosition(null);
        //        C1.WPF.DataGrid.DataGridCell cell = dgBoxhistory.GetCellFromPoint(pnt);

        //        if (cell == null || cell.Value == null)
        //        {
        //            return;
        //        }

        //        int iRow = cell.Row.Index;
        //        int iCol = cell.Column.Index;

        //        txtcstidH.Text = DataTableConverter.GetValue(dgBoxhistory.Rows[iRow].DataItem, "TAG_ID") == null ? null : DataTableConverter.GetValue(dgBoxhistory.Rows[iRow].DataItem, "TAG_ID").ToString();
        //        txtboxIdH.Text = DataTableConverter.GetValue(dgBoxhistory.Rows[iRow].DataItem, "BOXID") == null ? null : DataTableConverter.GetValue(dgBoxhistory.Rows[iRow].DataItem, "BOXID").ToString();
        //        txtProdIdH.Text = DataTableConverter.GetValue(dgBoxhistory.Rows[iRow].DataItem, "PRODID") == null ? null : DataTableConverter.GetValue(dgBoxhistory.Rows[iRow].DataItem, "PRODID").ToString();
        //        txtBoxQty.Text = DataTableConverter.GetValue(dgBoxhistory.Rows[iRow].DataItem, "BOXLOTCNT") == null ? null : DataTableConverter.GetValue(dgBoxhistory.Rows[iRow].DataItem, "BOXLOTCNT").ToString();
        //        //setBoxLabel();


        //    }
        //    catch (Exception ex)
        //    {
        //        Util.MessageException(ex);
        //    }
        //}
        //�������
        private void btnPacCancel_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtboxIdH.Text))
            {
                Util.MessageInfo("SFU1636");
                return;
            }
            if (string.IsNullOrEmpty(txtcstidH.Text))
            {
                Util.MessageInfo("SFU4564");
                return;
            }
            for (int i = 0; i < dgBoxhistory.Rows.Count; i++)
            {
                if (DataTableConverter.GetValue(dgBoxhistory.Rows[i].DataItem, "CHK").ToString() == "True")
                {
                    if (DataTableConverter.GetValue(dgBoxhistory.Rows[i].DataItem, "APPR_YN").ToString() == "Y")
                    {
                        Util.MessageInfo("SFU3723"); //�۾� ������ ���°� �ƴմϴ�.
                        return;
                    }
                }
            }
            LGC.GMES.MES.ControlsLibrary.MessageBox.Show(ms.AlertRetun("SFU3405"), null, "Information", MessageBoxButton.OKCancel, MessageBoxIcon.None, (result) =>
                //CSTID�� �����Ͻðڽ��ϱ�?
                {
                    if (result == MessageBoxResult.OK)
                    {
                        try
                        {


                        DataTable INDATA = new DataTable();
                        INDATA.TableName = "INDATA";
                        INDATA.Columns.Add("SRCTYPE", typeof(string));
                        INDATA.Columns.Add("BOXID", typeof(string));
                        INDATA.Columns.Add("PRODID", typeof(string));
                        INDATA.Columns.Add("UNPACK_QTY", typeof(string));
                        INDATA.Columns.Add("UNPACK_QTY2", typeof(string));
                        INDATA.Columns.Add("USERID", typeof(string));
                        INDATA.Columns.Add("NOTE", typeof(string));

                        DataRow dr = INDATA.NewRow();
                        dr["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                        dr["BOXID"] = txtboxIdH.Text;
                        dr["PRODID"] = txtProdIdH.Text;
                        dr["UNPACK_QTY"] = txtBoxQty.Text;
                        dr["UNPACK_QTY2"] = txtBoxQty.Text;
                        dr["USERID"] = LoginInfo.USERID;
                        dr["NOTE"] = "BOX UNPACK";
                        INDATA.Rows.Add(dr);

                        DataTable dsResult = new ClientProxy().ExecuteServiceSync("BR_PRD_REG_LOGIS_UNPACK", "INDATA", "OUTDATA", INDATA);

                        if (dsResult != null && dsResult.Rows.Count > 0)
                        {
                            Util.MessageInfo("SFU3390");

                            search();
                        }
                        }
                        catch (Exception ex)
                        {
                            Util.MessageException(ex);
                        }
                        finally
                        {
                        }
                    }
                }
          );
        }

        private void btnPltLabel_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                //rePrint = true; //�����
                if (string.IsNullOrEmpty(txtcstidH.Text))
                {
                    Util.MessageInfo("SFU4564");
                    return;
                }

                if (txtPLTH.Text.Length > 0)
                {
                    //labelPrint(sender); //pallet�� �� ���� ����

                    setTagReport();
                }

                //rePrint = false;
            }
            catch (Exception ex)
            {
                //Util.AlertInfo(ex.Message);
                Util.MessageException(ex);
            }
            finally
            {

            }
        }

        private void setTagReport()
        {
            try
            {
                LGC.GMES.MES.PACK001.LOGIS_CST_Tag rs = new LGC.GMES.MES.PACK001.LOGIS_CST_Tag();
                rs.FrameOperation = this.FrameOperation;

                if (rs != null)
                {
                    // �±� ���� â ȭ�鿡 ���.
                    object[] Parameters = new object[3];
                    Parameters[0] = "Pallet_Tag"; // "PalletHis_Tag";
                    Parameters[1] = txtPLTH.Text.ToString();
                    //Parameters[2] = unPack_EqsgID;

                    C1WindowExtension.SetParameters(rs, Parameters);

                    rs.Closed += new EventHandler(printPopUp_Closed);
                    //this.Dispatcher.BeginInvoke(new Action(() => rs.ShowModal()));
                    this.Dispatcher.BeginInvoke(new Action(() => rs.Show()));
                }
            }
            catch (Exception ex)
            {
                //Util.AlertInfo(ex.Message);
                Util.MessageException(ex);
            }
        }

        private void printPopUp_Closed(object sender, EventArgs e)
        {
            try
            {
                LGC.GMES.MES.PACK001.LOGIS_CST_Tag printPopUp = sender as LGC.GMES.MES.PACK001.LOGIS_CST_Tag;
                if (Convert.ToBoolean(printPopUp.DialogResult))
                {

                }
            }
            catch (Exception ex)
            {
                //Util.AlertInfo(ex.Message);
                Util.MessageException(ex);
            }
        }

        private void dgBoxhistory_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            try
            {
                if (e == null) return;

                Point pnt = new Point();
                C1.WPF.DataGrid.C1DataGrid c1Gd = (C1.WPF.DataGrid.C1DataGrid)sender;
               
                C1.WPF.DataGrid.DataGridCell crrCell = c1Gd.CurrentCell;

                if (crrCell != null)
                {
                    if (c1Gd.GetRowCount() > 0 && crrCell.Row.Index >= 0)
                    {

                        DataRowView drv = dgBoxhistory.CurrentRow.DataItem as DataRowView;
                        int iRow = crrCell.Row.Index;

                        if (DataTableConverter.GetValue(dgBoxhistory.Rows[iRow].DataItem, "CSTID") == null)
                        {
                            Util.MessageInfo("SFU8296");
                        }

                        if (DataTableConverter.GetValue(dgBoxhistory.Rows[iRow].DataItem, "BOXSTAT").ToString() != "CREATED")
                        {
                            if (drv != null && drv.Row.ItemArray.Length > 0)
                            {
                                if (crrCell.Column.Name == "BOXID")
                                {

                                    PACK003_007_POPUP popup = new PACK003_007_POPUP();
                                    popup.FrameOperation = this.FrameOperation;
                                    if (popup != null)
                                    {
                                        object[] Parameters = new object[3];
                                        Parameters[0] = drv.Row.ItemArray[1];
                                        Parameters[1] = drv.Row.ItemArray[2];
                                        //Parameters[2] = drv.Row.ItemArray[14];

                                        C1WindowExtension.SetParameters(popup, Parameters);

                                        popup.ShowModal();
                                        popup.CenterOnScreen();
                                    }
                                }
                            }
                        }
                        else
                        {
                            txtPLTH.Text = "";
                            return;
                        }

                    }
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void dgDeptChoice_Checked(object sender, RoutedEventArgs e)
        {
            if (sender == null)
                return;

            RadioButton rb = sender as RadioButton;

            if (rb.DataContext == null)
                return;

            int idx = ((C1.WPF.DataGrid.DataGridCellPresenter)rb.Parent).Row.Index;

            for (int i = 0; i < ((C1.WPF.DataGrid.DataGridCellPresenter)rb.Parent).DataGrid.Rows.Count; i++){
                if (idx == i){  
                    DataTableConverter.SetValue(((C1.WPF.DataGrid.DataGridCellPresenter)rb.Parent).DataGrid.Rows[i].DataItem, "CHK", true);
                    
                    ((C1.WPF.DataGrid.DataGridCellPresenter)rb.Parent).DataGrid.SelectedIndex = i;
                    //txtPLTH.Text = DataTableConverter.GetValue((((C1.WPF.DataGrid.DataGridCellPresenter)rb.Parent).DataGrid.Rows[idx].DataItem, "BOXID").ToString();
                    txtPLTH.Text = DataTableConverter.GetValue(((C1.WPF.DataGrid.DataGridCellPresenter)rb.Parent).DataGrid.Rows[idx].DataItem, "BOXID") == null ? null : DataTableConverter.GetValue(((C1.WPF.DataGrid.DataGridCellPresenter)rb.Parent).DataGrid.Rows[idx].DataItem, "BOXID").ToString();
                    txtcstidH.Text = DataTableConverter.GetValue(((C1.WPF.DataGrid.DataGridCellPresenter)rb.Parent).DataGrid.Rows[idx].DataItem, "CSTID") == null ? null : DataTableConverter.GetValue(((C1.WPF.DataGrid.DataGridCellPresenter)rb.Parent).DataGrid.Rows[idx].DataItem, "CSTID").ToString();
                    txtboxIdH.Text = DataTableConverter.GetValue(((C1.WPF.DataGrid.DataGridCellPresenter)rb.Parent).DataGrid.Rows[idx].DataItem, "BOXID") == null ? null : DataTableConverter.GetValue(((C1.WPF.DataGrid.DataGridCellPresenter)rb.Parent).DataGrid.Rows[idx].DataItem, "BOXID").ToString();
                    txtProdIdH.Text = DataTableConverter.GetValue(((C1.WPF.DataGrid.DataGridCellPresenter)rb.Parent).DataGrid.Rows[idx].DataItem, "PRODID") == null ? null : DataTableConverter.GetValue(((C1.WPF.DataGrid.DataGridCellPresenter)rb.Parent).DataGrid.Rows[idx].DataItem, "PRODID").ToString();
                    txtBoxQty.Text = DataTableConverter.GetValue(((C1.WPF.DataGrid.DataGridCellPresenter)rb.Parent).DataGrid.Rows[idx].DataItem, "BOXLOTCNT") == null ? null : DataTableConverter.GetValue(((C1.WPF.DataGrid.DataGridCellPresenter)rb.Parent).DataGrid.Rows[idx].DataItem, "BOXLOTCNT").ToString();
                }
                else
                { 
                    DataTableConverter.SetValue(((C1.WPF.DataGrid.DataGridCellPresenter)rb.Parent).DataGrid.Rows[i].DataItem, "CHK", false);
                }
            }
        }
    }
}
