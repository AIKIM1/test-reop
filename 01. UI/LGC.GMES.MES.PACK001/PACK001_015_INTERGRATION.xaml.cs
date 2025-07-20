/*************************************************************************************
 Created Date : 2016.06.16
      Creator : �常ö
   Decription : ����� �� ����, ����� ȭ��
--------------------------------------------------------------------------------------
 [Change History]
  2016.06.16  DEVELOPER : Initial Created.
  2018.10.08  �տ켮 ���� ��ǰ �ٸ� ����/�� ����Ʈ �ΰ�� ���� ���� �Ķ���� �߰�
  2020.10.22  ��ȣ��         2nd OOCV �����и�(P5200,P5400) �����߰�
  2021.03.22 ���Թ� DA_PRD_UPD_TB_SFC_LABEL_PRT_REQ_HIST_PRTFLAG Ÿ�� �ƿ� �̽� �ذ�� ���� Ÿ�� ���� ó��
**************************************************************************************/

using System;
using System.ComponentModel;
using System.Data;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using C1.WPF;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.CMM001.Popup;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;

namespace LGC.GMES.MES.PACK001
{
    public partial class PACK001_015_INTERGRATION : UserControl, IWorkArea
    {
        #region Declaration & Constructor 
        LGC.GMES.MES.PACK001.Class.MESSAGE_PARAM ms = new LGC.GMES.MES.PACK001.Class.MESSAGE_PARAM();
        DataTable dtSearchResult; //��ȸ ����� ��� ���� DAtaTable
        DataTable dtModelResult;
        DataTable dtResult;
        DataTable dtTextResult;
        System.ComponentModel.BackgroundWorker bkWorker; //bakcground thread

        #region MAIN ����
        //flag ����
        bool boxingYn = false;   //true - box ������, �̿Ϸ� / false - �ڽ����� ����
        bool unPackYn = false;   //����Ʈ���� ���� ���� ���� ���� : true-�������� ���� / false-�������� �Ұ���       
        bool rePrint = false;    //����Ʈ���� ���� �Ұ����� ����Ϸ� �� ���� �Ұ����� : true-����� / false-���ʹ���
        bool reBoxing = false;   //������ ����

        //���� ���� ����
        int boxLotmax_cnt = 0;   //�ڽ��� ��� lot�� �ִ� ����
        int boxingLot_idx = 0;   //�ڽ��� ��� Lot�� ����
        int lotCountReverse = 0; //�ڽ��� ��� lot�� ���� ����

        //box ���� ����
        string seleted_boxid;
        string boxing_lot;    //�������� boxid
        string OKCancel_Desc; //����Ϸ�� ���� ���ο� ���� text   
        string box_prod = ""; //�ڽ���prod : ���ԵǴ� lot�� prod�� ���ϱ� ���� ����
        string box_eqsg = "";
        string seleted_palletID;
        string model;

        //���ԵǴ� lot ���� ����
        string lot_prod = string.Empty;     //������ ������ lot�� prodid         
        string lot_proc = string.Empty;     //������ ������ lot�� procid
        string lot_eqsgid = string.Empty;   //������ ������ lot��  eqsgid
        string lot_class_old = string.Empty;//������ ������ lot�� class

        #region �������� ���� �������� ����
        string seleted_Box_Prod = string.Empty;    // �������� ���� prodid
        string seleted_Box_Procid = string.Empty;  // �������� ���� procid
        string seleted_Box_Eqptid = string.Empty;  // �������� ���� eqptid
        string seleted_Box_Eqsgid = string.Empty;  // �������� ���� eqsgid
        string seleted_Box_PrdClass = string.Empty;// �������� ���� prdclass
        string seleted_oqc_insp_id = string.Empty; // �������� ���� ���������� oqc_insp_id�� �ʱ�ȭ ��Ű�� ���� ����
        string seleted_judg_value = string.Empty;  // �������� ���� ���������� oqc_insp_id�� �ʱ�ȭ ��Ű�� ���� ����
        #endregion

        #region ����Ʈ���� ���������� ���� ����
        string unPack_ProdID = string.Empty;       // ���������� ���� prodid
        string unPack_EqsgID = string.Empty;       // ���������� ���� eqsgid
        string unPack_EqptID = string.Empty;       // ���������� ���� eqptid
        string unPack_ProcID = string.Empty;       // ���������� ���� procid
        string unPack_PrdClasee = string.Empty;    // ���������� ���� prdclass
        string unPack_oqc_insp_id = string.Empty;  // ���������� ���� ���������� oqc_insp_id�� �ʱ�ȭ ��Ű�� ���� ����
        string unPack_judg_value = string.Empty;   // ���������� ���� ���������� oqc_insp_id�� �ʱ�ȭ ��Ű�� ���� ����
        #endregion

        private bool blPrintStop = true;
        string label_code2 = "LBL0020";
        string zpl2 = string.Empty;
        string selectBox = string.Empty;
        string selectPallet = string.Empty;

        string combo_prd = string.Empty;
        bool palletYN = false;
        #endregion

        #region 315H
        bool boxingYn_315H = false; //���������� ����
        bool reBoxing_315H = false; //������ ���� : box id ��ȸ�� ���������(create/packed ��� ����) - packed�� ��츸 ���Ŀ� �������� ����
        bool unPackYN_315H = false; //�������� �ߴ��� ���� : boxid �Է� �� ���� lot �Է½� or lot ������ unpack �ϱ� ���� �뵵
        bool palletYN_315H = false; //�ȷ�Ʈ�� ����� box���� ����

        string boxStat_315H = string.Empty; //box�� stat : created / packed�� ��츸 �������� - packed�� ��� ���� �������� ����

        int boxingLot_idx_315H = 0;
        private bool blPrintStop_315H = true;
        string label_code_315H = "LBL0020";
        string zpl_315H = string.Empty;
        string prdClass_315H = "CMA";

        string combo_prd_315H = string.Empty;

        string lot_prod_315H = string.Empty; //������ ������ lot�� prodid         
        string lot_proc_315H = string.Empty; //������ ������ lot�� procid
        string lot_eqsgid_315H = string.Empty; //������ ������ lot��  eqsgid
        string lot_class_old_315H = string.Empty; //������ ������ lot�� class

        string box_prod_315H = string.Empty; //���� �����ߴ� BOX�� PRODID
        string boxing_lot_315H = string.Empty; //���� �������� BOXID

        string seleted_Box_Prod_315H = string.Empty;
        string seleted_Box_Procid_315H = string.Empty;
        string seleted_Box_Eqptid_315H = string.Empty;
        string seleted_Box_Eqsgid_315H = string.Empty;
        string seleted_BOX_LotCNT_315H = string.Empty;
        #endregion


        public IFrameOperation FrameOperation
        {
            get;
            set;
        }

        public PACK001_015_INTERGRATION()
        {
            InitializeComponent();

            this.Loaded += PPACK001_015_INTERGRATION_Loaded;
        }

       

        #endregion

        #region Initialize
        private void Initialize()
        {
            //���� �ʱⰪ ����
            dtpDateFrom.SelectedDateTime = (DateTime)System.DateTime.Now.AddDays(-7); //������ �� ��¥
            dtpDateTo.SelectedDateTime = (DateTime)System.DateTime.Now;//���� ��¥

            tbBoxingWait_cnt.Text = "[ 0 " + ObjectDic.Instance.GetObjectName("��") + " ]";
            tbBoxHistory_cnt.Text = "[ 0 " + ObjectDic.Instance.GetObjectName("��") + " ]";
            tbBoxingWait_cnt_315H.Text = "[ 0 " + ObjectDic.Instance.GetObjectName("��") + " ]";
            tbPalletHistory_cnt.Text = "[ 0 " + ObjectDic.Instance.GetObjectName("��") + " ]";

            #region 312H, 313H, 515H
            dtpDate2.SelectedDateTime = (DateTime)System.DateTime.Now;

            dtpDateFrom2.SelectedDateTime = (DateTime)System.DateTime.Now.AddDays(-7); //������ �� ��¥
            //dtpDateFrom2.Text = DateTime.Now.AddDays(-7).ToString("yyyy-MM-dd");
            dtpDateTo2.SelectedDateTime = (DateTime)System.DateTime.Now;

            txtDate2.Text = PrintOutDate(DateTime.Now);  //txtZpl018.Text = PrintOutDate(DateTime.Now);
            //dtpDate_SelectedDateChanged(null, null); //dtp312HDay_ValueChanged(null, null); dtpDate

            bkWorker = new System.ComponentModel.BackgroundWorker();
            bkWorker.WorkerSupportsCancellation = true;
            bkWorker.DoWork += new System.ComponentModel.DoWorkEventHandler(bkWorker_DoWork);
            bkWorker.RunWorkerCompleted += new System.ComponentModel.RunWorkerCompletedEventHandler(this.bkWorker_RunWorkerCompleted);

            setTexBox();
            #endregion

            InitCombo();
        }
        #endregion

        #region BOX ���� MAIN
        #region Event

        #region MAIN EVENT
        private void PPACK001_015_INTERGRATION_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                Initialize();

                search();

                this.Loaded -= PPACK001_015_INTERGRATION_Loaded;
            }
            catch (Exception ex)
            {
                Util.AlertInfo(ex.Message);
            }
        }
        private void tcMain_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (tcMain.SelectedIndex == 1) //315H
            {
                init_315H();
            }
            else if (tcMain.SelectedIndex == 2)
            {
                init_312_313_315_515();
            }
        }
        #endregion

        #region BUTTON EVENT
        private void btnBoxLabel_Click(object sender, RoutedEventArgs e)
        { 
            try
            {
                rePrint = true; //�����
                if (txtBoxIdR.Text.Length > 0)
                {

                    model = model.Substring(4);

                    if (model == "312H" || model == "313H" || model == "515H" || model == "MOKA" || model == "KANG")
                    {
                        if(selectPallet != null && selectPallet.Length > 0)
                        {
                            set312_313_315_517();
                        }
                        else
                        {
                            labelPrint(sender);
                        }
                    }
                    else
                    {
                        labelPrint(sender);
                    }
                }
                rePrint = false;
            }
            catch (Exception ex)
            {
                Util.AlertInfo(ex.Message);
            }
        }
        
        private void btnPacCancel_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if(txtBoxIdR.Text.Length == 0)
                {
                    return;
                }

                if (!unPackYn)
                {
                    Util.AlertInfo("�̹� PALLET�� ��� BOX �Դϴ�. UPNPACK �Ҽ� �����ϴ�.");
                    return;
                }

                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(ObjectDic.Instance.GetObjectName("���� ������� �Ͻðڽ��ϱ�?"), null, "CAUSE", MessageBoxButton.OKCancel, MessageBoxIcon.Warning, (caution_result) =>
                {
                    if (caution_result == MessageBoxResult.OK)
                    {  
                        pack_unPack_init(sender);

                        btnPack.Tag = ObjectDic.Instance.GetObjectName("�����");

                        //LGC.GMES.MES.ControlsLibrary.MessageBox.Show("������ ���� �Ǿ����ϴ�.", null, "�Ϸ�", MessageBoxButton.OK, ControlsLibrary.MessageBoxIcon.None);

                        search();
                    }
                    else
                    {
                        return;
                    }
                }
                   );
            }
            catch (Exception ex)
            {
                Util.AlertInfo(ex.Message);
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
                Util.AlertInfo(ex.Message);
            }
        }

        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {            
            try
            {
                if (cboArea.SelectedIndex == 0)
                {
                    Util.AlertInfo("��(AREA)�� �����ϼ���");
                    return;
                }

                search();
            }
            catch (Exception ex)
            {
                Util.AlertInfo(ex.Message);
            }
        }       

        private void btnSelectCacel_Click(object sender, RoutedEventArgs e)
        {
            if (dgBoxLot.ItemsSource != null)
            {
                for (int i = dgBoxLot.Rows.Count; 0 < i; i--)
                {
                    var chkYn = DataTableConverter.GetValue(dgBoxLot.Rows[i - 1].DataItem, "CHK");
                    var lot_id = DataTableConverter.GetValue(dgBoxLot.Rows[i - 1].DataItem, "LOTID");

                    if (chkYn == null)
                    {
                        dgBoxLot.RemoveRow(i - 1);
                    }
                    else if (Convert.ToBoolean(chkYn))
                    {
                        dgBoxLot.EndNewRow(true);
                        dgBoxLot.RemoveRow(i - 1);
                        boxingLot_idx--;
                        lotCountReverse++;

                        setBoxCnt(boxLotmax_cnt, boxingLot_idx, lotCountReverse,  "������");
                    }
                }

                DataTable dt = DataTableConverter.Convert(dgBoxLot.ItemsSource);

                Util.SetTextBlockText_DataGridRowCount(tbBoxingWait_cnt, Util.NVC(dt.Rows.Count));
            }
        }

        private void btncancel_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if(dgBoxLot.GetRowCount() == 0)
                {
                    return;
                }

                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(ObjectDic.Instance.GetObjectName("���� ���帮��Ʈ�� ���� �Ͻðڽ��ϱ�?"), null, "CAUSE", MessageBoxButton.OKCancel, MessageBoxIcon.Warning, (caution_result) =>
                {
                    if (caution_result == MessageBoxResult.OK)
                    {
                        dgBoxLot.ItemsSource = null;
                        boxingLot_idx = 0;
                        lotCountReverse = boxLotmax_cnt;
                        //boxingYn = false;

                        tbBoxingWait_cnt.Text = "[ 0 " + ObjectDic.Instance.GetObjectName("��") + " ]";

                        if (txtBoxId.Text.ToString() == "")
                        {
                            boxLotmax_cnt = 0;
                        }

                        setBoxCnt(boxLotmax_cnt, boxingLot_idx, lotCountReverse, "������");
                    }
                    else
                    {
                        return;
                    }
                }
                  );
            }
            catch (Exception ex)
            {
                Util.AlertInfo(ex.Message);
            }
        }

        private void btnPack_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (btnPack.Content.ToString() == ObjectDic.Instance.GetObjectName("�����۾���ȯ"))
                {
                    txtcnt.Visibility = Visibility.Visible;
                    btnUnPack.Visibility = Visibility.Hidden;

                    btnSelectCacel.Visibility = Visibility.Visible;
                    btncancel.Visibility = Visibility.Visible;
                    chkBoxId.IsChecked = false;
                    txtBoxId.IsEnabled = true;
                    txtLotId.IsEnabled = true;
                    chkBoxId.Visibility = Visibility.Visible;
                    txtBoxId.IsReadOnly = false;

                    btnPack.Content = ObjectDic.Instance.GetObjectName("����");
                    btnPack.Tag = "�ű�";

                    boxingLot_idx = 0;
                    boxLotmax_cnt = 0; // �ڽ� ���� ���� ���� - �������� ����  
                    nbBoxingCnt.Value = 5;
                    lotCountReverse = Convert.ToInt32(nbBoxingCnt.Value);
                    tbBoxingWait_cnt.Text = "[ 0 " + ObjectDic.Instance.GetObjectName("��") + " ]";



                    dgBoxLot.ItemsSource = null;
                    (dgBoxLot.Columns["CHK"] as C1.WPF.DataGrid.DataGridCheckBoxColumn).MaxWidth =40;
                    txtLotId.IsEnabled = true;

                    boxingYn = false;

                    setBoxCnt(boxLotmax_cnt, boxingLot_idx, lotCountReverse, "�����");

                    txtBoxId.Text = "";
                    txtLotId.Text = "";                   

                    search();
                }
                else
                {
                    if (txtBoxId.Text == "")
                    {
                        return;
                    }

                    if (dgBoxLot.GetRowCount() == 0)
                    {
                        return;
                    }

                    if (boxingYn == true && (boxLotmax_cnt == boxingLot_idx))
                    {
                        OKCancel_Desc = ObjectDic.Instance.GetObjectName("����Ϸ� �Ͻðڽ��ϱ�?");
                    }
                    else if (boxLotmax_cnt > boxingLot_idx)
                    {
                        OKCancel_Desc = ObjectDic.Instance.GetObjectName("BOX ���� ������ LOT ������ ��ġ���� �ʽ��ϴ�.\n���� �Ϸ� �Ͻðڽ��ϱ� ?");
                    }

                   LGC.GMES.MES.ControlsLibrary.MessageBox.Show(OKCancel_Desc, null, "CAUSE", MessageBoxButton.OKCancel, MessageBoxIcon.Warning, (caution_result) =>
                    {
                        if (caution_result == MessageBoxResult.OK)
                        {
                            boxingEnd(); //���� �Ϸ� �Լ�

                            //reSet();

                            reSet_Last_Stat();


                            //boxingYn = false; //����Ϸ� �Ǵ� ���� ��� flag

                            //control_Init(); //control �ʱ�ȭ

                            //setBoxCnt(5, 0, 5, "�����");

                            //�󺧹���
                            //�űԹ���, �����
                            //labelPrint(sender);

                            Util.AlertInfo("����Ϸ�");
                            search();
                        }
                        else
                        {
                            return;
                        }
                    }
                   );
                }
            }
            catch(Exception ex)
            {
                Util.AlertInfo(ex.Message);
            }
        }

        private void btnUnPack_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (txtBoxId.Text.Length == 0)
                {
                    return;
                }

                bool UNPACKYN = false;

                if (seleted_palletID != null && seleted_palletID != "")
                {
                    UNPACKYN = false;

                    //box_eqsg = seleted_Pallet_Eqsgid;
                    //box_prod = seleted_Pallet_Prod;

                }
                else
                {
                    UNPACKYN = true;
                }

                if (!UNPACKYN)
                {
                    Util.AlertInfo("PALLET�� ��� BOX�� ������� �Ҽ� �����ϴ�.");
                    return;
                }

                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(ObjectDic.Instance.GetObjectName("���� ������� �Ͻðڽ��ϱ�?"), null, "CAUSE", MessageBoxButton.OKCancel, MessageBoxIcon.Warning, (caution_result) =>
                {
                    if (caution_result == MessageBoxResult.OK)
                    {
                        //UNPACK ����
                        pack_unPack_init(sender);

                        btnPack.Tag = ObjectDic.Instance.GetObjectName("�����");

                        //LGC.GMES.MES.ControlsLibrary.MessageBox.Show("������ ��� �Ǿ����ϴ�.", null, "�Ϸ�", MessageBoxButton.OK, ControlsLibrary.MessageBoxIcon.None);

                        search();
                    }
                    else
                    {
                        return;
                    }
                }
                   );
            }
            catch(Exception ex)
            {
                Util.AlertInfo(ex.Message);
            }           
        }

        private void btnReset_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!boxingYn)
                {
                    reSet();

                    Util.AlertInfo("�۾��� �ʱ�ȭ �ƽ��ϴ�.");
                }
                else
                {
                    LGC.GMES.MES.ControlsLibrary.MessageBox.Show(ObjectDic.Instance.GetObjectName("�������� ������ �ֽ��ϴ�. ���� [�۾��ʱ�ȭ] �Ͻðڽ��ϱ�?"), null, "CAUSE", MessageBoxButton.OKCancel, MessageBoxIcon.Warning, (caution_result) =>
                    {
                        if (caution_result == MessageBoxResult.OK)
                        {
                            reSet();
                            Util.AlertInfo("�۾��� �ʱ�ȭ �ƽ��ϴ�.");
                        }
                        else
                        {
                            return;
                        }
                    }
              );
                }
            }
            catch (Exception ex)
            {
                Util.AlertInfo(ex.Message);
                //LGC.GMES.MES.ControlsLibrary.MessageBox.Show(ex.Message, null, ObjectDic.Instance.GetObjectName("�Ϸ�"), MessageBoxButton.OK, ControlsLibrary.MessageBoxIcon.None);
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                reBoxing = true; //������
                buttonAccess(sender);
            }
            catch (Exception ex)
            {
                Util.AlertInfo(ex.Message);
            }
        }
        #endregion

        #region GRID EVENT
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

                int currentRow = cell.Row.Index;
                int _col = cell.Column.Index;
                string value = cell.Value.ToString();

                if (dgBoxhistory.Rows[currentRow].DataItem == null)
                {
                    return;
                }

                selectBox = DataTableConverter.GetValue(dgBoxhistory.Rows[currentRow].DataItem, "BOXID").ToString();

                if (DataTableConverter.GetValue(dgBoxhistory.Rows[currentRow].DataItem, "PALLETID") != null)
                {
                    selectPallet = DataTableConverter.GetValue(dgBoxhistory.Rows[currentRow].DataItem, "PALLETID").ToString();
                }

                //PALLET�� ��� BOX�� ���� �Ұ���
                if (selectPallet == null || selectPallet == "")
                {
                    unPackYn = true;
                }
                else
                {
                    unPackYn = false;
                }
                
                txtBoxIdR.Text = selectBox;

                unPack_ProdID = DataTableConverter.GetValue(dgBoxhistory.Rows[currentRow].DataItem, "PRODID").ToString(); //PALLET�� ��ǰ
                unPack_ProcID = DataTableConverter.GetValue(dgBoxhistory.Rows[currentRow].DataItem, "PROCID") == null ? null : DataTableConverter.GetValue(dgBoxhistory.Rows[currentRow].DataItem, "PROCID").ToString(); //PALLET�� ��ǰ
                unPack_EqptID = DataTableConverter.GetValue(dgBoxhistory.Rows[currentRow].DataItem, "EQPTID").ToString(); //PALLET�� ��ǰ
                unPack_EqsgID = DataTableConverter.GetValue(dgBoxhistory.Rows[currentRow].DataItem, "EQSGID").ToString(); //PALLET�� ��ǰ
                
                boxLotmax_cnt = Convert.ToInt32(DataTableConverter.GetValue(dgBoxhistory.Rows[currentRow].DataItem, "BOXLOTCNT")); //����� ����
                boxingLot_idx = Convert.ToInt32(DataTableConverter.GetValue(dgBoxhistory.Rows[currentRow].DataItem, "BOXLOTCNT")); //����� ����

                model = DataTableConverter.GetValue(dgBoxhistory.Rows[currentRow].DataItem, "MODEL").ToString(); //PALLET�� ��

            }
            catch(Exception ex)
            {
                Util.AlertInfo(ex.Message);
            }            
        }

        private void gridDoubleClickProcess(object sender, MouseButtonEventArgs e, C1.WPF.DataGrid.C1DataGrid grid)
        {
            try
            {
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

                            //popup.Closed -= popup_Closed;
                            //popup.Closed += popup_Closed;
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

        private void dgBoxLot_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            try
            {
                C1.WPF.DataGrid.C1DataGrid grid = sender as C1.WPF.DataGrid.C1DataGrid;

                gridDoubleClickProcess(sender, e, grid);
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }
        #endregion

        #region TEXTBOX EVENT
        private void txtBoxId_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                try
                {
                    SetBoxkeyDown();
                }
                catch(Exception ex)
                {
                    Util.AlertInfo(ex.Message);
                }
            }
        }

        private void txtLotId_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                if (txtBoxId.Text == "") //boxid �� �Է� ���� �ʾҰ�
                {
                    if ((bool)chkBoxId.IsChecked) // BOXID üũ �ڽ��� üũ ������� BOXID �ڵ� ����
                    {
                        try
                        {
                            //�Էµ� lot validation
                            if (!lotValidation_BR()) //if (!lotValidation())
                            {
                                txtLotId.Text = "";
                                return;
                            }

                            //BOXID �ڵ� ����
                            autoBoxIdCreate();

                            //������ boxid�� prod ������.
                            getBoxProd();

                            if(lot_prod != box_prod)
                            {
                                Util.AlertInfo("BOX�� PROD�� LOT�� PROD�� �ٸ��ϴ�.");
                                return;
                            }

                            Util.gridClear(dgBoxLot); //�׸��� clear

                            //���� ���� ���� ����
                            setBoxLotCnt();

                            //lot�� �׸���(dgBoxLot)�� �߰�
                            addGridLot();

                            // BOX Label �ڵ�����
                            //labelPrint();

                            //boxing ���� �ʱ�ȭ
                            //boxingStatInit();

                            txtLotId.Text = "";

                            boxingYn = true; //�ڽ���.
                            //boxingLot_idx++; //box�� ��� lot ���� üũ
                            //lotCountReverse--;

                            setBoxCnt(boxLotmax_cnt, boxingLot_idx, lotCountReverse, "������");
                        }
                        catch (Exception ex)
                        {
                            Util.AlertInfo(ex.Message);
                        }
                    }
                    else
                    {
                        Util.AlertInfo("BoxId ���� �Է��ϼ���");
                    }
                }
                else
                {
                    try
                    {
                        //box�� ��� lot�� ����üũ
                        if (boxLotmax_cnt > boxingLot_idx)
                        {
                            if (gridDistinctCheck("lot")) //�׸��� �ߺ� üũ
                            {
                                //�Էµ� lotid validation
                                if (!lotValidation_BR()) //if (!lotValidation())
                                {
                                    txtLotId.Text = "";
                                    return;
                                }

                                if (lot_prod != box_prod)
                                {
                                    Util.AlertInfo("BOX�� PROD�� LOT�� PROD�� �ٸ��ϴ�.");
                                    return;
                                }

                                addGridLot();
                                setBoxCnt(boxLotmax_cnt, boxingLot_idx, lotCountReverse, "������");

                                txtLotId.Text = "";
                            }
                        }
                        else
                        {
                            Util.AlertInfo("���� ���� ����( " + boxLotmax_cnt.ToString() + " )�� �ѽ��ϴ�.");
                        }
                    }
                    catch (Exception ex)
                    {
                        txtLotId.Text = "";
                        Util.AlertInfo(ex.Message);
                    }
                }
            }
        }

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

                    SearchBox();
                }
                catch (Exception ex)
                {
                    Util.AlertInfo(ex.Message);
                }
            }
        }
        #endregion       

        #region CHECKBOX EVENT       

        private void chkBoxId_Click(object sender, RoutedEventArgs e)
        {
            if ((bool)chkBoxId.IsChecked)
            {
                if(boxingYn)
                {
                    chkBoxId.IsChecked = false;
                    txtBoxId.Text = boxing_lot;
                    txtLotId.Text = "";

                    return;
                }

                txtBoxId.IsReadOnly = true;
                txtBoxId.Text = "";
            }
            else
            {
                if(boxingYn)
                {
                    chkBoxId.IsChecked = true;
                    return;
                }

                txtBoxId.IsReadOnly = false;
            }
        }
        #endregion

        #region ��Ÿ �̺�Ʈ
        void popup_Closed(object sender, EventArgs e)
        {

        }
        private void nbBoxingCnt_ValueChanged(object sender, PropertyChangedEventArgs<double> e)
        {
            boxLotmax_cnt = Convert.ToInt32(nbBoxingCnt.Value);
            lotCountReverse = boxLotmax_cnt - boxingLot_idx;
            string stat = string.Empty;

            if (boxingYn)
            {
                stat = ObjectDic.Instance.GetObjectName("������");
            }
            else
            {
                stat = ObjectDic.Instance.GetObjectName("�����");
            }

            setBoxCnt(boxLotmax_cnt, boxingLot_idx, lotCountReverse, stat);

        }
        #endregion

        #endregion

        #region Mehod
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
                RQSTDT.Columns.Add("BOXID", typeof(string));
                RQSTDT.Columns.Add("SYSTEM_ID", typeof(string));
                RQSTDT.Columns.Add("USERID", typeof(string));
                RQSTDT.Columns.Add("SHOPID", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["AREAID"] = Util.GetCondition(cboArea);
                dr["PRODID"] = Util.GetCondition(cboProduct);
                dr["MODLID"] = Util.GetCondition(cboProductModel); 
                dr["FROMDATE"] = Util.GetCondition(dtpDateFrom);  //dtpDateFrom.SelectedDateTime.ToString();
                dr["TODATE"] = Util.GetCondition(dtpDateTo); //dtpDateTo.SelectedDateTime.ToString();
                dr["BOXID"] = "";
                dr["SYSTEM_ID"] = LoginInfo.SYSID;
                dr["USERID"] = LoginInfo.USERID;
                dr["SHOPID"] = LoginInfo.CFG_SHOP_ID;

                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_BOXHISTORY_SEARCH", "INDATA", "OUTDATA", RQSTDT);

                dgBoxhistory.ItemsSource = null;
                txtBoxIdR.Text = "";

                if (dtResult.Rows.Count != 0)
                {
                    Util.GridSetData(dgBoxhistory, dtResult, FrameOperation);
                    txtSearchBox.Text = "";
                }

                Util.SetTextBlockText_DataGridRowCount(tbBoxHistory_cnt, Util.NVC(dtResult.Rows.Count));
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }      

        private void InitCombo()
        {
            CommonCombo _combo = new CommonCombo();

            C1ComboBox cboSHOPID = new C1ComboBox();
            cboSHOPID.SelectedValue = LoginInfo.CFG_SHOP_ID;
            C1ComboBox cboAREA_TYPE_CODE = new C1ComboBox();
            cboAREA_TYPE_CODE.SelectedValue = Area_Type.PACK;
            C1ComboBox cboEquipmentSegment = new C1ComboBox();
            cboEquipmentSegment.SelectedValue = null; // LoginInfo.CFG_EQSG_ID;           
            C1ComboBox cboPrdtClass = new C1ComboBox();
            cboPrdtClass.SelectedValue = "";

            //��           
            C1ComboBox[] cboAreaChild = { cboProductModel };
            _combo.SetCombo(cboArea, CommonCombo.ComboStatus.SELECT, cbChild : cboAreaChild);         

            //��          
            //C1ComboBox[] cboProductModelParent = { cboSHOPID, cboArea, cboEquipmentSegment };
            //C1ComboBox[] cboProductModelChild = { cboProduct };
            //_combo.SetCombo(cboProductModel, CommonCombo.ComboStatus.ALL, cbParent: cboProductModelParent, cbChild: cboProductModelChild);

            //��          
            C1ComboBox[] cboProductModelParent = { cboArea, cboEquipmentSegment };
            C1ComboBox[] cboProductModelChild = { cboProduct };
            _combo.SetCombo(cboProductModel, CommonCombo.ComboStatus.ALL, cbParent: cboProductModelParent, cbChild: cboProductModelChild, sCase: "PRJ_MODEL_AUTH");

            //��ǰ    
            //C1ComboBox[] cboProductParent = { cboSHOPID, cboArea, cboEquipmentSegment, cboProductModel, cboAREA_TYPE_CODE, cboPrdtClass };
            //_combo.SetCombo(cboProduct, CommonCombo.ComboStatus.ALL, cbParent: cboProductParent);

            //��ǰ�ڵ�  
            C1ComboBox[] cboProductParent = { cboSHOPID, cboArea, cboEquipmentSegment, cboProductModel, cboPrdtClass };
            _combo.SetCombo(cboProduct, CommonCombo.ComboStatus.ALL, cbParent: cboProductParent, sCase: "PRJ_PRODUCT");

            //getProduct(cboProduct);

            //517H �󺧹��� tab�� ��ǰ�޺�
            setCombo();

        }

        private void setCombo()
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LABEL", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LABEL"] = label_code2; //dtpDateTo.SelectedDateTime.ToString();

                RQSTDT.Rows.Add(dr);

                dtModelResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_PROJECTNAME_PRODID_COMBO", "INDATA", "OUTDATA", RQSTDT);

                cboProduct2.DisplayMemberPath = "CBO_NAME";
                cboProduct2.SelectedValuePath = "CBO_CODE";
                cboProduct2.ItemsSource = DataTableConverter.Convert(dtModelResult); //AddStatus(dtResult, cs, "CBO_CODE", "CBO_NAME").Copy().AsDataView();

                cboProduct2.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private void getBoxProd()
        {
            try
            {
                DataTable RQSTDT = new DataTable();

                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("BOXID", typeof(string));


                DataRow searchCondition = RQSTDT.NewRow();
                searchCondition["BOXID"] = boxing_lot;


                RQSTDT.Rows.Add(searchCondition);

                DataTable dtBoxProd = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_BOXCHECK_PROD", "RQSTDT", "RSLTDT", RQSTDT);

                if (dtBoxProd.Rows.Count > 0)
                {
                    box_prod = dtBoxProd.Rows[0]["PRODID"].ToString();
                    box_eqsg = dtBoxProd.Rows[0]["EQSGID"] == null ? LoginInfo.CFG_EQSG_ID : dtBoxProd.Rows[0]["EQSGID"].ToString();
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private void SetBoxkeyDown()
        {
            try
            {
                if (chkBoxId.IsChecked == true && btnPack.Content.ToString() == ObjectDic.Instance.GetObjectName("����")) //boxid�� üũ �Ǿ� ������ biz���� validaiton ���� ��  boxid ���� �ϹǷ� �������� validation �ʿ����.
                {
                    Util.AlertInfo("chekbox�� Ǯ�� boxid�� �Է��ϼ���");
                }
                else
                {
                    if (boxingYn)
                    {
                        Util.AlertInfo("���� ���� �۾��� �Ϸ� ���� �ʾҽ��ϴ�.");

                        txtBoxId.Text = boxing_lot;
                        return;
                    }

                    //�Էµ� boxid validation
                    boxValidation();                   
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private void setBoxCnt(int max_cnt, int lot_cnt, int lotCountReverse, string boxing_stat)
        {
            if(txtcnt == null)
            {
                return;
            }

            txtcnt.Text = ObjectDic.Instance.GetObjectName(boxing_stat) + " : " + lot_cnt.ToString() + " / " + max_cnt.ToString();
            tbCount.Text = lotCountReverse.ToString();
        }

        //CHECK BOX(chkBoxId ) �� üũ �ư� LOTID �Է�(KEYIN, SCAN)�� BOXID �����ϴ� �Լ�
        private void autoBoxIdCreate()
        {
            try
            {
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

                DataRow dr = RQSTDT.NewRow();
                dr["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                dr["LANGID"] = LoginInfo.LANGID;
                dr["LOTID"] = txtLotId.Text;
                dr["PROCID"] = lot_proc; // CMA:P5500, BMA:P9500
                dr["PRODID"] = lot_prod; 
                dr["LOTQTY"] = nbBoxingCnt.Value.ToString(); ; // ȭ�鿡�� ������ ����....���߿� ����� ���� ��� �������� �����.         
                dr["EQSGID"] = lot_eqsgid;
                dr["USERID"] = LoginInfo.USERID;

                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("BR_PRD_GET_BOXIDREQUEST", "INDATA", "OUTDATA", RQSTDT);

                if (dtResult != null && dtResult.Rows.Count != 0)
                {
                    txtBoxId.Text = dtResult.Rows[0][0].ToString();
                    boxing_lot = txtBoxId.Text.ToString();
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private bool lotValidation()
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("SRCTYPE", typeof(string));  //INPUT TYPE (UI OR EQ)         
                RQSTDT.Columns.Add("LANGID", typeof(string));   //LANGUAGE ID         
                RQSTDT.Columns.Add("LOTID", typeof(string));    //����LOT(ó�� LOT)               

                DataRow dr = RQSTDT.NewRow();
                dr["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                dr["LANGID"] = LoginInfo.LANGID;
                dr["LOTID"] = Util.GetCondition(txtLotId);

                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("BR_PRD_CHK_BOXLOT", "INDATA", "OUTDATA", RQSTDT);

                //LOT TABLE�� WIP TABLE�� PROD ��
                if (dtResult.Rows.Count > 0)
                {
                    lot_proc = string.Empty;

                    DataTable dtLOTINFO = new DataTable();
                    dtLOTINFO.TableName = "RQSTDT";
                    dtLOTINFO.Columns.Add("LOTID", typeof(string));    //����LOT(ó�� LOT)               

                    DataRow drLOTINFO = dtLOTINFO.NewRow();
                    drLOTINFO["LOTID"] = Util.GetCondition(txtLotId);

                    dtLOTINFO.Rows.Add(drLOTINFO);

                    DataTable dtLOTINFOResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_LOTWIP_INFO", "INDATA", "OUTDATA", RQSTDT);

                    if (dtLOTINFOResult.Rows.Count > 0)
                    {
                        string lot_class = dtLOTINFOResult.Rows[0]["CLASS"].ToString();
                        string lot_prodtype = dtLOTINFOResult.Rows[0]["PRODTYPE"].ToString();
                        string lot_wiphold = dtLOTINFOResult.Rows[0]["WIPHOLD"].ToString();
                        string lot_wipstat = dtLOTINFOResult.Rows[0]["WIPSTAT"].ToString();
                        string lot_procid = dtLOTINFOResult.Rows[0]["PROCID"].ToString();
                        string lot_prodid = dtLOTINFOResult.Rows[0]["PRODID"].ToString();
                        string lot_eqsg = dtLOTINFOResult.Rows[0]["EQSGID"].ToString();
                        string lot_route = dtLOTINFOResult.Rows[0]["ROUTID"].ToString();

                        if (lot_wipstat == "TERM") // ���� lot���� üũ
                        {
                            Util.AlertInfo("���� LOT�Դϴ�.");
                            return false;
                        }

                        if (lot_wiphold == "Y") //hold �������� üũ
                        {
                            Util.AlertInfo("HOLD LOT�Դϴ�.");
                            return false;
                        }

                        if (lot_class_old != null && lot_class_old != "") //���� ���� ��ǰ Ÿ�԰� ������ ��
                        {
                            if (lot_class_old != lot_class)
                            {
                                Util.AlertInfo("������ ������ LOT�� ��ǰ Ÿ�԰� �ٸ��ϴ�.");
                                return false;
                            }
                        }

                        if (lot_class == "CMA") //��ǰŸ�Ժ� ���� ���� ���� üũ
                        {
                            if (lot_procid == "P5000" || lot_procid == "P5500" || lot_procid == "P5200" || lot_procid == "P5400")
                            {

                            }
                            else
                            {
                                Util.AlertInfo("���� �Ұ����� �����Դϴ�.");
                                return false;
                            }
                        }
                        else if (lot_class == "BMA")
                        {
                            if (lot_procid == "P9000" || lot_procid == "P9500")
                            {

                            }
                            else
                            {
                                Util.AlertInfo("���� �Ұ����� �����Դϴ�.");
                                return false;
                            }
                        }
                        else
                        {
                            Util.AlertInfo("���尡���� ��ǰŸ���� �ƴմϴ�.");
                            return false;
                        }

                        if (lot_prod != null && lot_prod != "") //���� ���� lot�� ��ǰ�� ������ ��
                        {
                            if (lot_prod != lot_prodid)
                            {
                                Util.AlertInfo("������ ������ LOT�� ��ǰ�� �ٸ��ϴ�.");
                                return false;
                            }
                        }

                        if (lot_eqsgid != null && lot_eqsgid != "") //���� ���� lot�� ���ΰ� ������ ��
                        {
                            if (lot_eqsgid != lot_eqsg)
                            {
                                Util.AlertInfo("������ ������ LOT�� ���ΰ� �ٸ��ϴ�.");
                                return false;
                            }
                        }

                        //��������� ���� id ã��
                        DataTable dtPROC = new DataTable();
                        dtPROC.TableName = "RQSTDT";
                        dtPROC.Columns.Add("ROUTID", typeof(string));
                        //dtPROC.Columns.Add("PROCTYPE", typeof(string));

                        DataRow drPROC = dtPROC.NewRow();
                        drPROC["ROUTID"] = lot_route;
                        //drPROC["PROCTYPE"] = "B"; //������� Ÿ��

                        dtPROC.Rows.Add(drPROC);

                        DataTable dtdtPROCResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_ENDPROCID_PACK", "INDATA", "OUTDATA", dtPROC);

                        if (dtdtPROCResult == null || dtdtPROCResult.Rows.Count == 0)
                        {
                            Util.AlertInfo("��������� ã���� �����ϴ�.");
                            return false;
                        }

                        lot_proc = dtdtPROCResult.Rows[0]["PROCID"].ToString();
                        lot_prod = lot_prodid;
                        lot_eqsgid = lot_eqsg;
                        lot_class_old = lot_class;

                        txtBoxInfo.Text = lot_class_old + " : " + lot_eqsgid + " : " + lot_prodid + " : " + lot_proc;

                        return true;
                    }
                    else
                    {
                        Util.AlertInfo("LOT�� ���� ���� ������ ���� �ʾ� ������ �� �����ϴ�.");
                        return false;
                    }
                }
                else
                {
                    Util.AlertInfo("���� �Ұ����� LOT�Դϴ�.");
                    return false;
                }
            }
            catch (Exception ex)
            {
                throw ex;
                //LGC.GMES.MES.ControlsLibrary.MessageBox.Show(ex.Message, null, "Error", MessageBoxButton.OK, ControlsLibrary.MessageBoxIcon.Warning);
            }
        }

        private bool lotValidation_BR()
        {
            try
            {
                //lot_proc = string.Empty;

                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("SRCTYPE", typeof(string));    //����LOT(ó�� LOT)       
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("BOXID", typeof(string));
                RQSTDT.Columns.Add("BOXTYPE", typeof(string));
                RQSTDT.Columns.Add("BOX_PRODID", typeof(string));
                RQSTDT.Columns.Add("PRDT_CLSS", typeof(string));
                RQSTDT.Columns.Add("EQSGID", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                dr["LANGID"] = LoginInfo.LANGID;
                dr["BOXID"] = Util.GetCondition(txtLotId); //LOTID
                dr["BOXTYPE"] = "LOT";
                dr["BOX_PRODID"] = lot_prod == "" ? null : lot_prod;
                dr["PRDT_CLSS"] = lot_class_old == "" ? null : lot_class_old;
                dr["EQSGID"] = lot_eqsgid == "" ? null : lot_eqsgid;

                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("BR_PRD_CHK_BOXLOT_PALLET", "INDATA", "OUTDATA", RQSTDT);

                //LOT TABLE�� WIP TABLE�� PROD ��
                if (dtResult.Rows.Count > 0)
                {
                    string lot_class = dtResult.Rows[0]["CLASS"].ToString();
                    string lot_procid = dtResult.Rows[0]["PROCID"].ToString();
                    string lot_prodid = dtResult.Rows[0]["PRODID"].ToString();
                    string lot_eqsg = dtResult.Rows[0]["EQSGID"].ToString();

                    if (getModel(lot_prodid) == "315H" || getModel(lot_prodid) == "MOKA")
                    {
                        set315H(txtLotId.Text, "LOT"); //315H ���� ������ ȭ��(tab 313H CMA) ���� �۾�.

                        txtLotId.Text = "";
                        return false;
                    }

                    lot_proc = lot_procid;
                    lot_prod = lot_prodid;
                    lot_eqsgid = lot_eqsg;
                    lot_class_old = lot_class;

                    txtBoxInfo.Text = lot_class_old + " : " + lot_eqsgid + " : " + lot_prodid;

                    return true;
                }
                else
                {
                    Util.AlertInfo("LOT�� ���� ���� ������ ���� �ʾ� ������ �� �����ϴ�.");
                    return false;
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private void boxValidation()
        {
            //���� ���� �۾� ����
            if (boxingYn)
            {
                Util.AlertInfo("���� ���� �۾��� �Ϸ� ���� �ʾҽ��ϴ�.");
                return;
            }

            //�Էµ� boxid ����
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("BOXID", typeof(string));     //       
                RQSTDT.Columns.Add("BOXTYPE", typeof(string));   //                                 

                DataRow dr = RQSTDT.NewRow();
                dr["BOXID"] =  txtBoxId.Text;
                dr["BOXTYPE"] = "BOX"; //"BOX";                

                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_BOXID", "INDATA", "OUTDATA", RQSTDT);

                if (dtResult == null || dtResult.Rows.Count == 0)
                {
                    Util.AlertInfo("�Է��� BOX�� �������� �ʽ��ϴ�.");
                    return;
                }

                box_prod = dtResult.Rows[0]["PRODID"].ToString(); //box�� prod ��Ƶ�.
                box_eqsg = dtResult.Rows[0]["PACK_EQSGID"] == null ? LoginInfo.CFG_EQSG_ID : dtResult.Rows[0]["PACK_EQSGID"].ToString(); //box�� eqsgid ��Ƶ�.

                if (getModel(box_prod) == "315H" || getModel(box_prod) == "MOKA")
                {
                    if(seleted_palletID.Length == 0)
                    {
                        set315H(txtBoxId.Text, "BOX"); //315H ���� ������ ȭ��(tab 313H CMA) ���� �۾�.

                        txtBoxId.Text = "";
                    }
                }

                if (btnPack.Content.ToString() == ObjectDic.Instance.GetObjectName("�����۾���ȯ")) //�����丮 Ŭ���ؼ� �� ���
                {
                    foreach (DataRow drw in dtResult.Rows)
                    {
                        if (drw["BOXSTAT"].ToString() == "PACKED" && drw["BOXTYPE"].ToString() == "BOX") //if (drw["BOXSTAT"].ToString() == "PACKED") //BOXSTAT ������ ���� �Ǹ� ���� �ʿ�.
                        {
                            chkBoxId.IsChecked = true;

                            DataTable RQSTDT1 = new DataTable();
                            RQSTDT1.TableName = "RQSTDT";
                            RQSTDT1.Columns.Add("BOXID", typeof(string));                                                                                                 

                            DataRow dr1 = RQSTDT1.NewRow();
                            dr1["BOXID"] = txtBoxId.Text;

                            RQSTDT1.Rows.Add(dr1);

                            DataTable dtBoxLots = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_BOXLOTS_SEARCH", "INDATA", "OUTDATA", RQSTDT1);

                            txtcnt.Visibility = Visibility.Hidden;
                            btnUnPack.Visibility = Visibility.Visible;
                            btnSelectCacel.Visibility = Visibility.Hidden;
                            btncancel.Visibility = Visibility.Hidden;
                            txtLotId.IsEnabled = false;
                            chkBoxId.IsChecked = false;
                            chkBoxId.Visibility = Visibility.Hidden;
                                             
                            dgBoxLot.ItemsSource = null;
                            dgBoxLot.ItemsSource = DataTableConverter.Convert(dtBoxLots);
                            Util.SetTextBlockText_DataGridRowCount(tbBoxingWait_cnt, Util.NVC(dtBoxLots.Rows.Count));

                            //(dgBoxLot.Columns["CHK"] as C1.WPF.DataGrid.DataGridCheckBoxColumn).MaxWidth = 0;

                        }
                    }
                }
                else if (btnPack.Content.ToString() != ObjectDic.Instance.GetObjectName("�����۾���ȯ"))
                {
                    foreach (DataRow drw in dtResult.Rows)
                    {
                        if (drw["BOXSTAT"].ToString() == "PACKED") //if (drw["BOXSTAT"].ToString() == "PACKED") //BOXSTAT ������ ���� �Ǹ� ���� �ʿ�.
                        {
                            Util.AlertInfo("�۾��Ұ�! �̹������BOX�Դϴ�.");
                            return;
                        }
                    }

                    boxingYn = true; 
                    boxing_lot = txtBoxId.Text.ToString();

                    //boxing ���� ���� ���� �ʿ�
                    setBoxLotCnt();
                    boxLotmax_cnt = Convert.ToInt32(nbBoxingCnt.Value);
                    lotCountReverse = boxLotmax_cnt;
                    
                }                

                setBoxCnt(boxLotmax_cnt, boxingLot_idx, lotCountReverse, "������");
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private string getModel(string prod)
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("PRODID", typeof(string));                                

                DataRow dr = RQSTDT.NewRow();
                dr["PRODID"] = prod;

                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_MODEL", "INDATA", "OUTDATA", RQSTDT);

                if(dtResult.Rows.Count > 0)
                {
                    return dtResult.Rows[0][0].ToString();
                }
                else
                {
                    return "";
                }           
            }
            catch(Exception ex)
            {
                throw ex;
            }
        }

        private bool boxValidation_accept(string boxid)
        {            
            //�Էµ� boxid ����
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("BOXID", typeof(string));     //       
                RQSTDT.Columns.Add("BOXTYPE", typeof(string));   //                                 

                DataRow dr = RQSTDT.NewRow();
                dr["BOXID"] = boxid;
                dr["BOXTYPE"] = "BOX"; //"BOX";                

                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_BOXID", "INDATA", "OUTDATA", RQSTDT);

                if (dtResult == null || dtResult.Rows.Count == 0)
                {
                    Util.AlertInfo("������ BOX�� �������� �ʽ��ϴ�.");
                    return false;
                }
               
                foreach (DataRow drw in dtResult.Rows)
                {
                    if (drw["BOXSTAT"].ToString() == "CREATED" && drw["BOXTYPE"].ToString() == "BOX") 
                    {
                        Util.AlertInfo("�̹� ���������� BOX�Դϴ�.");
                        return false;
                    }
                }

                return true;                
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private bool gridDistinctCheck(string gubun)
        {
            if (gubun == "box")
            {
                return false;
            }
            else
            {
                DataRowView rowview = null;

                foreach (C1.WPF.DataGrid.DataGridRow row in dgBoxLot.Rows)
                {

                    if (row.DataItem != null)
                    {
                        rowview = row.DataItem as DataRowView;

                        if (rowview["LOTID"].ToString() == txtLotId.Text.ToString())
                        {
                            Util.AlertInfo("�̹� ���� ����Ʈ�� �ִ� LOTID�Դϴ�.");
                            return false;
                        }
                    }
                }
                return true;
            }
        }

        private void addGridLot()
        {
            if (boxingLot_idx == 0)
            {
                DataTable dtBoxLot = new DataTable();
                dtBoxLot.Columns.Add("CHK", typeof(bool));
                dtBoxLot.Columns.Add("BOXID", typeof(string));
                dtBoxLot.Columns.Add("LOTID", typeof(string));

                DataRow dr = dtBoxLot.NewRow();
                dr["CHK"] = false;
                dr["BOXID"] = Util.GetCondition(txtBoxId);
                dr["LOTID"] = Util.GetCondition(txtLotId);

                dtBoxLot.Rows.Add(dr);

                dgBoxLot.ItemsSource = DataTableConverter.Convert(dtBoxLot);

                boxingYn = true;
            }
            else
            {
                int TotalRow = dgBoxLot.GetRowCount(); //�������

                dgBoxLot.EndNewRow(true);
                DataGridRowAdd(dgBoxLot);

                DataTableConverter.SetValue(dgBoxLot.Rows[TotalRow].DataItem, "CHK", "false");
                DataTableConverter.SetValue(dgBoxLot.Rows[TotalRow].DataItem, "BOXID", Util.GetCondition(txtBoxId));
                DataTableConverter.SetValue(dgBoxLot.Rows[TotalRow].DataItem, "LOTID", Util.GetCondition(txtLotId));
            }
            boxingLot_idx++;
            lotCountReverse--;

            DataTable dt = DataTableConverter.Convert(dgBoxLot.ItemsSource);

            Util.SetTextBlockText_DataGridRowCount(tbBoxingWait_cnt, Util.NVC(dt.Rows.Count));
        }

        private void DataGridRowAdd(C1.WPF.DataGrid.C1DataGrid dg)
        {
            dg.BeginNewRow();
            dg.EndNewRow(true);
        }

        private void boxingEnd()
        {
            try
            {
                string boxID = Util.GetCondition(txtBoxId);
                int lot_total_qty;
                string rebox_lotbox_ckh = string.Empty;
                string gubun = string.Empty; //box�������� lot�������� ����

                string eqsg = string.Empty;
                string proc = string.Empty;

                if (reBoxing) //������
                {
                    eqsg = seleted_Box_Eqsgid;
                    proc = seleted_Box_Procid;
                }
                else//��������
                {
                    eqsg = lot_eqsgid;
                    proc = lot_proc;
                }

                lot_total_qty = dgBoxLot.GetRowCount();

                DataSet indataSet = new DataSet();

                DataTable INDATA = indataSet.Tables.Add("INDATA");
                INDATA.Columns.Add("SRCTYPE", typeof(string));  //INPUT TYPE (UI OR EQ)         
                INDATA.Columns.Add("LANGID", typeof(string));   //LANGUAGE ID         
                INDATA.Columns.Add("BOXID", typeof(string));    //����LOT(ó�� LOT)
                INDATA.Columns.Add("PROCID", typeof(string));   //����ID(������ ������ ����) 
                INDATA.Columns.Add("BOXQTY", typeof(string));   //���� �Ѽ���
                INDATA.Columns.Add("EQSGID", typeof(string));   //����ID
                INDATA.Columns.Add("USERID", typeof(string));   //�����ID

                DataRow dr = INDATA.NewRow();
                dr["SRCTYPE"] = "UI";
                dr["LANGID"] = LoginInfo.LANGID;
                dr["BOXID"] = boxID;
                dr["PROCID"] = proc;
                dr["BOXQTY"] = lot_total_qty;
                dr["EQSGID"] = eqsg;
                dr["USERID"] = LoginInfo.USERID;
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

                DataSet dsResult = new ClientProxy().ExecuteServiceSync_Multi("BR_PRD_REG_BOXING", "INDATA,IN_LOTID", "OUTDATA,OUT_LOTID", indataSet);

                if(dsResult != null && dsResult.Tables["OUTDATA"].Rows.Count >0)
                {
                    //LGC.GMES.MES.ControlsLibrary.MessageBox.Show("������ �Ϸ� �Ǿ����ϴ�.", null, "�Ϸ�", MessageBoxButton.OK, ControlsLibrary.MessageBoxIcon.None);
                }
                else
                {
                    throw new Exception("���� �۾� ���� BOXING BIZ Ȯ�� �ϼ���.");
                }               
            }
            catch (Exception ex)
            {
                Util.AlertInfo(ex.Message);
            }
        }

        private void labelTest()
        {
            try
            {
                //x,y ��������
                DataTable dt = LoginInfo.CFG_SERIAL_PRINT;

                string startX = "0";
                string startY = "0";
                if (dt.Rows.Count > 0)
                {
                    startX = dt.Rows[0]["X"].ToString();
                    startY = dt.Rows[0]["Y"].ToString();
                }

                DataTable dtRqst = new DataTable();
                dtRqst.Columns.Add("LBCD", typeof(string));
                dtRqst.Columns.Add("PRMK", typeof(string));
                dtRqst.Columns.Add("RESO", typeof(string));
                dtRqst.Columns.Add("PRCN", typeof(string));
                dtRqst.Columns.Add("MARH", typeof(string));
                dtRqst.Columns.Add("MARV", typeof(string));
                dtRqst.Columns.Add("ATTVAL001", typeof(string));
                dtRqst.Columns.Add("ATTVAL002", typeof(string));
                dtRqst.Columns.Add("ATTVAL003", typeof(string));
                dtRqst.Columns.Add("ATTVAL004", typeof(string));
                dtRqst.Columns.Add("ATTVAL005", typeof(string));
                dtRqst.Columns.Add("ATTVAL006", typeof(string));
                dtRqst.Columns.Add("ATTVAL007", typeof(string));
                dtRqst.Columns.Add("ATTVAL008", typeof(string));

                DataRow dr = dtRqst.NewRow();
                dtRqst.Rows.Add(dr);

                //for (int i = 0; i < inData.Rows.Count; i++)
                //{

                dtRqst.Rows[0]["LBCD"] = "LBL0017";
                dtRqst.Rows[0]["PRMK"] = "Z";
                dtRqst.Rows[0]["RESO"] = "203";
                dtRqst.Rows[0]["PRCN"] = "1";
                dtRqst.Rows[0]["MARH"] = startX;
                dtRqst.Rows[0]["MARV"] = startY;               
                dtRqst.Rows[0]["ATTVAL001"] = "REF : 295B93949R__C";//  inData.Rows[i]["MODELID"].ToString();
                dtRqst.Rows[0]["ATTVAL002"] = "966Wh";//  inData.Rows[i]["LOTID"].ToString();
                dtRqst.Rows[0]["ATTVAL003"] = "LOT0000001";//  inData.Rows[i]["WIPQTY"].ToString();
                dtRqst.Rows[0]["ATTVAL004"] = "11111111";//  inData.Rows[i]["RESNNAME"].ToString();
                dtRqst.Rows[0]["ATTVAL005"] = DateTime.Now.ToString("yyyy.MM.dd");
                dtRqst.Rows[0]["ATTVAL006"] = "";//  dtExpected.SelectedDateTime.ToString("yyyy.MM.dd");
                dtRqst.Rows[0]["ATTVAL007"] = "";//  inData.Rows[i]["PERSON"].ToString();
                dtRqst.Rows[0]["ATTVAL008"] = "";

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_COM_SEL_LABEL_DESIGN_CODE_ITEM_COMMON", "INDATA", "OUTDATA", dtRqst);

                    try
                    {
                        CMM_ZPL_VIEWER2 wndPopup = new CMM_ZPL_VIEWER2(dtRslt.Rows[0]["LABELCD"].ToString());
                        wndPopup.Show();
                    }
                    catch (Exception ex)
                    {
                        LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
                    }
                //}
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private void labelPrint(object sender)
        {
            try
            {
                string print_cnt = string.Empty;
                string print_yn = string.Empty;
                string prodID = string.Empty;
                string boxID = string.Empty;
                DataTable dtzpl;

                //����� �ű� ���� ���� : PRODID �����ؼ� �ѷ���.
                if (rePrint) //�����
                {
                    prodID = unPack_ProdID;
                    boxID = txtBoxIdR.Text;
                }
                else
                {
                    prodID = "";
                    boxID = txtBoxId.Text;
                }

                //zpl ��������
                //string sLOTID, string sPROCID, string sEQPTID, string sEQSGID, string sLABEL_TYPE, string sLABEL_CODE, string sSAMPLE_FLAG, string sPRN_QTY,string sPRODID
                //dtzpl = Util.getZPL_Pack(boxID, null, null, null, "PACK_INBOX", "LBL0020", "N", "1", prodID, null);

                dtzpl = Util.getZPL_Pack(sLOTID: boxID
                                        , sLABEL_TYPE: LABEL_TYPE_CODE.PACK_INBOX
                                        , sLABEL_CODE: "LBL0020"
                                        , sSAMPLE_FLAG: "N"
                                        , sPRN_QTY: "1"
                                        , sPRODID: prodID
                                        );

                if (dtzpl == null || dtzpl.Rows.Count == 0)
                {
                    return;
                }

                string zpl = Util.NVC(dtzpl.Rows[0]["ZPLSTRING"]);
                //�� ����
                Util.PrintLabel(FrameOperation, loadingIndicator, zpl);

                CMM_ZPL_VIEWER2 wndPopup = new CMM_ZPL_VIEWER2(zpl);
                wndPopup.Show();

                //Util.printLabel_Pack(FrameOperation, loadingIndicator, txtPalleyIdR.Text, "PACK", "N", "1");

                if(!rePrint)
                {
                    return;
                }

                //����� �� ��� ó�� : ���� ���� ���� Ȯ��
                DataTable dtBoxPrintHistory = setBoxResultList();

                if (dtBoxPrintHistory == null || dtBoxPrintHistory.Rows.Count == 0)
                {
                    return;
                }

                print_cnt = dtBoxPrintHistory.Rows[0]["PRT_REQ_SEQNO"].ToString();
                print_yn = dtBoxPrintHistory.Rows[0]["PRT_FLAG"].ToString();

                if (dtBoxPrintHistory.Rows[0]["PRT_FLAG"].ToString() == "N")//print ���� N�ΰ�� Y�� update
                {
                    updateTB_SFC_LABEL_PRT_REQ_HIST(print_yn, print_cnt);
                }

                //Button btn = sender as Button;

                //if (btn.Name == "btnBoxLabel") //�����
                //{
                //    DataTable dtWipList = getWipList();
                //    string lotId = Util.GetCondition(txtBoxIdR);

                //    Util.printLabel_Pack(FrameOperation, loadingIndicator, lotId, "PROC", "N", "1");

                //    updateTB_SFC_LABEL_PRT_REQ_HIST(lotId, dtWipList.Rows[0]["BOXSEQ"].ToString());

                //    showLabelPrintInfoPopup(lotId, lotId);
                //}
                //else
                //{
                //    string lotId = Util.GetCondition(txtBoxId);

                //    Util.printLabel_Pack(FrameOperation, loadingIndicator, lotId, "PROC", "N", "1"); //�� ���

                //    if (btn.Name == "btnPack" && btn.Content.ToString() == "����")
                //    {
                //        DataTable dtWipList = getWipList();
                //        updateTB_SFC_LABEL_PRT_REQ_HIST(lotId, dtWipList.Rows[0]["BOXSEQ"].ToString()); //�̷����̺� update

                //        showLabelPrintInfoPopup(lotId, lotId);
                //    }

                //}


                /* REAL ����
                                string print_cnt = string.Empty;
                                string print_yn = string.Empty;

                                Util.printLabel_Pack(FrameOperation, loadingIndicator, txtBoxIdR.Text, "PACK", "N", "1");

                                //����� �� ��� ó�� : ���� ���� ���� Ȯ��
                                DataTable dtBoxPrintHistory = setBoxResultList();

                                if (dtBoxPrintHistory == null || dtBoxPrintHistory.Rows.Count == 0)
                                {
                                    return;
                                }

                                print_cnt = dtBoxPrintHistory.Rows[0]["PRT_REQ_SEQNO"].ToString();
                                print_yn = dtBoxPrintHistory.Rows[0]["PRT_FLAG"].ToString();

                                if (dtBoxPrintHistory.Rows[0]["PRT_FLAG"].ToString() == "N")//print ���� N�ΰ�� Y�� update
                                {
                                    updateTB_SFC_LABEL_PRT_REQ_HIST(print_yn, print_cnt);
                                }
                  */

                //�׽�Ʈ ����
                //DataTable dtResult = getZPL_Pack(txtBoxIdR.Text, null, null, null, "", "", "N", "1");
                //DataTable dtResult = getZPL_Pack(null, null, null, null, "PACK", "LBL0017", "N", "1"); //AMDAU0068A

                //for (int i = 0; i < dtResult.Rows.Count; i++)
                //{
                //    string zpl = Util.NVC(dtResult.Rows[i]["ZPLSTRING"]);
                //    CMM_ZPL_VIEWER2 wndPopup = new CMM_ZPL_VIEWER2(zpl);
                //    wndPopup.Show();

                //    Util.PrintLabel(FrameOperation, loadingIndicator, zpl);
                //}
            }
            catch (Exception ex)
            {
                throw ex;
            }         
        }

        public static DataTable getZPL_Pack(string sBOXID, string sPROCID, string sEQPTID, string sEQSGID, string sLABEL_TYPE, string sLABEL_CODE, string sSAMPLE_FLAG, string sPRN_QTY)
        {
            DataTable dtResult = null;
            try
            {
                DataTable INDATA = new DataTable();
                INDATA.TableName = "INDATA";
                INDATA.Columns.Add("SRCTYPE", typeof(string));
                INDATA.Columns.Add("LANGID", typeof(string));
                INDATA.Columns.Add("BOXID", typeof(string));
                INDATA.Columns.Add("PROCID", typeof(string));
                INDATA.Columns.Add("EQPTID", typeof(string));
                INDATA.Columns.Add("EQSGID", typeof(string));
                INDATA.Columns.Add("LABEL_TYPE", typeof(string));
                INDATA.Columns.Add("LABEL_CODE", typeof(string));
                INDATA.Columns.Add("PRN_QTY", typeof(string));
                INDATA.Columns.Add("SAMPLE_FLAG", typeof(string));


                DataRow dr = INDATA.NewRow();
                dr["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                dr["LANGID"] = LoginInfo.LANGID;
                dr["BOXID"] = sBOXID;
                dr["PROCID"] = sPROCID;
                dr["EQPTID"] = sEQPTID;
                dr["EQSGID"] = sEQSGID;
                dr["LABEL_TYPE"] = sLABEL_TYPE;
                dr["LABEL_CODE"] = sLABEL_CODE;
                dr["PRN_QTY"] = sPRN_QTY;
                dr["SAMPLE_FLAG"] = sSAMPLE_FLAG;

                INDATA.Rows.Add(dr);

                dtResult = new ClientProxy().ExecuteServiceSync("BR_PRD_GET_ZPL_BOX", "INDATA", "OUTDATA", INDATA);
                //new ClientProxy().ExecuteServiceSync("BR_PRD_GET_ZPL_BOX", "INDATA", "OUTDATA", INDATA);
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return dtResult;
        }

        private void control_Init()
        {
            if (!boxingYn) //����Ϸ� �� ���
            {
                txtBoxId.Text = "";
                txtLotId.Text = "";
                boxing_lot = "";

                lot_prod = "";
                lot_proc = "";
                lot_eqsgid = "";
                lot_class_old = "";
                txtBoxInfo.Text = "";

                box_prod = "";

                boxingLot_idx = 0;
                btnPack.Tag = ObjectDic.Instance.GetObjectName("�ű�");
                nbBoxingCnt.Value = 5;
                lotCountReverse = 5;

                tbBoxingWait_cnt.Text = "[ 0 " + ObjectDic.Instance.GetObjectName("��") + " ]";

                chkBoxId.IsChecked = false;
                txtBoxId.IsReadOnly = false;
                //boxLotmax_cnt = 0;
                Util.gridClear(dgBoxLot); //�׸��� clear
            }
        }

        private void boxingStatInit()
        {
            boxingYn = false; //��⳪ �Ϸ� ���·�.
            boxingLot_idx = 0;
            boxLotmax_cnt = 5; // boxid�� ���� ���� ��� ����. ����� ������ �Ǿ� �־ �׽�Ʈ������ 5�� ����
        }

        private void pack_unPack_init(object sender)
        {
            try
            {
                Button btn = sender as Button;
                string unpack_boxid = string.Empty;

                string boxid = Util.GetCondition(txtBoxId);               
                string prod = string.Empty;
                string eqsg = string.Empty;
                string eqpt = string.Empty;
                string proc = string.Empty;

                if (btn.Name == "btnUnPack") //�������� ���� ��������
                {
                    unpack_boxid = Util.GetCondition(txtBoxId);

                    prod = seleted_Box_Prod;
                    eqsg = seleted_Box_Eqsgid;
                    eqpt = seleted_Box_Eqptid;
                    proc = seleted_Box_Procid;
                }
                else // ����Ʈ���� ��������
                {
                    unpack_boxid = Util.GetCondition(txtBoxIdR);
                    prod = unPack_ProdID;
                    eqsg = unPack_EqsgID;
                    eqpt = unPack_EqptID;
                    proc = unPack_ProcID;
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

                DataRow dr = INDATA.NewRow();              
                dr["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                dr["BOXID"] = unpack_boxid;
                dr["PRODID"] = prod;
                dr["PACK_LOT_TYPE_CODE"] = "LOT";
                dr["UNPACK_QTY"] = DataTableConverter.GetValue(dgBoxhistory.Rows[dgBoxhistory.CurrentRow.Index].DataItem, "BOXLOTCNT");
                dr["UNPACK_QTY2"] = DataTableConverter.GetValue(dgBoxhistory.Rows[dgBoxhistory.CurrentRow.Index].DataItem, "BOXLOTCNT"); ;
                dr["USERID"] = LoginInfo.USERID;
                dr["ERP_IF_FLAG"] = "C";
                dr["NOTE"] = "BOX UNPACK";
                INDATA.Rows.Add(dr);

                //DataTable dsResult = new ClientProxy().ExecuteServiceSync("DA_BAS_UPD_BOX_UNPACK", "INDATA", "OUTDATA", INDATA);
                DataTable dsResult = new ClientProxy().ExecuteServiceSync("BR_PRD_REG_UNPACK_BOX", "INDATA", "OUTDATA", INDATA);

                if (dsResult != null && dsResult.Rows.Count > 0)
                {
                    if (btn.Name == "btnUnPack") //������ø� ó��
                    {
                        txtcnt.Visibility = Visibility.Visible;
                        btnUnPack.Visibility = Visibility.Hidden;

                        btnSelectCacel.Visibility = Visibility.Visible;
                        btncancel.Visibility = Visibility.Visible;
                        btnPack.Content = ObjectDic.Instance.GetObjectName("����");

                        boxingLot_idx = 0;
                        boxLotmax_cnt = Convert.ToInt32(nbBoxingCnt.Value);
                        lotCountReverse = boxLotmax_cnt;

                        //dgBoxLot.ItemsSource = null;
                        txtLotId.IsEnabled = true;
                        (dgBoxLot.Columns["CHK"] as C1.WPF.DataGrid.DataGridCheckBoxColumn).MaxWidth = 40;

                        boxingYn = true;

                        setBoxCnt(boxLotmax_cnt, boxingLot_idx, lotCountReverse, "������");

                        Util.AlertInfo("������ ���� �Ǿ� ������ �����մϴ�.");
                    }
                    else
                    {
                        Util.AlertInfo("������ ���� �Ǿ����ϴ�.");
                    }
                }
                else
                {
                    throw new Exception();
                }
            }
            catch(Exception ex)
            {
                throw ex;
            }
        }

        private void PopUpOpen(string sMAINFORMPATH, string sMAINFORMNAME)
        {
            Assembly asm = Assembly.LoadFrom("ClientBin\\" + sMAINFORMPATH + ".dll");
            Type targetType = asm.GetType(sMAINFORMPATH + "." + sMAINFORMNAME);
            object obj = Activator.CreateInstance(targetType);

            IWorkArea workArea = obj as IWorkArea;
            workArea.FrameOperation = FrameOperation;

            C1Window popup = obj as C1Window;
            if (popup != null)
            {
                popup.Closed -= popup_Closed;
                popup.Closed += popup_Closed;
                popup.ShowModal();
                popup.CenterOnScreen();
            }
        }

        private DataTable getWipList()
        {
            try
            {
                //DA_PRD_SEL_WIP_PACK_ROUTE
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("EQSGID", typeof(string));
                RQSTDT.Columns.Add("PROCID", typeof(string));
                RQSTDT.Columns.Add("EQPTID", typeof(string));
                RQSTDT.Columns.Add("TOPCNT", typeof(Int32));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["EQSGID"] = LoginInfo.CFG_EQSG_ID;
                dr["PROCID"] = LoginInfo.CFG_PROC_ID;
                dr["EQPTID"] = LoginInfo.CFG_EQPT_ID;
                dr["TOPCNT"] = 10;
                RQSTDT.Rows.Add(dr);

                return new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_TB_SFC_LABEL_PRT_REQ_HIST_BYLOT", "RQSTDT", "RSLTDT", RQSTDT);

            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private void showLabelPrintInfoPopup(string sLotid, string sMLotid)
        {
            try
            {
                PACK001_002_PRINTINFOMATION popup = new PACK001_002_PRINTINFOMATION();
                popup.FrameOperation = this.FrameOperation;

                if (popup != null)
                {
                    DataTable dtData = new DataTable();
                    dtData.Columns.Add("LOTID", typeof(string));
                    dtData.Columns.Add("MLOTID", typeof(string));

                    DataRow newRow = null;

                    newRow = dtData.NewRow();
                    newRow["LOTID"] = sLotid;
                    newRow["MLOTID"] = sMLotid;
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
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// TB_SFC_LABEL_PRT_REQ_HIST 
        /// PRT_FLAG = 'Y' �� UPDATE
        /// </summary>
        /// <param name="sScanid"></param>
        /// <param name="sPRT_SEQ"></param>
        private void updateTB_SFC_LABEL_PRT_REQ_HIST(string sScanid, string sPRT_SEQ)
        {
            try
            {
                //DA_PRD_UPD_TB_SFC_LABEL_PRT_REQ_HIST_PRTFLAG
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("SCAN_ID", typeof(string));
                RQSTDT.Columns.Add("PRT_REQ_SEQNO", typeof(Int32));

                DataRow dr = RQSTDT.NewRow();
                dr["SCAN_ID"] = sScanid;
                dr["PRT_REQ_SEQNO"] = Util.NVC_Int((sPRT_SEQ));
                RQSTDT.Rows.Add(dr);

                new ClientProxy().ExecuteServiceSync("DA_PRD_UPD_TB_SFC_LABEL_PRT_REQ_HIST_PRTFLAG", "RQSTDT", "", RQSTDT);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

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
                dr["BOXID"] = Util.GetCondition(txtBoxIdR) == "" ? null : Util.GetCondition(txtBoxIdR);
                RQSTDT.Rows.Add(dr);

                DataTable dtboxList = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_BOX_LIST_FOR_LABEL1", "RQSTDT", "RSLTDT", RQSTDT);

                return dtboxList;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private void reSet()
        {
            txtcnt.Visibility = Visibility.Visible;
            btnUnPack.Visibility = Visibility.Hidden;
            btnSelectCacel.Visibility = Visibility.Visible;
            btncancel.Visibility = Visibility.Visible;
            btnPack.Content = ObjectDic.Instance.GetObjectName("����");
            btnPack.Tag = ObjectDic.Instance.GetObjectName("�ű�");

            boxingLot_idx = 0;
            nbBoxingCnt.Value = 5;
            boxLotmax_cnt = Convert.ToInt32(nbBoxingCnt.Value);
            lotCountReverse = boxLotmax_cnt;
            tbBoxingWait_cnt.Text = "[ 0 " + ObjectDic.Instance.GetObjectName("��") + " ]";
            

            dgBoxLot.ItemsSource = null;
            (dgBoxLot.Columns["CHK"] as C1.WPF.DataGrid.DataGridCheckBoxColumn).MaxWidth = 40;
            txtLotId.IsEnabled = true;
            chkBoxId.IsChecked = false;
            chkBoxId.Visibility = Visibility.Visible;
            txtBoxId.IsEnabled = true;
            txtBoxId.IsReadOnly = false;

            boxingYn = false;

            setBoxCnt(boxLotmax_cnt, boxingLot_idx, lotCountReverse, "�����");

            txtBoxId.Text = "";
            txtLotId.Text = "";
            txtBoxIdR.Text = "";
                      
            lot_prod = "";
            lot_proc = "";
            lot_eqsgid = "";
            lot_class_old = "";
            txtBoxInfo.Text = "";

            boxing_lot = "";
            box_prod = "";
        }

        private void reSet_Last_Stat()
        {
            txtcnt.Visibility = Visibility.Visible;
            btnUnPack.Visibility = Visibility.Hidden;
            btnSelectCacel.Visibility = Visibility.Visible;
            btncancel.Visibility = Visibility.Visible;
            btnPack.Content = ObjectDic.Instance.GetObjectName("����");
            btnPack.Tag = ObjectDic.Instance.GetObjectName("�ű�");

            boxingLot_idx = 0;
            //nbBoxingCnt.Value = 5;
            boxLotmax_cnt = Convert.ToInt32(nbBoxingCnt.Value);
            lotCountReverse = boxLotmax_cnt;
            tbBoxingWait_cnt.Text = "[ 0 " + ObjectDic.Instance.GetObjectName("��") + " ]";


            dgBoxLot.ItemsSource = null;
            (dgBoxLot.Columns["CHK"] as C1.WPF.DataGrid.DataGridCheckBoxColumn).MaxWidth = 40;
            txtLotId.IsEnabled = true;
            //chkBoxId.IsChecked = false;
            //chkBoxId.Visibility = Visibility.Visible;
            //txtBoxId.IsEnabled = true;
            //txtBoxId.IsReadOnly = false;

            boxingYn = false;

            setBoxCnt(boxLotmax_cnt, boxingLot_idx, lotCountReverse, "�����");

            txtBoxId.Text = "";
            txtLotId.Text = "";
            txtBoxIdR.Text = "";

            lot_prod = "";
            lot_proc = "";
            lot_eqsgid = "";
            lot_class_old = "";
            txtBoxInfo.Text = "";

            boxing_lot = "";
            box_prod = "";
        }

        private void buttonAccess(object sender)
        {
            try
            {
                Button btn = sender as Button;

                string grid_name = "dgBoxhistory";

                C1.WPF.DataGrid.DataGridRow row = new C1.WPF.DataGrid.DataGridRow();
                System.Collections.Generic.IList<System.Windows.FrameworkElement> ilist = btn.GetAllParents();
                foreach (var item in ilist)
                {
                    C1.WPF.DataGrid.DataGridRowPresenter presenter = item as C1.WPF.DataGrid.DataGridRowPresenter;
                    if (presenter != null)
                    {
                        row = presenter.Row;

                        grid_name = presenter.DataGrid.Name;
                    }
                }

                DataRowView drv = row.DataItem as DataRowView;

                seleted_boxid = drv["BOXID"].ToString();

                //�׸��忡�� ���� ��ư Ŭ���ؼ� �Ѿ�� ��� : �̹� ���� ���� �� box �� �� �����Ƿ� Ȯ������.
                if (btn.Name == "btnAccept")
                {
                    if (!boxValidation_accept(seleted_boxid))
                    {
                        return;
                    }
                }

                if (drv["PALLETID"] != null)
                {
                    seleted_palletID = drv["PALLETID"].ToString();
                }

                if (!boxingYn)
                {
                    btnPack.Content = ObjectDic.Instance.GetObjectName("�����۾���ȯ");
                    txtBoxId.Text = seleted_boxid;
                    SetBoxkeyDown();
                }
                else
                {
                    Util.AlertInfo("���� ���� �۾��� �Ϸ� ���� �ʾҽ��ϴ�.");
                }

                

                seleted_Box_Prod = drv["PRODID"].ToString(); //PALLET�� ��ǰ
                seleted_Box_Procid = drv["PROCID"] == null ? null : drv["PROCID"].ToString(); //PALLET�� ����
                seleted_Box_Eqptid = drv["EQPTID"].ToString(); //PALLET�� ����
                seleted_Box_Eqsgid = drv["EQSGID"].ToString(); //PALLET�� ����
                seleted_Box_PrdClass = drv["PRDCLASS"].ToString(); //PALLET�� ����

                boxLotmax_cnt = Convert.ToInt32(drv["BOXLOTCNT"]); //���尡�ɼ���
                boxingLot_idx = Convert.ToInt32(drv["BOXLOTCNT"]); //����� ����

                lotCountReverse = boxLotmax_cnt - boxingLot_idx; //��������
                nbBoxingCnt.Value = boxLotmax_cnt;

                txtBoxInfo.Text = seleted_Box_PrdClass + " : " + seleted_Box_Eqsgid + " : " + seleted_Box_Prod;

                setBoxCnt(boxLotmax_cnt, boxingLot_idx, lotCountReverse, "������");
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private void SearchBox()
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
                RQSTDT.Columns.Add("BOXID", typeof(string));
                RQSTDT.Columns.Add("SYSTEM_ID", typeof(string));
                RQSTDT.Columns.Add("USERID", typeof(string));
                RQSTDT.Columns.Add("SHOPID", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["AREAID"] = ""; //Util.GetCondition(cboArea);
                dr["PRODID"] = ""; // Util.GetCondition(cboProductModel);
                dr["MODLID"] = "";  //Util.GetCondition(cboProduct);
                dr["FROMDATE"] = DateTime.Now.AddYears(-10).ToString("yyyyMMdd");
                dr["TODATE"] = Util.GetCondition(dtpDateTo); //dtpDateTo.SelectedDateTime.ToString();
                dr["BOXID"] = Util.GetCondition(txtSearchBox);
                dr["SYSTEM_ID"] = LoginInfo.SYSID;
                dr["USERID"] = LoginInfo.USERID;
                dr["SHOPID"] = LoginInfo.CFG_SHOP_ID;

                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_BOXHISTORY_SEARCH", "INDATA", "OUTDATA", RQSTDT);

                dgBoxhistory.ItemsSource = null;
                txtBoxIdR.Text = "";

                if (dtResult.Rows.Count != 0)
                {
                    Util.GridSetData(dgBoxhistory, dtResult, FrameOperation);
                }
                Util.SetTextBlockText_DataGridRowCount(tbBoxHistory_cnt, Util.NVC(dtResult.Rows.Count));
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private void toggle_Checked(object sender)
        {
            try
            {
                ToggleButton btn = sender as ToggleButton;

                if (btn == null)
                {
                    return;
                }

                if (btn.Name == "btnExpandFrame2")
                {
                    if(btnExpandFrame3 != null)
                    {
                        btnExpandFrame3.Checked -= btnExpandFrame3_Checked;
                        btnExpandFrame3.IsChecked = true;
                        btnExpandFrame3.Checked += btnExpandFrame3_Checked;
                    }                    
                }
                else
                {
                    btnExpandFrame2.Checked -= btnExpandFrame2_Checked;
                    btnExpandFrame2.IsChecked = true;
                    btnExpandFrame2.Checked += btnExpandFrame2_Checked;
                }

                main_grid.ColumnDefinitions[1].Width = new GridLength(8);
                LGC.GMES.MES.CMM001.GridLengthAnimation gla = new LGC.GMES.MES.CMM001.GridLengthAnimation();
                gla.From = new GridLength(0, GridUnitType.Star);
                gla.To = new GridLength(1, GridUnitType.Star);
                gla.Duration = new TimeSpan(0, 0, 0, 0, 500);
                main_grid.ColumnDefinitions[0].BeginAnimation(ColumnDefinition.WidthProperty, gla);
            }
            catch (Exception ex)
            {
                throw ex;
            }

        }
        private void toggle_Unchecked(object sender)
        {
            try
            {
                ToggleButton btn = sender as ToggleButton;

                if (btn == null)
                {
                    return;
                }

                if (btn.Name == "btnExpandFrame2")
                {
                    btnExpandFrame3.Unchecked -= btnExpandFrame3_Unchecked;
                    btnExpandFrame3.IsChecked = false;
                    btnExpandFrame3.Unchecked += btnExpandFrame3_Unchecked;
                }
                else
                {
                    btnExpandFrame2.Unchecked -= btnExpandFrame2_Unchecked;
                    btnExpandFrame2.IsChecked = false;
                    btnExpandFrame2.Unchecked += btnExpandFrame2_Unchecked;
                }

                main_grid.ColumnDefinitions[1].Width = new GridLength(0);
                LGC.GMES.MES.CMM001.GridLengthAnimation gla = new LGC.GMES.MES.CMM001.GridLengthAnimation();
                gla.From = new GridLength(0, GridUnitType.Star);
                gla.To = new GridLength(0, GridUnitType.Star); ;
                gla.Duration = new TimeSpan(0, 0, 0, 0, 500);
                main_grid.ColumnDefinitions[0].BeginAnimation(ColumnDefinition.WidthProperty, gla);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private void setBoxLotCnt()
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("EQSGID", typeof(string));
                RQSTDT.Columns.Add("SHIPTO_ID", typeof(string));
                RQSTDT.Columns.Add("PRODID", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["EQSGID"] = box_eqsg == "" ? lot_eqsgid : box_eqsg;
                dr["SHIPTO_ID"] = null;
                dr["PRODID"] = box_prod;

                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_BOXTOTAL_LOTTOTAL_CNT_FIND", "INDATA", "OUTDATA", RQSTDT);

                if (dtResult.Rows.Count != 0)
                {
                    if (dtResult.Rows[0]["LOTCNT"] == null || dtResult.Rows[0]["LOTCNT"].ToString() == "")
                    {
                        ms.AlertInfo("BOX�� [���� ����]�� ���� ���� �ʾҽ��ϴ�. Defalut : 5�� ���õ˴ϴ�. \nMMD->����/���� ����(Pack) ���� �������� ����ϼ���."); // �߰� : BOX�� [���� ����]�� ���� ���� �ʾҽ��ϴ�. Defalut : 5�� ���õ˴ϴ�. n\MMD->����/���� ����(Pack) ���� �������� ����ϼ���.
                        nbBoxingCnt.Value = 5;
                    }
                    else
                    {
                        if (Convert.ToInt32(dtResult.Rows[0]["LOTCNT"]) != 0)
                        {
                            nbBoxingCnt.Value = Convert.ToInt32(dtResult.Rows[0]["LOTCNT"]);
                        }
                        else
                        {
                            nbBoxingCnt.Value = 5;
                        }
                    }
                }
                else
                {
                    nbBoxingCnt.Value = 5;
                }
            }
            catch (Exception ex)
            {
                Util.Alert(ex.Message);
            }
        }

        #endregion

        #endregion


        #region 315H CMA

        #region EVENT
        private void btnOutPrint_Click(object sender, RoutedEventArgs e)
        {

        }

        private void txtAdvice_TextChanged(object sender, TextChangedEventArgs e)
        {
            setBacode(sender);
        }

        private void txtBatch_TextChanged(object sender, TextChangedEventArgs e)
        {
            setBacode(sender);
        }

        private void txtpartNumber_TextChanged(object sender, TextChangedEventArgs e)
        {
            setBacode(sender);
        }

        private void txtquantity_TextChanged(object sender, TextChangedEventArgs e)
        {
            setBacode(sender);
        }

        private void txtSerial_TextChanged(object sender, TextChangedEventArgs e)
        {
            setBacode(sender);
        }

        private void txtSupplierID_TextChanged(object sender, TextChangedEventArgs e)
        {
            setBacode(sender);
        }

        private void btnExpandFrame2_Checked(object sender, RoutedEventArgs e)
        {
            try
            {
                toggle_Checked(sender);
            }
            catch (Exception ex)
            {
                Util.AlertInfo(ex.Message);
            }
        }

        private void btnExpandFrame2_Unchecked(object sender, RoutedEventArgs e)
        {
            try
            {
                toggle_Unchecked(sender);
            }
            catch (Exception ex)
            {
                Util.AlertInfo(ex.Message);
            }
        }

        private void btnExpandFrame3_Checked(object sender, RoutedEventArgs e)
        {
            try
            {
                toggle_Checked(sender);
            }
            catch (Exception ex)
            {
                Util.AlertInfo(ex.Message);
            }
        }

        private void btnExpandFrame3_Unchecked(object sender, RoutedEventArgs e)
        {
            try
            {
                toggle_Unchecked(sender);
            }
            catch (Exception ex)
            {
                Util.AlertInfo(ex.Message);
            }
        }

        private void cboProduct_315H_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            combo_prd_315H = Util.GetCondition(cboProduct_315H);

            txtpartNumber.Text = combo_prd_315H;
        }

        private void chkBoxid_315H_Click(object sender, RoutedEventArgs e)
        {
            if ((bool)chkBoxid_315H.IsChecked)
            {
                if (boxingYn_315H)
                {
                    chkBoxid_315H.IsChecked = false;
                    txtBoxId_315H.Text = boxing_lot;
                    txtLotId_315H.Text = "";
                    txtBoxId_315H.Text = "";

                    return;
                }

                if (dgResult_315H.GetRowCount() > 0)
                {
                    LGC.GMES.MES.ControlsLibrary.MessageBox.Show(ObjectDic.Instance.GetObjectName("���� LIST�� �����մϴ�. ��� ���� �Ͻø� LIST�� ���� �˴ϴ�."), null, "CAUSE", MessageBoxButton.OKCancel, MessageBoxIcon.Warning, (caution_result) =>
                    {
                        if (caution_result == MessageBoxResult.OK)
                        {
                            dgResult_315H.ItemsSource = null;
                        }
                        else
                        {
                            chkBoxid_315H.IsChecked = false;
                            txtBoxId_315H.IsReadOnly = true;
                        }
                    }
                 );
                }

                txtBoxId_315H.IsReadOnly = false;
                txtBoxId_315H.Text = "";
                txtLotId_315H.Text = "";
                reBoxing_315H = false;
                palletYN_315H = false;
                boxingLot_idx_315H = 0;
            }
            else
            {
                if (boxingYn_315H)
                {
                    chkBoxid_315H.IsChecked = true;
                    return;
                }

                dgResult_315H.ItemsSource = null;
                txtBoxId_315H.Text = "";
                txtLotId_315H.Text = "";
                txtBoxId_315H.IsReadOnly = true;
                chkBoxid_315H.IsChecked = false;
                txtcnt_315H.Text = ObjectDic.Instance.GetObjectName("������");
            }
        }

        private void txtBoxId_315H_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (boxingYn_315H)
            {
                txtBoxId_315H.TextChanged -= txtBoxId_315H_TextChanged;
                txtBoxId_315H.Text = boxing_lot_315H;
                txtBoxId_315H.TextChanged += txtBoxId_315H_TextChanged;
            }
        }
        
        private void txtLotId_315H_KeyDown(object sender, KeyEventArgs e)
        {
            try
            {
                if (e.Key == Key.Enter)
                {
                    if (txtLotId_315H.Text.Length == 0)
                    {
                        return;
                    }

                    lotInputProcess_315H();
                }
            }
            catch (Exception ex)
            {
                Util.AlertInfo(ex.Message);
            }
        }

        private void btnInput_315H_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (txtLotId_315H.Text.Length == 0)
                {
                    return;
                }

                lotInputProcess_315H();

                txtcnt.Text = ObjectDic.Instance.GetObjectName("������");
            }
            catch (Exception ex)
            {
                Util.AlertInfo(ex.Message);
            }
        }

        private void btnReset_315H_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!boxingYn)
                {
                    reSet_315H();

                    Util.AlertInfo("�۾��� �ʱ�ȭ �ƽ��ϴ�.");
                }
                else
                {
                    LGC.GMES.MES.ControlsLibrary.MessageBox.Show(ObjectDic.Instance.GetObjectName("�������� ������ �ֽ��ϴ�. ���� [�۾��ʱ�ȭ] �Ͻðڽ��ϱ�?"), null, "CAUSE", MessageBoxButton.OKCancel, MessageBoxIcon.Warning, (caution_result) =>
                    {
                        if (caution_result == MessageBoxResult.OK)
                        {
                            reSet_315H();
                            Util.AlertInfo("�۾��� �ʱ�ȭ �ƽ��ϴ�.");
                        }
                        else
                        {
                            return;
                        }
                    }
              );
                }
            }
            catch (Exception ex)
            {
                Util.AlertInfo(ex.Message);
            }
        }

        private void dgResult_315H_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            try
            {
                Point pnt = e.GetPosition(null);
                C1.WPF.DataGrid.DataGridCell cell = dgResult_315H.GetCellFromPoint(pnt);

                if (cell != null)
                {
                    if (cell.Column.Name == "LOTID")
                    {
                        this.FrameOperation.OpenMenu("SFU010090090", true, cell.Text);
                    }
                }
            }
            catch (Exception ex)
            {
                Util.AlertInfo(ex.Message);
            }
        }

        private void dgResult_315H_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            Point pnt = e.GetPosition(null);
            C1.WPF.DataGrid.DataGridCell cell = dgResult_315H.GetCellFromPoint(pnt);
            if (cell != null)
            {
                dgResult_315H.CurrentCell = cell;
            }
        }

        private void menu_LotDelete_315H_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (dgResult_315H.ItemsSource == null)
                {
                    return;
                }

                if (dgResult_315H.CurrentCell != null)
                {
                    if (reBoxing_315H)
                    {
                        if (unPackYN_315H)
                        {
                            unpackProcess_315H();

                            txtcnt_315H.Text = ObjectDic.Instance.GetObjectName("������");
                            boxingYn_315H = true;
                        }
                    }

                    int delete_idx = dgResult_315H.CurrentCell.Row.Index;
                    dgResult_315H.EndNewRow(true);
                    dgResult_315H.RemoveRow(delete_idx);

                    txtquantity.Text = dgResult_315H.GetRowCount().ToString();

                    Util.SetTextBlockText_DataGridRowCount(tbBoxingWait_cnt_315H, Util.NVC(dgResult_315H.GetRowCount()));
                }
            }
            catch (Exception ex)
            {
                Util.Alert(ex.ToString());
            }
        }

        private void btnOutput_315H_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (txtBoxId_315H.Text.Length == 0)
                {
                    if (reBoxing_315H)
                    {
                        return;
                    }

                    if (dgResult_315H.GetRowCount() == 0)
                    {
                        return;
                    }

                    autoBoxIdCreate_315H();
                }

                if (dgResult_315H.GetRowCount() == 0)
                {
                    return;
                }

                if (reBoxing_315H)
                {
                    if (!boxingYn_315H)
                    {
                        return;
                    }
                }

                if (txtSerial.Text != txtBatch.Text)
                {
                    ms.AlertWarning("SFU3376"); //Serial No�� Batch No�� ��ġ ���� �ʽ��ϴ�
                    return;
                }

                boxingEnd_315H(); //���� �Ϸ� �Լ�

                PrintAcess_315H(); //�󺧹���               

                reSet_315H();

                Util.AlertInfo("����/�󺧹��� �Ϸ�");  

            }
            catch (Exception ex)
            {
                Util.AlertInfo(ex.Message);
            }
        }

        private void dtpDate_SelectedDataTimeChanged(object sender, SelectionChangedEventArgs e)
        {
            if (dtpDate == null || dtpDate.SelectedDateTime == null)
            {
                return;
            }

            txtBatch.Text = dtpDate.SelectedDateTime.ToString("yyMMdd");
        }

        private void btnBoxSearch_Click(object sender, RoutedEventArgs e)
        {
            if (txtBoxId_315H.Text.Length == 0)
            {
                return;
            }

            if (!Convert.ToBoolean(chkBoxid_315H.IsChecked))
            {
                return;
            }

            boxValidation_315H();
        }


        #endregion

        #region METHOD
       

        private void set315H(string lotid, string gubun)
        {
            tcMain.SelectedIndex = 1;

            //init_315H();

            if (gubun == "BOX")
            {
                chkBoxid_315H.IsChecked = true;
                txtBoxId_315H.IsReadOnly = false;
                txtBoxId_315H.Text = lotid;
                txtLotId_315H.Text = "";

                btnBoxSearch_Click(null, null);
            }
            else
            {
                chkBoxid_315H.IsChecked = false;
                txtBoxId_315H.IsReadOnly = true;
                txtLotId_315H.Text = lotid;
                txtBoxId_315H.Text = "";

                lotInputProcess_315H();
            }

            btnExpandFrame2.IsChecked = false;
            btnExpandFrame2_Unchecked(null, null);
        }

        private void init_315H()
        {
            setTexBox_315H();

            InitCombo_315H();
        }

        private void setTexBox_315H()
        {
            txtReceive.Text = "Viridi E-Mobility Technology";
            txtDock.Text = ""; // "TVV";
            txtSupplierAddress.Text = "LG Chem, Ltd";
            txtNetWeight.Text = "144";
            txtGrossWeight.Text = "163";
            txtBoxes.Text = "1";
            txtquantity.Text = "12";
            txtSerial.Text = "BOXID";
            txtBatch.Text = DateTime.Now.ToString("yyMMdd"); // "161207";
        }

        private void InitCombo_315H()
        {
            CommonCombo _combo = new CommonCombo();

            C1ComboBox cboSHOPID = new C1ComboBox();
            cboSHOPID.SelectedValue = LoginInfo.CFG_SHOP_ID;
            C1ComboBox cboArea = new C1ComboBox();
            cboArea.SelectedValue = LoginInfo.CFG_AREA_ID;
            C1ComboBox cboEquipmentSegment = new C1ComboBox();
            cboEquipmentSegment.SelectedValue = LoginInfo.CFG_EQSG_ID;
            C1ComboBox cboProductModel = new C1ComboBox();
            cboProductModel.SelectedValue = "";
            C1ComboBox cboPrdtClass = new C1ComboBox();
            cboPrdtClass.SelectedValue = prdClass_315H;

            //��ǰ�ڵ�  
            C1ComboBox[] cboProductParent = { cboSHOPID, cboArea, cboEquipmentSegment, cboProductModel, cboPrdtClass };
            _combo.SetCombo(cboProduct_315H, CommonCombo.ComboStatus.NONE, cbParent: cboProductParent, sCase: "PRJ_PRODUCT");
        }
        private void boxValidation_315H()
        {
            //���� ���� �۾� ����
            if (boxingYn_315H)
            {
                Util.AlertInfo("���� ���� �۾��� �Ϸ� ���� �ʾҽ��ϴ�.");
                return;
            }

            //�Էµ� boxid ����
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("BOXID", typeof(string));     //       
                RQSTDT.Columns.Add("BOXTYPE", typeof(string));   //                                 

                DataRow dr = RQSTDT.NewRow();
                dr["BOXID"] = txtBoxId_315H.Text;
                dr["BOXTYPE"] = "BOX"; //"BOX";                

                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_BOXID_WITHNOT_LOTBOX", "INDATA", "OUTDATA", RQSTDT);

                if (dtResult == null || dtResult.Rows.Count == 0)
                {
                    reSet_315H();
                    Util.AlertInfo("�Է��� BOX�� �������� �ʽ��ϴ�.");
                    return;
                }

                string BOXING_GUBUN = dtResult.Rows[0]["BOXING_GUBUN"].ToString(); //lot���� pallet������ boxid ���� üũ

                if (BOXING_GUBUN == "BOXING_NO")
                {
                    reSet_315H();
                    Util.AlertInfo("����,������ �Ұ����� BOX�Դϴ�.");
                    return;
                }

                string PalletID = dtResult.Rows[0]["OUTER_BOXID"].ToString(); //pallet�� ����� boxid ���� üũ

                if (PalletID.Length > 0)
                {
                    reSet_315H();

                    String[] param = { PalletID };
                    //Util.AlertInfo("�̹� PALLET( " + PalletID + " )�� ����� BOX�Դϴ�.");
                    Util.AlertInfo("�̹� PALLET({0})�� ����� BOX�Դϴ�.", param);
                    return;
                }

                box_prod_315H = dtResult.Rows[0]["PRODID"].ToString(); //box�� prod ��Ƶ�.

                if (Util.GetCondition(cboProduct_315H) != box_prod_315H)
                {
                    Util.AlertInfo("�Է��� BOX�� �������� ��ǰ�� �ٸ��ϴ�.");
                    return;
                }

                foreach (DataRow drw in dtResult.Rows) //�̹� ����� box
                {
                    if (drw["BOXSTAT"].ToString() == "PACKED" && drw["BOXTYPE"].ToString() == "BOX") //if (drw["BOXSTAT"].ToString() == "PACKED") //BOXSTAT ������ ���� �Ǹ� ���� �ʿ�.
                    {
                        if (drw["OUTER_BOXID"] != null && drw["OUTER_BOXID"].ToString().Length > 0 && drw["BOXSTAT"].ToString().Length > 0)
                        {
                            palletYN_315H = true; //�ȷ�Ʈ�� �̹� ����� box                            
                        }

                        seleted_BOX_LotCNT_315H = drw["TOTAL_QTY"].ToString(); //box�� ����� lot�� ����

                        DataTable RQSTDT1 = new DataTable();
                        RQSTDT1.TableName = "RQSTDT";
                        RQSTDT1.Columns.Add("BOXID", typeof(string));

                        DataRow dr1 = RQSTDT1.NewRow();
                        dr1["BOXID"] = txtBoxId_315H.Text;

                        RQSTDT1.Rows.Add(dr1);

                        DataTable dtBoxLots = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_BOXLOTS_SEARCH", "INDATA", "OUTDATA", RQSTDT1);

                        //txtLotId.IsEnabled = false;

                        dgResult_315H.ItemsSource = null;
                        Util.GridSetData(dgResult_315H, dtBoxLots, FrameOperation);
                        boxingLot_idx_315H = dgResult_315H.GetRowCount();
                        Util.SetTextBlockText_DataGridRowCount(tbBoxingWait_cnt_315H, Util.NVC(dtBoxLots.Rows.Count));

                        if (!palletYN_315H)
                        {
                            unPackYN_315H = true;
                        }

                    }
                    else if (drw["BOXSTAT"].ToString() == "CREATED" && drw["BOXTYPE"].ToString() == "BOX")
                    {
                        dgResult_315H.ItemsSource = null;
                        boxingLot_idx_315H = 0;
                        boxing_lot_315H = txtBoxId_315H.Text.ToString();
                    }

                    boxing_lot_315H = drw["BOXID"].ToString();
                    box_prod_315H = drw["PRODID"].ToString();
                    txtquantity.Text = Convert.ToInt32(drw["TOTAL_QTY"]).ToString();
                    txtSerial.Text = drw["BOXID"].ToString();
                    boxStat_315H = drw["BOXSTAT"].ToString(); // packed, created
                    reBoxing_315H = true; //box�� �����ϸ� box ���¿� ������� ��������.
                    txtcnt_315H.Text = ObjectDic.Instance.GetObjectName("���尡��");
                }

            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private void reSet_315H()
        {
            boxingYn_315H = false;
            reBoxing_315H = false;
            palletYN_315H = false;
            boxingLot_idx_315H = 0;

            txtBoxId_315H.Text = "";
            txtLotId_315H.Text = "";

            chkBoxid_315H.IsChecked = false;
            txtBoxId_315H.IsReadOnly = true;

            dgResult_315H.ItemsSource = null;
            tbBoxingWait_cnt_315H.Text = "[ 0 " + ObjectDic.Instance.GetObjectName("��") + " ]";
            txtcnt_315H.Text = ObjectDic.Instance.GetObjectName("������");

            lot_prod_315H = string.Empty; //������ ������ lot�� prodid         
            lot_proc_315H = string.Empty; //������ ������ lot�� procid
            lot_eqsgid_315H = string.Empty; //������ ������ lot��  eqsgid
            lot_class_old_315H = string.Empty; //������ ������ lot�� class

            box_prod_315H = string.Empty; //���� �����ߴ� BOX�� PRODID
            boxing_lot_315H = string.Empty; //���� �������� BOXID

            seleted_Box_Prod_315H = string.Empty;
            seleted_Box_Procid_315H = string.Empty;
            seleted_Box_Eqptid_315H = string.Empty;
            seleted_Box_Eqsgid_315H = string.Empty;

            txtSerial.Text = "BOX ID";
        }

        private void autoBoxIdCreate_315H()
        {
            try
            {
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

                DataRow dr = RQSTDT.NewRow();
                dr["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                dr["LANGID"] = LoginInfo.LANGID;
                dr["LOTID"] = txtLotId_315H.Text;
                dr["PROCID"] = lot_proc_315H; // CMA:P5500, BMA:P9500
                dr["PRODID"] = lot_prod_315H;
                dr["LOTQTY"] = dgResult_315H.GetRowCount().ToString();
                dr["EQSGID"] = lot_eqsgid_315H;
                dr["USERID"] = LoginInfo.USERID;

                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("BR_PRD_GET_BOXIDREQUEST", "INDATA", "OUTDATA", RQSTDT);

                if (dtResult != null && dtResult.Rows.Count != 0)
                {
                    txtBoxId_315H.Text = dtResult.Rows[0][0].ToString();
                    txtSerial.Text = dtResult.Rows[0][0].ToString();
                    boxing_lot_315H = txtBoxId.Text.ToString();
                    txtquantity.Text = dgResult_315H.GetRowCount().ToString();

                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private void boxingEnd_315H()
        {
            try
            {
                DataSet indataSet = new DataSet();

                DataTable INDATA = indataSet.Tables.Add("INDATA");
                INDATA.Columns.Add("SRCTYPE", typeof(string));  //INPUT TYPE (UI OR EQ)         
                INDATA.Columns.Add("LANGID", typeof(string));   //LANGUAGE ID         
                INDATA.Columns.Add("BOXID", typeof(string));    //����LOT(ó�� LOT)
                INDATA.Columns.Add("PROCID", typeof(string));   //����ID(������ ������ ����) 
                INDATA.Columns.Add("BOXQTY", typeof(string));   //���� �Ѽ���
                INDATA.Columns.Add("EQSGID", typeof(string));   //����ID
                INDATA.Columns.Add("USERID", typeof(string));   //�����ID

                DataRow dr = INDATA.NewRow();
                dr["SRCTYPE"] = "UI";
                dr["LANGID"] = LoginInfo.LANGID;
                dr["BOXID"] = Util.GetCondition(txtBoxId_315H);
                dr["PROCID"] = lot_proc_315H;
                dr["BOXQTY"] = dgResult_315H.GetRowCount().ToString();
                dr["EQSGID"] = lot_eqsgid_315H;
                dr["USERID"] = LoginInfo.USERID;
                INDATA.Rows.Add(dr);

                DataTable IN_LOTID = indataSet.Tables.Add("IN_LOTID");
                IN_LOTID.Columns.Add("LOTID", typeof(string));

                for (int i = 0; i < dgResult_315H.GetRowCount(); i++)
                {
                    string sLotId = Util.NVC(dgResult_315H.GetCell(i, dgResult_315H.Columns["LOTID"].Index).Value);

                    DataRow inDataDtl = IN_LOTID.NewRow();
                    inDataDtl["LOTID"] = sLotId;
                    IN_LOTID.Rows.Add(inDataDtl);
                }

                DataSet dsResult = new ClientProxy().ExecuteServiceSync_Multi("BR_PRD_REG_BOXING", "INDATA,IN_LOTID", "OUTDATA,OUT_LOTID", indataSet);

                if (dsResult != null && dsResult.Tables["OUTDATA"].Rows.Count > 0)
                {
                    boxingYn_315H = false;                   
                }
                else
                {
                    throw new Exception("���� �۾� ���� BOXING BIZ Ȯ�� �ϼ���.");
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        //private void PrintProcess_315H()
        //{
        //    if (!bkWorker.IsBusy)
        //    {
        //        blPrintStop_315H = false;
        //        bkWorker.RunWorkerAsync();
        //        btnOutput_315H.Content = ObjectDic.Instance.GetObjectName("�� ��");
        //        btnOutput_315H.Foreground = Brushes.White;
        //    }
        //    else
        //    {
        //        btnOutput_315H.Content = ObjectDic.Instance.GetObjectName("�� ��");
        //        blPrintStop_315H = true;
        //        btnOutput_315H.Foreground = Brushes.Red;
        //    }

        //}

        private void lotInputProcess_315H()
        {
            try
            {
                if (txtBoxId_315H.Text == "") //BOXID �Էµ��� ����
                {
                    boxingLotAddProcess_315H(); //�ű�����     

                    txtcnt_315H.Text = ObjectDic.Instance.GetObjectName("���尡��");
                }
                else
                {
                    reBoxingLotAddProcess_315H(); //������      

                }
            }
            catch (Exception ex)
            {
                txtLotId_315H.Text = "";
                throw ex;
            }
        }       

        //�ű� ������ ��� lot �߰�
        private void boxingLotAddProcess_315H()
        {
            try
            {
                if (!Convert.ToBoolean(chkBoxid_315H.IsChecked)) //BOXID üũ �ڽ� üũ�� Ǯ���� ���
                {
                    //grid �ߺ� üũ
                    if (!gridDistinctCheck_315H())
                    {
                        Util.AlertInfo("�̹� list�� �ִ� LOT�Դϴ�.");
                        return;
                    }

                    //�Էµ� lot validation
                    if (!lotValidation_BR_315H()) //if (!lotValidation())
                    {
                        txtLotId_315H.Text = "";
                        return;
                    }

                    if (lot_prod_315H != combo_prd_315H)
                    {
                        Util.AlertInfo("������(combo) PROD�� LOT�� PROD�� �ٸ��ϴ�.");
                        return;
                    }

                    //lot�� �׸���(dgBoxLot)�� �߰�
                    addGridLot_315H();

                    //txtBoxId.Text = boxing_lot;

                    txtLotId_315H.Text = "";

                    //boxingYn = true; //�ڽ���.

                }
                else
                {
                    Util.AlertInfo("###[�����۾�]�� ��� �Ͻ÷���### \n1.BOXID�� �Է� �� ��ȸ ��ư�� Ŭ���Ѵ�. \n2.CHECKBOX üũ�� �����Ѵ�.\n�ΰ��� ����� �Ѱ����� ������ �����ϼ���.");
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        //������ �� ��� lot �߰�
        private void reBoxingLotAddProcess_315H()
        {
            try
            {
                if (gridDistinctCheck_315H()) //�׸��� �ߺ� üũ
                {
                    //�Էµ� lotid validation
                    if (!lotValidation_BR_315H()) //if (!lotValidation())
                    {
                        txtLotId_315H.Text = "";
                        return;
                    }

                    if (lot_prod_315H != combo_prd_315H)
                    {
                        Util.AlertInfo("������(combo) PROD�� LOT�� PROD�� �ٸ��ϴ�.");
                        return;
                    }

                    if (lot_prod_315H != box_prod_315H)
                    {
                        Util.AlertInfo("BOX�� PROD�� LOT�� PROD�� �ٸ��ϴ�.");
                        return;
                    }

                    if (unPackYN_315H)
                    {
                        unpackProcess_315H();
                    }


                    addGridLot_315H();
                    txtLotId_315H.Text = "";
                    txtcnt_315H.Text = ObjectDic.Instance.GetObjectName("������");
                    boxingYn_315H = true;
                }
                else
                {
                    Util.AlertInfo("�̹� list�� �ִ� LOT�Դϴ�.");
                    txtLotId.Text = "";
                    return;
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private bool gridDistinctCheck_315H()
        {
            try
            {
                DataRowView rowview = null;

                if (dgResult_315H.GetRowCount() == 0)
                {
                    return true;
                }

                foreach (C1.WPF.DataGrid.DataGridRow row in dgResult_315H.Rows)
                {

                    if (row.DataItem != null)
                    {
                        rowview = row.DataItem as DataRowView;

                        if (rowview["LOTID"].ToString() == txtLotId_315H.Text.ToString())
                        {                           
                            return false;
                        }
                    }
                }
                return true;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private bool lotValidation_BR_315H()
        {
            try
            {
                //lot_proc = string.Empty;

                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("SRCTYPE", typeof(string));    //����LOT(ó�� LOT)       
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("BOXID", typeof(string));
                RQSTDT.Columns.Add("BOXTYPE", typeof(string));
                RQSTDT.Columns.Add("BOX_PRODID", typeof(string));
                RQSTDT.Columns.Add("PRDT_CLSS", typeof(string));
                RQSTDT.Columns.Add("EQSGID", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                dr["LANGID"] = LoginInfo.LANGID;
                dr["BOXID"] = Util.GetCondition(txtLotId_315H); //LOTID
                dr["BOXTYPE"] = "LOT";
                dr["BOX_PRODID"] = lot_prod_315H == "" ? null : lot_prod_315H;
                dr["PRDT_CLSS"] = lot_class_old_315H == "" ? null : lot_class_old_315H;
                dr["EQSGID"] = lot_eqsgid_315H == "" ? null : lot_eqsgid_315H;

                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("BR_PRD_CHK_BOXLOT_PALLET", "INDATA", "OUTDATA", RQSTDT);

                //LOT TABLE�� WIP TABLE�� PROD ��
                if (dtResult.Rows.Count > 0)
                {
                    string lot_class = dtResult.Rows[0]["CLASS"].ToString();
                    string lot_procid = dtResult.Rows[0]["PROCID"].ToString();
                    string lot_prodid = dtResult.Rows[0]["PRODID"].ToString();
                    string lot_eqsg = dtResult.Rows[0]["EQSGID"].ToString();

                    lot_proc_315H = lot_procid;
                    lot_prod_315H = lot_prodid;
                    lot_eqsgid_315H = lot_eqsg;
                    lot_class_old_315H = lot_class;

                    return true;
                }
                else
                {
                    Util.AlertInfo("LOT�� ���� ���� ������ ���� �ʾ� ������ �� �����ϴ�.");
                    return false;
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private void addGridLot_315H()
        {
            if (boxingLot_idx_315H == 0)
            {
                DataTable dtBoxLot = new DataTable();
                dtBoxLot.Columns.Add("LOTID", typeof(string));

                DataRow dr = dtBoxLot.NewRow();
                dr["LOTID"] = Util.GetCondition(txtLotId_315H);

                dtBoxLot.Rows.Add(dr);

                dgResult_315H.ItemsSource = DataTableConverter.Convert(dtBoxLot);

                //boxingYn = true;
            }
            else
            {
                int TotalRow = dgResult_315H.GetRowCount(); //�������

                dgResult_315H.EndNewRow(true);
                DataGridRowAdd(dgResult_315H);

                DataTableConverter.SetValue(dgResult_315H.Rows[TotalRow].DataItem, "LOTID", Util.GetCondition(txtLotId_315H));
            }
            boxingLot_idx_315H++;
            //lotCountReverse--;

            DataTable dt = DataTableConverter.Convert(dgResult_315H.ItemsSource);

            Util.SetTextBlockText_DataGridRowCount(tbBoxingWait_cnt_315H, Util.NVC(dt.Rows.Count));


            txtquantity.Text = dgResult_315H.GetRowCount().ToString();
            txtLotId_315H.SelectAll();
            txtLotId_315H.Focus();
        }

        private void unpackProcess_315H()
        {
            try
            {
                if (reBoxing_315H == true && unPackYN_315H == true) //�������̸鼭 ���� unpack ���� ���� ��� uppack ��Ŵ.
                {
                    unPack_315H();

                    unPackYN_315H = false;
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private void unPack_315H()
        {
            try
            {
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

                DataRow dr = INDATA.NewRow();
                dr["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                dr["BOXID"] = boxing_lot_315H;
                dr["PRODID"] = box_prod_315H;
                dr["PACK_LOT_TYPE_CODE"] = "LOT";
                dr["UNPACK_QTY"] = seleted_BOX_LotCNT_315H;
                dr["UNPACK_QTY2"] = seleted_BOX_LotCNT_315H;
                dr["USERID"] = LoginInfo.USERID;
                dr["ERP_IF_FLAG"] = "C";
                dr["NOTE"] = "BOX UNPACK";
                INDATA.Rows.Add(dr);

                //DataTable dsResult = new ClientProxy().ExecuteServiceSync("DA_BAS_UPD_BOX_UNPACK", "INDATA", "OUTDATA", INDATA);
                DataTable dsResult = new ClientProxy().ExecuteServiceSync("BR_PRD_REG_UNPACK_BOX", "INDATA", "OUTDATA", INDATA);

                if (dsResult != null && dsResult.Rows.Count > 0)
                {
                    boxStat_315H = "CREATED";
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private void PrintAcess_315H()//private void BarcodePrint(bool isTest, string sLabelType, string sLotId, string sProdId, string sProdName, string sQty)
        {
            try
            {
                string I_ATTVAL = string.Empty;
                CMM_ZPL_VIEWER2 wndPopup;

                I_ATTVAL = labelItemsGet_315H();

                getZpl_315H(I_ATTVAL);
                Util.PrintLabel(FrameOperation, loadingIndicator, zpl_315H);

                ms.AlertInfo("SFU1933"); //����� ���� �Ǿ����ϴ�

                if (LoginInfo.USERID.Trim() == "cnswkdakscjf")
                {
                    wndPopup = new CMM_ZPL_VIEWER2(zpl_315H);
                    wndPopup.Show();
                }                
            }
            catch (Exception ex)
            {
                ms.AlertWarning(ex.Message);
            }
        }

        private string labelItemsGet_315H()
        {
            string I_ATTVAL = string.Empty;
            string item_code = string.Empty;
            string item_value = string.Empty;

            DataTable dtInput;

            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LABEL_CODE", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LABEL_CODE"] = label_code_315H;

                RQSTDT.Rows.Add(dr);

                //ITEM001=TEST1^ITEM002=TEST2 : �ڵ�=��^�ڵ�=��

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_LABEL_ITEM_FIND", "INDATA", "OUTDATA", RQSTDT);

                if (dtResult == null || dtResult.Rows.Count > 0)
                {
                    dtInput = getInputData_315H();

                    for (int i = 0; i < dtResult.Rows.Count; i++)
                    {
                        #region sample value �Ѹ�
                        /*
                        item_code = dtResult.Rows[i]["ITEM_CODE"].ToString();
                        item_value = dtResult.Rows[i]["ITEM_VALUE"].ToString();
                        */
                        #endregion

                        #region ȭ�鿡�� �Էµ� �� �Ѹ�                        
                        item_code = dtResult.Rows[i]["ITEM_CODE"].ToString();
                        item_value = dtInput.Rows[0][item_code].ToString() == "" ? null : dtInput.Rows[0][item_code].ToString();
                        #endregion

                        I_ATTVAL += item_code + "=" + item_value;

                        if (i < dtResult.Rows.Count - 1)
                        {
                            I_ATTVAL += "^";
                        }
                    }
                }

                return I_ATTVAL;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private DataTable getInputData_315H()
        {
            DataTable dt = new DataTable();
            dt.TableName = "INPUTDATA";
            dt.Columns.Add("ITEM001", typeof(string));
            dt.Columns.Add("ITEM002", typeof(string));
            dt.Columns.Add("ITEM003", typeof(string));
            dt.Columns.Add("ITEM004", typeof(string));
            dt.Columns.Add("ITEM005", typeof(string));
            dt.Columns.Add("ITEM006", typeof(string));
            dt.Columns.Add("ITEM007", typeof(string));
            dt.Columns.Add("ITEM008", typeof(string));
            dt.Columns.Add("ITEM009", typeof(string));
            dt.Columns.Add("ITEM010", typeof(string));
            dt.Columns.Add("ITEM011", typeof(string));
            dt.Columns.Add("ITEM012", typeof(string));

            dt.Columns.Add("ITEM013", typeof(string));
            dt.Columns.Add("ITEM014", typeof(string));
            dt.Columns.Add("ITEM015", typeof(string));
            dt.Columns.Add("ITEM016", typeof(string));
            dt.Columns.Add("ITEM017", typeof(string));
            dt.Columns.Add("ITEM018", typeof(string));
            dt.Columns.Add("ITEM019", typeof(string));
            dt.Columns.Add("ITEM020", typeof(string));
            dt.Columns.Add("ITEM021", typeof(string));

            DataRow dr = dt.NewRow();
            dr["ITEM001"] = Util.GetCondition(txtReceive); //VOLVO TORSLANDA MONTERING
            dr["ITEM002"] = Util.GetCondition(txtDock); //TCV
            dr["ITEM003"] = Util.GetCondition(txtAdvice); //61606292
            dr["ITEM004"] = Util.GetCondition(txtAdvice) == "" ? "" : "N" + Util.GetCondition(txtAdvice); //61606292
            dr["ITEM005"] = Util.GetCondition(txtSupplierAddress); // LG Chem, Ltd
            dr["ITEM006"] = Util.GetCondition(txtNetWeight); //115
            dr["ITEM007"] = Util.GetCondition(txtGrossWeight); //160
            dr["ITEM008"] = Util.GetCondition(txtBoxes); //1
            dr["ITEM009"] = Util.GetCondition(txtpartNumber); //31491834
            dr["ITEM010"] = Util.GetCondition(txtpartNumber) == "" ? "" : "P" + Util.GetCondition(txtpartNumber); //31491834
            dr["ITEM011"] = Util.GetCondition(txtquantity); //1
            dr["ITEM012"] = Util.GetCondition(txtquantity) == "" ? "" : "Q" + Util.GetCondition(txtquantity); //1

            dr["ITEM013"] = Util.GetCondition(cboDescription) == null ? "" : Util.GetCondition(cboDescription); //BATTERY PACK
            dr["ITEM014"] = Util.GetCondition(cboDescription) == null ? "" : Util.GetCondition(cboDescription); //355V,25.9A,6500Wh(Usable)
            dr["ITEM015"] = Util.GetCondition(txtSupplierID); //GE2PB
            dr["ITEM016"] = Util.GetCondition(txtSupplierID) == "" ? "" : Util.GetCondition(txtSupplierID); //GE2PB
            dr["ITEM017"] = Util.GetCondition(txtDate); //D160629
            dr["ITEM018"] = Util.GetCondition(txtSerial); //616242017
            dr["ITEM019"] = Util.GetCondition(txtSerial); //616242017
            dr["ITEM020"] = Util.GetCondition(txtBatch); //616242017
            dr["ITEM021"] = Util.GetCondition(txtBatch) == "" ? "" : "H" + Util.GetCondition(txtBatch); //616242017
            dt.Rows.Add(dr);

            return dt;
        }

        private void getZpl_315H(string I_ATTVAL)
        {
            try
            {
                //DataTable RQSTDT = new DataTable();
                //RQSTDT.TableName = "RQSTDT";
                //RQSTDT.Columns.Add("I_LBCD", typeof(string));
                //RQSTDT.Columns.Add("I_PRMK", typeof(string));
                //RQSTDT.Columns.Add("I_RESO", typeof(string));
                //RQSTDT.Columns.Add("I_PRCN", typeof(string));
                //RQSTDT.Columns.Add("I_MARH", typeof(string));
                //RQSTDT.Columns.Add("I_MARV", typeof(string));
                //RQSTDT.Columns.Add("I_ATTVAL", typeof(string));

                //DataRow dr = RQSTDT.NewRow();
                //dr["I_LBCD"] = label_code_315H;
                //dr["I_PRMK"] = "Z";
                //dr["I_RESO"] = "203";
                //dr["I_PRCN"] = "1";
                //dr["I_MARH"] = "0";
                //dr["I_MARV"] = "0";
                //dr["I_ATTVAL"] = I_ATTVAL;

                //RQSTDT.Rows.Add(dr);

                ////ITEM001=TEST1^ITEM002=TEST2

                //DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_LABEL_DESIGN_TEST", "INDATA", "OUTDATA", RQSTDT);

                DataTable dtResult = Util.getDirectZpl(
                                                      sLBCD: label_code_315H
                                                     , sATTVAL: I_ATTVAL
                                                     );

                if (dtResult == null || dtResult.Rows.Count > 0)
                {
                    zpl_315H = dtResult.Rows[0]["ZPLSTRING"].ToString();
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        #endregion



        #endregion


        #region 312,313,515H, 517H  

        #region EVENT
        private void cboProduct2_seletedItemChanged(object sender, PropertyChangedEventArgs<object> e)
        {

            if (dtModelResult != null || dtModelResult.Rows.Count > 0)
            {
                DataRow[] dr = dtModelResult.Select("CBO_CODE = '" + Util.GetCondition(cboProduct2) + "'");

                tabItem.Header = Util.NVC(dr[0]["MODEL"]);

                getValueSetting();
            }
        }

        private void getValueSetting()
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("PRODID", typeof(string));
                RQSTDT.Columns.Add("LABEL", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["PRODID"] = Util.GetCondition(cboProduct2); //dtpDateTo.SelectedDateTime.ToString
                dr["LABEL"] = label_code2; //dtpDateTo.SelectedDateTime.ToString();

                RQSTDT.Rows.Add(dr);

                dtTextResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_PRINTVALUE_FIND", "INDATA", "OUTDATA", RQSTDT);

                if (dtTextResult == null || dtTextResult.Rows.Count == 0)
                {
                    //ms.AlertWarning("�߰�"); //MMD �������� ���� : ����ó �μ��׸� ����
                    return;
                }
                else
                {
                    setTextBox();

                    dtpDate2_SelectedDateChanged(null, null);
                }

            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private void setTextBox()
        {
            if (dtTextResult == null || dtTextResult.Rows.Count == 0)
            {
                return;
            }

            txtNetWeight2.Text = returnString("ITEM006");        //Net Weight(kg) : 115           
            txtGrossWeight2.Text = returnString("ITEM007");      //Gross Weight(kg) : 160           
            txtDescription22.Text = returnString("ITEM014");     //Description 2 : 355V,25.9A, 6,500Wh(Usable)

            txtReceive2.Text = returnString("ITEM001");          //Receiver : VOLVO TORSLANDA MONTERING
            txtAdvice2.Text = returnString("ITEM003");           //ADVICE NOTE NO : 61606292
            txtDock2.Text = returnString("ITEM002");             //DOCK GATE : TCV
            txtSupplierAddress2.Text = returnString("ITEM005");  //Supplier Address : LG Chem, Ltd
            txtBoxes2.Text = returnString("ITEM008");            //No of Boxes : 1
            txtpartNumber2.Text = "31407014";                    //Part No : 31491834
            txtquantity2.Text = returnString("ITEM011");         //Quantity : 1
            txtSupplierID2.Text = returnString("ITEM015");       //Supplier ID : GE2PB
            txtSerial.Text = returnString("ITEM018");           //Serial No : 312031601
            txtLogistic12.Text = "logic";
            txtLogistic22.Text = "reference";
            txtBatch2.Text = returnString("ITEM020");            //Batch No : 312031601
            txtDescription12.Text = returnString("ITEM013");     //Description : BATTERY PACK
        }

        private DataRow[] selectText(string item_code)
        {
            DataRow[] drs;

            drs = dtTextResult.Select("ITEM_CODE = '" + item_code + "'");
            return drs;
        }

        private string returnString(string item_code)
        {
            return selectText(item_code).Length > 0 ? Util.NVC(selectText(item_code)[0]["ITEM_VALUE"]) : "";
        }
        

        private void dtpDate2_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            if (dtpDate2 == null || dtpDate2.SelectedDateTime == null)
            {
                return;
            }

            if (cboProduct2 == null)
            {
                return;
            }

            //��¥ ���ý� Advice Note No, Date ������ �����ͼ� Text �ڽ��� ����   
            txtDate2.Text = "D" + dtpDate.SelectedDateTime.ToString("yyyyMMdd");

            string line_no = getLine();

            txtAdvice2.Text = line_no + dtpDate2.SelectedDateTime.ToString("yyMMdd") + nbProductBox.Value.ToString();

        }

        private string getLine()
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("PRODID", typeof(string));
                //2018.10.08
                RQSTDT.Columns.Add("EQSGID", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["PRODID"] = Util.GetCondition(cboProduct2); //dtpDateTo.SelectedDateTime.ToString();
                //2018.10.08
                dr["PRODID"] = LoginInfo.CFG_EQSG_ID.ToString();

                RQSTDT.Rows.Add(dr);

                dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_LINE_WITH_PRODID_FIND", "INDATA", "OUTDATA", RQSTDT);

                if (dtResult == null || dtResult.Rows.Count == 0)
                {
                    return "8";
                }
                else
                {
                    return dtResult.Rows[0]["LINE_NO"].ToString();
                }

            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private void dtpDate2_SelectedDateChanged1(object sender, PropertyChangedEventArgs<double> e)
        {
            dtpDate2_SelectedDateChanged(null, null);
        }

        private void rdb515H_CheckedChanged(object sender, RoutedEventArgs e)
        {
            dtpDate2_SelectedDateChanged(null, null);
        }

        private void btnSearch2_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                getValueSetting();

                dtpDate2_SelectedDateChanged(null, null);

                Get_Product_Lot();
                
            }
            catch (Exception ex)
            {
                Util.AlertInfo(ex.Message);
            }
        }

        private void btnPrint2_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (btnPrint2.Content.ToString() == ObjectDic.Instance.GetObjectName("�� ��"))
                {
                    bkWorker.Dispose();
                    blPrintStop = true;
                    btnPrint2.Content = ObjectDic.Instance.GetObjectName("�� ��");
                    return;
                }

                if (txtSerial2.Text != txtBatch2.Text)
                {
                    ms.AlertWarning("SFU3376"); //Serial No�� Batch No�� ��ġ ���� �ʽ��ϴ�
                    return;
                }

                PrintProcess();
            }
            catch (Exception ex)
            {
                Util.AlertInfo(ex.Message);
            }
        }

        private void btnAdvice_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                //Label Layout�� Advice Note No �κ� ���
                string strZPLString = "^XA^MCY^XZ^XA^LRN^FWN^CFD,24^LH10,10^CI0^PR2^MNY^MTT^MMT^MD0^PON^PMN^XZ^XA";

                if ((bool)chkAutoPrint.IsChecked)
                {
                    strZPLString += string.Format("^A0N,18,20^FO5,0^CI0^FDAdvice Note No (N)^FS");
                    strZPLString += string.Format("^A0N,70,60^FO210,0^CI0^FD{0}^FS", txtAdvice2.Text); //txt312H03
                    strZPLString += string.Format("^BY4,2.8^FO30,60^B3N,N,104,N,N^FDN{0}^FS", txtAdvice2.Text); //txt312H03
                    strZPLString += string.Format("^PQ{0},0,1,Y^XZ", nbPrintCnt_R.Value);  //nuAdviceNoteNo
                    PrintBoxLabel(strZPLString);

                    ms.AlertInfo("SFU1933"); //����� ���� �Ǿ����ϴ� 
                }
                else
                {
                    strZPLString += string.Format("^A0N,70,60^FO210,0^CI0^FD{0}^FS", txtAdvice2.Text); //txt312H03
                    strZPLString += string.Format("^BY4,2.8^FO30,60^B3N,N,104,N,N^FDN{0}^FS", txtAdvice2.Text); //txt312H03
                    strZPLString += string.Format("^PQ{0},0,1,Y^XZ", nbPrintCnt_R.Value);  //nuAdviceNoteNo
                    PrintBoxLabel(strZPLString);

                    ms.AlertInfo("SFU1933"); //����� ���� �Ǿ����ϴ� 

                    return;
                }
            }
            catch(Exception ex)
            {
                Util.Alert(ex.Message);
            }            
        }

        private void dgResult_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            try
            {
                Point pnt = e.GetPosition(null);
                C1.WPF.DataGrid.DataGridCell cell = dgResult.GetCellFromPoint(pnt);

                if (cell == null || cell.Value == null)
                {
                    return;
                }

                int currentRow = cell.Row.Index;
                int _col = cell.Column.Index;
                string value = cell.Value.ToString();

                //2189498 3ȣ��_312H ���� �� �����û                   

                string strLotId = DataTableConverter.GetValue(dgResult.Rows[currentRow].DataItem, "LOTID").ToString();

                //2297504 Volvo 312H (Pack 3ȣ��) Barcode ü�� ����
                //string strRefNo = strLotId.Contains("T") ? strLotId.Substring(0, strLotId.IndexOf('T')) : "";
                string strRefNo = strLotId.Contains("T") ? strLotId.Substring(0, 11) : "";


                txtpartNumber2.Text = strRefNo;
                txtSerial2.Text = strLotId;
                txtBatch2.Text = strRefNo;

                /*
                string strDateCodeYear = "123456789ABCDEFGHJKLMNPRSTVWXY";
                string strDateCodeMonth = "ABCDEFGHJKLM";

                if (strLotId.Length == 16)
                {
                    string strYY = strLotId.Substring(10, 1);
                    string strMM = strLotId.Substring(11, 1);

                    strLotId = string.Format("3{0:00}{1:00}{2:0000}", strDateCodeYear.IndexOf(strYY) + 1, strDateCodeMonth.IndexOf(strMM) + 1, strLotId.Substring(12, strLotId.Length - 12));

                txtpartNumber.Text = strRefNo;                       

                }
                //2189498 3ȣ��_312H ���� �� �����û

                //2297504 Volvo 312H (Pack 3ȣ��) Barcode ü�� ����
                if (strLotId.Length == 18)
                {
                    //strLotId = strLotId.Substring(strLotId.IndexOf('T') + 1);
                    strLotId = strLotId.Substring(11);
                    txtpartNumber.Text = strRefNo; 
                }

                //to-be
                if (strLotId.Length > 18)
                {
                    strLotId = strLotId.Substring(11)
                    //strLotId = strLotId.Substring(strLotId.IndexOf('T') + 1);;
                    txtpartNumber.Text = strRefNo;
                }

                txtSerial.Text = strLotId;
                txtBatch.Text = strLotId;

                //2534222 3ȣ��_313H ����ڽ� �� Dock Gate ���� ��� ����
                txtDock.Text = "NODATE";
                    //Clipboard.SetText(MakeZPLString());
 */
            }
            catch (Exception ex)
            {
                Util.AlertInfo(ex.Message);
            }
        }
        #endregion

        #region METHOD
        private void set312_313_315_517()
        {
            tcMain.SelectedIndex = 2;

            btnExpandFrame3.IsChecked = false;
            btnExpandFrame3_Unchecked(null, null);
        }

        private void init_312_313_315_515()
        {            
            //dtpDate2_SelectedDateChanged(null, null);

            labelTextSet();

            cboProduct2.SelectedItemChanged -= cboProduct2_seletedItemChanged;

            setCombo();

            cboProduct2.SelectedItemChanged += cboProduct2_seletedItemChanged;

            //�󺧹��� Ŭ���� �Ѿ�� ��ǰ ������ ��ǰ�޺��� ���ε�(����:�׳� tab Ŭ���� ����ó��)
            if (setComboBinding())
            {
                search2_LINK();
            }
            else
            {
                ms.AlertWarning("�Ѿ�� ��ǰ ������ ���������� ������ ������ �ٸ��ϴ�.");
            }

            //dgResult.ItemsSource = null;

        }

        private bool setComboBinding()
        {
            bool ret = false;

            if(unPack_ProdID.Length == 0) //��ȸȭ�� Ÿ�� �Ѿ� ���� ���� ���
            {
                return ret;
            }

            for(int i = 0; i < cboProduct2.Items.Count; i++)
            {
                cboProduct2.SelectedIndex = i;
                if(unPack_ProdID == cboProduct2.SelectedValue.ToString()) //�Ѿ�� ��ǰ�� ���õ� �޺��߿� ���� ���
                {
                    cboProduct2.SelectedValue = unPack_ProdID;

                    ret = true;
                }
                else
                {
                    ret = false;
                }
            }

            return ret;
        }

        private void bkWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            try
            {
                //Dispatcher.Invoke(DispatcherPriority.Normal, new Action(delegate
                //{
                PrintAcess();
                //}));
            }
            catch (Exception ex)
            {
                bkWorker.CancelAsync();
                blPrintStop = true;

                Util.AlertInfo(ex.Message);
            }
        }

        private void bkWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            btnPrint2.Content = ObjectDic.Instance.GetObjectName("���");
            blPrintStop = true;
            btnPrint2.Foreground = Brushes.White;            
        }       

        private void search2_LINK()
        {
            try
            {
                //��ȸ ���ǵ鿡 �ش��ϴ� LOT_ID�� PALLET_ID �����ͼ� Grid�� ���ε�  
                dtpDate2_SelectedDateChanged(null, null);
                Get_Product_Lot_LINK();                
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private void labelTextSet()
        {
            string strRefNo = selectBox.Contains("T") ? selectBox.Substring(0, 11) : "";

            txtpartNumber2.Text = strRefNo;
            //txtSerial2.Text = selectBox;
            txtBatch2.Text = strRefNo;
        }       

        private void Get_Product_Lot()
        {
            DataTable dtResult;

            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("PRODID", typeof(string));
                RQSTDT.Columns.Add("FROMDATE", typeof(string));
                RQSTDT.Columns.Add("TODATE", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["PRODID"] = Util.GetCondition(cboProduct2);
                dr["FROMDATE"] = Util.GetCondition(dtpDateFrom2);  //dtpDateFrom.SelectedDateTime.ToString();
                dr["TODATE"] = Util.GetCondition(dtpDateTo2); //dtpDateTo.SelectedDateTime.ToString();

                RQSTDT.Rows.Add(dr);

                dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_PRODUCT_COUNT_VOLVOBMA", "INDATA", "OUTDATA", RQSTDT);

                dgResult.ItemsSource = null;
                tbPalletHistory_cnt.Text = "[ 0 " + ObjectDic.Instance.GetObjectName("��") + " ]";
                if (dtResult == null || dtResult.Rows.Count == 0)
                {
                    return;
                }

                Util.GridSetData(dgResult, dtResult, FrameOperation);

                Util.SetTextBlockText_DataGridRowCount(tbPalletHistory_cnt, Util.NVC(dtResult.Rows.Count));
            }
            catch (Exception ex)
            {
                Util.AlertInfo(ex.Message + "\r\n" + ex.StackTrace);
            }
        }

        private void Get_Product_Lot_LINK()
        {
            DataTable dtResult;

            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("BOXID", typeof(string));
               
                DataRow dr = RQSTDT.NewRow();
                dr["BOXID"] = Util.GetCondition(txtBoxIdR);

                RQSTDT.Rows.Add(dr);

                dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_PALLETLOT_WITH_BOX_FIND", "INDATA", "OUTDATA", RQSTDT);

                dgResult.ItemsSource = null;
                tbPalletHistory_cnt.Text = "[ 0 " + ObjectDic.Instance.GetObjectName("��") + " ]";
                if (dtResult == null || dtResult.Rows.Count == 0)
                {
                    return;
                }

                Util.GridSetData(dgResult, dtResult, FrameOperation);

                Util.SetTextBlockText_DataGridRowCount(tbPalletHistory_cnt, Util.NVC(dtResult.Rows.Count));
            }
            catch (Exception ex)
            {
                Util.AlertInfo(ex.Message + "\r\n" + ex.StackTrace);
            }
        }

        private string PrintOutDate(DateTime dt)
        {
            System.IFormatProvider format = new System.Globalization.CultureInfo("en-US", true);
            return dt.ToString("dd") + dt.ToString("MMMM", format).Substring(0, 3).ToUpper() + dt.ToString("yyyy");
        }

        private void PrintProcess()
        {
            if (!bkWorker.IsBusy)
            {
                blPrintStop = false;
                bkWorker.RunWorkerAsync();
                btnPrint2.Content = ObjectDic.Instance.GetObjectName("���");
                btnPrint2.Foreground = Brushes.White;
            }
            else
            {
                btnPrint2.Content = ObjectDic.Instance.GetObjectName("���");
                blPrintStop = true;
                btnPrint2.Foreground = Brushes.Red;
            }
        }

        private void PrintAcess()
        {
            try
            {
                Util.SetCondition_Thread(tbPrint2_cnt, "[" + ObjectDic.Instance.GetObjectName("�μ����") + " : 0 " + ObjectDic.Instance.GetObjectName("��") + " ]");
                //tbPrint2_cnt.Text = "[�μ�� �� : 0 ��]";
                string I_ATTVAL = string.Empty;
                

                I_ATTVAL = labelItemsGet();

                getZpl(I_ATTVAL);

                for (int i = 0; i <Convert.ToUInt32(Util.GetCondition_Thread(nbPrintCnt)); i++)
                {
                    if (blPrintStop) break;

                    //Util.PrintLabel(FrameOperation, loadingIndicator, zpl2);                   
                    Util.SetCondition_Thread(tbPrint2_cnt, "[" + ObjectDic.Instance.GetObjectName("�μ����") + " : 0 " + ObjectDic.Instance.GetObjectName("��") + " ]");                    
                    System.Threading.Thread.Sleep(Convert.ToInt32(Util.GetCondition_Thread(nbDelay)) * 1000);                   
                }

                if (LoginInfo.USERID.Trim() == "cnswkdakscjf")
                {
                    Dispatcher.Invoke(DispatcherPriority.Normal, new Action(delegate
                    {
                        CMM_ZPL_VIEWER2 wndPopup;
                        wndPopup = new CMM_ZPL_VIEWER2(zpl2);
                        wndPopup.Show();
                    }));
                }

                ms.AlertInfo("SFU1933"); //����� ���� �Ǿ����ϴ� 
            }
            catch (Exception ex)
            {
                Util.AlertInfo(ex.Message);                
            }
        }
       
        private string labelItemsGet()
        {
            string I_ATTVAL = string.Empty;
            string item_code = string.Empty;
            string item_value = string.Empty;

            DataTable dtInput;

            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LABEL_CODE", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LABEL_CODE"] = label_code2;

                RQSTDT.Rows.Add(dr);

                //ITEM001=TEST1^ITEM002=TEST2 : �ڵ�=��^�ڵ�=��

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_LABEL_ITEM_FIND", "INDATA", "OUTDATA", RQSTDT);

                if (dtResult == null || dtResult.Rows.Count > 0)
                {
                    dtInput = getInputData();

                    for (int i = 0; i < dtResult.Rows.Count; i++)
                    {
                        #region sample value �Ѹ�
                        /*
                        item_code = dtResult.Rows[i]["ITEM_CODE"].ToString();
                        item_value = dtResult.Rows[i]["ITEM_VALUE"].ToString();
                        */
                        #endregion

                        #region ȭ�鿡�� �Էµ� �� �Ѹ�                        
                        item_code = dtResult.Rows[i]["ITEM_CODE"].ToString();
                        item_value = dtInput.Rows[0][item_code].ToString();
                        #endregion

                        I_ATTVAL += item_code + "=" + item_value;

                        if (i < dtResult.Rows.Count - 1)
                        {
                            I_ATTVAL += "^";
                        }
                    }
                }

                return I_ATTVAL;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private DataTable getInputData()
        {
            DataTable dt = new DataTable();
            dt.TableName = "INPUTDATA";
            dt.Columns.Add("ITEM001", typeof(string));
            dt.Columns.Add("ITEM002", typeof(string));
            dt.Columns.Add("ITEM003", typeof(string));
            dt.Columns.Add("ITEM004", typeof(string));
            dt.Columns.Add("ITEM005", typeof(string));
            dt.Columns.Add("ITEM006", typeof(string));
            dt.Columns.Add("ITEM007", typeof(string));
            dt.Columns.Add("ITEM008", typeof(string));
            dt.Columns.Add("ITEM009", typeof(string));
            dt.Columns.Add("ITEM010", typeof(string));
            dt.Columns.Add("ITEM011", typeof(string));
            dt.Columns.Add("ITEM012", typeof(string));

            dt.Columns.Add("ITEM013", typeof(string));
            dt.Columns.Add("ITEM014", typeof(string));
            dt.Columns.Add("ITEM015", typeof(string));
            dt.Columns.Add("ITEM016", typeof(string));
            dt.Columns.Add("ITEM017", typeof(string));
            dt.Columns.Add("ITEM018", typeof(string));
            dt.Columns.Add("ITEM019", typeof(string));
            dt.Columns.Add("ITEM020", typeof(string));
            dt.Columns.Add("ITEM021", typeof(string));

            DataRow dr = dt.NewRow();
            dr["ITEM001"] = Util.GetCondition_Thread (txtReceive2); //VOLVO TORSLANDA MONTERING
            dr["ITEM002"] = Util.GetCondition_Thread(txtDock2); //TCV
            dr["ITEM003"] = Util.GetCondition_Thread(txtAdvice2); //61606292
            dr["ITEM004"] = Util.GetCondition_Thread(txtAdvice2); //61606292
            dr["ITEM005"] = Util.GetCondition_Thread(txtSupplierAddress2); // LG Chem, Ltd
            dr["ITEM006"] = Util.GetCondition_Thread(txtNetWeight2); //115
            dr["ITEM007"] = Util.GetCondition_Thread(txtGrossWeight2); //160
            dr["ITEM008"] = Util.GetCondition_Thread(txtBoxes2); //1
            dr["ITEM009"] = Util.GetCondition_Thread(txtpartNumber2); //31491834
            dr["ITEM010"] = Util.GetCondition_Thread(txtpartNumber2); //31491834
            dr["ITEM011"] = Util.GetCondition_Thread(txtquantity2); //1
            dr["ITEM012"] = Util.GetCondition_Thread(txtquantity2); //1

            dr["ITEM013"] = Util.GetCondition_Thread(txtDescription12); //BATTERY PACK
            dr["ITEM014"] = Util.GetCondition_Thread(txtDescription22); //355V,25.9A,6500Wh(Usable)
            dr["ITEM015"] = Util.GetCondition_Thread(txtSupplierID2); //GE2PB
            dr["ITEM016"] = Util.GetCondition_Thread(txtSupplierID2); //GE2PB
            dr["ITEM017"] = Util.GetCondition_Thread(txtDate2); //D160629
            dr["ITEM018"] = Util.GetCondition_Thread(txtSerial2); //616242017
            dr["ITEM019"] = Util.GetCondition_Thread(txtSerial2); //616242017
            dr["ITEM020"] = Util.GetCondition_Thread(txtBatch2); //616242017
            dr["ITEM021"] = Util.GetCondition_Thread(txtBatch2); //616242017
            dt.Rows.Add(dr);

            return dt;
        }

        private void getZpl(string I_ATTVAL)
        {
            try
            {
                //DataTable RQSTDT = new DataTable();
                //RQSTDT.TableName = "RQSTDT";
                //RQSTDT.Columns.Add("I_LBCD", typeof(string));
                //RQSTDT.Columns.Add("I_PRMK", typeof(string));
                //RQSTDT.Columns.Add("I_RESO", typeof(string));
                //RQSTDT.Columns.Add("I_PRCN", typeof(string));
                //RQSTDT.Columns.Add("I_MARH", typeof(string));
                //RQSTDT.Columns.Add("I_MARV", typeof(string));
                //RQSTDT.Columns.Add("I_ATTVAL", typeof(string));

                //DataRow dr = RQSTDT.NewRow();
                //dr["I_LBCD"] = label_code2;
                //dr["I_PRMK"] = "Z";
                //dr["I_RESO"] = "203";
                //dr["I_PRCN"] = "1";
                //dr["I_MARH"] = "0";
                //dr["I_MARV"] = "0";
                //dr["I_ATTVAL"] = I_ATTVAL;

                //RQSTDT.Rows.Add(dr);

                ////ITEM001=TEST1^ITEM002=TEST2

                //DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_LABEL_DESIGN_TEST", "INDATA", "OUTDATA", RQSTDT);

                DataTable dtResult = Util.getDirectZpl(
                                                      sLBCD: label_code2
                                                     , sATTVAL: I_ATTVAL
                                                     );

                if (dtResult == null || dtResult.Rows.Count > 0)
                {
                    zpl2 = dtResult.Rows[0]["ZPLSTRING"].ToString();

                    //CMM_ZPL_VIEWER2 wndPopup = new CMM_ZPL_VIEWER2(testZpl);
                    //wndPopup.Show();

                    //Util.PrintLabel(FrameOperation, loadingIndicator, testZpl);
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private void setTexBox()
        {
            try
            {
                txtReceive2.Text = "VOLVO TORSLANDA MONTERING";
                txtAdvice2.Text = "31607151";
                txtDock2.Text = " TVV ";
                txtSupplierAddress2.Text = "LG Chem, Ltd";
                txtNetWeight2.Text = "150";
                txtGrossWeight2.Text = "180";
                txtBoxes2.Text = "1";
                txtpartNumber2.Text = "31407014";
                txtquantity2.Text = "1";
                txtSupplierID2.Text = "GE2PB";
                txtSerial2.Text = "312031601";
                txtLogistic12.Text = "logic";
                txtLogistic22.Text = "reference";
                txtDate2.Text = "D160629";
                txtBatch2.Text = "312031601";
                txtDescription12.Text = "BATTERY PACK";
                txtDescription22.Text = "375V, 30Ah, 11,250Wh";
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private void PrintBoxLabel(string sZpl)
        {
            try
            {
                CMM_ZPL_VIEWER2 wndPopup;

                wndPopup = new CMM_ZPL_VIEWER2(sZpl);

                //Util.PrintLabel(FrameOperation, loadingIndicator, sZpl);
                System.Threading.Thread.Sleep((int)nbDelay.Value * 1000);

                wndPopup.Show();
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private void setBacode(object sender)
        {
            try
            {
                TextBox tbBox = (TextBox)sender;

                switch (tbBox.Name)
                {
                    case "txtAdvice":
                        bcAdvice.Text = tbBox.Text;
                        break;
                    case "txtpartNumber":
                        bcpartNumber.Text = tbBox.Text;
                        break;
                    case "txtquantity":
                        bcquantity.Text = tbBox.Text;
                        break;
                    case "txtSerial":
                        bcSerial.Text = tbBox.Text;
                        break;
                    case "txtBatch":
                        bcBatch.Text = tbBox.Text;
                        break;

                    case "txtAdvice2":
                        bcAdvice2.Text = tbBox.Text;
                        break;
                    case "txtpartNumber2":
                        bcpartNumber2.Text = tbBox.Text;
                        break;
                    case "txtquantity2":
                        bcquantity2.Text = tbBox.Text;
                        break;
                    case "txtSupplierID2":
                        bcSupplierID2.Text = tbBox.Text;
                        break;
                    case "txtSerial2":
                        bcSerial2.Text = tbBox.Text;
                        break;
                    case "txtBatch2":
                        bcBatch2.Text = tbBox.Text;
                        break;
                    default:
                        break;
                }
            }
            catch (Exception ex)
            {

                Util.AlertInfo(ex.Message);
            }
        }
        #endregion

        #endregion

        
    }
}
