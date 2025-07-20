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
using LGC.GMES.MES.CMM001;
using LGC.GMES.MES.CMM001.Extensions;

namespace LGC.GMES.MES.FORM001
{
    public partial class FORM001_CHANGE_AOMM_GRADE_CONFIRM : C1Window, IWorkArea
    {
        string _sCtnrID = string.Empty;
        #region Initialize 
        /// <summary>
        ///  Frame과 상호작용하기 위한 객체
        /// </summary>
        public IFrameOperation FrameOperation
        {
            get;
            set;
        }
        public FORM001_CHANGE_AOMM_GRADE_CONFIRM()
        {
            InitializeComponent();
        }
        #endregion

        #region Event

        private void C1Window_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                object[] tmps = C1WindowExtension.GetParameters(this);

                _sCtnrID = tmps[0] as string;

                if (string.IsNullOrEmpty(_sCtnrID))
                    return;

                GetCtnrInfo(_sCtnrID);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void GetCtnrInfo(string sCtnrID)
        {
            try
            {
                DataTable dtRqst = new DataTable();
                dtRqst.Columns.Add("CTNR_ID", typeof(string));

                DataRow newRow = dtRqst.NewRow();
                newRow["CTNR_ID"] = sCtnrID;
                dtRqst.Rows.Add(newRow);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_POLYMER_CART_TAG_LIST", "INDATA", "OUTDATA", dtRqst);

                Util.GridSetData(dgCtnr, dtRslt, FrameOperation, false);

            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }

        }

        private void btnClose_Clicked(object sender, RoutedEventArgs e)
        {
            this.DialogResult = MessageBoxResult.Cancel;
        }
        #endregion

        #region Mehod
     

        #endregion


    }
}
