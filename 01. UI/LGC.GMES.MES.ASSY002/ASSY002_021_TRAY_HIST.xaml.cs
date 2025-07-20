/*************************************************************************************
 Created Date : 2017.07.25
      Creator : 
   Decription : 
--------------------------------------------------------------------------------------
 [Change History]
 
**************************************************************************************/

using System;
using System.Windows;
using System.Data;

using C1.WPF;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.CMM001.Extensions;
using LGC.GMES.MES.CMM001.UserControls;
using LGC.GMES.MES.CMM001.Popup;
using System.Windows.Controls;
using System.Collections.Generic;
using C1.WPF.DataGrid.Summaries;
using System.Windows.Media;

namespace LGC.GMES.MES.ASSY002
{
    public partial class ASSY002_021_TRAY_HIST : C1Window, IWorkArea
    {
        #region Declaration & Constructor 

        Util _util = new Util();
        string _sLotID = string.Empty;

        public ASSY002_021_TRAY_HIST()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Frame과 상호작용하기 위한 객체
        /// </summary>
        public IFrameOperation FrameOperation
        {
            get;
            set;
        }

        private void C1Window_Loaded(object sender, RoutedEventArgs e)
        {
            ApplyPermissions();
            InitCombo();
            InitControl();
            SetEvent();
            GetTRAYHist();
        }

        #endregion

        #region Initialize
        /// <summary>
        /// 화면내 버튼 권한 처리
        /// </summary>
        private void ApplyPermissions()
        {
            List<Button> listAuth = new List<Button>();
            //listAuth.Add(btnInReplace);
            Util.pageAuth(listAuth, FrameOperation.AUTHORITY);
        }

        /// <summary>
        /// 콤보박스 초기 설정
        /// </summary>
        private void InitCombo()
        {
            CommonCombo _combo = new CommonCombo();

            //_combo.SetCombo(cboLine, CommonCombo.ComboStatus.SELECT, sFilter: new string[]{LoginInfo.CFG_AREA_ID}, cbChild: new C1ComboBox[] {cboEquipment_Search }, sCase: "LINE_CP");

        }

        /// <summary>
        /// 컨트롤 초기값 설정
        /// </summary>
        private void InitControl()
        {
            DataTable dtInfo = new DataTable();
            for (int i = 0; i < dgMain.Columns.Count; i++)
            {
                dtInfo.Columns.Add(dgMain.Columns[i].Name);
            }

            object[] param = C1WindowExtension.GetParameters(this);

            if (param.Length >= 5)
            {
                dtInfo.Rows.Add();
                dtInfo.Rows[0]["LOTID"] = _sLotID = Util.NVC(param[0]);
                dtInfo.Rows[0]["CALDATE"] = param[1];
                dtInfo.Rows[0]["EQSGNAME"] = param[2];
                dtInfo.Rows[0]["PRJT_NAME"] = param[3];
                dtInfo.Rows[0]["PRODID"] = param[4];
                Util.GridSetData(dgMain, dtInfo, FrameOperation);
            }
        }
        #endregion

        #region Event
        /// <summary>
        /// Initializing 이후에 FormLoad시 Event를 생성.
        /// </summary>
        private void SetEvent()
        {
            //   this.Loaded -= UserControl_Loaded;
        }

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

        #region [Biz]
        /// <summary>
        /// 조회
        /// BIZ : 
        /// </summary>
        private void GetTRAYHist()
        {
            try
            {
                object[] tmps = C1WindowExtension.GetParameters(this);

                DataTable RQSTDT = new DataTable();
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("LOTID", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["LOTID"] = _sLotID;

                RQSTDT.Rows.Add(dr);

                new ClientProxy().ExecuteService("DA_PRD_SEL_FORM_MOVE_DETAIL", "RQSTDT", "RSLTDT", RQSTDT, (searchResult, searchException) =>
                {
                    try
                    {
                        if (searchException != null)
                        {
                            Util.MessageException(searchException);
                            return;
                        }
                        if (searchResult.Rows.Count > 0)
                        {
                            DataGridAggregate.SetAggregateFunctions(dgDetail.Columns["TRAYID"], new DataGridAggregatesCollection { new DataGridAggregateCount() });
                            DataGridAggregate.SetAggregateFunctions(dgDetail.Columns["WIPQTY2_ED"], new DataGridAggregatesCollection { new DataGridAggregateSum() });
                            DataGridAggregate.SetAggregateFunctions(dgDetail.Columns["FCS_CHG_QTY"], new DataGridAggregatesCollection { new DataGridAggregateSum() });
                            DataGridAggregate.SetAggregateFunctions(dgDetail.Columns["QTY_GAP"], new DataGridAggregatesCollection { new DataGridAggregateSum() });
                        }

                        Util.GridSetData(dgDetail, searchResult, FrameOperation, true);
                    }
                    catch (Exception ex)
                    {
                        Util.MessageException(ex);

                    }
                    finally
                    {
                        loadingIndicator.Visibility = Visibility.Collapsed;
                    }
                }
                );
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
            finally
            {
                loadingIndicator.Visibility = Visibility.Collapsed;
            }
        }
        #endregion

        private void dgDetail_LoadedCellPresenter(object sender, C1.WPF.DataGrid.DataGridCellEventArgs e)
        {
            try
            {
                C1.WPF.DataGrid.C1DataGrid dataGrid = sender as C1.WPF.DataGrid.C1DataGrid;

                dataGrid.Dispatcher.BeginInvoke(new Action(() =>
                {
                    if (e.Cell.Presenter == null)
                    {
                        return;
                    }

                    if (e.Cell.Column.Name == "QTY_GAP"
                    && !string.IsNullOrWhiteSpace(e.Cell.Text))
                    {
                        e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Red);
                        e.Cell.Presenter.FontWeight = FontWeights.Bold;
                    }

                    else
                    {
                        e.Cell.Presenter.Foreground = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#FF333333"));
                    }
                }));
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }

        }
    }
}
