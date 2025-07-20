/*************************************************************************************
 Created Date : 2023.07.10
      Creator : 김선준
   Decription : 출고예약
--------------------------------------------------------------------------------------
 [Change History]
     Date         Author      CSR                    Description...
  2023.07.10      김선준 :                           Initial Created.
***************************************************************************************/
using C1.WPF;
using C1.WPF.DataGrid;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using System;
using System.Data;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace LGC.GMES.MES.PACK001
{
    public partial class PACK003_044_ISS_POPUP : C1Window, IWorkArea
    {
        #region #. Member Variable Lists...
        LGC.GMES.MES.PACK001.Class.MESSAGE_PARAM ms = new LGC.GMES.MES.PACK001.Class.MESSAGE_PARAM();
        Util util = new Util();
        DataTable dtMain = new DataTable();
        private string uEQPTID = string.Empty;
        private string uTYPE = "EMPTY";
        private string uGUBUN = "ISS";

        /// <summary>
        /// Frame과 상호작용하기 위한 객체
        /// </summary>
        public IFrameOperation FrameOperation
        {
            get;
            set;
        }
        #endregion

        #region #. Declaration & Constructor
        public PACK003_044_ISS_POPUP()
        {
            InitializeComponent();
        }
        private void C1Window_Initialized(object sender, EventArgs e)
        {            

        }
        private void C1Window_Loaded(object sender, RoutedEventArgs e)
        {
            //파라메터 등록
            object[] tmps = C1WindowExtension.GetParameters(this);
            uEQPTID = tmps[0].ToString();
            uTYPE =  tmps[1].ToString();
            uGUBUN =   tmps[2].ToString();
            if (uGUBUN.Equals("ISS"))
            {
                this.Header = ObjectDic.Instance.GetObjectName("공 TRAY 출고예약");
                this.btnSave.Content = ObjectDic.Instance.GetObjectName("출고예약");
            }
            else
            {
                this.Header = ObjectDic.Instance.GetObjectName("공 TRAY 출고예약취소");
                this.btnSave.Content = ObjectDic.Instance.GetObjectName("출고예약취소");
            }
            SearchRack();
        }
        #endregion

        #region GridSet
        C1.WPF.DataGrid.DataGridRowHeaderPresenter pre = new C1.WPF.DataGrid.DataGridRowHeaderPresenter()
        {
            Background = new SolidColorBrush(Colors.Transparent),
            MouseOverBrush = new SolidColorBrush(Colors.Transparent),
        };

        CheckBox chkAll = new CheckBox()
        {
            IsChecked = false,
            Background = new SolidColorBrush(Colors.Transparent),
            VerticalAlignment = System.Windows.VerticalAlignment.Center,
            HorizontalAlignment = System.Windows.HorizontalAlignment.Center
        };

        private void dgRackList_LoadedColumnHeaderPresenter(object sender, C1.WPF.DataGrid.DataGridColumnEventArgs e)
        {
            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                try
                {
                    if (!string.IsNullOrEmpty(e.Column.Name))
                    {
                        if (e.Column.Name.Equals("CHK"))
                        {
                            pre.Content = chkAll;

                            e.Column.HeaderPresenter.Content = pre;
                            chkAll.Checked -= new RoutedEventHandler(checkAll_Checked);
                            chkAll.Unchecked -= new RoutedEventHandler(checkAll_Unchecked);
                            chkAll.Checked += new RoutedEventHandler(checkAll_Checked);
                            chkAll.Unchecked += new RoutedEventHandler(checkAll_Unchecked);
                        }
                    }
                }
                catch (Exception ex)
                {

                }

            }));

        }

        private void checkAll_Checked(object sender, RoutedEventArgs e)
        { 
            string sOcapLot = string.Empty;

            if (dgRackList.ItemsSource == null) return;

            DataTable dt = ((DataView)dgRackList.ItemsSource).Table;

            foreach (DataRow row in dt.Rows)
            {
                if ((uGUBUN.Equals("ISS")    && Util.NVC(row["RACK_STAT_CODE"]).Equals("USING")) ||
                    (uGUBUN.Equals("CANCEL") && Util.NVC(row["RACK_STAT_CODE"]).Equals("ISS_RESERVE")))
                {
                    row["CHK"] = true;
                }
                else
                {
                    row["CHK"] = false;
                }
            }

            dt.AcceptChanges(); 
        }

        private void checkAll_Unchecked(object sender, RoutedEventArgs e)
        {
            if (dgRackList.ItemsSource == null) return;

            DataTable dt = ((DataView)dgRackList.ItemsSource).Table;

            dt.Select("CHK = 1").ToList<DataRow>().ForEach(r => r["CHK"] = 0);

            dt.AcceptChanges(); 
        }
         
        private void dgRackList_LoadedCellPresenter(object sender, C1.WPF.DataGrid.DataGridCellEventArgs e)
        {
            C1.WPF.DataGrid.C1DataGrid dataGrid = sender as C1.WPF.DataGrid.C1DataGrid;

            dataGrid.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (e.Cell.Presenter == null)
                {
                    return;
                }

                //진행중인 색 변경
                if (e.Cell.Column.Name.Equals("RACK_STAT_CODE"))
                {
                    string sCheck = Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "RACK_STAT_CODE"));
                    if ((uGUBUN.Equals("ISS") && sCheck.Equals("USING")) ||
                        (uGUBUN.Equals("CANCEL") && sCheck.Equals("ISS_RESERVE")))
                    {
                        foreach (C1.WPF.DataGrid.DataGridColumn dc in dataGrid.Columns)
                        {
                            if (dc.Visibility == Visibility.Visible)
                            {
                                if (dataGrid.GetCell(e.Cell.Row.Index, dc.Index).Presenter != null)
                                    dataGrid.GetCell(e.Cell.Row.Index, dc.Index).Presenter.Background = new SolidColorBrush(Colors.LightGray);
                            }
                        }


                        CheckBox cb = dataGrid.GetCell(e.Cell.Row.Index, 0).Presenter.Content as CheckBox;
                        cb.Visibility = Visibility.Hidden;
                    }
                }
            }));
        }
        #endregion //GridSet 

        #region 조회
        private void btnClear_Click(object sender, RoutedEventArgs e)
        {
            SearchRack();
        }

        /// <summary>
        /// Rack조회
        /// </summary>
        private void SearchRack()
        {
            if (string.IsNullOrEmpty(uEQPTID) || string.IsNullOrEmpty(uTYPE)) return;
            try
            {
                #region 변수선언
                DataSet dsResult;
                DataSet dsInput = new DataSet();
 
                DataTable INDATA = new DataTable();
                INDATA.TableName = "INDATA";
                INDATA.Columns.Add("LANGID", typeof(string));
                INDATA.Columns.Add("EQPTID", typeof(string));
                INDATA.Columns.Add("ZONE_ID", typeof(string));
                INDATA.Columns.Add("RACK_STAT_CODE", typeof(string)); 
                #endregion //변수선언

                DataRow dr = INDATA.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["EQPTID"] = uEQPTID;
                dr["ZONE_ID"] = uTYPE.Equals("EMPTY") ? "E" : "F";
                if (uGUBUN.Equals("ISS"))
                {
                    dr["RACK_STAT_CODE"] = "USING";
                }
                else
                {
                    dr["RACK_STAT_CODE"] = "ISS_RESERVE";
                }

                INDATA.Rows.Add(dr);
                dsInput.Tables.Add(INDATA);

                dsResult = new ClientProxy().ExecuteServiceSync_Multi("BR_PRD_SEL_PACK_STK_RACK_ISS", "INDATA", "OUTDATA", dsInput, null);
                Util.GridSetData(this.dgRackList, dsResult.Tables["OUTDATA"], FrameOperation, true);
            }
            catch (Exception ex)
            { 
                Util.MessageException(ex);
            }
            finally
            {
            }            
        }
        #endregion

        #region 저장
        /// <summary>
        /// 저장
        /// </summary>
        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            if (null == this.dgRackList.ItemsSource || this.dgRackList.Rows.Count == 0) return;
            DataTable dt = DataTableConverter.Convert(this.dgRackList.ItemsSource);

            int iCnt = dt.AsEnumerable().Where(x => x.Field<bool>("CHK") == true).Count();
            if (iCnt == 0) return;

            LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("SFU1925"), null, "Information", MessageBoxButton.OKCancel, MessageBoxIcon.None, (result) =>
            {
                if (result == MessageBoxResult.OK)
                {
                    Save();
                }
            });
        }

        private void Save()
        {
            try
            {
                DataTable dt = DataTableConverter.Convert(this.dgRackList.ItemsSource);
                var query = dt.AsEnumerable().Where(x => x.Field<bool>("CHK") == true).Select(x => x.Field<string>("RACK_ID"));
                if (null == query || query.Count() == 0) return;

                #region 변수선언 
                DataSet dsInput = new DataSet();

                DataTable INDATA = new DataTable();
                INDATA.TableName = "INDATA";
                INDATA.Columns.Add("SRCTYPE", typeof(string));
                INDATA.Columns.Add("LANGID", typeof(string));
                INDATA.Columns.Add("EQPTID", typeof(string));
                INDATA.Columns.Add("GUBUN", typeof(string));
                INDATA.Columns.Add("USERID", typeof(string));

                DataRow dr = INDATA.NewRow();
                dr["SRCTYPE"] = "UI";
                dr["LANGID"] = LoginInfo.LANGID;
                dr["EQPTID"] = uEQPTID;
                dr["GUBUN"] = uGUBUN;
                dr["USERID"] = LoginInfo.USERID;

                INDATA.Rows.Add(dr);
                dsInput.Tables.Add(INDATA);

                DataTable IN_RACK = new DataTable();
                IN_RACK.TableName = "IN_RACK";
                IN_RACK.Columns.Add("RACK_ID", typeof(string));
                foreach (string item in query)
                {
                    DataRow dr1 = IN_RACK.NewRow();
                    dr1["RACK_ID"] = item;
                    IN_RACK.Rows.Add(dr1);
                }
                dsInput.Tables.Add(IN_RACK);
                #endregion //변수선언

                ShowLoadingIndicator();
                DataSet dsRslt = new ClientProxy().ExecuteServiceSync_Multi("BR_PRD_UPD_PACK_STK_RACK_INFO", "INDATA,IN_RACK", "OUTDATA", dsInput);
                HiddenLoadingIndicator();
                if (dsRslt.Tables["OUTDATA"].Rows.Count > 0)
                {
                    string[] parm = new string[2];
                    parm[0] = IN_RACK.Rows.Count.ToString();
                    parm[1] = dsRslt.Tables["OUTDATA"].Rows[0]["WORK_CNT"].ToString();
                    if (dsRslt.Tables["OUTDATA"].Rows[0]["WORK_CNT"].ToString().Equals("0"))
                    {
                        Util.MessageValidation("SFU8221", parm);     //건중 0건 처리되었습니다.
                    }
                    else
                    {
                        Util.MessageValidation("SFU8221", parm);     //건중 건 처리되었습니다.
                        this.DialogResult = MessageBoxResult.OK;
                        this.Close();
                    }
                }
            }
            catch (Exception ex)
            { 
                Util.MessageException(ex);
            }
        }
        //추가
        private void ShowLoadingIndicator()
        {
            if (loadingIndicator != null)
                loadingIndicator.Visibility = Visibility.Visible;
        }

        private void HiddenLoadingIndicator()
        {
            if (loadingIndicator != null)
                loadingIndicator.Visibility = Visibility.Collapsed;
        }
        #endregion //저장
    }
}