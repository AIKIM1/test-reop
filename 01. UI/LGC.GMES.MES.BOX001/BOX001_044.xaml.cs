/*************************************************************************************
 Created Date : 2017.07.06
      Creator : 이영준
   Decription : 제품별 포장 규격
--------------------------------------------------------------------------------------
 [Change History]
  2017.09.07  이영준 : Initial Created.





 
**************************************************************************************/

using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Data;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.CMM001.Class;

namespace LGC.GMES.MES.BOX001
{
    public partial class BOX001_044 : UserControl, IWorkArea
    {
        #region Declaration & Constructor 

        Util _util = new Util();
        public BOX001_044()
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
            InitCombo();
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
        /// 콤보박스 초기 설정
        /// </summary>
        private void InitCombo()
        {
            CommonCombo _combo = new CommonCombo();
            String[] sFilter = { "IUSE" };
            //_combo.SetCombo(cboElecType, CommonCombo.ComboStatus.ALL, sFilter: sFilter2, sCase: "COMMCODE");

            _combo.SetCombo(cboUseYN, CommonCombo.ComboStatus.NONE, sFilter: sFilter, sCase: "COMMCODE");

        }

        /// <summary>
        /// 컨트롤 초기값 설정
        /// </summary>
        private void InitControl()
        {
            if(string.Equals(LoginInfo.CFG_SHOP_ID,"G182") || LoginInfo.CFG_AREA_ID.Equals("S5"))
            {
                grdUseFlag.SetValue(Grid.ColumnProperty, 4);
                grdShipTo.Visibility = Visibility.Visible;
            }
        }
        #endregion

        #region Event
        /// <summary>
        /// Initializing 이후에 FormLoad시 Event를 생성.
        /// </summary>



        private void btnSearch_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            loadingIndicator.Visibility = Visibility.Visible;
        }

        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            Search();
        }



        #endregion

        #region Method
        /// <summary>
        /// 조회
        /// BIZ : BR_PRD_GET_INPALLET_FOR_SHIP_FM
        /// </summary>
        private void Search()
        {
            try
            {
                string bizRule = string.Empty;

                if (string.Equals(LoginInfo.CFG_SHOP_ID, "G182") || LoginInfo.CFG_AREA_ID.Equals("S5"))
                    bizRule = "BR_PRD_GET_PACK_COND_NJ";
                else
                    bizRule = "BR_PRD_GET_PACK_COND_FM";

                DataTable RQSTDT = new DataTable("INDATA");
                RQSTDT.Columns.Add("SHOPID", typeof(string));
                RQSTDT.Columns.Add("SHIPTO_ID", typeof(string));
                RQSTDT.Columns.Add("PRODID", typeof(string));
                RQSTDT.Columns.Add("PROJECT", typeof(string));
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("USE_FLAG", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                if (string.Equals(LoginInfo.CFG_SHOP_ID, "G182") || LoginInfo.CFG_AREA_ID.Equals("S5"))
                {
                    dr["SHOPID"] = LoginInfo.CFG_SHOP_ID;
                    dr["SHIPTO_ID"] = string.IsNullOrWhiteSpace(txtShipTo.Text) ? null : txtShipTo.Text;
                    dr["LANGID"] = LoginInfo.LANGID;
                }
                dr["PRODID"] = txtProdID.Text;
                dr["PROJECT"] = txtProject.Text;
                dr["USE_FLAG"] = cboUseYN.SelectedValue.ToString();
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync(bizRule, "INDATA", "OUTDATA", RQSTDT);
                Util.GridSetData(dgSearhResult, dtResult, FrameOperation);
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
