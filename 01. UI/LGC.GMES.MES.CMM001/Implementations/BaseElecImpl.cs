using C1.WPF.DataGrid;
using LGC.GMES.MES.CMM001.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace LGC.GMES.MES.CMM001.Implementations
{
    public abstract class BaseElecImpl : IBaseElec
    {
        public event RoutedEventHandler ConfirmClicked;
        public event RoutedEventHandler StartCancelClicked;
        public event RoutedEventHandler StartClicked;

        public void GetColorTagInfoList()
        {
            throw new NotImplementedException();
        }

        public void GetColorTagInfoList(object SelectedItem)
        {
            throw new NotImplementedException();
        }

        public virtual void RunProcess(ref C1DataGrid grid, string procId)
        {
            throw new NotImplementedException();
        }
    }
}