/*************************************************************************************
 Created Date : 2016.06.16
      Creator : 
   Decription : 동간이동 계획조회
--------------------------------------------------------------------------------------
 [Change History]
  2016.06.16  DEVELOPER : Initial Created.

  * TABLE 
    TB_SFC_FP_MOVE_ORD
  * BIZ NAME
       


 
**************************************************************************************/

using C1.WPF;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;
using System.Collections.Generic;
using System.Data;
using System.Windows;
using System.Windows.Controls;

namespace LGC.GMES.MES.COM001
{
    public partial class COM001_038 : UserControl, IWorkArea
    {
        #region Declaration & Constructor
        Util _Util = new Util();
        private string _SHOPID = string.Empty;
        public COM001_038()
        {
            InitializeComponent();
            Initialize();
        }
        public IFrameOperation FrameOperation
        {
            get;
            set;
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            List<Button> listAuth = new List<Button>();
            //listAuth.Add(btnInReplace);

            Util.pageAuth(listAuth, FrameOperation.AUTHORITY);

        }

        #endregion

        #region Initialize
        private void Initialize()
        {
            CommonCombo _combo = new CommonCombo();


            //동
            C1ComboBox[] cboAreaChild = { cboEquipmentSegment };
            _combo.SetCombo(cboArea, CommonCombo.ComboStatus.ALL, cbChild: cboAreaChild);

            //라인
            C1ComboBox[] cboLineParent = { cboArea };
            C1ComboBox[] cboLineChild = { cboProcess };
            _combo.SetCombo(cboEquipmentSegment, CommonCombo.ComboStatus.ALL, cbChild: cboLineChild, cbParent: cboLineParent);

            //공정
            C1ComboBox[] cbProcessParent = { cboEquipmentSegment };
            C1ComboBox[] cbProcessChild = { cboModel };
            _combo.SetCombo(cboProcess, CommonCombo.ComboStatus.ALL, cbChild: cbProcessChild, cbParent: cbProcessParent);
            //제품  
            //C1ComboBox[] cbProductParent = { cboArea, cboEquipmentSegment, cboProcess };
            //C1ComboBox[] cbProductChild = { cboModel };
            //_combo.SetCombo(cboProduct, CommonCombo.ComboStatus.ALL, cbChild: cbProductChild, cbParent: cbProductParent, sCase: "PRODUCT");

            C1ComboBox[] cboMountPstParent = { cboEquipmentSegment,cboProcess};
            _combo.SetCombo(cboModel, CommonCombo.ComboStatus.ALL, cbParent: cboMountPstParent, sCase: "cboEqptModel");

        }
        #endregion

        #region Event

        #endregion

        #region Mehod

        #endregion

        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            //cboArea
            if (cboArea.SelectedIndex < 0 || cboArea.SelectedValue.ToString().Equals("SELECT"))
            {
                Util.Alert("SFU1499");  //동을 선택하세요.
                return;
            }

            if (cboEquipmentSegment.SelectedIndex < 0 || cboEquipmentSegment.SelectedValue.ToString().Equals("SELECT"))
            {
                Util.Alert("SFU1223");  //라인을 선택하세요.
                return;
            }

            if (cboProcess.SelectedIndex < 0 || cboProcess.SelectedValue.ToString().Equals("SELECT"))
            {
                Util.Alert("SFU1459");  //공정을 선택하세요.
                return;
            }


            string sArea = cboArea.SelectedValue.ToString();
            string sProcess = cboProcess.SelectedValue.ToString();
            string sLine = cboEquipmentSegment.SelectedValue.ToString();
            //string sProd = cboProduct.SelectedValue.ToString();
            string sModel = cboModel.SelectedValue.ToString();

            if (sArea == "")
            {
                sArea = null;
            }

            if (sProcess == "")
            {
                sProcess = null;
            }

            if (sLine == "")
            {
                sLine = null;
            }

            //if (sProd == "")
            //{
            //    sProd = null;
            //}

            if (sModel == "")
            {
                sModel = null;
            }


            if (rdoIN.IsChecked.Value)
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("FROM_AREAID", typeof(string));
                RQSTDT.Columns.Add("FROM_PROCID", typeof(string));
                RQSTDT.Columns.Add("FROM_EQSGID", typeof(string));
                RQSTDT.Columns.Add("LOTDTTM_CR_FROM", typeof(string));
                RQSTDT.Columns.Add("LOTDTTM_CR_TO", typeof(string));
              //  RQSTDT.Columns.Add("PRODID", typeof(string));
                RQSTDT.Columns.Add("MODLID", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["FROM_AREAID"] = sArea;
                dr["FROM_PROCID"] = sProcess;
                dr["FROM_EQSGID"] = sLine;
                dr["LOTDTTM_CR_FROM"] = dtpFrom.SelectedDateTime.ToString("yyyyMMdd");
                dr["LOTDTTM_CR_TO"] = dtpTo.SelectedDateTime.ToString("yyyyMMdd");
               // dr["PRODID"] = sProd;
                dr["MODLID"] = sModel;
                RQSTDT.Rows.Add(dr);


                DataTable SearchResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_MOVE_PLAN", "RQSTDT", "RSLTDT", RQSTDT);

                dgMovePlan.ItemsSource = DataTableConverter.Convert(SearchResult);

            }
            else
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("TO_AREAID", typeof(string));
                RQSTDT.Columns.Add("TO_PROCID", typeof(string));
                RQSTDT.Columns.Add("TO_EQSGID", typeof(string));
                RQSTDT.Columns.Add("LOTDTTM_CR_FROM", typeof(string));
                RQSTDT.Columns.Add("LOTDTTM_CR_TO", typeof(string));
             //   RQSTDT.Columns.Add("PRODID", typeof(string));
                RQSTDT.Columns.Add("MODLID", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["TO_AREAID"] = sArea;
                dr["TO_PROCID"] = sProcess;
                dr["TO_EQSGID"] = sLine;
                dr["LOTDTTM_CR_FROM"] = dtpFrom.SelectedDateTime.ToString("yyyyMMdd");
                dr["LOTDTTM_CR_TO"] = dtpTo.SelectedDateTime.ToString("yyyyMMdd");
              //  dr["PRODID"] = sProd;
                dr["MODLID"] = sModel;
                RQSTDT.Rows.Add(dr);


                DataTable SearchResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_MOVE_PLAN_OUT", "RQSTDT", "RSLTDT", RQSTDT);

                dgMovePlan.ItemsSource = DataTableConverter.Convert(SearchResult);
            }
        }
    }
}
