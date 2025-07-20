/*************************************************************************************
 Created Date : 2018.07.17
      Creator : 이제섭
   Decription : 대차단위 ROUTE 변경
--------------------------------------------------------------------------------------
 [Change History]

    
**************************************************************************************/

using C1.WPF;
using C1.WPF.DataGrid;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using System;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace LGC.GMES.MES.CMM001.Popup
{
    /// <summary>
    /// CMM_BOX_CART_CHANGE_ROUTE.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class CMM_BOX_CART_CHANGE_ROUTE : C1Window, IWorkArea
    {
        #region Declaration & Constructor

        // private string _procID = string.Empty;        // 공정코드
        //private string _eqptID = string.Empty;        // 설비코드
        private string _userid = string.Empty;        // 작업자ID

        private DataTable _inboxList;

        public bool QueryCall { get; set; }

        Util _Util = new Util();
        BizDataSet _bizRule = new BizDataSet();

        private bool _load = true;

        #endregion

        #region Initialize
        /// <summary>
        ///  
        /// </summary>
        public IFrameOperation FrameOperation
        {
            get;
            set;
        }

        public CMM_BOX_CART_CHANGE_ROUTE()
        {
            InitializeComponent();
        }

        #endregion


        #region [Form Load]
        private void C1Window_Loaded(object sender, RoutedEventArgs e)
        {
            if (_load)
            {
                InitializeUserControls();
                SetControl();

                _load = false;
            }
        }

        private void InitializeUserControls()
        {

        }

        private void SetControl()
        {
            object[] tmps = C1WindowExtension.GetParameters(this);

            //_procID = tmps[0] as string;
            //_eqptID = tmps[2] as string;
            _userid = tmps[5] as string;

            SetGridCartList();

        }
        #endregion


        #region [닫기]
        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = MessageBoxResult.Cancel;
        }
        #endregion

        #region [대차ID KeyDown]
        private void txtCartID_GotFocus(object sender, RoutedEventArgs e)
        {
            InputMethod.Current.ImeConversionMode = ImeConversionModeValues.Alphanumeric;
        }

        private void txtCartID_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                if (string.IsNullOrWhiteSpace(txtCartID.Text)) return;

                SetGridCartList();
                

                if (dgCart != null && dgCart.Rows.Count > 0)
                {
                    SetGridProductInboxList();
                }

            }

            txtCartID.Text = string.Empty; //대차ID 입력 Textbox Clear
            
        }

        #endregion

        #region Mehod

        /// <summary>
        /// Cart List
        /// </summary>
        private void SetGridCartList()
        {
            try
            {
                DataTable inTable = new DataTable();
                inTable.Columns.Add("LANGID", typeof(string));
                inTable.Columns.Add("CTNR_ID", typeof(string));
                
                
                DataRow newRow = inTable.NewRow();
                newRow["LANGID"] = LoginInfo.LANGID;
                newRow["CTNR_ID"] = txtCartID.Text;
                inTable.Rows.Add(newRow);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_TB_SFC_CTNR_PC", "INDATA", "OUTDATA", inTable);
                Util.GridSetData(dgCart, dtResult, FrameOperation, true);

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }    
         }

        private void SetGridProductInboxList()
        {
            try
            {

                string sCtnrid = Util.NVC(DataTableConverter.GetValue(dgCart.Rows[0].DataItem, "CTNR_ID"));

                DataTable inTable = new DataTable();
                inTable.Columns.Add("LANGID", typeof(string));
                inTable.Columns.Add("CTNR_ID", typeof(string));
                
                DataRow newRow = inTable.NewRow();
                newRow["LANGID"] = LoginInfo.LANGID;
                newRow["CTNR_ID"] = sCtnrid.Trim();


                inTable.Rows.Add(newRow);

                _inboxList = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_INBOX_CART_LOAD", "INDATA", "OUTDATA", inTable);

                Util.GridSetData(dgProductionInbox, _inboxList, FrameOperation, true);

                if (_inboxList != null && _inboxList.Rows.Count > 0)
                    dgProductionInbox.CurrentCell = dgProductionInbox.GetCell(0, 1);

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        /// <summary>
        /// Route Change 
        /// </summary>
        private void ChageRoute()
        {
            try
            {
                ShowLoadingIndicator();

                DataSet inDataSet = new DataSet();
                DataTable inTable = inDataSet.Tables.Add("INDATA");
                inTable.Columns.Add("CTNR_ID");
                inTable.Columns.Add("USERID");

                DataRow newRow = inTable.NewRow();
                newRow["CTNR_ID"] = Util.NVC(DataTableConverter.GetValue(dgCart.Rows[0].DataItem, "CTNR_ID")).Trim();
                newRow["USERID"] = _userid;
                inTable.Rows.Add(newRow);
                

                new ClientProxy().ExecuteService_Multi("BR_PRD_UPD_RMA_CHANGE_ROUTE_NJ", "INDATA", null, (bizResult, bizException) =>
                {
                    try
                    {
                        if (bizException != null)
                        {
                            HiddenLoadingIndicator();
                            Util.MessageException(bizException);
                            return;
                        }
                        else
                        {
                            // 변경되었습니다.
                            Util.MessageInfo("SFU1166");

                        }

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

        #endregion

        #region [Func]
        private bool ValidationChangeRoute()
        {
            string ctnr_id = Util.NVC(DataTableConverter.GetValue(dgCart.Rows[0].DataItem, "CTNR_ID"));

            DataTable dt = DataTableConverter.Convert(dgCart.ItemsSource);
            DataRow[] dr = dt.Select("CTNR_ID = '" + ctnr_id.ToString() + "'");

            if (dr.Length == 0)
            {
                // 대차 정보가 없습니다.
                Util.MessageValidation("SFU4365");
                return false;
            }

            if (Util.NVC_Int(dr[0]["CELL_QTY"]) == 0)
            {
                // 대차에 Inbox 정보가 없습니다.
                Util.MessageValidation("SFU4375");
                return false;
            }

            return true;
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


        #endregion

        #region [변경버튼 클릭]
        private void btnChangeRoute_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidationChangeRoute())
                return;

            // 변경 하시겠습니까?
            Util.MessageConfirm("SFU2875", (result) =>
            {
                if (result == MessageBoxResult.OK)
                    {
                         ChageRoute();
                   
                    }
                });
             }
          }
      }
        #endregion
