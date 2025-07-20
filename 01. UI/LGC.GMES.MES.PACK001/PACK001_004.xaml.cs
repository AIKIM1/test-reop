/*************************************************************************************
 Created Date : 2016.08.01
      Creator : Jeong Hyeon Sik
   Decription : Pack ���۾� ����ȭ��
--------------------------------------------------------------------------------------
 [Change History]
  2016.08.01  Jeong Hyeon Sik : Initial Created.
  2020.03.19 �տ켮 CSR ID 30636 GMES ��ǰ ��� ó���� ���� ���Ͷ� ��� ���� [��û��ȣ] C20200212-000273
**************************************************************************************/

using System;
using System.Collections.Generic;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using C1.WPF;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;

namespace LGC.GMES.MES.PACK001
{
    public partial class PACK001_004 : UserControl, IWorkArea
    {
        #region Declaration & Constructor 
        LGC.GMES.MES.PACK001.Class.MESSAGE_PARAM ms = new LGC.GMES.MES.PACK001.Class.MESSAGE_PARAM();
        string now_labelcode = string.Empty;

        public IFrameOperation FrameOperation
        {
            get;
            set;
        }

        public PACK001_004()
        {
            InitializeComponent();
        }

        /// <summary>
        /// ����ũ���� �ڵ�
        /// </summary>
        private string sSelectedProcID = string.Empty;
        /// <summary>
        /// ���԰��� ��������
        /// </summary>
        private DataTable dtReturnProcess = null;

        private DataSet dsResult = new DataSet();

        
        #endregion

        #region Initialize

        #endregion

        #region Event

        #region Event - UserControl
        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            List<Button> listAuth = new List<Button>();
            listAuth.Add(btnReworkConfirm);
            listAuth.Add(btnReworkInput);
            listAuth.Add(btnWipLabel);
            Util.pageAuth(listAuth, FrameOperation.AUTHORITY);

            setReworkResultQty();

            dtpDateFrom.SelectedDateTime = DateTime.Now.AddDays(-7);
            dtpDateTo.SelectedDateTime = DateTime.Now;

            CommonCombo _combo = new CommonCombo();

            String[] sFilter3 = { "REWORK_JUDGE" };
            _combo.SetCombo(cboReworkResult, CommonCombo.ComboStatus.NONE, sFilter: sFilter3, sCase: "COMMCODE");

            String[] sFilterArea = { LoginInfo.CFG_SHOP_ID, Area_Type.PACK };
            C1ComboBox[] cboAreaChild = { cboEquipmentSegment };
            _combo.SetCombo(cboArea, CommonCombo.ComboStatus.NONE, sCase: "AREA_AREATYPE", cbChild: cboAreaChild ,sFilter: sFilterArea);
            
            C1ComboBox[] cboEquipmentSegmentParent = { cboArea };
            _combo.SetCombo(cboEquipmentSegment, CommonCombo.ComboStatus.NONE, cbParent: cboEquipmentSegmentParent);

            String[] sFilter = { "RESP_DEPT" };
            _combo.SetCombo(cboReworkIMPUTE_CODE, CommonCombo.ComboStatus.NONE, sFilter: sFilter, sCase: "COMMCODE");

            this.Loaded -= new System.Windows.RoutedEventHandler(this.UserControl_Loaded);
            tbWipListCount.Text = "[ 0 " + ObjectDic.Instance.GetObjectName("��") + " ]";
        }
        private void UserControl_KeyUp(object sender, KeyEventArgs e)
        {
            try
            {
                switch (e.Key)
                {
                    case Key.F4:
                        txtReworkLotInput.Focus();
                        txtReworkLotInput.SelectAll();
                        break;
                    default:
                        break;
                }

            }
            catch { }
        }

        #endregion

        #region Event - TextBox
        private void txtReworkLotInput_KeyDown(object sender, KeyEventArgs e)
        {
            try
            {
                if (e.Key == Key.Enter)
                {

                    ReworkLotStart(txtReworkLotInput.Text);
                }
            }
            catch (Exception ex)
            {
                Util.Alert(ex.ToString());
            }
        }

        #endregion

        #region Event - Button
        private void btnWipLabel_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (dgWipList.CurrentRow != null)
                {
                    string sSelectLotid = Util.NVC(DataTableConverter.GetValue(dgWipList.CurrentRow.DataItem, "LOTID"));
                    string sPRODID = Util.NVC(DataTableConverter.GetValue(dgWipList.CurrentRow.DataItem, "PRODID"));
                    string sLabelCode = Util.NVC(cboLabelCode.SelectedValue);
                    now_labelcode = sLabelCode;

                    if (sLabelCode.Length > 0)
                    {
                        Util.printLabel_Pack(FrameOperation, loadingIndicator, sSelectLotid, LABEL_TYPE_CODE.PACK, sLabelCode, "N", "1", sPRODID);
                    }
                    else
                    {
                        Util.printLabel_Pack(FrameOperation, loadingIndicator, sSelectLotid, LABEL_TYPE_CODE.PACK, "N", "1", null);
                    }
                }
            }
            catch (Exception ex)
            {
                //Util.Alert(ex.ToString());
                Util.MessageException(ex);
            }
        }

        private void btnWipSearch_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                setWipList();
            }
            catch (Exception ex)
            {
                Util.Alert(ex.ToString());
            }
        }

        private void btnReworkInput_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                ReworkLotStart(txtReworkLotInput.Text);
            }
            catch (Exception ex)
            {
                Util.Alert(ex.ToString());
            }
        }

        private void btnReworkConfirm_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (ConfirmValidation())
                {
                    LGC.GMES.MES.ControlsLibrary.MessageBox.Show(ms.AlertRetun("SFU1294", cboReworkResult.Text), null, "Information", MessageBoxButton.OKCancel, MessageBoxIcon.None, (sResult) =>
                    //{0} ���� �Ͻðڽ��ϱ� ?
                    {
                        if (sResult == MessageBoxResult.OK)
                        {                        
                            ReworkLotEnd();                        
                        }

                    });
                }
            }
            catch (Exception ex)
            {
                Util.Alert(ex.ToString());
            }
        }

        #endregion

        #region Event - comboBox
        private void cboReworkResult_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            try
            {
                

                if (Util.NVC(cboReworkResult.SelectedValue) == "REWORK")
                {
                    cboReworkReturnProcess.SelectedValue = null;
                    cboReworkReturnProcess.ItemsSource = null;
                    setReturnProcess();


                    if (dsResult != null)
                    {
                        if (dsResult.Tables.IndexOf("ACTIVITYREASON") > -1)
                        {
                            dgDefect.ItemsSource = DataTableConverter.Convert(dsResult.Tables["ACTIVITYREASON"]);
                        }
                    }
                }
                else
                {
                    cboReworkReturnProcess.SelectedValue = null;
                    cboReworkReturnProcess.ItemsSource = null;
                    setScrapProcess();
                    
                    Util.gridClear(dgDefect);
                }
            }
            catch (Exception ex)
            {
                Util.Alert(ex.ToString());
            }
        }

        private void cboEquipmentSegment_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            try
            {
                DataTable dtResult = getSelectedProcid(Process_Type.REPAIR);
                if (dtResult.Rows.Count > 0)
                {
                    sSelectedProcID = Util.NVC(dtResult.Rows[0]["PROCID"]);
                }

                setReworkResultQty();
            }
            catch (Exception ex)
            {
                Util.Alert(ex.ToString());
            }
        }

        #endregion

        #region Event - DataGrid

        private void dgWipList_LoadedCellPresenter(object sender, C1.WPF.DataGrid.DataGridCellEventArgs e)
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
                    if (e.Cell.Column.Name == "LOTID")
                    {
                        //e.Cell.Presenter.Foreground = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#72166B"));
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
                Util.Alert(ex.ToString());
            }
        }

        private void dgWipList_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            try
            {
                Point pnt = e.GetPosition(null);
                C1.WPF.DataGrid.DataGridCell cell = dgWipList.GetCellFromPoint(pnt);

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
                Util.Alert(ex.ToString());
            }
        }

        private void dgWipList_MouseUp(object sender, MouseButtonEventArgs e)
        {
            try
            {
                Point pnt = e.GetPosition(null);
                C1.WPF.DataGrid.DataGridCell cell = dgWipList.GetCellFromPoint(pnt);

                if (cell != null)
                {
                    string sPRDT_CLSS_CODE = Util.NVC(DataTableConverter.GetValue(dgWipList.Rows[cell.Row.Index].DataItem, "PRDT_CLSS_CODE"));
                    string sPRODID = Util.NVC(DataTableConverter.GetValue(dgWipList.Rows[cell.Row.Index].DataItem, "PRODID"));

                    if (sPRDT_CLSS_CODE == "CELL")
                    {
                        btnWipLabel.IsEnabled = false;
                        cboLabelCode.IsEnabled = false;
                    }
                    else
                    {
                        btnWipLabel.IsEnabled = true;
                        setLabelCode(sPRODID);
                        cboLabelCode.IsEnabled = true;
                    }
                }
            }
            catch (Exception ex)
            {
                Util.Alert(ex.ToString());
            }
        }

        private void wipListInputButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Button bt = sender as Button;

                C1.WPF.DataGrid.C1DataGrid dg = ((C1.WPF.DataGrid.DataGridCellPresenter)bt.Parent).DataGrid;

                dg.SelectedIndex = ((C1.WPF.DataGrid.DataGridCellPresenter)bt.Parent).Cell.Row.Index;

                string sSelectLotid = Util.NVC(DataTableConverter.GetValue(dgWipList.Rows[dg.SelectedIndex].DataItem, "LOTID"));

                txtReworkLotInput.Text = sSelectLotid;
                //text�� �Է�
                //txtReworkLotid.Text = sSelectLotid;

                //����
                ReworkLotStart(sSelectLotid);
            }
            catch (Exception ex)
            {
                Util.Alert(ex.ToString());
            }
        }

        private void dgDefect_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            try
            {
                Point pnt = e.GetPosition(null);
                C1.WPF.DataGrid.DataGridCell cell = dgDefect.GetCellFromPoint(pnt);
                if (cell != null)
                {
                    DataTableConverter.SetValue(dgDefect.Rows[cell.Row.Index].DataItem, "CHK", true);
                }
            }
            catch (Exception ex)
            {
                Util.Alert(ex.ToString());
            }
        }

        private void dgDefectChoice_Checked(object sender, RoutedEventArgs e)
        {
            try
            {
                TextRange textRange = new TextRange(txtConfirmNote.Document.ContentStart, txtConfirmNote.Document.ContentEnd);
                string sTemp = textRange.Text.Replace("\r\n", "");
                if (!(sTemp.Length > 0))
                {
                    RadioButton rbt = sender as RadioButton;

                    C1.WPF.DataGrid.C1DataGrid dg = ((C1.WPF.DataGrid.DataGridCellPresenter)rbt.Parent).DataGrid;

                    dg.SelectedIndex = ((C1.WPF.DataGrid.DataGridCellPresenter)rbt.Parent).Cell.Row.Index;

                    string sRESNNAME = Util.NVC(DataTableConverter.GetValue(dgDefect.Rows[dg.SelectedIndex].DataItem, "RESNNAME"));

                    txtConfirmNote.AppendText(sRESNNAME);
                }
                
            }
            catch (Exception ex)
            {
                Util.Alert(ex.ToString());
            }
        }

        private void dgInspection_CommittedEdit(object sender, C1.WPF.DataGrid.DataGridCellEventArgs e)
        {
            try
            {
                int iRow = e.Cell.Row.Index;
                Decimal dValue = Convert.ToDecimal(e.Cell.Text);
                Decimal dCLCTLSL = Util.NVC_Decimal(DataTableConverter.GetValue(dgInspection.Rows[iRow].DataItem, "CLCTLSL"));
                Decimal dCLCTUSL = Util.NVC_Decimal(DataTableConverter.GetValue(dgInspection.Rows[iRow].DataItem, "CLCTUSL"));

                if (dValue >= dCLCTLSL &&
                   dValue <= dCLCTUSL)
                {
                    if (e.Cell.Presenter != null)
                    {
                        e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Blue);
                        e.Cell.Presenter.FontWeight = FontWeights.Regular;
                    }
                    DataTableConverter.SetValue(dgInspection.Rows[iRow].DataItem, "PASSYN", "Y");
                }
                else
                {
                    if (e.Cell.Presenter != null)
                    {
                        e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Red);
                        e.Cell.Presenter.FontWeight = FontWeights.Bold;
                    }
                    DataTableConverter.SetValue(dgInspection.Rows[iRow].DataItem, "PASSYN", "N");
                }
            }
            catch (Exception ex)
            {
                Util.Alert(ex.ToString());
            }
        }

        #endregion

        #region - Grid
        private void btnExpandFrameTop_Checked(object sender, RoutedEventArgs e)
        {
            // 20210317 | ��ǽ� | GridSplitter �� ���� ���� GridSplitter ���� error �߻��Ͽ� ������ ó��
            //ContentRight.RowDefinitions[1].Height = new GridLength(8);
            //LGC.GMES.MES.CMM001.GridLengthAnimation gla = new LGC.GMES.MES.CMM001.GridLengthAnimation();
            //gla.From = new GridLength(0, GridUnitType.Star);
            //gla.To = new GridLength(6, GridUnitType.Star);
            //gla.Duration = new TimeSpan(0, 0, 0, 0, 500);
            //ContentRight.RowDefinitions[2].BeginAnimation(RowDefinition.HeightProperty, gla);
        }

        private void btnExpandFrameTop_Unchecked(object sender, RoutedEventArgs e)
        {
            // 20210317 | ��ǽ� | GridSplitter �� ���� ���� GridSplitter ���� error �߻��Ͽ� ������ ó��
            //ContentRight.RowDefinitions[1].Height = new GridLength(0);
            //LGC.GMES.MES.CMM001.GridLengthAnimation gla = new LGC.GMES.MES.CMM001.GridLengthAnimation();
            //gla.From = new GridLength(6, GridUnitType.Star);
            //gla.To = new GridLength(0, GridUnitType.Star);
            //gla.Duration = new TimeSpan(0, 0, 0, 0, 500);
            //ContentRight.RowDefinitions[2].BeginAnimation(RowDefinition.HeightProperty, gla);
        }
        #endregion

        #endregion

        #region Mehod

        /// <summary>
        /// ����ũ���� ����
        /// </summary>
        /// <param name="sLotid"></param>
        private void ReworkLotStart(string sLotid)
        {
            try
            {
                if (!(sLotid.Length > 0))
                {
                    ms.AlertWarning("SFU1366"); //LOT ID�� �Է����ּ���
                    return;
                }
                if (!(sSelectedProcID.Length > 0))
                {                    
                    ms.AlertWarning("SFU1296", Util.GetCondition(cboEquipmentSegment)); //{0}���ο����۾����������������ϴ�.\r\n����������Ȯ���ϼ���.
                    return;
                }

                //BR_PRD_REG_REWORK_START_LOT_PACK

                DataSet dsInput = new DataSet();

                DataTable INDATA = new DataTable();
                INDATA.TableName = "INDATA";
                INDATA.Columns.Add("SRCTYPE", typeof(string));
                INDATA.Columns.Add("LANGID", typeof(string));
                INDATA.Columns.Add("LOTID", typeof(string));                
                INDATA.Columns.Add("PROCID", typeof(string));
                INDATA.Columns.Add("EQPTID", typeof(string));
                INDATA.Columns.Add("USERID", typeof(string));
                INDATA.Columns.Add("PROCTYPE", typeof(string));
                INDATA.Columns.Add("UPDDTTM_FROM", typeof(string));
                INDATA.Columns.Add("UPDDTTM_TO", typeof(string));
                INDATA.Columns.Add("EQSGID", typeof(string));

                DataRow dr = INDATA.NewRow();
                dr["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                dr["LANGID"] = LoginInfo.LANGID;
                dr["LOTID"] = sLotid;
                dr["PROCID"] = sSelectedProcID;
                dr["EQPTID"] = null;
                dr["USERID"] = LoginInfo.USERID;
                dr["PROCTYPE"] = Process_Type.REPAIR;
                dr["UPDDTTM_FROM"] = dtpDateFrom.SelectedDateTime.ToString("yyyyMMdd");
                dr["UPDDTTM_TO"] = dtpDateTo.SelectedDateTime.AddDays(1).ToString("yyyyMMdd");
                dr["EQSGID"] = cboEquipmentSegment.SelectedValue.ToString();
                INDATA.Rows.Add(dr);

                dsInput.Tables.Add(INDATA);

                dsResult = new ClientProxy().ExecuteServiceSync_Multi("BR_PRD_REG_REWORK_START_LOT_PACK2", "INDATA", "WIP_PROC,ACTIVITYREASON,LOT_INFO,OUTDATA,WIPREASONCOLLECT,WO_MTRL", dsInput, null);

                if (dsResult != null && dsResult.Tables.Count > 0)
                {
                    string sStartedLotid = string.Empty;
                    string sStartedProductName = string.Empty;
                    string sStartedProductDesc = string.Empty;

                    if ((dsResult.Tables.IndexOf("LOT_INFO") > -1))
                    {
                        if (dsResult.Tables["LOT_INFO"].Rows.Count > 0)
                        {
                            sStartedLotid = Util.NVC(dsResult.Tables["LOT_INFO"].Rows[0]["LOTID"]);
                            sStartedProductName = Util.NVC(dsResult.Tables["LOT_INFO"].Rows[0]["PRODNAME"]);
                            sStartedProductDesc = Util.NVC(dsResult.Tables["LOT_INFO"].Rows[0]["PRODDESC"]);
                            string sProcID_Cause = Util.NVC(dsResult.Tables["LOT_INFO"].Rows[0]["PROCID_CAUSE"]);
                            string sProcID_Cause_Name = Util.NVC(dsResult.Tables["LOT_INFO"].Rows[0]["PROCID_CAUSE_NAME"]);
                            string sRoutID_Cause = Util.NVC(dsResult.Tables["LOT_INFO"].Rows[0]["ROUTID_CAUSE"]);
                            string sFlowID_Cause = Util.NVC(dsResult.Tables["LOT_INFO"].Rows[0]["FLOWID_CAUSE"]);
                            string sRepair_ResnCode_Cause = Util.NVC(dsResult.Tables["LOT_INFO"].Rows[0]["REPAIR_RESNCODE_CAUSE"]);
                            
                            if (sRepair_ResnCode_Cause.Length > 0)
                            {
                                cboReworkIMPUTE_CODE.SelectedValue = sRepair_ResnCode_Cause;
                            }
                            

                            txtReworkLotid.Text = sStartedLotid;
                            //txtReworkProductid.Text = sStartedProductName;
                            txtReworkProductid.Text = sStartedProductDesc;
                            txtReworkProcCause.Text = sProcID_Cause_Name;
                            dtReturnProcess = getReturnProcess(sProcID_Cause, sRoutID_Cause, sFlowID_Cause);

                            if (Util.NVC(cboReworkResult.SelectedValue) == "REWORK")
                            {
                                cboReworkReturnProcess.SelectedValue = null;
                                cboReworkReturnProcess.ItemsSource = null;
                                setReturnProcess();
                            }
                            else
                            {
                                cboReworkReturnProcess.SelectedValue = null;
                                cboReworkReturnProcess.ItemsSource = null;
                                setScrapProcess();

                                Util.gridClear(dgDefect);
                            }
                        }
                        
                    }
                    if ((dsResult.Tables.IndexOf("WIP_PROC") > -1))
                    {
                        //dgWipList.ItemsSource = DataTableConverter.Convert(dsResult.Tables["WIP_PROC"]);
                        Util.GridSetData(dgWipList, dsResult.Tables["WIP_PROC"], FrameOperation, true);
                        //���ε��� ��Ŀ���̵�
                        Util.gridFindDataRow(ref dgWipList, "LOTID", sStartedLotid, true);

                        Util.SetTextBlockText_DataGridRowCount(tbWipListCount, Util.NVC(dgWipList.Rows.Count));
                    }
                    if ((dsResult.Tables.IndexOf("WIPREASONCOLLECT") > -1))
                    {
                        dgDefectHist.ItemsSource = DataTableConverter.Convert(dsResult.Tables["WIPREASONCOLLECT"]);
                    }
                    if ((dsResult.Tables.IndexOf("ACTIVITYREASON") > -1))
                    {
                        if (Util.NVC(cboReworkResult.SelectedValue) == "REWORK")
                        {
                            dgDefect.ItemsSource = DataTableConverter.Convert(dsResult.Tables["ACTIVITYREASON"]);
                        }
                    }

                    cboArea.IsEnabled = false;
                    cboEquipmentSegment.IsEnabled = false;

                }
            }
            catch (Exception ex)
            {
                //Util.AlertByBiz("BR_PRD_REG_REWORK_START_LOT_PACK", ex.Message, ex.ToString());
                Util.MessageException(ex);
            }
        }

        /// <summary>
        /// ����ũ���� �Ϸ�
        /// </summary>
        private void ReworkLotEnd()
        {
            try
            {
                TextRange textRange = new TextRange(txtConfirmNote.Document.ContentStart, txtConfirmNote.Document.ContentEnd);
                string sNote = textRange.Text;
                

                //resncode ��ǰ�ΰ��� OK
                string sResnCode = string.Empty;
                //if (cboReworkResult.SelectedIndex == 0)
                //{
                //    sResnCode = "OK";
                //}
                //else
                //{
                //    if (dgDefect.Rows.Count > 0)
                //    {
                //        sResnCode = Util.gridFindDataRow_GetValue(ref dgDefect, "CHK", "True", "RESNCODE");
                //    }
                //    else
                //    {
                //        sResnCode = "NG";
                //    }
                //}

                if (dgDefect.Rows.Count > 0)
                {
                    sResnCode = Util.gridFindDataRow_GetValue(ref dgDefect, "CHK", "True", "RESNCODE");
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
                drINDATA["LOTID"] = txtReworkLotid.Text;
                drINDATA["END_PROCID"] = sSelectedProcID;
                drINDATA["END_EQPTID"] = null;
                drINDATA["STRT_PROCID"] = Util.NVC(cboReworkReturnProcess.SelectedValue);
                drINDATA["STRT_EQPTID"] = null;
                drINDATA["USERID"] = LoginInfo.USERID;
                drINDATA["WIPNOTE"] = sNote;
                drINDATA["RESNCODE"] = sResnCode;
                drINDATA["RESNNOTE"] = sNote;
                drINDATA["RESNDESC"] = Util.NVC(cboReworkIMPUTE_CODE.SelectedValue);
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
                if (dsResult.Tables["OUTDATA"] != null)
                {
                    if (dsResult.Tables["OUTDATA"].Rows.Count > 0)
                    {                        
                        ms.AlertInfo("SFU1275"); //����ó�� �Ǿ����ϴ�.

                        Refresh();
                    }
                }

            }
            catch(Exception ex)
            {
                //Util.AlertByBiz("BR_PRD_REG_OUTPUTASSY", ex.Message, ex.ToString());
                Util.MessageException(ex);
            }
        }

        /// <summary>
        /// �Ϸ�ó���� �Է�Ȯ��.
        /// </summary>
        /// <returns></returns>
        private bool ConfirmValidation()
        {
            bool bReturn = true;
            try
            {
                #region �ϰ����lot ���� Ȯ��
                //�ϰ�LOT ���� Ȯ��.
                if (txtReworkLotid.Text == "")
                {                    
                    ms.AlertRetun("SFU1746"); //�Ϸ��� LOT�� �����ϴ�.
                    bReturn = false;
                    txtReworkLotid.Focus();
                    return bReturn;
                }
                #endregion

                #region �����ڵ� ���ÿ���Ȯ��               
                //����ϰ�� VALIDATION
                int iRow = -1;
                if (Util.NVC(cboReworkResult.SelectedValue) == "REWORK")
                {
                    //����ڵ� ���������� �ִ°�쿡�� ���� validation
                    if (dgDefect.Rows.Count > 0)
                    {
                        iRow = Util.gridFindDataRow(ref dgDefect, "CHK", "True", false);

                        if (iRow == -1)
                        {                            
                            ms.AlertWarning("SFU1642"); //���õ� �����ڵ尡 �����ϴ�
                            bReturn = false;
                            return bReturn;
                        }
                    }
                }
                #endregion

                #region NOTE�Է� Ȯ��
                //���� ó���� NOTE �ʼ��Է�
                TextRange textRange = new TextRange(txtConfirmNote.Document.ContentStart, txtConfirmNote.Document.ContentEnd);
                string sNote = textRange.Text.Replace("\r\n", "");
                if (sNote == "")
                {                   
                    ms.AlertWarning("SFU1898"); //��ġ������ �ʼ� �׸� �Դϴ�.
                    txtConfirmNote.Focus();
                    bReturn = false;
                    return bReturn;
                }
                #endregion

                #region �˻�DAta �Է�Ȯ��
                //if (dgInspection.Rows.Count > 0)
                //{
                //    for (int i = 0; i < dgInspection.Rows.Count; i++)
                //    {
                //        string itemVal = Util.NVC(DataTableConverter.GetValue(dgInspection.Rows[i].DataItem, "CLCTVAL"));
                //        if (itemVal == "")
                //        {
                //            LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("�Է��Ͻ� �������� ���� �ʽ��ϴ�. Ȯ�� �ϼ���"), null, "Info", MessageBoxButton.OK, MessageBoxIcon.Warning);
                //            //��Ŀ���̵�
                //            Util.gridSetFocusRow(ref dgInspection, i);
                //            bReturn = false;
                //            return bReturn;
                //        }
                //    }
                //}

                //��ǰ�ϰ�� VALIDATION
                //if (cboReworkResult.SelectedIndex == 0)
                //{
                //    if (GetDefectCount())
                //    {
                //        LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("�Էµ������� �ҷ� ������ �׸��� �ֽ��ϴ�. �����ϰ� ��ǰ ���� �Ͻðڽ��ϱ�?."), null, "Information", MessageBoxButton.OKCancel, MessageBoxIcon.None, (sResult) =>
                //        {
                //            if (sResult != MessageBoxResult.OK)
                //            {
                //                bReturn = false;
                //            }
                //            else
                //            {
                //                bReturn = true;
                //            }
                //        });
                //        return bReturn;
                //    }
                //}
                #endregion

                #region ���԰��� ����Ȯ��
                string sSelectedStartProc = Util.NVC(cboReworkReturnProcess.SelectedValue);
                if (!(sSelectedStartProc.Length > 0))
                {                    
                    ms.AlertWarning("SFU1968"); //���԰����������ϼ���.
                    bReturn = false;
                    return bReturn;
                }
                #endregion
            }
            catch (Exception ex)
            {
                throw ex;
            }

            return bReturn;
        }

        /// <summary>
        /// ������ȸ.
        /// </summary>
        private void setReworkResultQty()
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("EQSGID", typeof(string));
                RQSTDT.Columns.Add("PROCID", typeof(string));
                RQSTDT.Columns.Add("EQPTID", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["EQSGID"] = Util.NVC(cboEquipmentSegment.SelectedValue);
                dr["PROCID"] = sSelectedProcID;
                dr["EQPTID"] = null;
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_PRODPLAN_RESULT_BY_EQSG_REWORK", "RQSTDT", "RSLTDT", RQSTDT);

                if (dtResult.Rows.Count > 0)
                {

                    string sReworkCnt_CELL = Util.NVC(dtResult.Rows[0]["GOODQTY_CELL"]);
                    string sReworkCnt_CMA = Util.NVC(dtResult.Rows[0]["GOODQTY_CMA"]);
                    string sReworkCnt_BMA = Util.NVC(dtResult.Rows[0]["GOODQTY_BMA"]);
                    string sScrapCnt_CELL = Util.NVC(dtResult.Rows[0]["DEFECTQTY_CELL"]);
                    string sScrapCnt_CMA = Util.NVC(dtResult.Rows[0]["DEFECTQTY_CMA"]);
                    string sScrapCnt_BMA = Util.NVC(dtResult.Rows[0]["DEFECTQTY_BMA"]);

                    txtCaldate.Text = Util.NVC(dtResult.Rows[0]["CALDATE_DATE"]);
                    txtShift.Text = Util.NVC(dtResult.Rows[0]["SHFT_NAME"]);
                    txtGoodQty.Text = sReworkCnt_CELL + "/" + sReworkCnt_CMA + "/" + sReworkCnt_BMA;
                    txtDefectQty.Text = sScrapCnt_CELL + "/" + sScrapCnt_CMA + "/" + sScrapCnt_BMA;
                }
            }
            catch (Exception ex)
            {
                //Util.AlertByBiz("DA_PRD_SEL_PRODPLAN_RESULT_BY_EQSG_REWORK", ex.Message, ex.ToString());
                Util.MessageException(ex);
            }
        }

        private void setWipList()
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("PROCID", typeof(string));
                RQSTDT.Columns.Add("EQSGID", typeof(string));
                RQSTDT.Columns.Add("PROCTYPE", typeof(string));
                RQSTDT.Columns.Add("UPDDTTM_FROM", typeof(string));
                RQSTDT.Columns.Add("UPDDTTM_TO", typeof(string));
                //2020.03.19
                RQSTDT.Columns.Add("LOTID", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["PROCID"] = null;
                dr["EQSGID"] = Util.NVC(cboEquipmentSegment.SelectedValue)== "" ?null : Util.NVC(cboEquipmentSegment.SelectedValue);
                dr["PROCTYPE"] = Process_Type.REPAIR;
                dr["UPDDTTM_FROM"] = dtpDateFrom.SelectedDateTime.ToString("yyyyMMdd");
                dr["UPDDTTM_TO"] = dtpDateTo.SelectedDateTime.AddDays(1).ToString("yyyyMMdd");
                //2020.03.19
                dr["LOTID"] = null;
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_WIP_PACK_REWORK", "RQSTDT", "RSLTDT", RQSTDT);

                //dgWipList.ItemsSource = DataTableConverter.Convert(dtResult);
                Util.GridSetData(dgWipList, dtResult, FrameOperation, true);
                Util.SetTextBlockText_DataGridRowCount(tbWipListCount, Util.NVC(dtResult.Rows.Count));
            }
            catch (Exception ex)
            {
                //Util.AlertByBiz("DA_PRD_SEL_WIP_PACK_REWORK", ex.Message, ex.ToString());
                Util.MessageException(ex);
            }
        }

        private void Refresh()
        {
            try
            {
                //�����ȸ
                setWipList();

                //�׸��� clear
                Util.gridClear(dgDefect);
                Util.gridClear(dgInspection);
                Util.gridClear(dgDefectHist);

                txtReworkLotid.Text = string.Empty;
                txtReworkLotInput.Text = string.Empty;
                txtReworkProductid.Text = string.Empty;
                txtReworkProcCause.Text = string.Empty;

                txtConfirmNote.Document.Blocks.Clear();

                //cboReworkReturnProcess.IsEnabled = false;
                cboReworkReturnProcess.SelectedValue = null;
                cboReworkReturnProcess.ItemsSource = null;
                //cboReworkReturnProcess.Items.Clear();

                //���� ���� ����ȸ
                setReworkResultQty();

                cboArea.IsEnabled = true;
                cboEquipmentSegment.IsEnabled = true;

            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private void setReturnProcess()
        {
            try
            {
                if (dtReturnProcess != null)
                {
                    cboReworkReturnProcess.ItemsSource = dtReturnProcess.AsDataView();
                    cboReworkReturnProcess.SelectedIndex = 0;
                }
            }
            catch(Exception ex)
            {
                throw ex;
            }
        }
        /// <summary>
        /// ��� ���ý� ���Ե� ������ ǥ��
        /// </summary>
        private void setScrapProcess()
        {
            try
            {
                DataTable dtResult = getSelectedProcid(Process_Type.SCRAP);
                
                cboReworkReturnProcess.ItemsSource = dtResult.AsDataView();
                cboReworkReturnProcess.SelectedIndex = 0;
            }
            catch(Exception ex)
            {
                throw ex;
            }
        }

        private void setInspectionColor()
        {
            try
            {
                for (int i = 0; i < dgInspection.Rows.Count; i++)
                {
                    C1.WPF.DataGrid.DataGridCell cell = dgInspection.GetCell(i, 2);
                    
                    Decimal dValue = Convert.ToDecimal(cell.Text);
                    Decimal dCLCTLSL = Util.NVC_Decimal(DataTableConverter.GetValue(dgInspection.Rows[i].DataItem, "CLCTLSL"));
                    Decimal dCLCTUSL = Util.NVC_Decimal(DataTableConverter.GetValue(dgInspection.Rows[i].DataItem, "CLCTUSL"));

                    if (dValue >= dCLCTLSL &&
                       dValue <= dCLCTUSL)
                    {
                        if (cell.Presenter != null)
                        {
                            cell.Presenter.Foreground = new SolidColorBrush(Colors.Blue);
                            cell.Presenter.FontWeight = FontWeights.Regular;
                        }
                        DataTableConverter.SetValue(dgInspection.Rows[i].DataItem, "PASSYN", "Y");
                    }
                    else
                    {
                        if (cell.Presenter != null)
                        {
                            cell.Presenter.Foreground = new SolidColorBrush(Colors.Red);
                            cell.Presenter.FontWeight = FontWeights.Bold;
                        }
                        DataTableConverter.SetValue(dgInspection.Rows[i].DataItem, "PASSYN", "N");
                    }
                }
            }
            catch(Exception ex)
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
                dr["SRCTYPE"] = "UI";
                dr["LANGID"] = LoginInfo.LANGID;
                dr["PROCID"] = sProcess_cause;
                dr["ROUTID"] = sRoutid;
                dr["FLOWID"] = sFlowid;
                RQSTDT.Rows.Add(dr);

                dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_PROC_ROUTE_PREVIOUS", "RQSTDT", "RSLTDT", RQSTDT);
            }
            catch (Exception ex)
            {
                //Util.AlertByBiz("DA_PRD_SEL_PROC_ROUTE_PREVIOUS", ex.Message, ex.ToString());
                Util.MessageException(ex);
            }
            return dtResult;
        }
        private DataTable getSelectedProcid(string sType)
        {
            DataTable dtResult = null;

            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("EQSGID", typeof(string));
                RQSTDT.Columns.Add("PROCTYPE", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["EQSGID"] = Util.NVC(cboEquipmentSegment.SelectedValue);
                dr["PROCTYPE"] = sType;
                RQSTDT.Rows.Add(dr);

                dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_PROCESS_BY_PROCTYPE_EQSGID", "RQSTDT", "RSLTDT", RQSTDT);
            }
            catch(Exception ex)
            {
                //Util.AlertByBiz("DA_PRD_SEL_PROCESS_BY_PROCTYPE_EQSGID", ex.Message, ex.ToString());
                Util.MessageException(ex);
            }
            return dtResult;
        }

        private void setLabelCode(string sProdID)
        {
            try
            {
                if (sProdID.Length > 0)
                {
                    CommonCombo _combo = new CommonCombo();
                    //�� ����
                    String[] sFilter = { sProdID, null, null, LABEL_TYPE_CODE.PACK };
                    _combo.SetCombo(cboLabelCode, CommonCombo.ComboStatus.NONE, sFilter: sFilter, sCase: "LABELCODE_BY_PROD");

                    int combo_cnt = cboLabelCode.Items.Count;

                    for (int i = 0; i < combo_cnt; i++)
                    {
                        DataRowView drv = cboLabelCode.Items[i] as DataRowView;
                        string temp = drv["CBO_CODE"].ToString();

                        if (now_labelcode == temp)
                        {
                            cboLabelCode.SelectedValue = now_labelcode;
                            break;
                        }
                        else
                        {
                            cboLabelCode.SelectedIndex = 0;
                        }
                    }
                }
                else
                {
                    cboLabelCode.ItemsSource = null;
                    cboLabelCode.SelectedValue = null;
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }


        #endregion

        private void btnReworkCancel_Click(object sender, RoutedEventArgs e)
        {
            Refresh();
        }
    }
}
