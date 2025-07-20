/*************************************************************************************
 Created Date : 2017.10.10
      Creator : 이진선
   Decription : 남경 폴리머 VD예약
--------------------------------------------------------------------------------------
 [Change History]
  2017.10.10  이진선S : Initial Created.
  2022.04.19  장희만  : C20220410-000011- V/D(STP후) 공정 V/D 공정예약 할 수 있도록 공정 추가
                                          재공이동 기능 추가
  2022.07.12  김태균  : 2산단 NFF 재공이동 추가 - M9
**************************************************************************************/


using C1.WPF.DataGrid;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.CMM001.UserControls;
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

namespace LGC.GMES.MES.ASSY003
{
    /// <summary>
    /// ASSY003_034.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class ASSY003_034 : UserControl, IWorkArea
    {
        Util _Util = new Util();
        private UC_WORKORDER_LINE winWo = new UC_WORKORDER_LINE();
        CommonCombo combo = new CommonCombo();

        private string _Process;
        private string _EquipElec;
        private string _EquipFlag;
        private string _Unit;
        private string _EQPT_ONLINE_FLAG;
        private string _VD_WRK_COND_GR_CODE = string.Empty;
        private string _PRODID = string.Empty;
        private int _MaxNum = -1;
        DataTable dtAllLot;
        DataTable dtMain = new DataTable();

        public IFrameOperation FrameOperation
        {
            get;
            set;
        }

        public ASSY003_034()
        {
            InitializeComponent();
        }

        private void UserControl_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {
            Loaded -= UserControl_Loaded;
            ApplyPermissions();
            SetWorkOrderWindow();
            btnMoveWip.Visibility = Visibility.Collapsed;
            initcombo();
        }



        private void cboVDEquipmentSegment_SelectedValueChanged(object sender, C1.WPF.PropertyChangedEventArgs<object> e)
        {
            //공정
            String[] sFilter = { cboVDEquipmentSegment.SelectedValue.ToString() };
            combo.SetCombo(cboVDProcess, CommonCombo.ComboStatus.NONE, sFilter: sFilter);
            _Process = Convert.ToString(cboVDProcess.SelectedValue);

            //남경 NJ STP 라인
            //if (cboVDEquipmentSegment.SelectedValue.ToString().Equals(LoginInfo.CFG_AREA_ID + "MV2"))
            //    btnMoveWip.Visibility = Visibility.Visible;
            //else
            //    btnMoveWip.Visibility = Visibility.Collapsed;

            //남경 NJ STP 라인
            if (cboVDEquipmentSegment.SelectedValue.ToString().Equals(LoginInfo.CFG_AREA_ID + "MV2"))
            {
                btnMoveWip.Visibility = Visibility.Visible;
            }
            else if (LoginInfo.CFG_AREA_ID == "M9")
            {
                //오창 제2산단 NFF 라인
                btnMoveWip.Visibility = Visibility.Visible;
            }
            else
            {
                btnMoveWip.Visibility = Visibility.Collapsed;
            }
        }

        private void cboVDProcess_SelectedValueChanged(object sender, C1.WPF.PropertyChangedEventArgs<object> e)
        {
            try
            {
                //설비
                String[] sFilter = { cboVDProcess.SelectedValue.ToString(), cboVDEquipmentSegment.SelectedValue.ToString() };
                combo.SetCombo(cboVDEquipment, CommonCombo.ComboStatus.SELECT, sFilter: sFilter);

                InitializeControls();

                Util.gridClear(dgCanReserve);
                Util.gridClear(dgLotList);
                Util.gridClear(dgReservedLot);

                //if (Util.NVC(cboVDProcess.SelectedValue).Equals("A6000"))
                //    chkUseGroup.Visibility = Visibility.Collapsed;
                //else
                //    chkUseGroup.Visibility = Visibility.Visible;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        private void cboVDEquipment_SelectedValueChanged(object sender, C1.WPF.PropertyChangedEventArgs<object> e)
        {
            try
            {
                Util.gridClear(dgCanReserve);
                Util.gridClear(dgReservedLot);
                Util.gridClear(dgLotList);

                if (winWo != null)
                {
                    winWo.EQPTSEGMENT = cboVDEquipmentSegment.SelectedValue != null ? Convert.ToString(cboVDEquipmentSegment.SelectedValue) : "";
                    winWo.EQPTID = cboVDEquipment.SelectedValue != null ? Convert.ToString(cboVDEquipment.SelectedValue) : "";
                    winWo.PROCID = cboVDProcess.SelectedValue != null ? Convert.ToString(cboVDProcess.SelectedValue) : "";

                    winWo.ClearWorkOrderInfo();
                }

                if (cboVDEquipment.Text.Equals("-SELECT-"))
                {
                    return;
                }

                DataTable dt = new DataTable();
                dt.Columns.Add("EQPTID", typeof(string));

                DataRow row = dt.NewRow();
                row["EQPTID"] = Convert.ToString(cboVDEquipment.SelectedValue);
                dt.Rows.Add(row);

                DataTable result = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_EQUIPMENT_VD_INFO", "INDATA", "RSLTDT", dt);
                if (result == null)
                {
                    Util.MessageValidation("SFU1672");  //설비 정보가 없습니다.
                    return;
                }
                if (result.Rows.Count == 0)
                {
                    Util.MessageValidation("SFU1672");  //설비 정보가 없습니다.
                    return;
                }

                _EquipFlag = Convert.ToString(result.Rows[0]["PRDT_CLSS_CHK_FLAG"]);
                _EquipElec = Convert.ToString(result.Rows[0]["PRDT_CLSS_CODE"]).Equals("") ? null : Convert.ToString(result.Rows[0]["PRDT_CLSS_CODE"]);

                if (_EquipFlag.Equals("Y") && (_EquipElec == null))
                {
                    Util.MessageValidation("SFU1399"); //MMD에서 VD설비 극성을 등록해주세요
                    return;
                }

                //if (result.Rows[0]["WRK_RSV_UNIT_CODE"].ToString().Equals(""))
                //{
                //    Util.MessageValidation("SFU1400");  //MMD에서 VD설비 작업예약단위를 입력해주세요

                //    dgCanReserve.ItemsSource = null;
                //    dgReservedLot.ItemsSource = null;

                //    btnSearch.IsEnabled = false;

                //    return;
                //}

                //_Unit = Convert.ToString(result.Rows[0]["WRK_RSV_UNIT_CODE"]);

                //if (_Unit.Equals("LOT"))//파우치
                //{
                //    SKIDID_CANRESERVE.Visibility = Visibility.Collapsed;
                //    SKIDID_LOTLIST.Visibility = Visibility.Collapsed;
                //    LOTID_LOTLIST.Visibility = Visibility.Visible;
                //    SKIDID_RESERVED.Visibility = Visibility.Collapsed;

                //    tbSKIDID.Visibility = Visibility.Collapsed;
                //    tbLotID.Visibility = Visibility.Visible;
                //}
                //else //원각형
                //{
                //    SKIDID_CANRESERVE.Visibility = Visibility.Visible;
                //    SKIDID_LOTLIST.Visibility = Visibility.Visible;
                //    LOTID_LOTLIST.Visibility = Visibility.Collapsed;
                //    SKIDID_RESERVED.Visibility = Visibility.Visible;

                //    tbSKIDID.Visibility = Visibility.Visible;
                //    tbLotID.Visibility = Visibility.Collapsed;
                //}


                _EQPT_ONLINE_FLAG = "Y";

                if (!result.Rows[0]["EQPT_ONLINE_FLAG"].ToString().Equals(""))//없으면 기본 온라인
                {
                    _EQPT_ONLINE_FLAG = result.Rows[0]["EQPT_ONLINE_FLAG"].ToString();

                }

                if (_EQPT_ONLINE_FLAG.Equals("N")) //오프라인일때 착완공 동시 진행
                {
                    btnReserve.Visibility = Visibility.Collapsed;
                    btnReserveStart.Visibility = Visibility.Visible;

                    btnReserveCancel.Visibility = Visibility.Collapsed;
                    btnRunCancel.Visibility = Visibility.Visible;


                    if (dgReservedLot.Columns.Contains("EQPT_BTCH_WRK_NO"))
                        dgReservedLot.Columns["EQPT_BTCH_WRK_NO"].Visibility = Visibility.Visible;
                }
                else
                {
                    btnReserve.Visibility = Visibility.Visible;
                    btnReserveStart.Visibility = Visibility.Collapsed;

                    btnReserveCancel.Visibility = Visibility.Visible;
                    btnRunCancel.Visibility = Visibility.Collapsed;

                    if (dgReservedLot.Columns.Contains("EQPT_BTCH_WRK_NO"))
                        dgReservedLot.Columns["EQPT_BTCH_WRK_NO"].Visibility = Visibility.Collapsed;
                }


                if (Convert.ToString(result.Rows[0]["WRK_RSV_MAX_QTY"]).Equals(""))
                {
                    Util.MessageValidation("SFU1396");   //MMD에 VD설비 작업예약최대개수를 입력해주세요
                }
                _MaxNum = Convert.ToString(result.Rows[0]["WRK_RSV_MAX_QTY"]).Equals("") ? 20 : int.Parse(Convert.ToString(result.Rows[0]["WRK_RSV_MAX_QTY"]));

                Util.gridClear(dgCanReserve);
                Util.gridClear(dgReservedLot);
                Util.gridClear(dgLotList);
                //dgCanReserve.ItemsSource = null;
                //dgReservedLot.ItemsSource = null;
                //dgLotList.ItemsSource = null;

                btnSearch.IsEnabled = true;



                ClearControls();


                // 설비 선택 시 자동 조회 처리
                if (cboVDEquipment.SelectedIndex > 0 && cboVDEquipment.Items.Count > cboVDEquipment.SelectedIndex)
                {
                    if (Util.NVC((cboVDEquipment.Items[cboVDEquipment.SelectedIndex] as DataRowView).Row["CBO_NAME"]).IndexOf("SELECT") < 0)
                    {
                        loadingIndicator.Visibility = Visibility.Visible;

                        this.Dispatcher.BeginInvoke(new Action(() => btnSearch_Click(null, null)));
                    }
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        private void chkWoProduct_Click(object sender, RoutedEventArgs e)
        {
            if (sender == null) return;
            SetWorkorderProduct();
        }

        private void txtLOTID_KeyUp(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                SetInfo();
                //for (int i = 0; i < dgCanReserve.Rows.Count; i++)
                //{
                //    if (Util.NVC(DataTableConverter.GetValue(dgCanReserve.Rows[i].DataItem, _Unit.Equals("LOT") ?  "LOTID" : "CSTID")).Equals(txtLOTID.Text))
                //    {
                //        DataTableConverter.SetValue(dgCanReserve.Rows[i].DataItem, "CHK", true);

                //        if (!ValidationLot(i))
                //        {
                //            txtLOTID.Text = "";
                //            return;
                //        }

                //        if (!_Unit.Equals("LOT"))
                //        {
                //            SetChkCutID(i, dgCanReserve);
                //        }

                //        dgCanReserve.ScrollIntoView(i, 0);

                //        AddRow(dgLotList, dgCanReserve, i);

                //        txtLOTID.Text = "";
                //        return;
                //    }
                //}
                //Util.MessageValidation("SFU1734"); //예약가능 LOT 리스트에 없습니다.
                //txtLOTID.Text = "";

            }
        }

        private void btnSearch_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            dgCanReserve.ItemsSource = null;
            dgReservedLot.ItemsSource = null;

            SearchData();
        }
        private void CheckBox_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            try
            {
                if (sender == null) return;

                int index = ((C1.WPF.DataGrid.DataGridCellPresenter)((sender as CheckBox).Parent)).Row.Index;
                if (!ValidationLot(index)) return;

                if (!_Unit.Equals("LOT"))
                {
                    SetChkCutID(index, dgCanReserve);
                }

                DataTable dgWorkOrder = DataTableConverter.Convert(((grdWorkOrder.Children[0] as UC_WORKORDER_LINE).dgWorkOrder).ItemsSource);
                DataRow[] dr = dgWorkOrder.Select("EIO_WO_SEL_STAT = 'Y'");

                if (dr.Length > 0)
                {
                    string woPRODID = dr[0]["PRODID"].ToString();

                    if (!Util.NVC(DataTableConverter.GetValue(dgCanReserve.Rows[index].DataItem, "PRODID")).Equals(woPRODID))
                    {
                        DataTableConverter.SetValue(dgCanReserve.Rows[index].DataItem, "CHK", 0);
                        Util.Alert("SFU4178"); // 동일한 제품이 아닙니다.
                    }
                }




                if (Util.NVC(DataTableConverter.GetValue(dgCanReserve.Rows[index].DataItem, "CHK")).Equals("1") || Util.NVC(DataTableConverter.GetValue(dgCanReserve.Rows[index].DataItem, "CHK")).Equals("True"))
                {
                    AddRow(dgLotList, dgCanReserve, index);

                }
                else
                {
                    dgLotList.IsReadOnly = false;

                    //remove
                    for (int i = 0; i < dgLotList.Rows.Count; i++)
                    {
                        if (Util.NVC(DataTableConverter.GetValue(dgLotList.Rows[i].DataItem, _Unit.Equals("LOT") ? "LOTID" : "CSTID")).Equals(Util.NVC(DataTableConverter.GetValue(dgCanReserve.Rows[index].DataItem, _Unit.Equals("LOT") ? "LOTID" : "CSTID"))))
                        {
                            dgLotList.RemoveRow(i);

                        }
                    }


                    for (int i = 0; i < dgLotList.Rows.Count; i++)
                    {
                        DataTableConverter.SetValue(dgLotList.Rows[i].DataItem, "RSV_SEQ", i + 1);
                    }

                    dgLotList.IsReadOnly = true;

                    if (!_Unit.Equals("LOT"))
                    {
                        DataTable dt = DataTableConverter.Convert(dgCanReserve.ItemsSource).Select("CSTID = '" + Util.NVC(DataTableConverter.GetValue(dgCanReserve.Rows[index].DataItem, "CSTID")) + "'").Count() == 0 ? null : DataTableConverter.Convert(dgCanReserve.ItemsSource).Select("CSTID = '" + Util.NVC(DataTableConverter.GetValue(dgCanReserve.Rows[index].DataItem, "CSTID")) + "'").CopyToDataTable();
                        if (dt == null) return;
                        if (dt.Rows.Count == 0) return;
                    }

                }

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }



        private void btnReserve_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidationReserve())
            {
                return;
            }

            try
            {

                DataSet indataSet = new DataSet();
                DataTable inData = indataSet.Tables.Add("IN_EQP");
                inData.Columns.Add("SRCTYPE", typeof(string));
                inData.Columns.Add("IFMODE", typeof(string));
                inData.Columns.Add("EQPTID", typeof(string));
                inData.Columns.Add("USERID", typeof(string));
                inData.Columns.Add("EQSGID", typeof(string));
                inData.Columns.Add("PROCID", typeof(string));
                inData.Columns.Add("EQPT_ONLINE_FLAG", typeof(string));

                DataRow row = inData.NewRow();
                row["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                row["IFMODE"] = IFMODE.IFMODE_OFF;
                row["EQPTID"] = Convert.ToString(cboVDEquipment.SelectedValue);
                row["USERID"] = LoginInfo.USERID;
                row["EQSGID"] = Convert.ToString(cboVDEquipmentSegment.SelectedValue);
                row["PROCID"] = Convert.ToString(cboVDProcess.SelectedValue);
                row["EQPT_ONLINE_FLAG"] = _EQPT_ONLINE_FLAG;
                indataSet.Tables["IN_EQP"].Rows.Add(row);


                DataTable inLot = indataSet.Tables.Add("IN_LOT");
                inLot.Columns.Add("LOTID", typeof(string));
                inLot.Columns.Add("RSV_SEQ", typeof(string));

                for (int i = 0; i < dgLotList.GetRowCount(); i++)
                {
                    row = inLot.NewRow();
                    row["LOTID"] = Util.NVC(DataTableConverter.GetValue(dgLotList.Rows[i].DataItem, _Unit.Equals("LOT") ? "LOTID" : "CSTID"));
                    row["RSV_SEQ"] = Util.NVC(DataTableConverter.GetValue(dgLotList.Rows[i].DataItem, "RSV_SEQ"));
                    indataSet.Tables["IN_LOT"].Rows.Add(row);

                }

                loadingIndicator.Visibility = Visibility.Visible;

                new ClientProxy().ExecuteService_Multi("BR_PRD_REG_RESERVE_LOT_VD_NJ_BR", "IN_EQP,IN_LOT", null, (bizResult, bizException) =>
                {
                    try
                    {
                        if (bizException != null)
                        {
                            Util.MessageException(bizException);
                            return;
                        }

                        Util.MessageInfo("SFU1731");

                        dgLotList.ItemsSource = null;
                        initGridTable(dgLotList);

                        SearchData();

                    }
                    catch (Exception ex)
                    {
                        Util.MessageException(ex);
                    }
                    finally
                    {
                        loadingIndicator.Visibility = Visibility.Collapsed;
                    }
                }, indataSet);

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                // Util.MessageException(ex);   //예약 오류
            }
        }
        private void btnReserveCancel_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            try
            {

                if (_Util.GetDataGridCheckFirstRowIndex(dgReservedLot, "CHK") == -1)
                {
                    Util.MessageValidation("SFU1632"); //선택된 LOT이 없습니다.
                    return;
                }

                DataTable dt = DataTableConverter.Convert(dgReservedLot.ItemsSource);
                if (dt.Rows.Count == 0) return;

                if (dt.Select("CHK = 0").Count() != 0)
                {
                    Util.MessageValidation("SFU3202");//모든 LOT을 선택하세요
                    return;
                }


                if (Convert.ToString(cboVDEquipment.SelectedValue).Equals(""))
                {
                    Util.MessageValidation("SFU1732");
                    //Util.Alert("SFU1732");  //예약 취소 할 설비를 선택해주세요.
                    return;
                }


                DataSet indataSet = new DataSet();
                DataTable inData = indataSet.Tables.Add("IN_EQP");
                inData.Columns.Add("SRCTYPE", typeof(string));
                inData.Columns.Add("IFMODE", typeof(string));
                inData.Columns.Add("EQPTID", typeof(string));
                inData.Columns.Add("USERID", typeof(string));
                inData.Columns.Add("PROCID", typeof(string));
                inData.Columns.Add("EQSGID", typeof(string));
                inData.Columns.Add("TERM_FLAG", typeof(string));

                DataRow row = inData.NewRow();
                row["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                row["IFMODE"] = "OFF";
                row["EQPTID"] = Util.NVC(DataTableConverter.GetValue(dgReservedLot.Rows[_Util.GetDataGridCheckFirstRowIndex(dgReservedLot, "CHK")].DataItem, "EQPTID"));
                row["USERID"] = LoginInfo.USERID;
                row["PROCID"] = Convert.ToString(cboVDProcess.SelectedValue);
                row["EQSGID"] = Convert.ToString(cboVDEquipmentSegment.SelectedValue);
                row["TERM_FLAG"] = ((bool)chkWaterSpecOut.IsChecked && !_Unit.Equals("LOT")) ? "Y" : "N";
                indataSet.Tables["IN_EQP"].Rows.Add(row);

                DataTable inLot = indataSet.Tables.Add("IN_LOT");
                inLot.Columns.Add("LOTID", typeof(string));


                for (int i = 0; i < dgReservedLot.GetRowCount(); i++)
                {
                    if (Util.NVC(DataTableConverter.GetValue(dgReservedLot.Rows[i].DataItem, "CHK")).Equals("1") || Util.NVC(DataTableConverter.GetValue(dgReservedLot.Rows[i].DataItem, "CHK")).Equals("True"))
                    {
                        row = inLot.NewRow();
                        row["LOTID"] = Util.NVC(DataTableConverter.GetValue(dgReservedLot.Rows[i].DataItem, "LOTID"));
                        indataSet.Tables["IN_LOT"].Rows.Add(row);
                    }
                }



                try
                {
                    loadingIndicator.Visibility = Visibility.Visible;

                    new ClientProxy().ExecuteService_Multi("BR_PRD_REG_CANCEL_RESERVE_LOT", "IN_EQP,IN_LOT", null, (bizResult, bizException) =>
                    {
                        try
                        {
                            if (bizException != null)
                            {
                                Util.MessageException(bizException);
                                return;
                            }

                            Util.MessageInfo("SFU1736");
                            SearchData();

                        }
                        catch (Exception ex)
                        {
                            Util.MessageException(ex);
                        }
                        finally
                        {
                            loadingIndicator.Visibility = Visibility.Collapsed;
                        }
                    }, indataSet);

                }
                catch (Exception ex)
                {
                    Util.MessageException(ex);
                }


            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
            finally
            {
                chkAll.IsChecked = false;
            }
        }
        private void btnRunCancel_Click(object sender, RoutedEventArgs e)
        {
            if (_Util.GetDataGridCheckFirstRowIndex(dgReservedLot, "CHK") == -1)
            {
                //Util.Alert("선택된 LOT이 없습니다.");
                Util.MessageValidation("SFU1661");
                return;
            }
            if (!Util.NVC(DataTableConverter.GetValue(dgReservedLot.Rows[_Util.GetDataGridCheckFirstRowIndex(dgReservedLot, "CHK")].DataItem, "WIPSTAT")).Equals("PROC"))
            {
                //Util.Alert("작업시작 후 시작 취소 할 수 있습니다.");
                Util.MessageValidation("SFU3035");
                return;
            }

            string sTmp = Util.NVC(DataTableConverter.GetValue(dgReservedLot.Rows[_Util.GetDataGridCheckFirstRowIndex(dgReservedLot, "CHK")].DataItem, "EQPT_BTCH_WRK_NO"));

            for (int i = 0; i < dgReservedLot.GetRowCount(); i++)
            {
                if (Util.NVC(DataTableConverter.GetValue(dgReservedLot.Rows[i].DataItem, "CHK")).Equals("1") || Util.NVC(DataTableConverter.GetValue(dgReservedLot.Rows[i].DataItem, "CHK")).Equals("True"))
                {
                    if (Util.NVC(DataTableConverter.GetValue(dgReservedLot.Rows[i].DataItem, "WIPSTAT")).Equals("RESERVE") || Util.NVC(DataTableConverter.GetValue(dgReservedLot.Rows[i].DataItem, "WIPSTAT")).Equals("READY"))
                    {
                        //Util.Alert("작업시작 후 시작 취소 할 수 있습니다.");
                        Util.MessageValidation("SFU3035");
                        return;
                    }

                    if (!Util.NVC(DataTableConverter.GetValue(dgReservedLot.Rows[i].DataItem, "EQPT_BTCH_WRK_NO")).Equals(sTmp))
                    {
                        Util.MessageValidation("SFU4288", Util.NVC(DataTableConverter.GetValue(dgReservedLot.Rows[i].DataItem, "LOTID")));  // 선택한 Lot[%]은 동일한 작업배치번호가 아닙니다. 같은 작업배치번호 단위로 시작취소할 수 있습니다.
                        return;
                    }
                }
                else
                {
                    if (Util.NVC(DataTableConverter.GetValue(dgReservedLot.Rows[i].DataItem, "EQPT_BTCH_WRK_NO")).Equals(sTmp))
                    {
                        Util.MessageValidation("SFU4289");  // 동일한 작업배치번호를 모두 선택 후 시작취소하세요.
                        return;
                    }
                }
            }


            DataTable dt = new DataTable();
            dt.Columns.Add("SRCTYPE", typeof(string));
            dt.Columns.Add("EQPTID", typeof(string));
            dt.Columns.Add("LOTID", typeof(string));
            dt.Columns.Add("USERID", typeof(string));
            dt.Columns.Add("IFMODE", typeof(string));



            DataRow row = dt.NewRow();
            DataTable dtProductLot = DataTableConverter.Convert(dgReservedLot.ItemsSource);

            foreach (DataRow _iRow in dtProductLot.Rows)
            {

                if (_iRow["CHK"].ToString().Equals("True") || _iRow["CHK"].ToString().Equals("1"))
                {
                    row = dt.NewRow();
                    row["LOTID"] = _iRow["LOTID"];
                    row["SRCTYPE"] = "UI";
                    row["EQPTID"] = cboVDEquipment.SelectedValue.ToString();
                    row["USERID"] = LoginInfo.USERID;
                    row["IFMODE"] = "OFF";
                    dt.Rows.Add(row);
                }

            }

            try
            {
                new ClientProxy().ExecuteServiceSync("BR_PRD_REG_CANCEL_START_LOT_VD_NJ", "RQSTDT", null, dt);
                Util.MessageInfo("SFU1839");
                SearchData();
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
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

        private void dgReservedLot_LoadedColumnHeaderPresenter(object sender, C1.WPF.DataGrid.DataGridColumnEventArgs e)
        {
            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (string.IsNullOrEmpty(e.Column.Name) == false)
                {
                    if (e.Column.Name.Equals("CHK"))
                    {
                        pre.Content = chkAll;
                        e.Column.HeaderPresenter.Content = pre;
                        chkAll.Checked -= new RoutedEventHandler(checkAll_Checked);
                        chkAll.Unchecked -= new RoutedEventHandler(checkAll_Unchecked);
                        chkAll.Checked += new RoutedEventHandler(checkAll_Checked);
                        chkAll.Unchecked += new RoutedEventHandler(checkAll_Unchecked);
                    }
                }
            }));
        }

        #region[Method]
        private void SetChkCutID(int index, C1.WPF.DataGrid.C1DataGrid datagrid)
        {
            if (Util.NVC(DataTableConverter.GetValue(datagrid.Rows[index].DataItem, "CHK")).Equals("1") || Util.NVC(DataTableConverter.GetValue(datagrid.Rows[index].DataItem, "CHK")).Equals("True"))
            {
                //SKID ID 같으면 동시에 선택이 되게
                for (int i = 0; i < datagrid.Rows.Count; i++)
                {
                    if (index != i)
                    {
                        if (Util.NVC(DataTableConverter.GetValue(datagrid.Rows[index].DataItem, "CSTID")).Equals(Util.NVC(DataTableConverter.GetValue(datagrid.Rows[i].DataItem, "CSTID"))) &&
                             (Util.NVC(DataTableConverter.GetValue(datagrid.Rows[index].DataItem, "CHK")).Equals("1") || Util.NVC(DataTableConverter.GetValue(datagrid.Rows[index].DataItem, "CHK")).Equals("True")))
                        {
                            DataTableConverter.SetValue(datagrid.Rows[i].DataItem, "CHK", 1);
                        }
                    }
                }
            }
            else if (Util.NVC(DataTableConverter.GetValue(datagrid.Rows[index].DataItem, "CHK")).Equals("0") || Util.NVC(DataTableConverter.GetValue(datagrid.Rows[index].DataItem, "CHK")).Equals("False"))
            {
                for (int i = 0; i < datagrid.Rows.Count; i++)
                {
                    if (index != i)
                    {
                        if (Util.NVC(DataTableConverter.GetValue(datagrid.Rows[index].DataItem, "CSTID")).Equals(Util.NVC(DataTableConverter.GetValue(datagrid.Rows[i].DataItem, "CSTID"))) &&
                             (Util.NVC(DataTableConverter.GetValue(datagrid.Rows[index].DataItem, "CHK")).Equals("0") || Util.NVC(DataTableConverter.GetValue(datagrid.Rows[index].DataItem, "CHK")).Equals("False")))
                        {
                            DataTableConverter.SetValue(datagrid.Rows[i].DataItem, "CHK", 0);
                        }
                    }
                }
            }
        }

        void checkAll_Checked(object sender, RoutedEventArgs e)
        {

            if ((bool)chkAll.IsChecked)
            {

                for (int i = 0; i < dgReservedLot.GetRowCount(); i++)
                {
                    DataTableConverter.SetValue(dgReservedLot.Rows[i].DataItem, "CHK", true);
                }
            }


        }


        void checkAll_Unchecked(object sender, RoutedEventArgs e)
        {
            if (!(bool)chkAll.IsChecked)
            {
                for (int i = 0; i < dgReservedLot.GetRowCount(); i++)
                {
                    DataTableConverter.SetValue(dgReservedLot.Rows[i].DataItem, "CHK", false);
                }
            }
        }


        private void initcombo()
        {
            // V/D(STP후) 공정 추가 : Process.VD_LMN_AFTER_STP
            string[] sFilter = { "A1000," + Process.VD_LMN + "," + Process.VD_ELEC + "," + Process.VD_LMN_AFTER_STP, LoginInfo.CFG_AREA_ID };
            combo.SetCombo(cboVDEquipmentSegment, CommonCombo.ComboStatus.NONE, sFilter: sFilter);
        }

        private void ApplyPermissions()
        {
            List<Button> listAuth = new List<Button>();
            listAuth.Add(btnReserve);
            listAuth.Add(btnReserveCancel);

            Util.pageAuth(listAuth, FrameOperation.AUTHORITY);
        }

        private void SetWorkOrderWindow()
        {

            if (grdWorkOrder.Children.Count == 0)
            {
                winWo.FrameOperation = FrameOperation;
                winWo._UCParent = this;
                winWo.PROCID = Convert.ToString(cboVDProcess.SelectedValue);
                grdWorkOrder.Children.Add(winWo);

            }

        }
        public bool GetSearchConditions(out string procid, out string eqsgid, out string eqptid)
        {
            try
            {
                procid = Convert.ToString(cboVDProcess.SelectedValue);//Process.VD_LMN;
                eqsgid = Convert.ToString(cboVDEquipmentSegment.SelectedValue);
                eqptid = Convert.ToString(cboVDEquipment.SelectedValue);

                return true;
            }
            catch (Exception ex)
            {
                procid = "";
                eqsgid = "";
                eqptid = "";

                return false;
                throw ex;

            }

        }

        private void AddRow(C1.WPF.DataGrid.C1DataGrid toDatagrid, C1.WPF.DataGrid.C1DataGrid fromDatagrid, int i)
        {

            if (_Unit.Equals("LOT"))
            {

                if ((toDatagrid.ItemsSource as DataView).ToTable().Select("LOTID = '" + Util.NVC(DataTableConverter.GetValue(fromDatagrid.Rows[i].DataItem, "LOTID")) + "'").Count() != 0)
                {
                    Util.MessageValidation("SFU1384"); //LOT이 이미 있습니다.
                    return;
                }
            }
            else
            {
                if ((toDatagrid.ItemsSource as DataView).ToTable().Select("CSTID = '" + Util.NVC(DataTableConverter.GetValue(fromDatagrid.Rows[i].DataItem, "CSTID")) + "'").Count() != 0)
                {
                    Util.MessageValidation("SFU3068");//SKIDID가 이미 선택되었습니다.
                    return;
                }
            }

            toDatagrid.IsReadOnly = false;
            toDatagrid.BeginNewRow();
            toDatagrid.EndNewRow(true);

            DataTableConverter.SetValue(toDatagrid.CurrentRow.DataItem, "RSV_SEQ", (toDatagrid.CurrentCell.Row.Index + 1));
            if (_Unit.Equals("LOT"))
            {
                DataTableConverter.SetValue(toDatagrid.CurrentRow.DataItem, "LOTID", Util.NVC(DataTableConverter.GetValue(fromDatagrid.Rows[i].DataItem, "LOTID")));
            }
            else
            {
                DataTableConverter.SetValue(toDatagrid.CurrentRow.DataItem, "CSTID", Util.NVC(DataTableConverter.GetValue(fromDatagrid.Rows[i].DataItem, "CSTID")));
            }

            DataTableConverter.SetValue(toDatagrid.CurrentRow.DataItem, "LARGELOT", Util.NVC(DataTableConverter.GetValue(fromDatagrid.Rows[i].DataItem, "LARGELOT")));
            DataTableConverter.SetValue(toDatagrid.CurrentRow.DataItem, "WIPSTAT", Util.NVC(DataTableConverter.GetValue(fromDatagrid.Rows[i].DataItem, "WIPSTAT")));
            DataTableConverter.SetValue(toDatagrid.CurrentRow.DataItem, "WIPSNAME", Util.NVC(DataTableConverter.GetValue(fromDatagrid.Rows[i].DataItem, "WIPSNAME")));
            DataTableConverter.SetValue(toDatagrid.CurrentRow.DataItem, "WIPQTY", Util.NVC(DataTableConverter.GetValue(fromDatagrid.Rows[i].DataItem, "WIPQTY")));
            DataTableConverter.SetValue(toDatagrid.CurrentRow.DataItem, "MODLID", Util.NVC(DataTableConverter.GetValue(fromDatagrid.Rows[i].DataItem, "MODLID")));
            DataTableConverter.SetValue(toDatagrid.CurrentRow.DataItem, "PRODID", Util.NVC(DataTableConverter.GetValue(fromDatagrid.Rows[i].DataItem, "PRODID")));
            DataTableConverter.SetValue(toDatagrid.CurrentRow.DataItem, "PRODNAME", Util.NVC(DataTableConverter.GetValue(fromDatagrid.Rows[i].DataItem, "PRODNAME")));
            DataTableConverter.SetValue(toDatagrid.CurrentRow.DataItem, "ELEC", Util.NVC(DataTableConverter.GetValue(fromDatagrid.Rows[i].DataItem, "ELEC")));
            DataTableConverter.SetValue(toDatagrid.CurrentRow.DataItem, "ELECNAME", Util.NVC(DataTableConverter.GetValue(fromDatagrid.Rows[i].DataItem, "ELECNAME")));
            DataTableConverter.SetValue(toDatagrid.CurrentRow.DataItem, "S12", Util.NVC(DataTableConverter.GetValue(fromDatagrid.Rows[i].DataItem, "S12")));
            DataTableConverter.SetValue(toDatagrid.CurrentRow.DataItem, "UNIT", Util.NVC(DataTableConverter.GetValue(fromDatagrid.Rows[i].DataItem, "UNIT")));
            DataTableConverter.SetValue(toDatagrid.CurrentRow.DataItem, "PRJT_NAME", Util.NVC(DataTableConverter.GetValue(fromDatagrid.Rows[i].DataItem, "PRJT_NAME")));
            toDatagrid.IsReadOnly = true;

        }

        private bool ValidationLot(int index)
        {
            try
            {
                //DataTable dgWorkOrder = DataTableConverter.Convert(((grdWorkOrder.Children[0] as UC_WORKORDER_LINE).dgWorkOrder).ItemsSource);
                //if (dgWorkOrder.Select("EIO_WO_SEL_STAT = 'Y'").Count() == 0)
                //{
                //    Util.MessageValidation("SFU1443");  //작업지시를 선택하세요
                //    DataTableConverter.SetValue(dgCanReserve.Rows[index].DataItem, "CHK", 0);
                //    return false;
                //}

                //string prodid = Convert.ToString(dgWorkOrder.Rows[0]["PRODID"]);

                //string _VD_WRK_COND_GR_CODE = string.Empty;

                DataTable dt = new DataTable();
                //dt.Columns.Add("EQPTID", typeof(string));

                //DataRow row = dt.NewRow();
                //row["EQPTID"] = Convert.ToString(cboVDEquipment.SelectedValue);
                //dt.Rows.Add(row);

                //DataTable result = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_PRDT_CONV_RATE_WO_RECIP", "INDATA", "RSLTDT", dt);

                //if (result.Rows.Count == 0)
                //{
                //    //err; 설비 정보 없음
                //}

                //if (!Util.NVC(DataTableConverter.GetValue(dgCanReserve.Rows[index].DataItem, "PRODID")).Equals(prodid))
                //{
                //    _VD_WRK_COND_GR_CODE = Convert.ToString(result.Rows[0]["VD_WRK_COND_GR_CODE"]);
                //    if (_VD_WRK_COND_GR_CODE.Equals(""))
                //    {
                //        Util.MessageValidation("PRODID가 다릅니다.");
                //        DataTableConverter.SetValue(dgCanReserve.Rows[index].DataItem, "CHK", 0);
                //        return false;
                //    }
                //    else
                //    {
                //        if (!Util.NVC(DataTableConverter.GetValue(dgCanReserve.Rows[index].DataItem, "VD_WRK_COND_GR_CODE")).Equals(_VD_WRK_COND_GR_CODE))
                //        {
                //            Util.MessageValidation("해당 LOT의 레시피[%1]와 선택된 작업지시의 레시피[%2]가 다릅니다.", Util.NVC(DataTableConverter.GetValue(dgCanReserve.Rows[index].DataItem, "VD_WRK_COND_GR_CODE")), _VD_WRK_COND_GR_CODE);
                //            DataTableConverter.SetValue(dgCanReserve.Rows[index].DataItem, "CHK", 0);
                //            return false;
                //        }

                //    }
                //}

                #region 기존 주석 로직
                //if (Convert.ToString(result.Rows[0]["RECIPEID"]).Equals("")) //레시피 정보가 없으면 해당 PRODID만 예약 가능
                //{
                //    if (!Util.NVC(DataTableConverter.GetValue(dgCanReserveElec.Rows[index].DataItem, "PRODID")).Equals(prodid))
                //    {
                //        Util.MessageValidation("SFU2905");
                //        DataTableConverter.SetValue(dgCanReserveElec.Rows[index].DataItem, "CHK", 0);
                //        return false;
                //    }
                //    //같은제품만 선택되게
                //    List<System.Data.DataRow> list = DataTableConverter.Convert(dgCanReserveElec.ItemsSource).Select("CHK = 1").ToList();
                //    list = list.GroupBy(c => c["PRODID"]).Select(group => group.First()).ToList();
                //    if (list.Count > 1)
                //    {
                //        Util.MessageValidation("SFU1480"); //다른 제품을 선택하셨습니다.
                //        DataTableConverter.SetValue(dgCanReserveElec.Rows[index].DataItem, "CHK", 0);
                //        SetChkCutID(index, dgCanReserveElec);
                //        return false;
                //    }

                //}
                //else //레시피 정보가 같은 LOT은 모두 예약 가능
                //{
                //    recipe = Convert.ToString(result.Rows[0]["RECIPEID"]);
                //    if (!Util.NVC(DataTableConverter.GetValue(dgCanReserveElec.Rows[index].DataItem, "RECIPEID")).Equals(recipe))
                //    {
                //        Util.MessageValidation("해당 LOT의 레시피[%1]와 선택된 작업지시의 레시피[%2]가 다릅니다.", Util.NVC(DataTableConverter.GetValue(dgCanReserveElec.Rows[index].DataItem, "RECIPEID")), recipe);
                //        DataTableConverter.SetValue(dgCanReserveElec.Rows[index].DataItem, "CHK", 0);
                //        return false;
                //    }
                //}
                #endregion



                int firstIndex = _Util.GetDataGridCheckFirstRowIndex(dgCanReserve, "CHK");
                if (firstIndex == -1)
                {
                    dgLotList.IsReadOnly = false;
                    dgLotList.RemoveRow(0);
                    dgLotList.IsReadOnly = true;
                    return false;
                }


                dt = null;
                dt = DataTableConverter.Convert(dgCanReserve.ItemsSource);

                if (dt == null)
                {
                    return false;
                }
                if (dt.Select("CHK = '1'").Count() > _MaxNum)
                {
                    Util.MessageValidation("SFU1658", _MaxNum);//선택한 LOT의 개수가 {0}을 넘을 수 없습니다.
                    DataTableConverter.SetValue(dgCanReserve.Rows[index].DataItem, "CHK", 0);

                    return false;
                }


                /*
                 
                (1) 예약 대상 입력
                   1) 첫 작업대상을 Scan
                      - 첫 스캔 대상 구분이 LOT, SKID에 따라 다음 Scan 대상도 동일한 대상만 Scan 가능
                      - 첫 스캔 대상에 대해 W/O 존재 여부 체크 및 W/O 선택 처리 표시 
                   2) 첫번째 이후 작업대상 Scan
                      - 첫번째 대상의 구분(LOT, SKID) 및 동일 VD 작업그룹내에 해당하고 W/O 존재하는 것만 대상 선택 가능
                      - 첫번째 대상의 Cell Type(원형, 각형, 초소형, 파우치)이 일치하는 것만 선택 가능
                  ※ 기존 작업지시 선택 후 예약 처리 순서를 무시할 것
                  ※ VD 작업그룹이 등록되지 않은 경우는 VD 그룹이 등록되어 있는 것과 같이 작업될 수 없음
                    
                */

                bool bExistRsv = false;

                // W/O 존재 여부 체크 및 W/O 선택 처리 표시
                DataTable dgWorkOrder = DataTableConverter.Convert(((grdWorkOrder.Children[0] as UC_WORKORDER_LINE).dgWorkOrder).ItemsSource);
                DataRow[] dtWorkOrderRow = dgWorkOrder.Select("PRODID = '" + Util.NVC(DataTableConverter.GetValue(dgCanReserve.Rows[index].DataItem, "PRODID")) + "'");

                if (dtWorkOrderRow.Length == 0)
                {
                    Util.MessageValidation("SFU4249");  //선택 가능한 작업지시 정보가 없습니다.
                    DataTableConverter.SetValue(dgCanReserve.Rows[index].DataItem, "CHK", 0);
                    return false;
                }

                // 처음 Scan 인 경우에는 W/O 자동 선택 처리....
                if (dgLotList.Rows.Count < 1)
                {
                    if (dgWorkOrder.Select("EIO_WO_SEL_STAT = 'Y'").Count() == 0)
                    {
                        // 작업지시가 없는 경우 자동 선택 처리.
                        winWo.SetWorkOrderForVD(Util.NVC(dtWorkOrderRow[0]["WO_DETL_ID"]), true, false);

                        GetWorkOrder();
                    }
                    else
                    {
                        // 예약중인 Lot 존재여부 확인
                        if (CheckReserveLot())
                        {
                            //Util.MessageValidation("SFU1917");  //진행중인 LOT이 있습니다.
                            //DataTableConverter.SetValue(dgCanReserve.Rows[index].DataItem, "CHK", 0);
                            //return false;

                            bExistRsv = true;
                        }
                        else
                        {
                            // 예약 lot 없으면 작업지시 자동 선택 처리.
                            winWo.SetWorkOrderForVD(Util.NVC(dtWorkOrderRow[0]["WO_DETL_ID"]), true, false);

                            GetWorkOrder();
                        }
                    }

                    //GetWorkOrder();
                }


                // 작업그룹(레시피) 정보 확인
                DataTable dtSel = new DataTable();
                dtSel.Columns.Add("EQPTID", typeof(string));

                DataRow row = dtSel.NewRow();
                row["EQPTID"] = Convert.ToString(cboVDEquipment.SelectedValue);
                dtSel.Rows.Add(row);

                DataTable result = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_PRDT_CONV_RATE_WO_RECIP", "INDATA", "RSLTDT", dtSel);

                if (result.Rows.Count == 0)
                {
                    //err; 설비 정보 없음
                }

                // 첫번째 Scan
                if (dgLotList.Rows.Count < 1)
                {
                    //// W/O 선택 처리 표시
                    //SetSelectWorkOrder(Util.NVC(DataTableConverter.GetValue(dgCanReserve.Rows[index].DataItem, "PRODID")));

                    if (!bExistRsv)
                        _VD_WRK_COND_GR_CODE = Convert.ToString(result.Rows[0]["VD_WRK_COND_GR_CODE"]);
                    else
                    {
                        // 동일 작업그룹 확인
                        if (!Util.NVC(DataTableConverter.GetValue(dgCanReserve.Rows[index].DataItem, "VD_WRK_COND_GR_CODE")).Equals(_VD_WRK_COND_GR_CODE))
                        {
                            Util.MessageValidation("SFU4250", Util.NVC(DataTableConverter.GetValue(dgCanReserve.Rows[index].DataItem, "VD_WRK_COND_GR_CODE")), _VD_WRK_COND_GR_CODE);   // 해당 LOT의 레시피[%1]와 선택된 작업지시의 레시피[%2]가 다릅니다.
                            DataTableConverter.SetValue(dgCanReserve.Rows[index].DataItem, "CHK", 0);
                            return false;
                        }
                    }

                    // 첫번째 선택 제품 ID
                    _PRODID = Util.NVC(DataTableConverter.GetValue(dgCanReserve.Rows[index].DataItem, "PRODID"));
                }
                else
                {
                    // 첫 Scan 의 그룹 정보가 없는 경우에는 동일 제품만 예약 가능.
                    //if (Util.NVC(_VD_WRK_COND_GR_CODE).Trim().Equals(""))
                    //{
                    //    if (!_PRODID.Equals(Util.NVC(DataTableConverter.GetValue(dgCanReserve.Rows[index].DataItem, "PRODID"))))
                    //    {
                    //        Util.MessageValidation("SFU2929");  // 이전에 스캔한 LOT의 제품코드와 다릅니다.
                    //        DataTableConverter.SetValue(dgCanReserve.Rows[index].DataItem, "CHK", 0);
                    //        return false;
                    //    }
                    //}
                    //else
                    //{
                    // 동일 작업그룹 확인
                    if (!Util.NVC(DataTableConverter.GetValue(dgCanReserve.Rows[index].DataItem, "VD_WRK_COND_GR_CODE")).Equals(_VD_WRK_COND_GR_CODE))
                    {
                        Util.MessageValidation("SFU4250", Util.NVC(DataTableConverter.GetValue(dgCanReserve.Rows[index].DataItem, "VD_WRK_COND_GR_CODE")), _VD_WRK_COND_GR_CODE);   // 해당 LOT의 레시피[%1]와 선택된 작업지시의 레시피[%2]가 다릅니다.
                        DataTableConverter.SetValue(dgCanReserve.Rows[index].DataItem, "CHK", 0);
                        return false;
                    }
                    //}

                    // 첫번째 대상의 Cell Type(원형, 각형, 초소형, 파우치)이 일치하는 것만 선택 가능


                }


                return true;

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return false;
            }


        }

        private void SearchData()
        {
            try
            {
                DataTable dt = null;
                DataRow row = null;
                DataTable result = null;

                if (cboVDEquipment.Text.Equals("-SELECT-"))
                {
                    Util.MessageValidation("SFU1673");
                    return;
                }

                GetWorkOrder();

                loadingIndicator.Visibility = Visibility.Visible;

                initGridTable(dgLotList);
                dgReservedLot.ItemsSource = null;

                //예약가능LOT
                dt = new DataTable();
                dt.Columns.Add("LANGID", typeof(string));
                dt.Columns.Add("EQSGID", typeof(string));
                dt.Columns.Add("PROCID", typeof(string));
                dt.Columns.Add("ELEC", typeof(string));

                row = dt.NewRow();
                row["LANGID"] = LoginInfo.LANGID;
                row["EQSGID"] = Convert.ToString(cboVDEquipmentSegment.SelectedValue);
                row["PROCID"] = Convert.ToString(cboVDProcess.SelectedValue);
                row["ELEC"] = _EquipFlag.Equals("N") ? null : _EquipElec;
                dt.Rows.Add(row);

                result = new ClientProxy().ExecuteServiceSync(Util.NVC(cboVDProcess.SelectedValue).Equals("A6000") ? "DA_PRD_SEL_RESERVE_LOT_LIST_NJ_PU" : "DA_PRD_SEL_RESERVE_LOT_LIST_NJ", "INDATA", "RSLTDT", dt);
                //result = new ClientProxy().ExecuteServiceSync(_Unit.Equals("LOT") ? "DA_PRD_SEL_RESERVE_LOT_LIST_NJ_PU" : "DA_PRD_SEL_RESERVE_LOT_LIST_NJ", "INDATA", "RSLTDT", dt);
                Util.GridSetData(dgCanReserve, result, FrameOperation, true);
                /*
                DataTable dgWorkOrder = DataTableConverter.Convert(((grdWorkOrder.Children[0] as UC_WORKORDER_LINE).dgWorkOrder).ItemsSource);
                DataRow[] dr = dgWorkOrder.Select("EIO_WO_SEL_STAT = 'Y'");
                DataTable prodResult = null;
                if (dr.Length > 0)
                {
                   prodResult  = result.Select("PRODID = '" + dr[0]["PRODID"].ToString() + "'").CopyToDataTable();
                   Util.GridSetData(dgCanReserve, prodResult, FrameOperation, true);
                }
                else
                {
                    Util.GridSetData(dgCanReserve, result, FrameOperation, true);
                }
                */




                //예약된LOT
                dt = new DataTable();
                dt.Columns.Add("LANGID", typeof(string));
                dt.Columns.Add("EQSGID", typeof(string));
                dt.Columns.Add("PROCID", typeof(string));
                dt.Columns.Add("EQPTID", typeof(string));
                dt.Columns.Add("EQPT_ONLINE_FLAG_FLAG", typeof(string));

                row = dt.NewRow();
                row["LANGID"] = LoginInfo.LANGID;
                row["EQSGID"] = Convert.ToString(cboVDEquipmentSegment.SelectedValue);
                row["PROCID"] = Convert.ToString(cboVDProcess.SelectedValue);
                row["EQPTID"] = Convert.ToString(cboVDEquipment.SelectedValue).Equals("") ? null : Convert.ToString(cboVDEquipment.SelectedValue);
                row["EQPT_ONLINE_FLAG_FLAG"] = _EQPT_ONLINE_FLAG;
                dt.Rows.Add(row);

                result = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_WIP_VD_RESERVED_NJ", "INDATA", "RSLTDT", dt);
                dgReservedLot.ItemsSource = DataTableConverter.Convert(result);

                if (chkWoProduct.IsChecked == true)
                {
                    SetWorkorderProduct();
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
            finally
            {
                loadingIndicator.Visibility = Visibility.Collapsed;
            }

        }
        private void SetWorkorderProduct()
        {
            DataTable dgWorkOrder = DataTableConverter.Convert(((grdWorkOrder.Children[0] as UC_WORKORDER_LINE).dgWorkOrder).ItemsSource);
            if (dgWorkOrder.Select("EIO_WO_SEL_STAT = 'Y'").Count() == 0)
            {
                return;
            }

            string woprodid = Convert.ToString(dgWorkOrder.Rows[0]["PRODID"]);

            if (chkWoProduct.IsChecked == true)
            {
                try
                {
                    dtAllLot = DataTableConverter.Convert(dgCanReserve.ItemsSource);
                    DataTable dt = dtAllLot.Select("PRODID = '" + woprodid + "'").CopyToDataTable();
                    Util.GridSetData(dgCanReserve, dt, FrameOperation, true);
                }
                catch (Exception ex)
                {

                    dgCanReserve.ItemsSource = null;

                    FrameOperation.PrintFrameMessage("[" + System.DateTime.Now.ToString("hh:mm:ss") + "]" + MessageDic.Instance.GetMessage("SFU1905")); //조회된 Data가 없습니다.
                    return;
                }
            }
            else
            {
                Util.GridSetData(dgCanReserve, dtAllLot, FrameOperation);
            }
        }
        private void GetWorkOrder()
        {
            if (winWo == null)
                return;

            winWo.EQPTSEGMENT = Convert.ToString(cboVDEquipmentSegment.SelectedValue);
            winWo.EQPTID = Convert.ToString(cboVDEquipment.SelectedValue);
            winWo.PROCID = Convert.ToString(cboVDProcess.SelectedValue);

            winWo.GetWorkOrder();
        }
        private void initGridTable(C1.WPF.DataGrid.C1DataGrid datagrid)
        {
            DataTable dt = new DataTable();
            foreach (C1.WPF.DataGrid.DataGridColumn col in datagrid.Columns)
            {
                dt.Columns.Add(Convert.ToString(col.Name));
            }
            datagrid.BeginEdit();
            datagrid.ItemsSource = DataTableConverter.Convert(dt);
            datagrid.EndEdit();
        }

        public bool ClearControls()
        {
            bool bRet = false;

            try
            {
                Util.gridClear(dgCanReserve);
                //예약된 LOT clear

                bRet = true;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                bRet = false;
            }
            return bRet;
        }

        private bool ValidationReserve()
        {
            if (cboVDEquipment.Text.Equals("-ALL-"))
            {
                Util.MessageValidation("SFU1733"); //예약 할 설비를 선택해주세요
                return false;
            }

            if (_Util.GetDataGridCheckFirstRowIndex(dgCanReserve, "CHK") == -1)
            {
                Util.MessageValidation("SFU1632"); //선택된 LOT이 없습니다.
                return false;
            }

            if (dgReservedLot.Rows.Count != 0)
            {
                Util.MessageValidation("SFU1735"); //예약된 LOT이 이미 있습니다.
                return false;
            }

            for (int i = 0; i < dgCanReserve.GetRowCount(); i++)
            {
                if (Util.NVC(DataTableConverter.GetValue(dgCanReserve.Rows[i].DataItem, "CHK")).Equals("1"))
                {
                    if (_EquipElec != null && !Util.NVC(DataTableConverter.GetValue(dgCanReserve.Rows[i].DataItem, "ELEC")).Equals(_EquipElec))
                    {
                        Util.MessageValidation("SFU3070", Util.NVC(DataTableConverter.GetValue(dgCanReserve.Rows[i].DataItem, "LOTID"))); // [%1]와 설비의 극성이 다릅니다.
                        return false;
                    }

                }
            }


            return true;
        }



        private void dgReservedLot_BeganEdit(object sender, C1.WPF.DataGrid.DataGridBeganEditEventArgs e)
        {
            if (_Unit.Equals("LOT"))
            {
                return;
            }

            dgReservedLot.Dispatcher.BeginInvoke(new Action(() =>
            {
                try
                {
                    if (e.Column is C1.WPF.DataGrid.DataGridCheckBoxColumn)
                    {
                        C1.WPF.DataGrid.DataGridCell cell = (sender as C1DataGrid).GetCell(e.Row.Index, e.Column.Index);
                        CheckBox cb = cell.Presenter.Content as CheckBox;

                        int index = ((C1.WPF.DataGrid.DataGridCellPresenter)cb.Parent).Row.Index;
                        SetChkCutID(index, dgReservedLot);

                    }

                }
                catch (Exception ex)
                {
                    Util.MessageException(ex);
                }

            }));
        }

        private void SetInfo()
        {
            try
            {
                if (Util.NVC(cboVDEquipment.Text).IndexOf("SELECT") >= 0 || Util.NVC(cboVDEquipment.Text).Equals(""))
                {
                    Util.MessageValidation("SFU1673");
                    txtLOTID.Text = "";
                    return;
                }

                /*
                 * 1. LOT 스캔 첫번째 스캔 이면 공정 확인후 화면 공정 자동 선택 (최초 선택 여부는 하단의 선택 DATA 존재로 확인)
                 * 2. 공정이 바뀌면 LOT/SKID VIEW 변경 및 조회 DATA CLEAR
                 * 3. 이전에 선택했던 설비 자동 선택
                 * 4. 해당 예약 가능 DATA 조회
                 * 5. 스캔한 DATA 자동 선택
                 * 6. 선택하면 W/O 확인하여 선택 VIEW 처리
                 * 7. 예약 할때 W/O DATA UPDATE
                 * 8. 착공 BIZ에서 W/O VALIDATION 로직 은 랏의 W/O 로 처리
                */

                // 1. 첫번째 Scan 확인
                if (dgLotList.Rows.Count < 1)
                {
                    string sProcid = GetProcessID();

                    if (Util.NVC(sProcid).Equals(""))
                    {
                        Util.MessageValidation("SFU4251");  // 해당 Lot(Skid) ID는 정보가 존재하지 않습니다.
                        txtLOTID.Text = "";
                        return;
                    }

                    if (sProcid.Equals(Util.NVC(cboVDProcess.SelectedValue)))
                    {
                        #region 조회된 data에서 체크.. 기존 로직..
                        for (int i = 0; i < dgCanReserve.Rows.Count; i++)
                        {
                            if (Util.NVC(DataTableConverter.GetValue(dgCanReserve.Rows[i].DataItem, _Unit.Equals("LOT") ? "LOTID" : "CSTID")).Equals(txtLOTID.Text))
                            {
                                DataTableConverter.SetValue(dgCanReserve.Rows[i].DataItem, "CHK", true);

                                if (!ValidationLot(i))
                                {
                                    txtLOTID.Text = "";
                                    return;
                                }

                                if (!_Unit.Equals("LOT"))
                                {
                                    SetChkCutID(i, dgCanReserve);
                                }

                                dgCanReserve.ScrollIntoView(i, 0);

                                AddRow(dgLotList, dgCanReserve, i);

                                txtLOTID.Text = "";
                                return;
                            }
                        }
                        Util.MessageValidation("SFU1734"); //예약가능 LOT 리스트에 없습니다.
                        txtLOTID.Text = "";
                        #endregion
                    }
                    else // 2. 공정이 바뀌면 LOT/SKID VIEW 변경 및 조회 DATA CLEAR
                    {
                        Util.gridClear(dgCanReserve);
                        Util.gridClear(dgLotList);
                        Util.gridClear(dgReservedLot);


                        string sTmpEqpid = Util.NVC(cboVDEquipment.SelectedValue);

                        // 공정 자동 선택
                        cboVDProcess.SelectedValueChanged -= cboVDProcess_SelectedValueChanged;
                        cboVDProcess.SelectedValue = sProcid;

                        if (cboVDProcess.SelectedIndex < 0)
                            cboVDProcess.SelectedIndex = 0;

                        InitializeControls();

                        // 설비 조회
                        cboVDEquipment.SelectedValueChanged -= cboVDEquipment_SelectedValueChanged;

                        String[] sFilter = { cboVDProcess.SelectedValue.ToString(), cboVDEquipmentSegment.SelectedValue.ToString() };
                        combo.SetCombo(cboVDEquipment, CommonCombo.ComboStatus.SELECT, sFilter: sFilter);

                        cboVDProcess.SelectedValueChanged += cboVDProcess_SelectedValueChanged;

                        // 설비 자동 선택
                        cboVDEquipment.SelectedValue = sTmpEqpid;

                        if (cboVDEquipment.SelectedIndex < 0)
                            cboVDEquipment.SelectedIndex = 0;

                        cboVDEquipment.SelectedValueChanged += cboVDEquipment_SelectedValueChanged;

                        if (cboVDEquipment.Text.Equals("-SELECT-"))
                        {
                            Util.MessageValidation("SFU1673");
                            return;
                        }

                        // 4. 해당 예약 가능 DATA 조회
                        btnSearch_Click(null, null);


                        #region 5. 스캔한 DATA 자동 선택.. 기존 로직..
                        for (int i = 0; i < dgCanReserve.Rows.Count; i++)
                        {
                            if (Util.NVC(DataTableConverter.GetValue(dgCanReserve.Rows[i].DataItem, _Unit.Equals("LOT") ? "LOTID" : "CSTID")).Equals(txtLOTID.Text))
                            {
                                DataTableConverter.SetValue(dgCanReserve.Rows[i].DataItem, "CHK", true);

                                if (!ValidationLot(i))
                                {
                                    txtLOTID.Text = "";
                                    return;
                                }

                                if (!_Unit.Equals("LOT"))
                                {
                                    SetChkCutID(i, dgCanReserve);
                                }

                                dgCanReserve.ScrollIntoView(i, 0);

                                AddRow(dgLotList, dgCanReserve, i);

                                txtLOTID.Text = "";
                                return;
                            }
                        }
                        Util.MessageValidation("SFU1734"); //예약가능 LOT 리스트에 없습니다.
                        txtLOTID.Text = "";
                        #endregion
                    }
                }
                else
                {
                    #region 5. 스캔한 DATA 자동 선택.. 기존 로직..
                    for (int i = 0; i < dgCanReserve.Rows.Count; i++)
                    {
                        if (Util.NVC(DataTableConverter.GetValue(dgCanReserve.Rows[i].DataItem, _Unit.Equals("LOT") ? "LOTID" : "CSTID")).Equals(txtLOTID.Text))
                        {
                            DataTableConverter.SetValue(dgCanReserve.Rows[i].DataItem, "CHK", true);

                            if (!ValidationLot(i))
                            {
                                txtLOTID.Text = "";
                                return;
                            }

                            if (!_Unit.Equals("LOT"))
                            {
                                SetChkCutID(i, dgCanReserve);
                            }

                            dgCanReserve.ScrollIntoView(i, 0);

                            AddRow(dgLotList, dgCanReserve, i);

                            txtLOTID.Text = "";
                            return;
                        }
                    }
                    Util.MessageValidation("SFU1734"); //예약가능 LOT 리스트에 없습니다.
                    txtLOTID.Text = "";
                    #endregion
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private string GetProcessID()
        {
            try
            {
                string sRet = "";

                loadingIndicator.Visibility = Visibility.Visible;

                DataTable inTable = new DataTable();
                inTable.Columns.Add("SCANID", typeof(string));
                inTable.Columns.Add("EQSGID", typeof(string));

                DataRow newRow = inTable.NewRow();
                newRow["SCANID"] = Util.NVC(txtLOTID.Text);
                newRow["EQSGID"] = Util.NVC(cboVDEquipmentSegment.SelectedValue);

                inTable.Rows.Add(newRow);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_PROCID_VD_RESERVE_NJ", "INDATA", "OUTDATA", inTable);

                if (dtResult != null && dtResult.Rows.Count > 0)
                {
                    sRet = Util.NVC(dtResult.Rows[0]["PROCID"]);
                }

                loadingIndicator.Visibility = Visibility.Collapsed;

                return sRet;
            }
            catch (Exception ex)
            {
                loadingIndicator.Visibility = Visibility.Collapsed;
                Util.MessageException(ex);
                return "";
            }
        }

        private void InitializeControls()
        {
            try
            {
                if (cboVDProcess.SelectedValue == null) return;

                if (Util.NVC(cboVDProcess.SelectedValue).Equals("A6000"))//파우치
                {
                    SKIDID_CANRESERVE.Visibility = Visibility.Collapsed;
                    SKIDID_LOTLIST.Visibility = Visibility.Collapsed;
                    LOTID_LOTLIST.Visibility = Visibility.Visible;
                    SKIDID_RESERVED.Visibility = Visibility.Collapsed;

                    tbSKIDID.Visibility = Visibility.Collapsed;
                    tbLotID.Visibility = Visibility.Visible;

                    _Unit = "LOT";
                }
                else //원각형
                {
                    // 원각 사용자 교육 중 랏단위로 작업 하겠다고 하여 임시 처리.(2017.11.01)
                    if (chkUseGroup != null && chkUseGroup.Visibility == Visibility.Visible && chkUseGroup.IsChecked.HasValue && (bool)chkUseGroup.IsChecked)//if (GetUseSkid())
                    {
                        SKIDID_CANRESERVE.Visibility = Visibility.Visible;
                        SKIDID_LOTLIST.Visibility = Visibility.Visible;
                        LOTID_LOTLIST.Visibility = Visibility.Collapsed;
                        SKIDID_RESERVED.Visibility = Visibility.Visible;

                        tbSKIDID.Visibility = Visibility.Visible;
                        tbLotID.Visibility = Visibility.Collapsed;

                        _Unit = "SKID";
                    }
                    else
                    {
                        SKIDID_CANRESERVE.Visibility = Visibility.Collapsed;
                        SKIDID_LOTLIST.Visibility = Visibility.Collapsed;
                        LOTID_LOTLIST.Visibility = Visibility.Visible;
                        SKIDID_RESERVED.Visibility = Visibility.Collapsed;

                        tbSKIDID.Visibility = Visibility.Collapsed;
                        tbLotID.Visibility = Visibility.Visible;

                        _Unit = "LOT";
                    }
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void SetSelectWorkOrder(string sProdID)
        {
            try
            {
                C1DataGrid dg = new C1DataGrid();

                if (dg.Columns.Contains("CHK"))
                {
                    DataTable dt = DataTableConverter.Convert(dg.ItemsSource);
                    if (dt != null)
                    {
                        DataRow[] dtRows = dt.Select("PRODID = '" + sProdID + "' AND CHK <> '1'");

                        for (int i = 0; i < dtRows.Length; i++)
                        {
                            // View 선택 처리..
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private bool CheckReserveLot()
        {
            try
            {
                bool bRet = false;

                string ValueToCondition = string.Empty;

                //ValueToCondition = "RESERVE";// "PROC,RESERVE,READY";

                DataTable inTable = new DataTable();
                inTable.Columns.Add("LANGID", typeof(string));
                inTable.Columns.Add("EQPTID", typeof(string));
                inTable.Columns.Add("PROCID", typeof(string));
                //inTable.Columns.Add("WIPSTAT", typeof(string));

                DataRow newRow = inTable.NewRow();
                newRow["LANGID"] = LoginInfo.LANGID;
                newRow["EQPTID"] = Convert.ToString(cboVDEquipment.SelectedValue);
                newRow["PROCID"] = cboVDProcess.SelectedValue.ToString();
                //newRow["WIPSTAT"] = ValueToCondition;

                inTable.Rows.Add(newRow);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync(Util.NVC(cboVDProcess.SelectedValue).Equals("A6000") ? "DA_PRD_SEL_VD_RESV_LOT_INFO_NJ_PU" : "DA_PRD_SEL_VD_RESV_LOT_INFO_NJ", "RQSTDT", "RSLTDT", inTable);
                //DataTable dtResult = new ClientProxy().ExecuteServiceSync(_Unit.Equals("LOT") ? "DA_PRD_SEL_VD_RESV_LOT_INFO_NJ_PU" : "DA_PRD_SEL_VD_RESV_LOT_INFO_NJ", "RQSTDT", "RSLTDT", inTable);

                if (dtResult != null && dtResult.Rows.Count > 0)
                {
                    bRet = true;

                    _VD_WRK_COND_GR_CODE = Util.NVC(dtResult.Rows[0]["VD_WRK_COND_GR_CODE"]);
                }

                return bRet;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return false;
            }
        }

        private bool GetUseSkid()
        {
            try
            {
                bool bRet = false;

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_CR_VD_SKID_USE_YN_NJ", "RQSTDT", "RSLTDT", null);

                if (dtResult != null && dtResult.Rows.Count > 0 && Util.NVC(dtResult.Rows[0]["NJ_VD_CR_SKID_USE_FLAG"]).Equals("Y"))
                {
                    bRet = true;
                }

                return bRet;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return false;
            }
        }

        private void GetProductLot()
        {
            try
            {
                DataTable inTable = new DataTable();
                inTable.Columns.Add("LANGID", typeof(string));
                inTable.Columns.Add("PROCID", typeof(string));
                inTable.Columns.Add("EQSGID", typeof(string));
                inTable.Columns.Add("EQPTID", typeof(string));
                inTable.Columns.Add("WIPSTAT", typeof(string));

                DataRow newRow = inTable.NewRow();
                newRow["LANGID"] = LoginInfo.LANGID;
                newRow["EQPTID"] = Util.NVC(cboVDEquipment.SelectedValue);
                newRow["PROCID"] = Process.VD_LMN_AFTER_STP;
                newRow["EQSGID"] = Util.NVC(cboVDEquipmentSegment.SelectedValue);
                newRow["WIPSTAT"] = "RESERVE";

                inTable.Rows.Add(newRow);

                dtMain = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_WIP_VDSTP", "INDATA", "OUTDATA", inTable);
                Util.GridSetData(dgCanReserve, dtMain, FrameOperation);

                if (dtMain?.Select("WIPSTAT IN ('PROC', 'EQPT_END')").Length > 0)
                {
                    if (dgCanReserve.Columns.Contains("EQPT_BTCH_WRK_NO"))
                        dgCanReserve.Columns["EQPT_BTCH_WRK_NO"].Visibility = Visibility.Visible;
                }
                else
                {
                    if (dgCanReserve.Columns.Contains("EQPT_BTCH_WRK_NO"))
                        dgCanReserve.Columns["EQPT_BTCH_WRK_NO"].Visibility = Visibility.Collapsed;
                }

                HiddenLoadingIndicator();
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                HiddenLoadingIndicator();
            }
        }

        #endregion

        private void chkUseGroup_Checked(object sender, RoutedEventArgs e)
        {
            try
            {
                if (sender == null) return;

                if (!Util.NVC(cboVDProcess?.SelectedValue).Equals("A6000"))
                {
                    Util.gridClear(dgCanReserve);
                    Util.gridClear(dgLotList);
                    Util.gridClear(dgReservedLot);

                    InitializeControls();

                    if (Util.NVC(cboVDEquipment?.SelectedValue).IndexOf("SELECT") < 0)
                    {
                        SearchData();
                    }


                    //if ((sender as CheckBox).IsChecked.HasValue && (sender as CheckBox).IsChecked == true)
                    //{
                    //    Util.gridClear(dgCanReserve);
                    //    Util.gridClear(dgLotList);
                    //    Util.gridClear(dgReservedLot);

                    //    SKIDID_CANRESERVE.Visibility = Visibility.Visible;
                    //    SKIDID_LOTLIST.Visibility = Visibility.Visible;
                    //    LOTID_LOTLIST.Visibility = Visibility.Collapsed;
                    //    SKIDID_RESERVED.Visibility = Visibility.Visible;

                    //    tbSKIDID.Visibility = Visibility.Visible;
                    //    tbLotID.Visibility = Visibility.Collapsed;

                    //    _Unit = "SKID";
                    //}
                    //else if ((sender as CheckBox).IsChecked.HasValue && (sender as CheckBox).IsChecked == false)
                    //{
                    //    Util.gridClear(dgCanReserve);
                    //    Util.gridClear(dgLotList);
                    //    Util.gridClear(dgReservedLot);

                    //    SKIDID_CANRESERVE.Visibility = Visibility.Collapsed;
                    //    SKIDID_LOTLIST.Visibility = Visibility.Collapsed;
                    //    LOTID_LOTLIST.Visibility = Visibility.Visible;
                    //    SKIDID_RESERVED.Visibility = Visibility.Collapsed;

                    //    tbSKIDID.Visibility = Visibility.Collapsed;
                    //    tbLotID.Visibility = Visibility.Visible;

                    //    _Unit = "LOT";
                    //}
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void chkUseGroup_Unchecked(object sender, RoutedEventArgs e)
        {
            try
            {
                if (sender == null) return;

                if (!Util.NVC(cboVDProcess?.SelectedValue).Equals("A6000"))
                {
                    Util.gridClear(dgCanReserve);
                    Util.gridClear(dgLotList);
                    Util.gridClear(dgReservedLot);

                    InitializeControls();

                    if (Util.NVC(cboVDEquipment?.SelectedValue).IndexOf("SELECT") < 0)
                    {
                        SearchData();
                    }
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void Run_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed &&
                (Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control
               //(Keyboard.Modifiers & ModifierKeys.Alt) == ModifierKeys.Alt &&
               //(Keyboard.Modifiers & ModifierKeys.Shift) == ModifierKeys.Shift
               )
            {
                if (chkUseGroup != null && chkUseGroup.Visibility == Visibility.Collapsed)
                {
                    chkUseGroup.Visibility = Visibility.Visible;
                }
                else if (chkUseGroup != null && chkUseGroup.Visibility == Visibility.Visible)
                {
                    chkUseGroup.Visibility = Visibility.Collapsed;
                }
            }
        }

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

        private void btnMoveWip_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!ValidMoveWip()) return;

                Util.MessageConfirm("SFU1763", (result) => //이동 하시겠습니까?
                {
                    if (result == MessageBoxResult.OK)
                    {
                        try
                        {
                            ShowLoadingIndicator();

                            DataSet indataSet = new DataSet();
                            DataTable ineqp = indataSet.Tables.Add("INDATA");
                            ineqp.Columns.Add("SRCTYPE", typeof(string));
                            ineqp.Columns.Add("IFMODE", typeof(string));
                            ineqp.Columns.Add("PROCID", typeof(string));
                            ineqp.Columns.Add("EQPTID", typeof(string));
                            ineqp.Columns.Add("USERID", typeof(string));
                            ineqp.Columns.Add("PROCID_TO", typeof(string));
                            ineqp.Columns.Add("EQSGID_TO", typeof(string));
                            ineqp.Columns.Add("FLOWNORM", typeof(string));

                            DataRow row = null;

                            row = ineqp.NewRow();
                            row["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                            row["IFMODE"] = IFMODE.IFMODE_OFF;

                            if(LoginInfo.CFG_AREA_ID == "M9")
                                row["PROCID"] = cboVDProcess.SelectedValue.ToString(); // 조회된 LOT의 공정 (from 공정) - 오창 소형2동
                            else
                                row["PROCID"] = Process.VD_LMN_AFTER_STP; // 조회된 LOT의 공정 (from 공정) - 남경

                            row["EQPTID"] = Util.NVC(cboVDEquipment.SelectedValue);
                            row["USERID"] = LoginInfo.USERID;
                            row["PROCID_TO"] = null;
                            row["EQSGID_TO"] = null;
                            row["FLOWNORM"] = "Y";  // 정상 흐름 여부

                            ineqp.Rows.Add(row);

                            DataTable INLOT = indataSet.Tables.Add("INLOT");
                            INLOT.Columns.Add("LOTID", typeof(string));
                            INLOT.Columns.Add("WIPNOTE", typeof(string));

                            DataTable dtProductLot = DataTableConverter.Convert(dgCanReserve.ItemsSource);
                            foreach (DataRow _iRow in dtProductLot.Rows)
                            {
                                if (_iRow["CHK"].ToString().Equals("1") || _iRow["CHK"].ToString().Equals("True"))
                                {
                                    row = INLOT.NewRow();
                                    row["LOTID"] = _iRow["LOTID"];

                                    INLOT.Rows.Add(row);
                                }
                            }

                            new ClientProxy().ExecuteService_Multi("BR_PRD_REG_LOCATE_LOT_S", "INDATA,INLOT", null, (bizResult, bizException) =>
                            {
                                try
                                {
                                    if (bizException != null)
                                    {
                                        Util.MessageException(bizException);
                                        return;
                                    }

                                    GetProductLot();
                                    chkAll.IsChecked = false;

                                    Util.MessageInfo("SFU1275");    //정상 처리 되었습니다.

                                    //재공이동 후 자동 조회 처리
                                    loadingIndicator.Visibility = Visibility.Visible;
                                    this.Dispatcher.BeginInvoke(new Action(() => btnSearch_Click(null, null)));

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

                        }
                        catch (Exception ex)
                        {
                            Util.MessageException(ex);
                            HiddenLoadingIndicator();
                        }
                    }

                });
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }


        }

        private bool ValidMoveWip()
        {
            if (_Util.GetDataGridCheckFirstRowIndex(dgCanReserve, "CHK") == -1)
            {
                Util.MessageValidation("SFU1661"); //선택 된 LOT이 없습니다.
                return false;

            }

            for (int i = 0; i < dgCanReserve.GetRowCount(); i++)
            {
                if (Util.NVC(DataTableConverter.GetValue(dgCanReserve.Rows[i].DataItem, "CHK")).Equals("1") || Util.NVC(DataTableConverter.GetValue(dgCanReserve.Rows[i].DataItem, "CHK")).Equals("True"))
                {
                    if (!Util.NVC(DataTableConverter.GetValue(dgCanReserve.Rows[i].DataItem, "WIPSTAT")).Equals("WAIT"))
                    {
                        Util.MessageValidation("SFU1869"); //재공 상태가 이동가능한 상태가 아닙니다.
                        return false;
                    }
                }
            }

            return true;
        }
    }
}
