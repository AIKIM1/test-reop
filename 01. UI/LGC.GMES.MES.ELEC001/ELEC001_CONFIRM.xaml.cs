/*************************************************************************************
 Created Date : 2016.08.19
      Creator : 
   Decription : LOT Confirm
--------------------------------------------------------------------------------------
 [Change History]
  2016.08.19  : Initial Created.
  
**************************************************************************************/

using C1.WPF;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using System;
using System.Data;
using System.Windows;

namespace LGC.GMES.MES.ELEC001
{
    /// <summary>
    /// ELEC001_006_LOTSTART
    /// </summary>
    public partial class ELEC001_CONFIRM : C1Window, IWorkArea
    {
        #region Declaration & Constructor
        public string _LOTID = string.Empty;
        public string _MSG = string.Empty;
        Util _Util = new Util();
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
        public ELEC001_CONFIRM()
        {
            InitializeComponent();
        }

        private void InitializeControls()
        {

        }
        #endregion

        #region Event
        private void C1Window_Loaded(object sender, RoutedEventArgs e)
        {
            object[] tmps = C1WindowExtension.GetParameters(this);

            DataRowView rowview = tmps[0] as DataRowView;

            if (rowview == null)
            {
                return;
            }

            _LOTID = rowview[0].ToString();
            _MSG = rowview[1].ToString();

            InitializeControls();


        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = MessageBoxResult.OK;
        }

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = MessageBoxResult.Cancel;
        }
        
        #endregion

        #region Mehod

        #region [BizCall]
        private void GetLotInfo()
        {
            try
            {

                DataTable IndataTable = new DataTable();
                IndataTable.Columns.Add("LOTID", typeof(string));

                DataRow Indata = IndataTable.NewRow();
                Indata["LOTID"] = _LOTID;
                IndataTable.Rows.Add(Indata);
                //MUST_BIZ_APPLY
                DataTable dtMain = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_CHILDLOT_LIST", "INDATA", "RSLTDT", IndataTable);

                dgConfirmLot.ItemsSource = DataTableConverter.Convert(dtMain);

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
            
        }
        #endregion
        

        #endregion


    }
}
