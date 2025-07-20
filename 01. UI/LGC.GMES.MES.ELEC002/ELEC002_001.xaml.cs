/*****************************************
 Created Date : 2019.10.02
      Creator : JEONG
   Decription : 믹서 공정진척(예정)
------------------------------------------
 [Change History]
 2019-10-11   : BASEFORM UI분리
******************************************/
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Data;
using System.Globalization;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.CMM001;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.CMM001.UserControls;
using LGC.GMES.MES.ControlsLibrary;
using LGC.GMES.MES.CMM001.Popup;

namespace LGC.GMES.MES.ELEC002
{
    public partial class ELEC002_001 : UserControl, IWorkArea
    {
        private Util _Util = new Util();

        public ELEC002_001()
        {
            InitializeComponent();
        }

        public IFrameOperation FrameOperation { get; set; }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            ApplyPermissions();
        }

        #region Event
        private void ApplyPermissions()
        {
            List<Button> listAuth = new List<Button>();
            Util.pageAuth(listAuth, FrameOperation.AUTHORITY);
        }
        #endregion
    }
}