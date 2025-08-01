﻿/***************************************************************
 Created Date : 2017.12.05
      Creator : 
   Decription : 전지 5MEGA-GMES 구축 - 작업조별 작업자 조회 및 라벨 발행 팝업
----------------------------------------------------------------
  2023.08.09  김도형 : [E20230715-001963] [Ultium Cells PI Team] GMES 전극 공정 진척 => 작업조별 작업자 조회의 Operator 이름 검색기능 향상 요청
****************************************************************/

using C1.WPF;
using C1.WPF.DataGrid;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.CMM001.Extensions;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using System;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace LGC.GMES.MES.CMM001.Popup
{

    public partial class CMM_BOX_SHFT : C1Window, IWorkArea
    {
        #region Declaration & Constructor
        private string _RetUserID = string.Empty;
        private string _RetUserName = string.Empty;
        private string _RetWrkGrID = string.Empty;
        private string _RetWrkGrName = string.Empty;
        private string _RetShftID = string.Empty;
        private string _RetShftName = string.Empty;
        private string _RetShftStrt = string.Empty;
        private string _RetShftEnd = string.Empty;

        private string _Shop = string.Empty;
        private string _Area = string.Empty;
        private string _Segment = string.Empty;
        private string _Proc = string.Empty;
        private string _Shift = string.Empty;
        private string _Worker = string.Empty;
        TextBox _target;

        // 프린트 설정용
        string _sPrt = string.Empty;
        string _sRes = string.Empty;
        string _sCopy = string.Empty;
        string _sXpos = string.Empty;
        string _sYpos = string.Empty;
        string _sDark = string.Empty;

        // Selected 값 선택 확인 변수
        private string _SelectedTrue = "True";

        DataRow _drPrtInfo = null;

        Util _Util = new Util();
        bool _bClose = true;

        public string USERID
        {
            get { return _RetUserID; }
        }

        public string USERNAME
        {
            get { return _RetUserName; }
        }
        public string SHFTID
        {
            get { return _RetShftID; }
        }

        public string SHFTNAME
        {
            get { return _RetShftName; }
        }
        public string SHFT_STRT_HMS
        {
            get { return _RetShftStrt; }
        }

        public string SHFT_END_HMS
        {
            get { return _RetShftEnd; }
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

        public CMM_BOX_SHFT()
        {
            InitializeComponent();
        }

        public CMM_BOX_SHFT(TextBox target)
        {
            InitializeComponent();
            _target = target;
        }
        #endregion

        #region Event
        private void C1Window_Loaded(object sender, RoutedEventArgs e)
        {
            object[] tmps = C1WindowExtension.GetParameters(this);

            if (tmps.Length >= 5)
            {
                _Shop = tmps[0].ToString();
                _Area = tmps[1].ToString();
                _Segment = tmps[2].ToString();
                _Proc = tmps[3].ToString();
                _Shift = tmps[4].ToString();
                if (tmps.Length > 5 && tmps[5] != null)
                {
                    _Worker = tmps[5].ToString();
                }
                else
                {
                    _Worker = string.Empty;
                }
            }
            else
            {
                _Shop = string.Empty;
                _Area = string.Empty;
                _Segment = string.Empty;
                _Proc = string.Empty;
                _Shift = string.Empty;
                _Worker = string.Empty;
            }

            GetWrkGr();
        }

        //private void btnSelect_Click(object sender, RoutedEventArgs e)
        //{
        //    int iSelCnt = 0;
        //    _RetUserID = string.Empty;
        //    _RetUserName = string.Empty;

        //    for (int i = 0; i < dgShiftUser.Rows.Count; i++)
        //    {
        //        if (Util.NVC(dgShiftUser.GetCell(i, dgShiftUser.Columns["CHK_1"].Index).Value) == "1")
        //        {
        //            iSelCnt = iSelCnt + 1;

        //            if (iSelCnt == 1)
        //            {
        //                _RetUserID = Util.NVC(dgShiftUser.GetCell(i, dgShiftUser.Columns["USERID_1"].Index).Value);
        //                _RetUserName = Util.NVC(dgShiftUser.GetCell(i, dgShiftUser.Columns["USERNAME_1"].Index).Value);
        //            }
        //            else
        //            {
        //                _RetUserID = _RetUserID + "," + Util.NVC(dgShiftUser.GetCell(i, dgShiftUser.Columns["USERID_1"].Index).Value);
        //                _RetUserName = _RetUserName + "," + Util.NVC(dgShiftUser.GetCell(i, dgShiftUser.Columns["USERNAME_1"].Index).Value);
        //            }
        //        }

        //        if (Util.NVC(dgShiftUser.GetCell(i, dgShiftUser.Columns["CHK_2"].Index).Value) == "1")
        //        {
        //            iSelCnt = iSelCnt + 1;

        //            if (iSelCnt == 1)
        //            {
        //                _RetUserID = Util.NVC(dgShiftUser.GetCell(i, dgShiftUser.Columns["USERID_2"].Index).Value);
        //                _RetUserName = Util.NVC(dgShiftUser.GetCell(i, dgShiftUser.Columns["USERNAME_2"].Index).Value);
        //            }
        //            else
        //            {
        //                _RetUserID = _RetUserID + "," + Util.NVC(dgShiftUser.GetCell(i, dgShiftUser.Columns["USERID_2"].Index).Value);
        //                _RetUserName = _RetUserName + "," + Util.NVC(dgShiftUser.GetCell(i, dgShiftUser.Columns["USERNAME_2"].Index).Value);
        //            }
        //        }

        //        if (Util.NVC(dgShiftUser.GetCell(i, dgShiftUser.Columns["CHK_3"].Index).Value) == "1")
        //        {
        //            iSelCnt = iSelCnt + 1;

        //            if (iSelCnt == 1)
        //            {
        //                _RetUserID = Util.NVC(dgShiftUser.GetCell(i, dgShiftUser.Columns["USERID_3"].Index).Value);
        //                _RetUserName = Util.NVC(dgShiftUser.GetCell(i, dgShiftUser.Columns["USERNAME_3"].Index).Value);
        //            }
        //            else
        //            {
        //                _RetUserID = _RetUserID + "," + Util.NVC(dgShiftUser.GetCell(i, dgShiftUser.Columns["USERID_3"].Index).Value);
        //                _RetUserName = _RetUserName + "," + Util.NVC(dgShiftUser.GetCell(i, dgShiftUser.Columns["USERNAME_3"].Index).Value);
        //            }
        //        }
        //    }

        //    if (iSelCnt == 0)
        //    {
        //        //Util.MessageValidation("SFU1651");    //선택된 항목이 없습니다.
        //        Util.MessageValidation("SFU1651"); //선택된 항목이 없습니다.
        //        return;
        //    }

        //    this.DialogResult = MessageBoxResult.OK;
        //}

        private void btnSelect_Click(object sender, RoutedEventArgs e)
        {
            int iSelCnt = 0;
            _RetUserID = string.Empty;
            _RetUserName = string.Empty;
            _RetShftID = string.Empty;
            _RetShftName = string.Empty;

            for (int i = 0; i < dgShiftUser.Rows.Count; i++)
            {
                if (Util.NVC(dgShiftUser.GetCell(i, dgShiftUser.Columns["CHK_1"].Index).Value) == "1"
                   || Util.NVC(dgShiftUser.GetCell(i, dgShiftUser.Columns["CHK_1"].Index).Value) == _SelectedTrue)
                {
                    iSelCnt = iSelCnt + 1;

                    if (iSelCnt == 1)
                    {
                        _RetUserID = Util.NVC(dgShiftUser.GetCell(i, dgShiftUser.Columns["USERID_1"].Index).Value);
                        _RetUserName = Util.NVC(dgShiftUser.GetCell(i, dgShiftUser.Columns["USERNAME_1"].Index).Value);
                    }
                    else
                    {
                        _RetUserID = _RetUserID + "," + Util.NVC(dgShiftUser.GetCell(i, dgShiftUser.Columns["USERID_1"].Index).Value);
                        _RetUserName = _RetUserName + "," + Util.NVC(dgShiftUser.GetCell(i, dgShiftUser.Columns["USERNAME_1"].Index).Value);
                    }
                }

                if (Util.NVC(dgShiftUser.GetCell(i, dgShiftUser.Columns["CHK_2"].Index).Value) == "1"
                    || Util.NVC(dgShiftUser.GetCell(i, dgShiftUser.Columns["CHK_2"].Index).Value) == _SelectedTrue)
                {
                    iSelCnt = iSelCnt + 1;

                    if (iSelCnt == 1)
                    {
                        _RetUserID = Util.NVC(dgShiftUser.GetCell(i, dgShiftUser.Columns["USERID_2"].Index).Value);
                        _RetUserName = Util.NVC(dgShiftUser.GetCell(i, dgShiftUser.Columns["USERNAME_2"].Index).Value);
                    }
                    else
                    {
                        _RetUserID = _RetUserID + "," + Util.NVC(dgShiftUser.GetCell(i, dgShiftUser.Columns["USERID_2"].Index).Value);
                        _RetUserName = _RetUserName + "," + Util.NVC(dgShiftUser.GetCell(i, dgShiftUser.Columns["USERNAME_2"].Index).Value);
                    }
                }

                if (Util.NVC(dgShiftUser.GetCell(i, dgShiftUser.Columns["CHK_3"].Index).Value) == "1"
                    || Util.NVC(dgShiftUser.GetCell(i, dgShiftUser.Columns["CHK_3"].Index).Value) == _SelectedTrue)
                {
                    iSelCnt = iSelCnt + 1;

                    if (iSelCnt == 1)
                    {
                        _RetUserID = Util.NVC(dgShiftUser.GetCell(i, dgShiftUser.Columns["USERID_3"].Index).Value);
                        _RetUserName = Util.NVC(dgShiftUser.GetCell(i, dgShiftUser.Columns["USERNAME_3"].Index).Value);
                    }
                    else
                    {
                        _RetUserID = _RetUserID + "," + Util.NVC(dgShiftUser.GetCell(i, dgShiftUser.Columns["USERID_3"].Index).Value);
                        _RetUserName = _RetUserName + "," + Util.NVC(dgShiftUser.GetCell(i, dgShiftUser.Columns["USERNAME_3"].Index).Value);
                    }
                }
            }

            if (iSelCnt == 0)
            {
                //Util.MessageValidation("SFU1651");    //선택된 항목이 없습니다.
                Util.MessageValidation("SFU1651"); //선택된 항목이 없습니다.
                if(_bClose)
                    return;
            }
            if (_bClose)
            {
                SelectShft();


            }
        }

        //private void SelectShft()
        //{
        //    try
        //    {
        //        DataTable dt = new DataTable();
        //        dt.Columns.Add("SHOPID");
        //        dt.Columns.Add("AREAID");
        //        dt.Columns.Add("EQSGID");
        //        dt.Columns.Add("PROCID");
        //        dt.Columns.Add("LANGID");
        //        DataRow dr = dt.NewRow();
        //        dr["SHOPID"] = LoginInfo.CFG_SHOP_ID;
        //        dr["AREAID"] = LoginInfo.CFG_AREA_ID;
        //        dr["EQSGID"] = LoginInfo.CFG_EQSG_ID;
        //        dr["PROCID"] = Process.CELL_BOXING;
        //        dr["LANGID"] = LoginInfo.LANGID;
        //        dt.Rows.Add(dr);
        //        new ClientProxy().ExecuteService("DA_BAS_SEL_TB_MMD_SHFT_BY_CUR_TIME", "RQSTDT", "RSLTDT", dt, (result, ex) =>
        //        {
        //            if (ex != null)
        //            {
        //                Util.MessageException(ex);
        //                return;
        //            }
        //            if (result == null || result.Rows.Count < 1)
        //            {
        //                Util.MessageValidation("SFU1646");
        //                return;
        //            }
        //            _RetShftID = result.Rows[0]["SHFT_ID"].ToString();
        //            _RetShftName = result.Rows[0]["SHFT_NAME"].ToString();
        //            this.DialogResult = MessageBoxResult.OK;
        //        });
        //    }
        //    catch (Exception ex)
        //    {

        //        Util.MessageException(ex);
        //    }
        //}
        private void SelectShft()
        {
            try
            {
                DataSet ds = new DataSet();
                DataTable dt = ds.Tables.Add("INDATA");
                dt.Columns.Add("SHOPID");
                dt.Columns.Add("AREAID");
                dt.Columns.Add("EQSGID");
                dt.Columns.Add("PROCID");
                dt.Columns.Add("LANGID");
                DataRow dr = dt.NewRow();
                dr["SHOPID"] = LoginInfo.CFG_SHOP_ID;
                dr["AREAID"] = LoginInfo.CFG_AREA_ID;
                dr["EQSGID"] = LoginInfo.CFG_EQSG_ID;
                dr["PROCID"] = Process.CELL_BOXING;
                dr["LANGID"] = LoginInfo.LANGID;
                dt.Rows.Add(dr);
                new ClientProxy().ExecuteService_Multi("BR_BAS_SEL_TB_MMD_SHFT_BY_CUR_TIME", "INDATA", "OUTDATA", (result, ex) =>
                {
                    if (ex != null)
                    {
                        Util.MessageException(ex);
                        return;
                    }
                    if (result == null || result.Tables.Count < 1 || result.Tables["OUTDATA"].Rows.Count < 1)
                    {
                        Util.MessageValidation("SFU1646");
                        return;
                    }
                    _RetShftID = result.Tables["OUTDATA"].Rows[0]["SHFT_ID"].ToString();
                    _RetShftName = result.Tables["OUTDATA"].Rows[0]["SHFT_NAME"].ToString();
                    _RetShftStrt = result.Tables["OUTDATA"].Rows[0]["SHFT_STRT_HMS"].ToString();
                    _RetShftEnd = result.Tables["OUTDATA"].Rows[0]["SHFT_END_HMS"].ToString();
                    this.DialogResult = MessageBoxResult.OK;
                }, ds);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
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
                            if (Util.NVC(dgShiftUser.GetCell(i, dgShiftUser.Columns["USERID_1"].Index).Value).Equals(_Worker.Split(',')[j].Trim()))
                            {
                                DataTableConverter.SetValue(dgShiftUser.Rows[i].DataItem, "CHK_1", true);
                                dgShiftUser.ScrollIntoView(i, dgShiftUser.Columns["CHK_1"].Index);

                                SetSelUserName(Util.NVC(DataTableConverter.GetValue(dgShiftUser.Rows[i].DataItem, "USERNAME_1")));
                            }
                            else if (Util.NVC(dgShiftUser.GetCell(i, dgShiftUser.Columns["USERID_2"].Index).Value).Equals(_Worker.Split(',')[j].Trim()))
                            {
                                DataTableConverter.SetValue(dgShiftUser.Rows[i].DataItem, "CHK_2", true);
                                dgShiftUser.ScrollIntoView(i, dgShiftUser.Columns["CHK_2"].Index);

                                SetSelUserName(Util.NVC(DataTableConverter.GetValue(dgShiftUser.Rows[i].DataItem, "USERNAME_2")));
                            }
                            else if (Util.NVC(dgShiftUser.GetCell(i, dgShiftUser.Columns["USERID_3"].Index).Value).Equals(_Worker.Split(',')[j].Trim()))
                            {
                                DataTableConverter.SetValue(dgShiftUser.Rows[i].DataItem, "CHK_3", true);
                                dgShiftUser.ScrollIntoView(i, dgShiftUser.Columns["CHK_3"].Index);

                                SetSelUserName(Util.NVC(DataTableConverter.GetValue(dgShiftUser.Rows[i].DataItem, "USERNAME_3")));
                            }
                        }
                    }
                }
            }
        }

        private void dgShiftUser_MouseDoubleClick(object sender, MouseButtonEventArgs e) //바로 발행되도록 수정한다.
        {
            int iSelCnt = 0;
            _RetUserID = string.Empty;
            _RetUserName = string.Empty;
            _RetShftID = string.Empty;
            _RetShftName = string.Empty;


            for (int i = 0; i < dgShiftUser.Rows.Count; i++)
            {
                if (Util.NVC(dgShiftUser.GetCell(i, dgShiftUser.Columns["CHK_1"].Index).Value) == "1"
                    || Util.NVC(dgShiftUser.GetCell(i, dgShiftUser.Columns["CHK_1"].Index).Value) == "True")
                {
                    if (string.IsNullOrEmpty(Util.NVC(dgShiftUser.GetCell(i, dgShiftUser.Columns["USERID_1"].Index).Value)))
                        return;

                    iSelCnt = iSelCnt + 1;

                    if (iSelCnt == 1)
                    {
                        _RetUserID = Util.NVC(dgShiftUser.GetCell(i, dgShiftUser.Columns["USERID_1"].Index).Value);
                        _RetUserName = Util.NVC(dgShiftUser.GetCell(i, dgShiftUser.Columns["USERNAME_1"].Index).Value);
                    }
                    else
                    {
                        _RetUserID = _RetUserID + "," + Util.NVC(dgShiftUser.GetCell(i, dgShiftUser.Columns["USERID_1"].Index).Value);
                        _RetUserName = _RetUserName + "," + Util.NVC(dgShiftUser.GetCell(i, dgShiftUser.Columns["USERNAME_1"].Index).Value);
                    }

                    SetSelUserName(Util.NVC(dgShiftUser.GetCell(i, dgShiftUser.Columns["USERNAME_1"].Index).Value));
                }

                if (Util.NVC(dgShiftUser.GetCell(i, dgShiftUser.Columns["CHK_2"].Index).Value) == "1")
                {
                    if (string.IsNullOrEmpty(Util.NVC(dgShiftUser.GetCell(i, dgShiftUser.Columns["USERID_2"].Index).Value)))
                        return;

                    iSelCnt = iSelCnt + 1;

                    if (iSelCnt == 1)
                    {
                        _RetUserID = Util.NVC(dgShiftUser.GetCell(i, dgShiftUser.Columns["USERID_2"].Index).Value);
                        _RetUserName = Util.NVC(dgShiftUser.GetCell(i, dgShiftUser.Columns["USERNAME_2"].Index).Value);
                    }
                    else
                    {
                        _RetUserID = _RetUserID + "," + Util.NVC(dgShiftUser.GetCell(i, dgShiftUser.Columns["USERID_2"].Index).Value);
                        _RetUserName = _RetUserName + "," + Util.NVC(dgShiftUser.GetCell(i, dgShiftUser.Columns["USERNAME_2"].Index).Value);
                    }

                    SetSelUserName(Util.NVC(dgShiftUser.GetCell(i, dgShiftUser.Columns["USERNAME_2"].Index).Value));
                }

                if (Util.NVC(dgShiftUser.GetCell(i, dgShiftUser.Columns["CHK_3"].Index).Value) == "1")
                {
                    if (string.IsNullOrEmpty(Util.NVC(dgShiftUser.GetCell(i, dgShiftUser.Columns["USERID_3"].Index).Value)))
                        return;

                    iSelCnt = iSelCnt + 1;

                    if (iSelCnt == 1)
                    {
                        _RetUserID = Util.NVC(dgShiftUser.GetCell(i, dgShiftUser.Columns["USERID_3"].Index).Value);
                        _RetUserName = Util.NVC(dgShiftUser.GetCell(i, dgShiftUser.Columns["USERNAME_3"].Index).Value);
                    }
                    else
                    {
                        _RetUserID = _RetUserID + "," + Util.NVC(dgShiftUser.GetCell(i, dgShiftUser.Columns["USERID_3"].Index).Value);
                        _RetUserName = _RetUserName + "," + Util.NVC(dgShiftUser.GetCell(i, dgShiftUser.Columns["USERNAME_3"].Index).Value);
                    }

                    SetSelUserName(Util.NVC(dgShiftUser.GetCell(i, dgShiftUser.Columns["USERNAME_3"].Index).Value));
                }
            }

            if (iSelCnt == 0)
            {
                Util.MessageValidation("SFU1651");    //선택된 항목이 없습니다.
                return;
            }
            SelectShft();
            //this.DialogResult = MessageBoxResult.OK;
        }

        #endregion

        #region Mehod

        #region [BizCall]
        private void GetShiftUser(string wrkGrID)
        {

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

            new ClientProxy().ExecuteService("DA_BAS_SEL_TB_MMD_AREA_PROC_USER", "INDATA", "OUTDATA", searchConditionTable, (searchResult, searchException) =>
            {
                try
                {
                    if (searchException != null)
                    {
                        Util.AlertByBiz("DA_BAS_SEL_TB_MMD_AREA_PROC_USER", searchException.Message, searchException.ToString());
                        return;
                    }

                    dgShiftUser.ItemsSource = DataTableConverter.Convert(searchResult);
                    dgShiftUser.ColumnWidth = new C1.WPF.DataGrid.DataGridLength(1, DataGridUnitType.Star);
                    dgWorker_Checked();
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
                        GetShiftUser(Util.NVC(result.Rows[0]["WRK_GR_ID_1"]));
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
        #endregion
        private bool PrintLabel(string zpl, DataRow drPrtInfo)
        {
            if (drPrtInfo?.Table == null)
            {
                loadingIndicator.Visibility = Visibility.Collapsed;
                FrameOperation.PrintFrameMessage(MessageDic.Instance.GetMessage("SFU3030"));

                return false;
            }

            bool brtndefault = false;
            if (drPrtInfo.Table.Columns.Contains("PORTNAME") && drPrtInfo["PORTNAME"].ToString().Trim().Length > 0)
            {
                if (drPrtInfo["PORTNAME"].GetString().ToUpper().Equals("USB"))
                {
                    brtndefault = FrameOperation.Barcode_ZPL_USB_Print(zpl);
                    if (brtndefault == false)
                    {
                        loadingIndicator.Visibility = Visibility.Collapsed;
                        FrameOperation.PrintFrameMessage(MessageDic.Instance.GetMessage("SFU1309"));
                    }
                }
                else if (drPrtInfo["PORTNAME"].ToString().IndexOf("LPT", StringComparison.Ordinal) >= 0)
                {
                    FrameOperation.PrintFrameMessage(string.Empty);
                    brtndefault = FrameOperation.Barcode_ZPL_LPT_Print(drPrtInfo, zpl);
                    if (brtndefault == false)
                    {
                        loadingIndicator.Visibility = Visibility.Collapsed;
                        FrameOperation.PrintFrameMessage(MessageDic.Instance.GetMessage("SFU1309"));
                    }
                }
                else
                {
                    FrameOperation.PrintFrameMessage(string.Empty);
                    brtndefault = FrameOperation.Barcode_ZPL_Print(drPrtInfo, zpl);
                    if (brtndefault == false)
                    {
                        loadingIndicator.Visibility = Visibility.Collapsed;
                        FrameOperation.PrintFrameMessage(MessageDic.Instance.GetMessage("SFU1309"));
                    }
                }

                //System.Threading.Thread.Sleep(200);
            }
            else
            {
                loadingIndicator.Visibility = Visibility.Collapsed;
                FrameOperation.PrintFrameMessage(MessageDic.Instance.GetMessage("SFU3031"));
                Util.MessageValidation("SFU3031");
            }

            return brtndefault;
        }

        #endregion

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

            if (dg.CurrentCell.Column.Index == dgShiftUser.Columns["USERNAME_1"].Index)
            {
                if (DataTableConverter.GetValue(dg.Rows[idx].DataItem, "CHK_1").ToString().Equals("1")
                    || DataTableConverter.GetValue(dg.Rows[idx].DataItem, "CHK_1").ToString().Equals(_SelectedTrue))
                {
                    DataTableConverter.SetValue(dg.Rows[idx].DataItem, "CHK_1", false);

                    SetSelUserName(Util.NVC(DataTableConverter.GetValue(dg.Rows[idx].DataItem, "USERNAME_1")), true);
                }
                else
                {
                    DataTableConverter.SetValue(dg.Rows[idx].DataItem, "CHK_1", true);

                    SetSelUserName(Util.NVC(DataTableConverter.GetValue(dg.Rows[idx].DataItem, "USERNAME_1")));
                }
            }
            else if (dg.CurrentCell.Column.Index == dgShiftUser.Columns["USERNAME_2"].Index)
            {
                if (DataTableConverter.GetValue(dg.Rows[idx].DataItem, "CHK_2").ToString().Equals("1")
                    || DataTableConverter.GetValue(dg.Rows[idx].DataItem, "CHK_2").ToString().Equals(_SelectedTrue))
                {
                    DataTableConverter.SetValue(dg.Rows[idx].DataItem, "CHK_2", false);

                    SetSelUserName(Util.NVC(DataTableConverter.GetValue(dg.Rows[idx].DataItem, "USERNAME_2")), true);
                }
                else
                {
                    DataTableConverter.SetValue(dg.Rows[idx].DataItem, "CHK_2", true);

                    SetSelUserName(Util.NVC(DataTableConverter.GetValue(dg.Rows[idx].DataItem, "USERNAME_2")));
                }
            }
            else if (dg.CurrentCell.Column.Index == dgShiftUser.Columns["USERNAME_3"].Index)
            {
                if (DataTableConverter.GetValue(dg.Rows[idx].DataItem, "CHK_3").ToString().Equals("1")
                    || DataTableConverter.GetValue(dg.Rows[idx].DataItem, "CHK_3").ToString().Equals(_SelectedTrue))
                {
                    DataTableConverter.SetValue(dg.Rows[idx].DataItem, "CHK_3", false);

                    SetSelUserName(Util.NVC(DataTableConverter.GetValue(dg.Rows[idx].DataItem, "USERNAME_3")), true);
                }
                else
                {
                    DataTableConverter.SetValue(dg.Rows[idx].DataItem, "CHK_3", true);

                    SetSelUserName(Util.NVC(DataTableConverter.GetValue(dg.Rows[idx].DataItem, "USERNAME_3")));
                }
            }
            else if (dg.CurrentCell.Column.Index == dgShiftUser.Columns["CHK_1"].Index)
            {
                if (DataTableConverter.GetValue(dg.Rows[idx].DataItem, "CHK_1").ToString().Equals("1")
                    || DataTableConverter.GetValue(dg.Rows[idx].DataItem, "CHK_1").ToString().Equals(_SelectedTrue))
                {
                    SetSelUserName(Util.NVC(DataTableConverter.GetValue(dg.Rows[idx].DataItem, "USERNAME_1")));
                }
                else
                {
                    SetSelUserName(Util.NVC(DataTableConverter.GetValue(dg.Rows[idx].DataItem, "USERNAME_1")), true);
                }
            }
            else if (dg.CurrentCell.Column.Index == dgShiftUser.Columns["CHK_2"].Index)
            {
                if (DataTableConverter.GetValue(dg.Rows[idx].DataItem, "CHK_2").ToString().Equals("1")
                    || DataTableConverter.GetValue(dg.Rows[idx].DataItem, "CHK_2").ToString().Equals(_SelectedTrue))
                {
                    SetSelUserName(Util.NVC(DataTableConverter.GetValue(dg.Rows[idx].DataItem, "USERNAME_2")));
                }
                else
                {
                    SetSelUserName(Util.NVC(DataTableConverter.GetValue(dg.Rows[idx].DataItem, "USERNAME_2")), true);
                }
            }
            else if (dg.CurrentCell.Column.Index == dgShiftUser.Columns["CHK_3"].Index)
            {
                if (DataTableConverter.GetValue(dg.Rows[idx].DataItem, "CHK_3").ToString().Equals("1")
                    || DataTableConverter.GetValue(dg.Rows[idx].DataItem, "CHK_3").ToString().Equals(_SelectedTrue))
                {
                    SetSelUserName(Util.NVC(DataTableConverter.GetValue(dg.Rows[idx].DataItem, "USERNAME_3")));
                }
                else
                {
                    SetSelUserName(Util.NVC(DataTableConverter.GetValue(dg.Rows[idx].DataItem, "USERNAME_3")), true);
                }
            }

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
                if (idx == dgGrList.Columns["WRK_GR_NAME_1"].Index)
                {
                    DataTableConverter.SetValue(dg.Rows[i].DataItem, "CHK_1", true);
                }
                else
                {
                    DataTableConverter.SetValue(dg.Rows[i].DataItem, "CHK_1", false);
                }
                if (idx == dgGrList.Columns["WRK_GR_NAME_2"].Index)
                {
                    DataTableConverter.SetValue(dg.Rows[i].DataItem, "CHK_2", true);
                }
                else
                {
                    DataTableConverter.SetValue(dg.Rows[i].DataItem, "CHK_2", false);
                }
                if (idx == dgGrList.Columns["WRK_GR_NAME_3"].Index)
                {
                    DataTableConverter.SetValue(dg.Rows[i].DataItem, "CHK_3", true);
                }
                else
                {
                    DataTableConverter.SetValue(dg.Rows[i].DataItem, "CHK_3", false);
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
                // [E20230715-001963] [Ultium Cells PI Team] GMES 전극 공정 진척 => 작업조별 작업자 조회의 Operator 이름 검색기능 향상 요청
                if (Util.NVC(dgShiftUser.GetCell(i, dgShiftUser.Columns["USERNAME_1"].Index).Value.ToString().ToUpper()).Equals(txtUserName.Text.Trim().ToString().ToUpper()))
                {
                    DataTableConverter.SetValue(dgShiftUser.Rows[i].DataItem, "CHK_1", true);
                    dgShiftUser.ScrollIntoView(i, dgShiftUser.Columns["CHK_1"].Index);

                    SetSelUserName(Util.NVC(DataTableConverter.GetValue(dgShiftUser.Rows[i].DataItem, "USERNAME_1")));
                }
                else if (Util.NVC(dgShiftUser.GetCell(i, dgShiftUser.Columns["USERNAME_2"].Index).Value.ToString().ToUpper()).Equals(txtUserName.Text.Trim().ToString().ToUpper()))
                {
                    DataTableConverter.SetValue(dgShiftUser.Rows[i].DataItem, "CHK_2", true);
                    dgShiftUser.ScrollIntoView(i, dgShiftUser.Columns["CHK_2"].Index);

                    SetSelUserName(Util.NVC(DataTableConverter.GetValue(dgShiftUser.Rows[i].DataItem, "USERNAME_2")));
                }
                else if (Util.NVC(dgShiftUser.GetCell(i, dgShiftUser.Columns["USERNAME_3"].Index).Value.ToString().ToUpper()).Equals(txtUserName.Text.Trim().ToString().ToUpper()))
                {
                    DataTableConverter.SetValue(dgShiftUser.Rows[i].DataItem, "CHK_3", true);
                    dgShiftUser.ScrollIntoView(i, dgShiftUser.Columns["CHK_3"].Index);

                    SetSelUserName(Util.NVC(DataTableConverter.GetValue(dgShiftUser.Rows[i].DataItem, "USERNAME_3")));
                }
            }

            txtUserName.Text = "";
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
                    if (Util.NVC(dgShiftUser.GetCell(i, dgShiftUser.Columns["USERNAME_1"].Index).Value).Equals(txtUserName.Text.Trim()))
                    {
                        DataTableConverter.SetValue(dgShiftUser.Rows[i].DataItem, "CHK_1", true);
                        dgShiftUser.ScrollIntoView(i, dgShiftUser.Columns["CHK_1"].Index);

                        SetSelUserName(Util.NVC(DataTableConverter.GetValue(dgShiftUser.Rows[i].DataItem, "USERNAME_1")));
                    }
                    else if (Util.NVC(dgShiftUser.GetCell(i, dgShiftUser.Columns["USERNAME_2"].Index).Value).Equals(txtUserName.Text.Trim()))
                    {
                        DataTableConverter.SetValue(dgShiftUser.Rows[i].DataItem, "CHK_2", true);
                        dgShiftUser.ScrollIntoView(i, dgShiftUser.Columns["CHK_2"].Index);

                        SetSelUserName(Util.NVC(DataTableConverter.GetValue(dgShiftUser.Rows[i].DataItem, "USERNAME_2")));
                    }
                    else if (Util.NVC(dgShiftUser.GetCell(i, dgShiftUser.Columns["USERNAME_3"].Index).Value).Equals(txtUserName.Text.Trim()))
                    {
                        DataTableConverter.SetValue(dgShiftUser.Rows[i].DataItem, "CHK_3", true);
                        dgShiftUser.ScrollIntoView(i, dgShiftUser.Columns["CHK_3"].Index);

                        SetSelUserName(Util.NVC(DataTableConverter.GetValue(dgShiftUser.Rows[i].DataItem, "USERNAME_3")));
                    }
                }

                txtUserName.Text = "";
            }
        }

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
                if (e.Cell.Column.Index % 3 == 0)
                {
                    CheckBox chkBox = e.Cell.Presenter.Content as CheckBox;
                    if (chkBox == null)
                        return;

                    if (string.IsNullOrEmpty(Util.NVC(DataTableConverter.GetValue(dgUser.Rows[e.Cell.Row.Index].DataItem, dgUser.Columns[e.Cell.Column.Index + 1].Name))))
                    {
                        if (chkBox.Visibility == Visibility.Visible)
                            chkBox.Visibility = Visibility.Collapsed;
                    }
                    else
                    {
                        if (chkBox.Visibility == Visibility.Collapsed)
                            chkBox.Visibility = Visibility.Visible;
                    }
                }

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

                for (int i = 0; i < ((C1.WPF.DataGrid.DataGridCellPresenter)rb.Parent).DataGrid.Rows.Count; i++)
                {
                    if (idx == i)
                    {
                        DataTableConverter.SetValue(((C1.WPF.DataGrid.DataGridCellPresenter)rb.Parent).DataGrid.Rows[i].DataItem, "CHK_1", true);
                        GetShiftUser((rb.DataContext as DataRowView).Row["WRK_GR_ID_1"].ToString());
                    }
                    else
                    {
                        DataTableConverter.SetValue(((C1.WPF.DataGrid.DataGridCellPresenter)rb.Parent).DataGrid.Rows[i].DataItem, "CHK_1", false);
                    }
                }
                dgGrList.SelectedIndex = idx;
            }
        }

        private void dgGrListChoice_Checked_1(object sender, RoutedEventArgs e)
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

                for (int i = 0; i < ((C1.WPF.DataGrid.DataGridCellPresenter)rb.Parent).DataGrid.Rows.Count; i++)
                {
                    if (idx == i)
                    {
                        DataTableConverter.SetValue(((C1.WPF.DataGrid.DataGridCellPresenter)rb.Parent).DataGrid.Rows[i].DataItem, "CHK_2", true);
                        GetShiftUser((rb.DataContext as DataRowView).Row["WRK_GR_ID_2"].ToString());
                    }
                    else
                    {
                        DataTableConverter.SetValue(((C1.WPF.DataGrid.DataGridCellPresenter)rb.Parent).DataGrid.Rows[i].DataItem, "CHK_2", false);
                    }

                }

                dgGrList.SelectedIndex = idx;
            }
        }

        private void dgGrListChoice_Checked_2(object sender, RoutedEventArgs e)
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

                for (int i = 0; i < ((C1.WPF.DataGrid.DataGridCellPresenter)rb.Parent).DataGrid.Rows.Count; i++)
                {
                    if (idx == i)
                    {
                        DataTableConverter.SetValue(((C1.WPF.DataGrid.DataGridCellPresenter)rb.Parent).DataGrid.Rows[i].DataItem, "CHK_3", true);
                        GetShiftUser((rb.DataContext as DataRowView).Row["WRK_GR_ID_3"].ToString());
                    }
                    else
                    {
                        DataTableConverter.SetValue(((C1.WPF.DataGrid.DataGridCellPresenter)rb.Parent).DataGrid.Rows[i].DataItem, "CHK_3", false);
                    }

                }

                dgGrList.SelectedIndex = idx;
            }
        }

        private void C1Window_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                //sender.PlacementRectangle = new Rect(new Point(e.GetPosition(this).X,e.GetPosition(this).Y), new Point(200, 200));

            }
        }

        private void SetSelUserName(string sName, bool bRemove = false)
        {
            try
            {
                if (bRemove)
                {
                    if (txtSelUserName.Text.Trim().Equals(""))
                    {
                        txtSelUserName.Text = "";
                    }
                    else
                    {
                        string[] sTmpList = txtSelUserName.Text.Split(',');

                        txtSelUserName.Text = "";

                        for (int i = 0; i < sTmpList.Length; i++)
                        {
                            if (sName.Equals(sTmpList[i].Trim()))
                            {
                            }
                            else
                            {
                                if (txtSelUserName.Text.Trim().Equals(""))
                                {
                                    txtSelUserName.Text = sTmpList[i].Trim();
                                }
                                else
                                {
                                    txtSelUserName.Text = txtSelUserName.Text + ", " + sTmpList[i].Trim();
                                }
                            }
                        }
                    }
                }
                else
                {
                    if (txtSelUserName.Text.Trim().Equals(""))
                    {
                        txtSelUserName.Text = sName;
                    }
                    else
                    {
                        if (txtSelUserName.Text.IndexOf(sName) < 0)
                            txtSelUserName.Text = txtSelUserName.Text + ", " + sName;
                    }
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void btnPrint_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if(string.IsNullOrEmpty(txtSelUserName.Text))
                {
                    Util.MessageValidation("SFU1651");
                    return;
                }


                _bClose = false;
                btnSelect_Click(null, null);
                _bClose = true;
                if (!_Util.GetConfigPrintInfo(out _sPrt, out _sRes, out _sCopy, out _sXpos, out _sYpos, out _sDark, out _drPrtInfo))
                    return;
                DataSet ds = new DataSet();
                DataTable dtInData = ds.Tables.Add("INDATA");
                dtInData.Columns.Add("USERID");
                string[] arrID = _RetUserID.Split(',');
                DataRow newRow = null;
                for (int i = 0; i < arrID.Length; i++)
                {
                    newRow = dtInData.NewRow();
                    newRow["USERID"] = arrID[i];
                    dtInData.Rows.Add(newRow);
                }
                DataTable dtInPrint = ds.Tables.Add("INPRINT");
                dtInPrint.Columns.Add("PRMK");
                dtInPrint.Columns.Add("RESO");
                dtInPrint.Columns.Add("PRCN");
                dtInPrint.Columns.Add("MARH");
                dtInPrint.Columns.Add("MARV");
                dtInPrint.Columns.Add("DARK");
                newRow = dtInPrint.NewRow();
                newRow["PRMK"] = _sPrt;
                newRow["RESO"] = _sRes;
                newRow["PRCN"] = _sCopy;
                newRow["MARH"] = _sXpos;
                newRow["MARV"] = _sYpos;
                newRow["DARK"] = _sDark;
                dtInPrint.Rows.Add(newRow);
                new ClientProxy().ExecuteService_Multi("BR_PRD_GET_USERID_ZPL_NJ", "INDATA,INPRINT", "OUTDATA", (result, ex) =>
                {
                    if (ex != null)
                    {
                        Util.MessageException(ex);
                        return;
                    }
                    if (result != null && result.Tables["OUTDATA"] != null && result.Tables["OUTDATA"].Rows.Count > 0)
                    {
                        string zplCode = result.Tables["OUTDATA"].Rows[0]["ZPLCODE"].ToString();
                        PrintLabel(zplCode, _drPrtInfo);
                    }
                }, ds);
            }
            catch (Exception ex)
            {

                Util.MessageException(ex);
            }
        }

    }
}