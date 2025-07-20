/*************************************************************************************
 Created Date : 2022.11.04
      Creator : 이정미
   Decription : Tray Type별 점유율 관리
--------------------------------------------------------------------------------------
 [Change History]
  2022.11.01 NAME   : Initial Created.  
 
**************************************************************************************/
using C1.WPF;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using System;
using System.Data;
using System.Windows;
using System.Windows.Controls;


namespace LGC.GMES.MES.FCS001
{
    public partial class FCS001_131 : UserControl, IWorkArea
    {
        #region Declaration & Constructor 

        Util _Util = new Util();

        public FCS001_131()
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
            _combo.SetCombo(cboDest, CommonCombo_Form.ComboStatus.NONE, sCase: "SCEQPID"); //수정 필요 
        }
        #endregion

        #region Event
        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            InitCombo();
            this.Loaded -= UserControl_Loaded;
        }

        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            GetList();
        }

        private void btnUnitPlus_Click(object sender, RoutedEventArgs e)
        {
            DataGridRowAdd(dgTrayList, 1);
            DataTableConverter.SetValue(dgTrayList.Rows[dgTrayList.Rows.Count - 1].DataItem, "FLAG", "Y");
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

                dgTrayList.EndEdit(true);

                DataTable dtRqst = new DataTable();
                dtRqst.TableName = "INDATA";
                dtRqst.Columns.Add("LANGID", typeof(string));
                dtRqst.Columns.Add("PROCESS_TYPE", typeof(string));
                dtRqst.Columns.Add("CNVR_LOCATION_ID", typeof(string));
                dtRqst.Columns.Add("CNVR_LOCATION_DESC", typeof(string));
                dtRqst.Columns.Add("CST_TYPE_CODE", typeof(string));
                dtRqst.Columns.Add("RATIO_LLMT", typeof(string));
                dtRqst.Columns.Add("RATIO_ULMT", typeof(string));
                dtRqst.Columns.Add("USER_ID", typeof(string));
                dtRqst.Columns.Add("MENUID", typeof(string));
                dtRqst.Columns.Add("MANUPATH", typeof(string));
                dtRqst.Columns.Add("USER_IP", typeof(string));
                dtRqst.Columns.Add("PC_NAME", typeof(string));

                for (int i = 0; i < dgTrayList.Rows.Count; i++)
                {
                    if (Util.NVC(DataTableConverter.GetValue(dgTrayList.Rows[i].DataItem, "FLAG")).Equals("Y"))
                    {
                        DataRow dr = dtRqst.NewRow();
                        dr["LANGID"] = LoginInfo.LANGID;
                        dr["PROCESS_TYPE"] = "I";
                        dr["CNVR_LOCATION_ID"] = Util.NVC(DataTableConverter.GetValue(dgTrayList.Rows[i].DataItem, "CNVR_LOCATION_ID"));
                        dr["CNVR_LOCATION_DESC"] = Util.NVC(DataTableConverter.GetValue(dgTrayList.Rows[i].DataItem, "CNVR_LOCATION_DESC"));
                        dr["CST_TYPE_CODE"] = Util.NVC(DataTableConverter.GetValue(dgTrayList.Rows[i].DataItem, "CST_TYPE_CODE"));
                        dr["RATIO_LLMT"] = Util.NVC(DataTableConverter.GetValue(dgTrayList.Rows[i].DataItem, "RATIO_LLMT"));
                        dr["RATIO_ULMT"] = Util.NVC(DataTableConverter.GetValue(dgTrayList.Rows[i].DataItem, "RATIO_ULMT"));
                        dr["USER_ID"] = LoginInfo.USERID;
                        dr["MENUID"] = LoginInfo.CFG_MENUID;
                        dr["MANUPATH"] = "";
                        dr["USER_IP"] = LoginInfo.USER_IP;
                        dr["PC_NAME"] = LoginInfo.PC_NAME;
                        dtRqst.Rows.Add(dr);
                    }
                }

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("BR_SET_RATE_INFO_UI", "INDATA", "OUTDATA", dtRqst);

                if (dtRslt != null)
                {
                    if (dtRslt.Rows[0]["RETVAL"].ToString().Equals("0"))
                    {
                        Util.Alert("FM_ME_0215");  //저장하였습니다.
                        GetList();
                    }
                    else
                    {
                        Util.Alert("FM_ME_0213");  //저장실패하였습니다.
                        return;
                    }
                }
                else
                {
                    Util.Alert("FM_ME_0213");  //저장실패하였습니다.
                    return;
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            DataTable dtRqst = new DataTable();
            dtRqst.TableName = "INDATA";
            dtRqst.Columns.Add("PROCESS_TYPE", typeof(string));
            dtRqst.Columns.Add("CNVR_LOCATION_DESC", typeof(string));
            dtRqst.Columns.Add("RATIO_LLMT", typeof(string));
            dtRqst.Columns.Add("RATIO_ULMT", typeof(string));
            dtRqst.Columns.Add("USERID", typeof(string));
            dtRqst.Columns.Add("CNVR_LOCATION_ID", typeof(string));
            dtRqst.Columns.Add("CST_TYPE_CODE", typeof(string));

            for (int i = 0; i < dgTrayList.Rows.Count; i++)
            {
                if (Util.NVC(DataTableConverter.GetValue(dgTrayList.Rows[i].DataItem, "CHK")).ToUpper().Equals("TRUE"))
                {
                    DataRow dr = dtRqst.NewRow();
                    dr["PROCESS_TYPE"] = "D";
                    dr["CNVR_LOCATION_DESC"] = Util.NVC(DataTableConverter.GetValue(dgTrayList.Rows[i].DataItem, "CNVR_LOCATION_DESC"));
                    dr["RATIO_LLMT"] = Util.NVC(DataTableConverter.GetValue(dgTrayList.Rows[i].DataItem, "RATIO_LLMT")); ;
                    dr["RATIO_ULMT"] = Util.NVC(DataTableConverter.GetValue(dgTrayList.Rows[i].DataItem, "RATIO_ULMT"));
                    dr["USERID"] = LoginInfo.USERID;
                    dr["CNVR_LOCATION_ID"] = Util.NVC(DataTableConverter.GetValue(dgTrayList.Rows[i].DataItem, "CNVR_LOCATION_ID"));
                    dr["CST_TYPE_CODE"] = Util.NVC(DataTableConverter.GetValue(dgTrayList.Rows[i].DataItem, "CST_TYPE_CODE"));
                    dtRqst.Rows.Add(dr);
                }
            }

            DataTable dtRslt = new ClientProxy().ExecuteServiceSync("BR_SET_RATE_INFO_UI", "INDATA", "OUTDATA", dtRqst);

            if (dtRslt.Rows[0]["RESULT"].ToString().Equals("0")) //0성공, -1실패
            {
                Util.MessageValidation("FM_ME_0154");  //삭제하였습니다.
            }
            else
            {
                Util.MessageValidation("FM_ME_0153");  //삭제 실패하였습니다.
            }

            GetList();
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
        #endregion

        #region Method
        private void GetList() ///수정 필요
        {
            try
            {
                DataTable dtRqst = new DataTable();
                dtRqst.Columns.Add("LANGID", typeof(string));
                dtRqst.Columns.Add("AREAID", typeof(string));
                dtRqst.Columns.Add("CNVR_LOCATION_ID", typeof(string));

                DataRow dr = dtRqst.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["AREAID"] = LoginInfo.CFG_AREA_ID;
                dr["CNVR_LOCATION_ID"] = Util.GetCondition(cboDest, bAllNull: true);

                dtRqst.Rows.Add(dr);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("BR_GET_DESTINATION_RATE_UI", "INDATA", "OUTDATA", dtRqst);

                dtRslt.Columns.Add("CHK", typeof(bool));
                dtRslt.Columns.Add("FLAG", typeof(string));
                for (int i = 0; i < dtRslt.Rows.Count; i++)
                {
                    dtRslt.Rows[i]["FLAG"] = "N";
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
                    dt.Columns.Add("FLAG", typeof(string));

                if (dg.ItemsSource != null)
                {
                    for (int i = 0; i < iRowcount; i++)
                    {
                        dt = DataTableConverter.Convert(dg.ItemsSource);

                        if (dg.Rows.Count == 0)
                            dt.Columns.Add("FLAG", typeof(string));

                        DataRow dr = dt.NewRow();
                        dr["MIN_RATE"] = "0";
                        dr["MAX_RATE"] = "0";
                        dr["INSUSER"] = LoginInfo.USERID;
                        dr["UPDUSER"] = LoginInfo.USERID;
                        dr["FLAG"] = "N";
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
                        dr["MIN_RATE"] = "0";
                        dr["MAX_RATE"] = "0";
                        dr["INSUSER"] = LoginInfo.USERID;
                        dr["UPDUSER"] = LoginInfo.USERID;
                        dr["FLAG"] = "N";
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

        private void GetRangeCheck() //수정 필요
        {
            for (int i = 0; i < dgTrayList.Rows.Count; i++)
            {
                if (Util.NVC(DataTableConverter.GetValue(dgTrayList.Rows[i].DataItem, "FLAG")).Equals("Y"))
                {
                    if (Util.NVC(DataTableConverter.GetValue(dgTrayList.Rows[i].DataItem, "CNVR_LOCATION_ID")).ToString().Equals("") ||
                       string.IsNullOrEmpty(Util.NVC(DataTableConverter.GetValue(dgTrayList.Rows[i].DataItem, "CNVR_LOCATION_ID")).ToString()))
                    {
                        Util.Alert("SFU7024"); //목적지 정보가 없습니다.
                        return;
                    }
                }
            }
            /*if (fpsDestRateInfo.ActiveSheet.ActiveColumnIndex == fpsDestRateInfo.GetColumnIndex("MAX_RATE")
   || fpsDestRateInfo.ActiveSheet.ActiveColumnIndex == fpsDestRateInfo.GetColumnIndex("DEST")) //최대 점유율, 목적지 수정
            {
                int cellValue = 0;

                if (string.IsNullOrEmpty(Convert.ToString(fpsDestRateInfo.GetValue(fpsDestRateInfo.ActiveSheet.ActiveRowIndex, fpsDestRateInfo.GetColumnIndex("DEST")))))
                {
                    fpsDestRateInfo.SetValue(0, "MAX_RATE", 0);
                    Util.AlertMsg("ME_0408"); //목적지를 먼저 입력해주세요.
                    return;
                }

                //같은 목적지만 처리하도록..
                for (int i = 0; i < fpsDestRateInfo.ActiveSheet.Rows.Count; i++)
                {
                    if (Convert.ToString(fpsDestRateInfo.GetValue(fpsDestRateInfo.ActiveSheet.Rows[i].Index, fpsDestRateInfo.GetColumnIndex("DEST"))).Equals(fpsDestRateInfo.GetValue(fpsDestRateInfo.ActiveSheet.ActiveRowIndex, fpsDestRateInfo.GetColumnIndex("DEST"))))
                        cellValue += Convert.ToInt32(fpsDestRateInfo.GetValue(fpsDestRateInfo.ActiveSheet.Rows[i].Index, fpsDestRateInfo.GetColumnIndex("MAX_RATE")) ?? 0);
                }

                int min_rate = Convert.ToInt32(fpsDestRateInfo.GetValue(fpsDestRateInfo.ActiveSheet.ActiveRowIndex, fpsDestRateInfo.GetColumnIndex("MIN_RATE")) ?? 0);
                int max_rate = Convert.ToInt32(fpsDestRateInfo.GetValue(fpsDestRateInfo.ActiveSheet.ActiveRowIndex, fpsDestRateInfo.GetColumnIndex("MAX_RATE")) ?? 0);

                if (cellValue > 100)
                {
                    fpsDestRateInfo.ActiveSheet.Cells[fpsDestRateInfo.ActiveSheet.ActiveRowIndex, fpsDestRateInfo.ActiveSheet.ActiveColumnIndex].BackColor = Color.Tomato;
                    Util.AlertMsg("ME_0404"); //해당 목적지의 트레이타입별 최대 점유율의 합이 100%가 넘습니다.
                    chkval = true;
                }
                else if (min_rate > max_rate)
                {
                    fpsDestRateInfo.ActiveSheet.Cells[fpsDestRateInfo.ActiveSheet.ActiveRowIndex, fpsDestRateInfo.ActiveSheet.ActiveColumnIndex].BackColor = Color.Tomato;
                    Util.AlertMsg("ME_0405"); //최소 점유율이 최대 점유율보다 높습니다.
                    chkval = true;
                }
                else
                {
                    fpsDestRateInfo.ActiveSheet.Cells[fpsDestRateInfo.ActiveSheet.ActiveRowIndex, fpsDestRateInfo.ActiveSheet.ActiveColumnIndex].BackColor = Color.White;
                    chkval = false;
                }
            }
            if (fpsDestRateInfo.ActiveSheet.ActiveColumnIndex == fpsDestRateInfo.GetColumnIndex("MIN_RATE")) //최소 점유율
            {
                int max_rate = Convert.ToInt32(fpsDestRateInfo.GetValue(fpsDestRateInfo.ActiveSheet.ActiveRowIndex, fpsDestRateInfo.GetColumnIndex("MAX_RATE")) ?? 0); ;
                int min_rate = Convert.ToInt32(fpsDestRateInfo.GetValue(fpsDestRateInfo.ActiveSheet.ActiveRowIndex, fpsDestRateInfo.GetColumnIndex("MIN_RATE")) ?? 0); ;

                if (min_rate > max_rate)
                {
                    fpsDestRateInfo.ActiveSheet.Cells[fpsDestRateInfo.ActiveSheet.ActiveRowIndex, fpsDestRateInfo.ActiveSheet.ActiveColumnIndex].BackColor = Color.Tomato;
                    Util.AlertMsg("ME_0405"); //최소 점유율이 최대 점유율보다 높습니다.
                    chkval = true;
                }
                else
                {
                    fpsDestRateInfo.ActiveSheet.Cells[fpsDestRateInfo.ActiveSheet.ActiveRowIndex, fpsDestRateInfo.ActiveSheet.ActiveColumnIndex].BackColor = Color.White;
                    chkval = false;
                }
            }*/
        }
        #endregion
    }
}
