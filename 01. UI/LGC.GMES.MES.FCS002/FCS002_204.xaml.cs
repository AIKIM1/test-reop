/*************************************************************************************
 Created Date : 2022.12.05
      Creator : 
   Decription : 강제 출고 예약 현황 조회
--------------------------------------------------------------------------------------
 [Change History]
  2022.12.05  DEVELOPER : Initial Created.
 
**************************************************************************************/
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

namespace LGC.GMES.MES.FCS002
{
    public partial class FCS002_204 : UserControl, IWorkArea
    {
        #region Declaration & Constructor 

        Util _Util = new Util();
        private string LANE_ID = string.Empty;
        private string EQPT_GR_TYPE_CODE = string.Empty;

        public FCS002_204()
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
            CommonCombo_Form_MB _combo = new CommonCombo_Form_MB();

            string[] sFilter1 = { "FORM_AGING_TYPE_CODE", "N" };
            _combo.SetCombo(cboAgingLoc, CommonCombo_Form_MB.ComboStatus.NONE, sCase: "SYSTEM_AREA_COMMON_CODE", sFilter: sFilter1);
        }
        #endregion

        #region Event
        private void cboAgingLoc_SelectionCommitted(object sender, PropertyChangedEventArgs<object> e)
        {
            CommonCombo_Form_MB _combo = new CommonCombo_Form_MB();
            GetCommonCode();

            cboSCLine.Text = string.Empty;
            string[] sFilter = { EQPT_GR_TYPE_CODE, LANE_ID };
            _combo.SetCombo(cboSCLine, CommonCombo_Form_MB.ComboStatus.NONE, sCase: "SCLINE", sFilter: sFilter); //20210331 S/C 호기 필수 값으로 변경
        }
        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                InitCombo();


                this.Loaded -= UserControl_Loaded;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void GetCommonCode()
        {
            try
            {
                LANE_ID = string.Empty;
                EQPT_GR_TYPE_CODE = string.Empty;

                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("AREAID", typeof(string));
                RQSTDT.Columns.Add("COM_TYPE_CODE", typeof(string));
                RQSTDT.Columns.Add("COM_CODE", typeof(string));
                RQSTDT.Columns.Add("USE_FLAG", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["AREAID"] = LoginInfo.CFG_AREA_ID;
                dr["COM_TYPE_CODE"] = "FORM_AGING_TYPE_CODE";
                dr["COM_CODE"] = Util.GetCondition(cboAgingLoc);
                dr["USE_FLAG"] = "Y";
                RQSTDT.Rows.Add(dr);
                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_SYSTEM_AREA_COM_CODE_USE", "RQSTDT", "RSLTDT", RQSTDT);
                foreach (DataRow row in dtResult.Rows)
                {
                    EQPT_GR_TYPE_CODE = row["ATTR1"].ToString();
                    LANE_ID = row["ATTR2"].ToString();
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            GetList();
        }

        private void btnFOutCancel_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                DataSet ds = new DataSet();
                DataTable dtInData = ds.Tables.Add("INDATA");
                dtInData.Columns.Add("USERID", typeof(string));
                dtInData.Columns.Add("LOTID", typeof(string));

                for (int i = 2; i < dgList.Rows.Count; i++)
                {
                    if (Util.NVC(DataTableConverter.GetValue(dgList.Rows[i].DataItem, "CHK")).Equals("True") ||
                    Util.NVC(DataTableConverter.GetValue(dgList.Rows[i].DataItem, "CHK")).Equals("1"))
                    {
                        DataRow drIn = dtInData.NewRow();
                        drIn["USERID"] = LoginInfo.USERID;
                        drIn["LOTID"] = Util.NVC(DataTableConverter.GetValue(dgList.Rows[i].DataItem, "LOTID"));
                        dtInData.Rows.Add(drIn);
                    }
                }

                if (dtInData.Rows.Count == 0)
                {
                    //선택된 데이터가 없습니다.
                    LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("FM_ME_0165"), null, "Information", MessageBoxButton.OK, MessageBoxIcon.None, (result) =>
                    {
                        if (result == MessageBoxResult.OK)
                        { }
                    });
                    return;
                }

                //강제출고 취소 하시겠습니까?
                Util.MessageConfirm("FM_ME_0461", (result) =>
                {
                    if (result == MessageBoxResult.OK)
                    {
                        new ClientProxy().ExecuteService_Multi("BR_SET_TRAY_FORCE_OUT_CANCEL_MB", "INDATA", "OUTDATA", (bizResult, bizException) =>
                        {
                            try
                            {
                                if (bizException != null)
                                {
                                    Util.MessageException(bizException);
                                    return;
                                }

                                //강제출고를 취소했습니다.
                                Util.MessageInfo("FM_ME_0462");
                                GetList();

                            }
                            catch (Exception ex)
                            {
                                Util.MessageException(ex);
                            }
                            finally
                            {
                                loadingIndicator.Visibility = Visibility.Collapsed;
                            }
                        }, ds);

                    }
                });
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }

        private void dgList_LoadedRowHeaderPresenter(object sender, C1.WPF.DataGrid.DataGridRowEventArgs e)
        {
            if (e.Row.HeaderPresenter == null)
            {
                return;
            }

            e.Row.HeaderPresenter.Content = null;
            C1DataGrid dataGrid = e.Row.DataGrid as C1DataGrid;
            TextBlock tb = new TextBlock();

            if (e.Row.Index - (dataGrid.TopRows.Count - 1) > 0 && e.Row.Index <= dataGrid.Rows.Count - 1)
            {
                tb.Text = (e.Row.Index + 1 - dataGrid.TopRows.Count).ToString();
                tb.VerticalAlignment = VerticalAlignment.Center;
                tb.HorizontalAlignment = HorizontalAlignment.Center;
                e.Row.HeaderPresenter.Content = tb;
            }
        }
        #endregion

        #region Method
        private void GetList()
        {
            try
            {
                DataTable inDataTable = new DataTable();
                inDataTable.Columns.Add("AREAID", typeof(string));
                inDataTable.Columns.Add("AGING_TYPE", typeof(string));
                inDataTable.Columns.Add("LANGID", typeof(string));
                inDataTable.Columns.Add("EQP_ID", typeof(string));

                DataRow newRow = inDataTable.NewRow();

                newRow["AREAID"] = LoginInfo.CFG_AREA_ID;
                newRow["AGING_TYPE"] = Util.GetCondition(cboAgingLoc, bAllNull: true);
                newRow["LANGID"] = LoginInfo.LANGID;
                newRow["EQP_ID"] = Util.GetCondition(cboSCLine);

                inDataTable.Rows.Add(newRow);
                ShowLoadingIndicator();

                // BIZ수정 필요 INDATA에 EQPTID 추가 
                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_SEL_AGING_FORCED_OUT_RESERV_LIST_MB", "INDATA", "OUTDATA", inDataTable);
                dgList.ItemsSource = DataTableConverter.Convert(dtRslt);
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