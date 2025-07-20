/*************************************************************************************
 Created Date : 2018.01.12
      Creator : 
   Decription : INBOX 분할
--------------------------------------------------------------------------------------
 [Change History]

**************************************************************************************/

using System;
using System.Windows;
using System.Data;
using System.Windows.Input;

using C1.WPF;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using LGC.GMES.MES.CMM001.Class;
using C1.WPF.DataGrid;
using System.Collections.Generic;
using System.Windows.Controls;
using LGC.GMES.MES.CMM001;
using LGC.GMES.MES.CMM001.Extensions;

using Application = System.Windows.Application;
using DataGridLength = C1.WPF.DataGrid.DataGridLength;
using DataGridRow = C1.WPF.DataGrid.DataGridRow;
using System.Linq;
using System.Windows.Media;

namespace LGC.GMES.MES.COM001
{
    public partial class COM001_215_SHEET_PRINT : C1Window, IWorkArea
    {
        #region Declaration
        Util _Util = new Util();
        BizDataSet _bizRule = new BizDataSet();
     
        private string _CTNR_ID_COMPLATE = string.Empty; //재구성된 INBOX
        private DataTable CHANGE_CTNR = null;


        private int _tagPrintCount;

        private bool _load = true;
        private readonly Util _util = new Util();
        private readonly BizDataSet _bizDataSet = new BizDataSet();
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

        public COM001_215_SHEET_PRINT()
        {
            InitializeComponent();
        }
        #endregion

        #region Event

        #region [Form Load]
        private void C1Window_Loaded(object sender, RoutedEventArgs e)
        {
            if (_load)
            {
                object[] parameters = C1WindowExtension.GetParameters(this);
                _CTNR_ID_COMPLATE = parameters[0] as string;//재구성된 대차ID
                 DataTable CTNR = parameters[1] as DataTable; //자투리 대차ID

                if (CTNR == null)
                    return;
                //테이블의 대차의 중복제거
                DubClear(CTNR);
                //수량 변경된 대차
                GetChange_Ctnr(CHANGE_CTNR);
                //재구성 완료 대차
                GetComplete_Ctnr(_CTNR_ID_COMPLATE);
                _load = false;
            }

        }

        private void InitializeUserControls()
        {
        }
        private void SetControl()
        {
        }

        private void SetControlVisibility()
        {
        }

        #endregion

        #region [닫기]
        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = MessageBoxResult.Cancel;
        }
        #endregion


        #endregion

        #region User Method

        #region [BizCall]
        #endregion
   

        #region [Func]
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
        private void DubClear(DataTable Ctnr)
        {
            //여러개의 같은데이터를 GROUP BY 
            DataRow Linqrow = null;
            CHANGE_CTNR = Ctnr.Clone();
            
            for (int i = 0; i < Ctnr.Rows.Count; i++)
            {
                Linqrow = CHANGE_CTNR.NewRow();
                Linqrow["CTNR_ID"] = Ctnr.Rows[i]["CTNR_ID"];
                CHANGE_CTNR.Rows.Add(Linqrow);
            }

            var summarydata = from SUMrow in CHANGE_CTNR.AsEnumerable()
                              group SUMrow by new
                              {
                                 CTNR_ID = SUMrow.Field<string>("CTNR_ID")

                              } into grp
                              select new
                              {
                                  CTNR_ID = grp.Key.CTNR_ID
                               };
         }

        private void GetChange_Ctnr(DataTable Result)
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
                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_MERGE_CTNR", "INDATA", "OUTDATA", dtRqst);
                Util.GridSetData(dgChangeCtnr, dtRslt, FrameOperation, false);

            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }

        }


        private void GetComplete_Ctnr(string ctnr_id)
        {
            try
            {
                DataTable dtRqst = new DataTable();
                dtRqst.Columns.Add("CTNR_ID", typeof(string));
                DataRow dr = null;
              
                dr = dtRqst.NewRow();
                dr["CTNR_ID"] = ctnr_id;
                dtRqst.Rows.Add(dr);
              
                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_MERGE_CTNR", "INDATA", "OUTDATA", dtRqst);
                Util.GridSetData(dgCompleteCtnr, dtRslt, FrameOperation, false);

            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }

        }

        #endregion

        #endregion

        private void btnPrint_Click(object sender, RoutedEventArgs e)
        {

            try
            {
                string DubCtnr = string.Empty;
                DataTable dtRqst = new DataTable();
                dtRqst.Columns.Add("CTNR_ID", typeof(string));
                dtRqst.Columns.Add("END_YN", typeof(string));
                dtRqst.Columns.Add("WIP_QLTY_TYPE_CODE", typeof(string));
                DataRow dr = null;
                //수량변경된 대차
                for (int i = 0; i < dgChangeCtnr.Rows.Count; i++)
                {
                    dr = dtRqst.NewRow();
                    dr["CTNR_ID"] = Util.NVC(DataTableConverter.GetValue(dgChangeCtnr.Rows[i].DataItem, "CTNR_ID"));
                    dr["END_YN"] = Util.NVC(DataTableConverter.GetValue(dgChangeCtnr.Rows[i].DataItem, "END_YN"));
                    dr["WIP_QLTY_TYPE_CODE"] = Util.NVC(DataTableConverter.GetValue(dgChangeCtnr.Rows[i].DataItem, "WIP_QLTY_TYPE_CODE"));
                    dtRqst.Rows.Add(dr);
                }
                //완료 대차가 수량 변경된 대차에 포함됬는지 체크
                for (int i = 0; i < dtRqst.Rows.Count; i++)
                {
                    if(dtRqst.Rows[i]["CTNR_ID"].ToString() == Util.NVC(DataTableConverter.GetValue(dgCompleteCtnr.Rows[0].DataItem, "CTNR_ID")))
                    {
                        DubCtnr = "Y";
                    }
                }

                if(DubCtnr == string.Empty)
                {
                    dr = dtRqst.NewRow();
                    dr["CTNR_ID"] = Util.NVC(DataTableConverter.GetValue(dgCompleteCtnr.Rows[0].DataItem, "CTNR_ID"));
                    dr["END_YN"] = string.Empty;
                    dr["WIP_QLTY_TYPE_CODE"] = Util.NVC(DataTableConverter.GetValue(dgCompleteCtnr.Rows[0].DataItem, "WIP_QLTY_TYPE_CODE"));
                    dtRqst.Rows.Add(dr);
                }
                DataRow[] drSelect = dtRqst.Select("END_YN <> 'Y'");

                foreach (DataRow drPrint in drSelect)
                {
                  POLYMER_TagPrint(drPrint);
                }

            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }




        }

        private void POLYMER_TagPrint(DataRow dr)
        {
            CMM_POLYMER_FORM_TAG_PRINT popupTagPrint = new CMM_POLYMER_FORM_TAG_PRINT();
            popupTagPrint.FrameOperation = this.FrameOperation;
            popupTagPrint.CART_MERGE = "Y";
            popupTagPrint.DefectCartYN = Util.NVC(dr["WIP_QLTY_TYPE_CODE"]).Equals("N") ? "Y": "N";
            popupTagPrint.PrintCount = _tagPrintCount.ToString();

            _tagPrintCount--;

            object[] parameters = new object[5];
            parameters[0] = "";       // _processCode;
            parameters[1] = null;     // Util.NVC(ComboEquipment.SelectedValue);
            parameters[2] = dr["CTNR_ID"];
            parameters[3] = "N";      // Direct 출력 여부
            parameters[4] = "N";      // 임시 대차 출력 여부

            C1WindowExtension.SetParameters(popupTagPrint, parameters);
            popupTagPrint.Closed += new EventHandler(POLYMER_popupTagPrint_Closed);
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
        private void POLYMER_popupTagPrint_Closed(object sender, EventArgs e)
        {
            CMM_FORM_TAG_PRINT popup = sender as CMM_FORM_TAG_PRINT;
            if (popup != null && popup.DialogResult == MessageBoxResult.OK)
            {
            }
            foreach (Grid tmp in Util.FindVisualChildren<Grid>(Application.Current.MainWindow))
            {
                if (tmp.Name == "grdMain")
                {
                    tmp.Children.Remove(popup);
                    break;
                }
            }

        }
    }
}
