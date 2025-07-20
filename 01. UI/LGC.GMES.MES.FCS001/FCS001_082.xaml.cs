/*************************************************************************************
 Created Date : 2020.12.21
      Creator : Kang Dong Hee
   Decription : Safety Sensor 자주검사 등록/조회
--------------------------------------------------------------------------------------
 [Change History]
  2020.12.21  NAME   : Initial Created
  2021.04.13  KDH    : 화면간 이동 시 초기화 현상 제거
  2022.06.09  이정미 : 등록Tab 조회시 오류 수정 
  2022.06.15  이정미 : 저장 오류 수정
**************************************************************************************/
using C1.WPF;
using C1.WPF.DataGrid;
using C1.WPF.DataGrid.Summaries;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using System;
using System.IO;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using LGC.GMES.MES.CMM001;
using Microsoft.Win32;
using System.Configuration;
using C1.WPF.Excel;
using System.Linq;
using System.Windows.Data;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace LGC.GMES.MES.FCS001
{
    public partial class FCS001_082 : UserControl, IWorkArea
    {
        #region [Declaration & Constructor]
        Util _Util = new Util();
        private bool bCLCT_TYPE_B = false;
        private bool bCLCT_TYPE_C = false;

        public FCS001_082()
        {
            InitializeComponent();
        }

        public IFrameOperation FrameOperation
        {
            get;
            set;
        }
        #endregion

        #region [Initialize]

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            InitCombo();
            //Control Setting
            InitControl();

            txtWorker.Text = LoginInfo.USERNAME;

            this.Loaded -= UserControl_Loaded; //2021.04.13 화면간 이동 시 초기화 현상 제거
        }
        private void InitCombo()
        {
            CommonCombo_Form ComCombo = new CommonCombo_Form();

            ComCombo.SetCombo(cboModel, CommonCombo_Form.ComboStatus.NONE, sCase: "MODEL");

            string[] sFilterEqpType = { "EQPT_GR_TYPE_CODE", "5,D" };
            C1ComboBox[] cboEqpKindChild = { cboEqp };
            ComCombo.SetCombo(cboEqpKind, CommonCombo_Form.ComboStatus.NONE, sCase: "FORM_CMN", sFilter: sFilterEqpType, cbChild: cboEqpKindChild);

            object[] objParent = { cboEqpKind, "5", "1", "L" };  //200305 이근사원 요청 : LOADER만 DISPLAY
            ComCombo.SetComboObjParent(cboEqp, CommonCombo_Form.ComboStatus.NONE, sCase: "EQPSUBSTRING", objParent: objParent);  //DEGAS/EOL 라인

            ComCombo.SetCombo(cboSelModel, CommonCombo_Form.ComboStatus.NONE, sCase: "MODEL");

            string[] sFilterEqpType2 = { "EQPT_GR_TYPE_CODE", "5,D" };
            C1ComboBox[] cboEqpKindChild2 = { cboSelEqp };
            ComCombo.SetCombo(cboSelEqpKind, CommonCombo_Form.ComboStatus.NONE, sCase: "FORM_CMN", sFilter: sFilterEqpType2, cbChild: cboEqpKindChild2);

            object[] objParent2 = { cboSelEqpKind, "5", "1", "L" };  //200305 이근사원 요청 : LOADER만 DISPLAY
            ComCombo.SetComboObjParent(cboSelEqp, CommonCombo_Form.ComboStatus.NONE, sCase: "EQPSUBSTRING", objParent: objParent2);  //DEGAS/EOL 라인
        }
        private void InitControl()
        {
            dtpWorkDate.SelectedDateTime = DateTime.Now;
            dtpFromDate.SelectedDateTime = DateTime.Now.AddDays(-7);
            dtpToDate.SelectedDateTime = DateTime.Now;
        }
        #endregion

        #region [Method]
        private void GetList()
        {
            try
            {
                bCLCT_TYPE_B = false;
                bCLCT_TYPE_C = false;

                DataTable dtRqst = new DataTable();
                dtRqst.TableName = "RQSTDT";
                dtRqst.Columns.Add("LANGID", typeof(string));
                dtRqst.Columns.Add("PROC_GR_CODE", typeof(string));
                dtRqst.Columns.Add("EQPTID", typeof(string));
                dtRqst.Columns.Add("MDLLOT_ID", typeof(string));
                dtRqst.Columns.Add("WRK_DTTM", typeof(string));
                dtRqst.Columns.Add("SELF_INSP_TYPE_CODE", typeof(string));

                DataRow dr = dtRqst.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["PROC_GR_CODE"] = Util.GetCondition(cboEqpKind);
                dr["EQPTID"] = Util.GetCondition(cboEqp);
                dr["MDLLOT_ID"] = Util.GetCondition(cboModel);
                dr["WRK_DTTM"] = Util.GetCondition(dtpWorkDate);
                dr["SELF_INSP_TYPE_CODE"] = "S";
                dtRqst.Rows.Add(dr);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_SEL_QA_INPUT_SAFETY", "RQSTDT", "RSLTDT", dtRqst);

                if (dtRslt.Rows.Count == 0)
                {
                    Util.MessageValidation("FM_ME_0095");  //검사기준 정보가 없습니다.
                    Util.gridClear(dgSSSfInsBas);
                    Util.gridClear(dgSSSfInsBasInput);
                    return;
                }
                Util.GridSetData(dgSSSfInsBasInput, dtRslt, FrameOperation, true);

                GetListDay();
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void GetListDay()
        {
            try
            {
                DataTable dtRqst = new DataTable();
                dtRqst.TableName = "RQSTDT";
                dtRqst.Columns.Add("LANGID", typeof(string));
                dtRqst.Columns.Add("PROC_GR_CODE", typeof(string));
                dtRqst.Columns.Add("EQPTID", typeof(string));
                dtRqst.Columns.Add("MDLLOT_ID", typeof(string));
                dtRqst.Columns.Add("WRK_DTTM", typeof(string));
                dtRqst.Columns.Add("SELF_INSP_TYPE_CODE", typeof(string));

                DataRow dr = dtRqst.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["PROC_GR_CODE"] = Util.GetCondition(cboEqpKind);
                dr["EQPTID"] = Util.GetCondition(cboEqp);
                dr["MDLLOT_ID"] = Util.GetCondition(cboModel);
                dr["WRK_DTTM"] = Util.GetCondition(dtpWorkDate);
                dr["SELF_INSP_TYPE_CODE"] = "S";
                dtRqst.Rows.Add(dr);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_SEL_QA_INPUT_SAFETY", "RQSTDT", "RSLTDT", dtRqst);

                if (dtRslt.Rows.Count == 0)
                {
                    Util.MessageValidation("FM_ME_0095");  //검사기준 정보가 없습니다.
                    Util.gridClear(dgSSSfInsBas);
                    Util.gridClear(dgSSSfInsBasInput);
                    return;
                }
                Util.GridSetData(dgSSSfInsBas, dtRslt, FrameOperation, true);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void GetListPeriod()
        {
            try
            {
                DataTable dtRqst = new DataTable();
                dtRqst.TableName = "RQSTDT";
                dtRqst.Columns.Add("LANGID", typeof(string));
                dtRqst.Columns.Add("PROC_GR_CODE", typeof(string));
                dtRqst.Columns.Add("EQPTID", typeof(string));
                dtRqst.Columns.Add("MDLLOT_ID", typeof(string));
                dtRqst.Columns.Add("WRK_DTTM", typeof(string));
                dtRqst.Columns.Add("SELF_INSP_TYPE_CODE", typeof(string));
                dtRqst.Columns.Add("FROM_DATE", typeof(string));
                dtRqst.Columns.Add("TO_DATE", typeof(string));

                DataRow dr = dtRqst.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["PROC_GR_CODE"] = Util.GetCondition(cboSelEqpKind);
                dr["EQPTID"] = Util.GetCondition(cboSelEqp);
                dr["MDLLOT_ID"] = Util.GetCondition(cboSelModel);
                dr["WRK_DTTM"] = "";
                dr["SELF_INSP_TYPE_CODE"] = "S";
                dr["FROM_DATE"] = Util.GetCondition(dtpFromDate);
                dr["TO_DATE"] = Util.GetCondition(dtpToDate);
                dtRqst.Rows.Add(dr);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_SEL_QA_INPUT_SAFETY", "RQSTDT", "RSLTDT", dtRqst);
                Util.GridSetData(dgInspList, dtRslt, FrameOperation, true);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        #endregion

        #region [Event]
        #region [등록 Tab]
        private void btnSearch_Click(object sender, EventArgs e)
        {
            GetList();
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            try
            {
                if (string.IsNullOrEmpty(txtWorker.Text))
                {
                    Util.MessageValidation("FM_ME_0200");  //작업자를 입력해주세요.
                    return;
                }
               // string sWorkDate = Util.GetCondition(dtpWorkDate);
                string sEqpId = Util.GetCondition(cboEqp);

                DataTable dtRqst = new DataTable();
                dtRqst.Columns.Add("SRCTYPE", typeof(string));
                dtRqst.Columns.Add("IFMODE", typeof(string));
                dtRqst.Columns.Add("USERID", typeof(string));
                dtRqst.Columns.Add("WRK_DTTM", typeof(DateTime));
                dtRqst.Columns.Add("EQPTID", typeof(string));
                dtRqst.Columns.Add("CLCTITEM", typeof(string));
                dtRqst.Columns.Add("WRKR_NAME", typeof(string));
                dtRqst.Columns.Add("CLCT_MEASR_PONT_CODE", typeof(string));
                dtRqst.Columns.Add("CLCT_VALUE", typeof(string));
                dtRqst.Columns.Add("ACTION_SEQNO", typeof(string));
                dtRqst.Columns.Add("PROD_LOTID", typeof(string));
                dtRqst.Columns.Add("CLCT_CNTT", typeof(string));
                dtRqst.Columns.Add("SUBLOTID", typeof(string));
                dtRqst.Columns.Add("SELF_INSP_TYPE_CODE", typeof(string));


                for (int i = 0; i < dgSSSfInsBasInput.Rows.Count; i++)
                {
                    for (int j = dgSSSfInsBasInput.Columns["CO"].Index; j <= dgSSSfInsBasInput.Columns["AN2"].Index; j++)
                    {
                        if (dgSSSfInsBasInput.GetCell(i, j).Presenter.Background.ToString().Equals(Colors.White.ToString()))
                        {
                            string sValue = Util.NVC(DataTableConverter.GetValue(dgSSSfInsBasInput.Rows[i].DataItem, dgSSSfInsBasInput.Columns[j].Name));
                            //string curCell = Util.NVC(DataTableConverter.GetValue(dgCell.Rows[i].DataItem, dg       Cell.Columns[j].Name));

                            //sValue 수정해야함!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!! 
                            string sLotID = Util.NVC(DataTableConverter.GetValue(dgSSSfInsBasInput.Rows[i].DataItem, "PROD_LOTID"));

                            if (string.IsNullOrEmpty(sValue) || string.IsNullOrEmpty(sLotID))
                            {
                                Util.MessageValidation("FM_ME_0256");  //항목에 빈값이 있습니다.
                                return;
                            }

                            DataRow dr = dtRqst.NewRow();
                            dr["SRCTYPE"] = "UI";
                            dr["IFMODE"] = "ON";
                            dr["USERID"] = LoginInfo.USERID;
                            dr["WRK_DTTM"] = dtpWorkDate.SelectedDateTime.ToString("yyyy-MM-dd");//sWorkDate
                            dr["EQPTID"] = sEqpId;
                            dr["CLCTITEM"] = Util.NVC(DataTableConverter.GetValue(dgSSSfInsBasInput.Rows[i].DataItem, "CLCTITEM"));
                            dr["WRKR_NAME"] = txtWorker.Text;
                            dr["CLCT_MEASR_PONT_CODE"] = dgSSSfInsBasInput.Columns[j].Name;
                            dr["CLCT_VALUE"] = Util.NVC(DataTableConverter.GetValue(dgSSSfInsBasInput.Rows[i].DataItem, dgSSSfInsBasInput.Columns[j].Name));
                            dr["ACTION_SEQNO"] = "1"; //확인 필요 
                            dr["PROD_LOTID"] = Util.NVC(DataTableConverter.GetValue(dgSSSfInsBasInput.Rows[i].DataItem, "PROD_LOTID"));
                            dr["CLCT_CNTT"] = Util.NVC(DataTableConverter.GetValue(dgSSSfInsBasInput.Rows[i].DataItem, "CLCT_CNTT"));
                            //dr["CLCT_CNTT"] = Util.NVC(DataTableConverter.GetValue(dgSSSfInsBasInput.Rows[i].DataItem, "CLCT_DESC"));
                            dr["SUBLOTID"] = "";
                            dr["SELF_INSP_TYPE_CODE"] = "S";
                            dtRqst.Rows.Add(dr);
                        }
                    }
                }

                //저장하시겠습니까?

                

                Util.MessageConfirm("FM_ME_0214", (result) =>
                {
                    if (result == MessageBoxResult.OK)
                    {
                        //DataTable dtRslt = new ClientProxy().ExecuteServiceSync("BR_SET_WORKINSP_DATA", "RQSTDT", null, dtRqst); // BR 개발필요
                        DataTable dtRslt = new ClientProxy().ExecuteServiceSync("BR_SET_WORKINSP_DATA", "INDATA", "OUTDATA", dtRqst);

                        if (dtRslt.Rows[0]["RETVAL"].ToString().Equals("0")) //0성공
                        {
                            Util.MessageValidation("FM_ME_0215");  //저장하였습니다.
                        }
                        else
                        {
                            Util.MessageValidation("FM_ME_0213");  //저장 실패하였습니다.
                        }

                        GetList();
                    }
                });
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void dgSSSfInsBasInput_LoadedCellPresenter(object sender, DataGridCellEventArgs e)
        {
            if (sender == null)
                return;


            //C1DataGrid dg = sender as C1DataGrid;
            C1.WPF.DataGrid.C1DataGrid dg = sender as C1.WPF.DataGrid.C1DataGrid;
            dg?.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (e.Cell.Presenter == null)
                {
                    return;
                }

                if (e.Cell.Row.Type == DataGridRowType.Item)
                {
                     int iFCol = dgSSSfInsBasInput.Columns["CO"].Index;  //공통 
                     int iACol = dgSSSfInsBasInput.Columns["CA"].Index;  // UNIT_1-1
                     int iTCol = dgSSSfInsBasInput.Columns["AN2"].Index; // UNIT_2-2                                                    
                     string sClctType = Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "CLCT_MTHD_TYPE_CODE"));

                     ////////////////////////////////////////////  default 색상 및 Cursor
                     e.Cell.Presenter.Cursor = Cursors.Arrow;

                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Black);
                    e.Cell.Presenter.FontSize = 12;
                    e.Cell.Presenter.FontWeight = FontWeights.Normal;
                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Transparent);
                     ///////////////////////////////////////////////////////////////////////////////////
   
                     if (e.Cell.Column.Index >= iFCol && e.Cell.Column.Index <= iTCol)
                    {
                        e.Cell.Presenter.Background = new SolidColorBrush(Colors.Gray);
                    }

                    if (sClctType.Equals("5B"))
                    {
                        bCLCT_TYPE_B = true;
                        if (e.Cell.Column.Name.Equals("CO"))
                        {
                            e.Cell.Presenter.Background = new SolidColorBrush(Colors.White);
                            e.Cell.Presenter.IsEnabled = true;
                        }
                    }

                    if (sClctType.Equals("5C"))
                    {
                        bCLCT_TYPE_C = true;
                        if (e.Cell.Column.Index >= iACol && e.Cell.Column.Index <= iTCol)
                        {
                            e.Cell.Presenter.Background = new SolidColorBrush(Colors.White);
                            e.Cell.Presenter.IsEnabled = true;
                        }
                    }
                }
            }));
        }

        private void dgSSSfInsBasInput_LoadedColumnHeaderPresenter(object sender, C1.WPF.DataGrid.DataGridColumnEventArgs e)
        {
            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                int iFCol = dgSSSfInsBasInput.Columns["CO"].Index; //공통 
                int iACol = dgSSSfInsBasInput.Columns["CA"].Index;  // UNIT_1-1
                int iTCol = dgSSSfInsBasInput.Columns["AN2"].Index;// UNIT_2 - 2

                if (string.IsNullOrEmpty(e.Column.Name) == false)
                {
                    if (e.Column.Name.Equals("PROD_LOTID") || e.Column.Name.Equals("CLCT_CNTT"))
                    {
                        e.Column.HeaderPresenter.Foreground = new SolidColorBrush(Colors.Red);
                        e.Column.HeaderPresenter.FontWeight = FontWeights.Bold;
                        e.Column.HeaderPresenter.Cursor = Cursors.Hand;
                    }

                    if(e.Column.Index >= iACol && e.Column.Index <= iTCol)
                    {
                        e.Column.HeaderPresenter.Foreground = new SolidColorBrush(Colors.Red);
                        e.Column.HeaderPresenter.FontWeight = FontWeights.Bold;
                        e.Column.HeaderPresenter.Cursor = Cursors.Hand;
                    }

                    /*if (bCLCT_TYPE_B.Equals(true))
                    {
                        if (e.Column.Name.Equals("CO"))
                        {
                            e.Column.HeaderPresenter.Foreground = new SolidColorBrush(Colors.Red);
                        }
                    }

                    if (bCLCT_TYPE_C.Equals(true))
                    {
                        if (e.Column.Index >= iFCol && e.Column.Index <= iTCol)
                        {
                            e.Column.HeaderPresenter.Foreground = new SolidColorBrush(Colors.Red);
                        }
                    }*/
                }
            }));
        }

        #endregion

        #region [조회 Tab]
        private void cboSelSearch_Click(object sender, RoutedEventArgs e)
        {
            GetListPeriod();
        }
        #endregion

        #endregion

    }
}


