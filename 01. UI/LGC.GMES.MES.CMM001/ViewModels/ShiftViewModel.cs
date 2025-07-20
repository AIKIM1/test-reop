using LGC.GMES.MES.Common.Commands;
using LGC.GMES.MES.Common.Mvvm;
using System.Windows.Input;

namespace LGC.GMES.MES.CMM001.ViewModels
{
    public class ShiftViewModel : BindableBase
    { 
        public ShiftViewModel()
        {
            this.ShiftCommand = new DelegateCommand(this.OnClickShift);
        }

        public ICommand ShiftCommand { get; set; }

        public virtual void OnClickShift()
        {
        }
    }
}