/*************************************************************************************
 Created Date : 2021.10.19
      Creator : 김용준
   Decription : 
--------------------------------------------------------------------------------------
 [Change History]
  2021.10.19  김용준     243862      Initialize
  2025.04.16  윤주일     SI          MI1_OSS_0103 / 검색 결과 = 0 이어도 목록에 반영되도록 수정
**************************************************************************************/

using C1.WPF;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.CMM001.Popup;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;
using System.Windows.Media;
using C1.WPF.DataGrid;
using LGC.GMES.MES.CMM001.Extensions;

namespace LGC.GMES.MES.PACK001
{
    public partial class PACK001_090 : UserControl, IWorkArea
    {
        #region [Declaration & Constructor]


        #endregion

        #region [ Initialize ]
        public PACK001_090()
        {
            InitializeComponent();
            tbCellListCount.Text = "[ 0 " + ObjectDic.Instance.GetObjectName("건") + " ]";
            InitializeCombo();
        }

        public IFrameOperation FrameOperation
        {
            get;
            set;
        }

        private void InitializeCombo()
        {
            CommonCombo _combo = new CommonCombo();

            // 동
            C1ComboBox[] cboDefectAreaChild = { cboDefectEquipmentSegment };
            String[] sFiltercboArea = { LoginInfo.CFG_SHOP_ID, Area_Type.PACK };
            _combo.SetCombo(cboDefectArea, CommonCombo.ComboStatus.NONE, sFilter: sFiltercboArea, sCase: "AREA_AREATYPE", cbChild: cboDefectAreaChild);

            //라인
            C1ComboBox[] cboInputEquipmentSegmentParent = { cboDefectArea };
            _combo.SetCombo(cboDefectEquipmentSegment, CommonCombo.ComboStatus.ALL, cbParent: cboInputEquipmentSegmentParent, cbChild: null, sCase: "EQUIPMENTSEGMENT");


        }
        #endregion

        #region [UserControl_Loaded]

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            dtpDateFromInput.SelectedDateTime = (DateTime)System.DateTime.Now.AddDays(-7);
            dtpDateToInput.SelectedDateTime = (DateTime)System.DateTime.Now;
            //SetHeaderLineBreak(dgBoxingList);
        }

        #endregion

        #region [Event]

        private void txtLotIDCr_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                GetDefectSearchList(txtLotIDCr.Text.Trim());
            }
        }

        private void txtLotIDCr_PreviewKeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == Key.V && Keyboard.IsKeyDown(Key.LeftCtrl))
            {
                try
                {
                    ShowLoadingIndicator();

                    string[] stringSeparators = new string[] { "\r\n" };
                    string sPasteString = Clipboard.GetText();
                    string[] sPasteStrings = sPasteString.Split(stringSeparators, StringSplitOptions.None);
                    string sParam = string.Empty;



                    for (int i = 0; i < sPasteStrings.Length; i++)
                    {
                        if (!string.IsNullOrWhiteSpace(sPasteStrings[i]))
                            sParam = sParam + string.Format(@"{0},", sPasteStrings[i]);
                    }
                    HiddenLoadingIndicator();

                    GetDefectSearchList(sParam.Trim());

                }
                catch (Exception ex)
                {
                    Util.MessageException(ex);
                    return;
                }
                finally
                {

                }

                e.Handled = true;
            }
        }

        private void btnDefectSearch_Click(object sender, RoutedEventArgs e)
        {
            GetDefectSearchList();
        }

        private void dgDefectHistoryList_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            Point pnt = e.GetPosition(null);
            C1.WPF.DataGrid.DataGridCell cell = dgDefectHistoryList.GetCellFromPoint(pnt);

            if (cell == null || cell.Value == null)
            {
                return;
            }

            //1.lot history 화면으로 link
            if (cell.Column.Name == "LOTID")
            {
                this.FrameOperation.OpenMenu("SFU010090090", true, cell.Text);
            }
        }

        #endregion

        #region [Func]

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


        private void DoEvents()
        {
            Application.Current.Dispatcher.Invoke(DispatcherPriority.Background, new System.Threading.ThreadStart(delegate { }));
        }


        public void GetDefectSearchList()
        {
            try
            {

                tbCellListCount.Text = "[ 0 " + ObjectDic.Instance.GetObjectName("건") + " ]";

                loadingIndicator.Visibility = Visibility.Visible;

                DataTable RQSTDT = new DataTable();

                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("AREAID", typeof(string));
                RQSTDT.Columns.Add("EQSGID", typeof(string));
                RQSTDT.Columns.Add("FROMDATE", typeof(string));
                RQSTDT.Columns.Add("TODATE", typeof(string));
                RQSTDT.Columns.Add("LOTID", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["AREAID"] = string.IsNullOrWhiteSpace(cboDefectArea.SelectedValue.ToString()) ? null : Util.GetCondition(cboDefectArea);
                dr["EQSGID"] = string.IsNullOrWhiteSpace(cboDefectEquipmentSegment.SelectedValue.ToString()) ? null : Util.GetCondition(cboDefectEquipmentSegment);
                dr["FROMDATE"] = Util.GetCondition(dtpDateFromInput);
                dr["TODATE"] = Util.GetCondition(dtpDateToInput);

                RQSTDT.Rows.Add(dr);

                new ClientProxy().ExecuteService("DA_PRD_SEL_WIP_FIRST_DEFECT_LIST", "RQSTDT", "RSLTDT", RQSTDT, (dtResult, ex) =>
                {

                    if (ex != null)
                    {
                        Util.MessageException(ex);
                        return;
                    }

                    Util.GridSetData(dgDefectHistoryList, dtResult, FrameOperation);

                    if (dtResult.Rows.Count != 0)
                    {
                        //Util.GridSetData(dgDefectHistoryList, dtResult, FrameOperation); //20250416

                        Util.SetTextBlockText_DataGridRowCount(tbCellListCount, Util.NVC(dtResult.Rows.Count));
                    }
                    loadingIndicator.Visibility = Visibility.Collapsed;
                });
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                loadingIndicator.Visibility = Visibility.Collapsed;
            }
        }

        public void GetDefectSearchList(string LotID)
        {
            try
            {

                tbCellListCount.Text = "[ 0 " + ObjectDic.Instance.GetObjectName("건") + " ]";

                loadingIndicator.Visibility = Visibility.Visible;

                DataTable RQSTDT = new DataTable();

                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("AREAID", typeof(string));
                RQSTDT.Columns.Add("EQSGID", typeof(string));
                RQSTDT.Columns.Add("FROMDATE", typeof(string));
                RQSTDT.Columns.Add("TODATE", typeof(string));
                RQSTDT.Columns.Add("LOTID", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["LOTID"] = LotID;
                RQSTDT.Rows.Add(dr);

                new ClientProxy().ExecuteService("DA_PRD_SEL_WIP_FIRST_DEFECT_LIST", "RQSTDT", "RSLTDT", RQSTDT, (dtResult, ex) =>
                {

                    if (ex != null)
                    {
                        Util.MessageException(ex);
                        return;
                    }

                    if (dtResult.Rows.Count != 0)
                    {
                        Util.GridSetData(dgDefectHistoryList, dtResult, FrameOperation);

                        Util.SetTextBlockText_DataGridRowCount(tbCellListCount, Util.NVC(dtResult.Rows.Count));

                        txtLotIDCr.Clear();
                    }
                    loadingIndicator.Visibility = Visibility.Collapsed;
                });
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                loadingIndicator.Visibility = Visibility.Collapsed;
            }
        }

        #endregion

        #region [엑셀 다운 로드]
        private void btnCellDownLoad_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                new LGC.GMES.MES.Common.ExcelExporter().Export_MergeHeader(dgDefectHistoryList);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        #endregion

        private void dgDefectHistoryList_LoadedCellPresenter(object sender, DataGridCellEventArgs e)
        {
            try
            {
                C1.WPF.DataGrid.C1DataGrid dataGrid = sender as C1.WPF.DataGrid.C1DataGrid;

                dataGrid.Dispatcher.BeginInvoke(new System.Action(() =>
                {
                    if (e.Cell.Presenter == null)
                    {
                        return;
                    }
                    if (e.Cell.Column.Name == "LOTID")
                    {
                        e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Blue);
                        e.Cell.Presenter.FontWeight = FontWeights.Bold;
                    }
                    else
                    {
                        e.Cell.Presenter.Foreground = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#FF333333"));
                        e.Cell.Presenter.FontWeight = FontWeights.Normal;
                    }
                }));
            }
            catch (Exception ex)
            {
                Util.Alert(ex.ToString());
            }
        }
    }


}

