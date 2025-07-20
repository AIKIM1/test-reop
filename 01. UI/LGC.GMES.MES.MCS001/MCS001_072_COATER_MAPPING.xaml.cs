/*************************************************************************************
 Created Date : 2023.03.08
      Creator : 이병윤 책임
   Decription : VD-Coating 매핑 [팝업]
--------------------------------------------------------------------------------------
 [Change History]
    Date        DEVELOPER   : 수정 내용 작성.
    2023.01.18  이병윤 책임 : E20230113-000080
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

namespace LGC.GMES.MES.MCS001
{
    public partial class MCS001_072_COATER_MAPPING : C1Window, IWorkArea
    {

        #region Declaration & Constructor 

        DataTable dtiUse = new DataTable();
        DataTable dtiVD = new DataTable();
        DataTable dtiCoater = new DataTable();

        string selectedArea = String.Empty;

        public MCS001_072_COATER_MAPPING()
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
            ApplyPermissions();
            InitializeCombo();
            dtiUse = GetCommonCode("IUSE");   // 그리드 사용유무
            dtiVD = GetVdEquipment();         // 그리드 VD설비
            dtiCoater = GetCoaterEquipment(); // 그리드 Coater설비
            object[] parameters = C1WindowExtension.GetParameters( this );
			
        }

        /// <summary>
        /// 화면내 버튼 권한 처리
        /// </summary>
        private void ApplyPermissions()
        {
            List<Button> listAuth = new List<Button>();
            
            Util.pageAuth(listAuth, FrameOperation.AUTHORITY);
        }

        private void InitializeCombo()
        {
            CommonCombo _combo = new CommonCombo();

            //사용여부
            String[] sFilter1 = { "IUSE" };
            _combo.SetCombo(cboUseYN, CommonCombo.ComboStatus.ALL, sFilter: sFilter1, sCase: "COMMCODE");

            // 검색조건 콤보
            SetAreaTypeCombo(cboAreaType);
            SetVDEqpCombo(cboVDEquipment);
            SetEquipmentCombo(cboCoater);
        }        
        #endregion

        #region Event

        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            SearchVDCoater();
        }

        private void btnAdd_Click(object sender, RoutedEventArgs e)
        {
            DataGridRowAdd(dgLotList);
        }

        private void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            DataGridRowRemove(dgLotList);
        }

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            setSaveVDCoater();
        }

        private void cboAreaType_SelectedItemChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (cboAreaType.SelectedIndex > -1)
                {
                    selectedArea = Convert.ToString(cboAreaType.SelectedValue);
                    SetEquipmentCombo(cboCoater);
                    dtiCoater = GetCoaterEquipment();
                }
                else
                {
                    selectedArea = string.Empty;
                }
            }));
        }
        #endregion

        #region Mehod
        /// <summary>
        /// 조회
        /// </summary>
        private void SearchVDCoater()
        {
            try
            {
                ShowLoadingIndicator();

                const string bizRuleName = "DA_PRD_SEL_VD_COATING_MAPP";

                DataTable inTable = new DataTable("RQSTDT");
                inTable.Columns.Add("LANGID", typeof(string));
                inTable.Columns.Add("VD_EQPTID", typeof(string));
                inTable.Columns.Add("COATING_EQPTID", typeof(string));
                inTable.Columns.Add("USE_FLAG", typeof(string));
                inTable.Columns.Add("AREAID", typeof(string));

                DataRow dr = inTable.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["VD_EQPTID"] = cboVDEquipment.SelectedValue;
                dr["COATING_EQPTID"] = cboCoater.SelectedValue;
                dr["USE_FLAG"] = !string.IsNullOrEmpty(Convert.ToString(cboUseYN.SelectedValue)) ? cboUseYN.SelectedValue : null;
                dr["AREAID"] = !string.IsNullOrEmpty(Convert.ToString(cboAreaType.SelectedValue)) ? cboAreaType.SelectedValue : null;
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

                        Util.GridSetData(dgLotList, result, null, false);
                        (dgLotList.Columns["VD_EQPTID"] as C1.WPF.DataGrid.DataGridComboBoxColumn).ItemsSource = DataTableConverter.Convert(dtiVD.Copy());          //VD설비
                        (dgLotList.Columns["COATING_EQPTID"] as C1.WPF.DataGrid.DataGridComboBoxColumn).ItemsSource = DataTableConverter.Convert(dtiCoater.Copy()); //Coater설비
                        (dgLotList.Columns["USE_FLAG"] as C1.WPF.DataGrid.DataGridComboBoxColumn).ItemsSource = DataTableConverter.Convert(dtiUse.Copy());          //사용여부
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
                HiddenLoadingIndicator();
                Util.MessageException(ex);
            }
        }

        /// <summary>
        /// 저장
        /// </summary>
        private void setSaveVDCoater()
        {
            Util.MessageConfirm("SFU1241", (result) =>
            {
                if (result == MessageBoxResult.OK)
                {
                    try
                    {
                        ShowLoadingIndicator();

                        DataTable dt = ((DataView)dgLotList.ItemsSource).Table;
                        DataSet indataSet = new DataSet();

                        DataTable inDATA = indataSet.Tables.Add("INDATA");
                        inDATA.Columns.Add("VD_EQPTID");
                        inDATA.Columns.Add("COATING_EQPTID");
                        inDATA.Columns.Add("USE_FLAG");
                        inDATA.Columns.Add("USERID");

                        foreach (DataRow inRow in dt.Rows)
                        {
                            if (Convert.ToBoolean(inRow["CHK"]))
                            {
                                DataRow newRow = inDATA.NewRow();
                                newRow["VD_EQPTID"] = Util.NVC(inRow["VD_EQPTID"]); //VD_EQPTID
                                newRow["COATING_EQPTID"] = Util.NVC(inRow["COATING_EQPTID"]);
                                newRow["USE_FLAG"] = Util.NVC(inRow["USE_FLAG"]);
                                newRow["USERID"] = LoginInfo.USERID;
                                //newRow["INSDTTM"] = System.DateTime.Now;
                                inDATA.Rows.Add(newRow);
                            }
                        }

                        new ClientProxy().ExecuteService_Multi("BR_PRD_MERGE_VD_COATING_MAPP", "INDATA", null, (bizResult, bizException) =>
                        {
                            try
                            {
                                if (bizException != null)
                                {
                                    HiddenLoadingIndicator();
                                    Util.MessageException(bizException);
                                    return;
                                }

                                SearchVDCoater();
                                Util.MessageInfo("SFU1275");    //정상 처리 되었습니다.
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
                        HiddenLoadingIndicator();
                        Util.MessageException(ex);
                    }
                }
            });
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

        /// <summary>
        /// 영역 유형 코드[AREA_TYPE_CODE] : 전극[E]
        /// </summary>
        /// <param name="cbo"></param>
        private void SetAreaTypeCombo(C1ComboBox cbo)
        {
            const string bizRuleName = "DA_BAS_SEL_AREA_BY_AREATYPE_CBO";
            string[] arrColumn = { "LANGID", "AREA_TYPE_CODE" };
            string[] arrCondition = { LoginInfo.LANGID, "E" };
            string selectedValueText = cbo.SelectedValuePath;
            string displayMemberText = cbo.DisplayMemberPath;

            CommonCombo.CommonBaseCombo(bizRuleName, cbo, arrColumn, arrCondition, CommonCombo.ComboStatus.NA, selectedValueText, displayMemberText, string.Empty);
        }

        /// <summary>
        /// VD설비 콤보 세팅 
        /// </summary>
        /// <param name="cbo"></param>
        private void SetVDEqpCombo(C1ComboBox cbo)
        {
            const string bizRuleName = "DA_BAS_SEL_EQUIPMENT_AREA_CBO"; // 
            string[] arrColumn = { "LANGID", "AREAID", "PROCID" };
            string[] arrCondition = { LoginInfo.LANGID, LoginInfo.CFG_AREA_ID, "A1000" };
            string selectedValueText = cbo.SelectedValuePath;
            string displayMemberText = cbo.DisplayMemberPath;

            CommonCombo.CommonBaseCombo(bizRuleName, cbo, arrColumn, arrCondition, CommonCombo.ComboStatus.ALL, selectedValueText, displayMemberText, string.Empty);
        }

        /// <summary>
        /// Coating설비 콤보 세팅
        /// </summary>
        /// <param name="cbo"></param>
        private void SetEquipmentCombo(C1ComboBox cbo)
        {

            cbo.ItemsSource = null;
            cbo.Items.Clear();

            string bizRuleName = "DA_BAS_SEL_EQUIPMENT_AREA_CBO";
            string[] arrColumn = { "LANGID", "AREAID", "PROCID" };
            string[] arrCondition = { LoginInfo.LANGID, selectedArea, "E2000" };

            if (selectedArea.Equals(String.Empty))
            {
                bizRuleName = "DA_BAS_SEL_EQUIPMENT_CBO";
                arrColumn = new string[] { "LANGID", "PROCID" };
                arrCondition = new string[] { LoginInfo.LANGID, "E2000" };
            }

            string selectedValueText = cbo.SelectedValuePath;
            string displayMemberText = cbo.DisplayMemberPath;

            CommonCombo.CommonBaseCombo(bizRuleName, cbo, arrColumn, arrCondition, CommonCombo.ComboStatus.ALL, selectedValueText, displayMemberText, string.Empty);

        }

        /// <summary>
        /// 그리드 사용유무 콤보조회
        /// </summary>
        /// <param name="sType"></param>
        /// <returns></returns>
        private DataTable GetCommonCode(string sType)
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("CMCDTYPE", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["CMCDTYPE"] = sType;
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_COMMONCODE", "RQSTDT", "RSLTDT", RQSTDT);

                if (dtResult != null && dtResult.Rows.Count > 0)
                    return dtResult;
            }
            catch (Exception ex) { }

            return new DataTable();
        }

        /// <summary>
        /// 그리드 VD설비 콤보조회
        /// </summary>
        /// <returns></returns>
        private DataTable GetVdEquipment()
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("AREAID", typeof(string));
                RQSTDT.Columns.Add("PROCID", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["AREAID"] = LoginInfo.CFG_AREA_ID;
                dr["PROCID"] = "A1000";
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_EQUIPMENT_AREA_CBO", "RQSTDT", "RSLTDT", RQSTDT);

                if (dtResult != null && dtResult.Rows.Count > 0)
                    return dtResult;
            }
            catch (Exception ex) { }

            return new DataTable();
        }

        /// <summary>
        /// 그리드 Coater설비 콤보조회
        /// </summary>
        /// <returns></returns>
        private DataTable GetCoaterEquipment()
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("AREAID", typeof(string));
                RQSTDT.Columns.Add("PROCID", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["AREAID"] = selectedArea;
                dr["PROCID"] = "E2000";
                RQSTDT.Rows.Add(dr);

                string bizRole = "DA_BAS_SEL_EQUIPMENT_AREA_CBO";

                if(selectedArea.Equals(String.Empty))
                {
                    bizRole = "DA_BAS_SEL_EQUIPMENT_CBO";
                }
                DataTable dtResult = new ClientProxy().ExecuteServiceSync(bizRole, "RQSTDT", "RSLTDT", RQSTDT);

                if (dtResult != null && dtResult.Rows.Count > 0)
                    return dtResult;
            }
            catch (Exception ex) { }

            return new DataTable();
        }

        /// <summary>
        /// 그리드 행추가
        /// </summary>
        /// <param name="dg"></param>
        private void DataGridRowAdd(C1.WPF.DataGrid.C1DataGrid dg)
        {
            try
            {
                DataTable dt = new DataTable();
                if (dg.ItemsSource == null || dg.Rows.Count < 0 || dtiCoater.Rows.Count < 0 || dtiVD.Rows.Count < 0)
                {
                    return;
                }

                foreach (C1.WPF.DataGrid.DataGridColumn col in dg.Columns)
                {
                    dt.Columns.Add(Convert.ToString(col.Name));
                }

                dt = DataTableConverter.Convert(dg.ItemsSource);
                DataRow dr = dt.NewRow();
                dr["CHK"] = 1;
                dr["VD_EQPTID"] = Convert.ToString(dtiVD.Rows[0][0]); // dtiVD
                dr["COATING_EQPTID"] = Convert.ToString(dtiCoater.Rows[0][0]); // dtiVD
                dr["USE_FLAG"] = "Y";

                dt.Rows.Add(dr);
                dt.AcceptChanges();

                dg.ItemsSource = DataTableConverter.Convert(dt);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return;
            }
        }

        /// <summary>
        /// 그리드 행삭제
        /// </summary>
        /// <param name="dg"></param>
        private void DataGridRowRemove(C1.WPF.DataGrid.C1DataGrid dg)
        {
            try
            {
                DataTable dtInfo = DataTableConverter.Convert(dg.ItemsSource);

                List<DataRow> drInfo = dtInfo.Select("CHK = 1 AND UPDUSER IS NULL ")?.ToList();
                foreach (DataRow dr in drInfo)
                {
                    dtInfo.Rows.Remove(dr);
                }
                Util.GridSetData(dg, dtInfo, FrameOperation, false);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return;
            }
        }


        #endregion

        
    }
}