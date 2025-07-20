/*************************************************************************************
 Created Date : 2020.11.09
      Creator : KANG DONG HEE
   Decription : Aging Rack 예약현황
--------------------------------------------------------------------------------------
 [Change History]
------------------------------------------------------------------------------
     Date     |   NAME   |                  DESCRIPTION
------------------------------------------------------------------------------
  2020.09.10  DEVELOPER : Initial Created.
  2020.11.09  KANG DONG HEE : 
  2021.04.13  KDH : 화면간 이동 시 초기화 현상 제거
  2023.11.01  이의철 : Aging Type combo 추가
**************************************************************************************/
#define SAMPLE_DEV
//#undef SAMPLE_DEV

using C1.WPF;
using C1.WPF.DataGrid;
using C1.WPF.DataGrid.Summaries;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using System;
using System.Collections.Generic;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using LGC.GMES.MES.CMM001;

namespace LGC.GMES.MES.FCS001
{
    public partial class FCS001_013 : UserControl, IWorkArea
    {
        #region Declaration & Constructor 
        /// <summary>
        /// Frame과 상호작용하기 위한 객체
        /// </summary>
        public IFrameOperation FrameOperation
        {
            get;
            set;
        }
        #endregion

        #region Initialize
        public FCS001_013()
        {
            InitializeComponent();
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                //Combo Setting
                InitCombo();
                this.Loaded -= UserControl_Loaded; //2021.04.13 화면간 이동 시 초기화 현상 제거
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        //화면내 combo 셋팅
        private void InitCombo()
        {
            CommonCombo_Form ComCombo = new CommonCombo_Form();

            ComCombo.SetCombo(cboLine, CommonCombo_Form.ComboStatus.ALL, sCase: "LINE");

            string[] sFilterEqpType = { "EQPT_GR_TYPE_CODE", "3,7" };
            C1ComboBox[] cboEqpKindChild = { cboRow };
            ComCombo.SetCombo(cboEqpKind, CommonCombo_Form.ComboStatus.ALL, sCase: "FORM_CMN", sFilter: sFilterEqpType, cbChild: cboEqpKindChild);

            object[] oFilterRow = { cboEqpKind };
            ComCombo.SetComboObjParent(cboRow, CommonCombo_Form.ComboStatus.ALL, sCase: "AGING_ROW", objParent: oFilterRow);

            //Aging Type combo 추가
            //string[] sFilter = { "FORM_AGING_TYPE_CODE", "N" };
            //ComCombo.SetCombo(cboAgingType, CommonCombo_Form.ComboStatus.ALL, sCase: "SYSTEM_AREA_COMMON_CODE", sFilter: sFilter);

            string[] sFilter = { "FORM_AGING_TYPE_CODE", "3,7" };
            AreaComCodeByCodeType(cboAgingType, sFilter: sFilter);

        }

        #endregion

        #region Event


        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            GetList();
        }
        #endregion

        #region Method

        private void GetList()
        {
            try
            {
                DataTable dtRqst = new DataTable();
                dtRqst.TableName = "RQSTDT";
                dtRqst.Columns.Add("LANGID", typeof(string));
                dtRqst.Columns.Add("EQSGID", typeof(string));
                dtRqst.Columns.Add("S70", typeof(string));
                dtRqst.Columns.Add("X_PSTN", typeof(string));
                dtRqst.Columns.Add("AREAID", typeof(string));
                dtRqst.Columns.Add("CMCODE", typeof(string)); //Aging Type combo 추가
                
                DataRow dr = dtRqst.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["EQSGID"] = Util.GetCondition(cboLine, bAllNull: true);
                dr["S70"] = Util.GetCondition(cboEqpKind, bAllNull: true);
                dr["X_PSTN"] = Util.GetCondition(cboRow, bAllNull: true);
                dr["AREAID"] = LoginInfo.CFG_AREA_ID;
                dr["CMCODE"] = Util.GetCondition(cboAgingType, bAllNull: true); //Aging Type combo 추가
                dtRqst.Rows.Add(dr);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_SEL_AGING_RESERVATION", "RQSTDT", "RSLTDT", dtRqst);
                Util.GridSetData(dgAgingRes, dtRslt, FrameOperation, true);

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }


        private void AreaComCodeByCodeType(C1ComboBox cbo, String[] sFilter)
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("COM_TYPE_CODE", typeof(string));
                RQSTDT.Columns.Add("CMCODE_LIST", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["COM_TYPE_CODE"] = sFilter[0];
                if (sFilter.Length > 1)
                    dr["CMCODE_LIST"] = sFilter[1];
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_AREA_COM_CODE_BY_CODE_TYPE", "RQSTDT", "RSLTDT", RQSTDT);

                DataRow drNew = dtResult.NewRow();
                drNew["CBO_NAME"] = " - ALL-";
                drNew["CBO_CODE"] = "";
                dtResult.Rows.InsertAt(drNew, 0);

                cbo.DisplayMemberPath = "CBO_NAME";
                cbo.SelectedValuePath = "CBO_CODE";
                //cbo.ItemsSource = AddStatus(dtResult, cs, "CBO_CODE", "CBO_NAME").Copy().AsDataView();
                cbo.ItemsSource = dtResult.Copy().AsDataView();

                cbo.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        #endregion

    }
}
