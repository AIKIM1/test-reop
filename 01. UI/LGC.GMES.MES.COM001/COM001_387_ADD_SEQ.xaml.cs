/*************************************************************************************
 Created Date : 2023.07.03
      Creator : 
   Decription : 
--------------------------------------------------------------------------------------
 [Change History]
  2023.07.03  DEVELOPER : Initial Created.
  2023.07.21  주재홍 : 현장 테스트후 3차 개선
  2023.07.31  주재홍 : BizAct 다국어

 ***************************************************************************************/

using C1.WPF;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using System;
using System.Data;
using System.Globalization;
using System.Windows;

namespace LGC.GMES.MES.COM001
{
    public partial class COM001_387_ADD_SEQ : C1Window, IWorkArea
    {
        #region Declaration & Constructor 

        string _AREAID = "";
        string _STCK_CNT_YM = "";
        string _SECTION = "";

        //const string _STCK_CNT_TYPE_CURR = "CURR";
        //const string _STCK_CNT_TYPE_DTTM = "DTTM";

        DataTable _dtBeforeSet = new DataTable();
        public COM001_387_ADD_SEQ()
        {
            InitializeComponent();
            InitCombo();
        }

        public IFrameOperation FrameOperation
        {
            get;
            set;
        }

        #endregion

        #region Initialize
        private void InitCombo()
        {
            CommonCombo _combo = new CommonCombo();
            //동
            //_combo.SetCombo(cboArea, CommonCombo.ComboStatus.SELECT, sCase: "AREA");

            C1ComboBox[] cboAreaChild = { cboSection };
            _combo.SetCombo(cboArea, CommonCombo.ComboStatus.SELECT, sCase: "AREA", cbChild: cboAreaChild);

            // Section
            object[] objSectionParent = { cboArea };
            String[] sFilterAll2 = { "" };
            _combo.SetComboObjParent(cboSection, CommonCombo.ComboStatus.NONE, sCase: "AREA_SECTION", objParent: objSectionParent, sFilter: sFilterAll2);

            //SetcboSection(cboSection);
        }

        #endregion



        private void C1Window_Loaded(object sender, RoutedEventArgs e)
        {
            object[] tmps = C1WindowExtension.GetParameters(this);

            if (tmps != null && tmps.Length >= 1)
            {
                _AREAID = Util.NVC(tmps[0]);
                _STCK_CNT_YM = Util.NVC(tmps[1]);
                _SECTION = Util.NVC(tmps[2]);

                cboArea.SelectedValue = _AREAID;
                cboSection.SelectedValue = _SECTION;
                ldpMonthShot.SelectedDateTime = Convert.ToDateTime(_STCK_CNT_YM);
            }
        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            
            string sArea = Util.GetCondition(cboArea);
            if (string.IsNullOrWhiteSpace(sArea))
            {
                Util.MessageValidation("SFU1499");  // 동을 선택하세요.
                return;
            }

            
            string sSection = Util.GetCondition(cboSection);
            if (string.IsNullOrWhiteSpace(sSection))
            {
                Util.MessageValidation("SFU8912");  // 창고를 선택하세요.
                return;
            }

            //차수를 추가하시겠습니까?
            LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("SFU2959"), null, "Information", MessageBoxButton.OKCancel, MessageBoxIcon.None, (result) =>
            {
                if (result.ToString().Equals("OK"))
                {
                    DegreeAdd();
                }
            }
            );
        }

        // 차수추가
        private void DegreeAdd()
        {
            try
            {
                DataTable INDATA = new DataTable();
                INDATA.TableName = "INDATA";
                INDATA.Columns.Add("STCK_CNT_YM", typeof(string));
                INDATA.Columns.Add("AREAID", typeof(string));
                INDATA.Columns.Add("WH_ID", typeof(string));
                INDATA.Columns.Add("STCK_CNT_NOTE", typeof(string));
                INDATA.Columns.Add("USERID", typeof(string));
                INDATA.Columns.Add("LANGID", typeof(string));

                DataRow dr = INDATA.NewRow();
                dr["STCK_CNT_YM"] = Util.GetCondition(ldpMonthShot);
                dr["AREAID"] = Util.GetCondition(cboArea); 
                dr["WH_ID"] = Util.GetCondition(cboSection); 
                dr["STCK_CNT_NOTE"] = Util.GetCondition(txtNote);
                dr["USERID"] = LoginInfo.USERID;

                if (dr["AREAID"].Equals("")) return;
                if (dr["WH_ID"].Equals("")) return;

                INDATA.Rows.Add(dr);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("BR_PRD_REG_WH_STCK_CNT_ORD", "INDATA", "OUTDATA", INDATA);
                this.DialogResult = MessageBoxResult.OK;
            }
            catch (Exception ex)
            {
                if (ex != null)
                {
                    Util.MessageException(ex);
                    return;
                }
            }
        }

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = MessageBoxResult.Cancel;
        }
    }
}
