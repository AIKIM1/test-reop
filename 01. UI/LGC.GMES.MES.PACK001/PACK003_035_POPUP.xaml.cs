/*************************************************************************************
 Created Date : 2022.09.21
      Creator : 주동석
   Decription : 사전 자재 요청 팝업
--------------------------------------------------------------------------------------
 [Change History]
     Date         Author      CSR                    Description...
  2022.09.21      주동석 :                           Initial Created.
  2022.10.20      김진수      C20221011-000373       Initial Created.
  2022.10.28      김진수      C20221011-000373       조회 컬럼 추가 및 컬럼 명 수정
  2022.11.24      김진수      C20221011-000373       대체 자재 요청 페이지 추가
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
using System.IO;
using System.Windows.Controls.Primitives;



namespace LGC.GMES.MES.PACK001
{
    public partial class PACK003_035_POPUP : C1Window, IWorkArea
    {
        #region #. Member Variable Lists...
        LGC.GMES.MES.PACK001.Class.MESSAGE_PARAM ms = new LGC.GMES.MES.PACK001.Class.MESSAGE_PARAM();
        Util util = new Util();

        //private int t1_cboSnapAreaCount = 0;
        //private int t1_cboSnapEqsgCount = 0;
        //private int t1_comMtrlPortCount = 0;
        //private int t2_cboSnapAreaCount = 0;
        private int t1_cboSnapEqsgCount = 0;
        private int t1_comMtrlPortCount = 0;
        private int t2_cboSnapEqsgCount = 0;
        private int t2_comMtrlPortCount = 0;
        private int t3_cboSnapEqsgCount = 0;
        private int t3_comMtrlPortCount = 0;
        /// <summary>
        /// Frame과 상호작용하기 위한 객체
        /// </summary>
        public IFrameOperation FrameOperation
        {
            get;
            set;
        }

        private string sEqsgID = string.Empty;
        public string EqsgID
        {
            get
            {
                return sEqsgID;
            }

            set
            {
                sEqsgID = value;
            }
        }
        private string sMtrlPortID = string.Empty;
        public string MtrlPortID
        {
            get
            {
                return sMtrlPortID;
            }

            set
            {
                sMtrlPortID = value;
            }
        }

        public DataTable initTableGrdMainTp1()
        {
            DataTable dt = new DataTable();
            dt.Columns.Add("CHK", typeof(string));
            dt.Columns.Add("AREANAME", typeof(string));
            dt.Columns.Add("EQSGNAME", typeof(string));
            dt.Columns.Add("MTRL_PORT_ID", typeof(string));
            dt.Columns.Add("MTRLID", typeof(string));
            dt.Columns.Add("REPACK_WH_ID", typeof(string));
            dt.Columns.Add("ONHAND", typeof(string));
            dt.Columns.Add("COUNT", typeof(string));
            dt.Columns.Add("KEP_BOX_QTY", typeof(string));
            dt.Columns.Add("CATN_BOX_QTY", typeof(string));
            dt.Columns.Add("DNGR_BOX_QTY", typeof(string));
            dt.Columns.Add("DIHAND", typeof(string));
            return dt;
        }
        public DataTable initTableGrdMainTp2()
        {
            DataTable dt = new DataTable();
            //dt.Columns.Add("CHK", typeof(string));DNGR_BOX_QTY
            dt.Columns.Add("AREANAME", typeof(string));
            dt.Columns.Add("EQSGNAME", typeof(string));
            dt.Columns.Add("MTRL_PORT_ID", typeof(string));
            dt.Columns.Add("MTRLID", typeof(string));
            dt.Columns.Add("REPACK_WH_ID", typeof(string));
            dt.Columns.Add("ON_HAND", typeof(string));
            dt.Columns.Add("REQ", typeof(string));
            dt.Columns.Add("DEL", typeof(string));
            return dt;
        }
        public DataTable initTableGrdMainTp3()
        {
            DataTable dt = new DataTable();
            dt.Columns.Add("CHK", typeof(string));
            dt.Columns.Add("AREANAME", typeof(string));
            dt.Columns.Add("EQSGNAME", typeof(string));
            dt.Columns.Add("MTRL_PORT_ID", typeof(string));
            dt.Columns.Add("MTRLID", typeof(string));
            dt.Columns.Add("REPACK_WH_ID", typeof(string));
            dt.Columns.Add("ONHAND", typeof(string));
            dt.Columns.Add("COUNT", typeof(string));
            dt.Columns.Add("KEP_BOX_QTY", typeof(string));
            dt.Columns.Add("CATN_BOX_QTY", typeof(string));
            dt.Columns.Add("DNGR_BOX_QTY", typeof(string));
            dt.Columns.Add("ALL_REQ", typeof(string));
            dt.Columns.Add("DIHAND", typeof(string));
            return dt;
        }
        #endregion

        #region #. Declaration & Constructor
        public PACK003_035_POPUP()
        {
            InitializeComponent();
        }
        private void C1Window_Initialized(object sender, EventArgs e)
        {

            

        }
        private void C1Window_Loaded(object sender, RoutedEventArgs e)
        {
            //파라메터 등록
            object[] tmps = C1WindowExtension.GetParameters(this);
            EqsgID = (string)tmps[0];
            MtrlPortID = (string)tmps[1];

            iniCombo();
            
        }
        
        private void iniCombo()
        {
            SetAreaByShopId(t1_cboSnapArea);
            
            PackCommon.SetMultiSelectionComboBox(SetCboEQSG_CBO(), this.t1_cboSnapEqsg, ref this.t1_cboSnapEqsgCount, EqsgID);
            PackCommon.SetMultiSelectionComboBox(SetCboMTRLPORT_CBO(EqsgID), this.t1_comMtrlPort, ref this.t1_comMtrlPortCount, MtrlPortID);
            
            SetAreaByShopId(t2_cboSnapArea);
            PackCommon.SetMultiSelectionComboBox(SetCboEQSG_CBO(), this.t2_cboSnapEqsg, ref this.t2_cboSnapEqsgCount);
            PackCommon.SetMultiSelectionComboBox(SetCboMTRLPORT_CBO(Convert.ToString(this.t2_cboSnapEqsg.SelectedItemsToString)), this.t2_comMtrlPort, ref this.t2_comMtrlPortCount);
            
            SetAreaByShopId(t3_cboSnapArea);
            PackCommon.SetMultiSelectionComboBox(SetCboEQSG_CBO(), this.t3_cboSnapEqsg, ref this.t3_cboSnapEqsgCount, EqsgID);
            PackCommon.SetMultiSelectionComboBox(SetCboMTRLPORT_CBO(EqsgID), this.t3_comMtrlPort, ref this.t3_comMtrlPortCount, MtrlPortID);


            SearchGrdMainTp3();
            SearchGrdMainTp1();
        }
        #endregion

        #region #. Event Lists...

        #region t1 page

        #region 동 콤보(미사용)
        //private void cboSnapArea_SelectedValueChanged_T1(object sender, EventArgs e)
        //{
        //    try
        //    {
        //        this.Dispatcher.BeginInvoke(new System.Action(() =>
        //        {
        //            SetCboEQSG(t1_cboSnapEqsg, t1_cboSnapArea);
        //        }));
        //    }
        //    catch (Exception ex)
        //    {
        //        Util.MessageException(ex);
        //    }
        //}
        #endregion

        #region <라인 콤보(미사용)
        private void t1_cboSnapEqsg_SelectionChanged(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(Convert.ToString(this.t1_cboSnapEqsg.SelectedItemsToString)))
            {
                t1_comMtrlPort.ItemsSource = null;
                return;
            }
            string strT1_cboSnapEqsg = Convert.ToString(this.t1_cboSnapEqsg.SelectedItemsToString) ?? null;
            PackCommon.SetMultiSelectionComboBox(SetCboMTRLPORT_CBO(strT1_cboSnapEqsg), this.t1_comMtrlPort, ref this.t1_comMtrlPortCount);
        }
        #endregion

        #region CheckBox
        private void CheckBox_Click(object sender, RoutedEventArgs e)
        {

        }
        #endregion

        #region 조회 버튼
        private void t1_btnSearch_Click(object sender, RoutedEventArgs e)
        {
            EqsgID = Convert.ToString(this.t1_cboSnapEqsg.SelectedItemsToString);
            MtrlPortID = Convert.ToString(this.t1_comMtrlPort.SelectedItemsToString);
            SearchGrdMainTp1();
            if (grdMainTp1.Rows.Count <= 0)
            {
                Util.MessageValidation("SFU1886"); // 정보가 없습니다.
            }
        }
        #endregion

        #region 요청 버튼
        private void t1_btnRequest_Click(object sender, RoutedEventArgs e)
        {
            //Validation
            if (!this.ValidationCheck("Request"))
            {
                return;
            }

            DataTable dt = DataTableConverter.Convert(this.grdMainTp1.ItemsSource).AsEnumerable().Where(x => x.Field<string>("CHK") == "True").CopyToDataTable();
            LGC.GMES.MES.ControlsLibrary.MessageBox.Show(ms.AlertRetun("SFU2086"), null, "Information", MessageBoxButton.OKCancel, MessageBoxIcon.None, (sResult) =>
            //{0} 판정 하시겠습니까 ? 
            {
                if (sResult == MessageBoxResult.OK)
                {
                    MtrlRequestTp1(dt);
                    SearchGrdMainTp1(); //grid refresh 
                }

            });

        }
        #endregion

        #region 자재 정보 조회
        private void t1_grdDetail_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            C1.WPF.DataGrid.C1DataGrid datagrid = (sender as C1.WPF.DataGrid.C1DataGrid);

            Point pnt = e.GetPosition(null);
            C1.WPF.DataGrid.DataGridCell cell = datagrid.GetCellFromPoint(pnt);

            if (cell == null || datagrid.CurrentRow == null)
            {
                return;
            }

            if (cell.Column.Name == "MTRLID")
            {
                ShowPopup(Util.NVC(grdMainTp1.GetCell(datagrid.CurrentRow.Index, grdMainTp1.Columns["MTRL_PORT_ID"].Index).Value), Util.NVC(grdMainTp1.GetCell(datagrid.CurrentRow.Index, grdMainTp1.Columns["MTRLID"].Index).Value),"t1");
            }

        }
        #endregion

        #endregion

        #region t2 page

        #region < 라인 콤보 이벤트 >
        private void t2_cboSnapEqsg_Chamged(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(Convert.ToString(this.t2_cboSnapEqsg.SelectedItemsToString)))
            {
                t2_comMtrlPort.ItemsSource = null;
                return;
            }

            PackCommon.SetMultiSelectionComboBox(SetCboMTRLPORT_CBO(Convert.ToString(this.t2_cboSnapEqsg.SelectedItemsToString)), this.t2_comMtrlPort, ref this.t2_comMtrlPortCount);
        }
        #endregion

        #region 조회 버튼
        private void t2_btnSearch_Click(object sender, RoutedEventArgs e)
        {
            if (!this.ValidationCheck("REV"))
            {
                return;
            }
            SearchGrdMainTp2();
        }
        #endregion

        #endregion

        #region t3 page

        #region <라인 콤보(미사용)
        private void t3_cboSnapEqsg_SelectionChanged(object sender, EventArgs e)
        {

        }
        #endregion

        #region 조회 버튼
        private void t3_btnSearch_Click(object sender, RoutedEventArgs e)
        {
            SearchGrdMainTp3();
            if (grdMainTp3.Rows.Count <= 0)
            {
                Util.MessageValidation("SFU1886"); // 정보가 없습니다.
            }
        }
        #endregion

        #region 요청 버튼
        private void t3_btnRequest_Click(object sender, RoutedEventArgs e)
        {
            //Validation
            if (!this.ValidationCheck("ALTRequest"))
            {
                return;
            }

            DataTable dt = DataTableConverter.Convert(this.grdMainTp3.ItemsSource).AsEnumerable().Where(x => x.Field<string>("CHK") == "True").CopyToDataTable();
            LGC.GMES.MES.ControlsLibrary.MessageBox.Show(ms.AlertRetun("SFU2086"), null, "Information", MessageBoxButton.OKCancel, MessageBoxIcon.None, (sResult) =>
            //{0} 판정 하시겠습니까 ? 
            {
                if (sResult == MessageBoxResult.OK)
                {
                    MtrlRequestTp3(dt);
                    SearchGrdMainTp3(); //grid refresh 
                }

            });
        }
        #endregion

        #region 자재 정보 조회
        private void grdMainTp3_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            C1.WPF.DataGrid.C1DataGrid datagrid = (sender as C1.WPF.DataGrid.C1DataGrid);

            Point pnt = e.GetPosition(null);
            C1.WPF.DataGrid.DataGridCell cell = datagrid.GetCellFromPoint(pnt);

            if (cell == null || datagrid.CurrentRow == null)
            {
                return;
            }

            if (cell.Column.Name == "MTRLID")
            {
                ShowPopup(Util.NVC(grdMainTp3.GetCell(datagrid.CurrentRow.Index, grdMainTp3.Columns["MTRL_PORT_ID"].Index).Value), Util.NVC(grdMainTp3.GetCell(datagrid.CurrentRow.Index, grdMainTp3.Columns["MTRLID"].Index).Value), "t3");
            }
        }
        #endregion

        #endregion

        #endregion

        #region #. Member Function Lists...

        #region 요청 버튼 Validation
        private bool ValidationCheck(string strType)
        {
            bool returnValue = true;
            if (strType.Equals("Request"))
            {

                DataTable dt = DataTableConverter.Convert(this.grdMainTp1.ItemsSource);
                if (!CommonVerify.HasTableRow(dt))
                {
                    Util.Alert("10008");  // 선택된 데이터가 없습니다.
                    //this.grdMainTp1.Focus();
                    return false;
                }

                // Validation Check
                var queryValidationCheck = dt.AsEnumerable().Where(x => x.Field<string>("CHK") == "True");

                if (queryValidationCheck.Count() <= 0)
                {
                    Util.Alert("10008");  // 선택된 데이터가 없습니다.
                    //this.grdMainTp1.Focus();
                    return false;
                }
                var cntValidationCheck = dt.AsEnumerable().Where(x => x.Field<string>("CHK") == "True" && x.Field<string>("COUNT") == "0");

                if (cntValidationCheck.Count() > 0)
                {
                    //Util.MessageValidation("SFU4925", ObjectDic.Instance.GetObjectName("요청 수량")); // %1(을)를 선택하세요.
                    Util.Alert("10008", "요청 수량");  // 선택된 데이터가 없습니다.
                    //this.grdMainTp1.Focus();
                    return false;
                }

            }
            else if (strType.Equals("REV"))
            {
                if (string.IsNullOrEmpty(Convert.ToString(this.t2_cboSnapEqsg.SelectedItemsToString)))
                {
                    Util.MessageValidation("SFU4925", ObjectDic.Instance.GetObjectName("라인")); // %1(을)를 선택하세요.
                    return false;
                }
                if (string.IsNullOrEmpty(Convert.ToString(this.t2_comMtrlPort.SelectedItemsToString)))
                {
                    Util.MessageValidation("SFU4925", ObjectDic.Instance.GetObjectName("포트")); // %1(을)를 선택하세요.
                    return false;
                }
            }
            else if (strType.Equals("ALTRequest"))
            {
                DataTable dt = DataTableConverter.Convert(this.grdMainTp3.ItemsSource);
                if (!CommonVerify.HasTableRow(dt))
                {
                    Util.Alert("10008");  // 선택된 데이터가 없습니다.
                    //this.grdMainTp1.Focus();
                    return false;
                }

                // Validation Check
                var queryValidationCheck = dt.AsEnumerable().Where(x => x.Field<string>("CHK") == "True");

                if (queryValidationCheck.Count() <= 0)
                {
                    Util.Alert("10008");  // 선택된 데이터가 없습니다.
                    //this.grdMainTp1.Focus();
                    return false;
                }
                var cntValidationCheck = dt.AsEnumerable().Where(x => x.Field<string>("CHK") == "True" && x.Field<string>("COUNT") == "0");

                if (cntValidationCheck.Count() > 0)
                {
                    //Util.MessageValidation("SFU4925", ObjectDic.Instance.GetObjectName("요청 수량")); // %1(을)를 선택하세요.
                    Util.Alert("10008", "요청 수량");  // 선택된 데이터가 없습니다.
                    //this.grdMainTp1.Focus();
                    return false;
                }
            }
            //return false;
            return returnValue;
        }
        #endregion
        
        #region t1 NumericColumn
        private void grdMainTp1_BeginningRowEdit(object sender, DataGridEditingRowEventArgs e)
        {
            if (((DataGridNumericColumn)grdMainTp1.CurrentCell.Column).ActualFilterMemberPath == "COUNT")
            {
                ((DataGridNumericColumn)grdMainTp1.CurrentCell.Column).Minimum = 1;
                ((DataGridNumericColumn)grdMainTp1.CurrentCell.Column).Maximum = double.Parse(grdMainTp1.GetDataRow().ItemArray[11].ToString());
            }
        }
        #endregion

        #region t3 NumericColumn
        private void grdMainTp3_BeginningRowEdit(object sender, DataGridEditingRowEventArgs e)
        {
            if (((DataGridNumericColumn)grdMainTp3.CurrentCell.Column).ActualFilterMemberPath == "COUNT")
            {
                ((DataGridNumericColumn)grdMainTp3.CurrentCell.Column).Minimum = 0;
                ((DataGridNumericColumn)grdMainTp3.CurrentCell.Column).Maximum = (double.Parse(grdMainTp3.GetDataRow().ItemArray[12].ToString()) > 0 ? double.Parse(grdMainTp3.GetDataRow().ItemArray[12].ToString()) : 0);
            }
        }
        #endregion

        #region #. Alert Popup

        //private void grdDetial_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
        //{
        //    HidePopUp();
        //}


        //private void grdDetail_CurrentCellChanged(object sender, DataGridCellEventArgs e)
        //{

        //}

        private void btnHideConfirm_Click(object sender, RoutedEventArgs e)
        {
            this.popupAlert.IsOpen = false;
            this.popupAlert.HorizontalOffset = 0;
            this.popupAlert.VerticalOffset = 0;
        }
        private void t3_btnHideConfirm_Click(object sender, RoutedEventArgs e)
        {
            this.t3_popupAlert.IsOpen = false;
            this.t3_popupAlert.HorizontalOffset = 0;
            this.t3_popupAlert.VerticalOffset = 0;
        }

        private void Image_MouseLeftButtonUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            this.HidePopUp();
        }

        private void t3_Image_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {

            this.t3_popupAlert.IsOpen = false;
            this.t3_popupAlert.HorizontalOffset = 0;
            this.t3_popupAlert.VerticalOffset = 0;
        }
        // Popup - Close Popup
        private void HidePopUp()
        {
            this.popupAlert.IsOpen = false;
            this.popupAlert.HorizontalOffset = 0;
            this.popupAlert.VerticalOffset = 0;
        }

        // Popup - Show Popup

        #endregion
            
        #region Biz

        #region < 동 정보 콤보 호출 >
        /// <summary>
        /// CWA 경우 DB 분리로 인해서, 자신의 동만 보이도록 처리,
        /// 오창의 경우, DB 분리가 되지 않아서, 모든 곳을 검색할수 있도록 콤보 선택 처리 
        /// </summary>
        private void SetAreaByShopId(C1ComboBox cb)
        {
            try
            {

                DataTable dt = new DataTable();
                dt.TableName = "RQSTDT";
                dt.Columns.Add("CBO_CODE", typeof(string));
                dt.Columns.Add("CBO_NAME", typeof(string));

                DataRow dtDr = dt.NewRow();
                dtDr["CBO_CODE"] = LoginInfo.CFG_AREA_ID;
                dtDr["CBO_NAME"] = LoginInfo.CFG_AREA_NAME;
                dt.Rows.Add(dtDr);

                cb.ItemsSource = DataTableConverter.Convert(dt);
                cb.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        #endregion
        
        #region < 라인 콤보 생성 >
        private DataTable SetCboEQSG_CBO()
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("AREAID", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["AREAID"] = LoginInfo.CFG_AREA_ID;// cboArea.SelectedValue ?? LoginInfo.CFG_AREA_ID;

                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_MTRL_SEL_EQUIPMENTSEGMENT_MTRL_PORT_CBO", "RQSTDT", "RSLTDT", RQSTDT);
                return dtResult;


            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return null;
            }
        }
        
        #endregion

        #region < 포트 콤보 생성 >
        private DataTable SetCboMTRLPORT_CBO(string strEqsgID)
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("AREAID", typeof(string));
                RQSTDT.Columns.Add("EQSGID", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["AREAID"] = LoginInfo.CFG_AREA_ID ?? null;
                dr["EQSGID"] = strEqsgID ?? null;

                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_MTRL_SEL_MTRL_PORT_CBO", "RQSTDT", "RSLTDT", RQSTDT);
                return dtResult;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return null;
            }
        }
        
        #endregion

        #region < 자재 리스트 조회 >
        private void SearchGrdMainTp1()
        {
            Util.gridClear(grdMainTp1);

            try
            {
                string bizRuleName = "DA_MTRL_SEL_WO_EXCT_MTRL_LIST";
                DataTable dtRQSTDT = new DataTable("RQSTDT");
                DataTable dtRSLTDT = new DataTable("RSLTDT");

                dtRQSTDT.Columns.Add("LANGID", typeof(string));
                dtRQSTDT.Columns.Add("EQSGID", typeof(string));
                dtRQSTDT.Columns.Add("PORT_ID", typeof(string));

                DataRow drRQSTDT = dtRQSTDT.NewRow();
                drRQSTDT["LANGID"] = LoginInfo.LANGID;
                drRQSTDT["EQSGID"] = EqsgID ?? null;
                drRQSTDT["PORT_ID"] = MtrlPortID ?? null;
                dtRQSTDT.Rows.Add(drRQSTDT);

                dtRSLTDT = new ClientProxy().ExecuteServiceSync(bizRuleName, dtRQSTDT.TableName, dtRSLTDT.TableName, dtRQSTDT);
                if (CommonVerify.HasTableRow(dtRSLTDT))
                {
                    //Grid 생성
                    DataTable dtData = initTableGrdMainTp1();
                    dtData.AcceptChanges();
                    for (int i = 0; i < dtRSLTDT.Rows.Count; i++)
                    {
                        DataRow newRow = null;
                        newRow = dtData.NewRow();
                        newRow["CHK"] = dtRSLTDT.Rows[i]["CHK"]; ;
                        newRow["AREANAME"] = dtRSLTDT.Rows[i]["AREANAME"];
                        newRow["EQSGNAME"] = dtRSLTDT.Rows[i]["EQSGNAME"];
                        newRow["MTRL_PORT_ID"] = dtRSLTDT.Rows[i]["MTRL_PORT_ID"];
                        newRow["MTRLID"] = dtRSLTDT.Rows[i]["MTRLID"];
                        newRow["REPACK_WH_ID"] = dtRSLTDT.Rows[i]["REPACK_WH_ID"];
                        newRow["ONHAND"] = dtRSLTDT.Rows[i]["ONHAND"];
                        newRow["COUNT"] = dtRSLTDT.Rows[i]["DIHAND"];
                        newRow["KEP_BOX_QTY"] = dtRSLTDT.Rows[i]["KEP_BOX_QTY"];
                        newRow["CATN_BOX_QTY"] = dtRSLTDT.Rows[i]["KEP_BOX_QTY"];
                        newRow["DNGR_BOX_QTY"] = dtRSLTDT.Rows[i]["KEP_BOX_QTY"];
                        newRow["DIHAND"] = dtRSLTDT.Rows[i]["DIHAND"];
                        dtData.Rows.Add(newRow);
                    }
                    DataView dvData = dtData.DefaultView;
                    Util.GridSetData(this.grdMainTp1, dtData, FrameOperation);
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        #endregion

        #region < 자재 요청 >
        private void MtrlRequestTp1(DataTable dt)
        {
            try
            {
                string bizRuleName = "BR_MTRL_INS_RACK_MTRL_BOX_STCK_MULT";
                DataTable dtIN_DATA = new DataTable("INDATA");

                dtIN_DATA.Columns.Add("SRCTYPE", typeof(string));
                dtIN_DATA.Columns.Add("LANGID", typeof(string));
                dtIN_DATA.Columns.Add("MTRL_PORT_ID", typeof(string));
                dtIN_DATA.Columns.Add("MTRLID", typeof(string));
                dtIN_DATA.Columns.Add("COUNT", typeof(string));
                dtIN_DATA.Columns.Add("USERID", typeof(string));

                foreach (DataRowView drv in dt.AsDataView())
                {
                    DataRow drIN_DATA = dtIN_DATA.NewRow();
                    drIN_DATA["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                    drIN_DATA["LANGID"] = LoginInfo.LANGID;
                    drIN_DATA["MTRL_PORT_ID"] = drv["MTRL_PORT_ID"].ToString();
                    drIN_DATA["MTRLID"] = drv["MTRLID"].ToString();
                    drIN_DATA["COUNT"] = drv["COUNT"].ToString();
                    drIN_DATA["USERID"] = LoginInfo.USERID;
                    dtIN_DATA.Rows.Add(drIN_DATA);
                }

                DataTable dtResult = new ClientProxy().ExecuteServiceSync(bizRuleName, dtIN_DATA.TableName, null, dtIN_DATA);

                Util.AlertInfo("SFU1275"); // 처리 완료
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                //throw ex;
            }
        }
        #endregion

        #region 자재 MMD 세팅 정보 조회
        private void ShowPopup(string sPortid, string sMtrlid, string sTType)
        {
            try
            {
                this.popupAlert.Tag = null;
                this.popupAlert.Tag = null;

                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("MTRL_PORT_ID", typeof(string));
                RQSTDT.Columns.Add("MTRLID", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["MTRL_PORT_ID"] = sPortid;
                dr["MTRLID"] = sMtrlid;

                RQSTDT.Rows.Add(dr);

                DataRow drData = new ClientProxy().ExecuteServiceSync("DA_MTRL_SEL_MTRL_PORT_DETL_INFO", "RQSTDT", "RSLTDT", RQSTDT).Rows[0];

                if (sTType.Equals("t1"))
                {
                    txtMessageAlert1.Text = drData["MTRL_PORT_ID"].ToString();
                    txtMessageAlert2.Text = drData["MTRLNAME"].ToString();
                    txtMessageAlert3.Text = drData["REPACK_WH_NM"].ToString();
                    txtMessageAlert4.Text = drData["KEP_BOX_QTY"].ToString();
                    txtMessageAlert5.Text = drData["CATN_BOX_QTY"].ToString();
                    txtMessageAlert6.Text = drData["DNGR_BOX_QTY"].ToString();
                    txtMessageAlert7.Text = drData["NOTE"].ToString();

                    this.popupAlert.Placement = System.Windows.Controls.Primitives.PlacementMode.Center;
                    this.popupAlert.IsOpen = true;
                }else if (sTType.Equals("t3"))
                {
                    t3_txtMessageAlert1.Text = drData["MTRL_PORT_ID"].ToString();
                    t3_txtMessageAlert2.Text = drData["MTRLNAME"].ToString();
                    t3_txtMessageAlert3.Text = drData["REPACK_WH_NM"].ToString();
                    t3_txtMessageAlert4.Text = drData["KEP_BOX_QTY"].ToString();
                    t3_txtMessageAlert5.Text = drData["CATN_BOX_QTY"].ToString();
                    t3_txtMessageAlert6.Text = drData["DNGR_BOX_QTY"].ToString();
                    t3_txtMessageAlert7.Text = drData["NOTE"].ToString();

                    this.t3_popupAlert.Placement = System.Windows.Controls.Primitives.PlacementMode.Center;
                    this.t3_popupAlert.IsOpen = true;
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        #endregion

        #region < 자재 요청 정보 조회 >
        private void SearchGrdMainTp2()
        {
            Util.gridClear(grdMainTp2);

            try
            {
                string bizRuleName = "DA_MTRL_SEL_WO_EXCT_MTRL_RSV_LIST";
                DataTable dtRQSTDT = new DataTable("RQSTDT");
                DataTable dtRSLTDT = new DataTable("RSLTDT");

                dtRQSTDT.Columns.Add("LANGID", typeof(string));
                dtRQSTDT.Columns.Add("EQSGID", typeof(string));
                dtRQSTDT.Columns.Add("PORT_ID", typeof(string));

                DataRow drRQSTDT = dtRQSTDT.NewRow();
                drRQSTDT["LANGID"] = LoginInfo.LANGID;
                drRQSTDT["EQSGID"] = Convert.ToString(this.t2_cboSnapEqsg.SelectedItemsToString) ?? null;
                drRQSTDT["PORT_ID"] = Convert.ToString(this.t2_comMtrlPort.SelectedItemsToString) ?? null;
                dtRQSTDT.Rows.Add(drRQSTDT);

                dtRSLTDT = new ClientProxy().ExecuteServiceSync(bizRuleName, dtRQSTDT.TableName, dtRSLTDT.TableName, dtRQSTDT);
                if (CommonVerify.HasTableRow(dtRSLTDT))
                {
                    //Grid 생성
                    DataTable dtData = initTableGrdMainTp2();
                    dtData.AcceptChanges();
                    for (int i = 0; i < dtRSLTDT.Rows.Count; i++)
                    {
                        DataRow newRow = null;
                        newRow = dtData.NewRow();
                        //newRow["CHK"] = dtRSLTDT.Rows[i]["CHK"];
                        newRow["AREANAME"] = dtRSLTDT.Rows[i]["AREANAME"];
                        newRow["EQSGNAME"] = dtRSLTDT.Rows[i]["EQSGNAME"];
                        newRow["MTRL_PORT_ID"] = dtRSLTDT.Rows[i]["MTRL_PORT_ID"];
                        newRow["MTRLID"] = dtRSLTDT.Rows[i]["MTRLID"];
                        newRow["REPACK_WH_ID"] = dtRSLTDT.Rows[i]["REPACK_WH_ID"];
                        newRow["ON_HAND"] = dtRSLTDT.Rows[i]["ON_HAND"];
                        newRow["REQ"] = dtRSLTDT.Rows[i]["REQ"];
                        newRow["DEL"] = dtRSLTDT.Rows[i]["DEL"];
                        dtData.Rows.Add(newRow);
                    }
                    DataView dvData = dtData.DefaultView;
                    //PackCommon.SearchRowCount(ref this.tbGrdMainTp2Cnt, dtData.Rows.Count);
                    Util.GridSetData(this.grdMainTp2, dtData, FrameOperation);
                }
                else
                {
                    Util.MessageValidation("SFU1886"); // 정보가 없습니다.
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }



        #endregion

        #region < 대체 자재 리스트 조회 >
        private void SearchGrdMainTp3()
        {
            Util.gridClear(grdMainTp3);

            try
            {
                string bizRuleName = "DA_MTRL_SEL_ALT_MTRL_LIST";
                DataTable dtRQSTDT = new DataTable("RQSTDT");
                DataTable dtRSLTDT = new DataTable("RSLTDT");

                dtRQSTDT.Columns.Add("LANGID", typeof(string));
                dtRQSTDT.Columns.Add("EQSGID", typeof(string));
                dtRQSTDT.Columns.Add("PORT_ID", typeof(string));

                DataRow drRQSTDT = dtRQSTDT.NewRow();
                drRQSTDT["LANGID"] = LoginInfo.LANGID;
                drRQSTDT["EQSGID"] = EqsgID ?? null;
                drRQSTDT["PORT_ID"] = MtrlPortID ?? null;
                dtRQSTDT.Rows.Add(drRQSTDT);

                dtRSLTDT = new ClientProxy().ExecuteServiceSync(bizRuleName, dtRQSTDT.TableName, dtRSLTDT.TableName, dtRQSTDT);
                if (CommonVerify.HasTableRow(dtRSLTDT))
                {
                    //Grid 생성
                    DataTable dtData = initTableGrdMainTp3();
                    dtData.AcceptChanges();
                    for (int i = 0; i < dtRSLTDT.Rows.Count; i++)
                    {
                        DataRow newRow = null;
                        newRow = dtData.NewRow();
                        newRow["CHK"] = dtRSLTDT.Rows[i]["CHK"]; ;
                        newRow["AREANAME"] = dtRSLTDT.Rows[i]["AREANAME"];
                        newRow["EQSGNAME"] = dtRSLTDT.Rows[i]["EQSGNAME"];
                        newRow["MTRL_PORT_ID"] = dtRSLTDT.Rows[i]["MTRL_PORT_ID"];
                        newRow["MTRLID"] = dtRSLTDT.Rows[i]["MTRLID"];
                        newRow["REPACK_WH_ID"] = dtRSLTDT.Rows[i]["REPACK_WH_ID"];
                        newRow["ONHAND"] = dtRSLTDT.Rows[i]["ONHAND"];
                        newRow["COUNT"] = Int32.Parse( dtRSLTDT.Rows[i]["DIHAND"].ToString() ) > 0 ? 1 : 0;
                        newRow["KEP_BOX_QTY"] = dtRSLTDT.Rows[i]["KEP_BOX_QTY"];
                        newRow["CATN_BOX_QTY"] = dtRSLTDT.Rows[i]["KEP_BOX_QTY"];
                        newRow["DNGR_BOX_QTY"] = dtRSLTDT.Rows[i]["KEP_BOX_QTY"];
                        newRow["ALL_REQ"] = dtRSLTDT.Rows[i]["ALL_REQ"];
                        newRow["DIHAND"] = dtRSLTDT.Rows[i]["DIHAND"];
                        dtData.Rows.Add(newRow);
                    }
                    DataView dvData = dtData.DefaultView;
                    Util.GridSetData(this.grdMainTp3, dtData, FrameOperation);
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        #endregion

        #region < 대체 자재 요청 >
        private void MtrlRequestTp3(DataTable dt)
        {
            try
            {
                string bizRuleName = "BR_MTRL_INS_RACK_MTRL_BOX_STCK_MULT";
                DataTable dtIN_DATA = new DataTable("INDATA");

                dtIN_DATA.Columns.Add("SRCTYPE", typeof(string));
                dtIN_DATA.Columns.Add("LANGID", typeof(string));
                dtIN_DATA.Columns.Add("MTRL_PORT_ID", typeof(string));
                dtIN_DATA.Columns.Add("MTRLID", typeof(string));
                dtIN_DATA.Columns.Add("COUNT", typeof(string));
                dtIN_DATA.Columns.Add("USERID", typeof(string));
                dtIN_DATA.Columns.Add("VAL_TYPE", typeof(string));

                foreach (DataRowView drv in dt.AsDataView())
                {
                    DataRow drIN_DATA = dtIN_DATA.NewRow();
                    drIN_DATA["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                    drIN_DATA["LANGID"] = LoginInfo.LANGID;
                    drIN_DATA["MTRL_PORT_ID"] = drv["MTRL_PORT_ID"].ToString();
                    drIN_DATA["MTRLID"] = drv["MTRLID"].ToString();
                    drIN_DATA["COUNT"] = drv["COUNT"].ToString();
                    drIN_DATA["USERID"] = LoginInfo.USERID;
                    drIN_DATA["VAL_TYPE"] = "ALT_MULT";
                    dtIN_DATA.Rows.Add(drIN_DATA);
                }

                DataTable dtResult = new ClientProxy().ExecuteServiceSync(bizRuleName, dtIN_DATA.TableName, null, dtIN_DATA);

                Util.AlertInfo("SFU1275"); // 처리 완료

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                //throw ex;
            }
        }

        #endregion

        #endregion

        #endregion


    }
}