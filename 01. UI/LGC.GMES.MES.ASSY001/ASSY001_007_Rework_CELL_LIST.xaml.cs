/*************************************************************************************
 Created Date : 2017.08.16
      Creator : Narutech 김준필
   Decription : LGCWA GMES PJT - PKG공정 재작업 CELL 등록.
--------------------------------------------------------------------------------------
 [Change History]
  2017.08.16  Narutech 김준필 : Initial Created.
  
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

namespace LGC.GMES.MES.ASSY001
{
    /// <summary>
    /// ASSY001_007_Rework_CELL_LIST.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class ASSY001_007_Rework_CELL_LIST : C1Window, IWorkArea
    {
        #region Declaration & Constructor 

        DataTable gdtDT = null;
        BizDataSet _Biz = new BizDataSet();

        
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

        public ASSY001_007_Rework_CELL_LIST()
        {
            InitializeComponent();
        }

        private void InitializeControls()
        {
            try
            {
                gdtDT = new DataTable();
                gdtDT.Columns.Add("NO", typeof(string));
                gdtDT.Columns.Add("CELLID", typeof(string));

                dgCell.BeginEdit();
                dgCell.ItemsSource = DataTableConverter.Convert(gdtDT);
                dgCell.EndEdit();
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        #endregion

        #region Event
        private void C1Window_Loaded(object sender, RoutedEventArgs e)
        {
            object[] tmps = C1WindowExtension.GetParameters(this);

            
            

            ApplyPermissions();
            InitializeControls();



        }

        private void C1Window_Initialized(object sender, EventArgs e)
        {

        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            if (gdtDT.Rows.Count <= 0)
                return;

            //저장 하시겠습니까?
            Util.MessageConfirm("SFU1241", (result) =>
            {
                if (result == MessageBoxResult.OK)
                {
                    DataSet inDataSet = new DataSet();
                    DataTable dt = inDataSet.Tables.Add("RQSTDT");

                    dt.Columns.Add("SUBLOTID", typeof(string));
                    dt.Columns.Add("USERID", typeof(string));

                    for (int i = 0; i < gdtDT.Rows.Count; i++)
                    {
                        DataRow dr = dt.NewRow();
                        dr["SUBLOTID"] = gdtDT.Rows[i]["CELLID"].ToString();
                        dr["USERID"] = LoginInfo.USERID;

                        dt.Rows.Add(dr);
                    }
                    
                    new ClientProxy().ExecuteService_Multi("DA_PRD_UPD_SUBLOT_PKG_PROC_RWK_FLAG", "RQSTDT", null, (Result, ex) =>
                    {
                        if (ex != null)
                        {
                            Util.MessageException(ex);
                            return;
                        }

                        Util.MessageInfo("SFU1275");    //정상 처리 되었습니다.
                        //this.DialogResult = MessageBoxResult.OK;

                        btnReflash_Click(null, null);
                    }, inDataSet);
                }
            });
        }
        private void txtSublotID_KeyDown(object sender, KeyEventArgs e)
        {
            try
            {
                if (e.Key == Key.Enter)
                {
                    if (txtSublotID.Text.Length != 10)
                    {
                        Util.MessageInfo("Cell ID Error!");
                        txtSublotID.Text = "";
                        txtSublotID.Focus();
                        return;
                    }

                    if (CheckDup())     //중복 확인
                    {
                        Util.MessageInfo("SFU3159");  //아래쪽 List에 이미 존재하는 CELL ID입니다.
                        txtSublotID.Focus();
                        txtSublotID.SelectAll();
                        txtSublotID.Text = "";
                        return;
                    }

                    int iNo = gdtDT.Rows.Count + 1;
                    string sCellID = txtSublotID.Text.Trim();

                    DataRow dr = gdtDT.NewRow();

                    dr["NO"] = iNo.ToString();
                    dr["CELLID"] = sCellID;

                    gdtDT.Rows.Add(dr);

                    dgCell.ItemsSource = DataTableConverter.Convert(gdtDT);

                    txtSublotID.SelectAll();
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }

        }

        private void btnReflash_Click(object sender, RoutedEventArgs e)
        {
            gdtDT.Clear();
            txtSublotID.Text = "";

            dgCell.BeginEdit();
            dgCell.ItemsSource = DataTableConverter.Convert(gdtDT);
            dgCell.EndEdit();
        }

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = MessageBoxResult.Cancel;
        }

        private void btnDelete2_Click(object sender, RoutedEventArgs e)
        {
            try
            {

                int index = ((C1.WPF.DataGrid.DataGridCellPresenter)((sender as Button).Parent)).Row.Index;

                gdtDT.Rows.RemoveAt(index);

                for (int i = 0; i < gdtDT.Rows.Count; i++)
                {
                    gdtDT.Rows[i]["NO"] = (i + 1).ToString();
                }     

                dgCell.BeginEdit();
                dgCell.ItemsSource = DataTableConverter.Convert(gdtDT);
                dgCell.EndEdit();

                txtSublotID.Focus();
                txtSublotID.SelectAll();
                
            }
            catch(Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        


        #endregion

        #region Mehod
        #endregion
        #region[BizRule]




        #endregion

        #region [Validation]
        private bool CheckDup()
        {
            try
            {
                bool bDup = false;
                DataRow[] dr = gdtDT.Select("CELLID='" + txtSublotID.Text + "'");

                bDup = (dr.Length > 0);

                return bDup;
            }
            catch (Exception ex)
            {
                return true;
                Util.MessageException(ex);
            }
        }
        #endregion

        #region[Func]
        private DataTable MadeDataTable()
        {
            DataTable dt = (dgCell.ItemsSource as DataView).Table;

            return dt;
        }

        public void ShowLoadingIndicator()
        {
            //if (loadingIndicator != null)
            //    loadingIndicator.Visibility = Visibility.Visible;
        }

        public void HiddenLoadingIndicator()
        {
            //if (loadingIndicator != null)
            //    loadingIndicator.Visibility = Visibility.Collapsed;
        }

        private void ApplyPermissions()
        {
            List<Button> listAuth = new List<Button>();
            listAuth.Add(btnSave);
            


            Util.pageAuth(listAuth, FrameOperation.AUTHORITY);
        }


        #endregion
        

        
    }
}
