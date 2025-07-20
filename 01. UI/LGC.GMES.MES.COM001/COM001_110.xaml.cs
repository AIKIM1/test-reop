/*************************************************************************************
 Created Date : 2017.09.18
      Creator : 
   Decription : 
--------------------------------------------------------------------------------------
 [Change History]
  2017.09.18  오화백: Initial Created.

   
**************************************************************************************/

using C1.WPF;
using C1.WPF.DataGrid;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.CMM001.Extensions;
using LGC.GMES.MES.CMM001.Popup;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;

namespace LGC.GMES.MES.COM001
{
    public partial class COM001_110 : UserControl, IWorkArea
    {
        #region Declaration & Constructor 

        private BizDataSet _Biz = new BizDataSet();
        Util _Util = new Util();

        public COM001_110()
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
            CommonCombo _combo = new CommonCombo();

            //동
            C1ComboBox[] cboAreaChild = { cboEquipmentSegment };
            _combo.SetCombo(cboArea, CommonCombo.ComboStatus.SELECT, cbChild: cboAreaChild);

            //라인
            C1ComboBox[] cboEquipmentSegmentParent = { cboArea };
            C1ComboBox[] cboEquipmentSegmentChild = { cboProcess, cboEquipment };
            _combo.SetCombo(cboEquipmentSegment, CommonCombo.ComboStatus.SELECT, sCase: "EQUIPMENTSEGMENT_FORM", cbChild: cboEquipmentSegmentChild, cbParent: cboEquipmentSegmentParent);

            //공정
            C1ComboBox[] cboProcessParent = { cboEquipmentSegment };
            C1ComboBox[] cboProcessChild = { cboEquipment };
            String[] sFilterProcess = { "SEARCH", string.Empty, string.Empty, string.Empty };
            _combo.SetCombo(cboProcess, CommonCombo.ComboStatus.SELECT, sFilter: sFilterProcess, cbChild: cboProcessChild, cbParent: cboProcessParent, sCase: "PROCESS_SORT");

            //설비
            C1ComboBox[] cboEquipmentParent = { cboEquipmentSegment, cboProcess };
            _combo.SetCombo(cboEquipment, CommonCombo.ComboStatus.ALL, cbParent: cboEquipmentParent);

            // 작업구분
            String[] sFilterElectype = { "", "FORM_WRK_TYPE_CODE" };
            _combo.SetCombo(cboJob, CommonCombo.ComboStatus.ALL, sFilter: sFilterElectype, sCase: "COMMCODES");

            //작업조
            String[] sFilterShift = { cboArea.SelectedValue.ToString(), cboEquipmentSegment.SelectedValue.ToString(), cboProcess.SelectedValue.ToString()};
            _combo.SetCombo(cboShift, CommonCombo.ComboStatus.ALL, sFilter: sFilterShift, sCase: "SHIFT");
          
        }

        #endregion

        #region Event

        #region [Form Load]
        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            //사용자 권한별로 버튼 숨기기
            List<Button> listAuth = new List<Button>();
            Util.pageAuth(listAuth, FrameOperation.AUTHORITY);
            //여기까지 사용자 권한별로 버튼 숨기기

            dtpDateTo.SelectedDateTime = DateTime.Now;
            dtpDateFrom.SelectedDateTime = DateTime.Now.AddDays(-7);
            this.Loaded -= UserControl_Loaded;
        }
        #endregion

        #region [조회]
        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            GetLotList();
        }
        #endregion
   
        #region [작업일] - 조회조건 ####### Visibility="Collapsed" #######
        private void dtpDate_SelectedDataTimeChanged(object sender, SelectionChangedEventArgs e)
        {

            if (dtpDateFrom.SelectedDateTime.Year > 1 && dtpDateTo.SelectedDateTime.Year > 1)
            {
                //if ((dtpDateTo.SelectedDateTime - dtpDateFrom.SelectedDateTime).TotalDays > 7)
                //{
                //    //Util.AlertInfo("SFU2042", new object[] { "7" });   //기간은 {0}일 이내 입니다.
                //    Util.MessageValidation("SFU2042", "7");

                //    dtpDateFrom.SelectedDateTime = dtpDateTo.SelectedDateTime.AddDays(-6);
                //    return;
                //}

                if ((dtpDateTo.SelectedDateTime - dtpDateFrom.SelectedDateTime).TotalDays < 0)
                {
                    dtpDateFrom.SelectedDateTime = dtpDateTo.SelectedDateTime;
                    return;
                }
            }
        }
        #endregion

        #region [리스트 그리드 : dgResult_LoadedCellPresenter, dgResult_MouseDoubleClick]
        private void dgResult_LoadedCellPresenter(object sender, DataGridCellEventArgs e)
        {
            dgResult.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (e.Cell.Presenter == null)
                {
                    return;
                }
                //link 색변경
                if (e.Cell.Column.Name.Equals("LOTID_RT"))
                {
                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Blue);
                }
                else
                {
                    e.Cell.Presenter.Foreground = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#FF333333"));
                }

            }));

        }

        private void dgResult_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            Point pnt = e.GetPosition(null);
            C1.WPF.DataGrid.DataGridCell cell = dgResult.GetCellFromPoint(pnt);

            if (cell != null)
            {
                // 등급별 합산 수량 표시
                GetGradeList(cell.Row.Index);
            }
        }

        private void dgResult_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            C1.WPF.DataGrid.C1DataGrid dg = sender as C1.WPF.DataGrid.C1DataGrid;

            if (dg.CurrentColumn.Name.Equals("LOTID_RT") && dg.GetRowCount() > 0 && dg.CurrentRow != null)
            {
                // 활성화 Pallet별 생산실적 조회로 이동
                loadingIndicator.Visibility = Visibility.Visible;

                object[] parameters = new object[10];
                parameters[0] = DataTableConverter.GetValue(dg.CurrentRow.DataItem, "CALDATE").GetString();
                parameters[1] = DataTableConverter.GetValue(dg.CurrentRow.DataItem, "AREAID").GetString();
                parameters[2] = DataTableConverter.GetValue(dg.CurrentRow.DataItem, "EQSGID").GetString();
                parameters[3] = DataTableConverter.GetValue(dg.CurrentRow.DataItem, "PROCID").GetString();
                parameters[4] = DataTableConverter.GetValue(dg.CurrentRow.DataItem, "EQPTID").GetString();
                parameters[5] = DataTableConverter.GetValue(dg.CurrentRow.DataItem, "PRJT_NAME").GetString();
                parameters[6] = DataTableConverter.GetValue(dg.CurrentRow.DataItem, "PRODID").GetString();
                parameters[7] = DataTableConverter.GetValue(dg.CurrentRow.DataItem, "LOTID_RT").GetString();
                parameters[8] = DataTableConverter.GetValue(dg.CurrentRow.DataItem, "SHIFT").GetString();
                parameters[9] = DataTableConverter.GetValue(dg.CurrentRow.DataItem, "MKT_TYPE_CODE").GetString();
                this.FrameOperation.OpenMenu("SFU010160340", true, parameters);

                loadingIndicator.Visibility = Visibility.Collapsed;
            }

        }

        #endregion


        #endregion

        #region Mehod

        #region [BizCall]

        /// <summary>
        /// 리스트 조회
        /// </summary>
        private void GetLotList()
        {
            try
            {
                if ((dtpDateTo.SelectedDateTime - dtpDateFrom.SelectedDateTime).TotalDays > 31)
                {
                    //Util.AlertInfo("SFU2042", new object[] { "7" });   //기간은 {0}일 이내 입니다.
                    Util.MessageValidation("SFU2042", "31");
                    return;
                }

                ShowLoadingIndicator();

                ClearValue();

                DataTable dtRqst = new DataTable();

                dtRqst.Columns.Add("LANGID", typeof(string));
                dtRqst.Columns.Add("FROM_DATE", typeof(string));
                dtRqst.Columns.Add("TO_DATE", typeof(string));
                dtRqst.Columns.Add("EQSGID", typeof(string));
                dtRqst.Columns.Add("PROCID", typeof(string));
                dtRqst.Columns.Add("EQPTID", typeof(string));
                dtRqst.Columns.Add("FORM_WRK_TYPE_CODE", typeof(string));
                dtRqst.Columns.Add("PRJT_NAME", typeof(string));
                dtRqst.Columns.Add("PRODID", typeof(string));
                dtRqst.Columns.Add("LOTID_RT", typeof(string));
                dtRqst.Columns.Add("SHIFT", typeof(string));

                DataRow dr = dtRqst.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["FROM_DATE"] = Util.GetCondition(dtpDateFrom);
                dr["TO_DATE"] = Util.GetCondition(dtpDateTo);

                //dr["EQSGID"] = Util.GetCondition(cboEquipmentSegment, "라인을선택하세요.");
                dr["EQSGID"] = Util.GetCondition(cboEquipmentSegment, MessageDic.Instance.GetMessage("SFU1223"));
                if (dr["EQSGID"].Equals(""))
                {
                    HiddenLoadingIndicator();
                    return;
                }
                //dr["PROCID"] = Util.GetCondition(cboProcess, "공정을선택하세요.");
                dr["PROCID"] = Util.GetCondition(cboProcess, MessageDic.Instance.GetMessage("SFU1459"));
                if (dr["PROCID"].Equals(""))
                {
                    HiddenLoadingIndicator();
                    return;
                }
                dr["EQPTID"] = Util.GetCondition(cboEquipment, bAllNull: true);
                dr["FORM_WRK_TYPE_CODE"] = Util.GetCondition(cboJob, bAllNull: true);
                dr["PRJT_NAME"] = txtPJT.Text;
                dr["PRODID"] = txtProd.Text;
                dr["LOTID_RT"] = txtLotRt.Text;
                dr["SHIFT"] = Util.GetCondition(cboShift, bAllNull: true);

                dtRqst.Rows.Add(dr);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_PALLET_RESULT_SUM", "INDATA", "OUTDATA", dtRqst);

                Util.GridSetData(dgResult, dtRslt, FrameOperation);
                dgResult.Columns["PRODUCT_QTY"].Width = new C1.WPF.DataGrid.DataGridLength(100);
                dgResult.Columns["GOOD_QTY"].Width = new C1.WPF.DataGrid.DataGridLength(100);
                dgResult.Columns["DEFECT_QTY"].Width = new C1.WPF.DataGrid.DataGridLength(100);
               HiddenLoadingIndicator();
            }
            catch (Exception ex)
            {
                HiddenLoadingIndicator();
                Util.MessageException(ex);
            }
        }

        /// <summary>
        /// 등급별 수량 합산
        /// </summary>
        private void GetGradeList(int row)
        {
            try
            {
                ShowLoadingIndicator();

                DataTable dtRqst = new DataTable();

                dtRqst.Columns.Add("LANGID", typeof(string));
                dtRqst.Columns.Add("FROM_DATE", typeof(string));
                dtRqst.Columns.Add("TO_DATE", typeof(string));
                dtRqst.Columns.Add("EQSGID", typeof(string));
                dtRqst.Columns.Add("PROCID", typeof(string));
                dtRqst.Columns.Add("EQPTID", typeof(string));
                dtRqst.Columns.Add("FORM_WRK_TYPE_CODE", typeof(string));
                dtRqst.Columns.Add("PRJT_NAME", typeof(string));
                dtRqst.Columns.Add("PRODID", typeof(string));
                dtRqst.Columns.Add("LOTID_RT", typeof(string));
                dtRqst.Columns.Add("SHIFT", typeof(string));
                dtRqst.Columns.Add("MKT_TYPE_CODE", typeof(string));

                DataRow dr = dtRqst.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["FROM_DATE"] = DataTableConverter.GetValue(dgResult.Rows[row].DataItem, "CALDATE").GetString();
                dr["TO_DATE"] = DataTableConverter.GetValue(dgResult.Rows[row].DataItem, "CALDATE").GetString();
                dr["EQSGID"] = DataTableConverter.GetValue(dgResult.Rows[row].DataItem, "EQSGID").GetString();
                dr["PROCID"] = DataTableConverter.GetValue(dgResult.Rows[row].DataItem, "PROCID").GetString();
                dr["EQPTID"] = DataTableConverter.GetValue(dgResult.Rows[row].DataItem, "EQPTID").GetString();
                dr["PRJT_NAME"] = DataTableConverter.GetValue(dgResult.Rows[row].DataItem, "PRJT_NAME").GetString();
                dr["PRODID"] = DataTableConverter.GetValue(dgResult.Rows[row].DataItem, "PRODID").GetString();
                dr["LOTID_RT"] = DataTableConverter.GetValue(dgResult.Rows[row].DataItem, "LOTID_RT").GetString();
                dr["SHIFT"] = DataTableConverter.GetValue(dgResult.Rows[row].DataItem, "SHIFT").GetString();
                dr["MKT_TYPE_CODE"] = DataTableConverter.GetValue(dgResult.Rows[row].DataItem, "MKT_TYPE_CODE").GetString();

                dtRqst.Rows.Add(dr);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_PALLET_RESULT_SUM_GRADE", "INDATA", "OUTDATA", dtRqst);

                Util.GridSetData(dgGrade, dtRslt, FrameOperation);

                HiddenLoadingIndicator();
            }
            catch (Exception ex)
            {
                HiddenLoadingIndicator();
                Util.MessageException(ex);
            }
        }

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

        private void ClearValue()
        {
            Util.gridClear(dgResult);
            Util.gridClear(dgGrade);
        }

        #endregion


        #endregion

    }
}
