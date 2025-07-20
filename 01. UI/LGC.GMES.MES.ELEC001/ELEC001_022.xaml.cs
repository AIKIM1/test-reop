/*************************************************************************************
 Created Date : 2017.01.10
      Creator : 이슬아
   Decription : 믹서 자재(바인더/CMC) 잔량 유효기간 관리
--------------------------------------------------------------------------------------
 [Change History]
  2017.01.10  이슬아 : Initial Created.





 
**************************************************************************************/
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using System;
using System.Collections.Generic;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace LGC.GMES.MES.ELEC001
{

    public partial class ELEC001_022 : UserControl, IWorkArea
    {
        #region Declaration & Constructor 
        Util util = new Util();       

        public IFrameOperation FrameOperation
        {
            get;
            set;
        }

        public ELEC001_022()
        {
            InitializeComponent();
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            ApplyPermissions();
            InitCombo();
            InitControl();
            SetEvent();
        }

        /// <summary>
        /// 화면내 버튼 권한 처리
        /// </summary>
        private void ApplyPermissions()
        {
            List<Button> listAuth = new List<Button>();
            //listAuth.Add(btnInReplace);
            Util.pageAuth(listAuth, FrameOperation.AUTHORITY);
        }

        #endregion

        #region Initialize
        /// <summary>
        /// 콤보박스 셋팅
        /// </summary>
        private void InitCombo()
        {
            CommonCombo combo = new CommonCombo();
         
            //CMC/바인더 구분
            string[] sFilter = { "CMC_BINDER_TYPE_CODE" };
            combo.SetCombo(cboType, CommonCombo.ComboStatus.ALL, sCase: "COMMCODE", sFilter: sFilter);
            combo.SetCombo(cboType_REG, CommonCombo.ComboStatus.SELECT, sCase: "COMMCODE", sFilter: sFilter);
        }

        /// <summary>
        /// 컨트롤 초기값 설정
        /// </summary>
        private void InitControl()
        {           
        }

        #endregion

        #region Event

        /// <summary>
        /// Initializing 이후에 FormLoad시 Event를 생성.
        /// </summary>
        private void SetEvent()
        {
            this.Loaded -= UserControl_Loaded;
            dtpDateFrom.SelectedDataTimeChanged += dtpDateFrom_SelectedDataTimeChanged;
            dtpDateTo.SelectedDataTimeChanged += dtpDateTo_SelectedDataTimeChanged;
        }

        private void btnSearch_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            loadingIndicator.Visibility = Visibility.Visible;
        }

        #region[TAB1]
        
        private void dtpDateFrom_SelectedDataTimeChanged(object sender, SelectionChangedEventArgs e)
        {
            LGCDatePicker dtPik = sender as LGCDatePicker;
            if (Convert.ToDecimal(dtpDateTo.SelectedDateTime.ToString("yyyyMMdd")) < Convert.ToDecimal(dtPik.SelectedDateTime.ToString("yyyyMMdd")))
            {
                dtPik.SelectedDateTime = dtpDateTo.SelectedDateTime;
                return;
            }
        }

        private void dtpDateTo_SelectedDataTimeChanged(object sender, SelectionChangedEventArgs e)
        {
            LGCDatePicker dtPik = sender as LGCDatePicker;
            if (Convert.ToDecimal(dtpDateFrom.SelectedDateTime.ToString("yyyyMMdd")) > Convert.ToDecimal(dtPik.SelectedDateTime.ToString("yyyyMMdd")))
            {
                dtPik.SelectedDateTime = dtpDateFrom.SelectedDateTime;
                return;
            }
        }

        private void btnSearch1_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                DataTable dtRQSTDT = new DataTable("RQSTDT");
                dtRQSTDT.Columns.Add("AREAID", typeof(string));
                dtRQSTDT.Columns.Add("DATEFROM", typeof(string));
                dtRQSTDT.Columns.Add("DATETO", typeof(string));
                dtRQSTDT.Columns.Add("CMC_BINDER_TYPE_CODE", typeof(string));

                DataRow drnewrow = dtRQSTDT.NewRow();

                drnewrow["AREAID"] = LoginInfo.CFG_AREA_ID;
                if(!string.IsNullOrWhiteSpace(cboType.SelectedValue.ToString())) drnewrow["CMC_BINDER_TYPE_CODE"] = cboType.SelectedValue;
                drnewrow["DATEFROM"] = dtpDateFrom.SelectedDateTime.ToString("yyyyMMdd");
                drnewrow["DATETO"] = dtpDateTo.SelectedDateTime.ToString("yyyyMMdd");
                dtRQSTDT.Rows.Add(drnewrow);

                new ClientProxy().ExecuteService("DA_BAS_SEL_TB_SFC_MIXER_CMC_BINDER", "RQSTDT", "RSLTDT", dtRQSTDT, (result, Exception) =>
                {
                    if (Exception != null)
                    {
                        Util.AlertByBiz("DA_BAS_SEL_TB_SFC_MIXER_CMC_BINDER", Exception.Message, Exception.ToString());
                        return;
                    }
                    Util.GridSetData(dgSearch, result, FrameOperation);                   
                });
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
        private void btnReprint_Click(object sender, RoutedEventArgs e)
        {

        }
        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(Util.GetCondition(cboType_REG, "SFU3201"))) return;  //CMC/바인더 구분을 선택해주세요.

                DataTable dtRQSTDT = new DataTable("RQSTDT");
                dtRQSTDT.Columns.Add("AREAID", typeof(string));
                dtRQSTDT.Columns.Add("CMC_BINDER_TYPE_CODE", typeof(string));
                dtRQSTDT.Columns.Add("MFG_DATE", typeof(string));
                dtRQSTDT.Columns.Add("VLD_DATE", typeof(string));
                dtRQSTDT.Columns.Add("USERID", typeof(string));

                DataRow drnewrow = dtRQSTDT.NewRow();

                drnewrow["AREAID"] = LoginInfo.CFG_AREA_ID;
                drnewrow["CMC_BINDER_TYPE_CODE"] = cboType_REG.SelectedValue;
                drnewrow["MFG_DATE"] = dtpMFG_DATE.SelectedDateTime.ToString("yyyyMMdd");
                drnewrow["VLD_DATE"] = dtpVLD_DATE.SelectedDateTime.ToString("yyyyMMdd");
                drnewrow["USERID"] = LoginInfo.USERID;
                dtRQSTDT.Rows.Add(drnewrow);

                new ClientProxy().ExecuteService("BR_PRD_REG_MIXER_CMC_BINDER", "RQSTDT", null, dtRQSTDT, (result, Exception) =>
                {
                    if (Exception != null)
                    {
                        Util.AlertByBiz("BR_PRD_REG_MIXER_CMC_BINDER", Exception.Message, Exception.ToString());
                        return;
                    }
                    try
                    {
                        if (result != null)
                        {
                            Util.AlertByBiz("BR_PRD_REG_MIXER_CMC_BINDER", Exception.Message, Exception.ToString());
                            return;
                        }

                        Util.AlertInfo("SFU1275");  //정상처리되었습니다.
                    }
                    catch (Exception ex)
                    {
                        Util.MessageException(ex);
                    }
                });
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return;
            }
        }
        #endregion

        #region[TAB2]
        private void txtNo_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key != Key.Enter) return;
            InPut();
        }
        private void btnInput_Click(object sender, RoutedEventArgs e)
        {
            InPut();
        }
        
        #endregion

        #endregion

        #region[Method]    

        private void InPut()
        {
            try
            {
                DataTable dtRQSTDT = new DataTable("RQSTDT");
                dtRQSTDT.Columns.Add("USERID", typeof(string));
                dtRQSTDT.Columns.Add("CMC_BINDER_ID", typeof(string));

                DataRow drnewrow = dtRQSTDT.NewRow();
                drnewrow["USERID"] = LoginInfo.USERID;
                drnewrow["CMC_BINDER_ID"] = txtNo.Text;
                dtRQSTDT.Rows.Add(drnewrow);

                new ClientProxy().ExecuteService("BR_PRD_UPD_MIXER_CMC_BINDER", "RQSTDT", "RSLTDT", dtRQSTDT, (result, Exception) =>
                {
                    if (Exception != null)
                    {
                        Util.AlertByBiz("BR_PRD_UPD_MIXER_CMC_BINDER", Exception.Message, Exception.ToString());
                        return;
                    }
                    if (result.Rows.Count < 1)
                        Util.MessageValidation("SFU2952"); //존재하지 않는 일련번호 입니다.
                    Util.GridSetData(dgList, result, FrameOperation);
                });
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

        #endregion

     
    }
}
