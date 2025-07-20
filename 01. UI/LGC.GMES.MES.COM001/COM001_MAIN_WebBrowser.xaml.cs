/*************************************************************************************
 Created Date : 2024.05.14
      Creator : kor21cman
   Decription : GMES WebBrowser
--------------------------------------------------------------------------------------
 [Change History]
  2024.07.03  조범모 : E20231011-000895 GMES Main 화면에서 WebBrowser 호출

**************************************************************************************/

using System;
using C1.WPF;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using System.Reflection;
using System.Windows.Controls;

namespace LGC.GMES.MES.COM001
{
    public partial class COM001_MAIN_WebBrowser : C1Window, IWorkArea
    {
        #region Declaration & Constructor 
        object[] parameter = null;

        public COM001_MAIN_WebBrowser()
        {
            InitializeComponent();

            this.Loaded += UserControl_Loaded;
        }
        #endregion

        #region Initialize
        /// <summary>
        /// Frame과 상호작용하기 위한 객체
        /// </summary>
        public IFrameOperation FrameOperation { get; set; }


        #endregion

        #region Event
        private void UserControl_Loaded(object sender, EventArgs e)
        {
            parameter = C1WindowExtension.GetParameters(this);

            myWebBrowser.Navigate(parameter[0].ToString());
            HideScriptErrors(myWebBrowser, true);
        }
        #endregion

        #region Method
        private static void HideScriptErrors(WebBrowser wb, bool Hide)
        {
            FieldInfo fiComWebBrowser = typeof(WebBrowser).GetField("_axIWebBrowser2", BindingFlags.Instance | BindingFlags.NonPublic);

            if (fiComWebBrowser == null) return;
            object objComWebBrowser = fiComWebBrowser.GetValue(wb);

            if (objComWebBrowser == null) return;

            objComWebBrowser.GetType().InvokeMember(
                "Silent", BindingFlags.SetProperty, null, objComWebBrowser,
                new object[] { Hide });
        }
        #endregion

    }
}
 