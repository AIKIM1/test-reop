/*************************************************************************************
 Created Date : 2016.06.16
      Creator : 
   Decription : 
--------------------------------------------------------------------------------------
 [Change History]
  2016.06.16  DEVELOPER : Initial Created.
  2018.12.12 �տ켮 CSR ID 3869450 GMES Pack HOLD ��� �߰� ��û  [��û��ȣ]C20181212_69450
  2019.01.16 �赵�� CSR ID 3890457 GMES_Lot Ȧ��/������ ȭ�� ���� ��û �� [��û��ȣ]C20190109_90457
  2019.08.19 �տ켮 CSR ID 4032058 GMES ��ɰ��� ��û�� ��  [��û��ȣ]C20190702_32058 [���񽺹�ȣ] 4032058
**************************************************************************************/

using System;
using System.Configuration;
using System.Data;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using C1.WPF;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using Microsoft.Win32;
//2018.12.12
using System.Linq;
using C1.WPF.DataGrid;
using C1.WPF.DataGrid.Summaries;
using System.Windows.Media.Animation;
using System.Windows.Media;
//2019.08.19
using LGC.GMES.MES.CMM001.Popup;
using System.Collections;

namespace LGC.GMES.MES.PACK001
{
    public partial class PACK001_021 : UserControl, IWorkArea
    {
        #region Declaration & Constructor 
        LGC.GMES.MES.PACK001.Class.MESSAGE_PARAM ms = new LGC.GMES.MES.PACK001.Class.MESSAGE_PARAM();
        DataTable dtSearchResult;
        DataTable dtFindResult;
        CommonCombo _combo = new CommonCombo();
        ExcelMng exl = new ExcelMng();
       
        string lot_holding = string.Empty;
        string lot_unholding = string.Empty;
        string cell_info = string.Empty;
        string temp = string.Empty;

        public IFrameOperation FrameOperation
        {
            get;
            set;
        }

        public PACK001_021()
        {
            InitializeComponent();

            this.Loaded += PACK001_021_Loaded;
        }

        //2018.12.12
        C1.WPF.DataGrid.DataGridRowHeaderPresenter pre = new C1.WPF.DataGrid.DataGridRowHeaderPresenter();
        //2019.01.16
        C1.WPF.DataGrid.DataGridRowHeaderPresenter pre1 = new C1.WPF.DataGrid.DataGridRowHeaderPresenter();

        CheckBox chkAll = new CheckBox()
        {
            IsChecked = false,
            Background = new SolidColorBrush(Colors.Transparent),
            VerticalAlignment = System.Windows.VerticalAlignment.Center,
            HorizontalAlignment = System.Windows.HorizontalAlignment.Center
        };

        //2019.01.16
        CheckBox chkAll1 = new CheckBox()
        {
            IsChecked = false,
            Background = new SolidColorBrush(Colors.Transparent),
            VerticalAlignment = System.Windows.VerticalAlignment.Center,
            HorizontalAlignment = System.Windows.HorizontalAlignment.Center
        };

        #endregion

        #region Initialize
        private void Initialize()
        {
            try
            {
                dtpDateFrom.SelectedDateTime = (DateTime)DateTime.Now.AddDays(-7);
                dtpDateTo.SelectedDateTime = (DateTime)DateTime.Now;

                //2019.08.19
                dtpDate.SelectedDateTime = (DateTime)DateTime.Now;
                dtpCalDate.SelectedDateTime = (DateTime)DateTime.Now;

                tbCellInput_cnt.Text = "[ 0 " + ObjectDic.Instance.GetObjectName("��") + " ]";
                tbSearch_cnt.Text = "[ 0 " + ObjectDic.Instance.GetObjectName("��") + " ]";

                InitCombo();

                getSearch();
            }
            catch(Exception ex)
            {
                Util.AlertInfo(ex.Message);
            }            
        }
        #endregion Initialize

        #region Event

        #region Load
        private void PACK001_021_Loaded(object sender, RoutedEventArgs e)
        {
            Initialize();

            this.Loaded -= PACK001_021_Loaded;
        }
        #endregion Load

        #region Button
        private void btnExcelDown_Click(object sender, RoutedEventArgs e)
        {

        }

        private void btnExcelLoad_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                getReturnTagetCell_By_Excel();
            }
            catch (Exception ex)
            {
                Util.AlertInfo(ex.Message);
            }
        }

        private void btnHold_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (dgCellId.ItemsSource == null || dgCellId.Rows.Count == 0)
                {
                    return;
                }

                //Ȧ�� ���� �ʼ� : 
                if (cboHoldReason.SelectedIndex < 1)
                {                    
                    ms.AlertWarning("SFU1342"); //HOLD ������ ���� �ϼ���.
                    return;
                }

                //reason = cboHoldReason.Text; //������

                TextRange textRange = new TextRange(rtbHoldCompare.Document.ContentStart, rtbHoldCompare.Document.ContentEnd);

                if (textRange.Text.Equals("\r\n") || textRange.Text.Equals(""))
                {                    
                    ms.AlertWarning("SFU1341"); //HOLD ��� �Է��ϼ���
                    return;
                }

                setHold(textRange.Text);
                
                ms.AlertInfo("SFU1344"); //HOLD �Ϸ�
                tbLotInfo.Text = "";
                rtbHoldCompare.Document.Blocks.Clear();
                getSearch();
              
                dgCellId.ItemsSource = null;

                tbCellInput_cnt.Text = "[ 0 " + ObjectDic.Instance.GetObjectName("��") + " ]";
            }
            catch (Exception ex)
            {
                Util.AlertInfo(ex.Message);
            }
        }

        private void btnUnHold_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string cell_id = "";
                string UnHoldreason;
                bool chk_vali = false;

                if (dgCellHistory.ItemsSource == null || dgCellHistory.GetRowCount() == 0)
                {
                    return;
                }

                //���� ���� �ʼ�
                if (cboUnHoldReason.SelectedIndex < 1)
                {                   
                    //LGC.GMES.MES.ControlsLibrary.MessageBox.Show("���� ������ �����ϼ���");
                    ms.AlertWarning("SFU3333"); //���ÿ��� : Ȧ����������(�ʼ�����) �޺��� �����ϼ���

                    return;
                }

                if (txtNote.Text == "")
                {
                    ms.AlertWarning("SFU1404"); //NOTE�� �Է� �ϼ���
                    return;
                }

                //���� �۾� ����
                for (int i = 0; i < dgCellHistory.Rows.Count; i++)
                {
                    var chkYn = DataTableConverter.GetValue(dgCellHistory.Rows[i].DataItem, "CHK");

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
                ms.AlertInfo("SFU1344"); //UNHOLD �Ϸ�
                getSearch();
                txtNote.Text = "";
            }
            catch (Exception ex)
            {
                Util.AlertInfo(ex.Message);
            }
        }

        private void btnSelectCancel_Click(object sender, RoutedEventArgs e)
        {
            if (dgCellId.ItemsSource != null)
            {
                for (int i = dgCellId.GetRowCount(); 0 < i; i--)
                {
                    var chkYn = DataTableConverter.GetValue(dgCellId.Rows[i - 1].DataItem, "CHK");
                    var lot_id = DataTableConverter.GetValue(dgCellId.Rows[i - 1].DataItem, "LOTID");

                    if (chkYn == null)
                    {
                        dgCellId.RemoveRow(i - 1);
                    }
                    else if (Convert.ToBoolean(chkYn))
                    {
                        dgCellId.EndNewRow(true);
                        dgCellId.RemoveRow(i - 1);
                    }

                    tbLotInfo.Text = "";
                }

                DataTable dt = DataTableConverter.Convert(dgCellId.ItemsSource);

                Util.SetTextBlockText_DataGridRowCount(tbCellInput_cnt, Util.NVC(dt.Rows.Count));
            }
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            if(dgCellId.GetRowCount() == 0)
            {
                return;
            }

            LGC.GMES.MES.ControlsLibrary.MessageBox.Show(ms.AlertRetun("SFU3407"), null, "CAUSE", MessageBoxButton.OKCancel, MessageBoxIcon.Warning, (caution_result) =>
            //���� HOLD����Ʈ�� ���� �Ͻðڽ��ϱ�?
            {
                if (caution_result == MessageBoxResult.OK)
                {
                    dgCellId.ItemsSource = null;
                    tbLotInfo.Text = "";

                    tbCellInput_cnt.Text = "[ 0 " + ObjectDic.Instance.GetObjectName("��") + " ]";
                }
            }
            );
        }

        private void btnExcel_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                new ExcelExporter().Export(dgCellHistory);
            }
            catch (Exception ex)
            {
                Util.AlertInfo(ex.Message);
                //LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }

        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                getSearch();
            }
            catch (Exception ex)
            {
                Util.AlertInfo(ex.Message);
            }
        }

        //2019.08.19
        private void btnPerson_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(txtPerson.Text))
                {
                    // ����ڸ� �����ϼ���.
                    Util.MessageValidation("SFU4983");
                    return;
                }

                GetUserWindow();
            }
            catch (Exception ex)
            {
                Util.Alert(ex.ToString());
            }
        }
        #endregion Button

        #region Text
        private void txtLotId_KeyDown(object sender, KeyEventArgs e)
        {
            try
            {
                if (e.Key == Key.Enter)
                {
                    if (txtLotId.Text != "")
                    {
                        tbLotInfo.Visibility = Visibility.Visible;

                        //cell id validation//
                        //1. wip�̷¿��� ��ȸ : ����Ȯ��, ��Ȯ��
                        if (!getCell_check(txtLotId.Text)) 
                        {
                            txtLotId.Text = "";
                            txtLotId.Focus();
                            return;
                        }
                                              
                        int TotalRow = dgCellId.GetRowCount();
                       
                        //2. cell list�� �����ϴ��� Ȯ��
                        for (int i = 0; i < TotalRow; i++)
                        {
                            string grid_id = DataTableConverter.GetValue(dgCellId.Rows[i].DataItem, "LOTID").ToString();

                            if (txtLotId.Text == grid_id)
                            {
                                //Util.AlertInfo("�̹� ����Ʈ�� ��ϵ� CELL ID �Դϴ�.");
                                ms.AlertWarning("SFU3334"); //�Է¿��� : �̹� Ȧ�� ��� ����Ʈ�� ��ϵ� LOTID�Դϴ�.

                                txtLotId.Text = "";
                                txtLotId.Focus();
                                return;
                            }
                        }

                        cellGridAdd(TotalRow); //�׸��忡 �߰�

                        DataTable dt = DataTableConverter.Convert(dgCellId.ItemsSource);

                        Util.SetTextBlockText_DataGridRowCount(tbCellInput_cnt, Util.NVC(dt.Rows.Count));

                        txtLotId.Text = "";
                    }
                }
            }
            catch (Exception ex)
            {
                Util.AlertInfo(ex.Message);
            }

        }

        //2019.08.19
        private void txtPerson_KeyDown(object sender, KeyEventArgs e)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(txtPerson.Text))
                {
                    // ����ڸ� �����ϼ���.
                    Util.MessageValidation("SFU4983");
                    return;
                }

                GetUserWindow();
            }
            catch (Exception ex)
            {
                Util.Alert(ex.ToString());
            }
        }

        #endregion Text

        #region Grid
        private void dgCellId_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {

        }

        private void dgCellHistory_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            try
            {
                if (dgCellHistory.Rows.Count == 0 || dgCellHistory == null)
                {
                    return;
                }

                Point pnt = e.GetPosition(null);
                C1.WPF.DataGrid.DataGridCell cell = dgCellHistory.GetCellFromPoint(pnt);

                if (cell == null || cell.Value == null)
                {
                    return;
                }

                int currentRow = cell.Row.Index;
                int _col = cell.Column.Index;
                string value = cell.Value.ToString();

                if (dgCellHistory.Rows[currentRow].DataItem == null)
                {
                    return;
                }
             
                DataTable dtCellHistory = DataTableConverter.Convert(dgCellHistory.ItemsSource);

                lot_unholding = DataTableConverter.GetValue(dgCellHistory.Rows[currentRow].DataItem, "LOTID").ToString();
            }
            catch (Exception ex)
            {
                Util.AlertInfo(ex.Message);
            }
        }

        //2018.12.12
        private void dgCellId_BeganEdit(object sender, DataGridBeganEditEventArgs e)
        {
            dgCellId.Dispatcher.BeginInvoke(new Action(() =>
            {
                try
                {
                    if (e.Column is C1.WPF.DataGrid.DataGridCheckBoxColumn)
                    {
                        C1.WPF.DataGrid.DataGridCell cell = (sender as C1DataGrid).GetCell(e.Row.Index, e.Column.Index);
                        CheckBox cb = cell.Presenter.Content as CheckBox;

                        int index = ((C1.WPF.DataGrid.DataGridCellPresenter)cb.Parent).Row.Index;
                    }
                }
                catch (Exception ex)
                {
                    Util.MessageException(ex);
                }

            }));
        }

        private void dgCellId_LoadedColumnHeaderPresenter(object sender, C1.WPF.DataGrid.DataGridColumnEventArgs e)
        {
            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (string.IsNullOrEmpty(e.Column.Name) == false)
                {
                    if (e.Column.Name.Equals("CHK"))
                    {
                        pre.Content = chkAll;
                        //pre.Content = new HorizontalContentAlignment = "Center";
                        e.Column.HeaderPresenter.Content = pre;

                        chkAll.Checked -= new RoutedEventHandler(checkAll_Checked);
                        chkAll.Unchecked -= new RoutedEventHandler(checkAll_Unchecked);
                        chkAll.Checked += new RoutedEventHandler(checkAll_Checked);
                        chkAll.Unchecked += new RoutedEventHandler(checkAll_Unchecked);
                    }
                }
            }));
        }

        //2019.01.16
        private void dgCellHistory_BeganEdit(object sender, DataGridBeganEditEventArgs e)
        {
            dgCellHistory.Dispatcher.BeginInvoke(new Action(() =>
            {
                try
                {
                    if (e.Column is C1.WPF.DataGrid.DataGridCheckBoxColumn)
                    {
                        C1.WPF.DataGrid.DataGridCell cell = (sender as C1DataGrid).GetCell(e.Row.Index, e.Column.Index);
                        CheckBox cb = cell.Presenter.Content as CheckBox;

                        int index = ((C1.WPF.DataGrid.DataGridCellPresenter)cb.Parent).Row.Index;
                    }
                }
                catch (Exception ex)
                {
                    Util.MessageException(ex);
                }

            }));
        }

        //2019.01.16
        private void dgCellHistory_LoadedColumnHeaderPresenter(object sender, C1.WPF.DataGrid.DataGridColumnEventArgs e)
        {
            dgCellHistory.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (string.IsNullOrEmpty(e.Column.Name) == false)
                {
                    if (e.Column.Name.Equals("CHK"))
                    {
                        pre1.Content = chkAll1;
                        //pre.Content = new HorizontalContentAlignment = "Center";
                        e.Column.HeaderPresenter.Content = pre1;

                        chkAll1.Checked -= new RoutedEventHandler(checkAllCellHistory_Checked);
                        chkAll1.Unchecked -= new RoutedEventHandler(checkAllCellHistory_Unchecked);
                        chkAll1.Checked += new RoutedEventHandler(checkAllCellHistory_Checked);
                        chkAll1.Unchecked += new RoutedEventHandler(checkAllCellHistory_Unchecked);
                    }
                }
            }));
        }

        #endregion Grid

        #region Check
        //2018.12.12
        void checkAll_Checked(object sender, RoutedEventArgs e)
        {
            DataTable dt = DataTableConverter.Convert(dgCellId.ItemsSource);
            foreach (DataRow dr in dt.Rows)
            {
                dr["CHK"] = true;
            }
            dgCellId.ItemsSource = DataTableConverter.Convert(dt);
        }

        //2018.12.12
        void checkAll_Unchecked(object sender, RoutedEventArgs e)
        {
            DataTable dt = DataTableConverter.Convert(dgCellId.ItemsSource);
            foreach (DataRow dr in dt.Rows)
            {
                dr["CHK"] = false;
            }
            dgCellId.ItemsSource = DataTableConverter.Convert(dt);
        }

        //2019.01.16
        void checkAllCellHistory_Checked(object sender, RoutedEventArgs e)
        {
            DataTable dt = DataTableConverter.Convert(dgCellHistory.ItemsSource);
            foreach (DataRow dr in dt.Rows)
            {
                dr["CHK"] = true;
            }
            dgCellHistory.ItemsSource = DataTableConverter.Convert(dt);
        }

        //2019.01.16
        void checkAllCellHistory_Unchecked(object sender, RoutedEventArgs e)
        {
            DataTable dt = DataTableConverter.Convert(dgCellHistory.ItemsSource);
            foreach (DataRow dr in dt.Rows)
            {
                dr["CHK"] = false;
            }
            dgCellHistory.ItemsSource = DataTableConverter.Convert(dt);
        }

        #endregion Check

        #endregion Event

        #region Mehod

        private void InitCombo()
        {
            CommonCombo _combo = new CommonCombo();

            C1ComboBox cboSHOPID = new C1ComboBox();
            cboSHOPID.SelectedValue = LoginInfo.CFG_SHOP_ID;
            C1ComboBox cboAREA_TYPE_CODE = new C1ComboBox();
            cboAREA_TYPE_CODE.SelectedValue = Area_Type.PACK;           
            //C1ComboBox cboPrdtClass = new C1ComboBox();
            //cboPrdtClass.SelectedValue = "CELL";
            C1ComboBox cboArea = new C1ComboBox();
            cboArea.SelectedValue = LoginInfo.CFG_AREA_ID;

            //����            
            C1ComboBox[] cboEquipmentSegmentParent = { cboArea };
            C1ComboBox[] cboEquipmentSegmentChild = { cboProductModel, cboProduct };
            _combo.SetCombo(cboEquipmentSegment, CommonCombo.ComboStatus.ALL, cbParent: cboEquipmentSegmentParent, cbChild: cboEquipmentSegmentChild);

            //��          
            C1ComboBox[] cboProductModelParent = { cboArea, cboEquipmentSegment };
            C1ComboBox[] cboProductModelChild = { cboProduct };
            _combo.SetCombo(cboProductModel, CommonCombo.ComboStatus.ALL, cbParent: cboProductModelParent, cbChild: cboProductModelChild, sCase: "PRJ_MODEL");

            //��ǰ�з�(PACK ��ǰ �з�)           
            C1ComboBox[] cboPrdtClassParent = { cboSHOPID, cboArea, cboEquipmentSegment, cboAREA_TYPE_CODE };
            C1ComboBox[] cboPrdtClassChild = { cboProduct };
            //C1ComboBox[] cboPrdtClassParent = { cboSHOPID, cboAreaByAreaType, cboEquipmentSegment, cboAREA_TYPE_CODE };
            _combo.SetCombo(cboPrdtClass, CommonCombo.ComboStatus.ALL, cbParent: cboPrdtClassParent, cbChild: cboPrdtClassChild);

            //��ǰ�ڵ�  
            C1ComboBox[] cboProductParent = { cboSHOPID, cboArea, cboEquipmentSegment, cboProductModel, cboPrdtClass };
            _combo.SetCombo(cboProduct, CommonCombo.ComboStatus.ALL, cbParent: cboProductParent, sCase: "PRJ_PRODUCT");           

            //HOLD ����
            string[] sFilter = { "HOLD_LOT" };
            _combo.SetCombo(cboHoldReason, CommonCombo.ComboStatus.SELECT, sCase: "ACTIVITIREASON", sFilter: sFilter);

            //UNHOLD(����)���� 
            string[] sFilter1 = { "UNHOLD_LOT" };
            _combo.SetCombo(cboUnHoldReason, CommonCombo.ComboStatus.SELECT, sCase: "ACTIVITIREASON", sFilter: sFilter1);            
        }
     
        private void DataGridRowAdd(C1.WPF.DataGrid.C1DataGrid dg)
        {
            dg.BeginNewRow();
            dg.EndNewRow(true);
        }

        private void cellGridAdd(int TotalRow)
        {
            try
            {
                if (TotalRow == 0)
                {
                    dgCellId.ItemsSource = DataTableConverter.Convert(dtFindResult);
                    return;
                }
                //cell list�� �߰�// 
                DataGridRowAdd(dgCellId);

                DataRow dr = dtFindResult.Rows[0];

                DataTableConverter.SetValue(dgCellId.Rows[TotalRow].DataItem, "CHK", "false");
                DataTableConverter.SetValue(dgCellId.Rows[TotalRow].DataItem, "LOTID", dr["LOTID"]);
                DataTableConverter.SetValue(dgCellId.Rows[TotalRow].DataItem, "PROCNAME", dr["PROCNAME"]);
                DataTableConverter.SetValue(dgCellId.Rows[TotalRow].DataItem, "WIPSNAME", dr["WIPSNAME"]);
                DataTableConverter.SetValue(dgCellId.Rows[TotalRow].DataItem, "BOXID", dr["BOXID"]);
            }
            catch (Exception ex)
            {
                throw ex;
            }           
        }

        private void getCell(string actid)
        {
            try
            {
                //��ȸ
                DataTable RQSTDT = new DataTable();

                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("PRODID", typeof(string));
                //RQSTDT.Columns.Add("MODLID", typeof(string));
                //RQSTDT.Columns.Add("PRODTYPE", typeof(string));
                RQSTDT.Columns.Add("FROMDATE", typeof(string));
                RQSTDT.Columns.Add("TODATE", typeof(string));
                //RQSTDT.Columns.Add("ACTID", typeof(string));
                RQSTDT.Columns.Add("EQSGID", typeof(string));
                RQSTDT.Columns.Add("AREAID", typeof(string));
                RQSTDT.Columns.Add("CLASS", typeof(string));

                DataRow searchCondition = RQSTDT.NewRow();
                searchCondition["LANGID"] = LoginInfo.LANGID;
                searchCondition["PRODID"] = Util.GetCondition(cboProduct) == "" ? null : Util.GetCondition(cboProduct); // "VEVECC036030"; //  Util.NVC(cboProduct.SelectedValue) == "" ? null : cboProduct.SelectedValue;
                //searchCondition["MODLID"] = Util.GetCondition(cboProductModel) == "" ? null : Util.GetCondition(cboProductModel); // "ACEN1060I-A1"; //  Util.NVC(cboProductModel.SelectedValue) == "" ? null : cboProductModel.SelectedValue;
                //searchCondition["PRODTYPE"] = null;//"CELL"; //��ǰ����(CELL, CMA, BMA) : CELL�� �����Ͱ� ��� �׽�Ʈ�� ����
                searchCondition["FROMDATE"] = Util.GetCondition(dtpDateFrom);  //dtpDateFrom.SelectedDateTime.ToString();
                searchCondition["TODATE"] = Util.GetCondition(dtpDateTo); //dtpDateTo.SelectedDateTime.ToString();                
                //searchCondition["ACTID"] = actid; // "HOLD_LOT";
                searchCondition["EQSGID"] = Util.GetCondition(cboEquipmentSegment) == "" ? null : Util.GetCondition(cboEquipmentSegment);
                searchCondition["AREAID"] = LoginInfo.CFG_AREA_ID;
                searchCondition["CLASS"] = Util.GetCondition(cboPrdtClass) == "" ? null : Util.GetCondition(cboPrdtClass);

                RQSTDT.Rows.Add(searchCondition);

                dtSearchResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_PACKHOLD_SEARCH", "RQSTDT", "RSLTDT", RQSTDT);

                dgCellHistory.ItemsSource = null;

                if (dtSearchResult.Rows.Count != 0)
                {
                    Util.GridSetData(dgCellHistory, dtSearchResult, FrameOperation);
                }

                Util.SetTextBlockText_DataGridRowCount(tbSearch_cnt, Util.NVC(dtSearchResult.Rows.Count));

            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
      
        private bool getCell_check(string lotId)
        {
            try
            {
                //��ȸ
                DataTable RQSTDT = new DataTable();

                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("LOTID", typeof(string));

                DataRow searchCondition = RQSTDT.NewRow();
                searchCondition["LANGID"] = LoginInfo.LANGID;
                searchCondition["LOTID"] = lotId; // "LOT";

                RQSTDT.Rows.Add(searchCondition);

                dtFindResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_CELLID_VALI", "RQSTDT", "RSLTDT", RQSTDT);

                tbLotInfo.Text = "";                
                string info_step = string.Empty;

                if (dtFindResult.Rows.Count > 0)
                {
                    //2018.12.12
                    ////CELL ����
                    //tbLotInfo.Text = ObjectDic.Instance.GetObjectName("��")   + "(" + dtFindResult.Rows[0]["AREAID"].ToString() + ")/" +
                    //                 ObjectDic.Instance.GetObjectName("����") + "(" + dtFindResult.Rows[0]["EQSGID"].ToString() + ")/" +
                    //                 ObjectDic.Instance.GetObjectName("��ǰ") + "(" + dtFindResult.Rows[0]["PRODNAME"].ToString() + ")/" +
                    //                 ObjectDic.Instance.GetObjectName("����") + "(" + dtFindResult.Rows[0]["CLASS"].ToString() + ") - ";

                    //info_step = ObjectDic.Instance.GetObjectName("��") + "(" + dtFindResult.Rows[0]["AREAID"].ToString() + ")/" +
                    //                 ObjectDic.Instance.GetObjectName("����") + "(" + dtFindResult.Rows[0]["EQSGID"].ToString() + ")/" +
                    //                 ObjectDic.Instance.GetObjectName("��ǰ") + "(" + dtFindResult.Rows[0]["PRODNAME"].ToString() + ")/" +
                    //                 ObjectDic.Instance.GetObjectName("����") + "(" + dtFindResult.Rows[0]["CLASS"].ToString() + ") - ";

                    //DataTable dt = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_CELL_BOX_INFOR", "RQSTDT", "RSLTDT", RQSTDT);

                    ////��������
                    //if(dt != null && dt.Rows.Count > 0)
                    //{
                    //    string boxid = string.Empty;
                        
                    //    if (dt.Rows.Count ==1)
                    //    {
                    //        boxid = dt.Rows[0]["BOXID"].ToString();

                    //        if (boxid == null || boxid == "")
                    //        {
                    //            boxid = ObjectDic.Instance.GetObjectName("������������");
                    //            //tbBoxInfo.Text += boxid;
                    //        }

                    //        tbLotInfo.Text += dt.Rows[0]["CLASS"].ToString() + "_LOT:" + dt.Rows[0]["LOTID"].ToString() + "(" + boxid + ")";
                    //        info_step += dt.Rows[0]["CLASS"].ToString() + "_LOT:" + dt.Rows[0]["LOTID"].ToString() + "(" + boxid + ")";

                    //        dtFindResult.Rows[0]["BOXID"] = boxid;
                    //    }
                    //    else
                    //    {
                    //        for(int i = 0; i<dt.Rows.Count; i++) 
                    //        {
                    //            boxid = dt.Rows[i]["BOXID"].ToString();

                    //            if(boxid == null || boxid == "")
                    //            {
                    //                boxid = ObjectDic.Instance.GetObjectName("������������");                                   
                    //            }

                    //            if (i==0)  //CMA
                    //            {
                    //                tbLotInfo.Text += dt.Rows[i]["CLASS"].ToString() + "_LOT:" + dt.Rows[i]["LOTID"].ToString() + "(" + boxid + ")";
                    //                info_step += dt.Rows[i]["CLASS"].ToString() + "_LOT:" + dt.Rows[i]["LOTID"].ToString() + "(" + boxid + ")";
                    //            }
                    //            else //BMA
                    //            {
                    //                tbLotInfo.Text +=  " / " +dt.Rows[i]["CLASS"].ToString() + "_LOT:" + dt.Rows[i]["LOTID"].ToString() + "(" + boxid + ")";
                    //                info_step += " / " + dt.Rows[i]["CLASS"].ToString() + "_LOT:" + dt.Rows[i]["LOTID"].ToString() + "(" + boxid + ")";
                    //            }

                    //            dtFindResult.Rows[0]["BOXID"] = boxid;
                    //        }
                    //    }                       
                    //}
                    //else
                    //{
                    //    tbLotInfo.Text += ObjectDic.Instance.GetObjectName("������������");
                    //    info_step += ObjectDic.Instance.GetObjectName("������������");
                    //}

                    if (dtFindResult.Rows[0]["AREAID"].ToString() != LoginInfo.CFG_AREA_ID.ToString())
                    {
                        //Util.AlertInfo("LOGIN ��(AREA)�� LOT�� ���� �ٸ��ϴ�.");
                        ms.AlertWarning("SFU3335"); //�Է¿��� : LOGIN ������� �������� LOT�� �������� �ٸ��ϴ�.
                        return false;
                    }
                 
                    cell_info += info_step;

                    return true;
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

        private void getReturnTagetCell_By_Excel()
        {
            try
            {
                OpenFileDialog fd = new OpenFileDialog();

                if (ConfigurationManager.AppSettings["APP_CONFIG_SBC_MODE"] == "SBC")
                {
                    fd.InitialDirectory = @"\\Client\C$";
                }

                fd.Filter = "Excel Files (*.xlsx,*.xls)|*.xlsx;*.xls";
                if (fd.ShowDialog() == true)
                {
                    using (Stream stream = fd.OpenFile())
                    {
                        DataTable dtExcelData = LoadExcelHelper.LoadExcelData(stream, 0, 0, true);

                        if (dtExcelData != null)
                        {
                            ReturnChkAndReturnCellCreate_ExcelOpen(dtExcelData);
                        }

                        //string sFileName = fd.FileName.ToString();

                        //if (sFileName != null && (sFileName.Substring(sFileName.Length - 4, 4).ToUpper() == "XLSX" || sFileName.Substring(sFileName.Length - 3, 3).ToUpper() == "XLS"))
                        //{                           
                        //    if (exl != null)
                        //    {
                        //        //���� ���� ����
                        //        exl.Conn_Close();
                        //    }
                        //    //���ϸ� Set ���� ����
                        //    exl.ExcelFileName = sFileName;
                        //    string[] str = exl.GetExcelSheets();
                        //    if (str.Length > 0)
                        //    {                               
                        //        ReturnChkAndReturnCellCreate_ExcelOpen(exl.GetSheetData(str[0]));
                        //    }
                        //}
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private void ReturnChkAndReturnCellCreate_ExcelOpen(DataTable dt)
        {
            try
            {
                if (dt != null)
                {
                    // ���̺�����ؾ���!!!
                    string vali_fail_list = "";
                    string temp_cellId = "";
                    string distinct = "";
                    int intFirstRow = 0;
                    bool addYn;

                    if (dt.Rows.Count > 0 && dt.Rows[0][0].ToString().Length != 10) intFirstRow = 1;

                    tbLotInfo.Visibility = Visibility.Collapsed;

                    for (int i = intFirstRow; i < dt.Rows.Count; i++)
                    {                       
                        addYn = true;

                        int TotalRow = dgCellId.GetRowCount(); 

                        temp_cellId = dt.Rows[i][0].ToString();

                        //getCell_check(temp_cellId);

                        //������ ���� CELL ID��
                        if (!getCell_check(temp_cellId))
                        {
                            if(vali_fail_list == "")
                            {
                                vali_fail_list = temp_cellId + ", ";
                            }
                            else
                            {
                                vali_fail_list = vali_fail_list + temp_cellId + ", ";
                            }

                            addYn = false;
                        }

                        //2. cell list�� �����ϴ��� Ȯ��
                        for (int j = 0; j < TotalRow; j++)
                        {
                            string grid_id = DataTableConverter.GetValue(dgCellId.Rows[j].DataItem, "LOTID").ToString();

                            if (temp_cellId == grid_id)
                            {
                                if(distinct == "")
                                {
                                    distinct = temp_cellId + ", ";
                                }
                                else
                                {
                                    distinct = distinct + temp_cellId + ", ";
                                }
                                //LGC.GMES.MES.ControlsLibrary.MessageBox.Show("�̹� ����Ʈ�� ��ϵ� CELL ID �Դϴ�.");

                                //txtLotId.Focus();
                                //return;
                                addYn = false;
                            }
                        }

                        if(addYn) cellGridAdd(TotalRow); //�׸��忡 �߰�
                    }

                    if(cell_info.Length > 0)
                    {
                        ms.AlertInfo(cell_info);

                        cell_info = "";
                    }

                    if(vali_fail_list != "")
                    {
                        //Util.AlertInfo("���� UPLOAD ��� LOT ID : " + vali_fail_list + " �� HOLD �� �� ���� ������ LOT�Դϴ�.\n(���������ʰų� ���, �̹� HOLD�� LOT��)");
                        ms.AlertWarning("SFU3397", vali_fail_list); //���� UPLOAD ��� LOT ID : {0} �� HOLD �� �� ���� ������ LOT�Դϴ�.\n(���������ʰų� ���, �̹� HOLD�� LOT��)
                    }

                    if(distinct != "")
                    {
                        //Util.AlertInfo("LOT ID : " + distinct + " �� �̹� �׸��忡 �߰��� ID �Դϴ�.\n");
                        ms.AlertWarning("SFU3337", distinct); //�Է¿��� : �̹� Ȧ�� ��� ����Ʈ�� ��ϵ� LOTID %1�Դϴ�.
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private void getSearch()
        {
            try
            {
                getCell("HOLD_LOT");
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private void setHold(string bigo)
        {
            try
            {
                if (dgCellId == null)
                {
                    return;
                }

                DataSet dsInput = new DataSet();

                DataTable INDATA = new DataTable();
                INDATA.TableName = "INDATA";
                INDATA.Columns.Add("SRCTYPE", typeof(string));
                INDATA.Columns.Add("LANGID", typeof(string));
                INDATA.Columns.Add("IFMODE", typeof(string));
                INDATA.Columns.Add("USERID", typeof(string));
                //2019.08.19
                INDATA.Columns.Add("ACTION_USERID", typeof(string));
                INDATA.Columns.Add("CALDATE", typeof(string));

                DataRow drINDATA = INDATA.NewRow();
                drINDATA["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                drINDATA["LANGID"] = LoginInfo.LANGID;
                drINDATA["IFMODE"] = "OFF"; //���񿡼� HOLD ó���ϴ��� ���� : UI-OFF
                drINDATA["USERID"] = LoginInfo.USERID;
                //2019.08.19
                drINDATA["ACTION_USERID"] = txtPersonId.Text;
                drINDATA["CALDATE"] = dtpCalDate.SelectedDateTime.ToString("yyyy-MM-dd");
                INDATA.Rows.Add(drINDATA);

                DataTable INLOT = new DataTable();
                INLOT.TableName = "INLOT";
                INLOT.Columns.Add("LOTID", typeof(string));
                INLOT.Columns.Add("HOLD_NOTE", typeof(string));
                INLOT.Columns.Add("RESNCODE", typeof(string));
                INLOT.Columns.Add("HOLD_CODE", typeof(string));
                INLOT.Columns.Add("UNHOLD_SCHD_DATE", typeof(string));

                int chk_idx = 0;
                int total_row = dgCellId.GetRowCount();

                for (int i = 0; i < total_row; i++)
                {
                    //if (DataTableConverter.GetValue(dgCellId.Rows[i].DataItem, "CHK").ToString() == "True")
                    //{
                    DataRow drINLOT = INLOT.NewRow();
                    drINLOT["LOTID"] = DataTableConverter.GetValue(dgCellId.Rows[i].DataItem, "LOTID").ToString();
                    drINLOT["HOLD_NOTE"] = bigo;
                    drINLOT["RESNCODE"] = Util.GetCondition(cboHoldReason) == "" ? null : cboHoldReason.SelectedValue;
                    drINLOT["HOLD_CODE"] = Util.GetCondition(cboHoldReason) == "" ? null : cboHoldReason.SelectedValue;
                    //2019.08.19
                    //drINLOT["UNHOLD_SCHD_DATE"] = null;
                    drINLOT["UNHOLD_SCHD_DATE"] = Util.GetCondition(dtpDate);

                    INLOT.Rows.Add(drINLOT);

                    chk_idx++;
                    //}
                }

                if (chk_idx == 0)
                {
                    return;
                }

                dsInput.Tables.Add(INDATA);
                dsInput.Tables.Add(INLOT);
               
                new ClientProxy().ExecuteServiceSync_Multi("BR_PRD_REG_HOLD_LOT", "INDATA,INLOT", null, dsInput);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private void setUnHold()
        {
            try
            {
                if (dgCellHistory == null)
                {
                    return;
                }

                DataSet dsInput = new DataSet();

                DataTable INDATA = new DataTable();
                INDATA.TableName = "INDATA";
                INDATA.Columns.Add("SRCTYPE", typeof(string));
                INDATA.Columns.Add("LANGID", typeof(string));
                INDATA.Columns.Add("IFMODE", typeof(string));
                INDATA.Columns.Add("USERID", typeof(string));

                DataRow drINDATA = INDATA.NewRow();
                drINDATA["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                drINDATA["LANGID"] = LoginInfo.LANGID;
                drINDATA["IFMODE"] = "OFF"; //���񿡼� HOLD ó���ϴ��� ���� : UI-OFF
                drINDATA["USERID"] = LoginInfo.USERID;
                INDATA.Rows.Add(drINDATA);

                DataTable INLOT = new DataTable();
                INLOT.TableName = "INLOT";
                INLOT.Columns.Add("LOTID", typeof(string));
                INLOT.Columns.Add("RESNCODE", typeof(string));
                INLOT.Columns.Add("UNHOLD_NOTE", typeof(string));
                INLOT.Columns.Add("UNHOLD_CODE", typeof(string));

                int chk_idx = 0;
                int total_row = dgCellHistory.GetRowCount();
                for (int i = 0; i < total_row; i++)
                {
                    if (DataTableConverter.GetValue(dgCellHistory.Rows[i].DataItem, "CHK").ToString() == "True")
                    {
                        DataRow drINLOT = INLOT.NewRow();
                        drINLOT["LOTID"] = DataTableConverter.GetValue(dgCellHistory.Rows[i].DataItem, "LOTID").ToString();
                        drINLOT["RESNCODE"] = Util.GetCondition(cboUnHoldReason) == "" ? null : cboUnHoldReason.SelectedValue;
                        drINLOT["UNHOLD_NOTE"] = txtNote.Text;
                        drINLOT["UNHOLD_CODE"] = Util.GetCondition(cboHoldReason) == "" ? null : cboHoldReason.SelectedValue; //P090000T

                        INLOT.Rows.Add(drINLOT);

                        chk_idx++;
                    }
                }

                if (chk_idx == 0)
                {
                    return;
                }

                dsInput.Tables.Add(INDATA);
                dsInput.Tables.Add(INLOT);

                new ClientProxy().ExecuteServiceSync_Multi("BR_PRD_REG_UNHOLD_LOT", "INDATA,INLOT", null, dsInput);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        //2019.08.19
        private void GetUserWindow()
        {
            CMM_PERSON wndPerson = new CMM_PERSON();
            wndPerson.FrameOperation = FrameOperation;

            if (wndPerson != null)
            {
                object[] Parameters = new object[1];
                Parameters[0] = txtPerson.Text;
                C1WindowExtension.SetParameters(wndPerson, Parameters);

                wndPerson.Closed += new EventHandler(wndUser_Closed);
                Search.Children.Add(wndPerson);
                wndPerson.BringToFront();
            }
        }

        //2019.08.19
        private void wndUser_Closed(object sender, EventArgs e)
        {
            CMM_PERSON wndPerson = sender as CMM_PERSON;
            if (wndPerson.DialogResult == MessageBoxResult.OK)
            {
                txtPerson.Text = wndPerson.USERNAME;
                txtPersonId.Text = wndPerson.USERID;
                txtPersonDept.Text = wndPerson.DEPTNAME;
            }
            else
            {
                txtPerson.Text = string.Empty;
                txtPersonId.Text = string.Empty;
                txtPersonDept.Text = string.Empty;
            }
        }

        #endregion


    }
}
