/*************************************************************************************
 Created Date : 2016.08.01
      Creator : Jeong Hyeon Sik
   Decription : Pack ������ ȭ��
--------------------------------------------------------------------------------------
 [Change History]
  2016.08.01  Jeong Hyeon Sik : Initial Created.
  2018.11.22 �տ켮 CSR ID 3850465 �� GMES ������ ȭ�� ���� ��û ��  [��û��ȣ]C20181121_50465
  2020.03.12 �տ켮 CSR ID 30636 GMES ��ǰ ��� ó���� ���� ���Ͷ� ��� ���� [��û��ȣ] C20200212-000273
  2020.03.19 �տ켮 CSR ID 30636 GMES ��ǰ ��� ó���� ���� ���Ͷ� ��� ���� [��û��ȣ] C20200212-000273
  2020.05.13 �տ켮 MES UI-��� ���� ���� �� ���� ���� ��û
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
    public partial class PACK001_023 : UserControl , IWorkArea
    {
        #region Declaration & Constructor 
        public IFrameOperation FrameOperation
        {
            get;
            set;
        }

        LGC.GMES.MES.PACK001.Class.MESSAGE_PARAM ms = new LGC.GMES.MES.PACK001.Class.MESSAGE_PARAM();
        DataSet dsResult = new DataSet();
        string now_labelcode = string.Empty;

        private string sSelectedProcID = string.Empty;
        private string sSelectedReworkProcID = string.Empty;

        //2020.03.12
        private int iKeypart = 0;
        private string sPRDT_CLSS_CODE = "";

        CommonCombo _combo = new CommonCombo();

        public PACK001_023()
        {
            InitializeComponent();
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


        #endregion

        #region Initialize

        #endregion

        #region Event

        #region Event - UserControl
        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            List<Button> listAuth = new List<Button>();
            listAuth.Add(btnScrapLotInput);
            listAuth.Add(btnScrapConfirm);
            Util.pageAuth(listAuth, FrameOperation.AUTHORITY);

            setScrapResultQty();

            dtpDateFrom.SelectedDateTime = DateTime.Now.AddDays(-7);
            dtpDateTo.SelectedDateTime = DateTime.Now;


            String[] sFilter3 = { "SCRAP_JUDGE" };
            _combo.SetCombo(cboScrapResult, CommonCombo.ComboStatus.NONE, sFilter: sFilter3, sCase: "COMMCODE");

            String[] sFilterArea = { LoginInfo.CFG_SHOP_ID, Area_Type.PACK };
            C1ComboBox[] cboAreaChild = { cboEquipmentSegment };
            _combo.SetCombo(cboArea, CommonCombo.ComboStatus.NONE, sCase: "AREA_AREATYPE", cbChild: cboAreaChild, sFilter: sFilterArea);

            C1ComboBox[] cboEquipmentSegmentParent = { cboArea };
            _combo.SetCombo(cboEquipmentSegment, CommonCombo.ComboStatus.NONE, cbParent: cboEquipmentSegmentParent);

            String[] sFilter = { "RESP_DEPT" };
            _combo.SetCombo(cboScrapIMPUTE_CODE, CommonCombo.ComboStatus.NONE, sFilter: sFilter, sCase: "COMMCODE");

            _combo.SetCombo(cboCostType, CommonCombo.ComboStatus.NONE, sCase: "cboDefectType");

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
                        txtScrapLotInput.Focus();
                        txtScrapLotInput.SelectAll();
                        break;
                    default:
                        break;
                }

            }
            catch { }
        }
        #endregion Event

        #region Event - Button
        private void btnWipSearch_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                //2018.11.22
                if (!ValidationWindingLotSearch()) return;

                setWipList();
            }
            catch (Exception ex)
            {
                Util.Alert(ex.ToString());
            }
        }

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
                Util.Alert(ex.ToString());
            }
        }

        private void btnScrapLotInput_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                txtScrapNote.SelectAll();
                txtScrapNote.Selection.Text = "";

                ScrapLotStart(txtScrapLotInput.Text);
            }
            catch (Exception ex)
            {
                Util.Alert(ex.ToString());
            }
        }

        private void btnScrapConfirm_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (ConfirmValidation())
                {
                    LGC.GMES.MES.ControlsLibrary.MessageBox.Show(ms.AlertRetun("SFU1294", cboScrapResult.Text), null, "Information", MessageBoxButton.OKCancel, MessageBoxIcon.None, (sResult) =>
                    //{0} ���� �Ͻðڽ��ϱ� ? 
                    {
                        if (sResult == MessageBoxResult.OK)
                        {
                            ScrapLotEnd();
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

        #region Event - TextBox

        private void txtScrapLotInput_KeyDown(object sender, KeyEventArgs e)
        {
            try
            {
                if (e.Key == Key.Enter)
                {
                    ScrapLotStart(txtScrapLotInput.Text);
                }
            }
            catch (Exception ex)
            {
                Util.Alert(ex.ToString());
            }
        }

        #endregion

        #region Event - ComboBox
        private void cboScrapResult_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            try
            {
                if (Util.NVC(cboScrapResult.SelectedValue) == "REWORK_WAIT")
                {
                    //����ϰ�� �ʱ�ȭ.
                    Util.gridClear(dgDefect);
                    Util.gridClear(dgWoList);

                }
                else
                {
                    //����ϰ�� ������.
                    if (dsResult != null)
                    {
                        GetDefect();
                        GetWoMtrlList();
                    }
                }
            }
            catch (Exception ex)
            {
                Util.Alert(ex.ToString());
            }
        }

        private void cboCostType_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            if(Util.NVC(cboScrapResult.SelectedValue) == "REWORK_WAIT")
             {
                //����ϰ�� �ʱ�ȭ.
                Util.gridClear(dgDefect);
            }
                    else
             {
                //����ϰ�� ������.
                if (dsResult != null)
                {
                    GetDefect();
                    
                }
            }

        }

        private void cboEquipmentSegment_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            try
            {
                DataTable dtResult = getSelectedProcid(Process_Type.SCRAP);

                if (dtResult.Rows.Count > 0)
                {
                    sSelectedProcID = Util.NVC(dtResult.Rows[0]["PROCID"]);
                }
                DataTable dtResultRework = getSelectedProcid(Process_Type.REPAIR);

                if (dtResultRework.Rows.Count > 0)
                {
                    sSelectedReworkProcID = Util.NVC(dtResultRework.Rows[0]["PROCID"]);
                }

                setScrapResultQty();
            }
            catch(Exception ex)
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

        private void wipListInputButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Button bt = sender as Button;

                C1.WPF.DataGrid.C1DataGrid dg = ((C1.WPF.DataGrid.DataGridCellPresenter)bt.Parent).DataGrid;

                dg.SelectedIndex = ((C1.WPF.DataGrid.DataGridCellPresenter)bt.Parent).Cell.Row.Index;

                string sSelectLotid = Util.NVC(DataTableConverter.GetValue(dgWipList.Rows[dg.SelectedIndex].DataItem, "LOTID"));

                txtScrapLotInput.Text = sSelectLotid;

                //����
                ScrapLotStart(sSelectLotid);
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
                if (sender == null)
                    return;

                RadioButton rb = sender as RadioButton;

                if (rb.DataContext == null)
                    return;

                C1.WPF.DataGrid.C1DataGrid dg = ((C1.WPF.DataGrid.DataGridCellPresenter)rb.Parent).DataGrid;

                if ((bool)rb.IsChecked && (rb.DataContext as DataRowView).Row["CHK"].ToString().Equals("0"))
                {
                    int idx = ((C1.WPF.DataGrid.DataGridCellPresenter)rb.Parent).Row.Index;

                 //   C1.WPF.DataGrid.C1DataGrid dg = ((C1.WPF.DataGrid.DataGridCellPresenter)rb.Parent).DataGrid;
                    for (int i = 0; i < dg.GetRowCount(); i++)
                    {
                        if (idx == i)
                            DataTableConverter.SetValue(dg.Rows[i].DataItem, ((C1.WPF.DataGrid.DataGridCellPresenter)rb.Parent).Column.Name, true);
                        else
                            DataTableConverter.SetValue(dg.Rows[i].DataItem, ((C1.WPF.DataGrid.DataGridCellPresenter)rb.Parent).Column.Name, false);
                    }

                    dgDefect.SelectedIndex = idx;

                    //TextRange textRange = new TextRange(txtScrapNote.Document.ContentStart, txtScrapNote.Document.ContentEnd);
                    //string sTemp = textRange.Text.Replace("\r\n", "");

                    string sTemp = DataTableConverter.GetValue(dg.Rows[idx].DataItem, "RESNNAME").ToString();

                    if (sTemp.Length > 0 && !(string.IsNullOrWhiteSpace(sTemp)))
                    {
                       //  RadioButton rbt = sender as RadioButton;
                       // C1.WPF.DataGrid.C1DataGrid dg = ((C1.WPF.DataGrid.DataGridCellPresenter)rbt.Parent).DataGrid;

                        dg.SelectedIndex = ((C1.WPF.DataGrid.DataGridCellPresenter)rb.Parent).Cell.Row.Index;

                        string sRESNNAME = DataTableConverter.GetValue(dg.Rows[dg.SelectedIndex].DataItem, "RESNNAME").ToString();
                        txtScrapNote.SelectAll();
                        txtScrapNote.Selection.Text = "";
                        txtScrapNote.AppendText(sRESNNAME);

                    }
                }
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

        private void dgWoList_LoadedColumnHeaderPresenter(object sender, C1.WPF.DataGrid.DataGridColumnEventArgs e)
        {
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

        void checkAll_Checked(object sender, RoutedEventArgs e)
        {
            if (sender == null)
                return;

            DataTable dt = DataTableConverter.Convert(dgWoList.ItemsSource);

            for (int idx = 0; idx < dt.Rows.Count; idx++)
            {
                dt.Rows[idx]["CHK"] = 1;
            }

            dgWoList.ItemsSource = DataTableConverter.Convert(dt);

        }

        void checkAll_Unchecked(object sender, RoutedEventArgs e)
        {
            if (sender == null)
                return;

            DataTable dt = DataTableConverter.Convert(dgWoList.ItemsSource);
            for (int idx = 0; idx < dt.Rows.Count; idx++)
            {
                dt.Rows[idx]["CHK"] = 0;
            }
            dgWoList.ItemsSource = DataTableConverter.Convert(dt);

        }
        
        /// <summary>
        /// ����ũ���� ����
        /// </summary>
        /// <param name="sLotid"></param>
        private void ScrapLotStart(string sLotid)
        {
            try
            {
                if (!(sLotid.Length > 0))
                {
                    ms.AlertWarning("SFU1366"); //LOT ID�� �Է����ּ���
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
                dr["PROCTYPE"] = Process_Type.SCRAP;
                dr["UPDDTTM_FROM"] = dtpDateFrom.SelectedDateTime.ToString("yyyyMMdd");
                dr["UPDDTTM_TO"] = dtpDateTo.SelectedDateTime.AddDays(1).ToString("yyyyMMdd");
                dr["EQSGID"] = cboEquipmentSegment.SelectedValue.ToString();
                INDATA.Rows.Add(dr);

                dsInput.Tables.Add(INDATA);

                dsResult = new ClientProxy().ExecuteServiceSync_Multi("BR_PRD_REG_REWORK_START_LOT_PACK", "INDATA", "WIP_PROC,ACTIVITYREASON,LOT_INFO,OUTDATA,WIPREASONCOLLECT,WO_MTRL", dsInput, null);
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
                            //sStartedProductName = Util.NVC(dsResult.Tables["LOT_INFO"].Rows[0]["PRODNAME"]);
                            sStartedProductDesc = Util.NVC(dsResult.Tables["LOT_INFO"].Rows[0]["PRODDESC"]);
                            string sProcID_Cause = Util.NVC(dsResult.Tables["LOT_INFO"].Rows[0]["PROCID_CAUSE_NAME"]);
                            string sRoutID = Util.NVC(dsResult.Tables["LOT_INFO"].Rows[0]["ROUTID"]);
                            string sFlowID = Util.NVC(dsResult.Tables["LOT_INFO"].Rows[0]["FLOWID"]);;
                            string sScrap_ResnCode_Cause = Util.NVC(dsResult.Tables["LOT_INFO"].Rows[0]["SCRAP_RESNCODE_CAUSE"]);
                            if (sScrap_ResnCode_Cause.Length > 0)
                            {
                                cboScrapIMPUTE_CODE.SelectedValue = sScrap_ResnCode_Cause;
                            }
                            txtScrapLotid.Text = sStartedLotid;
                            txtScrapProductid.Text = sStartedProductDesc;
                            txtScrapProcCause.Text = sProcID_Cause;

                        }
                    }
                    if ((dsResult.Tables.IndexOf("WIP_PROC") > -1))
                    {
                        //dgWipList.ItemsSource = DataTableConverter.Convert(dsResult.Tables["WIP_PROC"]);
                        Util.GridSetData(dgWipList, dsResult.Tables["WIP_PROC"], FrameOperation, true);
                        Util.SetTextBlockText_DataGridRowCount(tbWipListCount, Util.NVC(dgWipList.Rows.Count));
                        //���ε��� ��Ŀ���̵�
                        Util.gridFindDataRow(ref dgWipList, "LOTID", sStartedLotid, true);

                        if (!string.IsNullOrWhiteSpace(Util.NVC(dsResult.Tables["WIP_PROC"].Rows[0]["PRDT_CLSS_CODE"])))
                        {
                            sPRDT_CLSS_CODE = Util.NVC(dsResult.Tables["WIP_PROC"].Rows[0]["PRDT_CLSS_CODE"]);
                        }
                        //2020.03.12
                        //sPRDT_CLSS_CODE = Util.NVC(dsResult.Tables["WIP_PROC"].Rows[0]["PRDT_CLSS_CODE"]);
                        //2020.03.19
                        //2020.07.01 
                        // ���Թ� ���� �ش�� ������ ���ؼ�, ���� ó��
                        //setWip(sLotid);
                    }
                    if ((dsResult.Tables.IndexOf("WIPREASONCOLLECT") > -1))
                    {
                        dgDefectHist.ItemsSource = DataTableConverter.Convert(dsResult.Tables["WIPREASONCOLLECT"]);
                    }

                    if ((dsResult.Tables.IndexOf("WO_MTRL") > -1))
                    {
                        if (Convert.ToString(cboScrapResult.SelectedValue).Equals("SCRAP"))
                        {
                            dgWoList.ItemsSource = DataTableConverter.Convert(dsResult.Tables["WO_MTRL"]);
                            
                        }
                       
                    }
                    //if ((dsResult.Tables.IndexOf("ACTIVITYREASON") > -1))
                    //{
                    //    if (Util.NVC(cboScrapResult.SelectedValue) == "SCRAP")
                    //    {
                    //        dgDefect.ItemsSource = DataTableConverter.Convert(dsResult.Tables["ACTIVITYREASON"]);
                    //    }
                    //}

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
        private void ScrapLotEnd()
        {
            try
            {
                //2020.03.12
                getKeypart(txtScrapLotInput.Text);

                if ((iKeypart == 0) && (sPRDT_CLSS_CODE != "CELL"))
                {
                    //2020.03.12
                    //Keypart�� ������ LOTID�� ��� �� �� �����ϴ�.
                    Util.MessageValidation("SFU8172");
                    return;
                }
                else
                {
                    TextRange textRange = new TextRange(txtScrapNote.Document.ContentStart, txtScrapNote.Document.ContentEnd);
                    string sNote = textRange.Text;
                    string sStartProc = null;

                    //resncode �������ΰ��� OK
                    string sResnCode = string.Empty;
                    if (Util.NVC(cboScrapResult.SelectedValue) == "REWORK_WAIT")
                    {
                        sResnCode = "OK";
                        sStartProc = sSelectedReworkProcID;
                    }
                    else
                    {
                        if (dgDefect.Rows.Count > 0)
                        {
                            // MES 2.0 CHK �÷� Bool ���� Patch
                            //sResnCode = Util.gridFindDataRow_GetValue(ref dgDefect, "CHK", "1", "RESNCODE");
                            sResnCode = Util.gridFindDataRow_GetValue(ref dgDefect, "CHK", "True", "RESNCODE");
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
                    INDATA.Columns.Add("PROCID", typeof(string));
                    INDATA.Columns.Add("EQPTID", typeof(string));
                    //INDATA.Columns.Add("END_PROCID", typeof(string));
                    //INDATA.Columns.Add("END_EQPTID", typeof(string));
                    INDATA.Columns.Add("STRT_PROCID", typeof(string));
                    INDATA.Columns.Add("STRT_EQPTID", typeof(string));
                    INDATA.Columns.Add("USERID", typeof(string));
                    INDATA.Columns.Add("WIPNOTE", typeof(string));
                    INDATA.Columns.Add("RESNCODE", typeof(string));
                    INDATA.Columns.Add("RESNNOTE", typeof(string));
                    INDATA.Columns.Add("RESNDESC", typeof(string));
                    INDATA.Columns.Add("ACTID", typeof(string));

                    DataRow drINDATA = INDATA.NewRow();
                    drINDATA["SRCTYPE"] = "UI";
                    drINDATA["LANGID"] = LoginInfo.LANGID;
                    drINDATA["LOTID"] = txtScrapLotInput.Text;
                    drINDATA["PROCID"] = sSelectedProcID;
                    drINDATA["EQPTID"] = null;
                    //drINDATA["END_PROCID"] = sSelectedProcID;
                    //drINDATA["END_EQPTID"] = null;
                    drINDATA["STRT_PROCID"] = sStartProc;
                    drINDATA["STRT_EQPTID"] = null;
                    drINDATA["USERID"] = LoginInfo.USERID;
                    drINDATA["WIPNOTE"] = sNote;
                    drINDATA["RESNCODE"] = sResnCode;
                    drINDATA["RESNNOTE"] = sNote;
                    drINDATA["RESNDESC"] = Util.NVC(cboScrapIMPUTE_CODE.SelectedValue);
                    drINDATA["ACTID"] = Util.NVC(cboCostType.SelectedValue);
                    INDATA.Rows.Add(drINDATA);

                    dsInput.Tables.Add(INDATA);

                    //DataTable IN_CLCTITEM = new DataTable();
                    //IN_CLCTITEM.TableName = "IN_CLCTITEM";
                    //IN_CLCTITEM.Columns.Add("CLCTITEM", typeof(string));
                    //IN_CLCTITEM.Columns.Add("CLCTVAL", typeof(string));
                    //IN_CLCTITEM.Columns.Add("PASSYN", typeof(string));
                    //dsInput.Tables.Add(IN_CLCTITEM);

                    //DataTable IN_CLCTDITEM = new DataTable();
                    //IN_CLCTDITEM.TableName = "IN_CLCTDITEM";
                    //IN_CLCTDITEM.Columns.Add("CLCTDITEM", typeof(string));
                    //IN_CLCTDITEM.Columns.Add("CLCTDVAL", typeof(string));
                    //dsInput.Tables.Add(IN_CLCTDITEM);

                    DataTable IN_MTRL = new DataTable();
                    IN_MTRL.TableName = "IN_MTRL";
                    IN_MTRL.Columns.Add("MTRLID", typeof(string));
                    IN_MTRL.Columns.Add("RESNQTY", typeof(decimal));
                    IN_MTRL.Columns.Add("RSV_ITEM_NO", typeof(string));
                    IN_MTRL.Columns.Add("ITEM_NO", typeof(string));

                    for (int i = 0; i < dgWoList.GetRowCount(); i++)
                    {
                        if (Util.NVC(DataTableConverter.GetValue(dgWoList.Rows[i].DataItem, "CHK")).Equals("1") || Util.NVC(DataTableConverter.GetValue(dgWoList.Rows[i].DataItem, "CHK")).Equals("True"))
                        {
                            DataRow dr = IN_MTRL.NewRow();
                            dr["MTRLID"] = Util.NVC(DataTableConverter.GetValue(dgWoList.Rows[i].DataItem, "MTRLID"));
                            dr["RESNQTY"] = (decimal)DataTableConverter.GetValue(dgWoList.Rows[i].DataItem, "RESNQTY");
                            dr["RSV_ITEM_NO"] = Util.NVC(DataTableConverter.GetValue(dgWoList.Rows[i].DataItem, "RSV_ITEM_NO"));
                            dr["ITEM_NO"] = Util.NVC(DataTableConverter.GetValue(dgWoList.Rows[i].DataItem, "ITEM_NO"));
                            IN_MTRL.Rows.Add(dr);
                        }
                    }
                    dsInput.Tables.Add(IN_MTRL);

                    DataSet dsResult = new ClientProxy().ExecuteServiceSync_Multi("BR_PRD_REG_OUTPUTLOT_SCRAP", "INDATA, IN_MTRL", "OUTDATA", dsInput, null);
                    // DataSet dsResult = new ClientProxy().ExecuteServiceSync_Multi("BR_PRD_REG_OUTPUTASSY", "INDATA,IN_CLCTITEM,IN_CLCTDITEM", "OUTDATA", dsInput, null);
                    if (dsResult.Tables["OUTDATA"] != null)
                    {
                        if (dsResult.Tables["OUTDATA"].Rows.Count > 0)
                        {
                            //����ó�� �Ǿ����ϴ�.
                            LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("SFU1275"), null, "Info", MessageBoxButton.OK, MessageBoxIcon.None);

                            Refresh();
                        }
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
        /// �Ϸ�ó���� �Է�Ȯ��.
        /// </summary>
        /// <returns></returns>
        private bool ConfirmValidation()
        {
            bool bReturn = true;
            try
            {
                #region ����Ȯ�� ����
                
                #endregion

                #region �ϰ����lot ���� Ȯ��
                //�ϰ�LOT ���� Ȯ��.
                if (txtScrapLotid.Text == "")
                {
                    ms.AlertWarning("SFU1746"); //�Ϸ��� LOT�� �����ϴ�.
                    bReturn = false;
                    txtScrapLotid.Focus();
                    return bReturn;
                }
                #endregion

                #region �����ڵ� ���ÿ���Ȯ��               
                //����ϰ�� VALIDATION
                int iRow = -1;
                if (Util.NVC(cboScrapResult.SelectedValue) == "SCRAP")
                {
                    //����ڵ� ���������� �ִ°�쿡�� ���� validation
                    //if (dgDefect.Rows.Count > 0)
                    //{
                        iRow = Util.gridFindDataRow(ref dgDefect, "CHK", "1", false);
                       

                        if (iRow == -1)
                        {
                            ms.AlertWarning("SFU1642"); //���õ� �����ڵ尡 �����ϴ�
                            bReturn = false;
                            return bReturn;
                        }
                    //}
                }
                #endregion

                #region NOTE�Է� Ȯ��
                //���� ó���� NOTE �ʼ��Է�
                TextRange textRange = new TextRange(txtScrapNote.Document.ContentStart, txtScrapNote.Document.ContentEnd);
                string sNote = textRange.Text.Replace("\r\n", "");
                if (sNote == "")
                {
                    ms.AlertWarning("SFU1898"); //��ġ������ �ʼ� �׸� �Դϴ�.
                    txtScrapNote.Focus();
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
            }
            catch (Exception ex)
            {
                throw ex;
            }

            return bReturn;
        }

        //2018.11.22
        private bool ValidationWindingLotSearch()
        {
            TimeSpan timeSpan = dtpDateTo.SelectedDateTime.Date - dtpDateFrom.SelectedDateTime.Date;

            if (timeSpan.Days < 0)
            {
                //��ȸ �������ڴ� �������ڸ� �ʰ� �� �� �����ϴ�.
                Util.MessageValidation("SFU3569");
                return false;
            }

            if (timeSpan.Days > 30)
            {
                Util.MessageValidation("SFU4466");
                return false;
            }

            return true;
        }

        /// <summary>
        /// ������ȸ.
        /// </summary>
        private void setScrapResultQty()
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

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_PRODPLAN_RESULT_BY_EQSG_SCRAP", "RQSTDT", "RSLTDT", RQSTDT);

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
                //Util.AlertByBiz("DA_PRD_SEL_PRODPLAN_RESULT_BY_EQSG_SCRAP", ex.Message, ex.ToString());
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
                dr["EQSGID"] = Util.NVC(cboEquipmentSegment.SelectedValue) == "" ? null : Util.NVC(cboEquipmentSegment.SelectedValue);
                dr["PROCTYPE"] = Process_Type.SCRAP;
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
                Util.gridClear(dgDefectHist);
                Util.gridClear(dgWoList);

                txtScrapLotid.Text = string.Empty;
                txtScrapLotInput.Text = string.Empty;
                txtScrapProductid.Text = string.Empty;

                txtScrapNote.SelectAll();
                txtScrapNote.Selection.Text = "";

                txtScrapNote.Document.Blocks.Clear();

                //���� ���� ����ȸ
                setScrapResultQty();

                cboArea.IsEnabled = true;
                cboEquipmentSegment.IsEnabled = true;

            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private void getSelectedProcid()
        {
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
                dr["PROCTYPE"] = Process_Type.SCRAP;
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_PROCESS_BY_PROCTYPE_EQSGID", "RQSTDT", "RSLTDT", RQSTDT);

                if (dtResult.Rows.Count > 0)
                {
                    sSelectedProcID = Util.NVC(dtResult.Rows[0]["PROCID"]);
                }
            }
            catch (Exception ex)
            {
                //Util.AlertByBiz("DA_PRD_SEL_PROCESS_BY_PROCTYPE_EQSGID", ex.Message, ex.ToString());
                Util.MessageException(ex);
            }
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

        private void GetDefect()
        {
            try
            {
                DataTable dt = new DataTable();
                dt.Columns.Add("LANGID", typeof(string));
                dt.Columns.Add("ACTID", typeof(string));
                dt.Columns.Add("AREAID", typeof(string));
                dt.Columns.Add("PROCID", typeof(string));

                DataRow dr = dt.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["ACTID"] = cboCostType.SelectedValue.ToString();
                dr["AREAID"] = cboArea.SelectedValue.ToString();
                dr["PROCID"] = sSelectedProcID;
                dt.Rows.Add(dr);

                DataTable result = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_ACTIVITYREASON_SCRAP_PACK", "RQUST", "RSLT", dt);

                Util.GridSetData(dgDefect, result, FrameOperation, true);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void GetWoMtrlList()
        {
            try
            {
                DataSet ds = new DataSet();

                DataTable INLOT = new DataTable();
                INLOT.Columns.Add("LOTID", typeof(string));

                DataRow dr = INLOT.NewRow();
                dr["LOTID"] = txtScrapLotid.Text;
                INLOT.Rows.Add(dr);

                ds.Tables.Add(INLOT);

                DataSet  OUTDATA = new ClientProxy().ExecuteServiceSync_Multi("BR_PRD_GET_TB_SFC_WO_MTRL", "IN_LOT", "OUTDATA", ds);
                DataTable result = OUTDATA.Tables["OUTDATA"];

                Util.GridSetData(dgWoList, result, FrameOperation, true);

            } catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        //2020.03.12
        private void getKeypart(string sLotid)
        {
            try
            {
                iKeypart = 0;

                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("LOTID", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["LOTID"] = sLotid;
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_TB_SFC_WIP_INPUT_MTRL_HIST", "RQSTDT", "RSLTDT", RQSTDT);

                iKeypart = dtResult.Rows.Count;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        //2020.03.19
        private void setWip(string sLOTID)
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
                RQSTDT.Columns.Add("LOTID", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["PROCID"] = null;
                //2020.05.13
                //dr["EQSGID"] = Util.NVC(cboEquipmentSegment.SelectedValue) == "" ? null : Util.NVC(cboEquipmentSegment.SelectedValue);
                dr["EQSGID"] = null;
                dr["PROCTYPE"] = Process_Type.SCRAP;
                dr["UPDDTTM_FROM"] = dtpDateFrom.SelectedDateTime.ToString("yyyyMMdd");
                dr["UPDDTTM_TO"] = dtpDateTo.SelectedDateTime.AddDays(1).ToString("yyyyMMdd");
                dr["LOTID"] = sLOTID;
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_WIP_PACK_REWORK", "RQSTDT", "RSLTDT", RQSTDT);

                sPRDT_CLSS_CODE = Util.NVC(dtResult.Rows[0]["PRDT_CLSS_CODE"]);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        #endregion

        private void btnScrapLotCancel_Click(object sender, RoutedEventArgs e)
        {
            Refresh();
        }
    }
}
