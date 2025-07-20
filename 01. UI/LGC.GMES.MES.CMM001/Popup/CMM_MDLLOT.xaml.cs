/*************************************************************************************
 Created Date : 2017.05.31
      Creator : 이슬아D
   Decription : 전지 5MEGA-GMES 구축 - 특성실적관리 화면 - 작업시작 팝업
--------------------------------------------------------------------------------------
 [Change History]
  2017.05.31  이슬아D : 최초생성
  
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

namespace LGC.GMES.MES.CMM001.Popup
{

    public partial class CMM_MDLLOT : C1Window, IWorkArea
    {
        #region Declaration & Constructor 

        public string PRODID
        {
            get;
            set;
        }

        public string MDLLOT_ID
        {
            get;
            set;
        }

        public string PRJT_NAME
        {
            get;
            set;
        }

        public CMM_MDLLOT()
        {
            InitializeComponent();
        }

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


        /// <summary>
        /// 화면내 버튼 권한 처리
        /// </summary>
        private void ApplyPermissions()
        {
            List<Button> listAuth = new List<Button>();
            //listAuth.Add(btnInReplace);
            Util.pageAuth(listAuth, FrameOperation.AUTHORITY);
        }

        /// <summary>
        /// 콤보박스 초기 설정
        /// </summary>
        private void InitCombo()
        {
            CommonCombo _combo = new CommonCombo();
            _combo.SetCombo(cboArea, CommonCombo.ComboStatus.SELECT, sCase: "ALLAREA");
        }

        /// <summary>
        /// 컨트롤 초기값 설정
        /// </summary>
        private void InitControl()
        {
            object[] tmps = C1WindowExtension.GetParameters(this);
            if(tmps.Count() > 1) cboArea.SelectedValue = tmps[0];
        }
        #endregion

        #region Event

        private void C1Window_Loaded(object sender, RoutedEventArgs e)
        {
            InitCombo();
            InitControl();    
            //Search();
        }

        private void dgResult_LoadedCellPresenter(object sender, DataGridCellEventArgs e)
        {
            try
            {
                C1.WPF.DataGrid.C1DataGrid dataGrid = sender as C1.WPF.DataGrid.C1DataGrid;

                dataGrid.Dispatcher.BeginInvoke(new Action(() =>
                {
                    if (e.Cell.Presenter == null)
                    {
                        return;
                    }
                    else if (e.Cell.Column.Name == "PRODID")
                    {
                        e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Blue);
                    }
                    else
                    {
                        e.Cell.Presenter.Foreground = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#FF333333"));
                    }
                }));
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }

        }

        private void dgResult_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            try
            {
                C1.WPF.DataGrid.C1DataGrid datagrid = (sender as C1.WPF.DataGrid.C1DataGrid);

                Point pnt = e.GetPosition(null);
                C1.WPF.DataGrid.DataGridCell cell = datagrid.GetCellFromPoint(pnt);

                if (cell == null || datagrid.CurrentRow == null)
                {
                    return;
                }

                if (datagrid.CurrentColumn.Name == "PRODID")
                {
                    PRODID = cell.Text;
                    MDLLOT_ID = dgResult.GetCell(datagrid.CurrentRow.Index, dgResult.Columns["MDLLOT_ID"].Index).Text;
                    PRJT_NAME = dgResult.GetCell(datagrid.CurrentRow.Index, dgResult.Columns["PRJT_NAME"].Index).Text;

                    this.DialogResult = MessageBoxResult.OK;
                    this.Close();
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void txtSearchText_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                Search();

                if (dgResult.Rows.Count == 1)
                {
                    PRODID = dgResult.GetCell(0, dgResult.Columns["PRODID"].Index).Text;
                    MDLLOT_ID = dgResult.GetCell(0, dgResult.Columns["MDLLOT_ID"].Index).Text;
                    PRJT_NAME = dgResult.GetCell(0, dgResult.Columns["PRJT_NAME"].Index).Text;
                    this.DialogResult = MessageBoxResult.OK;
                    this.Close();
                }
            }
        }

        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            Search();
        }
        #endregion

        #region Mehod
        /// <summary>
        /// 조회
        /// </summary>
        private void Search()
        {
            try
            {               
                DataTable RQSTDT = new DataTable();
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("AREAID", typeof(string));
                RQSTDT.Columns.Add("SEARCHTEXT", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["AREAID"] = (string)cboArea.SelectedValue;
                dr["SEARCHTEXT"] = string.IsNullOrWhiteSpace(txtSearchText.Text)? null : txtSearchText.Text;
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_PRODUCT_INFO_FOR_SEARCH", "RQSTDT", "RSLTDT", RQSTDT);

                Util.GridSetData(dgResult, dtResult, FrameOperation);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        #endregion

      
    }
}
