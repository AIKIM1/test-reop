/********************************************************************************************************
 Created Date : 2020.11.03
      Creator : 김태균
   Decription : Aging 정보이상 Rack 현황
----------------------------------------------------------------------------------------------------------
 [Change History]
 2020.11.10  DEVELOPER : Initial Created.
 2022.07.21  이정미    : 조회 시, 분기로직 추가로 호출 BIZ 변경 
 2022.11.22  이정미    : cboEqpKind 콤보박스 수정
 2023.08.12  이의철    : 정보이상, 입고금지 체크 박스 검색 추가
 2023.12.25  이의철    : 리스트에 UPDUSER 컬럼 추가
*********************************************************************************************************/
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
    public partial class FCS001_015 : UserControl, IWorkArea
    {
        #region Declaration & Constructor 

        Util _Util = new Util();

        private bool RCV_PRHB_RSN_YN = false; //정보이상, 입고금지 체크 박스 검색 추가

        public FCS001_015()
        {
            InitializeComponent();
        }
        #endregion

        #region Initialize
        /// <summary>
        /// Frame과 상호작용하기 위한 객체
        /// </summary>
        public IFrameOperation FrameOperation
        {
            get;
            set;
        }

        //화면내 combo 셋팅
        private void InitCombo()
        {
            CommonCombo_Form _combo = new CommonCombo_Form();

            string[] sFilterEqpType = { "FORM_AGING_TYPE_CODE" ,"N"};
            _combo.SetCombo(cboEqpKind, CommonCombo_Form.ComboStatus.ALL, sCase: "SYSTEM_AREA_COMMON_CODE", sFilter: sFilterEqpType);
            //_combo.SetCombo(cboEqpKind, CommonCombo_Form.ComboStatus.ALL, sCase: "AGINGKIND", sFilter: sFilterEqpType);
        }

        #endregion

        #region Event

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            //정보이상, 입고금지 체크 박스 검색 추가
            //RCV_PRHB_RSN_YN = IsRCV_PRHB_RSN_YN();
            RCV_PRHB_RSN_YN = _Util.IsAreaCommonCodeUse("PROGRAM_BY_FUNC_USE_FLAG", "FCS001_015_RCV_PRHB_RSN"); 

            InitCombo();

            InitControl(); //정보이상, 입고금지 체크 박스 검색 추가

            this.Loaded -= UserControl_Loaded;
        }

        //정보이상, 입고금지 체크 박스 검색 추가
        private void InitControl()
        {
            if(RCV_PRHB_RSN_YN.Equals(true))
            {
                this.chkAbnormal.Visibility = Visibility.Visible;
                this.chkInput.Visibility = Visibility.Visible;

                this.dgEqpStatus.Columns["RCV_PRHB_RSN"].Visibility = Visibility.Visible;
                this.dgEqpStatus.Columns["UPDDTTM"].Visibility = Visibility.Visible;
                this.dgEqpStatus.Columns["UPDUSER"].Visibility = Visibility.Visible;
            }
            else
            {
                this.chkAbnormal.Visibility = Visibility.Hidden;
                this.chkInput.Visibility = Visibility.Hidden;

                this.dgEqpStatus.Columns["RCV_PRHB_RSN"].Visibility = Visibility.Collapsed;
                this.dgEqpStatus.Columns["UPDDTTM"].Visibility = Visibility.Collapsed;
                this.dgEqpStatus.Columns["UPDUSER"].Visibility = Visibility.Collapsed;
            }
        }

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
                dtRqst.Columns.Add("LANGID", typeof(string));
                //dtRqst.Columns.Add("EQPT_GR_TYPE_CODE", typeof(string));
                //dtRqst.Columns.Add("LANE_ID", typeof(string));
                dtRqst.Columns.Add("CMCODE", typeof(string));
                dtRqst.Columns.Add("AREAID", typeof(string));
                dtRqst.Columns.Add("ABNORMAL_YN", typeof(string));
                dtRqst.Columns.Add("INPUT_YN", typeof(string));

                DataRow dr = dtRqst.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["AREAID"] = LoginInfo.CFG_AREA_ID;

                if (!string.IsNullOrEmpty(cboEqpKind.SelectedValue.ToString()))
                {
                    //string[] sFilter = cboEqpKind.SelectedValue.ToString().Split('_');
                    //dr["EQPT_GR_TYPE_CODE"] = sFilter[0];
                    //dr["LANE_ID"] = sFilter[1];
                    dr["CMCODE"] = cboEqpKind.SelectedValue.ToString();
                }

                //정보이상, 입고금지 체크 박스 검색 추가
                if (RCV_PRHB_RSN_YN.Equals(true))
                {                    
                    if (this.chkAbnormal.IsChecked == true)
                    {
                        dr["ABNORMAL_YN"] = "Y";
                    }
                    else
                    {
                        dr["ABNORMAL_YN"] = "N";
                    }

                    if (this.chkInput.IsChecked == true)
                    {
                        dr["INPUT_YN"] = "Y";
                    }
                    else
                    {
                        dr["INPUT_YN"] = "N";
                    }
                }
                else
                {
                    dr["ABNORMAL_YN"] = "Y";
                }
                    

                dtRqst.Rows.Add(dr);

                ShowLoadingIndicator();
                //DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_SEL_AGING_ABNOMAL_RACK", "INDATA", "OUTDATA", dtRqst);
                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("BR_GET_AGING_ABNOMAL_RACK", "INDATA", "OUTDATA", dtRqst);
                Util.GridSetData(dgEqpStatus, dtRslt, FrameOperation, true);
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
            finally
            {
                HiddenLoadingIndicator();
            }
        }

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

        //정보이상, 입고금지 체크 박스 검색 추가
        private bool IsRCV_PRHB_RSN_YN()
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("AREAID", typeof(string));
                RQSTDT.Columns.Add("COM_TYPE_CODE", typeof(string));
                RQSTDT.Columns.Add("COM_CODE", typeof(string));
                RQSTDT.Columns.Add("USE_FLAG", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["AREAID"] = LoginInfo.CFG_AREA_ID;
                dr["COM_TYPE_CODE"] = "FORMLGS_AGING_ABNORMAL_RCV_PRHB_RSN_YN";
                dr["COM_CODE"] = "USE_YN";
                dr["USE_FLAG"] = "Y";
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_AREA_COM_CODE_USE", "RQSTDT", "RSLTDT", RQSTDT);

                if (dtResult.Rows.Count > 0 && dtResult.Rows[0]["ATTR1"] != null &&
                    dtResult.Rows[0]["ATTR1"].ToString().Equals("Y"))
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
            return false;
        }

    }
}
