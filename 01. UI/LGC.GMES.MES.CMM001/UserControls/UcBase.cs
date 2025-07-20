using LGC.GMES.MES.Common;
using System.ComponentModel;
using System.Windows.Controls;

namespace LGC.GMES.MES.CMM001.UserControls
{
    public abstract class UcBase : UserControl, INotifyPropertyChanged, IWorkArea
    {
        public IFrameOperation FrameOperation { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;

        internal void RaisePropertyChanged(string prop)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(prop));
        }
    }
}