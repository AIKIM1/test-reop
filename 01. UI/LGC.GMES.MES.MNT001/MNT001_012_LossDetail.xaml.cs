/*************************************************************************************
 Created Date : 2018.05.16
      Creator : 손우석
   Decription : 
--------------------------------------------------------------------------------------
 [Change History]  
 2018.05.16  손우석 최초 생성
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
using System.Windows.Forms;

namespace LGC.GMES.MES.MNT001
{
    public partial class MNT001_012_LossDetail : C1Window, IWorkArea
    {
        #region Declaration & Constructor 
        private string _LossCode = string.Empty;
        private string _EqptId = string.Empty;
        private string _WrkDate = string.Empty;

        public MNT001_012_LossDetail()
        {
            InitializeComponent();

            this.Width =  Screen.PrimaryScreen.Bounds.Width;  
            this.Height =  Screen.PrimaryScreen.Bounds.Height-40;
        }
        public IFrameOperation FrameOperation
        {
            get;
            set;
        }
        private void C1Window_Loaded(object sender, RoutedEventArgs e)
        {
            object[] tmps = C1WindowExtension.GetParameters(this);
            _LossCode = (string)tmps[0];
            _EqptId = (string)tmps[1];
            _WrkDate = (string)tmps[2];

            ApplyPermissions();
            SeachData();
        }

       // 화면내 버튼 권한 처리
        private void ApplyPermissions()
        {
           // List<Button> listAuth = new List<Button>();
            //listAuth.Add(btnInReplace);
           // Util.pageAuth(listAuth, FrameOperation.AUTHORITY);
        }
        #endregion

        #region Event
        // Initializing 이후에 FormLoad시 Event를 생성.
        private void SetEvent()
        {
            this.Loaded -= C1Window_Loaded;          
        }
        #endregion

        #region Mehod
        private void SeachData()
        {
            DataTable RQSTDT = new DataTable();
            RQSTDT.TableName = "RQSTDT";
            RQSTDT.Columns.Add("LANGID", typeof(string));
            RQSTDT.Columns.Add("LOSS_CODE", typeof(string));
            RQSTDT.Columns.Add("EQPTID", typeof(string));
            RQSTDT.Columns.Add("WRK_DATE", typeof(string));

            DataRow dr = RQSTDT.NewRow();
            dr["LANGID"] = LoginInfo.LANGID;
            dr["LOSS_CODE"] = _LossCode;
            dr["EQPTID"] = _EqptId; 
            dr["WRK_DATE"] = _WrkDate;
            RQSTDT.Rows.Add(dr);

            new ClientProxy().ExecuteService("DA_EQP_SEL_EQPTLOSS_SUMMARY_DETAIL_MNT", "RQSTDT", "RSLTDT", RQSTDT, (result, exception) =>
            {
                try
                {
                    if (result == null)
                    {
                        LGC.GMES.MES.ControlsLibrary.MessageBox.Show(exception.Message, null, "Error", MessageBoxButton.OK, ControlsLibrary.MessageBoxIcon.Warning);
                        return;
                    }

                    Util.GridSetData(dgLossDetail, result, FrameOperation,true);
                }
                catch (Exception ex)
                {
                    LGC.GMES.MES.ControlsLibrary.MessageBox.Show(ex.Message, null, "Error", MessageBoxButton.OK, ControlsLibrary.MessageBoxIcon.Warning);
                }
                finally
                {

                }
            });
        }
        #endregion
    }
}