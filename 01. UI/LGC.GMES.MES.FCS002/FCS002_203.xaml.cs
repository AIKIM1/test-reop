/*************************************************************************************
 Created Date : 2023.01.26
      Creator : KANG DONG HEE
   Decription : 전체 Aging Rack 현황
--------------------------------------------------------------------------------------
 [Change History]
------------------------------------------------------------------------------
     Date     |   NAME   |                  DESCRIPTION
------------------------------------------------------------------------------
  2023.01.26  DEVELOPER : Initial Created.
**************************************************************************************/
#define SAMPLE_DEV

using C1.WPF;
using C1.WPF.DataGrid;
using C1.WPF.DataGrid.Summaries;
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
using LGC.GMES.MES.CMM001;

namespace LGC.GMES.MES.FCS002
{
    public partial class FCS002_203 : UserControl, IWorkArea
    {
        #region Declaration & Constructor 

        /// <summary>
        /// Frame과 상호작용하기 위한 객체
        /// </summary>
        public IFrameOperation FrameOperation
        {
            get;
            set;
        }
        #endregion

        #region Initialize
        public FCS002_203()
        {
            InitializeComponent();
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            //Combo Setting
            InitCombo();
            this.Loaded -= UserControl_Loaded;
        }

        //화면내 combo 셋팅
        private void InitCombo()
        {
            CommonCombo_Form ComCombo = new CommonCombo_Form();

            C1ComboBox[] cbChild = { cboAgingType };
            ComCombo.SetCombo(cboBldgCd, CommonCombo_Form.ComboStatus.ALL, sCase: "BLDG", cbChild: cbChild);

            object[] objParent = { "FORM_AGING_TYPE_CODE", cboBldgCd };
            ComCombo.SetComboObjParent(cboAgingType, CommonCombo_Form.ComboStatus.ALL, sCase: "SYSTEM_AREA_COMMON_CODE", objParent: objParent);
        }
        #endregion

        #region Event
        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            GetList();
        }

        private void chkOnlyAll_Checked(object sender, RoutedEventArgs e)
        {
            btnSearch_Click(null, null);
        }

        private void chkOnlyAll_Unchecked(object sender, RoutedEventArgs e)
        {
            btnSearch_Click(null, null);
        }

        private void dgAgingStatus_LoadedCellPresenter(object sender, DataGridCellEventArgs e)
        {
            if (sender == null)
                return;

            C1DataGrid dg = sender as C1DataGrid;

            dg?.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (e.Cell.Presenter == null)
                {
                    return;
                }

                if (e.Cell.Row.Type == DataGridRowType.Item)
                {
                    string sROW = Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "ROW"));
                    ////////////////////////////////////////////  default 색상 및 Cursor
                    e.Cell.Presenter.Cursor = Cursors.Arrow;

                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Black);
                    e.Cell.Presenter.FontSize = 12;
                    e.Cell.Presenter.FontWeight = FontWeights.Normal;
                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Transparent);
                    ///////////////////////////////////////////////////////////////////////////////////

                    if (!chkOnlyAll.IsChecked.Equals(true) && sROW.Equals("ALL"))
                    {
                        e.Cell.Presenter.Background = new SolidColorBrush(Colors.Aquamarine);
                        e.Cell.Presenter.FontWeight = FontWeights.Bold;
                    }
                }
            }));
        }

        #endregion

        #region Method
        private void GetList()
        {
            try
            {
                DataTable dtRqst = new DataTable();
                dtRqst.TableName = "INDATA";
                dtRqst.Columns.Add("LANGID", typeof(string));
                dtRqst.Columns.Add("AGING_TYPE", typeof(string));
                dtRqst.Columns.Add("ONLY_ALL_YN", typeof(string));
                dtRqst.Columns.Add("BLDG_CD", typeof(string));

                DataRow dr = dtRqst.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["AGING_TYPE"] = Util.GetCondition(cboAgingType, bAllNull: true);
                dr["ONLY_ALL_YN"] = ((bool)chkOnlyAll.IsChecked) ? "Y" : "N";
                dr["BLDG_CD"] = Util.GetCondition(cboBldgCd, bAllNull: true);
                dtRqst.Rows.Add(dr);

                DataSet dsRqst = new DataSet();
                dsRqst.Tables.Add(dtRqst);

                ShowLoadingIndicator();
                new ClientProxy().ExecuteService_Multi("BR_GET_AGING_RACK_STATUS_MB", "INDATA", "OUTDATA,OUTDATA_ALL", (bizResult, bizException) =>
                {
                    try
                    {
                        if (bizException != null)
                        {
                            Util.MessageException(bizException);
                            return;
                        }

                        if (bizResult.Tables["OUTDATA"].Rows.Count > 0 && bizResult.Tables["OUTDATA_ALL"].Rows.Count > 0)
                        {
                            if (!chkOnlyAll.IsChecked.Equals(true))
                            {
                                dgAgingStatus.Columns["EQPT_NAME"].Visibility = Visibility.Visible;
                                Util.GridSetData(dgAgingStatus, bizResult.Tables["OUTDATA"], FrameOperation, true);
                            }
                            else
                            {
                                dgAgingStatus.Columns["EQPT_NAME"].Visibility = Visibility.Collapsed;
                                Util.GridSetData(dgAgingStatus, bizResult.Tables["OUTDATA_ALL"], FrameOperation, true);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Util.MessageException(ex);
                    }
                    finally
                    {
                        HiddenLoadingIndicator();
                    }
                }, dsRqst);
            }
            catch (Exception ex)
            {
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
