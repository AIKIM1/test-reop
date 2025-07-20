/*************************************************************************************
 Created Date : -
      Creator : 
   Decription : 반품요청승인
--------------------------------------------------------------------------------------
 [Change History]
  -           DEVELOPER : Initial Created.
  2023.05.22  김대현    : 반품 승인 요청 목록 조회시 SKID ID로도 조회가능하도록 수정
  2024.05.16  안유수    E20240502-001211 용어 변경, 반품요청 승인 -> 반품 승인, 반품요청 승인/반려 이력 -> 반품 승인/반려 이력




**************************************************************************************/
using System;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using C1.WPF.DataGrid;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using System.Configuration;
using System.IO;
using C1.WPF.Excel;
using System.Windows.Media;
using System.Globalization;

using LGC.GMES.MES.CMM001;
using LGC.GMES.MES.CMM001.Extensions;
using System.Windows.Input;
using LGC.GMES.MES.CMM001.Popup;
using System.Collections.Generic;
using System.Linq;

using C1.WPF;

namespace LGC.GMES.MES.COM001
{
    /// <summary>
    /// COM001_081.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class COM001_309 : UserControl
    {
        #region Private 변수
        CommonCombo _combo = new CMM001.Class.CommonCombo();
        DataTable dtHistList = new DataTable();
        #endregion

        #region Form Load & Init Control
        public IFrameOperation FrameOperation
        {
            get;
            set;
        }

        public COM001_309()
        {
            InitializeComponent();
        }

        C1.WPF.DataGrid.DataGridRowHeaderPresenter pre = new C1.WPF.DataGrid.DataGridRowHeaderPresenter()
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

        #endregion

        #region Events
        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            Init();
        }

        private void btnRtnReqSearch_Click(object sender, RoutedEventArgs e)
        {
            SearchRtnReqApprList();
        }

        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            SearchRtnReqApprHist();
        }

        private void btnAppr_Click(object sender, RoutedEventArgs e)
        {
            RtnApprReqApproval();
        }

        private void btnReject_Click(object sender, RoutedEventArgs e)
        {
            RtnApprReqReject();
        }
        #endregion

        #region Functions
        void Init()
        {
            dgLotList.ItemsSource = null;

            //CommonCombo combo = new CommonCombo();

            //string[] sFilter = { "RTN_STAT_CODE" };
            //combo.SetCombo(cboStat, CommonCombo.ComboStatus.ALL, sCase: "COMMCODE", sFilter: sFilter);

            //cboStat.SelectedValue = "RETURN_CONFIRM";

            //2023.05.22 김대현
            rdoLotId.Checked += rdoLotSkidId_Checked;
            rdoSkidId.Checked += rdoLotSkidId_Checked;
            rdoSearchLot.Checked += rdoSearchLotSkidId_Checked;
            rdoSearchSkid.Checked += rdoSearchLotSkidId_Checked;
        }
        
        #endregion Functions

        #region TextBox Event
        private void txtLotid_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == System.Windows.Input.Key.Enter)
            {
                if (!string.IsNullOrEmpty(txtLotid.Text.Trim()))
                {
                    SearchRtnReqApprList();
                }
            }
        }

        private void txtSkidid_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == System.Windows.Input.Key.Enter)
            {
                if (!string.IsNullOrEmpty(txtSkidid.Text.Trim()))
                {
                    SearchRtnReqApprList();
                }
            }
        }
        #endregion

        #region Radio Button Event
        private void rdoLotSkidId_Checked(object sender, RoutedEventArgs e)
        {
            if (rdoLotId.IsChecked == true)
            {
                tbLotid.Visibility = Visibility.Visible;
                txtLotid.Visibility = Visibility.Visible;

                tbSkidid.Visibility = Visibility.Collapsed;
                txtSkidid.Visibility = Visibility.Collapsed;
                txtSkidid.Text = string.Empty;
            }

            if (rdoSkidId.IsChecked == true)
            {
                tbLotid.Visibility = Visibility.Collapsed;
                txtLotid.Visibility = Visibility.Collapsed;

                tbSkidid.Visibility = Visibility.Visible;
                txtSkidid.Visibility = Visibility.Visible;
                txtLotid.Text = string.Empty;
            }
        }

        private void rdoSearchLotSkidId_Checked(object sender, RoutedEventArgs e)
        {
            if (rdoSearchLot.IsChecked == true)
            {
                tbSearchLot.Visibility = Visibility.Visible;
                txtSearchLOT.Visibility = Visibility.Visible;

                tbSearchSkid.Visibility = Visibility.Collapsed;
                txtSearchSkid.Visibility = Visibility.Collapsed;
                txtSearchSkid.Text = string.Empty;
            }

            if (rdoSearchSkid.IsChecked == true)
            {
                tbSearchLot.Visibility = Visibility.Collapsed;
                txtSearchLOT.Visibility = Visibility.Collapsed;

                tbSearchSkid.Visibility = Visibility.Visible;
                txtSearchSkid.Visibility = Visibility.Visible;
                txtSearchLOT.Text = string.Empty;
            }
        }
        #endregion

        #region METHOD

        private void RtnApprReqApproval()
        {
            try
            {
                if (dgLotList.GetRowCount() == 0)
                {
                    Util.Alert("SFU1651");  //선택된 항목이 없습니다.
                    return;
                }

                //승인하시겠습니까?
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("SFU2878"), null, "Information", MessageBoxButton.OKCancel, MessageBoxIcon.None, (result) =>
                {
                    if (result == MessageBoxResult.OK)
                    {
                        DataRow[] drChk = Util.gridGetChecked(ref dgLotList, "CHK");

                        if (drChk.Length == 0)
                        {
                            Util.Alert("SFU1651");  //선택된 항목이 없습니다.
                            return;
                        }

                        DataSet indataSet = new DataSet();
                        DataTable inData = indataSet.Tables.Add("INDATA");
                        inData.Columns.Add("SRCTYPE", typeof(string));
                        inData.Columns.Add("USERID", typeof(string));
                        inData.Columns.Add("RTN_STAT_CODE", typeof(string));

                        DataRow row = inData.NewRow();
                        row["SRCTYPE"] = "UI";
                        row["USERID"] = LoginInfo.USERID;
                        row["RTN_STAT_CODE"] = "RETURN_CONFIRM";

                        indataSet.Tables["INDATA"].Rows.Add(row);

                        DataTable inLot = indataSet.Tables.Add("INLOT");
                        inLot.Columns.Add("RTN_REQ_ID", typeof(string));
                        inLot.Columns.Add("RTN_APPR_NOTE", typeof(string));

                        for (int i = 0; i < dgLotList.GetRowCount(); i++)
                        {
                            if (DataTableConverter.GetValue(dgLotList.Rows[i].DataItem, "CHK").ToString() == "1")
                            {
                                DataRow row2 = inLot.NewRow();
                                row2["RTN_REQ_ID"] = DataTableConverter.GetValue(dgLotList.Rows[i].DataItem, "RTN_REQ_ID").ToString();
                                row2["RTN_APPR_NOTE"] = string.IsNullOrEmpty(DataTableConverter.GetValue(dgLotList.Rows[i].DataItem, "RTN_APPR_NOTE").ToString()) ? null : DataTableConverter.GetValue(dgLotList.Rows[i].DataItem, "RTN_APPR_NOTE").ToString();

                                indataSet.Tables["INLOT"].Rows.Add(row2);
                            }
                        }

                        new ClientProxy().ExecuteService_Multi("BR_PRD_REG_APPROVAL_RTN_APPR_REQ", "INDATA,INLOT", null, (bizResult, bizException) =>
                        {
                            try
                            {
                                if (bizException != null)
                                {
                                    Util.MessageException(bizException);
                                    return;
                                }
                                else
                                {
                                    //dgListHist.ItemsSource = null;

                                    //정상 처리 되었습니다.
                                    Util.Alert("SFU1275");

                                    //DeleteRows();
                                    SearchRtnReqApprList();

                                }
                            }
                            catch (Exception ex)
                            {
                                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
                            }
                        }, indataSet
                        );

                    }
                });

                }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
                return;
            }
        }

        private void RtnApprReqReject()
        {
            try
            {
                if (dgLotList.GetRowCount() == 0)
                {
                    Util.Alert("SFU1651");  //선택된 항목이 없습니다.
                    return;
                }

                //반려하시겠습니까?
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("SFU2866"), null, "Information", MessageBoxButton.OKCancel, MessageBoxIcon.None, (result) =>
                {
                    if (result == MessageBoxResult.OK)
                    {
                        DataRow[] drChk = Util.gridGetChecked(ref dgLotList, "CHK");

                        if (drChk.Length == 0)
                        {
                            Util.Alert("SFU1651");  //선택된 항목이 없습니다.
                            return;
                        }

                        DataSet indataSet = new DataSet();
                        DataTable inData = indataSet.Tables.Add("INDATA");
                        inData.Columns.Add("SRCTYPE", typeof(string));
                        inData.Columns.Add("USERID", typeof(string));
                        inData.Columns.Add("RTN_STAT_CODE", typeof(string));

                        DataRow row = inData.NewRow();
                        row["SRCTYPE"] = "UI";
                        row["USERID"] = LoginInfo.USERID;
                        row["RTN_STAT_CODE"] = "REJECT_RETURN";

                        indataSet.Tables["INDATA"].Rows.Add(row);

                        DataTable inLot = indataSet.Tables.Add("INLOT");
                        inLot.Columns.Add("RTN_REQ_ID", typeof(string));
                        inLot.Columns.Add("RTN_APPR_NOTE", typeof(string));

                        for (int i = 0; i < dgLotList.GetRowCount(); i++)
                        {
                            if (DataTableConverter.GetValue(dgLotList.Rows[i].DataItem, "CHK").ToString() == "1")
                            {
                                DataRow row2 = inLot.NewRow();
                                row2["RTN_REQ_ID"] = DataTableConverter.GetValue(dgLotList.Rows[i].DataItem, "RTN_REQ_ID").ToString();
                                row2["RTN_APPR_NOTE"] = string.IsNullOrEmpty(DataTableConverter.GetValue(dgLotList.Rows[i].DataItem, "RTN_APPR_NOTE").ToString()) ? null : DataTableConverter.GetValue(dgLotList.Rows[i].DataItem, "RTN_APPR_NOTE").ToString();

                                indataSet.Tables["INLOT"].Rows.Add(row2);
                            }
                        }

                        new ClientProxy().ExecuteService_Multi("BR_PRD_REG_APPROVAL_RTN_APPR_REQ", "INDATA,INLOT", null, (bizResult, bizException) =>
                        {
                            try
                            {
                                if (bizException != null)
                                {
                                    Util.MessageException(bizException);
                                    return;
                                }
                                else
                                {
                                    //dgListHist.ItemsSource = null;

                                    //정상 처리 되었습니다.
                                    Util.Alert("SFU1275");

                                    //DeleteRows();
                                    SearchRtnReqApprList();

                                }
                            }
                            catch (Exception ex)
                            {
                                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
                            }
                        }, indataSet
                        );

                    }
                });

            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
                return;
            }
        }

        private void DeleteRows()
        {
            try
            {
                for (int i = 0; i < dgLotList.GetRowCount(); i++)
                {
                    if (DataTableConverter.GetValue(dgLotList.Rows[i].DataItem, "CHK").ToString() == "1")
                    {
                        DataTable dt = DataTableConverter.Convert(dgLotList.ItemsSource);
                        dt.Rows[i].Delete();
                        dt.AcceptChanges();
                        dgLotList.ItemsSource = DataTableConverter.Convert(dt);
                    }
                }
            } catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
                return;
            }
            
        }

        private void SearchRtnReqApprHist()
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "INLOT";
                RQSTDT.Columns.Add("LANGID", typeof(String));
                RQSTDT.Columns.Add("AREAID", typeof(String));
                RQSTDT.Columns.Add("LOTID", typeof(String));
                RQSTDT.Columns.Add("SKIDID", typeof(String));   //2023.05.22 김대현
                RQSTDT.Columns.Add("RTN_STAT_CODE", typeof(String));
                RQSTDT.Columns.Add("FROM_RTN_APPR_DTTM", typeof(String));
                RQSTDT.Columns.Add("TO_RTN_APPR_DTTM", typeof(String));
                RQSTDT.Columns.Add("FROM_RTN_REJ_DTTM", typeof(String));
                RQSTDT.Columns.Add("TO_RTN_REJ_DTTM", typeof(String));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["AREAID"] = LoginInfo.CFG_AREA_ID;
                //2023.05.22 김대현
                if (rdoSearchLot.IsChecked == true)
                {
                    dr["LOTID"] = string.IsNullOrEmpty(txtSearchLOT.Text.Trim()) ? null : txtSearchLOT.Text;
                    dr["SKIDID"] = null;
                }
                if (rdoSearchSkid.IsChecked == true)
                {
                    dr["LOTID"] = null;
                    dr["SKIDID"] = string.IsNullOrEmpty(txtSearchSkid.Text.Trim()) ? null : txtSearchSkid.Text;
                }
                //2023.05.22 김대현

                if (rdoAppr.IsChecked == true)
                {
                    dr["RTN_STAT_CODE"] = "RETURN_CONFIRM";
                    dr["FROM_RTN_APPR_DTTM"] = string.Format("{0:yyyyMMdd}", dtpDateFrom.SelectedDateTime);
                    dr["TO_RTN_APPR_DTTM"] = string.Format("{0:yyyyMMdd}", dtpDateTo.SelectedDateTime);
                }
                else
                {
                    dr["RTN_STAT_CODE"] = "REJECT_RETURN";
                    dr["FROM_RTN_REJ_DTTM"] = string.Format("{0:yyyyMMdd}", dtpDateFrom.SelectedDateTime);
                    dr["TO_RTN_REJ_DTTM"] = string.Format("{0:yyyyMMdd}", dtpDateTo.SelectedDateTime);
                }

                //dr["RTN_STAT_CODE"] = Util.GetCondition(cboStat, bAllNull: true); ; // "RETURN_CONFIRM";
                //dr["FROM_DATE"] = string.Format("{0:yyyyMMdd}", dtpDateFrom.SelectedDateTime);
                //dr["TO_DATE"] = string.Format("{0:yyyyMMdd}", dtpDateTo.SelectedDateTime);

                RQSTDT.Rows.Add(dr);

                DataTable Result = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_RTN_APPR_REQ_LIST", "RQSTDT", "RSLTDT", RQSTDT);

                Util.gridClear(dgListHist);

                if (Result.Rows.Count >= 1)
                {
                    dgListHist.ItemsSource = DataTableConverter.Convert(Result);
                }
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
                return;
            }
        }

        private void SearchRtnReqApprList()
        {
            try
            {
                Util.gridClear(dgLotList);

                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "INLOT";
                RQSTDT.Columns.Add("LANGID", typeof(String));
                RQSTDT.Columns.Add("AREAID", typeof(String));
                RQSTDT.Columns.Add("LOTID", typeof(String));
                RQSTDT.Columns.Add("SKIDID", typeof(String));   //2023.05.22 김대현
                RQSTDT.Columns.Add("RTN_STAT_CODE", typeof(String));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["AREAID"] = LoginInfo.CFG_AREA_ID;
                //2023.05.22 김대현
                if (rdoLotId.IsChecked == true)
                {
                    dr["LOTID"] = string.IsNullOrEmpty(txtLotid.Text.Trim()) ? null : txtLotid.Text;
                    dr["SKIDID"] = null;
                }
                if (rdoSkidId.IsChecked == true)
                {
                    dr["LOTID"] = null;
                    dr["SKIDID"] = string.IsNullOrEmpty(txtSkidid.Text.Trim()) ? null : txtSkidid.Text;
                }
                //2023.05.22 김대현
                dr["RTN_STAT_CODE"] = "RETURN";

                RQSTDT.Rows.Add(dr);

                DataTable Result = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_RTN_APPR_REQ_LIST", "RQSTDT", "RSLTDT", RQSTDT);

                if (Result.Rows.Count >= 1)
                {
                    Util.gridClear(dgLotList);
                    dgLotList.ItemsSource = DataTableConverter.Convert(Result);
                }
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
                return;
            }
        }


        #endregion


    }
}