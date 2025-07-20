/*************************************************************************************
 Created Date : 2022.12.06
      Creator : 
   Decription : 설비 호기별 작업 LOT 설정
--------------------------------------------------------------------------------------
 [Change History]
  2022.12.06  DEVELOPER : Initial Created.
 
**************************************************************************************/
using C1.WPF;
using C1.WPF.DataGrid;
using C1.WPF.DataGrid.Summaries;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using System;
using System.Collections.Generic;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using LGC.GMES.MES.CMM001;

namespace LGC.GMES.MES.FCS002
{
    public partial class FCS002_210 : UserControl, IWorkArea
    {
        #region Declaration & Constructor 

        Util _Util = new Util();
        private string _CELLTYPE = string.Empty;

        public FCS002_210()
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

        //화면내 combo 셋팅
        private void InitCombo()
        {
            CommonCombo_Form_MB _combo = new CommonCombo_Form_MB();

            //C1ComboBox[] cboLaneChild = { cboLane };
            string[] sFilter = { "D,J,5" };   //EQPT_GR_TYPE_CODE 참고. (D:DeGas,J:Jig Formation,5:EOL) -> Taping에 대한 정의 아직 없음
            //_combo.SetCombo(cboLane, CommonCombo_Form_MB.ComboStatus.SELECT, sCase: "LANE_MB", sFilter: sFilter);
            _combo.SetCombo(cboLane, CommonCombo_Form_MB.ComboStatus.ALL, sCase: "LANE_MB", sFilter: sFilter);

            SetGridCboItem(dgList.Columns["ISS_PRHB_FLAG"], "USE_FLAG");
            SetGridCboItem(dgList.Columns["SPCL_LOT_ISS_PRHB_FLAG"], "USE_FLAG");
            SetGridCboItem(dgList.Columns["STK_ISS_PRHB_FLAG"], "USE_FLAG");
            SetGridCboItem(dgList.Columns["STK_SPCL_LOT_ISS_PRHB_FLAG"], "USE_FLAG");
        }
        #endregion

        #region Event
        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                SetForm();

                InitCombo();

                GetList();
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            GetList();
        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                try
                {
                    //저장하시겠습니까?
                    Util.MessageConfirm("FM_ME_0214", (re) =>
                    {
                        if (re != MessageBoxResult.OK)
                        {
                            return;
                        }

                        int sum = 0;
                        for (int i = 2; i < dgList.GetRowCount() + 2; i++)
                        {
                            if (Util.NVC(DataTableConverter.GetValue(dgList.Rows[i].DataItem, "CHK")).Equals("True"))
                                sum++;
                        }

                        if (sum < 1)
                        {
                            //선택된 항목이 없습니다.
                            Util.MessageInfo("SFU1651");
                            return;
                        }
                        
                        DataTable dtRqst = new DataTable();
                        dtRqst.TableName = "INDATA";
                        dtRqst.Columns.Add("EQPTID", typeof(string));
                        dtRqst.Columns.Add("PRE_PROC_GR_CODE", typeof(string));
                        dtRqst.Columns.Add("NEXT_PROC_GR_CODE", typeof(string));
                        dtRqst.Columns.Add("DAY_GR_LOTID", typeof(string));
                        dtRqst.Columns.Add("PKG_LOTID", typeof(string));
                        dtRqst.Columns.Add("ROUTID", typeof(string));
                        dtRqst.Columns.Add("ISS_PRHB_FLAG", typeof(string));
                        dtRqst.Columns.Add("SPCL_LOT_ISS_PRHB_FLAG", typeof(string));
                        dtRqst.Columns.Add("MAX_BUF_QTY", typeof(string));
                        dtRqst.Columns.Add("STK_DAY_GR_LOTID", typeof(string));
                        dtRqst.Columns.Add("STK_PKG_LOTID", typeof(string));
                        dtRqst.Columns.Add("STK_ROUTID", typeof(string));
                        dtRqst.Columns.Add("STK_ISS_PRHB_FLAG", typeof(string));
                        dtRqst.Columns.Add("STK_SPCL_LOT_ISS_PRHB_FLAG", typeof(string));
                        dtRqst.Columns.Add("STK_MAX_BUF_QTY", typeof(string));
                        dtRqst.Columns.Add("USERID", typeof(string));
                        dtRqst.Columns.Add("ALL_FLAG", typeof(string));

                        for (int i = 2; i < dgList.GetRowCount() + 2; i++)
                        {
                            if (Util.NVC(DataTableConverter.GetValue(dgList.Rows[i].DataItem, "CHK")).Equals("True"))
                            {
                                DataRow dr = dtRqst.NewRow();
                                dr["EQPTID"] = Util.NVC(DataTableConverter.GetValue(dgList.Rows[i].DataItem, "EQPTID"));
                                dr["PRE_PROC_GR_CODE"] = Util.NVC(DataTableConverter.GetValue(dgList.Rows[i].DataItem, "PRE_PROC_GR_CODE"));
                                dr["NEXT_PROC_GR_CODE"] = Util.NVC(DataTableConverter.GetValue(dgList.Rows[i].DataItem, "NEXT_PROC_GR_CODE"));
                                dr["DAY_GR_LOTID"] = Util.NVC(DataTableConverter.GetValue(dgList.Rows[i].DataItem, "DAY_GR_LOTID"));
                                if (string.IsNullOrEmpty(Util.NVC(DataTableConverter.GetValue(dgList.Rows[i].DataItem, "PKG_LOTID"))))
                                {
                                    dr["PKG_LOTID"] = "ALL";
                                    dr["ALL_FLAG"] = "Y";
                                }
                                else
                                {
                                    dr["PKG_LOTID"] = Util.NVC(DataTableConverter.GetValue(dgList.Rows[i].DataItem, "PKG_LOTID"));
                                    dr["ALL_FLAG"] = "N";
                                }
                                dr["ROUTID"] = Util.NVC(DataTableConverter.GetValue(dgList.Rows[i].DataItem, "ROUTID"));
                                dr["ISS_PRHB_FLAG"] = Util.NVC(DataTableConverter.GetValue(dgList.Rows[i].DataItem, "ISS_PRHB_FLAG"));
                                dr["SPCL_LOT_ISS_PRHB_FLAG"] = Util.NVC(DataTableConverter.GetValue(dgList.Rows[i].DataItem, "SPCL_LOT_ISS_PRHB_FLAG"));
                                dr["MAX_BUF_QTY"] = Util.NVC(DataTableConverter.GetValue(dgList.Rows[i].DataItem, "MAX_BUF_QTY"));
                                dr["STK_DAY_GR_LOTID"] = Util.NVC(DataTableConverter.GetValue(dgList.Rows[i].DataItem, "STK_DAY_GR_LOTID"));
                                dr["STK_PKG_LOTID"] = Util.NVC(DataTableConverter.GetValue(dgList.Rows[i].DataItem, "STK_PKG_LOTID"));
                                dr["STK_ROUTID"] = Util.NVC(DataTableConverter.GetValue(dgList.Rows[i].DataItem, "STK_ROUTID"));
                                dr["STK_ISS_PRHB_FLAG"] = Util.NVC(DataTableConverter.GetValue(dgList.Rows[i].DataItem, "STK_ISS_PRHB_FLAG"));
                                dr["STK_SPCL_LOT_ISS_PRHB_FLAG"] = Util.NVC(DataTableConverter.GetValue(dgList.Rows[i].DataItem, "STK_SPCL_LOT_ISS_PRHB_FLAG"));
                                dr["STK_MAX_BUF_QTY"] = Util.NVC(DataTableConverter.GetValue(dgList.Rows[i].DataItem, "STK_MAX_BUF_QTY"));
                                dr["USERID"] = LoginInfo.USERID;
                                dtRqst.Rows.Add(dr);
                            }
                        }

                        ShowLoadingIndicator();
                        new ClientProxy().ExecuteService("BR_SET_EQPT_EOL_WRKLOT_MB", "INDATA", null, dtRqst, (bizResult, bizException) =>
                        {
                            try
                            {
                                if (bizException != null)
                                {
                                    Util.MessageException(bizException);
                                    return;
                                }

                                //저장하였습니다.
                                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("FM_ME_0215"), null, "Information", MessageBoxButton.OK, MessageBoxIcon.None, (result) =>
                                {
                                    if (result == MessageBoxResult.OK)
                                    {
                                        GetList();
                                    }
                                });
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
                        });
                    });

                }
                catch (Exception ex)
                {
                    Util.MessageException(ex);
                }
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
            finally
            {
                HiddenLoadingIndicator();
            }           
        }

        private void btnLotSearch_Click(object sender, RoutedEventArgs e)
        {
            FCS002_210_Search LotSearch = new FCS002_210_Search();
            LotSearch.FrameOperation = FrameOperation;

            int index = ((C1.WPF.DataGrid.DataGridCellPresenter)((sender as Button).Parent)).Row.Index;

            object[] parameters = new object[5];
            //parameters[0] = Util.GetCondition(cboLane, bAllNull: true);
            parameters[0] = (DataTableConverter.Convert(dgList.ItemsSource)).Rows[index - 2]["EQSGID"].ToString();
            parameters[1] = index.ToString();
            parameters[2] = "AGING";
            parameters[3] = (DataTableConverter.Convert(dgList.ItemsSource)).Rows[index - 2]["PRE_PROC_GR_CODE"].ToString();
            parameters[4] = (DataTableConverter.Convert(dgList.ItemsSource)).Rows[index - 2]["NEXT_PROC_GR_CODE"].ToString();


            C1WindowExtension.SetParameters(LotSearch, parameters);
            LotSearch.Closed += new EventHandler(Lot_Search_Closed);

            this.Dispatcher.BeginInvoke(new Action(() => LotSearch.ShowModal()));
            //LotSearch.BringToFront();
            return;
        }

        private void btnStkSearch_Click(object sender, RoutedEventArgs e)
        {
            FCS002_210_Search LotSearch = new FCS002_210_Search();
            LotSearch.FrameOperation = FrameOperation;

            int index = ((C1.WPF.DataGrid.DataGridCellPresenter)((sender as Button).Parent)).Row.Index;

            object[] parameters = new object[5];
            //parameters[0] = Util.GetCondition(cboLane, bAllNull: true);
            parameters[0] = (DataTableConverter.Convert(dgList.ItemsSource)).Rows[index - 2]["EQSGID"].ToString();
            parameters[1] = index.ToString();
            parameters[2] = "STK";
            parameters[3] = (DataTableConverter.Convert(dgList.ItemsSource)).Rows[index - 2]["PRE_PROC_GR_CODE"].ToString();
            parameters[4] = (DataTableConverter.Convert(dgList.ItemsSource)).Rows[index - 2]["NEXT_PROC_GR_CODE"].ToString();

            C1WindowExtension.SetParameters(LotSearch, parameters);
            LotSearch.Closed += new EventHandler(STK_Lot_Search_Closed);

            this.Dispatcher.BeginInvoke(new Action(() => LotSearch.ShowModal()));
            //LotSearch.BringToFront();
            return;
        }

        private void dgList_LoadedRowHeaderPresenter(object sender, C1.WPF.DataGrid.DataGridRowEventArgs e)
        {
            if (e.Row.HeaderPresenter == null)
            {
                return;
            }

            e.Row.HeaderPresenter.Content = null;
            C1DataGrid dataGrid = e.Row.DataGrid as C1DataGrid;
            TextBlock tb = new TextBlock();

            if (e.Row.Index - (dataGrid.TopRows.Count - 1) > 0 && e.Row.Index <= dataGrid.Rows.Count - 1)
            {
                tb.Text = (e.Row.Index + 1 - dataGrid.TopRows.Count).ToString();
                tb.VerticalAlignment = VerticalAlignment.Center;
                tb.HorizontalAlignment = HorizontalAlignment.Center;
                e.Row.HeaderPresenter.Content = tb;
            }
        }
        #endregion

        #region Method
        private bool SetGridCboItem(C1.WPF.DataGrid.DataGridColumn col, string sClassId, bool bCodeDisplay = false, string sCmnCd = null)
        {
            try
            {
                bool rtn = false;

                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("CMCDTYPE", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["CMCDTYPE"] = sClassId;

                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_COMBO_CMN_FORM", "RQSTDT", "RSLTDT", RQSTDT);

                if (dtResult == null || dtResult.Rows.Count == 0)
                {
                    rtn = false;
                }
                else
                {
                    rtn = true;
                    (col as C1.WPF.DataGrid.DataGridComboBoxColumn).DisplayMemberPath = "CBO_NAME";
                    (col as C1.WPF.DataGrid.DataGridComboBoxColumn).SelectedValuePath = "CBO_CODE";
                    (col as C1.WPF.DataGrid.DataGridComboBoxColumn).ItemsSource = DataTableConverter.Convert(dtResult);
                }

                return rtn;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        
        private void GetList()
        {
            try
            {
                DataTable inDataTable = new DataTable();
                inDataTable.Columns.Add("LANGID", typeof(string));
                inDataTable.Columns.Add("LANEID", typeof(string));
                inDataTable.Columns.Add("AREAID", typeof(string));

                DataRow newRow = inDataTable.NewRow();
                newRow["LANGID"] = LoginInfo.LANGID;
                newRow["AREAID"] = LoginInfo.CFG_AREA_ID;

                if (cboLane.SelectedIndex > 0)
                    newRow["LANEID"] = Util.GetCondition(cboLane);

                //newRow["LANEID"] = Util.GetCondition(cboLane, sMsg: "SFU4986");

                inDataTable.Rows.Add(newRow);

                ShowLoadingIndicator();
                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_SEL_EQP_OP_LOT_MB", "INDATA", "OUTDATA", inDataTable);
                dgList.ItemsSource = DataTableConverter.Convert(dtRslt);
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
            finally
            {
                HiddenLoadingIndicator();
            }
        }

        private void Lot_Search_Closed(object sender, EventArgs e)
        {
            FCS002_210_Search window = sender as FCS002_210_Search;

            window.Closed -= new EventHandler(Lot_Search_Closed);

            int index = 0;
            if (window.DialogResult == MessageBoxResult.OK)
            {
                index = int.Parse(window.INDEX);

                DataTableConverter.SetValue(dgList.Rows[index].DataItem, "DAY_GR_LOTID", window.LOT);
                DataTableConverter.SetValue(dgList.Rows[index].DataItem, "PKG_LOTID", window.ASSYLOT);
                DataTableConverter.SetValue(dgList.Rows[index].DataItem, "ROUTID", window.ROUTID);
                DataTableConverter.SetValue(dgList.Rows[index].DataItem, "ROUTNAME", window.ROUTNAME);
                DataTableConverter.SetValue(dgList.Rows[index].DataItem, "PRE_PROC_GR_CODE", window.PREPROCGRCODE);
                DataTableConverter.SetValue(dgList.Rows[index].DataItem, "PRE_PROC_GR_NAME", window.PREPROCGRNAME);
                DataTableConverter.SetValue(dgList.Rows[index].DataItem, "NEXT_PROC_GR_CODE", window.NEXTPROCGRCODE);
                DataTableConverter.SetValue(dgList.Rows[index].DataItem, "NEXT_PROC_GR_NAME", window.NEXTPROCGRNAME);
            }
            //this.grdMain.Children.Remove(window);
        }

        private void STK_Lot_Search_Closed(object sender, EventArgs e)
        {
            FCS002_210_Search window = sender as FCS002_210_Search;

            window.Closed -= new EventHandler(STK_Lot_Search_Closed);

            int index = 0;
            if (window.DialogResult == MessageBoxResult.OK)
            {
                index = int.Parse(window.INDEX);

                DataTableConverter.SetValue(dgList.Rows[index].DataItem, "STK_DAY_GR_LOTID", window.LOT);
                DataTableConverter.SetValue(dgList.Rows[index].DataItem, "STK_PKG_LOTID", window.ASSYLOT);
                DataTableConverter.SetValue(dgList.Rows[index].DataItem, "STK_ROUTID", window.ROUTID);
                DataTableConverter.SetValue(dgList.Rows[index].DataItem, "STK_ROUTNAME", window.ROUTNAME);
            }
            //this.grdMain.Children.Remove(window);
        }

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


        private string GetCellType()
        {
            //DA_BAS_SEL_AREA_COM_CODE_USE

            DataTable dtRqst = new DataTable();
            dtRqst.TableName = "RQSTDT";
            dtRqst.Columns.Add("LANGID", typeof(string));
            dtRqst.Columns.Add("AREAID", typeof(string));
            dtRqst.Columns.Add("COM_TYPE_CODE", typeof(string));
            dtRqst.Columns.Add("COM_CODE", typeof(string));

            DataRow dr = dtRqst.NewRow();
            dr["LANGID"] = LoginInfo.LANGID;
            dr["AREAID"] = LoginInfo.CFG_AREA_ID;
            dr["COM_TYPE_CODE"] = "FORM_SPCL_FN_MB";
            dr["COM_CODE"] = "FORM_LOT_SET";
            dtRqst.Rows.Add(dr);

            DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_AREA_COM_CODE_USE", "RQSTDT", "RSLTDT", dtRqst);

            if (dtRslt.Rows.Count == 0)
                _CELLTYPE = "D";

            else
                _CELLTYPE = dtRslt.Rows[0]["ATTR1"].ToString();
            
            return _CELLTYPE;
        }

        private void SetForm()
        {  
            
           
            switch (GetCellType())
            { 
                // NFF 인경우
                case "N":
                    dgList.Columns["STK_ROUTID"].Visibility = Visibility.Collapsed;
                    dgList.Columns["STK_ROUTNAME"].Visibility = Visibility.Collapsed;
                    dgList.Columns["STK_DAY_GR_LOTID"].Visibility = Visibility.Collapsed;
                    dgList.Columns["STK_PKG_LOTID"].Visibility = Visibility.Collapsed;
                    dgList.Columns["STK_MAX_BUF_QTY"].Visibility = Visibility.Collapsed;
                    dgList.Columns["STK_ISS_PRHB_FLAG"].Visibility = Visibility.Collapsed;
                    dgList.Columns["STK_SPCL_LOT_ISS_PRHB_FLAG"].Visibility = Visibility.Collapsed; 
                    dgList.Columns["StkSearch"].Visibility = Visibility.Collapsed; 
                    break;
                default:
                    break;
            }

       
            
        }
        #endregion


    }
}