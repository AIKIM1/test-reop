/*************************************************************************************
 Created Date : 2023.02.10
      Creator : 
   Decription : 
--------------------------------------------------------------------------------------
 [Change History]
  2023.02.10  DEVELOPER : Initial Created.
 
**************************************************************************************/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using C1.WPF;
using LGC.GMES.MES.Common;
using System.Data;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.ControlsLibrary;
using System.Windows.Media;
using C1.WPF.DataGrid;
using System.Windows.Input;

namespace LGC.GMES.MES.FCS002
{
    /// <summary>
    /// TSK_112.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class FCS002_021_REL_JUDG : C1Window, IWorkArea
    {
        #region [Declaration & Constructor]
        private string _sTrayId = string.Empty;
        private string _sLotId = string.Empty;

        public FCS002_021_REL_JUDG()
        {
            InitializeComponent();
        }
        public IFrameOperation FrameOperation
        {
            get;
            set;
        }
        #endregion

        #region [Initialize]
        private void C1Window_Loaded(object sender, RoutedEventArgs e)
        {
            object[] tmps = C1WindowExtension.GetParameters(this);
            if (tmps != null && tmps.Length >= 1)
            {
                _sTrayId = Util.NVC(tmps[0]);
                _sLotId = Util.NVC(tmps[1]);
            }
            else
            {
                _sTrayId = string.Empty;
                _sLotId = string.Empty;
            }



            txtTrayID.Text = _sTrayId;
            txtLotID.Text = _sLotId;
            GetOp();
            GetList();
        }
        #endregion

        #region [Method]
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

        private void checkAll_Checked(object sender, RoutedEventArgs e)
        {
            DataTable dt = DataTableConverter.Convert(dgOp.ItemsSource);
            foreach (DataRow dr in dt.Rows)
            {
                dr["CHK"] = true;
            }
            dgOp.ItemsSource = DataTableConverter.Convert(dt);

        }

        private void checkAll_Unchecked(object sender, RoutedEventArgs e)
        {
            DataTable dt = DataTableConverter.Convert(dgOp.ItemsSource);
            foreach (DataRow dr in dt.Rows)
            {
                dr["CHK"] = false;
            }
            dgOp.ItemsSource = DataTableConverter.Convert(dt);

        }

        private bool GetOp()
        {
            try
            {
                DataTable dtRqst = new DataTable();
                dtRqst.TableName = "RQSTDT";
                dtRqst.Columns.Add("LANGID", typeof(string));
                dtRqst.Columns.Add("LOTID", typeof(string));

                DataRow dr = dtRqst.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["LOTID"] = _sLotId;

                dtRqst.Rows.Add(dr);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_SEL_LOAD_TRAY_OPER_MB", "RQSTDT", "RSLTDT", dtRqst);

                if (dtRslt.Rows.Count > 0)
                {
                    Util.GridSetData(dgOp, dtRslt, FrameOperation, true);
                    return true;
                }
                else
                {
                    Util.MessageInfo("SFU1905"); //"조회된 Data가 없습니다."
                    return false;
                }
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
            return true;
        }

        private void GetList()
        {
            try
            {
                Util.gridClear(dgCellData);
                int iChk = 0;

                DataTable dtRqst = new DataTable();
                dtRqst.TableName = "INDATA";
                dtRqst.Columns.Add("LANGID", typeof(string));
                dtRqst.Columns.Add("LOTID", typeof(string));
                dtRqst.Columns.Add("CSTID", typeof(string));
                dtRqst.Columns.Add("PROCID", typeof(string));

                DataRow dr = dtRqst.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["LOTID"] = _sLotId;
                if(!_sTrayId.Equals(string.Empty))
                    dr["CSTID"] = _sTrayId;

                for (int i = 0; i < dgOp.Rows.Count; i++)
                {
                    if (Util.NVC(DataTableConverter.GetValue(dgOp.Rows[i].DataItem, "CHK")).Equals("True")
                     || Util.NVC(DataTableConverter.GetValue(dgOp.Rows[i].DataItem, "CHK")).Equals("1"))
                    {
                        dr["PROCID"] += Util.NVC(DataTableConverter.GetValue(dgOp.Rows[i].DataItem, "PROCID")) + ",";
                        iChk++;
                    }
                }
                dtRqst.Rows.Add(dr);

                if (iChk == 0)
                {
                    Util.MessageValidation("FM_ME_0165");  //선택된 데이터가 없습니다.
                }
                else
                {
                    ShowLoadingIndicator();
                    DataTable dtRslt = new ClientProxy().ExecuteServiceSync("BR_GET_LOAD_TRAY_INFO_RJUDG_MB", "INDATA", "OUTDATA", dtRqst);

                    Util.GridSetData(dgCellData, dtRslt, FrameOperation, true);
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
        #endregion

        #region [Event]
        private void txtTrayID_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                GetOp();
                GetList();
            }
        }

        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                GetList();
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }

        private void dgOp_LoadedColumnHeaderPresenter(object sender, C1.WPF.DataGrid.DataGridColumnEventArgs e)
        {
            if (e.Column.HeaderPresenter == null)
            {
                return;
            }

            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                try
                {
                    if (string.IsNullOrEmpty(e.Column.Name) == false)
                    {
                        if (e.Column.Name.Equals("CHK"))
                        {
                            pre.Content = chkAll;

                            e.Column.HeaderPresenter.Content = pre;
                            chkAll.Checked -= new RoutedEventHandler(checkAll_Checked);
                            chkAll.Unchecked -= new RoutedEventHandler(checkAll_Unchecked);
                            chkAll.Checked += new RoutedEventHandler(checkAll_Checked);
                            chkAll.Unchecked += new RoutedEventHandler(checkAll_Unchecked);
                        }
                    }
                }
                catch (Exception ex)
                {
                    LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
                }

            }));
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
