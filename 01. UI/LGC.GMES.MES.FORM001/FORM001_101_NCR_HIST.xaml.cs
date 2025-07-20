/*************************************************************************************
 Created Date : 2017.07.25
      Creator : 
   Decription : 
--------------------------------------------------------------------------------------
 [Change History]
 
**************************************************************************************/

using System;
using System.Windows;
using System.Data;

using C1.WPF;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.CMM001.Extensions;
using LGC.GMES.MES.CMM001.UserControls;
using LGC.GMES.MES.CMM001.Popup;
using System.Windows.Controls;
using System.Collections.Generic;

namespace LGC.GMES.MES.FORM001
{
    public partial class FORM001_101_NCR_HIST : C1Window, IWorkArea
    {
        #region Declaration & Constructor 
        Util _util = new Util();
        string sLotId;
        string sELotid;
        string sInspId;
        
        public FORM001_101_NCR_HIST()
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

        private void C1Window_Loaded(object sender, RoutedEventArgs e)
        {
            ApplyPermissions();
            InitCombo();
            InitControl();
            SetEvent();
            GetNCRHist();
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
        /// 콤보박스 초기 설정
        /// </summary>
        private void InitCombo()
        {
            CommonCombo _combo = new CommonCombo();

            //_combo.SetCombo(cboLine, CommonCombo.ComboStatus.SELECT, sFilter: new string[]{LoginInfo.CFG_AREA_ID}, cbChild: new C1ComboBox[] {cboEquipment_Search }, sCase: "LINE_CP");
      
        }

        /// <summary>
        /// 컨트롤 초기값 설정
        /// </summary>
        private void InitControl()
        {
            DataTable dtInfo = new DataTable();
            for (int i = 0; i < dgMain.Columns.Count; i++)
            {
                dtInfo.Columns.Add(dgMain.Columns[i].Name);
            }

            object[] param = C1WindowExtension.GetParameters(this);

            if (param.Length >= 9)
            {
                dtInfo.Rows.Add();
                dtInfo.Rows[0]["LOTID"] = sLotId = Util.NVC(param[0]);
                dtInfo.Rows[0]["PRJT_NAME"] = param[1];
                dtInfo.Rows[0]["PRODID"] = param[2];
                dtInfo.Rows[0]["LOTTYPE"] = param[3];
                dtInfo.Rows[0]["RESULT"] = param[4];
                dtInfo.Rows[0]["ELTR_LOTID"] = sELotid = Util.NVC(param[5]);
                dtInfo.Rows[0]["INSP_NAME"] = param[6];
                dtInfo.Rows[0]["INSP_ID"] = sInspId = Util.NVC(param[7]);
                dtInfo.Rows[0]["JUDG_FLAG"] = param[8];
                if (Util.NVC(param[9]) == "Y")
                    sLotId = sELotid;
                Util.GridSetData(dgMain, dtInfo, FrameOperation, true);
            }
        }
        #endregion

        #region Event
        /// <summary>
        /// Initializing 이후에 FormLoad시 Event를 생성.
        /// </summary>
        private void SetEvent()
        {
         //   this.Loaded -= UserControl_Loaded;
        }

        /// <summary>
        /// 닫기
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = MessageBoxResult.Cancel;
        }
        #endregion

        #region [Biz]
        /// <summary>
        /// 조회
        /// BIZ : DA_PRD_SEL_INPALLET_FM
        /// </summary>
        private void GetNCRHist()
        {
            try
            {
                object[] tmps = C1WindowExtension.GetParameters(this);

                DataTable RQSTDT = new DataTable();
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("LOTID", typeof(string));
                RQSTDT.Columns.Add("INSP_ID", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["LOTID"] = sLotId;
                dr["INSP_ID"] = sInspId;

                RQSTDT.Rows.Add(dr);

                new ClientProxy().ExecuteService("DA_PRD_SEL_NCR_HIST", "RQSTDT", "RSLTDT", RQSTDT, (searchResult, searchException) =>
                {
                    try
                    {
                        if (searchException != null)
                        {
                            Util.MessageException(searchException);
                            return;
                        }
                        Util.GridSetData(dgDetail, searchResult, FrameOperation, true);                       
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
                );
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
