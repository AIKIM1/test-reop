/*************************************************************************************
 Created Date : 2017.01.24
      Creator : INS 김동일K
   Decription : 전지 5MEGA-GMES 구축 - 조립 공정진척 화면 - LOT 종료취소 팝업
--------------------------------------------------------------------------------------
 [Change History]
  2017.01.24  INS 김동일K : Initial Created.
  
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
using C1.WPF.DataGrid;
using System.Management;

namespace LGC.GMES.MES.CMM001
{
    /// <summary>
    /// CMM_PRINTER_SETTING.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class CMM_PRINTER_SETTING : C1Window, IWorkArea
    {
        #region Declaration & Constructor         
        private string _ProcID = string.Empty;
        private string sPrvQtyValue = "";

        private BizDataSet _Biz = new BizDataSet();
        private Util _Util = new Util();
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
        public CMM_PRINTER_SETTING()
        {
            InitializeComponent();
        }
        #endregion

        #region Event
        /// <summary>
        /// Initializing 이후에 FormLoad시 Event를 생성.
        /// </summary>
        private void SetEvent()
        {
            this.Loaded -= C1Window_Loaded;
        }

        private void C1Window_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                ApplyPermissions();
                InitCombo();
                SetEvent();
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void btnOK_Click(object sender, RoutedEventArgs e)
        {
            if (SaveConfig())
            {
                this.DialogResult = MessageBoxResult.OK;
                this.Close();
            }
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = MessageBoxResult.Cancel;
        }
        #endregion

        #region Mehod

        #region Initialize
        private void InitCombo()
        {
            DataTable dtResult = new DataTable();

            dtResult.Columns.Add("CODE", typeof(string));
            dtResult.Columns.Add("NAME", typeof(string));

            var printerQuery = new ManagementObjectSearcher("Select * from Win32_Printer");
     
            foreach (var printer in printerQuery.Get())
            {
                var name = printer.GetPropertyValue("Name");
                //var status = printer.GetPropertyValue("Status");
                //var isDefault = printer.GetPropertyValue("Default");
                //var isNetworkPrinter = printer.GetPropertyValue("Network");

                DataRow newRow = dtResult.NewRow();
                newRow.ItemArray = new object[] { name, name };
                dtResult.Rows.Add(newRow);
            }            

            cboPrinter.ItemsSource = DataTableConverter.Convert(dtResult);

            //if (LoginInfo.CFG_GENERAL_PRINTER.Rows.Count > 0)
            //        if (LoginInfo.CFG_GENERAL_PRINTER.Columns.Contains(CustomConfig.CONFIGTABLE_GENERALPRINTER_NAME))
            //            if (!string.IsNullOrEmpty(LoginInfo.CFG_GENERAL_PRINTER.Rows[0][CustomConfig.CONFIGTABLE_GENERALPRINTER_NAME].ToString()))
            //                cbo.SelectedValue = LoginInfo.CFG_GENERAL_PRINTER.Rows[0][CustomConfig.CONFIGTABLE_GENERALPRINTER_NAME].ToString();
        
        }
        #endregion

        #region [Save]        
        private bool SaveConfig()
        {
            return true;
        }
        #endregion
        

        #region [Func]
        private void ApplyPermissions()
        {
           // List<Button> listAuth = new List<Button>();
           // listAuth.Add(btnOK);
            
          //  Util.pageAuth(listAuth, FrameOperation.AUTHORITY);
        }
        #endregion

        #endregion

       
    }
}
