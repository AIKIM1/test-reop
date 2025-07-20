/*************************************************************************************
 Created Date : 2016.08.09
      Creator : INS 김동일K
   Decription : 전지 5MEGA-GMES 구축 - 공정진척화면의 작업일지 공통 화면
--------------------------------------------------------------------------------------
 [Change History]
  2016.08.09  INS 김동일K : Initial Created.
  
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
using System.Reflection;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.ControlsLibrary;

namespace LGC.GMES.MES.ProtoType04
{
    /// <summary>
    /// PGM_GUI_196_WORKORDER.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class PGM_GUI_196_WORKORDER : UserControl, IWorkArea
    {
        #region Declaration & Constructor
        DataTable dtMain = new DataTable();
        DataRow newRow = null;



        
        private string _LineID = string.Empty;
        private string _EqptID = string.Empty;
        //public PGM_GUI_196 PGM_GUI_196;     // Stacking
        public UserControl _UCParent;     // Caller

        public string LINEID
        {
            get { return _LineID; }
            set { _LineID = value; }
        }

        public string EQPTID
        {
            get { return _EqptID; }
            set { _EqptID = value; }
        }
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

        public PGM_GUI_196_WORKORDER()
        {
            InitializeComponent();
        }

        public PGM_GUI_196_WORKORDER(string sLineID, string sEqptID)
        {
            InitializeComponent();

            LINEID = sLineID;
            EQPTID = sEqptID;
        }

        private void InitializeWorkorderQuantityInfo()
        {
            txtBlockPlanQty.Text = "0";
            txtBlockOutQty.Text = "0";
            txtBlockRemainQty.Text = "0";
        }

        #endregion

        #region Event
        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            ApplyPermissions();

            InitializeWorkorderQuantityInfo();

            GetWorkOrder();
        }

        private void dgWorkOrder_SelectionChanged(object sender, C1.WPF.DataGrid.DataGridSelectionChangedEventArgs e)
        {
            //// Main 실적 조회 처리.
            //if (PGM_GUI_196 == null)
            //    return;

            //if (dgWorkOrder.ItemsSource == null || dgWorkOrder.CurrentRow.DataItem == null)
            //    return;

            //PGM_GUI_196.GetProductLot("aa");
        }

        private void dgWorkOrder_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            // Change Work Order...
            if (_UCParent == null)
                return;


            if (dgWorkOrder.CurrentRow.Index < 0)
                return;

            string sWorkOrder = DataTableConverter.GetValue(dgWorkOrder.Rows[dgWorkOrder.CurrentRow.Index].DataItem, "WORKORDER").ToString();
            string sMoveOrder = DataTableConverter.GetValue(dgWorkOrder.Rows[dgWorkOrder.CurrentRow.Index].DataItem, "MOVEORDER").ToString();

            if (!CanChangeWorkOrder(dgWorkOrder.CurrentRow.Index))
                return;

            
            LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("작업지시를 변경 하시겠습니까?"), null, "Information", MessageBoxButton.OKCancel, MessageBoxIcon.None, (result) =>
            {
                if (result == MessageBoxResult.OK)
                {
                    WorkOrderChange(sWorkOrder);
                    //SetWorkOrderQtyInfo(sWorkOrder);
                }
            });
        }
        #endregion

        #region Mehod
        private void ApplyPermissions()
        {
            List<Button> listAuth = new List<Button>();
            //listAuth.Add(btnInReplace);
            //listAuth.Add(btnInReplace);

            Util.pageAuth(listAuth, FrameOperation.AUTHORITY);
        }

        private bool CanChangeWorkOrder(int iRow)
        {
            bool bRet = true;

            if (iRow < 0)
                bRet = false;

            // 대기 중인 작업지시 인지..
            //if (!DataTableConverter.GetValue(dgWorkOrder.Rows[iRow].DataItem, "WDSTATUS").ToString().Equals("대기"))    // 하드코딩 변경 필요..
            //{
            //    LGC.GMES.MES.ControlsLibrary.MessageBox.Show("대기 작업지시가 아닙니다.", null, "Info", MessageBoxButton.OK);

            //    bRet = false;
            //}

            // 현 작업중인 실적 중 확정 처리 안된 실적 존재 확인.


            // 작업중인 작업지시 중 수량이 0이 아닌지 확인 필요..????

            
            return bRet;
        }

        private void WorkOrderChange(string sWorkOrder)
        {
            // Biz Call...


            // 재조회
            GetWorkOrder(sWorkOrder);

            SearchProductLot(GetWorkOrderInfo(sWorkOrder));
        }

        //private void SetWorkOrderQtyInfo()
        //{
        //    InitializeWorkorderQuantityInfo();

        //    if (dgWorkOrder.Rows.Count < 1)
        //        return;


        //    for (int i = 0; i < dgWorkOrder.Rows.Count; i++)
        //    {
        //        if (DataTableConverter.GetValue(dgWorkOrder.Rows[i].DataItem, "WDSTATUS").ToString().Equals("작업중")) // 하드코딩 삭제 필요...
        //        {
        //            if (DataTableConverter.GetValue(dgWorkOrder.Rows[i].DataItem, "PLANQTY").GetType() == typeof(Int32) &&
        //                DataTableConverter.GetValue(dgWorkOrder.Rows[i].DataItem, "OUTQTY").GetType() == typeof(Int32))
        //            {
        //                txtBlockPlanQty.Text = string.Format("{0:n0}", Int32.Parse(DataTableConverter.GetValue(dgWorkOrder.Rows[i].DataItem, "PLANQTY").ToString()));
        //                txtBlockOutQty.Text = string.Format("{0:n0}", Int32.Parse(DataTableConverter.GetValue(dgWorkOrder.Rows[i].DataItem, "OUTQTY").ToString()));
        //                txtBlockRemainQty.Text = string.Format("{0:n0}", Int32.Parse(DataTableConverter.GetValue(dgWorkOrder.Rows[i].DataItem, "PLANQTY").ToString())
        //                    - Int32.Parse(DataTableConverter.GetValue(dgWorkOrder.Rows[i].DataItem, "OUTQTY").ToString()));
        //            }
        //            break;
        //        }
        //    }
        //}

        private void SetWorkOrderQtyInfo(DataRow[] dataRow)
        {
            InitializeWorkorderQuantityInfo();

            //if (dgWorkOrder.Rows.Count < 1)
            //    return;


            //for (int i = 0; i < dgWorkOrder.Rows.Count; i++)
            //{
            //    if (DataTableConverter.GetValue(dgWorkOrder.Rows[i].DataItem, "WORKORDER").ToString().Equals(sWorkOrder))
            //    {
            //        if (DataTableConverter.GetValue(dgWorkOrder.Rows[i].DataItem, "PLANQTY").GetType() == typeof(Int32) &&
            //            DataTableConverter.GetValue(dgWorkOrder.Rows[i].DataItem, "OUTQTY").GetType() == typeof(Int32))
            //        {
            //            txtBlockPlanQty.Text = string.Format("{0:n0}", Int32.Parse(DataTableConverter.GetValue(dgWorkOrder.Rows[i].DataItem, "PLANQTY").ToString()));
            //            txtBlockOutQty.Text = string.Format("{0:n0}", Int32.Parse(DataTableConverter.GetValue(dgWorkOrder.Rows[i].DataItem, "OUTQTY").ToString()));
            //            txtBlockRemainQty.Text = string.Format("{0:n0}", Int32.Parse(DataTableConverter.GetValue(dgWorkOrder.Rows[i].DataItem, "PLANQTY").ToString())
            //                - Int32.Parse(DataTableConverter.GetValue(dgWorkOrder.Rows[i].DataItem, "OUTQTY").ToString()));
            //        }
            //        break;
            //    }
            //}

            //string expression = "WORKORDER = '" + sWorkOrder + "'";
            //DataRow[] foundRows;

            //foundRows = dtMain.Select(expression);

            if (dataRow == null)
                return;


            for (int i = 0; i < dataRow.Length; i++)
            {
                if (dataRow[i]["PLANQTY"].GetType() == typeof(Int32) && dataRow[i]["OUTQTY"].GetType() == typeof(Int32))
                {
                    txtBlockPlanQty.Text = string.Format("{0:n0}", Int32.Parse(dataRow[i]["PLANQTY"].ToString()));
                    txtBlockOutQty.Text = string.Format("{0:n0}", Int32.Parse(dataRow[i]["OUTQTY"].ToString()));
                    txtBlockRemainQty.Text = string.Format("{0:n0}", Int32.Parse(dataRow[i]["PLANQTY"].ToString())
                        - Int32.Parse(dataRow[i]["OUTQTY"].ToString()));

                    break;
                }
            }
        }

        public void GetWorkOrder(string sWorkOrder = "")
        {
            if (LINEID.Length < 1 || EQPTID.Length < 1)
                return;

            dgWorkOrder.ItemsSource = null;


            dtMain = new DataTable();
            dtMain.Columns.Add("RANKING", typeof(Int32));
            dtMain.Columns.Add("PRODID", typeof(string));
            dtMain.Columns.Add("PLANQTY", typeof(Int32));
            dtMain.Columns.Add("OUTQTY", typeof(Int32));
            dtMain.Columns.Add("WDTYPE", typeof(string));
            dtMain.Columns.Add("WDSTATUS", typeof(string));
            dtMain.Columns.Add("MOVEORDER", typeof(string));
            dtMain.Columns.Add("WORKORDER", typeof(string));

            //Random rnd = new Random();

            //if (rnd.Next(0, 2) > 0)
            //{
                newRow = dtMain.NewRow();
                newRow.ItemArray = new object[] { 1, "MBEV3601AM", 1000, 200, "양산", "작업중", "M0010", "2886316" };
                dtMain.Rows.Add(newRow);

                newRow = dtMain.NewRow();
                newRow.ItemArray = new object[] { 2, "MBEV3601AP", 2000, 1253, "양산", "대기", "MO0020", "2886317" };
                dtMain.Rows.Add(newRow);

                newRow = dtMain.NewRow();
                newRow.ItemArray = new object[] { 3, "MBEV3801AB", 1000000, 9856, "테스트", "대기", "M9000", "2886186" };
                dtMain.Rows.Add(newRow);

                newRow = dtMain.NewRow();
                newRow.ItemArray = new object[] { 4, "MBSLURRYAA2", 10000, 0, "시생산", "대기", "M2011", "V4-1_1000A" };
                dtMain.Rows.Add(newRow);
            //}
            //else
            //{
            //    newRow = dtMain.NewRow();
            //    newRow.ItemArray = new object[] { 1, "MBSLURRYAA2", 250000, 12242, "양산", "작업중", "V4-2_1250A", "0010" };
            //    dtMain.Rows.Add(newRow);

            //    newRow = dtMain.NewRow();
            //    newRow.ItemArray = new object[] { 3, "MBEV3801AB", 1000000, 0, "테스트", "대기", "M9000", "2886186" };
            //    dtMain.Rows.Add(newRow);
            //}

            dgWorkOrder.ItemsSource = DataTableConverter.Convert(dtMain);
                                    
            SetWorkOrderQtyInfo(GetWorkOrderInfo(sWorkOrder));
        }

        public DataRow[] GetWorkOrderInfo(string sWorkOrder = "")
        {
            string expression = string.Empty;

            DataTable dtTemp;
            DataRow[] foundRows;

            if (dgWorkOrder.ItemsSource == null || dgWorkOrder.Rows.Count < 1)
                return null;


            dtTemp = DataTableConverter.Convert(dgWorkOrder.ItemsSource);

            // Running 중인 Workorder 찾기.
            if (sWorkOrder.Equals(""))
                sWorkOrder = FindRunningWorkOrderNumber();
            
            expression = "WORKORDER = '" + sWorkOrder + "'";

            foundRows = dtTemp.Select(expression);

            return foundRows;
        }

        private string FindRunningWorkOrderNumber()
        {
            string sRet = string.Empty;
            DataTable dtTemp;
            DataRow[] foundRows;

            if (dgWorkOrder.ItemsSource == null || dgWorkOrder.Rows.Count < 1)
                return "";
            
            dtTemp = DataTableConverter.Convert(dgWorkOrder.ItemsSource);

            foundRows = dtTemp.Select("WDSTATUS = '작업중'");  // 하드코딩 삭제 필요....

            if (foundRows.Length > 0)
                sRet = foundRows[0]["WORKORDER"].ToString();

            return sRet;
        }

        private void SearchProductLot(DataRow[] drRunWorkOrder)
        {
            // Main 실적 조회 처리.
            if (_UCParent == null)
                return;

            if (dgWorkOrder.ItemsSource == null || dgWorkOrder.CurrentRow.DataItem == null)
                return;

            if (drRunWorkOrder != null && drRunWorkOrder.Length > 0)
            {
                //PGM_GUI_196.SearchProductLot(drRunWorkOrder[0]);

                Type t = _UCParent.GetType();
                MethodInfo sayHelloMethod = t.GetMethod("GetProductLot");
                sayHelloMethod.Invoke(_UCParent, drRunWorkOrder);
            }
        }
        #endregion


    }
}
