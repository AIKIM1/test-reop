/*************************************************************************************
 Created Date : 2017.11.21
      Creator : 이영준
   Decription : 반품 셀 양품화 등록
--------------------------------------------------------------------------------------
 [Change History]
  2017.11.21  이영준 : Initial Created.
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
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Data;
using System.Globalization;
using System.Reflection;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.CMM001;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.CMM001.UserControls;
using LGC.GMES.MES.ControlsLibrary;
using C1.WPF;
using LGC.GMES.MES.CMM001.Popup;
using C1.WPF.DataGrid;
using LGC.GMES.MES.CMM001.Extensions;
using System.Windows.Media.Animation;
using C1.WPF.DataGrid.Summaries;

namespace LGC.GMES.MES.BOX001
{
    /// <summary>
    /// BOX001_100.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class BOX001_208 : UserControl, IWorkArea
    {
        #region Declaration & Constructor
        private Util _util = new Util();
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

        public BOX001_208()
        {
            InitializeComponent();
        }

        /// <summary>
        /// 콤보박스 초기 설정
        /// </summary>
        private void InitCombo()
        {
            CommonCombo _combo = new CommonCombo();
        }

        /// <summary>
        /// 컨트롤 초기값 설정
        /// </summary>
        private void InitControl()
        {

        }

        #endregion

        #region Event

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            ApplyPermissions();
            InitCombo();
            InitControl();
            SetEvent();
        }
        /// <summary>
        /// Initializing 이후에 FormLoad시 Event를 생성.
        /// </summary>
        private void SetEvent()
        {
            this.Loaded -= UserControl_Loaded;
        }

        #endregion        

        #region Mehod

        #region [BizCall]

        private void Search()
        {
            try
            {
                if (!Validation())
                    return;

                DataTable dt = new DataTable();
                dt.Columns.Add("SUBLOTID");
                dt.Columns.Add("USERID");
                dt.Columns.Add("LANGID");
                DataRow dr = dt.NewRow();
                dr["SUBLOTID"] = txtCellID.Text.Trim();
                dr["USERID"] = LoginInfo.USERID;
                dr["LANGID"] = LoginInfo.LANGID;
                dt.Rows.Add(dr);
                DataTable dtResult = new ClientProxy().ExecuteServiceSync("BR_PRD_GET_SUBLOT_INFO_NJ", "INDATA", "OUTDATA", dt);
                if (dtResult != null && dtResult.Rows.Count > 0)
                {
                    DataTable dtSource = DataTableConverter.Convert(dgResult.ItemsSource);
                    dtSource.Merge(dtResult);
                    Util.GridSetData(dgResult, dtSource, FrameOperation);
                    txtCellID.Focus();
                }
                else
                {
                    //SFU1209		Cell 정보가 없습니다.	
                    
                    ControlsLibrary.MessageBox.ShowNoEnter(MessageDic.Instance.GetMessage("SFU1209"), null, "Error", MessageBoxButton.OK, MessageBoxIcon.Warning, (result) =>
                    {
                        if (result == MessageBoxResult.OK)
                        {
                            txtCellID.Focus();
                            txtCellID.Text = string.Empty;
                        }
                    });
                    return;
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return;
            }
            
        }

        private bool Validation()
        {
            try
            {
                string sCellID = txtCellID.Text.Trim();

                if (dgResult.GetRowCount() > 0)
                {
                    DataTable dtInfo = DataTableConverter.Convert(dgResult.ItemsSource);
                    DataRow[] drList = dtInfo.Select("SUBLOTID = '" + sCellID + "'");

                    if (drList.Length > 0)
                    {
                        // SFU3159 아래쪽 List에 이미 존재하는 CELL ID입니다.
                        ControlsLibrary.MessageBox.ShowNoEnter(MessageDic.Instance.GetMessage("SFU3099"), null, "Error", MessageBoxButton.OK, MessageBoxIcon.Warning, (result) =>
                        {
                            if (result == MessageBoxResult.OK)
                            {
                                txtCellID.Focus();
                                txtCellID.Text = string.Empty;
                            }
                        });

                        txtCellID.Text = string.Empty;
                        //  txtCellID.Focus();
                        return false;
                    }
                    else
                        return true;
                }
                else
                {
                    return true;
                }

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return false;
            }
        }

        #endregion

        #region [PopUp Event]

        #endregion

        #region [Func]

        private void ApplyPermissions()
        {
            List<Button> listAuth = new List<Button>();

            Util.pageAuth(listAuth, FrameOperation.AUTHORITY);

            btnStore.IsEnabled = LoginInfo.LOGGEDBYSSO == true ? true : false;
        }

        #endregion

        #endregion

        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtCellID.Text))
            {
                return;
            }
            Search();
        }

        private void txtCellID_KeyDown(object sender, KeyEventArgs e)
        {
            if(e.Key == Key.Enter)
            {
                Search();
                txtCellID.Clear();
                txtCellID.Focus();
            }
        }

        private void btnReset_Click(object sender, RoutedEventArgs e)
        {
            if(dgResult.ItemsSource!=null)
            {
                dgResult.ItemsSource = null;
            }
        }

        private void txtCellID_GotFocus(object sender, RoutedEventArgs e)
        {
            InputMethod.Current.ImeConversionMode = ImeConversionModeValues.Alphanumeric;
        }

        private void btnStore_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if(dgResult.Rows.Count<1)
                {
                    //입고 대상이 없습니다.
                    Util.MessageValidation("SFU4560");
                    return;
                }

                DataSet ds = new DataSet();
                DataTable inData = ds.Tables.Add("INDATA");
                inData.Columns.Add("USERID");
                DataRow dr = inData.NewRow();
                dr["USERID"] = LoginInfo.USERID;
                inData.Rows.Add(dr);

                DataTable inSublot = ds.Tables.Add("INSUBLOT");
                inSublot.Columns.Add("SUBLOTID");

                for (int i = 0; i < dgResult.Rows.Count; i++)
                {
                    string cellID = Util.NVC(dgResult.GetCell(i, dgResult.Columns["SUBLOTID"].Index).Value);
                    dr = inSublot.NewRow();
                    dr["SUBLOTID"] = cellID;
                    inSublot.Rows.Add(dr);
                }

                new ClientProxy().ExecuteService_Multi("BR_PRD_REG_RETURN_SUBLOT_NJ", "INDATA,INSUBLOT", null, (result, ex) =>
                   {
                       if(ex!=null)
                       {
                           Util.MessageException(ex);
                           return;
                       }
                       Util.MessageInfo("SFU1798");

                   },ds);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return;
            }
        }
    }
}
