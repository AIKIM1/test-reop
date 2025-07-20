/*************************************************************************************
 Created Date : 2018.3.27
      Creator : 
   Decription : 불량 대차/불량 그룹LOT 등록'
--------------------------------------------------------------------------------------
 [Change History]
 
**************************************************************************************/

using System;
using System.Windows;
using System.Data;

using C1.WPF;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.CMM001.Extensions;
using C1.WPF.DataGrid;
using System.Linq;
using System.Windows.Media;
using System.Windows.Input;
using System.Windows.Controls;
using Application = System.Windows.Application;
using DataGridLength = C1.WPF.DataGrid.DataGridLength;
using DataGridRow = C1.WPF.DataGrid.DataGridRow;
using LGC.GMES.MES.CMM001;
using LGC.GMES.MES.CMM001.Popup;

namespace LGC.GMES.MES.COM001
{

    public partial class COM001_231_DELETE_SCRAP : C1Window, IWorkArea
    {
        #region Declaration
        Util _Util = new Util();
        BizDataSet _bizRule = new BizDataSet();

        private DataTable dtBindData = null;
        public string DELETE_SCRAP_CHK { get; set; }  //삭제/폐기 구분


        private bool _load = true;
        private readonly Util _util = new Util();
        private readonly BizDataSet _bizDataSet = new BizDataSet();
        public C1DataGrid DgNonWip { get; set; }
        /// <summary>
        ///  Frame과 상호작용하기 위한 객체
        /// </summary>
        public IFrameOperation FrameOperation
        {
            get;
            set;
        }
        #endregion

        #region Initialize

        public COM001_231_DELETE_SCRAP()
        {
            InitializeComponent();
        }
        #endregion

        #region Event

        #region [Form Load]
        private void C1Window_Loaded(object sender, RoutedEventArgs e)
        {
            if (_load)
            {
                // 비재공 삭제일 경우
                if (DELETE_SCRAP_CHK == "DELETE")
                {
                    btnDelete.Visibility = Visibility.Visible;
                    btnScrap.Visibility = Visibility.Collapsed;
                }
                else // 비재공 폐기일 경우
                {
                    btnDelete.Visibility = Visibility.Collapsed;
                    btnScrap.Visibility = Visibility.Visible;
                }

                dtBindData = DataTableConverter.Convert(DgNonWip.ItemsSource);
                dtBindData.Select("CHK <> 1").ToList<DataRow>().ForEach(row => row.Delete());
                dtBindData.AcceptChanges();

                SetGridNonWipList(dtBindData);
            
                _load = false;
            }

        }
        #endregion

        #region [비재공 삭제/폐기 - 삭제 ]
        
        #region [삭제 btnDelete_Click()]
        /// <summary>
        /// 활성화 삭제/폐기 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            if (!Validation())
            {
                return;
            }
            // 삭제하시겠습니까?
            Util.MessageConfirm("SFU1230", (result) =>
            {
                if (result == MessageBoxResult.OK)
                {
                    NonWipDelete();
                }
            });
        }

        #endregion

        #endregion
    
        #region [비재공 삭제/폐기 - 폐기 ]
    
        #region [폐기 btnScrap_Click()]
        /// <summary>
        /// 비재공 폐기 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>

        private void btnScrap_Click(object sender, RoutedEventArgs e)
        {

            if (!Validation())
            {
                return;
            }
            // 폐기하시겠습니까?
            Util.MessageConfirm("SFU4191", (result) =>
            {
                if (result == MessageBoxResult.OK)
                {
                    NonWipScrap();
                }
            });
        }

        #endregion

        #endregion

        #region [공통]
        #region [작업자 조회] txtUser_KeyDown(),btnUser_Click(), wndUser_Closed()
        /// <summary>
        /// 작업자 텍스트박스 조회
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void txtUser_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                GetUserWindow();
            }
        }
        /// <summary>
        /// 작업자 팝업 열기
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnUser_Click(object sender, RoutedEventArgs e)
        {
            GetUserWindow();
        }
        /// <summary>
        ///  작업자 팝업 닫기
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void wndUser_Closed(object sender, EventArgs e)
        {
            CMM_PERSON wndPerson = sender as CMM_PERSON;
            if (wndPerson.DialogResult == MessageBoxResult.OK)
            {

                txtUser.Text = wndPerson.USERNAME;
                txtUser.Tag = wndPerson.USERID;

            }
            foreach (Grid tmp in Util.FindVisualChildren<Grid>(Application.Current.MainWindow))
            {
                if (tmp.Name == "grdMain")
                {
                    tmp.Children.Remove(wndPerson);
                    break;
                }
            }
        }
        #endregion
        #endregion

        #region [닫기]
        private void btnClose_Click(object sender, RoutedEventArgs e)
        {

            this.DialogResult = MessageBoxResult.Cancel;
            //if (btnInput.IsEnabled == true)
            //{
            //    this.DialogResult = MessageBoxResult.Cancel;
            //}
            //else
            //{
            //    this.DialogResult = MessageBoxResult.OK;
            //}


        }
        private void C1Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            this.DialogResult = MessageBoxResult.Cancel;
            //if (btnInput.IsEnabled == true)
            //{
            //    this.DialogResult = MessageBoxResult.Cancel;
            //}
            //else
            //{
            //    this.DialogResult = MessageBoxResult.OK;
            //}
        }
        #endregion


        #endregion

        #region User Method

        #region [비재공 삭제/폐기 - 삭제]
        /// <summary>
        ///  삭제
        /// </summary>
        private void NonWipDelete()
        {
            try
            {
                ShowLoadingIndicator();

                DataSet inData = new DataSet();

                //마스터 정보
                DataTable inDataTable = inData.Tables.Add("INDATA");
                inDataTable.Columns.Add("NON_WIP_ID", typeof(string));
                inDataTable.Columns.Add("USERID", typeof(string));
                inDataTable.Columns.Add("ACT_USERID", typeof(string));
                inDataTable.Columns.Add("NOTE", typeof(string));

                DataRow row = null;
                row = inDataTable.NewRow();
                row["NON_WIP_ID"] = Util.NVC(DataTableConverter.GetValue(dgNonWip.Rows[0].DataItem, "NON_WIP_ID"));
                row["USERID"] = LoginInfo.USERID;
                row["ACT_USERID"] = txtUser.Tag;
                row["NOTE"] = txtNote.Text;

                inDataTable.Rows.Add(row);

                try
                {
                    //
                    new ClientProxy().ExecuteService_Multi("BR_ACT_REG_DELETE_NON_WIP", "INDATA", null, (Result, ex) =>
                    {
                        if (ex != null)
                        {
                            Util.MessageException(ex);
                            HiddenLoadingIndicator();
                            return;
                        }
                        this.DialogResult = MessageBoxResult.OK;

                    }, inData);

                }
                catch (Exception ex)
                {
                    HiddenLoadingIndicator();
                    Util.AlertByBiz("BR_ACT_REG_DELETE_NON_WIP", ex.Message, ex.ToString());

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

        #region [비재공 삭제/폐기 - 폐기]
        /// <summary>
        ///  폐기
        /// </summary>
        private void NonWipScrap()
        {
            try
            {
                ShowLoadingIndicator();

                DataSet inData = new DataSet();

                //마스터 정보
                DataTable inDataTable = inData.Tables.Add("INDATA");
                inDataTable.Columns.Add("NON_WIP_ID", typeof(string));
                inDataTable.Columns.Add("USERID", typeof(string));
                inDataTable.Columns.Add("ACT_USERID", typeof(string));
                inDataTable.Columns.Add("NOTE", typeof(string));

                DataRow row = null;
                row = inDataTable.NewRow();
                row["NON_WIP_ID"] = Util.NVC(DataTableConverter.GetValue(dgNonWip.Rows[0].DataItem, "NON_WIP_ID"));
                row["USERID"] = LoginInfo.USERID;
                row["ACT_USERID"] = txtUser.Tag;
                row["NOTE"] = txtNote.Text;

                inDataTable.Rows.Add(row);

                try
                {
                    //
                    new ClientProxy().ExecuteService_Multi("BR_ACT_REG_SCRAP_NON_WIP", "INDATA", null, (Result, ex) =>
                    {
                        if (ex != null)
                        {
                            Util.MessageException(ex);
                            HiddenLoadingIndicator();
                            return;
                        }
                        this.DialogResult = MessageBoxResult.OK;

                    }, inData);

                }
                catch (Exception ex)
                {
                    HiddenLoadingIndicator();
                    Util.AlertByBiz("BR_ACT_REG_SCRAP_NON_WIP", ex.Message, ex.ToString());

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

        #region[[공통]

        #region 삭제 및 폐기  비재공 리스트 정보 셋팅 SetGridNonWipList()
        /// <summary>
        /// 수정할 비재공 리스트 정보 셋팅
        /// </summary>
        /// <param name="dt"></param>
        private void SetGridNonWipList(DataTable dt)
        {
            try
            {
                Util.GridSetData(dgNonWip, dt, FrameOperation);
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

        #region 작업자 팝업 GetUserWindow()

        //작업자
        private void GetUserWindow()
        {
            CMM_PERSON wndPerson = new CMM_PERSON();
            wndPerson.FrameOperation = FrameOperation;
            wndPerson.Width = 600;

            if (wndPerson != null)
            {
                object[] Parameters = new object[1];

                C1WindowExtension.SetParameters(wndPerson, Parameters);

                Parameters[0] = txtUser.Text;
                wndPerson.Closed += new EventHandler(wndUser_Closed);

                foreach (Grid tmp in Util.FindVisualChildren<Grid>(Application.Current.MainWindow))
                {
                    if (tmp.Name == "grdMain")
                    {
                        grdMain.Children.Add(wndPerson);
                        wndPerson.BringToFront();
                        break;
                    }
                }


            }
        }
        #endregion
      
        #region[[Validation]
        private bool Validation()
        {

            if (txtUser.Text == string.Empty || txtUser.Tag == null)
            {
                Util.MessageValidation("SFU4591"); // 작업자를 선택하세요
                return false;
            }

            return true;
        }


        #endregion

        #endregion

        #region [Func]
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

        #endregion

    }
}
