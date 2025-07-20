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
	public partial class MCS001_001_Layout : C1Window, IWorkArea
	{

		#region Declaration & Constructor 

		private string WH_ID;

		public MCS001_001_Layout() {
			InitializeComponent();
		}
		public IFrameOperation FrameOperation {
			get;
			set;
		}
		private void C1Window_Loaded( object sender, RoutedEventArgs e ) {
			ApplyPermissions();

            //Image.Source = new System.Windows.Media.Imaging.BitmapImage(new Uri(@"\LayoutSKIDbuffer.PNG", UriKind.Relative));         
		}

		/// <summary>
		/// 화면내 버튼 권한 처리
		/// </summary>
		private void ApplyPermissions() {
			List<Button> listAuth = new List<Button>();
			//listAuth.Add(btnInReplace);
			Util.pageAuth( listAuth, FrameOperation.AUTHORITY );
		}

		#endregion

		#region Event

		/// <summary>
		/// Initializing 이후에 FormLoad시 Event를 생성.
		/// </summary>
		private void SetEvent() {
			this.Loaded -= C1Window_Loaded;
		}

		

		#endregion
        
		

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = MessageBoxResult.Cancel;
        }


    }
}