/*************************************************************************************
 Created Date : 2016.11.22
      Creator : INS 김동일K
   Decription : 전지 5MEGA-GMES 구축 - WORKORDER BOM 조회 팝업
--------------------------------------------------------------------------------------
 [Change History]
  2016.11.22  INS 김동일K : Initial Created.
  
**************************************************************************************/

using System;
using System.Collections.Generic;
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
using System.Windows.Shapes;
using C1.WPF;
using C1.WPF.DataGrid;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using System.Data;

namespace LGC.GMES.MES.CMM001.Popup
{
    /// <summary>
    /// CMM_WORKORDER_BOM.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class CMM_WORKORDER_BOM : C1Window, IWorkArea
    {
        private string _WOID = string.Empty;

        /// <summary>
        ///  Frame과 상호작용하기 위한 객체
        /// </summary>
        public IFrameOperation FrameOperation
        {
            get;
            set;
        }

        public CMM_WORKORDER_BOM()
        {
            InitializeComponent();
        }

        private void C1Window_Loaded(object sender, RoutedEventArgs e)
        {
            object[] tmps = C1WindowExtension.GetParameters(this);

            if (tmps.Length >= 1)
            {
                _WOID = tmps[0].ToString();                
            }
            else
            {
                _WOID = "";
            }

            GetWorkorderBOM();
        }

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = MessageBoxResult.Cancel;
        }

        private void GetWorkorderBOM()
        {
            DataTable searchConditionTable = new DataTable();
            searchConditionTable.Columns.Add("LANGID", typeof(string));
            searchConditionTable.Columns.Add("WOID", typeof(string));
            
            DataRow searchCondition = searchConditionTable.NewRow();
            searchCondition["LANGID"] = LoginInfo.LANGID;
            searchCondition["WOID"] = _WOID;
            
            searchConditionTable.Rows.Add(searchCondition);

            new ClientProxy().ExecuteService("DA_PRD_SEL_WOMTRL_BY_WOID", "INDATA", "OUTDATA", searchConditionTable, (searchResult, searchException) =>
            {
                try
                {
                    if (searchException != null)
                    {
                        Util.AlertByBiz("BR_PRD_REG_START_END_LOT_RW", searchException.Message, searchException.ToString());
                        return;
                    }

                    dgBOM.ItemsSource = DataTableConverter.Convert(searchResult);
                }
                catch (Exception ex)
                {
                    Util.MessageException(ex);
                    //Logger.Instance.WriteLine(Logger.OPERATION_R + "GetProductLot", ex);
                }
                finally
                {
                    //Logger.Instance.WriteLine(Logger.OPERATION_R + "GetProductLot", Logger.MESSAGE_OPERATION_END);
                }
            }
            );
        }
    }
}
