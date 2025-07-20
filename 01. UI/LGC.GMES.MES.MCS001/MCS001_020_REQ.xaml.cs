/*************************************************************************************
 Created Date : 2018.12.27
      Creator : 오화백
   Decription : 자재공급요청
--------------------------------------------------------------------------------------
 [Change History]
  2018.12.27  오화백 : Initial Created.
**************************************************************************************/

using C1.WPF;
using C1.WPF.DataGrid;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.CMM001.Extensions;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using System;
using System.Collections.Generic;
using System.Data;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;


namespace LGC.GMES.MES.MCS001
{
     public partial class MCS001_020_REQ : C1Window, IWorkArea
    {
        #region < Declaration & Constructor >
        private string _EQPTID = string.Empty;
        private bool _load = true;

        public IFrameOperation FrameOperation
        {
            get;
            set;
        }

        #endregion < Declaration & Constructor >
        
        #region < Initialize >

        public MCS001_020_REQ()
        {
            InitializeComponent();
        }

        private void C1Window_Loaded(object sender, RoutedEventArgs e)
        {
            if (_load)
            {
                ApplyPermissions();
                //파라미터 셋팅
                object[] tmps = C1WindowExtension.GetParameters(this);
                if (tmps == null)
                    return;
                _EQPTID = Util.NVC(tmps[0]);
                txtEqptName.Text = Util.NVC(tmps[1]);
                GetMtrlInfo();
                _load = false;
            }
        }
        /// <summary>
        /// 권한
        /// </summary>
        private void ApplyPermissions()
        {
            List<Button> listAuth = new List<Button> { btnMtrlReq };
            Util.pageAuth(listAuth, FrameOperation.AUTHORITY);
        }

        #endregion < Initialize >
        
        #region < Event >        

        #region 팝업닫기 : btnClose_Click()    
        /// <summary>
        /// 닫기
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = MessageBoxResult.Cancel;
        }

        #endregion

        #region 자재요청 : btnMtrlReq_Click()      
        private void btnMtrlReq_Click(object sender, RoutedEventArgs e)
        {
            if (!Validation()) return;

            //요청하시겠습니까
            LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(
                    "SFU2924"), null, "Information", MessageBoxButton.OKCancel, MessageBoxIcon.None, (result) =>
                    {
                        if (result == MessageBoxResult.OK)
                        {
                            Request();

                        }
                    });

        }

        #endregion

        #region 스프레드 이벤트 : dgMtrlList_CurrentCellChanged(), dgMtrlList_LoadedCellPresenter()   

        /// <summary>
        ///  Cell change
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void dgMtrlList_CurrentCellChanged(object sender, DataGridCellEventArgs e)
        {
            if (sender == null || e.Cell == null)
                return;

            if (e.Cell.Row.Type == DataGridRowType.Item)
            {
                if (e.Cell.Column.Name.Equals("REQ_QTY") && e.Cell.IsEditable == true)
                {
                    DataTableConverter.SetValue(e.Cell.Row.DataItem, "CHK", true);
                }
            }

        }
        /// <summary>
        ///  LoadedCellPresente
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void dgMtrlList_LoadedCellPresenter(object sender, DataGridCellEventArgs e)
        {
            if (sender == null)
                return;

            C1DataGrid dg = sender as C1DataGrid;

            dg?.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (e.Cell.Presenter == null)
                {
                    return;
                }

                if (e.Cell.Row.Type == DataGridRowType.Item)
                {
                    if (e.Cell.Column.Name.Equals("REQ_QTY"))
                    {
                        e.Cell.Presenter.Background = new SolidColorBrush(Colors.Yellow);
                    }
                }
                //자재ID 셋팅
                if (e.Cell.Presenter == null || e.Cell.Presenter.Content == null) return;

                if (e.Cell.Presenter.Content.GetType() != typeof(StackPanel)) return;
                StackPanel panel = e.Cell.Presenter.Content as StackPanel;

                for (int cnt = 0; cnt < panel.Children.Count; cnt++)
                {
                   
                    if (panel.Children[cnt].GetType().Name == "TextBlock")
                    {
                        TextBlock n = panel.Children[cnt] as TextBlock;
                        if (Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "MTRL_TYPE")).Equals("PET"))
                        {
                            n.Visibility = Visibility.Collapsed;
                        }
                        else
                        {
                            n.Visibility = Visibility.Visible;
                            n.Text = Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "MTRLNAME"));
                        }

                    }
                    else if (panel.Children[cnt].GetType().Name == "ComboBox")
                    {
                        ComboBox n = panel.Children[cnt] as ComboBox;
                        n.ItemsSource = MtrlName().Copy().AsDataView();
                        n.SelectedIndex = 0;
                        if (Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "MTRL_TYPE")).Equals("PET"))
                        {
                            n.Visibility = Visibility.Visible;
                        }
                        else
                        {
                            n.Visibility = Visibility.Collapsed;
                        }
                    }
                }
                



            }));
        }


        #endregion

        #endregion < Event >

        #region < Method >
    
        #region 자재조회 : GetMtrlInfo()
        /// <summary>
        /// 자재조회
        /// </summary>
        private void GetMtrlInfo()
        {
            try
            {
                ShowLoadingIndicator();
                DataSet indataSet = new DataSet();
                DataTable inDataTable = indataSet.Tables.Add("INDATA");
                inDataTable.Columns.Add("LANGID");
                inDataTable.Columns.Add("PROCID");
                inDataTable.Columns.Add("EQPTID");

                DataRow newRow = inDataTable.NewRow();
                newRow["LANGID"] = LoginInfo.LANGID;
                newRow["PROCID"] = LoginInfo.CFG_PROC_ID;
                newRow["EQPTID"] = _EQPTID;
                inDataTable.Rows.Add(newRow);
                new ClientProxy().ExecuteService_Multi("BR_MCS_GET_MTRL_INFO_BY_EQPTID_CELL", "INDATA", "OUTDATA,OUT_WOID", (bizResult, bizException) =>
                {
                    try
                    {
                        if (bizException != null)
                        {
                            Util.MessageException(bizException);
                            return;
                        }
                        if (bizResult.Tables.Contains("OUTDATA"))
                        {
                            DataTable dtInfo = bizResult.Tables["OUTDATA"];
                            Util.GridSetData(dgMtrlList, dtInfo, FrameOperation, true);
                        }
                        if (bizResult.Tables.Contains("OUT_WOID"))
                        {
                            DataTable dtWOInfo = bizResult.Tables["OUT_WOID"];
                            txtWo.Text = dtWOInfo.Rows[0]["WO_DETL_ID"].ToString();
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

                }, indataSet);
            }
            catch (Exception ex)
            {
                HiddenLoadingIndicator();
                Util.MessageException(ex);
            }
        }
        #endregion

        #region 자재요청 : Request()
        /// <summary>
        /// 자재요청
        /// </summary>
        private void Request()
        {
            try
            {
                ShowLoadingIndicator();
                foreach (DataRow row in ((System.Data.DataView)dgMtrlList.ItemsSource).Table.Rows)
                {
                    if (row["CHK"].ToString() == "True")
                    {
                        DataSet inData = new DataSet();

                        //INMLOT
                        DataTable inMlot = inData.Tables.Add("INMLOT");
                        inMlot.Columns.Add("MLOTID", typeof(string));
                        DataRow newrow = null;

                        newrow = inMlot.NewRow();
                        newrow["MLOTID"] = null;
                        inMlot.Rows.Add(newrow);

                        //INDATA
                        DataTable inDataTable = inData.Tables.Add("INDATA");
                        inDataTable.Columns.Add("EQPTID", typeof(string));
                        inDataTable.Columns.Add("WO_DETL_ID", typeof(string));
                        inDataTable.Columns.Add("MTRLID", typeof(string));
                        inDataTable.Columns.Add("MTRL_SPLY_REQ_QTY", typeof(string));
                        inDataTable.Columns.Add("MTRL_ELTR_TYPE_CODE", typeof(string));
                        inDataTable.Columns.Add("UPDUSER", typeof(string));

                        newrow = inDataTable.NewRow();
                        newrow["EQPTID"] = _EQPTID.ToString();
                        newrow["WO_DETL_ID"] = txtWo.Text;
                        newrow["MTRLID"] = row["MTRLID"].ToString();
                        newrow["MTRL_SPLY_REQ_QTY"] = Convert.ToDecimal(row["REQ_QTY"]).ToString();
                        newrow["MTRL_ELTR_TYPE_CODE"] = null;
                        newrow["UPDUSER"] = LoginInfo.USERID;
                        inDataTable.Rows.Add(newrow);

                        //TYPE
                        DataTable inType = inData.Tables.Add("INTYPE");
                        inType.Columns.Add("SPLY_TYPE_CODE", typeof(string));

                        newrow = inType.NewRow();
                        newrow["SPLY_TYPE_CODE"] = "REQ";
                        inType.Rows.Add(newrow);
                         //자재요청
                        new ClientProxy().ExecuteService_Multi("BR_MCS_REG_MTRL_SPLY", "INMLOT,INDATA,INTYPE", "OUTDATA", (Result, ex) =>
                        {
                            if (ex != null)
                            {
                                HiddenLoadingIndicator();
                                Util.MessageException(ex);
                                return;
                            }
                        }, inData);
                    }
                }
            }
            catch(Exception ex)
            {
                HiddenLoadingIndicator();
                Util.AlertByBiz("BR_MCS_REG_MTRL_SPLY", ex.Message, ex.ToString());
            }
            finally
            {
                HiddenLoadingIndicator();
                this.DialogResult = MessageBoxResult.OK;
            }
          
            
        }
        #endregion

        #region Valldation : Validation()    
        private bool Validation()
        {


            if (dgMtrlList.Rows.Count == 0)
            {
                Util.MessageValidation("SFU3537"); // 조회된 데이터가 없습니다
                return false;
            }

            DataRow[] drSelect = DataTableConverter.Convert(dgMtrlList.ItemsSource).Select("CHK = 'True'");
            //DataTable dt = DataTableConverter.Convert(dgMtrlList.ItemsSource);

            int CheckCount = 0;
            int CheckQty = 0;
            int CheckMtrlQty = 0;

            for (int i = 0; i < dgMtrlList.Rows.Count; i++)
            {
                if (Util.NVC(DataTableConverter.GetValue(dgMtrlList.Rows[i].DataItem, "CHK")) == "True")
                {
                    CheckCount = CheckCount + 1;
                }


                if (Util.NVC(DataTableConverter.GetValue(dgMtrlList.Rows[i].DataItem, "CHK")) == "True" && Util.NVC(DataTableConverter.GetValue(dgMtrlList.Rows[i].DataItem, "REQ_QTY")) == "0")
                {
                    CheckQty = CheckQty + 1;
                }

                if (Util.NVC(DataTableConverter.GetValue(dgMtrlList.Rows[i].DataItem, "CHK")) == "True" && Util.NVC(DataTableConverter.GetValue(dgMtrlList.Rows[i].DataItem, "MTRLID")) == "")
                {
                    CheckMtrlQty = CheckMtrlQty + 1;
                }
            }

            if (CheckCount == 0)
            {
                Util.MessageValidation("SFU1651");//선택된 항목이 없습니다.
                return false;
            }
            //if (CheckCount > 1)
            //{
            //    Util.MessageValidation("SFU4159");//한건만 선택하세요.
            //    return false;
            //}
            
            if (CheckQty > 0)
            {
                Util.MessageValidation("SFU4135");//요청수량을 입력 하세요
                return false;
            }
            if (CheckMtrlQty > 0)
            {
                Util.MessageValidation("SFU1828");//자재를 선택하세요.
                return false;
            }
            return true;
        }


        #endregion
        
        #region LoadingIndicator : ShowLoadingIndicator(), HiddenLoadingIndicator()

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


        #endregion

        #endregion < Method >


        private DataTable MtrlName()
        {
            //동별 공통코드
            DataTable RQSTDT = new DataTable();
            RQSTDT.TableName = "RQSTDT";
            RQSTDT.Columns.Add("LANGID", typeof(string));
            RQSTDT.Columns.Add("AREAID", typeof(string));
            RQSTDT.Columns.Add("COM_TYPE_CODE", typeof(string));

            DataRow dr = RQSTDT.NewRow();
            dr["LANGID"] = LoginInfo.LANGID;
            dr["AREAID"] = LoginInfo.CFG_AREA_ID;
            dr["COM_TYPE_CODE"] = "PET_MTRLID";
            RQSTDT.Rows.Add(dr);

            DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_MCS_SEL_AREA_COMCODE", "RQSTDT", "RSLTDT", RQSTDT);
         
            DataRow dataRow = dtResult.NewRow();
            dataRow["CBO_CODE"] = string.Empty;
            dataRow["CBO_NAME"] = "-SELECT-";
            dtResult.Rows.InsertAt(dataRow, 0);

            return dtResult;
        }

        private void cbMtrl_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ComboBox combox = sender as ComboBox;
            if (sender == null) return;
             dgMtrlList.SelectedItem = ((FrameworkElement)(sender)).DataContext;
            if (Util.NVC(DataTableConverter.GetValue(dgMtrlList.SelectedItem, "MTRL_TYPE")).Equals("PET"))
            {
              DataTableConverter.SetValue(dgMtrlList.SelectedItem, "MTRLID", combox.SelectedValue);
            }
       }
    }
}