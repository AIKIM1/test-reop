/*************************************************************************************
 Created Date : 2016.06.16
      Creator : 
   Decription : 
--------------------------------------------------------------------------------------
 [Change History]  
  2023.11.30  차요한       소형활성화MES(기존 FCS002_400_REPLACE_DSNT 방습제 교체
 
**************************************************************************************/

using C1.WPF;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using System;
using System.Data;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace LGC.GMES.MES.FCS002
{
    public partial class FCS002_400_REPLACE_DSNT : C1Window, IWorkArea
    {
        #region Declaration & Constructor 

        private Util _Util = new Util();
        //private string _UserId = string.Empty;
        private string _boxId = string.Empty;
        private string _dsntId = string.Empty;

        /// <summary>
        ///  Frame과 상호작용하기 위한 객체
        /// </summary>
        public IFrameOperation FrameOperation
        {
            get;
            set;
        }

        public FCS002_400_REPLACE_DSNT()
        {
            InitializeComponent();
        }
        #endregion

        #region Initialize
        private void FCS002_400_REPACE_DSNT_Loaded(object sender, RoutedEventArgs e)
        {
            Loaded -= FCS002_400_REPACE_DSNT_Loaded;
            InitControl();

            txtReplaceDsnt.Focus();            
        }

        private void InitControl()
        {
            this.Focus();
            object[] tmps = C1WindowExtension.GetParameters(this);
            _boxId = tmps[0] as string;
            _dsntId = tmps[1] as string;
            txtOriginDsnt.Text = tmps[1] as string;                        
        }

        #endregion

        #region Event

        #endregion

        #region Method        

        #endregion

        private void btnClear_Click(object sender, RoutedEventArgs e)
        {
            Clear();
        }

        private void Clear()
        {
            txtReplaceDsnt.Text = string.Empty;            
        }

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = MessageBoxResult.Cancel;
        }
        
        private void btnChange_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(txtReplaceDsnt.Text.Trim()))
            {
                // SFU8028 방습제 아이디를 입력하세요.
                Util.MessageValidation("SFU8028", (result) =>
                {
                    txtReplaceDsnt.Focus();
                });
                return;
            }

            //	SFU8027		방습제를 교체하시겠습니까?
            Util.MessageConfirm("SFU8027", (result) =>
            {
                if (result == MessageBoxResult.OK)
                {                                        
                    DataSet indataSet = new DataSet();
                    DataTable inDataTable = indataSet.Tables.Add("INDATA");
                    inDataTable.Columns.Add("EQPTID");
                    inDataTable.Columns.Add("BOXID");
                    inDataTable.Columns.Add("RESULTCODE", typeof(int));
                    inDataTable.Columns.Add("DESICCANTID");
                    inDataTable.Columns.Add("USERID");                    

                    DataRow newRow = inDataTable.NewRow();
                    newRow["BOXID"] = _boxId;
                    newRow["RESULTCODE"] = 1;
                    newRow["DESICCANTID"] = txtReplaceDsnt.Text.Trim();
                    newRow["USERID"] = LoginInfo.USERID;
                    
                    inDataTable.Rows.Add(newRow);
                    
                    loadingIndicator.Visibility = Visibility.Visible;

                    new ClientProxy().ExecuteService_Multi("BR_SET_INBOX_DESICCANT_MB", "INDATA", "OUTDATA", (bizResult, bizException) =>
                    {
                        try
                        {
                            if (bizException != null)
                            {
                                Util.MessageException(bizException);
                                return;
                            }

                            Clear();
                            this.DialogResult = MessageBoxResult.OK;
                        }
                        catch (Exception ex)
                        {
                            Util.MessageException(ex);
                        }
                        finally
                        {
                            loadingIndicator.Visibility = Visibility.Collapsed;
                        }

                    }, indataSet);
                }
            });
        }

        private void text_GotFocus(object sender, RoutedEventArgs e)
        {
            InputMethod.Current.ImeConversionMode = ImeConversionModeValues.Alphanumeric;
        }

    }
}
