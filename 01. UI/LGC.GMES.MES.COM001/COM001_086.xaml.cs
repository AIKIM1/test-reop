/*************************************************************************************
 Created Date : 2017.06.13
      Creator : 이진선
   Decription : 설비 LOSS 관리
--------------------------------------------------------------------------------------
 [Change History]
  2016.06.16  DEVELOPER : Initial Created.
  2023.02.07  성민식    : CSR 요청번호 E20230207-000177 
                          LOSS 모니터링 정보 미조회 오류로 인해 _border_MouseDown() 파라미터 추가
  2023.03.21  성민식    : CSR 요청번호 E20230227-000468
                          라인, 공정 콤보박스 다중 선택 시 설비 누락 건 수정 -> cboProcess_SelectionChanged() for문 내부 변수 수정
  2024.02.08  이샛별    : CSR 요청번호 E20240124-001042
                          GM법인 시간 표시 오류로 인해 오류 원인인 if문 제거 
  2024.05.16  안유수 E20240409-000435 설비 극성 콤보박스 추가
  2024.05.27  최동훈 E20240523-001255 활성화 기능 적용
  2024.08.20  최동훈 E20240718-001016 상세그리드에 시작일 컬럼 추가
  2025.06.09  이민형 [HD_OSS_0386] 설비Loss조회(Multi) 색지도 표현 부분 오류 수정
**************************************************************************************/

using C1.WPF;
using C1.WPF.DataGrid;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using LGC.GMES.MES.CMM001;
using System.Globalization;
using System;
using System.Windows.Documents;
using System.Threading;

namespace LGC.GMES.MES.COM001
{
    public partial class COM001_086 : UserControl, IWorkArea
    {
        #region Declaration & Constructor 

        Hashtable hash_loss_color = new Hashtable();
        DataTable AreaTime;
        DataSet dsEqptTimeList = null;
        Util _Util = new Util();
        Hashtable org_set;


        public COM001_086()
        {
            InitializeComponent();

            InitCombo();

            InitGrid();

            GetLossColor();



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
            cboEquipmentSegment.ApplyTemplate();

            //동,라인,공정,설비 셋팅
            CommonCombo _combo = new CommonCombo();

            //동
            //C1ComboBox[] cboAreaChild = { cboEquipmentSegment};
            _combo.SetCombo(cboArea, CommonCombo.ComboStatus.NONE, null);

            //라인
            //C1ComboBox[] cboEquipmentSegmentParent = { cboArea };
            //C1ComboBox[] cboEquipmentSegmentChild = { cboProcess};
            //_combo.SetCombo(cboEquipmentSegment, CommonCombo.ComboStatus.NONE, cbParent: cboEquipmentSegmentParent);

            //공정
            //C1ComboBox[] cboProcessParent = { cboEquipmentSegment };
            //_combo.SetCombo(cboProcess, CommonCombo.ComboStatus.NONE, cbParent: cboProcessParent);


            C1ComboBoxItem cbItemTitle = new C1ComboBoxItem();
            cbItemTitle.Content = ObjectDic.Instance.GetObjectName("범례");
            cboColor.Items.Add(cbItemTitle);

            C1ComboBoxItem cbItemRun = new C1ComboBoxItem();
            cbItemRun.Content = "Run";
            cbItemRun.Background = ColorToBrush(GridBackColor.R);
            cboColor.Items.Add(cbItemRun);

            C1ComboBoxItem cbItemWait = new C1ComboBoxItem();
            cbItemWait.Content = "Wait";
            cbItemWait.Background = ColorToBrush(GridBackColor.W);
            cboColor.Items.Add(cbItemWait);

            C1ComboBoxItem cbItemTrouble = new C1ComboBoxItem();
            cbItemTrouble.Content = "Trouble";
            cbItemTrouble.Background = ColorToBrush(GridBackColor.T);
            cboColor.Items.Add(cbItemTrouble);

            C1ComboBoxItem cbItemOff = new C1ComboBoxItem();
            cbItemOff.Content = "OFF";
            cbItemOff.Background = ColorToBrush(GridBackColor.F);
            cboColor.Items.Add(cbItemOff);

            C1ComboBoxItem cbItemUserStop = new C1ComboBoxItem();
            cbItemUserStop.Content = "UserStop";
            cbItemUserStop.Background = ColorToBrush(GridBackColor.U);
            cboColor.Items.Add(cbItemUserStop);

            cboColor.SelectedIndex = 0;

            // 2024.05.27
            if (LoginInfo.CFG_SYSTEM_TYPE_CODE.Equals("F"))
            {
                tbElecType.Visibility = Visibility.Collapsed;
                cboElecType.Visibility = Visibility.Collapsed;
            }
            else
            {
                tbElecType.Visibility = Visibility.Visible;
                cboElecType.Visibility = Visibility.Visible;
                SetElecType();
            }

        }

        private void cboArea_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            try
            {
                string bizRuleName = "DA_BAS_SEL_EQUIPMENTSEGMENT_CBO";

                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("AREAID", typeof(string));

                // 2024.05.27
                if (LoginInfo.CFG_SYSTEM_TYPE_CODE.Equals("F"))
                {
                    bizRuleName = "BR_GET_EQUIPMENTSEGMENT_FORM_LOSS_CBO";

                    RQSTDT.Columns.Add("INCLUDE_GROUP", typeof(string));

                    DataRow dr = RQSTDT.NewRow();
                    dr["LANGID"] = LoginInfo.LANGID;
                    dr["AREAID"] = cboArea.SelectedValue;
                    dr["INCLUDE_GROUP"] = "AC";

                    RQSTDT.Rows.Add(dr);
                }
                else
                {
                    DataRow dr = RQSTDT.NewRow();
                    dr["LANGID"] = LoginInfo.LANGID;
                    dr["AREAID"] = cboArea.SelectedValue.ToString();
                    RQSTDT.Rows.Add(dr);
                }

                DataTable dtResult = new ClientProxy().ExecuteServiceSync(bizRuleName, "RQSTDT", "RSLTDT", RQSTDT);

                cboEquipmentSegment.DisplayMemberPath = "CBO_NAME";
                cboEquipmentSegment.SelectedValuePath = "CBO_CODE";
                cboEquipmentSegment.ItemsSource = DataTableConverter.Convert(dtResult);
            }
            catch (Exception ex)
            {
            }
        }

        private void cboEquipmentSegment_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {

            //try
            //{
            //    DataTable RQSTDT = new DataTable();
            //    RQSTDT.TableName = "RQSTDT";
            //    RQSTDT.Columns.Add("LANGID", typeof(string));
            //    RQSTDT.Columns.Add("EQSGID", typeof(string));

            //    DataRow dr = RQSTDT.NewRow();
            //    dr["LANGID"] = LoginInfo.LANGID;
            //    dr["EQSGID"] = Convert.ToString(cboEquipmentSegment.SelectedValue);
            //    RQSTDT.Rows.Add(dr);

            //    DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_PROCESS_CBO", "RQSTDT", "RSLTDT", RQSTDT);

            //    cboProcess.DisplayMemberPath = "CBO_NAME";
            //    cboProcess.SelectedValuePath = "CBO_CODE";


            //    cboProcess.ItemsSource = DataTableConverter.Convert(dtResult);
            //}
            //catch (Exception ex)
            //{
            //}
        }

        private void cboEquipmentSegment_SelectionChanged(object sender, EventArgs e)
        {

            try
            {
                cboProcess.ItemsSource = null;

                string str = string.Empty;
                for (int i = 0; i < cboEquipmentSegment.SelectedItems.Count; i++)
                {
                    if (i != cboProcess.SelectedItems.Count - 1)
                    {
                        str += cboEquipmentSegment.SelectedItems[i] + ",";
                    }
                    else
                    {
                        str += cboEquipmentSegment.SelectedItems[i];
                    }
                }

                if (string.IsNullOrEmpty(str)) return;

                string bizRuleName = "DA_BAS_SEL_PROCESS_CBO";

                // 2024.05.27
                if (LoginInfo.CFG_SYSTEM_TYPE_CODE.Equals("F"))
                {
                    bizRuleName = "DA_BAS_SEL_PROCESS_EQPTLOSS_CBO_FORM_EXIST_EQPT";
                }

                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("EQSGID", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["EQSGID"] = str;
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync(bizRuleName, "RQSTDT", "RSLTDT", RQSTDT);

                cboProcess.DisplayMemberPath = "CBO_NAME";
                cboProcess.SelectedValuePath = "CBO_CODE";

                cboProcess.ItemsSource = DataTableConverter.Convert(dtResult.DefaultView.ToTable(true));
            }
            catch (Exception ex)
            {
            }

        }

        private void cboProcess_SelectionChanged(object sender, EventArgs e)
        {
            SetEquipment();
            //try
            //{
            //    cboEquipment.ItemsSource = null;

            //    string strEqsg = string.Empty;
            //    for (int i = 0; i < cboEquipmentSegment.SelectedItems.Count; i++)
            //    {
            //        if (i != cboEquipmentSegment.SelectedItems.Count - 1)
            //        {
            //            strEqsg += cboEquipmentSegment.SelectedItems[i] + ",";
            //        }
            //        else
            //        {
            //            strEqsg += cboEquipmentSegment.SelectedItems[i];
            //        }
            //    }

            //    string str = string.Empty;
            //    for (int i = 0; i < cboProcess.SelectedItems.Count; i++)
            //    {
            //        if (i != cboProcess.SelectedItems.Count - 1)
            //        {
            //            str += cboProcess.SelectedItems[i] + ",";
            //        }
            //        else
            //        {
            //            str += cboProcess.SelectedItems[i];
            //        }
            //    }

            //    string strElec = string.Empty;
            //    for (int i = 0; i < cboElecType.SelectedItems.Count; i++)
            //    {
            //        if (i != cboElecType.SelectedItems.Count - 1)
            //        {
            //            strElec += cboElecType.SelectedItems[i] + ",";
            //        }
            //        else
            //        {
            //            strElec += cboElecType.SelectedItems[i];
            //        }
            //    }

            //    if (string.IsNullOrEmpty(str)) return;

            //    DataTable RQSTDT = new DataTable();
            //    RQSTDT.TableName = "RQSTDT";
            //    RQSTDT.Columns.Add("LANGID", typeof(string));
            //    RQSTDT.Columns.Add("EQSGID", typeof(string));
            //    RQSTDT.Columns.Add("PROCID", typeof(string));
            //    RQSTDT.Columns.Add("ELEC_TYPE_CODE", typeof(string));

            //    DataRow dr = RQSTDT.NewRow();
            //    dr["LANGID"] = LoginInfo.LANGID;
            //    dr["EQSGID"] = strEqsg;
            //    dr["PROCID"] = str;
            //    if (!cboElecType.MultiSelectionBoxSource.Cast<MultiSelectionBoxItem>().ElementAt(0).IsChecked)
            //        dr["ELEC_TYPE_CODE"] = strElec;
            //    RQSTDT.Rows.Add(dr);

            //    //DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_EQUIPMENT_EQPT_LOSS_CBO", "RQSTDT", "RSLTDT", RQSTDT);
            //    DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_EQUIPMENT_EQPTLEVEL_MAIN_CBO", "RQSTDT", "RSLTDT", RQSTDT);

            //    cboEquipment.DisplayMemberPath = "CBO_NAME";
            //    cboEquipment.SelectedValuePath = "CBO_CODE";

            //    cboEquipment.ItemsSource = DataTableConverter.Convert(dtResult);
            //}
            //catch (Exception ex)
            //{

            //}
        }

        private void cboProcess_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            //try
            //{
            //    DataTable RQSTDT = new DataTable();
            //    RQSTDT.TableName = "RQSTDT";
            //    RQSTDT.Columns.Add("LANGID", typeof(string));
            //    RQSTDT.Columns.Add("EQSGID", typeof(string));
            //    RQSTDT.Columns.Add("PROCID", typeof(string));

            //    DataRow dr = RQSTDT.NewRow();
            //    dr["LANGID"] = LoginInfo.LANGID;
            //    dr["EQSGID"] = Convert.ToString(cboEquipmentSegment.SelectedValue);
            //    dr["PROCID"] = Convert.ToString(cboProcess.SelectedValue);
            //    RQSTDT.Rows.Add(dr);

            //    DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_EQUIPMENT_CBO", "RQSTDT", "RSLTDT", RQSTDT);

            //    cboEquipment.DisplayMemberPath = "CBO_NAME";
            //    cboEquipment.SelectedValuePath = "CBO_CODE";

            //    cboEquipment.ItemsSource = DataTableConverter.Convert(dtResult); //AddStatus(dtResult, cs, "CBO_CODE", "CBO_NAME").Copy().AsDataView();
            //}
            //catch (Exception ex)
            //{

            //}


        }

        /// <summary>
        /// 색지도 그리드 초기화
        /// </summary>
        private void InitGrid()
        {
            try
            {

                //   _grid.Width = 3000;

                //_grid.Height = GridLength.Auto;//200 * 15;

                for (int i = 0; i < 722; i++)
                {
                    ColumnDefinition gridCol1 = new ColumnDefinition();
                    if (i == 0)
                    {
                        gridCol1.Width = GridLength.Auto;
                    }
                    else { gridCol1.Width = new GridLength(2.2); }

                    _grid.ColumnDefinitions.Add(gridCol1);

                }

                for (int i = 0; i < 200; i++)
                {
                    RowDefinition gridRow1 = new RowDefinition();
                    gridRow1.Height = new GridLength(15);
                    _grid.RowDefinitions.Add(gridRow1);
                }

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        /// <summary>
        /// 사용할 색상정보 가져오기
        /// </summary>
        private void GetLossColor()
        {
            try
            {
                DataTable dtRqst = new DataTable();
                dtRqst.Columns.Add("LANGID", typeof(string));

                DataRow dr = dtRqst.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dtRqst.Rows.Add(dr);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_EQP_SEL_LOSS_COLR", "RQSTDT", "RSLTDT", dtRqst);

                hash_loss_color = DataTableConverter.ToHash(dtRslt);

                foreach (DataRow drRslt in dtRslt.Rows)
                {
                    C1ComboBoxItem cbItem = new C1ComboBoxItem();
                    cbItem.Content = drRslt["LOSS_NAME"];
                    cbItem.Background = ColorToBrush(System.Drawing.Color.FromName(drRslt["DISPCOLOR"].ToString()));
                    cboColor.Items.Add(cbItem);
                }

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }



        }
        #endregion

        #region Event
        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            //사용자 권한별로 버튼 숨기기
            List<Button> listAuth = new List<Button>();
            //  listAuth.Add(btnSave);
            Util.pageAuth(listAuth, FrameOperation.AUTHORITY);
            //여기까지 사용자 권한별로 버튼 숨기기



            DataTable dt = new DataTable();
            dt.Columns.Add("AREAID", typeof(string));
            dt.Columns.Add("JOBDATE", typeof(string));

            DataRow row = dt.NewRow();
            row["AREAID"] = Convert.ToString(cboArea.SelectedValue);
            row["JOBDATE"] = Util.GetCondition(ldpDatePicker); //ldpDatePicker.SelectedDateTime.ToShortDateString();
            dt.Rows.Add(row);

            AreaTime = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_BAS_TIME_BY_AREA", "RQSTDT", "RSLTDT", dt);
            if (AreaTime.Rows.Count == 0) { }
            if (Convert.ToString(AreaTime.Rows[0]["HHMMSS"]).Equals(""))
            {
                Util.MessageValidation("SFU3432"); //동별 작업시작 기준정보를 입력해주세요
                return;
            }
            TimeSpan tmp = DateTime.Parse(System.DateTime.Now.ToString("HH:mm:ss")).TimeOfDay;

            if (tmp < DateTime.Parse(Convert.ToString(AreaTime.Rows[0]["HHMMSS"]).Substring(0, 2) + ":" + Convert.ToString(AreaTime.Rows[0]["HHMMSS"]).Substring(2, 2) + ":" + Convert.ToString(AreaTime.Rows[0]["HHMMSS"]).Substring(4, 2)).TimeOfDay)
            {
                ldpDatePicker.SelectedDateTime = System.DateTime.Now.AddDays(-1);
            }




        }

        /// <summary>
        /// 조회버튼 클릭
        /// </summary>
        public void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            if (cboProcess.SelectedItems.Count == 0)
            {
                Util.MessageValidation("SFU1459"); //공정을 선택하세요
                return;
            }

            if (cboEquipment.SelectedItems.Count == 0)
            {
                Util.MessageValidation("SFU1153"); //설비를 선택하세요
                return;
            }


            loadingIndicator.Visibility = Visibility.Visible;

            this.Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Background, new Action(() =>
            {
                try
                {
                    ClearGrid();


                    SelectProcess();
                    GetEqptLossDetailList();
                }
                catch (Exception ex)
                {

                }
                finally
                {
                    loadingIndicator.Visibility = Visibility.Collapsed;
                }




            }));






        }




        /// <summary>
        /// 상세 데이터 색변환하기
        /// </summary>
        //private void dgDetail_LoadedCellPresenter(object sender, C1.WPF.DataGrid.DataGridCellEventArgs e)
        //{
        //    C1.WPF.DataGrid.C1DataGrid dataGrid = sender as C1.WPF.DataGrid.C1DataGrid;

        //    dataGrid.Dispatcher.BeginInvoke(new Action(() =>
        //    {
        //        if (e.Cell.Presenter == null)
        //        {
        //            return;
        //        }
        //        //Grid Data Binding 이용한 Background 색 변경
        //        if (e.Cell.Row.Type == DataGridRowType.Item)
        //        {
        //            string sCheck = Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "CHECK_DELETE"));
        //            string loss_code = Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "LOSS_CODE"));
        //            string loss_detl_code = Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "LOSS_DETL_CODE"));

        //            if (sCheck.Equals("DELETE"))
        //            {
        //                System.Drawing.Color color = GridBackColor.Color4;
        //                e.Cell.Presenter.Background = new SolidColorBrush(System.Windows.Media.Color.FromArgb(color.A, color.R, color.G, color.B));
        //            }
        //            else if (!sCheck.Equals("DELETE") && !loss_code.Equals("") && !loss_detl_code.Equals(""))
        //            {
        //                e.Cell.Presenter.Background = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("lightBlue"));
        //            }
        //            else
        //            {
        //                System.Drawing.Color color = GridBackColor.Color6;
        //                e.Cell.Presenter.Background = new SolidColorBrush(System.Windows.Media.Color.FromArgb(color.A, color.R, color.G, color.B)); ;
        //            }

        //        }

        //        //link 색변경
        //        if (e.Cell.Column.Name != null)
        //        {
        //            if (e.Cell.Column.Name.Equals("CHECK_DELETE"))
        //            {
        //                e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Blue);
        //            }
        //            else if (e.Cell.Column.Name.Equals("SPLIT"))
        //            {
        //                e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Blue);
        //            }
        //            else
        //            {
        //                e.Cell.Presenter.Foreground = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#FF333333"));
        //            }
        //        }
        //    }));

        //}


        #endregion

        #region Mehod

        /// <summary>
        /// 색지도초기화
        /// </summary>
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
                Util.MessageException(ex);
            }
        }

        private string SelectEquipment()
        {
            string sEqptID = string.Empty;

            for (int i = 0; i < cboEquipment.SelectedItems.Count; i++)
            {
                if (i < cboEquipment.SelectedItems.Count - 1)
                {
                    sEqptID += Convert.ToString(cboEquipment.SelectedItems[i]) + ",";
                }
                else
                {
                    sEqptID += Convert.ToString(cboEquipment.SelectedItems[i]);
                }
            }

            return sEqptID;
        }

        /// <summary>
        /// 색지도 처리
        /// </summary>
        private void SelectProcess()
        {
            try
            {
                string sEqptID = SelectEquipment();
                string sEqptType = (chkMain.IsChecked.Equals(true)) ? "M" : "A";
                string sJobDate = Util.GetCondition(ldpDatePicker);


                Hashtable hash_color = new Hashtable();
                Hashtable hash_list = new Hashtable();
                Hashtable hash_title = new Hashtable();
                Hashtable hash_first_list = new Hashtable();
                Hashtable hash_first_color = new Hashtable();
                #region ...[HashTable 초기화]
                hash_title.Clear();
                hash_list.Clear();
                hash_color.Clear();
                hash_first_list.Clear();
                hash_first_color.Clear();
                #endregion

                #region ...[Data 조회]

                //-- 일자 별 초 단위(00) 조회
                dsEqptTimeList = GetEqptTimeList(sJobDate);
                if (dsEqptTimeList.Tables["RSLTDT"] == null) return;

                //-- 설비 타이틀 명 조회
                DataTable dtEqptName = GetEqptName(sEqptID, sEqptType);
                hash_title = DataTableConverter.ToHash(dtEqptName);

                string[] eqplist;
                if (sEqptID.Contains(','))
                {
                    eqplist = sEqptID.Split(',');
                }
                else
                {
                    eqplist = new string[1];
                    eqplist[0] = sEqptID;
                }

                for (int k = 0; k < eqplist.Length; k++)
                {
                    DataTable dtEqptLossFirstList = GetEqptLossFirstList(eqplist[k], sEqptType, sJobDate);
                    hash_first_list = DataTableConverter.ToHashByColName(dtEqptLossFirstList);
                    DataTable dtEqptNameTitle = GetEqptNameTitle(eqplist[k], sEqptType);
                    string sTitle = dtEqptNameTitle.Rows[0][0].ToString();
                    string sID = (string)hash_first_list[sTitle];   //--- 처음 기준으로 색깔 지정          
                    if (!string.IsNullOrEmpty(sID))
                    {
                        hash_first_color.Add(eqplist[k], sID);
                    }
                }

                //-- 설비 가동 Trend 조회
                DataTable dtEqptLossList = GetEqptLossList(sEqptID, sEqptType, sJobDate);
                hash_list = rsToHash2(dtEqptLossList);

                #endregion

                #region ...[색지도 처리]
                int cnt = 0;
                int inc = 0;
                int nRow = 0;
                int nCol = 0;



                for (int i = 0; i < dsEqptTimeList.Tables["RSLTDT"].Rows.Count; i++)
                {
                    nCol = cnt + 1;
                    nRow = inc * dtEqptName.Rows.Count + inc;


                    string sEqptTimeList = dsEqptTimeList.Tables["RSLTDT"].Rows[i][0].ToString();

                    //int nTime = int.Parse(sEqptTimeList.Substring(10, 2));

                    int nHour = int.Parse(sEqptTimeList.Substring(8, 2));
                    int nMinute = int.Parse(sEqptTimeList.Substring(10, 2));

                    if ((i) % (30) == 0)
                    {
                        Label _lable = new Label();


                        _lable.Content = $"{nHour:D2}:{nMinute:D2}";

                        // 05:30이 작업시작시간인 법인 시간표시오류 발생으로 인한 주석처리 
                        //if (nTime / 10 * 10 == 0)
                        //{
                        //    _lable.Content = sEqptTimeList.Substring(8, 2) + ":00";
                        //}
                        //else
                        //{
                        //    _lable.Content = (nTime / 10 * 10).ToString();
                        //}


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


                    for (int k = 0; k < hash_title.Count; k++)
                    {
                        string sTitle = dtEqptName.Rows[k][0].ToString();
                        nRow = k + inc * (hash_title.Count) + inc + 1;
                        string sStatus = string.Empty;

                        if (hash_list[sTitle] != null)
                        {
                            if (((DataTable)(hash_list[sTitle])).Rows.Count != 0)
                            {
                                DataRow[] dr = ((DataTable)hash_list[sTitle]).Select("EQPTID = '" + sTitle + "' and STRT_DTTM_YMDHMS < '" + sEqptTimeList + "'");
                                sStatus = dr.Length != 0 ? dr[dr.Length - 1]["LOSS_CODE"].ToString() : dtEqptLossList.Select("EQPTID = '" + sTitle + "'")[0]["LOSS_CODE"].ToString();
                            }

                            if (string.IsNullOrEmpty(sStatus))
                            {
                                sStatus = (string)hash_first_color[sTitle];   //--- 처음 기준으로 색깔 지정      
                            }

                        }

                        System.Drawing.Color color = GetColor(sStatus);

                        Border _border = new Border();

                        _border.Background = new SolidColorBrush(System.Windows.Media.Color.FromArgb(color.A, color.R, color.G, color.B));
                        _border.Margin = new Thickness(-1, 0, 0, 2);
                        Grid.SetColumn(_border, nCol);
                        Grid.SetRow(_border, nRow);

                        Hashtable org_set = new Hashtable();

                        org_set.Add("COL", nCol);
                        org_set.Add("ROW", nRow);
                        org_set.Add("COLOR", _border.Background);
                        org_set.Add("TIME", sEqptTimeList);
                        org_set.Add("STATUS", sStatus);
                        org_set.Add("EQPTID", sTitle);

                        _border.Tag = org_set;

                        _border.Name = "S" + sTitle.Replace("-", "_") + sEqptTimeList.ToString();

                        _border.MouseDown += _border_MouseDown;
                        if (!sStatus.Equals("R"))
                        {
                            // 2024.05.27
                            if (LoginInfo.CFG_SYSTEM_TYPE_CODE.Equals("F")
                                && string.IsNullOrEmpty(sStatus))
                            {

                            }
                            else
                            {
                                _border.Cursor = Cursors.Hand;
                            }
                        }

                        _grid.Children.Add(_border);

                        _grid.RegisterName(_border.Name, _border);

                        if (cnt == 0)
                        {
                            string sEqptName = dtEqptName.Rows[k][1].ToString();

                            TextBlock _text = new TextBlock();
                            _text.Text = sEqptName;
                            _text.FontSize = 10;
                            _text.Margin = new Thickness(10, 0, 10, 0);
                            Grid.SetColumn(_text, 0);
                            Grid.SetRow(_text, nRow);

                            _grid.Children.Add(_text);

                        }
                    }

                    cnt++;

                    //--- 마지막 칼럼 인 경우 다음 Row 수 지정 (설비 건수 별)
                    if (cnt == 720)
                    {
                        cnt = 0;
                        inc++;
                        if (i < dsEqptTimeList.Tables["RSLTDT"].Rows.Count - 1)
                        {
                        }
                    }


                }

                #endregion
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }


        }

        private void _border_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Border aa = sender as Border;

            org_set = aa.Tag as Hashtable;

            if (!org_set["STATUS"].ToString().Equals("R"))
            {
                // 2024.05.27
                if (LoginInfo.CFG_SYSTEM_TYPE_CODE.Equals("F")
                    && string.IsNullOrEmpty(Convert.ToString(org_set["STATUS"])))
                {
                    return;
                }

                COM001_014_TROUBLE wndPopup = new COM001_014_TROUBLE();
                wndPopup.FrameOperation = FrameOperation;

                if (wndPopup != null)
                {
                    object[] Parameters = new object[4];
                    Parameters[0] = "A" + org_set["EQPTID"].ToString();
                    Parameters[1] = org_set["TIME"].ToString();
                    Parameters[2] = 1;
                    Parameters[3] = Util.GetCondition(ldpDatePicker);


                    C1WindowExtension.SetParameters(wndPopup, Parameters);

                    this.Dispatcher.BeginInvoke(new Action(() => wndPopup.ShowModal()));
                }
            }

        }

        private void wndPopup_Closed(object sender, EventArgs e)
        {
            COM001_014_TROUBLE window = sender as COM001_014_TROUBLE;
            if (window.DialogResult == MessageBoxResult.OK)
            {
                //btnSearch_Click(null, null);
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
                DataTable RQSTDT = new DataTable();

                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("EQPTID", typeof(string));
                RQSTDT.Columns.Add("WRK_DATE", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["EQPTID"] = SelectEquipment();
                dr["WRK_DATE"] = Util.GetCondition(ldpDatePicker);
                RQSTDT.Rows.Add(dr);

                DataTable RSLTDT = new ClientProxy().ExecuteServiceSync("DA_EQP_SEL_EQPTLOSSDETAIL_MNT", "RQSTDT", "RSLTDT", RQSTDT);

                Util.GridSetData(dgDetail, RSLTDT, FrameOperation, true);


            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }




        /// <summary>
        /// 설비 시간 목록
        /// </summary>
        private DataSet GetEqptTimeList(string sJobDate)
        {

            DataTable dt = new DataTable();
            dt.Columns.Add("AREAID", typeof(string));
            dt.Columns.Add("JOBDATE", typeof(string));

            DataRow row = dt.NewRow();
            row["AREAID"] = Convert.ToString(cboArea.SelectedValue);
            row["JOBDATE"] = sJobDate;
            dt.Rows.Add(row);

            DataTable result = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_BAS_TIME_BY_AREA", "RQSTDT", "RSLTDT", dt);
            if (result.Rows.Count == 0) { }
            if (Convert.ToString(result.Rows[0]["JOBDATE_YYYYMMDD"]).Equals(""))
            {
                Util.MessageValidation("SFU3432"); //동별 작업시작 기준정보를 입력해주세요
                return null;
            }




            DataSet ds = new DataSet();

            try
            {

                DataTable RSLTDT = new DataTable("RSLTDT");

                RSLTDT.Columns.Add("STARTTIME", typeof(string));
                RSLTDT.Columns.Add("ENDTIME", typeof(string));

                int iTerm = 0;
                int iIncrease = 0;

                DateTime dJobDate = new DateTime();

                //dJobDate = DateTime.ParseExact(sJobDate + " 06:00:00", "yyyyMMdd HH:mm:ss", null);
                //마감일 MMD에서 AREAID로 조회하여 조회 조건에 추가
                //STRTTIME FORMAT = "2023-10-20 06:00:00"
                string STRTTIME = " " + Convert.ToString(AreaTime.Rows[0]["HHMMSS"]).Substring(0, 2) + ":" + Convert.ToString(AreaTime.Rows[0]["HHMMSS"]).Substring(2, 2) + ":" + Convert.ToString(AreaTime.Rows[0]["HHMMSS"]).Substring(4, 2);


                dJobDate = DateTime.ParseExact(sJobDate + STRTTIME, "yyyyMMdd HH:mm:ss", null);


                iTerm = 120;
                iIncrease = 120;

                DataTable dtGetDate = new ClientProxy().ExecuteServiceSync("COR_SEL_GETDATE", null, "RSLTDT", null);

                for (int i = 0; i < 24 * 60 * 60 / iTerm; i++)
                {
                    RSLTDT.Rows.Add(dJobDate.AddSeconds(i * iIncrease).ToString("yyyyMMddHHmmss"), dJobDate.AddSeconds(i * iIncrease + (iIncrease - 1)).ToString("yyyyMMddHHmmss"));

                }

                DataTable RSLTDT1 = RSLTDT.Select("STARTTIME <=" + Convert.ToDateTime(dtGetDate.Rows[0]["SYSTIME"]).ToString("yyyyMMddHHmmss"), "").CopyToDataTable();


                ds.Tables.Add(RSLTDT1);

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
        /// 설비명 가져오기
        /// </summary>
        private DataTable GetEqptName(string sEqptID, string sEqptType)
        {
            DataTable RSLTDT = new DataTable();
            try
            {

                DataTable RQSTDT = new DataTable();

                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("EQPTID", typeof(string));
                RQSTDT.Columns.Add("EQPTTYPE", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["EQPTID"] = sEqptID;
                dr["EQPTTYPE"] = sEqptType;
                RQSTDT.Rows.Add(dr);

                RSLTDT = new ClientProxy().ExecuteServiceSync("DA_EQP_SEL_EQPTLOSS_EQPTTITLE_MNT", "RQSTDT", "RSLTDT", RQSTDT);

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }

            return RSLTDT;
        }

        /// <summary>
        /// 시간별 상태 목록 가져오기
        /// </summary>
        private DataTable GetEqptLossList(string sEqptID, string sEqptType, string sJobDate)
        {
            DataTable RSLTDT = new DataTable();
            try
            {


                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("EQPTID", typeof(string));
                //RQSTDT.Columns.Add("EQPTTYPE", typeof(string));
                RQSTDT.Columns.Add("WRK_DATE", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["EQPTID"] = sEqptID;
                //dr["EQPTTYPE"] = sEqptType;
                dr["WRK_DATE"] = sJobDate;
                RQSTDT.Rows.Add(dr);


                RSLTDT = new ClientProxy().ExecuteServiceSync("DA_EQP_PROC_EQPTLOSS_MAP_MNT", "RQSTDT", "RSLTDT", RQSTDT);

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }

            return RSLTDT;

        }



        private Hashtable rsToHash2(DataTable dt)
        {
            Hashtable hash_return = new Hashtable();
            try
            {
                int cnt = dt.Select().ToList().GroupBy(row => row["EQPTID"]).Count();
                List<System.Data.DataRow> list = dt.Select().ToList().GroupBy(row => row["EQPTID"]).Select(group => group.First()).ToList();
                for (int i = 0; i < list.Count(); i++)
                {
                    hash_return.Add(list[i]["EQPTID"], dt.Select("EQPTID = '" + list[i]["EQPTID"] + "'").CopyToDataTable());
                }

            }
            catch (Exception ex)
            {
                hash_return = null;
            }
            return hash_return;
        }

        #endregion



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
                            //color = System.Drawing.Color.White;
                            color = System.Drawing.Color.FromName(hash_loss_color[sType].ToString());
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

        private Brush ColorToBrush(System.Drawing.Color C)
        {
            return new SolidColorBrush(Color.FromArgb(C.A, C.R, C.G, C.B));
        }

        private void cboElecType_SelectionChanged(object sender, EventArgs e)
        {
            SetEquipment();
        }

        private void SetElecType()
        {
            try
            {

                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("CMCDTYPE", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["CMCDTYPE"] = "ELEC_TYPE";
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_COMM_CBO", "RQSTDT", "RSLTDT", RQSTDT);

                cboElecType.DisplayMemberPath = "CBO_NAME";
                cboElecType.SelectedValuePath = "CBO_CODE";
                cboElecType.ItemsSource = DataTableConverter.Convert(dtResult);
            }
            catch (Exception ex)
            {
            }
        }

        private void SetEquipment()
        {
            try
            {
                cboEquipment.ItemsSource = null;

                string strEqsg = string.Empty;
                for (int i = 0; i < cboEquipmentSegment.SelectedItems.Count; i++)
                {
                    if (i != cboEquipmentSegment.SelectedItems.Count - 1)
                    {
                        strEqsg += cboEquipmentSegment.SelectedItems[i] + ",";
                    }
                    else
                    {
                        strEqsg += cboEquipmentSegment.SelectedItems[i];
                    }
                }

                string str = string.Empty;
                for (int i = 0; i < cboProcess.SelectedItems.Count; i++)
                {
                    if (i != cboProcess.SelectedItems.Count - 1)
                    {
                        str += cboProcess.SelectedItems[i] + ",";
                    }
                    else
                    {
                        str += cboProcess.SelectedItems[i];
                    }
                }

                string strElec = string.Empty;
                for (int i = 0; i < cboElecType.SelectedItems.Count; i++)
                {
                    if (i != cboElecType.SelectedItems.Count - 1)
                    {
                        strElec += cboElecType.SelectedItems[i] + ",";
                    }
                    else
                    {
                        strElec += cboElecType.SelectedItems[i];
                    }
                }

                if (string.IsNullOrEmpty(str)) return;

                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";

                // 2024.05.27
                if (LoginInfo.CFG_SYSTEM_TYPE_CODE.Equals("F"))
                {
                    RQSTDT.Columns.Add("LANGID", typeof(string));
                    RQSTDT.Columns.Add("EQSGID", typeof(string));
                    RQSTDT.Columns.Add("PROCID", typeof(string));

                    DataRow dr = RQSTDT.NewRow();
                    dr["LANGID"] = LoginInfo.LANGID;
                    dr["EQSGID"] = strEqsg;
                    dr["PROCID"] = str;
                    RQSTDT.Rows.Add(dr);
                }
                else
                {
                    RQSTDT.Columns.Add("LANGID", typeof(string));
                    RQSTDT.Columns.Add("EQSGID", typeof(string));
                    RQSTDT.Columns.Add("PROCID", typeof(string));
                    RQSTDT.Columns.Add("ELEC_TYPE_CODE", typeof(string));

                    DataRow dr = RQSTDT.NewRow();
                    dr["LANGID"] = LoginInfo.LANGID;
                    dr["EQSGID"] = strEqsg;
                    dr["PROCID"] = str;
                    if (!cboElecType.MultiSelectionBoxSource.Cast<MultiSelectionBoxItem>().ElementAt(0).IsChecked && !string.IsNullOrEmpty(strElec))
                        dr["ELEC_TYPE_CODE"] = strElec;
                    RQSTDT.Rows.Add(dr);
                }

                //DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_EQUIPMENT_EQPT_LOSS_CBO", "RQSTDT", "RSLTDT", RQSTDT);
                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_EQUIPMENT_EQPTLEVEL_MAIN_CBO", "RQSTDT", "RSLTDT", RQSTDT);

                // 2024.05.27
                if (LoginInfo.CFG_SYSTEM_TYPE_CODE.Equals("F"))
                {
                    DataTable dtEquipmentTemp = new DataTable();
                    dtEquipmentTemp.Columns.Add("CBO_NAME", typeof(string));
                    dtEquipmentTemp.Columns.Add("CBO_CODE", typeof(string));

                    if (dtResult != null)
                    {
                        foreach (DataRow dr in dtResult.Rows)
                        {
                            DataRow[] drRows = dtEquipmentTemp.Select(string.Format("CBO_NAME='{0}' AND CBO_CODE='{1}'", new object[] { dr["CBO_NAME"], dr["CBO_CODE"] }));
                            if (drRows == null || drRows.Length < 1)
                            {
                                DataRow drTemp = dtEquipmentTemp.NewRow();
                                drTemp["CBO_NAME"] = dr["CBO_NAME"];
                                drTemp["CBO_CODE"] = dr["CBO_CODE"];
                                dtEquipmentTemp.Rows.Add(drTemp);
                            }
                        }
                    }

                    cboEquipment.DisplayMemberPath = "CBO_NAME";
                    cboEquipment.SelectedValuePath = "CBO_CODE";

                    cboEquipment.ItemsSource = DataTableConverter.Convert(dtEquipmentTemp);

                }
                else
                {
                    cboEquipment.DisplayMemberPath = "CBO_NAME";
                    cboEquipment.SelectedValuePath = "CBO_CODE";

                    cboEquipment.ItemsSource = DataTableConverter.Convert(dtResult);
                }

            }
            catch (Exception ex)
            {

            }
        }

        //private void cboProcess_SelectionChanged(object sender, EventArgs e)
        //{

        //}










        //private void btnExpandFrameLeft_Checked(object sender, RoutedEventArgs e)
        //{
        //    grUp.RowDefinitions[0].Height = new GridLength(0);

        //    LGC.GMES.MES.CMM001.GridLengthAnimation gla = new LGC.GMES.MES.CMM001.GridLengthAnimation();
        //    gla.From = new GridLength(1, GridUnitType.Star);
        //    gla.To = new GridLength(0, GridUnitType.Star); 
        //    gla.Duration = new TimeSpan(0, 0, 0, 0, 500);

        //    grUp.RowDefinitions[1].Height = new GridLength(0);


        //}

        //private void btnExpandFrameLeft_Unchecked(object sender, RoutedEventArgs e)
        //{
        //    grUp.RowDefinitions[0].Height = new GridLength(300);
        //    grUp.RowDefinitions[1].Height = new GridLength(8);
        //    grUp.RowDefinitions[2].Height = GridLength.Auto;
        //    grUp.RowDefinitions[3].Height = new GridLength(8);
        //    grUp.RowDefinitions[4].Height = new GridLength(1, GridUnitType.Star);

        //    LGC.GMES.MES.CMM001.GridLengthAnimation gla = new LGC.GMES.MES.CMM001.GridLengthAnimation();
        //    gla.From = new GridLength(0);
        //    gla.To = new GridLength(1);
        //    gla.Duration = new TimeSpan(0, 0, 0, 0, 500);
        //}

        private DataTable GetEqptLossFirstList(string sEqptID, string sEqptType, string sJobDate)
        {
            DataTable RSLTDT = new DataTable();
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("EQPTID", typeof(string));
                RQSTDT.Columns.Add("EQPTTYPE", typeof(string));
                RQSTDT.Columns.Add("WRK_DATE", typeof(string));
                RQSTDT.Columns.Add("SHIFT", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["EQPTID"] = sEqptID;
                dr["EQPTTYPE"] = sEqptType;
                dr["WRK_DATE"] = sJobDate;
                dr["SHIFT"] = string.Empty;
                RQSTDT.Rows.Add(dr);
                RSLTDT = new ClientProxy().ExecuteServiceSync("DA_EQP_PROC_EQPTLOSSMAP_FIRST", "RQSTDT", "RSLTDT", RQSTDT);

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }

            return RSLTDT;
        }

        private DataTable GetEqptNameTitle(string sEqptID, string sEqptType)
        {
            DataTable RSLTDT = new DataTable();
            try
            {

                DataTable RQSTDT = new DataTable();

                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("EQPTID", typeof(string));
                RQSTDT.Columns.Add("EQPTTYPE", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["EQPTID"] = sEqptID;
                dr["EQPTTYPE"] = sEqptType;
                RQSTDT.Rows.Add(dr);

                RSLTDT = new ClientProxy().ExecuteServiceSync("DA_EQP_SEL_EQPTLOSS_EQPTTITLE", "RQSTDT", "RSLTDT", RQSTDT);

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }

            return RSLTDT;
        }
    }
}
