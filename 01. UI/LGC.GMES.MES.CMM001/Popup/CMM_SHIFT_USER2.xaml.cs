/***************************************************************
 Created Date : 2016.08.18
      Creator : 
   Decription : 전지 5MEGA-GMES 구축 - 작업조, 작업자 조회 팝업
----------------------------------------------------------------
 [Change History]
  2016.12.22  INS 김동일K : 작업자 4열로 표시, 검색기능 제공, 폰트 폴드로 표기 관련 프로그램 수정.
  2017.01.17  유관수K : 사용자 구룹 표시추가
  2017.01.23  유관수K : 사용자 선택 했던 선택 자동 표시
  2023.02.22  김대현 : 작업자 이름 검색시 대소문자 구분없이 조회되도록 수정
  2023.08.09  김도형 : [E20230715-001963] [Ultium Cells PI Team] GMES 전극 공정 진척 => 작업조별 작업자 조회의 Operator 이름 검색기능 향상 요청'
  2024.01.13  이지은 : 중복 데이터가 존재할경우 파라미터 추가
****************************************************************/

using C1.WPF;
using C1.WPF.DataGrid;
using C1.WPF.DateTimeEditors;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using LGC.GMES.MES.CMM001.Extensions;
using System;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Collections.Generic;


namespace LGC.GMES.MES.CMM001.Popup
{
    /// <summary>
    /// CMM_SHIFT_USER2.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class CMM_SHIFT_USER2 : C1Window, IWorkArea
    {
        #region Declaration & Constructor

        private string _RetShiftCode = string.Empty;
        private string _RetShiftName = string.Empty;
        private string _RetWrkStrtTime = string.Empty;
        private string _RetWrkEndTime = string.Empty;

        private string _RetUserID = string.Empty;
        private string _RetUserName = string.Empty;
        private string _RetWrkGrID = string.Empty;
        private string _RetWrkGrName = string.Empty;

        private string _Shop = string.Empty;
        private string _Area = string.Empty;
        private string _Segment = string.Empty;
        private string _Proc = string.Empty;
        private string _Shift = string.Empty;
        private string _Worker = string.Empty;
        private string _Eqpt = string.Empty;
        private string _Save = string.Empty;
        private string _Caller = string.Empty;

        private string _ShiftUserSearchFlag = string.Empty; // [E20230715-001963] [Ultium Cells PI Team] GMES 전극 공정 진척 => 작업조별 작업자 조회의 Operator 이름 검색기능 향상 요청

        TextBox _target;

        Util _Util = new Util();

        public string SHIFTCODE
        {
            get { return _RetShiftCode; }
        }

        public string SHIFTNAME
        {
            get { return _RetShiftName; }
        }

        public string WRKSTRTTIME
        {
            get { return _RetWrkStrtTime; }
        }

        public string WRKENDTTIME
        {
            get { return _RetWrkEndTime; }
        }

        public string USERID
        {
            get { return _RetUserID; }
        }

        public string USERNAME
        {
            get { return _RetUserName; }
        }

        public TextBox TARGET
        {
            get { return _target; }
        }
        #endregion

        #region Initialize
        /// <summary>
        ///  Frame과 상호작용하기 위한 객체
        /// </summary>
        public IFrameOperation FrameOperation { get; set; }

        public CMM_SHIFT_USER2()
        {
            InitializeComponent();
        }

        public CMM_SHIFT_USER2(TextBox target)
        {
            InitializeComponent();
            _target = target;
            ApplyPermissions();
        }

        /// <summary>
        /// 화면내 버튼 권한 처리
        /// </summary>
        private void ApplyPermissions()
        {
            List<Button> listAuth = new List<Button>();
            //listAuth.Add(btnInReplace);
            Util.pageAuth(listAuth, FrameOperation.AUTHORITY);
        }
        #endregion

        #region Event
        private void C1Window_Loaded(object sender, RoutedEventArgs e)
        {
            object[] tmps = C1WindowExtension.GetParameters(this);

            if (tmps.Length >= 9)
            {
                _Shop = tmps[0].GetString();
                _Area = tmps[1].GetString();
                _Segment = tmps[2].GetString();
                _Proc = tmps[3].GetString();
                _Shift = tmps[4].GetString();
                _Worker = tmps[5].GetString();
                _Eqpt = tmps[6].GetString();
                _Save = tmps[7].GetString();
                _Caller = tmps[8].GetString();
            }
            else if (tmps.Length >= 7)
            {
                _Shop    = tmps[0].GetString();
                _Area    = tmps[1].GetString();
                _Segment = tmps[2].GetString();
                _Proc    = tmps[3].GetString();
                _Shift   = tmps[4].GetString();
                _Worker  = tmps[5].GetString();
                _Eqpt    = tmps[6].GetString();
                _Save    = tmps[7].GetString();
            }
            else
            {
                _Shop    = string.Empty;
                _Area    = string.Empty;
                _Segment = string.Empty;
                _Proc    = string.Empty;
                _Shift   = string.Empty;
                _Worker  = string.Empty;
                _Eqpt    = string.Empty;
            }

            SetdgShiftUserGrid();  // [E20230715-001963] [Ultium Cells PI Team] GMES 전극 공정 진척 => 작업조별 작업자 조회의 Operator 이름 검색기능 향상 요청

            GetShift();
            GetWrkGr();
        }

        protected virtual void OnDateTimeChanged(object sender, NullablePropertyChangedEventArgs<DateTime> e)
        {
            if (txtWorkStartDateTime != null && !string.IsNullOrEmpty(txtWorkStartDateTime.Text))
            {
                if (txtWorkStartDateTime.Text.Equals("24:00:00"))
                    txtWorkStartDateTime.Text = "00:00:00";
                if (((txtWorkEndDateTime.DateTime.Value - Convert.ToDateTime(txtWorkStartDateTime.Text)).TotalMinutes) > 1439)
                {
                    Util.MessageValidation("SFU2937"); // 작업시간은 24시간을 넘을 수 없습니다.
                    if (_RetWrkEndTime.Equals(""))
                        _RetWrkEndTime = string.Format("{0:yyyy-MM-dd HH:mm}", DateTime.Now);
                    txtWorkEndDateTime.DateTime = Convert.ToDateTime(_RetWrkEndTime);
                    return;
                }
                _RetWrkEndTime = string.Format("{0:yyyy-MM-dd HH:mm}", txtWorkEndDateTime.DateTime.Value);
            }
        }

        private void btnSelect_Click(object sender, RoutedEventArgs e)
        {
            int iSelCnt = 0;
            _RetUserID = "";
            _RetUserName = "";

            /*
            for (int i = 0; i < dgShiftUser.Rows.Count; i++)
            {
                for (int j = 1; j <= 5; j++)
                {
                    if (Util.NVC(dgShiftUser.GetCell(i, dgShiftUser.Columns["CHK_" + j].Index).Value) == "1")
                    {
                        iSelCnt = iSelCnt + 1;

                        if (iSelCnt == 1)
                        {
                            _RetUserID = Util.NVC(dgShiftUser.GetCell(i, dgShiftUser.Columns["USERID_" + j].Index).Value);
                            _RetUserName = Util.NVC(dgShiftUser.GetCell(i, dgShiftUser.Columns["USERNAME_" + j].Index).Value);
                        }
                        else
                        {
                            _RetUserID = _RetUserID + "," + Util.NVC(dgShiftUser.GetCell(i, dgShiftUser.Columns["USERID_" + j].Index).Value);
                            _RetUserName = _RetUserName + "," + Util.NVC(dgShiftUser.GetCell(i, dgShiftUser.Columns["USERNAME_" + j].Index).Value);
                        }
                    }
                }
            }
            */

            if (dgWorkUser.Rows.Count > 0)
            {
                for (int i = 0; i < dgWorkUser.Rows.Count; i++)
                {
                    _RetUserID += Util.NVC(DataTableConverter.GetValue(dgWorkUser.Rows[i].DataItem, "USERID")) + ",";
                    _RetUserName += Util.NVC(DataTableConverter.GetValue(dgWorkUser.Rows[i].DataItem, "USERNAME")) + ",";
                    iSelCnt++;
                }
                _RetUserID = _RetUserID.Substring(0, _RetUserID.Length - 1);
                _RetUserName = _RetUserName.Substring(0, _RetUserName.Length - 1);
            }

            if (dgShift.SelectedIndex < 0)
            {
                Util.MessageValidation("SFU1844");// 작업조를 선택하세요.
                return;
            }

            _RetShiftCode = Util.NVC(DataTableConverter.GetValue(dgShift.Rows[dgShift.SelectedIndex].DataItem, "SHFT_ID").ToString());
            _RetShiftName = Util.NVC(DataTableConverter.GetValue(dgShift.Rows[dgShift.SelectedIndex].DataItem, "SHFT_NAME").ToString());
            if (DataTableConverter.GetValue(dgShift.Rows[dgShift.SelectedIndex].DataItem, "SHFT_STRT_HMS") != null)
            {
                _RetWrkStrtTime = Util.NVC(DataTableConverter.GetValue(dgShift.Rows[dgShift.SelectedIndex].DataItem, "SHFT_STRT_HMS").ToString());
            }
            else
            {
                _RetWrkStrtTime = string.Format("{0:yyyy-MM-dd HH:mm}", DateTime.Now);
            }

            if (_RetWrkEndTime.Equals(""))
                _RetWrkEndTime = Util.NVC(DataTableConverter.GetValue(dgShift.Rows[dgShift.SelectedIndex].DataItem, "SHFT_END_HMS").ToString());

            if (iSelCnt == 0)
            {
                Util.MessageValidation("SFU1842"); // 작업자를 선택 하세요.
                return;
            }

            //FORM001_DEFECT 에서는 한명만 선택하도록 함.
            if ("FORM001_DEFECT".Equals(_Caller))
            {
                if (iSelCnt > 1)
                {
                    Util.MessageValidation("SFU4989"); // 작업자는 한명만 선택해 주세요.
                    return;
                }
            }

            if(_Save.Equals("Y"))
            {
                SaveWRKHistory();
            }
            else
            {
                this.DialogResult = MessageBoxResult.OK;
            }
        }

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = MessageBoxResult.Cancel;
        }

        private void dgWorker_Checked()
        {
            if (!_Worker.Equals(""))
            {
                for (int j = 0; j < _Worker.Split(',').Length; j++)
                {
                    if (!_Worker.Split(',')[j].Trim().Equals(""))
                    {
                        for (int i = 0; i < dgShiftUser.Rows.Count; i++)
                        {
                            for (int k = 1; k <= 5; k++)
                            {
                                if (Util.NVC(dgShiftUser.GetCell(i, dgShiftUser.Columns["USERID_" + k].Index).Value).Equals(_Worker.Split(',')[j].Trim()))
                                {
                                    DataTableConverter.SetValue(dgShiftUser.Rows[i].DataItem, "CHK_" + k, true);
                                    dgShiftUser.ScrollIntoView(i, dgShiftUser.Columns["CHK_" + k].Index);
                                    SetSelUserName(dgShiftUser.Rows[i].DataItem, k, false);
                                }
                            }
                        }
                    }
                }
            }
        }

        private void dgWorkerDefault_Checked()
        {
            for (int i = 0; i < dgShiftUser.Rows.Count; i++)
            {
                for (int j = 0; j < dgWorkUser.Rows.Count; j++)
                {
                    for (int k = 1; k <= 5; k++)
                    {
                        if (Util.NVC(dgShiftUser.GetCell(i, dgShiftUser.Columns["USERID_" + k].Index).Value).Equals(DataTableConverter.GetValue(dgWorkUser.Rows[j].DataItem, "USERID")))
                        {
                            DataTableConverter.SetValue(dgShiftUser.Rows[i].DataItem, "CHK_" + k, true);

                            dgShiftUser.ScrollIntoView(i, dgShiftUser.Columns["CHK_" + k].Index);
                        }
                    }
                }
            }
        }

        private void dgShiftUser_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            // 더블클릭 선택
            if (sender == null)
                return;

            C1DataGrid dg = (sender as C1DataGrid);

            if (dg == null || dg.CurrentCell == null || dg.CurrentCell.Row == null)
                return;

            int idx = dg.CurrentCell.Row.Index;
            if (idx < 0)
                return;

            for (int j = 1; j <= 5; j++)
            {
                if (DataTableConverter.GetValue(dg.Rows[idx].DataItem, "USERID_" + j) == null) return; // 2024.11.14. 김선영 - 전체 선택 시 작업자명이 없는 것들은 Pass 한다.

                if (dg.CurrentCell.Column.Index == dgShiftUser.Columns["USERNAME_" + j].Index)
                {
                    if (DataTableConverter.GetValue(dg.Rows[idx].DataItem, "CHK_" + j).ToString().Equals("1"))
                    {
                        DataTableConverter.SetValue(dg.Rows[idx].DataItem, "CHK_" + j, false);
                        SetSelUserName(dg.Rows[idx].DataItem, j, true);
                    }
                    else
                    {
                        DataTableConverter.SetValue(dg.Rows[idx].DataItem, "CHK_" + j, true);
                        SetSelUserName(dg.Rows[idx].DataItem, j);
                    }
                }

                if (dg.CurrentCell.Column.Index == dgShiftUser.Columns["CHK_" + j].Index)
                {
                    if (DataTableConverter.GetValue(dg.Rows[idx].DataItem, "CHK_" + j).ToString().Equals("1"))
                    {
                        SetSelUserName(dg.Rows[idx].DataItem, j);
                    }
                    else
                    {
                        SetSelUserName(dg.Rows[idx].DataItem, j, true);
                    }
                }
            }
            dgShiftUser.Refresh(true);

            // 더블클릭 선택완료
            //int iSelCnt = 0;
            //_RetUserID = string.Empty;
            //_RetUserName = string.Empty;

            //for (int i = 0; i < dgShiftUser.Rows.Count; i++)
            //{
            //    for (int j = 1; j <= 4; j++)
            //    {
            //        if (Util.NVC(dgShiftUser.GetCell(i, dgShiftUser.Columns["CHK_" + j].Index).Value).Equals("1"))
            //        {
            //            if (string.IsNullOrEmpty(Util.NVC(dgShiftUser.GetCell(i, dgShiftUser.Columns["USERID_" + j].Index).Value)))
            //                return;

            //            iSelCnt = iSelCnt + 1;

            //            if (iSelCnt == 1)
            //            {
            //                _RetUserID = Util.NVC(dgShiftUser.GetCell(i, dgShiftUser.Columns["USERID_" + j].Index).Value);
            //                _RetUserName = Util.NVC(dgShiftUser.GetCell(i, dgShiftUser.Columns["USERNAME_" + j].Index).Value);
            //            }
            //            else
            //            {
            //                _RetUserID = _RetUserID + "," + Util.NVC(dgShiftUser.GetCell(i, dgShiftUser.Columns["USERID_" + j].Index).Value);
            //                _RetUserName = _RetUserName + "," + Util.NVC(dgShiftUser.GetCell(i, dgShiftUser.Columns["USERNAME_" + j].Index).Value);
            //            }
            //        }
            //    }
            //}

            //if (iSelCnt == 0)
            //{
            //    Util.MessageValidation("SFU1651");    //선택된 항목이 없습니다.
            //    return;
            //}
            //btnSelect_Click(sender,e);
        }

        private void dgShiftUser_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (sender == null)
                return;

            C1DataGrid dg = (sender as C1DataGrid);

            if (dg == null || dg.CurrentCell == null || dg.CurrentCell.Row == null)
                return;

            int idx = dg.CurrentCell.Row.Index;
            if (idx < 0)
                return;

            for (int j = 1; j <= 5; j++)
            {
                if (DataTableConverter.GetValue(dg.Rows[idx].DataItem, "USERID_" + j) == null) return; // 2024.11.14. 김선영 - 전체 선택 시 작업자명이 없는 것들은 Pass 한다.

                if (dg.CurrentCell.Column.Index == dgShiftUser.Columns["USERNAME_" + j].Index)
                {
                    if (DataTableConverter.GetValue(dg.Rows[idx].DataItem, "CHK_" + j).ToString().Equals("1"))
                    {
                        DataTableConverter.SetValue(dg.Rows[idx].DataItem, "CHK_" + j, false);
                        SetSelUserName(dg.Rows[idx].DataItem, j, true);
                    }
                    else
                    {
                        DataTableConverter.SetValue(dg.Rows[idx].DataItem, "CHK_" + j, true);
                        SetSelUserName(dg.Rows[idx].DataItem, j);
                    }
                }

                if (dg.CurrentCell.Column.Index == dgShiftUser.Columns["CHK_" + j].Index)
                {
                    if (DataTableConverter.GetValue(dg.Rows[idx].DataItem, "CHK_" + j).ToString().Equals("1"))
                    {
                        SetSelUserName(dg.Rows[idx].DataItem, j);
                    }
                    else
                    {
                        SetSelUserName(dg.Rows[idx].DataItem, j, true);
                    }
                }
            }
            dgShiftUser.Refresh(true);
        }

        private void dgGrList_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (sender == null)
                return;

            C1DataGrid dg = (sender as C1DataGrid);

            if (dg == null || dg.CurrentCell == null || dg.CurrentCell.Row == null)
                return;

            int idx = dg.CurrentCell.Column.Index;
            if (idx < 0)
                return;

            for (int i = 0; i < dgGrList.Rows.Count; i++)
            {
                for (int j = 1; j <= 3; j++)
                {
                    if (idx == dgGrList.Columns["WRK_GR_NAME_" + j].Index)
                    {
                        DataTableConverter.SetValue(dg.Rows[i].DataItem, "CHK_" + j, true);
                        
                    }
                    else
                    {
                        DataTableConverter.SetValue(dg.Rows[i].DataItem, "CHK_" + j, false);
                        dgGrList.SelectedIndex = 0;
                    }
                }
            }
            dgGrList.SelectedIndex = idx;

        }

        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            if (txtUserName.Text.Trim().Equals(""))
                return;

            for (int i = 0; i < dgShiftUser.Rows.Count; i++)
            {
                for (int j = 1; j <= 5; j++)
                {
                    // [E20230715-001963] [Ultium Cells PI Team] GMES 전극 공정 진척 => 작업조별 작업자 조회의 Operator 이름 검색기능 향상 요청
                    if (Util.NVC(dgShiftUser.GetCell(i, dgShiftUser.Columns["USERNAME_" + j].Index).Value.ToString().ToUpper()).Equals(txtUserName.Text.Trim().ToString().ToUpper()))
                    {
                        DataTableConverter.SetValue(dgShiftUser.Rows[i].DataItem, "CHK_" + j, true);
                        dgShiftUser.ScrollIntoView(i, dgShiftUser.Columns["CHK_" + j].Index);

                        SetSelUserName(dgShiftUser.Rows[i].DataItem, j);
                    }
                }
            }

            txtUserName.Text = "";
            dgShiftUser.Refresh(true);
        }

        private void txtUserName_KeyUp(object sender, KeyEventArgs e)
        {
            if (sender == null)
                return;

            if (e.Key == Key.Enter)
            {
                if (txtUserName.Text.Trim().Equals(""))
                    return;

                for (int i = 0; i < dgShiftUser.Rows.Count; i++)
                {
                    for (int j = 1; j <= 5; j++)
                    {
                        //2023.02.22 김대현
                        //if (Util.NVC(dgShiftUser.GetCell(i, dgShiftUser.Columns["USERNAME_" + j].Index).Value).Equals(txtUserName.Text.Trim()))
                        if (Util.NVC(dgShiftUser.GetCell(i, dgShiftUser.Columns["USERNAME_" + j].Index).Value).ToUpper().Equals(txtUserName.Text.Trim().ToUpper()))
                        {
                            DataTableConverter.SetValue(dgShiftUser.Rows[i].DataItem, "CHK_" + j, true);
                            dgShiftUser.ScrollIntoView(i, dgShiftUser.Columns["CHK_" + j].Index);

                            SetSelUserName(dgShiftUser.Rows[i].DataItem, j);
                        }
                    }
                }

                txtUserName.Text = "";
                dgShiftUser.Refresh(true);
            }
        }

        private void dgShift_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            C1.WPF.DataGrid.C1DataGrid datagrid = (sender as C1.WPF.DataGrid.C1DataGrid);
            if (datagrid.CurrentRow == null || datagrid.CurrentRow.Index < 0)
                return;

            if (_Util.GetDataGridCheckFirstRowIndex(datagrid, "CHK") < 0)
            {
                Util.MessageValidation("SFU1275");//선택된 항목이 없습니다.
                return;
            }

            _RetShiftCode = DataTableConverter.GetValue(datagrid.Rows[_Util.GetDataGridCheckFirstRowIndex(datagrid, "CHK")].DataItem, "SHFT_ID").GetString();
            _RetShiftName = DataTableConverter.GetValue(datagrid.Rows[_Util.GetDataGridCheckFirstRowIndex(datagrid, "CHK")].DataItem, "SHFT_NAME").GetString();
            if (DataTableConverter.GetValue(datagrid.Rows[_Util.GetDataGridCheckFirstRowIndex(datagrid, "CHK")].DataItem, "SHFT_STRT_HMS") != null)
            {
                _RetWrkStrtTime = DataTableConverter.GetValue(datagrid.Rows[_Util.GetDataGridCheckFirstRowIndex(datagrid, "CHK")].DataItem, "SHFT_STRT_HMS").GetString();
            }
            else
            {
                _RetWrkStrtTime = string.Format("{0:yyyy-MM-dd HH:mm}", DateTime.Now);
            }
            if (_RetWrkEndTime.Equals(""))
            {
                _RetWrkEndTime = DataTableConverter.GetValue(datagrid.Rows[_Util.GetDataGridCheckFirstRowIndex(datagrid, "CHK")].DataItem, "SHFT_END_HMS").GetString();
            }

            int iSelCnt = 0;
            _RetUserID = string.Empty;
            _RetUserName = string.Empty;

            for (int i = 0; i < dgShiftUser.Rows.Count; i++)
            {
                for (int j = 1; j <= 5; j++)
                {
                    if (Util.NVC(dgShiftUser.GetCell(i, dgShiftUser.Columns["CHK_" + j].Index).Value) == "1")
                    {
                        iSelCnt = iSelCnt + 1;

                        if (iSelCnt == 1)
                        {
                            _RetUserID = Util.NVC(dgShiftUser.GetCell(i, dgShiftUser.Columns["USERID_" + j].Index).Value);
                            _RetUserName = Util.NVC(dgShiftUser.GetCell(i, dgShiftUser.Columns["USERNAME_" + j].Index).Value);
                        }
                        else
                        {
                            _RetUserID = _RetUserID + "," + Util.NVC(dgShiftUser.GetCell(i, dgShiftUser.Columns["USERID_" + j].Index).Value);
                            _RetUserName = _RetUserName + "," + Util.NVC(dgShiftUser.GetCell(i, dgShiftUser.Columns["USERNAME_" + j].Index).Value);
                        }
                    }
                }
            }
            if (iSelCnt == 0)
            {
                Util.MessageValidation("SFU1842");    // 작업자를 선택 하세요.
                return;
            }
            this.DialogResult = MessageBoxResult.OK;
        }

        private void dgShift_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (sender == null)
                return;

            C1DataGrid dg = (sender as C1DataGrid);

            if (dg == null || dg.CurrentCell == null || dg.CurrentCell.Row == null)
                return;

            int idx = dg.CurrentCell.Row.Index;
            if (idx < 0)
                return;
            dgShift.SelectedIndex = idx;

            DataTableConverter.SetValue(dg.Rows[idx].DataItem, "CHK", true);
        }

        private void dgShiftChoice_Checked(object sender, RoutedEventArgs e)
        {
            DataTable IndataTable = new DataTable();
            IndataTable.Columns.Add("AREAID", typeof(string));

            DataRow Indata = IndataTable.NewRow();
            Indata["AREAID"] = LoginInfo.CFG_AREA_ID;
            IndataTable.Rows.Add(Indata);

            DataTable dateDt = new ClientProxy().ExecuteServiceSync("CUS_SEL_AREAATTR", "INDATA", "RSLTDT", IndataTable);

            DateTime currDate = GetCurrentTime();
            DateTime CurrDate2;
            string currTime = currDate.ToString("HHmmss");
            string baseTime = string.IsNullOrEmpty(Util.NVC(dateDt.Rows[0]["S02"])) ? "000000" : Util.NVC(dateDt.Rows[0]["S02"]);

            if (Util.NVC_Decimal(currTime) - Util.NVC_Decimal(baseTime) < 0)
            {
                CurrDate2 = currDate.AddDays(-1);
            }
            else
            {
                CurrDate2 = currDate;
            }
            RadioButton rb = sender as RadioButton;

            if (rb.DataContext == null)
                return;

            if ((bool)rb.IsChecked && (rb.DataContext as DataRowView).Row["CHK"].ToString().Equals("0")|| (bool)rb.IsChecked && (rb.DataContext as DataRowView).Row["CHK"].ToString().Equals("1"))
            {
                int idx = ((C1.WPF.DataGrid.DataGridCellPresenter)rb.Parent).Row.Index;
                DataRow dtRow = (rb.DataContext as DataRowView).Row;

                C1.WPF.DataGrid.C1DataGrid dg = ((C1.WPF.DataGrid.DataGridCellPresenter)rb.Parent).DataGrid;
                for (int i = 0; i < dg.GetRowCount(); i++)
                {
                    if (idx == i)
                    {
                        DataTableConverter.SetValue(dg.Rows[i].DataItem, ((C1.WPF.DataGrid.DataGridCellPresenter)rb.Parent).Column.Name, true);
                        txtWorkDate.Text = string.Format("{0:yyyy-MM-dd}", CurrDate2 );//DateTime.Now
                        txtShift.Text = DataTableConverter.GetValue(dg.Rows[i].DataItem, "SHFT_NAME").GetString();
                        txtShift.Tag = DataTableConverter.GetValue(dg.Rows[i].DataItem, "SHFT_ID").GetString();
                        txtWorkStartDateTime.Text = DataTableConverter.GetValue(dg.Rows[i].DataItem, "SHFT_STRT").GetString();
                        txtWorkStartDateTime.Tag = string.Format("{0:yyyy-MM-dd}", CurrDate2) + " " + DataTableConverter.GetValue(dg.Rows[i].DataItem, "SHFT_STRT").GetString();
                        if (Util.NVC_Decimal(string.Format("{0:HHmmss}", Convert.ToDateTime(DataTableConverter.GetValue(dg.Rows[i].DataItem, "SHFT_END_HMS").ToString()))) - Util.NVC_Decimal(baseTime) < 0)
                        {
                            txtWorkEndDateTime.DateTime = Convert.ToDateTime(DataTableConverter.GetValue(dg.Rows[i].DataItem, "SHFT_END_HMS").ToString()); //이후시간
                        }
                        else
                        {
                            txtWorkEndDateTime.DateTime = Convert.ToDateTime(DataTableConverter.GetValue(dg.Rows[i].DataItem, "SHFT_END_HMS").ToString()); // 이전시간
                        }

                        txtWorkEndDateTime.Tag = DataTableConverter.GetValue(dg.Rows[i].DataItem, "SHFT_END_HMS").GetString();
                        _RetWrkEndTime = DataTableConverter.GetValue(dg.Rows[i].DataItem, "SHFT_END_HMS").GetString();
                    }
                    else
                    {
                        DataTableConverter.SetValue(dg.Rows[i].DataItem, ((C1.WPF.DataGrid.DataGridCellPresenter)rb.Parent).Column.Name, false);
                    }
                }
                dgShift.SelectedIndex = idx;
            }
        }

        private void btnDel_Click(object sender, RoutedEventArgs e)
        {

            if (dgWorkUser.CurrentRow.DataItem == null)
                return;

            int rowIndex = ((C1.WPF.DataGrid.DataGridCellPresenter)((sender as Button).Parent)).Row.Index;
            DataTable dt = DataTableConverter.Convert(dgWorkUser.ItemsSource);

            for (int i = 0; i < dgShiftUser.Rows.Count; i++)
            {
                for (int j = 1; j <= 5; j++)
                {
                    if (Util.NVC(dgShiftUser.GetCell(i, dgShiftUser.Columns["USERID_" + j].Index).Value).Equals(dt.Rows[rowIndex].ItemArray[1]))
                    {
                        DataTableConverter.SetValue(dgShiftUser.Rows[i].DataItem, "CHK_" + j, false);
                        dgShiftUser.ScrollIntoView(i, dgShiftUser.Columns["CHK_" + j].Index);
                        SetSelUserName(dgShiftUser.Rows[i].DataItem, j, true);
                    }
                }
            }
            dgShiftUser.Refresh(true);
        }

        // [E20230715-001963] [Ultium Cells PI Team] GMES 전극 공정 진척 => 작업조별 작업자 조회의 Operator 이름 검색기능 향상 요청
        private void SetdgShiftUserGrid()
        {
            _ShiftUserSearchFlag = GetShiftUserSearchFlag();

            if (Util.NVC(_ShiftUserSearchFlag).Equals("Y"))
            {
                dgShiftUser.Columns["CHK_3"].Visibility = Visibility.Collapsed;
                dgShiftUser.Columns["USERID_3"].Visibility = Visibility.Collapsed;
                dgShiftUser.Columns["USERNAME_3"].Visibility = Visibility.Collapsed;
                dgShiftUser.Columns["DEPTNAME_3"].Visibility = Visibility.Collapsed;

                dgShiftUser.Columns["CHK_4"].Visibility = Visibility.Collapsed;
                dgShiftUser.Columns["USERID_4"].Visibility = Visibility.Collapsed;
                dgShiftUser.Columns["USERNAME_4"].Visibility = Visibility.Collapsed;
                dgShiftUser.Columns["DEPTNAME_4"].Visibility = Visibility.Collapsed;

                dgShiftUser.Columns["CHK_5"].Visibility = Visibility.Collapsed;
                dgShiftUser.Columns["USERID_5"].Visibility = Visibility.Collapsed;
                dgShiftUser.Columns["USERNAME_5"].Visibility = Visibility.Collapsed;
                dgShiftUser.Columns["DEPTNAME_5"].Visibility = Visibility.Collapsed;

                dgShiftUser.Columns["USERNAME_1"].Width = new C1.WPF.DataGrid.DataGridLength(287);
                dgShiftUser.Columns["USERNAME_1"].HorizontalAlignment = HorizontalAlignment.Left;

                dgShiftUser.Columns["USERNAME_2"].Width = new C1.WPF.DataGrid.DataGridLength(287);
                dgShiftUser.Columns["USERNAME_2"].HorizontalAlignment = HorizontalAlignment.Left;
                
            }

        }
        #endregion

        #region Mehod

        #region [BizCall]
        private void GetShiftUser(string wrkGrID)
        {
            string sBizName = string.Empty;             // [E20230715-001963] [Ultium Cells PI Team] GMES 전극 공정 진척 => 작업조별 작업자 조회의 Operator 이름 검색기능 향상 요청

            DataTable searchConditionTable = new DataTable();
            searchConditionTable.Columns.Add("LANGID", typeof(string));
            searchConditionTable.Columns.Add("AREAID", typeof(string));
            searchConditionTable.Columns.Add("PROCID", typeof(string));
            searchConditionTable.Columns.Add("WRK_GR_ID", typeof(string));

            DataRow searchCondition = searchConditionTable.NewRow();
            searchCondition["LANGID"] = LoginInfo.LANGID;
            searchCondition["AREAID"] = _Area;
            searchCondition["PROCID"] = _Proc;
            if (!wrkGrID.Equals(""))
                searchCondition["WRK_GR_ID"] = wrkGrID;

            searchConditionTable.Rows.Add(searchCondition);

            if (Util.NVC(_ShiftUserSearchFlag).Equals("Y")) // [E20230715-001963] [Ultium Cells PI Team] GMES 전극 공정 진척 => 작업조별 작업자 조회의 Operator 이름 검색기능 향상 요청
            {
                sBizName = "DA_BAS_SEL_TB_MMD_AREA_PROC_USER5_V01";
            }
            else
            {
                sBizName = "DA_BAS_SEL_TB_MMD_AREA_PROC_USER5";
            }
                        
            new ClientProxy().ExecuteService(sBizName, "INDATA", "OUTDATA", searchConditionTable, (searchResult, searchException) =>
            {
                try
                {
                    if (searchException != null)
                    {
                        Util.AlertByBiz(sBizName, searchException.Message, searchException.ToString()); // [E20230715-001963] [Ultium Cells PI Team] GMES 전극 공정 진척 => 작업조별 작업자 조회의 Operator 이름 검색기능 향상 요청
                        return;
                    }

                    dgShiftUser.ItemsSource = DataTableConverter.Convert(searchResult);
                    dgShiftUser.ColumnWidth = new C1.WPF.DataGrid.DataGridLength(1, DataGridUnitType.Star);
                    //Util.gridClear(dgWorkUser);
                    dgWorker_Checked();
                    dgWorkerDefault_Checked();
                    _Worker = "";
                }
                catch (Exception ex)
                {
                    Util.MessageException(ex);
                }
            });
        }

        private void GetWrkGr()
        {
            DataTable IndataTable = new DataTable("RQSTDT");
            IndataTable.Columns.Add("LANGID", typeof(string));
            IndataTable.Columns.Add("AREAID", typeof(string));
            IndataTable.Columns.Add("PROCID", typeof(string));

            DataRow Indata = IndataTable.NewRow();
            Indata["LANGID"] = LoginInfo.LANGID;
            Indata["AREAID"] = _Area;
            Indata["PROCID"] = _Proc;

            IndataTable.Rows.Add(Indata);

            new ClientProxy().ExecuteService("DA_BAS_SEL_TB_MMD_AREA_PROC_GR_LIST", "RQSTDT", "RSLTDT", IndataTable, (result, searchException) =>
            {
                try
                {
                    if (searchException != null)
                        throw searchException;
                    if (!result.Rows[0].ItemArray[6].Equals(""))
                    {
                        dgGrList.ItemsSource = DataTableConverter.Convert(result);
                        dgGrList.ColumnWidth = new C1.WPF.DataGrid.DataGridLength(1, DataGridUnitType.Star);
                        DataTableConverter.SetValue(dgGrList.Rows[0].DataItem, "CHK_1", true);
                        //GetShiftUser(Util.NVC(result.Rows[0]["WRK_GR_ID_1"]));
                    }
                    else
                    {
                        Util.MessageValidation("SFU2956");//지정된 그룹이 없습니다.
                        GetShiftUser("");
                    }
                }
                catch (Exception ex)
                {
                    Util.MessageException(ex);
                }
            });
        }

        private void GetShift()
        {
            DataTable searchConditionTable = new DataTable();
            searchConditionTable.Columns.Add("LANGID", typeof(string));
            searchConditionTable.Columns.Add("SHOPID", typeof(string));
            searchConditionTable.Columns.Add("AREAID", typeof(string));
            searchConditionTable.Columns.Add("EQSGID", typeof(string));
            searchConditionTable.Columns.Add("PROCID", typeof(string));

            DataRow searchCondition = searchConditionTable.NewRow();
            searchCondition["LANGID"] = LoginInfo.LANGID;
            searchCondition["SHOPID"] = _Shop;
            searchCondition["AREAID"] = _Area;
            searchCondition["EQSGID"] = _Segment;
            searchCondition["PROCID"] = _Proc;

            searchConditionTable.Rows.Add(searchCondition);

            new ClientProxy().ExecuteService("DA_BAS_SEL_SHIFT3", "INDATA", "OUTDATA", searchConditionTable, (searchResult, searchException) =>
            {
                try
                {
                    if (searchException != null)
                    {
                        Util.AlertByBiz("DA_BAS_SEL_SHIFT3", searchException.Message, searchException.ToString());
                        return;
                    }

                    dgShift.ItemsSource = DataTableConverter.Convert(searchResult);
                    dgShift.ColumnWidth = new C1.WPF.DataGrid.DataGridLength(1, DataGridUnitType.Star);
                    dgShift_Checked();
                }
                catch (Exception ex)
                {
                    Util.MessageException(ex);
                }
                finally
                {
                    //Logger.Instance.WriteLine(Logger.OPERATION_R + "GetProductLot", Logger.MESSAGE_OPERATION_END);
                }
            }
            );
        }

        private void SaveWRKHistory()
        {
            DataTable inDataTable = new DataTable();
            inDataTable.Columns.Add("WRK_STRT_DTTM", typeof(DateTime));
            inDataTable.Columns.Add("WRK_END_DTTM", typeof(DateTime));
            inDataTable.Columns.Add("WRK_USERID", typeof(string));
            inDataTable.Columns.Add("EQPTID", typeof(string));
            inDataTable.Columns.Add("SHFT_ID", typeof(string));
            inDataTable.Columns.Add("USERID", typeof(string));

            DataRow inLotDetailDataRow = null;
            inLotDetailDataRow = inDataTable.NewRow();
            inLotDetailDataRow["WRK_STRT_DTTM"] = Convert.ToDateTime(_RetWrkStrtTime);
            inLotDetailDataRow["WRK_END_DTTM"] = Convert.ToDateTime(_RetWrkEndTime);
            inLotDetailDataRow["WRK_USERID"] = _RetUserID;
            inLotDetailDataRow["EQPTID"] = _Eqpt;
            inLotDetailDataRow["SHFT_ID"] = _RetShiftCode;
            inLotDetailDataRow["USERID"] = LoginInfo.USERID.Trim();
            inDataTable.Rows.Add(inLotDetailDataRow);

            new ClientProxy().ExecuteService("BR_PRD_REG_TB_SFC_EQPT_WRK_INFO", "INDATA", null, inDataTable, (result, Returnex) =>
            {
                try
                {
                    System.Windows.Forms.Cursor.Current = System.Windows.Forms.Cursors.WaitCursor;
                    if (Returnex != null)
                    {
                        System.Windows.Forms.Cursor.Current = System.Windows.Forms.Cursors.Default;
                        Util.MessageException(Returnex);
                        return;
                    }
                    this.DialogResult = MessageBoxResult.OK;
                }
                catch (Exception ex)
                {
                    System.Windows.Forms.Cursor.Current = System.Windows.Forms.Cursors.Default;
                    //LGC.GMES.MES.ControlsLibrary.MessageBox.ShowKeyEnter(MessageDic.Instance.GetMessage("SFU2938"), ex.Message, "Info", MessageBoxButton.OK, ControlsLibrary.MessageBoxIcon.None); // 작업자 시간 정보 저장
                    Util.MessageException(ex);
                }
            });
        }

        private void dgShift_Checked()
        {
            DataTable IndataTable = new DataTable();
            IndataTable.Columns.Add("AREAID", typeof(string));

            DataRow Indata = IndataTable.NewRow();
            Indata["AREAID"] = LoginInfo.CFG_AREA_ID;
            IndataTable.Rows.Add(Indata);

            DataTable dateDt = new ClientProxy().ExecuteServiceSync("CUS_SEL_AREAATTR", "INDATA", "RSLTDT", IndataTable);

            DateTime currDate = GetCurrentTime();
            DateTime CurrDate2;
            string currTime = currDate.ToString("HHmmss");
            string baseTime = string.IsNullOrEmpty(Util.NVC(dateDt.Rows[0]["S02"])) ? "000000" : Util.NVC(dateDt.Rows[0]["S02"]);

            if (Util.NVC_Decimal(currTime) - Util.NVC_Decimal(baseTime) < 0)
            {
                CurrDate2 = currDate.AddDays(-1);
            }
            else
            {
                CurrDate2 = currDate;
            }

                for (int i = 0; i < dgShift.Rows.Count; i++)
                {
                    if (DataTableConverter.GetValue(dgShift.Rows[i].DataItem, "SHFT_ID").Equals(_Shift))  //_Shift
                    {
                        DataTableConverter.SetValue(dgShift.Rows[i].DataItem, "CHK", true);
                        dgShift.SelectedIndex = i;
                        txtWorkDate.Text = string.Format("{0:yyyy-MM-dd}", CurrDate2);
                        txtShift.Text = DataTableConverter.GetValue(dgShift.Rows[i].DataItem, "SHFT_NAME").GetString();
                        txtWorkStartDateTime.Text = DataTableConverter.GetValue(dgShift.Rows[i].DataItem, "SHFT_STRT").GetString();
                        txtWorkEndDateTime.DateTime = Convert.ToDateTime(DataTableConverter.GetValue(dgShift.Rows[i].DataItem, "SHFT_END_HMS").ToString());
                        _RetWrkEndTime = DataTableConverter.GetValue(dgShift.Rows[i].DataItem, "SHFT_END_HMS").GetString();
                    }
                }
        }

        private DateTime GetCurrentTime()
        {
            try
            {
                DataTable dt = new ClientProxy().ExecuteServiceSync("BR_CUS_GET_SYSTIME", null, "OUTDATA", null);
                return (DateTime)dt.Rows[0]["SYSTIME"];
            }
            catch (Exception ex) { }

            return DateTime.Now;
        }

        // [E20230715-001963] [Ultium Cells PI Team] GMES 전극 공정 진척 => 작업조별 작업자 조회의 Operator 이름 검색기능 향상 요청
        private string GetShiftUserSearchFlag()
        {

            string sShiftUserSearchFlag = "N";
            string sCodeType;
            string sCmCode;
            string[] sAttribute = null;

            sCodeType = "SHIFT_USER_SEARCH_FLAG";
            sCmCode = null;

            try
            {
                string[] sColumnArr = { "ATTR1", "ATTR2", "ATTR3", "ATTR4", "ATTR5" };

                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("AREAID", typeof(string));
                RQSTDT.Columns.Add("COM_TYPE_CODE", typeof(string));
                RQSTDT.Columns.Add("COM_CODE", typeof(string));
                RQSTDT.Columns.Add("USE_FLAG", typeof(string));

                for (int i = 0; i < sColumnArr.Length; i++)
                    RQSTDT.Columns.Add(sColumnArr[i], typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["AREAID"] = LoginInfo.CFG_AREA_ID;
                dr["COM_TYPE_CODE"] = sCodeType;
                dr["COM_CODE"] = (sCmCode == string.Empty ? null : sCmCode);
                dr["USE_FLAG"] = "Y";

                if (sAttribute != null)
                {
                    for (int i = 0; i < sAttribute.Length; i++)
                        dr[sColumnArr[i]] = string.IsNullOrEmpty(sAttribute[i]) ? (object)DBNull.Value : sAttribute[i];
                }
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_TB_MMD_AREA_COM_CODE_ATTR", "RQSTDT", "RSLTDT", RQSTDT);


                if (dtResult != null && dtResult.Rows.Count > 0)
                {
                    sShiftUserSearchFlag = "Y";   // Util.NVC(dtResult.Rows[0]["ATTR1"].ToString());

                }
                else
                {
                    sShiftUserSearchFlag = "N";
                }

                return sShiftUserSearchFlag;
            }
            catch (Exception ex)
            {
                // Util.MessageException(ex); 
                return sShiftUserSearchFlag;
            }
        }
        #endregion

        #endregion


        private void dgShiftUser_LoadedCellPresenter(object sender, DataGridCellEventArgs e)
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

                //Grid Data Binding 이용한 Background 색 변경
                if (e.Cell.Row.Type == DataGridRowType.Item)
                {
                    e.Cell.Presenter.FontWeight = FontWeights.Bold;
                }

                // 해당 ID없는것도 체크 박스 나와서 선택되는 문제 수정을 위하여 추가 ( 2017-02-11 )
                C1.WPF.DataGrid.C1DataGrid dgUser = sender as C1.WPF.DataGrid.C1DataGrid;
                //if ( e.Cell.Column.Index % 5 == 0)
                //{
                    CheckBox chkBox = e.Cell.Presenter.Content as CheckBox;
                    if (chkBox == null)
                        return;

                    if (string.IsNullOrEmpty(Util.NVC(DataTableConverter.GetValue(dgUser.Rows[e.Cell.Row.Index].DataItem, dgUser.Columns[e.Cell.Column.Index + 2].Name))))
                    {
                        if (chkBox.Visibility == Visibility.Visible)
                            chkBox.Visibility = Visibility.Collapsed;
                    }
                    else
                    {
                        if (chkBox.Visibility == Visibility.Collapsed)
                            chkBox.Visibility = Visibility.Visible;
                    }
                //}
            }));
        }

        private void dgGrList_LoadedCellPresenter(object sender, DataGridCellEventArgs e)
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

                C1.WPF.DataGrid.C1DataGrid dgGrList = sender as C1.WPF.DataGrid.C1DataGrid;
                if (e.Cell.Column.Index % 3 == 0)
                {
                    RadioButton RadioButton = e.Cell.Presenter.Content as RadioButton;
                    if (RadioButton == null)
                        return;

                    if (string.IsNullOrEmpty(Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, dataGrid.Columns[e.Cell.Column.Index + 2].Name))))
                    {
                        if (RadioButton.Visibility == Visibility.Visible)
                            RadioButton.Visibility = Visibility.Collapsed;
                    }
                    else
                    {
                        if (RadioButton.Visibility == Visibility.Collapsed)
                            RadioButton.Visibility = Visibility.Visible;
                    }
                }
            }));
        }

        private void dgGrListChoice_Checked(object sender, RoutedEventArgs e)
        {
            if (sender == null)
                return;

            RadioButton rb = sender as RadioButton;

            if (rb.DataContext == null)
                return;

            if ((bool)rb.IsChecked)
            {
                int idx = ((C1.WPF.DataGrid.DataGridCellPresenter)rb.Parent).Row.Index;
                DataRow dtRow = (rb.DataContext as DataRowView).Row;
                int idx2 = 1;
                if (((C1.WPF.DataGrid.DataGridCellPresenter)rb.Parent).Column.Index == 3)
                    idx2 = 2;
                if (((C1.WPF.DataGrid.DataGridCellPresenter)rb.Parent).Column.Index == 6)
                    idx2 = 3;

                for (int i = 0; i < ((C1.WPF.DataGrid.DataGridCellPresenter)rb.Parent).DataGrid.Rows.Count; i++)
                {
                    if (idx == i)
                    {
                        if (DataTableConverter.GetValue(((C1.WPF.DataGrid.DataGridCellPresenter)rb.Parent).DataGrid.Rows[i].DataItem, "WRK_GR_NAME_" + idx2) == null) continue; // 2024.11.14. 김영국 - Load시 Null값에 대한 처리를 하도록 수정함.
                        if (!DataTableConverter.GetValue(((C1.WPF.DataGrid.DataGridCellPresenter)rb.Parent).DataGrid.Rows[i].DataItem, "WRK_GR_NAME_" + idx2).Equals("") || DataTableConverter.GetValue(((C1.WPF.DataGrid.DataGridCellPresenter)rb.Parent).DataGrid.Rows[i].DataItem, "WRK_GR_NAME_" + idx2).Equals(ObjectDic.Instance.GetObjectName("ALL")))
                        {
                            DataTableConverter.SetValue(((C1.WPF.DataGrid.DataGridCellPresenter)rb.Parent).DataGrid.Rows[i].DataItem, "CHK_" + idx2, true);
                            GetShiftUser((rb.DataContext as DataRowView).Row["WRK_GR_ID_" + idx2].ToString());
                        }
                        else
                        {
                            DataTableConverter.SetValue(((C1.WPF.DataGrid.DataGridCellPresenter)rb.Parent).DataGrid.Rows[i].DataItem, "CHK_" + idx2, false);
                        }
                    }
                    else
                    {
                        DataTableConverter.SetValue(((C1.WPF.DataGrid.DataGridCellPresenter)rb.Parent).DataGrid.Rows[i].DataItem, "CHK_" + idx2, false);
                    }
                }
                dgGrList.SelectedIndex = idx;
            }
        }

        private void SetSelUserName(object sUser,int col, bool bRemove = false)
        {
            try
            {
                if (!bRemove)
                {

                    for (int i = dgWorkUser.GetRowCount() - 1; i >= 0; i--)
                    {
                        DataTable dt = DataTableConverter.Convert(dgWorkUser.ItemsSource);

                        
                        if (dt.Rows[i].ItemArray[1].Equals(DataTableConverter.GetValue(sUser, "USERID_" + col).ToString()))
                        {
                            Util.MessageValidation("SFU2051", dt.Rows[i].ItemArray[0] ); //중복 데이터가 존재 합니다..
                            return;
                        }
                    }

                    if (!DataTableConverter.GetValue(sUser, "USERID_" + col).ToString().Equals(""))
                    {
                        DataTable dt7 = new DataTable();

                        if (dgWorkUser.Rows.Count >= 1)
                        {
                            dt7 = ((DataView)dgWorkUser.ItemsSource).Table;
                        }
                        else
                        {
                            dt7.TableName = "dgWorkUser";
                            dt7.Columns.Add("USERNAME", typeof(String));
                            dt7.Columns.Add("USERID", typeof(String));
                            dt7.Columns.Add("DEPTNAME", typeof(String));
                        }
                        DataRow dr = dt7.NewRow();
                        dr["USERNAME"] = DataTableConverter.GetValue(sUser, "USERNAME_" + col).GetString();
                        dr["USERID"] = DataTableConverter.GetValue(sUser, "USERID_" + col).GetString();
                        dr["DEPTNAME"] = DataTableConverter.GetValue(sUser, "DEPTNAME_" + col).GetString();

                        dt7.Rows.Add(dr);
                        Util.GridSetData(dgWorkUser, dt7, null, false);
                    }
                }
                else
                {
                    for (int i = dgWorkUser.GetRowCount()-1; i >= 0; i--)
                    {
                        DataTable dt = DataTableConverter.Convert(dgWorkUser.ItemsSource);
                        if (dt.Rows[i].ItemArray[1].Equals(DataTableConverter.GetValue(sUser, "USERID_" + col).ToString()))
                        {
                            dt.Rows[i].Delete();
                        }
                        dgWorkUser.ItemsSource = DataTableConverter.Convert(dt);
                        dgWorkUser.ColumnWidth = new C1.WPF.DataGrid.DataGridLength(1, DataGridUnitType.Star);
                    }
                }
            }
            catch(Exception ex)
            {
                Util.MessageException(ex);
            }
        }


        //C20220307-000114 - 기능추가 작업자 <전체선택>/<전체해제>
        private void dgShiftUserSelectAllOrNothing_Checked(object sender, RoutedEventArgs e)
        {
            if (sender == null)
            {
                return;
            }
                
            if (dgShiftUser.ItemsSource == null)
            {
                return;
            }

            //1. DELETE EXISTS DATA
            for (int i = dgWorkUser.GetRowCount() - 1; i >= 0; i--)
            {
                DataTable dt = DataTableConverter.Convert(dgWorkUser.ItemsSource);
                dt.Rows[i].Delete();
                dgWorkUser.ItemsSource = DataTableConverter.Convert(dt);
                dgWorkUser.ColumnWidth = new C1.WPF.DataGrid.DataGridLength(1, DataGridUnitType.Star);
            }

            RadioButton rb = sender as RadioButton;

            if ((bool)rb.IsChecked)
            {
                if (rb.Name.Equals("rdbSelectALL"))
                {
                    //Util.MessageInfo(rb.Name);

                    //1. DELETE EXISTS DATA

                    //2. ADD ALL DATA
                    for (int i = 0; i < dgShiftUser.Rows.Count; i++)
                    {
                        for (int k = 1; k <= 5; k++)
                        {
                            if (DataTableConverter.GetValue(dgShiftUser.Rows[i].DataItem, "USERID_" + k) == null) return; // 2024.11.14. 김선영 - 전체 선택 시 작업자명이 없는 것들은 Pass 한다.

                            DataTableConverter.SetValue(dgShiftUser.Rows[i].DataItem, "CHK_" + k, true);
                            dgShiftUser.ScrollIntoView(i, dgShiftUser.Columns["CHK_" + k].Index);
                            SetSelUserName(dgShiftUser.Rows[i].DataItem, k, false);
                        }
                    }
                }
                else
                {
                    //Util.MessageInfo(rb.Name);

                    //1. DELETE EXISTS DATA

                    //2. DESLECT ALL DATA
                    for (int i = 0; i < dgShiftUser.Rows.Count; i++)
                    {
                        for (int k = 1; k <= 5; k++)
                        {
                            DataTableConverter.SetValue(dgShiftUser.Rows[i].DataItem, "CHK_" + k, false);
                            dgShiftUser.ScrollIntoView(i, dgShiftUser.Columns["CHK_" + k].Index);
                        }
                    }
                }

                dgShiftUser.Refresh(true);
            }
        }

    }
}