/*************************************************************************************
 Created Date : 2020.03.13
      Creator : 
   Decription : 재공현황 조회(CWA 전극 2동 전용)
--------------------------------------------------------------------------------------
 [Change History]
  2020.03.13  DEVELOPER : 김태균.
  2020.04.20  김태균 : QA Sampling 컬럼 추가

**************************************************************************************/
#define SAMPLE_DEV
//#undef SAMPLE_DEV

using C1.WPF;
using C1.WPF.DataGrid;
using C1.WPF.DataGrid.Summaries;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using System;
using System.Collections.Generic;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using LGC.GMES.MES.CMM001;

namespace LGC.GMES.MES.FCS001
{
    public partial class FCS001_RPM_110 : UserControl, IWorkArea
    {
        #region Declaration & Constructor 

        Util _Util = new Util();

        public FCS001_RPM_110()
        {
            InitializeComponent();
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

        //화면내 combo 셋팅
        private void InitCombo()
        {

            //CommonCombo _combo = new CommonCombo();
            //String[] sFilter = { LoginInfo.CFG_AREA_ID };
            ////동
            //C1ComboBox[] cboAreaChild = { cboEquipmentSegment };
            //_combo.SetCombo(cboArea, CommonCombo.ComboStatus.SELECT, cbChild: cboAreaChild);

            //cboArea.SelectedValue = LoginInfo.CFG_AREA_ID;
            //cboArea.IsEnabled = false;

            ////라인
            ////C1ComboBox[] cboLineChild = { cboElecType };
            //C1ComboBox[] cboLineParent = { cboArea };
            //_combo.SetCombo(cboEquipmentSegment, CommonCombo.ComboStatus.ALL, cbParent: cboLineParent);

            //cboEquipmentSegment.SelectedValue = LoginInfo.CFG_EQSG_ID;
            //cboEquipmentSegment.IsEnabled = false;

            ////공정
            //C1ComboBox[] cboProcessParent = { cboArea, cboEquipmentSegment };
            //_combo.SetCombo(cboProcess, CommonCombo.ComboStatus.NONE, cbParent: cboProcessParent, sCase: "PROCESSWH");

            //String[] sFilter1 = { "ELTR_TYPE_CODE" };
            //_combo.SetCombo(cboElecType, CommonCombo.ComboStatus.ALL, sFilter: sFilter1, sCase: "COMMCODE");

            ////if (cboEquipmentSegment.Items.Count > 0) cboEquipmentSegment.SelectedIndex = 0;

            ////생산구분
            //string[] sFilter2 = { "PRODUCT_DIVISION" };
            //_combo.SetCombo(cboProductDiv, CommonCombo.ComboStatus.ALL, sCase: "COMMCODE", sFilter: sFilter2);

            //// 생산구분 Default 정상생산
            //if (cboProductDiv.Items.Count > 1)
            //    cboProductDiv.SelectedIndex = 1;

        }

        #endregion

        //        #region Event

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                InitCombo();
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            //GetStock();
        }

        //        private void dgLotInfo_LoadedCellPresenter(object sender, DataGridCellEventArgs e)
        //        {
        //            dgLotInfo.Dispatcher.BeginInvoke(new Action(() =>
        //            {
        //                if (e.Cell.Presenter == null)
        //                {
        //                    return;
        //                }

        //                //BACKGROUND COLOR
        //                if ((e.Cell.Column.Name.Equals("LOTID")) || (e.Cell.Column.Name.Equals("WIPHOLD")) || (e.Cell.Column.Name.Equals("ST_HOLD_NOTE"))
        //                 || (e.Cell.Column.Name.Equals("ST_QMS")) || (e.Cell.Column.Name.Equals("ST_INSP_DFCT_CODE_CNTT")) || (e.Cell.Column.Name.Equals("ST_NCR"))
        //                 || (e.Cell.Column.Name.Equals("ST_NCR_INSP_DFCT_CODE_CNTT")) || (e.Cell.Column.Name.Equals("ST_QA_SAMPLING")) || (e.Cell.Column.Name.Equals("ST_QA_SAMPLING_DATE")))
        //                {
        //                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Ivory);
        //                }
        //                else if ((e.Cell.Column.Name.Equals("CT_LOTID")) || (e.Cell.Column.Name.Equals("CT_WIPHOLD")) || (e.Cell.Column.Name.Equals("CT_HOLD_NOTE"))
        //                 || (e.Cell.Column.Name.Equals("CT_QMS")) || (e.Cell.Column.Name.Equals("CT_INSP_DFCT_CODE_CNTT")) || (e.Cell.Column.Name.Equals("CT_NCR"))
        //                 || (e.Cell.Column.Name.Equals("CT_NCR_INSP_DFCT_CODE_CNTT")) || (e.Cell.Column.Name.Equals("CT_QA_SAMPLING")) || (e.Cell.Column.Name.Equals("CT_QA_SAMPLING_DATE")))
        //                {
        //                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Beige);
        //                }
        //                else if ((e.Cell.Column.Name.Equals("RP_LOTID")) || (e.Cell.Column.Name.Equals("RP_WIPHOLD")) || (e.Cell.Column.Name.Equals("RP_HOLD_NOTE"))
        //                 || (e.Cell.Column.Name.Equals("RP_QMS")) || (e.Cell.Column.Name.Equals("RP_INSP_DFCT_CODE_CNTT")) || (e.Cell.Column.Name.Equals("RP_NCR"))
        //                 || (e.Cell.Column.Name.Equals("RP_NCR_INSP_DFCT_CODE_CNTT")) || (e.Cell.Column.Name.Equals("RP_QA_SAMPLING")) || (e.Cell.Column.Name.Equals("RP_QA_SAMPLING_DATE")))
        //                {
        //                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Bisque);
        //                }
        //                else
        //                {
        //                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.White);
        //                }

        //                //HOLD
        //                if (((e.Cell.Column.Name.Equals("WIPHOLD")) && Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "WIPHOLD")).Equals("Y"))
        //                 || ((e.Cell.Column.Name.Equals("ST_QMS")) && Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "ST_QMS")).Equals("NG"))
        //                 || ((e.Cell.Column.Name.Equals("ST_NCR")) && Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "ST_NCR")).Equals("NG"))
        //                 || ((e.Cell.Column.Name.Equals("CT_WIPHOLD")) && Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "CT_WIPHOLD")).Equals("Y"))
        //                 || ((e.Cell.Column.Name.Equals("CT_QMS")) && Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "CT_QMS")).Equals("NG"))
        //                 || ((e.Cell.Column.Name.Equals("CT_NCR")) && Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "CT_NCR")).Equals("NG"))
        //                 || ((e.Cell.Column.Name.Equals("RP_WIPHOLD")) && Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "RP_WIPHOLD")).Equals("Y"))
        //                 || ((e.Cell.Column.Name.Equals("RP_QMS")) && Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "RP_QMS")).Equals("NG"))
        //                 || ((e.Cell.Column.Name.Equals("RP_NCR")) && Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "RP_NCR")).Equals("NG"))
        //                 || ((e.Cell.Column.Name.Equals("QMSHOLD")) && Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "QMSHOLD")).Equals("Y"))
        //                )
        //                {
        //                    e.Cell.Presenter.FontWeight = FontWeights.Bold;
        //                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Red);
        //                }
        //                else
        //                {
        //                    e.Cell.Presenter.FontWeight = FontWeights.Normal;
        //                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Black);
        //                }


        //            }));
        //        }

        //        private void cboProcess_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        //        {
        //            Util.gridClear(dgLotInfo);

        //            for (int i = 0; i < 18; i++)
        //            {
        //                dgLotInfo.Columns[i].Visibility = Visibility.Visible;
        //            }

        //            if (cboProcess.SelectedValue.ToString() == "E3000")
        //            {
        //                for (int i = 0; i < 18; i++)
        //                {
        //                    dgLotInfo.Columns[i].Visibility = Visibility.Collapsed;
        //                }
        //            }
        //            else if (cboProcess.SelectedValue.ToString() == "E4000")
        //            {
        //                for (int i = 0; i < 9; i++)
        //                {
        //                    dgLotInfo.Columns[i].Visibility = Visibility.Collapsed;
        //                }
        //            }

        //        }
        //        #endregion

        //        #region Method

        //        private void GetStock()
        //        {

        //            try
        //            {
        //                Util.gridClear(dgLotInfo);

        //                DataTable IndataTable = new DataTable();
        //                IndataTable.Columns.Add("LANGID", typeof(string));
        //                IndataTable.Columns.Add("EQSGID", typeof(string));
        //                IndataTable.Columns.Add("PROCID", typeof(string));
        //                IndataTable.Columns.Add("PRDT_CLSS_CODE", typeof(string));
        //                IndataTable.Columns.Add("LOTTYPE", typeof(string));
        //                IndataTable.Columns.Add("PRJT_NAME", typeof(string));
        //                IndataTable.Columns.Add("MODLID", typeof(string));
        //                IndataTable.Columns.Add("PRODID", typeof(string));

        //                DataRow Indata = IndataTable.NewRow();
        //                Indata["LANGID"] = LoginInfo.LANGID;
        //                Indata["EQSGID"] = Util.GetCondition(cboEquipmentSegment);
        //                Indata["PROCID"] = cboProcess.SelectedValue.ToString() == "" ? null : cboProcess.SelectedValue.ToString();
        //                Indata["PRDT_CLSS_CODE"] = cboElecType.SelectedValue.ToString() == "" ? null : cboElecType.SelectedValue.ToString();
        //                Indata["LOTTYPE"] = cboProductDiv.SelectedValue.ToString() == "" ? null : cboProductDiv.SelectedValue.ToString();
        //                Indata["PRJT_NAME"] = Util.GetCondition(txtPrjtName);
        //                Indata["MODLID"] = Util.GetCondition(txtModlId);
        //                Indata["PRODID"] = Util.GetCondition(txtProdId);

        //                IndataTable.Rows.Add(Indata);

        //                ShowLoadingIndicator();
        //                new ClientProxy().ExecuteService("DA_PRD_SEL_STOCK_FOR_ED", "RQSTDT", "RSLTDT", IndataTable, (result, Exception) =>
        //                {
        //                    try
        //                    {
        //                        if (Exception != null)
        //                        {
        //                            LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(Exception), null, "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
        //                            return;
        //                        }

        //                        if (result.Rows.Count > 0)
        //                        {
        //                            //string[] sColumnName = new string[] { "LOTID", "WIPHOLD", "ST_HOLD_NOTE", "ST_QMS", "ST_INSP_DFCT_CODE_CNTT", "ST_NCR", "ST_NCR_INSP_DFCT_CODE_CNTT" };
        //                            //_Util.SetDataGridMergeExtensionCol(dgLotInfo, sColumnName, DataGridMergeMode.VERTICAL);

        //                            if (cboProcess.SelectedValue.ToString() == "E3000")
        //                            {
        //                                string[] sColumnName = new string[] { "CT_LOTID" };
        //                                _Util.SetDataGridMergeExtensionCol(dgLotInfo, sColumnName, DataGridMergeMode.VERTICAL);
        //                            }
        //                            else if (cboProcess.SelectedValue.ToString() == "E4000")
        //                            {
        //                                string[] sColumnName = new string[] { "RP_LOTID" };
        //                                _Util.SetDataGridMergeExtensionCol(dgLotInfo, sColumnName, DataGridMergeMode.VERTICAL);
        //                            }
        //                            else if (cboProcess.SelectedValue.ToString() == "E7000")
        //                            {
        //                                string[] sColumnName = new string[] { "LOTID" };
        //                                _Util.SetDataGridMergeExtensionCol(dgLotInfo, sColumnName, DataGridMergeMode.VERTICAL);
        //                            }

        //                            Util.GridSetData(dgLotInfo, result, FrameOperation, true);
        //                        }


        //                    }
        //                    catch (Exception ex)
        //                    {
        //                        Util.MessageException(ex);
        //                    }
        //                    finally
        //                    {
        //                        HiddenLoadingIndicator();
        //                    }
        //                });


        //            }
        //            catch (Exception e)
        //            {
        //                Util.MessageException(e);
        //            }
        //        }

        //#endregion


        //        private void ShowLoadingIndicator()
        //        {
        //            if (loadingIndicator != null)
        //                loadingIndicator.Visibility = Visibility.Visible;
        //        }
        //        private void HiddenLoadingIndicator()
        //        {
        //            if (loadingIndicator != null)
        //                loadingIndicator.Visibility = Visibility.Collapsed;
        //        }


    }
}
