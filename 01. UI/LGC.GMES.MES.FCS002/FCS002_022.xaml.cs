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
**************************************************************************************/
using C1.WPF;
using C1.WPF.DataGrid;
using C1.WPF.DataGrid.Summaries;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using System;
using System.Collections.Generic;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using LGC.GMES.MES.CMM001;
using LGC.GMES.MES.CMM001.Extensions;

namespace LGC.GMES.MES.FCS002
{
    /// <summary>
    /// FCS002_022.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class FCS002_022 : UserControl, IWorkArea
    {
        #region Declaration & Constructor 
        private string _CellId;

        private string _sActYN = "N";
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
        public FCS002_022()
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
                txtInputID.IsReadOnly = false;  //20210408 Cell ID 입력 가능하도록 수정
                txtInputID.Text = _CellId;

                btnSearch_Click(null, null);
            }
            else
            {
                txtInputID.IsReadOnly = false;
                txtInputID.Text = string.Empty;
            }

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

                string sSubLotID = string.Empty;
                if (!string.IsNullOrEmpty(txtInputID.Text))
                    sSubLotID = Util.Convert_CellID(Util.GetCondition(txtInputID, sMsg: "FM_ME_0019"));
                else
                    Util.MessageInfo("FM_ME_0019");

                DataRow dr = dtRqst.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["SUBLOTID"] = sSubLotID;  //Cell ID를 입력해주세요.
                if (string.IsNullOrEmpty(dr["SUBLOTID"].ToString()))
                {
                    return;
                }
                dtRqst.Rows.Add(dr);

                DataSet dsRqst = new DataSet();
                dsRqst.Tables.Add(dtRqst);

                ShowLoadingIndicator();
                DataSet dsRslt = new ClientProxy().ExecuteServiceSync_Multi("BR_GET_PROCESS_RETRIEVE_INFO_MB", "RQSTDT", "INFO1,INFO2", dsRqst);

                if (dsRslt.Tables["INFO1"].Rows.Count == 0) { ClearControl(); }


                if (dsRslt.Tables["INFO1"].Rows.Count > 0)
                {
                    txtCellID.Text = Util.NVC(dsRslt.Tables["INFO1"].Rows[0]["SUBLOTID"].ToString());
                    txtCanID.Text = Util.NVC(dsRslt.Tables["INFO1"].Rows[0]["CANID"].ToString());
                    txtVentID.Text = Util.NVC(dsRslt.Tables["INFO1"].Rows[0]["VENTID"].ToString());
                    txtCellNo.Text = Util.NVC(dsRslt.Tables["INFO1"].Rows[0]["CSTSLOT"].ToString());
                    txtRouteID.Text = Util.NVC(dsRslt.Tables["INFO1"].Rows[0]["ROUTID"].ToString());
                    txtTrayNo.Text = Util.NVC(dsRslt.Tables["INFO1"].Rows[0]["LOTID"].ToString());

                    txtLotID.Text = Util.NVC(dsRslt.Tables["INFO1"].Rows[0]["PROD_LOTID"].ToString());
                    txtOper.Text = Util.NVC(dsRslt.Tables["INFO1"].Rows[0]["PROCID"].ToString());
                    txtCreateTime.Text = Util.NVC(dsRslt.Tables["INFO1"].Rows[0]["LOTDTTM_CR"].ToString());
                    txtTrayID.Text = Util.NVC(dsRslt.Tables["INFO1"].Rows[0]["CSTID"].ToString());
                    txtFilling.Text = Util.NVC(dsRslt.Tables["INFO1"].Rows[0]["ELCTRLT_EL_VALUE"].ToString());
                    textkval.Text = Util.NVC(dsRslt.Tables["INFO1"].Rows[0]["K_VAL"].ToString());

                    // 2023.11.14 CELL POSITON를 Bizrule에서 가져옮
                    //if (dsRslt.Tables["INFO1"].Rows[0]["LINE_GROUP_CODE"].ToString() == "MCC")
                    //{
                    //    int cellNo = Convert.ToInt32(txtCellNo.Text);
                    //    int i = 65 + cellNo / 16;
                    //    if (cellNo % 16 == 0)
                    //    {
                    //        cellNo = 16;
                    //        i--;
                    //    }
                    //    else
                    //    {
                    //        cellNo = cellNo % 16;
                    //    }
                    //    char unicode = (char)i;
                    //    txtPsition.Text = String.Format("{0}{1:0#}", unicode, cellNo);
                    //}
                    txtPosition.Text = dsRslt.Tables["INFO1"].Rows[0]["CELL_POS"].ToString();
                }

                if (dsRslt.Tables["INFO2"].Rows.Count > 0)
                {
                    Util.GridSetData(dgHist, dsRslt.Tables["INFO2"], FrameOperation, true);
                    //fpsHist.SetColAutoFit();
                }
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

                string sSubLotID = Util.Convert_CellID(Util.GetCondition(txtCellID));


                DataRow dr = dtRqst.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["AREAID"] = LoginInfo.CFG_AREA_ID; //2022.02.22_AREA 조건 추가
                dr["SUBLOTID"] = sSubLotID;
                if (string.IsNullOrEmpty(dr["SUBLOTID"].ToString())) return;
                dtRqst.Rows.Add(dr);

                ShowLoadingIndicator();
                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_SEL_PROCESS_GRADER_MB", "RQSTDT", "RSLTDT", dtRqst);

                Util.GridSetData(dgCell, dtRslt, FrameOperation, true);
                //fpsCell.SetColAutoFit();
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

        private void ClearControl()
        {
            txtCellID.Text = string.Empty;
            txtCanID.Text = string.Empty;
            txtVentID.Text = string.Empty;
            txtCellNo.Text = string.Empty;
            txtRouteID.Text = string.Empty;
            txtTrayNo.Text = string.Empty;
            txtLotID.Text = string.Empty;
            txtOper.Text = string.Empty;
            txtCreateTime.Text = string.Empty;
            txtTrayID.Text = string.Empty;
            txtPosition.Text = string.Empty;
            Util.gridClear(dgCell);
            Util.gridClear(dgHist);
        }
        #endregion

        #region [Event]
        private void btnSearch_Click(object sender, EventArgs e)
        {

            if ( string.IsNullOrEmpty(txtInputID.Text))
            {
                ClearControl();
                return;
            }

            GetList();
            GetProcessGradeInfo();

            if (!string.IsNullOrEmpty(txtInputID.Text))
            {
                txtInputID.SelectAll();
                txtInputID.Focus();
            }

        }

        private void txtCellID_KeyDown(object sender, KeyEventArgs e)
        {
            if ((!string.IsNullOrEmpty(txtInputID.Text)) && (e.Key == Key.Enter))
            {
                btnSearch_Click(null, null);
            }
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

                    if (!cell.Column.Name.Equals("CSTID") && !cell.Column.Name.Equals("LOTID") && !cell.Column.Name.Equals("ROUTID"))
                    {
                        return;
                    }

                    if (cell.Column.Name.Equals("CSTID") || cell.Column.Name.Equals("LOTID"))
                    {
                        string sTrayId =  Util.NVC(DataTableConverter.GetValue(dgHist.Rows[cell.Row.Index].DataItem, "CSTID")).ToString();
                        string sTrayNo = Util.NVC(DataTableConverter.GetValue(dgHist.Rows[cell.Row.Index].DataItem, "LOTID")).ToString();

                        // 프로그램 ID 확인 후 수정
                        object[] parameters = new object[6];
                        parameters[0] = sTrayId;
                        parameters[1] = sTrayNo;
                        parameters[2] = string.Empty;
                        parameters[3] = string.Empty;
                        parameters[4] = string.Empty;
                        parameters[5] = "Y";
                        this.FrameOperation.OpenMenu("SFU010710300", true, parameters); //Tray 정보조회(FORM - 소형)
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

       
    }
}
