/*************************************************************************************
  Created Date : 2020.10.07
      Creator : ����ȿ
   Decription : ���� ������ �̵�
--------------------------------------------------------------------------------------
 [Change History]
  2020.10.07  ����ȿ : Initial Created.  
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
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using LGC.GMES.MES.CMM001;
using LGC.GMES.MES.CMM001.Extensions;

namespace LGC.GMES.MES.ASSY004
{
    public partial class ASSY004_008_MANUAL_WAIT : C1Window, IWorkArea
    {
        #region Declaration & Constructor 

        public ASSY004_008_MANUAL_WAIT()
        {
            InitializeComponent();
        }

        public IFrameOperation FrameOperation
        {
            get;
            set;
        }

        #endregion


        #region Initialize

        #endregion


        #region Event

        private void C1Window_Loaded(object sender, RoutedEventArgs e)
        {
            //����� ���Ѻ��� ��ư �����
            List<Button> listAuth = new List<Button>();

            Util.pageAuth(listAuth, FrameOperation.AUTHORITY);
            //������� ����� ���Ѻ��� ��ư �����
        }

        #endregion


        #region Method

        #endregion
    }
}