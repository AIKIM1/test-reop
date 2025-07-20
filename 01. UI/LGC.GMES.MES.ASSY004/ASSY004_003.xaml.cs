/*************************************************************************************
 Created Date : 2019.05.09
      Creator : INS 김동일K
   Decription : CWA3동 증설 - VD QA 대상LOT조회 화면 (ASSY0001.ASSY001_043 2019/05/09 소스 카피 후 작성)
--------------------------------------------------------------------------------------
 [Change History]
  2019.05.09  INS 김동일K : Initial Created.
  2019.06.10  CNS 김대근 : Edit For VD QA.
  2022.08.22  신광희     : 선택된 동에 따라 검사대상, 대상변경 컬럼 Visibility 속성 조회 추가 및 대상변경 버튼 컬럼 바인딩을 위한 DataTable 컬럼 추가
  2023.03.08  유재기     : Summary Data, 컬럼 순서 변경, 자동조회 기능 추가
  2023.03.21  유재기     : AUTO HOLD 기능 개선, Area 별 기능 적용
  2023.03.28  유재기     : 음극 양극 2줄 표시 및 LayOut 수정
  2023.08.08  유재기     : SMPL_CLCT_CMPL_FLAG 가 “Y” 인 항목 합계 제외 
  2024.08.19  안유수 E20240720-001675 조회 기능 사용 시 ASSY004_003_EQPTWIN 클래스 다시 로드 되도록 수정(사용자 컬럼 설정 적용)
  **************************************************************************************/

using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using C1.WPF;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.CMM001.Extensions;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;

namespace LGC.GMES.MES.ASSY004
{
    /// <summary>
    /// ASSY004_003.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class ASSY004_003 : UserControl, IWorkArea
    {
        #region [Declaration & Constructor]
        List<ASSY004_003_EQPTWIN> list;
        CommonCombo combo = new CommonCombo();
        Util _Util = new Util();

        int listCtn = -1;
        string QA_TYPE = "N";
        string floor = string.Empty;
        bool isRefresh;
        private string _LDR_LOT_IDENT_BAS_CODE = string.Empty;
        private string _UNLDR_LOT_IDENT_BAS_CODE = string.Empty;
        private bool _isInspectionTarget = false;
        private readonly System.Windows.Threading.DispatcherTimer _dispatcherMainTimer = new System.Windows.Threading.DispatcherTimer();
        private readonly System.Windows.Threading.DispatcherTimer _dispatcherRackRateTimer = new System.Windows.Threading.DispatcherTimer();
        private bool _isAutoSelectTime = false;
        private bool _isVdQaTrgtChgHoldArea = false;

        public IFrameOperation FrameOperation { get; set; }
        public bool REFRESH
        {
            get
            {
                return isRefresh;
            }
            set
            {
                isRefresh = value;

                if (isRefresh)
                    SearchData();
            }
        }

        public ASSY004_003()
        {
            InitializeComponent();
        }
        #endregion

        #region [Event]
        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            Loaded -= UserControl_Loaded;
            ApplyPermissions();
            initcombo();

            SetFloorRadio();

            txtAnodeQty.Text = "0";
            txtCathodeQty.Text = "0";
            txtJudgeAnodeQty.Text = "0";
            txtJudgeCathodeQty.Text = "0";

            txtAnodeQty.IsReadOnly = true;
            txtAnodeQty.IsReadOnlyCaretVisible = true;
            txtCathodeQty.IsReadOnly = true;
            txtJudgeAnodeQty.IsReadOnly = true;
            txtJudgeCathodeQty.IsReadOnly = true;
            
            SetSummaryLayout();

            list = new List<ASSY004_003_EQPTWIN>();
            for (int i = 0; i < 10; i++)
            {
                list.Add(new ASSY004_003_EQPTWIN(this));
            }
            listCtn = 10;

            try
            {
                DataTable dt = new DataTable();
                //dt.Columns.Add("SHOPID", typeof(string));
                dt.Columns.Add("AREAID", typeof(string));

                DataRow dr = dt.NewRow();
                //dr["SHOPID"] = LoginInfo.CFG_SHOP_ID;
                dr["AREAID"] = Util.NVC(cboVDArea.SelectedValue);
                dt.Rows.Add(dr);

                //Shop별로 QA자동판단여부가 다르다.
                //DataTable result = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_SHOPATTR", "RQSTDT", "RSLTDT", dt);
                DataTable result = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_MMD_AREA_VD_QA_AUTO_FLAG", "RQSTDT", "RSLTDT", dt);
                if (result.Rows.Count != 0)
                    //QA_TYPE = Convert.ToString(result.Rows[0]["VD_QA_AUTO_JUDG_FLAG"]);
                    QA_TYPE = Convert.ToString(result.Rows[0]["AUTO_VD_FLAG"]);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }

            if (_dispatcherMainTimer != null)
            {
                int second = 0;

                if (cboAutoSearch?.SelectedValue != null && !string.IsNullOrEmpty(cboAutoSearch.SelectedValue.GetString()))
                    second = int.Parse(cboAutoSearch.SelectedValue.ToString());

                _dispatcherMainTimer.Tick -= DispatcherMainTimer_Tick;
                _dispatcherMainTimer.Tick += DispatcherMainTimer_Tick;
                _dispatcherMainTimer.Interval = new TimeSpan(0, 0, second);
            }
        }

        public void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            if (sender == null)
                return;

            SearchData();
        }

        private void chkLOTQA_Checked(object sender, RoutedEventArgs e)
        {
            if (list == null) return;
            if (list.Count == 0) return;

            int iCnt = list.Count();
            for (int i = 0; i < list.Count; i++)
            {
                list[i].FrameOperation = FrameOperation;
                object[] parameters = new object[7];
                parameters[0] = chkLOTQA.IsChecked.Value;
                C1WindowExtension.SetParameters(list[i], parameters);
                list[i].SetLotQAInsp();
            }
        }

        private void rbFloor_Checked(object sender, RoutedEventArgs e)
        {
            floor = (sender as RadioButton).Tag as string;
        }

        private void cboArea_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            DataTable dt = new DataTable();
            dt.Columns.Add("CMCDTYPE", typeof(string));
            dt.Columns.Add("CBO_CODE", typeof(string));

            DataRow dr = dt.NewRow();
            dr["CMCDTYPE"] = "QA_SMPL_PRDT_REQ";
            dr["CBO_CODE"] = cboVDArea.SelectedValue;
            dt.Rows.Add(dr);

            DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_COMMONCODE_ATTRIBUTE", "RQSTDT", "RSLTDT", dt);

            if (CommonVerify.HasTableRow(dtResult))
            {
                _isInspectionTarget = dtResult.Rows[0]["ATTRIBUTE2"].GetString().Equals("Y") ? true : false;
            }
            else
            {
                _isInspectionTarget = false;
            }
        }

        private void cboAutoSearch_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            try
            {
                if (_dispatcherMainTimer != null)
                {
                    _dispatcherMainTimer.Stop();

                    int iSec = 0;

                    if (cboAutoSearch?.SelectedValue != null && !cboAutoSearch.SelectedValue.ToString().Equals(""))
                        iSec = int.Parse(cboAutoSearch.SelectedValue.ToString());

                    if (iSec == 0 && _isAutoSelectTime)
                    {
                        _dispatcherMainTimer.Interval = new TimeSpan(0, 0, iSec);
                        // 자동조회가 사용하지 않도록 변경 되었습니다.
                        Util.MessageValidation("SFU8170");
                        return;
                    }

                    if (iSec == 0)
                    {
                        _isAutoSelectTime = true;
                        return;
                    }

                    _dispatcherMainTimer.Interval = new TimeSpan(0, 0, iSec);
                    _dispatcherMainTimer.Start();

                    if (_isAutoSelectTime)
                    {
                        // 자동조회  %1초로 변경 되었습니다.
                        Util.MessageValidation("SFU5127", cboAutoSearch?.SelectedValue?.ToString());
                    }

                    _isAutoSelectTime = true;
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void DispatcherMainTimer_Tick(object sender, EventArgs e)
        {
            if (sender == null) return;

            System.Windows.Threading.DispatcherTimer dpcTmr = sender as System.Windows.Threading.DispatcherTimer;
            dpcTmr?.Dispatcher.BeginInvoke(new Action(() =>
            {
                try
                {
                    dpcTmr.Stop();

                    // 0분이면 skip
                    if (dpcTmr.Interval.TotalSeconds.Equals(0)) return;

                    // 설비 Tree 조회
                    SearchData();
                }
                catch (Exception ex)
                {
                    Util.MessageException(ex);
                }
                finally
                {
                    if (dpcTmr.Interval.TotalSeconds > 0)
                        dpcTmr.Start();
                }
            }));
        }

        private void cboEquipmentElec_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            if (_isVdQaTrgtChgHoldArea)
            {
                if (Convert.ToString(cboEquipmentElec.SelectedValue).IsNullOrEmpty())
                {
                    rdoThree.Visibility = Visibility.Collapsed;
                    rdoTwo.IsChecked = true;
                }
                else
                {
                    rdoThree.Visibility = Visibility.Visible;
                }
            }
        }

        #endregion

        #region[Method]
        private void SearchData()
        {
            try
            {
                ShowLoadIndicator();

                GetLotIdentBasCode();

                SetSummaryLayout();

                list = null;
                GC.Collect();
                list = new List<ASSY004_003_EQPTWIN>();
                for (int i = 0; i < 10; i++)
                {
                    list.Add(new ASSY004_003_EQPTWIN(this));
                }
                listCtn = 10;


                DataTable result = SetVDFinish();

                DataTable data = new DataTable();
                data.Columns.Add("LANGID", typeof(string));
                data.Columns.Add("EQSGID", typeof(string));
                data.Columns.Add("ELEC", typeof(string));
                data.Columns.Add("FLOOR", typeof(string));
                data.Columns.Add("AREAID", typeof(string));

                DataRow row = data.NewRow();
                row["LANGID"] = LoginInfo.LANGID;
                row["EQSGID"] = string.IsNullOrEmpty(Convert.ToString(cboVDEquipmentSegment.SelectedValue).Trim()) ? null : Convert.ToString(cboVDEquipmentSegment.SelectedValue);
                row["ELEC"] = Convert.ToString(cboEquipmentElec.SelectedValue);
                row["FLOOR"] = floor.Equals("ALL") ? null : floor;
                row["AREAID"] = Convert.ToString(cboVDArea.SelectedValue);
                data.Rows.Add(row);

                new ClientProxy().ExecuteService("DA_PRD_SEL_EQPT_FOR_QA_L", "RQST", "RSLT", data, (bizResult, bizException) =>
                {
                    if (listCtn < bizResult.Rows.Count)
                    {
                        for (int i = 0; i < bizResult.Rows.Count - listCtn; i++)
                            list.Add(new ASSY004_003_EQPTWIN(this));

                        listCtn = list.Count();
                    }

                    ClearData();

                    if (bizException != null)
                    {
                        Util.MessageException(bizException);
                        return;
                    }

                    if (bizResult.Rows.Count == 0)
                    {
                        //조회된 Data가 없습니다.
                        FrameOperation.PrintFrameMessage("[" + System.DateTime.Now.ToString("hh:mm:ss") + "]" + MessageDic.Instance.GetMessage("SFU1905"));
                        return;
                    }

                    //bizResult : VD 설비정보 Table
                    //result : VD_QA 대기 WIP Table
                    //for문 : 하나의 VD 설비에서 나온 WIP들 중 VD_QA 대상인 친구들을 EQPTID를 기준으로 가져옵니다.
                    for (int i = 0; i < bizResult.Rows.Count; i++)
                    {
                        Util.gridClear(list[i].dgFinishLot);
                        DataTable resultByEqpt = !result.Select("EQPTID = '" + bizResult.Rows[i]["EQPTID"] + "'").Any() ? null : result.Select("EQPTID = '" + bizResult.Rows[i]["EQPTID"] + "'").CopyToDataTable();

                        list[i]._LDR_LOT_IDENT_BAS_CODE = _LDR_LOT_IDENT_BAS_CODE;
                        list[i]._UNLDR_LOT_IDENT_BAS_CODE = _UNLDR_LOT_IDENT_BAS_CODE;

                        //VD_QA 검사 조건 코드 : 대LOT단위
                        if (bizResult.Rows[i]["VD_QA_INSP_COND_CODE"].Equals("VD_QA_INSP_RULE_02"))
                        {
                            list[i].dgcLotId.Visibility = Visibility.Collapsed;
                            list[i].dgcLotCount.Visibility = Visibility.Visible;
                            //tmp : EQPTID에 속하는 WIP들만 추출한 Table
                            if (resultByEqpt != null)
                            {
                                //LOTID_RT, 즉 대LOT을 기준으로 GROUPBY한다.
                                var resultByGroup = resultByEqpt.AsEnumerable().GroupBy(x => new
                                {
                                    EQPT_BTCH_WRK_NO = x.Field<string>("EQPT_BTCH_WRK_NO"),
                                    LOTID_RT = x.Field<string>("LOTID_RT"),
                                    JUDG_VALUE = x.Field<string>("JUDG_VALUE")
                                }).Select(g => new
                                {
                                    EQPT_BTCH_WRK_NO = g.Key.EQPT_BTCH_WRK_NO,
                                    LOTID_RT = g.Key.LOTID_RT,
                                    JUDG_VALUE = g.Key.JUDG_VALUE,
                                    CHK = g.Max(x => x.Field<bool>("CHK")),
                                    LOTID = g.Max(x => x.Field<string>("LOTID")),
                                    CSTID = g.Max(x => x.Field<string>("CSTID")),
                                    EQPTID = g.Max(x => x.Field<string>("EQPTID")),
                                    PRODID = g.Max(x => x.Field<string>("PRODID")),
                                    PRJT_NAME = g.Max(x => x.Field<string>("PRJT_NAME")),
                                    JUDG_NAME = g.Max(x => x.Field<string>("JUDG_NAME")),
                                    WIPDTTM_ED = g.Max(x => x.Field<string>("WIPDTTM_ED")),
                                    ELEC = g.Max(x => x.Field<string>("ELEC")),
                                    VD_QA_INSP_COND_CODE = g.Max(x => x.Field<string>("VD_QA_INSP_COND_CODE")),
                                    REWORKCNT = g.Max(x => x.Field<decimal>("REWORKCNT")),
                                    COUNT = g.Count(),
                                    QA_SMPL_DTTM = g.Max(x => x.Field<string>("QA_SMPL_DTTM")),
                                    EQGRID = g.Max(x => x.Field<string>("EQGRID")),
                                    QA_INSP_TRGT_FLAG = g.Max(x => x.Field<string>("QA_INSP_TRGT_FLAG")),
                                });

                                DataTable dt = new DataTable();
                                dt.Columns.Add("CHK", typeof(bool));
                                dt.Columns.Add("LOTID", typeof(string));
                                dt.Columns.Add("CSTID", typeof(string));
                                dt.Columns.Add("LOTID_RT", typeof(string));
                                dt.Columns.Add("JUDG_VALUE", typeof(string));
                                dt.Columns.Add("JUDG_NAME", typeof(string));
                                dt.Columns.Add("EQPT_BTCH_WRK_NO", typeof(string));
                                dt.Columns.Add("WIPDTTM_ED", typeof(string));
                                dt.Columns.Add("PRODID", typeof(string));
                                dt.Columns.Add("PRJT_NAME", typeof(string));
                                dt.Columns.Add("ELEC", typeof(string));
                                dt.Columns.Add("VD_QA_INSP_COND_CODE", typeof(string));
                                dt.Columns.Add("REWORKCNT", typeof(decimal));
                                dt.Columns.Add("COUNT", typeof(decimal));
                                dt.Columns.Add("QA_SMPL_DTTM", typeof(string));
                                dt.Columns.Add("EQGRID", typeof(string));
                                dt.Columns.Add("QA_INSP_TRGT_FLAG", typeof(string));
                                dt.Columns.Add("TARGET_VISIVILITY", typeof(string));

                                DataRow dtRow = null;
                                foreach (var r in resultByGroup)
                                {
                                    dtRow = dt.NewRow();
                                    dtRow["CHK"] = r.CHK;
                                    dtRow["LOTID"] = r.LOTID;
                                    dtRow["CSTID"] = r.CSTID;
                                    dtRow["LOTID_RT"] = r.LOTID_RT;
                                    dtRow["JUDG_VALUE"] = r.JUDG_VALUE;
                                    dtRow["JUDG_NAME"] = r.JUDG_NAME;
                                    dtRow["EQPT_BTCH_WRK_NO"] = r.EQPT_BTCH_WRK_NO;
                                    dtRow["WIPDTTM_ED"] = r.WIPDTTM_ED;
                                    dtRow["PRODID"] = r.PRODID;
                                    dtRow["PRJT_NAME"] = r.PRJT_NAME;
                                    dtRow["ELEC"] = r.ELEC;
                                    dtRow["VD_QA_INSP_COND_CODE"] = r.VD_QA_INSP_COND_CODE;
                                    dtRow["REWORKCNT"] = r.REWORKCNT;
                                    dtRow["COUNT"] = r.COUNT;
                                    dtRow["QA_SMPL_DTTM"] = r.QA_SMPL_DTTM;
                                    dtRow["EQGRID"] = r.EQGRID;
                                    dtRow["QA_INSP_TRGT_FLAG"] = r.QA_INSP_TRGT_FLAG;
                                    dtRow["TARGET_VISIVILITY"] = !r.QA_INSP_TRGT_FLAG.GetString().Equals("Y") ? "Visible" : "Collapsed";
                                    dt.Rows.Add(dtRow);
                                }

                                Util.GridSetData(list[i].dgFinishLot, dt, FrameOperation, true);
                            }
                        }
                        else //대LOT단위 아니면
                        {
                            if (resultByEqpt != null)
                                Util.GridSetData(list[i].dgFinishLot, resultByEqpt, FrameOperation, true);
                        }
                        //각 서브창들에게 bizResult에 담긴 EQPT정보 전달
                        GetEqpt(i, bizResult);
                    }

                    //각 서브창들 데이터 세팅 후 서브창들의 디자인 세팅
                    //각 서브창들 띄워줌
                    SetEqptWindow(bizResult, rdoTwo.IsChecked == true ? 2 : 3);
                    FrameOperation.PrintFrameMessage(bizResult.Rows.Count + MessageDic.Instance.GetMessage("건"));

                    // Summary Data 를 보여줌
                    SetSummary(result);
                });
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
            finally
            {
                HideLoadIndicator();
            }
        }

        private void SetSummary(DataTable dtResult)
        {
            txtAnodeQty.Text = "0";
            txtCathodeQty.Text = "0";
            txtJudgeAnodeQty.Text = "0";
            txtJudgeCathodeQty.Text = "0";

            if (dtResult != null && dtResult.Rows.Count > 0)
            {
                DataTable dtJudgeAnode = null;
                DataTable dtJudgeCathode = null;

                try
                {
                    dtJudgeAnode = dtResult.Select("ELEC LIKE '*" + ObjectDic.Instance.GetObjectName("음극") + "*' AND QA_INSP_TRGT_FLAG = 'Y' AND (SMPL_CLCT_CMPL_FLAG IS NULL OR SMPL_CLCT_CMPL_FLAG = '' OR SMPL_CLCT_CMPL_FLAG = 'N') ").CopyToDataTable();
                }
                catch
                {
                    dtJudgeAnode = null;
                }

                if (dtJudgeAnode != null && dtJudgeAnode.Rows.Count > 0)
                {
                    txtJudgeAnodeQty.Text = dtJudgeAnode.Rows.Count.ToString();
                }


                try
                {
                    dtJudgeCathode = dtResult.Select("ELEC LIKE '*" + ObjectDic.Instance.GetObjectName("양극") + "*' AND QA_INSP_TRGT_FLAG = 'Y' AND (SMPL_CLCT_CMPL_FLAG IS NULL OR SMPL_CLCT_CMPL_FLAG = '' OR SMPL_CLCT_CMPL_FLAG = 'N') ").CopyToDataTable();
                }
                catch {
                    dtJudgeCathode = null;
                }

                if (dtJudgeCathode != null && dtJudgeCathode.Rows.Count > 0)
                {
                    txtJudgeCathodeQty.Text = dtJudgeCathode.Rows.Count.ToString();
                }
            }

            if (_isVdQaTrgtChgHoldArea)
            {
                const string bizRuleName = "BR_MHS_SEL_WAREHOUSE_SUMMARY_BY_ASSY_WO";
                try
                {
                    DataTable inTable = new DataTable("RQSTDT");
                    inTable.Columns.Add("LANGID", typeof(string));
                    inTable.Columns.Add("AREAID", typeof(string));

                    DataRow dr = inTable.NewRow();
                    dr["LANGID"] = LoginInfo.LANGID;
                    dr["AREAID"] = LoginInfo.CFG_AREA_ID;
                    inTable.Rows.Add(dr);

                    new ClientProxy().ExecuteService(bizRuleName, "RQSTDT", "RSLTDT", inTable, (bizResult, bizException) =>
                    {
                        if (bizException != null)
                        {
                            Util.MessageException(bizException);
                            return;
                        }

                        if (bizResult != null && bizResult.Rows.Count > 0)
                        {
                            try
                            {
                                DataView view = bizResult.DefaultView;

                                DataTable dtAnodeLamiStocker = view.ToTable(true, new string[] { "PRJT_NAME", "AN_QTY_AF_NT_LWW" });
                                DataTable dtCathodeLamiStocker = view.ToTable(true, new string[] { "PRJT_NAME", "CA_QTY_AF_NT_LWW" });

                                int iLamiStkCnt = 0;

                                for (int i = 0; i < dtAnodeLamiStocker.Rows.Count; i++)
                                {
                                    iLamiStkCnt += dtAnodeLamiStocker.Rows[i]["AN_QTY_AF_NT_LWW"].SafeToInt32();
                                }

                                txtAnodeQty.Text = iLamiStkCnt.ToString();


                                iLamiStkCnt = 0;

                                for (int i = 0; i < dtCathodeLamiStocker.Rows.Count; i++)
                                {
                                    iLamiStkCnt += dtCathodeLamiStocker.Rows[i]["CA_QTY_AF_NT_LWW"].SafeToInt32();
                                }
                                txtCathodeQty.Text = iLamiStkCnt.ToString();
                            } catch(Exception ex)
                            {
                                txtAnodeQty.Text = "";
                                txtCathodeQty.Text = "";
                            }

                            //txtAnodeQty.Text = bizResult.Rows[0]["AN_QTY_AF_NT_LWW"].ToString();
                            //txtCathodeQty.Text = bizResult.Rows[0]["CA_QTY_AF_NT_LWW"].ToString();
                        }
                    });
                }
                catch (Exception ex)
                {
                    Util.MessageException(ex);
                }
            }

        }

        private void GetEqpt(int i, DataTable dt)
        {
            list[i].FrameOperation = FrameOperation;

            object[] parameters = new object[10];
            parameters[0] = true;
            parameters[1] = dt.Rows[i]["PRDT_CLSS_CHK_FLAG"].GetString();
            parameters[2] = dt.Rows[i]["PRDT_CLSS_CODE"].GetString();
            parameters[3] = dt.Rows[i]["EQPTNAME"].GetString();
            parameters[4] = dt.Rows[i]["EQPTID"].GetString();
            parameters[5] = cboVDEquipmentSegment.SelectedValue.GetString();
            parameters[6] = dt.Rows[i]["VD_QA_INSP_COND_CODE"].GetString();
            parameters[7] = QA_TYPE; //QMS 인터페이스 여부
            parameters[8] = _isInspectionTarget; //검사대상, 대상변경 컬럼 Visibility 지정
            parameters[9] = Util.NVC(cboVDArea.SelectedValue);

            C1WindowExtension.SetParameters(list[i], parameters);

            list[i].SetValue();
        }

        private DataTable SetVDFinish()
        {
            try
            {
                DataSet data = new DataSet();
                DataTable dt = data.Tables.Add("INDATA");

                dt.Columns.Add("LANGID", typeof(string));
                dt.Columns.Add("SHOPID", typeof(string));
                dt.Columns.Add("AREAID", typeof(string));
                dt.Columns.Add("EQSGID", typeof(string));

                DataRow row = dt.NewRow();
                row["LANGID"] = LoginInfo.LANGID;
                row["SHOPID"] = LoginInfo.CFG_SHOP_ID;
                row["AREAID"] = Util.NVC(cboVDArea.SelectedValue);
                row["EQSGID"] = string.IsNullOrEmpty(Util.NVC(cboVDEquipmentSegment.SelectedValue)) ? null : Util.NVC(cboVDEquipmentSegment.SelectedValue);
                dt.Rows.Add(row);

                //string xml = data.GetXml();

                DataSet ds = new ClientProxy().ExecuteServiceSync_Multi("BR_PRD_GET_QA_TARGET_LIST_FOR_UI_L", "RQSTDT", "OUTDATA", data);
                DataTable result = ds.Tables["OUTDATA"];



                var dtBinding = result.Copy();
                dtBinding.Columns.Add(new DataColumn() { ColumnName = "TARGET_VISIVILITY", DataType = typeof(string) });

                foreach (DataRow dr in dtBinding.Rows)
                {
                    dr["TARGET_VISIVILITY"] = !dr["QA_INSP_TRGT_FLAG"].GetString().Equals("Y") ? "Visible" : "Collapsed";

                }

                if (dtBinding != null && dtBinding.Rows.Count > 0)
                {
                    string sBatchId = "";

                    for (int j = 0; j < dtBinding.Rows.Count; j++)
                    {
                        string sTempBatchId = dtBinding.Rows[j].GetValue("EQPT_BTCH_WRK_NO").ToString();

                        if (sBatchId.Equals(sTempBatchId))
                        {
                            continue;
                        } else {
                            sBatchId = sTempBatchId;
                        }

                        for (int k = j; k < dtBinding.Rows.Count; k++)
                        {
                            string sEqptBtchWrkNo = dtBinding.Rows[k].GetValue("EQPT_BTCH_WRK_NO").ToString();
                            string sQaInspTrgtFlag = dtBinding.Rows[k].GetValue("QA_INSP_TRGT_FLAG").ToString();

                            if (sEqptBtchWrkNo.Equals(sBatchId))
                            {
                                DataTable dtCheckTarget = null;
                                try
                                {
                                    dtCheckTarget = dtBinding.Select(String.Format("EQPT_BTCH_WRK_NO = '{0}' AND QA_INSP_TRGT_FLAG = 'Y' ", sEqptBtchWrkNo)).CopyToDataTable();
                                } catch (Exception ex)
                                {
                                    //null
                                }
                                

                                if(dtCheckTarget == null || dtCheckTarget.Rows.Count == 0)
                                {
                                    dtBinding.Rows[k]["TARGET_VISIVILITY"] = "Visible";
                                }
                                else
                                {
                                    if (sQaInspTrgtFlag.Equals("Y"))
                                    {
                                        break;
                                    }
                                    else
                                    {
                                        dtBinding.Rows[k]["TARGET_VISIVILITY"] = "Collapsed";
                                    }
                                }
                                
                            }
                            else
                            {
                                break;
                            }
                        }
                    }
                }

                dtBinding.AcceptChanges();
                return dtBinding;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return null;
            }
        }

        private void SetEqptWindow(DataTable bizResult, int rowCount)
        {
            int num = 0;

            if (_isVdQaTrgtChgHoldArea && Convert.ToString(cboEquipmentElec.SelectedValue).IsNullOrEmpty())
            {
                grdEqpt.RowDefinitions.Add(new RowDefinition { Height = new GridLength(320) });
                grdEqpt.RowDefinitions.Add(new RowDefinition { Height = new GridLength(320) });
                
                int iColNum1 = 0;
                int iColNum2 = 0;

                for (int i =0; i< bizResult.Rows.Count; i++)
                {
                    string sRowNum = bizResult.Rows[i].GetValue("RN").ToString();

                    var grid = new Grid
                    {
                        Name = "gr0" + i.ToString(),
                        Margin = sRowNum.Equals("0") ? new Thickness(0, 8, 8, 8) : new Thickness(0, 0, 8, 8)
                    };

                    grid.SetValue(Grid.RowProperty, int.Parse(sRowNum));

                    if (sRowNum.Equals("0"))
                    {
                        var colDef = new ColumnDefinition
                        {
                            MinWidth = 400,
                            Width = new GridLength(500)
                        };
                        
                        grdEqpt.ColumnDefinitions.Add(colDef);

                        grid.SetValue(Grid.ColumnProperty, iColNum1);
                    }
                    else
                    {
                        if(iColNum2 > iColNum1)
                        {
                            var colDef = new ColumnDefinition
                            {
                                MinWidth = 400,
                                Width = new GridLength(500)
                            };

                            grdEqpt.ColumnDefinitions.Add(colDef);
                        }
                        grid.SetValue(Grid.ColumnProperty, iColNum2);
                    }

                    grid.Children.Add(list[i]);

                    //큰 창에 추가
                    grdEqpt.Children.Add(grid);

                    if (sRowNum.Equals("0"))
                    {
                        iColNum1++;
                    } else
                    {
                        iColNum2++;
                    }
                }

            } else
            {
                for (int i = 0; i < rowCount; i++)
                {
                    var rowDef = new RowDefinition { Height = rowCount == 2 ? new GridLength(360) : new GridLength(250) };
                    grdEqpt.RowDefinitions.Add(rowDef);

                    for (int j = 0; j < Math.Ceiling((double)bizResult.Rows.Count / rowCount); j++)
                    {
                        if (i == 0)
                        {
                            var colDef = new ColumnDefinition
                            {
                                MinWidth = 400,
                                Width = rowCount == 2 ? new GridLength(500) : new GridLength(200)
                            };
                            grdEqpt.ColumnDefinitions.Add(colDef);
                        }

                        //grid안에 각각의 grid생성
                        var grid = new Grid
                        {
                            Name = "gr0" + num,
                            Margin = i == 0 ? new Thickness(0, 8, 8, 8) : new Thickness(0, 0, 8, 8)
                        };
                        grid.SetValue(Grid.RowProperty, i);
                        grid.SetValue(Grid.ColumnProperty, j);
                        grid.Children.Add(list[num]);

                        //큰 창에 추가
                        grdEqpt.Children.Add(grid);

                        num++;
                        if (bizResult.Rows.Count == num)
                        {
                            return;
                        }
                    }
                }
            }
        }

        private void SetAutoSearchCombo(C1ComboBox cbo)
        {
            const string bizRuleName = "DA_BAS_SEL_COMMCODE_CBO";
            string[] arrColumn = { "LANGID", "CMCDTYPE" };
            string[] arrCondition = { LoginInfo.LANGID, "DRB_REFRESH_TERM" };
            string selectedValueText = cbo.SelectedValuePath;
            string displayMemberText = cbo.DisplayMemberPath;

            CommonCombo.CommonBaseCombo(bizRuleName, cbo, arrColumn, arrCondition, CommonCombo.ComboStatus.NA, selectedValueText, displayMemberText, string.Empty);
        }

        private bool UseCommoncodePlant(string sArea)
        {
            DataTable inTable = new DataTable();
            inTable.Columns.Add("LANGID", typeof(string));
            inTable.Columns.Add("CMCDTYPE", typeof(string));
            inTable.Columns.Add("CMCODE", typeof(string));

            DataRow dr = inTable.NewRow();
            dr["LANGID"] = LoginInfo.LANGID;
            dr["CMCDTYPE"] = "VD_QA_TRGT_CHG_AUTO_HOLD_AREA";

            if(sArea.IsNullOrEmpty())
            {
                dr["CMCODE"] = LoginInfo.CFG_AREA_ID;
            } else
            {
                dr["CMCODE"] = sArea;
            }
            

            inTable.Rows.Add(dr);

            DataTable dtRslt = null;
            try
            {
                dtRslt = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_COMMONCODE_TBL", "INDATA", "OUTDATA", inTable);
            } catch(Exception ex)
            {
                // null 처리
            }

            if (dtRslt != null && dtRslt.Rows.Count > 0)
            {
                return true;
            }
            else
            {
                return false;
            }

        }
        #endregion

        #region [Init & Util Method]
        private void initcombo()
        {
            SetAutoSearchCombo(cboAutoSearch);

            string[] sFilter = { null, LoginInfo.CFG_AREA_ID };
            C1ComboBox[] children = { cboVDEquipmentSegment };
            combo.SetCombo(cboVDArea, CommonCombo.ComboStatus.NONE, cbChild: children, sFilter: sFilter);

            string[] sFilter2 = { LoginInfo.CFG_AREA_ID };
            C1ComboBox[] parents = { cboVDArea };
            combo.SetCombo(cboVDEquipmentSegment, CommonCombo.ComboStatus.ALL, cbParent: parents, sFilter: sFilter2, sCase: "EQUIPMENTSEGMENT_PLANT");
            //cboVDEquipmentSegment.SelectedIndex = 0;

            string[] sFilter3 = { "ELEC_TYPE" };
            combo.SetCombo(cboEquipmentElec, CommonCombo.ComboStatus.ALL, sFilter: sFilter3, sCase: "COMMCODE");

            //동 정보 로그인 정보로 기본 선택
            foreach (DataRowView drv in cboVDArea.ItemsSource)
            {
                if (Util.NVC(drv["CBO_CODE"]).Equals(LoginInfo.CFG_AREA_ID))
                {
                    cboVDArea.SelectedValue = Util.NVC(drv["CBO_CODE"]);
                    return;
                }
            }
        }

        private void SetFloorRadio()
        {
            try
            {
                DataTable dt = new DataTable();
                dt.Columns.Add("SHOPID");

                DataRow row = dt.NewRow();
                row["SHOPID"] = LoginInfo.CFG_SHOP_ID;

                dt.Rows.Add(row);

                new ClientProxy().ExecuteService("DA_PRD_SEL_SHOP_NND_VD_FLOOR", "INDATA", "OUTDATA", dt, (result, except) =>
                {
                    foreach (DataRowView drv in result.DefaultView)
                    {
                        RadioButton rb = new RadioButton();
                        rb.Content = DataTableConverter.GetValue(drv, "SHOP_FLOOR") as string + ObjectDic.Instance.GetObjectName("층");
                        rb.GroupName = rbAll.GroupName;
                        rb.Style = rbAll.Style;
                        rb.Margin = rbAll.Margin;
                        rb.Name = $"rbo{DataTableConverter.GetValue(drv, "SHOP_FLOOR") as string}floor";
                        rb.Tag = DataTableConverter.GetValue(drv, "SHOP_FLOOR") as string;
                        rb.Checked += rbFloor_Checked;
                        floorStackPanel.Children.Add(rb);
                    }
                });
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void ClearData()
        {
            if (list == null) return;
            if (list.Count == 0) return;

            for (int i = 0; i < list.Count; i++)
            {
                list[i].ClearData();
            }

            for (int i = grdEqpt.Children.Count - 1; i >= 0; i--)
            {
                if (list.Count >= grdEqpt.Children.Count)
                {
                    ((Grid)(grdEqpt.Children[i])).Children.Remove(list[i]);
                }
                grdEqpt.Children.Remove(((Grid)grdEqpt.Children[i]));
            }

            grdEqpt.Children.Clear();
            grdEqpt.ColumnDefinitions.Clear();
            grdEqpt.RowDefinitions.Clear();
        }

        private void GetLotIdentBasCode()
        {
            try
            {
                _LDR_LOT_IDENT_BAS_CODE = "";
                _UNLDR_LOT_IDENT_BAS_CODE = "";

                DataTable inTable = new DataTable();
                inTable.Columns.Add("LANGID", typeof(string));
                inTable.Columns.Add("PROCID", typeof(string));
                inTable.Columns.Add("EQSGID", typeof(string));

                DataRow dtRow = inTable.NewRow();
                dtRow["LANGID"] = LoginInfo.LANGID;
                dtRow["PROCID"] = Process.VD_LMN;
                dtRow["EQSGID"] = Util.NVC(cboVDEquipmentSegment.SelectedValue);

                inTable.Rows.Add(dtRow);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_EQP_SEL_LOT_IDENT_BAS_CODE", "INDATA", "OUTDATA", inTable);

                if (dtRslt != null && dtRslt.Rows.Count > 0)
                {
                    if (dtRslt.Columns.Contains("LDR_LOT_IDENT_BAS_CODE"))
                        _LDR_LOT_IDENT_BAS_CODE = Util.NVC(dtRslt.Rows[0]["LDR_LOT_IDENT_BAS_CODE"]);

                    if (dtRslt.Columns.Contains("UNLDR_LOT_IDENT_BAS_CODE"))
                        _UNLDR_LOT_IDENT_BAS_CODE = Util.NVC(dtRslt.Rows[0]["UNLDR_LOT_IDENT_BAS_CODE"]);
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void ApplyPermissions()
        {
            List<Button> listAuth = new List<Button>();
            Util.pageAuth(listAuth, FrameOperation.AUTHORITY);
        }

        private void ShowLoadIndicator()
        {
            if (loadingIndicator == null)
                return;

            if (loadingIndicator.Visibility != Visibility.Visible)
                loadingIndicator.Visibility = Visibility.Visible;
        }

        private void HideLoadIndicator()
        {
            if (loadingIndicator == null)
                return;

            if (loadingIndicator.Visibility != Visibility.Collapsed)
                loadingIndicator.Visibility = Visibility.Collapsed;
        }

        private void SetSummaryLayout()
        {
            _isVdQaTrgtChgHoldArea = UseCommoncodePlant(Util.NVC(cboVDArea.SelectedValue));

            if (_isVdQaTrgtChgHoldArea)
            {
                spnlLimiStocker.Visibility = Visibility.Visible;

                if (Convert.ToString(cboEquipmentElec.SelectedValue).IsNullOrEmpty())
                {
                    rdoThree.Visibility = Visibility.Collapsed;
                    rdoTwo.IsChecked = true;
                } else
                {
                    rdoThree.Visibility = Visibility.Visible;
                }
                    
                gdMain.RowDefinitions[6].Height = new GridLength(41);
            }
            else
            {
                spnlLimiStocker.Visibility = Visibility.Collapsed;
                rdoThree.Visibility = Visibility.Visible;
                gdMain.RowDefinitions[6].Height = new GridLength(0);
            }
            gdMain.UpdateLayout();
        }

        #endregion


    }
}
