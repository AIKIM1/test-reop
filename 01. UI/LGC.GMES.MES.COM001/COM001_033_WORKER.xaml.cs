/*************************************************************************************
 Created Date : 2018.06.11
      Creator : 손우석
   Decription : 월력관리 작업자 등록
--------------------------------------------------------------------------------------
 [Change History]
  2018.06.11  손우석  3715740  180618 GMES 작업자 정보관리 기능추가 요청건 요청번호 C20180618_15740
  2018.07.17  손우석  3758429  180711_GMES 작업자 정보 등록 기능 개선요청  요청번호 C20180806_58429
  2018.12.03  손우석  3853072  181126_남경LGCNA  GMES 작업자 등록화면 개선 요청건 [요청번호]C20181126_53072
  2019.12.09  손우석    SM     오류 수정
**************************************************************************************/

using C1.WPF;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.CMM001.Popup;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using Microsoft.Win32;
using System;
using System.Collections;
using System.Data;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace LGC.GMES.MES.COM001
{
    public partial class COM001_033_WORKER : C1Window, IWorkArea
    {
        #region Declaration & Constructor 
        Util _Util = new Util();
        private string sSHOPID = string.Empty;
        private string sAREAID = string.Empty;
        private string sLINEID = string.Empty;
        private string sPROCID = string.Empty;
        private string sEQPTID = string.Empty;
        private string sSHIFTID = string.Empty;
        private string sGubun = string.Empty;

        private DateTime dtFromDate = new DateTime();
        private DateTime dtToDate = new DateTime();
        
        //2018.07.17
        private DataTable dtTemp = new DataTable();
        private DataTable dtShift = new DataTable();
        //2018.07.17

        //bool InputRow = false;
        //int addRows;

        public COM001_033_WORKER()
        {
            InitializeComponent();
        }

        public IFrameOperation FrameOperation
        {
            get;
            set;
        }
        #endregion

        #region Initialize
        private void C1Window_Loaded(object sender, RoutedEventArgs e)
        {
            object[] tmps = C1WindowExtension.GetParameters(this);

            if (tmps == null)
            {
                this.DialogResult = MessageBoxResult.Cancel;
                this.Close();
            }
            sSHOPID = Util.NVC(tmps[0]);
            sAREAID = Util.NVC(tmps[1]);
            sLINEID = Util.NVC(tmps[2]);

            dtFromDate = Convert.ToDateTime(Util.NVC(tmps[3]));
            dtToDate = Convert.ToDateTime(Util.NVC(tmps[4]));

            //2018.07.17
            //DateTime firstOfThisMonth = new DateTime(System.DateTime.Now.Year, System.DateTime.Now.Month, 1);
            //DateTime firstOfNextMonth = new DateTime(System.DateTime.Now.Year, System.DateTime.Now.Month, 1).AddMonths(1);

            //dtpWorkStartDay.SelectedDateTime = firstOfThisMonth;
            //dtpWorkEndDay.SelectedDateTime = firstOfNextMonth.AddDays(-1);

            dtpWorkStartDay.SelectedDateTime = DateTime.Today;
            dtpWorkEndDay.SelectedDateTime = DateTime.Today;

            dgTimeList.Visibility = Visibility.Hidden;

            DataTable dtDGTimeList = DataTableConverter.Convert(dgTimeList.ItemsSource);
            dtTemp.Columns.Add("CBO_CODE", typeof(string));
            dtTemp.Columns.Add("CBO_NAME", typeof(string));
            dtTemp.Columns.Add("CBO_DESC", typeof(string));
            dtTemp.Columns.Add("SHFT_STRT_HMS", typeof(string));
            dtTemp.Columns.Add("SHFT_END_HMS", typeof(string));

            dgTimeList.ItemsSource = DataTableConverter.Convert(dtTemp);
            //2018.07.17

            InitCombo();
        }
        private void InitCombo()
        {
            Set_Combo_UserClass(cboUserClass);

            Set_Combo_Shop(cboShop);
            Set_Combo_Area(cboAreaByAreaType);
            Set_Combo_Process(cboProcess);
            Set_Combo_EquipmentSegmant(cboEquipmentSegment);
            Set_Combo_Equipment(cboEquipment);
            Set_Shift(cboShift, sSHOPID, cboAreaByAreaType.SelectedValue.ToString(), cboEquipmentSegment.SelectedValue.ToString());

            //2018.12.03
            Set_Combo_User1(cboUserNameCr, CommonCombo.ComboStatus.SELECT);
            Set_Combo_User1(cboFreUserNameCr, CommonCombo.ComboStatus.SELECT);
        }
        #endregion

        #region Event

        #region Button
        private void btnShftTimeChage_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                //if (Search.RowDefinitions[5].Height.Value == 50)
                //{
                //    Search.RowDefinitions[5].Height = new GridLength(280);
                //}
                //else
                //{
                //    Search.RowDefinitions[5].Height = new GridLength(50);
                //}
            }
            catch (Exception ex)
            {
                Util.Alert(ex.ToString());
            }
        }
        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            SaveData();
        }
        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = MessageBoxResult.Cancel;
        }
        private void btnAdd_Click(object sender, RoutedEventArgs e)
        {
            if (DateTime.Parse(dtpWorkStartDay.SelectedDateTime.ToString("yyyy-MM-dd")) > DateTime.Parse(dtpWorkEndDay.SelectedDateTime.ToString("yyyy-MM-dd")))
            {
                Util.Alert("SFU1517");  //등록 시작일이 종료일보다 큽니다.
                dtpWorkStartDay.Focus();
                return;
            }

            if (cboUserClass.SelectedValue.ToString() == "SUC_03")
            {
                if (cboProcess.SelectedValue.ToString() == "")
                {
                    Util.Alert("SFU1459");  //공정을 선택하세요.
                    return;
                }

                if (cboEquipment.SelectedValue.ToString() == "")
                {
                    Util.Alert("PSS9147");  //설비를 선택하세요.
                    return;
                }
            }

            //2018.12.03
            //if (string.IsNullOrWhiteSpace(txtUserNameCr.Text) || string.IsNullOrWhiteSpace(txtUserID.Text))
            //2019.12.09
            //if (string.IsNullOrWhiteSpace(cboUserNameCr.SelectedValue.ToString()))
            if (cboUserNameCr.SelectedIndex ==0)
            {
                // 사용자를 확인 하세요.
                Util.MessageValidation("SFU4983");
                return;
            }

            //2018.07.17
            if ((bool)chkTime.IsChecked)
            {
                if (DateTime.Parse(dtpWorkStartDay.SelectedDateTime.ToString("yyyy-MM-dd")) != DateTime.Parse(dtpWorkEndDay.SelectedDateTime.ToString("yyyy-MM-dd")))
                {
                    Util.Alert("SFU5013");  //시간 변경은 시작일, 종료일이 동일해야 됩니다.
                    dtpWorkStartDay.Focus();
                    return;
                }

                //2019.12.09
                if (cboFreUserNameCr.SelectedIndex == 0)
                {
                    // 사용자를 확인 하세요.
                    Util.MessageValidation("SFU4983");
                    return;
                }
            }

            DataGrid01RowAdd(dgWorkerList);
        }
        private void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            DataGrid01RowDelete(dgWorkerList);
        }
        private void btnUser_Click(object sender, RoutedEventArgs e)
        {
            GetUserWindow();
        }
        #endregion

        #region Text
        private void txtUserName_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                //2018.12.03
                //if (string.IsNullOrWhiteSpace(txtUserNameCr.Text))
                //{
                //    // 사용자를 선택하세요.
                //    Util.MessageValidation("SFU4964");
                //    return;
                //}

                //GetUserWindow();
            }
        }

        //2018.07.17
        private void txtFreUserNameCr_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                //2018.12.03
                //if (string.IsNullOrWhiteSpace(txtFreUserNameCr.Text))
                //{
                //    // 사용자를 선택하세요.
                //    Util.MessageValidation("SFU4964");
                //    return;
                //}

                //GetFreUserWindow();
            }
        }
        #endregion

        #region Checkbox 2018.07.17
        private void chkTime_Checked(object sender, RoutedEventArgs e)
        {
            dgTimeList.Visibility = Visibility.Visible;
        }

        private void chkTime_Unchecked(object sender, RoutedEventArgs e)
        {
            dgTimeList.Visibility = Visibility.Hidden;
        }
        #endregion

        #endregion

        #region Mehod
        #region Grid
        private void DataGrid01RowAdd(C1.WPF.DataGrid.C1DataGrid dg)
        {
            try
            {
                int icnt = 0;

                DataTable RQSTDT = new DataTable();
                DataTable dtResult = null;

                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("SHOPID", typeof(string));
                RQSTDT.Columns.Add("AREAID", typeof(string));
                RQSTDT.Columns.Add("EQSGID", typeof(string));
                RQSTDT.Columns.Add("SHFT_ID", typeof(string));
                RQSTDT.Columns.Add("FROM_DATE", typeof(string));
                RQSTDT.Columns.Add("TO_DATE", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["SHOPID"] = cboShop.SelectedValue.ToString();
                dr["AREAID"] = cboAreaByAreaType.SelectedValue.ToString();
                dr["EQSGID"] = cboEquipmentSegment.SelectedValue.ToString();
                dr["SHFT_ID"] = cboShift.SelectedValue.ToString();
                dr["FROM_DATE"] = dtpWorkStartDay.SelectedDateTime.ToString("yyyyMMdd");
                dr["TO_DATE"] = dtpWorkEndDay.SelectedDateTime.ToString("yyyyMMdd");

                RQSTDT.Rows.Add(dr);

                dtResult = new ClientProxy().ExecuteServiceSync("DA_SEL_TB_MMD_CALDATE_SHIFT", "RQSTDT", "RSLTDT", RQSTDT);

                if (dtResult != null)
                {
                    if (dtResult.Rows.Count > 0)
                    {
                        TimeSpan tsDay = Convert.ToDateTime(dtpWorkEndDay.SelectedDateTime.ToString()) - Convert.ToDateTime(dtpWorkStartDay.SelectedDateTime.ToString());
                        int nDay = tsDay.Days + 1;

                        if (nDay != dtResult.Rows.Count)
                        {
                            Util.Alert("90049");  //현재 날짜의 월력일이 등록되지 않았습니다.
                            return;
                        }
                        else
                        {
                            DataSet dsResult = new DataSet();
                            DataTable inTable = new DataTable();

                            if (dg.ItemsSource == null)
                            {
                                inTable = dsResult.Tables.Add("INDATA");
                            }
                            else
                            {
                                inTable = DataTableConverter.Convert(dgWorkerList.ItemsSource);
                            }

                            if (dg.ItemsSource != null)
                            {
                                for (int irow = 0; irow < inTable.Rows.Count; irow++)
                                {
                                    if (inTable.Rows[0][1].ToString() != cboShop.SelectedValue.ToString())
                                    {
                                        return;
                                    }

                                    if (inTable.Rows[irow][1].ToString() == cboShop.SelectedValue.ToString())
                                    {
                                        if (inTable.Rows[irow][3].ToString() == cboAreaByAreaType.SelectedValue.ToString())
                                        {
                                            if (inTable.Rows[irow][5].ToString() == cboEquipmentSegment.SelectedValue.ToString())
                                            {
                                                if (inTable.Rows[irow][7].ToString() == cboProcess.SelectedValue.ToString())
                                                {
                                                    if (inTable.Rows[irow][13].ToString() == cboUserClass.SelectedValue.ToString())
                                                    {
                                                        //작업자
                                                        if (cboUserClass.SelectedValue.ToString() == "SUC_03")
                                                        {
                                                            if (inTable.Rows[irow][9].ToString() == cboEquipment.SelectedValue.ToString())
                                                            {
                                                                if (inTable.Rows[irow][11].ToString() == Util.NVC(cboShift.SelectedValue))
                                                                {
                                                                    icnt = icnt + 1;
                                                                }//SHIFT
                                                            }
                                                            else
                                                            {

                                                            }//EQPT
                                                        }
                                                        else
                                                        {
                                                            //반장, 조장
                                                            if (inTable.Rows[irow][13].ToString() == cboUserClass.SelectedValue.ToString())
                                                            {
                                                                //if (inTable.Rows[irow][9].ToString() == cboEquipment.SelectedValue.ToString())
                                                                //{
                                                                if (inTable.Rows[irow][11].ToString() == Util.NVC(cboShift.SelectedValue))
                                                                {
                                                                    icnt = icnt + 1;
                                                                }//SHIFT
                                                                //}
                                                                //else
                                                                //{

                                                                //}//EQPT
                                                            }
                                                            else
                                                            {
                                                                //if (inTable.Rows[irow][9].ToString() == cboEquipment.SelectedValue.ToString())
                                                                //{
                                                                if (inTable.Rows[irow][11].ToString() == Util.NVC(cboShift.SelectedValue))
                                                                {
                                                                    //icnt = icnt + 1;
                                                                }//SHIFT
                                                                //}
                                                                //else
                                                                //{

                                                                //}//EQPT
                                                            }
                                                        } //작업자 분류 체크
                                                    }
                                                    else
                                                    {
                                                        //작업자
                                                        if (cboUserClass.SelectedValue.ToString() == "SUC_03")
                                                        {
                                                            if (inTable.Rows[irow][9].ToString() == cboEquipment.SelectedValue.ToString())
                                                            {
                                                                if (inTable.Rows[irow][11].ToString() == Util.NVC(cboShift.SelectedValue))
                                                                {
                                                                    icnt = icnt + 1;
                                                                }//SHIFT
                                                            }
                                                            else
                                                            {

                                                            }//EQPT
                                                        }
                                                        else
                                                        {
                                                            //반장, 조장
                                                            if (inTable.Rows[irow][13].ToString() == cboUserClass.SelectedValue.ToString())
                                                            {
                                                                //if (inTable.Rows[irow][9].ToString() == cboEquipment.SelectedValue.ToString())
                                                                //{
                                                                if (inTable.Rows[irow][11].ToString() == Util.NVC(cboShift.SelectedValue))
                                                                {
                                                                    icnt = icnt + 1;
                                                                }//SHIFT
                                                                //}
                                                                //else
                                                                //{

                                                                //}//EQPT
                                                            }
                                                            else
                                                            {
                                                                //if (inTable.Rows[irow][9].ToString() == cboEquipment.SelectedValue.ToString())
                                                                //{
                                                                if (inTable.Rows[irow][11].ToString() == Util.NVC(cboShift.SelectedValue))
                                                                {
                                                                    //icnt = icnt + 1;
                                                                }//SHIFT
                                                                //}
                                                                //else
                                                                //{

                                                                //}//EQPT
                                                            }
                                                        } //작업자 분류 체크
                                                    }//USER CLASS
                                                }//PROC
                                            }//LINE
                                        }//AREA
                                    }//SHOP
                                }

                                if ((icnt > 0) && (!(bool)chkTime.IsChecked))
                                {
                                    Util.Alert("10017");  //입력하려는 값이 이미 존재합니다.
                                    return;
                                }
                                else
                                {
                                    DataRow dr2 = inTable.NewRow();

                                    dr2["CHK"] = true;
                                    dr2["SHOPID"] = cboShop.SelectedValue.ToString();
                                    dr2["SHOPNAME"] = cboShop.Text.ToString();
                                    dr2["AREAID"] = cboAreaByAreaType.SelectedValue.ToString();
                                    dr2["AREANAME"] = cboAreaByAreaType.Text.ToString();
                                    dr2["EQSGID"] = cboEquipmentSegment.SelectedValue.ToString();
                                    dr2["EQSGNAME"] = cboEquipmentSegment.Text.ToString();
                                    dr2["PROCID"] = cboProcess.SelectedValue.ToString();
                                    dr2["PROCNAME"] = cboProcess.Text.ToString();
                                    dr2["EQPTID"] = cboEquipment.SelectedValue.ToString();
                                    dr2["EQPTNAME"] = cboEquipment.Text.ToString();
                                    dr2["SHIFTID"] = Util.NVC(cboShift.SelectedValue);
                                    dr2["SHIFTNAME"] = cboShift.Text.ToString();
                                    dr2["SHFT_USER_CLSS_CODE"] = cboUserClass.SelectedValue.ToString();
                                    dr2["SHFT_USER_CLSS_NAME"] = cboUserClass.Text.ToString();
                                    //2018.12.03
                                    //dr2["WRK_USERID"] = txtUserID.Text.ToString();
                                    //dr2["USERNAME"] = txtUserNameCr.Text.ToString();
                                    dr2["WRK_USERID"] = Util.NVC(cboUserNameCr.SelectedValue.ToString());
                                    dr2["USERNAME"] = Util.NVC(cboUserNameCr.Text.ToString()); 
                                    dr2["STRT_DTTM"] = dtpWorkStartDay.SelectedDateTime.ToString("yyyy-MM-dd");
                                    dr2["END_DTTM"] = dtpWorkEndDay.SelectedDateTime.ToString("yyyy-MM-dd");
                                    //2018.07.17
                                    if ((bool)chkTime.IsChecked)
                                    {
                                        //2018.12.03
                                        //dr2["PRE_WRK_USERID"] = txtFreUserID.Text.ToString();
                                        //dr2["PRE_USERNAME"] = txtFreUserNameCr.Text.ToString();
                                        dr2["PRE_WRK_USERID"] = Util.NVC(cboFreUserNameCr.SelectedValue.ToString());
                                        dr2["PRE_USERNAME"] = Util.NVC(cboFreUserNameCr.Text.ToString()); 

                                        dr2["TIME_FLAG"] = "Y";

                                        dr2["FROM_TIME"] = Util.NVC(DataTableConverter.GetValue(dgTimeList.Rows[0].DataItem, "SHFT_STRT_HMS"));
                                        dr2["TO_TIME"] = Util.NVC(DataTableConverter.GetValue(dgTimeList.Rows[0].DataItem, "SHFT_END_HMS"));
                                    }
                                    else
                                    {
                                        dr2["WRK_USERID"] = null;
                                        dr2["USERNAME"] = null;
                                        dr2["TIME_FLAG"] = "N";
                                        dr2["FROM_TIME"] = null;
                                        dr2["TO_TIME"] = null;
                                    }

                                    inTable.Rows.Add(dr2);

                                    dgWorkerList.ItemsSource = DataTableConverter.Convert(inTable);
                                }
                            }
                            else
                            {
                                inTable.Columns.Add("CHK", typeof(string));
                                inTable.Columns.Add("SHOPID", typeof(string));
                                inTable.Columns.Add("SHOPNAME", typeof(string));
                                inTable.Columns.Add("AREAID", typeof(string));
                                inTable.Columns.Add("AREANAME", typeof(string));
                                inTable.Columns.Add("EQSGID", typeof(string));
                                inTable.Columns.Add("EQSGNAME", typeof(string));
                                inTable.Columns.Add("PROCID", typeof(string));
                                inTable.Columns.Add("PROCNAME", typeof(string));
                                inTable.Columns.Add("EQPTID", typeof(string));
                                inTable.Columns.Add("EQPTNAME", typeof(string));
                                inTable.Columns.Add("SHIFTID", typeof(string));
                                inTable.Columns.Add("SHIFTNAME", typeof(string));
                                inTable.Columns.Add("SHFT_USER_CLSS_CODE", typeof(string));
                                inTable.Columns.Add("SHFT_USER_CLSS_NAME", typeof(string));
                                inTable.Columns.Add("WRK_USERID", typeof(string));
                                inTable.Columns.Add("USERNAME", typeof(string));
                                inTable.Columns.Add("STRT_DTTM", typeof(string));
                                inTable.Columns.Add("END_DTTM", typeof(string));
                                //2018.07.17
                                inTable.Columns.Add("PRE_WRK_USERID", typeof(string));
                                inTable.Columns.Add("PRE_USERNAME", typeof(string));
                                inTable.Columns.Add("TIME_FLAG");
                                inTable.Columns.Add("FROM_TIME");
                                inTable.Columns.Add("TO_TIME");

                                DataRow dr2 = inTable.NewRow();

                                dr2["CHK"] = true;
                                dr2["SHOPID"] = cboShop.SelectedValue.ToString();
                                dr2["SHOPNAME"] = cboShop.Text.ToString();
                                dr2["AREAID"] = cboAreaByAreaType.SelectedValue.ToString();
                                dr2["AREANAME"] = cboAreaByAreaType.Text.ToString();
                                dr2["EQSGID"] = cboEquipmentSegment.SelectedValue.ToString();
                                dr2["EQSGNAME"] = cboEquipmentSegment.Text.ToString();
                                dr2["PROCID"] = cboProcess.SelectedValue.ToString();
                                dr2["PROCNAME"] = cboProcess.Text.ToString();
                                dr2["EQPTID"] = cboEquipment.SelectedValue.ToString();
                                dr2["EQPTNAME"] = cboEquipment.Text.ToString();
                                dr2["SHIFTID"] = Util.NVC(cboShift.SelectedValue);
                                dr2["SHIFTNAME"] = cboShift.Text.ToString();
                                dr2["SHFT_USER_CLSS_CODE"] = cboUserClass.SelectedValue.ToString();
                                dr2["SHFT_USER_CLSS_NAME"] = cboUserClass.Text.ToString();
                                //2018.12.03
                                //dr2["WRK_USERID"] = txtUserID.Text.ToString();
                                //dr2["USERNAME"] = txtUserNameCr.Text.ToString();
                                dr2["WRK_USERID"] = Util.NVC(cboUserNameCr.SelectedValue.ToString());
                                dr2["USERNAME"] = Util.NVC(cboUserNameCr.Text.ToString()); 
                                dr2["STRT_DTTM"] = dtpWorkStartDay.SelectedDateTime.ToString("yyyy-MM-dd");
                                dr2["END_DTTM"] = dtpWorkEndDay.SelectedDateTime.ToString("yyyy-MM-dd");
                                //2018.07.17
                                if ((bool)chkTime.IsChecked)
                                {
                                    //2018.12.03
                                    //dr2["PRE_WRK_USERID"] = txtFreUserID.Text.ToString();
                                    //dr2["PRE_USERNAME"] = txtFreUserNameCr.Text.ToString();
                                    dr2["PRE_WRK_USERID"] = Util.NVC(cboFreUserNameCr.SelectedValue.ToString());
                                    dr2["PRE_USERNAME"] = Util.NVC(cboFreUserNameCr.Text.ToString()); 


                                    dr2["TIME_FLAG"] = "Y";

                                    dr2["FROM_TIME"] = Util.NVC(DataTableConverter.GetValue(dgTimeList.Rows[0].DataItem, "SHFT_STRT_HMS"));
                                    dr2["TO_TIME"] = Util.NVC(DataTableConverter.GetValue(dgTimeList.Rows[0].DataItem, "SHFT_END_HMS"));
                                }
                                else
                                {
                                    dr2["PRE_WRK_USERID"] = null;
                                    dr2["PRE_USERNAME"] = null;
                                    dr2["TIME_FLAG"] = "N";
                                    dr2["FROM_TIME"] = null;
                                    dr2["TO_TIME"] = null;
                                }

                                inTable.Rows.Add(dr2);

                                dgWorkerList.ItemsSource = DataTableConverter.Convert(dsResult.Tables["INDATA"]);
                            }
                        }
                    }
                    else
                    {
                        Util.Alert("90049");  //현재 날짜의 월력일이 등록되지 않았습니다.
                        return;
                    }
                }
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex.Message), null, "Error", MessageBoxButton.OK, MessageBoxIcon.None);
                return;
            }
        }
        private void DataGrid01RowDelete(C1.WPF.DataGrid.C1DataGrid dg)
        {
            try
            {
                if (dg.Rows.Count > 0)
                {
                    DataTable dt = ((DataView)dg.ItemsSource).Table;

                    for (int i = dt.Rows.Count; 0 <= i; i--)
                    {
                        if (_Util.GetDataGridCheckValue(dg, "CHK", i))
                        {
                            dt.Rows[i].Delete();
                            dg.BeginEdit();
                            dg.ItemsSource = DataTableConverter.Convert(dt);
                            dg.EndEdit();
                        }
                    }
                }
                else
                {
                    //삭제할 항목이 없습니다.
                    LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("SFU1597"), null, "Info", MessageBoxButton.OK, MessageBoxIcon.None);
                    return;
                }
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex.Message), null, "Error", MessageBoxButton.OK, MessageBoxIcon.None);
                return;
            }
        }
        private void dgTimeList_CommittedEdit(object sender, C1.WPF.DataGrid.DataGridCellEventArgs e)
        {
            try
            {
                //if(dgTimeList.Rows.Count == 1)
                //{
                //    return;
                //}

                //DateTime dEditValue = DateTime.Parse(e.Cell.Value.ToString());

                //int iEditIndex_Column = e.Cell.Column.Index;
                //int iEditIndex_Row = e.Cell.Row.Index;

                //string sEditColumnName = e.Cell.Column.Name;
                //string sChkTagetName = sEditColumnName == "SHFT_STRT_HMS" ? "SHFT_END_HMS" : "SHFT_STRT_HMS";

                //DataTable dtTimeList = DataTableConverter.Convert(dgTimeList.ItemsSource);

                //C1.WPF.DataGrid.DataGridCell cell = e.Cell;                
                //string sShft =Util.NVC( DataTableConverter.GetValue(dgTimeList.Rows[e.Cell.Row.Index].DataItem, "CBO_CODE"));

                ////DataTable dtBefore = DataTableConverter.Convert(dgTimeList.ItemsSource);
                //DataRow[] drBefore = dtBeForeShift.Select("CBO_CODE = '"+ sShft + "'");
                //string sBeforeValue = Util.NVC(drBefore[0][sEditColumnName]);

                //if (sEditColumnName == "SHFT_STRT_HMS")
                //{
                //    if (iEditIndex_Row == 0)
                //    {
                //        string sTempTaget = dtTimeList.Rows[dtTimeList.Rows.Count - 1][sChkTagetName].ToString();
                //        DateTime dTempTaget = DateTime.Parse(sTempTaget);
                //        int iCheck = DateTime.Compare(dEditValue, dTempTaget);
                //        if (iCheck < 0)
                //        {
                //            Util.Alert("SFU1694");
                //            e.Cell.Value = sBeforeValue;                            
                //        }
                //    }
                //    else
                //    {
                //        string sTempTaget = dtTimeList.Rows[iEditIndex_Row - 1][sChkTagetName].ToString();
                //        DateTime dTempTaget = DateTime.Parse(sTempTaget);
                //        int iCheck = DateTime.Compare(dEditValue, dTempTaget);
                //        if (iCheck < 0)
                //        {
                //            Util.Alert("SFU1694");
                //            e.Cell.Value = sBeforeValue;                            
                //        }
                //    }
                //}
                //else
                //{
                //    if (iEditIndex_Row == dtTimeList.Rows.Count - 1)
                //    {
                //        string sTempTaget = dtTimeList.Rows[0][sChkTagetName].ToString();
                //        DateTime dTempTaget = DateTime.Parse(sTempTaget);
                //        int iCheck = DateTime.Compare(dTempTaget, dEditValue);
                //        if (iCheck < 0)
                //        {
                //            Util.Alert("SFU1694");
                //            e.Cell.Value = sBeforeValue;
                //        }
                //    }
                //    else
                //    {
                //        string sTempTaget = dtTimeList.Rows[iEditIndex_Row + 1][sChkTagetName].ToString();
                //        DateTime dTempTaget = DateTime.Parse(sTempTaget);
                //        int iCheck = DateTime.Compare(dTempTaget, dEditValue);
                //        if (iCheck < 0)
                //        {
                //            Util.Alert("SFU1694");
                //            e.Cell.Value = sBeforeValue;
                //        }
                //    }
                //}
            }
            catch (Exception ex)
            {
                Util.Alert(ex.ToString());
            }
        }
        #endregion

        #region Combo
        private void Set_Combo_Shop(C1ComboBox cbo)
        {
            try
            {
                DataTable dtRQSTDT = new DataTable();
                dtRQSTDT.TableName = "RQSTDT";
                dtRQSTDT.Columns.Add("LANGID", typeof(string));
                dtRQSTDT.Columns.Add("USERID", typeof(string));
                dtRQSTDT.Columns.Add("SYSID", typeof(string));

                DataRow drnewrow = dtRQSTDT.NewRow();
                drnewrow["LANGID"] = LoginInfo.LANGID;
                drnewrow["USERID"] = LoginInfo.USERID;
                drnewrow["SYSID"] = LGC.GMES.MES.Common.LoginInfo.SYSID + "_" + LGC.GMES.MES.Common.Common.APP_System;
                dtRQSTDT.Rows.Add(drnewrow);

                new ClientProxy().ExecuteService("DA_BAS_SEL_SHOP_BY_USERID_CBO", "RQSTDT", "RSLTDT", dtRQSTDT, (result, Exception) =>
                {
                    if (Exception != null)
                    {
                        LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(Exception.Message), null, "Error", MessageBoxButton.OK, MessageBoxIcon.None);
                        return;
                    }
                    cbo.ItemsSource = DataTableConverter.Convert(result);
                    if ((from DataRow dr in result.Rows where dr["CBO_CODE"].Equals(sSHOPID) select dr).Count() > 0)
                    {
                        cbo.SelectedValue = sSHOPID;
                    }
                    else if (result.Rows.Count > 0)
                    {
                        cbo.SelectedIndex = 0;
                    }
                    else if (result.Rows.Count == 0)
                    {
                        cbo.SelectedItem = null;
                    }
                    //cboShop_SelectedItemChanged(cbo, null);
                });
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex.Message), null, "Error", MessageBoxButton.OK, MessageBoxIcon.None);
                return;
            }
        }
        private void Set_Combo_Area(C1ComboBox cbo)
        {
            try
            {
                CommonCombo _combo = new CommonCombo();
                String[] sFilter = { sSHOPID, Area_Type.PACK };
                _combo.SetCombo(cboAreaByAreaType, CommonCombo.ComboStatus.SELECT, sFilter: sFilter);
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex.Message), null, "Error", MessageBoxButton.OK, MessageBoxIcon.None);
                return;
            }
        }
        private void Set_Combo_EquipmentSegmant(C1ComboBox cbo)
        {
            try
            {
                CommonCombo _combo = new CommonCombo();
                String[] sFilter = { cboAreaByAreaType.SelectedValue.ToString() };
                _combo.SetCombo(cboEquipmentSegment, CommonCombo.ComboStatus.SELECT, sFilter: sFilter);

                if (cbo.SelectedIndex > 0)
                {
                    sLINEID = cboEquipmentSegment.SelectedValue.ToString();
                }
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex.Message), null, "Error", MessageBoxButton.OK, MessageBoxIcon.None);
                return;
            }
        }

        private void Set_Combo_Process(C1ComboBox cbo)
        {
            try
            {
                CommonCombo _combo = new CommonCombo();
                String[] sFilter = { cboEquipmentSegment.SelectedValue.ToString() };
                _combo.SetCombo(cboProcess, CommonCombo.ComboStatus.ALL, sFilter: sFilter);

                if (sGubun != null)
                {
                    if (sGubun != "SUC_03")
                    {
                        cbo.SelectedIndex = 0;
                        cbo.IsEnabled = false;
                    }
                    else
                    {
                        cbo.IsEnabled = true;
                    }
                }
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex.Message), null, "Error", MessageBoxButton.OK, MessageBoxIcon.None);
                return;
            }
        }

        private void Set_Combo_Equipment(C1ComboBox cbo)
        {
            try
            {
                CommonCombo _combo = new CommonCombo();
                sLINEID = cboEquipmentSegment.SelectedValue.ToString();
                sPROCID = cboProcess.SelectedValue.ToString();
                String[] sFilter = { sLINEID, sPROCID};
                _combo.SetCombo(cboEquipment, CommonCombo.ComboStatus.ALL, sFilter: sFilter, sCase: "EQUIPMENT_BY_EQSGID_PROCID");

                if (sGubun != null)
                {
                    if (sGubun != "SUC_03")
                    {
                        cbo.SelectedIndex = 0;
                        cbo.IsEnabled = false;
                    }
                    else
                    {
                        cbo.IsEnabled = true;
                    }
                }
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex.Message), null, "Error", MessageBoxButton.OK, MessageBoxIcon.None);
                return;
            }
        }

        private void Set_Shift(C1ComboBox cbo, string sSelectedShop, string sSelectedArea, string sSelectedEqsg)
        {
            try
            {
                //DataTable RQSTDT = new DataTable();
                //RQSTDT.TableName = "RQSTDT";
                //RQSTDT.Columns.Add("LANGID", typeof(string));
                //RQSTDT.Columns.Add("SHOPID", typeof(string));
                //RQSTDT.Columns.Add("AREAID", typeof(string));
                //RQSTDT.Columns.Add("EQSGID", typeof(string));
                //RQSTDT.Columns.Add("FROMDATE", typeof(string));
                //RQSTDT.Columns.Add("TODATE", typeof(string));

                //DataRow dr = RQSTDT.NewRow();
                //dr["LANGID"] = LoginInfo.LANGID;
                //dr["SHOPID"] = sSelectedShop;
                //dr["AREAID"] = sSelectedArea;
                //dr["EQSGID"] = sSelectedEqsg;
                //dr["FROMDATE"] = dtpWorkStartDay.SelectedDateTime.ToString("yyyyMMdd");
                //dr["TODATE"] = dtpWorkEndDay.SelectedDateTime.ToString("yyyyMMdd");
                //RQSTDT.Rows.Add(dr);

                //DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_SEL_TB_MMD_CALDATE_SHIFT_FOR_CBOSHIFT", "RQSTDT", "RSLTDT", RQSTDT);

                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("SHOPID", typeof(string));
                RQSTDT.Columns.Add("AREAID", typeof(string));
                RQSTDT.Columns.Add("EQSGID", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["SHOPID"] = sSelectedShop;
                dr["AREAID"] = sSelectedArea;
                dr["EQSGID"] = sSelectedEqsg;
                RQSTDT.Rows.Add(dr);

                //DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_SHIFT_BY_AREA_CBO", "RQSTDT", "RSLTDT", RQSTDT);
                dtShift = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_SHIFT_BY_AREA_CBO", "RQSTDT", "RSLTDT", RQSTDT);

                cbo.DisplayMemberPath = "CBO_DESC";
                cbo.SelectedValuePath = "CBO_CODE";

                //cbo.ItemsSource = DataTableConverter.Convert(dtResult);
                cbo.ItemsSource = DataTableConverter.Convert(dtShift);

                //if (dtResult.Rows.Count <= 0)
                if (dtShift.Rows.Count <= 0)
                {
                    //Util.Alert("90049");  //현재 날짜의 월력일이 등록도지 않았습니다.
                }
                else
                {
                    cbo.SelectedIndex = 0;
                    //2018.07.17
                    SetTime(dtShift);
                }
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex.Message), null, "Error", MessageBoxButton.OK, MessageBoxIcon.None);
                return;
            }
        }

        private void Set_Combo_UserClass(C1ComboBox cbo)
        {
            DataTable RQSTDT = new DataTable();
            RQSTDT.TableName = "RQSTDT";
            RQSTDT.Columns.Add("LANGID", typeof(string));
            RQSTDT.Columns.Add("CMCDTYPE", typeof(string));

            DataRow dr = RQSTDT.NewRow();
            dr["LANGID"] = LoginInfo.LANGID;
            dr["CMCDTYPE"] = "SHFT_USER_CLSS_CODE";
            RQSTDT.Rows.Add(dr);

            DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_COMMONCODE", "RQSTDT", "RSLTDT", RQSTDT);

            cbo.DisplayMemberPath = "CBO_NAME";
            cbo.SelectedValuePath = "CBO_CODE";

            cbo.ItemsSource = DataTableConverter.Convert(dtResult);

            if (dtResult.Rows.Count <= 0)
            {

            }
            else
            {
                cbo.SelectedIndex = 0;
            }

            //new ClientProxy().ExecuteService("DA_BAS_SEL_COMMONCODE", "INDATA", "RSLTDT", IndataTable, (result, Exception) =>
            //{
            //    if (Exception != null)
            //    {
            //        LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(Exception.Message), null, "Error", MessageBoxButton.OK, MessageBoxIcon.None);
            //        return;
            //    }

            //    cbo.ItemsSource = DataTableConverter.Convert(result);

            //   if (result.Rows.Count > 0)
            //    {
            //        cbo.SelectedIndex = 2;
            //    }
            //    else if (result.Rows.Count == 0)
            //    {
            //        cbo.SelectedItem = null;
            //    }
            //});
        }

        private void SetGridCboItem(C1.WPF.DataGrid.DataGridColumn col)
        {
            //DataTable IndataTable = new DataTable();
            //IndataTable.Columns.Add("LANGID", typeof(string));
            //IndataTable.Columns.Add("CMCDTYPE", typeof(string));

            //DataRow Indata = IndataTable.NewRow();
            //Indata["LANGID"] = LoginInfo.LANGID;
            //Indata["CMCDTYPE"] = "SHFT_USER_CLSS_CODE";
            //IndataTable.Rows.Add(Indata);

            //DataTable dtMain = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_COMMONCODE", "INDATA", "RSLTDT", IndataTable);

            //(col as C1.WPF.DataGrid.DataGridComboBoxColumn).ItemsSource = DataTableConverter.Convert(dtMain);
        }

        private void cboShop_SelectedItemChanged(object sender, PropertyChangedEventArgs<object> e)
        {
                if (cboShop.SelectedIndex > -1)
                {
                    sSHOPID = Convert.ToString(cboShop.SelectedValue);
                    Set_Combo_Area(cboAreaByAreaType);
                }
                else
                {
                    sSHOPID = string.Empty;
                }
        }

        private void cboAreaByAreaType_SelectedItemChanged(object sender, PropertyChangedEventArgs<object> e)
        {
                if (cboAreaByAreaType.SelectedIndex > -1)
                {
                    sAREAID = Convert.ToString(cboAreaByAreaType.SelectedValue);
                    Set_Combo_EquipmentSegmant(cboEquipmentSegment);
                }
                else
                {
                    sAREAID = string.Empty;
                }
        }

        private void cboEquipmentSegment_SelectedItemChanged(object sender, PropertyChangedEventArgs<object> e)
        {
                if (cboEquipmentSegment.SelectedIndex > -1)
                {
                    sLINEID = Convert.ToString(cboEquipmentSegment.SelectedValue);
                    
                    if ((sSHOPID != null) & (sAREAID != null))
                    {
                        Set_Shift(cboShift, sSHOPID, cboAreaByAreaType.SelectedValue.ToString(), cboEquipmentSegment.SelectedValue.ToString());
                    }
                    
                    Set_Combo_Process(cboProcess);
                }
                else
                {
                    sLINEID = string.Empty;
                }
        }

        private void cboProcess_SelectedItemChanged(object sender, PropertyChangedEventArgs<object> e)
        {
                if (cboProcess.SelectedIndex > -1)
                {
                    sPROCID = Convert.ToString(cboProcess.SelectedValue);
                    Set_Combo_Equipment(cboEquipment);
                }
                else
                {
                    sPROCID = string.Empty;
                }
        }

        private void cboEquipment_SelectedItemChanged(object sender, PropertyChangedEventArgs<object> e)
        {
                if (cboEquipment.SelectedIndex > -1)
                {
                    sEQPTID = Convert.ToString(cboEquipment.SelectedValue);
                }
                else
                {
                    sEQPTID = string.Empty;
                }
        }

        private void cboShift_SelectedItemChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            if (cboShift.SelectedIndex > -1)
            {
                sSHIFTID = Convert.ToString(cboShift.SelectedValue);
                //2018.07.17
                SetTime(dtShift);
            }
            else
            {
                sSHIFTID = string.Empty;
            }
        }

        private void cboUserClass_SelectedItemChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            if (cboUserClass.SelectedIndex > -1)
            {
                sGubun = cboUserClass.SelectedValue.ToString();

                if (cboUserClass.SelectedValue.ToString() == "SUC_03")
                {
                    cboProcess.IsEnabled = true;
                    cboEquipment.IsEnabled = true;
                }
                else
                {
                    if (cboProcess.SelectedIndex > -1)
                    {
                        cboProcess.SelectedIndex = 0;
                        cboProcess.IsEnabled = false;
                    }

                    if (cboEquipment.SelectedIndex > -1)
                    {
                        cboEquipment.SelectedIndex = 0;
                        cboEquipment.IsEnabled = false;
                    }
                }
            }
            else
            {
                
            }
        }
        
        private void Set_Combo_User1(C1ComboBox cbo, CommonCombo.ComboStatus cs)
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("CMCDTYPE", typeof(string));
                RQSTDT.Columns.Add("SHOPID", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["CMCDTYPE"] = "PACK_WORKORDER_WORKER";
                dr["SHOPID"] = LoginInfo.CFG_SHOP_ID;
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_WORKORDER_WORKER", "RQSTDT", "RSLTDT", RQSTDT);

                cbo.DisplayMemberPath = "CBO_NAME";
                cbo.SelectedValuePath = "CBO_CODE";

                DataRow dr2 = dtResult.NewRow();
                dr2["CBO_NAME"] = "-- SELECT --";
                dr2["CBO_CODE"] = "";
                dr2["ATTRIBUTE3"] = "";

                dtResult.Rows.InsertAt(dr2, 0);

                cbo.ItemsSource = DataTableConverter.Convert(dtResult);

                cbo.SelectedItem = "-- SELECT --";
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex.Message), null, "Error", MessageBoxButton.OK, MessageBoxIcon.None);
                return;
            }
        }
        #endregion Combo

        private void SetTime(DataTable dtTime)
        {
            //2018.07.17
            for (int iLRow = 0; iLRow < dtTime.Rows.Count; iLRow++)
            {
                if(dtTime.Rows[iLRow][0].ToString() == cboShift.SelectedValue.ToString())
                {
                    if (dtTemp.Rows.Count > 0)
                    {
                        dtTemp.Clear();
                    }

                    DataRow dr = dtTemp.NewRow();
                    dr["CBO_CODE"] = dtTime.Rows[iLRow][0].ToString();
                    dr["CBO_NAME"] = dtTime.Rows[iLRow][1].ToString();
                    dr["CBO_DESC"] = dtTime.Rows[iLRow][2].ToString();
                    dr["SHFT_STRT_HMS"] = dtTime.Rows[iLRow][3].ToString();
                    dr["SHFT_END_HMS"] = dtTime.Rows[iLRow][4].ToString();
                    dtTemp.Rows.Add(dr);

                    dgTimeList.ItemsSource = DataTableConverter.Convert(dtTemp);
                }
            }
            //2018.07.17
        }

        private bool isValid(string sSHOP, string sAREA, string sEQSG, string sPROC, string sEQPT, string sSHFT, string sUSERCLSS, string sStartDay, string sEndDay)
        {
            bool bRet = false;

            try
            {
                DataTable RQSTDT = new DataTable();
                DataTable dtResult = null;

                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("SHOPID", typeof(string));
                RQSTDT.Columns.Add("AREAID", typeof(string));
                RQSTDT.Columns.Add("EQSGID", typeof(string));
                RQSTDT.Columns.Add("PROCID", typeof(string));
                RQSTDT.Columns.Add("EQPTID", typeof(string));
                RQSTDT.Columns.Add("SHFT_ID", typeof(string));
                RQSTDT.Columns.Add("SHFT_USER_CLSS_CODE", typeof(string));
                RQSTDT.Columns.Add("FROMDATE", typeof(string));
                RQSTDT.Columns.Add("TODATE", typeof(string));

                DataRow dr = RQSTDT.NewRow();

                DataTable dtWorker = DataTableConverter.Convert(dgWorkerList.ItemsSource);

                //for (int irow = 0; irow < dtWorker.Rows.Count; irow++)
                //{
                //dr["SHOPID"] = dtWorker.Rows[irow][1].ToString();
                //dr["AREAID"] = dtWorker.Rows[irow][3].ToString();
                //dr["EQSGID"] = dtWorker.Rows[irow][5].ToString();
                //dr["PROCID"] = dtWorker.Rows[irow][7].ToString();
                //dr["EQPTID"] = dtWorker.Rows[irow][9].ToString();
                //dr["SHFT_ID"] = dtWorker.Rows[irow][11].ToString();
                //dr["SHFT_USER_CLSS_CODE"] = dtWorker.Rows[irow][13].ToString();
                //dr["FROMDATE"] = dtpWorkStartDay.SelectedDateTime.ToString("yyyyMMdd");
                //dr["TODATE"] = dtpWorkEndDay.SelectedDateTime.ToString("yyyyMMdd");

                dr["SHOPID"] = sSHOP;
                dr["AREAID"] = sAREA;
                dr["EQSGID"] = sEQSG;
                dr["PROCID"] = sPROC;
                dr["EQPTID"] = sEQPT;
                dr["SHFT_ID"] = sSHFT;
                dr["SHFT_USER_CLSS_CODE"] = sUSERCLSS;
                dr["FROMDATE"] = sStartDay;
                dr["TODATE"] = sEndDay;
                RQSTDT.Rows.Add(dr);

                dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_WORKCALENDAR_WORKER_BY_EQSGIDS", "RQSTDT", "RSLTDT", RQSTDT);

                if (dtResult != null)
                {
                    if (dtResult.Rows.Count > 0)
                    {
                        bRet = true;
                    }
                }
                //}
            }
            catch (Exception ex)
            {
                throw ex;
            }

            return bRet;
        }

        private void SaveData()
        {
            try
            {
                //for (int irow1 = 0; irow1 < dtWorker.Rows.Count; irow1++)
                //{
                    //if (isValid(dtWorker.Rows[irow1][1].ToString(), dtWorker.Rows[irow1][3].ToString(), dtWorker.Rows[irow1][5].ToString(),
                    //             dtWorker.Rows[irow1][7].ToString(), dtWorker.Rows[irow1][9].ToString(), dtWorker.Rows[irow1][11].ToString(),
                    //             dtWorker.Rows[irow1][13].ToString(), dtpWorkStartDay.SelectedDateTime.ToString("yyyyMMdd"),
                    //             dtpWorkEndDay.SelectedDateTime.ToString("yyyyMMdd")))
                    //{
                        ////이미 등록된 정보가 존재합니다.!\r\n정보 변경하시겠습니까?
                        //LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("SFU4981"), null, "Information", MessageBoxButton.OKCancel, MessageBoxIcon.None, (result1) =>
                        //{
                        //    if (result1 == MessageBoxResult.OK)
                        //    {
                        //        DataTable INDATA = new DataTable();
                        //        INDATA.Columns.Add("SRCTYPE");
                        //        INDATA.Columns.Add("LANGID", typeof(string));
                        //        INDATA.Columns.Add("SHOPID", typeof(string));
                        //        INDATA.Columns.Add("AREAID", typeof(string));
                        //        INDATA.Columns.Add("EQSGID", typeof(string));
                        //        INDATA.Columns.Add("PROCID", typeof(string));
                        //        INDATA.Columns.Add("EQPTID", typeof(string));
                        //        INDATA.Columns.Add("SHFT_ID", typeof(string));
                        //        INDATA.Columns.Add("SHFT_USER_CLSS_CODE", typeof(string));
                        //        INDATA.Columns.Add("WRK_USERID", typeof(string));
                        //        INDATA.Columns.Add("FROMDATE", typeof(string));
                        //        INDATA.Columns.Add("TODATE", typeof(string));
                        //        INDATA.Columns.Add("USERID", typeof(string));

                        //        //DataTable dtWorker = DataTableConverter.Convert(dgWorkerList.ItemsSource);

                        //        DataRow drINDATA = null;

                        //        for (int irow = 0; irow < dtWorker.Rows.Count; irow++)
                        //        {
                        //            drINDATA = INDATA.NewRow();

                        //            drINDATA["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                        //            drINDATA["LANGID"] = LoginInfo.LANGID;
                        //            drINDATA["SHOPID"] = dtWorker.Rows[irow][1].ToString();
                        //            drINDATA["AREAID"] = dtWorker.Rows[irow][3].ToString();
                        //            drINDATA["EQSGID"] = dtWorker.Rows[irow][5].ToString();
                        //            drINDATA["PROCID"] = dtWorker.Rows[irow][7].ToString();
                        //            drINDATA["EQPTID"] = dtWorker.Rows[irow][9].ToString();
                        //            drINDATA["SHFT_ID"] = dtWorker.Rows[irow][11].ToString();
                        //            drINDATA["SHFT_USER_CLSS_CODE"] = dtWorker.Rows[irow][13].ToString();
                        //            drINDATA["WRK_USERID"] = dtWorker.Rows[irow][15].ToString();
                        //            drINDATA["FROMDATE"] = dtpWorkStartDay.SelectedDateTime.ToString("yyyyMMdd");
                        //            drINDATA["TODATE"] = dtpWorkEndDay.SelectedDateTime.ToString("yyyyMMdd");
                        //            drINDATA["USERID"] = LoginInfo.USERID;

                        //            INDATA.Rows.Add(drINDATA);

                        //            DataTable dtResult = new ClientProxy().ExecuteServiceSync("BR_PRD_REG_TB_SFC_CALDATE_WRKR", "INDATA", null, INDATA);
                        //        }

                        //        dgWorkerList.ItemsSource = null;

                        //        LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("9003"), null, "Information", MessageBoxButton.OK, MessageBoxIcon.None, (result2) =>
                        //        {
                        //            // Util.AlertInfo("9003");  //저장이 완료되었습니다.
                        //            if (result2 == MessageBoxResult.OK)
                        //            {
                        //                this.Close();
                        //            }
                        //        });
                        //    }
                        //});
                    //}
                    //else
                    //{
                        //저장하시겠습니까?
                        //LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("SFU1241"), null, "Information", MessageBoxButton.OKCancel, MessageBoxIcon.None, (sresult) =>
                        //});
                        LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("SFU1241"), null, "Information", MessageBoxButton.OKCancel, MessageBoxIcon.None, (result3) =>
                        {
                            if (result3 == MessageBoxResult.OK)
                            {
                                DataSet dsInput = new DataSet();

                                DataTable INDATA = new DataTable();
                                INDATA.Columns.Add("SRCTYPE");
                                INDATA.Columns.Add("LANGID", typeof(string));
                                INDATA.Columns.Add("SHOPID", typeof(string));
                                INDATA.Columns.Add("AREAID", typeof(string));
                                INDATA.Columns.Add("EQSGID", typeof(string));
                                INDATA.Columns.Add("PROCID", typeof(string));
                                INDATA.Columns.Add("EQPTID", typeof(string));
                                INDATA.Columns.Add("SHFT_ID", typeof(string));
                                INDATA.Columns.Add("SHFT_USER_CLSS_CODE", typeof(string));
                                INDATA.Columns.Add("WRK_USERID", typeof(string));
                                INDATA.Columns.Add("FROMDATE", typeof(string));
                                INDATA.Columns.Add("TODATE", typeof(string));
                                INDATA.Columns.Add("USERID", typeof(string));
                                //2018.07.17
                                INDATA.Columns.Add("PRE_WRK_USERID", typeof(string));
                                INDATA.Columns.Add("TIME_FLAG", typeof(string));
                                INDATA.Columns.Add("FROM_TIME");
                                INDATA.Columns.Add("TO_TIME");

                                DataTable dtWorker = DataTableConverter.Convert(dgWorkerList.ItemsSource);

                                for (int irow = 0; irow < dtWorker.Rows.Count; irow++)
                                {
                                    DataRow drINDATA = null;
                                    drINDATA = INDATA.NewRow();

                                    drINDATA["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                                    drINDATA["LANGID"] = LoginInfo.LANGID;
                                    drINDATA["SHOPID"] = dtWorker.Rows[irow][1].ToString();
                                    drINDATA["AREAID"] = dtWorker.Rows[irow][3].ToString();
                                    drINDATA["EQSGID"] = dtWorker.Rows[irow][5].ToString();
                                    //drINDATA["PROCID"] = dtWorker.Rows[irow][7].ToString().Length > 0 ? dtWorker.Rows[irow][7].ToString() :  "ALL"; //dtWorker.Rows[irow][7].ToString();
                                    //drINDATA["EQPTID"] = dtWorker.Rows[irow][9].ToString().Length > 0 ? dtWorker.Rows[irow][9].ToString() : "ALL";  //dtWorker.Rows[irow][9].ToString();
                                    switch (dtWorker.Rows[irow][13].ToString())
                                    {
                                        case "SUC_01":
                                            drINDATA["PROCID"] = "S1";
                                            drINDATA["EQPTID"] = "S1";
                                            break;
                                        case "SUC_02":
                                            drINDATA["PROCID"] = "S2";
                                            drINDATA["EQPTID"] = "S2";
                                            break;
                                        //2018.07.17
                                        case "SUC_04":
                                            drINDATA["PROCID"] = "S4";
                                            drINDATA["EQPTID"] = "S4";
                                            break;
                                        //2018.07.17
                                        case "SUC_03":
                                            drINDATA["PROCID"] = dtWorker.Rows[irow][7].ToString();
                                            drINDATA["EQPTID"] = dtWorker.Rows[irow][9].ToString();
                                            break;
                                    }
                                    drINDATA["SHFT_ID"] = dtWorker.Rows[irow][11].ToString();
                                    drINDATA["SHFT_USER_CLSS_CODE"] = dtWorker.Rows[irow][13].ToString();
                                    drINDATA["WRK_USERID"] = dtWorker.Rows[irow][15].ToString();
                                    drINDATA["FROMDATE"] = dtpWorkStartDay.SelectedDateTime.ToString("yyyyMMdd");
                                    drINDATA["TODATE"] = dtpWorkEndDay.SelectedDateTime.ToString("yyyyMMdd");
                                    drINDATA["USERID"] = LoginInfo.USERID;
                                    //2018.07.17
                                    //if ((bool)chkTime.IsChecked)
                                    //{
                                    drINDATA["PRE_WRK_USERID"] = Util.NVC(dtWorker.Rows[irow][19].ToString());
                                    drINDATA["TIME_FLAG"] = dtWorker.Rows[irow][21].ToString();
                                        
                                    if (dtWorker.Rows[irow][21].ToString() == "Y")
                                    {
                                        string sStartTime = Util.NVC(dgWorkerList.GetCell(irow, dgWorkerList.Columns["FROM_TIME"].Index).Value);
                                        string sEndTime = Util.NVC(dgWorkerList.GetCell(irow, dgWorkerList.Columns["TO_TIME"].Index).Value);
                                        drINDATA["FROM_TIME"] = (DateTime.Parse(sStartTime)).ToString("HH:mm:ss"); 
                                        drINDATA["TO_TIME"] = (DateTime.Parse(sEndTime)).ToString("HH:mm:ss");
                                    }
                                    else
                                    {
                                        drINDATA["TIME_FLAG"] = "N";
                                        drINDATA["FROM_TIME"] = null;
                                        drINDATA["TO_TIME"] = null;
                                    }
                                    //}
                                    //else
                                    //{
                                    //    drINDATA["FRE_USERID"] = null;
                                   //     drINDATA["TIME_FLAG"] = "N";
                                   //     drINDATA["FROM_TIME"] = null;
                                   //     drINDATA["TO_TIME"] = null;
                                    //}

                                    //    drINDATA["TIME_FLAG"] = "Y";

                                    //    string sStartTime = Util.NVC(dgTimeList.GetCell(0, dgTimeList.Columns["SHFT_STRT_HMS"].Index).Value);
                                    //    string sEndTime = Util.NVC(dgTimeList.GetCell(0, dgTimeList.Columns["SHFT_END_HMS"].Index).Value);
                                    //    drINDATA["FROM_TIME"] = (DateTime.Parse(sStartTime)).ToString("HH:mm:ss");
                                    //    drINDATA["TO_TIME"] = (DateTime.Parse(sEndTime)).ToString("HH:mm:ss");


                                    INDATA.Rows.Add(drINDATA);

                                    //2018.07.17
                                    //dsInput.Tables.Add(INDATA);

                                    //DataTable dtTIME = new DataTable();
                                    //dtTIME.TableName = "TIME";
                                    //dtTIME.Columns.Add("SHFT_ID");
                                    //dtTIME.Columns.Add("FROM_TIME");
                                    //dtTIME.Columns.Add("TO_TIME");

                                    //for (int i = 0; i < dgTimeList.Rows.Count; i++)
                                    //{
                                    //    //string sStartTime = Util.NVC(dgTimeList.GetCell(i, dgTimeList.Columns["SHFT_STRT_HMS"].Index).Value);
                                    //    //string sEndTime = Util.NVC(dgTimeList.GetCell(i, dgTimeList.Columns["SHFT_END_HMS"].Index).Value);
                                    //    DataRow drTIME = null;
                                    //    drTIME = dtTIME.NewRow();
                                    //    drTIME["SHFT_ID"] = Util.NVC(dgTimeList.GetCell(i, dgTimeList.Columns["CBO_CODE"].Index).Value);
                                    //    drTIME["FROM_TIME"] = (DateTime.Parse(Util.NVC(dgTimeList.GetCell(i, dgTimeList.Columns["SHFT_STRT_HMS"].Index).Value))).ToString("HH:mm:ss");
                                    //    drTIME["TO_TIME"] = (DateTime.Parse(Util.NVC(dgTimeList.GetCell(i, dgTimeList.Columns["SHFT_END_HMS"].Index).Value))).ToString("HH:mm:ss");
                                    //    dtTIME.Rows.Add(drTIME);
                                    //}
                                    //dsInput.Tables.Add(dtTIME);

                                    DataTable dtResult = new ClientProxy().ExecuteServiceSync("BR_PRD_REG_TB_SFC_CALDATE_WRKR", "INDATA", null, INDATA);
                                    //new ClientProxy().ExecuteServiceSync_Multi("BR_PRD_REG_TB_SFC_CALDATE_WRKR", "INDATA,TIME", "", dsInput, null);
                                    //2018.07.17

                                    INDATA.Clear();
                                }

                                dgWorkerList.ItemsSource = null;

                                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("9003"), null, "Information", MessageBoxButton.OK, MessageBoxIcon.None, (result4) =>
                                {
                                    // Util.AlertInfo("9003");  //저장이 완료되었습니다.
                                    if (result4 == MessageBoxResult.OK)
                                    {
                                        this.Close();
                                    }
                                });

                                //if (IndataTable.Rows.Count !=0 )
                                //{
                                //    new ClientProxy().ExecuteService("BR_PRD_REG_TB_MMD_CALDATE_SHFT", "INDATA", null, IndataTable, (result, ex) =>
                                //    {
                                //        loadingIndicator.Visibility = Visibility.Collapsed;

                                //        if (ex != null)
                                //        {
                                //            LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex.Message), null, "Info", MessageBoxButton.OK, MessageBoxIcon.None);
                                //            return;
                                //        }

                                //        Util.AlertInfo("SFU1275");  //정상처리되었습니다.
                                //        //this.dgExcleload.ItemsSource = null;
                                //    });
                                //}
                                //else
                                //{
                                //    Util.Alert("SFU1278");  //처리 할 항목이 없습니다.
                                //    return;
                                //}
                            }
                        });
                    //}
                //} // For

            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex.Message), null, "Error", MessageBoxButton.OK, MessageBoxIcon.None);
            }
            
        }

        #region [요청자]
        private void GetUserWindow()
        {
            //2018.12.03
            //CMM_PERSON wndPerson = new CMM_PERSON();
            //wndPerson.FrameOperation = FrameOperation;

            //if (wndPerson != null)
            //{
            //    object[] Parameters = new object[1];
            //    Parameters[0] = txtUserNameCr.Text;
            //    C1WindowExtension.SetParameters(wndPerson, Parameters);

            //    wndPerson.Closed += new EventHandler(wndUser_Closed);
            //    Search.Children.Add(wndPerson);
            //    wndPerson.BringToFront();
            //}
        }
        private void wndUser_Closed(object sender, EventArgs e)
        {
            //2018.12.03
            //CMM_PERSON wndPerson = sender as CMM_PERSON;
            //if (wndPerson.DialogResult == MessageBoxResult.OK)
            //{
            //    txtUserNameCr.Text = wndPerson.USERNAME;
            //    txtUserID.Text = wndPerson.USERID;
            //}
            //else
            //{
            //    txtUserNameCr.Text = string.Empty;
            //    txtUserID.Text = string.Empty;
            //}
        }

        //2018.07.17
        private void GetFreUserWindow()
        {
            //2018.12.03
            //CMM_PERSON wndPerson = new CMM_PERSON();
            //wndPerson.FrameOperation = FrameOperation;

            //if (wndPerson != null)
            //{
            //    object[] Parameters = new object[1];
            //    Parameters[0] = txtFreUserNameCr.Text;
            //    C1WindowExtension.SetParameters(wndPerson, Parameters);

            //    wndPerson.Closed += new EventHandler(wndFreUser_Closed);
            //    Search.Children.Add(wndPerson);
            //    wndPerson.BringToFront();
            //}
        }

        private void wndFreUser_Closed(object sender, EventArgs e)
        {
            //2018.12.03
            //CMM_PERSON wndPerson = sender as CMM_PERSON;
            //if (wndPerson.DialogResult == MessageBoxResult.OK)
            //{
            //    txtFreUserNameCr.Text = wndPerson.USERNAME;
            //    txtFreUserID.Text = wndPerson.USERID;
            //}
            //else
            //{
            //    txtFreUserNameCr.Text = string.Empty;
            //    txtFreUserID.Text = string.Empty;
            //}
        }
        #endregion [요청자]
        #endregion
    }
}
