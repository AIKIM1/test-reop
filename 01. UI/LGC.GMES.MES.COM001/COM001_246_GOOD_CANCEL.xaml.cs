/*************************************************************************************
 Created Date : 
      Creator : 
   Decription : 
--------------------------------------------------------------------------------------
 [Change History]
 
**************************************************************************************/

using System;
using System.Windows;
using System.Data;
using System.Linq;
using C1.WPF;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using LGC.GMES.MES.CMM001.Class;
using C1.WPF.DataGrid;
using System.Windows.Input;
using System.Windows.Controls;
using Application = System.Windows.Application;
using LGC.GMES.MES.CMM001.Popup;

namespace LGC.GMES.MES.COM001
{

    public partial class COM001_246_GOOD_CANCEL : C1Window, IWorkArea
    {
        #region Declaration
      
        private bool _load = true;
        private readonly Util _util = new Util();
        private readonly BizDataSet _bizDataSet = new BizDataSet();
        private string _AreaID = string.Empty;
        private DataTable _CANCEL_GOOD_DATA = null;

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

        public COM001_246_GOOD_CANCEL()
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
                object[] parameters = C1WindowExtension.GetParameters(this);

                _CANCEL_GOOD_DATA = parameters[0] as DataTable;
                _AreaID = parameters[1] as string;

                if (_CANCEL_GOOD_DATA == null)
                {
                    return;
                }

                SetGridGoodInfo(_CANCEL_GOOD_DATA);

                _load = false;
            }

        }

        private void SetGridGoodInfo(DataTable dt)
        {
            try
            {
               Util.GridSetData(dgGood, dt, FrameOperation, true);
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

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidateCancel())
            {
                return;
            }

            //취소 하시겠습니까?
            Util.MessageConfirm("SFU1243", (result) =>
            {
                if (result == MessageBoxResult.OK)
                {
                    GoodCancel();
                }
            });
        }

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = MessageBoxResult.Cancel;
        }

        private void C1Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (this.DialogResult != MessageBoxResult.OK)
            {
                this.DialogResult = MessageBoxResult.Cancel;
            }

        }

        private void txtUserName_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                GetUserWindow();
            }
        }

        private void btnUser_Click(object sender, RoutedEventArgs e)
        {
            GetUserWindow();
        }

        private void wndUser_Closed(object sender, EventArgs e)
        {
            CMM_PERSON wndPerson = sender as CMM_PERSON;
            if (wndPerson.DialogResult == MessageBoxResult.OK)
            {

                txtUserNameCr.Text = wndPerson.USERNAME;
                txtUserNameCr.Tag = wndPerson.USERID;

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

        private void GoodCancel()
        {
            try
            {
                ShowLoadingIndicator();

                // DATA SET 
                DataSet inDataSet = new DataSet();
                DataTable inTable = inDataSet.Tables.Add("INDATA");
                inTable.Columns.Add("SRCTYPE", typeof(string));
                inTable.Columns.Add("IFMODE", typeof(string));
                inTable.Columns.Add("AREAID", typeof(string));
                inTable.Columns.Add("USERID", typeof(string));
                inTable.Columns.Add("WRK_USERID", typeof(string));
                inTable.Columns.Add("WRK_USERNAME", typeof(string));

                DataRow newRow = inTable.NewRow();
                newRow["SRCTYPE"] = "UI";
                newRow["IFMODE"] = "OFF";
                newRow["AREAID"] = _AreaID;
                newRow["USERID"] = LoginInfo.USERID;
                newRow["WRK_USERID"] = txtUserNameCr.Tag;
                newRow["WRK_USERNAME"] = txtUserNameCr.Text;

                inTable.Rows.Add(newRow);

                DataTable inRESN = inDataSet.Tables.Add("INLOT");
                inRESN.Columns.Add("LOTID", typeof(string));
                inRESN.Columns.Add("ACTQTY", typeof(Decimal));
                inRESN.Columns.Add("ACTQTY2", typeof(Decimal));
                inRESN.Columns.Add("ACT_CALDATE_YYYYMM", typeof(string));
                inRESN.Columns.Add("CTNR_ID", typeof(string));
                inRESN.Columns.Add("RESNCODE", typeof(string));
                inRESN.Columns.Add("PRE_RESNQTY", typeof(Decimal));

                for (int i = 0; i < _CANCEL_GOOD_DATA.Rows.Count; i++)
                {
                    newRow = inRESN.NewRow();
                    newRow["LOTID"] = _CANCEL_GOOD_DATA.Rows[i]["LOTID"];
                    newRow["ACTQTY"] = _CANCEL_GOOD_DATA.Rows[i]["RESNQTY"];
                    newRow["ACTQTY2"] = _CANCEL_GOOD_DATA.Rows[i]["RESNQTY"];
                    newRow["ACT_CALDATE_YYYYMM"] = _CANCEL_GOOD_DATA.Rows[i]["ACT_CALDATE_YYYYMM"];
                    newRow["CTNR_ID"] = _CANCEL_GOOD_DATA.Rows[i]["CTNR_ID"];
                    newRow["RESNCODE"] = _CANCEL_GOOD_DATA.Rows[i]["RESNCODE"];
                    newRow["PRE_RESNQTY"] = _CANCEL_GOOD_DATA.Rows[i]["RESNQTY"];
                    inRESN.Rows.Add(newRow);
                }

                new ClientProxy().ExecuteService_Multi("BR_PRD_CANCEL_GOOD_LOT_PC", "INDATA,INLOT", null, (bizResult, bizException) =>
                {
                    try
                    {
                        if (bizException != null)
                        {
                            HiddenLoadingIndicator();
                            Util.MessageException(bizException);
                            return;
                        }

                        //Util.AlertInfo("정상 처리 되었습니다.");
                        Util.MessageInfo("SFU1889");

                        this.DialogResult = MessageBoxResult.OK;
                    }
                    catch (Exception ex)
                    {
                        HiddenLoadingIndicator();
                        Util.MessageException(ex);
                    }
                }, inDataSet);
            }
            catch (Exception ex)
            {
                HiddenLoadingIndicator();
                Util.MessageException(ex);
            }
        }

        private void GetUserWindow()
        {
            CMM_PERSON wndPerson = new CMM_PERSON();
            wndPerson.FrameOperation = FrameOperation;
            wndPerson.Width = 600;
            if (wndPerson != null)
            {
                object[] Parameters = new object[1];

                C1WindowExtension.SetParameters(wndPerson, Parameters);

                Parameters[0] = txtUserNameCr.Text;
                wndPerson.Closed += new EventHandler(wndUser_Closed);

                foreach (Grid tmp in Util.FindVisualChildren<Grid>(Application.Current.MainWindow))
                {
                    if (tmp.Name == "grdMain")
                    {
                        tmp.Children.Add(wndPerson);
                        wndPerson.BringToFront();
                        break;
                    }
                }
            }
        }

        #endregion

        private bool ValidateCancel()
        {
            if (dgGood.Rows.Count == 0)
            {
                Util.MessageValidation("SFU3537");  //조회된 데이타가 없습니다
                return false;
            }

            if (txtUserNameCr.Text == string.Empty)
            {
                // 작업자 정보를 입력하세요
                Util.MessageValidation("SFU4201");
                return false;
            }
            if (txtUserNameCr.Tag == null)
            {
                // 작업자 정보를 입력하세요
                Util.MessageValidation("SFU4201");
                return false;
            }
           
            return true;
        }
        
        private void ShowLoadingIndicator()
        {
            if (loadingIndicator != null)
            {
                loadingIndicator.Visibility = Visibility.Visible;
            }
        }
        private void HiddenLoadingIndicator()
        {
            if (loadingIndicator != null)
            {
                loadingIndicator.Visibility = Visibility.Collapsed;
            }
        }


    }
}
