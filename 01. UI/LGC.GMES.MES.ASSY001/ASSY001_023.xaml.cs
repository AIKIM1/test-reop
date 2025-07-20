/*************************************************************************************
 Created Date : 2016.06.16
      Creator : 
   Decription : 
--------------------------------------------------------------------------------------
 [Change History]
  2016.06.16  DEVELOPER : Initial Created.
  2022.04.19  장희만    : C20220410-000011- V/D(STP후) 공정도 재작업 처리 할 수 있도록 수정  
  2022.07.13  김태균    : NNF Project - 소형도 가능하도록 수정
  2022.11.17  배현우    : M9동 ZZS VD (A6000) 추가 
  2023.12.12  박승렬    : E20231201-001866 / 텍스트 박스"LOTID(CSTID)" 에 복사,붙여넣기로 LOT 조회 가능하도록 수정
  2023.12.18  박승렬    : E20231201-001866 / 복사,붙여넣기로 LOT 조회 시, LOT 정보가 없을 경우 메세지창으로 해당 LOT 표시 
**************************************************************************************/

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
using System.Linq;

namespace LGC.GMES.MES.ASSY001
{
    public partial class ASSY001_023 : UserControl, IWorkArea
    {

        #region Declaration & Constructor 

        CommonCombo combo = new CommonCombo();
        Util _Util = new Util();

        public IFrameOperation FrameOperation
        {
            get;
            set;
        }

        public ASSY001_023()
        {
            InitializeComponent();
            this.Loaded += UserControl_Loaded;
        }

        #endregion
        // BIZ 에러 체크
        bool EX_Check = false;
        // 조회 결과가 없는 LotList
        string EmptyLot = string.Empty;
        #region event
        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            this.Loaded -= UserControl_Loaded;
            ApplyPermissions();
            initCombo();
        }
        private void txtModel_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                string[] sFilter = { txtModel.Text };
                combo.SetCombo(cboModel, CommonCombo.ComboStatus.ALL, sFilter: sFilter);
            }
        }

        private void CheckBox_Click(object sender, RoutedEventArgs e)
        {

        }
        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            SearchData();

        }
        private void SelectDate_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                SearchData();
            }

        }
        C1.WPF.DataGrid.DataGridRowHeaderPresenter pre = new C1.WPF.DataGrid.DataGridRowHeaderPresenter()
        {
            Background = new SolidColorBrush(Colors.Transparent),
            MouseOverBrush = new SolidColorBrush(Colors.Transparent),
        };
        CheckBox chkAll = new CheckBox()
        {
            IsChecked = false,
            Background = new SolidColorBrush(Colors.Transparent),
            VerticalAlignment = System.Windows.VerticalAlignment.Center,
            HorizontalAlignment = System.Windows.HorizontalAlignment.Center

        };
        private void dgReworkLotInfo_LoadedColumnHeaderPresenter(object sender, C1.WPF.DataGrid.DataGridColumnEventArgs e)
        {
            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                //this.Dispatcher.BeginInvoke
                if (string.IsNullOrEmpty(e.Column.Name) == false)
                {
                    if (e.Column.Name.Equals("CHK"))
                    {
                        pre.Content = chkAll;
                        e.Column.HeaderPresenter.Content = pre;
                        chkAll.Checked -= new RoutedEventHandler(checkAll_Checked);
                        chkAll.Unchecked -= new RoutedEventHandler(checkAll_Unchecked);
                        chkAll.Checked += new RoutedEventHandler(checkAll_Checked);
                        chkAll.Unchecked += new RoutedEventHandler(checkAll_Unchecked);
                    }
                }
            }));
        }

        void checkAll_Checked(object sender, RoutedEventArgs e)
        {
            DataTable dt = DataTableConverter.Convert(dgReworkLotInfo.ItemsSource);

            for (int i = 0; i < dt.Rows.Count; i++)
            {
                dt.Rows[i]["CHK"] = true;
            }

            dgReworkLotInfo.ItemsSource = DataTableConverter.Convert(dt);

        }
        void checkAll_Unchecked(object sender, RoutedEventArgs e)
        {
            DataTable dt = DataTableConverter.Convert(dgReworkLotInfo.ItemsSource);

            for (int i = 0; i < dt.Rows.Count; i++)
            {
                dt.Rows[i]["CHK"] = false;
            }

            dgReworkLotInfo.ItemsSource = DataTableConverter.Convert(dt);

        }
        private void btnRework_Click(object sender, RoutedEventArgs e)
        {
            if (cboVDEquipmentSegment.SelectedValue.ToString().Equals("SELECT"))
            {
                Util.MessageValidation("SFU3071");
                //Util.Alert("이동할 VD라인을 선택해주세요");
                return;
            }

            // VD 재작업을 하시겠습니까? \n 작업 시 [%1]의 예약 대기 상태로 돌아갑니다. 
            LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("SFU3072", Convert.ToString(cboVDEquipmentSegment.Text)), null, "Information", MessageBoxButton.OKCancel, MessageBoxIcon.None, (result) =>
            {
                if (result == MessageBoxResult.OK)
                {
                    ReworkProcess();
                }
            });

        }



        #endregion

        #region Method
        private void initCombo()
        {
            string[] sFilter = { LoginInfo.CFG_AREA_ID };
            combo.SetCombo(cboEquipmentSegment, CommonCombo.ComboStatus.NONE, sFilter: sFilter);

            string[] sFilter2 = { null };
            combo.SetCombo(cboModel, CommonCombo.ComboStatus.ALL, sFilter: sFilter2);

            if (LoginInfo.CFG_AREA_ID == "M9") //오창 2산단 NFF, ZZS Pilot 라인 전용 2022-11-17 배현우
            {
                string[] sFilter1 = {Process.VD_LMN + "," + Process.VD_CDE_PRS + "," + Process.WINDING, LoginInfo.CFG_AREA_ID };
                combo.SetCombo(cboVDEquipmentSegment, CommonCombo.ComboStatus.SELECT, sFilter: sFilter1);
            }
            else
            {
                string[] sFilter1 = { Process.VD_LMN + "," + Process.VD_LMN_AFTER_STP, LoginInfo.CFG_AREA_ID };
                combo.SetCombo(cboVDEquipmentSegment, CommonCombo.ComboStatus.SELECT, sFilter: sFilter1);
            }

        }



        private void ApplyPermissions()
        {
            List<Button> listAuth = new List<Button>();

            Util.pageAuth(listAuth, FrameOperation.AUTHORITY);
        }
        public void HiddenLoadingIndicator()
        {
            if (loadingIndicator != null)
                loadingIndicator.Visibility = Visibility.Collapsed;
        }
        public void ShowLoadingIndicator()
        {
            if (loadingIndicator != null)
                loadingIndicator.Visibility = Visibility.Visible;
        }
        private void SearchData()
        {
            try
            {

                string bizRuleName = string.Empty;

                DataSet ds = new DataSet();

                DataTable dt = new DataTable("INDATA");
                dt.Columns.Add("LANGID", typeof(string));
                dt.Columns.Add("EQSGID", typeof(string));
                dt.Columns.Add("MODLID", typeof(string));
                dt.Columns.Add("WIPDTTM", typeof(string));
                dt.Columns.Add("LOTID", typeof(string));

                DataRow row = dt.NewRow();
                row["LANGID"] = LoginInfo.LANGID;
                row["EQSGID"] = Convert.ToString(cboEquipmentSegment.SelectedValue);
                row["MODLID"] = Convert.ToString(cboModel.SelectedValue).Equals("") ? null : Convert.ToString(cboModel.SelectedValue);
                row["WIPDTTM"] = SelectDate.Value;
                row["LOTID"] = txtLOTID.Text;
                dt.Rows.Add(row);

                ds.Tables.Add(dt);

                ShowLoadingIndicator();

                //STP후VD 재작인 경우 추가 2022.04.19
                if (cboEquipmentSegment.SelectedValue.ToString().Equals(LoginInfo.CFG_AREA_ID + "MV2")) //2022.04.19
                {
                    bizRuleName = "BR_PRD_SEL_WIP_STPVD_REWORK";
                }
                else
                {
                    if (LoginInfo.CFG_AREA_ID == "M9")
                        bizRuleName = "BR_PRD_SEL_WIP_VD_REWORK_NFF";
                    else 
                        bizRuleName = "BR_PRD_SEL_WIP_VD_REWORK";
                }
                
                //new ClientProxy().ExecuteService_Multi("BR_PRD_SEL_WIP_VD_REWORK", "INDATA", "OUTDATA", (searchResult, searchException) =>
                new ClientProxy().ExecuteService_Multi(bizRuleName, "INDATA", "OUTDATA", (searchResult, searchException) =>
                {
                    try
                    {
                        if (searchException != null)
                        {
                            Util.MessageException(searchException);
                            //Util.AlertByBiz("DA_PRD_SEL_WIP_VD_REWORK", searchException.Message, searchException.ToString());
                            return;
                        }

                        Util.GridSetData(dgReworkLotInfo, searchResult.Tables["OUTDATA"], FrameOperation, true);
                        //  SetVDReworkLot(searchResult);//END중에 검사대기, 합격, 수분검사 LOT만 재조회

                        //Util.GridSetData(dgReworkLotInfo, searchResult, FrameOperation, true);

                    }
                    catch (Exception ex)
                    {
                        Util.MessageException(ex);
                        //LGC.GMES.MES.ControlsLibrary.MessageBox.ShowKeyEnter(ex.Message, ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
                    }
                    finally
                    {
                        HiddenLoadingIndicator();
                    }
                }, ds);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                //Util.Alert(ex.ToString());
            }
        }

        private void SetVDReworkLot(DataTable dt)
        {
            if (dt == null) return;
            if (dt.Rows.Count != 0)
            {

                try
                {
                    dt.Columns.Add("JUDG_VALUE", typeof(string));
                    dt.Columns.Add("JUDG_NAME", typeof(string));

                    DataSet ds = new DataSet();

                    DataTable inData = ds.Tables.Add("INDATA");
                    inData.Columns.Add("LANGID", typeof(string));

                    DataRow row = inData.NewRow();
                    row["LANGID"] = LoginInfo.LANGID;
                    inData.Rows.Add(row);

                    //DataTable inLot = dt;
                    //ds.Tables.Add(inLot);
                    DataTable inLot = ds.Tables.Add("IN_LOT");
                    foreach (DataColumn col in dt.Columns)
                    {
                        inLot.Columns.Add(col.ColumnName, col.DataType);
                    }

                    foreach (DataRow rw in dt.Rows)
                    {
                        row = inLot.NewRow();
                        foreach (DataColumn col in dt.Columns)
                        {
                            row[col.ColumnName] = rw[col.ColumnName];
                        }

                        inLot.Rows.Add(row);
                    }


                    //비즈룰 호출

                    DataSet result = new ClientProxy().ExecuteServiceSync_Multi("BR_PRD_SET_QA_JUDG_VD", "INDATA, IN_LOT", "OUT_LOT", ds);

                    Util.GridSetData(dgReworkLotInfo, result.Tables["OUT_LOT"], FrameOperation, true);


                    //new ClientProxy().ExecuteService_Multi("BR_PRD_SET_QA_JUDG_VD", "INDATA, IN_LOT","OUT_LOT", (searchResult, searchException) =>
                    //{
                    //    try
                    //    {
                    //        if (searchException != null)
                    //        {
                    //            Util.AlertByBiz("BR_PRD_SET_QA_JUDG_VD", searchException.Message, searchException.ToString());
                    //            return;
                    //        }

                    //        Util.GridSetData(dgReworkLotInfo, searchResult.Tables["OUT_LOT"], FrameOperation, true);

                    //    }
                    //    catch (Exception ex)
                    //    {
                    //        LGC.GMES.MES.ControlsLibrary.MessageBox.ShowKeyEnter(ex.Message, ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
                    //    }
                    //    finally
                    //    {
                    //        HiddenLoadingIndicator();
                    //    }
                    //}, ds);


                }
                catch (Exception ex)
                {
                    Util.MessageException(ex);
                    //Util.Alert(ex.ToString());
                }


                //for (int i = 0; i < dt.Rows.Count; i++)
                //{
                //    if (dt.Rows[i]["ELEC"].Equals("C"))
                //    {
                //        ValidateLOTCQA(dt.Rows[i]);
                //    }
                //    else
                //    {
                //        ValidateLOTAQA(dt.Rows[i]);
                //    }
                //}
            }
            else
            {
                Util.GridSetData(dgReworkLotInfo, dt, FrameOperation, true);
            }


        }
        private void txtLOTID_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                for (int i = 0; i < dgReworkLotInfo.GetRowCount(); i++)
                {
                    if (Util.NVC(DataTableConverter.GetValue(dgReworkLotInfo.Rows[i].DataItem, "LOTID")).Equals(txtLOTID.Text))
                    {
                        DataTableConverter.SetValue(dgReworkLotInfo.Rows[i].DataItem, "CHK", true);
                        txtLOTID.Text = "";
                        dgReworkLotInfo.ScrollIntoView(i, 0);
                    }
                }
            }
        }


        private void PreviewKeyDown_SearchData(string sLOTID)
        {
            try
            {
                string bizRuleName = string.Empty;

                DataSet ds = new DataSet();

                DataTable dt = new DataTable("INDATA");
                dt.Columns.Add("LANGID", typeof(string));
                dt.Columns.Add("EQSGID", typeof(string));
                dt.Columns.Add("MODLID", typeof(string));
                dt.Columns.Add("WIPDTTM", typeof(string));
                dt.Columns.Add("LOTID", typeof(string));

                DataRow row = dt.NewRow();
                row["LANGID"] = LoginInfo.LANGID;
                row["EQSGID"] = Convert.ToString(cboEquipmentSegment.SelectedValue);
                row["MODLID"] = Convert.ToString(cboModel.SelectedValue).Equals("") ? null : Convert.ToString(cboModel.SelectedValue);
                row["WIPDTTM"] = SelectDate.Value;
                row["LOTID"] = Util.NVC(sLOTID);
           
                dt.Rows.Add(row);
                ds.Tables.Add(dt);
                ShowLoadingIndicator();

                //STP후VD 재작인 경우 추가 2022.04.19
                if (cboEquipmentSegment.SelectedValue.ToString().Equals(LoginInfo.CFG_AREA_ID + "MV2")) //2022.04.19
                {
                    bizRuleName = "BR_PRD_SEL_WIP_STPVD_REWORK";
                }
                else
                {
                    if (LoginInfo.CFG_AREA_ID == "M9")
                        bizRuleName = "BR_PRD_SEL_WIP_VD_REWORK_NFF";
                    else
                        bizRuleName = "BR_PRD_SEL_WIP_VD_REWORK";
                }


                 if (!string.IsNullOrEmpty(sLOTID))
                 {
                    try
                    {
                        DataTable dtResult = new ClientProxy().ExecuteServiceSync(bizRuleName, "INDATA", "OUTDATA", dt);

                        if (dtResult.Rows.Count > 0)
                        {
                            if (dgReworkLotInfo.GetRowCount() == 0)
                            {
                                Util.GridSetData(dgReworkLotInfo, dtResult, FrameOperation);
                            }
                            else
                            {
                                DataTable dtInfo = DataTableConverter.Convert(dgReworkLotInfo.ItemsSource);
                                dtInfo.Merge(dtResult);
                                Util.GridSetData(dgReworkLotInfo, dtInfo, FrameOperation);
                            }
                        }
                        else
                        {
                            if (string.IsNullOrEmpty(EmptyLot))
                                EmptyLot += sLOTID;
                            else
                                EmptyLot = EmptyLot + ", " + sLOTID;
                        }       
                    }

                    catch (Exception ex)
                    {
                        Util.MessageException(ex);
                        EX_Check = true;               
                        return;
                    }
                }
                 else
                 {                   
                    HiddenLoadingIndicator();
                 } 
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return;             
            }
            finally
            {
                HiddenLoadingIndicator();
            }
        }

        private void txtLOTID_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.V && Keyboard.IsKeyDown(Key.LeftCtrl))
            {
                try
                {
                    EX_Check = false;
                    dgReworkLotInfo.ItemsSource = null;

                    string sPasteString = string.Empty;
                    string[] stringSeparators = new string[] { "\r\n" };
                    sPasteString = Clipboard.GetText();
                    string[] sPasteStrings = sPasteString.Split(stringSeparators, StringSplitOptions.None);

                    if (sPasteStrings.Count() > 11)
                    {
                        Util.MessageValidation("SFU4643");   //최대 10개 까지 가능합니다.
                        return;
                    }


                    for (int i = 0; i < sPasteStrings.Length; i++)
                    {                      
                        string ScanID = string.Empty;
                        ScanID = sPasteStrings[i].Trim();

                        if (!String.IsNullOrEmpty(ScanID) && EX_Check == false)
                        {                           
                            PreviewKeyDown_SearchData(ScanID);
                        }                   
                    }

                    if (!string.IsNullOrEmpty(EmptyLot))
                    {
                        Util.MessageValidation("SFU3588", EmptyLot);  // 입력한 LOTID[% 1] 정보가 없습니다.
                        EmptyLot = string.Empty;
                    }


                }
                catch (Exception ex)
                {
                    LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
                }

            }

        }

       


        //private void ValidateLOTCQA(DataRow lotInfo) //양극일 경우 합불판정
        //{
        //    DataTable dt = null;
        //    DataRow row = null;

        //    dt = new DataTable();
        //    dt.Columns.Add("LOTID", typeof(string));

        //    row = dt.NewRow();
        //    row["LOTID"] = lotInfo["LOTID"];
        //    dt.Rows.Add(row);

        //    DataTable result = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_QA_INSP_HIST_LOT", "RQSTDT", "RSLTDT", dt); //lot별로 조회
        //    if (result.Rows.Count == 0 || !result.Rows[0]["JUDG_VALUE"].Equals("RF"))
        //    {
        //        dt = new DataTable();
        //        dt.Columns.Add("LOTID_RT", typeof(string));

        //        row = dt.NewRow();
        //        row["LOTID_RT"] = lotInfo["LOTID_RT"];
        //        dt.Rows.Add(row);

        //        result = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_LOT_RT_QA", "RQSTDT", "RSLTDT", dt); //대LOT별로 조회
        //        if (result.Rows.Count == 0 || result.Rows[0]["JUDG_VALUE"].Equals("RF"))
        //        {
        //            lotInfo["JUDG_VALUE"] = "E"; //검사대기
        //            lotInfo["JUDG_NAME"] = SetQAName("E");
        //            return;
        //        }


        //    }
        //    else
        //    {
        //        dt = new DataTable();
        //        dt.Columns.Add("EQPT_BTCH_WRK_NO", typeof(string));

        //        row = dt.NewRow();
        //        row["EQPT_BTCH_WRK_NO"] = lotInfo["EQPT_BTCH_WRK_NO"];
        //        dt.Rows.Add(row);

        //        result = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_QA_HIST_EQPT_BATCHID", "RQSTDT", "RSLTDT", dt); //배치ID의 최신QA값 조회
        //        if (result.Rows.Count == 0)
        //        {
        //            lotInfo["JUDG_VALUE"] = "E";
        //            lotInfo["JUDG_NAME"] = SetQAName("E");
        //            return;
        //        }

        //    }

        //    lotInfo["JUDG_VALUE"] = result.Rows[0]["JUDG_VALUE"];
        //    lotInfo["JUDG_NAME"] = SetQAName(Convert.ToString(result.Rows[0]["JUDG_VALUE"]));

        //}

        //private string SetQAName(string qaValue)
        //{
        //    try
        //    {
        //        DataTable dt = new DataTable();
        //        dt.Columns.Add("LANGID", typeof(string));
        //        dt.Columns.Add("CMCODE", typeof(string));

        //        DataRow row = dt.NewRow();
        //        row["LANGID"] = LoginInfo.LANGID;
        //        row["CMCODE"] = qaValue;
        //        dt.Rows.Add(row);

        //        DataTable result = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_COMMONCODE_QA_NAME", "RQSTDT", "RSLTDT", dt);
        //        if (result.Rows.Count == 0) return null;

        //        return Convert.ToString(result.Rows[0]["CMCDNAME"]);
        //    }
        //    catch (Exception ex)
        //    {
        //        Util.Alert(ex.ToString());
        //        return null;
        //    }


        //}
        //private void ValidateLOTAQA(DataRow lotInfo)//음극일 경우 합불판정
        //{
        //    DataTable dt = null;
        //    DataRow row = null;

        //    dt = new DataTable();
        //    dt.Columns.Add("EQPT_BTCH_WRK_NO", typeof(string));

        //    row = dt.NewRow();
        //    row["EQPT_BTCH_WRK_NO"] = lotInfo["EQPT_BTCH_WRK_NO"];
        //    dt.Rows.Add(row);

        //    DataTable result = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_QA_HIST_EQPT_BATCHID", "RQSTDT", "RSLTDT", dt); //배치ID의 최신QA값 조회
        //    if (result.Rows.Count == 0)
        //    {
        //        lotInfo["JUDG_VALUE"] = "E";
        //        lotInfo["JUDG_NAME"] = SetQAName("E");
        //        return;
        //    }

        //    lotInfo["JUDG_VALUE"] = result.Rows[0]["JUDG_VALUE"];
        //    lotInfo["JUDG_NAME"] = SetQAName(Convert.ToString(result.Rows[0]["JUDG_VALUE"]));
        //}
        private void ReworkProcess()
        {
            try
            {
                string sBizName = string.Empty;

                if (LoginInfo.CFG_AREA_ID == "M9")
                    sBizName = "BR_PRD_REG_MOVE_LOT_WAIT_VD_NFF";
                else
                    sBizName = "BR_PRD_REG_MOVE_LOT_WAIT_VD";

                DataSet ds = new DataSet();
                DataTable dt = ds.Tables.Add("INDATA");
                dt.Columns.Add("SRCTYPE", typeof(string));
                dt.Columns.Add("IFMODE", typeof(string));
                dt.Columns.Add("USERID", typeof(string));
                dt.Columns.Add("PCSGID", typeof(string));
                dt.Columns.Add("WIPNOTE", typeof(string));
                dt.Columns.Add("PROCID_TO", typeof(string));
                dt.Columns.Add("EQSGID_TO", typeof(string));
                dt.Columns.Add("RE_VD_TYPE_CODE", typeof(string));

                DataRow row = dt.NewRow();
                row["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                row["IFMODE"] = IFMODE.IFMODE_OFF;
                row["USERID"] = LoginInfo.USERID;
                row["PCSGID"] = "A"; // 조립
                row["WIPNOTE"] = "";

                //V/D재작업에서 V/D(STP후) 공정 추가 2022.04.19
                if (cboEquipmentSegment.SelectedValue.ToString().ToString().Equals(LoginInfo.CFG_AREA_ID + "MV2")) //2022.04.19
                {
                    row["PROCID_TO"] = Process.VD_LMN_AFTER_STP;
                }
                else
                {
                    if (Convert.ToString(cboVDEquipmentSegment.SelectedValue) == "M9CP1")// NFF Pilot 라인 VD 공정(A1000) 추가 2022.11.17 배현우
                        row["PROCID_TO"] = Process.VD_CDE_PRS;
                    else
                        row["PROCID_TO"] = Process.VD_LMN;
                }
                
                row["EQSGID_TO"] = Convert.ToString(cboVDEquipmentSegment.SelectedValue);

                if (LoginInfo.CFG_AREA_ID == "M9")
                    row["RE_VD_TYPE_CODE"] = "V";
                else
                    row["RE_VD_TYPE_CODE"] = "L";

                dt.Rows.Add(row);

                row = null;

                DataTable inLot = ds.Tables.Add("IN_LOT");
                inLot.Columns.Add("LOTID", typeof(string));
                inLot.Columns.Add("TREATTYPE", typeof(string));
                inLot.Columns.Add("PROCID", typeof(string));

                for (int i = 0; i < dgReworkLotInfo.GetRowCount(); i++)
                {
                    if (Util.NVC(DataTableConverter.GetValue(dgReworkLotInfo.Rows[i].DataItem, "CHK")).Equals("True") || Util.NVC(DataTableConverter.GetValue(dgReworkLotInfo.Rows[i].DataItem, "CHK")).Equals("1"))
                    {
                        row = inLot.NewRow();
                        row["LOTID"] = Util.NVC(DataTableConverter.GetValue(dgReworkLotInfo.Rows[i].DataItem, "LOTID"));
                        row["TREATTYPE"] = (Util.NVC(DataTableConverter.GetValue(dgReworkLotInfo.Rows[i].DataItem, "PROCID")).Equals(Process.VD_LMN) || Util.NVC(DataTableConverter.GetValue(dgReworkLotInfo.Rows[i].DataItem, "PROCID")).Equals(Process.VD_CDE_PRS)) ? "V" : "L";
                        row["PROCID"] = Util.NVC(DataTableConverter.GetValue(dgReworkLotInfo.Rows[i].DataItem, "PROCID"));

                        inLot.Rows.Add(row);
                    }

                }

                ShowLoadingIndicator();

                new ClientProxy().ExecuteService_Multi(sBizName, "INDATA, IN_LOT", null, (searchResult, searchException) =>
                {
                    try
                    {
                        if (searchException != null)
                        {
                            Util.MessageException(searchException);
                            //Util.AlertByBiz("BR_PRD_REG_MOVE_LOT_WAIT_VD", searchException.Message, searchException.ToString());
                            return;
                        }

                        Util.MessageInfo("SFU3073");
                        //Util.AlertInfo("VD재작업 처리 완료 \n VD예약을 다시 해주세요");
                        SearchData();

                    }
                    catch (Exception ex)
                    {
                        Util.MessageException(ex);
                        //LGC.GMES.MES.ControlsLibrary.MessageBox.ShowKeyEnter(ex.Message, ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
                    }
                    finally
                    {
                        HiddenLoadingIndicator();
                    }
                }, ds);


            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                // Util.Alert(ex.ToString());
            }
        }

        #endregion

        private void cboEquipmentSegment_SelectedValueChanged(object sender, C1.WPF.PropertyChangedEventArgs<object> e)
        {
            //선택된 라인의 REWORK 라인으로 변경
            cboVDEquipmentSegment.SelectedValue = cboEquipmentSegment.SelectedValue.ToString();

        }
    }


}
