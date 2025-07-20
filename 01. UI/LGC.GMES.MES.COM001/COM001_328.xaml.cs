/*************************************************************************************
 Created Date : 2020.05.06
      Creator : 
   Decription : 슬러리 & 코팅 랏 맵핑 조회
--------------------------------------------------------------------------------------
 [Change History]
  2020.05.06  DEVELOPER : 김태균.

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

namespace LGC.GMES.MES.COM001
{
    public partial class COM001_328 : UserControl, IWorkArea
    {
        #region Declaration & Constructor 

        Util _Util = new Util();
        String _PRODID = "";

        public COM001_328()
        {
            InitializeComponent();
        }
        #endregion

        #region Initialize
        /// <summary>
        /// Frame과 상호작용하기 위한 객체
        /// </summary>
        public IFrameOperation FrameOperation
        {
            get;
            set;
        }

        private void Initialize()
        {
            dtpDateFrom.SelectedDateTime = (DateTime)System.DateTime.Now;
            dtpDateTo.SelectedDateTime = (DateTime)System.DateTime.Now;

            CommonCombo _combo = new CommonCombo();

            string[] sFilter = { LoginInfo.CFG_EQSG_ID, "E2000","DSC" };
            _combo.SetCombo(cboEquipment, CommonCombo.ComboStatus.NONE, sFilter: sFilter);
        }
        #endregion

        #region Event
        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                Initialize();
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            GetMappingData();
        }

        private void dgLotInfo_LoadedCellPresenter(object sender, DataGridCellEventArgs e)
        {
            dgLotInfo.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (e.Cell.Presenter == null)
                {
                    return;
                }

                if (((e.Cell.Column.Name.Equals("TOP1")) && Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "TOP1")).Equals("DUMMY"))
                 || ((e.Cell.Column.Name.Equals("TOP2")) && Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "TOP2")).Equals("DUMMY"))
                 || ((e.Cell.Column.Name.Equals("BACK1")) && Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "BACK1")).Equals("DUMMY"))
                 || ((e.Cell.Column.Name.Equals("BACK2")) && Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "BACK2")).Equals("DUMMY"))
                )
                {
                    e.Cell.Presenter.FontWeight = FontWeights.Bold;
                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Red);
                }
                else
                {
                    e.Cell.Presenter.FontWeight = FontWeights.Normal;
                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Black);
                }

            }));
        }

        private void dtpDate_SelectedDataTimeChanged(object sender, SelectionChangedEventArgs e)
        {
            if (dtpDateFrom.SelectedDateTime.Year > 1 && dtpDateTo.SelectedDateTime.Year > 1)
            {
                LGCDatePicker LGCdp = sender as LGCDatePicker;

                if ((dtpDateTo.SelectedDateTime - dtpDateFrom.SelectedDateTime).TotalDays > 31)
                {
                    //Util.AlertInfo("SFU2042", new object[] { "7" });   //기간은 {0}일 이내 입니다.
                    Util.MessageValidation("SFU2042", "31");

                    dtpDateFrom.SelectedDataTimeChanged -= dtpDate_SelectedDataTimeChanged;
                    dtpDateTo.SelectedDataTimeChanged -= dtpDate_SelectedDataTimeChanged;

                    if (LGCdp.Name.Equals("dtpDateTo"))
                        dtpDateTo.SelectedDateTime = dtpDateFrom.SelectedDateTime.AddDays(+30);
                    else
                        dtpDateFrom.SelectedDateTime = dtpDateTo.SelectedDateTime.AddDays(-30);

                    dtpDateFrom.SelectedDataTimeChanged += dtpDate_SelectedDataTimeChanged;
                    dtpDateTo.SelectedDataTimeChanged += dtpDate_SelectedDataTimeChanged;

                    return;
                }

                if ((dtpDateTo.SelectedDateTime - dtpDateFrom.SelectedDateTime).TotalDays < 0)
                {
                    dtpDateFrom.SelectedDateTime = dtpDateTo.SelectedDateTime;
                    return;
                }
            }
        }
        #endregion

        #region Method

        private void GetMappingData()
        {

            try
            {
                Util.gridClear(dgLotInfo);

                DataTable IndataTable = new DataTable();
                IndataTable.Columns.Add("LANGID", typeof(string));
                IndataTable.Columns.Add("FROM_DATE", typeof(string));
                IndataTable.Columns.Add("TO_DATE", typeof(string));
                IndataTable.Columns.Add("EQPTID", typeof(string));
                IndataTable.Columns.Add("EQSGID", typeof(string));
                IndataTable.Columns.Add("COM_TYPE_CODE", typeof(string));
                IndataTable.Columns.Add("LOTID", typeof(string));

                DataRow Indata = IndataTable.NewRow();
                Indata["LANGID"] = LoginInfo.LANGID;
                Indata["FROM_DATE"] = Util.GetCondition(dtpDateFrom);
                Indata["TO_DATE"] = Util.GetCondition(dtpDateTo);
                Indata["EQPTID"] = cboEquipment.SelectedValue.ToString();
                Indata["EQSGID"] = LoginInfo.CFG_EQSG_ID;
                Indata["COM_TYPE_CODE"] = "COATER_MIXER_MAP";

                if (!string.IsNullOrEmpty(txtCTLotID.Text.Trim()))
                    Indata["LOTID"] = txtCTLotID.Text.Trim();

                IndataTable.Rows.Add(Indata);

                ShowLoadingIndicator();
                new ClientProxy().ExecuteService("DA_PRD_SEL_MIX_COAT_MAPPING", "RQSTDT", "RSLTDT", IndataTable, (result, Exception) =>
                {
                    try
                    {
                        if (Exception != null)
                        {
                            LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(Exception), null, "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
                            return;
                        }

                        Util.GridSetData(dgLotInfo, result, FrameOperation, true);
                        
                    }
                    catch (Exception ex)
                    {
                        Util.MessageException(ex);
                    }
                    finally
                    {
                        HiddenLoadingIndicator();
                    }
                });


            }
            catch (Exception e)
            {
                Util.MessageException(e);
            }
        }

#endregion


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

    }
}
