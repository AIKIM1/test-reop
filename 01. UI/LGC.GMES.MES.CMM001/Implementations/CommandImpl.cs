using C1.WPF.DataGrid;
using LGC.GMES.MES.CMM001.Interfaces;
using System.Windows;

namespace LGC.GMES.MES.CMM001.Implementations
{
    public class CommandImpl : ICommand
    {
        public event RoutedEventHandler StartClicked;
        public event RoutedEventHandler StartCancelClicked;
        public event RoutedEventHandler ConfirmClicked;

        void ClickStart()
        {
            OnStartClicked(new RoutedEventArgs());
        }

        void ClickStartCancel()
        {
            OnStartCancelClicked(new RoutedEventArgs());
        }

        void ClickConfirm()
        {
            OnConfirmClicked(new RoutedEventArgs());
        }

        protected virtual void OnStartClicked(RoutedEventArgs e)
        {
            if (StartClicked != null)
                StartClicked(this, e);
        }

        protected virtual void OnStartCancelClicked(RoutedEventArgs e)
        {
            if (StartClicked != null)
                StartCancelClicked(this, e);
        }

        protected virtual void OnConfirmClicked(RoutedEventArgs e)
        {
            if (StartClicked != null)
                ConfirmClicked(this, e);
        }

        public virtual void RunProcess(ref C1DataGrid grid, string procId)
        {
        }
    }
}