/*************************************************************************************
 Created Date : 2021.04.13
      Creator : 
   Decription : 활성화 장기재고 관리
--------------------------------------------------------------------------------------
 [Change History]
  2021.04.13  DEVELOPER : Initial Created.





 
**************************************************************************************/

using C1.WPF;
using C1.WPF.DataGrid;
using C1.WPF.DataGrid.Summaries;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using LGC.GMES.MES.CMM001;
using LGC.GMES.MES.CMM001.Extensions;

namespace LGC.GMES.MES.FCS002
{

    public partial class FCS002_111 : UserControl, IWorkArea
    {
        Util Util = new Util();

        public IFrameOperation FrameOperation {
            get;
            set;
        }

        public FCS002_111()
        {
            InitializeComponent();
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            InitCombo();
            InitControl();

            this.Loaded -= UserControl_Loaded;
        }

        private void dgList_LoadedCellPresenter(object sender, DataGridCellEventArgs e)
        {
            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (e.Cell.Presenter == null) return;

                if (string.IsNullOrEmpty(e.Cell.Column.Name) == false)
                {
                    if (e.Cell.Column.Name.Equals("CSTID"))
                    {
                        e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Blue);
                        e.Cell.Presenter.FontWeight = FontWeights.Normal;
                    }
                    else
                    {
                        e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Black);
                        e.Cell.Presenter.FontWeight = FontWeights.Normal;
                    }
                }
            }));
        }

        private void dgList_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            try
            {
                if (dgList == null || dgList.CurrentRow == null || !dgList.Columns.Contains("CSTID"))
                    return;

                if (dgList.CurrentColumn.Name.Equals("CSTID"))
                {
                    FCS002_021 wndTRAY = new FCS002_021();
                    wndTRAY.FrameOperation = FrameOperation;

                    object[] Parameters = new object[10];
                    Parameters[0] = DataTableConverter.GetValue(dgList.CurrentRow.DataItem, "CSTID").ToString(); // TRAY ID
                    Parameters[1] = DataTableConverter.GetValue(dgList.CurrentRow.DataItem, "LOTID").ToString(); // LOTID ID
                    this.FrameOperation.OpenMenu("SFU010710300", true, Parameters);
                }
                
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }

        private void dgList_LoadedRowHeaderPresenter(object sender, C1.WPF.DataGrid.DataGridRowEventArgs e)
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

        private void InitCombo()
        {
            CommonCombo_Form_MB _combo = new CommonCombo_Form_MB();

            _combo.SetCombo(cboLine, CommonCombo_Form_MB.ComboStatus.ALL, sCase: "LINE");

            string[] sFilter = { "LONG_TERM_INV_KIND" };
            _combo.SetCombo(cboInvKind, CommonCombo_Form_MB.ComboStatus.ALL, sCase: "COMMCODE", sFilter: sFilter);

            //cboLotDetlTypeCode
            string[] sFilter1 = { "LOT_DETL_TYPE_CODE" };
            _combo.SetCombo(cboLotDetlTypeCode, CommonCombo_Form_MB.ComboStatus.ALL, sCase: "COMMCODE", sFilter: sFilter1);
        }

        private void InitControl()
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("AREAID", typeof(string));
                RQSTDT.Columns.Add("COM_TYPE_CODE", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["AREAID"] = LoginInfo.CFG_AREA_ID;
                dr["COM_TYPE_CODE"] = "PROC_GR_CODE_MB";

                RQSTDT.Rows.Add(dr);
                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_PROC_GR_ID_MB", "RQSTDT", "RSLTDT", RQSTDT, menuid: LoginInfo.CFG_MENUID);

                foreach (DataRow drResult in dtResult.Rows)
                {
                    CheckBox cbChk = new CheckBox();
                    cbChk.Tag = drResult["GR_ID"];
                    cbChk.Content = drResult["GR_NAME"];

                    if (drResult["SEARCH_YN"].ToString().Equals("N"))
                    {
                        cbChk.IsEnabled = false;
                        cbChk.Foreground = new SolidColorBrush(Colors.LightGray);
                    }
                    else
                    {
                        cbChk.IsChecked = true;
                    }
                    
                    lbProcGr.Items.Add(cbChk);
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }

        }

        private void GetList()
        {
            try
            {
                Util.gridClear(dgList);

                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("EQSGID", typeof(string));
                RQSTDT.Columns.Add("PROC_GR_CODE", typeof(string));
                RQSTDT.Columns.Add("LONG_TERM_INV_KIND", typeof(string));
                RQSTDT.Columns.Add("LOT_DETL_TYPE_CODE", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["EQSGID"] = Util.GetCondition(cboLine, bAllNull: true);
                dr["PROC_GR_CODE"] = GetCheckdGrade();
                dr["LONG_TERM_INV_KIND"] = Util.GetCondition(cboInvKind, bAllNull: true);
                dr["LOT_DETL_TYPE_CODE"] = Util.GetCondition(cboLotDetlTypeCode, bAllNull: true);
                RQSTDT.Rows.Add(dr);
                ShowLoadingIndicator();
                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_LONG_TERM_INV_LIST_MB", "RQSTDT", "RSLTDT", RQSTDT);

                Util.GridSetData(dgList, dtResult, FrameOperation, true);

                string[] sColumnName = new string[] { "LONG_TERM_INV_KIND_NAME", "EQSGNAME" };
                Util.SetDataGridMergeExtensionCol(dgList, sColumnName, DataGridMergeMode.VERTICALHIERARCHI);

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
            finally
            {
                HiddenLoadingIndicator();
            }
        }

        private string GetCheckdGrade()
        {
            string sCheck = "";
            foreach (CheckBox itemChecked in lbProcGr.Items)
            {
                if (itemChecked.IsChecked == true)
                {
                    if (sCheck != "")
                        sCheck += "," + itemChecked.Tag.ToString();
                    else
                        sCheck = itemChecked.Tag.ToString();
                }
            }

            return sCheck;
        }

        private void btnSearchClick(object sender, RoutedEventArgs e)
        {
            GetList();
        }

    }
}
