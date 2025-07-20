/*************************************************************************************
 Created Date : 2020.08.11
      Creator : 최우석
   Decription : 자재 Port 설정 현황(Pack)
--------------------------------------------------------------------------------------
 [Change History]
  2020.08.00  담당자 : Initial Created.

**************************************************************************************/

using C1.WPF;
using C1.WPF.DataGrid;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using System;
using System.Data;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace LGC.GMES.MES.PACK001
{
    public partial class PACK002_004 : UserControl, IWorkArea
    {
        #region Declaration & Constructor 

        public PACK002_004()
        {
            InitializeComponent();

            InitCombo();
        }

        private void InitCombo()
        {
            try
            {
                CommonCombo _combo = new CommonCombo();

                _combo.SetCombo(cboArea, CommonCombo.ComboStatus.NONE, cbChild: new C1ComboBox[] { cboLine });
                _combo.SetCombo(cboLine, CommonCombo.ComboStatus.ALL, cbParent: new C1ComboBox[] { cboArea });
                //포트 자재 그룹 콤보
                PortMtrlGroupCombo();
                cboArea.SelectedValue = LoginInfo.CFG_AREA_ID;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void PortMtrlGroupCombo()
        {
            try
            {
                DataTable dtIndata = new DataTable();
                DataTable dtAllRow = new DataTable();
                dtAllRow.Columns.AddRange(new DataColumn[] { new DataColumn("CBO_CODE"), new DataColumn("CBO_NAME") });

                dtIndata.TableName = "RQSTDT";
                dtIndata.Columns.Add("LANGID", typeof(string));
                dtIndata.Columns.Add("AREAID", typeof(string));

                DataRow dr = dtIndata.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["AREAID"] = LoginInfo.CFG_AREA_ID;

                dtIndata.Rows.Add(dr);

                dtAllRow.Rows.Add(new object[] { "", "-ALL-" });

                cboPortMtrlGrp.DisplayMemberPath = "CBO_NAME";
                cboPortMtrlGrp.SelectedValuePath = "CBO_CODE";

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_MTRL_SEL_GR_CODE_CBO", "RQSTDT", "RSLTDT", dtIndata);

                if (dtResult != null && dtResult.Rows.Count > 0)
                {
                    dtAllRow.Merge(dtResult);
                }

                cboPortMtrlGrp.ItemsSource = DataTableConverter.Convert(dtAllRow);
                cboPortMtrlGrp.SelectedIndex = 0;

            }
            catch 
            {

            }
        }

        public IFrameOperation FrameOperation
        {
            get;
            set;
        }

#endregion

#region Event
        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                SearchData();
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        //왼쪽 조회 더블클릭 처리
        private void dgSearchResult_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            try
            {
                if (dgSearchResult.GetRowCount() == 0) return;

                Point pnt = e.GetPosition(null);
                C1.WPF.DataGrid.DataGridCell cell = dgSearchResult.GetCellFromPoint(pnt);

                if (cell == null || cell.Value == null) return;

                if (cell.Column.Name.Equals("PORT_MTRL_GR_CODE"))
                {
                    int seleted_row = dgSearchResult.CurrentRow.Index;

                    string sEqsgid = string.Empty;
                    string sMtrlGrCode = string.Empty;

                    sEqsgid = DataTableConverter.GetValue(dgSearchResult.Rows[seleted_row].DataItem, "EQSGID").ToString();
                    sMtrlGrCode = DataTableConverter.GetValue(dgSearchResult.Rows[seleted_row].DataItem, "PORT_MTRL_GR_CODE").ToString();

                    SearchDetailData(sEqsgid, sMtrlGrCode);
                }

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }


        //왼쪽 조회결과 처리
        private void dgSearchResult_LoadedCellPresenter(object sender, C1.WPF.DataGrid.DataGridCellEventArgs e)
        {
            try
            {
                C1DataGrid dataGrid = sender as C1DataGrid;

                if (e.Cell.Presenter == null) return;
                if (e.Cell.Row.Type != DataGridRowType.Item) return;
                if (dgSearchResult == null) return;

                dataGrid?.Dispatcher.BeginInvoke(new Action(() =>
                {
                    if (e.Cell.Column.Name == "PORT_MTRL_GR_CODE")
                    {
                        if (e.Cell.Text.Equals("N/A"))
                        {
                            e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Red);
                            e.Cell.Presenter.FontWeight = FontWeights.Bold;
                        }
                        else
                        {
                            e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Blue);
                            e.Cell.Presenter.FontWeight = FontWeights.Bold;
                        }

                    }
                    else
                    {
                        if (e.Cell.Text.Equals("N/A"))
                        {
                            e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Red);
                            e.Cell.Presenter.FontWeight = FontWeights.Bold;
                        }
                        else
                        {
                            e.Cell.Presenter.Foreground = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#FF333333"));
                            e.Cell.Presenter.FontWeight = FontWeights.Normal;
                        }
                    }
                }));
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

#endregion

#region Mehod
        //DA_MTRL_SEL_MMD_MTRL_PORT_UI
        private void SearchData()
        {
            try
            {
                Util.gridClear(dgDetailResult);
                Util.gridClear(dgDetailResult);

                DataTable dtIndata = new DataTable();
                dtIndata.TableName = "RQSTDT";
                dtIndata.Columns.Add("LANGID", typeof(string));
                dtIndata.Columns.Add("SHOPID", typeof(string));
                dtIndata.Columns.Add("AREAID", typeof(string));
                dtIndata.Columns.Add("EQSGID", typeof(string));
                dtIndata.Columns.Add("PORT_MTRL_GR_CODE", typeof(string));

                DataRow dr = dtIndata.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["SHOPID"] = null;

                dr["AREAID"] = cboArea.SelectedValue.ToString();
                dr["EQSGID"] = cboLine.SelectedValue.ToString();

                dr["PORT_MTRL_GR_CODE"] = string.IsNullOrWhiteSpace(cboPortMtrlGrp.SelectedValue.ToString()) ? null : cboPortMtrlGrp.SelectedValue.ToString(); //cboLine.SelectedValue.ToString();
                dtIndata.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_MTRL_SEL_MMD_MTRL_PORT_UI", "RQSTDT", "RSLTDT", dtIndata);

                Util.gridClear(dgSearchResult);
                Util.gridClear(dgDetailResult);


                if (dtResult != null && dtResult.Rows.Count > 0)
                {
                    Util.GridSetData(dgSearchResult, dtResult, FrameOperation);
                }

                Util.SetTextBlockText_DataGridRowCount(tbDetail_cnt, Util.NVC(dgDetailResult.GetRowCount()));
                Util.SetTextBlockText_DataGridRowCount(tbResult_cnt, Util.NVC(dgSearchResult.GetRowCount()));
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }


        //DA_MTRL_SEL_MMD_MTRL_PORT_GR_CODE_UI
        private void SearchDetailData(string sEqsgid, string sMtrlGrCode)
        {
            try
            {
                Util.gridClear(dgDetailResult);

                DataTable dtIndata = new DataTable();
                dtIndata.TableName = "RQSTDT";
                dtIndata.Columns.Add("LANGID", typeof(string));
                dtIndata.Columns.Add("PORT_MTRL_GR_CODE", typeof(string));
                dtIndata.Columns.Add("EQSGID", typeof(string));

                DataRow dr = dtIndata.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["PORT_MTRL_GR_CODE"] = sMtrlGrCode;
                dr["EQSGID"] = sEqsgid;
                dtIndata.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_MTRL_SEL_MMD_MTRL_PORT_GR_CODE_UI", "RQSTDT", "RSLTDT", dtIndata);

                if (dtResult != null && dtResult.Rows.Count > 0)
                {
                    Util.GridSetData(dgDetailResult, dtResult, FrameOperation);
                }

                Util.SetTextBlockText_DataGridRowCount(tbDetail_cnt, Util.NVC(dgDetailResult.GetRowCount()));

            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

#endregion


    }
}
