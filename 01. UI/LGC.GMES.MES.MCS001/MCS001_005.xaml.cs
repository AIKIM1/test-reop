/*************************************************************************************
 Created Date : 2018.10.18
      Creator : 오화백
   Decription : MEB 라미창고 입출고 이력 조회
--------------------------------------------------------------------------------------
 [Change History]
  2018.10.18  DEVELOPER : Initial Created.


 
**************************************************************************************/

using C1.WPF;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using System;
using System.Data;
using System.Windows;
using System.Windows.Controls;

namespace LGC.GMES.MES.MCS001
{
    public partial class MCS001_005 : UserControl, IWorkArea
    {
        #region Declaration & Constructor 
        DataTable dtMain = new DataTable();
        Util _Util = new Util();


        public MCS001_005()
        {
            InitializeComponent();
            InitCombo();
        }
        public IFrameOperation FrameOperation
        {
            get;
            set;
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {

            if (this.FrameOperation.Parameters != null && this.FrameOperation.Parameters.Length > 0)
            {
                Array ary = FrameOperation.Parameters;
                txtLot.Text = ary.GetValue(0).ToString();
                GetResult();
            }
           

            this.Loaded -= UserControl_Loaded;
        }
        #endregion


        #region Initialize

        #endregion
        
        #region Event
        private void InitCombo()
        {
            try
            {
                CommonCombo _combo = new CommonCombo();

                //Stocker
                String[] sFilter2 = { "Y", "", "NPB" };
                C1ComboBox[] cboAreaChild = { cboPortID };
                _combo.SetCombo(cboStocker, CommonCombo.ComboStatus.ALL, cbChild: cboAreaChild, sFilter: sFilter2, sCase: "CWALAMISTOCKER");

                //PORTID
                C1ComboBox[] cboStockerParent = { cboStocker };
                _combo.SetCombo(cboPortID, CommonCombo.ComboStatus.ALL, cbParent: cboStockerParent, sCase: "CWALAMIPORT");
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        private void btnSearch(object sender, RoutedEventArgs e)
        {
            GetResult();
        }
        #endregion

        #region Mehod
        private void GetResult()
        {
            //if (Convert.ToDecimal(Convert.ToDateTime(dtpFrom.SelectedDateTime).ToString("yyyyMMdd")) > Convert.ToDecimal(Convert.ToDateTime(dtpTo.SelectedDateTime).ToString("yyyyMMdd")))
            //{
            //    Util.Alert("SFU1913");  //종료일자가 시작일자보다 빠릅니다.
            //    return;
            //}
            //string sProcess = cboProcess.SelectedValue.ToString();
            //string sLine = cboEquipmentSegment.SelectedValue.ToString();
            //string sEquip = cboEquipment.SelectedValue.ToString();
            //string sProd = cboProd.SelectedValue.ToString();
            //string sLotId = null;


            //if (sProcess == "")
            //{
            //    sProcess = null;
            //}
            //if (sLine == "")
            //{
            //    sLine = null;
            //}
            //if (sEquip == "")
            //{
            //    sEquip = null;
            //}
            //if (sProd == "")
            //{
            //    sProd = null;
            //}

            //DataTable inTable = new DataTable();
            //inTable.Columns.Add("LANGID", typeof(string));
            //inTable.Columns.Add("ACTID", typeof(string));
            //inTable.Columns.Add("EQSGID", typeof(string));
            //inTable.Columns.Add("PROCID", typeof(string));
            //inTable.Columns.Add("EQPTID", typeof(string));
            //inTable.Columns.Add("PRODID", typeof(string));
            //inTable.Columns.Add("LOTID", typeof(string));
            //inTable.Columns.Add("ACTFROM", typeof(string));
            //inTable.Columns.Add("ACTTO", typeof(string));

            //DataRow newRow = inTable.NewRow();
            //newRow["LANGID"] = LoginInfo.LANGID;
            //newRow["ACTID"] = "MODIFY_LOT";
            //newRow["EQSGID"] = sLine;
            //newRow["PROCID"] = sProcess;
            //newRow["EQPTID"] = sEquip;
            //newRow["PRODID"] = sProd;
            //if (txtLotId.Text == "")
            //{
            //    newRow["LOTID"] = sLotId;
            //}
            //else
            //{
            //    newRow["LOTID"] = txtLotId.Text;
            //}
            //newRow["ACTFROM"] = dtpFrom.SelectedDateTime.ToString("yyyyMMdd");
            //newRow["ACTTO"] = dtpTo.SelectedDateTime.ToString("yyyyMMdd");

            //inTable.Rows.Add(newRow);

            //dtMain = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_MODYFY_LOT", "INDATA", "OUTDATA", inTable);

            //dgLotList.ItemsSource = DataTableConverter.Convert(dtMain);

            //string[] sColumnName = new string[] { "EQSGNAME", "PROCNAME", "EQPTNAME", "PRODID","PRODNAME","LOTID"};
            //_Util.SetDataGridMergeExtensionCol(dgLotList, sColumnName, DataGridMergeMode.VERTICALHIERARCHI);
        }
        #endregion
    }
}
