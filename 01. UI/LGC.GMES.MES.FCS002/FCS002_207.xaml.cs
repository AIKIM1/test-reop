/*************************************************************************************
 Created Date : 2022.12.05
      Creator : 
   Decription : 다중 Tray 공정 변경
--------------------------------------------------------------------------------------
 [Change History]
  2023.07.14  DEVELOPER : Initial Created.
 
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
using C1.WPF.Excel;
using System.Configuration;
using Microsoft.Win32;
using System.IO;

namespace LGC.GMES.MES.FCS002
{
    public partial class FCS002_207 : UserControl, IWorkArea
    {
        #region Declaration & Constructor 

        Util _Util = new Util();

        public FCS002_207()
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

            string[] sFilter = { "" }; //설비 그룹정해지면..(TAPING 설비), 설비 그룹명
            _combo.SetCombo(cboTapingEqp, CommonCombo_Form_MB.ComboStatus.SELECT, sFilter: sFilter, sCase: "TAP_EQUIPMENTFORM");


            _combo.SetCombo(cboLane, CommonCombo_Form_MB.ComboStatus.NONE, sCase: "LANE");
            
            string[] sFilterLane = { "L" };
            _combo.SetCombo(cboCPFLane, CommonCombo_Form_MB.ComboStatus.NONE, sCase: "LANE_MB", sFilter: sFilterLane);

            chkReservation.IsChecked = false;
            cboLane.IsEnabled = false;
            cboRow.IsEnabled = false;
            cboCol.IsEnabled = false;
            cboStg.IsEnabled = false;

            chkCPFReservation.IsChecked = false;
            cboCPFLane.IsEnabled = false;
            cboCPFRow.IsEnabled = false;
            cboCPFStg.IsEnabled = false;
            cboCPFCol.IsEnabled = false;
        }
        #endregion

        #region Event
        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                InitCombo();
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void txtTrayID_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                GetList();
            }
        }

        private void btnDeleteAll_Click(object sender, RoutedEventArgs e)
        {
            dgList.ItemsSource = null;

            txtOp.Text = string.Empty;
            txtRoute.Text = string.Empty;

            cboOp.ItemsSource = null;
            cboOp.Text = string.Empty;
        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                //변경하시겠습니까?
                Util.MessageConfirm("FM_ME_0337", (re) =>
                {
                    if (re != MessageBoxResult.OK)
                    {
                        return;
                    }

                    DataTable preTable = new DataTable();
                    if (dgList.Rows.Count > 0)
                    {
                        preTable = DataTableConverter.Convert(dgList.ItemsSource);
                    }
                    else
                    {
                        Util.MessageValidation("FM_ME_0240");  //처리할 데이터가 없습니다.
                        return;
                    }
                    DataSet ds = new DataSet();
                    DataTable dt = ds.Tables.Add("INDATA");

                    dt.Columns.Add("SRCTYPE", typeof(string));
                    dt.Columns.Add("IFMODE", typeof(string));
                    dt.Columns.Add("CSTID", typeof(string));
                    dt.Columns.Add("PROCID", typeof(string));
                    dt.Columns.Add("USERID", typeof(string));


                    for (int i = 0; i < preTable.Rows.Count; i++)
                    {

                        DataRow dr = dt.NewRow();
                        dr["SRCTYPE"] = "UI";
                        dr["IFMODE"] = "OFF";
                        dr["CSTID"] = preTable.Rows[i]["CSTID"].ToString();
                        dr["PROCID"] = Util.GetCondition(cboOp);
                        dr["USERID"] = LoginInfo.USERID;


                        dt.Rows.Add(dr);

                    }

                    ShowLoadingIndicator();
                    new ClientProxy().ExecuteService_Multi("BR_SET_TRAY_CHANGE_TRAY_OP_MULTI_MB", "INDATA", "OUTDATA", (bizResult, bizException) =>
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
                                //변경완료하였습니다.
                                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("FM_ME_0136"), null, "Information", MessageBoxButton.OK, MessageBoxIcon.None, (result) =>
                                {
                                    if (result == MessageBoxResult.OK)
                                    {
                                        btnDeleteAll_Click(null, null);
                                    }
                                });
                            }
                            //switch (bizResult.Tables[0].Rows[0]["RESULT"].ToString())
                            //{
                            //    case "0":
                            //        //변경완료하였습니다
                            //        LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("FM_ME_0136"), null, "Information", MessageBoxButton.OK, MessageBoxIcon.None, (result) =>
                            //        {
                            //            if (result == MessageBoxResult.OK)
                            //            {
                            //                GetTrayInfo(false);
                            //            }


                            //        });
                            //        break;

                            //    case "1":
                            //        //이전 작업 공정 종료, Tray 정보 화면 내 정상종료를 실행해 주세요.
                            //        LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("FM_ME_0195"), null, "Information", MessageBoxButton.OK, MessageBoxIcon.None, (result) =>
                            //        {
                            //            if (result == MessageBoxResult.OK)
                            //            {
                            //                GetTrayInfo(false);
                            //            }
                            //        });
                            //        break;

                            //    case "2":
                            //        //작업 불가 공정 입니다.(델타, 판정)
                            //        LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("FM_ME_0198"), null, "Information", MessageBoxButton.OK, MessageBoxIcon.None, (result) =>
                            //        {
                            //            if (result == MessageBoxResult.OK)
                            //            {
                            //                GetTrayInfo(false);
                            //            }
                            //        });
                            //        break;

                            //    case "3":
                            //        //충방전기 열,단 정보가 없습니다.
                            //        LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("FM_ME_0243"), null, "Information", MessageBoxButton.OK, MessageBoxIcon.None, (result) =>
                            //        {
                            //            if (result == MessageBoxResult.OK)
                            //            {
                            //                GetTrayInfo(false);
                            //            }
                            //        });
                            //        break;

                            //    case "4":
                            //        //충방전기작업과 맞지않습니다.
                            //        LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("FM_ME_0244"), null, "Information", MessageBoxButton.OK, MessageBoxIcon.None, (result) =>
                            //        {
                            //            if (result == MessageBoxResult.OK)
                            //            {
                            //                GetTrayInfo(false);
                            //            }
                            //        });
                            //        break;

                            //    case "5":
                            //        //현재 예약된 Tray와 공정 정보가 맞지 않습니다.
                            //        LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("FM_ME_0264"), null, "Information", MessageBoxButton.OK, MessageBoxIcon.None, (result) =>
                            //        {
                            //            if (result == MessageBoxResult.OK)
                            //            {
                            //                GetTrayInfo(false);
                            //            }
                            //        });
                            //        break;

                            //    default:
                            //        //Tray 정보 변경 중 오류가 발생하였습니다.
                            //        LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("FM_ME_0072"), null, "Information", MessageBoxButton.OK, MessageBoxIcon.None, (result) =>
                            //        {
                            //            if (result == MessageBoxResult.OK)
                            //            {
                            //                GetTrayInfo(false);
                            //            }
                            //        });
                            //        break;

                            //}
                            //GetEqpCurTray();

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
                });
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

        private void btnTapingStart_Click(object sender, RoutedEventArgs e)
        {
            //try
            //{
            //    //변경하시겠습니까?
            //    Util.MessageConfirm("FM_ME_0337", (re) =>
            //    {
            //        if (re != MessageBoxResult.OK)
            //        {
            //            return;
            //        }

            //        DataTable preTable = new DataTable();
            //        if (dgList.Rows.Count > 0)
            //        {
            //            preTable = DataTableConverter.Convert(dgList.ItemsSource);
            //        }
            //        else
            //        {
            //            Util.MessageValidation("FM_ME_0240");  //처리할 데이터가 없습니다.
            //            return;
            //        }

            //        DataTable dtRqst = new DataTable();
            //        dtRqst.TableName = "INDATA";
            //        dtRqst.Columns.Add("CSTID", typeof(string));
            //        dtRqst.Columns.Add("EQPTID", typeof(string));
            //        dtRqst.Columns.Add("USERID", typeof(string));

            //        for (int i = 0; i < preTable.Rows.Count; i++)
            //        {
            //            DataRow dr = dtRqst.NewRow();
            //            dr["CSTID"] = preTable.Rows[i]["CSTID"].ToString();
            //            dr["EQPTID"] = Util.GetCondition(cboToRoute, sMsg: "FM_ME_0171");   //설비를 선택해주세요.
            //            if (string.IsNullOrEmpty(dr["EQPTID"].ToString())) return;
            //            dr["USERID"] = LoginInfo.USERID;
            //            dtRqst.Rows.Add(dr);
            //        }

            //        ShowLoadingIndicator();
            //        new ClientProxy().ExecuteService("BR_SET_DSF_TRAY_START_MB", "INDATA", null, dtRqst, (bizResult, bizException) =>
            //        {
            //            try
            //            {
            //                if (bizException != null)
            //                {
            //                    Util.MessageException(bizException);
            //                    return;
            //                }

            //                //변경완료하였습니다.
            //                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("FM_ME_0136"), null, "Information", MessageBoxButton.OK, MessageBoxIcon.None, (result) =>
            //                {
            //                    if (result == MessageBoxResult.OK)
            //                    {
            //                    }
            //                });
            //            }
            //            catch (Exception ex)
            //            {
            //                HiddenLoadingIndicator();
            //                Util.MessageException(ex);
            //            }
            //        });
            //    });
            //}

            //catch (Exception ex)
            //{
            //    LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            //}
            //finally
            //{
            //    HiddenLoadingIndicator();
            //}
        }

        private void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            //삭제 하시겠습니까?
            Util.MessageConfirm("FM_ME_0155", (result) =>
            {
                if (result == MessageBoxResult.OK)
                {
                   
                    int index = ((C1.WPF.DataGrid.DataGridCellPresenter)((sender as Button).Parent)).Row.Index;

                    DataTable dt = DataTableConverter.Convert(dgList.ItemsSource);
                    DataRow dr = dt.Rows[index];
                    dt.Rows.Remove(dr);
                  
                    dgList.ItemsSource = null;

                    Util.GridSetData(dgList, dt, FrameOperation, true);

                    //dgList.IsReadOnly = false;
                    //dgList.RemoveRow(index);
                    dgList.IsReadOnly = true;

                }
            });
        }

        private void btnInput_Click(object sender, RoutedEventArgs e)
        {
            GetList();
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

            Util _util = new Util();
            if (string.IsNullOrEmpty(txtTrayID.Text)) return;

            //스프레드에 있는지 확인
            int iRow = -1;

            if (dgList.Rows.Count > 0)
            {
                iRow = _util.GetDataGridRowIndex(dgList, dgList.Columns["CSTID"].Name, txtTrayID.Text.Trim());
                if (iRow > -1)
                {
                    Util.MessageValidation("FM_ME_0193");  //이미 스캔한 ID 입니다.
                    return;
                }
            }

            try
            {
                DataTable preTable = new DataTable();
                if (dgList.Rows.Count > 0)
                {
                    preTable = DataTableConverter.Convert(dgList.ItemsSource);
                }

                if (dgList.Rows.Count >= 20)
                {
                    Util.MessageValidation("FM_ME_0463");  //최대 20개 까지 입니다.
                    return;
                }

                DataTable inDataTable = new DataTable();
                inDataTable.Columns.Add("LANGID", typeof(string));
                inDataTable.Columns.Add("CSTID", typeof(string));

                DataRow newRow = inDataTable.NewRow();
                newRow["LANGID"] = LoginInfo.LANGID;
                newRow["CSTID"] = txtTrayID.Text.Trim();

                inDataTable.Rows.Add(newRow);
                ShowLoadingIndicator();

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_SEL_TRAY_INFO_RETRIEVE_MB", "INDATA", "OUTDATA", inDataTable);

                if (dtRslt.Rows.Count == 0)
                {
                    Util.MessageValidation("FM_ME_0232");  //조회된 값이 없습니다.
                    return;
                }

                //최초 Tray 일경우 처리
                if (dgList.Rows.Count == 0)
                {
                    txtRoute.Text = dtRslt.Rows[0]["ROUTID"].ToString();
                    txtRoute.Tag = dtRslt.Rows[0]["ROUTID"].ToString();
                    txtOp.Text = dtRslt.Rows[0]["PROCNAME"].ToString();
                    txtOp.Tag = dtRslt.Rows[0]["PROCID"].ToString();

                    CommonCombo_Form_MB _combo = new CommonCombo_Form_MB();

                    string sProcList = GetRoutProcByProcGRCode(dtRslt.Rows[0]["ROUTID"].ToString());
                    string[] sFilter = { dtRslt.Rows[0]["ROUTID"].ToString(), null, null, null, sProcList };
                    _combo.SetCombo(cboOp, CommonCombo_Form_MB.ComboStatus.SELECT, sCase: "ROUTE_OP", sFilter: sFilter);
                }
                //최초 Scan한 Tray와 비교
                else if (!dtRslt.Rows[0]["ROUTID"].ToString().Equals(txtRoute.Tag))
                { 
                  
                        Util.MessageValidation("FM_ME_0101");  //공정경로가 동일하지 않습니다.
                        return;
                   
                }

                else
                {
                    //최초 Scan한 Tray와 비교
                    if (!dtRslt.Rows[0]["PROCID"].ToString().Equals(txtOp.Tag))
                    {
                        Util.MessageValidation("SFU4167");  //동일한 공정이 아닙니다.
                        return;
                    }
                }
                if (preTable.Rows.Count > 0)
                {
                    DataRow row = preTable.NewRow();
                    row["CSTID"] = Util.NVC(dtRslt.Rows[0]["CSTID"]);
                    row["ROUTID"] = Util.NVC(dtRslt.Rows[0]["ROUTID"]);
                    row["PROCID"] = Util.NVC(dtRslt.Rows[0]["PROCID"]);
                    row["PROCNAME"] = Util.NVC(dtRslt.Rows[0]["PROCNAME"]);
                    row["WIPSNAME"] = Util.NVC(dtRslt.Rows[0]["WIPSNAME"]);
                    row["ISS_RSV_FLAG"] = Util.NVC(dtRslt.Rows[0]["ISS_RSV_FLAG"]);
                    row["EQSGID"] = Util.NVC(dtRslt.Rows[0]["EQSGID"]);
                    preTable.Rows.Add(row);
                    Util.GridSetData(dgList, preTable, FrameOperation, true);
                }
                else
                {
                    Util.GridSetData(dgList, dtRslt, FrameOperation, true);
                }

                if (dgList.Rows.Count > 1)
                {
                    chkReservation.IsChecked = false;
                    chkCPFReservation.IsChecked = false;
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
            finally
            {
                txtTrayID.Text = string.Empty;
                txtTrayID.Focus();
                txtTrayID.SelectAll();

                HiddenLoadingIndicator();
            }
        }

        private void GetListExcel(DataTable dt)
        {

            Util _util = new Util();
            string FirstROUT = string.Empty;
            string FirstOP = string.Empty;

            DataTable OUTDATA = new DataTable();
            try
            {
                ShowLoadingIndicator();

                for (int i = 0; i < dt.Rows.Count; i++)
                {

                    DataTable inDataTable = new DataTable();
                    inDataTable.Columns.Add("LANGID", typeof(string));
                    inDataTable.Columns.Add("CSTID", typeof(string));

                    DataRow newRow = inDataTable.NewRow();
                    newRow["LANGID"] = LoginInfo.LANGID;
                    newRow["CSTID"] = dt.Rows[i]["CSTID"].ToString().Trim();

                    inDataTable.Rows.Add(newRow);

                    DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_SEL_TRAY_INFO_RETRIEVE_MB", "INDATA", "OUTDATA", inDataTable);

                    if (dtRslt.Rows.Count == 0)
                    {
                        Util.MessageValidation("FM_ME_0232");  //조회된 값이 없습니다.
                        return;
                    }

                    //최초 Tray 일경우 처리
                    if (string.IsNullOrEmpty(FirstROUT))
                    {
                        OUTDATA = dtRslt.Copy();

                        FirstROUT = dtRslt.Rows[0]["ROUTID"].ToString();
                        FirstOP = dtRslt.Rows[0]["PROCNAME"].ToString();

                        txtOp.Text = FirstOP;
                        txtRoute.Text = FirstROUT;

                        CommonCombo_Form_MB _combo = new CommonCombo_Form_MB();

                        string sProcList = GetRoutProcByProcGRCode(dtRslt.Rows[0]["ROUTID"].ToString());
                        string[] sFilter = { dtRslt.Rows[0]["ROUTID"].ToString(), null, null, null, sProcList };
                        _combo.SetCombo(cboOp, CommonCombo_Form_MB.ComboStatus.SELECT, sCase: "ROUTE_OP", sFilter: sFilter);
                    }
                    //최초 Scan한 Tray와 비교
                    else if (FirstROUT != dtRslt.Rows[0]["ROUTID"].ToString())
                    {

                        Util.MessageValidation("FM_ME_0101");  //공정경로가 동일하지 않습니다.
                        return;

                    }

                    else
                    {
                        //최초 Scan한 Tray와 비교
                        if (FirstOP != dtRslt.Rows[0]["PROCNAME"].ToString())
                        {
                            Util.MessageValidation("SFU4167");  //동일한 공정이 아닙니다.
                            return;
                        }


                        OUTDATA.Merge(dtRslt);
                    }
                }
              Util.GridSetData(dgList, OUTDATA, FrameOperation, true);

                if (dgList.Rows.Count > 1)
                {
                    chkReservation.IsChecked = false;
                    chkCPFReservation.IsChecked = false;
                }

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
            finally
            {
                txtTrayID.Text = string.Empty;
                txtTrayID.Focus();
                txtTrayID.SelectAll();

                HiddenLoadingIndicator();
            }
        }
        private string GetRoutProcByProcGRCode(string sRoutID)
        {
            string sRtnValue = string.Empty;
            try
            {
                DataTable dtRqst = new DataTable();
                dtRqst.TableName = "RQSTDT";
                dtRqst.Columns.Add("LANGID", typeof(string));
                dtRqst.Columns.Add("ROUTID", typeof(string));
                dtRqst.Columns.Add("PROC_GRP_CODE", typeof(string));

                DataRow dr = dtRqst.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["ROUTID"] = sRoutID;
                dr["PROC_GRP_CODE"] = "A";
                dtRqst.Rows.Add(dr);

                ShowLoadingIndicator();
                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_SEL_ROUT_PROC_BY_PROCGRP_MB", "RQSTDT", "RSLTDT", dtRqst);

                if (dtRslt.Rows.Count > 0)
                {
                    foreach (DataRow drRslt in dtRslt.Rows)
                    {
                        sRtnValue += drRslt["PROCID"].ToString() + ",";
                    }
                    sRtnValue = sRtnValue.Remove(sRtnValue.LastIndexOf(","));
                }
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
            return sRtnValue;
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

        private void chkReservation_Checked(object sender, RoutedEventArgs e)
        {
            if(dgList.Rows.Count>1)
            {
                chkReservation.IsChecked = false;
                return;
            }


            chkCPFReservation.IsChecked = false;

            cboLane.IsEnabled = true;
            cboStg.IsEnabled = true;
            cboCol.IsEnabled = true;
            cboRow.IsEnabled = true;
        }

        private void chkReservation_Unchecked(object sender, RoutedEventArgs e)
        {
            cboLane.IsEnabled = false;
            cboStg.IsEnabled = false;
            cboCol.IsEnabled = false;
            cboRow.IsEnabled = false;
        }

        private void chkCPFReservation_Checked(object sender, RoutedEventArgs e)
        {
            if (dgList.Rows.Count > 1)
            {
                chkCPFReservation.IsChecked = false;
                return;
            }
            chkReservation.IsChecked = false;

            cboCPFLane.IsEnabled = true;
            cboCPFRow.IsEnabled = true;
            cboCPFStg.IsEnabled = true;
            cboCPFCol.IsEnabled = true;
        }

        private void chkCPFReservation_Unchecked(object sender, RoutedEventArgs e)
        {
            cboCPFLane.IsEnabled = false;
            cboCPFRow.IsEnabled = false;
            cboCPFStg.IsEnabled = false;
            cboCPFCol.IsEnabled = false;
        }

        private void btnExcel_Click(object sender, RoutedEventArgs e)
        {
            LoadExcel();
        }

        private void LoadExcel()
        {
           

            OpenFileDialog fd = new OpenFileDialog();

            if (ConfigurationManager.AppSettings["APP_CONFIG_SBC_MODE"] == "SBC")
            {
                fd.InitialDirectory = @"\\Client\C$";
            }

            fd.Filter = "Excel Files (.xlsx)|*.xlsx";


            if (fd.ShowDialog() == true)
            {
                using (Stream stream = fd.OpenFile())
                {
                    C1XLBook book = new C1XLBook();
                    book.Load(stream, FileFormat.OpenXml);
                    XLSheet sheet = book.Sheets[0];
                    DataTable dataTable = new DataTable();
                    dataTable.Columns.Add("CSTID", typeof(string));

                    if (sheet.Rows.Count > 20)
                    {
                        Util.MessageValidation("FM_ME_0463");  //최대 20개 까지 입니다.
                        return;
                    }

                    for (int rowInx = 0; rowInx < sheet.Rows.Count; rowInx++)
                    {
                        // CELL ID;
                        if (sheet.GetCell(rowInx, 0) == null)
                            return;

                        string CELL_ID = Util.NVC(sheet.GetCell(rowInx, 0).Text);
                        DataRow dataRow = dataTable.NewRow();
                        dataRow["CSTID"] = CELL_ID;
                        dataTable.Rows.Add(dataRow);
                    }

                    GetListExcel(dataTable);

                  
                }
            }
        }

        private void cboLane_SelectionCommitted(object sender, PropertyChangedEventArgs<object> e)
        {
            SetSearchFormationBoxList(Util.GetCondition(cboLane));
        }

        private void cboCPFLane_SelectionCommitted(object sender, PropertyChangedEventArgs<object> e)
        {
            SetSearchCPFFormationBoxList(Util.GetCondition(cboCPFLane));
        }

        private void SetSearchFormationBoxList(string sLaneID)
        {
            try
            {
                cboRow.Text = string.Empty;
                cboCol.Text = string.Empty;
                cboStg.Text = string.Empty;

                CommonCombo_Form_MB _combo = new CommonCombo_Form_MB();

                string[] sFilter = { "1", sLaneID };    //EquipmentAttr : S70(설비 그룹 유형 코드[EQPT_GR_TYPE_CODE]), S71(LANE 아이디[LANE_ID])
                _combo.SetCombo(cboRow, CommonCombo_Form_MB.ComboStatus.NONE, sFilter: sFilter, sCase: "ROW");
                _combo.SetCombo(cboCol, CommonCombo_Form_MB.ComboStatus.NONE, sFilter: sFilter, sCase: "COL");
                _combo.SetCombo(cboStg, CommonCombo_Form_MB.ComboStatus.NONE, sFilter: sFilter, sCase: "STG");
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }

        private void SetSearchCPFFormationBoxList(string sLaneID)
        {
            try
            {
                cboCPFRow.Text = string.Empty;
                cboCPFCol.Text = string.Empty;
                cboCPFStg.Text = string.Empty;

                CommonCombo_Form_MB _combo = new CommonCombo_Form_MB();

                string[] sFilter = { "L", sLaneID };    //EquipmentAttr : S70(설비 그룹 유형 코드[EQPT_GR_TYPE_CODE]), S71(LANE 아이디[LANE_ID])
                _combo.SetCombo(cboCPFRow, CommonCombo_Form_MB.ComboStatus.NONE, sFilter: sFilter, sCase: "ROW");
                _combo.SetCombo(cboCPFCol, CommonCombo_Form_MB.ComboStatus.NONE, sFilter: sFilter, sCase: "COL");
                _combo.SetCombo(cboCPFStg, CommonCombo_Form_MB.ComboStatus.NONE, sFilter: sFilter, sCase: "STG");
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }

        private void cboOp_SelectionCommitted(object sender, PropertyChangedEventArgs<object> e)
        {
            try
            {
                CommonCombo_Form_MB _combo = new CommonCombo_Form_MB();

                string sProcGrCode = string.Empty;  //공정 그룹코드
                string sRProcDetlTypeCode = string.Empty;   //공정 상세유형 코드

                DataTable dtRqst = new DataTable();
                dtRqst.TableName = "RQSTDT";
                dtRqst.Columns.Add("PROCID", typeof(string));

                DataRow dr = dtRqst.NewRow();
                dr["PROCID"] = Util.GetCondition(cboOp, bAllNull: true);
                dtRqst.Rows.Add(dr);

                ShowLoadingIndicator();
                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_PROCESSATTR_TBL", "RQSTDT", "RSLTDT", dtRqst);

                if (dtRslt.Rows.Count > 0)
                {
                    sProcGrCode = dtRslt.Rows[0]["S26"].ToString();
                    sRProcDetlTypeCode = dtRslt.Rows[0]["S27"].ToString();
                }
                else
                {
                    return;
                }

                if (sProcGrCode.Equals("1"))
                {
                    if (sRProcDetlTypeCode.Equals("1A") || sRProcDetlTypeCode.Equals("1B") || sRProcDetlTypeCode.Equals("1C"))
                    {
                        chkCPFReservation.IsEnabled = true;
              
                        chkReservation.IsEnabled = false;
                        chkReservation.IsChecked = false;
                    }
                    else
                    {
                        chkReservation.IsEnabled = true;
       
                        chkCPFReservation.IsEnabled = false;
                        chkCPFReservation.IsChecked = false;
                    }
                }
                else
                {
                    chkReservation.IsEnabled = false;
                    chkReservation.IsChecked = false;
                    chkCPFReservation.IsEnabled = false;
                    chkCPFReservation.IsChecked = false;
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
        }
    }
}