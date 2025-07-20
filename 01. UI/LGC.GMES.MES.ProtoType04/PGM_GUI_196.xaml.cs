/*************************************************************************************
 Created Date : 2016.08.03
      Creator : INS 김동일K
   Decription : 전지 5MEGA-GMES 구축 - Stacking 공정진척 화면
--------------------------------------------------------------------------------------
 [Change History]
  2016.08.03  INS 김동일K : Initial Created.
  
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
using C1.WPF;
using C1.WPF.DataGrid.Summaries;

namespace LGC.GMES.MES.ProtoType04
{
    /// <summary>
    /// PGM_GUI_196.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class PGM_GUI_196 : UserControl, IWorkArea
    {
        #region Declaration & Constructor
        DataTable dtMain = new DataTable();
        DataRow newRow = null;



        Util _Util = new Util();
        private PGM_GUI_196_WORKORDER winWorkOrder = new PGM_GUI_196_WORKORDER();
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

        public PGM_GUI_196()
        {
            InitializeComponent();
        }

        private void InitCombo()
        {
            CommonCombo _combo = new CommonCombo();
            
            C1ComboBox[] cboLineChild = { cboEqpt };
            _combo.SetCombo(cboLine, CommonCombo.ComboStatus.SELECT, cbChild: cboLineChild);

            C1ComboBox[] cbEquipmentParent = { cboLine };
            _combo.SetCombo(cboEqpt, CommonCombo.ComboStatus.SELECT, cbParent: cbEquipmentParent);
        }
        #endregion

        #region Event
        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            ApplyPermissions();

            SetWorkOrderWindow();

            GetLineCombo();
        }

        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            if (cboLine.SelectedIndex < 0 || cboLine.SelectedValue.ToString().Trim().Length < 1)
            {                
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("라인을 선택 하세요."), null, "Info", MessageBoxButton.OK, MessageBoxIcon.Warning);
                return;
            }

            if (cboEqpt.SelectedIndex < 0 || cboEqpt.SelectedValue.ToString().Trim().Length < 1)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("설비를 선택 하세요."), null, "Info", MessageBoxButton.OK, MessageBoxIcon.Warning);
                return;
            }

            GetWorkOrder();
            GetProductLot();
        }

        private void btnRunStart_Click(object sender, RoutedEventArgs e)
        {
            if (!CanStartRun())
                return;

            PGM_GUI_196_RUNSTART wndRunStart = new PGM_GUI_196_RUNSTART();
            wndRunStart.FrameOperation = FrameOperation;
            wndRunStart.Closed += new EventHandler(wndRunStart_Closed);
            this.Dispatcher.BeginInvoke(new Action(() => wndRunStart.ShowModal()));
        }

        private void btnRunComplete_Click(object sender, RoutedEventArgs e)
        {
            if (!CanRunComplete())
                return;

            LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("저장 하시겠습니까?"), null, "Information", MessageBoxButton.OKCancel, MessageBoxIcon.None, (result) =>
            {
                if (result == MessageBoxResult.OK)
                {
                    if(RunComplete())
                    {
                        LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("정상 처리 되었습니다."), null, "Info", MessageBoxButton.OK, MessageBoxIcon.Warning);
                    }

                    GetProductLot();
                }
            });
        }

        private void btnConfirm_Click(object sender, RoutedEventArgs e)
        {
            if (!CanConfirm())
                return;

            PGM_GUI_196_CONFIRM wndConfirm = new PGM_GUI_196_CONFIRM();
            wndConfirm.FrameOperation = FrameOperation;
            wndConfirm.Closed += new EventHandler(wndConfirm_Closed);
            this.Dispatcher.BeginInvoke(new Action(() => wndConfirm.ShowModal()));
        }

        private void btnWaitLot_Click(object sender, RoutedEventArgs e)
        {
            string MAINFORMPATH = "LGC.GMES.MES.ProtoType04";
            string MAINFORMNAME = "PGM_GUI_196_WAITLOT";

            Assembly asm = Assembly.LoadFrom("ClientBin\\" + MAINFORMPATH + ".dll");
            Type targetType = asm.GetType(MAINFORMPATH + "." + MAINFORMNAME);
            object obj = Activator.CreateInstance(targetType);

            IWorkArea workArea = obj as IWorkArea;
            workArea.FrameOperation = FrameOperation;

            C1Window popup = obj as C1Window;
            if (popup != null)
            {
                popup.Closed -= popup_Closed;
                popup.Closed += popup_Closed;
                popup.ShowModal();
                popup.CenterOnScreen();
            }
        }

        private void btnTESTPrint_Click(object sender, RoutedEventArgs e)
        {
            string MAINFORMPATH = "LGC.GMES.MES.ProtoType04";
            string MAINFORMNAME = "PGM_GUI_196_TEST_PRINT";

            Assembly asm = Assembly.LoadFrom("ClientBin\\" + MAINFORMPATH + ".dll");
            Type targetType = asm.GetType(MAINFORMPATH + "." + MAINFORMNAME);
            object obj = Activator.CreateInstance(targetType);

            IWorkArea workArea = obj as IWorkArea;
            workArea.FrameOperation = FrameOperation;

            C1Window popup = obj as C1Window;
            if (popup != null)
            {
                popup.Closed -= popup_Closed;
                popup.Closed += popup_Closed;
                popup.ShowModal();
                popup.CenterOnScreen();
            }
        }

        private void btnQuality_Click(object sender, RoutedEventArgs e)
        {
            string MAINFORMPATH = "LGC.GMES.MES.ProtoType04";
            string MAINFORMNAME = "PGM_GUI_196_QUALITY";

            Assembly asm = Assembly.LoadFrom("ClientBin\\" + MAINFORMPATH + ".dll");
            Type targetType = asm.GetType(MAINFORMPATH + "." + MAINFORMNAME);
            object obj = Activator.CreateInstance(targetType);

            IWorkArea workArea = obj as IWorkArea;
            workArea.FrameOperation = FrameOperation;

            C1Window popup = obj as C1Window;
            if (popup != null)
            {
                popup.Closed -= popup_Closed;
                popup.Closed += popup_Closed;
                popup.ShowModal();
                popup.CenterOnScreen();
            }
        }

        private void btnEqpRemark_Click(object sender, RoutedEventArgs e)
        {
            string MAINFORMPATH = "LGC.GMES.MES.ProtoType04";
            string MAINFORMNAME = "PGM_GUI_196_EQPCOMMENT";

            Assembly asm = Assembly.LoadFrom("ClientBin\\" + MAINFORMPATH + ".dll");
            Type targetType = asm.GetType(MAINFORMPATH + "." + MAINFORMNAME);
            object obj = Activator.CreateInstance(targetType);

            IWorkArea workArea = obj as IWorkArea;
            workArea.FrameOperation = FrameOperation;

            C1Window popup = obj as C1Window;
            if (popup != null)
            {
                popup.Closed -= popup_Closed;
                popup.Closed += popup_Closed;
                popup.ShowModal();
                popup.CenterOnScreen();
            }
        }

        private void btnWorkDiary_Click(object sender, RoutedEventArgs e)
        {

        }

        //private void cboLine_SelectedIndexChanged(object sender, C1.WPF.PropertyChangedEventArgs<int> e)
        //{
        //    this.Dispatcher.BeginInvoke(new Action(() =>
        //    {
        //        if (cboLine.SelectedIndex > -1)
        //        {
        //            string selectedLine = string.Empty;

        //            selectedLine = cboLine.SelectedValue.ToString();





        //            string temp = ((DataRowView)cboLine.Items[cboLine.SelectedIndex])["NAME"].ToString();
        //            DataTable dt = new DataTable();
        //            dt.Columns.Add("CODE", typeof(string));
        //            dt.Columns.Add("NAME", typeof(string));

        //            dt.Rows.Add("", "-SELECT-");
        //            dt.Rows.Add("N1ANTC428", temp + " Stacking 1호기");
        //            dt.Rows.Add("N1ANTC429", temp + " Stacking 2호기");
        //            dt.Rows.Add("N1ANTC430", temp + " Stacking 3호기");
        //            dt.Rows.Add("N1ANTC431", temp + " Stacking 4호기");
        //            dt.Rows.Add("N1ANTC432", temp + " Stacking 5호기");

        //            cboEqpt.ItemsSource = dt.Copy().AsDataView();
        //            cboEqpt.SelectedIndex = 0;





        //            //DataTable shopIndataTable = new DataTable();
        //            //shopIndataTable.Columns.Add("LANGID", typeof(string));
        //            //shopIndataTable.Columns.Add("SUPPLIERID", typeof(string));
        //            //shopIndataTable.Columns.Add("SITEID", typeof(string));
        //            //DataRow shopIndata = shopIndataTable.NewRow();
        //            //shopIndata["LANGID"] = LoginInfo.LANGID;
        //            //shopIndata["SUPPLIERID"] = string.IsNullOrEmpty(LoginInfo.SUPPLIERID.ToString()) ? null : LoginInfo.SUPPLIERID;
        //            //shopIndata["SITEID"] = cboLine.SelectedValue;
        //            //shopIndataTable.Rows.Add(shopIndata);
        //            //new ClientProxy().ExecuteService("COR_SEL_SHOP_BY_SUPPLIERID_G", "INDATA", "OUTDATA", shopIndataTable, (shopResult, shopException) =>
        //            //{
        //            //    if (shopException != null)
        //            //    {
        //            //        LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(shopException), null, "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
        //            //        return;
        //            //    }

        //            //    cboShop.ItemsSource = DataTableConverter.Convert(shopResult);

        //            //    if ((from DataRow shop in shopResult.Rows where shop["SHOPID"].Equals(selectedShop) select shop).Count() > 0)
        //            //        cboShop.SelectedValue = selectedShop;
        //            //    else if (shopResult.Rows.Count > 0)
        //            //        cboShop.SelectedIndex = 0;
        //            //    else
        //            //        cboShop_SelectionChanged(sender, null);
        //            //}
        //            //);
        //        }
        //    }));
        //}

        private void chkProductLot_Checked(object sender, RoutedEventArgs e)
        {
            if (dgProductLot.CurrentRow.DataItem == null)
                return;

            //int rowIndex = ((C1.WPF.DataGrid.DataGridCellPresenter)((sender as CheckBox).Parent)).Row.Index;
            //string sLot = DataTableConverter.GetValue(((C1.WPF.DataGrid.DataGridCellPresenter)((sender as CheckBox).Parent)).Row.DataItem, "LOTID").ToString();

            //for (int i = 0; i < dgProductLot.Rows.Count; i++)
            //{
            //    if (rowIndex != i)
            //        if(dgProductLot.GetCell(i, dgProductLot.Columns["CHK"].Index).Presenter != null 
            //            && (dgProductLot.GetCell(i, dgProductLot.Columns["CHK"].Index).Presenter.Content as CheckBox).IsChecked.HasValue)
            //            (dgProductLot.GetCell(i, dgProductLot.Columns["CHK"].Index).Presenter.Content as CheckBox).IsChecked = false;
            //}

            _Util.SetDataGridUncheck(dgProductLot, "CHK", ((C1.WPF.DataGrid.DataGridCellPresenter)((sender as CheckBox).Parent)).Row.Index);

            GetWaitMagazine(((C1.WPF.DataGrid.DataGridCellPresenter)((sender as CheckBox).Parent)).Row.DataItem);
            GetInputMagazine(((C1.WPF.DataGrid.DataGridCellPresenter)((sender as CheckBox).Parent)).Row.DataItem);
            GetOutProduct(((C1.WPF.DataGrid.DataGridCellPresenter)((sender as CheckBox).Parent)).Row.DataItem);
        }

        private void rdoWaitMaz_Checked(object sender, RoutedEventArgs e)
        {
            if((sender as RadioButton).IsChecked.HasValue)
            {
                if ((bool)(sender as RadioButton).IsChecked)
                {
                    int idx = _Util.GetDataGridCheckFirstRowIndex(dgProductLot, "CHK");
                    if (idx < 0)
                        GetWaitMagazineByCurrentModel();
                    else
                        GetWaitMagazine(dgProductLot.Rows[idx].DataItem);
                }
            }
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

        private void SetWorkOrderWindow()
        {
            if (grdWorkOrder.Children.Count == 0)
            {
                //IWorkArea winWorkOrder = obj as IWorkArea;
                winWorkOrder.FrameOperation = FrameOperation;

                winWorkOrder._UCParent = this;
                grdWorkOrder.Children.Add(winWorkOrder);
            }
        }

        private void GetLineCombo()
        {
            DataTable dt = new DataTable();
            dt.Columns.Add("CODE", typeof(string));
            dt.Columns.Add("NAME", typeof(string));

            dt.Rows.Add("", "-SELECT-");
            dt.Rows.Add("CAL140", "8 라인");
            dt.Rows.Add("CAL150", "9 라인");
            dt.Rows.Add("CAL160", "10 라인");
            
            cboLine.ItemsSource = dt.Copy().AsDataView();

            cboLine.SelectedIndex = 0;
        }

        public void GetProductLot(DataRow drWOInfo = null)
        {
            Util.gridClear(dgProductLot);

            if (drWOInfo == null)
            {
                DataRow[] runWOInfo = GetWorkOrderInfo();

                if (runWOInfo == null)
                    return;

                drWOInfo = runWOInfo[0];
            }

            dtMain = new DataTable();
            dtMain.Columns.Add("LOTID", typeof(string));
            dtMain.Columns.Add("LARGELOT", typeof(string));
            dtMain.Columns.Add("INPUTLOT", typeof(string));
            dtMain.Columns.Add("CHILDGRPSEQ", typeof(string));
            dtMain.Columns.Add("MODELNAME", typeof(string));
            dtMain.Columns.Add("WIPSTAT", typeof(string));
            dtMain.Columns.Add("WIPQTY", typeof(Int32));
            dtMain.Columns.Add("EQPQTY", typeof(Int32));
            dtMain.Columns.Add("LENGTH", typeof(Int32));
            dtMain.Columns.Add("FCUTYN", typeof(string));
            dtMain.Columns.Add("STARTTIME", typeof(string));
            dtMain.Columns.Add("EQPENDTIME", typeof(string));
            dtMain.Columns.Add("WORKORDER", typeof(string));
            dtMain.Columns.Add("OPERCODE", typeof(string));
            dtMain.Columns.Add("PRODID", typeof(string));
            dtMain.Columns.Add("PRODNAME", typeof(string));
            dtMain.Columns.Add("VERSION", typeof(string));
            dtMain.Columns.Add("CONV", typeof(string));



            if (drWOInfo["PRODID"].Equals("MBEV3601AM"))
            {
                newRow = dtMain.NewRow();
                newRow.ItemArray = new object[] { "4A56D922CA", "4A56D92", "4A56D92221", "2", "CB5 456674L1음극", "장비완료", 0, 269, 0, "Y", "2016-06-22 20:18", "2016-06-22 23:50", "1332346", "10", "MBEV3601AM", "BATTERY 456674L1 3390mAh  ANODE Notching", "", "" };
                dtMain.Rows.Add(newRow);

                newRow = dtMain.NewRow();
                newRow.ItemArray = new object[] { "4A56D922DA", "4A56D92", "4A56D92221", "2", "CB5 456674L1음극", "장비완료", 0, 269, 0, "Y", "2016-06-22 20:18", "2016-06-22 23:50", "1332346", "10", "MBEV3601AM", "BATTERY 456674L1 3390mAh  ANODE Notching", "", "" };
                dtMain.Rows.Add(newRow);


                newRow = dtMain.NewRow();
                newRow.ItemArray = new object[] { "6BW6F01ICD", "6BW6F01", "6BW6F01I22", "3", "CAW 466773L1음극", "장비완료", 0, 11569, 0, "Y", "2016-06-30 17:58:09", "2016-06-30 18:04:19", "1333384", "0010", "MBEV3601AM", "BATTERY 466773L1 4030mAh ANODE Notching", "", "" };
                dtMain.Rows.Add(newRow);

                newRow = dtMain.NewRow();
                newRow.ItemArray = new object[] { "6BW6F01IDD", "6BW6F01", "6BW6F01I22", "3", "CAW 466773L1음극", "장비완료", 0, 11569, 0, "Y", "2016-06-30 17:58:09", "2016-06-30 18:04:19", "1333384", "0010", "MBEV3601AM", "BATTERY 466773L1 4030mAh ANODE Notching", "", "" };
                dtMain.Rows.Add(newRow);

                newRow = dtMain.NewRow();
                newRow.ItemArray = new object[] { "5JS6E106CG", "5JS6E10", "5JS6E10623", "1", "CFS 3449109L1양극", "장비완료", 0, 688, 0, "Y", "2016-06-28 9:58", "2016-06-28 10:53", "1331224", "0010", "MBEV3601AM", "BATTERY 3449109L1 2930mAh CATHODE Notch", "", "" };
                dtMain.Rows.Add(newRow);

                newRow = dtMain.NewRow();
                newRow.ItemArray = new object[] { "5JS6E106DG", "5JS6E10", "5JS6E10623", "1", "CFS 3449109L1양극", "장비완료", 0, 688, 0, "Y", "2016-06-28 9:58", "2016-06-28 10:53", "1331224", "0010", "MBEV3601AM", "BATTERY 3449109L1 2930mAh  CATHODE Notch", "", "" };
                dtMain.Rows.Add(newRow);

                newRow = dtMain.NewRow();
                newRow.ItemArray = new object[] { "J4F6F024CA", "J4F6F02", "J4F6F02421", "5", "AHF 363391L1양극", "장비완료", 0, 23424, 0, "Y", "2016-07-01 14:50", "2016-07-01 14:51", "1333284", "0010", "MBEV3601AM", "BATTERY 363391L1 1640mAh  CATHODE Notchi", "", "" };
                dtMain.Rows.Add(newRow);
            }
            else if (drWOInfo["PRODID"].Equals("MBEV3601AP"))
            {
                newRow = dtMain.NewRow();
                newRow.ItemArray = new object[] { "5JS6E106DG", "5JS6E10", "5JS6E10623", "1", "CFS 3449109L1양극", "장비완료", 0, 688, 0, "Y", "2016-06-28 9:58", "2016-06-28 10:53", "1331224", "0010", "MBEV3601AP", "BATTERY 3449109L1 2930mAh  CATHODE Notch", "", "" };
                dtMain.Rows.Add(newRow);

                newRow = dtMain.NewRow();
                newRow.ItemArray = new object[] { "J4F6F024CA", "J4F6F02", "J4F6F02421", "5", "AHF 363391L1양극", "장비완료", 0, 23424, 0, "Y", "2016-07-01 14:50", "2016-07-01 14:51", "1333284", "0010", "MBEV3601AP", "BATTERY 363391L1 1640mAh  CATHODE Notchi", "", "" };
                dtMain.Rows.Add(newRow);
            }
            else if (drWOInfo["PRODID"].Equals("MBEV3801AB"))
            {
                newRow = dtMain.NewRow();
                newRow.ItemArray = new object[] { "4A56D922CA", "4A56D92", "4A56D92221", "2", "CB5 456674L1음극", "장비완료", 0, 269, 0, "Y", "2016-06-22 20:18", "2016-06-22 23:50", "1332346", "10", "MBEV3801AB", "BATTERY 456674L1 3390mAh  ANODE Notching", "", "" };
                dtMain.Rows.Add(newRow);

                newRow = dtMain.NewRow();
                newRow.ItemArray = new object[] { "4A56D922DA", "4A56D92", "4A56D92221", "2", "CB5 456674L1음극", "장비완료", 0, 269, 0, "Y", "2016-06-22 20:18", "2016-06-22 23:50", "1332346", "10", "MBEV3801AB", "BATTERY 456674L1 3390mAh  ANODE Notching", "", "" };
                dtMain.Rows.Add(newRow);


                newRow = dtMain.NewRow();
                newRow.ItemArray = new object[] { "6BW6F01ICD", "6BW6F01", "6BW6F01I22", "3", "CAW 466773L1음극", "장비완료", 0, 11569, 0, "Y", "2016-06-30 17:58:09", "2016-06-30 18:04:19", "1333384", "0010", "MBEV3801AB", "BATTERY 466773L1 4030mAh ANODE Notching", "", "" };
                dtMain.Rows.Add(newRow);

                newRow = dtMain.NewRow();
                newRow.ItemArray = new object[] { "6BW6F01IDD", "6BW6F01", "6BW6F01I22", "3", "CAW 466773L1음극", "장비완료", 0, 11569, 0, "Y", "2016-06-30 17:58:09", "2016-06-30 18:04:19", "1333384", "0010", "MBEV3801AB", "BATTERY 466773L1 4030mAh ANODE Notching", "", "" };
                dtMain.Rows.Add(newRow);
            }
            else if (drWOInfo["PRODID"].Equals("MBSLURRYAA2"))
            {
            }
            

            //newRow = dtMain.NewRow();
            //newRow.ItemArray = new object[] { "J4F6F024CA", "J4F6F02", "J4F6F02421", "5", "AHF 363391L1양극", "장비완료", 0, 23424, 0, "Y", "2016-07-01 14:50", "2016-07-01 14:51", "1333284", "0010", "ENP363391APNC", "BATTERY 363391L1 1640mAh  CATHODE Notchi", "", "" };
            //dtMain.Rows.Add(newRow);


            dgProductLot.ItemsSource = DataTableConverter.Convert(dtMain);
        }

        private void GetWorkOrder()
        {
            if (winWorkOrder == null)
                return;
            
            winWorkOrder.LINEID = cboLine.SelectedValue.ToString();
            winWorkOrder.EQPTID = cboEqpt.SelectedValue.ToString();

            winWorkOrder.GetWorkOrder();
        }

        private DataRow[] GetWorkOrderInfo()
        {
            if (winWorkOrder == null)
                return null;

            return winWorkOrder.GetWorkOrderInfo();            
        }

        private string GetCurrentModel()
        {
            string sModel = string.Empty;

            return sModel;
        }

        private void GetInputMagazine(object SelectedItem)
        {
            Util.gridClear(dgInMagazine);

            DataRowView rowview = SelectedItem as DataRowView;

            if (rowview == null)
                return;


            dtMain = new DataTable();
            dtMain.Columns.Add("MAGAZINEID", typeof(string));
            dtMain.Columns.Add("CASSETTEID", typeof(string));
            dtMain.Columns.Add("POSITION", typeof(string));
            dtMain.Columns.Add("PRODID", typeof(string));
            dtMain.Columns.Add("INQTY", typeof(Int32));
            dtMain.Columns.Add("ELECTYPE", typeof(string));
            dtMain.Columns.Add("INPUTDTTM", typeof(string));
            
            Random rnd = new Random();
            
            if (rnd.Next(0, 2) > 0)
            {
                newRow = dtMain.NewRow();
                newRow.ItemArray = new object[] { "A6G05L8611", "AL119", "1", "EVESALH0600I1", rnd.Next(0, 300), "C", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") };
                dtMain.Rows.Add(newRow);

                newRow = dtMain.NewRow();
                newRow.ItemArray = new object[] { "A6G05L8616", "AL110", "2", "EVESALH0600I1", rnd.Next(0, 300), "C", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") };
                dtMain.Rows.Add(newRow);

                newRow = dtMain.NewRow();
                newRow.ItemArray = new object[] { "A6G05L8617", "", "3", "EVESALH0600I1", rnd.Next(0, 300), "C", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") };
                dtMain.Rows.Add(newRow);

                newRow = dtMain.NewRow();
                newRow.ItemArray = new object[] { "A6G05L8618", "AL009", "1", "EVESALH0600I1", rnd.Next(0, 300), "C", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") };
                dtMain.Rows.Add(newRow);

                newRow = dtMain.NewRow();
                newRow.ItemArray = new object[] { "C6G12L8418", "AL219", "2", "EVESALH0600I1", rnd.Next(0, 300), "C", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") };
                dtMain.Rows.Add(newRow);

                newRow = dtMain.NewRow();
                newRow.ItemArray = new object[] { "C6G12L8432", "AL002", "3", "EVESALH0600I1", rnd.Next(0, 300), "C", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") };
                dtMain.Rows.Add(newRow);

                newRow = dtMain.NewRow();
                newRow.ItemArray = new object[] { "C6G12L8433", "AL100", "1", "EVESALH0600I1", rnd.Next(0, 50000), "A", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") };
                dtMain.Rows.Add(newRow);
            }
            else
            {
                newRow = dtMain.NewRow();
                newRow.ItemArray = new object[] { "C6G12L8432", "AL002", "1", "EVESALH0600I1", rnd.Next(0, 300), "C", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") };
                dtMain.Rows.Add(newRow);

                newRow = dtMain.NewRow();
                newRow.ItemArray = new object[] { "C6G12L8433", "AL100", "2", "EVESALH0600I1", rnd.Next(0, 99999), "A", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") };
                dtMain.Rows.Add(newRow);
            }

            dgInMagazine.ItemsSource = DataTableConverter.Convert(dtMain);



            SetElecTypeCount();
        }

        private void GetWaitMagazineByCurrentModel()
        {            
            string sElec = string.Empty;
            string sModel = GetCurrentModel();

            if (sModel.Equals("")) return;

            if (rdoHalf.IsChecked.HasValue)
            {
                if ((bool)rdoHalf.IsChecked) sElec = "HALF";
            }
            else if (rdoMono.IsChecked.HasValue)
            {
                if ((bool)rdoMono.IsChecked) sElec = "MONO";
            }
            else
                return;


            Util.gridClear(dgWaitMagazine);


            

            dtMain = new DataTable();
            dtMain.Columns.Add("MAGAZINEID", typeof(string));
            dtMain.Columns.Add("LAMILOT", typeof(string));
            dtMain.Columns.Add("CASSETTEID", typeof(string));
            dtMain.Columns.Add("QTY", typeof(Int32));
            dtMain.Columns.Add("INSDTTM", typeof(string));
            dtMain.Columns.Add("PRODID", typeof(string));
            dtMain.Columns.Add("PRODNAME", typeof(string));
            
            if (sElec.Equals("HALF"))
            {

                newRow = dtMain.NewRow();
                newRow.ItemArray = new object[] { "A5KW1LL301", "XGU5KW1LL3", "", 49, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), "EVESALH0600I1", "JH3_ESS_Laminating Half-Type" };
                dtMain.Rows.Add(newRow);

                newRow = dtMain.NewRow();
                newRow.ItemArray = new object[] { "A5KW1LL801", "XGU5KW1LL8", "", 49, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), "EVESALH0600I1", "JH3_ESS_Laminating Half-Type" };
                dtMain.Rows.Add(newRow);

                newRow = dtMain.NewRow();
                newRow.ItemArray = new object[] { "A5LW4LL601", "XGU5LW4LL6", "", 0, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), "EVESALH0600I1", "JH3_ESS_Laminating Half-Type" };
                dtMain.Rows.Add(newRow);

                newRow = dtMain.NewRow();
                newRow.ItemArray = new object[] { "A5LW6LL501", "XGU5LW6LL5", "", 3.734, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), "EVESALH0600I1", "JH3_ESS_Laminating Half-Type" };
                dtMain.Rows.Add(newRow);

                newRow = dtMain.NewRow();
                newRow.ItemArray = new object[] { "A6AW8LL306", "XGU6AW8LL3", "Read_Fail", 500, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), "EVESALH0600I1", "JH3_ESS_Laminating Half-Type" };
                dtMain.Rows.Add(newRow);

                newRow = dtMain.NewRow();
                newRow.ItemArray = new object[] { "A6AW8LL709", "XGU6AW8LL7", "Read_Fail", 500, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), "EVESALH0600I1", "JH3_ESS_Laminating Half-Type" };
                dtMain.Rows.Add(newRow);

                newRow = dtMain.NewRow();
                newRow.ItemArray = new object[] { "A6AX2LL511", "XGU6AX2LL5", "AL149", 500, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), "EVESALH0600I1", "JH3_ESS_Laminating Half-Type" };
                dtMain.Rows.Add(newRow);

                newRow = dtMain.NewRow();
                newRow.ItemArray = new object[] { "A6AX4LL703", "XGU6AX4LL7", "AL150", 500, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), "EVESALH0600I1", "JH3_ESS_Laminating Half-Type" };
                dtMain.Rows.Add(newRow);

                newRow = dtMain.NewRow();
                newRow.ItemArray = new object[] { "A6AX4LL715", "XGU6AX4LL7", "AL149", 500, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), "EVESALH0600I1", "JH3_ESS_Laminating Half-Type" };
                dtMain.Rows.Add(newRow);

                newRow = dtMain.NewRow();
                newRow.ItemArray = new object[] { "A6AX4LL721", "XGU6AX4LL7", "AL11", 500, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), "EVESALH0600I1", "JH3_ESS_Laminating Half-Type" };
                dtMain.Rows.Add(newRow);

                newRow = dtMain.NewRow();
                newRow.ItemArray = new object[] { "A6BW0LL701", "XGU6BW0LL7", "Read_Fail", 500, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), "EVESALH0600I1", "JH3_ESS_Laminating Half-Type" };
                dtMain.Rows.Add(newRow);

                newRow = dtMain.NewRow();
                newRow.ItemArray = new object[] { "A6BW0LL702", "XGU6BW0LL7", "AL11", 500, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), "EVESALH0600I1", "JH3_ESS_Laminating Half-Type" };
                dtMain.Rows.Add(newRow);

                newRow = dtMain.NewRow();
                newRow.ItemArray = new object[] { "A6BW0LL709", "XGU6BW0LL7", "AL150", 500, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), "EVESALH0600I1", "JH3_ESS_Laminating Half-Type" };
                dtMain.Rows.Add(newRow);

                newRow = dtMain.NewRow();
                newRow.ItemArray = new object[] { "A6BW0LL715", "XGU6BW0LL7", "Read_Fail", 500, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), "EVESALH0600I1", "JH3_ESS_Laminating Half-Type" };
                dtMain.Rows.Add(newRow);

                newRow = dtMain.NewRow();
                newRow.ItemArray = new object[] { "A6BW0LL717", "XGU6BW0LL7", "AL149", 500, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), "EVESALH0600I1", "JH3_ESS_Laminating Half-Type" };
                dtMain.Rows.Add(newRow);

                newRow = dtMain.NewRow();
                newRow.ItemArray = new object[] { "A6BW0LL721", "XGU6BW0LL7", "AL128", 500, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), "EVESALH0600I1", "JH3_ESS_Laminating Half-Type" };
                dtMain.Rows.Add(newRow);

                newRow = dtMain.NewRow();
                newRow.ItemArray = new object[] { "A6AY0LL409", "XGU6AY0LL4", "AL128", 500, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), "EVESALH0600I1", "JH3_ESS_Laminating Half-Type" };
                dtMain.Rows.Add(newRow);

                newRow = dtMain.NewRow();
                newRow.ItemArray = new object[] { "A6AY0LL412", "XGU6AY0LL4", "AL149", 500, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), "EVESALH0600I1", "JH3_ESS_Laminating Half-Type" };
                dtMain.Rows.Add(newRow);

                newRow = dtMain.NewRow();
                newRow.ItemArray = new object[] { "A6AY0LL422", "XGU6AY0LL4", "AL150", 500, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), "EVESALH0600I1", "JH3_ESS_Laminating Half-Type" };
                dtMain.Rows.Add(newRow);

                newRow = dtMain.NewRow();
                newRow.ItemArray = new object[] { "A6B01LL101", "XGU6B01LL1", "Read_Fail", 500, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), "EVESALH0600I1", "JH3_ESS_Laminating Half-Type" };
                dtMain.Rows.Add(newRow);

                newRow = dtMain.NewRow();
                newRow.ItemArray = new object[] { "A6B01LL107", "XGU6B01LL1", "AL150", 500, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), "EVESALH0600I1", "JH3_ESS_Laminating Half-Type" };
                dtMain.Rows.Add(newRow);

                newRow = dtMain.NewRow();
                newRow.ItemArray = new object[] { "A6B01LL109", "XGU6B01LL1", "Read_Fail", 500, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), "EVESALH0600I1", "JH3_ESS_Laminating Half-Type" };
                dtMain.Rows.Add(newRow);

                newRow = dtMain.NewRow();
                newRow.ItemArray = new object[] { "A6B01LL110", "XGU6B01LL1", "Read_Fail", 500, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), "EVESALH0600I1", "JH3_ESS_Laminating Half-Type" };
                dtMain.Rows.Add(newRow);

                newRow = dtMain.NewRow();
                newRow.ItemArray = new object[] { "A6B01LL117", "XGU6B01LL1", "AL149", 500, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), "EVESALH0600I1", "JH3_ESS_Laminating Half-Type" };
                dtMain.Rows.Add(newRow);

                newRow = dtMain.NewRow();
                newRow.ItemArray = new object[] { "A6C03LL201", "XGU6C03LL2", "AL128", 500, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), "EVESALH0600I1", "JH3_ESS_Laminating Half-Type" };
                dtMain.Rows.Add(newRow);

                newRow = dtMain.NewRow();
                newRow.ItemArray = new object[] { "A6C03LL202", "XGU6C03LL2", "Read_Fail", 500, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), "EVESALH0600I1", "JH3_ESS_Laminating Half-Type" };
                dtMain.Rows.Add(newRow);

                newRow = dtMain.NewRow();
                newRow.ItemArray = new object[] { "A6C03LL211", "XGU6C03LL2", "AL149", 500, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), "EVESALH0600I1", "JH3_ESS_Laminating Half-Type" };
                dtMain.Rows.Add(newRow);

                newRow = dtMain.NewRow();
                newRow.ItemArray = new object[] { "A6C03LL219", "XGU6C03LL2", "AL128", 500, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), "EVESALH0600I1", "JH3_ESS_Laminating Half-Type" };
                dtMain.Rows.Add(newRow);

                newRow = dtMain.NewRow();
                newRow.ItemArray = new object[] { "A6C04LL103", "XGU6C04LL1", "AL150", 500, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), "EVESALH0600I1", "JH3_ESS_Laminating Half-Type" };
                dtMain.Rows.Add(newRow);

                newRow = dtMain.NewRow();
                newRow.ItemArray = new object[] { "A6C04LL302", "XGU6C04LL3", "AL149", 500, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), "EVESALH0600I1", "JH3_ESS_Laminating Half-Type" };
                dtMain.Rows.Add(newRow);

                newRow = dtMain.NewRow();
                newRow.ItemArray = new object[] { "A6C04LL311", "XGU6C04LL3", "AL128", 500, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), "EVESALH0600I1", "JH3_ESS_Laminating Half-Type" };
                dtMain.Rows.Add(newRow);

                newRow = dtMain.NewRow();
                newRow.ItemArray = new object[] { "A6C07LL102", "XGU6C07LL1", "AL150", 500, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), "EVESALH0600I1", "JH3_ESS_Laminating Half-Type" };
                dtMain.Rows.Add(newRow);

                newRow = dtMain.NewRow();
                newRow.ItemArray = new object[] { "A6C07LL111", "XGU6C07LL1", "AL149", 500, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), "EVESALH0600I1", "JH3_ESS_Laminating Half-Type" };
                dtMain.Rows.Add(newRow);

                newRow = dtMain.NewRow();
                newRow.ItemArray = new object[] { "A6C07LL120", "XGU6C07LL1", "AL128", 500, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), "EVESALH0600I1", "JH3_ESS_Laminating Half-Type" };
                dtMain.Rows.Add(newRow);

                newRow = dtMain.NewRow();
                newRow.ItemArray = new object[] { "A6B17LL103", "XGU6B17LL1", "AL128", 500, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), "EVESALH0600I1", "JH3_ESS_Laminating Half-Type" };
                dtMain.Rows.Add(newRow);

                newRow = dtMain.NewRow();
                newRow.ItemArray = new object[] { "A6B17LL108", "XGU6B17LL1", "Read_Fail", 500, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), "EVESALH0600I1", "JH3_ESS_Laminating Half-Type" };
                dtMain.Rows.Add(newRow);

                newRow = dtMain.NewRow();
                newRow.ItemArray = new object[] { "A6B17LL115", "XGU6B17LL1", "AL149", 500, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), "EVESALH0600I1", "JH3_ESS_Laminating Half-Type" };
                dtMain.Rows.Add(newRow);

                newRow = dtMain.NewRow();
                newRow.ItemArray = new object[] { "A6B17LL117", "XGU6B17LL1", "AL150", 500, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), "EVESALH0600I1", "JH3_ESS_Laminating Half-Type" };
                dtMain.Rows.Add(newRow);

                newRow = dtMain.NewRow();
                newRow.ItemArray = new object[] { "A6C11LL301", "XGU6C11LL3", "Read_Fail", 500, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), "EVESALH0600I1", "JH3_ESS_Laminating Half-Type" };
                dtMain.Rows.Add(newRow);

                newRow = dtMain.NewRow();
                newRow.ItemArray = new object[] { "A6C11LL307", "XGU6C11LL3", "AL150", 500, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), "EVESALH0600I1", "JH3_ESS_Laminating Half-Type" };
                dtMain.Rows.Add(newRow);

                newRow = dtMain.NewRow();
                newRow.ItemArray = new object[] { "A6AX4LL808", "XGU6AX4LL8", "AL149", 500, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), "EVESALH0600I1", "JH3_ESS_Laminating Half-Type" };
                dtMain.Rows.Add(newRow);

                newRow = dtMain.NewRow();
                newRow.ItemArray = new object[] { "A6AX4LL809", "XGU6AX4LL8", "Read_Fail", 500, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), "EVESALH0600I1", "JH3_ESS_Laminating Half-Type" };
                dtMain.Rows.Add(newRow);

                newRow = dtMain.NewRow();
                newRow.ItemArray = new object[] { "A6AX4LL810", "XGU6AX4LL8", "Read_Fail", 500, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), "EVESALH0600I1", "JH3_ESS_Laminating Half-Type" };
                dtMain.Rows.Add(newRow);

                newRow = dtMain.NewRow();
                newRow.ItemArray = new object[] { "A6AX4LL811", "XGU6AX4LL8", "Read_Fail", 500, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), "EVESALH0600I1", "JH3_ESS_Laminating Half-Type" };
                dtMain.Rows.Add(newRow);

                newRow = dtMain.NewRow();
                newRow.ItemArray = new object[] { "A6AX4LL812", "XGU6AX4LL8", "Read_Fail", 500, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), "EVESALH0600I1", "JH3_ESS_Laminating Half-Type" };
                dtMain.Rows.Add(newRow);

                newRow = dtMain.NewRow();
                newRow.ItemArray = new object[] { "A6AX4LL813", "XGU6AX4LL8", "AL150", 500, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), "EVESALH0600I1", "JH3_ESS_Laminating Half-Type" };
                dtMain.Rows.Add(newRow);

                newRow = dtMain.NewRow();
                newRow.ItemArray = new object[] { "A6AX4LL818", "XGU6AX4LL8", "AL128", 500, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), "EVESALH0600I1", "JH3_ESS_Laminating Half-Type" };
                dtMain.Rows.Add(newRow);

                newRow = dtMain.NewRow();
                newRow.ItemArray = new object[] { "A6B08LL101", "XGU6B08LL1", "AL18A", 500, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), "EVESALH0600I1", "JH3_ESS_Laminating Half-Type" };
                dtMain.Rows.Add(newRow);

                newRow = dtMain.NewRow();
                newRow.ItemArray = new object[] { "A6D09LL301", "XGU6D09LL3", "", 977, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), "EVESALH0600I1", "JH3_ESS_Laminating Half-Type" };
                dtMain.Rows.Add(newRow);

                newRow = dtMain.NewRow();
                newRow.ItemArray = new object[] { "A6D20LL501", "XGU6D20LL5", "", 0, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), "EVESALH0600I1", "JH3_ESS_Laminating Half-Type" };
                dtMain.Rows.Add(newRow);

                newRow = dtMain.NewRow();
                newRow.ItemArray = new object[] { "A6E09LL301", "XGU6E09LL3", "", 44.668, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), "EVESALH0600I1", "JH3_ESS_Laminating Half-Type" };
                dtMain.Rows.Add(newRow);

                newRow = dtMain.NewRow();
                newRow.ItemArray = new object[] { "A6E13LLB05", "2GU6E13LLB", "AL120", 500, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), "EVESALH0600I1", "JH3_ESS_Laminating Half-Type" };
                dtMain.Rows.Add(newRow);

                newRow = dtMain.NewRow();
                newRow.ItemArray = new object[] { "A6F05LL901", "2GU6F05LL9", "AL124", 500, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), "EVESALH0600I1", "JH3_ESS_Laminating Half-Type" };
                dtMain.Rows.Add(newRow);

                newRow = dtMain.NewRow();
                newRow.ItemArray = new object[] { "A6F05LL902", "2GU6F05LL9", "AL113", 500, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), "EVESALH0600I1", "JH3_ESS_Laminating Half-Type" };
                dtMain.Rows.Add(newRow);

                newRow = dtMain.NewRow();
                newRow.ItemArray = new object[] { "A6F05LL904", "2GU6F05LL9", "AL112", 500, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), "EVESALH0600I1", "JH3_ESS_Laminating Half-Type" };
                dtMain.Rows.Add(newRow);

                newRow = dtMain.NewRow();
                newRow.ItemArray = new object[] { "A6F05LL908", "2GU6F05LL9", "AL121", 500, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), "EVESALH0600I1", "JH3_ESS_Laminating Half-Type" };
                dtMain.Rows.Add(newRow);

                newRow = dtMain.NewRow();
                newRow.ItemArray = new object[] { "A6F05LL911", "2GU6F05LL9", "AL125", 500, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), "EVESALH0600I1", "JH3_ESS_Laminating Half-Type" };
                dtMain.Rows.Add(newRow);

                newRow = dtMain.NewRow();
                newRow.ItemArray = new object[] { "A6F05LL912", "2GU6F05LL9", "AL120", 500, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), "EVESALH0600I1", "JH3_ESS_Laminating Half-Type" };
                dtMain.Rows.Add(newRow);

                newRow = dtMain.NewRow();
                newRow.ItemArray = new object[] { "A6F05LL913", "2GU6F05LL9", "AL115", 500, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), "EVESALH0600I1", "JH3_ESS_Laminating Half-Type" };
                dtMain.Rows.Add(newRow);

                newRow = dtMain.NewRow();
                newRow.ItemArray = new object[] { "A6F05LL914", "2GU6F05LL9", "AL124", 500, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), "EVESALH0600I1", "JH3_ESS_Laminating Half-Type" };
                dtMain.Rows.Add(newRow);

                newRow = dtMain.NewRow();
                newRow.ItemArray = new object[] { "A6F05LL915", "2GU6F05LL9", "AL103", 500, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), "EVESALH0600I1", "JH3_ESS_Laminating Half-Type" };
                dtMain.Rows.Add(newRow);

                newRow = dtMain.NewRow();
                newRow.ItemArray = new object[] { "A6F05LL917", "2GU6F05LL9", "AL112", 500, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), "EVESALH0600I1", "JH3_ESS_Laminating Half-Type" };
                dtMain.Rows.Add(newRow);

                newRow = dtMain.NewRow();
                newRow.ItemArray = new object[] { "A6F05LL920", "2GU6F05LL9", "AL108", 500, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), "EVESALH0600I1", "JH3_ESS_Laminating Half-Type" };
                dtMain.Rows.Add(newRow);

                newRow = dtMain.NewRow();
                newRow.ItemArray = new object[] { "A6F05LL921", "2GU6F05LL9", "AL116", 500, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), "EVESALH0600I1", "JH3_ESS_Laminating Half-Type" };
                dtMain.Rows.Add(newRow);

                newRow = dtMain.NewRow();
                newRow.ItemArray = new object[] { "A6F05LL923", "2GU6F05LL9", "AL119", 1200, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), "EVESALH0600I1", "JH3_ESS_Laminating Half-Type" };
                dtMain.Rows.Add(newRow);


            }
            else
            {
                newRow = dtMain.NewRow();
                newRow.ItemArray = new object[] { "A6F05LL917", "2GU6F05LL9", "AL112", 500, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), "EVESALH0600I1", "JH3_ESS_Laminating Half-Type" };
                dtMain.Rows.Add(newRow);

                newRow = dtMain.NewRow();
                newRow.ItemArray = new object[] { "A6F05LL920", "2GU6F05LL9", "AL108", 500, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), "EVESALH0600I1", "JH3_ESS_Laminating Half-Type" };
                dtMain.Rows.Add(newRow);

                newRow = dtMain.NewRow();
                newRow.ItemArray = new object[] { "A6F05LL921", "2GU6F05LL9", "AL116", 500, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), "EVESALH0600I1", "JH3_ESS_Laminating Half-Type" };
                dtMain.Rows.Add(newRow);

                newRow = dtMain.NewRow();
                newRow.ItemArray = new object[] { "A6F05LL923", "2GU6F05LL9", "AL119", 500, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), "EVESALH0600I1", "JH3_ESS_Laminating Half-Type" };
                dtMain.Rows.Add(newRow);
            }



            dgWaitMagazine.ItemsSource = DataTableConverter.Convert(dtMain);
        }

        private void GetWaitMagazine(object SelectedItem)
        {
            DataRowView rowview = SelectedItem as DataRowView;

            if (rowview == null)
                return;

            string sElec = string.Empty;

            if (rdoHalf.IsChecked.HasValue)
            {
                if ((bool)rdoHalf.IsChecked) sElec = "HALF";
            }
            else if (rdoMono.IsChecked.HasValue)
            {
                if ((bool)rdoMono.IsChecked) sElec = "MONO";
            }
            else
                return;

            Util.gridClear(dgWaitMagazine);

            string sWorkOrder = rowview["WORKORDER"].ToString();
            string sOperCode = rowview["OPERCODE"].ToString();



            dtMain = new DataTable();
            dtMain.Columns.Add("MAGAZINEID", typeof(string));
            dtMain.Columns.Add("LAMILOT", typeof(string));
            dtMain.Columns.Add("CASSETTEID", typeof(string));
            dtMain.Columns.Add("QTY", typeof(Int32));
            dtMain.Columns.Add("INSDTTM", typeof(string));
            dtMain.Columns.Add("PRODID", typeof(string));
            dtMain.Columns.Add("PRODNAME", typeof(string));

    

                newRow = dtMain.NewRow();
                newRow.ItemArray = new object[] { "A5KW1LL301", "XGU5KW1LL3", "", 49, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), "EVESALH0600I1", "JH3_ESS_Laminating Half-Type" };
                dtMain.Rows.Add(newRow);

                newRow = dtMain.NewRow();
                newRow.ItemArray = new object[] { "A5KW1LL801", "XGU5KW1LL8", "", 49, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), "EVESALH0600I1", "JH3_ESS_Laminating Half-Type" };
                dtMain.Rows.Add(newRow);

                newRow = dtMain.NewRow();
                newRow.ItemArray = new object[] { "A5LW4LL601", "XGU5LW4LL6", "", 0, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), "EVESALH0600I1", "JH3_ESS_Laminating Half-Type" };
                dtMain.Rows.Add(newRow);

                newRow = dtMain.NewRow();
                newRow.ItemArray = new object[] { "A5LW6LL501", "XGU5LW6LL5", "", 3.734, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), "EVESALH0600I1", "JH3_ESS_Laminating Half-Type" };
                dtMain.Rows.Add(newRow);

                newRow = dtMain.NewRow();
                newRow.ItemArray = new object[] { "A6AW8LL306", "XGU6AW8LL3", "Read_Fail", 500, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), "EVESALH0600I1", "JH3_ESS_Laminating Half-Type" };
                dtMain.Rows.Add(newRow);

                newRow = dtMain.NewRow();
                newRow.ItemArray = new object[] { "A6AW8LL709", "XGU6AW8LL7", "Read_Fail", 500, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), "EVESALH0600I1", "JH3_ESS_Laminating Half-Type" };
                dtMain.Rows.Add(newRow);

                newRow = dtMain.NewRow();
                newRow.ItemArray = new object[] { "A6AX2LL511", "XGU6AX2LL5", "AL149", 500, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), "EVESALH0600I1", "JH3_ESS_Laminating Half-Type" };
                dtMain.Rows.Add(newRow);

                newRow = dtMain.NewRow();
                newRow.ItemArray = new object[] { "A6AX4LL703", "XGU6AX4LL7", "AL150", 500, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), "EVESALH0600I1", "JH3_ESS_Laminating Half-Type" };
                dtMain.Rows.Add(newRow);

                newRow = dtMain.NewRow();
                newRow.ItemArray = new object[] { "A6AX4LL715", "XGU6AX4LL7", "AL149", 500, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), "EVESALH0600I1", "JH3_ESS_Laminating Half-Type" };
                dtMain.Rows.Add(newRow);

                newRow = dtMain.NewRow();
                newRow.ItemArray = new object[] { "A6AX4LL721", "XGU6AX4LL7", "AL11", 500, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), "EVESALH0600I1", "JH3_ESS_Laminating Half-Type" };
                dtMain.Rows.Add(newRow);

                newRow = dtMain.NewRow();
                newRow.ItemArray = new object[] { "A6BW0LL701", "XGU6BW0LL7", "Read_Fail", 500, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), "EVESALH0600I1", "JH3_ESS_Laminating Half-Type" };
                dtMain.Rows.Add(newRow);

                newRow = dtMain.NewRow();
                newRow.ItemArray = new object[] { "A6BW0LL702", "XGU6BW0LL7", "AL11", 500, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), "EVESALH0600I1", "JH3_ESS_Laminating Half-Type" };
                dtMain.Rows.Add(newRow);

                newRow = dtMain.NewRow();
                newRow.ItemArray = new object[] { "A6BW0LL709", "XGU6BW0LL7", "AL150", 500, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), "EVESALH0600I1", "JH3_ESS_Laminating Half-Type" };
                dtMain.Rows.Add(newRow);

                newRow = dtMain.NewRow();
                newRow.ItemArray = new object[] { "A6BW0LL715", "XGU6BW0LL7", "Read_Fail", 500, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), "EVESALH0600I1", "JH3_ESS_Laminating Half-Type" };
                dtMain.Rows.Add(newRow);

                newRow = dtMain.NewRow();
                newRow.ItemArray = new object[] { "A6BW0LL717", "XGU6BW0LL7", "AL149", 500, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), "EVESALH0600I1", "JH3_ESS_Laminating Half-Type" };
                dtMain.Rows.Add(newRow);

                newRow = dtMain.NewRow();
                newRow.ItemArray = new object[] { "A6BW0LL721", "XGU6BW0LL7", "AL128", 500, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), "EVESALH0600I1", "JH3_ESS_Laminating Half-Type" };
                dtMain.Rows.Add(newRow);

                newRow = dtMain.NewRow();
                newRow.ItemArray = new object[] { "A6AY0LL409", "XGU6AY0LL4", "AL128", 500, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), "EVESALH0600I1", "JH3_ESS_Laminating Half-Type" };
                dtMain.Rows.Add(newRow);

                newRow = dtMain.NewRow();
                newRow.ItemArray = new object[] { "A6AY0LL412", "XGU6AY0LL4", "AL149", 500, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), "EVESALH0600I1", "JH3_ESS_Laminating Half-Type" };
                dtMain.Rows.Add(newRow);

                newRow = dtMain.NewRow();
                newRow.ItemArray = new object[] { "A6AY0LL422", "XGU6AY0LL4", "AL150", 500, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), "EVESALH0600I1", "JH3_ESS_Laminating Half-Type" };
                dtMain.Rows.Add(newRow);

                newRow = dtMain.NewRow();
                newRow.ItemArray = new object[] { "A6B01LL101", "XGU6B01LL1", "Read_Fail", 500, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), "EVESALH0600I1", "JH3_ESS_Laminating Half-Type" };
                dtMain.Rows.Add(newRow);

                newRow = dtMain.NewRow();
                newRow.ItemArray = new object[] { "A6B01LL107", "XGU6B01LL1", "AL150", 500, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), "EVESALH0600I1", "JH3_ESS_Laminating Half-Type" };
                dtMain.Rows.Add(newRow);

                newRow = dtMain.NewRow();
                newRow.ItemArray = new object[] { "A6B01LL109", "XGU6B01LL1", "Read_Fail", 500, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), "EVESALH0600I1", "JH3_ESS_Laminating Half-Type" };
                dtMain.Rows.Add(newRow);

                newRow = dtMain.NewRow();
                newRow.ItemArray = new object[] { "A6B01LL110", "XGU6B01LL1", "Read_Fail", 500, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), "EVESALH0600I1", "JH3_ESS_Laminating Half-Type" };
                dtMain.Rows.Add(newRow);

                newRow = dtMain.NewRow();
                newRow.ItemArray = new object[] { "A6B01LL117", "XGU6B01LL1", "AL149", 500, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), "EVESALH0600I1", "JH3_ESS_Laminating Half-Type" };
                dtMain.Rows.Add(newRow);

                newRow = dtMain.NewRow();
                newRow.ItemArray = new object[] { "A6C03LL201", "XGU6C03LL2", "AL128", 500, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), "EVESALH0600I1", "JH3_ESS_Laminating Half-Type" };
                dtMain.Rows.Add(newRow);

                newRow = dtMain.NewRow();
                newRow.ItemArray = new object[] { "A6C03LL202", "XGU6C03LL2", "Read_Fail", 500, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), "EVESALH0600I1", "JH3_ESS_Laminating Half-Type" };
                dtMain.Rows.Add(newRow);

                newRow = dtMain.NewRow();
                newRow.ItemArray = new object[] { "A6C03LL211", "XGU6C03LL2", "AL149", 500, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), "EVESALH0600I1", "JH3_ESS_Laminating Half-Type" };
                dtMain.Rows.Add(newRow);

                newRow = dtMain.NewRow();
                newRow.ItemArray = new object[] { "A6C03LL219", "XGU6C03LL2", "AL128", 500, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), "EVESALH0600I1", "JH3_ESS_Laminating Half-Type" };
                dtMain.Rows.Add(newRow);

                newRow = dtMain.NewRow();
                newRow.ItemArray = new object[] { "A6C04LL103", "XGU6C04LL1", "AL150", 500, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), "EVESALH0600I1", "JH3_ESS_Laminating Half-Type" };
                dtMain.Rows.Add(newRow);

                newRow = dtMain.NewRow();
                newRow.ItemArray = new object[] { "A6C04LL302", "XGU6C04LL3", "AL149", 500, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), "EVESALH0600I1", "JH3_ESS_Laminating Half-Type" };
                dtMain.Rows.Add(newRow);

                newRow = dtMain.NewRow();
                newRow.ItemArray = new object[] { "A6C04LL311", "XGU6C04LL3", "AL128", 500, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), "EVESALH0600I1", "JH3_ESS_Laminating Half-Type" };
                dtMain.Rows.Add(newRow);

                newRow = dtMain.NewRow();
                newRow.ItemArray = new object[] { "A6C07LL102", "XGU6C07LL1", "AL150", 500, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), "EVESALH0600I1", "JH3_ESS_Laminating Half-Type" };
                dtMain.Rows.Add(newRow);

                newRow = dtMain.NewRow();
                newRow.ItemArray = new object[] { "A6C07LL111", "XGU6C07LL1", "AL149", 500, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), "EVESALH0600I1", "JH3_ESS_Laminating Half-Type" };
                dtMain.Rows.Add(newRow);

                newRow = dtMain.NewRow();
                newRow.ItemArray = new object[] { "A6C07LL120", "XGU6C07LL1", "AL128", 500, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), "EVESALH0600I1", "JH3_ESS_Laminating Half-Type" };
                dtMain.Rows.Add(newRow);

                newRow = dtMain.NewRow();
                newRow.ItemArray = new object[] { "A6B17LL103", "XGU6B17LL1", "AL128", 500, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), "EVESALH0600I1", "JH3_ESS_Laminating Half-Type" };
                dtMain.Rows.Add(newRow);

                newRow = dtMain.NewRow();
                newRow.ItemArray = new object[] { "A6B17LL108", "XGU6B17LL1", "Read_Fail", 500, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), "EVESALH0600I1", "JH3_ESS_Laminating Half-Type" };
                dtMain.Rows.Add(newRow);

                newRow = dtMain.NewRow();
                newRow.ItemArray = new object[] { "A6B17LL115", "XGU6B17LL1", "AL149", 500, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), "EVESALH0600I1", "JH3_ESS_Laminating Half-Type" };
                dtMain.Rows.Add(newRow);

                newRow = dtMain.NewRow();
                newRow.ItemArray = new object[] { "A6B17LL117", "XGU6B17LL1", "AL150", 500, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), "EVESALH0600I1", "JH3_ESS_Laminating Half-Type" };
                dtMain.Rows.Add(newRow);

                newRow = dtMain.NewRow();
                newRow.ItemArray = new object[] { "A6C11LL301", "XGU6C11LL3", "Read_Fail", 500, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), "EVESALH0600I1", "JH3_ESS_Laminating Half-Type" };
                dtMain.Rows.Add(newRow);

                newRow = dtMain.NewRow();
                newRow.ItemArray = new object[] { "A6C11LL307", "XGU6C11LL3", "AL150", 500, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), "EVESALH0600I1", "JH3_ESS_Laminating Half-Type" };
                dtMain.Rows.Add(newRow);

                newRow = dtMain.NewRow();
                newRow.ItemArray = new object[] { "A6AX4LL808", "XGU6AX4LL8", "AL149", 500, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), "EVESALH0600I1", "JH3_ESS_Laminating Half-Type" };
                dtMain.Rows.Add(newRow);

                newRow = dtMain.NewRow();
                newRow.ItemArray = new object[] { "A6AX4LL809", "XGU6AX4LL8", "Read_Fail", 500, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), "EVESALH0600I1", "JH3_ESS_Laminating Half-Type" };
                dtMain.Rows.Add(newRow);

                newRow = dtMain.NewRow();
                newRow.ItemArray = new object[] { "A6AX4LL810", "XGU6AX4LL8", "Read_Fail", 500, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), "EVESALH0600I1", "JH3_ESS_Laminating Half-Type" };
                dtMain.Rows.Add(newRow);

                newRow = dtMain.NewRow();
                newRow.ItemArray = new object[] { "A6AX4LL811", "XGU6AX4LL8", "Read_Fail", 500, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), "EVESALH0600I1", "JH3_ESS_Laminating Half-Type" };
                dtMain.Rows.Add(newRow);

                newRow = dtMain.NewRow();
                newRow.ItemArray = new object[] { "A6AX4LL812", "XGU6AX4LL8", "Read_Fail", 500, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), "EVESALH0600I1", "JH3_ESS_Laminating Half-Type" };
                dtMain.Rows.Add(newRow);

                newRow = dtMain.NewRow();
                newRow.ItemArray = new object[] { "A6AX4LL813", "XGU6AX4LL8", "AL150", 500, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), "EVESALH0600I1", "JH3_ESS_Laminating Half-Type" };
                dtMain.Rows.Add(newRow);

                newRow = dtMain.NewRow();
                newRow.ItemArray = new object[] { "A6AX4LL818", "XGU6AX4LL8", "AL128", 500, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), "EVESALH0600I1", "JH3_ESS_Laminating Half-Type" };
                dtMain.Rows.Add(newRow);

                newRow = dtMain.NewRow();
                newRow.ItemArray = new object[] { "A6B08LL101", "XGU6B08LL1", "AL18A", 500, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), "EVESALH0600I1", "JH3_ESS_Laminating Half-Type" };
                dtMain.Rows.Add(newRow);

                newRow = dtMain.NewRow();
                newRow.ItemArray = new object[] { "A6D09LL301", "XGU6D09LL3", "", 977, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), "EVESALH0600I1", "JH3_ESS_Laminating Half-Type" };
                dtMain.Rows.Add(newRow);

                newRow = dtMain.NewRow();
                newRow.ItemArray = new object[] { "A6D20LL501", "XGU6D20LL5", "", 0, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), "EVESALH0600I1", "JH3_ESS_Laminating Half-Type" };
                dtMain.Rows.Add(newRow);

                newRow = dtMain.NewRow();
                newRow.ItemArray = new object[] { "A6E09LL301", "XGU6E09LL3", "", 44.668, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), "EVESALH0600I1", "JH3_ESS_Laminating Half-Type" };
                dtMain.Rows.Add(newRow);

                newRow = dtMain.NewRow();
                newRow.ItemArray = new object[] { "A6E13LLB05", "2GU6E13LLB", "AL120", 500, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), "EVESALH0600I1", "JH3_ESS_Laminating Half-Type" };
                dtMain.Rows.Add(newRow);

                newRow = dtMain.NewRow();
                newRow.ItemArray = new object[] { "A6F05LL901", "2GU6F05LL9", "AL124", 500, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), "EVESALH0600I1", "JH3_ESS_Laminating Half-Type" };
                dtMain.Rows.Add(newRow);

                newRow = dtMain.NewRow();
                newRow.ItemArray = new object[] { "A6F05LL902", "2GU6F05LL9", "AL113", 500, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), "EVESALH0600I1", "JH3_ESS_Laminating Half-Type" };
                dtMain.Rows.Add(newRow);

                newRow = dtMain.NewRow();
                newRow.ItemArray = new object[] { "A6F05LL904", "2GU6F05LL9", "AL112", 500, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), "EVESALH0600I1", "JH3_ESS_Laminating Half-Type" };
                dtMain.Rows.Add(newRow);

                newRow = dtMain.NewRow();
                newRow.ItemArray = new object[] { "A6F05LL908", "2GU6F05LL9", "AL121", 500, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), "EVESALH0600I1", "JH3_ESS_Laminating Half-Type" };
                dtMain.Rows.Add(newRow);

                newRow = dtMain.NewRow();
                newRow.ItemArray = new object[] { "A6F05LL911", "2GU6F05LL9", "AL125", 500, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), "EVESALH0600I1", "JH3_ESS_Laminating Half-Type" };
                dtMain.Rows.Add(newRow);

                newRow = dtMain.NewRow();
                newRow.ItemArray = new object[] { "A6F05LL912", "2GU6F05LL9", "AL120", 500, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), "EVESALH0600I1", "JH3_ESS_Laminating Half-Type" };
                dtMain.Rows.Add(newRow);

                newRow = dtMain.NewRow();
                newRow.ItemArray = new object[] { "A6F05LL913", "2GU6F05LL9", "AL115", 500, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), "EVESALH0600I1", "JH3_ESS_Laminating Half-Type" };
                dtMain.Rows.Add(newRow);

                newRow = dtMain.NewRow();
                newRow.ItemArray = new object[] { "A6F05LL914", "2GU6F05LL9", "AL124", 500, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), "EVESALH0600I1", "JH3_ESS_Laminating Half-Type" };
                dtMain.Rows.Add(newRow);

                newRow = dtMain.NewRow();
                newRow.ItemArray = new object[] { "A6F05LL915", "2GU6F05LL9", "AL103", 500, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), "EVESALH0600I1", "JH3_ESS_Laminating Half-Type" };
                dtMain.Rows.Add(newRow);

                newRow = dtMain.NewRow();
                newRow.ItemArray = new object[] { "A6F05LL917", "2GU6F05LL9", "AL112", 500, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), "EVESALH0600I1", "JH3_ESS_Laminating Half-Type" };
                dtMain.Rows.Add(newRow);

                newRow = dtMain.NewRow();
                newRow.ItemArray = new object[] { "A6F05LL920", "2GU6F05LL9", "AL108", 500, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), "EVESALH0600I1", "JH3_ESS_Laminating Half-Type" };
                dtMain.Rows.Add(newRow);

                newRow = dtMain.NewRow();
                newRow.ItemArray = new object[] { "A6F05LL921", "2GU6F05LL9", "AL116", 500, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), "EVESALH0600I1", "JH3_ESS_Laminating Half-Type" };
                dtMain.Rows.Add(newRow);

                newRow = dtMain.NewRow();
                newRow.ItemArray = new object[] { "A6F05LL923", "2GU6F05LL9", "AL119", 1200, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), "EVESALH0600I1", "JH3_ESS_Laminating Half-Type" };
                dtMain.Rows.Add(newRow);




            dgWaitMagazine.ItemsSource = DataTableConverter.Convert(dtMain);

        }

        private void GetOutProduct(object SelectedItem)
        {
            DataRowView rowview = SelectedItem as DataRowView;

            if (rowview == null)
                return;

            Util.gridClear(dgOutProduct);

            dtMain = new DataTable();
            dtMain.Columns.Add("BOXID", typeof(string));
            dtMain.Columns.Add("PRODID", typeof(string));
            dtMain.Columns.Add("OUTQTY", typeof(Int32));
            dtMain.Columns.Add("PRINTYN", typeof(string));
            dtMain.Columns.Add("INPUTDTTM", typeof(string));
            

            Random rnd = new Random();

            if (rnd.Next(0, 2) > 0)
            {

                newRow = dtMain.NewRow();
                newRow.ItemArray = new object[] { "26G48FL1K7", "EVESALH0600I1", 21, "N", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") };
                dtMain.Rows.Add(newRow);

                newRow = dtMain.NewRow();
                newRow.ItemArray = new object[] { "26G48FL1K6", "EVESALH0600I2", 21, "N", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") };
                dtMain.Rows.Add(newRow);

                newRow = dtMain.NewRow();
                newRow.ItemArray = new object[] { "26G48FL1K5", "EVESALH0600I3", 21, "N", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") };
                dtMain.Rows.Add(newRow);

                newRow = dtMain.NewRow();
                newRow.ItemArray = new object[] { "26G48FL1K4", "EVESALH0600I4", 21, "N", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") };
                dtMain.Rows.Add(newRow);

                newRow = dtMain.NewRow();
                newRow.ItemArray = new object[] { "26G48FL1K3", "EVESALH0600I5", 21, "N", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") };
                dtMain.Rows.Add(newRow);

                newRow = dtMain.NewRow();
                newRow.ItemArray = new object[] { "26G48FL1K2", "EVESALH0600I6", 21, "N", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") };
                dtMain.Rows.Add(newRow);

                newRow = dtMain.NewRow();
                newRow.ItemArray = new object[] { "26G48FL1K1", "EVESALH0600I7", 21, "N", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") };
                dtMain.Rows.Add(newRow);

                newRow = dtMain.NewRow();
                newRow.ItemArray = new object[] { "26G48FL1K0", "EVESALH0600I8", 21, "N", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") };
                dtMain.Rows.Add(newRow);

                newRow = dtMain.NewRow();
                newRow.ItemArray = new object[] { "26G48FL1J9", "EVESALH0600I9", 21, "N", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") };
                dtMain.Rows.Add(newRow);

                newRow = dtMain.NewRow();
                newRow.ItemArray = new object[] { "26G48FL1J8", "EVESALH0600I10", 21, "N", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") };
                dtMain.Rows.Add(newRow);

                newRow = dtMain.NewRow();
                newRow.ItemArray = new object[] { "26G48FL1J7", "EVESALH0600I11", 21, "Y", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") };
                dtMain.Rows.Add(newRow);

                newRow = dtMain.NewRow();
                newRow.ItemArray = new object[] { "26G48FL1J6", "EVESALH0600I12", 21, "Y", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") };
                dtMain.Rows.Add(newRow);

                newRow = dtMain.NewRow();
                newRow.ItemArray = new object[] { "26G48FL1J5", "EVESALH0600I13", 21, "Y", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") };
                dtMain.Rows.Add(newRow);

                newRow = dtMain.NewRow();
                newRow.ItemArray = new object[] { "26G48FL1J4", "EVESALH0600I14", 21, "Y", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") };
                dtMain.Rows.Add(newRow);

                newRow = dtMain.NewRow();
                newRow.ItemArray = new object[] { "26G48FL1J3", "EVESALH0600I15", 21, "Y", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") };
                dtMain.Rows.Add(newRow);

                newRow = dtMain.NewRow();
                newRow.ItemArray = new object[] { "26G48FL1J2", "EVESALH0600I16", 21, "Y", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") };
                dtMain.Rows.Add(newRow);

                newRow = dtMain.NewRow();
                newRow.ItemArray = new object[] { "26G48FL1J1", "EVESALH0600I17", 21, "Y", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") };
                dtMain.Rows.Add(newRow);

                newRow = dtMain.NewRow();
                newRow.ItemArray = new object[] { "26G48FL1J0", "EVESALH0600I18", 21, "Y", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") };
                dtMain.Rows.Add(newRow);

                newRow = dtMain.NewRow();
                newRow.ItemArray = new object[] { "26G48FL1H9", "EVESALH0600I19", 21, "Y", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") };
                dtMain.Rows.Add(newRow);

                newRow = dtMain.NewRow();
                newRow.ItemArray = new object[] { "26G48FL1H8", "EVESALH0600I20", 21, "Y", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") };
                dtMain.Rows.Add(newRow);

                newRow = dtMain.NewRow();
                newRow.ItemArray = new object[] { "26G48FL1H7", "EVESALH0600I21", 21, "Y", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") };
                dtMain.Rows.Add(newRow);

                newRow = dtMain.NewRow();
                newRow.ItemArray = new object[] { "26G48FL1H6", "EVESALH0600I22", 21, "Y", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") };
                dtMain.Rows.Add(newRow);

                newRow = dtMain.NewRow();
                newRow.ItemArray = new object[] { "26G48FL1H5", "EVESALH0600I23", 21, "Y", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") };
                dtMain.Rows.Add(newRow);

                newRow = dtMain.NewRow();
                newRow.ItemArray = new object[] { "26G48FL1H4", "EVESALH0600I24", 21, "Y", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") };
                dtMain.Rows.Add(newRow);

                newRow = dtMain.NewRow();
                newRow.ItemArray = new object[] { "26G48FL1H3", "EVESALH0600I25", 21, "Y", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") };
                dtMain.Rows.Add(newRow);

                newRow = dtMain.NewRow();
                newRow.ItemArray = new object[] { "26G48FL1H2", "EVESALH0600I26", 21, "Y", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") };
                dtMain.Rows.Add(newRow);

                newRow = dtMain.NewRow();
                newRow.ItemArray = new object[] { "26G48FL1H1", "EVESALH0600I27", 21, "Y", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") };
                dtMain.Rows.Add(newRow);

                newRow = dtMain.NewRow();
                newRow.ItemArray = new object[] { "26G48FL1H0", "EVESALH0600I28", 21, "Y", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") };
                dtMain.Rows.Add(newRow);

                newRow = dtMain.NewRow();
                newRow.ItemArray = new object[] { "26G48FL1G9", "EVESALH0600I29", 21, "Y", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") };
                dtMain.Rows.Add(newRow);

                newRow = dtMain.NewRow();
                newRow.ItemArray = new object[] { "26G48FL1G8", "EVESALH0600I30", 21, "Y", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") };
                dtMain.Rows.Add(newRow);

                newRow = dtMain.NewRow();
                newRow.ItemArray = new object[] { "26G48FL1G7", "EVESALH0600I31", 21, "Y", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") };
                dtMain.Rows.Add(newRow);

                newRow = dtMain.NewRow();
                newRow.ItemArray = new object[] { "26G48FL1G6", "EVESALH0600I32", 21, "Y", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") };
                dtMain.Rows.Add(newRow);

                newRow = dtMain.NewRow();
                newRow.ItemArray = new object[] { "26G48FL1G5", "EVESALH0600I33", 21, "Y", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") };
                dtMain.Rows.Add(newRow);

                newRow = dtMain.NewRow();
                newRow.ItemArray = new object[] { "26G48FL1G4", "EVESALH0600I34", 21, "Y", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") };
                dtMain.Rows.Add(newRow);

                newRow = dtMain.NewRow();
                newRow.ItemArray = new object[] { "26G48FL1G3", "EVESALH0600I35", 21, "Y", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") };
                dtMain.Rows.Add(newRow);

                newRow = dtMain.NewRow();
                newRow.ItemArray = new object[] { "26G48FL1G2", "EVESALH0600I36", 21, "Y", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") };
                dtMain.Rows.Add(newRow);

                newRow = dtMain.NewRow();
                newRow.ItemArray = new object[] { "26G48FL1G1", "EVESALH0600I37", 21, "Y", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") };
                dtMain.Rows.Add(newRow);

                newRow = dtMain.NewRow();
                newRow.ItemArray = new object[] { "26G48FL1G0", "EVESALH0600I38", 21, "Y", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") };
                dtMain.Rows.Add(newRow);

                newRow = dtMain.NewRow();
                newRow.ItemArray = new object[] { "26G48FL1F9", "EVESALH0600I39", 21, "Y", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") };
                dtMain.Rows.Add(newRow);

                newRow = dtMain.NewRow();
                newRow.ItemArray = new object[] { "26G48FL1F8", "EVESALH0600I40", 21, "Y", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") };
                dtMain.Rows.Add(newRow);

                newRow = dtMain.NewRow();
                newRow.ItemArray = new object[] { "26G48FL1F7", "EVESALH0600I41", 21, "Y", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") };
                dtMain.Rows.Add(newRow);

                newRow = dtMain.NewRow();
                newRow.ItemArray = new object[] { "26G48FL1F6", "EVESALH0600I42", 21, "Y", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") };
                dtMain.Rows.Add(newRow);

                newRow = dtMain.NewRow();
                newRow.ItemArray = new object[] { "26G48FL1F5", "EVESALH0600I43", 21, "Y", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") };
                dtMain.Rows.Add(newRow);

                newRow = dtMain.NewRow();
                newRow.ItemArray = new object[] { "26G48FL1F4", "EVESALH0600I44", 21, "Y", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") };
                dtMain.Rows.Add(newRow);

                newRow = dtMain.NewRow();
                newRow.ItemArray = new object[] { "26G48FL1F3", "EVESALH0600I45", 21, "Y", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") };
                dtMain.Rows.Add(newRow);

                newRow = dtMain.NewRow();
                newRow.ItemArray = new object[] { "26G48FL1F2", "EVESALH0600I46", 21, "Y", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") };
                dtMain.Rows.Add(newRow);

                newRow = dtMain.NewRow();
                newRow.ItemArray = new object[] { "26G48FL1F1", "EVESALH0600I47", 21, "Y", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") };
                dtMain.Rows.Add(newRow);

                newRow = dtMain.NewRow();
                newRow.ItemArray = new object[] { "26G48FL1F0", "EVESALH0600I48", 21, "Y", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") };
                dtMain.Rows.Add(newRow);

                newRow = dtMain.NewRow();
                newRow.ItemArray = new object[] { "26G48FL1E9", "EVESALH0600I49", 21, "Y", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") };
                dtMain.Rows.Add(newRow);

                newRow = dtMain.NewRow();
                newRow.ItemArray = new object[] { "26G48FL1E8", "EVESALH0600I50", 21, "Y", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") };
                dtMain.Rows.Add(newRow);

                newRow = dtMain.NewRow();
                newRow.ItemArray = new object[] { "26G48FL1E7", "EVESALH0600I51", 21, "Y", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") };
                dtMain.Rows.Add(newRow);

                newRow = dtMain.NewRow();
                newRow.ItemArray = new object[] { "26G48FL1E6", "EVESALH0600I52", 21, "Y", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") };
                dtMain.Rows.Add(newRow);

                newRow = dtMain.NewRow();
                newRow.ItemArray = new object[] { "26G48FL1E5", "EVESALH0600I53", 21, "Y", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") };
                dtMain.Rows.Add(newRow);

                newRow = dtMain.NewRow();
                newRow.ItemArray = new object[] { "26G48FL1E4", "EVESALH0600I54", 21, "Y", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") };
                dtMain.Rows.Add(newRow);

                newRow = dtMain.NewRow();
                newRow.ItemArray = new object[] { "26G48FL1E3", "EVESALH0600I55", 21, "Y", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") };
                dtMain.Rows.Add(newRow);

                newRow = dtMain.NewRow();
                newRow.ItemArray = new object[] { "26G48FL1E2", "EVESALH0600I56", 21, "Y", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") };
                dtMain.Rows.Add(newRow);

                newRow = dtMain.NewRow();
                newRow.ItemArray = new object[] { "26G48FL1E1", "EVESALH0600I57", 21, "Y", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") };
                dtMain.Rows.Add(newRow);

                newRow = dtMain.NewRow();
                newRow.ItemArray = new object[] { "26G48FL1E0", "EVESALH0600I58", 21, "Y", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") };
                dtMain.Rows.Add(newRow);

                newRow = dtMain.NewRow();
                newRow.ItemArray = new object[] { "26G48FL1D9", "EVESALH0600I59", 21, "Y", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") };
                dtMain.Rows.Add(newRow);

                newRow = dtMain.NewRow();
                newRow.ItemArray = new object[] { "26G48FL1D8", "EVESALH0600I60", 21, "Y", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") };
                dtMain.Rows.Add(newRow);

                newRow = dtMain.NewRow();
                newRow.ItemArray = new object[] { "26G48FL1D7", "EVESALH0600I61", 21, "Y", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") };
                dtMain.Rows.Add(newRow);

                newRow = dtMain.NewRow();
                newRow.ItemArray = new object[] { "26G48FL1D6", "EVESALH0600I62", 21, "Y", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") };
                dtMain.Rows.Add(newRow);

                newRow = dtMain.NewRow();
                newRow.ItemArray = new object[] { "26G48FL1D5", "EVESALH0600I63", 21, "Y", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") };
                dtMain.Rows.Add(newRow);

                newRow = dtMain.NewRow();
                newRow.ItemArray = new object[] { "26G48FL1D4", "EVESALH0600I64", 21, "Y", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") };
                dtMain.Rows.Add(newRow);

                newRow = dtMain.NewRow();
                newRow.ItemArray = new object[] { "26G48FL1D3", "EVESALH0600I65", 21, "Y", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") };
                dtMain.Rows.Add(newRow);

                newRow = dtMain.NewRow();
                newRow.ItemArray = new object[] { "26G48FL1D2", "EVESALH0600I66", 21, "Y", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") };
                dtMain.Rows.Add(newRow);

                newRow = dtMain.NewRow();
                newRow.ItemArray = new object[] { "26G48FL1D1", "EVESALH0600I67", 21, "Y", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") };
                dtMain.Rows.Add(newRow);

                newRow = dtMain.NewRow();
                newRow.ItemArray = new object[] { "26G48FL1D0", "EVESALH0600I68", 21, "Y", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") };
                dtMain.Rows.Add(newRow);

                newRow = dtMain.NewRow();
                newRow.ItemArray = new object[] { "26G48FL1C9", "EVESALH0600I69", 21, "Y", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") };
                dtMain.Rows.Add(newRow);

                newRow = dtMain.NewRow();
                newRow.ItemArray = new object[] { "26G48FL1C8", "EVESALH0600I70", 21, "Y", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") };
                dtMain.Rows.Add(newRow);

                newRow = dtMain.NewRow();
                newRow.ItemArray = new object[] { "26G48FL1C7", "EVESALH0600I71", 21, "Y", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") };
                dtMain.Rows.Add(newRow);

                newRow = dtMain.NewRow();
                newRow.ItemArray = new object[] { "26G48FL1C6", "EVESALH0600I72", 21, "Y", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") };
                dtMain.Rows.Add(newRow);

                newRow = dtMain.NewRow();
                newRow.ItemArray = new object[] { "26G48FL1C5", "EVESALH0600I73", 21, "Y", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") };
                dtMain.Rows.Add(newRow);

                newRow = dtMain.NewRow();
                newRow.ItemArray = new object[] { "26G48FL1C4", "EVESALH0600I74", 21, "Y", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") };
                dtMain.Rows.Add(newRow);

                newRow = dtMain.NewRow();
                newRow.ItemArray = new object[] { "26G48FL1C3", "EVESALH0600I75", 21, "Y", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") };
                dtMain.Rows.Add(newRow);

                newRow = dtMain.NewRow();
                newRow.ItemArray = new object[] { "26G48FL1C2", "EVESALH0600I76", 21, "Y", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") };
                dtMain.Rows.Add(newRow);

                newRow = dtMain.NewRow();
                newRow.ItemArray = new object[] { "26G48FL1C1", "EVESALH0600I77", 21, "Y", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") };
                dtMain.Rows.Add(newRow);

                newRow = dtMain.NewRow();
                newRow.ItemArray = new object[] { "26G48FL1C0", "EVESALH0600I78", 21, "Y", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") };
                dtMain.Rows.Add(newRow);

                newRow = dtMain.NewRow();
                newRow.ItemArray = new object[] { "26G48FL1B9", "EVESALH0600I79", 21, "Y", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") };
                dtMain.Rows.Add(newRow);

                newRow = dtMain.NewRow();
                newRow.ItemArray = new object[] { "26G48FL1B8", "EVESALH0600I80", 21, "Y", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") };
                dtMain.Rows.Add(newRow);

                newRow = dtMain.NewRow();
                newRow.ItemArray = new object[] { "26G48FL1B7", "EVESALH0600I81", 21, "Y", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") };
                dtMain.Rows.Add(newRow);

                newRow = dtMain.NewRow();
                newRow.ItemArray = new object[] { "26G48FL1B6", "EVESALH0600I82", 21, "Y", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") };
                dtMain.Rows.Add(newRow);

                newRow = dtMain.NewRow();
                newRow.ItemArray = new object[] { "26G48FL1B5", "EVESALH0600I83", 21, "Y", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") };
                dtMain.Rows.Add(newRow);

                newRow = dtMain.NewRow();
                newRow.ItemArray = new object[] { "26G48FL1B4", "EVESALH0600I84", 21, "Y", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") };
                dtMain.Rows.Add(newRow);

                newRow = dtMain.NewRow();
                newRow.ItemArray = new object[] { "26G48FL1B3", "EVESALH0600I85", 21, "Y", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") };
                dtMain.Rows.Add(newRow);

                newRow = dtMain.NewRow();
                newRow.ItemArray = new object[] { "26G48FL1B2", "EVESALH0600I86", 21, "Y", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") };
                dtMain.Rows.Add(newRow);

                newRow = dtMain.NewRow();
                newRow.ItemArray = new object[] { "26G48FL1B1", "EVESALH0600I87", 21, "Y", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") };
                dtMain.Rows.Add(newRow);

                newRow = dtMain.NewRow();
                newRow.ItemArray = new object[] { "26G48FL1B0", "EVESALH0600I88", 21, "Y", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") };
                dtMain.Rows.Add(newRow);

                newRow = dtMain.NewRow();
                newRow.ItemArray = new object[] { "26G48FL1A9", "EVESALH0600I89", 21, "Y", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") };
                dtMain.Rows.Add(newRow);

            }
            else
            {
                newRow = dtMain.NewRow();
                newRow.ItemArray = new object[] { "26G48FL1J8", "EVESALH0600I10", 21, "N", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") };
                dtMain.Rows.Add(newRow);

                newRow = dtMain.NewRow();
                newRow.ItemArray = new object[] { "26G48FL1J7", "EVESALH0600I11", 21, "Y", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") };
                dtMain.Rows.Add(newRow);
            }

            dgOutProduct.ItemsSource = DataTableConverter.Convert(dtMain);

            //SetSumRow();
        }

        //private void SetSumRow()
        //{
        //    if (dgOutProduct.ItemsSource == null)
        //        return;

        //    if(dgOutProduct.Rows.Count > 0)
        //    {
        //        DataTable dt = DataTableConverter.Convert(dgOutProduct.ItemsSource);
        //        DataRow newRow = dt.NewRow();
        //        object[] oTmp = new object[dt.Columns.Count];
        //        //for(int i = 0 ; i < dgOutProduct.Columns.Count ; i++)
        //        //{
        //        //    DataRowView dgRow = (dgOutProduct.Rows[i].DataItem as DataRowView);

        //        //    oTmp[i] = "aa";
        //        //}

        //        oTmp[dgOutProduct.Columns["OUTQTY"].Index - 1] = dt.Compute("SUM(OUTQTY)", String.Empty); 
        //        newRow.ItemArray = oTmp;
        //        dt.Rows.Add(newRow);

        //        dgOutProduct.ItemsSource = DataTableConverter.Convert(dt);

        //        //DataGridAggregate.SetAggregateFunctions(dgOutProduct.Columns["OUTQTY"], new DataGridAggregatesCollection { new DataGridAggregateSum { ResultFormat = "SUM = {0}" } });
        //        dgOutProduct.Rows[dgOutProduct.Rows.Count - 1].DataItem.RowStyle = new Style();
        //    }
        //}

        private void SetElecTypeCount()
        {
            int iHalfCnt = 0;
            int iMonoCnt = 0;

            if (dgInMagazine.ItemsSource == null || dgInMagazine.Rows.Count < 1)
                return;

            for(int i=0;i<dgInMagazine.Rows.Count;i++)
            {
                if (DataTableConverter.GetValue(dgInMagazine.Rows[i].DataItem, "ELECTYPE").ToString().Equals("C")) iHalfCnt++;
                else if (DataTableConverter.GetValue(dgInMagazine.Rows[i].DataItem, "ELECTYPE").ToString().Equals("A")) iMonoCnt++;
            }
            txtHalfCnt.Text = iHalfCnt.ToString();
            txtMonoCnt.Text = iMonoCnt.ToString();
        }

        private void popup_Closed(object sender, EventArgs e)
        {
        }

        private void wndRunStart_Closed(object sender, EventArgs e)
        {
            PGM_GUI_196_RUNSTART runStartWindow = sender as PGM_GUI_196_RUNSTART;
            if (runStartWindow.DialogResult == MessageBoxResult.OK)
            {
                GetProductLot();
            }
        }

        private void wndConfirm_Closed(object sender, EventArgs e)
        {
            PGM_GUI_196_CONFIRM runStartWindow = sender as PGM_GUI_196_CONFIRM;
            if (runStartWindow.DialogResult == MessageBoxResult.OK)
            {
                GetProductLot();
            }
        }

        private bool CanStartRun()
        {
            bool bRet = true;

            if (dgProductLot.ItemsSource == null)
                return false;

            for(int i=0; i< dgProductLot.Rows.Count;i++)
            {
                if (DataTableConverter.GetValue(dgProductLot.Rows[i].DataItem, "WIPSTAT").ToString().Equals("RUN"))
                {
                    LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("RUN 상태의 LOT이 존재 합니다."), null, "Info", MessageBoxButton.OK, MessageBoxIcon.Warning);
                    bRet = false;
                    break;
                }
            }
            return bRet;
        }

        private bool CanRunComplete()
        {
            bool bRet = true;

            if (dgInMagazine.ItemsSource == null)
                return bRet;

            if (dgInMagazine.Rows.Count > 0)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("투입된 매거진을 모두 완료 후 실행 하세요."), null, "Info", MessageBoxButton.OK, MessageBoxIcon.Warning);
                bRet = false;
            }
            return bRet;
        }

        private bool CanConfirm()
        {
            bool bRet = false;

            if (dgProductLot.ItemsSource == null)
                return bRet;

            //for (int i = 0; i < dgProductLot.Rows.Count; i++)
            //{
            //    if ((dgProductLot.GetCell(i, dgProductLot.Columns["CHK"].Index).Presenter.Content as CheckBox).IsChecked.HasValue &&
            //        (bool)(dgProductLot.GetCell(i, dgProductLot.Columns["CHK"].Index).Presenter.Content as CheckBox).IsChecked)
            //    {
            //        bRet = true;
            //        break;
            //    }
            //}

            if (_Util.GetDataGridCheckFirstRowIndex(dgProductLot, "CHK") > -1)
                bRet = true;

            if (!bRet)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("LOT이 선택되지 않았습니다."), null, "Info", MessageBoxButton.OK, MessageBoxIcon.Warning);
                return bRet; 
            }
            

            // Short 불량 Check...
            //int iSumBoxCnt = int.Parse(DataTableConverter.GetValue(dgOutProduct.Rows[dgOutProduct.Rows.Count - 1].DataItem, "OUTQTY").ToString());  // 바구니수량 합계
            int iSumBoxCnt = int.Parse(DataTableConverter.Convert(dgOutProduct.ItemsSource).Compute("SUM(OUTQTY)", String.Empty).ToString());   // 바구니수량 합계
            int iShortCnt = GetShortDefectCount();
            
            double dShortRate = double.Parse(iShortCnt.ToString()) / double.Parse(iSumBoxCnt.ToString());   // Short 불량율
            double dMaxShortRate = GetShorDefectRate();    // 시스템에 등록된 최대 범위 불량율

            if (dShortRate > dMaxShortRate)
            {
                string sMsg = "쇼트불량률이 범위를 초과하였습니다!  \r\n\r\n" + "수량합계 : " + iSumBoxCnt.ToString() + "개 | 불량쇼트 : " + iShortCnt.ToString() + "개 | 불량률 : "
                             + (dShortRate * 100).ToString("###.#") + "% \r\n\r\n"
                             + "쇼트불량 개수를 수정하시고 다시 진행하십시요!   \r\n\r\n" + "수정할려면 [취소] / 무시하고 계속진행은 [확인]";
                
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(sMsg), null, "Information", MessageBoxButton.OKCancel, MessageBoxIcon.None, (result) =>
                {
                    if (result == MessageBoxResult.Cancel)
                    {
                        bRet = false;
                    }
                });
            }

            return bRet;
        }

        private bool RunComplete()
        {
            bool bRet = false;


            return bRet;
        }

        private double GetShorDefectRate()
        {
            double dShortRate = 0;


            dShortRate = dShortRate / 100;
            return dShortRate;
        }

        private int GetShortDefectCount()
        {
            int iShortCnt = 0;

            return iShortCnt;
        }

        private bool CanInputWaitMagazine()
        {
            bool bRet = false;

            if (_Util.GetDataGridCheckFirstRowIndex(dgProductLot, "CHK") < 0)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("LOT이 선택되지 않았습니다."), null, "Info", MessageBoxButton.OK, MessageBoxIcon.Warning);
                return bRet;
            }

            if(rdoHalf.IsChecked.HasValue && (bool)rdoHalf.IsChecked)
            {
                if(int.Parse(txtHalfCnt.Text) >= int.Parse(txtHalfTotCnt.Text))
                {
                    LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("Half 타입은 더이상 투입할 수 없습니다."), null, "Info", MessageBoxButton.OK, MessageBoxIcon.Warning);
                    return bRet;
                }
            }

            if(rdoMono.IsChecked.HasValue && (bool)rdoMono.IsChecked)
            {
                if(int.Parse(txtMonoCnt.Text) >= int.Parse(txtMonoTotCnt.Text))
                {
                    LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("Mono 타입은 더이상 투입할 수 없습니다."), null, "Info", MessageBoxButton.OK, MessageBoxIcon.Warning);
                    return bRet;
                }
            }

            if(txtInPosition.Text.Trim().Equals(""))
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("투입 위치를 입력 하세요."), null, "Info", MessageBoxButton.OK, MessageBoxIcon.Warning);
                return bRet;
            }

            bRet = true;

            return bRet;
        }

        private bool CheckInputMagazine(string sMagazineID)
        {
            bool bRet = false;

            // Magazine Check Biz Call...




            return bRet;
        }

        private bool MagazineInput()
        {
            bool bRet = false;

            return bRet;
        }

        private bool CanBeChangeInputMagazineInfo()
        {
            bool bRet = false;

            if (_Util.GetDataGridCheckFirstRowIndex(dgInMagazine, "CHK") < 0)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("선택된 항목이 없습니다."), null, "Info", MessageBoxButton.OK, MessageBoxIcon.Warning);
                return bRet;
            }

            bRet = true;

            return bRet;
        }

        private bool CanBeChangeOutputBoxInfo()
        {
            bool bRet = false;

            if (_Util.GetDataGridCheckFirstRowIndex(dgOutProduct, "CHK") < 0)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("선택된 항목이 없습니다."), null, "Info", MessageBoxButton.OK, MessageBoxIcon.Warning);
                return bRet;
            }

            bRet = true;

            return bRet;
        }

        private bool CancelInputMagazine()
        {
            bool bRet = false;

            return bRet;
        }
        
        private bool ReplaceInputMagazine()
        {
            bool bRet = false;


            return bRet;
        }

        private bool CompleteInputMagazine()
        {
            bool bRet = false;

            return bRet;
        }

        private bool CreateOutBox(out string sBoxID)
        {
            bool bRet = false;

            sBoxID = "";
            return bRet;
        }
        
        private void BoxIDPrint()
        {
            for(int i=0;i< dgOutProduct.Rows.Count - dgOutProduct.BottomRows.Count; i++)
            {
                if (dgOutProduct.GetCell(i, dgOutProduct.Columns["CHK"].Index).Presenter != null
                            && dgOutProduct.GetCell(i, dgOutProduct.Columns["CHK"].Index).Presenter.Content != null
                            && (dgOutProduct.GetCell(i, dgOutProduct.Columns["CHK"].Index).Presenter.Content as CheckBox).IsChecked.HasValue
                            && (bool)(dgOutProduct.GetCell(i, dgOutProduct.Columns["CHK"].Index).Presenter.Content as CheckBox).IsChecked)
                {
                    //Print Function Call.....



                }
            }
        }

        private bool DeleteOutBox()
        {
            bool bRet = false;

            return bRet;
        }
        #endregion

        private void btnInput_Click(object sender, RoutedEventArgs e)
        {
            if (!CanInputWaitMagazine())
                return;

            int idx = _Util.GetDataGridCheckFirstRowIndex(dgWaitMagazine, "CHK");
            if (idx < 0)
                return;

            string sMagID = DataTableConverter.GetValue(dgWaitMagazine.Rows[idx].DataItem, "MAGAZINEID").ToString();
            string sQty = DataTableConverter.GetValue(dgWaitMagazine.Rows[idx].DataItem, "QTY").ToString();

            if (!CheckInputMagazine(sMagID))
                return;


            LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("투입 하시겠습니까?"), null, "Information", MessageBoxButton.OKCancel, MessageBoxIcon.None, (result) =>
            {
                if (result == MessageBoxResult.OK)
                {
                    if (MagazineInput())
                    {
                        GetProductLot();
                        LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("정상 처리 되었습니다."), null, "Info", MessageBoxButton.OK, MessageBoxIcon.Warning);
                    }
                }
            });

        }

        private void btnInCancel_Click(object sender, RoutedEventArgs e)
        {
            if (!CanBeChangeInputMagazineInfo())
                return;


            LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("취소 하시겠습니까?"), null, "Information", MessageBoxButton.OKCancel, MessageBoxIcon.None, (result) =>
            {
                if (result == MessageBoxResult.OK)
                {
                    if (CancelInputMagazine())
                    {
                        GetProductLot();
                        LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("정상 처리 되었습니다."), null, "Info", MessageBoxButton.OK, MessageBoxIcon.Warning);
                    }
                }
            });
        }

        private void btnInReplace_Click(object sender, RoutedEventArgs e)
        {
            if (!CanBeChangeInputMagazineInfo())
                return;

            LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("교체처리 하시겠습니까?"), null, "Information", MessageBoxButton.OKCancel, MessageBoxIcon.None, (result) =>
            {
                if (result == MessageBoxResult.OK)
                {
                    if (ReplaceInputMagazine())
                    {
                        GetProductLot();
                        LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("정상 처리 되었습니다."), null, "Info", MessageBoxButton.OK, MessageBoxIcon.Warning);
                    }                    
                }
            });
        }

        private void btnInComplete_Click(object sender, RoutedEventArgs e)
        {
            if (!CanBeChangeInputMagazineInfo())
                return;

            LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("투입완료 처리 하시겠습니까?"), null, "Information", MessageBoxButton.OKCancel, MessageBoxIcon.None, (result) =>
            {
                if (result == MessageBoxResult.OK)
                {
                    if (CompleteInputMagazine())
                    {
                        GetProductLot();
                        LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("정상 처리 되었습니다."), null, "Info", MessageBoxButton.OK, MessageBoxIcon.Warning);
                    }
                }
            });
        }

        private void btnCreateBOX_Click(object sender, RoutedEventArgs e)
        {
            if (Util.CheckDecimal(txtOutBoxQty.Text, 0))
                return;

            LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("생성 하시겠습니까?"), null, "Information", MessageBoxButton.OKCancel, MessageBoxIcon.None, (result) =>
            {
                if (result == MessageBoxResult.OK)
                {
                    string sBoxID;
                    if (CreateOutBox(out sBoxID))
                    {
                        GetProductLot();

                        _Util.SetDataGridCheck(dgOutProduct, "CHK", "MAGAZINEID", sBoxID);

                        if (chkAutoPrint.IsChecked.HasValue && (bool)chkAutoPrint.IsChecked)
                            BoxIDPrint();

                        LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("정상 처리 되었습니다."), null, "Info", MessageBoxButton.OK, MessageBoxIcon.Warning);
                    }
                }
            });
        }

        private void btnDeleteBOX_Click(object sender, RoutedEventArgs e)
        {
            if (!CanBeChangeOutputBoxInfo())
                return;

            LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("삭제 하시겠습니까?"), null, "Information", MessageBoxButton.OKCancel, MessageBoxIcon.None, (result) =>
            {
                if (result == MessageBoxResult.OK)
                {
                    if (DeleteOutBox())
                    {
                        GetProductLot();

                        LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("정상 처리 되었습니다."), null, "Info", MessageBoxButton.OK, MessageBoxIcon.Warning);
                    }
                }
            });
        }

        private void btnOutPrint_Click(object sender, RoutedEventArgs e)
        {
            if (!CanBeChangeOutputBoxInfo())
                return;

            LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("인쇄 하시겠습니까?"), null, "Information", MessageBoxButton.OKCancel, MessageBoxIcon.None, (result) =>
            {
                if (result == MessageBoxResult.OK)
                {
                    BoxIDPrint();

                    LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("인쇄 완료 되었습니다."), null, "Info", MessageBoxButton.OK, MessageBoxIcon.Warning);                    
                }
            });
        }
    }
}
