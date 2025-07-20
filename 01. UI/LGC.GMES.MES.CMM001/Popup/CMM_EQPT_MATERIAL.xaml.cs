/*******************************
 Created Date :
      Creator :
   Decription : Recipe No.
--------------------------------
 [Change History]
  2016.08.19  : Initial Created.
********************************/

using C1.WPF;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using System;
using System.Data;
using System.Windows;
using System.Windows.Controls;

namespace LGC.GMES.MES.CMM001.Popup
{
    /// <summary>
    /// ELEC001_006_LOTEND.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class CMM_EQPT_MATERIAL : C1Window, IWorkArea
    {
        #region Declaration & Constructor
        private string _LOTID = string.Empty;
        private string _EQPTID = string.Empty;
        private string _EQPTNAME = string.Empty;
        private string _WOID = string.Empty;

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
        public CMM_EQPT_MATERIAL()
        {
            InitializeComponent();
        }
        #endregion

        #region Event
        private void C1Window_Loaded(object sender, RoutedEventArgs e)
        {
            object[] tmps = C1WindowExtension.GetParameters(this);

            if (tmps == null)
                return;

            _LOTID = Util.NVC(tmps[0]);
            _WOID = Util.NVC(tmps[1]);
            _EQPTID = Util.NVC(tmps[2]);
            _EQPTNAME = Util.NVC(tmps[3]);
            GetMaterial();
        }
        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = MessageBoxResult.OK;
        }
        #endregion

        #region Mehod
        private void GetMaterial()
        {
            try
            {
                Util.gridClear(dgMaterial);

                DataTable IndataTable = new DataTable();
                IndataTable.Columns.Add("LANGID", typeof(string));
                IndataTable.Columns.Add("LOTID", typeof(string));
                IndataTable.Columns.Add("EQPTID", typeof(string));

                DataRow Indata = IndataTable.NewRow();
                Indata["LANGID"] = LoginInfo.LANGID;
                Indata["LOTID"] = _LOTID;
                Indata["EQPTID"] = _EQPTID;
                IndataTable.Rows.Add(Indata);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_CONSUME_EQPT_MATERIAL", "INDATA", "RSLTDT", IndataTable);

                Util.GridSetData(dgMaterial, dtResult, null, true);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        #endregion
        
    }
}