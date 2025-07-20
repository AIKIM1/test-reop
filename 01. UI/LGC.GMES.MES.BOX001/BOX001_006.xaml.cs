/*************************************************************************************
 Created Date : 2016.06.16
      Creator : 
   Decription : 
--------------------------------------------------------------------------------------
 [Change History]
  2016.06.16  DEVELOPER : Initial Created.
  2021.04.07  cnskmaru      C20210409-000020 매거진 포장 처리 속도에 문제가 있어 개선함
                                             실적확정 처리 시 매거진에 포함되어 있는 Cell 체크 처리 기능을
                                             매거진 입력 처리에서 수행 하도록 변경
**************************************************************************************/

using C1.WPF;
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

namespace LGC.GMES.MES.BOX001
{
    public partial class BOX001_006 : UserControl, IWorkArea
    {
        #region Declaration & Constructor 
        private Util _Util = new Util();
        CommonCombo _combo = new CMM001.Class.CommonCombo();

        // 스프레드에 새로 Row를 추가해야 해서 필요한 변수
        private DataTable isDataTable = new DataTable();

        string sAREAID = string.Empty;
        string sSHOPID = string.Empty;

        // 스캔 수량 저장하기 위한 수량
        private int isScanQty = 0;

        private string sWO_DETL_ID = string.Empty;


     C1.WPF.DataGrid.DataGridRowHeaderPresenter pre = new C1.WPF.DataGrid.DataGridRowHeaderPresenter()
        {
            Background = new SolidColorBrush(Colors.Transparent),
            MouseOverBrush = new SolidColorBrush(Colors.Transparent),
        };
        CheckBox chkAll = new CheckBox()
        {
            Content = "ALL",
            IsChecked = true,
            Background = new SolidColorBrush(Colors.Transparent),
            VerticalAlignment = System.Windows.VerticalAlignment.Center,
            HorizontalAlignment = System.Windows.HorizontalAlignment.Center
        };

        public BOX001_006()
        {
            InitializeComponent();
            Loaded += BOX001_006_Loaded;
        }

        private void BOX001_006_Loaded(object sender, RoutedEventArgs e)
        {
            Loaded -= BOX001_006_Loaded;

            // 기본은 체크 해제
            //chkTrayRefresh.Checked = false;
            //chkDummy.Visible = false;

            setInit();
            List<Button> listAuth = new List<Button>();
            listAuth.Add(btnConfirm);
            Util.pageAuth(listAuth, FrameOperation.AUTHORITY);

        }

        /// <summary>
        /// Frame과 상호작용하기 위한 객체
        /// </summary>
        public IFrameOperation FrameOperation
        {
            get;
            set;
        }

        #endregion

        #region Initialize


        private void setInit()
        {
            if (LoginInfo.LOGGEDBYSSO == true)
            {
                txtWorker.Tag = LoginInfo.USERID;
                txtWorker.Text = LoginInfo.USERNAME;
            }

            //라인,모델 셋팅
            // String[] sFilter = { LoginInfo.CFG_AREA_ID };    // Area
            _combo.SetCombo(cboArea, CommonCombo.ComboStatus.SELECT, sCase: "AREA_CP");

            txtMagazineID.MaxLength = 14;
            txtMagazineID.Focus();
            txtMagazineID.SelectAll();

        }

        #endregion


        #region Event


        /// <summary>
        /// 사용안함....
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnWOSearch_Click(object sender, RoutedEventArgs e)
        {

            string sModelid = Util.NVC(cboModelLot.SelectedValue);
            if (sModelid == string.Empty || sModelid == "SELECT")
            {
                Util.MessageValidation("SFU1257"); //"모델을 선택해주십시오."
                return;   
            }

            BOX001_006_WORKORDER popUp = new BOX001_006_WORKORDER();
            popUp.FrameOperation = this.FrameOperation;

            if (popUp != null)
            {
                object[] Parameters = new object[4];
                Parameters[0] = LoginInfo.CFG_AREA_ID;
                Parameters[1] = Util.NVC(cboEquipmentSegment.SelectedValue);
                Parameters[2] = Process.CELL_BOXING;
                Parameters[3] = sModelid;
                C1WindowExtension.SetParameters(popUp, Parameters);

                popUp.Closed += new EventHandler(wndWO_Closed);
                this.Dispatcher.BeginInvoke(new Action(() => popUp.ShowModal()));
            }
        }

        private void wndWO_Closed(object sender, EventArgs e)
        {
            BOX001.BOX001_006_WORKORDER wndPopup = sender as BOX001.BOX001_006_WORKORDER;
            if (wndPopup.DialogResult == MessageBoxResult.OK)
            {
                //초기화
                AllClear();

                DataTable dt = wndPopup.retDT;
                if (dt.Rows.Count > 0)
                {
                    sWO_DETL_ID = Util.NVC(dt.Rows[0]["WO_DETL_ID"]);
                    //txtWO.Text = Util.NVC(dt.Rows[0]["WOID"]);
                    txtProdID.Text = Util.NVC(dt.Rows[0]["PRODID"]);
                    //txtProdName.Text = Util.NVC(dt.Rows[0]["PRODNAME"]);
                }
            }
            grdMain.Children.Remove(wndPopup);
        }

        private void txtMagazineID_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                // MagazineID 공백 or 널값 여부 확인
                if (!string.IsNullOrEmpty(txtMagazineID.Text))
                {
                    // 기본적인 Validation
                    string sLine = Util.NVC(cboEquipmentSegment.SelectedValue);
                    if (sLine == string.Empty || sLine == "SELECT")
                    {
                        Util.WarningPlayer();
                        //"라인을 선택해주십시오." >> 라인을 선택해 주세요.
                        Util.MessageValidation("SFU1223");
                       // LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("SFU1223"), null, "Information", MessageBoxButton.OK, MessageBoxIcon.None);
                    
                        return;
                    }

                    string sModel = Util.NVC(cboModelLot.SelectedValue);
                    if (sModel == string.Empty || sModel == "SELECT")
                    {
                        Util.WarningPlayer();
                        //모델을 선택해주십시오.
                        Util.MessageValidation("SFU1257");
                       // LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("SFU1257"), null, "Information", MessageBoxButton.OK, MessageBoxIcon.None);
                        return;
                    }

                    //if (txtWO.Text.Trim() == string.Empty)
                    //{
                    //    LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("작업지시를 선택해주십시오."), null, "Information", MessageBoxButton.OK, MessageBoxIcon.None);
                    //    return;
                    //}

                    // 동일한 매거진 ID 가 입력되었는지 여부 확인
                    // 스프레드 rows 카운트가 0보다 크면 아래 로직 수행
                    if (dgScanInfo.GetRowCount() > 0)
                    {
                        // 스프레드에 있는 값과 동일한 값이 입력되었다면 return됨.
                        for (int i = 0; i < dgScanInfo.GetRowCount(); i++)
                        {
                            if (Util.NVC(dgScanInfo.GetCell(i, dgScanInfo.Columns["TRAY_MAGAZINE"].Index).Value) == txtMagazineID.Text)
                            {
                                Util.WarningPlayer();
                                //아래쪽 List에 이미 존재하는 매거진 ID입니다.
                                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("SFU3160"), null, "Information", MessageBoxButton.OK, MessageBoxIcon.None, (result) =>
                                {
                                    if (result == MessageBoxResult.OK)
                                    {
                                        txtMagazineID.Focus();
                                        txtMagazineID.SelectAll();
                                    }
                                });
                                return;
                            }
                        }
                    }


                    try
                    {

                        if (isDataTable.Columns.Count <= 0)
                        {
                            // 스프레드에 어떤 값도 입력이 되어 있지 않다면, new DataColumn을 사용해 DataTable 구조를 만듬.
                            isDataTable.Columns.Add("CHK", typeof(Boolean));
                            isDataTable.Columns.Add("SEQ_NO", typeof(string));
                            isDataTable.Columns.Add("TRAY_MAGAZINE", typeof(string));
                            isDataTable.Columns.Add("QTY", typeof(string));
                            isDataTable.Columns.Add("PRODID", typeof(string));
                            isDataTable.Columns.Add("LOTID", typeof(string));
                            isDataTable.Columns.Add("MDF_TIME", typeof(string));
                        }

                        Search2Magazine();
                        Util.DingPlayer();
                    }
                    catch (Exception ex)
                    {
                        Util.WarningPlayer();
                        LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex.Message), null, "Information", MessageBoxButton.OK, MessageBoxIcon.None, (result) =>
                        {                         

                            if (result == MessageBoxResult.OK)
                            {                               
                                txtMagazineID.Focus();
                                txtMagazineID.SelectAll();
                            }
                        });
                        return;
                    }
                    finally
                    {

                    }
                }
            }
        }

        /// <summary>
        /// 매거진 삭제
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (dgScanInfo.GetRowCount() == 0) return;

                //선택된 매거진ID를 List에서 삭제하시겠습니까?  >> 삭제하시겠습니까?
              //  LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("SFU1230"), null, "Information", MessageBoxButton.OKCancel, MessageBoxIcon.None, (result) =>
              Util.MessageConfirm("SFU1230",(result)=>
                {
                    if (result == MessageBoxResult.OK)
                    {

                        for (int i = 0; i < dgScanInfo.GetRowCount(); i++)
                        {
                            if (Util.NVC(dgScanInfo.GetCell(i, dgScanInfo.Columns["CHK"].Index).Value) == "True")
                            {
                                dgScanInfo.IsReadOnly = false;
                                dgScanInfo.RemoveRow(i);
                                dgScanInfo.IsReadOnly = true;
                            }
                        }

                        isDataTable.Clear();

                        if (dgScanInfo.GetRowCount() <= 0)
                        {
                        // 행이 존재하지 않으니 객체 초기화해서 차후 에러 방지함
                        txtMagazineID.Focus();
                            txtMagazineID.SelectAll();
                            txtCellCount.Text = "0";
                            txtScanqty.Text = "0";
                            isScanQty = 0;
                        }
                        else
                        {
                        //  Scan 수량, Cell 수량, 번호 설정
                        int Cell_Qty = 0;
                            isScanQty = 0;
                            for (int i = 0; i < dgScanInfo.GetRowCount(); i++)
                            {
                                if (dgScanInfo.Rows[i].Visibility == Visibility.Visible)
                                {
                                    isScanQty++;
                                    DataTableConverter.SetValue(dgScanInfo.Rows[i].DataItem, "SEQ_NO", isScanQty.ToString());
                                    Cell_Qty += Util.NVC_Int(dgScanInfo.GetCell(i, dgScanInfo.Columns["QTY"].Index).Value);

                                    DataRow dRow = isDataTable.NewRow();
                                    dRow["SEQ_NO"] = Util.NVC(dgScanInfo.GetCell(i, dgScanInfo.Columns["SEQ_NO"].Index).Value);
                                    dRow["TRAY_MAGAZINE"] = Util.NVC(dgScanInfo.GetCell(i, dgScanInfo.Columns["TRAY_MAGAZINE"].Index).Value);
                                    dRow["PRODID"] = Util.NVC(dgScanInfo.GetCell(i, dgScanInfo.Columns["PRODID"].Index).Value);
                                    dRow["QTY"] = Util.NVC(dgScanInfo.GetCell(i, dgScanInfo.Columns["QTY"].Index).Value);
                                    dRow["LOTID"] = Util.NVC(dgScanInfo.GetCell(i, dgScanInfo.Columns["LOTID"].Index).Value);
                                    dRow["MDF_TIME"] = Util.NVC(dgScanInfo.GetCell(i, dgScanInfo.Columns["MDF_TIME"].Index).Value);
                                    isDataTable.Rows.Add(dRow);
                                }
                            }

                        //dgScanInfo.ItemsSource = DataTableConverter.Convert(isDataTable);
                        Util.GridSetData(dgScanInfo, isDataTable, FrameOperation, true);
                            txtScanqty.Text = isScanQty.ToString();
                            txtCellCount.Text = Cell_Qty.ToString();
                        }

                        dgCELLInfo.ItemsSource = null;
                    }

                });
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        /// <summary>
        /// 포장구성확인 해제 선택시
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void chkPack_Checked(object sender, RoutedEventArgs e)
        {
            if (chkPack.IsChecked == true)
            {
                Util.MessageInfo("SFU1927"); //"체크할 경우 Cell의 포장구성 여부를 확인 하지 않습니다. 주의 하세요!!"
            }
        }

        /// <summary>
        /// 초기화
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnInit_Click(object sender, RoutedEventArgs e)
        {
            //SFU3440 초기화 하시겠습니까?	
            Util.MessageConfirm("SFU3440", (result) =>
            {
                if (result == MessageBoxResult.OK)
                {
                    AllClear();
                }

            });
        }

        /// <summary>
        /// 실적 확인
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnConfirm_Click(object sender, RoutedEventArgs e)
        {
            try
            {

                //// WO 선택 여부 확인
                //if (selWOData == null)
                //{
                //    // 작업 취소하고자 하는 LotID를 선택하신 후 진행하시길 바랍니다.
                //    LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("WorkOrder 선택하신 후 실적 확정을 하시길 바랍니다."), null, "Error", MessageBoxButton.OK, ControlsLibrary.MessageBoxIcon.None);
                //    return;
                //}

                // 리스트에 데이터가 있는지 확인
                if (dgScanInfo.GetRowCount() <= 0)
                {
                    //실적 확정할 데이터가 없습니다. 아래쪽 List를 확인하세요.
                    // LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("SFU3157"), null, "Error", MessageBoxButton.OK, ControlsLibrary.MessageBoxIcon.None);
                    Util.MessageValidation("SFU3157");
                    return;
                }

                if (string.IsNullOrEmpty(txtWorker.Text))
                {
                    //진행 중인 LOT은 확정처리할 수 없습니다.
                    //LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("SFU1843"), null, "Error", MessageBoxButton.OK, ControlsLibrary.MessageBoxIcon.None);
                    Util.MessageValidation("SFU1843");
                    return;
                }

                //특성데이터를 가져오는데 많이 시간이 소요됩니다. 계속 진행하시겠습니까?
                //    LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("SFU3175"), null, "Information", MessageBoxButton.OKCancel, MessageBoxIcon.None, (result) =>
                Util.MessageConfirm("SFU3175", (result) =>
                {
                    if (result == MessageBoxResult.OK)
                    {
                        PopUpConfirm();
                    }

                });
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }

        }

        /// <summary>
        /// 매거진 한건씩 처리 가능 한 함수 생성
        /// C20210409-000020 
        /// </summary>
        /// <param name="dt"></param>
        /// <returns></returns>
        private Boolean SelectFormationCellInfo(string sTrayMagazinge)
        {

            try
            {
                loadingIndicator.Visibility = Visibility.Visible;

                DataSet indataSet_chk = new DataSet();
                DataTable inDataTable_chk = indataSet_chk.Tables.Add("INDATA");
                inDataTable_chk.Columns.Add("SHOPID", typeof(string));
                inDataTable_chk.Columns.Add("AREAID", typeof(string));
                inDataTable_chk.Columns.Add("EQSGID", typeof(string));
                inDataTable_chk.Columns.Add("MDLLOT_ID", typeof(string));
                inDataTable_chk.Columns.Add("SUBLOT_CHK_SKIP_FLAG", typeof(string));
                inDataTable_chk.Columns.Add("INSP_SKIP_FLAG", typeof(string));
                inDataTable_chk.Columns.Add("2D_BCR_SKIP_FLAG", typeof(string));
                inDataTable_chk.Columns.Add("USERID", typeof(string));

                DataTable inDataMagazineTable_chk1 = indataSet_chk.Tables.Add("INDATA_MAGAZINE");
                inDataMagazineTable_chk1.Columns.Add("TRAY_MAGAZINE", typeof(string));


                DataRow inData_chk = inDataTable_chk.NewRow();
                inData_chk["SHOPID"] = sSHOPID;
                inData_chk["AREAID"] = sAREAID;
                inData_chk["EQSGID"] = Util.NVC(cboEquipmentSegment.SelectedValue);
                inData_chk["MDLLOT_ID"] = Util.NVC(cboModelLot.SelectedValue);
                inData_chk["SUBLOT_CHK_SKIP_FLAG"] = "N";
                inData_chk["INSP_SKIP_FLAG"] = "N";
                inData_chk["2D_BCR_SKIP_FLAG"] = "Y";
                inData_chk["USERID"] = txtWorker.Tag.ToString();
                inDataTable_chk.Rows.Add(inData_chk);

                DataRow dr = inDataMagazineTable_chk1.NewRow();
                dr["TRAY_MAGAZINE"] = Util.NVC(sTrayMagazinge);
                inDataMagazineTable_chk1.Rows.Add(dr);

                new ClientProxy().ExecuteServiceSync_Multi("BR_PRD_CHK_FCS_VALIDATION_MGZ", "INDATA,INDATA_MAGAZINE", "", indataSet_chk);

                return true;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return false;
            }
            finally
            {
                loadingIndicator.Visibility = Visibility.Collapsed;
            }

        }

        /// <summary>
        /// 매거진 ID 를 조회조건으로 특성치 정보 조회 함수 : 2011 04 12 홍광표S
        /// </summary>
        /// <param name="dt"></param>
        /// <returns></returns>
        private Boolean SelectFormationCellInfo(DataTable dt)
        {

            try
            {
                loadingIndicator.Visibility = Visibility.Visible;

                DataSet indataSet_chk = new DataSet();
                DataTable inDataTable_chk = indataSet_chk.Tables.Add("INDATA");
                inDataTable_chk.Columns.Add("SHOPID", typeof(string));
                inDataTable_chk.Columns.Add("AREAID", typeof(string));
                inDataTable_chk.Columns.Add("EQSGID", typeof(string));                
                inDataTable_chk.Columns.Add("MDLLOT_ID", typeof(string));
                inDataTable_chk.Columns.Add("SUBLOT_CHK_SKIP_FLAG", typeof(string));
                inDataTable_chk.Columns.Add("INSP_SKIP_FLAG", typeof(string));
                inDataTable_chk.Columns.Add("2D_BCR_SKIP_FLAG", typeof(string));
                inDataTable_chk.Columns.Add("USERID", typeof(string));

                DataTable inDataMagazineTable_chk1 = indataSet_chk.Tables.Add("INDATA_MAGAZINE");
                inDataMagazineTable_chk1.Columns.Add("TRAY_MAGAZINE", typeof(string));


                DataRow inData_chk = inDataTable_chk.NewRow();
                inData_chk["SHOPID"] = sSHOPID;
                inData_chk["AREAID"] = sAREAID;
                inData_chk["EQSGID"] = Util.NVC(cboEquipmentSegment.SelectedValue);
                inData_chk["MDLLOT_ID"] = Util.NVC(cboModelLot.SelectedValue);
                inData_chk["SUBLOT_CHK_SKIP_FLAG"] = "N";
                inData_chk["INSP_SKIP_FLAG"] = "N";
                inData_chk["2D_BCR_SKIP_FLAG"] = "Y";
                inData_chk["USERID"] = txtWorker.Tag.ToString();
                inDataTable_chk.Rows.Add(inData_chk);

                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    DataRow dr = inDataMagazineTable_chk1.NewRow();
                    dr["TRAY_MAGAZINE"] = Util.NVC(dt.Rows[i]["TRAY_MAGAZINE"]);
                    //dr["I_PACKING_CHECK"] = Util.NVC(dt.Rows[i]["CHECK"]);
                    inDataMagazineTable_chk1.Rows.Add(dr);
                }

                new ClientProxy().ExecuteServiceSync_Multi("BR_PRD_CHK_FCS_VALIDATION_MGZ", "INDATA,INDATA_MAGAZINE", "", indataSet_chk);

                //DataTable dtRslt = null;
                //if ( sAREAID.Equals("A1"))
                //{
                //    dtRslt = new ClientProxy2007("AF1").ExecuteServiceSync("SET_GMES_MAGAZINE_LOT", "INDATA", "", RQSTDT);
                //    return true;
                //}
                //else if (sAREAID.Equals("A2") || sAREAID.Equals("S2"))
                //{
                //    dtRslt = new ClientProxy2007("AF2").ExecuteServiceSync("SET_GMES_MAGAZINE_LOT", "INDATA", "", RQSTDT);
                //    return true;
                //}
                //else
                //{
                //    return false;
                //}
                return true;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return false;
            }
            finally
            {
                loadingIndicator.Visibility = Visibility.Collapsed;
            }

        }


        private void PopUpConfirm()
        {

            //// 총 셀 수량
            int totalqty = 0;
            string sCheck = chkPack.IsChecked == true ? "Y" : "N";
            Boolean result = false;
            //chkPack.Checked = false;
            //chkPack.Visible = false;

            // 매거진 저장할 데이터테이블
            DataTable lsDataTable = new DataTable();
            lsDataTable.Columns.Add("TRAY_MAGAZINE", typeof(string));
            lsDataTable.Columns.Add("QTY", typeof(string));
            lsDataTable.Columns.Add("CHECK", typeof(string));

            for (int i = 0; i < dgScanInfo.GetRowCount(); i++)
            {
                DataRow row = lsDataTable.NewRow();
                row["TRAY_MAGAZINE"] = Util.NVC(dgScanInfo.GetCell(i, dgScanInfo.Columns["TRAY_MAGAZINE"].Index).Value);
                row["QTY"] = Util.NVC(dgScanInfo.GetCell(i, dgScanInfo.Columns["QTY"].Index).Value);
                row["CHECK"] = sCheck;                                  //2011.12.22 Add By Airman 
                lsDataTable.Rows.Add(row);

                totalqty = totalqty + Util.NVC_Int(dgScanInfo.GetCell(i, dgScanInfo.Columns["QTY"].Index).Value);

                // 매거진 ID를 조회조건으로 하여 [특성치 정보 조회 함수] 호출 : 문제가 없으면 아래 로직 수행
                // C20210409-000020 선능 문제로 한개의 매거진만 처리 하도록 변경
                result = SelectFormationCellInfo(Util.NVC(dgScanInfo.GetCell(i, dgScanInfo.Columns["TRAY_MAGAZINE"].Index).Value));
                if (!result)
                {
                    return;
                }
            }

            // 매거진 ID를 조회조건으로 하여 [특성치 정보 조회 함수] 호출 : 문제가 없으면 아래 로직 수행
            //C20210409-000020 성능 문재로 주석 처리함.(건씩 처리 하도록 변경)
            //Boolean result = SelectFormationCellInfo(lsDataTable);

            if (result)
            {
                // 폼 화면에 보여주는 메서드에 5개의 매개변수 전달
                BOX001_006_CONFIRM popUp = new BOX001_006_CONFIRM();
                popUp.FrameOperation = this.FrameOperation;

                //// 팝업창을 띄우고, 내가 처리하고자 하는 함수 번호를 2로 지정/MagazineID_수량 DataTable 객체 넘김
                //frmPopupPalletEnd.Show(this,
                //                       2,
                //                       lsDataTable,
                //                       data,
                //                       txtHModelID.Text.Trim(),               // 모델 ID
                //                       txtProdid.Text.Trim(),                 // 제품 ID
                //                       cboLine.SelectedValue.ToString(),      // LIne ID
                //                       totalqty.ToString());                  // 제품 수량

                if (popUp != null)
                {
                    object[] Parameters = new object[13];
                    Parameters[0] = lsDataTable;
                    Parameters[1] = Util.NVC(cboEquipmentSegment.SelectedValue);
                    Parameters[2] = Util.NVC(cboEquipmentSegment.Text);
                    Parameters[3] = txtProdID.Text.Trim();
                    Parameters[4] = Util.NVC(cboModelLot.SelectedValue);       // 모델 ID
                    Parameters[5] = totalqty.ToString();                    // 제품 수량
                    Parameters[6] = chkPack.IsChecked == true ? "Y" : "N";  //(*)포장검사 Skip 여부
                    Parameters[7] = ""; // txtWO.Text.Trim();
                    Parameters[8] = sWO_DETL_ID;
                    Parameters[9] = sSHOPID;
                    Parameters[10] = sAREAID;
                    Parameters[11] = txtWorker.Text;
                    Parameters[12] = txtWorker.Tag as string;


                    C1WindowExtension.SetParameters(popUp, Parameters);

                    popUp.Closed += new EventHandler(wndConfirm_Closed);
                    //this.Dispatcher.BeginInvoke(new Action(() => popUp.ShowModal()));
                    grdMain.Children.Add(popUp);
                    popUp.BringToFront();
                }

            }
        }

        private void wndConfirm_Closed(object sender, EventArgs e)
        {
            BOX001.BOX001_006_CONFIRM wndPopup = sender as BOX001.BOX001_006_CONFIRM;
            if (wndPopup.DialogResult == MessageBoxResult.OK)
            {

                // 초기화 함수 호출
                //AllClear();
                CompleteClear();

                DataTable dtPalletHisCard = wndPopup.retDT;

                if (dtPalletHisCard != null)
                {
                    // 태그 발행 창 화면에 띄움.
                    object[] Parameters = new object[5];
                    Parameters[0] = "PalletHis_Tag"; // "PalletHis_Tag";
                    Parameters[1] = dtPalletHisCard;
                    Parameters[2] = "2";
                    Parameters[4] = sSHOPID;

                    //LGC.GMES.MES.BOX001.Report rs = new LGC.GMES.MES.BOX001.Report();
                    //C1WindowExtension.SetParameters(rs, Parameters);
                    //rs.Show();
                    LGC.GMES.MES.BOX001.Report_Pallet_Hist rs = new LGC.GMES.MES.BOX001.Report_Pallet_Hist();
                    C1WindowExtension.SetParameters(rs, Parameters);
                    this.Dispatcher.BeginInvoke(new Action(() => rs.ShowModal()));

                    //try
                    //{
                    //    DataTable RQSTDT = new DataTable();
                    //    RQSTDT.Columns.Add("I_MAGID", typeof(string));

                    //    // 스프레드에 있는 값과 동일한 값이 입력되었다면 return됨.
                    //    for (int i = 0; i < dgScanInfo.GetRowCount(); i++)
                    //    {
                    //        DataRow dr = RQSTDT.NewRow();
                    //        dr["I_MAGID"] = Util.NVC(dgScanInfo.GetCell(i, dgScanInfo.Columns["TRAY_MAGAZINE"].Index).Value);
                    //        RQSTDT.Rows.Add(dr);
                    //    }

                    //    DataTable dtRslt = null;
                    //    if (sAREAID.Equals("A1"))
                    //    {
                    //        dtRslt = new ClientProxy2007("AF1").ExecuteServiceSync("SET_GMES_MAGAZINE_LOT", "INDATA", "OUTDATA", RQSTDT);
                    //    }
                    //    else if (sAREAID.Equals("A2") || sAREAID.Equals("S2"))
                    //    {
                    //        dtRslt = new ClientProxy2007("AF2").ExecuteServiceSync("SET_GMES_MAGAZINE_LOT", "INDATA", "OUTDATA", RQSTDT);
                    //    }
                    //    else
                    //    {
                    //    }
                    //}
                    //catch(Exception ex)
                    //{

                    //}
                }
            }
            grdMain.Children.Remove(wndPopup);
        }

        void popup_Closed(object sender, EventArgs e)
        {

        }

        private void dgScanInfo_SelectionChanged(object sender, C1.WPF.DataGrid.DataGridSelectionChangedEventArgs e)
        {
            if (dgScanInfo.CurrentRow == null || dgScanInfo.CurrentRow.Index < 0 || dgScanInfo.SelectedIndex < 0)
            {
                return;
            }

            if(dgScanInfo.GetRowCount() > 0 )
            {
                String sMagazineID = Util.NVC(DataTableConverter.GetValue(dgScanInfo.Rows[dgScanInfo.SelectedIndex].DataItem, "TRAY_MAGAZINE"));
                // MagazineID 를 조회조건으로 하여 셀정보 조회하는 함수 호출.
                SelectCellInformation(sMagazineID);
            }
        }

        private void dgScanInfo_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (dgScanInfo.CurrentRow == null || dgScanInfo.SelectedIndex == -1)
            {
                return;
            }

            try
            {
                if (e.ChangedButton.ToString().Equals("Left") && dgScanInfo.CurrentColumn.Name == "CHK")
                {
                    if (Util.NVC(dgScanInfo.GetCell(dgScanInfo.CurrentRow.Index, dgScanInfo.Columns["CHK"].Index).Value) == "True")
                    {
                        DataTableConverter.SetValue(dgScanInfo.Rows[dgScanInfo.CurrentRow.Index].DataItem, "CHK", false);
                    }
                    else
                    {
                        DataTableConverter.SetValue(dgScanInfo.Rows[dgScanInfo.CurrentRow.Index].DataItem, "CHK", true);
                    }
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
            finally
            {
                dgScanInfo.CurrentRow = null;
            }

        }


        private void dgScanInfo_LoadedColumnHeaderPresenter(object sender, C1.WPF.DataGrid.DataGridColumnEventArgs e)
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

        void checkAll_Checked(object sender, RoutedEventArgs e)
        {
            for (int idx = 0; idx < dgScanInfo.GetRowCount(); idx++)
            {
                C1.WPF.DataGrid.DataGridRow row = dgScanInfo.Rows[idx];
                DataTableConverter.SetValue(row.DataItem, "CHK", true);
            }
        }

        void checkAll_Unchecked(object sender, RoutedEventArgs e)
        {
            for (int idx = 0; idx < dgScanInfo.GetRowCount(); idx++)
            {
                C1.WPF.DataGrid.DataGridRow row = dgScanInfo.Rows[idx];
                DataTableConverter.SetValue(row.DataItem, "CHK", false);
            }
        }

        private void cboProduct_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            AllClear();
        }

        #endregion


        #region Mehod     


        public void AllClear()
        {
            dgScanInfo.ItemsSource = null;
            dgCELLInfo.ItemsSource = null;

            sWO_DETL_ID = string.Empty;
            //txtWO.Text = string.Empty;

            string sProduct = Util.NVC(cboProduct.SelectedValue);
            if (sProduct == "" || sProduct == "SELECT")
            {
                txtProdID.Text = string.Empty;
                //txtProdName.Text = string.Empty;
            }
            else
            {
                txtProdID.Text = sProduct;
                //txtProdName.Text = cboProduct.Text;
            }
            
            
            //if (cboModel.Items.Count > 0) cboModel.SelectedIndex = 0;

            txtMagazineID.Text = string.Empty;
            txtScanqty.Text = string.Empty;
            chkPack.IsChecked = false;

            // DataTable 초기화
            isDataTable.Clear();

            // 스캔 카운트 초기화
            isScanQty = 0;
            txtCellCount.Text = "0";

        }

        public void CompleteClear()
        {
            dgScanInfo.ItemsSource = null;
            dgCELLInfo.ItemsSource = null;

            sWO_DETL_ID = string.Empty;
            //txtWO.Text = string.Empty;

            txtMagazineID.Text = string.Empty;
            txtScanqty.Text = string.Empty;

            // DataTable 초기화
            isDataTable.Clear();

            // 스캔 카운트 초기화
            isScanQty = 0;
            txtCellCount.Text = "0";

        }

        /// <summary>
        /// Magazine 정보 조회
        /// </summary>
        private void Search2Magazine()
        {

            // QR_MAGAZINE_INFO
            try
            {
                //DataTable RQSTDT = new DataTable();
                //RQSTDT.Columns.Add("MAGAZINE_ID", typeof(string));
                //RQSTDT.Columns.Add("PRODID", typeof(string));

                //DataRow dr = RQSTDT.NewRow();
                //dr["MAGAZINE_ID"] = txtMagazineID.Text.Trim();
                //dr["PRODID"] = txtProdID.Text.Trim();
                //RQSTDT.Rows.Add(dr);

                //DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_EQP_SEL_EQPTLOSS_CHK_MAN", "RQSTDT", "RSLTDT", RQSTDT);

                string sModelID = cboModelLot.SelectedValue.ToString();
                string sLineID = cboEquipmentSegment.SelectedValue.ToString();

                string sArea = string.Empty;

                if (cboArea.SelectedIndex < 0 || cboArea.SelectedValue.ToString().Trim().Equals("SELECT"))
                {
                    //동 정보를 선택하세요.
                    //LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("SFU1499"), null, "Info", MessageBoxButton.OK, MessageBoxIcon.Warning);
                    Util.MessageValidation("SFU1499");
                    return;
                }
                else
                {
                    sArea = cboArea.SelectedValue.ToString();
                }


                DataSet indataSet_chk = new DataSet();
                DataTable inDataTable_chk = indataSet_chk.Tables.Add("INDATA");
                inDataTable_chk.Columns.Add("AREAID", typeof(string));
                inDataTable_chk.Columns.Add("EQSGID", typeof(string));
                inDataTable_chk.Columns.Add("SUBLOT_TYPE", typeof(string));
                inDataTable_chk.Columns.Add("MDLLOT_ID", typeof(string));
                inDataTable_chk.Columns.Add("USERID", typeof(string));

                DataTable inDataCellTable_chk1 = indataSet_chk.Tables.Add("INSUBLOT");
                inDataCellTable_chk1.Columns.Add("SUBLOTID", typeof(string));

                DataTable inDataCellTable_chk = indataSet_chk.Tables.Add("INMAGID");
                inDataCellTable_chk.Columns.Add("MAGAZINE_ID", typeof(string));



                DataRow inData_chk = inDataTable_chk.NewRow();
                inData_chk["AREAID"] = sAREAID;
                inData_chk["EQSGID"] = sLineID;
                inData_chk["SUBLOT_TYPE"] = "MGZ";
                inData_chk["MDLLOT_ID"] = sModelID;
                inData_chk["USERID"] = string.IsNullOrEmpty(txtWorker.Tag.ToString()) ? LoginInfo.USERID : txtWorker.Tag.ToString();
                inDataTable_chk.Rows.Add(inData_chk);

                DataRow inDataCell_chk = inDataCellTable_chk.NewRow();
                inDataCell_chk["MAGAZINE_ID"] = txtMagazineID.Text.ToString(); 
                inDataCellTable_chk.Rows.Add(inDataCell_chk);

                new ClientProxy().ExecuteServiceSync_Multi("BR_PRD_CHK_SUBLOT", "INDATA,INSUBLOT,INMAGID", "", indataSet_chk);


                DataSet indataSet = new DataSet();
                DataTable inData = indataSet.Tables.Add("INDATA_ROUTE");
                inData.Columns.Add("AREAID", typeof(string));

                DataRow row = inData.NewRow();
                row["AREAID"] = sAREAID;

                indataSet.Tables["INDATA_ROUTE"].Rows.Add(row);

                DataTable inPallet = indataSet.Tables.Add("INDATA");
                inPallet.Columns.Add("MAGAZINE_ID", typeof(string));
                inPallet.Columns.Add("PRODID", typeof(string));
                
                DataRow row2 = inPallet.NewRow();
                row2["MAGAZINE_ID"] = txtMagazineID.Text.ToString();
                row2["PRODID"] = txtProdID.Text.ToString();

                indataSet.Tables["INDATA"].Rows.Add(row2);

                DataSet dsRslt = new ClientProxy().ExecuteServiceSync_Multi("BR_INF_SEL_MAGAZINE_INFO", "INDATA_ROUTE,INDATA", "OUTDATA", indataSet);


                if (dsRslt.Tables["OUTDATA"].Rows.Count == 0)
                {
                    Util.WarningPlayer();

                    //조회된 Data가 없습니다.
                   // LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("SFU1905"), null, "Information", MessageBoxButton.OK, MessageBoxIcon.None, (result) =>
                   Util.MessageInfo("SFU1905", (result) =>
                   {
                        if (result == MessageBoxResult.OK)
                        {
                            txtMagazineID.Focus();
                            txtMagazineID.SelectAll();
                        }
                    });
                    return;
                }
                else
                {
                    DataRow dRow = isDataTable.NewRow();

                    dRow["SEQ_NO"] = dgScanInfo.GetRowCount() + 1;
                    dRow["TRAY_MAGAZINE"] = dsRslt.Tables["OUTDATA"].Rows[0]["MAGAZINE_ID"].ToString();
                    dRow["PRODID"] = dsRslt.Tables["OUTDATA"].Rows[0]["PRODID"].ToString();
                    dRow["QTY"] = dsRslt.Tables["OUTDATA"].Rows[0]["QTY"].ToString();
                    dRow["LOTID"] = dsRslt.Tables["OUTDATA"].Rows[0]["LOTID"].ToString();
                    dRow["MDF_TIME"] = dsRslt.Tables["OUTDATA"].Rows[0]["MDF_TIME"].ToString();
                    isDataTable.Rows.Add(dRow);

                    //dgScanInfo.ItemsSource = DataTableConverter.Convert(isDataTable);
                    Util.GridSetData(dgScanInfo, isDataTable, FrameOperation);

                    int Cell_Cnt = 0;
                    for (int i = 0; i < dgScanInfo.GetRowCount(); i++)
                    {
                        //row = sprScanInfo.ActiveSheet.Rows[i];
                        //size = row.GetPreferredHeight();
                        //row.Height = size + 15;
                        Cell_Cnt += Util.NVC_Int(dgScanInfo.GetCell(i, dgScanInfo.Columns["QTY"].Index).Value);
                    }

                    isScanQty = isScanQty + 1;
                    txtScanqty.Text = isScanQty.ToString();
                    txtCellCount.Text = Cell_Cnt.ToString();
                 
                    // 정상적인 경우 다음 매거진 입력할 수 있게 전체 선택.
                    txtMagazineID.Focus();
                    txtMagazineID.SelectAll();

                    //try
                    //{
                    //    DataTable RQSTDT = new DataTable();
                    //    RQSTDT.Columns.Add("I_MAGID", typeof(string));

                        //// 스프레드에 있는 값과 동일한 값이 입력되었다면 return됨.
                        //for (int i = 0; i < dgScanInfo.GetRowCount(); i++)
                        //{
                        //    DataRow dr = RQSTDT.NewRow();
                        //    dr["I_MAGID"] = Util.NVC(dgScanInfo.GetCell(i, dgScanInfo.Columns["TRAY_MAGAZINE"].Index).Value);
                        //    RQSTDT.Rows.Add(dr);
                        //}

                        //if (sAREAID.Equals("A1"))
                        //{
                        //    //new ClientProxy2007("AF1").ExecuteServiceSync("SET_GMES_MAGAZINE_LOT", "INDATA", "", RQSTDT);

                        //    new ClientProxy2007("AF1").ExecuteService("SET_GMES_MAGAZINE_LOT", "INDATA", "", RQSTDT, (dtResult, ex) =>
                        //    {
                        //        if (ex != null)
                        //            Util.MessageException(ex);
                        //    });


                        //}
                        //else if (sAREAID.Equals("A2") || sAREAID.Equals("S2"))
                        //{
                        //    //new ClientProxy2007("AF2").ExecuteServiceSync("SET_GMES_MAGAZINE_LOT", "INDATA", "", RQSTDT);
                        //    new ClientProxy2007("AF2").ExecuteService("SET_GMES_MAGAZINE_LOT", "INDATA", "", RQSTDT, (dtResult, ex) =>
                        //    {
                        //        if (ex != null)
                        //            Util.MessageException(ex);
                        //    });
                        //}
                        //else
                        //{
                    //    //}
                    //}
                    //catch (Exception ex)
                    //{

                    //}
                }
            }
            catch (Exception ex)
            {
                Util.WarningPlayer();
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex.Message), null, "Information", MessageBoxButton.OK, MessageBoxIcon.None, (result) =>
                {
                    if (result == MessageBoxResult.OK)
                    {
                        txtMagazineID.Focus();
                        txtMagazineID.SelectAll();
                    }
                });

                //// test.........
                //DataRow dRow = isDataTable.NewRow();

                //dRow["CHK"] = 0;
                //dRow["SEQ_NO"] = dgScanInfo.GetRowCount() + 1;
                //dRow["TRAY_MAGAZINE"] = txtMagazineID.Text.Trim();
                //dRow["PRODID"] = txtProdID.Text.Trim();
                //dRow["QTY"] = "2";
                //dRow["LOTID"] = "LOTID";
                //dRow["MDF_TIME"] = DateTime.Now.ToString("yyyy-MM-dd hh:MM:ss");
                //isDataTable.Rows.Add(dRow);

                //dgScanInfo.ItemsSource = DataTableConverter.Convert(isDataTable);

                //int Cell_Cnt = 0;
                //for (int i = 0; i < dgScanInfo.GetRowCount(); i++)
                //{
                //    Cell_Cnt += Util.NVC_Int(dgScanInfo.GetCell(i, dgScanInfo.Columns["QTY"].Index).Value);
                //}

                //isScanQty = isScanQty + 1;
                //txtScanqty.Text = isScanQty.ToString();
                //txtCellCount.Text = Cell_Cnt.ToString();
                // 정상적인 경우 다음 매거진 입력할 수 있게 전체 선택.
                //txtMagazineID.Focus();
                //txtMagazineID.SelectAll();
            }
           
        }

        /// <summary>
        /// 매거진ID를 조회조건으로 하여 셀정보 조회하는 함수
        /// </summary>
        /// <param name="magazineID"></param>
        private void SelectCellInformation(string magazineID)
        {
            // BizData data = new BizData("QR_CELLID_BY_MAGID", "RSLTDT");
            try
            {
                //DataTable RQSTDT = new DataTable();
                //RQSTDT.TableName = "RQSTDT";
                //RQSTDT.Columns.Add("I_MAGID", typeof(string));

                //DataRow dr = RQSTDT.NewRow();
                //dr["I_MAGID"] = magazineID;
                //RQSTDT.Rows.Add(dr);

                //DataTable dtResult = new ClientProxy().ExecuteServiceSync("BR_INF_SEL_CELL_INFO_BY_MGZ", "RQSTDT", "RSLTDT", RQSTDT);
                //dgCELLInfo.ItemsSource = DataTableConverter.Convert(dtResult);


                string sArea = string.Empty;

                if (cboArea.SelectedIndex < 0 || cboArea.SelectedValue.ToString().Trim().Equals("SELECT"))
                {
                    //동 정보를 선택하세요.
                    //  LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("SFU1499"), null, "Info", MessageBoxButton.OK, MessageBoxIcon.Warning);
                    Util.MessageValidation("SFU1499");
                    return;
                }
                else
                {
                    sArea = cboArea.SelectedValue.ToString();
                }

                DataSet indataSet = new DataSet();
                DataTable inData = indataSet.Tables.Add("INDATA_ROUTE");
                inData.Columns.Add("AREAID", typeof(string));

                DataRow row = inData.NewRow();
                row["AREAID"] = sAREAID;

                indataSet.Tables["INDATA_ROUTE"].Rows.Add(row);

                DataTable inPallet = indataSet.Tables.Add("INDATA");
                inPallet.Columns.Add("MAGAZINE_ID", typeof(string));

                DataRow row2 = inPallet.NewRow();
                row2["MAGAZINE_ID"] = magazineID;

                indataSet.Tables["INDATA"].Rows.Add(row2);

                DataSet dsRslt = new ClientProxy().ExecuteServiceSync_Multi("BR_INF_SEL_CELL_INFO_BY_MGZ", "INDATA_ROUTE,INDATA", "OUTDATA", indataSet);

                //dgCELLInfo.ItemsSource = DataTableConverter.Convert(dsRslt.Tables["OUTDATA"]);
                
                DataTable dtCellTable = new DataTable();
                dtCellTable.Columns.Add("SEQ_NO", typeof(int));
                dtCellTable.Columns.Add("CELLID", typeof(string));
                dtCellTable.Columns.Add("LOTID", typeof(string));

                for (int i = 0; i < dsRslt.Tables["OUTDATA"].Rows.Count; i++)
                {
                    DataRow dRow = dtCellTable.NewRow();

                    dRow["SEQ_NO"] = i+1;//dgCELLInfo.GetRowCount() + 1;
                    dRow["CELLID"] = dsRslt.Tables["OUTDATA"].Rows[i]["CELL_ID"].ToString();
                    dRow["LOTID"] = dsRslt.Tables["OUTDATA"].Rows[i]["LOT_ID"].ToString();
                    dtCellTable.Rows.Add(dRow);
                }

                //dgCELLInfo.ItemsSource = DataTableConverter.Convert(dtCellTable);
                Util.GridSetData(dgCELLInfo, dtCellTable, FrameOperation);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
               // LGC.GMES.MES.ControlsLibrary.MessageBox.Show(ex.Message, null, "Error", MessageBoxButton.OK, ControlsLibrary.MessageBoxIcon.None);
            }

        }



        #endregion

        private void cboArea_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            string sTemp = Util.NVC(cboArea.SelectedValue);
            if (sTemp == "" || sTemp == "SELECT")
            {
                sAREAID = "";
                sSHOPID = "";
            }
            else
            {
                string[] sArry = sTemp.Split('^');
                sAREAID = sArry[0];
                sSHOPID = sArry[1];
            }

            String[] sFilter = { sAREAID };    // Area
            _combo.SetCombo(cboEquipmentSegment, CommonCombo.ComboStatus.SELECT, sFilter: sFilter, sCase: "LINE_CP");

        }

        private void cboEquipmentSegment_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            C1ComboBox[] cboParent = { cboEquipmentSegment };
            _combo.SetCombo(cboModelLot, CommonCombo.ComboStatus.SELECT, cbParent: cboParent);

        }

        private void cboModelLot_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {

            string sModelLot = Util.NVC(cboModelLot.SelectedValue);
            if (sModelLot == "" || sModelLot == "SELECT")
            {
                sModelLot = "";
            }

            //C1ComboBox[] cboParent = { cboEquipmentSegment, cboModelLot };
            //_combo.SetCombo(cboProduct, CommonCombo.ComboStatus.SELECT, cbParent: cboParent, sCase: "PROD_MDL");

            C1ComboBox[] cboParent = { cboEquipmentSegment, cboModelLot };
            String[] sFilter = { sModelLot };
            _combo.SetCombo(cboProduct, CommonCombo.ComboStatus.NONE, cbParent: cboParent, sFilter: sFilter, sCase: "PROD_MDL");


            //String[] sFilter = { sEqsgid, Process.CELL_BOXING };
            //_combo.SetCombo(cboEquipment, CommonCombo.ComboStatus.NONE, sFilter: sFilter);
        }

        private void btnShift_Click(object sender, RoutedEventArgs e)
        {
            GMES.MES.CMM001.Popup.CMM_SHIFT_USER wndPopup = new GMES.MES.CMM001.Popup.CMM_SHIFT_USER();
            wndPopup.FrameOperation = FrameOperation;

            if (wndPopup != null)
            {
                object[] Parameters = new object[5];
                Parameters[0] = LoginInfo.CFG_SHOP_ID;
                Parameters[1] = LoginInfo.CFG_AREA_ID;
                Parameters[2] = LoginInfo.CFG_EQPT_ID;
                Parameters[3] = Process.CELL_BOXING; // LoginInfo.CFG_PROC_ID;
                Parameters[4] = "";
                C1WindowExtension.SetParameters(wndPopup, Parameters);

                wndPopup.Closed += new EventHandler(wndShiftUser_Closed);
                grdMain.Children.Add(wndPopup);
                wndPopup.BringToFront();
            }
        }
        private void wndShiftUser_Closed(object sender, EventArgs e)
        {
            GMES.MES.CMM001.Popup.CMM_SHIFT_USER window = sender as GMES.MES.CMM001.Popup.CMM_SHIFT_USER;
            if (window.DialogResult == MessageBoxResult.OK)
            {
                txtWorker.Text = window.USERNAME;
                txtWorker.Tag = window.USERID;
            }
            grdMain.Children.Remove(window);
        }
    }
}
