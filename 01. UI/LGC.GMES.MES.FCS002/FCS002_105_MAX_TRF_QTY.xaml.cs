/*************************************************************************************
 Created Date : 2021.04.06
      Creator : 조영대
   Decription : 활성화 외부 보관 Box/Pallet 구성 - 모델별 Box구성 Cell수량 기준
--------------------------------------------------------------------------------------
 [Change History]
  2021.04.06  조영대 : Initial Created

**************************************************************************************/

using C1.WPF;
using C1.WPF.DataGrid;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.ControlsLibrary;
using LGC.GMES.MES.Common;
using System;
using System.Collections.Generic;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using System.Linq;

namespace LGC.GMES.MES.FCS002
{
    public partial class FCS002_105_MAX_TRF_QTY : C1Window, IWorkArea
    {
        #region Declaration & Constructor         
        private string _AreaID = string.Empty;

        #endregion

        #region Initialize 
        /// <summary>
        ///  Frame과 상호작용하기 위한 객체
        /// </summary>
        public IFrameOperation FrameOperation
        {
            get;
            set;
        }

        public FCS002_105_MAX_TRF_QTY()
        {
            InitializeComponent();
        }
        #endregion

        #region Event


        private void C1Window_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                object[] tmps = C1WindowExtension.GetParameters(this);

                if (tmps != null && tmps.Length >= 1)
                {
                    _AreaID = Util.NVC(tmps[0]);
                }
                else
                {
                    _AreaID = "";
                }

                ApplyPermissions();

                GetMaxTrfQtyList();
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void btnClose_Clicked(object sender, RoutedEventArgs e)
        {
            this.DialogResult = MessageBoxResult.Cancel;
        }

        #endregion

        #region Mehod

        private void ApplyPermissions()
        {
            List<Button> listAuth = new List<Button>();
            
            Util.pageAuth(listAuth, FrameOperation.AUTHORITY);
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
        
        private void GetMaxTrfQtyList()
        {
            try
            {
                ShowLoadingIndicator();

                DataTable inTable = new DataTable("RQSTDT");
                inTable.Columns.Add("AREAID", typeof(string));

                DataRow newRow = inTable.NewRow();
                newRow["AREAID"] = _AreaID;

                inTable.Rows.Add(newRow);

                new ClientProxy().ExecuteService("DA_PRD_SEL_OUTER_WH_STAND_INFO_MB", "INDATA", "OUTDATA", inTable, (searchResult, searchException) =>
                {
                    if (searchException != null)
                    {
                        HiddenLoadingIndicator();
                        Util.MessageException(searchException);
                        return;
                    }
                    
                    dgMaxTrfQtyList.SetItemsSource(searchResult, FrameOperation, true);

                    HiddenLoadingIndicator();
                });
            }
            catch (Exception ex)
            {
                HiddenLoadingIndicator();
                Util.MessageException(ex);
            }

        }
        
        #endregion

    
    }
}
