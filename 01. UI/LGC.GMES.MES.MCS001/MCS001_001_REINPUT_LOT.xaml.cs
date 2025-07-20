/*************************************************************************************
 Created Date : 2017.01.16
      Creator : 김재호 부장
   Decription : SKID BUFFER 모니터링
--------------------------------------------------------------------------------------
  
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
    public partial class MCS001_001_REINPUT_LOT : C1Window, IWorkArea
    {

		#region Declaration & Constructor 

		private string WH_ID;

		public MCS001_001_REINPUT_LOT()
        {
            InitializeComponent();
        }
        public IFrameOperation FrameOperation
        {
            get;
            set;
        }
        private void C1Window_Loaded(object sender, RoutedEventArgs e)
        {
            ApplyPermissions();

			object[] Parameters = C1WindowExtension.GetParameters( this );
			WH_ID = Parameters[0].ToString();

			SeachData();
        }

        /// <summary>
        /// 화면내 버튼 권한 처리
        /// </summary>
        private void ApplyPermissions()
        {
            List<Button> listAuth = new List<Button>();
            
            Util.pageAuth(listAuth, FrameOperation.AUTHORITY);
        }

        #endregion

        #region Event

        /// <summary>
        /// Initializing 이후에 FormLoad시 Event를 생성.
        /// </summary>
        private void SetEvent()
        {
            this.Loaded -= C1Window_Loaded;
			
        }

        private void OnBtnRefresh(object sender, RoutedEventArgs e)
        {
			SeachData();
        }

        #endregion

        #region Mehod
        private void SeachData()
        {
            DataTable RQSTDT = new DataTable("RQSTDT");
            RQSTDT.Columns.Add("LANGID", typeof(string));
            RQSTDT.Columns.Add( "WH_ID", typeof(string));

            DataRow dr = RQSTDT.NewRow();
            dr["LANGID"] = LoginInfo.LANGID;
            dr["WH_ID"] = WH_ID;
			RQSTDT.Rows.Add(dr);

			new ClientProxy().ExecuteService("DA_MCS_SEL_REINPUT_LOT_INFO", "RQSTDT", "RSLTDT", RQSTDT, ( result, exception ) => {
				try {
					if( exception != null ) {
						Util.MessageException( exception );
						return;
					}
					Util.GridSetData( dgList, result, FrameOperation );
				} catch( Exception ex ) {
					Util.MessageException( ex );
				} finally {
				}
			} );
		}
        #endregion        
    }
}