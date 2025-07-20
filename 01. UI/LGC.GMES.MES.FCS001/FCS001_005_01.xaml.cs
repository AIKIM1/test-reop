/*************************************************************************************************************************
 Created Date : 2020.11.13
      Creator : Dooly
   Decription : 일별 출고 현황
---------------------------------------------------------------------------------------------------------------------------
 [Change History]
  2020.11.13  DEVELOPER : Initial Created.
  2023.03.30  이정미 : Lot ID 더블클릭 시 Tray List에 클릭한 Lot ID만 보이도록 수정
  2023.04.20  홍석원 : PKG LOT 별 조회 기능 추가
  2023.05.04  홍석원 : Hold 제외 기능 개선
  2023.08.15  손동혁 : 재공정보현황(공정별) 화면에서 파라미터 전달받는 NA 특화 경과일 , 상 / 하층레인 추가 
  2023.09.14  최도훈 : 빈 공간 더블 클릭시 에러 발생하는 현상 수정
**************************************************************************************************************************/
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
    public partial class FCS001_005_01 : UserControl, IWorkArea
    {
        #region Declaration & Constructor 
        Util _Util = new Util();

        private string _sOPER = string.Empty;
        private string _sOPER_NAME = string.Empty;
        private string _sLINE_ID = string.Empty;
        private string _sLINE_NAME = string.Empty;
        private string _sROUTE_ID = string.Empty;
        private string _sROUTE_NAME = string.Empty;
        private string _sMODEL_ID = string.Empty;
        private string _sMODEL_NAME = string.Empty;
        private string _sROUTE_TYPE_DG = string.Empty;
        private string _sROUTE_TYPE_DG_NAME = string.Empty;
        private string _sStatus = string.Empty;
        private string _sStatusName = string.Empty;
        private string _sSPECIAL_YN = string.Empty;
        private string _sSpecialName = string.Empty;
        private string _sLOT_ID = string.Empty;
        private string _sLotType = string.Empty;
        private string _sLotTypeName = string.Empty;

        private string _sExceptHold = string.Empty;
        /// 재공정보현황(공정별) 화면에서 파라미터 전달받는 NA 특화 경과일 , 상 / 하층레인 추가 
        private string _sOverTime = string.Empty;
        private string _sFloorLane = string.Empty;

        public FCS001_005_01()
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

        #endregion

        #region Event

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            object[] tmps = this.FrameOperation.Parameters;
            _sOPER = tmps[0] as string;
            _sOPER_NAME = tmps[1] as string;
            _sLINE_ID = tmps[2] as string;
            _sLINE_NAME = tmps[3] as string;
            _sROUTE_ID = tmps[4] as string;
            _sROUTE_NAME = tmps[5] as string;
            _sMODEL_ID = tmps[6] as string;
            _sMODEL_NAME = tmps[7] as string;
            _sROUTE_TYPE_DG = tmps[8] as string;
            _sROUTE_TYPE_DG_NAME = tmps[9] as string;
            _sStatus = tmps[10] as string;
            _sStatusName = tmps[11] as string;
            _sSPECIAL_YN = tmps[12] as string;
            _sSpecialName = tmps[13] as string;
            _sLOT_ID = tmps[14] as string;
            _sLotType = tmps[15] as string;
            _sLotTypeName = tmps[16] as string;

            _sExceptHold = tmps[17] as string;
            ///재공정보현황(공정별) 화면에서 파라미터 전달받는 NA 특화 경과일, 상 / 하층레인 추가
            _sOverTime = tmps[18] as string;
            _sFloorLane = tmps[19] as string;
            //Util.SetTextBoxReadOnly(txtLine, _sLineName);
            //Util.SetTextBoxReadOnly(txtModel, _sModelName);
            //Util.SetTextBoxReadOnly(txtOp, _sOperName);
            //Util.SetTextBoxReadOnly(txtRoute, _sRouteName);
            //Util.SetTextBoxReadOnly(txtSpecialYN, _sSpecialYNName);
            //Util.SetTextBoxReadOnly(txtStatus, _sTrayStatusName);

            txtLine.Text = _sLINE_NAME;
            txtModel.Text = _sMODEL_NAME;
            txtOp.Text = _sOPER_NAME;
            txtRoute.Text = _sROUTE_NAME;
            txtSpecialYN.Text = _sSpecialName;
            txtStatus.Text = _sStatusName;
            txtLotType.Text = _sLotTypeName;

            if (_sExceptHold == "Y")
            {
                chkHold.IsChecked = true;
            }
            else
            {
                chkHold.IsChecked = false;
            }

            GetList();
        }

        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            GetList();
        }

        private void chkLot_Checked(object sender, RoutedEventArgs e)
        {
            dgTrayList.Columns["DAY_GR_LOTID"].Visibility = Visibility.Visible;
            GetList();
        }

        private void chkLot_Unchecked(object sender, RoutedEventArgs e)
        {
            dgTrayList.Columns["DAY_GR_LOTID"].Visibility = Visibility.Collapsed;
            GetList();
        }

        private void dgTrayList_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            C1.WPF.DataGrid.C1DataGrid datagrid = (sender as C1.WPF.DataGrid.C1DataGrid);
            Point pnt = e.GetPosition(null);
            C1.WPF.DataGrid.DataGridCell cell = datagrid.GetCellFromPoint(pnt);

            if (sender == null || cell == null) return;

            int row = cell.Row.Index;


            try
            {
                if (dgTrayList.CurrentRow != null && (dgTrayList.CurrentColumn.Name.Equals("TRAY_CNT")))
                {
                    if (chkPkgLot.IsChecked == true)
                    {
                        _sLOT_ID = Util.NVC(DataTableConverter.GetValue(datagrid.Rows[row].DataItem, "PROD_LOTID"));
                    }
                    else if (chkLot.IsChecked == true)
                    {
                        _sLOT_ID = Util.NVC(DataTableConverter.GetValue(datagrid.Rows[row].DataItem, "DAY_GR_LOTID"));
                    }

                    Load_FCS001_005_02(_sOPER, _sOPER_NAME, _sLINE_ID, _sLINE_NAME, _sROUTE_ID, _sROUTE_NAME, _sMODEL_ID, _sMODEL_NAME, _sStatus, _sStatusName, _sLOT_ID, _sSPECIAL_YN, _sSpecialName, _sROUTE_TYPE_DG, _sROUTE_TYPE_DG_NAME, DataTableConverter.GetValue(dgTrayList.CurrentRow.DataItem, "PLANTIME").ToString(), _sLotType, _sLotTypeName, _sExceptHold, _sOverTime, _sFloorLane);
                }

                if (dgTrayList.CurrentRow != null && (dgTrayList.CurrentColumn.Name.Equals("DAY_GR_LOTID")))
                {
                    _sLOT_ID = Util.NVC(DataTableConverter.GetValue(datagrid.Rows[row].DataItem, "DAY_GR_LOTID"));
                    Load_FCS001_005_02(_sOPER, _sOPER_NAME, _sLINE_ID, _sLINE_NAME, _sROUTE_ID, _sROUTE_NAME, _sMODEL_ID, _sMODEL_NAME, _sStatus, _sStatusName, _sLOT_ID, _sSPECIAL_YN, _sSpecialName, _sROUTE_TYPE_DG, _sROUTE_TYPE_DG_NAME, DataTableConverter.GetValue(dgTrayList.CurrentRow.DataItem, "PLANTIME").ToString(), _sLotType, _sLotTypeName, _sExceptHold, _sOverTime, _sFloorLane);
                }

                if (dgTrayList.CurrentRow != null && (dgTrayList.CurrentColumn.Name.Equals("PROD_LOTID")))
                {
                    _sLOT_ID = Util.NVC(DataTableConverter.GetValue(datagrid.Rows[row].DataItem, "PROD_LOTID"));
                    Load_FCS001_005_02(_sOPER, _sOPER_NAME, _sLINE_ID, _sLINE_NAME, _sROUTE_ID, _sROUTE_NAME, _sMODEL_ID, _sMODEL_NAME, _sStatus, _sStatusName, _sLOT_ID, _sSPECIAL_YN, _sSpecialName, _sROUTE_TYPE_DG, _sROUTE_TYPE_DG_NAME, DataTableConverter.GetValue(dgTrayList.CurrentRow.DataItem, "PLANTIME").ToString(), _sLotType, _sLotTypeName, _sExceptHold, _sOverTime, _sFloorLane);
                }
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }

        private void dgTrayList_LoadedCellPresenter(object sender, DataGridCellEventArgs e)
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

                    if (e.Cell.Column.Name.Equals("DAY_GR_LOTID"))
                    {
                        e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Blue);
                    }

                    if (e.Cell.Column.Name.Equals("PROD_LOTID"))
                    {
                        e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Blue);
                    }
                }


            }));
        }

        private void dgTrayList_LoadedColumnHeaderPresenter(object sender, C1.WPF.DataGrid.DataGridColumnEventArgs e)
        {
            //this.Dispatcher.BeginInvoke(new Action(() =>
            //{
            //    if (string.IsNullOrEmpty(e.Column.Name) == false)
            //    {
            //        if (e.Column.Name.Equals("TRAY_CNT"))
            //        {
            //            e.Column.HeaderPresenter.Foreground = new SolidColorBrush(Colors.Blue);
            //            e.Column.HeaderPresenter.FontWeight = FontWeights.Bold;
            //            e.Column.HeaderPresenter.Cursor = Cursors.Hand;
            //        }
            //    }
            //}));
        }

        private void chkPkgLot_Checked(object sender, RoutedEventArgs e)
        {
            dgTrayList.Columns["PROD_LOTID"].Visibility = Visibility.Visible;
            GetList();
        }

        private void chkPkgLot_Unchecked(object sender, RoutedEventArgs e)
        {
            dgTrayList.Columns["PROD_LOTID"].Visibility = Visibility.Collapsed;
            GetList();
        }

        private void chkHold_Checked(object sender, RoutedEventArgs e)
        {
            _sExceptHold = "Y";
        }

        private void chkHold_Unchecked(object sender, RoutedEventArgs e)
        {
            _sExceptHold = "N";
        }

        #endregion

        #region Method

        private void GetList()
        {
            try
            {
                DataTable dtRqst = new DataTable();
                dtRqst.TableName = "RQSTDT";
                dtRqst.Columns.Add("PROD_LOTID", typeof(string));
                dtRqst.Columns.Add("PROCID", typeof(string));
                dtRqst.Columns.Add("EQSGID", typeof(string));
                dtRqst.Columns.Add("ROUTID", typeof(string));
                dtRqst.Columns.Add("TRAY_OP_STATUS_CD", typeof(string));
                dtRqst.Columns.Add("MDLLOT_ID", typeof(string));
                dtRqst.Columns.Add("SPECIAL_YN", typeof(string));
                dtRqst.Columns.Add("ROUTE_TYPE_DG", typeof(string));

                dtRqst.Columns.Add("WIPSTAT", typeof(string));
                dtRqst.Columns.Add("ISS_RSV_FLAG", typeof(string));
                dtRqst.Columns.Add("ABNORM_FLAG", typeof(string));
                dtRqst.Columns.Add("LOTTYPE", typeof(string));

                dtRqst.Columns.Add("BY_LARGE_LOT", typeof(string));
                dtRqst.Columns.Add("BY_PKG_LOT", typeof(string));

                dtRqst.Columns.Add("EXCEPT_HOLD", typeof(string));

                dtRqst.Columns.Add("OVERTIME", typeof(string));
                dtRqst.Columns.Add("FLOOR_LANE", typeof(string));

                DataRow dr = dtRqst.NewRow();
                dr["PROD_LOTID"] = (string.IsNullOrEmpty(_sLOT_ID) ? null : _sLOT_ID);
                dr["PROCID"] = (string.IsNullOrEmpty(_sOPER) ? null : _sOPER);
                dr["EQSGID"] = (string.IsNullOrEmpty(_sLINE_ID) ? null : _sLINE_ID);
                dr["ROUTID"] = (string.IsNullOrEmpty(_sROUTE_ID) ? null : _sROUTE_ID);
                dr["TRAY_OP_STATUS_CD"] = (string.IsNullOrEmpty(_sStatus) ? null : _sStatus);
                dr["MDLLOT_ID"] = (string.IsNullOrEmpty(_sMODEL_ID) ? null : _sMODEL_ID);
                dr["SPECIAL_YN"] = (string.IsNullOrEmpty(_sSPECIAL_YN) ? null : _sSPECIAL_YN);
                dr["ROUTE_TYPE_DG"] = (string.IsNullOrEmpty(_sROUTE_TYPE_DG) ? null : _sROUTE_TYPE_DG);
                dr["LOTTYPE"] = (string.IsNullOrEmpty(_sLotType) ? null : _sLotType);

                dr["BY_LARGE_LOT"] = chkLot.IsChecked == true ? "Y" : "N";
                dr["BY_PKG_LOT"] = chkPkgLot.IsChecked == true ? "Y" : "N";


                if (string.IsNullOrEmpty(_sStatus))
                {
                    dr["TRAY_OP_STATUS_CD"] = null;
                }
                else if (_sStatus == "S")
                {
                    dr["WIPSTAT"] = "PROC";
                }
                else if (_sStatus == "E")
                {
                    dr["WIPSTAT"] = "END";
                }
                else if (_sStatus == "P")
                {
                    dr["ISS_RSV_FLAG"] = "Y";
                }
                else if (_sStatus == "T")
                {
                    dr["ABNORM_FLAG"] = "Y";
                }

                dr["EXCEPT_HOLD"] = (string.IsNullOrEmpty(_sExceptHold) ? null : _sExceptHold);

                ///재공정보현황(공정별) 화면에서 파라미터 전달받는 NA 특화 경과일 , 상 / 하층레인 추가 1
                dr["OVERTIME"] = (string.IsNullOrEmpty(_sOverTime) ? null : _sOverTime);
                dr["FLOOR_LANE"] = (string.IsNullOrEmpty(_sFloorLane) ? null : _sFloorLane);

                dtRqst.Rows.Add(dr);

                ShowLoadingIndicator();
                if (chkLot.IsChecked == true || chkPkgLot.IsChecked == true)
                {
                    DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_SEL_SCHEDULED_OUTGOING_DAILY_WITH_LOT", "RQSTDT", "RSLTDT", dtRqst);
                    Util.GridSetData(dgTrayList, dtRslt, FrameOperation, true);
                }
                else
                {
                    DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_SEL_SCHEDULED_OUTGOING_DAILY", "RQSTDT", "RSLTDT", dtRqst);
                    Util.GridSetData(dgTrayList, dtRslt, FrameOperation, true);
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

        private void Load_FCS001_005_02(string sOPER, string sOPER_NAME,
                                         string sLINE_ID, string sLINE_NAME,
                                         string sROUTE_ID, string sROUTE_NAME,
                                         string sMODEL_ID, string sMODEL_NAME,
                                         string sStatus, string sStatusName,
                                         string sLotID, string sSPECIAL_YN,
                                         string sSpecialName,
                                         string sROUTE_TYPE_DG, string sROUTE_TYPE_DG_NAME,
                                         string sPLANTIME,
                                         string sLotType, string sLotTypeName,
                                         string sExceptHold,
                                         string sOverTime, string sFloorLane)
        {
            //Tray List
            FCS001_005_02 TrayList = new FCS001_005_02();
            TrayList.FrameOperation = FrameOperation;

            object[] Parameters = new object[23];
            Parameters[0] = sOPER; //sOPER
            Parameters[1] = sOPER_NAME; //sOPER_NAME
            Parameters[2] = sLINE_ID; //sLINE_ID
            Parameters[3] = sLINE_NAME; //sLINE_NAME
            Parameters[4] = sROUTE_ID; //sROUTE_ID
            Parameters[5] = sROUTE_NAME; //sROUTE_NAME
            Parameters[6] = sMODEL_ID; //sMODEL_ID
            Parameters[7] = sMODEL_NAME; //sMODEL_NAME
            Parameters[8] = sStatus; //sStatus
            Parameters[9] = sStatusName; //sStatusName
            Parameters[10] = sROUTE_TYPE_DG; //sROUTE_TYPE_DG
            Parameters[11] = sROUTE_TYPE_DG_NAME; //sROUTE_TYPE_DG_NAME
            Parameters[12] = sLotID; //sLotID
            Parameters[13] = sSPECIAL_YN; //sSPECIAL_YN
            Parameters[16] = sPLANTIME.Replace("-", ""); //sPLANTIME
            Parameters[17] = sLotType;
            Parameters[18] = sLotTypeName;

            Parameters[19] = "";
            Parameters[20] = sExceptHold;

            //NA 경과일 , 층 구분 추가
            Parameters[21] = sOverTime;
            Parameters[22] = sFloorLane;

            //this.FrameOperation.OpenMenu("SFU010705052", true, Parameters);
            this.FrameOperation.OpenMenuFORM("SFU010705052", "FCS001_005_02", "LGC.GMES.MES.FCS001", ObjectDic.Instance.GetObjectName("Tray List"), true, Parameters);
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
