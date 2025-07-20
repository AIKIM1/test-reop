/*************************************************************************************
 Created Date : 2016.06.16
      Creator : 
   Decription : 
--------------------------------------------------------------------------------------
 [Change History]
  2016.06.16  DEVELOPER : Initial Created.
  2018.05.02 �տ켮 ERP ���� ���� �� ��ȸ�� ��� �÷� �߰�
  2018.06.28 �տ켮 ���� ��ǰ�� �ٸ� ���ο��� �����ϴ� ��쿡 ���Ͽ� ��ȸ �Ķ���� �߰�
  2018.10.22 �տ켮 CSR ID 3805398 GMES ERP������� ��ȸ ȭ�� ��� �߰� ��û �� ��û��ȣ C20181001_05398
  2019.01.24 �տ켮 CSR ID 3859176 GMES W/O ���� ���� �� �����ϰ� ���� ��� ���� ��û�� �� [��û��ȣ]C20181130_59176
  2019.03.12 �̴�� CSR ID 3909890 GMES Pack UI ���� ��û (4��) [��û��ȣ] C20190129_09890
  2019.03.25 �տ켮 CSR ID 3942803 LGCMI ���1ȣ ����U611 LGC�����ڵ� ���� ��û �� | [��û��ȣ]C20190307_42803
  2020.05.26 �տ켮 ��ȸ �Ķ���� LANGID �߰�
**************************************************************************************/

using System;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using C1.WPF;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;

namespace LGC.GMES.MES.PACK001
{
    public partial class PACK001_031 : UserControl, IWorkArea
    {
        #region Declaration & Constructor 
        LGC.GMES.MES.PACK001.Class.MESSAGE_PARAM ms = new LGC.GMES.MES.PACK001.Class.MESSAGE_PARAM();
     
        public IFrameOperation FrameOperation
        {
            get;
            set;
        }

        public PACK001_031()
        {
            InitializeComponent();

            tbResult_cnt.Text = "[ 0 " + ObjectDic.Instance.GetObjectName("��") + " ]";
            tbDefectInform_cnt.Text = "[ 0 " + ObjectDic.Instance.GetObjectName("��") + " ]";

            this.Loaded += PACK001_014_Loaded;
        }

        #endregion

        #region Initialize
        private void Initialize()
        {
            //���� �ʱⰪ ���� : main ��ȸ����
            dtpDateFrom.SelectedDateTime = (DateTime)System.DateTime.Now.AddDays(-7); //������ �� ��¥
            dtpDateTo.SelectedDateTime = (DateTime)System.DateTime.Now;//���� ��¥

            //2019-01-24
            if (LoginInfo.CFG_SHOP_ID == "G481")
            {
                tbComment.Visibility = Visibility.Visible;
                btnWoChange.Visibility = Visibility.Visible;
            }

            InitCombo();
        }
        #endregion

        #region Event
        private void PACK001_014_Loaded(object sender, RoutedEventArgs e)
        {
            Initialize();

            this.Loaded -= PACK001_014_Loaded;
        }

        #region Button
        private void btnExcelDetail_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                new ExcelExporter().Export(dgFalseList);
            }
            catch (Exception ex)
            {
                Util.AlertInfo(ex.Message);
            }
        }

        private void btnExcelSummary_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                new ExcelExporter().Export(dgSearchResult);
            }
            catch (Exception ex)
            {
                Util.AlertInfo(ex.Message);
            }
        }

        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                dgSearchResult.ItemsSource = null;

                search();

                

                dgFalseList.ItemsSource = null;

                Util.SetTextBlockText_DataGridRowCount(tbDefectInform_cnt, Util.NVC(0));

            }
            catch (Exception ex)
            {
                Util.AlertInfo(ex.Message);
            }
        }     

        private void btnReTran_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (dgFalseList.GetRowCount() == 0) 
                {
                    return;
                }

                int CHK_CNT = 0;
                int fail_cnt = 0;

                for (int i = 0; i < dgFalseList.GetRowCount(); i++) //�����Ϸ��� row�� ���ۻ��� üũ
                {
                    if (DataTableConverter.GetValue(dgFalseList.Rows[i].DataItem, "CHK").ToString() == "True")
                    {
                        CHK_CNT++;

                        if (DataTableConverter.GetValue(dgFalseList.Rows[i].DataItem, "ERP_ERR_TYPE_CODE").ToString() == "FAIL")
                        {
                            fail_cnt++;
                        }
                    }
                        
                }

                if(CHK_CNT == 0) //CHKBOX�� üũ�� �ϳ��� �� �� ��Ȳ
                {
                    return;
                }

                if(fail_cnt ==0) //üũ�� �ߴµ� ���ۻ��°� Fail�� �ϳ��� ���� ���
                {
                    return;
                }

                DataTable INDATA = new DataTable();
                INDATA.TableName = "INDATA";
                INDATA.Columns.Add("ERP_TRNF_SEQNO", typeof(string));
                
                int total_row = dgFalseList.GetRowCount();
                string erp_trnf_old = string.Empty;

                for (int i = 0; i < total_row; i++)
                {
                    if (DataTableConverter.GetValue(dgFalseList.Rows[i].DataItem, "CHK").ToString() == "True")
                    {
                        if (DataTableConverter.GetValue(dgFalseList.Rows[i].DataItem, "ERP_ERR_TYPE_CODE").ToString() == "FAIL")
                        {
                            if(erp_trnf_old.Length != 0)
                            {
                                if(erp_trnf_old != DataTableConverter.GetValue(dgFalseList.Rows[i].DataItem, "ERP_TRNF_SEQNO").ToString())
                                {
                                    DataRow drINDATA = INDATA.NewRow();
                                    drINDATA["ERP_TRNF_SEQNO"] = DataTableConverter.GetValue(dgFalseList.Rows[i].DataItem, "ERP_TRNF_SEQNO").ToString();

                                    INDATA.Rows.Add(drINDATA);

                                    erp_trnf_old = DataTableConverter.GetValue(dgFalseList.Rows[i].DataItem, "ERP_TRNF_SEQNO").ToString();
                                }
                            }
                            else
                            {
                                DataRow drINDATA = INDATA.NewRow();
                                drINDATA["ERP_TRNF_SEQNO"] = DataTableConverter.GetValue(dgFalseList.Rows[i].DataItem, "ERP_TRNF_SEQNO").ToString();

                                INDATA.Rows.Add(drINDATA);

                                erp_trnf_old = DataTableConverter.GetValue(dgFalseList.Rows[i].DataItem, "ERP_TRNF_SEQNO").ToString();
                            }
                        }
                    }
                }

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("BR_PRD_ERP_RETRAN", "INDATA", null, INDATA);
            }
            catch (Exception ex)
            {               
                Util.MessageException(ex);
            }
        }

        //2019-01-24
        private void btnWoChange_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                PACK001_031_WORKORDER_CHANGE popup = new PACK001_031_WORKORDER_CHANGE();
                popup.FrameOperation = this.FrameOperation;

                if (popup != null)
                {
                    DataTable dtData = new DataTable();
                    dtData.Columns.Add("ERP_TRNF_SEQNO", typeof(string));
                    dtData.Columns.Add("EQSGID", typeof(string));
                    dtData.Columns.Add("LOTID", typeof(string));
                    dtData.Columns.Add("PRODID", typeof(string));

                    DataRow newRow = null;

                    int iRowCnt = 0;

                    string Be_Prod = string.Empty;
                    string Af_Prod = string.Empty;

                    for (int i = 0; i < dgFalseList.GetRowCount(); i++)
                    {
                        if (DataTableConverter.GetValue(dgFalseList.Rows[i].DataItem, "CHK").ToString() == "True")
                        {
                            if (DataTableConverter.GetValue(dgFalseList.Rows[i].DataItem, "DFCT_QTY").ToString() == "0")
                            {
                                if (DataTableConverter.GetValue(dgFalseList.Rows[i].DataItem, "ERP_RSLT_TYPE_CODE").ToString() == "BOXLOT")
                                {
                                    if (dtData.Rows.Count > 0)
                                    {
                                        Be_Prod = dtData.Rows[iRowCnt - 1]["PRODID"].ToString();
                                        Af_Prod = DataTableConverter.GetValue(dgFalseList.Rows[i].DataItem, "PRODID").ToString();

                                        if (Be_Prod == Af_Prod)
                                        {
                                            iRowCnt = iRowCnt + 1;
                                            newRow = dtData.NewRow();
                                            newRow["ERP_TRNF_SEQNO"] = DataTableConverter.GetValue(dgFalseList.Rows[i].DataItem, "ERP_TRNF_SEQNO").ToString();
                                            newRow["EQSGID"] = Util.NVC(cboEquipmentSegment.SelectedValue);
                                            newRow["LOTID"] = DataTableConverter.GetValue(dgFalseList.Rows[i].DataItem, "LOTID").ToString();
                                            newRow["PRODID"] = DataTableConverter.GetValue(dgFalseList.Rows[i].DataItem, "PRODID").ToString();

                                            dtData.Rows.Add(newRow);
                                        }
                                    }
                                }
                            }
                            else
                            {
                                if (DataTableConverter.GetValue(dgFalseList.Rows[i].DataItem, "DFCT_QTY").ToString() == "0")
                                {
                                    if (DataTableConverter.GetValue(dgFalseList.Rows[i].DataItem, "ERP_RSLT_TYPE_CODE").ToString() == "BOXLOT")
                                    {
                                        iRowCnt = iRowCnt + 1;
                                        newRow = dtData.NewRow();
                                        newRow["ERP_TRNF_SEQNO"] = DataTableConverter.GetValue(dgFalseList.Rows[i].DataItem, "ERP_TRNF_SEQNO").ToString();
                                        newRow["EQSGID"] = Util.NVC(cboEquipmentSegment.SelectedValue);
                                        newRow["LOTID"] = DataTableConverter.GetValue(dgFalseList.Rows[i].DataItem, "LOTID").ToString();
                                        newRow["PRODID"] = DataTableConverter.GetValue(dgFalseList.Rows[i].DataItem, "PRODID").ToString();

                                        dtData.Rows.Add(newRow);
                                    }
                                }
                            }
                        }
                    }
                    
                    if (dtData.Rows.Count > 0)
                    {
                        //========================================================================
                        object[] Parameters = new object[1];
                        Parameters[0] = dtData;
                        C1WindowExtension.SetParameters(popup, Parameters);
                        //========================================================================

                        popup.Closed -= popup_Closed;
                        popup.Closed += popup_Closed;
                        popup.ShowModal();
                        popup.CenterOnScreen();
                    }
                    else
                    {
                        Util.MessageValidation("SFU1261");
                        return;
                    }
                }
            }
            catch (Exception ex)
            {
                Util.AlertInfo(ex.Message);
            }
        }
        #endregion Button

        #region Grid
        private void dgFalseList_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            try
            {
                if(dgFalseList.GetRowCount() == 0)
                {
                    return;
                }

                Point pnt = e.GetPosition(null);
                C1.WPF.DataGrid.DataGridCell cell = dgFalseList.GetCellFromPoint(pnt);

                if (cell == null || cell.Value == null)
                {
                    return;
                }

                int seleted_row = dgFalseList.CurrentRow.Index;

                string postdate = string.Empty;
                string prodid = string.Empty;
                string ERP_TRNF_SEQNO = string.Empty;

                postdate = DataTableConverter.GetValue(dgFalseList.Rows[seleted_row].DataItem, "POST_DATE").ToString();
                prodid = DataTableConverter.GetValue(dgFalseList.Rows[seleted_row].DataItem, "PRODID").ToString();
                ERP_TRNF_SEQNO = DataTableConverter.GetValue(dgFalseList.Rows[seleted_row].DataItem, "ERP_TRNF_SEQNO").ToString(); ;

                if (DataTableConverter.GetValue(dgFalseList.Rows[seleted_row].DataItem, "CHK").ToString() == "True")
                {
                    for (int i = 0; i < dgFalseList.GetRowCount(); i++)
                    {
                        if (ERP_TRNF_SEQNO == DataTableConverter.GetValue(dgFalseList.Rows[i].DataItem, "ERP_TRNF_SEQNO").ToString())
                        {
                            DataTableConverter.SetValue(dgFalseList.Rows[i].DataItem, "CHK", "True");
                        }
                    }
                }
                else
                {
                    for (int i = 0; i < dgFalseList.GetRowCount(); i++)
                    {
                        if (ERP_TRNF_SEQNO == DataTableConverter.GetValue(dgFalseList.Rows[i].DataItem, "ERP_TRNF_SEQNO").ToString())
                        {
                            DataTableConverter.SetValue(dgFalseList.Rows[i].DataItem, "CHK", "False");
                        }
                    }
                }

            }
            catch (Exception ex)
            {               
                Util.MessageException(ex);
            }
        }

        private void dgFalseList_LoadedColumnHeaderPresenter(object sender, C1.WPF.DataGrid.DataGridColumnEventArgs e)
        {
            //if(dgFalseList.GetRowCount() ==0)
            //{
            //    return;
            //}

            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (string.IsNullOrEmpty(e.Column.Name) == false)
                {
                    if (e.Column.Name.Equals("CHK"))
                    {
                        pre.Content = chkAll;
                        e.Column.HeaderPresenter.Content = pre;
                        chkAll.Checked -= new RoutedEventHandler(checkAll_Checked);
                        chkAll.Unchecked -= new RoutedEventHandler(checkAll_Unchecked);
                        chkAll.Checked += new RoutedEventHandler(checkAll_Checked);
                        chkAll.Unchecked += new RoutedEventHandler(checkAll_Unchecked);
                    }
                }
            }));
        }

        //private void dgFalseList_CommittedEdit(object sender, C1.WPF.DataGrid.DataGridCellEventArgs e)
        //{

        //   C1.WPF.DataGrid.C1DataGrid dg = sender as C1.WPF.DataGrid.C1DataGrid;

        //    if (e.Cell != null &&
        //        e.Cell.Presenter != null &&
        //        e.Cell.Presenter.Content != null)
        //    {
        //        CheckBox chk = e.Cell.Presenter.Content as CheckBox;
        //        if (chk != null)
        //        {
        //            switch (Convert.ToString(e.Cell.Column.Name))
        //            {
        //                case "CHK":

        //                    chkAll.Checked -= new RoutedEventHandler(checkAll_Checked);
        //                    chkAll.Unchecked -= new RoutedEventHandler(checkAll_Unchecked);
        //                    DataTable dt = DataTableConverter.Convert(dg.ItemsSource);

        //                    if (!chk.IsChecked.Value)
        //                    {
        //                        chkAll.IsChecked = false;
        //                    }
        //                    else if (dt.AsEnumerable().Where(row => Convert.ToBoolean(row["CHK"]) == true).Count() == dt.Rows.Count)
        //                    {
        //                        chkAll.IsChecked = true;
        //                    }

        //                    chkAll.Checked += new RoutedEventHandler(checkAll_Checked);
        //                    chkAll.Unchecked += new RoutedEventHandler(checkAll_Unchecked);
        //                    break;
        //            }
        //        }
        //    }
        //}

        private void dgSearchResult_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (dgSearchResult.GetRowCount() == 0)
            {
                return;
            }

            Point pnt = e.GetPosition(null);
            C1.WPF.DataGrid.DataGridCell cell = dgSearchResult.GetCellFromPoint(pnt);

            if (cell == null || cell.Value == null)
            {
                return;
            }

            int seleted_row = dgSearchResult.CurrentRow.Index;

            string postdate = string.Empty;
            string prodid = string.Empty;
            string ERP_RSLT_TYPE_CODE = string.Empty;

            postdate = DataTableConverter.GetValue(dgSearchResult.Rows[seleted_row].DataItem, "POST_DATE").ToString();
            prodid = DataTableConverter.GetValue(dgSearchResult.Rows[seleted_row].DataItem, "PRODID").ToString();
            ERP_RSLT_TYPE_CODE = DataTableConverter.GetValue(dgSearchResult.Rows[seleted_row].DataItem, "ERP_RSLT_TYPE_CODE").ToString();

            if(dgSearchResult.CurrentColumn.Index < 1) { 
                if (DataTableConverter.GetValue(dgSearchResult.Rows[seleted_row].DataItem, "CHK").ToString() == "True")
                {
                    //������ �׸��忡 add
                    DataTable inDataTable = new DataTable();
                    DataTable dtResult = new DataTable();

                    if (ERP_RSLT_TYPE_CODE == "BOX")
                    {
                        inDataTable.Columns.Add("POST_DATE", typeof(string));
                        inDataTable.Columns.Add("PRODID", typeof(string));
                        inDataTable.Columns.Add("WOTYPE", typeof(string));
                        inDataTable.Columns.Add("ERP_ERR_TYPE_CODE", typeof(string));
                        inDataTable.Columns.Add("PRODCLASS", typeof(string));
                        //2018.06.28
                        inDataTable.Columns.Add("EQSGID", typeof(string));


                        DataRow searchCondition = inDataTable.NewRow();
                        searchCondition["POST_DATE"] = postdate;
                        searchCondition["PRODID"] = prodid;
                        searchCondition["WOTYPE"] = Util.NVC(DataTableConverter.GetValue(dgSearchResult.Rows[seleted_row].DataItem, "WOTYPE_CODE")); //Util.GetCondition(cboWoType) == "" ? null : Util.GetCondition(cboWoType);
                        searchCondition["ERP_ERR_TYPE_CODE"] = Util.GetCondition(cboStatus) == "" ? null : Util.GetCondition(cboStatus);
                        searchCondition["PRODCLASS"] = DataTableConverter.GetValue(dgSearchResult.Rows[seleted_row].DataItem, "CLASS").ToString();
                        //2018.06.28
                        searchCondition["EQSGID"] = Util.GetCondition(cboEquipmentSegment) == "" ? null : Util.GetCondition(cboEquipmentSegment);

                        inDataTable.Rows.Add(searchCondition);

                        dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_ERP_RESULT_DETAIL", "INDATA", "OUTDATA", inDataTable);
                    }
                    else
                    {
                        inDataTable.Columns.Add("POST_DATE", typeof(string));
                        inDataTable.Columns.Add("PRODID", typeof(string));
                        inDataTable.Columns.Add("WOTYPE", typeof(string));
                        inDataTable.Columns.Add("ERP_ERR_TYPE_CODE", typeof(string));
                        inDataTable.Columns.Add("PRODCLASS", typeof(string));
                        inDataTable.Columns.Add("RESULT_GUBUN", typeof(string));
                        //2018.06.28
                        inDataTable.Columns.Add("EQSGID", typeof(string));

                        DataRow searchCondition = inDataTable.NewRow();
                        searchCondition["POST_DATE"] = postdate;
                        searchCondition["PRODID"] = prodid;
                        searchCondition["WOTYPE"] = Util.NVC(DataTableConverter.GetValue(dgSearchResult.Rows[seleted_row].DataItem, "WOTYPE_CODE"));//Util.GetCondition(cboWoType) == "" ? null : Util.GetCondition(cboWoType);
                        searchCondition["ERP_ERR_TYPE_CODE"] = Util.GetCondition(cboStatus) == "" ? null : Util.GetCondition(cboStatus);
                        searchCondition["PRODCLASS"] = DataTableConverter.GetValue(dgSearchResult.Rows[seleted_row].DataItem, "CLASS").ToString();
                        searchCondition["RESULT_GUBUN"] = ERP_RSLT_TYPE_CODE;
                        //2018.06.28
                        searchCondition["EQSGID"] = Util.GetCondition(cboEquipmentSegment) == "" ? null : Util.GetCondition(cboEquipmentSegment);

                        inDataTable.Rows.Add(searchCondition);

                        dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_ERP_RESULT_DETAIL_CMA", "INDATA", "OUTDATA", inDataTable);
                    }

                    if (dtResult.Rows.Count != 0)
                    {
                        if (dgFalseList.GetRowCount() == 0)
                        {
                            Util.GridSetData(dgFalseList, dtResult, FrameOperation);
                        }
                        else
                        {
                            int TotalRow = dgFalseList.GetRowCount();

                            for (int i = 0; i < dtResult.Rows.Count; i++)
                            {
                                dgFalseList.EndNewRow(true);
                                DataGridRowAdd(dgFalseList);

                                DataTableConverter.SetValue(dgFalseList.Rows[TotalRow].DataItem, "CHK", "False");
                                DataTableConverter.SetValue(dgFalseList.Rows[TotalRow].DataItem, "POST_DATE", getDataTableValue(dtResult, i, "POST_DATE"));
                                DataTableConverter.SetValue(dgFalseList.Rows[TotalRow].DataItem, "WOID", getDataTableValue(dtResult, i, "WOID"));
                                DataTableConverter.SetValue(dgFalseList.Rows[TotalRow].DataItem, "LOTID", getDataTableValue(dtResult, i, "LOTID"));
                                DataTableConverter.SetValue(dgFalseList.Rows[TotalRow].DataItem, "GOOD_QTY", getDataTableValue(dtResult, i, "GOOD_QTY"));
                                DataTableConverter.SetValue(dgFalseList.Rows[TotalRow].DataItem, "DFCT_QTY", getDataTableValue(dtResult, i, "DFCT_QTY"));
                                DataTableConverter.SetValue(dgFalseList.Rows[TotalRow].DataItem, "TRNF_DTTM", getDataTableValue(dtResult, i, "TRNF_DTTM"));
                                DataTableConverter.SetValue(dgFalseList.Rows[TotalRow].DataItem, "ERP_ERR_TYPE_CODE", getDataTableValue(dtResult, i, "ERP_ERR_TYPE_CODE"));
                                DataTableConverter.SetValue(dgFalseList.Rows[TotalRow].DataItem, "PRODID", getDataTableValue(dtResult, i, "PRODID"));
                                DataTableConverter.SetValue(dgFalseList.Rows[TotalRow].DataItem, "PRODDESC", getDataTableValue(dtResult, i, "PRODDESC"));
                                DataTableConverter.SetValue(dgFalseList.Rows[TotalRow].DataItem, "BOXID_RESULT", getDataTableValue(dtResult, i, "BOXID_RESULT"));
                                DataTableConverter.SetValue(dgFalseList.Rows[TotalRow].DataItem, "ERP_TRNF_SEQNO", getDataTableValue(dtResult, i, "ERP_TRNF_SEQNO"));
                                DataTableConverter.SetValue(dgFalseList.Rows[TotalRow].DataItem, "ERP_RSLT_TYPE_CODE", getDataTableValue(dtResult, i, "ERP_RSLT_TYPE_CODE"));
                                DataTableConverter.SetValue(dgFalseList.Rows[TotalRow].DataItem, "BOXID_PACK", getDataTableValue(dtResult, i, "BOXID_PACK"));
                                //2019.03.25
                                DataTableConverter.SetValue(dgFalseList.Rows[TotalRow].DataItem, "LGC_LOTID", getDataTableValue(dtResult, i, "LGC_LOTID"));
                                DataTableConverter.SetValue(dgFalseList.Rows[TotalRow].DataItem, "ERP_ERR_CAUSE_CNTT", getDataTableValue(dtResult, i, "ERP_ERR_CAUSE_CNTT"));

                                TotalRow = dgFalseList.GetRowCount();
                            }
                        }
                    }
                }
                else
                {
                    int roof_cnt = dgFalseList.GetRowCount();

                    for (int i = roof_cnt - 1; 0 <= i; i--)
                    {
                        if (postdate == DataTableConverter.GetValue(dgFalseList.Rows[i].DataItem, "POST_DATE").ToString() &&
                            prodid == DataTableConverter.GetValue(dgFalseList.Rows[i].DataItem, "PRODID").ToString())
                        {
                            dgFalseList.RemoveRow(i);
                        }
                    }
                }
            }

            Util.SetTextBlockText_DataGridRowCount(tbDefectInform_cnt, Util.NVC(dgFalseList.GetRowCount()));
        }

        private void dgSearchResult_LoadedCellPresenter(object sender, C1.WPF.DataGrid.DataGridCellEventArgs e)
        {

        }

        #endregion Grid

        #region Combo
        //2019-01-24
        private void cboEquipmentSegment_SelectedItemChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            if (cboEquipmentSegment.SelectedIndex != 0)
            {
                btnWoChange.IsEnabled = true;
            }
            else
            {
                btnWoChange.IsEnabled = false;
            }
        }
        #endregion Combo

        #endregion Event

        #region Mehod  
        private void InitCombo()
        {
            CommonCombo _combo = new CMM001.Class.CommonCombo();
           
            C1ComboBox cboShop = new C1ComboBox();
            cboShop.SelectedValue = LoginInfo.CFG_SHOP_ID;
            C1ComboBox cboAREA_TYPE_CODE = new C1ComboBox();
            cboAREA_TYPE_CODE.SelectedValue = Area_Type.PACK;

            //��           
            C1ComboBox[] cboAreaParent = { cboShop };
            C1ComboBox[] cboAreaChild = { cboEquipmentSegment };
            //_combo.SetCombo(cboArea, CommonCombo.ComboStatus.ALL, cbParent: cboAreaParent, cbChild: cboAreaChild, sCase: "AREA_AREATYPE");
            _combo.SetCombo(cboArea, CommonCombo.ComboStatus.ALL, cbChild: cboAreaChild);

            //����            
            C1ComboBox[] cboEquipmentSegmentParent = { cboArea };
            C1ComboBox[] cboEquipmentSegmentChild = { cboProductModel };
            _combo.SetCombo(cboEquipmentSegment, CommonCombo.ComboStatus.ALL, cbParent: cboEquipmentSegmentParent, cbChild: cboEquipmentSegmentChild);

            //��     
            C1ComboBox[] cboProductModelParent = { cboArea, cboEquipmentSegment };
            C1ComboBox[] cboProductModelChild = { cboProduct };
            _combo.SetCombo(cboProductModel, CommonCombo.ComboStatus.ALL, cbParent: cboProductModelParent, cbChild: cboProductModelChild, sCase: "PRJ_MODEL");

            //��ǰ�з�(PACK ��ǰ �з�)           
            C1ComboBox[] cboPrdtClassParent = { cboShop, cboArea, cboEquipmentSegment, cboAREA_TYPE_CODE };
            C1ComboBox[] cboPrdtClassChild = { cboProduct };           
            _combo.SetCombo(cboPrdtClass, CommonCombo.ComboStatus.ALL, cbParent: cboPrdtClassParent, cbChild: cboPrdtClassChild);

            //��ǰ�ڵ�  
            C1ComboBox[] cboProductParent = { cboShop, cboArea, cboEquipmentSegment, cboProductModel, cboPrdtClass };
            _combo.SetCombo(cboProduct, CommonCombo.ComboStatus.ALL, cbParent: cboProductParent, sCase: "PRJ_PRODUCT");

            //WOTYPE
            //setWoType_COMBO();           
            _combo.SetCombo(cboWoType, CommonCombo.ComboStatus.ALL);

            //ó�����
            _combo.SetCombo(cboStatus, CommonCombo.ComboStatus.ALL, sCase: "CboErpStaus");

            SetcboErpResultCode();
        }

        C1.WPF.DataGrid.DataGridRowHeaderPresenter pre = new C1.WPF.DataGrid.DataGridRowHeaderPresenter()
        {
            Background = new SolidColorBrush(Colors.Transparent),
            MouseOverBrush = new SolidColorBrush(Colors.Transparent),
        };
        CheckBox chkAll = new CheckBox()
        {
            IsChecked = false,
            Background = new SolidColorBrush(Colors.Transparent),
            VerticalAlignment = System.Windows.VerticalAlignment.Center,
            HorizontalAlignment = System.Windows.HorizontalAlignment.Center
        };


        void checkAll_Checked(object sender, RoutedEventArgs e)
        {
            try
            {
                int chk_cnt = 0;
                for(int i = 0; i < dgFalseList.GetRowCount(); i++)
                {
                    if(DataTableConverter.GetValue(dgFalseList.Rows[i].DataItem, "ERP_ERR_TYPE_CODE").ToString() == "FAIL")
                    {
                        DataTableConverter.SetValue(dgFalseList.Rows[i].DataItem, "CHK", "True");

                        chk_cnt++;
                    }
                }

                if(chk_cnt == 0)
                {
                    chkAll.IsChecked = false;
                }
            }
            catch
            {
            }
        }

        void checkAll_Unchecked(object sender, RoutedEventArgs e)
        {
            for (int idx = 0; idx < dgFalseList.Rows.Count; idx++)
            {
                C1.WPF.DataGrid.DataGridRow row = dgFalseList.Rows[idx];
                DataTableConverter.SetValue(row.DataItem, "CHK", false);
            }
        }

        private void setWoType_COMBO()
        {
            DataTable dtWOTYPE = new DataTable();
         
            dtWOTYPE.Columns.Add("CBO_CODE", typeof(string));
            dtWOTYPE.Columns.Add("CBO_NAME", typeof(string));

            DataRow drWOTYPE = dtWOTYPE.NewRow();
            drWOTYPE["CBO_CODE"] = "";
            drWOTYPE["CBO_NAME"] = "ALL";
            dtWOTYPE.Rows.Add(drWOTYPE);

            DataRow drWOTYPE0 = dtWOTYPE.NewRow();
            drWOTYPE0["CBO_CODE"] = "PP02";
            drWOTYPE0["CBO_NAME"] = "���";  
            dtWOTYPE.Rows.Add(drWOTYPE0);

            DataRow drWOTYPE1 = dtWOTYPE.NewRow();
            drWOTYPE1["CBO_CODE"] = "PPRW";
            drWOTYPE1["CBO_NAME"] = "���۾�";
            dtWOTYPE.Rows.Add(drWOTYPE1);

            cboWoType.ItemsSource = DataTableConverter.Convert(dtWOTYPE);
        }

        private void search()
        {
            try
            {
                DataTable inDataTable = new DataTable();

                inDataTable.Columns.Add("FROMDATE", typeof(string));
                inDataTable.Columns.Add("TODATE", typeof(string));
                inDataTable.Columns.Add("WOTYPE", typeof(string));
                inDataTable.Columns.Add("AREAID", typeof(string));
                inDataTable.Columns.Add("EQSGID", typeof(string));
                inDataTable.Columns.Add("MODEL", typeof(string));
                inDataTable.Columns.Add("PRODCLASS", typeof(string));
                inDataTable.Columns.Add("PRODID", typeof(string));
                inDataTable.Columns.Add("ERP_ERR_TYPE_CODE", typeof(string));
                inDataTable.Columns.Add("ERP_RSLT_TYPE_CODE", typeof(string));
                //2020.05.26
                inDataTable.Columns.Add("LANGID", typeof(string));

                DataRow searchCondition = inDataTable.NewRow();
                searchCondition["FROMDATE"] = Util.GetCondition(dtpDateFrom);
                searchCondition["TODATE"] = Util.GetCondition(dtpDateTo);
                searchCondition["WOTYPE"] = Util.GetCondition(cboWoType) == "" ? null : Util.GetCondition(cboWoType);
                searchCondition["AREAID"] = Util.GetCondition(cboArea) == "" ? null : Util.GetCondition(cboArea);
                searchCondition["EQSGID"] = Util.GetCondition(cboEquipmentSegment) == "" ? null : Util.GetCondition(cboEquipmentSegment);
                searchCondition["MODEL"] = Util.GetCondition(cboProductModel) == "" ? null : Util.GetCondition(cboProductModel);
                searchCondition["PRODCLASS"] = Util.GetCondition(cboPrdtClass) == "" ? null : Util.GetCondition(cboPrdtClass);
                searchCondition["PRODID"] = Util.GetCondition(cboProduct) == "" ? null : Util.GetCondition(cboProduct);
                searchCondition["ERP_ERR_TYPE_CODE"] = Util.GetCondition(cboStatus) == "" ? null : Util.GetCondition(cboStatus);
                searchCondition["ERP_RSLT_TYPE_CODE"] = Util.GetCondition(cboErpResultCode) == "ALL" ? null : Util.GetCondition(cboErpResultCode);
                //2020.05.26
                searchCondition["LANGID"] = LoginInfo.LANGID;

                inDataTable.Rows.Add(searchCondition);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_ERP_RESULT_SUMMAY", "INDATA", "OUTDATA", inDataTable);

                dgSearchResult.ItemsSource = null;
                dgSearchResul_Sum.ItemsSource = null;

                if (dtResult.Rows.Count != 0)
                {
                    for(int i = 0; i < dtResult.Rows.Count; i++)
                    {
                        if(dtResult.Rows[i]["ERP_RSLT_TYPE_CODE"].ToString() == "LOT" && 
                            Convert.ToInt32(dtResult.Rows[i]["GOOD_QTY"]) == 0 &&
                            Convert.ToInt32(dtResult.Rows[i]["DFCT_QTY"]) > 0 )
                        {
                            dtResult.Rows[i]["ERP_RSLT_TYPE_CODE"] = "SCRAP";
                        }
                    }

                    Util.GridSetData(dgSearchResult, dtResult, FrameOperation);

                    setSummay(dtResult);
                }

                Util.SetTextBlockText_DataGridRowCount(tbResult_cnt, Util.NVC(dtResult.Rows.Count));

            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private void setSummay(DataTable dt)
        {
            int total_input = 0;
            int total_good = 0;
            int total_dfct = 0;

            for(int i = 0; i < dt.Rows.Count; i++)
            {
                //total_input += Convert.ToInt32(dt.Rows[i]["INPUT_QTY"]);
                total_good += Convert.ToInt32(dt.Rows[i]["GOOD_QTY"]);
                total_dfct += Convert.ToInt32(dt.Rows[i]["DFCT_QTY"]);
            }

            DataTable dtSUMMARY = new DataTable();

            //dtSUMMARY.Columns.Add("CHK", typeof(string));
            //dtSUMMARY.Columns.Add("POST_DATES", typeof(string));
            dtSUMMARY.Columns.Add("PRODID", typeof(string));
            dtSUMMARY.Columns.Add("CLASS", typeof(string));
            dtSUMMARY.Columns.Add("PRODABBR", typeof(string));
            //dtSUMMARY.Columns.Add("INPUT_QTY_T", typeof(string));
            dtSUMMARY.Columns.Add("GOOD_QTY", typeof(string));
            dtSUMMARY.Columns.Add("DFCT_QTY", typeof(string));

            DataRow drSUMMARY = dtSUMMARY.NewRow();
            //drSUMMARY["CHK"] = "SUMMARY";
            //drSUMMARY["POST_DATES"] = "SUMMARY"; //dtpDateFrom.SelectedDateTime.ToString("yyyyMMdd") + " ~ " + dtpDateTo.SelectedDateTime.ToString("yyyyMMdd");
            drSUMMARY["PRODID"] = "SUMMARY";
            drSUMMARY["CLASS"] = "SUMMARY";
            drSUMMARY["PRODABBR"] = "SUMMARY";
            //drSUMMARY["INPUT_QTY_T"] = total_input.ToString();
            drSUMMARY["GOOD_QTY"] = total_good.ToString();
            drSUMMARY["DFCT_QTY"] = total_dfct.ToString();
            dtSUMMARY.Rows.Add(drSUMMARY);         

            dgSearchResul_Sum.ItemsSource = DataTableConverter.Convert(dtSUMMARY);
        }

        private void setSeletedCacel()
        {
            try
            {
                if (dgSearchResult.ItemsSource != null)
                {
                    for (int i = dgSearchResult.GetRowCount(); 0 < i; i--)
                    {
                        var chkYn = DataTableConverter.GetValue(dgSearchResult.Rows[i - 1].DataItem, "CHK");

                        if (chkYn == null)
                        {
                            dgSearchResult.RemoveRow(i - 1);
                        }
                        else if (Convert.ToBoolean(chkYn))
                        {
                            dgSearchResult.EndNewRow(true);
                            dgSearchResult.RemoveRow(i - 1);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private void DataGridRowAdd(C1.WPF.DataGrid.C1DataGrid dg)
        {
            dg.BeginNewRow();
            dg.EndNewRow(true);
        }

        private string getDataTableValue(DataTable dt, int idx, string column_name)
        {
            return dt.Rows[idx][column_name].ToString();
        }

        //2019-01-24
        void popup_Closed(object sender, EventArgs e)
        {
            try
            {
                PACK001_031_WORKORDER_CHANGE popup = sender as PACK001_031_WORKORDER_CHANGE;
                if (popup.DialogResult == MessageBoxResult.OK)
                {
                    //txtReworkWOID.Text = popup.WOID;
                }
            }
            catch (Exception ex)
            {
                Util.Alert(ex.ToString());
            }
        }
        //cboErpResultCode
        private void SetcboErpResultCode()
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("CMCDTYPE", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["CMCDTYPE"] = "PACK_ERP_RESULT_TYPE";
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_COMMCODE_CBO", "RQSTDT", "RSLTDT", RQSTDT);

                cboErpResultCode.ItemsSource = DataTableConverter.Convert(dtResult);
                cboErpResultCode.SelectedIndex = 0;

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }


        #endregion Method
    }
}
