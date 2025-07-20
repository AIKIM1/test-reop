using C1.WPF;
using C1.WPF.DataGrid;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using System;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace LGC.GMES.MES.COM001
{
    /// <summary>
    /// COM001_014_AUTH_PERSON.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class COM001_014_AUTH_PERSON : C1Window, IWorkArea
    {
        #region Declaration & Constructor
        private string _AuthID = string.Empty;
        #endregion

        #region Initialize
        /// <summary> 
        ///  
        /// </summary>
        public IFrameOperation FrameOperation
        {
            get;
            set;
        }

        public string AUTHID
        {
            get { return _AuthID; }
        }

        public COM001_014_AUTH_PERSON()
        {
            InitializeComponent();
        }
        #endregion

        private void C1Window_Loaded(object sender, RoutedEventArgs e)
        {
            object[] tmps = C1WindowExtension.GetParameters(this);

            if (tmps != null && tmps.Length >= 1)
            {
                _AuthID = Util.NVC(tmps[0]);
            }

            GetAuthList();
        }

        private void GetAuthList()
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("AUTHID", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["AUTHID"] = _AuthID;
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_AUTH_PERSON", "RQSTDT", "RSLTDT", RQSTDT);

                Util.GridSetData(dgUser, dtResult, FrameOperation);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = MessageBoxResult.Cancel;
        }
    }
}
