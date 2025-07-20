/*************************************************************************************
 Created Date : 2016.10.15
      Creator : 
   Decription : 
--------------------------------------------------------------------------------------
 [Change History]
  2016.06.16  DEVELOPER : Initial Created.
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
    public partial class ELEC001_008_LOTEND : C1Window, IWorkArea
    {
        #region Declaration & Constructor 
        private static string PRODID = "";
        private static string WORKDATE = "";
        private static string LOTID = "";
        private static string STATUS = "";
        private static string EQPTID = "";

        /// <summary>
        ///  Frame과 상호작용하기 위한 객체
        /// </summary>
        public IFrameOperation FrameOperation
        {
            get;
            set;
        }

        public ELEC001_008_LOTEND()
        {
            InitializeComponent();
        }

        private void InitializeControls()
        {
        }
        #endregion

        #region Initialize

        #endregion

        #region Event
        private void C1Window_Loaded(object sender, RoutedEventArgs e)
        {
            object[] tmps = C1WindowExtension.GetParameters(this);

            DataRowView rowview = tmps[0] as DataRowView;

            if (rowview == null)
                return;

            InitializeControls();

            PRODID = rowview[0].ToString();
            WORKDATE = rowview[1].ToString();
            LOTID = rowview[2].ToString();
            STATUS = rowview[3].ToString();
            EQPTID = rowview[4].ToString();

            txtProdID.Text = PRODID;
            txtWorkDate.Text = WORKDATE;
            txtLotID.Text = LOTID;
            txtWipstat.Text = STATUS;

            dtpDate.SelectedDateTime = System.DateTime.Now;
            TimeEditor.Value = new TimeSpan(DateTime.Now.Hour, DateTime.Now.Minute, DateTime.Now.Second);
        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {

            DataTable inDataTable = new DataTable();
            inDataTable.Columns.Add("LOTID", typeof(string));
            inDataTable.Columns.Add("PROCID", typeof(string));
            inDataTable.Columns.Add("EQPTID", typeof(string));
            inDataTable.Columns.Add("INPUT_QTY", typeof(decimal));
            inDataTable.Columns.Add("CTRLQTY", typeof(decimal));
            inDataTable.Columns.Add("FINAL_CHECK", typeof(string));
            inDataTable.Columns.Add("USERID", typeof(string));

            DataRow Indata = inDataTable.NewRow();
            Indata["LOTID"] = LOTID;
            Indata["PROCID"] = Process.COATING;
            Indata["EQPTID"] = EQPTID;
            Indata["INPUT_QTY"] = Util.NVC_Decimal(txtGoodqty.Text);
            Indata["CTRLQTY"] = txtCtrlqty.Text == "" ? 0 : Util.NVC_Decimal(txtCtrlqty.Text);
            Indata["WFINAL_CHECK"] = chkFinalCut.IsChecked == true ? "Y" : "N";
            Indata["USERID"] = LoginInfo.USERID;
            inDataTable.Rows.Add(Indata);

            //MUST_BIZ_APPLY
            new ClientProxy().ExecuteService("BR_PRD_REG_END_LOT_CT_SINGLE", "INDATA", "RSLTDT", inDataTable, (result, ex) =>
            {
                if (ex != null)
                {
                    //LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("SFU1737" + ex.ToString()), null, "Info", MessageBoxButton.OK, MessageBoxIcon.Warning);
                    Util.AlertByBiz("BR_PRD_REG_END_LOT_CT_SINGLE", ex.Message, ex.ToString());
                    return;
                }

                Util.AlertInfo("SFU1275");  //정상처리되었습니다.
                btnSave.IsEnabled = false;
            });
        }

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
        #endregion

        #region Mehod

        #endregion
    }
}