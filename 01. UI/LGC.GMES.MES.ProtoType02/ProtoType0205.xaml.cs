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
using System.Windows;
using System.Windows.Controls;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;

namespace LGC.GMES.MES.ProtoType02
{
    public partial class ProtoType0205 : UserControl, IWorkArea
    {
        #region Declaration & Constructor 

        /// <summary>
        /// Frame과 상호작용하기 위한 객체
        /// </summary>
        public IFrameOperation FrameOperation
        {
            get;
            set;
        }

        private ProtoType0205_Window window01 = new ProtoType0205_Window();

        public ProtoType0205()
        {
            InitializeComponent();
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
        }

        #endregion

        #region Initialize

        private void Initialize()
        {

        }

        #endregion

        #region Event

        //Button =======================================================================================================

        private void btnMain01_Click(object sender, RoutedEventArgs e)
        {
            Get_Main01();
        }

        private void btnColtrol_Click(object sender, RoutedEventArgs e)
        {
            FrameOperation.PrintFrameMessage(Util.NVC(DataTableConverter.GetValue(window01.dgDatagrid.SelectedItem, "From")), true);
        }

        #endregion

        #region Mehod

        private void Get_Main01()
        {
            if (grSub.Children.Count == 0)
            {
                window01.ProtoType0205 = this;
                grSub.Children.Add(window01);
            }
        }

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
