/*************************************************************************************
 Created Date : 2016.06.16
      Creator : 
   Decription : 
--------------------------------------------------------------------------------------
 [Change History]
  2016.06.16  DEVELOPER : Initial Created.
  2017.03.08  용하옥    
  2017.03.14  정문교      생산실적 조회 화면 코터공정에서 Slurry 탭 제거 바랍니다
  2017.04.27  정문교      라미, 폴딩 감열지 발행시 DISPATCH_YN 가 N인 경우 자동 DISPATCH 추가
**************************************************************************************/

using C1.WPF;
using C1.WPF.DataGrid;
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

namespace LGC.GMES.MES.COM001
{
    public partial class COM001_200 : UserControl, IWorkArea
    {
        #region Declaration & Constructor 
        private const string _BizRule = "DA_PRD_SEL_PJT_DAY";

        /// <summary>
        /// Frame과 상호작용하기 위한 객체
        /// </summary>
        public IFrameOperation FrameOperation
        {
            get;
            set;
        }

        public COM001_200()
        {
            InitializeComponent();
        }

        #endregion

        #region Initialize
        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            ApplyPermissions();
            InitCombo();
            InitGrid();
        }

        #endregion

        #region Event

           
        private void btnSearch_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            loadingIndicator.Visibility = Visibility.Visible;
        }
        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            Search();
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

       

        private void InitGrid()
        {
            Util.gridClear(dgLOTYDESC);
        }


        private void InitCombo()
        {
            CommonCombo _combo = new CommonCombo();

           
            //극성
            String[] sFilter1 = { "ELEC_TYPE" };
            _combo.SetCombo(CMCDNAME, CommonCombo.ComboStatus.ALL, sFilter: sFilter1, sCase: "COMMCODE");
        }
          




        private void Search()
        {

            for (int i = dgLOTYDESC.Columns.Count; i-- > 8;) //컬럼수
                dgLOTYDESC.Columns.RemoveAt(i);

           

            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("LOTYDESC", typeof(string));
                RQSTDT.Columns.Add("PJTNAME", typeof(string));
                RQSTDT.Columns.Add("PROD_VER_CODE", typeof(string));
                RQSTDT.Columns.Add("CMCDNAME", typeof(string));
                RQSTDT.Columns.Add("AREAID", typeof(string));
                RQSTDT.Columns.Add("VERCHK", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["AREAID"] = LoginInfo.CFG_AREA_ID;
                dr["PJTNAME"] = Util.NVC(PJT_NAME.Text.Trim());
                dr["PROD_VER_CODE"] = Util.NVC(PROD_VER_CODE.Text.Trim());
                dr["CMCDNAME"] = Util.GetCondition(CMCDNAME, bAllNull: true);
                if (chkVerson.IsChecked.ToString().Equals("True"))
                {
                    dr["VERCHK"] = "Cheked";
                }

                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync(_BizRule, "RQSTDT", "RSLTDT", RQSTDT);





                dgLOTYDESC.ItemsSource = null;


                if (dtResult != null && dtResult.Rows.Count > 0)
                {
                    dgLOTYDESC.ItemsSource = DataTableConverter.Convert(dtResult);

                  //  Util.GridSetData(dgLOTYDESC, dtResult, FrameOperation, true);
                }

                
            }

            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(ex.Message, null, "Error", MessageBoxButton.OK, ControlsLibrary.MessageBoxIcon.Warning);
            }
            finally
            {
                loadingIndicator.Visibility = Visibility.Collapsed;
            }
        }



        #endregion

        private void chkVerson_Checked(object sender, RoutedEventArgs e)
        {
            Search();
        }

        private void chkVerson_Unchecked(object sender, RoutedEventArgs e)
        {
            Search();
        }
    }
}