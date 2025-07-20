/*************************************************************************************
 Created Date : 2017.11.04
      Creator : INS 오화백K
   Decription : 활성화 후공정 - Pallet 재구성 - Pallet 재선별
--------------------------------------------------------------------------------------
 [Change History]
  2017.11.04  INS 오화백K : Initial Created.
  
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
namespace LGC.GMES.MES.BOX001
{
    /// <summary>
    /// CMM_ASSY_CANCEL_TERM.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class BOX001_217_SELECT_TAG : C1Window, IWorkArea
    {
        #region Declaration & Constructor         
        private BizDataSet _Biz = new BizDataSet();
        private Util _Util = new Util();
        private string _REQID = string.Empty;
        private string _INSP_QTY = string.Empty;
        private string _CHECK = string.Empty;
        private int _tagPrintCount;
        private string _LOTID_RT = string.Empty;
        private string _PROCID = string.Empty;
        private string _ctnrID = string.Empty;
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
        public BOX001_217_SELECT_TAG()
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

                DataTable PalletID = tmps[0] as DataTable;

                if (PalletID == null)
                    return;

                GetSplitLotInfo(PalletID);

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
                Util.MessageConfirm("SFU4272", (result) =>// 발행하시겠습니까?
                {
                    if (result == MessageBoxResult.OK)
                    {

                        TagPrint(_ctnrID);
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
                dtRqst.Columns.Add("EQSGID", typeof(string));
                dtRqst.Columns.Add("PROCID", typeof(string));
                dtRqst.Columns.Add("PJT_NAME", typeof(string));
                dtRqst.Columns.Add("PRODID", typeof(string));
                dtRqst.Columns.Add("LOTID_RT", typeof(string));
                dtRqst.Columns.Add("LOTID", typeof(string));
                dtRqst.Columns.Add("LANGID", typeof(string));
                //dtRqst.Columns.Add("CTNR_ID", typeof(string));
                DataRow dr = null;
                for (int i = 0; i < Result.Rows.Count; i++)
                {
                    dr = dtRqst.NewRow();
                    // dr["CTNR_ID"] = Result.Rows[i]["CTNR_ID"].ToString();
                    dr["LOTID"] = Result.Rows[i]["SPLIT_LOTID"].ToString();
                    dr["LANGID"] = LoginInfo.LANGID;
                    dtRqst.Rows.Add(dr);
                }
                //DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_PALLTE_MERGER_SPLIT_QTY_TAG", "INDATA", "OUTDATA", dtRqst);
                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_INBOX_MERGE_SPLIT_QTY_TAG_NJ", "INDATA", "OUTDATA", dtRqst);

                _LOTID_RT = dtRslt.Rows[0]["LOTID_RT"].ToString();
                _PROCID = dtRslt.Rows[0]["PROCID"].ToString();
                Util.GridSetData(dgLotInfo, dtRslt, FrameOperation, true);


            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }

        }

        private void TagPrint(string ctnrID)
        {
            //CMM_FORM_TAG_PRINT popupTagPrint = new CMM_FORM_TAG_PRINT();
            //popupTagPrint.FrameOperation = this.FrameOperation;
            ////popupTagPrint.QMSRequestPalletYN = "Y";
            //popupTagPrint.PrintCount = _tagPrintCount.ToString();

            //_tagPrintCount--;

            //object[] parameters = new object[8];
            //parameters[0] = dr["PROCID"];
            //parameters[1] = null;              // 설비ID
            //parameters[2] = dr["PALLETID"];
            //parameters[3] = dr["WIPSEQ"].ToString();
            //parameters[4] = dr["WIPQTY"].ToString().Replace(",", "");
            //parameters[5] = "N";                                         // 디스패치 처리
            //parameters[6] = "Y";                                         // 출력 여부
            //parameters[7] = "Y";     // Direct 출력 여부

            //C1WindowExtension.SetParameters(popupTagPrint, parameters);

            //foreach (Grid tmp in Util.FindVisualChildren<Grid>(Application.Current.MainWindow))
            //{
            //    if (tmp.Name == "grdMain")
            //    {
            //        tmp.Children.Add(popupTagPrint);
            //        popupTagPrint.BringToFront();
            //        break;
            //    }
            //}
            CMM_POLYMER_FORM_TAG_PRINT popupCartPrint = new CMM_POLYMER_FORM_TAG_PRINT();
            popupCartPrint.FrameOperation = this.FrameOperation;

            popupCartPrint.PrintCount = "1";


            object[] parameters = new object[5];
            parameters[0] = Process.CELL_BOXING;
            parameters[1] = string.Empty;
            parameters[2] = ctnrID;   // ButtonCertSelect.Tag;
            parameters[3] = "N";      // Direct 출력 여부
            parameters[4] = "N";      // 임시 대차 출력 여부

            C1WindowExtension.SetParameters(popupCartPrint, parameters);

            //popupCartPrint.Closed += new EventHandler(popupCartPrint_Closed);

            foreach (Grid tmp in Util.FindVisualChildren<Grid>(Application.Current.MainWindow))
            {
                if (tmp.Name == "grdMain")
                {
                    tmp.Children.Add(popupCartPrint);
                    popupCartPrint.BringToFront();
                    break;
                }
            }
        }

        //private void TagPrint(string pallet, int pageCnt)
        //{
        //    CMM_FORM_TAG_PRINT popupTagPrint = new CMM_FORM_TAG_PRINT();
        //    popupTagPrint.FrameOperation = this.FrameOperation;

        //    popupTagPrint.PrintCount = pageCnt.ToString();

        //    object[] parameters = new object[8];
        //    parameters[0] = _PROCID;
        //    parameters[1] = null;
        //    parameters[2] = pallet;
        //    parameters[3] = null;
        //    parameters[4] = null;
        //    //parameters[5] = DataTableConverter.GetValue(DgProductionPallet.Rows[iRow].DataItem, "DISPATCH_YN").GetString();
        //    parameters[5] = "N";      // 디스패치 처리
        //    parameters[6] = "Y";      // 재발행 여부
        //    parameters[7] = "N";     // Direct 출력 여부
        //    C1WindowExtension.SetParameters(popupTagPrint, parameters);

        //    popupTagPrint.Closed += new EventHandler(popupTagPrint_Closed);

        //    foreach (Grid tmp in Util.FindVisualChildren<Grid>(Application.Current.MainWindow))
        //    {
        //        if (tmp.Name == "grdMain")
        //        {
        //            tmp.Children.Add(popupTagPrint);
        //            popupTagPrint.BringToFront();
        //            break;
        //        }
        //    }
        //}


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

        //private void popupTagPrint_Closed(object sender, EventArgs e)
        //{

        //    CMM_FORM_TAG_PRINT popup = sender as CMM_FORM_TAG_PRINT;
        //    if (popup != null && popup.DialogResult == MessageBoxResult.OK)
        //    {
        //    }

        //    foreach (Grid tmp in Util.FindVisualChildren<Grid>(Application.Current.MainWindow))
        //    {
        //        if (tmp.Name == "grdMain")
        //        {
        //            tmp.Children.Remove(popup);
        //            break;
        //        }
        //    }

        //}
        #endregion

        #region [Validation]
        private bool Validation()
        {
            for (int i = 0; i < dgLotInfo.Rows.Count; i++)
            {
                if (dgLotInfo.GetCell(i, dgLotInfo.Columns["CTNR_ID"].Index).Value == null)
                {
                    Util.MessageValidation("SFU4365");//대차 정보가 없습니다.
                    return false;
                }
                else
                {
                    _ctnrID = dgLotInfo.GetCell(i, dgLotInfo.Columns["CTNR_ID"].Index).Value.ToString();
                }
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
