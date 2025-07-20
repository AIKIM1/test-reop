using C1.WPF;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.Common.Commands;
using LGC.GMES.MES.Common.Mvvm;
using System;
using System.Windows.Input;

namespace LGC.GMES.MES.CMM001.ViewModels
{
    public class SearchViewModel : BindableBase
    { 
        public SearchViewModel()
        {
            this.SearchCommand = new DelegateCommand(this.OnClickSearch);
            this.InitComboCommand = new DelegateCommand(this.SetComboBoxControls);
        }

        public ICommand SearchCommand { get; set; }

        public ICommand InitComboCommand { get; set; }

        public C1ComboBox EquipmentCombo { get; set; }

        public C1ComboBox EquipmentSegmentCombo { get; set; }

        public string ProcId { get; set; }

        public virtual void OnClickSearch()
        {
        }

        void SetComboBoxControls()
        {
            CommonCombo _combo = new CommonCombo();
            String[] sFilter = { LoginInfo.CFG_AREA_ID };
            C1ComboBox[] cboLineChild = { EquipmentCombo };
            _combo.SetCombo(EquipmentSegmentCombo, CommonCombo.ComboStatus.SELECT, cbChild: cboLineChild, sFilter: sFilter);

            String[] sFilter2 = { ProcId };
            C1ComboBox[] cboEquipmentParent = { EquipmentSegmentCombo };
            _combo.SetCombo(EquipmentCombo, CommonCombo.ComboStatus.SELECT, cbParent: cboEquipmentParent, sFilter: sFilter2);
        }
    }
}