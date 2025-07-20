/*************************************************************************************
 Created Date : 2016.08.01
      Creator : Jeong Hyeon Sik
   Decription : Pack Cell 등록 화면
--------------------------------------------------------------------------------------
 [Change History]
  2016.08.01  Jeong Hyeon Sik : Initial Created.





 
**************************************************************************************/

using System;
using System.Collections.Generic;
using System.Data;
using System.Text.RegularExpressions;
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
    public partial class PACK001_020 : UserControl , IWorkArea
    {
        #region Declaration & Constructor 
        LGC.GMES.MES.PACK001.Class.MESSAGE_PARAM ms = new LGC.GMES.MES.PACK001.Class.MESSAGE_PARAM();

        public IFrameOperation FrameOperation
        {
            get;
            set;
        }

        Util util = new Util();
        public PACK001_020()
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
                List<Button> listAuth = new List<Button>();
                listAuth.Add(btnTagetInputComfirm);
                Util.pageAuth(listAuth, FrameOperation.AUTHORITY);

                DateTime DateNow = DateTime.Now;
                DateTime firstOfThisMonth = new DateTime(DateNow.Year, DateNow.Month, 1);
                DateTime firstOfNextMonth = new DateTime(DateNow.Year, DateNow.Month, 1).AddMonths(1);
                DateTime lastOfThisMonth = firstOfNextMonth.AddDays(-1);

                dtpDateFrom.SelectedDateTime = firstOfThisMonth;
                dtpDateTo.SelectedDateTime = lastOfThisMonth;

                setComboBox();

                tbTagetListCount.Text = "[ 0 " + ObjectDic.Instance.GetObjectName("건") + " ]";
                tbSearchListCount.Text = "[ 0 " + ObjectDic.Instance.GetObjectName("건") + " ]";

                this.Loaded -= new System.Windows.RoutedEventHandler(this.UserControl_Loaded);
            }
            catch (Exception ex)
            {
                //LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, ControlsLibrary.MessageBoxIcon.Warning);
                Util.Alert(ex.Message);
            }
        }

        private void txtLotID_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                if (txtLotID.Text.Length > 0)
                {
                    if (chkInputLot(txtLotID.Text))
                    {
                        setDgTagetList();
                    }
                }
            }
        }

        private void btnTagetInputComfirm_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (chkInputData())
                {
                    saveTaget();

                    getSearchData();
                }
            }
            catch (Exception ex)
            {
                Util.Alert(ex.ToString());
            }
        }

        private void btnTagetCancel_Click(object sender, RoutedEventArgs e)
        {
            LGC.GMES.MES.ControlsLibrary.MessageBox.Show(ms.AlertRetun("SFU1885"), null, "Information", MessageBoxButton.OKCancel, MessageBoxIcon.None, (result) =>
            //전체 취소 하시겠습니까?
            {
                if (result == MessageBoxResult.OK)
                {
                    Refresh();
                }
            }
            );
        }

        private void btnTagetSelectCancel_Click(object sender, RoutedEventArgs e)
        {
            try
            {

                DataTable dtTempTagetList = DataTableConverter.Convert(dgTagetList.ItemsSource);

                for (int i = (dtTempTagetList.Rows.Count - 1); i >= 0; i--)
                {
                    if (Util.NVC(dtTempTagetList.Rows[i]["CHK"]) == "True" ||
                        Util.NVC(dtTempTagetList.Rows[i]["CHK"]) == "1")
                    {

                        dtTempTagetList.Rows[i].Delete();
                        dtTempTagetList.AcceptChanges();
                    }
                }
                dgTagetList.ItemsSource = DataTableConverter.Convert(dtTempTagetList);
                Util.SetTextBlockText_DataGridRowCount(tbTagetListCount, Util.NVC(dtTempTagetList.Rows.Count));

                if (!(dtTempTagetList.Rows.Count > 0))
                {
                    dgTagetList.ItemsSource = null;
                    Refresh();
                }
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
                getSearchData();
            }
            catch (Exception ex)
            {
                Util.Alert(ex.ToString());
            }
        }

        private void btnExcel_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                new LGC.GMES.MES.Common.ExcelExporter().Export(dgSearchResultList);
            }
            catch (Exception ex)
            {
                Util.Alert(ex.ToString());
            }
        }

        private void cboTagetRoute_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            try
            {
                setComboBox_Prod_schd(Util.NVC(cboTagetRoute.SelectedValue));
            }
            catch (Exception ex)
            {
                Util.Alert(ex.ToString());
            }
        }

        private void cboTagetPRODID_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            try
            {
                txtTagetPRODNAME.Text = Util.NVC(cboTagetPRODID.SelectedValue);
            }
            catch (Exception ex)
            {
                Util.Alert(ex.ToString());
            }
        }

        private void cboTagetModel_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            try
            {
                setComboBox_Route_schd(Util.NVC(cboTagetModel.SelectedValue), null);
            }
            catch (Exception ex)
            {
                Util.Alert(ex.ToString());
            }
        }

        private void dgSearchResultList_LoadedCellPresenter(object sender, C1.WPF.DataGrid.DataGridCellEventArgs e)
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

        private void dgSearchResultList_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            try
            {
                Point pnt = e.GetPosition(null);
                C1.WPF.DataGrid.DataGridCell cell = dgSearchResultList.GetCellFromPoint(pnt);

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

        #endregion

        #region Mehod

        private void setComboBox()
        {
            try
            {
                CommonCombo _combo = new CommonCombo();

                //작업자 조회 동
                String[] sFilter = { LoginInfo.CFG_SHOP_ID, Area_Type.PACK };
                C1ComboBox[] cboSearchAREAIDChild = { cboSearchEQSGID };
                _combo.SetCombo(cboSearchAREAID, CommonCombo.ComboStatus.SELECT, cbChild: cboSearchAREAIDChild, sFilter: sFilter, sCase: "AREA_AREATYPE");

                //작업자 조회 라인
                C1ComboBox[] cboSearchEQSGIDParent = { cboSearchAREAID };
                _combo.SetCombo(cboSearchEQSGID, CommonCombo.ComboStatus.ALL, cbParent: cboSearchEQSGIDParent, sCase: "EQUIPMENTSEGMENT");

                setComboBox_Model_schd();
            }
            catch(Exception ex)
            {
                throw ex;
            }
        }

        private void setComboBox_Model_schd()
        {
            try
            {

                DataTable dtIndata = new DataTable();
                dtIndata.Columns.Add("LANGID", typeof(string));
                dtIndata.Columns.Add("MTRLID", typeof(string));
                dtIndata.Columns.Add("AREAID", typeof(string));
                DataRow drIndata = dtIndata.NewRow();

                drIndata["LANGID"] = LoginInfo.LANGID;
                drIndata["MTRLID"] = null;
                drIndata["AREAID"] = LoginInfo.CFG_AREA_ID;
                dtIndata.Rows.Add(drIndata);
                new ClientProxy().ExecuteService("DA_BAS_SEL_MODLID_BY_MTRLID_MBOM_DETL_CBO", "INDATA", "OUTDATA", dtIndata, (dtResult, ex) =>
                {
                    if (ex != null)
                    {
                        //Util.AlertByBiz("DA_BAS_SEL_MODLID_BY_MTRLID_MBOM_DETL_CBO", ex.Message, ex.ToString());
                        Util.MessageException(ex);
                        return;
                    }

                    cboTagetModel.ItemsSource = DataTableConverter.Convert(dtResult);

                    if (dtIndata.Rows.Count > 0)
                    {
                        cboTagetModel.SelectedIndex = 0;
                    }
                    else
                    {
                        cboTagetModel_SelectedValueChanged(null, null);
                    }

                }
                );
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private void setComboBox_Route_schd(string sMODLID, string sMTRLID)
        {
            try
            {
                DataTable dtIndata = new DataTable();
                dtIndata.TableName = "RQSTDT";
                dtIndata.Columns.Add("LANGID", typeof(string));
                dtIndata.Columns.Add("MODLID", typeof(string));
                dtIndata.Columns.Add("MTRLID", typeof(string));
                dtIndata.Columns.Add("AREAID", typeof(string));
                DataRow drIndata = dtIndata.NewRow();

                drIndata["LANGID"] = LoginInfo.LANGID;
                drIndata["MODLID"] = sMODLID;
                drIndata["MTRLID"] = sMTRLID;
                drIndata["AREAID"] = LoginInfo.CFG_AREA_ID;
                dtIndata.Rows.Add(drIndata);
                new ClientProxy().ExecuteService("DA_BAS_SEL_ROUTID_BY_MTRLID_MBOM_DETL_CBO", "RQSTDT", "RSLTDT", dtIndata, (dtResult, ex) =>
                {
                    if (ex != null)
                    {
                        //Util.AlertByBiz("DA_BAS_SEL_ROUTID_BY_MTRLID_MBOM_DETL_CBO", ex.Message, ex.ToString());
                        Util.MessageException(ex);
                        return;
                    }

                    cboTagetRoute.ItemsSource = DataTableConverter.Convert(dtResult);

                    if (dtIndata.Rows.Count > 0)
                    {
                        cboTagetRoute.SelectedIndex = 0;
                    }
                    //else
                    //{
                    //    cboTagetRoute_SelectionChanged(sender, null);
                    //}

                }
                );
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private void setComboBox_Prod_schd(string sRoutID)
        {
            try
            {
                DataTable dtIndata = new DataTable();
                dtIndata.TableName = "RQSTDT";
                dtIndata.Columns.Add("LANGID", typeof(string));
                dtIndata.Columns.Add("ROUTID", typeof(string));
                DataRow drIndata = dtIndata.NewRow();

                drIndata["LANGID"] = LoginInfo.LANGID;
                drIndata["ROUTID"] = sRoutID;
                dtIndata.Rows.Add(drIndata);
                new ClientProxy().ExecuteService("DA_BAS_SEL_PRODUCT_BY_ROUT_CBO", "RQSTDT", "RSLTDT", dtIndata, (dtResult, ex) =>
                {
                    if (ex != null)
                    {
                        //Util.AlertByBiz("DA_BAS_SEL_PRODUCT_BY_ROUT_CBO", ex.Message, ex.ToString());
                        Util.MessageException(ex);
                        return;
                    }

                    cboTagetPRODID.ItemsSource = DataTableConverter.Convert(dtResult);

                    if (dtIndata.Rows.Count > 0)
                    {
                        cboTagetPRODID.SelectedIndex = 0;
                    }
                    //else
                    //{
                    //    cboTagetRoute_SelectionChanged(sender, null);
                    //}

                }
                );
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private bool chkInputData()
        {
            bool bReturn = true;
            if (dgTagetList.Rows.Count < 1)
            {                
                ms.AlertWarning("SFU1511"); //등록 대상이 없습니다. LOT을 입력하세요.
                bReturn = false;
                return bReturn;
            }

            //LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("LOT을 등록 하시겠습니까?\n\nMODEL : {0}\nPRODUCT : {1}", cboModel.Text, cboProduct.Text), null, "Information", MessageBoxButton.OKCancel, MessageBoxIcon.None, (sresult) =>
            //{
            //    if (sresult != MessageBoxResult.OK)
            //    {
            //        bReturn = false;
            //    }
            //    else
            //    {
            //        bReturn = true;
            //    }
            //});

            return bReturn;
        }

        private void saveTaget()
        {
            try
            {
                DataSet dsInput = new DataSet();
                DataRow drINDATA = null;
                DataTable dtINDATA = new DataTable();
                dtINDATA.TableName = "INDATA";
                dtINDATA.Columns.Add("SRCTYPE", typeof(string));
                dtINDATA.Columns.Add("LANGID", typeof(string));
                dtINDATA.Columns.Add("ROUTID", typeof(string));
                dtINDATA.Columns.Add("PRODID", typeof(string));
                dtINDATA.Columns.Add("USERID", typeof(string));

                drINDATA = dtINDATA.NewRow();
                drINDATA["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                drINDATA["LANGID"] = LoginInfo.LANGID;
                drINDATA["ROUTID"] = Util.NVC(DataTableConverter.GetValue(dgTagetList.Rows[0].DataItem, "ROUTID")); 
                drINDATA["PRODID"] = Util.NVC(DataTableConverter.GetValue(dgTagetList.Rows[0].DataItem, "PRODID"));
                drINDATA["USERID"] = LoginInfo.USERID;
                dtINDATA.Rows.Add(drINDATA);

                DataRow drINLOT = null;
                DataTable dtINLOT = new DataTable();
                dtINLOT.TableName = "INLOT";
                dtINLOT.Columns.Add("LOTID", typeof(string));
                for (int i = 0; i < dgTagetList.Rows.Count; i++)
                {
                    drINLOT = dtINLOT.NewRow();
                    drINLOT["LOTID"] = Util.NVC(DataTableConverter.GetValue(dgTagetList.Rows[i].DataItem, "LOTID"));
                    dtINLOT.Rows.Add(drINLOT);
                }

                dsInput.Tables.Add(dtINDATA);
                dsInput.Tables.Add(dtINLOT);

                new ClientProxy().ExecuteService_Multi("BR_PRD_REG_DUMMY_LOT_CREATE", "INDATA,INLOT", "OUTDATA", (dsResult, dataException) =>
                {
                    try
                    {
                        loadingIndicator.Visibility = Visibility.Collapsed;
                        if (dataException != null)
                        {
                            loadingIndicator.Visibility = Visibility.Collapsed;
                            //Util.AlertByBiz("BR_PRD_REG_DUMMY_LOT_CREATE", dataException.Message, dataException.ToString());
                            Util.MessageException(dataException);
                            return;
                        }
                        else
                        {
                            if (dsResult != null && dsResult.Tables.Count > 0)
                            {
                                if ((dsResult.Tables.IndexOf("OUTDATA") > -1))
                                {
                                    if (dsResult.Tables["OUTDATA"].Rows.Count > 0)
                                    {
                                        //등록하였습니다.
                                        LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("SFU1518"), null, "Info", MessageBoxButton.OK, MessageBoxIcon.None);
                                        Refresh();
                                    }
                                }
                            }
                            return;
                        }
                    }
                    catch (Exception ex)
                    {
                        throw ex;
                    }

                }, dsInput);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private void setDgTagetList()
        {
            try
            {
                DataTable dtINDATA = new DataTable();

                dtINDATA.TableName = "INDATA";
                dtINDATA.Columns.Add("SRCTYPE", typeof(string));
                dtINDATA.Columns.Add("LANGID", typeof(string));
                dtINDATA.Columns.Add("LOTID", typeof(string));
                dtINDATA.Columns.Add("ROUTID", typeof(string));
                dtINDATA.Columns.Add("ROUTNAME", typeof(string));
                dtINDATA.Columns.Add("PRODID", typeof(string));
                dtINDATA.Columns.Add("PRODDESC", typeof(string));
                dtINDATA.Columns.Add("PRJT_NAME", typeof(string));

                DataRow dr = dtINDATA.NewRow();
                dr["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                dr["LANGID"] = LoginInfo.LANGID;
                dr["LOTID"] = txtLotID.Text;
                dr["ROUTID"] = Util.NVC(cboTagetRoute.SelectedValue);
                dr["ROUTNAME"] = cboTagetRoute.Text;
                dr["PRODID"] = cboTagetPRODID.Text;
                dr["PRODDESC"] = txtTagetPRODNAME.Text;
                dr["PRJT_NAME"] = cboTagetModel.Text;
                dtINDATA.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("BR_PRD_CHK_DUMMY_LOT", "INDATA", "OUTDATA", dtINDATA);
                if (dtResult.Rows.Count > 0)
                {
                    if (!(Util.NVC(dtResult.Rows[0]["LOTID"]).Length > 0))
                    {                       
                        ms.AlertWarning("SFU1513"); //등록된 LOT ID 입니다. 확인 후 다시 등록해 주세요.
                    }
                }

                if (dgTagetList.ItemsSource != null)
                {
                    DataTable dtBefore = DataTableConverter.Convert(dgTagetList.ItemsSource);
                    dtBefore.Merge(dtResult);
                    dgTagetList.ItemsSource = DataTableConverter.Convert(dtBefore);
                    Util.SetTextBlockText_DataGridRowCount(tbTagetListCount, Util.NVC(dgTagetList.Rows.Count));
                    txtLotID.Text = "";
                }
                else
                {
                    dgTagetList.ItemsSource = DataTableConverter.Convert(dtResult);
                    txtLotID.Text = "";
                }


            }
            catch(Exception ex)
            {
                //Util.AlertByBiz("BR_PRD_CHK_DUMMY_LOT", ex.Message, ex.ToString());
                Util.MessageException(ex);
            }
        }

        private bool chkInputLot(string sLotID)
        {
            bool bReturn = true;

            DataTable dtBefore = DataTableConverter.Convert(dgTagetList.ItemsSource);
            if (dtBefore.Rows.Count > 0)
            {
                DataRow[] drTemp = dtBefore.Select("LOTID = '" + sLotID + "'");
                if (drTemp.Length > 0)
                {                    
                    ms.AlertWarning("SFU1376", sLotID); //LOT ID는 중복 입력할수 없습니다.\r\n({0})
                    bReturn = false;
                }
            }

            //cell 만해당... check BIZ 에 lottype CMA , CELL 의 형식체크 하는기능을 넣던지.. LOT형식 리턴받아 CELL CMA별 ID체계 확인하는것 구현해야함!!
            if (!sLotID.Equals("") && (bool)chkLot.IsChecked)
            {
                Regex rx = new Regex("[[A-Z0-9]{2}([0-9]){2}([0-9A-Z]){1}([0-9]){5}");
                if (!rx.IsMatch(sLotID) || sLotID.Length != 10)
                {                    
                    ms.AlertWarning("SFU1362"); //LOT ID 형식이 올바르지 않습니다. 확인 후 다시 등록해 주세요.
                    bReturn = false;
                }
            }


            return bReturn;
        }

        private void getSearchData()
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("AREAID", typeof(string));
                RQSTDT.Columns.Add("EQSGID", typeof(string));
                RQSTDT.Columns.Add("FROM_DATE", typeof(string));
                RQSTDT.Columns.Add("TO_DATE", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["AREAID"] = Util.NVC(cboSearchAREAID.SelectedValue) == "" ? null : Util.NVC(cboSearchAREAID.SelectedValue);
                dr["EQSGID"] = Util.NVC(cboSearchEQSGID.SelectedValue) == "" ? null : Util.NVC(cboSearchEQSGID.SelectedValue);
                dr["FROM_DATE"] = dtpDateFrom.SelectedDateTime.ToString("yyyyMMdd");
                dr["TO_DATE"] = dtpDateTo.SelectedDateTime.ToString("yyyyMMdd");
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_DUMMY_LOT_HISTORY", "RQSTDT", "RSLTDT", RQSTDT);


                //dgSearchResultList.ItemsSource = DataTableConverter.Convert(dtResult);
                Util.GridSetData(dgSearchResultList, dtResult, FrameOperation, true);
                Util.SetTextBlockText_DataGridRowCount(tbSearchListCount, Util.NVC(dtResult.Rows.Count));
            }
            catch (Exception ex)
            {
                //Util.AlertByBiz("DA_PRD_SEL_DUMMY_LOT_HISTORY", ex.Message, ex.ToString());
                Util.MessageException(ex);
            }

        }

        private void Refresh()
        {
            try
            {
                //반품목록 조회
                getSearchData();

                //그리드 clear
                Util.gridClear(dgTagetList);
                Util.SetTextBlockText_DataGridRowCount(tbTagetListCount, "0");

                txtLotID.Text = string.Empty;
                txtLotID.Focus();
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }


        #endregion

       
    }
}
