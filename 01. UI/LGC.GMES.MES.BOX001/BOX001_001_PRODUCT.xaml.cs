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
    public partial class BOX001_001_PRODUCT : C1Window, IWorkArea
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

        public BOX001_001_PRODUCT()
        {
            InitializeComponent();
        }


        #endregion

        #region Event
        private void C1Window_Loaded(object sender, RoutedEventArgs e)
        {
            //object[] tmps = C1WindowExtension.GetParameters(this);

            //string lotid = tmps[0].ToString();
            //GetData(lotid);

            // Util.Alert(tmps[0].ToString());
          //  SetCombo(dgProductInfo.Columns["USAGE"]);
            GetProduct();

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
        private void GetProduct()
        {
            DataTable dt = new DataTable();
            dt.Columns.Add("LANGID", typeof(string));

            DataRow row = dt.NewRow();
            row["LANGID"] = LoginInfo.LANGID;

            dt.Rows.Add(row);

            SetCombo(dgProductInfo.Columns["USAGE"]);

            try
            {
                DataTable result = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_PRODUCT_SET_SRS", "RQSTDT", "RSLTDT", dt);

                dgProductInfo.ItemsSource = DataTableConverter.Convert(result);
            }
            catch (Exception ex)
            {
                Util.Alert(ex.ToString());
            }
        }


        #endregion

        #region [Validation]


        #endregion

        #region [Func]
        private void SetCombo(C1.WPF.DataGrid.DataGridColumn col)
        {
            DataTable dt = new DataTable();
            dt.Columns.Add("CBO_CODE");
            dt.Columns.Add("CBO_NAME");

            DataRow newRow = null;

            newRow = dt.NewRow();
            newRow.ItemArray = new object[] { "DEL", "삭제" };
            dt.Rows.Add(newRow);

            newRow = dt.NewRow();
            newRow.ItemArray = new object[] { "USE", "사용" };
            dt.Rows.Add(newRow);

            (col as C1.WPF.DataGrid.DataGridComboBoxColumn).ItemsSource = DataTableConverter.Convert(dt);
        }





        #endregion

        #endregion


    }
}
