/*************************************************************************************
 Created Date : 2019.10.04
      Creator : Lee sang jun
   Decription : 전지 5MEGA-GMES 구축 - 소형조립 공정진척(Assembly용) Washing Lot 으로 등록된 마지막 Cell ID 조회 
--------------------------------------------------------------------------------------
 [Change History]
  2019.10.21  이상준 : Initial Created.
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

namespace LGC.GMES.MES.CMM001
{
    /// <summary>
    /// CMM_ASSY_CELL_NO_LAST.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class CMM_ASSY_CELL_NO_LAST : C1Window, IWorkArea
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
        public CMM_ASSY_CELL_NO_LAST()
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
                    txtLotID.Text = Util.NVC(tmps[0]);
                    SearchLastCellNo();
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }

        }

        private void txtLotID_KeyUp(object sender, KeyEventArgs e)
        {
            try
            {
                txtPrefix.Text = string.Empty;
                txtLastCellNo.Text = string.Empty;

                if (e.Key == Key.Enter)
                {
                    SearchLastCellNo();
                }

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                SearchLastCellNo();
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

        private void SearchLastCellNo()
        {
            try
            {
                ShowLoadingIndicator();

                //Prefix 정보 조회 
                DataTable inTable = new DataTable();
                inTable.Columns.Add("PROD_LOTID", typeof(string));

                DataRow newRow = inTable.NewRow();
                newRow["PROD_LOTID"] = txtLotID.Text.Trim();
                inTable.Rows.Add(newRow);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("BR_PRD_GET_SUBLOT_PRFX_WS_CELLID", "RQSTDT", "RSLTDT", inTable);

                if (dtResult != null && dtResult.Rows.Count > 0)
                    txtPrefix.Text = dtResult.Rows[0]["CELL_ID_PRFX"].ToString();


                //마지막 Cell ID 조회 
                if (!txtPrefix.Text.Equals(""))
                {
                    DataTable inTable2 = new DataTable();
                    inTable2.Columns.Add("CELL_ID_PRFX", typeof(string));
                    inTable2.Columns.Add("PROD_LOTID", typeof(string));

                    DataRow newRow2 = inTable2.NewRow();
                    newRow2["CELL_ID_PRFX"] = txtPrefix.Text.Trim();
                    newRow2["PROD_LOTID"] = txtLotID.Text.Trim();
                    inTable2.Rows.Add(newRow2);

                    DataTable dtResult2 = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_SUBLOT_LAST_ID", "INDATA", "OUTDATA", inTable2);

                    if (dtResult2 != null && dtResult2.Rows.Count > 0)
                        txtLastCellNo.Text = dtResult2.Rows[0]["LAST_SUBLOTID"].ToString();
                }
                else
                {
                    Util.MessageValidation("SFU8113");//Prefix 정보가 없습니다.
                    return;
                }

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
