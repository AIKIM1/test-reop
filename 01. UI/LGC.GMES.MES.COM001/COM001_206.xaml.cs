/*************************************************************************************
 Created Date : 2017.12.11
      Creator : 오화백
   Decription : 불량 Cell 등록
--------------------------------------------------------------------------------------
 [Change History]
  2017.12.11  오화백 : Initial Created.
 
**************************************************************************************/

using C1.WPF;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.CMM001.Popup;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using C1.WPF.DataGrid;
using LGC.GMES.MES.CMM001.Extensions;
using System.Threading;
using System.Windows.Threading;
using LGC.GMES.MES.CMM001;

namespace LGC.GMES.MES.COM001
{
    public partial class COM001_206 : UserControl, IWorkArea
    {
        #region Declaration & Constructor 

        private Util _Util = new Util();
       
        string _AREAID = string.Empty;
        string _PROCID = string.Empty;
        string _EQPTID = string.Empty;
        string _LOTID = string.Empty;
        string _WIPSEQ = string.Empty;
        string _DFCT_PLLT_LOTID = string.Empty;
        string _RESNCODE = string.Empty;
        string _DFCT_CHK = string.Empty;

        public COM001_206()
        {
            InitializeComponent();
            InitCombo();
            this.Loaded += UserControl_Loaded;
        }


        public IFrameOperation FrameOperation
        {
            get;
            set;
        }


        #endregion

        #region Initialize

        /// <summary>
        /// 화면내 combo 셋팅
        /// </summary>
        private void InitCombo()
        {
            CommonCombo _combo = new CommonCombo();

            #region 불량 Cell 등록

            //동
            C1ComboBox[] cboAreaChild = { cboEquipmentSegment };
            _combo.SetCombo(cboArea, CommonCombo.ComboStatus.SELECT, cbChild: cboAreaChild, sCase: "AREA");

            //라인
            C1ComboBox[] cboEquipmentSegmentParent = { cboArea };
            C1ComboBox[] cboEquipmentSegmentChild = { cboProcess };
            _combo.SetCombo(cboEquipmentSegment, CommonCombo.ComboStatus.SELECT, sCase: "EQUIPMENTSEGMENT_FORM", cbChild: cboEquipmentSegmentChild, cbParent: cboEquipmentSegmentParent);

            //공정
            C1ComboBox[] cboProcessParent = { cboEquipmentSegment };
            C1ComboBox[] cboProcessChild = { cboEquipment };
            String[] sFilterProcess = { "SEARCH", string.Empty, string.Empty, string.Empty };
            _combo.SetCombo(cboProcess, CommonCombo.ComboStatus.SELECT, sFilter: sFilterProcess, cbParent: cboProcessParent, sCase: "PROCESS_SORT", cbChild: cboProcessChild);


            //설비
            C1ComboBox[] cboEquipmentParent = { cboEquipmentSegment, cboProcess };
            _combo.SetCombo(cboEquipment, CommonCombo.ComboStatus.ALL, cbParent: cboEquipmentParent);



            #endregion
        }


        #endregion

        #region Event
        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
             //사용자 권한별로 버튼 숨기기
            List<Button> listAuth = new List<Button>();
            listAuth.Add(btnSave);
            listAuth.Add(btnDelete);
            Util.pageAuth(listAuth, FrameOperation.AUTHORITY);
            //여기까지 사용자 권한별로 버튼 숨기기
            cTabHistory.Visibility = Visibility.Collapsed;
            this.Loaded -= UserControl_Loaded;
        }

        #region 불량 Cell등록
        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            ClearValue();
            GetLotList();
        }

        private void dgProductLotChoice_Checked(object sender, RoutedEventArgs e)
        {
            dgLotList.Selection.Clear();

            RadioButton rb = sender as RadioButton;

            //최초 체크시에만 로직 타도록 구현
            if (DataTableConverter.GetValue(rb.DataContext, "CHK").Equals(0))
            {
                //체크시 처리될 로직
                string sLotId = DataTableConverter.GetValue(rb.DataContext, "LOTID").ToString();
                int iWipSeq = Convert.ToInt16(DataTableConverter.GetValue(rb.DataContext, "WIPSEQ"));

                //선택값 셋팅
                foreach (C1.WPF.DataGrid.DataGridRow row in ((C1.WPF.DataGrid.DataGridCellPresenter)rb.Parent).DataGrid.Rows)
                {
                    DataTableConverter.SetValue(row.DataItem, "CHK", 0);
                }

                DataTableConverter.SetValue(rb.DataContext, "CHK", 1);
                //row 색 바꾸기
                ((C1.WPF.DataGrid.DataGridCellPresenter)rb.Parent).DataGrid.SelectedIndex = ((C1.WPF.DataGrid.DataGridCellPresenter)rb.Parent).Cell.Row.Index;

                ClearValue();
                SetValue(rb.DataContext);
                SelectInputDefectList();
            }
        }

        private void dgDefectChoice_Checked(object sender, RoutedEventArgs e)
        {
            dgDefect.Selection.Clear();

            RadioButton rb = sender as RadioButton;

            //최초 체크시에만 로직 타도록 구현
            if (DataTableConverter.GetValue(rb.DataContext, "CHK").Equals(0))
            {
                //선택값 셋팅
                foreach (C1.WPF.DataGrid.DataGridRow row in ((C1.WPF.DataGrid.DataGridCellPresenter)rb.Parent).DataGrid.Rows)
                {
                    DataTableConverter.SetValue(row.DataItem, "CHK", 0);
                }

                DataTableConverter.SetValue(rb.DataContext, "CHK", 1);
                //row 색 바꾸기
                ((C1.WPF.DataGrid.DataGridCellPresenter)rb.Parent).DataGrid.SelectedIndex = ((C1.WPF.DataGrid.DataGridCellPresenter)rb.Parent).Cell.Row.Index;

                CellClearValue();
                CellSetValue(rb.DataContext);
                SelectInputCellList();
                _DFCT_CHK = "Y";
            }
        }

        //private void btnShift_Click(object sender, RoutedEventArgs e)
        //{

        //    if (_EQPTID == string.Empty) return;

        //    CMM_SHIFT_USER2 popupShiftUser = new CMM_SHIFT_USER2 { FrameOperation = this.FrameOperation };

        //    object[] parameters = new object[8];
        //    parameters[0] = LoginInfo.CFG_SHOP_ID;
        //    parameters[1] = LoginInfo.CFG_AREA_ID;
        //    parameters[2] = LoginInfo.CFG_EQSG_ID;
        //    parameters[3] = _PROCID;
        //    parameters[4] = Util.NVC(txtShift.Tag);
        //    parameters[5] = Util.NVC(txtWorker.Tag);
        //    parameters[6] = _EQPTID;
        //    parameters[7] = "Y"; // 저장 Flag "Y" 일때만 저장.
        //    C1WindowExtension.SetParameters(popupShiftUser, parameters);

        //    popupShiftUser.Closed += new EventHandler(popupShiftUser_Closed);
        //    grdMain.Children.Add(popupShiftUser);
        //    popupShiftUser.BringToFront();
        //}

        //private void popupShiftUser_Closed(object sender, EventArgs e)
        //{
        //    CMM_SHIFT_USER2 popup = sender as CMM_SHIFT_USER2;
        //    if (popup != null && popup.DialogResult == MessageBoxResult.OK)
        //    {
        //        GetEqptWrkInfo();
        //    }
        //    this.grdMain.Children.Remove(popup);


        //}
        
        private void txtCell_KeyDown(object sender, KeyEventArgs e)
        {
            try
            {

                
                if (e.Key == Key.Enter)
                {

                    if (!CellValidation())
                    {
                        return;
                    }

                    DataTable dtSource = DataTableConverter.Convert(dgCell.ItemsSource);

                    DataRow dr = dtSource.NewRow();
                    dr["CHK"] = 1;
                    dr["SUBLOTID"] = txtCell.Text.Trim();
                    dr["DATACHEKC"] = string.Empty;
                    dtSource.Rows.Add(dr);
                    Util.gridClear(dgCell);
                    Util.GridSetData(dgCell, dtSource, FrameOperation, false);
                    txtCell.Text = string.Empty;
                }
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }

        private void dgCell_LoadedCellPresenter(object sender, DataGridCellEventArgs e)
        {
            dgCell.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (e.Cell.Presenter == null)
                {
                    return;
                }

                if (e.Cell.Row.Type == DataGridRowType.Item)
                {
                    if (DataTableConverter.GetValue(e.Cell.Row.DataItem, "DATACHEKC").ToString() == "Y")
                    {
                        e.Cell.Presenter.Background = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#E6F5FB"));
                    }
                    else
                    {
                        e.Cell.Presenter.Background = new SolidColorBrush(Colors.White);
                    }

                }
            }));
        }

        private void dgCell_UnloadedCellPresenter(object sender, DataGridCellEventArgs e)
        {
            if (sender == null)
                return;

            C1DataGrid dataGrid = sender as C1DataGrid;

            dataGrid?.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (e.Cell.Presenter != null)
                {
                    if (e.Cell.Row.Type == DataGridRowType.Item)
                    {
                        var convertFromString = System.Windows.Media.ColorConverter.ConvertFromString("#FF000000");
                        if (convertFromString != null)
                            e.Cell.Presenter.Foreground = new SolidColorBrush((Color)convertFromString);
                    }
                }
            }));
        }
        
        private void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!MinusCellValidation())
                {
                    return;
                }

                //삭제하시겠습니까?
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(
                        "SFU1230"), null, "Information", MessageBoxButton.OKCancel, MessageBoxIcon.None, (result) =>
                        {
                            if (result == MessageBoxResult.OK)
                            {
                                CellDelete();
                            }
                        });
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!SaveCellValidation())
                {
                    return;
                }

                //저장하시겠습니까?
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(
                        "SFU1241"), null, "Information", MessageBoxButton.OKCancel, MessageBoxIcon.None, (result) =>
                        {
                            if (result == MessageBoxResult.OK)
                            {
                                CellSave();
                            }
                        });
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }

        #endregion

        #region 불량 Cell 등록 현황


        #endregion

        #endregion

        #region Mehod

        #region 불량 Cell 등록
        public void GetLotList()
        {
            try
            {
                ShowLoadingIndicator();
                DoEvents();

                DataTable dtRqst = new DataTable();

                dtRqst.Columns.Add("FROM_DATE", typeof(string));
                dtRqst.Columns.Add("TO_DATE", typeof(string));
                dtRqst.Columns.Add("LANGID", typeof(string));
                dtRqst.Columns.Add("EQSGID", typeof(string));
                dtRqst.Columns.Add("PROCID", typeof(string));
                dtRqst.Columns.Add("EQPTID", typeof(string));
                dtRqst.Columns.Add("LOTID_RT", typeof(string));

                DataRow dr = dtRqst.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["EQSGID"] = Util.GetCondition(cboEquipmentSegment, MessageDic.Instance.GetMessage("SFU1223"));
                if (dr["EQSGID"].Equals("")) return;
                dr["PROCID"] = Util.GetCondition(cboProcess, MessageDic.Instance.GetMessage("SFU1459"));
                if (dr["PROCID"].Equals("")) return;
                dr["EQPTID"] = Util.GetCondition(cboEquipment, bAllNull: true);
                dr["LOTID_RT"] = txtLotRTD.Text;
                dr["FROM_DATE"] = Util.GetCondition(ldpDateFrom);
                dr["TO_DATE"] = Util.GetCondition(ldpDateTo);
                
                dtRqst.Rows.Add(dr);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_PRODUCTLOT_RESULT", "INDATA", "OUTDATA", dtRqst);
               
               if (dtRslt.Rows.Count > 0)
               {
                   ClearValue();
                   Util.GridSetData(dgLotList, dtRslt, FrameOperation, false);
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

        private void ClearValue()
        {
            Util.gridClear(dgDefect);
            Util.gridClear(dgCell);
            txtCell.Text = string.Empty;
            _AREAID = string.Empty;
            _PROCID = string.Empty;
            _EQPTID = string.Empty;
            _LOTID = string.Empty;
            _WIPSEQ = string.Empty;
            _DFCT_PLLT_LOTID = string.Empty;

        }

        private void SetValue(object oContext)
        {
            _WIPSEQ = Util.NVC(DataTableConverter.GetValue(oContext, "WIPSEQ"));
            _PROCID = Util.NVC(DataTableConverter.GetValue(oContext, "PROCID"));
            _EQPTID = Util.NVC(DataTableConverter.GetValue(oContext, "EQPTID"));
            _LOTID = Util.NVC(DataTableConverter.GetValue(oContext, "LOTID"));
            _AREAID = Util.NVC(DataTableConverter.GetValue(oContext, "AREAID"));
        }

        private void CellSetValue(object oContext)
        {

            _DFCT_PLLT_LOTID = Util.NVC(DataTableConverter.GetValue(oContext, "DFCT_PLLT_LOTID"));
            _RESNCODE = Util.NVC(DataTableConverter.GetValue(oContext, "RESNCODE"));
        }
        
        public void SelectInputDefectList()
        {
            try
            {
                DataTable inTable = new DataTable();
                inTable.Columns.Add("LANGID", typeof(string));
                inTable.Columns.Add("PROCID", typeof(string));
                inTable.Columns.Add("AREAID", typeof(string));
                inTable.Columns.Add("EQPTID", typeof(string));
                inTable.Columns.Add("LOTID", typeof(string));
                inTable.Columns.Add("WIPSEQ", typeof(string));

                DataRow newRow = inTable.NewRow();
                newRow["LANGID"] = LoginInfo.LANGID;
                newRow["PROCID"] = _PROCID;
                newRow["AREAID"] = _AREAID;
                newRow["EQPTID"] = _EQPTID;
                newRow["LOTID"]  = _LOTID;
                newRow["WIPSEQ"] = _WIPSEQ;
                inTable.Rows.Add(newRow);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_QCA_SEL_WIPRESONCOLLECT_INFO_FORMATION_POLYMER", "INDATA", "OUTDATA", inTable);
                Util.GridSetData(dgDefect, dtResult, FrameOperation, false);
          
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }

        }

        public void SelectInputCellList()
        {
            try
            {
                DataTable inTable = new DataTable();
                inTable.Columns.Add("DFCT_PLLT_LOTID", typeof(string));
              
                DataRow newRow = inTable.NewRow();
                newRow["DFCT_PLLT_LOTID"] = _DFCT_PLLT_LOTID;
                inTable.Rows.Add(newRow);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_POLYMER_CELL_CHK", "INDATA", "OUTDATA", inTable);
                Util.GridSetData(dgCell, dtResult, FrameOperation, false);

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }

        }

        private void CellClearValue()
        {
          
            Util.gridClear(dgCell);
            txtCell.Text = string.Empty;
             _DFCT_PLLT_LOTID = string.Empty;
            _RESNCODE = string.Empty;

        }


        private bool CellValidation()
        {

            if (txtCell.Text == string.Empty)
            {
                return false;
            }
           if (_DFCT_CHK == string.Empty)
            {

                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("선택된 불량 정보가 없습니다."), null, "Information", MessageBoxButton.OK, MessageBoxIcon.None, (result) =>
                {
                    if (result == MessageBoxResult.OK)
                    {
                        txtCell.Text = string.Empty;
                        txtCell.Focus();
                    }
                });
               
                return false;
            }

            if (txtCell.Text.Trim().Length != 10)
            {

                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("Cell ID는 10자리 입니다."), null, "Information", MessageBoxButton.OK, MessageBoxIcon.None, (result) =>
                {
                    if (result == MessageBoxResult.OK)
                    {
                        //txtCell.Text = string.Empty;
                        txtCell.Focus();
                    }
                });
                return false;
            }


            if (DefectCellChk() != string.Empty)
            {

                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("불량정보가 등록된 Cell 정보입니다."), null, "Information", MessageBoxButton.OK, MessageBoxIcon.None, (result) =>
                {
                    if (result == MessageBoxResult.OK)
                    {
                        txtCell.Text = string.Empty;
                        txtCell.Focus();
                    }
                });
                return false;
            }

            int dupChk = 0;
            for(int i=0; i<dgCell.Rows.Count; i++)
            {
                if(txtCell.Text.Trim() == DataTableConverter.GetValue(dgCell.Rows[0].DataItem, "SUBLOTID").ToString())
                {
                    dupChk = dupChk + 1;
                }
            }
            if(dupChk > 0 )
            {

                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("중복된 Cell 정보가 존재합니다."), null, "Information", MessageBoxButton.OK, MessageBoxIcon.None, (result) =>
                {
                    if (result == MessageBoxResult.OK)
                    {

                        for (int i = 0; i < dgCell.GetRowCount(); i++)
                        {
                            if (DataTableConverter.GetValue(dgCell.Rows[i].DataItem, "SUBLOTID").ToString() == txtCell.Text.Trim())
                            {
                                dgCell.ScrollIntoView(i, dgCell.Columns["SUBLOTID"].Index);
                                dgCell.SelectedIndex = i;
                            }
                        }
                        txtCell.Text = string.Empty;
                        txtCell.Focus();
                    }
                });
          
                return false;
            }
          
            return true;
        }
      
        private string DefectCellChk()
        {
            string ReturnValue = string.Empty;

            try
            {
                DataTable inTable = new DataTable();
                inTable.Columns.Add("DFCT_PLLT_LOTID", typeof(string));
                inTable.Columns.Add("SUBLOTID", typeof(string));

                DataRow newRow = inTable.NewRow();
                newRow["DFCT_PLLT_LOTID"] = _DFCT_PLLT_LOTID;
                newRow["SUBLOTID"] = txtCell.Text.Trim();
                inTable.Rows.Add(newRow);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_POLYMER_CELL_CHK", "INDATA", "OUTDATA", inTable);

                if (dtResult.Rows.Count > 0)
                {
                    ReturnValue = dtResult.Rows[0]["SUBLOTID"].ToString();
                }

            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }


            return ReturnValue;

        }
     
        #region 작업조, 작업자
        //private void GetEqptWrkInfo()
        //{
        //    try
        //    {
        //        const string bizRuleName = "DA_BAS_SEL_TB_SFC_EQPT_WRK_INFO";

        //        DataTable inTable = new DataTable("RQSTDT");
        //        inTable.Columns.Add("LANGID", typeof(string));
        //        inTable.Columns.Add("EQPTID", typeof(string));
        //        inTable.Columns.Add("SHOPID", typeof(string));
        //        inTable.Columns.Add("AREAID", typeof(string));
        //        inTable.Columns.Add("EQSGID", typeof(string));
        //        inTable.Columns.Add("PROCID", typeof(string));

        //        DataRow newRow = inTable.NewRow();
        //        newRow["LANGID"] = LoginInfo.LANGID;
        //        newRow["EQPTID"] = _EQPTID;
        //        newRow["SHOPID"] = LoginInfo.CFG_SHOP_ID;
        //        newRow["AREAID"] = LoginInfo.CFG_AREA_ID;
        //        newRow["EQSGID"] = LoginInfo.CFG_EQSG_ID;
        //        newRow["PROCID"] = _PROCID;

        //        inTable.Rows.Add(newRow);

        //        new ClientProxy().ExecuteService(bizRuleName, "RQSTDT", "RSLTDT", inTable, (result, searchException) =>
        //        {
        //            try
        //            {
        //                if (searchException != null)
        //                {
        //                    Util.MessageException(searchException);
        //                    return;
        //                }

        //                if (result.Rows.Count > 0)
        //                {
        //                    if (!result.Rows[0].ItemArray[0].ToString().Equals(""))
        //                    {
        //                        txtShiftStartTime.Text = Util.NVC(result.Rows[0]["WRK_STRT_DTTM"]);
        //                    }
        //                    else
        //                    {
        //                        txtShiftStartTime.Text = string.Empty;
        //                    }

        //                    if (!result.Rows[0].ItemArray[1].ToString().Equals(""))
        //                    {
        //                        txtShiftEndTime.Text = Util.NVC(result.Rows[0]["WRK_END_DTTM"]);
        //                    }
        //                    else
        //                    {
        //                        txtShiftEndTime.Text = string.Empty;
        //                    }

        //                    if (!string.IsNullOrEmpty(txtShiftStartTime.Text) && !string.IsNullOrEmpty(txtShiftEndTime.Text))
        //                    {
        //                        txtShiftDateTime.Text = txtShiftStartTime.Text + " ~ " + txtShiftEndTime.Text;
        //                    }
        //                    else
        //                    {
        //                        txtShiftDateTime.Text = string.Empty;
        //                    }

        //                    if (Util.NVC(result.Rows[0]["WRK_USERID"]).Equals(""))
        //                    {
        //                        txtWorker.Text = string.Empty;
        //                        txtWorker.Tag = string.Empty;
        //                    }
        //                    else
        //                    {
        //                        txtWorker.Text = Util.NVC(result.Rows[0]["WRK_USERNAME"]);
        //                        txtWorker.Tag = Util.NVC(result.Rows[0]["WRK_USERID"]);
        //                    }

        //                    if (Util.NVC(result.Rows[0]["SHFT_ID"]).Equals(""))
        //                    {
        //                        txtShift.Tag = string.Empty;
        //                        txtShift.Text = string.Empty;
        //                    }
        //                    else
        //                    {
        //                        txtShift.Text = Util.NVC(result.Rows[0]["SHFT_NAME"]);
        //                        txtShift.Tag = Util.NVC(result.Rows[0]["SHFT_ID"]);
        //                    }
        //                }
        //                else
        //                {
        //                    ClearShiftControl();
        //                }
        //            }
        //            catch (Exception ex)
        //            {
        //                Util.MessageException(ex);
        //            }
        //        });
        //    }
        //    catch (Exception ex)
        //    {
        //        Util.MessageException(ex);
        //    }
        //}
        //public void ClearShiftControl()
        //{
        //    txtWorker.Text = string.Empty;
        //    txtWorker.Tag = string.Empty;
        //    txtShift.Text = string.Empty;
        //    txtShift.Tag = string.Empty;
        //    txtShiftStartTime.Text = string.Empty;
        //    txtShiftEndTime.Text = string.Empty;
        //    txtShiftDateTime.Text = string.Empty;
        //}
        #endregion

        private void CellDelete()
        {
            DataSet inData = new DataSet();

            //마스터 정보
            DataTable inDataTable = inData.Tables.Add("INDATA");
            inDataTable.Columns.Add("USERID", typeof(string));
            DataRow row = null;

            row = inDataTable.NewRow();
            row["USERID"] = LoginInfo.USERID;
            inDataTable.Rows.Add(row);

            //삭제할 Cell 정보
            DataTable inSublot = inData.Tables.Add("INSUBLOT");
            inSublot.Columns.Add("SUBLOTID", typeof(string));


            for (int i = 0; i < dgCell.Rows.Count; i++)
            {
                if (Util.NVC(DataTableConverter.GetValue(dgCell.Rows[i].DataItem, "CHK")) == "1")
                {
                    row = inSublot.NewRow();
                    row["SUBLOTID"] = Util.NVC(DataTableConverter.GetValue(dgCell.Rows[i].DataItem, "SUBLOTID"));
                    inSublot.Rows.Add(row);
                }
            }
            try
            {
                new ClientProxy().ExecuteService_Multi("BR_ACT_REG_CANCEL_DEFECT_SUBLOT", "INDATA,INSUBLOT", null, (Result, ex) =>
                {
                    if (ex != null)
                    {
                        Util.MessageException(ex);
                        return;
                    }
                    LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("SFU1275"), null, "Information", MessageBoxButton.OK, MessageBoxIcon.None, (result) =>
                    {
                        SelectInputDefectList();

                        if (dgDefect.Rows.Count > 0)
                        {
                            int idx = _Util.GetDataGridRowIndex(dgDefect, "RESNCODE", _RESNCODE);

                            if (idx >= 0)
                            {
                                DataTableConverter.SetValue(dgDefect.Rows[idx].DataItem, "CHK", 1);
                                dgDefect.ScrollIntoView(idx, dgDefect.Columns["CHK"].Index);
                                dgDefect.SelectedIndex = idx;
                                SelectInputCellList();
                            }
                        }

                    });
                    return;

                }, inData);
            }
            catch (Exception ex)
            {
                Util.AlertByBiz("BR_ACT_REG_CANCEL_DEFECT_SUBLOT", ex.Message, ex.ToString());
            }
        }

        private bool MinusCellValidation()
        {
            DataRow[] drchk = DataTableConverter.Convert(dgCell.ItemsSource).Select("CHK = 1");

            if (drchk.Length == 0)
            {
                // 선택된 항목이 없습니다.
                Util.MessageValidation("SFU1651");
                return false;
            }
            for (int i = 0; i < dgCell.Rows.Count; i++)
            {
                if (Util.NVC(DataTableConverter.GetValue(dgCell.Rows[i].DataItem, "CHK")) == "1" && Util.NVC(DataTableConverter.GetValue(dgCell.Rows[i].DataItem, "DATACHEKC")) == string.Empty)
                {
                    Util.MessageValidation("SFU4215"); //저장되지 되지 않은 데이터 입니다.
                    return false;
                }
            }

            return true;
        }

        private bool SaveCellValidation()
        {
            DataRow[] drchk = DataTableConverter.Convert(dgCell.ItemsSource).Select("CHK = 1");

            if (drchk.Length == 0)
            {
                // 선택된 항목이 없습니다.
                Util.MessageValidation("SFU1651");
                return false;
            }
          
            for (int i = 0; i < dgCell.Rows.Count; i++)
            {
                if (Util.NVC(DataTableConverter.GetValue(dgCell.Rows[i].DataItem, "CHK")) == "1" && Util.NVC(DataTableConverter.GetValue(dgCell.Rows[i].DataItem, "DATACHEKC")) == "Y")
                {
                    Util.MessageValidation("SFU4216"); //이미 저장된 데이터입니다.
                    return false;
                }
               
            }

            return true;
        }

        private void CellSave()
        {
            DataSet inData = new DataSet();

            //마스터 정보
            DataTable inDataTable = inData.Tables.Add("INDATA");
            inDataTable.Columns.Add("LOTID", typeof(string));
            inDataTable.Columns.Add("USERID", typeof(string));
            DataRow row = null;

            row = inDataTable.NewRow();
            row["LOTID"] = _DFCT_PLLT_LOTID;
            row["USERID"] = LoginInfo.USERID;

            inDataTable.Rows.Add(row);

            //저장될 CELL정보
            DataTable inSublot = inData.Tables.Add("INSUBLOT");
            inSublot.Columns.Add("SUBLOTID", typeof(string));


            for (int i = 0; i < dgCell.Rows.Count; i++)
            {
                if (Util.NVC(DataTableConverter.GetValue(dgCell.Rows[i].DataItem, "CHK")) == "1")
                {
                    row = inSublot.NewRow();
                    row["SUBLOTID"] = Util.NVC(DataTableConverter.GetValue(dgCell.Rows[i].DataItem, "SUBLOTID"));
                    inSublot.Rows.Add(row);
                }
            }
            try
            {
                new ClientProxy().ExecuteService_Multi("BR_ACT_REG_DEFECT_SUBLOT", "INDATA,INSUBLOT", null, (Result, ex) =>
                {
                    if (ex != null)
                    {
                        Util.MessageException(ex);
                        return;
                    }
                    LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("SFU1275"), null, "Information", MessageBoxButton.OK, MessageBoxIcon.None, (result) =>
                    {
                        SelectInputDefectList();

                        if (dgDefect.Rows.Count > 0)
                        {
                            int idx = _Util.GetDataGridRowIndex(dgDefect, "RESNCODE", _RESNCODE);

                            if (idx >= 0)
                            {
                                DataTableConverter.SetValue(dgDefect.Rows[idx].DataItem, "CHK", 1);
                                dgDefect.ScrollIntoView(idx, dgDefect.Columns["CHK"].Index);
                                dgDefect.SelectedIndex = idx;
                                SelectInputCellList();
                            }
                        }
                    });
                    return;

                }, inData);
            }
            catch (Exception ex)
            {
                Util.AlertByBiz("BR_ACT_REG_DEFECT_SUBLOT", ex.Message, ex.ToString());
            }
        }

        #endregion

        #region 불량 Cell 등록 현황
        #endregion

        #endregion

       
      
    }
}
