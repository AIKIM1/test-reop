/*************************************************************************************
 Created Date : 2022.12.26
      Creator : 오화백 과장
   Decription : 창고별 적재율 - 입고LOT 조회 [팝업]
--------------------------------------------------------------------------------------
 [Change History]
  2022.12.26  오화백 : Initial Created.    
  2023.10.02  오화백      : GMES 및 CIMS MISMACHING  문제로  금지단 제외 로직 주석 처리 
**************************************************************************************/
using C1.WPF;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using System;
using System.Collections.Generic;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Linq;
using System.Threading;
using System.Windows.Media;
using System.Windows.Threading;
using C1.WPF.DataGrid;
using LGC.GMES.MES.CMM001.Extensions;
using LGC.GMES.MES.CMM001.UserControls;
using System.Windows.Data;
using System.Windows.Media.Animation;
namespace LGC.GMES.MES.MCS001
{
    public partial class MCS001_088_LOTLIST : C1Window, IWorkArea
    {

        #region Declaration & Constructor 

        private string _areaCode;
        private string _eqgrid;
        private string _stk_eltr_type;
        private string _eqptid;
        private string _pjt;
        private string _TrayStockerYN;

        /// <summary>
        /// 생성자
        /// </summary>
        public MCS001_088_LOTLIST()
        {
        
            InitializeComponent();
        }
        public IFrameOperation FrameOperation
        {
            get;
            set;
        }
       /// <summary>
       ///  화면 로드시
       /// </summary>
       /// <param name="sender"></param>
       /// <param name="e"></param>
        private void C1Window_Loaded(object sender, RoutedEventArgs e)
        {
            ApplyPermissions();

            /// 호출 파라미터 셋팅
            object[] parameters = C1WindowExtension.GetParameters(this);

            if (parameters != null)
            {
                _areaCode = Util.NVC(parameters[0]);
                _eqgrid = Util.NVC(parameters[1]);
                _stk_eltr_type = Util.NVC(parameters[2]);
                _eqptid = Util.NVC(parameters[3]);
                _pjt = Util.NVC(parameters[4]);
                _TrayStockerYN = Util.NVC(parameters[5]);

                //Tray Stocker 여부 확인
                if (_TrayStockerYN == "N")
                {
                    dgList.Visibility = Visibility.Visible;
                    dgList_Tray.Visibility = Visibility.Collapsed;
                    SelectLotList();

                    Columns_Visibility();
                }
                else
                {
                    //dgList.Visibility = Visibility.Collapsed;
                    //dgList_Tray.Visibility = Visibility.Visible;
                    //SelectLotList_TrayStocker();
                }
                
            }

        }

        /// <summary>
        /// 화면내 버튼 권한 처리
        /// </summary>
        private void ApplyPermissions()
        {
            List<Button> listAuth = new List<Button>();
            Util.pageAuth(listAuth, FrameOperation.AUTHORITY);
        }
        #endregion

        #region Event

        #region 화면닫기 : btnClose_Click()
        /// <summary>
        /// 화면닫기
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        #endregion

        #region 경과일수에 대한 컬럼 색깔 처리 : dgList_LoadedCellPresenter(), dgList_UnloadedCellPresenter()

        private void dgList_LoadedCellPresenter(object sender, C1.WPF.DataGrid.DataGridCellEventArgs e)
        {
            if (sender == null) return;

            C1DataGrid dataGrid = sender as C1DataGrid;
            dataGrid?.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (e.Cell.Presenter == null)
                {
                    return;
                }

                if (e.Cell.Row.Type == DataGridRowType.Item)
                {
                    if (Convert.ToString(e.Cell.Column.Name) == "PAST_DAY")
                    {
                        if (DataTableConverter.GetValue(e.Cell.Row.DataItem, "PAST_DAY").GetInt() >= 30)
                        {
                            e.Cell.Presenter.Background = new SolidColorBrush(Colors.Red);
                        }
                        else if (DataTableConverter.GetValue(e.Cell.Row.DataItem, "PAST_DAY").GetInt() >= 15)
                        {
                            e.Cell.Presenter.Background = new SolidColorBrush(Colors.Orange);
                        }
                        else if (DataTableConverter.GetValue(e.Cell.Row.DataItem, "PAST_DAY").GetInt() >= 7)
                        {
                            //e.Cell.Presenter.Background = new SolidColorBrush(Colors.GreenYellow);
                            e.Cell.Presenter.Background = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#F2CB61"));
                        }
                        else
                            e.Cell.Presenter.Background = null;
                    }
                    else
                    {
                        var convertFromString = System.Windows.Media.ColorConverter.ConvertFromString("Black");
                        if (convertFromString != null)
                            e.Cell.Presenter.Foreground = new SolidColorBrush((Color)convertFromString);
                    }
                }
                else
                {

                }
            }));
        }

        private void dgList_UnloadedCellPresenter(object sender, DataGridCellEventArgs e)
        {
            if (sender == null) return;

            C1DataGrid dataGrid = sender as C1DataGrid;
            if (e.Cell.Presenter != null)
            {
                if (e.Cell.Row.Type == DataGridRowType.Item)
                {
                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Black);
                    e.Cell.Presenter.FontWeight = FontWeights.Normal;
                    e.Cell.Presenter.Background = null;
                }
            }
        }

        #endregion

        #endregion

        #region Mehod
        
        #region LOT 상세정보 조회 : SelectLotList()
        /// <summary>
        ///  LOT 상세정보 조회 
        /// </summary>
        private void SelectLotList()
        {
            string bizRuleName = string.Empty;
            bizRuleName = "BR_MHS_SEL_STO_INVENT_LIST_NORM_USING_CST";
            try
            {
                ShowLoadingIndicator();
                DataTable inTable = new DataTable("RQSTDT");
                inTable.Columns.Add("LANGID", typeof(string));
                inTable.Columns.Add("AREAID", typeof(string));
                inTable.Columns.Add("EQGRID", typeof(string));
                inTable.Columns.Add("ELTR_TYPE_CODE", typeof(string));
                inTable.Columns.Add("EQPTID", typeof(string));
                inTable.Columns.Add("PRJT_NAME", typeof(string));
                inTable.Columns.Add("WIPHOLD", typeof(string));
                inTable.Columns.Add("CFG_AREA_ID", typeof(string));


                DataRow dr = inTable.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["AREAID"] = _areaCode;
                dr["EQGRID"] = _eqgrid;
                if (!string.IsNullOrEmpty(_stk_eltr_type))
                {
                    dr["ELTR_TYPE_CODE"] = _stk_eltr_type;
                }
                if (!string.IsNullOrEmpty(_pjt))
                {
                    dr["PRJT_NAME"] = _pjt;
                }
                dr["EQPTID"] = _eqptid;
                dr["CFG_AREA_ID"] = LoginInfo.CFG_AREA_ID;
                inTable.Rows.Add(dr);

                new ClientProxy().ExecuteService(bizRuleName, "RQSTDT", "RSLTDT", inTable, (bizResult, bizException) =>
                {
                    if (bizException != null)
                    {
                        HiddenLoadingIndicator();
                        Util.MessageException(bizException);
                        return;
                    }
                    // 금지단 ROW는 삭제
                    //bizResult.Select("RACK_STAT_CODE = 'DISABLE'").ToList<DataRow>().ForEach(row => row.Delete());
                    //bizResult.AcceptChanges();
                    Util.GridSetData(dgList, bizResult, null, true);
                    HiddenLoadingIndicator();

                });
            }
            catch (Exception ex)
            {
                HiddenLoadingIndicator();
                Util.MessageException(ex);
            }
        }

        #endregion

        #region LOT 상세정보 조회(Tray Stocker) : SelectLotList_TrayStocker()

        /// <summary>
        /// LOT 상세정보 조회(Tray Stocker) 
        /// </summary>
        private void SelectLotList_TrayStocker()
        {

            /*const string bizRuleName = "BR_MCS_GET_HIGH_CNV_U_TRAY_MNT_DETL";*/

            //try
            //{
            //    ShowLoadingIndicator();
            //    DataTable inTable = new DataTable("INDATA");
            //    inTable.Columns.Add("LANGID", typeof(string));
            //    inTable.Columns.Add("EQPTID", typeof(string));

            //    DataRow dr = inTable.NewRow();
            //    dr["LANGID"] = LoginInfo.LANGID;
            //    dr["EQPTID"] = _eqptid;
            //    inTable.Rows.Add(dr);
            //    //MES BIZ가 아닌 폴란드 MCS BIZ 접속
            //    new ClientProxy(_bizRuleIp, _bizRuleProtocol, _bizRulePort, _bizRuleServiceMode, _bizRuleServiceIndex).ExecuteService(bizRuleName, "INDATA", "OUTDATA", inTable, (bizResult, bizException) =>
            //    {
            //        if (bizException != null)
            //        {
            //            HiddenLoadingIndicator();
            //            Util.MessageException(bizException);
            //            return;
            //        }

            //        Util.GridSetData(dgList_Tray, bizResult, null, true);


            //        HiddenLoadingIndicator();
            //    });

            //}
            //catch (Exception ex)
            //{
            //    HiddenLoadingIndicator();
            //    Util.MessageException(ex);
            //}
        }
        #endregion

        #region Loading bar  : ShowLoadingIndicator(), HiddenLoadingIndicator()

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


        #region 창고에 따른 리스트 컬럼 숨김여부 : Columns_Visibility()
        /// <summary>
        ///  창고에 따른 리스트 컬럼 숨김여부
        /// </summary>
        private void Columns_Visibility()
        {
            //불량태그수
            dgList.Columns["DFCT_TAG_QTY"].Visibility = Visibility.Collapsed;
            //무지부, 권취방향
            dgList.Columns["HALF_SLIT_SIDE"].Visibility = Visibility.Collapsed;
            dgList.Columns["ROLL_DIRECTION"].Visibility = Visibility.Collapsed;
            //생산설비(COATING) 
            dgList.Columns["COATING_NAME"].Visibility = Visibility.Collapsed;
            //QA 검사결과, QA Hold 여부, QA 검사비고
            dgList.Columns["JUDG_TYPE"].Visibility = Visibility.Collapsed;
            dgList.Columns["QMS_HOLD_FLAG"].Visibility = Visibility.Collapsed;
            dgList.Columns["FINL_JUDG_NOTE"].Visibility = Visibility.Collapsed;
            //VD검사결과
            dgList.Columns["VD_QA_RESULT"].Visibility = Visibility.Collapsed;
            //IQC 검사결과
            dgList.Columns["IQC_JUDGEMENT"].Visibility = Visibility.Collapsed;
            //투입LOT
            dgList.Columns["INPUT_LOTID"].Visibility = Visibility.Collapsed;
            //FAST TRACK
            dgList.Columns["FAST_TRACK_FLAG"].Visibility = Visibility.Collapsed;
            //극성
            dgList.Columns["ELTR_TYPE_NAME"].Visibility = Visibility.Collapsed;
            //SKID 
            dgList.Columns["SKID_ID"].Visibility = Visibility.Collapsed;
            
            //스태킹 완성창고일 경우  컬럼명 변경
            dgList.Columns["SKID_ID"].Header = ObjectDic.Instance.GetObjectName("Carrier ID");
            dgList.Columns["BOBBIN_ID"].Header = ObjectDic.Instance.GetObjectName("보빈 ID");
     
            // 노칭대기창고
            if (_eqgrid == "NWW")
            {
                dgList.Columns["DFCT_TAG_QTY"].Visibility = Visibility.Visible;
                dgList.Columns["ELTR_TYPE_NAME"].Visibility = Visibility.Visible;
                dgList.Columns["ELTR_TYPE_NAME"].Visibility = Visibility.Visible;
                dgList.Columns["IQC_JUDGEMENT"].Visibility = Visibility.Visible;
                dgList.Columns["COATING_NAME"].Visibility = Visibility.Visible;
                dgList.Columns["SKID_ID"].Visibility = Visibility.Visible;
            }
            //노칭 완성창고
            else if (_eqgrid == "NPW")
            {
                dgList.Columns["COATING_NAME"].Visibility = Visibility.Visible;
                dgList.Columns["VD_QA_RESULT"].Visibility = Visibility.Visible;
                dgList.Columns["INPUT_LOTID"].Visibility = Visibility.Visible;
                dgList.Columns["ELTR_TYPE_NAME"].Visibility = Visibility.Visible;
                dgList.Columns["SKID_ID"].Visibility = Visibility.Visible;
            }
            //라미 대기창고
            else if (_eqgrid == "LWW")
            {
                dgList.Columns["COATING_NAME"].Visibility = Visibility.Visible;
                dgList.Columns["VD_QA_RESULT"].Visibility = Visibility.Visible;
                dgList.Columns["INPUT_LOTID"].Visibility = Visibility.Visible;
                dgList.Columns["ELTR_TYPE_NAME"].Visibility = Visibility.Visible;
                dgList.Columns["QMS_HOLD_FLAG"].Visibility = Visibility.Visible;
                dgList.Columns["DFCT_TAG_QTY"].Visibility = Visibility.Visible;
                dgList.Columns["SKID_ID"].Visibility = Visibility.Visible;
               
            }
            //스태킹 완성 창고
            else if (_eqgrid == "SWW")
            {
                dgList.Columns["INPUT_LOTID"].Visibility = Visibility.Visible;
                //스태킹 완성창고일 경우  컬럼명 변경
                dgList.Columns["SKID_ID"].Header = ObjectDic.Instance.GetObjectName("Group Carrier ID");
                dgList.Columns["BOBBIN_ID"].Header = ObjectDic.Instance.GetObjectName("Carrier ID");

                dgList.Columns["SKID_ID"].Visibility = Visibility.Visible;
               

            }
            else if (_eqgrid == "PCW")
            {
                dgList.Columns["JUDG_TYPE"].Visibility = Visibility.Visible;
                dgList.Columns["QMS_HOLD_FLAG"].Visibility = Visibility.Visible;
                dgList.Columns["FINL_JUDG_NOTE"].Visibility = Visibility.Visible;
                dgList.Columns["COATING_NAME"].Visibility = Visibility.Visible;
                dgList.Columns["FAST_TRACK_FLAG"].Visibility = Visibility.Visible;
                dgList.Columns["ELTR_TYPE_NAME"].Visibility = Visibility.Visible;
                dgList.Columns["SKID_ID"].Visibility = Visibility.Visible;
           }
        }

        #endregion


        #endregion


    }
}