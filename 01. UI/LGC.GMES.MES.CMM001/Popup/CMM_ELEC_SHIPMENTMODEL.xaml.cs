/*************************************************************************************
 Created Date : 2021.03.23
      Creator : 정재홍
   Decription : 출하MODEL 관리
--------------------------------------------------------------------------------------
 [Change History]
  2021.03.23    정재홍     Initial Created.
  
**************************************************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using C1.WPF;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.ControlsLibrary;
using System.Data;
using C1.WPF.DataGrid;
using LGC.GMES.MES.CMM001.Extensions;

namespace LGC.GMES.MES.CMM001
{
    /// <summary>
    /// CMM_ASSY_RETUBING_LOT_INPUT.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class CMM_ELEC_SHIPMENTMODEL : C1Window, IWorkArea
    {
        #region Declaration & Constructor 
        Util _Util = new Util();
        BizDataSet _Biz = new BizDataSet();
        CommonCombo _combo = new CommonCombo();

        DataTable dtiUse = new DataTable();
        DataTable dtArea = new DataTable();
        DataTable dtProc = new DataTable();
        DataTable dtShip = new DataTable();
        DataTable dtResn = new DataTable();

        string _ProcID = string.Empty;

        #endregion

        #region Initialize 
        /// <summary>
        ///  Frame과 상호작용하기 위한 객체
        /// </summary>
        public IFrameOperation FrameOperation
        {
            get;
            set;
        }

        public CMM_ELEC_SHIPMENTMODEL()
        {
            InitializeComponent();
            Initcombo();
        }


        private void Initcombo()
        {
            //동
            string[] sFilter = { LoginInfo.CFG_SHOP_ID };
            _combo.SetCombo(cboArea, CommonCombo.ComboStatus.SELECT, sFilter: sFilter);

            //공정 PROCBYPCSGID
            string[] sFilter1 = { "E3000,E4000" };
            SetComboProcMuti(cboProcess, sFilter1);
            

            //사용여부
            string[] sFilter2 = { "USE_FLAG" };
            _combo.SetCombo(cboUse, CommonCombo.ComboStatus.ALL, sCase: "COMMCODE", sFilter: sFilter2);
            cboUse.SelectedIndex = 1;
            
        }

        #endregion

        #region Event
        private void C1Window_Initialized(object sender, EventArgs e)
        {
            try
            {

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void C1Window_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                object[] tmps = C1WindowExtension.GetParameters(this);

                if (tmps == null)
                    return;

                _ProcID = Util.NVC(tmps[0]);

                cboProcess.SelectedValue = _ProcID;

                //GRID 사용여부 조회
                dtArea = GetAreaCode();
                dtProc = GetProcess();
                dtiUse = GetCommonCode("IUSE");
                dtShip = GetCommonCode("COMPANY_CODE");
                dtResn = GetResnCode();

                const string bizRuleName = "DA_BAS_SEL_PRODUCT_POPUP_CBO";
                string[] arrColumn = { "PRODID" };
                string[] arrCondition = { null };
                string selectedValueText = (string)((PopupFindDataColumn)dgShipmentModel.Columns["PRODID"]).SelectedValuePath;
                string displayMemberText = (string)((PopupFindDataColumn)dgShipmentModel.Columns["PRODID"]).DisplayMemberPath;

                SetFindGridCombo(bizRuleName, dgShipmentModel.Columns["PRODID"], arrColumn, arrCondition, selectedValueText, displayMemberText);

                GetShipModelSearch();

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        
        private void btnAdd_Click(object sender, RoutedEventArgs e)
        {
            DataGridRowAdd(dgShipmentModel);
        }

        private void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            DataGridRowRemove(dgShipmentModel);
        }

        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            GetShipModelSearch();
        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            setSaveProdDectTag();
        }
        
        private void btnClose_Clicked(object sender, RoutedEventArgs e)
        {
            this.DialogResult = MessageBoxResult.Cancel;
        }

        private void DataGridRowAdd(C1.WPF.DataGrid.C1DataGrid dg)
        {
            try
            {
                DataTable dt = new DataTable();
                if (dg.ItemsSource == null || dg.Rows.Count < 0)
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
                dr["AREAID"] = LoginInfo.CFG_AREA_ID;
                dr["PROCID"] = _ProcID;
                dr["DFCT_TAG_LIMIT_QTY"] = "20";
                dr["RESNCODE"] = "P140000APC";
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

        private void DataGridRowRemove(C1.WPF.DataGrid.C1DataGrid dg)
        {
            try
            {
                DataTable dtInfo = DataTableConverter.Convert(dg.ItemsSource);

                List<DataRow> drInfo = dtInfo.Select("CHK = 1")?.ToList();
                foreach (DataRow dr in drInfo)
                {
                    dtInfo.Rows.Remove(dr);
                }
                Util.GridSetData(dg, dtInfo, FrameOperation, true);

                // 기존 저장자료는 제외
                //if (dg.SelectedIndex > -1)
                //{
                //    DataTable dt = DataTableConverter.Convert(dg.ItemsSource);
                //    dt.Rows[dg.SelectedIndex].Delete();
                //    dt.AcceptChanges();
                //    dg.ItemsSource = DataTableConverter.Convert(dt);
                //}
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return;
            }
        }

        private void dgShipmentModel_BeginningEdit(object sender, C1.WPF.DataGrid.DataGridBeginningEditEventArgs e)
        {
            C1DataGrid grid = sender as C1DataGrid;
            if (grid != null)
            {
                if (e.Column.Index != grid.Columns["CHK"].Index && DataTableConverter.GetValue(e.Row.DataItem, "CHK").Equals(0))
                {
                    e.Cancel = true;
                }
            }
        }

        #endregion

        #region Mehod
        private void GetShipModelSearch()
        {
            try
            {
                DataTable dtRQSTDT = new DataTable();
                dtRQSTDT.TableName = "RQSTDT";
                dtRQSTDT.Columns.Add("AREAID", typeof(string));
                dtRQSTDT.Columns.Add("PROCID", typeof(string));
                dtRQSTDT.Columns.Add("PRODID", typeof(string));
                dtRQSTDT.Columns.Add("USEFLAG", typeof(string));

                DataRow dr = dtRQSTDT.NewRow();
                dr["AREAID"] = Util.GetCondition(cboArea);
                dr["PROCID"] = Util.GetCondition(cboProcess);
                dr["PRODID"] = Util.GetCondition(txtProdID);
                dr["USEFLAG"] = Util.GetCondition(cboUse);
                dtRQSTDT.Rows.Add(dr);

                DataTable dtMain = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_TB_SFC_PRDT_DFCT_TAG_LIMIT_QTY", "RQSTDT", "RSLTDT", dtRQSTDT);

                Util.GridSetData(dgShipmentModel, dtMain, FrameOperation, true);
                (dgShipmentModel.Columns["AREAID"] as C1.WPF.DataGrid.DataGridComboBoxColumn).ItemsSource = DataTableConverter.Convert(dtArea.Copy());      //AREA
                (dgShipmentModel.Columns["PROCID"] as C1.WPF.DataGrid.DataGridComboBoxColumn).ItemsSource = DataTableConverter.Convert(dtProc.Copy());      //PROC
                (dgShipmentModel.Columns["SHIPTO_ID"] as C1.WPF.DataGrid.DataGridComboBoxColumn).ItemsSource = DataTableConverter.Convert(dtShip.Copy());   //SHIP
                (dgShipmentModel.Columns["RESNCODE"] as C1.WPF.DataGrid.DataGridComboBoxColumn).ItemsSource = DataTableConverter.Convert(dtResn.Copy());    //RESNCODE
                (dgShipmentModel.Columns["USE_FLAG"] as C1.WPF.DataGrid.DataGridComboBoxColumn).ItemsSource = DataTableConverter.Convert(dtiUse.Copy());    //사용여부
                
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

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

        private DataTable GetAreaCode()
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("SHOPID", typeof(string));
                RQSTDT.Columns.Add("USERID", typeof(string));
                RQSTDT.Columns.Add("SYSTEM_ID", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["SHOPID"] = LoginInfo.CFG_SHOP_ID;
                dr["USERID"] = LoginInfo.USERID;
                dr["SYSTEM_ID"] = LoginInfo.SYSID;

                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_AUTH_AREA_CBO", "RQSTDT", "RSLTDT", RQSTDT);

                if (dtResult != null && dtResult.Rows.Count > 0)
                    return dtResult;
            }
            catch (Exception ex) { }

            return new DataTable();
        }

        private void SetComboProcMuti(C1ComboBox cbo, string[] sFilter)
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("PROCID", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["PROCID"] = sFilter[0].Equals("") ? null : sFilter[0];
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_PROCESS_BY_PCSGID_CBO", "RQSTDT", "RSLTDT", RQSTDT);

                cbo.DisplayMemberPath = "CBO_NAME";
                cbo.SelectedValuePath = "CBO_CODE";
                cbo.ItemsSource = AddStatus(dtResult, CommonCombo.ComboStatus.SELECT, "CBO_CODE", "CBO_NAME").Copy().AsDataView();

                cbo.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private DataTable GetProcess()
        {
            try
            {
                String[] sFilter = { "E3000,E4000" };

                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("PROCID", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["PROCID"] = sFilter[0].Equals("") ? null : sFilter[0];
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_PROCESS_BY_PCSGID_CBO", "RQSTDT", "RSLTDT", RQSTDT);

                if (dtResult != null && dtResult.Rows.Count > 0)
                    return dtResult;
            }
            catch (Exception ex) { }

            return new DataTable();
        }

        private DataTable GetResnCode()
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("AREAID", typeof(string));
                RQSTDT.Columns.Add("PROCID", typeof(string));
                RQSTDT.Columns.Add("ACTID", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["AREAID"] = LoginInfo.CFG_AREA_ID;
                dr["PROCID"] = _ProcID;
                dr["ACTID"] = "DEFECT_LOT";

                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_DECT_RESNCODE_CBO", "RQSTDT", "RSLTDT", RQSTDT);

                if (dtResult != null && dtResult.Rows.Count > 0)
                    return dtResult;
            }
            catch (Exception ex) { }

            return new DataTable();
        }

        private DataTable AddStatus(DataTable dt, CommonCombo.ComboStatus cs, string sValue, string sDisplay)
        {
            DataRow dr = dt.NewRow();

            switch (cs)
            {
                case CommonCombo.ComboStatus.ALL:
                    dr[sDisplay] = "-ALL-";
                    dr[sValue] = "";
                    dt.Rows.InsertAt(dr, 0);
                    break;

                case CommonCombo.ComboStatus.SELECT:
                    dr[sDisplay] = "-SELECT-";
                    dr[sValue] = "SELECT";
                    dt.Rows.InsertAt(dr, 0);
                    break;

                case CommonCombo.ComboStatus.NA:
                    dr[sDisplay] = "-N/A-";
                    dr[sValue] = "";
                    dt.Rows.InsertAt(dr, 0);
                    break;
            }

            return dt;
        }

        private void SetFindGridCombo(string bizRuleName, C1.WPF.DataGrid.DataGridColumn dgcol, string[] arrColumn, string[] arrCondition, string selectedValueText, string displayMemberText)
        {
            try
            {
                DataTable inDataTable = new DataTable();
                inDataTable.TableName = "RQSTDT";

                if (arrColumn != null)
                {
                    // 동적 컬럼 생성 및 Row 추가
                    foreach (string col in arrColumn)
                    {
                        inDataTable.Columns.Add(col, typeof(string));
                    }

                    DataRow dr = inDataTable.NewRow();
                    for (int i = 0; i < inDataTable.Columns.Count; i++)
                    {
                        dr[inDataTable.Columns[i].ColumnName] = arrCondition[i];
                    }
                    inDataTable.Rows.Add(dr);
                }

                DataTable dtResult = new ClientProxy().ExecuteServiceSync(bizRuleName, "RQSTDT", "RSLTDT", inDataTable);

                DataTable dtBinding = dtResult.DefaultView.ToTable(false, new string[] { selectedValueText, displayMemberText });

                PopupFindDataColumn column = dgcol as PopupFindDataColumn;
                column.AddMemberPath = "PRODID";
                column.ItemsSource = DataTableConverter.Convert(dtBinding);



            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        
        private void setSaveProdDectTag()
        {
            Util.MessageConfirm("SFU1241", (result) =>
            {
                if (result == MessageBoxResult.OK)
                {
                    try
                    {
                        ShowLoadingIndicator();

                        DataTable dt = ((DataView)dgShipmentModel.ItemsSource).Table;
                        DataSet indataSet = new DataSet();

                        DataTable inDATA = indataSet.Tables.Add("INDATA");
                        inDATA.Columns.Add("AREAID");
                        inDATA.Columns.Add("PROCID");
                        inDATA.Columns.Add("PRODID");
                        inDATA.Columns.Add("SHIPTO_ID");
                        inDATA.Columns.Add("DFCT_TAG_LIMIT_QTY");
                        inDATA.Columns.Add("ACTID");
                        inDATA.Columns.Add("RESNCODE");
                        inDATA.Columns.Add("USE_FLAG");
                        inDATA.Columns.Add("INSUSER");
                        inDATA.Columns.Add("INSDTTM");
                        inDATA.Columns.Add("UPDUSER");
                        inDATA.Columns.Add("UPDDTTM");

                        foreach (DataRow inRow in dt.Rows)
                        {
                            if (Convert.ToBoolean(inRow["CHK"]))
                            {
                                DataRow newRow = inDATA.NewRow();
                                newRow["AREAID"] = Util.NVC(inRow["AREAID"]);
                                newRow["PROCID"] = Util.NVC(inRow["PROCID"]);
                                newRow["PRODID"] = Util.NVC(inRow["PRODID"]).ToUpper();
                                newRow["SHIPTO_ID"] = Util.NVC(inRow["SHIPTO_ID"]);
                                newRow["DFCT_TAG_LIMIT_QTY"] = Util.NVC(inRow["DFCT_TAG_LIMIT_QTY"]);
                                newRow["ACTID"] = Util.NVC(inRow["ACTID"]);
                                newRow["RESNCODE"] = Util.NVC(inRow["RESNCODE"]);
                                newRow["USE_FLAG"] = Util.NVC(inRow["USE_FLAG"]);
                                newRow["INSUSER"] = LoginInfo.USERID;
                                newRow["INSDTTM"] = System.DateTime.Now;
                                newRow["UPDUSER"] = LoginInfo.USERID;
                                newRow["UPDDTTM"] = System.DateTime.Now;
                                inDATA.Rows.Add(newRow);
                            }
                        }

                        new ClientProxy().ExecuteService_Multi("DA_BAS_INS_TB_SFC_PRDT_DFCT_TAG_LIMIT_QTY", "INDATA", null, (bizResult, bizException) =>
                        {
                            try
                            {
                                if (bizException != null)
                                {
                                    HiddenLoadingIndicator();
                                    Util.MessageException(bizException);
                                    return;
                                }

                                GetShipModelSearch();
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
                        Util.MessageException(ex);
                    }
                }
            });
        }

        #region [Validation]
        private bool CanSaveProdDectTag()
        {

            DataTable dt = ((DataView)dgShipmentModel.ItemsSource).Table;

            foreach (DataRow row in dt.Rows)
            {
                if (Convert.ToBoolean(row["CHK"]))
                {
                    if (string.IsNullOrEmpty(Util.NVC(row["AREAID"])))
                    {
                        Util.MessageValidation("9063"); //AREA 입력하세요.
                        return false;
                    }

                    if (string.IsNullOrEmpty(Util.NVC(row["PROCID"])))
                    {
                        Util.MessageValidation("SFU1459");  //공정을 선택하세요
                        return false;
                    }

                    if (string.IsNullOrEmpty(Util.NVC(row["PRODID"])))
                    {
                        Util.MessageValidation("SFU2949");  //제품ID를 입력하세요.
                        return false;
                    }

                    if (string.IsNullOrEmpty(Util.NVC(row["DFCT_TAG_LIMIT_QTY"])) || (Util.NVC_Int(row["DFCT_TAG_LIMIT_QTY"]) <= 0))
                    {
                        Util.MessageValidation("SFU1154");  //수량을 입력하세요
                        return false;
                    }

                    if (string.IsNullOrEmpty(Util.NVC(row["RESNCODE"])))
                    {
                        Util.MessageValidation("SFU1639");  //수량을 입력하세요
                        return false;
                    }

                }
            }
            return true;
        }

        #endregion

        #region [Func]

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

        #endregion

        
    }
}

