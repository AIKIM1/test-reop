/************************************************************************************* 
 Created Date : 2023.12.27
      Creator : Chi Woo
   Decription : EOL/Degas 공 Tray 출고 NEW
--------------------------------------------------------------------------------------
 [Change History]
  2023.12.27  이의철 : Initial Created. 공트레이 공급-보관 현황 개선안 적용
**************************************************************************************/

using C1.WPF;
using C1.WPF.DataGrid;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.CMM001.Extensions;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using System;
using System.Data;
using System.Windows;

namespace LGC.GMES.MES.FCS001
{
    public partial class FCS001_165_TRAY_OUT_NEW : C1Window, IWorkArea
    {
        #region Declaration & Constructor 
        private string EQPTID = string.Empty;

        public FCS001_165_TRAY_OUT_NEW()
        {
            InitializeComponent();
        }

        public IFrameOperation FrameOperation
        {
            get;
            set;
        }
        #endregion

        #region Initialize
        

        #endregion

        #region Event
        private void C1Window_Loaded(object sender, RoutedEventArgs e)
        {
            InitCombo();

            InitSpread();

            GetList();
        }

        private void InitCombo()
        {

            //CommonCombo_Form _combo = new CommonCombo_Form();

        }

        private void InitSpread()
        {

        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                DataTable chkTable = dgTrayOut.GetDataTable();

                for (int row1 = 0; row1 < chkTable.Rows.Count; row1++)
                {
                    DataRow dr1 = chkTable.Rows[row1];
                    for (int row2 = 0; row2 < chkTable.Rows.Count; row2++)
                    {
                        DataRow dr2 = chkTable.Rows[row2];
                        if (row1 == row2) continue;

                        //if (dr1["PORT_ID"].Nvc() + dr1["EQPTID"].Nvc() + dr1["CST_TYPE_CODE"].Nvc() == dr2["PORT_ID"].Nvc() + dr2["EQPTID"].Nvc() + dr2["CST_TYPE_CODE"].Nvc())
                        if (dr1["PORT_ID"].Nvc() + dr1["DEFAULT_STORAGE_LOCATION_CODE"].Nvc() == dr2["PORT_ID"].Nvc() + dr2["DEFAULT_STORAGE_LOCATION_CODE"].Nvc() )
                        {
                            Util.MessageValidation("SFU2051"); //중복 데이터가 있습니다.
                            return;
                        }
                    }
                }

                if (!dgTrayOut.IsCheckedRow("CHK"))
                {
                    Util.MessageValidation("SFU1651"); //선택한 항목이 없습니다.
                    return;
                }

                foreach (int chkRowInx in dgTrayOut.GetCheckedRowIndex("CHK"))
                {
                    DataRow chkRow = dgTrayOut.GetDataRow(chkRowInx);

                    if (chkRow["PORT_ID"].IsNvc())
                    {
                        Util.MessageValidation("SFU8275", dgTrayOut.Columns["PORT_ID"].Header.Nvc()); //%1 입력해 주세요
                        return;
                    }
                    
                    if (chkRow["DEFAULT_STORAGE_LOCATION_CODE"].Nvc() == string.Empty)
                    {
                        Util.MessageValidation("SFU1636", dgTrayOut.Columns["DEFAULT_STORAGE_LOCATION_CODE"].Header.Nvc());  //선택된 대상이 없습니다.
                        return;
                    }

                }

                //요청 하시겠습니까?
                Util.MessageConfirm("SFU2924", (result) =>
                {
                    try
                    {
                        if (result == MessageBoxResult.OK)
                        {
                            DataTable dtRqst = new DataTable();
                            dtRqst.Columns.Add("PORT_ID", typeof(string));
                            dtRqst.Columns.Add("AREAID", typeof(string));
                            //dtRqst.Columns.Add("EQPTID", typeof(string));
                            dtRqst.Columns.Add("DEFAULT_SUPPLY_LOCATION_CODE", typeof(string));
                            dtRqst.Columns.Add("DEFAULT_STORAGE_LOCATION_CODE", typeof(string));                            
                            dtRqst.Columns.Add("USERID", typeof(string));

                            foreach (DataRow chkRow in dgTrayOut.GetCheckedDataRow("CHK"))
                            {
                                //if (chkRow["EQPTID"].Nvc() == string.Empty) continue;

                                DataRow drRqst = dtRqst.NewRow();
                                drRqst["PORT_ID"] = chkRow["PORT_ID"].Nvc();
                                drRqst["AREAID"] = LoginInfo.CFG_AREA_ID;
                                //drRqst["EQPTID"] = chkRow["EQPTID"].Nvc();
                                drRqst["DEFAULT_SUPPLY_LOCATION_CODE"] = "";// chkRow["DEFAULT_SUPPLY_LOCATION_CODE"].Nvc();
                                drRqst["DEFAULT_STORAGE_LOCATION_CODE"] = chkRow["DEFAULT_STORAGE_LOCATION_CODE"].Nvc();                                
                                drRqst["USERID"] = LoginInfo.USERID;
                                dtRqst.Rows.Add(drRqst);
                            }
                            
                            DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_MER_FORMLGS_EMPTY_TRAY_TRF_INFO", "RQSTDT", "RSLTDT", dtRqst);

                            Util.AlertInfo("FM_ME_0186");  //요청을 완료하였습니다.
                            
                            GetList();
                        
                        }
                    }
                    catch (Exception ex)
                    {
                        Util.MessageException(ex);
                    }
                });
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = MessageBoxResult.Cancel;
        }

        private void dgTrayOut_BeginningEdit(object sender, C1.WPF.DataGrid.DataGridBeginningEditEventArgs e)
        {

        }

        private void dgTrayOut_CommittedEdit(object sender, C1.WPF.DataGrid.DataGridCellEventArgs e)
        {

        }

        private void dgTrayOut_ExecuteDataCompleted(object sender, CMM001.Controls.UcBaseDataGrid.ExecuteDataEventArgs e)
        {
            //getEqpt();

            //getLocation("SUPPLY","DEFAULT_SUPPLY_LOCATION_CODE");
            getLocation("STORAGE", "DEFAULT_STORAGE_LOCATION_CODE");

        }

        private void getEqpt()
        {
            DataTable dtRqst = new DataTable();
            dtRqst.TableName = "RQSTDT";
            dtRqst.Columns.Add("LANGID", typeof(string));
            dtRqst.Columns.Add("S70", typeof(string));
            dtRqst.Columns.Add("EQPTLEVEL", typeof(string));

            DataRow drRqst = dtRqst.NewRow();
            drRqst["LANGID"] = LoginInfo.LANGID;
            drRqst["S70"] = "D,5";
            drRqst["EQPTLEVEL"] = "M";
            dtRqst.Rows.Add(drRqst);

            DataTable dtEqpt = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_COMBO_EQP", "RQSTDT", "RSLTDT", dtRqst);

            dgTrayOut.SetGridColumnCombo("EQPTID", dtEqpt, isInBlank: true, isInCode: true);
        }

        private void getLocation(string location_type_code, string column_name)
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("AREAID", typeof(string));
                RQSTDT.Columns.Add("LOCATION_TYPE_CODE", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["AREAID"] = LoginInfo.CFG_AREA_ID;
                dr["LOCATION_TYPE_CODE"] = location_type_code; //sFilter[0]; //SUPPLY/STORAGE
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_SEL_LOCATION_LIST_BY_COM_CODE", "RQSTDT", "RSLTDT", RQSTDT);
               
                dgTrayOut.SetGridColumnCombo(column_name, dtResult, isInBlank: true, isInCode: true);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }            
        }       


        #endregion

        #region Mehod
        private void GetList()
        {
            try
            {
                DataTable dtRqst = new DataTable("RQSTDT");
                dtRqst.Columns.Add("LANGID", typeof(string));

                DataRow dr = dtRqst.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dtRqst.Rows.Add(dr);

                // 백그라운드 실행. DA_SEL_FORMLGS_TRAY_MOVE_INFO
                dgTrayOut.ExecuteService("DA_SEL_FORMLGS_EMPTY_TRAY_TRF_INFO", "RQSTDT", "RSLTDT", dtRqst, autoWidth: false);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        #endregion

        private void btnRowAdd_Click(object sender, RoutedEventArgs e)
        {
            if (dgTrayOut.ItemsSource == null || dgTrayOut.Rows.Count < 0)
                return;

            DataTable dtData = dgTrayOut.GetDataTable(false);
            DataRow drNew = dtData.NewRow();
            drNew["CHK"] = 1;
            dtData.Rows.Add(drNew);
        }
                
    }
}
