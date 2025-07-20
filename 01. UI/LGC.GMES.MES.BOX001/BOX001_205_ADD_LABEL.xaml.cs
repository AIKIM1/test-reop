/*************************************************************************************
 Created Date : 2018.10.09
      Creator : 이제섭
   Decription : 고객사(sunwoda) 라벨 발행
--------------------------------------------------------------------------------------
 [Change History]
 2018.10.09 : 최초생성 (GMES-R0424)
    
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
using LGC.GMES.MES.CMM001.Extensions;
using DataGridRow = C1.WPF.DataGrid.DataGridRow;

namespace LGC.GMES.MES.BOX001
{
    /// <summary>
    /// BOX001_205_ADD_LABEL.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class BOX001_205_ADD_LABEL : C1Window, IWorkArea
    {
        #region Declaration & Constructor

        // private string _procID = string.Empty;        // 공정코드
        //private string _eqptID = string.Empty;        // 설비코드
        private string _userid = string.Empty;        // 작업자ID

        public bool QueryCall { get; set; }

        private readonly BizDataSet _bizDataSet = new BizDataSet();

        private bool _load = true;

        Util _util = new Util();

        string _mkt = string.Empty;

        //string sPrt = string.Empty;
        //string sRes = string.Empty;
        //string sCopy = string.Empty;
        //string sXpos = string.Empty;
        //string sYpos = string.Empty;
        //string sDark = string.Empty;
        DataRow drPrtInfo = null;

        string _sPGM_ID = "BOX001_205_ADD_LABEL";

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

        #endregion

        #region Initialize
        /// <summary>
        ///  
        /// </summary>
        public IFrameOperation FrameOperation
        {
            get;
            set;
        }

        public BOX001_205_ADD_LABEL()
        {
            InitializeComponent();
        }

        #endregion


        #region [Form Load]
        private void C1Window_Loaded(object sender, RoutedEventArgs e)
        {
            if (_load)
            {
                InitializeUserControls();
                SetControl();

                _load = false;
            }
        }

        private void InitializeUserControls()
        {

        }

        private void SetControl()
        {
            object[] tmps = C1WindowExtension.GetParameters(this);

            txtPalletID.Text = Util.NVC(tmps[0]) as string;
            txtExpdom.Text = Util.NVC(tmps[2]) as string;
            _mkt = Util.NVC(tmps[3]) as string;


            //_util.GetConfigPrintInfo(out sPrt, out sRes, out sCopy, out sXpos, out sYpos, out sDark, out drPrtInfo);

            //GetBoxInfo();
            SetOrdercombo(cboOrder);

        }
        #endregion

        private void SetOrdercombo(C1ComboBox cbo)
        {
            const string bizRuleName = "DA_PRD_SEL_EXP_DOM_TYPE_CBO_NJ";
            string[] arrColumn = { "ATTRIBUTE1", "LANGID" };
            string[] arrCondition = { _mkt, LoginInfo.LANGID };
            string selectedValueText = cbo.SelectedValuePath;
            string displayMemberText = cbo.DisplayMemberPath;

            CommonCombo.CommonBaseCombo(bizRuleName, cbo, arrColumn, arrCondition, CommonCombo.ComboStatus.SELECT, selectedValueText, displayMemberText);
        }

        #region 
        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = MessageBoxResult.Cancel;
        }
        #endregion

        #region Mehod  

        /// <summary>
        /// Box List
        /// </summary>
        private void GetBoxInfo()
        {
            try
            {
                string sPalletId = txtPalletID.Text;

                DataTable RQSTDT = new DataTable("INDATA");
                RQSTDT.Columns.Add("LANGID", typeof(String));
                RQSTDT.Columns.Add("BOXID", typeof(String));
                RQSTDT.Columns.Add("MKT_TYPE_CODE", typeof(String));
                RQSTDT.Columns.Add("SEQ", typeof(String));
                RQSTDT.Columns.Add("USERID", typeof(String));
                RQSTDT.Columns.Add("DATE", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["BOXID"] = sPalletId;
                dr["MKT_TYPE_CODE"] = Util.NVC(cboOrder.SelectedValue);
                dr["SEQ"] = txtOrder.Text.ToString();
                dr["USERID"] = LoginInfo.USERID;
                dr["DATE"] = dtpDate.SelectedDateTime.ToString("yyyyMMdd");

                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("BR_PRD_GET_OUTBOX_CUST_LABEL_NJ", "INDATA", "OUTDATA", RQSTDT);
                if (!dtResult.Columns.Contains("CHK"))
                    dtResult = _util.gridCheckColumnAdd(dtResult, "CHK");
                Util.GridSetData(dgbox, dtResult, FrameOperation, true);

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

        /// <summary>
        /// Label 출력 
        /// </summary>
        private void PrintProcessManual(C1DataGrid dg)
        {
            try
            {
                // 라벨 발행이력 저장
                string BizRuleName = "DA_PRD_INS_LABEL_HIST";

                // 태그(라벨) 항목
                DataTable dtLabelItem = new DataTable();
                dtLabelItem.Columns.Add("LABEL_CODE", typeof(string));  //"LBL0139"
                dtLabelItem.Columns.Add("ITEM001", typeof(string));     //"Supplier:"
                dtLabelItem.Columns.Add("ITEM002", typeof(string));     //"乐金化学（南京）信息电子材料有限公司"
                dtLabelItem.Columns.Add("ITEM003", typeof(string));     //MONTH
                dtLabelItem.Columns.Add("ITEM004", typeof(string));     //"批次 :"
                dtLabelItem.Columns.Add("ITEM005", typeof(string));     //ORD
                dtLabelItem.Columns.Add("ITEM006", typeof(string));     //"Qty :"
                dtLabelItem.Columns.Add("ITEM007", typeof(string));     //QTY
                dtLabelItem.Columns.Add("ITEM008", typeof(string));     //QTY_BCD
                dtLabelItem.Columns.Add("ITEM009", typeof(string));     //"ROHS"
                dtLabelItem.Columns.Add("ITEM010", typeof(string));     //"Item :"
                dtLabelItem.Columns.Add("ITEM011", typeof(string));     //CUST_PRODID
                dtLabelItem.Columns.Add("ITEM012", typeof(string));     //"SPEC :"
                dtLabelItem.Columns.Add("ITEM013", typeof(string));     //SPEC
                dtLabelItem.Columns.Add("ITEM014", typeof(string));     //PRODID
                dtLabelItem.Columns.Add("ITEM015", typeof(string));     //"等级 :"
                dtLabelItem.Columns.Add("ITEM016", typeof(string));     //PRDT_GRD_CODE
                dtLabelItem.Columns.Add("ITEM017", typeof(string));     //QRCODE            

                // 라벨이력 저장
                DataTable inTable = _bizDataSet.GetBR_PRD_REG_LABEL_HIST();

                foreach (DataGridRow row in dg.Rows)
                {
                    if (row.Type == DataGridRowType.Item &&
                        (DataTableConverter.GetValue(row.DataItem, "CHK").GetString() == "True" ||
                         DataTableConverter.GetValue(row.DataItem, "CHK").GetString() == "1"))
                    {
                        DataRow dr = dtLabelItem.NewRow();

                        dr["LABEL_CODE"] = "LBL0139";
                        dr["ITEM001"] = "Supplier:";
                        dr["ITEM002"] = "乐金化学（南京）信息电子材料有限公司";
                        dr["ITEM003"] = Util.NVC(DataTableConverter.GetValue(row.DataItem, "MON"));
                        dr["ITEM004"] = "批次 :";
                        dr["ITEM005"] = Util.NVC(DataTableConverter.GetValue(row.DataItem, "ORD"));
                        dr["ITEM006"] = "Qty :";
                        dr["ITEM007"] = Util.NVC(DataTableConverter.GetValue(row.DataItem, "QTY"));
                        dr["ITEM008"] = Util.NVC(DataTableConverter.GetValue(row.DataItem, "QTY_BCD"));
                        dr["ITEM009"] = "ROHS";
                        dr["ITEM010"] = "Item :";
                        dr["ITEM011"] = Util.NVC(DataTableConverter.GetValue(row.DataItem, "CUSTPRODID"));
                        dr["ITEM012"] = "SPEC :";
                        dr["ITEM013"] = Util.NVC(DataTableConverter.GetValue(row.DataItem, "SPEC"));
                        dr["ITEM014"] = Util.NVC(DataTableConverter.GetValue(row.DataItem, "PRODID"));
                        dr["ITEM015"] = "等级 :";
                        dr["ITEM016"] = Util.NVC(DataTableConverter.GetValue(row.DataItem, "PRDT_GRD_CODE"));
                        dr["ITEM017"] = Util.NVC(DataTableConverter.GetValue(row.DataItem, "QRCODE"));

                        // 라벨 발행 이력 저장
                        DataRow newRow = inTable.NewRow();
                        newRow["LABEL_PRT_COUNT"] = 1;
                        newRow["LABEL_CODE"] = "LBL0139";
                        //newRow["LABEL_ZPL_CNTT"] = 1;
                        newRow["PRT_ITEM01"] = "Supplier:";
                        newRow["PRT_ITEM02"] = "乐金化学（南京）信息电子材料有限公司";
                        newRow["PRT_ITEM03"] = Util.NVC(DataTableConverter.GetValue(row.DataItem, "MON"));
                        newRow["PRT_ITEM04"] = "批次 :";
                        newRow["PRT_ITEM05"] = Util.NVC(DataTableConverter.GetValue(row.DataItem, "ORD"));
                        newRow["PRT_ITEM06"] = "Qty :";
                        newRow["PRT_ITEM07"] = Util.NVC(DataTableConverter.GetValue(row.DataItem, "QTY"));
                        newRow["PRT_ITEM08"] = Util.NVC(DataTableConverter.GetValue(row.DataItem, "QTY_BCD"));
                        newRow["PRT_ITEM09"] = "ROHS";
                        newRow["PRT_ITEM10"] = "Item :";
                        newRow["PRT_ITEM11"] = Util.NVC(DataTableConverter.GetValue(row.DataItem, "CUSTPRODID"));
                        newRow["PRT_ITEM12"] = "SPEC :";
                        newRow["PRT_ITEM13"] = Util.NVC(DataTableConverter.GetValue(row.DataItem, "SPEC"));
                        newRow["PRT_ITEM14"] = Util.NVC(DataTableConverter.GetValue(row.DataItem, "PRODID"));
                        newRow["PRT_ITEM15"] = "等级 :";
                        newRow["PRT_ITEM16"] = Util.NVC(DataTableConverter.GetValue(row.DataItem, "PRDT_GRD_CODE"));
                        newRow["PRT_ITEM17"] = Util.NVC(DataTableConverter.GetValue(row.DataItem, "QRCODE"));
                        newRow["INSUSER"] = LoginInfo.USERID;
                        newRow["LOTID"] = Util.NVC(DataTableConverter.GetValue(row.DataItem, "BOXID"));
                        newRow["PGM_ID"] = _sPGM_ID;
                        newRow["BZRULE_ID"] = BizRuleName;
                        inTable.Rows.Add(newRow);

                        dtLabelItem.Rows.Add(dr);
                    }
                }


                string printType;
                string resolution;
                string issueCount;
                string xposition;
                string yposition;
                string darkness;
                DataRow drPrintInfo;

                if (!_util.GetConfigPrintInfo(out printType, out resolution, out issueCount, out xposition, out yposition, out darkness, out drPrintInfo))
                    return;

                bool isLabelPrintResult = Util.PrintLabelPacking(FrameOperation, loadingIndicator, dtLabelItem, printType, resolution, issueCount, xposition, yposition, darkness, drPrintInfo);
                if (!isLabelPrintResult)
                {
                    //라벨 발행중 문제가 발생하였습니다.
                    Util.MessageValidation("SFU3243");
                }
                else
                {
                    // 라벨 발행이력 저장
                    //string BizRuleName = "DA_PRD_INS_LABEL_HIST";

                    new ClientProxy().ExecuteService(BizRuleName, "INDATA", null, inTable, (result, searchException) =>
                    {
                        try
                        {
                            if (searchException != null)
                            {
                                Util.MessageException(searchException);
                                return;
                            }
                        }
                        catch (Exception ex)
                        {
                            Util.MessageException(ex);
                        }
                        finally
                        {

                        }
                    });
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }


        private bool PrintLabel(string zpl, DataRow drPrtInfo)
        {
            if (drPrtInfo?.Table == null)
            {
                loadingIndicator.Visibility = Visibility.Collapsed;
                FrameOperation.PrintFrameMessage(MessageDic.Instance.GetMessage("SFU3030"));

                return false;
            }

            bool brtndefault = false;
            if (drPrtInfo.Table.Columns.Contains("PORTNAME") && drPrtInfo["PORTNAME"].ToString().Trim().Length > 0)
            {
                if (drPrtInfo["PORTNAME"].GetString().ToUpper().Equals("USB"))
                {
                    brtndefault = FrameOperation.Barcode_ZPL_USB_Print(zpl);
                    if (brtndefault == false)
                    {
                        loadingIndicator.Visibility = Visibility.Collapsed;
                        FrameOperation.PrintFrameMessage(MessageDic.Instance.GetMessage("SFU1309"));
                    }
                }
                else if (drPrtInfo["PORTNAME"].ToString().IndexOf("LPT", StringComparison.Ordinal) >= 0)
                {
                    FrameOperation.PrintFrameMessage(string.Empty);
                    brtndefault = FrameOperation.Barcode_ZPL_LPT_Print(drPrtInfo, zpl);
                    if (brtndefault == false)
                    {
                        loadingIndicator.Visibility = Visibility.Collapsed;
                        FrameOperation.PrintFrameMessage(MessageDic.Instance.GetMessage("SFU1309"));
                    }
                }
                else
                {
                    FrameOperation.PrintFrameMessage(string.Empty);
                    brtndefault = FrameOperation.Barcode_ZPL_Print(drPrtInfo, zpl);
                    if (brtndefault == false)
                    {
                        loadingIndicator.Visibility = Visibility.Collapsed;
                        FrameOperation.PrintFrameMessage(MessageDic.Instance.GetMessage("SFU1309"));
                    }
                }

                //System.Threading.Thread.Sleep(200);
            }
            else
            {
                loadingIndicator.Visibility = Visibility.Collapsed;
                FrameOperation.PrintFrameMessage(MessageDic.Instance.GetMessage("SFU3031"));
                Util.MessageValidation("SFU3031");
            }

            return brtndefault;
        }

        #endregion

        #region [Func]
        private bool ValidationPrint()
         {
            int idx = _util.GetDataGridRowCountByCheck(dgbox, "CHK" , true); 

            if (idx == 0)
            {
                //선택된 박스가 없습니다.
                Util.MessageInfo("SFU2058");
                return false;
            }

            if (Util.NVC(cboOrder.SelectedValue) == "SELECT" || String.IsNullOrEmpty(Util.NVC(cboOrder.SelectedValue)))
            {
                //수출/내수를 선택해주세요.
                Util.MessageInfo("SFU3606");
                return false;
            }

            if (int.Parse(txtOrder.Text) > 50)
            {
                //입력값이 최대값을 초과 하였습니다
                Util.MessageInfo("SFU1145");
                return false;
            }

            if (int.Parse(txtOrder.Text) < 01)
            {
                //입력값이 최소값에 미달되었습니다.
                Util.MessageInfo("SFU1146");
                return false;
            }

            if (String.IsNullOrWhiteSpace(txtOrder.Text))
            {
                //입력 데이터가 존재하지 않습니다.
                Util.MessageInfo("SFU1801");
                return false;
            }

            return true;
        }

        private bool ValidationSearch()
        {

            if (Util.NVC(cboOrder.SelectedValue) == "SELECT" || String.IsNullOrEmpty(Util.NVC(cboOrder.SelectedValue)))
            {
                //수출/내수를 선택해주세요.
                Util.MessageInfo("SFU3606");
                return false;
            }

            if (String.IsNullOrEmpty(txtOrder.Text))
            {
                //입력 데이터가 존재하지 않습니다.
                Util.MessageInfo("SFU1801");
                return false;
            }

            if (int.Parse(txtOrder.Text) > 50)
            {
                //입력값이 최대값을 초과 하였습니다
                Util.MessageInfo("SFU1145");
                return false;
            }

            if (int.Parse(txtOrder.Text) < 01)
            {
                //입력값이 최소값에 미달되었습니다.
                Util.MessageInfo("SFU1146");
                return false;
            }

            return true;
        }

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

        #region [버튼 클릭]
        private void btnPrint_Click(object sender, RoutedEventArgs e)
        {
           
            if (!ValidationPrint())
                return;

            // 발행 하시겠습니까?
            Util.MessageConfirm("SFU2873", (result) =>
            {
                if (result == MessageBoxResult.OK)
                {
                    PrintProcessManual(dgbox);

                }
            });
        }

        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidationSearch())
                return;

            GetBoxInfo();
        }

        void checkAll_Checked(object sender, RoutedEventArgs e)
        {
            checkAllProcess();
        }

        void checkAll_Unchecked(object sender, RoutedEventArgs e)
        {
            uncheckProcess();
        }

        private void checkAllProcess()
        {
            if (dgbox == null)
            {
                return;
            }

            DataTable dt = DataTableConverter.Convert(dgbox.ItemsSource);

            for (int idx = 0; idx < dt.Rows.Count; idx++)
            {
                dt.Rows[idx]["CHK"] = true;
                // C1.WPF.DataGrid.DataGridRow row = dgLotInfo.Rows[idx];
                // DataTableConverter.SetValue(row.DataItem, "CHK", true);
            }

            dgbox.ItemsSource = DataTableConverter.Convert(dt);
        }

        private void uncheckProcess()
        {
            if (dgbox == null)
            {
                return;
            }

            DataTable dt = DataTableConverter.Convert(dgbox.ItemsSource);
            for (int idx = 0; idx < dt.Rows.Count; idx++)
            {
                dt.Rows[idx]["CHK"] = false;
            }
            dgbox.ItemsSource = DataTableConverter.Convert(dt);
        }

        private void dgbox_LoadedColumnHeaderPresenter(object sender, C1.WPF.DataGrid.DataGridColumnEventArgs e)
        {
            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (string.IsNullOrEmpty(e.Column.Name) == false)
                {
                    if (e.Column.Name.Equals("CHK"))
                    {
                        pre.Content = chkAll;
                        e.Column.HeaderPresenter.Content = pre;
                        chkAll.Checked -= new RoutedEventHandler(checkAll_Checked);
                        chkAll.Unchecked -= new RoutedEventHandler(checkAll_Unchecked);
                        chkAll.Checked += new RoutedEventHandler(checkAll_Checked);
                        chkAll.Unchecked += new RoutedEventHandler(checkAll_Unchecked);
                    }
                }
            }));
        }
    }
}
#endregion

