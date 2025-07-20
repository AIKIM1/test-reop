/*************************************************************************************
 Created Date : 2016.06.16
      Creator : 
   Decription : 
--------------------------------------------------------------------------------------
 [Change History]
  2016.06.16  DEVELOPER : Initial Created.

**************************************************************************************/


using C1.WPF;
using C1.WPF.DataGrid;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace LGC.GMES.MES.PACK001
{
    /// <summary>
    /// PACK003_047.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class PACK002_005 : UserControl, IWorkArea
    {
        LGC.GMES.MES.PACK001.Class.MESSAGE_PARAM ms = new LGC.GMES.MES.PACK001.Class.MESSAGE_PARAM();
        Util _util = new Util();
        CommonCombo _combo = new CMM001.Class.CommonCombo();

        public PACK002_005()
        {
            InitializeComponent();
            Loaded += PACK002_005_Loaded;
        }


        private void PACK002_005_Loaded(object sender, RoutedEventArgs e)
        {
            Loaded -= PACK002_005_Loaded;
            SetComboBox();
        }

        public IFrameOperation FrameOperation
        {
            get;
            set;
        }

        // 다시 해야함.
        private void SetComboBox()
        {
            C1ComboBox cboAreaByAreaType = new C1ComboBox();
            cboAreaByAreaType.SelectedValue = LoginInfo.CFG_AREA_ID;

            C1ComboBox cboLangSet = new C1ComboBox();
            cboLangSet.SelectedValue = LoginInfo.LANGID;

            string[] cboMtrlGroupFilterParam = { "EQSGID" };

            C1ComboBox[] cboEquipmentSegmentParent = { cboAreaByAreaType };
            C1ComboBox[] cboMtrlGroup01Parent = { cboEquipmentSegment };

            //날짜 콤보 셋
            dtpTmDateFrom.SelectedDateTime = (DateTime)System.DateTime.Now.AddDays(-1);
            dtpTmDateTo.SelectedDateTime = (DateTime)System.DateTime.Now;

            dtpTmDateFrom.SelectedDateTime = (DateTime)System.DateTime.Now;
            dtpTmDateTo.SelectedDateTime = (DateTime)System.DateTime.Now;

            //라인 셋팅
            _combo.SetCombo(cboEquipmentSegment, CommonCombo.ComboStatus.SELECT, cbParent: cboEquipmentSegmentParent, sCase: "EQUIPMENTSEGMENT");

            //자재 그룹 셋팅
            SetMtrlGroupCode(cboMtrlGroup, CommonCombo.ComboStatus.ALL, cbParent: cboMtrlGroup01Parent, sFilter: cboMtrlGroupFilterParam);

        }

        private void SetMtrlGroupCode(C1ComboBox cbo, CommonCombo.ComboStatus cs, C1ComboBox[] cbParent = null, string[] sFilter = null)
        {
            try
            {
                if (cbParent != null)
                {
                    C1ComboBox[] cbArray = cbParent;
                    int i = 0;
                    for (i = 0; i < cbArray.Length; i++)
                    {
                        if (cbArray[i].SelectedValue != null)
                        {
                            sFilter[i] = cbArray[i].SelectedValue.ToString();
                        }

                    }
                }


                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("AREAID", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["AREAID"] = LoginInfo.CFG_AREA_ID;
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_MTRL_SEL_GR_CODE_CBO", "RQSTDT", "RSLTDT", RQSTDT);

                cbo.DisplayMemberPath = "CBO_NAME";
                cbo.SelectedValuePath = "CBO_CODE";
                cbo.ItemsSource = AddStatus(dtResult, cs, "CBO_CODE", "CBO_NAME").Copy().AsDataView();

                if (cbo.SelectedIndex < 0)
                {
                    cbo.SelectedIndex = 0;
                }
                else
                {
                    cbo.SelectedIndex = 0;
                }
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, ControlsLibrary.MessageBoxIcon.Warning);
            }
        }
        private DataTable AddStatus(DataTable dt, CommonCombo.ComboStatus cs, string sValue, string sDisplay)
        {
            DataRow dr = dt.NewRow();

            switch (cs)
            {
                case CommonCombo.ComboStatus.ALL:
                    dr[sDisplay] = "-ALL-";
                    dr[sValue] = "";
                    dt.Rows.InsertAt(dr, 0);
                    break;

                case CommonCombo.ComboStatus.SELECT:
                    dr[sDisplay] = "-SELECT-";
                    dr[sValue] = "SELECT";
                    dt.Rows.InsertAt(dr, 0);
                    break;

                case CommonCombo.ComboStatus.NA:
                    dr[sDisplay] = "-N/A-";
                    dr[sValue] = "";
                    dt.Rows.InsertAt(dr, 0);
                    break;
            }

            return dt;
        }

        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                TimeSpan timeSpan = dtpTmDateTo.SelectedDateTime.Date - dtpTmDateFrom.SelectedDateTime.Date;
                if (timeSpan.Days > 7)
                {
                    //dtpTmDateTo.SelectedDateTime = dtpTmDateFrom.SelectedDateTime.Date.AddDays(+7);
                    Util.MessageValidation("SFU3567");  // 조회기간은 7일을 초과할 수 없습니다.
                    return;
                }
                if (Convert.ToString(cboEquipmentSegment.SelectedValue) == "SELECT")
                {
                    Util.MessageInfo("SFU1223");    //라인을 선택하세요
                    return;
                }
                SboxHistoryList();
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void txtSearch_MtrlCode_KeyDown(object sender, KeyEventArgs e)
        {
            try
            {
                if (e.Key == Key.Enter)
                {
                    TimeSpan timeSpan = dtpTmDateTo.SelectedDateTime.Date - dtpTmDateFrom.SelectedDateTime.Date;
                    if (timeSpan.Days > 7)
                    {
                        //dtpTmDateTo.SelectedDateTime = dtpTmDateFrom.SelectedDateTime.Date.AddDays(+7);
                        Util.MessageValidation("SFU3567"); // 조회기간은 7일을 초과할 수 없습니다.
                        return;
                    }
                    if (Convert.ToString(cboEquipmentSegment.SelectedValue) == "SELECT")
                    {
                        Util.MessageInfo("SFU1223");   // 라인을 선택하세요
                        return;
                    }
                    SboxHistoryList();
                }
            }
            catch (Exception ex)
            {
                Util.AlertInfo(ex.Message);
                return;
            }
        }

        private void SboxHistoryList()
        {
            Util.gridClear(dgReturnSummaryList);
            Util.gridClear(dgReturnDetailsList);

            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("DATE_FROM", typeof(string));
                RQSTDT.Columns.Add("DATE_TO", typeof(string));
                RQSTDT.Columns.Add("EQSGID", typeof(string));
                RQSTDT.Columns.Add("PORT_MTRL_GR_CODE", typeof(string));
                RQSTDT.Columns.Add("MTRLID", typeof(string));


                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["DATE_FROM"] = dtpTmDateFrom.SelectedDateTime.ToString("yyyyMMdd");
                dr["DATE_TO"] = dtpTmDateTo.SelectedDateTime.ToString("yyyyMMdd");
                dr["EQSGID"] = Convert.ToString(cboEquipmentSegment.SelectedValue).Equals("") ? null : Convert.ToString(cboEquipmentSegment.SelectedValue);
                dr["PORT_MTRL_GR_CODE"] = Convert.ToString(cboMtrlGroup.SelectedValue).Equals("") ? null : Convert.ToString(cboMtrlGroup.SelectedValue);
                dr["MTRLID"] = string.IsNullOrWhiteSpace(txtSearch_MtrlCode.Text) ? null : txtSearch_MtrlCode.Text;
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_MTRL_SEL_SBOX_INPUT_SUMMARY", "RQSTDT", "RSLTDT", RQSTDT);
                if (dtResult.Rows.Count > 0)
                {
                    Util.GridSetData(dgReturnSummaryList, dtResult, FrameOperation, true);
                }
                else
                {
                    Util.MessageInfo("SFU8927");    // 소포장 투입 이력이 존재하지 않습니다.
                }


            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private void dgSummary_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            Util.gridClear(dgReturnDetailsList);
            try
            {
                Point pnt = e.GetPosition(null);
                C1.WPF.DataGrid.DataGridCell cell = dgReturnSummaryList.GetCellFromPoint(pnt);
                if (cell != null && !cell.Value.ToString().Equals("0"))
                {
                    string sDATE_FROM = Util.NVC(DataTableConverter.GetValue(dgReturnSummaryList.Rows[cell.Row.Index].DataItem, "DATE_FROM"));
                    string sDATE_TO = Util.NVC(DataTableConverter.GetValue(dgReturnSummaryList.Rows[cell.Row.Index].DataItem, "DATE_TO"));
                    string sEQSGID = Util.NVC(DataTableConverter.GetValue(dgReturnSummaryList.Rows[cell.Row.Index].DataItem, "EQSGID"));
                    string sPORT_MTRL_GR_CODE = Util.NVC(DataTableConverter.GetValue(dgReturnSummaryList.Rows[cell.Row.Index].DataItem, "PORT_MTRL_GR_CODE"));
                    string sMTRLID = Util.NVC(DataTableConverter.GetValue(dgReturnSummaryList.Rows[cell.Row.Index].DataItem, "MTRLCODE"));

                    string sWIPSTAT_TYPE_CODE = Util.NVC(cell.Column.Name.ToString());

                    if (sWIPSTAT_TYPE_CODE == "INPUT_BOX_CNT" || sWIPSTAT_TYPE_CODE == "INPUT_TOT_CNT")
                    {
                        string sResult_WIPSTAT_TYPE_CODE = Util.NVC(DataTableConverter.GetValue(dgReturnSummaryList.Rows[cell.Row.Index].DataItem, "INPUT_ACTID"));
                        getWipList_Detail_Procedure(sDATE_FROM, sDATE_TO, sEQSGID, sPORT_MTRL_GR_CODE, sMTRLID, sResult_WIPSTAT_TYPE_CODE);
                        return;
                    }
                    if (sWIPSTAT_TYPE_CODE == "REINPUT_BOX_CNT" || sWIPSTAT_TYPE_CODE == "REINPUT_TOT_CNT")
                    {
                        string sResult_WIPSTAT_TYPE_CODE = Util.NVC(DataTableConverter.GetValue(dgReturnSummaryList.Rows[cell.Row.Index].DataItem, "REINPUT_ACTID"));
                        getWipList_Detail_Procedure(sDATE_FROM, sDATE_TO, sEQSGID, sPORT_MTRL_GR_CODE, sMTRLID, sResult_WIPSTAT_TYPE_CODE);
                        return;
                    }
                    if (sWIPSTAT_TYPE_CODE == "RETURN_BOX_CNT" || sWIPSTAT_TYPE_CODE == "RETURN_TOT_CNT")
                    {
                        string sResult_WIPSTAT_TYPE_CODE = Util.NVC(DataTableConverter.GetValue(dgReturnSummaryList.Rows[cell.Row.Index].DataItem, "RETURN_ACTID"));
                        getWipList_Detail_Procedure(sDATE_FROM, sDATE_TO, sEQSGID, sPORT_MTRL_GR_CODE, sMTRLID, sResult_WIPSTAT_TYPE_CODE);
                        return;
                    }
                    Util.gridClear(dgReturnDetailsList);
                }
            }
            catch (Exception ex)
            {
                Util.Alert(ex.ToString());
            }
        }

        private void getWipList_Detail_Procedure(string sDATE_FROM, string sDATE_TO, string sEQSGID, string sPORT_MTRL_GR_CODE, string sMTRLID, string sFINL_ACTID)
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("FINL_ACTID", typeof(string));
                RQSTDT.Columns.Add("DATE_FROM", typeof(string));
                RQSTDT.Columns.Add("DATE_TO", typeof(string));
                RQSTDT.Columns.Add("EQSGID", typeof(string));
                RQSTDT.Columns.Add("PORT_MTRL_GR_CODE", typeof(string));
                RQSTDT.Columns.Add("MTRLID", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["FINL_ACTID"] = sFINL_ACTID;
                dr["DATE_FROM"] = sDATE_FROM;
                dr["DATE_TO"] = sDATE_TO;
                dr["EQSGID"] = sEQSGID;
                dr["PORT_MTRL_GR_CODE"] = sPORT_MTRL_GR_CODE;
                dr["MTRLID"] = sMTRLID;
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_MTRL_SEL_SBOX_INPUT_DETAIL", "RQSTDT", "RSLTDT", RQSTDT);
                Util.GridSetData(dgReturnDetailsList, dtResult, FrameOperation, true);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private void dgSummary_LoadedCellPresenter(object sender, C1.WPF.DataGrid.DataGridCellEventArgs e)
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
                    if (e.Cell.Column.Name == "INPUT_BOX_CNT" || e.Cell.Column.Name == "INPUT_TOT_CNT" ||
                        e.Cell.Column.Name == "REINPUT_BOX_CNT" || e.Cell.Column.Name == "REINPUT_TOT_CNT" ||
                        e.Cell.Column.Name == "RETURN_BOX_CNT" || e.Cell.Column.Name == "RETURN_TOT_CNT")
                    {
                        e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Blue);
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
                throw ex;
            }
        }
    }
}
