/*************************************************************************************
 Created Date : 2023.09.21
      Creator : 백광영
   Decription : Cell 공급조건 설정
--------------------------------------------------------------------------------------
 [Change History]

**************************************************************************************/

using System;
using System.Data;
using System.Windows;
using C1.WPF;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.CMM001.Extensions;
using LGC.GMES.MES.Common;
using C1.WPF.DataGrid;
using LGC.GMES.MES.ControlsLibrary;
using System.Linq;
using System.Windows.Media;
using System.Windows.Controls;
using System.Collections.Generic;
using System.Windows.Input;
using System.Text.RegularExpressions;
using LGC.GMES.MES.PACK001.Class;

namespace LGC.GMES.MES.PACK001
{
    public partial class PACK001_078_AUTO_COND : C1Window, IWorkArea
    {
        #region Declaration & Constructor 
        LGC.GMES.MES.PACK001.Class.MESSAGE_PARAM ms = new LGC.GMES.MES.PACK001.Class.MESSAGE_PARAM();
        SolidColorBrush redBrush = new SolidColorBrush(Colors.Red);

        string sToday = string.Empty;
        string sAreaID = string.Empty;
        string sMtrlID = string.Empty;

        private Util _Util = new Util();

        public IFrameOperation FrameOperation
        {
            get;
            set;
        }


        public PACK001_078_AUTO_COND()
        {
            InitializeComponent();
            
        }

        #endregion

        #region Initialize
        private void C1Window_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                #region 권한별 보여주기
                List<Button> listAuth = new List<Button>();
                //listAuth.Add(btnReq);
                //listAuth.Add(btnReqClose);
                //listAuth.Add(btnReqCancel);
                Util.pageAuth(listAuth, FrameOperation.AUTHORITY);
                #endregion

                object[] tmps = C1WindowExtension.GetParameters(this);
                if (tmps != null)
                {
                    sToday = Util.NVC(tmps[0]);
                    sAreaID = Util.NVC(tmps[1]);
                    sMtrlID = Util.NVC(tmps[2]);


                    getCond(sToday, sAreaID, sMtrlID);

                    //this.Loaded -= new System.Windows.RoutedEventHandler(this.C1Window_Loaded);
                }

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        #endregion

        #region Event

        #region Button

        #region 요청버튼
        private void btnTrfSave_Click(object sender, RoutedEventArgs e)
        {
            try
            {


                // 저장하시겠습니까?
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("SFU1241"), null, "Information", MessageBoxButton.OKCancel, MessageBoxIcon.None, (result) =>
                {
                    if (result.ToString().Equals("OK"))
                    {
                        ShowLoadingIndicator();
                        saveTrf();
                        Refresh();

                        getCond(sToday, sAreaID, sMtrlID);
                        HiddenLoadingIndicator();
                    }
                }
                );
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        #endregion

        #region 마감클릭
        private void btnBlkSave_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (dgBlockCond.GetRowCount() <= 0)
                {
                    return;
                }

                string _msgid = string.Empty;
                DataTable dt = ((DataView)dgBlockCond.ItemsSource).Table;

                var query = dt.AsEnumerable().Where(x => x.Field<Boolean>("FROM_EQSGID_CHK").Equals(true));
                if (query.Count() <= 0)
                {
                    _msgid = "SFU1241";  // 저장하시겠습니까?
                }
                else
                {
                    _msgid = "SFU9050";  // 체크한 라인으로부터 셀을 자동 공급받지 않습니다. 그래도 진행하시겠습니까?
                }

                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(_msgid), null, "Information", MessageBoxButton.OKCancel, MessageBoxIcon.None, (result) =>
                {
                    if (result.ToString().Equals("OK"))
                    {
                        ShowLoadingIndicator();
                        saveBlock();
                        Refresh();

                        getCond(sToday, sAreaID, sMtrlID);
                        HiddenLoadingIndicator();
                    }
                }
                );
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        #endregion

        #region 닫기
        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                this.DialogResult = MessageBoxResult.Cancel;
                this.Close();
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);

                //HiddenLoadingIndicator();
            }
        }
        #endregion

        #endregion

        #region 로딩 인디케이터 보여주기
        private void ShowLoadingIndicator()
        {
            if (loadingIndicator != null)
            {
                loadingIndicator.Visibility = Visibility.Visible;
            }
        }
        #endregion

        #region 로딩 인디케이터 숨기기
        private void HiddenLoadingIndicator()
        {
            if (loadingIndicator != null)
            {
                loadingIndicator.Visibility = Visibility.Collapsed;
            }
        }
        #endregion

        #endregion

        #region Mehod

        #region 공급조건설정 
        private void getCond(string Today, string AreaId, string MtrlId )
        {
            try
            {
                ShowLoadingIndicator();

                DataSet InData = new DataSet();
                DataTable IndataTable = InData.Tables.Add("INDATA");

                IndataTable.Columns.Add("LANGID", typeof(string));
                IndataTable.Columns.Add("TODAY", typeof(string));
                IndataTable.Columns.Add("SHOPID", typeof(string));
                IndataTable.Columns.Add("AREAID", typeof(string));
                IndataTable.Columns.Add("PRODID", typeof(string));

                DataRow dr = IndataTable.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["TODAY"] = sToday;
                dr["SHOPID"] = LoginInfo.CFG_SHOP_ID;
                dr["AREAID"] = sAreaID;
                dr["PRODID"] = sMtrlID;

                IndataTable.Rows.Add(dr);

                DataSet dsRslt = new ClientProxy().ExecuteServiceSync_Multi("BR_PRD_SEL_AUTO_TRF", "INDATA", "OUTDATA_REQ_BAS,OUTDATA_AUTO_TRF,OUTDATA_TRF_BLOCK", InData);

                if (dsRslt != null && dsRslt.Tables["OUTDATA_REQ_BAS"].Rows.Count > 0)
                {
                    decimal _safehour = 0;
                    if (decimal.TryParse(Util.NVC(dsRslt.Tables["OUTDATA_REQ_BAS"].Rows[0]["SAFE_STCK_BAS_HOUR"]), out _safehour))
                        txtSafeHour.Value = Convert.ToDouble(_safehour);
                    decimal _reqpallet = 0;
                    if (decimal.TryParse(Util.NVC(dsRslt.Tables["OUTDATA_REQ_BAS"].Rows[0]["AUTO_REQ_PLLT_QTY"]), out _reqpallet))
                        txtReqPallet.Value = Convert.ToDouble(_reqpallet);
                }
                if (dsRslt != null && dsRslt.Tables["OUTDATA_AUTO_TRF"].Rows.Count > 0)
                {
                    Util.GridSetData(dgAutoCond, dsRslt.Tables["OUTDATA_AUTO_TRF"], FrameOperation, false);
                }
                if (dsRslt != null && dsRslt.Tables["OUTDATA_TRF_BLOCK"].Rows.Count > 0)
                {
                    Util.GridSetData(dgBlockCond, dsRslt.Tables["OUTDATA_TRF_BLOCK"], FrameOperation, false);

                    //dgBlockCond.MergingCells -= dgBlockCond_MergingCells;
                    //dgBlockCond.MergingCells += dgBlockCond_MergingCells;
                }
            }
            catch (Exception ex)
            {
                HiddenLoadingIndicator();
                Util.MessageException(ex);
            }
            finally
            {
                HiddenLoadingIndicator();
            }
        }
        #endregion


        #region 자동공급설정저장
        private void saveTrf()
        {
            try
            {
                ShowLoadingIndicator();

                DataTable dt = ((DataView)dgAutoCond.ItemsSource).Table;
                
                DataSet ds = new DataSet();
                DataTable dtIndata = ds.Tables.Add("INDATA");
                dtIndata.Columns.Add("SHOPID", typeof(string));
                dtIndata.Columns.Add("AREAID", typeof(string));
                dtIndata.Columns.Add("PRODID", typeof(string));
                dtIndata.Columns.Add("USERID", typeof(string));

                DataRow dr = null;
                dr = dtIndata.NewRow();
                dr["SHOPID"] = LoginInfo.CFG_SHOP_ID;
                dr["AREAID"] = sAreaID;
                dr["PRODID"] = sMtrlID;
                dr["USERID"] = LoginInfo.USERID;
                dtIndata.Rows.Add(dr);

                DataTable dtReqBase = ds.Tables.Add("INDATA_REQ_BAS");
                dtReqBase.Columns.Add("SAFE_STCK_BAS_HOUR", typeof(decimal));
                dtReqBase.Columns.Add("AUTO_REQ_PLLT_QTY", typeof(decimal));

                DataRow dr1 = null;
                dr1 = dtReqBase.NewRow();
                dr1["SAFE_STCK_BAS_HOUR"] = Util.NVC_Decimal(txtSafeHour.Value);
                dr1["AUTO_REQ_PLLT_QTY"] = Util.NVC_Decimal(txtReqPallet.Value);
                dtReqBase.Rows.Add(dr1);

                DataTable dtAutoTrf = ds.Tables.Add("INDATA_AUTO_TRF");
                dtAutoTrf.Columns.Add("EQSGID", typeof(string));
                dtAutoTrf.Columns.Add("AUTO_TRF_REQ_FLAG", typeof(string));

                DataRow dr3 = null;
                var query = (from t in dt.AsEnumerable()
                             //where t.Field<Boolean>("CHK").Equals(true)
                             select new
                             {
                                 _eqsgID = t.Field<string>("EQSGID"),
                                 _flag = t.Field<Boolean>("CHK")
                             }).ToList();

                foreach (var x in query)
                {
                    dr3 = dtAutoTrf.NewRow();
                    dr3["EQSGID"] = x._eqsgID;
                    dr3["AUTO_TRF_REQ_FLAG"] = x._flag == true ? "Y" : "N";
                    dtAutoTrf.Rows.Add(dr3);
                }

                new ClientProxy().ExecuteServiceSync_Multi("BR_PRD_REG_AUTO_TRF", "INDATA,INDATA_REQ_BAS,INDATA_AUTO_TRF", null, ds);

                Util.MessageInfo("SFU1275");

            }
            catch (Exception ex)
            {
                HiddenLoadingIndicator();
                Util.MessageException(ex);
            }
            finally
            {
                HiddenLoadingIndicator();
            }
        }
        #endregion

        #region 공급중단설정저장
        private void saveBlock()
        {
            try
            {
                ShowLoadingIndicator();

                DataSet ds = new DataSet();
                DataTable dtIndata = ds.Tables.Add("INDATA");
                dtIndata.Columns.Add("SHOPID", typeof(string));
                dtIndata.Columns.Add("AREAID", typeof(string));
                dtIndata.Columns.Add("PRODID", typeof(string));
                dtIndata.Columns.Add("FROM_AREAID", typeof(string));
                dtIndata.Columns.Add("FROM_EQSGID", typeof(string));
                dtIndata.Columns.Add("BLOCK_TRF_FLAG", typeof(string));
                dtIndata.Columns.Add("USERID", typeof(string));

                DataRow dr = null;
                DataTable dt = ((DataView)dgBlockCond.ItemsSource).Table;

                var query = (from t in dt.AsEnumerable()
                             //where t.Field<Boolean>("FROM_EQSGID_CHK").Equals(true)
                             select new
                             {
                                 _fromareaID = t.Field<string>("FROM_AREAID"),
                                 _fromeqsgID = t.Field<string>("FROM_EQSGID"),
                                 _flag = t.Field<Boolean>("FROM_EQSGID_CHK")
                             }).ToList();

                foreach (var x in query)
                {
                    dr = dtIndata.NewRow();
                    dr["SHOPID"] = LoginInfo.CFG_SHOP_ID;
                    dr["AREAID"] = LoginInfo.CFG_AREA_ID;
                    dr["PRODID"] = sMtrlID;
                    dr["FROM_AREAID"] = x._fromareaID;
                    dr["FROM_EQSGID"] = x._fromeqsgID;
                    dr["BLOCK_TRF_FLAG"] = x._flag == true ? "Y" : "N";
                    dr["USERID"] = LoginInfo.USERID;
                    dtIndata.Rows.Add(dr);
                }

                new ClientProxy().ExecuteServiceSync_Multi("BR_PRD_REG_TRF_BLOCK", "INDATA", null, ds);

                Util.MessageInfo("SFU1275");

            }
            catch (Exception ex)
            {
                HiddenLoadingIndicator();
                Util.MessageException(ex);
            }
            finally
            {
                HiddenLoadingIndicator();
            }
        }
        #endregion

        #region Refresh
        private void Refresh()
        {
            try
            {
                //그리드 clear
                Util.gridClear(dgAutoCond);
                Util.gridClear(dgBlockCond);

            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        #endregion Refresh

        #endregion

        #region Grid
        private void dgAutoCond_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (dgAutoCond.CurrentRow == null || dgAutoCond.SelectedIndex == -1)
            {
                return;
            }

            try
            {
                if (e.ChangedButton.ToString().Equals("Left"))
                {
                    string _sColName = dgAutoCond.CurrentColumn.Name;
                    int _rowIndex = dgAutoCond.CurrentRow.Index;

                    DataTable dt = ((DataView)dgAutoCond.ItemsSource).Table;
                    
                    // 전체Check시 라인 전체 선택
                    if (_sColName == "ALLCHK")
                    {
                        if (dt.Rows.Count > 0 || dt != null)
                        {
                            bool _chk = (bool)dt.Rows[0]["ALLCHK"];
                            if (_chk)
                            {
                                PackCommon.GridCheckAllFlag(this.dgAutoCond, true, "CHK");
                            }
                            else
                            {
                                PackCommon.GridCheckAllFlag(this.dgAutoCond, false, "CHK");
                            }
                        }
                    }
                    // 라인선택 해제 시 전체선택 해제
                    if (_sColName == "CHK")
                    {
                        var query = dt.AsEnumerable().Where(x => x.Field<Boolean>("CHK").Equals(true));
                        if (dgAutoCond.GetRowCount() != query.Count())
                        {
                            PackCommon.GridCheckAllFlag(this.dgAutoCond, false, "ALLCHK");
                        }
                        else
                        {
                            PackCommon.GridCheckAllFlag(this.dgAutoCond, true, "ALLCHK");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
            finally
            {
                dgAutoCond.CurrentRow = null;
            }
        }

        private void dgBlockCond_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (dgBlockCond.CurrentRow == null || dgBlockCond.SelectedIndex == -1)
            {
                return;
            }

            try
            {
                if (e.ChangedButton.ToString().Equals("Left"))
                {
                    string _sColName = dgBlockCond.CurrentColumn.Name;
                    int _rowIndex = dgBlockCond.CurrentRow.Index;

                    // 동 선택 시 라인전체 선택
                    if (_sColName == "FROM_AREA_CHK")
                    {
                        string _fromAreaid = Util.NVC(dgBlockCond.GetCell(_rowIndex, dgBlockCond.Columns["FROM_AREAID"].Index).Value);
                        DataTable dt = ((DataView)dgBlockCond.ItemsSource).Table;
                        DataTable _dt = dt.Select("FROM_AREAID = '" + _fromAreaid + "'").CopyToDataTable();

                        bool _chk = (bool)_dt.Rows[0]["FROM_AREA_CHK"];
                        //var query = dt.AsEnumerable().Where(x => x.Field<string>("FROM_AREAID").ToUpper().Equals(_fromAreaid));
                        if (!_chk)
                        {
                            for (int i = 0; i < dgBlockCond.Rows.Count; i++)
                            {
                                if (_fromAreaid == Util.NVC(dgBlockCond.GetCell(i, dgBlockCond.Columns["FROM_AREAID"].Index).Value))
                                {
                                    DataTableConverter.SetValue(dgBlockCond.Rows[i].DataItem, "FROM_EQSGID_CHK", false);
                                }
                            }
                        }
                        else
                        {
                            for (int i = 0; i < dgBlockCond.Rows.Count; i++)
                            {
                                if (_fromAreaid == Util.NVC(dgBlockCond.GetCell(i, dgBlockCond.Columns["FROM_AREAID"].Index).Value))
                                {
                                    DataTableConverter.SetValue(dgBlockCond.Rows[i].DataItem, "FROM_EQSGID_CHK", true);
                                }
                            }
                        }
                    }
                    //라인 선택 해제시 동 선택 해제
                    if (_sColName == "FROM_EQSGID_CHK")
                    {
                        string _fromAreaid = Util.NVC(dgBlockCond.GetCell(_rowIndex, dgBlockCond.Columns["FROM_AREAID"].Index).Value);
                        DataTable dt = ((DataView)dgBlockCond.ItemsSource).Table;

                        var query_1 = dt.AsEnumerable().Where(x => x.Field<string>("FROM_AREAID").ToUpper().Equals(_fromAreaid));
                        var query_2 = dt.AsEnumerable().Where(x => x.Field<Boolean>("FROM_EQSGID_CHK").Equals(true) && x.Field<string>("FROM_AREAID").ToUpper().Equals(_fromAreaid));

                        if (query_1.Count() != query_2.Count())
                        {
                            for (int i = 0; i < dgBlockCond.Rows.Count; i++)
                            {
                                if (_fromAreaid == Util.NVC(dgBlockCond.GetCell(i, dgBlockCond.Columns["FROM_AREAID"].Index).Value))
                                {
                                    DataTableConverter.SetValue(dgBlockCond.Rows[i].DataItem, "FROM_AREA_CHK", false);
                                }
                            }
                        }
                        else
                        {
                            for (int i = 0; i < dgBlockCond.Rows.Count; i++)
                            {
                                if (_fromAreaid == Util.NVC(dgBlockCond.GetCell(i, dgBlockCond.Columns["FROM_AREAID"].Index).Value))
                                {
                                    DataTableConverter.SetValue(dgBlockCond.Rows[i].DataItem, "FROM_AREA_CHK", true);
                                }
                            }
                        }
                    }
                    dgBlockCond.EndEdit();
                    dgBlockCond.EndEditRow(true);
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
            finally
            {
                dgBlockCond.CurrentRow = null;
            }
        }

        private void dgBlockCond_MergingCells(object sender, DataGridMergingCellsEventArgs e)
        {
            C1DataGrid dg = sender as C1DataGrid;
            if (dg?.ItemsSource == null) return;

            try
            {
                if (dg.GetRowCount() > 0)
                {
                    DataTable dt = ((DataView)dg.ItemsSource).Table;
                    var query = dt.AsEnumerable().GroupBy(x => new
                    {
                        rkName = x.Field<string>("FROM_AREAID")
                    }).Select(g => new
                    {
                        GroupRkName = g.Key.rkName,
                        Count = g.Count()
                    }).ToList();

                    string GroupName = string.Empty;

                    for (int i = 0; i < dg.Rows.Count; i++)
                    {
                        foreach (var item in query)
                        {
                            int rowIndex = i;
                            string _areaid = DataTableConverter.GetValue(dg.Rows[i].DataItem, "FROM_AREAID").GetString();
                            if (_areaid == item.GroupRkName && GroupName != _areaid)
                            {
                                e.Merge(new DataGridCellsRange(dg.GetCell(i, dg.Columns["FROM_AREA_CHK"].Index), dg.GetCell(item.Count - 1 + rowIndex, dg.Columns["FROM_AREA_CHK"].Index)));
                                e.Merge(new DataGridCellsRange(dg.GetCell(i, dg.Columns["FROM_AREANAME"].Index), dg.GetCell(item.Count - 1 + rowIndex, dg.Columns["FROM_AREANAME"].Index)));
                            }
                        }
                        GroupName = DataTableConverter.GetValue(dg.Rows[i].DataItem, "FROM_AREAID").GetString();
                    }

                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        #endregion


    }
}
