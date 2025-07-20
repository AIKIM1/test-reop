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
using System.Collections.Generic;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace LGC.GMES.MES.BOX001
{
    public partial class BOX001_002 : UserControl, IWorkArea
    {
        Util _Util = new Util();

        #region Declaration & Constructor 

        public BOX001_002()
        {
            InitializeComponent();

            //Initialize();

            this.Loaded += Window_Loaded;
        }

        public IFrameOperation FrameOperation
        {
            get;
            set;
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            List<Button> listAuth = new List<Button>();
            listAuth.Add(btnAdd);
            listAuth.Add(btnDelete);
            Util.pageAuth(listAuth, FrameOperation.AUTHORITY);

            SetEvent();
        }

        private void SetEvent()
        {
            this.Loaded -= UserControl_Loaded;
            dtpDateFrom.SelectedDataTimeChanged += dtpDateFrom_SelectedDataTimeChanged;
            dtpDateTo.SelectedDataTimeChanged += dtpDateTo_SelectedDataTimeChanged;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {

            Initialize();

            this.Loaded -= Window_Loaded;            
        }

        #endregion

        #region Initialize
        private void Initialize()
        {
            dgAdd.Loaded += dgAdd_Loaded;

            dtpDateFrom.SelectedDateTime = (DateTime)System.DateTime.Now;
            //dtpDateFrom.Text = DateTime.Now.AddDays(-7).ToString("yyyy-MM-dd");
            dtpDateTo.SelectedDateTime = (DateTime)System.DateTime.Now;

            //Initialize_dgAdd();
            //Init_Form();
        }
        #endregion

        private void Initialize_dgAdd()
        {
            //DataTable dtRANID_ADD = new DataTable();
            //DataRow newRow = null;

            //dtRANID_ADD = new DataTable();
            //dtRANID_ADD.Columns.Add("NUMBER", typeof(string));
            //dtRANID_ADD.Columns.Add("RAN_ID", typeof(string));
            //dtRANID_ADD.Columns.Add("INDATE", typeof(string));
            //dtRANID_ADD.Columns.Add("ELECTRODE", typeof(string));

            //List<object[]> list_RAN = new List<object[]>();

            //for (int i = 1; i < 301; i++)
            //{
            //    list_RAN.Add(new object[] { i, "", "", "" });
            //}
            ////list_RAN.Add(new object[] { "", "", "", "" });
            ////list_RAN.Add(new object[] { "", "", "", "" });

            //foreach (object[] item in list_RAN)
            //{
            //    newRow = dtRANID_ADD.NewRow();
            //    newRow.ItemArray = item;
            //    dtRANID_ADD.Rows.Add(newRow);
            //}

            //dgAdd.BeginEdit();
            //dgAdd.ItemsSource = DataTableConverter.Convert(dtRANID_ADD);
            //dgAdd.EndEdit();

            ////SetGridCboItem(dgAdd.Columns["ELECTRODE"]);

            Util.gridClear(dgAdd);
            dgAdd.ItemsSource = null;

            DataTable dt = new DataTable();
            dt.Columns.Add("NUMBER", typeof(string));
            dt.Columns.Add("RAN_ID", typeof(string));
            dt.Columns.Add("INDATE", typeof(string));
            dt.Columns.Add("ELECTRODE", typeof(string));

            DataRow row = dt.NewRow();
            row["NUMBER"] = "";
            row["RAN_ID"] = "";
            row["INDATE"] = "";
            row["ELECTRODE"] = "";

            dt.Rows.Add(row);

            dgAdd.Columns["NUMBER"].Visibility = Visibility.Collapsed;

            dgAdd.BeginEdit();
            dgAdd.ItemsSource = DataTableConverter.Convert(dt);
            dgAdd.EndEdit();

        }


        #region Mehod
        private void SetGridCboItem(C1.WPF.DataGrid.DataGridColumn col)
        {
            DataTable dt = new DataTable();
            dt.Columns.Add("CODE");
            dt.Columns.Add("ELECTRODE");

            DataRow newRow = null;

            newRow = dt.NewRow();
            newRow.ItemArray = new object[] { "A", "음극[A]" };
            dt.Rows.Add(newRow);

            newRow = dt.NewRow();
            newRow.ItemArray = new object[] { "C", "양극[C]" };
            dt.Rows.Add(newRow);

            //(col as C1.WPF.DataGrid.DataGridComboBoxColumn).ItemsSource = DataTableConverter.Convert(dt);


            //DataTable dResult = new DataTable();
            //dResult.Columns.Add("CBO_CODE");
            //dResult.Columns.Add("CBO_NAME");

            //dResult.Rows.Add("Y", "Y");
            //dResult.Rows.Add("N", "N");

            (dgAdd.Columns["ELECTRODE"] as C1.WPF.DataGrid.DataGridComboBoxColumn).ItemsSource = DataTableConverter.Convert(dt);

        }

        private void dtpDateFrom_SelectedDataTimeChanged(object sender, SelectionChangedEventArgs e)
        {
            LGCDatePicker dtPik = sender as LGCDatePicker;
            if (Convert.ToDecimal(dtpDateTo.SelectedDateTime.ToString("yyyyMMdd")) < Convert.ToDecimal(dtPik.SelectedDateTime.ToString("yyyyMMdd")))
            {
                dtPik.SelectedDateTime = dtpDateTo.SelectedDateTime;
                return;
            }
        }

        private void dtpDateTo_SelectedDataTimeChanged(object sender, SelectionChangedEventArgs e)
        {
            LGCDatePicker dtPik = sender as LGCDatePicker;
            if (Convert.ToDecimal(dtpDateFrom.SelectedDateTime.ToString("yyyyMMdd")) > Convert.ToDecimal(dtPik.SelectedDateTime.ToString("yyyyMMdd")))
            {
                dtPik.SelectedDateTime = dtpDateFrom.SelectedDateTime;
                return;
            }
        }
        #endregion

        #region Button Event
        private void btnAdd_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                int iCnt = 0;


                if (DataTableConverter.GetValue(dgAdd.Rows[0].DataItem, "RAN_ID").ToString() == "")
                {
                    Util.MessageValidation("SFU2052"); //입력된 항목이 없습니다.
                    return;
                }

                for (int i = 0; i < dgAdd.GetRowCount(); i++)
                {
                    if (DataTableConverter.GetValue(dgAdd.Rows[i].DataItem, "RAN_ID").ToString() != "")
                    {
                        if (DataTableConverter.GetValue(dgAdd.Rows[i].DataItem, "INDATE").ToString() == "")
                        {
                            Util.MessageValidation("SFU2053"); //날짜가 입력되지 않았습니다.
                            return;
                        }
                        else if (DataTableConverter.GetValue(dgAdd.Rows[i].DataItem, "ELECTRODE").ToString() == "")
                        {
                            Util.MessageValidation("SFU2054"); //극성이 입력되지 않았습니다.
                            return;
                        }

                        DateTime ProdDate = new DateTime();

                        //if (DataTableConverter.GetValue(dgAdd.Rows[i].DataItem, "INDATE").ToString().Contains("."))
                        //{
                        //    ProdDate = DateTime.ParseExact(DataTableConverter.GetValue(dgAdd.Rows[i].DataItem, "INDATE").ToString().Replace(".", ""), "yyyyMMdd", System.Globalization.CultureInfo.InvariantCulture);
                        //}
                        //else if (DataTableConverter.GetValue(dgAdd.Rows[i].DataItem, "INDATE").ToString().Contains("-"))
                        //{
                        //    ProdDate = DateTime.ParseExact(DataTableConverter.GetValue(dgAdd.Rows[i].DataItem, "INDATE").ToString().Replace("-", ""), "yyyyMMdd", System.Globalization.CultureInfo.InvariantCulture);
                        //}
                        //else
                        //{
                        //    ProdDate = DateTime.ParseExact(DataTableConverter.GetValue(dgAdd.Rows[i].DataItem, "INDATE").ToString().Replace(" ", ""), "yyyyMMdd", System.Globalization.CultureInfo.InvariantCulture);
                        //}

                        if (CommonVerify.IsValidDateTime(DataTableConverter.GetValue(dgAdd.Rows[i].DataItem, "INDATE").ToString()))
                        {
                            ProdDate = DateTime.ParseExact(DataTableConverter.GetValue(dgAdd.Rows[i].DataItem, "INDATE").ToString(), "yyyyMMdd", System.Globalization.CultureInfo.InvariantCulture);
                        }
                        else
                        {
                            //DATE 형식이 맞지 않습니다. [날짜 형식(YYYYMMDD)이 맞지 않습니다.]\r\n [DATE : %1]
                            Util.MessageValidation("SFU3241" + DataTableConverter.GetValue(dgAdd.Rows[i].DataItem, "INDATE").ToString());
                            return;
                        }

                        //ProdDate = DateTime.ParseExact(DataTableConverter.GetValue(dgAdd.Rows[i].DataItem, "INDATE").ToString(), "yyyyMMdd", System.Globalization.CultureInfo.InvariantCulture);
                        string sProdDate = ProdDate.Year.ToString() + ProdDate.Month.ToString("00") + ProdDate.Day.ToString("00");

                        string sRANID = Util.NVC(DataTableConverter.GetValue(dgAdd.Rows[i].DataItem, "RAN_ID").ToString());
                        string sElectrode = Util.NVC(DataTableConverter.GetValue(dgAdd.Rows[i].DataItem, "ELECTRODE").ToString().ToUpper());

                        if (sElectrode != "A" && sElectrode != "C")
                        {
                            //극성이 잘못 입력되었습니다.[극성 형식(A/C)이 맞지 않습니다.]\r\n [극성 : %1]
                            Util.MessageValidation("SFU3242", DataTableConverter.GetValue(dgAdd.Rows[i].DataItem, "ELECTRODE").ToString());
                            return;
                        }

                        DataTable RQSTDT1 = new DataTable();
                        RQSTDT1.TableName = "RQSTDT";
                        RQSTDT1.Columns.Add("RAN_ID", typeof(string));

                        DataRow dr1 = RQSTDT1.NewRow();
                        dr1["RAN_ID"] = Util.NVC(DataTableConverter.GetValue(dgAdd.Rows[i].DataItem, "RAN_ID").ToString());
                        RQSTDT1.Rows.Add(dr1);

                        DataTable DupChk = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_RANID_DUP", "RQSTDT", "RSLTDT", RQSTDT1);

                        if (Convert.ToInt32(DupChk.Rows[0]["CNT"].ToString()) == 0)
                        {
                            DataTable RQSTDT = new DataTable();
                            RQSTDT.TableName = "RQSTDT";
                            RQSTDT.Columns.Add("RAN_ID", typeof(string));
                            RQSTDT.Columns.Add("ELTR_TYPE_CODE", typeof(string));
                            RQSTDT.Columns.Add("RAN_DATE", typeof(string));
                            RQSTDT.Columns.Add("USERID", typeof(string));

                            DataRow dr = RQSTDT.NewRow();
                            dr["RAN_ID"] = Util.NVC(DataTableConverter.GetValue(dgAdd.Rows[i].DataItem, "RAN_ID").ToString());
                            dr["ELTR_TYPE_CODE"] = Util.NVC(DataTableConverter.GetValue(dgAdd.Rows[i].DataItem, "ELECTRODE").ToString().ToUpper());
                            //dr["RAN_DATE"] = Util.NVC(DataTableConverter.GetValue(dgAdd.Rows[i].DataItem, "INDATE").ToString().Replace("-", ""));
                            dr["RAN_DATE"] = sProdDate;
                            dr["USERID"] = LoginInfo.USERID;
                            RQSTDT.Rows.Add(dr);

                            DataTable insert = new ClientProxy().ExecuteServiceSync("DA_PRD_INS_RANID", "RQSTDT", "RSLTDT", RQSTDT);

                            iCnt = i + 1;
                        }
                        else
                        {
                            //저장된 RAN ID 입니다.\r\n [RAN_ID : %1]
                            Util.MessageValidation("SFU2994", sRANID);

                            //LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("저장된 RAN ID 입니다." + "[ " + sRANID + " ]"), null, "Info", MessageBoxButton.OK, MessageBoxIcon.Warning);
                            return;
                        }
                    }
                    //else
                    //{
                    //    LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("입력된 항목이 없습니다."), null, "Info", MessageBoxButton.OK, MessageBoxIcon.Warning);
                    //    return;
                    //}
                }

                if (iCnt > 0)
                {
                    //LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(iCnt + "개가 정상 처리 되었습니다."), null, "Info", MessageBoxButton.OK, MessageBoxIcon.Warning);
                    //Util.Alert("SFU2056", new object[] { iCnt });    //메세지내용
                    Util.MessageInfo("SFU2056", iCnt);
                    Util.gridClear(dgAdd);
                    Initialize_dgAdd();

                    Search_Date();
                }
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
                return;
            }
        }

        private void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            if(_Util.GetDataGridCheckCnt(dgWait, "CHK") == 0)
            {
                Util.MessageValidation("SFU1191");//삭제할 LOT ID 를 선택하세요.
                return;
            }

            //삭제 하시겠습니까?
            LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("SFU1230"), null, "Information", MessageBoxButton.OKCancel, MessageBoxIcon.None, (result) =>
            {
                if (result == MessageBoxResult.OK)
                {

                    DataTable RQSTDT = new DataTable();
                    RQSTDT.TableName = "RQSTDT";
                    RQSTDT.Columns.Add("RAN_ID", typeof(string));

                    for (int i = 0; i < dgWait.GetRowCount() ; i++)
                    {
                        //if ((dgWait.GetCell(i, dgWait.Columns["CHK"].Index).Presenter.Content as CheckBox).IsChecked.HasValue &&
                        //    (bool)(dgWait.GetCell(i, dgWait.Columns["CHK"].Index).Presenter.Content as CheckBox).IsChecked)
                        //{
                        //    DataRow dr = RQSTDT.NewRow();                          
                        //    dr["RAN_ID"] = Util.NVC(DataTableConverter.GetValue(dgWait.Rows[i].DataItem, "RAN_ID").ToString());
                        //    RQSTDT.Rows.Add(dr);
                        //}

                        if (Util.NVC(DataTableConverter.GetValue(dgWait.Rows[i].DataItem, "CHK")) == "True" )
                        {
                            DataRow dr = RQSTDT.NewRow();
                            dr["RAN_ID"] = Util.NVC(DataTableConverter.GetValue(dgWait.Rows[i].DataItem, "RAN_ID").ToString());
                            RQSTDT.Rows.Add(dr);
                        }

                    }


                    //DataTable DelResult = new ClientProxy().ExecuteServiceSync("DA_PRD_DEL_RANID", "RQSTDT", "RSLTDT", RQSTDT);                  

                    //LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("정상 처리 되었습니다."), null, "Info", MessageBoxButton.OK, MessageBoxIcon.Warning);

                    new ClientProxy().ExecuteService("DA_PRD_DEL_RANID", "RQSTDT", "RSLTDT", RQSTDT, (searchResult, searchException) =>
                    {
                        try
                        {
                            if (searchException != null)
                            {
                                Util.AlertByBiz("DA_PRD_DEL_RANID", searchException.Message, searchException.ToString());
                                return;
                            }

                            Util.MessageInfo("SFU1275"); //정상처리되었습니다.

                            Search_Date();
                        }
                        catch (Exception ex)
                        {
                            LGC.GMES.MES.ControlsLibrary.MessageBox.ShowKeyEnter(ex.Message, ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
                        }
                    }
                    );

                }
                else
                {
                    return;
                }
            });
        }

        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            Search_Date();
        }
        #endregion

        private void Search_Date()
        {
            try
            {
                //string sStartdate = dtpDateFrom.ToString();
                //string sEnddate = dtpDateTo.ToString();

                string sStartdate = string.Format("{0:yyyyMMdd}", dtpDateFrom.SelectedDateTime);
                string sEnddate = string.Format("{0:yyyyMMdd}", dtpDateTo.SelectedDateTime);

                // 미완료 작업
                // 이전 그리드 내용 초기화
                // 날짜 형식 맞추기
                // 조회 Biz 내용 수정

                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("STARTDATE", typeof(string));
                RQSTDT.Columns.Add("ENDDATE", typeof(string));
                RQSTDT.Columns.Add("LANGID", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["STARTDATE"] = sStartdate;
                dr["ENDDATE"] = sEnddate;
                dr["LANGID"] = LoginInfo.LANGID;
                RQSTDT.Rows.Add(dr);

                DataTable WaitResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_RANID_WAITLIST", "RQSTDT", "RSLTDT", RQSTDT);

                //dgWait.ItemsSource = DataTableConverter.Convert(WaitResult);
                Util.GridSetData(dgWait, WaitResult, FrameOperation);

                DataTable UsedResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_RANID_USEDLIST", "RQSTDT", "RSLTDT", RQSTDT);

                //dgUsed.ItemsSource = DataTableConverter.Convert(UsedResult);
                Util.GridSetData(dgUsed, UsedResult, FrameOperation);
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
                return;
            }
        }

        private void btnDelete2_Click(object sender, RoutedEventArgs e)
        {
            //삭제 하시겠습니까?
            LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("SFU1230"), null, "Information", MessageBoxButton.OKCancel, MessageBoxIcon.Warning, (result) =>
            {
                if (result == MessageBoxResult.OK)
                {
                    int index = ((C1.WPF.DataGrid.DataGridCellPresenter)((sender as Button).Parent)).Row.Index;

                    dgAdd.IsReadOnly = false;
                    dgAdd.RemoveRow(index);
                    dgAdd.IsReadOnly = true;

                }
            });
        }

        private void btnRefresh_Click(object sender, RoutedEventArgs e)
        {
            Init_Form();
        }

        private void Init_Form()
        {
            Util.gridClear(dgAdd);

            Initialize_dgAdd();
        }

        private void dgAdd_Loaded(object sender, RoutedEventArgs e)
        {
            dgAdd.Loaded -= dgAdd_Loaded;
            C1.WPF.DataGrid.C1DataGrid dataGrid = sender as C1.WPF.DataGrid.C1DataGrid;
            dataGrid.Dispatcher.BeginInvoke(new Action(() =>
            {
                Initialize_dgAdd();
                //dgAdd.ScrollIntoView(0, 0);
                //dgAdd.SelectedIndex = 0;
            }));
        }

        private void Grid_Loaded(object sender, RoutedEventArgs e)
        {
            dgAdd.Loaded -= dgAdd_Loaded;
            C1.WPF.DataGrid.C1DataGrid dataGrid = sender as C1.WPF.DataGrid.C1DataGrid;
            dataGrid.Dispatcher.BeginInvoke(new Action(() =>
            {
                Initialize_dgAdd();
            }));
        }

        private void dgAdd_KeyDown(object sender, KeyEventArgs e)
        {
            try
            {
                if (e.Key == Key.V && KeyboardUtil.Ctrl)
                {
                    DataTable dt = DataTableConverter.Convert(dgAdd.ItemsSource);

                    for (int i = dgAdd.GetRowCount() - 1; i >= 0; i--)
                    {
                        dt.Rows[i].Delete();
                    }

                    string text = Clipboard.GetText();
                    string[] table = text.Split('\n');

                    if (table == null)
                    {
                        Util.Alert("SFU1482");   //다시 복사 해주세요.
                        Initialize_dgAdd();
                        return;
                    }
                    if (table.Length == 1)
                    {
                        Util.Alert("SFU1482");   //다시 복사 해주세요.
                        Initialize_dgAdd();
                        return;
                    }


                    for (int i = 0; i < table.Length - 1; i++)
                    {
                        string[] rw = table[i].Split('\t');
                        if (rw == null)
                        {
                            Util.Alert("SFU1498");   //데이터가 없습니다.
                            return;
                        }
                        if (rw.Length != 3)
                        {
                            Util.Alert("SFU1532");   //모든 항목을 다 복사해주세요.
                            Initialize_dgAdd();
                            return;
                        }
                        DataRow row = dt.NewRow();
                        row["NUMBER"] = i + 1;
                        row["RAN_ID"] = rw[0].ToUpper();

                        if (CommonVerify.IsValidDateTime(rw[1]))
                        {
                            row["INDATE"] = rw[1];
                        }
                        else
                        {
                            //DATE 형식이 맞지 않습니다. [날짜 형식(YYYYMMDD)이 맞지 않습니다.]\r\n [DATE : %1]
                            Initialize_dgAdd();
                            Util.MessageValidation("SFU3241",  rw[1] );                            
                            return;
                        }


                        //Util.MessageInfo("SFU1700", sTmp);
                        //if ( rw[1].Contains("-"))
                        //    rw[1] = rw[1].Replace("-", "");

                        //if ( rw[1].Contains("."))
                        //    rw[1] = rw[1].Replace(".", "");

                        //if (rw[1].Contains(" "))
                        //    rw[1] = rw[1].Replace(" ", "");                      

                        //if (rw[1].Length != 8)
                        //{
                        //    Util.Alert("DATE 포멧이 맞지 않습니다. [YYYYMMDD 형식으로 입력해주세요.]" + "\r\n" + "[" + rw[1] + "]" );
                        //    Init_Form();
                        //    return;
                        //}

                        row["INDATE"] = rw[1];


                        if (rw[2].ToUpper().Substring(0, 1) != "A" && rw[2].ToUpper().Substring(0, 1) != "C")
                        {
                            //극성이 잘못 입력되었습니다.[극성 형식(A/C)이 맞지 않습니다.]\r\n [극성 : %1]
                            Initialize_dgAdd();
                            Util.MessageInfo("SFU3242",rw[2]);
                            return;
                        }

                        row["ELECTRODE"] = rw[2].ToUpper();

                        dt.Rows.Add(row);
                    }

                    dgAdd.BeginEdit();
                    dgAdd.ItemsSource = DataTableConverter.Convert(dt);
                    dgAdd.EndEdit();

                    dgAdd.Columns["NUMBER"].Visibility = Visibility.Visible;

                    //FrameOperation.PrintFrameMessage(dgAdd.Rows.Count + "건");
                }
            }
            catch (Exception ex)
            {
                Util.Alert(ex.ToString());
                Initialize_dgAdd();
            }
        }
    }
}
