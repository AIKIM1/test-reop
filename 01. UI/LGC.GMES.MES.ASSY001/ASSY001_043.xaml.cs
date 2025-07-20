/*************************************************************************************
 Created Date : 2018-11-20
      Creator : 이동우
   Decription : 공정진척(조립)_V/D 현황 (R to R) //
--------------------------------------------------------------------------------------
 [Change History]
   2019.03.05  오화백 : RF_ID 체크 로직 추가(RF_ID일 경우 리스트에 CST정보 보여줌)

**************************************************************************************/

using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using C1.WPF;
using C1.WPF.DataGrid;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.CMM001.Extensions;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;

namespace LGC.GMES.MES.ASSY001
{
    public partial class ASSY001_043 : UserControl, IWorkArea
    {
        #region Declaration & Constructor
        List<ASSY001_043_EQPTWIN> list;
        int listCtn = -1;
    
        CommonCombo combo = new CommonCombo();
        Util _Util = new Util();
        IFrameOperation iFO = null;
        string EQPT_VD_QA_COND = null;
        string QA_TYPE = "N";

        //2019.03.05 오화백 RF_ID 투입부, 배출부 RFID  
        private string _LDR_LOT_IDENT_BAS_CODE = string.Empty; //투입부
        private string _UNLDR_LOT_IDENT_BAS_CODE = string.Empty; //배출부

        public IFrameOperation FrameOperation { get; set; }

        bool isRefresh;
        public bool REFRESH
        {
            get
            {
                return isRefresh;
            }
            set
            {
                isRefresh = value;

                if (isRefresh)
                    SearchData();
            }
        }

        public ASSY001_043()
        {
            InitializeComponent();
            Loaded += UserControl_Loaded;
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            Loaded -= UserControl_Loaded;
            ApplyPermissions();
            initcombo();

            list = new List<ASSY001_043_EQPTWIN>();
            for (int i = 0; i < 10; i++)
            {
                list.Add(new ASSY001_043_EQPTWIN());
            }

            listCtn = 10;

            try
            {
                DataTable dt = new DataTable();
                //dt.Columns.Add("SHOPID", typeof(string));
                dt.Columns.Add("AREAID", typeof(string));

                DataRow dr = dt.NewRow();
                //dr["SHOPID"] = LoginInfo.CFG_SHOP_ID;
                dr["AREAID"] = Util.NVC(cboVDArea.SelectedValue);
                dt.Rows.Add(dr);

                //DataTable result = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_SHOPATTR", "RQSTDT", "RSLTDT", dt);
                DataTable result = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_MMD_AREA_VD_QA_AUTO_FLAG", "RQSTDT", "RSLTDT", dt);
                if (result.Rows.Count != 0)
                {
                    //QA_TYPE = Convert.ToString(result.Rows[0]["VD_QA_AUTO_JUDG_FLAG"]); 
                    QA_TYPE = Convert.ToString(result.Rows[0]["AUTO_VD_FLAG"]);
                }

            } catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        
        }
        #endregion

        public void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            if (sender == null)
                return;

            SearchData();
        }

        //private void rdoRun_Click(object sender, RoutedEventArgs e)
        //{
        //    //if (sender == null)
        //    //    return;

        //    //SearchData();
        //}

        //private void rdoTwo_Click(object sender, RoutedEventArgs e)
        //{
        //    //if (sender == null)
        //    //    return;
        //    //SearchData();
        //}

        private void cboVDArea_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            string[] sFilter = { Process.VD_LMN, Convert.ToString(cboVDArea.SelectedValue) };
            combo.SetCombo(cboVDEquipmentSegment, CommonCombo.ComboStatus.NONE, sFilter: sFilter);

            //string[] sFilter3 = { Process.VD_LMN, Convert.ToString(cboVDEquipmentSegment.SelectedValue) };
            //combo.SetCombo(cboVDFloor, CommonCombo.ComboStatus.ALL, sFilter: sFilter3);
        }

        private void cboVDEquipmentSegment_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            //string[] sFilter = { Process.VD_LMN, Convert.ToString(cboVDFloor.SelectedValue) };
            //combo.SetCombo(cboVDFloor, CommonCombo.ComboStatus.ALL, sFilter: sFilter);
        }

        #region[Method]
        private void initcombo()
        {
            string[] sFilter4 = {  null, LoginInfo.CFG_AREA_ID };
            combo.SetCombo(cboVDArea, CommonCombo.ComboStatus.NONE, sFilter: sFilter4);

            string[] sFilter = { Process.VD_LMN, Convert.ToString(cboVDArea.SelectedValue) };
            combo.SetCombo(cboVDEquipmentSegment, CommonCombo.ComboStatus.NONE, sFilter: sFilter);

            string[] sFilter2 = { "ELEC_TYPE" };
            combo.SetCombo(cboEquipmentElec, CommonCombo.ComboStatus.ALL, sFilter: sFilter2, sCase: "COMMCODE");

            //string[] sFilter3 = { Process.VD_LMN, Convert.ToString(cboVDEquipmentSegment.SelectedValue) };
            //combo.SetCombo(cboVDFloor, CommonCombo.ComboStatus.ALL, sFilter: sFilter3);
        }

        private void SearchData()
        {

            GetLotIdentBasCode(); //2019.03.05 오화백 RF_ID 투입부, 배출부 여부 

            DataTable result = null;
            result = rdoFinish.IsChecked == false ? SetVDoperation() : SetVDFinish();

            #region [Lot ID별로 각각 QA검사 선택 하기 위한 CheckBox, 2017.08.13 Add By Kim Joonphil]
            if(rdoFinish.IsChecked == true && LoginInfo.CFG_AREA_ID == "A6")
            {
                chkLOTQA.Visibility = Visibility.Visible;
            }
            #endregion

            DataTable data = new DataTable();
            data.Columns.Add("LANGID", typeof(string));
            data.Columns.Add("PROCID", typeof(string));
            data.Columns.Add("EQSGID", typeof(string));
            data.Columns.Add("ELEC", typeof(string));
            data.Columns.Add("FLOOR", typeof(string));

            DataRow row = data.NewRow();
            row["LANGID"] = LoginInfo.LANGID;
            row["PROCID"] = Process.VD_LMN;
            row["EQSGID"] = Convert.ToString(cboVDEquipmentSegment.SelectedValue);
            row["ELEC"] = Convert.ToString(cboEquipmentElec.SelectedValue);
            row["FLOOR"] = rdoAll.IsChecked == true ? null : rdo2Floor.IsChecked == true ? "2" : "3"; //Convert.ToString(cboVDFloor.SelectedValue).Equals("") ? null : Convert.ToString(cboVDFloor.SelectedValue);
            data.Rows.Add(row);

            loadingIndicator.Visibility = Visibility.Visible;
            try
            {
                new ClientProxy().ExecuteService("DA_PRD_SEL_EQPT_VD", "RQST", "RSLT", data, (bizResult, bizException) =>
                {
                    if (listCtn < bizResult.Rows.Count)
                    {
                        for (int i = 0; i < bizResult.Rows.Count - listCtn; i++)
                        {
                            list.Add(new ASSY001_043_EQPTWIN());
                        }

                        listCtn = list.Count();
                    }

                    System.Diagnostics.Debug.WriteLine("Before clear: {0} Bytes.", GC.GetTotalMemory(false));
                    
                    ClearData();


                    if (bizException != null)
                    {
                        Util.MessageException(bizException);
                        return;
                    }

                    if (bizResult.Rows.Count == 0)
                    {
                        FrameOperation.PrintFrameMessage("[" + System.DateTime.Now.ToString("hh:mm:ss") + "]" + MessageDic.Instance.GetMessage("SFU1905")); //SFU1905 //조회된 Data가 없습니다.
                        loadingIndicator.Visibility = Visibility.Collapsed;
                        return;
                    }

                    for (int i = 0; i < bizResult.Rows.Count; i++)
                    {

                        if (rdoRun.IsChecked == true)
                        {
                            list[i].dgRunLot.ItemsSource = null;

                            list[i].dgRunLot.ItemsSource = !result.Select("EQPTID = '" + bizResult.Rows[i]["EQPTID"] + "'").Any() ? null : DataTableConverter.Convert(result.Select("EQPTID = '" + bizResult.Rows[i]["EQPTID"] + "'").CopyToDataTable());
                            list[i].dcRework.Visibility = Visibility.Collapsed;

                            //2019.03.05 오화백 RF_ID 투입부, 배출부 여부 
                            list[i]._LDR_LOT_IDENT_BAS_CODE = _LDR_LOT_IDENT_BAS_CODE;
                            list[i]._UNLDR_LOT_IDENT_BAS_CODE = _UNLDR_LOT_IDENT_BAS_CODE;

                            if (list[i].dgRunLot.GetRowCount() != 0 && DataTableConverter.Convert(list[i].dgRunLot.ItemsSource).Select("REWORK = 'Y'").Count() != 0)
                               list[i].dcRework.Visibility = Visibility.Visible;

                            DataTable dtRunLot = DataTableConverter.Convert(list[i].dgRunLot.ItemsSource);

                            if (dtRunLot.Columns.Contains("CHK"))
                            {
                                dtRunLot.Columns.Remove("CHK");
                                dtRunLot = _Util.gridCheckColumnAdd(dtRunLot, "CHK");
                            }
                            Util.GridSetData(list[i].dgRunLot, dtRunLot, FrameOperation, true);

                        }
                        else if (rdoFinish.IsChecked == true)
                        {
                            DataTable tmp = null;
                            list[i].dgFinishLot.ItemsSource = null;
                            list[i].dgFinishLot.ItemsSource = !result.Select("EQPTID = '" + bizResult.Rows[i]["EQPTID"] + "'").Any() ? null : DataTableConverter.Convert(result.Select("EQPTID = '" + bizResult.Rows[i]["EQPTID"] + "'").CopyToDataTable());
                            //2019.03.05 오화백 RF_ID 투입부, 배출부 여부 
                            list[i]._LDR_LOT_IDENT_BAS_CODE = _LDR_LOT_IDENT_BAS_CODE;
                            list[i]._UNLDR_LOT_IDENT_BAS_CODE = _UNLDR_LOT_IDENT_BAS_CODE;


                            if (bizResult.Rows[i]["VD_QA_INSP_COND_CODE"].Equals("VD_QA_INSP_RULE_02"))//대LOT기준
                            {

                                list[i].dgcLotId.Visibility = Visibility.Collapsed;
                                list[i].dgcLotCount.Visibility = Visibility.Visible;
                                tmp = !result.Select("EQPTID = '" + bizResult.Rows[i]["EQPTID"] + "'").Any() ? null : result.Select("EQPTID = '" + bizResult.Rows[i]["EQPTID"] + "'").CopyToDataTable();

                                if (tmp != null)
                                {
                                    var a = tmp.AsEnumerable().GroupBy(x => new
                                    {
                                        EQPT_BTCH_WRK_NO = x.Field<string>("EQPT_BTCH_WRK_NO"),
                                        LOTID_RT = x.Field<string>("LOTID_RT"),
                                        JUDG_VALUE = x.Field<string>("JUDG_VALUE")
                                    }).Select(g => new
                                    {
                                        EQPT_BTCH_WRK_NO = g.Key.EQPT_BTCH_WRK_NO,
                                        LOTID_RT = g.Key.LOTID_RT,
                                        JUDG_VALUE = g.Key.JUDG_VALUE,
                                        CHK = g.Max(x => x.Field<bool>("CHK")),
                                        LOTID = g.Max(x => x.Field<string>("LOTID")),
                                        EQPTID = g.Max(x => x.Field<string>("EQPTID")),
                                        PRODID = g.Max(x => x.Field<string>("PRODID")),
                                        PRJT_NAME = g.Max(x => x.Field<string>("PRJT_NAME")),
                                        JUDG_NAME = g.Max(x => x.Field<string>("JUDG_NAME")),
                                        WIPDTTM_ED = g.Max(x => x.Field<string>("WIPDTTM_ED")),
                                        ELEC = g.Max(x => x.Field<string>("ELEC")),
                                        VD_QA_INSP_COND_CODE = g.Max(x => x.Field<string>("VD_QA_INSP_COND_CODE")),
                                        REWORKCNT = g.Max(x => x.Field<decimal>("REWORKCNT")),
                                        COUNT = g.Count(),
                                        QA_SMPL_DTTM = g.Max(x => x.Field<string>("QA_SMPL_DTTM")), // 05.02
                                        NT_LOTID = g.Max(x => x.Field<string>("NT_LOTID")), // 2019.05.21
                                        NT_STAT = g.Max(x => x.Field<string>("NT_STAT")) // 2019.05.21
                                    });

                                    DataTable dt = new DataTable();
                                    dt.Columns.Add("CHK", typeof(bool)); //
                                    dt.Columns.Add("LOTID", typeof(string)); //
                                    dt.Columns.Add("LOTID_RT", typeof(string));//
                                    dt.Columns.Add("JUDG_VALUE", typeof(string));
                                    dt.Columns.Add("JUDG_NAME", typeof(string));
                                    dt.Columns.Add("EQPT_BTCH_WRK_NO", typeof(string));//
                                    dt.Columns.Add("WIPDTTM_ED", typeof(string));
                                    dt.Columns.Add("PRODID", typeof(string));//
                                    dt.Columns.Add("PRJT_NAME", typeof(string));//
                                    dt.Columns.Add("ELEC", typeof(string));
                                    dt.Columns.Add("VD_QA_INSP_COND_CODE", typeof(string));
                                    dt.Columns.Add("REWORKCNT", typeof(decimal));
                                    dt.Columns.Add("COUNT", typeof(decimal));
                                    dt.Columns.Add("QA_SMPL_DTTM", typeof(string)); // 05.02
                                    dt.Columns.Add("NT_LOTID", typeof(string)); // 2019.05.21
                                    dt.Columns.Add("NT_STAT", typeof(string)); // 2019.05.21

                                    DataRow dtRow = null;

                                    foreach (var j in a)
                                    {
                                        dtRow = dt.NewRow();
                                        dtRow["CHK"] = j.CHK;
                                        dtRow["LOTID"] = j.LOTID;
                                        dtRow["LOTID_RT"] = j.LOTID_RT;
                                        dtRow["JUDG_VALUE"] = j.JUDG_VALUE;
                                        dtRow["JUDG_NAME"] = j.JUDG_NAME;
                                        dtRow["EQPT_BTCH_WRK_NO"] = j.EQPT_BTCH_WRK_NO;
                                        dtRow["WIPDTTM_ED"] = j.WIPDTTM_ED;
                                        dtRow["PRODID"] = j.PRODID;
                                        dtRow["PRJT_NAME"] = j.PRJT_NAME;
                                        dtRow["ELEC"] = j.ELEC;
                                        dtRow["VD_QA_INSP_COND_CODE"] = j.VD_QA_INSP_COND_CODE;
                                        dtRow["REWORKCNT"] = j.REWORKCNT;
                                        dtRow["COUNT"] = j.COUNT;
                                        dtRow["QA_SMPL_DTTM"] = j.QA_SMPL_DTTM; // 05.02
                                        dtRow["NT_LOTID"] = j.NT_LOTID; // 2019.05.21
                                        dtRow["NT_STAT"] = j.NT_LOTID; // 2019.05.21
                                        dt.Rows.Add(dtRow);
                                    }


                                   list[i].dgFinishLot.ItemsSource = DataTableConverter.Convert(dt);

                                    Util.GridSetData(list[i].dgFinishLot, DataTableConverter.Convert(list[i].dgFinishLot.ItemsSource), null,true);
                                }

                            }
                        }

                       
                        GetEqpt(i, bizResult);

                    }

                    loadingIndicator.Visibility = Visibility.Collapsed;
                    SetEqptWindow(bizResult, rdoTwo.IsChecked == true ? 2 : 3);
                    FrameOperation.PrintFrameMessage(bizResult.Rows.Count + MessageDic.Instance.GetMessage("건"));

                    System.Diagnostics.Debug.WriteLine("After clear: {0} Bytes.", GC.GetTotalMemory(false));

                });

            }
            catch (Exception ex)
            {
                loadingIndicator.Visibility = Visibility.Collapsed;
                Util.MessageException(ex);
            }

          
        }

        private DataTable SetVDFinish()
        {
            try
            {
                DataSet data = new DataSet();
                DataTable dt = data.Tables.Add("INDATA");

                dt.Columns.Add("LANGID", typeof(string));
                dt.Columns.Add("SHOPID", typeof(string));
                dt.Columns.Add("PROCID", typeof(string));
                dt.Columns.Add("AREAID", typeof(string));

                DataRow row = dt.NewRow();
                row["LANGID"] = LoginInfo.LANGID;
                row["SHOPID"] = LoginInfo.CFG_SHOP_ID;
                row["PROCID"] = Process.VD_LMN;
                row["AREAID"] = Util.NVC(cboVDArea.SelectedValue);
                dt.Rows.Add(row);


                DataSet ds = new ClientProxy().ExecuteServiceSync_Multi("BR_PRD_GET_VD_FINISH_VD_PANCAKE", "RQSTDT", "OUTDATA", data);
                DataTable result = ds.Tables["OUTDATA"];

                return result;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return null;
            }
        }

        private DataTable SetVDoperation()
        {
            try
            {

                DataSet data = new DataSet();
                DataTable dt =  data.Tables.Add("INDATA");
                dt.Columns.Add("LANGID", typeof(string));
                dt.Columns.Add("PROCID", typeof(string));
                dt.Columns.Add("SHOPID", typeof(string));
                dt.Columns.Add("AREAID", typeof(string));

                DataRow row = dt.NewRow();
                row["LANGID"] = LoginInfo.LANGID;
                row["PROCID"] = Process.VD_LMN;
                row["SHOPID"] = LoginInfo.CFG_SHOP_ID;
                row["AREAID"] = Util.NVC(cboVDArea.SelectedValue);
                dt.Rows.Add(row);

                DataSet ds = new ClientProxy().ExecuteServiceSync_Multi("BR_PRD_GET_VD_OPERATION_VD_PANCAKE", "INDATA", "OUTDATA", data);
                DataTable result = ds.Tables["OUTDATA"];

                return result;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return null;
            }
        }

        private void ClearData()
        {
            if (list == null) return;
            if (list.Count == 0) return;

            for (int i = 0; i < list.Count; i++)
            {
                list[i].ClearData();
            }

            for (int i = grdEqpt.Children.Count - 1; i >= 0; i--)
            {
                ((Grid)(grdEqpt.Children[i])).Children.Remove(list[i]);

                grdEqpt.Children.Remove(((Grid)grdEqpt.Children[i]));
            }


            grdEqpt.Children.Clear();
            grdEqpt.ColumnDefinitions.Clear();
            grdEqpt.RowDefinitions.Clear();
           
        }

        private void ApplyPermissions()
        {
            List<Button> listAuth = new List<Button>();
            Util.pageAuth(listAuth, FrameOperation.AUTHORITY);
        }

        private void SetEqptWindow(DataTable bizResult, int rowCount)//WeakReference _list
        {
            int num = 0;

            for (int i = 0; i < rowCount; i++)
            {
                var rowDef = new RowDefinition { Height = rowCount == 2 ? new GridLength(360) : new GridLength(250) };
                grdEqpt.RowDefinitions.Add(rowDef);


                for (int j = 0; j < Math.Ceiling((double)bizResult.Rows.Count / rowCount); j++)
                {

                    if (i == 0)
                    {
                        var colDef = new ColumnDefinition
                        {
                            MinWidth = 400,
                            Width = rowCount == 2 ? new GridLength(500) : new GridLength(200)
                        };
                        grdEqpt.ColumnDefinitions.Add(colDef);
                    }

                    var grid = new Grid
                    {
                        Name = "gr0" + num,
                        Margin = i == 0 ? new Thickness(0, 8, 8, 8) : new Thickness(0, 0, 8, 8)
                    };
                    grid.SetValue(Grid.RowProperty, i);
                    grid.SetValue(Grid.ColumnProperty, j);

                  //  list[num].FrameOperation = FrameOperation;
                    grid.Children.Add(list[num]);

                    grdEqpt.Children.Add(grid);

                 
                    num++;

                    if (bizResult.Rows.Count == num)
                    { 
                        return;
                    }


                }
            }
        }

        private void GetEqpt(int i, DataTable dt)
        {

            list[i].FrameOperation = FrameOperation;

            object[] parameters = new object[8];
            parameters[0] = rdoRun.IsChecked != true;
            parameters[1] = dt.Rows[i]["PRDT_CLSS_CHK_FLAG"].GetString();
            parameters[2] = dt.Rows[i]["PRDT_CLSS_CODE"].GetString();
            parameters[3] = dt.Rows[i]["EQPTNAME"].GetString();
            parameters[4] = dt.Rows[i]["EQPTID"].GetString();
            parameters[5] = cboVDEquipmentSegment.SelectedValue.GetString();
            parameters[6] = dt.Rows[i]["VD_QA_INSP_COND_CODE"].GetString();
            parameters[7] = QA_TYPE; //QMS 인터페이스 여부

            C1WindowExtension.SetParameters(list[i], parameters);


            list[i].SetValue();

        }


        #endregion

        /// <summary>
        /// LotID별로 검사 선택. 폴란드 사용 2017.08.13 Add By Kim Joonphil
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void chkLOTQA_Checked(object sender, RoutedEventArgs e)
        {
            if (list == null) return;
            if (list.Count == 0) return;
            
            int iCnt = list.Count() ;
            for (int i = 0; i < list.Count; i++)
            {

                list[i].FrameOperation = FrameOperation;
                object[] parameters = new object[7];
                parameters[0] = chkLOTQA.IsChecked.Value;
                C1WindowExtension.SetParameters(list[i], parameters);
                list[i].SetLotQAInsp();
            }
        }


        /// <summary>
        /// 2019.03.05 오화백 RF_ID 투입부, 배출부 여부 
        /// </summary>
        private void GetLotIdentBasCode()
        {
            try
            {
                _LDR_LOT_IDENT_BAS_CODE = "";
                _UNLDR_LOT_IDENT_BAS_CODE = "";

                //ShowLoadingIndicator();

                DataTable inTable = new DataTable();
                inTable.Columns.Add("LANGID", typeof(string));
                inTable.Columns.Add("PROCID", typeof(string));
                inTable.Columns.Add("EQSGID", typeof(string));

                DataRow dtRow = inTable.NewRow();
                dtRow["LANGID"] = LoginInfo.LANGID;
                dtRow["PROCID"] = Process.VD_LMN;
                dtRow["EQSGID"] = Util.NVC(cboVDEquipmentSegment.SelectedValue);

                inTable.Rows.Add(dtRow);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_EQP_SEL_LOT_IDENT_BAS_CODE", "INDATA", "OUTDATA", inTable);

                if (dtRslt != null && dtRslt.Rows.Count > 0)
                {
                    if (dtRslt.Columns.Contains("LDR_LOT_IDENT_BAS_CODE"))
                        _LDR_LOT_IDENT_BAS_CODE = Util.NVC(dtRslt.Rows[0]["LDR_LOT_IDENT_BAS_CODE"]);

                    if (dtRslt.Columns.Contains("UNLDR_LOT_IDENT_BAS_CODE"))
                        _UNLDR_LOT_IDENT_BAS_CODE = Util.NVC(dtRslt.Rows[0]["UNLDR_LOT_IDENT_BAS_CODE"]);
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
            finally
            {
                //HiddenLoadingIndicator();
            }
        }

    }
}