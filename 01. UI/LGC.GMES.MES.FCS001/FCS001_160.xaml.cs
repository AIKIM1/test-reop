/*************************************************************************************
 Created Date : 2023.05.22
      Creator : LJE
   Decription : Tray Type별 공 stocker 점유율
----------------------------------------------------------------------------------------------------------------------
 [Change History]
  2023.05.22  NAME  : Initial Created.
  2023.12.30  이정미 : 같은 목적지인 트레이 유형 저장 및 수정시 상한값 필터링 오류 수정
  2025.07.07  장의진: delete 이벤트 INDATA 수정
 **************************************************************************************/

using C1.WPF;
using C1.WPF.DataGrid;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using System;
using System.Data;
using System.Linq;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using LGC.GMES.MES.CMM001.Extensions;


namespace LGC.GMES.MES.FCS001
{
    public partial class FCS001_160 : UserControl, IWorkArea
    {
        #region Declaration & Constructor 

        Util _Util = new Util();

        public FCS001_160()
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
            string[] sFilter0 = { "E" };
            _combo.SetCombo(cboDest, CommonCombo_Form.ComboStatus.SELECT, sCase: "CNVR_LOCATION", sFilter : sFilter0); 
        }
        #endregion

        #region Event
        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            //사용자 권한별로 버튼 숨기기
            List<Button> listAuth = new List<Button>();
            Util.pageAuth(listAuth, FrameOperation.AUTHORITY);
            //여기까지 사용자 권한별로 버튼 숨기기

            if (!CheckButtonPermissionGroupByBtnGroupID("DELETE_INFORMATION_W", "FCS001_160"))
                btnDelete.Visibility = Visibility.Hidden;

            InitCombo();
            this.Loaded -= UserControl_Loaded;
        }

        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            GetList();
        }

        private void dgTrayList_BeginningRowEdit(object sender, DataGridEditingRowEventArgs e)
        {
            if (Util.NVC(DataTableConverter.GetValue(dgTrayList.Rows[e.Row.Index].DataItem, "NEW_FLAG")).ToUpper().Equals("N")
                && (e.Row.DataGrid.CurrentCell.Column.Name.Equals("CNVR_LOCATION_ID") || e.Row.DataGrid.CurrentCell.Column.Name.Equals("CST_TYPE_CODE")))
            {

                e.Cancel = true;
            }

        }

        private void btnUnitPlus_Click(object sender, RoutedEventArgs e)
        {
            //목적지가 설정되지 않았을경우 +버튼 무효화
            if (cboDest.GetStringValue().Equals(null) || cboDest.GetStringValue().Equals(""))
            {
                Util.AlertInfo("SFU7024", "", "");// 목적지를 설정하여 주세요.
                return;
            }
            DataGridRowAdd(dgTrayList, 1);
            DataTableConverter.SetValue(dgTrayList.Rows[dgTrayList.Rows.Count - 1].DataItem, "FLAG", "Y");
            DataTableConverter.SetValue(dgTrayList.Rows[dgTrayList.Rows.Count - 1].DataItem, "NEW_FLAG", "Y"); //새로운 Row에 대해서만 ReadOnly 이벤트 적용하기 위함
            DataTableConverter.SetValue(dgTrayList.Rows[dgTrayList.Rows.Count - 1].DataItem, "CHK", true);
            LocationSetGridCbo(dgTrayList.Columns["CNVR_LOCATION_ID"], "CNVR_LOCATION_ID");
            TrayTypeSetGridCbo(dgTrayList.Columns["CST_TYPE_CODE"], "CST_TYPE_CODE",true);

        }

        private void btnUnitMinus_Click(object sender, RoutedEventArgs e)
        {
            string flag = Util.NVC(DataTableConverter.GetValue(dgTrayList.Rows[dgTrayList.Rows.Count - 1].DataItem, "FLAG"));
            if (flag.Equals("Y")) DataGridRowRemove(dgTrayList);
        }

    
        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                bool checkOK = false;
                checkOK = GetRangeCheck();

                if(checkOK == false ) return;

                DataTable dtRqst = new DataTable();
                dtRqst.TableName = "INDATA";
                dtRqst.Columns.Add("CNVR_LOCATION_ID", typeof(string));
                dtRqst.Columns.Add("CST_TYPE_CODE", typeof(string));
                dtRqst.Columns.Add("USE_FLAG", typeof(string));
                dtRqst.Columns.Add("INSUSER", typeof(string));
                dtRqst.Columns.Add("INSDTTM", typeof(DateTime));
                dtRqst.Columns.Add("UPDUSER", typeof(string));
                dtRqst.Columns.Add("UPDDTTM", typeof(DateTime));
                dtRqst.Columns.Add("RATIO_LLMT", typeof(string));
                dtRqst.Columns.Add("RATIO_ULMT", typeof(string));
                dtRqst.Columns.Add("CNVR_LOCATION_DESC", typeof(string));

                DataRow dr = null;
                for (int i = 0; i < dgTrayList.Rows.Count; i++)
                {
                    if (Util.NVC(DataTableConverter.GetValue(dgTrayList.Rows[i].DataItem, "FLAG")).Equals("Y"))
                    {
                        dr = dtRqst.NewRow();
                        dr["CNVR_LOCATION_ID"] = Util.NVC(DataTableConverter.GetValue(dgTrayList.Rows[i].DataItem, "CNVR_LOCATION_ID"));
                        dr["CST_TYPE_CODE"] = Util.NVC(DataTableConverter.GetValue(dgTrayList.Rows[i].DataItem, "CST_TYPE_CODE"));
                        dr["USE_FLAG"] = Util.NVC(DataTableConverter.GetValue(dgTrayList.Rows[i].DataItem, "USE_FLAG"));
                        dr["INSUSER"] = LoginInfo.USERID;
                        dr["INSDTTM"] = System.DateTime.Now;
                        dr["UPDUSER"] = LoginInfo.USERID;
                        dr["UPDDTTM"] = System.DateTime.Now;
                        dr["RATIO_LLMT"] = Util.NVC(DataTableConverter.GetValue(dgTrayList.Rows[i].DataItem, "RATIO_LLMT"));
                        dr["RATIO_ULMT"] = Util.NVC(DataTableConverter.GetValue(dgTrayList.Rows[i].DataItem, "RATIO_ULMT"));
                        dr["CNVR_LOCATION_DESC"] = Util.NVC(DataTableConverter.GetValue(dgTrayList.Rows[i].DataItem, "CNVR_LOCATION_DESC"));
                        dtRqst.Rows.Add(dr);
                    }
                }

                if (dtRqst.Rows.Count == 0) return;

                //저장하시겠습니까?
                Util.MessageConfirm("SFU1241", result =>
                {
                    if (result == MessageBoxResult.OK)
                    {
                        DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_INS_CNVR_LOCATION_TRAYTYPE_RATIO", "INDATA", "OUTDATA", dtRqst);

                        Util.MessageValidation("FM_ME_0215");  //저장하였습니다.
                        GetList();
                    }
                });

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }

        }

        private void dgTrayType_CommittedEdit(object sender, DataGridCellEventArgs e)
        {
            if (e.Cell.Column.Name.Equals("CHK"))
            {
                return;
            }
            C1.WPF.DataGrid.C1DataGrid datagrid = (sender as C1.WPF.DataGrid.C1DataGrid);
            int row = e.Cell.Row.Index;
            DataTableConverter.SetValue(datagrid.Rows[row].DataItem, "FLAG", "Y");
            DataTableConverter.SetValue(datagrid.Rows[row].DataItem, "CHK", true);

            datagrid.Refresh();
        }

        private void dgList_LoadedCellPresenter(object sender, DataGridCellEventArgs e)
        {
            if (sender == null)
                return;

            C1.WPF.DataGrid.C1DataGrid dataGrid = sender as C1.WPF.DataGrid.C1DataGrid;
            dataGrid.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (e.Cell.Presenter == null)
                {
                    return;
                }
                string tag = Util.NVC(e.Cell.Column.Tag);

                //if (!string.IsNullOrEmpty(tag))
                //{
                //    if (tag.Equals("A"))
                //        e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Gray);
                //    else e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Blue);
                //}

                e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Gray);
                e.Cell.Presenter.FontWeight = FontWeights.Bold;
                e.Cell.Presenter.BorderBrush = new SolidColorBrush(Colors.Black);

                e.Cell.Presenter.BorderBrush = Brushes.LightGray;
                e.Cell.Presenter.BorderThickness = new Thickness(0.5, 0, 0, 0.5);

            }));
        }

        private void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                //선택한 데이터를 삭제하시겠습니까?
               Util.MessageConfirm("FM_ME_0167", (result) =>
               {
                   if (result == MessageBoxResult.OK)
                   {
                       DataTable dtRqst = new DataTable();
                       dtRqst.TableName = "INDATA";
                       dtRqst.Columns.Add("PROCESS_TYPE", typeof(string));
                       dtRqst.Columns.Add("CNVR_LOCATION_DESC", typeof(string));
                       dtRqst.Columns.Add("RATIO_LLMT", typeof(string));
                       dtRqst.Columns.Add("RATIO_ULMT", typeof(string));
                       dtRqst.Columns.Add("USER_ID", typeof(string));
                       dtRqst.Columns.Add("CNVR_LOCATION_ID", typeof(string));
                       dtRqst.Columns.Add("CST_TYPE_CODE", typeof(string));

                       for (int i = 0; i < dgTrayList.Rows.Count; i++)
                       {
                           if (Util.NVC(DataTableConverter.GetValue(dgTrayList.Rows[i].DataItem, "CHK")).ToUpper().Equals("TRUE")
                               && Util.NVC(DataTableConverter.GetValue(dgTrayList.Rows[i].DataItem, "NEW_FLAG")).ToUpper().Equals("N"))
                           {
                               DataRow dr = dtRqst.NewRow();
                               dr["PROCESS_TYPE"] = "D";
                               dr["CNVR_LOCATION_DESC"] = Util.NVC(DataTableConverter.GetValue(dgTrayList.Rows[i].DataItem, "CNVR_LOCATION_DESC"));
                               dr["RATIO_LLMT"] = Util.NVC(DataTableConverter.GetValue(dgTrayList.Rows[i].DataItem, "RATIO_LLMT")); ;
                               dr["RATIO_ULMT"] = Util.NVC(DataTableConverter.GetValue(dgTrayList.Rows[i].DataItem, "RATIO_ULMT"));
                               dr["USER_ID"] = LoginInfo.USERID;
                               dr["CNVR_LOCATION_ID"] = Util.NVC(DataTableConverter.GetValue(dgTrayList.Rows[i].DataItem, "CNVR_LOCATION_ID"));
                               dr["CST_TYPE_CODE"] = Util.NVC(DataTableConverter.GetValue(dgTrayList.Rows[i].DataItem, "CST_TYPE_CODE"));
                               dtRqst.Rows.Add(dr);
                           }
                       }

                       //선택된 row가 0일경우 return
                       if (dtRqst.Rows.Count == 0)
                           return;

                       DataTable dtRslt = new ClientProxy().ExecuteServiceSync("BR_SET_CNVR_LOCATION_TRAYTYPE_UI", "INDATA", "OUTDATA", dtRqst);

                       if (dtRslt.Rows[0]["RETVAL"].ToString().Equals("0")) //0성공, -1실패
                       {
                           Util.MessageValidation("FM_ME_0154");  //삭제하였습니다.
                       }
                       else
                       {
                           Util.MessageValidation("FM_ME_0153");  //삭제 실패하였습니다.
                       }

                       GetList();
                   }
                });
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }

        private void checkAll_Checked(object sender, RoutedEventArgs e)
        {
            Util.DataGridCheckAllChecked(dgTrayList);
        }

        private void checkAll_Unchecked(object sender, RoutedEventArgs e)
        {
            Util.DataGridCheckAllUnChecked(dgTrayList);
        }

        private void dgOutStationList_LoadedRowHeaderPresenter(object sender, C1.WPF.DataGrid.DataGridRowEventArgs e)
        {
            if (e.Row.HeaderPresenter == null)
            {
                return;
            }

            e.Row.HeaderPresenter.Content = null;

            TextBlock tb = new TextBlock();

            tb.Text = (e.Row.Index + 1 - dgTrayList.TopRows.Count).ToString();
            tb.VerticalAlignment = VerticalAlignment.Center;
            tb.HorizontalAlignment = HorizontalAlignment.Center;
            e.Row.HeaderPresenter.Content = tb;
        }

        private void dgTrayList_CommittedEdit(object sender, C1.WPF.DataGrid.DataGridCellEventArgs e)
        {
            int _row = 0;
            int _col = 0;
            int _top_cnt = 0;
            string value = string.Empty;
            string column_name = string.Empty;

            if (e.Cell.Value == null)
            {
                return;
            }

            column_name = e.Cell.Column.Name;

            _top_cnt = dgTrayList.Rows.TopRows.Count;
            _row = e.Cell.Row.Index - _top_cnt;
            _col = e.Cell.Column.Index;
            value = e.Cell.Value.ToString();

            if (value.Length == 0)
            {
                return;
            }

            //if (column_name == "CNVR_LOCATION_ID" || column_name == "CST_TYPE_CODE")
            //{
            //    //DB 중복 체크
            //    DataTable dt = dtHistResult();
            //    for (int i = 0; i < dt.Rows.Count; i++)
            //    {
            //        if (DataTableConverter.GetValue(dgTrayList.Rows[_row + _top_cnt].DataItem, "CNVR_LOCATION_ID") == null) return;
            //        if (DataTableConverter.GetValue(dgTrayList.Rows[_row + _top_cnt].DataItem, "CST_TYPE_CODE") == null) return;

            //        if (DataTableConverter.GetValue(dgTrayList.Rows[_row + _top_cnt].DataItem, "CNVR_LOCATION_ID").ToString() == dt.Rows[i]["CNVR_LOCATION_ID"].ToString())
            //        {
            //            if (DataTableConverter.GetValue(dgTrayList.Rows[_row + _top_cnt].DataItem, "CST_TYPE_CODE").ToString() == dt.Rows[i]["CST_TYPE_CODE"].ToString())
            //            {
            //                if (DataTableConverter.GetValue(dgTrayList.Rows[_row + _top_cnt].DataItem, "CST_TYPE_CODE") != null && (DataTableConverter.GetValue(dgTrayList.Rows[_row + _top_cnt].DataItem, "CST_TYPE_CODE").ToString() == dt.Rows[i]["CST_TYPE_CODE"].ToString()))
            //                {
            //                    if (column_name == "CST_TYPE_CODE")
            //                    {
            //                        DataTableConverter.SetValue(dgTrayList.Rows[_row + _top_cnt].DataItem, "CST_TYPE_CODE", "");
            //                    }
            //                    else
            //                    {
            //                        DataTableConverter.SetValue(dgTrayList.Rows[_row + _top_cnt].DataItem, "CNVR_LOCATION_ID", "");
            //                    }

            //                    Util.MessageInfo("SFU3471", " " + ObjectDic.Instance.GetObjectName("목적지") + "(" + dt.Rows[i]["CNVR_LOCATION_ID"].ToString() + ") / " +
            //                                                        ObjectDic.Instance.GetObjectName("TRAY TYPE") + "(" + dt.Rows[i]["CST_TYPE_CODE"].ToString() + ") "
            //                                    );//[%1]은 이미 등록되었습니다.               
            //                }
            //            }
            //        }
            //    }
            //}
            //if (column_name == "RATIO_LLMT" || column_name == "RATIO_ULMT")
            //{
            //    //하한 값이 상한 값보다 클 때 알람메세지 발생
            //    int a = Util.NVC_Int(DataTableConverter.GetValue(dgTrayList.Rows[_row + _top_cnt].DataItem, "RATIO_LLMT").ToString());
            //    int b = Util.NVC_Int(DataTableConverter.GetValue(dgTrayList.Rows[_row + _top_cnt].DataItem, "RATIO_ULMT").ToString());

            //    if (a>b) {
            //        Util.MessageInfo("ME_0405");
            //        return;
            //            };

            //    int cellValue = 0;

            //    //같은 목적지를 가진 트레이 유형들의 상한값이 100을 넘을 시 알람 메세지 출력
            //    for (int i = 0; i < dgTrayList.Rows.Count; i++)
            //    {
            //        if (Convert.ToString(DataTableConverter.GetValue(dgTrayList.Rows[_row + _top_cnt].DataItem, "CNVR_LOCATION_ID")).Equals(DataTableConverter.GetValue(dgTrayList.Rows[i].DataItem, "CNVR_LOCATION_ID").ToString()))
            //            cellValue += Util.NVC_Int(DataTableConverter.GetValue(dgTrayList.Rows[i].DataItem, "RATIO_ULMT").ToString());
            //    }

            //    if (cellValue > 100)
            //    {
            //        Util.MessageInfo("ME_0404"); //해당 목적지의 트레이타입별 최대 점유율의 합이 100%가 넘습니다.
            //        return;
            //    }


            //}


        }
        #endregion

        #region Method
        private void GetList() 
        {
            try
            {
                DataTable dtRqst = new DataTable();
                dtRqst.Columns.Add("LANGID", typeof(string));
                dtRqst.Columns.Add("CNVR_LOCATION_ID", typeof(string));

                DataRow dr = dtRqst.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["CNVR_LOCATION_ID"] = Util.GetCondition(cboDest, bAllNull: true);

                dtRqst.Rows.Add(dr);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_SEL_CNVR_LOCATION_TRAYTYPE_RATIO_V2_UI", "INDATA", "OUTDATA", dtRqst);

                dtRslt.Columns.Add("CHK", typeof(bool));

                if (dtRslt.Rows.Count > 0)
                {
                    dtRslt.Columns.Add("FLAG", typeof(string));
                    dtRslt.Columns.Add("NEW_FLAG", typeof(string));

                    for (int i = 0; i < dtRslt.Rows.Count; i++)
                    {
                        dtRslt.Rows[i]["FLAG"] = "N";
                        dtRslt.Rows[i]["NEW_FLAG"] = "N";

                    }

                    //Grid Combo Setting
                    SetGridCboItem(dgTrayList.Columns["USE_FLAG"], "USE_FLAG");
                    LocationSetGridCbo(dgTrayList.Columns["CNVR_LOCATION_ID"], "CNVR_LOCATION_ID");

                }

                Util.GridSetData(dgTrayList, dtRslt, this.FrameOperation, false);
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
                {
                    dt.Columns.Add("FLAG", typeof(string));
                    dt.Columns.Add("NEW_FLAG", typeof(string));
                }

                if (dg.ItemsSource != null)
                {
                    for (int i = 0; i < iRowcount; i++)
                    {
                        dt = DataTableConverter.Convert(dg.ItemsSource);

                        if (dg.Rows.Count == 0)
                        {
                            dt.Columns.Add("FLAG", typeof(string));
                            dt.Columns.Add("NEW_FLAG", typeof(string));
                        }

                        DataRow dr = dt.NewRow();
                        dr["CNVR_LOCATION_ID"] = Util.GetCondition(cboDest, bAllNull: true);
                        dr["RATIO_LLMT"] = "0";
                        dr["RATIO_ULMT"] = "0";
                        dr["INSUSER"] = LoginInfo.USERID;
                        dr["UPDUSER"] = LoginInfo.USERID;
                        dr["INSUSER_NAME"] = LoginInfo.USERNAME;
                        dr["UPDUSER_NAME"] = LoginInfo.USERNAME;
                        dr["UPDDTTM"] = System.DateTime.Now;
                        dr["INSDTTM"] = System.DateTime.Now;
                        dr["USE_FLAG"] = "Y";
                        dr["FLAG"] = "N";
                        dr["NEW_FLAG"] = "N";
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
                        dr["CNVR_LOCATION_ID"] = Util.GetCondition(cboDest, bAllNull: true);
                        dr["RATIO_LLMT"] = "0";
                        dr["RATIO_ULMT"] = "0";
                        dr["INSUSER"] = LoginInfo.USERID;
                        dr["UPDUSER"] = LoginInfo.USERID;
                        dr["INSUSER_NAME"] = LoginInfo.USERNAME;
                        dr["UPDUSER_NAME"] = LoginInfo.USERNAME;
                        dr["USE_FLAG"] = "Y";
                        dr["FLAG"] = "N";
                        dr["NEW_FLAG"] = "N";
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

        private bool CheckButtonPermissionGroupByBtnGroupID(string sBtnGrpID, string sFormID)
        {
            try
            {
                bool bRet = false;

                DataTable inTable = new DataTable();
                inTable.Columns.Add("USERID", typeof(string));
                inTable.Columns.Add("AREAID", typeof(string));
                inTable.Columns.Add("FORMID", typeof(string));

                DataRow dtRow = inTable.NewRow();
                dtRow["USERID"] = LoginInfo.USERID;
                dtRow["AREAID"] = LoginInfo.CFG_AREA_ID;
                dtRow["FORMID"] = sFormID;
                inTable.Rows.Add(dtRow);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_BTN_PERMISSION_GRP_FORM", "INDATA", "OUTDATA", inTable);

                if (CommonVerify.HasTableRow(dtRslt))
                {
                    DataRow[] drs = dtRslt.Select("BTN_PMS_GRP_CODE = '" + sBtnGrpID + "'");
                    if (drs?.Length > 0)
                        bRet = true;
                }

                if (bRet == false)
                {
                    string objectmessage = string.Empty;

                    //if (sBtnGrpID == "SCRAP_PROC_W")
                    //    objectmessage = ObjectDic.Instance.GetObjectName("PHYSICAL_DISPOSAL");

                    //Util.MessageValidation("SFU3520", LoginInfo.USERID, objectmessage);     // 해당 USER[%1]는 권한[%2]을 가지고 있지 않습니다.
                }

                return bRet;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return false;
            }
        }

        private bool GetRangeCheck() //수정 필요
        {
            bool rtn = false;

            for (int i = 0; i < dgTrayList.Rows.Count; i++)
            {
                if (Util.NVC(DataTableConverter.GetValue(dgTrayList.Rows[i].DataItem, "FLAG")).Equals("Y"))
                {
                    if (Util.NVC(DataTableConverter.GetValue(dgTrayList.Rows[i].DataItem, "CNVR_LOCATION_ID")).ToString().Equals("") ||
                       string.IsNullOrEmpty(Util.NVC(DataTableConverter.GetValue(dgTrayList.Rows[i].DataItem, "CNVR_LOCATION_ID")).ToString()))
                    {
                        Util.Alert("SFU7024"); //목적지 정보가 없습니다.
                        return rtn;
                    }

                    if (Util.NVC(DataTableConverter.GetValue(dgTrayList.Rows[i].DataItem, "CST_TYPE_CODE")).ToString().Equals("") ||
                       string.IsNullOrEmpty(Util.NVC(DataTableConverter.GetValue(dgTrayList.Rows[i].DataItem, "CST_TYPE_CODE")).ToString()))
                    {
                        Util.Alert("SFU7030"); //TrayType 정보가 없습니다.
                        return rtn;
                    }

                    if (Util.NVC(DataTableConverter.GetValue(dgTrayList.Rows[i].DataItem, "RATIO_ULMT")).ToString().Equals("0") ||
                       string.IsNullOrEmpty(Util.NVC(DataTableConverter.GetValue(dgTrayList.Rows[i].DataItem, "RATIO_ULMT")).ToString()))
                    {
                        Util.Alert("SFU7031"); //상한 값을 입력해 주세요
                        return rtn;
                    }

                    int a = Util.NVC_Int(DataTableConverter.GetValue(dgTrayList.Rows[i].DataItem, "RATIO_LLMT").ToString());
                    int b = Util.NVC_Int(DataTableConverter.GetValue(dgTrayList.Rows[i].DataItem, "RATIO_ULMT").ToString());

                      if (a>b) {
                          Util.MessageInfo("SFU7032"); //최소 점유율이 최대 점유율보다 높습니다.
                        return rtn;
                               };

                    //DB 중복 체크 - 주석처리, 현재는 같은 목적지,traytype 값을 입력하면 merge into로 update 됨
                    //DataTable dt = dtHistResult();
                    //for (int j = 0; j < dt.Rows.Count; j++)
                    //{
                    //    if (DataTableConverter.GetValue(dgTrayList.Rows[i].DataItem, "CNVR_LOCATION_ID").ToString() == dt.Rows[j]["CNVR_LOCATION_ID"].ToString())
                    //    {
                    //        if (DataTableConverter.GetValue(dgTrayList.Rows[i].DataItem, "CST_TYPE_CODE").ToString() == dt.Rows[j]["CST_TYPE_CODE"].ToString())
                    //        {
                    //            if (DataTableConverter.GetValue(dgTrayList.Rows[i].DataItem, "CST_TYPE_CODE") != null && (DataTableConverter.GetValue(dgTrayList.Rows[i].DataItem, "CST_TYPE_CODE").ToString() == dt.Rows[j]["CST_TYPE_CODE"].ToString()))
                    //            {

                    //                Util.MessageInfo("SFU3471", " " + ObjectDic.Instance.GetObjectName("목적지") + "(" + dt.Rows[j]["CNVR_LOCATION_ID"].ToString() + ") / " +
                    //                                                    ObjectDic.Instance.GetObjectName("TRAY TYPE") + "(" + dt.Rows[j]["CST_TYPE_CODE"].ToString() + ") "
                    //                                );//[%1]은 이미 등록되었습니다.    
                    //                return rtn;
                    //            }
                    //        }
                    //    }
                    //}

                    //같은 목적지를 가진 트레이 유형들의 상한값이 100을 넘을 시 알람 메세지 출력 후 저장불가(DB 계산)
                    //string temp_location_id = Util.NVC(DataTableConverter.GetValue(dgTrayList.Rows[i].DataItem, "CNVR_LOCATION_ID"));
                    //string temp_tray_type = Util.NVC(DataTableConverter.GetValue(dgTrayList.Rows[i].DataItem, "CST_TYPE_CODE"));
                    //decimal ulmt_ratio = Util.NVC_Int(DataTableConverter.GetValue(dgTrayList.Rows[i].DataItem, "RATIO_ULMT"));
                    //decimal sum_ulmt_ratio = 0;

                    //DataTable ddt = dtSumULMTResult(temp_location_id, temp_tray_type);
                    //for (int k = 0; k < ddt.Rows.Count; k++)
                    //{
                    //    sum_ulmt_ratio = ulmt_ratio + Util.NVC_Int(ddt.Rows[k]["SUM_RATIO_ULMT"]);
                    //}

                    //if (sum_ulmt_ratio>100)
                    //{
                    //    Util.MessageInfo("SFU7033"); //같은 목적지의 상한값의 합이 100을 넘습니다.
                    //    return rtn;
                    //}


                    //같은 목적지를 가진 트레이 유형들의 상한값이 100을 넘을 시 알람 메세지 출력 후 저장불가(Grid 계산)
                    int cellValue = 0;
                    for (int j = 0; j < dgTrayList.Rows.Count; j++)
                    {
                        if (Convert.ToString(DataTableConverter.GetValue(dgTrayList.Rows[j].DataItem, "CNVR_LOCATION_ID")).Equals(cboDest.SelectedValue)
                            && Convert.ToString(DataTableConverter.GetValue(dgTrayList.Rows[j].DataItem, "USE_FLAG")).Equals("Y"))
                            cellValue += Util.NVC_Int(DataTableConverter.GetValue(dgTrayList.Rows[j].DataItem, "RATIO_ULMT").ToString());
                    }

                    if (cellValue > 100)
                    {
                        Util.MessageInfo("SFU7033"); //해당 목적지의 트레이타입별 최대 점유율의 합이 100%가 넘습니다.
                        return false;
                    }


                }
            }


            return true;
        }

        private DataTable AddStatus(DataTable dt, string sValue, string sDisplay)
        {
            DataRow dr = dt.NewRow();
            dr[sDisplay] = "-ALL-";
            dr[sValue] = "";
            dt.Rows.InsertAt(dr, 0);
            return dt;
        }

        public static DataTable SetCodeDisplay(DataTable dt, bool bCodeDisplay)
        {
            if (bCodeDisplay)
            {
                foreach (DataRow drRslt in dt.Rows)
                {
                    drRslt["CBO_NAME"] = drRslt["CBO_NAME"].ToString();
                }
            }
            return dt;
        }

        private bool SetGridCboItem(C1.WPF.DataGrid.DataGridColumn col, string sClassId, bool bCodeDisplay = false, string sCmnCd = null)
        {
            try
            {
                bool rtn = false;

                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("CMCDTYPE", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["CMCDTYPE"] = sClassId;

                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_CMN_CBO", "RQSTDT", "RSLTDT", RQSTDT);

                if (dtResult == null || dtResult.Rows.Count == 0)
                {
                    rtn = false;
                }
                else
                {
                    rtn = true;
                    (col as C1.WPF.DataGrid.DataGridComboBoxColumn).DisplayMemberPath = "CBO_NAME";
                    (col as C1.WPF.DataGrid.DataGridComboBoxColumn).SelectedValuePath = "CBO_CODE";
                    (col as C1.WPF.DataGrid.DataGridComboBoxColumn).ItemsSource = DataTableConverter.Convert(dtResult);
                }

                return rtn;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private bool TrayTypeSetGridCbo(C1.WPF.DataGrid.DataGridColumn col, string sClassId, bool bCodeDisplay = false, string sCmnCd = null)
        {
            try
            {
                bool rtn = false;

                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("AREAID", typeof(string));
                RQSTDT.Columns.Add("CNVR_LOCATION_ID", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["AREAID"] = LoginInfo.CFG_AREA_ID;
                dr["CNVR_LOCATION_ID"] = Util.GetCondition(cboDest, bAllNull: true);

                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_COMBO_PORT_RCV_ENABLE_TRAY_TYPE", "RQSTDT", "RSLTDT", RQSTDT);

                if (dtResult == null || dtResult.Rows.Count == 0)
                {
                    rtn = false;
                }
                else
                {
                    rtn = true;
                    (col as C1.WPF.DataGrid.DataGridComboBoxColumn).DisplayMemberPath = "CBO_NAME";
                    (col as C1.WPF.DataGrid.DataGridComboBoxColumn).SelectedValuePath = "CBO_CODE";
                    (col as C1.WPF.DataGrid.DataGridComboBoxColumn).ItemsSource = DataTableConverter.Convert(dtResult);
                }

                return rtn;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private bool LocationSetGridCbo(C1.WPF.DataGrid.DataGridColumn col, string sClassId, bool bCodeDisplay = false, string sCmnCd = null)
        {
            try
            {
                bool rtn = false;

                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("EQPT_GR_TYPE_CODE", typeof(string));
                RQSTDT.Columns.Add("USE_FLAG", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["EQPT_GR_TYPE_CODE"] = "E";
                dr["USE_FLAG"] = "Y";

                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_COMBO_CNVR_LOCATION", "RQSTDT", "RSLTDT", RQSTDT);

                if (dtResult == null || dtResult.Rows.Count == 0)
                {
                    rtn = false;
                }
                else
                {
                    rtn = true;
                    (col as C1.WPF.DataGrid.DataGridComboBoxColumn).DisplayMemberPath = "CBO_NAME";
                    (col as C1.WPF.DataGrid.DataGridComboBoxColumn).SelectedValuePath = "CBO_CODE";
                    (col as C1.WPF.DataGrid.DataGridComboBoxColumn).ItemsSource = DataTableConverter.Convert(dtResult);
                }

                return rtn;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private DataTable dtHistResult()
        {
            try
            {
                DataTable dt = new DataTable();

                DataTable inDataTable = new DataTable();
                inDataTable.Columns.Add("USE_FLAG", typeof(string));

                DataRow searchCondition = inDataTable.NewRow();
                searchCondition["USE_FLAG"] = null;

                inDataTable.Rows.Add(searchCondition);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_SEL_CNVR_LOCATION_TRAYTYPE_RATIO_UI", "INDATA", "OUTDATA", inDataTable);

                if (dtResult != null && dtResult.Rows.Count > 0)
                {
                    dt = dtResult;
                }
                else
                {
                    dt = null;
                }

                return dt;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return null;
            }
        }

        private DataTable dtSumULMTResult(string sCNVR_LOCATION_ID, string stray_type)
        {
            try
            {
                if (sCNVR_LOCATION_ID == null) return null;

                DataTable dt = new DataTable();

                DataTable inDataTable = new DataTable();
                inDataTable.Columns.Add("USE_FLAG", typeof(string));
                inDataTable.Columns.Add("CNVR_LOCATION_ID", typeof(string));
                inDataTable.Columns.Add("CST_TYPE_CODE", typeof(string));

                DataRow searchCondition = inDataTable.NewRow();
                searchCondition["USE_FLAG"] = "Y";
                searchCondition["CNVR_LOCATION_ID"] = sCNVR_LOCATION_ID;
                searchCondition["CST_TYPE_CODE"] = stray_type;

                inDataTable.Rows.Add(searchCondition);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_SEL_CNVR_LOCATION_TRAYTYPE_SUM_ULMT_RATIO_UI", "INDATA", "OUTDATA", inDataTable);

                if (dtResult != null && dtResult.Rows.Count > 0)
                {
                    dt = dtResult;
                }
                else
                {
                    dt = null;
                }

                return dt;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return null;
            }
        }

        private void cboDest_Changed(object sender, PropertyChangedEventArgs<object> e)
        {
            GetList();
        }

        #endregion

    }
}
