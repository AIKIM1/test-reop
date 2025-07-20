/*************************************************************************************
 Created Date : 2024.07.04
      Creator : 양윤호
   Decription : 자동 라우트 변경 설정
   --------------------------------------------------------------------------------------
 [Change History]
  2024.07.04 양윤호 : Initial Created.
**************************************************************************************/

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
using System.Windows.Media;
using System.Linq;
using System.Collections.Generic;


namespace LGC.GMES.MES.FCS001
{
    /// <summary>
    /// Page2.xaml에 대한 상호 작용 논리 LoadedColumnHeaderPresenter="dgAutoChgRoutSet_LoadedColumnHeaderPresenter"  LoadedCellPresenter="dgAutoChgRoutSet_LoadedCellPresenter" 
    /// </summary>
    public partial class FCS001_172 : UserControl, IWorkArea
    {
        public IFrameOperation FrameOperation
        {
            get;
            set;
        }
        #region [Initialize]

        DataView _dvCMCD { get; set; }
        DataView _dvCMCD_AREA { get; set; }
        DataView _dvEQSG { get; set; }
        DataView _dvMODEL { get; set; }
        DataView _dvROUTE { get; set; }
        DataView _dvSHIFT { get; set; }
        DataView _dvCHGLINE { get; set; }



        private bool _manualCommit = false; //EndEditRow 호출시 재귀 이벤트 발생 회피용

        //Util _Util = new Util();
        public FCS001_172()
        {
            InitializeComponent();

            //this.Loaded += UserControl_Loaded;
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            //사용자 권한별로 버튼 숨기기
            //List<Button> listAuth = new List<Button>();

            InitCombo();
            //GetList();

            this.Loaded -= UserControl_Loaded;
        }

        private void UserControl_Unloaded(object sender, RoutedEventArgs e)
        {

        }

        private void InitCombo()
        {
            CommonCombo_Form ComCombo = new CommonCombo_Form();

            (dgAutoChgRoutSet.Columns["CHG_ROUT_DATE"] as DataGridDateTimeColumn).MinDate = DateTime.Now.AddDays(1);  //적용일시는 D+1 부터 설정가능

            #region USE_FLAG
            Get_MMD_SEL_CMCD_TYPE_MULT("'IUSE'", "Y", (dt, ex) =>
            {
                if (ex != null)
                {
                    ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), null, "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
                    return;
                }

                _dvCMCD = dt.DefaultView;

                CommonCombo.SetDtCommonCombo(_dvCMCD.ToTable(), cboUseFlag, CommonCombo.ComboStatus.ALL, "Y");
                CommonCombo.SetDtCommonCombo(_dvCMCD.ToTable(), dgAutoChgRoutSet.Columns["USE_FLAG"], CommonCombo.ComboStatus.NONE);
            });
            #endregion USE_FLAG

            #region LINE_ID
            Get_FORM_EQSG_CBO((dt, ex) =>
            {
                if (ex != null)
                {
                    ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), null, "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
                    return;
                }

                _dvEQSG = dt.DefaultView;

                CommonCombo.SetDtCommonCombo(_dvEQSG.ToTable(), dgAutoChgRoutSet.Columns["LINE_ID"], CommonCombo.ComboStatus.SELECT);
            });
            #endregion LINE_ID

            #region MDLLOT_ID
            Get_FORM_MDLLOT_CBO(null, (dt, ex) =>
            {
                if (ex != null)
                {
                    ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), null, "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
                    return;
                }

                _dvMODEL = dt.DefaultView;

                CommonCombo.SetDtCommonCombo(_dvMODEL.ToTable(), dgAutoChgRoutSet.Columns["MDLLOT_ID"], CommonCombo.ComboStatus.SELECT);
            });
            #endregion MDLLOT_ID

            #region CHG_ROUT
            Get_FORM_ROUT_CBO(null, null, "D", null, (dt, ex) =>
            {
                if (ex != null)
                {
                    ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), null, "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
                    return;
                }

                _dvROUTE = dt.DefaultView;

                CommonCombo.SetDtCommonCombo(_dvROUTE.ToTable(), dgAutoChgRoutSet.Columns["CHG_ROUT"], CommonCombo.ComboStatus.SELECT);
            });
            #endregion CHG_ROUT

            #region CHG_ROUT_SHIF
            Get_MMD_SEL_AREA_CMCD_TYPE_MULT("'COMBO_SHIFT'", LoginInfo.CFG_AREA_ID, "Y", (dt, ex) =>
            {
                if (ex != null)
                {
                    ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex.ToString()), null, "Info", MessageBoxButton.OK, MessageBoxIcon.Warning);
                    return;
                }

                _dvSHIFT = dt.DefaultView;

                CommonCombo.SetDtCommonCombo(_dvSHIFT.ToTable(), dgAutoChgRoutSet.Columns["CHG_ROUT_SHIF"], CommonCombo.ComboStatus.SELECT);
            });
            #endregion CHG_ROUT_SHIF

            #region CHG_LINE_ID
            Get_FORM_EQSG_BY_MODEL_CBO((dt, ex) =>
            {
                if (ex != null)
                {
                    ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex.ToString()), null, "Info", MessageBoxButton.OK, MessageBoxIcon.Warning);
                    return;
                }

                _dvCHGLINE = dt.DefaultView;

                CommonCombo.SetDtCommonCombo(_dvCHGLINE.ToTable(), dgAutoChgRoutSet.Columns["CHG_LINE_ID"], CommonCombo.ComboStatus.SELECT);
            });
            #endregion CHG_LINE_ID
        }

        #endregion

        #region Method
        private void GetList()
        {
            try
            {
                Util.gridClear(dgAutoChgRoutSet);
                //dgAutoChgRoutSet.ClearRows();

                DataTable dtRqst = new DataTable();
                dtRqst.Columns.Add("LANGID", typeof(string));
                dtRqst.Columns.Add("AREAID", typeof(string));
                dtRqst.Columns.Add("USE_FLAG", typeof(string));

                DataRow dr = dtRqst.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["AREAID"] = LoginInfo.CFG_AREA_ID;
                dr["USE_FLAG"] = Util.GetCondition(cboUseFlag, bAllNull: true);
                
                dtRqst.Rows.Add(dr);
                
                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_SEL_AUTO_CHG_ROUT_SET_UI", "RQSTDT", "RSLTDT", dtRqst);

                if (dtRslt == null || dtRslt.Rows.Count == 0)
                {
                    Util.GridSetData(dgAutoChgRoutSet, dtRslt, FrameOperation);

                    ////조회된 값이 없습니다.
                    //Util.Alert("FM_ME_0232");
                    //return;
                }
                else
                {
                    //CBO_NAME -> CBO_CODE : CBO_NAME 으로 표시되도록 변경
                    for (int i = 0; i < dtRslt.Rows.Count; i++)
                    {
                        dtRslt.Rows[i]["PROCID"] = dtRslt.Rows[i]["PROCID"].ToString() + " : PreAging#1";
                    }

                    //dgAutoChgRoutSet.ItemsSource = DataTableConverter.Convert(dtRslt);
                    Util.GridSetData(dgAutoChgRoutSet, dtRslt, FrameOperation);
                }

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        
        private void Loc_btnSave_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                //편집중인 내역 Commit.
                dgAutoChgRoutSet.EndEdit(true);
                //dgAutoChgRoutSet.EndEditRow(true);

                DataTable dtRqst = new DataTable();
                dtRqst.TableName = "dtRqst";
                dtRqst.Columns.Add("AREAID", typeof(string));
                dtRqst.Columns.Add("LINE_ID", typeof(string));
                dtRqst.Columns.Add("MDLLOT_ID", typeof(string));
                dtRqst.Columns.Add("PROCID", typeof(string));
                dtRqst.Columns.Add("CHG_LINE_ID", typeof(string));
                dtRqst.Columns.Add("CHG_ROUT", typeof(string));
                dtRqst.Columns.Add("CHG_ROUT_SHIF", typeof(string));
                dtRqst.Columns.Add("CHG_ROUT_DATE", typeof(string));
                dtRqst.Columns.Add("UPDUSER", typeof(string));
                dtRqst.Columns.Add("INSUSER", typeof(string));
                dtRqst.Columns.Add("USE_FLAG", typeof(string));
                dtRqst.Columns.Add("DATASTATE", typeof(string));
                DataRow dr = null;

                foreach (object added in dgAutoChgRoutSet.GetAddedItems())
                {
                    if (DataTableConverter.GetValue(added, "CHK").Equals("True"))
                    {
                        if (!In_Validliation(added)) return;

                        dr = dtRqst.NewRow();
                        dr["AREAID"] = LoginInfo.CFG_AREA_ID;
                        dr["LINE_ID"] = DataTableConverter.GetValue(added, "LINE_ID");
                        dr["MDLLOT_ID"] = DataTableConverter.GetValue(added, "MDLLOT_ID");
                        dr["PROCID"] = "FF9101";
                        dr["CHG_LINE_ID"] = DataTableConverter.GetValue(added, "CHG_LINE_ID");
                        dr["CHG_ROUT"] = DataTableConverter.GetValue(added, "CHG_ROUT");
                        dr["CHG_ROUT_SHIF"] = DataTableConverter.GetValue(added, "CHG_ROUT_SHIF");
                        dr["CHG_ROUT_DATE"] = DateTime.Parse(Convert.ToString(DataTableConverter.GetValue(added, "CHG_ROUT_DATE"))).ToString("yyyy-MM-dd");
                        dr["UPDUSER"] = LoginInfo.USERID;
                        dr["INSUSER"] = LoginInfo.USERID;
                        dr["USE_FLAG"] = Util.NVC(DataTableConverter.GetValue(added, "USE_FLAG"));
                        dr["DATASTATE"] = "Add";
                        dtRqst.Rows.Add(dr);
                        
                    }
                }
                foreach (object modified in dgAutoChgRoutSet.GetModifiedItems())
                {
                    if (DataTableConverter.GetValue(modified, "CHK").Equals("True"))
                    {
                        if (!In_Validliation(modified)) return;
                        
                        dr = dtRqst.NewRow();
                        dr["AREAID"] = LoginInfo.CFG_AREA_ID;
                        dr["LINE_ID"] = DataTableConverter.GetValue(modified, "LINE_ID");
                        dr["MDLLOT_ID"] = DataTableConverter.GetValue(modified, "MDLLOT_ID");
                        dr["PROCID"] = "FF9101";
                        dr["CHG_LINE_ID"] = DataTableConverter.GetValue(modified, "CHG_LINE_ID");
                        dr["CHG_ROUT"] = DataTableConverter.GetValue(modified, "CHG_ROUT");
                        dr["CHG_ROUT_SHIF"] = DataTableConverter.GetValue(modified, "CHG_ROUT_SHIF");
                        dr["CHG_ROUT_DATE"] = DateTime.Parse(Convert.ToString(DataTableConverter.GetValue(modified, "CHG_ROUT_DATE"))).ToString("yyyy-MM-dd");
                        dr["UPDUSER"] = LoginInfo.USERID;
                        dr["INSUSER"] = LoginInfo.USERID;
                        dr["USE_FLAG"] = Util.NVC(DataTableConverter.GetValue(modified, "USE_FLAG"));
                        dr["DATASTATE"] = "Modify";
                        dtRqst.Rows.Add(dr);
                        
                            
                    }
                }

                if (dtRqst.Rows.Count < 1)
                {
                    Util.MessageValidation("SFU3538"); //선택된 데이터가 없습니다
                    return;
                }

                Util.MessageConfirm("FM_ME_0214", (result) => //저장하시겠습니까?
                {
                    if (result == MessageBoxResult.OK)
                    {
                        DataTable dtRslt = new ClientProxy().ExecuteServiceSync("BR_SET_AUTO_CHG_ROUT_SET_UI", "INDATA", "OUTDATA", dtRqst);

                        if (dtRslt.Rows[0]["RETVAL"].ToString().Equals("0")) //0성공, -1실패
                        {
                            Util.MessageValidation("FM_ME_0215");  //저장하였습니다.
                        }
                        else
                        {
                            Util.MessageValidation("SFU2051","");  //중복 데이터가 존재 합니다.

                            return;
                        }

                        GetList();
                    }
                });
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        public void Get_MMD_SEL_CMCD_TYPE_MULT(string sCMCDTYPE, string sUSE_FLAG, Action<DataTable, Exception> ACTION_COMPLETED)
        {
            DataTable IndataTable = new DataTable();
            IndataTable.Columns.Add("LANGID", typeof(string));
            IndataTable.Columns.Add("CMCDTYPE", typeof(string));
            IndataTable.Columns.Add("USE_FLAG", typeof(string));

            DataRow Indata = IndataTable.NewRow();
            Indata["LANGID"] = LoginInfo.LANGID;
            Indata["CMCDTYPE"] = sCMCDTYPE;
            Indata["USE_FLAG"] = sUSE_FLAG;

            IndataTable.Rows.Add(Indata);

            new ClientProxy().ExecuteService("MMD_SEL_CMCD_TYPE_MULT", "RQSTDT", "RSLTDT", IndataTable, (Result, Exception) =>
            {
                try
                {
                    if (ACTION_COMPLETED != null) ACTION_COMPLETED(Result, Exception);
                }
                catch (Exception ex)
                {
                    ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
                }
            }
            );
        }

        public void Get_FORM_EQSG_CBO(Action<DataTable, Exception> ACTION_COMPLETED)
        {
            DataTable IndataTable = new DataTable();
            IndataTable.Columns.Add("LANGID", typeof(string));
            IndataTable.Columns.Add("AREAID", typeof(string));

            DataRow Indata = IndataTable.NewRow();
            Indata["LANGID"] = LoginInfo.LANGID;
            Indata["AREAID"] = LoginInfo.CFG_AREA_ID;

            IndataTable.Rows.Add(Indata);

            new ClientProxy().ExecuteService("DA_BAS_SEL_COMBO_FORM_LINE", "RQSTDT", "RSLTDT", IndataTable, (Result, Exception) =>
            {
                try
                {
                    if (ACTION_COMPLETED != null) ACTION_COMPLETED(Result, Exception);
                }
                catch (Exception ex)
                {
                    ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
                }
            }
            );
        }

        public void Get_FORM_MDLLOT_CBO(string sEQSGID, Action<DataTable, Exception> ACTION_COMPLETED)
        {
            DataTable IndataTable = new DataTable();
            IndataTable.Columns.Add("AREAID", typeof(string));
            IndataTable.Columns.Add("LANGID", typeof(string));
            IndataTable.Columns.Add("EQSGID", typeof(string));

            DataRow Indata = IndataTable.NewRow();
            Indata["AREAID"] = LoginInfo.CFG_AREA_ID;
            Indata["LANGID"] = LoginInfo.LANGID;
            Indata["EQSGID"] = sEQSGID;

            IndataTable.Rows.Add(Indata);

            new ClientProxy().ExecuteService("DA_BAS_SEL_COMBO_FORM_MODEL_CBO", "RQSTDT", "RSLTDT", IndataTable, (Result, Exception) =>
            {
                try
                {
                    if (ACTION_COMPLETED != null) ACTION_COMPLETED(Result, Exception);
                }
                catch (Exception ex)
                {
                    ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
                }
            }
            );
        }

        public void Get_FORM_ROUT_CBO(string sEQSGID, string sMDLLOT_ID, string sROUT_GR_CODE, string sROUT_TYPE_CODE, Action<DataTable, Exception> ACTION_COMPLETED)
        {
            DataTable IndataTable = new DataTable();
            IndataTable.Columns.Add("AREAID", typeof(string));
            IndataTable.Columns.Add("LANGID", typeof(string));
            //IndataTable.Columns.Add("EQSGID", typeof(string));
            IndataTable.Columns.Add("MDLLOT_ID", typeof(string));
            IndataTable.Columns.Add("ROUT_GR_CODE", typeof(string));
            IndataTable.Columns.Add("ROUT_TYPE_CODE", typeof(string));

            DataRow Indata = IndataTable.NewRow();
            Indata["AREAID"] = LoginInfo.CFG_AREA_ID;
            Indata["LANGID"] = LoginInfo.LANGID;
            //Indata["EQSGID"] = sEQSGID;
            Indata["MDLLOT_ID"] = sMDLLOT_ID;
            Indata["ROUT_GR_CODE"] = sROUT_GR_CODE;
            Indata["ROUT_TYPE_CODE"] = sROUT_TYPE_CODE;

            IndataTable.Rows.Add(Indata);

            new ClientProxy().ExecuteService("DA_BAS_SEL_COMBO_FORM_ROUT_CBO", "RQSTDT", "RSLTDT", IndataTable, (Result, Exception) =>
            {
                try
                {
                    if (ACTION_COMPLETED != null) ACTION_COMPLETED(Result, Exception);
                }
                catch (Exception ex)
                {
                    ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
                }
            }
            );
        }

        public void Get_MMD_SEL_AREA_CMCD_TYPE_MULT(string sCMCDTYPE, string sAREAID, string sUSE_FLAG, Action<DataTable, Exception> ACTION_COMPLETED)
        {
            DataTable IndataTable = new DataTable();
            IndataTable.Columns.Add("LANGID", typeof(string));
            IndataTable.Columns.Add("USERID", typeof(string));
            IndataTable.Columns.Add("AREAID", typeof(string));
            IndataTable.Columns.Add("CMCDTYPE", typeof(string));
            IndataTable.Columns.Add("USE_FLAG", typeof(string));

            DataRow Indata = IndataTable.NewRow();
            Indata["LANGID"] = LoginInfo.LANGID;
            Indata["USERID"] = LoginInfo.USERID;
            Indata["AREAID"] = sAREAID;
            Indata["CMCDTYPE"] = sCMCDTYPE;
            Indata["USE_FLAG"] = sUSE_FLAG;

            IndataTable.Rows.Add(Indata);
            new ClientProxy().ExecuteService("MMD_SEL_AREA_CMCD_TYPE_MULT", "RQSTDT", "RSLTDT", IndataTable, (Result, Exception) =>
            {
                try
                {
                    if (ACTION_COMPLETED != null) ACTION_COMPLETED(Result, Exception);
                }
                catch (Exception ex)
                {
                    ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
                }
            }
            );
        }
        public void Get_FORM_EQSG_BY_MODEL_CBO(Action<DataTable, Exception> ACTION_COMPLETED)
        {
            DataTable IndataTable = new DataTable();
            IndataTable.Columns.Add("LANGID", typeof(string));
            IndataTable.Columns.Add("AREAID", typeof(string));

            DataRow Indata = IndataTable.NewRow();
            Indata["LANGID"] = LoginInfo.LANGID;
            Indata["AREAID"] = LoginInfo.CFG_AREA_ID;

            IndataTable.Rows.Add(Indata);

            new ClientProxy().ExecuteService("DA_BAS_SEL_COMBO_LINE_BY_MODEL", "RQSTDT", "RSLTDT", IndataTable, (Result, Exception) =>
            {
                try
                {
                    if (ACTION_COMPLETED != null) ACTION_COMPLETED(Result, Exception);
                }
                catch (Exception ex)
                {
                    ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
                }
            }
            );
        }

        #endregion

        #region Event

        private void Loc_btnUnitPlus_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Util.DataGridRowAdd(dgAutoChgRoutSet, Convert.ToInt32(numAddCount.Value));
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void Loc_btnUnitMinus_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Util.DataGridRowDelete(dgAutoChgRoutSet, 1);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void dgAutoChgRoutSet_BeginningNewRow(object sender, C1.WPF.DataGrid.DataGridBeginningNewRowEventArgs e)
        {
            e.Item.SetValue("CHK", true);
            e.Item.SetValue("USE_FLAG", "Y");
            e.Item.SetValue("LINE_ID", "SELECT");
            e.Item.SetValue("MDLLOT_ID", "SELECT");
            e.Item.SetValue("CHG_LINE_ID", "SELECT");
            e.Item.SetValue("CHG_ROUT", "SELECT");
            e.Item.SetValue("PROCID", "FF9101 : PreAging#1");
            e.Item.SetValue("CHG_ROUT_SHIF", "SELECT");
            e.Item.SetValue("INSUSER", LoginInfo.USERID);
            e.Item.SetValue("UPDUSER", LoginInfo.USERID);
        }

        private void dgAutoChgRoutSet_BeginningEdit(object sender, C1.WPF.DataGrid.DataGridBeginningEditEventArgs e)
        {
            try
            {
                //string[] exceptionColumn = { "AREAID", "GROUPCODE" };
                Util.DataGridRowEditByCheckBoxColumn(e, null); //CHK true인 행만 수정가능

                DataRowView drv = e.Row.DataItem as DataRowView;

                string sCHK = Util.NVC(DataTableConverter.GetValue(e.Row.DataItem, "CHK")).ToUpper().ToString();

                if (sCHK.Equals("TRUE"))
                {
                    C1DataGrid dg = sender as C1DataGrid;
                    if (drv.Row.RowState == DataRowState.Added || drv.Row.RowState == DataRowState.Detached)
                    {
                        if (e.Column.Name == "MDLLOT_ID")
                        {
                            string sLineId = Util.NVC(DataTableConverter.GetValue(e.Row.DataItem, "LINE_ID"));
                            string sMdllotId = Util.NVC(DataTableConverter.GetValue(e.Row.DataItem, "MDLLOT_ID"));

                            if (string.IsNullOrEmpty(sLineId) || string.Equals(sLineId, "SELECT"))
                            {
                                Util.MessageValidation("SFU4925", ObjectDic.Instance.GetObjectName("LINE_ID"));
                                //DataTableConverter.SetValue(e.Row.DataItem, "MDLLOT_ID", "SELECT");
                                //DataTableConverter.SetValue(e.Row.DataItem, "CHG_ROUT", "SELECT");
                                //dg.Refresh();

                                e.Cancel = true;
                            }
                        }

                        else if (e.Column.Name == "CHG_ROUT")
                        {
                            string sChgLineId = Util.NVC(DataTableConverter.GetValue(e.Row.DataItem, "CHG_LINE_ID"));

                            if (string.IsNullOrEmpty(sChgLineId) || string.Equals(sChgLineId, "SELECT"))
                            {
                                Util.MessageValidation("SFU4925", ObjectDic.Instance.GetObjectName("CHG_LINE_ID"));
                                //DataTableConverter.SetValue(e.Row.DataItem, "MDLLOT_ID", "SELECT");
                                //DataTableConverter.SetValue(e.Row.DataItem, "CHG_ROUT", "SELECT");
                                //dg.Refresh();

                                e.Cancel = true;
                            }

                        }
                        else if (e.Column.Name == "CHG_LINE_ID")
                        {
                            string sMdllotId = Util.NVC(DataTableConverter.GetValue(e.Row.DataItem, "MDLLOT_ID"));

                            if (string.IsNullOrEmpty(sMdllotId) || string.Equals(sMdllotId, "SELECT"))
                            {
                                Util.MessageValidation("SFU4925", ObjectDic.Instance.GetObjectName("MDLLOT_ID"));
                                //DataTableConverter.SetValue(e.Row.DataItem, "CHG_ROUT", "SELECT");
                                //dg.Refresh();

                                e.Cancel = true;
                            }
                        }
                    }
                    else
                    {
                        if (e.Column != this.dgAutoChgRoutSet.Columns["CHK"]
                         && e.Column != this.dgAutoChgRoutSet.Columns["USE_FLAG"]
                         && e.Column != this.dgAutoChgRoutSet.Columns["CHG_ROUT_DATE"]
                         && e.Column != this.dgAutoChgRoutSet.Columns["CHG_ROUT_SHIF"])
                        {
                            e.Cancel = true;
                        }
                        else
                        {
                            e.Cancel = false;
                        }
                    }

                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void dgAutoChgRoutSet_BeganEdit(object sender, DataGridBeganEditEventArgs e)
        {
            try
            {
                C1ComboBox cbo = e.EditingElement as C1ComboBox;
                DataRowView drv = e.Row.DataItem as DataRowView;
                //C1DataGrid dg = sender as C1DataGrid;

                if (cbo != null)
                {
                    string sSelectedValue = drv[e.Column.Name].SafeToString();

                    if (e.Column.Name == "LINE_ID")
                    {
                        //
                    }
                    else if (e.Column.Name == "MDLLOT_ID")
                    {
                        string sLineId = Util.NVC(DataTableConverter.GetValue(drv, "LINE_ID"));

                        if (!string.IsNullOrEmpty(sLineId))
                        {
                            string sFilter = "EQSGID = '" + sLineId + "'";
                            _dvMODEL.RowFilter = sFilter;

                            CommonCombo.SetDtCommonCombo(_dvMODEL.ToTable(), cbo, CommonCombo.ComboStatus.SELECT, sSelectedValue);
                        }
                    }
                    else if (e.Column.Name == "CHG_LINE_ID")
                    {
                        string sMdllotId = Util.NVC(DataTableConverter.GetValue(drv, "MDLLOT_ID"));

                        if (!string.IsNullOrEmpty(sMdllotId))
                        {
                            string sFilter = "MDLLOT_ID = '" + sMdllotId + "'";
                            _dvCHGLINE.RowFilter = sFilter;

                            CommonCombo.SetDtCommonCombo(_dvCHGLINE.ToTable(), cbo, CommonCombo.ComboStatus.SELECT, sSelectedValue);
                        }
                    }
                    else if (e.Column.Name == "CHG_ROUT")
                    {
                        string sLineId = Util.NVC(DataTableConverter.GetValue(drv, "CHG_LINE_ID"));
                        string sMdllotId = Util.NVC(DataTableConverter.GetValue(drv, "MDLLOT_ID"));

                        if (!string.IsNullOrEmpty(sLineId) && !string.IsNullOrEmpty(sMdllotId))
                        {
                            string sFilter = "EQSGID = '" + sLineId + "'"
                                   + "AND " + "MDLLOT_ID = '" + sMdllotId + "'";
                            _dvROUTE.RowFilter = sFilter;

                            CommonCombo.SetDtCommonCombo(_dvROUTE.ToTable(), cbo, CommonCombo.ComboStatus.SELECT, sSelectedValue);
                        }
                    }

                    cbo.EditCompleted += delegate (object sender1, EventArgs e1)
                    {
                        Dispatcher.BeginInvoke(new Action(() =>
                        {
                            this.UpdateRowView(e.Row, e.Column);
                        }));
                    };
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        
        private void dgAutoChgRoutSet_CommittingEdit(object sender, DataGridEndingEditEventArgs e)
        {
            if (_manualCommit == false)
            {
                this.UpdateRowView(e.Row, e.Column);
            }

            //try
            //{
            //    if (_manualCommit == false)
            //    {
            //        _manualCommit = true;

            //        DataRowView drv = e.Row.DataItem as DataRowView;

            //        if (e.Column.Name == "LINE_ID")
            //        {
            //            //라인 설정시 모델,라우트 값 초기화
            //            DataTableConverter.SetValue(drv, "MDLLOT_ID", "SELECT");
            //            DataTableConverter.SetValue(drv, "CHG_ROUT", "SELECT");
            //            dgAutoChgRoutSet.EndEditRow(true);
            //        }
            //        else if (e.Column.Name == "MDLLOT_ID")
            //        {
            //            //모델 설정시 라우트 값 초기화
            //            DataTableConverter.SetValue(drv, "CHG_ROUT", "SELECT");
            //            dgAutoChgRoutSet.EndEditRow(true);
            //        }
            //    }
            //}
            //catch (Exception ex)
            //{
            //    Util.MessageException(ex);
            //}
            //finally
            //{
            //    _manualCommit = false;
            //}
        }

        private void dgAutoChgRoutSet_CommittedEdit(object sender, DataGridCellEventArgs e)
        {
            try
            {

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void CheckBox_Click(object sender, RoutedEventArgs e)
        {
            int checkIndex = ((C1.WPF.DataGrid.DataGridCellPresenter)((sender as CheckBox).Parent)).Row.Index;

        }

        private void btnExcel_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                new LGC.GMES.MES.Common.ExcelExporter().Export(dgAutoChgRoutSet);
            }
            catch (Exception ex)
            {
                ControlsLibrary.MessageBox.Show(ex.Message, null, "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }

        private void Loc_btnSearch_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                GetList();
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void cboUseFlag_SelectedItemChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            try
            {
                //if (!CommonVerify.HasDataGridRow(dgAutoChgRoutSet))
                //    return;

                GetList();
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void chkHeaderAll_Checked(object sender, RoutedEventArgs e)
        {
            Util.DataGridCheckAllChecked(dgAutoChgRoutSet);
        }

        private void chkHeaderAll_Unchecked(object sender, RoutedEventArgs e)
        {
            Util.DataGridCheckAllUnChecked(dgAutoChgRoutSet);
        }

        private bool In_Validliation(object row)
        {
            string[] columns = { "LINE_ID","MDLLOT_ID","CHG_LINE_ID", "CHG_ROUT", "CHG_ROUT_DATE", "CHG_ROUT_SHIF" };
            foreach(string col in columns)
            {
                if (Util.IsNVC(DataTableConverter.GetValue(row, col)) || DataTableConverter.GetValue(row, col).ToString() == "SELECT")
                {
                    Util.MessageValidation("SFU4925", ObjectDic.Instance.GetObjectName(col));
                    return false;
                }
            }
            return true;
        }


        private void dgAutoChgRoutSet_LoadedCellPresenter(object sender, DataGridCellEventArgs e)
        {
            if (!((DataRowView)e.Cell.Row.DataItem).Row.RowState.ToString().Equals(DataRowState.Added.ToString()))
            {
                string[] Col = { "LINE_ID", "MDLLOT_ID", "CHG_LINE_ID", "CHG_ROUT", "PROCID", "INSUSER", "INSDTTM", "UPDUSER", "UPDDTTM" };

                dgAutoChgRoutSet?.Dispatcher.BeginInvoke(new Action(() =>
                {
                    if (e.Cell == null || e.Cell.Presenter == null) return;

                    if (Col.Contains(e.Cell.Column.Name))
                    {
                        e.Cell.Presenter.Background = (SolidColorBrush)new BrushConverter().ConvertFrom("#fff8f8f8");
                    }
                }));
            }
            else
            {
                string[] Col = { "PROCID", "INSUSER", "INSDTTM", "UPDUSER", "UPDDTTM" };

                dgAutoChgRoutSet?.Dispatcher.BeginInvoke(new Action(() =>
                {
                    if (e.Cell == null || e.Cell.Presenter == null) return;

                    if (Col.Contains(e.Cell.Column.Name))
                    {
                        e.Cell.Presenter.Background = (SolidColorBrush)new BrushConverter().ConvertFrom("#fff8f8f8");
                    }
                }));
            }
        }

        private void dgAutoChgRoutSet_UnloadedCellPresenter(object sender, DataGridCellEventArgs e)
        {
            //CommonUtil.DataGridReadOnlyBackgroundColor(e, dgRoute);
        }

        void UpdateRowView(C1.WPF.DataGrid.DataGridRow dgr, C1.WPF.DataGrid.DataGridColumn dgc)
        {
            try
            {
                DataRowView drv = dgr.DataItem as DataRowView;

                if (drv != null && Convert.ToString(dgc.Name) == "LINE_ID")
                {
                    _manualCommit = true;

                    drv["MDLLOT_ID"] = "SELECT";
                    drv["CHG_LINE_ID"] = "SELECT";
                    drv["CHG_ROUT"] = "SELECT";

                    this.dgAutoChgRoutSet.EndEditRow(true);
                }

                if (drv != null && Convert.ToString(dgc.Name) == "MDLLOT_ID")
                {
                    _manualCommit = true;

                    drv["CHG_LINE_ID"] = "SELECT";
                    drv["CHG_ROUT"] = "SELECT";

                    this.dgAutoChgRoutSet.EndEditRow(true);
                }
                if (drv != null && Convert.ToString(dgc.Name) == "CHG_LINE_ID")
                {
                    _manualCommit = true;

                    drv["CHG_ROUT"] = "SELECT";

                    this.dgAutoChgRoutSet.EndEditRow(true);
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
            finally
            {
                _manualCommit = false;
            }
        }
        #endregion
    }
}
