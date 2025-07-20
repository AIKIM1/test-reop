/*************************************************************************************
 Created Date : 2024.11.07
      Creator : 오화백
   Decription : Lot List [팝업]
--------------------------------------------------------------------------------------
 [Change History]
  2024.11.07  오화백 : Initial Created.    
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
using System.Linq;
using System.Threading;
using System.Windows.Media;
using System.Windows.Threading;
using C1.WPF.DataGrid;
using LGC.GMES.MES.CMM001.Extensions;
using LGC.GMES.MES.CMM001.UserControls;
using System.Windows.Data;
using System.Windows.Media.Animation;
namespace LGC.GMES.MES.MTRL001
{
    public partial class MTRL001_218_LOTLIST : C1Window, IWorkArea
    {

		#region Declaration & Constructor 

        private string _CarrierID;
    
		public MTRL001_218_LOTLIST()
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

            object[] parameters = C1WindowExtension.GetParameters( this );

            if (parameters != null)
            {
                _CarrierID = Util.NVC(parameters[0]);
         
                SelectLotList();
            }

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

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

      
        #endregion

        #region Mehod

        private void SelectLotList()
        {
            const string bizRuleName = "DA_INV_SEL_DURABLE_LOT_INFO";
            try
            {
                DataTable dtResult = new DataTable();

                loadingIndicator.Visibility = Visibility.Visible;

                DataTable inTable = new DataTable("RQSTDT");
                inTable.Columns.Add("DURABLE_ID", typeof(string));


                DataRow dr = inTable.NewRow();
                dr["DURABLE_ID"] = _CarrierID;

                inTable.Rows.Add(dr);

                loadingIndicator.Visibility = Visibility.Visible;
                DataTable bizResult = new ClientProxy().ExecuteServiceSync(bizRuleName, "RQSTDT", "RSLTDT", inTable);
                if (bizResult.Rows.Count > 0)
                {
                    Util.GridSetData(dgList, bizResult, null, true);
                    loadingIndicator.Visibility = Visibility.Collapsed;

            
                }
                loadingIndicator.Visibility = Visibility.Collapsed;
            }
            catch (Exception ex)
            {
                loadingIndicator.Visibility = Visibility.Collapsed;
                Util.MessageException(ex);
            }
        }

      

        private void ShowLoadingIndicator()
        {
            if (loadingIndicator != null)
                loadingIndicator.Visibility = Visibility.Visible;
        }

        private void HiddenLoadingIndicator()
        {
            if (loadingIndicator != null)
                loadingIndicator.Visibility = Visibility.Collapsed;
        }


        #endregion

    
    }
}