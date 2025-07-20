/*************************************************************************************
 Created Date : 2016.06.16
      Creator : 
   Decription : 
--------------------------------------------------------------------------------------
 [Change History]
  2016.06.16  DEVELOPER : Initial Created.





 
**************************************************************************************/

using System;
using System.Collections;
using System.Data;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using LGC.GMES.MES.Common;

namespace LGC.GMES.MES.ProtoType03
{
    public partial class delta01 : UserControl
    {
        Hashtable hash_loss_color = new Hashtable();
        //Grid _grid = new Grid();

        DataTable dtMainList = new DataTable();
        //GridUtil.AddNumberColumn(spd, "CHK", "CHK", 50, 0, false, false);
        //GridUtil.AddTextColumn(spd, "FACTORYCODE", "FACTORYCODE", 50, 0, CharacterCasing.Normal, GridHorizontalAlignment.Center, false);
        //GridUtil.AddTextColumn(spd, "EQPTID", "EQPTID", 50, 0, CharacterCasing.Normal, GridHorizontalAlignment.Center, false);
        ////---GridUtil.AddTextColumn(spd, "EQPTNAME", "설비명", 150, 30, CharacterCasing.Normal, GridHorizontalAlignment.Center, false);
        //GridUtil.AddTextColumn(spd, "START_TIME", "시작시간", 100, 30, CharacterCasing.Normal, GridHorizontalAlignment.Center, false);
        //GridUtil.AddTextColumn(spd, "END_TIME", "종료시간", 100, 30, CharacterCasing.Normal, GridHorizontalAlignment.Center, false);
        //GridUtil.AddTextColumn(spd, "ACTID", "상태", 50, 30, false);
        //GridUtil.AddTextColumn(spd, "RESNCODE", "Trouble", 90, 30, CharacterCasing.Normal, GridHorizontalAlignment.Center, false);
        ////---GridUtil.AddTextColumn(spd, "RESNNAME", "Trouble명", 180, 30, CharacterCasing.Normal, GridHorizontalAlignment.Center, false);
        ////---GridUtil.AddTextColumn(spd, "LOSSREMARK", "LOSS비고", 180, 30, CharacterCasing.Normal, GridHorizontalAlignment.Center, false);
        //GridUtil.AddTextColumn(spd, "HIDDEN_START", "HIDDEN_START", 180, 30, CharacterCasing.Normal, GridHorizontalAlignment.Center, false);
        //GridUtil.AddTextColumn(spd, "HIDDEN_END", "HIDDEN_END", 180, 30, CharacterCasing.Normal, GridHorizontalAlignment.Center, false);

        
        #region Declaration & Constructor 
        public delta01()
        {
            InitializeComponent();
            //DocView.Document = this.ConvertExcelToXPSDoc("","").

            _grid.Width = 1200;

            _grid.Height = 30 * 15;

            InitGrid();

            GetLossColor();
            

        }

        #endregion

        #region Initialize
        private void InitGrid()
        {
            try
            {

                //_grid.Children.Clear();
                //_grid.ColumnDefinitions.Clear();
                //_grid.RowDefinitions.Clear();


                for (int i = 0; i < 361; i++)
                {
                    ColumnDefinition gridCol1 = new ColumnDefinition();
                    if (i == 0)
                    {
                        gridCol1.Width = new GridLength(120);
                    }
                    else { gridCol1.Width = new GridLength(3); }

                    _grid.ColumnDefinitions.Add(gridCol1);

                }

                for (int i = 0; i < 30; i++)
                {
                    RowDefinition gridRow1 = new RowDefinition();
                    gridRow1.Height = new GridLength(15);
                    _grid.RowDefinitions.Add(gridRow1);

                }

            }
            catch (Exception ex)
            {
                throw ex;
                //commMessage.Show(ex.Message);
            }
        }
        #endregion

        #region Event
        private void button_Click(object sender, RoutedEventArgs e)
        {

            ClearGrid();

            GetEqptLossRawList();

            SelectProcess();
            GetEqptLossDetailList();

            //DataTable RQSTDT = new DataTable();
            //RQSTDT.TableName = "RQSTDT";
            //RQSTDT.Columns.Add("EQPTID", typeof(string));
            //RQSTDT.Columns.Add("JOBDATE", typeof(string));
            //RQSTDT.Columns.Add("SHIFTCODE", typeof(string));

            //DataRow dr = RQSTDT.NewRow();
            //dr["EQPTID"] = "N1ANTC401";
            //dr["JOBDATE"] = "20160617";
            //dr["SHIFTCODE"] = "0";
            //RQSTDT.Rows.Add(dr);

            //DataTable dt2 = new DataTable();
            //new ClientProxy().ExecuteServiceSync("R_EQPTLOSS_LIST_RAW_SP", "RQSTDT", "OUTDATA", RQSTDT, (dt, ex) =>
            //    {


            //        dt2 = dt;
            //    }
            //);
            //MessageBox.Show(dt2.Rows.Count.ToString());
        }
        #endregion

        #region Mehod

        #endregion

        private void GetLossColor()
        {
            new ClientProxy("10.42.68.67", "tcp", "7866", "SERVICE", "1").ExecuteService("R_LOSS_COLOR", null, "RSLTDT", null, (dt, ex) =>
                {

                    hash_loss_color = DataTableConverter.ToHash(dt);
                }
            );
        }


        private void ClearGrid()
        {
            try
            {
                foreach (Border _border in _grid.Children.OfType<Border>())
                {
                    _grid.UnregisterName(_border.Name);
                }

                NameScope.SetNameScope(_grid, new NameScope());

                _grid.Children.Clear();
            }
            catch (Exception ex)
            {
                throw ex;
                //commMessage.Show(ex.Message);
            }
        }


        private DataSet GetEqptTimeList(string sJobDate, string sShiftCode)
        {
            //DataSet ds = null;

            //try
            //{
            //    Biz.PARA.AssyPara para = new Biz.PARA.AssyPara();
            //    para.JobDate = sJobDate;
            //    para.Shift = sShiftCode;
            //    ds = Biz.ASSY.AssyEqptLoss.GetEQPTLoss_TimeList(para);

            //    if (ds.Tables["RSLTDT"].Rows.Count <= 0)
            //    {
            //    }
            //    return ds;
            //}
            //catch (Exception ex)
            //{
            //    commMessage.Show(ex.Message);
            //    return ds;
            //}

            DataSet ds = new DataSet();

            try
            {
                DataTable RSLTDT = new DataTable("RSLTDT");

                RSLTDT.Columns.Add("STARTTIME", typeof(string));
                RSLTDT.Columns.Add("ENDTIME", typeof(string));

                int iTerm = 0;
                int iIncrease = 0;

                DateTime dJobDate = new DateTime();
                switch (Convert.ToInt16(sShiftCode))
                {
                    case 0:
                        dJobDate = DateTime.ParseExact(sJobDate + " 08:00:00", "yyyyMMdd HH:mm:ss", null);
                        iTerm = 20;
                        iIncrease = 20;
                        break;
                    case 1:
                        dJobDate = DateTime.ParseExact(sJobDate + " 08:00:00", "yyyyMMdd HH:mm:ss", null);
                        iTerm = 20;
                        iIncrease = 10;
                        break;
                    case 2:
                        dJobDate = DateTime.ParseExact(sJobDate + " 20:00:00", "yyyyMMdd HH:mm:ss", null);
                        iTerm = 20;
                        iIncrease = 10;
                        break;
                    case 3:
                        dJobDate = DateTime.ParseExact(sJobDate + " 08:00:00", "yyyyMMdd HH:mm:ss", null);
                        iTerm = 30;
                        iIncrease = 10;
                        break;
                    case 4:
                        dJobDate = DateTime.ParseExact(sJobDate + " 16:00:00", "yyyyMMdd HH:mm:ss", null);
                        iTerm = 30;
                        iIncrease = 10;
                        break;
                    case 5:
                        dJobDate = DateTime.ParseExact(sJobDate + " 00:00:00", "yyyyMMdd HH:mm:ss", null).AddDays(1);
                        iTerm = 30;
                        iIncrease = 10;
                        break;
                }

                for (int i = 0; i < 24 * 60 * 60 / iTerm; i++)
                {
                    RSLTDT.Rows.Add(dJobDate.AddSeconds(i * iIncrease).ToString("yyyyMMddHHmmss"), dJobDate.AddSeconds(i * iIncrease + (iIncrease - 1)).ToString("yyyyMMddHHmmss"));
                }

                string sSysDate;
                sSysDate = "20160621140000";

                ds.Tables.Add(RSLTDT.Select("STARTTIME<'" + sSysDate + "'").CopyToDataTable());

                ds.Tables[0].TableName = "RSLTDT";
                return ds;
            }
            catch (Exception ex)
            {
                //---commMessage.Show(ex.Message);                
                return ds;
            }
        }

        /// <summary>
        /// 부동 내역 조회
        /// - 시간 차이가 180초 이상 인 경우
        /// - OP Key-In 인 경우
        /// - LOSS CODE ( 38000)  자재교체인 경우
        /// 색깔 구분
        /// - 분홍색 (2)
        ///   : 180초 이상 
        ///   : 기준정보 기준시간 초과인 경우 ( 시작시간이 0 인  기준정보 )
        /// - 회색 (1)
        ///   : OP Key-In 
        ///   : 기준정보 기준시간 이내인 경우 ( 시작시간이 0 인  기준정보 )
        /// </summary>
        private void GetEqptLossDetailList()
        {
            try
            {
                string sShop = "N4A";
                string sEqptID = "N1ANTC401";
                string sEqptType = "M";
                string sJobDate = "20160617";
                string sShiftCode = "0";

                

                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("EQPTID", typeof(string));
                RQSTDT.Columns.Add("JOBDATE", typeof(string));
                RQSTDT.Columns.Add("LANGTYPE", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["EQPTID"] = "N1ANTC401";
                dr["JOBDATE"] = "20160617";
                dr["LANGTYPE"] = "KO";
                RQSTDT.Rows.Add(dr);

                new ClientProxy("10.42.68.67", "tcp", "7866", "SERVICE", "1").ExecuteService("R_EQPTLOSS_LIST_SP", "RQSTDT", "RSLTDT", RQSTDT, (dt, ex) =>
                    {
                        _dgDetail.ItemsSource = DataTableConverter.Convert(dt);
                    }
                );

                

            }
            catch (Exception ex)
            {
            }
        }

        private void SelectProcess()
        {
            try
            {
                string sShop = "N4A";
                string sEqptID = "N1ANTC401";
                string sEqptType = "M";
                string sJobDate = "20160617";
                string sShiftCode = "0";

                //if (cboProcess.SelectedValue.ToString() == PROC.PACKAGING || cboProcess.SelectedValue.ToString() == PROC.DEGAS ||
                //    cboProcess.SelectedValue.ToString() == PROC.ASSY || cboProcess.SelectedValue.ToString() == PROC.WASHING)
                //{
                //    if ((bool)chkSubEqpt.Checked)
                //    {
                //        sEqptType = "%";
                //    }
                //}

                Hashtable hash_color = new Hashtable();
                Hashtable hash_first_list = new Hashtable();
                Hashtable hash_list = new Hashtable();
                Hashtable hash_title = new Hashtable();
                Hashtable hash_loss_color = new Hashtable();

                #region ...[HashTable 초기화]
                hash_first_list.Clear();
                hash_title.Clear();
                hash_list.Clear();
                hash_color.Clear();

                //txtStart.Text = "";
                //txtEnd.Text = "";
                //txtTroubleName.Text = "";
                //txtStartHidn.Text = "";
                //txtEndHidn.Text = "";
                //txtEqptName.Text = "";
                //txtMdesc.Text = "";

                //spdMList.ActiveSheet.RowCount = 0;

                #endregion

                #region ...[Data 조회]

                //-- 일자 별 초 단위(00) 조회 ( 주,야간은 10초 간격 , 전체는 20초 간격)
                DataSet dsEqptTimeList = GetEqptTimeList(sJobDate, sShiftCode);
                if (dsEqptTimeList.Tables["RSLTDT"] == null) return;

                //-- 설비 타이틀 명 조회
                DataTable dtEqptName = GetEqptName(sShop, sEqptID, sEqptType);
                hash_title = DataTableConverter.ToHash(dtEqptName);

                //-- 설비 가동 Trend 조회
                DataTable dsEqptLossList = GetEqptLossList(sShop, sEqptID, sEqptType, sJobDate, sShiftCode);
                hash_list = rsToHash2(dsEqptLossList);

                //-- 설비 가동 Trend 조회 (일자 별 최초 가동 정보)
                DataTable dtEqptLossFirstList = GetEqptLossFirstList(sShop, sEqptID, sEqptType, sJobDate, sShiftCode);
                hash_first_list = DataTableConverter.ToHashByColName(dtEqptLossFirstList);

                #endregion

                #region ...[색지도 처리]
                int cnt = 0;
                int inc = 0;
                int nRow = 0;
                int nCol = 0;

                //spdMList.SuspendLayout();

                Hashtable hash_Merge = new Hashtable();     //--- 같은 시간  Merge 기능 용
                Hashtable hash_rs = new Hashtable();        //--- 설비 Trend 정보 임시 저장

                //spdMList.ActiveSheet.RowCount = (hash_title.Count) + 1;

                for (int k = 0; k < hash_title.Count; k++)
                {
                    string sTitle = dtEqptName.Rows[k][0].ToString();
                    string sID = (string)hash_first_list[sTitle];   //--- 처음 기준으로 색깔 지정                    
                    hash_color.Add(sTitle, sID);
                }

                for (int i = 0; i < dsEqptTimeList.Tables["RSLTDT"].Rows.Count; i++)
                //for (int i = 0; i < 1000; i++)
                {
                    nCol = cnt + 1;
                    nRow = inc * (hash_title.Count) + inc;

                    //--- 시간 단위 셋팅 (10 분 단위로 스프레드 설정
                    string sEqptTimeList = getVal(dsEqptTimeList, "RSLTDT", i, 0).ToString();
                    int nTime = int.Parse(sEqptTimeList.Substring(10, 2));
                    if ((i) % 30 == 0)
                    {
                        Label _lable = new Label();
                        if (nTime / 10 * 10 == 0)
                        {
                            _lable.Content = sEqptTimeList.Substring(8, 2) + ":00";
                        }
                        else
                        {
                            _lable.Content = (nTime / 10 * 10).ToString();
                        }
                        _lable.FontSize = 10;
                        _lable.Margin = new Thickness(0, 0, 0, 0);
                        _lable.Padding = new Thickness(0, 0, 0, 0);
                        _lable.BorderThickness = new Thickness(1, 0, 0, 0);
                        _lable.BorderBrush = new SolidColorBrush(Colors.Gray);
                        Grid.SetColumn(_lable, nCol);
                        Grid.SetRow(_lable, nRow);
                        Grid.SetColumnSpan(_lable, 30);


                        _grid.Children.Add(_lable);
                    }

                    //spdMList.ActiveSheet.Cells[nRow, nCol].HorizontalAlignment = CellHorizontalAlignment.Left;

                    //--- 연속적인 Data 설정
                    if (!hash_Merge.ContainsKey(nRow))
                    {
                        hash_Merge.Add(nRow, nRow);
                    }

                    hash_rs.Clear();

                    //--- 가동 Trend 대표 시간 가동상태 및 LOSS 코드 설정
                    if (hash_list.ContainsKey(sEqptTimeList))
                    {
                        hash_rs = (Hashtable)hash_list[sEqptTimeList];
                        for (int k = 0; k < hash_title.Count; k++)
                        {
                            string sTitle = dtEqptName.Rows[k][0].ToString();
                            string sID = (string)hash_rs[sTitle];
                            if (!string.IsNullOrEmpty(sID))
                            {
                                hash_color.Remove(sTitle);
                                hash_color.Add(sTitle, sID);
                            }
                        }
                    }
                    //--- 가동 Trend 스프레드 색깔 설정

                    for (int k = 0; k < hash_title.Count; k++)
                    {
                        string sTitle = dtEqptName.Rows[k][0].ToString();
                        nRow = k + inc * (hash_title.Count) + inc + 1;
                        string sStatus = (string)hash_color[sTitle];

                        //spdMList.ActiveSheet.Cells[nRow, nCol].CellType = tcell;
                        //spdMList.ActiveSheet.Cells[nRow, nCol].Text = dsEqptTimeList.Tables["RSLTDT"].Rows[i][1].ToString();

                        //spdMList.ActiveSheet.Cells[nRow, nCol].BackColor = GetColor(sStatus);
                        //spdMList.ActiveSheet.Cells[nRow, nCol].ForeColor = GetColor(sStatus);

                        System.Drawing.Color color = GetColor(sStatus);

                        Border _border = new Border();

                        _border.Background = new SolidColorBrush(System.Windows.Media.Color.FromArgb(color.A, color.R, color.G, color.B));
                        Grid.SetColumn(_border, nCol);
                        Grid.SetRow(_border, nRow);

                        Hashtable org_set = new Hashtable();

                        org_set.Add("COL", nCol);
                        org_set.Add("ROW", nRow);
                        org_set.Add("COLOR", _border.Background);
                        org_set.Add("TIME", sEqptTimeList);
                        org_set.Add("STATUS", sStatus);

                        _border.Tag = org_set;

                        _border.Name = "S" + sEqptTimeList.ToString();

                        _border.MouseDown += _border_MouseDown;

                        _grid.Children.Add(_border);

                        _grid.RegisterName(_border.Name, _border);



                        //DataRow[] dtRow = dtMainList.Select("HIDDEN_START <= '" + sEqptTimeList + "' and HIDDEN_END >= '" + sEqptTimeList + "'", "");
                        //if (dtRow.Length > 0)
                        //{
                        //    if (dtRow[0]["START_ROW"].Equals(System.DBNull.Value))
                        //    {
                        //        dtRow[0]["START_ROW"] = nRow;
                        //        dtRow[0]["START_COL"] = nCol;
                        //    }
                        //    dtRow[0]["END_ROW"] = nRow;
                        //    dtRow[0]["END_COL"] = nCol;

                        //}

                        //spdMList.ActiveSheet.Cells[nRow, nCol].Tag = sStatus;
                        if (cnt == 0)
                        {
                            string sEqptName = dtEqptName.Rows[k][1].ToString();

                            //spdMList.ActiveSheet.Cells[nRow, 0].CellType = tcell;
                            //spdMList.ActiveSheet.Cells[nRow, 361].CellType = tcell;
                            //spdMList.ActiveSheet.Cells[nRow, 0].Text = sEqptName;
                            //spdMList.ActiveSheet.Cells[nRow, 361].Text = sTitle;
                            //spdMList.ActiveSheet.Cells[nRow, 0].Font = new Font("굴림체", 8, System.Drawing.FontStyle.Regular);

                            TextBlock _text = new TextBlock();
                            _text.Text = sEqptName;
                            _text.FontSize = 10;
                            _text.Margin = new Thickness(0, 0, 0, 0);
                            Grid.SetColumn(_text, 0);
                            Grid.SetRow(_text, nRow);

                            _grid.Children.Add(_text);

                        }
                    }

                    cnt++;

                    //--- 마지막 칼럼 인 경우 다음 Row 수 지정 (설비 건수 별)
                    if (cnt == 360)
                    {
                        cnt = 0;
                        inc++;
                        if (i < dsEqptTimeList.Tables["RSLTDT"].Rows.Count - 1)
                        {
                            //spdMList.ActiveSheet.RowCount = spdMList.ActiveSheet.RowCount + (hash_title.Count) + 1;
                        }
                    }
                }

                ////--- 위에서 시간 중복 Hastable 처리
                //foreach (DictionaryEntry de in hash_Merge)
                //{
                //    int nRow1 = int.Parse(de.Key.ToString());
                //    spdMList.ActiveSheet.SetRowMerge(nRow1, FarPoint.Win.Spread.Model.MergePolicy.Always);
                //    FarPoint.Win.LineBorder bevelbrdr = new FarPoint.Win.LineBorder(GridBackColor.Color3, 1);
                //    spdMList.ActiveSheet.Rows[nRow1].Border = bevelbrdr;
                //}

                //spdMList.ActiveSheet.Protect = true;
                //spdMList.ResumeLayout();

                #endregion
            }
            catch (Exception ex)
            {
                throw ex;
                //commMessage.Show(ex.Message);
            }

        }

        /// <summary>
        /// 부동내역 전체 조회 ( 가동 Trend 마우스 선택 시 범위 지정 용으로 사용 )
        /// </summary>
        private void GetEqptLossRawList()
        {
            try
            {
                string sEqptID = "N1ANTC401";
                string sJobDate = "20160617";
                string sShiftCode = "0";

                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("EQPTID", typeof(string));
                RQSTDT.Columns.Add("JOBDATE", typeof(string));
                RQSTDT.Columns.Add("SHIFTCODE", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["EQPTID"] = "N1ANTC401";
                dr["JOBDATE"] = "20160617";
                dr["SHIFTCODE"] = "0";
                RQSTDT.Rows.Add(dr);

                dtMainList = new ClientProxy("10.42.68.67", "tcp", "7866", "SERVICE", "1").ExecuteServiceSync("R_EQPTLOSS_LIST_RAW_SP", "RQSTDT", "RSLTDT", RQSTDT);


            }
            catch (Exception ex)
            {

            }
        }

        private DataTable GetEqptName(string sShop, string sEqptID, string sEqptType)
        {
            DataTable RSLTDT = new DataTable();
            try
            {

                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGTYPE", typeof(string));
                RQSTDT.Columns.Add("SHOPID", typeof(string));
                RQSTDT.Columns.Add("EQPTID", typeof(string));
                RQSTDT.Columns.Add("EQPTTYPE", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGTYPE"] = "KO";
                dr["SHOPID"] = sShop;
                dr["EQPTID"] = sEqptID;
                dr["EQPTTYPE"] = sEqptType;
                RQSTDT.Rows.Add(dr);


                RSLTDT = new ClientProxy("10.42.68.67", "tcp", "7866", "SERVICE", "1").ExecuteServiceSync("R_EQPT_TITLE_LIST", "RQSTDT", "RSLTDT", RQSTDT);
                

            }
            catch (Exception ex)
            {
                //commMessage.Show(ex.Message);
            }

            return RSLTDT;
        }

        private DataTable GetEqptLossList(string sShop, string sEqptID, string sEqptType, string sJobDate, string sShiftCode)
        {
            DataTable RSLTDT = new DataTable();
            try
            {
                string sServiceName;
                if (sShiftCode == "0")
                {
                    sServiceName = "SP_EQPTLOSS_LIST_01";   //--- 20초 단위로 조회
                }
                else
                {
                    sServiceName = "SP_EQPTLOSS_LIST";      //--- 10초 단위로 조회
                }


                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGTYPE", typeof(string));
                RQSTDT.Columns.Add("SHOPID", typeof(string));
                RQSTDT.Columns.Add("EQPTID", typeof(string));
                RQSTDT.Columns.Add("EQPTTYPE", typeof(string));
                RQSTDT.Columns.Add("JOBDATE", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGTYPE"] = "KO";
                dr["SHOPID"] = sShop;
                dr["EQPTID"] = sEqptID;
                dr["EQPTTYPE"] = sEqptType;
                dr["JOBDATE"] = sJobDate;
                RQSTDT.Rows.Add(dr);


                RSLTDT = new ClientProxy("10.42.68.67", "tcp", "7866", "SERVICE", "1").ExecuteServiceSync(sServiceName, "RQSTDT", "RSLTDT", RQSTDT);

            }
            catch (Exception ex)
            {
                //commMessage.Show(ex.Message);
            }
            
            return RSLTDT;
            
        }

        private DataTable GetEqptLossFirstList(string sShop, string sEqptID, string sEqptType, string sJobDate, string sShiftCode)
        {
            DataTable RSLTDT = new DataTable();
            try
            {

                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("SHIFTCODE", typeof(string));
                RQSTDT.Columns.Add("SHOPID", typeof(string));
                RQSTDT.Columns.Add("EQPTID", typeof(string));
                RQSTDT.Columns.Add("EQPTTYPE", typeof(string));
                RQSTDT.Columns.Add("JOBDATE", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["SHIFTCODE"] = sShiftCode;
                dr["SHOPID"] = sShop;
                dr["EQPTID"] = sEqptID;
                dr["EQPTTYPE"] = sEqptType;
                dr["JOBDATE"] = sJobDate;
                RQSTDT.Rows.Add(dr);


                RSLTDT = new ClientProxy("10.42.68.67", "tcp", "7866", "SERVICE", "1").ExecuteServiceSync("SP_EQPTLOSS_FIRST_LIST", "RQSTDT", "RSLTDT", RQSTDT);
            }
            catch (Exception ex)
            {
                //commMessage.Show(ex.Message);
            }

            return RSLTDT;
        }


        private Hashtable rsToHash2(DataTable dt)
        {
            Hashtable hash_return = new Hashtable();
            try
            {
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    Hashtable hash_rs = new Hashtable();
                    for (int j = 0; j < dt.Columns.Count - 1; j++)
                    {
                        hash_rs.Add(dt.Columns[j].ColumnName, dt.Rows[i][j].ToString());
                    }
                    hash_return.Add(dt.Rows[i]["STARTTIME"].ToString(), hash_rs);
                }
            }
            catch (Exception ex)
            {
                //commMessage.Show(ex.Message);
                hash_return = null;
            }
            return hash_return;
        }


        public static object getVal(DataSet ds, string table, string column)
        {
            return getVal(ds, table, 0, column);
        }
        public static object getVal(DataSet ds, string table, int row, string column)
        {
            return ds.Tables[table].Rows[row][column].ToString();
        }

        public static object getVal(DataSet ds, string table, int row, int column)
        {
            return ds.Tables[table].Rows[row][column].ToString();
        }
        public static object getVal(DataTable dt, int row, string column)
        {
            return dt.Rows[row][column].ToString();
        }
        public static object getVal(DataTable dt, int row, int column)
        {
            return dt.Rows[row][column].ToString();
        }

        public static class GridBackColor
        {
            public static readonly System.Drawing.Color Color1 = System.Drawing.Color.FromArgb(0, 0, 0);
            public static readonly System.Drawing.Color Color2 = System.Drawing.Color.FromArgb(0, 0, 225);
            public static readonly System.Drawing.Color Color3 = System.Drawing.Color.FromArgb(185, 185, 185);

            public static readonly System.Drawing.Color Color4 = System.Drawing.Color.FromArgb(150, 150, 150);
            public static readonly System.Drawing.Color Color5 = System.Drawing.Color.FromArgb(255, 255, 155);
            public static readonly System.Drawing.Color Color6 = System.Drawing.Color.FromArgb(255, 127, 127);

            public static readonly System.Drawing.Color R = System.Drawing.Color.FromArgb(0, 255, 0);
            public static readonly System.Drawing.Color W = System.Drawing.Color.FromArgb(255, 255, 0);
            public static readonly System.Drawing.Color T = System.Drawing.Color.FromArgb(255, 0, 0);
            public static readonly System.Drawing.Color F = System.Drawing.Color.FromArgb(0, 0, 0);
            public static readonly System.Drawing.Color N = System.Drawing.Color.FromArgb(255, 255, 255);
            public static readonly System.Drawing.Color U = System.Drawing.Color.FromArgb(128, 128, 128);

            public static readonly System.Drawing.Color I = System.Drawing.Color.FromArgb(255, 255, 0);
            public static readonly System.Drawing.Color P = System.Drawing.Color.FromArgb(255, 255, 0);
            public static readonly System.Drawing.Color O = System.Drawing.Color.FromArgb(0, 0, 0);

            public static readonly System.Drawing.Color L11000 = System.Drawing.Color.FromArgb(0, 32, 96);
            public static readonly System.Drawing.Color L12000 = System.Drawing.Color.FromArgb(0, 32, 96);
            public static readonly System.Drawing.Color L13000 = System.Drawing.Color.FromArgb(0, 32, 96);
            public static readonly System.Drawing.Color L14000 = System.Drawing.Color.FromArgb(0, 32, 96);
            public static readonly System.Drawing.Color L15000 = System.Drawing.Color.FromArgb(0, 32, 96);
            public static readonly System.Drawing.Color L16000 = System.Drawing.Color.FromArgb(0, 32, 96);
            public static readonly System.Drawing.Color L21000 = System.Drawing.Color.FromArgb(217, 151, 149);
            public static readonly System.Drawing.Color L22000 = System.Drawing.Color.FromArgb(217, 151, 149);
            public static readonly System.Drawing.Color L23000 = System.Drawing.Color.FromArgb(217, 151, 149);
            public static readonly System.Drawing.Color L31000 = System.Drawing.Color.FromArgb(0, 176, 240);
            public static readonly System.Drawing.Color L32000 = System.Drawing.Color.FromArgb(255, 0, 0);
            public static readonly System.Drawing.Color L33000 = System.Drawing.Color.FromArgb(255, 0, 0);
            public static readonly System.Drawing.Color L34000 = System.Drawing.Color.FromArgb(228, 109, 10);
            public static readonly System.Drawing.Color L35000 = System.Drawing.Color.FromArgb(0, 112, 192);
            public static readonly System.Drawing.Color L36000 = System.Drawing.Color.FromArgb(83, 142, 213);
            public static readonly System.Drawing.Color L37000 = System.Drawing.Color.FromArgb(112, 48, 160);
            public static readonly System.Drawing.Color L38000 = System.Drawing.Color.FromArgb(112, 48, 160);
            public static readonly System.Drawing.Color L39000 = System.Drawing.Color.FromArgb(148, 39, 84);
            public static readonly System.Drawing.Color L3A000 = System.Drawing.Color.FromArgb(165, 165, 165);
            public static readonly System.Drawing.Color L3B000 = System.Drawing.Color.FromArgb(255, 255, 0);
            public static readonly System.Drawing.Color L41000 = System.Drawing.Color.FromArgb(0, 0, 255);
        }

        private System.Drawing.Color GetColor(string sType)
        {
            System.Drawing.Color color = System.Drawing.Color.White;
            try
            {
                switch (sType)
                {
                    case "R":
                        color = GridBackColor.R;
                        break;
                    case "W":
                        color = GridBackColor.W;
                        break;
                    case "T":
                        color = GridBackColor.T;
                        break;
                    case "F":
                        color = GridBackColor.F;
                        break;
                    case "N":
                        color = GridBackColor.N;
                        break;
                    case "U":
                        color = GridBackColor.U;
                        break;
                    case "I":
                        color = GridBackColor.I;
                        break;
                    case "P":
                        color = GridBackColor.P;
                        break;
                    case "O":
                        color = GridBackColor.O;
                        break;
                    default:
                        if (sType.Equals(""))
                        {
                            color = System.Drawing.Color.White;
                        }
                        else
                        {
                            color = System.Drawing.Color.FromName(hash_loss_color[sType.Substring(1)].ToString());
                        }
                        break;

                }
            }
            catch (Exception ex)
            {
                //commMessage.Show(ex.Message);
                color = System.Drawing.Color.White;
            }
            return color;
        }


        private void _border_MouseDown(object sender, MouseButtonEventArgs e)
        {
            //org_set.Add("COL", nCol);
            //org_set.Add("ROW", nRow);
            //org_set.Add("COLOR", _border.Background);
            //org_set.Add("TIME", sEqptTimeList);

            Border aa = sender as Border;



            if (aa.Background.ToString().Equals("#FF0000FF")) //파란색 다시 누르면 풀기
            {
                resetMapColor();
            }
            else
            {

                Hashtable org_set = aa.Tag as Hashtable;

                setMapColor(org_set["TIME"].ToString());

            }

        }

        private void setMapColor(String sTime)
        {
            DataRow[] dtRow = dtMainList.Select("HIDDEN_START <= '" + sTime + "' and HIDDEN_END >= '" + sTime + "'", "");
            DataRow[] dtRowBefore = dtMainList.Select("CHK = '1'", "HIDDEN_START ASC");

            //Shift 에 따라 변경 되도록 할것
            //전체일경우 20, 나머지는 10
            int inc = 20;

            if (dtRow.Length > 0)
            {
                dtRow[0]["CHK"] = "1";

                double dStartTime = new Double();
                Double dEndTime = new Double();

                if (dtRowBefore.Length > 0) //이미 체크가 있는경우
                {
                    if (Convert.ToDouble(dtRow[0]["HIDDEN_START"]) > Convert.ToDouble(dtRowBefore[0]["HIDDEN_START"]))
                    {
                        dStartTime = Math.Truncate(Convert.ToDouble(dtRowBefore[0]["HIDDEN_START"]) / inc) * inc;
                        dEndTime = Math.Truncate(Convert.ToDouble(dtRow[0]["HIDDEN_END"]) / inc) * inc;

                        txtStart.Text = dtRowBefore[0]["START_TIME"].ToString();
                        txtStartHidn.Text = dtRowBefore[0]["HIDDEN_START"].ToString();

                        txtEnd.Text = dtRow[0]["END_TIME"].ToString();
                        txtEndHidn.Text = dtRow[0]["HIDDEN_END"].ToString();
                    }
                    else
                    {
                        dStartTime = Math.Truncate(Convert.ToDouble(dtRow[0]["HIDDEN_START"]) / inc) * inc;
                        dEndTime = Math.Truncate(Convert.ToDouble(dtRowBefore[dtRowBefore.Length - 1]["HIDDEN_END"]) / inc) * inc;

                        txtStart.Text = dtRow[0]["START_TIME"].ToString();
                        txtStartHidn.Text = dtRow[0]["HIDDEN_START"].ToString();

                        txtEnd.Text = dtRowBefore[dtRowBefore.Length - 1]["END_TIME"].ToString();
                        txtEndHidn.Text = dtRowBefore[dtRowBefore.Length - 1]["HIDDEN_END"].ToString();
                    }
                }
                else
                {
                    dStartTime = Math.Truncate(Convert.ToDouble(dtRow[0]["HIDDEN_START"]) / inc) * inc;
                    dEndTime = Math.Truncate(Convert.ToDouble(dtRow[0]["HIDDEN_END"]) / inc) * inc;

                    txtStart.Text = dtRow[0]["START_TIME"].ToString();
                    txtStartHidn.Text = dtRow[0]["HIDDEN_START"].ToString();

                    txtEnd.Text = dtRow[0]["END_TIME"].ToString();
                    txtEndHidn.Text = dtRow[0]["HIDDEN_END"].ToString();
                }

                Border borderS = _grid.FindName("S" + dStartTime.ToString()) as Border;
                Border borderE = _grid.FindName("S" + dEndTime.ToString()) as Border;

                Hashtable hashStart = borderS.Tag as Hashtable;
                Hashtable hashEnd = borderE.Tag as Hashtable;

                DateTime dStart = new DateTime(Convert.ToInt16(dStartTime.ToString().Substring(0, 4)),
                                                Convert.ToInt16(dStartTime.ToString().Substring(4, 2)),
                                                Convert.ToInt16(dStartTime.ToString().Substring(6, 2)),
                                                Convert.ToInt16(dStartTime.ToString().Substring(8, 2)),
                                                Convert.ToInt16(dStartTime.ToString().Substring(10, 2)),
                                                Convert.ToInt16(dStartTime.ToString().Substring(12, 2)));

                int cellCnt = (Convert.ToInt16(hashEnd["ROW"]) - Convert.ToInt16(hashStart["ROW"])) / 2 * 360 + (Convert.ToInt16(hashEnd["COL"]) - Convert.ToInt16(hashStart["COL"]));

                for (int j = 0; j < cellCnt; j++)
                {
                    Border _border = _grid.FindName("S" + dStart.AddSeconds(j * inc).ToString("yyyyMMddHHmmss")) as Border;

                    _border.Background = new SolidColorBrush(Colors.Blue);
                }

                //마지막 칸 정리
                Border borderEndMinusOne = _grid.FindName("S" + dStart.AddSeconds((cellCnt - 1) * inc).ToString("yyyyMMddHHmmss")) as Border;
                Hashtable hashEndMinusOne = borderEndMinusOne.Tag as Hashtable;

                if (hashEnd["COLOR"].ToString().Equals(hashEndMinusOne["COLOR"].ToString()))
                {
                    borderE.Background = new SolidColorBrush(Colors.Blue);
                }


            }
        }

        private void resetMapColor()
        {
            foreach (Border _border in _grid.Children.OfType<Border>())
            {

                Hashtable org_set = (Hashtable)_border.Tag as Hashtable;
                _border.Background = org_set["COLOR"] as SolidColorBrush;
            }

            DataRow[] dtRow = dtMainList.Select("CHK = '1'", "");

            foreach (DataRow dr in dtRow)
            {
                dr["CHK"] = "0";
            }

        }
        

        private void label_MouseDown(object sender, MouseButtonEventArgs e)
        {
            MessageBox.Show("test");
        }
    }


}
