/*************************************************************************************
 Created Date : 2020.03.24
      Creator : 
   Decription : 21700 Tesla 자동포장기 팔레트 생성화면
--------------------------------------------------------------------------------------
 [Change History]
  날짜        버젼  수정자   CSR              내용
 -------------------------------------------------------------------------------------
 2020.03.24  0.1   이현호       21700 자동포장기 팔레트 생성화면 신규화면 개발.


**************************************************************************************/

using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Data;

using C1.WPF;
using C1.WPF.DataGrid;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.ControlsLibrary;
using System.ComponentModel;
using System.Threading;
using System.Linq;
using C1.WPF.DataGrid.Summaries;

namespace LGC.GMES.MES.BOX001
{
    /// <summary>
    /// BOX001_201_RUNSTART.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class BOX001_242_CREATEPALLET : C1Window, IWorkArea
    {
        Util _util = new Util();

        string _AREAID = string.Empty;
        string sUSERID = string.Empty;
        string sSHFTID = string.Empty;
        private string _processCode;
        private string _processName;
        private bool _load = true;

        public IFrameOperation FrameOperation
        {
            get;
            set;
        }

        public string PALLET_ID
        {
            get;
            set;
        }

        public string ProcessCode
        {
            get { return _processCode; }
            set
            {
                _processCode = value;
            }
        }
        public string ProcessName
        {
            get { return _processName; }
            set
            {
                _processName = value;
            }
        }

        #region Initialize
        public BOX001_242_CREATEPALLET()
        {
            InitializeComponent();
        }

        private void C1Window_Loaded(object sender, RoutedEventArgs e)
        {
            _processCode = Process.CircularCharacteristicGrader;

            object[] tmps = C1WindowExtension.GetParameters(this);
            _AREAID = Util.NVC(tmps[0]);
            sUSERID = Util.NVC(tmps[1]);
            sSHFTID = Util.NVC(tmps[2]);

            if (_load)
            {
                InitCombo();
                InitControl();
            }
        }

        /// <summary>
        /// 콤보박스 초기 설정
        /// </summary>
        private void InitCombo()
        {
            CommonCombo _combo = new CommonCombo();

            //출하처 콤보 Set
            SetEquipmentCombo(cboEquipment);

            ////Inbox type
            string[] sFilter = { LoginInfo.CFG_AREA_ID, Process.CircularCharacteristicGrader };
            _combo.SetCombo(cboInboxType, CommonCombo.ComboStatus.SELECT, sFilter: sFilter);

            SetEqptInboxType();

            cboEquipment.SelectedValueChanged += cboEquipment_SelectedValueChanged;
        }

        /// <summary>
        /// 컨트롤 초기값 설정
        /// </summary>
        private void InitControl()
        {
            object[] tmps = C1WindowExtension.GetParameters(this);

        }
        #endregion

        #region [EVENT]

        #region 텍스트박스 포커스 : text_GotFocus()
        private void text_GotFocus(object sender, RoutedEventArgs e)
        {
            InputMethod.Current.ImeConversionMode = ImeConversionModeValues.Alphanumeric;
        }
        #endregion

        #region Pallet 생성 : btnCreate_Click()
        private void btnCreate_Click(object sender, RoutedEventArgs e)
        {
            if (dgInPallet.Rows.Count == 1)
            {
                //Outbox 정보가 없습니다
                Util.MessageValidation("SFU5010");
                return;
            }

            if (cboEquipment.SelectedIndex < 0 || cboEquipment.SelectedValue.Equals("SELECT"))
            {
                //Util.Alert("설비를 선택 하세요.");
                Util.MessageValidation("SFU1673");
                return;
            }


            if (cboInboxType.SelectedIndex < 0 || cboInboxType.SelectedValue.Equals("SELECT"))
            {
                // Inbox 유형을 선택해주세요.
                Util.MessageValidation("SFU4005");
                return;
            }


            DataTable dt = DataTableConverter.Convert(dgInPallet.ItemsSource);

            DataRow[] dr = dt.Select("OUT_BOX_QTY <> ISNULL(INBOX_QTY, 0) OR OUT_BOX_QTY <> ISNULL(SUBLOT_QTY, 0)");

            // Outbox 수량과 Inbox 수량이 다른 Box가 존재하는 경우
            if(dr.Length > 0)
            {
                //수량이 변경 된 Box가 존재합니다. 그래도 Pallet 생성하시겠습니까?
                Util.MessageConfirm("SFU8353", (result) =>
                {
                    if (result == MessageBoxResult.OK)
                    {
                        RunStart();
                    }
                });
            }
            // Outbox 수량과 Inbox 수량이 모두 일치하는 경우.
            else
            {
                //Pallet 생성하시겠습니까?
                Util.MessageConfirm("SFU5009", (result) =>
                {
                    if (result == MessageBoxResult.OK)
                    {
                        RunStart();
                    }
                });
            }
        }
        #endregion

        #region 닫기 : btnClose_Click()
        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = MessageBoxResult.Cancel;
        }

        #endregion

        #region 생성 팔레트 대상 조회: txtInPalletID_KeyDown()
        private void txtInPalletID_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                try
                {
                    ClearProductLot();

                    DataTable inDataSet = new DataTable();

                    inDataSet.TableName = "INDATA";

                    inDataSet.Columns.Add("LANGID");
                    inDataSet.Columns.Add("PALLETID");
                    inDataSet.Columns.Add("SHOPID");
                    inDataSet.Columns.Add("AREAID");

                    DataRow newRow = inDataSet.NewRow();
                    newRow["LANGID"] = LoginInfo.LANGID;
                    newRow["PALLETID"] = txtInPalletID.Text;
                    newRow["SHOPID"] = LoginInfo.CFG_SHOP_ID;
                    newRow["AREAID"] = LoginInfo.CFG_AREA_ID;
                    inDataSet.Rows.Add(newRow);

                    DataTable dtResult = new ClientProxy().ExecuteServiceSync("BR_PRD_GET_AUTOBOXING_WORK_LIST", "INDATA", "OUTDATA", inDataSet);

                    DataTable dtMerge = new DataTable();

                    for (int i = 0; i < dtResult.Rows.Count; i++)
                    {
                        DataTable RQSTDT = new DataTable();

                        RQSTDT.TableName = "RQSTDT";
                        RQSTDT.Columns.Add("BOXID");

                        DataRow newRow1 = RQSTDT.NewRow();
                        newRow1["BOXID"] = dtResult.Rows[i]["OUT_BOX_ID"].ToString().Trim();
                        RQSTDT.Rows.Add(newRow1);

                        DataTable dtResult1 = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_INBOX_QTY_BY_OUTBOX_FM", "RQSTDT", "RSLTDT", RQSTDT);

                        if (dtResult1.Rows.Count > 0)
                        {
                            dtMerge.Merge(dtResult1);
                        }
                    }

                    if (dtMerge.Rows.Count > 0)
                    {
                        DataColumn[] keys = new DataColumn[1];
                        keys[0] = dtResult.Columns["OUT_BOX_ID"];
                        dtResult.PrimaryKey = keys;

                        dtResult.Merge(dtMerge);
                    }
                    else
                    {
                        dtResult.Columns.Add("INBOX_QTY", typeof(int));
                        dtResult.Columns.Add("SUBLOT_QTY", typeof(int));
                    }

                    Util.GridSetData(dgInPallet, dtResult, FrameOperation, false);

                    cboInboxType.SelectedValue = dtResult.Rows[0]["INBOX_TYPE_CD"]; 

                    if (string.IsNullOrEmpty(Util.NVC(dtResult.Rows[0]["INBOX_TYPE_CD"])))
                    {
                        cboInboxType.IsEnabled = true;
                    }
                    else
                    {
                        cboInboxType.IsEnabled = false;
                    }

                    GetProductLot(Util.NVC(dtResult.Rows[0]["LOT_ID"]), Util.NVC(cboEquipment.SelectedValue));
                }
                catch (Exception ex)
                {
                    Util.MessageException(ex);
                }
                finally
                {
                    txtInPalletID.Text = string.Empty;
                }
            }
        }

        public void GetProductLot(string pAssyLot = null, string pEqptID = null)
        {
            try
            {
                if (string.IsNullOrEmpty(pAssyLot))
                {
                    return;
                }

                if (string.IsNullOrEmpty(pEqptID) || pEqptID == "SELECT")
                {
                    return;
                }

                ShowLoadingIndicator();

                DataTable inTable = new DataTable();
                inTable.Columns.Add("LANGID", typeof(string));
                inTable.Columns.Add("EQSGID", typeof(string));
                inTable.Columns.Add("EQPTID", typeof(string));
                inTable.Columns.Add("WIPSTAT", typeof(string));
                inTable.Columns.Add("PROCID", typeof(string));
                inTable.Columns.Add("WIPTYPECODE", typeof(string));
                inTable.Columns.Add("ASSY_LOTID", typeof(string));

                DataRow newRow = inTable.NewRow();
                newRow["LANGID"] = LoginInfo.LANGID;
                newRow["EQSGID"] = LoginInfo.CFG_EQSG_ID;
                newRow["EQPTID"] = pEqptID;
                newRow["WIPSTAT"] = "PROC";
                newRow["PROCID"] = Process.CircularCharacteristicGrader;
                newRow["WIPTYPECODE"] = "PROD";
                newRow["ASSY_LOTID"] = pAssyLot;

                inTable.Rows.Add(newRow);

                new ClientProxy().ExecuteService("DA_PRD_SEL_PRODUCTLOT_FO", "RQSTDT", "RSLTDT", inTable, (result, ex) =>
                {
                    if (ex != null)
                    {
                        Util.MessageException(ex);
                    }
                    else
                    {
                        Util.GridSetData(dgProductLot, result, FrameOperation, true);

                        if (result == null || result.Rows.Count == 0)
                        {
                            return;
                        }
                        else
                        {
                            DataTableConverter.SetValue(dgProductLot.Rows[0].DataItem, "CHK", true);
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

        #endregion

        #endregion

        #region [Method]

        #region Pallet 생성 : RunStart()
        private void RunStart()
        {
            if (dgInPallet.Rows.Count == 1)
            {
                //Outbox 정보가 없습니다
                Util.MessageValidation("SFU5010");
                return;
            }

            if (cboEquipment.SelectedIndex < 0 || cboEquipment.SelectedValue.Equals("SELECT"))
            {
                //Util.Alert("설비를 선택 하세요.");
                Util.MessageValidation("SFU1673");
                return;
            }


            if (chkManualPack.IsChecked == false)
            {
                if (string.IsNullOrEmpty(Util.NVC(txtProductLot.Text)))
                {
                    Util.MessageValidation("SFU4014");  //생산 Lot 정보가 없습니다.
                    return;
                }
            }

            try
            {
                DataSet inDataSet = new DataSet();

                DataTable inDataTable = inDataSet.Tables.Add("INDATA");
                inDataTable.Columns.Add("LANGID");
                inDataTable.Columns.Add("SHOPID");
                inDataTable.Columns.Add("AREAID");
                inDataTable.Columns.Add("SHFT_ID");
                inDataTable.Columns.Add("USERID");
                inDataTable.Columns.Add("SHIPTO_ID");
                inDataTable.Columns.Add("WIPNOTE");
                inDataTable.Columns.Add("EQSGID");
                inDataTable.Columns.Add("IFMODE");
                inDataTable.Columns.Add("EQPTID");
                inDataTable.Columns.Add("INBOX_TYPE_CODE");
                inDataTable.Columns.Add("PROD_LOTID");

                DataTable inBoxTable = inDataSet.Tables.Add("OUTBOX");
                inBoxTable.Columns.Add("BOXID");
                inBoxTable.Columns.Add("OUTBOXID");
                inBoxTable.Columns.Add("OUTBOXQTY");
                inBoxTable.Columns.Add("PRODID");
                inBoxTable.Columns.Add("LOTID");
                inBoxTable.Columns.Add("CAPA_GRD_CODE");
                inBoxTable.Columns.Add("VLTG_GRD_CODE");
                inBoxTable.Columns.Add("SOC_VALUE");
                inBoxTable.Columns.Add("LOTTYPE");
                inBoxTable.Columns.Add("PJT_CODE");
                inBoxTable.Columns.Add("EXP_DOM_TYPE_CODE");
                inBoxTable.Columns.Add("LICENSE_NO");

                DataRow newRow = inDataTable.NewRow();
                newRow["LANGID"] = LoginInfo.LANGID;
                newRow["SHOPID"] = LoginInfo.CFG_SHOP_ID;
                newRow["AREAID"] = LoginInfo.CFG_AREA_ID;
                newRow["SHFT_ID"] = sSHFTID;
                newRow["USERID"] = sUSERID;
                //newRow["SHIPTO_ID"] = ""; //TEST
                //newRow["SHIPTO_ID"] = popShipto.SelectedValue.ToString().Trim();
                newRow["WIPNOTE"] = txtNote.Text;
                newRow["EQSGID"] = _processCode;
                newRow["IFMODE"] = IFMODE.IFMODE_OFF;
                newRow["EQPTID"] = Util.NVC(cboEquipment.SelectedValue);
                newRow["INBOX_TYPE_CODE"] = Util.NVC(cboInboxType.SelectedValue);
                if(chkManualPack.IsChecked == false)
                {
                    newRow["PROD_LOTID"] = Util.NVC(txtProductLot.Text);
                }
                else
                {
                    //수동 포장일 경우 기존대로 조립LOT ID 를 저장
                    string sLotid = Util.NVC(dgInPallet.GetCell(0, dgInPallet.Columns["LOT_ID"].Index).Value);
                    newRow["PROD_LOTID"] = sLotid;
                }

                inDataTable.Rows.Add(newRow);
                newRow = null;


                DataTable dt = DataTableConverter.Convert(dgInPallet.ItemsSource);
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    if (dt.Rows[i]["OUT_BOX_ID"].ToString() != string.Empty)
                    {
                        newRow = inBoxTable.NewRow();
                        newRow["BOXID"] = Util.NVC(dgInPallet.GetCell(i, dgInPallet.Columns["PALLET_ID"].Index).Value);
                        newRow["OUTBOXID"] = Util.NVC(dgInPallet.GetCell(i, dgInPallet.Columns["OUT_BOX_ID"].Index).Value);
                        newRow["OUTBOXQTY"] = Util.NVC(dgInPallet.GetCell(i, dgInPallet.Columns["OUT_BOX_QTY"].Index).Value);
                        newRow["PRODID"] = Util.NVC(dgInPallet.GetCell(i, dgInPallet.Columns["PROD_CODE"].Index).Value);
                        newRow["LOTID"] = Util.NVC(dgInPallet.GetCell(i, dgInPallet.Columns["LOT_ID"].Index).Value);
                        newRow["CAPA_GRD_CODE"] = Util.NVC(dgInPallet.GetCell(i, dgInPallet.Columns["CAPA_GRD_CODE"].Index).Value);
                        newRow["VLTG_GRD_CODE"] = Util.NVC(dgInPallet.GetCell(i, dgInPallet.Columns["VLTG_GRD_CODE"].Index).Value);
                        newRow["SOC_VALUE"] = Util.NVC(dgInPallet.GetCell(i, dgInPallet.Columns["SOC_VALUE"].Index).Value);
                        newRow["LOTTYPE"] = Util.NVC(dgInPallet.GetCell(i, dgInPallet.Columns["LOTTYPE"].Index).Value);
                        newRow["PJT_CODE"] = Util.NVC(dgInPallet.GetCell(i, dgInPallet.Columns["PJT_CODE"].Index).Value);
                        newRow["EXP_DOM_TYPE_CODE"] = Util.NVC(dgInPallet.GetCell(i, dgInPallet.Columns["MKT_TYPE_CODE"].Index).Value);
                        newRow["LICENSE_NO"] = Util.NVC(dgInPallet.GetCell(i, dgInPallet.Columns["LUCID_LICENSE_NO"].Index).Value);

                        inBoxTable.Rows.Add(newRow);
                    }
                }


                DataSet dsResult = new ClientProxy().ExecuteServiceSync_Multi("BR_ACT_REG_CREATE_OUTER_PALLET", "INDATA,OUTBOX", "OUTDATA", inDataSet);
                if (dsResult != null && dsResult.Tables["OUTDATA"] != null)
                {
                    PALLET_ID = dsResult.Tables["OUTDATA"].Rows[0]["BOXID"].ToString(); //생성된 아웃박스 출하팔레트 아이디
                    string sFcsPalletID = Util.NVC(dgInPallet.GetCell(0, dgInPallet.Columns["PALLET_ID"].Index).Value);
                    PalletFCSUpd(sFcsPalletID, LoginInfo.CFG_AREA_ID, PALLET_ID);
                   
                }

                this.DialogResult = MessageBoxResult.OK;

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }

        }
        #endregion

        private void PalletFCSUpd(string sPalletId, string sAreaId, string sShipPalletId)
        {
            try
            {
                DataTable dtRqst = new DataTable();
                dtRqst.Columns.Add("AREAID", typeof(string));
                dtRqst.Columns.Add("PALLET_ID", typeof(string));
                dtRqst.Columns.Add("SHIP_PALLET_ID", typeof(string));

                DataRow dr = dtRqst.NewRow();
                dr["AREAID"] = sAreaId;
                dr["PALLET_ID"] = sPalletId;
                dr["SHIP_PALLET_ID"] = sShipPalletId;
                dtRqst.Rows.Add(dr);

                new ClientProxy().ExecuteServiceSync("BR_INF_UPD_TC_AUTOBOXING_BOX_USE", "INDATA", null, dtRqst);
            }
            catch (Exception ex)
            {
                Util.AlertByBiz("BR_INF_UPD_TC_AUTOBOXING_BOX_USE", ex.Message, ex.ToString());
            }
        }
        
        #region 출하처 콤보 생성 : setShipToPopControl()
            //private void setShipToPopControl()
            //{
            //    const string bizRuleName = "DA_PRD_SEL_SHIPTO_CBO";
            //    string[] arrColumn = { "LANGID", "SHOPID" };
            //    string[] arrCondition = { LoginInfo.LANGID, LoginInfo.CFG_SHOP_ID };
            //    CommonCombo.SetFindPopupCombo(bizRuleName, popShipto, arrColumn, arrCondition, (string)popShipto.SelectedValuePath, (string)popShipto.DisplayMemberPath);
            //}
            #endregion

        private void SelectProcessName()
        {
            try
            {
                DataTable inTable = new DataTable();
                inTable.Columns.Add("LANGID", typeof(string));
                inTable.Columns.Add("PROCID", typeof(string));

                DataRow newRow = inTable.NewRow();
                newRow["LANGID"] = LoginInfo.LANGID;
                newRow["PROCID"] = _processCode;
                inTable.Rows.Add(newRow);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_PROCESS_FO", "INDATA", "OUTDATA", inTable);

                if (dtResult != null && dtResult.Rows.Count > 0)
                    _processName = dtResult.Rows[0]["PROCNAME"].ToString();
                else
                    _processName = string.Empty;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }


        private void SetEquipmentCombo(C1ComboBox cbo)
        {
            const string bizRuleName = "DA_BAS_SEL_EQUIPMENT_CBO";
            string[] arrColumn = { "LANGID", "EQSGID", "PROCID", "COATER_EQPT_TYPE_CODE" };
            string[] arrCondition = { LoginInfo.LANGID, LoginInfo.CFG_EQSG_ID, _processCode, null };
            string selectedValueText = cbo.SelectedValuePath;
            string displayMemberText = cbo.DisplayMemberPath;

            CommonCombo.CommonBaseCombo(bizRuleName, cbo, arrColumn, arrCondition, CommonCombo.ComboStatus.NONE, selectedValueText, displayMemberText, LoginInfo.CFG_EQPT_ID);

            ////////////////// 설비가 N건인 경우 Select 추가
            if (cboEquipment.Items.Count > 1 || cboEquipment.Items.Count == 0)
            {
                DataTable dtEqpt = DataTableConverter.Convert(cbo.ItemsSource);
                DataRow drEqpt = dtEqpt.NewRow();
                drEqpt[selectedValueText] = "SELECT";
                drEqpt[displayMemberText] = "- SELECT -";
                dtEqpt.Rows.InsertAt(drEqpt, 0);

                cbo.ItemsSource = null;
                cbo.ItemsSource = dtEqpt.Copy().AsDataView();

                int Index = 0;

                if (!string.IsNullOrEmpty(LoginInfo.CFG_EQPT_ID))
                {
                    DataRow[] drIndex = dtEqpt.Select("CBO_CODE ='" + LoginInfo.CFG_EQPT_ID + "'");

                    if (drIndex.Length > 0)
                    {
                        Index = dtEqpt.Rows.IndexOf(drIndex[0]);
                        cbo.SelectedValue = LoginInfo.CFG_EQPT_ID;
                    }
                }

                cbo.SelectedIndex = Index;
            }
        }   
        #endregion


        private void dgProductLotChoice_Checked(object sender, RoutedEventArgs e)
        {
            try
            {
                RadioButton rbt = sender as RadioButton;
                C1.WPF.DataGrid.C1DataGrid dg = ((C1.WPF.DataGrid.DataGridCellPresenter)rbt.Parent).DataGrid;
                dg.SelectedIndex = ((C1.WPF.DataGrid.DataGridCellPresenter)rbt.Parent).Cell.Row.Index;

                txtProductLot.Text = Util.NVC(DataTableConverter.GetValue(dgProductLot.Rows[dg.SelectedIndex].DataItem, "LOTID"));
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
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




        private void cboEquipment_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            try
            {
                if (cboEquipment.SelectedValue != null && cboInboxType.IsEnabled == true)
                {
                    SetEqptInboxType();
                }

                if (dgInPallet.Rows.Count == 1)
                {
                    return;
                }
                
                ClearProductLot();

                string sLotid = Util.NVC(dgInPallet.GetCell(0, dgInPallet.Columns["LOT_ID"].Index).Value);
                GetProductLot(sLotid, Util.NVC(cboEquipment.SelectedValue));

                
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }


        private void SetEqptInboxType()
        {
            try
            {
                DataTable inTable = new DataTable();
                inTable.Columns.Add("EQPTID", typeof(string));

                DataRow newRow = inTable.NewRow();
                newRow["EQPTID"] = cboEquipment.SelectedValue ?? "";

                inTable.Rows.Add(newRow);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_EQPT_INBOX_TYPE", "INDATA", "OUTDATA", inTable);

                if (dtResult != null && dtResult.Rows.Count > 0)
                {
                    if (cboInboxType.Items.Count > 0)
                        cboInboxType.SelectedValue = dtResult.Rows[0]["INBOX_TYPE"].ToString();
                }

                if (cboInboxType.SelectedValue == null)
                    cboInboxType.SelectedIndex = 0;

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }

        }


        private void ClearProductLot()
        {
            txtProductLot.Text = string.Empty;
            Util.gridClear(dgProductLot);
        }


        private void chkManualPack_Checked(object sender, RoutedEventArgs e)
        {
        }

    }

}
