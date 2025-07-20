/****************************************************************************************************
 Created Date : 2022.04.28
      Creator : 이정미
   Decription : JIG Unload Dummy Tray 관리
-----------------------------------------------------------------------------------------------------
 [Change History]
  2022.04.28  이정미 : Initial Created
  2022.06.03  이정미 : 오류 수정
  2022.07.06  이정미 : Receive_ScanMsg 함수 호출 BIZ 변경 - 물류 로직 변경에 따라, UI전용 BIZ 개발 
  2022.07.11  이정미 : 입력 버튼 클릭시 편집중인 내역 Commit
*****************************************************************************************************/

using C1.WPF;
using C1.WPF.DataGrid;
using C1.WPF.DataGrid.Summaries;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Globalization;
using System.Windows.Documents;
using LGC.GMES.MES.CMM001;
using LGC.GMES.MES.CMM001.Extensions;

namespace LGC.GMES.MES.FCS002
{
    /// <summary>
    /// TSK_120.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class FCS002_129 : UserControl, IWorkArea
    {
        public FCS002_129()
        {
            InitializeComponent();
            this.Loaded += UserControl_Loaded;
        }

        #region Initialize
        /// <summary>
        /// Frame과 상호작용하기 위한 객체
        /// </summary>#region Initialize
        /// <summary>
        /// Frame과 상호작용하기 위한 객체
        /// </summary>
        public IFrameOperation FrameOperation
        {
            get;
            set;
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            InitCombo();
            txtTrayCellID.Focus();
            // InitControl();
            //  SetEvent();
            CmdMultiCellDataGridRowAdd(dgMultiCell, 100);
        }

        private void InitCombo()
        {
            CommonCombo_Form_MB _combo = new CommonCombo_Form_MB();
            _combo.SetCombo(cboEqp, CommonCombo_Form_MB.ComboStatus.NONE, sCase: "JIG_EQPT");
        }

        #endregion

        #region Method

        #region Dummy Tray 생성  
        private bool Receive_ScanMsg(string sScan)
        {
            try
            {
                if (string.IsNullOrEmpty(sScan) || sScan.Length < 10)
                {
                    Util.Alert("ME_0205"); //잘못된 ID입니다.
                    return true;
                }

                //다중 입력시
                if (sScan.Length > 10)
                    sScan = sScan.Substring(0, 10);

                //입력값 앞 4자리로 TRAY/CELL 구분하기
                if (GetCheckTrayOrCell(sScan.Substring(0, 4)))
                {
                    if (!GetCheckTrayStatus(sScan))
                    {
                        Util.Alert("ME_0217"); //정보 변경 가능한 Tray가 아닙니다.
                        return true;
                    }

                    //값이 없을 경우에만 Tray 정보 Setting
                    if (string.IsNullOrEmpty(txtDummyTrayID.Text))
                    {
                        txtDummyTrayID.Text = sScan.Trim();
                        GetCellCnt(txtDummyTrayID.Text);    
                        return false;
                    }
                }
                else
                {
                    //Cell일 경우
                    if (string.IsNullOrEmpty(txtDummyTrayID.Text))
                    {
                        Util.Alert("ME_0080"); //Tray를 먼저 입력해주세요.
                        return true;
                    }

                    //스프레드에 있는지 확인

                    for (int i = 0; i < dgCell.GetRowCount(); i++)
                    {
                        if (DataTableConverter.GetValue(dgCell.Rows[i].DataItem, "PROD_LOTID").ToString() == sScan)
                        {
                            Util.Alert("ME_0193", new string[] { txtDummyTrayID.Text });  //이미 스캔한 ID 입니다.
                            return true;
                        }
                    }

                    string sGrade = "";
                    string sSpecial = "";

                    //CELL 공정 완료 체크 
                    if (GetCellCheck(sScan, ref sGrade, ref sSpecial))
                    {
                        Util.Alert("ME_0205"); //잘못된 ID입니다.
                        return true;
                    }

                    //최초 CELL 일경우
                    if (dgCell.Rows.Count == 0)
                    {
                        if (!GetFirstCellCheck(sScan))
                            return true;
                    }
                    //최초가 아닐경우 special 비교 
                    else
                    {
                        if (!sSpecial.Equals(dgCell.GetValue(0, "SPLT_FLAG")))
                        {
                            Util.Alert("ME_0344"); //특별 CELL 정보가 다릅니다.
                            return true;
                        }
                    }
                    //모든 체크 끝나면 데이터 추가 
                    CmdCellDataGridRowAdd(dgCell, 1, sScan, ref sGrade, ref sSpecial);
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
            return true;

        }

        private bool GetCheckTrayOrCell(string sTray)
        {
            try
            {
                DataTable dtRqst = new DataTable();
                dtRqst.TableName = "RQSTDT";
                dtRqst.Columns.Add("CST_TYPE_CODE", typeof(string));

                DataRow dr = dtRqst.NewRow();
                dr["CST_TYPE_CODE"] = sTray;
                dtRqst.Rows.Add(dr);

                // DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_SEL_TM_TRAY_TYPE", "RQSTDT", "RSLTDT", dtRqst);
                 DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_TB_MMD_FCS_CST_TYPE_FL_UI", "RQSTDT", "RSLTDT", dtRqst);

                if (dtRslt.Rows.Count > 0)
                    return true;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
            return false;
        }

        private bool GetCheckTrayStatus(string sTray)
        {
            bool bCheck = false;
            try
            {
                DataTable dtRqst = new DataTable();
                dtRqst.TableName = "INDATA";
                dtRqst.Columns.Add("SRCTYPE", typeof(string));
                dtRqst.Columns.Add("IFMODE", typeof(string));
                dtRqst.Columns.Add("USERID", typeof(string));
                dtRqst.Columns.Add("CSTID", typeof(string));
                dtRqst.Columns.Add("EQPTID", typeof(string));

                DataRow dr = dtRqst.NewRow();
                dr["SRCTYPE"] = "UI";
                dr["IFMODE"] = "Online";
                dr["USERID"] = LoginInfo.USERID;
                dr["CSTID"] = sTray;
                dr["EQPTID"] = Util.GetCondition(cboEqp, sMsg: "ME_0171");// 설비를 선택해주세요.
                if (string.IsNullOrEmpty(dr["EQPTID"].ToString())) return false;

                dtRqst.Rows.Add(dr);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("BR_GET_JIG_FORM_TRAY_EMPTY_CHECK", "INDATA", "OUTDATA", dtRqst);
                if (dtRslt.Rows.Count > 0 && dtRslt.Rows[0]["RETVAL"].ToString().Equals("0")) ;
                bCheck = true;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
            return bCheck;
        }

        private bool GetCellCheck(string sCellId, ref string sGrade, ref string sSpecial)
        {
            bool bCheck = true;
            try
            {
                DataTable dtRqst = new DataTable();
                dtRqst.TableName = "RQSTDT";
                dtRqst.Columns.Add("SUBLOTID", typeof(string));

                DataRow dr = dtRqst.NewRow();
                dr["SUBLOTID"] = sCellId;
                dtRqst.Rows.Add(dr);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_SEL_CELL_CHECK_JIG_DUMMY", "RQSTDT", "RSLTDT", dtRqst);

                if (dtRslt.Rows.Count > 0)//dtRqst
                {
                    bCheck = false;
                    sGrade = dtRslt.Rows[0]["FINL_JUDG_CODE"].ToString();
                    sSpecial = dtRslt.Rows[0]["SPLT_FLAG"].ToString();
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
            return bCheck;
        }

        private bool GetFirstCellCheck(string sCellId)
        {
            bool isSuccess = true;
            try
            {
                DataTable dtRqst = new DataTable();
                dtRqst.TableName = "RQSTDT";
                dtRqst.Columns.Add("SUBLOTID", typeof(string));

                DataRow dr = dtRqst.NewRow();
                dr["SUBLOTID"] = sCellId;
                dtRqst.Rows.Add(dr);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_SEL_CELL_CHECK_JIG_DUMMY", "RQSTDT", "RSLTDT", dtRqst);

                if (dtRslt.Rows.Count > 0)
                {
                    txtDummyLotID.Text = dtRslt.Rows[0]["PROD_LOTID"].ToString().Trim();
                    txtDummyProdCD.Text = dtRslt.Rows[0]["PRODID"].ToString().Trim();
                    txtDummyRoute.Text = dtRslt.Rows[0]["ROUTID"].ToString().Trim();
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                isSuccess = false;
            }
            return isSuccess;
        }

        private void GetCellCnt(string TrayID)
        {
            try
            {
                DataTable dtRqst = new DataTable();
                dtRqst.TableName = "RQSTDT";
                dtRqst.Columns.Add("CST_TYPE_CODE", typeof(string));

                DataRow dr = dtRqst.NewRow();
                dr["CST_TYPE_CODE"] = TrayID.Substring(0, 4);
                dtRqst.Rows.Add(dr);
                
                //DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_SEL_TM_TRAY_TYPE", "RQSTDT", "RSLTDT", dtRqst);
                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_TB_MMD_FCS_CST_TYPE_FL_UI", "RQSTDT", "RSLTDT", dtRqst);
                txtDummyCellCnt.Text = dtRslt.Rows[0]["CST_CELL_QTY"].ToString();
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        #endregion

        //MDI PARENT 에서 읽은 BCR 코드 처리하기
        public void SetChildBcr(string sBcrText)
        {
            if (LoginInfo.AUTHID.Equals("W")) /////////////확인 필요 
            {
                txtTrayCellID.Text = sBcrText.ToUpper();
                txtTrayCellID.Text = txtTrayCellID.Text.ToUpper();
            }
        }

        private void ShowHoldDetail(string pLotid)
        {
            /* COM001_018_HOLD_DETL wndRunStart = new COM001_018_HOLD_DETL();
             wndRunStart.FrameOperation = FrameOperation;

             if (wndRunStart != null)
             {
                 object[] Parameters = new object[1];
                 Parameters[0] = pLotid;
                 C1WindowExtension.SetParameters(wndRunStart, Parameters);

                 wndRunStart.ShowModal();
             }*/

        }

        private void CmdMultiCellDataGridRowAdd(C1.WPF.DataGrid.C1DataGrid dg, int iRowcount)
        {
            try
            {
                DataTable dt = new DataTable();

                foreach (C1.WPF.DataGrid.DataGridColumn col in dg.Columns)
                {
                    dt.Columns.Add(Convert.ToString(col.Name));
                }
                if (dg.ItemsSource != null)
                {
                    for (int i = 0; i < iRowcount; i++)
                    {
                        dt = DataTableConverter.Convert(dg.ItemsSource);

                        DataRow dr = dt.NewRow();
                        dr["CELL_CNT"] = i + 1;
                        dr["CELL_ID"] = "";
                        dt.Rows.Add(dr);
                        dg.BeginEdit();
                        dg.ItemsSource = DataTableConverter.Convert(dt);
                        dg.EndEdit();
                    }
                }
                else
                {
                    for (int i = 0; i < iRowcount; i++)
                    {                
                        DataRow dr = dt.NewRow();
                        dr["CELL_CNT"] = i + 1;
                        dr["CELL_ID"] = "";
                        dt.Rows.Add(dr);
                        dg.BeginEdit();
                        dg.ItemsSource = DataTableConverter.Convert(dt);
                        dg.EndEdit();
                    }
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void CmdCellDataGridRowAdd(C1.WPF.DataGrid.C1DataGrid dg, int iRowcount, string sScan, ref string sGrade, ref string sSpecial)
        {
            try
            {
                DataTable dt = new DataTable();

                foreach (C1.WPF.DataGrid.DataGridColumn col in dg.Columns)
                {
                    dt.Columns.Add(Convert.ToString(col.Name));
                }
                if (dg.ItemsSource != null)
                {
                    for (int i = 0; i < iRowcount; i++)
                    {
                        dt = DataTableConverter.Convert(dg.ItemsSource);

                        DataRow dr = dt.NewRow();
                        dr["PROD_LOTID"] = sScan;
                        dr["FINL_JUDG_CODE"] = sGrade;
                        dr["SPLT_FLAG"] = sSpecial;
                        dt.Rows.Add(dr);
                        dg.BeginEdit();
                        dg.ItemsSource = DataTableConverter.Convert(dt);
                        dg.EndEdit();
                    }
                }
                else
                {
                    for (int i = 0; i < iRowcount; i++)
                    {
                        DataRow dr = dt.NewRow();
                        dr["PROD_LOTID"] = sScan;
                        dr["FINL_JUDG_CODE"] = sGrade;
                        dr["SPLT_FLAG"] = sSpecial;
                        dt.Rows.Add(dr);
                        dg.BeginEdit();
                        dg.ItemsSource = DataTableConverter.Convert(dt);
                        dg.EndEdit();
                    }
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }


        #endregion

        #region [Event]

        private void btnCreateDummy_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                int iCurCnt = 0;
                for (int i = 0; i < dgCell.Rows.Count; i++)
                {
                    if (!dgCell.GetValue(i, "PROD_LOTID").Equals("0000000000"))//CELL_ID
                        iCurCnt++;
                }


                if (iCurCnt == 0)
                    return;

                DataTable dtCell = new DataTable();
                dtCell.TableName = "CELLDATA";
                dtCell.Columns.Add("SUBLOTID", typeof(string));
                dtCell.Columns.Add("CSTSLOT", typeof(string));

                for (int i = 0; i < dgCell.Rows.Count; i++)
                {
                    DataRow drCell = dtCell.NewRow();
                    drCell["SUBLOTID"] = dgCell.GetValue(i, "PROD_LOTID");
                    drCell["CSTSLOT"] = "1";

                    dtCell.Rows.Add(drCell);
                }

                for (int i = dgCell.Rows.Count; i < Convert.ToInt16(txtDummyCellCnt.Text); i++)
                {
                    if (txtDummyCellCnt.Text.Equals(null))
                    {
                        Util.Alert("ME_0170");
                        return;
                    }

                    DataRow drCell = dtCell.NewRow();
                    drCell["SUBLOTID"] = "";
                    drCell["CSTSLOT"] = "1";

                    dtCell.Rows.Add(drCell);
                }

                string sSpecial = dgCell.GetValue(0, "SPLT_FLAG").Equals("N") ? "0" : "1";//SPCL_FLAG

                //DataTable inDataTable = new DataTable();
                DataTable dtRqst = new DataTable();
                //inDataTable.TableName = "INDATA";
                dtRqst.TableName = "INDATA";
                dtRqst.Columns.Add("EQPTID", typeof(string));
                dtRqst.Columns.Add("CSTID", typeof(string));
                dtRqst.Columns.Add("CELL_COUNT", typeof(string));
                dtRqst.Columns.Add("DAY_GR_LOTID", typeof(string));
                dtRqst.Columns.Add("USERID", typeof(string));
                dtRqst.Columns.Add("DUMMY_FLAG", typeof(string));
                dtRqst.Columns.Add("SRCTYPE", typeof(string));
                dtRqst.Columns.Add("IFMODE", typeof(string));

                DataRow dr = dtRqst.NewRow();
                dr["EQPTID"] = Util.GetCondition(cboEqp, bAllNull: true);
                dr["CSTID"] = Util.GetCondition(txtDummyTrayID, sMsg: "ME_0070"); //Tray ID를 입력해주세요.
                if (string.IsNullOrEmpty(dr["CSTID"].ToString())) return;
                dr["CELL_COUNT"] = Util.GetCondition(txtDummyCellCnt, bAllNull: true);
                dr["DAY_GR_LOTID"] = Util.GetCondition(txtDummyLotID, bAllNull: true).Substring(0, 8);
                    //Util.GetCondition(txtDummyRoute, bAllNull: true) + Util.GetCondition(txtDummyLotID, bAllNull: true) + sSpecial; //수정하기 
                dr["USERID"] = LoginInfo.USERID;
                dr["DUMMY_FLAG"] = "Y";
                dr["SRCTYPE"] = "UI";
                dr["IFMODE"] = "OFF";  

                dtRqst.Rows.Add(dr);

                DataSet dsRqst = new DataSet();
                dsRqst.Tables.Add(dtRqst);
                dsRqst.Tables.Add(dtCell);

                DataSet dsRslt = new ClientProxy().ExecuteServiceSync_Multi("BR_SET_JIG_FORM_ULD_TRAY_CREATE", "INDATA,CELLDATA", "RSLTDT", dsRqst);

                if (dsRslt.Tables[0].Rows[0]["RETVAL"].ToString().Equals("0"))
                {
                    Util.MessageConfirm("ME_0160", (result) => //생성완료하였습니다.
                    {
                        if (result == MessageBoxResult.OK)
                        {
                            btnClear_Click(null, null);
                        }
                        else
                        {
                            return;
                        }
                    });
                }
                else
                    Util.Alert("ME_0159", new string[] { dsRslt.Tables[0].Rows[0]["RETVAL"].ToString() });
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void btnClear_Click(object sender, EventArgs e)
        {
            //fpsCell.ActiveSheet.Rows.Clear();

            txtDummyTrayID.Text = "";
            txtDummyCellCnt.Text = "";
            txtDummyLotID.Text = "";
            txtDummyProdCD.Text = "";
            txtDummyRoute.Text = "";

            if (dgCell.Rows.Count == 0)
                return;

            if (dgCell.Rows.Count > 0)
                dgCell.ClearRows();
        }

        private void txtTrayCellID_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                txtTrayCellID.Focus();
                Receive_ScanMsg(txtTrayCellID.Text.ToUpper());
            }
        }

        private void btnCellID_Click(object sender, RoutedEventArgs e)
        {
            Receive_ScanMsg(txtTrayCellID.Text.ToUpper());
            txtTrayCellID.Text = string.Empty;
        }

        /* private void btnSearch_Click(object sender, RoutedEventArgs e)
         {

         }*/

        /* private void btnSearch_PreviewMouseDown(object sender, MouseButtonEventArgs e)
          {

          }*/


        private void btnDeleteCell_Click(object sender, RoutedEventArgs e)
        {
            if (dgCell.Rows.Count == 0)
                return;

            if(dgCell.Rows.Count > 0 )
                dgCell.ClearRows();

            txtDummyLotID.Text = "";
            //fpsCell.ActiveSheet.Rows.Clear();
            //Util.SetTextBoxReadOnly(txtDummyLotID, string.Empty);
        }

        /* private void fpsCell_ButtonClicked(object sender, FarPoint.Win.Spread.EditorNotifyEventArgs e)
         {
             fpsCell.ActiveSheet.RemoveRows(e.Row, 1);
         }*/

        private void dgCellList_PreviewMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            //fpsCell.ActiveSheet.RemoveRows(e.Row, 1);
        }

        private void btnMultiCell_Click(object sender, RoutedEventArgs e)
        {
            //편집중인 내역 Commit.
            dgMultiCell.EndEdit(true);

            for (int i = 0; i < dgMultiCell.Rows.Count; i++) // Count = 100.....수정 필요 
            {
                /*if (string.IsNullOrEmpty(dgMultiCell.GetValue(i, "CELL_ID").ToString()))
                    return;*/

                if (!string.IsNullOrEmpty(dgMultiCell.GetValue(i, "CELL_ID").ToString()))
                {
                    Receive_ScanMsg(dgMultiCell.GetValue(i, "CELL_ID").ToString());
                    dgMultiCell.SetValue(i, "CELL_ID", string.Empty);
                }
            }
        }
        ///아래는 확인 필요 
        private void dgCellList_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            try
            {
                C1.WPF.DataGrid.C1DataGrid dg = sender as C1.WPF.DataGrid.C1DataGrid;
                /*  if (dg.CurrentColumn.Name.Equals("WIPHOLD") && DataTableConverter.GetValue(dg.CurrentRow.DataItem, "WIPHOLD").ToString() == "Y"
                      || dg.CurrentColumn.Name.Equals("LOTID") && DataTableConverter.GetValue(dg.CurrentRow.DataItem, "WIPHOLD").ToString() == "Y")
                  {
                      ShowHoldDetail(Util.NVC(DataTableConverter.GetValue(dg.CurrentRow.DataItem, "LOTID")));
                  }*/
                // Tray No , Tray ID, 공정경로 더블클릭 시 팝업 창(다른화면)이동                
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
    }
}