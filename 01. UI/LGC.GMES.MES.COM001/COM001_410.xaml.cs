/*************************************************************************************
 Created Date : 2024.05.28
      Creator : 
   Decription : 파우치 조립 불량 Cell 조회
--------------------------------------------------------------------------------------
 [Change History]
  2024.05.30  오수현 : Initial Created.        





**************************************************************************************/
using System;
using System.Data;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

using C1.WPF.DataGrid;
using C1.WPF;

using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;

namespace LGC.GMES.MES.COM001
{
    public partial class COM001_410 : UserControl, IWorkArea
    {
        #region Declaration & Constructor 

        Util _Util = new Util();
        BizDataSet _Biz = new BizDataSet();



        public COM001_410()
        {
            InitializeComponent();
        }
        public IFrameOperation FrameOperation
        {
            get;
            set;
        }


        private void UserControl_Initialized(object sender, EventArgs e)
        {
            Initialize();
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            List<Button> listAuth = new List<Button>();
            Util.pageAuth(listAuth, FrameOperation.AUTHORITY);
            InitCombo();
            this.Loaded -= UserControl_Loaded;
        }

        #endregion

        #region Initialize
        private void Initialize()
        {
        }

        private void InitCombo()
        {

            //동,라인,공정,설비 셋팅
            CommonCombo _combo = new CommonCombo();

            C1ComboBox[] cboAreaChild = { cboProcess };
            _combo.SetCombo(cboArea, CommonCombo.ComboStatus.NONE, cbChild: cboAreaChild);

            cboArea.SelectedValue = LoginInfo.CFG_AREA_ID;

            //공정        
            C1ComboBox[] cboProcParent = { cboArea };
            C1ComboBox[] cboProcChild = { cboEquipmentSegment, cboEquipment };
            _combo.SetCombo(cboProcess, CommonCombo.ComboStatus.SELECT, cbChild: cboProcChild, sCase: "PROCESSWITHAREA", cbParent: cboProcParent);

            //라인
            C1ComboBox[] cboEqsgParent = { cboArea, cboProcess };
            C1ComboBox[] cboEqsgChild = { cboEquipment };
            _combo.SetCombo(cboEquipmentSegment, CommonCombo.ComboStatus.ALL, cbChild: cboEqsgChild, cbParent: cboEqsgParent, sCase: "PROCESSEQUIPMENTSEGMENT");
            cboEquipmentSegment.SelectedValue = LoginInfo.CFG_EQSG_ID;

            //설비
            C1ComboBox[] cboEquipmentParent = { cboEquipmentSegment, cboProcess };
            _combo.SetCombo(cboEquipment, CommonCombo.ComboStatus.ALL, cbParent: cboEquipmentParent);

            //설비
            //SetEquipmentPopControl();

            //작업일
            dtpDateFrom.SelectedDateTime = System.DateTime.Now;
            dtpDateTo.SelectedDateTime = System.DateTime.Now;
        }

        #endregion

        #region Event
        private void dgSearch_LoadedCellPresenter(object sender, DataGridCellEventArgs e)
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

        private void dgSearch_UnloadedCellPresenter(object sender, DataGridCellEventArgs e)
        {
            if (sender == null)
                return;

            C1.WPF.DataGrid.C1DataGrid dataGrid = sender as C1.WPF.DataGrid.C1DataGrid;

            dataGrid.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (e.Cell.Presenter != null)
                {
                    if (e.Cell.Row.Type == DataGridRowType.Item)
                    {
                        e.Cell.Presenter.Background = null;
                    }
                }
            }));
        }

        #region 조회
        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            if (Convert.ToDecimal(Convert.ToDateTime(dtpDateFrom.SelectedDateTime).ToString("yyyyMMdd")) > Convert.ToDecimal(Convert.ToDateTime(dtpDateTo.SelectedDateTime).ToString("yyyyMMdd")))
            {
                Util.Alert("SFU1913");  //종료일자가 시작일자보다 빠릅니다.
                return;
            }

            if (cboArea.SelectedValue.ToString() == "SELECT")
            {
                Util.MessageValidation("SFU4238"); //동정보를 선택하세요
                return;
            }

            if (String.IsNullOrEmpty(txtCellID.Text))
            {
                GetSelectInfo();
            }   
            else
            {
                GetSelectInfo("cell");
            }
            
        }

        private void txtCellID_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                GetSelectInfo("cell");
            }
        }
        #endregion

        #endregion

        #region Method

        private void GetSelectInfo(String cell = null)
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("AREAID", typeof(string));
                RQSTDT.Columns.Add("EQSGID", typeof(string));
                RQSTDT.Columns.Add("PROCID", typeof(string));
                RQSTDT.Columns.Add("EQPTID", typeof(string));
                RQSTDT.Columns.Add("FROM_DTTM", typeof(string));
                RQSTDT.Columns.Add("TO_DTTM", typeof(string));
                RQSTDT.Columns.Add("SUBLOTID", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["AREAID"] = cboArea.SelectedValue.ToString();
                if (Util.NVC(cell).Equals("cell"))
                {
                    dr["SUBLOTID"] = Util.NVC(txtCellID.Text);
                }
                else
                {
                    dr["EQSGID"] = string.IsNullOrEmpty(cboEquipmentSegment.SelectedValue.ToString()) ? null : cboEquipmentSegment.SelectedValue.ToString();
                    dr["PROCID"] = string.IsNullOrEmpty(cboProcess.SelectedValue.ToString()) ? null : cboProcess.SelectedValue.ToString();
                    dr["EQPTID"] = string.IsNullOrEmpty(cboEquipment.SelectedValue.ToString()) ? null : cboEquipment.SelectedValue.ToString();
                    dr["FROM_DTTM"] = dtpDateFrom.SelectedDateTime.ToString("yyyy-MM-dd 00:00:00");
                    dr["TO_DTTM"] = dtpDateTo.SelectedDateTime.ToString("yyyy-MM-dd 23:59:59");
                }

                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_CELL_NG_NJ", "INDATA", "OUTDATA", RQSTDT);
                Util.GridSetData(dgSearch, dtResult, FrameOperation, true);

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
        #endregion

        #region Funct
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
    }
}

