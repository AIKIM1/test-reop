/*************************************************************************************
 Created Date : 2019.12.19
      Creator : INS 김동일K
   Decription : Notched Pancake QA Sample Monitoring 화면 Group User control.
--------------------------------------------------------------------------------------
 [Change History]
  2019.12.19  INS 김동일K : Initial Created.
**************************************************************************************/

using System;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using C1.WPF.DataGrid;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using System.Collections.Generic;

namespace LGC.GMES.MES.ASSY004
{
    /// <summary>
    /// ASSY004_010_GRP.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class ASSY004_010_GRP : UserControl, IWorkArea
    {
        #region [Declaration & Constructor]
        Util _Util = new Util();
        UserControl _UCParent = null;

        public IFrameOperation FrameOperation { get; set; }

        public ASSY004_010_GRP(UserControl uc, IFrameOperation frmOperation)
        {
            InitializeComponent();
            this.Loaded += UserControl_Loaded;
            SetUCParent(uc);
            FrameOperation = frmOperation;
        }

        public string GRP_NAME { get; set; } = string.Empty;

        public string GRP_CODE { get; set; } = string.Empty;

        public string AREAID { get; set; } = string.Empty;

        public string EQSGID { get; set; } = string.Empty;

        public string PROCID { get; set; } = string.Empty;

        public string GRPATTR1 { get; set; } = string.Empty;

        public string GRPATTR2 { get; set; } = string.Empty;

        public string GRPATTR3 { get; set; } = string.Empty;

        public string GRPATTR4 { get; set; } = string.Empty;

        public string GRPATTR5 { get; set; } = string.Empty;

        private string UC_STATUS { get; set; } = string.Empty;  // P:진행중

        C1.WPF.DataGrid.DataGridRowHeaderPresenter pre = new C1.WPF.DataGrid.DataGridRowHeaderPresenter()
        {
            Background = new SolidColorBrush(Colors.Transparent),
            MouseOverBrush = new SolidColorBrush(Colors.Transparent),
        };
        #endregion

        #region[Event]
        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            this.Loaded -= UserControl_Loaded;
            
            ApplyPermissions();

            //GetList();
        }
        

        private void dgInfo_LoadedCellPresenter(object sender, DataGridCellEventArgs e)
        {
            C1DataGrid dataGrid = sender as C1DataGrid;

            if (e.Cell.Presenter == null || dataGrid.GetRowCount() == 0)
            {
                return;
            }

            dataGrid.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (e.Cell.Presenter == null)
                {
                    return;
                }

                //Grid Data Binding 이용한 Background 색 변경
                if (e.Cell.Row.Type == DataGridRowType.Item)
                {
                    string grpCode = Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "STAT_GRP_CODE"));

                    if (grpCode.Equals("CMPL"))
                    {
                        e.Cell.Presenter.Background = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#35B62C"));
                        e.Cell.Presenter.Foreground = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#FFFFFFFF"));
                    }
                    else if (grpCode.Equals("PROC"))
                    {
                        e.Cell.Presenter.Background = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#FFFFFF"));
                        e.Cell.Presenter.Foreground = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#FF000000"));
                    }
                    else if (grpCode.Equals("FAIL"))
                    {
                        e.Cell.Presenter.Background = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#FF0000"));
                        e.Cell.Presenter.Foreground = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#FFFFFFFF"));
                    }
                    else
                    {
                        e.Cell.Presenter.Background = null;
                        e.Cell.Presenter.Foreground = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#FF000000"));
                    }
                }
            }));
        }

        private void dgInfo_UnloadedCellPresenter(object sender, DataGridCellEventArgs e)
        {
            if (sender == null)
                return;

            C1.WPF.DataGrid.C1DataGrid dataGrid = sender as C1.WPF.DataGrid.C1DataGrid;

            dataGrid.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (e.Cell.Presenter != null)
                {
                    if (e.Cell.Row.Type == DataGridRowType.Item)
                    {
                        e.Cell.Presenter.Background = null;
                        e.Cell.Presenter.Foreground = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#FF000000"));
                    }
                }
            }));
        }

        #endregion

        #region[Method]        
        private void SetUCParent(UserControl uc)
        {
            _UCParent = uc;
        }

        public void GetList()
        {
            try
            {
                tbGrpName.Text = GRP_NAME;

                if (this.UC_STATUS == "P") return;

                this.UC_STATUS = "P";

                dgInfo.ItemsSource = null;

                loadingIndicator.Visibility = Visibility.Visible;

                DataTable inTable = new DataTable();
                inTable.Columns.Add("LANGID", typeof(string));
                inTable.Columns.Add("AREAID", typeof(string));
                inTable.Columns.Add("EQSGID", typeof(string));
                inTable.Columns.Add("PROCID", typeof(string));
                inTable.Columns.Add("GRP_CODE", typeof(string));

                DataRow newRow = inTable.NewRow();
                newRow["LANGID"] = LoginInfo.LANGID;
                newRow["AREAID"] = AREAID;
                newRow["EQSGID"] = EQSGID.Equals("") ? null : EQSGID;
                newRow["PROCID"] = PROCID.Equals("") ? null : PROCID;
                newRow["GRP_CODE"] = GRP_CODE;

                inTable.Rows.Add(newRow);

                new ClientProxy().ExecuteService("DA_PRD_SEL_VD_QA_SMPL_MNT_LIST", "INDATA", "OUTDATA", inTable, (searchResult, searchException) =>
                {
                    try
                    {
                        if (searchException != null)
                        {
                            Util.MessageException(searchException);
                            return;
                        }
                        
                        Util.GridSetData(dgInfo, searchResult, FrameOperation, true);


                        //if (searchResult.Rows.Count > 0)
                            dgInfo.Visibility = Visibility.Visible;
                    }
                    catch (Exception ex)
                    {
                        Util.MessageException(ex);
                    }
                    finally
                    {
                        loadingIndicator.Visibility  = Visibility.Collapsed;
                        this.UC_STATUS = "";
                    }
                }
                );                
            }
            catch (Exception ex)
            {
                loadingIndicator.Visibility = Visibility.Collapsed;
                this.UC_STATUS = "";
                Util.MessageException(ex);
            }
        }
                
        #endregion

        #region [Init & Util Method]

        private void ApplyPermissions()
        {
            List<Button> listAuth = new List<Button>();


            Util.pageAuth(listAuth, FrameOperation.AUTHORITY);
        }

        public void ClearData()
        {
            GRP_NAME = string.Empty;
            AREAID = string.Empty;
            EQSGID = string.Empty;
            PROCID = string.Empty;
            GRPATTR1 = string.Empty;
            GRPATTR2 = string.Empty;
            GRPATTR3 = string.Empty;
            GRPATTR4 = string.Empty;
            GRPATTR5 = string.Empty;

            tbGrpName.Text = string.Empty;            
            dgInfo.ItemsSource = null;
        }
        #endregion
        
    }
}
