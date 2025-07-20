/*************************************************************************************
 Created Date : 2022.12.05
      Creator : 
   Decription : 다중 Tray 공정 변경
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
    public partial class FCS002_206 : UserControl, IWorkArea
    {
        #region Declaration & Constructor 

        Util _Util = new Util();

        public FCS002_206()
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

            txtLineId.Text = string.Empty;
            txtModelId.Text = string.Empty;
            txtRoute.Text = string.Empty;
            txtRoute.Tag = string.Empty;
            txtOp.Text = string.Empty;

            cboToRoute.ItemsSource = null;
            cboToOp.ItemsSource = null;
            cboToRoute.Text = string.Empty;
            cboToOp.Text = string.Empty;
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

                    DataSet indataSet = new DataSet();
                    DataTable dtIndata = indataSet.Tables.Add("INDATA");
                    dtIndata.Columns.Add("USERID", typeof(string));
                    dtIndata.Columns.Add("EQPTID", typeof(string));

                    DataRow InRow = dtIndata.NewRow();

                    InRow["USERID"] = LoginInfo.USERID;
                    InRow["EQPTID"] = DBNull.Value;
                    dtIndata.Rows.Add(InRow);

                    DataTable dtInCst = indataSet.Tables.Add("IN_CST");
                    dtInCst.Columns.Add("CSTID", typeof(string));
                    dtInCst.Columns.Add("PRE_ROUTID", typeof(string));
                    dtInCst.Columns.Add("AFTER_ROUTID", typeof(string));
                    dtInCst.Columns.Add("AFTER_PROCID", typeof(string));
                    
                    for (int i = 0; i < preTable.Rows.Count; i++)
                    {
                        DataRow dr = dtInCst.NewRow();
                        dr["CSTID"] = preTable.Rows[i]["CSTID"].ToString();
                        dr["PRE_ROUTID"] = txtRoute.Tag.ToString();
                        dr["AFTER_ROUTID"] = Util.GetCondition(cboToRoute, sMsg: "FM_ME_0106");    //공정경로를 선택해주세요.
                        if (string.IsNullOrEmpty(dr["AFTER_ROUTID"].ToString())) return;
                        dr["AFTER_PROCID"] = Util.GetCondition(cboToOp, sMsg: "FM_ME_0338");   //차기공정을 선택해주세요.
                        if (string.IsNullOrEmpty(dr["AFTER_PROCID"].ToString())) return;
                        dtInCst.Rows.Add(dr);
                    }

                    ShowLoadingIndicator();
                    new ClientProxy().ExecuteService_Multi("BR_SET_TRAY_ROUTE_CHANGE_MB", "INDATA,IN_CST", "OUTDATA", (bizResult, bizException) =>
                    {
                        try
                        {
                            if (bizException != null)
                            {
                                Util.MessageException(bizException);
                                return;
                            }

                            //변경완료하였습니다.
                            LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("FM_ME_0136"), null, "Information", MessageBoxButton.OK, MessageBoxIcon.None, (result) =>
                            {
                                if (result == MessageBoxResult.OK)
                                {
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
                    }, indataSet);
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

                    DataTable dtRqst = new DataTable();
                    dtRqst.TableName = "INDATA";
                    dtRqst.Columns.Add("CSTID", typeof(string));
                    dtRqst.Columns.Add("EQPTID", typeof(string));
                    dtRqst.Columns.Add("USERID", typeof(string));

                    for (int i = 0; i < preTable.Rows.Count; i++)
                    {
                        DataRow dr = dtRqst.NewRow();
                        dr["CSTID"] = preTable.Rows[i]["CSTID"].ToString();
                        dr["EQPTID"] = Util.GetCondition(cboToRoute, sMsg: "FM_ME_0171");   //설비를 선택해주세요.
                        if (string.IsNullOrEmpty(dr["EQPTID"].ToString())) return;
                        dr["USERID"] = LoginInfo.USERID;
                        dtRqst.Rows.Add(dr);
                    }

                    ShowLoadingIndicator();
                    new ClientProxy().ExecuteService("BR_SET_DSF_TRAY_START_MB", "INDATA", null, dtRqst, (bizResult, bizException) =>
                    {
                        try
                        {
                            if (bizException != null)
                            {
                                Util.MessageException(bizException);
                                return;
                            }

                            //변경완료하였습니다.
                            LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("FM_ME_0136"), null, "Information", MessageBoxButton.OK, MessageBoxIcon.None, (result) =>
                            {
                                if (result == MessageBoxResult.OK)
                                {
                                }
                            });
                        }
                        catch (Exception ex)
                        {
                            HiddenLoadingIndicator();
                            Util.MessageException(ex);
                        }
                    });
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
                    string sLoadCST = dr["LOAD_REP_CSTID"].ToString();
                    DataRow[] dr1 = dt.Select("LOAD_REP_CSTID = '"+ sLoadCST+ "'");

                    for(int i = 0;i<dr1.Length;i++)
                    {
                        dt.Rows.Remove(dr1[i]);
                    }
                  
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

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_SEL_TRAY_INFO_CHG_PROC_MB", "INDATA", "OUTDATA", inDataTable);

                if (dtRslt.Rows.Count == 0)
                {
                    Util.MessageValidation("FM_ME_0232");  //조회된 값이 없습니다.
                    return;
                }

                //최초 Tray 일경우 처리
                if (dgList.Rows.Count == 0)
                {
                    txtLineId.Text = dtRslt.Rows[0]["EQSGNAME"].ToString();
                    txtModelId.Text = dtRslt.Rows[0]["MDLLOT_ID"].ToString();
                    txtRoute.Text = dtRslt.Rows[0]["ROUT_NAME"].ToString();
                    txtRoute.Tag = dtRslt.Rows[0]["ROUTID"].ToString();
                    txtOp.Text = dtRslt.Rows[0]["PROCNAME"].ToString();
                    txtOp.Tag = dtRslt.Rows[0]["PROCID"].ToString();

                    CommonCombo_Form_MB _combo = new CommonCombo_Form_MB();

                    string[] sFilter = { dtRslt.Rows[0]["EQSGID"].ToString(), dtRslt.Rows[0]["MDLLOT_ID"].ToString(), null, null, null };

                    C1ComboBox[] cboRouteChild = { cboToOp };
                    _combo.SetCombo(cboToRoute, CommonCombo_Form_MB.ComboStatus.SELECT, sCase: "ROUTE", sFilter: sFilter, cbChild: cboRouteChild);

                    C1ComboBox[] cboOperParent = { cboToRoute };
                    _combo.SetCombo(cboToOp, CommonCombo_Form_MB.ComboStatus.SELECT, sCase: "ROUTE_OP", cbParent: cboOperParent);
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
                    for (int i = 0; i < dtRslt.Rows.Count; i++)
                    {
                        bool bCheck = false;
                        for (int j = 0; j < preTable.Rows.Count; j++)
                        {
                            if (preTable.Rows[j]["CSTID"].ToString().Equals(Util.NVC(dtRslt.Rows[i]["CSTID"])))
                                bCheck = true;
                        }

                        if (bCheck == true)
                            continue;


                            DataRow row = preTable.NewRow();
                        row["CSTID"] = Util.NVC(dtRslt.Rows[i]["CSTID"]);
                        row["ROUTID"] = Util.NVC(dtRslt.Rows[i]["ROUTID"]);
                        row["ROUT_NAME"] = Util.NVC(dtRslt.Rows[i]["ROUT_NAME"]);
                        row["PROCID"] = Util.NVC(dtRslt.Rows[i]["PROCID"]);
                        row["PROCNAME"] = Util.NVC(dtRslt.Rows[i]["PROCNAME"]);
                        row["ISS_RSV_FLAG"] = Util.NVC(dtRslt.Rows[i]["ISS_RSV_FLAG"]);
                        row["MDLLOT_ID"] = Util.NVC(dtRslt.Rows[i]["MDLLOT_ID"]);
                        row["EQSGID"] = Util.NVC(dtRslt.Rows[i]["EQSGID"]);
                        row["EQSGNAME"] = Util.NVC(dtRslt.Rows[i]["EQSGNAME"]);
                        row["LOAD_REP_CSTID"] = Util.NVC(dtRslt.Rows[i]["LOAD_REP_CSTID"]);
                        preTable.Rows.Add(row);

                    }
                    Util.GridSetData(dgList, preTable, FrameOperation, true);
                }
                else
                {
                    Util.GridSetData(dgList, dtRslt, FrameOperation, true);
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