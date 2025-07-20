/*************************************************************************************
 Created Date : 2020.03.13
      Creator : 
   Decription : Aging Rack 사용현황
--------------------------------------------------------------------------------------
 [Change History]
  2020.03.13  DEVELOPER : 김태균.
  2020.04.20  김태균    : QA Sampling 컬럼 추가
  2022.02.22  KDH       : AREA 조건 추가
  2022.11.22  이정미    : cboAgingType 콤보박스 수정
**************************************************************************************/
#define SAMPLE_DEV
//#undef SAMPLE_DEV

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
    public partial class FCS001_016 : UserControl, IWorkArea
    {
        #region Declaration & Constructor 

        Util _Util = new Util();

        public FCS001_016()
        {
            InitializeComponent();
        }
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

        //화면내 combo 셋팅
        private void InitCombo()
        {
            CommonCombo_Form _combo = new CommonCombo_Form();

            string[] sFilter = { "FORM_AGING_TYPE_CODE", "N" };
            _combo.SetCombo(cboAgingType, CommonCombo_Form.ComboStatus.ALL, sCase: "SYSTEM_AREA_COMMON_CODE", sFilter: sFilter);
            _combo.SetCombo(cboModel, CommonCombo_Form. ComboStatus.ALL, sCase: "MODEL");
        }

        #endregion

        #region Event

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            InitCombo();
        }

        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            GetList();
        }

        private void dgRackUse_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            try
            {
                string sEqptGroupTypeCode = string.Empty;
                string sLaneID = string.Empty;

                if (dgRackUse.ItemsSource == null)
                    return;

                if (sender == null)
                    return;

                if (dgRackUse.CurrentRow != null && dgRackUse.CurrentColumn.Name.Equals("TRAY_CNT"))
                {
                    FCS001_016_TRAY_LIST wndRunStart = new FCS001_016_TRAY_LIST();
                    wndRunStart.FrameOperation = FrameOperation;

                    if (wndRunStart != null)
                    {
                        object[] Parameters = new object[7];
                        Parameters[0] = Util.NVC(DataTableConverter.GetValue(dgRackUse.CurrentRow.DataItem, "MDLLOT_ID"));
                        Parameters[1] = Util.NVC(DataTableConverter.GetValue(dgRackUse.CurrentRow.DataItem, "EQSGID"));
                        Parameters[2] = Util.NVC(DataTableConverter.GetValue(dgRackUse.CurrentRow.DataItem, "DEGAS_AB"));
                        Parameters[3] = Util.NVC(DataTableConverter.GetValue(dgRackUse.CurrentRow.DataItem, "PROCID"));
                        Parameters[4] = Util.NVC(DataTableConverter.GetValue(dgRackUse.CurrentRow.DataItem, "DAN_CNT"));

                        //GetAttr(ref sEqptGroupTypeCode, ref sLaneID);

                        Parameters[5] = Util.NVC(DataTableConverter.GetValue(dgRackUse.CurrentRow.DataItem, "EQPT_GR_TYPE_CODE"));
                        Parameters[6] = Util.NVC(DataTableConverter.GetValue(dgRackUse.CurrentRow.DataItem, "LANE_ID"));

                        C1WindowExtension.SetParameters(wndRunStart, Parameters);

                        wndRunStart.ShowModal();
                    }
                }
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }

        private void dgRackUse_LoadedCellPresenter(object sender, DataGridCellEventArgs e)
        {
            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (e.Cell.Presenter == null) return;

                if (string.IsNullOrEmpty(e.Cell.Column.Name) == false)
                {
                    if (e.Cell.Column.Name.Equals("TRAY_CNT"))
                    {
                        e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Blue);
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
                string sEqptGroupTypeCode = string.Empty;
                string sLaneID = string.Empty;

                DataTable dtRqst = new DataTable();
                dtRqst.Columns.Add("LANGID", typeof(string));
                dtRqst.Columns.Add("AREAID", typeof(string)); //2022.02.22_AREA 조건 추가
                dtRqst.Columns.Add("EQPT_GR_TYPE_CODE", typeof(string));
                dtRqst.Columns.Add("LANE_ID", typeof(string));
                dtRqst.Columns.Add("MDLLOT_ID", typeof(string));

                DataRow dr = dtRqst.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["AREAID"] = LoginInfo.CFG_AREA_ID; //2022.02.22_AREA 조건 추가

                if (!string.IsNullOrEmpty(Util.GetCondition(cboAgingType, bAllNull: true)))
                {
                    GetAttr(ref sEqptGroupTypeCode, ref sLaneID);

                    dr["EQPT_GR_TYPE_CODE"] = sEqptGroupTypeCode;
                    dr["LANE_ID"] = sLaneID;
                }
                dr["MDLLOT_ID"] = Util.GetCondition(cboModel, bAllNull: true);
                dtRqst.Rows.Add(dr);

                ShowLoadingIndicator();
                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_SEL_AGING_RACK_SUMMARY", "INDATA", "OUTDATA", dtRqst);

                Util.GridSetData(dgRackUse, dtRslt, FrameOperation, true);
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
            finally
            {
                HiddenLoadingIndicator();
            }
        }

        private void GetAttr(ref string sEqptGroupTypeCode, ref string sLaneID)
        {
            try
            {
                DataTable dtRqst = new DataTable();
                dtRqst.Columns.Add("LANGID", typeof(string));
                dtRqst.Columns.Add("AREAID", typeof(string));
                dtRqst.Columns.Add("COM_TYPE_CODE", typeof(string));
                dtRqst.Columns.Add("COM_CODE", typeof(string));
                dtRqst.Columns.Add("USE_FLAG", typeof(string));

                DataRow dr = dtRqst.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["AREAID"] = LoginInfo.CFG_AREA_ID;
                dr["COM_TYPE_CODE"] = "FORM_AGING_TYPE_CODE";
                dr["COM_CODE"] = cboAgingType.SelectedValue.ToString();
                dr["USE_FLAG"] = "Y";
                dtRqst.Rows.Add(dr);

                ShowLoadingIndicator();
                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_AREA_COM_CODE_USE", "INDATA", "OUTDATA", dtRqst);

                if (dtRslt.Rows.Count > 0)
                {
                    sEqptGroupTypeCode = dtRslt.Rows[0]["ATTR1"].ToString();
                    sLaneID = dtRslt.Rows[0]["ATTR2"].ToString();
                }
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
            finally
            {
                HiddenLoadingIndicator();
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

        
    }
}
