/*************************************************************************************
 Created Date : 2016.06.16
      Creator : 
   Decription : 
--------------------------------------------------------------------------------------
 [Change History]
  2016.06.16  DEVELOPER : Initial Created.
  2017.03.08  ���Ͽ�    
  2017.03.14  ������      ������� ��ȸ ȭ�� ���Ͱ������� Slurry �� ���� �ٶ��ϴ�
  2017.04.27  ������      ���, ���� ������ ����� DISPATCH_YN �� N�� ��� �ڵ� DISPATCH �߰�
  2017.11.21  INS���Թ�   GEMS FOLDING ���� ���� �ܾ� ���� ��û CSR20171011_02178  - ���� _StackingYN �ǰ�� ���� ���ÿɼǿ��� �����Ǳ� ������, ALL �ϰ�� ��� �Ұ��� 
  2018.10.01  �̴��      [CSR ID:3805352] ������� ��ȸ(����Lot ����) 
  2019.05.17  �̻���      [C20190415_74474] x-ray pallet �� �� ���� ��� �߰�
  2019.07.16  �赿��      ������ 1,2 �� ��� ������ô 3LOSS ���뿡 ���� ����
  2020.01.06  ������      ǰ������, ��������, �ҷ��±� ��ȸ�� ������ ������ LoginInfo.CFG_AREA_ID -> LOT�� �� ������ ����
  2020.03.11  �赿��      ����̷� ���� ��ȸ ��� �߰�.
  2020.05.08  ���±�      ROLL PRESS ���������� TAG ���� ������� �ϴ� AREA üũ �߰�. CSRC20200508-000359
  2020.05.21  ���±�      COATING �������� HALF SIDE(�ű�)�� ���� ��� �ϴ� AREA �߰� C20200519-000286
  2020.06.18  ������      C20200610-000491 - ��� ���� Į�� �߰� 
  2020.07.01  ������      C20200630-000328 - ���� ���� TAG �� Į�� �߰� 
  2021.08.18  ������      ROLLMAP ��ư Ȱ��ȭ ���� ���� (���� -> LOT ����)
  2021.08.20  ����ȫ      C20210720-000108 - INPUT_SECTION_ROLL_DIRCT, EM_SECTION_ROLL_DIRCTN Į�� �߰�
  2021.08.23  ������      [GM JV Proj.]��ȸ����-���걸�� �߰�
  2021.09.06  ��ȭ��      FastTrack �÷� �߰�
  2021.09.09  �ű���      �Ѹ� �˾� ��Ʈ ���� �Ķ���� �߰�
  2021.11.02  �ű���      �Ѹ� �˾� ȣ�� �� EquipmentName -> EquipmentName + [EquipmentId] �� ǥ��ǵ��� ����
  2021.12.01  ����ȫ      C20211114-000011 Rewinding Process ���ػ���Site Column �߰�
  2022.03.24  ����ȣ      C20220110-000251 �����̵� ������, �ѹ��� �÷� �߰�
  2022.04.28  ����ȫ      C20220414-000238 - ���� ���ְ˻� ���� �÷� �߰� (ǰ������)
  2022.05.11  ����ȫ      C20220406-000241 - ����ҷ����� TAB �ҷ�������� �÷� �߰�
  2022.06.13  ������      COATING, ROLL PRESS�������� CSTID �����ֵ��� �߰�
  2022.06.23  ����ȫ      C20220117-000174 - �������� ���� �Ĵ�Ƚ�� �÷� �߰� / ����ε� ������ ������ Tag �÷� ǥ��
  2022.06.24  ��ȣ��      C20220622-000541 - 2�� ���� ���� ȭ�� ���ܼ� ���� ��/�� ��ȭ�� �߰� ��û��(�������� ���� �߰�)
  2022.11.04  ��ȣ��      C20221107-000542 - LASER_ABLATION �����߰�
  2022.11.25  �����      C20221006-000307 - ���� CT�˻� CHECK �׸� ����
  2023.02.21  �����      �Ѹ� ���� ��ȸ ��ư �߰�
  2023.03.24  ��ȫ��      ����Ȱ��ȭMES ����
  2023.12.08  ������      E20231211-000182 ��ȸ�� ǥ�� ���� ID �߰�
**************************************************************************************/

using C1.WPF;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.CMM001.Popup;
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
using LGC.GMES.MES.CMM001;
using C1.WPF.DataGrid;
using LGC.GMES.MES.CMM001.Extensions;

namespace LGC.GMES.MES.FCS002
{
    public partial class FCS002_318 : UserControl, IWorkArea
    {
        #region Declaration & Constructor 

        private string _StackingYN = string.Empty;
        string _AREATYPE = "";
        string _AREAID = "";
        string _PROCID = "";
        string _EQSGID = "";
        string _EQPTID = "";
        string _LOTID = "";
        string _WIPSEQ = "";
        string _LANEPTNQTY = "";
        string _ORGQTY = "";
        string _UNITCODE = "";
        string _WIP_NOTE = "";
        string _LANE = "";
        string _EQPTNAME = "";

        //���� �𸶿�Ʈ Ÿ�� �ڵ�.
        string _INPUT_LOT_UNMOUNT_TYPE_CODE = "";

        //FOLDING & STACKING ������
        string _EQGRID = string.Empty;

        bool _bLoad = true;

        //���� CT�˻� CHECK �׸� ����
        private string _workCalbuttonAuthYN = string.Empty; //���°���/��ư���� ��뿩��
        private string _unidentifiedLossYN = string.Empty;  //��Ȯ��LOSS ��뿩��
        private string _reworkBoxTypeYN = string.Empty;     //���۾� BOX ������ �۾����� REWORK Ȯ��
        private string _pkgInputCheckYN = string.Empty;     //PKG ���Խ� ���۾� �ϼ� LOT CHECK ��뿩��
        private string _qmsDefectCodeYN = string.Empty;     //QMS �ҷ��ڵ� ��뿩��

        List<string> _MColumns1;
        List<string> _MColumns2;

        private BizDataSet _Biz = new BizDataSet();
        public FCS002_318()
        {
            InitializeComponent();

            InitCombo();
            InitColumnsList();          // ���� ȯ�� üũ�� Į�� Visible 

            GetAreaType(cboProcess.SelectedValue.ToString());
            AreaCheck(cboProcess.SelectedValue.ToString());
            SetProcessNumFormat(cboProcess.SelectedValue.ToString());

            //20210906 ��ȭ�� : FastTrack ���뿩�� üũ
            if (ChkFastTrackOWNER())
            {
                dgLotList.Columns["FAST_TRACK_FLAG"].Visibility = Visibility.Visible;

            }

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
            C1ComboBox[] cboAreaChild = { cboEquipmentSegment };
            _combo.SetCombo(cboArea, CommonCombo.ComboStatus.NONE, cbChild: cboAreaChild);

            //����
            C1ComboBox[] cboEquipmentSegmentParent = { cboArea };
            //C1ComboBox[] cboEquipmentSegmentChild = { cboProcess, cboEquipment };
            //_combo.SetCombo(cboEquipmentSegment, CommonCombo.ComboStatus.SELECT, cbChild: cboEquipmentSegmentChild, cbParent: cboEquipmentSegmentParent);
            _combo.SetCombo(cboEquipmentSegment, CommonCombo.ComboStatus.ALL, null, cbParent: cboEquipmentSegmentParent);

            //����
            C1ComboBox[] cboProcessParent = { cboEquipmentSegment };
            //C1ComboBox[] cboProcessChild = { cboEquipment };
            //_combo.SetCombo(cboProcess, CommonCombo.ComboStatus.SELECT, cbChild: cboProcessChild, cbParent: cboProcessParent);
            _combo.SetCombo(cboProcess, CommonCombo.ComboStatus.SELECT, null, cboProcessParent);

            if (cboProcess.Items.Count < 1)
                SetProcess();

            //����
            C1ComboBox[] cboEquipmentParent = { cboEquipmentSegment, cboProcess };
            _combo.SetCombo(cboEquipment, CommonCombo.ComboStatus.ALL, cbParent: cboEquipmentParent);

            //����帧
            string[] sFilter = { "FLOWTYPE" };
            _combo.SetCombo(cboFlowType, CommonCombo.ComboStatus.ALL, sCase: "COMMCODE", sFilter: sFilter);

            // Top/Back
            String[] sFilter3 = { "COAT_SIDE_TYPE" };
            _combo.SetCombo(cboTopBack, CommonCombo.ComboStatus.ALL, sFilter: sFilter3, sCase: "COMMCODE");

            // �ؼ�
            String[] sFilter1 = { "ELEC_TYPE" };
            _combo.SetCombo(cboElecType, CommonCombo.ComboStatus.ALL, sFilter: sFilter1, sCase: "COMMCODE");

            // ��������
            string[] sFilterMKType = { "MKT_TYPE_CODE" };
            _combo.SetCombo(cboMKTtype, CommonCombo.ComboStatus.ALL, sCase: "COMMCODE", sFilter: sFilterMKType);

            // 2021.08.23 : ���걸�� �߰�
            // ���걸��
            string[] sFilterProdDiv = { "PRODUCT_DIVISION" };
            _combo.SetCombo(cboProductDiv, CommonCombo.ComboStatus.ALL, sCase: "COMMCODE", sFilter: sFilterProdDiv);

            // ���걸�� Default �������
            if (cboProductDiv.Items.Count > 1)
                cboProductDiv.SelectedIndex = 1;

            cboEquipmentSegment.SelectedItemChanged += cboEquipmentSegment_SelectedItemChanged;
            cboProcess.SelectedItemChanged += CboProcess_SelectedItemChanged;
            cboArea.SelectedItemChanged += cboArea_SelectedItemChanged;



            //// �� AutoComplete
            //GetModel();

        }

        #endregion

        private void InitColumnsList()
        {
            _MColumns1 = new List<string>();
            _MColumns1.Add("EQPT_END_QTY");
            _MColumns1.Add("INPUT_QTY");
            _MColumns1.Add("WIPQTY_ED");
            _MColumns1.Add("CNFM_DFCT_QTY");
            _MColumns1.Add("CNFM_LOSS_QTY");
            _MColumns1.Add("CNFM_PRDT_REQ_QTY");
            _MColumns1.Add("LENGTH_EXCEED");
            _MColumns1.Add("WIPQTY2_ED");
            _MColumns1.Add("CNFM_DFCT_QTY2");
            _MColumns1.Add("CNFM_LOSS_QTY2");
            _MColumns1.Add("CNFM_PRDT_REQ_QTY2");
            _MColumns1.Add("LENGTH_EXCEED2");

            _MColumns2 = new List<string>();
            _MColumns2.Add("EQPT_END_QTY_EA");
            _MColumns2.Add("INPUT_QTY_EA");
            _MColumns2.Add("WIPQTY_ED_EA");
            _MColumns2.Add("CNFM_DFCT_QTY_EA");
            _MColumns2.Add("CNFM_LOSS_QTY_EA");
            _MColumns2.Add("CNFM_PRDT_REQ_QTY_EA");
            _MColumns2.Add("LENGTH_EXCEED_EA");
            _MColumns2.Add("WIPQTY2_ED_EA");
            _MColumns2.Add("CNFM_DFCT_QTY2_EA");
            _MColumns2.Add("CNFM_LOSS_QTY2_EA");
            _MColumns2.Add("CNFM_PRDT_REQ_QTY2_EA");
            _MColumns2.Add("LENGTH_EXCEED2_EA");
        }

        /// <summary>
        /// 20210906 ��ȭ�� FastTrack ���� ���� üũ
        /// </summary>
        private bool ChkFastTrackOWNER()
        {

            bool bRet = false;
            DataTable dt = new DataTable();
            dt.Columns.Add("CMCDTYPE", typeof(string));
            dt.Columns.Add("CBO_CODE", typeof(string));

            DataRow dr = dt.NewRow();
            dr["CMCDTYPE"] = "FAST_TRACK_OWNER";
            dr["CBO_CODE"] = LoginInfo.CFG_AREA_ID;
            dt.Rows.Add(dr);

            DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_COMMONCODE_ATTRIBUTE", "RQSTDT", "RSLTDT", dt);

            if (dtResult.Rows.Count != 0 && dtResult.Rows[0]["ATTRIBUTE1"].ToString() == "Y")
            {
                bRet = true;
            }
            else
            {
                bRet = false;
            }
            return bRet;
        }

        #region Event

        #region [Form Load]
        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            //����� ���Ѻ��� ��ư �����
            List<Button> listAuth = new List<Button>();

            Util.pageAuth(listAuth, FrameOperation.AUTHORITY);
            //������� ����� ���Ѻ��� ��ư �����

            GetCaldate();

            dtpDateFrom.SelectedDataTimeChanged += dtpDate_SelectedDataTimeChanged;
            dtpDateTo.SelectedDataTimeChanged += dtpDate_SelectedDataTimeChanged;

            //2019.09.25 ���� : CNB�����̰� CT/RP/ST������ CarrierID�� �����ֵ��� ����
            //2022.06.13 ������ : �������� ���� �� �׷� Code �� ����� AREA�� CARRIER ID �����ֵ��� ������
            SetCarrierVisible();

            // ���� ��� ǥ�ÿ���
            EltrGrdCodeColumnVisible();

            // ���� CT�˻� CHECK �׸� ����
            GetAreaByCheckData();

            this.Loaded -= UserControl_Loaded;
        }
        #endregion

        #region [��ȸ]
        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            ClearValue();
            GetLotList();
        }
        #endregion

        #region [����] - ��ȸ ����
        private void cboEquipmentSegment_SelectedItemChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            if (cboEquipmentSegment.Items.Count > 0 && cboEquipmentSegment.SelectedValue != null && !cboEquipmentSegment.SelectedValue.Equals("SELECT"))
            {
                //cboProcess.SelectedItemChanged -= CboProcess_SelectedItemChanged;
                SetProcess();
                //cboProcess.SelectedItemChanged += CboProcess_SelectedItemChanged;
                //SetEquipment();
            }
        }
        #endregion

        #region [����] - ��ȸ ����
        private void CboProcess_SelectedItemChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            if (cboProcess.Items.Count > 0 && cboProcess.SelectedValue != null && !cboProcess.SelectedValue.Equals("SELECT"))
            {
                SetEquipment();

                Util.gridClear(dgLotList);
                Util.gridClear(dgModelList);
                ClearValue();

                GetAreaType(cboProcess.SelectedValue.ToString());
                AreaCheck(cboProcess.SelectedValue.ToString());
                SetProcessNumFormat(cboProcess.SelectedValue.ToString());

                //2019.09.25 ���� : CNB�����̰� CT/RP/ST������ CarrierID�� �����ֵ��� ����
                //2022.06.13 ������ : �������� ���� �� �׷� Code �� ����� AREA�� CARRIER ID �����ֵ��� ������
                SetCarrierVisible();
            }
        }
        #endregion

        #region [��] - ��ȸ ����
        private void cboArea_SelectedItemChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            // ���� ��� ǥ�ÿ���
            EltrGrdCodeColumnVisible();
        }
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
        private void txtLotId_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                GetLotList();
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

        #region [������Ʈ] - ��ȸ ����
        private void txtPrjtName_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                GetLotList();
            }
        }
        #endregion

        #region [M����ȯ��] - ��ȸ ����
        private void chkPtnLen_Checked(object sender, RoutedEventArgs e)
        {
            // Visibility.Collapsed
            foreach (string str in _MColumns1)
            {
                if (dgLotList.Columns.Contains(str))
                    dgLotList.Columns[str].Visibility = Visibility.Collapsed;

                if (dgModelList.Columns.Contains(str))
                    dgModelList.Columns[str].Visibility = Visibility.Collapsed;
            }

            // Visibility.Visible
            foreach (string str in _MColumns2)
            {
                if (dgLotList.Columns.Contains(str))
                    dgLotList.Columns[str].Visibility = Visibility.Visible;

                if (dgModelList.Columns.Contains(str))
                    dgModelList.Columns[str].Visibility = Visibility.Visible;
            }

        }

        private void chkPtnLen_Unchecked(object sender, RoutedEventArgs e)
        {
            // Visibility.Visible
            foreach (string str in _MColumns1)
            {
                if (dgLotList.Columns.Contains(str))
                    dgLotList.Columns[str].Visibility = Visibility.Visible;

                if (dgModelList.Columns.Contains(str))
                    dgModelList.Columns[str].Visibility = Visibility.Visible;
            }

            // Visibility.Collapsed
            foreach (string str in _MColumns2)
            {
                if (dgLotList.Columns.Contains(str))
                    dgLotList.Columns[str].Visibility = Visibility.Collapsed;

                if (dgModelList.Columns.Contains(str))
                    dgModelList.Columns[str].Visibility = Visibility.Collapsed;
            }

        }
        #endregion

        #region [���ջ�] - ��ȸ ����
        private void chkModel_Checked(object sender, RoutedEventArgs e)
        {
            tbcList.SelectedIndex = 1;
        }

        private void chkModel_Unchecked(object sender, RoutedEventArgs e)
        {
            tbcList.SelectedIndex = 0;
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
                string sLotId = DataTableConverter.GetValue(rb.DataContext, "LOTID").ToString();
                string sDate = DataTableConverter.GetValue(rb.DataContext, "STARTDTTM").ToString();
                int iWipSeq = Convert.ToInt16(DataTableConverter.GetValue(rb.DataContext, "WIPSEQ"));
                string sEqptID = DataTableConverter.GetValue(rb.DataContext, "EQPTID").ToString();

                foreach (C1.WPF.DataGrid.DataGridRow row in ((C1.WPF.DataGrid.DataGridCellPresenter)rb.Parent).DataGrid.Rows)
                {
                    DataTableConverter.SetValue(row.DataItem, "CHK", 0);
                }

                DataTableConverter.SetValue(rb.DataContext, "CHK", 1);
                //row �� �ٲٱ�
                ((C1.WPF.DataGrid.DataGridCellPresenter)rb.Parent).DataGrid.SelectedIndex = ((C1.WPF.DataGrid.DataGridCellPresenter)rb.Parent).Cell.Row.Index;

                ClearValue();
                SetValue(rb.DataContext);
                SetProcessNumFormat(_PROCID);
                GetDefectInfo();

                if (_AREATYPE.Equals("A") && !_PROCID.Equals(Process.NOTCHING))
                {
                    GetSubLot();
                }


                GetInputHistory();
                GetQuality();
                GetColor();
                GetInputMaterial();
                GetEqpFaultyData();
                GetSlurry();
                ////GetRemark();

                if (cboProcess.SelectedValue.Equals(Process.REWINDER) || cboProcess.SelectedValue.Equals(Process.LASER_ABLATION) || cboProcess.SelectedValue.Equals(Process.BACK_WINDER))
                    GetDefectTagList(sLotId, iWipSeq);

                // Slitter Vision Image �߰�( 2017-01-05 )
                //if (cboProcess.SelectedValue.Equals(Process.SLITTING))
                //    GetVisionImage(sLotId, sDate);

                if (_PROCID.Equals(Process.ASSEMBLY) || _PROCID.Equals(Process.WASHING))
                {
                    GetHalfProductList();

                    if (TabReInput.Visibility == Visibility.Visible)
                    {
                        GetDefectReInputList();
                    }
                }

                if (_PROCID.Equals(Process.PACKAGING))
                {
                    GetTrayLotByTime();
                }


                ProcCheck(_PROCID, sEqptID);

                #region # RollMap ���� 
                /* if (IsEquipmentAttr(sEqptID))
                    btnRollMap.Visibility = Visibility.Visible;
                else
                    btnRollMap.Visibility = Visibility.Collapsed; */

                if (IsRollMapLotAttribute(sLotId))
                    btnRollMap.Visibility = Visibility.Visible;
                else
                    btnRollMap.Visibility = Visibility.Collapsed;

                if(btnRollMap.Visibility == Visibility.Visible && IsRollMapResultApply())
                    btnRollMapUpdate.Visibility = Visibility.Visible;
                else
                    btnRollMapUpdate.Visibility = Visibility.Collapsed;
                #endregion
            }
        }
        #endregion

        #region [ǰ��������ȸ]
        private void btnQualityInfo_Click(object sender, RoutedEventArgs e)
        {
            if (cboEquipmentSegment.SelectedIndex < 0 || cboEquipmentSegment.SelectedValue.ToString().Trim().Equals("SELECT"))
            {
                //Util.Alert("������ ���� �ϼ���.");
                Util.MessageValidation("SFU1223");
                return;
            }

            if (cboEquipment.SelectedIndex < 0 || cboEquipment.SelectedValue.ToString().Trim().Equals("")) //SELECT
            {
                //Util.Alert("���� ���� �ϼ���.");
                Util.MessageValidation("SFU1673");
                return;
            }

            CMM_ASSY_QUALITY_PKG wndPopup = new CMM_ASSY_QUALITY_PKG();
            wndPopup.FrameOperation = FrameOperation;

            if (wndPopup != null)
            {
                object[] Parameters = new object[5];
                Parameters[0] = cboEquipmentSegment.SelectedValue;
                Parameters[1] = Process.PACKAGING;
                Parameters[2] = cboEquipment.SelectedValue;
                Parameters[3] = cboEquipmentSegment.Text.ToString();
                Parameters[4] = cboEquipment.Text.ToString();

                C1WindowExtension.SetParameters(wndPopup, Parameters);

                wndPopup.Closed += new EventHandler(wndQualityRslt_Closed);

                // �˾� ȭ�� �������� ���� ����.
                this.Dispatcher.BeginInvoke(new Action(() => wndPopup.ShowModal()));
                //foreach (System.Windows.UIElement child in grdMain.Children)
                //{
                //    if (child.GetType() == typeof(CMM_ASSY_QUALITY_PKG))
                //    {
                //        grdMain.Children.Remove(child);
                //        break;
                //    }
                //}

                //grdMain.Children.Add(wndPopup);
                //wndPopup.BringToFront();
            }
        }
        private void wndQualityRslt_Closed(object sender, EventArgs e)
        {
            CMM_ASSY_QUALITY_PKG window = sender as CMM_ASSY_QUALITY_PKG;
            if (window.DialogResult == MessageBoxResult.OK)
            {
            }
            this.grdMain.Children.Remove(window);
        }

        #endregion

        #region [�ϼ�LOT ��] - dgSubLot_LoadedCellPresenter, dgSubLot_MouseDoubleClick(�������˾�), print_Button_Click(�����)
        private void dgSubLot_LoadedCellPresenter(object sender, C1.WPF.DataGrid.DataGridCellEventArgs e)
        {
            C1.WPF.DataGrid.C1DataGrid dataGrid = sender as C1.WPF.DataGrid.C1DataGrid;

            dataGrid.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (e.Cell.Presenter == null)
                {
                    return;
                }
                //link ������
                if (cboProcess.SelectedValue.Equals(Process.PACKAGING) && e.Cell.Column.Name.Equals("CSTID"))
                {
                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Blue);
                }
                else
                {
                    e.Cell.Presenter.Foreground = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#FF333333"));
                }

            }));

        }
        private void dgSubLot_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            try
            {
                if (dgSubLot.CurrentRow != null && dgSubLot.CurrentColumn.Name.Equals("CSTID"))
                {
                    FCS002_318_CELL wndPopup = new FCS002_318_CELL();
                    wndPopup.FrameOperation = FrameOperation;

                    if (wndPopup != null)
                    {
                        object[] Parameters = new object[4];
                        Parameters[0] = _LOTID;
                        Parameters[1] = Util.NVC(DataTableConverter.GetValue(dgSubLot.CurrentRow.DataItem, "LOTID"));
                        Parameters[2] = Util.NVC(DataTableConverter.GetValue(dgSubLot.CurrentRow.DataItem, "CSTID")); ;

                        C1WindowExtension.SetParameters(wndPopup, Parameters);

                        //this.Dispatcher.BeginInvoke(new Action(() => wndPopup.ShowModal()));
                        grdMain.Children.Add(wndPopup);
                        wndPopup.BringToFront();
                    }
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        private void print_Button_Click(object sender, RoutedEventArgs e)
        {
            Button bt = sender as Button;

            String sBoxID = Util.NVC(DataTableConverter.GetValue(bt.DataContext, "LOTID"));

            if (!sBoxID.Equals(""))
            {
                // ����..
                DataTable dtRslt = GetThermalPaperPrintingInfo(sBoxID);

                if (dtRslt == null || dtRslt.Rows.Count < 1)
                    return;

                List<Dictionary<string, string>> dicList = new List<Dictionary<string, string>>();
                Dictionary<string, string> dicParam = new Dictionary<string, string>();

                if (_PROCID.Equals(Process.LAMINATION))
                {
                    dicParam.Add("reportName", "Lami");
                    dicParam.Add("LOTID", Util.NVC(dtRslt.Rows[0]["LOTID_RT"]));
                    dicParam.Add("QTY", Convert.ToDouble(Util.NVC(dtRslt.Rows[0]["WIPQTY"])).ToString());
                    dicParam.Add("MAGID", Util.NVC(dtRslt.Rows[0]["LOTID"]));
                    dicParam.Add("MAGIDBARCODE", Util.NVC(dtRslt.Rows[0]["LOTID"]));
                    dicParam.Add("CASTID", Util.NVC(dtRslt.Rows[0]["CSTID"])); // ī��Ʈ ID �÷���??
                    dicParam.Add("MODEL", Util.NVC(dtRslt.Rows[0]["MODLID"]));
                    dicParam.Add("REGDATE", Util.NVC(dtRslt.Rows[0]["LOTDTTM_CR"]));
                    dicParam.Add("EQPTNO", Util.NVC(dtRslt.Rows[0]["EQPTSHORTNAME"]));
                    dicParam.Add("CELLTYPE", Util.NVC(dtRslt.Rows[0]["PRODUCT_LEVEL3_CODE"]));
                    dicParam.Add("TITLEX", "MAGAZINE ID");
                    dicParam.Add("B_LOTID", Util.NVC(dtRslt.Rows[0]["LOTID"]));
                    dicParam.Add("B_WIPSEQ", Util.NVC(dtRslt.Rows[0]["WIPSEQ"]));
                    dicParam.Add("RE_PRT_YN", "Y"); // ����� ����.
                    dicList.Add(dicParam);

                    CMM_THERMAL_PRINT_LAMI printlami = new CMM_THERMAL_PRINT_LAMI();
                    printlami.FrameOperation = FrameOperation;

                    if (printlami != null)
                    {
                        object[] Parameters = new object[7];
                        Parameters[0] = dicList;
                        Parameters[1] = Process.LAMINATION;
                        Parameters[2] = _EQSGID;
                        Parameters[3] = _EQPTID;
                        Parameters[4] = "Y";   // �Ϸ� �޽��� ǥ�� ����.
                        Parameters[5] = "Y";   // ����ġ ó��.
                        Parameters[6] = "MAGAZINE";   // ���� Type M:Magazine, B:Folded Box, R:Remain Pancake, N:�Ű����籸��(Folding����)

                        C1WindowExtension.SetParameters(printlami, Parameters);
                        printlami.Show();
                    }

                }
                else if (_PROCID.Equals(Process.STACKING_FOLDING))
                {
                    int iCopys = 2;

                    if (LoginInfo.CFG_THERMAL_COPIES > 0)
                    {
                        iCopys = LoginInfo.CFG_THERMAL_COPIES;
                    }

                    dicParam.Add("reportName", "Fold");
                    dicParam.Add("LOTID", Util.NVC(dtRslt.Rows[0]["LOTID_RT"]));
                    dicParam.Add("QTY", Convert.ToDouble(Util.NVC(dtRslt.Rows[0]["WIPQTY"])).ToString());
                    dicParam.Add("MAGID", Util.NVC(dtRslt.Rows[0]["LOTID"]));
                    dicParam.Add("MAGIDBARCODE", Util.NVC(dtRslt.Rows[0]["LOTID"]));
                    dicParam.Add("LARGELOT", Util.NVC(dtRslt.Rows[0]["CAL_DATE"]));  // ���� LOT�� �����ð�(����ð�����)
                    dicParam.Add("MODEL", Util.NVC(dtRslt.Rows[0]["MODLID"]));
                    dicParam.Add("REGDATE", Util.NVC(dtRslt.Rows[0]["LOTDTTM_CR"]));
                    dicParam.Add("EQPTNO", Util.NVC(dtRslt.Rows[0]["EQPTSHORTNAME"]));
                    dicParam.Add("TITLEX", "BASKET ID");
                    dicParam.Add("PRINTQTY", iCopys.ToString());  // ���� ��
                    dicParam.Add("B_LOTID", Util.NVC(dtRslt.Rows[0]["LOTID"]));
                    dicParam.Add("B_WIPSEQ", Util.NVC(dtRslt.Rows[0]["WIPSEQ"]));
                    dicParam.Add("RE_PRT_YN", "Y"); // ����� ����.
                    dicList.Add(dicParam);

                    LGC.GMES.MES.CMM001.CMM_THERMAL_PRINT_FOLD printfold = new LGC.GMES.MES.CMM001.CMM_THERMAL_PRINT_FOLD(dicParam);
                    printfold.FrameOperation = FrameOperation;

                    if (printfold != null)
                    {
                        object[] Parameters = new object[6];
                        Parameters[0] = dicList;
                        Parameters[1] = Process.STACKING_FOLDING;
                        Parameters[2] = _EQSGID;
                        Parameters[3] = _EQPTID;
                        Parameters[4] = "Y";   // �Ϸ� �޽��� ǥ�� ����.
                        Parameters[5] = "Y";   // ����ġ ó��.

                        C1WindowExtension.SetParameters(printfold, Parameters);
                        printfold.ShowModal();
                    }

                }
                ///[C20190415_74474] x-ray ����� ��ư �߰�
                else if (_PROCID.Equals(Process.XRAY_REWORK))
                {
                    //�����Ͻðڽ��ϱ�?
                    Util.MessageConfirm("SFU2873", (result) =>
                    {
                        if (result == MessageBoxResult.OK)
                        {
                            xRayPalletPrint(Util.NVC(dtRslt.Rows[0]["LOTID"]));
                        }
                    });
                }

            }
        }
        #endregion

        #region [�� ���� ����]
        private void tbcList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (((System.Windows.FrameworkElement)tbcList.SelectedItem).Name.Equals("Lot"))
                chkModel.IsChecked = false;
            else
                chkModel.IsChecked = true;

        }
        #endregion

        #region [Rollmap]
        private void btnRollMap_Click(object sender, RoutedEventArgs e)
        {
            // Roll Map ȣ�� 
            string mainFormPath = "LGC.GMES.MES.FCS002";
            string mainFormName = string.Empty;

            if (string.Equals(_PROCID, Process.COATING))
            {
                mainFormName = "COM001_ROLLMAP_COATER";
            }
            else if (string.Equals(_PROCID, Process.ROLL_PRESSING))
            {
                mainFormName = "COM001_ROLLMAP_ROLLPRESS_NEW";
            }
            else if (string.Equals(_PROCID, Process.SLITTING))
            {
                mainFormName = "COM001_ROLLMAP_SLITTING";
            }
            else if (string.Equals(_PROCID, Process.TWO_SLITTING))
            {
                mainFormName = "COM001_ROLLMAP_TWOSLITTING";
            }
            else if (string.Equals(_PROCID, Process.SLIT_REWINDING) || string.Equals(_PROCID, Process.REWINDING))
            {
                mainFormName = "COM001_ROLLMAP_REWINDER";
            }
            else
            {
                return;
            }

            System.Reflection.Assembly asm = System.Reflection.Assembly.LoadFrom("ClientBin\\" + mainFormPath + ".dll");
            Type targetType = asm.GetType(mainFormPath + "." + mainFormName);
            object obj = Activator.CreateInstance(targetType);

            IWorkArea workrollmap = obj as IWorkArea;
            workrollmap.FrameOperation = FrameOperation;

            object[] parameters = new object[10];
            parameters[0] = _PROCID;
            parameters[1] = _EQSGID;
            parameters[2] = _EQPTID;
            parameters[3] = _LOTID;
            parameters[4] = _WIPSEQ;
            parameters[5] = _LANE;
            parameters[6] = _EQPTNAME + " [" + _EQPTID + "]";
            parameters[7] = txtProdVerCode.Text;

            C1Window popup = obj as C1Window;
            C1WindowExtension.SetParameters(popup, parameters);
            if (popup != null)
            {
                popup.ShowModal();
                popup.CenterOnScreen();
            }
        }

        private void btnRollMapUpdate_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // RollMap ���� ���� Popup Call
                CMM_ROLLMAP_COATER_DEFECT popupRollMapUpdate = new CMM_ROLLMAP_COATER_DEFECT { FrameOperation = FrameOperation };

                if (popupRollMapUpdate != null)
                {
                    object[] Parameters = new object[10];
                    Parameters[0] = _PROCID;
                    Parameters[1] = _EQSGID;
                    Parameters[2] = _EQPTID;
                    Parameters[3] = _LOTID;
                    Parameters[4] = _WIPSEQ;
                    Parameters[5] = _LANE;
                    Parameters[6] = _EQPTNAME + " [" + _EQPTID + "]";
                    Parameters[7] = txtProdVerCode.Text;
                    Parameters[8] = "Y"; //Test Cut Visible false
                    Parameters[9] = "Y"; //Search Mode True

                    C1WindowExtension.SetParameters(popupRollMapUpdate, Parameters);

                    if (popupRollMapUpdate != null)
                    {
                        popupRollMapUpdate.ShowModal();
                        popupRollMapUpdate.CenterOnScreen();
                    }
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }

        }
        #endregion
        #endregion

        #region Mehod

        #region [BizCall]

        #region [### ������ AreaType ###]
        public void GetAreaType(string sProcID)
        {
            try
            {
                ShowLoadingIndicator();

                _AREATYPE = string.Empty;

                DataTable dtRqst = new DataTable();
                dtRqst.Columns.Add("PCSGID", typeof(string));
                dtRqst.Columns.Add("PROCID", typeof(string));

                DataRow dr = dtRqst.NewRow();
                dr["PROCID"] = sProcID;
                dtRqst.Rows.Add(dr);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_PROCESSSEGMENTPROCESS", "INDATA", "OUTDATA", dtRqst);

                if (dtRslt != null && dtRslt.Rows.Count > 0)
                    _AREATYPE = dtRslt.Rows[0]["PCSGID"].ToString();
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

        #region [### ���� ���ڷ� ��ȸ ###]
        public void GetCaldate()
        {
            try
            {
                ShowLoadingIndicator();

                DataTable dtRqst = new DataTable();
                dtRqst.Columns.Add("AREAID", typeof(string));
                dtRqst.Columns.Add("DTTM", typeof(string));

                DataRow dr = dtRqst.NewRow();
                dr["DTTM"] = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                dtRqst.Rows.Add(dr);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_CALDATE", "INDATA", "OUTDATA", dtRqst);

                if (dtRslt != null && dtRslt.Rows.Count > 0)
                {
                    dtpDateFrom.SelectedDateTime = Convert.ToDateTime(dtRslt.Rows[0]["CALDATE"].ToString());
                    dtpDateTo.SelectedDateTime = Convert.ToDateTime(dtRslt.Rows[0]["CALDATE"].ToString());
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

        #region [���� ���� ��� Visible]  
        private void EltrGrdCodeColumnVisible()
        {
            try
            {
                if (cboArea.SelectedValue == null || cboArea.SelectedValue.ToString() == "SELECT")
                {
                    dgLotList.Columns["ELTR_GRD_CODE"].Visibility = Visibility.Collapsed;
                    return;
                }

                DataTable inTable = new DataTable();
                inTable.TableName = "RQSTDT";
                inTable.Columns.Add("LANGID", typeof(string));
                inTable.Columns.Add("AREAID", typeof(string));
                inTable.Columns.Add("COM_TYPE_CODE", typeof(string));
                inTable.Columns.Add("COM_CODE", typeof(string));

                DataRow dr = inTable.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["AREAID"] = cboArea.SelectedValue.ToString();
                dr["COM_TYPE_CODE"] = "ELTR_GRD_JUDG_ITEM_CODE";
                inTable.Rows.Add(dr);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_TB_MMD_AREA_COM_CODE", "INDATA", "OUTDATA", inTable);

                if (dtRslt != null && dtRslt.Rows.Count > 0)
                {
                    if (dgLotList.Columns.Contains("ELTR_GRD_CODE"))
                        dgLotList.Columns["ELTR_GRD_CODE"].Visibility = Visibility.Visible;
                }
                else
                {
                    if (dgLotList.Columns.Contains("ELTR_GRD_CODE"))
                        dgLotList.Columns["ELTR_GRD_CODE"].Visibility = Visibility.Collapsed;
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        #endregion

        #region [����, ������ Carrier ID Visible]
        /// <summary>
        /// �� �Ӽ� : ���� �� �׷� Code�� �����ϸ�, ������ Coating, Roll Press, Slitting �� ���� CSTID �÷� Visible
        /// </summary>
        private void SetCarrierVisible()
        {
            try
            {
                dgLotList.Columns["PR_CSTID"].Visibility = Visibility.Collapsed;
                dgLotList.Columns["CSTID"].Visibility = Visibility.Collapsed;

                if (cboArea.SelectedValue != null && !cboArea.SelectedValue.ToString().Equals("SELECT") && cboProcess.SelectedValue != null && !cboProcess.SelectedValue.ToString().Equals("SELECT"))
                {
                    DataTable inTable = new DataTable();
                    inTable.TableName = "RQSTDT";
                    inTable.Columns.Add("LANGID", typeof(string));
                    inTable.Columns.Add("AREAID", typeof(string));

                    DataRow dr = inTable.NewRow();
                    dr["LANGID"] = LoginInfo.LANGID;
                    dr["AREAID"] = LoginInfo.CFG_AREA_ID;
                    inTable.Rows.Add(dr);

                    DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_AREA_LOGIS_GROUP_CBO", "RQSTDT", "RSLTDT", inTable);

                    if (dtRslt != null && dtRslt.Rows.Count > 0)
                    {
                        if (cboProcess.SelectedValue.Equals(Process.COATING))
                        {
                            dgLotList.Columns["CSTID"].Visibility = Visibility.Visible;
                        }
                        else if (cboProcess.SelectedValue.Equals(Process.ROLL_PRESSING)
                                || cboProcess.SelectedValue.Equals(Process.SLITTING))
                        {
                            dgLotList.Columns["PR_CSTID"].Visibility = Visibility.Visible;
                            dgLotList.Columns["CSTID"].Visibility = Visibility.Visible;
                        }
                    }

                    if (LoginInfo.CFG_AREA_ID == "A3" && cboProcess.SelectedValue.Equals(Process.NOTCHING))
                        dgLotList.Columns["CSTID"].Visibility = Visibility.Visible;
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        #endregion

        #region [### �۾���� ��ȸ ###]
        public void GetLotList()
        {
            try
            {
                if ((dtpDateTo.SelectedDateTime - dtpDateFrom.SelectedDateTime).TotalDays > 31)
                {
                //    //Util.AlertInfo("SFU2042", new object[] { "7" });   //�Ⱓ�� {0}�� �̳� �Դϴ�.
                    Util.MessageValidation("SFU2042", "31");
                    return;
                }

                if (string.Equals(GetAreaType(), "E"))
                {
                    WIP_BCD_PRT_COUNT.Visibility = Visibility.Visible;
                    WIP_BCD_PRT_EXEC_COUNT.Visibility = Visibility.Visible;
                }

                //2022-12-29 ��ȭ��  �� :EP �߰� 
                if ((cboArea.SelectedValue.Equals("E5")|| cboArea.SelectedValue.Equals("EP")) && cboProcess.SelectedValue.Equals(Process.COATING))
                {
                    dgLotList.Columns["EQPT_DFCT_TOTAL_QTY"].Visibility = Visibility.Visible;
                }
                else
                {
                    dgLotList.Columns["EQPT_DFCT_TOTAL_QTY"].Visibility = Visibility.Collapsed;
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
                dtRqst.Columns.Add("LOTID", typeof(string));
                dtRqst.Columns.Add("AREATYPE", typeof(string));
                dtRqst.Columns.Add("FLOWTYPE", typeof(string));
                dtRqst.Columns.Add("TOPBACK", typeof(string));
                dtRqst.Columns.Add("FROM_DATE", typeof(string));
                dtRqst.Columns.Add("TO_DATE", typeof(string));
                dtRqst.Columns.Add("PR_LOTID", typeof(string));
                dtRqst.Columns.Add("MODLID", typeof(string));
                dtRqst.Columns.Add("PRODID", typeof(string));
                dtRqst.Columns.Add("RUNYN", typeof(string));
                dtRqst.Columns.Add("PRDT_CLSS_CODE", typeof(string));
                dtRqst.Columns.Add("PRJT_NAME", typeof(string));
                dtRqst.Columns.Add("MKT_TYPE_CODE", typeof(string));
                dtRqst.Columns.Add("PILOT_PROD_DIVS_CODE", typeof(string)); // 2021.08.23 : ���걸�� �߰�

                DataRow dr = dtRqst.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;

                dr["AREAID"] = Util.GetCondition(cboArea, MessageDic.Instance.GetMessage("SFU1499"));
                if (dr["AREAID"].Equals("")) return;

                ////dr["EQSGID"] = Util.GetCondition(cboEquipmentSegment, "�����������ϼ���.");
                //dr["EQSGID"] = Util.GetCondition(cboEquipmentSegment, MessageDic.Instance.GetMessage("SFU1223"));
                //if (dr["EQSGID"].Equals("")) return;

                string sEquipmentSegment = Util.GetCondition(cboEquipmentSegment);
                dr["EQSGID"] = string.IsNullOrWhiteSpace(sEquipmentSegment) ? null : sEquipmentSegment;

                //dr["PROCID"] = Util.GetCondition(cboProcess, "�����������ϼ���.");
                dr["PROCID"] = Util.GetCondition(cboProcess, MessageDic.Instance.GetMessage("SFU1459"));
                if (dr["PROCID"].Equals("")) return;

                //dr["EQPTID"] = Util.GetCondition(cboEquipment, bAllNull: true);
                string sEqptID = Util.GetCondition(cboEquipment);
                dr["EQPTID"] = string.IsNullOrWhiteSpace(sEqptID) ? null : sEqptID;

                dr["FLOWTYPE"] = Util.GetCondition(cboFlowType, bAllNull: true);

                if (cboTopBack.Visibility.Equals(Visibility.Visible))
                    dr["TOPBACK"] = Util.GetCondition(cboTopBack, bAllNull: true);
                if (cboElecType.Visibility.Equals(Visibility.Visible))
                    dr["PRDT_CLSS_CODE"] = Util.GetCondition(cboElecType, bAllNull: true);

                dr["FROM_DATE"] = Util.GetCondition(dtpDateFrom);
                dr["TO_DATE"] = Util.GetCondition(dtpDateTo);
                dr["AREATYPE"] = _AREATYPE;

                if (cboProcess.SelectedValue.Equals(Process.SLIT_REWINDING) || cboProcess.SelectedValue.Equals(Process.SLITTING) || cboProcess.SelectedValue.Equals(Process.HEAT_TREATMENT) || cboProcess.SelectedValue.Equals(Process.ROLL_PRESSING))
                {
                    FRST_MKT.Visibility = Visibility.Visible;
                }
                else
                {
                    FRST_MKT.Visibility = Visibility.Collapsed;
                }

                // ���� �÷��� ������, �ܷ�, LOSS ���� ������ WINDING ���� ���ϴ� ��İ� ���� �ʾ� WINDING ������ �ű� �÷� �߰���
                if (cboProcess.SelectedValue.Equals(Process.WINDING))
                {
                    dgInputHist.Columns["INPUT_QTY"].Header = new object[] { ObjectDic.Instance.GetObjectName("���Է�"), ObjectDic.Instance.GetObjectName("���Է�") };
                    dgInputHist.Columns["USED_QTY"].Visibility = Visibility.Visible;
                    dgInputHist.Columns["REMAIN_QTY"].Visibility = Visibility.Visible;
                    dgInputHist.Columns["LOSS_QTY"].Visibility = Visibility.Visible;
                }
                else
                {
                    dgInputHist.Columns["INPUT_QTY"].Header = new object[] { ObjectDic.Instance.GetObjectName("������"), ObjectDic.Instance.GetObjectName("������") };
                    dgInputHist.Columns["USED_QTY"].Visibility = Visibility.Collapsed;
                    dgInputHist.Columns["REMAIN_QTY"].Visibility = Visibility.Collapsed;
                    dgInputHist.Columns["LOSS_QTY"].Visibility = Visibility.Collapsed;
                }

                if (!string.IsNullOrWhiteSpace(txtPRLOTID.Text))
                {
                    dr["PR_LOTID"] = Util.GetCondition(txtPRLOTID);
                }
                else if (!string.IsNullOrWhiteSpace(txtLotId.Text))
                {
                    dr["LOTID"] = Util.GetCondition(txtLotId);
                    bLot = true;
                }

                if (!string.IsNullOrWhiteSpace(txtModlId.Text))
                    dr["MODLID"] = txtModlId.Text;

                if (!string.IsNullOrWhiteSpace(txtProdId.Text))
                    dr["PRODID"] = txtProdId.Text;

                if (!string.IsNullOrWhiteSpace(txtPrjtName.Text))
                    dr["PRJT_NAME"] = txtPrjtName.Text;

                if (chkProc.IsChecked == false)
                    dr["RUNYN"] = "Y";

                dr["MKT_TYPE_CODE"] = Util.GetCondition(cboMKTtype, bAllNull: true);
                dr["PILOT_PROD_DIVS_CODE"] = Util.GetCondition(cboProductDiv, bAllNull: true);    // 2021.08.23 : ���걸�� �߰�

                dtRqst.Rows.Add(dr);

                string sBizName = string.Empty;

                if (((System.Windows.FrameworkElement)tbcList.SelectedItem).Name.Equals("Lot"))
                {
                    if (cboProcess.SelectedValue.ToString().Equals(Process.ASSEMBLY) || cboProcess.SelectedValue.ToString().Equals(Process.WASHING))
                        sBizName = "DA_PRD_SEL_LOT_LIST_MOBILE";
                    else
                        sBizName = "DA_PRD_SEL_LOT_LIST";
                }
                else
                {
                    if (cboProcess.SelectedValue.ToString().Equals(Process.ASSEMBLY) || cboProcess.SelectedValue.ToString().Equals(Process.WASHING))
                        sBizName = "DA_PRD_SEL_LOT_LIST_MODEL_MOBILE";
                    else
                        sBizName = "DA_PRD_SEL_LOT_LIST_MODEL";  //DA.PRD_LOT_INFO
                }


                DataTable dtRslt = new ClientProxy().ExecuteServiceSync(sBizName, "INDATA", "OUTDATA", dtRqst);

                if (((System.Windows.FrameworkElement)tbcList.SelectedItem).Name.Equals("Lot"))
                    Util.GridSetData(dgLotList, dtRslt, FrameOperation, true);
                else
                    Util.GridSetData(dgModelList, dtRslt, FrameOperation, true);

                if (dtRslt != null && dtRslt.Rows.Count > 0 && bLot == true)
                {
                    _AREATYPE = dtRslt.Rows[0]["AREATYPE"].ToString();
                    AreaCheck(dtRslt.Rows[0]["PROCID"].ToString());
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

        #region [### �ҷ�/Loss/��ǰû�� ��ȸ ###]
        private void GetDefectInfo()
        {
            try
            {
                ShowLoadingIndicator();
                DoEvents();

                /* AZS ���� �ҷ�/Loss�׸� ������ ���� �غ� ���� */
                bool bAZS = false;

                DataTable inComTable = new DataTable();
                inComTable.Columns.Add("CMCODE", typeof(string));
                inComTable.Columns.Add("CMCDTYPE", typeof(string));
                inComTable.Columns.Add("LANGID", typeof(string));

                DataRow newComRow = inComTable.NewRow();
                newComRow["CMCODE"] = _EQPTID;
                newComRow["CMCDTYPE"] = "EQPT_EXCEPT_WORK_CALENDAR";
                newComRow["LANGID"] = LoginInfo.LANGID;
                inComTable.Rows.Add(newComRow);
                
                DataTable dtCom = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_COMMONCODE_TBL", "RQSTDT", "RSLTDT", inComTable);

                if (dtCom.Rows.Count > 0)
                    bAZS = true;
                /* AZS ���� �ҷ�/Loss�׸� ������ ���� �غ� �� */

                string BizNAme = string.Empty;
                if (bAZS)
                    BizNAme = "DA_QCA_SEL_AZSREASON_L";
                else if (_PROCID.Equals(Process.WINDING) || _PROCID.Equals(Process.ASSEMBLY) || _PROCID.Equals(Process.WASHING))
                    BizNAme = "BR_PRD_GET_WIPRESONCOLLECT_BY_MNGT_TYPE";  //LOT�� �ҷ� ���� ��ȸ -���κ�
                else
                    //C20210222-000365 �ҷ�/Loss�׸� ǥ��ȭ ���� DA_QCA_SEL_WIPRESONCOLLECT_INFO -> BR_PRD_SEL_WIPRESONCOLLECT_INFO ����
                    BizNAme = "BR_PRD_SEL_WIPRESONCOLLECT_INFO";

                // ���� CT�˻� CHECK �׸� ����
                if (_PROCID.Equals(Process.CT_INSP) && _qmsDefectCodeYN == "N")
                    BizNAme = "BR_PRD_SEL_WIPRESONCOLLECT_INFO";

                DataTable inTable = _Biz.GetDA_QCA_SEL_WIPRESONCOLLECT();

                DataRow newRow = inTable.NewRow();
                newRow["LANGID"] = LoginInfo.LANGID;
                newRow["AREAID"] = _AREAID;
                newRow["PROCID"] = _PROCID;
                newRow["EQPTID"] = _EQPTID;
                newRow["LOTID"] = _LOTID;
                newRow["WIPSEQ"] = _WIPSEQ;
                newRow["ACTID"] = "DEFECT_LOT,LOSS_LOT,CHARGE_PROD_LOT";

                inTable.Rows.Add(newRow);

                if (_PROCID.Equals(Process.WINDING) || _PROCID.Equals(Process.ASSEMBLY) || _PROCID.Equals(Process.WASHING))
                {
                    if (dgDefect.Columns.Contains("RESNNAME"))
                        dgDefect.Columns["RESNNAME"].Visibility = Visibility.Collapsed;
                    if (dgDefect.Columns.Contains("DFCT_CODE_DETL_NAME"))
                        dgDefect.Columns["DFCT_CODE_DETL_NAME"].Visibility = Visibility.Visible;
                    if (dgDefect.Columns.Contains("DFCT_PART_NAME"))
                        dgDefect.Columns["DFCT_PART_NAME"].Visibility = Visibility.Visible;

                    DataSet ds = new DataSet();
                    ds.Tables.Add(inTable);

                    DataSet dsResult = new ClientProxy().ExecuteServiceSync_Multi(BizNAme, "INDATA", "OUTTYPE,OUTDATA", ds, FrameOperation.MENUID);
                    if (CommonVerify.HasTableInDataSet(dsResult))
                    {
                        //'AP' ���� / ������
                        //'LP' ���� / ������
                        dgDefect.Columns["CLSS_NAME1"].Visibility = dsResult.Tables["OUTTYPE"].Rows[0]["DFCT_CODE_MNGT_TYPE"].GetString() == "LP" ? Visibility.Visible : Visibility.Collapsed;
                        dgDefect.ItemsSource = DataTableConverter.Convert(dsResult.Tables["OUTDATA"]);
                    }


                }
                else if(_PROCID.Equals(Process.CT_INSP) && _qmsDefectCodeYN == "N")
                {
                    dgDefect.Columns["CLSS_NAME1"].Visibility = Visibility.Collapsed;

                    DataTable dtResult = new ClientProxy().ExecuteServiceSync(BizNAme, "INDATA", "OUTDATA", inTable);
                    Util.GridSetData(dgDefect, dtResult, null);

                    if ((dtResult?.Rows?.Count > 0 && dtResult.Columns.Contains("DFCT_SYS_TYPE") && Util.NVC(dtResult.Rows[0]["DFCT_SYS_TYPE"]).Equals("Q")))
                    {
                        if (dgDefect.Columns.Contains("RESNNAME"))
                            dgDefect.Columns["RESNNAME"].Visibility = Visibility.Visible;
                        if (dgDefect.Columns.Contains("DFCT_CODE_DETL_NAME"))
                            dgDefect.Columns["DFCT_CODE_DETL_NAME"].Visibility = Visibility.Collapsed;
                        if (dgDefect.Columns.Contains("DFCT_PART_NAME"))
                            dgDefect.Columns["DFCT_PART_NAME"].Visibility = Visibility.Collapsed;
                    }
                    else
                    {
                        if (dgDefect.Columns.Contains("RESNNAME"))
                            dgDefect.Columns["RESNNAME"].Visibility = Visibility.Collapsed;
                        if (dgDefect.Columns.Contains("DFCT_CODE_DETL_NAME"))
                            dgDefect.Columns["DFCT_CODE_DETL_NAME"].Visibility = Visibility.Visible;
                        if (dgDefect.Columns.Contains("DFCT_PART_NAME"))
                            dgDefect.Columns["DFCT_PART_NAME"].Visibility = Visibility.Visible;
                    }
                }
                else
                {
                    dgDefect.Columns["CLSS_NAME1"].Visibility = Visibility.Collapsed;

                    DataTable dtResult = new ClientProxy().ExecuteServiceSync(BizNAme, "INDATA", "OUTDATA", inTable);
                    Util.GridSetData(dgDefect, dtResult, null);
                    
                    if (bAZS || (dtResult?.Rows?.Count > 0 && dtResult.Columns.Contains("DFCT_SYS_TYPE") && Util.NVC(dtResult.Rows[0]["DFCT_SYS_TYPE"]).Equals("Q")))
                    {
                        if (dgDefect.Columns.Contains("RESNNAME"))
                            dgDefect.Columns["RESNNAME"].Visibility = Visibility.Visible;
                        if (dgDefect.Columns.Contains("DFCT_CODE_DETL_NAME"))
                            dgDefect.Columns["DFCT_CODE_DETL_NAME"].Visibility = Visibility.Collapsed;
                        if (dgDefect.Columns.Contains("DFCT_PART_NAME"))
                            dgDefect.Columns["DFCT_PART_NAME"].Visibility = Visibility.Collapsed;
                    }
                    else
                    {
                        if (dgDefect.Columns.Contains("RESNNAME"))
                            dgDefect.Columns["RESNNAME"].Visibility = Visibility.Collapsed;
                        if (dgDefect.Columns.Contains("DFCT_CODE_DETL_NAME"))
                            dgDefect.Columns["DFCT_CODE_DETL_NAME"].Visibility = Visibility.Visible;
                        if (dgDefect.Columns.Contains("DFCT_PART_NAME"))
                            dgDefect.Columns["DFCT_PART_NAME"].Visibility = Visibility.Visible;
                    }
                }


                // Folding�� ��� 'FOLDED CELL' ���� ǥ�� STACKING �� ��� 'STAKCING CELL' ���� ǥ��
                // �׿� �ٸ� �����ϰ�� '����'���� ǥ��
                if (_PROCID.Equals(Process.STACKING_FOLDING))
                {
                    chkFoldingStacking();

                    if (_EQGRID=="STK")
                    {
                        dgDefect.Columns["RESNQTY"].Header = "STACKING CELL";
                    }
                    else
                    {
                        dgDefect.Columns["RESNQTY"].Header = "FOLDED CELL";
                    }
                }
                else
                {
                    dgDefect.Columns["RESNQTY"].Header = "����";
                }

                if (_StackingYN.Equals("Y"))
                {

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

        #region [### ����ҷ����� ��ȸ ###]
        private void GetEqpFaultyData() //string sLot, string sWipSeq)
        {
            try
            {
                ShowLoadingIndicator();
                DoEvents();

                string sBizRule = string.Empty;
                if (_PROCID.Equals(Process.WINDING) || _PROCID.Equals(Process.ASSEMBLY) || _PROCID.Equals(Process.WASHING))
                {
                    sBizRule = "DA_EQP_SEL_EQPTDFCT_INFO_HIST";
                }
                else
                {
                    sBizRule = "DA_EQP_SEL_EQPTDFCT_INFO";
                }

                DataTable inTable = _Biz.GetDA_EQP_SEL_EQPTDFCT_INFO();

                DataRow newRow = inTable.NewRow();
                newRow["LANGID"] = LoginInfo.LANGID;
                newRow["EQPTID"] = _EQPTID;
                newRow["LOTID"] = _LOTID;
                newRow["WIPSEQ"] = _WIPSEQ;

                inTable.Rows.Add(newRow);

                new ClientProxy().ExecuteService(sBizRule, "INDATA", "OUTDATA", inTable, (searchResult, searchException) =>
                {
                    try
                    {
                        if (searchException != null)
                        {
                            Util.MessageException(searchException);
                            return;
                        }

                        //dgEqpFaulty.ItemsSource = DataTableConverter.Convert(searchResult);
                        Util.GridSetData(dgEqpFaulty, searchResult, FrameOperation, true);
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
                );
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

        #region [### ǰ������ ��ȸ ###]
        private void GetQuality()
        {
            try
            {
                ShowLoadingIndicator();
                DoEvents();

                DataTable dtRqst = new DataTable();
                dtRqst.Columns.Add("LANGID", typeof(string));
                dtRqst.Columns.Add("AREAID", typeof(string));
                dtRqst.Columns.Add("PROCID", typeof(string));
                dtRqst.Columns.Add("LOTID", typeof(string));
                dtRqst.Columns.Add("WIPSEQ", typeof(Int16));

                if (_PROCID.Equals(Process.ASSEMBLY) || _PROCID.Equals(Process.WINDING))
                {
                    dtRqst.Columns.Add("CLCT_PONT_CODE", typeof(string));
                    dtRqst.Columns.Add("CLCTITEM_CLSS4", typeof(string));
                    dtRqst.Columns.Add("CLCTITEM_CLSS3", typeof(string));
                    dtRqst.Columns.Add("EQPTID", typeof(string));
                    dtRqst.Columns.Add("CLCT_BAS_CODE", typeof(string));
                }

                DataRow dr = dtRqst.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                //dr["AREAID"] = LoginInfo.CFG_AREA_ID;
                dr["AREAID"] = _AREAID;
                dr["PROCID"] = _PROCID;
                dr["LOTID"] = _LOTID;
                dr["WIPSEQ"] = _WIPSEQ;

                if (_PROCID.Equals(Process.ASSEMBLY) || _PROCID.Equals(Process.WINDING))
                {
                    dr["EQPTID"] = _EQPTID;
                }
                dtRqst.Rows.Add(dr);

                string BizName = string.Empty;
                if (_PROCID.Equals(Process.ASSEMBLY) || _PROCID.Equals(Process.WINDING))
                    BizName = "DA_QCA_SEL_SELF_INSP_CLCTITEM_LOT";
                else
                    BizName = "DA_QCA_SEL_WIPDATACOLLECT_LOT_PROC_END";

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync(BizName, "INDATA", "OUTDATA", dtRqst);

                //Util.gridClear(dgQualityInfo);
                //dgQualityInfo.ItemsSource = DataTableConverter.Convert(dtRslt);
                Util.GridSetData(dgQualityInfo, dtRslt, null);
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

        #region [### �������� ��ȸ ###]
        private void GetColor()
        {
            try
            {
                if (_PROCID != Process.ROLL_PRESSING)
                    return;

                ShowLoadingIndicator();
                DoEvents();

                DataTable dtRqst = new DataTable();
                dtRqst.Columns.Add("LANGID", typeof(string));
                dtRqst.Columns.Add("AREAID", typeof(string));
                dtRqst.Columns.Add("LOTID", typeof(string));
                dtRqst.Columns.Add("EQPTID", typeof(string));


                DataRow dr = dtRqst.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                //dr["AREAID"] = LoginInfo.CFG_AREA_ID;
                dr["AREAID"] = _AREAID;
                dr["LOTID"] = _LOTID;
                dr["EQPTID"] = _EQPTID;

                dtRqst.Rows.Add(dr);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_QUALITY_COLORTAG_LOT", "INDATA", "RSLTDT", dtRqst);

                Util.GridSetData(dgColor, dtRslt, FrameOperation);
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
        private void GetSubLot()
        {
            try
            {
                ShowLoadingIndicator();
                DoEvents();

                string sBizName = string.Empty;

                sBizName = "DA_PRD_SEL_EDIT_SUBLOT_LIST";

                DataTable dtRqst = new DataTable();
                dtRqst.Columns.Add("LANGID", typeof(string));
                dtRqst.Columns.Add("PR_LOTID", typeof(string));

                DataRow dr = dtRqst.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["PR_LOTID"] = _LOTID;

                dtRqst.Rows.Add(dr);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync(sBizName, "INDATA", "OUTDATA", dtRqst);

                //Util.gridClear(dgLotList);
                //dgLotList.ItemsSource = DataTableConverter.Convert(dtRslt);

                Util.GridSetData(dgSubLot, dtRslt, FrameOperation);
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

        #region [### �����̷� ��ȸ ###]
        private void GetInputHistory()
        {
            try
            {
                if (!cTabInputHalf.Visibility.Equals(Visibility.Visible)) return;

                ShowLoadingIndicator();
                DoEvents();

                // ���� �𸶿�Ʈ Ÿ�� ��ȸ
                _INPUT_LOT_UNMOUNT_TYPE_CODE = GetInputUnMountType();

                string sBizName = string.Empty;

                if (_INPUT_LOT_UNMOUNT_TYPE_CODE.Equals("UNMOUNT"))             // 3Loss �̷� ���� Ÿ��.
                    sBizName = "DA_PRD_SEL_INPUT_MTRL_HIST_END_3LOSS";
                else if (_INPUT_LOT_UNMOUNT_TYPE_CODE.Equals("UNMOUNT_LOSS"))   // CNB, CWA3�� 3Loss Ÿ��.
                    sBizName = "DA_PRD_SEL_INPUT_MTRL_HIST_END_L";
                else
                    sBizName = "DA_PRD_SEL_INPUT_MTRL_HIST_END";

                DataTable inTable = new DataTable();
                inTable.Columns.Add("LANGID", typeof(string));
                inTable.Columns.Add("PROD_LOTID", typeof(string));
                inTable.Columns.Add("PROD_WIPSEQ", typeof(int));
                inTable.Columns.Add("INPUT_LOTID", typeof(string));
                inTable.Columns.Add("EQPT_MOUNT_PSTN_ID", typeof(string));

                DataRow newRow = inTable.NewRow();
                newRow["LANGID"] = LoginInfo.LANGID;
                newRow["PROD_LOTID"] = _LOTID;
                newRow["PROD_WIPSEQ"] = _WIPSEQ;
                newRow["INPUT_LOTID"] = null;
                newRow["EQPT_MOUNT_PSTN_ID"] = null;

                inTable.Rows.Add(newRow);

                new ClientProxy().ExecuteService(sBizName, "INDATA", "OUTDATA", inTable, (searchResult, searchException) =>
                {
                    try
                    {
                        if (searchException != null)
                        {
                            //Util.AlertByBiz("DA_PRD_SEL_INPUT_MTRL_HIST", searchException.Message, searchException.ToString());
                            //LGC.GMES.MES.ControlsLibrary.MessageBox.ShowKeyEnter(searchException.Message, searchException.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
                            Util.MessageException(searchException);
                            return;
                        }

                        switch (_INPUT_LOT_UNMOUNT_TYPE_CODE)
                        {
                            case "UNMOUNT":         // 3Loss �̷� ���� Ÿ��.
                                if (dgInputHist.Columns.Contains("PRE_PROC_LOSS_QTY"))
                                    dgInputHist.Columns["PRE_PROC_LOSS_QTY"].Visibility = Visibility.Visible;
                                if (dgInputHist.Columns.Contains("FIX_LOSS_QTY"))
                                    dgInputHist.Columns["FIX_LOSS_QTY"].Visibility = Visibility.Visible;
                                if (dgInputHist.Columns.Contains("CURR_PROC_LOSS_QTY"))
                                    dgInputHist.Columns["CURR_PROC_LOSS_QTY"].Visibility = Visibility.Visible;
                                if (dgInputHist.Columns.Contains("RMN_QTY"))
                                    dgInputHist.Columns["RMN_QTY"].Visibility = Visibility.Visible;
                                if (dgInputHist.Columns.Contains("EQPT_INPUT_PTN_QTY"))
                                    dgInputHist.Columns["EQPT_INPUT_PTN_QTY"].Visibility = Visibility.Visible;

                                if (dgInputHist.Columns.Contains("INPUT_QTY"))
                                    dgInputHist.Columns["INPUT_QTY"].Visibility = Visibility.Collapsed;
                                if (dgInputHist.Columns.Contains("WIPQTY_IN"))
                                    dgInputHist.Columns["WIPQTY_IN"].Visibility = Visibility.Visible;

                                break;
                            case "UNMOUNT_LOSS":    // CNB, CWA3�� 3Loss Ÿ��.
                                if (dgInputHist.Columns.Contains("PRE_PROC_LOSS_QTY")
                                   && (_PROCID.Equals(Process.NOTCHING) || _PROCID.Equals(Process.PACKAGING)))
                                    dgInputHist.Columns["PRE_PROC_LOSS_QTY"].Visibility = Visibility.Visible;
                                if (dgInputHist.Columns.Contains("FIX_LOSS_QTY")
                                   && (_PROCID.Equals(Process.NOTCHING) || _PROCID.Equals(Process.LAMINATION)))
                                    dgInputHist.Columns["FIX_LOSS_QTY"].Visibility = Visibility.Visible;
                                if (dgInputHist.Columns.Contains("CURR_PROC_LOSS_QTY")
                                   && (_PROCID.Equals(Process.NOTCHING) || _PROCID.Equals(Process.LAMINATION) || _PROCID.Equals(Process.STACKING_FOLDING)))
                                    dgInputHist.Columns["CURR_PROC_LOSS_QTY"].Visibility = Visibility.Visible;
                                if (dgInputHist.Columns.Contains("RMN_QTY"))
                                    dgInputHist.Columns["RMN_QTY"].Visibility = Visibility.Visible;
                                if (dgInputHist.Columns.Contains("EQPT_INPUT_PTN_QTY"))
                                    dgInputHist.Columns["EQPT_INPUT_PTN_QTY"].Visibility = Visibility.Collapsed;

                                if (dgInputHist.Columns.Contains("INPUT_QTY"))
                                    dgInputHist.Columns["INPUT_QTY"].Visibility = Visibility.Visible;
                                if (dgInputHist.Columns.Contains("WIPQTY_IN"))
                                    dgInputHist.Columns["WIPQTY_IN"].Visibility = Visibility.Visible;

                                break;
                            default:
                                if (dgInputHist.Columns.Contains("PRE_PROC_LOSS_QTY"))
                                    dgInputHist.Columns["PRE_PROC_LOSS_QTY"].Visibility = Visibility.Collapsed;
                                if (dgInputHist.Columns.Contains("FIX_LOSS_QTY"))
                                    dgInputHist.Columns["FIX_LOSS_QTY"].Visibility = Visibility.Collapsed;
                                if (dgInputHist.Columns.Contains("CURR_PROC_LOSS_QTY"))
                                    dgInputHist.Columns["CURR_PROC_LOSS_QTY"].Visibility = Visibility.Collapsed;
                                if (dgInputHist.Columns.Contains("RMN_QTY"))
                                    dgInputHist.Columns["RMN_QTY"].Visibility = Visibility.Collapsed;
                                if (dgInputHist.Columns.Contains("EQPT_INPUT_PTN_QTY"))
                                    dgInputHist.Columns["EQPT_INPUT_PTN_QTY"].Visibility = Visibility.Collapsed;

                                if (dgInputHist.Columns.Contains("INPUT_QTY"))
                                    dgInputHist.Columns["INPUT_QTY"].Visibility = Visibility.Visible;
                                if (dgInputHist.Columns.Contains("WIPQTY_IN"))
                                    dgInputHist.Columns["WIPQTY_IN"].Visibility = Visibility.Collapsed;

                                break;
                        }

                        Util.GridSetData(dgInputHist, searchResult, FrameOperation, true);
                    }
                    catch (Exception ex)
                    {
                        Util.MessageException(ex);
                        //Logger.Instance.WriteLine(Logger.OPERATION_R + "GetProductLot", ex);
                    }
                    finally
                    {
                        HiddenLoadingIndicator();
                    }
                }
                );
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

        #region [### �������� ��ȸ ###]
        private void GetInputMaterial()
        {
            try
            {
                if (!cTabInputMaterial.Visibility.Equals(Visibility.Visible)) return;

                ShowLoadingIndicator();
                DoEvents();

                string sBizName = string.Empty;

                if (_PROCID.Equals(Process.MIXING) || _PROCID.Equals(Process.PRE_MIXING) || _PROCID.Equals(Process.SRS_MIXING))  //MIXER��ü���� ���� Ȯ�� 
                    sBizName = "DA_PRD_SEL_CONSUME_MATERIAL_SUMMARY";
                else
                    sBizName = "DA_PRD_SEL_CONSUME_MATERIAL";

                DataTable dtRqst = new DataTable();
                dtRqst.Columns.Add("LANGID", typeof(string));
                dtRqst.Columns.Add("LOTID", typeof(string));
                if (_PROCID.Equals(Process.MIXING) || _PROCID.Equals(Process.PRE_MIXING) || _PROCID.Equals(Process.SRS_MIXING))  //MIXER��ü���� ���� Ȯ�� 
                    dtRqst.Columns.Add("WOID", typeof(string));

                DataRow dr = dtRqst.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["LOTID"] = _LOTID;
                if (_PROCID.Equals(Process.MIXING) || _PROCID.Equals(Process.PRE_MIXING) || _PROCID.Equals(Process.SRS_MIXING))  //MIXER��ü���� ���� Ȯ�� 
                    dr["WOID"] = txtWorkorder.Text;

                dtRqst.Rows.Add(dr);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync(sBizName, "INDATA", "OUTDATA", dtRqst);

                Util.GridSetData(dgMaterial, dtRslt, FrameOperation);

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

        #region [### �ҷ��±� ��ȸ ###]
        private void GetDefectTagList(string sLotID, int iWipSeq)
        {
            try
            {
                if (cTabDefectTag.Visibility != Visibility.Visible)
                    return;

                ShowLoadingIndicator();

                DataTable dtRqst = new DataTable();
                dtRqst.Columns.Add("LANGID", typeof(string));
                dtRqst.Columns.Add("AREAID", typeof(string));
                dtRqst.Columns.Add("LOTID", typeof(string));
                dtRqst.Columns.Add("WIPSEQ", typeof(string));

                DataRow dr = dtRqst.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                //dr["AREAID"] = LoginInfo.CFG_AREA_ID;
                dr["AREAID"] = _AREAID;
                dr["LOTID"] = sLotID;
                dr["WIPSEQ"] = Util.NVC(iWipSeq);

                dtRqst.Rows.Add(dr);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_QUALITY_DEFECT_TAG_WIP", "INDATA", "RSLTDT", dtRqst);

                Util.GridSetData(dgDefectTag, dtRslt, FrameOperation);
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

        #region [### ������ ��ȸ ###]
        private void GetSlurry()
        {
            try
            {
                if (!cTabSlurry.Visibility.Equals(Visibility.Visible)) return;

                ShowLoadingIndicator();
                DoEvents();

                DataTable dtRqst = new DataTable();
                dtRqst.Columns.Add("LOTID", typeof(string));


                DataRow dr = dtRqst.NewRow();
                dr["LOTID"] = _LOTID;

                dtRqst.Rows.Add(dr);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_WIPMTRL_CT", "INDATA", "OUTDATA", dtRqst);

                Util.GridSetData(dgSlurry, dtRslt, FrameOperation);

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

        #region[### Ư�̻��� ��ȸ ###]
        private void GetRemark()
        {
            try
            {
                ShowLoadingIndicator();
                DoEvents();

                rtxRemark.Document.Blocks.Clear();

                DataTable inTable = _Biz.GetDA_PRD_SEL_LOT_REMARK();

                DataRow newRow = inTable.NewRow();
                newRow["LANGID"] = LoginInfo.LANGID;
                newRow["LOTID"] = _LOTID;
                newRow["WIPSEQ"] = _WIPSEQ;

                inTable.Rows.Add(newRow);

                new ClientProxy().ExecuteService("DA_PRD_SEL_LOT_NOTE", "INDATA", "OUTDATA", inTable, (searchResult, searchException) =>
                {
                    try
                    {
                        if (searchException != null)
                        {
                            //Util.AlertByBiz("DA_PRD_SEL_LOT_NOTE", searchException.Message, searchException.ToString());
                            //LGC.GMES.MES.ControlsLibrary.MessageBox.ShowKeyEnter(searchException.Message, searchException.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
                            Util.MessageException(searchException);
                            return;
                        }

                        if (searchResult.Rows.Count > 0 && !Util.NVC(searchResult.Rows[0]["WIP_NOTE"]).Equals(""))
                            rtxRemark.AppendText(Util.NVC(searchResult.Rows[0]["WIP_NOTE"]));

                    }
                    catch (Exception ex)
                    {
                        Util.MessageException(ex);
                        //Logger.Instance.WriteLine(Logger.OPERATION_R + "GetProductLot", ex);
                    }
                    finally
                    {
                        HiddenLoadingIndicator();
                        //Logger.Instance.WriteLine(Logger.OPERATION_R + "GetProductLot", Logger.MESSAGE_OPERATION_END);
                    }
                }
                );
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

        #region [### �ҷ�/Loss/��ǰû�� - ATYPE, CTYPE �������� ###]
        private void GetMBOMInfo()
        {
            try
            {
                ShowLoadingIndicator();

                DataTable inTable = _Biz.GetDA_PRD_SEL_MBOM();

                DataRow newRow = inTable.NewRow();
                newRow["WO_DETL_ID"] = txtWorkorderDetail.Text;
                newRow["SHOPID"] = LoginInfo.CFG_SHOP_ID;

                inTable.Rows.Add(newRow);

                bool bAZS = false;

                new ClientProxy().ExecuteService("DA_PRD_SEL_MBOM_INFO", "INDATA", "OUTDATA", inTable, (searchResult, searchException) =>
                {
                    try
                    {
                        if (searchException != null)
                        {
                            Util.MessageException(searchException);
                            return;
                        }

                        if (searchResult == null || searchResult.Rows.Count < 1)
                        {
                            Util.MessageValidation("SFU1941");      //Ÿ�Ժ� �ҷ� ���������� �������� �ʽ��ϴ�.
                            return;
                        }
                        else
                        {
                            if (_StackingYN.Equals("Y"))
                            {
                                for (int i = 0; i < searchResult.Rows.Count; i++)
                                {
                                    if (Util.NVC(searchResult.Rows[i]["PRODUCT_LEVEL2_CODE"]).Equals("NC"))
                                    {
                                        //AZS ��ǰ
                                        if (Util.NVC(searchResult.Rows[i]["PRDT_CLSS_CODE"]).Equals("AN"))
                                        {
                                            txtInAType.Text = double.Parse(Util.NVC(searchResult.Rows[i]["PROC_INPUT_CNT"])).ToString();
                                        }
                                        else if (Util.NVC(searchResult.Rows[i]["PRDT_CLSS_CODE"]).Equals("CA"))
                                        {
                                            txtInCType.Text = double.Parse(Util.NVC(searchResult.Rows[i]["PROC_INPUT_CNT"])).ToString();
                                        }
                                        
                                        bAZS = true;
                                    }
                                    else
                                    {
                                        if (Util.NVC(searchResult.Rows[i]["PRODUCT_LEVEL2_CODE"]).Equals("HC"))
                                        {
                                            txtInAType.Text = double.Parse(Util.NVC(searchResult.Rows[i]["PROC_INPUT_CNT"])).ToString();
                                        }
                                        else if (Util.NVC(searchResult.Rows[i]["PRODUCT_LEVEL2_CODE"]).Equals("MC"))
                                        {
                                            txtInCType.Text = double.Parse(Util.NVC(searchResult.Rows[i]["PROC_INPUT_CNT"])).ToString();
                                        }
                                    }
                                }
                            }
                            else
                            {
                                for (int i = 0; i < searchResult.Rows.Count; i++)
                                {
                                    if (Util.NVC(searchResult.Rows[i]["PRODUCT_LEVEL3_CODE"]).Equals("AT"))
                                    {
                                        txtInAType.Text = double.Parse(Util.NVC(searchResult.Rows[i]["PROC_INPUT_CNT"])).ToString();
                                    }
                                    else if (Util.NVC(searchResult.Rows[i]["PRODUCT_LEVEL3_CODE"]).Equals("CT"))
                                    {
                                        txtInCType.Text = double.Parse(Util.NVC(searchResult.Rows[i]["PROC_INPUT_CNT"])).ToString();
                                    }
                                }
                            }

                            if (bAZS)
                            {
                                tbAType.Text = "AN";
                                tbCType.Text = "CA";

                                if (dgDefect != null && dgDefect.Columns.Contains("A_TYPE_DFCT_QTY"))
                                    dgDefect.Columns["A_TYPE_DFCT_QTY"].Header = "AN";
                                if (dgDefect != null && dgDefect.Columns.Contains("C_TYPE_DFCT_QTY"))
                                    dgDefect.Columns["C_TYPE_DFCT_QTY"].Header = "CA";
                                
                                dgDefect.Columns["RESNQTY"].Header = "AZS CELL";

                                if (txtInAType.Text.Equals(""))
                                {
                                    Util.MessageValidation("AN �ҷ� ���������� �������� �ʽ��ϴ�.");      //AN �ҷ� ���������� �������� �ʽ��ϴ�.
                                    return;
                                }

                                if (txtInCType.Text.Equals(""))
                                {
                                    Util.MessageValidation("CA �ҷ� ���������� �������� �ʽ��ϴ�.");      //CA �ҷ� ���������� �������� �ʽ��ϴ�.
                                    return;
                                }
                            }
                            else
                            {
                                if (txtInAType.Text.Equals(""))
                                {
                                    if (_StackingYN.Equals("Y"))
                                        Util.MessageValidation("SFU1337");      //HALF TYPE �ҷ� ���������� �������� �ʽ��ϴ�.
                                    else
                                        Util.MessageValidation("SFU1306");        //ATYPE �ҷ� ���������� �������� �ʽ��ϴ�.

                                    return;
                                }

                                if (txtInCType.Text.Equals(""))
                                {
                                    if (_StackingYN.Equals("Y"))
                                        Util.MessageValidation("SFU1401");     //MONO TYPE �ҷ� ���������� �������� �ʽ��ϴ�.
                                    else
                                        Util.MessageValidation("SFU1326");        //CTYPE �ҷ� ���������� �������� �ʽ��ϴ�.

                                    return;
                                }
                            }

                            //gDfctAQty = double.Parse(txtInAType.Text);
                            //gDfctCQty = double.Parse(txtInCType.Text);
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
                );
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

        #region [### ����� ###]
        private DataTable GetThermalPaperPrintingInfo(string sLotID)
        {
            try
            {
                ShowLoadingIndicator();

                DataTable inTable = _Biz.GetDA_PRD_SEL_THERMAL_PAPER_PRT_INFO();

                DataRow newRow = inTable.NewRow();
                newRow["AREAID"] = _AREAID;
                newRow["LOTID"] = sLotID;

                inTable.Rows.Add(newRow);

                return new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_THERMAL_PAPER_PRT_INFO", "INDATA", "OUTDATA", inTable);
            }
            catch (Exception ex)
            {
                //Util.AlertByBiz("DA_PRD_SEL_THERMAL_PAPER_PRT_INFO", ex.Message, ex.ToString());
                //LGC.GMES.MES.ControlsLibrary.MessageBox.ShowKeyEnter(ex.Message, ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
                Util.MessageException(ex);
                return null;
            }
            finally
            {
                HiddenLoadingIndicator();
            }
        }

        private void SetLabelPrtHist(string sZPL, DataRow drPrtInfo, string sLot, string sWipseq)
        {
            try
            {
                ShowLoadingIndicator();

                DataTable inTable = _Biz.GetBR_PRD_REG_LABEL_HIST();

                DataRow newRow = inTable.NewRow();
                //newRow["LABEL_CODE"] = "LBL0001";
                //newRow["LABEL_ZPL_CNTT"] = sZPL;
                newRow["LABEL_PRT_COUNT"] = "2";
                newRow["PRT_ITEM01"] = sLot;
                newRow["PRT_ITEM02"] = sWipseq;
                //newRow["PRT_ITEM03"] = "";
                //newRow["PRT_ITEM04"] = "";
                //newRow["PRT_ITEM05"] = "";
                //newRow["PRT_ITEM06"] = "";
                //newRow["PRT_ITEM07"] = "";
                //newRow["PRT_ITEM08"] = "";
                //newRow["PRT_ITEM09"] = "";
                //newRow["PRT_ITEM10"] = "";
                //newRow["PRT_ITEM11"] = "";
                //newRow["PRT_ITEM12"] = "";
                //newRow["PRT_ITEM13"] = "";
                //newRow["PRT_ITEM14"] = "";
                //newRow["PRT_ITEM15"] = "";
                //newRow["PRT_ITEM16"] = "";
                //newRow["PRT_ITEM17"] = "";
                //newRow["PRT_ITEM18"] = "";
                //newRow["PRT_ITEM19"] = "";
                //newRow["PRT_ITEM20"] = "";
                //newRow["PRT_ITEM21"] = "";
                //newRow["PRT_ITEM22"] = "";
                //newRow["PRT_ITEM23"] = "";
                //newRow["PRT_ITEM24"] = "";
                //newRow["PRT_ITEM25"] = "";
                //newRow["PRT_ITEM26"] = "";
                //newRow["PRT_ITEM27"] = "";
                //newRow["PRT_ITEM28"] = "";
                //newRow["PRT_ITEM29"] = "";
                //newRow["PRT_ITEM30"] = "";
                newRow["INSUSER"] = LoginInfo.USERID;

                inTable.Rows.Add(newRow);

                new ClientProxy().ExecuteServiceSync("BR_PRD_REG_LABEL_PRINT_HIST", "INDATA", null, inTable);
            }
            catch (Exception ex)
            {
                //Util.AlertByBiz("BR_PRD_REG_LABEL_PRINT_HIST", ex.Message, ex.ToString());
                //LGC.GMES.MES.ControlsLibrary.MessageBox.ShowKeyEnter(ex.Message, ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
                Util.MessageException(ex);
            }
            finally
            {
                HiddenLoadingIndicator();
            }

        }

        /// [C20190415_74474] x-ray ����� ��ư �߰�
        private void xRayPalletPrint(string sPalletId)
        {
            try
            {

                //string palletId = _util.GetDataGridFirstRowBycheck(dgOutPallet, "CHK").Field<string>("PALLETID").GetString();
                const string bizRuleName = "DA_PRD_SEL_PALLET_RUNCARD_DATA_ASSY_XR";
                DataTable inDataTable = new DataTable();
                inDataTable.Columns.Add("PALLET_ID", typeof(string));
                DataRow indata = inDataTable.NewRow();
                indata["PALLET_ID"] = sPalletId;
                inDataTable.Rows.Add(indata);

                new ClientProxy().ExecuteService(bizRuleName, "RQSTDT", "RSLTDT", inDataTable, (result, ex) =>
                {
                    loadingIndicator.Visibility = Visibility.Collapsed;
                    if (ex != null)
                    {
                        Util.MessageException(ex);
                        return;
                    }

                    if (CommonVerify.HasTableRow(result))
                    {
                        CMM_ASSY_PALLET_PRINT poopupPallet = new CMM_ASSY_PALLET_PRINT { FrameOperation = this.FrameOperation };
                        object[] parameters = new object[3];
                        parameters[0] = result;
                        parameters[1] = sPalletId;
                        parameters[2] = Process.XRAY_REWORK;
                        C1WindowExtension.SetParameters(poopupPallet, parameters);
                        poopupPallet.Closed += new EventHandler(poopupPallet_Closed);
                        this.Dispatcher.BeginInvoke(new Action(() => poopupPallet.ShowModal()));
                    }
                    else
                    {
                        //�����Ͱ� �����ϴ�.
                        Util.MessageValidation("SFU1498");
                        return;
                    }
                });
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void poopupPallet_Closed(object sender, EventArgs e)
        {
            CMM_ASSY_PALLET_PRINT popup = sender as CMM_ASSY_PALLET_PRINT;
            if (popup != null && popup.DialogResult == MessageBoxResult.OK)
            {
                //GetOutPallet();
            }
        }
        #endregion

        #region [### ���� ����ǰ ��ȸ ###]
        private void GetHalfProductList()
        {
            try
            {
                Util.gridClear(dgInHalfProduct);

                string bizRuleName = string.Equals(_PROCID, Process.ASSEMBLY) ? "DA_PRD_SEL_INPUT_HALFPROD_AS" : "DA_PRD_SEL_INPUT_HALFPROD_WS";
                const string materialType = "PROD";

                DataTable indataTable = new DataTable();
                indataTable.Columns.Add("LANGID", typeof(string));
                indataTable.Columns.Add("LOTID", typeof(string));
                indataTable.Columns.Add("MTRLTYPE", typeof(string));
                indataTable.Columns.Add("EQPT_MOUNT_PSTN_ID", typeof(string));
                indataTable.Columns.Add("EQPTID", typeof(string));
                indataTable.Columns.Add("PROD_LOTID", typeof(string));

                DataRow dr = indataTable.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["MTRLTYPE"] = "PROD";
                dr["EQPTID"] = _EQPTID;
                dr["PROD_LOTID"] = _LOTID;

                indataTable.Rows.Add(dr);
                //DataSet ds = new DataSet();
                //ds.Tables.Add(indataTable);
                //string xml = ds.GetXml();

                DataTable dt = new ClientProxy().ExecuteServiceSync(bizRuleName, "INDATA", "RSLTDT", indataTable);
                dgInHalfProduct.ItemsSource = DataTableConverter.Convert(dt);

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
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

        #region [Process ���� ��������]
        private void SetProcess()
        {
            try
            {
                // ���� �����ϼ���.
                string sArea = Util.GetCondition(cboArea);
                if (string.IsNullOrWhiteSpace(sArea))
                    return;

                string sEquipmentSegment = Util.GetCondition(cboEquipmentSegment);

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

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_PROCESS_BY_AREA_CBO", "RQSTDT", "RSLTDT", RQSTDT);

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

                string sEquipmentSegment = Util.GetCondition(cboEquipmentSegment);

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

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_EQUIPMENT_AREA_CBO_MB", "RQSTDT", "RSLTDT", RQSTDT);

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

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        #endregion

        #region [FOLDING & STACKING ����]
        //2017.11.21  INS���Թ� GEMS FOLDING ���� ���� �ܾ� ���� ��û CSR20171011_02178
        private void chkFoldingStacking()
        {

            DataTable inDataTable = new DataTable();
            inDataTable.Columns.Add("LOTID", typeof(string));

            DataRow newRow = inDataTable.NewRow();
            newRow["LOTID"] = _LOTID;

            inDataTable.Rows.Add(newRow);

            new ClientProxy().ExecuteService("DA_PRD_SEL_EQGRID_BYLOTID", "INDATA", "OUTDATA", inDataTable, (dtResult, searchException) =>
            {
                try
                {
                    if (searchException != null)
                    {
                        Util.MessageException(searchException);
                        return;
                    }

                    _EQGRID = dtResult.Rows[0]["EQGRID"].ToString();

                }
                catch (Exception ex)
                {
                    Util.MessageException(ex);
                }
                finally
                {
                    HiddenLoadingIndicator();
                }
            });
        }
        #endregion

        #region [���� ���� ����]
        private string GetAreaType()
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("AREAID", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["AREAID"] = LoginInfo.CFG_AREA_ID;
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_AREATYPE", "RQSTDT", "RSLTDT", RQSTDT);

                if (dtResult != null && dtResult.Rows.Count > 0)
                    return Util.NVC(dtResult.Rows[0]["AREA_TYPE_CODE"]);
            }
            catch (Exception ex) { }
            return "";
        }
        #endregion

        #region [���� Unmount Ÿ�� ��ȸ]
        private string GetInputUnMountType()
        {
            try
            {
                string sRet = string.Empty;
                
                DataTable RQSTDT = new DataTable();
                RQSTDT.Columns.Add("EQPTID", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["EQPTID"] = Util.NVC(_EQPTID);

                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_EQUIPMENTATTR", "RQSTDT", "RSLTDT", RQSTDT);

                if (dtResult?.Rows?.Count > 0)
                    if (dtResult.Columns.Contains("INPUT_LOT_UNMOUNT_TYPE_CODE"))
                        sRet = Util.NVC(dtResult.Rows[0]["INPUT_LOT_UNMOUNT_TYPE_CODE"]);

                return sRet;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return "";
            }
        }
        #endregion

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

        #region [�ʱ�ȭ]
        private void ClearValue()
        {
            txtSelectLot.Text = "";
            txtWorkorder.Text = "";
            txtWorkorderDetail.Text = "";

            txtWorkdate.Text = "";
            txtShift.Text = "";
            txtStartTime.Text = "";
            txtWorker.Text = "";
            txtInputQty.Value = 0;
            txtOutQty.Value = 0;
            txtDefectQty.Value = 0;
            txtLossQty.Value = 0;
            txtPrdtReqQty.Value = 0;
            txtProdVerCode.Text = "";
            txtLaneQty.Text = "";
            txtEndDttm.Text = "";
            txtNote.Text = "";
            txtWipWrkTypeCOde.Text = "";

            _AREAID = "";
            _PROCID = "";
            _EQSGID = "";
            _EQPTID = "";
            _LOTID = "";
            _WIPSEQ = "";
            _LANEPTNQTY = "";
            _ORGQTY = "";
            _WIP_NOTE = "";
            _StackingYN = "";
            _LANE = "";
            _EQPTNAME = "";

            txtInAType.Text = "";
            txtInCType.Text = "";

            Util.gridClear(dgDefect);
            Util.gridClear(dgQualityInfo);
            Util.gridClear(dgEqpFaulty);
            Util.gridClear(dgColor);
            Util.gridClear(dgSubLot);
            Util.gridClear(dgInputHist);
            Util.gridClear(dgDefectTag);
            Util.gridClear(dgTrayInfo);

            if (!cboProcess.SelectedValue.ToString().Equals(Process.STACKING_FOLDING))
            {
                grdMBomTypeCnt.Visibility = Visibility.Collapsed;
            }
            else
            {
                grdMBomTypeCnt.Visibility = Visibility.Visible;
            }

            #region ���� �̷� �÷� View �ʱ�ȭ.
            if (dgInputHist.Visibility == Visibility.Visible)
            {
                if (dgInputHist.Columns.Contains("PRE_PROC_LOSS_QTY"))
                    dgInputHist.Columns["PRE_PROC_LOSS_QTY"].Visibility = Visibility.Collapsed;
                if (dgInputHist.Columns.Contains("FIX_LOSS_QTY"))
                    dgInputHist.Columns["FIX_LOSS_QTY"].Visibility = Visibility.Collapsed;
                if (dgInputHist.Columns.Contains("CURR_PROC_LOSS_QTY"))
                    dgInputHist.Columns["CURR_PROC_LOSS_QTY"].Visibility = Visibility.Collapsed;
                if (dgInputHist.Columns.Contains("RMN_QTY"))
                    dgInputHist.Columns["RMN_QTY"].Visibility = Visibility.Collapsed;
                if (dgInputHist.Columns.Contains("EQPT_INPUT_PTN_QTY"))
                    dgInputHist.Columns["EQPT_INPUT_PTN_QTY"].Visibility = Visibility.Collapsed;

                if (dgInputHist.Columns.Contains("INPUT_QTY"))
                    dgInputHist.Columns["INPUT_QTY"].Visibility = Visibility.Visible;
                if (dgInputHist.Columns.Contains("WIPQTY_IN"))
                    dgInputHist.Columns["WIPQTY_IN"].Visibility = Visibility.Collapsed;
            }
            #endregion
            btnRollMap.Visibility = Visibility.Collapsed;
        }
        #endregion

        #region ��, Grid Column Visibility
        private void AreaCheck(string sProcID)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(sProcID) || sProcID.Equals("SELECT"))
                    return;

                //ROLL PRESS �����϶��� TAG���� ������� �ϴ� AREA���� üũ(CSRC20200508-000359)
                bool bTagView = RollPressTagCountViewCheck();
                
                chkPtnLen.IsChecked = false;
                TabReInput.Visibility = Visibility.Collapsed;
                tbInputDiffQty.Visibility = Visibility.Collapsed;
                txtInputDiffQty.Visibility = Visibility.Collapsed;
                tbLengthExceedQty.Visibility = Visibility.Collapsed;
                txtLengthExceedQty.Visibility = Visibility.Collapsed;
                tbLaneQty.Visibility = Visibility.Collapsed;
                tbProdVerCode.Visibility = Visibility.Collapsed;
                txtLaneQty.Visibility = Visibility.Collapsed;
                txtProdVerCode.Visibility = Visibility.Collapsed;
                cboTopBackTiltle.Visibility = Visibility.Collapsed;
                cboTopBack.Visibility = Visibility.Collapsed;
                cboElecTypeTiltle.Visibility = Visibility.Collapsed;
                cboElecType.Visibility = Visibility.Collapsed;

                tbWipWrkTypeCode.Visibility = Visibility.Collapsed;
                txtWipWrkTypeCOde.Visibility = Visibility.Collapsed;

                cboElecType.Visibility = Visibility.Collapsed;
                cTabDefectTag.Visibility = Visibility.Collapsed;
                cTabImage.Visibility = Visibility.Collapsed;
                cTabEqptDefect.Visibility = Visibility.Collapsed;
                cTabColor.Visibility = Visibility.Collapsed;
                cTabTrayTime.Visibility = Visibility.Collapsed;

                #region ################ �ּ� #########################
                //dgLotList.Columns["EQPTID_COATER"].Visibility = Visibility.Collapsed;
                //dgLotList.Columns["EQPT_END_PSTN_ID"].Visibility = Visibility.Collapsed;
                //dgLotList.Columns["INPUT_DIFF_QTY"].Visibility = Visibility.Collapsed;
                //dgLotList.Columns["PR_LOTID"].Visibility = Visibility.Collapsed;
                //dgLotList.Columns["WIPQTY2_ED"].Visibility = Visibility.Collapsed;
                //dgLotList.Columns["CNFM_LOSS_QTY2"].Visibility = Visibility.Collapsed;
                //dgLotList.Columns["CNFM_DFCT_QTY2"].Visibility = Visibility.Collapsed;
                //dgLotList.Columns["CNFM_PRDT_REQ_QTY2"].Visibility = Visibility.Collapsed;
                //dgLotList.Columns["PROD_VER_CODE"].Visibility = Visibility.Collapsed;
                //dgLotList.Columns["EQPT_END_QTY"].Visibility = Visibility.Collapsed;
                //dgLotList.Columns["SRS1_QTY"].Visibility = Visibility.Collapsed;
                //dgLotList.Columns["SRS2_QTY"].Visibility = Visibility.Collapsed;
                //dgLotList.Columns["ROLLPRESS_COUNT"].Visibility = Visibility.Collapsed;
                //dgLotList.Columns["LENGTH_EXCEED"].Visibility = Visibility.Collapsed;
                //dgLotList.Columns["LENGTH_EXCEED2"].Visibility = Visibility.Collapsed;
                //dgLotList.Columns["TAG_QTY"].Visibility = Visibility.Collapsed;

                //dgLotList.Columns["EQPT_END_QTY_EA"].Visibility = Visibility.Collapsed;
                //dgLotList.Columns["INPUT_QTY_EA"].Visibility = Visibility.Collapsed;
                //dgLotList.Columns["WIPQTY_ED_EA"].Visibility = Visibility.Collapsed;
                //dgLotList.Columns["CNFM_DFCT_QTY_EA"].Visibility = Visibility.Collapsed;
                //dgLotList.Columns["CNFM_LOSS_QTY_EA"].Visibility = Visibility.Collapsed;
                //dgLotList.Columns["CNFM_PRDT_REQ_QTY_EA"].Visibility = Visibility.Collapsed;
                //dgLotList.Columns["LENGTH_EXCEED_EA"].Visibility = Visibility.Collapsed;
                //dgLotList.Columns["WIPQTY2_ED_EA"].Visibility = Visibility.Collapsed;
                //dgLotList.Columns["CNFM_DFCT_QTY2_EA"].Visibility = Visibility.Collapsed;
                //dgLotList.Columns["CNFM_LOSS_QTY2_EA"].Visibility = Visibility.Collapsed;
                //dgLotList.Columns["CNFM_PRDT_REQ_QTY2_EA"].Visibility = Visibility.Collapsed;
                //dgLotList.Columns["LENGTH_EXCEED2_EA"].Visibility = Visibility.Collapsed;
                #endregion #######################################

                List<string> ColumnsVisibility = new List<string>();
                ColumnsVisibility.Add("EQPTID_COATER");
                ColumnsVisibility.Add("EQPT_END_PSTN_ID");
                ColumnsVisibility.Add("INPUT_DIFF_QTY");
                ColumnsVisibility.Add("PR_LOTID");
                ColumnsVisibility.Add("WIPQTY2_ED");
                ColumnsVisibility.Add("CNFM_LOSS_QTY2");
                ColumnsVisibility.Add("CNFM_DFCT_QTY2");
                ColumnsVisibility.Add("CNFM_PRDT_REQ_QTY2");
                ColumnsVisibility.Add("PROD_VER_CODE");
                ColumnsVisibility.Add("EQPT_END_QTY");
                ColumnsVisibility.Add("SRS1_QTY");
                ColumnsVisibility.Add("SRS2_QTY");
                ColumnsVisibility.Add("COUNTQTY");
                ColumnsVisibility.Add("ROLLPRESS_COUNT");
                ColumnsVisibility.Add("LENGTH_EXCEED");
                ColumnsVisibility.Add("LENGTH_EXCEED2");
                ColumnsVisibility.Add("TAG_QTY");
                ColumnsVisibility.Add("EQP_TAG_QTY");

                ColumnsVisibility.Add("EQPT_END_QTY_EA");
                ColumnsVisibility.Add("INPUT_QTY_EA");
                ColumnsVisibility.Add("WIPQTY_ED_EA");
                ColumnsVisibility.Add("CNFM_DFCT_QTY_EA");
                ColumnsVisibility.Add("CNFM_LOSS_QTY_EA");
                ColumnsVisibility.Add("CNFM_PRDT_REQ_QTY_EA");
                ColumnsVisibility.Add("LENGTH_EXCEED_EA");
                ColumnsVisibility.Add("WIPQTY2_ED_EA");
                ColumnsVisibility.Add("CNFM_DFCT_QTY2_EA");
                ColumnsVisibility.Add("CNFM_LOSS_QTY2_EA");
                ColumnsVisibility.Add("CNFM_PRDT_REQ_QTY2_EA");
                ColumnsVisibility.Add("LENGTH_EXCEED2_EA");
                ColumnsVisibility.Add("FOIL_LOTID");
                ColumnsVisibility.Add("AUTO_STOP_FLAG");

                // ����, ������ ����
                ColumnsVisibility.Add("DIFF_QTY");
                ColumnsVisibility.Add("INPUT_DIFF_QTY_WS");
                ColumnsVisibility.Add("WIPQTY_END_WS");
                ColumnsVisibility.Add("DFCT_QTY_WS");
                ColumnsVisibility.Add("LOSS_QTY_WS");
                ColumnsVisibility.Add("REQ_QTY_WS");
                ColumnsVisibility.Add("BOXQTY_IN");
                ColumnsVisibility.Add("BOXQTY");
                ColumnsVisibility.Add("WINDING_RUNCARD_ID");
                ColumnsVisibility.Add("LOTID_AS");
                ColumnsVisibility.Add("MODL_CHG_FRST_PROD_LOT_FLAG");
                ColumnsVisibility.Add("WIP_WRK_TYPE_CODE_DESC");
                /////////////////////////////////////////////////
                ColumnsVisibility.Add("AFTER_PUNCH_CUTOFF_COUNT");
                ColumnsVisibility.Add("REWINDING_DFCT_TAG_QTY");

                //C20200519-000286
                dgLotList.Columns["HALF_SLIT_SIDE"].Visibility = Visibility.Collapsed;

                // Visibility.Collapsed
                foreach (string str in ColumnsVisibility)
                {
                    if (dgLotList.Columns.Contains(str))
                        dgLotList.Columns[str].Visibility = Visibility.Collapsed;

                    if (dgModelList.Columns.Contains(str))
                        dgModelList.Columns[str].Visibility = Visibility.Collapsed;
                }

                dgDefect.Columns["A_TYPE_DFCT_QTY"].Visibility = Visibility.Collapsed;
                dgDefect.Columns["C_TYPE_DFCT_QTY"].Visibility = Visibility.Collapsed;
                dgDefect.Columns["TAG_QTY"].Visibility = Visibility.Collapsed;
                dgDefect.Columns["DFCT_QTY_DDT_RATE"].Visibility = Visibility.Collapsed;

                tbAType.Visibility = Visibility.Collapsed;
                tbCType.Visibility = Visibility.Collapsed;

                dgSubLot.Columns["PRINT"].Visibility = Visibility.Collapsed;
                dgSubLot.Columns["CSTID"].Visibility = Visibility.Collapsed;
                dgSubLot.Columns["SPECIALYN"].Visibility = Visibility.Collapsed;
                dgSubLot.Columns["SPECIALDESC"].Visibility = Visibility.Collapsed;
                dgSubLot.Columns["FORM_MOVE_STAT_CODE_NAME"].Visibility = Visibility.Collapsed;

                dgInputHist.Columns["CSTID"].Visibility = Visibility.Collapsed;
                //dgInputHist.Columns["SCAN_LOTID"].Visibility = Visibility.Collapsed;

                cTabInputMaterial.Visibility = Visibility.Collapsed;
                cTabSlurry.Visibility = Visibility.Collapsed;
                cTabHalf.Visibility = Visibility.Collapsed;
                cTabWipNote.Visibility = Visibility.Collapsed;

                btnQualityInfo.Visibility = Visibility.Collapsed;

                //if (sProcID.Equals(Process.PACKAGING))
                //    tbInqty.Text = ObjectDic.Instance.GetObjectName("���Է�");
                //else
                //    tbInqty.Text = ObjectDic.Instance.GetObjectName("���귮");

                dgLotList.Columns["INPUT_QTY"].Header = ObjectDic.Instance.GetObjectName("���귮");
                dgLotList.Columns["INPUT_QTY_EA"].Header = ObjectDic.Instance.GetObjectName("���귮");

                dgModelList.Columns["INPUT_QTY"].Header = ObjectDic.Instance.GetObjectName("���귮");
                dgModelList.Columns["INPUT_QTY_EA"].Header = ObjectDic.Instance.GetObjectName("���귮");

                if (sProcID.Equals(Process.PACKAGING))
                {
                    dgLotList.Columns["INPUT_QTY"].Header = ObjectDic.Instance.GetObjectName("���Է�");
                    tbInqty.Text = ObjectDic.Instance.GetObjectName("���Է�");

                    dgModelList.Columns["INPUT_QTY"].Header = ObjectDic.Instance.GetObjectName("���Է�");

                    cTabTrayTime.Visibility = Visibility.Visible;
                }
                else
                {
                    if (((_AREATYPE.Equals("E") && !sProcID.Equals(Process.MIXING)) && (_AREATYPE.Equals("E") && !sProcID.Equals(Process.COATING)))
                         ||
                        (_AREATYPE.Equals("A") && sProcID.Equals(Process.NOTCHING)))
                    {
                        dgLotList.Columns["INPUT_QTY"].Header = ObjectDic.Instance.GetObjectName("���귮(����)");
                        dgLotList.Columns["INPUT_QTY_EA"].Header = ObjectDic.Instance.GetObjectName("���귮(����)");
                        tbInqty.Text = ObjectDic.Instance.GetObjectName("���귮(����)");

                        dgModelList.Columns["INPUT_QTY"].Header = ObjectDic.Instance.GetObjectName("���귮(����)");
                        dgModelList.Columns["INPUT_QTY_EA"].Header = ObjectDic.Instance.GetObjectName("���귮(����)");
                    }
                    else
                    {
                        tbInqty.Text = ObjectDic.Instance.GetObjectName("���귮");
                    }
                }


                if (!_AREATYPE.Equals("A"))
                {
                    chkPtnLen.IsEnabled = true;
                    dgLotList.Columns["EQPT_END_QTY"].Visibility = Visibility.Visible;
                    dgLotList.Columns["JUDG_NAME"].Visibility = Visibility.Visible;
                }
                else
                {
                    chkPtnLen.IsEnabled = false;
                }

                if (_AREATYPE.Equals("A") && !sProcID.Equals(Process.NOTCHING)) //����ǰ �� �����ֱ�
                {
                    cTabHalf.Visibility = Visibility.Visible;
                    dgInputHist.Columns["CSTID"].Visibility = Visibility.Visible;
                    dgSubLot.Columns["CSTID"].Visibility = Visibility.Visible;

                    dgLotList.Columns["WIPQTY_ED"].Header = ObjectDic.Instance.GetObjectName("��ǰ��");
                    dgLotList.Columns["CNFM_DFCT_QTY"].Header = ObjectDic.Instance.GetObjectName("�ҷ���");
                    dgLotList.Columns["CNFM_LOSS_QTY"].Header = ObjectDic.Instance.GetObjectName("LOSS��");
                    dgLotList.Columns["CNFM_PRDT_REQ_QTY"].Header = ObjectDic.Instance.GetObjectName("��ǰû��");

                    dgModelList.Columns["WIPQTY_ED"].Header = ObjectDic.Instance.GetObjectName("��ǰ��");
                    dgModelList.Columns["CNFM_DFCT_QTY"].Header = ObjectDic.Instance.GetObjectName("�ҷ���");
                    dgModelList.Columns["CNFM_LOSS_QTY"].Header = ObjectDic.Instance.GetObjectName("LOSS��");
                    dgModelList.Columns["CNFM_PRDT_REQ_QTY"].Header = ObjectDic.Instance.GetObjectName("��ǰû��");
                }
                else
                {
                    dgLotList.Columns["PR_LOTID"].Visibility = Visibility.Visible;
                    dgLotList.Columns["WIPQTY2_ED"].Visibility = Visibility.Visible;
                    dgLotList.Columns["CNFM_LOSS_QTY2"].Visibility = Visibility.Visible;
                    dgLotList.Columns["CNFM_DFCT_QTY2"].Visibility = Visibility.Visible;
                    dgLotList.Columns["CNFM_PRDT_REQ_QTY2"].Visibility = Visibility.Visible;
                    dgLotList.Columns["PROD_VER_CODE"].Visibility = Visibility.Visible;

                    dgModelList.Columns["WIPQTY2_ED"].Visibility = Visibility.Visible;
                    dgModelList.Columns["CNFM_LOSS_QTY2"].Visibility = Visibility.Visible;
                    dgModelList.Columns["CNFM_DFCT_QTY2"].Visibility = Visibility.Visible;
                    dgModelList.Columns["CNFM_PRDT_REQ_QTY2"].Visibility = Visibility.Visible;

                    dgLotList.Columns["WIPQTY_ED"].Header = ObjectDic.Instance.GetObjectName("��ǰ��(Roll)");
                    dgLotList.Columns["WIPQTY2_ED"].Header = ObjectDic.Instance.GetObjectName("��ǰ��(Lane)");
                    dgLotList.Columns["CNFM_DFCT_QTY"].Header = ObjectDic.Instance.GetObjectName("�ҷ���(Roll)");
                    dgLotList.Columns["CNFM_DFCT_QTY2"].Header = ObjectDic.Instance.GetObjectName("�ҷ���(Lane)");
                    dgLotList.Columns["CNFM_LOSS_QTY"].Header = ObjectDic.Instance.GetObjectName("LOSS��(Roll)");
                    dgLotList.Columns["CNFM_LOSS_QTY2"].Header = ObjectDic.Instance.GetObjectName("LOSS��(Lane)");
                    dgLotList.Columns["CNFM_PRDT_REQ_QTY"].Header = ObjectDic.Instance.GetObjectName("��ǰû��(Roll)");
                    dgLotList.Columns["CNFM_PRDT_REQ_QTY2"].Header = ObjectDic.Instance.GetObjectName("��ǰû��(Lane)");

                    dgLotList.Columns["WIPQTY_ED_EA"].Header = ObjectDic.Instance.GetObjectName("��ǰ��(Roll)");
                    dgLotList.Columns["WIPQTY2_ED_EA"].Header = ObjectDic.Instance.GetObjectName("��ǰ��(Lane)");
                    dgLotList.Columns["CNFM_DFCT_QTY_EA"].Header = ObjectDic.Instance.GetObjectName("�ҷ���(Roll)");
                    dgLotList.Columns["CNFM_DFCT_QTY2_EA"].Header = ObjectDic.Instance.GetObjectName("�ҷ���(Lane)");
                    dgLotList.Columns["CNFM_LOSS_QTY_EA"].Header = ObjectDic.Instance.GetObjectName("LOSS��(Roll)");
                    dgLotList.Columns["CNFM_LOSS_QTY2_EA"].Header = ObjectDic.Instance.GetObjectName("LOSS��(Lane)");
                    dgLotList.Columns["CNFM_PRDT_REQ_QTY_EA"].Header = ObjectDic.Instance.GetObjectName("��ǰû��(Roll)");
                    dgLotList.Columns["CNFM_PRDT_REQ_QTY2_EA"].Header = ObjectDic.Instance.GetObjectName("��ǰû��(Lane)");

                    dgModelList.Columns["WIPQTY_ED"].Header = ObjectDic.Instance.GetObjectName("��ǰ��(Roll)");
                    dgModelList.Columns["WIPQTY2_ED"].Header = ObjectDic.Instance.GetObjectName("��ǰ��(Lane)");
                    dgModelList.Columns["CNFM_DFCT_QTY"].Header = ObjectDic.Instance.GetObjectName("�ҷ���(Roll)");
                    dgModelList.Columns["CNFM_DFCT_QTY2"].Header = ObjectDic.Instance.GetObjectName("�ҷ���(Lane)");
                    dgModelList.Columns["CNFM_LOSS_QTY"].Header = ObjectDic.Instance.GetObjectName("LOSS��(Roll)");
                    dgModelList.Columns["CNFM_LOSS_QTY2"].Header = ObjectDic.Instance.GetObjectName("LOSS��(Lane)");
                    dgModelList.Columns["CNFM_PRDT_REQ_QTY"].Header = ObjectDic.Instance.GetObjectName("��ǰû��(Roll)");
                    dgModelList.Columns["CNFM_PRDT_REQ_QTY2"].Header = ObjectDic.Instance.GetObjectName("��ǰû��(Lane)");

                    dgModelList.Columns["WIPQTY_ED_EA"].Header = ObjectDic.Instance.GetObjectName("��ǰ��(Roll)");
                    dgModelList.Columns["WIPQTY2_ED_EA"].Header = ObjectDic.Instance.GetObjectName("��ǰ��(Lane)");
                    dgModelList.Columns["CNFM_DFCT_QTY_EA"].Header = ObjectDic.Instance.GetObjectName("�ҷ���(Roll)");
                    dgModelList.Columns["CNFM_DFCT_QTY2_EA"].Header = ObjectDic.Instance.GetObjectName("�ҷ���(Lane)");
                    dgModelList.Columns["CNFM_LOSS_QTY_EA"].Header = ObjectDic.Instance.GetObjectName("LOSS��(Roll)");
                    dgModelList.Columns["CNFM_LOSS_QTY2_EA"].Header = ObjectDic.Instance.GetObjectName("LOSS��(Lane)");
                    dgModelList.Columns["CNFM_PRDT_REQ_QTY_EA"].Header = ObjectDic.Instance.GetObjectName("��ǰû��(Roll)");
                    dgModelList.Columns["CNFM_PRDT_REQ_QTY2_EA"].Header = ObjectDic.Instance.GetObjectName("��ǰû��(Lane)");

                }

                dgLotList.Columns["MOLD_ID"].Visibility = Visibility.Collapsed;
                dgLotList.Columns["STD_TOOL_ID"].Visibility = Visibility.Collapsed; // 2023.12.08 ������ E20231211-000182 ǥ�ذ���ID ����ó��
                dgLotList.Columns["MOLD_USE_COUNT"].Visibility = Visibility.Collapsed;

                if (sProcID.Equals(Process.NOTCHING))
                {
                    cTabHalf.Visibility = Visibility.Collapsed;
                    dgLotList.Columns["EQPT_END_PSTN_ID"].Visibility = Visibility.Visible;

                    dgLotList.Columns["MOLD_ID"].Visibility = Visibility.Visible;
                    dgLotList.Columns["STD_TOOL_ID"].Visibility = Visibility.Visible; // 2023.12.08 ������ E20231211-000182 ǥ�ذ���ID ǥ��ó��
                    dgLotList.Columns["MOLD_USE_COUNT"].Visibility = Visibility.Visible;
                }

                if (sProcID.Equals(Process.LAMINATION))
                {
                    dgSubLot.Columns["PRINT"].Visibility = Visibility.Visible;
                    #region
                    dgSubLot.Columns["CSTID"].Visibility = Visibility.Visible;
                    dgSubLot.Columns["CSTID"].Header = ObjectDic.Instance.GetObjectName("ī��ƮID");
                    #endregion
                }

                /// [C20190415_74474] x-ray ����� ��ư �߰�
                if (sProcID.Equals(Process.XRAY_REWORK))
                {
                    dgSubLot.Columns["PRINT"].Visibility = Visibility.Visible;
                }

                if (sProcID.Equals(Process.STACKING_FOLDING))
                {
                    #region
                    dgSubLot.Columns["CSTID"].Visibility = Visibility.Visible;
                    dgSubLot.Columns["CSTID"].Header = ObjectDic.Instance.GetObjectName("ī��ƮID");
                    #endregion
                    dgLotList.Columns["WIP_WRK_TYPE_CODE_DESC"].Visibility = Visibility.Visible;
                    txtWipWrkTypeCOde.Visibility = Visibility.Visible;
                    tbWipWrkTypeCode.Visibility = Visibility.Visible;
                }

                if (sProcID.Equals(Process.PACKAGING))
                {
                    btnQualityInfo.Visibility = Visibility.Visible;

                    dgLotList.Columns["INPUT_DIFF_QTY"].Visibility = Visibility.Visible;
                    dgLotList.Columns["WIP_WRK_TYPE_CODE_DESC"].Visibility = Visibility.Visible;

                    dgModelList.Columns["INPUT_DIFF_QTY"].Visibility = Visibility.Visible;

                    dgSubLot.Columns["SPECIALYN"].Visibility = Visibility.Visible;
                    dgSubLot.Columns["SPECIALDESC"].Visibility = Visibility.Visible;
                    dgSubLot.Columns["FORM_MOVE_STAT_CODE_NAME"].Visibility = Visibility.Visible;

                    dgSubLot.Columns["LOTID"].Visibility = Visibility.Collapsed;
                    dgSubLot.Columns["PRINT_YN"].Visibility = Visibility.Collapsed;
                    dgSubLot.Columns["DISPATCH_YN"].Visibility = Visibility.Collapsed;

                    tbInputDiffQty.Visibility = Visibility.Visible;
                    txtInputDiffQty.Visibility = Visibility.Visible;

                    cTabQualityInfo.Visibility = Visibility.Collapsed;
                }
                else
                {
                    dgSubLot.Columns["LOTID"].Visibility = Visibility.Visible;
                    dgSubLot.Columns["PRINT_YN"].Visibility = Visibility.Visible;
                    dgSubLot.Columns["DISPATCH_YN"].Visibility = Visibility.Visible;
                    cTabQualityInfo.Visibility = Visibility.Visible;
                    #region
                    dgSubLot.Columns["CSTID"].Header = ObjectDic.Instance.GetObjectName("Tray ID");
                    #endregion
                }

                // C20211114-000011, Rewinding Process ���ػ���Site
                //if (sProcID.Equals(Process.ROLL_PRESSING) || sProcID.Equals(Process.SLITTING) || sProcID.Equals(Process.SLIT_REWINDING))
                //{
                //    dgLotList.Columns["EQPTID_COATER"].Visibility = Visibility.Visible;
                //}

                if (sProcID.Equals(Process.ROLL_PRESSING))
                {
                   
                    dgLotList.Columns["ROLLPRESS_COUNT"].Visibility = Visibility.Visible;
                    dgLotList.Columns["AUTO_STOP_FLAG"].Visibility = Visibility.Visible;    // Auto Stop �߰� [2018-09-19]
                    dgLotList.Columns["EQPTID_COATER"].Visibility = Visibility.Visible; // C20211114-000011, Rewinding Process ���ػ���Site
                    dgLotList.Columns["AFTER_PUNCH_CUTOFF_COUNT"].Visibility = Visibility.Visible; // CSR : C20220117-000174 [2022.06.23]

                    cTabColor.Visibility = Visibility.Visible;

                    if (bTagView)
                    {
                        dgLotList.Columns["TAG_QTY"].Visibility = Visibility.Visible;
                        dgDefect.Columns["TAG_QTY"].Visibility = Visibility.Visible;

                        // ������ ������ ��츸 ����
                        if (LoginInfo.CFG_SHOP_ID == "G482")
                            dgLotList.Columns["EQP_TAG_QTY"].Visibility = Visibility.Visible;
                    }

                }

                if (sProcID.Equals(Process.SLITTING))
                {
                    dgLotList.Columns["TAG_QTY"].Visibility = Visibility.Visible;
                    dgLotList.Columns["EQPTID_COATER"].Visibility = Visibility.Visible; // C20211114-000011, Rewinding Process ���ػ���Site
                    dgLotList.Columns["AFTER_PUNCH_CUTOFF_COUNT"].Visibility = Visibility.Visible; // CSR : C20220117-000174 [2022.06.23]

                    dgDefect.Columns["TAG_QTY"].Visibility = Visibility.Visible;

                    // ������ ������ ��츸 ����
                    if (LoginInfo.CFG_SHOP_ID == "G482")
                       dgLotList.Columns["EQP_TAG_QTY"].Visibility = Visibility.Visible;
                }

                if (sProcID.Equals(Process.SLIT_REWINDING))
                {
                    dgLotList.Columns["EQPTID_COATER"].Visibility = Visibility.Visible; // C20211114-000011, Rewinding Process ���ػ���Site
                    dgLotList.Columns["REWINDING_DFCT_TAG_QTY"].Visibility = Visibility.Visible;  // CSR : C20220117-000174 [2022.06.23]
                }

                if (sProcID.Equals(Process.MIXING))
                {
                    cTabInputMaterial.Visibility = Visibility.Visible;
                }

                // Vision Image�� Sliiter ���������� �������� �߰� ( 2017-01-05 )
                if (sProcID.Equals(Process.COATING))
                {
                    dgLotList.Columns["FOIL_LOTID"].Visibility = Visibility.Visible;

                    cTabInputMaterial.Visibility = Visibility.Visible;
                    //cTabSlurry.Visibility = Visibility.Visible;

                    tbLaneQty.Visibility = Visibility.Visible;
                    tbProdVerCode.Visibility = Visibility.Visible;
                    txtLaneQty.Visibility = Visibility.Visible;
                    txtProdVerCode.Visibility = Visibility.Visible;

                    //dgInputHist.Columns["SCAN_LOTID"].Visibility = Visibility.Visible;

                    //C20200519-000286
                    if (AreaViewCheck("HS_SIDE_VIEW_AREA", LoginInfo.CFG_AREA_ID))
                        dgLotList.Columns["HALF_SLIT_SIDE"].Visibility = Visibility.Visible;

                }

                if (sProcID.Equals(Process.COATING) || sProcID.Equals(Process.INS_COATING) || sProcID.Equals(Process.INS_SLIT_COATING))
                {
                    cboTopBackTiltle.Visibility = Visibility.Visible;
                    cboTopBack.Visibility = Visibility.Visible;

                    //dgInputHist.Columns["SCAN_LOTID"].Visibility = Visibility.Visible;
                }

                if (sProcID.Equals(Process.REWINDER) || sProcID.Equals(Process.LASER_ABLATION) || sProcID.Equals(Process.BACK_WINDER))
                    cTabDefectTag.Visibility = Visibility.Visible;

                if (!string.IsNullOrEmpty(sProcID) && !sProcID.Substring(0, 1).Equals("E"))
                    cTabEqptDefect.Visibility = Visibility.Visible;

                if (sProcID.Equals(Process.SRS_MIXING) ||
                    sProcID.Equals(Process.SRS_COATING) ||
                    sProcID.Equals(Process.SRS_SLITTING) ||
                    sProcID.Equals(Process.SRS_BOXING))
                {
                    dgLotList.Columns["SRS1_QTY"].Visibility = Visibility.Visible;
                    dgLotList.Columns["SRS2_QTY"].Visibility = Visibility.Visible;
                }


                // CWA�ܼ�Ƚ�� ������û���� �߰� [2019-04-22]
                if (sProcID.Equals(Process.COATING) ||
                     sProcID.Equals(Process.ROLL_PRESSING) ||
                     sProcID.Equals(Process.HALF_SLITTING) ||
                     sProcID.Equals(Process.SLITTING))
                {
                    if (LoginInfo.CFG_SHOP_ID.Equals("A041") || LoginInfo.CFG_SHOP_ID.Equals("A011"))
                    {
                        dgLotList.Columns["COUNTQTY2"].Visibility = Visibility.Visible;
                        dgLotList.Columns["COUNTQTY"].Visibility = Visibility.Collapsed;
                    }
                    else
                    {
                        dgLotList.Columns["COUNTQTY"].Visibility = Visibility.Visible;
                        dgLotList.Columns["COUNTQTY2"].Visibility = Visibility.Collapsed;
                    }
                }

                // �����ʰ� Į���� ���ؿ��� MIXING���� �������� NOTCHING��
                if (((_AREATYPE.Equals("E") && !sProcID.Equals(Process.MIXING)) && (_AREATYPE.Equals("E") && !sProcID.Equals(Process.COATING)))
                 ||
                (_AREATYPE.Equals("A") && sProcID.Equals(Process.NOTCHING)))
                {
                    tbLengthExceedQty.Text = ObjectDic.Instance.GetObjectName("�����ʰ�");
                    tbLengthExceedQty.Visibility = Visibility.Visible;
                    txtLengthExceedQty.Visibility = Visibility.Visible;

                    dgLotList.Columns["LENGTH_EXCEED"].Visibility = Visibility.Visible;
                    dgLotList.Columns["LENGTH_EXCEED2"].Visibility = Visibility.Visible;

                    dgModelList.Columns["LENGTH_EXCEED"].Visibility = Visibility.Visible;
                    dgModelList.Columns["LENGTH_EXCEED2"].Visibility = Visibility.Visible;
                }

                if (_AREATYPE.Equals("E"))
                {
                    cboElecTypeTiltle.Visibility = Visibility.Visible;
                    cboElecType.Visibility = Visibility.Visible;
                }

                // ����, �ʼ��� ���� ����ǰ �� 
                if (sProcID.Equals(Process.WINDING) || sProcID.Equals(Process.ASSEMBLY) || sProcID.Equals(Process.WASHING))
                {
                    cTabEqptDefect.Visibility = Visibility.Visible;

                    if (sProcID.Equals(Process.WINDING))
                    {
                        dgLotList.Columns["WINDING_RUNCARD_ID"].Visibility = Visibility.Visible;
                        dgLotList.Columns["LOTID_AS"].Visibility = Visibility.Visible;
                        dgLotList.Columns["MODL_CHG_FRST_PROD_LOT_FLAG"].Visibility = Visibility.Visible;

                        cTabInputHalfProduct.Visibility = Visibility.Collapsed;
                    }
                    else
                    {
                        cTabInputHalfProduct.Visibility = Visibility.Visible;
                        dgInHalfProduct.Columns["WN_LOTID"].Visibility = Visibility.Collapsed;

                        if (sProcID.Equals(Process.ASSEMBLY))
                        {
                            cTabHalf.Visibility = Visibility.Collapsed;
                            dgInHalfProduct.Columns["WN_LOTID"].Visibility = Visibility.Visible;

                            dgLotList.Columns["INPUT_QTY"].Header = ObjectDic.Instance.GetObjectName("���Է�");
                            dgModelList.Columns["INPUT_QTY"].Header = ObjectDic.Instance.GetObjectName("���Է�");
                            tbInqty.Text = ObjectDic.Instance.GetObjectName("���Է�");

                            tbInputDiffQty.Visibility = Visibility.Visible;
                            txtInputDiffQty.Visibility = Visibility.Visible;

                            tbLengthExceedQty.Text = ObjectDic.Instance.GetObjectName("������");
                            tbLengthExceedQty.Visibility = Visibility.Visible;
                            txtLengthExceedQty.Visibility = Visibility.Visible;


                            dgLotList.Columns["DIFF_QTY"].Visibility = Visibility.Visible;
                            dgLotList.Columns["INPUT_DIFF_QTY_WS"].Visibility = Visibility.Visible;
                            dgLotList.Columns["WIPQTY_END_WS"].Visibility = Visibility.Visible;
                            dgLotList.Columns["DFCT_QTY_WS"].Visibility = Visibility.Visible;
                            dgLotList.Columns["LOSS_QTY_WS"].Visibility = Visibility.Visible;
                            dgLotList.Columns["REQ_QTY_WS"].Visibility = Visibility.Visible;
                            dgLotList.Columns["BOXQTY"].Visibility = Visibility.Visible;

                            dgModelList.Columns["DIFF_QTY"].Visibility = Visibility.Visible;
                            dgModelList.Columns["INPUT_DIFF_QTY_WS"].Visibility = Visibility.Visible;
                            dgModelList.Columns["WIPQTY_END_WS"].Visibility = Visibility.Visible;
                            dgModelList.Columns["DFCT_QTY_WS"].Visibility = Visibility.Visible;
                            dgModelList.Columns["LOSS_QTY_WS"].Visibility = Visibility.Visible;
                            dgModelList.Columns["REQ_QTY_WS"].Visibility = Visibility.Visible;
                            dgModelList.Columns["BOXQTY"].Visibility = Visibility.Visible;
                        }
                        else
                        {
                            dgLotList.Columns["INPUT_QTY"].Header = ObjectDic.Instance.GetObjectName("���귮");
                            dgModelList.Columns["INPUT_QTY"].Header = ObjectDic.Instance.GetObjectName("���귮");
                            tbInqty.Text = ObjectDic.Instance.GetObjectName("���귮");

                            dgLotList.Columns["BOXQTY"].Visibility = Visibility.Visible;
                            dgLotList.Columns["BOXQTY_IN"].Visibility = Visibility.Visible;

                            dgModelList.Columns["BOXQTY"].Visibility = Visibility.Visible;
                            dgModelList.Columns["BOXQTY_IN"].Visibility = Visibility.Visible;
                        }

                    }
                }
                else
                {
                    cTabEqptDefect.Visibility = Visibility.Visible;
                    cTabInputHalfProduct.Visibility = Visibility.Collapsed;
                }

                if (sProcID.Equals(Process.NOTCHING_REWINDING))
                {
                    cTabEqptDefect.Visibility = Visibility.Collapsed;
                    cTabHalf.Visibility = Visibility.Collapsed;
                    cTabQualityInfo.Visibility = Visibility.Collapsed;
                    cTabInputHalf.Visibility = Visibility.Collapsed;
                }
                else
                {
                    cTabInputHalf.Visibility = Visibility.Visible;
                }

                // CSR : C20210720-000108 
                if (IsAreaCommonCodeUse("MNG_ROLL_DIR_AREA", sProcID))
                {
                    dgLotList.Columns["INPUT_SECTION_ROLL_DIRCTN"].Visibility = Visibility.Visible;
                    dgLotList.Columns["EM_SECTION_ROLL_DIRCTN"].Visibility = Visibility.Visible;
                }
                else
                {
                    dgLotList.Columns["INPUT_SECTION_ROLL_DIRCTN"].Visibility = Visibility.Collapsed;
                    dgLotList.Columns["EM_SECTION_ROLL_DIRCTN"].Visibility = Visibility.Collapsed;
                }

                // C20210928-000539 / ����ε� NG TAG�� Į�� �߰�
                if (IsAreaCommonCodeUse("REMARK_NG_TAG_COL_USE", sProcID))
                {
                    dgLotList.Columns["TAG_QTY"].Visibility = Visibility.Visible;
                    dgDefect.Columns["TAG_QTY"].Visibility = Visibility.Visible;
                }

                string sBizname = string.Empty;
                string[] sEqptAttr = { LoginInfo.CFG_AREA_ID, sProcID };
                switch (sProcID)
                {
                    case "E2000":
                        sBizname = "COATER_QLTY_EQPT_VISIBLE";
                        break;
                    case "E4000":
                        sBizname = "DEFECT_COLUMN_VISIBILITY";
                        break;
                    default:
                        break;
                }

                if (!string.IsNullOrEmpty(sBizname) && IsCommoncodeAttrUse(sBizname, null, sEqptAttr))
                {
                    // C20220308-000577 - ���� ���ְ˻� ���� [2022-04-27]
                    if (string.Equals(sProcID, Process.COATING))
                        dgQualityInfo.Columns["EQPT_VALUE"].Visibility = Visibility.Visible;
                    else
                        dgQualityInfo.Columns["EQPT_VALUE"].Visibility = Visibility.Collapsed;

                    // CSR : C20220406-000241
                    if (string.Equals(sProcID, Process.SLITTING))
                        dgEqpFaulty.Columns["QLTY_DFCT_QTY"].Visibility = Visibility.Visible;
                    else
                        dgEqpFaulty.Columns["QLTY_DFCT_QTY"].Visibility = Visibility.Collapsed;
                }
                else
                {
                    dgQualityInfo.Columns["EQPT_VALUE"].Visibility = Visibility.Collapsed;
                    dgEqpFaulty.Columns["QLTY_DFCT_QTY"].Visibility = Visibility.Collapsed;
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private bool IsCommoncodeAttrUse(string sCodeType, string sCmCode, string[] sAttribute)
        {
            try
            {
                string[] sColumnArr = { "ATTRIBUTE1", "ATTRIBUTE2", "ATTRIBUTE3", "ATTRIBUTE4", "ATTRIBUTE5" };

                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("CMCDTYPE", typeof(string));
                RQSTDT.Columns.Add("CMCODE", typeof(string));
                for (int i = 0; i < sColumnArr.Length; i++)
                    RQSTDT.Columns.Add(sColumnArr[i], typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["CMCDTYPE"] = sCodeType;
                dr["CMCODE"] = (sCmCode == string.Empty ? null : sCmCode);
                for (int i = 0; i < sAttribute.Length; i++)
                    dr[sColumnArr[i]] = string.IsNullOrEmpty(sAttribute[i]) ? (object)DBNull.Value : sAttribute[i];
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_COMMONCODE_TBL_BY_ATTRIBUTES", "RQSTDT", "RSLTDT", RQSTDT);

                if (dtResult != null && dtResult.Rows.Count > 0)
                    return true;

                return false;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return false;
            }
        }

        private bool RollPressTagCountViewCheck()
        {
            bool bFlag = false;

            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("CMCDTYPE", typeof(String));
                RQSTDT.Columns.Add("CMCODE", typeof(String));

                DataRow dr = RQSTDT.NewRow();
                dr["CMCDTYPE"] = "ROLLPRESS_TAG_VIEW_AREA";
                dr["CMCODE"] = LoginInfo.CFG_AREA_ID;
                RQSTDT.Rows.Add(dr);

                DataTable SearchResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_COMMONCODE_TBL", "RQSTDT", "RSLTDT", RQSTDT);

                if (SearchResult.Rows.Count > 0)
                    bFlag = true;

                return bFlag;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return false;
            }
        }

        private bool AreaViewCheck(string sCMCDTYPE, string sAREAID)
        {
            bool bFlag = false;

            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("CMCDTYPE", typeof(String));
                RQSTDT.Columns.Add("CMCODE", typeof(String));

                DataRow dr = RQSTDT.NewRow();
                dr["CMCDTYPE"] = sCMCDTYPE;
                dr["CMCODE"] = sAREAID;
                RQSTDT.Rows.Add(dr);

                DataTable SearchResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_COMMONCODE_TBL", "RQSTDT", "RSLTDT", RQSTDT);

                if (SearchResult.Rows.Count > 0)
                    bFlag = true;

                return bFlag;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return false;
            }
        }

        private void ProcCheck(string sProcID, string sEqptID)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(sProcID) || sProcID.Equals("SELECT"))
                    return;

                if (cboProcess.SelectedValue.Equals(Process.STACKING_FOLDING))
                {
                    DataTable RQSTDT = new DataTable();
                    RQSTDT.TableName = "RQSTDT";
                    RQSTDT.Columns.Add("EQPTID", typeof(String));

                    DataRow dr = RQSTDT.NewRow();
                    dr["EQPTID"] = sEqptID;
                    RQSTDT.Rows.Add(dr);

                    DataTable SearchResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_EQGRID", "RQSTDT", "RSLTDT", RQSTDT);

                    if (SearchResult.Rows[0]["EQGRID"].ToString().Equals("FOL"))
                        _StackingYN = "N";
                    else
                        _StackingYN = "Y";

                    dgSubLot.Columns["PRINT"].Visibility = Visibility.Visible;
                    grdMBomTypeCnt.Visibility = Visibility.Visible;

                    if (dgDefect != null && dgDefect.Columns.Contains("A_TYPE_DFCT_QTY"))
                        dgDefect.Columns["A_TYPE_DFCT_QTY"].Visibility = Visibility.Visible;
                    if (dgDefect != null && dgDefect.Columns.Contains("C_TYPE_DFCT_QTY"))
                        dgDefect.Columns["C_TYPE_DFCT_QTY"].Visibility = Visibility.Visible;
                    if (dgDefect != null && dgDefect.Columns.Contains("DFCT_QTY_DDT_RATE"))
                        dgDefect.Columns["DFCT_QTY_DDT_RATE"].Visibility = Visibility.Visible;

                    tbAType.Visibility = Visibility.Visible;
                    tbCType.Visibility = Visibility.Visible;

                    if (_StackingYN.Equals("Y"))
                    {
                        tbAType.Text = "HALFTYPE";
                        tbCType.Text = "MONOTYPE";

                        if (dgDefect != null && dgDefect.Columns.Contains("A_TYPE_DFCT_QTY"))
                            dgDefect.Columns["A_TYPE_DFCT_QTY"].Header = "HALFTYPE";
                        if (dgDefect != null && dgDefect.Columns.Contains("C_TYPE_DFCT_QTY"))
                            dgDefect.Columns["C_TYPE_DFCT_QTY"].Header = "MONOTYPE";
                    }
                    else
                    {
                        tbAType.Text = "ATYPE";
                        tbCType.Text = "CTYPE";

                        if (dgDefect != null && dgDefect.Columns.Contains("A_TYPE_DFCT_QTY"))
                            dgDefect.Columns["A_TYPE_DFCT_QTY"].Header = "ATYPE";
                        if (dgDefect != null && dgDefect.Columns.Contains("C_TYPE_DFCT_QTY"))
                            dgDefect.Columns["C_TYPE_DFCT_QTY"].Header = "CTYPE";
                        if (dgDefect != null && dgDefect.Columns.Contains("DFCT_QTY_DDT_RATE"))
                            dgDefect.Columns["DFCT_QTY_DDT_RATE"].Header = ObjectDic.Instance.GetObjectName("����");
                    }

                    GetMBOMInfo();
                }
                else
                {
                    grdMBomTypeCnt.Visibility = Visibility.Collapsed;

                    if (dgDefect != null && dgDefect.Columns.Contains("A_TYPE_DFCT_QTY"))
                        dgDefect.Columns["A_TYPE_DFCT_QTY"].Visibility = Visibility.Collapsed;
                    if (dgDefect != null && dgDefect.Columns.Contains("C_TYPE_DFCT_QTY"))
                        dgDefect.Columns["C_TYPE_DFCT_QTY"].Visibility = Visibility.Collapsed;
                    if (dgDefect != null && dgDefect.Columns.Contains("DFCT_QTY_DDT_RATE"))
                        dgDefect.Columns["DFCT_QTY_DDT_RATE"].Header = ObjectDic.Instance.GetObjectName("����");

                    tbAType.Visibility = Visibility.Collapsed;
                    tbCType.Visibility = Visibility.Collapsed;

                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        #endregion

        #region ������ ���� Format
        private void SetProcessNumFormat(string sProcid)
        {
            // ���ڰ� Format 
            string sFormat = string.Empty;

            if (sProcid.Equals(Process.MIXING))
            {
                // MIXING
                sFormat = "###,##0.###";
            }
            else if (_AREATYPE.Equals("A"))
            {
                // ����
                sFormat = "###,##0";
            }
            else
            {
                // ����
                if (_UNITCODE.Equals("EA"))
                    sFormat = "###,##0.##";
                else
                    sFormat = "###,##0.#";
            }

            ((C1.WPF.DataGrid.DataGridBoundColumn)(dgLotList.Columns["EQPT_END_QTY"])).Format = sFormat;
            ((C1.WPF.DataGrid.DataGridBoundColumn)(dgLotList.Columns["INPUT_QTY"])).Format = sFormat;
            ((C1.WPF.DataGrid.DataGridBoundColumn)(dgLotList.Columns["WIPQTY_ED"])).Format = sFormat;
            ((C1.WPF.DataGrid.DataGridBoundColumn)(dgLotList.Columns["CNFM_DFCT_QTY"])).Format = sFormat;
            ((C1.WPF.DataGrid.DataGridBoundColumn)(dgLotList.Columns["CNFM_LOSS_QTY"])).Format = sFormat;
            ((C1.WPF.DataGrid.DataGridBoundColumn)(dgLotList.Columns["CNFM_PRDT_REQ_QTY"])).Format = sFormat;
            ((C1.WPF.DataGrid.DataGridBoundColumn)(dgLotList.Columns["WIPQTY2_ED"])).Format = sFormat;
            ((C1.WPF.DataGrid.DataGridBoundColumn)(dgLotList.Columns["CNFM_DFCT_QTY2"])).Format = sFormat;
            ((C1.WPF.DataGrid.DataGridBoundColumn)(dgLotList.Columns["CNFM_LOSS_QTY2"])).Format = sFormat;
            ((C1.WPF.DataGrid.DataGridBoundColumn)(dgLotList.Columns["CNFM_PRDT_REQ_QTY2"])).Format = sFormat;
            ((C1.WPF.DataGrid.DataGridBoundColumn)(dgLotList.Columns["INPUT_DIFF_QTY"])).Format = sFormat;
            ((C1.WPF.DataGrid.DataGridBoundColumn)(dgLotList.Columns["LENGTH_EXCEED"])).Format = sFormat;
            ((C1.WPF.DataGrid.DataGridBoundColumn)(dgLotList.Columns["LENGTH_EXCEED2"])).Format = sFormat;

            ((C1.WPF.DataGrid.DataGridBoundColumn)(dgDefect.Columns["A_TYPE_DFCT_QTY"])).Format = sFormat;
            ((C1.WPF.DataGrid.DataGridBoundColumn)(dgDefect.Columns["C_TYPE_DFCT_QTY"])).Format = sFormat;
            ((C1.WPF.DataGrid.DataGridBoundColumn)(dgDefect.Columns["RESNQTY"])).Format = sFormat;

            ((C1.WPF.DataGrid.DataGridBoundColumn)(dgEqpFaulty.Columns["DFCT_QTY"])).Format = sFormat;

            ((C1.WPF.DataGrid.DataGridBoundColumn)(dgLotList.Columns["EQPT_END_QTY_EA"])).Format = sFormat;
            ((C1.WPF.DataGrid.DataGridBoundColumn)(dgLotList.Columns["INPUT_QTY_EA"])).Format = sFormat;
            ((C1.WPF.DataGrid.DataGridBoundColumn)(dgLotList.Columns["WIPQTY_ED_EA"])).Format = sFormat;
            ((C1.WPF.DataGrid.DataGridBoundColumn)(dgLotList.Columns["CNFM_DFCT_QTY_EA"])).Format = sFormat;
            ((C1.WPF.DataGrid.DataGridBoundColumn)(dgLotList.Columns["CNFM_LOSS_QTY_EA"])).Format = sFormat;
            ((C1.WPF.DataGrid.DataGridBoundColumn)(dgLotList.Columns["CNFM_PRDT_REQ_QTY_EA"])).Format = sFormat;
            ((C1.WPF.DataGrid.DataGridBoundColumn)(dgLotList.Columns["LENGTH_EXCEED_EA"])).Format = sFormat;
            ((C1.WPF.DataGrid.DataGridBoundColumn)(dgLotList.Columns["WIPQTY2_ED_EA"])).Format = sFormat;
            ((C1.WPF.DataGrid.DataGridBoundColumn)(dgLotList.Columns["CNFM_DFCT_QTY2_EA"])).Format = sFormat;
            ((C1.WPF.DataGrid.DataGridBoundColumn)(dgLotList.Columns["CNFM_LOSS_QTY2_EA"])).Format = sFormat;
            ((C1.WPF.DataGrid.DataGridBoundColumn)(dgLotList.Columns["CNFM_PRDT_REQ_QTY2_EA"])).Format = sFormat;
            ((C1.WPF.DataGrid.DataGridBoundColumn)(dgLotList.Columns["LENGTH_EXCEED2_EA"])).Format = sFormat;

            txtInputQty.Format = sFormat;
            txtOutQty.Format = sFormat;
            txtDefectQty.Format = sFormat;
            txtLossQty.Format = sFormat;
            txtPrdtReqQty.Format = sFormat;
            txtInputDiffQty.Format = sFormat;
            txtLengthExceedQty.Format = sFormat;

            ((C1.WPF.DataGrid.DataGridBoundColumn)(dgModelList.Columns["INPUT_QTY"])).Format = sFormat;
            ((C1.WPF.DataGrid.DataGridBoundColumn)(dgModelList.Columns["WIPQTY_ED"])).Format = sFormat;
            ((C1.WPF.DataGrid.DataGridBoundColumn)(dgModelList.Columns["CNFM_DFCT_QTY"])).Format = sFormat;
            ((C1.WPF.DataGrid.DataGridBoundColumn)(dgModelList.Columns["CNFM_LOSS_QTY"])).Format = sFormat;
            ((C1.WPF.DataGrid.DataGridBoundColumn)(dgModelList.Columns["CNFM_PRDT_REQ_QTY"])).Format = sFormat;
            ((C1.WPF.DataGrid.DataGridBoundColumn)(dgModelList.Columns["WIPQTY2_ED"])).Format = sFormat;
            ((C1.WPF.DataGrid.DataGridBoundColumn)(dgModelList.Columns["CNFM_DFCT_QTY2"])).Format = sFormat;
            ((C1.WPF.DataGrid.DataGridBoundColumn)(dgModelList.Columns["CNFM_LOSS_QTY2"])).Format = sFormat;
            ((C1.WPF.DataGrid.DataGridBoundColumn)(dgModelList.Columns["CNFM_PRDT_REQ_QTY2"])).Format = sFormat;
            ((C1.WPF.DataGrid.DataGridBoundColumn)(dgModelList.Columns["INPUT_DIFF_QTY"])).Format = sFormat;
            ((C1.WPF.DataGrid.DataGridBoundColumn)(dgModelList.Columns["LENGTH_EXCEED"])).Format = sFormat;
            ((C1.WPF.DataGrid.DataGridBoundColumn)(dgModelList.Columns["LENGTH_EXCEED2"])).Format = sFormat;

            ((C1.WPF.DataGrid.DataGridBoundColumn)(dgModelList.Columns["INPUT_QTY_EA"])).Format = sFormat;
            ((C1.WPF.DataGrid.DataGridBoundColumn)(dgModelList.Columns["WIPQTY_ED_EA"])).Format = sFormat;
            ((C1.WPF.DataGrid.DataGridBoundColumn)(dgModelList.Columns["CNFM_DFCT_QTY_EA"])).Format = sFormat;
            ((C1.WPF.DataGrid.DataGridBoundColumn)(dgModelList.Columns["CNFM_LOSS_QTY_EA"])).Format = sFormat;
            ((C1.WPF.DataGrid.DataGridBoundColumn)(dgModelList.Columns["CNFM_PRDT_REQ_QTY_EA"])).Format = sFormat;
            ((C1.WPF.DataGrid.DataGridBoundColumn)(dgModelList.Columns["LENGTH_EXCEED_EA"])).Format = sFormat;
            ((C1.WPF.DataGrid.DataGridBoundColumn)(dgModelList.Columns["WIPQTY2_ED_EA"])).Format = sFormat;
            ((C1.WPF.DataGrid.DataGridBoundColumn)(dgModelList.Columns["CNFM_DFCT_QTY2_EA"])).Format = sFormat;
            ((C1.WPF.DataGrid.DataGridBoundColumn)(dgModelList.Columns["CNFM_LOSS_QTY2_EA"])).Format = sFormat;
            ((C1.WPF.DataGrid.DataGridBoundColumn)(dgModelList.Columns["CNFM_PRDT_REQ_QTY2_EA"])).Format = sFormat;
            ((C1.WPF.DataGrid.DataGridBoundColumn)(dgModelList.Columns["LENGTH_EXCEED2_EA"])).Format = sFormat;

        }
        #endregion

        #region [������]
        private void SetValue(object oContext)
        {
            txtSelectLot.Text = Util.NVC(DataTableConverter.GetValue(oContext, "LOTID")); ;
            txtWorkorder.Text = Util.NVC(DataTableConverter.GetValue(oContext, "WOID"));
            txtWorkorderDetail.Text = Util.NVC(DataTableConverter.GetValue(oContext, "WO_DETL_ID"));
            //txtLotStatus.Text = Util.NVC(DataTableConverter.GetValue(oContext, "WIPSNAME"));
            txtWorkdate.Text = Util.NVC(DataTableConverter.GetValue(oContext, "CALDATE"));
            txtShift.Text = Util.NVC(DataTableConverter.GetValue(oContext, "SHFT_NAME"));
            txtShift.Tag = Util.NVC(DataTableConverter.GetValue(oContext, "SHIFT"));
            txtStartTime.Text = Util.NVC(DataTableConverter.GetValue(oContext, "STARTDTTM"));
            txtWorker.Text = Util.NVC(DataTableConverter.GetValue(oContext, "WRK_USER_NAME"));
            txtEndDttm.Text = Util.NVC(DataTableConverter.GetValue(oContext, "ENDDTTM"));
            txtInputQty.Value = string.IsNullOrEmpty(Util.NVC(DataTableConverter.GetValue(oContext, "INPUT_QTY"))) == true ? 0 : Double.Parse(Util.NVC(DataTableConverter.GetValue(oContext, "INPUT_QTY")));
            txtOutQty.Value = Double.Parse(Util.NVC(DataTableConverter.GetValue(oContext, "WIPQTY_ED")));
            txtDefectQty.Value = Double.Parse(Util.NVC(DataTableConverter.GetValue(oContext, "CNFM_DFCT_QTY")));
            txtLossQty.Value = Double.Parse(Util.NVC(DataTableConverter.GetValue(oContext, "CNFM_LOSS_QTY")));
            txtPrdtReqQty.Value = Double.Parse(Util.NVC(DataTableConverter.GetValue(oContext, "CNFM_PRDT_REQ_QTY")));
            txtWipWrkTypeCOde.Text = Util.NVC(DataTableConverter.GetValue(oContext, "WIP_WRK_TYPE_CODE_DESC"));

            if (Util.NVC(DataTableConverter.GetValue(oContext, "PROCID")).Equals(Process.ASSEMBLY) || Util.NVC(DataTableConverter.GetValue(oContext, "PROCID")).Equals(Process.WASHING))
            {
                txtInputDiffQty.Value = Double.Parse(Util.NVC(DataTableConverter.GetValue(oContext, "DIFF_QTY")));
                txtLengthExceedQty.Value = Double.Parse(Util.NVC(DataTableConverter.GetValue(oContext, "INPUT_DIFF_QTY")));
                txtLaneQty.Text = string.Empty;

                if (string.Equals(DataTableConverter.GetValue(oContext, "PROCID").GetString(), Process.ASSEMBLY))
                {
                    if (!string.IsNullOrEmpty(DataTableConverter.GetValue(oContext, "PROCID").GetString()))
                    {
                        if (GetReInputReasonApplyFlag(DataTableConverter.GetValue(oContext, "EQPTID").GetString()) == "Y")
                        {
                            TabReInput.Visibility = Visibility.Visible;
                            txtInputDiffQty.IsEnabled = false;
                        }
                        else
                        {
                            txtInputDiffQty.IsEnabled = true;
                            TabReInput.Visibility = Visibility.Collapsed;
                        }
                    }
                    else
                    {
                        txtInputDiffQty.IsEnabled = true;
                        TabReInput.Visibility = Visibility.Collapsed;
                    }
                }
            }
            else
            {
                txtInputDiffQty.Value = Double.Parse(Util.NVC(DataTableConverter.GetValue(oContext, "INPUT_DIFF_QTY")));
                txtLengthExceedQty.Value = Double.Parse(Util.NVC(DataTableConverter.GetValue(oContext, "LENGTH_EXCEED")));
                txtLaneQty.Text = Util.NVC_NUMBER(DataTableConverter.GetValue(oContext, "LANE_QTY"));
            }

            txtProdVerCode.Text = Util.NVC(DataTableConverter.GetValue(oContext, "PROD_VER_CODE"));

            //txtNote.Text = Util.NVC(DataTableConverter.GetValue(oContext, "WIP_NOTE"));

            _AREAID = Util.NVC(DataTableConverter.GetValue(oContext, "AREAID"));
            _PROCID = Util.NVC(DataTableConverter.GetValue(oContext, "PROCID"));
            _EQSGID = Util.NVC(DataTableConverter.GetValue(oContext, "EQSGID"));
            _EQPTID = Util.NVC(DataTableConverter.GetValue(oContext, "EQPTID"));
            _LOTID = Util.NVC(DataTableConverter.GetValue(oContext, "LOTID"));
            _WIPSEQ = Util.NVC(DataTableConverter.GetValue(oContext, "WIPSEQ"));
            _LANEPTNQTY = Util.NVC(DataTableConverter.GetValue(oContext, "LANE_PTN_QTY"));
            ////_ORGQTY = Util.NVC(DataTableConverter.GetValue(oContext, "ORG_QTY"));
            _WIP_NOTE = Util.NVC(DataTableConverter.GetValue(oContext, "WIP_NOTE"));
            _LANE = Util.NVC(DataTableConverter.GetValue(oContext, "LANE_QTY"));
            _EQPTNAME = Util.NVC(DataTableConverter.GetValue(oContext, "EQPTNAME"));

            txtNote.Text = GetWipNote();

            //txtWorkorder.FontWeight = FontWeights.Normal;
            //txtLotStatus.FontWeight = FontWeights.Normal;
            //txtWorkdate.FontWeight = FontWeights.Normal;
            //txtShift.FontWeight = FontWeights.Normal;
            //txtStartTime.FontWeight = FontWeights.Normal;
            //txtWorker.FontWeight = FontWeights.Normal;
            //ldpDatePicker.FontWeight = FontWeights.Normal;
            //txtOutQty.FontWeight = FontWeights.Normal;
            //txtDefectQty.FontWeight = FontWeights.Normal;
            //txtLossQty.FontWeight = FontWeights.Normal;
            //txtPrdtReqQty.FontWeight = FontWeights.Normal;
            //txtNote.FontWeight = FontWeights.Normal;
        }
        #endregion

        #region [VISION IMAGE]
        private void GetVisionImage(string sLotId, string sDate)
        {
            try
            {
                // ���� ���� �������� Biz�߰�����
                if (string.IsNullOrEmpty(sDate))
                    return;

                // TEST��
                string address = "165.244.114.238";
                string userId = "mesmgr";
                string password = "mesmgr@2010";

                string rootPath = string.Format(@"ftp://{0}/", new object[] { address }) + @"test111/";
                rootPath += Convert.ToDateTime(sDate).ToString("yyyyMMdd") + @"/" + sLotId.Substring(0, 9) + "0" + sLotId.Substring(9, 1) + @"/";

                /*
                System.Threading.Tasks.Task.Factory.StartNew(() =>
                {
                    return GetRequestImageList(rootPath, userId, password);
                }).ContinueWith((x) => {
                    Application.Current.Dispatcher.Invoke((Action)(delegate
                    {
                        ImageListView.ItemsSource = new List<ImageSource>(x.Result);
                    }));
                }, System.Threading.Tasks.TaskScheduler.FromCurrentSynchronizationContext());
                */
                ImageListView.ItemsSource = new List<ImageSource>();
                ImageListView.ItemsSource = Util.GetRequestImageList(rootPath, userId, password);
            }
            catch { /* Nothing */ }
        }

        #endregion

        private void DoEvents()
        {
            Application.Current.Dispatcher.Invoke(DispatcherPriority.Background, new System.Threading.ThreadStart(delegate { }));
        }

        private string GetWipNote()
        {
            string sReturn;
            string[] sWipNote = _WIP_NOTE.Split('|');

            if (sWipNote.Length == 0)
            {
                sReturn = _WIP_NOTE;
            }
            else
            {
                sReturn = sWipNote[0];
            }
            return sReturn;
        }

        private string GetReInputReasonApplyFlag(string equipmentCode)
        {
            const string bizRuleName = "DA_PRD_SEL_REINPUT_RSN_APPLY_FLAG";
            DataTable inTable = new DataTable();
            inTable.Columns.Add("EQPTID", typeof(string));
            DataRow dr = inTable.NewRow();
            dr["EQPTID"] = equipmentCode;
            inTable.Rows.Add(dr);
            DataTable dtResult = new ClientProxy().ExecuteServiceSync(bizRuleName, "RQSTDT", "RSLTDT", inTable);

            return CommonVerify.HasTableRow(dtResult) ? dtResult.Rows[0][0].GetString() : string.Empty;
        }

        //������ �׸�(�ҷ��׸�) ���� ��ȸ - ��/������ or ����/������
        private void GetDefectReInputList()
        {
            try
            {
                const string bizRuleName = "BR_PRD_GET_WIPRESONCOLLECT_FOR_REINPUT";
                DataTable inTable = _Biz.GetDA_QCA_SEL_WIPRESONCOLLECT();
                inTable.TableName = "INDATA";

                DataRow newRow = inTable.NewRow();
                newRow["LANGID"] = LoginInfo.LANGID;
                newRow["PROCID"] = _PROCID;
                newRow["LOTID"] = _LOTID;
                newRow["WIPSEQ"] = _WIPSEQ;
                newRow["AREAID"] = LoginInfo.CFG_AREA_ID;
                newRow["EQPTID"] = _EQPTID;
                newRow["ACTID"] = "DEFECT_LOT,LOSS_LOT,CHARGE_PROD_LOT";
                inTable.Rows.Add(newRow);

                DataSet ds = new DataSet();
                ds.Tables.Add(inTable);
                //string xml = ds.GetXml();

                DataSet dsResult = new ClientProxy().ExecuteServiceSync_Multi(bizRuleName, "INDATA", "OUTTYPE,OUTDATA", ds, FrameOperation.MENUID);
                if (CommonVerify.HasTableInDataSet(dsResult))
                {
                    //'AP' ���� / ������
                    //'LP' ���� / ������
                    dgDefectReInput.Columns["CLSS_NAME1"].Visibility = dsResult.Tables["OUTTYPE"].Rows[0]["DFCT_CODE_MNGT_TYPE"].GetString() == "LP" ? Visibility.Visible : Visibility.Collapsed;
                    dgDefectReInput.ItemsSource = DataTableConverter.Convert(dsResult.Tables["OUTDATA"]);
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        private void GetTrayLotByTime()
        {
            try
            {
                DataTable dt = new DataTable();
                dt.Columns.Add("LANGID", typeof(string));
                dt.Columns.Add("PR_LOTID", typeof(string));

                DataRow dr = dt.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["PR_LOTID"] = _LOTID;
                dt.Rows.Add(dr);

                DataTable result = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_EDIT_SUBLOT_QTY_BY_TIME", "INDATA", "OUTDATA", dt);

                Util.GridSetData(dgTrayInfo, result, FrameOperation, true);

            }
            catch (Exception ex)
            {

            }
        }
        
        #region # Roll Map
        private bool IsEquipmentAttr(string sEqptID)
        {
            try
            {
                DataRow[] dr = Util.getEquipmentAttr(sEqptID).Select();
                if (dr?.Length > 0)
                {
                    if (string.Equals(Util.NVC(dr[0]["ROLLMAP_EQPT_FLAG"]), "Y"))
                        return true;
                }
            }
            catch (Exception ex) { }

            return false;
        }

        private bool IsRollMapLotAttribute(string sLotID)
        {
            try
            {               
                DataTable dt = new DataTable();
                dt.Columns.Add("LOTID", typeof(string));

                DataRow row = dt.NewRow();
                row["LOTID"] = sLotID;
                dt.Rows.Add(row);

                DataTable result = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_LOTATTR", "RQSTDT", "RSLTDT", dt);

                if (result != null && result.Rows.Count > 0)
                    if (string.Equals(result.Rows[0]["ROLLMAP_APPLY_FLAG"], "Y"))
                        return true;
            }
            catch (Exception ex) { }

            return false;
        }

        private bool IsRollMapResultApply()
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("PROCID", typeof(string));
                RQSTDT.Columns.Add("EQSGID", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["PROCID"] = _PROCID;
                dr["EQSGID"] = _EQSGID;
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_PROCESSEQUIPMENTSEGMENT", "RQSTDT", "RSLTDT", RQSTDT);

                if (dtResult != null & dtResult.Rows.Count > 0)
                    if (string.Equals(dtResult.Rows[0]["ROLLMAP_SBL_APPLY_FLAG"], "Y"))
                        return true;
            }
            catch (Exception ex) { }

            return false;
        }
        #endregion

        #region
        private bool IsAreaCommonCodeUse(string sCodeType, string sCodeName)
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("AREAID", typeof(string));
                RQSTDT.Columns.Add("COM_TYPE_CODE", typeof(string));
                RQSTDT.Columns.Add("COM_CODE", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["AREAID"] = LoginInfo.CFG_AREA_ID;
                dr["COM_TYPE_CODE"] = sCodeType;
                dr["COM_CODE"] = sCodeName;
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_TB_MMD_AREA_COM_CODE_FOR_AREA", "RQSTDT", "RSLTDT", RQSTDT);

                if (dtResult != null && dtResult.Rows.Count > 0)
                    return true;
            }
            catch (Exception ex) { }

            return false;
        }

        private void GetAreaByCheckData()
        {
            _workCalbuttonAuthYN = string.Empty; //���°���/��ư���� ��뿩��
            _unidentifiedLossYN = string.Empty;  //��Ȯ��LOSS ��뿩��
            _reworkBoxTypeYN = string.Empty;     //���۾� BOX ������ �۾����� REWORK Ȯ��
            _pkgInputCheckYN = string.Empty;     //PKG ���Խ� ���۾� �ϼ� LOT CHECK ��뿩��
            _qmsDefectCodeYN = string.Empty;     //QMS �ҷ��ڵ� ��뿩��

            const string bizRuleName = "DA_BAS_SEL_COMMONCODE_TBL";
            try
            {
                DataTable inTable = new DataTable("RQSTDT");
                inTable.Columns.Add("LANGID", typeof(string));
                inTable.Columns.Add("CMCDTYPE", typeof(string));
                inTable.Columns.Add("CMCODE", typeof(string));

                DataRow dr = inTable.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["CMCDTYPE"] = "CT_INSP_AREA_BY_CHECK";
                dr["CMCODE"] = LoginInfo.CFG_AREA_ID;
                inTable.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync(bizRuleName, "RQSTDT", "RSLTDT", inTable);

                if (CommonVerify.HasTableRow(dtResult))
                {
                    _workCalbuttonAuthYN = dtResult.Rows[0]["ATTRIBUTE1"].GetString();
                    _unidentifiedLossYN = dtResult.Rows[0]["ATTRIBUTE2"].GetString();
                    _reworkBoxTypeYN = dtResult.Rows[0]["ATTRIBUTE3"].GetString();
                    _pkgInputCheckYN = dtResult.Rows[0]["ATTRIBUTE4"].GetString();
                    _qmsDefectCodeYN = dtResult.Rows[0]["ATTRIBUTE5"].GetString();
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        #endregion

        #endregion

        #endregion

        private void dgLotList_DoubleClick(object sender, MouseButtonEventArgs e)
        {
            try
            {
                C1.WPF.DataGrid.C1DataGrid dg = sender as C1.WPF.DataGrid.C1DataGrid;
                if (Util.NVC(dg.CurrentColumn.Name).Equals("WIPHOLD") && DataTableConverter.GetValue(dg.CurrentRow.DataItem, "WIPHOLD").ToString() == "Y"
                    || Util.NVC(dg.CurrentColumn.Name).Equals("LOTID") && DataTableConverter.GetValue(dg.CurrentRow.DataItem, "WIPHOLD").ToString() == "Y")
                {
                    ShowHoldDetail(Util.NVC(DataTableConverter.GetValue(dg.CurrentRow.DataItem, "LOTID")));
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
            finally
            {
                loadingIndicator.Visibility = Visibility.Collapsed;
            }
        }
        private void ShowHoldDetail(string pLotid)
        {
            FCS002_318_HOLD_DETL wndRunStart = new FCS002_318_HOLD_DETL();
            wndRunStart.FrameOperation = FrameOperation;

            if (wndRunStart != null)
            {
                object[] Parameters = new object[1];
                Parameters[0] = pLotid;
                C1WindowExtension.SetParameters(wndRunStart, Parameters);

                wndRunStart.ShowModal();
            }
        }

        private void btnHist_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Button bt = sender as Button;

                if (bt == null || bt.DataContext == null) return;

                CMM001.Popup.CMM_COM_WIPHIST_HIST wndHist = new CMM001.Popup.CMM_COM_WIPHIST_HIST();
                wndHist.FrameOperation = FrameOperation;

                if (wndHist != null)
                {
                    object[] Parameters = new object[2];

                    Parameters[0] = Util.NVC(DataTableConverter.GetValue(bt.DataContext, "LOTID"));
                    Parameters[1] = Util.NVC(DataTableConverter.GetValue(bt.DataContext, "WIPSEQ"));

                    C1WindowExtension.SetParameters(wndHist, Parameters);

                    wndHist.Closed += new EventHandler(wndHist_Closed);

                    this.Dispatcher.BeginInvoke(new Action(() => wndHist.ShowModal()));
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void wndHist_Closed(object sender, EventArgs e)
        {
            CMM001.Popup.CMM_COM_WIPHIST_HIST window = sender as CMM001.Popup.CMM_COM_WIPHIST_HIST;
            if (window.DialogResult == MessageBoxResult.OK)
            {

            }
        }
    }
}