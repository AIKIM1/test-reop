/*************************************************************************************
 Created Date : 2016.06.16
      Creator : 
   Decription : 
--------------------------------------------------------------------------------------
 [Change History]
  2016.06.16  DEVELOPER : Initial Created.





 
**************************************************************************************/

using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using C1.WPF;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;

namespace LGC.GMES.MES.ProtoType04
{
    public partial class PGM_GUI_149 : UserControl, IWorkArea
    {
        #region Declaration & Constructor 
        public PGM_GUI_149()
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
            _combo.SetCombo(cboArea, CommonCombo.ComboStatus.SELECT, cbChild: cboAreaChild);

            //라인
            C1ComboBox[] cboLineParent = { cboArea };
            C1ComboBox[] cboLineChild = { cboProcess };
            _combo.SetCombo(cboEquipmentSegment, CommonCombo.ComboStatus.SELECT, cbChild: cboLineChild, cbParent: cboLineParent);

            ////공정
            C1ComboBox[] cbProcessParent = { cboEquipmentSegment };
            _combo.SetCombo(cboProcess, CommonCombo.ComboStatus.SELECT, null, cbParent: cbProcessParent);

            String[] sFilter3 = { "ELECTYPE" };
            _combo.SetCombo(cboElecType, CommonCombo.ComboStatus.SELECT, sFilter: sFilter3, sCase: "COMMCODE");

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
            string sType = cboType.SelectedValue.ToString();

            string sElectrode = cboElecType.SelectedValue.ToString();
            string sLocation = cboLocation.SelectedValue.ToString();
            string sModel = cboModel.SelectedValue.ToString();

            string sLotid = txtLOTID.ToString();
            //string sKeyword = txtKeyword.ToString();

            string sType1 = string.Empty;
            string sType2 = string.Empty;
            string sType3 = string.Empty;
            string sType4 = string.Empty;

            string sBizname = string.Empty;

            if (sType == "P")
            {
                sType1 = "P";
                sType2 = "P";
                sType3 = "P";
                sType4 = "P";
            }
            else if (sType == "J")
            {
                sType1 = "C";
                sType2 = "R";
                sType3 = "W1";
                sType4 = "W2";
            }
            else
            {
                sType1 = "P";
                sType2 = "P";
                sType3 = "P";
                sType4 = "P";
            }

            if (!string.IsNullOrEmpty(sLotid))
            {
                sElectrode = "";
                sModel = "";
                sProcess = "";
                sType1 = "";
                sType2 = "";
                sType3 = "";
                sType4 = "";
            }

            if (sLocation == "IN")
            {
                // Bizname 1
            }
            else if (sLocation == "OUT")
            {
                // Bizname 2
            }
            else
            {
                // Bizname 3
            }

            //DataTable RQSTDT = new DataTable();
            //RQSTDT.TableName = "RQSTDT";
            //RQSTDT.Columns.Add("AREAID", typeof(string));
            //RQSTDT.Columns.Add("MODELID", typeof(string));
            //RQSTDT.Columns.Add("ELECTRODE", typeof(string));
            //RQSTDT.Columns.Add("FROM_SLOCID", typeof(string));

            //DataRow dr = RQSTDT.NewRow();
            //dr["FROM_AREAID"] = sShop;
            //dr["FROM_PROCID"] = sProd;
            //dr["FROM_EQSGID"] = sLine;
            //dr["FROM_SLOCID"] = sLocation;
            //RQSTDT.Rows.Add(dr);

            //new ClientProxy().ExecuteService(sBizname, "RQSTDT", "RSLTDT", RQSTDT, (result, ex) =>
            //{
            //    loadingIndicator.Visibility = Visibility.Collapsed;
            //    if (ex != null)
            //    {
            //        //LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("예외발생" + ex.ToString()), null, "Info", MessageBoxButton.OK, MessageBoxIcon.Warning);
            //        return;
            //    }
            //    dgStoreHist.ItemsSource = DataTableConverter.Convert(result);

            //});

        }

        private void cboArea_SelectedItemChanged(object sender, PropertyChangedEventArgs<object> e)
        {

        }
    }
}
