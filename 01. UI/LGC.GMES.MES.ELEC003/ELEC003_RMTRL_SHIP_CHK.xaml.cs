/*************************************************************************************
 Created Date : 2023.04.20
      Creator : 강성묵
   Decription : 원자재 출고 요청시 진행중인 자재 존재 여부 체크
--------------------------------------------------------------------------------------
 [Change History]
 2023.04.20  강성묵 : Initial Created.
**************************************************************************************/

using C1.WPF;
using C1.WPF.DataGrid;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using System;
using System.Data;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Collections.Generic;

namespace LGC.GMES.MES.ELEC003
{
    /// <summary>
    /// ELEC003_RMTRL_SHIP_CHK
    /// </summary>
    public partial class ELEC003_RMTRL_SHIP_CHK : C1Window, IWorkArea
    {
        #region Declaration & Constructor

        Util _Util = new Util();
        
        #endregion

        #region Initialize
        /// <summary>
        ///  Frame과 상호작용하기 위한 객체
        /// </summary>
        public IFrameOperation FrameOperation { get; set; }

        public ELEC003_RMTRL_SHIP_CHK()
        {
            InitializeComponent();
        }

        #endregion

        #region Event
        private void C1Window_Loaded(object sender, RoutedEventArgs e)
        {
            //ApplyPermissions();
            txtComment1.Text = MessageDic.Instance.GetMessage("SFU9995");
            txtComment2.Text = MessageDic.Instance.GetMessage("SFU9996");

            object[] tmps = C1WindowExtension.GetParameters(this);

            if (tmps.Length >= 1)
            {
                try
                {
                    DataTable dtRmtrlShipList = (DataTable)tmps[0];

                    if (CommonVerify.HasTableRow(dtRmtrlShipList))
                    {
                        Util.GridSetData(dgInputRequest, dtRmtrlShipList, FrameOperation, true);
                    }
                }
                catch(Exception ex)
                {
                }
            }
        }

        #endregion

        #region Mehod
        /// <summary>
        /// 화면내 버튼 권한 처리
        /// </summary>
        private void ApplyPermissions()
        {
            //List<Button> listAuth = new List<Button>();
            ////listAuth.Add(btnInReplace);
            //Util.pageAuth(listAuth, FrameOperation.AUTHORITY);
        }

        #endregion

        private void btnOK_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = MessageBoxResult.OK;
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = MessageBoxResult.Cancel;
        }
    }
}