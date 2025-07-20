using System;
using System.Collections.Generic;
using System.Windows;

namespace LGC.GMES.MES.ProtoType02
{
    public partial class ProtoType0205_Window
    {
        #region Declaration & Constructor 

        public ProtoType0205 ProtoType0205;

        public ProtoType0205_Window()
        {
            InitializeComponent();
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            dgDatagrid.ItemsSource = CreateSampleData();
        }

        #endregion

        #region Initialize

        #endregion

        #region Event

        private void dgDatagrid_SelectionChanged(object sender, C1.WPF.DataGrid.DataGridSelectionChangedEventArgs e)
        {
            System.Diagnostics.Debug.Write("");
            ProtoType0205.dgDatagrid.ItemsSource = ProtoType0205.CreateSampleData();
        }

        #endregion

        #region Mehod

        List<MainItem> CreateSampleData()
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
}
