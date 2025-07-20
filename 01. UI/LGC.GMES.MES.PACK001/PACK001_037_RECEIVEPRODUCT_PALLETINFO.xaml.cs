/*************************************************************************************
 Created Date : 2019.01.28
      Creator : 손우석
   Decription : 
--------------------------------------------------------------------------------------
 [Change History]
 2019.01.28 손우석 CSR ID 3870223 법인간 자동차 Cell이송 프로세스 구축 요청 건 [요청번호]C20181213_70223
 2021.11.17 김용준 IWMS 반품 개선 건
**************************************************************************************/

using System;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using C1.WPF;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;

namespace LGC.GMES.MES.PACK001
{
    public partial class PACK001_037_RECEIVEPRODUCT_PALLETINFO : C1Window, IWorkArea
    {
        #region Declaration & Constructor 
        LGC.GMES.MES.PACK001.Class.MESSAGE_PARAM ms = new LGC.GMES.MES.PACK001.Class.MESSAGE_PARAM();

        DataTable dtCellListResult;
        String sPRODID = string.Empty;
        String sTAPMENU = string.Empty;

        /// <summary>
        ///  Frame과 상호작용하기 위한 객체
        /// </summary>
        public IFrameOperation FrameOperation
        {
            get;
            set;
        }

        public PACK001_037_RECEIVEPRODUCT_PALLETINFO()
        {
            InitializeComponent();
        }

        Util util = new Util();
        #endregion

        #region Initialize

        #endregion

        #region Event

        private void C1Window_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                object[] tmps = C1WindowExtension.GetParameters(this);
                if (tmps != null)
                {
                    DataTable dtText = tmps[0] as DataTable;
                    
                    string sRcvIssId = Util.NVC(tmps[0]);
                    string sPalletId = Util.NVC(tmps[1]);
                    sTAPMENU = Util.NVC(tmps[2]);
                    txtPallet.Text = sRcvIssId;
                    txtPallet.Tag = sRcvIssId;

                    if (txtPallet.Text.Length > 0)
                    {
                        //2019.01.10
                        //getPalletInfo(null, sPalletId,null);
                        //getPalletInfo(sRcvIssId, sPalletId, null);

                        //getPalletInfo_CellList(sRcvIssId,sPalletId);
                        if (sTAPMENU.Equals("dgSearchResultList2"))
                        {
                            this.Header = ObjectDic.Instance.GetObjectName("IWMS 재입고 정보");

                            get_IWMS_PalletInfo(sRcvIssId, sPalletId, null);
                            getPalletInfo_IWMS_CellList(sPalletId);
                        }
                        else
                        {
                            this.Header = ObjectDic.Instance.GetObjectName("반품정보");

                            getPalletInfo(sRcvIssId, null, null);

                            getPalletInfo_CellList(sRcvIssId, null);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Util.Alert(ex.ToString());
            }
        }

        #region Button
        private void btnEcel_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                new LGC.GMES.MES.Common.ExcelExporter().Export(dgSearchResultList);
            }
            catch (Exception ex)
            {
                Util.Alert(ex.ToString());
            }
        }

        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (txtPallet.Text.Length > 0)
                {
                        getPalletInfo();
                }
            }
            catch (Exception ex)
            {
                Util.Alert(ex.ToString());
            }
        }

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = MessageBoxResult.Cancel;
            this.Close();
        }

        private void btnExpandFrame_Checked(object sender, RoutedEventArgs e)
        {
            if (gdContent != null)
            {
                gdContent.ColumnDefinitions[1].Width = new GridLength(8);
                LGC.GMES.MES.CMM001.GridLengthAnimation gla = new LGC.GMES.MES.CMM001.GridLengthAnimation();
                gla.From = new GridLength(0);
                gla.To = new GridLength(1);
                gla.Duration = new TimeSpan(0, 0, 0, 0, 500);
                gdContent.ColumnDefinitions[0].BeginAnimation(ColumnDefinition.WidthProperty, gla);
            }
        }

        private void btnExpandFrame_Unchecked(object sender, RoutedEventArgs e)
        {
            if (gdContent != null)
            {
                gdContent.ColumnDefinitions[1].Width = new GridLength(0);
                LGC.GMES.MES.CMM001.GridLengthAnimation gla = new LGC.GMES.MES.CMM001.GridLengthAnimation();
                gla.From = new GridLength(1, GridUnitType.Star);
                gla.To = new GridLength(0, GridUnitType.Star); ;
                gla.Duration = new TimeSpan(0, 0, 0, 0, 500);
                gdContent.ColumnDefinitions[0].BeginAnimation(ColumnDefinition.WidthProperty, gla);
            }
        }

        #endregion Button

        #region Text
        private void txtPallet_KeyDown(object sender, KeyEventArgs e)
        {
            try
            {
                if (e.Key == Key.Enter)
                {
                    chkInputYn.IsChecked = false;
                    getPalletInfo();
                }
            }
            catch (Exception ex)
            {
                Util.Alert(ex.ToString());
            }
        }

        #endregion Text

        #region Combo
       

        #endregion Combo

        #endregion Event

        #region Mehod

        private void getPalletInfo()
        {
            try
            {
                if ((bool)rdoCell.IsChecked)
                {
                    getPalletInfo(null, null, txtPallet.Text);
                }
                else
                {
                    if (sTAPMENU.Equals("dgSearchResultList2"))
                    {
                        get_IWMS_PalletInfo(null, txtPallet.Text, null);
                    }
                    else
                    {
                        getPalletInfo(null, txtPallet.Text, null);                                                                            
                    }
                        
                }

                string sRcvID = txtInfoRcvIssID.Text.Length > 0 ? txtInfoRcvIssID.Text : null;
                string sBoxID = txtInfoBoxID.Text.Length > 0 ? txtInfoBoxID.Text : null;

                if (sRcvID != null && sBoxID != null)
                {
                    if (sTAPMENU.Equals("dgSearchResultList2"))
                    {
                        getPalletInfo_IWMS_CellList(sRcvID);
                    }
                    else
                    {
                        getPalletInfo_CellList(sRcvID, sBoxID);
                    }                                            
                }
                else
                {
                    dgSearchResultList.ItemsSource = null;
                    Util.SetTextBlockText_DataGridRowCount(tbCellListCount, Util.NVC(0));
                }
            }
            catch (Exception ex)
            {
                Util.Alert(ex.ToString());
            }
        }

        private void getPalletInfo_CellList(string sRCV_ISS_ID , string sPalletID)
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("RCV_ISS_ID", typeof(string));
                RQSTDT.Columns.Add("PALLETID", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["RCV_ISS_ID"] = sRCV_ISS_ID;
                dr["PALLETID"] = sPalletID;
                RQSTDT.Rows.Add(dr);

                dtCellListResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_TB_SFC_IWMS_PLLT_DTL", "RQSTDT", "RSLTDT", RQSTDT);

                if (dtCellListResult != null)
                {
                    if (dtCellListResult.Rows.Count > 0)
                    {
                        sPRODID = dtCellListResult.Rows[0]["PRODID"].ToString();

                        //setComboBox_Model_schd(sPRODID);
                    }
                    else
                    {
                        sPRODID = string.Empty;
                    }
                }
                else
                {
                    sPRODID = string.Empty;
                }

                Util.GridSetData(dgSearchResultList, dtCellListResult, FrameOperation, true);

                Util.SetTextBlockText_DataGridRowCount(tbCellListCount, Util.NVC(dtCellListResult.Rows.Count));
            }
            catch(Exception ex)
            {
                Util.MessageException(ex);
            }

        }

        private void getPalletInfo_IWMS_CellList(string sRCV_ISS_ID)
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("RCV_ISS_ID", typeof(string));
                

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["RCV_ISS_ID"] = sRCV_ISS_ID;
                
                RQSTDT.Rows.Add(dr);

                dtCellListResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_TB_SFC_IWMS_RETURN_PLLT_DTL", "RQSTDT", "RSLTDT", RQSTDT);

                if (dtCellListResult != null)
                {
                    if (dtCellListResult.Rows.Count > 0)
                    {
                        sPRODID = dtCellListResult.Rows[0]["PRODID"].ToString();

                        //setComboBox_Model_schd(sPRODID);
                    }
                    else
                    {
                        sPRODID = string.Empty;
                    }
                }
                else
                {
                    sPRODID = string.Empty;
                }

                Util.GridSetData(dgSearchResultList, dtCellListResult, FrameOperation, true);

                Util.SetTextBlockText_DataGridRowCount(tbCellListCount, Util.NVC(dtCellListResult.Rows.Count));
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }

        }

        private void getPalletInfo(string sRCV_ISS_ID, string sPalletID , string sCell)
        {
            try
            {
                ClearBoxInfo();

                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("RCV_ISS_ID", typeof(string));
                RQSTDT.Columns.Add("PALLETID", typeof(string));
                RQSTDT.Columns.Add("CELLID", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["RCV_ISS_ID"] = sRCV_ISS_ID;
                dr["PALLETID"] = sPalletID;
                dr["CELLID"] = sCell;
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_TB_SFC_IWMS_PLLT_DTL_INFO", "RQSTDT", "RSLTDT", RQSTDT);

                if (dtResult != null)
                {
                    if(dtResult.Rows.Count > 0)
                    {
                        txtInfoRcvIssID.Text = Util.NVC(dtResult.Rows[0]["IWMS_RCV_ID"]);
                        txtInfoBoxID.Text = Util.NVC(dtResult.Rows[0]["PLLT_ID"]);
                        txtInfoProductID.Text = Util.NVC(dtResult.Rows[0]["PRODID"]);
                        txtInfoBoxState.Text = Util.NVC(dtResult.Rows[0]["LOT_STAT_CODE_NAME"]);
                        txtInfoCellQty.Text = Util.NVC(dtResult.Rows[0]["RCV_COUNT"]);
                        txtInfoBoxFromArea.Text = Util.NVC(dtResult.Rows[0]["FROM_AREA_NAME"]);
                        txtBoxFromSloc.Text = Util.NVC(dtResult.Rows[0]["FROM_SLOC_NAME"]);
                        txtBoxToSloc.Text = Util.NVC(dtResult.Rows[0]["TO_SLOC_NAME"]);
                        txtBoxToLine.Text = Util.NVC(dtResult.Rows[0]["EQSGNAME"]);
                        txtBoxFromDate.Text = Util.NVC(dtResult.Rows[0]["INSDTTM"]);
                        txtBoxToDate.Text = Util.NVC(dtResult.Rows[0]["RCV_DTTM"]);
                        txtInfoOcvFlag.Text = Util.NVC(dtResult.Rows[0]["OCV_FLAG"]);
                        txtProdLine.Text = Util.NVC(dtResult.Rows[0]["PROD_EQSGNAME"]);
                    }
                    else
                    {
                        ClearBoxInfo();
                    }
                }
                
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }

        }

        private void get_IWMS_PalletInfo(string sRCV_ISS_ID, string sPalletID, string sCell)
        {
            try
            {
                ClearBoxInfo();
                rdoCell.Visibility = Visibility.Collapsed;
                txtRcvName.Text = "입고출고ID";
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("RCV_ISS_ID", typeof(string));
                RQSTDT.Columns.Add("PALLETID", typeof(string));
                RQSTDT.Columns.Add("CELLID", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["RCV_ISS_ID"] = sRCV_ISS_ID;
                dr["PALLETID"] = sPalletID;
                dr["CELLID"] = sCell;
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_TB_SFC_IWMS_RETURN_PLLT_DTL_INFO", "RQSTDT", "RSLTDT", RQSTDT);

                if (dtResult != null)
                {
                    if (dtResult.Rows.Count > 0)
                    {
                        txtPallet.Text = Util.NVC(dtResult.Rows[0]["PLLT_ID"]);
                        txtInfoRcvIssID.Text = Util.NVC(dtResult.Rows[0]["PLLT_ID"]);
                        txtInfoBoxID.Text = Util.NVC(dtResult.Rows[0]["PLLT_ID"]);
                        txtInfoProductID.Text = Util.NVC(dtResult.Rows[0]["PRODID"]);
                        txtInfoBoxState.Text = Util.NVC(dtResult.Rows[0]["LOT_STAT_CODE_NAME"]);
                        txtInfoCellQty.Text = Util.NVC(dtResult.Rows[0]["RCV_COUNT"]);
                        txtInfoBoxFromArea.Text = Util.NVC(dtResult.Rows[0]["FROM_AREA_NAME"]);
                        txtBoxFromSloc.Text = Util.NVC(dtResult.Rows[0]["FROM_SLOC_NAME"]);
                        txtBoxToSloc.Text = Util.NVC(dtResult.Rows[0]["TO_SLOC_NAME"]);
                        txtBoxToLine.Text = Util.NVC(dtResult.Rows[0]["EQSGNAME"]);
                        txtBoxFromDate.Text = Util.NVC(dtResult.Rows[0]["INSDTTM"]);
                        txtBoxToDate.Text = Util.NVC(dtResult.Rows[0]["RCV_DTTM"]);
                        txtInfoOcvFlag.Text = Util.NVC(dtResult.Rows[0]["OCV_FLAG"]);
                        txtProdLine.Text = Util.NVC(dtResult.Rows[0]["PROD_EQSGNAME"]);
                    }
                    else
                    {
                        ClearBoxInfo();
                    }
                }

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }

        }


        private void ClearBoxInfo()
        {
            txtInfoRcvIssID.Text = "";
            txtInfoBoxID.Text = "";
            txtInfoProductID.Text = "";
            txtInfoBoxState.Text = "";
            txtInfoCellQty.Text = "";
            txtInfoBoxFromArea.Text = "";
            txtBoxFromSloc.Text = "";
            txtBoxToSloc.Text = "";
            txtBoxToLine.Text = "";
            txtBoxFromDate.Text = "";
            txtBoxToDate.Text = "";
            txtInfoOcvFlag.Text = "";
            txtProdLine.Text = "";
        }

        #endregion Method

        private void chkInputYn_Checked(object sender, RoutedEventArgs e)
        {
            if(dtCellListResult == null)
            {
                return;
            }

            if(dtCellListResult.Rows.Count == 0 || dtCellListResult == null)
            {
                return;
            }

            DataTable dtBind = makeBindTable();

            dgSearchResultList.ItemsSource = null;           
            Util.SetTextBlockText_DataGridRowCount(tbCellListCount, Util.NVC(0));
            
            DataRow[] drr = dtCellListResult.Select("INPUT_STAT = 'N' "); //잔량 cell만 가져옴

            foreach(DataRow dr in drr)
            {
                DataRow drBind = dtBind.NewRow();
                drBind["CHK"] = dr["CHK"].ToString();
                drBind["IWMS_RCV_ID"] = dr["IWMS_RCV_ID"].ToString();
                drBind["PLLT_ID"] = dr["PLLT_ID"].ToString();
                drBind["LOTID"] = dr["LOTID"].ToString();
                drBind["MAGAZINEID"] = dr["MAGAZINEID"].ToString();                
                drBind["PRODID"] = dr["PRODID"].ToString();
                drBind["MODLID"] = dr["MODLID"].ToString();
                drBind["MODLNAME"] = dr["MODLNAME"].ToString();
                drBind["PROD_SCHD_MODLID"] = dr["PROD_SCHD_MODLID"].ToString();
                drBind["PROD_SCHD_MODLNAME"] = dr["PROD_SCHD_MODLNAME"].ToString();
                drBind["OCV1"] = dr["OCV1"].ToString();
                drBind["OCV1DTTM"] = dr["OCV1DTTM"].ToString();
                drBind["OCV3"] = dr["OCV3"].ToString();
                drBind["OCV3DTTM"] = dr["OCV3DTTM"].ToString();
                drBind["SOCV"] = dr["SOCV"].ToString();
                drBind["SOCVDTTM"] = dr["SOCVDTTM"].ToString();
                drBind["MOVE_PERIOD"] = dr["MOVE_PERIOD"].ToString();
                drBind["INPUT_STAT"] = dr["INPUT_STAT"].ToString();
               
                dtBind.Rows.Add(drBind);
            }   

            Util.GridSetData(dgSearchResultList, dtBind, FrameOperation, true);
            Util.SetTextBlockText_DataGridRowCount(tbCellListCount, Util.NVC(dtBind.Rows.Count));
        }

        private DataTable makeBindTable()
        {
            DataTable dt = new DataTable();
            dt.Columns.Add("CHK", typeof(string));
            dt.Columns.Add("IWMS_RCV_ID", typeof(string));
            dt.Columns.Add("PLLT_ID", typeof(string));
            dt.Columns.Add("LOTID", typeof(string));
            dt.Columns.Add("MAGAZINEID", typeof(string));        
            dt.Columns.Add("PRODID", typeof(string));
            dt.Columns.Add("MODLID", typeof(string));
            dt.Columns.Add("MODLNAME", typeof(string));
            dt.Columns.Add("PROD_SCHD_MODLID", typeof(string));
            dt.Columns.Add("PROD_SCHD_MODLNAME", typeof(string));
            dt.Columns.Add("OCV1", typeof(string));
            dt.Columns.Add("OCV1DTTM", typeof(string));
            dt.Columns.Add("OCV3", typeof(string));
            dt.Columns.Add("OCV3DTTM", typeof(string));
            dt.Columns.Add("SOCV", typeof(string));
            dt.Columns.Add("SOCVDTTM", typeof(string));
            dt.Columns.Add("MOVE_PERIOD", typeof(string));
            dt.Columns.Add("INPUT_STAT", typeof(string));           

            return dt;
        }

        private void chkInputYn_Unchecked(object sender, RoutedEventArgs e)
        {
            if (dtCellListResult == null)
            {
                return;
            }

            if (dtCellListResult.Rows.Count == 0 || dtCellListResult == null)
            {
                return;
            }

            DataTable dtBind = new DataTable();
            dgSearchResultList.ItemsSource = null;
            Util.SetTextBlockText_DataGridRowCount(tbCellListCount, Util.NVC(0));

            dtBind = dtCellListResult;
           

            Util.GridSetData(dgSearchResultList, dtBind, FrameOperation, true);
            Util.SetTextBlockText_DataGridRowCount(tbCellListCount, Util.NVC(dtBind.Rows.Count));
        }
        
    }
}
