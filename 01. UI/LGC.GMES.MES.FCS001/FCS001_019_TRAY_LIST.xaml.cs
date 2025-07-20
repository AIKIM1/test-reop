/*************************************************************************************
 Created Date : 2020.11.03
      Creator : Dooly
   Decription : Tray List
--------------------------------------------------------------------------------------
 [Change History]
  2020.11.03  DEVELOPER : Initial Created.
 
**************************************************************************************/

using C1.WPF;
using C1.WPF.DataGrid;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace LGC.GMES.MES.FCS001
{
    public partial class FCS001_019_TRAY_LIST : C1Window, IWorkArea
    {
        #region Declaration & Constructor 
        private string _LotID = string.Empty;
        
        public FCS001_019_TRAY_LIST()
        {
            InitializeComponent();
        }
        
        public IFrameOperation FrameOperation
        {
            get;
            set;
        }
        #endregion

        #region Initialize

      
        #endregion

        #region Event
        private void C1Window_Loaded(object sender, RoutedEventArgs e)
        {
            object[] tmps = C1WindowExtension.GetParameters(this);
            _LotID = tmps[0] as string;

            try
            {
                DataTable IndataTable = new DataTable();
                IndataTable.Columns.Add("LOTID", typeof(string));

                DataRow Indata = IndataTable.NewRow();
                Indata["LOTID"] = _LotID;
                IndataTable.Rows.Add(Indata);

                new ClientProxy().ExecuteService("DA_SEL_W_LOT_CELL_ID", "INDATA", "OUTDATA", IndataTable, (result, exception) =>
                {
                    try
                    {
                        if (exception != null)
                            throw exception;

                        Util.gridClear(dgList);
                        Util.GridSetData(dgList, result, this.FrameOperation);
                    }
                    catch (Exception ex)
                    {
                        Util.MessageException(ex);
                    }
                    finally
                    {

                    }
                });

            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex.Message), null, "Error", MessageBoxButton.OK, MessageBoxIcon.None);
                return;
            }
        }      
        #endregion

        #region Mehod     

        #endregion


    }
}
