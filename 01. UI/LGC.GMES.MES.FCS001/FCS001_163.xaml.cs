/*************************************************************************************
 Created Date : 2023.08.31
      Creator : 이의철
   Decription : J/F 설비별 LOT 설정
--------------------------------------------------------------------------------------
 [Change History]
  2023.09.21  이의철 : Initial Created.
  2023.10.07  이의철 : Update eqpt, route combobox in Grid
  2023.1025   이의철 : 입력 제한 수정
    1.라우트 정보 키인 변경
    2.키인 항목 사이즈 제한 
        - 공정 경로 :6자리
        - 조립 Lot Id : 8 자리
        - 생산 Lot Id : 10 자리
    3.항목 명칭 변경
        - 특별 LOT 출고 금지 여부 -> 특별 Tray 출고 금지 여부
    4.설비코드 -> 설비명 보이게 수정
  2023.11.14  이의철 : 라우트 정보 콤보박스 버전으로 재수정
  2023.11.25  이의철 : this.Loaded -= UserControl_Loaded; 추가
  2023.12.04  이의철 : Lane, route 콤보 수정
  2023.12.15  이의철 : 초기 로딩시 설비 콤보에서 LANEID_LIST 추가
**************************************************************************************/
using C1.WPF;
using C1.WPF.DataGrid;
using C1.WPF.DataGrid.Summaries;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using System;
using System.Collections.Generic;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using LGC.GMES.MES.CMM001;

namespace LGC.GMES.MES.FCS001
{
    public partial class FCS001_163 : UserControl, IWorkArea
    {
        #region Declaration & Constructor 

        Util _Util = new Util();
        private string _sLANE_ID = string.Empty;

        public FCS001_163()
        {
            InitializeComponent();
        }
        #endregion

        #region Initialize
        /// <summary>
        /// Frame과 상호작용하기 위한 객체
        /// </summary>
        public IFrameOperation FrameOperation
        {
            get;
            set;
        }


        #endregion


        #region load
        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                InitCombo();

                InitSpread();

                GetList();

                this.Loaded -= UserControl_Loaded;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        
        private void InitCombo()
        {

            CommonCombo_Form _combo = new CommonCombo_Form();

            string[] sFilterLane = { "J" };
            //_combo.SetCombo(cboLane, CommonCombo_Form.ComboStatus.ALL, sCase: "LANE", sFilter: sFilterLane);
            LaneSetCboItem(cboLane, sFilter: sFilterLane);

            //grid combo setting
            LaneSetGridCboItem(dgList.Columns["LANE_ID"], sFilter: sFilterLane);
            //JGEqptGridColItem(dgList.Columns["EQPTID"], "", true);
            //RouteGridColItem(dgList.Columns["ROUTID"], "", true);
            //입력 제한 수정
            string[] sFilter = {null,null,null,"A3XJ1X1,A3XJ2X1" };
            EqptSetGridCboItem(dgList.Columns["EQPTID"], "", true, sFilter);
        }

        private void InitSpread()
        {
            
        }

        #endregion

        #region Event
        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            GetList();
        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {

            if (!dgList.IsCheckedRow("CHK"))
            {
                Util.Alert("SFU1645"); //선택된 작업대상이 없습니다.
                return;
            }

            //중복 확인
            for (int i = 0; i < dgList.GetRowCount(); i++)
            {
                if (Util.NVC(DataTableConverter.GetValue(dgList.Rows[i].DataItem, "CHK")).Equals("True"))
                {
                    if (!CheckinputValue("EQPTID", i))
                    {
                        string sValue = Util.NVC(DataTableConverter.GetValue(dgList.Rows[i].DataItem, "EQPTID"));
                        Util.MessageValidation("SFU3471", sValue);//[%1]은 이미 등록되었습니다.  
                        return;
                    }

                    //Assy_lot 확인
                    string DAY_GR_LOTID = Util.NVC(DataTableConverter.GetValue(dgList.Rows[i].DataItem, "DAY_GR_LOTID"));
                    if(!string.IsNullOrEmpty(DAY_GR_LOTID))
                    {
                        if(DAY_GR_LOTID.Length < 8)
                        {
                            // SFU4148	조립LOTID는 8자리 이상입니다.
                            Util.MessageValidation("SFU4148");                            
                            return;
                        }
                    }
                    //prod lot 확인
                    string PROD_LOTID = Util.NVC(DataTableConverter.GetValue(dgList.Rows[i].DataItem, "PROD_LOTID"));
                    if (!string.IsNullOrEmpty(PROD_LOTID))
                    {
                        if (PROD_LOTID.Length < 9)
                        {
                            // Lot ID는 9자리 이상 넣어 주세요.
                            Util.MessageValidation("SFU3608");
                            return;
                        }
                    }

                    //route 확인     
                    //string ROUTID = Util.NVC(DataTableConverter.GetValue(dgList.Rows[i].DataItem, "ROUTID"));
                    //if(!string.IsNullOrEmpty(ROUTID))
                    //{
                    //    if (CheckRoute(ROUTID).Equals(false))
                    //    {
                    //        //Route Not Exist
                    //        Util.MessageInfo("FM_ME_0102");//공정경로가 존재하지 않습니다.
                    //        return;
                    //    }
                    //}                    
                }
            }


            
            try
            {
                try
                {
                    //저장하시겠습니까?
                    Util.MessageConfirm("FM_ME_0214", (re) =>
                    {
                        if (re != MessageBoxResult.OK)
                        {
                            return;
                        }

                        int sum = 0;
                        for (int i = 0; i < dgList.GetRowCount(); i++)
                        {
                            if (Util.NVC(DataTableConverter.GetValue(dgList.Rows[i].DataItem, "CHK")).Equals("True"))
                                sum++;
                        }

                        if (sum < 1)
                        {
                            //선택된 항목이 없습니다.
                            Util.MessageInfo("SFU1651");
                            return;
                        }
                        
                        DataTable dtRqst = new DataTable();
                        dtRqst.TableName = "INDATA";
                        dtRqst.Columns.Add("EQPTID", typeof(string));
                        dtRqst.Columns.Add("DAY_GR_LOTID", typeof(string));
                        dtRqst.Columns.Add("PROD_LOTID", typeof(string));
                        dtRqst.Columns.Add("ROUTID", typeof(string));
                        dtRqst.Columns.Add("SPCL_LOT_ISS_PRHB_FLAG", typeof(string));
                        dtRqst.Columns.Add("USE_FLAG", typeof(string));
                        dtRqst.Columns.Add("USERID", typeof(string));

                        for (int i = 0; i < dgList.GetRowCount(); i++)
                        {
                            if (Util.NVC(DataTableConverter.GetValue(dgList.Rows[i].DataItem, "CHK")).Equals("True"))
                            {
                                DataRow dr = dtRqst.NewRow();
                                dr["EQPTID"] = Util.NVC(DataTableConverter.GetValue(dgList.Rows[i].DataItem, "EQPTID"));
                                dr["DAY_GR_LOTID"] = Util.NVC(DataTableConverter.GetValue(dgList.Rows[i].DataItem, "DAY_GR_LOTID"));
                                dr["PROD_LOTID"] = Util.NVC(DataTableConverter.GetValue(dgList.Rows[i].DataItem, "PROD_LOTID"));
                                dr["ROUTID"] = Util.NVC(DataTableConverter.GetValue(dgList.Rows[i].DataItem, "ROUTID"));
                                dr["SPCL_LOT_ISS_PRHB_FLAG"] = Util.NVC(DataTableConverter.GetValue(dgList.Rows[i].DataItem, "SPCL_LOT_ISS_PRHB_FLAG"));
                                dr["USE_FLAG"] = Util.NVC(DataTableConverter.GetValue(dgList.Rows[i].DataItem, "USE_FLAG"));
                                dr["USERID"] = LoginInfo.USERID;
                                dtRqst.Rows.Add(dr);
                            }
                        }

                        ShowLoadingIndicator();
                        
                        new ClientProxy().ExecuteService("BR_SET_TB_SFC_JIG_EQPT_LOT_SET", "INDATA", null, dtRqst, (bizResult, bizException) =>
                        {
                            try
                            {
                                if (bizException != null)
                                {
                                    Util.MessageException(bizException);
                                    return;
                                }

                                //저장하였습니다.
                                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("FM_ME_0215"), null, "Information", MessageBoxButton.OK, MessageBoxIcon.None, (result) =>
                                {
                                    if (result == MessageBoxResult.OK)
                                    {
                                        GetList();
                                    }
                                });
                            }
                            catch (Exception ex)
                            {
                                HiddenLoadingIndicator();
                                Util.MessageException(ex);
                            }
                            finally
                            {
                                HiddenLoadingIndicator();
                            }
                        });
                    });

                }
                catch (Exception ex)
                {
                    Util.MessageException(ex);
                }
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
            finally
            {
                HiddenLoadingIndicator();
            }           
        }     
        

        private void dgList_LoadedRowHeaderPresenter(object sender, C1.WPF.DataGrid.DataGridRowEventArgs e)
        {
            if (e.Row.HeaderPresenter == null)
            {
                return;
            }

            e.Row.HeaderPresenter.Content = null;
            C1DataGrid dataGrid = e.Row.DataGrid as C1DataGrid;
            TextBlock tb = new TextBlock();

            if (e.Row.Index - (dataGrid.TopRows.Count - 1) > 0 && e.Row.Index <= dataGrid.Rows.Count - 1)
            {
                tb.Text = (e.Row.Index + 1 - dataGrid.TopRows.Count).ToString();
                tb.VerticalAlignment = VerticalAlignment.Center;
                tb.HorizontalAlignment = HorizontalAlignment.Center;
                e.Row.HeaderPresenter.Content = tb;
            }
        }

        private void dgList_LoadedCellPresenter(object sender, DataGridCellEventArgs e)
        {

        }
        #endregion

        #region Method
        //private bool SetGridCboItem(C1.WPF.DataGrid.DataGridColumn col, string sClassId, bool bCodeDisplay = false, string sCmnCd = null)
        //{
        //    try
        //    {
        //        bool rtn = false;

        //        DataTable RQSTDT = new DataTable();
        //        RQSTDT.TableName = "RQSTDT";
        //        RQSTDT.Columns.Add("LANGID", typeof(string));
        //        RQSTDT.Columns.Add("CMCDTYPE", typeof(string));

        //        DataRow dr = RQSTDT.NewRow();
        //        dr["LANGID"] = LoginInfo.LANGID;
        //        dr["CMCDTYPE"] = sClassId;

        //        RQSTDT.Rows.Add(dr);

        //        DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_COMBO_CMN_FORM", "RQSTDT", "RSLTDT", RQSTDT);

        //        if (dtResult == null || dtResult.Rows.Count == 0)
        //        {
        //            rtn = false;
        //        }
        //        else
        //        {
        //            rtn = true;
        //            (col as C1.WPF.DataGrid.DataGridComboBoxColumn).DisplayMemberPath = "CBO_NAME";
        //            (col as C1.WPF.DataGrid.DataGridComboBoxColumn).SelectedValuePath = "CBO_CODE";
        //            (col as C1.WPF.DataGrid.DataGridComboBoxColumn).ItemsSource = DataTableConverter.Convert(dtResult);
        //        }

        //        return rtn;
        //    }
        //    catch (Exception ex)
        //    {
        //        throw ex;
        //    }
        //}
        
        private void GetList()
        {
            try
            {
                DataTable inDataTable = new DataTable();
                inDataTable.Columns.Add("LANGID", typeof(string));
                inDataTable.Columns.Add("LANEID", typeof(string));

                DataRow newRow = inDataTable.NewRow();
                newRow["LANGID"] = LoginInfo.LANGID;

                if (cboLane.SelectedIndex > 0)
                {
                    newRow["LANEID"] = Util.GetCondition(cboLane);
                }                    

                inDataTable.Rows.Add(newRow);

                ShowLoadingIndicator();
                
                //DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_SEL_EQP_OP_LOT", "INDATA", "OUTDATA", inDataTable);
                //dgList.ItemsSource = DataTableConverter.Convert(dtRslt);

                new ClientProxy().ExecuteService("DA_SEL_TB_SFC_JIG_EQPT_LOT_SET", "INDATA", "OUTDATA", inDataTable, (result, Exception) =>
                {
                    try
                    {
                        if (Exception != null)
                        {
                            Util.MessageException(Exception);
                            return;
                        }

                        DataTable dtRslt = result.Copy();

                        //dtRslt.Columns.Add("USE_FLAG", typeof(string));
                        //dtRslt.Columns.Add("OLD_LANE_ID", typeof(string));                        

                        for (int i = 0; i < dtRslt.Rows.Count; i++) //불량코드 컬럼 추가
                        {
                            string CHK = Util.NVC(dtRslt.Rows[i]["CHK"]);

                            //사용 여부
                            if (CHK.Equals("0"))
                            {
                                dtRslt.Rows[i]["CHK"] = false;
                            }
                            else
                            {
                                dtRslt.Rows[i]["CHK"] = true;
                            }

                            //dtRslt.Rows[i]["USE_FLAG"] = "Y";
                            //dtRslt.Rows[i]["OLD_LANE_ID"] = Util.NVC(dtRslt.Rows[i]["LANE_ID"]);
                        }
                        
                        dgList.ItemsSource = DataTableConverter.Convert(dtRslt);
                    }
                    catch (Exception ex)
                    {
                        HiddenLoadingIndicator();
                        Util.MessageException(ex);
                    }
                    finally
                    {
                        HiddenLoadingIndicator();
                    }

                });
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
            finally
            {
                HiddenLoadingIndicator();
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


        private void btnAddRow_Click(object sender, RoutedEventArgs e)
        {
            //this.dgList.AddRows(1);

            //C1.WPF.DataGrid.DataGridColumn col = dgList.Columns["ISS_PRHB_FLAG"];
            //C1.WPF.DataGrid.DataGridComboBoxColumn cboColumn = col as C1.WPF.DataGrid.DataGridComboBoxColumn;
            //if (cboColumn.ItemsSource == null)
            //{
            //    dgList.SetGridColumnCommonCombo("ISS_PRHB_FLAG", "USE_FLAG", isInCode: true, isInBlank: false);
            //}

            //C1.WPF.DataGrid.DataGridColumn col2 = dgList.Columns["SPCL_LOT_ISS_PRHB_FLAG"];
            //C1.WPF.DataGrid.DataGridComboBoxColumn cboColumn2 = col2 as C1.WPF.DataGrid.DataGridComboBoxColumn;
            //if (cboColumn2.ItemsSource == null)
            //{
            //    dgList.SetGridColumnCommonCombo("SPCL_LOT_ISS_PRHB_FLAG", "USE_FLAG", isInCode: true, isInBlank: false);
            //}

            //C1.WPF.DataGrid.DataGridColumn col3 = dgList.Columns["EQSGID"];
            //C1.WPF.DataGrid.DataGridTextColumn cboColumn3 = col3 as C1.WPF.DataGrid.DataGridTextColumn;
        }
                
        private void btnAdd_Click(object sender, RoutedEventArgs e)
        {
            if (dgList.ItemsSource == null || dgList.Rows.Count < 0)
                return;

            DataTable dt = ((DataView)dgList.ItemsSource).Table;

            DataRow dr = dt.NewRow();
            dr["CHK"] = true;
            dr["LANE_NAME"] = string.Empty;
            dr["LANE_ID"] = string.Empty; //Util.GetCondition(cboLane);
            dr["EQPTID"] = string.Empty;
            dr["DAY_GR_LOTID"] = string.Empty;
            dr["PROD_LOTID"] = string.Empty;
            dr["ROUTID"] = string.Empty;
            dr["SPCL_LOT_ISS_PRHB_FLAG"] = "N";
            dr["USE_FLAG"] = "Y";
            dr["DELETEYN"] = "Y";
            dt.Rows.Add(dr);

            dgList.ScrollIntoView(dt.Rows.Count - 1, 0);
        }

        private void btnRemove_Click(object sender, RoutedEventArgs e)
        {
            if (dgList.ItemsSource == null || dgList.Rows.Count < 0)
                return;

            DataTable dt = ((DataView)dgList.ItemsSource).Table;

            for (int i = (dt.Rows.Count - 1); i >= 0; i--)
                if (Convert.ToBoolean(dt.Rows[i]["CHK"]) && string.Equals(dt.Rows[i]["DELETEYN"], "Y"))
                    dt.Rows[i].Delete();
        }        

        private bool LaneSetGridCboItem(C1.WPF.DataGrid.DataGridColumn col, String[] sFilter = null,string sCmnCd = null)
        {
            try
            {
                bool rtn = false;

                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("ONLY_X", typeof(string));
                RQSTDT.Columns.Add("S70_LIST", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["ONLY_X"] = (string.IsNullOrEmpty(sCmnCd)) ? "Y" : null;
                dr["S70_LIST"] = sFilter[0]; // "J";
                RQSTDT.Rows.Add(dr);

                //DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_LANE_CBO", "RQSTDT", "RSLTDT", RQSTDT);
                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_LANE_CBO_MB", "RQSTDT", "RSLTDT", RQSTDT);

                if (dtResult == null || dtResult.Rows.Count == 0)
                {
                    rtn = false;
                }
                else
                {
                    rtn = true;
                    (col as C1.WPF.DataGrid.DataGridComboBoxColumn).DisplayMemberPath = "CBO_NAME";
                    (col as C1.WPF.DataGrid.DataGridComboBoxColumn).SelectedValuePath = "CBO_CODE";
                    //(col as C1.WPF.DataGrid.DataGridComboBoxColumn).ItemsSource = DataTableConverter.Convert(SetCodeDisplay(dtResult, bCodeDisplay));
                    (col as C1.WPF.DataGrid.DataGridComboBoxColumn).ItemsSource = DataTableConverter.Convert(dtResult);
                }

                return rtn;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private bool LaneSetCboItem(C1ComboBox cb, String[] sFilter = null, bool bCodeDisplay = true, string sCmnCd = null)
        {
            try
            {
                bool rtn = false;

                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("ONLY_X", typeof(string));
                RQSTDT.Columns.Add("S70_LIST", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["ONLY_X"] = (string.IsNullOrEmpty(sCmnCd)) ? "Y" : null;
                dr["S70_LIST"] = sFilter[0]; // "J";
                RQSTDT.Rows.Add(dr);

                //DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_LANE_CBO", "RQSTDT", "RSLTDT", RQSTDT);
                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_LANE_CBO_MB", "RQSTDT", "RSLTDT", RQSTDT);

                if (dtResult == null || dtResult.Rows.Count == 0)
                {
                    rtn = false;
                }
                else
                {
                    DataRow drItem = dtResult.NewRow();
                    ////-case ComboStatus.ALL:
                    drItem["CBO_NAME"] = " - ALL-";
                    drItem["CBO_CODE"] = "";
                    dtResult.Rows.InsertAt(drItem, 0);

                    rtn = true;
                    cb.DisplayMemberPath = "CBO_NAME";
                    cb.SelectedValuePath = "CBO_CODE";
                    //(col as C1.WPF.DataGrid.DataGridComboBoxColumn).ItemsSource = DataTableConverter.Convert(SetCodeDisplay(dtResult, bCodeDisplay));
                    cb.ItemsSource = DataTableConverter.Convert(dtResult);
                }

                return rtn;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private bool EqptSetGridCboItem(C1.WPF.DataGrid.DataGridColumn col, string sClassId, bool bCodeDisplay = true, string[] sFilter = null)
        {
            try
            {
                bool rtn = false;

                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("S70", typeof(string));
                RQSTDT.Columns.Add("S71", typeof(string));                
                RQSTDT.Columns.Add("EQP_ID_LAST", typeof(string));
                RQSTDT.Columns.Add("LANEID_LIST", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                if (sFilter != null)
                {
                    if (sFilter.Length > 1)
                        dr["S71"] = sFilter[0]; //LANE_ID

                    if (sFilter.Length > 2)
                        dr["EQP_ID_LAST"] = sFilter[1];

                    if (sFilter.Length > 4)
                        dr["LANEID_LIST"] = sFilter[1];
                }

                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_COMBO_EQP_BY_LANE", "RQSTDT", "RSLTDT", RQSTDT);

                if (dtResult == null || dtResult.Rows.Count == 0)
                {
                    rtn = false;
                }
                else
                {
                    rtn = true;
                    (col as C1.WPF.DataGrid.DataGridComboBoxColumn).DisplayMemberPath = "CBO_NAME";
                    (col as C1.WPF.DataGrid.DataGridComboBoxColumn).SelectedValuePath = "CBO_CODE";
                    //(col as C1.WPF.DataGrid.DataGridComboBoxColumn).ItemsSource = DataTableConverter.Convert(SetCodeDisplay(dtResult, bCodeDisplay));
                    (col as C1.WPF.DataGrid.DataGridComboBoxColumn).ItemsSource = DataTableConverter.Convert(dtResult);
                }

                return rtn;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private bool JGEqptGridColItem(C1.WPF.DataGrid.DataGridColumn col, string sClassId, bool bCodeDisplay = true)
        {
            try
            {
                bool rtn = false;

                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));


                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;

                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_SEL_COMBO_JIG_EQPT_BY_LANE", "RQSTDT", "RSLTDT", RQSTDT);

                if (dtResult == null || dtResult.Rows.Count == 0)
                {
                    rtn = false;
                }
                else
                {
                    rtn = true;
                    (col as C1.WPF.DataGrid.DataGridComboBoxColumn).DisplayMemberPath = "CBO_NAME";
                    (col as C1.WPF.DataGrid.DataGridComboBoxColumn).SelectedValuePath = "CBO_CODE";
                    //(col as C1.WPF.DataGrid.DataGridComboBoxColumn).ItemsSource = DataTableConverter.Convert(SetCodeDisplay(dtResult, bCodeDisplay));
                    (col as C1.WPF.DataGrid.DataGridComboBoxColumn).ItemsSource = DataTableConverter.Convert(dtResult);
                }

                return rtn;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private bool RouteGridColItem(C1.WPF.DataGrid.DataGridColumn col, String[] sFilter = null, bool bCodeDisplay = true)
        {
            try
            {
                bool rtn = false;

                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("ROUT_GR_CODE", typeof(string));


                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["ROUT_GR_CODE"] = sFilter[0];

                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_COMBO_FORM_ROUTE", "RQSTDT", "RSLTDT", RQSTDT);

                if (dtResult == null || dtResult.Rows.Count == 0)
                {
                    rtn = false;
                }
                else
                {
                    rtn = true;
                    (col as C1.WPF.DataGrid.DataGridComboBoxColumn).DisplayMemberPath = "CBO_NAME";
                    (col as C1.WPF.DataGrid.DataGridComboBoxColumn).SelectedValuePath = "CBO_CODE";
                    //(col as C1.WPF.DataGrid.DataGridComboBoxColumn).ItemsSource = DataTableConverter.Convert(SetCodeDisplay(dtResult, bCodeDisplay));
                    (col as C1.WPF.DataGrid.DataGridComboBoxColumn).ItemsSource = DataTableConverter.Convert(dtResult);
                }


                //DataTable RQSTDT = new DataTable();
                //RQSTDT.TableName = "RQSTDT";
                //RQSTDT.Columns.Add("LANGID", typeof(string));
                //RQSTDT.Columns.Add("MDLLOT_ID", typeof(string));
                //RQSTDT.Columns.Add("EQSGID", typeof(string));
                //RQSTDT.Columns.Add("MODEL_TYPE_CODE", typeof(string));
                //RQSTDT.Columns.Add("ROUT_RSLT_GR_CODE", typeof(string));
                //RQSTDT.Columns.Add("ROUTE_TYPE_DG", typeof(string));
                //RQSTDT.Columns.Add("ROUT_TYPE_CODE", typeof(string));

                //DataRow dr = RQSTDT.NewRow();
                //dr["LANGID"] = LoginInfo.LANGID;
                //dr["EQSGID"] = (!string.IsNullOrEmpty(sFilter[0])) ? sFilter[0] : null;
                //dr["MDLLOT_ID"] = (!string.IsNullOrEmpty(sFilter[1])) ? sFilter[1] : null;
                //dr["ROUT_RSLT_GR_CODE"] = (!string.IsNullOrEmpty(sFilter[2])) ? sFilter[2] : null;
                //dr["MODEL_TYPE_CODE"] = (!string.IsNullOrEmpty(sFilter[3])) ? sFilter[3] : null;
                //dr["ROUTE_TYPE_DG"] = (!string.IsNullOrEmpty(sFilter[4])) ? sFilter[4] : null;
                //if (sFilter.Length > 5) dr["ROUT_TYPE_CODE"] = (!string.IsNullOrEmpty(sFilter[5])) ? sFilter[5] : null;
                //RQSTDT.Rows.Add(dr);

                //DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_COMBO_FORM_ROUTE", "RQSTDT", "RSLTDT", RQSTDT);

                //cbo.DisplayMemberPath = "CBO_NAME";
                //cbo.SelectedValuePath = "CBO_CODE";
                //cbo.ItemsSource = AddStatus(dtResult, cs, "CBO_CODE", "CBO_NAME").Copy().AsDataView();

                //if (cbo.Items.Count > 0)
                //    cbo.SelectedIndex = 0;
                return rtn;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public static DataTable SetCodeDisplay(DataTable dt, bool bCodeDisplay)
        {
            if (bCodeDisplay)
            {
                foreach (DataRow drRslt in dt.Rows)
                {
                    drRslt["CBO_NAME"] = "[" + drRslt["CBO_CODE"].ToString() + "] " + drRslt["CBO_NAME"].ToString();
                }
            }
            return dt;
        }

        private void dgList_BeginningEdit(object sender, C1.WPF.DataGrid.DataGridBeginningEditEventArgs e)
        {
            C1.WPF.DataGrid.C1DataGrid dataGrid = sender as C1.WPF.DataGrid.C1DataGrid;

            bool bCheck = false;  
            if (Util.NVC(DataTableConverter.GetValue(dgList.Rows[e.Row.Index].DataItem, "CHK")).Equals("True"))
            {
                bCheck = true;
            }
            string sDELETEYN = Util.NVC(DataTableConverter.GetValue(e.Row.DataItem, "DELETEYN"));

            /***********************************************************************************
             title      : edit mode
             contents   :   
                bCheck = true , sDELETEYN = 'N'  기존 데이터 수정, 레인, 장비 선택 불가능
                bCheck = true , sDELETEYN = 'Y'  신규 데이터 입력, 레인, 장비 선택 가능
             ***********************************************************************************/

            if(!e.Column.Name.Equals("CHK"))
            {
                if (!bCheck)
                {
                    e.Cancel = true;
                }
            }

            if (e.Column.Name.Equals("EQPTID") || e.Column.Name.Equals("LANE_ID"))
            {
                if (sDELETEYN.Equals("N"))
                {
                    e.Cancel = true;
                }
            }
        }

        private void dgList_BeganEdit(object sender, DataGridBeganEditEventArgs e)
        {
            C1ComboBox cbo = e.EditingElement as C1ComboBox;
            bool bCodeDisplay = true;

            if (cbo != null)
            {
                if (e.Column.Name.Equals("LANE_ID"))
                {
                    
                }

                if (e.Column.Name.Equals("EQPTID"))
                {
                    string sLANE_ID = Util.NVC(DataTableConverter.GetValue(e.Row.DataItem, "LANE_ID"));


                    if (string.IsNullOrEmpty(sLANE_ID))
                    {
                        Util.MessageValidation("SFU4925", ObjectDic.Instance.GetObjectName("LANE_ID"));
                        return;
                    }

                    DataTable dt = SetEqptCode(sLANE_ID);

                    dt.AcceptChanges();

                    //cbo.ItemsSource = DataTableConverter.Convert(dt.Copy());
                    cbo.ItemsSource = DataTableConverter.Convert(SetCodeDisplay(dt.Copy(), bCodeDisplay));
                    //cbo.SelectedIndex = -1;                    

                }

                if (e.Column.Name.Equals("ROUTID"))
                {
                    string sLANE_ID = Util.NVC(DataTableConverter.GetValue(e.Row.DataItem, "LANE_ID"));
                    string sDAY_GR_LOTID = Util.NVC(DataTableConverter.GetValue(e.Row.DataItem, "DAY_GR_LOTID"));

                    if (string.IsNullOrEmpty(sLANE_ID))
                    {
                        Util.MessageValidation("SFU4925", ObjectDic.Instance.GetObjectName("LANE_ID"));
                        return;
                    }

                    DataTable dt = SetRoute(sLANE_ID, sDAY_GR_LOTID);
                    
                    dt.AcceptChanges();
                    
                    cbo.ItemsSource = DataTableConverter.Convert(dt.Copy());
                    //cbo.SelectedIndex = -1;

                }
            }
        }

        private void dgList_CommittingEdit(object sender, DataGridEndingEditEventArgs e)
        {
            try
            {
                C1DataGrid dg = sender as C1DataGrid;

                C1ComboBox cbo = e.EditingElement as C1ComboBox;



                if (e.Column.Name == "LANE_ID")
                {
                    string sLANE_ID = Util.NVC(DataTableConverter.GetValue(e.Row.DataItem, "LANE_ID"));

                    if (string.IsNullOrEmpty(sLANE_ID))
                    {
                        Util.MessageValidation("SFU4925", ObjectDic.Instance.GetObjectName("LANE_ID"));
                        return;
                    }

                    //DataTableConverter.SetValue(dgList.Rows[e.Row.Index].DataItem, "EQPTID", string.Empty); //CELL INIT
                    
                    //DataTableConverter.SetValue(dgList.Rows[e.Row.Index].DataItem, "ROUTID", string.Empty); //CELL INIT
                }               

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }


        private void dgList_CommittedEdit(object sender, DataGridCellEventArgs e)
        {
            C1DataGrid dg = sender as C1DataGrid;

            if (dg.CurrentColumn.Name == "LANE_ID")
            {
                DataTableConverter.SetValue(dgList.Rows[dg.CurrentRow.Index].DataItem, "EQPTID", string.Empty); //CELL INIT

                DataTableConverter.SetValue(dgList.Rows[dg.CurrentRow.Index].DataItem, "ROUTID", string.Empty); //CELL INIT

                dg.CurrentRow.Refresh();
            }
            
        }

        private DataTable SetEqptCode(string LANE_ID)
        {
            DataTable RSLTDT = new DataTable();
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("S70", typeof(string));
                RQSTDT.Columns.Add("S71", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                //dr["S70"] = "J"; //EQPT_GR_TYPE_CODE
                dr["S71"] = LANE_ID; //LANE_ID

                RQSTDT.Rows.Add(dr);

                RSLTDT = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_COMBO_EQP_BY_LANE", "RQSTDT", "RSLTDT", RQSTDT);

                return RSLTDT;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private DataTable SetEqptCodeByLane(string LANE_ID)
        {
            DataTable RSLTDT = new DataTable();
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("LANEID", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["LANEID"] = LANE_ID; //LANE_ID

                RQSTDT.Rows.Add(dr);

                RSLTDT = new ClientProxy().ExecuteServiceSync("DA_SEL_COMBO_JIG_EQPT_BY_LANE", "RQSTDT", "RSLTDT", RQSTDT);

                return RSLTDT;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }


        private DataTable SetRoute(string LANE_ID, string DAY_GR_LOTID)
        {
            DataTable RSLTDT = new DataTable();
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("LANE_ID", typeof(string));
                RQSTDT.Columns.Add("ROUT_GR_CODE", typeof(string));
                RQSTDT.Columns.Add("DAY_GR_LOTID", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["LANE_ID"] = LANE_ID; //LANE_ID
                dr["ROUT_GR_CODE"] = "D";
                if(!string.IsNullOrEmpty(DAY_GR_LOTID))
                {
                    dr["DAY_GR_LOTID"] = DAY_GR_LOTID;
                }                

                RQSTDT.Rows.Add(dr);

                //RSLTDT = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_COMBO_FORM_LANE_MODEL_ROUTE", "RQSTDT", "RSLTDT", RQSTDT);
                RSLTDT = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_COMBO_FORM_ROUTE_BY_LANE", "RQSTDT", "RSLTDT", RQSTDT);

                return RSLTDT;                
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }


        private bool CheckinputValue(string sName)
        {
            bool bRet = true;           

            for (int i = 0; i <= dgList.GetRowCount()-1; i++)
            {
                if (Util.NVC(DataTableConverter.GetValue(dgList.Rows[i].DataItem, "CHK")).Equals("True"))
                {
                    string sValue = Util.NVC(DataTableConverter.GetValue(dgList.Rows[i].DataItem, sName));

                    for (int j = 0; j <= dgList.GetRowCount()-1; j++)
                    {
                        if(!j.Equals(i))
                        {
                            if (DataTableConverter.GetValue(dgList.Rows[i].DataItem, sName).Equals(sValue))
                            {
                                bRet = false;
                            }
                        }
                        
                    }
                }
                    
            }

            return bRet;

        }

        private bool CheckinputValue(string sName, int nIndex)
        {
            bool bRet = true;

            string sValue = Util.NVC(DataTableConverter.GetValue(dgList.Rows[nIndex].DataItem, sName));

            for (int i = 0; i <= dgList.GetRowCount() -1; i++)
            {
                if (!i.Equals(nIndex))
                {
                    if (DataTableConverter.GetValue(dgList.Rows[i].DataItem, sName).Equals(sValue))
                    {
                        bRet = false;
                        return bRet;
                    }
                }
            }

            return bRet;

        }


        private bool CheckRoute(string ROUTID)
        {
            bool bRet = false;
            DataTable RSLTDT = new DataTable();
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("ROUTID", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["ROUTID"] = ROUTID;

                RQSTDT.Rows.Add(dr);

                RSLTDT = new ClientProxy().ExecuteServiceSync("ROUTID", "RQSTDT", "RSLTDT", RQSTDT);

                if (RSLTDT != null && RSLTDT.Rows.Count > 0)
                {
                    bRet = true;
                }
                return bRet;
            }
            catch (Exception ex)
            {
                throw ex;                
            }

            //return bRet;
        }


    }
}
 
