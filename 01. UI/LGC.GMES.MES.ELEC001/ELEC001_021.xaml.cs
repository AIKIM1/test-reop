/*************************************************************************************
 Created Date : 2016.12.06
      Creator : 이슬아
   Decription : 믹서원자재 Lot 추적
--------------------------------------------------------------------------------------
 [Change History]
  2016.12.06  이슬아 : 최초 생성
  2025.02.24  이민형 : 날짜 함수 Util.GetConfition 으로 형변환 함수 변경




 
**************************************************************************************/

using C1.WPF;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace LGC.GMES.MES.ELEC001
{
    public partial class ELEC001_021 : UserControl, IWorkArea
    {
        #region Declaration & Constructor 
        private const string _SearchBizRule = "DA_PRD_SEL_MIX_LOT_TRACE_FOR_TREEVIEW";
        private const string _SearchDetailBizRule = "BR_PRD_SEL_MIX_LOT_INFO";
        private const string _SearchDetailHistoryBizRule = "BR_PRD_SEL_MIX_LOT_HISTORY_INFO";
        private const string _SearchCboEquipemnt = "DA_PRD_SEL_EQPT_BY_PROC_CBO";

        private const string _SelectValue = "SELECT";
        Util _Util = new Util();

        /// <summary>
        /// Frame과 상호작용하기 위한 객체
        /// </summary>
        public IFrameOperation FrameOperation
        {
            get;
            set;
        }

        public ELEC001_021()
        {
            InitializeComponent();     
        }

        #endregion

        #region Initialize
        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            ApplyPermissions();
            InitCombo();
            InitControl();
            SetEvent();
        }        

        #endregion

        #region Event

        #region 조회버튼 클릭시 조회 + loadingindicator
        /// <summary>
        /// 조회버튼 클릭 이벤트 전
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnSearch_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            loadingIndicator.Visibility = Visibility.Visible;
        }

        /// <summary>
        /// 조회버튼 클릭 이벤트
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            SearchData();
        }
        #endregion
        void treeViewItem_Click(object sender, SourcedEventArgs e)
        {
            C1TreeViewItem item = sender as C1TreeViewItem;
            DataSet dsRslt = new DataSet();
            bool bVisibility;

            if (item == null) return;

            if (item.IsExpanded)
            {
                item.Collapse();
            }
            else
            {
                item.Expand();
            }

            if (item.Parent == null || item.Parent.Parent == null)
            {
                bVisibility = true;
                string strLotId = item.Parent != null ? item.Parent.Header.ToString() : item.Header.ToString();

                DataSet inData = new DataSet();
                DataTable dtRqst = inData.Tables.Add("INDATA");

                dtRqst.Columns.Add("LANGID", typeof(string));
                dtRqst.Columns.Add("LOTID", typeof(string));
                dtRqst.Columns.Add("PROCID", typeof(string));

                DataRow dr = dtRqst.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["LOTID"] = strLotId;
                dr["PROCID"] = cboMixProcess.SelectedValue;

                dtRqst.Rows.Add(dr);
                dsRslt = new ClientProxy().ExecuteServiceSync_Multi(_SearchDetailBizRule, "INDATA", "MATERIALINFO,LOTINFO", inData);
                dgMBomInfo.ItemsSource = DataTableConverter.Convert(dsRslt.Tables["MATERIALINFO"]);
                dgLotInfo.ItemsSource = DataTableConverter.Convert(dsRslt.Tables["LOTINFO"]);

            }
            else
            {
                bVisibility = false;
                string strLotId = item.Header.ToString();

                DataSet inData = new DataSet();
                DataTable dtRqst = inData.Tables.Add("INDATA");

                dtRqst.Columns.Add("LANGID", typeof(string));
                dtRqst.Columns.Add("LOTID", typeof(string));
                dtRqst.Columns.Add("PROCID", typeof(string));

                DataRow dr = dtRqst.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["LOTID"] = strLotId;
                dr["PROCID"] = cboMixProcess.SelectedValue;

                dtRqst.Rows.Add(dr);
                dsRslt = new ClientProxy().ExecuteServiceSync_Multi(_SearchDetailHistoryBizRule, "INDATA", "MIXLOTINFO,LOTINFO", inData);
                dgMBomInfo.ItemsSource = DataTableConverter.Convert(dsRslt.Tables["MIXLOTINFO"]);
                dgLotInfo.ItemsSource = DataTableConverter.Convert(dsRslt.Tables["LOTINFO"]);
            }
            dgMBomInfo.Columns["MTRLNAME"].Visibility = bVisibility ? Visibility.Visible : Visibility.Collapsed;
            dgMBomInfo.Columns["WIPDTTM_ST"].Visibility = !bVisibility ? Visibility.Visible : Visibility.Collapsed;
            dgMBomInfo.Columns["WIPDTTM_ED"].Visibility = !bVisibility ? Visibility.Visible : Visibility.Collapsed;

            dgLotInfo.Columns["LOTID"].Visibility = bVisibility ? Visibility.Visible : Visibility.Collapsed;
            dgLotInfo.Columns["PRJT_NAME"].Visibility = !bVisibility ? Visibility.Visible : Visibility.Collapsed;
            dgLotInfo.Columns["WIPDTTM_ST"].Visibility = !bVisibility ? Visibility.Visible : Visibility.Collapsed;
            dgLotInfo.Columns["WIPDTTM_ED"].Visibility = !bVisibility ? Visibility.Visible : Visibility.Collapsed;
            dgLotInfo.Columns["WIPDTTM_CO"].Visibility = !bVisibility ? Visibility.Visible : Visibility.Collapsed;
        }
        private void SetEvent()
        {
            this.Loaded -= UserControl_Loaded;
            dtpDateFrom.SelectedDataTimeChanged += dtpDateFrom_SelectedDataTimeChanged;
            dtpDateTo.SelectedDataTimeChanged += dtpDateTo_SelectedDataTimeChanged;
        }

        private void InitControl()
        {
            dtpDateFrom.SelectedDateTime = DateTime.Now;
            dtpDateTo.SelectedDateTime = DateTime.Now;
        }
        private void dtpDateFrom_SelectedDataTimeChanged(object sender, SelectionChangedEventArgs e)
        {
            LGCDatePicker dtPik = sender as LGCDatePicker;
            if (Convert.ToDecimal(dtpDateTo.SelectedDateTime.ToString("yyyyMMdd")) < Convert.ToDecimal(dtPik.SelectedDateTime.ToString("yyyyMMdd")))
            {
                dtPik.SelectedDateTime = dtpDateTo.SelectedDateTime;
                //  LGC.GMES.MES.ControlsLibrary.MessageBox.Show("시작일자 이전 날짜는 선택할 수 없습니다.", null, "Error", MessageBoxButton.OK, ControlsLibrary.MessageBoxIcon.Warning);
                return;
            }
        }
        private void dtpDateTo_SelectedDataTimeChanged(object sender, SelectionChangedEventArgs e)
        {
            LGCDatePicker dtPik = sender as LGCDatePicker;
            if (Convert.ToDecimal(dtpDateFrom.SelectedDateTime.ToString("yyyyMMdd")) > Convert.ToDecimal(dtPik.SelectedDateTime.ToString("yyyyMMdd")))
            {
                dtPik.SelectedDateTime = dtpDateFrom.SelectedDateTime;
                //  LGC.GMES.MES.ControlsLibrary.MessageBox.Show("시작일자 이전 날짜는 선택할 수 없습니다.", null, "Error", MessageBoxButton.OK, ControlsLibrary.MessageBoxIcon.Warning);
                return;
            }
        }
        #endregion

        #region Mehod

        /// <summary>
        /// 화면내 버튼 권한 처리
        /// </summary>
        private void ApplyPermissions()
        {
            List<Button> listAuth = new List<Button>();
            //listAuth.Add(btnInReplace);
            //listAuth.Add(btnInReplace);

            Util.pageAuth(listAuth, FrameOperation.AUTHORITY);
        }

        private void InitCombo()
        {
            CommonCombo _combo = new CommonCombo();
            // 동
            _combo.SetCombo(cboArea, CommonCombo.ComboStatus.NONE, cbChild: new C1ComboBox[] { cboLine }, sFilter: new string[] { LoginInfo.CFG_SHOP_ID });
            // 라인
            _combo.SetCombo(cboLine, CommonCombo.ComboStatus.NONE, cbParent: new C1ComboBox[] { cboArea }, cbChild: new C1ComboBox[] { cboMixProcess, cboEquipment }, sFilter: new string[] { LoginInfo.CFG_EQSG_ID });
            // 공정
            _combo.SetCombo(cboMixProcess, CommonCombo.ComboStatus.NONE, cbParent: new C1ComboBox[] { cboLine }, cbChild: new C1ComboBox[] { cboEquipment });
            // 설비
            _combo.SetCombo(cboEquipment, CommonCombo.ComboStatus.SELECT, cbParent: new C1ComboBox[] { cboLine, cboMixProcess }); //, sFilter: new string[] { cboProcess.SelectedValue.ToString() });           
        }
        
        private DataTable AddStatus(DataTable dt, string sValue, string sDisplay)
        {
            DataRow dr = dt.NewRow();

            dr[sDisplay] = "-SELECT-";
            dr[sValue] = _SelectValue;
            dt.Rows.InsertAt(dr, 0);

            return dt;
        }

        /// <summary>
        /// 조회
        /// </summary>
        private void SearchData()
        {
            string returnValue = string.Empty;
            try
            {
                if (_SelectValue.Equals(cboEquipment.SelectedValue))
                {
                    Util.MessageValidation("SFU1673");  //설비를 선택하세요.
                    return;
                }                

                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("EQSGID", typeof(string));
                RQSTDT.Columns.Add("PROCID", typeof(string));
                RQSTDT.Columns.Add("EQPTID", typeof(string));
                RQSTDT.Columns.Add("FROM_DATE", typeof(string));
                RQSTDT.Columns.Add("TO_DATE", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["EQSGID"] = cboLine.SelectedValue;
                dr["PROCID"] = cboMixProcess.SelectedValue;
                dr["EQPTID"] = cboEquipment.SelectedValue;
                dr["FROM_DATE"] = Util.GetCondition(dtpDateFrom);
                dr["TO_DATE"] = Util.GetCondition(dtpDateTo);
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync(_SearchBizRule, "RQSTDT", "RSLTDT", RQSTDT);
                Clear();
                setTreeMenuItems(trvData, string.Empty, DataTableConverter.Convert(dtResult));
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

        private void setTreeMenuItems(ItemsControl control, string MENUID_PR, System.Collections.IEnumerable treeMenuList)
        {
            IEnumerable<object> childMenuList = (from object menu in treeMenuList
                                                 where MENUID_PR.Equals(DataTableConverter.GetValue(menu, "PR_NODE") == null ? "" : DataTableConverter.GetValue(menu, "PR_NODE").ToString())
                                                // orderby DataTableConverter.GetValue(menu, "MENUSEQ")
                                                 select menu);

            foreach (object childMenu in childMenuList)
            {
                //C1TreeViewItem treeViewItem = new C1TreeViewItem();
                C1TreeViewItem treeViewItem = new C1TreeViewItem();
                treeViewItem.Click += treeViewItem_Click;
                treeViewItem.DataContext = childMenu;
                //treeViewItem.VerticalContentAlignment = System.Windows.VerticalAlignment.Center;
                //treeViewItem.HorizontalContentAlignment = System.Windows.HorizontalAlignment.Stretch;
                if ("".Equals(MENUID_PR))
                {
                    //treeViewItem.HeaderTemplate = tvProcTree.Resources["ProcTreeViewItemTemplate"] as DataTemplate;
                }
                else
                {
                    //treeViewItem.HeaderTemplate = tvProcTree.Resources["EqptTreeViewItemTemplate"] as DataTemplate;
                }
                treeViewItem.Header = DataTableConverter.GetValue(childMenu, "NODE_NAME");
                control.Items.Add(treeViewItem);
                setTreeMenuItems(treeViewItem, DataTableConverter.GetValue(childMenu, "NODE_KEY").ToString(), treeMenuList);
            }
        }

        private void Clear()
        {
            trvData.Items.Clear();
            Util.gridClear(dgLotInfo);
            Util.gridClear(dgMBomInfo);
        }
        #endregion

    }
}
