using C1.WPF.DataGrid;
using LGC.GMES.MES.CMM001.Class;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace LGC.GMES.MES.CMM001.Interfaces
{
    public interface ICommand
    {
        event RoutedEventHandler StartClicked;
        event RoutedEventHandler StartCancelClicked;
        event RoutedEventHandler ConfirmClicked;

        void RunProcess(ref C1DataGrid grid, string procId);
    }
}