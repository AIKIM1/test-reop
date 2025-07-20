/*************************************************************************************
 Created Date :
      Creator :
   Decription :
--------------------------------------------------------------------------------------
 [Change History]
  2020.06.16  ���Թ� : Initial Created.
  2021.11.16  ���� : Pack3�� Carrier - Pallet �������� ���� ��ȸ �÷��߰� �� Pallet�˻� Box CarrierID�ε� ��ȸ�����ϵ��� Biz����
  2023.04.13  ����ȫ : Pallet ȥ������ÿ��� Validation �ϱ� ���� GQMS_INTERLOCK_USE_CALL �Ķ���� �߰�
  2024.01.24  �輱�� : ������� ��������. Grid(OQC_INSP_RESULT)�÷��߰� -> ������Ҹ޽��� -> �������(BIZ����)
  2024.08.07  ���뼮 : ����ó���� ���尳�� ������ �Է��� ID�� ������ �ٸ��� ������ Interlock [��û��ȣ] E20240717-000988
**************************************************************************************/
using C1.WPF;
using C1.WPF.DataGrid;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using LGC.GMES.MES.PACK001.Class;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace LGC.GMES.MES.PACK001
{
    public partial class PACK001_076 : UserControl, IWorkArea
    {
        #region [ Global variable ]
        //Pallet ���ý� �޽��� ����
        string confirmMessage = string.Empty;

        LGC.GMES.MES.PACK001.Class.MESSAGE_PARAM ms = new LGC.GMES.MES.PACK001.Class.MESSAGE_PARAM();

        private Boolean chkWorkStrart = false;

        //���� ���� ����
        int boxLOTCount = 0;        // PALLET�� ��� BOX ���� OR ������� Control���� ������ ����
        int boxingBox_idx = 0;      // PALLET�� ��� BOX �� LOT ����

        #endregion

        #region [ Initialize ]
        public IFrameOperation FrameOperation
        {
            get;
            set;
        }

        public PACK001_076()
        {
            InitializeComponent();
            Initialize();
        }

        private void Initialize()
        {
            InitCombo();

            dtpDateFrom.SelectedDateTime = (DateTime)System.DateTime.Now.AddDays(-7); //������ �� ��¥
            dtpDateTo.SelectedDateTime = (DateTime)System.DateTime.Now;//���� ��¥
            if (LoginInfo.CFG_AREA_ID == "PA") dgPlthistory.Columns["CARRIERID"].Visibility = Visibility.Visible;
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
            string[] cboAreaFilter = { Area_Type.PACK };
            _combo.SetCombo(cboArea, CommonCombo.ComboStatus.NONE, cbChild: cboAreaChild, sFilter: cboAreaFilter, sCase: "AREA_PACK");

            //��
            C1ComboBox[] cboProductModelParent = { cboArea, cboEquipmentSegment };
            C1ComboBox[] cboProductModelChild = { cboProduct };
            _combo.SetCombo(cboProductModel, CommonCombo.ComboStatus.ALL, cbParent: cboProductModelParent, cbChild: cboProductModelChild, sCase: "PRJ_MODEL_AUTH");

            //��ǰ�ڵ�
            C1ComboBox[] cboProductParent = { cboSHOPID, cboArea, cboEquipmentSegment, cboProductModel, cboPrdtClass };
            _combo.SetCombo(cboProduct, CommonCombo.ComboStatus.ALL, cbParent: cboProductParent, sCase: "PRJ_PRODUCT");

            SetcboKind();

        }

        private void SetcboKind()
        {
            DataTable dtResult = new DataTable();
            dtResult.Columns.Add("KEY", typeof(string));
            dtResult.Columns.Add("VALUE", typeof(string));

            DataRow newRow = dtResult.NewRow();
            newRow = dtResult.NewRow();
            newRow.ItemArray = new object[] { "BOXID", "BOX" };
            dtResult.Rows.Add(newRow);

            newRow = dtResult.NewRow();
            newRow.ItemArray = new object[] { "LOTID", "LOT" };
            dtResult.Rows.Add(newRow);


            cboKind.ItemsSource = DataTableConverter.Convert(dtResult);
        }

        #endregion

        #region [ Event ]

        private void UserControl_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {

        }

        private void AddLOTforBoxing(string LOTID)
        {
            if (string.IsNullOrEmpty(LOTID))
            {
                Util.MessageInfo("SFU1813");
                return;
            }

            DataTable dtTemp = this.InputValidation(LOTID);
            if (!CommonVerify.HasTableRow(dtTemp))
            {
                return;
            }
            if (!dtTemp.Columns.Contains("CHK"))
            {
                dtTemp.Columns.Add("CHK", typeof(string));
            }

            // BMA ��ǰ�� ����ȵǰ� Interlock
            foreach(DataRowView dr in dtTemp.AsDataView())
            {
                if (dr["CLASS"].ToString() == "BMA")
                {
                    Util.MessageValidation("SFU3404");
                    return;
                }
            }

            // �����Ͱ� �������� ���� ���
            if (!chkWorkStrart)
            {
                WorkStart();
                Util.GridSetData(dgBoxLot, dtTemp, FrameOperation);
                txtMainWorkorderID.Text = dtTemp.Rows[0]["WOID"].ToString();
                txtMainEquipmentSegmentID.Text = dtTemp.Rows[0]["EQSGID"].ToString();
                txtMainProcessID.Text = dtTemp.Rows[0]["PROCID"].ToString();
                txtMainProductID.Text = dtTemp.Rows[0]["PRODID"].ToString();
                txtClass.Text = dtTemp.Rows[0]["CLASS"].ToString();
                txtProcessSegmentID.Text = dtTemp.Rows[0]["PCSGID"].ToString();

                if (dtTemp.Rows[0]["CLASS"].ToString().Equals("CMA") && cboKind.SelectedValue.ToString().Equals("LOT"))
                {
                    woHeadGrid.RowDefinitions[5].Height = new GridLength(5, GridUnitType.Star);
                    //CMA�� ������ ��� BOX ��ǰ ( ����ǰ ) �� �ڵ�� ���� ó�� �ؾ���
                    FindWoProd(txtMainProductID.Text.ToString(), txtMainEquipmentSegmentID.Text.ToString());
                }
                else
                {
                    woHeadGrid.RowDefinitions[5].Height = new GridLength(0);
                    txtBoxingProd.Text = dtTemp.Rows[0]["PRODID"].ToString();
                    if (!AutoPalettIdCreate(cboKind.SelectedValue.ToString(), txtBoxID.Text, txtMainEquipmentSegmentID.Text.ToString(), txtBoxingProd.Text.ToString()))
                    {
                        Reset();
                        return;
                    }
                }
            }
            else
            {
                if (!CheckBoxIDDuplicate(DataTableConverter.Convert(dgBoxLot.ItemsSource), dtTemp.Rows[0]["BOXID"].ToString()))
                {
                    txtBoxID.Text = string.Empty;
                    return;
                }

                if (dtTemp.Rows[0]["CLASS"].ToString().Equals("CMA") && cboKind.SelectedValue.ToString().Equals("LOT"))
                {
                    if (!txtMainProductID.Text.ToString().Equals(dtTemp.Rows[0]["PRODID"].ToString()))
                    {
                        Util.MessageInfo("SFU3457");
                        return;
                    }
                }
                else
                {
                    if (!txtBoxingProd.Text.ToString().Equals(dtTemp.Rows[0]["PRODID"].ToString()))
                    {
                        Util.MessageInfo("SFU3457");
                        return;
                    }
                }

                AddGridRow(dgBoxLot, dtTemp, false);
            }

            txtClass.Text = dtTemp.Rows[0]["CLASS"].ToString();
            txtBoxID.Text = string.Empty;
            Util.SetTextBlockText_DataGridRowCount(tbBoxingWait_cnt, Util.NVC(this.dgBoxLot.GetRowCount()));
            SetBoxingStatus(boxLOTCount, this.dgBoxLot.GetRowCount(), "������");

            if (dtTemp.Rows[0]["CLASS"].ToString().Equals("CMA") && cboKind.SelectedValue.ToString().Equals("LOT"))
            {
                MakeWorkorderListByEquipmentSegmentID(dtTemp);  // ���κ��� WO �����ؼ� PALLET ����� �̸� BOX ���� ó���Ҷ� WOID�� ���κ��� ���� ó��
            }
        }

        private void txtBoxID_KeyDown(object sender, KeyEventArgs e)
        {
            try
            {
                if (e.Key != Key.Enter)
                {
                    return;
                }

                if (this.boxLOTCount < this.dgBoxLot.GetRowCount() + 1)
                {
                    ms.AlertWarning("SFU3319", this.boxLOTCount.ToString()); // �Է¿��� : PALLET�� ���尡�� ���� %1�� �Ѿ����ϴ�. [������� ���� �� LOT �Է�]
                    return;
                }
                this.AddLOTforBoxing(this.txtBoxID.Text);
            }
            catch (Exception ex)
            {
                Reset();
                Util.MessageException(ex);
            }
        }

        private void WorkStart()
        {
            try
            {
                //TEXT BOX Default Ȯ�� �ѹ� �ʿ�
                //txtStauts.Text = ObjectDic.Instance.GetObjectName("�۾���");
                string boxingStatus = ObjectDic.Instance.GetObjectName("������");

                //ComboBox
                cboKind.IsEnabled = false;

                //���� �۾� ���� ����
                chkWorkStrart = true;

                SetBoxingStatus(boxLOTCount, boxingBox_idx, boxingStatus);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void Reset()
        {
            try
            {
                //TEXT BOX �ʱ�ȭ
                txtPltId.Text = string.Empty;
                txtBoxID.Text = string.Empty;
                txtClass.Text = string.Empty;
                txtProcessSegmentID.Text = string.Empty;
                txtMainEquipmentSegmentID.Text = string.Empty;
                txtMainProcessID.Text = string.Empty;
                txtMainProductID.Text = string.Empty;
                txtBoxingProd.Text = string.Empty;

                //���� �۾� ���� ����
                chkWorkStrart = false;
                nbBoxingCnt.Value = 0;
                boxLOTCount = 0;
                cboKind.IsEnabled = false;
                txtBoxID.IsEnabled = false;

                //DATAGRID �ʱ�ȭ
                Util.gridClear(dgBoxWo);
                Util.gridClear(dgBoxLot);
                Util.SetTextBlockText_DataGridRowCount(tbBoxingWait_cnt, Util.NVC(dgBoxLot.GetRowCount()));
                this.SetBoxingStatus(boxLOTCount, this.dgBoxLot.GetRowCount(), "������");
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void PackingReSet()
        {
            try
            {
                //TEXT BOX �ʱ�ȭ
                txtPltId.Text = "";
                txtBoxID.Text = "";
                txtClass.Text = "";
                txtProcessSegmentID.Text = "";
                txtMainEquipmentSegmentID.Text = "";
                txtMainProcessID.Text = "";
                txtMainProductID.Text = "";
                txtBoxingProd.Text = "";

                //���� �۾� ���� ����
                chkWorkStrart = false;

                this.nbBoxingCnt.Value = (double)boxLOTCount;
                this.txtRemainCount.Text = boxLOTCount.ToString();
                cboKind.IsEnabled = true;
                this.txtBoxID.IsEnabled = true;

                //DATAGRID �ʱ�ȭ
                Util.gridClear(dgBoxWo);
                Util.gridClear(dgBoxLot);
                Util.SetTextBlockText_DataGridRowCount(tbBoxingWait_cnt, Util.NVC(dgBoxLot.GetRowCount()));
                this.SetBoxingStatus(boxLOTCount, this.dgBoxLot.GetRowCount(), "������");
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void MakeWorkorderListByEquipmentSegmentID(DataTable dt)
        {
            try
            {
                if (dt == null || dt.Rows.Count == 0) return;


                DataTable dtData = new DataTable();
                dtData.Columns.Add("EQSGID", typeof(string));
                dtData.Columns.Add("EQSGNAME", typeof(string));
                dtData.Columns.Add("PRODID", typeof(string));
                dtData.Columns.Add("CLASS", typeof(string));

                DataRow newRow = null;

                newRow = dtData.NewRow();
                newRow["EQSGID"] = dt.Rows[0]["EQSGID"].ToString();
                newRow["EQSGNAME"] = dt.Rows[0]["EQSGNAME"].ToString();
                newRow["PRODID"] = dt.Rows[0]["PRODID"].ToString();
                newRow["CLASS"] = dt.Rows[0]["CLASS"].ToString();

                dtData.Rows.Add(newRow);

                int iChkOverLap = 0;

                if (dgBoxWo.GetRowCount() != 0)
                {
                    iChkOverLap = ((DataView)dgBoxWo.ItemsSource).Table.Select("EQSGID = '" + dtData.Rows[0]["EQSGID"].ToString() + "'").Length;
                }

                if (iChkOverLap == 0)
                {
                    AddWorkorderGridRow(dgBoxWo, dtData, false);
                }

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void WorkOrerPopup(string strBoxProdId, string strProdId, string strProdClassCode, string strEqsgId)
        {
            try
            {
                PACK001_015_WORKORDERSELECT popup = new PACK001_015_WORKORDERSELECT();
                popup.FrameOperation = this.FrameOperation;

                if (popup != null)
                {
                    DataTable dtData = new DataTable();
                    dtData.Columns.Add("PRODID", typeof(string));
                    dtData.Columns.Add("PRODID_LOT", typeof(string));
                    dtData.Columns.Add("PROD_CLSS_CODE", typeof(string));
                    //2018.10.12
                    dtData.Columns.Add("EQSGID", typeof(string));

                    DataRow newRow = null;

                    newRow = dtData.NewRow();
                    newRow["PRODID"] = strBoxProdId;
                    newRow["PRODID_LOT"] = strProdId;
                    newRow["PROD_CLSS_CODE"] = strProdClassCode;
                    newRow["EQSGID"] = strEqsgId;

                    dtData.Rows.Add(newRow);

                    //========================================================================
                    object[] Parameters = new object[1];
                    Parameters[0] = dtData;
                    C1WindowExtension.SetParameters(popup, Parameters);
                    //========================================================================

                    popup.Closed -= Popup_Closed;
                    popup.Closed += Popup_Closed;
                    popup.ShowModal();
                    popup.CenterOnScreen();
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        void Popup_Closed(object sender, EventArgs e)
        {
            try
            {
                PACK001_015_WORKORDERSELECT popup = sender as PACK001_015_WORKORDERSELECT;

                if (popup.DialogResult == MessageBoxResult.OK)
                {
                    popup.EQSGID.ToString();
                    popup.WOID.ToString();
                    popup.EQSGNAME.ToString();
                    popup.PRODID.ToString();


                    DataTable dt = DataTableConverter.Convert(dgBoxWo.ItemsSource);


                    if (dt.Columns.Contains("BOX_PRODID"))
                    {

                        List<string> list = null;

                        list = dt.AsEnumerable().Where(x => x.Field<string>("BOX_PRODID") != popup.PRODID.ToString() && x.Field<string>("EQSGID") != popup.EQSGID.ToString()).Select(x => x.Field<string>("BOX_PRODID")).ToList();

                        if (list.Count() != 0)
                        {
                            if (list[0] != null)
                            {
                                Util.MessageValidation("SFU3266", popup.PRODID.ToString(), list[0].ToString());
                                return;
                            }

                        }
                    }

                    if (!dt.Columns.Contains("WOID"))
                    {
                        dt.Columns.Add("WOID", typeof(string));
                    }

                    if (!dt.Columns.Contains("BOX_PRODID"))
                    {
                        dt.Columns.Add("BOX_PRODID", typeof(string));
                    }

                    int iEqsgRow = Util.gridFindDataRow(ref dgBoxWo, "EQSGID", popup.EQSGID.ToString(), false);

                    if (iEqsgRow != -1)
                    {
                        dt.Rows[iEqsgRow]["WOID"] = popup.WOID.ToString();
                        dt.Rows[iEqsgRow]["BOX_PRODID"] = popup.PRODID.ToString();
                        txtBoxingProd.Text = popup.PRODID.ToString();

                        if (!AutoPalettIdCreate(cboKind.SelectedValue.ToString(), txtBoxID.Text, txtMainEquipmentSegmentID.Text.ToString(), txtBoxingProd.Text.ToString()))
                        {
                            dt.Rows[iEqsgRow]["WOID"] = "";
                            dt.Rows[iEqsgRow]["BOX_PRODID"] = "";
                            txtBoxingProd.Text = "";
                            return;
                        }
                    }


                    Util.GridSetData(dgBoxWo, dt, FrameOperation, false);
                }
            }
            catch (Exception ex)
            {
                //Util.Alert(ex.ToString());
                Util.MessageException(ex);
            }
        }


        private void AddWorkorderGridRow(C1DataGrid c1DataGrid, DataTable dt, bool isAutoWidth)
        {
            if (c1DataGrid.ItemsSource == null)
            {
                Util.GridSetData(c1DataGrid, dt, FrameOperation, isAutoWidth);
            }
            else
            {
                DataTable dtTemp = DataTableConverter.Convert(c1DataGrid.ItemsSource);
                dt.AsEnumerable().CopyToDataTable(dtTemp, LoadOption.Upsert);
                Util.GridSetData(c1DataGrid, dtTemp, FrameOperation, isAutoWidth);
            }
        }

        private void AddGridRow(C1DataGrid c1DataGrid, DataTable dt, bool isAutoWidth)
        {
            try
            {
                this.AddWorkorderGridRow(c1DataGrid, dt, isAutoWidth);
                boxingBox_idx++;
                txtBoxID.Text = string.Empty;
                Util.SetTextBlockText_DataGridRowCount(tbBoxingWait_cnt, Util.NVC(dgBoxLot.GetRowCount()));
                SetBoxingStatus(boxLOTCount, this.dgBoxLot.GetRowCount(), "������");
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void BtnChange_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Button btn = sender as Button;
                int iRow = ((C1.WPF.DataGrid.DataGridCellPresenter)btn.Parent).Row.Index;

                DataTable dtTem = DataTableConverter.Convert(dgBoxWo.ItemsSource);

                string strBoxProdId = txtBoxingProd.Text.ToString();
                string strProdId = DataTableConverter.GetValue(dgBoxWo.Rows[iRow].DataItem, "PRODID").ToString();
                string strProdClassCode = DataTableConverter.GetValue(dgBoxWo.Rows[iRow].DataItem, "CLASS").ToString();
                string strEqsgId = DataTableConverter.GetValue(dgBoxWo.Rows[iRow].DataItem, "EQSGID").ToString();

                WorkOrerPopup(strBoxProdId, strProdId, strProdClassCode, strEqsgId);

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void BtnSelectCacel_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (this.dgBoxLot.GetRowCount() <= 0)
                {
                    return;
                }

                for (int i = this.dgBoxLot.GetRowCount(); i > 0; i--)
                {
                    if (!Convert.ToBoolean(DataTableConverter.GetValue(dgBoxLot.Rows[i - 1].DataItem, "CHK")))
                    {
                        continue;
                    }
                    string equipmentSegmentID = DataTableConverter.GetValue(dgBoxLot.Rows[i - 1].DataItem, "EQSGID").ToString();
                    this.dgBoxLot.EndNewRow(true);
                    this.dgBoxLot.RemoveRow(i - 1);
                    string boxingStatus = (this.dgBoxLot.GetRowCount() <= 0) ? "�����" : "������";
                    SetBoxingStatus(boxLOTCount, this.dgBoxLot.GetRowCount(), boxingStatus);

                    // Workorder Data �����
                    if (cboKind.SelectedValue.ToString().Equals("LOT") && txtClass.Text.ToString().Equals("CMA"))
                    {
                        if (DataTableConverter.Convert(dgBoxLot.ItemsSource).Select("EQSGID ='" + equipmentSegmentID + "'").Length == 0)
                        {
                            DataTable dtBoxWo = DataTableConverter.Convert(dgBoxWo.ItemsSource);
                            dtBoxWo.AsEnumerable().Where(x => x.Field<string>("EQSGID").ToUpper().Equals(equipmentSegmentID)).ToList().ForEach(r => r.Delete());
                            Util.GridSetData(dgBoxWo, dtBoxWo, FrameOperation, false);
                        }
                    }
                }
                Util.SetTextBlockText_DataGridRowCount(tbBoxingWait_cnt, Convert.ToString(this.dgBoxLot.GetRowCount()));
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void BtnReset_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (DataTableConverter.Convert(dgBoxLot.ItemsSource).Rows.Count == 0)
                {
                    Reset();
                    ms.AlertInfo("SFU3377");
                }
                else
                {
                    LGC.GMES.MES.ControlsLibrary.MessageBox.Show(ms.AlertRetun("SFU3282"), null, "CAUSE", MessageBoxButton.OKCancel, MessageBoxIcon.Warning, (caution_result) =>
                    //�۾����� : �������� �۾��� �ֽ��ϴ�. ���� [�۾��ʱ�ȭ] �Ͻðڽ��ϱ�?
                    {
                        if (caution_result == MessageBoxResult.OK)
                        {
                            Reset();
                            Util.MessageInfo("SFU3377");
                        }
                        else
                        {
                            return;
                        }
                    });
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private Boolean CheckBoxIDDuplicate(DataTable dt, string BoxID)
        {
            try
            {
                if (dt.Select("BOXID ='" + BoxID + "'").Length > 0)
                {
                    Util.MessageInfo("SFU1508");
                    this.loadingIndicator.Visibility = Visibility.Collapsed;
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

        private void btnPack_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (cboKind.SelectedValue.ToString().Equals("LOT") && txtClass.Text.ToString().Equals("CMA"))
                {
                    DataTable dtBoxLot = DataTableConverter.Convert(dgBoxLot.ItemsSource);
                    DataTable dtBoxWo = DataTableConverter.Convert(dgBoxWo.ItemsSource);

                    if (!CommonVerify.HasTableRow(dtBoxLot))
                    {
                        Util.MessageInfo("SFU1177");
                        return;
                    }

                    if (!CommonVerify.HasTableRow(dtBoxWo) || !dtBoxWo.Columns.Contains("WOID"))
                    {
                        Util.MessageInfo("SFU1436");
                        return;
                    }

                    DataTable distinctTable = dtBoxLot.DefaultView.ToTable(true, new string[] { "EQSGID" });

                    foreach (DataRow row in distinctTable.Rows)
                    {
                        if (dtBoxWo.Select("EQSGID = '" + row.ItemArray[0].ToString() + "' AND WOID IS NULL").Length > 0)
                        {
                            Util.MessageInfo("SFU1436");
                            return;
                        }
                    }
                }

                if (boxLOTCount == this.dgBoxLot.GetRowCount())
                {
                    this.BoxingEnd();       // ���� �Ϸ� �Լ�
                }
                else
                {
                    Util.MessageValidation("SFU3392");
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }


        private string GetLotTotal_qty()
        {
            try
            {
                int roof_cnt = dgBoxLot.GetRowCount();
                int lotTotal_cnt = 0;

                for (int i = 0; i < roof_cnt; i++)
                {
                    string inboxid = DataTableConverter.GetValue(dgBoxLot.Rows[i].DataItem, "BOXID").ToString();

                    DataTable INDATA = new DataTable();
                    INDATA.TableName = "INDATA";
                    INDATA.Columns.Add("BOXID", typeof(string));

                    DataRow dr = INDATA.NewRow();
                    dr["BOXID"] = inboxid;

                    INDATA.Rows.Add(dr);

                    DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_PALLET_OF_TOTAL_LOT_QTY", "INDATA", "OUTDATA", INDATA);

                    lotTotal_cnt += Convert.ToInt32(dtResult.Rows[0]["TOTAL_QTY"]);

                }

                return lotTotal_cnt.ToString();
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private Boolean DtDateCompare()
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

        private void BtnSearch_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (string.IsNullOrEmpty(cboArea.SelectedValue.ToString()))
                {
                    ms.AlertWarning("SFU1499"); //���� �����ϼ���.
                    return;
                }

                if (!DtDateCompare())
                {
                    return;
                }

                Search();
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void TxtSearchBox_KeyDown(object sender, KeyEventArgs e)
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
                    Util.MessageException(ex);
                }
            }
        }


        private void DgPlthistory_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            try
            {
                if (dgPlthistory == null || dgPlthistory.Rows.Count == 0)
                {
                    return;
                }

                Point pnt = e.GetPosition(null);
                C1.WPF.DataGrid.DataGridCell cell = dgPlthistory.GetCellFromPoint(pnt);

                if (cell == null || cell.Value == null)
                {
                    return;
                }

                int currentRow = cell.Row.Index;
                int _col = cell.Column.Index;
                string value = cell.Value.ToString();

                DataTable dt = DataTableConverter.Convert(dgPlthistory.ItemsSource);

                txtPltIdRight.Text = dt.Rows[currentRow]["PALLETID"].ToString();
                txtEqsgIdRight.Text = dt.Rows[currentRow]["EQSGID"].ToString();
                txtEqptIdRight.Text = dt.Rows[currentRow]["EQPTID"].ToString();
                txtProdIdRight.Text = dt.Rows[currentRow]["PRODID"].ToString();
                txtProcIdRight.Text = dt.Rows[currentRow]["PROCID"].ToString();
                txtProdClassRight.Text = dt.Rows[currentRow]["PRDCLASS"].ToString();
                txtBoxQtyRight.Text = dt.Rows[currentRow]["PALLETBOXCNT"].ToString();
                txtOqcIdRight.Text = dt.Rows[currentRow]["OQC_INSP_REQ_ID"].ToString();
                txtRcvIssIdRight.Text = dt.Rows[currentRow]["RCV_ISS_ID"].ToString();

                #region OQC�˻���
                //������ü�� �˻����� ���� Msg�ٸ��� �����ֱ� ���� ����(2024.01.23 seonjun9)
                if (null != dt.Rows[currentRow]["OQC_INSP_REQ_ID"] && dt.Rows[currentRow]["OQC_INSP_REQ_ID"].ToString().Length > 0)
                {
                    if (null != dt.Rows[currentRow]["OQC_INSP_RESULT"] && dt.Rows[currentRow]["OQC_INSP_RESULT"].ToString().Equals("F"))
                    {
                        //_Msg_OkNg = "���� ������� �Ͻðڽ��ϱ�?";
                        confirmMessage = ms.AlertRetun("SFU3135"); //������� �Ͻðڽ��ϱ�?
                    }
                    else
                    {
                        //okNg_msg = "���ϰ˻� ����� [���հ�]�� �ƴմϴ�.\n��� ���� ��� �Ͻðڽ��ϱ�?";
                        confirmMessage = ms.AlertRetun("SFU3322"); //�۾����� : ������ PALLET�� ���� �˻� ����� [���հ�]�� �ƴմϴ�. ������� �Ͻðڽ��ϱ�?
                    }
                }
                else
                {
                    //_Msg_OkNg = "���� ������� �Ͻðڽ��ϱ�?";
                    confirmMessage = ms.AlertRetun("SFU3135"); //������� �Ͻðڽ��ϱ�?
                }
                #endregion
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void DgPlthistory_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            try
            {
                Point pnt = e.GetPosition(null);
                C1.WPF.DataGrid.DataGridCell cell = dgPlthistory.GetCellFromPoint(pnt);
                GridDoubleClickProcess(cell, "INFO_BOX");
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void GridDoubleClickProcess(C1.WPF.DataGrid.DataGridCell cell, string sPopUp_Flag)
        {
            try
            {
                if (cell != null)
                {
                    if (cell.Row.Index > -1)
                    {
                        if (cell.Column.Name == "LOTID")
                        {
                            this.FrameOperation.OpenMenu("SFU010090090", true, cell.Text);
                        }

                        if (cell.Column.Name == "BOXID" || cell.Column.Name == "PALLETID")
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
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private void DgPlthistory_LoadedCellPresenter(object sender, DataGridCellEventArgs e)
        {
            try
            {
                C1.WPF.DataGrid.C1DataGrid dataGrid = sender as C1.WPF.DataGrid.C1DataGrid;

                dataGrid.Dispatcher.BeginInvoke(new Action(() =>
                {
                    if (e.Cell.Presenter == null)
                    {
                        return;
                    }
                    if (e.Cell.Column.Name == "PALLETID")
                    {
                        e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Blue);
                        e.Cell.Presenter.FontWeight = FontWeights.Bold;
                    }
                    else
                    {
                        e.Cell.Presenter.Foreground = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#FF333333"));
                        e.Cell.Presenter.FontWeight = FontWeights.Normal;
                    }
                }));
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private void BtnPacCancel_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                unPack_Process(sender);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void BtnPalletLabel_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (txtPltIdRight.Text.Length > 0)
                {
                    setTagReport();
                }

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void setTagReport()
        {
            PackCommon.ShowPalletTag(this.GetType().Name, txtPltIdRight.Text.ToString(), txtEqptIdRight.Text.ToString(), string.Empty);
        }
        #endregion

        #region [ Biz Actor ]

        private DataTable InputValidation(string boxID = null)
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("SRCTYPE", typeof(string));
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("BOXID", typeof(string));
                RQSTDT.Columns.Add("BOXTYPE", typeof(string));
                RQSTDT.Columns.Add("BOX_PRODID", typeof(string));
                RQSTDT.Columns.Add("PRDT_CLSS", typeof(string));
                RQSTDT.Columns.Add("EQSG_MIX", typeof(string));
                RQSTDT.Columns.Add("GQMS_INTERLOCK_USE_CALL", typeof(string)); // 2023.04.13 ����ȫ
                RQSTDT.Columns.Add("AREAID", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                dr["LANGID"] = LoginInfo.LANGID;
                dr["BOXID"] = string.IsNullOrWhiteSpace(boxID) ? txtBoxID.Text.ToString() : boxID;
                dr["BOXTYPE"] = Util.GetCondition(cboKind);
                dr["BOX_PRODID"] = string.IsNullOrEmpty(txtBoxingProd.Text.ToString()) ? null : txtBoxingProd.Text.ToString();
                dr["PRDT_CLSS"] = string.IsNullOrEmpty(txtClass.Text.ToString()) ? null : txtClass.Text.ToString();
                // Pallet Mix�� �⺻���� �ϴ� ȭ���̱⿡, ����ÿ� �׻� ������ MIX �����ϵ��� ����
                dr["EQSG_MIX"] = "True";
                dr["GQMS_INTERLOCK_USE_CALL"] = "Y"; // 2023.04.13 ����ȫ
                dr["AREAID"] = LoginInfo.CFG_AREA_ID;

                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("BR_PRD_CHK_BOXLOT_PALLET", "INDATA", "OUTDATA", RQSTDT); //�ڽ��� �ִ��� Ȯ��

                if (dtResult.Rows.Count > 0)
                {
                    return dtResult;
                }
                else
                {
                    Util.MessageInfo("SFU3327");
                    return null;
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return null;
            }
        }

        private void FindWoProd(string strProdId, string strEqsgId)
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("PRODID", typeof(string));
                RQSTDT.Columns.Add("EQSGID", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["PRODID"] = strProdId;
                dr["EQSGID"] = strEqsgId;

                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_CMA_BOX_ORDER_FIND", "INDATA", "OUTDATA", RQSTDT);

                if (dtResult.Rows.Count > 0)
                {
                    txtBoxingProd.Text = dtResult.Rows[0]["PRODID"].ToString();
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private void BoxingEnd()
        {
            try
            {
                if (string.IsNullOrEmpty(txtPltId.Text.ToString()))
                {
                    Util.MessageInfo("SFU8327");
                    return;
                }

                if (dgBoxLot.GetRowCount() == 0)
                {
                    Util.MessageInfo("SFU8328");
                    return;
                }

                string palletID = Util.GetCondition(txtPltId);
                string strTotalQty = string.Empty;
                string strProcId = string.Empty;

                if (cboKind.SelectedValue.ToString() == "BOX")
                {
                    strTotalQty = GetLotTotal_qty();
                }
                else
                {
                    strTotalQty = (dgBoxLot.GetRowCount()).ToString();
                }

                if (txtClass.Text.ToString() == "CMA")
                {
                    strProcId = "P5500";
                }
                else
                {
                    strProcId = "P9500";
                }


                DataSet indataSet = new DataSet();

                DataTable INDATA = indataSet.Tables.Add("INDATA");
                INDATA.Columns.Add("SRCTYPE", typeof(string));  //INPUT TYPE (UI OR EQ)
                INDATA.Columns.Add("BOXID", typeof(string));    //palletid
                INDATA.Columns.Add("PACK_EQSGID", typeof(string));   //����ID(������ ������ ����)
                INDATA.Columns.Add("PACK_EQPTID", typeof(string));   //����ID(������ ������ ����)
                INDATA.Columns.Add("PRODID", typeof(string));   //����ID(������ ������ ����)
                INDATA.Columns.Add("BOXLAYER", typeof(Int32));   //����ID(������ ������ ����)
                INDATA.Columns.Add("BOX_QTY", typeof(Int32));   //���� �Ѽ���
                INDATA.Columns.Add("BOX_QTY2", typeof(Int32));   //���� �Ѽ���
                INDATA.Columns.Add("USERID", typeof(string));   //�����ID
                INDATA.Columns.Add("NOTE", typeof(string));   //��Ʈ
                INDATA.Columns.Add("PROCID", typeof(string));   //����ID
                INDATA.Columns.Add("WOID", typeof(string));   //WOID ù��° �Է��� LOT�� WO
                INDATA.Columns.Add("CUST_PALLETID", typeof(string));   //���� PLT ID

                INDATA.Columns.Add("EQSG_MIX", typeof(string));   //���� ȥ�� ����
                INDATA.Columns.Add("AREAID", typeof(string));

                DataRow dr = INDATA.NewRow();
                dr["SRCTYPE"] = "UI";
                dr["BOXID"] = palletID;
                dr["PACK_EQSGID"] = txtMainEquipmentSegmentID.Text.ToString();
                dr["PACK_EQPTID"] = "PLT MIX UI";
                if (cboKind.SelectedValue.ToString() == "LOT" && txtClass.Text.ToString() == "CMA")
                {
                    dr["PRODID"] = txtBoxingProd.Text.ToString();
                }
                else
                {
                    dr["PRODID"] = txtMainProductID.Text.ToString();
                }
                dr["BOXLAYER"] = 2;
                dr["BOX_QTY"] = strTotalQty; // dgPalletBox.GetRowCount().ToString(); //���� ����Ǵ� ����
                dr["BOX_QTY2"] = strTotalQty;// dgPalletBox.GetRowCount().ToString(); //���� ����Ǵ� ����
                dr["USERID"] = LoginInfo.USERID;
                dr["NOTE"] = "PLT MIX UI";
                dr["PROCID"] = strProcId;
                dr["WOID"] = txtMainWorkorderID.Text.ToString();
                dr["EQSG_MIX"] = "True";
                dr["AREAID"] = LoginInfo.CFG_AREA_ID;

                //��� ���� Ȯ�� �ʿ�
                //2021-01-28 �ֿ켮P, ���Թ�S, ���Ժ�S ���ó�� Ȯ���� �ȵ�
                //�ٸ� �������� ������ ���� ����
                dr["CUST_PALLETID"] = "";

                INDATA.Rows.Add(dr);

                DataTable ININNERBOX = indataSet.Tables.Add("ININNERBOX");
                ININNERBOX.Columns.Add("BOXID", typeof(string));

                DataTable LOTDATA = indataSet.Tables.Add("LOTDATA");
                LOTDATA.Columns.Add("LOTID", typeof(string));
                LOTDATA.Columns.Add("WOID", typeof(string));

                //LOTID�� �ȷ��� �������� BOXID�� �ȷ��� �������� �Ǵ�
                if (cboKind.SelectedValue.ToString() == "BOX")
                {
                    for (int i = 0; i < dgBoxLot.GetRowCount(); i++)
                    {
                        string sBoxId = Util.NVC(dgBoxLot.GetCell(i, dgBoxLot.Columns["BOXID"].Index).Value);

                        DataRow inDataDtl = ININNERBOX.NewRow();
                        inDataDtl["BOXID"] = sBoxId;
                        ININNERBOX.Rows.Add(inDataDtl);
                    }
                }
                else
                {
                    for (int i = 0; i < dgBoxLot.GetRowCount(); i++)
                    {
                        DataRow[] tempRows = null;

                        if (txtClass.Text.ToString().Equals("CMA"))
                        {
                            DataTable dtBoxWo = DataTableConverter.Convert(dgBoxWo.ItemsSource);
                            tempRows = dtBoxWo.Select("EQSGID = '" + Util.NVC(dgBoxLot.GetCell(i, dgBoxLot.Columns["EQSGID"].Index).Value) + "'");

                            if (tempRows.Length == 0)
                            {
                                Util.MessageInfo("SFU1436");
                                return;
                            }
                        }

                        string strLotId = Util.NVC(dgBoxLot.GetCell(i, dgBoxLot.Columns["BOXID"].Index).Value);
                        string strWoId = Util.NVC(dgBoxLot.GetCell(i, dgBoxLot.Columns["WOID"].Index).Value);

                        DataRow inDataDtl = LOTDATA.NewRow();
                        inDataDtl["LOTID"] = strLotId;
                        if (txtClass.Text.ToString().Equals("CMA"))
                        {
                            inDataDtl["WOID"] = tempRows[0]["WOID"].ToString();
                        }
                        else
                        {
                            inDataDtl["WOID"] = strWoId;
                        }


                        LOTDATA.Rows.Add(inDataDtl);
                    }
                }

                DataSet dsResult = new ClientProxy().ExecuteServiceSync_Multi("BR_PRD_REG_BOXING_PALLET", "INDATA,ININNERBOX,LOTDATA", "OUTDATA,OUT_LOTID", indataSet);

                if (dsResult != null && dsResult.Tables["OUTDATA"].Rows.Count > 0)
                {
                    SearchBox(palletID, false);

                    //�ʱ�ȭ
                    PackingReSet();
                }
                else
                {
                    throw new Exception();
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private Boolean AutoPalettIdCreate(string strCboKind, string strLotId, string strEqsgId, string strProdId)
        {
            try
            {
                string setProcId = string.Empty;
                string prodId = string.Empty;

                if (Util.GetCondition(cboKind.SelectedValue.ToString()) == "BOX")
                {
                    setProcId = txtMainProcessID.Text.ToString();
                }
                else
                {
                    if (txtClass.Text.ToString() == "CMA")
                    {
                        setProcId = "P5500";
                    }
                    else if (txtClass.Text.ToString() == "BMA")
                    {
                        setProcId = "P9500";
                    }
                }

                DataTable dtPALLET = new DataTable(); //plletid ������ return

                //pallet ���� ����
                dtPALLET.TableName = "dtPALLET";
                dtPALLET.Columns.Add("SRCTYPE", typeof(string));  //INPUT TYPE (UI OR EQ)
                dtPALLET.Columns.Add("LANGID", typeof(string));   //LANGUAGE ID
                dtPALLET.Columns.Add("PROCID", typeof(string));   //����ID(������ ������ ����)
                dtPALLET.Columns.Add("BOXQTY", typeof(Int32));    //���� �Ѽ���
                dtPALLET.Columns.Add("EQSGID", typeof(string));   //����ID
                dtPALLET.Columns.Add("USERID", typeof(string));   //�����ID
                dtPALLET.Columns.Add("LOTID", typeof(string));    //LOTID �Ǵ� BOXID
                dtPALLET.Columns.Add("GUBUN", typeof(string));    //LOTID �Ǵ� BOXID
                dtPALLET.Columns.Add("PRODID", typeof(string));    //LOTID �Ǵ� BOXID

                DataRow drPALLET = dtPALLET.NewRow();
                drPALLET["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                drPALLET["LANGID"] = LoginInfo.LANGID;
                drPALLET["PROCID"] = setProcId; // Util.GetCondition(cboGubun) == "Box ID" ? box_proc : lot_proc;
                drPALLET["BOXQTY"] = "1";
                drPALLET["EQSGID"] = strEqsgId;
                drPALLET["USERID"] = LoginInfo.USERID;
                drPALLET["LOTID"] = strLotId;
                drPALLET["GUBUN"] = strCboKind;
                drPALLET["PRODID"] = strProdId;

                dtPALLET.Rows.Add(drPALLET);

                DataTable dtPalletResult = new ClientProxy().ExecuteServiceSync("BR_PRD_GET_BOXIDREQUEST_PALLET", "INDATA", "OUTDATA", dtPALLET);

                if (dtPalletResult != null && dtPalletResult.Rows.Count != 0)
                {
                    txtPltId.Text = dtPalletResult.Rows[0]["BOXID"].ToString();
                    return true;
                }
                else
                {
                    Util.MessageInfo("SFU8326");
                    return false;
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private void Search()
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
                RQSTDT.Columns.Add("PALLETID", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["AREAID"] = Util.GetCondition(cboArea);
                dr["PRODID"] = Util.GetCondition(cboProduct) == "" ? null : Util.GetCondition(cboProduct);
                dr["MODLID"] = Util.GetCondition(cboProductModel) == "" ? null : Util.GetCondition(cboProductModel);
                dr["FROMDATE"] = dtpDateFrom.SelectedDateTime.ToString("yyyy-MM-dd");
                dr["TODATE"] = dtpDateTo.SelectedDateTime.ToString("yyyy-MM-dd");
                dr["PALLETID"] = null;

                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_PALLETHISTORY_SEARCH", "INDATA", "OUTDATA", RQSTDT);

                dgPlthistory.ItemsSource = null;
                txtSearchBox.Text = "";

                if (dtResult.Rows.Count != 0)
                {
                    Util.GridSetData(dgPlthistory, dtResult, FrameOperation);
                    txtSearchBox.Text = string.Empty;
                }

                txtPltIdRight.Text = string.Empty;
                txtEqsgIdRight.Text = string.Empty;
                txtEqptIdRight.Text = string.Empty;
                txtProdIdRight.Text = string.Empty;
                txtProcIdRight.Text = string.Empty;
                txtProdClassRight.Text = string.Empty;
                txtBoxQtyRight.Text = string.Empty;
                txtOqcIdRight.Text = string.Empty;
                txtRcvIssIdRight.Text = string.Empty;

                Util.SetTextBlockText_DataGridRowCount(tbBoxHistory_cnt, Util.NVC(dtResult.Rows.Count));
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private void SearchBox(string strPalletId, Boolean bGridClear)
        {
            try
            {
                DataTable dt = DataTableConverter.Convert(dgPlthistory.ItemsSource);

                if (dt.Rows.Count > 0)
                {
                    if (dt.Select("PALLETID = '" + strPalletId + "'").Count() > 0 && !bGridClear)
                    {
                        Util.MessageInfo("SFU8251");
                        return;
                    }
                }

                DataTable INDATA = new DataTable();
                INDATA.TableName = "RQSTDT";
                INDATA.Columns.Add("LANGID", typeof(string));
                INDATA.Columns.Add("AREAID", typeof(string));
                INDATA.Columns.Add("PRODID", typeof(string));
                INDATA.Columns.Add("MODLID", typeof(string));
                INDATA.Columns.Add("FROMDATE", typeof(string));
                INDATA.Columns.Add("TODATE", typeof(string));
                INDATA.Columns.Add("PALLETID", typeof(string));


                DataRow dr = INDATA.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["AREAID"] = null;
                dr["PRODID"] = null;
                dr["MODLID"] = null;
                dr["FROMDATE"] = null;
                dr["TODATE"] = null;
                dr["PALLETID"] = strPalletId;

                INDATA.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("BR_PRD_SEL_PALLETHISTORY_SEARCH", "INDATA", "OUTDATA", INDATA);

                dgPlthistory.ItemsSource = null;
                txtSearchBox.Text = "";


                if (dtResult.Rows.Count != 0)
                {
                    if (bGridClear)
                    {
                        txtSearchBox.Text = "";
                        Util.GridSetData(dgPlthistory, dtResult, FrameOperation);
                    }
                    else
                    {
                        txtSearchBox.Text = "";

                        Util.gridClear(dgPlthistory);

                        dt.AsEnumerable().CopyToDataTable(dtResult, LoadOption.Upsert);

                        Util.GridSetData(dgPlthistory, dtResult, FrameOperation);

                        Util.SetTextBlockText_DataGridRowCount(tbBoxHistory_cnt, Util.NVC(dtResult.Rows.Count));
                    }
                }

                txtPltIdRight.Text = string.Empty;
                txtEqsgIdRight.Text = string.Empty;
                txtEqptIdRight.Text = string.Empty;
                txtProdIdRight.Text = string.Empty;
                txtProcIdRight.Text = string.Empty;
                txtProdClassRight.Text = string.Empty;
                txtBoxQtyRight.Text = string.Empty;
                txtOqcIdRight.Text = string.Empty;
                txtRcvIssIdRight.Text = string.Empty;
                confirmMessage = string.Empty;

                Util.SetTextBlockText_DataGridRowCount(tbBoxHistory_cnt, Util.NVC(dtResult.Rows.Count));
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private void Pallet_unpack(object sender)
        {
            try
            {
                Button btn = sender as Button;

                DataTable INDATA = new DataTable();
                INDATA.TableName = "INDATA";
                INDATA.Columns.Add("SRCTYPE", typeof(string));
                INDATA.Columns.Add("BOXID", typeof(string));
                INDATA.Columns.Add("PRODID", typeof(string));
                INDATA.Columns.Add("UNPACK_QTY", typeof(string));
                INDATA.Columns.Add("UNPACK_QTY2", typeof(string));
                INDATA.Columns.Add("NOTE", typeof(string));
                INDATA.Columns.Add("USERID", typeof(string));
                INDATA.Columns.Add("EQSGID", typeof(string));
                INDATA.Columns.Add("EQPTID", typeof(string));
                INDATA.Columns.Add("PROCID", typeof(string));
                INDATA.Columns.Add("LANGID", typeof(string));
                INDATA.Columns.Add("WORKTYPE", typeof(string));
                INDATA.Columns.Add("OQC_INSP_REQ_ID", typeof(string));

                DataRow dr = INDATA.NewRow();
                dr["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                dr["BOXID"] = txtPltIdRight.Text.ToString();
                dr["PRODID"] = txtProdClassRight.Text.ToString();
                dr["UNPACK_QTY"] = txtBoxQtyRight.Text.ToString();
                dr["UNPACK_QTY2"] = txtBoxQtyRight.Text.ToString();

                dr["NOTE"] = "";
                dr["USERID"] = LoginInfo.USERID;
                dr["EQSGID"] = txtEqsgIdRight.Text.ToString();
                dr["EQPTID"] = txtEqptIdRight.Text.ToString();
                dr["PROCID"] = txtProcIdRight.Text.ToString();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["WORKTYPE"] = "MIX"; //ȥ������
                dr["OQC_INSP_REQ_ID"] = txtOqcIdRight.Text.ToString();
                INDATA.Rows.Add(dr);

                DataTable dsResult = new ClientProxy().ExecuteServiceSync("BR_PRD_REG_UNPACK_PALLET", "INDATA", "OUTDATA", INDATA);

                if (dsResult != null && dsResult.Rows.Count > 0)
                {
                    Util.MessageInfo("SFU3390"); //UNPACK�Ǿ����ϴ�.

                    DataTable dt = new DataTable();
                    dt = DataTableConverter.Convert(dgPlthistory.ItemsSource);

                    dt.AcceptChanges();

                    foreach (DataRow dgBoxHistoryDr in dt.Rows)
                    {
                        if (dgBoxHistoryDr["PALLETID"].ToString() == txtPltIdRight.Text.ToString())
                        {
                            dgBoxHistoryDr.Delete();
                            break;
                        }
                    }

                    dt.AcceptChanges();

                    Util.GridSetData(dgPlthistory, dt, FrameOperation);

                    Util.SetTextBlockText_DataGridRowCount(tbBoxHistory_cnt, Util.NVC(dt.Rows.Count));

                    txtPltIdRight.Text = string.Empty;
                    txtEqsgIdRight.Text = string.Empty;
                    txtEqptIdRight.Text = string.Empty;
                    txtProdIdRight.Text = string.Empty;
                    txtProcIdRight.Text = string.Empty;
                    txtProdClassRight.Text = string.Empty;
                    txtBoxQtyRight.Text = string.Empty;
                    txtOqcIdRight.Text = string.Empty;
                    txtRcvIssIdRight.Text = string.Empty;
                    confirmMessage = string.Empty;
                }
                else
                {
                    throw new Exception();
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private bool oqcInspID_Init_chk(string palletID)
        {
            try
            {
                //oqc �˻� �Ƿ� �ƴ��� �ѹ��� detail�ϰ� üũ
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("BOXID", typeof(string)); //PALLETID

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["BOXID"] = palletID;

                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_OQC_INSP_PALLET_FIND", "INDATA", "OUTDATA", RQSTDT);

                if (dtResult.Rows.Count != 0)
                {
                    if (dtResult.Rows[0]["OQC_INSP_REQ_ID"] != null && dtResult.Rows[0]["OQC_INSP_REQ_ID"].ToString().Length > 0)
                    {
                        if (dtResult.Rows[0]["JUDG_VALUE"].ToString() == "F")
                        {
                            return true;
                        }
                        else
                        {
                            return false;
                        }
                    }
                    else
                    {
                        return false;
                    }
                }
                else
                {
                    return false;
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private void oqcInspID_Init(string palletID)
        {
            try
            {
                //oqc �˻� �Ƿ� ID : null ������ UPDATE
                DataTable INDATA = new DataTable();
                INDATA.TableName = "RQSTDT";
                INDATA.Columns.Add("BOXID", typeof(string));
                INDATA.Columns.Add("OQC_INSP_REQ_ID", typeof(string)); //PALLETID

                DataRow drUPD = INDATA.NewRow();
                drUPD["BOXID"] = palletID;
                drUPD["OQC_INSP_REQ_ID"] = txtOqcIdRight.Text.ToString();

                INDATA.Rows.Add(drUPD);

                new ClientProxy().ExecuteServiceSync("DA_PRD_UPD_OQC_INSP_SEQ_ID", "INDATA", "OUTDATA", INDATA);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }


        private void unPack_Process(object sender)
        {
            try
            {
                if (!string.IsNullOrEmpty(txtRcvIssIdRight.Text.ToString()))
                {
                    Util.MessageInfo("SFU3393");
                    return;
                }

                if (txtPltIdRight.Text == null || string.IsNullOrEmpty(txtPltIdRight.Text.ToString()))
                {
                    Util.MessageInfo("SFU1636");
                    return;
                }

                #region �ּ� ó�� UI -> Biz Validation

                #endregion

                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(confirmMessage, null, "CAUSE", MessageBoxButton.OKCancel, MessageBoxIcon.Warning, (caution_result) =>
                {
                    if (caution_result == MessageBoxResult.OK)
                    {
                        //oqcInspID_Init(txtPltIdRight.Text.ToString()); //�ű� Biz ó��(2024-01-24 seonjun9)

                        //UNPACK ����
                        pack_unPack_init(sender);
                    }
                    else
                    {
                        return;
                    }
                });
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private void pack_unPack_init(object sender)
        {
            try
            {
                //pallet unpak\
                Pallet_unpack(sender);
            }
            catch (Exception ex)
            {
                //Util.Alert(ex.Message);
                Util.MessageException(ex);
            }
        }
        #endregion

        private void txtBoxID_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            try
            {

                if (e.Key == Key.V && Keyboard.IsKeyDown(Key.LeftCtrl))
                {
                    string sPasteString = Clipboard.GetText();

                    if (string.IsNullOrWhiteSpace(sPasteString))
                    {
                        return;
                    }

                    if (!AuthCopyAndPasteChk())
                    {

                        if (LoginInfo.CFG_SHOP_ID.Equals("G481"))
                        {
                            Util.MessageInfo("SFU3180");
                            txtBoxID.Clear();
                            return;
                        }
                        else
                        {
                            txtBoxID.Focus();
                            txtBoxID.SelectAll();
                        }
                    }
                    else
                    {
                        string[] stringSeparators = new string[] { "\r\n", "\n", "," };
                        string[] arrBoxID = sPasteString.Split(stringSeparators, StringSplitOptions.RemoveEmptyEntries);

                        if (this.boxLOTCount < (this.dgBoxLot.GetRowCount() + arrBoxID.Length))
                        {
                            ms.AlertWarning("SFU3319", boxLOTCount.ToString()); // �Է¿��� : PALLET�� ���尡�� ���� %1�� �Ѿ����ϴ�. [������� ���� �� LOT �Է�]
                            return;
                        }

                        this.loadingIndicator.Visibility = Visibility.Visible;
                        foreach (string boxID in arrBoxID)
                        {
                            this.AddLOTforBoxing(boxID);
                        }
                        loadingIndicator.Visibility = Visibility.Collapsed;
                    }
                }
            }
            catch (Exception ex)
            {
                loadingIndicator.Visibility = Visibility.Collapsed;
                Util.MessageException(ex);
            }
        }

        private void MenuItem_Copy_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Clipboard.SetText(txtBoxID.SelectedText.ToString());
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
                Clipboard.SetText(txtBoxID.SelectedText.ToString());
                txtBoxID.Clear();

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private Boolean AuthCopyAndPasteChk()
        {
            Boolean bChk = false;

            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("AREAID", typeof(string));
                RQSTDT.Columns.Add("USERNAME", typeof(string));
                RQSTDT.Columns.Add("APPR_CODE", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["AREAID"] = LoginInfo.CFG_AREA_ID;
                dr["USERNAME"] = LoginInfo.USERID;
                dr["APPR_CODE"] = "GMES_PACK_SM_PLPLT";
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_APPR_PERSON_PACK", "RQSTDT", "RSLTDT", RQSTDT);

                if (dtResult.Rows.Count > 0)
                {
                    bChk = true;
                }
                return bChk;

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return bChk;
            }
        }

        private void nbBoxingCnt_ValueChanged(object sender, PropertyChangedEventArgs<double> e)
        {
            C1NumericBox c1NumericBox = (C1NumericBox)sender;
            double newValue = e.NewValue;
            double oldValue = e.OldValue;
            if (newValue > c1NumericBox.Maximum)
            {
                c1NumericBox.Value = oldValue;
                newValue = oldValue;
            }

            if (nbBoxingCnt.Value - this.dgBoxLot.GetRowCount() < 0)
            {
                nbBoxingCnt.Value = boxLOTCount;
                return;
            }

            boxLOTCount = Convert.ToInt32(newValue);
            string boxingStatus = string.Empty;


            //���� ���� ���� 0�� ��� �۾� �Ұ�
            if (nbBoxingCnt.Value == 0)
            {
                cboKind.IsEnabled = false;
                txtBoxID.IsEnabled = false;
            }
            //�۾����� �� �϶� �޺��ڽ� �� ���� �Ұ�
            else if (chkWorkStrart)
            {
                cboKind.IsEnabled = false;
            }
            //���� ���� ���� 0�� �ƴϰ� �۾����� ���� �ƴϸ� �޺��ڽ� �� �ؽ�Ʈ �ڽ� �� �Է� ����
            else
            {
                cboKind.IsEnabled = true;
                txtBoxID.IsEnabled = true;
            }

            if (chkWorkStrart)
            {
                boxingStatus = ObjectDic.Instance.GetObjectName("������");
            }
            else
            {
                boxingStatus = ObjectDic.Instance.GetObjectName("�����");
            }

            SetBoxingStatus(boxLOTCount, this.dgBoxLot.GetRowCount(), boxingStatus);
        }

        private void SetBoxingStatus(int boxLOTCount, int gridLOTCount, string boxingStatus)
        {
            if (txtPackingStatus == null)
            {
                return;
            }
            if (boxingStatus == "������")
            {
                this.txtPackingStatus.Text = ObjectDic.Instance.GetObjectName(boxingStatus);
            }
            else
            {
                txtPackingStatus.Text = ObjectDic.Instance.GetObjectName(boxingStatus) + " : " + gridLOTCount.ToString() + " / " + boxLOTCount.ToString();
                txtRemainCount.Text = Convert.ToString(boxLOTCount - gridLOTCount);
            }
        }
    }
}