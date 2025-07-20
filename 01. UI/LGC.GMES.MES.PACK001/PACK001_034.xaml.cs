/*************************************************************************************
 Created Date : 2018.07.11
      Creator : 손홍구
   Decription : Pack Master Lot 등록 화면
--------------------------------------------------------------------------------------
 [Change History]
  2016.08.01  손홍구 : Initial Created.
  2019.06.18  김도형 : [CSR ID:4002930] Master Lot  수정/삭제 기능 보완 件 | [요청번호]C20190527_02930 | [서비스번호]4002930 
  2020.03.30  염규범 : [CSR ID:C20200229-000028] 300Dpi 라벨 모양 수정의 건
  2020.04.06  김민석 : [CSR ID:C20200406-000405] 조회 조건 변경 시 조회 버튼 사라지는 현상 개선 및 제품 조건 추가
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

namespace LGC.GMES.MES.PACK001
{
    public partial class PACK001_034 : UserControl , IWorkArea
    {
        #region Declaration & Constructor 
        LGC.GMES.MES.PACK001.Class.MESSAGE_PARAM ms = new LGC.GMES.MES.PACK001.Class.MESSAGE_PARAM();

        string now_labelcode = string.Empty;

        public IFrameOperation FrameOperation
        {
            get;
            set;
        }

        Util util = new Util();
        public PACK001_034()
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
                    if(txtLotID.Text.Contains("MASTER;"))
                    {
                        ms.AlertWarning("SFU1362"); //LOT ID 형식이 올바르지 않습니다. 확인 후 다시 등록해 주세요.
                    }
                    else if (chkInputLot(txtLotID.Text) && chkInputInfo(Util.NVC(cboTargetEQSGID.SelectedValue), Util.NVC(cboTargetROUTID.SelectedValue), Util.NVC(cboTargetPRODID.SelectedValue), Util.NVC(cboTargetPROCID.SelectedValue), Util.NVC(cboTargetEQPTID.SelectedValue)))
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


        private void cboTargetAREAID_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            try
            {
                setComboBox_EquipmentSegment_schd(Util.NVC(cboTargetAREAID.SelectedValue));
            }
            catch (Exception ex)
            {
                Util.Alert(ex.ToString());
            }
        }

        private void cboTargetEQSGID_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            try
            {
                setComboBox_Route_schd(Util.NVC(cboTargetAREAID.SelectedValue), Util.NVC(cboTargetEQSGID.SelectedValue));
            }
            catch (Exception ex)
            {
                Util.Alert(ex.ToString());
            }
        }

        private void cboTargetROUTID_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            try
            {
                setComboBox_Product_schd(Util.NVC(cboTargetROUTID.SelectedValue));
            }
            catch (Exception ex)
            {
                Util.Alert(ex.ToString());
            }
        }

        private void cboTargetPRODID_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            try
            {
                setComboBox_Process_schd(Util.NVC(cboTargetAREAID.SelectedValue), Util.NVC(cboTargetEQSGID.SelectedValue), Util.NVC(cboTargetROUTID.SelectedValue), Util.NVC(cboTargetPRODID.SelectedValue));
            }
            catch (Exception ex)
            {
                Util.Alert(ex.ToString());
            }
        }

        private void cboTargetPROCID_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            try
            {
                setComboBox_Equipment_schd(Util.NVC(cboTargetAREAID.SelectedValue), Util.NVC(cboTargetEQSGID.SelectedValue), Util.NVC(cboTargetPROCID.SelectedValue));
            }
            catch (Exception ex)
            {
                Util.Alert(ex.ToString());
            }
        }

        //private void cboSearchAREAID_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        //{
        //    try
        //    {
        //        setComboBox_EquipmentSegment_Search_schd(Util.NVC(cboSearchAREAID.SelectedValue));
        //    }
        //    catch (Exception ex)
        //    {
        //        Util.Alert(ex.ToString());
        //    }
        //}

        //private void cboSearchEQSGID_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        //{
        //    try
        //    {
        //        setComboBox_Route_Search_schd(Util.NVC(cboSearchAREAID.SelectedValue), Util.NVC(cboSearchEQSGID.SelectedValue));
        //    }
        //    catch (Exception ex)
        //    {
        //        Util.Alert(ex.ToString());
        //    }
        //}

        //private void cboSearchROUTID_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        //{
        //    setComboBox_Process_Search_schd(Util.NVC(cboSearchAREAID.SelectedValue), Util.NVC(cboSearchEQSGID.SelectedValue), Util.NVC(cboSearchROUTID.SelectedValue));
        //    setComboBox_Product_Search_schd(Util.NVC(cboSearchROUTID.SelectedValue));
        //}

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
                C1.WPF.DataGrid.DataGridCell Lot = dgSearchResultList.GetCellFromPoint(pnt);

                if (Lot != null)
                {
                    if (Lot.Column.Name.Equals("LOTID"))
                    {
                        PACK001_034_ADDMASTERDATA popup = new PACK001_034_ADDMASTERDATA();
                        popup.FrameOperation = this.FrameOperation;

                        if (popup != null)
                        {
                            DataTable dtData = new DataTable();
                            dtData.Columns.Add("LOTID", typeof(string));

                            DataRow newRow = null;
                            newRow = dtData.NewRow();
                            newRow["LOTID"] = Lot.Text;

                            dtData.Rows.Add(newRow);

                            //========================================================================
                            object[] Parameters = new object[1];
                            Parameters[0] = dtData;
                            C1WindowExtension.SetParameters(popup, Parameters);
                            //========================================================================

                            //popup.Closed -= popup_Closed;
                            //popup.Closed += popup_Closed;
                            popup.ShowModal();
                            popup.CenterOnScreen();
                        }
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

                //작업자 동
                String[] sFilter = { LoginInfo.CFG_SHOP_ID, Area_Type.PACK};
                C1ComboBox[] cboSearchAREAIDChild = { cboSearchEQSGID, cboSearchPROCID };
                _combo.SetCombo(cboSearchAREAID, CommonCombo.ComboStatus.NONE, cbChild: cboSearchAREAIDChild, sFilter: sFilter, sCase: "AREA_AREATYPE");

                C1ComboBox[] cboSearchEQSGIDParent = { cboSearchAREAID };
                C1ComboBox[] cboSearchEQSGIDChild = { cboSearchROUTID, cboSearchPROCID };
                _combo.SetCombo(cboSearchEQSGID, CommonCombo.ComboStatus.NONE, cbChild: cboSearchEQSGIDChild, cbParent: cboSearchEQSGIDParent, sCase: "cboEquipmentSegment");

                //String[] sFilter_Rout = { cboSearchAREAID.SelectedValue.ToString(), cboSearchEQSGID.SelectedValue.ToString() };
                C1ComboBox[] cboSearchROUTIDParent = { cboSearchAREAID  ,cboSearchEQSGID };
                C1ComboBox[] cboSearchROUTIDChild = { cboSearchPRODID, cboSearchPROCID };
                _combo.SetCombo(cboSearchROUTID, CommonCombo.ComboStatus.ALL, cbChild:cboSearchROUTIDChild, cbParent: cboSearchROUTIDParent, sCase: "cboRoutGroup");

                //String[] sFilter_Proc = { null, cboSearchROUTID.SelectedValue.ToString() };
                C1ComboBox[] cboSearchPROCIDParent = { cboSearchAREAID, cboSearchEQSGID, cboSearchPRODID, cboSearchROUTID };
                _combo.SetCombo(cboSearchPROCID, CommonCombo.ComboStatus.ALL, cbParent: cboSearchPROCIDParent, sCase: "cboProcessRout");

                C1ComboBox[] cboSearchPRODIDParent = { cboSearchROUTID };
                C1ComboBox[] cboSearchPRODIDChild = { cboSearchPROCID };
                _combo.SetCombo(cboSearchPRODID, CommonCombo.ComboStatus.ALL, null, cbParent: cboSearchPRODIDParent, sCase: "cboProductRout");

                setComboBox_Area_schd();

                SetComboDay(cboDpi);
            }
            catch(Exception ex)
            {
                throw ex;
            }
        }

        private void setComboBox_Area_schd()
        {
            try
            {
                DataTable dtIndata = new DataTable();
                dtIndata.TableName = "RQSTDT";

                dtIndata.Columns.Add("SYSTEM_ID", typeof(string));
                dtIndata.Columns.Add("LANGID", typeof(string));
                dtIndata.Columns.Add("SHOPID", typeof(string));
                dtIndata.Columns.Add("USERID", typeof(string));

                DataRow drnewrow = dtIndata.NewRow();
                drnewrow["SYSTEM_ID"] = LoginInfo.SYSID;
                drnewrow["LANGID"] = LoginInfo.LANGID;
                drnewrow["SHOPID"] = LoginInfo.CFG_SHOP_ID;
                drnewrow["USERID"] = LoginInfo.USERID;
                dtIndata.Rows.Add(drnewrow);

                new ClientProxy().ExecuteService("DA_BAS_SEL_AUTH_AREA_CBO", "RQSTDT", "RSLTDT", dtIndata, (dtResult, ex) =>
                {
                    if (ex != null)
                    {
                        Util.MessageException(ex);
                        return;
                    }

                    cboTargetAREAID.ItemsSource = DataTableConverter.Convert(dtResult);

                    if((from DataRow dr in dtResult.Rows where dr["CBO_CODE"].Equals(LoginInfo.CFG_AREA_ID) select dr).Count() > 0)
                    {
                        cboTargetAREAID.SelectedValue = LoginInfo.CFG_AREA_ID;
                    }
                    else if (dtIndata.Rows.Count > 0)
                    {
                        cboTargetAREAID.SelectedIndex = 0;
                    }
                    else
                    {
                        cboTargetAREAID_SelectedValueChanged(null, null);
                    }
                });
            }
            catch(Exception ex)
            {
                throw ex;
            }
        }

        private void setComboBox_EquipmentSegment_schd(string sAREAID)
        {
            try
            {

                DataTable dtIndata = new DataTable();
                dtIndata.Columns.Add("LANGID", typeof(string));
                dtIndata.Columns.Add("AREAID", typeof(string));
                DataRow drNewRow = dtIndata.NewRow();

                drNewRow["LANGID"] = LoginInfo.LANGID;
                drNewRow["AREAID"] = sAREAID;
                dtIndata.Rows.Add(drNewRow);

                new ClientProxy().ExecuteService("DA_BAS_SEL_EQUIPMENTSEGMENT_CBO", "RQSTDT", "RSLTDT", dtIndata, (dtResult, ex) =>
                {
                    if (ex != null)
                    {
                        Util.MessageException(ex);
                        return;
                    }
                    
                    cboTargetEQSGID.ItemsSource = DataTableConverter.Convert(dtResult);

                    if ((from DataRow dr in dtResult.Rows where dr["CBO_CODE"].Equals(LoginInfo.CFG_EQSG_ID) select dr).Count() > 0)
                    {
                        cboTargetEQSGID.SelectedValue = LoginInfo.CFG_EQSG_ID;
                    }
                    else if (dtResult.Rows.Count > 0)
                    {
                        cboTargetEQSGID.SelectedIndex = 0;
                    }
                    else
                    {
                        cboTargetEQSGID_SelectedValueChanged(null, null);
                    }

                }
                );

            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        //private void setComboBox_EquipmentSegment_Search_schd(string sAREAID)
        //{
        //    try
        //    {

        //        DataTable dtIndata = new DataTable();
        //        dtIndata.Columns.Add("LANGID", typeof(string));
        //        dtIndata.Columns.Add("AREAID", typeof(string));
        //        DataRow drNewRow = dtIndata.NewRow();

        //        drNewRow["LANGID"] = LoginInfo.LANGID;
        //        drNewRow["AREAID"] = sAREAID;
        //        dtIndata.Rows.Add(drNewRow);

        //        new ClientProxy().ExecuteService("DA_BAS_SEL_EQUIPMENTSEGMENT_CBO", "RQSTDT", "RSLTDT", dtIndata, (dtResult, ex) =>
        //        {
        //            if (ex != null)
        //            {
        //                Util.MessageException(ex);
        //                return;
        //            }

        //            cboSearchEQSGID.ItemsSource = DataTableConverter.Convert(dtResult);

        //            if ((from DataRow dr in dtResult.Rows where dr["CBO_CODE"].Equals(LoginInfo.CFG_EQSG_ID) select dr).Count() > 0)
        //            {
        //                cboSearchEQSGID.SelectedValue = LoginInfo.CFG_EQSG_ID;
        //            }
        //            else if (dtResult.Rows.Count > 0)
        //            {
        //                cboSearchEQSGID.SelectedIndex = 0;
        //            }
        //            else
        //            {
        //                cboSearchEQSGID_SelectedValueChanged(null, null);
        //            }

        //        }
        //        );

        //    }
        //    catch (Exception ex)
        //    {
        //        throw ex;
        //    }
        //}

        private void setComboBox_Route_schd(string sAREAID, string sEQSGID)
        {
            try
            {

                DataTable dtIndata = new DataTable();
                dtIndata.Columns.Add("LANGID", typeof(string));
                dtIndata.Columns.Add("AREAID", typeof(string));
                dtIndata.Columns.Add("EQSGID", typeof(string));
                DataRow drIndata = dtIndata.NewRow();

                drIndata["LANGID"] = LoginInfo.LANGID;
                drIndata["AREAID"] = sAREAID;
                drIndata["EQSGID"] = sEQSGID;
                dtIndata.Rows.Add(drIndata);

                new ClientProxy().ExecuteService("DA_BAS_SEL_ROUT_GROUP_CBO", "RQSTDT", "RSLTDT", dtIndata, (dtResult, ex) =>
                {
                    if (ex != null)
                    {
                        Util.MessageException(ex);
                        return;
                    }

                    cboTargetROUTID.ItemsSource = DataTableConverter.Convert(dtResult);

                    if (dtIndata.Rows.Count > 0)
                    {
                        cboTargetROUTID.SelectedIndex = 0;
                    }
                    else
                    {
                        cboTargetROUTID_SelectedValueChanged(null, null);
                    }

                }
                );
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        //private void setComboBox_Route_Search_schd(string sAREAID, string sEQSGID)
        //{
        //    try
        //    {

        //        DataTable dtIndata = new DataTable();
        //        dtIndata.Columns.Add("LANGID", typeof(string));
        //        dtIndata.Columns.Add("AREAID", typeof(string));
        //        dtIndata.Columns.Add("EQSGID", typeof(string));
        //        DataRow drIndata = dtIndata.NewRow();

        //        drIndata["LANGID"] = LoginInfo.LANGID;
        //        drIndata["AREAID"] = sAREAID;
        //        drIndata["EQSGID"] = sEQSGID;
        //        dtIndata.Rows.Add(drIndata);

        //        new ClientProxy().ExecuteService("DA_BAS_SEL_ROUT_GROUP_CBO", "RQSTDT", "RSLTDT", dtIndata, (dtResult, ex) =>
        //        {
        //            if (ex != null)
        //            {
        //                Util.MessageException(ex);
        //                return;
        //            }

        //            DataRow dRow = dtResult.NewRow();

        //            dRow["CBO_NAME"] = "-ALL-";
        //            dRow["CBO_CODE"] = "";
        //            dtResult.Rows.InsertAt(dRow, 0);

        //            cboSearchROUTID.ItemsSource = DataTableConverter.Convert(dtResult);

        //            if (dtIndata.Rows.Count > 0)
        //            {
        //                cboSearchROUTID.SelectedIndex = 0;
        //            }
        //            else
        //            {
        //                cboSearchROUTID_SelectedValueChanged(null, null);
        //            }

        //        }
        //        );
        //    }
        //    catch (Exception ex)
        //    {
        //        throw ex;
        //    }
        //}

        private void setComboBox_Product_schd(string sROUTID)
        {
            try
            {
                DataTable dtIndata = new DataTable();
                dtIndata.Columns.Add("LANGID", typeof(string));
                dtIndata.Columns.Add("ROUTID", typeof(string));
                DataRow drIndata = dtIndata.NewRow();

                drIndata["LANGID"] = LoginInfo.LANGID;
                drIndata["ROUTID"] = sROUTID;
                dtIndata.Rows.Add(drIndata);

                new ClientProxy().ExecuteService("DA_BAS_SEL_PRODUCT_BY_ROUT_CBO", "RQSTDT", "RSLTDT", dtIndata, (dtResult, ex) =>
                {
                    if (ex != null)
                    {
                        Util.MessageException(ex);
                        return;
                    }

                    cboTargetPRODID.ItemsSource = DataTableConverter.Convert(dtResult);

                    if (dtIndata.Rows.Count > 0)
                    {
                        cboTargetPRODID.SelectedIndex = 0;
                    }
                    else
                    {
                        cboTargetPRODID_SelectedValueChanged(null, null);
                    }

                }
                );
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        //private void setComboBox_Product_Search_schd(string sROUTID)
        //{
        //    try
        //    {
        //        DataTable dtIndata = new DataTable();
        //        dtIndata.Columns.Add("LANGID", typeof(string));
        //        dtIndata.Columns.Add("ROUTID", typeof(string));
        //        DataRow drIndata = dtIndata.NewRow();

        //        drIndata["LANGID"] = LoginInfo.LANGID;
        //        drIndata["ROUTID"] = sROUTID.Equals("") ? null : sROUTID;
        //        dtIndata.Rows.Add(drIndata);

        //        new ClientProxy().ExecuteService("DA_BAS_SEL_PRODUCT_BY_ROUT_CBO", "RQSTDT", "RSLTDT", dtIndata, (dtResult, ex) =>
        //        {
        //            if (ex != null)
        //            {
        //                Util.MessageException(ex);
        //                return;
        //            }

        //            DataRow dRow = dtResult.NewRow();

        //            dRow["CBO_NAME"] = "-ALL-";
        //            dRow["CBO_CODE"] = "-ALL-";
        //            dtResult.Rows.InsertAt(dRow, 0);

        //            cboSearchPRODID.ItemsSource = DataTableConverter.Convert(dtResult);

        //            if (dtIndata.Rows.Count > 0)
        //            {
        //                cboSearchPRODID.SelectedIndex = 0;
        //            }
        //            else
        //            {
        //                cboSearchROUTID_SelectedValueChanged(null, null);
        //            }

        //        }
        //        );
        //    }
        //    catch (Exception ex)
        //    {
        //        throw ex;
        //    }
        //}

        private void setComboBox_Process_schd(string sAREAID, string sEQSGID, string sROUTID, string sPRODID)
        {
            try
            {
                DataTable dtIndata = new DataTable();
                dtIndata.Columns.Add("LANGID", typeof(string));
                dtIndata.Columns.Add("EQSGID", typeof(string));
                dtIndata.Columns.Add("ROUTID", typeof(string));
                dtIndata.Columns.Add("PRODID", typeof(string));
                dtIndata.Columns.Add("AREAID", typeof(string));
                DataRow drIndata = dtIndata.NewRow();

                drIndata["LANGID"] = LoginInfo.LANGID;
                drIndata["EQSGID"] = sEQSGID;
                drIndata["ROUTID"] = sROUTID;
                drIndata["PRODID"] = sPRODID;
                drIndata["AREAID"] = sAREAID;
                dtIndata.Rows.Add(drIndata);

                new ClientProxy().ExecuteService("DA_BAS_SEL_PROCESS_ROUT_CBO", "RQSTDT", "RSLTDT", dtIndata, (dtResult, ex) =>
                {
                    if (ex != null)
                    {
                        Util.MessageException(ex);
                        return;
                    }

                    cboTargetPROCID.ItemsSource = DataTableConverter.Convert(dtResult);

                    if ((from DataRow dr in dtResult.Rows where dr["CBO_CODE"].Equals(LoginInfo.CFG_PROC_ID) select dr).Count() > 0)
                    {
                        cboTargetPROCID.SelectedValue = LoginInfo.CFG_PROC_ID;
                    }
                    else if (dtIndata.Rows.Count > 0)
                    {
                        cboTargetPROCID.SelectedIndex = 0;
                    }
                    else
                    {
                        cboTargetPROCID_SelectedValueChanged(null, null);
                    }
                }
                );
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        //private void setComboBox_Process_Search_schd(string sAREAID, string sEQSGID, string sROUTID)
        //{
        //    try
        //    {
        //        DataTable dtIndata = new DataTable();
        //        dtIndata.Columns.Add("LANGID", typeof(string));
        //        dtIndata.Columns.Add("EQSGID", typeof(string));
        //        dtIndata.Columns.Add("ROUTID", typeof(string));
        //        dtIndata.Columns.Add("AREAID", typeof(string));
        //        DataRow drIndata = dtIndata.NewRow();

        //        drIndata["LANGID"] = LoginInfo.LANGID;
        //        drIndata["EQSGID"] = sEQSGID;
        //        drIndata["ROUTID"] = sROUTID.Equals("") ? null : sROUTID;
        //        drIndata["AREAID"] = sAREAID;
        //        dtIndata.Rows.Add(drIndata);
        //        //SetProcessRout
        //        new ClientProxy().ExecuteService("DA_BAS_SEL_PROCESS_ROUT_CBO", "RQSTDT", "RSLTDT", dtIndata, (dtResult, ex) =>
        //        {
        //            if (ex != null)
        //            {
        //                Util.MessageException(ex);
        //                return;
        //            }

        //            DataRow dRow = dtResult.NewRow();

        //            dRow["CBO_NAME"] = "-ALL-";
        //            dRow["CBO_CODE"] = "";
        //            dtResult.Rows.InsertAt(dRow, 0);

        //            cboSearchPROCID.ItemsSource = DataTableConverter.Convert(dtResult);

        //            if (dtIndata.Rows.Count > 0)
        //            {
        //                cboSearchPROCID.SelectedIndex = 0;
        //            }
        //        }
        //        );
        //    }
        //    catch (Exception ex)
        //    {
        //        throw ex;
        //    }
        //}

        private void setComboBox_Equipment_schd(string sAREAID, string sEQSGID, string sPROCID)
        {
            try
            {
                DataTable dtIndata = new DataTable();
                dtIndata.TableName = "RQSTDT";
                dtIndata.Columns.Add("LANGID", typeof(string));
                dtIndata.Columns.Add("EQSGID", typeof(string));
                dtIndata.Columns.Add("PROCID", typeof(string));
                dtIndata.Columns.Add("AREAID", typeof(string));
                DataRow drIndata = dtIndata.NewRow();

                drIndata["LANGID"] = LoginInfo.LANGID;
                drIndata["EQSGID"] = sEQSGID;
                drIndata["PROCID"] = sPROCID;
                drIndata["AREAID"] = sAREAID;
                dtIndata.Rows.Add(drIndata);

                new ClientProxy().ExecuteService("DA_BAS_SEL_EQUIPMENT_NUMBER_CBO", "RQSTDT", "RSLTDT", dtIndata, (dtResult, ex) =>
                {
                    if (ex != null)
                    {
                        Util.MessageException(ex);
                        return;
                    }

                    cboTargetEQPTID.ItemsSource = DataTableConverter.Convert(dtResult);

                    if ((from DataRow dr in dtResult.Rows where dr["CBO_CODE"].Equals(LoginInfo.CFG_EQPT_ID) select dr).Count() > 0)
                    {
                        cboTargetEQPTID.SelectedValue = LoginInfo.CFG_EQPT_ID;
                    }
                    else if (dtIndata.Rows.Count > 0)
                    {
                        cboTargetEQPTID.SelectedIndex = 0;
                    }
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
                dtINDATA.Columns.Add("EQSGID", typeof(string));
                dtINDATA.Columns.Add("PROCID", typeof(string));
                dtINDATA.Columns.Add("EQPTID", typeof(string));
                dtINDATA.Columns.Add("PRODID", typeof(string));
                dtINDATA.Columns.Add("ROUTID", typeof(string));
                dtINDATA.Columns.Add("USERID", typeof(string));

                drINDATA = dtINDATA.NewRow();
                drINDATA["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                drINDATA["LANGID"] = LoginInfo.LANGID;
                drINDATA["EQSGID"] = Util.NVC(DataTableConverter.GetValue(dgTagetList.Rows[0].DataItem, "EQSGID"));
                drINDATA["PROCID"] = Util.NVC(DataTableConverter.GetValue(dgTagetList.Rows[0].DataItem, "PROCID"));
                drINDATA["EQPTID"] = Util.NVC(DataTableConverter.GetValue(dgTagetList.Rows[0].DataItem, "EQPTID"));
                drINDATA["PRODID"] = Util.NVC(DataTableConverter.GetValue(dgTagetList.Rows[0].DataItem, "PRODID"));
                drINDATA["ROUTID"] = Util.NVC(DataTableConverter.GetValue(dgTagetList.Rows[0].DataItem, "ROUTID"));
                drINDATA["USERID"] = LoginInfo.USERID;
                dtINDATA.Rows.Add(drINDATA);

                DataRow drINLOT = null;
                DataTable dtINLOT = new DataTable();
                dtINLOT.TableName = "INLOT";
                dtINLOT.Columns.Add("LOT_PARAM", typeof(string));
                dtINLOT.Columns.Add("PROC_CHG_FLAG", typeof(string));

                for (int i = 0; i < dgTagetList.Rows.Count; i++)
                {
                    drINLOT = dtINLOT.NewRow();
                    //고정으로 붙이는 MASTER; 포멧 삭제하여 PARAMETER로 넘긴다.
                    drINLOT["LOT_PARAM"]     = Util.NVC(DataTableConverter.GetValue(dgTagetList.Rows[i].DataItem, "LOTID")).Substring(7);
                    drINLOT["PROC_CHG_FLAG"] = Util.NVC(DataTableConverter.GetValue(dgTagetList.Rows[i].DataItem, "PROC_CHG_FLAG"));
                    dtINLOT.Rows.Add(drINLOT);
                }

                dsInput.Tables.Add(dtINDATA);
                dsInput.Tables.Add(dtINLOT);

                new ClientProxy().ExecuteService_Multi("BR_PRD_CREATE_MST_SMPL_LOT_UI", "INDATA,INLOT", "OUTDATA", (dsResult, dataException) =>
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
                DataTable dtIndata = new DataTable();

                dtIndata.TableName = "INDATA";
                dtIndata.Columns.Add("SRCTYPE",   typeof(string));
                dtIndata.Columns.Add("LANGID",    typeof(string));
                dtIndata.Columns.Add("LOTID",     typeof(string));
                dtIndata.Columns.Add("EQSGID",    typeof(string));
                dtIndata.Columns.Add("EQSGNAME",  typeof(string));
                dtIndata.Columns.Add("ROUTID",    typeof(string));
                dtIndata.Columns.Add("ROUTNAME",  typeof(string));
                dtIndata.Columns.Add("PRODID",    typeof(string));
                dtIndata.Columns.Add("PRODDESC",  typeof(string));
                dtIndata.Columns.Add("PROCID",    typeof(string));
                dtIndata.Columns.Add("PROCNAME",  typeof(string));
                dtIndata.Columns.Add("EQPTID",    typeof(string));
                dtIndata.Columns.Add("EQPTNAME",  typeof(string));
                dtIndata.Columns.Add("PRJT_NAME", typeof(string));
                dtIndata.Columns.Add("PROC_CHG_FLAG", typeof(string));


                DataRow dr = dtIndata.NewRow();
                dr["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                dr["LANGID"] = LoginInfo.LANGID;
                dr["LOTID"] = txtLotID.Text;
                dr["EQSGID"] = Util.NVC(cboTargetEQSGID.SelectedValue);
                dr["EQSGNAME"] = cboTargetEQSGID.Text;
                dr["ROUTID"] = Util.NVC(cboTargetROUTID.SelectedValue);
                dr["ROUTNAME"] = cboTargetROUTID.Text;
                dr["PRODID"] = Util.NVC(cboTargetPRODID.SelectedValue);
                dr["PRODDESC"] = cboTargetPRODID.Text;
                dr["PROCID"] = Util.NVC(cboTargetPROCID.SelectedValue);
                dr["PROCNAME"] = cboTargetPROCID.Text;
                dr["EQPTID"] = Util.NVC(cboTargetEQPTID.SelectedValue);
                dr["EQPTNAME"] = cboTargetEQPTID.Text;
                dr["PROC_CHG_FLAG"] = ((bool)chkPass.IsChecked) ? "Y" : null;
                dtIndata.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("BR_PRD_CHK_MST_SMPL_LOT_UI", "INDATA", "OUTDATA", dtIndata);
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
                    Util.SetTextBlockText_DataGridRowCount(tbTagetListCount, Util.NVC(dgTagetList.Rows.Count));
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
                DataRow[] drTemp = dtBefore.Select("LOTID = 'MASTER;" + sLotID + "'");
                if (drTemp.Length > 0)
                {                    
                    ms.AlertWarning("SFU1376", sLotID); //LOT ID는 중복 입력할수 없습니다.\r\n({0})
                    bReturn = false;
                }
            }

            return bReturn;
        }

        private bool chkInputInfo(string sEQSGID, string sROUTID, string sPRODID, string sPROCID, string sEQPTID)
        {
            bool bReturn = true;

            DataTable dtBefore = DataTableConverter.Convert(dgTagetList.ItemsSource);
            if(dtBefore.Rows.Count > 0)
            {
                DataRow[] drTemp = dtBefore.Select("EQSGID = '" + sEQSGID +"' AND ROUTID = '" + sROUTID + "' AND PRODID = '" + sPRODID + "' AND PROCID = '" + sPROCID + "' AND EQPTID = '" + sEQPTID + "'");
                if(drTemp.Length == 0)
                {
                    ms.AlertWarning("SFU2037"); //확인 처리정보확인
                    bReturn = false;
                }
            }

            return bReturn;
        }

        private void getSearchData()
        {
            try
            {
                DataTable dtIndata = new DataTable();
                dtIndata.TableName = "RQSTDT";
                dtIndata.Columns.Add("LANGID", typeof(string));
                dtIndata.Columns.Add("EQSGID", typeof(string));
                dtIndata.Columns.Add("ROUTID", typeof(string));
                dtIndata.Columns.Add("PROCID", typeof(string));
                dtIndata.Columns.Add("PRODID", typeof(string));
                dtIndata.Columns.Add("FROM_DATE", typeof(string));
                dtIndata.Columns.Add("TO_DATE", typeof(string));
                dtIndata.Columns.Add("LOTID", typeof(string));

                DataRow dr = dtIndata.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["EQSGID"] = Util.NVC(cboSearchEQSGID.SelectedValue) == "" ? null : Util.NVC(cboSearchEQSGID.SelectedValue);
                dr["ROUTID"] = Util.NVC(cboSearchROUTID.SelectedValue) == "" ? null : Util.NVC(cboSearchROUTID.SelectedValue);
                dr["PROCID"] = Util.NVC(cboSearchPROCID.SelectedValue) == "" ? null : Util.NVC(cboSearchPROCID.SelectedValue);
                dr["PRODID"] = Util.NVC(cboSearchPRODID.SelectedValue) == "" ? null : Util.NVC(cboSearchPRODID.SelectedValue);
                dr["FROM_DATE"] = dtpDateFrom.SelectedDateTime.ToString("yyyyMMdd");
                dr["TO_DATE"] = dtpDateTo.SelectedDateTime.ToString("yyyyMMdd");
                dr["LOTID"] = txtSearchLOTID.Text.Trim() == "" ? null : txtSearchLOTID.Text.Trim();
                dtIndata.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_MST_SMPL_LOT_UI", "RQSTDT", "RSLTDT", dtIndata);


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

        //private void getLabelCode()
        //{
        //    try
        //    {
        //        DataTable dtIndata = new DataTable();
        //        dtIndata.TableName = "RQSTDT";
        //        dtIndata.Columns.Add("LANGID", typeof(string));
        //        dtIndata.Columns.Add("LABEL_TYPE_CODE", typeof(string));
        //        DataRow drIndata = dtIndata.NewRow();

        //        drIndata["LANGID"] = LoginInfo.LANGID;
        //        drIndata["LABEL_TYPE_CODE"] = "PACK";
        //        dtIndata.Rows.Add(drIndata);

        //        new ClientProxy().ExecuteService("DA_PRD_SEL_LABELCODE_BY_LABEL_TYPE_CBO", "RQSTDT", "RSLTDT", dtIndata, (dtResult, ex) =>
        //        {
        //            if (ex != null)
        //            {
        //                Util.MessageException(ex);
        //                return;
        //            }

        //            cboLabelCode.ItemsSource = DataTableConverter.Convert(dtResult);

        //            if ((from DataRow dr in dtResult.Rows where dr["CBO_CODE"].Equals(now_labelcode) select dr).Count() > 0)
        //            {
        //                cboLabelCode.SelectedValue = now_labelcode;
        //            }
        //            else if (dtIndata.Rows.Count > 0)
        //            {
        //                cboLabelCode.SelectedIndex = 0;
        //            }
        //        }
        //        );
        //    }
        //    catch (Exception ex)
        //    {
        //        throw ex;
        //        }

        //    }

        private void Refresh()
        {
            try
            {
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

        private void dgSearchResultList_MouseUp(object sender, MouseButtonEventArgs e)
        {
            try
            {
                Point pnt = e.GetPosition(null);
                C1.WPF.DataGrid.DataGridCell cell = dgSearchResultList.GetCellFromPoint(pnt);

                if (cell == null || cell.Value == null)
                {
                    return;
                }

                int currentRow = cell.Row.Index;

                txtSeletedLotId.Text = DataTableConverter.GetValue(dgSearchResultList.Rows[currentRow].DataItem, "LOTID").ToString();
                txtPrintProdId.Text = DataTableConverter.GetValue(dgSearchResultList.Rows[currentRow].DataItem, "PRODID").ToString();
                txtPrintDate.Text = Convert.ToDateTime(DataTableConverter.GetValue(dgSearchResultList.Rows[currentRow].DataItem, "LOTDTTM_CR").ToString()).ToString("yyyy-MM-dd");
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private void btnPrint_Click(object sender, RoutedEventArgs e)
        {
            try
            {
               if (txtSeletedLotId.Text.Length > 0)
                {
                    //#01
                    //라벨 발행
                    string strZPL = string.Empty;
                    string strLOTID = string.Empty;
                    string strPRODID = string.Empty;
                    string strDATE = string.Empty;

                    strLOTID = txtSeletedLotId.Text;
                    strPRODID = txtPrintProdId.Text;
                    strDATE = txtPrintDate.Text;


                    if (cboDpi.SelectedValue.ToString().Equals("203"))
                    {
                        strZPL = "^XA^DFR:TEMP_FMT.ZPL^XZ^XA^XFR:TEMP_FMT.ZPL^AAN,18,5^FO7,16^FDREF : "
                                     + strPRODID
                                     + "^FS^AAN,18,5^FO114,15^FD^FS^AAN,18,5^FO7,34^FD"
                                     + strDATE
                                     + "^FS^AAN,18,5^FO62,34^FD^FS^AAN,9,5^FO14,52^FDMASTER LOT SAPLE^FS^FO22,56^BQN,2,3^FH^FDMA,"
                                     + strLOTID
                                     + "^FS^FS^AAN,18,5^FO4,144^FD"
                                     + strLOTID
                                     + "^FS^AAN,18,5^FO4,162^FD^FS^AAN,18,5^FO113,186^FD^FS^PQ1,0,1,Y^XZ^XA^IDR:TEMP_FMT.ZPL^XZ";
                    }
                    else
                    {
                        strZPL = "^XA^DFR:TEMP_FMT.ZPL^XZ^XA^XFR:TEMP_FMT.ZPL^AAN,27,10^FO31,20^FD"
                                + strPRODID
                                +"^FS^AAN,27,10^FO31,47^FD"
                                + strLOTID
                                + "^FS^FO43,83^BQN,2,6^FH^FDMA,"
                                + strLOTID
                                + "^FS^FS^AAN,27,10^PQ1,0,1,Y^XZ^XA^IDR:TEMP_FMT.ZPL^XZ";
                    }

                    Util.PrintLabel(FrameOperation, loadingIndicator, strZPL);

                    //#02
                    //now_labelcode = Util.NVC(cboLabelCode.SelectedValue);
                }
            }
            catch (Exception ex)
            {
                //Util.AlertInfo(ex.Message);
                Util.MessageException(ex);
            }
        }

        //MASTER LOT ID 정보변경 - 20180905 손홍구S
        private void btnUpdate_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (txtSeletedLotId.Text.Length > 0)
                {
                    DataTable dtIndata = new DataTable();

                    dtIndata.TableName = "INDATA";
                    dtIndata.Columns.Add("SRCTYPE", typeof(string));
                    dtIndata.Columns.Add("LANGID", typeof(string));
                    dtIndata.Columns.Add("LOTID", typeof(string));

                    DataRow dr = dtIndata.NewRow();
                    dr["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                    dr["LANGID"] = LoginInfo.LANGID;
                    dr["LOTID"] = txtSeletedLotId.Text;

                    dtIndata.Rows.Add(dr);

                    DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_MST_SMPL_DATA_CLCT_TYPE_Q", "INDATA", "OUTDATA", dtIndata);

                    // 2019-06-18 
                    //데이터 존재 여부로 변경 가능 판단
                    //if (dtResult.Rows.Count > 0)
                    //{
                    //    ms.AlertWarning("SFU1128"); //투입된 LOT 입니다.
                    //}
                    //else
                    //{
                        PACK001_034_MASTERLOT_CHANGE popup = new PACK001_034_MASTERLOT_CHANGE();
                        popup.FrameOperation = this.FrameOperation;
                        if (popup != null)
                        {
                            DataTable dtData = new DataTable();
                            dtData.Columns.Add("LOTID", typeof(string));

                            DataRow newRow = null;
                            newRow = dtData.NewRow();
                            newRow["LOTID"] = txtSeletedLotId.Text;

                            dtData.Rows.Add(newRow);

                            //========================================================================
                            object[] Parameters = new object[1];
                            Parameters[0] = dtData;
                            C1WindowExtension.SetParameters(popup, Parameters);
                            //========================================================================

                            popup.Closed -= popup_Closed;
                            popup.Closed += popup_Closed;
                            popup.ShowModal();
                            popup.CenterOnScreen();
                        }
                    //}
                }
                else
                {
                    ms.AlertWarning("SFU1364"); //LOT ID가 선택되지 않았습니다.
                }
            }
            catch (Exception ex)
            {
                //Util.AlertInfo(ex.Message);
                Util.MessageException(ex);
            }
        }

        void popup_Closed(object sender, EventArgs e)
        {
            PACK001_034_MASTERLOT_CHANGE popup = sender as PACK001_034_MASTERLOT_CHANGE;

            if (popup.DialogResult == MessageBoxResult.OK)
            {
                getSearchData(); //정보변경 후 재조회
            }
        }
        #endregion

         //2019.06.18  김도형 : [CSR ID:4002930] Master Lot  수정/삭제 기능 보완 件 | [요청번호]C20190527_02930 | [서비스번호]4002930 
        private void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(ms.AlertRetun("SFU1230"), null, "Information", MessageBoxButton.OKCancel, MessageBoxIcon.None, (result) =>
                //삭제 하시겠습니까?
                {
                    if (result == MessageBoxResult.OK)
                    {
                        if (txtSeletedLotId.Text.Length > 0)
                        {
                            DataSet dsInput = new DataSet();
                            DataTable dtIndata = new DataTable();

                            dtIndata.TableName = "INDATA";
                            dtIndata.Columns.Add("LOTID", typeof(string));

                            DataRow dr = dtIndata.NewRow();
                            dr["LOTID"] = txtSeletedLotId.Text;

                            dtIndata.Rows.Add(dr);
                            dsInput.Tables.Add(dtIndata);

                            new ClientProxy().ExecuteServiceSync_Multi("BR_PRD_DELETE_MST_SPML_LOT", "INDATA", "", dsInput, null);

                            ms.AlertInfo("SFU1275"); //정상처리되었습니다.

                            getSearchData();

                        }
                        else
                        {
                            ms.AlertWarning("SFU1364"); //LOT ID가 선택되지 않았습니다.
                        }
                    }
                }
            );
            }

            catch (Exception ex)
            {
                //Util.AlertInfo(ex.Message);
                Util.MessageException(ex);
            }
        }

        private void SetComboDay(C1ComboBox cb)
        {
            DataTable dt = new DataTable();
            dt.Columns.Add("CBO_NAME", typeof(string));
            dt.Columns.Add("CBO_CODE", typeof(string));

            DataRow dr = dt.NewRow();
            dr["CBO_NAME"] = "203 Dpi";
            dr["CBO_CODE"] = "203";
            dt.Rows.Add(dr);

            dr = dt.NewRow();
            dr["CBO_NAME"] = "300 Dpi";
            dr["CBO_CODE"] = "300";
            dt.Rows.Add(dr);

            dt.AcceptChanges();

            cb.ItemsSource = DataTableConverter.Convert(dt);

            if(LoginInfo.CFG_SERIAL_PRINT.Rows.Count > 0)
            {
                if ((LoginInfo.CFG_SERIAL_PRINT.Rows[0]).ItemArray[12].ToString().Equals("203"))
                {
                    cb.SelectedIndex = 0;
                }
                else
                {
                    cb.SelectedIndex = 1;
                }
            }
            else
            {
                cb.SelectedIndex = 0;
            }
        }


    }
}
