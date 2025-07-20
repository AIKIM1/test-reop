/*************************************************************************************
 Created Date : 2023.05.15
      Creator : 이다혜
   Decription : 설비 Loss 수정 승인
--------------------------------------------------------------------------------------
 [Change History]
  2023.05.15  이다혜 : Initial Created
  2023.06.08  이다혜 : 승인 후 Loss 반영 시, OCCR_EQPTID Para 값 제거
  2023.08.30  김대현 : 승인시 날짜 파라미터 제거
  2023.09.15  안유수  E20230913-000991 LOSS_NOTE 컬럼 추가
  2023.09.26  김대현  설비 승인시 LOSS HIST 에 적용하는 비즈 수정
  2024.01.30  김대현  설비 Loss 등록/수정 통합화
**************************************************************************************/

using C1.WPF;
using C1.WPF.DataGrid;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using LGC.GMES.MES.CMM001.Controls;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using System.Globalization;
using System.Windows.Documents;
using LGC.GMES.MES.CMM001.Extensions;
using LGC.GMES.MES.CMM001.Popup;
using LGC.GMES.MES.COM001;


namespace LGC.GMES.MES.COM001
{
    /// <summary>
    /// COM001_380.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class COM001_380 : UserControl, IWorkArea
    {
        #region Declaration & Constructor
        public COM001_380()
        {
            InitializeComponent();
        }

        public IFrameOperation FrameOperation
        {
            get;
            set;
        }
        C1.WPF.DataGrid.DataGridRowHeaderPresenter pre1 = new C1.WPF.DataGrid.DataGridRowHeaderPresenter()
        {
            Background = new SolidColorBrush(Colors.Transparent),
            MouseOverBrush = new SolidColorBrush(Colors.Transparent),
        };

        CheckBox chkAll = new CheckBox()
        {
            IsChecked = false,
            Background = new SolidColorBrush(Colors.Transparent),
            VerticalAlignment = System.Windows.VerticalAlignment.Center,
            HorizontalAlignment = System.Windows.HorizontalAlignment.Center
        };
        Util _Util = new Util();

        #endregion

        #region Initialize

        #endregion

        #region Event

        #region 화면 로드 : UserControl_Loaded()

        /// <summary>
        ///  화면 로드
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            dtpFrom.SelectedDateTime = System.DateTime.Now.AddDays(-7);
            dtpTo.SelectedDateTime = System.DateTime.Now;
        }
        #endregion

        #region 조회  : btnSearch_Click()
        /// <summary>
        /// 조회
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            // 로그인한 User의 승인대기 건만 조회
            try
            {
                ShowLoadingIndicator();
                DoEvents();

                GetEqptLossChangeApprList();
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

        #region 승인 : btnApproval_Click()
        private void btnApproval_Click(object sender, RoutedEventArgs e)
        {
            if (!this.TotalSaveValidation("A"))
            {
                return;
            }
            // 승인하시겠습니까?
            Util.MessageConfirm("SFU2878", (result) =>
            {
                if (result == MessageBoxResult.OK)
                {
                    LossSave(); // 비조업, 비부하  저장
                }
            });


        }

        #endregion

        #region 반려 : btnReject_Click()

        /// <summary>
        /// 반려
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnReject_Click(object sender, RoutedEventArgs e)
        {
            if (!this.TotalSaveValidation("R"))
            {
                return;
            }
            //반려하시겠습니까?
            LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(
                        "SFU2866"), null, "Information", MessageBoxButton.OKCancel, MessageBoxIcon.None, (result) =>
                        {
                            if (result == MessageBoxResult.OK)
                            {
                                ApprProcess("R");
                            }
                        });
        }

        #endregion

        #region  스프레드 색 표현  : dgList_LoadedCellPresenter()
        /// <summary>
        /// 스프레드 색 표현
        /// </summary>
        private void dgList_LoadedCellPresenter(object sender, C1.WPF.DataGrid.DataGridCellEventArgs e)
        {
            C1.WPF.DataGrid.C1DataGrid dataGrid = sender as C1.WPF.DataGrid.C1DataGrid;

            dataGrid.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (e.Cell.Presenter == null)
                {
                    return;
                }
                //Grid Data Binding 이용한 Foregroune 색 변경
                if (e.Cell.Row.Type == DataGridRowType.Item)
                {
                    // ERP 마감일 경우  붉은색 표현
                    string sERPCheck = Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "ERP_CLOSING_FLAG"));
                    if (sERPCheck.Equals("CLOSE"))
                    {
                        e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Red);
                    }
                }
                
            }));

        }
        #endregion

        #region 스프레드 전체 선택  : dgList_LoadedColumnHeaderPresenter(), checkAll1_Checked(), checkAll1_Unchecked()

        /// <summary>
        /// 스프레드 Head 이벤트
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void dgList_LoadedColumnHeaderPresenter(object sender, C1.WPF.DataGrid.DataGridColumnEventArgs e)
        {
            try
            {
                this.Dispatcher.BeginInvoke(new Action(() =>
                {
                    if (string.IsNullOrEmpty(e.Column.Name) == false)
                    {
                        //Head 선택시  
                        if (e.Column.Name.Equals("CHK"))
                        {
                            pre1.Content = chkAll;
                            e.Column.HeaderPresenter.Content = pre1;
                            chkAll.Checked -= new RoutedEventHandler(checkAll1_Checked);
                            chkAll.Unchecked -= new RoutedEventHandler(checkAll1_Unchecked);
                            chkAll.Checked += new RoutedEventHandler(checkAll1_Checked);
                            chkAll.Unchecked += new RoutedEventHandler(checkAll1_Unchecked);
                        }
                    }
                }));
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return;
            }
        }

        private void dgList_MergingCells(object sender, C1.WPF.DataGrid.DataGridMergingCellsEventArgs e)
        {
            C1DataGrid dg = sender as C1DataGrid;
            if (dg.Rows.Count == 0) return;

            List<System.Data.DataRow> list = DataTableConverter.Convert(dg.ItemsSource).Select().ToList();
            List<int> arr = list.GroupBy(c => c["REQ_SEQNO"]).Select(group => group.Count()).ToList();

            int p = 0;
            for (int j = 0; j < arr.Count; j++)
            {
                for (int i = 0; i < dg.Columns.Count; i++)
                {
                    if (dg.Columns[i].Name.Equals("CHK")
                        || dg.Columns[i].Name.Equals("AREANAME")
                        || dg.Columns[i].Name.Equals("EQSGNAME")
                        || dg.Columns[i].Name.Equals("EQPTNAME")
                        || dg.Columns[i].Name.Equals("APPR_REQ_USERNAME")
                        || dg.Columns[i].Name.Equals("APPR_REQ_DTTM")
                        || dg.Columns[i].Name.Equals("APPR_STAT_NAME"))
                    {
                        e.Merge(new DataGridCellsRange(dg.GetCell(p, i), dg.GetCell((p + arr[j] - 1), i)));
                    }
                }
                p += arr[j];
            }
        }
        /// <summary>
        /// 전체선택
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void checkAll1_Checked(object sender, RoutedEventArgs e)
        {

            if ((bool)chkAll.IsChecked)
            {
                for (int i = 0; i < dgList.GetRowCount(); i++)
                {
                    DataTableConverter.SetValue(dgList.Rows[i].DataItem, "CHK", true);
                }
            }
        }
        /// <summary>
        /// 전체 선택해제
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void checkAll1_Unchecked(object sender, RoutedEventArgs e)
        {
            if (!(bool)chkAll.IsChecked)
            {
                for (int i = 0; i < dgList.GetRowCount(); i++)
                {
                    DataTableConverter.SetValue(dgList.Rows[i].DataItem, "CHK", false);
                }
            }
        }


        #endregion

        #endregion


        #region Method

        #region 승인 요청 리스트 조회 : GetEqptLossChangeApprList()
        /// <summary>
        /// 승인리스트 
        /// </summary>
        private void GetEqptLossChangeApprList()
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                DataTable RSLTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                //RQSTDT.Columns.Add("AREAID", typeof(string));
                RQSTDT.Columns.Add("APPR_STAT", typeof(string));
                RQSTDT.Columns.Add("APPR_USERID", typeof(string));
                RQSTDT.Columns.Add("FROM_APPR_REQ_DTTM", typeof(string));
                RQSTDT.Columns.Add("TO_APPR_REQ_DTTM", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                //dr["AREAID"] = LoginInfo.CFG_AREA_ID;
                dr["APPR_STAT"] = "W";
                dr["APPR_USERID"] = LoginInfo.USERID;
                dr["FROM_APPR_REQ_DTTM"] = dtpFrom.SelectedDateTime.ToString("yyyyMMdd");
                dr["TO_APPR_REQ_DTTM"] = dtpTo.SelectedDateTime.ToString("yyyyMMdd");

                RQSTDT.Rows.Add(dr);

                RSLTDT = new ClientProxy().ExecuteServiceSync("DA_EQP_SEL_EQPTLOSS_APPROVAL_TARGET_LIST", "RQSTDT", "RSLTDT", RQSTDT);

                dgList.MergingCells -= dgList_MergingCells;
                Util.GridSetData(dgList, RSLTDT, FrameOperation, true);
                dgList.MergingCells += dgList_MergingCells;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        #endregion

        #region 승인시 Loss 저장  : LossSave()

        private void LossSave()
        {

            int idx = _Util.GetDataGridCheckFirstRowIndex(dgList, "CHK") == -1 ? 0 : _Util.GetDataGridCheckFirstRowIndex(dgList, "CHK");

            DataSet ds = new DataSet();
            DataTable RQSTDT = ds.Tables.Add("INDATA");
            //RQSTDT.TableName = "RQSTDT";
            RQSTDT.Columns.Add("EQPTID", typeof(string));
            RQSTDT.Columns.Add("WRK_DATE", typeof(string));
            RQSTDT.Columns.Add("STRT_DTTM", typeof(string));
            RQSTDT.Columns.Add("END_DTTM", typeof(string));
            RQSTDT.Columns.Add("LOSS_CODE", typeof(string));
            RQSTDT.Columns.Add("LOSS_DETL_CODE", typeof(string));
            RQSTDT.Columns.Add("LOSS_NOTE", typeof(string));
            RQSTDT.Columns.Add("SYMP_CODE", typeof(string));
            RQSTDT.Columns.Add("CAUSE_CODE", typeof(string));
            RQSTDT.Columns.Add("REPAIR_CODE", typeof(string));
            RQSTDT.Columns.Add("OCCR_EQPTID", typeof(string));
            RQSTDT.Columns.Add("SYMP_CNTT", typeof(string));
            RQSTDT.Columns.Add("CAUSE_CNTT", typeof(string));
            RQSTDT.Columns.Add("REPAIR_CNTT", typeof(string));
            RQSTDT.Columns.Add("CHKW", typeof(string));
            RQSTDT.Columns.Add("CHKT", typeof(string));
            RQSTDT.Columns.Add("CHKU", typeof(string));
            RQSTDT.Columns.Add("USERID", typeof(string));
            RQSTDT.Columns.Add("SPLT_MRG_FLAG", typeof(string));
            RQSTDT.Columns.Add("ORG_STRT_DTTM", typeof(string));
            RQSTDT.Columns.Add("ORG_END_DTTM", typeof(string));
            RQSTDT.Columns.Add("LOSS_SEQNO", typeof(string));
            RQSTDT.Columns.Add("PRE_LOSS_SEQNO", typeof(string));

            for (int i = 0; i < dgList.GetRowCount(); i++)
            {
                if (Util.NVC(DataTableConverter.GetValue(dgList.Rows[i].DataItem, "CHK")).Equals("1") || Util.NVC(DataTableConverter.GetValue(dgList.Rows[i].DataItem, "CHK")).Equals("True"))
                {
                    DataRow[] drs = DataTableConverter.Convert(dgList.ItemsSource).Select("REQ_SEQNO='" + Util.NVC(DataTableConverter.GetValue(dgList.Rows[i].DataItem, "REQ_SEQNO") + "'"));

                    foreach(DataRow dri in drs)
                    {
                        DataRow dr = RQSTDT.NewRow();
                        dr["EQPTID"] = dri["EQPTID"];                //설비ID
                        dr["WRK_DATE"] = dri["WRK_DATE"];            //작업일자
                        dr["STRT_DTTM"] = dri["HIDDEN_START"];       //시작일자
                        dr["END_DTTM"] = dri["HIDDEN_END"];          //완료일자
                        dr["LOSS_CODE"] = dri["APPR_REQ_LOSS_CODE"]; //변경 LOSS 코드 
                        dr["LOSS_DETL_CODE"] = dri["APPR_REQ_LOSS_DETL_CODE"]; // 변경 부동내용
                        dr["LOSS_NOTE"] = dri["APPR_REQ_LOSS_CNTT"]; //Comment
                        dr["SYMP_CODE"] = string.Empty;
                        dr["CAUSE_CODE"] = string.Empty;
                        dr["REPAIR_CODE"] = string.Empty;
                        dr["OCCR_EQPTID"] = string.Empty; //Util.NVC(DataTableConverter.GetValue(dgList.Rows[i].DataItem, "OCCR_EQPTID")); //원인설비
                        dr["USERID"] = LoginInfo.USERID; //변경자 (승인자)
                        dr["SPLT_MRG_FLAG"] = dri["SPLT_MRG_FLAG"];
                        dr["ORG_STRT_DTTM"] = dri["ORG_STRT_DTTM"];
                        dr["ORG_END_DTTM"] = dri["ORG_END_DTTM"];
                        dr["LOSS_SEQNO"] = dri["LOSS_SEQNO"];
                        dr["PRE_LOSS_SEQNO"] = dri["PRE_LOSS_SEQNO"];

                        RQSTDT.Rows.Add(dr);
                    }
                }
            }

            try
            {
                //변경 저장
                //new ClientProxy().ExecuteServiceSync_Multi("BR_EQPT_EQPTLOSS_UPD_LOSS_ALL", "RQSTDT", null, ds);
                new ClientProxy().ExecuteServiceSync_Multi("BR_EQPT_EQPTLOSS_UPD_LOSS_APPR", "RQSTDT", null, ds);


                try
                {
                    // 변경 이력 저장
                    DataTable dtRslt2 = new ClientProxy().ExecuteServiceSync("DA_EQP_INS_EQPTLOSS_CHG_HIST", "RQSTDT", "RSLTDT", RQSTDT);
                }
                catch (Exception ex9)
                {
                    Util.MessageException(ex9);
                }
                //승인정보 저장 ( A: 승인  R : 반려)
                ApprProcess("A");

            }

            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private static DataRow[] gridGetChecked2(ref UcBaseDataGrid dg, string sCheckColName)
        {
            DataRow[] dr = null;
            try
            {
                DataTable dtChk = DataTableConverter.Convert(dg.ItemsSource);
                if (dtChk.Columns.Contains(sCheckColName))
                {
                    dr = dtChk.Select(sCheckColName + " = 'True' ");
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return dr;
        }

        #endregion

        #region 승인 및 반려 처리  : ApprProcess()
        private void ApprProcess(string _STAT)
        {

            DataTable RQSTDT = new DataTable();
            RQSTDT.TableName = "RQSTDT";
            RQSTDT.Columns.Add("APPR_STAT", typeof(string));
            RQSTDT.Columns.Add("USERID", typeof(string));
            RQSTDT.Columns.Add("APPR_SEQNO", typeof(string));
            RQSTDT.Columns.Add("REQ_SEQNO", typeof(string));

            for (int i = 0; i < dgList.GetRowCount(); i++)
            {
                if (Util.NVC(DataTableConverter.GetValue(dgList.Rows[i].DataItem, "CHK")).Equals("1") || Util.NVC(DataTableConverter.GetValue(dgList.Rows[i].DataItem, "CHK")).Equals("True"))
                {
                    DataRow dr = RQSTDT.NewRow();
                    dr["APPR_STAT"] = _STAT;
                    dr["USERID"] = LoginInfo.USERID;
                    dr["APPR_SEQNO"] = Util.NVC(DataTableConverter.GetValue(dgList.Rows[i].DataItem, "APPR_SEQNO"));
                    dr["REQ_SEQNO"] = Util.NVC(DataTableConverter.GetValue(dgList.Rows[i].DataItem, "REQ_SEQNO"));

                    RQSTDT.Rows.Add(dr);
                }
            }
            DataTable dtR = new ClientProxy().ExecuteServiceSync("DA_EQP_UPD_EQPTLOSS_APPR", "RQSTDT", "RSLTDT", RQSTDT);
            GetEqptLossChangeApprList();
            if (_STAT == "A")
            {
                Util.AlertInfo("SFU1690");  //승인되었습니다.

            }
            else
            {
                Util.AlertInfo("SFU1541");  //반려되었습니다.
            }

        }
        #endregion

        #region 승인 및 반려 Validation  : TotalSaveValidation()
        private bool TotalSaveValidation(string _STAT)
        {
            if (dgList.Rows.Count == 0)
            {
                Util.MessageValidation("SFU3537");  //조회된 데이타가 없습니다
                return false;
            }

            DataRow[] drInfo;

            if (LoginInfo.CFG_SYSTEM_TYPE_CODE.Equals("F"))
            {
                drInfo = gridGetChecked2(ref dgList, "CHK");
            }
            else
            { 
                drInfo = Util.gridGetChecked(ref dgList, "CHK");
            }

            if (drInfo.Count() <= 0)
            {
                Util.MessageValidation("SFU3538");  //선택된 데이터가 없습니다
                return false;
            }
            if(_STAT == "A")
            {
                // ERP 마감여부체크.
                foreach (DataRow row in drInfo)
                {
                    if (Util.NVC(row["ERP_CLOSING_FLAG"]) == "CLOSE")
                    {
                        
                        Util.MessageValidation("SFU9018"); // ERP실적이 마감된 데이터가 선택되었습니다.\n실적이 마감된 데이터는 승인할 수 없습니다..
                        return false;
                    }
                }
            }
          
            return true;
        }
        #endregion

        #region Loading  : ShowLoadingIndicator(), HiddenLoadingIndicator()

        public void ShowLoadingIndicator()
        {
            if (loadingIndicator != null)
                loadingIndicator.Visibility = Visibility.Visible;
        }

        public void HiddenLoadingIndicator()
        {
            if (loadingIndicator != null)
                loadingIndicator.Visibility = Visibility.Collapsed;
        }

        #endregion

        #region DoEvent : DoEvents()
        private void DoEvents()
        {
            Application.Current.Dispatcher.Invoke(DispatcherPriority.Background, new System.Threading.ThreadStart(delegate { }));
        }
        #endregion

        #endregion

    }
}
