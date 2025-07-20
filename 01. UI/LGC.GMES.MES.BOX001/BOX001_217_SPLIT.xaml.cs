/*************************************************************************************
 Created Date : 2017.01.24
      Creator : INS 김동일K
   Decription : 전지 5MEGA-GMES 구축 - 조립 공정진척 화면 - LOT 종료취소 팝업
--------------------------------------------------------------------------------------
 [Change History]
  2017.01.24  INS 김동일K : Initial Created.
  
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
    public partial class BOX001_217_SPLIT : C1Window, IWorkArea
    {
        #region Declaration & Constructor         
        private BizDataSet _Biz = new BizDataSet();
        private Util _Util = new Util();
        private string _PROCID = string.Empty;
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
        public BOX001_217_SPLIT()
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
               if(dgLotInfo.Rows.Count > 0)
                {
                    
                    //if (LoginInfo.CFG_SHOP_ID.Equals("G182")) //남경
                    //{
                    //    if(dtRslt.Rows[0]["LOTID_RT"].ToString().Length == 8) //원각일경우
                    //    {
                    //        _PROCID = dtRslt.Rows[0]["PROCID"].ToString();
                    //        DataRow[] drSelect = DataTableConverter.Convert(dgLotInfo.ItemsSource).Select();
                    //        int PageCount = 0;
                    //        int RowCount = 0;

                    //        // Page수 산출
                    //        PageCount = drSelect.Length % 18.0 != 0 ? (drSelect.Length / 18) + 1 : drSelect.Length / 18;

                    //        // Pallet List
                    //        string[] PalletList = new string[PageCount];

                    //        // Page 수만큼 Pallet List를 채운다
                    //        for (int cnt = 0; cnt < PageCount; cnt++)
                    //        {
                    //            for (int row = RowCount; row < RowCount + 18; row++)
                    //            {
                    //                if (drSelect.Length <= row)
                    //                    break;
                    //                PalletList[cnt] += drSelect[row]["PALLETID"] + ",";
                    //            }
                    //            RowCount += 18;
                    //        }
                    //        // grdMain.Children.Add라서 처음 호출이 마지막에 뒤에 보인다.  
                    //        for (int print = PalletList.Length - 1; print > -1; print--)
                    //        {
                    //            TagPrint(PalletList[print].Substring(0, PalletList[print].Length - 1), print + 1);
                    //        }
                    //    }
                    //    else
                    //    {
                    //        foreach (DataRow drPrint in DataTableConverter.Convert(dgLotInfo.ItemsSource).Rows)
                    //        {
                    //            Tag(drPrint);
                    //        }
                    //    }
                    //}
                    //else
                    //{
                        //foreach (DataRow drPrint in DataTableConverter.Convert(dgLotInfo.ItemsSource).Rows)
                        //{
                        //    Tag(drPrint);
                        //}
                    //}
              
                }
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }

        }


        //private void TagPrint(string pallet, int pageCnt)
        //{
        //    CMM_FORM_TAG_PRINT popupTagPrint = new CMM_FORM_TAG_PRINT();
        //    popupTagPrint.FrameOperation = this.FrameOperation;

        //    object[] parameters = new object[8];
        //    parameters[0] = _PROCID;
        //    parameters[1] = null;
        //    parameters[2] = pallet;
        //    parameters[3] = null;
        //    parameters[4] = null;
        //    //parameters[5] = DataTableConverter.GetValue(DgProductionPallet.Rows[iRow].DataItem, "DISPATCH_YN").GetString();
        //    parameters[5] = "N";      // 디스패치 처리
        //    parameters[6] = "Y";      // 재발행 여부
        //    parameters[7] = "Y";     // Direct 출력 여부

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


        private void Tag(DataRow dr)
        {
            try
            {
                // SET PARAMETER
                object[] parameters = new object[8];
                CMM_FORM_TAG_PRINT popupTagPrint   = new CMM_FORM_TAG_PRINT();
                popupTagPrint.FrameOperation = this.FrameOperation;
                popupTagPrint.QMSRequestPalletYN = "Y";
                parameters[0] = dr["PROCID"];  
                parameters[1] = null; 
                parameters[2] = dr["PALLETID"]; 
                parameters[3] = dr["WIPSEQ"].ToString(); 
                parameters[4] = dr["WIPQTY"].ToString(); 
                parameters[5] = "N";      // 디스패치 처리
                parameters[6] = dr["PROC_LABEL_PRT_FLAG"];
                parameters[7] = "Y";      // Direct 출력 여부
                C1WindowExtension.SetParameters(popupTagPrint, parameters);
                //grdMain.Children.Add(popupTagPrint);
                //popupTagPrint.BringToFront();
                //this.grdMain.Children.Remove(popupTagPrint);
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



    }
}
