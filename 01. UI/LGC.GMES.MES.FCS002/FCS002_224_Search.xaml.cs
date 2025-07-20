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
    public partial class FCS002_224_Search : C1Window, IWorkArea
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
        public FCS002_224_Search()
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


          
           
        }
        #endregion

        #region Event

        private void InitCombo()
        {
            CommonCombo_Form_MB _combo = new CommonCombo_Form_MB();

            C1ComboBox[] cboModelChild = { cboRoute };
            string[] sFilter = { _sEQSGID };
            _combo.SetCombo(cboModel, CommonCombo_Form_MB.ComboStatus.SELECT, sFilter: sFilter, sCase: "LINEMODEL", cbChild: cboModelChild);

            C1ComboBox[] cboRouteParent = { cboModel };
            _combo.SetCombo(cboRoute, CommonCombo_Form_MB.ComboStatus.SELECT, sFilter: sFilter, sCase: "ROUTE_LOT", cbParent: cboRouteParent);

            C1ComboBox[] cboLotChild = { cboAssyLot };
            _combo.SetCombo(cboLot, CommonCombo_Form_MB.ComboStatus.SELECT, sFilter: sFilter, sCase: "DAY_GR_LOTID", cbChild: cboLotChild);

            C1ComboBox[] cboAssyLotParent = { cboLot };
            _combo.SetCombo(cboAssyLot, CommonCombo_Form_MB.ComboStatus.SELECT, sFilter: sFilter, sCase: "PRODLOT", cbParent: cboAssyLotParent);

          
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

            if (_gubun.Equals("AGING"))

         
            _MDLLot = Util.GetCondition(cboModel, bAllNull: true);
            _Lot = Util.GetCondition(cboLot, bAllNull: true);
            _AssyLot = Util.GetCondition(cboAssyLot, bAllNull: true);
            _Route = Util.GetCondition(cboRoute, bAllNull: true);
            _RouteName = cboRoute.Text;
           
            
            this.DialogResult = MessageBoxResult.OK;
            Close();
        }
        #endregion
    }
}
