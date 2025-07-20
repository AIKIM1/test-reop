/*************************************************************************************
 Created Date : 2020.10.08
      Creator : 
   Decription : 
--------------------------------------------------------------------------------------
 [Change History]
  2020.10.08  DEVELOPER : Initial Created.
  2022.02.23  KDH : AREA 조건 추가




 
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
    public partial class FCS001_016_TRAY_LIST : C1Window, IWorkArea
    {
        #region Declaration & Constructor 
        private string _sModelID = string.Empty;
        private string _sLineID = string.Empty;
        private string _sDegasAb = string.Empty;
        private string _sOpID = string.Empty;
        private string _sDanCnt = string.Empty;
        private string _sEqpKindCD = string.Empty;
        private string _sLaneID = string.Empty;

        public FCS001_016_TRAY_LIST()
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
            _sModelID = tmps[0] as string;
            _sLineID = tmps[1] as string;
            _sDegasAb = tmps[2] as string;
            _sOpID = tmps[3] as string;
            _sDanCnt = tmps[4] as string;
            _sEqpKindCD = tmps[5] as string;
            _sLaneID = tmps[6] as string;

            try
            {
                DataTable IndataTable = new DataTable();
                IndataTable.Columns.Add("LANGID", typeof(string));
                IndataTable.Columns.Add("MDL_ID", typeof(string));
                IndataTable.Columns.Add("EQSGID", typeof(string));
                IndataTable.Columns.Add("DEGAS_AB", typeof(string));
                IndataTable.Columns.Add("PROCID", typeof(string));
                IndataTable.Columns.Add("DAN_CNT", typeof(string));
                IndataTable.Columns.Add("EQPT_GR_TYPE_CODE", typeof(string));
                IndataTable.Columns.Add("LANE_ID", typeof(string));
                IndataTable.Columns.Add("AREAID", typeof(string)); //2022.02.23_AREA 조건 추가


                DataRow Indata = IndataTable.NewRow();
                Indata["LANGID"] = LoginInfo.LANGID;
                Indata["MDL_ID"] = _sModelID;
                Indata["EQSGID"] = _sLineID;
                Indata["DEGAS_AB"] = _sDegasAb;
                Indata["PROCID"] = _sOpID;
                Indata["DAN_CNT"] = _sDanCnt;
                Indata["EQPT_GR_TYPE_CODE"] = _sEqpKindCD;
                Indata["LANE_ID"] = _sLaneID;
                Indata["AREAID"] = LoginInfo.CFG_AREA_ID; //2022.02.23_AREA 조건 추가

                IndataTable.Rows.Add(Indata);

                new ClientProxy().ExecuteService("DA_SEL_AGING_RACK_POPUP", "INDATA", "OUTDATA", IndataTable, (result, exception) =>
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

        private void dgList_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            try
            {
                C1.WPF.DataGrid.C1DataGrid dg = sender as C1.WPF.DataGrid.C1DataGrid;
                if (dg.CurrentColumn.Name.Equals("TRAY_ID"))
                {
                    //Tray 정보조회 Menu Open(FCS001_021)
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
            finally
            {
                loadingIndicator.Visibility = Visibility.Collapsed;
            }
        }
        #endregion

        #region Mehod     

        #endregion


    }
}
