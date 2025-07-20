/*************************************************************************************
Created Date : 2019.11.08
      Creator : 염규범
   Decription : 출하처 인쇄 항목 체크 리스트
--------------------------------------------------------------------------------------
 [Change History]
  2019.11.08 염규범A : Initial Created.

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
using System.Linq;
using C1.WPF.DataGrid;
using System.Windows.Media;
using System.Drawing;
using System.Collections.Generic;

namespace LGC.GMES.MES.PACK001
{
    /// <summary>
    /// Window1.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class PACK001_000_LABEL_CHECK : C1Window, IWorkArea
    {
        #region Declaration & Constructor
        private string strEqsgID = string.Empty;
        private string strProdID = string.Empty;
        private string strPouteID = string.Empty;
        private string strSeletedPCSG = string.Empty;
        private string strWoID = string.Empty;
        private string strEqsgName = string.Empty;
        private string strLableCode = string.Empty;

        private int iLableCnt = 0;

        DataTable dtLableI = new DataTable();
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
        public PACK001_000_LABEL_CHECK()
        {
            InitializeComponent();
            
        }

        private void C1Window_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                object[] tmps = C1WindowExtension.GetParameters(this);

                if (tmps != null && tmps.Length >= 1)
                {
                    strEqsgID = Util.NVC(tmps[0]);
                    strProdID = Util.NVC(tmps[1]);
                    strPouteID = Util.NVC(tmps[2]);
                    strSeletedPCSG = Util.NVC(tmps[3]);
                    strWoID = Util.NVC(tmps[4]);
                    strEqsgName = Util.NVC(tmps[5]);
                }
                else
                {
                    Util.MessageConfirm("정상적인 접근이 아닙니다.", (result) =>
                    {
                        if (result == MessageBoxResult.OK)
                        {
                            this.Close();
                        }
                    });
                }

                txtWO.Text = strWoID;
                txtLine.Text = strEqsgName;
                txtProd.Text = strProdID;

                GetCreateLotDesc(strEqsgID, strSeletedPCSG, strProdID);
                //라벨 코드 전역변수에 선언 처리
                GetEqptLabelCode(strEqsgID, strProdID);
                if(!strLableCode.Equals(""))
                {
                    GetLabelItem(strWoID, strEqsgID, strProdID);
                    //GetLabelItem2(strProdID, strEqsgID, strLableCode);
                    //GetLabelItem3(strWoID, strProdID, strEqsgID);
                }
                else
                {
                    Util.MessageInfo("설비별 프린트가 설정되지 않았습니다.");
                }

                string strAuthority = string.Empty;

                if(!LoginInfo.USERTYPE.Equals("G"))
                {
                    strAuthority = "R";
                }

                //사용자권한별로 메뉴 숨기기
                List<Button> listAuth = new List<Button>();
                listAuth.Add(btnSave);
                Util.pageAuth(listAuth, strAuthority);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        #endregion

        #region Presenter
        
        #region [ CELL 조회 시 기능 ]
        private void dgLable_LoadedCellPresenter(object sender, DataGridCellEventArgs e)
        {
            if (sender == null)
                return;

            C1DataGrid dg = sender as C1DataGrid;

            dg?.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (e.Cell.Presenter == null)
                    return;

                if (string.IsNullOrEmpty(e.Cell.Column.Name))
                    return;

                if (e.Cell.Column.Name.Contains("SCAN") || e.Cell.Column.Name.Contains("CHK"))
                {
                    dg.GetCell(e.Cell.Row.Index, e.Cell.Column.Index).Presenter.IsEnabled = true;
                }
                else
                {
                    dg.GetCell(e.Cell.Row.Index, e.Cell.Column.Index).Presenter.IsEnabled = false;
                }

                if (!e.Cell.Column.Name.Contains("ITEM"))
                    return;


                if (e.Cell.Row.Type == DataGridRowType.Item)
                {

                    if (dg.GetCell(e.Cell.Row.Index, e.Cell.Column.Index).Text.Equals("") || dg.GetCell(e.Cell.Row.Index, e.Cell.Column.Index).Text.Equals(null))
                    {
                        if (e.Cell.Column.Name.Contains("ITEM"))
                        {
                            dg.GetCell(e.Cell.Row.Index, e.Cell.Column.Index).Presenter.Foreground = new SolidColorBrush(Colors.Gray);
                            dg.GetCell(e.Cell.Row.Index, e.Cell.Column.Index).Presenter.Background = new SolidColorBrush(Colors.Gray);
                            dg.GetCell(e.Cell.Row.Index, e.Cell.Column.Index).Presenter.IsEnabled = false;
                        }

                    }
                    else
                    {
                        dg.GetCell(e.Cell.Row.Index, e.Cell.Column.Index).Presenter.Foreground = new SolidColorBrush(Colors.Black);
                        dg.GetCell(e.Cell.Row.Index, e.Cell.Column.Index).Presenter.Background = new SolidColorBrush(Colors.White);
                    }
                }
            }));



        }
        #endregion

        #endregion

        #region [ EVENT ]

        #region [ 팝업 Button ] 
        /// <summary>
        /// MesssageBoxResult 를 가지로 부모창 Pack001_000.xaml 인터페이스
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = MessageBoxResult.Cancel;
            this.Close();
            // Close();
        }

        /// <summary>
        /// 현재 미사용
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            C1DataGrid dg = sender as C1DataGrid;

            string Recive = Save(dg);

            if (!Util.NVC(Recive).Equals(null) && !Util.NVC(Recive).Equals(""))
            {
                this.DataContext = Recive;
                this.DialogResult = MessageBoxResult.OK;
                
                this.Close();
            }
        }

        private void btnWoSeeting_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(txtlotid.Text.ToString()))
            {
                Util.MessageInfo("SFU8160");
                return;
            }

            //GetEqptLabelCode(strEqsgID, strProdID);
            if (iLableCnt != GetLabelItem(strWoID, strEqsgID, strProdID))
            {
                Util.MessageInfo("SFU8161");
                return;
            }
            else
            {
                this.DialogResult = MessageBoxResult.Yes;
                this.Close();
            }
        }

        private void btnHiddenSave_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = MessageBoxResult.Yes;
            this.Close();
        }
        #endregion

        #region [ 프린트 ]
        private void btnPrint_Click(object sender, RoutedEventArgs e)
        {
            if (sender == null)
                return;

            if (string.IsNullOrEmpty(txtlotid.Text.ToString()))
            {
                Util.MessageInfo("LOT 발번 진행을 하세요.");
                return;
            }

            C1DataGrid dg = sender as C1DataGrid;

            int indexRow = ((C1.WPF.DataGrid.DataGridCellPresenter)((sender as Button).Parent)).Row.Index;
            int indexColumn = ((C1.WPF.DataGrid.DataGridCellPresenter)((sender as Button).Parent)).Column.Index;

            string strLangId = LoginInfo.LANGID;
            string strLotID = Util.NVC(txtlotid.Text.ToString());

        }

        #region [ 라벨 아이템 팝업 ]

        #region [ 현재 라벨 확인 ] 
        private void btnPrintItem_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                PACK001_000_LABEL_CHECK_POPUP popup = new PACK001_000_LABEL_CHECK_POPUP();
                popup.FrameOperation = this.FrameOperation;

                object[] tmps = C1WindowExtension.GetParameters(this);

                //헤드 2개 열 제외 처리
                int index = ((C1.WPF.DataGrid.DataGridCellPresenter)((sender as Button).Parent)).Row.Index - 2;
                DataTable dtLableItemDetail = new DataTable();
                dtLableItemDetail = DataTableConverter.Convert(dgLable.ItemsSource);

                string strLabelCode = dtLableItemDetail.Rows[index]["LABEL_CODE"].ToString();
                string strEqsgId = dtLableItemDetail.Rows[index]["EQSGID"].ToString();
                string strProdId = txtProd.Text.ToString();
                string strWoId = txtWO.Text.ToString();
                string strLastView = "N";

                if (popup != null)
                {
                    object[] Parameters = new object[5];
                    Parameters[0] = strLabelCode;
                    Parameters[1] = strEqsgId;
                    //Parameters[2] = strProcId;
                    //Parameters[3] = strEqptId;
                    Parameters[2] = strProdId;
                    Parameters[3] = strWoId;
                    Parameters[4] = strLastView;
                    //Parameters[5] = strProcId;


                    C1WindowExtension.SetParameters(popup, Parameters);

                    popup.Closed -= popup_Closed;
                    popup.Closed += popup_Closed;
                    popup.ShowModal();
                    popup.CenterOnScreen();
                    popup.BringToFront();
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        void popup_Closed(object sender, EventArgs e)
        {
            try
            {
                PACK001_000_LABEL_CHECK_POPUP popup = sender as PACK001_000_LABEL_CHECK_POPUP;

                if (popup != null)
                {
                    if (popup.DialogResult == MessageBoxResult.OK)
                    {
                        GetLabelItem(strWoID, strEqsgID, strProdID);
                        /*
                        string strLableCode = popup.DataContext.ToString();
                        
                        for (int i =0; i < dgLable.Rows.Count -2 ; i++)
                        {
                            if (strLableCode.Equals(dtLableI.Rows[i]["LABEL_CODE"].ToString()))
                            { 
                                DataTableConverter.SetValue(dtLableI.Rows[i], "CHK", "OK");
                            }
                        }
                        */

                        Util.MessageInfo("저장 되었습니다.");
                    }
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        #endregion

        #endregion

        #region [ 마지막 라벨 확인 ]
        private void btnPrintItemLast_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                PACK001_000_LABEL_CHECK_POPUP popup = new PACK001_000_LABEL_CHECK_POPUP();
                popup.FrameOperation = this.FrameOperation;

                object[] tmps = C1WindowExtension.GetParameters(this);

                //헤드 2개 열 제외 처리
                int index = ((C1.WPF.DataGrid.DataGridCellPresenter)((sender as Button).Parent)).Row.Index - 2;
                DataTable dtLableItemDetail = new DataTable();
                dtLableItemDetail = DataTableConverter.Convert(dgLable.ItemsSource);

                string strLabelCode = dtLableItemDetail.Rows[index]["LABEL_CODE"].ToString();
                string strEqsgId = dtLableItemDetail.Rows[index]["EQSGID"].ToString();
                //string strProcId = dtLableItemDetail.Rows[index]["PROCID"].ToString();
                //string strEqptId = dtLableItemDetail.Rows[index]["EQPTID"].ToString();
                string strProdId = txtProd.Text.ToString();
                string strWoId = txtWO.Text.ToString();
                string strLastView = "Y";

                if (popup != null)
                {
                    object[] Parameters = new object[5];
                    Parameters[0] = strLabelCode;
                    Parameters[1] = strEqsgId;
                    //Parameters[2] = strProcId;
                    //Parameters[3] = strEqptId;
                    Parameters[2] = strProdId;
                    Parameters[3] = strWoId;
                    Parameters[4] = strLastView;
                    //Parameters[5] = strProcId;

                    C1WindowExtension.SetParameters(popup, Parameters);

                    popup.Closed -= popup_Closed;
                    popup.Closed += popup_Closed;
                    popup.ShowModal();
                    popup.CenterOnScreen();
                    popup.BringToFront();
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        #endregion

        #region [ 라벨 체크 이력 확인 ] 
        private void btnLabelHist_Click(object sender, RoutedEventArgs e)
        {

            try
            {
                PACK001_000_LABEL_CHECK_HIST popup = new PACK001_000_LABEL_CHECK_HIST();
                popup.FrameOperation = this.FrameOperation;

                object[] tmps = C1WindowExtension.GetParameters(this);

                //헤드 2개 열 제외 처리
                int index = ((C1.WPF.DataGrid.DataGridCellPresenter)((sender as Button).Parent)).Row.Index - 2;
                DataTable dtLableItemDetail = new DataTable();
                dtLableItemDetail = DataTableConverter.Convert(dgLable.ItemsSource);

                string strLabelCode = dtLableItemDetail.Rows[index]["LABEL_CODE"].ToString();
                string strEqsgId = dtLableItemDetail.Rows[index]["EQSGID"].ToString();
                //string strProcId = dtLableItemDetail.Rows[index]["PROCID"].ToString();
                //string strEqptId = dtLableItemDetail.Rows[index]["EQPTID"].ToString();
                string strProdId = txtProd.Text.ToString();
                string strWoId = txtWO.Text.ToString();
                string strLastView = "N";

                if (popup != null)
                {
                    object[] Parameters = new object[5];
                    Parameters[0] = strLabelCode;
                    Parameters[1] = strEqsgId;
                   // Parameters[3] = strEqptId;
                    Parameters[2] = strProdId;
                    Parameters[3] = strWoId;
                    Parameters[4] = strLastView;
                    //Parameters[5] = strProcId;

                    C1WindowExtension.SetParameters(popup, Parameters);

                    popup.Closed -= popup_Closed;
                    popup.Closed += popup_Closed;
                    popup.ShowModal();
                    popup.CenterOnScreen();
                    popup.BringToFront();
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }

        }
        #endregion

        #endregion

        #region [ 체크 박스 작동 ]
        private void dgLable_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (dgLable.CurrentRow == null || dgLable.SelectedIndex == -1)
            {
                return;
            }

            string sColName = dgLable.CurrentColumn.Name;
            string strChkValue = "";

            if (!sColName.Contains("CHK"))
            {
                return;
            }

            try
            {
                if (e.ChangedButton.ToString().Equals("Left"))
                {
                    int indexRow = dgLable.CurrentRow.Index;
                    int indexColumn = dgLable.CurrentColumn.Index;

                    strChkValue = Util.NVC(dgLable.GetCell(indexRow, indexColumn).Value);

                    if (string.IsNullOrEmpty(strChkValue) || strChkValue.Equals(""))
                        return;

                    if (strChkValue == "0" || strChkValue == "False")
                    {
                        DataTableConverter.SetValue(dgLable.Rows[dgLable.CurrentRow.Index].DataItem, sColName, true);
                    }
                    else if (strChkValue == "1" || strChkValue == "True")
                    {
                        DataTableConverter.SetValue(dgLable.Rows[dgLable.CurrentRow.Index].DataItem, sColName, false);
                    }
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        #endregion

        #region [ 데이터 그리드 Pivot 기능
        /*
        private void DataGridPivotGenerated(object sender, EventArgs e)
        {
            var dg = sender as DataGrid;
            dg.Columns.Clear();
            var first = dg.ItemsSource.Cast<object>().FirstOrDefault() as IDictionary<string, object>;
            if (first == null) return;
            var names = first.Keys;
            foreach (var name in names)
            {
                dg.Columns.Add(new DataGridTextColumn { Header = name, Binding = new Binding(name) });
            }
        }
        */

                        #endregion

                        #region [ 개발자 기능 ]        
        private void txtdis_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == System.Windows.Input.MouseButtonState.Pressed &&
                (System.Windows.Input.Keyboard.Modifiers & System.Windows.Input.ModifierKeys.Control) == System.Windows.Input.ModifierKeys.Control &&
                //(System.Windows.Input.Keyboard.Modifiers & System.Windows.Input.ModifierKeys.Alt) == System.Windows.Input.ModifierKeys.Alt &&
                (System.Windows.Input.Keyboard.Modifiers & System.Windows.Input.ModifierKeys.Shift) == System.Windows.Input.ModifierKeys.Shift)
            {
                //btnHiddenSave.Visibility = Visibility.Visible;
            }
        }
        #endregion

        #endregion

        #region [ Method ]

        #region [ LOT 발번후, ROLLBACK ] 
        /// <summary>
        /// LOT 발번 SQL을 조회 해서 발번후에 ROLLBACK ( BIZ에서 처리 )
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btn_LotIDTemp_Create_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                DataTable INDATA = new DataTable();
                INDATA.TableName = "INDATA";
                INDATA.Columns.Add("PRODID", typeof(string));
                INDATA.Columns.Add("EQSGID", typeof(string));
                INDATA.Columns.Add("ROUTID", typeof(string));

                DataRow drINDATA = INDATA.NewRow();
                drINDATA["PRODID"] = strProdID;
                drINDATA["EQSGID"] = strEqsgID;
                drINDATA["ROUTID"] = strPouteID;

                INDATA.Rows.Add(drINDATA);


                DataTable dtResult = new ClientProxy().ExecuteServiceSync("BR_PRD_CREATE_LOT_PACK_TEST", "RQSTDT", "RSLTDT", INDATA);

                if (dtResult.Rows.Count > 0)
                {
                    txtlotid.Text = Util.NVC(dtResult.Rows[0]["LOTID"]).ToString();
                }
                else
                {
                    // 메세지로 변경 요청
                    Util.MessageInfo("LOT 발번에 실패하였습니다.");
                }
            }
            catch(Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        #endregion

        #region [ LOT 발번룰 조회 ]
        /// <summary>
        ///  SQL롤 된 LOT 발번룰을 가지고 LOT 발번, 
        ///  MMD에 설정되어 있는 LOT 생성 DESC을 조회 화면에 뿌려줌
        /// </summary>
        /// <param name="strEqsgID"></param>
        /// <param name="strPCSG"></param>
        /// <param name="strProdId"></param>
        /// <returns></returns>
        private string GetCreateLotDesc(string strEqsgID, string strPCSG, string strProdId)
        {
            try
            {
                
                DataTable INDATA = new DataTable();
                INDATA.TableName = "RQSTDT";
                INDATA.Columns.Add("EQSGID",                   typeof(string));
                INDATA.Columns.Add("PRDT_CLSS_CODE",           typeof(string));
                INDATA.Columns.Add("PACK_LOT_GNRT_LOGIC_TYPE", typeof(string));
                INDATA.Columns.Add("PRODID",                   typeof(string));

                DataRow drINDATA = INDATA.NewRow();
                drINDATA["EQSGID"]                   = strEqsgID;
                drINDATA["PRDT_CLSS_CODE"]           = strPCSG;
                drINDATA["PACK_LOT_GNRT_LOGIC_TYPE"] = "SQL";
                drINDATA["PRODID"]                   = strProdId;

                INDATA.Rows.Add(drINDATA);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_LOTRULE", "RQSTDT", "RSLTDT", INDATA);

                if(dtResult.Rows.Count > 0)
                {
                    txtdis.Text = dtResult.Rows[0]["LOGIC_DESC"].ToString();
                }
                else
                {
                    txtdis.Text = "LOT 발번 SAMPLE 값이 현재 입력되지 않았습니다.";
                }

            }
            catch(Exception ex)
            {
                Util.MessageException(ex);
                return null;
            }

            return null;
        }
        #endregion

        #region [ 설비별 LableCode 가져오기 ]
        /// <summary>
        /// 설비별 라벨 프린터 정보 ( PACK001_043.xaml ) 의 세팅되어 있는 Lable Code 조회
        /// </summary>
        /// <param name="sEQSG"></param>
        /// <param name="sEQPT"></param>
        /// <param name="sPROD"></param>
        /// <returns></returns>
        private void GetEqptLabelCode(string sEQSG, string sPROD)
        {
            try
            {
                DataTable RQSTDT = new DataTable();

                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("EQSGID", typeof(string));
                RQSTDT.Columns.Add("PRODID", typeof(string));
                RQSTDT.Columns.Add("USE_FLAG", typeof(string));


                DataRow dr = RQSTDT.NewRow();

                dr["LANGID"] = LoginInfo.LANGID;
                dr["EQSGID"] = sEQSG;
                dr["PRODID"] = sPROD;
                dr["USE_FLAG"] = "Y";

                RQSTDT.Rows.Add(dr);

            
                DataTable dtResult = null;
                dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_TB_SFC_EQPT_LABEL", "RQSTDT", "RSLTDT", RQSTDT);

                if (dtResult.Rows.Count > 0)
                {
                    iLableCnt = dtResult.Rows.Count;
                    strLableCode = Util.NVC(dtResult.Rows[0]["LABEL_CODE"]);
                }
                else
                {
                    strLableCode = "";
                    this.Close();
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        #endregion

        #region [ Lable HIST 가져오기 ]
        /// <summary>
        /// 라벨 Item 가져오기
        /// </summary>
        /// <param name="sEQSG"></param>
        /// <param name="sEQPT"></param>
        /// <param name="sPROD"></param>
        /// <returns></returns>
        
        private int GetLabelItem(string strWoID, string strEqsgID, string strProdID)
        {
            try
            {
                DataTable INDATA = new DataTable();

                INDATA.TableName = "INDATA";
                INDATA.Columns.Add("LANGID", typeof(string));
                INDATA.Columns.Add("WOID", typeof(string));
                INDATA.Columns.Add("EQSGID", typeof(string));
                INDATA.Columns.Add("PRODID", typeof(string));
                INDATA.Columns.Add("USE_FLAG", typeof(string));
                INDATA.Columns.Add("SHOPID", typeof(string));

                DataRow dr = INDATA.NewRow();

                dr["LANGID"] = LoginInfo.LANGID;
                dr["WOID"] = strWoID;
                dr["EQSGID"] = strEqsgID;
                dr["PRODID"] = strProdID;
                dr["USE_FLAG"] = "Y";
                dr["SHOPID"] = LoginInfo.CFG_SHOP_ID;

                INDATA.Rows.Add(dr);

                DataTable dtResult = null;
                dtResult = new ClientProxy().ExecuteServiceSync("BR_PRD_GET_LBL_ITEM_BY_PRODID", "INDATA", "OUTDATA", INDATA);

                if (dtResult.Rows.Count > 0)
                {
                    Util.GridSetData(dgLable, dtResult, FrameOperation);
                    dtLableI = DataTableConverter.Convert(dgLable.ItemsSource);

                    int temp = (from DataRow drCnt in dtResult.Rows where !drCnt["UPDUSER"].Equals("-") select drCnt).Count();
                    return temp;
                }
                else
                {
                    return 0;
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return 0;
            }
        }

        #region [ 미사용 ] 
        /*
        private void GetLabelItem4(string sPROD, string strLableCode)
        {
            try
            {
                DataTable RQSTDT = new DataTable();

                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LABEL_CODE", typeof(string));
                RQSTDT.Columns.Add("LABEL_TYPE_CODE", typeof(string));
                RQSTDT.Columns.Add("PRODID", typeof(string));
                RQSTDT.Columns.Add("SHOPID", typeof(string));

                DataRow dr = RQSTDT.NewRow();

                dr["LABEL_CODE"] = strLableCode;
                dr["LABEL_TYPE_CODE"] = "PACK";
                dr["PRODID"] = sPROD;
                dr["SHOPID"] = LoginInfo.CFG_SHOP_ID;

                RQSTDT.Rows.Add(dr);

                DataTable dtResult = null;
                dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_LABEL_ITEM_SHOPID", "RQSTDT", "RSLTDT", RQSTDT);


                DataTable dt = new DataTable();
                //GetLableCodeDataTable();

                dt = dtResult.AsEnumerable()
                           .GroupBy(r => new { Col = r["LABEL_CODE"] })
                           .Select(g => g.OrderBy(r => r["LABEL_CODE"]).First())
                           .CopyToDataTable();

                

                if (dtResult.Rows.Count > 0)
                {
                    Util.GridSetData(dgLable, dtResult, FrameOperation);
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void GetLabelItem2(string sPROD, string strEqsgID, string strLableCode)
       {
           try
           {
               DataTable INDATA = new DataTable();

               INDATA.TableName = "INDATA";
               INDATA.Columns.Add("LANGID", typeof(string));
               INDATA.Columns.Add("EQSGID", typeof(string));
               INDATA.Columns.Add("PRODID", typeof(string));
               INDATA.Columns.Add("USE_FLAG", typeof(string));
               INDATA.Columns.Add("SHOPID", typeof(string));

               DataRow dr = INDATA.NewRow();

               dr["LANGID"]   = LoginInfo.LANGID;
               dr["EQSGID"]   = strEqsgID;
               dr["PRODID"]   = sPROD;
               dr["USE_FLAG"] = "Y";
               dr["SHOPID"]   = LoginInfo.CFG_SHOP_ID;

               INDATA.Rows.Add(dr);

               DataTable dtResult = null;
               dtResult = new ClientProxy().ExecuteServiceSync("BR_PRD_GET_LABEL_ITEM_BY_PRODID", "INDATA", "OUTDATA", INDATA);

               if (dtResult.Rows.Count > 0)
               {
                   Util.GridSetData(dgLable, dtResult, FrameOperation);
               }



           }
           catch (Exception ex)
           {
               Util.MessageException(ex);
           }
       }

       private void GetLabelItem3(string strWOID, string strProdId, string strEqsgID)
       {
           try
           {
               DataTable INDATA = new DataTable();

               INDATA.TableName = "INDATA";
               INDATA.Columns.Add("LANGID", typeof(string));
               INDATA.Columns.Add("WOID", typeof(string));
               INDATA.Columns.Add("PRODID", typeof(string));
               INDATA.Columns.Add("EQSGID", typeof(string));

               DataRow dr = INDATA.NewRow();

               dr["LANGID"] = LoginInfo.LANGID;
               dr["WOID"] = strWOID;
               dr["PRODID"] = strProdId;
               dr["EQSGID"] = strEqsgID;

               INDATA.Rows.Add(dr);

               DataTable dtResult = null;
               dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_WO_LABEL_ITEM_MAX_CHK_SEQ", "INDATA", "OUTDATA", INDATA);

               if (dtResult.Rows.Count > 0)
               {
                   Util.GridSetData(dgLable, dtResult, FrameOperation);
               }
           }
           catch (Exception ex)
           {
               Util.MessageException(ex);
           }
       }
       */
        #endregion

        #endregion

        #region [ 저장 - 미사용 ] 
        private string Save(C1DataGrid dg)
        {
            try
            {
                // 총 컬럼 갯수
                int iColumnsMax;
                int iItemCnt;
                // 체크 컬럼 갯수
                int iItemChkCnt;
                //C1DataGrid dgTempLable = dgLable;
                iColumnsMax = dgLable.Columns.Count;

                DataTable dtTmp = DataTableConverter.Convert(dgLable.ItemsSource);

                //추후 사용 개발
                //string[] filter = dtTmp.Select("columnName LIKE '%{1}%'");
                //var temp = from DataRow xx in dtTmp.Rows select xx;
                //DataRow[] drTmp = dtTmp != null && dtTmp.Columns.Contains("CHK") ? dtTmp?.Select("CHK = True") : null;
                //DataRow[] drTmp2 = dtTmp != null && dtTmp.Columns.Contains("CHK") ? dtTmp?.Select("CHK != ''") : null;\
                //LableRows = dtTmp.Select();
                //LableRows2 = from xx in drTmp.ToList()
                //where xx.Field<string>("RESNCOD") == "P999000AJR"
                //select xx.Field<DataRow>("LABLECODE") ;
                //var temp = from DataRow xx in dtTmp.Rows select xx;
                //DataRow[] drTmp = dtTmp != null && dtTmp.Columns.Contains("CHK") ? dtTmp?.Select() : null;

                //DataView view = new DataView(dtTmp);
                //DataTable TempTable = view.ToTable(true, "ITEM01_CHK", "ITEM02_CHK", "ITEM03_CHK", "ITEM04_CHK", "ITEM05_CHK", "ITEM06_CHK", "ITEM07_CHK", "ITEM08_CHK", "ITEM09_CHK"
                //                                       , "ITEM10_CHK", "ITEM11_CHK", "ITEM12_CHK", "ITEM13_CHK", "ITEM14_CHK", "ITEM15_CHK");

                //int iTempRowCnt = TempTable.Rows.Count;
                //int iTempColumnsCnt = TempTable.Columns.Count;

                //총갯수
                int iTempChkCnt = 0;
                //미체크갯수
                int iTempNotChkCnt = 0;

                int index = 0;

                string strTemp = string.Empty;

                foreach (DataRow dtRow in dtTmp.Rows)
                {
                    string chkYn = string.Empty;

                    foreach (DataColumn dtc in dtTmp.Columns)
                    {
                        if (dtc.ColumnName.Contains("CHK"))
                        {

                            string strfieldChk = dtRow[dtc].ToString();

                            if (strfieldChk.Equals("0"))
                            {
                                iTempNotChkCnt++;
                                chkYn = "N";
                            }
                        }

                        if(!dtc.ColumnName.Contains("CHK") && dtc.ColumnName.Contains("ITEM"))
                        {
                            string strfieldValue = dtRow[dtc].ToString();

                            if(!strfieldValue.Equals(""))
                                iTempChkCnt++;
                        }
                    }

                    index = dtTmp.Rows.IndexOf(dtRow);

                    if (chkYn != "N")
                    {
                        if (strTemp.Equals("") || strTemp.Equals(null))
                        {
                            strTemp = dtTmp.Rows[index]["PROCID"].ToString();
                            strTemp = strTemp + ';' + dtTmp.Rows[index]["LABEL_CODE"].ToString();
                        }
                        else
                        {
                            strTemp = strTemp + ',' +  dtTmp.Rows[index]["PROCID"].ToString();
                            strTemp = strTemp +';' + dtTmp.Rows[index]["LABEL_CODE"].ToString();
                        }

                        
                    }

                }

                if(iTempNotChkCnt > 0)
                {
                    Util.MessageInfo("총 : " + iTempChkCnt + "체크 항목중 - 현재 미체크 된 항목 : " + iTempNotChkCnt );

                    return null;
                }
                else
                {
                    return strTemp;
                }
               
            }
            catch(Exception ex)
            {
                Util.MessageException(ex);
                return null;
            }
        }
        #endregion

        #endregion
    }
}

