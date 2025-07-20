/*************************************************************************************
 Created Date : 2016.06.16
      Creator : 
   Decription : 
--------------------------------------------------------------------------------------
 [Change History]
  2016.06.16  DEVELOPER : Initial Created.
  2019.02.13 �赵�� CSR ID 3894540 �ڵ��� Pack ������Ȳ�� Prod. Type(W/O Type) ǥ�� ��û �� [��û��ȣ] C20190114_94540
  2019.02.25 �赵�� CSR ID 3894540 �ڵ��� Pack ������Ȳ�� Prod. Type(W/O Type) ǥ�� ��û �� [��û��ȣ] C20190114_94540
  2019.02.28 �赵�� CSR ID 3894540 �ڵ��� Pack ������Ȳ�� Prod. Type(W/O Type) ǥ�� ��û �� [��û��ȣ] C20190114_94540
**************************************************************************************/

using System;
using System.Data;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using C1.WPF;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using Microsoft.Win32;
using System.Configuration;
using System.IO;

namespace LGC.GMES.MES.PACK001
{
    public partial class PACK001_012 : UserControl, IWorkArea
    {
        #region Declaration & Constructor 
        LGC.GMES.MES.PACK001.Class.MESSAGE_PARAM ms = new LGC.GMES.MES.PACK001.Class.MESSAGE_PARAM();
        DataTable dtResult;
        DataTable dtFindResult;
        string cell_info = string.Empty;
        CommonCombo _combo = new CommonCombo();

        private DataTable dtReturnProcess = null;
        private DataTable dtReturnProcess1 = null;
        bool search_fullCheck = false;
        bool lot_fullCheck = false;

        #region LOT SCAN�� ó�� ����
        DataTable dtLotResult;
        string pre_procid_cause = string.Empty; // ���� ���� LOT�� ���ΰ���
        string pre_proctype = string.Empty;     // ���� ���� LOT�� ����Ÿ�� : R(��������), S(������)
        string pre_procid = string.Empty;       // ���� ���� LOT�� ����
        string pre_eqsgid = string.Empty;       // ���� ���� LOT�� ����
        string statusvalue = string.Empty;      // ���� ���� LOT�� status : REWORK_WAIT(���۾����), SCRAP_WAIT(�����)
        #endregion

        public IFrameOperation FrameOperation
        {
            get;
            set;
        }

        public PACK001_012()
        {
            InitializeComponent();

            this.Loaded += PACK001_012_Loaded;
        }

        #endregion

        #region Initialize
        private void Initialize()
        {
            InitCombo();
        }

        private void InitCombo()
        {
            try
            {
                dtpDateFrom.SelectedDateTime = DateTime.Now.AddDays(-7);
                dtpDateTo.SelectedDateTime = DateTime.Now;

                this.cboListCount.SelectedValueChanged -= new System.EventHandler<C1.WPF.PropertyChangedEventArgs<object>>(this.cboListCount_SelectedValueChanged);
                Util.Set_Pack_cboListCoount(cboListCount, "CBO_NAME", "CBO_CODE", 100, 1000, 100);
                this.cboListCount.SelectedValueChanged += new System.EventHandler<C1.WPF.PropertyChangedEventArgs<object>>(this.cboListCount_SelectedValueChanged);

                C1ComboBox cboSHOPID = new C1ComboBox();
                cboSHOPID.SelectedValue = LoginInfo.CFG_SHOP_ID;
                C1ComboBox cboAreaByAreaType = new C1ComboBox();
                cboAreaByAreaType.SelectedValue = LoginInfo.CFG_AREA_ID;
                C1ComboBox cboAREA_TYPE_CODE = new C1ComboBox();
                cboAREA_TYPE_CODE.SelectedValue = Area_Type.PACK;
                C1ComboBox cboACTID = new C1ComboBox();
                cboACTID.SelectedValue = "DEFECT_LOT";

                #region ��ȸ���� �޺�
                //����            
                C1ComboBox[] cboEquipmentSegmentParent = { cboAreaByAreaType };
                C1ComboBox[] cboEquipmentSegmentChild = { cboProductModel, cboProcessPack };
                _combo.SetCombo(cboEquipmentSegment, CommonCombo.ComboStatus.ALL, cbParent: cboEquipmentSegmentParent, cbChild: cboEquipmentSegmentChild);

                //��          
                C1ComboBox[] cboProductModelParent = { cboAreaByAreaType, cboEquipmentSegment };
                C1ComboBox[] cboProductModelChild = { cboProduct };
                _combo.SetCombo(cboProductModel, CommonCombo.ComboStatus.ALL, cbParent: cboProductModelParent, cbChild: cboProductModelChild, sCase: "PRJ_MODEL");

                //��ǰ�з�(PACK ��ǰ �з�)           
                C1ComboBox[] cboPrdtClassParent = { cboSHOPID, cboAreaByAreaType, cboEquipmentSegment, cboAREA_TYPE_CODE };
                C1ComboBox[] cboPrdtClassChild = { cboProduct };
                //C1ComboBox[] cboPrdtClassParent = { cboSHOPID, cboAreaByAreaType, cboEquipmentSegment, cboAREA_TYPE_CODE };
                _combo.SetCombo(cboPrdtClass, CommonCombo.ComboStatus.ALL, cbParent: cboPrdtClassParent, cbChild: cboPrdtClassChild);

                //��ǰ�ڵ�  
                C1ComboBox[] cboProductParent = { cboSHOPID, cboAreaByAreaType, cboEquipmentSegment, cboProductModel, cboPrdtClass };
                _combo.SetCombo(cboProduct, CommonCombo.ComboStatus.ALL, cbParent: cboProductParent, sCase: "PRJ_PRODUCT");

                //�ҷ� ���� : ���ð��� �ƴ� �ʼ������� defalut�� ������ �ϳ��� ����.
                //getProcess_cbo(cboProcess);

                //���԰��� : loading�� �ҷ� ������ �����ϰ� �Ѹ��� ��ȸ �� �ٽ� �ѷ���
                //getProcess_cbo(cboReworkReturnProcess);

                //�ҷ��߻�����
                C1ComboBox[] cboProcessParent = { cboEquipmentSegment };
                _combo.SetCombo(cboProcessPack, CommonCombo.ComboStatus.SELECT, cbParent: cboProcessParent);

                //���� : �������,�����            
                C1ComboBox[] cboStatusChild = { cboWork };
                _combo.SetCombo(cboStatus, CommonCombo.ComboStatus.NONE, cbChild: cboStatusChild);
                //_combo.SetCombo(cboStatus, CommonCombo.ComboStatus.NONE);
                #endregion

                #region �۾����� �޺�
                //�ϰ��۾� cbo ����
                C1ComboBox[] cboReworkResultParent = { cboStatus };
                _combo.SetCombo(cboWork, CommonCombo.ComboStatus.NONE, cbParent: cboReworkResultParent);

                //2019.02.28
                cboWork.SelectedIndex = 1;

                //cboStatus.SelectedItemChanged -= cboStatus_SelectedValueChanged;


                //���԰���
                //C1ComboBox[] cboReworkReturnProcessParent = { cboProcess };            
                //_combo.SetCombo(cboReworkReturnProcess, CommonCombo.ComboStatus.ALL, cbParent: cboReworkReturnProcessParent);

                //���� : textBox�� �ٲ�.
                //C1ComboBox[] cboReasonParent = { cboEquipmentSegment, cboProcess };
                //string[] resonFilter = { Util.GetCondition(cboStatus) == "REWORK_JUDGE" ? "REPAIR_LOT" : "DEFECT_LOT", "P" };
                //_combo.SetCombo(cboReason, CommonCombo.ComboStatus.ALL, cbParent: cboReasonParent, sFilter: resonFilter);

                //��å�μ�
                String[] sImpute = { "RESP_DEPT" };
                _combo.SetCombo(cboScrapIMPUTE_CODE, CommonCombo.ComboStatus.NONE, sFilter: sImpute, sCase: "COMMCODE");

                //��å�μ�               
                _combo.SetCombo(cboScrapIMPUTE_CODE1, CommonCombo.ComboStatus.NONE, sFilter: sImpute, sCase: "COMMCODE");

                //��뱸��
                _combo.SetCombo(cboCostType, CommonCombo.ComboStatus.SELECT, sCase: "cboDefectType");
                _combo.SetCombo(cboCostType1, CommonCombo.ComboStatus.SELECT, sCase: "cboDefectType");
                //cboCostType.SelectedIndex = 0;

                //�����ڵ�
                //_combo.SetCombo(cboDefectChoice, CommonCombo.ComboStatus.NONE, sCase: "cboDefectChoice");
                //_combo.SetCombo(cboDefectChoice1, CommonCombo.ComboStatus.NONE, sCase: "cboDefectChoice");
                setComboBox_DefectChoice(cboCostType, CommonCombo.ComboStatus.SELECT);
                setComboBox_DefectChoice1(cboCostType, CommonCombo.ComboStatus.SELECT);
                cboDefectChoice.IsEnabled = false;
                cboDefectChoice1.IsEnabled = false;

                //����
                //testSet_Cbo1(cboStatus, "1", "�������");
                #endregion

                //cboStatus.SelectedItemChanged -= cboStatus_SelectedValueChanged;
                //cboWork.SelectedItemChanged -= cboWork_SelectedValueChanged;
                //cboProcessPack.SelectedValueChanged -= cboProcessPack_SelectedValueChanged;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        #endregion

        #region Event
        private void PACK001_012_Loaded(object sender, RoutedEventArgs e)
        {
            this.Loaded -= PACK001_012_Loaded;
            cboProcessPack.SelectedValueChanged -= cboProcessPack_SelectedValueChanged;
            cboWork.SelectedValueChanged -= cboWork_SelectedValueChanged;
            cboWork1.SelectedValueChanged -= cboWork1_SelectedValueChanged;

            Initialize();
            //getSearch();
            txtLotID.Focus();

            cboProcessPack.SelectedValueChanged += cboProcessPack_SelectedValueChanged;
            cboWork.SelectedValueChanged += cboWork_SelectedValueChanged;
            cboWork1.SelectedValueChanged += cboWork1_SelectedValueChanged;
        }

        #region Combo
        private void cboWork_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            try
            {
                if (cboWork.Items.Count == 0)
                {
                    return;
                }

                if (cboStatus.SelectedValue.ToString() == "REWORK_WAIT") //REWORK_WAIT : ���۾����
                {
                    if (cboWork.SelectedValue.ToString() == "REWORK") //REWORK : ���۾�
                    {
                        cboReworkReturnProcess.Visibility = Visibility.Visible;
                        txtReworkReturnProcess.Visibility = Visibility.Hidden;
                        //2019.02.07
                        cboDefectChoice.SelectedIndex = -1;
                        cboDefectChoice.IsEnabled = false;
                        cboCostType.SelectedIndex = 0;
                        cboCostType.IsEnabled = false;
                    }
                    else //SCRAP_WAIT : �����
                    {
                        cboReworkReturnProcess.Visibility = Visibility.Hidden;
                        txtReworkReturnProcess.Visibility = Visibility.Visible;
                        txtReworkReturnProcess.Text = ObjectDic.Instance.GetObjectName("�����");
                        txtReworkReturnProcess.IsReadOnly = true;
                        //2019.02.07
                        cboCostType.SelectedIndex = 0;
                        cboCostType.IsEnabled = true;
                        cboDefectChoice.IsEnabled = true;
                    }
                }
                else // SCRAP_WAIT : �����
                {
                    if (cboWork.SelectedValue.ToString() == "REWORK_WAIT") //REWORK_WAIT : �������
                    {
                        cboReworkReturnProcess.Visibility = Visibility.Hidden;
                        txtReworkReturnProcess.Visibility = Visibility.Visible;
                        txtReworkReturnProcess.Text = ObjectDic.Instance.GetObjectName("����/���۾�");
                        txtReworkReturnProcess.IsReadOnly = true;
                        //2019.02.07
                        cboDefectChoice.SelectedIndex = -1;
                        cboDefectChoice.IsEnabled = false;
                        cboCostType.SelectedIndex = 0;
                        cboCostType.IsEnabled = false;
                    }
                    else // //SCRAP_WAIT : �����
                    {
                        //cboReworkReturnProcess.IsEnabled = false;
                        cboReworkReturnProcess.Visibility = Visibility.Hidden;
                        txtReworkReturnProcess.Visibility = Visibility.Visible;
                        txtReworkReturnProcess.Text = ObjectDic.Instance.GetObjectName("�����");
                        txtReworkReturnProcess.IsReadOnly = true;
                        //2019.02.07
                        cboCostType.SelectedIndex = 0;
                        cboCostType.IsEnabled = true;
                        cboDefectChoice.IsEnabled = true;
                    }
                }

                getSearch();
            }
            catch(Exception ex)
            {
                Util.MessageException(ex);
            }            
        }

        private void cboProcessPack_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
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

        private void cboWork1_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            if (cboWork1.Items.Count == 0)
            {
                return;
            }

            if (statusvalue == "REWORK_WAIT") //REWORK_WAIT : ���۾����
            {
                if (cboWork1.SelectedValue.ToString() == "REWORK") //REWORK : ���۾�
                {
                    cboReworkReturnProcess1.Visibility = Visibility.Visible;
                    txtReworkReturnProcess1.Visibility = Visibility.Hidden;
                    //2019.02.22
                    cboDefectChoice1.SelectedIndex = -1;
                    cboDefectChoice1.IsEnabled = false;
                }
                else //SCRAP_WAIT : �����
                {
                    cboReworkReturnProcess1.Visibility = Visibility.Hidden;
                    txtReworkReturnProcess1.Visibility = Visibility.Visible;
                    txtReworkReturnProcess1.Text = ObjectDic.Instance.GetObjectName("�����");
                    txtReworkReturnProcess1.IsReadOnly = true;
                    //2019.02.22
                    cboDefectChoice1.IsEnabled = true;
                }
            }
            else // SCRAP_WAIT : �����
            {
                if (cboWork1.SelectedValue.ToString() == "REWORK_WAIT") //REWORK_WAIT : �������
                {
                    cboReworkReturnProcess1.Visibility = Visibility.Hidden;
                    txtReworkReturnProcess1.Visibility = Visibility.Visible;
                    txtReworkReturnProcess1.Text = ObjectDic.Instance.GetObjectName("����/���۾�");
                    txtReworkReturnProcess1.IsReadOnly = true;
                    //2019.02.22
                    cboDefectChoice1.SelectedIndex = -1;
                    cboDefectChoice1.IsEnabled = false;
                }
                else // //SCRAP_WAIT : �����
                {
                    //cboReworkReturnProcess.IsEnabled = false;
                    cboReworkReturnProcess1.Visibility = Visibility.Hidden;
                    txtReworkReturnProcess1.Visibility = Visibility.Visible;
                    txtReworkReturnProcess1.Text = ObjectDic.Instance.GetObjectName("�����");
                    txtReworkReturnProcess1.IsReadOnly = true;
                    //2019.02.22
                    cboDefectChoice1.IsEnabled = true;
                }
            }
        }

        private void cboListCount_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {

        }

        private void cboCostTcboWork1_SelectedValueChangedype_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            if (Util.NVC(cboWork.SelectedValue) == "SCRAP")
            {
                cboDefectChoice1.IsEnabled = true;
                cboCostType1.IsEnabled = true;
            }
            else
            {
                cboCostType.SelectedIndex = 0;
                cboDefectChoice1.IsEnabled = false;
                cboCostType1.IsEnabled = false;
            }
        }

        private void cboCostType_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            if (Util.NVC(cboWork.SelectedValue) == "SCRAP")
            {
                //����ϰ�� ������.
                //if (txtReason != null)
                //{
                //cboDefectChoice.SelectedIndex = -1;
                if (cboCostType.SelectedValue.ToString() != "SELECT")
                {
                    setComboBox_DefectChoice(cboCostType, CommonCombo.ComboStatus.SELECT);
                    cboDefectChoice.IsEnabled = true;
                    cboDefectChoice.SelectedIndex = 0;
                }

                //}
            }
            else
            {
                //���۾��ϰ�� �н�.
                //cboDefectChoice.SelectedIndex = 0;
            }
        }

        private void cboDefectChoice_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {

        }

        private void cboCostType1_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            //2019.02.28
            //if (Util.NVC(cboWork1.SelectedValue) == "SCRAP_WAIT")
            if (Util.NVC(cboWork1.SelectedValue) == "SCRAP")
            {
                    //����ϰ�� ������.
                    //if (txtReason1 != null)
                    //{
                    if (cboCostType1.SelectedValue.ToString() != "SELECT")
                    {
                        setComboBox_DefectChoice1(cboCostType1, CommonCombo.ComboStatus.SELECT);
                        cboDefectChoice1.IsEnabled = true;
                        cboDefectChoice1.SelectedIndex = 0;
                    }
                        //setComboBox_DefectChoice1(cboCostType1, CommonCombo.ComboStatus.SELECT);
                    //}
            }
            else
            {
                //���۾��ϰ�� �н�.
                //cboDefectChoice.SelectedIndex = 0;
            }
        }

        private void cboDefectChoice1_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {

        }

        #endregion Combo

        #region Button
        private void btnAllEnd_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(ms.AlertRetun("SFU1744"), null, "Information", MessageBoxButton.OKCancel, MessageBoxIcon.None, (sResult) =>
                //�ϰ�ó�� �Ͻðڽ��ϱ�?
                {
                    if (sResult == MessageBoxResult.OK)
                    {
                        if (tcMain.SelectedIndex == 0)
                        {
                            endProcess();
                        }
                        else
                        {
                            endProcess_lot();
                        }
                    }
                });
                
            }
            catch (Exception ex)
            {
                //Util.AlertInfo(ex.Message);
                Util.MessageException(ex);
            }
        }

        private void btnExcel_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                C1.WPF.DataGrid.C1DataGrid dg = new C1.WPF.DataGrid.C1DataGrid();
                if (tcMain.SelectedIndex == 0)
                {
                    dg = dgSearchResult;
                }
                else
                {
                    dg = dgSearchResult1;
                }

                if (dg.GetRowCount() == 0)
                {
                    return;
                }

                new ExcelExporter().Export(dg);
            }
            catch (Exception ex)
            {
                Util.Alert(ex.Message);
            }
        }

        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                //���� �޺� �� ��ȸ(������ �ٲ�� ���ο� �ɷ��ִ� �������� �ٽ� ������)
                //string[] PROC = { cboEquipmentSegment.SelectedValue.ToString() };
                //_combo.SetCombo(cboReason, CommonCombo.ComboStatus.ALL, sFilter: PROC);
                if (Util.GetCondition(cboProcessPack) == "")
                {
                    ms.AlertWarning("SFU1458"); //���������� �ٽ� ���� �ϼ���
                    return;
                }

                dgSearchResult.ItemsSource = null;
                getSearch();

                txtReason.Text = "";
            }
            catch (Exception ex)
            {
                Util.AlertInfo(ex.Message);
            }
        }

        private void btnAllSelect_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                C1.WPF.DataGrid.C1DataGrid dg = new C1.WPF.DataGrid.C1DataGrid();
                bool fullCheck;

                if (tcMain.SelectedIndex == 0)
                {
                    dg = dgSearchResult;
                    fullCheck = search_fullCheck;
                }
                else
                {
                    dg = dgSearchResult1;
                    fullCheck = lot_fullCheck;
                }

                if (dg.GetRowCount() == 0)
                {
                    return;
                }
                else
                {
                    if (tcMain.SelectedIndex == 0)
                    {
                        fullCheck = search_fullCheck;
                    }
                    else
                    {
                        fullCheck = lot_fullCheck;
                    }
                }

                DataTable dt = DataTableConverter.Convert(dg.ItemsSource);
                Button btn = sender as Button;

                if (fullCheck == false)
                {
                    for (int i = 0; i < dt.Rows.Count; i++)
                    {
                        dt.Rows[i][0] = true;
                    }
                    fullCheck = true;
                    btn.Content = ObjectDic.Instance.GetObjectName("��ü����");
                    btn.Foreground = Brushes.Red;
                }
                else
                {
                    for (int i = 0; i < dt.Rows.Count; i++)
                    {
                        dt.Rows[i][0] = false;
                    }
                    fullCheck = false;
                    btn.Content = ObjectDic.Instance.GetObjectName("��ü����");
                    btn.Foreground = Brushes.White;
                }

                if (tcMain.SelectedIndex == 0)
                {
                    search_fullCheck = fullCheck;
                }
                else
                {
                    lot_fullCheck = fullCheck;
                }

                SetBinding(dg, dt);
            }
            catch (Exception ex)
            {
                Util.AlertInfo(ex.Message);
            }
        }

        private void btnSelectCancel1_Click(object sender, RoutedEventArgs e)
        {
            if (dgSearchResult1.ItemsSource != null)
            {
                for (int i = dgSearchResult1.GetRowCount(); 0 < i; i--)
                {
                    var chkYn = DataTableConverter.GetValue(dgSearchResult1.Rows[i - 1].DataItem, "CHK");
                    var lot_id = DataTableConverter.GetValue(dgSearchResult1.Rows[i - 1].DataItem, "LOTID");

                    if (chkYn == null)
                    {
                        dgSearchResult1.RemoveRow(i - 1);
                    }
                    else if (Convert.ToBoolean(chkYn))
                    {
                        dgSearchResult1.EndNewRow(true);
                        dgSearchResult1.RemoveRow(i - 1);
                    }
                }

                if (dgSearchResult1.GetRowCount() == 0)
                {
                    tbState.Text = "";
                    lot_fullCheck = false;
                    btnAllSelect1.Content = ObjectDic.Instance.GetObjectName("��ü����");
                    btnAllSelect1.Foreground = Brushes.White;
                }

                DataTable dt = DataTableConverter.Convert(dgSearchResult1.ItemsSource);

                Util.SetTextBlockText_DataGridRowCount(tbSearch_cnt1, Util.NVC(dt.Rows.Count));
            }
        }

        //EXCEL UPLOAD
        private void btnExcelUpload_Click(object sender, RoutedEventArgs e)
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

        #endregion Button

        #region Text
        private void txtLotID_KeyDown(object sender, KeyEventArgs e)
        {
            try
            {
                if (e.Key == Key.Enter)
                {
                    if (txtLotID.Text != "")
                    {
                        //cell id validation//
                        //1. wip�̷¿��� ��ȸ : ����Ȯ��, ��Ȯ��
                        if (!getLotSearch())
                        {
                            ms.AlertWarning("SFU2025"); //�ش��ϴ� LOT������ �����ϴ�.
                            txtLotID.Text = "";
                            txtLotID.Focus();
                            return;
                        }

                        if (dgSearchResult1.GetRowCount() > 0)
                        {
                            string procid_cause = Util.NVC(dtLotResult.Rows[0]["PROCID_CAUSE"]);
                            string proctype = Util.NVC(dtLotResult.Rows[0]["PROCTYPE"]);
                            string eqsgid = Util.NVC(dtLotResult.Rows[0]["EQSGID"]);

                            if (pre_procid_cause != procid_cause)
                            {
                                //Util.AlertInfo("���� ���� ���ΰ���(" + pre_procid_cause + ")�� �ٸ� ���ΰ���(" + procid_cause  + ")�� �ִ� LOT�Դϴ�.");
                                ms.AlertWarning("SFU3297", pre_procid_cause, procid_cause); // �Է¿��� : �۾� ������� LOT�� ���ΰ��� %1�� �Է��� LOT�� ���ΰ��� %2�� �ٸ��ϴ�.
                                return;
                            }

                            if (pre_proctype != proctype)
                            {
                                //Util.AlertInfo("���� ���� ����(" + pre_procid + ")�� �ٸ� ����(" + Util.NVC(dtLotResult.Rows[0]["PROCID"]).ToString() + ")�� �ִ� LOT�Դϴ�.");
                                ms.AlertWarning("SFU3297", pre_procid, Util.NVC(dtLotResult.Rows[0]["PROCID"]).ToString()); // �Է¿��� : �۾� ������� LOT�� ���ΰ��� %1�� �Է��� LOT�� ���ΰ��� %2�� �ٸ��ϴ�.
                                return;
                            }

                            if (pre_eqsgid != eqsgid)
                            {
                                //Util.AlertInfo("���� ���� ����(" + pre_eqsgid + ")�� �ٸ� ����(" + eqsgid + ")�� �ִ� LOT�Դϴ�.");
                                ms.AlertWarning("SFU3299", pre_eqsgid, eqsgid); // �Է¿��� : �۾� ������� LOT�� ���� %1�� �Է��� LOT�� ���� %2�� �ٸ��ϴ�.
                                return;
                            }
                        }

                        int TotalRow = dgSearchResult1.GetRowCount();

                        for (int i = 0; i < TotalRow; i++)
                        {
                            string grid_id = DataTableConverter.GetValue(dgSearchResult1.Rows[i].DataItem, "LOTID").ToString();

                            if (txtLotID.Text == grid_id)
                            {
                                ms.AlertWarning("SFU1513"); //��ϵ� LOT ID �Դϴ�. Ȯ�� �� �ٽ� ����� �ּ���.

                                txtLotID.Text = "";
                                txtLotID.Focus();
                                return;
                            }
                        }

                        lotGridAdd(TotalRow); //�׸��忡 �߰�

                        DataTable dt = DataTableConverter.Convert(dgSearchResult1.ItemsSource);

                        Util.SetTextBlockText_DataGridRowCount(tbSearch_cnt1, Util.NVC(dt.Rows.Count));

                        txtLotID.Text = "";
                        txtLotID.Focus();
                    }
                }
            }
            catch (Exception ex)
            {
                //Util.AlertInfo(ex.Message);
                Util.MessageException(ex);
            }
        }

        #endregion Text

        private void endProcess()
        {
            try
            {
                if (dgSearchResult== null)
                {
                    return;
                }

                int chk_idx = 0;
                for (int i = 0; i < dgSearchResult.GetRowCount(); i++)
                {
                    if (DataTableConverter.GetValue(dgSearchResult.Rows[i].DataItem, "CHK").ToString() == "True")
                    {
                        chk_idx++;
                    }
                }

                if (chk_idx == 0)
                {
                    return;
                }

                if (Util.GetCondition(cboProcessPack) == "")
                {                    
                    ms.AlertWarning("SFU1459"); //������ �����ϼ���.
                    return;
                }

                if (txtReason.Text.Length == 0)
                {
                    ms.AlertWarning("SFU1594"); //������ �Է��ϼ���.
                    return;
                }

                //BIZ���� MULTIL�� ó��
                lotEnd_multi();
            }
            catch(Exception ex)
            {
                throw ex;
            }
        }

        private void endProcess_lot()
        {
            try
            {
                if (dgSearchResult1 == null)
                {
                    return;
                }

                int chk_idx = 0;
                for (int i = 0; i < dgSearchResult1.GetRowCount(); i++)
                {
                    if (DataTableConverter.GetValue(dgSearchResult1.Rows[i].DataItem, "CHK").ToString() == "True")
                    {
                        chk_idx++;
                    }
                }

                if (chk_idx == 0)
                {
                    return;
                }
                if (txtReason1.Text.Length == 0)
                {
                    ms.AlertWarning("SFU1594"); //������ �Է��ϼ���.
                    return;
                }

                if(cboScrapIMPUTE_CODE1 == null)
                {
                   // Util.AlertInfo("��å�μ��� �����ϼ���");
                    ms.AlertWarning("SFU3296"); //���ÿ��� : ��å�μ�(�ʼ�����) �޺��� �������� �ʾҽ��ϴ�.[�޺�����]
                    return;
                }

                //BIZ���� MULTIL�� ó��
                lotEnd_multi_SCAN();
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private void dgSearchResult_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            try
            {
                Point pnt = e.GetPosition(null);
                C1.WPF.DataGrid.DataGridCell cell = dgSearchResult.GetCellFromPoint(pnt);

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
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }

        private void dgSearchResult1_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            try
            {
                Point pnt = e.GetPosition(null);
                C1.WPF.DataGrid.DataGridCell cell = dgSearchResult1.GetCellFromPoint(pnt);

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
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }

        private void tcMain_ItemsChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (tcMain.SelectedIndex == 1)
            {
                txtLotID.Focus();
            }
        }

        private void tcMain_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (tcMain.SelectedIndex == 1)
            {
                txtLotID.Focus();
            }
        }

        private void SetBinding(C1.WPF.DataGrid.C1DataGrid dg, DataTable dt)
        {
            dg.ItemsSource = DataTableConverter.Convert(dt);
        }

        #endregion Event

        #region Mehod

        private void lotEnd(object obj)
        {
            try
            {
                if (obj == null)
                {
                    return;
                }

                DataRow drv = obj as DataRow;

                //resncode �������ΰ��� OK
                string sResnCode = string.Empty;
                if (Util.GetCondition(cboWork) == "REWORK_JUDGE")
                {
                    sResnCode = "OK";
                }
                else
                {
                    if (txtReason.Text.Length > 0)
                    {
                        sResnCode = Util.GetCondition(txtReason);
                    }
                    else
                    {
                        sResnCode = "NG";
                    }
                }

                DataSet dsInput = new DataSet();
                DataTable INDATA = new DataTable();
                INDATA.TableName = "INDATA";
                INDATA.Columns.Add("SRCTYPE", typeof(string));
                INDATA.Columns.Add("LANGID", typeof(string));
                INDATA.Columns.Add("LOTID", typeof(string));
                INDATA.Columns.Add("END_PROCID", typeof(string));
                INDATA.Columns.Add("END_EQPTID", typeof(string));
                INDATA.Columns.Add("STRT_PROCID", typeof(string));
                INDATA.Columns.Add("STRT_EQPTID", typeof(string));
                INDATA.Columns.Add("USERID", typeof(string));
                INDATA.Columns.Add("WIPNOTE", typeof(string));
                INDATA.Columns.Add("RESNCODE", typeof(string));
                INDATA.Columns.Add("RESNNOTE", typeof(string));
                INDATA.Columns.Add("RESNDESC", typeof(string));

                DataRow drINDATA = INDATA.NewRow();
                drINDATA["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                drINDATA["LANGID"] = LoginInfo.LANGID;
                drINDATA["LOTID"] = drv["LOTID"].ToString();
                drINDATA["END_PROCID"] = drv["PROCID"].ToString(); //����ҷ�����
                drINDATA["END_EQPTID"] = drv["EQPTID"].ToString();
                drINDATA["STRT_PROCID"] = Util.GetCondition(cboReworkReturnProcess) == "" ? null : cboReworkReturnProcess.SelectedValue; //��������
                drINDATA["STRT_EQPTID"] = null;
                drINDATA["USERID"] = LoginInfo.USERID;
                drINDATA["WIPNOTE"] = null;
                drINDATA["RESNCODE"] = sResnCode;
                drINDATA["RESNNOTE"] = Util.GetCondition(txtReason);  //Util.NVC(cboReason.DisplayMemberPath) == "" ? null : cboReason.DisplayMemberPath;
                drINDATA["RESNDESC"] = Util.GetCondition(cboScrapIMPUTE_CODE) == "" ? null : cboScrapIMPUTE_CODE.SelectedValue;
                INDATA.Rows.Add(drINDATA);

                dsInput.Tables.Add(INDATA);

                DataTable IN_CLCTITEM = new DataTable();
                IN_CLCTITEM.TableName = "IN_CLCTITEM";
                IN_CLCTITEM.Columns.Add("CLCTITEM", typeof(string));
                IN_CLCTITEM.Columns.Add("CLCTVAL", typeof(string));
                IN_CLCTITEM.Columns.Add("PASSYN", typeof(string));
                dsInput.Tables.Add(IN_CLCTITEM);

                DataTable IN_CLCTDITEM = new DataTable();
                IN_CLCTDITEM.TableName = "IN_CLCTDITEM";
                IN_CLCTDITEM.Columns.Add("CLCTDITEM", typeof(string));
                IN_CLCTDITEM.Columns.Add("CLCTDVAL", typeof(string));
                dsInput.Tables.Add(IN_CLCTDITEM);

                DataSet dsResult = new ClientProxy().ExecuteServiceSync_Multi("BR_PRD_REG_OUTPUTASSY", "INDATA,IN_CLCTITEM,IN_CLCTDITEM", "OUTDATA", dsInput, null);
                //BR_PRD_REG_OUTPUTASSY_MULTI
                //BR_PRD_REG_OUTPUTASSY_MULTILOT_INPUT

                if (dsResult.Tables["OUTDATA"] != null)
                {
                    if (dsResult.Tables["OUTDATA"].Rows.Count > 0)
                    {                        
                        ms.AlertInfo("SFU1275"); //����ó�� �Ǿ����ϴ�.
                        getSearch();
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private void lotEnd_multi()
        {
            try
            {
                //resncode �������ΰ��� OK
                string sResnCode = string.Empty;
                sResnCode = Util.GetCondition(cboWork);
                //if (Util.GetCondition(cboWork) == "REWORK_JUDGE")
                //{
                //    sResnCode = "OK";
                //}
                //else
                //{
                //    sResnCode = "NG";   
                //}

                DataSet dsInput = new DataSet();

                DataTable INDATA = new DataTable();
                INDATA.TableName = "INDATA";
                INDATA.Columns.Add("SRCTYPE", typeof(string));
                INDATA.Columns.Add("LANGID", typeof(string));
                INDATA.Columns.Add("LOTID", typeof(string));
                INDATA.Columns.Add("END_PROCID", typeof(string));
                INDATA.Columns.Add("END_EQPTID", typeof(string));
                INDATA.Columns.Add("STRT_PROCID", typeof(string));
                INDATA.Columns.Add("STRT_EQPTID", typeof(string));
                INDATA.Columns.Add("USERID", typeof(string));
                INDATA.Columns.Add("WIPNOTE", typeof(string));
                INDATA.Columns.Add("RESNCODE", typeof(string));
                INDATA.Columns.Add("RESNNOTE", typeof(string));
                INDATA.Columns.Add("RESNDESC", typeof(string));
                //2019.02.13
                INDATA.Columns.Add("RESNDEFT", typeof(string));


                //�������� ã��
                string nextProcessId = string.Empty;
                if (cboStatus.SelectedValue.ToString() == "REWORK_WAIT") //REWORK_WAIT : ���۾����
                {
                    if (cboWork.SelectedValue.ToString() == "REWORK") //REWORK : ���۾�
                    {
                        nextProcessId = Util.GetCondition(cboReworkReturnProcess) == "" ? null : cboReworkReturnProcess.SelectedValue.ToString();  //�������� ��
                    }
                    else
                    {
                        nextProcessId = "PS000"; //������
                    }
                }
                else
                {
                    if (cboWork.SelectedValue.ToString() == "REWORK_WAIT") //REWORK_WAIT : �������
                    {
                        nextProcessId = "PR000"; //��������                       
                    }
                    else
                    {
                        nextProcessId = null; //������
                    }
                }

                for (int i = 0; i < dgSearchResult.Rows.Count; i++)
                {
                    if (DataTableConverter.GetValue(dgSearchResult.Rows[i].DataItem, "CHK").ToString() == "True")
                    {
                        //C1.WPF.DataGrid.DataGridRow dgr = dgSearchResult.Rows[i] as C1.WPF.DataGrid.DataGridRow;
                        //DataRowView drv = dgr.DataItem as DataRowView;
                        DataRow drv = dtResult.Rows[i] as DataRow;

                        DataRow drINDATA = INDATA.NewRow();
                        drINDATA["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                        drINDATA["LANGID"] = LoginInfo.LANGID;
                        drINDATA["LOTID"] = drv["LOTID"].ToString();
                        drINDATA["END_PROCID"] = drv["PROCID"].ToString(); //���� ����
                        drINDATA["END_EQPTID"] = drv["EQPTID"].ToString();
                        drINDATA["STRT_PROCID"] = nextProcessId; //��������
                        drINDATA["STRT_EQPTID"] = null;
                        drINDATA["USERID"] = LoginInfo.USERID;
                        drINDATA["WIPNOTE"] = null;
                        drINDATA["RESNCODE"] = sResnCode; // drv["RESNCODE"].ToString(); ; // drv["RESNCODE"]; //sResnCode;
                        drINDATA["RESNNOTE"] = Util.GetCondition(txtReason); //Util.NVC(cboReason.DisplayMemberPath) == "" ? null : cboReason.DisplayMemberPath;
                        drINDATA["RESNDESC"] = Util.GetCondition(cboScrapIMPUTE_CODE) == "" ? null : cboScrapIMPUTE_CODE.SelectedValue;
                        //2019.02.13
                        drINDATA["RESNDEFT"] = Util.GetCondition(cboDefectChoice) == "" ? null : cboDefectChoice.SelectedValue;
                        INDATA.Rows.Add(drINDATA);
                    }
                }

                dsInput.Tables.Add(INDATA);

                DataTable IN_CLCTITEM = new DataTable();
                IN_CLCTITEM.TableName = "IN_CLCTITEM";
                IN_CLCTITEM.Columns.Add("CLCTITEM", typeof(string));
                IN_CLCTITEM.Columns.Add("CLCTVAL", typeof(string));
                IN_CLCTITEM.Columns.Add("PASSYN", typeof(string));
                dsInput.Tables.Add(IN_CLCTITEM);

                DataTable IN_CLCTDITEM = new DataTable();
                IN_CLCTDITEM.TableName = "IN_CLCTDITEM";
                IN_CLCTDITEM.Columns.Add("CLCTDITEM", typeof(string));
                IN_CLCTDITEM.Columns.Add("CLCTDVAL", typeof(string));
                dsInput.Tables.Add(IN_CLCTDITEM);

                //DataSet dsResult = new ClientProxy().ExecuteServiceSync_Multi("BR_PRD_REG_OUTPUTASSY_MULTILOT_INPUT", "INDATA,IN_CLCTITEM,IN_CLCTDITEM", "OUTDATA", dsInput, null);
                //DataSet dsResult = new ClientProxy().ExecuteServiceSync_Multi("BR_PRD_REG_OUTPUTASSY_MULTILOT", "INDATA,IN_CLCTITEM,IN_CLCTDITEM", "OUTDATA", dsInput, null);

                loadingIndicator.Visibility = Visibility.Visible;

                new ClientProxy().ExecuteService_Multi("BR_PRD_REG_OUTPUTASSY_MULTILOT_DEFECT", "INDATA,IN_CLCTITEM,IN_CLCTDITEM", "OUTDATA", (dsResult, dataException) =>
                {
                    try
                    {
                        if (dataException != null)
                        {
                            //Util.AlertByBiz("BR_PRD_REG_OUTPUTASSY_MULTILOT_DEFECT", dataException.Message, dataException.ToString());
                            Util.MessageException(dataException);
                            loadingIndicator.Visibility = Visibility.Collapsed;
                            return;
                        }
                        else
                        {
                            loadingIndicator.Visibility = Visibility.Collapsed;
                            if (dsResult.Tables["OUTDATA"] != null)
                            {
                                ms.AlertInfo("SFU1275"); //����ó�� �Ǿ����ϴ�.
                                getSearch();                             
                            }
                        }

                        return;
                    }
                    catch (Exception ex)
                    {
                        loadingIndicator.Visibility = Visibility.Collapsed;
                        throw ex;
                    }

                }, dsInput);

                
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private void lotEnd_multi_SCAN()
        {
            try
            {
                string sResnCode = string.Empty;
                sResnCode = Util.GetCondition(cboWork1);

                DataSet dsInput = new DataSet();

                DataTable INDATA = new DataTable();
                INDATA.TableName = "INDATA";
                INDATA.Columns.Add("SRCTYPE", typeof(string));
                INDATA.Columns.Add("LANGID", typeof(string));
                INDATA.Columns.Add("LOTID", typeof(string));
                INDATA.Columns.Add("END_PROCID", typeof(string));
                INDATA.Columns.Add("END_EQPTID", typeof(string));
                INDATA.Columns.Add("STRT_PROCID", typeof(string));
                INDATA.Columns.Add("STRT_EQPTID", typeof(string));
                INDATA.Columns.Add("USERID", typeof(string));
                INDATA.Columns.Add("WIPNOTE", typeof(string));
                INDATA.Columns.Add("RESNCODE", typeof(string));
                INDATA.Columns.Add("RESNNOTE", typeof(string));
                INDATA.Columns.Add("RESNDESC", typeof(string));
                //2019.02.22
                INDATA.Columns.Add("RESNDEFT", typeof(string));

                //�������� ã��
                string nextProcessId = string.Empty;
                if (statusvalue == "REWORK_WAIT") //REWORK_WAIT : ���۾����
                {
                    if (cboWork1.SelectedValue.ToString() == "REWORK") //REWORK : ���۾�
                    {
                        nextProcessId = Util.GetCondition(cboReworkReturnProcess1) == "" ? null : cboReworkReturnProcess1.SelectedValue.ToString();  //�������� ��
                    }
                    else
                    {
                        nextProcessId = "PS000"; //������
                    }
                }
                else
                {
                    if (cboWork1.SelectedValue.ToString() == "REWORK_WAIT") //REWORK_WAIT : �������
                    {
                        nextProcessId = "PR000"; //��������                       
                    }
                    else
                    {
                        nextProcessId = null; //������
                    }
                }
                DataTable dtResult1 = DataTableConverter.Convert(dgSearchResult1.ItemsSource);

                for (int i = 0; i < dgSearchResult1.GetRowCount(); i++)
                {
                    if (DataTableConverter.GetValue(dgSearchResult1.Rows[i].DataItem, "CHK").ToString() == "True")
                    {
                        //C1.WPF.DataGrid.DataGridRow dgr = dgSearchResult.Rows[i] as C1.WPF.DataGrid.DataGridRow;
                        //DataRowView drv = dgr.DataItem as DataRowView;
                        DataRow drv = dtResult1.Rows[i] as DataRow;

                        DataRow drINDATA = INDATA.NewRow();
                        drINDATA["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                        drINDATA["LANGID"] = LoginInfo.LANGID;
                        drINDATA["LOTID"] = drv["LOTID"].ToString();
                        drINDATA["END_PROCID"] = drv["PROCID"].ToString(); //���� ����
                        drINDATA["END_EQPTID"] = drv["EQPTID"].ToString();
                        drINDATA["STRT_PROCID"] = nextProcessId; //��������
                        drINDATA["STRT_EQPTID"] = null;
                        drINDATA["USERID"] = LoginInfo.USERID;
                        drINDATA["WIPNOTE"] = null;
                        drINDATA["RESNCODE"] = sResnCode; // drv["RESNCODE"]; //sResnCode;
                        drINDATA["RESNNOTE"] = Util.GetCondition(txtReason1); //Util.NVC(cboReason.DisplayMemberPath) == "" ? null : cboReason.DisplayMemberPath;
                        drINDATA["RESNDESC"] = Util.GetCondition(cboScrapIMPUTE_CODE1) == "" ? null : cboScrapIMPUTE_CODE1.SelectedValue;
                        //2019.02.22
                        drINDATA["RESNDEFT"] = Util.GetCondition(cboDefectChoice1) == "" ? null : cboDefectChoice1.SelectedValue;
                        INDATA.Rows.Add(drINDATA);
                    }
                }

                dsInput.Tables.Add(INDATA);

                DataTable IN_CLCTITEM = new DataTable();
                IN_CLCTITEM.TableName = "IN_CLCTITEM";
                IN_CLCTITEM.Columns.Add("CLCTITEM", typeof(string));
                IN_CLCTITEM.Columns.Add("CLCTVAL", typeof(string));
                IN_CLCTITEM.Columns.Add("PASSYN", typeof(string));
                dsInput.Tables.Add(IN_CLCTITEM);

                DataTable IN_CLCTDITEM = new DataTable();
                IN_CLCTDITEM.TableName = "IN_CLCTDITEM";
                IN_CLCTDITEM.Columns.Add("CLCTDITEM", typeof(string));
                IN_CLCTDITEM.Columns.Add("CLCTDVAL", typeof(string));
                dsInput.Tables.Add(IN_CLCTDITEM);

                //DataSet dsResult = new ClientProxy().ExecuteServiceSync_Multi("BR_PRD_REG_OUTPUTASSY_MULTILOT_INPUT", "INDATA,IN_CLCTITEM,IN_CLCTDITEM", "OUTDATA", dsInput, null);
                //DataSet dsResult = new ClientProxy().ExecuteServiceSync_Multi("BR_PRD_REG_OUTPUTASSY_MULTILOT", "INDATA,IN_CLCTITEM,IN_CLCTDITEM", "OUTDATA", dsInput, null);
                
                loadingIndicator.Visibility = Visibility.Visible;

                new ClientProxy().ExecuteService_Multi("BR_PRD_REG_OUTPUTASSY_MULTILOT_DEFECT", "INDATA,IN_CLCTITEM,IN_CLCTDITEM", "OUTDATA", (dsResult, dataException) =>
                {
                    try
                    {


                        if (dataException != null)
                        {
                            //Util.AlertByBiz("BR_PRD_REG_OUTPUTASSY_MULTILOT_DEFECT", dataException.Message, dataException.ToString());
                            Util.MessageException(dataException);
                            loadingIndicator.Visibility = Visibility.Collapsed;
                            return;
                        }
                        else
                        {
                            loadingIndicator.Visibility = Visibility.Collapsed;
                            if (dsResult.Tables["OUTDATA"] != null)
                            {
                                tbState.Text = "";
                                txtLotID.Text = "";
                                txtReason1.Text = "";
                                dgSearchResult1.ItemsSource = null;

                                lot_fullCheck = false;
                                btnAllSelect1.Content = ObjectDic.Instance.GetObjectName("��ü����");
                                btnAllSelect1.Foreground = Brushes.White;

                                ms.AlertInfo("SFU1275"); //����ó�� �Ǿ����ϴ�.
                            }
                        }

                        return;
                    }
                    catch (Exception ex)
                    {
                        loadingIndicator.Visibility = Visibility.Collapsed;
                        throw ex;
                    }

                }, dsInput);                
            }
            catch (Exception ex)
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

        void popup_Closed(object sender, EventArgs e)
        {

        }

        //2019-02-08
        private void getSearch()
        {
            try
            {
                string procType = string.Empty; //R,S
                string RS_proc = string.Empty;
                string actId = string.Empty;

                if(cboStatus.SelectedValue == null)
                {
                    return;
                }

                if (cboStatus.SelectedValue.ToString() == "SCRAP_WAIT") //�����
                {
                    procType = "S";
                    actId = "DEFECT_LOT";
                    RS_proc = getRSProcess(procType);
                }
                else // REPAIRE_JUDGE : �������
                {
                    procType = "R";
                    actId = "REPAIRE_LOT";
                    RS_proc = getRSProcess(procType);
                }

                DataTable inDataTable = new DataTable();

                inDataTable.Columns.Add("LANGID", typeof(string));
                inDataTable.Columns.Add("EQSGID", typeof(string));
                inDataTable.Columns.Add("MODLID", typeof(string));
                inDataTable.Columns.Add("PRODCLASS", typeof(string));
                inDataTable.Columns.Add("PRODID", typeof(string));
                inDataTable.Columns.Add("PROCID", typeof(string));
                inDataTable.Columns.Add("ACTID", typeof(string));
                inDataTable.Columns.Add("PROCTYPE", typeof(string));
                inDataTable.Columns.Add("PROCID_CAUSE", typeof(string));
                inDataTable.Columns.Add("FROMDATE", typeof(string));
                inDataTable.Columns.Add("TODATE", typeof(string));
                inDataTable.Columns.Add("SHIFT", typeof(string));
                inDataTable.Columns.Add("AREAID", typeof(string));
                inDataTable.Columns.Add("COUNT", typeof(Int64));

                DataRow searchCondition = inDataTable.NewRow();
                searchCondition["LANGID"] = LoginInfo.LANGID;
                searchCondition["EQSGID"] = Util.GetCondition(cboEquipmentSegment) == "" ? null : cboEquipmentSegment.SelectedValue;
                searchCondition["MODLID"] = Util.GetCondition(cboProductModel) == "" ? null : cboProductModel.SelectedValue;
                searchCondition["PRODCLASS"] = Util.GetCondition(cboPrdtClass) == "" ? null : cboPrdtClass.SelectedValue;
                searchCondition["PRODID"] = Util.GetCondition(cboProduct) == "" ? null : cboProduct.SelectedValue;
                searchCondition["PROCID"] = RS_proc;
                searchCondition["ACTID"] = actId;
                searchCondition["PROCTYPE"] = procType;
                searchCondition["PROCID_CAUSE"] = Util.GetCondition(cboProcessPack) == "" ? null : cboProcessPack.SelectedValue;
                searchCondition["FROMDATE"] = dtpDateFrom.SelectedDateTime.ToString("yyyyMMdd");
                searchCondition["TODATE"] = dtpDateTo.SelectedDateTime.ToString("yyyyMMdd");
                searchCondition["SHIFT"] = null; // �۾��� ���� ����
                searchCondition["AREAID"] = LoginInfo.CFG_AREA_ID; // �۾��� ���� ����
                searchCondition["COUNT"] = cboListCount.SelectedValue;

                inDataTable.Rows.Add(searchCondition);

                //dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_REPAIR_DEFECT_SEARCH", "INDATA", "OUTDATA", inDataTable);
                loadingIndicator.Visibility = Visibility.Visible;
                new ClientProxy().ExecuteService("DA_PRD_SEL_REPAIR_DEFECT_SEARCH_INFO", "INDATA", "OUTDATA", inDataTable, (dtSearchResult, ex) =>
                {
                    loadingIndicator.Visibility = Visibility.Collapsed;

                    if (ex != null)
                    {
                        //Util.AlertByBiz("DA_PRD_SEL_REPAIR_DEFECT_SEARCH_INFO", ex.Message, ex.ToString());
                        Util.MessageException(ex);
                        return;
                    }
                    dgSearchResult.ItemsSource = null;

                    if (dtSearchResult.Rows.Count != 0)
                    {
                        //dgSearchResult.ItemsSource = DataTableConverter.Convert(dtResult);
                        Util.GridSetData(dgSearchResult, dtSearchResult, FrameOperation);
                        dtResult = DataTableConverter.Convert(dgSearchResult.ItemsSource);
                        //���� ���� ����
                        string sProcID_Cause = Util.GetCondition(cboProcessPack);  //Util.NVC(dtResult.Rows[0]["PROCID"]);
                        string sProcID_Cause_Name = Util.NVC(cboProcessPack.Text); //Util.NVC(dtResult.Rows[0]["PROCNAME"]);
                        string sRoutID_Cause = Util.NVC(dtSearchResult.Rows[0]["ROUTID"]);
                        string sFlowID_Cause = Util.NVC(dtSearchResult.Rows[0]["FLOWID"]);

                        cboReworkReturnProcess.ItemsSource = null;
                        cboReworkReturnProcess.SelectedIndex = 0;


                        dtReturnProcess = getReturnProcess(sProcID_Cause, sRoutID_Cause, sFlowID_Cause);
                        setReturnProcess();
                    }

                    Util.SetTextBlockText_DataGridRowCount(tbSearch_cnt, Util.NVC(dtSearchResult.Rows.Count));
                });

                
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private string getRSProcess(string procType)
        {
            try
            {
                DataTable inDataTable = new DataTable();

                inDataTable.Columns.Add("LANGID", typeof(string));
                inDataTable.Columns.Add("SHOPID", typeof(string));
                inDataTable.Columns.Add("AREAID", typeof(string));
                inDataTable.Columns.Add("EQSGID", typeof(string));
                inDataTable.Columns.Add("PROCTYPE", typeof(string));

                DataRow searchCondition = inDataTable.NewRow();
                searchCondition["LANGID"] = LoginInfo.LANGID;
                searchCondition["SHOPID"] = LoginInfo.CFG_SHOP_ID;
                searchCondition["AREAID"] = LoginInfo.CFG_AREA_ID;
                searchCondition["EQSGID"] = Util.GetCondition(cboEquipmentSegment) == "" ? null : cboEquipmentSegment.SelectedValue;
                searchCondition["PROCTYPE"] = procType;

                inDataTable.Rows.Add(searchCondition);

                DataTable dtRsProc = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_REPAIR_DEFECT_PROCESS_INFO", "INDATA", "OUTDATA", inDataTable);

                string rsProc = string.Empty;
                if (dtRsProc.Rows.Count > 0)
                {
                    rsProc = dtRsProc.Rows[0]["PROCID"].ToString();
                }

                return rsProc;
            }
            catch (Exception ex)
            {
                throw ex;
            }

        }

        /// <summary>
        /// ������� ���Ե� ���� �� dataTable
        /// </summary>
        /// <param name="sProcess_cause"></param>
        private DataTable getReturnProcess(string sProcess_cause, string sRoutid, string sFlowid)
        {
            DataTable dtResult = null;
            try
            {
                //DA_PRD_SEL_PROC_ROUTE_PREVIOUS
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("SRCTYPE", typeof(string));
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("PROCID", typeof(string));
                RQSTDT.Columns.Add("ROUTID", typeof(string));
                RQSTDT.Columns.Add("FLOWID", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                dr["LANGID"] = LoginInfo.LANGID;
                dr["PROCID"] = sProcess_cause;
                dr["ROUTID"] = sRoutid;
                dr["FLOWID"] = sFlowid;
                RQSTDT.Rows.Add(dr);

                dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_PROC_ROUTE_PREVIOUS", "RQSTDT", "RSLTDT", RQSTDT);
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return dtResult;
        }

        private void setReturnProcess()
        {
            try
            {
                if (dtReturnProcess != null)
                {
                    DataTable dt = new DataTable();
                    dt.Columns.Add("CBO_NAME", typeof(string));
                    dt.Columns.Add("CBO_CODE", typeof(string));

                    foreach (DataRow drow in dtReturnProcess.Rows)
                    {
                        DataRow dr = dt.NewRow();
                        dr["CBO_NAME"] = drow["PROCID"].ToString() + " : " + drow["PROCNAME"].ToString();
                        dr["CBO_CODE"] = drow["PROCID"].ToString();

                        dt.Rows.Add(dr);
                    }

                    cboReworkReturnProcess.DisplayMemberPath = "CBO_NAME";
                    cboReworkReturnProcess.SelectedValuePath = "CBO_CODE";
                    cboReworkReturnProcess.ItemsSource = dt.Copy().AsDataView();
                    cboReworkReturnProcess.SelectedIndex = 0;
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public bool getLotSearch()
        {
            try
            {
                string procType = string.Empty; //R,S

                DataTable inDataTable = new DataTable();
                inDataTable.TableName = "INDATA";
                inDataTable.Columns.Add("LANGID", typeof(string));
                inDataTable.Columns.Add("LOTID", typeof(string));

                DataRow searchCondition = inDataTable.NewRow();
                searchCondition["LANGID"] = LoginInfo.LANGID;
                searchCondition["LOTID"] = txtLotID.Text;

                inDataTable.Rows.Add(searchCondition);

                //dtLotResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_REPAIR_DEFECT_SINGLE_LOT_SEARCH", "INDATA", "OUTDATA", inDataTable);
                dtLotResult = new ClientProxy().ExecuteServiceSync("BR_PRD_SEL_REPAIR_DEFECT_SINGLE_LOT_SEARCH", "INDATA", "OUTDATA", inDataTable);
                
                if (dtLotResult.Rows.Count > 0)
                {
                    if (dgSearchResult1.GetRowCount() == 0)
                    {
                        pre_procid_cause = Util.NVC(dtLotResult.Rows[0]["PROCID_CAUSE"]);
                        pre_proctype = Util.NVC(dtLotResult.Rows[0]["PROCTYPE"]);
                        pre_procid = Util.NVC(dtLotResult.Rows[0]["PROCID"]);
                        pre_eqsgid = Util.NVC(dtLotResult.Rows[0]["EQSGID"]);
                        string sRoutID_Cause = Util.NVC(dtLotResult.Rows[0]["ROUTID"]);
                        string sFlowID_Cause = Util.NVC(dtLotResult.Rows[0]["FLOWID"]);

                        string PROCNAME_CAUSE = Util.NVC(dtLotResult.Rows[0]["PROCNAME_CAUSE"]);
                        string EQSGNAME = Util.NVC(dtLotResult.Rows[0]["EQSGNAME"]);
                        string PROCNAME = Util.NVC(dtLotResult.Rows[0]["PROCNAME"]);

                        string status_text = string.Empty;
                        string statusvalue_kr = string.Empty;

                        dtReturnProcess1 = getReturnProcess(pre_procid_cause, sRoutID_Cause, sFlowID_Cause);
                        setReturnProcess1();

                        status_text = "==> " + ObjectDic.Instance.GetObjectName("����") + " : " + EQSGNAME + " / ";

                        if (pre_proctype == "R")
                        {
                            status_text += ObjectDic.Instance.GetObjectName("�������") + " : " + ObjectDic.Instance.GetObjectName("����") + ObjectDic.Instance.GetObjectName("����") + " / ";
                            statusvalue = "REWORK_WAIT";
                            statusvalue_kr = ObjectDic.Instance.GetObjectName("���۾����");
                        }
                        else
                        {
                            status_text += ObjectDic.Instance.GetObjectName("�������") + " : " + ObjectDic.Instance.GetObjectName("���") + ObjectDic.Instance.GetObjectName("����") + " / ";
                            statusvalue = "SCRAP_WAIT";
                            statusvalue_kr = ObjectDic.Instance.GetObjectName("�����");
                        }

                        status_text += ObjectDic.Instance.GetObjectName("����") + ObjectDic.Instance.GetObjectName("����") + " : " + PROCNAME_CAUSE + " / ";
                        status_text += ObjectDic.Instance.GetObjectName("����") + " : " + statusvalue + "(" + statusvalue_kr + ") <==";

                        tbState.Text = ObjectDic.Instance.GetObjectName(status_text);

                        //cboWork1.SelectedItemChanged -= cboWork1_SelectedValueChanged;

                        //�ϰ��۾� cbo ����
                        String[] cboStatusValue = { statusvalue };
                        _combo.SetCombo(cboWork1, CommonCombo.ComboStatus.NONE, sFilter: cboStatusValue, sCase: "WORK");

                        //cboWork1_SelectedValueChanged(null, null);

                        //cboWork1.SelectedItemChanged += cboWork1_SelectedValueChanged;
                    }

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

        private void lotGridAdd(int TotalRow)
        {
            try
            {
                if (TotalRow == 0)
                {
                    dgSearchResult1.ItemsSource = DataTableConverter.Convert(dtLotResult);
                    return;
                }
                //cell list�� �߰�// 
                DataGridRowAdd(dgSearchResult1);

                DataRow dr = dtLotResult.Rows[0];

                DataTableConverter.SetValue(dgSearchResult1.Rows[TotalRow].DataItem, "CHK", "false");
                DataTableConverter.SetValue(dgSearchResult1.Rows[TotalRow].DataItem, "LOTID", dr["LOTID"]);
                //DataTableConverter.SetValue(dgSearchResult1.Rows[TotalRow].DataItem, "PRODNAME", dr["PRODNAME"]);
                DataTableConverter.SetValue(dgSearchResult1.Rows[TotalRow].DataItem, "PRODDESC", dr["PRODDESC"]);
                DataTableConverter.SetValue(dgSearchResult1.Rows[TotalRow].DataItem, "PROCNAME_CAUSE", dr["PROCNAME_CAUSE"]);
                DataTableConverter.SetValue(dgSearchResult1.Rows[TotalRow].DataItem, "ACTDTTM", dr["ACTDTTM"]);
                DataTableConverter.SetValue(dgSearchResult1.Rows[TotalRow].DataItem, "FAILNAME", dr["FAILNAME"]);
                DataTableConverter.SetValue(dgSearchResult1.Rows[TotalRow].DataItem, "MODLNAME", dr["MODLNAME"]);
                DataTableConverter.SetValue(dgSearchResult1.Rows[TotalRow].DataItem, "ROUTNAME", dr["ROUTNAME"]);
                DataTableConverter.SetValue(dgSearchResult1.Rows[TotalRow].DataItem, "EQSGNAME", dr["EQSGNAME"]);
                DataTableConverter.SetValue(dgSearchResult1.Rows[TotalRow].DataItem, "PRODCLASS", dr["PRODCLASS"]);
                DataTableConverter.SetValue(dgSearchResult1.Rows[TotalRow].DataItem, "PCTYNAME", dr["PCTYNAME"]);

                DataTableConverter.SetValue(dgSearchResult1.Rows[TotalRow].DataItem, "PRODID", dr["PRODID"]);
                DataTableConverter.SetValue(dgSearchResult1.Rows[TotalRow].DataItem, "PROCID", dr["PROCID"]);
                DataTableConverter.SetValue(dgSearchResult1.Rows[TotalRow].DataItem, "EQPTID", dr["EQPTID"]);
                DataTableConverter.SetValue(dgSearchResult1.Rows[TotalRow].DataItem, "WIPSTAT", dr["WIPSTAT"]);
                DataTableConverter.SetValue(dgSearchResult1.Rows[TotalRow].DataItem, "PROCID_CAUSE", dr["PROCID_CAUSE"]);
                DataTableConverter.SetValue(dgSearchResult1.Rows[TotalRow].DataItem, "EQSGID", dr["EQSGID"]);
                DataTableConverter.SetValue(dgSearchResult1.Rows[TotalRow].DataItem, "MODLID", dr["MODLID"]);

                DataTableConverter.SetValue(dgSearchResult1.Rows[TotalRow].DataItem, "ROUTID", dr["ROUTID"]);
                DataTableConverter.SetValue(dgSearchResult1.Rows[TotalRow].DataItem, "FLOWID", dr["FLOWID"]);
                DataTableConverter.SetValue(dgSearchResult1.Rows[TotalRow].DataItem, "RESNNOTE", dr["RESNNOTE"]);
                DataTableConverter.SetValue(dgSearchResult1.Rows[TotalRow].DataItem, "RESNCODE", dr["RESNCODE"]);
                DataTableConverter.SetValue(dgSearchResult1.Rows[TotalRow].DataItem, "ACTID", dr["ACTID"]);
                DataTableConverter.SetValue(dgSearchResult1.Rows[TotalRow].DataItem, "PROCTYPE", dr["PROCTYPE"]);

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

        private DataTable getReturnProcess1(string sProcess_cause, string sRoutid, string sFlowid)
        {
            DataTable dtResult = null;
            try
            {
                //DA_PRD_SEL_PROC_ROUTE_PREVIOUS
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("SRCTYPE", typeof(string));
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("PROCID", typeof(string));
                RQSTDT.Columns.Add("ROUTID", typeof(string));
                RQSTDT.Columns.Add("FLOWID", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                dr["LANGID"] = LoginInfo.LANGID;
                dr["PROCID"] = sProcess_cause;
                dr["ROUTID"] = sRoutid;
                dr["FLOWID"] = sFlowid;
                RQSTDT.Rows.Add(dr);

                dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_PROC_ROUTE_PREVIOUS", "RQSTDT", "RSLTDT", RQSTDT);

                return dtResult;
            }
            catch (Exception ex)
            {
                throw ex;
            }
            
        }

        private void setReturnProcess1()
        {
            try
            {
                if (dtReturnProcess1 != null)
                {

                    DataTable dt = new DataTable();
                    dt.Columns.Add("CBO_NAME", typeof(string));
                    dt.Columns.Add("CBO_CODE", typeof(string));

                    foreach (DataRow drow in dtReturnProcess1.Rows)
                    {
                        DataRow dr = dt.NewRow();
                        dr["CBO_NAME"] = drow["PROCID"].ToString() + " : " + drow["PROCNAME"].ToString();
                        dr["CBO_CODE"] = drow["PROCID"].ToString();

                        dt.Rows.Add(dr);
                    }

                    cboReworkReturnProcess1.DisplayMemberPath = "CBO_NAME";
                    cboReworkReturnProcess1.SelectedValuePath = "CBO_CODE";
                    cboReworkReturnProcess1.ItemsSource = dt.Copy().AsDataView();
                    cboReworkReturnProcess1.SelectedIndex = 0;
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private void setComboBox_DefectChoice(C1ComboBox cbo, CommonCombo.ComboStatus cs)
        {
            try
            {
                DataTable dt = new DataTable();
                dt.Columns.Add("LANGID", typeof(string));
                dt.Columns.Add("ACTID", typeof(string));
                dt.Columns.Add("PROCID", typeof(string));
                dt.Columns.Add("AREAID", typeof(string));

                DataRow dr = dt.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["ACTID"] = cboCostType.SelectedValue.ToString();
                dr["PROCID"] = "PS000";
                dr["AREAID"] = LoginInfo.CFG_AREA_ID;
                dt.Rows.Add(dr);

                DataTable result = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_ACTIVITY_REASON_SCRAP_PACK_CBO", "RQUST", "RSLT", dt);

                cbo.DisplayMemberPath = "CBO_NAME";
                cbo.SelectedValuePath = "CBO_CODE";

                cboDefectChoice.ItemsSource = DataTableConverter.Convert(result);

                //cbo.SelectedIndex = 0;

            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex.Message), null, "Error", MessageBoxButton.OK, MessageBoxIcon.None);
                return;

            }
        }

        private void setComboBox_DefectChoice1(C1ComboBox cbo, CommonCombo.ComboStatus cs)
        {

            try
            {
                DataTable dt = new DataTable();
                dt.Columns.Add("LANGID", typeof(string));
                dt.Columns.Add("ACTID", typeof(string));
                dt.Columns.Add("PROCID", typeof(string));
                dt.Columns.Add("AREAID", typeof(string));

                DataRow dr = dt.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["ACTID"] = cboCostType1.SelectedValue.ToString();
                dr["PROCID"] = "PS000";
                dr["AREAID"] = LoginInfo.CFG_AREA_ID;
                dt.Rows.Add(dr);

                DataTable result = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_ACTIVITY_REASON_SCRAP_PACK_CBO", "RQUST", "RSLT", dt);

                cbo.DisplayMemberPath = "CBO_NAME";
                cbo.SelectedValuePath = "CBO_CODE";

                cboDefectChoice1.ItemsSource = DataTableConverter.Convert(result);

                //cbo.SelectedIndex = 0;

            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex.Message), null, "Error", MessageBoxButton.OK, MessageBoxIcon.None);
                return;

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

                    txtLotID.Visibility = Visibility.Collapsed;

                    for (int i = intFirstRow; i < dt.Rows.Count; i++)
                    {
                        addYn = true;

                        int TotalRow = dgSearchResult1.GetRowCount();

                        //�̰� LOTID ������ ���� �����ΰ���?????
                        temp_cellId = dt.Rows[i][0].ToString();

                        //getCell_check(temp_cellId);

                        //������ ���� CELL ID��
                        if (!getCell_check(temp_cellId))
                        {
                            if (vali_fail_list == "")
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
                            string grid_id = DataTableConverter.GetValue(dgSearchResult1.Rows[j].DataItem, "LOTID").ToString();

                            if (temp_cellId == grid_id)
                            {
                                if (distinct == "")
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

                        if (addYn) cellGridAdd(TotalRow); //�׸��忡 �߰�
                    }

                    if (cell_info.Length > 0)
                    {
                        ms.AlertInfo(cell_info);

                        cell_info = "";
                    }

                    if (vali_fail_list != "")
                    {
                        //Util.AlertInfo("���� UPLOAD ��� LOT ID : " + vali_fail_list + " �� HOLD �� �� ���� ������ LOT�Դϴ�.\n(���������ʰų� ���, �̹� HOLD�� LOT��)");
                        ms.AlertWarning("SFU3397", vali_fail_list); //���� UPLOAD ��� LOT ID : {0} �� HOLD �� �� ���� ������ LOT�Դϴ�.\n(���������ʰų� ���, �̹� HOLD�� LOT��)
                    }

                    if (distinct != "")
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

                txtLotID.Text = "";
                string info_step = string.Empty;

                if (dtFindResult.Rows.Count > 0)
                {

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

        private void cellGridAdd(int TotalRow)
        {
            try
            {
                if (TotalRow == 0)
                {
                    dgSearchResult1.ItemsSource = DataTableConverter.Convert(dtFindResult);
                    return;
                }
                //cell list�� �߰�// 
                DataGridRowAdd(dgSearchResult1);

                DataRow dr = dtFindResult.Rows[0];


                // �Ƹ��� ���� �κ� ���� (���;� �ϴ� �÷����� ��? LOTID�� ���;� �� ����)
                DataTableConverter.SetValue(dgSearchResult1.Rows[TotalRow].DataItem, "CHK", "false");
                DataTableConverter.SetValue(dgSearchResult1.Rows[TotalRow].DataItem, "LOTID", dr["LOTID"]);
                //DataTableConverter.SetValue(dgSearchResult1.Rows[TotalRow].DataItem, "PROCNAME", dr["PROCNAME"]);
                //DataTableConverter.SetValue(dgSearchResult1.Rows[TotalRow].DataItem, "WIPSNAME", dr["WIPSNAME"]);
                //DataTableConverter.SetValue(dgSearchResult1.Rows[TotalRow].DataItem, "BOXID", dr["BOXID"]);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        #endregion Method




    }
}
