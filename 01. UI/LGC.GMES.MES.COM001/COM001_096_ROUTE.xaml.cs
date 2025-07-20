/*************************************************************************************
 Created Date : 2016.06.16
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
using System.Windows.Controls;

namespace LGC.GMES.MES.COM001
{
    public partial class COM001_096_ROUTE : C1Window, IWorkArea
    {
        #region Declaration & Constructor 

        string _PRODID = "";
        string _ROUTID = "";
        string _FLOWID = "";
        string _PROCID = "";
        string _WIP_TYPE = "";

        public COM001_096_ROUTE()
        {
            InitializeComponent();
            InitCombo();
        }

        public IFrameOperation FrameOperation
        {
            get;
            set;
        }


        public string ROUTID
        {
            get { return _ROUTID; }
        }

        public string FLOWID
        {
            get { return _FLOWID; }
        }

        public string PROCID
        {
            get { return _PROCID; }
        }

        public string WIP_TYPE
        {
            get { return _WIP_TYPE; }
        }

        #endregion

        #region Initialize


        #endregion

        #region Event


        private void C1Window_Loaded(object sender, RoutedEventArgs e)
        {
            object[] tmps = C1WindowExtension.GetParameters(this);

            if (tmps != null && tmps.Length >= 1)
            {
                _PRODID = Util.NVC(tmps[0]);
                //_LOSS_SEQ = Util.NVC(tmps[1]);
                //_START_DTTM = Util.NVC(tmps[2]);
                //_END_DTTM = Util.NVC(tmps[3]);

            }
            GetRoute();
        }

        //화면내 combo 셋팅
        private void InitCombo()
        {

            CommonCombo _combo = new CommonCombo();
            string[] sFilter = { "WIP_TYPE" };
            _combo.SetCombo(cboWipType, CommonCombo.ComboStatus.SELECT, sCase: "COMMCODE", sFilter: sFilter);

            cboWipType.SelectedValueChanged += cboWipType_SelectedItemChanged;

        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                this.DialogResult = MessageBoxResult.OK;
            }
            catch (Exception ex)
            {
                Util.Alert(ex.ToString());
            }
        }


        #endregion

        #region Mehod
        #region [조회]
        public void GetRoute()
        {
            try
            {
                Util.gridClear(dgRoute);
                Util.gridClear(dgFlow);
                Util.gridClear(dgProc);

                DataTable dtRqst = new DataTable();
                dtRqst.Columns.Add("LANGID", typeof(string));
                dtRqst.Columns.Add("SHOPID", typeof(string));
                dtRqst.Columns.Add("PRODID", typeof(string));

                DataRow dr = dtRqst.NewRow();

                dr["LANGID"] = LoginInfo.LANGID;
                dr["SHOPID"] = LoginInfo.CFG_SHOP_ID;
                dr["PRODID"] = _PRODID;


                dtRqst.Rows.Add(dr);
                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_ROUTE_FOR_CHANGE", "INDATA", "OUTDATA", dtRqst);

                dgRoute.ItemsSource = DataTableConverter.Convert(dtRslt);
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }


        public void GetFlow()
        {
            try
            {
                DataTable dtRqst = new DataTable();
                dtRqst.Columns.Add("LANGID", typeof(string));
                dtRqst.Columns.Add("ROUTID", typeof(string));

                DataRow dr = dtRqst.NewRow();

                dr["LANGID"] = LoginInfo.LANGID;
                dr["ROUTID"] = _ROUTID;


                dtRqst.Rows.Add(dr);
                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_FLOW_FOR_CHANGE", "INDATA", "OUTDATA", dtRqst);

                dgFlow.ItemsSource = DataTableConverter.Convert(dtRslt);
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }

        public void GetProc()
        {
            try
            {

                DataTable dtRqst = new DataTable();
                dtRqst.Columns.Add("LANGID", typeof(string));
                dtRqst.Columns.Add("ROUTID", typeof(string));
                dtRqst.Columns.Add("FLOWID", typeof(string));

                DataRow dr = dtRqst.NewRow();

                dr["LANGID"] = LoginInfo.LANGID;
                dr["ROUTID"] = _ROUTID;
                dr["FLOWID"] = _FLOWID;


                dtRqst.Rows.Add(dr);
                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_PROC_FOR_CHANGE", "INDATA", "OUTDATA", dtRqst);

                dgProc.ItemsSource = DataTableConverter.Convert(dtRslt);
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }


        #endregion

        #endregion

        private void cboWipType_SelectedItemChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            _WIP_TYPE = cboWipType.SelectedValue.ToString();
        }

        private void dgRouteChoice_Checked(object sender, RoutedEventArgs e)
        {
            RadioButton rb = sender as RadioButton;

            _ROUTID = DataTableConverter.GetValue(rb.DataContext, "ROUTID").ToString();

            GetFlow();
        }

        private void dgFlowChoice_Checked(object sender, RoutedEventArgs e)
        {
            RadioButton rb = sender as RadioButton;

            _FLOWID = DataTableConverter.GetValue(rb.DataContext, "FLOWID").ToString();

            GetProc();
        }

        private void dgProcChoice_Checked(object sender, RoutedEventArgs e)
        {
            RadioButton rb = sender as RadioButton;

            _PROCID = DataTableConverter.GetValue(rb.DataContext, "PROCID").ToString();

            
        }
    }
}
