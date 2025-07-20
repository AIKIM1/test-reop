/*************************************************************************************
 Created Date : 2017.03.07
      Creator : 
   Decription : 
--------------------------------------------------------------------------------------
 [Change History]
  2018.01.06  DEVELOPER : Initial Created.
 
**************************************************************************************/

using C1.WPF;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using System;
using System.Collections.Generic;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using C1.WPF.DataGrid;
using LGC.GMES.MES.CMM001;
using LGC.GMES.MES.CMM001.Extensions;
using System.Windows.Threading;

namespace LGC.GMES.MES.COM001
{
    public partial class COM001_212 : UserControl, IWorkArea
    {
        #region Declaration & Constructor 

        private BizDataSet _Biz = new BizDataSet();
        private readonly Util _util = new Util();

        // 자주검사 팝업 호출을 위한 파라메터 정의
        private string _selectedProcessCode = string.Empty;
        private string _selectedequipmentSegmentCode = string.Empty;
        private string _selectedequipmentCode = string.Empty;
        private string _selectedequipmentName = string.Empty;
        private string _selectedLotId = string.Empty;
        private string _selectedWipSeq = string.Empty;
        private string _selectedActDate = string.Empty;
        private string _selectedActTime = string.Empty;
        private string _selectedClassGroup = string.Empty;

        public COM001_212()
        {
            InitializeComponent();

            InitCombo();
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
            //동,라인,공정,설비 셋팅
            CommonCombo combo = new CommonCombo();

            //동
            C1ComboBox[] cboAreaChild = { cboEquipmentSegment };
            combo.SetCombo(cboArea, CommonCombo.ComboStatus.SELECT, cbChild: cboAreaChild);

            //라인
            C1ComboBox[] cboEquipmentSegmentParent = { cboArea };
            C1ComboBox[] cboEquipmentSegmentChild = { cboProcess };
            combo.SetCombo(cboEquipmentSegment, CommonCombo.ComboStatus.SELECT, sCase: "EQUIPMENTSEGMENT_FORM", cbChild: cboEquipmentSegmentChild, cbParent: cboEquipmentSegmentParent);

            //공정
            C1ComboBox[] cbProcessParent = { cboEquipmentSegment };
            String[] cbProcess = { "SEARCH", string.Empty, string.Empty, string.Empty };
            combo.SetCombo(cboProcess, CommonCombo.ComboStatus.NONE, sFilter: cbProcess, sCase: "PROCESS_SORT", cbParent: cbProcessParent);

        }
        #endregion

        #region Event
        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            //사용자 권한별로 버튼 숨기기
            List<Button> listAuth = new List<Button>();

            Util.pageAuth(listAuth, FrameOperation.AUTHORITY);
            //여기까지 사용자 권한별로 버튼 숨기기

            dtpDateTo.SelectedDateTime = DateTime.Now;
            dtpDateFrom.SelectedDateTime = DateTime.Now.AddDays(-7);

        }

        private void dtpDate_SelectedDataTimeChanged(object sender, SelectionChangedEventArgs e)
        {
            //if (dtpDateFrom.SelectedDateTime.Year > 1 && dtpDateTo.SelectedDateTime.Year > 1)
            //{
            //    if ((dtpDateTo.SelectedDateTime - dtpDateFrom.SelectedDateTime).TotalDays > 7)
            //    {
            //        Util.AlertInfo("SFU2042", new object[] { "7" });   //기간은 {0}일 이내 입니다.
            //        dtpDateFrom.SelectedDateTime = dtpDateTo.SelectedDateTime.AddDays(-6);
            //        return;
            //    }

            //    if ((dtpDateTo.SelectedDateTime - dtpDateFrom.SelectedDateTime).TotalDays < 0)
            //    {
            //        dtpDateFrom.SelectedDateTime = dtpDateTo.SelectedDateTime;
            //        return;
            //    }
            //}
        }

        private void cboProcess_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            if (Util.NVC(cboProcess.SelectedValue).Equals(Process.DEGASING))
            {
                tiQualityDeg.Header = Util.NVC(cboProcess.Text);
                tiQualityDeg.Visibility = Visibility.Visible;

                tiQuality.Visibility = Visibility.Collapsed;
            }
            else
            {
                tiQuality.Header = Util.NVC(cboProcess.Text);
                tiQuality.Visibility = Visibility.Visible;

                tiQualityDeg.Visibility = Visibility.Collapsed;
            }

        }

        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            ClearValue();

            if (Util.NVC(cboProcess.SelectedValue).Equals(Process.DEGASING))
            {
                GetResultDeg();
            }
            else
            {
                GetResult();
            }

        }
        private void btnUpdate_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidationInspection())
                return;

            try
            {
                CMM_FORM_QUALITY popupQualty = new CMM_FORM_QUALITY { FrameOperation = FrameOperation };
                //if (ValidationGridAdd(popupQualty.Name) == false)
                //    return;

                popupQualty.Update_YN = "Y";

                object[] parameters = new object[10];
                parameters[0] = _selectedProcessCode;
                parameters[1] = null;
                parameters[2] = _selectedequipmentSegmentCode;
                parameters[3] = _selectedequipmentCode;
                parameters[4] = _selectedequipmentName;
                parameters[5] = _selectedLotId;
                parameters[6] = _selectedWipSeq;
                parameters[7] = _selectedActDate;
                parameters[8] = _selectedActTime;
                parameters[9] = _selectedClassGroup;

                C1WindowExtension.SetParameters(popupQualty, parameters);

                popupQualty.Closed += popupQualty_Closed;
                grdMain.Children.Add(popupQualty);
                popupQualty.BringToFront();

            }
            catch (Exception exception)
            {
                Util.MessageException(exception);
            }
        }

        private void popupQualty_Closed(object sender, EventArgs e)
        {
            CMM_FORM_QUALITY popup = sender as CMM_FORM_QUALITY;
            if (popup != null && popup.DialogResult == MessageBoxResult.OK)
            {
                btnSearch_Click(btnSearch, null);
            }

            grdMain.Children.Remove(popup);
        }

        private void dgQuality_SelectionChanged(object sender, DataGridSelectionChangedEventArgs e)
        {
            try
            {
                if (dgQuality.SelectedItem != null)
                {
                    _selectedProcessCode = DataTableConverter.GetValue(dgQuality.SelectedItem, "PROCID").GetString();
                    _selectedequipmentSegmentCode = DataTableConverter.GetValue(dgQuality.SelectedItem, "EQSGID").GetString();
                    _selectedequipmentCode = DataTableConverter.GetValue(dgQuality.SelectedItem, "EQPTID").GetString();
                    _selectedequipmentName = DataTableConverter.GetValue(dgQuality.SelectedItem, "EQPTNAME").GetString();
                    _selectedLotId = DataTableConverter.GetValue(dgQuality.SelectedItem, "LOTID").GetString();
                    _selectedWipSeq = DataTableConverter.GetValue(dgQuality.SelectedItem, "WIPSEQ").GetString();
                    _selectedActDate = DataTableConverter.GetValue(dgQuality.SelectedItem, "ACTDATE").GetString();
                    _selectedActTime = DataTableConverter.GetValue(dgQuality.SelectedItem, "ACTTIME").GetString();
                    _selectedClassGroup = DataTableConverter.GetValue(dgQuality.SelectedItem, "CLCTITEM_CLSS_GROUP").GetString();
                }
                else
                {
                    _selectedProcessCode = string.Empty;
                    _selectedequipmentSegmentCode = string.Empty;
                    _selectedequipmentCode = string.Empty;
                    _selectedequipmentName = string.Empty;
                    _selectedLotId = string.Empty;
                    _selectedWipSeq = string.Empty;
                    _selectedActDate = string.Empty;
                    _selectedActTime = string.Empty;
                    _selectedClassGroup = string.Empty;
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void dgQualityDeg_SelectionChanged(object sender, DataGridSelectionChangedEventArgs e)
        {
            try
            {
                if (dgQualityDeg.SelectedItem != null)
                {
                    _selectedProcessCode = DataTableConverter.GetValue(dgQualityDeg.SelectedItem, "PROCID").GetString();
                    _selectedequipmentSegmentCode = DataTableConverter.GetValue(dgQualityDeg.SelectedItem, "EQSGID").GetString();
                    _selectedequipmentCode = DataTableConverter.GetValue(dgQualityDeg.SelectedItem, "EQPTID").GetString();
                    _selectedequipmentName = DataTableConverter.GetValue(dgQualityDeg.SelectedItem, "EQPTNAME").GetString();
                    _selectedLotId = DataTableConverter.GetValue(dgQualityDeg.SelectedItem, "LOTID").GetString();
                    _selectedWipSeq = DataTableConverter.GetValue(dgQualityDeg.SelectedItem, "WIPSEQ").GetString();
                    _selectedActDate = DataTableConverter.GetValue(dgQualityDeg.SelectedItem, "ACTDATE").GetString();
                    _selectedActTime = DataTableConverter.GetValue(dgQualityDeg.SelectedItem, "ACTTIME").GetString();
                    _selectedClassGroup = DataTableConverter.GetValue(dgQualityDeg.SelectedItem, "CLCTITEM_CLSS_GROUP").GetString();
                }
                else
                {
                    _selectedProcessCode = string.Empty;
                    _selectedequipmentSegmentCode = string.Empty;
                    _selectedequipmentCode = string.Empty;
                    _selectedequipmentName = string.Empty;
                    _selectedLotId = string.Empty;
                    _selectedWipSeq = string.Empty;
                    _selectedActDate = string.Empty;
                    _selectedActTime = string.Empty;
                    _selectedClassGroup = string.Empty;
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        #endregion

        #region Mehod
        public void GetResult()
        {
            try
            {
                ShowLoadingIndicator();
                const string bizRuleName = "DA_QCA_SEL_SELF_INSP_LIST_POUCH";

                DataTable dtRqst = new DataTable();
                dtRqst.Columns.Add("LANGID", typeof(string));
                dtRqst.Columns.Add("DATE_FR", typeof(string));
                dtRqst.Columns.Add("DATE_TO", typeof(string));
                dtRqst.Columns.Add("AREAID", typeof(string));
                dtRqst.Columns.Add("EQSGID", typeof(string));
                dtRqst.Columns.Add("PROCID", typeof(string));
                dtRqst.Columns.Add("LOTID", typeof(string));

                DataRow dr = dtRqst.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["DATE_FR"] = Util.GetCondition(dtpDateFrom);
                dr["DATE_TO"] = Util.GetCondition(dtpDateTo);
                dr["AREAID"] = Util.GetCondition(cboArea, bAllNull: true);
                dr["EQSGID"] = Util.GetCondition(cboEquipmentSegment, bAllNull: true);
                dr["PROCID"] = Util.GetCondition(cboProcess, bAllNull: true);
                dr["LOTID"] = Util.GetCondition(txtLot.Text, bAllNull: true);

                dtRqst.Rows.Add(dr);
                //DataSet ds = new DataSet();
                //ds.Tables.Add(dtRqst);
                //string xml = ds.GetXml();

                //DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_QCA_SEL_SELF_INSP_LIST_POUCH", "INDATA", "OUTDATA", dtRqst);
                //Util.GridSetData(dgQuality, dtRslt, FrameOperation);

                new ClientProxy().ExecuteService(bizRuleName, "RQSTDT", "RSLTDT", dtRqst, (bizResult, bizException) =>
                {
                    HiddenLoadingIndicator();

                    if (bizException != null)
                    {
                        Util.MessageException(bizException);
                        return;
                    }
                    Util.GridSetData(dgQuality, bizResult, FrameOperation);
                });
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void GetResultDeg()
        {
            try
            {
                ShowLoadingIndicator();
                DoEvents();

                DataTable dtRqst = new DataTable();
                dtRqst.Columns.Add("LANGID", typeof(string));
                dtRqst.Columns.Add("DATE_FR", typeof(string));
                dtRqst.Columns.Add("DATE_TO", typeof(string));
                dtRqst.Columns.Add("AREAID", typeof(string));
                dtRqst.Columns.Add("EQSGID", typeof(string));
                dtRqst.Columns.Add("PROCID", typeof(string));
                dtRqst.Columns.Add("LOTID", typeof(string));

                DataRow dr = dtRqst.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["DATE_FR"] = Util.GetCondition(dtpDateFrom);
                dr["DATE_TO"] = Util.GetCondition(dtpDateTo);
                dr["AREAID"] = Util.GetCondition(cboArea, bAllNull: true);
                dr["EQSGID"] = Util.GetCondition(cboEquipmentSegment, bAllNull: true);
                dr["PROCID"] = Util.GetCondition(cboProcess, bAllNull: true);
                dr["LOTID"] = Util.GetCondition(txtLot.Text, bAllNull: true);
                dtRqst.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_QCA_SEL_SELF_INSP_LIST_POUCH_DEG", "RQSTDT", "RSLTDT", dtRqst);
                Util.GridSetData(dgQualityDeg, dtResult, null);

                if (dtResult == null || dtResult.Rows.Count == 0)
                {
                    return;
                }

                DataTable inTable = new DataTable();
                inTable.Columns.Add("LANGID", typeof(string));
                inTable.Columns.Add("PROCID", typeof(string));
                inTable.Columns.Add("EQPTID", typeof(string));
                inTable.Columns.Add("CLCTITEM_CLSS4", typeof(string));

                DataRow newRow = inTable.NewRow();
                newRow["LANGID"] = LoginInfo.LANGID;
                newRow["PROCID"] = Util.NVC(cboProcess.SelectedValue); 
                newRow["EQPTID"] = Util.NVC(dtResult.Rows[0]["EQPTID"]);
                inTable.Rows.Add(newRow);

                DataTable dtclctItem = new ClientProxy().ExecuteServiceSync("DA_QCA_SEL_SELF_INSP_DEG_CLCTITEM", "RQSTDT", "RSLTDT", inTable);

                if (dtclctItem == null || dtclctItem.Rows.Count == 0)
                {
                    return;
                }

                #region 그리드 header Setting

                DataTable dt = dtclctItem.DefaultView.ToTable(true, "CLCTITEM_CLSS_NAME3");

                // 검사 항목의 Max Column까지만 보이게
                int _maxColumn = Util.NVC_Int(dtclctItem.Rows[0]["COLUMN_COUNNT"]);

                int startcol = dgQualityDeg.Columns["EQPTID"].Index;
                int forCount = 0;

                for (int col = startcol + 1; col < dgQualityDeg.Columns.Count; col++)
                {
                    forCount++;

                    if (forCount > _maxColumn)
                    {
                        dgQualityDeg.Columns[col].Visibility = Visibility.Collapsed;
                    }
                    else
                    {
                        dgQualityDeg.Columns[col].Visibility = Visibility.Visible;
                        dgQualityDeg.Columns[col].Header = dt.Rows[forCount - 1]["CLCTITEM_CLSS_NAME3"].ToString();
                    }
                }

                #endregion

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

        #region [초기화]
        private void ClearValue()
        {
            if (Util.NVC(cboProcess.SelectedValue).Equals(Process.DEGASING))
            {
                Util.gridClear(dgQualityDeg);
            }
            else
            {
                Util.gridClear(dgQuality);
            }

            _selectedProcessCode = string.Empty;
            _selectedequipmentSegmentCode = string.Empty;
            _selectedequipmentCode = string.Empty;
            _selectedequipmentName = string.Empty;
            _selectedLotId = string.Empty;
            _selectedWipSeq = string.Empty;
            _selectedActTime = string.Empty;
            _selectedClassGroup = string.Empty;
        }

        private bool ValidationInspection()
        {
            if (string.IsNullOrEmpty(_selectedLotId) || string.IsNullOrEmpty(_selectedWipSeq))
            {
                Util.MessageValidation("SFU1636");
                return false;
            }

            return true;
        }

        private bool ValidationGridAdd(string popName)
        {
            foreach (UIElement ui in Util.FindVisualChildren<UIElement>(Application.Current.MainWindow))
            {
                if (((FrameworkElement)ui).Name.Equals(popName))
                {
                    // 프로그램이 이미 실행 중 입니다. 
                    Util.MessageValidation("SFU3193");
                    return false;
                }
            }

            return true;
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


        #endregion

        #endregion

    }
}
