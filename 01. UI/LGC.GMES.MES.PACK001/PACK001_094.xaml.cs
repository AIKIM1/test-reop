/*************************************************************************************
 Created Date : 2016.06.16
      Creator : 
   Decription : 
--------------------------------------------------------------------------------------
 [Change History]
  2016.06.16  DEVELOPER : Initial Created.
  2018.06.29  손우석  여러 장 바코드 발행시 SLEEP 추가
  2018.10.17  손우석  Slow 쿼리 처리를 위한 파라미터 수정
  2020.04.03  염규범  Keypart 시리얼 번호로 검색 가능하도록 변경 처리의 건
**************************************************************************************/

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using C1.WPF;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.CMM001.Popup;
using LGC.GMES.MES.Common;

namespace LGC.GMES.MES.PACK001
{
    public partial class PACK001_094 : UserControl, IWorkArea
    {
        /// <summary>
        /// Frame과 상호작용하기 위한 객체
        /// </summary>
        public IFrameOperation FrameOperation
        {
            get;
            set;
        }
        string now_labelcode = string.Empty;
        C1.WPF.DataGrid.DataGridRowHeaderPresenter pre = new C1.WPF.DataGrid.DataGridRowHeaderPresenter()
        {
            Background = new SolidColorBrush(Colors.Transparent),
            MouseOverBrush = new SolidColorBrush(Colors.Transparent),
        };

        CheckBox chkAll = new CheckBox()
        {
            Content = "",
            IsChecked = true,
            Background = new SolidColorBrush(Colors.Transparent),
            VerticalAlignment = System.Windows.VerticalAlignment.Center,
            HorizontalAlignment = System.Windows.HorizontalAlignment.Center
        };
        



        #region Declaration & Constructor 
        LGC.GMES.MES.PACK001.Class.MESSAGE_PARAM ms = new LGC.GMES.MES.PACK001.Class.MESSAGE_PARAM();
        public PACK001_094()
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
                ShowLoadingIndicator();
                //2018.04.10
                getWipList();
                HiddenLoadingIndicator();
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

        private void txtScanLotID_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                ShowLoadingIndicator();
                getWIPLOT(txtScanLotID.Text.Trim());
                HiddenLoadingIndicator();
            }
        }

        private void txtScanLotID_PreviewKeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == Key.V && Keyboard.IsKeyDown(Key.LeftCtrl))
            {
                try
                {
                    ShowLoadingIndicator();

                    string[] stringSeparators = new string[] { "\r\n" };
                    string sPasteString = Clipboard.GetText();
                    string[] sPasteStrings = sPasteString.Split(stringSeparators, StringSplitOptions.None);
                    string sParam = string.Empty;



                    for (int i = 0; i < sPasteStrings.Length; i++)
                    {
                        if (!string.IsNullOrWhiteSpace(sPasteStrings[i]))
                            sParam = sParam + string.Format(@"{0},", sPasteStrings[i]);
                    }
                    getWIPLOT(sParam.Trim());
                    HiddenLoadingIndicator();

                }
                catch (Exception ex)
                {
                    Util.MessageException(ex);
                    return;
                }
                finally
                {

                }

                e.Handled = true;
            }
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

        #region Mehod

        private void setComboBox()
        {
            try
            {
                CommonCombo _combo = new CommonCombo();
               

                SetAreaByAreaType();
                cboAreaByAreaType.isAllUsed = true;
                this.cboListCount.SelectedValueChanged -= new System.EventHandler<C1.WPF.PropertyChangedEventArgs<object>>(this.cboListCount_SelectedValueChanged);
                Util.Set_Pack_cboListCoount(cboListCount, "CBO_NAME", "CBO_CODE", 1000, 10000, 1000);
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
                //DA_PRD_SEL_WIP

                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("PRODID", typeof(string));
                RQSTDT.Columns.Add("STDTTM", typeof(string));
                RQSTDT.Columns.Add("EDDTTM", typeof(string));
                RQSTDT.Columns.Add("PROCID", typeof(string));
                RQSTDT.Columns.Add("PRDT_CLSS_CODE", typeof(string));
                RQSTDT.Columns.Add("EQSGID", typeof(string));
                RQSTDT.Columns.Add("AREAID", typeof(string));
                RQSTDT.Columns.Add("MODLID", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["PRODID"] = Util.NVC(cboProduct.SelectedItemsToString) == "" ? null : cboProduct.SelectedItemsToString;
                dr["STDTTM"] = dtpDateFrom.SelectedDateTime.ToString("yyyy-MM-dd") + " " + cboTimeFrom.SelectedValue.ToString();
                dr["EDDTTM"] = dtpDateTo.SelectedDateTime.ToString("yyyy-MM-dd") + " " + cboTimeTo.SelectedValue.ToString();
                dr["PROCID"] = Util.NVC(cboProcess.SelectedItemsToString) == "" ? null : cboProcess.SelectedItemsToString;
                dr["PRDT_CLSS_CODE"] = Util.NVC(cboPrdtClass.SelectedItemsToString) == "" ? null : cboPrdtClass.SelectedItemsToString;
                dr["AREAID"] = cboAreaByAreaType.SelectedItemsToString;
                dr["EQSGID"] = cboEquipmentSegment.SelectedItemsToString;
                dr["MODLID"] = Util.NVC(cboProductModel.SelectedItemsToString) == "" ? null : cboProductModel.SelectedItemsToString;

                RQSTDT.Rows.Add(dr);

                new ClientProxy().ExecuteService("DA_PRD_SEL_WIPDATACOLLECT_AND_ATTACH_PR_LOTID", "RQSTDT", "RSLTDT", RQSTDT, (dtResult, ex) =>
                {

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

        private void getWIPLOT(string sLotID)
        {
            try
            {
                //DA_PRD_SEL_WIPDATACOLLECT_AND_ATTACH_PR_LOTID

                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("LOTID", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["LOTID"] = sLotID;
                RQSTDT.Rows.Add(dr);

                
                new ClientProxy().ExecuteService("DA_PRD_SEL_WIPDATACOLLECT_AND_ATTACH_PR_LOTID", "RQSTDT", "RSLTDT", RQSTDT, (dtResult, ex) =>
                {
    

                    if (ex != null)
                    {
                        Util.MessageException(ex);
                        return;
                    }
                    Util.GridSetData(dgWipList, dtResult, FrameOperation, true);
                    Util.SetTextBlockText_DataGridRowCount(tbLotListCount, Util.NVC(dgWipList.Rows.Count));
                });
            }
            catch (Exception ex)
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
                RQSTDT.Columns.Add("AREA_TYPE_CODE", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["EQSGID"] = cboEquipmentSegment.SelectedItemsToString;
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
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        
        #endregion
    }
}
