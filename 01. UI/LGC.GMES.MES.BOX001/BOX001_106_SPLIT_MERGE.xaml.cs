/*************************************************************************************
 Created Date : 2017.05.22
      Creator : 이영준S
   Decription : 전지 5MEGA-GMES 구축 - 1차 포장 구성 - 작업시작 팝업
--------------------------------------------------------------------------------------
 [Change History]


**************************************************************************************/

using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Data;
using LGC.GMES.MES.CMM001.Extensions;

using C1.WPF;
using C1.WPF.DataGrid;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.ControlsLibrary;
using System.ComponentModel;
using System.Threading;
using System.Linq;
using System.Windows.Media;

namespace LGC.GMES.MES.BOX001
{
    /// <summary>
    /// BOX001_027_RUNSTART.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class BOX001_106_SPLIT_MERGE : C1Window, IWorkArea
    {

        Util _Util = new Util();
        DataTable _dtData = null;
        DataTable _dtTemp = null;
        DataTable _dtOutLot = null;
        // string[] _resultBoxID = null;
        //string _BOXID = string.Empty;
        DataTable _inData = null;

        string _USERNAME = string.Empty;
        string _SHFTNAME = string.Empty;
        string _WRKSUPP = string.Empty;

        public IFrameOperation FrameOperation
        {
            get;
            set;
        }
        public BOX001_106_SPLIT_MERGE()
        {
            InitializeComponent();
        }
        private void C1Window_Loaded(object sender, RoutedEventArgs e)
        {
            object[] tmps = C1WindowExtension.GetParameters(this);
            ApplyPermissions();
            _inData = (DataTable)tmps[0];
            _USERNAME = (string)tmps[1];
            _SHFTNAME = (string)tmps[2];
            _WRKSUPP = (string)tmps[3];
            GetInpalletList();

        }

        #region Events
        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            dgBot.EndEdit(true);
            dgBot.EndEditRow(true);
            if (dgBot.ItemsSource == null || dgBot.Rows.Count < 1)
                return;
            DoSplitMerge();
        }
        private void PrintTag(string sBoxID)
        {
            DataSet inDataSet = new DataSet();
            DataTable inBox = inDataSet.Tables.Add("INBOX");
            inBox.Columns.Add("LANGID", typeof(string));
            inBox.Columns.Add("BOXID", typeof(string));

            DataRow dr = inBox.NewRow();
            dr["LANGID"] = LoginInfo.LANGID;
            dr["BOXID"] = sBoxID;
            inBox.Rows.Add(dr);

            new ClientProxy().ExecuteService_Multi("BR_PRD_GET_INPALLET_FOR_SHIP_FM", "INBOX", "OUTDATA", (result, ex) =>
            {
                if (ex != null)
                {
                    Util.MessageException(ex);
                    return;
                }

                // Util.MessageInfo("SFU1275");    //정상 처리 되었습니다.
                BOX001_101_REPORT _print = new BOX001_101_REPORT();
                _print.FrameOperation = this.FrameOperation;

                if (_print != null)
                {
                    // SET PARAMETER
                    object[] Parameters = new object[3];
                    Parameters[0] = result;
                    Parameters[1] = _USERNAME;
                    Parameters[2] = txtRemark.Text;
                    C1WindowExtension.SetParameters(_print, Parameters);

                    _print.ShowModal();

                }

            }, inDataSet);
        }

        private void DoSplitMerge()
        {
            try
            {
                DataSet ds = new DataSet();
                DataTable dtInData = ds.Tables.Add("INDATA");
                dtInData.Columns.Add("REMARK", typeof(string));
                dtInData.Columns.Add("SHOPID", typeof(string));
                dtInData.Columns.Add("AREAID", typeof(string));
                dtInData.Columns.Add("USERID", typeof(string));
                dtInData.Columns.Add("SHFT_ID", typeof(string));
                dtInData.Columns.Add("WRK_SUPPLIERID", typeof(string));
                DataRow drIndata = dtInData.NewRow();
                drIndata["REMARK"] = txtRemark.Text;
                drIndata["SHOPID"] = _dtData.Rows[0].Field<string>("SHOPID");
                drIndata["AREAID"] = _dtData.Rows[0].Field<string>("AREAID");
                drIndata["USERID"] = _USERNAME;
                drIndata["SHFT_ID"] = _SHFTNAME;
                drIndata["WRK_SUPPLIERID"] = _WRKSUPP;
                dtInData.Rows.Add(drIndata);

                DataTable dtFrmPlt = ((DataView)dgTop.ItemsSource).Table.Copy();
                dtFrmPlt.TableName = "FROM_PALLET";
                dtFrmPlt.Columns.Remove("WIPQTY");
                dtFrmPlt.Columns["WIPQTY2"].ColumnName = "WIPQTY";
                ds.Tables.Add(dtFrmPlt);

                DataTable dtToPlt = ((DataView)dgBot.ItemsSource).Table.Copy();
                dtToPlt.TableName = "TO_PALLET";
                dtToPlt.Columns.Remove("WIPQTY");
                dtToPlt.Columns["WIPQTY2"].ColumnName = "WIPQTY";
                ds.Tables.Add(dtToPlt);

                DataSet aa = new DataSet();
                aa = ds;
                string xmltxt = ds.GetXml();

                DataSet resultDs = new ClientProxy().ExecuteServiceSync_Multi("BR_PRD_REG_SPLITMERGE_INPALLET_FM", "INDATA,FROM_PALLET,TO_PALLET", "OUTDATA", ds);

                if (resultDs != null && (resultDs.Tables.IndexOf("OUTDATA") > -1) && resultDs.Tables["OUTDATA"].Rows.Count > 0)
                {
                    //_resultBoxID = resultDs.Tables["OUTDATA"].Rows[0]["BOXID"].ToString();
                    for (int i = 0; i < resultDs.Tables["OUTDATA"].Rows.Count; i++)
                    {
                        PrintTag(resultDs.Tables["OUTDATA"].Rows[i]["BOXID"].ToString());
                    }
                }
                this.DialogResult = MessageBoxResult.OK;


                //new ClientProxy().ExecuteService_Multi("BR_PRD_REG_SPLITMERGE_INPALLET_FM", "INDATA,FROM_PALLET,TO_PALLET", "OUTDATA", (resultDs, ex) =>
                //   {
                //       if (ex != null)
                //       {
                //           Util.MessageException(ex);
                //       }
                //       if ((resultDs.Tables.IndexOf("OUTDATA") > -1) && resultDs.Tables["OUTDATA"].Rows.Count > 0)
                //       {
                //           //_resultBoxID = resultDs.Tables["OUTDATA"].Rows[0]["BOXID"].ToString();
                //           for (int i = 0; i < resultDs.Tables["OUTDATA"].Rows.Count; i++)
                //           {
                //               PrintTag(resultDs.Tables["OUTDATA"].Rows[i]["BOXID"].ToString());
                //           }
                //       }
                //       //ControlsLibrary.MessageBox.Show("처리되었습니다.", "", "Caution", MessageBoxButton.OK, MessageBoxIcon.None, null);
                //       //PrintTag(_resultBoxID);
                //       this.DialogResult = MessageBoxResult.OK;
                //   }, ds);

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }

            this.Close();
        }
        #endregion

        #region Methods
        private void ApplyPermissions()
        {
            List<Button> listAuth = new List<Button>();
            Util.pageAuth(listAuth, FrameOperation.AUTHORITY);
        }
        private void GetInpalletList()
        {
            try
            {
                DataSet ds = new DataSet();
                //DataTable dt = ds.Tables.Add("INDATA");
                //dt.Columns.Add("LANGID", typeof(string));
                //dt.Columns.Add("BOXID", typeof(string));
                //DataRow dr = dt.NewRow();
                //dr["LANGID"] = LoginInfo.LANGID;
                //dr["BOXID"] = _BOXID;
                //dt.Rows.Add(dr);
                _inData.TableName = "INDATA";
                ds.Tables.Add(_inData);


                DataSet resultDS = new ClientProxy().ExecuteServiceSync_Multi("BR_PRD_GET_INPALLET_FOR_SPLITMERGE_FM", "INDATA", "OUTDATA,OUTLOT", ds);

                if ((resultDS.Tables.IndexOf("OUTDATA") > -1) && resultDS.Tables["OUTDATA"].Rows.Count > 0)
                {
                    DataTable dtOutData = resultDS.Tables["OUTDATA"];
                    if (!dtOutData.Columns.Contains("CHK"))
                    { dtOutData = _Util.gridCheckColumnAdd(dtOutData, "CHK"); }
                    //_dtTemp = dtOutData.Copy();
                    //for (int i = 0; i < _dtTemp.Rows.Count; i++)
                    //{
                    //    _dtTemp.Rows[i].SetField("WIPQTY2", 0);
                    //}
                    _dtData = dtOutData.Copy();
                    Util.GridSetData(dgTop, dtOutData, FrameOperation, true);


                }
                if ((resultDS.Tables.IndexOf("OUTLOT") > -1) && resultDS.Tables["OUTLOT"].Rows.Count > 0)
                {
                    _dtOutLot = resultDS.Tables["OUTLOT"];
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        #endregion

        private void dgTopChoice_Checked(object sender, RoutedEventArgs e)
        {
            CheckBox cb = sender as CheckBox;
            if (cb?.DataContext == null) return;
            if (cb.IsChecked == null) return;

            DataRowView drv = cb.DataContext as DataRowView;

            drv["CHK"] = true;
            //row색 변경부분 추가
        }

        private void dgBotChoice_Checked(object sender, RoutedEventArgs e)
        {
            CheckBox cb = sender as CheckBox;
            if (cb?.DataContext == null) return;
            if (cb.IsChecked == null) return;

            DataRowView drv = cb.DataContext as DataRowView;

            drv["CHK"] = true;
        }

        private void btnDownwards_Click(object sender, RoutedEventArgs e)
        {
            //GetInpalletList();
            // DataTable dtTempCopy = _dtTemp.Copy();
            DataTable dt = ((DataView)dgTop.ItemsSource).Table;
            _dtTemp = dt.Clone();

            var chkRows = (from t in dt.AsEnumerable()
                           where t.Field<bool>("CHK") == true
                           select t).ToList();  //체크된 dgtop rows

            var unChkRows = (from t in dt.AsEnumerable()
                             where t.Field<bool>("CHK") == false
                             select t).ToList();  //언체크된 dgtop rows
            foreach (DataRow dr in dt.AsEnumerable())
            {
                dr["WIPQTY2"] = dr["WIPQTY"];
            }

            foreach (DataRow dr in chkRows)
            {
                _dtTemp.ImportRow(dr);
                dr["WIPQTY2"] = 0;
            }
            dgBot.ItemsSource = DataTableConverter.Convert(_dtTemp);
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                dt.Rows[i].SetField("CHK", false);
                dt.AcceptChanges();
                dgTop.Refresh(false, false);
            }
            //int topQty, botQty, totalQty = 0;
            ////bool isPass = true;
            //DataTable dtTempCopy = _dtTemp.Copy();
            //DataTable dt = ((DataView)dgTop.ItemsSource).Table;
            //var chkRows = (from t in dt.AsEnumerable()
            //               where t.Field<bool>("CHK") == true
            //               select t).ToList();  //체크된 dgtop rows

            //foreach (DataRow item in chkRows)
            //{
            //    foreach (DataRow dr in dtTempCopy.AsEnumerable())
            //    {
            //        if (item["PALLETID"] == dr["PALLETID"])
            //        {
            //            topQty = Convert.ToInt32(item["WIPQTY2"]);
            //            botQty = Convert.ToInt32(dr["WIPQTY2"]);
            //            totalQty = Convert.ToInt32(item["WIPQTY"]);


            //            else if (topQty + botQty <= topQty)
            //            {
            //                botQty += topQty;
            //                dr["WIPQTY2"] = botQty;
            //                item["WIPQTY2"] = totalQty - botQty;
            //            }
            //            else
            //            {
            //                ControlsLibrary.MessageBox.Show("수량을 확인하세요.", "", "Caution", MessageBoxButton.OK, MessageBoxIcon.None, null);
            //                return;
            //            }
            //        }
            //    }
            //    //_dtTemp.ImportRow(item); //  dtToGo에 체크된 rows 담음
            //}

            //dtTempCopy.AcceptChanges();
            //_dtTemp = dtTempCopy;
            //dgBot.ItemsSource = DataTableConverter.Convert(_dtTemp);


        }

        private void btnUpwards_Click(object sender, RoutedEventArgs e)
        {

            try
            {
                dgBot.EndEdit(true);
                dgBot.EndEditRow(true);
                DataTable dtBot = ((DataView)dgBot.ItemsSource).Table;
                DataTable dtTop = ((DataView)dgTop.ItemsSource).Table;
                var chkRows = (from t in dtBot.AsEnumerable()
                               where t.Field<bool>("CHK") == true
                               select t).ToList();  //체크된 dgtop rows

                var unChkRows = (from t in dtBot.AsEnumerable()
                                 where t.Field<bool>("CHK") == false
                                 select t).ToList();  //언체크된 dgtop rows

                for (int i = dtBot.Rows.Count - 1; i >= 0; i--)
                {
                    if (dtBot.Rows[i].Field<bool>("CHK") == true)
                    {
                        for (int j = 0; j < dtTop.Rows.Count; j++)
                        {
                            if (dtBot.Rows[i].Field<string>("PALLETID").Equals(dtTop.Rows[j].Field<string>("PALLETID")))
                            {
                                dtTop.Rows[j].SetField<int>("WIPQTY2", dtTop.Rows[j].Field<int>("WIPQTY"));
                            }
                        }
                        dtBot.Rows.RemoveAt(i);
                    }
                }

            }
            catch (Exception ex)
            {

                Util.MessageException(ex);
            }

        }

        private void dgTopChoice_Unchecked(object sender, RoutedEventArgs e)
        {
            CheckBox cb = sender as CheckBox;
            if (cb?.DataContext == null) return;
            if (cb.IsChecked == null) return;

            DataRowView drv = cb.DataContext as DataRowView;

            drv["CHK"] = false;
        }

        private void dgBotChoice_Unchecked(object sender, RoutedEventArgs e)
        {
            CheckBox cb = sender as CheckBox;
            if (cb?.DataContext == null) return;
            if (cb.IsChecked == null) return;

            DataRowView drv = cb.DataContext as DataRowView;

            drv["CHK"] = false;
        }

        private void dgBot_CommittingRowEdit(object sender, DataGridEditingRowEventArgs e)
        {
            DataTable dtTop = ((DataView)dgTop.ItemsSource).Table;
            DataTable dtBot = ((DataView)dgBot.ItemsSource).Table;

            C1DataGrid dg = sender as C1DataGrid;
            if (e.Row.Index > -1)
            {
                if (dgBot.CurrentCell.Column.Name == "WIPQTY2")
                {
                    int botQty = dgBot.CurrentCell.Value.SafeToInt32();
                    string palletID = Util.NVC(dgBot.GetCell(e.Row.Index, dgBot.Columns["PALLETID"].Index).Value);
                    if (palletID != "")
                    {
                        foreach (DataRow dr in dtTop.AsEnumerable())
                        {
                            if (dr["PALLETID"].Equals(palletID))
                            {
                                if (botQty <= dr["WIPQTY"].SafeToInt32())
                                    dr["WIPQTY2"] = dr["WIPQTY"].SafeToInt32() - botQty;
                                else
                                {
                                    ControlsLibrary.MessageBox.Show("수량을 확인하세요.", "", "Caution", MessageBoxButton.OK, MessageBoxIcon.None, null);
                                    foreach (DataRow drBot in dtBot.AsEnumerable())
                                    {
                                        if (drBot["PALLETID"].Equals(palletID))
                                            drBot["WIPQTY2"] = dr["WIPQTY"].SafeToInt32() - dr["WIPQTY2"].SafeToInt32();
                                    }
                                    return;

                                }
                            }
                        }
                    }
                }
            }
        }
    }
}
