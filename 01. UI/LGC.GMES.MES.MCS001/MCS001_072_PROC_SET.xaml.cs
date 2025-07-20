/*************************************************************************************
 Created Date : 2022.05.25
      Creator : 곽란영
   Decription : VD 대기 창고 모니터링 - 제품별 창고 선택 팝업 신규 추가
--------------------------------------------------------------------------------------
 [Change History]
  2022.05.25  곽란영 대리 : Initial Created.
  
**************************************************************************************/

using System;
using System.Windows;
using System.Windows.Controls;
using System.Data;
using C1.WPF;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.ControlsLibrary;
using System.Windows.Media;
using Microsoft.Win32;
using System.Configuration;
using C1.WPF.Excel;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;
using LGC.GMES.MES.CMM001;
using LGC.GMES.MES.CMM001.Extensions;

namespace LGC.GMES.MES.MCS001
{
    /// <summary>
    /// MCS001_072_PROC_SET.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class MCS001_072_PROC_SET : C1Window, IWorkArea
    {
        private Util _Util = new Util();

        private DataTable _DtSearchInfo = new DataTable();

        private const string AuthGroup = "LOGIS_MANA";// 물류관리자

        C1.WPF.DataGrid.DataGridRowHeaderPresenter pre = new C1.WPF.DataGrid.DataGridRowHeaderPresenter()
        {
            Background = new SolidColorBrush(Colors.Transparent),
            MouseOverBrush = new SolidColorBrush(Colors.Transparent),
        };
        
        CheckBox chkAll = new CheckBox()
        {
            IsChecked = false,
            Background = new SolidColorBrush(Colors.Transparent),
            VerticalAlignment = System.Windows.VerticalAlignment.Center,
            HorizontalAlignment = System.Windows.HorizontalAlignment.Center

        };

        public MCS001_072_PROC_SET()
        {
            InitializeComponent();
        }

        public IFrameOperation FrameOperation
        {
            get;
            set;
        }

        private void C1Window_Initialized(object sender, System.EventArgs e)
        {

        }

        private void C1Window_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {
            InitializeCombo();
            InitControl();
            SelectVdWhList();
        }

        private void btnAdd_Click(object sender, RoutedEventArgs e)
        {
            //DataTable dt = DataTableConverter.Convert(dgHold.ItemsSource);
            //dt.Columns["USE_FLAG"].DefaultValue = "Y";
            //dt.Columns["EQPTID"].DefaultValue = cboStocker.SelectedValue.ToString();
            //DataRow dr = dt.NewRow();

            //dt.Rows.Add(dr);

            //Util.GridSetData(dgHold, dt, FrameOperation);
            //dgHold.ScrollIntoView(dt.Rows.Count - 1, 0);

            _DtSearchInfo.Rows.Add();

        }

        private void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            //DataTable dt = DataTableConverter.Convert(dgHold.ItemsSource);

            //List<DataRow> drList = dt.Select("CHK = 'True'")?.ToList();
            //if (drList.Count > 0)
            //{
            //    foreach (DataRow dr in drList)
            //    {
            //        dt.Rows.Remove(dr);
            //    }
            //    Util.GridSetData(dgHold, dt, FrameOperation);
            //    chkAll.IsChecked = false;
            //}
            //tbTotCount.Text = dgHold.GetRowCount().ToString();
        }

        private void btnDelete2_Click(object sender, RoutedEventArgs e)
        {
            List<int> list = _Util.GetDataGridCheckRowIndex(dgHold, "CHK");
            if (list.Count <= 0)
            {
                // 선택된 항목이 없습니다.
                Util.MessageValidation("SFU1651");
                return;
            }

            // 삭제하시겠습니까?
            Util.MessageConfirm("FM_ME_0155", (result) =>
            {
                if (result == MessageBoxResult.OK)
                {
                    CMM_COM_AUTH_CONFIRM popupAuthConfirm = new CMM_COM_AUTH_CONFIRM { FrameOperation = FrameOperation };

                    object[] parameters = new object[1];
                    parameters[0] = AuthGroup;
                    C1WindowExtension.SetParameters(popupAuthConfirm, parameters);

                    popupAuthConfirm.Closed += popupChangeProd_Delete_Closed;
                    Dispatcher.BeginInvoke(new Action(() => popupAuthConfirm.ShowModal()));
                }
            });
        }


        private void dgHold_LoadedColumnHeaderPresenter(object sender, C1.WPF.DataGrid.DataGridColumnEventArgs e)
        {
            try
            {
                this.Dispatcher.BeginInvoke(new Action(() =>
                {
                    if (string.IsNullOrEmpty(e.Column.Name) == false)
                    {
                        if (e.Column.Name.Equals("CHK"))
                        {
                            pre.Content = chkAll;
                            //e.Column.HeaderPresenter.Content = pre;
                            chkAll.Checked -= new RoutedEventHandler(checkAll_Checked);
                            chkAll.Unchecked -= new RoutedEventHandler(checkAll_Unchecked);
                            chkAll.Checked += new RoutedEventHandler(checkAll_Checked);
                            chkAll.Unchecked += new RoutedEventHandler(checkAll_Unchecked);
                        }
                    }
                }));
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return;
            }
        }

        private void SelectVdWhList()
        {
            try
            {
                loadingIndicator.Visibility = Visibility.Visible;

                const string bizRuleName = "DA_MCS_SEL_VD_WH_PRDT_NJ";

                DataTable inTable = new DataTable("RQSTDT");
                inTable.Columns.Add("AREAID", typeof(string));
                inTable.Columns.Add("EQPTID", typeof(string));
                inTable.Columns.Add("PRODID", typeof(string));

                DataRow dr = inTable.NewRow();
                dr["AREAID"] = LoginInfo.CFG_AREA_ID;
                dr["EQPTID"] = cboStocker.SelectedValue;
                dr["PRODID"] = string.IsNullOrEmpty(txtProductId.Text.Trim()) ? null : txtProductId.Text.Trim();
                inTable.Rows.Add(dr);

                new ClientProxy().ExecuteService(bizRuleName, "RQSTDT", "RSLTDT", inTable, (_DtSearchInfo, bizException) =>
                {
                    loadingIndicator.Visibility = Visibility.Collapsed;
                    try
                    {
                        if (bizException != null)
                        {
                            Util.MessageException(bizException);
                            return;
                        }

                        Util.GridSetData(dgHold, _DtSearchInfo, null, false);
                    }
                    catch (Exception ex)
                    {
                        loadingIndicator.Visibility = Visibility.Collapsed;
                        Util.MessageException(ex);
                    }
                });
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void btnSave_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            // 저장하시겠습니까?
            Util.MessageConfirm("SFU1241", (result) =>
            {
                if (result == MessageBoxResult.OK)
                {
                    CMM_COM_AUTH_CONFIRM popupAuthConfirm = new CMM_COM_AUTH_CONFIRM { FrameOperation = FrameOperation };

                    object[] parameters = new object[1];
                    parameters[0] = AuthGroup;
                    C1WindowExtension.SetParameters(popupAuthConfirm, parameters);

                    popupAuthConfirm.Closed += popupChangeProd_Save_Closed;
                    Dispatcher.BeginInvoke(new Action(() => popupAuthConfirm.ShowModal()));
                }
            });
        }

        private void btnClose_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            this.DialogResult = MessageBoxResult.Cancel;
            this.Close();
        }

        private void popupChangeProd_Save_Closed(object sender, EventArgs e)
        {
            CMM_COM_AUTH_CONFIRM popup = sender as CMM_COM_AUTH_CONFIRM;
            if (popup != null && popup.DialogResult == MessageBoxResult.OK)
            {
                try
                {
                    Save("C");
                }
                catch (Exception ex)
                {
                    Util.MessageException(ex);
                }
            }
        }

        private void popupChangeProd_Delete_Closed(object sender, EventArgs e)
        {
            CMM_COM_AUTH_CONFIRM popup = sender as CMM_COM_AUTH_CONFIRM;
            if (popup != null && popup.DialogResult == MessageBoxResult.OK)
            {
                try
                {
                    Save("D");
                }
                catch (Exception ex)
                {
                    Util.MessageException(ex);
                }
            }
        }

        private void checkAll_Checked(object sender, RoutedEventArgs e)
        {
            if ((bool)chkAll.IsChecked)
            {
                for (int i = 0; i < dgHold.GetRowCount(); i++)
                {
                    // 기존 저장자료는 제외
                    if (string.IsNullOrWhiteSpace(Util.NVC(DataTableConverter.GetValue(dgHold.Rows[i].DataItem, "CHK"))) || Util.NVC(DataTableConverter.GetValue(dgHold.Rows[i].DataItem, "CHK")).Equals("0") || Util.NVC(DataTableConverter.GetValue(dgHold.Rows[i].DataItem, "CHK")).Equals(bool.FalseString))
                        DataTableConverter.SetValue(dgHold.Rows[i].DataItem, "CHK", true);
                }
            }
        }
        private void checkAll_Unchecked(object sender, RoutedEventArgs e)
        {
            if (!(bool)chkAll.IsChecked)
            {
                for (int i = 0; i < dgHold.GetRowCount(); i++)
                {
                    DataTableConverter.SetValue(dgHold.Rows[i].DataItem, "CHK", false);
                }
            }
        }


        private void InitControl()
        {
            _DtSearchInfo = new DataTable();

            for (int i = 0; i < dgHold.Columns.Count; i++)
            {
                _DtSearchInfo.Columns.Add(dgHold.Columns[i].Name);
            }

            Util.GridSetData(dgHold, _DtSearchInfo, FrameOperation);

            _DtSearchInfo.Clear();
        }

        private void Save(string flag)
        {
            try
            {
                //DA_MCS_MERGE_VD_WH_PRDT_NJ
                const string bizRuleName = "BR_MCS_CHK_VD_WH_PRDT_NJ";

                DataSet ds = new DataSet();
                DataTable inDataTable = ds.Tables.Add("INDATA");
                inDataTable.Columns.Add("AREAID", typeof(string));
                inDataTable.Columns.Add("PRODID", typeof(string));
                inDataTable.Columns.Add("WH_ID", typeof(string));
                inDataTable.Columns.Add("EQPTID", typeof(string));
                inDataTable.Columns.Add("USE_FLAG", typeof(string));
                inDataTable.Columns.Add("USERID", typeof(string));
                inDataTable.Columns.Add("LANGID", typeof(string));

                DataRow newRow = inDataTable.NewRow();

                if (flag == "C")
                {
                    newRow = inDataTable.NewRow();
                    newRow["AREAID"] = LoginInfo.CFG_AREA_ID;
                    newRow["PRODID"] = txtProductId2.Text;
                    newRow["WH_ID"] = GetWhId();
                    newRow["EQPTID"] = cboStocker2.SelectedValue.ToString();
                    newRow["USE_FLAG"] = "Y";
                    newRow["USERID"] = LoginInfo.USERID;
                    newRow["LANGID"] = LoginInfo.LANGID;
                    inDataTable.Rows.Add(newRow);
                }
                else if (flag == "D")
                {
                    DataTable dtInfo = DataTableConverter.Convert(dgHold.ItemsSource);

                    for (int row = 0; row < dtInfo.Rows.Count; row++)
                    {
                        if (dtInfo.Rows[row]["CHK"].ToString() == "1")
                        {
                            newRow = inDataTable.NewRow();
                            newRow["AREAID"] = LoginInfo.CFG_AREA_ID;
                            newRow["PRODID"] = dtInfo.Rows[row]["PRODID"];
                            newRow["WH_ID"] = dtInfo.Rows[row]["WH_ID"];
                            newRow["EQPTID"] = dtInfo.Rows[row]["EQPTID"];
                            newRow["USE_FLAG"] = "N";
                            newRow["USERID"] = LoginInfo.USERID;
                            newRow["LANGID"] = LoginInfo.LANGID;
                            inDataTable.Rows.Add(newRow);
                        }
                    }
                }

                new ClientProxy().ExecuteService_Multi(bizRuleName, "INDATA", null, (bizResult, bizException) =>
                {
                    try
                    {
                        if (bizException != null)
                        {
                            Util.MessageException(bizException);
                            return;
                        }

                        Util.MessageInfo("SFU1275"); //정상 처리 되었습니다.

                        cboStocker.SelectedValue = cboStocker2.SelectedValue.ToString();
                        txtProductId.Text = null;
                        txtProductId2.Text = null;
                        // 재조회 처리
                        SelectVdWhList();
                    }
                    catch (Exception ex)
                    {
                        Util.MessageException(ex);
                    }
                }, ds);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }            
        }

        private void InitializeCombo()
        {
            // Stocker 콤보박스
            SetStockerCombo(cboStocker);
            SetStockerCombo(cboStocker2);
        }

        private static void SetStockerCombo(C1ComboBox cbo)
        {
            //const string bizRuleName = "DA_MCS_SEL_MCS_WAREHOUSE_CBO";
            //string[] arrColumn = { "LANGID", "SHOPID", "EQGRID", "AREAID" };
            //string[] arrCondition = { LoginInfo.LANGID, LoginInfo.CFG_SHOP_ID, "VWW", LoginInfo.CFG_AREA_ID };
            const string bizRuleName = "DA_MCS_SEL_MCS_WAREHOUSE_CBO_NJ";
            string[] arrColumn = { "LANGID", "SHOPID", "AREAID" };
            string[] arrCondition = { LoginInfo.LANGID, LoginInfo.CFG_SHOP_ID, LoginInfo.CFG_AREA_ID };
            string selectedValueText = cbo.SelectedValuePath;
            string displayMemberText = cbo.DisplayMemberPath;

            CommonCombo.CommonBaseCombo(bizRuleName, cbo, arrColumn, arrCondition, CommonCombo.ComboStatus.NONE, selectedValueText, displayMemberText, LoginInfo.CFG_EQPT_ID);
        }

        private void btnProductId_Click(object sender, RoutedEventArgs e)
        {
            GetProductWindow();
        }

        private void GetProductWindow()
        {
            //COM001_009 참고
            //CMM_PERSON wndPerson = new CMM_PERSON();
            //wndPerson.FrameOperation = FrameOperation;

            //if (wndPerson != null)
            //{
            //    object[] Parameters = new object[1];
            //    Parameters[0] = txtUserName.Text;
            //    C1WindowExtension.SetParameters(wndPerson, Parameters);

            //    wndPerson.Closed += new EventHandler(wndUser_Closed);
            //    grdMain.Children.Add(wndPerson);
            //    wndPerson.BringToFront();
            //}
        }

        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            SelectVdWhList();
        }

        private void cboStocker_SelectedItemChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            SelectVdWhList();
        }

        private string GetWhId()
        {
            string whid = string.Empty;

            DataTable RQSTDT = new DataTable();
            RQSTDT.TableName = "RQSTDT";
            RQSTDT.Columns.Add("LANGID", typeof(string));
            RQSTDT.Columns.Add("CMCDTYPE", typeof(string));
            RQSTDT.Columns.Add("CMCODE", typeof(string));

            DataRow dr = RQSTDT.NewRow();
            dr["LANGID"] = LoginInfo.LANGID;
            dr["CMCDTYPE"] = "VD_EQPT_WH_MNGT";
            dr["CMCODE"] = cboStocker2.SelectedValue.ToString();

            RQSTDT.Rows.Add(dr);

            DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_COMMONCODE_TBL", "RQSTDT", "RSLTDT", RQSTDT);

            if (dtResult.Rows.Count > 0)
            {
                whid = dtResult.Rows[0]["ATTRIBUTE1"].ToString();
                return whid;
            }

            return whid;
        }

        private void txtProductId_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                SelectVdWhList();
            }
        }
    }
}
