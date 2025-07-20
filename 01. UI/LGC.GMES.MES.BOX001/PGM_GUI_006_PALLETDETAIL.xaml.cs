/*************************************************************************************
 Created Date : 
      Creator : 
   Decription : 
--------------------------------------------------------------------------------------
 [Change History]
  
  
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
using LGC.GMES.MES.Common;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.ControlsLibrary;
using System.Data;

namespace LGC.GMES.MES.BOX001
{
    /// <summary>
    /// ASSY001_001_EQPEND.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class PGM_GUI_006_PALLETDETAIL : C1Window, IWorkArea
    {
        #region Declaration & Constructor
        
        #endregion

        #region Initialize
        /// <summary>
        ///  Frame과 상호작용하기 위한 객체
        /// </summary>
        public IFrameOperation FrameOperation
        {
            get;
            set;
        }

        public PGM_GUI_006_PALLETDETAIL()
        {
            InitializeComponent();
        }


        #endregion

        #region Event
        private void C1Window_Loaded(object sender, RoutedEventArgs e)
        {
           object[] tmps = C1WindowExtension.GetParameters(this);

            string lotid = tmps[0].ToString();
            GetData(lotid);

            // Util.Alert(tmps[0].ToString());
        }
        private void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            //?????
        }
        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = MessageBoxResult.Cancel;
        }
        #endregion

        #region Mehod

        #region [BizCall]



        #endregion

        #region [Validation]


        #endregion

        #region [Func]

        private void GetData(string lotid)
        {
            DataTable RQSTDT = new DataTable();
            RQSTDT.TableName = "RQSTDT";
            RQSTDT.Columns.Add("LANGID", typeof(string));
            RQSTDT.Columns.Add("PALLETID", typeof(string));

            DataRow dr = RQSTDT.NewRow();
            dr["LANGID"] = LoginInfo.LANGID;
            dr["PALLETID"] = lotid;
            RQSTDT.Rows.Add(dr);

            DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_BOXLOT_SRS", "RQSTDT", "RSLTDT", RQSTDT);
            dgPancakeInfo.ItemsSource = DataTableConverter.Convert(dtResult);

        }




        #endregion

        #endregion

     
    }
}
