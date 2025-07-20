/*************************************************************************************
 Created Date : 2017.12.04
      Creator : 오화백K
   Decription : 활성화 대차 재구성
--------------------------------------------------------------------------------------
 [Change History]
  2017.12.04  오화백K : Initial Created.
  2018.03.09  오화백K : 불량대차 재구성 추가
**************************************************************************************/

using C1.WPF;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.CMM001.Popup;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using C1.WPF.DataGrid;
using LGC.GMES.MES.CMM001;
using DataGridRow = C1.WPF.DataGrid.DataGridRow;



using LGC.GMES.MES.CMM001.Extensions;

using Application = System.Windows.Application;

namespace LGC.GMES.MES.COM001
{

    public partial class COM001_203 : UserControl, IWorkArea
    {
        #region Declaration & Constructor 

        private Util _Util = new Util();
        private string _MOVE_CASTID = string.Empty;
        private string _TAGET_CASTID = string.Empty;
        private string _PROCID = string.Empty;
        private string _AREAID = string.Empty;
        private string _SPLIT_LOTID_RT = string.Empty;
        private string _WIP_QLTY_TYPE_CODE = string.Empty;
        private string _PRODID = string.Empty;
        private string _MKT_TYPE_CODE = string.Empty;



        private int _tagPrintCount;
        private readonly BizDataSet _bizDataSet = new BizDataSet();

      
        //이동대차
        private DataTable MOVE_CART_LOTID_RT = null;
        private DataTable MOVE_CART_INBOX = null;
        private DataTable MOVE_CART_BIND_INBOX = null;
        //생성대차
        private DataTable TAGET_CART_LOTID_RT = null;
        private DataTable TAGET_CART_INBOX = null;
        private DataTable TAGET_CART_BIND_INBOX = null;

        public COM001_203()
        {
            InitializeComponent();
            InitCombo();

            this.Loaded += UserControl_Loaded;
        }

        C1.WPF.DataGrid.DataGridRowHeaderPresenter pre = new C1.WPF.DataGrid.DataGridRowHeaderPresenter()
        {
            Background = new SolidColorBrush(Colors.Transparent),
            MouseOverBrush = new SolidColorBrush(Colors.Transparent),
        };
        C1.WPF.DataGrid.DataGridRowHeaderPresenter pre_S = new C1.WPF.DataGrid.DataGridRowHeaderPresenter()
        {
            Background = new SolidColorBrush(Colors.Transparent),
            MouseOverBrush = new SolidColorBrush(Colors.Transparent),
        };

        //전체선택
        CheckBox chkAll = new CheckBox()
        {
            IsChecked = false,
            Background = new SolidColorBrush(Colors.Transparent),
            VerticalAlignment = System.Windows.VerticalAlignment.Center,
            HorizontalAlignment = System.Windows.HorizontalAlignment.Center
        };
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
            #region 대차재구성

            //동
            CommonCombo _combo = new CommonCombo();
            _combo.SetCombo(cboArea, CommonCombo.ComboStatus.SELECT, sCase: "AREA");


            #endregion

            #region 재구성이력조회
            
            //동
            C1ComboBox[] cboAreaChild = { cboEquipmentSegment };
            _combo.SetCombo(cboAreaHistory, CommonCombo.ComboStatus.SELECT, cbChild: cboAreaChild, sCase: "AREA");

            //라인
            C1ComboBox[] cboEquipmentSegmentParent = { cboAreaHistory };
            C1ComboBox[] cboEquipmentSegmentChild = { cboProcessHistory};
            _combo.SetCombo(cboEquipmentSegment, CommonCombo.ComboStatus.ALL, sCase: "EQUIPMENTSEGMENT_FORM", cbChild: cboEquipmentSegmentChild, cbParent: cboEquipmentSegmentParent);

            //공정
            C1ComboBox[] cboProcessParent = { cboEquipmentSegment };
            String[] sFilterProcess = { "SEARCH", string.Empty, string.Empty, string.Empty };
            _combo.SetCombo(cboProcessHistory, CommonCombo.ComboStatus.ALL, sFilter: sFilterProcess, cbParent: cboProcessParent, sCase: "PROCESS_SORT");

            //특수
            String[] sFilter1 = { "", "WIP_PRCS_TYPE_CODE" };
            _combo.SetCombo(cboOrder, CommonCombo.ComboStatus.SELECT, sFilter: sFilter1, sCase: "COMMCODES_PRDT_REQ_PRCS");

            #endregion

        }

        #endregion

        #region Event
        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
           
            ldpDateFromHist_Detail.SelectedDateTime = (DateTime)System.DateTime.Now.AddDays(1- DateTime.Now.Day);
            ldpDateToHist_Detail.SelectedDateTime = (DateTime)System.DateTime.Now;

            //사용자 권한별로 버튼 숨기기
            List<Button> listAuth = new List<Button>();
            listAuth.Add(btnComplate);
            Util.pageAuth(listAuth, FrameOperation.AUTHORITY);
           

            this.Loaded -= UserControl_Loaded;

        }

        #region 대차재구성

        private void btnReSet_Click(object sender, RoutedEventArgs e)
        {
            Clear();
        }

        //이동대차 CTNR_ID 
        private void txtMove_KeyDown(object sender, KeyEventArgs e)
        {
            try
            {

                if (e.Key == Key.Enter)
                {
                    if (txtMove.Text == string.Empty) return;
                    //Cart 정보
                    GetCartInfo(txtMove.Text.Trim(), "T");
                    if (txtMove.Text != string.Empty)
                    {
                        //조립LOT 정보
                        GetLotId_RT_Info(txtMove.Text.Trim(), "T");
                        //INBOX 정보
                        Inbox_Info(txtMove.Text.Trim(), "T");
                        _MOVE_CASTID = txtMove.Text;
                        _PROCID = Util.NVC(DataTableConverter.GetValue(dgCurrentList.Rows[0].DataItem, "PROCID"));
                        _AREAID = Util.NVC(DataTableConverter.GetValue(dgCurrentList.Rows[0].DataItem, "AREAID"));
                        Util.gridClear(dgInbox);
                        //양품/불량에 따른 스프레드 컬럼 변경
                        DefectColumChange(Util.NVC(DataTableConverter.GetValue(dgCurrentList.Rows[0].DataItem, "WIP_QLTY_TYPE_CODE")));

                        _WIP_QLTY_TYPE_CODE = Util.NVC(DataTableConverter.GetValue(dgCurrentList.Rows[0].DataItem, "WIP_QLTY_TYPE_CODE"));
                        _PRODID    = Util.NVC(DataTableConverter.GetValue(dgCurrentList.Rows[0].DataItem, "PRODID"));
                        _MKT_TYPE_CODE = Util.NVC(DataTableConverter.GetValue(dgCurrentList.Rows[0].DataItem, "MKT_TYPE_CODE"));


                    }
                    txtMove.Text = string.Empty;
                    txtMove.Focus();
                }
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }

        //이동대차 조립LOT 이벤트
        private void dgLotID_RT_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (dgLotID_RT.GetRowCount() == 0)
            {
                return;
            }

            Point pnt = e.GetPosition(null);
            C1.WPF.DataGrid.DataGridCell cell = dgLotID_RT.GetCellFromPoint(pnt);

            if (cell == null || cell.Value == null)
            {
                return;
            }
            if (cell.Column.Name != "CHK")
            {
                return;
            }
            //선택 조립LOT 의 INBOX 정보 BIND
            CHK_Inbox_Bind();
            
        }
        //이동대차 INBOX 정보 전체 선택
        private void dgInbox_LoadedColumnHeaderPresenter(object sender, C1.WPF.DataGrid.DataGridColumnEventArgs e)
        {
            //전체 선택 
            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (string.IsNullOrEmpty(e.Column.Name) == false)
                {
                    if (e.Column.Name.Equals("CHK"))
                    {
                        pre.Content = chkAll;
                        e.Column.HeaderPresenter.Content = pre;
                        chkAll.Checked -= new RoutedEventHandler(checkAll_Checked);
                        chkAll.Unchecked -= new RoutedEventHandler(checkAll_Unchecked);
                        chkAll.Checked += new RoutedEventHandler(checkAll_Checked);
                        chkAll.Unchecked += new RoutedEventHandler(checkAll_Unchecked);
                    }
                }
            }));
        }
        private void checkAll_Checked(object sender, RoutedEventArgs e)
        {
            if (dgInbox.ItemsSource == null) return;

            DataTable dt = ((DataView)dgInbox.ItemsSource).Table;

            dt.Select("CHK = 0").ToList<DataRow>().ForEach(r => r["CHK"] = 1);
            dt.AcceptChanges();

        }
        private void checkAll_Unchecked(object sender, RoutedEventArgs e)
        {
            if (dgInbox.ItemsSource == null) return;

            DataTable dt = ((DataView)dgInbox.ItemsSource).Table;

            dt.Select("CHK = 1").ToList<DataRow>().ForEach(r => r["CHK"] = 0);
            dt.AcceptChanges();

        }
        //이동대차 대차정보 품질유형 색변경
        private void dgCurrentList_LoadedCellPresenter(object sender, DataGridCellEventArgs e)
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
                    if (DataTableConverter.GetValue(e.Cell.Row.DataItem, "WIP_QLTY_TYPE_CODE").ToString() == "G")
                    {
                        if (e.Cell.Column.Name.Equals("WIP_QLTY_TYPE_NAME"))
                        {
                            e.Cell.Presenter.Background = new SolidColorBrush(Colors.Green);
                            e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                        }
                    }
                    else
                    {
                        if (e.Cell.Column.Name.Equals("WIP_QLTY_TYPE_NAME"))
                        {
                            e.Cell.Presenter.Background = new SolidColorBrush(Colors.Red);
                            e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                        }
                    }

                }
            }));
        }
        //생성대차 CTNR_ID
        private void txtTaget_KeyDown(object sender, KeyEventArgs e)
        {
            try
            {

                if (!Create_Cart_Validation())
                {
                    return;
                }


                if (e.Key == Key.Enter)
                {
                    //Cart 정보
                    GetCartInfo(txtTaget.Text.Trim(),"C");

                    if (txtTaget.Text != string.Empty)
                    {

                        if (DataTableConverter.GetValue(dgCurrentList.Rows[0].DataItem, "PROCID").ToString() != DataTableConverter.GetValue(dgCurrentList_Taget.Rows[0].DataItem, "PROCID").ToString())
                        {
                            Util.MessageValidation("SFU4386"); //재구성할 대차의 공정 정보가 틀립니다.
                            NewCheckClear();
                            return;
                        }
                    
                        else if (_WIP_QLTY_TYPE_CODE != DataTableConverter.GetValue(dgCurrentList_Taget.Rows[0].DataItem, "WIP_QLTY_TYPE_CODE").ToString())
                        {
                            Util.MessageValidation("SFU4597"); //품질유형 정보가 틀립니다.
                            NewCheckClear();
                            return;
                        }

                        else if (_PRODID != DataTableConverter.GetValue(dgCurrentList_Taget.Rows[0].DataItem, "PRODID").ToString())
                        {
                            Util.MessageValidation("SFU4611"); //제품ID가 틀린 대차 입니다.
                            NewCheckClear();
                            return;
                        }

                        else if (_MKT_TYPE_CODE != DataTableConverter.GetValue(dgCurrentList_Taget.Rows[0].DataItem, "MKT_TYPE_CODE").ToString())
                        {
                            Util.MessageValidation("SFU4612"); //시장유형정보가 틀린 대차 입니다.
                            NewCheckClear();
                            return;
                        }


                        else
                        {

                            //조립LOT 정보
                            GetLotId_RT_Info(txtTaget.Text.Trim(), "C");
                            //INBOX 정보
                            Inbox_Info(txtTaget.Text.Trim(), "C");
                            _TAGET_CASTID = txtTaget.Text;

                            txtTaget.Text = string.Empty;
                            txtTaget.Focus();
                        }
                    }
                 
                }
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }
        //생성대차 조립LOT 이벤트
        private void dgLotID_RT_Taget_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (dgLotID_RT_Taget.GetRowCount() == 0)
            {
                return;
            }

            Point pnt = e.GetPosition(null);
            C1.WPF.DataGrid.DataGridCell cell = dgLotID_RT_Taget.GetCellFromPoint(pnt);

            if (cell == null || cell.Value == null)
            {
                return;
            }
            if (cell.Column.Name != "CHK")
            {
                return;
            }
         

            TAGET_CART_BIND_INBOX = new DataTable();
            TAGET_CART_BIND_INBOX = TAGET_CART_INBOX.Clone();

            for (int i = 0; i < dgLotID_RT_Taget.Rows.Count; i++)
            {
                if (DataTableConverter.GetValue(dgLotID_RT_Taget.Rows[i].DataItem, "CHK").ToString() == "1")
                {
                    DataRow[] drInbox = TAGET_CART_INBOX.Select("LOTID_RT ='" + DataTableConverter.GetValue(dgLotID_RT_Taget.Rows[i].DataItem, "LOTID_RT").ToString() + "'");
                    foreach (DataRow dr in drInbox)
                    {
                        DataRow drBindInBox = TAGET_CART_BIND_INBOX.NewRow();
                        drBindInBox["CHK"] = 1;
                        drBindInBox["INBOX_ID"] = dr["INBOX_ID"];
                        drBindInBox["INBOX_ID_DEF"] = dr["INBOX_ID_DEF"];
                        drBindInBox["INBOX_TYPE_NAME"] = dr["INBOX_TYPE_NAME"];
                        drBindInBox["DFCT_RSN_GR_NAME"] = dr["DFCT_RSN_GR_NAME"];
                        drBindInBox["CAPA_GRD_CODE"] = dr["CAPA_GRD_CODE"];
                        drBindInBox["WIPQTY"] = dr["WIPQTY"];
                        drBindInBox["INPUT_DATA"] = dr["INPUT_DATA"];
                        drBindInBox["INBOX_QTY"] = dr["INBOX_QTY"];
                        drBindInBox["INBOX_QTY_DEF"] = dr["INBOX_QTY_DEF"];
                        drBindInBox["CTNR_ID"] = dr["CTNR_ID"];
                        drBindInBox["LOTID_RT"] = dr["LOTID_RT"];
                        drBindInBox["PRJT_NAME"] = dr["PRJT_NAME"];
                        drBindInBox["PRODID"] = dr["PRODID"];
                        drBindInBox["INBOX_TYPE_CODE"] = dr["INBOX_TYPE_CODE"];
                        drBindInBox["DFCT_RSN_GR_ID"] = dr["DFCT_RSN_GR_ID"];
                        drBindInBox["FORM_WRK_TYPE_CODE"] = dr["FORM_WRK_TYPE_CODE"];
                        drBindInBox["FORM_WRK_TYPE_NAME"] = dr["FORM_WRK_TYPE_NAME"];
                        drBindInBox["AOMM_GRD_CODE"] = dr["AOMM_GRD_CODE"];
                        TAGET_CART_BIND_INBOX.Rows.Add(drBindInBox);
                    }

                }
            }
            if (TAGET_CART_BIND_INBOX.Rows.Count == 0)
            {
                Util.gridClear(dgInbox_Taget);
            }
            else
            {
                Util.GridSetData(dgInbox_Taget, TAGET_CART_BIND_INBOX, FrameOperation, false);
            }
           
        }
        //대차이동 버튼 이벤트
        private void btnRight_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!Move_Cart_Validation())
                {
                    return;
                }
                if (dgInbox.ItemsSource == null) return;
                if (dgInbox.GetRowCount() == 0) return;

                DataTable dtdgInbox = DataTableConverter.Convert(dgInbox.ItemsSource);


                if (chkNew.IsChecked != true)
                {
                    if (dgInbox_Taget.ItemsSource == null || dgInbox_Taget.Rows.Count == 0)
                    {
                        TAGET_CART_BIND_INBOX = new DataTable();
                        TAGET_CART_BIND_INBOX = TAGET_CART_INBOX.Clone();
                    }
                }
                else //신규 CART 일경우
                {
                    if (TAGET_CART_INBOX == null)
                    {
                        TAGET_CART_INBOX = new DataTable();
                        TAGET_CART_INBOX = MOVE_CART_INBOX.Clone();
                    }
                    if (dgInbox_Taget.ItemsSource == null || dgInbox_Taget.Rows.Count == 0)
                    {
                        TAGET_CART_BIND_INBOX = new DataTable();
                        TAGET_CART_BIND_INBOX = TAGET_CART_INBOX.Clone();
                    }
                }

                //타겟 Inbox 전체 Table에 추가
                DataRow newRow = null;
                for (int i = 0; i < dtdgInbox.Rows.Count; i++)
                {
                    if (dtdgInbox.Rows[i]["CHK"].ToString() == "1")
                    {
                        newRow = TAGET_CART_INBOX.NewRow();
                        newRow["LOTID_RT"] = dtdgInbox.Rows[i]["LOTID_RT"].ToString();
                        newRow["INBOX_ID"] = dtdgInbox.Rows[i]["INBOX_ID"].ToString();
                        newRow["INBOX_ID_DEF"] = dtdgInbox.Rows[i]["INBOX_ID_DEF"].ToString();
                        newRow["INBOX_TYPE_NAME"] = dtdgInbox.Rows[i]["INBOX_TYPE_NAME"].ToString();
                        newRow["DFCT_RSN_GR_NAME"] = dtdgInbox.Rows[i]["DFCT_RSN_GR_NAME"].ToString();
                        newRow["CAPA_GRD_CODE"] = dtdgInbox.Rows[i]["CAPA_GRD_CODE"].ToString();
                        newRow["WIPQTY"] = dtdgInbox.Rows[i]["WIPQTY"].ToString();
                        newRow["CTNR_ID"] = dtdgInbox.Rows[i]["CTNR_ID"].ToString();
                        newRow["INPUT_DATA"] = "";
                        newRow["INBOX_QTY"] = dtdgInbox.Rows[i]["INBOX_QTY"].ToString();
                        newRow["INBOX_QTY_DEF"] = dtdgInbox.Rows[i]["INBOX_QTY_DEF"].ToString();
                        newRow["PRJT_NAME"] = dtdgInbox.Rows[i]["PRJT_NAME"].ToString();
                        newRow["PRODID"] = dtdgInbox.Rows[i]["PRODID"].ToString();
                        newRow["INBOX_TYPE_CODE"] = dtdgInbox.Rows[i]["INBOX_TYPE_CODE"].ToString();
                        newRow["DFCT_RSN_GR_ID"] = dtdgInbox.Rows[i]["DFCT_RSN_GR_ID"].ToString();
                        newRow["FORM_WRK_TYPE_CODE"] = dtdgInbox.Rows[i]["FORM_WRK_TYPE_CODE"].ToString();
                        newRow["FORM_WRK_TYPE_NAME"] = dtdgInbox.Rows[i]["FORM_WRK_TYPE_NAME"].ToString();
                        newRow["AOMM_GRD_CODE"] = dtdgInbox.Rows[i]["AOMM_GRD_CODE"].ToString();
                        TAGET_CART_INBOX.Rows.Add(newRow);
                    }
                }

                //타겟 INBOX 바인드
                DataRow bindRow = null;
                for (int i = 0; i < dtdgInbox.Rows.Count; i++)
                {
                    if (dtdgInbox.Rows[i]["CHK"].ToString() == "1")
                    {
                        bindRow = TAGET_CART_BIND_INBOX.NewRow();
                        bindRow["LOTID_RT"] = dtdgInbox.Rows[i]["LOTID_RT"].ToString();
                        bindRow["INBOX_ID"]   = dtdgInbox.Rows[i]["INBOX_ID"].ToString();
                        bindRow["INBOX_ID_DEF"] = dtdgInbox.Rows[i]["INBOX_ID_DEF"].ToString();
                        bindRow["INBOX_TYPE_NAME"] = dtdgInbox.Rows[i]["INBOX_TYPE_NAME"].ToString();
                        bindRow["DFCT_RSN_GR_NAME"] = dtdgInbox.Rows[i]["DFCT_RSN_GR_NAME"].ToString();
                        bindRow["CAPA_GRD_CODE"] = dtdgInbox.Rows[i]["CAPA_GRD_CODE"].ToString();
                        bindRow["WIPQTY"]     = dtdgInbox.Rows[i]["WIPQTY"].ToString();
                        bindRow["CTNR_ID"]      = dtdgInbox.Rows[i]["CTNR_ID"].ToString();
                        bindRow["INPUT_DATA"] = "";
                        bindRow["INBOX_QTY"] = dtdgInbox.Rows[i]["INBOX_QTY"].ToString();
                        bindRow["INBOX_QTY_DEF"] = dtdgInbox.Rows[i]["INBOX_QTY_DEF"].ToString();
                        bindRow["PRJT_NAME"] = dtdgInbox.Rows[i]["PRJT_NAME"].ToString();
                        bindRow["PRODID"] = dtdgInbox.Rows[i]["PRODID"].ToString();
                        bindRow["INBOX_TYPE_CODE"] = dtdgInbox.Rows[i]["INBOX_TYPE_CODE"].ToString();
                        bindRow["DFCT_RSN_GR_ID"] = dtdgInbox.Rows[i]["DFCT_RSN_GR_ID"].ToString();
                        bindRow["FORM_WRK_TYPE_CODE"] = dtdgInbox.Rows[i]["FORM_WRK_TYPE_CODE"].ToString();
                        bindRow["FORM_WRK_TYPE_NAME"] = dtdgInbox.Rows[i]["FORM_WRK_TYPE_NAME"].ToString();
                        bindRow["AOMM_GRD_CODE"] = dtdgInbox.Rows[i]["AOMM_GRD_CODE"].ToString();
                        TAGET_CART_BIND_INBOX.Rows.Add(bindRow);
                    }
                }
                Util.GridSetData(dgInbox_Taget, TAGET_CART_BIND_INBOX, FrameOperation, false);
                //이동 Inbox 정보 삭제
                for (int i = 0; i < dtdgInbox.Rows.Count; i++)
                {
                    if (dtdgInbox.Rows[i]["CHK"].ToString() == "1")
                    {
                        //전체 조회 DT에서 이동 정보 삭제
                        MOVE_CART_INBOX.Rows.Remove(MOVE_CART_INBOX.Select("INBOX_ID = '" + dtdgInbox.Rows[i]["INBOX_ID"].ToString() + "'")[0]);
                        // Bind DT에서 이동된 정보 삭제 
                      
                        MOVE_CART_BIND_INBOX.Rows.Remove(MOVE_CART_BIND_INBOX.Select("INBOX_ID = '" + dtdgInbox.Rows[i]["INBOX_ID"].ToString() + "'")[0]);

                    }
                }
               // 이동 INBOX 정보 Binding
                Util.GridSetData(dgInbox, MOVE_CART_BIND_INBOX, FrameOperation, false);

                //이동대차 조립LOT 정보 다시 집계
                MOVE_CART_GroupBy();
                //생성대차 조립LOT 정보 다시 집계
                MOVE_TAGET_CART_GroupBy();
                //이동대차 대차 정보 다시 집계
                MOVE_CTNR_GrouBy();
                //생성대차 대차 정보 다시 집계
                //신규대차일 경우 대차정보가 없으므로 집계하지 않음
                if(chkNew.IsChecked != true)
                {
                    MOVE_TAGET_CTNR_GrouBy();
                }

                //신규대차 일 경우 생성대차 정보 NEW 로 표시

                if (chkNew.IsChecked == true)
                {
                    DataTable TAGET_CTNR = DataTableConverter.Convert(dgCurrentList.ItemsSource);

                    TAGET_CTNR.Rows[0]["CTNR_ID"] = "< NEW >";
                    TAGET_CTNR.Rows[0]["INBOX_QTY"] = dgInbox_Taget.Rows.Count;

                    decimal SuMCellQty = 0;

                    for(int i=0; i <dgLotID_RT_Taget.Rows.Count; i++)
                    {
                        SuMCellQty = SuMCellQty + Convert.ToDecimal(Util.NVC(DataTableConverter.GetValue(dgLotID_RT_Taget.Rows[i].DataItem, "WIPQTY")));
                    }
                    TAGET_CTNR.Rows[0]["WIPQTY"] = SuMCellQty;

                    Util.GridSetData(dgCurrentList_Taget, TAGET_CTNR, FrameOperation, false);
                }


             }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }
        //대차이동 대차정보 품질유형 색변경
        private void dgCurrentList_Taget_LoadedCellPresenter(object sender, DataGridCellEventArgs e)
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
                    if (DataTableConverter.GetValue(e.Cell.Row.DataItem, "WIP_QLTY_TYPE_CODE").ToString() == "G")
                    {
                        if (e.Cell.Column.Name.Equals("WIP_QLTY_TYPE_NAME"))
                        {
                            e.Cell.Presenter.Background = new SolidColorBrush(Colors.Green);
                            e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                        }
                    }
                    else
                    {
                        if (e.Cell.Column.Name.Equals("WIP_QLTY_TYPE_NAME"))
                        {
                            e.Cell.Presenter.Background = new SolidColorBrush(Colors.Red);
                            e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                        }
                    }

                }
            }));
        }
        
        private void dgInbox_Taget_LoadedCellPresenter(object sender, DataGridCellEventArgs e)
        {
            dgInbox_Taget.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (e.Cell.Presenter == null)
                {
                    return;
                }

                if (e.Cell.Row.Type == DataGridRowType.Item)
                {
                    if (DataTableConverter.GetValue(e.Cell.Row.DataItem, "INPUT_DATA").ToString() == "Y")
                    {
                        e.Cell.Presenter.Background =  new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#E6F5FB"));
                    }
                    else
                    {
                        e.Cell.Presenter.Background = new SolidColorBrush(Colors.White);
                    }

                }
            }));
        }

        private void dgInbox_Taget_UnloadedCellPresenter(object sender, DataGridCellEventArgs e)
        {
            if (sender == null)
                return;

            C1DataGrid dataGrid = sender as C1DataGrid;

            dataGrid?.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (e.Cell.Presenter != null)
                {
                    if (e.Cell.Row.Type == DataGridRowType.Item)
                    {
                        var convertFromString = System.Windows.Media.ColorConverter.ConvertFromString("#FF000000");
                        if (convertFromString != null)
                            e.Cell.Presenter.Foreground = new SolidColorBrush((Color)convertFromString);
                    }
                }
            }));
        }

        // 이동버튼 이벤트
        private void chkNew_Click(object sender, RoutedEventArgs e)
        {

            if (_MOVE_CASTID == string.Empty) return;

            if (chkNew.IsChecked == true)
            {

                if (TAGET_CART_INBOX != null)
                {
                    //이동대차에서 생성대차 쪽으로 이동된 데이터가 있는지 확인
                    int mergeCount = 0;
                    for (int i = 0; i < TAGET_CART_INBOX.Rows.Count; i++)
                    {
                        if (TAGET_CART_INBOX.Rows[i]["INPUT_DATA"].ToString() == string.Empty)
                        {
                            mergeCount = mergeCount + 1;
                        }
                    }
                    // 이동된 데이터가 있으면 이동대차정보는 재 조회
                    if (mergeCount > 0)
                    {
                        GetCartInfo(_MOVE_CASTID, "T");
                        //조립LOT 정보
                        GetLotId_RT_Info(_MOVE_CASTID, "N");
                        //INBOX 정보
                        Inbox_Info(_MOVE_CASTID, "T");
                        //선택 조립LOT 의 INBOX 정보 BIND
                        CHK_Inbox_Bind();
                    }
                }
                NewCheckClear();
                txtTaget.IsEnabled = false;



            }
            else
            {
                NewCheckClear();
                txtTaget.IsEnabled = true;

                GetCartInfo(_MOVE_CASTID, "T");
                //조립LOT 정보
                GetLotId_RT_Info(_MOVE_CASTID, "N");
                //INBOX 정보
                Inbox_Info(_MOVE_CASTID, "T");
                //선택 조립LOT 의 INBOX 정보 BIND
                CHK_Inbox_Bind();
            }
        }

        // 대차재구성 이벤트
        private void btnComplate_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!Merge_Validation())
                {
                    return;
                }
                //재구성하시겠습니까?
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(
                        "SFU4377"), null, "Information", MessageBoxButton.OKCancel, MessageBoxIcon.None, (result) =>
                        {
                            if (result == MessageBoxResult.OK)
                            {
                                CartMerge();

                            }
                        });

            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }

        private void popupTagPrint_Select_Closed(object sender, EventArgs e)
        {
            COM001_203_TAG popupTagPrint = new COM001_203_TAG();

            LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("SFU1275"), null, "Information", MessageBoxButton.OK, MessageBoxIcon.None, (result) =>
            {
                if (result == MessageBoxResult.OK)
                {
                    Clear();
                }
            });
            //Util.MessageInfo("SFU1275");    //정상 처리 되었습니다.
            this.grdMain.Children.Remove(popupTagPrint);
        }

        //발행 이벤트
        private void btnPrint_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidationPrint()) return;

            string processName = Util.NVC(DataTableConverter.GetValue(dgInbox.Rows[0].DataItem, "PROCNAME")).ToString();
            string modelId = Util.NVC(DataTableConverter.GetValue(dgInbox.Rows[0].DataItem, "MODLID")).ToString();
            string projectName = Util.NVC(DataTableConverter.GetValue(dgInbox.Rows[0].DataItem, "PRJT_NAME")).ToString();
            string marketTypeName = Util.NVC(DataTableConverter.GetValue(dgInbox.Rows[0].DataItem, "MKT_TYPE_NAME")).ToString();
            string assyLotId = Util.NVC(DataTableConverter.GetValue(dgInbox.Rows[0].DataItem, "LOTID_RT")).ToString();
            string calDate = Util.NVC(DataTableConverter.GetValue(dgInbox.Rows[0].DataItem, "CALDATE")).ToString();
            string shiftName = Util.NVC(DataTableConverter.GetValue(dgInbox.Rows[0].DataItem, "SHFT_NAME")).ToString();
            string equipmentShortName = Util.NVC(DataTableConverter.GetValue(dgInbox.Rows[0].DataItem, "EQPTSHORTNAME")).ToString();
            string inspectorId = Util.NVC(DataTableConverter.GetValue(dgInbox.Rows[0].DataItem, "VISL_INSP_USERNAME")).ToString();



            // 불량 태그(라벨) 항목
            DataTable dtLabelItem = new DataTable();
            dtLabelItem.Columns.Add("LABEL_CODE", typeof(string));     //LABEL CODE
            dtLabelItem.Columns.Add("ITEM001", typeof(string));     //PROCESSNAME
            dtLabelItem.Columns.Add("ITEM002", typeof(string));     //MODEL
            dtLabelItem.Columns.Add("ITEM003", typeof(string));     //PKGLOT
            dtLabelItem.Columns.Add("ITEM004", typeof(string));     //DEFECT
            dtLabelItem.Columns.Add("ITEM005", typeof(string));     //QTY
            dtLabelItem.Columns.Add("ITEM006", typeof(string));     //MKTTYPE
            dtLabelItem.Columns.Add("ITEM007", typeof(string));     //PRODDATE
            dtLabelItem.Columns.Add("ITEM008", typeof(string));     //EQUIPMENT
            dtLabelItem.Columns.Add("ITEM009", typeof(string));     //INSPECTOR
            dtLabelItem.Columns.Add("ITEM010", typeof(string));     //
            dtLabelItem.Columns.Add("ITEM011", typeof(string));     //

            DataTable inTable = _bizDataSet.GetBR_PRD_REG_LABEL_HIST();

            foreach (DataGridRow row in dgInbox.Rows)
            {
                if (row.Type == DataGridRowType.Item &&
                   (DataTableConverter.GetValue(row.DataItem, "CHK").GetString() == "True" ||
                    DataTableConverter.GetValue(row.DataItem, "CHK").GetString() == "1"))
                {
                    DataRow dr = dtLabelItem.NewRow();
                    dr["LABEL_CODE"] = "LBL0106";
                    dr["ITEM001"] = Util.NVC(DataTableConverter.GetValue(row.DataItem, "CAPA_GRD_CODE"));
                    dr["ITEM002"] = modelId + "(" + projectName + ") ";
                    dr["ITEM003"] = Util.NVC(DataTableConverter.GetValue(row.DataItem, "LOTID_RT"));   //assyLotId;
                    dr["ITEM004"] = Util.NVC_Int(DataTableConverter.GetValue(row.DataItem, "WIPQTY")).GetString();
                    dr["ITEM005"] = equipmentShortName;
                    dr["ITEM006"] = DataTableConverter.GetValue(row.DataItem, "CALDATE").GetString() + "(" + DataTableConverter.GetValue(row.DataItem, "SHFT_NAME").GetString() + ")";
                    dr["ITEM007"] = inspectorId;
                    dr["ITEM008"] = marketTypeName;
                    dr["ITEM009"] = Util.NVC(DataTableConverter.GetValue(row.DataItem, "INBOX_ID"));
                    dr["ITEM010"] = string.Empty;
                    dr["ITEM011"] = string.Empty;
                    dtLabelItem.Rows.Add(dr);
            
                }
            }
            string printType;
            string resolution;
            string issueCount;
            string xposition;
            string yposition;
            string darkness;
            //string portName = dr["PORTNAME"].GetString();
            DataRow drPrintInfo;

            if (!_Util.GetConfigPrintInfo(out printType, out resolution, out issueCount, out xposition, out yposition, out darkness, out drPrintInfo))
                return;

            bool isLabelPrintResult = Util.PrintLabelPolymerForm(FrameOperation, loadingIndicator, dtLabelItem, printType, resolution, issueCount, xposition, yposition, darkness, drPrintInfo);

            if (!isLabelPrintResult)
            {
                //라벨 발행중 문제가 발생하였습니다.
                Util.MessageValidation("SFU3243");
            }
            else
            {
                //// 라벨 발행이력 저장
                //new ClientProxy().ExecuteService("BR_PRD_REG_LABEL_PRINT_HIST", "INDATA", null, inTable, (result, searchException) =>
                //{
                //    try
                //    {
                //        if (searchException != null)
                //        {
                //            Util.MessageException(searchException);
                //            return;
                //        }

                //    }
                //    catch (Exception ex)
                //    {
                //        Util.MessageException(ex);
                //    }
                //    finally
                //    {

                //    }
                //});

            }
        }

        //private void POLYMER_popupTagPrint_Closed(object sender, EventArgs e)
        //{
        //    CMM_FORM_TAG_PRINT popup = sender as CMM_FORM_TAG_PRINT;
        //    if (popup != null && popup.DialogResult == MessageBoxResult.OK)
        //    {
        //    }
        //    this.grdMain.Children.Remove(popup);
        //}

        //분할 팝업 열기
        private void btnSplit_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!Split_Validation())
                {
                    return;
                }
                int idxchk = _Util.GetDataGridCheckFirstRowIndex(dgInbox, "CHK");
                _SPLIT_LOTID_RT = Util.NVC(dgInbox.GetCell(idxchk, dgInbox.Columns["LOTID_RT"].Index).Value);
                COM001_203_INBOX_SPLIT popupSplitInbox = new COM001_203_INBOX_SPLIT();


                popupSplitInbox.FrameOperation = this.FrameOperation;

                object[] parameters = new object[3];

                parameters[0] = _MOVE_CASTID;   //대차ID;
                parameters[1] = Util.NVC(dgInbox.GetCell(idxchk, dgInbox.Columns["INBOX_ID"].Index).Value); //Inbox id
                parameters[2] = _WIP_QLTY_TYPE_CODE; //품질유형

                C1WindowExtension.SetParameters(popupSplitInbox, parameters);
                popupSplitInbox.Closed += new EventHandler(popupSplitInbox_Closed);
                //grdMain.Children.Add(popupCartDetail);
                //popupCartDetail.BringToFront();
                foreach (Grid tmp in Util.FindVisualChildren<Grid>(Application.Current.MainWindow))
                {
                    if (tmp.Name == "grdMain")
                    {
                        tmp.Children.Add(popupSplitInbox);
                        popupSplitInbox.BringToFront();
                        break;
                    }
                }

            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }

        }

        //분할팝업닫기
        private void popupSplitInbox_Closed(object sender, EventArgs e)
        {
            COM001_203_INBOX_SPLIT popup = sender as COM001_203_INBOX_SPLIT;
            if (popup != null && popup.DialogResult == MessageBoxResult.Cancel)
            {
                GetCartInfo(_MOVE_CASTID, "T");
                GetLotId_RT_Info(_MOVE_CASTID, "T");
                Inbox_Info(_MOVE_CASTID, "T");
                Split_Re_Bind(_SPLIT_LOTID_RT);

            }
            //this.grdMain.Children.Remove(popup);
            foreach (Grid tmp in Util.FindVisualChildren<Grid>(Application.Current.MainWindow))
            {
                if (tmp.Name == "grdMain")
                {
                    tmp.Children.Remove(popup);
                    break;
                }
            }

        }


        //병합 버튼 이벤트
        private void btnMerge_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!Merge_Inbox_Validation())
                {
                    return;
                }

                int idxchk = _Util.GetDataGridCheckFirstRowIndex(dgInbox, "CHK");
                _SPLIT_LOTID_RT = Util.NVC(dgInbox.GetCell(idxchk, dgInbox.Columns["LOTID_RT"].Index).Value);
                DataTable dtdgInbox = DataTableConverter.Convert(dgInbox.ItemsSource);

                DataTable Merge_Inbox = new DataTable();
                Merge_Inbox = MOVE_CART_INBOX.Clone();
                DataRow newRow = null;
                for (int i = 0; i < dtdgInbox.Rows.Count; i++)
                {
                    if (dtdgInbox.Rows[i]["CHK"].ToString() == "1")
                    {

                        newRow = Merge_Inbox.NewRow();
                        newRow["CHK"] = 0;
                        newRow["LOTID_RT"] = dtdgInbox.Rows[i]["LOTID_RT"].ToString();
                        newRow["INBOX_ID"] = dtdgInbox.Rows[i]["INBOX_ID"].ToString();
                        newRow["INBOX_ID_DEF"] = dtdgInbox.Rows[i]["INBOX_ID_DEF"].ToString();
                        newRow["INBOX_TYPE_NAME"] = dtdgInbox.Rows[i]["INBOX_TYPE_NAME"].ToString();
                        newRow["DFCT_RSN_GR_NAME"] = dtdgInbox.Rows[i]["DFCT_RSN_GR_NAME"].ToString();
                        newRow["CAPA_GRD_CODE"] = dtdgInbox.Rows[i]["CAPA_GRD_CODE"].ToString();
                        newRow["WIPQTY"] = dtdgInbox.Rows[i]["WIPQTY"].ToString();
                        newRow["CTNR_ID"] = dtdgInbox.Rows[i]["CTNR_ID"].ToString();
                        newRow["INPUT_DATA"] = "";
                        newRow["INBOX_QTY"] = dtdgInbox.Rows[i]["INBOX_QTY"].ToString();
                        newRow["INBOX_QTY_DEF"] = dtdgInbox.Rows[i]["INBOX_QTY_DEF"].ToString();
                        newRow["MODLID"] = dtdgInbox.Rows[i]["MODLID"].ToString();
                        newRow["MKT_TYPE_NAME"] = dtdgInbox.Rows[i]["MKT_TYPE_NAME"].ToString();
                        newRow["CALDATE"] = dtdgInbox.Rows[i]["CALDATE"].ToString();
                        newRow["EQPTSHORTNAME"] = dtdgInbox.Rows[i]["EQPTSHORTNAME"].ToString();
                        newRow["VISL_INSP_USERNAME"] = dtdgInbox.Rows[i]["VISL_INSP_USERNAME"].ToString();
                        newRow["PROCID"] = dtdgInbox.Rows[i]["PROCID"].ToString();
                        newRow["PROCNAME"] = dtdgInbox.Rows[i]["PROCNAME"].ToString();
                        newRow["PRJT_NAME"] = dtdgInbox.Rows[i]["PRJT_NAME"].ToString();
                        newRow["PRODID"] = dtdgInbox.Rows[i]["PRODID"].ToString();
                        newRow["INBOX_TYPE_CODE"] = dtdgInbox.Rows[i]["INBOX_TYPE_CODE"].ToString();
                        newRow["DFCT_RSN_GR_ID"] = dtdgInbox.Rows[i]["DFCT_RSN_GR_ID"].ToString();
                        newRow["FORM_WRK_TYPE_CODE"] = dtdgInbox.Rows[i]["FORM_WRK_TYPE_CODE"].ToString();
                        newRow["FORM_WRK_TYPE_NAME"] = dtdgInbox.Rows[i]["FORM_WRK_TYPE_NAME"].ToString();
                        newRow["RESNGR_ABBR_CODE"] = dtdgInbox.Rows[i]["RESNGR_ABBR_CODE"].ToString();

                        Merge_Inbox.Rows.Add(newRow);
                    }
                }

                COM001_203_INBOX_MERGE popupMergeInbox = new COM001_203_INBOX_MERGE();


                popupMergeInbox.FrameOperation = this.FrameOperation;

                object[] parameters = new object[3];

                parameters[0] = _MOVE_CASTID;   //대차ID;
                parameters[1] = Merge_Inbox;
                parameters[2] = _WIP_QLTY_TYPE_CODE; //품질유형

                C1WindowExtension.SetParameters(popupMergeInbox, parameters);
                popupMergeInbox.Closed += new EventHandler(popupMergeInbox_Closed);
                //grdMain.Children.Add(popupCartDetail);
                //popupCartDetail.BringToFront();
                foreach (Grid tmp in Util.FindVisualChildren<Grid>(Application.Current.MainWindow))
                {
                    if (tmp.Name == "grdMain")
                    {
                        tmp.Children.Add(popupMergeInbox);
                        popupMergeInbox.BringToFront();
                        break;
                    }
                }

            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }

        //병합 팝업 닫기
        private void popupMergeInbox_Closed(object sender, EventArgs e)
        {
            COM001_203_INBOX_MERGE popup = sender as COM001_203_INBOX_MERGE;
            if (popup != null && popup.DialogResult == MessageBoxResult.Cancel)
            {
                GetCartInfo(_MOVE_CASTID, "T");
                GetLotId_RT_Info(_MOVE_CASTID, "T");
                Inbox_Info(_MOVE_CASTID, "T");
                Split_Re_Bind(_SPLIT_LOTID_RT);

            }
            //this.grdMain.Children.Remove(popup);
            foreach (Grid tmp in Util.FindVisualChildren<Grid>(Application.Current.MainWindow))
            {
                if (tmp.Name == "grdMain")
                {
                    tmp.Children.Remove(popup);
                    break;
                }
            }

        }

        //특수 체크박스 클릭
        private void chkOrder_Click(object sender, RoutedEventArgs e)
        {
            if (_MOVE_CASTID == string.Empty) return;
            if (chkOrder.IsChecked == true)
            {
                cboOrder.IsEnabled = true;
                chkNew.IsChecked = true;
                chkNew.IsEnabled = false;

                if (TAGET_CART_INBOX != null)
                {
                    //이동대차에서 생성대차 쪽으로 이동된 데이터가 있는지 확인
                    int mergeCount = 0;
                    for (int i = 0; i < TAGET_CART_INBOX.Rows.Count; i++)
                    {
                        if (TAGET_CART_INBOX.Rows[i]["INPUT_DATA"].ToString() == string.Empty)
                        {
                            mergeCount = mergeCount + 1;
                        }
                    }
                    // 이동된 데이터가 있으면 이동대차정보는 재 조회
                    if (mergeCount > 0)
                    {
                        GetCartInfo(_MOVE_CASTID, "T");
                        //조립LOT 정보
                        GetLotId_RT_Info(_MOVE_CASTID, "N");
                        //INBOX 정보
                        Inbox_Info(_MOVE_CASTID, "T");
                        //선택 조립LOT 의 INBOX 정보 BIND
                        CHK_Inbox_Bind();
                    }
                }
                NewCheckClear();
                txtTaget.IsEnabled = false;
            }
            else
            {

                cboOrder.SelectedIndex = 0;
                cboOrder.IsEnabled = false;

                chkNew.IsChecked = false;
                chkNew.IsEnabled = true;

                NewCheckClear();
                txtTaget.IsEnabled = true;

                GetCartInfo(_MOVE_CASTID, "T");
                //조립LOT 정보
                GetLotId_RT_Info(_MOVE_CASTID, "N");
                //INBOX 정보
                Inbox_Info(_MOVE_CASTID, "T");
                //선택 조립LOT 의 INBOX 정보 BIND
                CHK_Inbox_Bind();
            }
        }

        #endregion

        #region 재구성이력조회
        private void btnSearchHistory_Click(object sender, RoutedEventArgs e)
        {
            Cart_Merge_History();
        }

        private void btnRePrint_Click(object sender, RoutedEventArgs e)
        {


            if (!RePrint_Validation())
            {
                return;
            }

            for (int i = 0; i < dgCartHistory.Rows.Count; i++)
            {
                if (Util.NVC(DataTableConverter.GetValue(dgCartHistory.Rows[i].DataItem, "CHK")) == "1")
                {
                    Cart_Merge_Tag(Util.NVC(DataTableConverter.GetValue(dgCartHistory.Rows[i].DataItem, "CTNR_ID")), Util.NVC(DataTableConverter.GetValue(dgCartHistory.Rows[i].DataItem, "TO_CTNR_ID")));
                }
            }

        }
        #endregion

        #endregion

        #region Mehod

        #region 대차재구성

        //이동대차 정보 조회
        public void GetCartInfo(string cstid, string chk)
        {
            try
            {
                DataTable dtRqst = new DataTable();
                dtRqst.Columns.Add("CTNR_ID", typeof(string));
                dtRqst.Columns.Add("LANGID", typeof(string));
                DataRow dr = dtRqst.NewRow();
                dr["CTNR_ID"] = cstid;
                dr["LANGID"] = LoginInfo.LANGID;
                dtRqst.Rows.Add(dr);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_POLYMER_CART_INFO", "INDATA", "OUTDATA", dtRqst);
                if (dtRslt.Rows.Count > 0)
                {
                    if (chk == "T")
                    {
                        Util.gridClear(dgCurrentList);
                        Util.GridSetData(dgCurrentList, dtRslt, FrameOperation, false);
                    }
                    else
                    {
                        Util.gridClear(dgCurrentList_Taget);
                        Util.GridSetData(dgCurrentList_Taget, dtRslt, FrameOperation, false);
                    }

                }
                else
                {
                    if(chk == "T")
                    {
                        Util.AlertInfo("SFU1905"); //"조회된 Data가 없습니다."
                        //Clear();
                        txtMove.Text = string.Empty;
                        return;
                    }
                    else
                    {
                        Util.AlertInfo("SFU1905"); //"조회된 Data가 없습니다."
                        //NewCheckClear();
                        txtTaget.Text = string.Empty;
                        return;
                    }
                }
               
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }
        //이동대차 조립LOT 정보 조회
        public void GetLotId_RT_Info(string cstid, string chk)
        {
            try
            {
                DataTable dtRqst = new DataTable();
                dtRqst.Columns.Add("CTNR_ID", typeof(string));
                dtRqst.Columns.Add("LANGID", typeof(string));
                DataRow dr = dtRqst.NewRow();
                dr["CTNR_ID"] = cstid;
                dr["LANGID"] = LoginInfo.LANGID;
                dtRqst.Rows.Add(dr);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_POLYMER_CART_LOTID_RT_INFO", "INDATA", "OUTDATA", dtRqst);
                if (dtRslt.Rows.Count > 0)
                {
                    if (chk == "T") // 이동
                    {
                        Util.gridClear(dgLotID_RT);
                        Util.GridSetData(dgLotID_RT, dtRslt, FrameOperation, false);
                        MOVE_CART_LOTID_RT = new DataTable();
                        MOVE_CART_LOTID_RT = dtRslt.Copy();
                    }
                    else if (chk == "C") //생성
                    {
                        Util.gridClear(dgLotID_RT_Taget);
                        Util.GridSetData(dgLotID_RT_Taget, dtRslt, FrameOperation, false);

                        TAGET_CART_LOTID_RT = new DataTable();
                        TAGET_CART_LOTID_RT = dtRslt.Copy();

                    }
                    else //신규대차 선택시 초기화
                    {
                        MOVE_CART_LOTID_RT = new DataTable();
                        MOVE_CART_LOTID_RT = dtRslt.Copy();

                        DataTable TMP = DataTableConverter.Convert(dgLotID_RT.ItemsSource);

                        for (int i =0; i< TMP.Rows.Count; i++)
                        {
                            for(int j=0; j< MOVE_CART_LOTID_RT.Rows.Count; j++)
                            {
                                if(TMP.Rows[i]["LOTID_RT"].ToString() == MOVE_CART_LOTID_RT.Rows[j]["LOTID_RT"].ToString())
                                {
                                    MOVE_CART_LOTID_RT.Rows[j]["CHK"] = TMP.Rows[i]["CHK"];

                                }

                            }

                        }
                        Util.gridClear(dgLotID_RT);
                        Util.GridSetData(dgLotID_RT, MOVE_CART_LOTID_RT, FrameOperation, false);
                        CHK_Inbox_Bind();
                    }
                }
              
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }
        //이동대차 INBOX 정보 조회
        public void Inbox_Info(string cstid, string chk)
        {
            try
            {
                DataTable dtRqst = new DataTable();
                dtRqst.Columns.Add("CTNR_ID", typeof(string));
                dtRqst.Columns.Add("LANGID", typeof(string));
                DataRow dr = dtRqst.NewRow();
                dr["CTNR_ID"] = cstid;
                dr["LANGID"] = LoginInfo.LANGID;
                dtRqst.Rows.Add(dr);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_POLYMER_CART_INBOX_INFO", "INDATA", "OUTDATA", dtRqst);
                if (dtRslt.Rows.Count > 0)
                {
                    if (chk == "T")
                    {
                        MOVE_CART_INBOX = new DataTable();
                        MOVE_CART_INBOX = dtRslt.Copy();

                    }
                    else if (chk == "C")
                    {
                        TAGET_CART_INBOX = new DataTable();
                        TAGET_CART_INBOX = dtRslt.Copy();

                    }
                
                }

            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }

        //전체 초기화
        public void Clear()
        {
           
            //이동대차
            txtMove.Text = string.Empty;
            Util.gridClear(dgCurrentList);
            Util.gridClear(dgLotID_RT);
            Util.gridClear(dgInbox);

            MOVE_CART_LOTID_RT = null;
            MOVE_CART_INBOX = null;
            MOVE_CART_BIND_INBOX = null;
            //생성대차
            txtTaget.Text = string.Empty;
            chkNew.IsChecked = false;
            Util.gridClear(dgCurrentList_Taget);
            Util.gridClear(dgLotID_RT_Taget);
            Util.gridClear(dgInbox_Taget);

            TAGET_CART_LOTID_RT = null;
            TAGET_CART_INBOX = null;
            TAGET_CART_BIND_INBOX = null;

            _MOVE_CASTID = string.Empty;
            _TAGET_CASTID = string.Empty;
            txtTaget.IsEnabled = true;
            chkOrder.IsChecked = false;
            cboOrder.SelectedIndex = 0;
            cboOrder.IsEnabled = false;


            //양품/불량 스프레드 셋팅
            DefectColumChange("G");

        }

        //생성대차 초기화
        public void NewCheckClear()
        {
          //생성대차
            txtTaget.Text = string.Empty;
           
            Util.gridClear(dgCurrentList_Taget);
            Util.gridClear(dgLotID_RT_Taget);
            Util.gridClear(dgInbox_Taget);
            TAGET_CART_LOTID_RT = null;
            TAGET_CART_INBOX = null;
            TAGET_CART_BIND_INBOX = null;
            _TAGET_CASTID = string.Empty;

        }
    
        //생성대차 Valldation
        private bool Create_Cart_Validation()
        {

            if(txtTaget.Text == string.Empty)
            {
                return false;
            }
            if (dgCurrentList.Rows.Count == 0)
            {
                Util.MessageValidation("SFU4378"); //이동대차 정보가 없습니다.
                return false;
            }

            if(_MOVE_CASTID == txtTaget.Text)
            {
                Util.MessageValidation("SFU4379"); //이동대차와 동일한 대차ID입니다.
                txtTaget.Text = string.Empty;
                txtTaget.Focus();
                return false;
            }


          
            return true;
        }
        
        private bool Move_Cart_Validation()
        {

            int Chk = 0;
            int ChkDub = 0;
            if (dgInbox.Rows.Count == 0)
            {
                Util.MessageValidation("SFU4387"); //이동할 INBOX 정보가 없습니다.
                return false;
            }
            for(int i=0; i< dgInbox.Rows.Count; i++)
            {
                if(DataTableConverter.GetValue(dgInbox.Rows[i].DataItem, "CHK").ToString() == "1")
                {
                    Chk = Chk + 1;
                }
                if (dgInbox_Taget.Rows.Count > 0)
                {
                    if (DataTableConverter.Convert(dgInbox_Taget.ItemsSource).Select("INBOX_ID = '" + Util.NVC(DataTableConverter.GetValue(dgInbox.Rows[i].DataItem, "INBOX_ID")) + "'").Length == 1)
                    {
                        ChkDub = ChkDub + 1;
                    }
                }
            }
           if(Chk ==0)
            {
                Util.MessageValidation("SFU4387"); //이동할 INBOX 정보가 없습니다.
                return false;
            }
            if(ChkDub >0)
            {

                Util.MessageValidation("SFU4381"); //"이미 이동된 INBOX 정보가 존재합니다."
                return false;
            }
            if (chkNew.IsChecked == false && dgCurrentList_Taget.Rows.Count == 0)
            {
                Util.MessageValidation("SFU4380"); //생성대차 정보가 없습니다.
                return false;
            }

            // 특수 일 경우 INBOX는 한개만 선택 가능하다
            if (chkOrder.IsChecked == true)
            {
                if (cboOrder.SelectedIndex == 0)
                {
                    Util.MessageValidation("SFU4599");  //특수 콤보를 선택하세요
                    return false;
                }

                // QMS 검사의뢰 대상인 경우 한건만 처리
                if (cboOrder.SelectedValue.Equals("QMS_INSP_REQ"))
                {
                    if (Chk > 1)
                    {
                        // 특수가 선택되어 INBOX 정보는 한개만 선택가능합니다.
                        Util.MessageValidation("SFU4598");
                        return false;
                    }
                    if (dgInbox_Taget.Rows.Count > 0)
                    {
                        // 특수가 선택되어 INBOX 정보는 한개만 선택가능합니다.
                        Util.MessageValidation("SFU4598");
                        return false;
                    }
                }
            }
            return true;
        }
        //선택된 조립LOT에 대한 INBOX 정보 조회
        public void CHK_Inbox_Bind()
        {
            MOVE_CART_BIND_INBOX = new DataTable();
            MOVE_CART_BIND_INBOX = MOVE_CART_INBOX.Clone();

            for (int i = 0; i < dgLotID_RT.Rows.Count; i++)
            {
                if (DataTableConverter.GetValue(dgLotID_RT.Rows[i].DataItem, "CHK").ToString() == "1")
                {
                    DataRow[] drInbox = MOVE_CART_INBOX.Select("LOTID_RT ='" + DataTableConverter.GetValue(dgLotID_RT.Rows[i].DataItem, "LOTID_RT").ToString() + "'");
                    foreach (DataRow dr in drInbox)
                    {
                        DataRow drBindInBox = MOVE_CART_BIND_INBOX.NewRow();
                        drBindInBox["CHK"] = 1;
                        drBindInBox["LOTID_RT"] = dr["LOTID_RT"];  //조립LOT
                        drBindInBox["INBOX_ID"] = dr["INBOX_ID"];  //INBOX ID
                        drBindInBox["INBOX_ID_DEF"] = dr["INBOX_ID_DEF"]; //불량그룹LOT
                        drBindInBox["INBOX_TYPE_NAME"] = dr["INBOX_TYPE_NAME"]; //INBOX유형명
                        drBindInBox["DFCT_RSN_GR_NAME"] = dr["DFCT_RSN_GR_NAME"]; //불량그룹명
                        drBindInBox["CAPA_GRD_CODE"] = dr["CAPA_GRD_CODE"]; //등급
                        drBindInBox["WIPQTY"] = dr["WIPQTY"];               //수량
                        drBindInBox["INPUT_DATA"] = dr["INPUT_DATA"]; 
                        drBindInBox["CTNR_ID"] = dr["CTNR_ID"];
                        drBindInBox["INBOX_QTY"] = dr["INBOX_QTY"];
                        drBindInBox["INBOX_QTY_DEF"] = dr["INBOX_QTY_DEF"];
                        drBindInBox["MODLID"] = dr["MODLID"];
                        drBindInBox["MKT_TYPE_NAME"] = dr["MKT_TYPE_NAME"];
                        drBindInBox["CALDATE"] = dr["CALDATE"];
                        drBindInBox["EQPTSHORTNAME"] = dr["EQPTSHORTNAME"];
                        drBindInBox["VISL_INSP_USERNAME"] = dr["VISL_INSP_USERNAME"];
                        drBindInBox["PROCID"] = dr["PROCID"];
                        drBindInBox["PROCNAME"] = dr["PROCNAME"];
                        drBindInBox["PRJT_NAME"] = dr["PRJT_NAME"];
                        drBindInBox["PRODID"] = dr["PRODID"];
                        drBindInBox["INBOX_TYPE_CODE"] = dr["INBOX_TYPE_CODE"];
                        drBindInBox["DFCT_RSN_GR_ID"] = dr["DFCT_RSN_GR_ID"];
                        drBindInBox["FORM_WRK_TYPE_CODE"] = dr["FORM_WRK_TYPE_CODE"];
                        drBindInBox["FORM_WRK_TYPE_NAME"] = dr["FORM_WRK_TYPE_NAME"];
                        drBindInBox["AOMM_GRD_CODE"] = dr["AOMM_GRD_CODE"];
                        MOVE_CART_BIND_INBOX.Rows.Add(drBindInBox);
                    }

                }
            }
            if (MOVE_CART_BIND_INBOX.Rows.Count == 0)
            {
                Util.gridClear(dgInbox);
            }
            else
            {
                Util.GridSetData(dgInbox, MOVE_CART_BIND_INBOX, FrameOperation, false);
                dgInbox.Columns["CAPA_GRD_CODE"].Width = new C1.WPF.DataGrid.DataGridLength(90);
            }

        }

        //이동대차 조립LOT 정보 재 집계
        public void MOVE_CART_GroupBy()
        {
            try
            {
                    var summarydata = from row in MOVE_CART_INBOX.AsEnumerable()
                        group row by new
                        {
                            LOTID_RT = row.Field<string>("LOTID_RT")
                            ,
                            FORM_WRK_TYPE_NAME = row.Field<string>("FORM_WRK_TYPE_NAME")
                            ,
                            PRJT_NAME = row.Field<string>("PRJT_NAME")
                            ,
                            PRODID = row.Field<string>("PRODID")
                            ,
                            CSTID = row.Field<string>("CTNR_ID")
                            ,
                            INPUT_DATA = row.Field<string>("INPUT_DATA")
                            ,
                            FORM_WRK_TYPE_CODE = row.Field<string>("FORM_WRK_TYPE_CODE")

                        } into grp
                        select new
                        {
                            LOTID_RT = grp.Key.LOTID_RT
                            ,
                            FORM_WRK_TYPE_NAME = grp.Key.FORM_WRK_TYPE_NAME
                            ,
                            PRJT_NAME = grp.Key.PRJT_NAME
                            ,
                            PRODID = grp.Key.PRODID
                            ,
                            CSTID = grp.Key.CSTID
                            ,
                            INPUT_DATA = grp.Key.INPUT_DATA
                            ,
                            FORM_WRK_TYPE_CODE = grp.Key.FORM_WRK_TYPE_CODE
                            ,
                            WIPQTY = grp.Sum(r => r.Field<int>("WIPQTY"))
                            ,
                            INBOX_QTY = grp.Sum(r => r.Field<int>("INBOX_QTY"))
                            ,
                            INBOX_QTY_DEF = grp.Sum(r => r.Field<int>("INBOX_QTY_DEF"))
                        };


                //기존 데이터의 체크된 값을 간직하기 위해 TEMP 테이블에 데이터를 COPY함
                //기존 테이블은 초기화 하여 다시 GROUPING 한 값으로 채워 넣는다.
                DataTable TMPTB = DataTableConverter.Convert(dgLotID_RT.ItemsSource);
                MOVE_CART_LOTID_RT = new DataTable();
                MOVE_CART_LOTID_RT = TMPTB.Clone();
                foreach (var data in summarydata)
                {
                    DataRow nrow = MOVE_CART_LOTID_RT.NewRow();
                    nrow["CHK"] = 0;  
                    nrow["LOTID_RT"] = data.LOTID_RT;
                    nrow["FORM_WRK_TYPE_NAME"] = data.FORM_WRK_TYPE_NAME;
                    nrow["PRJT_NAME"] = data.PRJT_NAME;
                    nrow["PRODID"] = data.PRODID;
                    nrow["INBOX_QTY"] = data.INBOX_QTY;
                    nrow["INBOX_QTY_DEF"] = data.INBOX_QTY_DEF;
                    nrow["WIPQTY"] = data.WIPQTY;
                    nrow["CTNR_ID"] = data.CSTID;
                    nrow["INPUT_DATA"] = data.INPUT_DATA;
                    nrow["FORM_WRK_TYPE_CODE"] = data.FORM_WRK_TYPE_CODE;
                    MOVE_CART_LOTID_RT.Rows.Add(nrow);
                 }
                //기존 체크값을 세팅함
                for(int i= 0; i< TMPTB.Rows.Count; i++)
                {
                    for(int j=0; j< MOVE_CART_LOTID_RT.Rows.Count; j++)
                    {
                        if(TMPTB.Rows[i]["LOTID_RT"].ToString() == MOVE_CART_LOTID_RT.Rows[j]["LOTID_RT"].ToString())
                        {
                            MOVE_CART_LOTID_RT.Rows[j]["CHK"] = TMPTB.Rows[i]["CHK"];

                        }
                    }
                }
                TMPTB = null;
                //이동 조립LOT 정보 Binding
                Util.GridSetData(dgLotID_RT, MOVE_CART_LOTID_RT, FrameOperation, false);



                //CHK_Inbox_Bind();
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }
        //생성대차 조립LOT 정보 재 집계
        public void MOVE_TAGET_CART_GroupBy()
        {
            try
            {
                var summarydata = from row in TAGET_CART_INBOX.AsEnumerable()
                                  group row by new
                                  {
                                      LOTID_RT = row.Field<string>("LOTID_RT")
                                      ,
                                      FORM_WRK_TYPE_NAME = row.Field<string>("FORM_WRK_TYPE_NAME")
                                      ,
                                      PRJT_NAME = row.Field<string>("PRJT_NAME")
                                      ,
                                      PRODID = row.Field<string>("PRODID")
                                        ,
                                      FORM_WRK_TYPE_CODE = row.Field<string>("FORM_WRK_TYPE_CODE")

                                  } into grp
                                  select new
                                  {
                                      LOTID_RT = grp.Key.LOTID_RT
                                      ,
                                      FORM_WRK_TYPE_NAME = grp.Key.FORM_WRK_TYPE_NAME
                                      ,
                                      PRJT_NAME = grp.Key.PRJT_NAME
                                      ,
                                      PRODID = grp.Key.PRODID
                                      ,
                                      WIPQTY = grp.Sum(r => r.Field<int>("WIPQTY"))
                                      ,
                                      INBOX_QTY = grp.Sum(r => r.Field<int>("INBOX_QTY"))
                                        ,
                                      INBOX_QTY_DEF = grp.Sum(r => r.Field<int>("INBOX_QTY_DEF"))
                                        ,
                                      FORM_WRK_TYPE_CODE = grp.Key.FORM_WRK_TYPE_CODE
                                  };


                //기존 데이터의 체크된 값을 간직하기 위해 TEMP 테이블에 데이터를 COPY함
                //기존 테이블은 초기화 하여 다시 GROUPING 한 값으로 채워 넣는다.
                DataTable TMPTB = new DataTable();
                if (dgLotID_RT_Taget.ItemsSource != null)
                {
                    TMPTB = DataTableConverter.Convert(dgLotID_RT_Taget.ItemsSource);
                }
                else
                {
                    TMPTB = MOVE_CART_LOTID_RT.Clone();
                }
                TAGET_CART_LOTID_RT = new DataTable();
                TAGET_CART_LOTID_RT = TMPTB.Clone();
                foreach (var data in summarydata)
                {
                    DataRow nrow = TAGET_CART_LOTID_RT.NewRow();
                    nrow["CHK"] = 0;
                    nrow["LOTID_RT"] = data.LOTID_RT;
                    nrow["FORM_WRK_TYPE_NAME"] = data.FORM_WRK_TYPE_NAME;
                    nrow["PRJT_NAME"] = data.PRJT_NAME;
                    nrow["PRODID"] = data.PRODID;
                    nrow["INBOX_QTY"] = data.INBOX_QTY;
                    nrow["INBOX_QTY_DEF"] = data.INBOX_QTY_DEF;
                    nrow["WIPQTY"] = data.WIPQTY;
                    if(chkNew.IsChecked == true)
                    {
                        nrow["CTNR_ID"] = string.Empty;
                    }
                    else
                    {
                        nrow["CTNR_ID"] = Util.NVC(DataTableConverter.GetValue(dgCurrentList_Taget.Rows[0].DataItem, "CTNR_ID"));
                    }
                   
                    nrow["INPUT_DATA"] = "Y";
                    nrow["FORM_WRK_TYPE_CODE"] = data.FORM_WRK_TYPE_CODE;
                    TAGET_CART_LOTID_RT.Rows.Add(nrow);
                }
                //기존 체크값을 세팅함
                for (int i = 0; i < TMPTB.Rows.Count; i++)
                {
                    for (int j = 0; j < TAGET_CART_LOTID_RT.Rows.Count; j++)
                    {
                        if (TMPTB.Rows[i]["LOTID_RT"].ToString() == TAGET_CART_LOTID_RT.Rows[j]["LOTID_RT"].ToString())
                        {
                            TAGET_CART_LOTID_RT.Rows[j]["CHK"] = TMPTB.Rows[i]["CHK"];

                        }
                    }
                }
                TMPTB = null;
                //이동 조립LOT 정보 Binding
                Util.GridSetData(dgLotID_RT_Taget, TAGET_CART_LOTID_RT, FrameOperation, false);



                //CHK_Inbox_Bind();
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }

        //이동대차 대차 정보 재 집계
        public void MOVE_CTNR_GrouBy()
        {
            if(dgLotID_RT.Rows.Count > 0)
            {
                decimal SumInBoxQty = 0;
                decimal SumCellQty = 0;

                for(int i =0; i< dgLotID_RT.Rows.Count; i++)
                {
                    SumInBoxQty = SumInBoxQty + Convert.ToDecimal(DataTableConverter.GetValue(dgLotID_RT.Rows[0].DataItem, "INBOX_QTY"));
                    SumCellQty = SumCellQty + Convert.ToDecimal(DataTableConverter.GetValue(dgLotID_RT.Rows[0].DataItem, "WIPQTY"));
                }
                DataTableConverter.SetValue(dgCurrentList.Rows[0].DataItem, "INBOX_QTY", SumInBoxQty);
                DataTableConverter.SetValue(dgCurrentList.Rows[0].DataItem, "WIPQTY", SumCellQty);
            }
            else
            {
                DataTableConverter.SetValue(dgCurrentList.Rows[0].DataItem, "INBOX_QTY", 0);
                DataTableConverter.SetValue(dgCurrentList.Rows[0].DataItem, "WIPQTY", 0);
            }
        }
        //생성대차  대차 정보 재 집계
        public void MOVE_TAGET_CTNR_GrouBy()
        {
            if (dgLotID_RT_Taget.Rows.Count > 0)
            {
                decimal SumInBoxQty = 0;
                decimal SumCellQty = 0;

                for (int i = 0; i < dgLotID_RT_Taget.Rows.Count; i++)
                {
                    SumInBoxQty = SumInBoxQty + Convert.ToDecimal(DataTableConverter.GetValue(dgLotID_RT_Taget.Rows[0].DataItem, "INBOX_QTY"));
                    SumCellQty = SumCellQty + Convert.ToDecimal(DataTableConverter.GetValue(dgLotID_RT_Taget.Rows[0].DataItem, "WIPQTY"));
                }
                DataTableConverter.SetValue(dgCurrentList_Taget.Rows[0].DataItem, "INBOX_QTY", SumInBoxQty);
                DataTableConverter.SetValue(dgCurrentList_Taget.Rows[0].DataItem, "WIPQTY", SumCellQty);
            }
            else
            {
                DataTableConverter.SetValue(dgCurrentList_Taget.Rows[0].DataItem, "INBOX_QTY", 0);
                DataTableConverter.SetValue(dgCurrentList_Taget.Rows[0].DataItem, "WIPQTY", 0);

            }
        }

        private bool Merge_Validation()
        {

            if (_MOVE_CASTID == string.Empty)
            {
                Util.MessageValidation("SFU4378"); //이동대차 정보가 없습니다.
                return false;
            }
            if (chkNew.IsChecked == false && _TAGET_CASTID == string.Empty)
            {
                Util.MessageValidation("SFU4380"); //생성대차 정보가 없습니다.
                return false;
            }
            if(TAGET_CART_INBOX == null)
            {
                Util.MessageValidation("SFU4388"); //재구성할 정보가 없습니다.
                return false;
            }
            if (TAGET_CART_INBOX.Rows.Count == 0)
            {
                Util.MessageValidation("SFU4388"); //재구성할 정보가 없습니다.
                return false;
            }

            int mergeCount = 0;
            for(int i=0; i< TAGET_CART_INBOX.Rows.Count; i++)
            {
                if(TAGET_CART_INBOX.Rows[i]["INPUT_DATA"].ToString() == string.Empty)
                {
                    mergeCount = mergeCount + 1;
                }
            }
             if(mergeCount == 0)
            {
                Util.MessageValidation("SFU4388"); //재구성할 정보가 없습니다.
                return false;
            }


            return true;
        }

        private void CartMerge()
        {
            DataSet inData = new DataSet();

            //마스터 정보
            DataTable inDataTable = inData.Tables.Add("INDATA");
            inDataTable.Columns.Add("SRCTYPE", typeof(string));
            inDataTable.Columns.Add("CTNR_ID", typeof(string));
            inDataTable.Columns.Add("TO_CTNR_ID", typeof(string));
            inDataTable.Columns.Add("USERID", typeof(string));
            inDataTable.Columns.Add("PROCID", typeof(string));
            inDataTable.Columns.Add("AREAID", typeof(string));
            inDataTable.Columns.Add("CTNR_TYPE_CODE", typeof(string));
            inDataTable.Columns.Add("EXCEPT_FLAG", typeof(string));
            inDataTable.Columns.Add("PRDT_REQ_PRCS_TYPE_CODE", typeof(string));
                     

            DataRow row = null;
            row = inDataTable.NewRow();
            row["SRCTYPE"] = "UI";
            row["CTNR_ID"] = _MOVE_CASTID;
            row["TO_CTNR_ID"] = _TAGET_CASTID;
            row["USERID"] = LoginInfo.USERID;
            if(chkNew.IsChecked == true && chkOrder.IsChecked == false) // 신규대차일경우
            {
                row["PROCID"] = _PROCID;
                row["AREAID"] = _AREAID;
                row["CTNR_TYPE_CODE"] = "CART";
            }else if (chkNew.IsChecked == true && chkOrder.IsChecked == true) // 특수일경우
            {

                if (cboOrder.SelectedIndex == 0)
                {
                    Util.MessageValidation("SFU4599");  //특수 콤보를 선택하세요
                    return;
                }
                row["PROCID"] = _PROCID;
                row["AREAID"] = _AREAID;
                row["CTNR_TYPE_CODE"] = "CART";
                row["EXCEPT_FLAG"] = "Y";
                row["PRDT_REQ_PRCS_TYPE_CODE"] = cboOrder.SelectedValue.ToString();
            }
            else
            {
                row["EXCEPT_FLAG"] = "N";
            }

            inDataTable.Rows.Add(row);

            //재구성 LOT
            DataTable inLot = inData.Tables.Add("INLOT");
            inLot.Columns.Add("LOTID", typeof(string));

            for (int i = 0; i < TAGET_CART_INBOX.Rows.Count; i++)
            {

                if (TAGET_CART_INBOX.Rows[i]["INPUT_DATA"].ToString() == string.Empty)
                {
                    row = inLot.NewRow();
                    row["LOTID"] = TAGET_CART_INBOX.Rows[i]["INBOX_ID"].ToString();
                    inLot.Rows.Add(row);
                }

            }
            try
            {
                //대차 재구성
                new ClientProxy().ExecuteService_Multi("BR_PRD_REG_COMPOSE_CONTAINER", "INDATA,INLOT", "OUT_DATA", (Result, ex) =>
                {
                    if (ex != null)
                    {
                        Util.MessageException(ex);
                        return;
                    }
                    Cart_Merge_Tag(_MOVE_CASTID, Result.Tables["OUT_DATA"].Rows[0]["TO_CTNR_ID"].ToString());

                }, inData);



            }
            catch (Exception ex)
            {
                Util.AlertByBiz("BR_ACT_REG_COMPOSE_CONTAINER", ex.Message, ex.ToString());

            }
        }

        public void Cart_Merge_Tag(string MoveCastid, string TagetCastid)
        {
            try
            {
                COM001_203_TAG popupTagPrint = new COM001_203_TAG();
                popupTagPrint.FrameOperation = this.FrameOperation;

                DataTable Result = new DataTable();
                Result.Columns.Add("CTNR_ID", typeof(string));

                DataRow row = null;

                row = Result.NewRow();
                row["CTNR_ID"] = MoveCastid;
                Result.Rows.Add(row);

                row = Result.NewRow();
                row["CTNR_ID"] = TagetCastid;
                Result.Rows.Add(row);

                object[] parameters = new object[1];

                if (Result.Rows.Count > 0)
                {
                    parameters[0] = Result;
                }
                C1WindowExtension.SetParameters(popupTagPrint, parameters);

                popupTagPrint.Closed += new EventHandler(popupTagPrint_Select_Closed);
                grdMain.Children.Add(popupTagPrint);
                popupTagPrint.BringToFront();
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
            finally
            {

            }
        }

        //Inbox 병합 Validation
        private bool Merge_Inbox_Validation()
        {

            if (dgInbox.Rows.Count == 0)
            {
                Util.MessageValidation("SFU4467"); //Inbox 정보가 없습니다.
                return false;
            }

            int ChkCount = 0;
            string sAOMM_GRD_CODE = string.Empty;
            string sINBOX_ID = string.Empty;

            for (int i = 0; i < dgInbox.Rows.Count; i++)
            {
                if (DataTableConverter.GetValue(dgInbox.Rows[i].DataItem, "CHK").ToString() == "1")
                {
                    ChkCount = ChkCount + 1;

                    if(ChkCount == 1)
                    {
                        sAOMM_GRD_CODE = Util.NVC(DataTableConverter.GetValue(dgInbox.Rows[i].DataItem, "AOMM_GRD_CODE"));
                        sINBOX_ID = Util.NVC(DataTableConverter.GetValue(dgInbox.Rows[i].DataItem, "INBOX_ID"));
                    }
                    else
                    {
                        string sAOMM_GRD_CODE2 = Util.NVC(DataTableConverter.GetValue(dgInbox.Rows[i].DataItem, "AOMM_GRD_CODE"));
                        string sINBOX_ID2 = Util.NVC(DataTableConverter.GetValue(dgInbox.Rows[i].DataItem, "INBOX_ID"));

                        if (sAOMM_GRD_CODE2 != sAOMM_GRD_CODE)
                        {
                            Util.MessageValidation("SFU3802", new object[] { sINBOX_ID, sAOMM_GRD_CODE, sINBOX_ID2, sAOMM_GRD_CODE2 }); //%1 의 AOMM 등급 %2 와 %3 의 AOMM 등급 %4 가 다릅니다.
                            return false;
                        }
                    }
                }
            }
            if (ChkCount == 0)
            {
                Util.MessageValidation("SFU1651"); //선택된 항목이 없습니다.
                return false;
            }
            if (ChkCount == 1)
            {
                Util.MessageValidation("SFU4600"); //한개 이상의 데이터를 선택하세요
                return false;
            }

            return true;
        }

        //Inbox 분할 Validation
        private bool Split_Validation()
        {

            if (dgInbox.Rows.Count == 0)
            {
                Util.MessageValidation("SFU4467"); //Inbox 정보가 없습니다.
                return false;
            }

            int ChkCount = 0;

            for (int i = 0; i < dgInbox.Rows.Count; i++)
            {
                if (DataTableConverter.GetValue(dgInbox.Rows[i].DataItem, "CHK").ToString() == "1")
                {
                    ChkCount = ChkCount + 1;
                }
            }
            if (ChkCount == 0)
            {
                Util.MessageValidation("SFU1651"); //선택된 항목이 없습니다.
                return false;
            }
            if(ChkCount > 1)
            {
                Util.MessageValidation("SFU4468"); //한개의 데이터만 선택하세요
                return false;

            }



            return true;
        }
        
        private bool ValidationPrint()
        {

            if (_Util.GetDataGridCheckFirstRowIndex(dgInbox, "CHK") < 0)
            {
                //Util.Alert("선택된 항목이 없습니다.");
                Util.MessageValidation("SFU1651");
                return false;
            }

            if (LoginInfo.CFG_SERIAL_PRINT.Rows.Count < 1)
            {
                Util.MessageValidation("SFU2003"); //프린트 환경 설정값이 없습니다.
                return false;
            }

            var query = (from t in LoginInfo.CFG_SERIAL_PRINT.AsEnumerable()
                         where t.Field<string>("LABELID") == "LBL0106"
                         select t).ToList();

            if (!query.Any())
            {
                Util.MessageValidation("SFU4339"); //프린터 환경설정에 라벨정보 항목이 없습니다.
                return false;
            }

            if (LoginInfo.CFG_SERIAL_PRINT.Rows.Cast<DataRow>().Any(itemRow => string.IsNullOrEmpty(itemRow[CustomConfig.CONFIGTABLE_SERIALPRINTER_LABELID].GetString())))
            {
                Util.MessageValidation("SFU4339"); //프린터 환경설정에 라벨정보 항목이 없습니다.
                return false;
            }

            return true;
        }

        //양품대차/불량대차에 따라 스프레드 컬럼정보 변경
        public void DefectColumChange(string Qlty_Type)
        {

            if(Qlty_Type == "G")
            {
                //이동대차 조립LOT 스프레드
                dgLotID_RT.Columns["INBOX_QTY"].Visibility = Visibility.Visible;       // INBOX 수량
                dgLotID_RT.Columns["INBOX_QTY_DEF"].Visibility = Visibility.Collapsed; // 불량그룹 LOT 수
                //이동대차 INBOX 스프레드
                dgInbox.Columns["INBOX_ID"].Visibility = Visibility.Visible;           // INBOX ID
                dgInbox.Columns["INBOX_ID_DEF"].Visibility = Visibility.Collapsed;     // 불량그룹 LOT
                dgInbox.Columns["INBOX_TYPE_NAME"].Visibility = Visibility.Visible;    // INBOX 유형
                dgInbox.Columns["DFCT_RSN_GR_NAME"].Visibility = Visibility.Collapsed;     // 불량그룹명

                //생성대차 조립LOT 스프레드
                dgLotID_RT_Taget.Columns["INBOX_QTY"].Visibility = Visibility.Visible;       // INBOX 수량
                dgLotID_RT_Taget.Columns["INBOX_QTY_DEF"].Visibility = Visibility.Collapsed; // 불량그룹 LOT 수
                //생성대차 INBOX 스프레드
                dgInbox_Taget.Columns["INBOX_ID"].Visibility = Visibility.Visible;           // INBOX ID
                dgInbox_Taget.Columns["INBOX_ID_DEF"].Visibility = Visibility.Collapsed;     // 불량그룹 LOT
                dgInbox_Taget.Columns["INBOX_TYPE_NAME"].Visibility = Visibility.Visible;    // INBOX 유형
                dgInbox_Taget.Columns["DFCT_RSN_GR_NAME"].Visibility = Visibility.Collapsed;     // 불량그룹명

                btnPrint.Visibility = Visibility.Visible;

            }
            else
            {
                //이동대차 조립LOT 스프레드
                dgLotID_RT.Columns["INBOX_QTY"].Visibility = Visibility.Collapsed;   // INBOX 수량
                dgLotID_RT.Columns["INBOX_QTY_DEF"].Visibility = Visibility.Visible; // 불량그룹 LOT 수
                //이동대차 INBOX 스프레드
                dgInbox.Columns["INBOX_ID"].Visibility = Visibility.Collapsed;       // INBOX ID
                dgInbox.Columns["INBOX_ID_DEF"].Visibility = Visibility.Visible;     // 불량그룹 LOT
                dgInbox.Columns["INBOX_TYPE_NAME"].Visibility = Visibility.Collapsed;// INBOX 유형
                dgInbox.Columns["DFCT_RSN_GR_NAME"].Visibility = Visibility.Visible;     // 불량그룹명

                //생성대차 조립LOT 스프레드
                dgLotID_RT_Taget.Columns["INBOX_QTY"].Visibility = Visibility.Collapsed;   // INBOX 수량
                dgLotID_RT_Taget.Columns["INBOX_QTY_DEF"].Visibility = Visibility.Visible; // 불량그룹 LOT 수
                //생성대차 INBOX 스프레드
                dgInbox_Taget.Columns["INBOX_ID"].Visibility = Visibility.Collapsed;       // INBOX ID
                dgInbox_Taget.Columns["INBOX_ID_DEF"].Visibility = Visibility.Visible;     // 불량그룹 LOT
                dgInbox_Taget.Columns["INBOX_TYPE_NAME"].Visibility = Visibility.Collapsed;// INBOX 유형
                dgInbox_Taget.Columns["DFCT_RSN_GR_NAME"].Visibility = Visibility.Visible;     // 불량그룹명

                btnPrint.Visibility = Visibility.Collapsed;
            }




        }

        //분할후  조립LOT 재조회
        private void Split_Re_Bind(string LotID_RT)
        {
            MOVE_CART_BIND_INBOX = new DataTable();
            MOVE_CART_BIND_INBOX = MOVE_CART_INBOX.Clone();
            for (int i = 0; i < dgLotID_RT.Rows.Count; i++)
            {

                if (DataTableConverter.GetValue(dgLotID_RT.Rows[i].DataItem, "LOTID_RT").ToString() == LotID_RT)
                {
                    DataTableConverter.SetValue(dgLotID_RT.Rows[i].DataItem, "CHK", true);

                    DataRow[] drInbox = MOVE_CART_INBOX.Select("LOTID_RT ='" + DataTableConverter.GetValue(dgLotID_RT.Rows[i].DataItem, "LOTID_RT").ToString() + "'");
                    foreach (DataRow dr in drInbox)
                    {

                        DataRow drBindInBox = MOVE_CART_BIND_INBOX.NewRow();
                        drBindInBox["CHK"] = 1;
                        drBindInBox["LOTID_RT"] = dr["LOTID_RT"];  //조립LOT
                        drBindInBox["INBOX_ID"] = dr["INBOX_ID"];  //INBOX ID
                        drBindInBox["INBOX_ID_DEF"] = dr["INBOX_ID_DEF"]; //불량그룹LOT
                        drBindInBox["INBOX_TYPE_NAME"] = dr["INBOX_TYPE_NAME"]; //INBOX유형명
                        drBindInBox["DFCT_RSN_GR_NAME"] = dr["DFCT_RSN_GR_NAME"]; //불량그룹명
                        drBindInBox["CAPA_GRD_CODE"] = dr["CAPA_GRD_CODE"]; //등급
                        drBindInBox["WIPQTY"] = dr["WIPQTY"];               //수량
                        drBindInBox["INPUT_DATA"] = dr["INPUT_DATA"];
                        drBindInBox["CTNR_ID"] = dr["CTNR_ID"];
                        drBindInBox["INBOX_QTY"] = dr["INBOX_QTY"];
                        drBindInBox["INBOX_QTY_DEF"] = dr["INBOX_QTY_DEF"];
                        drBindInBox["MODLID"] = dr["MODLID"];
                        drBindInBox["MKT_TYPE_NAME"] = dr["MKT_TYPE_NAME"];
                        drBindInBox["CALDATE"] = dr["CALDATE"];
                        drBindInBox["EQPTSHORTNAME"] = dr["EQPTSHORTNAME"];
                        drBindInBox["VISL_INSP_USERNAME"] = dr["VISL_INSP_USERNAME"];
                        drBindInBox["PROCID"] = dr["PROCID"];
                        drBindInBox["PROCNAME"] = dr["PROCNAME"];
                        drBindInBox["PRJT_NAME"] = dr["PRJT_NAME"];
                        drBindInBox["PRODID"] = dr["PRODID"];
                        drBindInBox["INBOX_TYPE_CODE"] = dr["INBOX_TYPE_CODE"];
                        drBindInBox["DFCT_RSN_GR_ID"] = dr["DFCT_RSN_GR_ID"];
                        drBindInBox["FORM_WRK_TYPE_CODE"] = dr["FORM_WRK_TYPE_CODE"];
                        drBindInBox["FORM_WRK_TYPE_NAME"] = dr["FORM_WRK_TYPE_NAME"];
                        drBindInBox["AOMM_GRD_CODE"] = dr["AOMM_GRD_CODE"];
                        MOVE_CART_BIND_INBOX.Rows.Add(drBindInBox);
                    }
                }

            }
            if (MOVE_CART_BIND_INBOX.Rows.Count == 0)
            {
                Util.gridClear(dgInbox);
            }
            else
            {
                Util.GridSetData(dgInbox, MOVE_CART_BIND_INBOX, FrameOperation, false);
            }

        }

        #endregion

        #region 재구성이력조회
        public void Cart_Merge_History()
        {
            try
            {
                DataTable dtRqst = new DataTable();
                dtRqst.Columns.Add("FROM_DATE", typeof(String));
                dtRqst.Columns.Add("TO_DATE", typeof(String));
                dtRqst.Columns.Add("EQSGID", typeof(string));
                dtRqst.Columns.Add("PROCID", typeof(string));
                dtRqst.Columns.Add("PJT_NAME", typeof(string));
                dtRqst.Columns.Add("PRODID", typeof(string));
                dtRqst.Columns.Add("LOTID_RT", typeof(string));
                dtRqst.Columns.Add("LANGID", typeof(string));
                dtRqst.Columns.Add("CTNR_ID", typeof(string));
                dtRqst.Columns.Add("AREAID", typeof(string));

                //ldpDateFromHist.SelectedDateTime.ToShortDateString();
                DataRow dr = dtRqst.NewRow();
                dr["FROM_DATE"] = Util.GetCondition(ldpDateFromHist_Detail);
                dr["TO_DATE"] = Util.GetCondition(ldpDateToHist_Detail);
                //dr["EQSGID"] = Util.GetCondition(cboEquipmentSegment, "SFU1223"); // 라인을선택하세요. >> 라인을 선택 하세요.
                //if (dr["EQSGID"].Equals("")) return;
                dr["EQSGID"] = Util.GetCondition(cboEquipmentSegment, bAllNull: true);
                dr["PROCID"] = Util.GetCondition(cboProcessHistory, bAllNull: true);
                dr["PJT_NAME"] = txtPjtHistory_Detail.Text;
                dr["PRODID"] = txtProdID_Detail.Text;
                dr["LOTID_RT"] = txtLotRTHistory_Detail.Text;
                dr["LANGID"] = LoginInfo.LANGID;
                dr["CTNR_ID"] = Util.GetCondition(txtCTNR_IDHistory);
                dr["AREAID"] = Util.GetCondition(cboAreaHistory, "SFU4238"); // 동정보를 선택하세요
                if (dr["AREAID"].Equals("")) return;
                dtRqst.Rows.Add(dr);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_POLYMER_CART_MERGE_HISTORY", "INDATA", "OUTDATA", dtRqst);

                if (dtRslt.Rows.Count == 0)
                {
                    Util.AlertInfo("SFU1905"); //"조회된 Data가 없습니다."
                    dgCartHistory.ItemsSource = DataTableConverter.Convert(dtRslt);
                    return;
                }

                for (int i = 0; i < dtRslt.Rows.Count; i++)
                {
                    if (dtRslt.Rows[i]["TO_CTNR_PRE_WIPQTY"].ToString() == "0")
                    {
                        dtRslt.Rows[i]["DIVISION"] = ObjectDic.Instance.GetObjectName("신규");
                    }
                    else
                    {
                        dtRslt.Rows[i]["DIVISION"] = ObjectDic.Instance.GetObjectName("추가");
                    }
                }

                Util.GridSetData(dgCartHistory, dtRslt, FrameOperation, false);

            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }

        private bool RePrint_Validation()
        {

            int CheckCount = 0;
            for (int i = 0; i < dgCartHistory.Rows.Count; i++)
            {
                if (Util.NVC(DataTableConverter.GetValue(dgCartHistory.Rows[i].DataItem, "CHK")) == "1")
                {
                    CheckCount = CheckCount + 1;
                }
            }

            if (CheckCount > 1)
            {
                Util.MessageValidation("SFU4159");//한건만 선택하세요.
                return false;
            }


            return true;
        }




        #endregion

        #endregion

      
    }
}
