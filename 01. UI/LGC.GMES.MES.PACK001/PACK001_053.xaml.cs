/*************************************************************************************
 Created Date : 2019.10.21
      Creator : 김도형
   Decription : Pack 재작업 공정화면(NEW)
--------------------------------------------------------------------------------------
 [Change History]
  2019.10.21  김도형 Initial Created.(PACK001_004 카피)
  2020.03.19  손우석 CSR ID 30636 GMES 제품 폐기 처리에 대한 인터락 기능 적용 [요청번호] C20200212-000273
  2021.10.19  정용석 라인 Combo -> Multi Combo로 변경
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
using LGC.GMES.MES.PACK001.Class;

namespace LGC.GMES.MES.PACK001
{
    public partial class PACK001_053 : UserControl, IWorkArea
    {
        #region Declaration & Constructor
        LGC.GMES.MES.PACK001.Class.MESSAGE_PARAM ms = new LGC.GMES.MES.PACK001.Class.MESSAGE_PARAM();
        private int equipmentSegmentIDMultiComboBindDataCount = 0;
        private string selectedEquipmentSegmentID = string.Empty;
        private string now_labelcode = string.Empty;

        public IFrameOperation FrameOperation
        {
            get;
            set;
        }

        public PACK001_053()
        {
            InitializeComponent();
        }

        /// <summary>
        /// 리워크공정 코드
        /// </summary>
        private string sSelectedProcID = string.Empty;
        /// <summary>
        /// 투입공정 전역변수
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

            dtpDateFrom.SelectedDateTime = DateTime.Now.AddDays(-7);
            dtpDateTo.SelectedDateTime = DateTime.Now;

            CommonCombo _combo = new CommonCombo();

            String[] sFilter3 = { "REWORK_JUDGE" };
            _combo.SetCombo(cboReworkResult, CommonCombo.ComboStatus.NONE, sFilter: sFilter3, sCase: "COMMCODE");

            //String[] sFilterArea = { LoginInfo.CFG_SHOP_ID, Area_Type.PACK };
            //C1ComboBox[] cboAreaChild = { cboEquipmentSegment };
            //_combo.SetCombo(cboArea, CommonCombo.ComboStatus.NONE, sCase: "AREA_AREATYPE", cbChild: cboAreaChild ,sFilter: sFilterArea);

            //C1ComboBox[] cboEquipmentSegmentParent = { cboArea };
            //_combo.SetCombo(cboEquipmentSegment, CommonCombo.ComboStatus.NONE, cbParent: cboEquipmentSegmentParent);

            String[] sFilterArea = { LoginInfo.CFG_SHOP_ID, Area_Type.PACK };
            _combo.SetCombo(cboArea, CommonCombo.ComboStatus.NONE, sCase: "AREA_AREATYPE", cbChild: null, sFilter: sFilterArea);

            String[] sFilter = { "RESP_DEPT" };
            _combo.SetCombo(cboReworkIMPUTE_CODE, CommonCombo.ComboStatus.NONE, sFilter: sFilter, sCase: "COMMCODE");

            // 초기 Load시에 작업정보 보여주는 부분
            this.selectedEquipmentSegmentID = LoginInfo.CFG_EQSG_ID;
            DataTable dtProcInfo = GetSelectedProcID(selectedEquipmentSegmentID, Process_Type.REPAIR);
            if (dtProcInfo.Rows.Count > 0)
            {
                this.sSelectedProcID = Util.NVC(dtProcInfo.Rows[0]["PROCID"]);
            }
            setReworkResultQty();

            // 2021-10-22 : 추가적으로, 재작업 화면 특성 상 라인에서 공정 PC에서 작업하는 경우가 많으므로,
            // 화면 오픈 시 Default Line을 ALL이 아닌 MES Setting에 설정한 LINE ID를 가져오는 것이 더 어떠신가요?
            this.SyncSearchCondition(this.cboEquipmentSegment, LoginInfo.CFG_EQSG_ID, "CBO_CODE");



            this.Loaded -= new System.Windows.RoutedEventHandler(this.UserControl_Loaded);
            tbWipListCount.Text = "[ 0 " + ObjectDic.Instance.GetObjectName("건") + " ]";
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
                    if (!Util.pageAuthCheck(FrameOperation.AUTHORITY))
                    {
                        Util.MessageInfo("FM_ME_0183");
                        return;
                    }

                    this.ReworkInput();
                }
            }
            catch (Exception ex)
            {
                Util.Alert(ex.ToString());
            }
        }

        // 착공처리
        private void ReworkInput()
        {
            DataTable dt = this.GetLOTInfo(this.txtReworkLotInput.Text);
            if (CommonVerify.HasTableRow(dt))
            {
                foreach (DataRow dr in dt.Select())
                {
                    this.selectedEquipmentSegmentID = dr["EQSGID"].ToString();
                }

                DataTable dtProcInfo = GetSelectedProcID(selectedEquipmentSegmentID, Process_Type.REPAIR);
                if (dtProcInfo.Rows.Count > 0)
                {
                    sSelectedProcID = Util.NVC(dtProcInfo.Rows[0]["PROCID"]);
                }
                this.setReworkResultQty();
                ReworkLotStart(dt.Rows[0]["LOTID"].ToString());
                this.txtReworkLotInput.Text = dt.Rows[0]["LOTID"].ToString();
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
                this.ReworkInput();
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
                    //{0} 판정 하시겠습니까 ?
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

        // 라인 콤보가 멀티 콤보로 바뀜에 따라 아래 이벤트 함수 기능 제거.
        private void cboEquipmentSegment_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            //try
            //{
            //    DataTable dtResult = getSelectedProcid(Process_Type.REPAIR);
            //    if (dtResult.Rows.Count > 0)
            //    {
            //        sSelectedProcID = Util.NVC(dtResult.Rows[0]["PROCID"]);
            //    }

            //    setReworkResultQty();
            //}
            //catch (Exception ex)
            //{
            //    Util.Alert(ex.ToString());
            //}
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
                Button button = sender as Button;
                C1.WPF.DataGrid.C1DataGrid c1DataGrid = ((C1.WPF.DataGrid.DataGridCellPresenter)button.Parent).DataGrid;
                c1DataGrid.SelectedIndex = ((C1.WPF.DataGrid.DataGridCellPresenter)button.Parent).Cell.Row.Index;
                string selectedLOTID = Util.NVC(DataTableConverter.GetValue(dgWipList.Rows[c1DataGrid.SelectedIndex].DataItem, "LOTID"));
                this.selectedEquipmentSegmentID = Util.NVC(DataTableConverter.GetValue(dgWipList.Rows[c1DataGrid.SelectedIndex].DataItem, "EQSGID"));

                DataTable dtResult = GetSelectedProcID(selectedEquipmentSegmentID, Process_Type.REPAIR);
                if (dtResult.Rows.Count > 0)
                {
                    sSelectedProcID = Util.NVC(dtResult.Rows[0]["PROCID"]);
                }

                this.setReworkResultQty();

                txtReworkLotInput.Text = selectedLOTID;
                //text에 입력
                //txtReworkLotid.Text = sSelectLotid;

                //착공
                ReworkLotStart(selectedLOTID);
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

        #region Event - CheckBox
        private void chkOmitDate_Checked(object sender, RoutedEventArgs e)
        {
            this.DateTimePickerControlEnabled(false);
        }

        private void chkOmitDate_Unchecked(object sender, RoutedEventArgs e)
        {
            this.DateTimePickerControlEnabled(true);
        }

        private void cboArea_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            PackCommon.SetMultiSelectionComboBox(this.GetEquipmentSegmentInfo(this.cboArea.SelectedValue.ToString()), this.cboEquipmentSegment, ref this.equipmentSegmentIDMultiComboBindDataCount);
        }
        #endregion

        #region - Grid
        private void btnExpandFrameTop_Checked(object sender, RoutedEventArgs e)
        {
            // 20210317 | 김건식 | GridSplitter 와 같이 사용시 GridSplitter 동작 error 발생하여 사용안함 처리
            //ContentRight.RowDefinitions[1].Height = new GridLength(8);
            //LGC.GMES.MES.CMM001.GridLengthAnimation gla = new LGC.GMES.MES.CMM001.GridLengthAnimation();
            //gla.From = new GridLength(0, GridUnitType.Star);
            //gla.To = new GridLength(6, GridUnitType.Star);
            //gla.Duration = new TimeSpan(0, 0, 0, 0, 500);
            //ContentRight.RowDefinitions[2].BeginAnimation(RowDefinition.HeightProperty, gla);
        }

        private void btnExpandFrameTop_Unchecked(object sender, RoutedEventArgs e)
        {
            // 20210317 | 김건식 | GridSplitter 와 같이 사용시 GridSplitter 동작 error 발생하여 사용안함 처리
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
        private void SyncSearchCondition(MultiSelectionBox multiSelectionBox, string inputID, string searchKey)
        {
            DataTable dt = DataTableConverter.Convert(multiSelectionBox.ItemsSource);
            if (string.IsNullOrEmpty(inputID))
            {
                multiSelectionBox.Check(-1);
            }
            else
            {
                int index = 0;
                foreach (DataRowView drv in dt.AsDataView())
                {
                    if (inputID.Contains(drv[searchKey].ToString()))
                    {
                        multiSelectionBox.Check(index++);
                    }
                    else
                    {
                        multiSelectionBox.Uncheck(index++);
                    }
                }
            }
        }

        // DateTimePicker Control Enabled Disabled
        private void DateTimePickerControlEnabled(bool isEnabled)
        {
            this.dtpDateFrom.IsEnabled = isEnabled;
            this.dtpDateTo.IsEnabled = isEnabled;
        }

        /// <summary>
        /// 리워크공정 착공
        /// </summary>
        /// <param name="sLotid"></param>
        private void ReworkLotStart(string sLotid)
        {
            try
            {
                if (!(sLotid.Length > 0))
                {
                    ms.AlertWarning("SFU1366"); //LOT ID를 입력해주세요
                    return;
                }

                if (!(sSelectedProcID.Length > 0))
                {
                    ms.AlertWarning("SFU1296", this.selectedEquipmentSegmentID); //{0}라인에재작업공정정보가없습니다.\r\n기준정보를확인하세요.
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
                dr["EQSGID"] = this.selectedEquipmentSegmentID;
                INDATA.Rows.Add(dr);

                dsInput.Tables.Add(INDATA);

                dsResult = new ClientProxy().ExecuteServiceSync_Multi("BR_PRD_REG_REWORK_START_LOT_PACK_LINE", "INDATA", "WIP_PROC,ACTIVITYREASON,LOT_INFO,OUTDATA,WIPREASONCOLLECT,WO_MTRL", dsInput, null);

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
                        //바인딩후 포커스이동
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

                }
            }
            catch (Exception ex)
            {
                //Util.AlertByBiz("BR_PRD_REG_REWORK_START_LOT_PACK", ex.Message, ex.ToString());
                Util.MessageException(ex);
            }
        }

        /// <summary>
        /// 리워크공정 완료
        /// </summary>
        private void ReworkLotEnd()
        {
            try
            {
                TextRange textRange = new TextRange(txtConfirmNote.Document.ContentStart, txtConfirmNote.Document.ContentEnd);
                string sNote = textRange.Text.Trim();



                //resncode 양품인경우는 OK
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
                        ms.AlertInfo("SFU1275"); //정상처리 되었습니다.

                        Refresh();
                    }
                }

            }
            catch (Exception ex)
            {
                //Util.AlertByBiz("BR_PRD_REG_OUTPUTASSY", ex.Message, ex.ToString());
                Util.MessageException(ex);
            }
        }

        /// <summary>
        /// 완료처리전 입력확인.
        /// </summary>
        /// <returns></returns>
        private bool ConfirmValidation()
        {
            bool bReturn = true;
            try
            {
                #region 완공대상lot 존재 확인
                //완공LOT 존재 확인.
                if (txtReworkLotid.Text == "")
                {
                    ms.AlertRetun("SFU1746"); //완료할 LOT이 없습니다.
                    bReturn = false;
                    txtReworkLotid.Focus();
                    return bReturn;
                }
                #endregion

                #region 유형코드 선택여부확인
                //재생일경우 VALIDATION
                int iRow = -1;
                if (Util.NVC(cboReworkResult.SelectedValue) == "REWORK")
                {
                    //재생코드 기준정보가 있는경우에만 선택 validation
                    if (dgDefect.Rows.Count > 0)
                    {
                        iRow = Util.gridFindDataRow(ref dgDefect, "CHK", "True", false);

                        if (iRow == -1)
                        {
                            ms.AlertWarning("SFU1642"); //선택된 유형코드가 없습니다
                            bReturn = false;
                            return bReturn;
                        }
                    }
                }
                #endregion

                #region NOTE입력 확인
                //수동 처리시 NOTE 필수입력
                TextRange textRange = new TextRange(txtConfirmNote.Document.ContentStart, txtConfirmNote.Document.ContentEnd);
                string sNote = textRange.Text.Trim();
                if (sNote == "")
                {
                    ms.AlertWarning("SFU1898"); //조치내역은 필수 항목 입니다.
                    txtConfirmNote.Focus();
                    bReturn = false;
                    return bReturn;
                }
                #endregion

                #region 검사DAta 입력확인
                //if (dgInspection.Rows.Count > 0)
                //{
                //    for (int i = 0; i < dgInspection.Rows.Count; i++)
                //    {
                //        string itemVal = Util.NVC(DataTableConverter.GetValue(dgInspection.Rows[i].DataItem, "CLCTVAL"));
                //        if (itemVal == "")
                //        {
                //            LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("입력하신 측정값이 옮지 않습니다. 확인 하세요"), null, "Info", MessageBoxButton.OK, MessageBoxIcon.Warning);
                //            //포커스이동
                //            Util.gridSetFocusRow(ref dgInspection, i);
                //            bReturn = false;
                //            return bReturn;
                //        }
                //    }
                //}

                //양품일경우 VALIDATION
                //if (cboReworkResult.SelectedIndex == 0)
                //{
                //    if (GetDefectCount())
                //    {
                //        LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("입력데이터중 불량 판정된 항목이 있습니다. 무시하고 양품 판정 하시겠습니까?."), null, "Information", MessageBoxButton.OKCancel, MessageBoxIcon.None, (sResult) =>
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

                #region 투입공정 선택확인
                string sSelectedStartProc = Util.NVC(cboReworkReturnProcess.SelectedValue);
                if (!(sSelectedStartProc.Length > 0))
                {
                    ms.AlertWarning("SFU1968"); //투입공정를선택하세요.
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
        /// 실적조회.
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
                dr["EQSGID"] = this.selectedEquipmentSegmentID;
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

        private DataTable GetLOTInfo(string lotID)
        {
            DataTable dt = new DataTable();

            try
            {
                DataSet dsINDATA = new DataSet();

                DataTable dtINDATA = new DataTable("INDATA");
                dtINDATA.TableName = "INDATA";
                dtINDATA.Columns.Add("LANGID", typeof(string));
                dtINDATA.Columns.Add("PROCID", typeof(string));
                dtINDATA.Columns.Add("EQSGID", typeof(string));
                dtINDATA.Columns.Add("PROCTYPE", typeof(string));
                dtINDATA.Columns.Add("UPDDTTM_FROM", typeof(string));
                dtINDATA.Columns.Add("UPDDTTM_TO", typeof(string));
                dtINDATA.Columns.Add("LOTID", typeof(string));

                DataRow drINDATA = dtINDATA.NewRow();
                drINDATA["LANGID"] = LoginInfo.LANGID;
                drINDATA["PROCID"] = null;
                drINDATA["EQSGID"] = null;
                drINDATA["PROCTYPE"] = Process_Type.REPAIR;
                drINDATA["UPDDTTM_FROM"] = null;
                drINDATA["UPDDTTM_TO"] = null;
                drINDATA["LOTID"] = lotID;
                dtINDATA.Rows.Add(drINDATA);

                dsINDATA.Tables.Add(dtINDATA);
                dsResult = new ClientProxy().ExecuteServiceSync_Multi("BR_PRD_GET_WIP_PACK_REWORK", "INDATA", "OUTDATA", dsINDATA, null);

                if (CommonVerify.HasTableInDataSet(dsResult) && CommonVerify.HasTableRow(dsResult.Tables["OUTDATA"]))
                {
                    dt = dsResult.Tables["OUTDATA"].Copy();
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }

            return dt;
        }

        private void setWipList()
        {
            if (string.IsNullOrEmpty(Convert.ToString(this.cboEquipmentSegment.SelectedItemsToString)))
            {
                Util.MessageValidation("SFU4925", ObjectDic.Instance.GetObjectName("라인")); // %1(을)를 선택하세요.
                this.cboEquipmentSegment.Focus();
                return;
            }

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
                dr["EQSGID"] = this.cboEquipmentSegment.SelectedItemsToString;
                dr["PROCTYPE"] = Process_Type.REPAIR;
                dr["UPDDTTM_FROM"] = this.chkOmitDate.IsChecked == true ? null : dtpDateFrom.SelectedDateTime.ToString("yyyyMMdd");
                dr["UPDDTTM_TO"] = this.chkOmitDate.IsChecked == true ? null : dtpDateTo.SelectedDateTime.AddDays(1).ToString("yyyyMMdd");
                //2020.03.19
                dr["LOTID"] = null;
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_WIP_PACK_REWORK_V2", "RQSTDT", "RSLTDT", RQSTDT);

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


                //그리드 clear
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

                //실적 수량 재조회
                setReworkResultQty();

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
            catch (Exception ex)
            {
                throw ex;
            }
        }
        /// <summary>
        /// 폐기 선택시 투입될 폐기공정 표시
        /// </summary>
        private void setScrapProcess()
        {
            try
            {
                DataTable dtResult = GetSelectedProcID(this.selectedEquipmentSegmentID, Process_Type.SCRAP);

                cboReworkReturnProcess.ItemsSource = dtResult.AsDataView();
                cboReworkReturnProcess.SelectedIndex = 0;
            }
            catch (Exception ex)
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
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// 재생이후 투입될 공정 의 dataTable
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

        // 라인 콤보가 멀티선택 콤보로 바뀜에 따라 인자값에 EquipmentSegment 추가
        private DataTable GetSelectedProcID(string equipmentSegmentID, string procType)
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
                dr["EQSGID"] = equipmentSegmentID;
                dr["PROCTYPE"] = procType;
                RQSTDT.Rows.Add(dr);

                dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_PROCESS_BY_PROCTYPE_EQSGID", "RQSTDT", "RSLTDT", RQSTDT);
            }
            catch (Exception ex)
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
                    //라벨 세팅
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

        // BizRule 호출 - EquipmentSegment
        public DataTable GetEquipmentSegmentInfo(string areaID = null)
        {
            DataTable dtReturn = new DataTable();
            try
            {
                string bizRuleName = "DA_BAS_SEL_EQUIPMENTSEGMENT_CBO";
                DataTable dtRQSTDT = new DataTable("RQSTDT");
                DataTable dtRSLTDT = new DataTable("RSLTDT");

                dtRQSTDT.Columns.Add("LANGID", typeof(string));
                dtRQSTDT.Columns.Add("AREAID", typeof(string));

                DataRow drRQSTDT = dtRQSTDT.NewRow();
                drRQSTDT["LANGID"] = LoginInfo.LANGID;
                drRQSTDT["AREAID"] = areaID;
                dtRQSTDT.Rows.Add(drRQSTDT);

                dtRSLTDT = new ClientProxy().ExecuteServiceSync(bizRuleName, dtRQSTDT.TableName, dtRSLTDT.TableName, dtRQSTDT);
                if (CommonVerify.HasTableRow(dtRSLTDT))
                {
                    dtReturn = dtRSLTDT;
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }

            return dtReturn;
        }
        #endregion

        private void btnReworkCancel_Click(object sender, RoutedEventArgs e)
        {
            Refresh();
        }
    }
}
