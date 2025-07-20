/*************************************************************************************
 Created Date : 2017.12.20
      Creator : Shin Kwang Hee
   Decription : 전지 5MEGA-GMES 구축 - 소형조립 공정진척(Assembly용) CMM_ASSY_CANCEL_TERM 파일 참조 
--------------------------------------------------------------------------------------
 [Change History]
  2017.12.20  Shin Kwang Hee : Initial Created.
  2020.05.27  김동일 : C20200513-000349 재고 및 수율 정합성 향상을 위한 투입Lot 종료 취소에 대한 기능변경

**************************************************************************************/


using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using C1.WPF;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.ControlsLibrary;
using System.Data;
using C1.WPF.DataGrid;
using System.Windows.Media;
using System.Linq;

namespace LGC.GMES.MES.BOX001
{

    public partial class BOX001_BOX_MAPPING : C1Window, IWorkArea
    {
        #region Declaration & Constructor         
        private string _processCode = string.Empty;
        public string lotCode = string.Empty;
        private readonly BizDataSet _bizDataSet = new BizDataSet();

        #endregion

        #region Initialize 
        /// <summary>
        ///  Frame과 상호작용하기 위한 객체
        /// </summary>
        public IFrameOperation FrameOperation
        {
            get;
            set;
        }
        public BOX001_BOX_MAPPING()
        {
            InitializeComponent();
        }
        #endregion

        #region Event
        private void C1Window_Initialized(object sender, EventArgs e)
        {
            try
            {
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void txtPalletID_KeyDown(object sender, KeyEventArgs e)
        {
            try
            {
                if (e.Key == Key.Enter)
                {

                    DataTable dtSource = DataTableConverter.Convert(dgPalletID.ItemsSource);

                    for (int i = 0; i < dtSource.Rows.Count; i++)
                    {
                        if (txtPalletID.Text.ToString().Equals(dtSource.Rows[i]["PalletID"]))
                        {
                            Util.Alert("SFU1781");
                            return;
                        }
                    }


                    DataTable inTable = new DataTable();
                    inTable.Columns.Add("PALLETID", typeof(string));

                    DataRow dr = inTable.NewRow();

                    dr["PALLETID"] = txtPalletID.Text;
                    inTable.Rows.Add(dr);

                    DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_BOX_DESAY", "INDATA", "OUTDATA", inTable);

                    if (dtResult.Rows.Count == 0)
                    {
                        Util.Alert("SFU3394");
                        return;
                    }

                    if (!string.IsNullOrEmpty(dtResult.Rows[0]["CUST_BOXID"].ToString()))
                    {
                        Util.Alert("SFU8256", dtResult.Rows[0]["PALLETID"].ToString(), dtResult.Rows[0]["CUST_BOXID"].ToString());
                        return;
                    }

                    if(dtSource.Rows.Count > 0){
                        if (!dtSource.Rows[0]["PRODID"].Equals(dtResult.Rows[0]["PRODID"]))
                        {
                            Util.Alert("SFU1502");
                            return;
                        }
                        if (!dtSource.Rows[0]["CUSTPRODID"].Equals(dtResult.Rows[0]["CUSTPRODID"]))
                        {
                            Util.Alert("SFU8254");
                            return;
                        }
                        if (!dtSource.Rows[0]["CUST_MDLID"].Equals(dtResult.Rows[0]["CUST_MDLID"]))
                        {
                            Util.Alert("SFU8255");
                            return;
                        }
                    }


                    //DataTable dtMerge = new DataTable();
                    //dtMerge.Columns.Add("CHK", typeof(Boolean));
                    //dtMerge.Columns.Add("PalletID", typeof(string));
                    //DataRow newRow = dtMerge.NewRow();
                    //newRow["CHK"] = false;
                    //newRow["PalletID"] = txtPalletID.Text.ToString();

                    //dtMerge.Rows.Add(newRow);

                    dtResult.Merge(dtSource);


                    for (int i = 0; i < dtResult.Rows.Count; i++)
                    {
                        dtResult.Rows[i]["CHK"] = false;
                    }

                    Util.GridSetData(dgPalletID, dtResult, FrameOperation, false);
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void btnClose_Clicked(object sender, RoutedEventArgs e)
        {
            this.DialogResult = MessageBoxResult.Cancel;
        }
        #endregion

        #region Mehod




        private void btnBoxMapping_Click(object sender, RoutedEventArgs e)
        {
            try
            {

                if (dgPalletID.ItemsSource == null || dgPalletID.Rows.Count <= 0)
                {
                    Util.Alert("SFU3394");
                    return;
                }

                DataTable dtSource = DataTableConverter.Convert(dgPalletID.ItemsSource);

                DataSet indataSet = new DataSet();

                DataTable dtInPallet = indataSet.Tables.Add("INPALLET");
                dtInPallet.Columns.Add("PALLETID", typeof(string));

                DataRow drnewrow;

                for (int i = 0; i < dtSource.Rows.Count; i++)
                {
                    drnewrow = dtInPallet.NewRow();
                    drnewrow["PALLETID"] = dtSource.Rows[i]["PalletID"];
                    dtInPallet.Rows.Add(drnewrow);
                }

                DataTable dtProd = indataSet.Tables.Add("INPROD");
                dtProd.Columns.Add("PRODID");
                dtProd.Columns.Add("CUSTPRODID");
                dtProd.Columns.Add("CUST_MDLID");

                drnewrow = dtProd.NewRow();
                drnewrow["PRODID"] = dtSource.Rows[0]["PRODID"];
                drnewrow["CUSTPRODID"] = dtSource.Rows[0]["CUSTPRODID"];
                drnewrow["CUST_MDLID"] = dtSource.Rows[0]["CUST_MDLID"]; 

                dtProd.Rows.Add(drnewrow);


                DataTable dtData = indataSet.Tables.Add("INDATA");
                dtData.Columns.Add("USERID");

                drnewrow = dtData.NewRow();
                drnewrow["USERID"] = LoginInfo.USERID;

                dtData.Rows.Add(drnewrow);

                new ClientProxy().ExecuteService_Multi("BR_PRD_REG_BOXID_MAPPING", "INPALLET,INPROD,INDATA", null, (Result, ex) =>
                {
                    if (ex != null)
                    {
                        Util.MessageException(ex);
                        return;
                    }
                    Util.MessageInfo("SFU1275");    //정상 처리 되었습니다.
                    this.DialogResult = MessageBoxResult.OK;
                    this.Close();

                }, indataSet);

            }
            catch (Exception ex)
            {
                Util.AlertInfo(ex.Message);
            }
        }

        private void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (dgPalletID.ItemsSource == null || dgPalletID.Rows.Count <= 0)
                    return;

                DataTable dt = DataTableConverter.Convert(dgPalletID.ItemsSource);

               foreach(DataRow dr in dt.AsEnumerable().ToList())
                {
                    if (dr["CHK"].Equals(1) || dr["CHK"].Equals(true))
                    {
                        dt.Rows.Remove(dr);
                    }
                }

                Util.GridSetData(dgPalletID, dt, FrameOperation, false);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void wndMappingCust_Closed(object sender, EventArgs e)
        {
            BOX001_BOX_MAPPING_CUST window = sender as BOX001_BOX_MAPPING_CUST;

            if (window.DialogResult == MessageBoxResult.OK)
            {
                txtPalletID.Clear();
                Util.gridClear(dgPalletID);
            }

        }
    }
    #endregion
}
