/*************************************************************************************
 Created Date : 2018.06.01
      Creator : 
   Decription : 전극 샘플링 정보등록
--------------------------------------------------------------------------------------
 [Change History]
  2018.06.01  DEVELOPER : Initial Created.
  2022.06.28  정재홍    : C20220224-000116 - GMES 슬리터 공정진척 샘플링 팝업 요청 건
    
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
    public partial class CMM_ELEC_SAMPLING : C1Window, IWorkArea    {
        
        #region Initialize 
        /// <summary>
        ///  Frame과 상호작용하기 위한 객체
        /// </summary>
        public IFrameOperation FrameOperation
        {
            get;
            set;
        }
        private string _procID = string.Empty;        // 공정코드

        DataTable dtiUse = new DataTable();
        DataTable dtComp = new DataTable();
        DataTable dtTUse = new DataTable();
        DataTable dtIsnp = new DataTable();

        public CMM_ELEC_SAMPLING()
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
                _procID = Util.NVC(tmps[0]);

                if (_procID.Equals(Process.SLITTING))
                {
                    dgSampling.Columns["QA_INSP_SMPLG"].Visibility = Visibility.Visible;
                }
                else
                {
                    dgSampling.Columns["QA_INSP_SMPLG"].Visibility = Visibility.Collapsed;
                }

                ApplyPermissions();
                setDataTable();
                setHead();

                //// Default 라벨 출력 수량 수정 권한
                //if (IsPersonByAuth(LoginInfo.USERID))
                //{
                //    dgSampling.Columns["BAS_PRT_QTY"].IsReadOnly = false;
                //}                    

                GetSamplingTargetList();
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
            if (dgSampling.ItemsSource == null || dgSampling.Rows.Count < 0)
                return;

            Button button = sender as Button;
            if (button != null)
            {
                CMM_ELEC_SAMPLING_PROD wndPopup = new CMM_ELEC_SAMPLING_PROD();
                wndPopup.FrameOperation = FrameOperation;

                if (wndPopup != null)
                {
                    object[] Parameters = new object[1];
                    Parameters[0] = _procID;

                    C1WindowExtension.SetParameters(wndPopup, Parameters);

                    wndPopup.Closed += new EventHandler(OnCloseProd);
                    this.Dispatcher.BeginInvoke(new Action(() => wndPopup.ShowModal()));
                }
            }


        }

        private void OnCloseProd(object sender, EventArgs e)
        {
            CMM_ELEC_SAMPLING_PROD window = sender as CMM_ELEC_SAMPLING_PROD;
            if (window.DialogResult == MessageBoxResult.OK)
            {
                DataTable dt = ((DataView)dgSampling.ItemsSource).Table;

                DataRow dr = dt.NewRow();
                dr["CHK"] = true;
                dr["USE_FLAG"] = "Y";
                dr["ELTR_TYPE"] = window._GetElecType;
                dr["PRJT_NAME"] = window._GetPrjtName;
                dr["PRODID"] = window._GetProductName;
                dr["BAS_PRT_QTY"] = 2;
                dt.Rows.Add(dr);

                dgSampling.ScrollIntoView(dt.Rows.Count - 1, 0);
            }
        }

        private void btnRemove_Click(object sender, RoutedEventArgs e)
        {
            if (dgSampling.ItemsSource == null || dgSampling.Rows.Count < 0)
                return;

            DataTable dt = ((DataView)dgSampling.ItemsSource).Table;

            for (int i = (dt.Rows.Count - 1); i >= 0; i--)
                if (Convert.ToBoolean(dt.Rows[i]["CHK"]) && string.Equals(dt.Rows[i]["DELETEYN"], "Y"))
                    dt.Rows[i].Delete();
        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            SaveSamplingTargetData();
        }

        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            GetSamplingTargetList();
        }

        private void btnClose_Clicked(object sender, RoutedEventArgs e)
        {
            this.DialogResult = MessageBoxResult.Cancel;
        }

        private void cboIUse_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            GetSamplingTargetList();
        }
        #endregion

        #region Method
        private void setDataTable()
        {
            //this.Loaded -= C1Window_Loaded;
            dtiUse = GetCommonCode("IUSE");
            dtComp = GetCommonCode("COMPANY_CODE");
            dtTUse = GetCommonCode("IUSE");
            dtIsnp = GetCommonCode("QA_INSP_SMPLG_TYPE_CODE");

            DataRow dRow = dtTUse.NewRow();
            dRow["CBO_CODE"] = "ALL";
            dRow["CBO_NAME"] = "-ALL-";
            dtTUse.Rows.InsertAt(dRow, 0);

            cboIUse.ItemsSource = dtTUse.Copy().AsDataView();
            cboIUse.SelectedIndex = 1;
        }

        private void setHead()
        {
            DataTable dt = dtComp.DefaultView.ToTable();

            //사용유무 = 'Y'
            DataRow[] dr = dtComp.Select("ATTRIBUTE5 = 'Y'");

            int _maxColumn = Util.NVC_Int(dtComp.Rows.Count);

            int startcol = dgSampling.Columns["EXCL_FLAG"].Index;
            int forCount = 0;
            string colName;

            C1.WPF.DataGrid.DataGridLength width = new C1.WPF.DataGrid.DataGridLength(1, C1.WPF.DataGrid.DataGridUnitType.Auto);

            for (int col = 0; col < dr.Length; col++)
            {
                forCount++;
                
                colName = "P" + forCount.ToString("00");

                List<string> sHeader = new List<string>() { ObjectDic.Instance.GetObjectName("설비팝업"), dr[forCount -1]["ATTRIBUTE1"].ToString() };

                SetGridCheckBoxColumn(dgSampling, colName, sHeader, false, false, false, false, width, HorizontalAlignment.Center, Visibility.Visible, true, dr[forCount - 1]["CBO_CODE"].ToString());
            }
        }

        private  void SetGridCheckBoxColumn(C1DataGrid C1Grid, string sBinding, List<string> sHeadNames, bool bUserResize, bool bUserSort,
                                            bool bUserFilter, bool bReadOnly, C1.WPF.DataGrid.DataGridLength nHeaderWidth, HorizontalAlignment HorizontalAlignment,
                                            Visibility ColumnVisibility, bool bEditOnSelection, string sTag)
        {
            C1.WPF.DataGrid.DataGridCheckBoxColumn col = new C1.WPF.DataGrid.DataGridCheckBoxColumn();

            Binding databinding = new Binding(sBinding);
            databinding.Mode = BindingMode.TwoWay;

            col.Header = sHeadNames;
            col.Binding = databinding;
            col.CanUserResize = bUserResize;
            col.CanUserSort = bUserSort;
            col.CanUserFilter = bUserFilter;
            col.IsReadOnly = bReadOnly;
            col.Width = nHeaderWidth;
            col.HorizontalAlignment = HorizontalAlignment;
            col.Visibility = ColumnVisibility;
            col.EditOnSelection = bEditOnSelection;
            col.Tag = sTag;
            C1Grid.Columns.Add(col);
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

        private bool IsPersonByAuth(string sUserID)
        {
            try
            {
                DataTable RQSTDT = new DataTable("RQSTDT");
                RQSTDT.Columns.Add("USERID", typeof(string));
                RQSTDT.Columns.Add("AUTHID", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["USERID"] = sUserID;
                dr["AUTHID"] = "ELEC_MANA,MESADMIN,MESDEV";
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_PERSON_BY_AUTH_MULTI", "RQSTDT", "RSLTDT", RQSTDT);

                if (dtResult != null && dtResult.Rows.Count > 0)
                    return true;
            }
            catch (Exception ex) { }

            return false;
        }

        private void GetSamplingTargetList()
        {
            try
            {
                DataTable dt = new DataTable();
                dt.Columns.Add("SHOPID", typeof(string));
                dt.Columns.Add("PROCID", typeof(string));
                dt.Columns.Add("USE_FLAG", typeof(string));
                dt.Columns.Add("PRJT_NAME", typeof(string));

                DataRow dataRow = dt.NewRow();
                dataRow["SHOPID"] = LoginInfo.CFG_SHOP_ID;
                dataRow["PROCID"] = _procID;
                dataRow["USE_FLAG"] = Util.GetCondition(cboIUse);
                dataRow["PRJT_NAME"] = Util.NVC(txtPRJ.Text);
                dt.Rows.Add(dataRow);

                DataTable result = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_LOT_SAMPLE_PROD_T1", "INDATA", "RSLTDT", dt);

                Util.GridSetData(dgSampling, result, FrameOperation, true);
                (dgSampling.Columns["USE_FLAG"] as C1.WPF.DataGrid.DataGridComboBoxColumn).ItemsSource = DataTableConverter.Convert(dtiUse.Copy());
                (dgSampling.Columns["QA_INSP_SMPLG"] as C1.WPF.DataGrid.DataGridComboBoxColumn).ItemsSource = DataTableConverter.Convert(dtIsnp.Copy());

            }
            catch (Exception ex) { Util.MessageException(ex); }
        }

        private void SaveSamplingTargetData()
        {
            Util.MessageConfirm("SFU1241", (result) =>
            {
                if (result == MessageBoxResult.OK)
                {
                    try
                    {
                        ShowLoadingIndicator();

                        string PRID2 = "";
                        DataTable dt = ((DataView)dgSampling.ItemsSource).Table;
                        DataSet indataSet = new DataSet();

                        DataTable inDATA = indataSet.Tables.Add("INDATA");
                        inDATA.Columns.Add("SHOPID", typeof(string));
                        inDATA.Columns.Add("PROCID", typeof(string));
                        inDATA.Columns.Add("PRODID", typeof(string));
                        inDATA.Columns.Add("EXCL_FLAG", typeof(string));
                        inDATA.Columns.Add("USE_FLAG", typeof(string));
                        inDATA.Columns.Add("USERID", typeof(string));
                        inDATA.Columns.Add("BAS_PRT_QTY", typeof(Int16));
                        inDATA.Columns.Add("LABEL_PRT_QTY", typeof(Int16));
                        inDATA.Columns.Add("QA_INSP_SMPLG", typeof(string));

                        DataTable inDETL = indataSet.Tables.Add("INDETL");
                        inDETL.Columns.Add("SHOPID", typeof(string));
                        inDETL.Columns.Add("PROCID", typeof(string));
                        inDETL.Columns.Add("PRODID", typeof(string));
                        inDETL.Columns.Add("COMPANY_CODE", typeof(string));
                        inDETL.Columns.Add("POPUP_FLAG", typeof(string));

                        int iIndex = dgSampling.Columns["DELETEYN"].Index; 

                        foreach (DataRow inRow in dt.Rows)
                        {
                            int shipCount = 0;
                            if (Convert.ToBoolean(inRow["CHK"]))
                            {
                                DataRow newRow = inDATA.NewRow();
                                newRow["SHOPID"] = LoginInfo.CFG_SHOP_ID;
                                newRow["PROCID"] = _procID;
                                newRow["PRODID"] = Util.NVC(inRow["PRODID"]).ToUpper();
                                newRow["EXCL_FLAG"] = Util.NVC(inRow["EXCL_FLAG"]) == "1" ? "Y" : "N";
                                newRow["USE_FLAG"] = Util.NVC(inRow["USE_FLAG"]);
                                newRow["USERID"] = LoginInfo.USERID;
                                newRow["BAS_PRT_QTY"] = Util.NVC_Int(inRow["BAS_PRT_QTY"]);
                                newRow["QA_INSP_SMPLG"] = Util.NVC(inRow["QA_INSP_SMPLG"]);

                                //출하처 Count(발행매수 저장을 위한 Count)
                                for (int i = iIndex + 1; i < inRow.ItemArray.Length - 1; i++)
                                {
                                    shipCount = shipCount + Util.NVC_Int(inRow.ItemArray[i]);
                                }
                                // 발행매수 : 출하지개수 + 1
                                newRow["LABEL_PRT_QTY"] = shipCount + 1;

                                inDATA.Rows.Add(newRow);

                                PRID2 = Util.NVC(inRow["PRODID"]).ToUpper() + "," + PRID2;

                                int colValue = 0;

                                for (int col = iIndex + 1; col < dgSampling.Columns.Count; col++)
                                {
                                    colValue++;
                                    if (colValue > dt.Columns.Count)
                                        break;

                                    DataRow[] dr = dtComp.Select("CBO_CODE = '" + dgSampling.Columns[col].Tag.ToString() + "' AND ATTRIBUTE5 = 'Y'");
                                    if (dr.Length > 0)
                                    {
                                        newRow = null;
                                        newRow = inDETL.NewRow();
                                        newRow["SHOPID"] = LoginInfo.CFG_SHOP_ID;
                                        newRow["PROCID"] = _procID;
                                        newRow["PRODID"] = Util.NVC(inRow["PRODID"]).ToUpper();
                                        newRow["COMPANY_CODE"] = Util.NVC(dr[0][0]);
                                        newRow["POPUP_FLAG"] = Util.NVC(inRow[col]) == "1" ? "Y" : "N";

                                        inDETL.Rows.Add(newRow);
                                    }
                                }
                            }                            
                        }
                        new ClientProxy().ExecuteService_Multi("BR_PRD_REG_SAMPLE_ELTR", "INDATA,INDETL", null, (bizResult, bizException) =>
                        {
                            try
                            {
                                if (bizException != null)
                                {
                                    HiddenLoadingIndicator();
                                    Util.MessageException(bizException);
                                    return;
                                }

                                GetSamplingTargetList();
                                Util.MessageInfo("SFU1275");    //정상 처리 되었습니다.

                                CMM_ELEC_SAMPLING_POPUP popup = new CMM_ELEC_SAMPLING_POPUP();

                                if (popup != null)
                                {

                                    object[] Parameters = new object[1];
                                    Parameters[0] = PRID2;
                                    C1WindowExtension.SetParameters(popup, Parameters);

                                    popup.owner = this;
                                    popup.ShowModal();
                                    popup.CenterOnScreen();
                                    EventHandler popup_Closed = null;
                                    popup.Closed -= popup_Closed;

                                    popup.FrameOperation = this.FrameOperation;
                                }
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
