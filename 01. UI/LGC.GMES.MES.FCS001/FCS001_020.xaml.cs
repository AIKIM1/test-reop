/*************************************************************************************
 Created Date : 2020.10.20
      Creator : Kang Dong Hee
   Decription : 저전압 유출방지
--------------------------------------------------------------------------------------
 [Change History]
  2020.11.20  NAME : Initial Created
  2021.04.01  KDH : 링크 기능을 Head -> Cell 로 전환 및 Bold 제거
  2021.04.09  KDH : 화면간 이동 시 초기화 현상 제거
  2021.04.11  KDH : 조건 선택 시 발생되는 Event 변경 수정 및 이전 출력 정보 Clear 되도록 대응.
  2022.09.19  김진섭 : [C20220816-000503] ESNB2동 장기미처리 CSR 대응건 - W판정 진행 유무 플래그값 추가 (JUDG_YN - 판정을 한번 이라도 진행했을 시 'Y')
  2022.11.02  KDH : 조회 조건 선택 후 조회 버튼 클릭 시 조회 되도록 수정
**************************************************************************************/
#define SAMPLE_DEV

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
    public partial class FCS001_020 : UserControl, IWorkArea
    {
        #region Declaration & Constructor 

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

        public FCS001_020()
        {
            InitializeComponent();
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                //Combo Setting
                InitCombo();
                //Control Setting
                InitControl();

                this.Loaded -= UserControl_Loaded; //2021.04.09 화면간 이동 시 초기화 현상 제거
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

            C1ComboBox[] cboLineChild = { cboModel };
            ComCombo.SetCombo(cboLine, CommonCombo_Form.ComboStatus.ALL, sCase: "LINE", cbChild: cboLineChild);

            C1ComboBox[] cboModelChild = { cboRoute };
            C1ComboBox[] cboModelParent = { cboLine };
            ComCombo.SetCombo(cboModel, CommonCombo_Form.ComboStatus.ALL, sCase: "LINEMODEL", cbChild: cboModelChild, cbParent: cboModelParent);

            C1ComboBox[] cboRouteParent = { cboLine, cboModel };
            ComCombo.SetCombo(cboRoute, CommonCombo_Form.ComboStatus.ALL, sCase: "ROUTE", cbParent: cboRouteParent);

            //2021.04.11 조건 선택 시 발생되는 Event 변경 수정. START
            //cboLine.SelectedIndexChanged += cbo_SelectedIndexChanged;
            //cboModel.SelectedIndexChanged += cbo_SelectedIndexChanged;
            //cboRoute.SelectedIndexChanged += cbo_SelectedIndexChanged;
            //2021.04.11 조건 선택 시 발생되는 Event 변경 수정. END
        }

        private void InitControl()
        {
            dtpFromDate.SelectedDateTime = DateTime.Now.AddMonths(-2);
            dtpToDate.SelectedDateTime = DateTime.Now;
        }
        #endregion

        #region Event
        private void btnSearch_Click(object sender, EventArgs e)
        {
             GetList();
        }

        private void dgLowVoltage_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            try
            {
                Point pnt = e.GetPosition(null);
                C1.WPF.DataGrid.DataGridCell cell = dgLowVoltage.GetCellFromPoint(pnt);

                if (cell != null)
                {
                    if (!cell.Column.Name.Equals("DAY_GR_LOTID"))
                    {
                        return;
                    }

                    //화면 ID 확인 후 수정
                    FCS001_019_TRAY_SEL TraySelectInfo = new FCS001_019_TRAY_SEL();
                    TraySelectInfo.FrameOperation = FrameOperation;

                    if (TraySelectInfo != null)
                    {
                        object[] Parameters = new object[1];
                        Parameters[0] = Util.NVC(DataTableConverter.GetValue(dgLowVoltage.Rows[cell.Row.Index].DataItem, "DAY_GR_LOTID")).ToString(); //Lot ID;

                        C1WindowExtension.SetParameters(TraySelectInfo, Parameters);
                        TraySelectInfo.Closed += new EventHandler(TraySelectInfo_Closed);

                        this.Dispatcher.BeginInvoke(new Action(() => TraySelectInfo.ShowModal()));
                    }
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return;
            }
            finally
            {
                loadingIndicator.Visibility = Visibility.Collapsed;
            }

        }

        private void TraySelectInfo_Closed(object sender, EventArgs e)
        {
            FCS001_019_TRAY_SEL popup = sender as FCS001_019_TRAY_SEL;
            if (popup.DialogResult == MessageBoxResult.Yes)
            {
                GetList();
            }
            this.grdMain.Children.Remove(popup);
        }

        private void txtLotId_KeyDown(object sender, KeyEventArgs e)
        {
            if ((!string.IsNullOrEmpty(txtLotId.Text)) && (e.Key == Key.Enter))
            {
                btnSearch_Click(null, null);
            }
        }

        //2021.04.11 조건 선택 시 발생되는 Event 변경 수정. START
        //private void cbo_SelectedIndexChanged(object sender, EventArgs e)
        //{
        //    btnSearch_Click(null, null);
        //}
        //2021.04.11 조건 선택 시 발생되는 Event 변경 수정. END

        //2021.04.11 조건 선택 시 발생되는 Event 변경 수정. START
        private void cboLine_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            //btnSearch_Click(null, null); //20221102_조회 조건 선택 후 조회 버튼 클릭 시 조회 되도록 수정
        }
        //2021.04.11 조건 선택 시 발생되는 Event 변경 수정. END

        //2021.04.11 조건 선택 시 발생되는 Event 변경 수정. START
        private void cboModel_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            //btnSearch_Click(null, null); //20221102_조회 조건 선택 후 조회 버튼 클릭 시 조회 되도록 수정
        }
        //2021.04.11 조건 선택 시 발생되는 Event 변경 수정. END

        //2021.04.11 조건 선택 시 발생되는 Event 변경 수정. START
        private void cboRoute_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            //btnSearch_Click(null, null); //20221102_조회 조건 선택 후 조회 버튼 클릭 시 조회 되도록 수정
        }
        //2021.04.11 조건 선택 시 발생되는 Event 변경 수정. END

        private void dgLowVoltage_LoadedCellPresenter(object sender, DataGridCellEventArgs e)
        {
            if (sender == null)
            {
                return;
            }

            C1DataGrid dg = sender as C1DataGrid;

            dg?.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (e.Cell.Presenter == null)
                {
                    return;
                }

                if (e.Cell.Row.Type == DataGridRowType.Item)
                {
                    string Lot = Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "DAY_GR_LOTID"));

                    ////////////////////////////////////////////  default 색상 및 Cursor
                    e.Cell.Presenter.Cursor = Cursors.Arrow;

                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Black);
                    e.Cell.Presenter.FontSize = 12;
                    e.Cell.Presenter.FontWeight = FontWeights.Normal;
                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Transparent);
                    ///////////////////////////////////////////////////////////////////////////////////

                    if (e.Cell.Column.Name.ToString() == "DAY_GR_LOTID")
                    {
                        e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Blue); //2021.04.01 링크 기능을 Head -> Cell 로 전환 및 Bold 제거
                        e.Cell.Presenter.Cursor = Cursors.Hand;
                    }
                }
            }));
        }

        private void dgLowVoltage_LoadedColumnHeaderPresenter(object sender, C1.WPF.DataGrid.DataGridColumnEventArgs e)
        {
            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (string.IsNullOrEmpty(e.Column.Name) == false)
                {
                    if (e.Column.Name.Equals("DAY_GR_LOTID"))
                    {
                        //e.Column.HeaderPresenter.Foreground = new SolidColorBrush(Colors.Blue); //2021.04.01 링크 기능을 Head -> Cell 로 전환 및 Bold 제거
                        //e.Column.HeaderPresenter.FontWeight = FontWeights.Bold;                 //2021.04.01 링크 기능을 Head -> Cell 로 전환 및 Bold 제거
                    }
                }
            }));
        }

        #endregion

        #region Method
        private void GetList()
        {
            try
            {
                Util.gridClear(dgLowVoltage); //2021.04.11 이전 출력 정보 Clear 되도록 대응.

                DataTable dtRqst = new DataTable();
                dtRqst.TableName = "RQSTDT";
                dtRqst.Columns.Add("LANGID", typeof(string));
                dtRqst.Columns.Add("AREAID", typeof(string));
                dtRqst.Columns.Add("EQSGID", typeof(string));
                dtRqst.Columns.Add("MDLLOT_ID", typeof(string));
                dtRqst.Columns.Add("ROUTID", typeof(string));
                dtRqst.Columns.Add("FROM_DATE", typeof(string));
                dtRqst.Columns.Add("TO_DATE", typeof(string));
                dtRqst.Columns.Add("PROD_LOTID", typeof(string));

                DataRow dr = dtRqst.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["AREAID"] = LoginInfo.CFG_AREA_ID;
                dr["EQSGID"] = Util.GetCondition(cboLine, bAllNull: true);
                dr["MDLLOT_ID"] = Util.GetCondition(cboModel, bAllNull: true);
                dr["ROUTID"] = Util.GetCondition(cboRoute, bAllNull: true);
                dr["FROM_DATE"] = Util.GetCondition(dtpFromDate);
                dr["TO_DATE"] = Util.GetCondition(dtpToDate);
                dr["PROD_LOTID"] = Util.GetCondition(txtLotId, bAllNull: true);

                dtRqst.Rows.Add(dr);

                ShowLoadingIndicator();
                new ClientProxy().ExecuteService("DA_SEL_W_LOT_ROUTE_CHECK", "RQSTDT", "RSLTDT", dtRqst, (result, Exception) =>
                {
                    try
                    {
                        if (Exception != null)
                        {
                            LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(Exception), null, "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
                            return;
                        }

                        if (result.Rows.Count > 0)
                        {
                            Util.GridSetData(dgLowVoltage, result, FrameOperation, true);
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
                });

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void ShowLoadingIndicator()
        {
            if (loadingIndicator != null)
            {
                loadingIndicator.Visibility = Visibility.Visible;
            }
        }

        private void HiddenLoadingIndicator()
        {
            if (loadingIndicator != null)
            {
                loadingIndicator.Visibility = Visibility.Collapsed;
            }
        }
        #endregion

    }
}
