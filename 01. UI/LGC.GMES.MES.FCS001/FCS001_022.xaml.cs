/*************************************************************************************
 Created Date :
      Creator : 
   Decription : Cell 정보조회
--------------------------------------------------------------------------------------
 [Change History]
  2020.10.07  NAME : Initial Created
  2021.04.01  KDH : 링크 기능을 Head -> Cell 로 전환 및 Bold 제거
  2021.04.08  KDH : Cell ID 입력 가능하도록 수정
  2021.04.13  KDH : 화면간 이동 시 초기화 현상 제거
  2022.02.22  KDH : AREA 조건 추가
  2022.06.03  KDH : COLDPRESS_IR_VALUE 컬럼 추가
  2022.08.31  강동희 : Cell ID 입력/조회 후 Cell ID Block 설정
  2022.11.24  조영대 : Tray Lot ID 별 마지막 등급으로 일치 처리
  2023.08.15  손동혁 : NA 1동 요청 (주액량 판정 , 주액량 판정 세부사항 ) 컬럼 동별공통코드 사용 NA 1동만 보이게 추가                   
  2023.10.14  조영대 : Cell 정보 없을시 클리어
  2024.01.03  조영대 : (-)Lead Pin 접촉 IV (V) 제거, (+)Lead Pin 접촉 IV (V) -> 절연전압 IV (V)
  2024.07.25  최석준 : RTN_NO 표시 추가, Sorting Cell 이력표시 추가 (2025년 적용예정, 수정 시 연락부탁드립니다)
  2024.09.04  임정훈 : HOLD status, Cell condition 표시 추가 및 팔레트 클릭시 기간별 Pallet 확정 이력 정보 조회로 연계
  2025.04.18  이원용 : 방전 CCCV 용량 항목 추가
  2025.06.16  이원용 : TFS -> Git 누락된 항목 추가 
**************************************************************************************/
using C1.WPF.DataGrid;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;
using System;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace LGC.GMES.MES.FCS001
{
    /// <summary>
    /// FCS001_022.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class FCS001_022 : UserControl, IWorkArea
    {
        #region Declaration & Constructor 
        private string _CellId;

        private string _sActYN = "N";
        private string _PROCESSING_GROUP_ID = string.Empty;

        Util _Util = new Util();

        bool bUseFlag = false; //2023.08.15 컬럼 동별공통코드 사용 NA1동만 보이게 추가 테스트 후 삭제

        public string CELLID
        {
            set { this._CellId = value; }
            get { return this._CellId; }
        }

        public string ACTYN
        {
            set { this._sActYN = value; }
            get { return this._sActYN; }
        }
        #endregion

        #region [Initialize]
        public FCS001_022()
        {
            InitializeComponent();
        }

        public IFrameOperation FrameOperation
        {
            get;
            set;
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {


            ///2023.08.15
            bUseFlag = _Util.IsAreaCommonCodeUse("PROGRAM_BY_FUNC_USE_FLAG", "FCS001_062"); //동별공통코드에 기준정보 등록되어 있고, Attr1 속성의 값이 "Y"인 경우 기능 사용 가능.
            if (bUseFlag)
            {
                dgCell.Columns["RESULT_JUDG"].Visibility = System.Windows.Visibility.Visible;
                dgCell.Columns["RESULT_DETAIL"].Visibility = System.Windows.Visibility.Visible;
            }
            else
            {

                dgCell.Columns["RESULT_JUDG"].Visibility = System.Windows.Visibility.Collapsed;
                dgCell.Columns["RESULT_DETAIL"].Visibility = System.Windows.Visibility.Collapsed;

            }

            object[] parameters = this.FrameOperation.Parameters;
            if (parameters != null && parameters.Length >= 1)
            {
                _CellId = Util.NVC(parameters[0]);
                _sActYN = Util.NVC(parameters[1]);
            }
            //다른화면에서 넘어온 경우
            if (!string.IsNullOrEmpty(_CellId) && _sActYN.Equals("Y"))
            {
                _sActYN = "N";

                //txtCellID.IsReadOnly = true; //20210408 Cell ID 입력 가능하도록 수정
                txtCellID.IsReadOnly = false;  //20210408 Cell ID 입력 가능하도록 수정
                txtCellID.Text = _CellId;

                btnSearch_Click(null, null);
            }
            else
            {
                txtCellID.IsReadOnly = false;
                txtCellID.Text = string.Empty;
            }

            //if(GetOcopRtnPsgArea())
            //{
            //    txtblRtnNo.Visibility = System.Windows.Visibility.Visible;
            //    txtRtnNo.Visibility = System.Windows.Visibility.Visible;
            //}
            //else
            //{
            //    txtblRtnNo.Visibility = System.Windows.Visibility.Collapsed;
            //    txtRtnNo.Visibility = System.Windows.Visibility.Collapsed;
            //}

            this.Loaded -= UserControl_Loaded; //2021.04.13 화면간 이동 시 초기화 현상 제거
        }
        #endregion

        #region[Method]
        private void GetList()
        {
            try
            {
                ClearControl();

                DataTable dtRqst = new DataTable();
                dtRqst.TableName = "RQSTDT";
                dtRqst.Columns.Add("LANGID", typeof(string));
                dtRqst.Columns.Add("SUBLOTID", typeof(string));

                DataRow dr = dtRqst.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["SUBLOTID"] = Util.GetCondition(txtCellID, sMsg: "FM_ME_0019");  //Cell ID를 입력해주세요.
                if (string.IsNullOrEmpty(dr["SUBLOTID"].ToString()))
                {
                    return;
                }
                dtRqst.Rows.Add(dr);

                DataSet dsRqst = new DataSet();
                dsRqst.Tables.Add(dtRqst);

                // 백그라운드 실행, 실행 완료 후 dgHist_ExecuteDataCompleted 이벤트 호출
                dgHist.ExecuteService("BR_GET_PROCESS_RETRIEVE_INFO", "RQSTDT", "INFO1,INFO2", dsRqst, bindTableName: "INFO2");


            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
            finally
            {
                HiddenLoadingIndicator();
            }
        }

        private void GetProcessGradeInfo()
        {
            try
            {
                DataTable dtRqst = new DataTable();
                dtRqst.TableName = "RQSTDT";
                dtRqst.Columns.Add("LANGID", typeof(string));
                dtRqst.Columns.Add("AREAID", typeof(string)); //2022.02.22_AREA 조건 추가
                dtRqst.Columns.Add("SUBLOTID", typeof(string));

                DataRow dr = dtRqst.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["AREAID"] = LoginInfo.CFG_AREA_ID; //2022.02.22_AREA 조건 추가
                dr["SUBLOTID"] = Util.GetCondition(txtCellID);
                if (string.IsNullOrEmpty(dr["SUBLOTID"].ToString())) return;
                dtRqst.Rows.Add(dr);

                // 백그라운드로 실행, 결과 처리시 ExecuteComplete 이벤트 사용
                dgCell.ExecuteService("DA_SEL_PROCESS_GRADER", "RQSTDT", "RSLTDT", dtRqst);

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
            finally
            {
                HiddenLoadingIndicator();
            }
        }

        private bool GetOcopRtnPsgArea()
        {
            DataTable inTable = new DataTable();
            inTable.Columns.Add("LANGID", typeof(string));
            inTable.Columns.Add("CMCDTYPE", typeof(string));
            inTable.Columns.Add("CMCODE", typeof(string));

            DataRow dr = inTable.NewRow();
            dr["LANGID"] = LoginInfo.LANGID;
            dr["CMCDTYPE"] = "FORM_OCOP_RTN_PSG_YN";
            dr["CMCODE"] = LoginInfo.CFG_AREA_ID;

            inTable.Rows.Add(dr);

            DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_COMMONCODE_TBL", "INDATA", "OUTDATA", inTable);

            if (dtRslt.Rows.Count > 0)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        private void ClearControl()
        {
            txtCellNo.Text = string.Empty;
            txtRouteID.Text = string.Empty;
            txtTrayNo.Text = string.Empty;

            txtLotID.Text = string.Empty;
            txtOper.Text = string.Empty;
            txtCreateTime.Text = string.Empty;
            txtTrayID.Text = string.Empty;

            txtHoldStatus.Text = string.Empty;
            txtCellCondition.Text= string.Empty;
        }
        #endregion

        #region [Event]
        private void btnSearch_Click(object sender, EventArgs e)
        {
            GetList();
            GetProcessGradeInfo();

            if (!string.IsNullOrEmpty(txtCellID.Text))
            {
                txtCellID.SelectAll();
                txtCellID.Focus();
            }

        }

        private void txtCellID_KeyDown(object sender, KeyEventArgs e)
        {
            if ((!string.IsNullOrEmpty(txtCellID.Text)) && (e.Key == Key.Enter))
            {
                btnSearch_Click(null, null);
            }
        }

        private void dgHist_ExecuteDataCompleted(object sender, CMM001.Controls.UcBaseDataGrid.ExecuteDataEventArgs e)
        {
            DataSet dsRslt = e.ResultData as DataSet;
            
            if (dsRslt.Tables["INFO1"].Rows.Count > 0)
            {
                txtCellID.Text = Util.NVC(dsRslt.Tables["INFO1"].Rows[0]["SUBLOTID"].ToString());
                txtCellNo.Text = Util.NVC(dsRslt.Tables["INFO1"].Rows[0]["CSTSLOT"].ToString());
                txtRouteID.Text = Util.NVC(dsRslt.Tables["INFO1"].Rows[0]["ROUTID"].ToString());
                txtTrayNo.Text = Util.NVC(dsRslt.Tables["INFO1"].Rows[0]["LOTID"].ToString());
                txtHoldStatus.Text = GetHoldStatus(dsRslt);

                txtLotID.Text = Util.NVC(dsRslt.Tables["INFO1"].Rows[0]["PROD_LOTID"].ToString());
                txtOper.Text = Util.NVC(dsRslt.Tables["INFO1"].Rows[0]["PROCID"].ToString());
                txtCreateTime.Text = Util.NVC(dsRslt.Tables["INFO1"].Rows[0]["LOTDTTM_CR"].ToString());
                txtTrayID.Text = Util.NVC(dsRslt.Tables["INFO1"].Rows[0]["CSTID"].ToString());
                txtCellCondition.Text = GetCellCondition(dsRslt);
                //txtRtnNo.Text = Util.NVC(dsRslt.Tables["INFO1"].Rows[0]["RTN_NO"].ToString());
                _PROCESSING_GROUP_ID = Util.NVC(dsRslt.Tables["INFO1"].Rows[0]["PROCESSING_GROUP_ID"].ToString());
            }
            else
            {
                dgHist.ClearRows();
                dgCell.ClearRows();
            }

            // Tray Lot ID 별 마지막 등급으로 일치 처리
            string saveTrayLotId = string.Empty;
            string saveSublotJudge = string.Empty;
            for (int row = dsRslt.Tables["INFO2"].Rows.Count - 1; row >= 0; row--)
            {
                if (!saveTrayLotId.Equals(dsRslt.Tables["INFO2"].Rows[row]["LOTID"]))
                {
                    saveTrayLotId = Util.NVC(dsRslt.Tables["INFO2"].Rows[row]["LOTID"]);
                    saveSublotJudge = Util.NVC(dsRslt.Tables["INFO2"].Rows[row]["SUBLOTJUDGE"]);
                    continue;
                }

                if (string.IsNullOrEmpty(saveSublotJudge))
                {
                    saveSublotJudge = Util.NVC(dsRslt.Tables["INFO2"].Rows[row]["SUBLOTJUDGE"]);
                }
                else
                {
                    dsRslt.Tables["INFO2"].Rows[row]["SUBLOTJUDGE"] = saveSublotJudge;
                }
            }

        }

        private void dgCell_ExecuteDataCompleted(object sender, CMM001.Controls.UcBaseDataGrid.ExecuteDataEventArgs e)
        {

        }
        #endregion

        private void dgHist_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            try
            {
                Point pnt = e.GetPosition(null);
                C1.WPF.DataGrid.DataGridCell cell = dgHist.GetCellFromPoint(pnt);

                if (cell != null)
                {
                    string sTrayNo = Util.NVC(DataTableConverter.GetValue(dgHist.Rows[cell.Row.Index].DataItem, "LOTID")).ToString();
                    string sRouteType = Util.NVC(DataTableConverter.GetValue(dgHist.Rows[cell.Row.Index].DataItem, "ROUT_GR_CODE")).ToString();

                    if (!cell.Column.Name.Equals("CSTID") && !cell.Column.Name.Equals("LOTID") && !cell.Column.Name.Equals("ROUTID"))
                    {
                        return;
                    }
                    if (cell.Column.Name.Equals("LOTID") && sRouteType.Equals("P"))
                    {
                        //string sTrayNo = Util.NVC(DataTableConverter.GetValue(dgHist.Rows[cell.Row.Index].DataItem, "LOTID")).ToString();

                        // 프로그램 ID 확인 후 수정
                        object[] parameters = new object[3];
                        parameters[0] = LoginInfo.CFG_AREA_ID;
                        parameters[1] = LoginInfo.CFG_SHOP_ID;
                        parameters[2] = _PROCESSING_GROUP_ID;
                        this.FrameOperation.OpenMenu("SFU010736080", true, parameters); //기간별 Pallet 확정 이력 정보 조회 연계
                    }

                    else if (cell.Column.Name.Equals("CSTID") || cell.Column.Name.Equals("LOTID"))
                    {
                        string sTrayId =  Util.NVC(DataTableConverter.GetValue(dgHist.Rows[cell.Row.Index].DataItem, "CSTID")).ToString();
                        //string sTrayNo = Util.NVC(DataTableConverter.GetValue(dgHist.Rows[cell.Row.Index].DataItem, "LOTID")).ToString();

                        // 프로그램 ID 확인 후 수정
                        object[] parameters = new object[6];
                        parameters[0] = sTrayId;
                        parameters[1] = sTrayNo;
                        parameters[2] = string.Empty;
                        parameters[3] = string.Empty;
                        parameters[4] = string.Empty;
                        parameters[5] = "Y";
                        this.FrameOperation.OpenMenu("SFU010710010", true, parameters); //Tray 정보조회 연계
                    }
                    else if (cell.Column.Name.Equals("ROUTID"))
                    {
                        //DataTable dtRqst = new DataTable();
                        //dtRqst.TableName = "RQSTDT";
                        //dtRqst.Columns.Add("LOTID", typeof(string));

                        //DataRow dr = dtRqst.NewRow();
                        //dr["LOTID"] = Util.NVC(DataTableConverter.GetValue(dgHist.Rows[cell.Row.Index].DataItem, "LOTID")).ToString();
                        //dtRqst.Rows.Add(dr);

                        //DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_SEL_LINE_MODEL_ROUTE_FROM_TRAY", "RQSTDT", "RSLTDT", dtRqst);

                        //string sLineId = Util.NVC(dtRslt.Rows[0]["EQSGID"].ToString());
                        //string sModelId = Util.NVC(dtRslt.Rows[0]["MDLLOT_ID"].ToString());
                        //string sRouteId = Util.NVC(dtRslt.Rows[0]["ROUTID"].ToString());
                        
                        //if (string.IsNullOrEmpty(sRouteId))
                        //{
                        //    return;
                        //}

                        //object[] parameters = new object[4];
                        //parameters[0] = sLineId;
                        //parameters[1] = sModelId;
                        //parameters[2] = sRouteId;
                        //parameters[3] = "Y";
                        ////연계 화면 확인 후 수정(Route 관리 화면이 기준정보로 이동되면서 연계가 불가능함.)
                        //this.FrameOperation.OpenMenu("SFU010705170", true, parameters);
                    }
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return;
            }
            finally
            {
                loadingIndicator.Visibility = Visibility.Collapsed;
            }
        }

        private void dgHist_LoadedColumnHeaderPresenter(object sender, C1.WPF.DataGrid.DataGridColumnEventArgs e)
        {
            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (string.IsNullOrEmpty(e.Column.Name) == false)
                {
                    if (e.Column.Name.Equals("LOTID") || e.Column.Name.Equals("CSTID"))
                    {
                        //2021.04.01 링크 기능을 Head -> Cell 로 전환 및 Bold 제거 START
                        //e.Column.HeaderPresenter.Foreground = new SolidColorBrush(Colors.Blue);
                        //e.Column.HeaderPresenter.FontWeight = FontWeights.Bold;
                        //e.Column.HeaderPresenter.Cursor = Cursors.Hand;
                        //2021.04.01 링크 기능을 Head -> Cell 로 전환 및 Bold 제거 END
                    }
                }
            }));
        }

        //2021.04.01 링크 기능을 Head -> Cell 로 전환 및 Bold 제거 START
        private void dgHist_LoadedCellPresenter(object sender, DataGridCellEventArgs e)
        {
            if (sender == null)
                return;

            C1DataGrid dg = sender as C1DataGrid;

            dg?.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (e.Cell.Presenter == null)
                {
                    return;
                }

                if (e.Cell.Row.Type == DataGridRowType.Item)
                {
                    ////////////////////////////////////////////  default 색상 및 Cursor
                    e.Cell.Presenter.Cursor = Cursors.Arrow;

                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Black);
                    e.Cell.Presenter.FontSize = 12;
                    e.Cell.Presenter.FontWeight = FontWeights.Normal;
                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Transparent);
                    ///////////////////////////////////////////////////////////////////////////////////

                    if (e.Cell.Column.Name.Equals("LOTID") || e.Cell.Column.Name.Equals("CSTID"))
                    {
                        e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Blue);
                        e.Cell.Presenter.Cursor = Cursors.Hand;
                    }
                }
            }));
        }
        //2021.04.01 링크 기능을 Head -> Cell 로 전환 및 Bold 제거 END

        private void ShowLoadingIndicator()
        {
            if (loadingIndicator != null)
                loadingIndicator.Visibility = Visibility.Visible;
        }

        private void HiddenLoadingIndicator()
        {
            if (loadingIndicator != null)
                loadingIndicator.Visibility = Visibility.Collapsed;
        }
        private string GetHoldStatus(DataSet dsRslt)
        {
            if (dsRslt.Tables["INFO1"].Columns.Contains("MES_HOLD_YN") && dsRslt.Tables["INFO1"].Columns.Contains("QMS_HOLD_YN") && dsRslt.Tables["INFO1"].Columns.Contains("SUBLOT_HOLD_YN") && dsRslt.Tables["INFO1"].Columns.Contains("PACK_HOLD_YN"))
            {
                string holdFlag = "N";
                string mesHold = Util.NVC(dsRslt.Tables["INFO1"].Rows[0]["MES_HOLD_YN"].ToString() == "Y" ? "Y" : "N");
                string qmsHold = Util.NVC(dsRslt.Tables["INFO1"].Rows[0]["QMS_HOLD_YN"].ToString() == "Y" ? "Y" : "N");
                string cellHold = Util.NVC(dsRslt.Tables["INFO1"].Rows[0]["SUBLOT_HOLD_YN"].ToString() == "Y" ? "Y" : "N");
                string palletHold = Util.NVC(dsRslt.Tables["INFO1"].Rows[0]["PACK_HOLD_YN"].ToString() == "Y" ? "Y" : "N");
                if (mesHold == "Y" || qmsHold == "Y" || cellHold == "Y" || palletHold == "Y")
                {
                    holdFlag = "Y";
                }
                return $"({holdFlag}) MES: {mesHold}, QMS: {qmsHold}, CELL: {cellHold}, Pallet: {palletHold}";
            }
            else
            {
                return null;
            }
        }
        private string GetCellCondition(DataSet dsRslt)
        {
            if (dsRslt.Tables["INFO1"].Columns.Contains("EQPT_INPUT_AVAIL_FLAG_NAME")&& dsRslt.Tables["INFO1"].Columns.Contains("CURR_LOT_DETL_TYPE_CODE"))
            {
                string inputFlag = Util.NVC(dsRslt.Tables["INFO1"].Rows[0]["EQPT_INPUT_AVAIL_FLAG_NAME"].ToString());
                string lotTypeCode = Util.NVC(dsRslt.Tables["INFO1"].Rows[0]["CURR_LOT_DETL_TYPE_CODE"].ToString());
                if(!String.IsNullOrEmpty(inputFlag))
                {
                    lotTypeCode = '-' + lotTypeCode;
                }
                return inputFlag + lotTypeCode;
            }
            else
            {
                return null;
            }
        }

    }
}
