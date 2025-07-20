/*************************************************************************************
 Created Date : 2017.03.07
      Creator : 
   Decription : 
--------------------------------------------------------------------------------------
 [Change History]
  2017.03.07  DEVELOPER : Initial Created.
 
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
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace LGC.GMES.MES.COM001
{
    public partial class COM001_103 : UserControl, IWorkArea
    {
        #region Declaration & Constructor 
        Grid grid;
        private System.Windows.Threading.DispatcherTimer timer;



        public COM001_103()
        {
            InitializeComponent();

        }

        public IFrameOperation FrameOperation
        {
            get;
            set;
        }

        #endregion

        #region Initialize
        /// <summary>
        /// 화면내 combo 셋팅
        /// </summary>
      
        #endregion

        #region Event
        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            //사용자 권한별로 버튼 숨기기
            List<Button> listAuth = new List<Button>();
            
            Util.pageAuth(listAuth, FrameOperation.AUTHORITY);
            //여기까지 사용자 권한별로 버튼 숨기기

       //     tbTime.Text = Convert.ToString(System.DateTime.Now);
            timer = new System.Windows.Threading.DispatcherTimer();

            SetGrid(rdoM1.IsChecked == true ? "M1" : "M2");
            SetTimer();
        
          }

        private void UserControl_Unloaded(object sender, RoutedEventArgs e)
        {
            ResetTimer();
        }

        private void txtTime_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
              //  ResetTimer();
                SetTimer();
            }
            
        }

        private void rdoM1_Click(object sender, RoutedEventArgs e)
        {
            //ResetTimer();
            SetGrid(rdoM1.IsChecked == true ? "M1" : "M2");
            SetTimer();
        }

        private void rdoM2_Click(object sender, RoutedEventArgs e)
        {
            //ResetTimer();
            SetGrid(rdoM1.IsChecked == true ? "M1" : "M2");
            SetTimer();
        }

        #endregion

        #region Mehod
        private void ResetTimer()
        {
            if (timer != null)
            {
                timer.Stop();
                timer.Tick -= SetGrid_Event;
            }
        }

        private void SetTimer()
        {
            if (txtTime.Value < 1)
            {
                Util.MessageValidation("최소 1분 주기로 조회 가능합니다.");
                txtTime.Value = 1;
            }

            timer.Interval = TimeSpan.FromMinutes(txtTime.Value);
            timer.Tick += SetGrid_Event;
            timer.Start();
            
        }
        private void SetGrid_Event(object sender, EventArgs e)
        {
           // loadingIndicator.Visibility = Visibility.Visible;

          //  this.Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Background, new Action(() =>
          //  {
                try
                {
                    SetGrid(rdoM1.IsChecked == true ? "M1" : "M2");
                }
                catch (Exception ex)
                {

                }
                finally
                {
                    loadingIndicator.Visibility = Visibility.Collapsed;
                }

        //    }));
        }


        private void SetGrid(string areaid)
        {
        
                ClearGrid();

                int rowcnt = 0;
                int colcnt = 0;

                rowcnt = GetRowCnt(areaid) + 1;
                colcnt = GetColCnt(areaid) + 1;

                grid = new Grid();

                for (int i = 0; i < rowcnt; i++)
                {
                    RowDefinition row = new RowDefinition();
                    row.Height = GridLength.Auto;
                    row.MinHeight = 20;
                    grid.RowDefinitions.Add(row);

                }

                for (int i = 0; i < colcnt; i++)
                {
                    ColumnDefinition col = new ColumnDefinition();
                    col.Width = GridLength.Auto;
                    col.MinWidth = 20;
                    grid.ColumnDefinitions.Add(col);
                }

                for (int i = 0; i < grid.RowDefinitions.Count; i++)
                {
                    for (int j = 0; j < grid.ColumnDefinitions.Count; j++)
                    {
                        var child_grid = new Grid();
                        child_grid.Name = "grid" + i + j;

                        child_grid.SetValue(Grid.RowProperty, i);
                        child_grid.SetValue(Grid.ColumnProperty, j);
                        child_grid.Margin = new Thickness(0, 0, 5, 5);

                        Label lb1 = new Label();
                        Label lb2 = new Label();

                        if (i == 0 && j == 0)
                        {
                            lb2.Content = "LINE / 공정";
                            lb2.HorizontalContentAlignment = HorizontalAlignment.Center;
                            lb2.VerticalContentAlignment = VerticalAlignment.Center;
                            lb2.FontWeight = FontWeights.Bold;
                            lb2.FontSize = 20;
                            lb2.Background = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("white"));
                            lb2.BorderBrush = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("black"));
                            lb2.BorderThickness = new Thickness(3, 3, 3, 3);

                        }

                        lb1.Name = "tb1" + i + j;
                        lb1.Visibility = Visibility.Collapsed;

                        lb2.Name = "tb2" + i + j;

                        child_grid.Children.Add(lb1);
                        child_grid.Children.Add(lb2);

                        grid.Children.Add(child_grid);

                    }
                }

                Main.Children.Add(grid);

                SetData(areaid);

        }

        private int GetRowCnt(string areaid)
        {
            try
            {
                DataTable dt = new DataTable();
                dt.Columns.Add("AREAID", typeof(string));

                DataRow dr = dt.NewRow();
                dr["AREAID"] = areaid;
                dt.Rows.Add(dr);

                DataTable rowResult = new ClientProxy().ExecuteServiceSync("DA_LMS_SEL_PRCESSEQUIPMENTSEGMENT_FOR_LINE", "INDATA", "OUTDATA", dt);

                if (rowResult == null)
                {
                    return 0;

                }
                else
                {
                    return rowResult.Rows.Count;
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return 0;
            }
        }

        private int GetColCnt(string areaid)
        {
            DataTable dt = new DataTable();
            dt.Columns.Add("AREAID", typeof(string));

            DataRow dr = dt.NewRow();
            dr["AREAID"] = areaid;
            dt.Rows.Add(dr);

            
            DataTable ColResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_PROCESS_COL_CNT", "INDATA", "OUTDATA", dt);

            if (ColResult == null)
            {
                return 0;

            }
            else
            {
                return ColResult.Rows.Count;
            }
        }

        private void SetData(string areaid)
        {

            DataTable dt = new DataTable();
            dt.Columns.Add("AREAID", typeof(string));
            dt.Columns.Add("LANGID", typeof(string));

            DataRow dr = dt.NewRow();
            dr["AREAID"] = areaid;
            dr["LANGID"] = LoginInfo.LANGID;
            dt.Rows.Add(dr);

            DataTable result_row = new ClientProxy().ExecuteServiceSync("DA_LMS_SEL_PRCESSEQUIPMENTSEGMENT_FOR_LINE", "INDATA", "OUTDATA", dt);



            for (int i = 1; i < grid.RowDefinitions.Count; i++)
            {

                foreach (Label tmplb in Util.FindVisualChildren<Label>(Application.Current.MainWindow))
                {
                    if (tmplb.Name.Equals("tb1" + i + 0))
                    {
                        tmplb.Content = Convert.ToString(result_row.Rows[i - 1]["EQSGID"]);
                    }

                    if (tmplb.Name.Equals("tb2" + i + 0))
                    {
                        tmplb.Content = Convert.ToString(result_row.Rows[i - 1]["EQSGNAME"]);
                        tmplb.HorizontalContentAlignment = HorizontalAlignment.Center;
                        tmplb.VerticalContentAlignment = VerticalAlignment.Center;
                        tmplb.FontWeight = FontWeights.Bold;
                        tmplb.FontSize = 20;
                        tmplb.Background = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("white"));
                        tmplb.BorderBrush = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("black"));
                        tmplb.BorderThickness = new Thickness(3, 3, 3, 3);
                    }
                }

            }

            DataTable result_col = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_PROCESS_COL_NAME", "INDATA", "OUTDATA", dt);

            for (int i = 1; i < grid.ColumnDefinitions.Count; i++)
            {

                foreach (Label tmplb in Util.FindVisualChildren<Label>(Application.Current.MainWindow))
                {
                    if (tmplb.Name.Equals("tb1" + 0 + i))
                    {
                        tmplb.Content = Convert.ToString(result_col.Rows[i - 1]["PROCID"]);
                    }

                    if (tmplb.Name.Equals("tb2" + 0 + i))
                    {
                        tmplb.Content = Convert.ToString(result_col.Rows[i - 1]["PROCNAME"]);
                        tmplb.HorizontalContentAlignment = HorizontalAlignment.Center;
                        tmplb.VerticalContentAlignment = VerticalAlignment.Center;
                        tmplb.FontWeight = FontWeights.Bold;
                        tmplb.FontSize = 20;
                        tmplb.Background = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("white"));
                        tmplb.BorderBrush = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("black"));
                        tmplb.BorderThickness = new Thickness(3, 3, 3, 3);
                    }
                }

            }




            DataTable result_eqpt = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_EQUIPMENT_FOR_EQUIP", "INDATA", "OUTDATA", dt);
            DataTable result_time = new ClientProxy().ExecuteServiceSync("DA_COM_SEL_GETDATE", "INDATA", "OUTDATA", dt);
            tbTime.Text = Convert.ToString(result_time.Rows[0]["DATE"]);

            string eqsgid = string.Empty;
            string procid = string.Empty;

            int eqpt_cnt = -1;

            for (int i = 1; i < grid.RowDefinitions.Count; i++)
            {
                for (int j = 1; j < grid.ColumnDefinitions.Count; j++)
                {
                    foreach (Label tmplb in Util.FindVisualChildren<Label>(Application.Current.MainWindow))
                    {
                        if (tmplb.Name.Equals("tb1" + i + 0))
                        {
                            eqsgid = (string)tmplb.Content ;   
                        }

                        if (tmplb.Name.Equals("tb1" + 0 + j))
                        {
                            procid = (string)tmplb.Content;
                        }

                    }

                   eqpt_cnt = result_eqpt.Select("EQSGID = '" + eqsgid + "' and PROCID = " + "'" + procid + "'").Count();
                   DataTable result_eqpt_tmp = result_eqpt.Select("EQSGID = '" + eqsgid + "' and PROCID = " + "'" + procid + "'").Count() == 0 ? null : result_eqpt.Select("EQSGID = '" + eqsgid + "' and PROCID = " + "'" + procid + "'").CopyToDataTable();

                    if (result_eqpt_tmp != null)
                    {
                        foreach (Grid grid1 in Util.FindVisualChildren<Grid>(Application.Current.MainWindow))
                        {
                            if (grid1.Name.Equals("grid" + i + j))
                            {
                                for (int z = 0; z < 4; z++)
                                {
                                    ColumnDefinition coldef = new ColumnDefinition();
                                    coldef.Width = GridLength.Auto;
                                    coldef.MinWidth = 20;
                                    grid1.ColumnDefinitions.Add(coldef);
                                }

                                for (int z = 0; z < Math.Ceiling((double)eqpt_cnt / 4); z++)
                                {
                                    RowDefinition rowdef = new RowDefinition();
                                    rowdef.Height = GridLength.Auto;
                                    rowdef.MinHeight = 20;
                                    grid1.RowDefinitions.Add(rowdef);
                                }

                                for (int z = 0; z < eqpt_cnt; z++)
                                {
                                    Grid eqpt_grid = new Grid();
                                    eqpt_grid.SetValue(Grid.ColumnProperty, z%4);
                                    eqpt_grid.SetValue(Grid.RowProperty, z/4);
                                    eqpt_grid.Margin = new Thickness(0,0,5,5);
                                   // eqpt_grid.Width = 200;

                                    grid1.Children.Add(eqpt_grid);

                                    TextBox tb = new TextBox();
                                    grid1.RegisterName(Convert.ToString(result_eqpt_tmp.Rows[z]["EQPTID"]), tb);
                                    tb.Text = Convert.ToString(result_eqpt_tmp.Rows[z]["EQPTID"]);
                                    tb.Visibility = Visibility.Collapsed;

                                    Label lb2 = new Label();
                                    lb2.Content =  (Convert.ToString(result_eqpt_tmp.Rows[z]["EQPTNAME1"]) + "\r\n"+ Convert.ToString(result_eqpt_tmp.Rows[z]["EQPTNAME2"]));
                                    lb2.FontSize = 10;
                                    lb2.Width = 120;
                                    lb2.HorizontalContentAlignment = HorizontalAlignment.Center;
                                    lb2.VerticalContentAlignment = VerticalAlignment.Center;
                                    lb2.BorderBrush = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("black"));
                                    lb2.BorderThickness = new Thickness(2, 2, 2, 2);

                                    eqpt_grid.Children.Add(lb2);

                                    lb2.Background = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("white"));
                                    lb2.Foreground = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("black"));

                                    if (Convert.ToString(result_eqpt_tmp.Rows[z]["EIOSTAT"]).Equals("R"))
                                    {
                                        lb2.Background = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("lightgreen"));
                                    }

                                    if (Convert.ToString(result_eqpt_tmp.Rows[z]["EIOSTAT"]).Equals("W"))
                                    {
                                        lb2.Background = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("yellow"));
                                    }

                                    if (Convert.ToString(result_eqpt_tmp.Rows[z]["EIOSTAT"]).Equals("T"))
                                    {
                                        lb2.Background = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("red"));
                                        lb2.Foreground = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("white"));
                                    }

                                    if (Convert.ToString(result_eqpt_tmp.Rows[z]["EIOSTAT"]).Equals("U"))
                                    {
                                    }

                                    if (Convert.ToString(result_eqpt_tmp.Rows[z]["EIOSTAT"]).Equals("F"))
                                    {
                                        lb2.Background = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("black"));
                                        lb2.Foreground = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("white"));
                                    }


                                }

                            }

                        }
                    }
                  
                 


                }
              
            }
            
        }

        private void ClearGrid()
        {
            grid = null;
            Main.Children.Clear();
        }






        #endregion

      
    }
}
