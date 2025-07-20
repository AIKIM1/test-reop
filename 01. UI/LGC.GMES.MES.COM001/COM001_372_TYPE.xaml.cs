/*************************************************************************************
 Created Date : 2017.01.24
      Creator : 
   Decription : 계획버전 등록
--------------------------------------------------------------------------------------
 [Change History]
  2017.01.24  DEVELOPER : Initial Created.


**************************************************************************************/

using C1.WPF;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using System;
using System.Data;
using System.Windows;

namespace LGC.GMES.MES.COM001
{
    public partial class COM001_372_TYPE : C1Window, IWorkArea
    {
        #region Declaration & Constructor 
        DataTable result = new DataTable();

        private string _LOTID = string.Empty;
        private string _WIPSEQ = string.Empty;
        private string _CT_LOT_YN = string.Empty;

        Util _Util = new Util();

        public COM001_372_TYPE()
        {
            InitializeComponent();
        }

        /// <summary>
        ///  Frame과 상호작용하기 위한 객체
        /// </summary>
        public IFrameOperation FrameOperation
        {
            get;
            set;
        }
        #endregion

        #region Initialize
        private void C1Window_Loaded(object sender, RoutedEventArgs e)
        {           
            
            object[] parameters = C1WindowExtension.GetParameters(this);

            if (parameters == null)
            {
                this.DialogResult = MessageBoxResult.Cancel;
                this.Close();
            }

            _LOTID = Util.NVC(parameters[0]);
            _WIPSEQ = Util.NVC(parameters[1]);
            _CT_LOT_YN = Util.NVC(parameters[2]);


            SetData();
        }
        #endregion


        #region Mehod
        private void SetData()
        {
            try
            {
                loadingIndicator.Visibility = Visibility.Visible;
                Util.gridClear(dgType);

                DataTable dtType = new DataTable("RQSTDT");
                dtType.Columns.Add("LOTID", typeof(string));
                dtType.Columns.Add("WIPSEQ", typeof(string));
                dtType.Columns.Add("CT_LOT_YN", typeof(string));


                DataRow drType = dtType.NewRow();
                drType["LOTID"] = _LOTID;
                drType["WIPSEQ"] = _WIPSEQ;
                drType["CT_LOT_YN"] = _CT_LOT_YN;

                dtType.Rows.Add(drType);                               

                new ClientProxy().ExecuteService("DA_PRD_SEL_ROLLMAP_MARK", "RQSTDT", "RSLTDT", dtType, (result, ex) =>
                {
                    loadingIndicator.Visibility = Visibility.Collapsed;

                    if (ex != null)
                    {
                        return;
                    }

                    Util.GridSetData(dgType, result, FrameOperation, true);                    
                });
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        #endregion

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
