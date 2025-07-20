/*************************************************************************************
 Created Date : 2016.06.16
      Creator : �常ö
   Decription : BOX ����� �� ����, ����� ȭ��
--------------------------------------------------------------------------------------
 [Change History]
  2020.06.16  ���Թ� : Initial Created.
  2023.10.25  ��μ� �̷� ��ȸ �� PRODID�� NULL�� ���� ���� cboProduct ���� �������� ������ ���� [��û��ȣ] E20231018-000854 
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
using System.Linq;

namespace LGC.GMES.MES.PACK001
{
    public partial class PACK001_064 : UserControl, IWorkArea
    {
        LGC.GMES.MES.PACK001.Class.MESSAGE_PARAM ms = new LGC.GMES.MES.PACK001.Class.MESSAGE_PARAM();

        #region Box ��ĵ�� Box������ ��Ƶα� ���� ����(BOX����)
        DataTable dtBoxWoResult;

        #endregion

        #region [ Initialize ] 
        public IFrameOperation FrameOperation
        {
            get;
            set;
        }

        public PACK001_064()
        {
            InitializeComponent();
            Initialize();
        }

        private void Initialize()
        {
            InitCombo();
            //���� �ʱⰪ ����
            dtpDateFrom.SelectedDateTime = (DateTime)System.DateTime.Now.AddDays(-7); //������ �� ��¥
            dtpDateTo.SelectedDateTime = (DateTime)System.DateTime.Now;//���� ��¥

        }

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

            //��           
            C1ComboBox[] cboAreaChild = { cboProductModel };
            _combo.SetCombo(cboArea, CommonCombo.ComboStatus.SELECT, cbChild: cboAreaChild);

            //��          
            C1ComboBox[] cboProductModelParent = { cboArea, cboEquipmentSegment };
            C1ComboBox[] cboProductModelChild = { cboProduct };
            _combo.SetCombo(cboProductModel, CommonCombo.ComboStatus.ALL, cbParent: cboProductModelParent, cbChild: cboProductModelChild, sCase: "PRJ_MODEL_AUTH");

            //��ǰ�ڵ�  
            C1ComboBox[] cboProductParent = { cboSHOPID, cboArea, cboEquipmentSegment, cboProductModel, cboPrdtClass };
            _combo.SetCombo(cboProduct, CommonCombo.ComboStatus.ALL, cbParent: cboProductParent, sCase: "PRJ_PRODUCT");

        }
        #endregion

        #region [ Global variable ] 
        #endregion

        #region [ Event ] 

        private void UserControl_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {
            nbBoxingCnt.ValueChanged += nbBoxingCnt_ValueChanged;
        }

        #region [ BOX ���� ���� ]
        private void nbBoxingCnt_ValueChanged(object sender, PropertyChangedEventArgs<double> e)
        {
            int boxLotmax_cnt = nbBoxingCnt == null ? 5 : Convert.ToInt32(nbBoxingCnt.Value);
            int inPutLot = nbBoxingCnt == null ? 0 : dgBoxLot.GetRowCount();
            string stat = string.Empty;

            setBoxCnt(stat, boxLotmax_cnt, inPutLot);
        }
        #endregion

        #region [ BOX �ڵ� ���� ���� ]
        private void chkBoxId_Checked(object sender, System.Windows.RoutedEventArgs e)
        {
            txtBoxId.IsEnabled = false;
        }

        private void chkBoxId_Unchecked(object sender, System.Windows.RoutedEventArgs e)
        {
            txtBoxId.IsEnabled = true;
        }
        #endregion

        #region ( Lot �Է� )
        private void txtLotId_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            try
            {
                if (e.Key == Key.Enter)
                {
                    DataTable dt = null;

                    if ((bool)chkBoxId.IsChecked) // BOXID üũ �ڽ��� üũ ������� BOXID �ڵ� ����
                    {
                        dt = chkReworkBox();

                        if (dt == null)
                        {
                            Util.MessageInfo("SFU3381");
                            return;
                        }


                        if (dt.Rows.Count > 0)
                        {

                            if (!(dgBoxLot.GetRowCount() > 0) || dgBoxLot.ItemsSource == null)
                            {
                                #region ( BOXID �ڵ� ������, ù LOT ���� )
                                autoBoxIdCreate(dt.Rows[0]["CLASS"].ToString(), dt.Rows[0]["PRODID"].ToString(), dt.Rows[0]["EQSGID"].ToString(), dt.Rows[0]["WOID"].ToString());

                                Util.GridSetData(dgBoxLot, dt, FrameOperation, true);

                                txtBoxingModel.Text = dt.Rows[0]["BOXID"].ToString();
                                txtBoxingProd.Text = dt.Rows[0]["PRODID"].ToString();
                                txtEqsgID.Text = dt.Rows[0]["EQSGID"].ToString();
                                txtEqsgName.Text = dt.Rows[0]["EQSGNAME"].ToString();
                                txtBoxingPcsg.Text = dt.Rows[0]["CLASS"].ToString();
                                txtBoxingModel.Text = dt.Rows[0]["MODEL"].ToString();
                                txtProcId.Text = dt.Rows[0]["PROCID"].ToString();
                                txtWoId.Text = dt.Rows[0]["WOID"].ToString();

                                nbBoxingCnt.IsEnabled = false;
                                chkBoxId.IsEnabled = false;

                                setBoxCnt("������", Int32.Parse(nbBoxingCnt.Value.ToString()), 1);
                                #endregion
                            }
                            else
                            {
                                #region ( BOX ID �ڵ�������, �߰� LOT ���� )

                                int TotalRow = dgBoxLot.GetRowCount();
                                int CanBoxCnt = Int32.Parse(nbBoxingCnt.Value.ToString());

                                if (TotalRow >= CanBoxCnt)
                                {
                                    Util.MessageInfo("SFU4554");
                                    return;
                                }

                                for (int i = 0; i < TotalRow; i++)
                                {
                                    string strLotId = DataTableConverter.GetValue(dgBoxLot.Rows[i].DataItem, "LOTID").ToString();

                                    if (txtLotId.Text.ToString() == strLotId)
                                    {
                                        Util.MessageInfo("SFU8166");
                                        txtLotId.Text = "";
                                        txtLotId.Focus();
                                        return;
                                    }
                                }

                                if (!dt.Rows[0]["PRODID"].ToString().Equals(txtBoxingProd.Text.ToString()))
                                {
                                    Util.MessageInfo("SFU3457");
                                    return;
                                }

                                GridAdd(dgBoxLot, dt);
                                setBoxCnt("������", CanBoxCnt, dgBoxLot.GetRowCount());
                                #endregion
                            }
                        }
                    }
                    else
                    {
                        if (string.IsNullOrWhiteSpace(txtBoxId.Text.ToString()))
                        {
                            Util.MessageInfo("BOX ID�� ���� �����ϼ���.");
                            return;
                        }

                        dt = chkReworkBox();

                        int TotalRow = dgBoxLot.GetRowCount();
                        int CanBoxCnt = Int32.Parse(nbBoxingCnt.Value.ToString());

                        if (!(dgBoxLot.GetRowCount() > 0) || dgBoxLot.ItemsSource == null)
                        {
                            #region ( �ڽ� ID �Է�, ù LOT ���� )
                            if (TotalRow >= CanBoxCnt)
                            {
                                Util.MessageInfo("SFU4554");
                                return;
                            }

                            if (!dt.Rows[0]["PRODID"].ToString().Equals(txtBoxingProd.Text.ToString()))
                            {
                                Util.MessageInfo("SFU3457");
                                return;
                            }
                            Util.GridSetData(dgBoxLot, dt, FrameOperation, true);

                            txtEqsgID.Text = dt.Rows[0]["EQSGID"].ToString();
                            txtEqsgName.Text = dt.Rows[0]["EQSGNAME"].ToString();
                            txtWoId.Text = dt.Rows[0]["WOID"].ToString();


                            nbBoxingCnt.IsEnabled = false;
                            chkBoxId.IsEnabled = false;

                            setBoxCnt("������", Int32.Parse(nbBoxingCnt.Value.ToString()), 1);
                            #endregion
                        }
                        else
                        {
                            #region ( �ڽ� ID �Է�, �߰� LOT ���� )

                            if (TotalRow >= CanBoxCnt)
                            {
                                Util.MessageInfo("SFU4554");
                                return;
                            }

                            for (int i = 0; i < TotalRow; i++)
                            {
                                string strLotId = DataTableConverter.GetValue(dgBoxLot.Rows[i].DataItem, "LOTID").ToString();

                                if (txtLotId.Text.ToString() == strLotId)
                                {
                                    Util.MessageInfo("SFU8166");
                                    txtLotId.Text = "";
                                    txtLotId.Focus();
                                    return;
                                }
                            }

                            if (!dt.Rows[0]["PRODID"].ToString().Equals(txtBoxingProd.Text.ToString()))
                            {
                                Util.MessageInfo("SFU3457");
                                return;
                            }

                            GridAdd(dgBoxLot, dt);
                            setBoxCnt("������", Int32.Parse(nbBoxingCnt.Value.ToString()), dgBoxLot.GetRowCount());

                            #endregion
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        #endregion

        #region ( ���� )
        private void dgBoxLot_MouseUp(object sender, MouseButtonEventArgs e)
        {
            try
            {
                Point pnt = e.GetPosition(null);
                C1.WPF.DataGrid.DataGridCell cell = dgBoxLot.GetCellFromPoint(pnt);

                if (cell != null)
                {
                    string sColName = dgBoxLot.CurrentColumn.Name;

                    if (!sColName.Contains("CHK"))
                    {
                        return;
                    }

                }

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        #endregion


        #region ( ��� )
        private void btncancel_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            try
            {
                Util.gridClear(dgBoxLot);
                clear();
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        #endregion

        #region ( ���� ��� )
        private void btnSelectCacel_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            try
            {
                if (dgBoxLot.ItemsSource != null)
                {
                    for (int i = dgBoxLot.Rows.Count; 0 < i; i--)
                    {
                        var chkYn = DataTableConverter.GetValue(dgBoxLot.Rows[i - 1].DataItem, "CHK");
                        var lot_id = DataTableConverter.GetValue(dgBoxLot.Rows[i - 1].DataItem, "LOTID");

                        if (Convert.ToBoolean(chkYn))
                        {
                            dgBoxLot.EndNewRow(true);
                            dgBoxLot.RemoveRow(i - 1);

                        }
                    }

                    DataTable dt = DataTableConverter.Convert(dgBoxLot.ItemsSource);


                    if (dt.Rows.Count < 1)
                    {
                        LGC.GMES.MES.ControlsLibrary.MessageBox.Show(ms.AlertRetun("SFU3406"), null, "CAUSE", MessageBoxButton.OKCancel, MessageBoxIcon.Warning, (caution_result) =>
                        //���� ���帮��Ʈ�� ���� �Ͻðڽ��ϱ�?
                        {
                            if (caution_result == MessageBoxResult.OK)
                            {
                                dgBoxLot.ItemsSource = null;

                                tbBoxingWait_cnt.Text = "[ 0 " + ObjectDic.Instance.GetObjectName("��") + " ]";

                                setBoxCnt("�����", Convert.ToInt32(nbBoxingCnt.Value), 0);
                                //clearBoxingContents();
                                clear();
                            }
                            else
                            {
                                return;
                            }
                        }
                      );
                    }
                    else
                    {
                        setBoxCnt("������", Convert.ToInt32(nbBoxingCnt.Value), 0);
                    }
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        #endregion

        #region ( BOXID Ȯ�� )
        private void txtBoxId_KeyDown(object sender, KeyEventArgs e)
        {
            try
            {
                if (e.Key == Key.Enter)
                {

                    DataTable RQSTDT = new DataTable();
                    RQSTDT.TableName = "RQSTDT";
                    RQSTDT.Columns.Add("BOXID", typeof(string));     //       
                    RQSTDT.Columns.Add("BOXTYPE", typeof(string));   //                                 

                    DataRow dr = RQSTDT.NewRow();
                    dr["BOXID"] = txtBoxId.Text;
                    dr["BOXTYPE"] = "BOX"; //"BOX";                

                    RQSTDT.Rows.Add(dr);

                    DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_BOX_INFO_REPACKING_PACK", "INDATA", "OUTDATA", RQSTDT);

                    //2023.03.29 PLT ���� �Ϸ�� ��� BOX ���� ���� ������ �������� VALIDATION �߰�(PACK001_015:boxValidation ����) - KIM MIN SEOK
                    foreach (DataRow drw in dtResult.Rows)
                    {
                        if (drw["BOXSTAT"].ToString() == "PACKED")
                        {
                            ms.AlertWarning("SFU3315"); //�Է¿��� : �Է��� BOX�� ����Ϸ� �� BOX�Դϴ�.[BOX ���� Ȯ��].
                            txtBoxingProd.Text = "";
                            txtBoxingPcsg.Text = "";
                            return;
                        }
                    }

                    if (dtResult == null || dtResult.Rows.Count == 0)
                    {
                        Util.MessageInfo("SFU1180"); //BOX ������ �����ϴ�.
                        txtBoxId.Text = "";
                        txtBoxingProd.Text = "";
                        txtBoxingPcsg.Text = "";
                        //txtBoxId.GotFocus();
                        return;
                    }
                    //else if (!BoxWoInform())//BOX�� ������ ��ǰ�� ��ϵƴ��� Ȯ��.
                    //{
                    //    ms.AlertWarning("SFU3454"); //��������� ������ �� �ٽ� ��ĵ�ϼ���
                    //    txtBoxingProd.Text = "";
                    //    txtBoxingPcsg.Text = "";
                    //    return;
                    //}
                    else
                    {
                        //txtBoxingModel.Text = dtResult.Rows[0]["BOXID"].ToString();
                        txtBoxingProd.Text = dtResult.Rows[0]["PRODID"].ToString();
                        txtBoxingPcsg.Text = dtResult.Rows[0]["PRDT_CLSS_CODE"].ToString();
                        txtBoxingModel.Text = dtResult.Rows[0]["PRJT_ABBR_NAME"].ToString();
                        txtProcId.Text = dtResult.Rows[0]["PROCID"].ToString();

                        chkBoxId.IsEnabled = false;
                        txtBoxId.IsEnabled = false;
                        chkBoxId.IsEnabled = false;
                    }
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        #endregion

        //2023.03.29 PLT ���� �Ϸ�� ��� BOX ���� ���� ������ �������� VALIDATION �߰�(PACK001_015:boxValidation ����) - KIM MIN SEOK
        private bool BoxWoInform()
        {
            try
            {
                bool woYn = false;


                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("BOXID", typeof(string));     //       
                RQSTDT.Columns.Add("BOXTYPE", typeof(string));   //                                 

                DataRow dr = RQSTDT.NewRow();
                dr["BOXID"] = txtBoxId.Text;
                dr["BOXTYPE"] = "BOX"; //"BOX";                

                RQSTDT.Rows.Add(dr);

                dtBoxWoResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_BOX_PROD_WO_FIND", "INDATA", "OUTDATA", RQSTDT);

                if (dtBoxWoResult.Rows.Count > 0)
                {
                    woYn = true;
                }

                return woYn;
            }
            catch (Exception ex)
            {
                throw ex;
            }

        }


        #region ( ��ȸ ��ư Ŭ�� )
        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (cboArea.SelectedIndex == 0)
                {
                    Util.MessageInfo("SFU1499"); //���� �����ϼ���.
                    return;
                }

                search();
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        #endregion

        #region (BOXID + ENTER)
        private void txtSearchBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                try
                {
                    if (txtSearchBox.Text.Length == 0)
                    {
                        return;
                    }

                    SearchBox(Util.GetCondition(txtSearchBox), true);
                }
                catch (Exception ex)
                {
                    //Util.AlertInfo(ex.Message);
                    Util.MessageException(ex);
                }
            }
        }
        #endregion

        #region ( �ʱ�ȭ ó�� )
        private void clear()
        {
            try
            {
                chkBoxId.IsEnabled = true;
                chkBoxId.IsChecked = false;
                txtBoxId.IsEnabled = true;
                txtLotId.IsEnabled = true;
                nbBoxingCnt.IsEnabled = true;


                txtWoId.Text = "";
                txtLotId.Text = "";
                txtBoxId.Text = "";
                txtBoxingProd.Text = "";
                txtBoxingPcsg.Text = "";
                txtBoxingModel.Text = "";
                txtEqsgName.Text = "";
                txtProcId.Text = "";
                //txtProdClass.Text = "";

                Util.gridClear(dgBoxLot);

                nbBoxingCnt.Value = 5;
                tbCount.Text = "5";

                setBoxCnt("������", 5, 0);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        #endregion


        private void setBoxCnt(string boxing_stat, int max_cnt, int lot_cnt)
        {
            if (txtcnt == null)
            {
                return;
            }

            // ���� ���� : ���� ���� ���� / �ִ� �������
            txtcnt.Text = ObjectDic.Instance.GetObjectName(boxing_stat) + " : " + lot_cnt.ToString() + " / " + max_cnt.ToString();
            //���� ����
            tbCount.Text = (max_cnt - lot_cnt).ToString();
        }

        private void GridAdd(C1.WPF.DataGrid.C1DataGrid dg, DataTable dt)
        {
            try
            {
                int TotalRow = dg.GetRowCount();

                if (TotalRow == 0)
                {
                    return;
                }

                for (int k = 0; dt.Rows.Count > k; k++)
                {
                    dg.BeginNewRow();
                    dg.EndNewRow(true);
                    for (int i = 0; dg.Columns.Count > i; i++)
                    {
                        for (int j = 0; dt.Columns.Count > j; j++)
                        {
                            if (dg.Columns[i].Name.ToString() == dt.Columns[j].ToString())
                            {
                                DataTableConverter.SetValue(dg.Rows[TotalRow + k].DataItem, dg.Columns[i].Name.ToString(), dt.Rows[k][j].ToString());
                            }
                            //break;
                        }
                    }
                }

                DataTable dtTemp = Util.MakeDataTable(dg, true);

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        #region ( BOX LABEL ��ư Ŭ�� )
        private void btnBoxLabel_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                labelPrint();
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        #endregion

        #region ( �� �μ� )
        private void labelPrint()
        {
            try
            {
                if (
                string.IsNullOrWhiteSpace(txtBoxIdH.Text.ToString()) ||
                string.IsNullOrWhiteSpace(txtEqsgIdH.Text.ToString()) ||
                string.IsNullOrWhiteSpace(txtProdIdH.Text.ToString()) ||
                string.IsNullOrWhiteSpace(txtProcIdH.Text.ToString()) ||
                string.IsNullOrWhiteSpace(txtProdClassH.Text.ToString()) ||
                string.IsNullOrWhiteSpace(txtseletedWO.Text.ToString()) ||
                string.IsNullOrWhiteSpace(txtBoxQty.Text.ToString())
                //string.IsNullOrWhiteSpace(txtShipToId.Text.ToString())
                )
                {
                    Util.MessageInfo("SFU1636");
                    return;
                }

                string sLBL_code = Util.NVC(cboLabelCode.SelectedValue).Length > 0 ? Util.NVC(cboLabelCode.SelectedValue) : "LBL0021";

                DataTable dtzpl = Util.getZPL_Pack(sLOTID: txtBoxIdH.Text.ToString()
                                        , sLABEL_TYPE: LABEL_TYPE_CODE.PACK_INBOX
                                        , sLABEL_CODE: sLBL_code//null /*"LBL0020"*/
                                        , sSAMPLE_FLAG: "N"
                                        , sPRN_QTY: "1"
                                        , sPRODID: txtProdIdH.Text.ToString()
                                        , sSHIPTO_ID: txtShipToId.Text.ToString()
                                        );

                if (dtzpl == null || dtzpl.Rows.Count == 0)
                {
                    return;
                }

                string zpl = Util.NVC(dtzpl.Rows[0]["ZPLSTRING"]);

                Util.PrintLabel(FrameOperation, loadingIndicator, zpl);

                DataTable dtBoxPrintHistory = setBoxResultList();

                if (dtBoxPrintHistory == null || dtBoxPrintHistory.Rows.Count == 0)
                {
                    return;
                }

                string print_cnt = dtBoxPrintHistory.Rows[0]["PRT_REQ_SEQNO"].ToString();
                string print_yn = dtBoxPrintHistory.Rows[0]["PRT_FLAG"].ToString();

                if (dtBoxPrintHistory.Rows[0]["PRT_FLAG"].ToString() == "N")//print ���� N�ΰ�� Y�� update
                {
                    updateTB_SFC_LABEL_PRT_REQ_HIST(print_yn, print_cnt);
                }
                else
                {
                    updateTB_SFC_LABEL_PRT_REQ_HIST(null, null);
                }


            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        #endregion

        #region ( �ڽ� �� �ҷ����� ) 
        private void setBoxLabel()
        {
            try
            {
                CommonCombo _combo = new CommonCombo();
                String[] sFilter = { txtProdIdH.Text.ToString(), null, null, LABEL_TYPE_CODE.PACK_INBOX };

                _combo.SetCombo(cboLabelCode, CommonCombo.ComboStatus.NONE, sFilter: sFilter, sCase: "LABELCODE_BY_PROD");

                if (cboLabelCode.ItemsSource == null) return;

                cboLabelCode.SelectedIndex = 0;


            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        #endregion

        #region ( �̷�ȭ�� Ŭ�� �̺�Ʈ )
        private void dgBoxhistory_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            try
            {
                if (dgBoxhistory.Rows.Count == 0 || dgBoxhistory == null)
                {
                    return;
                }

                Point pnt = e.GetPosition(null);
                C1.WPF.DataGrid.DataGridCell cell = dgBoxhistory.GetCellFromPoint(pnt);

                if (cell == null || cell.Value == null)
                {
                    return;
                }

                int iRow = cell.Row.Index;
                int iCol = cell.Column.Index;

                txtBoxIdH.Text = DataTableConverter.GetValue(dgBoxhistory.Rows[iRow].DataItem, "BOXID") == null ? null : DataTableConverter.GetValue(dgBoxhistory.Rows[iRow].DataItem, "BOXID").ToString();
                txtEqsgIdH.Text = DataTableConverter.GetValue(dgBoxhistory.Rows[iRow].DataItem, "EQSGID") == null ? null : DataTableConverter.GetValue(dgBoxhistory.Rows[iRow].DataItem, "EQSGID").ToString();
                txtProdIdH.Text = DataTableConverter.GetValue(dgBoxhistory.Rows[iRow].DataItem, "PRODID") == null ? null : DataTableConverter.GetValue(dgBoxhistory.Rows[iRow].DataItem, "PRODID").ToString();
                txtProcIdH.Text = DataTableConverter.GetValue(dgBoxhistory.Rows[iRow].DataItem, "PROCID") == null ? null : DataTableConverter.GetValue(dgBoxhistory.Rows[iRow].DataItem, "PROCID").ToString();
                txtProdClassH.Text = DataTableConverter.GetValue(dgBoxhistory.Rows[iRow].DataItem, "PRDCLASS") == null ? null : DataTableConverter.GetValue(dgBoxhistory.Rows[iRow].DataItem, "PRDCLASS").ToString();
                txtseletedWO.Text = DataTableConverter.GetValue(dgBoxhistory.Rows[iRow].DataItem, "WOID") == null ? null : DataTableConverter.GetValue(dgBoxhistory.Rows[iRow].DataItem, "WOID").ToString();
                txtBoxQty.Text = DataTableConverter.GetValue(dgBoxhistory.Rows[iRow].DataItem, "BOXLOTCNT") == null ? null : DataTableConverter.GetValue(dgBoxhistory.Rows[iRow].DataItem, "BOXLOTCNT").ToString();

                setBoxLabel();


            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        #endregion

        #region ( BOX �� �̷� )
        private void dgBoxhistory_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            try
            {
                C1.WPF.DataGrid.C1DataGrid grid = sender as C1.WPF.DataGrid.C1DataGrid;

                Point pnt = e.GetPosition(null);
                C1.WPF.DataGrid.DataGridCell cell = grid.GetCellFromPoint(pnt);

                if (cell != null)
                {
                    if (cell.Column.Name == "LOTID")
                    {
                        this.FrameOperation.OpenMenu("SFU010090090", true, cell.Text);
                    }

                    if (cell.Column.Name == "BOXID")
                    {
                        PACK001_003_BOXINFO popup = new PACK001_003_BOXINFO();
                        popup.FrameOperation = this.FrameOperation;

                        if (popup != null)
                        {
                            DataTable dtData = new DataTable();
                            dtData.Columns.Add("BOXID", typeof(string));

                            DataRow newRow = null;
                            newRow = dtData.NewRow();
                            newRow["BOXID"] = cell.Text;

                            dtData.Rows.Add(newRow);

                            //========================================================================
                            object[] Parameters = new object[1];
                            Parameters[0] = dtData;
                            C1WindowExtension.SetParameters(popup, Parameters);
                            //========================================================================

                            popup.ShowModal();
                            popup.CenterOnScreen();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        #endregion


        #endregion

        #region [ BizActor ]

        #region ( BOXID ���� )
        private void autoBoxIdCreate(string txtBoxingPcsg, string strProdId, string strEqsgId, string strWoId)
        {
            try
            {
                string setProcid = string.Empty;

                if (txtBoxingPcsg == "CMA")
                {
                    setProcid = "P5500";
                }
                else if (txtBoxingPcsg == "BMA")
                {
                    setProcid = "P9500";
                }
                else
                {
                    return;
                }

                //boxid ���� ����
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("SRCTYPE", typeof(string));  //INPUT TYPE (UI OR EQ)         
                RQSTDT.Columns.Add("LANGID", typeof(string));   //LANGUAGE ID         
                RQSTDT.Columns.Add("LOTID", typeof(string));    //����LOT(ó�� LOT)
                RQSTDT.Columns.Add("PROCID", typeof(string));   //������� ID
                RQSTDT.Columns.Add("PRODID", typeof(string));   //lot�� ��ǰ
                RQSTDT.Columns.Add("LOTQTY", typeof(Int32));   //���� �Ѽ���
                RQSTDT.Columns.Add("EQSGID", typeof(string));   //����ID
                RQSTDT.Columns.Add("USERID", typeof(string));   //�����ID
                RQSTDT.Columns.Add("WOID", typeof(string));   //�����ID

                DataRow dr = RQSTDT.NewRow();
                dr["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                dr["LANGID"] = LoginInfo.LANGID;
                dr["LOTID"] = txtLotId.Text.ToString();
                dr["PROCID"] = setProcid;//lot_proc; // CMA:P5500, BMA:P9500
                dr["PRODID"] = strProdId; //wo�� ������ ������ ������ǰ, wo�� ������ ������ǰ ã�Ƽ� box table�� �־���(�� ������ ū �ǹ� ����).
                dr["LOTQTY"] = nbBoxingCnt.Value.ToString();// ȭ�鿡�� ������ ����....���߿� ����� ���� ��� �������� �����.         
                dr["EQSGID"] = strEqsgId;
                dr["USERID"] = LoginInfo.USERID;
                dr["WOID"] = strWoId;

                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("BR_PRD_GET_BOXIDREQUEST_WO", "INDATA", "OUTDATA", RQSTDT);

                if (dtResult != null && dtResult.Rows.Count != 0)
                {
                    txtBoxId.Text = dtResult.Rows[0][0].ToString();
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        #endregion

        #region ( ���� ���� ���� üũ ) 
        private DataTable chkReworkBox()
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("SRCTYPE", typeof(string));
                RQSTDT.Columns.Add("LOTID", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                dr["LANGID"] = LoginInfo.LANGID;
                dr["LOTID"] = Util.GetCondition(txtLotId).Trim(); //LOTID

                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("BR_PRD_CHK_REWORK_BOXLOT", "INDATA", "OUTDATA", RQSTDT);

                if (dtResult.Rows.Count > 0)
                {
                    return dtResult;
                }
                else
                {
                    return null;
                }

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return null;
            }
        }
        #endregion

        #region  ( ���� ó�� )
        private void btnPack_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (Boxing())
                {
                    clear();
                    Util.MessageInfo("SFU3386");
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        #endregion

        #region ( ���� ó�� �Լ� )
        private Boolean Boxing()
        {
            try
            {
                string strProc = string.Empty;

                if (Util.GetCondition(txtBoxingPcsg.Text.ToString()) == "CMA")
                {
                    strProc = "P5500";
                }
                else if (Util.GetCondition(txtBoxingPcsg.Text.ToString()) == "BMA")
                {
                    strProc = "9500";
                }


                DataSet indataSet = new DataSet();

                DataTable INDATA = indataSet.Tables.Add("INDATA");
                INDATA.Columns.Add("SRCTYPE", typeof(string));  //INPUT TYPE (UI OR EQ)         
                INDATA.Columns.Add("LANGID", typeof(string));   //LANGUAGE ID         
                INDATA.Columns.Add("BOXID", typeof(string));    //����LOT(ó�� LOT)
                INDATA.Columns.Add("PROCID", typeof(string));   //����ID(������ ������ ����) 
                INDATA.Columns.Add("BOXQTY", typeof(string));   //���� �Ѽ���
                INDATA.Columns.Add("EQSGID", typeof(string));   //����ID
                INDATA.Columns.Add("USERID", typeof(string));   //�����ID
                INDATA.Columns.Add("WOID", typeof(string));   //�����ID

                DataRow dr = INDATA.NewRow();
                dr["SRCTYPE"] = "UI";
                dr["LANGID"] = LoginInfo.LANGID;
                dr["BOXID"] = txtBoxId.Text.ToString();
                dr["PROCID"] = strProc;
                dr["BOXQTY"] = dgBoxLot.GetRowCount();
                dr["EQSGID"] = txtEqsgID.Text.ToString(); // BOX�� ����ID, ù ��� BOX ������ ������ ��
                dr["USERID"] = LoginInfo.USERID;
                dr["WOID"] = txtWoId.Text.ToString(); // BOX�� WOID
                INDATA.Rows.Add(dr);

                DataTable IN_LOTID = indataSet.Tables.Add("IN_LOTID");
                IN_LOTID.Columns.Add("LOTID", typeof(string));

                for (int i = 0; i < dgBoxLot.GetRowCount(); i++)
                {
                    string sLotId = Util.NVC(dgBoxLot.GetCell(i, dgBoxLot.Columns["LOTID"].Index).Value);

                    DataRow inDataDtl = IN_LOTID.NewRow();
                    inDataDtl["LOTID"] = sLotId;
                    IN_LOTID.Rows.Add(inDataDtl);
                }

                DataSet dsResult = new ClientProxy().ExecuteServiceSync_Multi("BR_PRD_REG_REWORK_BOXING_WO", "INDATA,IN_LOTID", "OUTDATA,OUT_LOTID", indataSet);

                if (dsResult != null && dsResult.Tables["OUTDATA"].Rows.Count > 0)
                {
                    return true;
                }
                else
                {
                    Util.MessageInfo("SFU3462"); //���� �۾� ���� BOXING BIZ Ȯ�� �ϼ���.
                    return false;
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return false;
            }
        }
        #endregion

        #region ( ��ȸ ) 
        private void search()
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("AREAID", typeof(string));
                RQSTDT.Columns.Add("PRODID", typeof(string));
                RQSTDT.Columns.Add("MODLID", typeof(string));
                RQSTDT.Columns.Add("FROMDATE", typeof(string));
                RQSTDT.Columns.Add("TODATE", typeof(string));
                //RQSTDT.Columns.Add("BOXID", typeof(string));
                RQSTDT.Columns.Add("SYSTEM_ID", typeof(string));
                RQSTDT.Columns.Add("USERID", typeof(string));
                RQSTDT.Columns.Add("SHOPID", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["AREAID"] = Util.GetCondition(cboArea);
                //2023.10.26 - ���� null�� ���� PRODOD ������ cboProduct�� ��ȸ�ϵ��� ���� - KIM MIN SEOK
                dr["PRODID"] = Util.GetCondition(cboProduct) == "" ? null : Util.GetCondition(cboProduct);
                dr["MODLID"] = Util.GetCondition(cboProductModel) == "" ? null : Util.GetCondition(cboProductModel);
                dr["FROMDATE"] = Util.GetCondition(dtpDateFrom);
                dr["TODATE"] = Util.GetCondition(dtpDateTo);
                //dr["BOXID"] = "";
                dr["SYSTEM_ID"] = LoginInfo.SYSID;
                dr["USERID"] = LoginInfo.USERID;
                dr["SHOPID"] = LoginInfo.CFG_SHOP_ID;

                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_BOXHISTORY_SEARCH", "INDATA", "OUTDATA", RQSTDT);

                dgBoxhistory.ItemsSource = null;

                if (dtResult.Rows.Count != 0)
                {
                    Util.GridSetData(dgBoxhistory, dtResult, FrameOperation);
                    txtSearchBox.Text = "";
                }

                Util.SetTextBlockText_DataGridRowCount(tbBoxHistory_cnt, Util.NVC(dtResult.Rows.Count));
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        #endregion

        #region (BOXID�� ��ȸ)
        private void SearchBox(string strBoxId, Boolean bGridClear)
        {
            try
            {
                DataTable dt = DataTableConverter.Convert(dgBoxhistory.ItemsSource);

                if (dt.Rows.Count > 0)
                {
                    if (dt.Select("BOXID = '" + strBoxId + "'").Count() > 0 && !bGridClear)
                    {
                        Util.MessageInfo("SFU8251");
                        return;
                    }
                }


                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("BOXID", typeof(string));
                RQSTDT.Columns.Add("SYSTEM_ID", typeof(string));
                RQSTDT.Columns.Add("USERID", typeof(string));
                RQSTDT.Columns.Add("SHOPID", typeof(string));

                RQSTDT.Columns.Add("AREAID", typeof(string));
                RQSTDT.Columns.Add("PRODID", typeof(string));
                RQSTDT.Columns.Add("MODLID", typeof(string));
                RQSTDT.Columns.Add("FROMDATE", typeof(string));
                RQSTDT.Columns.Add("TODATE", typeof(string));


                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["BOXID"] = strBoxId;
                dr["SYSTEM_ID"] = LoginInfo.SYSID;
                dr["USERID"] = LoginInfo.USERID;
                dr["SHOPID"] = LoginInfo.CFG_SHOP_ID;

                dr["AREAID"] = LoginInfo.CFG_AREA_ID;
                dr["PRODID"] = null;
                dr["MODLID"] = null;
                dr["FROMDATE"] = null;
                dr["TODATE"] = null;

                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_BOXHISTORY_SEARCH", "INDATA", "OUTDATA", RQSTDT);

                if (dtResult.Rows.Count != 0)
                {
                    if (bGridClear)
                    {
                        txtSearchBox.Text = "";
                        Util.GridSetData(dgBoxhistory, dtResult, FrameOperation);
                    }
                    else
                    {
                        txtSearchBox.Text = "";

                        Util.gridClear(dgBoxhistory);

                        dt.AsEnumerable().CopyToDataTable(dtResult, LoadOption.Upsert);

                        Util.GridSetData(dgBoxhistory, dtResult, FrameOperation);
                    }
                }
                else
                {
                    Util.MessageInfo("SFU1179");
                }

                Util.SetTextBlockText_DataGridRowCount(tbBoxHistory_cnt, Util.NVC(dtResult.Rows.Count));
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        #endregion

        #region ( ���� ��� )

        private void btnPacCancel_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(txtBoxIdH.Text.ToString()))
                {
                    Util.MessageInfo("SFU1636");
                    return;
                }

                DataTable INDATA = new DataTable();
                INDATA.TableName = "INDATA";
                INDATA.Columns.Add("SRCTYPE", typeof(string));
                INDATA.Columns.Add("BOXID", typeof(string));
                INDATA.Columns.Add("PRODID", typeof(string));
                INDATA.Columns.Add("PACK_LOT_TYPE_CODE", typeof(string));
                INDATA.Columns.Add("UNPACK_QTY", typeof(string));
                INDATA.Columns.Add("UNPACK_QTY2", typeof(string));
                INDATA.Columns.Add("USERID", typeof(string));
                INDATA.Columns.Add("ERP_IF_FLAG", typeof(string));
                INDATA.Columns.Add("NOTE", typeof(string));
                INDATA.Columns.Add("WOID", typeof(string));

                DataRow dr = INDATA.NewRow();
                dr["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                dr["BOXID"] = txtBoxIdH.Text.ToString();
                dr["PRODID"] = txtProdIdH.Text.ToString();
                dr["PACK_LOT_TYPE_CODE"] = "LOT";
                dr["UNPACK_QTY"] = txtBoxQty.Text.ToString();
                dr["UNPACK_QTY2"] = txtBoxQty.Text.ToString();
                dr["USERID"] = LoginInfo.USERID;
                dr["ERP_IF_FLAG"] = "C";
                dr["NOTE"] = "BOX UNPACK";
                dr["WOID"] = txtseletedWO.Text.ToString();
                INDATA.Rows.Add(dr);

                DataTable dsResult = new ClientProxy().ExecuteServiceSync("BR_PRD_REG_UNPACK_BOX", "INDATA", "OUTDATA", INDATA);

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
        }
        #endregion


        #region ( �� ���ý� ShipTo_ID )
        private void cboLabelCode_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            try
            {
                if (e.NewValue == null) return;

                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("PRODID", typeof(string));
                RQSTDT.Columns.Add("SHOPID", typeof(string));
                RQSTDT.Columns.Add("LABEL_CODE", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["PRODID"] = txtProcIdH.Text.ToString();
                dr["SHOPID"] = LoginInfo.CFG_SHOP_ID;
                //���� ������ "LBL0020,LBL0067" ����
                dr["LABEL_CODE"] = cboLabelCode.SelectedValue.ToString() == null ? "LBL0020,LBL0067" : cboLabelCode.SelectedValue.ToString();

                RQSTDT.Rows.Add(dr);

                DataTable dt = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_LABELCODE_BY_PRODID_BOX_CBO", "RQSTDT", "RSLTDT", RQSTDT);

                if (dt.Rows.Count > 0)
                {
                    txtShipToId.Text = dt.Rows[0]["SHIPTO_ID"].ToString();
                }
                else
                {
                    txtShipToId.Text = "";
                }



            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        #endregion

        #region ( BOX �� ���� ��û �̷� ��ȸ )
        private DataTable setBoxResultList()
        {
            try
            {
                //DA_PRD_SEL_BOX_LIST_FOR_LABEL

                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("EQSGID", typeof(string));
                RQSTDT.Columns.Add("PROCID", typeof(string));
                RQSTDT.Columns.Add("EQPTID", typeof(string));
                RQSTDT.Columns.Add("FROMDATE", typeof(string));
                RQSTDT.Columns.Add("TODATE", typeof(string));
                RQSTDT.Columns.Add("BOXID", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["EQSGID"] = LoginInfo.CFG_EQSG_ID;
                dr["PROCID"] = null;
                dr["EQPTID"] = null;
                dr["FROMDATE"] = dtpDateFrom.SelectedDateTime.ToString("yyyyMMdd");
                dr["TODATE"] = dtpDateTo.SelectedDateTime.ToString("yyyyMMdd");
                dr["BOXID"] = Util.GetCondition(txtBoxIdH) == "" ? null : Util.GetCondition(txtBoxIdH);
                RQSTDT.Rows.Add(dr);

                DataTable dtboxList = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_BOX_LIST_FOR_LABEL1", "RQSTDT", "RSLTDT", RQSTDT);

                return dtboxList;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        #endregion

        #region ( BOX �� ���� �̷� UPDATE )
        private void updateTB_SFC_LABEL_PRT_REQ_HIST(string sScanid = null, string sPRT_SEQ = null)
        {
            try
            {
                //DA_PRD_UPD_TB_SFC_LABEL_PRT_REQ_HIST_PRTFLAG
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("SCAN_ID", typeof(string));
                RQSTDT.Columns.Add("PRT_REQ_SEQNO", typeof(string));
                RQSTDT.Columns.Add("USERID", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["SCAN_ID"] = sScanid;
                dr["PRT_REQ_SEQNO"] = sPRT_SEQ;
                dr["USERID"] = LoginInfo.USERID;
                RQSTDT.Rows.Add(dr);

                new ClientProxy().ExecuteServiceSync("DA_PRD_UPD_TB_SFC_LABEL_PRT_REQ_HIST_PRTFLAG_USERID", "RQSTDT", "", RQSTDT);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        #endregion

        #endregion

        private void txtLotId_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            try
            {
                if (LoginInfo.CFG_SHOP_ID.Equals("G481"))
                {
                    if (e.Key == Key.V && Keyboard.IsKeyDown(Key.LeftCtrl))
                    {
                        string sPasteString = Clipboard.GetText();

                        if (!string.IsNullOrWhiteSpace(sPasteString))
                        {

                            Util.MessageInfo("SFU3180");
                            txtLotId.Clear();
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
        private void MenuItem_Copy_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Clipboard.SetText(txtLotId.SelectedText.ToString());
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void MenuItem_Cut_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Clipboard.SetText(txtLotId.SelectedText.ToString());
                txtLotId.Clear();

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void txtBoxId_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            try
            {
                if (LoginInfo.CFG_SHOP_ID.Equals("G481"))
                {
                    if (e.Key == Key.V && Keyboard.IsKeyDown(Key.LeftCtrl))
                    {
                        string sPasteString = Clipboard.GetText();

                        if (!string.IsNullOrWhiteSpace(sPasteString))
                        {

                            Util.MessageInfo("SFU3180");
                            txtBoxId.Clear();
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

        private void MenuItem_BoxId_Copy_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Clipboard.SetText(txtBoxId.SelectedText.ToString());
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void MenuItem_BoxId_Cut_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Clipboard.SetText(txtBoxId.SelectedText.ToString());
                txtBoxId.Clear();

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
    }
}
