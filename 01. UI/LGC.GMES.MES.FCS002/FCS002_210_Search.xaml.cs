/*************************************************************************************
 Created Date : 2022.12.06
      Creator : 
   Decription : 
--------------------------------------------------------------------------------------
 [Change History]
  2022.12.06  DEVELOPER : Initial Created..
 
**************************************************************************************/

using C1.WPF;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using System;
using System.Data;
using System.Windows;

namespace LGC.GMES.MES.FCS002
{
    public partial class FCS002_210_Search : C1Window, IWorkArea
    {
        #region Declaration & Constructor 
        private DataTable dtTrayList = null;
        string _sEQSGID = string.Empty;
        string _index = string.Empty;
        string _gubun = string.Empty;

        private string _MDLLot = string.Empty;
        private string _Lot = string.Empty;
        private string _AssyLot = string.Empty;
        private string _Route = string.Empty;
        private string _RouteName = string.Empty;
        private string _PreProcGrCode = string.Empty;
        private string _PreProcGrName = string.Empty;
        private string _NextProcGrCode = string.Empty;
        private string _NextProcGrName = string.Empty;

        public DataTable TrayList
        {
            set { this.dtTrayList = value; }
        }
        #endregion

        #region Initialize
        public FCS002_210_Search()
        {
            InitializeComponent();
        }

        public IFrameOperation FrameOperation
        {
            get;
            set;
        }

        public string MDLLOT
        {
            get { return _MDLLot; }
        }

        public string LOT
        {
            get { return _Lot; }
        }

        public string ASSYLOT
        {
            get { return _AssyLot; }
        }

        public string ROUTID
        {
            get { return _Route; }
        }

        public string ROUTNAME
        {
            get { return _RouteName; }
        }

        public string PREPROCGRCODE
        {
            get { return _PreProcGrCode; }
        }

        public string PREPROCGRNAME
        {
            get { return _PreProcGrName; }
        }

        public string NEXTPROCGRCODE
        {
            get { return _NextProcGrCode; }
        }

        public string NEXTPROCGRNAME
        {
            get { return _NextProcGrName; }
        }

        public string INDEX
        {
            get { return _index; }
        }

        private void C1Window_Loaded(object sender, RoutedEventArgs e)
        {
            object[] tmps = C1WindowExtension.GetParameters(this);
            if (tmps != null && tmps.Length >= 1)
            {
                _sEQSGID = Util.NVC(tmps[0]);
                _index = Util.NVC(tmps[1]);
                _gubun = Util.NVC(tmps[2]);
                _PreProcGrCode = Util.NVC(tmps[3]);
                _NextProcGrCode = Util.NVC(tmps[4]);
            }

            if (_sEQSGID.Equals(string.Empty))
            {
                Util.MessageValidation("SFU5040");  //라인정보가 존재하지 않습니다.
                return;
            }

            //Combo Setting
            InitCombo();


            cboPreProcGrCode.IsEnabled = false;
            cboNextProcGrCode.IsEnabled = false;
           
        }
        #endregion

        #region Event

        private void InitCombo()
        {
            CommonCombo_Form_MB _combo = new CommonCombo_Form_MB();

            C1ComboBox[] cboModelChild = { cboRoute };
            string[] sFilter = { _sEQSGID };
            _combo.SetCombo(cboModel, CommonCombo_Form_MB.ComboStatus.ALL, sFilter: sFilter, sCase: "LINEMODEL", cbChild: cboModelChild);

            C1ComboBox[] cboRouteParent = { cboModel };
            _combo.SetCombo(cboRoute, CommonCombo_Form_MB.ComboStatus.SELECT, sFilter: sFilter, sCase: "ROUTE_LOT", cbParent: cboRouteParent);

            C1ComboBox[] cboLotChild = { cboAssyLot };
            _combo.SetCombo(cboLot, CommonCombo_Form_MB.ComboStatus.SELECT, sFilter: sFilter, sCase: "DAY_GR_LOTID", cbChild: cboLotChild);

            C1ComboBox[] cboAssyLotParent = { cboLot };
            _combo.SetCombo(cboAssyLot, CommonCombo_Form_MB.ComboStatus.ALL, sFilter: sFilter, sCase: "PRODLOT", cbParent: cboAssyLotParent);
            
            string[] sFilter1 = { "PROC_GR_CODE_MB" };
            _combo.SetCombo(cboPreProcGrCode, CommonCombo_Form_MB.ComboStatus.SELECT, sFilter: sFilter1, sCase: "AREA_COMMON_CODE");
            cboPreProcGrCode.SelectedValue = _PreProcGrCode;

            _combo.SetCombo(cboNextProcGrCode, CommonCombo_Form_MB.ComboStatus.SELECT, sFilter: sFilter1, sCase: "AREA_COMMON_CODE");
            cboNextProcGrCode.SelectedValue = _NextProcGrCode;

        }

      
        #endregion

        #region Event
        private void btnSelect_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(Util.GetCondition(cboLot, bAllNull: false)))
            {
                //대 LOT을 선택하십시오.
                Util.MessageInfo("SFU1490");
                return;
            }
            if (string.IsNullOrEmpty(Util.GetCondition(cboRoute, bAllNull: false)))
            {
                //공정경로을 선택하세요.
                Util.MessageInfo("FM_ME_0106");
                return;
            }

            string RVAL = ChkDocvLot();
            if (RVAL == "0")
            {
                Util.MessageInfo("FM_ME_0540");
                return;
            }
            if (_gubun.Equals("AGING"))

            {
                if (string.IsNullOrEmpty(Util.GetCondition(cboPreProcGrCode, bAllNull: false)))
                {
                    //이전 공정 그룹을 선택하세요.
                    Util.MessageInfo("FM_ME_0489");
                    return;
                }

                if (string.IsNullOrEmpty(Util.GetCondition(cboNextProcGrCode, bAllNull: false)))
                {
                    //다음 공정 그룹을 선택하세요.
                    Util.MessageInfo("FM_ME_0490");
                    return;
                }
            }

            _MDLLot = Util.GetCondition(cboModel, bAllNull: true);
            _Lot = Util.GetCondition(cboLot, bAllNull: true);
            _AssyLot = Util.GetCondition(cboAssyLot, bAllNull: true);
            if (string.IsNullOrEmpty(_AssyLot))
                _AssyLot = "ALL";
            _Route = Util.GetCondition(cboRoute, bAllNull: true);
            _RouteName = cboRoute.Text;
            _PreProcGrCode = Util.GetCondition(cboPreProcGrCode, bAllNull: true);
            _PreProcGrName = cboPreProcGrCode.Text;
            _NextProcGrCode = Util.GetCondition(cboNextProcGrCode, bAllNull: true);
            _NextProcGrName = cboNextProcGrCode.Text;
            
            this.DialogResult = MessageBoxResult.OK;
            Close();
        }
        #endregion

        private void cboModel_SelectedItemChanged(object sender, PropertyChangedEventArgs<object> e)
        {
          
            
        }

        private string ChkDocvLot()
        {
            DataTable RQSTDT = new DataTable();
            RQSTDT.TableName = "RQSTDT";
            RQSTDT.Columns.Add("ROUTID", typeof(string));
            RQSTDT.Columns.Add("PROD_LOTID", typeof(string));

            DataRow dr = RQSTDT.NewRow();
            dr["ROUTID"] = Util.GetCondition(cboRoute, bAllNull: true);
            dr["PROD_LOTID"] = Util.GetCondition(cboLot, bAllNull: true);
            RQSTDT.Rows.Add(dr);

            DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_SEL_LOT_DOCV_CHK_MB", "RQSTDT", "RSLTDT", RQSTDT);

            return dtResult.Rows[0]["RVAL"].ToString();
          
        }

        private void cboModel_SelectionCommitted(object sender, PropertyChangedEventArgs<object> e)
        {
            try
            {
                cboLot.ClearItems();

                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("EQSGID", typeof(string));
                RQSTDT.Columns.Add("MDLLOT_ID", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["EQSGID"] = !string.IsNullOrEmpty(_sEQSGID) ? _sEQSGID : null ;
                dr["MDLLOT_ID"] = cboModel.SelectedValue.ToString();
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_DAYGRLOT_FORM_CBO_MB", "RQSTDT", "RSLTDT", RQSTDT);

                cboLot.DisplayMemberPath = "CBO_NAME";
                cboLot.SelectedValuePath = "CBO_CODE";

                DataRow d = dtResult.NewRow();
                d["CBO_NAME"] = "-SELECT-";
                d["CBO_CODE"] = "SELECT";
                dtResult.Rows.InsertAt(d, 0);

                cboLot.ItemsSource = dtResult.Copy().AsDataView();
                cboLot.SelectedIndex = 0;
                
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }
    }
}
