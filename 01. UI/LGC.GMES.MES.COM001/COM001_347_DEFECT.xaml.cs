using C1.WPF;
using C1.WPF.DataGrid;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.CMM001.Popup;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace LGC.GMES.MES.COM001
{
    public partial class COM001_347_DEFECT : C1Window, IWorkArea
    {
        class ViewModel : INotifyPropertyChanged
        {
            public event PropertyChangedEventHandler PropertyChanged;
            private DataView dvDefect;
            public DataView DvDefect
            {
                get { return dvDefect; }
                set
                {
                    dvDefect = value;
                    OnPropertyUpdate("DvDefect");
                }
            }

            private void OnPropertyUpdate(string propertyName)
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        #region Declaration & Constructor
        ViewModel vm;
        public IFrameOperation FrameOperation
        {
            get;
            set;
        }

        public COM001_347_DEFECT()
        {
            InitializeComponent();
            vm = new ViewModel();
            this.DataContext = vm;
        }
        #endregion

        #region Initialize
        #endregion

        #region Event
        private void C1Window_Loaded(object sender, RoutedEventArgs e)
        {
            object[] parameters = C1WindowExtension.GetParameters(this);
            if (parameters == null || parameters.Length < 1)
            {
                return;
            }

            string lotid = Util.NVC(parameters[0]);
            SetDefect(lotid);
        }

        private void dgDefect_LoadedCellPresenter(object sender, DataGridCellEventArgs e)
        {
            try
            {
                if (e == null || e.Cell == null || e.Cell.Presenter == null)
                {
                    return;
                }

                if (e.Cell.Column.Name.Equals("ACTID"))
                {
                    return;
                }

                int flagRow = e.Cell.Row.Index;
                int flagCol = e.Cell.DataGrid.Columns["DFCT_QTY_CHG_BLOCK_FLAG"].Index;
                C1.WPF.DataGrid.DataGridCell flagDataCell = e.Cell.DataGrid[flagRow, flagCol];
                string flagData = Util.NVC(flagDataCell.Value);

                if (flagData.Equals("Y"))
                {
                    C1DataGrid dg = sender as C1DataGrid;
                    if (dg == null)
                    {
                        return;
                    }
                    dg.Dispatcher.BeginInvoke(new Action(() =>
                    {
                        try
                        {
                            if (e == null || e.Cell == null || e.Cell.Presenter == null)
                            {
                                return;
                            }
                            e.Cell.Presenter.Background = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#D4D4D4"));
                            e.Cell.Presenter.Foreground = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#FF000000"));
                        }
                        catch (Exception ex)
                        {
                            throw ex;
                        }
                    }));
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void dgDefect_UnloadedCellPresenter(object sender, DataGridCellEventArgs e)
        {
            try
            {
                if (e == null || e.Cell == null || e.Cell.Presenter == null)
                {
                    return;
                }

                C1DataGrid dg = sender as C1DataGrid;

                if (dg == null)
                {
                    return;
                }

                dg.Dispatcher.BeginInvoke(new Action(() =>
                {
                    try
                    {
                        if (e == null || e.Cell == null || e.Cell.Presenter == null)
                        {
                            return;
                        }
                        e.Cell.Presenter.Background = new SolidColorBrush(Colors.White);
                    }
                    catch (Exception ex)
                    {
                        throw ex;
                    }
                }));
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        #endregion

        #region Mehod
        private void SetDefect(string lotid)
        {
            try
            {
                DataTable dt = new DataTable();
                dt.Columns.Add("LANGID", typeof(string));
                dt.Columns.Add("LOTID", typeof(string));

                DataRow dr = dt.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["LOTID"] = lotid;
                dt.Rows.Add(dr);

                new ClientProxy().ExecuteService("DA_QCA_SEL_WIPRESONCOLLECT_BY_LOTID_L", "RQSTDT", "RSLTDT", dt, (bizResult, exception) =>
                {
                    if (exception != null)
                    {
                        throw exception;
                    }

                    try
                    {
                        vm.DvDefect = bizResult.DefaultView;
                    }
                    catch (Exception ex)
                    {
                        throw ex;
                    }
                });

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        #endregion
    }
}
