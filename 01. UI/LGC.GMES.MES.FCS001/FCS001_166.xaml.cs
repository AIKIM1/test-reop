/*************************************************************************************
 Created Date : 2023.10.23
      Creator : 이의철
   Decription : 불량 기준 관리
--------------------------------------------------------------------------------------
 [Change History]
  2023.10.23  이의철 : Initial Created.
  2024.01.21  손동혁 : 레인정보 동별공통코드에서 해당 레인정보를 가져옴
  2024.03.15  양강주 : 미사용 항목(생산레인 조건 combo-box, 자동 BM 변경 여부) 숨김 처리
                       BAD PIN 한계 값, 점검 누적 시간 입력 항목 숫자 정합성 기능 추가
  2024.06.03  양강주 : 연속불량 BM 화면(탭) 추가(E20240523-001117)
  2025.03.31  양윤호 : 표준편차 연속불량 BM 화면(탭) 추가(Catch-Up)
**************************************************************************************/
using C1.WPF;
using C1.WPF.DataGrid;
using C1.WPF.DataGrid.Summaries;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.CMM001.Extensions;
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
using System.Linq;

namespace LGC.GMES.MES.FCS001
{
    public partial class FCS001_166 : UserControl, IWorkArea
    {
        #region Declaration & Constructor 

        private const int CONTI_BM_MIN_LIMIT_VALUE = 2;
        private const int CONTI_BM_MAX_LIMIT_VALUE = 5;
        private const int CONTI_BM_MIN_TIME_VALUE = 1440;
        private const int CONTI_BM_MAX_TIME_VALUE = 2880;
        DataView _dvEQSG { get; set; }
        DataView _dvMODEL { get; set; }
        DataView _dvCMCD { get; set; }


        Util _Util = new Util();
        private string _sLANE_ID = string.Empty;
        private bool _manualCommit = false;
        public FCS001_166()
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
//                InitCombo();

                InitSpread();
                InitCombo();
                GetAccuBMList();
                GetContiBMList();
                GetSDContiBMList();
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }


        private void InitCombo()
        {

            //CommonCombo_Form _combo = new CommonCombo_Form();
            CommonCombo_Form ComCombo = new CommonCombo_Form();
            //string[] sFilterLane = { "J" };
            //_combo.SetCombo(cboAccuBMLane, CommonCombo_Form.ComboStatus.ALL, sCase: "LANE", sFilter: sFilterLane);

            //grid combo setting
            //IsFormationUse();
            //LaneSetGridCboItem(dgAccuBMList.Columns["LANE_ID"], "");
            //JGEqptGridColItem(dgAccuBMList.Columns["EQPTID"], "", true);
            //RouteGridColItem(dgAccuBMList.Columns["ROUTID"], "", true);


            #region USE_FLAG
            Get_MMD_SEL_CMCD_TYPE_MULT("'IUSE'", "Y", (dt, ex) =>
            {
                if (ex != null)
                {
                    ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), null, "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
                    return;
                }

                _dvCMCD = dt.DefaultView;

                CommonCombo.SetDtCommonCombo(_dvCMCD.ToTable(), cboUseFlag, CommonCombo.ComboStatus.ALL, "Y");
                CommonCombo.SetDtCommonCombo(_dvCMCD.ToTable(), dgSDContiBMList.Columns["USE_FLAG"], CommonCombo.ComboStatus.NONE);
            });
            #endregion USE_FLAG

            #region LINE_ID
            Get_FORM_EQSG_CBO((dt, ex) =>
            {
                if (ex != null)
                {
                    ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), null, "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
                    return;
                }

                _dvEQSG = dt.DefaultView;

                CommonCombo.SetDtCommonCombo(_dvEQSG.ToTable(), dgSDContiBMList.Columns["LINE_ID"], CommonCombo.ComboStatus.SELECT);
            });
            #endregion LINE_ID

            #region MDLLOT_ID
            Get_FORM_MDLLOT_CBO(null, (dt, ex) =>
            {
                if (ex != null)
                {
                    ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), null, "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
                    return;
                }

                _dvMODEL = dt.DefaultView;

                CommonCombo.SetDtCommonCombo(_dvMODEL.ToTable(), dgSDContiBMList.Columns["MDLLOT_ID"], CommonCombo.ComboStatus.SELECT);
            });
            #endregion MDLLOT_ID


        }


        private void IsFormationUse()
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("AREAID", typeof(string));
                RQSTDT.Columns.Add("COM_TYPE_CODE", typeof(string));
                RQSTDT.Columns.Add("COM_CODE", typeof(string));
                RQSTDT.Columns.Add("USE_FLAG", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["AREAID"] = LoginInfo.CFG_AREA_ID;
                dr["COM_TYPE_CODE"] = "FORM_BAD_PIN_LANE_INFO";

                dr["USE_FLAG"] = "Y";
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_SEL_TB_MMD_AREA_COM_CODE_FORM", "RQSTDT", "RSLTDT", RQSTDT);

                if (dtResult.Rows.Count > 0)
                {

                    (dgAccuBMList.Columns["LANE_ID"] as C1.WPF.DataGrid.DataGridComboBoxColumn).DisplayMemberPath = "CBO_NAME";
                    (dgAccuBMList.Columns["LANE_ID"] as C1.WPF.DataGrid.DataGridComboBoxColumn).SelectedValuePath = "CBO_CODE";
                    //(col as C1.WPF.DataGrid.DataGridComboBoxColumn).ItemsSource = DataTableConverter.Convert(SetCodeDisplay(dtResult, bCodeDisplay));
                    (dgAccuBMList.Columns["LANE_ID"] as C1.WPF.DataGrid.DataGridComboBoxColumn).ItemsSource = DataTableConverter.Convert(dtResult);
                }

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }

        }

        private void InitSpread()
        {
            
        }

        #endregion

        #region [ ACCUMULATE BM ]


        #region Event
        private void btnAccuBMSearch_Click(object sender, RoutedEventArgs e)
        {
            GetAccuBMList();
        }

        private void btnAccuBMSave_Click(object sender, RoutedEventArgs e)
        {

            if (!dgAccuBMList.IsCheckedRow("CHK"))
            {
                Util.Alert("SFU1645"); //선택된 작업대상이 없습니다.
                return;
            }

            #region [ 중복 확인 ]
            for (int i = 0; i < dgAccuBMList.GetRowCount(); i++)
            {
                if (Util.NVC(DataTableConverter.GetValue(dgAccuBMList.Rows[i].DataItem, "CHK")).Equals("True"))
                {
/*
                    if (!CheckAccuBMinputValue("LANE_ID", i))
                    {
                        string sValue = Util.NVC(DataTableConverter.GetValue(dgAccuBMList.Rows[i].DataItem, "LANE_ID"));
                        Util.MessageValidation("SFU3471", sValue);//[%1]은 이미 등록되었습니다.  
                        return;
                    }

                    //AUTO_CHG_FLAG
                    string AUTO_CHG_FLAG = Util.NVC(DataTableConverter.GetValue(dgAccuBMList.Rows[i].DataItem, "AUTO_CHG_FLAG"));
                    if (string.IsNullOrEmpty(AUTO_CHG_FLAG))
                    {
                        string sValue = ObjectDic.Instance.GetObjectName("AUTO_CHG_FLAG");                        
                        // SFU4925	%1을 선택하세요.
                        Util.MessageValidation("SFU4925", sValue);
                        return;

                    }
*/

                    //USE_FLAG
                    string USE_FLAG = Util.NVC(DataTableConverter.GetValue(dgAccuBMList.Rows[i].DataItem, "USE_FLAG"));
                    if (string.IsNullOrEmpty(USE_FLAG))
                    {
                        string sValue = ObjectDic.Instance.GetObjectName("USE_FLAG");
                        // SFU4925	%1을 선택하세요.
                        Util.MessageValidation("SFU4925", sValue);
                        return;

                    }

                }
            }
            #endregion [ 중복 확인 ]



            #region [ BM 한계 값, 누적 시간 입력 여부 확인 ]
            {
                float convNumber;

                List<String> inputList = new List<String> { "BAD_PN_LIMIT_VALUE", "BAD_PN_MGA_LIMIT_VALUE", "CHK_ACCU_TIME" };
                for (int i = 0; i < dgAccuBMList.GetRowCount(); i++)
                {
                    if (Util.NVC(DataTableConverter.GetValue(dgAccuBMList.Rows[i].DataItem, "CHK")).Equals("True"))
                    {
                        string sObjName, sValue;

                        foreach (string sTarget in inputList)
                        {
                            //BAD_PN_LIMIT_VALUE
                            sValue = Util.NVC(DataTableConverter.GetValue(dgAccuBMList.Rows[i].DataItem, sTarget));
                            sObjName = ObjectDic.Instance.GetObjectName(sTarget);
                            if (string.IsNullOrEmpty(sValue))
                            {
                                // SFU1299	%1이 입력되지 않았습니다.
                                Util.MessageValidation("SFU1299", sObjName);
                                return;
                            }

                            if(float.TryParse(sValue, out convNumber) == false || convNumber < 0)
                            {
                                // SFU4209	양수를 입력하세요.
                                Util.MessageValidation("SFU4209", sObjName);
                                return;
                            }
                        }
                    }
                }
            }
            #endregion [ BM 한계 값, 누적 시간 입력 여부 확인 ]



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
                        for (int i = 0; i < dgAccuBMList.GetRowCount(); i++)
                        {
                            if (Util.NVC(DataTableConverter.GetValue(dgAccuBMList.Rows[i].DataItem, "CHK")).Equals("True"))
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
                        dtRqst.Columns.Add("LANE_ID", typeof(string));
                        dtRqst.Columns.Add("BAD_PN_LIMIT_VALUE", typeof(string));
                        dtRqst.Columns.Add("BAD_PN_MGA_LIMIT_VALUE", typeof(string));
                        dtRqst.Columns.Add("AUTO_CHG_FLAG", typeof(string));
                        dtRqst.Columns.Add("CHK_ACCU_TIME", typeof(string));
                        dtRqst.Columns.Add("USE_FLAG", typeof(string));
                        dtRqst.Columns.Add("USERID", typeof(string));

                        for (int i = 0; i < dgAccuBMList.GetRowCount(); i++)
                        {
                            if (Util.NVC(DataTableConverter.GetValue(dgAccuBMList.Rows[i].DataItem, "CHK")).Equals("True"))
                            {
                                DataRow dr = dtRqst.NewRow();
                                dr["LANE_ID"] = Util.NVC(DataTableConverter.GetValue(dgAccuBMList.Rows[i].DataItem, "LANE_ID"));
                                dr["BAD_PN_LIMIT_VALUE"] = Util.NVC(DataTableConverter.GetValue(dgAccuBMList.Rows[i].DataItem, "BAD_PN_LIMIT_VALUE"));
                                dr["BAD_PN_MGA_LIMIT_VALUE"] = Util.NVC(DataTableConverter.GetValue(dgAccuBMList.Rows[i].DataItem, "BAD_PN_MGA_LIMIT_VALUE"));
//                                dr["AUTO_CHG_FLAG"] = Util.NVC(DataTableConverter.GetValue(dgAccuBMList.Rows[i].DataItem, "AUTO_CHG_FLAG"));
                                dr["AUTO_CHG_FLAG"] = "Y";
                                dr["CHK_ACCU_TIME"] = Util.NVC(DataTableConverter.GetValue(dgAccuBMList.Rows[i].DataItem, "CHK_ACCU_TIME"));
                                dr["USE_FLAG"] = Util.NVC(DataTableConverter.GetValue(dgAccuBMList.Rows[i].DataItem, "USE_FLAG"));
                                dr["USERID"] = LoginInfo.USERID;
                                dtRqst.Rows.Add(dr);
                            }
                        }

                        ShowLoadingIndicator();
                        
                        new ClientProxy().ExecuteService("BR_SET_TB_SFC_LANE_BAD_PN_LIMIT", "INDATA", null, dtRqst, (bizResult, bizException) =>
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
                                        GetAccuBMList();
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
        

        private void dgAccuBMList_LoadedRowHeaderPresenter(object sender, C1.WPF.DataGrid.DataGridRowEventArgs e)
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

        private void dgAccuBMList_LoadedCellPresenter(object sender, DataGridCellEventArgs e)
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
        
        private void GetAccuBMList()
        {
            try
            {
                DataTable inDataTable = new DataTable();
                inDataTable.Columns.Add("LANGID", typeof(string));
                inDataTable.Columns.Add("LANE_ID", typeof(string));
                inDataTable.Columns.Add("FOR_LANE_USE_YN", typeof(string));


                DataRow newRow = inDataTable.NewRow();
                newRow["LANGID"] = LoginInfo.LANGID;
                newRow["FOR_LANE_USE_YN"] = "Y";

                if (cboAccuBMLane.SelectedIndex > 0)
                {
                    newRow["LANE_ID"] = Util.GetCondition(cboAccuBMLane);
                }                    

                inDataTable.Rows.Add(newRow);

                ShowLoadingIndicator();


                new ClientProxy().ExecuteService("DA_SEL_TB_SFC_LANE_BAD_PN_LIMIT", "INDATA", "OUTDATA", inDataTable, (result, Exception) =>
                {
                    try
                    {
                        if (Exception != null)
                        {
                            Util.MessageException(Exception);
                            return;
                        }

                        DataTable dtRslt = result.Copy();                

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

                        }

                        dgAccuBMList.ItemsSource = DataTableConverter.Convert(dtRslt);
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
            

        #endregion


        private void btnAccuBMAddRow_Click(object sender, RoutedEventArgs e)
        {
            //this.dgAccuBMList.AddRows(1);

            //C1.WPF.DataGrid.DataGridColumn col = dgAccuBMList.Columns["ISS_PRHB_FLAG"];
            //C1.WPF.DataGrid.DataGridComboBoxColumn cboColumn = col as C1.WPF.DataGrid.DataGridComboBoxColumn;
            //if (cboColumn.ItemsSource == null)
            //{
            //    dgAccuBMList.SetGridColumnCommonCombo("ISS_PRHB_FLAG", "USE_FLAG", isInCode: true, isInBlank: false);
            //}

            //C1.WPF.DataGrid.DataGridColumn col2 = dgAccuBMList.Columns["SPCL_LOT_ISS_PRHB_FLAG"];
            //C1.WPF.DataGrid.DataGridComboBoxColumn cboColumn2 = col2 as C1.WPF.DataGrid.DataGridComboBoxColumn;
            //if (cboColumn2.ItemsSource == null)
            //{
            //    dgAccuBMList.SetGridColumnCommonCombo("SPCL_LOT_ISS_PRHB_FLAG", "USE_FLAG", isInCode: true, isInBlank: false);
            //}

            //C1.WPF.DataGrid.DataGridColumn col3 = dgAccuBMList.Columns["EQSGID"];
            //C1.WPF.DataGrid.DataGridTextColumn cboColumn3 = col3 as C1.WPF.DataGrid.DataGridTextColumn;
        }
                
        private void btnAccuBMAdd_Click(object sender, RoutedEventArgs e)
        {
            if (dgAccuBMList.ItemsSource == null || dgAccuBMList.Rows.Count < 0)
                return;

            DataTable dt = ((DataView)dgAccuBMList.ItemsSource).Table;

            DataRow dr = dt.NewRow();
            dr["CHK"] = true;
            dr["LANE_NAME"] = string.Empty;
            dr["LANE_ID"] = string.Empty;
            //dr["BAD_PN_LIMIT_VALUE"] = "";
            //dr["BAD_PN_MGA_LIMIT_VALUE"] = "";
            //dr["CHK_ACCU_TIME"] = "";
// 생산PI.MES팀 최희곤 책임 요청으로 자동 BM 변경 여부 항목 숨김처리 하기 위해 기본 값을 N -> Y 변경(2024-03-14, 양강주)
// 사유 : 자동 BM 변경 여부 항목과 사용 여부 항목이 동일한 의미로 사용되고 있음.
            dr["AUTO_CHG_FLAG"] = "Y";
            dr["USE_FLAG"] = "Y";
            dr["DELETEYN"] = "Y";
            dt.Rows.Add(dr);

            dgAccuBMList.ScrollIntoView(dt.Rows.Count - 1, 0);
        }

        private void btnAccuBMRemove_Click(object sender, RoutedEventArgs e)
        {
            if (dgAccuBMList.ItemsSource == null || dgAccuBMList.Rows.Count < 0)
                return;

            DataTable dt = ((DataView)dgAccuBMList.ItemsSource).Table;

            for (int i = (dt.Rows.Count - 1); i >= 0; i--)
                if (Convert.ToBoolean(dt.Rows[i]["CHK"]) && string.Equals(dt.Rows[i]["DELETEYN"], "Y"))
                    dt.Rows[i].Delete();
        }        

        private void dgAccuBMList_BeginningEdit(object sender, C1.WPF.DataGrid.DataGridBeginningEditEventArgs e)
        {
            C1.WPF.DataGrid.C1DataGrid dataGrid = sender as C1.WPF.DataGrid.C1DataGrid;

            bool bCheck = false;
            if (Util.NVC(DataTableConverter.GetValue(dgAccuBMList.Rows[e.Row.Index].DataItem, "CHK")).Equals("True"))
            {
                bCheck = true;
            }
            string sDELETEYN = Util.NVC(DataTableConverter.GetValue(e.Row.DataItem, "DELETEYN"));

            ///***********************************************************************************
            // title      : edit mode
            // contents   :   
            //    bCheck = true , sDELETEYN = 'N'  기존 데이터 수정, 레인  선택 불가능
            //    bCheck = true , sDELETEYN = 'Y'  신규 데이터 입력, 레인  선택 가능
            // ***********************************************************************************/

            if (!e.Column.Name.Equals("CHK"))
            {
                if (!bCheck)
                {
                    e.Cancel = true;
                }
            }

            if (e.Column.Name.Equals("LANE_ID"))
            {
                if (sDELETEYN.Equals("N"))
                {
                    e.Cancel = true;
                }
            }
        }

        private void dgAccuBMList_BeganEdit(object sender, DataGridBeganEditEventArgs e)
        {
            C1ComboBox cbo = e.EditingElement as C1ComboBox;
            bool bCodeDisplay = true;

            if (cbo != null)
            {
                if (e.Column.Name.Equals("LANE_ID"))
                {
                    
                }

                //if (e.Column.Name.Equals("EQPTID"))
                //{
                //    string sLANE_ID = Util.NVC(DataTableConverter.GetValue(e.Row.DataItem, "LANE_ID"));


                //    if (string.IsNullOrEmpty(sLANE_ID))
                //    {
                //        Util.MessageValidation("SFU4925", ObjectDic.Instance.GetObjectName("LANE_ID"));
                //        return;
                //    }

                //    DataTable dt = SetEqptCode(sLANE_ID);

                //    dt.AcceptChanges();

                //    //cbo.ItemsSource = DataTableConverter.Convert(dt.Copy());
                //    cbo.ItemsSource = DataTableConverter.Convert(SetCodeDisplay(dt.Copy(), bCodeDisplay));
                //    //cbo.SelectedIndex = -1;                    

                //}

                //if (e.Column.Name.Equals("ROUTID"))
                //{
                //    string sLANE_ID = Util.NVC(DataTableConverter.GetValue(e.Row.DataItem, "LANE_ID"));


                //    if (string.IsNullOrEmpty(sLANE_ID))
                //    {
                //        Util.MessageValidation("SFU4925", ObjectDic.Instance.GetObjectName("LANE_ID"));
                //        return;
                //    }

                //    DataTable dt = SetRoute(sLANE_ID);
                    
                //    dt.AcceptChanges();
                    
                //    cbo.ItemsSource = DataTableConverter.Convert(dt.Copy());
                //    //cbo.SelectedIndex = -1;

                //}
            }
        }

        private void dgAccuBMList_CommittingEdit(object sender, DataGridEndingEditEventArgs e)
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

                    //DataTableConverter.SetValue(dgAccuBMList.Rows[e.Row.Index].DataItem, "EQPTID", string.Empty); //CELL INIT
                    
                    //DataTableConverter.SetValue(dgAccuBMList.Rows[e.Row.Index].DataItem, "ROUTID", string.Empty); //CELL INIT
                }               

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }


        private void dgAccuBMList_CommittedEdit(object sender, DataGridCellEventArgs e)
        {
            C1DataGrid dg = sender as C1DataGrid;

            if (dg.CurrentColumn.Name == "LANE_ID")
            {
                //DataTableConverter.SetValue(dgAccuBMList.Rows[dg.CurrentRow.Index].DataItem, "EQPTID", string.Empty); //CELL INIT

                //DataTableConverter.SetValue(dgAccuBMList.Rows[dg.CurrentRow.Index].DataItem, "ROUTID", string.Empty); //CELL INIT

                //dg.CurrentRow.Refresh();
            }
            
        }


        private bool CheckAccuBMinputValue(string sName)
        {
            bool bRet = true;           

            for (int i = 0; i <= dgAccuBMList.GetRowCount()-1; i++)
            {
                if (Util.NVC(DataTableConverter.GetValue(dgAccuBMList.Rows[i].DataItem, "CHK")).Equals("True"))
                {
                    string sValue = Util.NVC(DataTableConverter.GetValue(dgAccuBMList.Rows[i].DataItem, sName));

                    for (int j = 0; j <= dgAccuBMList.GetRowCount()-1; j++)
                    {
                        if(!j.Equals(i))
                        {
                            if (DataTableConverter.GetValue(dgAccuBMList.Rows[i].DataItem, sName).Equals(sValue))
                            {
                                bRet = false;
                            }
                        }
                        
                    }
                }
                    
            }

            return bRet;

        }

        private bool CheckAccuBMinputValue(string sName, int nIndex)
        {
            bool bRet = true;

            string sValue = Util.NVC(DataTableConverter.GetValue(dgAccuBMList.Rows[nIndex].DataItem, sName));

            for (int i = 0; i <= dgAccuBMList.GetRowCount() -1; i++)
            {
                if (!i.Equals(nIndex))
                {
                    if (DataTableConverter.GetValue(dgAccuBMList.Rows[i].DataItem, sName).Equals(sValue))
                    {
                        bRet = false;
                        return bRet;
                    }
                }
            }

            return bRet;

        }

        #endregion [ ACCUMULATE BM ]



        #region [ CONTINUOUS BM ]


        #region Event
        private void btnContiBMSearch_Click(object sender, RoutedEventArgs e)
        {
            GetContiBMList();
        }

        private void btnContiBMSave_Click(object sender, RoutedEventArgs e)
        {

            if (!dgContiBMList.IsCheckedRow("CHK"))
            {
                Util.Alert("SFU1645"); //선택된 작업대상이 없습니다.
                return;
            }

            #region [ 중복 확인 ]
/*
            for (int i = 0; i < dgContiBMList.GetRowCount(); i++)
            {
                if (Util.NVC(DataTableConverter.GetValue(dgContiBMList.Rows[i].DataItem, "CHK")).Equals("True"))
                {
                    if (!CheckContiBMinputValue("LANE_ID", i))
                    {
                        string sValue = Util.NVC(DataTableConverter.GetValue(dgContiBMList.Rows[i].DataItem, "LANE_ID"));
                        Util.MessageValidation("SFU3471", sValue);//[%1]은 이미 등록되었습니다.  
                        return;
                    }

                    //AUTO_CHG_FLAG
                    string AUTO_CHG_FLAG = Util.NVC(DataTableConverter.GetValue(dgContiBMList.Rows[i].DataItem, "AUTO_CHG_FLAG"));
                    if (string.IsNullOrEmpty(AUTO_CHG_FLAG))
                    {
                        string sValue = ObjectDic.Instance.GetObjectName("AUTO_CHG_FLAG");
                        // SFU4925	%1을 선택하세요.
                        Util.MessageValidation("SFU4925", sValue);
                        return;

                    }

                    //USE_FLAG
                    string USE_FLAG = Util.NVC(DataTableConverter.GetValue(dgContiBMList.Rows[i].DataItem, "USE_FLAG"));
                    if (string.IsNullOrEmpty(USE_FLAG))
                    {
                        string sValue = ObjectDic.Instance.GetObjectName("USE_FLAG");
                        // SFU4925	%1을 선택하세요.
                        Util.MessageValidation("SFU4925", sValue);
                        return;

                    }

                }
            }
*/
            #endregion [ 중복 확인 ]



            #region [ BM 한계 값, 누적 시간 입력 여부 확인 ]
            {
                string sValue;
                C1.WPF.DataGrid.DataGridRow row;

                for (int i = 0; i < dgContiBMList.GetRowCount(); i++)
                {
                    if (Util.NVC(DataTableConverter.GetValue(dgContiBMList.Rows[i].DataItem, "CHK")).Equals("True"))
                    {
                        row = dgContiBMList.Rows[i];
                        sValue = Util.NVC(DataTableConverter.GetValue(row.DataItem, "JGF_LANE_USE_YN"));
                        if (sValue.Equals("Y") && validInputData("JIG_FORMATION_LIMIT_VALUE", row, CONTI_BM_MIN_LIMIT_VALUE, CONTI_BM_MAX_LIMIT_VALUE) == false)
                        {
                            return;
                        }
/*
                        else if (sValue.Equals("N"))
                        {
                            DataTableConverter.SetValue(dgContiBMList.Rows[i].DataItem, "JIG_FORMATION_LIMIT_VALUE", "0");
                        }
*/

                        sValue = Util.NVC(DataTableConverter.GetValue(row.DataItem, "FOR_LANE_USE_YN"));
                        if (sValue.Equals("Y") 
                            && ( validInputData("FORMEQPT_LIMIT_VALUE", row, CONTI_BM_MIN_LIMIT_VALUE, CONTI_BM_MAX_LIMIT_VALUE) == false 
                                || validInputData("MGFORM_LIMIT_VALUE", row, CONTI_BM_MIN_LIMIT_VALUE, CONTI_BM_MAX_LIMIT_VALUE) == false))
                        {
                            return;
                        }
/*
                        else if (sValue.Equals("N"))
                        {
                            DataTableConverter.SetValue(dgContiBMList.Rows[i].DataItem, "FORMEQPT_LIMIT_VALUE", "0");
                            DataTableConverter.SetValue(dgContiBMList.Rows[i].DataItem, "MGFORM_LIMIT_VALUE", "0");
                        }
*/

                        if (validInputData("CHK_ACCU_TIME", row, CONTI_BM_MIN_TIME_VALUE, CONTI_BM_MAX_TIME_VALUE) == false)
                        {
                            return;
                        }
                    }
                }
            }
            #endregion [ BM 한계 값, 누적 시간 입력 여부 확인 ]



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
                        for (int i = 0; i < dgContiBMList.GetRowCount(); i++)
                        {
                            if (Util.NVC(DataTableConverter.GetValue(dgContiBMList.Rows[i].DataItem, "CHK")).Equals("True"))
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
                        dtRqst.Columns.Add("LANE_ID", typeof(string));
                        dtRqst.Columns.Add("JF_LIMIT_VALUE", typeof(string));
                        dtRqst.Columns.Add("FORMEQPT_LIMIT_VALUE", typeof(string));
                        dtRqst.Columns.Add("MGFORM_LIMIT_VALUE", typeof(string));
                        dtRqst.Columns.Add("CHK_ACCU_TIME", typeof(string));
                        dtRqst.Columns.Add("USE_FLAG", typeof(string));
                        dtRqst.Columns.Add("USERID", typeof(string));

                        for (int i = 0; i < dgContiBMList.GetRowCount(); i++)
                        {
                            if (Util.NVC(DataTableConverter.GetValue(dgContiBMList.Rows[i].DataItem, "CHK")).Equals("True"))
                            {
                                DataRow dr = dtRqst.NewRow();
                                dr["LANE_ID"] = Util.NVC(DataTableConverter.GetValue(dgContiBMList.Rows[i].DataItem, "LANE_ID"));
                                dr["JF_LIMIT_VALUE"] = Util.NVC(DataTableConverter.GetValue(dgContiBMList.Rows[i].DataItem, "JIG_FORMATION_LIMIT_VALUE"));
                                dr["FORMEQPT_LIMIT_VALUE"] = Util.NVC(DataTableConverter.GetValue(dgContiBMList.Rows[i].DataItem, "FORMEQPT_LIMIT_VALUE"));
                                dr["MGFORM_LIMIT_VALUE"] = Util.NVC(DataTableConverter.GetValue(dgContiBMList.Rows[i].DataItem, "MGFORM_LIMIT_VALUE"));
                                dr["CHK_ACCU_TIME"] = Util.NVC(DataTableConverter.GetValue(dgContiBMList.Rows[i].DataItem, "CHK_ACCU_TIME"));
                                dr["USE_FLAG"] = Util.NVC(DataTableConverter.GetValue(dgContiBMList.Rows[i].DataItem, "USE_FLAG"));
                                dr["USERID"] = LoginInfo.USERID;
                                dtRqst.Rows.Add(dr);
                            }
                        }

                        ShowLoadingIndicator();

                        new ClientProxy().ExecuteService("BR_SET_TB_SFC_LANE_CONTI_BAD_PN_LIMIT", "INDATA", null, dtRqst, (bizResult, bizException) =>
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
                                        GetContiBMList();
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


        private void dgContiBMList_LoadedRowHeaderPresenter(object sender, C1.WPF.DataGrid.DataGridRowEventArgs e)
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

        private void GetContiBMList()
        {
            try
            {
                DataTable inDataTable = new DataTable();
                inDataTable.Columns.Add("AREAID", typeof(string));
                inDataTable.Columns.Add("LANGID", typeof(string));
                inDataTable.Columns.Add("LANE_ID", typeof(string));

                DataRow newRow = inDataTable.NewRow();
                newRow["LANGID"] = LoginInfo.LANGID;
                newRow["AREAID"] = LoginInfo.CFG_AREA_ID;

                if (cboContiBMLane.SelectedIndex > 0)
                {
                    newRow["LANE_ID"] = Util.GetCondition(cboContiBMLane);
                }

                inDataTable.Rows.Add(newRow);

                ShowLoadingIndicator();


                new ClientProxy().ExecuteService("DA_SEL_TB_SFC_LANE_CONTI_BAD_PN_LIMIT", "INDATA", "OUTDATA", inDataTable, (result, Exception) =>
                {
                    try
                    {
                        if (Exception != null)
                        {
                            Util.MessageException(Exception);
                            return;
                        }

                        DataTable dtRslt = result.Copy();

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

                        }

                        dgContiBMList.ItemsSource = DataTableConverter.Convert(dtRslt);
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

        private void dgContiBMList_ExecuteCustomBinding(object sender, CMM001.Controls.UcBaseDataGrid.ExecuteDataEventArgs e)
        {
//            _dtDATA = e.ResultData as DataTable;
        }

        private void dgContiBMList_ExecuteDataCompleted(object sender, CMM001.Controls.UcBaseDataGrid.ExecuteDataEventArgs e)
        {
            try
            {
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
            finally
            {
            }
        }


        #endregion


        private void btnContiBMAddRow_Click(object sender, RoutedEventArgs e)
        {
            //this.dgContiBMList.AddRows(1);

            //C1.WPF.DataGrid.DataGridColumn col = dgContiBMList.Columns["ISS_PRHB_FLAG"];
            //C1.WPF.DataGrid.DataGridComboBoxColumn cboColumn = col as C1.WPF.DataGrid.DataGridComboBoxColumn;
            //if (cboColumn.ItemsSource == null)
            //{
            //    dgContiBMList.SetGridColumnCommonCombo("ISS_PRHB_FLAG", "USE_FLAG", isInCode: true, isInBlank: false);
            //}

            //C1.WPF.DataGrid.DataGridColumn col2 = dgContiBMList.Columns["SPCL_LOT_ISS_PRHB_FLAG"];
            //C1.WPF.DataGrid.DataGridComboBoxColumn cboColumn2 = col2 as C1.WPF.DataGrid.DataGridComboBoxColumn;
            //if (cboColumn2.ItemsSource == null)
            //{
            //    dgContiBMList.SetGridColumnCommonCombo("SPCL_LOT_ISS_PRHB_FLAG", "USE_FLAG", isInCode: true, isInBlank: false);
            //}

            //C1.WPF.DataGrid.DataGridColumn col3 = dgContiBMList.Columns["EQSGID"];
            //C1.WPF.DataGrid.DataGridTextColumn cboColumn3 = col3 as C1.WPF.DataGrid.DataGridTextColumn;
        }

        private void btnContiBMAdd_Click(object sender, RoutedEventArgs e)
        {
            if (dgContiBMList.ItemsSource == null || dgContiBMList.Rows.Count < 0)
                return;

            DataTable dt = ((DataView)dgContiBMList.ItemsSource).Table;

            DataRow dr = dt.NewRow();
            dr["CHK"] = true;
            dr["LANE_NAME"] = string.Empty;
            dr["LANE_ID"] = string.Empty;
            //dr["BAD_PN_LIMIT_VALUE"] = "";
            //dr["BAD_PN_MGA_LIMIT_VALUE"] = "";
            //dr["CHK_ACCU_TIME"] = "";
            // 생산PI.MES팀 최희곤 책임 요청으로 자동 BM 변경 여부 항목 숨김처리 하기 위해 기본 값을 N -> Y 변경(2024-03-14, 양강주)
            // 사유 : 자동 BM 변경 여부 항목과 사용 여부 항목이 동일한 의미로 사용되고 있음.
            dr["AUTO_CHG_FLAG"] = "Y";
            dr["USE_FLAG"] = "Y";
            dr["DELETEYN"] = "Y";
            dt.Rows.Add(dr);

            dgContiBMList.ScrollIntoView(dt.Rows.Count - 1, 0);
        }

        private void btnContiBMRemove_Click(object sender, RoutedEventArgs e)
        {
            if (dgContiBMList.ItemsSource == null || dgContiBMList.Rows.Count < 0)
                return;

            DataTable dt = ((DataView)dgContiBMList.ItemsSource).Table;

            for (int i = (dt.Rows.Count - 1); i >= 0; i--)
                if (Convert.ToBoolean(dt.Rows[i]["CHK"]) && string.Equals(dt.Rows[i]["DELETEYN"], "Y"))
                    dt.Rows[i].Delete();
        }
        
        private void dgContiBMList_BeginningEdit(object sender, C1.WPF.DataGrid.DataGridBeginningEditEventArgs e)
        {
            C1.WPF.DataGrid.C1DataGrid dataGrid = sender as C1.WPF.DataGrid.C1DataGrid;
            C1.WPF.DataGrid.DataGridRow row = dgContiBMList.Rows[e.Row.Index];
            string sValue;
            bool bCheck = false;

            if (Util.NVC(DataTableConverter.GetValue(row.DataItem, "CHK")).Equals("True"))
            {
                bCheck = true;
            }


            string sDELETEYN = Util.NVC(DataTableConverter.GetValue(e.Row.DataItem, "DELETEYN"));

            ///***********************************************************************************
            // title      : edit mode
            // contents   :   
            //    bCheck = true , sDELETEYN = 'N'  기존 데이터 수정, 레인  선택 불가능
            //    bCheck = true , sDELETEYN = 'Y'  신규 데이터 입력, 레인  선택 가능
            // ***********************************************************************************/

/*
            if(bCheck)
            {
                try
                {
                    dgContiBMList.Columns["JF_LIMIT_VALUE"].IsReadOnly = (Util.NVC(DataTableConverter.GetValue(row.DataItem, "JGF_LANE_USE_YN")).Equals("N"));

                    dgContiBMList.Columns["BAD_PN_LIMIT_VALUE"].IsReadOnly = (Util.NVC(DataTableConverter.GetValue(row.DataItem, "FOR_LANE_USE_YN")).Equals("N"));
                    dgContiBMList.Columns["BAD_PN_MGA_LIMIT_VALUE"].IsReadOnly = (Util.NVC(DataTableConverter.GetValue(row.DataItem, "FOR_LANE_USE_YN")).Equals("N"));
                }
                catch (Exception e)
                {
                    Util.MessageException(ex);
                }
            }
*/

            if (!e.Column.Name.Equals("CHK"))
            {
                if (bCheck)
                {
                    if (e.Column.Name.Equals("JIG_FORMATION_LIMIT_VALUE") && Util.NVC(DataTableConverter.GetValue(row.DataItem, "JGF_LANE_USE_YN")).Equals("N"))
                    {
                        e.Cancel = true;
//                        dgContiBMList.CurrentCell.Presenter.Background = new BrushConverter().ConvertFromString("Gray") as SolidColorBrush;
                    }
                    else if (( e.Column.Name.Equals("FORMEQPT_LIMIT_VALUE") || e.Column.Name.Equals("MGFORM_LIMIT_VALUE"))
                        && Util.NVC(DataTableConverter.GetValue(row.DataItem, "FOR_LANE_USE_YN")).Equals("N"))
                    {
//                        dgContiBMList.CurrentCell.Presenter.Background = new BrushConverter().ConvertFromString("Gray") as SolidColorBrush;
                        e.Cancel = true;
                    }
                    else
                    {
//                        dgContiBMList.CurrentCell.Presenter.Background = new BrushConverter().ConvertFromString("White") as SolidColorBrush;
                    }
                }
                else
                {
                    e.Cancel = true;
                }
            }


            if (e.Column.Name.Equals("LANE_ID"))
            {
                if (sDELETEYN.Equals("N"))
                {
                    e.Cancel = true;
                }
            }
        }

        private void dgContiBMList_BeganEdit(object sender, DataGridBeganEditEventArgs e)
        {
            C1ComboBox cbo = e.EditingElement as C1ComboBox;
            bool bCodeDisplay = true;

            if (cbo != null)
            {
                if (e.Column.Name.Equals("LANE_ID"))
                {

                }

                //if (e.Column.Name.Equals("EQPTID"))
                //{
                //    string sLANE_ID = Util.NVC(DataTableConverter.GetValue(e.Row.DataItem, "LANE_ID"));


                //    if (string.IsNullOrEmpty(sLANE_ID))
                //    {
                //        Util.MessageValidation("SFU4925", ObjectDic.Instance.GetObjectName("LANE_ID"));
                //        return;
                //    }

                //    DataTable dt = SetEqptCode(sLANE_ID);

                //    dt.AcceptChanges();

                //    //cbo.ItemsSource = DataTableConverter.Convert(dt.Copy());
                //    cbo.ItemsSource = DataTableConverter.Convert(SetCodeDisplay(dt.Copy(), bCodeDisplay));
                //    //cbo.SelectedIndex = -1;                    

                //}

                //if (e.Column.Name.Equals("ROUTID"))
                //{
                //    string sLANE_ID = Util.NVC(DataTableConverter.GetValue(e.Row.DataItem, "LANE_ID"));


                //    if (string.IsNullOrEmpty(sLANE_ID))
                //    {
                //        Util.MessageValidation("SFU4925", ObjectDic.Instance.GetObjectName("LANE_ID"));
                //        return;
                //    }

                //    DataTable dt = SetRoute(sLANE_ID);

                //    dt.AcceptChanges();

                //    cbo.ItemsSource = DataTableConverter.Convert(dt.Copy());
                //    //cbo.SelectedIndex = -1;

                //}
            }
        }

        private void dgContiBMList_CommittingEdit(object sender, DataGridEndingEditEventArgs e)
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

                    //DataTableConverter.SetValue(dgContiBMList.Rows[e.Row.Index].DataItem, "EQPTID", string.Empty); //CELL INIT

                    //DataTableConverter.SetValue(dgContiBMList.Rows[e.Row.Index].DataItem, "ROUTID", string.Empty); //CELL INIT
                }

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }


        private void dgContiBMList_CommittedEdit(object sender, DataGridCellEventArgs e)
        {
            C1DataGrid dg = sender as C1DataGrid;

            if (dg.CurrentColumn.Name == "LANE_ID")
            {
                //DataTableConverter.SetValue(dgContiBMList.Rows[dg.CurrentRow.Index].DataItem, "EQPTID", string.Empty); //CELL INIT

                //DataTableConverter.SetValue(dgContiBMList.Rows[dg.CurrentRow.Index].DataItem, "ROUTID", string.Empty); //CELL INIT

                //dg.CurrentRow.Refresh();
            }

        }


        private bool CheckContiBMinputValue(string sName)
        {
            bool bRet = true;

            for (int i = 0; i <= dgContiBMList.GetRowCount() - 1; i++)
            {
                if (Util.NVC(DataTableConverter.GetValue(dgContiBMList.Rows[i].DataItem, "CHK")).Equals("True"))
                {
                    string sValue = Util.NVC(DataTableConverter.GetValue(dgContiBMList.Rows[i].DataItem, sName));

                    for (int j = 0; j <= dgContiBMList.GetRowCount() - 1; j++)
                    {
                        if (!j.Equals(i))
                        {
                            if (DataTableConverter.GetValue(dgContiBMList.Rows[i].DataItem, sName).Equals(sValue))
                            {
                                bRet = false;
                            }
                        }

                    }
                }

            }

            return bRet;

        }

        private bool CheckContiBMinputValue(string sName, int nIndex)
        {
            bool bRet = true;

            string sValue = Util.NVC(DataTableConverter.GetValue(dgContiBMList.Rows[nIndex].DataItem, sName));

            for (int i = 0; i <= dgContiBMList.GetRowCount() - 1; i++)
            {
                if (!i.Equals(nIndex))
                {
                    if (DataTableConverter.GetValue(dgContiBMList.Rows[i].DataItem, sName).Equals(sValue))
                    {
                        bRet = false;
                        return bRet;
                    }
                }
            }

            return bRet;

        }

        private void dgContiBMList_LoadedCellPresenter(object sender, DataGridCellEventArgs e)
        {
            if (sender == null)
                return;

            C1.WPF.DataGrid.C1DataGrid dataGrid = sender as C1.WPF.DataGrid.C1DataGrid;
            C1.WPF.DataGrid.DataGridRow row = dgContiBMList.Rows[e.Cell.Row.Index];

            dataGrid.Dispatcher.BeginInvoke(new Action(() =>
            {

                if (e.Cell.Presenter == null || e.Cell.Column.Name == null)
                {
                    return;
                }

                if (e.Cell.Row.Type == DataGridRowType.Item)
                {
                    C1.WPF.DataGrid.DataGridCell cell = e.Cell;


                    if (cell.Column.Name.Equals("JIG_FORMATION_LIMIT_VALUE"))
                    {
                        if (Util.NVC(DataTableConverter.GetValue(row.DataItem, "JGF_LANE_USE_YN")).Equals("N"))
                        {
                            cell.Presenter.Background = new BrushConverter().ConvertFromString("LightGray") as SolidColorBrush;
                        }
                    }

                    if (cell.Column.Name.Equals("FORMEQPT_LIMIT_VALUE") || cell.Column.Name.Equals("MGFORM_LIMIT_VALUE"))
                    {
                        if (Util.NVC(DataTableConverter.GetValue(row.DataItem, "FOR_LANE_USE_YN")).Equals("N"))
                        {
                            cell.Presenter.Background = new BrushConverter().ConvertFromString("LightGray") as SolidColorBrush;
                        }
                    }
                }
            }));
        }


        private bool validInputData(string sTarget, C1.WPF.DataGrid.DataGridRow row, int minValue, int maxValue)
        {
            string sObjName, sValue;
            float convNumber;
            bool result = true;

            //BAD_PN_LIMIT_VALUE
            sValue = Util.NVC(DataTableConverter.GetValue(row.DataItem, sTarget));
            sObjName = ObjectDic.Instance.GetObjectName(sTarget);
            if (string.IsNullOrEmpty(sValue))
            {
                // SFU1299	%1이 입력되지 않았습니다.
                Util.MessageValidation("SFU1299", sObjName);
                result = false;
            }

            if (float.TryParse(sValue, out convNumber) == false || convNumber < 0)
            {
                // SFU4209	양수를 입력하세요.
                Util.MessageValidation("SFU4209", sObjName);
                result = false;
            }

            if (convNumber < minValue || convNumber > maxValue)
            {
                // SFU4209	양수를 입력하세요.
                Util.MessageValidation("10034", minValue, maxValue);
                result = false;
            }

            return result;
        }


        #endregion [ CONTINUOUS BM ]


        #region [ SD CONTINUOUS BM ]


        #region Event
        private void Loc_btnSDContiBMSearchClick(object sender, RoutedEventArgs e)
        {
            GetSDContiBMList();
        }

        private void Loc_btnSDContiBMSave_Click(object sender, RoutedEventArgs e)
        {

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
                        for (int i = 0; i < dgSDContiBMList.GetRowCount(); i++)
                        {
                            if (Util.NVC(DataTableConverter.GetValue(dgSDContiBMList.Rows[i].DataItem, "CHK")).Equals("True"))
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
                        dtRqst.Columns.Add("LINE_ID", typeof(string));
                        dtRqst.Columns.Add("MDLLOT_ID", typeof(string));
                        dtRqst.Columns.Add("STDEV_LIMIT_VALUE", typeof(string));
                        dtRqst.Columns.Add("CONTIDFCT_LIMIT_CNT", typeof(string));
                        dtRqst.Columns.Add("CHK_ACCU_TIME", typeof(string));
                        dtRqst.Columns.Add("USERID", typeof(string));
                        dtRqst.Columns.Add("USE_FLAG", typeof(string));

                        for (int i = 0; i < dgSDContiBMList.GetRowCount(); i++)
                        {
                            if (Util.NVC(DataTableConverter.GetValue(dgSDContiBMList.Rows[i].DataItem, "CHK")).Equals("True"))
                            {
                                if (!In_Validliation(dgSDContiBMList.Rows[i])) return;

                                DataRow dr = dtRqst.NewRow();
                                dr["LINE_ID"] = Util.NVC(DataTableConverter.GetValue(dgSDContiBMList.Rows[i].DataItem, "LINE_ID"));
                                dr["MDLLOT_ID"] = Util.NVC(DataTableConverter.GetValue(dgSDContiBMList.Rows[i].DataItem, "MDLLOT_ID"));
                                dr["STDEV_LIMIT_VALUE"] = Util.NVC(DataTableConverter.GetValue(dgSDContiBMList.Rows[i].DataItem, "STDEV_LIMIT_VALUE"));
                                dr["CONTIDFCT_LIMIT_CNT"] = Util.NVC(DataTableConverter.GetValue(dgSDContiBMList.Rows[i].DataItem, "CONTIDFCT_LIMIT_CNT"));
                                dr["CHK_ACCU_TIME"] = Util.NVC(DataTableConverter.GetValue(dgSDContiBMList.Rows[i].DataItem, "CHK_ACCU_TIME"));
                                dr["USE_FLAG"] = Util.NVC(DataTableConverter.GetValue(dgSDContiBMList.Rows[i].DataItem, "USE_FLAG"));
                                dr["USERID"] = LoginInfo.USERID;
                                dtRqst.Rows.Add(dr);
                            }
                        }

                        ShowLoadingIndicator();

                        new ClientProxy().ExecuteService("BR_SET_TB_SFC_MDL_STDEV_CONTI_DFCT_BAS", "INDATA", null, dtRqst, (bizResult, bizException) =>
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
                                        GetSDContiBMList();
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

        private void dgSDContiBMList_LoadedRowHeaderPresenter(object sender, C1.WPF.DataGrid.DataGridRowEventArgs e)
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

        #endregion

        #region Method

        private bool In_Validliation(C1.WPF.DataGrid.DataGridRow row)
        {
            Double convNumber;

            string[] columns = { "LINE_ID", "MDLLOT_ID", "STDEV_LIMIT_VALUE", "CONTIDFCT_LIMIT_CNT", "CHK_ACCU_TIME" };
            foreach (string col in columns)
            {
                if (Util.IsNVC(DataTableConverter.GetValue(row.DataItem, col)) || DataTableConverter.GetValue(row.DataItem, col).ToString() == "SELECT")
                {
                    Util.MessageValidation("SFU4925", ObjectDic.Instance.GetObjectName(col));
                    return false;
                }
                if (col == "CHK_ACCU_TIME")
                {
                    convNumber = Convert.ToDouble(DataTableConverter.GetValue(row.DataItem, col));

                    if (convNumber < CONTI_BM_MIN_TIME_VALUE || CONTI_BM_MAX_TIME_VALUE < convNumber)
                    {
                        Util.MessageValidation("10034", CONTI_BM_MIN_TIME_VALUE, CONTI_BM_MAX_TIME_VALUE);
                        return false;
                    }

                }
            }
            return true;
        }

        public void Get_MMD_SEL_CMCD_TYPE_MULT(string sCMCDTYPE, string sUSE_FLAG, Action<DataTable, Exception> ACTION_COMPLETED)
        {
            DataTable IndataTable = new DataTable();
            IndataTable.Columns.Add("LANGID", typeof(string));
            IndataTable.Columns.Add("CMCDTYPE", typeof(string));
            IndataTable.Columns.Add("USE_FLAG", typeof(string));

            DataRow Indata = IndataTable.NewRow();
            Indata["LANGID"] = LoginInfo.LANGID;
            Indata["CMCDTYPE"] = sCMCDTYPE;
            Indata["USE_FLAG"] = sUSE_FLAG;

            IndataTable.Rows.Add(Indata);

            new ClientProxy().ExecuteService("MMD_SEL_CMCD_TYPE_MULT", "RQSTDT", "RSLTDT", IndataTable, (Result, Exception) =>
            {
                try
                {
                    if (ACTION_COMPLETED != null) ACTION_COMPLETED(Result, Exception);
                }
                catch (Exception ex)
                {
                    ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
                }
            }
            );
        }

        public void Get_FORM_EQSG_CBO(Action<DataTable, Exception> ACTION_COMPLETED)
        {
            DataTable IndataTable = new DataTable();
            IndataTable.Columns.Add("LANGID", typeof(string));
            IndataTable.Columns.Add("AREAID", typeof(string));

            DataRow Indata = IndataTable.NewRow();
            Indata["LANGID"] = LoginInfo.LANGID;
            Indata["AREAID"] = LoginInfo.CFG_AREA_ID;

            IndataTable.Rows.Add(Indata);

            new ClientProxy().ExecuteService("DA_BAS_SEL_COMBO_FORM_LINE", "RQSTDT", "RSLTDT", IndataTable, (Result, Exception) =>
            {
                try
                {
                    if (ACTION_COMPLETED != null) ACTION_COMPLETED(Result, Exception);
                }
                catch (Exception ex)
                {
                    ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
                }
            }
            );
        }

        private void cboUseFlag_SelectedItemChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            try
            {
                //if (!CommonVerify.HasDataGridRow(dgAutoChgRoutSet))
                //    return;

                GetSDContiBMList();
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        public void Get_FORM_MDLLOT_CBO(string sEQSGID, Action<DataTable, Exception> ACTION_COMPLETED)
        {
            DataTable IndataTable = new DataTable();
            IndataTable.Columns.Add("AREAID", typeof(string));
            IndataTable.Columns.Add("LANGID", typeof(string));
            IndataTable.Columns.Add("EQSGID", typeof(string));

            DataRow Indata = IndataTable.NewRow();
            Indata["AREAID"] = LoginInfo.CFG_AREA_ID;
            Indata["LANGID"] = LoginInfo.LANGID;
            Indata["EQSGID"] = sEQSGID;

            IndataTable.Rows.Add(Indata);

            new ClientProxy().ExecuteService("DA_BAS_SEL_COMBO_FORM_MODEL_CBO", "RQSTDT", "RSLTDT", IndataTable, (Result, Exception) =>
            {
                try
                {
                    if (ACTION_COMPLETED != null) ACTION_COMPLETED(Result, Exception);
                }
                catch (Exception ex)
                {
                    ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
                }
            }
            );
        }

        private void GetSDContiBMList()
        {
            try
            {
                Util.gridClear(dgSDContiBMList);
                //dgSDContiBMList.ClearRows();

                DataTable dtRqst = new DataTable();

                dtRqst.Columns.Add("USE_FLAG", typeof(string));

                DataRow dr = dtRqst.NewRow();
                dr["USE_FLAG"] = Util.GetCondition(cboUseFlag, bAllNull: true);

                dtRqst.Rows.Add(dr);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_SEL_TB_SFC_MDL_STDEV_CONTI_DFCT_BAS", "RQSTDT", "RSLTDT", dtRqst);

                if (dtRslt == null || dtRslt.Rows.Count == 0)
                {
                    Util.GridSetData(dgSDContiBMList, dtRslt, FrameOperation);

                    ////조회된 값이 없습니다.
                    //Util.Alert("FM_ME_0232");
                    //return;
                }
                else
                {
                    //dgSDContiBMList.ItemsSource = DataTableConverter.Convert(dtRslt);
                    Util.GridSetData(dgSDContiBMList, dtRslt, FrameOperation);
                }

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void Loc_btnUnitPlus_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Util.DataGridRowAdd(dgSDContiBMList, 1);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void Loc_btnUnitMinus_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Util.DataGridRowDelete(dgSDContiBMList, 1);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        private void dgSDContiBMList_BeginningNewRow(object sender, C1.WPF.DataGrid.DataGridBeginningNewRowEventArgs e)
        {
            e.Item.SetValue("CHK", true);
            e.Item.SetValue("USE_FLAG", "Y");
            e.Item.SetValue("LINE_ID", "SELECT");
            e.Item.SetValue("MDLLOT_ID", "SELECT");
            e.Item.SetValue("INSUSER", LoginInfo.USERID);
            e.Item.SetValue("UPDUSER", LoginInfo.USERID);
        }

        private void dgSDContiBMList_BeginningEdit(object sender, C1.WPF.DataGrid.DataGridBeginningEditEventArgs e)
        {
            try
            {
                //string[] exceptionColumn = { "AREAID", "GROUPCODE" };
                Util.DataGridRowEditByCheckBoxColumn(e, null); //CHK true인 행만 수정가능

                DataRowView drv = e.Row.DataItem as DataRowView;

                string sCHK = Util.NVC(DataTableConverter.GetValue(e.Row.DataItem, "CHK")).ToUpper().ToString();

                if (sCHK.Equals("TRUE"))
                {
                    C1DataGrid dg = sender as C1DataGrid;
                    if (drv.Row.RowState == DataRowState.Added || drv.Row.RowState == DataRowState.Detached)
                    {
                        if (e.Column.Name == "MDLLOT_ID")
                        {
                            string sLineId = Util.NVC(DataTableConverter.GetValue(e.Row.DataItem, "LINE_ID"));
                            string sMdllotId = Util.NVC(DataTableConverter.GetValue(e.Row.DataItem, "MDLLOT_ID"));

                            if (string.IsNullOrEmpty(sLineId) || string.Equals(sLineId, "SELECT"))
                            {
                                Util.MessageValidation("SFU4925", ObjectDic.Instance.GetObjectName("LINE_ID"));
                                //DataTableConverter.SetValue(e.Row.DataItem, "MDLLOT_ID", "SELECT");
                                //DataTableConverter.SetValue(e.Row.DataItem, "CHG_ROUT", "SELECT");
                                //dg.Refresh();

                                e.Cancel = true;
                            }
                        }
                    }
                    else
                    {
                        if (e.Column != this.dgSDContiBMList.Columns["CHK"]
                         && e.Column != this.dgSDContiBMList.Columns["USE_FLAG"]
                         && e.Column != this.dgSDContiBMList.Columns["STDEV_LIMIT_VALUE"]
                         && e.Column != this.dgSDContiBMList.Columns["CHK_ACCU_TIME"]
                         && e.Column != this.dgSDContiBMList.Columns["CONTIDFCT_LIMIT_CNT"])
                        {
                            e.Cancel = true;
                        }
                        else
                        {
                            e.Cancel = false;
                        }
                    }

                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void dgSDContiBMList_BeganEdit(object sender, DataGridBeganEditEventArgs e)
        {
            try
            {
                C1ComboBox cbo = e.EditingElement as C1ComboBox;
                DataRowView drv = e.Row.DataItem as DataRowView;
                //C1DataGrid dg = sender as C1DataGrid;

                if (cbo != null)
                {
                    string sSelectedValue = drv[e.Column.Name].SafeToString();

                    if (e.Column.Name == "LINE_ID")
                    {
                        //
                    }
                    else if (e.Column.Name == "MDLLOT_ID")
                    {
                        string sLineId = Util.NVC(DataTableConverter.GetValue(drv, "LINE_ID"));

                        if (!string.IsNullOrEmpty(sLineId))
                        {
                            string sFilter = "EQSGID = '" + sLineId + "'";
                            _dvMODEL.RowFilter = sFilter;

                            CommonCombo.SetDtCommonCombo(_dvMODEL.ToTable(), cbo, CommonCombo.ComboStatus.SELECT, sSelectedValue);
                        }
                    }

                    cbo.EditCompleted += delegate (object sender1, EventArgs e1)
                    {
                        Dispatcher.BeginInvoke(new Action(() =>
                        {
                            this.UpdateRowView(e.Row, e.Column);
                        }));
                    };
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void dgSDContiBMList_CommittingEdit(object sender, DataGridEndingEditEventArgs e)
        {
            if (_manualCommit == false)
            {
                //   this.UpdateRowView(e.Row, e.Column);
            }

        }

        private void CheckBox_Click(object sender, RoutedEventArgs e)
        {
            int checkIndex = ((C1.WPF.DataGrid.DataGridCellPresenter)((sender as CheckBox).Parent)).Row.Index;

        }

        private void dgSDContiBMList_CommittedEdit(object sender, DataGridCellEventArgs e)
        {
            try
            {

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private bool In_Validliation(object row)
        {
            string[] columns = { "LINE_ID", "MDLLOT_ID", "STDEV_LIMIT_VALUE", "CONTIDFCT_LIMIT_CNT" };
            foreach (string col in columns)
            {
                if (Util.IsNVC(DataTableConverter.GetValue(row, col)) || DataTableConverter.GetValue(row, col).ToString() == "SELECT")
                {
                    Util.MessageValidation("SFU4925", ObjectDic.Instance.GetObjectName(col));
                    return false;
                }
            }
            return true;
        }


        private void dgSDContiBMList_LoadedCellPresenter(object sender, DataGridCellEventArgs e)
        {
            if (!((DataRowView)e.Cell.Row.DataItem).Row.RowState.ToString().Equals(DataRowState.Added.ToString()))
            {
                string[] Col = { "LINE_ID", "MDLLOT_ID", "INSUSER", "INSDTTM", "UPDUSER", "UPDDTTM" };

                dgSDContiBMList?.Dispatcher.BeginInvoke(new Action(() =>
                {
                    if (e.Cell == null || e.Cell.Presenter == null) return;

                    if (Col.Contains(e.Cell.Column.Name))
                    {
                        e.Cell.Presenter.Background = (SolidColorBrush)new BrushConverter().ConvertFrom("#fff8f8f8");
                    }
                }));
            }
            else
            {
                string[] Col = { "INSUSER", "INSDTTM", "UPDUSER", "UPDDTTM" };

                dgSDContiBMList?.Dispatcher.BeginInvoke(new Action(() =>
                {
                    if (e.Cell == null || e.Cell.Presenter == null) return;

                    if (Col.Contains(e.Cell.Column.Name))
                    {
                        e.Cell.Presenter.Background = (SolidColorBrush)new BrushConverter().ConvertFrom("#fff8f8f8");
                    }
                }));
            }
        }

        void UpdateRowView(C1.WPF.DataGrid.DataGridRow dgr, C1.WPF.DataGrid.DataGridColumn dgc)
        {
            try
            {
                DataRowView drv = dgr.DataItem as DataRowView;

                if (drv != null && Convert.ToString(dgc.Name) == "LINE_ID")
                {
                    _manualCommit = true;

                    drv["MDLLOT_ID"] = "SELECT";
                    this.dgSDContiBMList.EndEditRow(true);
                }

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
            finally
            {
                _manualCommit = false;
            }
        }

        private void chkHeaderAll_Checked(object sender, RoutedEventArgs e)
        {
            Util.DataGridCheckAllChecked(dgSDContiBMList);
        }

        private void chkHeaderAll_Unchecked(object sender, RoutedEventArgs e)
        {
            Util.DataGridCheckAllUnChecked(dgSDContiBMList);
        }
        #endregion

        #endregion [ SD CONTINUOUS BM ]
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
        private bool LaneSetGridCboItem(C1.WPF.DataGrid.DataGridColumn col, string sClassId, bool bCodeDisplay = true, string sCmnCd = null)
        {
            try
            {
                bool rtn = false;

                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("ONLY_X", typeof(string));
                RQSTDT.Columns.Add("LANE_LAST_CHAR", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["ONLY_X"] = (string.IsNullOrEmpty(sCmnCd)) ? "Y" : null;
                //if (sFilter.Length > 1)
                //    dr["LANE_LAST_CHAR"] = sFilter[1];
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_LANE_CBO", "RQSTDT", "RSLTDT", RQSTDT);

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
                //RQSTDT.Columns.Add("EQP_ID_LAST", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                //dr["S70"] = sFilter[0]; //EQPT_GR_TYPE_CODE
                if (sFilter.Length > 2)
                    dr["S71"] = sFilter[1]; //LANE_ID

                if (sFilter.Length > 3)
                    dr["EQP_ID_LAST"] = sFilter[2];

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

        private bool RouteGridColItem(C1.WPF.DataGrid.DataGridColumn col, string sClassId, bool bCodeDisplay = true)
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


        private int getColIndex(C1.WPF.DataGrid.DataGridColumnCollection columns, string tagName)
        {
            bool isMatched = false;
            int index = 0;


            foreach (C1.WPF.DataGrid.DataGridColumn col in columns)
            {
                if(col.Tag.ToString() == tagName)
                {
                    isMatched = true;
                    break;
                }

                index++;
            }

            return (isMatched) ? index : -1;
        }

    }
}
 
