/*************************************************************************************
 Created Date : 2022.05.04
      Creator : 오화백
   Decription : 슬라이딩 Group 설정 - LOT LIST 조회 [팝업]
--------------------------------------------------------------------------------------
 [Change History]
  2022.05.04  오화백 : Initial Created.    
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



namespace LGC.GMES.MES.COM001
{
    public partial class COM001_148_LOTLIST : C1Window, IWorkArea
    {

        #region Declaration & Constructor 
     
     
        private string _GroupType = string.Empty;
        //private string _Polarity  = string.Empty;
        public bool IsUpdated;

        private readonly Util _util = new Util();

        protected override void OnInitialized(EventArgs e)
        {
            base.OnInitialized(e);
        }
        public COM001_148_LOTLIST()
        {
            InitializeComponent();
        }
        public IFrameOperation FrameOperation
        {
            get;
            set;
        }

        public DataTable dtOutLotList { get; set; }
        public string _Polarity { get; set; }
        public string _Prodid { get; set; }

        /// <summary>
        /// 화면로드시
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void C1Window_Loaded(object sender, RoutedEventArgs e)
        {
            ApplyPermissions();
            object[] tmps = C1WindowExtension.GetParameters(this);

            if (tmps != null && tmps.Length >= 1)
            {
                _GroupType = Util.NVC(tmps[0]);
                _Prodid    = Util.NVC(tmps[1]);
                _Polarity  = Util.NVC(tmps[2]);
            }
            txtProdID.Text = _Prodid;
            txtProdID.IsEnabled = false;
            txtLot_TabMax.Value = 1;
            txtLot_BottomMax.Value = 1;

            //SearchLotList();
        }

        /// <summary>
        /// 화면내 버튼 권한 처리
        /// </summary>
        private void ApplyPermissions()
        {
            List<Button> listAuth = new List<Button> { btnAdd };
            Util.pageAuth(listAuth, FrameOperation.AUTHORITY);
        }
    

        #endregion

        #region Event

        #region 조회버튼 클릭 : btnSearch_Click()
        /// <summary>
        /// 조회
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            SearchLotList();
        }
        #endregion
  
        #region 추가 버튼 클릭 : btnOutput_Click()
        /// <summary>
        /// 그룹추가 선택된 LOT LIST
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnAdd_Click(object sender, RoutedEventArgs e)
        {
            //추가 하시겠습니까?
            Util.MessageConfirm("SFU2965", result =>
            {
                if (result == MessageBoxResult.OK)
                {
                    if (dgList.Rows.Count == 0)
                    {
                        //Util.Alert("조회된 데이터가 없습니다.");
                        Util.MessageValidation("SFU3537");
                        return;
                    }
                    dtOutLotList = ((DataView)dgList.ItemsSource).Table;
                    int ChkCount = 0;
                    for (int i = 0; i < dtOutLotList.Rows.Count; i++)
                    {
                        if (dtOutLotList.Rows[i]["CHK"].ToString() == "True" || dtOutLotList.Rows[i]["CHK"].ToString() == "1")
                        {
                            ChkCount = ChkCount + 1;
                        }
                    }
                    if (ChkCount == 0)
                    {
                        //Util.Alert("선택된 항목이 없습니다.");
                        Util.MessageValidation("SFU1651");
                        return;
                    }
                    for (int i = (dtOutLotList.Rows.Count - 1); i >= 0; i--)
                    {
                        if (dtOutLotList.Rows[i]["CHK"].ToString() == "False" || dtOutLotList.Rows[i]["CHK"].ToString() == "0")
                        {
                            dtOutLotList.Rows[i].Delete();
                        }
                    }
                    dtOutLotList.AcceptChanges();
                    this.DialogResult = MessageBoxResult.OK;
                }
            });
         
        }
        #endregion

        #region 닫기 : btnClose_Click()
        /// <summary>
        /// 닫기
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = MessageBoxResult.Cancel;
        }
        #endregion

        #endregion

        #region Mehod

        #region 조회  : SearchLotList()
        /// <summary>
        /// 선택한 제품에 해당되는 Lot 조회
        /// </summary>
        /// <param name="sProdId"></param>
        private void SearchLotList()
        {
            try
            {
                Util.gridClear(dgList);

                string BizName = string.Empty;

                if(_GroupType == "NP")
                {
                    BizName = "DA_PRD_SEL_SLID_MATC_GR_LOT_INFO_BY_PROD_ASSY";
                }
                else
                {
                    BizName = "DA_PRD_SEL_SLID_MATC_GR_LOT_INFO_BY_PROD_ELEC";
                }


                DataTable dt = new DataTable("RQSTDT");
                dt.Columns.Add("PRODID", typeof(string));
                dt.Columns.Add("TAB_MIN_VAL", typeof(string));
                dt.Columns.Add("TAB_MAX_VAL", typeof(string));
                dt.Columns.Add("BOTTOM_MIN_VAL", typeof(string));
                dt.Columns.Add("BOTTOM_MAX_VAL", typeof(string));
                dt.Columns.Add("MODIFY_CHK", typeof(string));
                dt.Columns.Add("AREAID", typeof(string));

                DataRow dr = dt.NewRow();
                dr["PRODID"] = Util.GetCondition(txtProdID);
                dr["TAB_MIN_VAL"] = txtLot_TabMin.Value;
                dr["TAB_MAX_VAL"] = txtLot_TabMax.Value;
                dr["BOTTOM_MIN_VAL"] = txtLot_BottomMin.Value;
                dr["BOTTOM_MAX_VAL"] = txtLot_BottomMax.Value;
                dr["MODIFY_CHK"] = "Y";
                dr["AREAID"] = LoginInfo.CFG_AREA_ID;
                dt.Rows.Add(dr);

                ShowLoadingIndicator();

                DataTable dtResult = new ClientProxy().ExecuteServiceSync(BizName, "RQSTDT", "RSLTDT", dt);

                if (dtResult != null && dtResult.Rows.Count > 0)
                {
                    Util.GridSetData(dgList, dtResult, FrameOperation, false);
                }
                else
                {
                    //Util.Alert("조회된 데이터가 없습니다.");
                    Util.MessageValidation("SFU3537");
                    return;
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
            finally
            {
                HiddenLoadingIndicator();
            }
        }


        #endregion

        #region 프로그래스 관련 Method : ShowLoadingIndicator(), HiddenLoadingIndicator()
        /// <summary>
        /// 프로그래스 바 보이기
        /// </summary>
        private void ShowLoadingIndicator()
        {
            if (loadingIndicator != null)
                loadingIndicator.Visibility = Visibility.Visible;
        }

        /// <summary>
        /// 프로그래스 바 숨기기
        /// </summary>
        private void HiddenLoadingIndicator()
        {
            if (loadingIndicator != null)
                loadingIndicator.Visibility = Visibility.Collapsed;
        }
        #endregion
    

        #endregion


    }
}