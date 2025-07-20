/*************************************************************************************
 Created Date : 2024.07.06
      Creator : 
   Decription : 입출고관리 > 활성화 인계
--------------------------------------------------------------------------------------
 [Change History]
  2024.07.06  오수현 : E20240620-001415 : Initial Created.        





**************************************************************************************/
using System;
using System.Data;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Linq;
using C1.WPF.DataGrid;
using System.Windows.Media;

using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;

namespace LGC.GMES.MES.BOX001
{
    public partial class BOX001_341 : UserControl, IWorkArea
    {
        #region Declaration & Constructor 

        Util _Util = new Util();
        BizDataSet _Biz = new BizDataSet();



        public BOX001_341()
        {
            InitializeComponent();
        }
        public IFrameOperation FrameOperation
        {
            get;
            set;
        }


        private void UserControl_Initialized(object sender, EventArgs e)
        {
            Initialize();
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            List<Button> listAuth = new List<Button>();
            Util.pageAuth(listAuth, FrameOperation.AUTHORITY);
            this.Loaded -= UserControl_Loaded;
        }

        #endregion

        #region Initialize
        private void Initialize()
        {
        }

        #endregion

        #region Event

        private void dgTagetListCellPresenter(object sender, DataGridCellEventArgs e)
        {

        }

        // 인계일자 Load
        private void dtpDateFrom_Loaded(object sender, RoutedEventArgs e)
        {
            LGCDatePicker datePicker = sender as LGCDatePicker;
            datePicker.SelectedDateTime = DateTime.Today.AddDays(-7); //일주일
        }

        // 활성화 인계 - PalletID
        private void txtPalletID1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                getTagetListSearch();
            }
        }
        // 이력조회 - PalletID
        private void txtPalletID2_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                getHistorySearch();
            }
        }


        // 활성화 인계 - 조회
        private void btnSearch1_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                getTagetListSearch();
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        // 이력조회 - 조회
        private void btnSearch2_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!dtDateCompare()) return;

                getHistorySearch();
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }


        // 활성화 인계 - 인계 버튼
        private void btnReceive_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (dgTargetList.GetRowCount() == 0)
                {
                    Util.MessageValidation("SFU1498");   //데이터가 없습니다.
                    return;
                }

                if (_Util.GetDataGridCheckCnt(dgTargetList, "CHK") <= 0)
                {
                    Util.AlertInfo("SFU1408"); //Pallet ID를 선택을 하신 후 버튼을 클릭해주십시오
                    return;
                }

                DataTable dt = DataTableConverter.Convert(dgTargetList.ItemsSource);

                var serWareHousingList = dt.AsEnumerable().Where(row => Convert.ToBoolean(row["CHK"]) == true);

                DataTable dtPallet = serWareHousingList.CopyToDataTable();

                ShowLoadingIndicator();

                //인계처리 하시겠습니까?
                Util.MessageConfirm("SFU2931", (result) =>
                {
                    if (result == MessageBoxResult.OK)
                    {
                        DataSet inData = new DataSet();

                        //마스터 정보
                        DataTable INDATA = new DataTable();
                        INDATA.TableName = "INDATA";
                        INDATA.Columns.Add("SRCTYPE", typeof(string));
                        INDATA.Columns.Add("IFMODE", typeof(string));
                        INDATA.Columns.Add("USERID", typeof(string));
                        DataRow drINDATA = null;

                        drINDATA = INDATA.NewRow();
                        drINDATA["SRCTYPE"] = "UI";
                        drINDATA["IFMODE"] = "OFF";
                        drINDATA["USERID"] = LoginInfo.USERID;

                        INDATA.Rows.Add(drINDATA);


                        DataSet dsIndata = new DataSet();
                        dsIndata.Tables.Add(INDATA);


                        //LOT 정보
                        DataTable INLOT = new DataTable();
                        INLOT.TableName = "INLOT";
                        INLOT.Columns.Add("PALLETID", typeof(string));

                        for (int i = 0; i < dtPallet.Rows.Count; i++)
                        {
                            DataRow drINLOT = null;

                            drINLOT = INLOT.NewRow();
                            drINLOT["PALLETID"] = Util.NVC(dtPallet.Rows[i]["PALLETID"]);
                            INLOT.Rows.Add(drINLOT);

                            dsIndata.Tables.Add(INLOT);
                        }

                        try
                        {
                            new ClientProxy().ExecuteService_Multi("BR_PRD_REG_MOVE_RECEIVE_CELL_PLLT", "INDATA,INLOT", null, (bizResult, bizException) =>
                            {
                                if (bizException != null)
                                {
                                    Util.MessageException(bizException);
                                    return;
                                }

                                Util.gridClear(dgTargetList);
                                txtPalletID1.Text = string.Empty;

                                DataTable dtBefore = DataTableConverter.Convert(dgTargetList.ItemsSource);
                                Util.SetTextBlockText_DataGridRowCount(tbTagetListCount, Util.NVC(dtBefore.Rows.Count));

                                Util.MessageInfo("SFU4165");   // 이동하였습니다

                            }, dsIndata);
                        }
                        catch (Exception ex)
                        {
                            Util.AlertByBiz("BR_PRD_REG_MOVE_RECEIVE_CELL_PLLT", ex.Message, ex.ToString());
                        }
                    }
                });
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

        #region GRID Check / Uncheck
        private void checkAll_Checked(object sender, RoutedEventArgs e)
        {
            DataTable dt = DataTableConverter.Convert(dgTargetList.ItemsSource);
            foreach (DataRow dr in dt.Rows)
            {
                dr["CHK"] = 1;
            }
            dgTargetList.ItemsSource = DataTableConverter.Convert(dt);
        }
        private void checkAll_Unchecked(object sender, RoutedEventArgs e)
        {
            Util.DataGridCheckAllUnChecked(dgTargetList);
        }
        #endregion

        #endregion

        #region Method
        // 활성화 인계 - 조회
        private void getTagetListSearch()
        {
            try
            {
                string sPallet_ID = txtPalletID1.Text.Trim();

                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("PALLETID", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["PALLETID"] = sPallet_ID;
                RQSTDT.Rows.Add(dr);

				ShowLoadingIndicator();
                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_MOVE_RECEIVE_CELL_PLLT_LIST_NJ", "RQSTDT", "RSLTDT", RQSTDT);

				Util.GridSetData(dgTargetList, dtResult, FrameOperation, true);
                Util.SetTextBlockText_DataGridRowCount(tbTagetListCount, Util.NVC(dtResult.Rows.Count));
				
                //DataTable dtBefore = DataTableConverter.Convert(dgTargetList.ItemsSource);
                //if (dgTargetList.GetRowCount() != 0)
                //{   
                //    // 중복 체크
                //    if (dtBefore.Select("PALLETID = '" + sPallet_ID + "'").Count() > 0)
                //    {
                //        Util.MessageValidation("SFU1914");   //중복 스캔되었습니다.
                //        return;
                //    }
                //}
                //dtBefore.Merge(dtResult);

                //dgTargetList.ItemsSource = DataTableConverter.Convert(dtBefore);
                //Util.SetTextBlockText_DataGridRowCount(tbTagetListCount, Util.NVC(dtBefore.Rows.Count));

                txtPalletID1.Text = string.Empty;
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

        // 이력 조회 - 검색 날짜 Validation
        private Boolean dtDateCompare()
        {
            try
            {
                TimeSpan timeSpan = dtpDateTo.SelectedDateTime.Date - dtpDateFrom.SelectedDateTime.Date;

                if (timeSpan.Days < 0)
                {
                    //조회 시작일자는 종료일자를 초과 할 수 없습니다.
                    Util.MessageValidation("SFU3569");
                    return false;
                }

                if (timeSpan.Days > 7)
                {
                    dtpDateTo.SelectedDateTime = dtpDateFrom.SelectedDateTime.AddDays(+7);
                    //조회기간은 7일을 초과 할 수 없습니다.
                    Util.MessageValidation("SFU3567");
                    return false;
                }

                return true;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return false;
            }
        }
        
        // 이력 조회 - 조회
        private void getHistorySearch()
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("PALLETID", typeof(string));
                RQSTDT.Columns.Add("FROMDATE", typeof(string));
                RQSTDT.Columns.Add("TODATE", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["PALLETID"] = txtPalletID2.Text.Trim();
                dr["FROMDATE"] = dtpDateFrom.SelectedDateTime.ToString("yyyyMMdd");
                dr["TODATE"] = dtpDateTo.SelectedDateTime.ToString("yyyyMMdd");
                RQSTDT.Rows.Add(dr);

				ShowLoadingIndicator();
                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_MOVE_RECEIVE_CELL_PLLT_HISTORY_NJ", "INDATA", "OUTDATA", RQSTDT);

                Util.GridSetData(dgSearchResultList, dtResult, FrameOperation, true);
                Util.SetTextBlockText_DataGridRowCount(tbSearchListCount, Util.NVC(dtResult.Rows.Count));

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

        #region Funct
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

