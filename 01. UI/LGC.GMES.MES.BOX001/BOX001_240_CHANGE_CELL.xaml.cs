/*************************************************************************************
 Created Date : 2016.06.16
      Creator : 
   Decription : 
--------------------------------------------------------------------------------------
 [Change History]
  2016.06.16  DEVELOPER : Initial Created.
  2023.09.06  이병윤 : E20230704-000395 1. cell추가 : 동일한 MODEL/LOT/ Grader 비교를 위한 indata 컬럼추가
                                       2. outbox생성 : inbox입력데이터 정규식 제거
                                       3. 동/부모창에 의한 모델 선택 방법 분기 
  2024.02.09  이병윤 : E20230704-000395 1차 포장 구성(원/각형)인 경우 cell위치가 BOX_PSTN_NO으로 등록처리    



 
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

namespace LGC.GMES.MES.BOX001
{
    public partial class BOX001_240_CHANGE_CELL : C1Window, IWorkArea
    {
        #region Declaration & Constructor 

        private Util _Util = new Util();
        
        string _userID = string.Empty;
        string _eqsgID = string.Empty;
        string _callID = string.Empty;

        /// <summary>
        ///  Frame과 상호작용하기 위한 객체
        /// </summary>
        public IFrameOperation FrameOperation
        {
            get;
            set;
        }

        public BOX001_240_CHANGE_CELL()
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
            _eqsgID = tmps[0] as string;
            _userID = tmps[1] as string;
            _callID = tmps[2] as string;
            /* 
             * 1차 포장 구성(원/각형)[BOX001_202] 일때
             *  -. CELL위치 : BOX_PSTN_NO
             *  -. 라디오버튼 : Collapsed 처리_18650모델 단일
             * 자동 포장 구성(원/각형)[BOX001_240]S 일때
             *  -. CELL위치 : FORM_TRAY_PSTN_NO
             *  -. 라디오버튼 : Visible 처리_ M50L(21700), TESLA 중 선택 후 체크
             *  -. 
             */
            if (_callID.Equals("BOX001_202"))
            {
                dgSource.Columns["BOX_PSTN_NO"].Visibility = Visibility.Visible;
                dgSource.Columns["FORM_TRAY_PSTN_NO"].Visibility = Visibility.Collapsed;
                Collapsed2.Height = new GridLength(0, GridUnitType.Pixel);
                Collapsed1.Visibility = Visibility.Collapsed;
            }
            else
            {
                dgSource.Columns["FORM_TRAY_PSTN_NO"].Visibility = Visibility.Visible;
                dgSource.Columns["BOX_PSTN_NO"].Visibility = Visibility.Collapsed;
                Collapsed1.Visibility = Visibility.Visible;
                Collapsed2.Height = new GridLength(40, GridUnitType.Pixel);
                if(LoginInfo.CFG_AREA_ID.Equals("M8"))
                {
                    // N6_C:    자동 포장 구성 (원/각형 ) -> 21700 TESLA
                    rdoM50L.IsChecked = false;
                    rdoM50L.IsEnabled = false;
                    rdoTESLA.IsChecked = true;
                    rdoTESLA.IsEnabled = false;

                }
                else if(LoginInfo.CFG_AREA_ID.Equals("M5"))
                {
                    //N3: 자동 포장 구성(원 / 각형)->NON - IM M50L(21700)
                    rdoM50L.IsChecked = true;
                    rdoM50L.IsEnabled = false;
                    rdoTESLA.IsChecked = false;
                    rdoTESLA.IsEnabled = false;
                }
            }
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
                        newRow["BOXID"] = Util.NVC(dgSource.GetCell(0, dgSource.Columns["BOXID"].Index).Value);
                        newRow["USERID"] = _userID;

                        inDataTable.Rows.Add(newRow);

                        DataTable inSublotTable = indataSet.Tables.Add("INSUBLOT");
                        inSublotTable.Columns.Add("PACK_GB");
                        inSublotTable.Columns.Add("SUBLOTID");
                        inSublotTable.Columns.Add("FORM_TRAY_PSTN_NO");
                        inSublotTable.Columns.Add("BOX_PSTN_NO", typeof(int));

                        newRow = inSublotTable.NewRow();
                        newRow["PACK_GB"] = _callID;
                        newRow["SUBLOTID"] = Util.NVC(dgTarget.GetCell(0, dgTarget.Columns["SUBLOTID"].Index).Value);
                        newRow["FORM_TRAY_PSTN_NO"] = Util.NVC(dgSource.GetCell(0, dgSource.Columns["FORM_TRAY_PSTN_NO"].Index).Value);
                        newRow["BOX_PSTN_NO"] = Util.StringToInt(Util.NVC(dgSource.GetCell(0, dgSource.Columns["BOX_PSTN_NO"].Index).Value));

                        inSublotTable.Rows.Add(newRow);                       

                        DataTable inSublotDelTable = indataSet.Tables.Add("INSUBLOT_DELETE");
                        inSublotDelTable.Columns.Add("SUBLOTID");

                        newRow = inSublotDelTable.NewRow();
                        newRow["SUBLOTID"] = Util.NVC(dgSource.GetCell(0, dgSource.Columns["SUBLOTID"].Index).Value);

                        inSublotDelTable.Rows.Add(newRow);

                        loadingIndicator.Visibility = Visibility.Visible;

                        new ClientProxy().ExecuteService_Multi("BR_PRD_REG_SUBLOT_REPLACE_TESLA_NJ", "INDATA,INSUBLOT,INSUBLOT_DELETE", null, (bizResult, bizException) =>
                        {
                            try
                            {
                                loadingIndicator.Visibility = Visibility.Collapsed;
                                if (bizException != null)
                                {
                                    Util.MessageException(bizException);
                                    return;
                                }

                                //정상 처리 되었습니다.
                                Util.MessageInfo("SFU1275");

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
                if (e.Key == Key.Enter)
                {
                    string sCellID = txtTargetID.Text.Trim();

                    //if (dgTarget.GetRowCount() > 0)
                    //{
                    //    DataTable dtInfo = DataTableConverter.Convert(dgTarget.ItemsSource);
                    //    DataRow[] drList = dtInfo.Select("CELLID = '" + sCellID + "'");

                    //    if (drList.Length > 0)
                    //    {
                    //        // SFU3159 아래쪽 List에 이미 존재하는 CELL ID입니다.
                    //        ControlsLibrary.MessageBox.ShowNoEnter(MessageDic.Instance.GetMessage("SFU3099"), null, "Error", MessageBoxButton.OK, MessageBoxIcon.Warning, (result) =>
                    //        {
                    //            if (result == MessageBoxResult.OK)
                    //            {
                    //                txtTargetID.Focus();
                    //                txtTargetID.Text = string.Empty;
                    //            }
                    //        });

                    //        txtTargetID.Text = string.Empty;
                    //        //  txtCellID.Focus();
                    //        return;
                    //    }
                    //}
                    if (dgSource.GetRowCount() == 0)
                        return;

                    string pack = string.Empty;
                    string model = string.Empty;
                    if (_callID.Equals("BOX001_202"))
                    {
                        pack = "1ST";
                        model = "18650";
                    }
                    else
                    {
                        pack = "AUTO";
                        model = ((bool)rdoM50L.IsChecked ? "M50L" : "TESLA");
                    }

                    DataTable RQSTDT = new DataTable("INDATA");
                    RQSTDT.Columns.Add("SUBLOTID");
                    RQSTDT.Columns.Add("USERID");
                    RQSTDT.Columns.Add("SHOPID");
                    RQSTDT.Columns.Add("AREAID");
                    RQSTDT.Columns.Add("EQSGID");
                    RQSTDT.Columns.Add("ASSY_LOTID");
                    RQSTDT.Columns.Add("PRODID");
                    RQSTDT.Columns.Add("PRDT_GRD_CODE");
                    RQSTDT.Columns.Add("PACK_GB");
                    RQSTDT.Columns.Add("MODEL");

                    DataRow dr = RQSTDT.NewRow();
                    dr["SUBLOTID"] = sCellID;
                    dr["USERID"] = _userID;
                    dr["SHOPID"] = LoginInfo.CFG_SHOP_ID;
                    dr["AREAID"] = LoginInfo.CFG_AREA_ID;
                    dr["EQSGID"] = _eqsgID;
                    dr["ASSY_LOTID"] = Util.NVC(dgSource.GetCell(0, dgSource.Columns["ASSY_LOTID"].Index).Value).Substring(0,8);
                    dr["PRODID"] = Util.NVC(dgSource.GetCell(0, dgSource.Columns["PRODID"].Index).Value);
                    dr["PRDT_GRD_CODE"] = Util.NVC(dgSource.GetCell(0, dgSource.Columns["PRDT_GRD_CODE"].Index).Value);
                    dr["PACK_GB"] = pack;
                    dr["MODEL"] = model;

                    RQSTDT.Rows.Add(dr);

                    DataTable dtRslt = new ClientProxy().ExecuteServiceSync("BR_PRD_CHK_SUBLOT_TESLA_NJ", "INDATA", "OUTDATA", RQSTDT);

                    //if (dtRslt != null)
                    //{
                    //    DataTable dtInfo = DataTableConverter.Convert(dgTarget.ItemsSource);
                    //    DataRow drInfo = dtInfo.NewRow();
                    //    drInfo["SUBLOTID"] = dtRslt.Rows[0]["SUBLOTID"];
                    //    dtInfo.Rows.Add(drInfo);
                    //    Util.GridSetData(dgTarget, dtInfo, FrameOperation, true);
                    //}
                    
                    Util.GridSetData(dgTarget, dtRslt, FrameOperation, true);

                    txtTargetID.Text = string.Empty;
                    txtTargetID.Focus();
                    dgTarget.ScrollIntoView(dgTarget.Rows.Count-1, 0);

                    if (dgTarget.Rows.Count > 0)
                    {
                        DataGridAggregate.SetAggregateFunctions(dgTarget.Columns["SUBLOTID"], new DataGridAggregatesCollection { new DataGridAggregateCount() });
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

        private void txtSourceID_KeyDown(object sender, KeyEventArgs e)
        {
            try
            {
                if (e.Key == Key.Enter)
                {
                    string sCellID = txtSourceID.Text.Trim();

                    //if (dgSource.GetRowCount() > 0)
                    //{
                    //    DataTable dtInfo = DataTableConverter.Convert(dgSource.ItemsSource);
                    //    DataRow[] drList = dtInfo.Select("CELLID = '" + sCellID + "'");

                    //    if (drList.Length > 0)
                    //    {
                    //        // SFU3159 아래쪽 List에 이미 존재하는 CELL ID입니다.
                    //        ControlsLibrary.MessageBox.ShowNoEnter(MessageDic.Instance.GetMessage("SFU3099"), null, "Error", MessageBoxButton.OK, MessageBoxIcon.Warning, (result) =>
                    //        {
                    //            if (result == MessageBoxResult.OK)
                    //            {
                    //                txtSourceID.Focus();
                    //                txtSourceID.Text = string.Empty;
                    //            }
                    //        });

                    //        txtSourceID.Text = string.Empty;
                    //        //  txtCellID.Focus();
                    //        return;
                    //    }
                    //}
                    string pack = string.Empty;
                    if(_callID.Equals("BOX001_202"))
                    {
                        pack = "1ST";
                    }
                    else
                    {
                        pack = "AUTO";
                    }
                    DataTable RQSTDT = new DataTable("INDATA");
                    RQSTDT.Columns.Add("SUBLOTID");
                    RQSTDT.Columns.Add("PACK_GB");

                    DataRow dr = RQSTDT.NewRow();
                    dr["SUBLOTID"] = sCellID;
                    dr["PACK_GB"] = pack;
                    RQSTDT.Rows.Add(dr);

                    DataTable dtRslt = new ClientProxy().ExecuteServiceSync("BR_PRD_GET_SUBLOT_FOR_REPLACE_NJ", "INDATA", "OUTDATA", RQSTDT);

                    //if (dtRslt != null)
                    //{
                    //    if (dgSource.GetRowCount() > 0)
                    //    {
                    //        DataTable dt = DataTableConverter.Convert(dgSource.ItemsSource); ;
                    //        DataRow[] drList = dt.Select("CELLID = '" + dtRslt.Rows[0]["SUBLOTID"] + "'");

                    //        if (drList.Length > 0)
                    //        {
                    //            // SFU3159 아래쪽 List에 이미 존재하는 CELL ID입니다.
                    //            ControlsLibrary.MessageBox.ShowNoEnter(MessageDic.Instance.GetMessage("SFU3099"), null, "Error", MessageBoxButton.OK, MessageBoxIcon.Warning, (result) =>
                    //            {
                    //                if (result == MessageBoxResult.OK)
                    //                {
                    //                    txtSourceID.Focus();
                    //                    txtSourceID.Text = string.Empty;
                    //                }
                    //            });

                    //            txtSourceID.Text = string.Empty;
                    //            return;
                    //        }
                    //    }
                    //dgSource.
                    Util.GridSetData(dgSource, dtRslt, FrameOperation, true);

                    txtSourceID.Focus();
                    txtSourceID.Text = string.Empty;
                    dgSource.ScrollIntoView(dgSource.Rows.Count - 1, 0);
                    if (dgSource.Rows.Count > 0)
                    {
                        DataGridAggregate.SetAggregateFunctions(dgSource.Columns["SUBLOTID"], new DataGridAggregatesCollection { new DataGridAggregateCount() });
                    }
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
