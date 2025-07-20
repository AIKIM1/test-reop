/*************************************************************************************
 Created Date : 2022.10.28
      Creator : 이태규
   Decription : 자재 재고실사(Box)
--------------------------------------------------------------------------------------
 [Change History]
  2022.10.28  이태규 : Initial Created.
  2024.05.24  윤지해 (수정사항 없음) NERP 대응 프로젝트-차수마감 취소 등 개발 범위에서 제외(사용안함 처리)
***************************************************************************************/

using C1.WPF;
using C1.WPF.DataGrid;
using C1.WPF.Excel;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.CMM001.Extensions;
using LGC.GMES.MES.COM001;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using LGC.GMES.MES.PACK001.Class;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;
using System.Windows.Media.Animation;
using System.Windows.Input;

namespace LGC.GMES.MES.PACK001
{
    public partial class PACK003_037 : UserControl, IWorkArea
    {
        #region Declaration & Constructor 
        Util _Util = new Util();

        DataTable dtCompareDetail = new DataTable();
        DataTable dtCompareNotRsltDetail = new DataTable();
        DataTable dtCompareNotSnapDetail = new DataTable();

        DataTable dtDiffDetail = new DataTable();
        DataTable dtDiffNotRsltDetail = new DataTable();
        DataTable dtDiffNotSnapDetail = new DataTable();

        DataTable dtExclude = new DataTable();
        string sExclude = string.Empty;

        private PACK003_037_DataHelper dataHelper = new PACK003_037_DataHelper();
        LGC.GMES.MES.PACK001.Class.MESSAGE_PARAM ms = new LGC.GMES.MES.PACK001.Class.MESSAGE_PARAM();
        int cboLineCount = 0;
        int cboMtrlRackCount = 0;

        public PACK003_037()
        {
            InitializeComponent();
            InitCombo();
        }

        public IFrameOperation FrameOperation
        {
            get;
            set;
        }

        CommonCombo _combo = new CommonCombo();

        private string _sEqsgID = string.Empty;
        private string _sProcID = string.Empty;
        private string _sProdID = string.Empty;
        private string _sElecType = string.Empty;
        private string _sPrjtName = string.Empty;
        private string _sAutoWhStckFlag = string.Empty;
        private string _sStckAdjFlag = string.Empty;
        private string _sStckAdjDiffFlag = string.Empty;

        private const string _sLOTID = "LOTID";
        private const string _sBOXID = "BOXID";

        DataView _dvSTCKCNT { get; set; }

        string _sSTCK_CNT_CMPL_FLAG = string.Empty;

        #endregion

        #region Initialize
        private void InitCombo()
        {
            PackCommon.SetMultiSelectionComboBox(this.dataHelper.GetLineData(), this.cboLine, ref this.cboLineCount, LoginInfo.CFG_EQSG_ID);
            Util.gridClear(grdMainTp1);

            PackCommon.SetMultiSelectionComboBox(this.dataHelper.GetLineData(), this.cboLine2, ref this.cboLineCount, LoginInfo.CFG_EQSG_ID);
            Util.gridClear(grdMainTp2);

            PackCommon.SetMultiSelectionComboBox(this.dataHelper.GetLineData(), this.cboLine3, ref this.cboLineCount, LoginInfo.CFG_EQSG_ID);
            Util.gridClear(grdMainTp3);


            SetcboSeqNo(cboSeqNoTp1, dpStckCntYmTp1);

            SetcboSeqNo(cboSeqNoTp2, dpStckCntYmTp2);

            SetcboSeqNo(cboSeqNoTp3, dpStckCntYmTp3);

            setUseYN();

            makeDetailTypeCombo();
        }

        private void cboLine_SelectionChanged(object sender, EventArgs e)
        {
            string sTemp = Util.NVC(cboLine.SelectedItemsToString);

            PackCommon.SetMultiSelectionComboBox(this.dataHelper.GetRackData(sTemp), this.cboMtrlPort, ref this.cboMtrlRackCount);
        }

        private void cboLine2_SelectionChanged(object sender, EventArgs e)
        {
            string sTemp = Util.NVC(cboLine2.SelectedItemsToString);

            PackCommon.SetMultiSelectionComboBox(this.dataHelper.GetRackData(sTemp), this.cboMtrlPort2, ref this.cboMtrlRackCount);
        }

        private void cboLine3_SelectionChanged(object sender, EventArgs e)
        {
            string sTemp = Util.NVC(cboLine3.SelectedItemsToString);

            PackCommon.SetMultiSelectionComboBox(this.dataHelper.GetRackData(sTemp), this.cboMtrlPort3, ref this.cboMtrlRackCount);
        }

        #region < 콤보 생성 >
        //제외유무 Combo 설정
        private void setUseYN()
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
                dr["CBO_NAME"] = "Y : " + ObjectDic.Instance.GetObjectName("Y");
                dr["CBO_CODE"] = "Y";
                dt.Rows.Add(dr);

                DataRow dr1 = dt.NewRow();
                dr1["CBO_NAME"] = "N : " + ObjectDic.Instance.GetObjectName("N");
                dr1["CBO_CODE"] = "N";
                dt.Rows.Add(dr1);

                dt.AcceptChanges();

                cboExcludeYNTp1.ItemsSource = DataTableConverter.Convert(dt);
                cboExcludeYNTp1.SelectedIndex = 0; //default Y

                cboExcludeYNTp2.ItemsSource = DataTableConverter.Convert(dt);
                cboExcludeYNTp2.SelectedIndex = 0; //default Y
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private void makeDetailTypeCombo()
        {
            try
            {
                DataTable dt = new DataTable();
                dt.Columns.Add("CBO_NAME", typeof(string));
                dt.Columns.Add("CBO_CODE", typeof(string));

                DataRow newRow = dt.NewRow();
                newRow["CBO_NAME"] = "ALL";
                newRow["CBO_CODE"] = "ALL";
                dt.Rows.Add(newRow);

                newRow = dt.NewRow();
                newRow["CBO_NAME"] = ObjectDic.Instance.GetObjectName("전산재고 대비 차이");
                newRow["CBO_CODE"] = "NOT_SNAP";
                dt.Rows.Add(newRow);

                newRow = dt.NewRow();
                newRow["CBO_NAME"] = ObjectDic.Instance.GetObjectName("실물재고 대비 차이");
                newRow["CBO_CODE"] = "NOT_RSLT";
                dt.Rows.Add(newRow);

                dt.AcceptChanges();

                cboDetailTypeCompare.ItemsSource = DataTableConverter.Convert(dt);
                cboDetailTypeCompare.SelectedIndex = 0; //default Y
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private void SetcboSeqNo(C1ComboBox SeqNo, LGC.GMES.MES.ControlsLibrary.LGCDatePicker date)
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                DataTable dtResult = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("STCK_CNT_YM", typeof(string));

                DataRow drn = RQSTDT.NewRow();
                drn["LANGID"] = LoginInfo.LANGID;
                drn["STCK_CNT_YM"] = date.SelectedDateTime.ToString("yyyyMM");

                if (date.SelectedDateTime.ToString("yyyy") == "0001")
                    return;

                RQSTDT.Rows.Add(drn);

                dtResult = new ClientProxy().ExecuteServiceSync("DA_MTRL_SEL_STOCKCNT_CBO", "RQSTDT", "RSLTDT", RQSTDT);

                SeqNo.ItemsSource = DataTableConverter.Convert(dtResult);
                //cboEqsg.IsEnabled = false;
                SeqNo.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        #endregion



        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            //// 전산재고 Tab
            //// SetEqsgCombo(mboEqsgShot, cboAreaShot);
            //// SetModelMultiCombo(mboModelShot, mboEqsgShot, cboAreaShot);
            //SetWhId(mboSnapWhId, cboAreaShot);
            //SetRackId(mboSnapRackId, mboSnapWhId, cboAreaShot);

            //// 실물재고실사 Tab
            //// SetEqsgCombo(mboEqsgRslt, cboAreaRslt);
            //// SetModelMultiCombo(mboModelRslt, mboEqsgRslt, cboAreaRslt);
            //SetWhId(mboRsltWhId, cboRsltArea);
            //SetRackId(mboRsltRackId, mboRsltWhId, cboRsltArea);
            this.Loaded -= UserControl_Loaded;
        }
        #endregion

        private void wndSTOCKCNT_START_Closed(object sender, EventArgs e)
        {
            try
            {
                //PACK003_037_POPUP window = sender as PACK003_037_POPUP;
                //if (window.DialogResult == MessageBoxResult.OK)
                //{
                //    _combo.SetCombo(cboStockSeqShot);
                //    _combo.SetCombo(cboStockSeqUpload);
                //    _combo.SetCombo(cboStockSeqCompare);
                //    _combo.SetCombo(cboStockSeqDiff);

                //    Util.gridClear(dgListShot);
                //    Util.gridClear(dgListStock);
                //    Util.gridClear(dgListCompare);
                //    Util.gridClear(dgListCompareDetail);
                //    Util.gridClear(dgListDiff);

                //    SetListShot();
                //}
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void btnSearchTp1_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(this.cboLine.SelectedItems.ToString()))
            {
                //SFU1223 : 라인을 선택하세요.
                Util.MessageInfo("SFU1223");
                return;
            }

            this.SearchProcess(this.grdMainTp1, "DA_MTRL_SEL_PROD_RACK_MTRL_BOX_STCK_CNT_SNAP", "RQSTDT", "RSLTDT");
        }

        private void btnSearchTp2_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(this.cboLine2.SelectedItems.ToString()))
            {
                //SFU1223 : 라인을 선택하세요.
                Util.MessageInfo("SFU1223");
                return;
            }

            this.SearchProcess(this.grdMainTp2, "DA_MTRL_SEL_PROD_RACK_MTRL_BOX_STCK_CNT_RSLT", "RQSTDT", "RSLTDT");
        }

        private void btnSearchTp3_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(this.cboLine2.SelectedItems.ToString()))
            {
                //SFU1223 : 라인을 선택하세요.
                Util.MessageInfo("SFU1223");
                return;
            }

            this.SearchProcess(this.grdMainTp3, "DA_MTRL_SEL_PROD_RACK_MTRL_BOX_STCK_CNT_DIF", "RQSTDT", "RSLTDT");

            Util.gridClear(grdDetailTp3);
            //this.SearchProcess(this.grdDetailTp3, "DA_MTRL_SEL_PROD_RACK_MTRL_BOX_STCK_CNT_DIF_DETAIL", "RQSTDT", "RSLTDT");
        }

        #region 제외 버튼
        private void btnExclude1_Click(object sender, RoutedEventArgs e)
        {
            ExcludeProcess(grdMainTp1, "BR_MTRL_UPD_PROD_RACK_MTRL_BOX_STCK_CNT", "DA_MTRL_SEL_PROD_RACK_MTRL_BOX_STCK_CNT_SNAP");
        }


        private void btnExclude2_Click(object sender, RoutedEventArgs e)
        {
            ExcludeProcess(grdMainTp2, "BR_MTRL_UPD_PROD_RACK_MTRL_BOX_STCK_CNT", "DA_MTRL_SEL_PROD_RACK_MTRL_BOX_STCK_CNT_RSLT");
        }

        /// <summary>
        /// 제외
        /// </summary>
        /// <param name="grid"></param>
        /// <param name="bizRulName"></param>
        /// <param name="dt"></param>
        /// <param name="msgYN"></param>
        /// <param name="msgcplt"></param>
        private void ExcludeProcess(C1DataGrid grid, string bizRulName, DataTable dt, string msgYN, string msgcplt, string searchBiz)
        {
            if (sExclude == "Y")
            {
                Util.MessageInfo("SFU3499");
                return;
            }

            LGC.GMES.MES.ControlsLibrary.MessageBox.Show(ms.AlertRetun(msgYN), null, "Information", MessageBoxButton.OKCancel, MessageBoxIcon.None, (sResult) =>
            //{0} 판정 하시겠습니까 ? 
            {
                if (sResult == MessageBoxResult.OK)
                {
                    SetBizRulDataTable(grid, bizRulName, "INDATA", "OUTDATA", dt, msgcplt, searchBiz);
                    //SearchGrdMainTp1(); //grid refresh 
                }

            });


        }

        private void ExcludeProcess(C1DataGrid grid, string bizRulName, string searchBiz)
        {
            DataTable dtVali = DataTableConverter.Convert(grid.ItemsSource);

            var queryValidationCheck = dtVali.AsEnumerable().Where(x => x.Field<string>("CHK") == "True");

            if (queryValidationCheck.Count() <= 0)
            {
                Util.Alert("10008");  // 선택된 데이터가 없습니다.
                                      //this.grdMainTp1.Focus();
                return;
            }

            DataTable dt = DataTableConverter.Convert(grid.ItemsSource).AsEnumerable().Where(x => x.Field<string>("CHK") == "True").CopyToDataTable();

            if (dt.Select("STCK_CNT_EXCL_FLAG = 'Y'").Length > 0)
            {
                Util.MessageInfo("SUF9010");
                return;
            }

            LGC.GMES.MES.ControlsLibrary.MessageBox.Show(ms.AlertRetun(tabControlMain.SelectedIndex == 0 ? "SUF9008" : "SUF9009"), null, "Information", MessageBoxButton.OKCancel, MessageBoxIcon.None, (sResult) =>
            //{0} 판정 하시겠습니까 ? 
            {
                if (sResult == MessageBoxResult.OK)
                {
                    this.SaveProcess(grid, bizRulName, "INDATA", "OUTDATA", dt, searchBiz);
                }

            });
        }
        #endregion

        #region #. Search, Save
        // 조회
        private void SearchProcess(C1DataGrid grid, string sBizRulName, string sInTableName, string sOutTableName)
        {
            this.loadingIndicator.Visibility = Visibility.Visible;
            PackCommon.DoEvents();
            try
            {
                DataTable dt = null;
                string[] sParam = new string[] {
                                            "LANGID"
                                            , "STCK_CNT_YM"
                                            , "STCK_CNT_SEQNO"
                                            , "EQSGID"
                                            , "MTRL_PORT_ID"
                                            , "MTRLID"
                                            , "REPACK_BOX_ID"
                                            , "STCK_CNT_EXCL_FLAG"
                                            , "REQ_NO"
                                            , "REPACK_BOX_TYPE"
                                            , "RACKIDGAP"
                                            , "STATUS"
                };

                /* 각 탭당 동적 컨트롤 */
                LGC.GMES.MES.ControlsLibrary.LGCDatePicker date = tabControlMain.SelectedIndex == 0 ? dpStckCntYmTp1 : tabControlMain.SelectedIndex == 1 ? dpStckCntYmTp2 : dpStckCntYmTp3;
                C1.WPF.C1ComboBox seqNo = tabControlMain.SelectedIndex == 0 ? cboSeqNoTp1 : tabControlMain.SelectedIndex == 1 ? cboSeqNoTp2 : cboSeqNoTp3;
                LGC.GMES.MES.ControlsLibrary.MultiSelectionBox line = tabControlMain.SelectedIndex == 0 ? cboLine : tabControlMain.SelectedIndex == 1 ? cboLine2 : cboLine3;
                LGC.GMES.MES.ControlsLibrary.MultiSelectionBox mtrlPort = tabControlMain.SelectedIndex == 0 ? cboMtrlPort : tabControlMain.SelectedIndex == 1 ? cboMtrlPort2 : cboMtrlPort3;
                System.Windows.Controls.TextBox mtrkId = tabControlMain.SelectedIndex == 0 ? txtMtrkIdTp1 : tabControlMain.SelectedIndex == 1 ? txtMtrkIdTp2 : txtMtrkIdTp3;
                string _line = string.Empty;
                string port = string.Empty;
                string mtrl = string.Empty;
                string repackBoxType = string.Empty;

                if (grid.Name == "grdDetailTp3")
                {
                    grdMainTp3.DataContext = null;

                    if (grdMainTp3.CurrentRow != null)
                    {
                        _line = grdMainTp3.CurrentRow.DataItem.GetValue("EQSGID").ToString() == "*" ? line.SelectedItemsToString : grdMainTp3.CurrentRow.DataItem.GetValue("EQSGID").ToString();
                        port = grdMainTp3.CurrentRow.DataItem.GetValue("MTRL_PORT_ID").ToString() == "*" ? mtrlPort.SelectedItemsToString : grdMainTp3.CurrentRow.DataItem.GetValue("MTRL_PORT_ID").ToString();
                        mtrl = grdMainTp3.CurrentRow.DataItem.GetValue("MTRLID").ToString() == "*" ? "*" : grdMainTp3.CurrentRow.DataItem.GetValue("MTRLID").ToString();
                        repackBoxType = cboDetailTypeCompare.SelectedValue.ToString() == "ALL" ? "EQUAL,EXCESS,LACK" : cboDetailTypeCompare.SelectedValue.ToString() == "NOT_SNAP" ? "LACK" : "EXCESS";
                    }
                }
                else
                {
                    _line = line.SelectedItemsToString;
                    port = mtrlPort.SelectedItemsToString;
                    mtrl = mtrkId.Text == "" ? null : mtrkId.Text;
                }

                System.Windows.Controls.TextBox RepackBox = tabControlMain.SelectedIndex == 0 ? txtRepackBoxIdTp1 : tabControlMain.SelectedIndex == 1 ? txtRepackBoxIdTp2 : txtRepackBoxIdTp3;
                C1.WPF.C1ComboBox ExcludeYN = tabControlMain.SelectedIndex == 0 ? cboExcludeYNTp1 : cboExcludeYNTp2;
                System.Windows.Controls.TextBlock rowCount = tabControlMain.SelectedIndex == 0 ? tbGrdMainTp1Cnt : tabControlMain.SelectedIndex == 1 ? tbGrdMainTp2Cnt : grid == grdMainTp3 ? tbGrdMainTp3Cnt : tbGrdDetailTp3Cnt;
                string rackIDGap = chkRackIDGap.IsChecked == true ? "Y" : string.Empty;
                string Status = chkStatus.IsChecked == true ? "Y" : string.Empty;

                //string repackBoxType = grid.Name == "grdDetailTp3" ? grdMainTp3.CurrentRow.DataItem.GetValue("REPACK_BOX_TYPE").ToString() : null;
                /* 각 탭당 동적 컨트롤 */

                object[] oParamValue = new object[]
                {
                    LoginInfo.LANGID
                    , date.SelectedDateTime.ToString("yyyyMM")
                    , seqNo.SelectedValue == null ? null : seqNo.SelectedValue
                    , _line /* line.SelectedItemsToString*/
                    , port
                    , mtrl
                    , RepackBox.Text == "" ? null : RepackBox.Text
                    , ExcludeYN.SelectedValue.ToString() == "ALL" ? null : ExcludeYN.SelectedValue.ToString()
                    , null
                    , repackBoxType
                    , rackIDGap == "" ? null : rackIDGap
                    , Status == "" ? null : Status
                };

                dt = GetBizRulDataTable(sBizRulName, sInTableName, sOutTableName, sParam, oParamValue);

                PackCommon.SearchRowCount(ref rowCount, dt.Rows.Count);
                Util.GridSetData(grid, dt, FrameOperation);

                if (SeqNoCloseYN() == false)
                {
                    if (tabControlMain.SelectedIndex == 0)
                    {
                        txtExcludeNote_SNAP.IsEnabled = false;
                        btnExclude1.IsEnabled = false;
                        btnSeqNoCloseTp1.IsEnabled = false;
                    }
                    else
                    {
                        txtExcludeNote_SNAP2.IsEnabled = false;
                        btnExclude2.IsEnabled = false;
                    }
                }
                else
                {
                    if (tabControlMain.SelectedIndex == 0)
                    {
                        txtExcludeNote_SNAP.IsEnabled = true;
                        btnExclude1.IsEnabled = true;
                        btnSeqNoCloseTp1.IsEnabled = true;
                    }
                    else
                    {
                        txtExcludeNote_SNAP2.IsEnabled = true;
                        btnExclude2.IsEnabled = true;
                    }
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                this.loadingIndicator.Visibility = Visibility.Collapsed;
            }

            this.loadingIndicator.Visibility = Visibility.Collapsed;
        }

        private bool SeqNoCloseYN()
        {
            bool returnResult = true;

            string[] sParam = new string[] { "LANGID"
                                            , "STCK_CNT_YM"
                                            , "STCK_CNT_SEQNO" };

            LGC.GMES.MES.ControlsLibrary.LGCDatePicker date = tabControlMain.SelectedIndex == 0 ? dpStckCntYmTp1 : tabControlMain.SelectedIndex == 1 ? dpStckCntYmTp2 : dpStckCntYmTp3;
            C1.WPF.C1ComboBox seqNo = tabControlMain.SelectedIndex == 0 ? cboSeqNoTp1 : tabControlMain.SelectedIndex == 1 ? cboSeqNoTp2 : cboSeqNoTp3;

            object[] oParamValue = new object[]
                {
                    LoginInfo.LANGID
                    , date.SelectedDateTime.ToString("yyyyMM")
                    , seqNo.SelectedValue == null ? null : seqNo.SelectedValue
                };

            if (GetBizRulDataTable("DA_MTRL_SEL_STOCK_ORD", "RQSTDT", "RSLTDT", sParam, oParamValue).Rows[0]["STCK_CNT_CMPL_FLAG"].ToString() == "Y")
                returnResult = false;

            return returnResult;
        }

        // 저장
        private void SaveProcess(C1DataGrid grid, string sBizRulName, string sInTableName, string sOutTableName, DataTable dtInData, string searchBiz)
        {
            try
            {
                if (grid == null)
                {
                    return;
                }

                DataTable dt = new DataTable();
                dt.TableName = "INDATA";
                dt.Columns.Add("LANGID", typeof(string));
                dt.Columns.Add("STCK_CNT_YM", typeof(string));
                dt.Columns.Add("STCK_CNT_SEQNO", typeof(string));
                dt.Columns.Add("REQ_NO", typeof(string));
                dt.Columns.Add("MTRL_PORT_ID", typeof(string));
                dt.Columns.Add("REPACK_BOX_ID", typeof(string));
                dt.Columns.Add("STCK_CNT_EXCL_NOTE", typeof(string));
                dt.Columns.Add("USERID", typeof(string));
                dt.Columns.Add("TABLEGUBUN", typeof(string));

                /* 각 탭당 동적 컨트롤 */
                System.Windows.Controls.TextBox ExcloudeNote = tabControlMain.SelectedIndex == 0 ? txtExcludeNote_SNAP : txtExcludeNote_SNAP2;
                /* 각 탭당 동적 컨트롤 */

                DataRow drInData = null;

                for (int i = 0; i < dtInData.Rows.Count; i++)
                {
                    drInData = dt.NewRow();

                    drInData["LANGID"] = LoginInfo.LANGID;
                    drInData["STCK_CNT_YM"] = dtInData.Rows[i]["STCK_CNT_YM"].ToString();
                    drInData["STCK_CNT_SEQNO"] = dtInData.Rows[i]["STCK_CNT_SEQNO"].ToString();
                    drInData["REQ_NO"] = dtInData.Rows[i]["REQ_NO"].ToString() == "" ? null : dtInData.Rows[i]["REQ_NO"].ToString();
                    drInData["MTRL_PORT_ID"] = dtInData.Columns["REAL_MTRL_PORT_ID"] == null ? null : dtInData.Rows[i]["REAL_MTRL_PORT_ID"].ToString();
                    drInData["REPACK_BOX_ID"] = dtInData.Columns["REAL_REPACK_BOX_ID"] == null ? null : dtInData.Rows[i]["REAL_REPACK_BOX_ID"].ToString();
                    drInData["STCK_CNT_EXCL_NOTE"] = ExcloudeNote.Text == "" ? null : ExcloudeNote.Text;
                    drInData["USERID"] = LoginInfo.USERID;
                    drInData["TABLEGUBUN"] = tabControlMain.SelectedIndex == 0 ? "SNAP" : "RSLT";

                    dt.Rows.Add(drInData);
                }

                SetBizRulDataTable(grid, sBizRulName, sInTableName, sOutTableName, dt, "SFU1275", searchBiz);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        #endregion

        private DataTable GetBizRulDataTable(string bizRuleName, string sInTableName, string sOutTableName, string[] sParam, object[] oParamValue)
        {
            DataTable dtInData = new DataTable(sInTableName);
            DataTable dtOutData = new DataTable(sOutTableName);

            try
            {
                for (int i = 0; i < sParam.Length; i++)
                {
                    dtInData.Columns.Add(sParam[i], typeof(string));
                }

                DataRow drInData = dtInData.NewRow();

                for (int i = 0; i < oParamValue.Length; i++)
                {
                    drInData[sParam[i]] = oParamValue[i];
                }

                dtInData.Rows.Add(drInData);

                dtOutData = new ClientProxy().ExecuteServiceSync(bizRuleName, dtInData.TableName, dtOutData.TableName, dtInData);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }

            return dtOutData;
        }

        private void SetBizRulDataTable(C1DataGrid grid, string bizRuleName, string sInTableName, string sOutTableName, DataTable dtInData, string msg, string searchBiz)
        {
            try
            {
                new ClientProxy().ExecuteService(bizRuleName, sInTableName, null, dtInData, (result, ex) =>
                {
                    loadingIndicator.Visibility = Visibility.Collapsed;

                    if (ex != null)
                    {
                        Util.MessageException(ex);
                        return;
                    }

                    ms.AlertInfo(msg); //정상처리되었습니다.

                    if (bizRuleName == "BR_MTRL_UPD_PROD_RACK_MTRL_BOX_STCK_CNT_ORD" || bizRuleName == "DA_MTRL_INS_PROD_RACK_MTRL_BOX_STCK_CNT_SNAP_SP")
                    {
                        SetcboSeqNo(cboSeqNoTp1, dpStckCntYmTp1);

                        SetcboSeqNo(cboSeqNoTp2, dpStckCntYmTp2);

                        SetcboSeqNo(cboSeqNoTp3, dpStckCntYmTp3);
                    }

                    this.SearchProcess(grid, searchBiz, "RQSTDT", "RSLTDT");

                    txtExcludeNote_SNAP.Text = string.Empty;
                    txtExcludeNote_SNAP2.Text = string.Empty;

                    //this.SearchProcess();

                    //SetGrdDetail(sParam);
                });
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void dpStckCntYmTp1_SelectedDataTimeChanged(object sender, SelectionChangedEventArgs e)
        {
            SetcboSeqNo(cboSeqNoTp1, dpStckCntYmTp1);
        }

        private void dpStckCntYmTp2_SelectedDataTimeChanged(object sender, SelectionChangedEventArgs e)
        {
            SetcboSeqNo(cboSeqNoTp2, dpStckCntYmTp2);
        }

        private void dpStckCntYmTp3_SelectedDataTimeChanged(object sender, SelectionChangedEventArgs e)
        {
            SetcboSeqNo(cboSeqNoTp3, dpStckCntYmTp3);
        }

        /// <summary>
        /// 차수 마감
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnSeqNoCloseTp1_Click(object sender, RoutedEventArgs e)
        {
            if (SeqNoCloseYN() == false)
            {
                Util.MessageInfo("SFU1172");
                return;
            }

            DataTable dt = new DataTable();
            dt.TableName = "INDATA";
            dt.Columns.Add("STCK_CNT_YM", typeof(string));
            dt.Columns.Add("STCK_CNT_SEQNO", typeof(string));
            dt.Columns.Add("STCK_CNT_CMPL_FLAG", typeof(string));
            dt.Columns.Add("USERID", typeof(string));

            LGC.GMES.MES.ControlsLibrary.LGCDatePicker date = tabControlMain.SelectedIndex == 0 ? dpStckCntYmTp1 : tabControlMain.SelectedIndex == 1 ? dpStckCntYmTp2 : dpStckCntYmTp3;
            C1.WPF.C1ComboBox seqNo = tabControlMain.SelectedIndex == 0 ? cboSeqNoTp1 : tabControlMain.SelectedIndex == 1 ? cboSeqNoTp2 : cboSeqNoTp3;
            C1.WPF.C1ComboBox ExcludeYN = tabControlMain.SelectedIndex == 0 ? cboExcludeYNTp1 : cboExcludeYNTp2;

            DataRow drInData = dt.NewRow();

            drInData["STCK_CNT_YM"] = date.SelectedDateTime.ToString("yyyyMM");
            drInData["STCK_CNT_SEQNO"] = seqNo.SelectedValue;
            drInData["STCK_CNT_CMPL_FLAG"] = "Y";
            drInData["USERID"] = LoginInfo.USERID;

            dt.Rows.Add(drInData);

            ExcludeProcess(grdMainTp1, "BR_MTRL_UPD_PROD_RACK_MTRL_BOX_STCK_CNT_ORD", dt, "SFU1276", "SFU1277", "DA_MTRL_SEL_PROD_RACK_MTRL_BOX_STCK_CNT_SNAP");
        }

        /// <summary>
        /// 차수 증가
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnSeqNoAddTp1_Click(object sender, RoutedEventArgs e)
        {
            DataTable dt = new DataTable();
            dt.TableName = "RQSTDT";
            dt.Columns.Add("P_USERID", typeof(string));
            dt.Columns.Add("P_STCK_CNT_NOTE", typeof(string));

            System.Windows.Controls.TextBox ExcloudeNote = tabControlMain.SelectedIndex == 0 ? txtExcludeNote_SNAP : txtExcludeNote_SNAP2;

            DataRow drInData = dt.NewRow();

            drInData["P_USERID"] = LoginInfo.USERID;
            drInData["P_STCK_CNT_NOTE"] = ExcloudeNote.Text == "" ? null : ExcloudeNote.Text;

            dt.Rows.Add(drInData);

            ExcludeProcess(grdMainTp1, "DA_MTRL_INS_PROD_RACK_MTRL_BOX_STCK_CNT_SNAP_SP", dt, "SFU2959", "SFU1275", "DA_MTRL_SEL_PROD_RACK_MTRL_BOX_STCK_CNT_SNAP");
        }

        private void cboDetailTypeCompare_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            try
            {
                if (dpStckCntYmTp3.SelectedDateTime.ToString("yyyy") == "0001")
                    return;

                this.SearchProcess(this.grdDetailTp3, "DA_MTRL_SEL_PROD_RACK_MTRL_BOX_STCK_CNT_DIF_DETAIL", "RQSTDT", "RSLTDT");
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void btnExcel_Compare_Detail_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                new ExcelExporter().Export(grdDetailTp3);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void grdMainTp3_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            this.SearchProcess(this.grdDetailTp3, "DA_MTRL_SEL_PROD_RACK_MTRL_BOX_STCK_CNT_DIF_DETAIL", "RQSTDT", "RSLTDT");
        }
    }

    #region #999. DataHelper (Biz 호출)
    internal class PACK003_037_DataHelper
    {
        #region #999-1. Member Variable Lists...
        #endregion #999-1. Member Variable Lists...

        #region #999-2. Constructor
        internal PACK003_037_DataHelper()
        {
        }
        #endregion #999-2. Constructor

        #region #999-3. Member Function Lists...
        internal DataTable GetLineData()
        {
            string bizRuleName = "DA_MTRL_SEL_EQUIPMENTSEGMENT_MTRL_PORT_CBO";

            DataTable dtInData = new DataTable("RQSTDT");
            DataTable dtOutData = new DataTable("RSLTDT");

            try
            {
                dtInData.Columns.Add("LANGID", typeof(string));
                dtInData.Columns.Add("AREAID", typeof(string));

                DataRow drRQSTDT = dtInData.NewRow();
                drRQSTDT["LANGID"] = LoginInfo.LANGID;
                drRQSTDT["AREAID"] = LoginInfo.CFG_AREA_ID;
                dtInData.Rows.Add(drRQSTDT);

                dtOutData = new ClientProxy().ExecuteServiceSync(bizRuleName, dtInData.TableName, dtOutData.TableName, dtInData);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }

            return dtOutData;
        }

        internal DataTable GetRackData(string EqsgID)
        {
            string bizRuleName = "DA_MTRL_SEL_MTRL_PORT_CBO";

            DataTable dtInData = new DataTable("RQSTDT");
            DataTable dtOutData = new DataTable("RSLTDT");

            try
            {
                dtInData.Columns.Add("LANGID", typeof(string));
                dtInData.Columns.Add("AREAID", typeof(string));
                dtInData.Columns.Add("EQSGID", typeof(string));

                DataRow drRQSTDT = dtInData.NewRow();
                drRQSTDT["LANGID"] = LoginInfo.LANGID;
                drRQSTDT["AREAID"] = LoginInfo.CFG_AREA_ID;
                drRQSTDT["EQSGID"] = string.IsNullOrWhiteSpace(EqsgID) || EqsgID.Equals("ALL") ? null : EqsgID;
                dtInData.Rows.Add(drRQSTDT);

                dtOutData = new ClientProxy().ExecuteServiceSync(bizRuleName, dtInData.TableName, dtOutData.TableName, dtInData);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }

            return dtOutData;
        }
        #endregion #999-3. Member Function Lists...
    }
    #endregion #999. DataHelper (Biz 호출)
}
