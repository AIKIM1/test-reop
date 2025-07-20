/*************************************************************************************
 Created Date : 2016.08.00
      Creator : Jeong HyeonSik
   Decription : 
--------------------------------------------------------------------------------------
 [Change History]
  2016.06.16  DEVELOPER : Initial Created.
  2018.04.10 손우석  상태 미 선택후 조회시 메시지 추가
**************************************************************************************/

using C1.WPF;
using C1.WPF.C1Chart;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using System;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace LGC.GMES.MES.COM001
{
    public partial class COM001_022 : UserControl, IWorkArea
    {
        /// <summary>
        /// Frame과 상호작용하기 위한 객체
        /// </summary>
        public IFrameOperation FrameOperation
        {
            get;
            set;
        }

        #region Declaration & Constructor 
        string now_labelcode = string.Empty;

        public COM001_022()
        {
            InitializeComponent();
        }


        #endregion

        #region Initialize

        #endregion

        #region Event

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                dtpDateFrom.SelectedDateTime = (DateTime)System.DateTime.Now.AddDays(-7);
                dtpDateTo.SelectedDateTime = (DateTime)System.DateTime.Now;

                Util.Set_Pack_cboTimeList(cboTimeFrom, "CBO_NAME", "CBO_CODE", "06:00:00");
                Util.Set_Pack_cboTimeList(cboTimeTo, "CBO_NAME", "CBO_CODE", "23:59:59");

                setComboBox();

                tbLotListCount.Text = "[ 0 " + ObjectDic.Instance.GetObjectName("건") + " ]";                

                this.Loaded -= new System.Windows.RoutedEventHandler(this.UserControl_Loaded);
            }
            catch (Exception ex)
            {
                Util.Alert(ex.ToString());
            }
        }

        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if(cboAreaByAreaType.SelectedItemsToString == "")
                {
                    Util.Alert("SFU1499");  //동을 선택하세요.
                    return;
                }

                if (cboEquipmentSegment.SelectedItemsToString == "")
                {
                    Util.Alert("SFU1223");  //라인을 선택하세요.
                    return;
                }

                //2018.04.10
                if (cboWipState.SelectedItemsToString == "")
                {
                    Util.Alert("SFU1438");  //WIP 상태를 선택하세요.
                    return;
                }
                //2018.04.10
                getWipList();
            }
            catch (Exception ex)
            {
                //Util.Alert(ex.ToString());
                Util.MessageException(ex);
            }
        }

        private void btnExcel_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                new LGC.GMES.MES.Common.ExcelExporter().Export(dgWipList);
            }
            catch (Exception ex)
            {
                Util.Alert(ex.ToString());
            }
        }

        private void btnLabel_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if(Util.NVC(cboLabelCode.SelectedValue) == "" || Util.NVC(cboLabelCode.SelectedValue) == "SELECT")
                {
                    Util.Alert("SFU1637");
                }
                else
                {
                    now_labelcode = Util.NVC(cboLabelCode.SelectedValue);

                    //Util.printLabel_Pack(FrameOperation, loadingIndicator, txtLabelLot.Text, "PACK", Util.NVC(cboLabelCode.SelectedValue), "N", "1");
                    Util.printLabel_Pack(FrameOperation, loadingIndicator, txtLabelLot.Text, LABEL_TYPE_CODE.PACK, Util.NVC(cboLabelCode.SelectedValue), "N", "1", txtLabelProdID.Text);
                }
            }
            catch (Exception ex)
            {
                Util.Alert(ex.ToString());
            }
        }

        private void dgWipList_LoadedCellPresenter(object sender, C1.WPF.DataGrid.DataGridCellEventArgs e)
        {
            try
            {
                C1.WPF.DataGrid.C1DataGrid dataGrid = sender as C1.WPF.DataGrid.C1DataGrid;

                dataGrid.Dispatcher.BeginInvoke(new System.Action(() =>
                {
                    if (e.Cell.Presenter == null)
                    {
                        return;
                    }
                    if (e.Cell.Column.Name == "LOTID")
                    {
                        //e.Cell.Presenter.Background = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#FFDC143C"));
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

        private void dgWipList_MouseUp(object sender, MouseButtonEventArgs e)
        {
            try
            {
                Point pnt = e.GetPosition(null);
                C1.WPF.DataGrid.DataGridCell cell = dgWipList.GetCellFromPoint(pnt);

                if (cell != null)
                {
                    string sLOTID = Util.NVC(DataTableConverter.GetValue(dgWipList.Rows[cell.Row.Index].DataItem, "LOTID"));
                    string sPRODID = Util.NVC(DataTableConverter.GetValue(dgWipList.Rows[cell.Row.Index].DataItem, "PRODID"));
                    //string sPRODNAME = Util.NVC(DataTableConverter.GetValue(dgWipList.Rows[cell.Row.Index].DataItem, "PRODNAME"));
                    string sPRODDESC = Util.NVC(DataTableConverter.GetValue(dgWipList.Rows[cell.Row.Index].DataItem, "PRODDESC"));

                    txtLabelLot.Text = sLOTID;
                    txtLabelProdName.Text = sPRODDESC;
                    txtLabelProdID.Text = sPRODID;

                    setLabelCode(sPRODID);
                }
            }
            catch (Exception ex)
            {
                Util.Alert(ex.ToString());
            }
        }

        private void dgWipList_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            try
            {
                Point pnt = e.GetPosition(null);
                C1.WPF.DataGrid.DataGridCell cell = dgWipList.GetCellFromPoint(pnt);

                if (cell != null)
                {
                    if (cell.Column.Name == "LOTID")
                    {
                        this.FrameOperation.OpenMenu("SFU010090090", true, cell.Text);
                    }
                }
            }
            catch (Exception ex)
            {
                Util.Alert(ex.ToString());
            }
        }

        private void chkToday_Checked(object sender, RoutedEventArgs e)
        {
            try
            {
                dtpDateFrom.SelectedDateTime = (DateTime)System.DateTime.Now;
                dtpDateTo.SelectedDateTime = (DateTime)System.DateTime.Now;
                cboTimeTo.SelectedValue = "23:59:59";
                dtpDateFrom.IsEnabled = false;
                dtpDateTo.IsEnabled = false;
            }
            catch (Exception ex)
            {
                Util.Alert(ex.ToString());
            }
        }
        private void chkToday_Unchecked(object sender, RoutedEventArgs e)
        {
            try
            {
                dtpDateFrom.IsEnabled = true;
                dtpDateTo.IsEnabled = true;
            }
            catch (Exception ex)
            {
                Util.Alert(ex.ToString());
            }
        }

        private void cboListCount_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            btnSearch_Click(null, null);
        }

        private void cboAreaByAreaType_SelectionChanged(object sender, EventArgs e)
        {
            try
            {
                this.Dispatcher.BeginInvoke(new System.Action(() =>
                {
                    cboEquipmentSegment.isAllUsed = true;
                    SetCboEQSG();
                    SetcboProcess();
                    SetcboProductModel();
                    SetcboPrdtClass();
                    SetcboProduct();
                    cboProductModel.isAllUsed = true;
                    cboProduct.isAllUsed = true;
                }));
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private void cboEquipmentSegment_SelectionChanged(object sender, EventArgs e)
        {
            try
            {
                this.Dispatcher.BeginInvoke(new System.Action(() =>
                {
                    SetcboProcess();
                    SetcboProductModel();
                    SetcboPrdtClass();
                    SetcboProduct();
                    cboProcess.isAllUsed = true;
                    cboProductModel.isAllUsed = true;
                    cboProduct.isAllUsed = true;
                }));
            }
            catch
            {
            }
        }

        private void cboProcess_SelectionChanged(object sender, EventArgs e)
        {
            try
            {
                this.Dispatcher.BeginInvoke(new System.Action(() =>
                {
                    SetcboProductModel();
                    SetcboPrdtClass();
                    SetcboProduct();
                    cboProductModel.isAllUsed = true;
                    cboProduct.isAllUsed = true;
                }));
            }
            catch
            {
            }
        }
        private void cboProductModel_SelectionChanged(object sender, EventArgs e)
        {
            try
            {
                this.Dispatcher.BeginInvoke(new System.Action(() =>
                {
                    SetcboPrdtClass();
                    SetcboProduct();
                    cboProduct.isAllUsed = true;
                }));
            }
            catch
            {
            }
        }
        private void cboPrdtClass_SelectionChanged(object sender, EventArgs e)
        {
            try
            {
                this.Dispatcher.BeginInvoke(new System.Action(() =>
                {
                    SetcboProduct();
                    cboProduct.isAllUsed = true;
                }));
            }
            catch
            {
            }
        }
        #endregion

        #region Mehod

        private void setComboBox()
        {
            try
            {
                CommonCombo _combo = new CommonCombo();
                /*
                //동
                String[] sFilter = { LoginInfo.CFG_SHOP_ID, Area_Type.PACK };
                C1ComboBox[] cboAreaChild = { cboEquipmentSegment };
                _combo.SetCombo(cboAreaByAreaType, CommonCombo.ComboStatus.SELECT, cbChild: cboAreaChild, sFilter: sFilter);

                //라인
                C1ComboBox[] cboLineParent = { cboAreaByAreaType };
                C1ComboBox[] cboLineChild = { cboProcess , cboProductModel };
                _combo.SetCombo(cboEquipmentSegment, CommonCombo.ComboStatus.SELECT, cbChild: cboLineChild, cbParent: cboLineParent);

                //공정
                C1ComboBox[] cbProcessParent = { cboEquipmentSegment };
                C1ComboBox[] cbProcessChild = { cboProduct };
                _combo.SetCombo(cboProcess, CommonCombo.ComboStatus.ALL, cbParent: cbProcessParent, cbChild: cbProcessChild);

                //모델
                C1ComboBox[] cboProductModelChild = { cboPrdtClass, cboProduct };
                C1ComboBox[] cboProductModelParent = { cboAreaByAreaType, cboEquipmentSegment };
                //_combo.SetCombo(cboProductModel, CommonCombo.ComboStatus.ALL, cbChild: cboProductModelChild, cbParent: cboProductModelParent, sCase: "PRJ_MODEL"); //CELL,CMA,BMA 모델 전체 나옴
                _combo.SetCombo(cboProductModel, CommonCombo.ComboStatus.ALL, cbChild: cboProductModelChild, cbParent: cboProductModelParent, sCase: "PRJ_MODEL_AUTH"); //CMA,BMA 모델만


                C1ComboBox cboSHOPID = new C1ComboBox();
                cboSHOPID.SelectedValue = LoginInfo.CFG_SHOP_ID;
                C1ComboBox cboAREA_TYPE_CODE = new C1ComboBox();
                cboAREA_TYPE_CODE.SelectedValue = "P";
                C1ComboBox cboPRODID = new C1ComboBox();
                cboPRODID.SelectedValue = null;

                //제품 CLASS TYPE : CELL CMA BMA
                C1ComboBox[] ccboPrdtClassChild = { cboProduct };
                C1ComboBox[] cboPrdtClassParent = { cboSHOPID, cboAreaByAreaType, cboEquipmentSegment, cboAREA_TYPE_CODE };
                _combo.SetCombo(cboPrdtClass, CommonCombo.ComboStatus.ALL, cbChild: ccboPrdtClassChild, cbParent: cboPrdtClassParent);

                //제품
                C1ComboBox[] cboProductParent = { cboSHOPID, cboAreaByAreaType, cboEquipmentSegment, cboProcess, cboProductModel, cboAREA_TYPE_CODE, cboPrdtClass };
                _combo.SetCombo(cboProduct, CommonCombo.ComboStatus.ALL, cbParent: cboProductParent);
                */

                SetAreaByAreaType();
                cboAreaByAreaType.isAllUsed = true;
                SetcboWipState();
                this.cboListCount.SelectedValueChanged -= new System.EventHandler<C1.WPF.PropertyChangedEventArgs<object>>(this.cboListCount_SelectedValueChanged);
                Util.Set_Pack_cboListCoount(cboListCount, "CBO_NAME", "CBO_CODE", 1000, 10000, 1000);
                this.cboListCount.SelectedValueChanged += new System.EventHandler<C1.WPF.PropertyChangedEventArgs<object>>(this.cboListCount_SelectedValueChanged);
            }
            catch(Exception ex)
            {
                throw ex;
            }
        }

        private void setLabelCode(string sProdID)
        {
            try
            {
                if (sProdID.Length > 0)
                {
                    CommonCombo _combo = new CommonCombo();
                    //라벨 세팅
                    String[] sFilter = { sProdID, null ,null ,LABEL_TYPE_CODE.PACK};
                    _combo.SetCombo(cboLabelCode, CommonCombo.ComboStatus.SELECT, sFilter: sFilter, sCase: "LABELCODE_BY_PROD");

                    int combo_cnt = cboLabelCode.Items.Count;

                    for (int i = 0; i < combo_cnt; i++)
                    {
                        DataRowView drv = cboLabelCode.Items[i] as DataRowView;
                        string temp = drv["CBO_CODE"].ToString();

                        if (now_labelcode == temp)
                        {
                            cboLabelCode.SelectedValue = now_labelcode;
                            break;
                        }
                        else
                        {
                            cboLabelCode.SelectedIndex = 0;
                        }
                    }

                }
                else
                {
                    cboLabelCode.ItemsSource = null;
                    cboLabelCode.SelectedValue = null;
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        private void getWipList()
        {
            try
            {
                //DA_PRD_SEL_WIP

                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("WIPSDTTM_FROM", typeof(string));
                RQSTDT.Columns.Add("WIPSDTTM_TO", typeof(string));
                RQSTDT.Columns.Add("AREAID", typeof(string));
                RQSTDT.Columns.Add("EQSGID", typeof(string));
                RQSTDT.Columns.Add("PROCID", typeof(string));
                RQSTDT.Columns.Add("MODLID", typeof(string));
                RQSTDT.Columns.Add("PRDT_CLSS_CODE", typeof(string));
                RQSTDT.Columns.Add("PRODID", typeof(string));
                RQSTDT.Columns.Add("COUNT", typeof(Int64));
                RQSTDT.Columns.Add("WIPSTAT", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["WIPSDTTM_FROM"] = dtpDateFrom.SelectedDateTime.ToString("yyyy-MM-dd") + " " + cboTimeFrom.SelectedValue.ToString();
                dr["WIPSDTTM_TO"] = dtpDateTo.SelectedDateTime.ToString("yyyy-MM-dd") + " " + cboTimeTo.SelectedValue.ToString();
                dr["AREAID"] = cboAreaByAreaType.SelectedItemsToString;
                dr["EQSGID"] = cboEquipmentSegment.SelectedItemsToString;
                dr["PROCID"] = Util.NVC(cboProcess.SelectedItemsToString) =="" ? null : cboProcess.SelectedItemsToString;
                dr["MODLID"] = Util.NVC(cboProductModel.SelectedItemsToString) == "" ? null : cboProductModel.SelectedItemsToString;
                dr["PRDT_CLSS_CODE"] = Util.NVC(cboPrdtClass.SelectedItemsToString) == "" ? null : cboPrdtClass.SelectedItemsToString;
                dr["PRODID"] = Util.NVC(cboProduct.SelectedItemsToString) == "" ? null : cboProduct.SelectedItemsToString;
                dr["COUNT"] = cboListCount.SelectedValue;
                dr["WIPSTAT"] = Util.NVC(cboWipState.SelectedItemsToString) == "" ? null : cboWipState.SelectedItemsToString;
                RQSTDT.Rows.Add(dr);
                
                loadingIndicator.Visibility = Visibility.Visible;
                new ClientProxy().ExecuteService("DA_PRD_SEL_WIP_MULTI_PACK", "RQSTDT", "RSLTDT", RQSTDT, (dtResult, ex) =>
                {
                    loadingIndicator.Visibility = Visibility.Collapsed;

                    if (ex != null)
                    {
                        //Util.AlertByBiz("DA_PRD_SEL_WIP_MULTI_PACK", ex.Message, ex.ToString());
                        Util.MessageException(ex);
                        return;
                    }
                    //dgWipList.ItemsSource = DataTableConverter.Convert(dtResult);
                    Util.GridSetData(dgWipList, dtResult, FrameOperation, true);
                    Util.SetTextBlockText_DataGridRowCount(tbLotListCount, Util.NVC(dgWipList.Rows.Count));
                });
            }
            catch(Exception ex)
            {
                throw ex;
            }
        }

        private void SetAreaByAreaType()
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("SHOPID", typeof(string));
                RQSTDT.Columns.Add("AREA_TYPE_CODE", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["SHOPID"] = LoginInfo.CFG_SHOP_ID;
                dr["AREA_TYPE_CODE"] = Area_Type.PACK;
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_AREA_BY_AREATYPE_CBO", "RQSTDT", "RSLTDT", RQSTDT);

                cboAreaByAreaType.ItemsSource = DataTableConverter.Convert(dtResult);

                for (int i = 0; i < dtResult.Rows.Count; i++)
                {
                    if (Util.NVC(dtResult.Rows[i]["CBO_CODE"]) == LoginInfo.CFG_AREA_ID)
                    {
                        cboAreaByAreaType.Check(i);
                    }
                }
            }
            catch (Exception ex)
            {
                //LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
                Util.MessageException(ex);
            }
        }

        private void SetCboEQSG()
        {
            try
            {
                this.cboAreaByAreaType.SelectionChanged -= new System.EventHandler(this.cboAreaByAreaType_SelectionChanged);
                string sSelectedValue = cboEquipmentSegment.SelectedItemsToString;
                string[] sSelectedList = sSelectedValue.Split(',');

                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("AREAID", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["AREAID"] = cboAreaByAreaType.SelectedItemsToString;
                RQSTDT.Rows.Add(dr);
                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_EQUIPMENTSEGMENT_MULTI_CBO", "RQSTDT", "RSLTDT", RQSTDT);
                
                cboEquipmentSegment.ItemsSource = DataTableConverter.Convert(dtResult);
                
                for (int i = 0; i < dtResult.Rows.Count; i++)
                {
                    if (sSelectedList.Length > 0 && sSelectedList[0] != "")
                    {
                        for (int j = 0; j < sSelectedList.Length; j++)
                        {
                            if (Util.NVC(dtResult.Rows[i]["CBO_CODE"]) == sSelectedList[j])
                            {
                                cboEquipmentSegment.Check(i);
                                break;
                            }
                        }
                    }
                    else if (Util.NVC(dtResult.Rows[i]["CBO_CODE"]) == LoginInfo.CFG_EQSG_ID)
                    {
                        cboEquipmentSegment.Check(i);
                        break;
                    }
                }
                this.cboAreaByAreaType.SelectionChanged += new System.EventHandler(this.cboAreaByAreaType_SelectionChanged);
            }
            catch(Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void SetcboProcess()
        {
            try
            {
                this.cboProcess.SelectionChanged -= new System.EventHandler(this.cboProcess_SelectionChanged);
                string sSelectedValue = cboProcess.SelectedItemsToString;
                string[] sSelectedList = sSelectedValue.Split(',');

                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("EQSGID", typeof(string));
                RQSTDT.Columns.Add("AREA_TYPE_CODE", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"]         = LoginInfo.LANGID;
                dr["EQSGID"]         = cboEquipmentSegment.SelectedItemsToString;
                dr["AREA_TYPE_CODE"] = Area_Type.PACK;
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_PROCESS_PACK_CBO", "RQSTDT", "RSLTDT", RQSTDT);
                
                cboProcess.ItemsSource = DataTableConverter.Convert(dtResult);

                for (int i = 0; i < dtResult.Rows.Count; i++)
                {
                    if (sSelectedList.Length > 0 && sSelectedList[0] != "")
                    {
                        for (int j = 0; j < sSelectedList.Length; j++)
                        {
                            if (Util.NVC(dtResult.Rows[i]["CBO_CODE"]) == sSelectedList[j])
                            {
                                cboProcess.Check(i);
                                break;
                            }
                        }
                    }
                    else if (Util.NVC(dtResult.Rows[i]["CBO_CODE"]) == LoginInfo.CFG_PROC_ID)
                    {
                        cboProcess.Check(i);
                        break;
                    }
                }
                this.cboProcess.SelectionChanged += new System.EventHandler(this.cboProcess_SelectionChanged);
            }
            catch(Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void SetcboProductModel()
        {
            try
            {
                this.cboProductModel.SelectionChanged -= new System.EventHandler(this.cboProductModel_SelectionChanged);

                string sSelectedValue = cboProductModel.SelectedItemsToString;
                string[] sSelectedList = sSelectedValue.Split(',');

                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("AREAID", typeof(string));
                RQSTDT.Columns.Add("EQSGID", typeof(string));
                RQSTDT.Columns.Add("SHOPID", typeof(string));
                RQSTDT.Columns.Add("SYSTEM_ID", typeof(string));
                RQSTDT.Columns.Add("USERID", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["AREAID"] = cboAreaByAreaType.SelectedItemsToString;
                dr["EQSGID"] = cboEquipmentSegment.SelectedItemsToString;
                dr["SHOPID"] = LoginInfo.CFG_SHOP_ID;
                dr["SYSTEM_ID"] = LoginInfo.SYSID;
                dr["USERID"] = LoginInfo.USERID;
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_PRJMODEL_AUTH_MULTI_CBO", "RQSTDT", "RSLTDT", RQSTDT);
                cboProductModel.ItemsSource = DataTableConverter.Convert(dtResult);
                

                for (int i = 0; i < dtResult.Rows.Count; i++)
                {
                    if (sSelectedList.Length > 0 && sSelectedList[0] != "")
                    {
                        for (int j = 0; j < sSelectedList.Length; j++)
                        {
                            if (Util.NVC(dtResult.Rows[i]["CBO_CODE"]) == sSelectedList[j])
                            {
                                cboProductModel.Check(i);
                                break;
                            }
                        }
                    }
                    else if (Util.NVC(dtResult.Rows[i]["CBO_CODE"]) == LoginInfo.CFG_EQSG_ID)
                    {
                        cboProductModel.Check(i);
                        break;
                    }
                }
                this.cboProductModel.SelectionChanged += new System.EventHandler(this.cboProductModel_SelectionChanged);
            }
            catch(Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void SetcboPrdtClass()
        {
            try
            {
                this.cboPrdtClass.SelectionChanged -= new System.EventHandler(this.cboPrdtClass_SelectionChanged);
                string sSelectedValue = cboPrdtClass.SelectedItemsToString;
                string[] sSelectedList = sSelectedValue.Split(',');

                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("SHOPID", typeof(string));
                RQSTDT.Columns.Add("AREAID", typeof(string));
                RQSTDT.Columns.Add("EQSGID", typeof(string));
                RQSTDT.Columns.Add("AREA_TYPE_CODE", typeof(string));
                RQSTDT.Columns.Add("PROCID", typeof(string));
                
                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["SHOPID"] = LoginInfo.CFG_SHOP_ID;
                dr["AREAID"] = cboAreaByAreaType.SelectedItemsToString;
                dr["EQSGID"] = cboEquipmentSegment.SelectedItemsToString;
                dr["AREA_TYPE_CODE"] = Area_Type.PACK;
                dr["PROCID"] = cboProcess.SelectedItemsToString == "" ? null : cboProcess.SelectedItemsToString;
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_PRODUCTTYPE_MULTI_CBO", "RQSTDT", "RSLTDT", RQSTDT);

                cboPrdtClass.ItemsSource = DataTableConverter.Convert(dtResult);

                for (int i = 0; i < dtResult.Rows.Count; i++)
                {
                    if (sSelectedList.Length > 0 && sSelectedList[0] != "")
                    {
                        for (int j = 0; j < sSelectedList.Length; j++)
                        {
                            if (Util.NVC(dtResult.Rows[i]["CBO_CODE"]) == sSelectedList[j])
                            {
                                cboPrdtClass.Check(i);
                                break;
                            }
                        }
                    }
                }
                this.cboPrdtClass.SelectionChanged += new System.EventHandler(this.cboPrdtClass_SelectionChanged);
            }
            catch(Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void SetcboProduct()
        {
            try
            {
                string sSelectedValue = cboProduct.SelectedItemsToString;
                string[] sSelectedList = sSelectedValue.Split(',');

                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("SHOPID", typeof(string));
                RQSTDT.Columns.Add("AREAID", typeof(string));
                RQSTDT.Columns.Add("EQSGID", typeof(string));
                RQSTDT.Columns.Add("PROCID", typeof(string));
                RQSTDT.Columns.Add("MODLID", typeof(string));
                RQSTDT.Columns.Add("AREA_TYPE_CODE", typeof(string));
                RQSTDT.Columns.Add("PRDT_CLSS_CODE", typeof(string));


                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["SHOPID"] = LoginInfo.CFG_SHOP_ID;
                dr["AREAID"] = cboAreaByAreaType.SelectedItemsToString;
                dr["EQSGID"] = cboEquipmentSegment.SelectedItemsToString;
                dr["PROCID"] = cboProcess.SelectedItemsToString == "" ? null : cboProcess.SelectedItemsToString;
                dr["MODLID"] = cboProductModel.SelectedItemsToString == "" ? null : cboProductModel.SelectedItemsToString;
                dr["AREA_TYPE_CODE"] = Area_Type.PACK;
                dr["PRDT_CLSS_CODE"] = cboPrdtClass.SelectedItemsToString == "" ? null : cboPrdtClass.SelectedItemsToString; 

                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_PRODUCT_MULTI_CBO", "RQSTDT", "RSLTDT", RQSTDT);
                cboProduct.ItemsSource = DataTableConverter.Convert(dtResult);


                for (int i = 0; i < dtResult.Rows.Count; i++)
                {
                    if (sSelectedList.Length > 0 && sSelectedList[0] != "")
                    {
                        for (int j = 0; j < sSelectedList.Length; j++)
                        {
                            if (Util.NVC(dtResult.Rows[i]["CBO_CODE"]) == sSelectedList[j])
                            {
                                cboProduct.Check(i);
                                break;
                            }
                        }
                    }
                }
            }
            catch(Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void SetcboWipState()
        {
            try
            {
                string sSelectedValue = cboWipState.SelectedItemsToString;
                string[] sSelectedList = sSelectedValue.Split(',');

                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("CMCDTYPE", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["CMCDTYPE"] = "PACK_WIPSTAT_WIPSEARCH";
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_COMMCODE_CBO", "RQSTDT", "RSLTDT", RQSTDT);

                cboWipState.ItemsSource = DataTableConverter.Convert(dtResult);

                for (int i = 0; i < dtResult.Rows.Count; i++)
                {
                    if (sSelectedList.Length > 0 && sSelectedList[0] != "")
                    {
                        for (int j = 0; j < sSelectedList.Length; j++)
                        {
                            if (Util.NVC(dtResult.Rows[i]["CBO_CODE"]) == sSelectedList[j])
                            {
                                cboWipState.Check(i);
                                break;
                            }
                        }
                    }else if(Util.NVC(dtResult.Rows[i]["CBO_CODE"]) == "WAIT" 
                        || Util.NVC(dtResult.Rows[i]["CBO_CODE"]) == "PROC")
                    {
                        cboWipState.Check(i);
                    }
                }
            }
            catch (Exception ex)
            {
                //LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
                Util.MessageException(ex);
            }
        }


        #endregion


    }
}
