/*************************************************************************************
 Created Date : 2017.02.21
      Creator : INS 정문교C
   Decription : 전지 5MEGA-GMES 구축 - Tray 조회 팝업
--------------------------------------------------------------------------------------
 [Change History]
  2017.02.21   INS 정문교C : Initial Created.
  2017.07.19    신광희C ASSY002_003_TRAY_INFO.xaml => CMM_ASSY_TRAY_INFO.xaml 파일로 변경
**************************************************************************************/

using System;
using System.Windows;
using System.Data;

using C1.WPF;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using LGC.GMES.MES.CMM001.Class;
using C1.WPF.DataGrid;
using System.Collections.Generic;
using System.Windows.Media;

namespace LGC.GMES.MES.CMM001
{
    public partial class CMM_ASSY_TRAY_INFO : C1Window, IWorkArea
    {
        #region Declaration & Constructor 

        BizDataSet _bizRule = new BizDataSet();

        private string procID = string.Empty;
        private string lineID = string.Empty;

        /// <summary>
        ///  Frame과 상호작용하기 위한 객체
        /// </summary>
        public IFrameOperation FrameOperation
        {
            get;
            set;
        }

        #endregion

        #region Initialize
        public CMM_ASSY_TRAY_INFO()
        {
            InitializeComponent();
        }

        private void InitCombo()
        {
            CommonCombo combo = new CommonCombo();

            // 특별 TRAY  사유 Combo
            String[] sFilter = { LoginInfo.CFG_AREA_ID };
            combo.SetCombo(cboSpecialTRay, CommonCombo.ComboStatus.SELECT, sFilter: sFilter, sCase: "SpecialResonCodebyAreaCode");

            if (cboSpecialTRay?.Items != null && cboSpecialTRay.Items.Count > 0)
                cboSpecialTRay.SelectedIndex = 0;
        }
        private void InitControl()
        {
            dtpDateFrom.SelectedDateTime = System.DateTime.Now.AddDays(-7);
            dtpDateTo.SelectedDateTime = System.DateTime.Now;
        }
        private void InitControlEnabled(bool bDate)
        {
            if (bDate)
            {
                if ((bool)chkProdDate.IsChecked)
                {
                    dtpDateFrom.IsEnabled = true;
                    dtpDateTo.IsEnabled = true;
                }
                else
                {
                    dtpDateFrom.IsEnabled = false;
                    dtpDateTo.IsEnabled = false;
                }
            }
            else
            {
                if ((bool)chkSpecialTRay.IsChecked)
                    cboSpecialTRay.IsEnabled = true;
                else
                    cboSpecialTRay.IsEnabled = false;
            }
        }
        #endregion

        #region Event

        #region [Form Load]
        private void CMM_ASSY_TRAY_INFO_Loaded(object sender, RoutedEventArgs e)
        {
            object[] tmps = C1WindowExtension.GetParameters(this);

            procID = tmps[0] as string;
            lineID = tmps[1] as string;

            chkProdDate.IsChecked = true;

            InitCombo();                  // 특별관리Tray구분
            InitControl();                // Control 초기화

            InitControlEnabled(false);

            chkProdDate.Click += chkProdDate_Click;
            chkSpecialTRay.Click += chkSpecialTRay_Click;
        }
        #endregion

        #region [조회]
        /// <summary>
        /// 조회
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            if ((txtTrayID.Text.Trim()).Length > 0 && (txtTrayID.Text.Trim()).Length < 4)
            {
                //Util.Alert("Try ID는 4자리 이상 입력 하세요.");
                Util.MessageValidation("Try ID는 4자리 이상 입력 하세요.");
                return;
            }

            GetTrayList();
        }
        #endregion

        #region [Excel]
        /// <summary>
        /// Excel
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnExcel_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                new ExcelExporter().Export(dgTray);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        #endregion

        #region [닫기]
        /// <summary>
        /// 닫기
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
        #endregion

        #region [생산일자] - chkProdDate_Click
        /// <summary>
        /// 생산일자 체크
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void chkProdDate_Click(object sender, RoutedEventArgs e)
        {
            InitControlEnabled(true);
        }
        #endregion

        #region [특별관리Tray구분] - chkSpecialTRay_Click
        /// <summary>
        /// 특별관리Tray구분 체크
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void chkSpecialTRay_Click(object sender, RoutedEventArgs e)
        {
            InitControlEnabled(false);
        }
        #endregion

        #endregion

        #region User Mehod

        #region [BizCall]
        /// <summary>
        /// Tray 조회
        /// </summary>
        private void GetTrayList()
        {
            try
            {
                ShowLoadingIndicator();

                DataTable IndataTable = _bizRule.GetDA_BAS_SEL_TRAY_LIST_WS();
                DataRow Indata = IndataTable.NewRow();

                Indata["LANGID"] = LoginInfo.LANGID;
                Indata["PROCID"] = procID;
                Indata["EQSGID"] = lineID;
                Indata["TRAYID"] = string.IsNullOrWhiteSpace(txtTrayID.Text) ? null : txtTrayID.Text;

                if ((bool)chkSpecialTRay.IsChecked)
                    Indata["SPCL_RSNCODE"] = cboSpecialTRay.SelectedValue == null ? "" : cboSpecialTRay.SelectedValue.ToString();
                else
                    Indata["SPCL_RSNCODE"] = DBNull.Value;

                if ((bool)chkProdDate.IsChecked)
                {
                    Indata["FROM_DATE"] = dtpDateFrom.SelectedDateTime.ToString("yyyyMMdd");
                    Indata["TO_DATE"] = dtpDateTo.SelectedDateTime.ToString("yyyyMMdd");
                }
                else
                {
                    Indata["FROM_DATE"] = DBNull.Value;
                    Indata["TO_DATE"] = DBNull.Value;
                }

                IndataTable.Rows.Add(Indata);

                //DataSet ds = new DataSet();
                //ds.Tables.Add(IndataTable);
                //string xml = ds.GetXml();

                DataTable dt = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_TRAY_LIST_WS", "INDATA", "RSLTDT", IndataTable);

                Util.GridSetData(dgTray, dt, FrameOperation, true);
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

        #region[[Validation]
        /// <summary>
        ///  조회 Validation
        /// </summary>
        /// <returns></returns>
        private bool CanSearch()
        {
            bool bRet = false;

            if (chkProdDate.IsChecked == false && chkSpecialTRay.IsChecked == false && string.IsNullOrWhiteSpace(txtTrayID.Text))
            {
                //Util.Alert("조회할 Tray ID 를 입력하세요.");
                Util.MessageValidation("조회할 Tray ID 를 입력하세요.");
                return bRet;
            }

            if ((bool)chkSpecialTRay.IsChecked && (cboSpecialTRay.SelectedValue == null || cboSpecialTRay.SelectedIndex == 0))
            {
                //Util.Alert("조회할 특별관리 Tray구분을 선택하세요.");
                Util.MessageValidation("조회할 특별관리 Tray구분을 선택하세요.");
                return bRet;
            }

            bRet = true;
            return bRet;
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

        #endregion

        #endregion




    }
}
