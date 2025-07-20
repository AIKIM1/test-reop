/*************************************************************************************
 Created Date : 2016.06.16
      Creator : 
   Decription : 
--------------------------------------------------------------------------------------
 [Change History]
  2016.06.16  DEVELOPER : Initial Created.
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

namespace LGC.GMES.MES.ELEC001
{
    public partial class ELEC001_029 : UserControl, IWorkArea
    {
        #region Declaration & Constructor
       // List<ASSY001_024_EQPTWIN> list;
        int listCtn = -1;
    
        CommonCombo combo = new CommonCombo();
        Util _Util = new Util();
        IFrameOperation iFO = null;
        string EQPT_VD_QA_COND = null;

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

        public ELEC001_029()
        {
            InitializeComponent();
            Loaded += UserControl_Loaded;
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            //Loaded -= UserControl_Loaded;
            //ApplyPermissions();
            //initcombo();

            //list = new List<ASSY001_024_EQPTWIN>();
            //for (int i = 0; i < 10; i++)
            //{
            //    list.Add(new ASSY001_024_EQPTWIN());
            //}

            //listCtn = 10;
        }
        #endregion

        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            if (sender == null)
                return;

            SearchData();
        }

        private void rdoRun_Click(object sender, RoutedEventArgs e)
        {
            if (sender == null)
                return;

            SearchData();
        }

        private void rdoTwo_Click(object sender, RoutedEventArgs e)
        {
            if (sender == null)
                return;
            SearchData();
        }

        private void cboVDArea_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            
            string[] sFilter = {  "VD" , Convert.ToString(cboVDArea.SelectedValue) };
            combo.SetCombo(cboVDEquipmentSegment, CommonCombo.ComboStatus.NONE, sFilter: sFilter, sCase: "VDEquipmentSegmentQA");

            string[] sFilter3 = { Convert.ToString(cboVDProcess.SelectedValue), Convert.ToString(cboVDEquipmentSegment.SelectedValue) };
            combo.SetCombo(cboVDFloor, CommonCombo.ComboStatus.ALL, sFilter: sFilter3);
        }

        private void cboVDEquipmentSegment_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            string[] sFilter = { Convert.ToString(cboVDProcess.SelectedValue), Convert.ToString(cboVDFloor.SelectedValue) };
            combo.SetCombo(cboVDFloor, CommonCombo.ComboStatus.ALL, sFilter: sFilter);
        }

        #region[Method]
        private void initcombo()
        {
            string[] sFilter4 = { LoginInfo.CFG_AREA_ID };
            combo.SetCombo(cboVDArea, CommonCombo.ComboStatus.NONE, sFilter: sFilter4);

            string[] sFilter = { "VD", Convert.ToString(cboVDArea.SelectedValue) };
            combo.SetCombo(cboVDEquipmentSegment, CommonCombo.ComboStatus.NONE, sFilter: sFilter, sCase: "VDEquipmentSegmentQA");

            string[] sFilter5 = { Convert.ToString(cboVDEquipmentSegment.SelectedValue) };
            combo.SetCombo(cboVDProcess, CommonCombo.ComboStatus.NONE, sFilter: sFilter5);

            // string[] sFilter = { Process.VD_LMN, Convert.ToString(cboVDArea.SelectedValue) };
            // combo.SetCombo(cboVDEquipmentSegment, CommonCombo.ComboStatus.NONE, sFilter: sFilter);

            string[] sFilter2 = { "ELEC_TYPE" };
            combo.SetCombo(cboEquipmentElec, CommonCombo.ComboStatus.ALL, sFilter: sFilter2, sCase: "COMMCODE");

            string[] sFilter3 = { Convert.ToString(cboVDProcess.SelectedValue) , Convert.ToString(cboVDEquipmentSegment.SelectedValue) };
            combo.SetCombo(cboVDFloor, CommonCombo.ComboStatus.ALL, sFilter: sFilter3);
        }

        private void SearchData()
        {
            DataTable result = null;
            result = rdoFinish.IsChecked == false ? SetVDoperation() : SetVDFinish();

            DataTable data = new DataTable();
            data.Columns.Add("LANGID", typeof(string));
            data.Columns.Add("PROCID", typeof(string));
            data.Columns.Add("EQSGID", typeof(string));
            data.Columns.Add("ELEC", typeof(string));
            data.Columns.Add("FLOOR", typeof(string));

            DataRow row = data.NewRow();
            row["LANGID"] = LoginInfo.LANGID;
            row["PROCID"] = Convert.ToString(cboVDProcess.SelectedValue);
            row["EQSGID"] = Convert.ToString(cboVDEquipmentSegment.SelectedValue);
            row["ELEC"] = Convert.ToString(cboEquipmentElec.SelectedValue);
            row["FLOOR"] = Convert.ToString(cboVDFloor.SelectedValue).Equals("") ? null : Convert.ToString(cboVDFloor.SelectedValue);
            data.Rows.Add(row);

            loadingIndicator.Visibility = Visibility.Visible;
          //  try
           // {
            //    new ClientProxy().ExecuteService("DA_PRD_SEL_EQPT_VD", "RQST", "RSLT", data, (bizResult, bizException) =>
            //    {
                   

            //        if (listCtn < bizResult.Rows.Count)
            //        {
            //            for (int i = 0; i < bizResult.Rows.Count - listCtn; i++)
            //            {
            //             //   list.Add(new ASSY001_024_EQPTWIN());
            //            }

            //            //listCtn = list.Count();
            //        }

            //        System.Diagnostics.Debug.WriteLine("Before clear: {0} Bytes.", GC.GetTotalMemory(false));
                    
            //        ClearData();


            //        if (bizException != null)
            //        {
            //            Util.MessageException(bizException);
            //            return;
            //        }

            //        if (bizResult.Rows.Count == 0)
            //        {
            //            FrameOperation.PrintFrameMessage("[" + System.DateTime.Now.ToString("hh:mm:ss") + "]" + MessageDic.Instance.GetMessage("SFU1905")); //SFU1905 //조회된 Data가 없습니다.
            //            loadingIndicator.Visibility = Visibility.Collapsed;
            //            return;
            //        }

            //        for (int i = 0; i < bizResult.Rows.Count; i++)
            //        {

            //            //if (rdoRun.IsChecked == true)
            //            //{
            //            //    list[i].dgRunLot.ItemsSource = null;

            //            //    list[i].dgRunLot.ItemsSource = !result.Select("EQPTID = '" + bizResult.Rows[i]["EQPTID"] + "'").Any() ? null : DataTableConverter.Convert(result.Select("EQPTID = '" + bizResult.Rows[i]["EQPTID"] + "'").CopyToDataTable());
            //            //    list[i].dcRework.Visibility = Visibility.Collapsed;



            //            //    if (list[i].dgRunLot.GetRowCount() != 0 && DataTableConverter.Convert(list[i].dgRunLot.ItemsSource).Select("REWORK = 'Y'").Count() != 0)
            //            //       list[i].dcRework.Visibility = Visibility.Visible;

            //            //    Util.GridSetData(list[i].dgRunLot, DataTableConverter.Convert(list[i].dgRunLot.ItemsSource), null, true);

            //            //}
            //            //else if (rdoFinish.IsChecked == true)
            //            //{
            //            //    DataTable tmp = null;
            //            //    list[i].dgFinishLot.ItemsSource = null;
            //            //    list[i].dgFinishLot.ItemsSource = !result.Select("EQPTID = '" + bizResult.Rows[i]["EQPTID"] + "'").Any() ? null : DataTableConverter.Convert(result.Select("EQPTID = '" + bizResult.Rows[i]["EQPTID"] + "'").CopyToDataTable());

            //            //    if (bizResult.Rows[i]["VD_QA_INSP_COND_CODE"].Equals("VD_QA_INSP_RULE_02"))//대LOT기준
            //            //    {

            //            //        list[i].dgcLotId.Visibility = Visibility.Collapsed;
            //            //        list[i].dgcLotCount.Visibility = Visibility.Visible;
            //            //        tmp = !result.Select("EQPTID = '" + bizResult.Rows[i]["EQPTID"] + "'").Any() ? null : result.Select("EQPTID = '" + bizResult.Rows[i]["EQPTID"] + "'").CopyToDataTable();

            //            //        if (tmp != null)
            //            //        {
            //            //            var a = tmp.AsEnumerable().GroupBy(x => new
            //            //            {
            //            //                EQPT_BTCH_WRK_NO = x.Field<string>("EQPT_BTCH_WRK_NO"),
            //            //                LOTID_RT = x.Field<string>("LOTID_RT"),
            //            //                JUDG_VALUE = x.Field<string>("JUDG_VALUE")
            //            //            }).Select(g => new
            //            //            {
            //            //                EQPT_BTCH_WRK_NO = g.Key.EQPT_BTCH_WRK_NO,
            //            //                LOTID_RT = g.Key.LOTID_RT,
            //            //                JUDG_VALUE = g.Key.JUDG_VALUE,
            //            //                CHK = g.Max(x => x.Field<int>("CHK")),
            //            //                LOTID = g.Max(x => x.Field<string>("LOTID")),
            //            //                EQPTID = g.Max(x => x.Field<string>("EQPTID")),
            //            //                PRODID = g.Max(x => x.Field<string>("PRODID")),
            //            //                PRJT_NAME = g.Max(x => x.Field<string>("PRJT_NAME")),
            //            //                JUDG_NAME = g.Max(x => x.Field<string>("JUDG_NAME")),
            //            //                WIPDTTM_ED = g.Max(x => x.Field<string>("WIPDTTM_ED")),
            //            //                ELEC = g.Max(x => x.Field<string>("ELEC")),
            //            //                VD_QA_INSP_COND_CODE = g.Max(x => x.Field<string>("VD_QA_INSP_COND_CODE")),
            //            //                REWORKCNT = g.Max(x => x.Field<decimal>("REWORKCNT")),
            //            //                COUNT = g.Count()
            //            //            });

            //            //            DataTable dt = new DataTable();
            //            //            dt.Columns.Add("CHK", typeof(bool)); //
            //            //            dt.Columns.Add("LOTID", typeof(string)); //
            //            //            dt.Columns.Add("LOTID_RT", typeof(string));//
            //            //            dt.Columns.Add("JUDG_VALUE", typeof(string));
            //            //            dt.Columns.Add("JUDG_NAME", typeof(string));
            //            //            dt.Columns.Add("EQPT_BTCH_WRK_NO", typeof(string));//
            //            //            dt.Columns.Add("WIPDTTM_ED", typeof(string));
            //            //            dt.Columns.Add("PRODID", typeof(string));//
            //            //            dt.Columns.Add("PRJT_NAME", typeof(string));//
            //            //            dt.Columns.Add("ELEC", typeof(string));
            //            //            dt.Columns.Add("VD_QA_INSP_COND_CODE", typeof(string));
            //            //            dt.Columns.Add("REWORKCNT", typeof(decimal));
            //            //            dt.Columns.Add("COUNT", typeof(decimal));

            //            //            DataRow dtRow = null;

            //            //            foreach (var j in a)
            //            //            {
            //            //                dtRow = dt.NewRow();
            //            //                dtRow["CHK"] = j.CHK;
            //            //                dtRow["LOTID"] = j.LOTID;
            //            //                dtRow["LOTID_RT"] = j.LOTID_RT;
            //            //                dtRow["JUDG_VALUE"] = j.JUDG_VALUE;
            //            //                dtRow["JUDG_NAME"] = j.JUDG_NAME;
            //            //                dtRow["EQPT_BTCH_WRK_NO"] = j.EQPT_BTCH_WRK_NO;
            //            //                dtRow["WIPDTTM_ED"] = j.WIPDTTM_ED;
            //            //                dtRow["PRODID"] = j.PRODID;
            //            //                dtRow["PRJT_NAME"] = j.PRJT_NAME;
            //            //                dtRow["ELEC"] = j.ELEC;
            //            //                dtRow["VD_QA_INSP_COND_CODE"] = j.VD_QA_INSP_COND_CODE;
            //            //                dtRow["REWORKCNT"] = j.REWORKCNT;
            //            //                dtRow["COUNT"] = j.COUNT;
            //            //                dt.Rows.Add(dtRow);
            //            //            }


            //            //           list[i].dgFinishLot.ItemsSource = DataTableConverter.Convert(dt);

            //            //            Util.GridSetData(list[i].dgFinishLot, DataTableConverter.Convert(list[i].dgFinishLot.ItemsSource), null,true);
            //            //        }

            //            //    }
            //            }

                       
            //            GetEqpt(i, bizResult);

            //        }

            //        loadingIndicator.Visibility = Visibility.Collapsed;
            //        SetEqptWindow(bizResult, rdoTwo.IsChecked == true ? 2 : 3);
            //        FrameOperation.PrintFrameMessage(bizResult.Rows.Count + MessageDic.Instance.GetMessage("건"));

            //        System.Diagnostics.Debug.WriteLine("After clear: {0} Bytes.", GC.GetTotalMemory(false));

            //    });

            //}
            //catch (Exception ex)
            //{
            //    loadingIndicator.Visibility = Visibility.Collapsed;
            //    Util.MessageException(ex);
            //}

          
        }

        private DataTable SetVDFinish()
        {
            try
            {
                DataTable result = null;

                DataTable data = new DataTable();
                data.Columns.Add("LANGID", typeof(string));
                data.Columns.Add("EQPTID", typeof(string));
                data.Columns.Add("PROCID", typeof(string));
                data.Columns.Add("WIPSTAT", typeof(string));
                data.Columns.Add("CMCDTYPE", typeof(string));
                data.Columns.Add("JUDG_VALUE", typeof(string));
                data.Columns.Add("QA_INSP_TRGT_FLAG_CONFIRM", typeof(string));

                DataRow row = data.NewRow();
                row["LANGID"] = LoginInfo.LANGID;
                row["EQPTID"] = null;
                row["WIPSTAT"] = Wip_State.END;
                row["PROCID"] = Convert.ToString(cboVDProcess.SelectedValue);
                row["CMCDTYPE"] = "QAJUDGE";
                row["QA_INSP_TRGT_FLAG_CONFIRM"] = "C";
                data.Rows.Add(row);

                result = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_WIP_VD_QA_TARGET", "RQSTDT", "RSLTDT", data);
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
                DataTable data = new DataTable();
                data.Columns.Add("LANGID", typeof(string));
                data.Columns.Add("EQPTID", typeof(string));
                data.Columns.Add("PROCID", typeof(string));

                DataRow row = data.NewRow();
                row["LANGID"] = LoginInfo.LANGID;
                row["EQPTID"] = null;
                row["PROCID"] = Convert.ToString(cboVDProcess.SelectedValue);
                data.Rows.Add(row);

                DataTable result = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_VD_STATUS_QA", "RQSTDT", "RSLTDT", data);

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
            //if (list == null) return;
            //if (list.Count == 0) return;

            //for (int i = 0; i < list.Count; i++)
            //{
            //    list[i].ClearData();
            //}

            //for (int i = grdEqpt.Children.Count - 1; i >= 0; i--)
            //{
            //    ((Grid)(grdEqpt.Children[i])).Children.Remove(list[i]);

            //    grdEqpt.Children.Remove(((Grid)grdEqpt.Children[i]));
            //}


            //grdEqpt.Children.Clear();
            //grdEqpt.ColumnDefinitions.Clear();
            //grdEqpt.RowDefinitions.Clear();
           
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
                  //  grid.Children.Add(list[num]);

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

            //list[i].FrameOperation = FrameOperation;

            //object[] parameters = new object[7];
            //parameters[0] = rdoRun.IsChecked != true;
            //parameters[1] = dt.Rows[i]["PRDT_CLSS_CHK_FLAG"].GetString();
            //parameters[2] = dt.Rows[i]["PRDT_CLSS_CODE"].GetString();
            //parameters[3] = dt.Rows[i]["EQPTNAME"].GetString();
            //parameters[4] = dt.Rows[i]["EQPTID"].GetString();
            //parameters[5] = cboVDEquipmentSegment.SelectedValue.GetString();
            //parameters[6] = dt.Rows[i]["VD_QA_INSP_COND_CODE"].GetString();

            //C1WindowExtension.SetParameters(list[i], parameters);


            //list[i].SetValue();

        }


        #endregion
    }
}