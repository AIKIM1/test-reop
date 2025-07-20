using LGC.GMES.MES.CMM001.Class;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LGC.GMES.MES.CMM001.Interfaces
{
    public interface IBaseElec : ICommon, ICommand, ISearch, IWorkOrder, IProdLot, IResult, IDataCollect
    {
    }
}