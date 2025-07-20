/*************************************************************************************
 Created Date : 2021.04.20
      Creator : 김길용
   Decription : 팩/모듈 라인 Loader별 Cell 투입 정보
--------------------------------------------------------------------------------------
 [Change History]
   Date         Author      CSR         Description...
  2021.04.20    김길용      SI         Initial Created.    
  2021.05.13    김길용      SI         DA_PRD_LOGIS_SEL_PACK_LOADER_INFO->BR_PRD_LOGIS_SEL_PACK_LOADER_INFO 2021-05-13 김우련C에 의한 수정
**************************************************************************************/
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Data;
using System.Linq;
using System.Threading;
using System.Windows.Media;
using System.Windows.Threading;
using C1.WPF;
using C1.WPF.DataGrid;
using LGC.GMES.MES.CMM001;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.CMM001.Extensions;

namespace LGC.GMES.MES.PACK001
{
    public partial class PACK003_004 : UserControl, IWorkArea
    {
        #region Declaration & Constructor 
        private readonly Util _util = new Util();

        /// <summary>
        /// Frame과 상호작용하기 위한 객체
        /// </summary>
        public IFrameOperation FrameOperation
        {
            get;
            set;
        }

        public PACK003_004()
        {
            InitializeComponent();
        }
        #endregion

        #region Initialize
        private void Initialize()
        {
            InitializeControls();
            InitializeCombo();
        }

        private void InitializeControls()
        {

        }

        #endregion

        #region

        private void InitializeCombo()
        {
            // Area 콤보박스
            CommonCombo _combo = new CommonCombo();
            string[] sFilter1 = { LoginInfo.CFG_AREA_ID ,"Y", null};
            _combo.SetCombo(cboEqsgid, CommonCombo.ComboStatus.ALL, sFilter: sFilter1, sCase: "LOGIS_EQSG_FOR_MEB");
        }
        #endregion

        #region Event
        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            InitializeCombo();
            Loaded -= UserControl_Loaded;
        }

        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Util.gridClear(dgHistory);
                SelectEolHistory();
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        #endregion

        #region Mehod
        private void SelectEolHistory()
        {
            try
            {
                loadingIndicator.Visibility = Visibility.Visible;
                //DA_PRD_LOGIS_SEL_PACK_LOADER_INFO->BR_PRD_LOGIS_SEL_PACK_LOADER_INFO 2021-05-13 김우련C에 의한 수정
                const string bizRuleName = "BR_PRD_LOGIS_SEL_PACK_LOADER_INFO";

                DataTable inDataTable = new DataTable("RQSTDT");
                inDataTable.Columns.Add("LANGID", typeof(string));
                inDataTable.Columns.Add("AREAID", typeof(string));
                inDataTable.Columns.Add("EQSGID", typeof(string));

                DataRow inData = inDataTable.NewRow();
                inData["LANGID"] = LoginInfo.LANGID;
                inData["AREAID"] = LoginInfo.CFG_AREA_ID;
                inData["EQSGID"] = string.IsNullOrEmpty(this.cboEqsgid.SelectedValue.ToString()) ? null : this.cboEqsgid.SelectedValue.ToString();

                inDataTable.Rows.Add(inData);

                new ClientProxy().ExecuteService(bizRuleName, "RQSTDT", "RSLTDT", inDataTable, (result, ex) =>
                {
                    loadingIndicator.Visibility = Visibility.Collapsed;
                    if (ex != null)
                    {
                        Util.MessageException(ex);
                        return;
                    }

                    Util.GridSetData(dgHistory, result, FrameOperation, true);

                    string[] columnName = new string[] { "EQSGNAME" };
                    _util.SetDataGridMergeExtensionCol(dgHistory, columnName, DataGridMergeMode.VERTICAL);
                });
            }
            catch (Exception ex)
            {
                loadingIndicator.Visibility = Visibility.Collapsed;
                Util.MessageException(ex);
            }
        }

        #endregion


        private void dgHistory_MergingCells(object sender, DataGridMergingCellsEventArgs e)
        {
            try
            {
        
                if (dgHistory.ItemsSource == null) return;

                int k = 0;
                if (dgHistory.GetRowCount() > 0)
                {
                    for (int i = 0; i <= dgHistory.GetRowCount() - 1; i++)
                    {

                        for (k = 1; k < dgHistory.GetRowCount() - i; k++)
                        {
                            if ((Util.NVC(DataTableConverter.GetValue(dgHistory.Rows[i].DataItem, "LOADER_EQPT_NAME"))) != (Util.NVC(DataTableConverter.GetValue(dgHistory.Rows[i + k].DataItem, "LOADER_EQPT_NAME"))))
                            {

                                break;
                            }
                        }
                        k--;

                        {
                            e.Merge(new C1.WPF.DataGrid.DataGridCellsRange(dgHistory.GetCell(i, 10), dgHistory.GetCell(i + k, 10)));
                        }

                        i = i + k;

                    }
                }

                C1DataGrid dg = dgHistory; 
                string columnNameBase = "EQSGNAME";
                //int columnIdxBase = dg.Columns[columnNameBase].Index;

                string[] columnName = new string[] { "CMA_PROD", "CELL_PROD" , "PRJT_NAME", "STK_LINE_QTY" };//,"PRJT_NAME","STK_LINE_QTY", "MOVING_QTY"
                int[] columnIdx = new int[columnName.Length];
        
                for (int i = 0; i < columnName.Length; i++)
                {
                    columnIdx[i] = dg.Columns[columnName[i]].Index;
                }
                int j = 0;
                if (dg.GetRowCount() > 0)
                {
                    for (int i = 0; i <= dg.GetRowCount() - 1; i++)
                    {
                        for (j = 1; j < dg.GetRowCount() - i; j++)
                        {   
                            if ((Util.NVC(DataTableConverter.GetValue(dg.Rows[i].DataItem, columnNameBase))) != (Util.NVC(DataTableConverter.GetValue(dg.Rows[i + j].DataItem, columnNameBase))))
                            {

                                break;
                            }
                            
                        }
                        j--;
        
                        for (int x = 0; x < columnName.Length; x++)
                        {
                            int iTemp = columnIdx[x];
                            e.Merge(new C1.WPF.DataGrid.DataGridCellsRange(dg.GetCell(i, iTemp), dg.GetCell(i + j, iTemp)));
                        }
        
                        i = i + j;
                    }
                }
            }
            catch (Exception ex)
            {
                //Util.MessageInfo(ex.Message.ToString());
                Util.MessageInfo(ex.Data["CODE"].ToString());
            }
        }

    }
}
