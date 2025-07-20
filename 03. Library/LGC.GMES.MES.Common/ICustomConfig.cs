using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;

namespace LGC.GMES.MES.Common
{
    public interface ICustomConfig
    {
        string ConfigName { get; }
        DataTable[] GetCustomConfigs();
        void SetCustomConfigs(DataSet configSet);
        bool CanSave();
    }
}
