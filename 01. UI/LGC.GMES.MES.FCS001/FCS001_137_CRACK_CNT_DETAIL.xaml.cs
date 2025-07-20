/*************************************************************************************
 Created Date : 2022.10.05
      Creator : 최도훈
   Decription : CRACK 카운트 상세 조회 
--------------------------------------------------------------------------------------
 [Change History]
  2022.10.05  최도훈 : Initial Created
  2022.10.11  최도훈 : SPCL_MNGT_CNT 관리 테이블 수정 (동별 공통코드 관리 -> Tray 유형) 
  2023.04.11  최도훈 : 형변환 예외처리 추가
**************************************************************************************/

using C1.WPF;
using C1.WPF.DataGrid;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.ControlsLibrary;
using LGC.GMES.MES.Common;
using System;
using System.Collections.Generic;
using System.Data;
using System.Windows;
using System.Windows.Media;

namespace LGC.GMES.MES.FCS001
{
    public partial class FCS001_137_CRACK_CNT_DETAIL : C1Window, IWorkArea
    {
        #region Declaration & Constructor         

        private string _sLineId;
        private string _sModelId;
        private string _sBldgCd;
        private string _sMin;
        private string _sMax;
        private DateTime _dtFromDate;
        private DateTime _dtToDate;

        private int _CrackLimitCnt = 0;

        #region Initialize 
        /// <summary>
        ///  Frame과 상호작용하기 위한 객체
        /// </summary>
        public IFrameOperation FrameOperation
        {
            get;
            set;
        }

        public FCS001_137_CRACK_CNT_DETAIL()
        {
            InitializeComponent();

            C1RowIndexHeaderBehavior.SetRowIndexHeaderBehavior(dgTrayList, new C1RowIndexHeaderBehavior() { Mode = C1RowIndexHeaderMode.RowIndex, StretchHeader = true });
        }
        #endregion

        private void C1Window_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                object[] tmps = C1WindowExtension.GetParameters(this);
                _sLineId = tmps[0] as string;
                _sModelId = tmps[1] as string;
                _sBldgCd = tmps[2] as string;
                _sMin = tmps[3] as string;
                _sMax = tmps[4] as string;
                _dtFromDate = Convert.ToDateTime(tmps[5]);
                _dtToDate = Convert.ToDateTime(tmps[6]);

                GetList();
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        
        private void btnClose_Clicked(object sender, RoutedEventArgs e)
        {
            this.DialogResult = MessageBoxResult.Cancel;
        }

        #endregion

        #region Mehod

        private void GetList()
        {
            try
            {
                Util.gridClear(dgTrayList);
                ShowLoadingIndicator();

                DataTable dtRqst = new DataTable();
                dtRqst.Columns.Add("AREAID", typeof(string));
                dtRqst.Columns.Add("LINE_ID", typeof(string));
                dtRqst.Columns.Add("MODEL_ID", typeof(string));
                dtRqst.Columns.Add("BLDG_CD", typeof(string));
                dtRqst.Columns.Add("MIN", typeof(string));
                dtRqst.Columns.Add("MAX", typeof(string));
                dtRqst.Columns.Add("FROM_DATE", typeof(DateTime));
                dtRqst.Columns.Add("TO_DATE", typeof(DateTime));

                DataRow dr = dtRqst.NewRow();
                dr["AREAID"] = LoginInfo.CFG_AREA_ID;
                dr["LINE_ID"] = !string.IsNullOrWhiteSpace(_sLineId) ? _sLineId : null;
                dr["MODEL_ID"] = !string.IsNullOrWhiteSpace(_sModelId) ? _sModelId : null;
                dr["BLDG_CD"] = !string.IsNullOrWhiteSpace(_sBldgCd) ? _sBldgCd : null;
                dr["MIN"] = _sMin;
                dr["MAX"] = _sMax;
                dr["FROM_DATE"] = _dtFromDate;
                dr["TO_DATE"] = _dtToDate;

                dtRqst.Rows.Add(dr);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_MCS_SEL_CRACK_CNT_INFO", "RQSTDT", "RSLTDT", dtRqst);
                Util.GridSetData(dgTrayList, dtRslt, FrameOperation, true);

                HiddenLoadingIndicator();
            }
            catch (Exception ex)
            {
                HiddenLoadingIndicator();
                Util.MessageException(ex);
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

        #region [Event]
              
        private void dgTrayList_LoadedCellPresenter(object sender, DataGridCellEventArgs e)
        {
            if (sender == null)
                return;

            C1DataGrid dg = sender as C1DataGrid;

            dg?.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (e.Cell.Presenter == null)
                {
                    return;
                }

                if (e.Cell.Row.Type == DataGridRowType.Item)
                {
                    if (e.Cell.Column.Name != null && e.Cell.Column.Name.ToString() == "SPCL_MNGT_CNT")
                    {
                        if (!Int32.TryParse(e.Cell.Text, out _CrackLimitCnt))
                        {
                            _CrackLimitCnt = 0;
                        }
                    }

                    if (e.Cell.Column.Name != null && e.Cell.Column.Name.ToString() == "BCR_SCAN_CNT")
                    {
                        if (Convert.ToInt32(e.Cell.Text) > _CrackLimitCnt)
                        {
                            e.Cell.Presenter.Background = new SolidColorBrush(Colors.HotPink);
                            _CrackLimitCnt = 0;
                        }
                    }
                }
            }));
        }

        #endregion
        
    }
}
