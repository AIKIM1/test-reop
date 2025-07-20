/*************************************************************************************
 Created Date : 2020.10.13
      Creator : 정문교
   Decription : LOT 변경이력 팝업
--------------------------------------------------------------------------------------
 [Change History]
  2020.10.13  정문교 : Initial Created.
    
**************************************************************************************/

using System;
using System.Data;
using System.Windows;
using C1.WPF;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;

namespace LGC.GMES.MES.CMM001.Popup
{
    /// <summary>
    /// CMM_LOT_CHANGE_HISTORY_DRB.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class CMM_LOT_CHANGE_HISTORY_DRB : C1Window, IWorkArea
    {
        #region Declaration & Constructor

        private string _lotID = string.Empty; 

        private readonly BizDataSet _bizDataSet = new BizDataSet();
        private readonly Util _util = new Util();

        #endregion

        #region Initialize
        /// <summary>
        ///  
        /// </summary>
        public IFrameOperation FrameOperation
        {
            get;
            set;
        }

        public CMM_LOT_CHANGE_HISTORY_DRB()
        {
            InitializeComponent();
        }

        private void InitializeUserControls()
        {
        }

        private void SetParameters()
        {
            object[] tmps = C1WindowExtension.GetParameters(this);

            _lotID = tmps[0] as string;

            txtLotID.Text = _lotID;
        }

        private void SetControl()
        {
        }

        private void SetCombo()
        {
        }

        private void SetDataGridColumnVisibility()
        {
        }

        #endregion

        #region Event

        /// <summary>
        /// Form Load
        /// </summary>
        private void C1Window_Loaded(object sender, RoutedEventArgs e)
        {
            InitializeUserControls();
            SetParameters();
            SetControl();
            SetCombo();
            SetDataGridColumnVisibility();

            // 조회
            SelectHistory();
        }

        /// <summary>
        /// 닫기
        /// </summary>
        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = MessageBoxResult.Cancel;
        }

        #endregion

        #region Mehod
        /// <summary>
        /// 조회
        /// </summary>
        private void SelectHistory()
        {
            try
            {
                DataTable inTable = new DataTable();
                inTable.Columns.Add("LANGID", typeof(string));
                inTable.Columns.Add("LOTID", typeof(string));

                // INDATA SET
                DataRow newRow = inTable.NewRow();
                newRow["LANGID"] = LoginInfo.LANGID;
                newRow["LOTID"] = _lotID;
                inTable.Rows.Add(newRow);

                ShowLoadingIndicator();

                new ClientProxy().ExecuteService("DA_PRD_SEL_WIPACTHISTORY", "INDATA", "OUTDATA", inTable, (bizResult, bizException) =>
                {
                    try
                    {
                        HiddenLoadingIndicator();

                        if (bizException != null)
                        {
                            Util.MessageException(bizException);
                            return;
                        }

                        Util.GridSetData(dgHistory, bizResult, null, true);
                    }
                    catch (Exception ex)
                    {
                        HiddenLoadingIndicator();
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

        #region [Func]

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