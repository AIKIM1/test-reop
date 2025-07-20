/*************************************************************************************
 Created Date : 2016.06.16
      Creator : 
   Decription : 
--------------------------------------------------------------------------------------
 [Change History]
  2016.06.16  DEVELOPER : Initial Created.





 
**************************************************************************************/

using System.Data;
using System.Windows;
using C1.WPF;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;

namespace LGC.GMES.MES.ASSY001
{
    public partial class ASSY001_031_ReserveStop : C1Window, IWorkArea
    {
        #region Declaration & Constructor 
        
        /// <summary>
        ///  Frame과 상호작용하기 위한 객체
        /// </summary>

        public IFrameOperation FrameOperation
        {
            get;
            set;
        }

        public ASSY001_031_ReserveStop()
        {
            InitializeComponent();
        }


        #endregion
        #region Initialize

        #endregion

        #region Event



        private void C1Window_Loaded(object sender, RoutedEventArgs e)
        {
            

           
            DataTable dt = new DataTable();
            dt.Columns.Add("CODE");
            dt.Columns.Add("NAME");

            DataRow dr = dt.NewRow();
            dr["CODE"] = "Y";
            dr["NAME"] = "Y";
            dt.Rows.Add(dr);

            dr = dt.NewRow();
            dr["CODE"] = "N";
            dr["NAME"] = "N";
            dt.Rows.Add(dr);

            cboReserveStop.DisplayMemberPath = "NAME";
            cboReserveStop.SelectedValuePath = "CODE";
            cboReserveStop.ItemsSource = dt.Copy().AsDataView();

            cboEqptStop.DisplayMemberPath = "NAME";
            cboEqptStop.SelectedValuePath = "CODE";
            cboEqptStop.ItemsSource = dt.Copy().AsDataView();            

            object[] tmps = C1WindowExtension.GetParameters(this);


            txtEqpt.Tag = tmps[0].ToString();
            txtEqpt.Text = tmps[1].ToString();

            cboReserveStop.SelectedValue = tmps[2].ToString();
            cboEqptStop.SelectedValue = tmps[3].ToString();

        }
        

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }





        #endregion

        #region Mehod

        #endregion

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(txtEqpt.Tag.ToString()))
                return;

            string sEqptID = txtEqpt.Tag.ToString();
            string sAUTO_RSV_STOP_FLAG = cboReserveStop.SelectedValue.ToString();
            string sEQPT_WRK_STOP_FLAG = cboEqptStop.SelectedValue.ToString();

            DataTable dt = new DataTable();
            dt.Columns.Add("EQPTID", typeof(string));
            dt.Columns.Add("AUTO_RSV_STOP_FLAG", typeof(string));
            dt.Columns.Add("EQPT_WRK_STOP_FLAG", typeof(string));

            DataRow dr = dt.NewRow();
            dr["EQPTID"] = sEqptID;
            dr["AUTO_RSV_STOP_FLAG"] = sAUTO_RSV_STOP_FLAG;
            dr["EQPT_WRK_STOP_FLAG"] = sEQPT_WRK_STOP_FLAG;

            dt.Rows.Add(dr);

            DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_PRD_UPD_VD_EQPT_RSV_CHG", "RQSTDT", null, dt);

            if (dtRslt == null)
            {
                Util.MessageInfo("PSS9097");    // 변경되었습니다.
                this.DialogResult = MessageBoxResult.OK;
                this.Close();
            }
        }
    }
}
