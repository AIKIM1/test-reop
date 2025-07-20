/*************************************************************************************
 Created Date : 2022.12.01
      Creator : KIM TAEKYUN
   Decription : Tray List
--------------------------------------------------------------------------------------
 [Change History]
------------------------------------------------------------------------------
     Date     |   NAME   |                  DESCRIPTION
------------------------------------------------------------------------------
  2022.12.01  DEVELOPER : Initial Created.
**************************************************************************************/

using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Collections;
using System.Data;
using C1.WPF;
using C1.WPF.DataGrid;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.ControlsLibrary;

namespace LGC.GMES.MES.FCS002
{
    public partial class FCS002_202_TRAY_LIST : UserControl, IWorkArea
    {
        #region Declaration & Constructor 
        Hashtable hash_loss_color = new Hashtable();
        private DataTable dtColor = new DataTable();
        private DataTable dtTemp = new DataTable();
        private DataTable _dtCopy = new DataTable();

        private string _sStocker = string.Empty;
        private string LANE_ID = string.Empty;
        private string EQPT_GR_TYPE_CODE = string.Empty;
        #endregion

        #region Initialize
        public FCS002_202_TRAY_LIST()
        {
            InitializeComponent();
        }

        ///// <summary>
        ///// Frame�� ��ȣ�ۿ��ϱ� ���� ��ü
        ///// </summary>
        public IFrameOperation FrameOperation
        {
            get;
            set;
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            object[] tmps = this.FrameOperation.Parameters;

            _sStocker = tmps[0] as string;

            //Combo Setting
            InitCombo();
            this.Loaded -= UserControl_Loaded; //20210406 ȭ���̵� �� �� Load �̺�Ʈ �� Ÿ���� ����
        }

        

        private Brush ColorToBrush(System.Drawing.Color C)
        {
            return new SolidColorBrush(Color.FromArgb(C.A, C.R, C.G, C.B));
        }

        /// <summary>
        /// �޺��ڽ� �ʱ� ����
        /// </summary>
        private void InitCombo()
        {
            CommonCombo_Form_MB _combo = new CommonCombo_Form_MB();

            string[] sFilter1 = { "FORM_STOCKER_TYPE_CODE", string.Empty };
            _combo.SetCombo(cboStockerType, CommonCombo_Form_MB.ComboStatus.NONE, sCase: "SYSTEM_AREA_COMMON_CODE", sFilter: sFilter1);


            C1ComboBox[] cboLineChild = { cboModel, cboRoute };
            _combo.SetCombo(cboLine, CommonCombo_Form_MB.ComboStatus.ALL, sCase: "LINE", cbChild: cboLineChild);

            C1ComboBox[] cboModelChild = { cboRoute };
            C1ComboBox[] cboModelParent = { cboStockerType,cboLine };
            _combo.SetCombo(cboModel, CommonCombo_Form_MB.ComboStatus.ALL, sCase: "LANEMODEL", cbChild: cboModelChild, cbParent: cboModelParent);

            C1ComboBox[] cboRouteParent = { cboStockerType, cboLine, cboModel };
            _combo.SetCombo(cboRoute, CommonCombo_Form_MB.ComboStatus.ALL, sCase: "LANEMODELROUTE", cbParent: cboRouteParent);


            string[] sFilter4 = { "COMBO_FORM_SPCL_FLAG" };
            _combo.SetCombo(cboSpecial, CommonCombo_Form_MB.ComboStatus.ALL, sCase: "COMMCODE", sFilter: sFilter4);

            string[] sFilter2 = { "FORM_SEARCH_ORDERBY", "ORDER_LOT_ID,ORDER_START_TIME,ORDER_EQP_ID" };  //E09
            _combo.SetCombo(cboOrder, CommonCombo_Form_MB.ComboStatus.NONE, sCase: "COMMCODE", sFilter: sFilter2);

            //cboNextEqp

            string[] sFilter3 = { "FORM_STOCKER_TRAY_STATUS" };
            _combo.SetCombo(cboStatus, CommonCombo_Form_MB.ComboStatus.ALL, sCase: "COMMCODE", sFilter: sFilter3);

            //cboRackLocation

            C1ComboBox[] cboEqpParent = { cboStockerType };
            _combo.SetCombo(cboEqp, CommonCombo_Form_MB.ComboStatus.ALL, sCase: "STOEQP", cbParent: cboEqpParent);

            _combo.SetCombo(cboTrayType, CommonCombo_Form_MB.ComboStatus.ALL, sCase: "TRAYTYPE");

            cboStockerType.SelectedValue = _sStocker;

        }
        #endregion

        #region Event
        private void btnSearch2_Click(object sender, RoutedEventArgs e)
        {
            GetList();
        }
        #endregion

        #region Method
        private void GetList()
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("LANE_ID", typeof(string));
                RQSTDT.Columns.Add("MDLLOT_ID", typeof(string));
                RQSTDT.Columns.Add("ROUTID", typeof(string));
                RQSTDT.Columns.Add("STATUS", typeof(string));
                RQSTDT.Columns.Add("PROD_LOTID", typeof(string));
                RQSTDT.Columns.Add("RACK_TIME_OVER", typeof(string));
                RQSTDT.Columns.Add("TRAY_TYPE_CODE", typeof(string));
                RQSTDT.Columns.Add("EQPTID", typeof(string));
                RQSTDT.Columns.Add("SORT", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["LANE_ID"] = Util.GetCondition(cboStockerType, bAllNull: true);
                dr["MDLLOT_ID"] = Util.GetCondition(cboModel, bAllNull: true);
                dr["ROUTID"] = Util.GetCondition(cboRoute, bAllNull: true);
                dr["STATUS"] = Util.GetCondition(cboStatus, bAllNull: true);

                if (!string.IsNullOrEmpty(txtProdLotID.Text))
                    dr["PROD_LOTID"] = txtProdLotID.Text;

                if (chkbOverTime.IsChecked == true)
                    dr["RACK_TIME_OVER"] = "Y";

                dr["TRAY_TYPE_CODE"] = Util.GetCondition(cboTrayType, bAllNull: true);
                dr["EQPTID"] = Util.GetCondition(cboEqp, bAllNull: true);
                dr["SORT"] = Util.GetCondition(cboOrder, bAllNull: true);

                RQSTDT.Rows.Add(dr);

                ShowLoadingIndicator();
                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_SEL_SEL_TRAY_LIST_ST341_MB", "RQSTDT", "RSLTDT", RQSTDT);

                Util.GridSetData(dgList, dtResult, this.FrameOperation);

                //���� 0�� Column Visible ����
                for (int i = 0; i < dtResult.Columns.Count; i++)
                {
                    if (dtResult.Columns[i].ColumnName.Contains("GRD_QTY"))
                    {
                        int sum = 0;
                        for (int j = 0; j < dtResult.Rows.Count; j++)
                        {
                            sum += Util.NVC_Int(dtResult.Rows[j][i]);
                        }

                        if (sum == 0)
                        {
                            dgList.Columns[dtResult.Columns[i].ColumnName].Visibility = Visibility.Collapsed;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
            finally
            {
                HiddenLoadingIndicator();
            }
        }

        /// <summary>
        /// ����� �������� ��������
        /// </summary>

        private void ShowLoadingIndicator()
        {
            if (loadingIndicator != null)
                loadingIndicator.Visibility = Visibility.Visible;
        }

        private void HiddenLoadingIndicator()
        {
            if (loadingIndicator != null)
                loadingIndicator.Visibility = Visibility.Collapsed;
        }
        #endregion

        
    }
}
