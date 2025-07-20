/*************************************************************************************
 Created Date : 2018.07.16
      Creator : 
   Decription : 파우치 활성화 불량 Cell 스캔 이력 조회
--------------------------------------------------------------------------------------
 [Change History]
  2018.03.06  DEVELOPER : Initial Created.

**************************************************************************************/

using LGC.GMES.MES.CMM001.Extensions;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;
using System.Windows.Media;
using LGC.GMES.MES.CMM001.Popup;
using LGC.GMES.MES.ControlsLibrary;
using System;
using System.Data;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Threading;
using C1.WPF.DataGrid;
using C1.WPF;
using System.Linq;
namespace LGC.GMES.MES.COM001
{
    public partial class COM001_244 : UserControl, IWorkArea
    {
        #region Declaration & Constructor 

        Util _Util = new Util();
        BizDataSet _Biz = new BizDataSet();

        public COM001_244()
        {
            InitializeComponent();
        }
        public IFrameOperation FrameOperation
        {
            get;
            set;
        }


        private void UserControl_Initialized(object sender, EventArgs e)
        {
            Initialize();
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            ApplyPermissions();
            txtSubLotID.Focus();

            this.Loaded -= UserControl_Loaded;
        }

        #endregion

        #region Initialize
        private void Initialize()
        {
            Util.gridClear(dgSubLotHistory);
        }

        #endregion

        #region Event
        //조회
        private void txtSubLotID_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                SubLotHistorySelect();

                txtSubLotID.SelectAll();
                txtSubLotID.Focus();
            }
        }

        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            SubLotHistorySelect();

            txtSubLotID.SelectAll();
            txtSubLotID.Focus();
        }

        #endregion

        #region Mehod
        /// <summary>
        /// SubLot ActHistory 조회
        /// </summary>
        private void SubLotHistorySelect()
        {
            try
            {
                if (string.IsNullOrWhiteSpace(txtSubLotID.Text))
                {
                    // CELL ID를 입력 하세요.
                    Util.MessageValidation("SFU1319");
                    return;
                }

                DataTable inTable = new DataTable();
                inTable.Columns.Add("LANGID", typeof(string));
                inTable.Columns.Add("SUBLOTID", typeof(string));

                // INDATA SET
                DataRow newRow = inTable.NewRow();
                newRow["LANGID"] = LoginInfo.LANGID;
                newRow["SUBLOTID"] = txtSubLotID.Text;
                inTable.Rows.Add(newRow);

                ShowLoadingIndicator();

                new ClientProxy().ExecuteService("DA_PRD_SEL_SUBLOT_ACTHISTORY", "INDATA", "OUTDATA", inTable, (result, bizException) =>
                {
                    HiddenLoadingIndicator();
                    try
                    {
                        if (bizException != null)
                        {
                            Util.MessageException(bizException);
                            return;
                        }

                        Util.GridSetData(dgSubLotHistory, result, FrameOperation, true);
                    }
                    catch (Exception ex)
                    {
                        Util.MessageException(ex);
                    }
                });
            }
            catch (Exception ex)
            {
                HiddenLoadingIndicator();
                Util.MessageException(ex);
            }
        }
        #endregion

        #region Func

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

        private void ApplyPermissions()
        {
            //List<Button> listAuth = new List<Button>
            //{
            //    btnSearch
            //};

            //Util.pageAuth(listAuth, FrameOperation.AUTHORITY);
        }


        #endregion

    }
}

