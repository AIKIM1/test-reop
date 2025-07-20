/*************************************************************************************
 Created Date : 2023.01.06
      Creator : 이태규
   Decription : 반품사유 수정 팝업
--------------------------------------------------------------------------------------
 [Change History]
     Date         Author      CSR                    Description...
  2023.01.06      이태규 :                           Initial Created.
***************************************************************************************/
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using C1.WPF;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using LGC.GMES.MES.CMM001.Extensions; 
using C1.WPF.DataGrid;
using System.Threading.Tasks;
using System.Windows.Threading;
using System.Windows.Media.Animation;
using LGC.GMES.MES.PACK001.Class;
using System.IO;
using System.Windows.Controls.Primitives;



namespace LGC.GMES.MES.PACK001
{
    public partial class PACK003_038_POPUP : C1Window, IWorkArea
    {
        #region #. Member Variable Lists...
        LGC.GMES.MES.PACK001.Class.MESSAGE_PARAM ms = new LGC.GMES.MES.PACK001.Class.MESSAGE_PARAM();
        Util util = new Util();
        DataTable dtMain = new DataTable(); 
        /// <summary>
        /// Frame과 상호작용하기 위한 객체
        /// </summary>
        public IFrameOperation FrameOperation
        {
            get;
            set;
        }
        #endregion

        #region #. Declaration & Constructor
        public PACK003_038_POPUP()
        {
            InitializeComponent();
        }
        private void C1Window_Initialized(object sender, EventArgs e)
        {            

        }
        private void C1Window_Loaded(object sender, RoutedEventArgs e)
        {
            //파라메터 등록
            object[] tmps = C1WindowExtension.GetParameters(this);
            dtMain = (DataTable)tmps[0];
            grdMain.ItemsSource = dtMain.DefaultView;
        }        
        #endregion

        #region #. Event Lists...
        

        #region 조회 버튼
        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            grdMain.ItemsSource = dtMain.DefaultView;
        }
        #endregion

        #region 요청 버튼
        private void btnRTN_NOTE_Click(object sender, RoutedEventArgs e)
        {
            LGC.GMES.MES.ControlsLibrary.MessageBox.Show(ms.AlertRetun("SUF9012"), null, "Information", MessageBoxButton.OKCancel, MessageBoxIcon.None, (sResult) =>
            //{0} 판정 하시겠습니까 ? 
            {
                if (sResult == MessageBoxResult.OK)
                {
                    SaveProcess(dtMain);
                }

            });
        }
        #endregion

        #endregion
        

        #region #. Member Function Lists...

        #region t1 NumericColumn
        private void grdMain_BeginningRowEdit(object sender, DataGridEditingRowEventArgs e)
        {
            if (((DataGridNumericColumn)grdMain.CurrentCell.Column).ActualFilterMemberPath == "COUNT")
            {
                ((DataGridNumericColumn)grdMain.CurrentCell.Column).Minimum = 1;
                ((DataGridNumericColumn)grdMain.CurrentCell.Column).Maximum = double.Parse(grdMain.GetDataRow().ItemArray[11].ToString());
            }
        }
        #endregion

        #region Biz    
        private void SaveProcess(DataTable dt)
        {
            try
            {
                string bizRuleName = "BR_MTRL_UPD_PROD_RACK_MTRL_BOX_STCK";
                DataTable dtIN_DATA = new DataTable("INDATA");

                dtIN_DATA.Columns.Add("LANGID", typeof(string));
                dtIN_DATA.Columns.Add("REQ_NO", typeof(string));
                dtIN_DATA.Columns.Add("RTN_NOTE", typeof(string));
                dtIN_DATA.Columns.Add("USERID", typeof(string));

                foreach (DataRowView drv in dt.AsDataView())
                {
                    DataRow drIN_DATA = dtIN_DATA.NewRow();
                    drIN_DATA["LANGID"] = LoginInfo.LANGID;
                    drIN_DATA["REQ_NO"] = drv["REQ_NO"].ToString();
                    drIN_DATA["RTN_NOTE"] = txtRTN_NOTE.Text;
                    drIN_DATA["USERID"] = LoginInfo.USERID;
                    dtIN_DATA.Rows.Add(drIN_DATA);
                }
                new ClientProxy().ExecuteService(bizRuleName, dtIN_DATA.TableName, null, dtIN_DATA, (result, ex) =>
                {
                    loadingIndicator.Visibility = Visibility.Collapsed;

                    if (ex != null)
                    {
                        Util.MessageException(ex);
                        return;
                    }

                    Util.AlertInfo("PSS9072"); // 처리 완료
                    this.Close();
                });
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        
        #endregion

        #endregion


    }
}