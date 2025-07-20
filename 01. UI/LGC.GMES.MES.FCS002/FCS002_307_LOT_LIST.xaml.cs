/*************************************************************************************
 Created Date : 2019.11.11
      Creator : 최상민
   Decription : 전지 5MEGA-GMES 구축 - 출하HOLD 관리 - NCR HOLD(보류재고) 등록 팝업
--------------------------------------------------------------------------------------
 [Change History]

  2019-11-18 최상민 : GMES Hold, QMS Hold건 보류재고 등록 기능 상세 lot, Cell 정보 확인 팝업
                     C20191104-000168 + GMES 상 보류재고 관리 시스템 개발(9월 요청 건) +  2200
  2023.03.13  LEEHJ     : 소형활성화 MES 복사
**************************************************************************************/

using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Data;

using C1.WPF;
using C1.WPF.DataGrid;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.ControlsLibrary;
using System.ComponentModel;
using System.Threading;
using System.Linq;
using C1.WPF.DataGrid.Summaries;
using System.Windows.Media;
using static LGC.GMES.MES.CMM001.Class.CommonCombo;
using LGC.GMES.MES.CMM001.Popup;
using System.Configuration;
using Microsoft.Win32;
using System.IO;
using C1.WPF.Excel;

namespace LGC.GMES.MES.FCS002
{
    /// <summary>
    /// BOX001_201_RUNSTART.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class FCS002_307_LOT_LIST : C1Window, IWorkArea
    {
        #region Declaration & Constructor 

        string _hold_GR_id = string.Empty;

        C1.WPF.DataGrid.DataGridRowHeaderPresenter pre = new C1.WPF.DataGrid.DataGridRowHeaderPresenter()
        {
            Background = new SolidColorBrush(Colors.Transparent),
            MouseOverBrush = new SolidColorBrush(Colors.Transparent),
        };

        CheckBox chkAll = new CheckBox()
        {
            IsChecked = false,
            Background = new SolidColorBrush(Colors.Transparent),
            VerticalAlignment = System.Windows.VerticalAlignment.Center,
            HorizontalAlignment = System.Windows.HorizontalAlignment.Center

        };

        public IFrameOperation FrameOperation
        {
            get;
            set;
        }

        public FCS002_307_LOT_LIST()
        {
            InitializeComponent();
            Loaded += FCS002_307_LOT_LIST_Loaded;
        }

        private void FCS002_307_LOT_LIST_Loaded(object sender, RoutedEventArgs e)
        {
            Loaded -= FCS002_307_LOT_LIST_Loaded;           

            object[] tmps = C1WindowExtension.GetParameters(this);
            _hold_GR_id = tmps[0] as string;

            getLotIDInfo(_hold_GR_id);
        }
        #endregion

        #region Initialize

        private void C1Window_Loaded(object sender, RoutedEventArgs e)
        {
            InitControl();
            InitCombo();
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
        }

        private void dgLotList_LoadedColumnHeaderPresenter(object sender, C1.WPF.DataGrid.DataGridColumnEventArgs e)
        {
            try
            {
                this.Dispatcher.BeginInvoke(new Action(() =>
                {
                   
                }));
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return;
            }
        }

      
        #endregion

        #region Validation
        private void dgLotList_CommittedEdit(object sender, DataGridCellEventArgs e)
        {
            C1.WPF.DataGrid.C1DataGrid dataGrid = sender as C1.WPF.DataGrid.C1DataGrid;

            if (e.Cell.Column.Name == "HOLD_REG_QTY")
            {
                string hold_req_qty = Util.NVC(dataGrid.GetCell(e.Cell.Row.Index, dataGrid.Columns["HOLD_REG_QTY"].Index).Value);
                int iHold_req_qty;

                if (!string.IsNullOrWhiteSpace(hold_req_qty) && !int.TryParse(hold_req_qty, out iHold_req_qty))
                {
                    //SFU3435	숫자만 입력해주세요
                    Util.MessageInfo("SFU3435");
                }
            }
            
        }

        private void getLotIDInfo(string sHoldGRid)
        {
            try
            {

                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";                
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("HOLD_GR_ID", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["HOLD_GR_ID"] = sHoldGRid;
                RQSTDT.Rows.Add(dr);

                

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_TB_SFC_ASSY_LOT_HOLD_GR_LIST", "RQSTDT", "OUTDATA", RQSTDT);               

                //txtTotalSQty.Text = Convert.ToString(dtResult.Rows.Count);

                Util.GridSetData(dgLotList, dtResult, FrameOperation, true);

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        #endregion       

        #region 닫기 버튼 이벤트       

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = MessageBoxResult.Cancel;
            this.Close();
        }
        #endregion

       


        private void dgLotList_BeginningEdit(object sender, C1.WPF.DataGrid.DataGridBeginningEditEventArgs e)
        {
            C1.WPF.DataGrid.C1DataGrid dataGrid = sender as C1.WPF.DataGrid.C1DataGrid;

            //if (e.Column.Name == "STRT_SUBLOTID" || e.Column.Name == "END_SUBLOTID")
            //{
            //    string sLotType = Util.NVC(dataGrid.GetCell(e.Row.Index, dataGrid.Columns["HOLD_TRGT_CODE"].Index).Value);               

            //    if (string.IsNullOrWhiteSpace(sLotType))
            //    {
            //        //	SFU4349		보류범위 먼저 선택하세요.	
            //        Util.MessageValidation("SFU4349");
            //        e.Cancel = true;
            //    }
            //    else if (sLotType == "LOT")
            //        e.Cancel = true;               
            //}
        }
       
    }
}
