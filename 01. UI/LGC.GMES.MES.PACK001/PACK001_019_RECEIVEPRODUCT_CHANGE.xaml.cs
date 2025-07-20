/*************************************************************************************
 Created Date : 2016.08.01
      Creator : Jeong Hyeon Sik
   Decription : Pack 자재입고 화면 - 입고정보 변경 팝업
--------------------------------------------------------------------------------------
 [Change History]
  2016.08.01  Jeong Hyeon Sik : Initial Created.
  2018.04.12  손우석 생산예정모델 ALL 추가 
  2020.01.17  염규범  폴란드의 경우, DB가 분리되어 있어서, 입고 잘못되는 Issus Validation Logic 추가
***************************************************************************************/

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
    public partial class PACK001_019_RECEIVEPRODUCT_CHANGE : C1Window, IWorkArea
    {
        #region Declaration & Constructor 
        private string sPRODID = string.Empty;
        string sTagetArea = string.Empty;

        /// <summary>
        ///  Frame과 상호작용하기 위한 객체
        /// </summary>
        public IFrameOperation FrameOperation
        {
            get;
            set;
        }
        public PACK001_019_RECEIVEPRODUCT_CHANGE()
        {
            InitializeComponent();
        }

        Util util = new Util();

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
        #endregion

        #region Initialize

        #endregion

        #region Event
        private void C1Window_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                txtPallet.Text = C1WindowExtension.GetParameter(this);
                if (txtPallet.Text.Length > 0)
                {
                    getPalletInfo();
                }
            }
            catch (Exception ex)
            {
                Util.Alert(ex.ToString());
            }
        }
        private void btnEcel_Click(object sender, RoutedEventArgs e)
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
        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                getPalletInfo();
            }
            catch (Exception ex)
            {
                Util.Alert(ex.ToString());
            }
        }
        private void btnSAVE_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (chkInputData())
                {
                    //선택한 Lot의 정보를 변경 하시겠습니까?\n\nRout : {0}\n생산적용모델 : {1}
                    LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("SFU3599", cboChangeRoute.Text, cboChangeModel.Text), null, "Information", MessageBoxButton.OKCancel, MessageBoxIcon.None, (sresult) =>
                    {
                        if (sresult == MessageBoxResult.OK)
                        {
                            changeInputData();

                            getPalletInfo();
                        }
                    });
                    
                }
            }
            catch (Exception ex)
            {
                Util.Alert(ex.ToString());
            }
        }
        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = MessageBoxResult.Cancel;
            this.Close();
        }
        private void txtRcvIssID_KeyDown(object sender, KeyEventArgs e)
        {
            try
            {
                if (e.Key == Key.Enter)
                {
                    getPalletInfo();
                }
            }
            catch (Exception ex)
            {
                Util.Alert(ex.ToString());
            }
        }
        private void txtPallet_KeyDown(object sender, KeyEventArgs e)
        {
            try
            {
                if (e.Key == Key.Enter)
                {
                    getPalletInfo();
                }
            }
            catch (Exception ex)
            {
                Util.Alert(ex.ToString());
            }
        }
        private void dgSearchResultList_LoadedColumnHeaderPresenter(object sender, C1.WPF.DataGrid.DataGridColumnEventArgs e)
        {
            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (string.IsNullOrEmpty(e.Column.Name) == false)
                {
                    if (e.Column.Name.Equals("CHK"))
                    {
                        pre.Content = chkAll;
                        e.Column.HeaderPresenter.Content = pre;
                        chkAll.Checked += new RoutedEventHandler(checkAll_Checked);
                        chkAll.Unchecked += new RoutedEventHandler(checkAll_Unchecked);
                    }
                }

            }
            ));
        }
        void checkAll_Checked(object sender, RoutedEventArgs e)
        {
            for (int idx = 0; idx < dgSearchResultList.Rows.Count; idx++)
            {
                C1.WPF.DataGrid.DataGridRow row = dgSearchResultList.Rows[idx];
                DataTableConverter.SetValue(row.DataItem, "CHK", true);
            }
        }
        void checkAll_Unchecked(object sender, RoutedEventArgs e)
        {
            for (int idx = 0; idx < dgSearchResultList.Rows.Count; idx++)
            {
                C1.WPF.DataGrid.DataGridRow row = dgSearchResultList.Rows[idx];
                DataTableConverter.SetValue(row.DataItem, "CHK", false);
            }
        }
        private void cboProduct_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {

        }
        private void cboChangeModel_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            try
            {
                setComboBox_Route_schd(Util.NVC(cboChangeModel.SelectedValue), sPRODID);
            }
            catch (Exception ex)
            {
                Util.Alert(ex.ToString());
            }
        }
        //2018-04-12
        private void cboChangeRoute_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            try
            {
                if (cboChangeRoute.SelectedItem == null)
                {
                    sTagetArea = "";
                }
                else
                {
                    sTagetArea = Convert.ToString(DataTableConverter.GetValue(cboChangeRoute.SelectedItem, "AREAID"));
                }
            }
            catch (Exception ex)
            {
                Util.Alert(ex.ToString());
            }
        }
        //2018-04-12
        #endregion

        #region Mehod
        private void setComboBox()
        {
            try
            {
                //CommonCombo _combo = new CommonCombo();


                //C1ComboBox cboSHOPID = new C1ComboBox();
                //cboSHOPID.SelectedValue = LoginInfo.CFG_SHOP_ID;
                //C1ComboBox cboAREA_TYPE_CODE = new C1ComboBox();
                //cboAREA_TYPE_CODE.SelectedValue = "P";
                //C1ComboBox cboPRODID = new C1ComboBox();
                //cboPRODID.SelectedValue = null;
                //C1ComboBox cboAreaByAreaType = new C1ComboBox();
                //cboAreaByAreaType.SelectedValue = null;
                //C1ComboBox cboEquipmentSegment = new C1ComboBox();
                //cboEquipmentSegment.SelectedValue = null;
                //C1ComboBox cboProductType = new C1ComboBox();
                //cboProductType.SelectedValue = "CELL";

                ////입고대상입력 모델
                //C1ComboBox[] cboTagetProductModelParent = { cboSHOPID, cboAreaByAreaType, cboEquipmentSegment, cboAREA_TYPE_CODE, cboPRODID };
                //C1ComboBox[] cboTagetProductModelChild = { cboProduct };
                //_combo.SetCombo(cboModel, CommonCombo.ComboStatus.NONE, cbChild: cboTagetProductModelChild, cbParent: cboTagetProductModelParent, sCase: "PRODUCTMODEL");

                ////입고대상입력 제품
                //C1ComboBox[] cboProductParent = { cboSHOPID, cboAreaByAreaType, cboEquipmentSegment, cboModel, cboAREA_TYPE_CODE, cboProductType };
                //_combo.SetCombo(cboProduct, CommonCombo.ComboStatus.ALL, cbParent: cboProductParent, sCase: "PRODUCT");


                ////변경 모델
                //C1ComboBox[] cboChangeModelParent = { cboSHOPID, cboAreaByAreaType, cboEquipmentSegment, cboAREA_TYPE_CODE, cboPRODID };
                //_combo.SetCombo(cboChangeModel, CommonCombo.ComboStatus.ALL, cbParent: cboChangeModelParent, sCase: "PRODUCTMODEL");


                ////변경 LOTTYPE
                //_combo.SetCombo(cboChangeLotType, CommonCombo.ComboStatus.NONE, null, sCase: "LOTTYPE");

                ////입고대상입력 route
                //C1ComboBox[] cboTagetRouteParent = { cboTagetModel, cboTagetProduct };
                //_combo.SetCombo(cboChangeRoute, CommonCombo.ComboStatus.NONE, cbParent: cboTagetRouteParent, sCase: "ROUTEBYMODLID");

                

                ////조회 제품
                //C1ComboBox[] cboSearchProductParent = { cboSHOPID, cboAreaByAreaType, cboEquipmentSegment, cboSearchModel, cboAREA_TYPE_CODE, cboProductType };
                //C1ComboBox[] cboSearchProductChild = { cboSearchRoute };
                //_combo.SetCombo(cboSearchProduct, CommonCombo.ComboStatus.ALL, cbChild: cboSearchProductChild, cbParent: cboSearchProductParent, sCase: "PRODUCT");

                ////조회 route
                //C1ComboBox[] cboSearchRouteParent = { cboSearchModel, cboSearchProduct };
                //_combo.SetCombo(cboSearchRoute, CommonCombo.ComboStatus.NONE, cbParent: cboSearchRouteParent, sCase: "ROUTEBYMODLID");
                
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        private void getPalletInfo()
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("ROUTID", typeof(string));
                RQSTDT.Columns.Add("MODLID", typeof(string));
                RQSTDT.Columns.Add("PRODID", typeof(string));
                RQSTDT.Columns.Add("FROMDATE", typeof(string));
                RQSTDT.Columns.Add("TODATE", typeof(string));
                RQSTDT.Columns.Add("PALLETID", typeof(string));
                RQSTDT.Columns.Add("LOTID", typeof(string));
                RQSTDT.Columns.Add("EQSGID", typeof(string));
                RQSTDT.Columns.Add("AREAID", typeof(string));
                RQSTDT.Columns.Add("RCV_ISS_ID", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["ROUTID"] = null;
                dr["MODLID"] = null;
                dr["PRODID"] = null;
                dr["FROMDATE"] = null;
                dr["TODATE"] = null;
                dr["PALLETID"] = txtPallet.Text.Length > 0 ? txtPallet.Text : null;
                dr["LOTID"] = null;
                dr["EQSGID"] = null;
                dr["AREAID"] = null;
                dr["RCV_ISS_ID"] = txtRcvIssID.Text.Length > 0 ? txtRcvIssID.Text : null;
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_RECEIVE_PRODUCT_PACK", "RQSTDT", "RSLTDT", RQSTDT);

                //DataTable RQSTDT = new DataTable();
                //RQSTDT.TableName = "RQSTDT";
                //RQSTDT.Columns.Add("LANGID", typeof(string));
                //RQSTDT.Columns.Add("RCV_ISS_ID", typeof(string));
                //RQSTDT.Columns.Add("PALLETID", typeof(string));

                //DataRow dr = RQSTDT.NewRow();
                //dr["LANGID"] = LoginInfo.LANGID;
                //dr["RCV_ISS_ID"] = txtRcvIssID.Text.Length > 0 ? txtRcvIssID.Text : null;
                //dr["PALLETID"] = txtPallet.Text.Length > 0 ? txtPallet.Text : null;
                //RQSTDT.Rows.Add(dr);

                //DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_TB_SFC_RCV_ISS_PLLT_DTL", "RQSTDT", "RSLTDT", RQSTDT);
                
                dgSearchResultList.ItemsSource = DataTableConverter.Convert(dtResult);

                if (dtResult != null)
                {
                    if (dtResult.Rows.Count > 0)
                    {
                        sPRODID = Util.NVC(DataTableConverter.GetValue(dgSearchResultList.Rows[0].DataItem, "PRODID"));

                        setComboBox_Model_schd(sPRODID);
                    }
                    else
                    {
                        sPRODID = string.Empty;
                    }
                }
                else
                {
                    sPRODID = string.Empty;
                    Util.MessageInfo("SFU1498");
                }
            }
            catch (Exception ex)
            {
                //Util.AlertByBiz("DA_PRD_SEL_RECEIVE_PRODUCT_PACK", ex.Message, ex.ToString());
                Util.MessageException(ex);
            }
        }
        /// <summary>
        /// 입력 Validation
        /// LOT선택수확인.
        /// 제품코드비교
        /// 입력확인메세지표시
        /// </summary>
        /// <returns>true:정상 , false: 입력/선택 오류</returns>
        private bool chkInputData()
        {
            bool bReturn = true;

            if ( !(util.GetDataGridCheckCnt(dgSearchResultList,"CHK") > 0))
            {
                //변경 대상이 없습니다. 변경 할 LOT을 선택 하세요.
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("SFU1565"), null, "Info", MessageBoxButton.OK, MessageBoxIcon.Warning);
                bReturn = false;
                return bReturn;
            }

            if (cboChangeRoute.SelectedIndex < 0)
            {
                //경로를 선택 하세요.
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("SFU1455"), null, "Info", MessageBoxButton.OK, MessageBoxIcon.Warning);
                bReturn = false;
                cboChangeRoute.Focus();
                return bReturn;
            }

            if (cboChangeModel.SelectedIndex < 0)
            {
                //모델을 선택 하세요.
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("SFU1257"), null, "Info", MessageBoxButton.OK, MessageBoxIcon.Warning);
                bReturn = false;
                cboChangeModel.Focus();
                return bReturn;
            }

            //if (cboChangeLotType.SelectedIndex < 0)
            //{
            //    LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("생산Type을 선택 하세요."), null, "Info", MessageBoxButton.OK, MessageBoxIcon.Warning);
            //    bReturn = false;
            //    cboChangeLotType.Focus();
            //    return bReturn;
            //}

            for (int i = 0; i < dgSearchResultList.Rows.Count; i++)
            {
                //if (Util.NVC(DataTableConverter.GetValue(dgSearchResultList.Rows[i].DataItem, "CHK")) == "True")
                //{
                //    if (!txtProduct.Text.Equals(Util.NVC(DataTableConverter.GetValue(dgSearchResultList.Rows[i].DataItem, "PRODID"))))
                //    {
                //        LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("선택된 LOT중 다른 제품코드가 존재합니다."), null, "Info", MessageBoxButton.OK, MessageBoxIcon.Warning);
                //        bReturn = false;
                //        return bReturn;
                //    }
                //}
            }

            return bReturn;
        }
        private void changeInputData()
        {
            try
            {
                DataTable dtINDATA = new DataTable();
                dtINDATA.TableName = "INDATA";
                dtINDATA.Columns.Add("SRCTYPE", typeof(string));
                dtINDATA.Columns.Add("LANGID", typeof(string));
                dtINDATA.Columns.Add("MODLID", typeof(string));
                dtINDATA.Columns.Add("EQSGID", typeof(string));
                dtINDATA.Columns.Add("ROUTID", typeof(string));
                dtINDATA.Columns.Add("USERID", typeof(string));

                DataRow drINDATA = dtINDATA.NewRow();
                drINDATA["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                drINDATA["LANGID"] = LoginInfo.LANGID;
                drINDATA["MODLID"] = cboChangeModel.SelectedValue;
                drINDATA["EQSGID"] = "";
                drINDATA["ROUTID"] = cboChangeRoute.SelectedValue;
                drINDATA["USERID"] = LoginInfo.USERID;
                dtINDATA.Rows.Add(drINDATA);

                DataRow drRCV_ISS = null;
                DataTable dtRCV_ISS = new DataTable();
                dtRCV_ISS.TableName = "RCV_ISS";
                dtRCV_ISS.Columns.Add("RCV_ISS_ID", typeof(string));
                dtRCV_ISS.Columns.Add("PALLETID", typeof(string));
                for (int i = 0; i < dgSearchResultList.Rows.Count; i++)
                {
                    if (Util.NVC(DataTableConverter.GetValue(dgSearchResultList.Rows[i].DataItem, "CHK")) == "True" ||
                        Util.NVC(DataTableConverter.GetValue(dgSearchResultList.Rows[i].DataItem, "CHK")) == "1")
                    {
                        drRCV_ISS = dtRCV_ISS.NewRow();
                        drRCV_ISS["RCV_ISS_ID"] = Util.NVC(DataTableConverter.GetValue(dgSearchResultList.Rows[i].DataItem, "RCV_ISS_ID"));
                        drRCV_ISS["PALLETID"] = Util.NVC(DataTableConverter.GetValue(dgSearchResultList.Rows[i].DataItem, "PALLETID"));
                        dtRCV_ISS.Rows.Add(drRCV_ISS);
                    }
                }

                DataSet indataSet = new DataSet();
                indataSet.Tables.Add(dtINDATA);
                indataSet.Tables.Add(dtRCV_ISS);

                new ClientProxy().ExecuteServiceSync_Multi("BR_PRD_REG_RECEIVE_PRODUCT_EDIT_PACK", "INDATA,RCV_ISS", "OUTDATA", indataSet, null);
            }
            catch (Exception ex)
            {
                //Util.AlertByBiz("BR_PRD_REG_RECEIVE_PRODUCT_EDIT_PACK", ex.Message, ex.ToString());
                Util.MessageException(ex);
            }
        }
        private void setComboBox_Model_schd(string sMTRLID)
        {
            try
            {
                DataTable dtIndata = new DataTable();
                dtIndata.Columns.Add("LANGID", typeof(string));
                dtIndata.Columns.Add("MTRLID", typeof(string));
                dtIndata.Columns.Add("AREAID", typeof(string));
                DataRow drIndata = dtIndata.NewRow();

                drIndata["LANGID"] = LoginInfo.LANGID;
                drIndata["MTRLID"] = sMTRLID;

                if (LoginInfo.CFG_SHOP_ID.Equals("G481"))
                {
                    drIndata["AREAID"] = LoginInfo.CFG_AREA_ID;
                }
                else
                {
                    drIndata["AREAID"] = null;
                }
                    
                dtIndata.Rows.Add(drIndata);

                new ClientProxy().ExecuteService("DA_BAS_SEL_MODLID_BY_MTRLID_MBOM_DETL_CBO", "INDATA", "OUTDATA", dtIndata, (dtResult, ex) =>
                {
                    if (ex != null)
                    {
                        //Util.AlertByBiz("DA_BAS_SEL_MODLID_BY_MTRLID_MBOM_DETL_CBO", ex.Message, ex.ToString());
                        Util.MessageException(ex);
                        return;
                    }

                    //2018-04-12
                    //[TB_SFC_RCV_SUBLOT_INFO] PROD_SCHD_MODL NULL로 업데이트 하기위함.
                    if (dtResult.Rows.Count > 0)
                    {
                        DataRow dr = dtResult.Rows[0];
                        dtResult.ImportRow(dr);

                        dr["CBO_NAME"] = "-ALL-";
                        dr["CBO_CODE"] = null;
                        //dtResult.Rows.Add(dr);
                    }
                    //2018-04-12
                    cboChangeModel.ItemsSource = DataTableConverter.Convert(dtResult);

                    if (dtIndata.Rows.Count > 0)
                    {
                        cboChangeModel.SelectedIndex = 0;
                        //2018-04-12
                        cboChangeModel_SelectedValueChanged(null, null);
                    }
                    else
                    {
                        cboChangeModel_SelectedValueChanged(null, null);
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
                //2018-04-12
                //drIndata["MODLID"] = sMODLID;
                drIndata["MODLID"] = sMODLID == "" ? null : sMODLID;
                drIndata["MTRLID"] = sMTRLID;
                //2018-04-12
                //drIndata["AREAID"] = null;

                if (LoginInfo.CFG_SHOP_ID.Equals("G481"))
                {
                    drIndata["AREAID"] = LoginInfo.CFG_AREA_ID;
                }
                else
                {
                    drIndata["AREAID"] = sTagetArea == "" ? null : sTagetArea;
                }

                    
                dtIndata.Rows.Add(drIndata);
                new ClientProxy().ExecuteService("DA_BAS_SEL_ROUTID_BY_MTRLID_MBOM_DETL_CBO", "RQSTDT", "RSLTDT", dtIndata, (dtResult, ex) =>
                {
                    if (ex != null)
                    {
                        //Util.AlertByBiz("DA_BAS_SEL_ROUTID_BY_MTRLID_MBOM_DETL_CBO", ex.Message, ex.ToString());
                        Util.MessageException(ex);
                        return;
                    }

                    cboChangeRoute.ItemsSource = DataTableConverter.Convert(dtResult);

                    if (dtIndata.Rows.Count > 0)
                    {
                        cboChangeRoute.SelectedIndex = 0;
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
        #endregion

        
    }
}
