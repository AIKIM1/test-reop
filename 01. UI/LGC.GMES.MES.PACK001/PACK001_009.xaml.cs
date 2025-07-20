/*************************************************************************************
 Created Date : 2016.06.16
      Creator : 
   Decription : 
--------------------------------------------------------------------------------------
 [Change History]
  2016.06.16  DEVELOPER : Initial Created.
  2018.08.06 �տ켮 3758784 GMES 315H �ڽ��� ��� ���� ���� ��û �� ��û��ȣ C20180806_58784
  2018.08.08 �տ켮 3760796 [��û] ��7ȣ�� Volvo 315H ����Box label ���� ���� �� ��û��ȣ C20180808_60796
**************************************************************************************/

using System;
using System.ComponentModel;
using System.Data;
using System.Windows;
using System.Windows.Controls;
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
    public partial class PACK001_009 : UserControl, IWorkArea
    {
        #region Declaration & Constructor 
        System.ComponentModel.BackgroundWorker bkWorker;
        LGC.GMES.MES.PACK001.Class.MESSAGE_PARAM ms = new LGC.GMES.MES.PACK001.Class.MESSAGE_PARAM();

        bool boxingYn = false; //���������� ����
        bool reBoxing = false; //������ ���� : box id ��ȸ�� ���������(create/packed ��� ����) - packed�� ��츸 ���Ŀ� �������� ����
        bool unPackYN = false; //�������� �ߴ��� ���� : boxid �Է� �� ���� lot �Է½� or lot ������ unpack �ϱ� ���� �뵵
        bool palletYN = false; //�ȷ�Ʈ�� ����� box���� ����

        string boxStat = string.Empty; //box�� stat : created / packed�� ��츸 �������� - packed�� ��� ���� �������� ����
        
        int boxingLot_idx = 0;
        private bool blPrintStop = true;
        string label_code = "LBL0020";
        string zpl = string.Empty;       

        string combo_prd = string.Empty;

        string lot_prod = string.Empty; //������ ������ lot�� prodid         
        string lot_proc = string.Empty; //������ ������ lot�� procid
        string lot_eqsgid = string.Empty; //������ ������ lot��  eqsgid
        string lot_class_old = string.Empty; //������ ������ lot�� class

        string box_prod = string.Empty; //���� �����ߴ� BOX�� PRODID
        string boxing_lot = string.Empty; //���� �������� BOXID

        string seleted_Box_Prod = string.Empty;
        string seleted_Box_Procid = string.Empty;
        string seleted_Box_Eqptid = string.Empty;
        string seleted_Box_Eqsgid = string.Empty;
        string seleted_BOX_LotCNT = string.Empty;

        public PACK001_009()
        {
            InitializeComponent();

            this.Loaded += PACK001_009_Loaded;
        }

        public IFrameOperation FrameOperation
        {
            get;
            set;
        }

        #endregion
       
        #region Initialize
        private void Initialize()
        {
            bkWorker = new System.ComponentModel.BackgroundWorker();
            bkWorker.WorkerSupportsCancellation = true;
            bkWorker.DoWork += new System.ComponentModel.DoWorkEventHandler(bkWorker_DoWork);
            bkWorker.RunWorkerCompleted += new System.ComponentModel.RunWorkerCompletedEventHandler(this.bkWorker_RunWorkerCompleted);

            dtpDate.SelectedDateTime = (DateTime)System.DateTime.Now;

            tbBoxingWait_cnt.Text = "[ 0 " + ObjectDic.Instance.GetObjectName("��") + " ]";

            setTexBox();

            InitCombo();

            //getProductID();
        }
        #endregion

        #region Event
        private void PACK001_009_Loaded(object sender, RoutedEventArgs e)
        {
            Initialize();

            this.Loaded -= PACK001_009_Loaded;
        }

        private void dgResult_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {

        }

        private void btnOutput_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if(txtBoxId.Text.Length ==0)
                {
                    if(reBoxing)
                    {
                        return;
                    }

                    if (dgResult.GetRowCount() == 0)
                    {
                        return;
                    }

                    autoBoxIdCreate();
                }

                if(dgResult.GetRowCount() == 0)
                {
                    return;
                }

                if(reBoxing)
                {
                    if (!boxingYn)
                    {
                        return;
                    }
                }

                if (txtSerial.Text != txtBatch.Text)
                {
                    ms.AlertWarning("SFU3376"); //Serial No�� Batch No�� ��ġ ���� �ʽ��ϴ�
                    return;
                }
             
                boxingEnd(); //���� �Ϸ� �Լ�               

                //�󺧹���
                PrintProcess();                

                chkBoxid.Visibility = Visibility.Visible;
                // labelPrint();

            }
            catch(Exception ex)
            {
                //Util.AlertInfo(ex.Message);
                Util.MessageException(ex);
            }
        }

        private void txtAdvice_TextChanged(object sender, TextChangedEventArgs e)
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

        private void txtSupplierID_TextChanged(object sender, TextChangedEventArgs e)
        {
            setBacode(sender);
        }

        private void txtSerial_TextChanged(object sender, TextChangedEventArgs e)
        {
            setBacode(sender);
        }

        private void txtBatch_TextChanged(object sender, TextChangedEventArgs e)
        {
            setBacode(sender);
        }

        private void dgResult_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            try
            {
                Point pnt = e.GetPosition(null);
                C1.WPF.DataGrid.DataGridCell cell = dgResult.GetCellFromPoint(pnt);

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
                ms.AlertWarning(ex.Message);                
            }
        }

        private void chkBoxid_Click(object sender, RoutedEventArgs e)
        {
            if ((bool)chkBoxid.IsChecked)
            {
                if (boxingYn)
                {
                    chkBoxid.IsChecked = false;
                    txtBoxId.Text = boxing_lot;
                    txtLotId.Text = "";
                    txtBoxId.Text = "";

                    return;
                }

                if (dgResult.GetRowCount() > 0)
                {
                    //LGC.GMES.MES.ControlsLibrary.MessageBox.Show(ObjectDic.Instance.GetObjectName("���� LIST�� �����մϴ�. ��� ���� �Ͻø� LIST�� ���� �˴ϴ�."), null, "CAUSE", MessageBoxButton.OKCancel, MessageBoxIcon.Warning, (caution_result) ==>
                     LGC.GMES.MES.ControlsLibrary.MessageBox.Show(ms.AlertRetun("SFU3402"), null, "CAUSE", MessageBoxButton.OKCancel, MessageBoxIcon.Warning, (caution_result) =>
                     {
                        if (caution_result == MessageBoxResult.OK)
                        {
                            dgResult.ItemsSource = null;
                        }
                        else
                        {
                            chkBoxid.IsChecked = false;
                            txtBoxId.IsReadOnly = true;
                        }
                    }
                 );

                }

                txtBoxId.IsReadOnly = false;
                txtBoxId.Text = "";
                txtLotId.Text = "";
                reBoxing = false;
                palletYN = false;
                boxingLot_idx = 0;
            }
            else
            {
                if (boxingYn)
                {
                    chkBoxid.IsChecked = true;
                    return;
                }

                dgResult.ItemsSource = null;
                txtBoxId.Text = "";
                txtLotId.Text = "";
                txtBoxId.IsReadOnly = true;
                chkBoxid.IsChecked = false;
                txtcnt.Text = ObjectDic.Instance.GetObjectName("������");
            }
        }

        private void dtpDate_SelectedDataTimeChanged(object sender, SelectionChangedEventArgs e)
        {
            if (dtpDate == null || dtpDate.SelectedDateTime == null)
            {
                return;
            }

            //2018.08.06
            //txtBatch.Text = dtpDate.SelectedDateTime.ToString("yyMMdd");
            ////txtAdvice.Text = dtpDate.SelectedDateTime.ToString("yyyyMMdd");
        }

        private void btnReset_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!boxingYn)
                {
                    reSet();

                    //Util.AlertInfo("�۾��� �ʱ�ȭ �ƽ��ϴ�.");
                    ms.AlertInfo("SFU3377"); //�۾��� �ʱ�ȭ �ƽ��ϴ�.
                }
                else
                {
                    //LGC.GMES.MES.ControlsLibrary.MessageBox.Show(ObjectDic.Instance.GetObjectName("�������� ������ �ֽ��ϴ�. ���� [�۾��ʱ�ȭ] �Ͻðڽ��ϱ�?"), null, "CAUSE", MessageBoxButton.OKCancel, MessageBoxIcon.Warning, (caution_result) =>
                    LGC.GMES.MES.ControlsLibrary.MessageBox.Show(ms.AlertRetun("SFU3282"), null, "CAUSE", MessageBoxButton.OKCancel, MessageBoxIcon.Warning, (caution_result) =>
                    {
                        if (caution_result == MessageBoxResult.OK)
                        {
                            reSet();                            
                            ms.AlertInfo("SFU3377"); //�۾��� �ʱ�ȭ �ƽ��ϴ�.
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

        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if(txtBoxId.Text.Length == 0)
                {
                    return;
                }

                if(!Convert.ToBoolean(chkBoxid.IsChecked))
                {
                    return;
                }                

                boxValidation();                
            }
            catch(Exception ex)
            {
                //Util.AlertInfo(ex.Message);
                Util.MessageException(ex);
            }
        }

        private void btnInput_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (txtLotId.Text.Length == 0)
                {
                    return;
                }

                lotInputProcess();

                txtcnt.Text = ObjectDic.Instance.GetObjectName("������");
            }
            catch(Exception ex)
            {
                //Util.AlertInfo(ex.Message);
                Util.MessageException(ex);
            }
        }

        private void txtLotId_KeyDown(object sender, KeyEventArgs e)
        {
            try
            {
                if(e.Key == Key.Enter)
                {
                    if(txtLotId.Text.Length == 0)
                    {
                        return;
                    }

                    lotInputProcess();                    
                }
            }
            catch (Exception ex)
            {
                Util.AlertInfo(ex.Message);
            }
        }        

        private void DataGridRowAdd(C1.WPF.DataGrid.C1DataGrid dg)
        {
            dg.BeginNewRow();
            dg.EndNewRow(true);
        }

        private void btnExcelActHistory_Click(object sender, RoutedEventArgs e)
        {

        }

        private void cboProduct_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            combo_prd = Util.GetCondition(cboProduct);

            txtpartNumber.Text = combo_prd;
        }

        private void txtBoxId_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (boxingYn)
            {
                txtBoxId.TextChanged -= txtBoxId_TextChanged;
                txtBoxId.Text = boxing_lot;
                txtBoxId.TextChanged += txtBoxId_TextChanged;
            }
        }

        private void dgResult_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            Point pnt = e.GetPosition(null);
            C1.WPF.DataGrid.DataGridCell cell = dgResult.GetCellFromPoint(pnt);
            if (cell != null)
            {
                dgResult.CurrentCell = cell;
            }
        }

        private void menu_LotDelete_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (dgResult.ItemsSource == null)
                {
                    return;
                }

                if (dgResult.CurrentCell != null)
                {
                    if (reBoxing)
                    {
                        if (unPackYN)
                        {
                            unpackProcess();

                            txtcnt.Text = ObjectDic.Instance.GetObjectName("������");
                            boxingYn = true;
                        }
                    }

                    int delete_idx = dgResult.CurrentCell.Row.Index;
                    dgResult.EndNewRow(true);
                    dgResult.RemoveRow(delete_idx);

                    Util.SetTextBlockText_DataGridRowCount(tbBoxingWait_cnt, Util.NVC(dgResult.GetRowCount()));
                }
            }
            catch (Exception ex)
            {
                //Util.Alert(ex.ToString());
                Util.MessageException(ex);
            }
        }
        #endregion

        #region Mehod

        private void InitCombo()
        {
            CommonCombo _combo = new CommonCombo();

            C1ComboBox cboSHOPID = new C1ComboBox();
            cboSHOPID.SelectedValue = LoginInfo.CFG_SHOP_ID;
            C1ComboBox cboArea = new C1ComboBox();
            cboArea.SelectedValue = "P6"; //�ش� ȭ���� 6�� ���� ȭ����.
            C1ComboBox cboEquipmentSegment = new C1ComboBox();
            cboEquipmentSegment.SelectedValue = "P6Q07"; // �ش� ȭ���� 7ȣ�� ���� ȭ����.
            C1ComboBox cboProductModel = new C1ComboBox();
            cboProductModel.SelectedValue = "";
            C1ComboBox cboPrdtClass = new C1ComboBox();
            cboPrdtClass.SelectedValue = "CMA"; // �ش� ȭ���� CMA ����ȭ����

            //tbInfo.Text = LoginInfo.CFG_AREA_NAME + " // " + LoginInfo.CFG_EQSG_NAME + "(" + prdClass + ")";

            //��ǰ    
            //C1ComboBox[] cboProductParent = { cboSHOPID, cboArea, cboEquipmentSegment, cboProductModel, cboAREA_TYPE_CODE, cboPrdtClass };
            //string[] line = { null, null, "P6Q09", null, null, "CMA" };
            //string[] line = { null, null, "P1Q02", null, null, "CMA" };
            //_combo.SetCombo(cboProduct, CommonCombo.ComboStatus.NONE, sFilter : line);


            //��ǰ�ڵ�  
            C1ComboBox[] cboProductParent = { cboSHOPID, cboArea, cboEquipmentSegment, cboProductModel, cboPrdtClass };
            _combo.SetCombo(cboProduct, CommonCombo.ComboStatus.NONE, cbParent: cboProductParent, sCase: "PRJ_PRODUCT");
        }

        private void boxingEnd()
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
                dr["BOXID"] = Util.GetCondition(txtBoxId);
                dr["PROCID"] = lot_proc;
                dr["BOXQTY"] = dgResult.GetRowCount().ToString();
                dr["EQSGID"] = lot_eqsgid;
                dr["USERID"] = LoginInfo.USERID;
                INDATA.Rows.Add(dr);

                DataTable IN_LOTID = indataSet.Tables.Add("IN_LOTID");
                IN_LOTID.Columns.Add("LOTID", typeof(string));

                for (int i = 0; i < dgResult.GetRowCount(); i++)
                {
                    string sLotId = Util.NVC(dgResult.GetCell(i, dgResult.Columns["LOTID"].Index).Value);

                    DataRow inDataDtl = IN_LOTID.NewRow();
                    inDataDtl["LOTID"] = sLotId;
                    IN_LOTID.Rows.Add(inDataDtl);
                }

                DataSet dsResult = new ClientProxy().ExecuteServiceSync_Multi("BR_PRD_REG_BOXING", "INDATA,IN_LOTID", "OUTDATA,OUT_LOTID", indataSet);

                if (dsResult != null && dsResult.Tables["OUTDATA"].Rows.Count > 0)
                {
                    boxingYn = false;                   
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
        private void PrintProcess()
        {
            if (!bkWorker.IsBusy)
            {
                blPrintStop = false;
                bkWorker.RunWorkerAsync();
                btnOutput.Content = ObjectDic.Instance.GetObjectName("���");
                btnOutput.Foreground = Brushes.White;
            }
            else
            {
                btnOutput.Content = ObjectDic.Instance.GetObjectName("���");
                blPrintStop = true;
                btnOutput.Foreground = Brushes.Red;
            }

        }
        private void bkWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            btnOutput.Content = ObjectDic.Instance.GetObjectName("���");
            blPrintStop = true;
            btnOutput.Foreground = Brushes.White;   

            reSet();
        }

        private void bkWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            try
            {
                Dispatcher.Invoke(DispatcherPriority.Normal, new Action(delegate
                {
                    PrintAcess();
                }));
            }
            catch (Exception ex)
            {
                bkWorker.CancelAsync();
                blPrintStop = true;

                Util.AlertInfo(ex.Message);
            }
        }

        private void PrintAcess()//private void BarcodePrint(bool isTest, string sLabelType, string sLotId, string sProdId, string sProdName, string sQty)
        {
            try
            {
                string I_ATTVAL = string.Empty;
                CMM_ZPL_VIEWER2 wndPopup;

                I_ATTVAL = labelItemsGet();

                getZpl(I_ATTVAL);
                Util.PrintLabel(FrameOperation, loadingIndicator, zpl);

                ms.AlertInfo("SFU1933"); //����� ���� �Ǿ����ϴ�

                if (LoginInfo.USERID.Trim() == "cnswkdakscjf")
                {
                    wndPopup = new CMM_ZPL_VIEWER2(zpl);
                    wndPopup.Show();
                }
               
            }
            catch (Exception ex)
            {
                //ms.AlertWarning(ex.Message);
                Util.MessageException(ex);
            }           
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
                //dr["I_LBCD"] = label_code;
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
                                                      sLBCD: label_code
                                                     , sATTVAL: I_ATTVAL
                                                     );

                if (dtResult == null || dtResult.Rows.Count > 0)
                {
                    zpl = dtResult.Rows[0]["ZPLSTRING"].ToString();
                }
            }
            catch (Exception ex)
            {
                throw ex;
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
                dr["LABEL_CODE"] = label_code;

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
            dr["ITEM001"] = Util.GetCondition(txtReceive);
            dr["ITEM002"] = Util.GetCondition(txtDock);
            dr["ITEM003"] = Util.GetCondition(txtAdvice);
            dr["ITEM004"] = Util.GetCondition(txtAdvice) == "" ? "" : "N" + Util.GetCondition(txtAdvice);
            dr["ITEM005"] = Util.GetCondition(txtSupplierAddress);
            dr["ITEM006"] = Util.GetCondition(txtNetWeight);
            dr["ITEM007"] = Util.GetCondition(txtGrossWeight);
            dr["ITEM008"] = Util.GetCondition(txtBoxes);
            dr["ITEM009"] = Util.GetCondition(txtpartNumber);
            dr["ITEM010"] = Util.GetCondition(txtpartNumber) == "" ? "" : "P" + Util.GetCondition(txtpartNumber);
            dr["ITEM011"] = Util.GetCondition(txtquantity);
            dr["ITEM012"] = Util.GetCondition(txtquantity) == "" ? "" : "Q" + Util.GetCondition(txtquantity);

            //2018.08.08
            //dr["ITEM013"] = Util.GetCondition(cboDescription) == null ? "" : Util.GetCondition(cboDescription);
            //dr["ITEM014"] = Util.GetCondition(cboDescription) == null ? "" : Util.GetCondition(cboDescription);
            dr["ITEM013"] = Util.GetCondition(txtDescription) == null ? "" : Util.GetCondition(txtDescription);
            dr["ITEM014"] = "";

            dr["ITEM015"] = Util.GetCondition(txtSupplierID);
            dr["ITEM016"] = Util.GetCondition(txtSupplierID) == "" ? "" : Util.GetCondition(txtSupplierID);
            dr["ITEM017"] = Util.GetCondition(txtDate);
            dr["ITEM018"] = Util.GetCondition(txtSerial);
            dr["ITEM019"] = Util.GetCondition(txtSerial);
            dr["ITEM020"] = Util.GetCondition(txtBatch);
            dr["ITEM021"] = Util.GetCondition(txtBatch) == "" ? "" : "H" + Util.GetCondition(txtBatch);
            dt.Rows.Add(dr);

            return dt;
        }

        private void lotInputProcess()
        {
            try
            {
                if (txtBoxId.Text == "") //BOXID �Էµ��� ����
                {
                    boxingLotAddProcess(); //�ű�����     

                    txtcnt.Text = ObjectDic.Instance.GetObjectName("���尡��");
                }
                else
                {
                    reBoxingLotAddProcess(); //������      

                }
            }
            catch (Exception ex)
            {
                txtLotId.Text = "";
                throw ex;
            }
        }

        //�ű� ������ ��� lot �߰�
        private void boxingLotAddProcess()
        {
            try
            {
                if (!Convert.ToBoolean(chkBoxid.IsChecked)) //BOXID üũ �ڽ� üũ�� Ǯ���� ���
                {
                    //grid �ߺ� üũ
                    if (!gridDistinctCheck())
                    {                        
                        ms.AlertWarning("SFU2014"); //�ش� LOT�� �̹� �����մϴ�.
                        return;
                    }

                    //�Էµ� lot validation
                    if (!lotValidation_BR()) //if (!lotValidation())
                    {
                        txtLotId.Text = "";
                        return;
                    }

                    if (lot_prod != combo_prd)
                    {
                        //Util.AlertInfo("������(combo) PROD�� LOT�� PROD�� �ٸ��ϴ�.");      
                        ms.AlertWarning("SFU3284"); //�Է¿��� : �޺����� ������ ��ǰ�� �Է��� BOX�� ��ǰ�� �ٸ��ϴ�.[������ǰ�� �´� BOX�Է�]           
                        return;
                    }

                    //BOXID �ڵ� ���� : ��½� 
                    //autoBoxIdCreate();

                    //������ boxid�� prod ������. ==> ȭ�� �ε��� �۾����� wordorder�� prod�� �������� ������ ���� �ʿ� ���� �Լ�
                    //getBoxProd();

                    //if (lot_prod != box_prod)
                    //{
                    //    LGC.GMES.MES.ControlsLibrary.MessageBox.Show("BOX�� PROD�� LOT�� PROD�� �ٸ��ϴ�.", null, "Error", MessageBoxButton.OK, ControlsLibrary.MessageBoxIcon.None);
                    //    return;
                    //}

                    //Util.gridClear(dgResult); //�׸��� clear

                    //lot�� �׸���(dgBoxLot)�� �߰�
                    addGridLot();

                    //txtBoxId.Text = boxing_lot;

                    txtLotId.Text = "";

                    //boxingYn = true; //�ڽ���.

                }
                else
                {
                    //Util.AlertInfo("###[�����۾�]�� ��� �Ͻ÷���### \n1.BOXID�� �Է� �� ��ȸ ��ư�� Ŭ���Ѵ�. \n2.CHECKBOX üũ�� �����Ѵ�.\n�ΰ��� ����� �Ѱ����� ������ �����ϼ���.");
                    ms.AlertInfo("SFU3378"); //###[�����۾�]�� ��� �Ͻ÷���### \n1.BOXID�� �Է� �� ��ȸ ��ư�� Ŭ���Ѵ�. \n2.CHECKBOX üũ�� �����Ѵ�.\n�ΰ��� ����� �Ѱ����� ������ �����ϼ���.
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        //������ �� ��� lot �߰�
        private void reBoxingLotAddProcess()
        {
            try
            {
                if (gridDistinctCheck()) //�׸��� �ߺ� üũ
                {
                    //�Էµ� lotid validation
                    if (!lotValidation_BR()) //if (!lotValidation())
                    {
                        txtLotId.Text = "";
                        return;
                    }

                    if (lot_prod != combo_prd)
                    {
                        //Util.AlertInfo("������(combo) PROD�� LOT�� PROD�� �ٸ��ϴ�.");
                        ms.AlertWarning("SFU3284");//�Է¿��� : �޺����� ������ ��ǰ�� �Է��� BOX�� ��ǰ�� �ٸ��ϴ�.[������ǰ�� �´� BOX�Է�]
                        return;
                    }

                    if (lot_prod != box_prod)
                    {
                        //Util.AlertInfo("BOX�� PROD�� LOT�� PROD�� �ٸ��ϴ�.");
                        ms.AlertWarning("SFU3328"); //�Է¿��� : ���� ������� BOX���� ��ǰ�� ���� ������ BOX�� ��ǰ�� �ٸ��ϴ�.
                        return;
                    }

                    if (unPackYN)
                    {
                        unpackProcess();
                    }


                    addGridLot();
                    txtLotId.Text = "";
                    txtcnt.Text = ObjectDic.Instance.GetObjectName("������");
                    boxingYn = true;
                }
                else
                {                    
                    ms.AlertWarning("SFU2014"); //�ش� LOT�� �̹� �����մϴ�.
                    txtLotId.Text = "";
                    return;
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private void unpackProcess()
        {
            try
            {
                if (reBoxing == true && unPackYN == true) //�������̸鼭 ���� unpack ���� ���� ��� uppack ��Ŵ.
                {
                    unPack();

                    unPackYN = false;
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private void unPack()
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
                dr["BOXID"] = boxing_lot;
                dr["PRODID"] = box_prod;
                dr["PACK_LOT_TYPE_CODE"] = "LOT";
                dr["UNPACK_QTY"] = seleted_BOX_LotCNT;
                dr["UNPACK_QTY2"] = seleted_BOX_LotCNT;
                dr["USERID"] = LoginInfo.USERID;
                dr["ERP_IF_FLAG"] = "C";
                dr["NOTE"] = "BOX UNPACK";
                INDATA.Rows.Add(dr);

                //DataTable dsResult = new ClientProxy().ExecuteServiceSync("DA_BAS_UPD_BOX_UNPACK", "INDATA", "OUTDATA", INDATA);
                DataTable dsResult = new ClientProxy().ExecuteServiceSync("BR_PRD_REG_UNPACK_BOX", "INDATA", "OUTDATA", INDATA);

                if (dsResult != null && dsResult.Rows.Count > 0)
                {
                    boxStat = "CREATED";
                }
            }
            catch (Exception ex)
            {
                throw ex;
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

                    lot_proc = lot_procid;
                    lot_prod = lot_prodid;
                    lot_eqsgid = lot_eqsg;
                    lot_class_old = lot_class;

                    //txtBoxInfo.Text = lot_class_old + " : " + lot_eqsgid + " : " + lot_prodid;

                    return true;
                }
                else
                {
                    //Util.AlertInfo("LOT�� ���� ���� ������ ���� �ʾ� ������ �� �����ϴ�.");
                    ms.AlertWarning("SFU3379"); //LOT�� ���� ���� ������ ���� �ʾ� ������ �� �����ϴ�.
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
               // Util.AlertInfo("���� ���� �۾��� �Ϸ� ���� �ʾҽ��ϴ�.");
                ms.AlertWarning("SFU3380"); //���� ���� �۾��� �Ϸ� ���� �ʾҽ��ϴ�.            
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
                dr["BOXID"] = txtBoxId.Text;
                dr["BOXTYPE"] = "BOX"; //"BOX";                

                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_BOXID_WITHNOT_LOTBOX", "INDATA", "OUTDATA", RQSTDT);

                if (dtResult == null || dtResult.Rows.Count == 0)
                {
                    reSet();                    
                    ms.AlertWarning("SFU1180"); //BOX ������ �����ϴ�.
                    return;
                }

                string BOXING_GUBUN = dtResult.Rows[0]["BOXING_GUBUN"].ToString(); //lot���� pallet������ boxid ���� üũ

                if (BOXING_GUBUN == "BOXING_NO")
                {
                    reSet();
                    //Util.AlertInfo("����,������ �Ұ����� BOX�Դϴ�.");
                    ms.AlertWarning("SFU3381"); //����,������ �Ұ����� BOX�Դϴ�."
                    return;
                }

                string PalletID = dtResult.Rows[0]["OUTER_BOXID"].ToString(); //pallet�� ����� boxid ���� üũ

                if (PalletID.Length > 0)
                {
                    reSet();
                    //Util.AlertInfo("�̹� PALLET( " + PalletID + " )�� ����� BOX�Դϴ�."); 
                    ms.AlertWarning("SFU3288", PalletID); // �Է¿��� : �Է��� BOX�� �̹� PALLET�� ���� �ƽ��ϴ�.[BOX�� ���� Ȯ��]

                    return;
                }

                box_prod = dtResult.Rows[0]["PRODID"].ToString(); //box�� prod ��Ƶ�.

                if (Util.GetCondition(cboProduct) != box_prod)
                {
                   //Util.AlertInfo("�Է��� BOX�� �������� ��ǰ�� �ٸ��ϴ�.");
                    ms.AlertWarning("SFU3283"); //�Է¿��� : �������� BOX�� ��ǰ�� �Է��� LOT�� ��ǰ�� �ٸ��ϴ�.

                    return;
                }

                foreach (DataRow drw in dtResult.Rows) //�̹� ����� box
                {
                    if (drw["BOXSTAT"].ToString() == "PACKED" && drw["BOXTYPE"].ToString() == "BOX") //if (drw["BOXSTAT"].ToString() == "PACKED") //BOXSTAT ������ ���� �Ǹ� ���� �ʿ�.
                    {
                        if (drw["OUTER_BOXID"] != null && drw["OUTER_BOXID"].ToString().Length > 0 && drw["BOXSTAT"].ToString().Length > 0)
                        {
                            palletYN = true; //�ȷ�Ʈ�� �̹� ����� box                            
                        }

                        seleted_BOX_LotCNT = drw["TOTAL_QTY"].ToString(); //box�� ����� lot�� ����

                        DataTable RQSTDT1 = new DataTable();
                        RQSTDT1.TableName = "RQSTDT";
                        RQSTDT1.Columns.Add("BOXID", typeof(string));

                        DataRow dr1 = RQSTDT1.NewRow();
                        dr1["BOXID"] = txtBoxId.Text;

                        RQSTDT1.Rows.Add(dr1);

                        DataTable dtBoxLots = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_BOXLOTS_SEARCH", "INDATA", "OUTDATA", RQSTDT1);

                        //txtLotId.IsEnabled = false;

                        dgResult.ItemsSource = null;
                        Util.GridSetData(dgResult, dtBoxLots, FrameOperation);                       
                        boxingLot_idx = dgResult.GetRowCount();
                        Util.SetTextBlockText_DataGridRowCount(tbBoxingWait_cnt, Util.NVC(dtBoxLots.Rows.Count));

                        if (!palletYN)
                        {
                            unPackYN = true;
                        }

                    }
                    else if (drw["BOXSTAT"].ToString() == "CREATED" && drw["BOXTYPE"].ToString() == "BOX")
                    {
                        dgResult.ItemsSource = null;
                        boxingLot_idx = 0;
                        boxing_lot = txtBoxId.Text.ToString();
                    }

                    boxing_lot = drw["BOXID"].ToString();
                    box_prod = drw["PRODID"].ToString();
                    txtquantity.Text = drw["TOTAL_QTY"].ToString();
                    //2018.08.06
                    //txtSerial.Text = drw["BOXID"].ToString();
                    //txtBatch.Text = drw["BOXID"].ToString();
                    txtSerial.Text = drw["BOXID"].ToString().Substring(9,8);
                    txtBatch.Text = drw["BOXID"].ToString().Substring(9, 8);
                    boxStat = drw["BOXSTAT"].ToString(); // packed, created
                    reBoxing = true; //box�� �����ϸ� box ���¿� ������� ��������.
                    txtcnt.Text = ObjectDic.Instance.GetObjectName("���尡��");
                }

            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private bool gridDistinctCheck()
        {
            try
            {
                DataRowView rowview = null;

                if (dgResult.GetRowCount() == 0)
                {
                    return true;
                }

                foreach (C1.WPF.DataGrid.DataGridRow row in dgResult.Rows)
                {

                    if (row.DataItem != null)
                    {
                        rowview = row.DataItem as DataRowView;

                        if (rowview["LOTID"].ToString() == txtLotId.Text.ToString())
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
                           // Util.AlertInfo("���� LOT�Դϴ�.");
                            ms.AlertWarning("SFU3290"); //�Է¿��� : ���� LOT�Դϴ�. [LOT �̷� Ȯ��]
                            return false;
                        }

                        if (lot_wiphold == "Y") //hold �������� üũ
                        {                            
                            ms.AlertWarning("SFU1340"); //HOLD �� LOT ID �Դϴ�.
                            return false;
                        }

                        if (lot_class_old != null && lot_class_old != "") //���� ���� ��ǰ Ÿ�԰� ������ ��
                        {
                            if (lot_class_old != lot_class)
                            {
                                //Util.AlertInfo("������ ������ LOT�� ��ǰ Ÿ�԰� �ٸ��ϴ�.");
                                ms.AlertWarning("SFU3291"); //�Է¿��� : ���� ������� LOT���� ��ǰŸ�԰� ���� ������ LOT�� ��ǰŸ���� �ٸ��ϴ�.
                                return false;
                            }
                        }

                        if (lot_class == "CMA") //��ǰŸ�Ժ� ���� ���� ���� üũ
                        {
                            if (lot_procid == "P5000" || lot_procid == "P5500")
                            {

                            }
                            else
                            {
                                //Util.AlertInfo("���� �Ұ����� �����Դϴ�.");
                                ms.AlertWarning("SFU3388"); //���� �Ұ����� �����Դϴ�.
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
                                //Util.AlertInfo("���� �Ұ����� �����Դϴ�.");
                                ms.AlertWarning("SFU3388"); //���� �Ұ����� �����Դϴ�.
                                return false;
                            }
                        }
                        else
                        {
                            //Util.AlertInfo("���尡���� ��ǰŸ��(CMA,BMA)�� �ƴմϴ�.");
                            ms.AlertWarning("SFU3382"); //���尡���� ��ǰŸ��(CMA,BMA)�� �ƴմϴ�.
                            return false;
                        }

                        if (lot_prod != null && lot_prod != "") //���� ���� lot�� ��ǰ�� ������ ��
                        {
                            if (lot_prod != lot_prodid)
                            {
                                //Util.AlertInfo("������ ������ LOT�� ��ǰ�� �ٸ��ϴ�.");
                                ms.AlertWarning("SFU3291"); //�Է¿��� : ���� ������� LOT���� ��ǰŸ�԰� ���� ������ LOT�� ��ǰŸ���� �ٸ��ϴ�.

                                return false;
                            }
                        }

                        if (lot_eqsgid != null && lot_eqsgid != "") //���� ���� lot�� ���ΰ� ������ ��
                        {
                            if (lot_eqsgid != lot_eqsg)
                            {
                                //Util.AlertInfo("������ ������ LOT�� ���ΰ� �ٸ��ϴ�.");
                                ms.AlertWarning("SFU3383"); //������ ������ LOT�� ���ΰ� �ٸ��ϴ�.
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
                            //Util.AlertInfo("��������� ã���� �����ϴ�.");
                            ms.AlertWarning("SFU3384"); //��������� ã���� �����ϴ�.

                            return false;
                        }

                        lot_proc = dtdtPROCResult.Rows[0]["PROCID"].ToString();
                        lot_prod = lot_prodid;
                        lot_eqsgid = lot_eqsg;
                        lot_class_old = lot_class;

                        //txtBoxInfo.Text = lot_class_old + " : " + lot_eqsgid + " : " + lot_prodid + " : " + lot_proc;

                        return true;
                    }
                    else
                    {
                        //Util.AlertInfo("LOT�� ���� ���� ������ ���� �ʾ� ������ �� �����ϴ�.");
                        ms.AlertWarning("SFU3379"); //LOT�� ���� ���� ������ ���� �ʾ� ������ �� �����ϴ�.
                        return false;
                    }
                }
                else
                {
                    //Util.AlertInfo("���� �Ұ����� LOT�Դϴ�.");
                    ms.AlertWarning("SFU3404"); //
                    return false;
                }
            }
            catch (Exception ex)
            {
                txtLotId.Text = "";
                throw ex;
            }
        }

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
                dr["LOTQTY"] = dgResult.GetRowCount().ToString();
                dr["EQSGID"] = lot_eqsgid;
                dr["USERID"] = LoginInfo.USERID;

                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("BR_PRD_GET_BOXIDREQUEST", "INDATA", "OUTDATA", RQSTDT);

                if (dtResult != null && dtResult.Rows.Count != 0)
                {
                    txtBoxId.Text = dtResult.Rows[0][0].ToString();
                    //2018.08.06
                    //txtSerial.Text = dtResult.Rows[0][0].ToString();
                    //txtBatch.Text = dtResult.Rows[0][0].ToString();
                    txtSerial.Text = dtResult.Rows[0][0].ToString().Substring(9,8);
                    txtBatch.Text = dtResult.Rows[0][0].ToString().Substring(9, 8);
                    boxing_lot = txtBoxId.Text.ToString();
                    txtquantity.Text = dgResult.GetRowCount().ToString();

                }
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
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private void addGridLot()
        {
            if (boxingLot_idx == 0)
            {
                DataTable dtBoxLot = new DataTable();
                dtBoxLot.Columns.Add("LOTID", typeof(string));

                DataRow dr = dtBoxLot.NewRow();
                dr["LOTID"] = Util.GetCondition(txtLotId);

                dtBoxLot.Rows.Add(dr);

                dgResult.ItemsSource = DataTableConverter.Convert(dtBoxLot);

                //boxingYn = true;
            }
            else
            {
                int TotalRow = dgResult.GetRowCount(); //�������

                dgResult.EndNewRow(true);
                DataGridRowAdd(dgResult);

                DataTableConverter.SetValue(dgResult.Rows[TotalRow].DataItem, "LOTID", Util.GetCondition(txtLotId));
            }
            boxingLot_idx++;
            //lotCountReverse--;

            DataTable dt = DataTableConverter.Convert(dgResult.ItemsSource);

            Util.SetTextBlockText_DataGridRowCount(tbBoxingWait_cnt, Util.NVC(dt.Rows.Count));


            txtquantity.Text = dgResult.GetRowCount().ToString();
            txtLotId.SelectAll();
            txtLotId.Focus();
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
                    case "txtSupplierID":
                        //bcSupplierID.Text = tbBox.Text;
                        break;
                    case "txtSerial":
                        bcSerial.Text = tbBox.Text;
                        break;
                    case "txtBatch":
                        bcBatch.Text = tbBox.Text;
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

        private void setTexBox()
        {
            txtReceive.Text = "Viridi E-Mobility Technology";
            //txtAdvice.Text = "31607151";
            txtDock.Text = ""; // "TVV";
            txtSupplierAddress.Text = "LG Chem, Ltd";
            txtNetWeight.Text = "144";
            txtGrossWeight.Text = "163";
            txtBoxes.Text = "1";
            //txtpartNumber.Text = "MMHV1502A312A-L0";
            txtquantity.Text = "12";
            //txtSupplierID.Text = "GE2PB";
            txtSerial.Text = "BOXID";
            //txtLogistic1.Text = "logic";
            //txtLogistic2.Text = "reference";
            //txtDate.Text = "D160715";

            //2018.08.06
            //txtBatch.Text = DateTime.Now.ToString("yyMMdd"); // "161207";
            txtBatch.Text = "BOXID";
        }

        private void reSet()
        {

            boxingYn = false;
            reBoxing = false;
            palletYN = false;
            boxingLot_idx = 0;

            txtBoxId.Text = "";
            txtLotId.Text = "";

            chkBoxid.IsChecked = false;
            txtBoxId.IsReadOnly = true;

            dgResult.ItemsSource = null;
            tbBoxingWait_cnt.Text = "[ 0 " + ObjectDic.Instance.GetObjectName("��") + " ]";
            txtcnt.Text = ObjectDic.Instance.GetObjectName("������");

            lot_prod = string.Empty; //������ ������ lot�� prodid         
            lot_proc = string.Empty; //������ ������ lot�� procid
            lot_eqsgid = string.Empty; //������ ������ lot��  eqsgid
            lot_class_old = string.Empty; //������ ������ lot�� class

            box_prod = string.Empty; //���� �����ߴ� BOX�� PRODID
            boxing_lot = string.Empty; //���� �������� BOXID

            seleted_Box_Prod = string.Empty;
            seleted_Box_Procid = string.Empty;
            seleted_Box_Eqptid = string.Empty;
            seleted_Box_Eqsgid = string.Empty;

            txtSerial.Text = "BOX ID";
            //2018.08.06
            txtBatch.Text = "BOX ID";
        }
        #endregion
       
    }
}
