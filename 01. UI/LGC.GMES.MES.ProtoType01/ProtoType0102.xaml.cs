/*************************************************************************************
 Created Date : 2016.06.16
      Creator : 
   Decription : 
--------------------------------------------------------------------------------------
 [Change History]
  2016.06.16  DEVELOPER : Initial Created.





 
**************************************************************************************/

using System;
using System.Collections.Generic;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;

namespace LGC.GMES.MES.ProtoType01
{

    public partial class ProtoType0102 : UserControl, IWorkArea
    {

        /// <summary>
        /// Frame과 상호작용하기 위한 객체
        /// </summary>
        public IFrameOperation FrameOperation
        {
            get;
            set;
        }

        #region Declaration & Constructor 
        public ProtoType0102()
        {
            InitializeComponent();
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            Initialize();
        }

        #endregion

        #region Initialize

        private void Initialize()
        {
            dtpDateFrom.SelectedDateTime = (DateTime)System.DateTime.Now;
            dtpDateTo.SelectedDateTime = (DateTime)System.DateTime.Now;
        }

        #endregion

        #region Event

        private void LayoutRoot_SearchClick(object sender, RoutedEventArgs e)
        {
            dgDatagrid.ItemsSource = CreateSampleData();
        }

        private void button01_Click(object sender, RoutedEventArgs e)
        {
            ProtoType0102Windows01 popup02 = new ProtoType0102Windows01();
            popup02.FrameOperation = this.FrameOperation;

            if (popup02 != null)
            {
                DataTable dtData = new DataTable();

                dtData.Columns.Add("COL01", typeof(string));
                dtData.Columns.Add("COL02", typeof(string));

                DataRow newRow = null;

                newRow = dtData.NewRow();
                newRow.ItemArray = new object[] { "Text01", "Text02"};
                dtData.Rows.Add(newRow);

                newRow = dtData.NewRow();
                newRow.ItemArray = new object[] { "Text01", "Text02"};
                dtData.Rows.Add(newRow);

                //========================================================================
                string Parameter = "Parameter";
                C1WindowExtension.SetParameter(popup02, Parameter);
                //========================================================================
                object[] Parameters = new object[2];
                Parameters[0] = "Parameter01";
                Parameters[1] = dtData;
                C1WindowExtension.SetParameters(popup02, Parameters);
                //========================================================================

                //popup02.Closed -= popup02_Closed;
                //popup02.Closed += popup02_Closed;
                popup02.ShowModal();
                popup02.CenterOnScreen();
            }


            //string MAINFORMPATH = "LGC.GMES.MES.ProtoType01";
            //string MAINFORMNAME = "ProtoType0102Windows01";

            //Assembly asm = Assembly.LoadFrom("ClientBin\\" + MAINFORMPATH + ".dll");
            //Type targetType = asm.GetType(MAINFORMPATH + "." + MAINFORMNAME);
            //object obj = Activator.CreateInstance(targetType);

            //IWorkArea workArea = obj as IWorkArea;
            //workArea.FrameOperation = FrameOperation;

            //C1Window popup01 = obj as C1Window;
            //if (popup01 != null)
            //{
            //    popup01.Closed -= popup01_Closed;
            //    popup01.Closed += popup01_Closed;
            //    popup01.ShowModal();
            //    popup01.CenterOnScreen();
            //}
        }

        private void button02_Click(object sender, RoutedEventArgs e)
        {
        }

        void popup01_Closed(object sender, EventArgs e)
        {

        }

        private void dg_LoadedCellPresenter(object sender, C1.WPF.DataGrid.DataGridCellEventArgs e)
        {
            if (e.Cell.Column.Name == "Background")
            {
                switch (Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "Background"), "").ToString().Trim())
                {
                    case "R":
                        this.Dispatcher.BeginInvoke(new Action(() =>
                        {
                            e.Cell.Presenter.Background = new SolidColorBrush(Colors.Red);
                        }));
                        break;
                    case "Y":
                        this.Dispatcher.BeginInvoke(new Action(() =>
                        {
                            e.Cell.Presenter.Background = new SolidColorBrush(Colors.Yellow);
                        }));
                        break;
                    default:
                        this.Dispatcher.BeginInvoke(new Action(() =>
                        {
                            e.Cell.Presenter.Background = new SolidColorBrush(Colors.Transparent);
                        }));
                        break;
                }
            }
        }

        private void dg_UnloadedCellPresenter(object sender, C1.WPF.DataGrid.DataGridCellEventArgs e)
        {
            if (e.Cell.Column.Name == "Background")
            {
                e.Cell.Presenter.Background = new SolidColorBrush(Colors.Transparent);
            }
        }

        private void dgDatagrid_SelectionChanged(object sender, C1.WPF.DataGrid.DataGridSelectionChangedEventArgs e)
        {
            txtFrom.Text = Util.NVC(DataTableConverter.GetValue(dgDatagrid.SelectedItem, "From"));
            txtSubject.Text = Util.NVC(DataTableConverter.GetValue(dgDatagrid.SelectedItem, "Subject"));
            txtReceived.Text = Util.NVC(DataTableConverter.GetValue(dgDatagrid.SelectedItem, "Received"));
            txtSize.Text = Util.NVC(DataTableConverter.GetValue(dgDatagrid.SelectedItem, "Size"));
            txtRead.Text = Util.NVC(DataTableConverter.GetValue(dgDatagrid.SelectedItem, "Read"));
            txtFlagged.Text = Util.NVC(DataTableConverter.GetValue(dgDatagrid.SelectedItem, "Flagged"));
        }

        #endregion

        #region Mehod

        public List<MainItem> CreateSampleData()
        {
            List<MainItem> items = new List<MainItem>();
            Random rnd = new Random();
            string[] names = { "Ben Schorr", "Chris Brown", "Richard Mosher", "Michael Flatley", "Ken Slovak", "Greg Lutz", "Kris Kringle", "Kevin", "Larry Seltzer", "Rob M. Smith" };
            string[] subjects = { "Re: Alert - Microsoft Exchange", "Re: Destination folder", "Re: Repeating Controls", "Sync Issues Reference?", "Are you able to attend tomorrow?", "Create a service from Webinar series", "Re: Modify the IE toolbar", "Re: Sync Issues Reference?", "Re: Update on Bugs reported in TFS" };
            for (int i = 0; i < 70; i++)
            {
                MainItem item = new MainItem();
                item.Subject = subjects[rnd.Next(subjects.Length - 1)];
                item.Received = DateTime.Now.Subtract(new TimeSpan(rnd.Next(100), rnd.Next(60), 0));
                int r = rnd.Next(3);
                if (r == 1)
                {
                    item.Read = true;
                }
                else if (r == 2)
                {
                    item.Read = false;
                }
                else
                {
                    item.Read = null;
                }
                item.From = names[rnd.Next(names.Length - 1)];
                item.Size = rnd.Next(2000);
                item.Flagged = rnd.Next(5) % 2 == 0 ? false : true;
                items.Add(item);
            }
            return items;
        }

        #endregion

    }

    public class MainItem
    {
        public string From { get; set; }
        public string Subject { get; set; }
        public DateTime Received { get; set; }
        public double Size { get; set; }
        public bool? Read { get; set; }
        public bool Flagged { get; set; }

    }
}
