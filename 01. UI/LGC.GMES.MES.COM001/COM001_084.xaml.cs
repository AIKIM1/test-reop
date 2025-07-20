/*************************************************************************************
 Created Date : 2017.06.14
      Creator : 
   Decription : 
--------------------------------------------------------------------------------------
 [Change History]
  2017.06.14  JMK : Initial Created.
  2021.07.15  김지은 : [GM JV Proj.]시험 생산 구분 코드 추가로 인한 수정
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
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;

namespace LGC.GMES.MES.COM001
{
    public partial class COM001_084 : UserControl, IWorkArea
    {
        #region Declaration & Constructor 

        private BizDataSet _Biz = new BizDataSet();
        public COM001_084()
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
        private void InitCombo()
        {
            //동,라인,공정,설비 셋팅
            CommonCombo _combo = new CommonCombo();

            //동
            C1ComboBox[] cboAreaChild = { cboEquipmentSegment };
            _combo.SetCombo(cboArea, CommonCombo.ComboStatus.NONE, cbChild: cboAreaChild);

            //라인
            C1ComboBox[] cboEquipmentSegmentParent = { cboArea };
            C1ComboBox[] cboEquipmentSegmentChild = { cboProcess };
            _combo.SetCombo(cboEquipmentSegment, CommonCombo.ComboStatus.SELECT, cbChild: cboEquipmentSegmentChild, cbParent: cboEquipmentSegmentParent);

            //공정
            C1ComboBox[] cboProcessParent = { cboEquipmentSegment };
            _combo.SetCombo(cboProcess, CommonCombo.ComboStatus.ALL, cbChild: null, cbParent: cboProcessParent);

            //생산구분
            string[] sFilter = new string[] { "PRODUCT_DIVISION" };
            _combo.SetCombo(cboProductDiv, CommonCombo.ComboStatus.ALL, sCase: "COMMCODE", sFilter: sFilter);

            // 생산구분 Default 정상생산
            if (cboProductDiv.Items.Count > 1)
                cboProductDiv.SelectedIndex = 1;

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

            dtpDateFrom.SelectedDataTimeChanged += dtpDate_SelectedDataTimeChanged;
            dtpDateTo.SelectedDataTimeChanged += dtpDate_SelectedDataTimeChanged;

        }
        #endregion

        #region [조회]
        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            GetLotList();
        }
        #endregion

        #region [작업일] - 조회 조건
        private void dtpDate_SelectedDataTimeChanged(object sender, SelectionChangedEventArgs e)
        {
            if (dtpDateFrom.SelectedDateTime.Year > 1 && dtpDateTo.SelectedDateTime.Year > 1)
            {
                LGCDatePicker LGCdp = sender as LGCDatePicker;

                // 조회 버튼 클릭시로 변경
                //if ((dtpDateTo.SelectedDateTime - dtpDateFrom.SelectedDateTime).TotalDays > 31)
                //{
                //    // 조회 기간 한달 To 일자 선택시 From은 해당월의 1일자로 변경
                //    //Util.AlertInfo("SFU2042", new object[] { "7" });   //기간은 {0}일 이내 입니다.
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

                //// To 일자 변경시 From일자 1일자로 변경
                //if (LGCdp.Name.Equals("dtpDateTo"))
                //{
                //    dtpDateFrom.SelectedDateTime = new DateTime(dtpDateTo.SelectedDateTime.Year, dtpDateTo.SelectedDateTime.Month, 1);
                //}

            }
        }
        #endregion

        #region [작업대상 목록에서 선택]
        private void dgProductLotChoice_Checked(object sender, RoutedEventArgs e)
        {
            dgLotList.Selection.Clear();

            RadioButton rb = sender as RadioButton;

            //최초 체크시에만 로직 타도록 구현
            if (DataTableConverter.GetValue(rb.DataContext, "CHK").Equals(0))
            {
                //체크시 처리될 로직
                foreach (C1.WPF.DataGrid.DataGridRow row in ((C1.WPF.DataGrid.DataGridCellPresenter)rb.Parent).DataGrid.Rows)
                {
                    DataTableConverter.SetValue(row.DataItem, "CHK", 0);
                }

                DataTableConverter.SetValue(rb.DataContext, "CHK", 1);

                //row 색 바꾸기
                ((C1.WPF.DataGrid.DataGridCellPresenter)rb.Parent).DataGrid.SelectedIndex = ((C1.WPF.DataGrid.DataGridCellPresenter)rb.Parent).Cell.Row.Index;

                GetSubLot(Util.NVC(DataTableConverter.GetValue(rb.DataContext, "PR_PROCID")).ToString(),
                          Util.NVC(DataTableConverter.GetValue(rb.DataContext, "NEXT_PROCID")).ToString(),
                          Util.NVC(DataTableConverter.GetValue(rb.DataContext, "PR_LOTID")).ToString(),
                          Util.NVC(DataTableConverter.GetValue(rb.DataContext, "MERGE_LOTID")).ToString(),
                          Util.NVC(DataTableConverter.GetValue(rb.DataContext, "NEXT_LOTID")).ToString()
                        );
            }
        }
        #endregion

        #endregion

        #region Mehod

        #region [BizCall]

        #region [### 작업대상 조회 ###]
        public void GetLotList()
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
                DoEvents();

                DataTable dtRqst = new DataTable();
                dtRqst.Columns.Add("LANGID", typeof(string));
                dtRqst.Columns.Add("AREAID", typeof(string));
                dtRqst.Columns.Add("EQSGID", typeof(string));
                dtRqst.Columns.Add("PROCID", typeof(string));
                dtRqst.Columns.Add("FROM_DATE", typeof(string));
                dtRqst.Columns.Add("TO_DATE", typeof(string));
                dtRqst.Columns.Add("PRODID", typeof(string));
                dtRqst.Columns.Add("PR_LOTID", typeof(string));
                dtRqst.Columns.Add("LOTID", typeof(string));
                //dtRqst.Columns.Add("NORMAL", typeof(string));
                //dtRqst.Columns.Add("PILOT", typeof(string));
                dtRqst.Columns.Add("PILOT_PROD_DIVS_CODE", typeof(string)); // 2021.07.15 : 시험 생산 구분 코드 추가로 인한 수정

                DataRow dr = dtRqst.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;

                dr["AREAID"] = Util.GetCondition(cboArea, MessageDic.Instance.GetMessage("SFU1499"));
                if (dr["AREAID"].Equals("")) return;

                ////dr["EQSGID"] = Util.GetCondition(cboEquipmentSegment, "라인을선택하세요.");
                dr["EQSGID"] = Util.GetCondition(cboEquipmentSegment, MessageDic.Instance.GetMessage("SFU1223"));
                if (dr["EQSGID"].Equals("")) return;

                if (!string.IsNullOrWhiteSpace(Util.GetCondition(cboProcess)))
                   dr["PROCID"] = Util.GetCondition(cboProcess);

                dr["FROM_DATE"] = Util.GetCondition(dtpDateFrom);
                dr["TO_DATE"] = Util.GetCondition(dtpDateTo);

                if (!string.IsNullOrWhiteSpace(txtProdid.Text))
                    dr["PRODID"] = txtProdid.Text;

                if (!string.IsNullOrWhiteSpace(txtprLotid.Text))
                    dr["PR_LOTID"] = txtprLotid.Text;

                if (!string.IsNullOrWhiteSpace(txtLotId.Text))
                    dr["LOTID"] = txtLotId.Text;

                // 2021.07.15 : 시험 생산 구분 코드 추가로 인한 수정
                //if (cboProductDiv.SelectedValue.ToString() == "P")
                //    dr["NORMAL"] = cboProductDiv.SelectedValue.ToString();
                //else if (cboProductDiv.SelectedValue.ToString() == "X")
                //    dr["PILOT"] = cboProductDiv.SelectedValue.ToString();
                dr["PILOT_PROD_DIVS_CODE"] = Util.GetCondition(cboProductDiv, bAllNull: true);

                dtRqst.Rows.Add(dr);

                Util.gridClear(dginLot);
                Util.gridClear(dgoutLot);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_LOT_LIST_ELTR_COMP", "INDATA", "OUTDATA", dtRqst);

                Util.GridSetData(dgLotList, dtRslt, FrameOperation, true);

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

        #region [### 투입, 완성LOT 조회 ###]
        private void GetSubLot(string sprProcid, string snxProcid, string sprLotid, string smgLotid, string snxLotid)
        {
            try
            {
                ShowLoadingIndicator();
                DoEvents();

                // 투입 Lot List
                DataTable dtRqst = new DataTable();
                dtRqst.Columns.Add("LANGID", typeof(string));
                dtRqst.Columns.Add("LOTID", typeof(string));
                dtRqst.Columns.Add("PROCID", typeof(string));

                DataRow dr = dtRqst.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;

                if (string.IsNullOrWhiteSpace(smgLotid))
                    dr["LOTID"] = sprLotid;
                else
                    dr["LOTID"] = smgLotid;

                dr["PROCID"] = sprProcid;

                dtRqst.Rows.Add(dr);

                DataTable dtRsltIn = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_LOT_LIST_ELTR_COMP_DETAIL", "INDATA", "OUTDATA", dtRqst);

                Util.GridSetData(dginLot, dtRsltIn, FrameOperation, true);

                //  완성 Lot List
                dtRqst.Clear();
                dr = dtRqst.NewRow();

                dr["LANGID"] = LoginInfo.LANGID;
                dr["LOTID"] = snxLotid;
                dr["PROCID"] = snxProcid;

                dtRqst.Rows.Add(dr);

                DataTable dtRsltOut = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_LOT_LIST_ELTR_COMP_DETAIL", "INDATA", "OUTDATA", dtRqst);

                Util.GridSetData(dgoutLot, dtRsltOut, FrameOperation, true);

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

        private void DoEvents()
        {
            Application.Current.Dispatcher.Invoke(DispatcherPriority.Background, new System.Threading.ThreadStart(delegate { }));
        }


        #endregion

        #endregion

    }
}