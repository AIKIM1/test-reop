/*************************************************************************************
 Created Date : 2016.06.16
      Creator : 
   Decription : 
--------------------------------------------------------------------------------------
 [Change History]
  2016.06.16  DEVELOPER : Initial Created.
  2023.03.16  LEEHJ         소형활성화MES(기존 FCS002_315_CHANGE_CELL) 파우치 전용

  
 
**************************************************************************************/

using C1.WPF;
using C1.WPF.DataGrid.Summaries;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace LGC.GMES.MES.FCS002
{
    public partial class FCS002_315_CHANGE_CELL : C1Window, IWorkArea
    {
        #region Declaration & Constructor 

        private Util _Util = new Util();
        
        string _userID = string.Empty;
        string _inboxID = string.Empty;
        string _boxID = string.Empty;

        /// <summary>
        ///  Frame과 상호작용하기 위한 객체
        /// </summary>
        public IFrameOperation FrameOperation
        {
            get;
            set;
        }

        public FCS002_315_CHANGE_CELL()
        {
            InitializeComponent();
        }
        #endregion

        #region Initialize

        private void C1Window_Loaded(object sender, RoutedEventArgs e)
        {
            InitControl();
        }

        private void InitControl()
        {
            object[] tmps = C1WindowExtension.GetParameters(this);
            _boxID = tmps[0] as string;
            _userID = tmps[1] as string;
            _inboxID = tmps[2] as string;

            DataTable dt = new DataTable();
            dt.Columns.Add("SEQ");
            dt.Columns.Add("CELLID");
            dt.Columns.Add("PRINTID");

            dgSource.ItemsSource = DataTableConverter.Convert(dt.Copy());
            dgTarget.ItemsSource = DataTableConverter.Convert(dt.Copy());
        }

        #endregion

        #region Event
     
       private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (dgSource.GetRowCount() <= 0)
                { 
                    //SFU1462		교체 대상 Cell ID가 입력되지 않았습니다.
                    Util.MessageValidation("SFU1462", (result) =>
                    {
                        if (result == MessageBoxResult.OK)
                        {
                            txtSourceID.Focus();
                            txtSourceID.Text = string.Empty;
                        }
                    });
                    return;
                }

                else if (dgSource.GetRowCount() < dgTarget.GetRowCount())
                {
                    //SFU1462		교체 대상 Cell ID가 입력되지 않았습니다.
                    Util.MessageValidation("SFU1462", (result) =>
                    {
                        if (result == MessageBoxResult.OK)
                        {
                            txtSourceID.Focus();
                            txtSourceID.Text = string.Empty;
                        }
                    });
                    return;
                }

                else if (dgSource.GetRowCount() > dgTarget.GetRowCount())
                {
                    //CELLID를 스캔 또는 입력하세요.
                    Util.MessageValidation("SFU1462", (result) =>
                    {
                        if (result == MessageBoxResult.OK)
                        {
                            txtTargetID.Focus();
                            txtTargetID.Text = string.Empty;
                        }
                    });
                    return;
                }

                //SFU1465	교체처리 하시겠습니까?	
                Util.MessageConfirm("SFU1465", (result) =>
                {
                    if (result == MessageBoxResult.OK)
                    {
                        DataTable dtSource = DataTableConverter.Convert(dgSource.ItemsSource);
                        DataTable dtTarget = DataTableConverter.Convert(dgTarget.ItemsSource);

                        DataSet indataSet = new DataSet();
                        DataTable inDataTable = indataSet.Tables.Add("INDATA");
                        inDataTable.Columns.Add("BOXID");
                        inDataTable.Columns.Add("USERID");
                        inDataTable.Columns.Add("FORM_INBOX");

                        DataRow newRow = inDataTable.NewRow();
                        newRow["BOXID"] = _boxID;
                        newRow["USERID"] = _userID;
                        newRow["FORM_INBOX"] = _inboxID;

                        inDataTable.Rows.Add(newRow);

                        DataTable inSublotTable = indataSet.Tables.Add("INSUBLOT");
                        inSublotTable.Columns.Add("SUBLOTID");
                        inSublotTable.Columns.Add("BOX_PSTN_NO");

                        foreach (DataRow dr in dtTarget.Rows)
                        {
                            newRow = inSublotTable.NewRow();
                            newRow["SUBLOTID"] = dr["CELLID"];
                            newRow["BOX_PSTN_NO"] = 0;
                            inSublotTable.Rows.Add(newRow);
                        }
                       

                        DataTable inSublotDelTable = indataSet.Tables.Add("INSUBLOT_DELETE");
                        inSublotDelTable.Columns.Add("SUBLOTID");
                        
                        foreach (DataRow dr in dtSource.Rows)
                        {
                            newRow = inSublotDelTable.NewRow();
                            newRow["SUBLOTID"] = dr["CELLID"];
                            inSublotDelTable.Rows.Add(newRow);
                        }
                        loadingIndicator.Visibility = Visibility.Visible;
                        // 1차 포장 셀 교체
                        new ClientProxy().ExecuteService_Multi("BR_PRD_REG_SUBLOT_REPLACE_NJ", "INDATA,INSUBLOT,INSUBLOT_DELETE", null, (bizResult, bizException) =>
                        {
                            try
                            {
                                loadingIndicator.Visibility = Visibility.Collapsed;
                                if (bizException != null)
                                {
                                    Util.MessageException(bizException);
                                    return;
                                }

                                this.DialogResult = MessageBoxResult.OK;
                                this.Close();
                            }
                            catch (Exception ex)
                            {
                                Util.MessageException(ex);
                            }
                        }, indataSet);
                    }
                });

            }
            catch (Exception ex)
            { }
        }
        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = MessageBoxResult.Cancel;
            this.Close();
        }
        


        #endregion

        #region Mehod        


        #endregion

        private void btnTarget_Click(object sender, RoutedEventArgs e)
        {
            txtTargetID.Text = string.Empty;
            Util.gridClear(dgTarget);
        }

        private void txtTargetID_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            try
            {
                if (e.Key == System.Windows.Input.Key.Enter)
                {
                    string sCellID = txtTargetID.Text.Trim();

                    if (dgTarget.GetRowCount() > 0)
                    {
                        DataTable dtInfo = DataTableConverter.Convert(dgTarget.ItemsSource);
                        DataRow[] drList = dtInfo.Select("CELLID = '" + sCellID + "'");

                        if (drList.Length > 0)
                        {
                            // SFU3159 아래쪽 List에 이미 존재하는 CELL ID입니다.
                            ControlsLibrary.MessageBox.ShowNoEnter(MessageDic.Instance.GetMessage("SFU3099"), null, "Error", MessageBoxButton.OK, MessageBoxIcon.Warning, (result) =>
                            {
                                if (result == MessageBoxResult.OK)
                                {
                                    txtTargetID.Focus();
                                    txtTargetID.Text = string.Empty;
                                }
                            });

                            txtTargetID.Text = string.Empty;
                            //  txtCellID.Focus();
                            return;
                        }
                    }

                    DataTable RQSTDT = new DataTable("INDATA");
                    RQSTDT.Columns.Add("BOXID");
                    RQSTDT.Columns.Add("SUBLOTID");
                    //         RQSTDT.Columns.Add("BOX_PSTN_NO");
                    RQSTDT.Columns.Add("USERID");

                    DataRow dr = RQSTDT.NewRow();
                    dr["BOXID"] = _boxID;
                    dr["SUBLOTID"] = sCellID;
                    //       dr["BOX_PSTN_NO"] = 1;
                    dr["USERID"] = _userID;
                    RQSTDT.Rows.Add(dr);

                    DataTable dtRslt = new ClientProxy().ExecuteServiceSync("BR_PRD_CHK_SUBLOT_NJ", "INDATA", "OUTDATA", RQSTDT);

                    if (dtRslt != null)
                    {
                        DataTable dtInfo = DataTableConverter.Convert(dgTarget.ItemsSource);
                        DataRow drInfo = dtInfo.NewRow();
                        drInfo["SEQ"] = dtInfo.Rows.Count + 1;
                        drInfo["CELLID"] = dtRslt.Rows[0]["SUBLOTID"];
                        drInfo["PRINTID"] = dtRslt.Rows[0]["PRINTID"];
                        dtInfo.Rows.Add(drInfo);
                        Util.GridSetData(dgTarget, dtInfo, FrameOperation, true);
                    }

                    txtTargetID.Text = string.Empty;
                    txtTargetID.Focus();
                    dgTarget.ScrollIntoView(dgTarget.Rows.Count-1, 0);

                    if (dgTarget.Rows.Count > 0)
                    {
                        DataGridAggregate.SetAggregateFunctions(dgTarget.Columns["CELLID"], new DataGridAggregatesCollection { new DataGridAggregateCount() });
                    }
                }
            }
            catch (Exception ex)
            {
                Util.MessageExceptionNoEnter(ex, msgResult =>
                {
                    if (msgResult == MessageBoxResult.OK)
                    {
                        txtTargetID.Text = string.Empty;
                        txtTargetID.Focus();
                    }
                });
            }
            finally
            {
            }
        }

        private void txtSourceID_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            try
            {
                if (e.Key == System.Windows.Input.Key.Enter)
                {
                    string sCellID = txtSourceID.Text.Trim();

                    if (dgSource.GetRowCount() > 0)
                    {
                        DataTable dtInfo = DataTableConverter.Convert(dgSource.ItemsSource);
                        DataRow[] drList = dtInfo.Select("CELLID = '" + sCellID + "'");

                        if (drList.Length > 0)
                        {
                            // SFU3159 아래쪽 List에 이미 존재하는 CELL ID입니다.
                            ControlsLibrary.MessageBox.ShowNoEnter(MessageDic.Instance.GetMessage("SFU3099"), null, "Error", MessageBoxButton.OK, MessageBoxIcon.Warning, (result) =>
                            {
                                if (result == MessageBoxResult.OK)
                                {
                                    txtSourceID.Focus();
                                    txtSourceID.Text = string.Empty;
                                }
                            });

                            txtSourceID.Text = string.Empty;
                            //  txtCellID.Focus();
                            return;
                        }
                    }

                    DataTable RQSTDT = new DataTable("INDATA");
                    RQSTDT.Columns.Add("BOXID");
                    RQSTDT.Columns.Add("SUBLOTID");
                    //         RQSTDT.Columns.Add("BOX_PSTN_NO");
                    RQSTDT.Columns.Add("USERID");

                    DataRow dr = RQSTDT.NewRow();
                    dr["BOXID"] = _boxID;
                    dr["SUBLOTID"] = sCellID;
                    //       dr["BOX_PSTN_NO"] = 1;
                    dr["USERID"] = _userID;
                    RQSTDT.Rows.Add(dr);

                    DataTable dtRslt = new ClientProxy().ExecuteServiceSync("BR_PRD_GET_CELL_ID_NJ", "INDATA", "OUTDATA", RQSTDT);

                    if (dtRslt != null)
                    {
                        if (dgSource.GetRowCount() > 0)
                        {
                            DataTable dt = DataTableConverter.Convert(dgSource.ItemsSource); ;
                            DataRow[] drList = dt.Select("CELLID = '" + dtRslt.Rows[0]["SUBLOTID"] + "'");

                            if (drList.Length > 0)
                            {
                                // SFU3159 아래쪽 List에 이미 존재하는 CELL ID입니다.
                                ControlsLibrary.MessageBox.ShowNoEnter(MessageDic.Instance.GetMessage("SFU3099"), null, "Error", MessageBoxButton.OK, MessageBoxIcon.Warning, (result) =>
                                {
                                    if (result == MessageBoxResult.OK)
                                    {
                                        txtSourceID.Focus();
                                        txtSourceID.Text = string.Empty;
                                    }
                                });

                                txtSourceID.Text = string.Empty;
                                return;
                            }
                        }

                        DataTable dtInfo = DataTableConverter.Convert(dgSource.ItemsSource);
                        DataRow drInfo = dtInfo.NewRow();
                        drInfo["SEQ"] = dtInfo.Rows.Count + 1;
                        drInfo["CELLID"] = dtRslt.Rows[0]["SUBLOTID"];
                        drInfo["PRINTID"] = dtRslt.Rows[0]["PRINTID"];
                        dtInfo.Rows.Add(drInfo);
                        Util.GridSetData(dgSource, dtInfo, FrameOperation, true);

                        txtSourceID.Focus();
                        txtSourceID.Text = string.Empty;
                        dgSource.ScrollIntoView(dgSource.Rows.Count - 1, 0);
                        if (dgSource.Rows.Count > 0)
                        {
                            DataGridAggregate.SetAggregateFunctions(dgSource.Columns["CELLID"], new DataGridAggregatesCollection { new DataGridAggregateCount() });
                        }
                    }
                    txtSourceID.Text = string.Empty;
                    txtSourceID.Focus();
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void btnClearSource_Click(object sender, RoutedEventArgs e)
        {
            txtSourceID.Text = string.Empty;
            Util.gridClear(dgSource);
        }

        private void text_GotFocus(object sender, RoutedEventArgs e)
        {
            InputMethod.Current.ImeConversionMode = ImeConversionModeValues.Alphanumeric;
        }
    }
}
