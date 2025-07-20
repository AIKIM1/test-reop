/*************************************************************************************
 Created Date : 2023.11.15
      Creator : 배현우
   Decription : 포트별 자재등록 - 설비 포트 등록

--------------------------------------------------------------------------------------
 [Change History]
  2023.12.04  배현우 : Initial Created.
**************************************************************************************/


using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using C1.WPF;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.ControlsLibrary;
using System.Data;
using C1.WPF.DataGrid;

namespace LGC.GMES.MES.COM001
{
    /// <summary>
    /// .xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class COM001_392_EQPT_PORT_MAPPING : C1Window, IWorkArea
    {
        #region Declaration & Constructor         

        private string _previewQtyValue = string.Empty;
        private readonly BizDataSet _bizDataSet = new BizDataSet();
        private bool _load = true;

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
        public COM001_392_EQPT_PORT_MAPPING()
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
                if (_load)
                {
                    SetControl();
                    _load = false;
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void SetControl()
        {
            try
            {
                object[] tmps = C1WindowExtension.GetParameters(this);

                if (!Util.NVC(tmps[0]).Equals(""))
                {
                    txtEqptID.Text = Util.NVC(tmps[0]);
                    //SearchLastCellNo();
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
                Equipment_Port_Mapping();
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void btnClose_Clicked(object sender, RoutedEventArgs e)
        {
            this.DialogResult = MessageBoxResult.Cancel;
        }
        #endregion

        #region Mehod

        #region [BizCall]

        private void Equipment_Port_Mapping()
        {
            try
            {
                ShowLoadingIndicator();
                
                DataSet ds = new DataSet();
                DataTable inTable = ds.Tables.Add("INDATA");
                inTable.Columns.Add("EQPTID", typeof(string));
                inTable.Columns.Add("PORT_ID", typeof(string));
                inTable.Columns.Add("USERID", typeof(string));
                inTable.Columns.Add("USE_FLAG", typeof(string));



                DataRow newRow = inTable.NewRow();
                newRow["EQPTID"] = txtEqptID.Text.Trim();
                newRow["PORT_ID"] = txtPortID.Text.Trim();
                newRow["USERID"] = LoginInfo.USERID;
                newRow["USE_FLAG"] = 'Y';
                inTable.Rows.Add(newRow);

                new ClientProxy().ExecuteService_Multi("BR_PRD_REG_PORT_MTRL_SET", "INDATA", null, (bizResult, bizException) =>
                {
                    try
                    {
                        if (bizException != null)
                        {
                            Util.MessageException(bizException);
                            return;
                        }

                        Util.MessageInfo("SFU1275");//정상처리되었습니다.
                        

                    }
                    catch (Exception ex)
                    {
                        Util.MessageException(ex);
                    }
                    
                }, ds);

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
            finally
            {
                HiddenLoadingIndicator();
            }
        }


        #endregion

        #region [Validation]


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
