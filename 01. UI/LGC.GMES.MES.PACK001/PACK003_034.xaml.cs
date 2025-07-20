/*************************************************************************************
 Created Date : 2022.05.26
      Creator : 이태규
   Decription : 창고 저장위치 입/출고
--------------------------------------------------------------------------------------
 [Change History]
  2022.05.26  이태규 : Initial Created.
***************************************************************************************/

using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using C1.WPF;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using LGC.GMES.MES.CMM001.Extensions; 
using C1.WPF.DataGrid;
using System.Threading.Tasks;
using System.Windows.Threading;
using System.Windows.Media.Animation;
using LGC.GMES.MES.PACK001.Class;

namespace LGC.GMES.MES.PACK001
{
    public partial class PACK003_034 : UserControl, IWorkArea
    {
        #region #. Member Variable Lists...
        LGC.GMES.MES.PACK001.Class.MESSAGE_PARAM ms = new LGC.GMES.MES.PACK001.Class.MESSAGE_PARAM();
        Util util = new Util();
        CommonCombo _combo = new CommonCombo();

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
        public PACK003_034()
        {
            InitializeComponent();
        }
        #endregion

        #region #. Event Lists...
        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                List<Button> listAuth = new List<Button>();
                Util.pageAuth(listAuth, FrameOperation.AUTHORITY);

                #region #. tp1
                tbGrdMainTp1Cnt.Text = "[ 0 " + ObjectDic.Instance.GetObjectName("건") + " ]";
                tbGrdMainTp2Cnt.Text = "[ 0 " + ObjectDic.Instance.GetObjectName("건") + " ]";

                /* 버튼 제거로 주석 처리
                btnRack_IN.Tag = "RI";
                btnWH_IN.Tag = "WI";
                btnWH_OUT.Tag = "WO";
                */
                txtLocation.Focus();
                #endregion

                #region #. tp2
                DateTime firstOfThisMonth = DateTime.Now.AddDays(-7);
                DateTime firstOfNextMonth = DateTime.Now;

                dtpFr.IsNullInitValue = true;
                dtpTo.IsNullInitValue = true;

                dtpFr.SelectedDateTime = firstOfThisMonth;
                dtpTo.SelectedDateTime = firstOfNextMonth;

                _combo.SetCombo(cboWhId, CommonCombo.ComboStatus.SELECT, sCase: "cboWHID");
                #endregion

                this.Loaded -= new System.Windows.RoutedEventHandler(this.UserControl_Loaded);
            }
            catch (Exception ex)
            {
                Util.Alert(ex.ToString());
            }
        }

        #region #. tp1
        private void btnInit_Click(object sender, RoutedEventArgs e)
        {
            if (this.grdMainTp1.Rows.Count != 0)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("SFU1815"), null, "Info", MessageBoxButton.OKCancel, MessageBoxIcon.None, (Result) =>
                {
                    if (Result == MessageBoxResult.OK)
                    {
                        this.txtScanLot.Clear();
                        this.txtLocation.Clear();
                        Util.gridClear(grdMainTp1);
                        txtLocation.Focus();
                    }
                });
            }
            else
            {
                this.txtLocation.Clear();
                this.txtScanLot.Clear();
                txtLocation.Focus();
            }
        }

        /* 버튼 제거로 인한 주석 처리
        private void btnWH_IN_Click(object sender, RoutedEventArgs e)
        {
            grdMainTp1.ClearRows();
            string sLocation = txtLocation.Text.Trim();
            string sScanLot = txtScanLot.Text.Trim();
            if (string.IsNullOrWhiteSpace(sLocation))
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("SFU2060"), null, "Info", MessageBoxButton.OK, MessageBoxIcon.None, (result) =>
                {
                    this.txtLocation.Clear();
                    this.txtLocation.Focus();
                    return;
                });
            }
            else if (string.IsNullOrWhiteSpace(sScanLot))
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("SFU2060"), null, "Info", MessageBoxButton.OK, MessageBoxIcon.None, (result) =>
                {
                    this.txtScanLot.Focus();
                    return;
                });
            }
            else
            {
                Scan(sender, sLocation, sScanLot);
            }
        }

        private void btnRack_IN_Click(object sender, RoutedEventArgs e)
        {
            grdMainTp1.ClearRows();
            string sLocation = txtLocation.Text.Trim();
            string sScanLot = txtScanLot.Text.Trim();
            if (string.IsNullOrWhiteSpace(sLocation))
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("SFU2060"), null, "Info", MessageBoxButton.OK, MessageBoxIcon.None, (result) =>
                {
                    this.txtLocation.Clear();
                    this.txtLocation.Focus();
                    return;
                });
            }
            else if (string.IsNullOrWhiteSpace(sScanLot))
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("SFU2060"), null, "Info", MessageBoxButton.OK, MessageBoxIcon.None, (result) =>
                {
                    this.txtScanLot.Focus();
                    return;
                });
            }
            else
            {
                Scan(sender, sLocation, sScanLot);
            }
        }

        private void btnWH_OUT_Click(object sender, RoutedEventArgs e)
        {
            txtLocation.Clear();
            grdMainTp1.ClearRows();
            string sLocation = txtLocation.Text.Trim();
            string sScanLot = txtScanLot.Text.Trim();
            if (string.IsNullOrWhiteSpace(sScanLot))
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("SFU2060"), null, "Info", MessageBoxButton.OK, MessageBoxIcon.None, (result) =>
                {
                    this.txtScanLot.Focus();
                    return;
                });
            }
            else
            {
                Scan(sender, sLocation, sScanLot);
            }
        }
        */

        private void txtScanLot_KeyDown(object sender, KeyEventArgs e)
        {
            try
            {
                if (e.Key == Key.Enter)
                {
                    string sScanLot = txtScanLot.Text.ToString().Trim();
                    if (string.IsNullOrWhiteSpace(sScanLot))
                    {
                        // 스캔한 데이터가 없습니다.
                        LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("SFU2060"), null, "Info", MessageBoxButton.OKCancel, MessageBoxIcon.None, (Result) =>
                        {
                            if (Result == MessageBoxResult.OK)
                            {
                                this.txtScanLot.Clear();
                                txtScanLot.Focus();
                            }
                        });
                    }
                    else
                    {
                        if (rbWH_Enter.IsChecked == false && rbRACK_Enter.IsChecked == false && rbWH_Release.IsChecked == false)
                        {
                            string sDetail = ObjectDic.Instance.GetObjectName(tbRadioButton.Text.ToString()).Replace("[#] ", string.Empty);
                            // 선택오류 : 필수조건을 선택하지 않았습니다. [%]
                            LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("SFU8393", sDetail), null, "Info", MessageBoxButton.OKCancel, MessageBoxIcon.None, (Result) =>
                            {
                                if (Result == MessageBoxResult.OK)
                                {
                                    this.txtScanLot.Clear();
                                    txtScanLot.Focus();
                                }
                            });
                        }
                        else
                        {
                            Scan(sScanLot);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void txtLocation_KeyDown(object sender, KeyEventArgs e)
        {
            try
            {
                if (e.Key == Key.Enter)
                {
                    string sLocation = txtLocation.Text.ToString().Trim();
                    if(string.IsNullOrWhiteSpace(sLocation))
                    {
                        // 스캔한 데이터가 없습니다.
                        LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("SFU2060"), null, "Info", MessageBoxButton.OKCancel, MessageBoxIcon.None, (Result) =>
                        {
                            if (Result == MessageBoxResult.OK)
                            {
                                txtLocation.Focus();
                            }
                        });
                    }
                    else
                    {
                        this.txtScanLot.Focus();
                    }
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        #endregion

        #region #. tp2
        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            SearchGrdMainTp2(sender);
        }

        #endregion

        #endregion

        #region #. Member Function Lists...
        public DataTable initTable()
        {
            DataTable dt = new DataTable();
            dt.Columns.Add("SEQ_NO", typeof(string));
            dt.Columns.Add("LOTID", typeof(string));
            dt.Columns.Add("PRODID", typeof(string));
            dt.Columns.Add("PRODNAME", typeof(string));
            dt.Columns.Add("PRJT_NAME", typeof(string));
            dt.Columns.Add("RACK_ID", typeof(string));
            dt.Columns.Add("WIPQTY", typeof(string));
            dt.Columns.Add("WIPQTY2", typeof(string));
            dt.Columns.Add("PROCID", typeof(string));
            dt.Columns.Add("EQSGID", typeof(string));
            dt.Columns.Add("EQSGNAME", typeof(string));
            dt.Columns.Add("WIPSTAT", typeof(string));
            dt.Columns.Add("ROLLPRESS_SEQNO", typeof(string));
            dt.Columns.Add("MKT_TYPE_CODE", typeof(string));
            dt.Columns.Add("CSTID", typeof(string));
            dt.Columns.Add("CURR_WIP_QLTY_TYPE_CODE", typeof(string));
            dt.Columns.Add("SYSTEM_WH_ID", typeof(string));
            dt.Columns.Add("SYSTEM_RACK_ID", typeof(string));
            dt.Columns.Add("BOXID", typeof(string));
            dt.Columns.Add("LOTSTAT", typeof(string));
            return dt;
        }

        public DataTable initTable2()
        {
            DataTable dt = new DataTable();
            dt.Columns.Add("SEQ_NO", typeof(string));
            dt.Columns.Add("LOTID", typeof(string));
            dt.Columns.Add("ACTNAME", typeof(string));
            dt.Columns.Add("WIPNOTE", typeof(string));
            dt.Columns.Add("ACTDTTM", typeof(string));
            dt.Columns.Add("USERID", typeof(string));
            dt.Columns.Add("USERNAME", typeof(string));
            dt.Columns.Add("BOXID", typeof(string));
            dt.Columns.Add("PALLETID", typeof(string));
            dt.Columns.Add("GROUPID", typeof(string));
            return dt;
        }
        // public void Scan(Object obj, string sLocation, string sScanLot)
        public void Scan(string sScanLot)
        {
            try
            {
                string sLocation = txtLocation.Text.Trim();
                DataSet dsInput = new DataSet();

                DataTable INDATA = new DataTable();
                INDATA.TableName = "INDATA";
                INDATA.Columns.Add("SRCTYPE", typeof(string));
                INDATA.Columns.Add("LANGID", typeof(string));
                INDATA.Columns.Add("LOTID", typeof(string));
                INDATA.Columns.Add("WH_ID", typeof(string));
                INDATA.Columns.Add("RACK_ID", typeof(string));
                INDATA.Columns.Add("JOB_FLAG", typeof(string));
                INDATA.Columns.Add("USERID", typeof(string));

                DataRow dr = INDATA.NewRow();
                dr["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                dr["LANGID"] = LoginInfo.LANGID;
                dr["LOTID"] = sScanLot;
                dr["WH_ID"] = sLocation;
                dr["RACK_ID"] = sLocation;
                //dr["JOB_FLAG"] = ((Control)obj).Tag;
                dr["JOB_FLAG"] = rbWH_Enter.IsChecked == true ? "WI" : (rbRACK_Enter.IsChecked == true ? "RI" : "WO");
                dr["USERID"] = LoginInfo.USERID;
                INDATA.Rows.Add(dr);
                dsInput.Tables.Add(INDATA);

                DataSet dsResult = new ClientProxy().ExecuteServiceSync_Multi("BR_PRD_REG_WIPATTR_FOR_WHID_RACK_ID_PACK", "INDATA", "OUTDATA", dsInput, null);

                if (dsResult.Tables[0].Rows.Count > 0)
                {
                    //Grid 생성
                    DataTable dtData = initTable();
                    dtData.AcceptChanges();
                    for (int i = 0; i < dsResult.Tables["OUTDATA"].Rows.Count; i++)
                    {
                        DataRow newRow = null;
                        newRow = dtData.NewRow();
                        newRow["SEQ_NO"] = (i + 1).ToString();
                        newRow["LOTID"] = dsResult.Tables["OUTDATA"].Rows[i]["LOTID"];
                        newRow["PRODID"] = dsResult.Tables["OUTDATA"].Rows[i]["PRODID"];
                        newRow["PRODNAME"] = dsResult.Tables["OUTDATA"].Rows[i]["PRODNAME"];
                        newRow["PRJT_NAME"] = dsResult.Tables["OUTDATA"].Rows[i]["PRJT_NAME"];
                        newRow["RACK_ID"] = dsResult.Tables["OUTDATA"].Rows[i]["RACK_ID"];
                        newRow["WIPQTY"] = dsResult.Tables["OUTDATA"].Rows[i]["WIPQTY"];
                        newRow["WIPQTY2"] = dsResult.Tables["OUTDATA"].Rows[i]["WIPQTY2"];
                        newRow["PROCID"] = dsResult.Tables["OUTDATA"].Rows[i]["PROCID"];
                        newRow["EQSGID"] = dsResult.Tables["OUTDATA"].Rows[i]["EQSGID"];
                        newRow["EQSGNAME"] = dsResult.Tables["OUTDATA"].Rows[i]["EQSGNAME"];
                        newRow["WIPSTAT"] = dsResult.Tables["OUTDATA"].Rows[i]["WIPSTAT"];
                        newRow["ROLLPRESS_SEQNO"] = dsResult.Tables["OUTDATA"].Rows[i]["ROLLPRESS_SEQNO"];
                        newRow["MKT_TYPE_CODE"] = dsResult.Tables["OUTDATA"].Rows[i]["MKT_TYPE_CODE"];
                        newRow["CSTID"] = dsResult.Tables["OUTDATA"].Rows[i]["CSTID"];
                        newRow["CURR_WIP_QLTY_TYPE_CODE"] = dsResult.Tables["OUTDATA"].Rows[i]["CURR_WIP_QLTY_TYPE_CODE"];
                        newRow["SYSTEM_WH_ID"] = dsResult.Tables["OUTDATA"].Rows[i]["SYSTEM_WH_ID"];
                        newRow["SYSTEM_RACK_ID"] = dsResult.Tables["OUTDATA"].Rows[i]["SYSTEM_RACK_ID"];
                        newRow["BOXID"] = dsResult.Tables["OUTDATA"].Rows[i]["BOXID"];
                        newRow["LOTSTAT"] = dsResult.Tables["OUTDATA"].Rows[i]["LOTSTAT"];
                        dtData.Rows.Add(newRow);
                    }
                    DataView dvData = dtData.DefaultView;
                    dvData.Sort = "SEQ_NO DESC";

                    grdMainTp1.BeginEdit();
                    grdMainTp1.ItemsSource = DataTableConverter.Convert(dvData.ToTable());
                    grdMainTp1.EndEdit();
                    //this.txtLocation.Clear();
                    this.txtScanLot.Clear();
                    txtScanLot.Focus();
                }
                else
                {
                    //{0}은(는) 유효하지 않은 LOT ID 입니다.
                    LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("SFU2891", new object[] { sScanLot }), null, "Info", MessageBoxButton.OK, MessageBoxIcon.None, (result) =>
                    {
                        //this.txtLocation.Clear();
                        this.txtScanLot.Clear();
                        txtScanLot.Focus();
                    });
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                //this.txtLocation.Clear();
                this.txtScanLot.Clear();
                txtScanLot.Focus();
            }
        }
        private void SearchGrdMainTp2(object sender)
        {
            Util.gridClear(grdMainTp2);
            DateTime fromDate = this.dtpFr.SelectedDateTime;
            DateTime toDate = this.dtpTo.SelectedDateTime;
            fromDate = new DateTime(fromDate.Year, fromDate.Month, fromDate.Day, 0, 0, 0);
            toDate = new DateTime(toDate.Year, toDate.Month, toDate.Day, 0, 0, 0);

            if (fromDate > toDate)
            {
                Util.MessageValidation("SFU3569");  // 조회 시작일자는 종료일자를 초과 할 수 없습니다.
                this.dtpFr.Focus();
                return;
            }

            if (Convert.ToInt32((toDate - fromDate).TotalDays) > 15)
            {
                Util.MessageValidation("SFU2042", "15");   //기간은 {0}일 이내 입니다.
                this.dtpTo.Focus();
                return;
            }

            /* 필수 조건 삭제로 주석 처리함. 김건식S
            if (txtLOTID.Text.Length < 3)
            {
                Util.MessageValidation("SFU3624"); // LOTID는 3자리 이상 입력하세요.
                this.txtLOTID.Clear();
                this.txtLOTID.Focus();
                return;
            }
            */

            if (sender == null)
            {
                return;
            }
            try
            {
                string bizRuleName = "DA_PRD_SEL_WIPACTHISTORY_FOR_ACTID_WH";
                DataTable dtRQSTDT = new DataTable("RQSTDT");
                DataTable dtRSLTDT = new DataTable("RSLTDT");

                dtRQSTDT.Columns.Add("LANGID", typeof(string));
                dtRQSTDT.Columns.Add("FROMDATE", typeof(string));
                dtRQSTDT.Columns.Add("TODATE", typeof(string));
                dtRQSTDT.Columns.Add("LOTID", typeof(string));
                dtRQSTDT.Columns.Add("WH_ID", typeof(string));

                DataRow drRQSTDT = dtRQSTDT.NewRow();
                drRQSTDT["LANGID"] = LoginInfo.LANGID;
                drRQSTDT["FROMDATE"] = dtpFr.SelectedDateTime.ToString("yyyyMMdd");
                drRQSTDT["TODATE"] = dtpTo.SelectedDateTime.AddDays(1).ToString("yyyyMMdd");
                drRQSTDT["LOTID"] = string.IsNullOrEmpty(this.txtLOTID.Text) ? null : Util.GetCondition(txtLOTID);
                //drRQSTDT["WH_ID"] = Util.GetCondition(cboWhId);
                dtRQSTDT.Rows.Add(drRQSTDT);

                dtRSLTDT = new ClientProxy().ExecuteServiceSync(bizRuleName, dtRQSTDT.TableName, dtRSLTDT.TableName, dtRQSTDT);
                if (CommonVerify.HasTableRow(dtRSLTDT))
                {
                    //Grid 생성
                    DataTable dtData = initTable2();
                    dtData.AcceptChanges();
                    for (int i = 0; i < dtRSLTDT.Rows.Count; i++)
                    {
                        DataRow newRow = null;
                        newRow = dtData.NewRow();
                        newRow["SEQ_NO"] = (i + 1).ToString();
                        newRow["LOTID"] = dtRSLTDT.Rows[i]["LOTID"];
                        newRow["ACTNAME"] = dtRSLTDT.Rows[i]["ACTNAME"];
                        newRow["WIPNOTE"] = dtRSLTDT.Rows[i]["WIPNOTE"];
                        newRow["ACTDTTM"] = dtRSLTDT.Rows[i]["ACTDTTM"];
                        newRow["USERID"] = dtRSLTDT.Rows[i]["USERID"];
                        newRow["USERNAME"] = dtRSLTDT.Rows[i]["USERNAME"];
                        newRow["BOXID"] = dtRSLTDT.Rows[i]["BOXID"];
                        newRow["PALLETID"] = dtRSLTDT.Rows[i]["PALLET_ID"];
                        newRow["GROUPID"] = dtRSLTDT.Rows[i]["GR_ID"];
                        dtData.Rows.Add(newRow);
                    }
                    DataView dvData = dtData.DefaultView;
                    PackCommon.SearchRowCount(ref this.tbGrdMainTp2Cnt, dtData.Rows.Count);
                    Util.GridSetData(this.grdMainTp2, dtData, FrameOperation);
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        #endregion
    }
}