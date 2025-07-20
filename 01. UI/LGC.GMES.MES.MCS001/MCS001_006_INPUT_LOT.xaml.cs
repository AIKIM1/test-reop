/*************************************************************************************
 Created Date : 2018.11.01
      Creator : 오화백
   Decription : 입고LOT 조회
--------------------------------------------------------------------------------------
  [Change History]
  2021.08.03  오화백 : DA_MCS_SEL_LAMI_INPUT_LOT -> DA_MCS_SEL_LAMI_INPUT_LOT_POP
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

namespace LGC.GMES.MES.MCS001
{
	public partial class MCS001_006_INPUT_LOT : C1Window, IWorkArea
	{

        #region Declaration & Constructor 
        private string _PRJ_NAME = string.Empty;  //프로젝트명
        private string _POLARITY= string.Empty;   //극성
        private string _QAINSP = string.Empty;    // 구분
     
        public MCS001_006_INPUT_LOT() {
			InitializeComponent();
		}
		public IFrameOperation FrameOperation {
			get;
			set;
		}
		private void C1Window_Loaded( object sender, RoutedEventArgs e ) {
			ApplyPermissions();
            InitCombo();
            object[] parameters = C1WindowExtension.GetParameters(this);
            _PRJ_NAME = parameters[0] as string; //프로젝트명
            _POLARITY = parameters[1] as string; //극성
            _QAINSP = parameters[2] as string;   //구분

            txtPrj.Text = _PRJ_NAME;
            cboEltr.SelectedValue = _POLARITY;
            cboQa.SelectedValue = _QAINSP;

            SeachData();
            this.Loaded -= C1Window_Loaded;

        }

		/// <summary>
		/// 화면내 버튼 권한 처리
		/// </summary>
		private void ApplyPermissions() {
			List<Button> listAuth = new List<Button>();
			//listAuth.Add(btnInReplace);
			Util.pageAuth( listAuth, FrameOperation.AUTHORITY );
		}
        /// <summary>
        /// 콤보박스 
        /// </summary>
        private void InitCombo()
        {
            try
            {
                CommonCombo _combo = new CommonCombo();

                //극성
                String[] sFilter3 = { "ELTR_TYPE_CODE" };
                _combo.SetCombo(cboEltr, CommonCombo.ComboStatus.ALL, sFilter: sFilter3, sCase: "COMMCODE");
                //QA검사
                String[] sFilter1 = { "MCS_QA_INSP" };
                _combo.SetCombo(cboQa, CommonCombo.ComboStatus.ALL, sFilter: sFilter1, sCase: "COMMCODE");

              
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        #endregion

        #region Event

        /// <summary>
        /// 닫기
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = MessageBoxResult.Cancel;
        }

        /// <summary>
        /// 조회
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnSearch(object sender, RoutedEventArgs e)
        {
            SeachData();
        }
        #endregion

        #region Mehod
        /// <summary>
        /// 조회
        /// </summary>
        private void SeachData() {
            loadingIndicator.Visibility = Visibility.Visible;
            DataTable RQSTDT = new DataTable( "RQSTDT" );
			RQSTDT.Columns.Add( "LANGID", typeof( string ) );
            //RQSTDT.Columns.Add("INPUT_LOT_YN", typeof(string));
            RQSTDT.Columns.Add("PRJT_NAME", typeof(string));
            RQSTDT.Columns.Add("ELTR_TYPE_CODE", typeof(string));
            RQSTDT.Columns.Add("QA_VAL", typeof(string));

            DataRow dr = RQSTDT.NewRow();
			dr["LANGID"] = LoginInfo.LANGID;
            //dr["INPUT_LOT_YN"] = "Y";

            dr["PRJT_NAME"] = txtPrj.Text == string.Empty ? null : txtPrj.Text;
            dr["ELTR_TYPE_CODE"] = cboEltr.SelectedValue.ToString() == string.Empty ? null : cboEltr.SelectedValue;
            dr["QA_VAL"] = cboQa.SelectedValue.ToString() == string.Empty ? null : cboQa.SelectedValue;

            RQSTDT.Rows.Add( dr );

			new ClientProxy().ExecuteService("DA_MCS_SEL_LAMI_INPUT_LOT_POP", "RQSTDT", "RSLTDT", RQSTDT, ( result, exception ) => {
				try {
					if( exception != null ) {
                        loadingIndicator.Visibility = Visibility.Collapsed;
                        Util.MessageException( exception );
						return;
					}

					Util.GridSetData( dgList, result, FrameOperation,true);
				} catch( Exception ex ) {
					Util.MessageException( ex );
				} finally
                {
                    loadingIndicator.Visibility = Visibility.Collapsed;
                }

			} );
		}

        #endregion
      
    }
}