/*************************************************************************************
 Created Date : 2017.12.14
      Creator : INS 오화백K
   Decription : 활성화 후공정 - 대차 재구성 - Tag 발행
--------------------------------------------------------------------------------------
 [Change History]
  2017.12.14  INS 오화백K : Initial Created.
  
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
using LGC.GMES.MES.CMM001;
using LGC.GMES.MES.CMM001.Extensions;
namespace LGC.GMES.MES.COM001
{
    /// <summary>
    /// CMM_ASSY_CANCEL_TERM.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class COM001_203_TAG : C1Window, IWorkArea
    {
        #region Declaration & Constructor         
        private BizDataSet _Biz = new BizDataSet();
        private string _REQID = string.Empty;
        private string _INSP_QTY = string.Empty;
        private string _CHECK = string.Empty;
        private int _tagPrintCount;
        private string _LOTID_RT = string.Empty;
        private string _PROCID = string.Empty;
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
        ///  Frame과 상호작용하기 위한 객체
        /// </summary>
        public IFrameOperation FrameOperation
        {
            get;
            set;
        }
        public COM001_203_TAG()
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
                object[] tmps = C1WindowExtension.GetParameters(this);

                DataTable CNTR_ID = tmps[0] as DataTable;

                if (CNTR_ID == null)
                    return;

                GetSplitLotInfo(CNTR_ID);

                this.Loaded -= C1Window_Loaded;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
               
        private void btnPrint_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!Validation())
                {
                    return;
                }
                Util.MessageConfirm("SFU2873", (result) =>// 발행하시겠습니까?
                {
                    if (result == MessageBoxResult.OK)
                    {

                        DataRow[] drSelect = DataTableConverter.Convert(dgLotInfo.ItemsSource).Select("CHK = 1");

                        if (drSelect.Length == 0)
                        {
                            Util.MessageValidation("SFU1651");
                            return;
                        }
                     
                        _tagPrintCount = drSelect.Length;

                        foreach (DataRow drPrint in drSelect)
                        {
                            TagPrint(drPrint);
                        }

                     
                    }
                });
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
        private void GetSplitLotInfo(DataTable Result)
        {
            try
            {
                DataTable dtRqst = new DataTable();
                dtRqst.Columns.Add("CTNR_ID", typeof(string));
             
                DataRow dr = null;
                for (int i = 0; i < Result.Rows.Count; i++)
                {
                    dr = dtRqst.NewRow();
                    dr["CTNR_ID"] = Result.Rows[i]["CTNR_ID"].ToString();
                    dtRqst.Rows.Add(dr);
                }
                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_POLYMER_CART_TAG_LIST", "INDATA", "OUTDATA", dtRqst);
          
                Util.GridSetData(dgLotInfo, dtRslt, FrameOperation, false);
               

            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }

        }
     
        private void TagPrint(DataRow dr)
        {
            CMM_POLYMER_FORM_TAG_PRINT popupTagPrint = new CMM_POLYMER_FORM_TAG_PRINT();
            popupTagPrint.FrameOperation = this.FrameOperation;
            popupTagPrint.CART_MERGE = "Y";
            popupTagPrint.DefectCartYN = Util.NVC(dr["WIP_QLTY_TYPE_CODE"]).Equals("N") ? "Y" : "N";
            popupTagPrint.PrintCount = _tagPrintCount.ToString();

            _tagPrintCount--;

            object[] parameters = new object[5];
            parameters[0] = "";       // _processCode;
            parameters[1] = dr["CURR_EQPTID"];     // Util.NVC(ComboEquipment.SelectedValue);
            parameters[2] = dr["CTNR_ID"];
            parameters[3] = "N";      // Direct 출력 여부
            parameters[4] = "N";      // 임시 대차 출력 여부

            C1WindowExtension.SetParameters(popupTagPrint, parameters);

            foreach (Grid tmp in Util.FindVisualChildren<Grid>(Application.Current.MainWindow))
            {
                if (tmp.Name == "grdMain")
                {
                    tmp.Children.Add(popupTagPrint);
                    popupTagPrint.BringToFront();
                    break;
                }
            }
        }
        private void popupTagPrint_Closed(object sender, EventArgs e)
        {
            CMM_FORM_TAG_PRINT popup = sender as CMM_FORM_TAG_PRINT;
            if (popup != null && popup.DialogResult == MessageBoxResult.OK)
            {
            }
            //this.grdMain.Children.Remove(popup);
            foreach (Grid tmp in Util.FindVisualChildren<Grid>(Application.Current.MainWindow))
            {
                if (tmp.Name == "grdMain")
                {
                    tmp.Children.Remove(popup);
                    break;
                }
            }
        }
        private void chkWip_Click(object sender, RoutedEventArgs e)
        {
            CheckBox cb = sender as CheckBox;

            if (DataTableConverter.GetValue(cb.DataContext, "CHK").Equals(1))//체크되는 경우
            {
                DataRow[] drUnchk = DataTableConverter.Convert(dgLotInfo.ItemsSource).Select("CHK = 0");

                if (drUnchk.Length == 0)
                {
                    chkAll.Checked -= new RoutedEventHandler(checkAll_Checked);
                    chkAll.IsChecked = true;
                    chkAll.Checked += new RoutedEventHandler(checkAll_Checked);
                }

            }
            else//체크 풀릴때
            {
                chkAll.Unchecked -= new RoutedEventHandler(checkAll_Unchecked);
                chkAll.IsChecked = false;
                chkAll.Unchecked += new RoutedEventHandler(checkAll_Unchecked);
            }
        }

        private void checkAll_Checked(object sender, RoutedEventArgs e)
        {
            if (dgLotInfo.ItemsSource == null) return;

            DataTable dt = ((DataView)dgLotInfo.ItemsSource).Table;

            dt.Select("CHK = 0").ToList<DataRow>().ForEach(r => r["CHK"] = 1);
            dt.AcceptChanges();
        }
        private void checkAll_Unchecked(object sender, RoutedEventArgs e)
        {
            if (dgLotInfo.ItemsSource == null) return;

            DataTable dt = ((DataView)dgLotInfo.ItemsSource).Table;

            dt.Select("CHK = 1").ToList<DataRow>().ForEach(r => r["CHK"] = 0);
            dt.AcceptChanges();
        }

        #endregion

        #region [Validation]
        private bool Validation()
        {

            int ChkCount = 0;
            for (int i = 0; i < dgLotInfo.Rows.Count; i++)
            {
                if (Util.NVC(DataTableConverter.GetValue(dgLotInfo.Rows[i].DataItem, "CHK")) == "True" || Util.NVC(DataTableConverter.GetValue(dgLotInfo.Rows[i].DataItem, "CHK")) == "1")
                {
                    ChkCount = ChkCount + 1;
                }
            }
            if(ChkCount == 0)
            {
                Util.MessageValidation("SFU3538");
                return false;
            }
            return true;
        }

        #endregion

        #region [Func]
        private void ApplyPermissions()
        {
            List<Button> listAuth = new List<Button>();
            listAuth.Add(btnPrint);
            
            Util.pageAuth(listAuth, FrameOperation.AUTHORITY);
        }



        #endregion

        #endregion

     
    }
}
