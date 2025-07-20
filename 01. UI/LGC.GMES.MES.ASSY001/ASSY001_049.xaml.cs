/*************************************************************************************
 Created Date : 2023.03.21
      Creator : 신명섭
   Decription : 본 화면은 Batch Lot 조회시 샘플 검사 대상 랏과 같은 배치에 포함된 랏 검색하는 화면입니다.
--------------------------------------------------------------------------------------
 [Change History]
  2023.03.21  DEVELOPER : Initial Created.
  2023.06.21  유재기    : NND END DATE Format 변경 Date -> Date Time




 
**************************************************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
using C1.WPF;
using LGC.GMES.MES.Common;
using System.Net;
using System.Reflection;
using System.Collections;
using System.Globalization;
using LGC.GMES.MES.CMM001;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.ControlsLibrary;
using C1.WPF.DataGrid.Summaries;

namespace LGC.GMES.MES.ASSY001
{
    public partial class ASSY001_049 : UserControl, IWorkArea
    {
        #region Declaration & Constructor 
        
        public ASSY001_049()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Frame과 상호작용하기 위한 객체
        /// </summary>
        public IFrameOperation FrameOperation
        {
            get;
            set;
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            ApplyPermissions();
            InitCombo();
            InitControl();
            InitGrid();
            SetEvent();
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
        }

        /// <summary>
        /// 컨트롤 초기값 설정
        /// </summary>
        private void InitControl()
        {
            //txtLotID.Text = "AD2QF077";
            //txtCSTID.Text = "";
        }

        /// <summary>
        /// C20180330_49835 화면 출력 대상 제어
        /// </summary>
        private void InitGrid()
        {

        }

        #endregion

        #region Event
        /// <summary>
        /// Initializing 이후에 FormLoad시 Event를 생성.
        /// </summary>
        private void SetEvent()
        {
            this.Loaded -= UserControl_Loaded;
        }


        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            Search();
        }



        private void dgSearchResult_LoadedCellPresenter(object sender, C1.WPF.DataGrid.DataGridCellEventArgs e)
        {
            try
            {
                //C1.WPF.DataGrid.C1DataGrid dataGrid = sender as C1.WPF.DataGrid.C1DataGrid;

                //dataGrid.Dispatcher.BeginInvoke(new Action(() =>
                //{
                //    if (e.Cell.Presenter == null)
                //    {
                //        return;
                //    }
                //}));

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
		#endregion

		#region Method
		/// <summary>
		/// 조회
		/// BIZ : DA_PRD_QA_SMPL_INSP_TRGT_LOT
		/// </summary>
		private void Search()
        {
            try
            {
                Util.gridClear(this.dgSearchResult);

                DataTable RQSTDT = new DataTable();
				RQSTDT.Columns.Add("LANGID", typeof(string));
				RQSTDT.Columns.Add("AREAID", typeof(string));
				RQSTDT.Columns.Add("LOTID", typeof(string));
                RQSTDT.Columns.Add("CSTID", typeof(string));

                DataRow dr = RQSTDT.NewRow();
				dr["LANGID"] = LoginInfo.LANGID;                // en-US
				dr["AREAID"] = LoginInfo.CFG_AREA_ID;	// "AB" : ESGM

                if (!string.IsNullOrEmpty(txtLotID.Text))
				{
					dr["LOTID"] = txtLotID.Text.Trim();
				}
                //else 
                if (!string.IsNullOrEmpty(txtCSTID.Text))
				{
					dr["CSTID"] = txtCSTID.Text.Trim();
				}

				RQSTDT.Rows.Add(dr);

                if (string.IsNullOrEmpty(txtLotID.Text) && string.IsNullOrEmpty(txtCSTID.Text))
                {
                    //조회된 Data가 없습니다.
                    Util.MessageInfo("SFU3537");
                    loadingIndicator.Visibility = Visibility.Collapsed;
                    return;
                }

                string bizName = string.Empty;

				bizName = "DA_PRD_QA_SMPL_INSP_TRGT_LOT";
				new ClientProxy().ExecuteService(bizName, "RQSTDT", "RSLTDT", RQSTDT, (searchResult, searchException) =>
				{
					try
					{
						if (searchException != null)
						{
							Util.MessageException(searchException);
							return;
						}
                        if (searchResult == null || searchResult.Rows.Count == 0)
                        {
                            //조회된 Data가 없습니다.
                            Util.MessageInfo("SFU3537");
                            return;
                        }
                        Util.GridSetData(dgSearchResult, searchResult, FrameOperation, true);
					}
					catch (Exception ex)
					{
						Util.MessageException(ex);

					}
					finally
					{
						loadingIndicator.Visibility = Visibility.Collapsed;
					}
				}
              );              
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
            finally
            {
               // loadingIndicator.Visibility = Visibility.Collapsed;
            }
        }

     

        #endregion

        private void btn_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            loadingIndicator.Visibility = Visibility.Visible;
        }

        private void dgSearchResult_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            try
            {
                C1.WPF.DataGrid.C1DataGrid datagrid = (sender as C1.WPF.DataGrid.C1DataGrid);

                Point pnt = e.GetPosition(null);
                C1.WPF.DataGrid.DataGridCell cell = datagrid.GetCellFromPoint(pnt);

                if (cell == null || datagrid.CurrentRow == null)
                    return;
			}
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

		private void txtLotID_KeyDown(object sender, KeyEventArgs e)
		{
			if (e.Key == Key.Enter)
			{
				Search();
			}
		}

        private void txtCSTID_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                Search();
            }
        }
    }
}
