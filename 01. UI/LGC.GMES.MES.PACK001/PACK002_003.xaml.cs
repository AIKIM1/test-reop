/*************************************************************************************
 Created Date : 2020.08.11
      Creator : �ֿ켮
   Decription : ������,���� Hold/Realese(Pack)
--------------------------------------------------------------------------------------
 [Change History]
  2020.08.00  ����� : Initial Created.

**************************************************************************************/

using C1.WPF;
using C1.WPF.DataGrid;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.CMM001.Extensions;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using System;
using System.Data;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;

namespace LGC.GMES.MES.PACK001
{
    public partial class PACK002_003 : UserControl, IWorkArea
    {
        CommonCombo _combo = new CommonCombo();
        string sAREAID = LoginInfo.CFG_SHOP_ID;
        string sEmpty_sboxid = string.Empty;
        LGC.GMES.MES.PACK001.Class.MESSAGE_PARAM ms = new LGC.GMES.MES.PACK001.Class.MESSAGE_PARAM();
        #region Declaration & Constructor 
        #region <Hold ����>
        int TotalRow;
        private string sEmpty_Lot = string.Empty;
        private DataTable isCreateTable;
        private DataTable isSearchTable;
        #endregion

        public PACK002_003()
        {
            InitializeComponent();

        }


        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            intHoldCombo();

            dtpDateFrom.SelectedDateTime = DateTime.Now.AddDays(-7);
            dtpDateTo.SelectedDateTime = DateTime.Now;

            

            this.Loaded -= new System.Windows.RoutedEventHandler(this.UserControl_Loaded);

        }

        #region [ Combo ]

        #region < Hold Combo >
        public void intHoldCombo()
        {

            cboHoldType.Items.Add("S_BOX HOLD");
            cboHoldType.Items.Add("RANGE HOLD");
            cboHoldType.Items.Add("BEFORE WAREHOUSEING. HOLD");
            cboHoldType.SelectedIndex = 0;

            CommonCombo _combo = new CommonCombo();

            C1ComboBox cboSHOPID = new C1ComboBox();
            cboSHOPID.SelectedValue = LoginInfo.CFG_SHOP_ID;

            C1ComboBox cboAREA_TYPE_CODE = new C1ComboBox();
            cboAREA_TYPE_CODE.SelectedValue = Area_Type.PACK;

            C1ComboBox cboArea = new C1ComboBox();
            cboArea.SelectedValue = LoginInfo.CFG_AREA_ID;

            //����_���� ����HOLD Tab    
            C1ComboBox[] cboEquipmentSegmentParent = { cboArea };
            _combo.SetCombo(cboEquipmentSegment, CommonCombo.ComboStatus.ALL, cbParent: cboEquipmentSegmentParent);

            //�����ڵ�_���� HOLD ��ȸ Tab2
            PortMtrlGroupCombo();

            //��ü�ڵ�_���� HOLD ��ȸ Tab
            PortSupGroupCombo();

            //��������_���� ���������� HOLD Tab
            String[] sFilter_Mtrl_t = { "MTRL_HOLD_UI_TRGT_CODE" };
            _combo.SetCombo(cboSearchSboxtype, CommonCombo.ComboStatus.SELECT, sFilter: sFilter_Mtrl_t, sCase: "COMMCODE");

            //�����ڵ�_���� ���������� HOLD Tab
            SetMtrlCombo();

            //HOLD ����_���� ���������� HOLD Tab 
            SetHoldReason();

            //�����ڵ�_���� ����HOLD Tab
            SetSearchMtrltype();

            //��ü�ڵ�_���� ����HOLD Tab
            SetSearchSup();

            //Hold Flag �޺�
            HoldFlagCombo();
            //HOLD ����_���� ����HOLD Tab
            SetHoldCaseReason();


        }
        private void cboSearchMtrltype_SelectedItemChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            SetSearchSup();
        }

        private void cboHoldReason_SelectedItemChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            if (cboHoldReason.SelectedIndex > -1)
            {
                SetHoldReason();
            }
        }

        private void cboHoldCaseReason_SelectedItemChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            if (cboHoldCaseReason.SelectedIndex > -1)
            {
                SetHoldCaseReason();
            }
        }

        private void btnHoldRelease_Click(object sender, RoutedEventArgs e)
        {
            Util.MessageConfirm("SFU4046", (result) =>
            {
                if (result == MessageBoxResult.OK)
                {
                    SetMtrlUnHold(txtNote.Text);
                }
            });
        }
        #endregion

        #region �޴�COMBO DA

        private void PortMtrlGroupCombo()
        {
            try
            {
                DataTable dtIndata = new DataTable();
                DataTable dtAllRow = new DataTable();
                dtAllRow.Columns.AddRange(new DataColumn[] { new DataColumn("CBO_CODE"), new DataColumn("CBO_NAME") });

                dtIndata.TableName = "RQSTDT";
                dtIndata.Columns.Add("LANGID", typeof(string));
                dtIndata.Columns.Add("AREAID", typeof(string));

                DataRow dr = dtIndata.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["AREAID"] = LoginInfo.CFG_AREA_ID;

                dtIndata.Rows.Add(dr);

                dtAllRow.Rows.Add(new object[] { "-ALL-", "-ALL-" });

                cboSBoxType.DisplayMemberPath = "CBO_NAME";
                cboSBoxType.SelectedValuePath = "CBO_CODE";

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_MTRL_SEL_S_BOX_MATID_CBO", "RQSTDT", "RSLTDT", dtIndata);

                if (dtResult != null && dtResult.Rows.Count > 0)
                {
                    dtAllRow.Merge(dtResult);
                }

                cboSBoxType.ItemsSource = DataTableConverter.Convert(dtAllRow);
                cboSBoxType.SelectedIndex = 0;

            }
            catch
            {

            }
        }
        private void SetMtrlCombo()
        {
            try
            {
                DataTable dtIndata = new DataTable();
                DataTable dtAllRow = new DataTable();
                dtAllRow.Columns.AddRange(new DataColumn[] { new DataColumn("CBO_CODE"), new DataColumn("CBO_NAME") });

                dtIndata.TableName = "RQSTDT";
                dtIndata.Columns.Add("LANGID", typeof(string));
                dtIndata.Columns.Add("AREAID", typeof(string));

                DataRow dr = dtIndata.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["AREAID"] = LoginInfo.CFG_AREA_ID;

                dtIndata.Rows.Add(dr);

                dtAllRow.Rows.Add(new object[] { "-SELECT-", "-SELECT-" });

                cboMtrlid.DisplayMemberPath = "CBO_NAME";
                cboMtrlid.SelectedValuePath = "CBO_CODE";

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_MTRL_SEL_S_BOX_MATID_CBO", "RQSTDT", "RSLTDT", dtIndata);

                if (dtResult != null && dtResult.Rows.Count > 0)
                {
                    dtAllRow.Merge(dtResult);
                }

                cboMtrlid.ItemsSource = DataTableConverter.Convert(dtAllRow);
                cboMtrlid.SelectedIndex = 0;

            }
            catch
            {

            }
        }
        private void PortSupGroupCombo()
        {
            try
            {
                DataTable dtIndata = new DataTable();
                DataTable dtAllRow = new DataTable();
                dtAllRow.Columns.AddRange(new DataColumn[] { new DataColumn("CBO_CODE"), new DataColumn("CBO_NAME") });

                dtIndata.TableName = "RQSTDT";
                dtIndata.Columns.Add("LANGID", typeof(string));
                dtIndata.Columns.Add("AREAID", typeof(string));

                DataRow dr = dtIndata.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["AREAID"] = LoginInfo.CFG_AREA_ID;

                dtIndata.Rows.Add(dr);

                dtAllRow.Rows.Add(new object[] { "-ALL-", "-ALL-" });

                cboSuppliercode.DisplayMemberPath = "CBO_CODE";
                cboSuppliercode.SelectedValuePath = "CBO_NAME";

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_MTRL_SEL_S_BOX_SUPPLIER_CBO", "RQSTDT", "RSLTDT", dtIndata);

                if (dtResult != null && dtResult.Rows.Count > 0)
                {
                    dtAllRow.Merge(dtResult);
                }

                cboSuppliercode.ItemsSource = DataTableConverter.Convert(dtAllRow);
                cboSuppliercode.SelectedIndex = 0;

            }
            catch
            {

            }
        }

        private void HoldFlagCombo()
        {
            try
            {
                DataTable dtIndata = new DataTable();
                DataTable dtAllRow = new DataTable();
                dtAllRow.Columns.AddRange(new DataColumn[] { new DataColumn("CBO_CODE"), new DataColumn("CBO_NAME") });

                dtAllRow.Rows.Add(new object[] { "-ALL-", "-ALL-" });
                dtAllRow.Rows.Add(new object[] { "Y", "Y" });
                dtAllRow.Rows.Add(new object[] { "N", "N" });

                cboHoldFlag.DisplayMemberPath = "CBO_NAME";
                cboHoldFlag.SelectedValuePath = "CBO_CODE";

                cboHoldFlag.ItemsSource = DataTableConverter.Convert(dtAllRow);
                cboHoldFlag.SelectedIndex = 1;

            }
            catch
            {

            }
        }
        private void SetSearchMtrltype()
        {
            try
            {
                DataTable dtIndata = new DataTable();
                DataTable dtAllRow = new DataTable();
                dtAllRow.Columns.AddRange(new DataColumn[] { new DataColumn("CBO_CODE"), new DataColumn("CBO_NAME") });

                dtIndata.TableName = "RQSTDT";
                dtIndata.Columns.Add("LANGID", typeof(string));
                dtIndata.Columns.Add("AREAID", typeof(string));

                DataRow dr = dtIndata.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["AREAID"] = LoginInfo.CFG_AREA_ID;

                dtIndata.Rows.Add(dr);

                dtAllRow.Rows.Add(new object[] { "-SELECT-", "-SELECT-" });

                cboSearchMtrltype.DisplayMemberPath = "CBO_NAME";
                cboSearchMtrltype.SelectedValuePath = "CBO_CODE";

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_MTRL_SEL_S_BOX_MATID_CBO", "RQSTDT", "RSLTDT", dtIndata);

                if (dtResult != null && dtResult.Rows.Count > 0)
                {
                    dtAllRow.Merge(dtResult);
                }

                cboSearchMtrltype.ItemsSource = DataTableConverter.Convert(dtAllRow);
                cboSearchMtrltype.SelectedIndex = 0;

            }
            catch
            {
            }
        }
        private void SetSearchSup()
        {
            try
            {
                DataTable dtIndata = new DataTable();
                DataTable dtAllRow = new DataTable();
                dtAllRow.Columns.AddRange(new DataColumn[] { new DataColumn("CBO_CODE"), new DataColumn("CBO_NAME") });

                dtIndata.TableName = "RQSTDT";
                dtIndata.Columns.Add("LANGID", typeof(string));
                dtIndata.Columns.Add("AREAID", typeof(string));

                DataRow dr = dtIndata.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["AREAID"] = LoginInfo.CFG_AREA_ID;

                dtIndata.Rows.Add(dr);

                dtAllRow.Rows.Add(new object[] { "-SELECT-", "-SELECT-" });

                cboSearchSup.DisplayMemberPath = "CBO_NAME";
                cboSearchSup.SelectedValuePath = "CBO_CODE";

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_MTRL_SEL_S_BOX_SUPPLIER_CBO", "RQSTDT", "RSLTDT", dtIndata);

                if (dtResult != null && dtResult.Rows.Count > 0)
                {
                    dtAllRow.Merge(dtResult);
                }

                cboSearchSup.ItemsSource = DataTableConverter.Convert(dtAllRow);
                cboSearchSup.SelectedIndex = 0;

            }
            catch
            {

            }
        }
        private void SetHoldReason()
        {
            string[] filter = { sAREAID, Area_Type.PACK };
            SetHoldReason_sum(cboHoldReason, CommonCombo.ComboStatus.SELECT, filter);
        }
        private void SetHoldCaseReason()
        {
            string[] filter = { sAREAID, Area_Type.PACK };
            SetHoldCaseReason_sum(cboHoldCaseReason, CommonCombo.ComboStatus.SELECT, filter);
        }

        //Combo �������� : ALL, N/A, SELECT
        private static DataTable AddStatus(DataTable dt, CommonCombo.ComboStatus cs, string sValue, string sDisplay)
        {
            DataRow dr = dt.NewRow();

            switch (cs)
            {
                case CommonCombo.ComboStatus.ALL:
                    dr[sDisplay] = "-ALL-";
                    dr[sValue] = "";
                    dt.Rows.InsertAt(dr, 0);
                    break;

                case CommonCombo.ComboStatus.SELECT:
                    dr[sDisplay] = "-SELECT-";
                    dr[sValue] = "SELECT";
                    dt.Rows.InsertAt(dr, 0);
                    break;

                case CommonCombo.ComboStatus.NA:
                    dr[sDisplay] = "-N/A-";
                    dr[sValue] = "";
                    dt.Rows.InsertAt(dr, 0);
                    break;
            }

            return dt;
        }

        #endregion

        #endregion
        private void dgSBoxListLoadedCellPresenter(object sender, C1.WPF.DataGrid.DataGridCellEventArgs e)
        {
            try
            {
                C1.WPF.DataGrid.C1DataGrid dataGrid = sender as C1.WPF.DataGrid.C1DataGrid;

                if (e.Cell.Presenter == null)
                {
                    return;
                }

                if (e.Cell.Row.Type != DataGridRowType.Item)
                    return;
                if (dgSBoxList == null)
                    return;
            }
            catch (Exception ex)
            {
                Util.Alert(ex.ToString());
            }
        }
        public IFrameOperation FrameOperation
        {
            get;
            set;
        }


        #endregion

        #region Event

        private void tabControlMain_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if(cboHoldType != null)
                cboHoldType.SelectedIndex = (sender as C1TabControl).SelectedIndex;
        }

        #region ( loadingIndicator Event )
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
        private void DoEvents()
        {
            Application.Current.Dispatcher.Invoke(DispatcherPriority.Background, new System.Threading.ThreadStart(delegate { }));
        }
        private void DataGridRowAdd(C1.WPF.DataGrid.C1DataGrid dg)
        {
            dg.BeginNewRow();
            dg.EndNewRow(true);
        }
        #endregion

        #region (���������� - ID�Է�)
        private void txtSboxId_KeyDown(object sender, KeyEventArgs e)
        {
            try
            {
                if (e.Key == Key.Enter)
                {
                    if (Check_Input())
                    {
                        string sHoldCode = cboSearchSboxtype.Text.Split(':')[0].Trim();

                        DataTable RQSTDT = new DataTable();
                        RQSTDT.TableName = "RQSTDT";
                        RQSTDT.Columns.Add("CHK", typeof(string));
                        RQSTDT.Columns.Add("S_BOX_ID", typeof(string));

                        DataRow dr = RQSTDT.NewRow();
                        dr["CHK"] = true;
                        dr["S_BOX_ID"] = txtSboxId.Text.Trim();
                        RQSTDT.Rows.Add(dr);

                        //DA_MTRL_SEL_S_HOLD_PRGT_UI->DA_MTRL_SEL_HOLD_SBOXID ����
                        new ClientProxy().ExecuteService("DA_MTRL_SEL_HOLD_SBOXID", "RQSTDT", "RSLTDT", RQSTDT, (dtResult, ex) =>
                        {
                            if (ex != null)
                            {
                                Util.MessageException(ex);
                                return;
                            }

                            if (dtResult.Rows.Count == 0)
                            {
                                DataTable dtBefore = DataTableConverter.Convert(dgSBoxList.ItemsSource);

                                dtBefore.Merge(RQSTDT);
                                dgSBoxList.ItemsSource = DataTableConverter.Convert(dtBefore);
                                txtSboxId.Text = "";
                            }
                            else
                            {
                                Util.MessageInfo("SFU3537");
                            }
                            loadingIndicator.Visibility = Visibility.Collapsed;
                        });
                    }
                }
            }

            catch (Exception ex)
            {
                Util.MessageException(ex);
                HiddenLoadingIndicator();
            }

        }

        private void txtSboxIdKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.V && Keyboard.IsKeyDown(Key.LeftCtrl))
            {
                try
                {
                    ShowLoadingIndicator();

                    string[] stringSeparators = new string[] { "\r\n" };
                    string sPasteString = Clipboard.GetText();
                    string[] sPasteStrings = sPasteString.Split(stringSeparators, StringSplitOptions.None);
                    DataTable dt = DataTableConverter.Convert(dgSBoxList.ItemsSource);
                    string fs = sPasteString.Split(';')[0].Trim();
                    string r = cboMtrlid.SelectedValue.ToString();

                    //�ѱ� ����ó��
                    char[] charArr = sPasteString.ToCharArray();
                    foreach (char c in charArr)
                    {
                        if (char.GetUnicodeCategory(c) == System.Globalization.UnicodeCategory.OtherLetter)
                        {
                            Util.MessageValidation("SFU1801"); //�Է� �����Ͱ� �������� �ʽ��ϴ�.
                            return;
                        }
                    }
                    for (int i = 0; i < dt.Rows.Count; i++)
                    {
                        for (int j = 0; j < sPasteStrings.Count(); j++)
                        {
                            if (dt.Rows.Count > 0)
                            {
                                if (Util.NVC(dt.Rows[i]["S_BOX_ID"]) == Util.NVC(sPasteStrings[j].ToString()))
                                {
                                    Util.MessageInfo("MMD0067"); //�ߺ� �����Ͱ� ���� �մϴ�.
                                    return;
                                }
                            }
                        }
                    }
                    if (cboSearchSboxtype.SelectedValue.ToString() == "SELECT")
                    {
                        Util.MessageValidation("SFU1642");   //���õ� �����ڵ尡 �����ϴ�.
                        return;
                    }
                    if (cboMtrlid.SelectedValue.ToString() == "-SELECT-")
                    {
                        Util.MessageValidation("SFU1239");   //�����ڵ带 Ȯ�� �ϼ���.
                        return;
                    }
                    if (sPasteStrings.Count() > 100)
                    {
                        Util.MessageValidation("SFU3695");   //�ִ� 100�� ���� �����մϴ�.
                        return;
                    }
                    if (fs != r)
                    {
                        Util.MessageValidation("SFU1239"); //�����ڵ带 Ȯ�� �ϼ���.
                        return;
                    }
                    for (int i = 0; i < sPasteStrings.Length; i++)
                    {
                        if (!string.IsNullOrEmpty(sPasteStrings[i]) && Multi_Create(sPasteStrings[i]) == false)
                            break;

                        System.Windows.Forms.Application.DoEvents();
                    }

                    if (sEmpty_Lot != "")
                    {
                        //�Է��� LOTID[%1] ������ Ȯ���Ͻʽÿ�.
                        Util.MessageValidation("SFU4306", sEmpty_Lot);
                        sEmpty_Lot = "";
                    }

                }
                catch (Exception ex)
                {
                    Util.MessageException(ex);
                    return;
                }
                finally
                {
                    HiddenLoadingIndicator();
                }

                e.Handled = true;
            }
        }
        //MTRL_LOT�� ��� WIP_INPUT_MTRL_HIST ��ȸ
        //private bool Chk_InputMtrl(string sMtrlid)
        //{
        //    bool bReturn = true;
        //    try
        //    {
        //        iKeypart = 0;

        //        DataTable RQSTDT = new DataTable();
        //        RQSTDT.TableName = "RQSTDT";
        //        RQSTDT.Columns.Add("LANGID", typeof(string));
        //        RQSTDT.Columns.Add("INPUT_LOTID", typeof(string));

        //        DataRow dr = RQSTDT.NewRow();
        //        dr["LANGID"] = LoginInfo.LANGID;
        //        dr["INPUT_LOTID"] = sMtrlid;
        //        RQSTDT.Rows.Add(dr);

        //        DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_TB_SFC_WIP_INPUT_MTRL_HIST_BY_MTRLID", "RQSTDT", "RSLTDT", RQSTDT);

        //        iKeypart = dtResult.Rows.Count;
        //    }
        //    catch (Exception ex)
        //    {
        //        throw ex;
        //    }
        //    return bReturn;
        //}
        #endregion

        #region (CHECK_Logic)
        private bool Check_Input()
        {
            bool bReturn = true;
            try
            {
                string s = txtSboxId.Text.ToString();
                string fs = txtSboxId.Text.Split(';')[0].Trim();
                string r = cboMtrlid.SelectedValue.ToString();

                char[] charArr = s.ToCharArray();
                foreach (char c in charArr)
                {
                    if (char.GetUnicodeCategory(c) == System.Globalization.UnicodeCategory.OtherLetter)
                    {
                        ms.AlertWarning("SFU1801"); //�Է� �����Ͱ� �������� �ʽ��ϴ�.
                        bReturn = false;
                        txtSboxId.Focus();
                        return bReturn;
                    }
                }
                DataTable dt = DataTableConverter.Convert(dgSBoxList.ItemsSource);
                //ID �Է�Ȯ��.
                if (!(txtSboxId.Text.Length > 0))
                {
                    ms.AlertWarning("SFU1801"); //�Է� �����Ͱ� �������� �ʽ��ϴ�.
                    bReturn = false;
                    txtSboxId.Focus();
                    return bReturn;
                }
                //�������� �Է�Ȯ��.
                if ((cboSearchSboxtype.SelectedIndex < 1))
                {
                    ms.AlertWarning("SFU1642"); //���õ� �����ڵ尡 �����ϴ�.
                    bReturn = false;
                    txtSboxId.Focus();
                    return bReturn;
                }
                if ((cboMtrlid.SelectedIndex < 1))
                {
                    ms.AlertWarning("SFU1239");  //�����ڵ带 Ȯ�� �ϼ���.
                    bReturn = false;
                    txtSboxId.Focus();
                    return bReturn;
                }
                if (fs != r)
                {
                    ms.AlertWarning("SFU1239"); //�����ڵ带 Ȯ�� �ϼ���.
                    bReturn = false;
                    txtSboxId.Focus();
                    return bReturn;
                }

                //�ߺ�ID �Է�Ȯ��.
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    if (dt.Rows.Count > 0)
                    {
                        if (Util.NVC(dt.Rows[i]["S_BOX_ID"]) == Util.NVC(txtSboxId.Text))
                        {
                            Util.MessageInfo("MMD0067"); //�ߺ� �����Ͱ� ���� �մϴ�.
                            bReturn = false;
                            txtSboxId.Focus();
                            return bReturn;
                        }
                    }
                }

            }
            catch (Exception ex)
            {
                throw ex;
            }
            return bReturn;
        }

        private bool chkSave()
        {
            bool bRetrun = true;
            try
            {
                if (Util.NVC(cboSearchMtrltype.SelectedValue) == "-SELECT-")
                {
                    Util.Alert("SFU1239");  //�����ڵ带 Ȯ�� �ϼ���.
                    bRetrun = false;
                    cboSearchMtrltype.Focus();
                    return bRetrun;
                }
                if (Util.NVC(cboSearchSup.SelectedValue) == "-SELECT-")
                {
                    Util.Alert("SFU4220");  //��ü�� ������ �ּ���
                    bRetrun = false;
                    cboSearchSup.Focus();
                    return bRetrun;
                }
                if ((Util.NVC(cboHoldCaseReason.SelectedValue) == "") && (cboHoldCaseReason.SelectedValue.ToString() == "-SELECT-"))
                {
                    Util.Alert("SFU1342");  //HOLD ������ ���� �ϼ���.
                    bRetrun = false;
                    cboHoldCaseReason.Focus();
                    return bRetrun;
                }

                if (DateTime.Parse(dtpDateFromF.SelectedDateTime.ToString("yyyy-MM-dd"))
                    > DateTime.Parse(dtpDateToF.SelectedDateTime.ToString("yyyy-MM-dd")))
                {
                    Util.Alert("SFU1517");  //��� �������� �����Ϻ��� Ů�ϴ�.
                    bRetrun = false;
                    dtpDateFromF.Focus();
                    return bRetrun;
                }
                //if (DateTime.Parse(dtpDateFromF.SelectedDateTime.ToString("yyyy-MM-dd"))
                //    == DateTime.Parse(dtpDateToF.SelectedDateTime.ToString("yyyy-MM-dd")))
                //{
                //    Util.Alert("9180");  //���۽ð��� ����ð��� �����ϴ�.�������ֽʽÿ�.
                //    bRetrun = false;
                //    dtpDateFromF.Focus();
                //    return bRetrun;
                //}

            }
            catch (Exception ex)
            {
                throw ex;
            }

            return bRetrun;

        }
        private void ChK_Releasebtn(object sender, MouseButtonEventArgs e)
        {
            try
            {
                if (!(dgHoldHistory.ItemsSource == null || dgHoldHistory.GetRowCount() == 0))
                {
                    DataTable dt = DataTableConverter.Convert(dgHoldHistory.ItemsSource);
                    if (!(bool)ChkOption.IsSelected)
                    {
                        return;
                    }
                    Point pnt = e.GetPosition(null);
                    C1.WPF.DataGrid.DataGridCell cell = dgHoldHistory.GetCellFromPoint(pnt);

                    if (cell.Value.ToString() == "True")
                    {
                        btnHoldRelease.IsEnabled = true;
                        txtNote.IsEnabled = true;
                    }
                    if (cell.Value.ToString() == "False")
                    {
                        for (int i = 0; i < dt.Rows.Count; i++)
                        {
                            //for (int j=0; j < dt.Columns.Count; j++)
                            //{
                            if (dt.Rows[i][0].ToString() == "True")
                            {
                                btnHoldRelease.IsEnabled = true;
                                txtNote.IsEnabled = true;
                                return;
                            }
                            //}
                        }
                        btnHoldRelease.IsEnabled = false;
                        txtNote.IsEnabled = false;
                    }

                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return;
        }
        private bool Check_HoldConfirm()
        {
            bool bReturn = true;
            try
            {

                DataTable dt = DataTableConverter.Convert(dgSBoxList.ItemsSource);
                string r = cboMtrlid.SelectedValue.ToString();
                //string fs = txtSboxId.Text.Split(';')[0].Trim();


                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    if (Util.NVC(dt.Rows[i]["S_BOX_ID"]).Split(';')[0].ToString() != r)
                    {
                        ms.AlertWarning("SFU8235"); //���� �ٸ� �����ڵ尡 �ֽ��ϴ�.
                        bReturn = false;
                        return bReturn;
                    }
                }
                //��� Ȯ��
                if (dgSBoxList.ItemsSource == null || dgSBoxList.Rows.Count - 1 == 0)
                {
                    ms.AlertWarning("SFU1984"); //�������� LOT ID�� �Է��ϼ���.
                    bReturn = false;
                    return bReturn;
                }
                //Ȧ�� ���� �ʼ�
                if (txtHoldReason.Text == "-SELECT-")
                {
                    Util.MessageInfo("SFU1342"); //HOLD ������ ���� �ϼ���.
                    bReturn = false;
                    return bReturn;
                }
                if (cboMtrlid.Text == "-SELECT-")
                {
                    Util.MessageInfo("SFU1239"); //�����ڵ带 Ȯ�� �ϼ���.
                    bReturn = false;
                    return bReturn;
                }
                if (txtHoldReason.Text.Length < 1)
                {
                    Util.MessageInfo("SFU4301"); //Hold ���� ������ �Է��ϼ���.
                    bReturn = false;
                    return bReturn;
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return bReturn;
        }

        private bool Check_HoldHist()
        {
            bool bRetrun = true;
            try
            {
                if (DateTime.Parse(dtpDateFrom.SelectedDateTime.ToString("yyyy-MM-dd"))
                    > DateTime.Parse(dtpDateTo.SelectedDateTime.ToString("yyyy-MM-dd")))
                {
                    Util.Alert("SFU1517");  //��� �������� �����Ϻ��� Ů�ϴ�.
                    bRetrun = false;
                    dtpDateFrom.Focus();
                    return bRetrun;
                }
                //if (DateTime.Parse(dtpDateFrom.SelectedDateTime.ToString("yyyy-MM-dd"))
                //    == DateTime.Parse(dtpDateTo.SelectedDateTime.ToString("yyyy-MM-dd")))
                //{
                //    Util.Alert("9180");  //���۽ð��� ����ð��� �����ϴ�.�������ֽʽÿ�.
                //    bRetrun = false;
                //    dtpDateFrom.Focus();
                //    return bRetrun;
                //}

            }
            catch (Exception ex)
            {
                throw ex;
            }

            return bRetrun;
        }
        #endregion

        #region ( Excel )
        //private void btnExcelLoad_Click(object sender, System.Windows.RoutedEventArgs e)
        //{
        //    try
        //    {
        //        DataTable dtDataGrid = new DataTable();
        //        dtDataGrid.TableName = "dtDataTable";

        //        dtDataGrid.Columns.Add("CHK", typeof(string));
        //        dtDataGrid.Columns.Add("LOTID", typeof(string));
        //        dtDataGrid.Columns.Add("PRODID", typeof(string));
        //        dtDataGrid.Columns.Add("WOID", typeof(string));
        //        dtDataGrid.Columns.Add("PRODNAME", typeof(string));
        //        dtDataGrid.Columns.Add("EQSGID", typeof(string));
        //        dtDataGrid.Columns.Add("EQSGNAME", typeof(string));
        //        dtDataGrid.Columns.Add("PROCNAME", typeof(string));
        //        dtDataGrid.Columns.Add("WIPSNAME", typeof(string));
        //        dtDataGrid.Columns.Add("BOXID", typeof(string));


        //        HoldExcelImportEditor frm = new HoldExcelImportEditor(dtDataGrid);
        //        DataTable dtChild = new DataTable();

        //        frm.FrameOperation = this.FrameOperation;
        //        //frm.FormClosed -= frm.FormClosed;

        //        frm.FormClosed += delegate ()
        //        {
        //            if (frm.DialogResult.Equals(MessageBoxResult.OK))
        //            {
        //                TotalRow = 0;
        //                dgSBoxList.ItemsSource = null;
        //                //CellGridAdd(frm.dtIfMethod);
        //            }
        //        };

        //        frm.ShowModal();
        //        frm.CenterOnScreen();
        //    }
        //    catch (Exception ex)
        //    {
        //        Util.MessageException(ex);
        //    }
        //}
        private void btnExcel_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                new LGC.GMES.MES.Common.ExcelExporter().Export(dgHoldHistory);
            }
            catch (Exception ex)
            {
                Util.Alert(ex.ToString());
            }

        }
        #endregion

        #region ( Hold ȭ�� ���� ��� )
        private void btnSelectCancel_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                DataTable dtTempTagetList = DataTableConverter.Convert(dgSBoxList.ItemsSource);

                if (!(dtTempTagetList.AsEnumerable().Count(row => row.Field<string>("CHK") == "True") > 0))
                {
                    Util.MessageInfo("SFU1137");
                    return;
                }

                for (int i = (dtTempTagetList.Rows.Count - 1); i >= 0; i--)
                {
                    if (Util.NVC(dtTempTagetList.Rows[i]["CHK"]) == "True" ||
                        Util.NVC(dtTempTagetList.Rows[i]["CHK"]) == "1")
                    {

                        dtTempTagetList.Rows[i].Delete();
                        dtTempTagetList.AcceptChanges();
                        TotalRow = TotalRow - 1;
                    }
                }

                dgSBoxList.ItemsSource = DataTableConverter.Convert(dtTempTagetList);

                if (!(dtTempTagetList.Rows.Count > 0))
                {
                    dgSBoxList.ItemsSource = null;
                }

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }

        }
        #endregion

        #region ( Hold ȭ�� ��ü ��� )
        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            //SFU3440 �ʱ�ȭ �Ͻðڽ��ϱ�?	
            Util.MessageConfirm("SFU3440", (result) =>
            {
                if (result == MessageBoxResult.OK)
                {
                    TotalRow = 0;
                    Util.gridClear(dgSBoxList);
                }
            });
        }
        #endregion

        #region ( ����������HOLD ȭ�� ����- Hold ��ư )
        public void btnHold_Click(object sender, RoutedEventArgs e)
        {
            try
            {

                if (Check_HoldConfirm())
                {
                    DataTable dt = DataTableConverter.Convert(dgSBoxList.ItemsSource);

                    DataRow[] drINPUT_LOT = dt.Select(" CHK = true ");

                    if (drINPUT_LOT.Length == 0)
                    {
                        Util.Alert("SFU1651"); //���õ� �׸��� �����ϴ�.
                        return;
                    }
                    if (drINPUT_LOT.Length > 500)
                    {
                        //�޽��� �Է� 500�� �̻��̸�
                        Util.Alert("SFU8102");  //�ִ� 1000�ڱ��� �����մϴ�.
                        return;

                    }
                    else if (drINPUT_LOT.Length > 0 && drINPUT_LOT.Length < 500)
                    {
                        LGC.GMES.MES.ControlsLibrary.MessageBox.Show(ms.AlertRetun("SFU1345"), null, "Information", MessageBoxButton.OKCancel, MessageBoxIcon.None, (result) =>
                        {
                            if (result == MessageBoxResult.OK)
                            {
                                SetHold();
                            }
                        });
                    }
                    else
                    {
                        LGC.GMES.MES.ControlsLibrary.MessageBox.Show(ms.AlertRetun("SFU1345"), null, "Information", MessageBoxButton.OKCancel, MessageBoxIcon.None, (result) =>
                        //HOLD �Ͻðڽ��ϱ�?
                        {
                            if (result == MessageBoxResult.OK)
                            {
                                if (drINPUT_LOT.Length > 500)
                                {
                                    //�޽��� �Է� 500�� �̻��̸�
                                    Util.Alert("SFU8102");  //�ִ� 1000�ڱ��� �����մϴ�.
                                    return;

                                }
                                else
                                {
                                    SetHold();
                                }
                            }
                        });
                    }
                }

            }
            catch (Exception ex)
            {
                Util.Alert(ex.ToString());
            }
        }
        #endregion

        #region ( Hold ȭ�� Ȧ�� )

        public void SetHold()
        {
            try
            {
                string sHoldReason_C = string.Empty;
                string sHoldCode_B = cboSearchSboxtype.Text.Split(':')[0].Trim();

                if (!(cboHoldReason.SelectedValue.ToString() == "N/A"))
                {
                    sHoldReason_C = txtHoldReason.Text.Split(':')[1].Trim();
                }
                if (cboHoldReason.SelectedValue.ToString() == "N/A")
                {
                    sHoldReason_C = txtHoldReason.Text;
                }

                if (dgSBoxList == null)
                {
                    return;
                }

                DataSet dsInput = new DataSet();

                DataTable INDATA = new DataTable();
                INDATA.TableName = "INDATA";
                INDATA.Columns.Add("SRCTYPE", typeof(string));
                INDATA.Columns.Add("HOLDTYPE", typeof(string));
                INDATA.Columns.Add("HOLD_REASON", typeof(string));
                INDATA.Columns.Add("USERID", typeof(string));

                DataRow drINDATA = INDATA.NewRow();
                drINDATA["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                drINDATA["HOLDTYPE"] = sHoldCode_B;
                drINDATA["HOLD_REASON"] = sHoldReason_C;
                drINDATA["USERID"] = LoginInfo.USERID;
                INDATA.Rows.Add(drINDATA);
                dsInput.Tables.Add(INDATA);

                DataRow drINLOT = null;
                DataTable INLOT = new DataTable();
                INLOT.TableName = "IN_HOLDID";
                INLOT.Columns.Add("MTRLID", typeof(string));
                INLOT.Columns.Add("HOLDID", typeof(string));

                for (int i = 0; i < dgSBoxList.Rows.Count - 1; i++)
                {
                    if (DataTableConverter.GetValue(dgSBoxList.Rows[i].DataItem, "CHK").ToString().Equals("True"))
                    {
                        drINLOT = INLOT.NewRow();
                        drINLOT["MTRLID"] = cboMtrlid.SelectedValue.ToString();
                        drINLOT["HOLDID"] = DataTableConverter.GetValue(dgSBoxList.Rows[i].DataItem, "S_BOX_ID").ToString();
                        INLOT.Rows.Add(drINLOT);
                    }
                }

                dsInput.Tables.Add(INLOT);

                DataTable IN_RANGE = new DataTable();
                IN_RANGE.TableName = "IN_RANGE";
                IN_RANGE.Columns.Add("MTRLID", typeof(string));
                IN_RANGE.Columns.Add("HOLD_FROM_DTTM", typeof(DateTime));
                IN_RANGE.Columns.Add("HOLD_TO_DTTM", typeof(DateTime));
                IN_RANGE.Columns.Add("SUPPLIERID", typeof(string));
                dsInput.Tables.Add(IN_RANGE);

                loadingIndicator.Visibility = Visibility.Visible;

                new ClientProxy().ExecuteService_Multi("BR_MTRL_REG_HOLD_PROC", "INDATA,IN_HOLDID,IN_RANGE", null, (dsResult, dataException) =>
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
                            if (!(dsResult == null))
                            {
                                //HOLD �Ϸ�.
                                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("SFU1344"), null, "Info", MessageBoxButton.OK, MessageBoxIcon.None);
                                dgSBoxList.Refresh();
                                Util.gridClear(dgSBoxList);

                            }
                            return;
                        }
                    }
                    catch (Exception ex)
                    {
                        throw ex;
                    }

                }, dsInput);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        #endregion

        #region ( ����HOLD Tab - HOLD ��ư )
        private void btnCaseHold_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (chkSave())
                {
                    string MtrlTypeName = Util.NVC(cboSearchMtrltype.Text);
                    string SupplierName = Util.NVC(cboSearchSup.Text);

                    //\r\n������ ������ HOLD �Ͻðڽ��ϱ�?\r\n[�����ڵ�: {0}]\r\n[��ü�ڵ�: {1}]\r\n[������: {2}]\r\n[������: {3}]
                    LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("\r\n������ ������ HOLD �Ͻðڽ��ϱ�?\r\n[�����ڵ�: {0}]\r\n[��ü�ڵ�: {1}]\r\n[������: {2}]\r\n[������: {3}]", new object[] { MtrlTypeName, SupplierName, dtpDateFromF.SelectedDateTime.ToString("yyyy-MM-dd"), dtpDateToF.SelectedDateTime.ToString("yyyy-MM-dd") }), null, "Information", MessageBoxButton.OKCancel, MessageBoxIcon.None, (result) =>
                        {
                            if (result == MessageBoxResult.OK)
                            {
                                //����
                                saveCaseHold();

                            }
                        });


                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void saveCaseHold()
        {
            try
            {

                string Range = "RANGE";
                string sHoldReason_D = string.Empty;
                string sMtrlType = Util.NVC(cboSearchMtrltype.SelectedValue.ToString());
                string sSuplType = Util.NVC(cboSearchSup.SelectedValue.ToString());

                

                //string sHoldReason = Util.NVC(txtHoldCaseReason.Text);

                if (!(cboHoldCaseReason.SelectedValue.ToString() == "N/A"))
                {
                    sHoldReason_D = txtHoldCaseReason.Text.Split(':')[1].Trim();
                }
                if (cboHoldCaseReason.SelectedValue.ToString() == "N/A")
                {
                    sHoldReason_D = txtHoldCaseReason.Text;
                }

                DataSet dsInput = new DataSet();

                DataTable INDATA = new DataTable();
                INDATA.TableName = "INDATA";
                INDATA.Columns.Add("SRCTYPE", typeof(string));
                INDATA.Columns.Add("HOLDTYPE", typeof(string));
                INDATA.Columns.Add("HOLD_REASON", typeof(string));
                INDATA.Columns.Add("USERID", typeof(string));

                DataRow drINDATA = null;
                drINDATA = INDATA.NewRow();
                drINDATA["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                drINDATA["HOLDTYPE"] = Range;
                drINDATA["HOLD_REASON"] = sHoldReason_D;
                drINDATA["USERID"] = LoginInfo.USERID;
                INDATA.Rows.Add(drINDATA);
                dsInput.Tables.Add(INDATA);

                DataTable INLOT = new DataTable();
                INLOT.TableName = "IN_HOLDID";
                INLOT.Columns.Add("MTRLID", typeof(string));
                INLOT.Columns.Add("HOLDID", typeof(string));
                dsInput.Tables.Add(INLOT);

                DataTable IN_RANGE = new DataTable();
                IN_RANGE.TableName = "IN_RANGE";
                IN_RANGE.Columns.Add("MTRLID", typeof(string));
                IN_RANGE.Columns.Add("HOLD_FROM_DTTM", typeof(DateTime));
                IN_RANGE.Columns.Add("HOLD_TO_DTTM", typeof(DateTime));
                IN_RANGE.Columns.Add("SUPPLIERID", typeof(string));

                DataRow drIN_RANGE = null;
                drIN_RANGE = IN_RANGE.NewRow();
                drIN_RANGE["MTRLID"] = sMtrlType.Trim();
                drIN_RANGE["HOLD_FROM_DTTM"] = dtpDateFromF.SelectedDateTime;//.ToString("yyyy-MM-dd"); //Util.StringToDateTime(istartDay);
                drIN_RANGE["HOLD_TO_DTTM"] = dtpDateToF.SelectedDateTime;//.ToString("yyyy-MM-dd"); // Util.StringToDateTime(iendDay);
                drIN_RANGE["SUPPLIERID"] = sSuplType;
                IN_RANGE.Rows.Add(drIN_RANGE);

                dsInput.Tables.Add(IN_RANGE);

                new ClientProxy().ExecuteService_Multi("BR_MTRL_REG_HOLD_PROC", "INDATA,IN_HOLDID,IN_RANGE", null, (dsResult, dataException) =>
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
                            if (!(dsResult == null))
                            {
                                //HOLD �Ϸ�.
                                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("SFU1344"), null, "Info", MessageBoxButton.OK, MessageBoxIcon.None);

                            }
                            return;
                        }
                    }
                    catch (Exception ex)
                    {
                        throw ex;
                    }

                }, dsInput);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        #endregion

        #region ( Hold ȭ�� ��ȸ )
        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (Check_HoldHist())
                {
                    getHoldCell();
                }

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }


        #endregion

        #region ( Hold �̷�ȭ�� ��ü ���� )
        private void chkHeaderAll_Checked(object sender, RoutedEventArgs e)
        {
            int iChkCnt = 0;

            foreach (C1.WPF.DataGrid.DataGridRow row in dgHoldHistory.Rows)
            {
                CheckBox o = dgHoldHistory.GetCell(row.Index, 0).Presenter?.Content as CheckBox;
                if (o != null && o.IsEnabled)
                {
                    DataTableConverter.SetValue(row.DataItem, "CHK", true);
                    iChkCnt++;
                }
            }

            if (iChkCnt > 0)
            {
                btnHoldRelease.IsEnabled = true;
                txtNote.IsEnabled = true;
            }

            dgHoldHistory.EndEdit();
            dgHoldHistory.EndEditRow(true);
        }
        #endregion

        #region ( Hold �̷�ȭ�� ��ü ��� )
        private void chkHeaderAll_Unchecked(object sender, RoutedEventArgs e)
        {
            foreach (C1.WPF.DataGrid.DataGridRow row in dgHoldHistory.Rows)
            {

                DataTableConverter.SetValue(row.DataItem, "CHK", false);
                btnHoldRelease.IsEnabled = false;
                txtNote.IsEnabled = false;
            }

            dgHoldHistory.EndEdit();
            dgHoldHistory.EndEditRow(true);
        }
        #endregion

        #region ( Hold ����ȭ�� üũ�ڽ���ü ���� )
        private void chkHeaderAllList_Checked(object sender, RoutedEventArgs e)
        {
            foreach (C1.WPF.DataGrid.DataGridRow row in dgSBoxList.Rows)
            {
                DataTableConverter.SetValue(row.DataItem, "CHK", true);
            }

            dgSBoxList.EndEdit();
            dgSBoxList.EndEditRow(true);
        }
        #endregion

        #region HoldHistory Grid ���ε� ó��
        private void dgHoldHistory_LoadedCellPresenter(object sender, DataGridCellEventArgs e)
        {
            try
            {
                C1DataGrid dataGrid = sender as C1DataGrid;
                dataGrid?.Dispatcher.BeginInvoke(new Action(() =>
                {
                    if (e.Cell.Presenter == null) return;
                    if (e.Cell.Row.Type != DataGridRowType.Item) return;

                    if (e.Cell.Column.Name == "CHK")
                    {
                        string sHoldFlag = Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "HOLD_FLAG"));
                        if (sHoldFlag.Equals("N"))
                        {
                            CheckBox o = e.Cell.Presenter.Content as CheckBox;
                            o.IsEnabled = false;
                            o.Visibility = Visibility.Collapsed;
                        }
                    }
                }
                ));
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        #endregion

        #region ( Hold ����ȭ�� üũ�ڽ���ü ��� )
        private void chkHeaderAllList_Unchecked(object sender, RoutedEventArgs e)
        {
            foreach (C1.WPF.DataGrid.DataGridRow row in dgSBoxList.Rows)
            {
                DataTableConverter.SetValue(row.DataItem, "CHK", false);
            }
            dgSBoxList.EndEdit();
            dgSBoxList.EndEditRow(true);
        }
        #endregion
        private void dgHoldHistory_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (!(dgHoldHistory.ItemsSource == null || dgHoldHistory.GetRowCount() == 0))
            {
                Point pnt = e.GetPosition(null);
                C1.WPF.DataGrid.DataGridCell cell = dgHoldHistory.GetCellFromPoint(pnt);

                if (!(bool)ChkOption.IsSelected)
                {
                    return;
                }

                if (cell.Column.Name.Equals("CHK"))
                {
                    CheckBox o = dgHoldHistory.GetCell(cell.Row.Index, 0).Presenter?.Content as CheckBox;

                    if (o == null) return;
                    if (!o.IsEnabled) { o.IsChecked = false; return; }

                    DataTable dt = DataTableConverter.Convert(dgHoldHistory.ItemsSource);

                    btnHoldRelease.IsEnabled = false;
                    txtNote.IsEnabled = false;

                    for (int i = 0; i < dt.Rows.Count; i++)
                    {
                        if (dt.Rows[i][0].ToString() == "True")
                        {
                            btnHoldRelease.IsEnabled = true;
                            txtNote.IsEnabled = true;
                            break;
                        }
                    }

                }
            }

        }

        #endregion

        #region Mehod

        #region ( Hold ȭ�� �̷� ��ȸ )
        private void getHoldCell()
        {
            try
            {
                string strInputWait = null;


                if (cboHoldType.SelectedIndex == 2)
                {
                    strInputWait = "Y";
                }


                    loadingIndicator.Visibility = Visibility.Visible;
                //��ȸ
                DataTable RQSTDT = new DataTable();

                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("CHK", typeof(string));
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("EQSGID", typeof(string));
                RQSTDT.Columns.Add("MTRLID", typeof(string));
                RQSTDT.Columns.Add("SUPPLIERID", typeof(string));
                RQSTDT.Columns.Add("HOLD_FROM_DTTM", typeof(string));
                RQSTDT.Columns.Add("HOLD_TO_DTTM", typeof(string));
                RQSTDT.Columns.Add("HOLD_FLAG", typeof(string));
                RQSTDT.Columns.Add("HOLD_TRGT_CODE", typeof(string));
                RQSTDT.Columns.Add("INPUT_WAIT", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                
                dr["CHK"] = false;
                dr["LANGID"] = LoginInfo.LANGID;
                dr["EQSGID"] = strInputWait == "Y" ? null : (cboHoldType.SelectedIndex == 0 ? ( Util.GetCondition(cboEquipmentSegment) == "" ? null : Util.GetCondition(cboEquipmentSegment)) : null);
                dr["MTRLID"] = cboSBoxType.SelectedIndex < 1 ? null : cboSBoxType.SelectedValue.ToString();
                dr["SUPPLIERID"] = cboSuppliercode.SelectedIndex < 1  ? null : cboSuppliercode.SelectedValue.ToString();
                dr["HOLD_FROM_DTTM"] = Util.GetCondition(dtpDateFrom);
                dr["HOLD_TO_DTTM"] = Util.GetCondition(dtpDateTo);
                dr["HOLD_FLAG"] = cboHoldFlag.SelectedIndex < 1 ? null : cboHoldFlag.SelectedValue.ToString();
                dr["HOLD_TRGT_CODE"] = cboHoldType.SelectedIndex == 1 ? "RANGE" : null;
                dr["INPUT_WAIT"] = strInputWait;




                RQSTDT.Rows.Add(dr);

                new ClientProxy().ExecuteService("DA_MTRL_SEL_MTRLHOLD_HIST", "RQSTDT", "RSLTDT", RQSTDT, (dtResult, ex) =>
                {

                    if (ex != null)
                    {
                        Util.MessageException(ex);
                        return;
                    }

                    if (dtResult.Rows.Count != 0)
                    {
                        Util.GridSetData(dgHoldHistory, dtResult, FrameOperation);

                        Util.SetTextBlockText_DataGridRowCount(tbResult_cnt, Util.NVC(dtResult.Rows.Count));
                    }
                    else
                    {
                        Util.gridClear(dgHoldHistory);
                        Util.SetTextBlockText_DataGridRowCount(tbResult_cnt, "0");
                        Util.MessageInfo("SFU3537");
                    }
                    loadingIndicator.Visibility = Visibility.Collapsed;
                });

            }
            catch (Exception ex)
            {
                loadingIndicator.Visibility = Visibility.Collapsed;
                Util.MessageException(ex);
            }
        }

        

        //����HOLD_�����ڵ�
        private void SetMtrlGrCode_sum(C1ComboBox cbo, CommonCombo.ComboStatus cs, String[] sFilter)
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("EQSGID", typeof(string));

                //�׽�Ʈ��
                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["EQSGID"] = cboEquipmentSegment.SelectedValue.ToString();

                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_MTRL_SEL_S_BOX_MATID_CBO", "RQSTDT", "RSLTDT", RQSTDT);

                cbo.DisplayMemberPath = "CBO_NAME";
                cbo.SelectedValuePath = "CBO_CODE";
                cbo.ItemsSource = AddStatus(dtResult, cs, "CBO_CODE", "CBO_NAME").Copy().AsDataView();

                if (cbo.SelectedIndex < 0)
                {
                    cbo.SelectedIndex = 0;
                }
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }
        private void SetSearchMtrltype_sum(C1ComboBox cbo, CommonCombo.ComboStatus cs, String[] sFilter)
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("EQSGID", typeof(string));

                //�׽�Ʈ��
                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["EQSGID"] = cboEquipmentSegment.SelectedValue.ToString();

                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_MTRL_SEL_S_BOX_MATID_CBO", "RQSTDT", "RSLTDT", RQSTDT);

                cbo.DisplayMemberPath = "CBO_NAME";
                cbo.SelectedValuePath = "CBO_CODE";
                cbo.ItemsSource = AddStatus(dtResult, cs, "CBO_CODE", "CBO_NAME").Copy().AsDataView();

                if (cbo.SelectedIndex < 0)
                {
                    cbo.SelectedIndex = 0;
                }

                else
                {
                    cbo.SelectedIndex = 0;
                }
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }
        //����HOLD_��ü�ڵ�
        private void SetCaseSup_sum(C1ComboBox cbo, CommonCombo.ComboStatus cs, String[] sFilter)
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("MTRLID", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["MTRLID"] = sFilter[0] == "" ? null : sFilter[0];
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_MTRL_SEL_S_BOX_SUPPLIER_CBO", "RQSTDT", "RSLTDT", RQSTDT);

                cbo.DisplayMemberPath = "CBO_NAME";
                cbo.SelectedValuePath = "CBO_CODE";
                cbo.ItemsSource = AddStatus(dtResult, cs, "CBO_CODE", "CBO_NAME").Copy().AsDataView();

                if (cbo.SelectedIndex < 0)
                {
                    cbo.SelectedIndex = 0;
                }

                else
                {
                    cbo.SelectedIndex = 0;
                }
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }
        private void SetHoldReason_sum(C1ComboBox cbo, CommonCombo.ComboStatus cs, String[] sFilter)
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("CMCDTYPE", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["CMCDTYPE"] = "PACK_MTRL_HOLD_RESNCODE";
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_COMMCODE_CBO", "RQSTDT", "RSLTDT", RQSTDT);
                DataTable dsResult = dtResult.Copy();
                
                DataRow dt = dsResult.NewRow();
                dt["CBO_CODE"] = "N/A";
                dt["CBO_NAME"] = "-���� �Է�-";
                dsResult.Rows.Add(dt);
                
                cbo.DisplayMemberPath = "CBO_NAME";
                cbo.SelectedValuePath = "CBO_CODE";


                txtHoldReason.IsEnabled = false;
                if (cbo.SelectedIndex < 0)
                {
                    cbo.ItemsSource = AddStatus(dsResult, cs, "CBO_CODE", "CBO_NAME").Copy().AsDataView();
                    cbo.SelectedIndex = 0;
                }
                else if(cbo.Text == "-���� �Է�-")
                {
                    txtHoldReason.IsEnabled = true;
                    txtHoldReason.Clear();
                }
                else
                {
                    txtHoldReason.Text = cbo.Text;
                }
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }
        private void SetHoldCaseReason_sum(C1ComboBox cbo, CommonCombo.ComboStatus cs, String[] sFilter)
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("CMCDTYPE", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["CMCDTYPE"] = "PACK_MTRL_HOLD_RESNCODE";
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_COMMCODE_CBO", "RQSTDT", "RSLTDT", RQSTDT);
                DataTable dsResult = dtResult.Copy();

                DataRow dt = dsResult.NewRow();
                dt["CBO_CODE"] = "N/A";
                dt["CBO_NAME"] = "-���� �Է�-";
                dsResult.Rows.Add(dt);

                cbo.DisplayMemberPath = "CBO_NAME";
                cbo.SelectedValuePath = "CBO_CODE";


                txtHoldCaseReason.IsEnabled = false;
                if (cbo.SelectedIndex < 0)
                {
                    cbo.ItemsSource = AddStatus(dsResult, cs, "CBO_CODE", "CBO_NAME").Copy().AsDataView();
                    cbo.SelectedIndex = 0;
                }
                else if (cbo.Text == "-���� �Է�-")
                {
                    txtHoldCaseReason.IsEnabled = true;
                    txtHoldCaseReason.Clear();
                }
                else
                {
                    txtHoldCaseReason.Text = cbo.Text;
                }
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }
        

        bool Multi_Create(string sLotid)
        {
            try
            {
                DoEvents();

                string sHoldCode = cboSearchSboxtype.Text.Split(':')[0].Trim();
                MatchCollection matches_M = Regex.Matches(sLotid, ";");
                MatchCollection matches_S = Regex.Matches(sLotid, ":");
                int cnt_M = matches_M.Count;
                int cnt_S = matches_S.Count;
                
                DataTable inTable = new DataTable();
                inTable.Columns.Add("CHK", typeof(string));
                inTable.Columns.Add("S_BOX_ID", typeof(string));

                DataRow dr = inTable.NewRow();
                dr["CHK"] = true;
                dr["S_BOX_ID"] = sLotid;

                inTable.Rows.Add(dr);
                
                    DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_MTRL_SEL_HOLD_SBOXID", "INDATA", "OUTDATA", inTable);
                    
                    if (dtResult.Rows.Count > 0)
                    {
                        if (sEmpty_Lot == "")
                            sEmpty_Lot += sLotid;
                        else
                            sEmpty_Lot = sEmpty_Lot + ", " + sLotid;
                    }

                    if (dgSBoxList.GetRowCount() == 0)
                    {
                        Util.GridSetData(dgSBoxList, inTable, FrameOperation);
                    }
                    else
                    {
                        DataTable dtInfo = DataTableConverter.Convert(dgSBoxList.ItemsSource);

                        dtInfo.Merge(inTable);
                        Util.GridSetData(dgSBoxList, dtInfo, FrameOperation);
                    }
                    isCreateTable = DataTableConverter.Convert(dgSBoxList.GetCurrentItems());

                    return true;
                
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return false;
            }
        }
        #endregion

        public void Multi(string sLotid)
        {
            try
            {
                DoEvents();

                if (dgSBoxList == null)
                {
                    return;
                }

                DataSet dsInput = new DataSet();

                DataTable INDATA = new DataTable();
                INDATA.TableName = "INDATA";
                INDATA.Columns.Add("SRCTYPE", typeof(string));
                INDATA.Columns.Add("HOLDTYPE", typeof(string));
                INDATA.Columns.Add("HOLD_REASON", typeof(string));
                INDATA.Columns.Add("USERID", typeof(string));

                DataRow drINDATA = INDATA.NewRow();
                drINDATA["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                drINDATA["HOLDTYPE"] = cboSearchSboxtype.Text;
                drINDATA["HOLD_REASON"] = Util.GetCondition(cboHoldReason) == "" ? null : cboHoldReason.SelectedValue;
                drINDATA["USERID"] = LoginInfo.USERID;
                INDATA.Rows.Add(drINDATA);

                DataTable IN_HOLDID = new DataTable();
                IN_HOLDID.TableName = "IN_HOLDID";
                IN_HOLDID.Columns.Add("MTRLID", typeof(string));
                IN_HOLDID.Columns.Add("HOLDID", typeof(string));


                DataRow drINLOT = IN_HOLDID.NewRow();
                drINDATA["MTRLID"] = "";
                drINDATA["HOLDID"] = txtSboxId.Text;
                IN_HOLDID.Rows.Add(drINLOT);

                dsInput.Tables.Add(INDATA);
                dsInput.Tables.Add(IN_HOLDID);



                DataSet dsResult = new ClientProxy().ExecuteServiceSync_Multi("BR_MTRL_REG_HOLD_PROC", "INDATA,IN_HOLDID", "OUTDATA", dsInput);
                DataTable dtResult = dsResult.Tables["OUTDATA"];

                if (dtResult.Rows.Count == 0)
                {
                    if (sEmpty_sboxid == "")
                    {
                        sEmpty_sboxid += sLotid;
                    }
                    else
                    {
                        sEmpty_sboxid = sEmpty_sboxid + ", " + sLotid;
                    }
                    return;
                }
                else
                {
                    for (int i = 0; i < dtResult.Rows.Count; i++)
                    {
                        if (dtResult.Rows.Count != 0)
                        {

                        }
                    }
                    if (dgSBoxList.GetRowCount() == 0)
                    {
                        Util.GridSetData(dgSBoxList, dtResult, FrameOperation);

                    }
                    else
                    {
                        DataTable dtInfo = DataTableConverter.Convert(dgSBoxList.ItemsSource);

                        dtInfo.Merge(dtResult);
                        Util.GridSetData(dgSBoxList, dtInfo, FrameOperation);
                    }
                    isSearchTable = DataTableConverter.Convert(dgSBoxList.GetCurrentItems());
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        //Ȧ�帱����
        private void SetMtrlUnHold(string bigo)
        {
            try
            {
                bool chk_vali = false;

                if (dgHoldHistory.ItemsSource == null || dgHoldHistory.GetRowCount() == 0)
                {
                    return;
                }

                if (txtNote.Text == "")
                {
                    ms.AlertWarning("SFU4301"); //Hold ���� ������ �Է��ϼ���.
                    return;
                }

                //���� �۾� ����
                for (int i = 0; i < dgHoldHistory.Rows.Count; i++)
                {
                    var chkYn = DataTableConverter.GetValue(dgHoldHistory.Rows[i].DataItem, "CHK");

                    if (chkYn != null)
                    {
                        if (Convert.ToBoolean(chkYn))
                        {
                            chk_vali = true;
                        }
                    }
                }

                if (!chk_vali)
                {
                    ms.AlertWarning("SFU1651"); //���õ� �׸��� �����ϴ�.
                    return;
                }

                setUnHold();

                //Util.AlertInfo("HOLD ���� �Ϸ�");
                ms.AlertInfo("SFU1268"); //UNHOLD �Ϸ�
                getHoldCell();
                txtNote.Text = "";

                btnHoldRelease.IsEnabled = false;
                txtNote.IsEnabled = false;

            }
            catch (Exception ex)
            {
                Util.AlertInfo(ex.Message);
            }
        }
        private void setUnHold()
        {
            try
            {
                if (dgHoldHistory == null)
                {
                    return;
                }

                DataSet dsInput = new DataSet();

                DataTable INDATA = new DataTable();
                INDATA.TableName = "INDATA";
                INDATA.Columns.Add("SRCTYPE", typeof(string));
                INDATA.Columns.Add("RELEASE_REASON", typeof(string));
                INDATA.Columns.Add("USERID", typeof(string));

                DataRow drINDATA = INDATA.NewRow();
                drINDATA["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                drINDATA["RELEASE_REASON"] = txtNote.Text;
                drINDATA["USERID"] = LoginInfo.USERID;
                INDATA.Rows.Add(drINDATA);

                DataTable IN_RELEASE_SEQ = new DataTable();
                IN_RELEASE_SEQ.TableName = "IN_RELEASE_SEQ";
                IN_RELEASE_SEQ.Columns.Add("HOLD_SEQNO", typeof(string));

                int chk_idx = 0;
                int total_row = dgHoldHistory.GetRowCount();
                for (int i = 0; i < total_row; i++)
                {
                    if (DataTableConverter.GetValue(dgHoldHistory.Rows[i].DataItem, "CHK").ToString() == "True")
                    {
                        DataRow drINLOT = IN_RELEASE_SEQ.NewRow();
                        drINLOT["HOLD_SEQNO"] = DataTableConverter.GetValue(dgHoldHistory.Rows[i].DataItem, "HOLD_SEQNO").ToString();

                        IN_RELEASE_SEQ.Rows.Add(drINLOT);

                        chk_idx++;
                    }
                }

                if (chk_idx == 0)
                {
                    return;
                }

                dsInput.Tables.Add(INDATA);
                dsInput.Tables.Add(IN_RELEASE_SEQ);

                new ClientProxy().ExecuteServiceSync_Multi("BR_MTRL_REG_RELEASE_PROC", "INDATA,IN_RELEASE_SEQ", null, dsInput);

            }
            catch (Exception ex)
            {
                throw ex;
            }
        }



        #endregion

       

       
    }
}