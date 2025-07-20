/*************************************************************************************
 Created Date : 2017.01.25
      Creator : 이진선
   Decription : 라미이송
--------------------------------------------------------------------------------------
 [Change History]
  2016.06.16  DEVELOPER : Initial Created.
  2019.03.05  오화백 : RF_ID 체크 로직 추가(RF_ID일 경우 리스트에 CST정보 보여줌)




 
**************************************************************************************/

using C1.WPF;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace LGC.GMES.MES.ASSY001
{
    public partial class ASSY001_025 : UserControl, IWorkArea
    {
        #region Declaration & Constructor 

        List<ASSY001_025_EQPTWIN> list = null;
        //ASSY001_025_EQPTWIN _win = null;
        CommonCombo combo = new CommonCombo();
        Util _Util = new Util();
        IFrameOperation iFO = null;
        string _SkipFlag = string.Empty;
        int ListCnt = -1;

        //2019.03.05 오화백 RF_ID 투입부, 배출부 RFID  
        private string _LDR_LOT_IDENT_BAS_CODE = string.Empty; //투입부
        private string _UNLDR_LOT_IDENT_BAS_CODE = string.Empty; //배출부

        public IFrameOperation FrameOperation
        {
            get;
            set;
        }

        public ASSY001_025()
        {
            InitializeComponent();
            Loaded += UserControl_Loaded;
        }
        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            Loaded -= UserControl_Loaded;
            ApplyPermissions();
            initcombo();

            list = new List<ASSY001_025_EQPTWIN>();
            for (int i = 0; i < 10; i++)
            {
                list.Add(new ASSY001_025_EQPTWIN());
            }

            ListCnt = list.Count();
            
        }

        #endregion

        private void btnSearch_Click(object sender, RoutedEventArgs e)
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

        private void cboVDEquipmentSegment_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            string[] sFilter = { Process.VD_LMN, Convert.ToString(cboVDFloor.SelectedValue) };
            combo.SetCombo(cboVDFloor, CommonCombo.ComboStatus.ALL, sFilter: sFilter);

            DataTable dt = new DataTable();
            dt.Columns.Add("PROCID", typeof(string));
            dt.Columns.Add("EQSGID", typeof(string));


            DataRow row = dt.NewRow();
            row["PROCID"] = Process.VD_LMN;
            row["EQSGID"] = Convert.ToString(cboVDEquipmentSegment.SelectedValue);
            dt.Rows.Add(row);

            DataTable result = new ClientProxy().ExecuteServiceSync("DA_PROD_SEL_PROCESSEQUIPMENTSEGMENT_VDQA", "RQSTDT", "RSLTDT", dt);
            if (result == null) return;
            if (result.Rows.Count == 0 || result.Rows[0]["LQC_SKIP_FLAG"].Equals(""))
            {
                Util.Alert("SFU2810"); //LQC_SKIP_FLAG가 없습니다.
                return;
            }
            _SkipFlag = Convert.ToString(result.Rows[0]["LQC_SKIP_FLAG"]);
        }

        #region[Method]
        private void initcombo()
        {
            string[] sFilter = { Process.VD_LMN,  LoginInfo.CFG_AREA_ID,  };
            combo.SetCombo(cboVDEquipmentSegment, CommonCombo.ComboStatus.NONE, sFilter:sFilter);

            string[] sFilter2 = { "ELEC_TYPE" };
            combo.SetCombo(cboEquipmentElec, CommonCombo.ComboStatus.ALL, sFilter:sFilter2, sCase: "COMMCODE");

            string[] sFilter3 = { Process.VD_LMN, Convert.ToString(cboVDEquipmentSegment.SelectedValue) };
            combo.SetCombo(cboVDFloor, CommonCombo.ComboStatus.ALL, sFilter: sFilter3);

        }

        public void SearchData()
        {

            // ClearData();

            
            GetLotIdentBasCode(); //2019.03.05 오화백 RF_ID 투입부, 배출부 여부 

            DataTable result = null;
            DataTable tmp = null;

            result = setVDFinish();

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
            row["FLOOR"] = Convert.ToString(cboVDFloor.SelectedValue).Equals("") ? null : Convert.ToString(cboVDFloor.SelectedValue);
            data.Rows.Add(row);

            try
            {
                loadingIndicator.Visibility = Visibility.Visible;

                new ClientProxy().ExecuteService("DA_PRD_SEL_EQPT_VD", "RQST", "RSLT",data, (bizResult, bizException) =>
                {
                    try
                    {
                      

                        if (bizException != null)
                        {
                            Util.MessageException(bizException);
                            return;
                        }

                        if (bizResult.Rows.Count == 0)
                        {
                            FrameOperation.PrintFrameMessage("[" + System.DateTime.Now.ToString("hh:mm:ss") + "]" + MessageDic.Instance.GetMessage("SFU1905"));//조회된 Data가 없습니다.
                            return;
                        }

                        if (ListCnt < bizResult.Rows.Count)
                        {
                            for (int i = 0; i < bizResult.Rows.Count - ListCnt; i++)
                            {
                                list.Add(new ASSY001_025_EQPTWIN());
                            }

                            ListCnt = list.Count();
                        }

                        ClearData();


                        for (int i = 0; i < bizResult.Rows.Count; i++)
                        {
                            list[i].dgFinishLot.ItemsSource = null;

                            tmp = result.Select("EQPTID = '" + bizResult.Rows[i]["EQPTID"] + "'").Count() == 0 ? null : result.Select("EQPTID = '" + bizResult.Rows[i]["EQPTID"] + "'").CopyToDataTable();
                            if (tmp != null)
                            {
                                if (_SkipFlag.Equals("Y")) //검사대기, pass 이송(HOLD 안된LOT)
                                {
                                    Util.GridSetData(list[i].dgFinishLot, tmp, null, true);
                                }
                                else //pass만 이송(HOLD된 LOT)
                                {

                                    list[i].dgFinishLot.ItemsSource = tmp.Select("JUDG_VALUE <> 'WAIT'").Count() == 0 ? null : DataTableConverter.Convert(tmp.Select("JUDG_VALUE <> 'WAIT'").CopyToDataTable());
                                    Util.GridSetData(list[i].dgFinishLot, DataTableConverter.Convert(list[i].dgFinishLot.ItemsSource), null, true);
                                }
                            }

                            list[i].ELECCHECK = Convert.ToString(bizResult.Rows[i]["PRDT_CLSS_CHK_FLAG"]);
                            list[i].EQPT_ELEC = Convert.ToString(bizResult.Rows[i]["PRDT_CLSS_CODE"]);
                            list[i].EQPTNAME = Convert.ToString(bizResult.Rows[i]["EQPTNAME"]);
                            list[i].EQPTID = Convert.ToString(bizResult.Rows[i]["EQPTID"]);
                            list[i].LINE_SKIP = _SkipFlag;
                            list[i].GetEqpt();
                            //2019.03.05 오화백 RF_ID 투입부, 배출부 여부 
                            list[i]._LDR_LOT_IDENT_BAS_CODE = _LDR_LOT_IDENT_BAS_CODE;
                            list[i]._UNLDR_LOT_IDENT_BAS_CODE = _UNLDR_LOT_IDENT_BAS_CODE;
                           
                        }

                  
                         SetEqptWindow(bizResult, rdoTwo.IsChecked == true ? 2 : 3);

                        FrameOperation.PrintFrameMessage(bizResult.Rows.Count + MessageDic.Instance.GetMessage("건"));


                    }
                    catch (Exception ex)
                    {
                        Util.MessageException(ex);
                    }
                    finally
                    {
                        loadingIndicator.Visibility = Visibility.Collapsed;
                    }
                });

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }

        }

        private void ClearData()
        {
            if (list == null) return;
            if (list.Count() == 0) return;

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
        private DataTable setVDFinish()
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
                data.Columns.Add("WIPHOLD", typeof(string));
                data.Columns.Add("QA_INSP_TRGT_FLAG_CONFIRM_LAMI", typeof(string));


                DataRow row = data.NewRow();
                row["LANGID"] = LoginInfo.LANGID;
                row["EQPTID"] = null;
                row["WIPSTAT"] = Wip_State.END;
                row["PROCID"] = Process.VD_LMN;
                row["CMCDTYPE"] = "QAJUDGE";
                row["JUDG_VALUE"] = null;
                row["WIPHOLD"] = "N";
                row["QA_INSP_TRGT_FLAG_CONFIRM_LAMI"] = _SkipFlag.Equals("Y") ? null : "C";
                data.Rows.Add(row);


                result = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_WIP_VD_QA_TARGET_LAMI", "RQSTDT", "RSLTDT", data);
                return result;


            }
            catch (Exception ex)
            {
                Util.Alert(ex.ToString());
                return null;
            }

        }
        private void ApplyPermissions()
        {
            List<Button> listAuth = new List<Button>();
            Util.pageAuth(listAuth, FrameOperation.AUTHORITY);
        }

        private void SetEqptWindow(DataTable bizResult, int rowCount)
        {
            for (int i = 0; i < rowCount; i++)
            {
                var rowDef = new RowDefinition();
                // rowDef.Height = GridLength.Auto;
                // rowDef.MinHeight = rowCount==2 ? 350 : 300;
                rowDef.Height = rowCount == 2 ? new GridLength(360) : new GridLength(250);
                grdEqpt.RowDefinitions.Add(rowDef);

            }

            for (int i = 0; i < Math.Ceiling(((double)bizResult.Rows.Count / rowCount)); i++)
            {
                var colDef = new ColumnDefinition();
                colDef.MinWidth = 400;
                //colDef.Width = GridLength.Auto;
                colDef.Width = rowCount == 2 ? new GridLength(470) : new GridLength(200);
                
                grdEqpt.ColumnDefinitions.Add(colDef);
            }


            int num = 0;

            for (int i = 0; i < grdEqpt.RowDefinitions.Count; i++)
            {
                for (int j = 0; j < grdEqpt.ColumnDefinitions.Count; j++)
                {

                    var grid = new Grid();
                    grid.Name = "gr0" + num;
                    if (i == 0) grid.Margin = new Thickness(0, 8, 8, 8);
                    else grid.Margin = new Thickness(0, 0, 8, 8);

                    grid.SetValue(Grid.RowProperty, i);
                    grid.SetValue(Grid.ColumnProperty, j);

                    list[num].FrameOperation = FrameOperation;

                    grid.Children.Add(list[num]);
                   // GetEqpt(num, bizResult);

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
            list[i].ELECCHECK = Convert.ToString(dt.Rows[i]["PRDT_CLSS_CHK_FLAG"]);
            list[i].EQPT_ELEC =  Convert.ToString(dt.Rows[i]["PRDT_CLSS_CODE"]);
            list[i].EQPTNAME = Convert.ToString(dt.Rows[i]["EQPTNAME"]);
            list[i].EQPTID = Convert.ToString(dt.Rows[i]["EQPTID"]);
            list[i].LINE_SKIP = _SkipFlag;
            list[i].GetEqpt();
            //list[i].Parent = this;

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

        #endregion




    }


}
