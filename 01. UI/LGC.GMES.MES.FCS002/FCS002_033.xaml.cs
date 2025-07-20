/*************************************************************************************
 Created Date : 2020.10.15
      Creator : Dooly
   Decription : Lot Hold 및 특성 투입여부
--------------------------------------------------------------------------------------
 [Change History]
  2020.10.15  DEVELOPER : Initial Created.


       


 
**************************************************************************************/

using C1.WPF;
using C1.WPF.DataGrid;
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
using System.Linq;
using System.Windows.Threading;

namespace LGC.GMES.MES.FCS002
{
    public partial class FCS002_033 : UserControl, IWorkArea
    {
        #region Declaration & Constructor
        Util _Util = new Util();
        private string _SHOPID = string.Empty;
        public FCS002_033()
        {
            InitializeComponent();
        }
        public IFrameOperation FrameOperation
        {
            get;
            set;
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            List<Button> listAuth = new List<Button>();
            Util.pageAuth(listAuth, FrameOperation.AUTHORITY);

            Initialize();

            this.Loaded -= UserControl_Loaded;
        }

        #endregion

        #region Initialize
        private void Initialize()
        {
            CommonCombo_Form_MB _combo = new CommonCombo_Form_MB();

            ////동
            //C1ComboBox[] cboAreaChild = { cboEquipmentSegment };
            //_combo.SetCombo(cboArea, CommonCombo_Form_MB.ComboStatus.ALL, cbChild: cboAreaChild);

            ////Login 한 AREA Setting
            //cboArea.SelectedValue = LoginInfo.CFG_AREA_ID;

            ////라인
            //C1ComboBox[] cboLineParent = { cboArea };
            //_combo.SetCombo(cboEquipmentSegment, CommonCombo_Form_MB.ComboStatus.ALL, cbParent: cboLineParent);

            //라인
            C1ComboBox[] cboLineChild = { cboModel };
            _combo.SetCombo(cboEquipmentSegment, CommonCombo_Form_MB.ComboStatus.ALL, sCase: "LINE", cbChild: cboLineChild);

            //모델
            C1ComboBox[] cboModelParent = { cboEquipmentSegment };
            _combo.SetCombo(cboModel, CommonCombo_Form_MB.ComboStatus.ALL, sCase: "LINEMODEL", cbParent: cboModelParent);

            string[] sFilter = { "RELEASE_YN" };
            _combo.SetCombo(cboRelease, CommonCombo_Form_MB.ComboStatus.ALL, sCase: "COMMCODE", sFilter: sFilter);

            //날짜
            dtpFrom.SelectedDateTime = System.DateTime.Now;
            dtpTo.SelectedDateTime = System.DateTime.Now.AddDays(1);

        }
        #endregion

        #region Event

        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            GetList();
        }

        private void btnEolInputYN_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string sMsgID = string.Empty;
                Button btnEolInput = sender as Button;
                if (btnEolInput != null)
                {
                    DataRowView dataRow = ((FrameworkElement)sender).DataContext as DataRowView;

                    if (string.Equals(btnEolInput.Name, "btnEolInputYN"))
                    {
                        DataTable RQSTDT = new DataTable();
                        RQSTDT.TableName = "RQSTDT";
                        RQSTDT.Columns.Add("HOLD_ID", typeof(string));
                        RQSTDT.Columns.Add("FORM_UNHOLD_FLAG", typeof(string));
                        RQSTDT.Columns.Add("USERID", typeof(string));

                        DataRow dr = RQSTDT.NewRow();
                        dr["HOLD_ID"] = Util.NVC(dataRow.Row["HOLD_ID"]);
                        //dr["HOLD_ID"] = Util.NVC(dataRow.Row["MDF_ID"]);
                        dr["USERID"] = LoginInfo.USERID;

                        if ((Util.NVC(dataRow.Row["FORM_UNHOLD_FLAG"]) == "Y") || (Util.NVC(dataRow.Row["UNHOLD_FLAG"]) == "Y"))
                        {
                            dr["FORM_UNHOLD_FLAG"] = "N";
                            sMsgID = "FM_ME_0332";  //특성 작업 가능 설정을 취소하시겠습니까?
                        }
                        else
                        {
                            dr["FORM_UNHOLD_FLAG"] = "Y";
                            sMsgID = "FM_ME_0247";  //특성 작업이 가능하도록 변경하시겠습니까?
                        }
                        RQSTDT.Rows.Add(dr);

                        //요청하시겠습니까?
                        Util.MessageConfirm(sMsgID, (result) =>
                        {
                            if (result == MessageBoxResult.OK)
                            {
                                ShowLoadingIndicator();
                                DataTable Result = new ClientProxy().ExecuteServiceSync("DA_UPD_TC_LOT_HOLD_PACK_FCS_MB", "RQSTDT", "RSLTDT", RQSTDT);
                                GetList();
                            }
                        });
                    }

                }
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
            finally
            {
                HiddenLoadingIndicator();
            }
        }

        private void dgLotHoldOCV_LoadedCellPresenter(object sender, C1.WPF.DataGrid.DataGridCellEventArgs e)
        {
            C1.WPF.DataGrid.C1DataGrid dataGrid = sender as C1.WPF.DataGrid.C1DataGrid;

            dataGrid.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (e.Cell.Presenter == null)
                {
                    return;
                }

                //특성 투입 가능 설정 버튼 Control
                if (e.Cell.Column.Name.Equals("EOLINPUT"))
                {
                    //Button btn = dataGrid.GetCell(e.Cell.Row.Index, e.Cell.Column.Index).Presenter.Content as Button;

                    if (Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "UNHOLD_FLAG")).Equals("Y"))
                    {
                        e.Cell.Presenter.IsEnabled = false;
                        //btn.Visibility = Visibility.Hidden;
                    }
                    else
                    {
                        e.Cell.Presenter.IsEnabled = true;
                        //btn.Visibility = Visibility.Visible;
                    }
                }
            }));
        }

        private void dgLotHoldOCV_LoadedRowHeaderPresenter(object sender, C1.WPF.DataGrid.DataGridRowEventArgs e)
        {
            if (e.Row.HeaderPresenter == null)
            {
                return;
            }

            e.Row.HeaderPresenter.Content = null;
            C1DataGrid dataGrid = e.Row.DataGrid as C1DataGrid;
            TextBlock tb = new TextBlock();

            if (e.Row.Index - (dataGrid.TopRows.Count - 1) > 0 && e.Row.Index <= dataGrid.Rows.Count - 1)
            {
                tb.Text = (e.Row.Index + 1 - dataGrid.TopRows.Count).ToString();
                tb.VerticalAlignment = VerticalAlignment.Center;
                tb.HorizontalAlignment = HorizontalAlignment.Center;
                e.Row.HeaderPresenter.Content = tb;
            }
        }
        #endregion

        #region Mehod
        private void GetList()
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("FROM_DATE", typeof(string));
                RQSTDT.Columns.Add("TO_DATE", typeof(string));
                RQSTDT.Columns.Add("EQSGID", typeof(string));
                RQSTDT.Columns.Add("MDLLOT_ID", typeof(string));
                RQSTDT.Columns.Add("HOLD_ID", typeof(string));
                RQSTDT.Columns.Add("UNHOLD_FLAG", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["FROM_DATE"] = dtpFrom.SelectedDateTime.ToString("yyyyMMdd");
                dr["TO_DATE"] = dtpTo.SelectedDateTime.ToString("yyyyMMdd");
                dr["EQSGID"] = Util.GetCondition(cboEquipmentSegment, bAllNull: true); 
                dr["MDLLOT_ID"] = Util.GetCondition(cboModel, bAllNull: true); 
                dr["HOLD_ID"] = Util.GetCondition(txtHoldId);
                dr["UNHOLD_FLAG"] = Util.GetCondition(cboRelease, bAllNull: true);
                RQSTDT.Rows.Add(dr);

                ShowLoadingIndicator();
                DataTable SearchResult = new ClientProxy().ExecuteServiceSync("DA_SEL_LOTHOLD_LIST_MB", "RQSTDT", "RSLTDT", RQSTDT);

                Util.GridSetData(dgLotHoldOCV, SearchResult, FrameOperation, true);
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
            finally
            {
                HiddenLoadingIndicator();
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
