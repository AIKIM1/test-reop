/*************************************************************************************
 Created Date : 2017.01.24
      Creator : INS 김동일K
   Decription : 전지 5MEGA-GMES 구축 - 조립 공정진척 화면 - LOT 종료취소 팝업
--------------------------------------------------------------------------------------
 [Change History]
  2017.01.24  INS 김동일K : Initial Created.
  2023.05.10  김린겸   C20220905-000462 Returned pallet generate logic modify
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
    public partial class COM001_094_SPLIT : C1Window, IWorkArea
    {
        #region Declaration & Constructor         
        private BizDataSet _Biz = new BizDataSet();
        private Util _Util = new Util();
        private string _PROCID = string.Empty;
        private int _tagPrintCount;
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
        public COM001_094_SPLIT()
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

                if (tmps == null)
                {
                    return;
                }

                DataTable PalletID = tmps[0] as DataTable;

                if (PalletID == null)
                    return;

                if (tmps.Length == 2)
                {
                    if (tmps[1] == null)
                    {
                        return;
                    }

                    if (tmps[1].ToString().Equals("ReworkSplit"))
                    {
                        GetReworkSplitLotInfo(PalletID);
                    }
                }
                else if (tmps.Length >= 1)
                {
                    GetSplitLotInfo(PalletID);
                }

                this.Loaded -= C1Window_Loaded;
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
                DataRow dr = null;
                for (int i=0; i< Result.Rows.Count; i++)
                {
                    dr = dtRqst.NewRow();
                    dr["LOTID"] = Result.Rows[i]["SPLIT_LOTID"].ToString();
                    dr["LANGID"] = LoginInfo.LANGID;
                    dtRqst.Rows.Add(dr);
                }
               DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_PALLTE_MERGER_SPLIT_QTY_TAG", "INDATA", "OUTDATA", dtRqst);
               Util.GridSetData(dgLotInfo, dtRslt, FrameOperation, true);
             
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }

        private void GetReworkSplitLotInfo(DataTable Result)
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
                DataRow dr = null;
                for (int i = 0; i < Result.Rows.Count; i++)
                {
                    dr = dtRqst.NewRow();
                    dr["LOTID"] = Result.Rows[i]["SPLIT_LOTID"].ToString();
                    dr["LANGID"] = LoginInfo.LANGID;
                    dtRqst.Rows.Add(dr);
                }
                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_PALLTE_MERGER_REWORKSPLIT_QTY_TAG", "INDATA", "OUTDATA", dtRqst);
                Util.GridSetData(dgLotInfo, dtRslt, FrameOperation, true);

            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }
        private void Tag(DataRow dr)
        {
            try
            {
                // SET PARAMETER
                object[] parameters = new object[8];
                CMM_FORM_TAG_PRINT popupTagPrint   = new CMM_FORM_TAG_PRINT();
                popupTagPrint.FrameOperation = this.FrameOperation;
                // 불량여부
                if (dr["WIP_QLTY_TYPE_CODE"].ToString() == "G")
                {
                    //남경일경우
                    if (LoginInfo.CFG_SHOP_ID == "G182" || LoginInfo.CFG_AREA_ID.Equals("S5"))
                    {
                        //특성 Grader
                        if (dr["PROCID"].Equals(Process.CircularCharacteristicGrader))
                        {
                            popupTagPrint.QMSRequestPalletYN = "Y";
                        }
                        else
                        {
                            // 초소형 일경우 오창이랑 동일함
                           if(dr["S04"].ToString() == "MCS")
                           {
                               popupTagPrint.QMSRequestPalletYN = "Y";
                           }
                           else
                           {
                               popupTagPrint.returnPalletYN = "Y";
                           }
                        }
                          
                    }
                   else // 오창
                    {
                        popupTagPrint.QMSRequestPalletYN = "Y";
                    }
                }
                else
                {
                    popupTagPrint.DefectPalletYN = "Y";
                }
                parameters[0] = dr["PROCID"];  
                parameters[1] = null; 
                parameters[2] = dr["PALLETID"]; 
                parameters[3] = dr["WIPSEQ"].ToString(); 
                parameters[4] = dr["WIPQTY"].ToString(); 
                parameters[5] = "N";      // 디스패치 처리
                parameters[6] = dr["PROC_LABEL_PRT_FLAG"];
                parameters[7] = "N";      // Direct 출력 여부
                C1WindowExtension.SetParameters(popupTagPrint, parameters);
                popupTagPrint.Closed += new EventHandler(popupTagPrint_Closed);
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
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
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


        #endregion

        #region [Validation]


        #endregion

        #region [Func]

        #endregion

        #endregion

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

                        DataRow[] drSelect = DataTableConverter.Convert(dgLotInfo.ItemsSource).Select("CHK = 1");

                        if (drSelect.Length == 0)
                        {
                            Util.MessageValidation("SFU1651");
                            return;
                        }

                    
                        _tagPrintCount = drSelect.Length;

                        foreach (DataRow drPrint in drSelect)
                        {
                            Tag(drPrint);

                        }

                        //}
                    }
                });
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
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
            if (ChkCount == 0)
            {
                Util.MessageValidation("SFU3538");
                return false;
            }
            return true;
        }
    }
}
