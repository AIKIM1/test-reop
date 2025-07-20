/*************************************************************************************
 Created Date : 2016.06.16
      Creator : 
   Decription : 
--------------------------------------------------------------------------------------
 [Change History]
  2016.06.16  DEVELOPER : Initial Created.





 
**************************************************************************************/

using System;
using System.Collections.Generic;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using C1.WPF;
using C1.WPF.DataGrid;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;

namespace LGC.GMES.MES.ProtoType04
{
    public partial class PGM_GUI_016 : UserControl, IWorkArea
    {
        DataTable dtMain = new DataTable();

        #region Declaration & Constructor 

        private string _LOTID = string.Empty;
        private string _INPUTLOT = string.Empty;
        private string _WIPSTAT = string.Empty;
        private string _WIPQTY = string.Empty;
        private string _EQIPQTY = string.Empty;
        private string _FINALCUT = string.Empty;
        private string _WOID = string.Empty;
        private string _PRODID = string.Empty;
        private string _PRODNAME = string.Empty;
        private string _WIPDTTM_ST = string.Empty;
        private string _WIPDTTM_ED = string.Empty;
        private string _INPUTQTY = string.Empty;
        private string _VERSION = string.Empty;
        private string _GOODQTY = string.Empty;
        private string _LOSSQTY = string.Empty;
        private string _combo = string.Empty;
        private string _WORKORDER = string.Empty;




        Util _Util = new Util();

        private LGC.GMES.MES.CMM001.UserControls.UC_WORKORDER winWorkOrder = new LGC.GMES.MES.CMM001.UserControls.UC_WORKORDER();

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            SetWorkOrderWindow();
            ApplyPermissions();
        }

        public IFrameOperation FrameOperation
        {
            get;
            set;
        }

        public PGM_GUI_016()
        {
            InitializeComponent();
            initCombo();
        }

        private void initCombo()
        {
            String[] sFilter = { "E2" };
            CommonCombo _combo = new CommonCombo();

            C1ComboBox[] cboEquipmentChild = { cboEquipment };
            _combo.SetCombo(cboEquipmentSegment, CommonCombo.ComboStatus.NA, cbChild: cboEquipmentChild, sFilter: sFilter);

            C1ComboBox[] cboEquipmentParent = { cboEquipmentSegment };
            String[] sFilter2 = { Process.TAPING };
            _combo.SetCombo(cboEquipment, CommonCombo.ComboStatus.NA, cbParent: cboEquipmentParent, sFilter: sFilter2);
        }

        public bool ClearControls()
        {
            bool bRet = false;

            try
            {
                Util.gridClear(dgProductLot);
                Util.gridClear(dgMaterial);

                bRet = true;
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(ex.Message, null, "Error", MessageBoxButton.OK, ControlsLibrary.MessageBoxIcon.Warning);
                bRet = false;
            }
            return bRet;
        }








        #endregion

        #region Initialize

        #endregion

        #region Event


        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            if (cboEquipmentSegment.SelectedIndex < 0 || cboEquipmentSegment.SelectedValue.ToString().Trim().Length < 1)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("라인을 선택 하세요."), null, "Info", MessageBoxButton.OK, MessageBoxIcon.Warning);
                return;
            }

            if (cboEquipment.SelectedIndex < 0 || cboEquipment.SelectedValue.ToString().Trim().Length < 1)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("설비를 선택 하세요."), null, "Info", MessageBoxButton.OK, MessageBoxIcon.Warning);
                return;
            }

            GetWorkOrder();

            GetProductLot(null);
        }

        private void chkProductLot_Checked(object sender, RoutedEventArgs e)
        {
            if (dgProductLot.CurrentRow.DataItem == null)
            {
                return;
            }

            _Util.SetDataGridUncheck(dgProductLot, "CHK", ((C1.WPF.DataGrid.DataGridCellPresenter)((sender as CheckBox).Parent)).Row.Index);
            GetLotInfo(((C1.WPF.DataGrid.DataGridCellPresenter)((sender as CheckBox).Parent)).Row.DataItem);
        }

        private void chkProductLot_UnChecked(object sender, RoutedEventArgs e)
        {
            InitGrid();
            InitTxt();
        }


        private void btnCommit_Click(object sender, RoutedEventArgs e)
        {
            SetDefect(_LOTID);
        }

        private void btnQualitySave_Click(object sender, RoutedEventArgs e)
        {
            SetQuality(_LOTID);
        }

        private void btnInputMaterial_Click(object sender, RoutedEventArgs e)
        {
            SetMaterial(_LOTID);
        }



        private void btnAdd_Click(object sender, RoutedEventArgs e)
        {
            if (dgMaterial.ItemsSource == null || dgMaterial.Rows.Count < 0)
            {
                return;
            }

            DataTable dt = ((DataView)dgMaterial.ItemsSource).Table;

            DataRow dr = dt.NewRow();
            dr["INPUT_DTTM"] = string.Format("{0:yyyy-MM-dd hh:mm}", DateTime.Now);
            dt.Rows.Add(dr);
        }

        private void btnDel_Click(object sender, RoutedEventArgs e)
        {
            if (dgMaterial.ItemsSource == null || dgMaterial.Rows.Count < 0)
            {
                return;
            }

            DataTable dt = ((DataView)dgMaterial.ItemsSource).Table;

            for (int i = 0; i < dgMaterial.Rows.Count; i++)
            {
                if (_Util.GetDataGridCheckValue(dgMaterial, "CHK", i))
                {
                    dt.Rows[i].Delete();
                }
            }
        }


        private void btnRunCancel_Click(object sender, RoutedEventArgs e)
        {
            LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("취소 하시겠습니까?"), null, "Information", MessageBoxButton.OKCancel, MessageBoxIcon.None, (sresult) =>
            {
                if (sresult == MessageBoxResult.OK)
                {
                    DataTable IndataTable = new DataTable();
                    IndataTable.Columns.Add("LOTID", typeof(string));

                    DataRow Indata = IndataTable.NewRow();
                    Indata["LOTID"] = _LOTID;
                    IndataTable.Rows.Add(Indata);

                    string _BizRule = "작업시작취소";

                    new ClientProxy().ExecuteService(_BizRule, "INDATA", "RSLTDT", IndataTable, (result, ex) =>
                    {

                        if (ex != null)
                        {
                            LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex.ToString()), null, "Info", MessageBoxButton.OK, MessageBoxIcon.Warning);
                            return;
                        }
                        LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("정상 처리 되었습니다."), null, "Info", MessageBoxButton.OK, MessageBoxIcon.Warning);
                    });
                }
            });

        }
        //DataGridCellEventArgs
        private void dgDefect_CommittedEdit(object sender, DataGridCellEventArgs e)
        {
            decimal sum = SumDefectQty();
            _LOSSQTY = Util.NVC_DecimalStr(sum);
            _GOODQTY = Util.NVC_DecimalStr(Util.NVC_Decimal(_INPUTQTY) - Util.NVC_Decimal(_LOSSQTY));
            txtGoodQuntity.Text = _GOODQTY;
            txtLossQuntity.Text = _LOSSQTY;
        }



























        #endregion

        #region Mehod




        private void ApplyPermissions()
        {
            List<Button> listAuth = new List<Button>();
            Util.pageAuth(listAuth, FrameOperation.AUTHORITY);
        }


        private void SetWorkOrderWindow()
        {
            if (grdWorkOrder.Children.Count == 0)
            {
                winWorkOrder.FrameOperation = FrameOperation;

                winWorkOrder._UCParent = this;
                grdWorkOrder.Children.Add(winWorkOrder);
            }
        }





        private void GetWorkOrder()
        {
            if (winWorkOrder == null)
                return;

            winWorkOrder.EQPTSEGMENT = cboEquipmentSegment.SelectedValue.ToString();
            winWorkOrder.EQPTID = cboEquipment.SelectedValue.ToString();
            winWorkOrder.PROCID = "E1000";//Process.LAMINATION;

            winWorkOrder.GetWorkOrder();
        }








        public void GetProductLot(DataRow drWOInfo = null)
        {
            try
            {
                Util.gridClear(dgProductLot);
                DataTable IndataTable = new DataTable();
                IndataTable.TableName = "INDATA";
                IndataTable.Columns.Add("LANGID", typeof(string));
                IndataTable.Columns.Add("PROCID", typeof(string));
                IndataTable.Columns.Add("WOID", typeof(string));
                IndataTable.Columns.Add("EQPTID", typeof(string));

                DataRow Indata = IndataTable.NewRow();
                Indata["LANGID"] = LoginInfo.LANGID;
                Indata["PROCID"] = "E1000"; //Process.TAPING;
                Indata["WOID"] = "1K16810CT001"; // drWOInfo[0].ToString();
                Indata["EQPTID"] = "EQP000002";
                IndataTable.Rows.Add(Indata);


                dtMain = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_TAPPING_LOTINFO", "INDATA", "RSLTDT", IndataTable);

                dgProductLot.ItemsSource = DataTableConverter.Convert(dtMain);

            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }



        private void InitGrid()
        {
            dgLotInfo.ItemsSource = null;
            dgDefect.ItemsSource = null;
            dgQuality.ItemsSource = null;
            dgMaterial.ItemsSource = null;
        }

        private void InitTxt()
        {
            txtWorkOrder.Text = string.Empty;
            txtStatus.Text = string.Empty;
            txtVersion.Text = string.Empty;
            txtChangeRate.Text = string.Empty;
            txtWorkDate.Text = string.Empty;
            txtResultQty.Text = string.Empty;
            txtRemainQty.Text = string.Empty;
            txtLossqty.Text = string.Empty;
            txtOperator.Text = string.Empty;
            txtStartTime.Text = string.Empty;
            txtEndTime.Text = string.Empty;
            txtRunTime.Text = string.Empty;
            txtLotComment.Text = string.Empty;
            txtRemark.Text = string.Empty;
        }
        private void GetLotInfo(Object SelectedItem)
        {
            DataRowView rowview = SelectedItem as DataRowView;

            if (rowview == null)
            {
                return;
            }

            Util.gridClear(dgLotInfo);

            DataTable _dtLotInfo = new DataTable();
            _dtLotInfo.Columns.Add("LOTID", typeof(System.String));
            _dtLotInfo.Columns.Add("INPUTQTY", typeof(System.String));
            _dtLotInfo.Columns.Add("GOODQTY", typeof(System.String));
            _dtLotInfo.Columns.Add("LOSSQTY", typeof(System.String));

            DataRow dRow = _dtLotInfo.NewRow();
            dRow["LOTID"] = rowview["LOTID"].ToString();
            dRow["INPUTQTY"] = rowview["INPUTQTY"].ToString();
            dRow["GOODQTY"] = rowview["GOODQTY"].ToString();
            dRow["LOSSQTY"] = rowview["LOSSQTY"].ToString();
            _dtLotInfo.Rows.Add(dRow);

            dgLotInfo.ItemsSource = DataTableConverter.Convert(_dtLotInfo);

            _LOTID = rowview["LOTID"].ToString();
            _WIPSTAT = rowview["WIPSTAT"].ToString();
            if (_WIPSTAT == "CONFIRM")
            {
                _INPUTQTY = rowview["INPUTQTY"].ToString();
            }
            else
            {
                _INPUTQTY = rowview["GOODQTY"].ToString();
            }


            _INPUTLOT = rowview["INPUTLOT"].ToString();
            _WIPQTY = rowview["WIPQTY"].ToString();
            _EQIPQTY = rowview["EQIPQTY"].ToString();
            _FINALCUT = rowview["FINALCUT"].ToString();
            _WOID = rowview["WOID"].ToString();
            _PRODID = rowview["PRODID"].ToString();
            _PRODNAME = rowview["PRODNAME"].ToString();
            _WIPDTTM_ST = rowview["WIPDTTM_ST"].ToString();
            _WIPDTTM_ED = rowview["WIPDTTM_ED"].ToString();

            if (rowview["TAPING"].ToString() == "")
            {
                _VERSION = "V01";

            }

            _WIPSTAT = "EQPEND";




            if (_WIPSTAT == "EQPEND" || _WIPSTAT == "CONFIRM")
            {

                GetDefect(rowview);
                GetQuality(rowview);
                
                //String[] sFilter3 = { "A1", "E2D01",  "E1000" };
                //_combo.SetCombo(cboShift, CommonCombo.ComboStatus.SELECT, sFilter: sFilter3);
                


            }
            else
            {
                txtVersion.Text = "";
            }

            GetMaterial(rowview);

            txtVersion.Text = rowview["TAPING"].ToString();
            txtWorkOrder.Text = rowview["WOID"].ToString();
            txtStartTime.Text = rowview["WIPDTTM_ST"].ToString();
            txtEndTime.Text = rowview["WIPDTTM_ED"].ToString();
            txtLotComment.Text = rowview["LOT_COMMENT"].ToString();
        }

        private decimal SumDefectQty()
        {
            decimal sum = 0;

            DataRowView rowview = null;

            foreach (C1.WPF.DataGrid.DataGridRow row in dgDefect.Rows)
            {
                rowview = row.DataItem as DataRowView;

                if (!rowview["RESNQTY"].ToString().Equals(""))
                {
                    sum += Util.NVC_Decimal(rowview["RESNQTY"]);
                }
            }
            return sum;
        }

        private void GetDefect(Object SelectedItem)
        {
            try
            {
                DataRowView rowview = SelectedItem as DataRowView;

                if (rowview == null)
                {
                    return;
                }

                Util.gridClear(dgDefect);

                DataTable IndataTable = new DataTable();
                IndataTable.Columns.Add("LANGID", typeof(string));
                IndataTable.Columns.Add("LOTID", typeof(string));

                DataRow Indata = IndataTable.NewRow();
                Indata["LANGID"] = LoginInfo.LANGID;
                Indata["LOTID"] = rowview["LOTID"].ToString();
                IndataTable.Rows.Add(Indata);

                dtMain = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_WIPREASONCOLLECT_ELEC", "INDATA", "RSLTDT", IndataTable);

                dgDefect.ItemsSource = DataTableConverter.Convert(dtMain);

            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }

        private void SetDefect(string LotID)
        {

            if (dgDefect.Rows.Count < 0)
            {
                return;
            }

            DataTable inDataTable = new DataTable();
            inDataTable.Columns.Add("LOTID", typeof(string));
            inDataTable.Columns.Add("PROCID", typeof(string));
            inDataTable.Columns.Add("RESNCODE", typeof(string));
            inDataTable.Columns.Add("RESNQTY", typeof(int));

            DataRow inData = null;
            DataRowView rowview = null;

            foreach (C1.WPF.DataGrid.DataGridRow row in dgDefect.Rows)
            {

                rowview = row.DataItem as DataRowView;

                inData = inDataTable.NewRow();

                inData["LOTID"] = _LOTID;
                inData["PROCID"] = "E1000";//Process.TAPING;
                if (!rowview["RESNQTY"].ToString().Equals(""))
                {
                    inData["RESNCODE"] = rowview["RESNCODE"].ToString();
                    inData["RESNQTY"] = Util.NVC_Decimal(rowview["RESNQTY"].ToString());

                }

                inDataTable.Rows.Add(inData);
            }

            new ClientProxy().ExecuteService("DA_PRD_INS_WIPREASONCOLLECT_ELEC", "INDATA", "RSLTDT", inDataTable, (result, ex) =>
            {
                if (ex != null)
                {
                    LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("예외발생" + ex.ToString()), null, "Info", MessageBoxButton.OK, MessageBoxIcon.Warning);
                    return;
                }

                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("저장완료"), null, "Info", MessageBoxButton.OK, MessageBoxIcon.Warning);

            });
        }




        private void GetQuality(Object SelectedItem)
        {
            try
            {
                DataRowView rowview = SelectedItem as DataRowView;

                if (rowview == null)
                {
                    return;
                }
                Util.gridClear(dgQuality);

                DataTable IndataTable = new DataTable();
                IndataTable.Columns.Add("LANGID", typeof(string));
                IndataTable.Columns.Add("LOTID", typeof(string));

                DataRow Indata = IndataTable.NewRow();
                Indata["LANGID"] = LoginInfo.LANGID;
                Indata["LOTID"] = rowview["LOTID"].ToString();
                IndataTable.Rows.Add(Indata);

                dtMain = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_TAPING_WIPDATACOLLECT_ELEC", "INDATA", "RSLTDT", IndataTable);

                dgQuality.ItemsSource = DataTableConverter.Convert(dtMain);

            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }

        private void SetQuality(string LotID)
        {
            if (dgQuality.Rows.Count < 0)
            {
                return;
            }
            DataTable inDataTable = new DataTable();
            inDataTable.Columns.Add("LOTID", typeof(string));
            inDataTable.Columns.Add("PROCID", typeof(string));
            inDataTable.Columns.Add("EQPTID", typeof(string));
            inDataTable.Columns.Add("CLCTITEM", typeof(string));
            inDataTable.Columns.Add("CLCTVAL", typeof(string));

            DataRow inData = null;
            DataRowView rowview = null;

            foreach (C1.WPF.DataGrid.DataGridRow row in dgQuality.Rows)
            {

                rowview = row.DataItem as DataRowView;

                inData = inDataTable.NewRow();

                inData["LOTID"] = _LOTID;
                if (!rowview["CLCTVAL"].ToString().Equals(""))
                {
                    inData["CLCTITEM"] = rowview["CLCTITEM"].ToString();
                    inData["CLCTVAL"] = rowview["CLCTVAL"];
                }

                inDataTable.Rows.Add(inData);

            }

            new ClientProxy().ExecuteService("DA_PRD_INS_WIPDATACOLLECT_ELEC", "INDATA", "RSLTDT", inDataTable, (result, ex) =>
            {
                if (ex != null)
                {
                    LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("예외발생" + ex.ToString()), null, "Info", MessageBoxButton.OK, MessageBoxIcon.Warning);
                    return;
                }

                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("품질 정보 저장완료"), null, "Info", MessageBoxButton.OK, MessageBoxIcon.Warning);

            });


        }


        private void GetMaterial(Object SelectedItem)
        {
            try
            {
                DataRowView rowview = SelectedItem as DataRowView;

                if (rowview == null)
                {
                    return;
                }

                Util.gridClear(dgMaterial);

                DataTable IndataTable = new DataTable();
                IndataTable.Columns.Add("LANGID", typeof(string));
                IndataTable.Columns.Add("LOTID", typeof(string));

                DataRow Indata = IndataTable.NewRow();
                Indata["LANGID"] = LoginInfo.LANGID;
                Indata["LOTID"] = rowview["LOTID"].ToString();
                IndataTable.Rows.Add(Indata);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_CONSUME_MATERIAL", "INDATA", "RSLTDT", IndataTable);

                dgMaterial.ItemsSource = DataTableConverter.Convert(dtResult);

                SetGridCboItem(dgMaterial.Columns["MTRLID"], _WORKORDER);

            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }

        private void SetMaterial(string LotID)
        {
            if (dgMaterial.Rows.Count < 0)
            {
                return;
            }
            DataTable inDataTable = new DataTable();
            inDataTable.Columns.Add("LOTID", typeof(string));
            inDataTable.Columns.Add("INPUT_LOTID", typeof(string));
            inDataTable.Columns.Add("INPUT_DTTM", typeof(DateTime));
            inDataTable.Columns.Add("MTRLID", typeof(string));
            inDataTable.Columns.Add("INPUT_QTY", typeof(decimal));

            DataRow inData = null;
            DataRowView rowview = null;

            foreach (C1.WPF.DataGrid.DataGridRow row in dgMaterial.Rows)
            {

                rowview = row.DataItem as DataRowView;

                inData = inDataTable.NewRow();

                inData["LOTID"] = LotID;
                inData["INPUT_LOTID"] = Util.NVC(rowview["INPUT_LOTID"]);
                inData["INPUT_DTTM"] = rowview["INPUT_DTTM"];
                inData["MTRLID"] = Util.NVC(rowview["MTRLID"]);
                inData["INPUT_QTY"] = Util.NVC_Decimal(rowview["INPUT_QTY"]);

                inDataTable.Rows.Add(inData);
            }

            new ClientProxy().ExecuteService("DA_PRD_INS_WIPMTRL_ELEC", "INDATA", "RSLTDT", inDataTable, (result, ex) =>
            {
                if (ex != null)
                {
                    LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("예외발생" + ex.ToString()), null, "Info", MessageBoxButton.OK, MessageBoxIcon.Warning);
                    return;
                }

                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("투입자재 저장완료"), null, "Info", MessageBoxButton.OK, MessageBoxIcon.Warning);

            });
        }


        private void SetGridCboItem(C1.WPF.DataGrid.DataGridColumn col, string sWOID)
        {
            DataTable IndataTable = new DataTable();
            IndataTable.Columns.Add("LANGID", typeof(string));
            IndataTable.Columns.Add("WOID", typeof(string));

            DataRow Indata = IndataTable.NewRow();
            Indata["LANGID"] = LoginInfo.LANGID;
            Indata["WOID"] = sWOID;
            IndataTable.Rows.Add(Indata);

            dtMain = new ClientProxy().ExecuteServiceSync("DA_COM_SEL_TB_SFC_WO_MTRL", "INDATA", "RSLTDT", IndataTable);

            (col as C1.WPF.DataGrid.DataGridComboBoxColumn).ItemsSource = DataTableConverter.Convert(dtMain);
        }
        #endregion
    }






}

