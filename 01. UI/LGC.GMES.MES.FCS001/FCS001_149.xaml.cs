/*************************************************************************************
 Created Date : 2023.05.16
      Creator : 
   Decription : 단적재 출고
--------------------------------------------------------------------------------------
 [Change History]
  2023.05.16  조영대 : Initial Created.
**************************************************************************************/

using System;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

using C1.WPF;
using C1.WPF.DataGrid;

using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;


namespace LGC.GMES.MES.FCS001
{
    public partial class FCS001_149 : UserControl, IWorkArea
    {
        #region Declaration & Constructor 

        Util _Util = new Util();
        DataTable dtHeader = new DataTable();

        public FCS001_149()
        {
            InitializeComponent();
        }
        #endregion

        #region Initialize
        public IFrameOperation FrameOperation
        {
            get;
            set;
        }

        private void InitCombo()
        {
            CommonCombo_Form _combo = new CommonCombo_Form();
            C1ComboBox[] cboLineChild = { cboModel };
            _combo.SetCombo(cboLine, CommonCombo_Form.ComboStatus.ALL, sCase: "LINE", cbChild: cboLineChild);

            C1ComboBox[] cboModelParent = { cboLine };
            _combo.SetCombo(cboModel, CommonCombo_Form.ComboStatus.ALL, sCase: "LINEMODEL", cbParent: cboModelParent);

            string[] arrColum = { "LANGID", "AREAID" };
            string[] arrCondition = { LoginInfo.LANGID, LoginInfo.CFG_AREA_ID };
            cboShipLoc.SetDataComboItem("DA_SEL_AUTO_STACK_SHIP_LOC", arrColum, arrCondition);
        }

        private void InitControl()
        {
            dtpWorkDate.SelectedFromDateTime = DateTime.Now;
            dtpWorkDate.SelectedToDateTime = DateTime.Now;
        }
        #endregion

        #region Method
        
        private void GetList()
        {
            try
            {
                dgLoadShipStatus.ClearRows();

                DataTable dtRqst = new DataTable();
                dtRqst.TableName = "RQSTDT";
                dtRqst.Columns.Add("LANGID", typeof(string));
                dtRqst.Columns.Add("AREAID", typeof(string));
                dtRqst.Columns.Add("EQSGID", typeof(string));
                dtRqst.Columns.Add("MDLLOT_ID", typeof(string));
                dtRqst.Columns.Add("FROM_DATE", typeof(string));
                dtRqst.Columns.Add("TO_DATE", typeof(string));
                dtRqst.Columns.Add("SHIP_LOC", typeof(string));

                DataRow drStatus = dtRqst.NewRow();
                drStatus["LANGID"] = LoginInfo.LANGID;
                drStatus["AREAID"] = LoginInfo.CFG_AREA_ID;
                drStatus["EQSGID"] = cboLine.GetBindValue();
                drStatus["MDLLOT_ID"] = cboModel.GetBindValue();
                drStatus["FROM_DATE"] = dtpWorkDate.SelectedFromDateTime.ToString("yyyyMMddHHmmss");
                drStatus["TO_DATE"] = dtpWorkDate.SelectedToDateTime.ToString("yyyyMMddHHmmss");
                drStatus["SHIP_LOC"] = cboShipLoc.GetBindValue();
                dtRqst.Rows.Add(drStatus);

                dgLoadShipStatus.ExecuteService("BR_GET_AUTO_STACK_SHIP_LIST", "INDATA", "OUTDATA,STACK_INFO", dtRqst);
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }

        private void GetListHist()
        {
            try
            {
                dgLoadShipHist.ClearRows();

                DataTable dtRqst = new DataTable();
                dtRqst.TableName = "RQSTDT";
                dtRqst.Columns.Add("LANGID", typeof(string));
                dtRqst.Columns.Add("AREAID", typeof(string));
                dtRqst.Columns.Add("EQSGID", typeof(string));
                dtRqst.Columns.Add("MDLLOT_ID", typeof(string));
                dtRqst.Columns.Add("FROM_DATE", typeof(string));
                dtRqst.Columns.Add("TO_DATE", typeof(string));
                dtRqst.Columns.Add("SHIP_LOC", typeof(string));

                DataRow drStatus = dtRqst.NewRow();
                drStatus["LANGID"] = LoginInfo.LANGID;
                drStatus["AREAID"] = LoginInfo.CFG_AREA_ID;
                drStatus["EQSGID"] = cboLine.GetBindValue();
                drStatus["MDLLOT_ID"] = cboModel.GetBindValue();
                drStatus["FROM_DATE"] = dtpWorkDate.SelectedFromDateTime.ToString("yyyyMMddHHmmss");
                drStatus["TO_DATE"] = dtpWorkDate.SelectedToDateTime.ToString("yyyyMMddHHmmss");
                drStatus["SHIP_LOC"] = cboShipLoc.GetBindValue();
                dtRqst.Rows.Add(drStatus);

                dgLoadShipHist.ExecuteService("DA_SEL_AUTO_STACK_SHIP_HIST", "RQSTDT", "RSLTDT", dtRqst);
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }

        #endregion

        #region Event

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            InitCombo();
            InitControl();
 
            this.Loaded -= UserControl_Loaded;
        }

        private void dgLoadShipStatus_ExecuteCustomBinding(object sender, CMM001.Controls.UcBaseDataGrid.ExecuteDataEventArgs e)
        {
            DataSet ds = e.ResultData as DataSet;
            Util.GridSetData(dgLoadShipStatus, ds.Tables["STACK_INFO"], FrameOperation);
        }

        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            GetList();
        }

        private void btnSearch2_Click(object sender, RoutedEventArgs e)
        {
            GetListHist();
        }

        private void btnAutoConf_Click(object sender, RoutedEventArgs e)
        {
            try
            {

                FCS001_149_AUTO_CONF popAutoConfig = new FCS001_149_AUTO_CONF();

                if (popAutoConfig != null)
                {
                    object[] Parameters = new object[1];
                    Parameters[0] = cboShipLoc.GetBindValue();

                    ControlsLibrary.C1WindowExtension.SetParameters(popAutoConfig, Parameters);
                    
                    this.Dispatcher.BeginInvoke(new Action(() => popAutoConfig.ShowModal()));
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }


        #endregion

      
    }

}
