/*************************************************************************************
 Created Date : 2018.10.26
      Creator : 
   Decription : 전극 슬러리 반제품 물성관리
--------------------------------------------------------------------------------------
 [Change History]
  2018.10.26  DEVELOPER : Initial Created.
    
**************************************************************************************/

using C1.WPF;
using C1.WPF.DataGrid;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using System;
using System.Collections.Generic;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Linq;

namespace LGC.GMES.MES.CMM001
{
    /// <summary>
    /// CMM_ELEC_SAMPLING.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class CMM_ELEC_SLURRY_CONF : C1Window, IWorkArea    {
        
        #region Initialize 
        /// <summary>
        ///  Frame과 상호작용하기 위한 객체
        /// </summary>
        public IFrameOperation FrameOperation
        {
            get;
            set;
        }
        private string _AREAID = string.Empty;

        DataTable dtiUse = new DataTable();
        DataTable dtTUse = new DataTable();
        DataTable dtSlurry = new DataTable();

        public CMM_ELEC_SLURRY_CONF()
        {
            InitializeComponent();
        }
        #endregion

        #region Loaded Event
        private void C1Window_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                object[] tmps = C1WindowExtension.GetParameters(this);
                _AREAID = Util.NVC(tmps[0]);

                ApplyPermissions();
                //InitCombo();
                setDataTable();
                getSlurryConfList();
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        #endregion

        #region Event Method
        private void dgSampling_BeginningEdit(object sender, C1.WPF.DataGrid.DataGridBeginningEditEventArgs e)
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

        private void btnAdd_Click(object sender, RoutedEventArgs e)
        {
            if (dgSlurryConf.ItemsSource == null || dgSlurryConf.Rows.Count < 0)
                return;

            //DataTable dt = ((DataView)dgSlurryConf.ItemsSource).Table;

            //DataRow dr = dt.NewRow();
            //dr["CHK"] = true;
            //dr["AREAID"] = LoginInfo.CFG_AREA_ID;
            //dr["PRODID"] = Util.NVC(cboProdName.Text);
            //dr["SLURRY_CONF_CODE"] = "Y";
            //dr["DELETE_YN"] = "Y";
            //dt.Rows.Add(dr);

            //dgSlurryConf.ScrollIntoView(dt.Rows.Count - 1, 0);

            Button button = sender as Button;
            if (button != null)
            {
                CMM_ELEC_SLURRY_PROD wndPopup = new CMM_ELEC_SLURRY_PROD();
                wndPopup.FrameOperation = FrameOperation;

                if (wndPopup != null)
                {
                    wndPopup.Header = ObjectDic.Instance.GetObjectName("제품등록");
                    object[] Parameters = new object[3];
                    Parameters[0] = Process.MIXING;
                    Parameters[1] = LoginInfo.CFG_EQSG_ID;

                    C1WindowExtension.SetParameters(wndPopup, Parameters);

                    wndPopup.Closed += new EventHandler(OnCloseProd);
                    this.Dispatcher.BeginInvoke(new Action(() => wndPopup.ShowModal()));
                }
            }

        }
        private void OnCloseProd(object sender, EventArgs e)
        {
            CMM_ELEC_SLURRY_PROD window = sender as CMM_ELEC_SLURRY_PROD;
            if (window.DialogResult == MessageBoxResult.OK)
            {
                DataTable dt = ((DataView)dgSlurryConf.ItemsSource).Table;

                DataRow dr = dt.NewRow();
                dr["CHK"] = true;
                dr["USE_FLAG"] = "Y";
                dr["AREAID"] = _AREAID;
                dr["PRODID"] = window._GetProductName;
                dr["SLURRY_CONF_CODE"] = string.Empty;
                dr["DELETE_YN"] = "Y";
                dt.Rows.Add(dr);

                dgSlurryConf.ScrollIntoView(dt.Rows.Count - 1, 0);
            }
        }

        private void btnRemove_Click(object sender, RoutedEventArgs e)
        {
            if (dgSlurryConf.ItemsSource == null || dgSlurryConf.Rows.Count < 0)
                return;

            DataTable dt = ((DataView)dgSlurryConf.ItemsSource).Table;

            for (int i = (dt.Rows.Count - 1); i >= 0; i--)
                if (Convert.ToBoolean(dt.Rows[i]["CHK"]) && string.Equals(dt.Rows[i]["DELETE_YN"], "Y"))
                    dt.Rows[i].Delete();
        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            saveSlurryConfCode();
        }

        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            getSlurryConfList();
        }

        private void btnClose_Clicked(object sender, RoutedEventArgs e)
        {
            this.DialogResult = MessageBoxResult.Cancel;
        }

        private void cboIUse_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            getSlurryConfList();
        }
        #endregion

        #region Method
        private void InitCombo()
        {
            CommonCombo _combo = new CommonCombo();
            _combo.SetCombo(cboProdName, CommonCombo.ComboStatus.SELECT, sCase: "PRODUCT");
        }

        private void setDataTable()
        {
            dtSlurry = getAreaCommonCode("SLURRY_CONF_CODE");
            dtiUse = GetCommonCode("IUSE");

            dtTUse = GetCommonCode("IUSE");
            DataRow dRow = dtTUse.NewRow();
            dRow["CBO_CODE"] = "ALL";
            dRow["CBO_NAME"] = "-ALL-";
            dtTUse.Rows.InsertAt(dRow, 0);
            cboIUse.ItemsSource = dtTUse.Copy().AsDataView();
            cboIUse.SelectedIndex = 1;
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

        private DataTable getAreaCommonCode(string sType)
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("AREAID", typeof(string));
                RQSTDT.Columns.Add("COM_TYPE_CODE", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["AREAID"] = LoginInfo.CFG_AREA_ID;
                dr["COM_TYPE_CODE"] = sType;
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_AREA_COM_CODE_BY_TYPE_CODE_CBO", "RQSTDT", "RSLTDT", RQSTDT);
                if (dtResult != null && dtResult.Rows.Count > 0)
                    return dtResult;
            }
            catch (Exception ex) { }

            return new DataTable();
        }

        private void getSlurryConfList()
        {
            try
            {
                DataTable dt = new DataTable();
                dt.Columns.Add("AREAID", typeof(string));
                dt.Columns.Add("PRODID", typeof(string));
                dt.Columns.Add("USE_FLAG", typeof(string));

                DataRow dataRow = dt.NewRow();
                dataRow["AREAID"] = _AREAID;
                dataRow["PRODID"] = Util.NVC(txtProduct.Text);
                dataRow["USE_FLAG"] = Util.GetCondition(cboIUse);

                dt.Rows.Add(dataRow);

                DataTable result = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_AREA_SLURRY_CONF", "INDATA", "RSLTDT", dt);

                Util.GridSetData(dgSlurryConf, result, FrameOperation);

                (dgSlurryConf.Columns["USE_FLAG"] as C1.WPF.DataGrid.DataGridComboBoxColumn).ItemsSource = DataTableConverter.Convert(dtiUse.Copy());
                (dgSlurryConf.Columns["SLURRY_CONF_CODE"] as C1.WPF.DataGrid.DataGridComboBoxColumn).ItemsSource = DataTableConverter.Convert(dtSlurry.Copy());

            }
            catch (Exception ex) { Util.MessageException(ex); }
        }

        private void saveSlurryConfCode()
        {
            Util.MessageConfirm("SFU1241", (result) =>
            {
                if (result == MessageBoxResult.OK)
                {
                    try
                    {
                        ShowLoadingIndicator();
                        DataTable dt = ((DataView)dgSlurryConf.ItemsSource).Table;
                        DataSet indataSet = new DataSet();

                        DataTable inDATA = indataSet.Tables.Add("INDATA");
                        inDATA.Columns.Add("AREAID", typeof(string));
                        inDATA.Columns.Add("PRODID", typeof(string));
                        inDATA.Columns.Add("SLURRY_CONF_CODE", typeof(string));
                        inDATA.Columns.Add("USE_FLAG", typeof(string));
                        inDATA.Columns.Add("USERID", typeof(string));

                        foreach (DataRow inRow in dt.Rows)
                        {
                            if (Convert.ToBoolean(inRow["CHK"]))
                            {
                                if (string.IsNullOrEmpty(Util.NVC(inRow["PRODID"])))
                                {
                                    Util.MessageValidation("SFU2949");  //제품ID를 입력하세요.
                                    return;
                                }
                                if (string.IsNullOrEmpty(Util.NVC(inRow["SLURRY_CONF_CODE"])))
                                {
                                    Util.MessageValidation("SFU5049");  //물성정보를 선택하십시오
                                    return;
                                }
                                DataRow newRow = inDATA.NewRow();
                                newRow["AREAID"] = LoginInfo.CFG_AREA_ID;
                                newRow["PRODID"] = Util.NVC(inRow["PRODID"]).ToUpper();
                                newRow["SLURRY_CONF_CODE"] = Util.NVC(inRow["SLURRY_CONF_CODE"]);
                                newRow["USE_FLAG"] = Util.NVC(inRow["USE_FLAG"]);
                                newRow["USERID"] = LoginInfo.USERID;
                                inDATA.Rows.Add(newRow);
                            }                            
                        }
                        new ClientProxy().ExecuteService("BR_PRD_REG_SLURRY_CONF_ELTR", "INDATA", null, inDATA, (bizResult, bizException) =>
                        {
                            loadingIndicator.Visibility = Visibility.Collapsed;

                            if (bizException != null)
                            {
                                HiddenLoadingIndicator();
                                Util.MessageException(bizException);
                                return;
                            }
                            Util.AlertInfo("SFU1275");  //정상처리되었습니다.
                        });


                    }
                    catch (Exception ex)
                    {
                        Util.MessageException(ex);
                    }
                    finally
                    {
                        HiddenLoadingIndicator();
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
        #endregion

        #region Authrity
        private void ApplyPermissions()
        {
            List<Button> listAuth = new List<Button>();
            listAuth.Add(btnSave);

            Util.pageAuth(listAuth, FrameOperation.AUTHORITY);
        }
        #endregion

    }
}
