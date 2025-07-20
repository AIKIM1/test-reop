/*************************************************************************************
 Created Date : 2022.03.03
      Creator : 윤세진 선임
   Decription : 원통형 9, 10호, 수동포트 단 적재 기능 추가
--------------------------------------------------------------------------------------
 [Change History]
  2022.03.03  윤세진
  2024.01.29  오수현   저장 버튼시 IndexOutOfRange 오류수정
**************************************************************************************/


using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using C1.WPF;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.ControlsLibrary;
using System.Data;
using C1.WPF.DataGrid;
using ColorConverter = System.Windows.Media.ColorConverter;

namespace LGC.GMES.MES.CMM001
{
    /// <summary>
    /// CMM_ASSY_LAYER_STACK.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class CMM_ASSY_LAYER_STACK : C1Window, IWorkArea
    {
        #region Declaration & Constructor         
        private string rep_cstid = string.Empty;
        private string cststat = string.Empty;
        private string loadYN = string.Empty;
        private bool bViewMsg = false;

        private BizDataSet _Biz = new BizDataSet();
        private Util _Util = new Util();
        #endregion

        #region Initialize 
        /// <summary>
        ///  Frame과 상호작용하기 위한 객체
        /// </summary>
        public IFrameOperation FrameOperation
        {
            get;
            set;
        }
        public CMM_ASSY_LAYER_STACK()
        {
            InitializeComponent();
        }
        #endregion

        #region Event
        private void C1Window_Initialized(object sender, EventArgs e)
        {
            try
            {
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void C1Window_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                object[] tmps = C1WindowExtension.GetParameters(this);

                //if (tmps != null && tmps.Length >= 1)
                //{                    
                //    _ProcID = Util.NVC(tmps[0]);
                //}
                //else
                //{                    
                //    _ProcID = "";
                //}

                //ApplyPermissions();
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void txtCSTID_KeyUp(object sender, KeyEventArgs e)
        {
            try
            {
                if (e.Key == Key.Enter)
                {
                    GetRepCarrierInfo();
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (bViewMsg) return;

                if (dgLotInfo.Rows.Count > 0)
                {
                    SetRepCarrier();
                }
                else
                {
                    Util.MessageValidation("SFU1498");    // 데이터가 없습니다.
                }
            }
            catch(Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void btnClose_Clicked(object sender, RoutedEventArgs e)
        {
            this.DialogResult = MessageBoxResult.Cancel;
        }

        private void btnClear_Click(object sender, RoutedEventArgs e)
        {
            dgLotInfo.ClearRows();
        }
        #endregion

        #region Mehod

        #region [BizCall]

        private void GetRepCarrierInfo()
        {
            try
            {
                loadingIndicator.Visibility = Visibility.Visible;

                DataTable inTable = new DataTable();
                inTable.Columns.Add("LOAD_REP_CSTID", typeof(string));

                DataRow newRow = inTable.NewRow();
                newRow["LOAD_REP_CSTID"] = txtCSTID.Text.Trim();

                inTable.Rows.Add(newRow);

                new ClientProxy().ExecuteService("DA_BAS_SEL_CARRIER_BY_LOAD_REP_CSTID", "RQSTDT", "RSLTDT", inTable, (searchResult, searchException) =>
                {
                    try
                    {
                        if (searchException != null)
                        {
                            Util.MessageException(searchException);
                            return;
                        }

                        if(searchResult.Rows.Count > 0)
                        {
                            dgLotInfo.ClearRows();
                            Util.GridSetData(dgLotInfo, searchResult, FrameOperation, true);
                            loadYN = "N";
                            cststat = string.Empty;
                        }

                        else
                            GetCarrierInfo();
                        
                    }
                    catch (Exception ex)
                    {
                        Util.MessageException(ex);
                    }
                    finally
                    {
                        loadingIndicator.Visibility = Visibility.Collapsed;
                    }
                }
                );
            }
            catch (Exception ex)
            {
                loadingIndicator.Visibility = Visibility.Collapsed;
                Util.MessageException(ex);
            }

        }

        private void GetCarrierInfo()
        {
            try
            {
                DataTable inTable = new DataTable();
                inTable.Columns.Add("CSTID", typeof(string));

                DataRow newRow = inTable.NewRow();
                newRow["CSTID"] = txtCSTID.Text.Trim();

                inTable.Rows.Add(newRow);


                DataTable searchResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_CARRIER", "RQSTDT", "RSLTDT", inTable);

                if (searchResult != null && searchResult.Rows.Count < 1)
                    Util.MessageValidation("SFU4564");    // 캐리어 정보가 없습니다

                else if (!string.IsNullOrEmpty(searchResult.Rows[0]["LOAD_REP_CSTID"].ToString()))
                {
                    txtCSTID.Text = searchResult.Rows[0]["LOAD_REP_CSTID"].ToString();
                    GetRepCarrierInfo();
                }

                else
                {
                    if(loadYN == "N")
                    {
                        dgLotInfo.ClearRows();
                    }
                    DataTable dtTo = DataTableConverter.Convert(dgLotInfo.ItemsSource);

                    if(dgLotInfo.Rows.Count == 0)
                    {
                        Util.GridSetData(dgLotInfo, searchResult, FrameOperation, true);
                        cststat = searchResult.Rows[0]["CSTSTAT"].ToString();
                    }

                    else if (dtTo.Select("CSTID = '" + txtCSTID.Text.Trim() + "'").Length > 0) //중복조건 체크
                    {
                        Util.MessageValidation("SFU1777"); //존재하는 Tray입니다.
                        return;
                    }
                    else
                    {
                        DataTable dtInfo = DataTableConverter.Convert(dgLotInfo.ItemsSource);

                        dtInfo.Merge(searchResult);
                        Util.GridSetData(dgLotInfo, dtInfo, FrameOperation);
                        
                    }
                    loadYN = "Y";
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }

        }        

        private void SetRepCarrier()
        {
            try
            {
                string sBizNAme = string.Empty;

                sBizNAme = "BR_PRD_REG_LOAD_REP_CSTID_WN";

                DataSet inDataSet = new DataSet();

                DataTable inEQP = inDataSet.Tables.Add("IN_EQP");
                DataRow erow = inEQP.NewRow();

                inEQP.Columns.Add("SRCTYPE", typeof(string));
                inEQP.Columns.Add("IFMODE", typeof(string));
                inEQP.Columns.Add("EQPTID", typeof(string));
                inEQP.Columns.Add("USERID", typeof(string));
                inEQP.Columns.Add("LOAD_YN", typeof(string));

                erow["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                erow["IFMODE"] = IFMODE.IFMODE_OFF;
                erow["USERID"] = LoginInfo.USERID;
                erow["LOAD_YN"] = loadYN;
                inDataSet.Tables["IN_EQP"].Rows.Add(erow);


                DataTable inLot = inDataSet.Tables.Add("INLOT");
                inLot.Columns.Add("CSTID", typeof(string));
                for (int i = 0; i < dgLotInfo.Rows.Count; i++)
                {
                    DataRow crow = inLot.NewRow();
                    crow["CSTID"] = Util.NVC(DataTableConverter.GetValue(dgLotInfo.Rows[i].DataItem, "CSTID"));
                    inDataSet.Tables["INLOT"].Rows.Add(crow);
                    if(!string.IsNullOrEmpty(cststat) && !cststat.Equals(Util.NVC(DataTableConverter.GetValue(dgLotInfo.Rows[i].DataItem, "CSTSTAT"))))
                    {
                        Util.MessageValidation("SFU3813"); //Tray 상태 NG.
                        return;
                
                    }
                }

                DataTable inCst = inDataSet.Tables.Add("IN_CST_R");
                DataRow rrow = inCst.NewRow();
                inCst.Columns.Add("CSTID_R", typeof(string));
                rrow["CSTID_R"] = Util.NVC(DataTableConverter.GetValue(dgLotInfo.Rows[0].DataItem, "CSTID")); ;
                inDataSet.Tables["IN_CST_R"].Rows.Add(rrow);

                new ClientProxy().ExecuteService_Multi(sBizNAme, "IN_EQP,INLOT,IN_CST_R", null, (Result, ex) =>
                {
                    if (ex != null)
                    {
                        Util.MessageException(ex);
                        return;
                    }

                    Util.MessageInfo("SFU1275");    //정상 처리 되었습니다.
                    //GetProductCellList(dgProdCellWinding);
                    dgLotInfo.ClearRows();
                    cststat = string.Empty;
                    loadYN = string.Empty;
                    txtCSTID.Clear();
                }, inDataSet);

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        #endregion

        #region [Validation]


        #endregion

        #region [Func]
        private void ApplyPermissions()
        {
            List<Button> listAuth = new List<Button>();
            listAuth.Add(btnSave);
            
            Util.pageAuth(listAuth, FrameOperation.AUTHORITY);
        }
        #endregion

        #endregion

        private void dgLotInfo_LoadedCellPresenter(object sender, DataGridCellEventArgs e)
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

                if (e.Cell.Row.Type == DataGridRowType.Item)
                {
                    if (string.IsNullOrEmpty(Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "LOAD_REP_CSTID")).ToString()) && e.Cell.Row.Index == 0)
                    {
                        e.Cell.Presenter.Background = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#E6F5FB"));
                    }
                    else
                    {
                        e.Cell.Presenter.Foreground = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#FF000000"));
                    }
                }
            }));
        }

        //private void dgLotInfo_UnloadedCellPresenter(object sender, DataGridCellEventArgs e)
        //{
        //    if (sender == null)
        //        return;

        //    C1.WPF.DataGrid.C1DataGrid dataGrid = sender as C1.WPF.DataGrid.C1DataGrid;

        //    dataGrid.Dispatcher.BeginInvoke(new Action(() =>
        //    {
        //        if (e.Cell.Presenter != null)
        //        {
        //            if (e.Cell.Row.Type == DataGridRowType.Item)
        //            {
        //                e.Cell.Presenter.Background = null;
        //                //e.Row.Presenter.Foreground = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#FF000000"));
        //            }
        //        }
        //    }));
        //}

        private void dgLotInfo_IsKeyboardFocusWithinChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (sender == null) return;

            C1DataGrid grd = sender as C1DataGrid;

            if ((bool)e.NewValue == false)
                grd.EndEdit();
        }

        
    }
}
