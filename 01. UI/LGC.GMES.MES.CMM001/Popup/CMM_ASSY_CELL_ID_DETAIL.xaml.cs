/*************************************************************************************
 Created Date : 2019.10.30
      Creator : 이상준
   Decription : Washing 공정진척 화면 - 추가기능 : Cell Id 상세정보 조회
--------------------------------------------------------------------------------------
 [Change History]
  2019.10.31  이상준 : Initial Created.
**************************************************************************************/


using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using C1.WPF;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.CMM001.Class;
using System.Data;
using LGC.GMES.MES.ControlsLibrary;

namespace LGC.GMES.MES.CMM001
{
    /// <summary>
    /// CMM_ASSY_CELL_ID_DETAIL.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class CMM_ASSY_CELL_ID_DETAIL : C1Window, IWorkArea
    {
        #region Declaration & Constructor         

        private BizDataSet _bizDataSet = new BizDataSet();
        private Util _util = new Util();
        private string _processCode = string.Empty;
        private bool _load = true;

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
        public CMM_ASSY_CELL_ID_DETAIL()
        {
            InitializeComponent();
        }
        #endregion

        #region Event
        private void C1Window_Initialized(object sender, EventArgs e)
        {
            try
            {
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void C1Window_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                if (_load)
                {
                    SetControl();
                    _load = false;
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void SetControl()
        {
            try
            {
                object[] tmps = C1WindowExtension.GetParameters(this);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }

        }

        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Search();
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

        private void txtCellID_KeyUp(object sender, KeyEventArgs e)
        {
            try
            {
                if (e.Key == Key.Enter)
                {
                    Search();
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        #endregion

        #region Mehod


        private void Search()
        {
            try
            {
                loadingIndicator.Visibility = Visibility.Visible;
                const string bizRuleName = "DA_BAS_SEL_SUBLOT_INFO_DETAIL";
                DataTable inDataTable = new DataTable();
                inDataTable.Columns.Add("LANGID", typeof(string));
                inDataTable.Columns.Add("SUBLOTID", typeof(string));

                DataRow dr = inDataTable.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["SUBLOTID"] = txtCellID.Text;
                
                inDataTable.Rows.Add(dr);

                new ClientProxy().ExecuteService(bizRuleName, "RQSTDT", "RSLTDT", inDataTable, (searchResult, searchException) =>
                {
                    loadingIndicator.Visibility = Visibility.Collapsed;
                    try
                    {
                        if (searchException != null)
                        {
                            Util.MessageException(searchException);
                            return;
                        }

                        Util.GridSetData(dgCell_Info, searchResult, FrameOperation, true);
                    }
                    catch (Exception ex)
                    {
                        Util.MessageException(ex);
                    }
                    finally
                    {
                        loadingIndicator.Visibility = Visibility.Collapsed;
                    }
                });

            }
            catch (Exception ex)
            {
                loadingIndicator.Visibility = Visibility.Collapsed;
                Util.MessageException(ex);
            }
        }
        
        #endregion


    }
}
