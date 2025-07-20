/*************************************************************************************
 Created Date : 2020.06.12
      Creator : 김길용A
   Decription : 
--------------------------------------------------------------------------------------
 [Change History]
  2016.06.16  DEVELOPER : Initial Created.

**************************************************************************************/
using System;
using System.Collections.Generic;
using System.Data;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Linq;
using System.Management;
using System.Configuration;
using System.IO;
using System.IO.Ports;
using C1.WPF;
using C1.WPF.DataGrid;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using LGC.GMES.MES.CMM001.Popup;

namespace LGC.GMES.MES.PACK001
{
    public partial class PACK001_063 : UserControl, IWorkArea
    {
        public IFrameOperation FrameOperation
        {
            get;
            set;
        }

        #region Declaration & Constructor 
        public PACK001_063()
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
                cboTimeFrom.SelectedDateTime = (DateTime)System.DateTime.Now.AddDays(-7);
                cboTimeTo.SelectedDateTime = (DateTime)System.DateTime.Now;


                setComboBox();

                tbLotListCount.Text = "[ 0 " + ObjectDic.Instance.GetObjectName("건") + " ]";

                this.Loaded -= new System.Windows.RoutedEventHandler(this.UserControl_Loaded);
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
                    SetcboProduct();
                    cboProduct.isAllUsed = true;
                }));
            }
            catch
            {
            }
        }

        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (cboAreaByAreaType.SelectedItemsToString == "")
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
                //2022.04.10
                if ((!string.IsNullOrEmpty(Convert.ToString(this.txtUser.Text)) &&  string.IsNullOrEmpty(Convert.ToString(this.txtUser.Tag))) ||
                    ( string.IsNullOrEmpty(Convert.ToString(this.txtUser.Text)) && !string.IsNullOrEmpty(Convert.ToString(this.txtUser.Tag)))
                   )
                {
                    Util.Alert("SFU4595");  //작업자를 입력하세요
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


        #endregion

        #region Mehod
        private void setComboBox()
        {
            try
            {
                CommonCombo _combo = new CommonCombo();


                SetAreaByAreaType();
                cboAreaByAreaType.isAllUsed = true;
                SetcboWipState();
                SetcboSocode();
                this.cboListCount.SelectedValueChanged -= new System.EventHandler<C1.WPF.PropertyChangedEventArgs<object>>(this.cboListCount_SelectedValueChanged);
                Util.Set_Pack_cboListCoount(cboListCount, "CBO_NAME", "CBO_CODE", 500, 10000, 500);
                this.cboListCount.SelectedValueChanged += new System.EventHandler<C1.WPF.PropertyChangedEventArgs<object>>(this.cboListCount_SelectedValueChanged);

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
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                // LOT 생성일자가 아닌 LOT 등록일자로 조회 되도록 변경 C20220118-000456
                //RQSTDT.Columns.Add("WIPSDTTM_FROM", typeof(string));
                //RQSTDT.Columns.Add("WIPSDTTM_TO", typeof(string));
                RQSTDT.Columns.Add("UPDDTTM_FROM", typeof(string));
                RQSTDT.Columns.Add("UPDDTTM_TO", typeof(string));
                RQSTDT.Columns.Add("AREAID", typeof(string));
                RQSTDT.Columns.Add("EQSGID", typeof(string));
                RQSTDT.Columns.Add("PROCID", typeof(string));
                RQSTDT.Columns.Add("PRODID", typeof(string));
                RQSTDT.Columns.Add("COUNT", typeof(Int64));
                RQSTDT.Columns.Add("WIPSTAT", typeof(string));
                RQSTDT.Columns.Add("CHG_TYPE", typeof(string));
                RQSTDT.Columns.Add("MODLID", typeof(string));
                RQSTDT.Columns.Add("UPDUSER", typeof(string));


                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                // LOT 생성일자가 아닌 LOT 등록일자로 조회 되도록 변경 C20220118-000456
                //dr["WIPSDTTM_FROM"] = cboTimeFrom.SelectedDateTime.ToString("yyyy-MM-dd");
                //dr["WIPSDTTM_TO"] = cboTimeTo.SelectedDateTime.ToString("yyyy-MM-dd");
                dr["UPDDTTM_FROM"] = cboTimeFrom.SelectedDateTime.ToString("yyyy-MM-dd");
                dr["UPDDTTM_TO"] = cboTimeTo.SelectedDateTime.ToString("yyyy-MM-dd");
                dr["AREAID"] = cboAreaByAreaType.SelectedItemsToString;
                dr["EQSGID"] = cboEquipmentSegment.SelectedItemsToString;
                dr["PROCID"] = Util.NVC(cboProcess.SelectedItemsToString) == "" ? null : cboProcess.SelectedItemsToString;
                dr["PRODID"] = Util.NVC(cboProduct.SelectedItemsToString) == "" ? null : cboProduct.SelectedItemsToString;
                dr["COUNT"] = cboListCount.SelectedValue;
                dr["WIPSTAT"] = Util.NVC(cboWipState.SelectedItemsToString) == "" ? null : cboWipState.SelectedItemsToString;
                dr["CHG_TYPE"] = Util.NVC(cboSocode.SelectedItemsToString) == "" ? null : cboSocode.SelectedItemsToString;
                dr["MODLID"] = Util.NVC(cboProductModel.SelectedItemsToString) == "" ? null : cboProductModel.SelectedItemsToString;
                dr["UPDUSER"] = string.IsNullOrEmpty(Convert.ToString(this.txtUser.Tag)) ? null : this.txtUser.Tag.ToString();
                RQSTDT.Rows.Add(dr);

                loadingIndicator.Visibility = Visibility.Visible;
                new ClientProxy().ExecuteService("DA_PRD_SEL_TB_SFC_AFTER_SHIP_WIP_MULTI_HIST", "RQSTDT", "RSLTDT", RQSTDT, (dtResult, ex) =>
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
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private bool Check_Select()
        {
            bool bReturn = true;
            try
            {
                //라인선택확인
                if (Util.NVC(cboEquipmentSegment.SelectedItemsToString) == "SELECT")
                {
                    Util.Alert("SFU1223");  //라인을 선택하세요.
                    bReturn = false;
                    cboEquipmentSegment.Focus();
                    return bReturn;
                }
                //라인선택확인
                if (Util.NVC(cboProcess.SelectedItemsToString) == "SELECT")
                {
                    Util.Alert("SFU1459");  //공정을 선택하세요.
                    bReturn = false;
                    cboProcess.Focus();
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
            catch (Exception ex)
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

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["EQSGID"] = cboEquipmentSegment.SelectedItemsToString;
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
            catch (Exception ex)
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
            catch (Exception ex)
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
                dr["CMCDTYPE"] = "PACK_WIPSTAT_LOTSEARCH";
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
                    }
                    else if (Util.NVC(dtResult.Rows[i]["CBO_CODE"]) == "WAIT"
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

        private void SetcboSocode()
        {
            try
            {
                string sSelectedValue = cboSocode.SelectedItemsToString;
                string[] sSelectedList = sSelectedValue.Split(',');

                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("CMCDTYPE", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["CMCDTYPE"] = "PACK_UI_INFO_AFT_CHG_SHIP_HIST";
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_COMMCODE_CBO", "RQSTDT", "RSLTDT", RQSTDT);

                cboSocode.ItemsSource = DataTableConverter.Convert(dtResult);

                for (int i = 0; i < dtResult.Rows.Count; i++)
                {
                    if (sSelectedList.Length > 0 && sSelectedList[0] != "")
                    {
                        for (int j = 0; j < sSelectedList.Length; j++)
                        {
                            if (Util.NVC(dtResult.Rows[i]["CBO_CODE"]) == sSelectedList[j])
                            {
                                cboSocode.Check(i);
                                break;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
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
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void dgWipList_LoadedCellPresenter(object sender, DataGridCellEventArgs e)
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
                    //string sPRODDESC = Util.NVC(DataTableConverter.GetValue(dgWipList.Rows[cell.Row.Index].DataItem, "PRODDESC"));
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

        private void txtUser_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                GetUserWindow();
            }
        }

        private void btnUser_Click(object sender, RoutedEventArgs e)
        {
            GetUserWindow();
        }

        private void GetUserWindow()
        {
            CMM_PERSON wndPerson = new CMM_PERSON { FrameOperation = FrameOperation };

            object[] parameters = new object[1];
            string userName = this.txtUser.Text;

            parameters[0] = userName;
            C1WindowExtension.SetParameters(wndPerson, parameters);

            wndPerson.Closed += new EventHandler(wndUser_Closed);
            grdMain.Children.Add(wndPerson);
            wndPerson.BringToFront();
        }

        private void wndUser_Closed(object sender, EventArgs e)
        {
            CMM_PERSON wndPerson = sender as CMM_PERSON;
            if (wndPerson != null && wndPerson.DialogResult == MessageBoxResult.OK)
            {
                txtUser.Text = wndPerson.USERNAME;
                txtUser.Tag = wndPerson.USERID;
            }
            else
            {
                txtUser.Text = string.Empty;
                txtUser.Tag = string.Empty;
            }
        }
    }
}