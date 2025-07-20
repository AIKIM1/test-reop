/*************************************************************************************
 Created Date : 2022.01.13
      Creator : 이정미
   Decription : Aging 출고 Station 공정 관리
--------------------------------------------------------------------------------------
 [Change History]
  2022.01.13 이정미 : Initial Created.
  2022.11.10 이정미 : 콤보박스 선택시 자동 조회되지 않도록 수정 
  2022.11.11 이정미 : Plus Button 오류 수정 
  2022.11.22 이정미 : Plus Button 클릭 시 Column 0일 경우 예외 처리
  2024.04.12 이준영 : E20240411-001004 ESNB PI Aging 출고 Station 공정 기능 변경 
  2025.07.07 이정미 : 체크박스 미선택 상태에서 저장버튼 클릭 시 저장 완료 팝업 노출되는 오류 수정
**************************************************************************************/
using C1.WPF;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using System;
using System.Data;
using System.Windows;
using System.Windows.Controls;


namespace LGC.GMES.MES.FCS001
{
    /// <summary>
    /// TSK_120.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class FCS001_126 : UserControl, IWorkArea
    {
        #region Declaration & Constructor 

        Util _Util = new Util();

        private object oPortid;

        public FCS001_126()
        {
            InitializeComponent();
        }
        #endregion

        #region Initialize
        /// <summary>
        /// Frame과 상호작용하기 위한 객체
        /// </summary>
        public IFrameOperation FrameOperation
        {
            get;
            set;
        }

        /// <summary>
        /// 화면 내 ComboBox Setting
        /// </summary>
        private void InitCombo()
        {
            CommonCombo_Form _combo = new CommonCombo_Form();
            C1ComboBox[] cboCraneChild = { cboPort };
            _combo.SetCombo(cboCrane, CommonCombo_Form.ComboStatus.NONE, sCase: "SCEQPID", cbChild: cboCraneChild);
        }
        #endregion

        #region Event
        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            InitCombo();
            this.Loaded -= UserControl_Loaded;
        }

        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            GetList();
        }

        private void cboCrane_SelectionCommitted(object sender, PropertyChangedEventArgs<object> e)
        {
            CommonCombo_Form _combo = new CommonCombo_Form();
            C1ComboBox[] cboPortParent = { cboCrane };
            _combo.SetCombo(cboPort, CommonCombo_Form.ComboStatus.NONE, sCase: "CNV_OUTPUT", cbParent: cboPortParent);
        }

        private void cboPort_SelectionCommitted(object sender, PropertyChangedEventArgs<object> e)
        {
            GetList();
        }

        private void btnUnitPlus_Click(object sender, RoutedEventArgs e)
        {
            DataGridRowAdd(dgOutStationList, 1);

            if (string.IsNullOrEmpty(Util.GetCondition(cboPort)).Equals(true)) return;

            if (dgOutStationList.Rows.Count > 0)
            {
               
                DataTableConverter.SetValue(dgOutStationList.Rows[dgOutStationList.Rows.Count - 1].DataItem, "FLAG", "Y");
            }
        }

        private void btnUnitMinus_Click(object sender, RoutedEventArgs e)
        {
            if (dgOutStationList.Rows.Count > 0)
            {
                string flag = Util.NVC(DataTableConverter.GetValue(dgOutStationList.Rows[dgOutStationList.Rows.Count - 1].DataItem, "FLAG"));
                if (flag.Equals("Y")) DataGridRowRemove(dgOutStationList);
            }
        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            int iChkCnt = 0;

            try
            {

                for (int i = 0; i < dgOutStationList.Rows.Count; i++)
                {
                    if (Util.NVC(DataTableConverter.GetValue(dgOutStationList.Rows[i].DataItem, "CHK")).Equals("True"))
                    {
                        iChkCnt++;
                    }
                }

                if (iChkCnt == 0)
                {
                    //선택된 데이터가 없습니다.
                    Util.MessageInfo("FM_ME_0165");
                    return;
                }

                DataTable dtRqst = new DataTable();
                dtRqst.TableName = "INDATA";
                dtRqst.Columns.Add("PORT_ID", typeof(string));
                dtRqst.Columns.Add("STN_ISS_COND_CODE", typeof(string));
                dtRqst.Columns.Add("DUAL_STK_SEQNO", typeof(string));
                dtRqst.Columns.Add("ADD_ISS_ENABLE_LANE_LIST", typeof(string));
                dtRqst.Columns.Add("ADD_ISS_ENABLE_PROC_DETL_TYPE_CODE_LIST", typeof(string));
                dtRqst.Columns.Add("INSUSER", typeof(string));
                dtRqst.Columns.Add("UPDUSER", typeof(string));
                dtRqst.Columns.Add("EVENT_FLAG", typeof(string));

                for (int i = 0; i < dgOutStationList.Rows.Count; i++)
                {
                    //if (Util.NVC(DataTableConverter.GetValue(dgOutStationList.Rows[i].DataItem, "FLAG")).Equals("Y") &&
                    //    Util.NVC(DataTableConverter.GetValue(dgOutStationList.Rows[i].DataItem, "CHK")).ToUpper().Equals("TRUE"))

                        if (Util.NVC(DataTableConverter.GetValue(dgOutStationList.Rows[i].DataItem, "FLAG")).Equals("Y") &&
                            Util.NVC(DataTableConverter.GetValue(dgOutStationList.Rows[i].DataItem, "CHK")).ToUpper().Equals("TRUE"))
                        {
                        DataRow dr = dtRqst.NewRow();
                        dr["PORT_ID"] = Util.NVC(DataTableConverter.GetValue(dgOutStationList.Rows[i].DataItem, "PORT_ID"));
                        dr["STN_ISS_COND_CODE"] = Util.NVC(DataTableConverter.GetValue(dgOutStationList.Rows[i].DataItem, "STN_ISS_COND_CODE"));
                        dr["DUAL_STK_SEQNO"] = Util.NVC(DataTableConverter.GetValue(dgOutStationList.Rows[i].DataItem, "DUAL_STK_SEQNO"));
                        dr["ADD_ISS_ENABLE_LANE_LIST"] = Util.NVC(DataTableConverter.GetValue(dgOutStationList.Rows[i].DataItem, "ADD_ISS_ENABLE_LANE_LIST"));
                        dr["ADD_ISS_ENABLE_PROC_DETL_TYPE_CODE_LIST"] = Util.NVC(DataTableConverter.GetValue(dgOutStationList.Rows[i].DataItem, "ADD_ISS_ENABLE_PROC_DETL_TYPE_CODE_LIST"));
                        dr["INSUSER"] = LoginInfo.USERID;
                        dr["UPDUSER"] = LoginInfo.USERID;
                        dr["EVENT_FLAG"] = "I";
                        dtRqst.Rows.Add(dr);
                        }

                        // 업데이트 이벤트 추가
                        // E20240411-001004 ESNB PI Aging 출고 Station 공정 기능 변경 - 이준영

                    if (Util.NVC(DataTableConverter.GetValue(dgOutStationList.Rows[i].DataItem, "FLAG")).Equals("N") &&
                        Util.NVC(DataTableConverter.GetValue(dgOutStationList.Rows[i].DataItem, "CHK")).ToUpper().Equals("TRUE"))
                    {

                        DataRow dr = dtRqst.NewRow();
                        dr["PORT_ID"] = Util.NVC(DataTableConverter.GetValue(dgOutStationList.Rows[i].DataItem, "PORT_ID"));
                        dr["STN_ISS_COND_CODE"] = Util.NVC(DataTableConverter.GetValue(dgOutStationList.Rows[i].DataItem, "STN_ISS_COND_CODE"));
                        dr["DUAL_STK_SEQNO"] = Util.NVC(DataTableConverter.GetValue(dgOutStationList.Rows[i].DataItem, "DUAL_STK_SEQNO"));
                        dr["ADD_ISS_ENABLE_LANE_LIST"] = Util.NVC(DataTableConverter.GetValue(dgOutStationList.Rows[i].DataItem, "ADD_ISS_ENABLE_LANE_LIST"));
                        dr["ADD_ISS_ENABLE_PROC_DETL_TYPE_CODE_LIST"] = Util.NVC(DataTableConverter.GetValue(dgOutStationList.Rows[i].DataItem, "ADD_ISS_ENABLE_PROC_DETL_TYPE_CODE_LIST"));
                        dr["INSUSER"] = LoginInfo.USERID;
                        dr["UPDUSER"] = LoginInfo.USERID;
                        dr["EVENT_FLAG"] = "U";
                        dtRqst.Rows.Add(dr);

                    }
                }



                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("BR_SET_STK_ISS_COND_UI", "INDATA", "OUTDATA", dtRqst);

                if (dtRslt != null)
                {
                    if (dtRslt.Rows[0]["RESULT"].ToString().Equals("0"))
                    {
                        Util.Alert("FM_ME_0215");  //저장하였습니다.
                        GetList();
                    }
                    else
                    {
                        Util.Alert("FM_ME_0213");  //저장실패하였습니다.
                        return;
                    }
                }
                else
                {
                    Util.Alert("FM_ME_0213");  //저장실패하였습니다.
                    return;
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            DataTable dtRqst = new DataTable();
            dtRqst.TableName = "INDATA";
            dtRqst.Columns.Add("PORT_ID", typeof(string));
            dtRqst.Columns.Add("STN_ISS_COND_CODE", typeof(string));
            dtRqst.Columns.Add("EVENT_FLAG", typeof(string));

            for (int i = 0; i < dgOutStationList.Rows.Count; i++)
            {
                if (Util.NVC(DataTableConverter.GetValue(dgOutStationList.Rows[i].DataItem, "CHK")).ToUpper().Equals("TRUE"))
                {
                    DataRow dr = dtRqst.NewRow();
                    dr["PORT_ID"] = Util.NVC(DataTableConverter.GetValue(dgOutStationList.Rows[i].DataItem, "PORT_ID"));
                    dr["STN_ISS_COND_CODE"] = Util.NVC(DataTableConverter.GetValue(dgOutStationList.Rows[i].DataItem, "STN_ISS_COND_CODE"));
                    dr["EVENT_FLAG"] = "D";
                    dtRqst.Rows.Add(dr);
                }
            }

            DataTable dtRslt = new ClientProxy().ExecuteServiceSync("BR_SET_STK_ISS_COND_UI", "INDATA", "OUTDATA", dtRqst);
            if (dtRslt.Rows[0]["RESULT"].ToString().Equals("0")) //0성공, -1실패
            {
                Util.MessageValidation("FM_ME_0154");  //삭제하였습니다.
            }
            else
            {
                Util.MessageValidation("FM_ME_0153");  //삭제 실패하였습니다.
            }

            GetList();
        }

        private void checkAll_Checked(object sender, RoutedEventArgs e)
        {
            Util.DataGridCheckAllChecked(dgOutStationList);
        }

        private void checkAll_Unchecked(object sender, RoutedEventArgs e)
        {
            Util.DataGridCheckAllUnChecked(dgOutStationList);
        }
        
        private void dgOutStationList_LoadedRowHeaderPresenter(object sender, C1.WPF.DataGrid.DataGridRowEventArgs e)
        {
            if (e.Row.HeaderPresenter == null)
            {
                return;
            }

            e.Row.HeaderPresenter.Content = null;

            TextBlock tb = new TextBlock();

            tb.Text = (e.Row.Index + 1 - dgOutStationList.TopRows.Count).ToString();
            tb.VerticalAlignment = VerticalAlignment.Center;
            tb.HorizontalAlignment = HorizontalAlignment.Center;
            e.Row.HeaderPresenter.Content = tb;
        }
        #endregion

        #region Method
        private void GetList()
        {
            try
            {
                DataTable inDataTable = new DataTable();
                inDataTable.Columns.Add("PORT_ID", typeof(string));

                DataRow newRow = inDataTable.NewRow();
                newRow["PORT_ID"] = Util.GetCondition(cboPort, bAllNull: true);

                inDataTable.Rows.Add(newRow);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_SEL_STK_ISS_COND_UI", "INDATA", "OUTDATA", inDataTable);

                dtRslt.Columns.Add("CHK", typeof(bool));
                dtRslt.Columns.Add("FLAG", typeof(string));
                for (int i = 0; i < dtRslt.Rows.Count; i++)
                {
                    dtRslt.Rows[i]["FLAG"] = "N";
                }

                Util.GridSetData(dgOutStationList, dtRslt, this.FrameOperation, false);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void DataGridRowAdd(C1.WPF.DataGrid.C1DataGrid dg, int iRowcount)
        {
            try
            {
                DataTable dt = new DataTable();

                foreach (C1.WPF.DataGrid.DataGridColumn col in dg.Columns)
                {
                    dt.Columns.Add(Convert.ToString(col.Name));
                }

                if (dg.Rows.Count == 0)
                    dt.Columns.Add("FLAG", typeof(string));

                if (dg.ItemsSource != null)
                {
                    for (int i = 0; i < iRowcount; i++)
                    {
                        dt = DataTableConverter.Convert(dg.ItemsSource);

                        if (dg.Rows.Count == 0 && !dt.Columns.Contains("FLAG"))
                            dt.Columns.Add("FLAG", typeof(string));

                        DataRow dr = dt.NewRow();
                        dr["PORT_ID"] = Util.GetCondition(cboPort, bAllNull: true);
                        dr["INSUSER"] = LoginInfo.USERID;
                        dr["FLAG"] = "N";
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
                        dr["PORT_ID"] = Util.GetCondition(cboPort, bAllNull: true);
                        dr["INSUSER"] = LoginInfo.USERID;
                        dr["FLAG"] = "N";
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

        private void DataGridRowRemove(C1.WPF.DataGrid.C1DataGrid dg)
        {
            try
            {
                if (dg.GetRowCount() > 0)
                {
                    DataTable dt = DataTableConverter.Convert(dg.ItemsSource);
                    dt.Rows[dg.Rows.Count - 1].Delete();
                    dt.AcceptChanges();
                    dg.ItemsSource = DataTableConverter.Convert(dt);
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return;
            }
        }

        #endregion
    }
}
