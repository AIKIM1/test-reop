/*************************************************************************************
 Created Date : 2017.05.22
      Creator : 이영준S
   Decription : 전지 5MEGA-GMES 구축 - 1차 포장 구성 - 작업시작 팝업
--------------------------------------------------------------------------------------
 [Change History]


**************************************************************************************/

using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Data;

using C1.WPF;
using C1.WPF.DataGrid;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.ControlsLibrary;
using System.ComponentModel;
using System.Threading;

namespace LGC.GMES.MES.BOX001
{
    /// <summary>
    /// BOX001_027_RUNSTART.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class BOX001_038_INBOX : C1Window, IWorkArea
    {
        string sLINEID = string.Empty;
        string sEQPTID = string.Empty;
        string sUSERID = string.Empty;
        string sSHFTID = string.Empty;

        public IFrameOperation FrameOperation
        {
            get;
            set;
        }

        public BOX001_038_INBOX()
        {
            InitializeComponent();
        }

        private void C1Window_Loaded(object sender, RoutedEventArgs e)
        {
            ApplyPermissions();

            object[] tmps = C1WindowExtension.GetParameters(this);

            if(tmps.Length>=4)
            {
                sLINEID = Util.NVC(tmps[0]);
                sEQPTID = Util.NVC(tmps[1]);
                sUSERID = Util.NVC(tmps[2]);
                sSHFTID = Util.NVC(tmps[2]);
            }
            else
            {
                sLINEID = "";
                sEQPTID = "";
                sUSERID = "";
                sSHFTID = "";
            }

        }

        private void btnOK_Click(object sender, RoutedEventArgs e)
        {

            Util.MessageConfirm("SFU1240", (result) =>
            {
                if (result == MessageBoxResult.OK)
                {
                   
                    ////txtLot.Text = sNewLot;
                 
                }
            });
        }


        private void ApplyPermissions()
        {
            List<Button> listAuth = new List<Button>();
            ////listAuth.Add(btnOutAdd);
            ////listAuth.Add(btnOutDel);

            Util.pageAuth(listAuth, FrameOperation.AUTHORITY);
        }

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = MessageBoxResult.Cancel;
        }
    }
}
