/*************************************************************************************
 Created Date : 2017.07.13
      Creator : 
   Decription : 
--------------------------------------------------------------------------------------
 [Change History]
  2017.07.13  DEVELOPER : Initial Created.
  2019.12.17  ������ : ��ȸ ���ǿ� ���� ���� ���� �߰�
  2021.07.15  ������ : [GM JV Proj.]���� ���� ���� �ڵ� �߰��� ���� ����
  2023.03.23  LEEHJ  : ����Ȱ��ȭ MES ����
  2023.11.29  ����   : Bizrule ����(DA_PRD_SEL_LOT_LIST_CALDATE -> DA_PRD_SEL_LOT_LIST_CALDATE_MB)
**************************************************************************************/

using C1.WPF;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using System;
using System.Collections.Generic;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using LGC.GMES.MES.CMM001.Extensions;

namespace LGC.GMES.MES.FCS002
{
    public partial class FCS002_317 : UserControl, IWorkArea
    {
        #region Declaration & Constructor 

        bool _bLoad = true;
        private string _areaType = string.Empty;

        private List<string> _listColumn;
        private List<string> _listColumnEach;


        private BizDataSet _Biz = new BizDataSet();
        public FCS002_317()
        {
            InitializeComponent();


        }

        public IFrameOperation FrameOperation
        {
            get;
            set;
        }

        #endregion

        #region Initialize
        private void InitCombo()
        {
            //��,����,����,���� ����
            CommonCombo _combo = new CommonCombo();

            //��
            // C20180627_23689 ��Ƽ �������� Child �ڵ� ��ȸ ���� ó��
            // C1ComboBox[] cboAreaChild = { cboEquipmentSegment };
            //_combo.SetCombo(cboArea, CommonCombo.ComboStatus.NONE, cbChild: cboAreaChild);

            _combo.SetCombo(cboArea, CommonCombo.ComboStatus.NONE);
            #region C20180627_23689 ��Ƽ �������� �������� ���Ͽ� �ּ� ó��
            //����
            //C1ComboBox[] cboEquipmentSegmentParent = { cboArea };
            //C1ComboBox[] cboEquipmentSegmentChild = { cboProcess, cboEquipment };
            //_combo.SetCombo(cboEquipmentSegment, CommonCombo.ComboStatus.SELECT, cbChild: cboEquipmentSegmentChild, cbParent: cboEquipmentSegmentParent);
            //_combo.SetCombo(cboEquipmentSegment, CommonCombo.ComboStatus.ALL, null, cbParent: cboEquipmentSegmentParent);

            //����
            //C1ComboBox[] cboProcessParent = { cboEquipmentSegment };
            //C1ComboBox[] cboProcessChild = { cboEquipment };
            //_combo.SetCombo(cboProcess, CommonCombo.ComboStatus.SELECT, cbChild: cboProcessChild, cbParent: cboProcessParent);
            //_combo.SetCombo(cboProcess, CommonCombo.ComboStatus.SELECT,null, cboProcessParent);

            //if (cboProcess.Items.Count < 1)
            //    SetProcess();

            //����
            //C1ComboBox[] cboEquipmentParent = { cboEquipmentSegment, cboProcess };
            //_combo.SetCombo(cboEquipment, CommonCombo.ComboStatus.ALL, cbParent: cboEquipmentParent);

            //cboEquipmentSegment.SelectedItemChanged += cboEquipmentSegment_SelectedItemChanged; 
            //cboProcess.SelectedItemChanged += CboProcess_SelectedItemChanged; 
            #endregion

            //// �� AutoComplete
            //GetModel();

            //���걸��
            string[] sFilter2 = { "PRODUCT_DIVISION" };
            _combo.SetCombo(cboProductDiv, CommonCombo.ComboStatus.ALL, sCase: "COMMCODE", sFilter: sFilter2);

            // ���걸�� Default �������
            if (cboProductDiv.Items.Count > 1)
                cboProductDiv.SelectedIndex = 1;

        }




        private void InitColumnsList()
        {
            _listColumn = new List<string>
            {
                "WIPQTY_ED",
                "CNFM_DFCT_QTY",
                "CNFM_LOSS_QTY",
                "CNFM_PRDT_REQ_QTY",
                "FORM_IN",
                //"INPUT_DIFF_QTY",
                "COST_DFCT_QTY",
                "COST_LOSS_QTY",
                "COST_PRDT_REQ_QTY"
            };

            _listColumnEach = new List<string>
            {
                "WIPQTY_ED_EA",
                "CNFM_DFCT_QTY_EA",
                "CNFM_LOSS_QTY_EA",
                "CNFM_PRDT_REQ_QTY_EA",
                "FORM_IN_EA",
                //"INPUT_DIFF_QTY_EA",
                "COST_DFCT_QTY_EA",
                "COST_LOSS_QTY_EA",
                "COST_PRDT_REQ_QTY_EA"
            };
        }

        #endregion

        #region Event

        #region [Form Load]
        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            //����� ���Ѻ��� ��ư �����
            List<Button> listAuth = new List<Button>();

            Util.pageAuth(listAuth, FrameOperation.AUTHORITY);
            //������� ����� ���Ѻ��� ��ư �����

            dtpDateFrom.SelectedDataTimeChanged += dtpDate_SelectedDataTimeChanged;
            dtpDateTo.SelectedDataTimeChanged += dtpDate_SelectedDataTimeChanged;



            InitCombo();
            InitColumnsList();

            GetAreaType(cboProcess.SelectedValue.GetString());
            AreaCheck(cboProcess.SelectedValue.GetString());

            this.Loaded -= UserControl_Loaded;
        }
        #endregion

        #region [��ȸ]
        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            GetLotList();
        }
        #endregion

        #region [����] - ��ȸ ����
        //private void cboEquipmentSegment_SelectedItemChanged(object sender, PropertyChangedEventArgs<object> e)
        //{
        //    if (cboEquipmentSegment.Items.Count > 0 && cboEquipmentSegment.SelectedValue != null && !cboEquipmentSegment.SelectedValue.Equals("SELECT"))
        //    {
        //        cboProcess.SelectedItemChanged -= CboProcess_SelectedItemChanged;
        //        SetProcess();
        //        cboProcess.SelectedItemChanged += CboProcess_SelectedItemChanged;
        //        SetEquipment();
        //    }
        //}
        #endregion



        #region [�۾���] - ��ȸ ����
        private void dtpDate_SelectedDataTimeChanged(object sender, SelectionChangedEventArgs e)
        {
            if (dtpDateFrom.SelectedDateTime.Year > 1 && dtpDateTo.SelectedDateTime.Year > 1)
            {
                LGCDatePicker LGCdp = sender as LGCDatePicker;

                // ��ȸ ��ư Ŭ���÷� ����
                //if ((dtpDateTo.SelectedDateTime - dtpDateFrom.SelectedDateTime).TotalDays > 31)
                //{
                //    // ��ȸ �Ⱓ �Ѵ� To ���� ���ý� From�� �ش���� 1���ڷ� ����
                //    //Util.AlertInfo("SFU2042", new object[] { "7" });   //�Ⱓ�� {0}�� �̳� �Դϴ�.
                //    Util.MessageValidation("SFU2042", "31");

                //    dtpDateFrom.SelectedDataTimeChanged -= dtpDate_SelectedDataTimeChanged;
                //    dtpDateTo.SelectedDataTimeChanged -= dtpDate_SelectedDataTimeChanged;

                //    //dtpDateFrom.SelectedDateTime = dtpDateTo.SelectedDateTime.AddDays(-6);
                //    if (LGCdp.Name.Equals("dtpDateTo"))
                //        dtpDateTo.SelectedDateTime = dtpDateFrom.SelectedDateTime.AddDays(+30);
                //    else
                //        dtpDateFrom.SelectedDateTime = dtpDateTo.SelectedDateTime.AddDays(-30);

                //    dtpDateFrom.SelectedDataTimeChanged += dtpDate_SelectedDataTimeChanged;
                //    dtpDateTo.SelectedDataTimeChanged += dtpDate_SelectedDataTimeChanged;

                //    return;
                //}

                if ((dtpDateTo.SelectedDateTime - dtpDateFrom.SelectedDateTime).TotalDays < 0)
                {
                    dtpDateFrom.SelectedDateTime = dtpDateTo.SelectedDateTime;
                    return;
                }

                //// To ���� ����� From���� 1���ڷ� ����
                //if (LGCdp.Name.Equals("dtpDateTo"))
                //{
                //    dtpDateFrom.SelectedDateTime = new DateTime(dtpDateTo.SelectedDateTime.Year, dtpDateTo.SelectedDateTime.Month, 1);
                //}

            }
        }
        #endregion

        #region [LOT] - ��ȸ ����
        private void txtWO_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                //GetLotList();
            }
        }
        #endregion

        #region [��] - ��ȸ ����
        private void txtModlId_GotFocus(object sender, RoutedEventArgs e)
        {
            // �� AutoComplete
            if (_bLoad)
            {
                GetModel();
                _bLoad = false;
            }
        }
        #endregion

        #region [�۾���� ��� ���� ����]
        private void dgProductLotChoice_Checked(object sender, RoutedEventArgs e)
        {
            dgLotList.Selection.Clear();

            RadioButton rb = sender as RadioButton;

            //���� üũ�ÿ��� ���� Ÿ���� ����
            if (DataTableConverter.GetValue(rb.DataContext, "CHK").Equals(0))
            {
                //üũ�� ó���� ����
                foreach (C1.WPF.DataGrid.DataGridRow row in ((C1.WPF.DataGrid.DataGridCellPresenter)rb.Parent).DataGrid.Rows)
                {
                    DataTableConverter.SetValue(row.DataItem, "CHK", 0);
                }

                DataTableConverter.SetValue(rb.DataContext, "CHK", 1);

                //row �� �ٲٱ�
                ((C1.WPF.DataGrid.DataGridCellPresenter)rb.Parent).DataGrid.SelectedIndex = ((C1.WPF.DataGrid.DataGridCellPresenter)rb.Parent).Cell.Row.Index;

                GetSubLot(Util.NVC(DataTableConverter.GetValue(rb.DataContext, "CALDATE")).ToString(),
                          Util.NVC(DataTableConverter.GetValue(rb.DataContext, "PROCID")).ToString(),
                          Util.NVC(DataTableConverter.GetValue(rb.DataContext, "WOID")).ToString(),
                          Util.NVC(DataTableConverter.GetValue(rb.DataContext, "PRODID")).ToString(),
                          Util.NVC(DataTableConverter.GetValue(rb.DataContext, "MODLID")).ToString(),
                          Util.NVC(DataTableConverter.GetValue(rb.DataContext, "PRJT_NAME")).ToString()
                        );
            }
        }
        #endregion

        #region [�ϼ�LOT] - dgSubLot_LoadedCellPresenter
        private void dgSubLot_LoadedCellPresenter(object sender, C1.WPF.DataGrid.DataGridCellEventArgs e)
        {
            C1.WPF.DataGrid.C1DataGrid dataGrid = sender as C1.WPF.DataGrid.C1DataGrid;

            dataGrid.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (e.Cell.Presenter == null)
                {
                    return;
                }

                //Grid Data Binding �̿��� Background �� ����
                if (Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "ROWNO")).ToString().Equals("0") &&
                    !Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "END_COUNT")).ToString().Equals("0"))
                {
                    e.Cell.Presenter.Background = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("pink"));
                }

            }));

        }

        #endregion

        private void chkPtnLen_Checked(object sender, RoutedEventArgs e)
        {
            foreach (string str in _listColumn)
            {
                if (dgLotList.Columns.Contains(str))
                    dgLotList.Columns[str].Visibility = Visibility.Collapsed;

            }

            foreach (string str in _listColumnEach)
            {
                if (dgLotList.Columns.Contains(str))
                    dgLotList.Columns[str].Visibility = Visibility.Visible;
            }
        }

        private void chkPtnLen_Unchecked(object sender, RoutedEventArgs e)
        {
            foreach (string str in _listColumnEach)
            {
                if (dgLotList.Columns.Contains(str))
                    dgLotList.Columns[str].Visibility = Visibility.Collapsed;
            }

            foreach (string str in _listColumn)
            {
                if (dgLotList.Columns.Contains(str))
                    dgLotList.Columns[str].Visibility = Visibility.Visible;
            }
        }

        /// <summary>
        /// �� ���� �̺�Ʈ ó��
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void cboArea_SelectedItemChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            try
            {
                this.Dispatcher.BeginInvoke(new System.Action(() =>
                {

                    SetCboEQSG();
                    cboEquipmentSegment.isAllUsed = true;
                    //SetProcess();

                }));
            }
            catch
            {
            }
        }

        /// <summary>
        /// ���� ���� �̺�Ʈ ó��
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void cboEquipmentSegment_SelectionChanged(object sender, EventArgs e)
        {
            try
            {
                this.Dispatcher.BeginInvoke(new System.Action(() =>
                {
                    SetProcess();
                }));
            }
            catch
            {
            }
        }

        /// <summary>
        /// ���� ���� �̺�Ʈ ó��
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CboProcess_SelectedItemChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            if (cboProcess.Items.Count > 0 && cboProcess.SelectedValue != null && !cboProcess.SelectedValue.Equals("SELECT"))
            {
                SetEquipment();

                Util.gridClear(dgLotList);
                Util.gridClear(dgSubLot);

                // ������ ������ �����(A3000)���� �϶��� ���δ�. 2017.10.25 ����ȯ
                if (cboProcess.SelectedValue.Equals(Process.ASSEMBLY))
                {
                    ((C1.WPF.DataGrid.DataGridBoundColumn)(dgLotList.Columns["INPUT_DIFF_QTY"])).Visibility = Visibility.Visible;
                }
                else
                {
                    ((C1.WPF.DataGrid.DataGridBoundColumn)(dgLotList.Columns["INPUT_DIFF_QTY"])).Visibility = Visibility.Collapsed;
                }
                GetAreaType(cboProcess.SelectedValue.ToString());
                AreaCheck(cboProcess.SelectedValue.ToString());
            }
            else
            {
                //cboEquipment.SelectedItem = "";
                //cboEquipment.SelectedValue = "";
                cboEquipment.ItemsSource = null;
            }
        }

        #endregion

        #region Mehod

        #region [BizCall]

        #region [### �۾���� ��ȸ ###]
        public void GetLotList()
        {
            try
            {
                if ((dtpDateTo.SelectedDateTime - dtpDateFrom.SelectedDateTime).TotalDays > 31)
                {
                    //Util.AlertInfo("SFU2042", new object[] { "7" });   //�Ⱓ�� {0}�� �̳� �Դϴ�.
                    Util.MessageValidation("SFU2042", "31");
                    return;
                }

                ShowLoadingIndicator();
                DoEvents();

                bool bLot = false;

                DataTable dtRqst = new DataTable();
                dtRqst.Columns.Add("LANGID", typeof(string));
                dtRqst.Columns.Add("AREAID", typeof(string));
                dtRqst.Columns.Add("EQSGID", typeof(string));
                dtRqst.Columns.Add("EQPTID", typeof(string));
                dtRqst.Columns.Add("PROCID", typeof(string));
                dtRqst.Columns.Add("FROM_DATE", typeof(string));
                dtRqst.Columns.Add("TO_DATE", typeof(string));
                dtRqst.Columns.Add("WOID", typeof(string));
                dtRqst.Columns.Add("MODLID", typeof(string));
                dtRqst.Columns.Add("PRODID", typeof(string));
                dtRqst.Columns.Add("PRJT_NAME", typeof(string));
                dtRqst.Columns.Add("RUNYN", typeof(string));
                //dtRqst.Columns.Add("NORMAL", typeof(string));
                //dtRqst.Columns.Add("PILOT", typeof(string));
                dtRqst.Columns.Add("PILOT_PROD_DIVS_CODE", typeof(string)); // 2021.07.15 : ���� ���� ���� �ڵ� �߰��� ���� ����

                DataRow dr = dtRqst.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;

                dr["AREAID"] = Util.GetCondition(cboArea, MessageDic.Instance.GetMessage("SFU1499"));
                if (dr["AREAID"].Equals("")) return;

                ////dr["EQSGID"] = Util.GetCondition(cboEquipmentSegment, "�����������ϼ���.");
                //dr["EQSGID"] = Util.GetCondition(cboEquipmentSegment, MessageDic.Instance.GetMessage("SFU1223"));
                //if (dr["EQSGID"].Equals("")) return;

                //string sEquipmentSegment = Util.GetCondition(cboEquipmentSegment);
                string sEquipmentSegment = Util.ConvertEmptyToNull(cboEquipmentSegment.SelectedItemsToString);
                dr["EQSGID"] = string.IsNullOrWhiteSpace(sEquipmentSegment) ? null : sEquipmentSegment;

                //dr["PROCID"] = Util.GetCondition(cboProcess, "�����������ϼ���.");
                dr["PROCID"] = Util.GetCondition(cboProcess, MessageDic.Instance.GetMessage("SFU1459"));
                if (dr["PROCID"].Equals("")) return;

                //dr["EQPTID"] = Util.GetCondition(cboEquipment, bAllNull: true);
                //string sEqptID = Util.GetCondition(cboEquipment);
                string sEqptID = Util.ConvertEmptyToNull(cboEquipment.SelectedItemsToString);
                dr["EQPTID"] = string.IsNullOrWhiteSpace(sEqptID) ? null : sEqptID;

                dr["FROM_DATE"] = Util.GetCondition(dtpDateFrom);
                dr["TO_DATE"] = Util.GetCondition(dtpDateTo);

                if (!string.IsNullOrWhiteSpace(txtWO.Text))
                    dr["WOID"] = txtWO.Text;

                if (!string.IsNullOrWhiteSpace(txtModlId.Text))
                    dr["MODLID"] = txtModlId.Text;

                if (!string.IsNullOrWhiteSpace(txtProdId.Text))
                    dr["PRODID"] = txtProdId.Text;

                if (!string.IsNullOrWhiteSpace(txtPrjtName.Text))
                    dr["PRJT_NAME"] = txtPrjtName.Text;

                if (chkProc.IsChecked == false)
                    dr["RUNYN"] = "Y";

                // 2021.07.15 : ���� ���� ���� �ڵ� �߰��� ���� ����
                //if (cboProductDiv.SelectedValue.ToString() == "P")
                //    dr["NORMAL"] = cboProductDiv.SelectedValue.ToString();
                //else if (cboProductDiv.SelectedValue.ToString() == "X")
                //    dr["PILOT"] = cboProductDiv.SelectedValue.ToString();
                dr["PILOT_PROD_DIVS_CODE"] = Util.GetCondition(cboProductDiv, bAllNull: true);

                dtRqst.Rows.Add(dr);

                Util.gridClear(dgSubLot);

                // 2023.11.29 Bizrule ����
                //DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_LOT_LIST_CALDATE", "INDATA", "OUTDATA", dtRqst);
                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_LOT_LIST_CALDATE_MB", "INDATA", "OUTDATA", dtRqst);

                Util.GridSetData(dgLotList, dtRslt, FrameOperation, true);

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

        #endregion

        #region [### �ϼ�LOT ��ȸ ###]
        private void GetSubLot(string sCaldate, string sProcid, string sWoid, string sProdid, string sModlid, string sPrjtname)
        {
            try
            {
                ShowLoadingIndicator();
                DoEvents();

                DataTable dtRqst = new DataTable();
                dtRqst.Columns.Add("LANGID", typeof(string));
                dtRqst.Columns.Add("AREAID", typeof(string));
                dtRqst.Columns.Add("EQSGID", typeof(string));
                dtRqst.Columns.Add("EQPTID", typeof(string));
                dtRqst.Columns.Add("PROCID", typeof(string));
                dtRqst.Columns.Add("WOID", typeof(string));
                dtRqst.Columns.Add("CALDATE", typeof(string));
                dtRqst.Columns.Add("MODLID", typeof(string));
                dtRqst.Columns.Add("PRODID", typeof(string));
                dtRqst.Columns.Add("PRJT_NAME", typeof(string));
                dtRqst.Columns.Add("RUNYN", typeof(string));

                DataRow dr = dtRqst.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;

                dr["AREAID"] = Util.GetCondition(cboArea, MessageDic.Instance.GetMessage("SFU1499"));
                if (dr["AREAID"].Equals("")) return;

                
                string eqsgID = Util.NVC(DataTableConverter.GetValue(dgLotList.Rows[new Util().GetDataGridCheckFirstRowIndex(dgLotList, "CHK")].DataItem, "EQSGID"));
                string eqptID = Util.NVC(DataTableConverter.GetValue(dgLotList.Rows[new Util().GetDataGridCheckFirstRowIndex(dgLotList, "CHK")].DataItem, "EQPTID"));

                string sEquipmentSegment = cboEquipmentSegment.SelectedItemsToString;
                string sEqptID = cboEquipment.SelectedItemsToString;

                dr["EQSGID"] = string.IsNullOrWhiteSpace(eqsgID) ? sEquipmentSegment : eqsgID;
                dr["EQPTID"] = string.IsNullOrWhiteSpace(eqptID) ? sEqptID : eqptID;

                if (!string.IsNullOrWhiteSpace(sWoid))
                    dr["WOID"] = sWoid;

                dr["PROCID"] = sProcid;
                dr["CALDATE"] = sCaldate;
                dr["MODLID"] = sModlid;
                dr["PRODID"] = sProdid;

                if (!string.IsNullOrWhiteSpace(sPrjtname))
                    dr["PRJT_NAME"] = sPrjtname;

                if (chkProc.IsChecked == false)
                    dr["RUNYN"] = "Y";

                dtRqst.Rows.Add(dr);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_LOT_LIST_ASSY_DETAIL", "INDATA", "OUTDATA", dtRqst);

                Util.GridSetData(dgSubLot, dtRslt, FrameOperation, true);
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
        #endregion

        #region[### ��ȸ ���� �� ��ȸ ###]
        private void GetModel()
        {
            try
            {
                ShowLoadingIndicator();
                DoEvents();

                DataTable inTable = new DataTable();
                inTable.Columns.Add("PRODID", typeof(string));

                DataRow newRow = inTable.NewRow();

                inTable.Rows.Add(newRow);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_MODEL", "INDATA", "OUTDATA", inTable);

                foreach (DataRow r in dtRslt.Rows)
                {
                    string displayString = r["MODLID"].ToString(); //ǥ�� �ؽ�Ʈ
                    //string[] keywordString = new string[r["MODLID"].ToString().Length - 1]; //�˻� �ʿ� �ּ� ���ڼ�(Threshold)�� 2�̹Ƿ� �� ���ھ� ��� �迭(�������� �� �� �����̰� '����'�� '����'�� 2���� �������� ���� �� �����Ƿ� �迭�� Count�� 2�� �ȴ�)�� ������ �˻� ����.
                    string keywordString;


                    //for (int i = 0; i < displayString.Length - 1; i++)
                    //keywordString[i] = displayString.Substring(i, txtModlId.Threshold); //Threshold ��ŭ �߶� �迭�� ��´� (�� �ּ� ����)

                    keywordString = displayString;

                    txtModlId.AddItem(new CMM001.AutoCompleteEntry(displayString, keywordString)); //ǥ�� �ؽ�Ʈ�� �˻��� �ؽ�Ʈ(�迭)�� AutoCompleteTextBox�� Item�� �߰��Ѵ�.
                }
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
        #endregion

        #region [Combo ���� ��������]

        private void SetCboEQSG()
        {
            try
            {
                this.cboArea.SelectedItemChanged -= this.cboArea_SelectedItemChanged;
                string sSelectedValue = cboEquipmentSegment.SelectedItemsToString;
                string[] sSelectedList = sSelectedValue.Split(',');

                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("AREAID", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["AREAID"] = cboArea.SelectedValue.ToString();
                RQSTDT.Rows.Add(dr);
                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_EQUIPMENTSEGMENT_MULTI_CBO", "RQSTDT", "RSLTDT", RQSTDT);
                                                                           
                cboEquipmentSegment.ItemsSource = DataTableConverter.Convert(dtResult);

                for (int i = 0; i < dtResult.Rows.Count; i++)
                {
                    if (sSelectedList.Length > 0 && sSelectedList[0] != "")
                    {
                        for (int j = 0; j < sSelectedList.Length; j++)
                        {
                            if (Util.NVC(dtResult.Rows[i]["CBO_CODE"]) == sSelectedList[j])
                            {
                                cboEquipmentSegment.Check(i);
                                break;
                            }
                        }
                    }
                    else if (Util.NVC(dtResult.Rows[i]["CBO_CODE"]) == LoginInfo.CFG_EQSG_ID)
                    {
                        cboEquipmentSegment.Check(i);
                        break;
                    }
                }
                this.cboArea.SelectedItemChanged += this.cboArea_SelectedItemChanged;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void SetProcess()
        {
            try
            {
                // ���� �����ϼ���.
                string sArea = Util.GetCondition(cboArea);
                if (string.IsNullOrWhiteSpace(sArea))
                    return;

                //string sEquipmentSegment = Util.GetCondition(cboEquipmentSegment);
                string sEquipmentSegment = cboEquipmentSegment.SelectedItemsToString;

                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("AREAID", typeof(string));
                RQSTDT.Columns.Add("EQSGID", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["AREAID"] = sArea;
                dr["EQSGID"] = string.IsNullOrWhiteSpace(sEquipmentSegment) ? null : sEquipmentSegment;
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_PROCESS_MULTI_CBO", "RQSTDT", "RSLTDT", RQSTDT);

                cboProcess.DisplayMemberPath = "CBO_NAME";
                cboProcess.SelectedValuePath = "CBO_CODE";

                //DataRow drIns = dtResult.NewRow();
                //drIns["CBO_NAME"] = "-SELECT-";
                //drIns["CBO_CODE"] = "SELECT";
                //dtResult.Rows.InsertAt(drIns, 0);

                cboProcess.ItemsSource = dtResult.Copy().AsDataView();

                if (!LoginInfo.CFG_PROC_ID.Equals(""))
                {
                    cboProcess.SelectedValue = LoginInfo.CFG_PROC_ID;

                    if (cboProcess.SelectedIndex < 0)
                        cboProcess.SelectedIndex = 0;
                }
                else
                {
                    if (cboProcess.Items.Count > 0)
                        cboProcess.SelectedIndex = 0;
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        #endregion

        #region [���� ���� ��������]
        private void SetEquipment()
        {
            try
            {
                // ���� �����ϼ���.
                string sArea = Util.GetCondition(cboArea);
                if (string.IsNullOrWhiteSpace(sArea))
                    return;

                string sProc = Util.GetCondition(cboProcess);
                if (string.IsNullOrWhiteSpace(sProc))
                {
                    cboEquipment.ItemsSource = null;
                    return;
                }

                //string sEquipmentSegment = Util.GetCondition(cboEquipmentSegment);
                string sEquipmentSegment = cboEquipmentSegment.SelectedItemsToString;


                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("AREAID", typeof(string));
                RQSTDT.Columns.Add("EQSGID", typeof(string));
                RQSTDT.Columns.Add("PROCID", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["AREAID"] = sArea;
                dr["EQSGID"] = string.IsNullOrWhiteSpace(sEquipmentSegment) ? null : sEquipmentSegment;
                dr["PROCID"] = cboProcess.SelectedValue.ToString();
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_EQUIPMENT_MULT_CBO_MB", "RQSTDT", "RSLTDT", RQSTDT);

                /* C20180627_23689 ���� �� �ҽ�
                cboEquipment.DisplayMemberPath = "CBO_NAME";
                cboEquipment.SelectedValuePath = "CBO_CODE";

                DataRow drIns = dtResult.NewRow();
                drIns["CBO_NAME"] = "-ALL-";
                drIns["CBO_CODE"] = "";
                dtResult.Rows.InsertAt(drIns, 0);

                cboEquipment.ItemsSource = dtResult.Copy().AsDataView();

                if (!LoginInfo.CFG_EQPT_ID.Equals(""))
                {
                    cboEquipment.SelectedValue = LoginInfo.CFG_EQPT_ID;

                    if (cboEquipment.SelectedIndex < 0)
                        cboEquipment.SelectedIndex = 0;
                }
                else
                {
                    cboEquipment.SelectedIndex = 0;
                }
                */
                cboEquipment.ItemsSource = DataTableConverter.Convert(dtResult);

                string sSelectedValue = cboEquipment.SelectedItemsToString;
                string[] sSelectedList = sSelectedValue.Split(',');

                for (int i = 0; i < dtResult.Rows.Count; i++)
                {
                    if (sSelectedList.Length > 0 && sSelectedList[0] != "")
                    {
                        for (int j = 0; j < sSelectedList.Length; j++)
                        {
                            if (Util.NVC(dtResult.Rows[i]["CBO_CODE"]) == sSelectedList[j])
                            {
                                cboEquipment.Check(i);
                                break;
                            }
                        }
                    }
                    else if (Util.NVC(dtResult.Rows[i]["CBO_CODE"]) == LoginInfo.CFG_EQPT_ID)
                    {
                        cboEquipment.Check(i);
                        break;
                    }

                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        #endregion


        private void GetAreaType(string processCode)
        {
            try
            {
                _areaType = string.Empty;

                DataTable dtRqst = new DataTable();
                dtRqst.Columns.Add("PCSGID", typeof(string));
                dtRqst.Columns.Add("PROCID", typeof(string));

                DataRow dr = dtRqst.NewRow();
                dr["PROCID"] = processCode;
                dtRqst.Rows.Add(dr);
                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_PROCESSSEGMENTPROCESS", "INDATA", "OUTDATA", dtRqst);

                if (CommonVerify.HasTableRow(dtRslt))
                    _areaType = dtRslt.Rows[0]["PCSGID"].ToString();
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }


        private void AreaCheck(string processCode)
        {
            if (string.IsNullOrWhiteSpace(processCode) || processCode.Equals("SELECT"))
                return;

            chkPtnLen.IsChecked = false;
            chkPtnLen.IsEnabled = !_areaType.Equals("A");


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

        private void DoEvents()
        {
            Application.Current.Dispatcher.Invoke(DispatcherPriority.Background, new System.Threading.ThreadStart(delegate { }));
        }
        #endregion [Func]

        #endregion

    }
}