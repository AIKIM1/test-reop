/*************************************************************************************
 Created Date : 2021.01.13
      Creator : 조영대
   Decription : 공상평 ECM 전지 GMES 구축 DRB - 활성화 : 이상 재공 재고
--------------------------------------------------------------------------------------
 [Change History]
  2021.01.13  조영대 : Initial Created.
  2024.06.10  윤지해 : NERP 대응 프로젝트      마감된 차수도 조회되도록 변경
**************************************************************************************/

using System;
using System.Collections.Generic;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using C1.WPF;
using C1.WPF.DataGrid;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using System.ComponentModel;

namespace LGC.GMES.MES.FCS002
{
    public partial class FCS002_102 : UserControl, IWorkArea
    {
        #region Declaration & Constructor 
        SolidColorBrush captionBrush = new SolidColorBrush(Color.FromArgb(255, 238, 238, 238));
        SolidColorBrush allBrush = new SolidColorBrush(Color.FromArgb(255, 255, 255, 153));
        SolidColorBrush surveyBrush = new SolidColorBrush(Color.FromArgb(255, 225, 255, 225));
        SolidColorBrush realBrush = new SolidColorBrush(Colors.Gray);
        SolidColorBrush infoBrush = new SolidColorBrush(Colors.White);

        Util _Util = new Util();

        public FCS002_102()
        {
            InitializeComponent();
        }

        public IFrameOperation FrameOperation
        {
            get;
            set;
        }

        CommonCombo _combo = new CommonCombo();
        
        #endregion

        #region Initialize
        private void InitControls()
        {
            InitCombo();

            InitGrid();

            ClearControls();
        }

        /// <summary>
        /// 화면내 combo 셋팅
        /// </summary>
        private void InitCombo()
        {

            //String[] sFilterStock = { LoginInfo.CFG_AREA_ID, DateTime.Today.ToString("yyyyMM"), "N" };
            String[] sFilterStock = { LoginInfo.CFG_AREA_ID, DateTime.Today.ToString("yyyyMM"), null };    // 2024.06.10  윤지해 차수마감 취소 기능 추가로 마감되지 않은 차수만 보여주는 부분 삭제. 공백으로 오류 발생함
            _combo.SetComboObjParent(cboStockSeqShot, CommonCombo.ComboStatus.NONE, sCase: "STOCKSEQ", sFilter: sFilterStock);

            SetLineCombo();

            SetProcessCombo();

            SetStorageLocCombo();
        }

        private void InitGrid()
        {
            DataTable dtSummary = new DataTable();
            dtSummary.Columns.Add("ALL", typeof(string));
            dtSummary.Columns.Add("ALL_LOT", typeof(string));
            dtSummary.Columns.Add("ALL_CNT", typeof(string));
            dtSummary.Columns.Add("COMPUTE", typeof(string));
            dtSummary.Columns.Add("COMPUTE_LOT", typeof(string));
            dtSummary.Columns.Add("COMPUTE_CNT", typeof(string));
            dtSummary.Columns.Add("PDA", typeof(string));
            dtSummary.Columns.Add("PDA_LOT", typeof(string));
            dtSummary.Columns.Add("PDA_CNT", typeof(string));
            dtSummary.Columns.Add("REAL", typeof(string));
            dtSummary.Columns.Add("REAL_LOT", typeof(string));
            dtSummary.Columns.Add("REAL_CNT", typeof(string));
            dtSummary.Columns.Add("INFO", typeof(string));
            dtSummary.Columns.Add("INFO_LOT", typeof(string));
            dtSummary.Columns.Add("INFO_CNT", typeof(string));

            DataRow drNew = dtSummary.NewRow();
            drNew["ALL"] = ObjectDic.Instance.GetObjectName("ALL_DUE_DILIGENCE_TARGET");
            drNew["ALL_LOT"] = ObjectDic.Instance.GetObjectName("LOT_CNT");
            drNew["ALL_CNT"] = ObjectDic.Instance.GetObjectName("QTY");
            drNew["COMPUTE"] = ObjectDic.Instance.GetObjectName("COMPUTE_AUTO_DUE_DILIGENCE");
            drNew["COMPUTE_LOT"] = ObjectDic.Instance.GetObjectName("LOT_CNT");
            drNew["COMPUTE_CNT"] = ObjectDic.Instance.GetObjectName("QTY");
            drNew["PDA"] = ObjectDic.Instance.GetObjectName("PDA");
            drNew["PDA_LOT"] = ObjectDic.Instance.GetObjectName("LOT_CNT");
            drNew["PDA_CNT"] = ObjectDic.Instance.GetObjectName("QTY");
            drNew["REAL"] = ObjectDic.Instance.GetObjectName("REAL_UNCONFIRMED");
            drNew["REAL_LOT"] = ObjectDic.Instance.GetObjectName("LOT_CNT");
            drNew["REAL_CNT"] = ObjectDic.Instance.GetObjectName("QTY");
            drNew["INFO"] = ObjectDic.Instance.GetObjectName("INFO_UNCONFIRMED");
            drNew["INFO_LOT"] = ObjectDic.Instance.GetObjectName("LOT_CNT");
            drNew["INFO_CNT"] = ObjectDic.Instance.GetObjectName("QTY");
            dtSummary.Rows.Add(drNew);

            drNew = dtSummary.NewRow();
            drNew["ALL"] = ObjectDic.Instance.GetObjectName("ALL_DUE_DILIGENCE_TARGET");
            drNew["ALL_LOT"] = "-";
            drNew["ALL_CNT"] = "-";
            drNew["COMPUTE"] = ObjectDic.Instance.GetObjectName("COMPUTE_AUTO_DUE_DILIGENCE");
            drNew["COMPUTE_LOT"] = "-";
            drNew["COMPUTE_CNT"] = "-";
            drNew["PDA"] = ObjectDic.Instance.GetObjectName("PDA");
            drNew["PDA_LOT"] = "-";
            drNew["PDA_CNT"] = "-";
            drNew["REAL"] = ObjectDic.Instance.GetObjectName("REAL_UNCONFIRMED");
            drNew["REAL_LOT"] = "-";
            drNew["REAL_CNT"] = "-";
            drNew["INFO"] = ObjectDic.Instance.GetObjectName("INFO_UNCONFIRMED");
            drNew["INFO_LOT"] = "-";
            drNew["INFO_CNT"] = "-";
            dtSummary.Rows.Add(drNew);

            dgSumShot.SetItemsSource(dtSummary, FrameOperation, false);

        }
        #endregion

        #region Event
        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            if (DesignerProperties.GetIsInDesignMode(this)) return;

            if (this.ActualHeight > 0)
            {
                InitControls();

                ApplyPermissions();

                Loaded -= new RoutedEventHandler(UserControl_Loaded);
            }
        }
        

        #region 전산재고 조회
        private void btnSearchShot_Click(object sender, RoutedEventArgs e)
        {
            SetListShot();
        }
        #endregion

        private void cboProcShot_SelectionChanged(object sender, EventArgs e)
        {
            ClearControls();
        }
        
        private void cboEqsgShot_SelectionChanged(object sender, EventArgs e)
        {
            ClearControls();

            SetProcessCombo();
        }


        private void cboStorageLoc_SelectionChanged(object sender, EventArgs e)
        {
            ClearControls();
        }

        private void dgSumShot_LoadedCellPresenter(object sender, DataGridCellEventArgs e)
        {
            dgSumShot.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (e.Cell.Presenter == null || e.Cell.Row.Type.Equals(DataGridRowType.Top))
                {
                    return;
                }

                if (e.Cell.Row.Index == 0 &&
                    (e.Cell.Column.Name.Contains("_LOT") || e.Cell.Column.Name.Contains("_CNT")))
                {
                    e.Cell.Presenter.Background = captionBrush;
                    e.Cell.Presenter.FontWeight = FontWeights.Bold;
                }
                else
                {
                    if (e.Cell.Column.Name.Contains("ALL"))
                    {
                        e.Cell.Presenter.Background = allBrush;
                        e.Cell.Presenter.FontWeight = FontWeights.Bold;
                    }
                    if (e.Cell.Column.Name.Contains("COMPUTE"))
                    {
                        e.Cell.Presenter.Background = surveyBrush;
                        e.Cell.Presenter.FontWeight = FontWeights.Bold;
                    }
                    if (e.Cell.Column.Name.Contains("PDA"))
                    {
                        e.Cell.Presenter.Background = surveyBrush;
                        e.Cell.Presenter.FontWeight = FontWeights.Bold;
                    }
                    if (e.Cell.Column.Name.Contains("REAL"))
                    {
                        e.Cell.Presenter.Background = realBrush;
                        e.Cell.Presenter.FontWeight = FontWeights.Bold;
                    }
                    if (e.Cell.Column.Name.Contains("INFO"))
                    {
                        e.Cell.Presenter.Background = infoBrush;
                        e.Cell.Presenter.FontWeight = FontWeights.Bold;
                    }

                    if (e.Cell.Column.Name.Contains("_LOT") || e.Cell.Column.Name.Contains("_CNT"))
                    {
                        e.Cell.Presenter.HorizontalContentAlignment = HorizontalAlignment.Right;
                    }
                }
            }));
        }

        #endregion

        #region Mehod

        private void ApplyPermissions()
        {
            List<Button> listAuth = new List<Button>
            {
            };

            Util.pageAuth(listAuth, FrameOperation.AUTHORITY);
        }

        private void ClearControls()
        {            
            dgSumShot.SetValue(1, "REAL_LOT", "-");
            dgSumShot.SetValue(1, "REAL_CNT", "-");
            dgSumShot.SetValue(1, "INFO_LOT", "-");
        }
        
        #region 재고 조회
        private void SetListShot()
        {
            try
            {
                //차수는필수입니다.
                if (Util.GetCondition(cboStockSeqShot, "SFU2958").Equals("")) return;

                loadingIndicator.Visibility = Visibility.Visible;

                int iEqsgShotItemCnt = (cboEqsgShot.ItemsSource == null ? 0 : ((DataView)cboEqsgShot.ItemsSource).Count);
                int iEqsgShotSelectedCnt = (cboEqsgShot.ItemsSource == null ? 0 : cboEqsgShot.SelectedItemsToString.Split(',').Length);
                int iProcShotItemCnt = (cboProcShot.ItemsSource == null ? 0 : ((DataView)cboProcShot.ItemsSource).Count);
                int iProcShotSelectedCnt = (cboProcShot.ItemsSource == null ? 0 : cboProcShot.SelectedItemsToString.Split(',').Length);
                int iStorageLocItemCnt = (cboStorageLoc.ItemsSource == null ? 0 : ((DataView)cboStorageLoc.ItemsSource).Count);
                int iStorageLocSelectedCnt = (cboStorageLoc.ItemsSource == null ? 0 : cboStorageLoc.SelectedItemsToString.Split(',').Length);


                loadingIndicator.Visibility = Visibility.Visible;

                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("STCK_CNT_YM", typeof(string));
                RQSTDT.Columns.Add("STCK_CNT_SEQNO", typeof(Int16));
                RQSTDT.Columns.Add("AREAID", typeof(string));
                RQSTDT.Columns.Add("EQSGID", typeof(string));
                RQSTDT.Columns.Add("PROCID", typeof(string));
                RQSTDT.Columns.Add("POSITN", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["STCK_CNT_YM"] = DateTime.Today.ToString("yyyyMM");
                dr["STCK_CNT_SEQNO"] = Convert.ToInt16(Util.GetCondition(cboStockSeqShot, "SFU2958"));
                dr["AREAID"] = LoginInfo.CFG_AREA_ID;
                
                dr["EQSGID"] = (iEqsgShotItemCnt == iEqsgShotSelectedCnt ? null : Util.ConvertEmptyToNull(cboEqsgShot.SelectedItemsToString));
                dr["PROCID"] = (iProcShotItemCnt == iProcShotSelectedCnt ? null : Util.ConvertEmptyToNull(cboProcShot.SelectedItemsToString));
                dr["POSITN"] = (iStorageLocItemCnt == iStorageLocSelectedCnt ? null : Util.ConvertEmptyToNull(cboStorageLoc.SelectedItemsToString));

                RQSTDT.Rows.Add(dr);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_SEL_STOCK_WORKING_SURVEY", "RQSTDT", "RSLTDT", RQSTDT);
                if (dtRslt != null && dtRslt.Rows.Count > 0)
                {
                    DataRow selectRow = dtRslt.Rows[0];
                    dgSumShot.SetValue(1, "REAL_LOT", Util.NVC_Int(selectRow["LOT_REAL_CNT"]));
                    dgSumShot.SetValue(1, "REAL_CNT", Util.NVC_Int(selectRow["SUM_REAL_QTY"]));
                    dgSumShot.SetValue(1, "INFO_LOT", Util.NVC_Int(selectRow["LOT_INFO_CNT"]));                    
                }

                dtRslt = new ClientProxy().ExecuteServiceSync("DA_SEL_STOCK_OVER_WORK_REAL_UNIDENT", "RQSTDT", "RSLTDT", RQSTDT);
                if (dtRslt != null && dtRslt.Rows.Count > 0)
                {
                    dgRealList.SetItemsSource(dtRslt, FrameOperation, false);
                }

                dtRslt = new ClientProxy().ExecuteServiceSync("DA_SEL_STOCK_OVER_WORK_INFO_UNIDENT", "RQSTDT", "RSLTDT", RQSTDT);
                if (dtRslt != null && dtRslt.Rows.Count > 0)
                {
                    dgInfoList.SetItemsSource(dtRslt, FrameOperation, false);
                }

                loadingIndicator.Visibility = Visibility.Collapsed;
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
        #endregion

        #region 공정 Combo
        private void SetProcessCombo()
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("AREAID", typeof(string));
                RQSTDT.Columns.Add("EQSGID", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["AREAID"] = LoginInfo.CFG_AREA_ID;
                dr["EQSGID"] = cboEqsgShot.SelectedItemsToString;
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_PROCESS_CBO_MULTI_WITH_EQSG", "RQSTDT", "RSLTDT", RQSTDT);
                //DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_COMBO_FORM_PROC_BY_LINE", "RQSTDT", "RSLTDT", RQSTDT);

                cboProcShot.ItemsSource = DataTableConverter.Convert(dtResult);

                if (dtResult.Rows.Count > 0)
                {
                    cboProcShot.CheckAll();
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        #endregion

        #region 라인 Combo
        private void SetLineCombo()
        {
            cboEqsgShot.SelectionChanged -= cboEqsgShot_SelectionChanged;

            string[] arrColumn = { "LANGID", "AREAID" };

            DataTable RQSTDT = new DataTable();
            RQSTDT.TableName = "RQSTDT";
            RQSTDT.Columns.Add("LANGID", typeof(string));
            RQSTDT.Columns.Add("AREAID", typeof(string));

            DataRow dr = RQSTDT.NewRow();
            dr["LANGID"] = LoginInfo.LANGID;
            dr["AREAID"] = LoginInfo.CFG_AREA_ID;

            RQSTDT.Rows.Add(dr);

            DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_COMBO_FORM_LINE", "RQSTDT", "RSLTDT", RQSTDT);

            cboEqsgShot.ItemsSource = DataTableConverter.Convert(dtResult);

            if (dtResult.Rows.Count > 0)
            {
                cboEqsgShot.CheckAll();
            }

            cboEqsgShot.SelectionChanged += cboEqsgShot_SelectionChanged;
        }

 

        #endregion

        #region 보관위치 Combo
        private void SetStorageLocCombo()
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("CMCDTYPE", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["CMCDTYPE"] = "STORAGE_LOCATION";
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_COMMCODE_CBO_WITHOUT_CODE", "RQSTDT", "RSLTDT", RQSTDT);

                cboStorageLoc.ItemsSource = DataTableConverter.Convert(dtResult);

                if (dtResult.Rows.Count > 0)
                {
                    cboStorageLoc.CheckAll();
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        
        #endregion

        #endregion


    }
}
