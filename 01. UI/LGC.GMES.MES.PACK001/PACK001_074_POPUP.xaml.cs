/*************************************************************************************
 Created Date : 2020.09.15
      Creator : JYS
  Description : Check MMD Setting Nav.
--------------------------------------------------------------------------------------
 [Change History]
  2020.09.15  정용석 : Initial Created.
  2021.03.20  정용석 : 그리드 컬럼 명칭 변경
**************************************************************************************/
using C1.WPF;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using System;
using System.Data;
using System.Windows;
using System.Windows.Threading;

namespace LGC.GMES.MES.PACK001
{
    public partial class PACK001_074_POPUP : C1Window, IWorkArea
    {
        #region Declaration & Constructor 
        private string areaID = string.Empty;
        private string equipmentSegmentID = string.Empty;
        private string productID = string.Empty;
        private string commonCode = string.Empty;

        ///  Frame과 상호작용하기 위한 객체
        /// </summary>
        public IFrameOperation FrameOperation
        {
            get;
            set;
        }
        public PACK001_074_POPUP()
        {
            InitializeComponent();
        }
        #endregion

        #region Event 
        private void C1Window_Loaded(object sender, RoutedEventArgs e)
        {
            object[] obj = C1WindowExtension.GetParameters(this);

            if (obj != null && obj.Length > 0)
            {
                this.areaID = Util.NVC(obj[0]);
                this.equipmentSegmentID = Util.NVC(obj[1]);
                this.productID = Util.NVC(obj[2]);
                this.commonCode = Util.NVC(obj[3]);

                this.SearchDetail();
            }
        }
        #endregion

        #region Method
        private void SearchDetail()
        {
            Util.gridClear(this.dgRegistered);

            DataSet dsInputSet = new DataSet();
            DataSet dsOutputSet = new DataSet();
            try
            {
                DataTable dt = new DataTable("IN_DATA");
                dt.Columns.Add("LANGID", typeof(string));
                dt.Columns.Add("AREAID", typeof(string));
                dt.Columns.Add("EQSGID", typeof(string));
                dt.Columns.Add("PRODID", typeof(string));
                dt.Columns.Add("ISSUMMARY", typeof(string));
                dt.Columns.Add("CMCODE", typeof(string));
                dt.Columns.Add("REGISTER_YN", typeof(string));

                DataRow dr = dt.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["AREAID"] = this.areaID;
                dr["EQSGID"] = this.equipmentSegmentID;
                if (string.IsNullOrEmpty(this.productID) || this.productID.Equals("-ALL-"))
                {
                    dr["PRODID"] = null;
                }
                else
                {
                    dr["PRODID"] = this.productID;
                }
                dr["ISSUMMARY"] = "N";
                dr["CMCODE"] = this.commonCode;
                dr["REGISTER_YN"] = "Y";

                dt.Rows.Add(dr);
                dsInputSet.Tables.Add(dt);
                dsOutputSet = new ClientProxy().ExecuteServiceSync_Multi("BR_PRD_CHK_MMD_SETTING", "IN_DATA", "OUTDATA_DETAIL,OUTDATA", dsInputSet);

                if (dsOutputSet.Tables["OUTDATA_DETAIL"] != null && dsOutputSet.Tables["OUTDATA_DETAIL"].Rows.Count > 0)
                {
                    Util.GridSetData(this.dgRegistered, dsOutputSet.Tables["OUTDATA_DETAIL"], FrameOperation);
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void DoEvents()
        {
            Application.Current.Dispatcher.Invoke(DispatcherPriority.Background, new System.Threading.ThreadStart(delegate { }));
        }
        #endregion
    }
}
