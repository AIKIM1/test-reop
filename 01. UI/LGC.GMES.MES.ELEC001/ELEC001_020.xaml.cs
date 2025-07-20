/*************************************************************************************
 Created Date : 2016.11.23
      Creator : 이슬아D
   Decription : 믹서원자재 자재LOT 입력
--------------------------------------------------------------------------------------
 [Change History]
  2016.11.23  이슬아 : 최초 생성





 
**************************************************************************************/

using C1.WPF;
using C1.WPF.DataGrid;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace LGC.GMES.MES.ELEC001
{
    public partial class ELEC001_020 : UserControl, IWorkArea
    {
        #region Declaration & Constructor 
        private const string _SearchBizRule = "DA_PRD_SEL_RMTRL_LABEL_INFO_MIX";
        private const string _SaveBizRule = "BR_PRD_REG_MTRL_LOTID_FROM_LABEL";

        private const string _SelectValue = "SELECT";
        private const string _CheckColumn = "CHK";
        Util _Util = new Util();

        /// <summary>
        /// Frame과 상호작용하기 위한 객체
        /// </summary>
        public IFrameOperation FrameOperation
        {
            get;
            set;
        }

        public ELEC001_020()
        {
            InitializeComponent();     
        }

        #endregion

        #region Initialize
        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            ApplyPermissions();
            InitCombo();
            InitControl();
            SetEvent();
        }

        #endregion

        #region Event

        private void SetEvent()
        {
            this.Loaded -= UserControl_Loaded;
            dtpDateFrom.SelectedDataTimeChanged += dtpDateFrom_SelectedDataTimeChanged;
            dtpDateTo.SelectedDataTimeChanged += dtpDateTo_SelectedDataTimeChanged;
        }
        private void InitControl()
        {
            dtpDateFrom.SelectedDateTime = DateTime.Now;
            dtpDateTo.SelectedDateTime = DateTime.Now;
        }
        private void dtpDateFrom_SelectedDataTimeChanged(object sender, SelectionChangedEventArgs e)
        {
            LGCDatePicker dtPik = sender as LGCDatePicker;
            if (Convert.ToDecimal(dtpDateTo.SelectedDateTime.ToString("yyyyMMdd")) < Convert.ToDecimal(dtPik.SelectedDateTime.ToString("yyyyMMdd")))
            {
                dtPik.SelectedDateTime = dtpDateTo.SelectedDateTime;
                //  LGC.GMES.MES.ControlsLibrary.MessageBox.Show("시작일자 이전 날짜는 선택할 수 없습니다.", null, "Error", MessageBoxButton.OK, ControlsLibrary.MessageBoxIcon.Warning);
                return;
            }
        }

        private void dtpDateTo_SelectedDataTimeChanged(object sender, SelectionChangedEventArgs e)
        {
            LGCDatePicker dtPik = sender as LGCDatePicker;
            if (Convert.ToDecimal(dtpDateFrom.SelectedDateTime.ToString("yyyyMMdd")) > Convert.ToDecimal(dtPik.SelectedDateTime.ToString("yyyyMMdd")))
            {
                dtPik.SelectedDateTime = dtpDateFrom.SelectedDateTime;
                //  LGC.GMES.MES.ControlsLibrary.MessageBox.Show("시작일자 이전 날짜는 선택할 수 없습니다.", null, "Error", MessageBoxButton.OK, ControlsLibrary.MessageBoxIcon.Warning);
                return;
            }
        }

        #region 조회조건 콤보 선택시
        /// <summary>
        /// 동 콤보 선택시
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void cboArea_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("AREAID", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["AREAID"] = cboArea.SelectedValue;

                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_MTGR_FOR_RMTRL_MIX_CBO", "RQSTDT", "RSLTDT", RQSTDT);

                cboMaterialGroup.DisplayMemberPath = "MTGRNAME";
                cboMaterialGroup.SelectedValuePath = "MTGRID";

                cboMaterialGroup.ItemsSource = AddStatus(dtResult, CommonCombo.ComboStatus.ALL, "MTGRID", "MTGRNAME").Copy().AsDataView();
                cboMaterialGroup.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }

        }
        /// <summary>
        /// 자재군 콤보 선택시
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void cboMaterialGroup_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("AREAID", typeof(string));
                RQSTDT.Columns.Add("MTGRID", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["AREAID"] = cboArea.SelectedValue;
                dr["MTGRID"] = cboMaterialGroup.SelectedValue;
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_RMTRL_BY_MTGR_MIX_CBO", "RQSTDT", "RSLTDT", RQSTDT);

                cboMaterial.DisplayMemberPath = "MTRLDISP2";
                cboMaterial.SelectedValuePath = "MTRLID";
                cboMaterial.ItemsSource = AddStatus(dtResult, CommonCombo.ComboStatus.ALL, "MTRLID", "MTRLDISP2").Copy().AsDataView();
                cboMaterial.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        /// <summary>
        /// 자재콤보 선택시
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void cboMaterial_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            try
            {
                if (cboMaterial.Text == "")
                {
                    Util.MessageValidation("SFU1828");
                    return;
                }
                DataTable dtData = DataTableConverter.Convert(cboMaterial.ItemsSource);
                DataRow drData = dtData.AsEnumerable().Where(c => c.Field<string>("MTRLID") == e.NewValue.ToString()).FirstOrDefault();
                txtMaterialDESC.Text = Util.NVC(drData["MTRLDESC"]);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        #endregion

        #region 조회버튼 클릭시 조회 + loadingindicator
        /// <summary>
        /// 조회버튼 클릭 이벤트 전
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnSearch_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            loadingIndicator.Visibility = Visibility.Visible;
        }

        /// <summary>
        /// 조회버튼 클릭 이벤트
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            SearchData();
        }
        #endregion

        private void dgResult_LoadedCellPresenter(object sender, DataGridCellEventArgs e)
        {
            try
            {
                //C1.WPF.DataGrid.C1DataGrid dataGrid = sender as C1.WPF.DataGrid.C1DataGrid;
               
                //dataGrid.Dispatcher.BeginInvoke(new Action(() =>
                //{
                //    if (e.Cell.Presenter == null)
                //    {
                //        return;
                //    }
                //    if (e.Cell.Column.Name == "MTRL_LOTID" && Convert.ToString(DataTableConverter.GetValue(e.Cell.Row.DataItem, "CHK").ToString()).Equals("True"))
                //    {                       
                //        e.Cell.Presenter.Background = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#f1b0bd"));
                //    }
                //}));
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            if (_Util.GetDataGridCheckCnt(dgResult, _CheckColumn) <= 0)
            {
                Util.MessageValidation("SFU1651");  //선택된 항목이 없습니다.
                return;
            }

            SaveData();
        }
        #endregion

        #region Mehod

        /// <summary>
        /// 화면내 버튼 권한 처리
        /// </summary>
        private void ApplyPermissions()
        {
            List<Button> listAuth = new List<Button>();
            //listAuth.Add(btnInReplace);
            //listAuth.Add(btnInReplace);

            Util.pageAuth(listAuth, FrameOperation.AUTHORITY);
        }

        private void InitCombo()
        {
            CommonCombo _combo = new CommonCombo();

            //동
            _combo.SetCombo(cboArea, CommonCombo.ComboStatus.SELECT, sFilter: new string[] { LoginInfo.CFG_SHOP_ID });
        }

        private DataTable AddStatus(DataTable dt, CommonCombo.ComboStatus cs, string sValue, string sDisplay)
        {

            DataRow dr = dt.NewRow();
            switch (cs)
            {
                case CommonCombo.ComboStatus.ALL:
                    dr[sDisplay] = "-ALL-";
                    dr[sValue] = String.Empty;
                    dt.Rows.InsertAt(dr, 0);
                    break;

                case CommonCombo.ComboStatus.SELECT:
                    dr[sDisplay] = "-SELECT-";
                    dr[sValue] = _SelectValue;
                    dt.Rows.InsertAt(dr, 0);
                    break;
            }

            return dt;


        }

        /// <summary>
        /// 조회
        /// </summary>
        private void SearchData()
        {
            string returnValue = string.Empty;
            try
            {
                if (_SelectValue.Equals(cboArea.SelectedValue))
                {
                    Util.MessageValidation("SFU1499");  //동을 선택하세요.
                    return;
                }

                if (_SelectValue.Equals(cboMaterialGroup.SelectedValue))
                {
                    Util.MessageValidation("SFU1826");  //자재군을 선택하세요.
                    return;
                }

                if (_SelectValue.Equals(cboMaterial.SelectedValue) || string.IsNullOrWhiteSpace(cboMaterial.Text))
                {
                    Util.MessageValidation("SFU1828");  //자재를 선택하세요.
                    return;
                }

                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("AREAID", typeof(string));
                RQSTDT.Columns.Add("MTGRID", typeof(string));
                RQSTDT.Columns.Add("MTRLID", typeof(string));
                RQSTDT.Columns.Add("DATEFROM", typeof(string));
                RQSTDT.Columns.Add("DATETO", typeof(string));
                RQSTDT.Columns.Add("PLLT_ID", typeof(string));
                RQSTDT.Columns.Add("INPUT_RSLT_FLAG", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["AREAID"] = cboArea.SelectedValue;
                if (!cboMaterialGroup.SelectedValue.Equals(""))
                    dr["MTGRID"] = cboMaterialGroup.SelectedValue;
                if (!cboMaterial.SelectedValue.Equals(""))
                    dr["MTRLID"] = cboMaterial.SelectedValue;
                dr["DATEFROM"] = dtpDateFrom.SelectedDateTime.ToShortDateString();
                dr["DATETO"] = dtpDateTo.SelectedDateTime.ToShortDateString();

                if (!string.IsNullOrEmpty(txtPalleteID.Text.Trim()))
                    dr["PLLT_ID"] = txtPalleteID.Text;

                if (chkInputRslt.IsChecked == true)
                    dr["INPUT_RSLT_FLAG"] = "Y";

                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync(_SearchBizRule, "RQSTDT", "RSLTDT", RQSTDT);
                Util.GridSetData(dgResult, dtResult, FrameOperation);                
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

        private void SaveData()
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LABELID", typeof(string));
                RQSTDT.Columns.Add("PLLT_ID", typeof(string));
                RQSTDT.Columns.Add("MTRL_LOTID", typeof(string));
                RQSTDT.Columns.Add("USERID", typeof(string));

                for (int row = 0; row < dgResult.Rows.Count - dgResult.BottomRows.Count; row++)
                {
                    if (_Util.GetDataGridCheckValue(dgResult, _CheckColumn, row))
                    {
                        DataRow dr = RQSTDT.NewRow();
                        dr["LABELID"] = Util.NVC(dgResult.GetCell(row, dgResult.Columns["RMTRL_LABEL_ID"].Index).Text);
                        dr["PLLT_ID"] = Util.NVC(dgResult.GetCell(row, dgResult.Columns["PLLT_ID"].Index).Text);
                        dr["MTRL_LOTID"] = Util.NVC(dgResult.GetCell(row, dgResult.Columns["MTRL_LOTID"].Index).Text);
                        dr["USERID"] = LoginInfo.USERID;
                        RQSTDT.Rows.Add(dr);
                    }
                }              

                new ClientProxy().ExecuteService(_SaveBizRule, "RQSTDT", null, RQSTDT, (result, ex) =>
                {
                    if (ex != null)
                    {
                        //LGC.GMES.MES.ControlsLibrary.MessageBox.ShowKeyEnter(MessageDic.Instance.GetMessage(ex.Message), ex.ToString(), "Info", MessageBoxButton.OK, MessageBoxIcon.Warning);
                        Util.AlertByBiz(_SaveBizRule, ex.Message, ex.ToString());
                        return;
                    }

                    Util.AlertInfo("SFU1275"); //정상처리되었습니다.
                    SearchData();
                });
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

        private void dgResult_BeginningEdit(object sender, C1.WPF.DataGrid.DataGridBeginningEditEventArgs e)
        {
            if (!Convert.ToString(DataTableConverter.GetValue(e.Row.DataItem, "CHK").ToString()).Equals(bool.TrueString))
                e.Cancel = true;
        }
        
        private void chkResult_Click(object sender, RoutedEventArgs e)
        {
            CheckBox cb = sender as CheckBox;
            //  DataTableConverter.SetValue(((C1.WPF.DataGrid.DataGridCellPresenter)rb.Parent).DataGrid.Rows[i].DataItem, "CHK", true);
            dgResult.SelectedIndex = ((C1.WPF.DataGrid.DataGridCellPresenter)cb.Parent).Row.Index;
        }
    }
}
