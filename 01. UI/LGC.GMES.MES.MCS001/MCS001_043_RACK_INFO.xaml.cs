/*************************************************************************************
 Created Date : 
      Creator : 
   Decription : RACK 정보 [팝업]
--------------------------------------------------------------------------------------
 [Change History]
    0000.00.00  홍길동 차장 : 수정 내용 작성.
**************************************************************************************/
using C1.WPF;
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
using C1.WPF.DataGrid;
using LGC.GMES.MES.CMM001;
using LGC.GMES.MES.CMM001.Extensions;

namespace LGC.GMES.MES.MCS001
{
    public partial class MCS001_043_RACK_INFO : C1Window, IWorkArea
    {
        #region Declaration & Constructor 

        public bool IsUpdated;        
        private string _equipmentCode;

        public MCS001_043_RACK_INFO()
        {
            InitializeComponent();
        }

        public IFrameOperation FrameOperation
        {
            get;
            set;
        }

        private void C1Window_Loaded(object sender, RoutedEventArgs e)
        {
            object[] parameters = C1WindowExtension.GetParameters(this);
            if (parameters != null)
            {
                _equipmentCode = Util.NVC(parameters[0]);
                DataTable dtRackInfo = parameters[1] as DataTable;

                if (dtRackInfo != null)
                    SetRackInfo(dtRackInfo);
            }

            ApplyPermissions();
            InitializeCombo();
        }

        /// <summary>
        /// 화면내 버튼 권한 처리
        /// </summary>
        private void ApplyPermissions()
        {
            List<Button> listAuth = new List<Button> { btnChangeRackState, btnDeleteSKId, btnAddSKId};
            Util.pageAuth(listAuth, FrameOperation.AUTHORITY);
        }

        private void InitializeCombo()
        {
            // 상태 콤보박스
            SetRackState(cboRackState);
            CommonCombo _combo = new CommonCombo();
            // 특별 SKID  사유 Combo
            String[] sFilter3 = { "SPCL_RSNCODE" };
            _combo.SetCombo(cboSPCL, CommonCombo.ComboStatus.SELECT, sFilter: sFilter3, sCase: "COMMCODE_WITHOUT_CODE");
        }

        #endregion

        #region Event
        private void btnChangeRackState_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidationChangeRackState())
                return;

            if (CheckManagerAuth())
                ChangeRackState();
        }

        private void btnDeleteSKId_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidationDeleteSkId())
                return;

            try
            {
                const string bizRuleName = "BR_MCS_REG_JUMBOROLL_CST_INFO_ON_RACK_NJ";

                DataSet ds = new DataSet();
                DataTable inDataTable = ds.Tables.Add("INDATA");
                inDataTable.Columns.Add("SRCTYPE", typeof(string));
                inDataTable.Columns.Add("IFMODE", typeof(string));
                inDataTable.Columns.Add("ACT_TYPE", typeof(string));
                inDataTable.Columns.Add("EQPTID", typeof(string));
                inDataTable.Columns.Add("RACK_ID", typeof(string));
                inDataTable.Columns.Add("MCS_CST_ID", typeof(string));
                inDataTable.Columns.Add("UPDUSER", typeof(string));

                DataRow newRow = inDataTable.NewRow();
                newRow["SRCTYPE"] = "UI";
                newRow["IFMODE"] = "OFF";
                newRow["ACT_TYPE"] = "D";
                newRow["EQPTID"] = _equipmentCode;
                newRow["RACK_ID"] = DataTableConverter.GetValue(dgRackInfo.Rows[0].DataItem, "RACK_ID").GetString();
                newRow["MCS_CST_ID"] = DataTableConverter.GetValue(dgSkidInfoDelete.Rows[0].DataItem, "MCS_CST_ID").GetString();
                newRow["UPDUSER"] = LoginInfo.USERID;
                inDataTable.Rows.Add(newRow);

                new ClientProxy().ExecuteService_Multi(bizRuleName, "INDATA", null, (bizResult, bizException) =>
                {
                    try
                    {
                        if (bizException != null)
                        {
                            HiddenLoadingIndicator();
                            Util.MessageException(bizException);
                            return;
                        }

                        Util.MessageInfo("SFU1275"); //정상 처리 되었습니다.
                        IsUpdated = true;

                        // 재조회 처리
                        SelectLotInfoByCarrierId(dgSkidInfoDelete, DataTableConverter.GetValue(dgRackInfo.Rows[0].DataItem, "MCS_CST_ID").GetString());
                    }
                    catch (Exception ex)
                    {
                        HiddenLoadingIndicator();
                        Util.MessageException(ex);
                    }
                }, ds);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void btnAddSKId_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidationAddSkId())
                return;

            try
            {
                const string bizRuleName = "BR_MCS_REG_JUMBOROLL_CST_INFO_ON_RACK_NJ";

                DataSet ds = new DataSet();
                DataTable inDataTable = ds.Tables.Add("INDATA");
                inDataTable.Columns.Add("SRCTYPE", typeof(string));
                inDataTable.Columns.Add("IFMODE", typeof(string));
                inDataTable.Columns.Add("ACT_TYPE", typeof(string));
                inDataTable.Columns.Add("EQPTID", typeof(string));
                inDataTable.Columns.Add("RACK_ID", typeof(string));
                inDataTable.Columns.Add("MCS_CST_ID", typeof(string));
                inDataTable.Columns.Add("UPDUSER", typeof(string));

                DataRow newRow = inDataTable.NewRow();
                newRow["SRCTYPE"] = "UI";
                newRow["IFMODE"] = "OFF";
                newRow["ACT_TYPE"] = "C";
                newRow["EQPTID"] = _equipmentCode;
                newRow["RACK_ID"] = DataTableConverter.GetValue(dgRackInfo.Rows[0].DataItem, "RACK_ID").GetString();
                newRow["MCS_CST_ID"] = DataTableConverter.GetValue(dgSkidInfoAdd.Rows[0].DataItem, "MCS_CST_ID").GetString();
                newRow["UPDUSER"] = LoginInfo.USERID;
                inDataTable.Rows.Add(newRow);

                new ClientProxy().ExecuteService_Multi(bizRuleName, "INDATA", null, (bizResult, bizException) =>
                {
                    try
                    {
                        if (bizException != null)
                        {
                            HiddenLoadingIndicator();
                            Util.MessageException(bizException);
                            return;
                        }

                        Util.MessageInfo("SFU1275"); //정상 처리 되었습니다.
                        IsUpdated = true;

                        // 재조회 처리
                        SelectLotInfoByCarrierId(dgSkidInfoDelete, DataTableConverter.GetValue(dgRackInfo.Rows[0].DataItem, "MCS_CST_ID").GetString());
                    }
                    catch (Exception ex)
                    {
                        HiddenLoadingIndicator();
                        Util.MessageException(ex);
                    }
                }, ds);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void txtSkidId_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter || e.Key == Key.Tab)
            {
                e.Handled = true;

                if (!ValidationDuplicationSkid())
                    return;

                if(dgSkidInfoAdd.GetRowCount() > 0)
                    Util.gridClear(dgSkidInfoAdd);

                SelectLotInfoByCarrierId(dgSkidInfoAdd, txtSkidId.Text.ToUpper());
            }
        }

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
        #endregion

        #region Mehod

        private void SetRackInfo(DataTable dtRackInfo)
        {
            Util.GridSetData(dgRackInfo, dtRackInfo, null, true);
            SelectLotInfoByCarrierId(dgSkidInfoDelete,DataTableConverter.GetValue(dgRackInfo.Rows[0].DataItem, "MCS_CST_ID").GetString());
        }

        private void SelectLotInfoByCarrierId(C1DataGrid dg, string carrierId)
        {
            try
            {
                ShowLoadingIndicator();

                const string bizRuleName = "DA_MCS_SEL_LOT_INFO_BY_CSTID";

                DataTable inTable = new DataTable("RQSTDT");
                inTable.Columns.Add("LANGID", typeof(string));
                inTable.Columns.Add("MCS_CST_ID", typeof(string));

                DataRow dr = inTable.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["MCS_CST_ID"] = carrierId;
                inTable.Rows.Add(dr);

                new ClientProxy().ExecuteService(bizRuleName, "RQSTDT", "RSLTDT", inTable, (result, bizException) =>
                {
                    HiddenLoadingIndicator();
                    try
                    {
                        if (bizException != null)
                        {
                            Util.MessageException(bizException);
                            return;
                        }

                        Util.GridSetData(dg, result, null, true);

                        if (DataTableConverter.GetValue(dgRackInfo.Rows[0].DataItem, "RACK_STAT_CODE").GetString() == "CHECK" || DataTableConverter.GetValue(dgRackInfo.Rows[0].DataItem, "RACK_STAT_CODE").GetString() == "UNUSE")
                        {
                            ChkSPCL.Visibility = Visibility.Collapsed;
                            ChkSPCL_UNDO.Visibility = Visibility.Collapsed;
                        }
                        else
                        {
                            if (result.Rows.Count > 0)
                            {
                                //특별해제 여부 체크
                                if (result.Rows[0]["SPCL_FLAG"].ToString() == "Y")
                                {
                                    ChkSPCL.Visibility = Visibility.Collapsed;
                                    ChkSPCL_UNDO.Visibility = Visibility.Visible;
                                }
                                else
                                {
                                    ChkSPCL.Visibility = Visibility.Visible;
                                    ChkSPCL_UNDO.Visibility = Visibility.Collapsed;
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        HiddenLoadingIndicator();
                        Util.MessageException(ex);
                    }
                });
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void ChangeRackState()
        {
            try
            {
                ShowLoadingIndicator();

                const string bizRuleName = "BR_MCS_REG_RACK_INFO_CHANGE_NJ";
                DataTable inDataTable = new DataTable("INDATA");
                inDataTable.Columns.Add("RACK_ID", typeof(string));
                inDataTable.Columns.Add("RACK_STAT_CODE", typeof(string));
                inDataTable.Columns.Add("EMPTY_REEL_TYPE_CODE", typeof(string));
                inDataTable.Columns.Add("REEL_TYPE_CODE", typeof(string));
                inDataTable.Columns.Add("MCS_CST_ID", typeof(string));
                inDataTable.Columns.Add("UPDUSER", typeof(string));

                DataRow dr = inDataTable.NewRow();
                dr["RACK_ID"] = DataTableConverter.GetValue(dgRackInfo.Rows[0].DataItem, "RACK_ID").GetString();
                dr["RACK_STAT_CODE"] = cboRackState.SelectedValue;
                dr["EMPTY_REEL_TYPE_CODE"] = null;
                dr["REEL_TYPE_CODE"] = null;
                dr["MCS_CST_ID"] = !string.IsNullOrEmpty(DataTableConverter.GetValue(dgRackInfo.Rows[0].DataItem, "MCS_CST_ID").GetString()) ? DataTableConverter.GetValue(dgRackInfo.Rows[0].DataItem, "MCS_CST_ID").GetString() : null;
                dr["UPDUSER"] = LoginInfo.USERID;
                inDataTable.Rows.Add(dr);

                new ClientProxy().ExecuteService(bizRuleName, "INDATA", null, inDataTable, (bizResult, bizException) =>
                {
                    HiddenLoadingIndicator();

                    if (bizException != null)
                    {
                        HiddenLoadingIndicator();
                        Util.MessageException(bizException);
                        return;
                    }

                    Util.MessageInfo("SFU1275"); //정상 처리 되었습니다.
                    IsUpdated = true;
                });
            }
            catch (Exception ex)
            {
                HiddenLoadingIndicator();
                Util.MessageException(ex);
            }
        }

        private bool ValidationChangeRackState()
        {
            if (cboRackState?.SelectedValue == null || cboRackState.Items.Count < 1 || cboRackState?.SelectedValue.GetString() == "SELECT")
            {
                Util.MessageValidation("SFU5059");
                return false;
            }

            return true;
        }

        private bool ValidationAddSkId()
        {
            if (dgSkidInfoAdd.GetRowCount() < 1)
            {
                Util.MessageValidation("SFU1331");
                return false;
            }

            return true;
        }

        private bool ValidationDeleteSkId()
        {
            if (dgSkidInfoDelete.GetRowCount() < 1)
            {
                Util.MessageValidation("SFU1331");
                return false;
            }

            return true;
        }

        private bool ValidationDuplicationSkid()
        {
            if (dgSkidInfoDelete.GetRowCount() > 0)
            {
                var query = (from t in ((DataView)dgSkidInfoDelete.ItemsSource).Table.AsEnumerable()
                    where t.Field<string>("MCS_CST_ID") == txtSkidId.Text
                    select t).ToList();

                if (query.Any())
                {
                    Util.MessageValidation("SFU6007");
                    return false;
                }
            }
            return true;
        }

        private static void SetRackState(C1ComboBox cbo)
        {
            const string bizRuleName = "DA_BAS_SEL_COMMCODE_ATTR_CBO";
            string[] arrColumn = { "CMCDTYPE", "ATTRIBUTE1", "LANGID" };
            string[] arrCondition = { "MCS_RACK_STAT_CODE", "Y", LoginInfo.LANGID };
            string selectedValueText = cbo.SelectedValuePath;
            string displayMemberText = cbo.DisplayMemberPath;

            CommonCombo.CommonBaseCombo(bizRuleName, cbo, arrColumn, arrCondition, CommonCombo.ComboStatus.SELECT, selectedValueText, displayMemberText, LoginInfo.CFG_EQPT_ID);
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
      
        /// <summary>
        /// 특별관리 체크
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CheckBox_Checked(object sender, RoutedEventArgs e)
        {
            this.StackSPCL.Visibility = Visibility.Visible;
        }

        /// <summary>
        /// 특별관리 해제
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            this.StackSPCL.Visibility = Visibility.Collapsed;
        }

        /// <summary>
        /// 특별관리해제 체크
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ChkSPCL_UNDO_Checked(object sender, RoutedEventArgs e)
        {
            this.StackSPCL_UNDO.Visibility = Visibility.Visible;
        }

        /// <summary>
        /// 특별관리해제 체크 해제
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ChkSPCL_UNDO_Unchecked(object sender, RoutedEventArgs e)
        {
            this.StackSPCL_UNDO.Visibility = Visibility.Collapsed;
        }

        /// <summary>
        /// 특별관리 실행
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ApplyButton_Click(object sender, RoutedEventArgs e)
        {
            string sSPCLCode = this.cboSPCL.SelectedValue.ToString();
            string sRemarks = this.tbSPCLREMARKS.Text.Trim();
            //string sRackId = rack.RackId;

            if (sSPCLCode == "SELECT")
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("SFU4542"), null, "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);   //메세지 작성요
                //this.cboSPCL.SelectAll();
                this.cboSPCL.Focus();
                return;
            }

            Util.MessageConfirm("SFU4545", (result) =>
            {
                if (result == MessageBoxResult.OK)
                {
                    DataTable RQSTDT = new DataTable();
                    RQSTDT.TableName = "RQSTDT";
                    RQSTDT.Columns.Add("RACK_ID", typeof(string));
                    RQSTDT.Columns.Add("SPCL_RSNCODE", typeof(string));
                    RQSTDT.Columns.Add("WIP_REMARKS", typeof(string));

                    DataRow dr = RQSTDT.NewRow();
                    dr["RACK_ID"] = DataTableConverter.GetValue(dgRackInfo.Rows[0].DataItem, "RACK_ID").GetString();
                    dr["SPCL_RSNCODE"] = sSPCLCode;
                    dr["WIP_REMARKS"] = sRemarks;
                    RQSTDT.Rows.Add(dr);

                    DataTable RSLTDT = new ClientProxy().ExecuteServiceSync("DA_MCS_UPD_SPCL", "INDATA", null, RQSTDT);

                    SelectLotInfoByCarrierId(dgSkidInfoDelete, DataTableConverter.GetValue(dgRackInfo.Rows[0].DataItem, "MCS_CST_ID").GetString());

                    this.cboSPCL.SelectedIndex = 0;
                    this.tbSPCLREMARKS.Text = "";
                    this.ChkSPCL.IsChecked = false;
                    this.StackSPCL.Visibility = Visibility.Collapsed;
                    Util.MessageInfo("SFU1275");
                    IsUpdated = true;
                }
            });
        }

        /// <summary>
        /// 특별관리해제 실행
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ApplyButton_UNFO_Click(object sender, RoutedEventArgs e)
        {
            string sSPCLCode = this.cboSPCL.SelectedValue.ToString();
            Util.MessageConfirm("SFU5092", (result) =>
            {
                if (result == MessageBoxResult.OK)
                {
                    DataTable RQSTDT = new DataTable();
                    RQSTDT.TableName = "RQSTDT";
                    RQSTDT.Columns.Add("RACK_ID", typeof(string));


                    DataRow dr = RQSTDT.NewRow();
                    dr["RACK_ID"] = DataTableConverter.GetValue(dgRackInfo.Rows[0].DataItem, "RACK_ID").GetString();

                    RQSTDT.Rows.Add(dr);

                    DataTable RSLTDT = new ClientProxy().ExecuteServiceSync("DA_MCS_UPD_SPCL_CLEAR", "INDATA", null, RQSTDT);
                    SelectLotInfoByCarrierId(dgSkidInfoDelete, DataTableConverter.GetValue(dgRackInfo.Rows[0].DataItem, "MCS_CST_ID").GetString());

                    this.ChkSPCL_UNDO.IsChecked = false;
                    this.StackSPCL_UNDO.Visibility = Visibility.Collapsed;
                    Util.MessageInfo("SFU1275");
                    IsUpdated = true;
                }
            });
        }

        private bool CheckManagerAuth()
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("USERID", typeof(string));
                RQSTDT.Columns.Add("AUTHID", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["USERID"] = LoginInfo.USERID;
                dr["AUTHID"] = "LOGIS_MANA";
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_PERSON_BY_AUTH_MULTI", "RQSTDT", "RSLTDT", RQSTDT);

                if (dtResult == null || dtResult.Rows?.Count <= 0)
                {
                    Util.MessageValidation("SFU8324", LoginInfo.USERID);    // [%1] 사용 권한이 없습니다.
                    return false;
                }

                return true;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return false;
            }
        }
    }
}