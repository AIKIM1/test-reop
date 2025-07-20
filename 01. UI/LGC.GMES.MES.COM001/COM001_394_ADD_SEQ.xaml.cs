/*************************************************************************************
 Created Date : 2023.12.08
      Creator : 백광영
   Decription : 공Pallet 재고조사
--------------------------------------------------------------------------------------
 [Change History]
  2023.12.08  백광영 : COM001_387_ADD_SEQ Copy

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
    public partial class COM001_394_ADD_SEQ : C1Window, IWorkArea
    {
        #region Declaration & Constructor 

        string _STCK_CNT_YM = "";
        string _SECTION = "";
        DataTable dtWH = new DataTable();

        DataTable _dtBeforeSet = new DataTable();
        public COM001_394_ADD_SEQ()
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
            //C1ComboBox[] cboAreaChild = { cboSection };
            //_combo.SetCombo(cboArea, CommonCombo.ComboStatus.SELECT, sCase: "AREA", cbChild: cboAreaChild);

            setWareHouse(cboSection);
        }

        #endregion
        private void C1Window_Loaded(object sender, RoutedEventArgs e)
        {
            object[] tmps = C1WindowExtension.GetParameters(this);

            if (tmps != null && tmps.Length >= 1)
            {
                _STCK_CNT_YM = Util.NVC(tmps[0]);
                _SECTION = Util.NVC(tmps[1]);

                cboSection.SelectedValue = _SECTION;
                ldpMonthShot.SelectedDateTime = Convert.ToDateTime(_STCK_CNT_YM);
            }
        }

        private void setWareHouse(C1ComboBox _cb)
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("AREAID", typeof(string));
                RQSTDT.Columns.Add("WH_TYPE_CODE", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["AREAID"] = LoginInfo.CFG_AREA_ID;
                dr["WH_TYPE_CODE"] = "EEP";

                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_TB_MMD_PRDT_WH_INFO", "RQSTDT", "RSLTDT", RQSTDT);
                _cb.DisplayMemberPath = "WH_NAME";
                _cb.SelectedValuePath = "WH_ID";

                dtWH.Clear();
                dtWH = dtResult;

                _cb.ItemsSource = DataTableConverter.Convert(dtWH.DefaultView.ToTable(true));
                if (dtResult.Rows.Count > 0)
                {
                    _cb.SelectedIndex = 0;

                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
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
                dr["AREAID"] = LoginInfo.CFG_AREA_ID;
                dr["WH_ID"] = Util.GetCondition(cboSection); 
                dr["STCK_CNT_NOTE"] = Util.GetCondition(txtNote);
                dr["USERID"] = LoginInfo.USERID;

                if (dr["AREAID"].Equals("")) return;
                if (dr["WH_ID"].Equals("")) return;

                INDATA.Rows.Add(dr);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("BR_PRD_REG_WH_STCK_CNT_ORD_EMPTY_PALLET", "INDATA", "OUTDATA", INDATA);
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
