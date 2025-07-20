/*************************************************************************************
 Created Date : 2016.11.14
      Creator : 
   Decription : 
--------------------------------------------------------------------------------------
 [Change History]
  2016.11.14  DEVELOPER : Initial Created.





 
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

namespace LGC.GMES.MES.COM001
{
    public partial class COM001_073_UNUSUAL : C1Window, IWorkArea
    {
        #region Declaration & Constructor 

        private string _sWHID = string.Empty;

        public COM001_073_UNUSUAL()
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
            object[] tmps = C1WindowExtension.GetParameters(this);
            _sWHID = (string)tmps[0];

            ApplyPermissions();
            SeachData();
        }

        /// <summary>
        /// 화면내 버튼 권한 처리
        /// </summary>
        private void ApplyPermissions()
        {
            List<Button> listAuth = new List<Button>();
            //listAuth.Add(btnInReplace);
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

        #endregion


        #region Mehod
        private void SeachData()
        {
            DataTable RQSTDT = new DataTable("RQSTDT");
            RQSTDT.Columns.Add("LANGID", typeof(string));
            RQSTDT.Columns.Add("AREAID", typeof(string));
            RQSTDT.Columns.Add("WH_ID", typeof(string));

            DataRow dr = RQSTDT.NewRow();
            dr["AREAID"] = LoginInfo.LANGID;
            dr["AREAID"] = LoginInfo.CFG_AREA_ID;
            dr["WH_ID"] = _sWHID;

            RQSTDT.Rows.Add(dr);

            new ClientProxy().ExecuteService("DA_PRD_SEL_STOCK_BY_SEARCH_UNUSUAL", "RQSTDT", "RSLTDT", RQSTDT, (result, exception) =>
            {
                try
                {
                    if (result == null)
                    {
                        LGC.GMES.MES.ControlsLibrary.MessageBox.Show(exception.Message, null, "Error", MessageBoxButton.OK, ControlsLibrary.MessageBoxIcon.Warning);
                        return;
                    }

                    Util.GridSetData(dgList, result, FrameOperation);
                }
                catch (Exception ex)
                {
                    LGC.GMES.MES.ControlsLibrary.MessageBox.Show(ex.Message, null, "Error", MessageBoxButton.OK, ControlsLibrary.MessageBoxIcon.Warning);
                    //Logger.Instance.WriteLine(Logger.OPERATION_R + "GetProductLot", ex);
                }
                finally
                {
                    //Logger.Instance.WriteLine(Logger.OPERATION_R + "GetProductLot", Logger.MESSAGE_OPERATION_END);
                }

            });
        }
        #endregion
    }
}
