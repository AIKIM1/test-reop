/*************************************************************************************
 Created Date : 2020.11.23
      Creator : Kang Dong Hee
   Decription : Box 유지보수
--------------------------------------------------------------------------------------
 [Change History]
  2020.11.23  NAME : Initial Created
  2021.03.31  KDH : GetTestData 함수 호출 로직 제거
  2021.04.13  KDH : 화면간 이동 시 초기화 현상 제거
  2021.05.06  KDH : LANGID 대입값 수정
  2024.05.01  조영대 : Formation 구분 콤보 추가
  2024.07.09  남형희 : E20240528-000667 BM 등록(해제 전) 상태에서는 시작 시간을 ACTDTTM(등록 시간)으로 보여지도록 쿼리 수정
  2024.08.09  김용준 : 전체 라인 조회 추가 E20240515-000763
**************************************************************************************/


using C1.WPF;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using System;
using System.Data;
using System.Windows;
using System.Windows.Controls;

namespace LGC.GMES.MES.FCS001
{
    /// <summary>
    /// TSK_120.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class FCS001_072 : UserControl, IWorkArea
    {
        private string _LANEID = string.Empty;
        private string _FORMATIONTYPE = string.Empty;

        public IFrameOperation FrameOperation
        {
            get;
            set;
        }

        public FCS001_072()
        {
            InitializeComponent();
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            object[] tmps = this.FrameOperation.Parameters;
            if (tmps != null && tmps.Length >= 1)
            {
                _LANEID = Util.NVC(tmps[0]);
                if (tmps.Length >= 2) _FORMATIONTYPE = Util.NVC(tmps[1]);
            }

            InitCombo();
            InitControl();

            if (!string.IsNullOrEmpty(_LANEID))
            {
                cboLane.SelectedValue = _LANEID;
                GetList();
            }

            this.Loaded -= UserControl_Loaded; //2021.04.13 화면간 이동 시 초기화 현상 제거
        }

        private void InitCombo()
        {
            try
            {
                CommonCombo_Form _combo = new CommonCombo_Form();

                string[] sData = { "Formation", "1", "JIG_FORMATION", "J" };
                cboFormationType.SetDataComboItem(sData, CommonCombo.ComboStatus.ALL);
                if (!string.IsNullOrEmpty(_FORMATIONTYPE))
                {
                    cboFormationType.SelectedValue = _FORMATIONTYPE;
                }

                string[] sFilter = { "FORMEQPT_MAINT_STAT_CODE" };
                _combo.SetCombo(cboFlag, CommonCombo_Form.ComboStatus.ALL, sFilter: sFilter, sCase: "CMN");


            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void cboFormationType_SelectionCommitted(object sender, PropertyChangedEventArgs<object> e)
        {
            SetLane();
        }

        private void cboLane_SelectionCommitted(object sender, PropertyChangedEventArgs<object> e)
        {
            SetRow();
            SetCol();
            SetStg();
        }

        private void SetLane()
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("ONLY_X", typeof(string));
                RQSTDT.Columns.Add("EQPT_GR_TYPE_CODE", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["ONLY_X"] = "Y";
                dr["EQPT_GR_TYPE_CODE"] = cboFormationType.GetBindValue();
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_LANE_FORMATION_CBO", "RQSTDT", "RSLTDT", RQSTDT);

                // 2024-08-09 김용준 : 전체 라인 조회 추가
                cboLane.SetDataComboItem(dtResult, CommonCombo.ComboStatus.ALL);
                if (cboLane.Items.Count > 0) cboLane.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        public void SetRow()
        {
            try
            {
                if (cboLane.GetBindValue() == null)
                {
                    if (cboLane.SelectedIndex != 0)
                    {
                        return;
                    }
                }

                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("S70", typeof(string));
                RQSTDT.Columns.Add("S71", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["S71"] = cboLane.GetBindValue();
                dr["S70"] = cboLane.GetBindValue("EQPT_GR_TYPE_CODE");
                if (dr["S70"].Equals("J")) dr["S70"] = "1";
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_COMBO_UNIT_ROW_INFO", "RQSTDT", "RSLTDT", RQSTDT);

                cboRow.SetDataComboItem(dtResult, CommonCombo.ComboStatus.ALL);

                if (cboRow.Items.Count > 0) cboRow.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        public void SetCol()
        {
            try
            {
                if (cboLane.GetBindValue() == null)
                {
                    if(cboLane.SelectedIndex != 0)
                    {
                        return;
                    }
                }

                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("S70", typeof(string));
                RQSTDT.Columns.Add("S71", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["S71"] = cboLane.GetBindValue();
                dr["S70"] = cboLane.GetBindValue("EQPT_GR_TYPE_CODE");
                if (dr["S70"].Equals("J")) dr["S70"] = "1";
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_COMBO_UNIT_COL_INFO", "RQSTDT", "RSLTDT", RQSTDT);

                cboCol.SetDataComboItem(dtResult, CommonCombo.ComboStatus.ALL);

                if (cboCol.Items.Count > 0) cboCol.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        public void SetStg()
        {
            try
            {
                if (cboLane.GetBindValue() == null)
                {
                    if (cboLane.SelectedIndex != 0)
                    {
                        return;
                    }
                }

                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("S70", typeof(string));
                RQSTDT.Columns.Add("S71", typeof(string));
                RQSTDT.Columns.Add("AGING_YN", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["S71"] = cboLane.GetBindValue();
                dr["S70"] = cboLane.GetBindValue("EQPT_GR_TYPE_CODE");
                if (dr["S70"].Equals("J")) dr["S70"] = "1";
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_COMBO_UNIT_STG_INFO", "RQSTDT", "RSLTDT", RQSTDT);

                cboStg.SetDataComboItem(dtResult, CommonCombo.ComboStatus.ALL);

                if (cboStg.Items.Count > 0) cboStg.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void InitControl()
        {
            try
            {
                dtpFromDate.SelectedDateTime = DateTime.Now.AddDays(-7);
                dtpToDate.SelectedDateTime = DateTime.Now;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }

        }

        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            GetList();
        }

        private void GetList()
        {
            try
            {
                DataTable dtRqst = new DataTable();
                dtRqst.TableName = "RQSTDT";
                dtRqst.Columns.Add("LANGID", typeof(string));
                dtRqst.Columns.Add("S71", typeof(string));
                dtRqst.Columns.Add("MAINT_STAT_CODE", typeof(string));
                dtRqst.Columns.Add("FROM_DAY", typeof(string));
                dtRqst.Columns.Add("TO_DAY", typeof(string));
                dtRqst.Columns.Add("S67", typeof(string));
                dtRqst.Columns.Add("S68", typeof(string));
                dtRqst.Columns.Add("S70", typeof(string));

                DataRow dr = dtRqst.NewRow();
                dr["LANGID"] = LoginInfo.LANGID; //2021.05.06 LANGID 대입값 수정
                
                // 2024-08-09 김용준 : 전체 라인 조회 추가
                if (!string.IsNullOrWhiteSpace(Util.GetCondition(cboLane)))
                {
                    dr["S71"] = Util.GetCondition(cboLane);
                }

                dr["MAINT_STAT_CODE"] = Util.GetCondition(cboFlag, bAllNull: true);
                dr["FROM_DAY"] = dtpFromDate.SelectedDateTime.ToString("yyyyMMdd");
                dr["TO_DAY"] = dtpToDate.SelectedDateTime.ToString("yyyyMMdd");
                dr["S67"] = Util.GetCondition(cboCol, bAllNull: true);
                dr["S68"] = Util.GetCondition(cboStg, bAllNull: true);
                dr["S70"] = cboFormationType.GetBindValue();
                dtRqst.Rows.Add(dr);

                dgList.ExecuteService("DA_SEL_EQP_MNT_DAY", "RQSTDT", "RSLTDT", dtRqst);

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        public void callOther(string sLaneId, string sCol, string sStg)
        {
            cboLane.SelectedValue = sLaneId;
            cboCol.SelectedValue = sCol;
            cboStg.SelectedValue = sStg;

            btnSearch_Click(null, null);
        }

        private void GetTestData(ref DataTable dt)
        {
            dt.TableName = "RQSTDT";
            //dt.Columns.Add("TRAY_OP_STATUS_CD", typeof(string));

            #region ROW ADD
            DataRow row1 = dt.NewRow();
            row1["EQPTID"] = "E111010101";
            row1["EQPTNAME"] = "ESS 1호 01열 01연 01단 Box";
            row1["ACTDTTM"] = "20210111";
            row1["SHIFT_USER_NAME"] = "吴家伟";
            row1["MAINT_PART_NAME"] = "";
            row1["MAINT_CNTT"] = "......";
            row1["MAINT_CODE"] = "M0";
            row1["CMCDNAME"] = "수동 BM";
            row1["MAINT_STRT_DTTM"] = "01/11/2021 11:44:29";
            row1["MAINT_END_DTTM"] = "01/11/2021 11:44:43";
            row1["MAINT_STAT_CODE"] = "C";
            row1["MNT_FLAG_NAME"] = "부동";
            row1["INTERVAL_TIME"] = "0.000162037037037037";
            dt.Rows.Add(row1);
            DataRow row2 = dt.NewRow();
            row2["EQPTID"] = "E111010102";
            row2["EQPTNAME"] = "ESS 1호 01열 01연 02단 Box";
            row2["ACTDTTM"] = "20210111";
            row2["SHIFT_USER_NAME"] = "吴家伟";
            row2["MAINT_PART_NAME"] = "";
            row2["MAINT_CNTT"] = "  ";
            row2["MAINT_CODE"] = "M0";
            row2["CMCDNAME"] = "수동 BM";
            row2["MAINT_STRT_DTTM"] = "01/11/2021 12:47:24";
            row2["MAINT_END_DTTM"] = "01/11/2021 12:47:31";
            row2["MAINT_STAT_CODE"] = "C";
            row2["MNT_FLAG_NAME"] = "부동";
            row2["INTERVAL_TIME"] = "0.0000810185185185185";
            dt.Rows.Add(row2);
            DataRow row3 = dt.NewRow();
            row3["EQPTID"] = "E111010103";
            row3["EQPTNAME"] = "ESS 1호 01열 01연 03단 Box";
            row3["ACTDTTM"] = "20210111";
            row3["SHIFT_USER_NAME"] = "杨天恩";
            row3["MAINT_PART_NAME"] = "";
            row3["MAINT_CNTT"] = "   ";
            row3["MAINT_CODE"] = "M0";
            row3["CMCDNAME"] = "수동 BM";
            row3["MAINT_STRT_DTTM"] = "01/11/2021 13:18:14";
            row3["MAINT_END_DTTM"] = "01/11/2021 13:18:21";
            row3["MAINT_STAT_CODE"] = "C";
            row3["MNT_FLAG_NAME"] = "부동";
            row3["INTERVAL_TIME"] = "0.0000810185185185185";
            dt.Rows.Add(row3);
            DataRow row4 = dt.NewRow();
            row4["EQPTID"] = "E111010104";
            row4["EQPTNAME"] = "ESS 1호 01열 01연 04단 Box";
            row4["ACTDTTM"] = "20210111";
            row4["SHIFT_USER_NAME"] = "李芹";
            row4["MAINT_PART_NAME"] = "";
            row4["MAINT_CNTT"] = "test";
            row4["MAINT_CODE"] = "E";
            row4["CMCDNAME"] = "생산준비";
            row4["MAINT_STRT_DTTM"] = "01/11/2021 11:37:25";
            row4["MAINT_END_DTTM"] = "01/11/2021 13:18:34";
            row4["MAINT_STAT_CODE"] = "C";
            row4["MNT_FLAG_NAME"] = "부동";
            row4["INTERVAL_TIME"] = "0.0702430555555556";
            dt.Rows.Add(row4);
            DataRow row5 = dt.NewRow();
            row5["EQPTID"] = "E111010105";
            row5["EQPTNAME"] = "ESS 1호 01열 01연 05단 Box";
            row5["ACTDTTM"] = "20210111";
            row5["SHIFT_USER_NAME"] = "杨天恩";
            row5["MAINT_PART_NAME"] = "";
            row5["MAINT_CNTT"] = "   ";
            row5["MAINT_CODE"] = "M0";
            row5["CMCDNAME"] = "수동 BM";
            row5["MAINT_STRT_DTTM"] = "01/11/2021 13:18:46";
            row5["MAINT_END_DTTM"] = "01/11/2021 13:18:56";
            row5["MAINT_STAT_CODE"] = "C";
            row5["MNT_FLAG_NAME"] = "부동";
            row5["INTERVAL_TIME"] = "0.000115740740740741";
            dt.Rows.Add(row5);
            DataRow row6 = dt.NewRow();
            row6["EQPTID"] = "E111010106";
            row6["EQPTNAME"] = "ESS 1호 01열 01연 06단 Box";
            row6["ACTDTTM"] = "20210111";
            row6["SHIFT_USER_NAME"] = "杨天恩";
            row6["MAINT_PART_NAME"] = "";
            row6["MAINT_CNTT"] = "厂家检修";
            row6["MAINT_CODE"] = "M0";
            row6["CMCDNAME"] = "수동 BM";
            row6["MAINT_STRT_DTTM"] = "05/01/2020 16:07:00";
            row6["MAINT_END_DTTM"] = "01/11/2021 11:06:27";
            row6["MAINT_STAT_CODE"] = "C";
            row6["MNT_FLAG_NAME"] = "부동";
            row6["INTERVAL_TIME"] = "254.791284722222";
            dt.Rows.Add(row6);
            DataRow row7 = dt.NewRow();
            row7["EQPTID"] = "E111010107";
            row7["EQPTNAME"] = "ESS 1호 01열 01연 07단 Box";
            row7["ACTDTTM"] = "20210111";
            row7["SHIFT_USER_NAME"] = "杨天恩";
            row7["MAINT_PART_NAME"] = "";
            row7["MAINT_CNTT"] = " ";
            row7["MAINT_CODE"] = "M0";
            row7["CMCDNAME"] = "수동 BM";
            row7["MAINT_STRT_DTTM"] = "01/11/2021 13:19:05";
            row7["MAINT_END_DTTM"] = "01/11/2021 13:19:13";
            row7["MAINT_STAT_CODE"] = "C";
            row7["MNT_FLAG_NAME"] = "부동";
            row7["INTERVAL_TIME"] = "0.0000925925925925926";
            dt.Rows.Add(row7);
            DataRow row8 = dt.NewRow();
            row8["EQPTID"] = "E111010108";
            row8["EQPTNAME"] = "ESS 1호 01열 01연 08단 Box";
            row8["ACTDTTM"] = "20210111";
            row8["SHIFT_USER_NAME"] = "吴家伟";
            row8["MAINT_PART_NAME"] = "";
            row8["MAINT_CNTT"] = "...";
            row8["MAINT_CODE"] = "M0";
            row8["CMCDNAME"] = "수동 BM";
            row8["MAINT_STRT_DTTM"] = "01/11/2021 11:43:32";
            row8["MAINT_END_DTTM"] = "01/11/2021 11:43:55";
            row8["MAINT_STAT_CODE"] = "C";
            row8["MNT_FLAG_NAME"] = "부동";
            row8["INTERVAL_TIME"] = "0.000266203703703704";
            dt.Rows.Add(row8);
            DataRow row9 = dt.NewRow();
            row9["EQPTID"] = "E111010201";
            row9["EQPTNAME"] = "ESS 1호 01열 02연 01단 Box";
            row9["ACTDTTM"] = "20210112";
            row9["SHIFT_USER_NAME"] = "陈晗铃";
            row9["MAINT_PART_NAME"] = "";
            row9["MAINT_CNTT"] = "厂家检修";
            row9["MAINT_CODE"] = "M1";
            row9["CMCDNAME"] = "수동 BM";
            row9["MAINT_STRT_DTTM"] = "04/08/2020 10:32:32";
            row9["MAINT_END_DTTM"] = "01/12/2021 17:50:32";
            row9["MAINT_STAT_CODE"] = "C";
            row9["MNT_FLAG_NAME"] = "부동";
            row9["INTERVAL_TIME"] = "279.304166666667";
            dt.Rows.Add(row9);
            DataRow row10 = dt.NewRow();
            row10["EQPTID"] = "E111010202";
            row10["EQPTNAME"] = "ESS 1호 01열 02연 02단 Box";
            row10["ACTDTTM"] = "20210112";
            row10["SHIFT_USER_NAME"] = "陈晗铃";
            row10["MAINT_PART_NAME"] = "";
            row10["MAINT_CNTT"] = "S/C与库位信号对接异常";
            row10["MAINT_CODE"] = "M0";
            row10["CMCDNAME"] = "수동 BM";
            row10["MAINT_STRT_DTTM"] = "03/25/2020 00:10:02";
            row10["MAINT_END_DTTM"] = "01/12/2021 17:50:22";
            row10["MAINT_STAT_CODE"] = "C";
            row10["MNT_FLAG_NAME"] = "부동";
            row10["INTERVAL_TIME"] = "293.736342592593";
            dt.Rows.Add(row10);
            DataRow row11 = dt.NewRow();
            row11["EQPTID"] = "E111010203";
            row11["EQPTNAME"] = "ESS 1호 01열 02연 03단 Box";
            row11["ACTDTTM"] = "20210112";
            row11["SHIFT_USER_NAME"] = "陈晗铃";
            row11["MAINT_PART_NAME"] = "";
            row11["MAINT_CNTT"] = " 厂家检修";
            row11["MAINT_CODE"] = "M1";
            row11["CMCDNAME"] = "수동 BM";
            row11["MAINT_STRT_DTTM"] = "04/25/2020 14:57:06";
            row11["MAINT_END_DTTM"] = "01/12/2021 17:50:14";
            row11["MAINT_STAT_CODE"] = "C";
            row11["MNT_FLAG_NAME"] = "부동";
            row11["INTERVAL_TIME"] = "262.120231481481";
            dt.Rows.Add(row11);
            DataRow row12 = dt.NewRow();
            row12["EQPTID"] = "E111010204";
            row12["EQPTNAME"] = "ESS 1호 01열 02연 04단 Box";
            row12["ACTDTTM"] = "20210112";
            row12["SHIFT_USER_NAME"] = "陈晗铃";
            row12["MAINT_PART_NAME"] = "";
            row12["MAINT_CNTT"] = "  ";
            row12["MAINT_CODE"] = "M1";
            row12["CMCDNAME"] = "수동 BM";
            row12["MAINT_STRT_DTTM"] = "04/08/2020 10:32:18";
            row12["MAINT_END_DTTM"] = "01/12/2021 17:49:54";
            row12["MAINT_STAT_CODE"] = "C";
            row12["MNT_FLAG_NAME"] = "부동";
            row12["INTERVAL_TIME"] = "279.303888888889";
            dt.Rows.Add(row12);
            DataRow row13 = dt.NewRow();
            row13["EQPTID"] = "E111010205";
            row13["EQPTNAME"] = "ESS 1호 01열 02연 05단 Box";
            row13["ACTDTTM"] = "20210112";
            row13["SHIFT_USER_NAME"] = "陈晗铃";
            row13["MAINT_PART_NAME"] = "";
            row13["MAINT_CNTT"] = "厂家检修";
            row13["MAINT_CODE"] = "M2";
            row13["CMCDNAME"] = "수동 BM";
            row13["MAINT_STRT_DTTM"] = "04/08/2020 10:33:47";
            row13["MAINT_END_DTTM"] = "01/12/2021 17:49:45";
            row13["MAINT_STAT_CODE"] = "C";
            row13["MNT_FLAG_NAME"] = "부동";
            row13["INTERVAL_TIME"] = "279.30275462963";
            dt.Rows.Add(row13);
            DataRow row14 = dt.NewRow();
            row14["EQPTID"] = "E111010206";
            row14["EQPTNAME"] = "ESS 1호 01열 02연 06단 Box";
            row14["ACTDTTM"] = "20210112";
            row14["SHIFT_USER_NAME"] = "陈晗铃";
            row14["MAINT_PART_NAME"] = "";
            row14["MAINT_CNTT"] = "d ";
            row14["MAINT_CODE"] = "M0";
            row14["CMCDNAME"] = "수동 BM";
            row14["MAINT_STRT_DTTM"] = "01/12/2021 17:49:22";
            row14["MAINT_END_DTTM"] = "01/12/2021 17:49:33";
            row14["MAINT_STAT_CODE"] = "C";
            row14["MNT_FLAG_NAME"] = "부동";
            row14["INTERVAL_TIME"] = "0.000127314814814815";
            dt.Rows.Add(row14);
            DataRow row15 = dt.NewRow();
            row15["EQPTID"] = "E111010207";
            row15["EQPTNAME"] = "ESS 1호 01열 02연 07단 Box";
            row15["ACTDTTM"] = "20210112";
            row15["SHIFT_USER_NAME"] = "陈晗铃";
            row15["MAINT_PART_NAME"] = "";
            row15["MAINT_CNTT"] = "d ";
            row15["MAINT_CODE"] = "M0";
            row15["CMCDNAME"] = "수동 BM";
            row15["MAINT_STRT_DTTM"] = "01/12/2021 17:49:01";
            row15["MAINT_END_DTTM"] = "01/12/2021 17:49:10";
            row15["MAINT_STAT_CODE"] = "C";
            row15["MNT_FLAG_NAME"] = "부동";
            row15["INTERVAL_TIME"] = "0.000104166666666667";
            dt.Rows.Add(row15);
            DataRow row16 = dt.NewRow();
            row16["EQPTID"] = "E111010208";
            row16["EQPTNAME"] = "ESS 1호 01열 02연 08단 Box";
            row16["ACTDTTM"] = "20210112";
            row16["SHIFT_USER_NAME"] = "陈晗铃";
            row16["MAINT_PART_NAME"] = "";
            row16["MAINT_CNTT"] = "   ";
            row16["MAINT_CODE"] = "M0";
            row16["CMCDNAME"] = "수동 BM";
            row16["MAINT_STRT_DTTM"] = "01/12/2021 17:48:21";
            row16["MAINT_END_DTTM"] = "01/12/2021 17:48:31";
            row16["MAINT_STAT_CODE"] = "C";
            row16["MNT_FLAG_NAME"] = "부동";
            row16["INTERVAL_TIME"] = "0.000115740740740741";
            dt.Rows.Add(row16);
            DataRow row17 = dt.NewRow();
            row17["EQPTID"] = "E111010301";
            row17["EQPTNAME"] = "ESS 1호 01열 03연 01단 Box";
            row17["ACTDTTM"] = "20210112";
            row17["SHIFT_USER_NAME"] = "陈晗铃";
            row17["MAINT_PART_NAME"] = "";
            row17["MAINT_CNTT"] = "  ";
            row17["MAINT_CODE"] = "M0";
            row17["CMCDNAME"] = "수동 BM";
            row17["MAINT_STRT_DTTM"] = "01/12/2021 17:52:18";
            row17["MAINT_END_DTTM"] = "01/12/2021 17:52:34";
            row17["MAINT_STAT_CODE"] = "C";
            row17["MNT_FLAG_NAME"] = "부동";
            row17["INTERVAL_TIME"] = "0.000185185185185185";
            dt.Rows.Add(row17);
            DataRow row18 = dt.NewRow();
            row18["EQPTID"] = "E111010302";
            row18["EQPTNAME"] = "ESS 1호 01열 03연 02단 Box";
            row18["ACTDTTM"] = "20210112";
            row18["SHIFT_USER_NAME"] = "陈晗铃";
            row18["MAINT_PART_NAME"] = "";
            row18["MAINT_CNTT"] = "  ";
            row18["MAINT_CODE"] = "M0";
            row18["CMCDNAME"] = "수동 BM";
            row18["MAINT_STRT_DTTM"] = "01/12/2021 17:52:09";
            row18["MAINT_END_DTTM"] = "01/12/2021 17:52:26";
            row18["MAINT_STAT_CODE"] = "C";
            row18["MNT_FLAG_NAME"] = "부동";
            row18["INTERVAL_TIME"] = "0.000196759259259259";
            dt.Rows.Add(row18);
            DataRow row19 = dt.NewRow();
            row19["EQPTID"] = "E111010303";
            row19["EQPTNAME"] = "ESS 1호 01열 03연 03단 Box";
            row19["ACTDTTM"] = "20210112";
            row19["SHIFT_USER_NAME"] = "陈晗铃";
            row19["MAINT_PART_NAME"] = "";
            row19["MAINT_CNTT"] = "  ";
            row19["MAINT_CODE"] = "M0";
            row19["CMCDNAME"] = "수동 BM";
            row19["MAINT_STRT_DTTM"] = "01/12/2021 17:51:55";
            row19["MAINT_END_DTTM"] = "01/12/2021 17:52:03";
            row19["MAINT_STAT_CODE"] = "C";
            row19["MNT_FLAG_NAME"] = "부동";
            row19["INTERVAL_TIME"] = "0.0000925925925925926";
            dt.Rows.Add(row19);
            DataRow row20 = dt.NewRow();
            row20["EQPTID"] = "E111010304";
            row20["EQPTNAME"] = "ESS 1호 01열 03연 04단 Box";
            row20["ACTDTTM"] = "20210112";
            row20["SHIFT_USER_NAME"] = "陈晗铃";
            row20["MAINT_PART_NAME"] = "";
            row20["MAINT_CNTT"] = "  ";
            row20["MAINT_CODE"] = "M0";
            row20["CMCDNAME"] = "수동 BM";
            row20["MAINT_STRT_DTTM"] = "01/12/2021 17:51:39";
            row20["MAINT_END_DTTM"] = "01/12/2021 17:51:47";
            row20["MAINT_STAT_CODE"] = "C";
            row20["MNT_FLAG_NAME"] = "부동";
            row20["INTERVAL_TIME"] = "0.0000925925925925926";
            dt.Rows.Add(row20);
            DataRow row21 = dt.NewRow();
            row21["EQPTID"] = "E111010305";
            row21["EQPTNAME"] = "ESS 1호 01열 03연 05단 Box";
            row21["ACTDTTM"] = "20210112";
            row21["SHIFT_USER_NAME"] = "陈晗铃";
            row21["MAINT_PART_NAME"] = "";
            row21["MAINT_CNTT"] = "厂家检修";
            row21["MAINT_CODE"] = "M2";
            row21["CMCDNAME"] = "수동 BM";
            row21["MAINT_STRT_DTTM"] = "04/08/2020 10:35:19";
            row21["MAINT_END_DTTM"] = "01/12/2021 17:51:30";
            row21["MAINT_STAT_CODE"] = "C";
            row21["MNT_FLAG_NAME"] = "부동";
            row21["INTERVAL_TIME"] = "279.302905092593";
            dt.Rows.Add(row21);
            DataRow row22 = dt.NewRow();
            row22["EQPTID"] = "E111010306";
            row22["EQPTNAME"] = "ESS 1호 01열 03연 06단 Box";
            row22["ACTDTTM"] = "20210112";
            row22["SHIFT_USER_NAME"] = "陈晗铃";
            row22["MAINT_PART_NAME"] = "";
            row22["MAINT_CNTT"] = "  ";
            row22["MAINT_CODE"] = "M0";
            row22["CMCDNAME"] = "수동 BM";
            row22["MAINT_STRT_DTTM"] = "01/12/2021 17:51:13";
            row22["MAINT_END_DTTM"] = "01/12/2021 17:51:22";
            row22["MAINT_STAT_CODE"] = "C";
            row22["MNT_FLAG_NAME"] = "부동";
            row22["INTERVAL_TIME"] = "0.000104166666666667";
            dt.Rows.Add(row22);
            DataRow row23 = dt.NewRow();
            row23["EQPTID"] = "E111010307";
            row23["EQPTNAME"] = "ESS 1호 01열 03연 07단 Box";
            row23["ACTDTTM"] = "20210112";
            row23["SHIFT_USER_NAME"] = "陈晗铃";
            row23["MAINT_PART_NAME"] = "";
            row23["MAINT_CNTT"] = "    ";
            row23["MAINT_CODE"] = "M2";
            row23["CMCDNAME"] = "수동 BM";
            row23["MAINT_STRT_DTTM"] = "04/08/2020 12:21:34";
            row23["MAINT_END_DTTM"] = "01/12/2021 17:51:05";
            row23["MAINT_STAT_CODE"] = "C";
            row23["MNT_FLAG_NAME"] = "부동";
            row23["INTERVAL_TIME"] = "279.228831018519";
            dt.Rows.Add(row23);
            DataRow row24 = dt.NewRow();
            row24["EQPTID"] = "E111010308";
            row24["EQPTNAME"] = "ESS 1호 01열 03연 08단 Box";
            row24["ACTDTTM"] = "20210112";
            row24["SHIFT_USER_NAME"] = "陈晗铃";
            row24["MAINT_PART_NAME"] = "";
            row24["MAINT_CNTT"] = "   ";
            row24["MAINT_CODE"] = "M0";
            row24["CMCDNAME"] = "수동 BM";
            row24["MAINT_STRT_DTTM"] = "01/12/2021 17:50:42";
            row24["MAINT_END_DTTM"] = "01/12/2021 17:50:51";
            row24["MAINT_STAT_CODE"] = "C";
            row24["MNT_FLAG_NAME"] = "부동";
            row24["INTERVAL_TIME"] = "0.000104166666666667";
            dt.Rows.Add(row24);
            DataRow row25 = dt.NewRow();
            row25["EQPTID"] = "E111010401";
            row25["EQPTNAME"] = "ESS 1호 01열 04연 01단 Box";
            row25["ACTDTTM"] = "20210112";
            row25["SHIFT_USER_NAME"] = "陈晗铃";
            row25["MAINT_PART_NAME"] = "";
            row25["MAINT_CNTT"] = "  ";
            row25["MAINT_CODE"] = "M0";
            row25["CMCDNAME"] = "수동 BM";
            row25["MAINT_STRT_DTTM"] = "01/12/2021 17:57:23";
            row25["MAINT_END_DTTM"] = "01/12/2021 17:57:49";
            row25["MAINT_STAT_CODE"] = "C";
            row25["MNT_FLAG_NAME"] = "부동";
            row25["INTERVAL_TIME"] = "0.000300925925925926";
            dt.Rows.Add(row25);
            DataRow row26 = dt.NewRow();
            row26["EQPTID"] = "E111010402";
            row26["EQPTNAME"] = "ESS 1호 01열 04연 02단 Box";
            row26["ACTDTTM"] = "20210112";
            row26["SHIFT_USER_NAME"] = "陈晗铃";
            row26["MAINT_PART_NAME"] = "";
            row26["MAINT_CNTT"] = "  ";
            row26["MAINT_CODE"] = "M0";
            row26["CMCDNAME"] = "수동 BM";
            row26["MAINT_STRT_DTTM"] = "01/12/2021 17:57:14";
            row26["MAINT_END_DTTM"] = "01/12/2021 17:57:41";
            row26["MAINT_STAT_CODE"] = "C";
            row26["MNT_FLAG_NAME"] = "부동";
            row26["INTERVAL_TIME"] = "0.0003125";
            dt.Rows.Add(row26);
            DataRow row27 = dt.NewRow();
            row27["EQPTID"] = "E111010403";
            row27["EQPTNAME"] = "ESS 1호 01열 04연 03단 Box";
            row27["ACTDTTM"] = "20210112";
            row27["SHIFT_USER_NAME"] = "陈晗铃";
            row27["MAINT_PART_NAME"] = "";
            row27["MAINT_CNTT"] = "  ";
            row27["MAINT_CODE"] = "M0";
            row27["CMCDNAME"] = "수동 BM";
            row27["MAINT_STRT_DTTM"] = "01/12/2021 17:57:06";
            row27["MAINT_END_DTTM"] = "01/12/2021 17:57:31";
            row27["MAINT_STAT_CODE"] = "C";
            row27["MNT_FLAG_NAME"] = "부동";
            row27["INTERVAL_TIME"] = "0.000289351851851852";
            dt.Rows.Add(row27);
            DataRow row28 = dt.NewRow();
            row28["EQPTID"] = "E111010404";
            row28["EQPTNAME"] = "ESS 1호 01열 04연 04단 Box";
            row28["ACTDTTM"] = "20210112";
            row28["SHIFT_USER_NAME"] = "杨天恩";
            row28["MAINT_PART_NAME"] = "";
            row28["MAINT_CNTT"] = "厂家检修";
            row28["MAINT_CODE"] = "M0";
            row28["CMCDNAME"] = "수동 BM";
            row28["MAINT_STRT_DTTM"] = "04/29/2020 15:35:27";
            row28["MAINT_END_DTTM"] = "01/12/2021 17:56:57";
            row28["MAINT_STAT_CODE"] = "C";
            row28["MNT_FLAG_NAME"] = "부동";
            row28["INTERVAL_TIME"] = "258.098263888889";
            dt.Rows.Add(row28);
            DataRow row29 = dt.NewRow();
            row29["EQPTID"] = "E111010405";
            row29["EQPTNAME"] = "ESS 1호 01열 04연 05단 Box";
            row29["ACTDTTM"] = "20210112";
            row29["SHIFT_USER_NAME"] = "杨天恩";
            row29["MAINT_PART_NAME"] = "";
            row29["MAINT_CNTT"] = "厂家检修";
            row29["MAINT_CODE"] = "M0";
            row29["CMCDNAME"] = "수동 BM";
            row29["MAINT_STRT_DTTM"] = "04/29/2020 15:35:46";
            row29["MAINT_END_DTTM"] = "01/12/2021 17:56:47";
            row29["MAINT_STAT_CODE"] = "C";
            row29["MNT_FLAG_NAME"] = "부동";
            row29["INTERVAL_TIME"] = "258.097928240741";
            dt.Rows.Add(row29);
            DataRow row30 = dt.NewRow();
            row30["EQPTID"] = "E111010406";
            row30["EQPTNAME"] = "ESS 1호 01열 04연 06단 Box";
            row30["ACTDTTM"] = "20210112";
            row30["SHIFT_USER_NAME"] = "杨天恩";
            row30["MAINT_PART_NAME"] = "";
            row30["MAINT_CNTT"] = "厂家升级";
            row30["MAINT_CODE"] = "M0";
            row30["CMCDNAME"] = "수동 BM";
            row30["MAINT_STRT_DTTM"] = "04/16/2020 18:05:59";
            row30["MAINT_END_DTTM"] = "01/12/2021 17:56:38";
            row30["MAINT_STAT_CODE"] = "C";
            row30["MNT_FLAG_NAME"] = "부동";
            row30["INTERVAL_TIME"] = "270.993506944444";
            dt.Rows.Add(row30);
            DataRow row31 = dt.NewRow();
            row31["EQPTID"] = "E111010407";
            row31["EQPTNAME"] = "ESS 1호 01열 04연 07단 Box";
            row31["ACTDTTM"] = "20210112";
            row31["SHIFT_USER_NAME"] = "陈晗铃";
            row31["MAINT_PART_NAME"] = "";
            row31["MAINT_CNTT"] = "  ";
            row31["MAINT_CODE"] = "M0";
            row31["CMCDNAME"] = "수동 BM";
            row31["MAINT_STRT_DTTM"] = "01/12/2021 17:56:24";
            row31["MAINT_END_DTTM"] = "01/12/2021 17:56:31";
            row31["MAINT_STAT_CODE"] = "C";
            row31["MNT_FLAG_NAME"] = "부동";
            row31["INTERVAL_TIME"] = "0.0000810185185185185";
            dt.Rows.Add(row31);
            DataRow row32 = dt.NewRow();
            row32["EQPTID"] = "E111010408";
            row32["EQPTNAME"] = "ESS 1호 01열 04연 08단 Box";
            row32["ACTDTTM"] = "20210112";
            row32["SHIFT_USER_NAME"] = "陈晗铃";
            row32["MAINT_PART_NAME"] = "";
            row32["MAINT_CNTT"] = "  ";
            row32["MAINT_CODE"] = "M0";
            row32["CMCDNAME"] = "수동 BM";
            row32["MAINT_STRT_DTTM"] = "01/12/2021 17:52:41";
            row32["MAINT_END_DTTM"] = "01/12/2021 17:52:49";
            row32["MAINT_STAT_CODE"] = "C";
            row32["MNT_FLAG_NAME"] = "부동";
            row32["INTERVAL_TIME"] = "0.0000925925925925926";
            dt.Rows.Add(row32);
            DataRow row33 = dt.NewRow();
            row33["EQPTID"] = "E111010501";
            row33["EQPTNAME"] = "ESS 1호 01열 05연 01단 Box";
            row33["ACTDTTM"] = "20210112";
            row33["SHIFT_USER_NAME"] = "陈晗铃";
            row33["MAINT_PART_NAME"] = "";
            row33["MAINT_CNTT"] = "    ";
            row33["MAINT_CODE"] = "M0";
            row33["CMCDNAME"] = "수동 BM";
            row33["MAINT_STRT_DTTM"] = "01/12/2021 17:58:52";
            row33["MAINT_END_DTTM"] = "01/12/2021 17:59:29";
            row33["MAINT_STAT_CODE"] = "C";
            row33["MNT_FLAG_NAME"] = "부동";
            row33["INTERVAL_TIME"] = "0.000428240740740741";
            dt.Rows.Add(row33);
            DataRow row34 = dt.NewRow();
            row34["EQPTID"] = "E111010502";
            row34["EQPTNAME"] = "ESS 1호 01열 05연 02단 Box";
            row34["ACTDTTM"] = "20210112";
            row34["SHIFT_USER_NAME"] = "陈晗铃";
            row34["MAINT_PART_NAME"] = "";
            row34["MAINT_CNTT"] = "   ";
            row34["MAINT_CODE"] = "M0";
            row34["CMCDNAME"] = "수동 BM";
            row34["MAINT_STRT_DTTM"] = "01/12/2021 17:58:44";
            row34["MAINT_END_DTTM"] = "01/12/2021 17:59:11";
            row34["MAINT_STAT_CODE"] = "C";
            row34["MNT_FLAG_NAME"] = "부동";
            row34["INTERVAL_TIME"] = "0.0003125";
            dt.Rows.Add(row34);
            DataRow row35 = dt.NewRow();
            row35["EQPTID"] = "E111010503";
            row35["EQPTNAME"] = "ESS 1호 01열 05연 03단 Box";
            row35["ACTDTTM"] = "20210112";
            row35["SHIFT_USER_NAME"] = "陈晗铃";
            row35["MAINT_PART_NAME"] = "";
            row35["MAINT_CNTT"] = "厂家升级";
            row35["MAINT_CODE"] = "M0";
            row35["CMCDNAME"] = "수동 BM";
            row35["MAINT_STRT_DTTM"] = "04/30/2020 11:18:36";
            row35["MAINT_END_DTTM"] = "01/12/2021 17:58:35";
            row35["MAINT_STAT_CODE"] = "C";
            row35["MNT_FLAG_NAME"] = "부동";
            row35["INTERVAL_TIME"] = "257.277766203704";
            dt.Rows.Add(row35);
            DataRow row36 = dt.NewRow();
            row36["EQPTID"] = "E111010504";
            row36["EQPTNAME"] = "ESS 1호 01열 05연 04단 Box";
            row36["ACTDTTM"] = "20210112";
            row36["SHIFT_USER_NAME"] = "陈晗铃";
            row36["MAINT_PART_NAME"] = "";
            row36["MAINT_CNTT"] = "厂家升级";
            row36["MAINT_CODE"] = "M3";
            row36["CMCDNAME"] = "수동 BM";
            row36["MAINT_STRT_DTTM"] = "04/30/2020 11:18:28";
            row36["MAINT_END_DTTM"] = "01/12/2021 17:58:29";
            row36["MAINT_STAT_CODE"] = "C";
            row36["MNT_FLAG_NAME"] = "부동";
            row36["INTERVAL_TIME"] = "257.277789351852";
            dt.Rows.Add(row36);
            DataRow row37 = dt.NewRow();
            row37["EQPTID"] = "E111010505";
            row37["EQPTNAME"] = "ESS 1호 01열 05연 05단 Box";
            row37["ACTDTTM"] = "20210112";
            row37["SHIFT_USER_NAME"] = "陈晗铃";
            row37["MAINT_PART_NAME"] = "";
            row37["MAINT_CNTT"] = "厂家升级";
            row37["MAINT_CODE"] = "M0";
            row37["CMCDNAME"] = "수동 BM";
            row37["MAINT_STRT_DTTM"] = "04/30/2020 11:18:15";
            row37["MAINT_END_DTTM"] = "01/12/2021 17:58:21";
            row37["MAINT_STAT_CODE"] = "C";
            row37["MNT_FLAG_NAME"] = "부동";
            row37["INTERVAL_TIME"] = "257.277847222222";
            dt.Rows.Add(row37);
            DataRow row38 = dt.NewRow();
            row38["EQPTID"] = "E111010506";
            row38["EQPTNAME"] = "ESS 1호 01열 05연 06단 Box";
            row38["ACTDTTM"] = "20210112";
            row38["SHIFT_USER_NAME"] = "陈晗铃";
            row38["MAINT_PART_NAME"] = "";
            row38["MAINT_CNTT"] = "厂家升级";
            row38["MAINT_CODE"] = "M0";
            row38["CMCDNAME"] = "수동 BM";
            row38["MAINT_STRT_DTTM"] = "04/30/2020 11:18:01";
            row38["MAINT_END_DTTM"] = "01/12/2021 17:58:13";
            row38["MAINT_STAT_CODE"] = "C";
            row38["MNT_FLAG_NAME"] = "부동";
            row38["INTERVAL_TIME"] = "257.277916666667";
            dt.Rows.Add(row38);
            DataRow row39 = dt.NewRow();
            row39["EQPTID"] = "E111010507";
            row39["EQPTNAME"] = "ESS 1호 01열 05연 07단 Box";
            row39["ACTDTTM"] = "20210112";
            row39["SHIFT_USER_NAME"] = "陈晗铃";
            row39["MAINT_PART_NAME"] = "";
            row39["MAINT_CNTT"] = "厂家升级";
            row39["MAINT_CODE"] = "M0";
            row39["CMCDNAME"] = "수동 BM";
            row39["MAINT_STRT_DTTM"] = "04/30/2020 11:17:49";
            row39["MAINT_END_DTTM"] = "01/12/2021 17:58:07";
            row39["MAINT_STAT_CODE"] = "C";
            row39["MNT_FLAG_NAME"] = "부동";
            row39["INTERVAL_TIME"] = "257.277986111111";
            dt.Rows.Add(row39);
            DataRow row40 = dt.NewRow();
            row40["EQPTID"] = "E111010508";
            row40["EQPTNAME"] = "ESS 1호 01열 05연 08단 Box";
            row40["ACTDTTM"] = "20210112";
            row40["SHIFT_USER_NAME"] = "陈晗铃";
            row40["MAINT_PART_NAME"] = "";
            row40["MAINT_CNTT"] = "厂家升级";
            row40["MAINT_CODE"] = "M0";
            row40["CMCDNAME"] = "수동 BM";
            row40["MAINT_STRT_DTTM"] = "04/30/2020 11:17:29";
            row40["MAINT_END_DTTM"] = "01/12/2021 17:57:59";
            row40["MAINT_STAT_CODE"] = "C";
            row40["MNT_FLAG_NAME"] = "부동";
            row40["INTERVAL_TIME"] = "257.278125";
            dt.Rows.Add(row40);
            DataRow row41 = dt.NewRow();
            row41["EQPTID"] = "E111010601";
            row41["EQPTNAME"] = "ESS 1호 01열 06연 01단 Box";
            row41["ACTDTTM"] = "20210111";
            row41["SHIFT_USER_NAME"] = "吴家伟";
            row41["MAINT_PART_NAME"] = "";
            row41["MAINT_CNTT"] = "....";
            row41["MAINT_CODE"] = "M0";
            row41["CMCDNAME"] = "수동 BM";
            row41["MAINT_STRT_DTTM"] = "01/11/2021 11:45:29";
            row41["MAINT_END_DTTM"] = "01/11/2021 11:45:45";
            row41["MAINT_STAT_CODE"] = "C";
            row41["MNT_FLAG_NAME"] = "부동";
            row41["INTERVAL_TIME"] = "0.000185185185185185";
            dt.Rows.Add(row41);
            DataRow row42 = dt.NewRow();
            row42["EQPTID"] = "E111010601";
            row42["EQPTNAME"] = "ESS 1호 01열 06연 01단 Box";
            row42["ACTDTTM"] = "20210126";
            row42["SHIFT_USER_NAME"] = "陈晗铃";
            row42["MAINT_PART_NAME"] = "";
            row42["MAINT_CNTT"] = "A";
            row42["MAINT_CODE"] = "M0";
            row42["CMCDNAME"] = "수동 BM";
            row42["MAINT_STRT_DTTM"] = "01/26/2021 12:36:32";
            row42["MAINT_END_DTTM"] = "01/26/2021 15:13:16";
            row42["MAINT_STAT_CODE"] = "C";
            row42["MNT_FLAG_NAME"] = "부동";
            row42["INTERVAL_TIME"] = "0.108842592592593";
            dt.Rows.Add(row42);
            DataRow row43 = dt.NewRow();
            row43["EQPTID"] = "E111010101";
            row43["EQPTNAME"] = "ESS 1호 01열 01연 01단 Box";
            row43["ACTDTTM"] = "20210111";
            row43["SHIFT_USER_NAME"] = "吴家伟";
            row43["MAINT_PART_NAME"] = "";
            row43["MAINT_CNTT"] = "...";
            row43["MAINT_CODE"] = "M0";
            row43["CMCDNAME"] = "수동 BM";
            row43["MAINT_STRT_DTTM"] = "01/11/2021 11:44:29";
            row43["MAINT_END_DTTM"] = "01/11/2021 11:44:43";
            row43["MAINT_STAT_CODE"] = "R";
            row43["MNT_FLAG_NAME"] = "수리완료";
            row43["INTERVAL_TIME"] = "0.000162037037037037";
            dt.Rows.Add(row43);
            DataRow row44 = dt.NewRow();
            row44["EQPTID"] = "E111010102";
            row44["EQPTNAME"] = "ESS 1호 01열 01연 02단 Box";
            row44["ACTDTTM"] = "20210111";
            row44["SHIFT_USER_NAME"] = "吴家伟";
            row44["MAINT_PART_NAME"] = "";
            row44["MAINT_CNTT"] = " ";
            row44["MAINT_CODE"] = "M0";
            row44["CMCDNAME"] = "수동 BM";
            row44["MAINT_STRT_DTTM"] = "01/11/2021 12:47:24";
            row44["MAINT_END_DTTM"] = "01/11/2021 12:47:31";
            row44["MAINT_STAT_CODE"] = "R";
            row44["MNT_FLAG_NAME"] = "수리완료";
            row44["INTERVAL_TIME"] = "0.0000810185185185185";
            dt.Rows.Add(row44);
            DataRow row45 = dt.NewRow();
            row45["EQPTID"] = "E111010103";
            row45["EQPTNAME"] = "ESS 1호 01열 01연 03단 Box";
            row45["ACTDTTM"] = "20210111";
            row45["SHIFT_USER_NAME"] = "杨天恩";
            row45["MAINT_PART_NAME"] = "";
            row45["MAINT_CNTT"] = " ";
            row45["MAINT_CODE"] = "M0";
            row45["CMCDNAME"] = "수동 BM";
            row45["MAINT_STRT_DTTM"] = "01/11/2021 13:18:14";
            row45["MAINT_END_DTTM"] = "01/11/2021 13:18:21";
            row45["MAINT_STAT_CODE"] = "R";
            row45["MNT_FLAG_NAME"] = "수리완료";
            row45["INTERVAL_TIME"] = "0.0000810185185185185";
            dt.Rows.Add(row45);
            DataRow row46 = dt.NewRow();
            row46["EQPTID"] = "E111010104";
            row46["EQPTNAME"] = "ESS 1호 01열 01연 04단 Box";
            row46["ACTDTTM"] = "20210111";
            row46["SHIFT_USER_NAME"] = "杨天恩";
            row46["MAINT_PART_NAME"] = "";
            row46["MAINT_CNTT"] = "  ";
            row46["MAINT_CODE"] = "E";
            row46["CMCDNAME"] = "생산준비";
            row46["MAINT_STRT_DTTM"] = "01/11/2021 11:37:25";
            row46["MAINT_END_DTTM"] = "01/11/2021 13:18:34";
            row46["MAINT_STAT_CODE"] = "R";
            row46["MNT_FLAG_NAME"] = "수리완료";
            row46["INTERVAL_TIME"] = "0.0702430555555556";
            dt.Rows.Add(row46);
            DataRow row47 = dt.NewRow();
            row47["EQPTID"] = "E111010105";
            row47["EQPTNAME"] = "ESS 1호 01열 01연 05단 Box";
            row47["ACTDTTM"] = "20210111";
            row47["SHIFT_USER_NAME"] = "杨天恩";
            row47["MAINT_PART_NAME"] = "";
            row47["MAINT_CNTT"] = "  ";
            row47["MAINT_CODE"] = "M0";
            row47["CMCDNAME"] = "수동 BM";
            row47["MAINT_STRT_DTTM"] = "01/11/2021 13:18:46";
            row47["MAINT_END_DTTM"] = "01/11/2021 13:18:56";
            row47["MAINT_STAT_CODE"] = "R";
            row47["MNT_FLAG_NAME"] = "수리완료";
            row47["INTERVAL_TIME"] = "0.000115740740740741";
            dt.Rows.Add(row47);
            DataRow row48 = dt.NewRow();
            row48["EQPTID"] = "E111010106";
            row48["EQPTNAME"] = "ESS 1호 01열 01연 06단 Box";
            row48["ACTDTTM"] = "20210111";
            row48["SHIFT_USER_NAME"] = "杨天恩";
            row48["MAINT_PART_NAME"] = "";
            row48["MAINT_CNTT"] = "11";
            row48["MAINT_CODE"] = "M0";
            row48["CMCDNAME"] = "수동 BM";
            row48["MAINT_STRT_DTTM"] = "05/01/2020 16:07:00";
            row48["MAINT_END_DTTM"] = "01/11/2021 11:06:27";
            row48["MAINT_STAT_CODE"] = "R";
            row48["MNT_FLAG_NAME"] = "수리완료";
            row48["INTERVAL_TIME"] = "254.791284722222";
            dt.Rows.Add(row48);
            DataRow row49 = dt.NewRow();
            row49["EQPTID"] = "E111010107";
            row49["EQPTNAME"] = "ESS 1호 01열 01연 07단 Box";
            row49["ACTDTTM"] = "20210111";
            row49["SHIFT_USER_NAME"] = "杨天恩";
            row49["MAINT_PART_NAME"] = "";
            row49["MAINT_CNTT"] = "  ";
            row49["MAINT_CODE"] = "M0";
            row49["CMCDNAME"] = "수동 BM";
            row49["MAINT_STRT_DTTM"] = "01/11/2021 13:19:05";
            row49["MAINT_END_DTTM"] = "01/11/2021 13:19:13";
            row49["MAINT_STAT_CODE"] = "R";
            row49["MNT_FLAG_NAME"] = "수리완료";
            row49["INTERVAL_TIME"] = "0.0000925925925925926";
            dt.Rows.Add(row49);
            DataRow row50 = dt.NewRow();
            row50["EQPTID"] = "E111010108";
            row50["EQPTNAME"] = "ESS 1호 01열 01연 08단 Box";
            row50["ACTDTTM"] = "20210111";
            row50["SHIFT_USER_NAME"] = "吴家伟";
            row50["MAINT_PART_NAME"] = "";
            row50["MAINT_CNTT"] = "....";
            row50["MAINT_CODE"] = "M0";
            row50["CMCDNAME"] = "수동 BM";
            row50["MAINT_STRT_DTTM"] = "01/11/2021 11:43:32";
            row50["MAINT_END_DTTM"] = "01/11/2021 11:43:55";
            row50["MAINT_STAT_CODE"] = "R";
            row50["MNT_FLAG_NAME"] = "수리완료";
            row50["INTERVAL_TIME"] = "0.000266203703703704";
            dt.Rows.Add(row50);
            DataRow row51 = dt.NewRow();
            row51["EQPTID"] = "E111010201";
            row51["EQPTNAME"] = "ESS 1호 01열 02연 01단 Box";
            row51["ACTDTTM"] = "20210112";
            row51["SHIFT_USER_NAME"] = "陈晗铃";
            row51["MAINT_PART_NAME"] = "";
            row51["MAINT_CNTT"] = "  ";
            row51["MAINT_CODE"] = "M1";
            row51["CMCDNAME"] = "수동 BM";
            row51["MAINT_STRT_DTTM"] = "04/08/2020 10:32:32";
            row51["MAINT_END_DTTM"] = "01/12/2021 17:50:32";
            row51["MAINT_STAT_CODE"] = "R";
            row51["MNT_FLAG_NAME"] = "수리완료";
            row51["INTERVAL_TIME"] = "279.304166666667";
            dt.Rows.Add(row51);
            DataRow row52 = dt.NewRow();
            row52["EQPTID"] = "E111010202";
            row52["EQPTNAME"] = "ESS 1호 01열 02연 02단 Box";
            row52["ACTDTTM"] = "20210112";
            row52["SHIFT_USER_NAME"] = "陈晗铃";
            row52["MAINT_PART_NAME"] = "";
            row52["MAINT_CNTT"] = "  ";
            row52["MAINT_CODE"] = "M0";
            row52["CMCDNAME"] = "수동 BM";
            row52["MAINT_STRT_DTTM"] = "03/25/2020 00:10:02";
            row52["MAINT_END_DTTM"] = "01/12/2021 17:50:22";
            row52["MAINT_STAT_CODE"] = "R";
            row52["MNT_FLAG_NAME"] = "수리완료";
            row52["INTERVAL_TIME"] = "293.736342592593";
            dt.Rows.Add(row52);
            DataRow row53 = dt.NewRow();
            row53["EQPTID"] = "E111010203";
            row53["EQPTNAME"] = "ESS 1호 01열 02연 03단 Box";
            row53["ACTDTTM"] = "20210112";
            row53["SHIFT_USER_NAME"] = "陈晗铃";
            row53["MAINT_PART_NAME"] = "";
            row53["MAINT_CNTT"] = "  ";
            row53["MAINT_CODE"] = "M1";
            row53["CMCDNAME"] = "수동 BM";
            row53["MAINT_STRT_DTTM"] = "04/25/2020 14:57:06";
            row53["MAINT_END_DTTM"] = "01/12/2021 17:50:14";
            row53["MAINT_STAT_CODE"] = "R";
            row53["MNT_FLAG_NAME"] = "수리완료";
            row53["INTERVAL_TIME"] = "262.120231481481";
            dt.Rows.Add(row53);
            DataRow row54 = dt.NewRow();
            row54["EQPTID"] = "E111010204";
            row54["EQPTNAME"] = "ESS 1호 01열 02연 04단 Box";
            row54["ACTDTTM"] = "20210112";
            row54["SHIFT_USER_NAME"] = "陈晗铃";
            row54["MAINT_PART_NAME"] = "";
            row54["MAINT_CNTT"] = "  ";
            row54["MAINT_CODE"] = "M1";
            row54["CMCDNAME"] = "수동 BM";
            row54["MAINT_STRT_DTTM"] = "04/08/2020 10:32:18";
            row54["MAINT_END_DTTM"] = "01/12/2021 17:49:54";
            row54["MAINT_STAT_CODE"] = "R";
            row54["MNT_FLAG_NAME"] = "수리완료";
            row54["INTERVAL_TIME"] = "279.303888888889";
            dt.Rows.Add(row54);
            DataRow row55 = dt.NewRow();
            row55["EQPTID"] = "E111010205";
            row55["EQPTNAME"] = "ESS 1호 01열 02연 05단 Box";
            row55["ACTDTTM"] = "20210112";
            row55["SHIFT_USER_NAME"] = "陈晗铃";
            row55["MAINT_PART_NAME"] = "";
            row55["MAINT_CNTT"] = "  ";
            row55["MAINT_CODE"] = "M2";
            row55["CMCDNAME"] = "수동 BM";
            row55["MAINT_STRT_DTTM"] = "04/08/2020 10:33:47";
            row55["MAINT_END_DTTM"] = "01/12/2021 17:49:45";
            row55["MAINT_STAT_CODE"] = "R";
            row55["MNT_FLAG_NAME"] = "수리완료";
            row55["INTERVAL_TIME"] = "279.30275462963";
            dt.Rows.Add(row55);
            DataRow row56 = dt.NewRow();
            row56["EQPTID"] = "E111010206";
            row56["EQPTNAME"] = "ESS 1호 01열 02연 06단 Box";
            row56["ACTDTTM"] = "20210112";
            row56["SHIFT_USER_NAME"] = "陈晗铃";
            row56["MAINT_PART_NAME"] = "";
            row56["MAINT_CNTT"] = " ";
            row56["MAINT_CODE"] = "M0";
            row56["CMCDNAME"] = "수동 BM";
            row56["MAINT_STRT_DTTM"] = "01/12/2021 17:49:22";
            row56["MAINT_END_DTTM"] = "01/12/2021 17:49:33";
            row56["MAINT_STAT_CODE"] = "R";
            row56["MNT_FLAG_NAME"] = "수리완료";
            row56["INTERVAL_TIME"] = "0.000127314814814815";
            dt.Rows.Add(row56);
            DataRow row57 = dt.NewRow();
            row57["EQPTID"] = "E111010207";
            row57["EQPTNAME"] = "ESS 1호 01열 02연 07단 Box";
            row57["ACTDTTM"] = "20210112";
            row57["SHIFT_USER_NAME"] = "陈晗铃";
            row57["MAINT_PART_NAME"] = "";
            row57["MAINT_CNTT"] = "d ";
            row57["MAINT_CODE"] = "M0";
            row57["CMCDNAME"] = "수동 BM";
            row57["MAINT_STRT_DTTM"] = "01/12/2021 17:49:01";
            row57["MAINT_END_DTTM"] = "01/12/2021 17:49:10";
            row57["MAINT_STAT_CODE"] = "R";
            row57["MNT_FLAG_NAME"] = "수리완료";
            row57["INTERVAL_TIME"] = "0.000104166666666667";
            dt.Rows.Add(row57);
            DataRow row58 = dt.NewRow();
            row58["EQPTID"] = "E111010208";
            row58["EQPTNAME"] = "ESS 1호 01열 02연 08단 Box";
            row58["ACTDTTM"] = "20210112";
            row58["SHIFT_USER_NAME"] = "陈晗铃";
            row58["MAINT_PART_NAME"] = "";
            row58["MAINT_CNTT"] = "   ";
            row58["MAINT_CODE"] = "M0";
            row58["CMCDNAME"] = "수동 BM";
            row58["MAINT_STRT_DTTM"] = "01/12/2021 17:48:21";
            row58["MAINT_END_DTTM"] = "01/12/2021 17:48:31";
            row58["MAINT_STAT_CODE"] = "R";
            row58["MNT_FLAG_NAME"] = "수리완료";
            row58["INTERVAL_TIME"] = "0.000115740740740741";
            dt.Rows.Add(row58);
            DataRow row59 = dt.NewRow();
            row59["EQPTID"] = "E111010301";
            row59["EQPTNAME"] = "ESS 1호 01열 03연 01단 Box";
            row59["ACTDTTM"] = "20210112";
            row59["SHIFT_USER_NAME"] = "陈晗铃";
            row59["MAINT_PART_NAME"] = "";
            row59["MAINT_CNTT"] = "  ";
            row59["MAINT_CODE"] = "M0";
            row59["CMCDNAME"] = "수동 BM";
            row59["MAINT_STRT_DTTM"] = "01/12/2021 17:52:18";
            row59["MAINT_END_DTTM"] = "01/12/2021 17:52:34";
            row59["MAINT_STAT_CODE"] = "R";
            row59["MNT_FLAG_NAME"] = "수리완료";
            row59["INTERVAL_TIME"] = "0.000185185185185185";
            dt.Rows.Add(row59);
            DataRow row60 = dt.NewRow();
            row60["EQPTID"] = "E111010302";
            row60["EQPTNAME"] = "ESS 1호 01열 03연 02단 Box";
            row60["ACTDTTM"] = "20210112";
            row60["SHIFT_USER_NAME"] = "陈晗铃";
            row60["MAINT_PART_NAME"] = "";
            row60["MAINT_CNTT"] = "   ";
            row60["MAINT_CODE"] = "M0";
            row60["CMCDNAME"] = "수동 BM";
            row60["MAINT_STRT_DTTM"] = "01/12/2021 17:52:09";
            row60["MAINT_END_DTTM"] = "01/12/2021 17:52:26";
            row60["MAINT_STAT_CODE"] = "R";
            row60["MNT_FLAG_NAME"] = "수리완료";
            row60["INTERVAL_TIME"] = "0.000196759259259259";
            dt.Rows.Add(row60);
            DataRow row61 = dt.NewRow();
            row61["EQPTID"] = "E111010303";
            row61["EQPTNAME"] = "ESS 1호 01열 03연 03단 Box";
            row61["ACTDTTM"] = "20210112";
            row61["SHIFT_USER_NAME"] = "陈晗铃";
            row61["MAINT_PART_NAME"] = "";
            row61["MAINT_CNTT"] = "   ";
            row61["MAINT_CODE"] = "M0";
            row61["CMCDNAME"] = "수동 BM";
            row61["MAINT_STRT_DTTM"] = "01/12/2021 17:51:55";
            row61["MAINT_END_DTTM"] = "01/12/2021 17:52:03";
            row61["MAINT_STAT_CODE"] = "R";
            row61["MNT_FLAG_NAME"] = "수리완료";
            row61["INTERVAL_TIME"] = "0.0000925925925925926";
            dt.Rows.Add(row61);
            DataRow row62 = dt.NewRow();
            row62["EQPTID"] = "E111010304";
            row62["EQPTNAME"] = "ESS 1호 01열 03연 04단 Box";
            row62["ACTDTTM"] = "20210112";
            row62["SHIFT_USER_NAME"] = "陈晗铃";
            row62["MAINT_PART_NAME"] = "";
            row62["MAINT_CNTT"] = "  ";
            row62["MAINT_CODE"] = "M0";
            row62["CMCDNAME"] = "수동 BM";
            row62["MAINT_STRT_DTTM"] = "01/12/2021 17:51:39";
            row62["MAINT_END_DTTM"] = "01/12/2021 17:51:47";
            row62["MAINT_STAT_CODE"] = "R";
            row62["MNT_FLAG_NAME"] = "수리완료";
            row62["INTERVAL_TIME"] = "0.0000925925925925926";
            dt.Rows.Add(row62);
            DataRow row63 = dt.NewRow();
            row63["EQPTID"] = "E111010305";
            row63["EQPTNAME"] = "ESS 1호 01열 03연 05단 Box";
            row63["ACTDTTM"] = "20210112";
            row63["SHIFT_USER_NAME"] = "陈晗铃";
            row63["MAINT_PART_NAME"] = "";
            row63["MAINT_CNTT"] = "  ";
            row63["MAINT_CODE"] = "M2";
            row63["CMCDNAME"] = "수동 BM";
            row63["MAINT_STRT_DTTM"] = "04/08/2020 10:35:19";
            row63["MAINT_END_DTTM"] = "01/12/2021 17:51:30";
            row63["MAINT_STAT_CODE"] = "R";
            row63["MNT_FLAG_NAME"] = "수리완료";
            row63["INTERVAL_TIME"] = "279.302905092593";
            dt.Rows.Add(row63);
            DataRow row64 = dt.NewRow();
            row64["EQPTID"] = "E111010306";
            row64["EQPTNAME"] = "ESS 1호 01열 03연 06단 Box";
            row64["ACTDTTM"] = "20210112";
            row64["SHIFT_USER_NAME"] = "陈晗铃";
            row64["MAINT_PART_NAME"] = "";
            row64["MAINT_CNTT"] = "  ";
            row64["MAINT_CODE"] = "M0";
            row64["CMCDNAME"] = "수동 BM";
            row64["MAINT_STRT_DTTM"] = "01/12/2021 17:51:13";
            row64["MAINT_END_DTTM"] = "01/12/2021 17:51:22";
            row64["MAINT_STAT_CODE"] = "R";
            row64["MNT_FLAG_NAME"] = "수리완료";
            row64["INTERVAL_TIME"] = "0.000104166666666667";
            dt.Rows.Add(row64);
            DataRow row65 = dt.NewRow();
            row65["EQPTID"] = "E111010307";
            row65["EQPTNAME"] = "ESS 1호 01열 03연 07단 Box";
            row65["ACTDTTM"] = "20210112";
            row65["SHIFT_USER_NAME"] = "陈晗铃";
            row65["MAINT_PART_NAME"] = "";
            row65["MAINT_CNTT"] = "  ";
            row65["MAINT_CODE"] = "M2";
            row65["CMCDNAME"] = "수동 BM";
            row65["MAINT_STRT_DTTM"] = "04/08/2020 12:21:34";
            row65["MAINT_END_DTTM"] = "01/12/2021 17:51:05";
            row65["MAINT_STAT_CODE"] = "R";
            row65["MNT_FLAG_NAME"] = "수리완료";
            row65["INTERVAL_TIME"] = "279.228831018519";
            dt.Rows.Add(row65);
            DataRow row66 = dt.NewRow();
            row66["EQPTID"] = "E111010308";
            row66["EQPTNAME"] = "ESS 1호 01열 03연 08단 Box";
            row66["ACTDTTM"] = "20210112";
            row66["SHIFT_USER_NAME"] = "陈晗铃";
            row66["MAINT_PART_NAME"] = "";
            row66["MAINT_CNTT"] = "  ";
            row66["MAINT_CODE"] = "M0";
            row66["CMCDNAME"] = "수동 BM";
            row66["MAINT_STRT_DTTM"] = "01/12/2021 17:50:42";
            row66["MAINT_END_DTTM"] = "01/12/2021 17:50:51";
            row66["MAINT_STAT_CODE"] = "R";
            row66["MNT_FLAG_NAME"] = "수리완료";
            row66["INTERVAL_TIME"] = "0.000104166666666667";
            dt.Rows.Add(row66);
            DataRow row67 = dt.NewRow();
            row67["EQPTID"] = "E111010401";
            row67["EQPTNAME"] = "ESS 1호 01열 04연 01단 Box";
            row67["ACTDTTM"] = "20210112";
            row67["SHIFT_USER_NAME"] = "陈晗铃";
            row67["MAINT_PART_NAME"] = "";
            row67["MAINT_CNTT"] = "   ";
            row67["MAINT_CODE"] = "M0";
            row67["CMCDNAME"] = "수동 BM";
            row67["MAINT_STRT_DTTM"] = "01/12/2021 17:57:23";
            row67["MAINT_END_DTTM"] = "01/12/2021 17:57:49";
            row67["MAINT_STAT_CODE"] = "R";
            row67["MNT_FLAG_NAME"] = "수리완료";
            row67["INTERVAL_TIME"] = "0.000300925925925926";
            dt.Rows.Add(row67);
            DataRow row68 = dt.NewRow();
            row68["EQPTID"] = "E111010402";
            row68["EQPTNAME"] = "ESS 1호 01열 04연 02단 Box";
            row68["ACTDTTM"] = "20210112";
            row68["SHIFT_USER_NAME"] = "陈晗铃";
            row68["MAINT_PART_NAME"] = "";
            row68["MAINT_CNTT"] = "  ";
            row68["MAINT_CODE"] = "M0";
            row68["CMCDNAME"] = "수동 BM";
            row68["MAINT_STRT_DTTM"] = "01/12/2021 17:57:14";
            row68["MAINT_END_DTTM"] = "01/12/2021 17:57:41";
            row68["MAINT_STAT_CODE"] = "R";
            row68["MNT_FLAG_NAME"] = "수리완료";
            row68["INTERVAL_TIME"] = "0.0003125";
            dt.Rows.Add(row68);
            DataRow row69 = dt.NewRow();
            row69["EQPTID"] = "E111010403";
            row69["EQPTNAME"] = "ESS 1호 01열 04연 03단 Box";
            row69["ACTDTTM"] = "20210112";
            row69["SHIFT_USER_NAME"] = "陈晗铃";
            row69["MAINT_PART_NAME"] = "";
            row69["MAINT_CNTT"] = "  ";
            row69["MAINT_CODE"] = "M0";
            row69["CMCDNAME"] = "수동 BM";
            row69["MAINT_STRT_DTTM"] = "01/12/2021 17:57:06";
            row69["MAINT_END_DTTM"] = "01/12/2021 17:57:31";
            row69["MAINT_STAT_CODE"] = "R";
            row69["MNT_FLAG_NAME"] = "수리완료";
            row69["INTERVAL_TIME"] = "0.000289351851851852";
            dt.Rows.Add(row69);
            DataRow row70 = dt.NewRow();
            row70["EQPTID"] = "E111010404";
            row70["EQPTNAME"] = "ESS 1호 01열 04연 04단 Box";
            row70["ACTDTTM"] = "20210112";
            row70["SHIFT_USER_NAME"] = "陈晗铃";
            row70["MAINT_PART_NAME"] = "";
            row70["MAINT_CNTT"] = "  ";
            row70["MAINT_CODE"] = "M0";
            row70["CMCDNAME"] = "수동 BM";
            row70["MAINT_STRT_DTTM"] = "04/29/2020 15:35:27";
            row70["MAINT_END_DTTM"] = "01/12/2021 17:56:57";
            row70["MAINT_STAT_CODE"] = "R";
            row70["MNT_FLAG_NAME"] = "수리완료";
            row70["INTERVAL_TIME"] = "258.098263888889";
            dt.Rows.Add(row70);
            DataRow row71 = dt.NewRow();
            row71["EQPTID"] = "E111010405";
            row71["EQPTNAME"] = "ESS 1호 01열 04연 05단 Box";
            row71["ACTDTTM"] = "20210112";
            row71["SHIFT_USER_NAME"] = "陈晗铃";
            row71["MAINT_PART_NAME"] = "";
            row71["MAINT_CNTT"] = "  ";
            row71["MAINT_CODE"] = "M0";
            row71["CMCDNAME"] = "수동 BM";
            row71["MAINT_STRT_DTTM"] = "04/29/2020 15:35:46";
            row71["MAINT_END_DTTM"] = "01/12/2021 17:56:47";
            row71["MAINT_STAT_CODE"] = "R";
            row71["MNT_FLAG_NAME"] = "수리완료";
            row71["INTERVAL_TIME"] = "258.097928240741";
            dt.Rows.Add(row71);
            DataRow row72 = dt.NewRow();
            row72["EQPTID"] = "E111010406";
            row72["EQPTNAME"] = "ESS 1호 01열 04연 06단 Box";
            row72["ACTDTTM"] = "20210112";
            row72["SHIFT_USER_NAME"] = "陈晗铃";
            row72["MAINT_PART_NAME"] = "";
            row72["MAINT_CNTT"] = "  ";
            row72["MAINT_CODE"] = "M0";
            row72["CMCDNAME"] = "수동 BM";
            row72["MAINT_STRT_DTTM"] = "04/16/2020 18:05:59";
            row72["MAINT_END_DTTM"] = "01/12/2021 17:56:38";
            row72["MAINT_STAT_CODE"] = "R";
            row72["MNT_FLAG_NAME"] = "수리완료";
            row72["INTERVAL_TIME"] = "270.993506944444";
            dt.Rows.Add(row72);
            DataRow row73 = dt.NewRow();
            row73["EQPTID"] = "E111010407";
            row73["EQPTNAME"] = "ESS 1호 01열 04연 07단 Box";
            row73["ACTDTTM"] = "20210112";
            row73["SHIFT_USER_NAME"] = "陈晗铃";
            row73["MAINT_PART_NAME"] = "";
            row73["MAINT_CNTT"] = "  ";
            row73["MAINT_CODE"] = "M0";
            row73["CMCDNAME"] = "수동 BM";
            row73["MAINT_STRT_DTTM"] = "01/12/2021 17:56:24";
            row73["MAINT_END_DTTM"] = "01/12/2021 17:56:31";
            row73["MAINT_STAT_CODE"] = "R";
            row73["MNT_FLAG_NAME"] = "수리완료";
            row73["INTERVAL_TIME"] = "0.0000810185185185185";
            dt.Rows.Add(row73);
            DataRow row74 = dt.NewRow();
            row74["EQPTID"] = "E111010408";
            row74["EQPTNAME"] = "ESS 1호 01열 04연 08단 Box";
            row74["ACTDTTM"] = "20210112";
            row74["SHIFT_USER_NAME"] = "陈晗铃";
            row74["MAINT_PART_NAME"] = "";
            row74["MAINT_CNTT"] = "   ";
            row74["MAINT_CODE"] = "M0";
            row74["CMCDNAME"] = "수동 BM";
            row74["MAINT_STRT_DTTM"] = "01/12/2021 17:52:41";
            row74["MAINT_END_DTTM"] = "01/12/2021 17:52:49";
            row74["MAINT_STAT_CODE"] = "R";
            row74["MNT_FLAG_NAME"] = "수리완료";
            row74["INTERVAL_TIME"] = "0.0000925925925925926";
            dt.Rows.Add(row74);
            DataRow row75 = dt.NewRow();
            row75["EQPTID"] = "E111010501";
            row75["EQPTNAME"] = "ESS 1호 01열 05연 01단 Box";
            row75["ACTDTTM"] = "20210112";
            row75["SHIFT_USER_NAME"] = "陈晗铃";
            row75["MAINT_PART_NAME"] = "";
            row75["MAINT_CNTT"] = "   ";
            row75["MAINT_CODE"] = "M0";
            row75["CMCDNAME"] = "수동 BM";
            row75["MAINT_STRT_DTTM"] = "01/12/2021 17:58:52";
            row75["MAINT_END_DTTM"] = "01/12/2021 17:59:29";
            row75["MAINT_STAT_CODE"] = "R";
            row75["MNT_FLAG_NAME"] = "수리완료";
            row75["INTERVAL_TIME"] = "0.000428240740740741";
            dt.Rows.Add(row75);
            DataRow row76 = dt.NewRow();
            row76["EQPTID"] = "E111010502";
            row76["EQPTNAME"] = "ESS 1호 01열 05연 02단 Box";
            row76["ACTDTTM"] = "20210112";
            row76["SHIFT_USER_NAME"] = "陈晗铃";
            row76["MAINT_PART_NAME"] = "";
            row76["MAINT_CNTT"] = "   ";
            row76["MAINT_CODE"] = "M0";
            row76["CMCDNAME"] = "수동 BM";
            row76["MAINT_STRT_DTTM"] = "01/12/2021 17:58:44";
            row76["MAINT_END_DTTM"] = "01/12/2021 17:59:11";
            row76["MAINT_STAT_CODE"] = "R";
            row76["MNT_FLAG_NAME"] = "수리완료";
            row76["INTERVAL_TIME"] = "0.0003125";
            dt.Rows.Add(row76);
            DataRow row77 = dt.NewRow();
            row77["EQPTID"] = "E111010503";
            row77["EQPTNAME"] = "ESS 1호 01열 05연 03단 Box";
            row77["ACTDTTM"] = "20210112";
            row77["SHIFT_USER_NAME"] = "陈晗铃";
            row77["MAINT_PART_NAME"] = "";
            row77["MAINT_CNTT"] = "  ";
            row77["MAINT_CODE"] = "M0";
            row77["CMCDNAME"] = "수동 BM";
            row77["MAINT_STRT_DTTM"] = "04/30/2020 11:18:36";
            row77["MAINT_END_DTTM"] = "01/12/2021 17:58:35";
            row77["MAINT_STAT_CODE"] = "R";
            row77["MNT_FLAG_NAME"] = "수리완료";
            row77["INTERVAL_TIME"] = "257.277766203704";
            dt.Rows.Add(row77);
            DataRow row78 = dt.NewRow();
            row78["EQPTID"] = "E111010504";
            row78["EQPTNAME"] = "ESS 1호 01열 05연 04단 Box";
            row78["ACTDTTM"] = "20210112";
            row78["SHIFT_USER_NAME"] = "陈晗铃";
            row78["MAINT_PART_NAME"] = "";
            row78["MAINT_CNTT"] = " ";
            row78["MAINT_CODE"] = "M3";
            row78["CMCDNAME"] = "수동 BM";
            row78["MAINT_STRT_DTTM"] = "04/30/2020 11:18:28";
            row78["MAINT_END_DTTM"] = "01/12/2021 17:58:29";
            row78["MAINT_STAT_CODE"] = "R";
            row78["MNT_FLAG_NAME"] = "수리완료";
            row78["INTERVAL_TIME"] = "257.277789351852";
            dt.Rows.Add(row78);
            DataRow row79 = dt.NewRow();
            row79["EQPTID"] = "E111010505";
            row79["EQPTNAME"] = "ESS 1호 01열 05연 05단 Box";
            row79["ACTDTTM"] = "20210112";
            row79["SHIFT_USER_NAME"] = "陈晗铃";
            row79["MAINT_PART_NAME"] = "";
            row79["MAINT_CNTT"] = "  ";
            row79["MAINT_CODE"] = "M0";
            row79["CMCDNAME"] = "수동 BM";
            row79["MAINT_STRT_DTTM"] = "04/30/2020 11:18:15";
            row79["MAINT_END_DTTM"] = "01/12/2021 17:58:21";
            row79["MAINT_STAT_CODE"] = "R";
            row79["MNT_FLAG_NAME"] = "수리완료";
            row79["INTERVAL_TIME"] = "257.277847222222";
            dt.Rows.Add(row79);
            DataRow row80 = dt.NewRow();
            row80["EQPTID"] = "E111010506";
            row80["EQPTNAME"] = "ESS 1호 01열 05연 06단 Box";
            row80["ACTDTTM"] = "20210112";
            row80["SHIFT_USER_NAME"] = "陈晗铃";
            row80["MAINT_PART_NAME"] = "";
            row80["MAINT_CNTT"] = "  ";
            row80["MAINT_CODE"] = "M0";
            row80["CMCDNAME"] = "수동 BM";
            row80["MAINT_STRT_DTTM"] = "04/30/2020 11:18:01";
            row80["MAINT_END_DTTM"] = "01/12/2021 17:58:13";
            row80["MAINT_STAT_CODE"] = "R";
            row80["MNT_FLAG_NAME"] = "수리완료";
            row80["INTERVAL_TIME"] = "257.277916666667";
            dt.Rows.Add(row80);
            DataRow row81 = dt.NewRow();
            row81["EQPTID"] = "E111010507";
            row81["EQPTNAME"] = "ESS 1호 01열 05연 07단 Box";
            row81["ACTDTTM"] = "20210112";
            row81["SHIFT_USER_NAME"] = "陈晗铃";
            row81["MAINT_PART_NAME"] = "";
            row81["MAINT_CNTT"] = "  ";
            row81["MAINT_CODE"] = "M0";
            row81["CMCDNAME"] = "수동 BM";
            row81["MAINT_STRT_DTTM"] = "04/30/2020 11:17:49";
            row81["MAINT_END_DTTM"] = "01/12/2021 17:58:07";
            row81["MAINT_STAT_CODE"] = "R";
            row81["MNT_FLAG_NAME"] = "수리완료";
            row81["INTERVAL_TIME"] = "257.277986111111";
            dt.Rows.Add(row81);
            DataRow row82 = dt.NewRow();
            row82["EQPTID"] = "E111010508";
            row82["EQPTNAME"] = "ESS 1호 01열 05연 08단 Box";
            row82["ACTDTTM"] = "20210112";
            row82["SHIFT_USER_NAME"] = "陈晗铃";
            row82["MAINT_PART_NAME"] = "";
            row82["MAINT_CNTT"] = "  ";
            row82["MAINT_CODE"] = "M0";
            row82["CMCDNAME"] = "수동 BM";
            row82["MAINT_STRT_DTTM"] = "04/30/2020 11:17:29";
            row82["MAINT_END_DTTM"] = "01/12/2021 17:57:59";
            row82["MAINT_STAT_CODE"] = "R";
            row82["MNT_FLAG_NAME"] = "수리완료";
            row82["INTERVAL_TIME"] = "257.278125";
            dt.Rows.Add(row82);
            DataRow row83 = dt.NewRow();
            row83["EQPTID"] = "E111010601";
            row83["EQPTNAME"] = "ESS 1호 01열 06연 01단 Box";
            row83["ACTDTTM"] = "20210111";
            row83["SHIFT_USER_NAME"] = "吴家伟";
            row83["MAINT_PART_NAME"] = "";
            row83["MAINT_CNTT"] = "....";
            row83["MAINT_CODE"] = "M0";
            row83["CMCDNAME"] = "수동 BM";
            row83["MAINT_STRT_DTTM"] = "01/11/2021 11:45:29";
            row83["MAINT_END_DTTM"] = "01/11/2021 11:45:45";
            row83["MAINT_STAT_CODE"] = "R";
            row83["MNT_FLAG_NAME"] = "수리완료";
            row83["INTERVAL_TIME"] = "0.000185185185185185";
            dt.Rows.Add(row83);
            DataRow row84 = dt.NewRow();
            row84["EQPTID"] = "E111010601";
            row84["EQPTNAME"] = "ESS 1호 01열 06연 01단 Box";
            row84["ACTDTTM"] = "20210126";
            row84["SHIFT_USER_NAME"] = "杨天恩";
            row84["MAINT_PART_NAME"] = "";
            row84["MAINT_CNTT"] = " ";
            row84["MAINT_CODE"] = "M0";
            row84["CMCDNAME"] = "수동 BM";
            row84["MAINT_STRT_DTTM"] = "01/26/2021 12:36:32";
            row84["MAINT_END_DTTM"] = "01/26/2021 15:13:16";
            row84["MAINT_STAT_CODE"] = "R";
            row84["MNT_FLAG_NAME"] = "수리완료";
            row84["INTERVAL_TIME"] = "0.108842592592593";
            dt.Rows.Add(row84);

            #endregion

        }


    }
}
