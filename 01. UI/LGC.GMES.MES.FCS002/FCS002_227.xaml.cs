/*************************************************************************************
 Created Date : 2022.12.05
      Creator : 
   Decription : Cmp 수동 입고
--------------------------------------------------------------------------------------
 [Change History]
  2023.07.14  DEVELOPER : Initial Created.
 
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
using C1.WPF.Excel;
using System.Configuration;
using Microsoft.Win32;
using System.IO;

namespace LGC.GMES.MES.FCS002
{
    public partial class FCS002_227 : UserControl, IWorkArea
    {
        #region Declaration & Constructor 

        Util _Util = new Util();

        DataTable preTable = new DataTable();

        string sToPort = string.Empty;
        string sFromPort = string.Empty;
        string sCNCL_FLAG = string.Empty;

        // 미등록 TRAYID 포함여부 
        string sExistTray = string.Empty;
        public FCS002_227()
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

        //화면내 combo 셋팅
        private void InitCombo()
        {
           
        }
        #endregion

        #region Event
        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                sCNCL_FLAG = "N";

                GetFromPort();
                GetToPort();
                GetReqList();
             //   InitCombo();
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void txtTrayID_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                GetList();
            }
        }

        private void btnDeleteAll_Click(object sender, RoutedEventArgs e)
        {
            dgList.ItemsSource = null;
            sCNCL_FLAG = "N";
            sExistTray = "N";
            btnSave.Content = ObjectDic.Instance.GetObjectName("CMP_OUT");
            GetReqList();
        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            Save();
        }

        private void Save()
        {
            if (dgList.Rows.Count == 0)
            {
                Util.MessageValidation("FM_ME_0240");  //처리할 데이터가 없습니다.
                return;
            }
        
            string sMSG = "FM_ME_0337";

            if (sExistTray == "Y")
                sMSG = "FM_ME_0536";
            Util.MessageConfirm(sMSG, (result) =>
            {
                if (result == MessageBoxResult.OK)
                {
                    try
                    {
                        if (dgList.Rows.Count > 0)
                        {
                            preTable = DataTableConverter.Convert(dgList.ItemsSource);
                        }

                        DataTable dt = new DataTable();

                        dt.Columns.Add("SRCTYPE", typeof(string));
                        dt.Columns.Add("CST_ID", typeof(string));
                        dt.Columns.Add("CNCL_FLAG", typeof(string));
                        dt.Columns.Add("USERID", typeof(string));
                        dt.Columns.Add("FROM_PORT", typeof(string));
                        dt.Columns.Add("TO_PORT", typeof(string));

                        for (int i = 0; i < preTable.Rows.Count; i++)
                        {
                            DataRow dr = dt.NewRow();
                            dr["SRCTYPE"] = "UI";
                            dr["CST_ID"] = preTable.Rows[i]["CSTID"].ToString();
                            dr["CNCL_FLAG"] = sCNCL_FLAG;
                            dr["USERID"] = LoginInfo.USERID;
                            dr["FROM_PORT"] = sFromPort;
                            dr["TO_PORT"] = sToPort;

                            dt.Rows.Add(dr);

                            DataTable dtRslt = new ClientProxy().ExecuteServiceSync("BR_SET_CMP_OUT_MB", "INDATA", "OUTDATA", dt);

                            dt.Rows.Remove(dr);
                        }
                    }
                    catch (Exception ex)
                    {
                        Util.MessageException(ex);
                    }
                    finally
                    {
                        Util.AlertInfo("FM_ME_0136");  //변경완료하였습니다.
                         //refresh();
                        dgList.ItemsSource = null;
                        sCNCL_FLAG = "N";
                        sExistTray = "N";
                        btnSave.Content = ObjectDic.Instance.GetObjectName("CMP_OUT");
                        GetReqList();
                    }
                }
            });
        }

        private void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            //삭제 하시겠습니까?
            Util.MessageConfirm("FM_ME_0155", (result) =>
            {
                if (result == MessageBoxResult.OK)
                {
                    int index = ((C1.WPF.DataGrid.DataGridCellPresenter)((sender as Button).Parent)).Row.Index;

                    DataTable dt = DataTableConverter.Convert(dgList.ItemsSource);
                    DataRow dr = dt.Rows[index];
                    dt.Rows.Remove(dr);
                  
                    dgList.ItemsSource = null;

                    Util.GridSetData(dgList, dt, FrameOperation, true);

                    //dgList.IsReadOnly = false;
                    //dgList.RemoveRow(index);
                    dgList.IsReadOnly = true;

                    if(dt.Rows.Count == 0)
                    {
                        dgList.ItemsSource = null;
                        sCNCL_FLAG = "N";
                        sExistTray = "N";
                        btnSave.Content = ObjectDic.Instance.GetObjectName("CMP_OUT"); 
                        GetReqList();
                    }
                }
            });
        }

        private void btnInput_Click(object sender, RoutedEventArgs e)
        {
            GetList();
        }

        private void dgList_LoadedRowHeaderPresenter(object sender, C1.WPF.DataGrid.DataGridRowEventArgs e)
        {
            if (e.Row.HeaderPresenter == null)
            {
                return;
            }

            e.Row.HeaderPresenter.Content = null;
            C1DataGrid dataGrid = e.Row.DataGrid as C1DataGrid;
            TextBlock tb = new TextBlock();

            if (e.Row.Index - (dataGrid.TopRows.Count - 1) > 0 && e.Row.Index <= dataGrid.Rows.Count - 1)
            {
                tb.Text = (e.Row.Index + 1 - dataGrid.TopRows.Count).ToString();
                tb.VerticalAlignment = VerticalAlignment.Center;
                tb.HorizontalAlignment = HorizontalAlignment.Center;
                e.Row.HeaderPresenter.Content = tb;
            }
        }

        private void btnExcel_Click(object sender, RoutedEventArgs e)
        {
            LoadExcel();
        }

        private void LoadExcel()
        {
            sExistTray = "N";

            OpenFileDialog fd = new OpenFileDialog();

            if (ConfigurationManager.AppSettings["APP_CONFIG_SBC_MODE"] == "SBC")
            {
                fd.InitialDirectory = @"\\Client\C$";
            }

            fd.Filter = "Excel Files (.xlsx)|*.xlsx";


            if (fd.ShowDialog() == true)
            {
                using (Stream stream = fd.OpenFile())
                {
                    C1XLBook book = new C1XLBook();
                    book.Load(stream, FileFormat.OpenXml);
                    XLSheet sheet = book.Sheets[0];
                    DataTable dataTable = new DataTable();
                    dataTable.Columns.Add("CSTID", typeof(string));

                    if (sheet.Rows.Count > 20)
                    {
                        Util.MessageValidation("FM_ME_0463");  //최대 20개 까지 입니다.
                        return;
                    }

                    for (int rowInx = 0; rowInx < sheet.Rows.Count; rowInx++)
                    {
                        // CELL ID;
                        if (sheet.GetCell(rowInx, 0) == null)
                            return;

                        string CELL_ID = Util.NVC(sheet.GetCell(rowInx, 0).Text);
                        DataRow dataRow = dataTable.NewRow();
                        dataRow["CSTID"] = CELL_ID;
                        dataTable.Rows.Add(dataRow);
                    }

                    GetListExcel(dataTable);


                }
            }
        }

        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            GetReqList();
        }
        #endregion

        #region Method
        private void GetReqList()
        {
            DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_SEL_TB_SFC_FORM_TRF_TRGT_TRAY_ALL_MB", null, "RSLTDT", null);
            Util.GridSetData(dgReqList, dtRslt, FrameOperation, true);
        }

        private void GetList()
        {
            Util _util = new Util();
            if (string.IsNullOrEmpty(txtTrayID.Text))
            {
                return;
            }

            if (txtTrayID.Text.Length != 10)
            {
                Util.MessageValidation("FM_ME_0071");  //자릿수안맞음
                return;
            }

            if (!CheckTrayType(txtTrayID.Text))
            {
                Util.MessageValidation("FM_ME_0285");  //tray 타입 안맞음
                return;
            }

            //스프레드에 있는지 확인
            int iRow = -1;

            if (dgList.Rows.Count > 0)
            {
                iRow = _util.GetDataGridRowIndex(dgList, dgList.Columns["CSTID"].Name, txtTrayID.Text.Trim());
                if (iRow > -1)
                {
                    Util.MessageValidation("FM_ME_0193");  //이미 스캔한 ID 입니다.
                    return;
                }
            }
            
            string Tray_Frag = "N"; // 현재 선택 tray의 요청여부

            try
            {
                preTable = DataTableConverter.Convert(dgList.ItemsSource);

                if (dgList.Rows.Count > 20)
                {
                    Util.MessageValidation("FM_ME_0463");  //최대 20개 까지 입니다.
                    return;
                }

                DataTable dtRqst = new DataTable();
                dtRqst.Columns.Add("CST_ID", typeof(string));

                DataRow dr = dtRqst.NewRow();
                dr["CST_ID"] = txtTrayID.Text;
                dtRqst.Rows.Add(dr);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_SEL_TB_SFC_FORM_TRF_TRGT_TRAY_LIST_MB", "RQSTDT", "RSLTDT", dtRqst);

                if (dtRslt.Rows.Count > 0)
                {
                    Tray_Frag = "Y"; // 
                }

                // tray 존재여부
                string Exist_Flag = GetTrayInfo(txtTrayID.Text);

                // Y : CMP요청 가능 등록 TRAY, N : Carrier 등록되지 않은 Tray ID
                if (Exist_Flag == string.Empty)
                {
                    return;
                }

                //최초 Tray 일경우 처리
                if (dgList.Rows.Count == 0)
                {
                    if (dtRslt.Rows.Count > 0)
                    {
                        sCNCL_FLAG = "Y"; // 
                        btnSave.Content = ObjectDic.Instance.GetObjectName("OUT_CANCEL"); ;
                    }

                    DataTable FirstTray = new DataTable();
                    FirstTray.Columns.Add("CSTID", typeof(string));
                    FirstTray.Columns.Add("CNCL_FLAG", typeof(string));

                    DataRow row1 = FirstTray.NewRow();
                    row1["CSTID"] = Util.NVC(txtTrayID.Text);
                    row1["CNCL_FLAG"] = Util.NVC(sCNCL_FLAG);
                    FirstTray.Rows.Add(row1);

                    Util.GridSetData(dgList, FirstTray, FrameOperation, true);
                }
                //최초 Scan한 Tray와 비교
                else
                {
                    //최초 Scan한 Tray와 비교
                    if (!Tray_Frag.Equals(sCNCL_FLAG)) // 최초 tray와 현재 tray의 요청여부 비교
                    {
                        Util.MessageValidation("FM_ME_0527");  // 요청 상태를 확인해주세요
                        return;
                    }

                    DataRow row = preTable.NewRow();
                    row["CSTID"] = Util.NVC(txtTrayID.Text);
                    row["CNCL_FLAG"] = Util.NVC(sCNCL_FLAG);
                    preTable.Rows.Add(row);

                    Util.GridSetData(dgList, preTable, FrameOperation, true);
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
            finally
            {
                txtTrayID.Text = string.Empty;
                txtTrayID.Focus();
                txtTrayID.SelectAll();

                HiddenLoadingIndicator();
            }
        }

        private bool CheckTrayType(string sTrayID)
        {
            try
            {
                string sTrayType = GetTrayType(sTrayID);

                if (sTrayType.Equals(string.Empty))
                {
                    sTrayType = Util.NVC(sTrayID.Substring(0, 4));
                }

                DataTable dtRqst = new DataTable();
                dtRqst.Columns.Add("CST_TYPE_CODE", typeof(string));

                DataRow dr = dtRqst.NewRow();
                dr["CST_TYPE_CODE"] = sTrayType;
                dtRqst.Rows.Add(dr);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_SEL_CHECK_CST_TYPE_CODE", "RQSTDT", "RSLTDT", dtRqst);

                if (dtRslt.Rows.Count == 0)
                {
                    return false;
                }

                return true;
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
                return false;
            }

        }

        private void GetFromPort()
        {
            try
            {
                DataTable dtRqst = new DataTable();
                dtRqst.Columns.Add("AREAID", typeof(string));
                dtRqst.Columns.Add("CMCDTYPE", typeof(string));
                dtRqst.Columns.Add("ATTRIBUTE1", typeof(string));

                DataRow dr = dtRqst.NewRow();
                dr["AREAID"] = LoginInfo.CFG_AREA_ID;
                dr["CMCDTYPE"] = "FORM_CMP_TRF_PORT_MB";
                dr["ATTRIBUTE1"] = "Y";
                dtRqst.Rows.Add(dr);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_TB_MMD_AREA_COM_CODE_F", "RQSTDT", "RSLTDT", dtRqst);

                if (dtRslt.Rows.Count > 0)
                {
                    txtFrom.Text = dtRslt.Rows[0]["CMCDNAME"].ToString();
                    sFromPort = dtRslt.Rows[0]["CMCODE"].ToString();
                }
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }

        }

        private void GetToPort()
        {
            try
            {
                DataTable dtRqst = new DataTable();
                dtRqst.Columns.Add("AREAID", typeof(string));
                dtRqst.Columns.Add("CMCDTYPE", typeof(string));
                dtRqst.Columns.Add("ATTRIBUTE2", typeof(string));

                DataRow dr = dtRqst.NewRow();
                dr["AREAID"] = LoginInfo.CFG_AREA_ID;
                dr["CMCDTYPE"] = "FORM_CMP_TRF_PORT_MB";
                dr["ATTRIBUTE2"] = "Y";
                dtRqst.Rows.Add(dr);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_TB_MMD_AREA_COM_CODE_F", "RQSTDT", "RSLTDT", dtRqst);

                if (dtRslt.Rows.Count > 0)
                {
                    txtTo.Text = dtRslt.Rows[0]["CMCDNAME"].ToString();
                    sToPort = dtRslt.Rows[0]["CMCODE"].ToString();
                }
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }

        private string GetTrayInfo(string sTrayID)
        {
            string sRet = string.Empty;
            
            DataTable RQSTDT = new DataTable();
            RQSTDT.TableName = "RQSTDT";
            RQSTDT.Columns.Add("CSTID", typeof(string));

            DataRow dr1 = RQSTDT.NewRow();
            dr1["CSTID"] = sTrayID.Trim();
            RQSTDT.Rows.Add(dr1);
            
            DataTable SearchTray = new ClientProxy().ExecuteServiceSync("DA_SEL_EMPTY_NR_TRAY_MB", "RQSTDT", "RSLTDT", RQSTDT);

            if (SearchTray.Rows.Count == 0)
            {
                Util.MessageValidation("SFU8237");  // 등록되지 않은 TRAY ID입니다.

                // 등록되지 않은 Tray 포함됨 .
                sExistTray = "Y";
                sRet = "N";
            }
            else if (!(SearchTray.Rows[0]["CSTSTAT"].ToString() == "E" || // 공 TRAY
               SearchTray.Rows[0]["LOT_DETL_TYPE_CODE"].ToString() == "N" || // 폐기대기 등록 TRAY
               SearchTray.Rows[0]["LOT_DETL_TYPE_CODE"].ToString() == "R")) // 재작업 TRAY
            {
                Util.MessageValidation("FM_ME_0528");  // cmp 입고 가능한 tray가 아닙니다.
            }
            else // CMP 입고가능한 등록된 TRAY
            {
                sRet = "Y";
            }

            return sRet;
        }

        private string GetExTrayInfo(string sTrayID)
        {
            string sRet = string.Empty;

            DataTable RQSTDT = new DataTable();
            RQSTDT.TableName = "RQSTDT";
            RQSTDT.Columns.Add("CSTID", typeof(string));

            DataRow dr1 = RQSTDT.NewRow();
            dr1["CSTID"] = sTrayID.Trim();
            RQSTDT.Rows.Add(dr1);

            DataTable SearchTray = new ClientProxy().ExecuteServiceSync("DA_SEL_EMPTY_NR_TRAY_MB", "RQSTDT", "RSLTDT", RQSTDT);

            if (SearchTray.Rows.Count == 0)
            {
                //Util.MessageValidation("FM_ME_0536");  // 등록되지 않은 TRAY ID입니다.

                // 등록되지 않은 Tray 포함됨 .
                sExistTray = "Y";
                sRet = "N";
            }
            else if (!(SearchTray.Rows[0]["CSTSTAT"].ToString() == "E" || // 공 TRAY
               SearchTray.Rows[0]["LOT_DETL_TYPE_CODE"].ToString() == "N" || // 폐기대기 등록 TRAY
               SearchTray.Rows[0]["LOT_DETL_TYPE_CODE"].ToString() == "R")) // 재작업 TRAY
            {
                Util.MessageValidation("FM_ME_0528");  // cmp 입고 가능한 tray가 아닙니다.
            }
            else // CMP 입고가능한 등록된 TRAY
            {
                sRet = "Y";
            }

            return sRet;
        }

        private void GetListExcel(DataTable dt)
        {
            Util _util = new Util();
            sCNCL_FLAG = string.Empty;
            
            DataTable OUTDATA = new DataTable();
            OUTDATA.Columns.Add("CSTID", typeof(string));
            OUTDATA.Columns.Add("CNCL_FLAG", typeof(string));

            try
            {
                ShowLoadingIndicator();

                sCNCL_FLAG = string.Empty; // 

                // tray list 이상여부
                string sCheckList = "N";

                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    string ExcelTray = dt.Rows[i]["CSTID"].ToString().Trim();
                    string CNCL_FLAG = "N";

                    if (string.IsNullOrEmpty(ExcelTray))
                    {
                        sCheckList = "Y";
                    }
                    else if (ExcelTray.Length != 10)
                    {
                        sCheckList = "Y";
                    }
                    else if (!CheckTrayType(ExcelTray))
                    {
                        sCheckList = "Y";
                    }

                    if (sCheckList == "Y")
                    {
                        Util.MessageValidation("FM_ME_0217");  // 정보변경 가능 tray 아님
                        return;
                    }

                    if (GetExTrayInfo(ExcelTray) == string.Empty)
                    {
                        return;
                    }

                    DataTable dtRqst = new DataTable();
                    dtRqst.Columns.Add("CST_ID", typeof(string));

                    DataRow dr = dtRqst.NewRow();
                    dr["CST_ID"] = ExcelTray;
                    dtRqst.Rows.Add(dr);

                    DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_SEL_TB_SFC_FORM_TRF_TRGT_TRAY_LIST_MB", "RQSTDT", "RSLTDT", dtRqst);

                    if (dtRslt.Rows.Count > 0)
                    {
                        CNCL_FLAG = "Y"; // 
                    }

                    //최초 Tray 일경우 처리
                    if (string.IsNullOrEmpty(sCNCL_FLAG))
                    {

                        if (dtRslt.Rows.Count > 0)
                        {
                            sCNCL_FLAG = "Y"; // 
                            btnSave.Content = ObjectDic.Instance.GetObjectName("OUT_CANCEL");
                        }
                        else
                        {
                            sCNCL_FLAG = "N"; // 
                            btnSave.Content = ObjectDic.Instance.GetObjectName("CMP_OUT");
                        }

                        DataRow newRow = OUTDATA.NewRow();
                        newRow["CSTID"] = ExcelTray.Trim();
                        newRow["CNCL_FLAG"] = sCNCL_FLAG;

                        OUTDATA.Rows.Add(newRow);
                    }
                    else
                    {
                        //최초 Scan한 Tray와 비교
                        if (!CNCL_FLAG.Equals(sCNCL_FLAG)) // 최초 tray와 현재 tray의 요청여부 비교
                        {
                            Util.MessageValidation("FM_ME_0527");  // 요청 상태를 확인해주세요
                            return;
                        }

                        DataRow newRow = OUTDATA.NewRow();
                        newRow["CSTID"] = ExcelTray.Trim();
                        newRow["CNCL_FLAG"] = sCNCL_FLAG;

                        OUTDATA.Rows.Add(newRow);
                    }
                }

                if (sExistTray == "Y")
                {
                    Util.MessageValidation("FM_ME_0536");
                }

                Util.GridSetData(dgList, OUTDATA, FrameOperation, true);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
            finally
            {
                txtTrayID.Text = string.Empty;
                txtTrayID.Focus();
                txtTrayID.SelectAll();

                HiddenLoadingIndicator();
            }
        }
     
        private void ShowLoadingIndicator()
        {
            if (loadingIndicator != null)
            {
                loadingIndicator.Visibility = Visibility.Visible;
            }
        }

        private void HiddenLoadingIndicator()
        {
            if (loadingIndicator != null)
            {
                loadingIndicator.Visibility = Visibility.Collapsed;
            }
        }

        private string GetTrayType(string sTrayID)
        {
            string sRet = string.Empty;

            DataTable RQSTDT = new DataTable();
            RQSTDT.TableName = "RQSTDT";
            RQSTDT.Columns.Add("CSTID", typeof(string));

            DataRow dr1 = RQSTDT.NewRow();
            dr1["CSTID"] = sTrayID.Trim();
            RQSTDT.Rows.Add(dr1);

            DataTable SearchTray = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_CARRIER_F", "RQSTDT", "RSLTDT", RQSTDT);

            if (SearchTray.Rows.Count == 0)
            {
                Util.MessageValidation("SFU8237");  // 등록되지 않은 TRAY ID입니다.
                sRet = string.Empty;
            }
            else
            {
                sRet = SearchTray.Rows[0]["TRAY_TYPE_CODE"].ToString();
            }

            return sRet;
        }
        #endregion
        
    }
}