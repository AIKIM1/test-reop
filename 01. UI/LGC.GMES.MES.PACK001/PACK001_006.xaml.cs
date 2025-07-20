/*************************************************************************************
 Created Date : 2016.06.16
      Creator : 
   Decription : 
--------------------------------------------------------------------------------------
 [Change History]
  2016.06.16  DEVELOPER : Initial Created.





 
**************************************************************************************/

using System;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using C1.WPF;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;

namespace LGC.GMES.MES.PACK001
{
    public partial class PACK001_006 : UserControl, IWorkArea
    {
        #region Declaration & Constructor 
        LGC.GMES.MES.PACK001.Class.MESSAGE_PARAM ms = new LGC.GMES.MES.PACK001.Class.MESSAGE_PARAM();

        //DataTable dtSearchResult;
        DataTable dtLotSearchResult;
        DataTable dtGridChek;
        CommonCombo _combo = new CommonCombo();

        bool search_fullCheck = false;
        bool lot_fullCheck = false;        

        #region LOT SCAN시 처리 변수
        DataTable dtLotResult;
        string pre_procid_cause = string.Empty; // 최초 투입 LOT의 원인공정
        string pre_proctype = string.Empty;     // 최초 투입 LOT의 공정타입 : R(수리공정), S(폐기공정)
        string pre_procid = string.Empty;       // 최초 투입 LOT의 공정
        string pre_eqsgid = string.Empty;       // 최초 투입 LOT의 라인
        string statusvalue = string.Empty;      // 최초 투입 LOT의 status : REWORK_WAIT(재작업대기), SCRAP_WAIT(폐기대기)
        #endregion

        public PACK001_006()
        {
            InitializeComponent();

            this.Loaded += PACK001_006_Loaded;           
        }       

        public IFrameOperation FrameOperation
        {
            get;
            set;
        }

        #endregion

        #region Initialize
        private void Initialize()
        {
            try
            {
                InitCombo();

                if (Util.GetCondition(cboEquipmentSegment) == "")
                {
                    return;
                }

                //getSearch();
            }
            catch(Exception ex)
            {
                Util.AlertInfo(ex.Message);
            }
        }

        private void InitCombo()
        {
            C1ComboBox cboSHOPID = new C1ComboBox();
            cboSHOPID.SelectedValue = LoginInfo.CFG_SHOP_ID;
            C1ComboBox cboAreaByAreaType = new C1ComboBox();
            cboAreaByAreaType.SelectedValue = LoginInfo.CFG_AREA_ID;
            C1ComboBox cboAREA_TYPE_CODE = new C1ComboBox();
            cboAREA_TYPE_CODE.SelectedValue = Area_Type.PACK;  
            C1ComboBox cboProductModel = new C1ComboBox();

            setPilotGubun();

            //라인             
            C1ComboBox[] cboEquipmentSegmentParent = { cboAreaByAreaType, cboPilotGubun };
            C1ComboBox[] cboLineChild = { cboProductPilot, cboPilotProc };
            _combo.SetCombo(cboEquipmentSegment, CommonCombo.ComboStatus.SELECT, cbParent: cboEquipmentSegmentParent, cbChild: cboLineChild, sCase:"PILOTLINE");

            //제품분류(PACK 제품 분류)
            string[] productType = { LoginInfo.CFG_SHOP_ID
                                    ,LoginInfo.CFG_AREA_ID
                                    ,LoginInfo.CFG_EQSG_ID
                                    ,Area_Type.PACK };
            C1ComboBox[] cboPrdtClassChild = { cboProductPilot };          
            _combo.SetCombo(cboPrdtClass, CommonCombo.ComboStatus.SELECT, cbChild: cboPrdtClassChild, sFilter: productType);

            //제품코드  
            C1ComboBox[] cboProductParent = { cboSHOPID, cboAreaByAreaType, cboEquipmentSegment, cboProductModel, cboPrdtClass, cboPilotGubun };
            _combo.SetCombo(cboProductPilot, CommonCombo.ComboStatus.ALL, cbParent: cboProductParent, sCase: "PRJ_PRODUCT_PILOT");

            //파일럿 공정
            C1ComboBox[] cboPilotProcParent = { cboAreaByAreaType, cboEquipmentSegment, cboPilotGubun };           
            _combo.SetCombo(cboPilotProc, CommonCombo.ComboStatus.SELECT, cbParent: cboPilotProcParent);

            this.cboListCount.SelectedValueChanged -= new System.EventHandler<C1.WPF.PropertyChangedEventArgs<object>>(this.cboListCount_SelectedValueChanged);
            Util.Set_Pack_cboListCoount(cboListCount, "CBO_NAME", "CBO_CODE", 100, 1000, 100);
            this.cboListCount.SelectedValueChanged += new System.EventHandler<C1.WPF.PropertyChangedEventArgs<object>>(this.cboListCount_SelectedValueChanged);
        }

        private void setPilotGubun()
        {
            DataTable dtResult = new DataTable();
            dtResult.Columns.Add("CBO_NAME", typeof(string));
            dtResult.Columns.Add("CBO_CODE", typeof(string));

            DataRow newRow = dtResult.NewRow();
/*
            newRow = dtResult.NewRow();
            newRow.ItemArray = new object[] { "전체", "" };
            dtResult.Rows.Add(newRow);

            
            newRow = dtResult.NewRow();
            newRow.ItemArray = new object[] { "일반-LINE", "NORMAL" };
            dtResult.Rows.Add(newRow);
*/
            newRow = dtResult.NewRow();
            newRow.ItemArray = new object[] { "PILOT-LINE", "PILOT" };
            dtResult.Rows.Add(newRow);
            
            cboPilotGubun.ItemsSource = DataTableConverter.Convert(dtResult);

            cboPilotGubun.SelectedIndex = 0;
            
        }

        #endregion

        #region Event
        private void PACK001_006_Loaded(object sender, RoutedEventArgs e)
        {
            Initialize();

            this.Loaded -= PACK001_006_Loaded;
            //this.cboPilotGubun.SelectedIndexChanged += cboPilotGubun_SelectedIndexChanged;
            this.cboPilotGubun.SelectedValueChanged += CboPilotGubun_SelectedValueChanged;

            txtLotID.Focus();
            tbSearch_cnt.Text = "[ 0 " + ObjectDic.Instance.GetObjectName("건") + " ]";
            tbSearch_cnt1.Text = "[ 0 " + ObjectDic.Instance.GetObjectName("건") + " ]";
        }
        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                dgSearchResult.ItemsSource = null;
                
                if (chkInputData())
                {
                    getSearch();
                }

                txtLotID.Focus();
            }
            catch(Exception ex)
            {
                Util.AlertInfo(ex.Message);
            }
        }        

        private void btnAllSelect_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                C1.WPF.DataGrid.C1DataGrid dg = new C1.WPF.DataGrid.C1DataGrid();
                bool fullCheck;

                if (tcMain.SelectedIndex == 0)
                {
                    dg = dgSearchResult;
                    fullCheck = search_fullCheck;
                }
                else
                {
                    dg = dgSearchResult1;
                    fullCheck = lot_fullCheck;
                }

                if (dg.GetRowCount() == 0)
                {
                    return;
                }
                else
                {
                    if (tcMain.SelectedIndex == 0)
                    {
                        fullCheck = search_fullCheck;
                    }
                    else
                    {
                        fullCheck = lot_fullCheck;
                    }
                }

                DataTable dt = DataTableConverter.Convert(dg.ItemsSource);
                Button btn = sender as Button;

                if (fullCheck == false)
                {
                    for (int i = 0; i < dt.Rows.Count; i++)
                    {
                        dt.Rows[i][0] = true;
                    }
                    fullCheck = true;
                    btn.Content = ObjectDic.Instance.GetObjectName("전체해제");
                    btn.Foreground = Brushes.Red;
                }
                else
                {
                    for (int i = 0; i < dt.Rows.Count; i++)
                    {
                        dt.Rows[i][0] = false;
                    }
                    fullCheck = false;
                    btn.Content = ObjectDic.Instance.GetObjectName("전체선택");
                    btn.Foreground = Brushes.Red;
                }

                if (tcMain.SelectedIndex == 0)
                {
                    search_fullCheck = fullCheck;
                }
                else
                {
                    lot_fullCheck = fullCheck;
                }

                SetBinding(dg, dt);
            }
            catch(Exception ex)
            {
                Util.AlertInfo(ex.Message);
            }                  
        }

        private void btnAllEndOper_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                C1.WPF.DataGrid.C1DataGrid dg = new C1.WPF.DataGrid.C1DataGrid();               

                if (tcMain.SelectedIndex == 0)
                {
                    dg = dgSearchResult;                   
                }
                else
                {
                    dg = dgSearchResult1;                   
                }

                if(dg.GetRowCount() == 0)
                {
                    return;
                }

                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(ms.AlertRetun("SFU1744"), null, "Information", MessageBoxButton.OKCancel, MessageBoxIcon.None, (sResult) =>
                //완공처리 하시겠습니까?
                {
                    if (sResult == MessageBoxResult.OK)
                    {
                        //BIZ에서 MULTIL로 처리
                        lotEnd_multi(dg);
                    }
                });
                
            }
            catch (Exception ex)
            {
                //Util.AlertInfo(ex.Message);
                Util.MessageException(ex);
            }
        }        

        private void btnExel_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                C1.WPF.DataGrid.C1DataGrid dg = new C1.WPF.DataGrid.C1DataGrid();
                if (tcMain.SelectedIndex == 0)
                {
                    dg = dgSearchResult;
                }
                else
                {
                    dg = dgSearchResult1;
                }

                if (dg.GetRowCount() == 0)
                {
                    return;
                }

                new ExcelExporter().Export(dg);
            }
            catch (Exception ex)
            {
                Util.AlertInfo(ex.Message);
            }
        }

        private void dgSearchResult_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            try
            {
                Point pnt = e.GetPosition(null);
                C1.WPF.DataGrid.DataGridCell cell = dgSearchResult.GetCellFromPoint(pnt);

                if (cell != null)
                {
                    if (cell.Column.Name == "LOTID")
                    {
                        this.FrameOperation.OpenMenu("SFU010090090", true, cell.Text, "LOTID");
                    }
                }
            }
            catch (Exception ex)
            {
                Util.AlertInfo(ex.Message);
            }
        }

        private void dgSearchResult1_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            try
            {
                Point pnt = e.GetPosition(null);
                C1.WPF.DataGrid.DataGridCell cell = dgSearchResult1.GetCellFromPoint(pnt);

                if (cell != null)
                {
                    if (cell.Column.Name == "LOTID")
                    {
                        this.FrameOperation.OpenMenu("SFU010090090", true, cell.Text, "LOTID");
                    }
                }
            }
            catch (Exception ex)
            {
                Util.AlertInfo(ex.Message);
            }
        }

        private void cboListCount_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {

        }

        private void dgSearchResult_LoadedCellPresenter(object sender, C1.WPF.DataGrid.DataGridCellEventArgs e)
        {
            try
            {

                C1.WPF.DataGrid.C1DataGrid dataGrid = sender as C1.WPF.DataGrid.C1DataGrid;

                dataGrid.Dispatcher.BeginInvoke(new Action(() =>
                {
                    if (e.Cell.Presenter == null)
                    {
                        return;
                    }
                    if (e.Cell.Column.Name == "LOTID")
                    {

                        //e.Cell.Presenter.Background = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#72166B"));
                        //e.Cell.Presenter.Foreground = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#72166B"));
                        e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Blue);
                        e.Cell.Presenter.FontWeight = FontWeights.Bold;
                    }
                    else
                    {
                        e.Cell.Presenter.Foreground = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#FF333333"));
                        e.Cell.Presenter.FontWeight = FontWeights.Normal;
                    }
                }));
            }
            catch (Exception ex)
            {
                Util.Alert(ex.ToString());
            }
        }

        private void dgSearchResult1_LoadedCellPresenter(object sender, C1.WPF.DataGrid.DataGridCellEventArgs e)
        {
            try
            {

                C1.WPF.DataGrid.C1DataGrid dataGrid = sender as C1.WPF.DataGrid.C1DataGrid;

                dataGrid.Dispatcher.BeginInvoke(new Action(() =>
                {
                    if (e.Cell.Presenter == null)
                    {
                        return;
                    }
                    if (e.Cell.Column.Name == "LOTID")
                    {

                        //e.Cell.Presenter.Background = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#72166B"));
                        //e.Cell.Presenter.Foreground = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#72166B"));
                        e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Blue);
                        e.Cell.Presenter.FontWeight = FontWeights.Bold;
                    }
                    else
                    {
                        e.Cell.Presenter.Foreground = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#FF333333"));
                        e.Cell.Presenter.FontWeight = FontWeights.Normal;
                    }
                }));
            }
            catch (Exception ex)
            {
                Util.Alert(ex.ToString());
            }
        }
        #endregion

        #region Mehod

        private void getSearch()
        {
            try
            {
                DataTable inDataTable = new DataTable();

                inDataTable.Columns.Add("LANGID", typeof(string));
                inDataTable.Columns.Add("PROCID", typeof(string));
                inDataTable.Columns.Add("PRODID", typeof(string));
                inDataTable.Columns.Add("PRDT_CLASS_CODE", typeof(string));
                inDataTable.Columns.Add("EQSGID", typeof(string));
                inDataTable.Columns.Add("PILOT_GUBUN", typeof(string));
                inDataTable.Columns.Add("COUNT", typeof(Int64));

                DataRow searchCondition = inDataTable.NewRow();
                searchCondition["LANGID"] = LoginInfo.LANGID;
                searchCondition["PROCID"] = Util.GetCondition(cboPilotProc) == "" ? null : cboPilotProc.SelectedValue; //"POA2010"; //Util.NVC(cboPilotProc.SelectedValue);                
                searchCondition["PRODID"] = Util.GetCondition(cboProductPilot) == "" ? null : cboProductPilot.SelectedValue; //  "ACEP1043I-A1";// pilot 관련 제품정보가 없어서 데이터 나오는 것으로 하드코딩 : Util.NVC(cboProduct.SelectedValue); 
                searchCondition["PRDT_CLASS_CODE"] = Util.GetCondition(cboPrdtClass) == "" ? null : cboPrdtClass.SelectedValue; // "CELL"; // PILOT 관련 제품분류정보가 없어서 데이터 나오는 것으로 하드 코딩 Util.NVC(cboPrdtClass.SelectedValue);  
                searchCondition["EQSGID"] = Util.GetCondition(cboEquipmentSegment) == "" ? null : cboEquipmentSegment.SelectedValue;
                searchCondition["PILOT_GUBUN"] = Util.GetCondition(cboPilotGubun) == "" ? "" : cboPilotGubun.SelectedValue;
                searchCondition["COUNT"] = Util.GetCondition(cboListCount) == "" ? "" : cboListCount.SelectedValue;

                inDataTable.Rows.Add(searchCondition);

                //dtSearchResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_PILOTLOT_SEARCH", "INDATA", "OUTDATA", inDataTable);
                loadingIndicator.Visibility = Visibility.Visible;
                new ClientProxy().ExecuteService("DA_PRD_SEL_PILOTLOT_SEARCH", "INDATA", "OUTDATA", inDataTable, (dtSearchResult, ex) =>
                {
                    loadingIndicator.Visibility = Visibility.Collapsed;

                    if (ex != null)
                    {
                        //Util.AlertByBiz("DA_PRD_SEL_PILOTLOT_SEARCH", ex.Message, ex.ToString());
                        Util.MessageException(ex);
                        return;
                    }
                    if (dtSearchResult.Rows.Count != 0)
                    {
                        Util.GridSetData(dgSearchResult, dtSearchResult, FrameOperation);
                    }

                    Util.SetTextBlockText_DataGridRowCount(tbSearch_cnt, Util.NVC(dtSearchResult.Rows.Count));
                });
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private void lotEnd_multi(C1.WPF.DataGrid.C1DataGrid dg)
        {
            try
            {              

                DataTable dtSearchResult = DataTableConverter.Convert(dg.ItemsSource);

                DataSet dsInput = new DataSet();

                DataTable INDATA = new DataTable();
                INDATA.TableName = "INDATA";
                INDATA.Columns.Add("SRCTYPE", typeof(string));
                INDATA.Columns.Add("LANGID", typeof(string));
                INDATA.Columns.Add("LOTID", typeof(string));
                INDATA.Columns.Add("END_PROCID", typeof(string));
                INDATA.Columns.Add("END_EQPTID", typeof(string));
                INDATA.Columns.Add("STRT_PROCID", typeof(string));
                INDATA.Columns.Add("STRT_EQPTID", typeof(string));
                INDATA.Columns.Add("USERID", typeof(string));
                INDATA.Columns.Add("WIPNOTE", typeof(string));
                INDATA.Columns.Add("RESNCODE", typeof(string));
                INDATA.Columns.Add("RESNNOTE", typeof(string));
                INDATA.Columns.Add("RESNDESC", typeof(string));

                int chk_idx = 0;
                for (int i = 0; i < dg.GetRowCount(); i++)
                {
                    if (DataTableConverter.GetValue(dg.Rows[i].DataItem, "CHK").ToString() == "True")
                    {

                        //C1.WPF.DataGrid.DataGridRow dgr = dgSearchResult.Rows[i] as C1.WPF.DataGrid.DataGridRow;
                        //DataRowView drv = dgr.DataItem as DataRowView;
                        DataRow drv = dtSearchResult.Rows[i] as DataRow;

                        DataRow drINDATA = INDATA.NewRow();
                        drINDATA["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                        drINDATA["LANGID"] = LoginInfo.LANGID;
                        drINDATA["LOTID"] = drv["LOTID"].ToString();
                        drINDATA["END_PROCID"] = drv["PROCID"].ToString(); //현재불량공정
                        drINDATA["END_EQPTID"] = Util.NVC(drv["EQPTID"]) == "" ? null : drv["EQPTID"].ToString();
                        drINDATA["STRT_PROCID"] = getReturnProcess(drv); //다음공정
                        drINDATA["STRT_EQPTID"] = null;
                        drINDATA["USERID"] = LoginInfo.USERID;
                        drINDATA["WIPNOTE"] = null;
                        drINDATA["RESNCODE"] = "OK";
                        drINDATA["RESNNOTE"] = null;
                        drINDATA["RESNDESC"] = null;
                        INDATA.Rows.Add(drINDATA);

                        chk_idx++;
                    }
                }

                if (chk_idx == 0)
                {
                    return;
                }

                dsInput.Tables.Add(INDATA);

                DataTable IN_CLCTITEM = new DataTable();
                IN_CLCTITEM.TableName = "IN_CLCTITEM";
                IN_CLCTITEM.Columns.Add("CLCTITEM", typeof(string));
                IN_CLCTITEM.Columns.Add("CLCTVAL", typeof(string));
                IN_CLCTITEM.Columns.Add("PASSYN", typeof(string));
                dsInput.Tables.Add(IN_CLCTITEM);

                DataTable IN_CLCTDITEM = new DataTable();
                IN_CLCTDITEM.TableName = "IN_CLCTDITEM";
                IN_CLCTDITEM.Columns.Add("CLCTDITEM", typeof(string));
                IN_CLCTDITEM.Columns.Add("CLCTDVAL", typeof(string));
                dsInput.Tables.Add(IN_CLCTDITEM);

                //DataSet dsResult = new ClientProxy().ExecuteServiceSync_Multi("BR_PRD_REG_OUTPUTASSY_MULTILOT", "INDATA,IN_CLCTITEM,IN_CLCTDITEM", "OUTDATA", dsInput, null);

                

                loadingIndicator.Visibility = Visibility.Visible;

                new ClientProxy().ExecuteService_Multi("BR_PRD_REG_OUTPUTASSY_MULTILOT", "INDATA,IN_CLCTITEM,IN_CLCTDITEM", "OUTDATA", (dsResult, dataException) =>
                {
                    try
                    {


                        if (dataException != null)
                        {
                            //Util.AlertByBiz("BR_PRD_REG_OUTPUTASSY_MULTILOT", dataException.Message, dataException.ToString());
                            Util.MessageException(dataException);
                            loadingIndicator.Visibility = Visibility.Collapsed;
                            return;
                        }
                        else
                        {
                            loadingIndicator.Visibility = Visibility.Collapsed;
                            if (dsResult.Tables["OUTDATA"] != null)
                            {
                                if (dsResult.Tables["OUTDATA"].Rows.Count > 0)
                                {
                                    ms.AlertInfo("SFU1275"); //정상처리되었습니다.

                                    if (tcMain.SelectedIndex == 0)
                                    {
                                        getSearch();
                                    }
                                    else
                                    {
                                        tbState.Text = "";
                                        txtLotID.Text = "";
                                        dg.ItemsSource = null;

                                        lot_fullCheck = false;
                                        btnAllSelect1.Content = ObjectDic.Instance.GetObjectName("전체선택");
                                        btnAllSelect1.Foreground = Brushes.White;
                                    }
                                }
                            }
                        }
                        
                        return;
                    }
                    catch (Exception ex)
                    {
                        loadingIndicator.Visibility = Visibility.Collapsed;
                        throw ex;
                    }

                }, dsInput);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private string getReturnProcess(object obj)
        {
            try
            {
                DataRow drv = obj as DataRow;
                string return_str = string.Empty;

                //다음 공정 선택           
                string sProcID = Util.NVC(drv["PROCID"]);
                string sRoutID = Util.NVC(drv["ROUTID"]);
                string sFlowID = Util.NVC(drv["FLOWID"]);

                DataTable dtNextProcess = getDbProcess(sProcID, sRoutID, sFlowID);

                if (dtNextProcess.Rows.Count == 0)
                {
                    return_str = null;
                }
                else
                {
                    if (dtNextProcess.Rows.Count != 1)
                    {
                        return_str = null;
                    }
                    else
                    {
                        return_str = dtNextProcess.Rows[0]["PROCID_TO"].ToString(); //다음공정
                    }
                }

                return return_str;
            }
            catch(Exception ex)
            {
                throw ex;
            }
            
        }

        private DataTable getDbProcess(string sProcess, string sRoutid, string sFlowid)
        {
            DataTable dtResult = null;
            try
            {
                //DA_PRD_SEL_PROC_ROUTE_PREVIOUS
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("PROCID", typeof(string));
                RQSTDT.Columns.Add("ROUTID", typeof(string));
                RQSTDT.Columns.Add("FLOWID", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["PROCID"] = sProcess;
                dr["ROUTID"] = sRoutid;
                dr["FLOWID"] = sFlowid;
                RQSTDT.Rows.Add(dr);

                dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_WIP_PACK_NEXTPROCID_INFO", "RQSTDT", "RSLTDT", RQSTDT);
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return dtResult;
        }

        private void SetBinding(C1.WPF.DataGrid.C1DataGrid dg, DataTable dt)
        {
            //dg.ItemsSource = DataTableConverter.Convert(dt);
            Util.GridSetData(dg, dt, FrameOperation, true);
        }

        private bool chkInputData()
        {
            bool bReturn = true;

            try
            {

                if (Util.NVC(cboEquipmentSegment.SelectedValue).Length < 1 || Util.NVC(cboEquipmentSegment.SelectedValue) =="SELECT")
                {
                    LGC.GMES.MES.ControlsLibrary.MessageBox.Show(ms.AlertRetun("SFU1223"), null, "Info", MessageBoxButton.OK, MessageBoxIcon.Warning);
                    //라인을 선택 하세요.
                    bReturn = false;
                    cboEquipmentSegment.Focus();
                    return bReturn;
                }

                if (Util.NVC(cboPrdtClass.SelectedValue).Length < 1 || Util.NVC(cboPrdtClass.SelectedValue) == "SELECT")
                {
                    LGC.GMES.MES.ControlsLibrary.MessageBox.Show(ms.AlertRetun("MMD0036"), null, "Info", MessageBoxButton.OK, MessageBoxIcon.Warning);
                    //제품분류를 선택해 주세요.
                    bReturn = false;
                    cboPrdtClass.Focus();
                    return bReturn;
                }
                if (Util.NVC(cboPilotProc.SelectedValue).Length < 1 || Util.NVC(cboPilotProc.SelectedValue) == "SELECT")
                {
                    LGC.GMES.MES.ControlsLibrary.MessageBox.Show(ms.AlertRetun("SFU1459"), null, "Info", MessageBoxButton.OK, MessageBoxIcon.Warning);
                    //공정을 선택하세요.
                    bReturn = false;
                    cboPilotProc.Focus();
                    return bReturn;
                }

            }
            catch (Exception ex)
            {
                throw ex;
            }

            return bReturn;
        }

        #endregion

        private void CboPilotGubun_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            C1ComboBox cboAreaByAreaType = new C1ComboBox();
            cboAreaByAreaType.SelectedValue = LoginInfo.CFG_AREA_ID;

            var obj = cboPilotGubun.SelectedItem;
            System.Data.DataRowView drv = obj as System.Data.DataRowView;
            string value = drv[1].ToString();
            string value_ = cboPilotGubun.SelectedValue.ToString();

            //라인             
            C1ComboBox[] cboEquipmentSegmentParent = { cboAreaByAreaType };
            C1ComboBox[] cboLineChild = { cboProductPilot, cboPilotProc };
            String[] sParam = { value };
            _combo.SetCombo(cboEquipmentSegment, CommonCombo.ComboStatus.ALL, cbParent: cboEquipmentSegmentParent, cbChild: cboLineChild, sFilter: sParam, sCase: "PILOTLINE");

            cboPilotGubun.SelectedIndex = 0;
        }

        private void txtLotID_KeyDown(object sender, KeyEventArgs e)
        {
            try
            {
                if (e.Key == Key.Enter)
                {
                    if(txtLotID.Text != "")
                    {
                        //cell id validation//
                        //1. wip이력에서 조회 : 제공확인, 동확인
                        if (!getLotSearch())
                        {
                            Util.AlertInfo("SFU1195"); //Lot 정보가 없습니다.
                            txtLotID.Text = "";
                            txtLotID.Focus();
                            return;
                        }

                        if (dgSearchResult1.GetRowCount() > 0)
                        {
                            //string procid_cause = Util.NVC(dtLotResult.Rows[0]["PROCID_CAUSE"]);
                            string procid = Util.NVC(dtLotSearchResult.Rows[0]["PROCID"]);
                            string eqsgid = Util.NVC(dtLotSearchResult.Rows[0]["EQSGID"]);

                            if (pre_procid != procid)
                            {                               
                                ms.AlertWarning("1028", Util.GetCondition(txtLotID), pre_procid, procid); //LOT %1는 작업 시작 공정%2과 다른 공정%3에 있습니다.
                                return;
                            }

                            //if (pre_proctype != proctype)
                            //{
                            //    Util.AlertInfo("이전 투입 공정(" + pre_procid + ")과 다른 공정(" + Util.NVC(dtLotResult.Rows[0]["PROCID"]).ToString() + ")에 있는 LOT입니다.");
                            //    return;
                            //}

                            if (pre_eqsgid != eqsgid)
                            {
                                //Util.AlertInfo("이전 투입 라인(" + pre_eqsgid + ")과 다른 라인(" + eqsgid + ")에 있는 LOT입니다.");
                                ms.AlertWarning("SFU3299", pre_eqsgid, eqsgid); //입력오류 : 작업 대기중인 LOT의 라인 %1과 입력한 LOT의 라인 %2이 다릅니다.
                                return;
                            }
                        }

                        int TotalRow = dgSearchResult1.GetRowCount();

                        //2. 결과  list에 존재하는지 확인
                        for (int i = 0; i < TotalRow; i++)
                        {
                            string grid_id = DataTableConverter.GetValue(dgSearchResult1.Rows[i].DataItem, "LOTID").ToString();

                            if (txtLotID.Text == grid_id)
                            {                                
                                ms.AlertWarning("SFU1513"); //등록된 LOT ID 입니다. 확인 후 다시 등록해 주세요.

                                txtLotID.Text = "";
                                txtLotID.Focus();
                                return;
                            }
                        }

                        lotGridAdd(TotalRow); //그리드에 추가

                        DataTable dt = DataTableConverter.Convert(dgSearchResult.ItemsSource);

                        Util.SetTextBlockText_DataGridRowCount(tbSearch_cnt, Util.NVC(dt.Rows.Count));

                        txtLotID.Text = "";
                        txtLotID.Focus();
                    }
                }

            }
            catch(Exception ex)
            {
                //Util.AlertInfo(ex.Message);
                Util.MessageException(ex);
            }
        }

        private void lotGridAdd(int TotalRow)
        {
            try
            {
                if (TotalRow == 0)
                {
                    //dgSearchResult1.ItemsSource = DataTableConverter.Convert(dtLotSearchResult);
                    Util.GridSetData(dgSearchResult1, dtLotSearchResult, FrameOperation, true);
                    return;
                }
                //cell list에 추가// 
                DataGridRowAdd(dgSearchResult1);

                DataRow dr = dtLotSearchResult.Rows[0];
                DataTableConverter.SetValue(dgSearchResult1.Rows[TotalRow].DataItem, "CHK", "false");
                DataTableConverter.SetValue(dgSearchResult1.Rows[TotalRow].DataItem, "LOTID", dr["LOTID"]);
                //DataTableConverter.SetValue(dgSearchResult1.Rows[TotalRow].DataItem, "PRODNAME", dr["PRODNAME"]);
                DataTableConverter.SetValue(dgSearchResult1.Rows[TotalRow].DataItem, "PRODDESC", dr["PRODDESC"]);
                DataTableConverter.SetValue(dgSearchResult1.Rows[TotalRow].DataItem, "INSDTTM", dr["INSDTTM"]);
                DataTableConverter.SetValue(dgSearchResult1.Rows[TotalRow].DataItem, "PROCNAME", dr["PROCNAME"]);
                DataTableConverter.SetValue(dgSearchResult1.Rows[TotalRow].DataItem, "WIPSNAME", dr["WIPSNAME"]);
                DataTableConverter.SetValue(dgSearchResult1.Rows[TotalRow].DataItem, "EQSGNAME", dr["EQSGNAME"]);
                DataTableConverter.SetValue(dgSearchResult1.Rows[TotalRow].DataItem, "PRODCLASS", dr["PRODCLASS"]);
                DataTableConverter.SetValue(dgSearchResult1.Rows[TotalRow].DataItem, "WIPHOLD", dr["WIPHOLD"]);

                DataTableConverter.SetValue(dgSearchResult1.Rows[TotalRow].DataItem, "PRODID", dr["PRODID"]);
                DataTableConverter.SetValue(dgSearchResult1.Rows[TotalRow].DataItem, "PROCID", dr["PROCID"]);
                DataTableConverter.SetValue(dgSearchResult1.Rows[TotalRow].DataItem, "ROUTID", dr["ROUTID"]);
                DataTableConverter.SetValue(dgSearchResult1.Rows[TotalRow].DataItem, "FLOWID", dr["FLOWID"]);
                DataTableConverter.SetValue(dgSearchResult1.Rows[TotalRow].DataItem, "EQPTID", dr["EQPTID"]);
                DataTableConverter.SetValue(dgSearchResult1.Rows[TotalRow].DataItem, "WIPSTAT", dr["WIPSTAT"]);
                DataTableConverter.SetValue(dgSearchResult1.Rows[TotalRow].DataItem, "MODLID", dr["MODLID"]);
                DataTableConverter.SetValue(dgSearchResult1.Rows[TotalRow].DataItem, "EQSGID", dr["EQSGID"]);
                DataTableConverter.SetValue(dgSearchResult1.Rows[TotalRow].DataItem, "MODLNAME", dr["MODLNAME"]);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private void DataGridRowAdd(C1.WPF.DataGrid.C1DataGrid dg)
        {
            dg.BeginNewRow();
            dg.EndNewRow(true);
        }

        private bool getLotSearch()
        {
            try
            {
                DataTable inDataTable = new DataTable();

                inDataTable.Columns.Add("LANGID", typeof(string));
                inDataTable.Columns.Add("LOTID", typeof(string));                               

                DataRow searchCondition = inDataTable.NewRow();
                searchCondition["LANGID"] = LoginInfo.LANGID;
                searchCondition["LOTID"] = Util.GetCondition(txtLotID);

                inDataTable.Rows.Add(searchCondition);

                //dtLotSearchResult = new ClientProxy().ExecuteServiceSync("DA_PAR_SEL_PILOTLOT_SINGLE_LOT_SEARCH", "INDATA", "OUTDATA", inDataTable);
                dtLotSearchResult = new ClientProxy().ExecuteServiceSync("BR_PRD_SEL_PILOTLOT_SINGLE_LOT_SEARCH", "INDATA", "OUTDATA", inDataTable);
                
                if (dtLotSearchResult.Rows.Count > 0)
                {
                    if (dgSearchResult1.GetRowCount() == 0)
                    {
                        //pre_procid_cause = Util.NVC(dtLotResult.Rows[0]["PROCID_CAUSE"]);
                        //pre_proctype = Util.NVC(dtLotResult.Rows[0]["PROCTYPE"]);
                        pre_procid = Util.NVC(dtLotSearchResult.Rows[0]["PROCID"]);
                        pre_eqsgid = Util.NVC(dtLotSearchResult.Rows[0]["EQSGID"]);
                        //string sRoutID_Cause = Util.NVC(dtLotResult.Rows[0]["ROUTID"]);
                        //string sFlowID_Cause = Util.NVC(dtLotResult.Rows[0]["FLOWID"]);

                        //string PROCNAME_CAUSE = Util.NVC(dtLotResult.Rows[0]["PROCNAME_CAUSE"]);
                        string EQSGNAME = Util.NVC(dtLotSearchResult.Rows[0]["EQSGNAME"]);
                        string PROCNAME = Util.NVC(dtLotSearchResult.Rows[0]["PROCNAME"]);

                        string status_text = string.Empty;
                        string statusvalue_kr = string.Empty;

                        status_text = "==> " + ObjectDic.Instance.GetObjectName("라인") + " : " + EQSGNAME + " / ";
                        status_text += ObjectDic.Instance.GetObjectName("공정") + " : " + PROCNAME + " /<==";
                        //status_text += "상태 : " + statusvalue + "(" + statusvalue_kr + ") <==";

                        tbState.Text = ObjectDic.Instance.GetObjectName(status_text);
                    }

                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private void btnSelectCancel1_Click(object sender, RoutedEventArgs e)
        {
            if (dgSearchResult1.ItemsSource != null)
            {
                for (int i = dgSearchResult1.GetRowCount(); 0 < i; i--)
                {
                    var chkYn = DataTableConverter.GetValue(dgSearchResult1.Rows[i - 1].DataItem, "CHK");
                    var lot_id = DataTableConverter.GetValue(dgSearchResult1.Rows[i - 1].DataItem, "LOTID");

                    if (chkYn == null)
                    {
                        dgSearchResult1.RemoveRow(i - 1);
                    }
                    else if (Convert.ToBoolean(chkYn))
                    {
                        dgSearchResult1.EndNewRow(true);
                        dgSearchResult1.RemoveRow(i - 1);
                    }
                }

                if (dgSearchResult1.GetRowCount() == 0)
                {
                    tbState.Text = "";
                    lot_fullCheck = false;
                    btnAllSelect1.Content = ObjectDic.Instance.GetObjectName("전체선택");
                    btnAllSelect1.Foreground = Brushes.White;
                }

                DataTable dt = DataTableConverter.Convert(dgSearchResult1.ItemsSource);

                Util.SetTextBlockText_DataGridRowCount(tbSearch_cnt1, Util.NVC(dt.Rows.Count));
            }
        }

        
    }
}
