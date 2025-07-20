/*********************************************************************************************************************************
 Created Date : 2024.03.13
      Creator : 유명환
   Decription : 자재LOT 잔량재공 조회 바코드 프린트
-----------------------------------------------------------------------------------------------------------------------------------
 [Change History]
-----------------------------------------------------------------------------------------------------------------------------------
       DATE            CSR NO            DEVELOPER            DESCRIPTION
-----------------------------------------------------------------------------------------------------------------------------------
  2024.03.13                               유명환           Initial Created.
***********************************************************************************************************************************/

using C1.WPF;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.CMM001.Popup;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using System;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Linq;
using System.Globalization;
using LGC.GMES.MES.CMM001.Extensions;
using System.Collections.Generic;

namespace LGC.GMES.MES.MTRL001
{
    public partial class MTRL001_203_LABEL : C1Window, IWorkArea
    {
        #region Declaration & Constructor 

        private string _reqNo = string.Empty;
        private string _reqType = string.Empty;
        private string _scrapType = string.Empty;
        private string _resnGubun = string.Empty;
        private string _area = string.Empty;
        private object[] tmps = null;
        private Util _Util = new Util();
        NumberFormatInfo nfi = new CultureInfo("en-US", false).NumberFormat;

        public MTRL001_203_LABEL()
        {
            InitializeComponent();

            //InitCombo();

        }

        public IFrameOperation FrameOperation
        {
            get;
            set;
        }


        #endregion

        #region Initialize

        /// <summary>
        /// 화면내 combo 셋팅
        /// </summary>
        private void InitCombo()
        {

        }

        private void ClearList()
        {
            Util.gridClear(dgDeleteList);
        }
        #endregion

        #region Event


        private void C1Window_Loaded(object sender, RoutedEventArgs e)
        {
            //파라메터 등록
            tmps = C1WindowExtension.GetParameters(this);

            setGridHeader();
            LabelListData(tmps);
        }


        #region [조회클릭]
        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            ClearList();

            LabelListData(tmps);
        }
        #endregion
        
        private void C1Window_Closed(object sender, EventArgs e)
        {
            this.DialogResult = MessageBoxResult.Cancel;
        }

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void txtScanId_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                ClearList();

                LabelListData(tmps);
            }
        }

        private void btnGrid_Click(object sender, RoutedEventArgs e)
        {
            //FrameOperation
            DataRowView dataRow = ((FrameworkElement)sender).DataContext as DataRowView;
            string sMLotID = Util.NVC(dataRow.Row["MLOTID"]);
            string sINPUTSEQNO = Util.NVC(dataRow.Row["INPUTSEQNO"]);

            if (!LoginInfo.CFG_LABEL_TYPE.Equals("LBL0362"))
            {
                Util.MessageInfo("SFU2346"); //공정라벨 유형(자재 잔량 라벨)을 확인하세요.
                return;
            }

            try
            {
                string blCode = string.Empty;
                blCode = LoginInfo.CFG_LABEL_TYPE;

                DataTable inTable = new DataTable();
                inTable.Columns.Add("MLOTID", typeof(string));
                inTable.Columns.Add("INPUTSEQNO", typeof(string));
                inTable.Columns.Add("I_LBCD", typeof(string));   // 라벨코드
                inTable.Columns.Add("I_PRMK", typeof(string));   // 프린터기종
                inTable.Columns.Add("I_RESO", typeof(string));   // 해상도
                inTable.Columns.Add("I_PRCN", typeof(string));   // 출력매수
                inTable.Columns.Add("I_MARH", typeof(string));   // 시작위치H
                inTable.Columns.Add("I_MARV", typeof(string));   // 시작위치V
                inTable.Columns.Add("I_DARK", typeof(string));   // 농도

                foreach (DataRow row in LoginInfo.CFG_SERIAL_PRINT.Rows)
                {
                    if (Boolean.Parse(Util.NVC(row["DEFAULT"])) == true)
                    {
                        DataRow indata = inTable.NewRow();
                        indata["MLOTID"] = sMLotID;
                        indata["INPUTSEQNO"] = sINPUTSEQNO;
                        indata["I_LBCD"] = LoginInfo.CFG_LABEL_TYPE;
                        indata["I_PRMK"] = string.IsNullOrEmpty(Util.NVC(row["PRINTERTYPE"])) ? "Z" : string.Equals(row["PRINTERTYPE"], "Datamax") ? "D" : "Z";
                        indata["I_RESO"] = string.IsNullOrEmpty(Util.NVC(row["DPI"])) ? "203" : Util.NVC(row["DPI"]);
                        indata["I_PRCN"] = string.IsNullOrEmpty(Util.NVC(row["COPIES"])) ? "1" : Util.NVC(row["COPIES"]);
                        indata["I_MARH"] = string.IsNullOrEmpty(Util.NVC(row["X"])) ? "0" : Util.NVC(row["X"]);
                        indata["I_MARV"] = string.IsNullOrEmpty(Util.NVC(row["Y"])) ? "0" : Util.NVC(row["Y"]);
                        indata["I_DARK"] = string.IsNullOrEmpty(Util.NVC(row["DARKNESS"])) ? "15" : Util.NVC(row["DARKNESS"]);
                        inTable.Rows.Add(indata);

                        break;
                    }
                }

                if (inTable.Rows.Count < 1)
                {
                    Util.MessageValidation("SFU3030");  //프린터 환경설정 정보가 없습니다.
                    return;
                }

                string sBizName = string.Empty;

                sBizName = "BR_PRD_GET_MTRL_LOT_LABEL";

                DataTable dtMain = new ClientProxy().ExecuteServiceSync(sBizName, "INDATA", "OUTDATA", inTable);

                if (dtMain.Rows.Count == 0)
                    return;

                if (!string.Equals(Util.NVC(dtMain.Rows[0]["I_ATTVAL"]).Substring(0, 1), "0"))
                {
                    Util.MessageValidation("SFU1309");  //Barcode Print 실패
                    return;
                }

                // 동시에 출력시 순서 뒤바끼는 문제때문에 SLEEP 추가
                foreach (DataRow row in dtMain.Rows)
                {
                    System.Threading.Thread.Sleep(500);
                    Util.PrintLabel(FrameOperation, loadingIndicator, Util.NVC(row["I_ATTVAL"]));
                }
            }
            catch (Exception ex) { throw ex; }
        }

        #endregion

        #region Mehod

        private void setGridHeader()
        {
            if (LoginInfo.CFG_AREA_ID.Substring(0, 1).Equals("E"))
            {
                dgDeleteList.Columns["WIPQTY"].Visibility = Visibility.Visible;
                dgDeleteList.Columns["ORIG_WIPQTY"].Visibility = Visibility.Visible;
                dgDeleteList.Columns["WIPQTY_M"].Visibility = Visibility.Collapsed;

                dgDeleteList.Columns["MTRL_ISS_QTY"].Visibility = Visibility.Visible;
                dgDeleteList.Columns["ORIG_MTRL_ISS_QTY"].Visibility = Visibility.Visible;
                dgDeleteList.Columns["MTRL_ISS_QTY_M"].Visibility = Visibility.Collapsed;

                dgDeleteList.Columns["TCK"].Visibility = Visibility.Visible;
                dgDeleteList.Columns["WIDTH"].Visibility = Visibility.Visible;
                dgDeleteList.Columns["CONV_RATE"].Visibility = Visibility.Visible;
            }
            else
            {
                dgDeleteList.Columns["WIPQTY"].Visibility = Visibility.Collapsed;
                dgDeleteList.Columns["ORIG_WIPQTY"].Visibility = Visibility.Collapsed;
                dgDeleteList.Columns["WIPQTY_M"].Visibility = Visibility.Visible;

                dgDeleteList.Columns["MTRL_ISS_QTY"].Visibility = Visibility.Collapsed;
                dgDeleteList.Columns["ORIG_MTRL_ISS_QTY"].Visibility = Visibility.Collapsed;
                dgDeleteList.Columns["MTRL_ISS_QTY_M"].Visibility = Visibility.Visible;

                dgDeleteList.Columns["TCK"].Visibility = Visibility.Collapsed;
                dgDeleteList.Columns["WIDTH"].Visibility = Visibility.Collapsed;
                dgDeleteList.Columns["CONV_RATE"].Visibility = Visibility.Collapsed;
            }
        }

        private void LabelListData(object[] tmps)
        {
            try
            {
                DataTable dtINDATA = new DataTable();
                DataTable dtOUTDATA = new DataTable();

                dtINDATA.Columns.Add("LANGID", typeof(string));
                dtINDATA.Columns.Add("AREAID", typeof(string));
                dtINDATA.Columns.Add("EQSGID", typeof(string));
                dtINDATA.Columns.Add("PROCID", typeof(string));
                dtINDATA.Columns.Add("MTRLID", typeof(string));
                dtINDATA.Columns.Add("MLOTID", typeof(string));
                //dtINDATA.Columns.Add("ISLABEL", typeof(Boolean));
                dtINDATA.Columns.Add("INWAITWIPSTAT", typeof(Boolean));

                DataRow Indata = dtINDATA.NewRow();
                Indata["LANGID"] = tmps[0].ToString();
                Indata["AREAID"] = tmps[1].ToString();

                if (!String.IsNullOrEmpty(tmps[2].ToString()))
                    Indata["EQSGID"] = tmps[2].ToString();

                if (!String.IsNullOrEmpty(tmps[3].ToString()))
                    Indata["PROCID"] = tmps[3].ToString();

                if (!String.IsNullOrEmpty(tmps[4].ToString()))
                    Indata["MTRLID"] = tmps[4].ToString();

                if (!String.IsNullOrEmpty(txtScanId.Text))
                    Indata["MLOTID"] = txtScanId.Text;

                //Indata["ISLABEL"] = true;
                Indata["INWAITWIPSTAT"] = true;

                dtINDATA.Rows.Add(Indata);

                string bizRule = "DA_BAS_SEL_MLOT_WIP_REMAIN_DETAIL";

                loadingIndicator.Visibility = Visibility.Visible;

                new ClientProxy().ExecuteService(bizRule, "RQSTDT", "RSLTDT", dtINDATA, (dtResult, ex) =>
                {
                    loadingIndicator.Visibility = Visibility.Collapsed;
                    if (ex != null)
                    {
                        Util.MessageException(ex); //Util.AlertInfo(ex.Message);
                        return;
                    }

                    if (dtResult == null || dtResult.Rows.Count == 0)
                    {
                        Util.MessageValidation("SFU3537");  //조회된 데이터가 없습니다.
                        return;
                    }

                    Util.GridSetData(dgDeleteList, dtResult, FrameOperation);
                });
            }
            catch (Exception ex)
            {
                loadingIndicator.Visibility = Visibility.Collapsed;
                Util.MessageException(ex);
            }
        }        
        #endregion
    }
}
