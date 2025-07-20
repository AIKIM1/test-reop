/*************************************************************************************
 Created Date : 2019.10.25
      Creator : 강호운
   Decription : 특이작업 > 한계불량 관리
--------------------------------------------------------------------------------------
 [Change History]
  2019.10.25  강호운 : Initial Created.
  2020.04.27  강호운 CSR ID 48542 한계불량 라인정지 해제 인터락 추가 요청의 건 [요청번호] C20200402-000068
**************************************************************************************/

using System;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using C1.WPF;
using C1.WPF.DataGrid;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using LGC.GMES.MES.CMM001.Extensions;
using System.Windows.Threading;
using System.Windows.Media;

namespace LGC.GMES.MES.PACK001
{
    /// <summary>
    /// PACK001_049.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class PACK001_049 : UserControl
    {
        CommonCombo _combo = new CommonCombo();
        string sAREAID = LoginInfo.CFG_SHOP_ID;
        DataTable dtMain = new DataTable();
        private bool _manualCommit = false;
        private DataTable isCreateTable = new DataTable();
        string sBeforeUse_flag = null;
        public PACK001_049()
        {
            InitializeComponent();
            InitCombo();
        }
        public IFrameOperation FrameOperation
        {
            get;
            set;
        }

        private void InitCombo()
        {
            try
            {

                //KI, LEVEL COMBO 생성 , 초기값 설정

                
                _combo.SetCombo(cboLevel1,CommonCombo.ComboStatus.SELECT, sCase:"cboLimitDefectLevel1");
                cboLevel1.SelectedIndex = 0;

               

                _combo.SetCombo(cboLevel2,CommonCombo.ComboStatus.SELECT, sCase:"cboLimitDefectLevel2");
                cboLevel2.SelectedIndex = 0;


                _combo.SetCombo(cboLevel3,CommonCombo.ComboStatus.SELECT, sCase:"cboLimitDefectLevel3");
                cboLevel3.SelectedIndex = 0;
                




                // 콤보박스 자식,부모 관계

                /*
                C1ComboBox[] cboAction2Child = { cboLevel2 };
                string[] strFilter = { "PACK_UI_ACTION_CNTT_LEVEL1" };
                _combo.SetCombo(cboLevel1, CommonCombo.ComboStatus.NONE, cbChild: cboAction2Child, sFilter: strFilter, sCase: "COMMCODE");
               


                C1ComboBox[] cboAction1Parent = { cboLevel1 };
                C1ComboBox[] cboAction3Child = { cboLevel3 };
                string[] strDetail1Filter = { "PACK_UI_ACTION_CNTT_LEVEL2" };
                _combo.SetCombo(cboLevel2, CommonCombo.ComboStatus.NONE, cbParent: cboAction1Parent, cbChild: cboAction3Child, sFilter: strDetail1Filter, sCase: "COMMCODEATTR2");

           

                C1ComboBox[] cboAction2Parent = { cboLevel2 };
                string[] strDetail2Filter = { "PACK_UI_ACTION_CNTT_LEVEL3" };
                _combo.SetCombo(cboLevel3, CommonCombo.ComboStatus.NONE, cbParent: cboAction2Parent, sFilter: strDetail2Filter, sCase: "COMMCODEATTR2");
             */
                




                //tab1
                //동
                String[] sFilter = { sAREAID, Area_Type.PACK };
                C1ComboBox[] cboAreaChild = { cboEquipmentSegment };
                _combo.SetCombo(cboAreaByAreaType, CommonCombo.ComboStatus.ALL, cbChild: cboAreaChild, sFilter: sFilter, sCase: "cboAreaByAreaType");

                //라인
                C1ComboBox[] cboLineParent = { cboAreaByAreaType };
                C1ComboBox[] cboProcessChild = { cboProcess };
                _combo.SetCombo(cboEquipmentSegment, CommonCombo.ComboStatus.ALL, cbParent: cboLineParent, cbChild: cboProcessChild, sCase: "cboEquipmentSegment");

                //공정
                C1ComboBox[] cboProcessParent = { cboEquipmentSegment };
                string strCase = string.Empty;
                strCase = "cboProcessPack";
                _combo.SetCombo(cboProcess, CommonCombo.ComboStatus.ALL, cbParent: cboProcessParent, sCase: strCase);

                //tab2
                //동
                C1ComboBox[] cboAreaChild1 = { cboEquipmentSegment1 };
                _combo.SetCombo(cboAreaByAreaType1, CommonCombo.ComboStatus.ALL, cbChild: cboAreaChild1, sFilter: sFilter, sCase: "cboAreaByAreaType");

                //라인
                C1ComboBox[] cboLineParent1 = { cboAreaByAreaType1 };
                C1ComboBox[] cboProcessChild1 = { cboProcess1 };
                _combo.SetCombo(cboEquipmentSegment1, CommonCombo.ComboStatus.ALL, cbParent: cboLineParent1, cbChild: cboProcessChild1, sCase: "cboEquipmentSegment");

                //공정
                C1ComboBox[] cboProcessParent1 = { cboEquipmentSegment1 };
                string strCase1 = string.Empty;
                strCase1 = "cboProcessPack";
                _combo.SetCombo(cboProcess1, CommonCombo.ComboStatus.ALL, cbParent: cboProcessParent1, sCase: strCase1);

                SetLimtDfctFlag();

                SetProdType();
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private void SetLimtDfctFlag()
        {
            try
            {
                DataTable dt = new DataTable();
                dt.Columns.Add("CBO_NAME", typeof(string));
                dt.Columns.Add("CBO_CODE", typeof(string));

                DataRow dr_ = dt.NewRow();
                dr_["CBO_NAME"] = "ALL";
                dr_["CBO_CODE"] = "ALL";
                dt.Rows.Add(dr_);

                DataRow dr = dt.NewRow();
                dr["CBO_NAME"] = "Y : " + ObjectDic.Instance.GetObjectName("사용");
                dr["CBO_CODE"] = "Y";
                dt.Rows.Add(dr);

                DataRow dr1 = dt.NewRow();
                dr1["CBO_NAME"] = "N : " + ObjectDic.Instance.GetObjectName("사용 안함");
                dr1["CBO_CODE"] = "N";
                dt.Rows.Add(dr1);

                dt.AcceptChanges();

                cboLimtDfctFlag.ItemsSource = DataTableConverter.Convert(dt);
                cboLimtDfctFlag.SelectedIndex = 0; //default Y
                cboLimtDfctFlag1.ItemsSource = DataTableConverter.Convert(dt);
                cboLimtDfctFlag1.SelectedIndex = 0; //default Y
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private void SetProdType()
        {
            try
            {
                DataTable dt = new DataTable();
                dt.Columns.Add("CBO_NAME", typeof(string));
                dt.Columns.Add("CBO_CODE", typeof(string));

                DataRow dr_ = dt.NewRow();
                dr_["CBO_NAME"] = "ALL";
                dr_["CBO_CODE"] = "ALL";
                dt.Rows.Add(dr_);

                DataRow dr = dt.NewRow();
                dr["CBO_NAME"] = "CELL";
                dr["CBO_CODE"] = "CELL";
                dt.Rows.Add(dr);

                DataRow dr1 = dt.NewRow();
                dr1["CBO_NAME"] = "CMA";
                dr1["CBO_CODE"] = "CMA";
                dt.Rows.Add(dr1);

                DataRow dr2 = dt.NewRow();
                dr2["CBO_NAME"] = "BMA";
                dr2["CBO_CODE"] = "BMA";
                dt.Rows.Add(dr2);

                dt.AcceptChanges();

                cboProdType.ItemsSource = DataTableConverter.Convert(dt);
                cboProdType.SelectedIndex = 0; //default Y

                cboProdType1.ItemsSource = DataTableConverter.Convert(dt);
                cboProdType1.SelectedIndex = 0; //default Y
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        

        private void DoEvents()
        {
            Application.Current.Dispatcher.Invoke(DispatcherPriority.Background, new System.Threading.ThreadStart(delegate { }));
        }

        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            ShowLoadingIndicator();
            DoEvents();
            string[] sfilter = new string[] {
                cboAreaByAreaType.SelectedValue.ToString(),
                cboEquipmentSegment.SelectedValue.ToString(),
                cboProcess.SelectedValue.ToString(),
                cboLimtDfctFlag.SelectedValue.ToString(),
                cboProdType.SelectedValue.ToString()
            };
            loadingIndicator.Visibility = System.Windows.Visibility.Visible;
            dtMain = GetList(sfilter);
            Util.GridSetData(dgPrintList, dtMain, FrameOperation);
            loadingIndicator.Visibility = Visibility.Collapsed;
            HiddenLoadingIndicator();
        }

        private void btnSearch1_Click(object sender, RoutedEventArgs e)
        {
            if ((dtpDateTo.SelectedDateTime - dtpDateFrom.SelectedDateTime).TotalDays > 31)
            {
                //Util.AlertInfo("SFU2042", new object[] { "7" });   //기간은 {0}일 이내 입니다.
                Util.MessageValidation("SFU2042", "31");
                return;
            }

            DateTime dtEndTime = new DateTime(dtpDateTo.SelectedDateTime.Year, dtpDateTo.SelectedDateTime.Month, dtpDateTo.SelectedDateTime.Day);
            DateTime dtStartTime = new DateTime(dtpDateFrom.SelectedDateTime.Year, dtpDateFrom.SelectedDateTime.Month, dtpDateFrom.SelectedDateTime.Day);

            if (!(Math.Truncate(dtEndTime.Subtract(dtStartTime).TotalSeconds) >= 0))
            {
                //종료일자가 시작일자보다 빠릅니다.
                Util.MessageValidation("SFU1913");
                return;
            }

            ShowLoadingIndicator();
            DoEvents();
            string[] sfilter = new string[] {
                cboAreaByAreaType1.SelectedValue.ToString(),
                cboEquipmentSegment1.SelectedValue.ToString(),
                cboProcess1.SelectedValue.ToString(),
                cboLimtDfctFlag1.SelectedValue.ToString(),
                cboProdType1.SelectedValue.ToString(),
                //dtpDateFrom.SelectedDateTime.ToShortDateString(),
                //dtpDateTo.SelectedDateTime.ToShortDateString()
                dtpDateFrom.SelectedDateTime.ToString("yyyyMMdd"),
                dtpDateTo.SelectedDateTime.ToString("yyyyMMdd")
            };
            loadingIndicator.Visibility = System.Windows.Visibility.Visible;
            dtMain = GetList1(sfilter);
            Util.GridSetData(dgPrintList1, dtMain, FrameOperation);
            loadingIndicator.Visibility = Visibility.Collapsed;
            HiddenLoadingIndicator();
        }

        public DataTable GetList(string[] filter)
        {
            DataTable result = new DataTable();
            try
            {
                DataTable inTable = new DataTable();

                inTable.Columns.Add("AREAID", typeof(string));
                inTable.Columns.Add("EQSGID", typeof(string));
                inTable.Columns.Add("PROCID", typeof(string));
                inTable.Columns.Add("LIMIT_DFCT_FLAG", typeof(string));
                inTable.Columns.Add("S08", typeof(string));
                inTable.Columns.Add("LANGID", typeof(string));


                DataRow newRow = inTable.NewRow();
                newRow["AREAID"] = String.IsNullOrEmpty(filter[0]) || filter[0].Equals("ALL") ? null : filter[0]; ;
                newRow["EQSGID"] = String.IsNullOrEmpty(filter[1]) || filter[1].Equals("ALL") ? null : filter[1];
                newRow["PROCID"] = String.IsNullOrEmpty(filter[2]) || filter[2].Equals("ALL") ? null : filter[2];
                newRow["LIMIT_DFCT_FLAG"] = String.IsNullOrEmpty(filter[3]) || filter[3].Equals("ALL") ? null : filter[3];
                newRow["S08"] = String.IsNullOrEmpty(filter[4]) || filter[4].Equals("ALL") ? null : filter[4];
                newRow["LANGID"] = LoginInfo.LANGID;


                inTable.Rows.Add(newRow);

                result = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_TB_SFC_EQSG_PROC_LIMIT_DFCT", "RQSTDT", "RSLTDT", inTable);
            }
            catch (Exception ex)
            {
                loadingIndicator.Visibility = System.Windows.Visibility.Collapsed;
                ControlsLibrary.MessageBox.Show(ex.Message, null, "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
                Logger.Instance.WriteLine(Logger.OPERATION_C + "DA_PRD_SEL_TB_SFC_EQSG_PROC_LIMIT_DFCT", ex);
                return result;
            }
            finally
            {
                loadingIndicator.Visibility = System.Windows.Visibility.Collapsed;
                Logger.Instance.WriteLine(Logger.OPERATION_C + "DA_PRD_SEL_TB_SFC_EQSG_PROC_LIMIT_DFCT", Logger.MESSAGE_OPERATION_END);
            }
            return result;
        }

        public DataTable GetList1(string[] filter)
        {
            DataTable result = new DataTable();
            try
            {
                DataTable inTable = new DataTable();

                inTable.Columns.Add("AREAID", typeof(string));
                inTable.Columns.Add("EQSGID", typeof(string));
                inTable.Columns.Add("PROCID", typeof(string));
                inTable.Columns.Add("LIMIT_DFCT_FLAG", typeof(string));
                inTable.Columns.Add("S08", typeof(string));
                inTable.Columns.Add("LANGID", typeof(string));
                inTable.Columns.Add("STDTTM", typeof(string));
                inTable.Columns.Add("EDDTTM", typeof(string));

                DataRow newRow = inTable.NewRow();
                newRow["AREAID"] = String.IsNullOrEmpty(filter[0]) || filter[0].Equals("ALL") ? null : filter[0]; ;
                newRow["EQSGID"] = String.IsNullOrEmpty(filter[1]) || filter[1].Equals("ALL") ? null : filter[1];
                newRow["PROCID"] = String.IsNullOrEmpty(filter[2]) || filter[2].Equals("ALL") ? null : filter[2];
                newRow["LIMIT_DFCT_FLAG"] = String.IsNullOrEmpty(filter[3]) || filter[3].Equals("ALL") ? null : filter[3];
                newRow["S08"] = String.IsNullOrEmpty(filter[4]) || filter[4].Equals("ALL") ? null : filter[4];
                newRow["LANGID"] = LoginInfo.LANGID;
                //newRow["STDTTM"] = dtpDateFrom.SelectedDateTime.ToShortDateString();
                //newRow["EDDTTM"] = dtpDateTo.SelectedDateTime.ToShortDateString();
                newRow["STDTTM"] = dtpDateFrom.SelectedDateTime.ToString("yyyyMMdd");
                newRow["EDDTTM"] = dtpDateTo.SelectedDateTime.ToString("yyyyMMdd");

                inTable.Rows.Add(newRow);

                result = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_TB_SFC_EQSG_PROC_LIMIT_DFCT_HIST", "RQSTDT", "RSLTDT", inTable);
            }
            catch (Exception ex)
            {
                loadingIndicator.Visibility = System.Windows.Visibility.Collapsed;
                ControlsLibrary.MessageBox.Show(ex.Message, null, "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
                Logger.Instance.WriteLine(Logger.OPERATION_C + "DA_PRD_SEL_TB_SFC_EQSG_PROC_LIMIT_DFCT_HIST", ex);
                return result;
            }
            finally
            {
                loadingIndicator.Visibility = System.Windows.Visibility.Collapsed;
                Logger.Instance.WriteLine(Logger.OPERATION_C + "DA_PRD_SEL_TB_SFC_EQSG_PROC_LIMIT_DFCT_HIST", Logger.MESSAGE_OPERATION_END);
            }
            return result;
        }

        private void ShowLoadingIndicator()
        {
            if (loadingIndicator != null)
            {
                loadingIndicator.Visibility = Visibility.Visible;
            }
        }

        private void HiddenLoadingIndicator()
        {
            if (loadingIndicator != null)
            {
                loadingIndicator.Visibility = Visibility.Collapsed;
            }
        }




        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {

        }


        // KI, 체크박스 확인 VALIDATION 기능 추가
        private bool Validation()
        {
            if(dgPrintList.GetRowCount() == 0)
            {
                Util.MessageValidation("SFU3538");
                return false;
            }

            int CHK = 0;
            foreach (object current in dgPrintList.GetCurrentItems())
            {
                if (DataTableConverter.GetValue(current, "CHK").Equals("True"))
                {

                    CHK = 1;
                    if (Util.NVC(DataTableConverter.GetValue(current, "LIMIT_DFCT_FLAG")).Equals("N"))
                    {
                        Util.MessageValidation("SFU3748");
                        return false;
                    }
                }
            }
            if (CHK == 0)
            {
                Util.MessageValidation("SFU3538");
                return false;
            }


            return true;
        }

        private void btnSave(object sender, RoutedEventArgs e)
        {
            //// C20200402-000068 한계불량 라인정지 해제 인터락 추가 요청의 건


            // KI, 콤보 미선택시 저장 불가

            if (!Validation()) return;

            if (cboLevel1.SelectedValue.ToString().Substring(0, 2) != "DL" || cboLevel2.SelectedValue.ToString().Substring(0, 2) != "DL" || cboLevel3.SelectedValue.ToString().Substring(0, 2) != "DL")
            {
                Util.MessageValidation("SFU8348"); // 콤보박스 관련 메시지 MMD 추가 필요
                return;
            }

            /*
            if (string.IsNullOrEmpty(ACTION_CNTT.Text.Trim()) || ACTION_CNTT.Text.Trim().Length < 2)
            {
                Util.MessageValidation("SFU8204");
                return;
            }
            */

            
            //if (!Validation()) return;

            Util.MessageConfirm("SFU3533", (result) =>
            {
                if (result == MessageBoxResult.OK)
                {
                    Save();
                    DoEvents();
                    btnSearch_Click(null, null);
                }
            });
        }



        


        

        // KI, 이력탭 저장 기능 추가 , 저장 VALIDATION 기능 추가
        private void btnSave1(object sender, RoutedEventArgs e)
        {


            if (dgPrintList1.GetRowCount() == 0)
            {
                Util.MessageValidation("SFU3538");
                return;
            }

            int CHK = 0;
            foreach (object current in dgPrintList1.GetCurrentItems())
            {
                if (DataTableConverter.GetValue(current, "CHK").Equals("True"))
                {
                    CHK = 1;

                }
            }

            if (CHK == 0)
            {
                Util.MessageValidation("SFU3538"); 
                return;
            }




            Util.MessageConfirm("SFU3533", (result) =>
            {
                if (result == MessageBoxResult.OK)
                {
                    Save1();
                    DoEvents();
                    btnSearch1_Click(null, null);
                }
            });
          
        }




        private void Save()
        {
            ShowLoadingIndicator();
            DoEvents();

            try
            {
                string bizRuleName = "BR_PRD_REG_LIMIT_DEFECT_HIST"; // KI , biz에 indata 추가 필요

                isCreateTable = DataTableConverter.Convert(dgPrintList.GetCurrentItems());
                if (!CommonVerify.HasDataGridRow(dgPrintList)) return;

                this.dgPrintList.EndEdit();
                this.dgPrintList.EndEditRow(true);

                DataSet indataSet = new DataSet();


                // KI, INDATA에 컬럼 추가 
                DataTable inDataTable = indataSet.Tables.Add("INDATA");
                inDataTable.Columns.Add("AREAID", typeof(string));
                inDataTable.Columns.Add("EQSGID", typeof(string));
                inDataTable.Columns.Add("PROCID", typeof(string));
                inDataTable.Columns.Add("DFCT_RESNCODE", typeof(string));
                inDataTable.Columns.Add("ACTION_CNTT", typeof(string));
                inDataTable.Columns.Add("LIMIT_DFCT_FLAG", typeof(string));
                inDataTable.Columns.Add("USERID", typeof(string));
                inDataTable.Columns.Add("ACTION_CNTT_LEVEL1_CODE", typeof(string));//
                inDataTable.Columns.Add("ACTION_CNTT_LEVEL2_CODE", typeof(string));//
                inDataTable.Columns.Add("ACTION_CNTT_LEVEL3_CODE", typeof(string));//
                //inDataTable.Columns.Add("FPI_VAL_CHK_CNTT", typeof(string));
                //inDataTable.Columns.Add("LATEST", typeof(DateTime));


                foreach (object current in dgPrintList.GetCurrentItems())
                {
                    if (DataTableConverter.GetValue(current, "CHK").Equals("True"))
                    {
                        DataRow param = inDataTable.NewRow();

                        param["AREAID"] = DataTableConverter.GetValue(current, "AREAID");
                        param["EQSGID"] = DataTableConverter.GetValue(current, "EQSGID");
                        param["PROCID"] = DataTableConverter.GetValue(current, "PROCID");
                        param["DFCT_RESNCODE"] = DataTableConverter.GetValue(current, "DFCT_RESNCODE");
                        param["ACTION_CNTT"] = ACTION_CNTT.Text;
                        param["LIMIT_DFCT_FLAG"] = DataTableConverter.GetValue(current, "LIMIT_DFCT_FLAG");
                        param["USERID"] = LoginInfo.USERID;
                        param["ACTION_CNTT_LEVEL1_CODE"] = cboLevel1.SelectedValue.ToString(); //
                        param["ACTION_CNTT_LEVEL2_CODE"] = cboLevel2.SelectedValue.ToString(); // 
                        param["ACTION_CNTT_LEVEL3_CODE"] = cboLevel3.SelectedValue.ToString(); //
                        //param["FPI_VAL_CHK_CNTT"] = "EMPTY"; //
                        //param["LATEST"] = null; //
                        inDataTable.Rows.Add(param);
                    }
                }
                
                if (inDataTable.Rows.Count < 1)
                {
                    Util.MessageValidation("SFU3538");
                    return;
                }

                new ClientProxy().ExecuteServiceSync_Multi(bizRuleName, "INDATA", null, indataSet);
                Util.MessageInfo("SFU2056", inDataTable.Rows.Count);
                ACTION_CNTT.Text = String.Empty;
                Util.gridClear(dgPrintList);

                inDataTable = new DataTable();
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


        //KI, 이력탭 저장 추가

        private void Save1()
        {
            ShowLoadingIndicator();
            DoEvents();

            try
            {


                isCreateTable = DataTableConverter.Convert(dgPrintList1.GetCurrentItems());
                if (!CommonVerify.HasDataGridRow(dgPrintList1)) return;

                this.dgPrintList1.EndEdit();
                this.dgPrintList1.EndEditRow(true);

                DataSet indataSet = new DataSet();


                DataTable inDataTable = indataSet.Tables.Add("RQSTDT");
                inDataTable.Columns.Add("AREAID", typeof(string));
                inDataTable.Columns.Add("EQSGID", typeof(string));
                inDataTable.Columns.Add("PROCID", typeof(string));
                inDataTable.Columns.Add("DFCT_RESNCODE", typeof(string));
                inDataTable.Columns.Add("LOTID", typeof(string));
                //inDataTable.Columns.Add("ACTION_CNTT", typeof(string));
                inDataTable.Columns.Add("FPI_VAL_CHK_CNTT", typeof(string));
                inDataTable.Columns.Add("UPDUSER", typeof(string));






                //inDataTable.Columns.Add("UPDDTTM", typeof(string));
                //inDataTable.Columns.Add("LATEST", typeof(DateTime));


                foreach (object current in dgPrintList1.GetCurrentItems())
                {
                    if (DataTableConverter.GetValue(current, "CHK").Equals("True"))
                    {
                        DataRow param = inDataTable.NewRow();
                        param["AREAID"] = DataTableConverter.GetValue(current, "AREAID");
                        param["EQSGID"] = DataTableConverter.GetValue(current, "EQSGID");
                        param["PROCID"] = DataTableConverter.GetValue(current, "PROCID");
                        param["DFCT_RESNCODE"] = DataTableConverter.GetValue(current, "DFCT_RESNCODE");
                        param["LOTID"] = DataTableConverter.GetValue(current, "LOTID");
                        //param["ACTION_CNTT"] = DataTableConverter.GetValue(current, "ACTION_CNTT");
                        param["FPI_VAL_CHK_CNTT"] = DataTableConverter.GetValue(current, "FPI_VAL_CHK_CNTT");
                        param["UPDUSER"] = LoginInfo.USERID;
                        //param["UPDDTTM"] = DataTableConverter.GetValue(current, "UPDDTTM");
                        //param["LATEST"] = DataTableConverter.GetValue(current, "LATEST");

                        inDataTable.Rows.Add(param);
                    }
                }

                if (inDataTable.Rows.Count < 1)
                {
                    Util.MessageValidation("SFU3538");
                    return;
                }

                new ClientProxy().ExecuteServiceSync_Multi("DA_PRD_UPD_FPI_VAL_CHK", "RQSTDT", null, indataSet);
                Util.MessageInfo("SFU2056", inDataTable.Rows.Count);

                Util.gridClear(dgPrintList1);

                inDataTable = new DataTable();
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



        private void dgPrintList_LoadedCellPresenter(object sender, DataGridCellEventArgs e)
        {

        }

        private void dgPrintList_BeginningEdit(object sender, C1.WPF.DataGrid.DataGridBeginningEditEventArgs e)
        {

        }

        private void cboLimtDfctFlag_SelectedItemChanged(object sender, PropertyChangedEventArgs<object> e)
        {

        }

        private void cboProdType_SelectedItemChanged(object sender, PropertyChangedEventArgs<object> e)
        {

        }

        private void cboProdType1_SelectedItemChanged(object sender, PropertyChangedEventArgs<object> e)
        {

        }

        private void cboLimtDfctFlag1_SelectedItemChanged(object sender, PropertyChangedEventArgs<object> e)
        {

        }

 
    }
}
