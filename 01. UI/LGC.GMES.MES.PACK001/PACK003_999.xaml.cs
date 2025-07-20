/************************************************************************************
  Created Date : 2022.06.22
       Creator : 이태규
   Description : 원통형 Cell Pack 출고 (가칭)
 ------------------------------------------------------------------------------------
  [Change History]
    2022.06.22  이태규 : Initial Created.
 ************************************************************************************/

using System;
using System.Collections.Generic;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using C1.WPF;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using System.Windows.Threading;

namespace LGC.GMES.MES.PACK001
{
    public partial class PACK003_999 : UserControl, IWorkArea
    {
        #region #. Member Variable Lists...
        LGC.GMES.MES.PACK001.Class.MESSAGE_PARAM ms = new LGC.GMES.MES.PACK001.Class.MESSAGE_PARAM();
        Util util = new Util();
        private static string CREATED = "CREATED,";
        private static string PACKING = "PACKING,";
        private static string PACKED = "PACKED,";
        private static string SHIPPING = "SHIPPING,";
        private string _searchStat = string.Empty;
        private bool bInit = true;
        string sTagetArea = string.Empty;
        string sTagetEqsg = string.Empty;

        /// <summary>
        /// Frame과 상호작용하기 위한 객체
        /// </summary>
        public IFrameOperation FrameOperation
        {
            get;
            set;
        }
        #endregion

        #region #. Declaration & Constructor 
        public PACK003_999()
        {
            InitializeComponent();

            //if (LoginInfo.CFG_AREA_ID == "P7" || LoginInfo.CFG_AREA_ID == "P8" || LoginInfo.CFG_AREA_ID == "PA" )
            //{
            //    btnTagetMove.Visibility = Visibility.Visible;
            //}
            //else
            //{
            //    btnTagetMove.Visibility = Visibility.Collapsed;
            //}
        }
        #endregion

        #region #. Event Lists...
        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                List<Button> listAuth = new List<Button>();
                Util.pageAuth(listAuth, FrameOperation.AUTHORITY);
                this.Initialize();
                this.InitCombo();
                this.Loaded -= new System.Windows.RoutedEventHandler(this.UserControl_Loaded);
            }
            catch (Exception ex)
            {
                Util.Alert(ex.ToString());
            }

        }

        #region #. Scan events
        /// <summary>
        /// Initialize
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnInit_Click(object sender, RoutedEventArgs e)
        {
            if (this.dgTargetList.Rows.Count != 0)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("SFU1815"), null, "Info", MessageBoxButton.OKCancel, MessageBoxIcon.None, (Result) =>
                {
                    if (Result == MessageBoxResult.OK)
                    {
                        txtPalletID.Focus();
                        this.txtPalletID.Clear();
                        Util.gridClear(dgTargetList);
                        txtPalletID.Focus();
                        Util.DataGridCheckAllUnChecked(dgTargetList);
                    }
                });
            }
            else
            {
                txtPalletID.Focus();
            }
        }
        /// <summary>
        /// Scan
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void txtPalletID_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                string sPalletID = txtPalletID.Text.Trim();
                if (string.IsNullOrWhiteSpace(sPalletID))
                {
                    LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("SFU2060"), null, "Info", MessageBoxButton.OK, MessageBoxIcon.None, (result) =>
                    {
                        this.txtPalletID.Focus();
                    });
                }
                else
                {
                    Scan(sPalletID);
                }
            }

            #region #. 참고
            //try
            //{
            //    if (e.Key == Key.Enter)
            //    {
            //        dgTargetList.LoadedCellPresenter -= dgTagetListCellPresenter;
            //        dgTargetList.LoadedCellPresenter += dgTagetListCellPresenter;
            //        //ChkGbtInPallet();
            //        // 2019.12.27 염규범
            //        // 김정균 책임님 요청의 건
            //        // 입고 Validation
            //        // getTagetPalletInfo();
            //        getTagetPalletInfoNew();

            //        //dgTagetList.LoadedCellPresenter -= dgTagetListCellPresenter;
            //    }
            //}
            //catch (Exception ex)
            //{
            //    Util.Alert(ex.ToString());
            //}
            #endregion
        }
        /// <summary>
        /// Retreive
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnRetreive_Click(object sender, RoutedEventArgs e)
        {
            DataTable dtData = DataTableConverter.Convert(dgTargetList.ItemsSource);

            if (dtData.Rows.Count == 0 ) {
                // 10008 : 선택된 데이터가 없습니다.]
                Util.MessageValidation("10008");
                return;
            }

            try
            {
                for (int i = 0; i < dtData.Rows.Count; i++)
                {
                    if (dtData.Rows[i]["CHK"].Equals("true"))
                    {
                        DataTable RQSTDT = new DataTable("INDATA");
                        RQSTDT.Columns.Add("TRAY_ID");

                        DataRow dr = RQSTDT.NewRow();
                        RQSTDT.Rows.Add(dr);
                        RQSTDT.Rows[0]["TRAY_ID"] = dtData.Rows[i]["TRAY_ID"].ToString();


                        DataTable RSLTDT = new ClientProxy().ExecuteServiceSync("DA_GET_FCS_TRAY_INFO", "INDATA", "OUTDATA", RQSTDT);// //BR_PRD_GET_2ND_PALLET_LIST_NJ

                        if (RSLTDT.Rows.Count == 0)
                        {
                            DataTableConverter.SetValue(dgTargetList.Rows[i].DataItem, "CHK", false);
                            DataTableConverter.SetValue(dgTargetList.Rows[i].DataItem, "STAT_TXT", MessageDic.Instance.GetMessage("SFU2918"));   // 91SW 메시지다국어 처리 문의
                            DataTableConverter.SetValue(dgTargetList.Rows[i].DataItem, "STAT_CD", "-1");

                        }
                        else
                        {
                            DataTableConverter.SetValue(dgTargetList.Rows[i].DataItem, "TRAY_NO", RSLTDT.Rows[0]["TRAY_NO"]);
                            DataTableConverter.SetValue(dgTargetList.Rows[i].DataItem, "PROD_ID", RSLTDT.Rows[0]["PROD_ID"]);
                            DataTableConverter.SetValue(dgTargetList.Rows[i].DataItem, "CELL_CNT", RSLTDT.Rows[0]["CELL_CNT"]);
                            DataTableConverter.SetValue(dgTargetList.Rows[i].DataItem, "STAT_TXT", RSLTDT.Rows[0]["STAT_TXT"]);
                            DataTableConverter.SetValue(dgTargetList.Rows[i].DataItem, "STAT_CD", RSLTDT.Rows[0]["STAT_CD"]);
                        }

                    }
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        
        /// <summary>
        /// Comfirm
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnComfirm_Click(object sender, RoutedEventArgs e)
        {
            //SFU1716 : 실적확정 하시겠습니까?
            LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("SFU1716"), null, "Info", MessageBoxButton.OKCancel, MessageBoxIcon.None, (Result) =>
            {
                if (Result == MessageBoxResult.OK)
                {
                    try
                    {
                        DataTable dtData = DataTableConverter.Convert(dgTargetList.ItemsSource);
                        DataTable RQSTDT = new DataTable("INDATA");
                        //TRAY_NO, TRAY_ID, PRODID, EQPTID, EQSGID, USERID, CELL_CNT, AREAID
                        RQSTDT.Columns.Add("TRAY_NO");
                        RQSTDT.Columns.Add("TRAY_ID");
                        RQSTDT.Columns.Add("PRODID");
                        RQSTDT.Columns.Add("EQPTID");
                        RQSTDT.Columns.Add("EQSGID");
                        RQSTDT.Columns.Add("USERID");
                        RQSTDT.Columns.Add("CELL_CNT");
                        RQSTDT.Columns.Add("AREAID");

                        
                        if (dtData.Rows.Count == 0 )
                        {
                            // 10008 : 선택된 데이터가 없습니다.]
                            Util.MessageValidation("10008");
                            return;
                        }


                        //DataTable dtData2 = null;
                        for (int i = 0; i < dtData.Rows.Count; i++)
                        {

                            if (dtData.Rows[i]["CHK"].Equals("true"))
                            {
                                if (Convert.ToString(dtData.Rows[i]["STAT_CD"]) == "1")
                                {
                                    DataRow dr = RQSTDT.NewRow();
                                    //TRAY_NO, TRAY_ID, PRODID, EQPTID, EQSGID, USERID, CELL_CNT, AREAID
                                    dr["TRAY_NO"] = dtData.Rows[i]["TRAY_NO"];
                                    dr["TRAY_ID"] = dtData.Rows[i]["TRAY_ID"];
                                    dr["PRODID"] = dtData.Rows[i]["PROD_ID"];
                                    dr["EQPTID"] = LoginInfo.CFG_EQPT_ID;
                                    dr["EQSGID"] = LoginInfo.CFG_EQSG_ID;
                                    dr["USERID"] = LoginInfo.USERID;
                                    dr["CELL_CNT"] = dtData.Rows[i]["CELL_CNT"];
                                    dr["AREAID"] = LoginInfo.CFG_AREA_ID;
                                    RQSTDT.Rows.Add(dr);

                                    DataTable RSLTDT = new ClientProxy().ExecuteServiceSync("BR_CONFIRM_CELL_TO_PACK_NJ", "INDATA", "OUTDATA", RQSTDT);// //BR_PRD_GET_2ND_PALLET_LIST_NJ


                                    if (RSLTDT.Rows.Count == 0)
                                    {
                                        DataTableConverter.SetValue(dgTargetList.Rows[i].DataItem, "CHK", false);
                                        DataTableConverter.SetValue(dgTargetList.Rows[i].DataItem, "STAT_TXT", MessageDic.Instance.GetMessage("SFU2918"));   // 91SW 메시지다국어 처리 문의
                                        DataTableConverter.SetValue(dgTargetList.Rows[i].DataItem, "STAT_CD", "-1");
                                    }
                                    else
                                    {
                                        DataTableConverter.SetValue(dgTargetList.Rows[i].DataItem, "PALLET_ID", RSLTDT.Rows[0]["PALLET_ID"]);
                                        DataTableConverter.SetValue(dgTargetList.Rows[i].DataItem, "STAT_TXT", RSLTDT.Rows[0]["STAT_TXT"]);
                                        DataTableConverter.SetValue(dgTargetList.Rows[i].DataItem, "STAT_CD", RSLTDT.Rows[0]["STAT_CD"]);
                                    }
                                }
                            }
                        }
                        //DataView dvData = dtData2.DefaultView;
                        //dgTargetList.BeginEdit();
                        //dgTargetList.ItemsSource = DataTableConverter.Convert(dvData.ToTable());
                        //dgTargetList.EndEdit();
                    }
                    catch (Exception ex)
                    {
                        Util.MessageException(ex);
                    }
                    finally
                    {
                        loadingIndicator.Visibility = Visibility.Collapsed;
                    }
                }
            });

        }

        void checkAllLEFT_Checked(object sender, RoutedEventArgs e)
        {
            for (int i = 0; i < dgTargetList.Rows.Count; i++)
            {
                DataTableConverter.SetValue(dgTargetList.Rows[i].DataItem, "CHK", true);
            }

        }

        void checkAllLEFT_Unchecked(object sender, RoutedEventArgs e)
        {
            Util.DataGridCheckAllUnChecked(dgTargetList);
        }

        #endregion


        #region  radio 선택 이벤트
        private void dgPalletListChoice_Checked(object sender, RoutedEventArgs e)
        {
            if (sender == null)
                return;

            RadioButton rb = sender as RadioButton;

            if (rb.DataContext == null)
                return;


            if ((bool)rb.IsChecked && (rb.DataContext as DataRowView).Row["CHK"].ToString().Equals(bool.FalseString))
            {
                int idx = ((C1.WPF.DataGrid.DataGridCellPresenter)rb.Parent).Row.Index;
                DataRow dtRow = (rb.DataContext as DataRowView).Row;

                for (int i = 0; i < ((C1.WPF.DataGrid.DataGridCellPresenter)rb.Parent).DataGrid.Rows.Count; i++)
                {
                    if (idx == i)   // Mode = OneWay 이므로 Set 처리.
                        DataTableConverter.SetValue(((C1.WPF.DataGrid.DataGridCellPresenter)rb.Parent).DataGrid.Rows[i].DataItem, "CHK", true);
                    else
                        DataTableConverter.SetValue(((C1.WPF.DataGrid.DataGridCellPresenter)rb.Parent).DataGrid.Rows[i].DataItem, "CHK", false);
                }

            }
        }
        #endregion

        #region #. Main events
        /// <summary>
        /// Search
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                GetPalletList();
            }
            catch (Exception ex)
            {
                Util.Alert(ex.ToString());
            }

        }
        /// <summary>
        /// ComfirmCancel
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnComfirmCancel_Click(object sender, RoutedEventArgs e)
        {
            // SFU4263 : 실적 취소하시겠습니까?
            LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("SFU4263"), null, "Info", MessageBoxButton.OKCancel, MessageBoxIcon.None, (Result) =>
            {
                if (Result == MessageBoxResult.OK)
                {

                    try
                    {
                        DataTable dtData = DataTableConverter.Convert(grdMain.ItemsSource);

                        DataTable RQSTDT = new DataTable("INDATA");
                        RQSTDT.Columns.Add("BOXID");
                        RQSTDT.Columns.Add("USERID");


                        int idxPallet = util.GetDataGridCheckFirstRowIndex(grdMain, "CHK");



                        if (idxPallet < 0)
                        {
                            // 10008 : 선택된 데이터가 없습니다.]
                            Util.MessageValidation("10008");
                            return;
                        }

                        if (Util.NVC(grdMain.GetCell(idxPallet, grdMain.Columns["RCV_ISS_STAT_CODE"].Index).Value) == "SHIPPING")
                        {
                            //SFU3618 : 확정 상태만 확정 취소할 수 있습니다.
                            Util.MessageValidation("SFU3618");
                            return;
                        }
                        DataRow dr = RQSTDT.NewRow();
                        dr["BOXID"] = dtData.Rows[idxPallet]["PALLET_ID"];
                        dr["USERID"] = LoginInfo.USERID;
                        RQSTDT.Rows.Add(dr);

                        DataTable RSLTDT = new ClientProxy().ExecuteServiceSync("BR_PRD_REG_CANCEL_END_TRAY_NJ", "INDATA", "OUTDATA", RQSTDT);


                        if (RSLTDT.Rows.Count != 0)
                        {
                            //SFU2912 : 성공
                            Util.MessageValidation("SFU2912");
                            GetPalletList();
                        }
                        else
                        {
                            //PSS9144 : 취소실패
                            Util.MessageValidation("PSS9144");
                            

                        }


                        /*
                        //DataTable dtData2 = null;
                        for (int i = 0; i < dtData.Rows.Count; i++)
                        {

                            if (dtData.Rows[i]["CHK"].Equals("true"))
                            {
                                if (Convert.ToString(dtData.Rows[i]["RCV_ISS_STAT_CODE"]) == "SHIPPING")
                                {
                                    DataRow dr = RQSTDT.NewRow();
                                    dr["BOXID"] = dtData.Rows[i]["PALLET_ID"];
                                    dr["USERID"] = LoginInfo.USERID;
                                    RQSTDT.Rows.Add(dr);

                                    DataTable RSLTDT = new ClientProxy().ExecuteServiceSync("BR_PRD_REG_CANCEL_END_TRAY_NJ", "INDATA", "OUTDATA", RQSTDT);

                                    if (RSLTDT.Rows.Count == 0)
                                    {
                                        DataTableConverter.SetValue(dgTargetList.Rows[i].DataItem, "REMARK", RSLTDT.Rows[0]["RESULT"]);
                                        DataTableConverter.SetValue(dgTargetList.Rows[i].DataItem, "STAT_TXT", MessageDic.Instance.GetMessage("SFU2918"));   // 91SW 메시지다국어 처리 문의
                                        DataTableConverter.SetValue(dgTargetList.Rows[i].DataItem, "STAT_CD", "-1");
                                    }
                                    else
                                    {
                                        DataTableConverter.SetValue(dgTargetList.Rows[i].DataItem, "PALLET_ID", RSLTDT.Rows[0]["PALLET_ID"]);
                                        DataTableConverter.SetValue(dgTargetList.Rows[i].DataItem, "STAT_TXT", RSLTDT.Rows[0]["STAT_TXT"]);
                                        DataTableConverter.SetValue(dgTargetList.Rows[i].DataItem, "STAT_CD", RSLTDT.Rows[0]["STAT_CD"]);
                                    }
                                }
                            }
                        }
                        */


                    }
                    catch (Exception ex)
                    {
                        Util.MessageException(ex);
                    }
                    finally
                    {
                        loadingIndicator.Visibility = Visibility.Collapsed;
                    }
                }

            });
        }
        /// <summary>
        /// WHOut
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnWHOut_Click(object sender, RoutedEventArgs e)
        {
            LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("SFU2802"), null, "Info", MessageBoxButton.OKCancel, MessageBoxIcon.None, (Result) =>
            {
                if (Result == MessageBoxResult.OK)
                {

                    try
                    {
                        DataTable dtData = DataTableConverter.Convert(grdMain.ItemsSource);

                        DataTable RQSTDT = new DataTable("INDATA");
                        RQSTDT.Columns.Add("BOXID");
                        RQSTDT.Columns.Add("USERID");

                        int idxPallet = util.GetDataGridCheckFirstRowIndex(grdMain, "CHK");


                        if (idxPallet < 0)
                        {
                            // 10008 : 선택된 데이터가 없습니다.]
                            Util.MessageValidation("10008");
                            return;
                        }
                        if (Util.NVC(grdMain.GetCell(idxPallet, grdMain.Columns["RCV_ISS_STAT_CODE"].Index).Value) == "SHIPPING")
                        {
                            //SFU4416			
                            Util.MessageValidation("SFU4416");
                            return;
                        }

                        DataRow dr = RQSTDT.NewRow();
                        dr["BOXID"] = dtData.Rows[idxPallet]["PALLET_ID"];
                        dr["USERID"] = LoginInfo.USERID;
                        RQSTDT.Rows.Add(dr);

                        DataTable RSLTDT = new ClientProxy().ExecuteServiceSync("BR_PRD_REG_SHIPMENT_TRAY_NJ", "INDATA", "OUTDATA", RQSTDT);


                        if (RSLTDT.Rows.Count != 0)
                        {
                            //SFU1931 : 출고완료
                            Util.MessageValidation("SFU1931");
                            GetPalletList();
                        }
                        else
                        { 
                            
                        }

                    }
                    catch (Exception ex)
                    {
                        Util.MessageException(ex);
                    }
                    finally
                    {
                        loadingIndicator.Visibility = Visibility.Collapsed;
                    }
                }

            });
        }
        /// <summary>
        /// WHOutCancel
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnWHOutCancel_Click(object sender, RoutedEventArgs e)
        {

            LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("SFU2805"), null, "Info", MessageBoxButton.OKCancel, MessageBoxIcon.None, (Result) =>
            {
                if (Result == MessageBoxResult.OK)
                {

                    try
                    {
                        DataTable dtData = DataTableConverter.Convert(grdMain.ItemsSource);

                        DataTable RQSTDT = new DataTable("INDATA");
                        RQSTDT.Columns.Add("BOXID");
                        RQSTDT.Columns.Add("USERID");

                        int idxPallet = util.GetDataGridCheckFirstRowIndex(grdMain, "CHK");


                        if (idxPallet < 0)
                        {
                            // 10008 : 선택된 데이터가 없습니다.]
                            Util.MessageValidation("10008");
                            return;
                        }
                        if (Util.NVC(grdMain.GetCell(idxPallet, grdMain.Columns["RCV_ISS_STAT_CODE"].Index).Value) != "SHIPPING")
                        {
                            //SFU3717			
                            Util.MessageValidation("SFU3717");
                            return;
                        }

                        DataRow dr = RQSTDT.NewRow();
                        dr["BOXID"] = dtData.Rows[idxPallet]["PALLET_ID"];
                        dr["USERID"] = LoginInfo.USERID;
                        RQSTDT.Rows.Add(dr);
                        DataTable RSLTDT = new ClientProxy().ExecuteServiceSync("BR_PRD_REG_CANCEL_SHIPMENT_TRAY_NJ", "INDATA", "OUTDATA", RQSTDT);



                        if (RSLTDT.Rows.Count != 0)
                        {
                            // SFU3431 : 출고 취소 되었습니다.
                            Util.MessageValidation("SFU3431");
                            GetPalletList();
                        }
                        else
                        {
                            // SFU2806 : 출고 취소 에러
                            Util.MessageValidation("SFU2806");
                            
                        }
                    }
                    catch (Exception ex)
                    {
                        Util.MessageException(ex);
                    }
                    finally
                    {
                        loadingIndicator.Visibility = Visibility.Collapsed;
                    }
                }
            });
        }

        #endregion

        #endregion

        #region #. Member Function Lists...
        private void Initialize()
        {
            tbTagetListCount.Text = "[ 0 " + ObjectDic.Instance.GetObjectName("건") + " ]";
            tbSearchListCount.Text = "[ 0 " + ObjectDic.Instance.GetObjectName("건") + " ]";
        }
        
        public void Scan(string sPalletID)
        {
            try
            {
                DataTable dtData = null;
                if (dgTargetList.GetRowCount() == 0)
                    dtData = initTable();
                else           
                    dtData  = DataTableConverter.Convert(dgTargetList.ItemsSource);
                dtData.AcceptChanges();
                DataRow newRow = dtData.NewRow();
                newRow["TRAY_ID"] = txtPalletID.Text.Trim();
                newRow["CHK"] = "true";
                dtData.Rows.Add(newRow);
                DataView dvData = dtData.DefaultView;
                dgTargetList.BeginEdit();
                dgTargetList.ItemsSource = DataTableConverter.Convert(dvData.ToTable());
                dgTargetList.EndEdit();
                txtPalletID.Focus();
                txtPalletID.Clear();
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                txtPalletID.Focus();
            }
        }

        public DataTable initTable()
        {
            DataTable dt = new DataTable();
            dt.Columns.Add("CHK", typeof(string));
            dt.Columns.Add("PALLET_ID", typeof(string));
            dt.Columns.Add("TRAY_ID", typeof(string));
            dt.Columns.Add("TRAY_NO", typeof(string));
            dt.Columns.Add("PROD_ID", typeof(string));
            dt.Columns.Add("CELL_CNT", typeof(string));
            dt.Columns.Add("STAT_TXT", typeof(string));
            dt.Columns.Add("STAT_CD", typeof(string));
            return dt;
        }

        public DataTable initTable2()
        {
            DataTable dt = new DataTable();
            dt.Columns.Add("CHK", typeof(string));
            dt.Columns.Add("PALLET_ID", typeof(string));
            dt.Columns.Add("TRAY_ID", typeof(string));
            dt.Columns.Add("TRAY_NO", typeof(string));
            dt.Columns.Add("PROJECT", typeof(string));//
            dt.Columns.Add("PRODID", typeof(string));
            dt.Columns.Add("PRODNAME", typeof(string));//
            dt.Columns.Add("TOTAL_QTY", typeof(string));//
            dt.Columns.Add("SHIPTO_ID", typeof(string));//
            dt.Columns.Add("BOXSTAT_NAME", typeof(string));//
            dt.Columns.Add("BOXSTAT", typeof(string));//
            dt.Columns.Add("RCV_ISS_STAT_NAME", typeof(string));//
            dt.Columns.Add("RCV_ISS_STAT_CODE", typeof(string));//
            dt.Columns.Add("AREAID", typeof(string));//
            dt.Columns.Add("EQSGID", typeof(string));//
            dt.Columns.Add("EQPTID", typeof(string));//
            dt.Columns.Add("RCV_ISS_ID", typeof(string));//   
            return dt;
        }

        #region #. Retrieve data to grdMain

        /// <summary>
        /// 작업 팔레트 리스트
        /// BIZ : BR_PRD_GET_SHIP_PALLET_LIST_NJ
        /// </summary>
        private void GetPalletList(int idx = -1)
        {
            try
            {
                DataTable RQSTDT = new DataTable("INDATA");
                RQSTDT.Columns.Add("LANGID");
                RQSTDT.Columns.Add("AREAID");
                RQSTDT.Columns.Add("EQSGID");
                RQSTDT.Columns.Add("BOXID");
                RQSTDT.Columns.Add("PKG_LOTID");
                RQSTDT.Columns.Add("BOXSTAT_LIST");
                

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["AREAID"] = LoginInfo.CFG_AREA_ID; //Util.NVC(cboArea.SelectedValue);
                dr["EQSGID"] = Util.NVC(cboLine.SelectedValue);
                
                if (!string.IsNullOrWhiteSpace(txtPalletIDMain.Text))
                {
                    dr["BOXID"] = txtPalletIDMain.Text != "" ? Util.NVC(txtPalletIDMain.Text) : null; 
                    dr["BOXSTAT_LIST"] = string.IsNullOrEmpty(_searchStat) ? _searchStat : _searchStat.Remove(_searchStat.Length - 1);
                }
                else
                {
                    dr["BOXID"] = null;
                    dr["PKG_LOTID"] = null;
                    dr["BOXSTAT_LIST"] = string.IsNullOrEmpty(_searchStat) ? _searchStat : _searchStat.Remove(_searchStat.Length - 1);
                }

                RQSTDT.Rows.Add(dr);

                DataTable RSLTDT = new ClientProxy().ExecuteServiceSync("BR_PRD_GET_2ND_PALLET_LIST_TO_PACK_NJ", "INDATA", "OUTDATA", RQSTDT);// //BR_PRD_GET_2ND_PALLET_LIST_NJ

                if (!RSLTDT.Columns.Contains("CHK"))
                    RSLTDT = util.gridCheckColumnAdd(RSLTDT, "CHK");

                Util.GridSetData(grdMain, RSLTDT, FrameOperation, true);
                if (idx != -1)
                {
                    DataTableConverter.SetValue(grdMain.Rows[idx].DataItem, "CHK", true);
                    grdMain.SelectedIndex = idx;
                    grdMain.ScrollIntoView(idx, 0);

                }
                else
                {
                    grdMain.SelectedIndex = -1;
                }

                
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
            finally
            {
                loadingIndicator.Visibility = Visibility.Collapsed;
            }
        }

        #endregion

        #region #. Set combo
        /// <summary>
        /// 
        /// </summary>
        /// <param name="cbo"></param>
        private void InitCombo()
        {
            CommonCombo _combo = new CommonCombo();
            _combo.SetCombo(cboLine, CommonCombo.ComboStatus.SELECT, sFilter: new string[] { LoginInfo.CFG_SHOP_ID, "MCC,MCR,MCS", Process.CELL_BOXING, LoginInfo.CFG_AREA_ID }, sCase: "LINEBYSHOP");
            //_combo.SetCombo(cboArea, CommonCombo.ComboStatus.SELECT, sCase: "AREA_CP");
            //_combo.SetCombo(cboShipTo, CommonCombo.ComboStatus.SELECT, sFilter: new string[] { LoginInfo.CFG_SHOP_ID, null, null }, sCase: "SHIPTO_CP");


            //_combo.SetCombo(cboExpDom_DETL, CommonCombo.ComboStatus.SELECT, sFilter: new string[] { "EXP_DOM_TYPE_CODE" }, sCase: "COMMCODE_WITHOUT_CODE");
        }

        #endregion



        #region 체크박스 처리
        private void chkSearch_Checked(object sender, RoutedEventArgs e)
        {
            CheckBox chk = sender as CheckBox;
            switch (chk.Name)
            {
                case "chkCONFIRM":
                    _searchStat += PACKED;
                    break;
                case "chkSHIPPING":
                    _searchStat += SHIPPING;
                    break;
                default:
                    break;
            }
        }
        private void chkSearch_Unchecked(object sender, RoutedEventArgs e)
        {
            CheckBox chk = sender as CheckBox;
            switch (chk.Name)
            {
                case "chkCONFIRM":
                    if (_searchStat.Contains(PACKED))
                        _searchStat = _searchStat.Replace(PACKED, "");
                    break;
                case "chkSHIPPING":
                    //_rcvStat += SHIPPING;
                    if (_searchStat.Contains(SHIPPING))
                        _searchStat = _searchStat.Replace(SHIPPING, "");
                    break;
                default:
                    break;
            }
        }
        #endregion

        private void DoEvents()
        {
            Application.Current.Dispatcher.Invoke(DispatcherPriority.Background, new System.Threading.ThreadStart(delegate { }));
        }

        #endregion

    }
}