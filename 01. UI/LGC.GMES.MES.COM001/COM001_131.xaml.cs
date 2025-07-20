/*************************************************************************************
 Created Date : 2018.09.12
      Creator : INS 김동일K
   Decription : 전지 GMES 고도화 - 패키지 특별관리LOT QMS 자동 HOLD 이력 조회
--------------------------------------------------------------------------------------
 [Change History]
  2018.09.12  INS 김동일K : Initial Created.

**************************************************************************************/


using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using System;
using System.Collections.Generic;
using System.Data;
using System.Windows;
using System.Windows.Controls;

namespace LGC.GMES.MES.COM001
{
    /// <summary>
    /// COM001_131.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class COM001_131 : UserControl, IWorkArea
    {

        Util _util = new Util();

        /// <summary>
        /// Frame과 상호작용하기 위한 객체
        /// </summary>
        public IFrameOperation FrameOperation
        {
            get;
            set;
        }

        public COM001_131()
        {
            InitializeComponent();
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                ApplyPermissions();
                InitCombo();
                SetEvent();
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.Columns.Add("LANGID");
                RQSTDT.Columns.Add("HOLD_TYPE_CODE");
                RQSTDT.Columns.Add("HOLD_FLAG");
                RQSTDT.Columns.Add("FROM_HOLD_DTTM");
                RQSTDT.Columns.Add("TO_HOLD_DTTM");
                RQSTDT.Columns.Add("AREAID");
                RQSTDT.Columns.Add("ASSY_LOTID");
                RQSTDT.Columns.Add("SUBLOTID");
                RQSTDT.Columns.Add("HOLD_TRGT_CODE");
                RQSTDT.Columns.Add("PRODID");
                
                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["HOLD_TYPE_CODE"] = "SPCL_HOLD"; // 특별관리 HOLD

                if (!string.IsNullOrEmpty(txtLotID.Text))
                    dr["ASSY_LOTID"] = string.IsNullOrEmpty(txtLotID.Text) ? null : txtLotID.Text;
                else
                {

                    dr["FROM_HOLD_DTTM"] = dtpDateFrom.SelectedDateTime.ToString("yyyy-MM-dd") + " 00:00:00";
                    dr["TO_HOLD_DTTM"] = dtpDateTo.SelectedDateTime.ToString("yyyy-MM-dd") + " 23:59:59";
                    dr["AREAID"] = (string)cboArea.SelectedValue;
                    //dr["HOLD_TRGT_CODE"] = (string)cboLotType2.SelectedValue;
                    dr["HOLD_FLAG"] = (string)cboHoldYN.SelectedValue == "" ? null : (string)cboHoldYN.SelectedValue;
                }
                //dr["SUBLOTID"] = string.IsNullOrEmpty(txtCellID2.Text) ? null : txtCellID2.Text;
                //dr["PRODID"] = string.IsNullOrEmpty(txtProdID2.Text) ? null : txtProdID2.Text;


                RQSTDT.Rows.Add(dr);

                new ClientProxy().ExecuteService("DA_BAS_SEL_TB_SFC_ASSY_LOT_HOLD_HIST_BY_HOLD_TYPE", "INDATA", "OUTDATA", RQSTDT, (result, bizEx) =>
                {
                    try
                    {
                        if (bizEx != null)
                        {
                            Util.MessageException(bizEx);
                            return;
                        }

                        if (!result.Columns.Contains("CHK"))
                            result = _util.gridCheckColumnAdd(result, "CHK");

                        Util.GridSetData(dgHist, result, FrameOperation, true);
                    }
                    catch (Exception ex)
                    {
                        Util.MessageException(ex);
                    }
                    finally
                    {
                        loadingIndicator.Visibility = Visibility.Collapsed;
                    }
                });
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                loadingIndicator.Visibility = Visibility.Collapsed;
            }
        }

        private void ApplyPermissions()
        {
            List<Button> listAuth = new List<Button>();
            //listAuth.Add(btnInReplace);
            Util.pageAuth(listAuth, FrameOperation.AUTHORITY);
        }

        private void InitCombo()
        {
            try
            {
                CommonCombo _combo = new CommonCombo();
                _combo.SetCombo(cboArea, CommonCombo.ComboStatus.ALL, sCase: "ALLAREA");

                string[] sFilter = { "HOLD_YN" };
                _combo.SetCombo(cboHoldYN, CommonCombo.ComboStatus.ALL, sFilter: sFilter, sCase: "COMMCODE");
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void SetEvent()
        {
            try
            {
                this.Loaded -= UserControl_Loaded;
                dtpDateFrom.SelectedDataTimeChanged += dtpDateFrom_SelectedDataTimeChanged;
                dtpDateTo.SelectedDataTimeChanged += dtpDateTo_SelectedDataTimeChanged;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
            
        }

        private void dtpDateFrom_SelectedDataTimeChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                LGCDatePicker dtPik = sender as LGCDatePicker;
                if (Convert.ToDecimal(dtpDateTo.SelectedDateTime.ToString("yyyyMMdd")) < Convert.ToDecimal(dtPik.SelectedDateTime.ToString("yyyyMMdd")))
                {
                    dtPik.SelectedDateTime = dtpDateTo.SelectedDateTime;
                    return;
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
            
        }

        private void dtpDateTo_SelectedDataTimeChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                LGCDatePicker dtPik = sender as LGCDatePicker;
                if (Convert.ToDecimal(dtpDateFrom.SelectedDateTime.ToString("yyyyMMdd")) > Convert.ToDecimal(dtPik.SelectedDateTime.ToString("yyyyMMdd")))
                {
                    dtPik.SelectedDateTime = dtpDateFrom.SelectedDateTime;
                    return;
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
            
        }
    }
}
