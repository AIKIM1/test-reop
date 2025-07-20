/*************************************************************************************
 Created Date : 2017.12.14
      Creator : 이진선
   Decription : 설비Loss입력률
--------------------------------------------------------------------------------------
 [Change History]
  2017.11.09  DEVELOPER : Initial Created.





 
**************************************************************************************/

using C1.WPF;
using C1.WPF.DataGrid;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Collections.ObjectModel;
using System.Windows.Media.Imaging;
using System.IO;
using System.Reflection;



namespace LGC.GMES.MES.COM001
{
    public partial class COM001_205 : UserControl, IWorkArea
    {

        #region Declaration & Constructor 

        CommonCombo combo = new CommonCombo();

        

        public COM001_205()
        {
            InitializeComponent();
        }

        public IFrameOperation FrameOperation
        {
            get;
            set;
        }

        #endregion

        #region [EVENT]
        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            Loaded -= UserControl_Loaded;

            List<Button> listAuth = new List<Button>();
            Util.pageAuth(listAuth, FrameOperation.AUTHORITY);


            initCombo();


        }

     


        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            if (sender == null)
                return;

            SearchData();
        }

        private void dgLossMst_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (sender == null)
                return;

            if (dgLossMst.CurrentCell != null)
            {
                string wrkdate = Util.NVC(DataTableConverter.GetValue(dgLossMst.CurrentCell.Row.DataItem, "WRK_DATE"));
                int idx = dgLossMst.CurrentCell.Row.Index;

                if (wrkdate.Equals("N/A"))
                {
                    return;
                }
                GetLossDetail(idx);
            }

        }
        private void dgLossMst_LoadedCellPresenter(object sender, DataGridCellEventArgs e)
        {
            e.Cell.Presenter.Foreground = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("black"));

            if (e.Cell.Column.Name.Equals("WRK_DATE"))
            {
                e.Cell.Presenter.Foreground = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("blue"));
            }
        }

        private void dgLossHist_LoadedCellPresenter(object sender, DataGridCellEventArgs e)
        {
            C1.WPF.DataGrid.C1DataGrid dataGrid = sender as C1.WPF.DataGrid.C1DataGrid;

            dataGrid.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (e.Cell.Presenter == null)
                {
                    return;
                }
                //Grid Data Binding 이용한 Background 색 변경
                if (e.Cell.Row.Type == DataGridRowType.Item)
                {
                    string sCheck = Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "CHECK_DELETE"));
                    string loss_code = Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "LOSS_CODE"));
                    string loss_detl_code = Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "LOSS_DETL_CODE"));

                    if (sCheck.Equals("DELETE"))
                    {
                        System.Drawing.Color color = GridBackColor.Color4;
                        e.Cell.Presenter.Background = new SolidColorBrush(System.Windows.Media.Color.FromArgb(color.A, color.R, color.G, color.B));
                    }
                    else if (!sCheck.Equals("DELETE") && !loss_code.Equals("") && !loss_detl_code.Equals(""))
                    {
                        e.Cell.Presenter.Background = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("lightBlue"));
                    }
                    else
                    {
                        System.Drawing.Color color = GridBackColor.Color6;
                        e.Cell.Presenter.Background = new SolidColorBrush(System.Windows.Media.Color.FromArgb(color.A, color.R, color.G, color.B)); ;
                    }

                }

                //link 색변경
                if (e.Cell.Column.Name != null)
                {
                    if (e.Cell.Column.Name.Equals("CHECK_DELETE"))
                    {
                        e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Blue);
                    }
                    else if (e.Cell.Column.Name.Equals("SPLIT"))
                    {
                        e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Blue);
                    }
                    else
                    {
                        e.Cell.Presenter.Foreground = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#FF333333"));
                    }
                }

            }));
        }
        #endregion

        #region [Method]
        private void initCombo()
        {
            //동
            C1ComboBox[] cboAreaChild = { cboEquipmentSegment };
            combo.SetCombo(cboArea, CommonCombo.ComboStatus.NONE, cbChild: cboAreaChild);

            //라인
            C1ComboBox[] cboEquipmentSegmentParent = { cboArea };
            C1ComboBox[] cboEquipmentSegmentChild = {  cboProcess, cboEquipment };
            combo.SetCombo(cboEquipmentSegment, CommonCombo.ComboStatus.NONE, cbChild: cboEquipmentSegmentChild, cbParent: cboEquipmentSegmentParent);

            //공정
            C1ComboBox[] cboProcessParent = { cboEquipmentSegment };
            C1ComboBox[] cboProcessChild = {  cboEquipment };
            combo.SetCombo(cboProcess, CommonCombo.ComboStatus.ALL, cbChild: cboProcessChild, cbParent: cboProcessParent);

            //설비
            C1ComboBox[] cboEquipmentParent = { cboEquipmentSegment, cboProcess };
            combo.SetCombo(cboEquipment, CommonCombo.ComboStatus.ALL, cbParent: cboEquipmentParent);

        }

        private void SearchData()
        {
            try
            {
                DataTable dt = new DataTable();
                dt.Columns.Add("LANGID", typeof(string));
                dt.Columns.Add("EQPTID", typeof(string));
                dt.Columns.Add("WRK_DATE_FROM", typeof(string));
                dt.Columns.Add("WRK_DATE_TO", typeof(string));
                dt.Columns.Add("PROCID", typeof(string));

                DataRow dr = dt.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["EQPTID"] = Convert.ToString(cboEquipment.SelectedValue).Equals("") ? null : Convert.ToString(cboEquipment.SelectedValue);
                dr["WRK_DATE_FROM"] = Util.NVC(dtpDateFrom.SelectedDateTime);
                dr["WRK_DATE_TO"] = Util.NVC(dtpDateTo.SelectedDateTime);
                dr["PROCID"] = Convert.ToString(cboProcess.SelectedValue);
                dt.Rows.Add(dr);

                // DataTable result = new ClientProxy().ExecuteServiceSync("DA_EQP_SEL_EQPTLOSS_LOSSRATE", "INDATA", "RSLTDT", dt);
                loadingIndicator.Visibility = Visibility.Visible;

                new ClientProxy().ExecuteService("DA_EQP_SEL_EQPTLOSS_LOSSRATE", "RQST", "RSLT", dt, (bizResult, bizException) =>
                {
                    if (bizException != null)
                    {
                        Util.MessageException(bizException);
                        return;
                    }

                    Util.GridSetData(dgLossMst, bizResult, FrameOperation, true);
                    loadingIndicator.Visibility = Visibility.Collapsed;

                });


            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                loadingIndicator.Visibility = Visibility.Collapsed;
            }
            
        }

        private void GetLossDetail(int idx)
        {
            try
            {
                DataTable RQSTDT = new DataTable();

                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("EQPTID", typeof(string));
                RQSTDT.Columns.Add("WRK_DATE", typeof(string));
                RQSTDT.Columns.Add("ASC", typeof(string));
                RQSTDT.Columns.Add("REVERSE_CHECK", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["EQPTID"] = Util.NVC(DataTableConverter.GetValue(dgLossMst.Rows[idx].DataItem, "EQPTID"));
                dr["WRK_DATE"] = Util.NVC(DataTableConverter.GetValue(dgLossMst.Rows[idx].DataItem, "WRK_DATE")).Replace("-", "");
                dr["ASC"] = "Y";
                dr["REVERSE_CHECK"] = null;
                RQSTDT.Rows.Add(dr);

                loadingIndicator.Visibility = Visibility.Visible;

                new ClientProxy().ExecuteService("DA_EQP_SEL_EQPTLOSSDETAIL", "RQST", "RSLT", RQSTDT, (bizResult, bizException) =>
                {
                    if (bizException != null)
                    {
                        Util.MessageException(bizException);
                        return;
                    }

                    Util.GridSetData(dgLossHist, bizResult, FrameOperation, true);
                    loadingIndicator.Visibility = Visibility.Collapsed;

                });

              //  Util.GridSetData(dgLossHist, result, FrameOperation, true);

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                loadingIndicator.Visibility = Visibility.Collapsed;
            }
        }

        public static class GridBackColor
        {
            public static readonly System.Drawing.Color Color1 = System.Drawing.Color.FromArgb(0, 0, 0);
            public static readonly System.Drawing.Color Color2 = System.Drawing.Color.FromArgb(0, 0, 225);
            public static readonly System.Drawing.Color Color3 = System.Drawing.Color.FromArgb(185, 185, 185);

            public static readonly System.Drawing.Color Color4 = System.Drawing.Color.FromArgb(150, 150, 150);
            public static readonly System.Drawing.Color Color5 = System.Drawing.Color.FromArgb(255, 255, 155);
            public static readonly System.Drawing.Color Color6 = System.Drawing.Color.FromArgb(255, 127, 127);

            public static readonly System.Drawing.Color R = System.Drawing.Color.FromArgb(0, 255, 0);
            public static readonly System.Drawing.Color W = System.Drawing.Color.FromArgb(255, 255, 0);
            public static readonly System.Drawing.Color T = System.Drawing.Color.FromArgb(255, 0, 0);
            public static readonly System.Drawing.Color F = System.Drawing.Color.FromArgb(0, 0, 0);
            public static readonly System.Drawing.Color N = System.Drawing.Color.FromArgb(255, 255, 255);
            public static readonly System.Drawing.Color U = System.Drawing.Color.FromArgb(128, 128, 128);

            public static readonly System.Drawing.Color I = System.Drawing.Color.FromArgb(255, 255, 0);
            public static readonly System.Drawing.Color P = System.Drawing.Color.FromArgb(255, 255, 0);
            public static readonly System.Drawing.Color O = System.Drawing.Color.FromArgb(0, 0, 0);
            
        }






        #endregion


    }
}
