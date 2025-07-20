/*************************************************************************************
 Created Date : 2023.06.16
      Creator : 
   Decription : 포장 출고 (Location 관리) - SHIPPING_ORDER
--------------------------------------------------------------------------------------
 [Change History]
  2016.06.16  DEVELOPER : Initial Created.
  2023.07.21  주재홍 : 현장 테스트후 3차 개선
  2023.07.31  주재홍 : BizAct 다국어

**************************************************************************************/

using C1.WPF;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using Microsoft.Win32;
using System;
using System.Data;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using LGC.GMES.MES.CMM001.Extensions;
using LGC.GMES.MES.CMM001.Popup;
using LGC.GMES.MES.CMM001;
using System.Collections;
using LGC.GMES.MES.COM001;
using System.Windows.Controls;
using C1.WPF.DataGrid;

namespace LGC.GMES.MES.COM001
{
    public partial class COM001_383_SHIPPING_ORDER : C1Window, IWorkArea
    {
        #region Declaration & Constructor 
        Util _Util = new Util();

        private string sAREAID = string.Empty;
        private string sWH_PHYS_PSTN_CODE = string.Empty;
        private string sWH_ID = string.Empty;
        private string sRACK_ID = string.Empty;
        private string sPRJT_NAME = string.Empty;

        public COM001_383_SHIPPING_ORDER()
        {
            InitializeComponent();
        }

        /// <summary>
        ///  Frame과 상호작용하기 위한 객체
        /// </summary>
        public IFrameOperation FrameOperation
        {
            get;
            set;
        }
        #endregion

        #region Initialize
        private void C1Window_Loaded(object sender, RoutedEventArgs e)
        {
            object[] tmps = C1WindowExtension.GetParameters(this);

            if (tmps == null)
            {
                this.DialogResult = MessageBoxResult.Cancel;
                this.Close();
            }

            InitCombo();

            SearchTopProduct();

            this.Loaded -= C1Window_Loaded;
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

        #region Event
        private void InitCombo()
        {
            CommonCombo _combo = new CommonCombo();

            SetcboTOSLOC(cboTOSLOC);
            SetcboSHIPTO(cboSHIPTO);
        }


        private static DataTable AddStatus(DataTable dt, string sValue, string sDisplay, string statusType)
        {
            DataRow dr = dt.NewRow();
            switch (statusType)
            {
                case "ALL":
                    dr[sDisplay] = "-ALL-";
                    dr[sValue] = "";
                    dt.Rows.InsertAt(dr, 0);
                    break;

                case "SELECT":
                    dr[sDisplay] = "-SELECT-";
                    dr[sValue] = "SELECT";
                    dt.Rows.InsertAt(dr, 0);
                    break;

                case "NA":
                    dr[sDisplay] = "-N/A-";
                    dr[sValue] = "";
                    dt.Rows.InsertAt(dr, 0);
                    break;

                case "EMPTY":
                    dr[sValue] = string.Empty;
                    dr[sDisplay] = string.Empty;
                    dt.Rows.InsertAt(dr, 0);
                    break;
            }

            return dt;
        }



        private void SetcboTOSLOC(C1ComboBox cbo)
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("AREAID", typeof(string));
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("SLOC_TYPE_CODE", typeof(string)); 

                DataRow dr = RQSTDT.NewRow();
                dr["AREAID"] = LoginInfo.CFG_AREA_ID;
                dr["LANGID"] = LoginInfo.LANGID;
                //dr["SLOC_TYPE_CODE"] = SLOC_TYPE_CODE.SLOC05;  // 물류창고

                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_TOSLOC_BY_AREA_FORM", "RQSTDT", "RSLTDT", RQSTDT);
                cbo.DisplayMemberPath = "CBO_NAME";
                cbo.SelectedValuePath = "CBO_CODE";

                cbo.ItemsSource = AddStatus(dtResult, "CBO_CODE", "CBO_NAME", "SELECT").Copy().AsDataView();

                cbo.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }


        private void SetcboSHIPTO(C1ComboBox cbo)
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("AREAID", typeof(string));
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("TO_SLOC_ID", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["AREAID"] = LoginInfo.CFG_AREA_ID;
                dr["LANGID"] = LoginInfo.LANGID;
                dr["TO_SLOC_ID"] = cboTOSLOC.SelectedValue.ToString();

                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_SHIPSLOC_BY_AREA_FORM", "RQSTDT", "RSLTDT", RQSTDT);
                cbo.DisplayMemberPath = "CBO_NAME";
                cbo.SelectedValuePath = "CBO_CODE";

                cbo.ItemsSource = AddStatus(dtResult, "CBO_CODE", "CBO_NAME", "SELECT").Copy().AsDataView();

                cbo.SelectedIndex = 0;

            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }


        private void SetcboSection(C1ComboBox cbo)
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("AREAID", typeof(string));
                RQSTDT.Columns.Add("WH_PHYS_PSTN_CODE", typeof(string));
                RQSTDT.Columns.Add("LANGID", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["AREAID"] = sAREAID;
                dr["WH_PHYS_PSTN_CODE"] = sWH_PHYS_PSTN_CODE;
                dr["LANGID"] = LoginInfo.LANGID;

                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_GET_COMBO_SECTION_BY_BLDG", "RQSTDT", "RSLTDT", RQSTDT);
                cbo.DisplayMemberPath = "WH_NAME";
                cbo.SelectedValuePath = "WH_ID";

                cbo.ItemsSource = AddStatus(dtResult, "WH_ID", "WH_NAME", "ALL").Copy().AsDataView();

                if (sWH_ID != null && !sWH_ID.Equals(""))
                {
                    for (int i = 0; i < dtResult.Rows.Count; i++)
                    {
                        DataRow row = dtResult.Rows[i];
                        if (sWH_ID.Equals(row["WH_ID"].ToString()))
                            cbo.SelectedIndex = i;
                    }
                }
                else
                {
                    cbo.SelectedIndex = 0;
                }

            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }


        private void popSearchProdID_ValueChanged(object sender, EventArgs e)
        {
            SearchTopProduct();
        }


        private void SearchTopProduct()
        {
            DataTable RQSTDT = new DataTable();
            RQSTDT.TableName = "RQSTDT";
            RQSTDT.Columns.Add("LANGID", typeof(string));

            DataRow newRow = RQSTDT.NewRow();
            newRow["LANGID"] = LoginInfo.LANGID;
            RQSTDT.Rows.Add(newRow);

            ShowLoadingIndicator();

            new ClientProxy().ExecuteService("DA_BAS_SEL_PRODUCT_PRJT_NAME_CBO", "RQSTDT", "RSLTDT", RQSTDT, (searchResult, bizex) =>
            {
                HiddenLoadingIndicator();
                try
                {
                    if (bizex != null)
                    {
                        Util.MessageException(bizex);
                        return;
                    }
                    popSearchTopProdID.ItemsSource = DataTableConverter.Convert(searchResult);
                }
                catch (Exception ex)
                {
                    HiddenLoadingIndicator();
                    Util.MessageException(ex);
                }
            });
           
        }

        public void SaveProcess()
        {

            DataSet inDataSet = null;
            inDataSet = new DataSet();
            DataTable IN_REQ = inDataSet.Tables.Add("IN_REQ");
            DataTable IN_RSPN_AREA = inDataSet.Tables.Add("IN_RSPN_AREA");
            IN_REQ.TableName = "IN_REQ";
            IN_REQ.Columns.Add("SHOPID", typeof(string));
            IN_REQ.Columns.Add("CELL_SPLY_STAT_CODE", typeof(string));
            //IN_REQ.Columns.Add("CELL_SPLY_TYPE_CODE", typeof(string));
            //IN_REQ.Columns.Add("AUTO_LOGIS_FLAG", typeof(string));
            IN_REQ.Columns.Add("AREAID", typeof(string));
            IN_REQ.Columns.Add("SLOC_ID", typeof(string));
            IN_REQ.Columns.Add("PRODID", typeof(string));
            IN_REQ.Columns.Add("REQ_QTY", typeof(string));
            IN_REQ.Columns.Add("NOTE", typeof(string));
            //IN_REQ.Columns.Add("AN_COATER_EQPTID", typeof(string));
            //IN_REQ.Columns.Add("CA_COATER_EQPTID", typeof(string));
            //IN_REQ.Columns.Add("PKG_EQPTID", typeof(string));
            IN_REQ.Columns.Add("INSUSER", typeof(string));
            IN_REQ.Columns.Add("UPDUSER", typeof(string));
            //IN_REQ.Columns.Add("PLLT_UNIT_CELL_REQ_QTY", typeof(string));
            IN_REQ.Columns.Add("REQ_DTTM", typeof(string));
            IN_REQ.Columns.Add("REQ_USERID", typeof(string));
            IN_REQ.Columns.Add("SHIPTO_ID", typeof(string));
            //IN_REQ.Columns.Add("TO_EQSGID", typeof(string));

            IN_RSPN_AREA.TableName = "IN_RSPN_AREA";
            IN_RSPN_AREA.Columns.Add("CELL_SPLY_RSPN_AREAID", typeof(string));

            DataRow dr = IN_REQ.NewRow();
            dr["SHOPID"] = LoginInfo.CFG_SHOP_ID;
            dr["CELL_SPLY_STAT_CODE"] = "REQUEST";
            //dr["CELL_SPLY_TYPE_CODE"] = ??
            //dr["AUTO_LOGIS_FLAG"] = ??
            dr["AREAID"] = LoginInfo.CFG_AREA_ID;
            dr["SLOC_ID"] = cboTOSLOC.SelectedValue;
            dr["PRODID"] = popSearchTopProdID.SelectedValue;
            dr["REQ_QTY"] = txtPalletQTY.Text;
            dr["NOTE"] = txtNote.Text;
            //dr["AN_COATER_EQPTID"] = ??
            //dr["CA_COATER_EQPTID"] = ??
            //dr["PKG_EQPTID"] = ??
            dr["INSUSER"] = LoginInfo.USERID;
            dr["UPDUSER"] = LoginInfo.USERID;
            //dr["PLLT_UNIT_CELL_REQ_QTY"] = ??
            dr["REQ_DTTM"] = dtpDateFrom.SelectedDateTime.ToString("yyyy-MM-dd");
            dr["REQ_USERID"] = txtRequestor.Tag;
            dr["SHIPTO_ID"] = cboSHIPTO.SelectedValue;
            //dr["TO_EQSGID"] = ??

            IN_REQ.Rows.Add(dr);

            DataRow drbox = IN_RSPN_AREA.NewRow();
            drbox["CELL_SPLY_RSPN_AREAID"] = LoginInfo.CFG_AREA_ID;
            IN_RSPN_AREA.Rows.Add(drbox);

            try
            {

                ShowLoadingIndicator();

                new ClientProxy().ExecuteService_Multi("BR_PRD_REG_CELL_PLLT_SHIP_ORDER_TO_CUST", "IN_REQ,IN_RSPN_AREA", null, (result, bizex) =>
                {
                    HiddenLoadingIndicator();
                    if (bizex != null)
                    {
                        Util.MessageException(bizex);
                        this.txtPalletQTY.Focus();
                        return;
                    }

                    //정상처리되었습니다. 
                    LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("SFU1275"), null, "Info", MessageBoxButton.OK, MessageBoxIcon.None, (Result) =>
                    {
                            
                    });
                    this.DialogResult = MessageBoxResult.OK;
                }, inDataSet);

            }
            catch (Exception ex)
            {
                HiddenLoadingIndicator();

                this.DialogResult = MessageBoxResult.Cancel;
                Util.MessageException(ex);
            }

        }


        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = MessageBoxResult.Cancel;
        }

        #endregion

        #region Mehod
        private void Set_PopUp_ProductID()
        {
            try
            {
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        
        
        private void InitializeControls()
        {
        }
        #endregion

        private void cboTOSLOC_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            SetcboSHIPTO(cboSHIPTO);
        }

        private void btnRequestor_Click(object sender, RoutedEventArgs e)
        {
            CMM_PERSON wndPerson = new CMM_PERSON();
            wndPerson.FrameOperation = FrameOperation;

            if (wndPerson != null)
            {
                object[] Parameters = new object[1];
                Parameters[0] = txtRequestor.Text;
                C1WindowExtension.SetParameters(wndPerson, Parameters);

                wndPerson.Closed += new EventHandler(wndUser_Closed);


                this.Dispatcher.BeginInvoke(new Action(() => wndPerson.ShowModal()));

                /*
                foreach (Grid tmp in Util.FindVisualChildren<Grid>(Application.Current.MainWindow))
                {
                    if (tmp.Name == "grdMain")
                    {
                        tmp.Children.Add(wndPerson);
                        wndPerson.BringToFront();
                        break;
                    }
                }
                */
            }

        }

        private void wndUser_Closed(object sender, EventArgs e)
        {
            CMM_PERSON wndPerson = sender as CMM_PERSON;
            if (wndPerson.DialogResult == MessageBoxResult.OK)
            {
                txtRequestor.Text = wndPerson.USERNAME;
                txtRequestor.Tag = wndPerson.USERID;
            }

            foreach (Grid tmp in Util.FindVisualChildren<Grid>(Application.Current.MainWindow))
            {
                if (tmp.Name == "grdMain")
                {
                    tmp.Children.Remove(wndPerson);
                    break;
                }
            }
        }


        private void txtPalletQTY_KeyDown(object sender, KeyEventArgs e)
        {

        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            if (popSearchTopProdID.SelectedValue == null)
            {
                
                Util.MessageValidation("SFU1182");  // PRODUCT 를 선택하세요.
                popSearchTopProdID.Focus();
                return;

            }

            string _prodId = popSearchTopProdID.SelectedValue.ToString();
            if (_prodId == null || string.IsNullOrWhiteSpace(_prodId))
            {
                Util.MessageValidation("SFU1182"); // PRODUCT 를 선택하세요.
                popSearchTopProdID.Focus();
                return;
            }

            string _whId = cboTOSLOC.SelectedValue.ToString();
            if (_whId == null || string.IsNullOrWhiteSpace(_whId) || _whId.Equals("SELECT"))
            {
               
                Util.MessageValidation("SFU2069");   // 입고창고를 선택하세요.
                cboTOSLOC.Focus();
            }

            string _outWh = cboSHIPTO.SelectedValue.ToString();
            if (_outWh == null || string.IsNullOrWhiteSpace(_outWh) || _outWh.Equals("SELECT"))
            {
                
                Util.MessageValidation("SFU4096");  // 출하처를 선택하세요.
                cboSHIPTO.Focus();
            }

            string _sPalletQTY = txtPalletQTY.Text;
            int _sintPalletQTY = 0;
            if (!string.IsNullOrWhiteSpace(_sPalletQTY) && !int.TryParse(_sPalletQTY, out _sintPalletQTY))
            {
               
                Util.MessageInfo("SFU3435");   //SFU3435	숫자만 입력해주세요
                txtPalletQTY.Text = "0";
                txtPalletQTY.Focus();
                return;
            }

            if (string.IsNullOrEmpty(txtRequestor.Text))
            {
                
                Util.MessageValidation("SFU4976");  // 요청자를 확인 하세요.
                txtRequestor.Focus();
                return;
            }

            if (txtRequestor.Tag == null)
            {
               
                Util.MessageValidation("SFU4976");  // 요청자를 확인 하세요.
                txtRequestor.Focus();
                txtRequestor.Text = string.Empty;
                return;
            }

            if (txtRequestor.Tag.ToString() == "")
            {
               
                Util.MessageValidation("SFU4976");  // 요청자를 확인 하세요.
                txtRequestor.Focus();
                txtRequestor.Text = string.Empty;
                return;
            }


            if (Convert.ToDecimal(DateTime.Now.ToString("yyyyMMdd")) > Convert.ToDecimal(dtpDateFrom.SelectedDateTime.ToString("yyyyMMdd")))
            {
                dtpDateFrom.Text = System.DateTime.Now.ToLongDateString();
                dtpDateFrom.SelectedDateTime = System.DateTime.Now;
                
                Util.MessageValidation("SFU1738");     //오늘 이전 날짜는 선택할 수 없습니다.
                dtpDateFrom.Focus();
                return;
            }


            Util.MessageConfirm("SFU8567", result =>   // 출고요청 진행 하시겠습니까?
            {
                if (result == MessageBoxResult.OK)
                {
                    SaveProcess();
                }
            });

        }

        private void txtRequestor_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                btnRequestor_Click(sender, e);
            }
        }

        private void dtpDateFrom_SelectedDataTimeChanged(object sender, SelectionChangedEventArgs e)
        {
            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                DateTime baseDate = DateTime.Now;
                if (Convert.ToDecimal(baseDate.ToString("yyyyMMdd")) > Convert.ToDecimal(dtpDateFrom.SelectedDateTime.ToString("yyyyMMdd")))
                {
                    dtpDateFrom.Text = baseDate.ToLongDateString();
                    dtpDateFrom.SelectedDateTime = baseDate;
                    Util.MessageValidation("SFU1738");  //오늘 이전 날짜는 선택할 수 없습니다.
                    return;
                }
            }));

            //this.BringToFront();
        }
    }
}
