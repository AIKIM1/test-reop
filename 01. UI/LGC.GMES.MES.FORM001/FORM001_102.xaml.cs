/*************************************************************************************
 Created Date : 2020.08.19
      Creator : 
   Decription : 
--------------------------------------------------------------------------------------
 [Change History]
  2020.08.19  DEVELOPER : Initial Created.




 
**************************************************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Data;
using C1.WPF;
using LGC.GMES.MES.Common;
using System.Net;
using System.Reflection;
using System.Collections;
using System.Globalization;
using LGC.GMES.MES.CMM001;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.ControlsLibrary;
using C1.WPF.DataGrid.Summaries;

namespace LGC.GMES.MES.FORM001
{
    public partial class FORM001_102 : UserControl, IWorkArea
    {
        #region Declaration & Constructor 
        Util _util = new Util();
        public FORM001_102()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Frame과 상호작용하기 위한 객체
        /// </summary>
        public IFrameOperation FrameOperation
        {
            get;
            set;
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            ApplyPermissions();
            InitControl();
        }

        #endregion

        #region Initialize
        /// <summary>
        /// 화면내 버튼 권한 처리
        /// </summary>
        private void ApplyPermissions()
        {
            List<Button> listAuth = new List<Button>();
            //listAuth.Add(btnInReplace);
            Util.pageAuth(listAuth, FrameOperation.AUTHORITY);
        }

        /// <summary>
        /// 컨트롤 초기값 설정
        /// </summary>
        private void InitControl()
        {
            InitCombo();
            InitbPeriod();
            chkRptType.IsChecked = false;
        }

        /// <summary>
        /// 콤보박스 초기 설정
        /// </summary>
        private void InitCombo()
        {
            CommonCombo _combo = new CommonCombo();

            String[] sFilter = { LoginInfo.CFG_AREA_ID };
            //동
            C1ComboBox[] cboAreaChild = { cboEquipmentSegment };
            _combo.SetCombo(cboArea, CommonCombo.ComboStatus.SELECT, cbChild: cboAreaChild);
            //cboArea.SelectedValue = LoginInfo.CFG_AREA_ID;
            if (cboArea.Items.Count > 0) cboEquipmentSegment.SelectedIndex = 0;

            //라인
            C1ComboBox[] cboLineParent = { cboArea };
            _combo.SetCombo(cboEquipmentSegment, CommonCombo.ComboStatus.ALL, cbParent: cboLineParent);
            //cboEquipmentSegment.SelectedValue = LoginInfo.CFG_EQSG_ID;

            //공정
            C1ComboBox[] cboProcessParent = { cboArea, cboEquipmentSegment };
            _combo.SetCombo(cboEquipmentSegment, CommonCombo.ComboStatus.ALL, cbParent: cboLineParent);

            //극성
            String[] sFilter1 = { "ELTR_TYPE_CODE" };
            _combo.SetCombo(cboElecType, CommonCombo.ComboStatus.ALL, sFilter: sFilter1, sCase: "COMMCODE");

            //if (cboEquipmentSegment.Items.Count > 0) cboEquipmentSegment.SelectedIndex = 0;

            //생산구분
            string[] sFilter2 = { "PRODUCT_DIVISION" };
            _combo.SetCombo(cboProductDiv, CommonCombo.ComboStatus.ALL, sCase: "COMMCODE", sFilter: sFilter2);

            // 생산구분 Default 정상생산
            if (cboProductDiv.Items.Count > 1)
                cboProductDiv.SelectedIndex = 1;

            // 차단 유형 코드
            string[] sFilter3 = { "BLOCK_TYPE_CODE" };
            _combo.SetCombo(cboBlockType, CommonCombo.ComboStatus.ALL, sCase: "COMMCODE", sFilter: sFilter3);

        }

        #endregion

        #region Event
        /// <summary>
        /// Initializing 이후에 FormLoad시 Event를 생성.
        /// </summary>

        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            Search();
        }
        
        #endregion

        #region Method
        /// <summary>
        /// 조회
        /// </summary>
        private void Search()
        {
            try
            {                
                if ((ldpDateTo.SelectedDateTime - ldpDateFrom.SelectedDateTime).TotalDays > 31)
                {
                    //기간은 {0}일 이내 입니다.
                    Util.MessageValidation("SFU2042", "31");        // 31일 이내로 조회
                    return;
                }

                if (Util.NVC(cboArea.SelectedValue.ToString()) == "")
                {
                    Util.MessageValidation("SFU3203");  //동은필수입니다.
                    return;
                }

                //if (Util.NVC(cboEquipmentSegment.SelectedValue.ToString()) == "")
                //{
                //    Util.MessageValidation("SFU1223");  //라인은필수입니다.
                //    return;
                //}

                //if (Util.NVC(cboProcess.SelectedItemsToString) == "")
                //{
                //    Util.MessageValidation("SFU1459");  //공정을 선택하세요.
                //    return;
                //}

                ShowLoadingIndicator();

                DataTable RQSTDT = new DataTable();
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("AREAID", typeof(string));                   // 동
                RQSTDT.Columns.Add("EQSGID", typeof(string));                   // 라인
                RQSTDT.Columns.Add("PROCID", typeof(string));                   // 공정
                RQSTDT.Columns.Add("PRDT_CLSS_CODE", typeof(string));
                RQSTDT.Columns.Add("LOTTYPE", typeof(string));
                RQSTDT.Columns.Add("PRJT_NAME", typeof(string));
                RQSTDT.Columns.Add("MODLID", typeof(string));
                RQSTDT.Columns.Add("PRODID", typeof(string));
                RQSTDT.Columns.Add("LOTID", typeof(string));
                RQSTDT.Columns.Add("BLOCK_TYPE_CODE", typeof(string));
                RQSTDT.Columns.Add("RPT_TYPE", typeof(string));
                RQSTDT.Columns.Add("FROM_DATE", typeof(string));
                RQSTDT.Columns.Add("TO_DATE", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["AREAID"] = cboArea.SelectedValue.ToString();    
                dr["EQSGID"] = Util.GetCondition(cboEquipmentSegment);
                dr["PROCID"] = cboProcess.SelectedItemsToString.ToString();
                dr["PRDT_CLSS_CODE"] = cboElecType.SelectedValue.ToString() == "" ? null : cboElecType.SelectedValue.ToString();
                dr["LOTTYPE"] = cboProductDiv.SelectedValue.ToString() == "" ? null : cboProductDiv.SelectedValue.ToString();
                dr["PRJT_NAME"] = Util.GetCondition(txtPrjtName);
                dr["MODLID"] = Util.GetCondition(txtModlId);
                dr["PRODID"] = Util.GetCondition(txtProdId);
                dr["LOTID"] = Util.GetCondition(txtLotID);            
                dr["BLOCK_TYPE_CODE"] = cboBlockType.SelectedValue.ToString() == "" ? null : cboBlockType.SelectedValue.ToString(); ;                 
                if (chkRptType.IsChecked.ToString().Equals("True"))
                {
                    dr["RPT_TYPE"] = "HIST";
                }
                else
                {
                    dr["RPT_TYPE"] = "FINL";
                }

                if (((System.Windows.FrameworkElement)tbcList.SelectedItem).Name.Equals("TERM"))
                {
                    dr["FROM_DATE"] = Util.GetCondition(ldpDateFrom);
                    dr["TO_DATE"] = Util.GetCondition(ldpDateTo);
                }
                RQSTDT.Rows.Add(dr);
                string bizName = string.Empty;
                
                if (((System.Windows.FrameworkElement)tbcList.SelectedItem).Name.Equals("WIPSTATE"))
                {
                    bizName = "DA_PRD_SEL_LOT_QMS_INSP_HIST";
                }
                else
                {
                    bizName = "DA_PRD_SEL_LOT_QMS_INSP_HIST_TERM";
                }

                new ClientProxy().ExecuteService(bizName, "RQSTDT", "RSLTDT", RQSTDT, (searchResult, searchException) =>
                {
                    try
                    {

                        if (searchException != null)
                        {
                            Util.MessageException(searchException);
                            return;
                        }

                        if (((System.Windows.FrameworkElement)tbcList.SelectedItem).Name.Equals("WIPSTATE"))
                        {
                            Util.gridClear(dgSearchResult);
                            Util.GridSetData(dgSearchResult, searchResult, FrameOperation, true);
                            string[] sColumnName = new string[] { "LOTID", "PRODID" };
                            _util.SetDataGridMergeExtensionCol(dgSearchResult, sColumnName, DataGridMergeMode.VERTICALHIERARCHI); //VERTICALHIERARCHI);
                        }
                        else
                        {
                            Util.gridClear(dgTermSearchResult);
                            Util.GridSetData(dgTermSearchResult, searchResult, FrameOperation, true);
                            string[] sColumnName = new string[] { "LOTID", "PRODID" };
                            _util.SetDataGridMergeExtensionCol(dgTermSearchResult, sColumnName, DataGridMergeMode.VERTICALHIERARCHI); //VERTICALHIERARCHI);
                        }

                    }
                    catch (Exception ex)
                    {
                        Util.MessageException(ex);

                    }
                    finally
                    {
                        //HiddenLoadingIndicator();
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
        
        private void tbcList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            InitbPeriod();
        }

        private void InitbPeriod()
        {
            if (((System.Windows.FrameworkElement)tbcList.SelectedItem).Name.Equals("WIPSTATE"))
            {
                tbPeriod.Visibility = Visibility.Collapsed;
                spPeriod.Visibility = Visibility.Collapsed;
                tbPeriod.IsEnabled = false;
                spPeriod.IsEnabled = false;
            }
            else
            {
                tbPeriod.Visibility = Visibility.Visible;
                spPeriod.Visibility = Visibility.Visible;
                tbPeriod.IsEnabled = true;
                spPeriod.IsEnabled = true;
            }
        }

        private void cboEquipmentSegment_SelectedIndexChanged(object sender, PropertyChangedEventArgs<int> e)
        {
            try
            {
                this.Dispatcher.BeginInvoke(new System.Action(() =>
                {
                    SetcboProcess();
                }));
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private void SetcboProcess()
        {
            try
            {
                ShowLoadingIndicator();

                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("AREAID", typeof(string));
                RQSTDT.Columns.Add("EQSGID", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["AREAID"] = cboArea.SelectedValue;
                dr["EQSGID"] = Util.GetCondition(cboEquipmentSegment);
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_WIP_PROCESS_CBO", "RQSTDT", "RSLTDT", RQSTDT);

                cboProcess.ItemsSource = DataTableConverter.Convert(dtResult);

                for (int i = 0; i < dtResult.Rows.Count; i++)
                {
                    cboProcess.Check(i);
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

        private void chkSelHist_Checked(object sender, RoutedEventArgs e)
        {
            dgSearchResult.Columns["SEQNO"].Visibility = Visibility.Visible;
            Search();      
        }

        private void chkSelHist_Unchecked(object sender, RoutedEventArgs e)
        {
            dgSearchResult.Columns["SEQNO"].Visibility = Visibility.Collapsed;
            Search();
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
    }
}
