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

using System.Collections.Generic;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using C1.WPF;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;

namespace LGC.GMES.MES.ProtoType04
{
    public partial class PGM_GUI_204 : UserControl, IWorkArea
    {
        #region Declaration & Constructor 
        public PGM_GUI_204()
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
            _combo.SetCombo(cboProcess, CommonCombo.ComboStatus.ALL, null, cbParent: cbProcessParent);


            ////동
            //C1ComboBox[] cboAreaChild = { cboEquipmentSegment };
            //_combo.SetCombo(cboArea, CommonCombo.ComboStatus.ALL, cbChild: cboAreaChild);

            ////라인
            //C1ComboBox[] cboLineParent = { cboArea };
            //C1ComboBox[] cboLineChild = { cboProcess };
            //_combo.SetCombo(cboEquipmentSegment, CommonCombo.ComboStatus.ALL, cbChild: cboLineChild, cbParent: cboLineParent);

            //////공정
            //C1ComboBox[] cbProcessParent = { cboEquipmentSegment };
            //_combo.SetCombo(cboProcess, CommonCombo.ComboStatus.ALL, null, cbParent: cbProcessParent);

        }
        #endregion

        #region Event

        #endregion

        #region Mehod

        #endregion

        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            string sArea = cboArea.SelectedValue.ToString();
            string sProcess = cboProcess.SelectedValue.ToString();
            string sLine = cboEquipmentSegment.SelectedValue.ToString();
            //string sLocation = cboLocation.SelectedValue.ToString();

            if (rdoIN.IsChecked.Value)
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("FROM_AREAID", typeof(string));
                RQSTDT.Columns.Add("FROM_PROCID", typeof(string));
                RQSTDT.Columns.Add("FROM_EQSGID", typeof(string));
                //RQSTDT.Columns.Add("FROM_SLOCID", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["FROM_AREAID"] = sArea;
                dr["FROM_PROCID"] = sProcess;
                dr["FROM_EQSGID"] = sLine;
                //dr["FROM_SLOCID"] = sLocation;
                RQSTDT.Rows.Add(dr);

                //DataRow dr = RQSTDT.NewRow();
                //dr["FROM_AREAID"] = "E1";
                //dr["FROM_PROCID"] = "E2000";
                //dr["FROM_EQSGID"] = "E1D02";
                ////dr["FROM_SLOCID"] = "V1100";
                //RQSTDT.Rows.Add(dr);

                DataTable SearchResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_MOVE_PLAN", "RQSTDT", "RSLTDT", RQSTDT);

                dgMovePlan.ItemsSource = DataTableConverter.Convert(SearchResult);

                //new ClientProxy().ExecuteService("DA_PRD_SEL_MOVE_PLAN", "RQSTDT", "RSLTDT", RQSTDT, (result, ex) =>
                //{
                //    loadingIndicator.Visibility = Visibility.Collapsed;
                //    if (ex != null)
                //    {
                //        //LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("예외발생" + ex.ToString()), null, "Info", MessageBoxButton.OK, MessageBoxIcon.Warning);
                //        return;
                //    }
                //    dgMovePlan.ItemsSource = DataTableConverter.Convert(result);

                //});
            }
            else
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("TO_AREAID", typeof(string));
                RQSTDT.Columns.Add("TO_PROCID", typeof(string));
                RQSTDT.Columns.Add("TO_EQSGID", typeof(string));
                //RQSTDT.Columns.Add("FROM_SLOCID", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["TO_AREAID"] = sArea;
                dr["TO_PROCID"] = sProcess;
                dr["TO_EQSGID"] = sLine;
                //dr["FROM_SLOCID"] = sLocation;
                RQSTDT.Rows.Add(dr);

                //DataRow dr = RQSTDT.NewRow();
                //dr["FROM_AREAID"] = "E1";
                //dr["FROM_PROCID"] = "E2000";
                //dr["FROM_EQSGID"] = "E1D02";
                ////dr["FROM_SLOCID"] = "V1100";
                //RQSTDT.Rows.Add(dr);

                DataTable SearchResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_MOVE_PLAN_OUT", "RQSTDT", "RSLTDT", RQSTDT);

                dgMovePlan.ItemsSource = DataTableConverter.Convert(SearchResult);
            }
        }
    }
}