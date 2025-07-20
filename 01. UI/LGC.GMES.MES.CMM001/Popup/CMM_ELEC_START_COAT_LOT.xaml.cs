using C1.WPF;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using System;
using System.Data;
using System.Windows;

namespace LGC.GMES.MES.CMM001
{
    /// <summary>
    /// CMM_ELEC_START_COAT_LOT.xaml에 대한 상호 작용 논리
    /// Development : cnsjwjeong [2018-11-14, C20181016_17746] => 코터 Cut 임의 생성 팝업
    /// </summary>
    public partial class CMM_ELEC_START_COAT_LOT : C1Window, IWorkArea
    {
        #region Initialize
        private string sEqptID = string.Empty;
        private string sEqptName = string.Empty;
        private string sLargeLotID = string.Empty;
        private string sOutLotID = string.Empty;
        private string sCoatSideType = string.Empty;
        private string sSlittingFlag = string.Empty;
        private string sAuth = "MESADMIN,MESDEV";
        private bool isSingleCoater = false;

        public string returnOutLotID
        {
            get { return sOutLotID; }
        }

        public string returnLargeLotID
        {
            get { return sLargeLotID; }
        }

        public IFrameOperation FrameOperation
        {
            get;
            set;
        }

        public CMM_ELEC_START_COAT_LOT()
        {
            InitializeComponent();
        }
        #endregion
        #region Event Method
        private void C1Window_Loaded(object sender, RoutedEventArgs e)
        {
            object[] tmps = C1WindowExtension.GetParameters(this);

            if (tmps == null)
            {
                this.DialogResult = MessageBoxResult.Cancel;
                return;
            }

            sEqptID = Util.NVC(tmps[0]);
            sEqptName = Util.NVC(tmps[1]);
            sLargeLotID = Util.NVC(tmps[2]);
            isSingleCoater = Convert.ToBoolean(tmps[3]);
            sCoatSideType = Util.NVC(tmps[4]);

            txtEqptName.Text = sEqptName;
            txtLargeLot.Text = sLargeLotID;

            // CNA 코터 + H/S 용 기능 대응 추가
            SetHalfSlitterProcess();
            if (string.Equals(sSlittingFlag, "Y"))
            {
                lblSideType.Visibility = Visibility.Visible;
                cboSideType.Visibility = Visibility.Visible;
            }

            SetApplyPermissions();
        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(sEqptID))
            {
                Util.MessageValidation("SFU1672");  //설비 정보가 없습니다.
                return;
            }

            if (string.IsNullOrEmpty(sLargeLotID))
            {
                Util.MessageValidation("SFU1490"); // 대LOT을 선택하십시오.
                return;
            }

            if (string.IsNullOrEmpty(Util.NVC(cboCutNo.Text)))
            {
                Util.MessageValidation("SFU5065"); // Cut No를 선택해주세요
                return;
            }

            if (string.Equals(sSlittingFlag, "Y") && cboSideType.Visibility == Visibility.Visible && string.IsNullOrEmpty(Util.NVC(cboSideType.Text)))
            {
                Util.MessageValidation("SFU3622");  //H/S설비에서는 H/S Side를 선택 후 진행바랍니다. 
                return;
            }

            Util.MessageConfirm("SFU1371", (sResult) =>     // LOT을 생성 하시겠습니까?
            {
                if (sResult == MessageBoxResult.OK)
                {
                    //string sCutNo = sLargeLotID + Util.NVC(cboCutNo.Text);
                    string sCutNo = Util.NVC(cboCutNo.Text); // MES 2.0 Cut No 만 넘기는걸로 수정 (이상권 책임 요청)

                    // Message Set
                    DataTable inTable = new DataTable();
                    inTable.Columns.Add("SRCTYPE", typeof(string));
                    inTable.Columns.Add("IFMODE", typeof(string));
                    inTable.Columns.Add("EQPTID", typeof(string));
                    inTable.Columns.Add("PROD_LOTID", typeof(string));
                    inTable.Columns.Add("CUT_NO", typeof(string));
                    inTable.Columns.Add("COAT_SIDE_TYPE", typeof(string));
                    inTable.Columns.Add("HALF_SLIT_SIDE", typeof(string));
                    inTable.Columns.Add("USERID", typeof(string));

                    DataRow indata = inTable.NewRow();
                    indata["SRCTYPE"] = SRCTYPE.SRCTYPE_EQ;
                    indata["IFMODE"] = IFMODE.IFMODE_OFF;
                    indata["EQPTID"] = sEqptID;
                    indata["PROD_LOTID"] = sLargeLotID;
                    indata["CUT_NO"] = sCutNo;
                    indata["COAT_SIDE_TYPE"] = isSingleCoater == true ? string.Equals(sCoatSideType, "T") ? "T" : "B" : null;
                    indata["HALF_SLIT_SIDE"] = string.Equals(sSlittingFlag, "Y") && cboSideType.Visibility == Visibility.Visible ? Util.NVC(cboSideType.Text) : null;
                    indata["USERID"] = LoginInfo.USERID;

                    inTable.Rows.Add(indata);

                    new ClientProxy().ExecuteService("BR_PRD_REG_START_OUT_LOT_CT_EIF_MANUAL", "IN_EQP", "OUT_LOT", inTable, (result, searchException) =>
                    {
                        try
                        {
                            if (searchException != null)
                            {
                                Util.MessageException(searchException);
                                return;
                            }
                            DataTable msg = result.DataSet.Tables["OUT_LOT"];
                            sOutLotID = Util.NVC(msg.Rows[0]["OUT_LOTID"]);

                            Util.MessageInfo("SFU3091", sOutLotID);    //[%1] LOT이 생성 되었습니다.
                            this.DialogResult = MessageBoxResult.OK;
                        }
                        catch (Exception ex) { Util.MessageException(ex); }
                    });
                }
            });
        }

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = MessageBoxResult.Cancel;
        }
        #endregion
        #region User Method
        private void SetHalfSlitterProcess()
        {
            try
            {
                DataTable IndataTable = new DataTable();
                IndataTable.Columns.Add("EQPTID", typeof(string));

                DataRow Indata = IndataTable.NewRow();
                Indata["EQPTID"] = sEqptID;
                IndataTable.Rows.Add(Indata);

                DataTable dtMain = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_EQPT_SLIT_INFO", "INDATA", "RSLTDT", IndataTable);

                if (dtMain.Rows.Count > 0)
                    sSlittingFlag = Util.NVC(dtMain.Rows[0]["HALF_SLIT_FLAG"]);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void SetApplyPermissions()
        {
            try
            {
                DataTable IndataTable = new DataTable("RQSTDT");
                IndataTable.Columns.Add("USERID", typeof(string));
                IndataTable.Columns.Add("AUTHID", typeof(string));

                DataRow Indata = IndataTable.NewRow();
                Indata["USERID"] = LoginInfo.USERID;
                Indata["AUTHID"] = sAuth;
                IndataTable.Rows.Add(Indata);

                DataTable dtMain = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_PERSON_BY_AUTH_MULTI", "RQSTDT", "RSLTDT", IndataTable);

                if (dtMain != null && dtMain.Rows.Count > 0)
                    btnSave.Visibility = Visibility.Visible;
            }
            catch (Exception ex) { btnSave.Visibility = Visibility.Collapsed; }
        }
        #endregion
    }
}
