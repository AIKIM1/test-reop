/*************************************************************************************
 Created Date : 2016.06.16
      Creator : 
   Decription : 
--------------------------------------------------------------------------------------
 [Change History]
  2016.06.16  DEVELOPER : Initial Created.





 
**************************************************************************************/

using C1.WPF;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace LGC.GMES.MES.COM001
{
    public partial class COM001_049 : UserControl, IWorkArea
    {
        #region Declaration & Constructor 

        string _AREATYPE = "";

        ArrayList alColumnFields = new ArrayList();
        ArrayList alRowFields = new ArrayList();
        ArrayList alValueFields = new ArrayList();


        private BizDataSet _Biz = new BizDataSet();
        public COM001_049()
        {
            InitializeComponent();

            InitCombo();
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

            //동,라인,공정,설비 셋팅
            CommonCombo _combo = new CommonCombo();


            //동
            C1ComboBox[] cboAreaChild = { cboEquipmentSegment };
            _combo.SetCombo(cboArea, CommonCombo.ComboStatus.SELECT, cbChild: cboAreaChild);

            //라인
            C1ComboBox[] cboEquipmentSegmentParent = { cboArea };
            C1ComboBox[] cboEquipmentSegmentChild = { cboProcess, cboEquipment };
            _combo.SetCombo(cboEquipmentSegment, CommonCombo.ComboStatus.SELECT, cbChild: cboEquipmentSegmentChild, cbParent: cboEquipmentSegmentParent);

            //공정
            C1ComboBox[] cboProcessParent = { cboEquipmentSegment };
            C1ComboBox[] cboProcessChild = { cboEquipment };
            _combo.SetCombo(cboProcess, CommonCombo.ComboStatus.SELECT, cbChild: cboProcessChild, cbParent: cboProcessParent);


            cboProcess.SelectedItemChanged += CboProcess_SelectedItemChanged; //공정변할때 lane_qty, coating ver 보여주기 관리
            CboProcess_SelectedItemChanged(null, null);

            //설비
            C1ComboBox[] cboEquipmentParent = { cboEquipmentSegment, cboProcess };
            _combo.SetCombo(cboEquipment, CommonCombo.ComboStatus.ALL, cbParent: cboEquipmentParent);

            //경로흐름
            string[] sFilter = { "FLOWTYPE" };
            _combo.SetCombo(cboFlowType, CommonCombo.ComboStatus.ALL, sCase: "COMMCODE", sFilter: sFilter);

            String[] sFilter3 = { "COAT_SIDE_TYPE" };
            _combo.SetCombo(cboTopBack, CommonCombo.ComboStatus.ALL, sFilter: sFilter3, sCase: "COMMCODE");


            if (LoginInfo.CFG_PROC_ID.Equals(Process.LAMINATION) || LoginInfo.CFG_PROC_ID.Equals(Process.STACKING_FOLDING) || LoginInfo.CFG_PROC_ID.Equals(Process.PACKAGING))
            {
                _AREATYPE = "A";
            }
            else
            {
                _AREATYPE = "E";
            }


        }



        #endregion

        #region Event
        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            //사용자 권한별로 버튼 숨기기
            List<Button> listAuth = new List<Button>();

            Util.pageAuth(listAuth, FrameOperation.AUTHORITY);
            //여기까지 사용자 권한별로 버튼 숨기기
        }

        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            if (ctLotList.IsSelected)
            {
                GetLotList();
            }
            else
            {
                GetWipNote();
            }
        }




        private void CboProcess_SelectedItemChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            if (cboProcess.Items.Count > 0 && cboProcess.SelectedValue != null)
            {
                if (cboProcess.SelectedValue.Equals(Process.COATING)) // E2000
                {
                    cboTopBackTiltle.Visibility = Visibility.Visible;
                    cboTopBack.Visibility = Visibility.Visible;
                }
                else
                {
                    cboTopBackTiltle.Visibility = Visibility.Collapsed;
                    cboTopBack.Visibility = Visibility.Collapsed;
                }


                if (cboProcess.SelectedValue.Equals(Process.LAMINATION) || cboProcess.SelectedValue.Equals(Process.STACKING_FOLDING) || cboProcess.SelectedValue.Equals(Process.PACKAGING))
                {
                    _AREATYPE = "A";
                }
                else
                {
                    _AREATYPE = "E";
                }

            }
        }



        private void txtLotId_KeyDown(object sender, KeyEventArgs e)
        {
            if (ctLotList.IsSelected)
            {
                GetLotList();
            }
            else
            {
                GetWipNote();
            }
        }


        private void olapPage_Updated(object sender, EventArgs e)
        {

            for (int i = 0; i < olapPage.OlapGrid.Columns.Count; i++)
            {
                if (olapPage.OlapGrid.Rows[olapPage.OlapGrid.Rows.Count - 1][i] == null)
                {
                    olapPage.OlapGrid.Columns[i].Visible = false;//값이 없는것들 숨기기
                }
            }

            //셋팅된값 저장하기

            alRowFields.Clear();
            alColumnFields.Clear();
            alValueFields.Clear();

            foreach (C1.Olap.C1OlapField cf in olapPage.OlapEngine.RowFields)
            {
                alRowFields.Add(cf.Name);
            }

            foreach (C1.Olap.C1OlapField cf in olapPage.OlapEngine.ColumnFields)
            {
                alColumnFields.Add(cf.Name);
            }

            foreach (C1.Olap.C1OlapField cf in olapPage.OlapEngine.ValueFields)
            {
                alValueFields.Add(cf.Name);
            }
        }
        #endregion

        #region Mehod
        #region [작업대상 가져오기]
        public void GetLotList()
        {
            try
            {
                DataTable dtRqst = new DataTable();
                dtRqst.Columns.Add("LANGID", typeof(string));
                dtRqst.Columns.Add("EQSGID", typeof(string));
                dtRqst.Columns.Add("PROCID", typeof(string));
                dtRqst.Columns.Add("EQPTID", typeof(string));
                dtRqst.Columns.Add("LOTID", typeof(string));
                dtRqst.Columns.Add("FROM_DATE", typeof(string));
                dtRqst.Columns.Add("TO_DATE", typeof(string));
                dtRqst.Columns.Add("AREATYPE", typeof(string));
                dtRqst.Columns.Add("FLOWTYPE", typeof(string));
                dtRqst.Columns.Add("TOPBACK", typeof(string));

                DataRow dr = dtRqst.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["AREATYPE"] = _AREATYPE;


                if (Util.GetCondition(txtLotId).Equals("")) //lot id 가 없는 경우
                {
                    dr["EQSGID"] = Util.GetCondition(cboEquipmentSegment, "라인을선택하세요.");
                    if (dr["EQSGID"].Equals("")) return;
                    dr["PROCID"] = Util.GetCondition(cboProcess, "공정을선택하세요.");
                    if (dr["PROCID"].Equals("")) return;
                    dr["EQPTID"] = Util.GetCondition(cboEquipment, bAllNull: true);
                    dr["FLOWTYPE"] = Util.GetCondition(cboFlowType, bAllNull: true);
                    dr["TOPBACK"] = Util.GetCondition(cboTopBack, bAllNull: true);
                    dr["FROM_DATE"] = Util.GetCondition(dtpDateFrom);
                    dr["TO_DATE"] = Util.GetCondition(dtpDateTo);


                }
                else //lot id 가 있는경우 다른 조건 모두 무시
                {
                    dr["LOTID"] = Util.GetCondition(txtLotId);

                }

                dtRqst.Rows.Add(dr);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_LOT_LIST_WITH_LOSS", "INDATA", "OUTDATA", dtRqst);

                //Util.gridClear(dgLotList);
                //dgLotList.ItemsSource = DataTableConverter.Convert(dtRslt);

                dtRslt.Columns["PROCNAME"].ColumnName           = "01PROCNAME";
                dtRslt.Columns["EQPTNAME"].ColumnName           = "02EQPTNAME";
                dtRslt.Columns["PRDT_CLSS_NAME"].ColumnName     = "03PRDT_CLSS_NAME";
                dtRslt.Columns["CALDATE"].ColumnName            = "04CALDATE";
                dtRslt.Columns["LOTID"].ColumnName              = "05LOTID";
                dtRslt.Columns["WIPQTY_ED"].ColumnName          = "06WIPQTY_ED";
                dtRslt.Columns["PRODID"].ColumnName             = "07PRODID";
                dtRslt.Columns["PRODNAME"].ColumnName           = "08PRODNAME";
                dtRslt.Columns["MODLID"].ColumnName             = "09MODLID";
                dtRslt.Columns["PRJT_NAME"].ColumnName          = "11PRJT_NAME";
                dtRslt.Columns["PROD_VER_CODE"].ColumnName      = "12PROD_VER_CODE";
                dtRslt.Columns["UNIT_CODE"].ColumnName          = "13UNIT_CODE";
                dtRslt.Columns["CNFM_DFCT_QTY"].ColumnName      = "14CNFM_DFCT_QTY";
                dtRslt.Columns["CNFM_LOSS_QTY"].ColumnName      = "15CNFM_LOSS_QTY";
                dtRslt.Columns["CNFM_PRDT_REQ_QTY"].ColumnName  = "16CNFM_PRDT_REQ_QTY";
                dtRslt.Columns["STARTDTTM"].ColumnName          = "17STARTDTTM";
                dtRslt.Columns["ENDDTTM"].ColumnName            = "18ENDDTTM";
                dtRslt.Columns["SHFT_NAME"].ColumnName          = "19SHFT_NAME";
                dtRslt.Columns["WRK_USER_NAME"].ColumnName      = "20WRK_USER_NAME";
                dtRslt.Columns["EQSGNAME"].ColumnName           = "21EQSGNAME";
                dtRslt.Columns["FLOW_NAME"].ColumnName          = "22FLOW_NAME";
                dtRslt.Columns["WOID"].ColumnName               = "23WOID";
                dtRslt.Columns["WO_DETL_ID"].ColumnName         = "24WO_DETL_ID";
                dtRslt.Columns["ACTID"].ColumnName              = "90ACTID";
                dtRslt.Columns["RESNNAME"].ColumnName           = "91RESNNAME";
                dtRslt.Columns["DFCT_CODE_DETL_NAME"].ColumnName           = "92DFCT_CODE_DETL_NAME";
                dtRslt.Columns["RESNQTY"].ColumnName            = "93RESNQTY";

                olapPage.DataSource = dtRslt.DefaultView;

                ListBox lbColumns = olapPage.FindName("Columns") as ListBox;

                var olap = olapPage.OlapPanel.OlapEngine;


                olap.BeginUpdate();
                olap.Fields.Remove("CHK");
                olap.Fields.Remove("WIPSEQ");
                olap.Fields.Remove("WIPQTY");
                olap.Fields.Remove("WIPQTY2");
                olap.Fields.Remove("WIPHOLD");
                olap.Fields.Remove("WIPSTAT");
                olap.Fields.Remove("WIPQTY2_ED");
                olap.Fields.Remove("CNFM_LOSS_QTY2");
                olap.Fields.Remove("CNFM_PRDT_REQ_QTY2");
                olap.Fields.Remove("CNFM_DFCT_QTY2");
                olap.Fields.Remove("AREAID");
                olap.Fields.Remove("PROCID");
                olap.Fields.Remove("EQSGID");
                olap.Fields.Remove("EQPTID");
                olap.Fields.Remove("LANE_QTY");
                olap.Fields.Remove("LANE_PTN_QTY");
                olap.Fields.Remove("ORG_QTY");
                olap.Fields.Remove("SHIFT");
                olap.Fields.Remove("RESNCODE");

                olap.Fields["01PROCNAME"].Caption = ObjectDic.Instance.GetObjectName("공정");
                olap.Fields["02EQPTNAME"].Caption = ObjectDic.Instance.GetObjectName("설비");
                olap.Fields["03PRDT_CLSS_NAME"].Caption = ObjectDic.Instance.GetObjectName("양/음");
                olap.Fields["04CALDATE"].Caption = ObjectDic.Instance.GetObjectName("작업일");
                olap.Fields["05LOTID"].Caption = ObjectDic.Instance.GetObjectName("LOTID");
                olap.Fields["06WIPQTY_ED"].Caption = ObjectDic.Instance.GetObjectName("양품량");
                olap.Fields["07PRODID"].Caption = ObjectDic.Instance.GetObjectName("제품");
                olap.Fields["08PRODNAME"].Caption = ObjectDic.Instance.GetObjectName("제품명");
                olap.Fields["09MODLID"].Caption = ObjectDic.Instance.GetObjectName("모델");
                olap.Fields["11PRJT_NAME"].Caption = ObjectDic.Instance.GetObjectName("프로젝트명");
                olap.Fields["12PROD_VER_CODE"].Caption = ObjectDic.Instance.GetObjectName("코팅버전");
                olap.Fields["13UNIT_CODE"].Caption = ObjectDic.Instance.GetObjectName("단위");
                olap.Fields["14CNFM_DFCT_QTY"].Caption = ObjectDic.Instance.GetObjectName("불량량");
                olap.Fields["15CNFM_LOSS_QTY"].Caption = ObjectDic.Instance.GetObjectName("로스량");
                olap.Fields["16CNFM_PRDT_REQ_QTY"].Caption = ObjectDic.Instance.GetObjectName("물청량");
                olap.Fields["17STARTDTTM"].Caption = ObjectDic.Instance.GetObjectName("시작시간");
                olap.Fields["18ENDDTTM"].Caption = ObjectDic.Instance.GetObjectName("종료시간");
                olap.Fields["19SHFT_NAME"].Caption = ObjectDic.Instance.GetObjectName("작업조");
                olap.Fields["20WRK_USER_NAME"].Caption = ObjectDic.Instance.GetObjectName("작업자");
                olap.Fields["21EQSGNAME"].Caption = ObjectDic.Instance.GetObjectName("라인");
                olap.Fields["22FLOW_NAME"].Caption = ObjectDic.Instance.GetObjectName("경로유형");
                olap.Fields["23WOID"].Caption = ObjectDic.Instance.GetObjectName("W/O");
                olap.Fields["24WO_DETL_ID"].Caption = ObjectDic.Instance.GetObjectName("W/O상세");
                olap.Fields["90ACTID"].Caption = ObjectDic.Instance.GetObjectName("사유구분");
                olap.Fields["91RESNNAME"].Caption = ObjectDic.Instance.GetObjectName("사유명");
                olap.Fields["92DFCT_CODE_DETL_NAME"].Caption = ObjectDic.Instance.GetObjectName("불량/LOSS/물청명");
                olap.Fields["93RESNQTY"].Caption = ObjectDic.Instance.GetObjectName("불량/LOSS/물청 수량");


                if (alRowFields.Count == 0)
                {
                    olap.RowFields.Add("01PROCNAME");
                    olap.RowFields.Add("02EQPTNAME");
                    olap.RowFields.Add("03PRDT_CLSS_NAME");
                    olap.RowFields.Add("04CALDATE");
                    olap.RowFields.Add("05LOTID");
                    olap.RowFields.Add("06WIPQTY_ED");
                }
                else
                {
                    foreach (string sName in alRowFields)
                    {
                        olap.RowFields.Add(sName);
                    }
                }

                if (alColumnFields.Count == 0)
                {
                    olap.ColumnFields.Add("92DFCT_CODE_DETL_NAME");
                }
                else
                {
                    foreach (string sName in alColumnFields)
                    {
                        olap.ColumnFields.Add(sName);
                    }
                }
                if (alValueFields.Count == 0)
                {
                    olap.ValueFields.Add("93RESNQTY");
                    olap.Fields["93RESNQTY"].Format = "###,###,##0.##";
                }
                else
                {
                    foreach (string sName in alValueFields)
                    {
                        olap.ValueFields.Add(sName);
                        olap.Fields[sName].Format = "###,###,##0.##";
                    }

                }
                
                olap.EndUpdate();

            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }

        #endregion

        #region [작업대상 가져오기]
        public void GetWipNote()
        {
            try
            {
                DataTable dtRqst = new DataTable();
                dtRqst.Columns.Add("LANGID", typeof(string));
                dtRqst.Columns.Add("EQSGID", typeof(string));
                dtRqst.Columns.Add("PROCID", typeof(string));
                dtRqst.Columns.Add("EQPTID", typeof(string));
                dtRqst.Columns.Add("LOTID", typeof(string));
                dtRqst.Columns.Add("FROM_DATE", typeof(string));
                dtRqst.Columns.Add("TO_DATE", typeof(string));
                dtRqst.Columns.Add("AREATYPE", typeof(string));
                dtRqst.Columns.Add("FLOWTYPE", typeof(string));
                dtRqst.Columns.Add("TOPBACK", typeof(string));

                DataRow dr = dtRqst.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["AREATYPE"] = _AREATYPE;


                if (Util.GetCondition(txtLotId).Equals("")) //lot id 가 없는 경우
                {
                    dr["EQSGID"] = Util.GetCondition(cboEquipmentSegment, "라인을선택하세요.");
                    if (dr["EQSGID"].Equals("")) return;
                    dr["PROCID"] = Util.GetCondition(cboProcess, "공정을선택하세요.");
                    if (dr["PROCID"].Equals("")) return;
                    dr["EQPTID"] = Util.GetCondition(cboEquipment, bAllNull: true);
                    dr["FLOWTYPE"] = Util.GetCondition(cboFlowType, bAllNull: true);
                    dr["TOPBACK"] = Util.GetCondition(cboTopBack, bAllNull: true);
                    dr["FROM_DATE"] = Util.GetCondition(dtpDateFrom);
                    dr["TO_DATE"] = Util.GetCondition(dtpDateTo);


                }
                else //lot id 가 있는경우 다른 조건 모두 무시
                {
                    dr["LOTID"] = Util.GetCondition(txtLotId);

                }

                dtRqst.Rows.Add(dr);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_LOT_LIST_WITH_NOTE", "INDATA", "OUTDATA", dtRqst);

                //Util.gridClear(dgWipNote);
                //dgWipNote.ItemsSource = DataTableConverter.Convert(dtRslt);
                Util.GridSetData(dgWipNote, dtRslt, FrameOperation);
                

            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }

        #endregion
        private void SetChgFont(object obj)
        {
            //TextBox tb = obj as TextBox;
            //tb.FontWeight = FontWeights.Bold;
        }

        private void SetChgFontClear(object obj)
        {
            //TextBox tb = obj as TextBox;
            //tb.FontWeight = FontWeights.Normal;
        }


        #endregion

        private void dtpDate_SelectedDataTimeChanged(object sender, SelectionChangedEventArgs e)
        {

            if (dtpDateFrom.SelectedDateTime.Year > 1 && dtpDateTo.SelectedDateTime.Year > 1)
            {
                if ((dtpDateTo.SelectedDateTime - dtpDateFrom.SelectedDateTime).TotalDays > 7)
                {
                    Util.AlertInfo("SFU2042", new object[] { "7" });   //기간은 {0}일 이내 입니다.
                    dtpDateFrom.SelectedDateTime = dtpDateTo.SelectedDateTime.AddDays(-6);
                    return;
                }

                if ((dtpDateTo.SelectedDateTime - dtpDateFrom.SelectedDateTime).TotalDays < 0)
                {
                    dtpDateFrom.SelectedDateTime = dtpDateTo.SelectedDateTime;
                    return;
                }
            }
        }


        
    }
}
