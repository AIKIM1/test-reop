/*************************************************************************************************************************
 Created Date : 2021.09.06
      Creator : 오화백
   Decription : FastTrack Lot 이력조회
--------------------------------------------------------------------------------------------------------------------------
 [Change History]
  2021.09.06  DEVELOPER : Initial Created.
  2022.03.30  이춘우    : [C20220322-000659] - GMES(ESWA 전극)시스템의 Fast track 기능 개선 요청의 건
  2022.04.19  정재홍    : [C20220322-000659] - GMES(ESWA 전극)시스템의 Fast track 기능 개선 요청의 건 - 그리드 Data 바인딩 부분 수정
**************************************************************************************************************************/

using C1.WPF;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using System;
using System.Collections.Generic;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using System.Windows.Input;
using System.Linq;
using System.Threading;
using System.Windows.Media;
using C1.WPF.DataGrid;
using LGC.GMES.MES.CMM001.Extensions;
using LGC.GMES.MES.CMM001.UserControls;
using System.Windows.Data;
using System.Windows.Media.Animation;



namespace LGC.GMES.MES.COM001
{
    public partial class COM001_365 : UserControl, IWorkArea
    {
        public int iPopupEnableFlag = 0;
        public int iScrollRow = 0 , iScrollCol = 0;

        #region Declaration & Constructor 

        private BizDataSet _Biz = new BizDataSet();
        public COM001_365()
        {
            InitializeComponent();
            InitCombo();
        }

        public IFrameOperation FrameOperation
        {
            get;
            set;
        }

        #endregion

        #region Initialize
        private void InitCombo()
        {
            //동,라인,공정,설비 셋팅
            CommonCombo _combo = new CommonCombo();

            //동
            C1ComboBox[] cboAreaChild = { cboEquipmentSegment };
            _combo.SetCombo(cboArea, CommonCombo.ComboStatus.SELECT, cbChild: cboAreaChild);

            //라인
            C1ComboBox[] cboEquipmentSegmentParent = { cboArea };
            //C1ComboBox[] cboEquipmentSegmentChild = {cboProcess };
            _combo.SetCombo(cboEquipmentSegment, CommonCombo.ComboStatus.SELECT, null, cbParent: cboEquipmentSegmentParent);

            //공정
            C1ComboBox[] cboProcessParent = { cboEquipmentSegment };
            //C1ComboBox[] cboProcessChild = { cboEquipment };
            _combo.SetCombo(cboProcess, CommonCombo.ComboStatus.SELECT, null, cboProcessParent);

            //설비
            C1ComboBox[] cboEquipmentParent = { cboProcess };
            _combo.SetCombo(cboEquipment, CommonCombo.ComboStatus.ALL, cbParent: cboEquipmentParent, sFilter: new string[] { cboProcess.SelectedValue.ToString() });

            cboEquipmentSegment.SelectedItemChanged += cboEquipmentSegment_SelectedItemChanged;
            cboProcess.SelectedItemChanged += CboProcess_SelectedItemChanged;

            SetEquipment();
        }

        #endregion

        #region Event

        #region [Form Load]
        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            //사용자 권한별로 버튼 숨기기
            List<Button> listAuth = new List<Button>();
            
            Util.pageAuth(listAuth, FrameOperation.AUTHORITY);
            //여기까지 사용자 권한별로 버튼 숨기기

            dtpDateFrom.SelectedDataTimeChanged += dtpDate_SelectedDataTimeChanged;
            dtpDateTo.SelectedDataTimeChanged += dtpDate_SelectedDataTimeChanged;

        }
        #endregion

        #region [조회]
        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            GetList();
        }
        #endregion

        #region [작업일] - 조회 조건
        private void dtpDate_SelectedDataTimeChanged(object sender, SelectionChangedEventArgs e)
        {
            if (dtpDateFrom.SelectedDateTime.Year > 1 && dtpDateTo.SelectedDateTime.Year > 1)
            {
                LGCDatePicker LGCdp = sender as LGCDatePicker;

                if ((dtpDateTo.SelectedDateTime - dtpDateFrom.SelectedDateTime).TotalDays < 0)
                {
                    dtpDateFrom.SelectedDateTime = dtpDateTo.SelectedDateTime;
                    return;
                }
            }
        }
        #endregion

        #region 이력  Cell Merge : dgEmptyCarrier_MergingCells()
        /// <summary>
        ///  공 Tray 관련 체크박스  Cell Merge
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void dgNote_MergingCells(object sender, DataGridMergingCellsEventArgs e)
        {
            try
            {
             
                #region C/L LOT 기준 머지
                int idxS = 0;
                int idxE = 0;
                bool bStrt = false;
                string sTmpLvCd = string.Empty;
                string sTmpTOTALQTY = string.Empty;

                for (int i = dgNote.TopRows.Count; i < dgNote.Rows.Count; i++)
                {

                    if (dgNote.Rows[i].DataItem.GetType() == typeof(System.Data.DataRowView))
                    {
                        
                        if (!bStrt)
                        {
                            bStrt = true;
                            sTmpLvCd = Util.NVC(DataTableConverter.GetValue(dgNote.Rows[i].DataItem, "LOTID"));
                            idxS = i;

                            if (sTmpLvCd.Equals(""))
                                bStrt = false;
                        }
                        else
                        {
                            if (Util.NVC(DataTableConverter.GetValue(dgNote.Rows[i].DataItem, "LOTID")).Equals(sTmpLvCd))
                            {
                                idxE = i;
                                //마지막 Row 일경우
                                if (i == dgNote.Rows.Count - 1)
                                {
                                    if (idxS > idxE)
                                    {
                                        idxE = idxS;
                                    }
                                    e.Merge(new DataGridCellsRange(dgNote.GetCell(idxS, dgNote.Columns["ELEC_TYPE"].Index), dgNote.GetCell(idxE, dgNote.Columns["ELEC_TYPE"].Index)));
                                    e.Merge(new DataGridCellsRange(dgNote.GetCell(idxS, dgNote.Columns["PRJT_NAME"].Index), dgNote.GetCell(idxE, dgNote.Columns["PRJT_NAME"].Index)));
                                    e.Merge(new DataGridCellsRange(dgNote.GetCell(idxS, dgNote.Columns["PRODID"].Index), dgNote.GetCell(idxE, dgNote.Columns["PRODID"].Index)));
                                    e.Merge(new DataGridCellsRange(dgNote.GetCell(idxS, dgNote.Columns["FAST_TRACK_REG_DTTM"].Index), dgNote.GetCell(idxE, dgNote.Columns["FAST_TRACK_REG_DTTM"].Index)));
                                    e.Merge(new DataGridCellsRange(dgNote.GetCell(idxS, dgNote.Columns["QMS_RSLT_INPUT_DTTM"].Index), dgNote.GetCell(idxE, dgNote.Columns["QMS_RSLT_INPUT_DTTM"].Index)));
                                    e.Merge(new DataGridCellsRange(dgNote.GetCell(idxS, dgNote.Columns["EQPTNAME"].Index), dgNote.GetCell(idxE, dgNote.Columns["EQPTNAME"].Index)));
                                    e.Merge(new DataGridCellsRange(dgNote.GetCell(idxS, dgNote.Columns["EQPTID"].Index), dgNote.GetCell(idxE, dgNote.Columns["EQPTID"].Index)));
                                    e.Merge(new DataGridCellsRange(dgNote.GetCell(idxS, dgNote.Columns["WIPDTTM_ST"].Index), dgNote.GetCell(idxE, dgNote.Columns["WIPDTTM_ST"].Index)));
                                    e.Merge(new DataGridCellsRange(dgNote.GetCell(idxS, dgNote.Columns["WIPDTTM_ED"].Index), dgNote.GetCell(idxE, dgNote.Columns["WIPDTTM_ED"].Index)));
                                    e.Merge(new DataGridCellsRange(dgNote.GetCell(idxS, dgNote.Columns["WIPHOLD"].Index), dgNote.GetCell(idxE, dgNote.Columns["WIPHOLD"].Index)));
                                    e.Merge(new DataGridCellsRange(dgNote.GetCell(idxS, dgNote.Columns["HOLD_NOTE"].Index), dgNote.GetCell(idxE, dgNote.Columns["HOLD_NOTE"].Index)));
                                    // FASTTRACK 이력 추가
                                    e.Merge(new DataGridCellsRange(dgNote.GetCell(idxS, dgNote.Columns["WRIN_FLAG"].Index), dgNote.GetCell(idxE, dgNote.Columns["WRIN_FLAG"].Index)));
                                    e.Merge(new DataGridCellsRange(dgNote.GetCell(idxS, dgNote.Columns["WRIN_ATTR01"].Index), dgNote.GetCell(idxE, dgNote.Columns["WRIN_ATTR01"].Index)));
                                    e.Merge(new DataGridCellsRange(dgNote.GetCell(idxS, dgNote.Columns["WRIN_ATTR02"].Index), dgNote.GetCell(idxE, dgNote.Columns["WRIN_ATTR02"].Index)));
                                    e.Merge(new DataGridCellsRange(dgNote.GetCell(idxS, dgNote.Columns["WRIN_ATTR03"].Index), dgNote.GetCell(idxE, dgNote.Columns["WRIN_ATTR03"].Index)));
                                    e.Merge(new DataGridCellsRange(dgNote.GetCell(idxS, dgNote.Columns["NOTE"].Index), dgNote.GetCell(idxE, dgNote.Columns["NOTE"].Index)));
                                    e.Merge(new DataGridCellsRange(dgNote.GetCell(idxS, dgNote.Columns["CMCODE_CT1"].Index), dgNote.GetCell(idxE, dgNote.Columns["CMCODE_CT1"].Index)));
                                    e.Merge(new DataGridCellsRange(dgNote.GetCell(idxS, dgNote.Columns["CMCODE_CT2"].Index), dgNote.GetCell(idxE, dgNote.Columns["CMCODE_CT2"].Index)));
                                    e.Merge(new DataGridCellsRange(dgNote.GetCell(idxS, dgNote.Columns["CMCODE_CT3"].Index), dgNote.GetCell(idxE, dgNote.Columns["CMCODE_CT3"].Index)));
                                }
                            }
                            else
                            {
                                if (idxS > idxE)
                                {
                                    idxE = idxS;
                                }
                                e.Merge(new DataGridCellsRange(dgNote.GetCell(idxS, dgNote.Columns["ELEC_TYPE"].Index), dgNote.GetCell(idxE, dgNote.Columns["ELEC_TYPE"].Index)));
                                e.Merge(new DataGridCellsRange(dgNote.GetCell(idxS, dgNote.Columns["PRJT_NAME"].Index), dgNote.GetCell(idxE, dgNote.Columns["PRJT_NAME"].Index)));
                                e.Merge(new DataGridCellsRange(dgNote.GetCell(idxS, dgNote.Columns["PRODID"].Index), dgNote.GetCell(idxE, dgNote.Columns["PRODID"].Index)));
                                e.Merge(new DataGridCellsRange(dgNote.GetCell(idxS, dgNote.Columns["FAST_TRACK_REG_DTTM"].Index), dgNote.GetCell(idxE, dgNote.Columns["FAST_TRACK_REG_DTTM"].Index)));
                                e.Merge(new DataGridCellsRange(dgNote.GetCell(idxS, dgNote.Columns["QMS_RSLT_INPUT_DTTM"].Index), dgNote.GetCell(idxE, dgNote.Columns["QMS_RSLT_INPUT_DTTM"].Index)));
                                e.Merge(new DataGridCellsRange(dgNote.GetCell(idxS, dgNote.Columns["EQPTNAME"].Index), dgNote.GetCell(idxE, dgNote.Columns["EQPTNAME"].Index)));
                                e.Merge(new DataGridCellsRange(dgNote.GetCell(idxS, dgNote.Columns["EQPTID"].Index), dgNote.GetCell(idxE, dgNote.Columns["EQPTID"].Index)));
                                e.Merge(new DataGridCellsRange(dgNote.GetCell(idxS, dgNote.Columns["WIPDTTM_ST"].Index), dgNote.GetCell(idxE, dgNote.Columns["WIPDTTM_ST"].Index)));
                                e.Merge(new DataGridCellsRange(dgNote.GetCell(idxS, dgNote.Columns["WIPDTTM_ED"].Index), dgNote.GetCell(idxE, dgNote.Columns["WIPDTTM_ED"].Index)));
                                e.Merge(new DataGridCellsRange(dgNote.GetCell(idxS, dgNote.Columns["WIPHOLD"].Index), dgNote.GetCell(idxE, dgNote.Columns["WIPHOLD"].Index)));
                                e.Merge(new DataGridCellsRange(dgNote.GetCell(idxS, dgNote.Columns["HOLD_NOTE"].Index), dgNote.GetCell(idxE, dgNote.Columns["HOLD_NOTE"].Index)));
                                // FASTTRACK 이력 추가
                                e.Merge(new DataGridCellsRange(dgNote.GetCell(idxS, dgNote.Columns["WRIN_FLAG"].Index), dgNote.GetCell(idxE, dgNote.Columns["WRIN_FLAG"].Index)));
                                e.Merge(new DataGridCellsRange(dgNote.GetCell(idxS, dgNote.Columns["WRIN_ATTR01"].Index), dgNote.GetCell(idxE, dgNote.Columns["WRIN_ATTR01"].Index)));
                                e.Merge(new DataGridCellsRange(dgNote.GetCell(idxS, dgNote.Columns["WRIN_ATTR02"].Index), dgNote.GetCell(idxE, dgNote.Columns["WRIN_ATTR02"].Index)));
                                e.Merge(new DataGridCellsRange(dgNote.GetCell(idxS, dgNote.Columns["WRIN_ATTR03"].Index), dgNote.GetCell(idxE, dgNote.Columns["WRIN_ATTR03"].Index)));
                                e.Merge(new DataGridCellsRange(dgNote.GetCell(idxS, dgNote.Columns["NOTE"].Index), dgNote.GetCell(idxE, dgNote.Columns["NOTE"].Index)));
                                e.Merge(new DataGridCellsRange(dgNote.GetCell(idxS, dgNote.Columns["CMCODE_CT1"].Index), dgNote.GetCell(idxE, dgNote.Columns["CMCODE_CT1"].Index)));
                                e.Merge(new DataGridCellsRange(dgNote.GetCell(idxS, dgNote.Columns["CMCODE_CT2"].Index), dgNote.GetCell(idxE, dgNote.Columns["CMCODE_CT2"].Index)));
                                e.Merge(new DataGridCellsRange(dgNote.GetCell(idxS, dgNote.Columns["CMCODE_CT3"].Index), dgNote.GetCell(idxE, dgNote.Columns["CMCODE_CT3"].Index)));

                                bStrt = true;
                                sTmpLvCd = Util.NVC(DataTableConverter.GetValue(dgNote.Rows[i].DataItem, "LOTID"));
                                idxS = i;

                                if (sTmpLvCd.Equals(""))
                                    bStrt = false;
                            }
                        }
                    }

                }


                #endregion

                #region R/P 기준 머지
                int idxS_RP = 0;
                int idxE_RP = 0;
                bool bStrt_RP = false;
                string sTmpLvCd_RP = string.Empty;
                for (int i = dgNote.TopRows.Count; i < dgNote.Rows.Count; i++)
                {
                    if(Util.NVC(DataTableConverter.GetValue(dgNote.Rows[i].DataItem, "LOTID_RP"))!= string.Empty )
                    {
                        if (dgNote.Rows[i].DataItem.GetType() == typeof(System.Data.DataRowView))
                        {

                            if (!bStrt_RP)
                            {
                                bStrt_RP = true;
                                sTmpLvCd_RP = Util.NVC(DataTableConverter.GetValue(dgNote.Rows[i].DataItem, "LOTID_RP"));
                                idxS_RP = i;

                                if (sTmpLvCd_RP.Equals(""))
                                    bStrt_RP = false;
                            }
                            else
                            {
                                if (Util.NVC(DataTableConverter.GetValue(dgNote.Rows[i].DataItem, "LOTID_RP")).Equals(sTmpLvCd_RP))
                                {
                                    idxE_RP = i;
                                    //마지막 Row 일경우
                                    if (i == dgNote.Rows.Count - 1)
                                    {
                                        if (idxS_RP > idxE_RP)
                                        {
                                            idxE_RP = idxS_RP;
                                        }
                                        e.Merge(new DataGridCellsRange(dgNote.GetCell(idxS_RP, dgNote.Columns["LOTID_RP"].Index), dgNote.GetCell(idxE_RP, dgNote.Columns["LOTID_RP"].Index)));
                                        e.Merge(new DataGridCellsRange(dgNote.GetCell(idxS_RP, dgNote.Columns["EQPTNAME_RP"].Index), dgNote.GetCell(idxE_RP, dgNote.Columns["EQPTNAME_RP"].Index)));
                                        e.Merge(new DataGridCellsRange(dgNote.GetCell(idxS_RP, dgNote.Columns["EQPTID_RP"].Index), dgNote.GetCell(idxE_RP, dgNote.Columns["EQPTID_RP"].Index)));
                                        e.Merge(new DataGridCellsRange(dgNote.GetCell(idxS_RP, dgNote.Columns["WIPDTTM_ST_RP"].Index), dgNote.GetCell(idxE_RP, dgNote.Columns["WIPDTTM_ST_RP"].Index)));
                                        e.Merge(new DataGridCellsRange(dgNote.GetCell(idxS_RP, dgNote.Columns["WIPDTTM_ED_RP"].Index), dgNote.GetCell(idxE_RP, dgNote.Columns["WIPDTTM_ED_RP"].Index)));
                                        e.Merge(new DataGridCellsRange(dgNote.GetCell(idxS_RP, dgNote.Columns["WIPHOLD_RP"].Index), dgNote.GetCell(idxE_RP, dgNote.Columns["WIPHOLD_RP"].Index)));
                                        e.Merge(new DataGridCellsRange(dgNote.GetCell(idxS_RP, dgNote.Columns["HOLD_NOTE_RP"].Index), dgNote.GetCell(idxE_RP, dgNote.Columns["HOLD_NOTE_RP"].Index)));
                                        // FASTTRACK 이력 추가
                                        e.Merge(new DataGridCellsRange(dgNote.GetCell(idxS_RP, dgNote.Columns["DELAY_FLAG_RP"].Index), dgNote.GetCell(idxE_RP, dgNote.Columns["DELAY_FLAG_RP"].Index)));
                                        e.Merge(new DataGridCellsRange(dgNote.GetCell(idxS_RP, dgNote.Columns["DELAY_NOTE_RP"].Index), dgNote.GetCell(idxE_RP, dgNote.Columns["DELAY_NOTE_RP"].Index)));
                                        e.Merge(new DataGridCellsRange(dgNote.GetCell(idxS_RP, dgNote.Columns["WRIN_FLAG_RP"].Index), dgNote.GetCell(idxE_RP, dgNote.Columns["WRIN_FLAG_RP"].Index)));
                                        e.Merge(new DataGridCellsRange(dgNote.GetCell(idxS_RP, dgNote.Columns["WRIN_ATTR01_RP"].Index), dgNote.GetCell(idxE_RP, dgNote.Columns["WRIN_ATTR01_RP"].Index)));
                                        e.Merge(new DataGridCellsRange(dgNote.GetCell(idxS_RP, dgNote.Columns["WRIN_ATTR02_RP"].Index), dgNote.GetCell(idxE_RP, dgNote.Columns["WRIN_ATTR02_RP"].Index)));
                                        e.Merge(new DataGridCellsRange(dgNote.GetCell(idxS_RP, dgNote.Columns["WRIN_ATTR03_RP"].Index), dgNote.GetCell(idxE_RP, dgNote.Columns["WRIN_ATTR03_RP"].Index)));
                                        e.Merge(new DataGridCellsRange(dgNote.GetCell(idxS_RP, dgNote.Columns["NOTE_RP"].Index), dgNote.GetCell(idxE_RP, dgNote.Columns["NOTE_RP"].Index)));
                                        e.Merge(new DataGridCellsRange(dgNote.GetCell(idxS, dgNote.Columns["CMCODE_RP1"].Index), dgNote.GetCell(idxE, dgNote.Columns["CMCODE_RP1"].Index)));
                                        e.Merge(new DataGridCellsRange(dgNote.GetCell(idxS, dgNote.Columns["CMCODE_RP2"].Index), dgNote.GetCell(idxE, dgNote.Columns["CMCODE_RP2"].Index)));
                                        e.Merge(new DataGridCellsRange(dgNote.GetCell(idxS, dgNote.Columns["CMCODE_RP3"].Index), dgNote.GetCell(idxE, dgNote.Columns["CMCODE_RP3"].Index)));
                                    }
                                }
                                else
                                {
                                    if (idxS_RP > idxE_RP)
                                    {
                                        idxE_RP = idxS_RP;
                                    }
                                    e.Merge(new DataGridCellsRange(dgNote.GetCell(idxS_RP, dgNote.Columns["LOTID_RP"].Index), dgNote.GetCell(idxE_RP, dgNote.Columns["LOTID_RP"].Index)));
                                    e.Merge(new DataGridCellsRange(dgNote.GetCell(idxS_RP, dgNote.Columns["EQPTNAME_RP"].Index), dgNote.GetCell(idxE_RP, dgNote.Columns["EQPTNAME_RP"].Index)));
                                    e.Merge(new DataGridCellsRange(dgNote.GetCell(idxS_RP, dgNote.Columns["EQPTID_RP"].Index), dgNote.GetCell(idxE_RP, dgNote.Columns["EQPTID_RP"].Index)));
                                    e.Merge(new DataGridCellsRange(dgNote.GetCell(idxS_RP, dgNote.Columns["WIPDTTM_ST_RP"].Index), dgNote.GetCell(idxE_RP, dgNote.Columns["WIPDTTM_ST_RP"].Index)));
                                    e.Merge(new DataGridCellsRange(dgNote.GetCell(idxS_RP, dgNote.Columns["WIPDTTM_ED_RP"].Index), dgNote.GetCell(idxE_RP, dgNote.Columns["WIPDTTM_ED_RP"].Index)));
                                    e.Merge(new DataGridCellsRange(dgNote.GetCell(idxS_RP, dgNote.Columns["WIPHOLD_RP"].Index), dgNote.GetCell(idxE_RP, dgNote.Columns["WIPHOLD_RP"].Index)));
                                    e.Merge(new DataGridCellsRange(dgNote.GetCell(idxS_RP, dgNote.Columns["HOLD_NOTE_RP"].Index), dgNote.GetCell(idxE_RP, dgNote.Columns["HOLD_NOTE_RP"].Index)));
                                    // FASTTRACK 이력 추가
                                    e.Merge(new DataGridCellsRange(dgNote.GetCell(idxS_RP, dgNote.Columns["DELAY_FLAG_RP"].Index), dgNote.GetCell(idxE_RP, dgNote.Columns["DELAY_FLAG_RP"].Index)));
                                    e.Merge(new DataGridCellsRange(dgNote.GetCell(idxS_RP, dgNote.Columns["DELAY_NOTE_RP"].Index), dgNote.GetCell(idxE_RP, dgNote.Columns["DELAY_NOTE_RP"].Index)));
                                    e.Merge(new DataGridCellsRange(dgNote.GetCell(idxS_RP, dgNote.Columns["WRIN_FLAG_RP"].Index), dgNote.GetCell(idxE_RP, dgNote.Columns["WRIN_FLAG_RP"].Index)));
                                    e.Merge(new DataGridCellsRange(dgNote.GetCell(idxS_RP, dgNote.Columns["WRIN_ATTR01_RP"].Index), dgNote.GetCell(idxE_RP, dgNote.Columns["WRIN_ATTR01_RP"].Index)));
                                    e.Merge(new DataGridCellsRange(dgNote.GetCell(idxS_RP, dgNote.Columns["WRIN_ATTR02_RP"].Index), dgNote.GetCell(idxE_RP, dgNote.Columns["WRIN_ATTR02_RP"].Index)));
                                    e.Merge(new DataGridCellsRange(dgNote.GetCell(idxS_RP, dgNote.Columns["WRIN_ATTR03_RP"].Index), dgNote.GetCell(idxE_RP, dgNote.Columns["WRIN_ATTR03_RP"].Index)));
                                    e.Merge(new DataGridCellsRange(dgNote.GetCell(idxS_RP, dgNote.Columns["NOTE_RP"].Index), dgNote.GetCell(idxE_RP, dgNote.Columns["NOTE_RP"].Index)));
                                    e.Merge(new DataGridCellsRange(dgNote.GetCell(idxS, dgNote.Columns["CMCODE_RP1"].Index), dgNote.GetCell(idxE, dgNote.Columns["CMCODE_RP1"].Index)));
                                    e.Merge(new DataGridCellsRange(dgNote.GetCell(idxS, dgNote.Columns["CMCODE_RP2"].Index), dgNote.GetCell(idxE, dgNote.Columns["CMCODE_RP2"].Index)));
                                    e.Merge(new DataGridCellsRange(dgNote.GetCell(idxS, dgNote.Columns["CMCODE_RP3"].Index), dgNote.GetCell(idxE, dgNote.Columns["CMCODE_RP3"].Index)));

                                    bStrt_RP = true;
                                    sTmpLvCd_RP = Util.NVC(DataTableConverter.GetValue(dgNote.Rows[i].DataItem, "LOTID_RP"));
                                    idxS_RP = i;

                                    if (sTmpLvCd_RP.Equals(""))
                                        bStrt_RP = false;
                                }
                            }
                        }
                    }
                }

                #endregion

                #region S/L 기준 머지
                int idxS_SL = 0;
                int idxE_SL = 0;
                bool bStrt_SL = false;
                string sTmpLvCd_SL = string.Empty;
                for (int i = dgNote.TopRows.Count; i < dgNote.Rows.Count; i++)
                {

                    if(Util.NVC(DataTableConverter.GetValue(dgNote.Rows[i].DataItem, "LOTID_SL")) != string.Empty)
                    {
                        if (dgNote.Rows[i].DataItem.GetType() == typeof(System.Data.DataRowView))
                        {

                            if (!bStrt_SL)
                            {
                                bStrt_SL = true;
                                sTmpLvCd_SL = Util.NVC(DataTableConverter.GetValue(dgNote.Rows[i].DataItem, "LOTID_SL"));
                                idxS_SL = i;

                                if (sTmpLvCd_SL.Equals(""))
                                    bStrt_SL = false;
                            }
                            else
                            {
                                if (Util.NVC(DataTableConverter.GetValue(dgNote.Rows[i].DataItem, "LOTID_SL")).Equals(sTmpLvCd_SL))
                                {
                                    idxE_SL = i;
                                    //마지막 Row 일경우
                                    if (i == dgNote.Rows.Count - 1)
                                    {
                                        if (idxS_SL > idxE_SL)
                                        {
                                            idxE_SL = idxS_SL;
                                        }
                                        e.Merge(new DataGridCellsRange(dgNote.GetCell(idxS_SL, dgNote.Columns["LOTID_SL"].Index), dgNote.GetCell(idxE_SL, dgNote.Columns["LOTID_SL"].Index)));
                                        e.Merge(new DataGridCellsRange(dgNote.GetCell(idxS_SL, dgNote.Columns["EQPTNAME_SL"].Index), dgNote.GetCell(idxE_SL, dgNote.Columns["EQPTNAME_SL"].Index)));
                                        e.Merge(new DataGridCellsRange(dgNote.GetCell(idxS_SL, dgNote.Columns["EQPTID_SL"].Index), dgNote.GetCell(idxE_SL, dgNote.Columns["EQPTID_SL"].Index)));
                                        e.Merge(new DataGridCellsRange(dgNote.GetCell(idxS_SL, dgNote.Columns["WIPDTTM_ST_SL"].Index), dgNote.GetCell(idxE_SL, dgNote.Columns["WIPDTTM_ST_SL"].Index)));
                                        e.Merge(new DataGridCellsRange(dgNote.GetCell(idxS_SL, dgNote.Columns["WIPDTTM_ED_SL"].Index), dgNote.GetCell(idxE_SL, dgNote.Columns["WIPDTTM_ED_SL"].Index)));
                                        e.Merge(new DataGridCellsRange(dgNote.GetCell(idxS_SL, dgNote.Columns["WIPHOLD_SL"].Index), dgNote.GetCell(idxE_SL, dgNote.Columns["WIPHOLD_SL"].Index)));
                                        e.Merge(new DataGridCellsRange(dgNote.GetCell(idxS_SL, dgNote.Columns["HOLD_NOTE_SL"].Index), dgNote.GetCell(idxE_SL, dgNote.Columns["HOLD_NOTE_SL"].Index)));
                                        // FASTTRACK 이력 추가
                                        e.Merge(new DataGridCellsRange(dgNote.GetCell(idxS_SL, dgNote.Columns["DELAY_FLAG_SL"].Index), dgNote.GetCell(idxE_SL, dgNote.Columns["DELAY_FLAG_SL"].Index)));
                                        e.Merge(new DataGridCellsRange(dgNote.GetCell(idxS_SL, dgNote.Columns["DELAY_NOTE_SL"].Index), dgNote.GetCell(idxE_SL, dgNote.Columns["DELAY_NOTE_SL"].Index)));
                                        e.Merge(new DataGridCellsRange(dgNote.GetCell(idxS_SL, dgNote.Columns["NOTE_SL"].Index), dgNote.GetCell(idxE_SL, dgNote.Columns["NOTE_SL"].Index)));
                                    }
                                }
                                else
                                {
                                    if (idxS_SL > idxE_SL)
                                    {
                                        idxE_SL = idxS_SL;
                                    }
                                    e.Merge(new DataGridCellsRange(dgNote.GetCell(idxS_SL, dgNote.Columns["LOTID_SL"].Index), dgNote.GetCell(idxE_SL, dgNote.Columns["LOTID_SL"].Index)));
                                    e.Merge(new DataGridCellsRange(dgNote.GetCell(idxS_SL, dgNote.Columns["EQPTNAME_SL"].Index), dgNote.GetCell(idxE_SL, dgNote.Columns["EQPTNAME_SL"].Index)));
                                    e.Merge(new DataGridCellsRange(dgNote.GetCell(idxS_SL, dgNote.Columns["EQPTID_SL"].Index), dgNote.GetCell(idxE_SL, dgNote.Columns["EQPTID_SL"].Index)));
                                    e.Merge(new DataGridCellsRange(dgNote.GetCell(idxS_SL, dgNote.Columns["WIPDTTM_ST_SL"].Index), dgNote.GetCell(idxE_SL, dgNote.Columns["WIPDTTM_ST_SL"].Index)));
                                    e.Merge(new DataGridCellsRange(dgNote.GetCell(idxS_SL, dgNote.Columns["WIPDTTM_ED_SL"].Index), dgNote.GetCell(idxE_SL, dgNote.Columns["WIPDTTM_ED_SL"].Index)));
                                    e.Merge(new DataGridCellsRange(dgNote.GetCell(idxS_SL, dgNote.Columns["WIPHOLD_SL"].Index), dgNote.GetCell(idxE_SL, dgNote.Columns["WIPHOLD_SL"].Index)));
                                    e.Merge(new DataGridCellsRange(dgNote.GetCell(idxS_SL, dgNote.Columns["HOLD_NOTE_SL"].Index), dgNote.GetCell(idxE_SL, dgNote.Columns["HOLD_NOTE_SL"].Index)));
                                    // FASTTRACK 이력 추가
                                    e.Merge(new DataGridCellsRange(dgNote.GetCell(idxS_SL, dgNote.Columns["DELAY_FLAG_SL"].Index), dgNote.GetCell(idxE_SL, dgNote.Columns["DELAY_FLAG_SL"].Index)));
                                    e.Merge(new DataGridCellsRange(dgNote.GetCell(idxS_SL, dgNote.Columns["DELAY_NOTE_SL"].Index), dgNote.GetCell(idxE_SL, dgNote.Columns["DELAY_NOTE_SL"].Index)));
                                    e.Merge(new DataGridCellsRange(dgNote.GetCell(idxS_SL, dgNote.Columns["NOTE_SL"].Index), dgNote.GetCell(idxE_SL, dgNote.Columns["NOTE_SL"].Index)));

                                    bStrt_SL = true;
                                    sTmpLvCd_SL = Util.NVC(DataTableConverter.GetValue(dgNote.Rows[i].DataItem, "LOTID_SL"));
                                    idxS_SL = i;

                                    if (sTmpLvCd_SL.Equals(""))
                                        bStrt_SL = false;
                                }
                            }
                        }
                    }
                }
                #endregion
            }
            catch (Exception ex)
            {
                //Util.MessageException(ex);
            }
        }
        #endregion

        #region [라인] - 조회 조건
        private void cboEquipmentSegment_SelectedItemChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            if (cboEquipmentSegment.Items.Count > 0 && cboEquipmentSegment.SelectedValue != null)
            {
                SetProcess();

            }
        }
        #endregion

        #region [공정] - 조회 조건
        private void CboProcess_SelectedItemChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            if (cboProcess.Items.Count > 0 && cboProcess.SelectedValue != null)
            {
                SetEquipment();
            }
        }
        #endregion


        #endregion

        #region Mehod

        #region [BizCall]

        #region [### 조회 ###]
        public void GetList()
        {
            try
            {
                
                ShowLoadingIndicator();
                DoEvents();

                DataTable dtRqst = new DataTable();
                dtRqst.Columns.Add("LANGID", typeof(string));
                dtRqst.Columns.Add("LOTID", typeof(string));
                dtRqst.Columns.Add("FROM_DATE", typeof(string));
                dtRqst.Columns.Add("TO_DATE", typeof(string));
                dtRqst.Columns.Add("EQSGID", typeof(string));
                dtRqst.Columns.Add("PROCID", typeof(string));
                dtRqst.Columns.Add("EQPTID", typeof(string));
       
                DataRow dr = dtRqst.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;

                if(txtLotID.Text != string.Empty)
                {
                    dr["LOTID"] = txtLotID.Text;
                }
                else
                {
                    dr["LOTID"] = null;
                }
                dr["FROM_DATE"] = Util.GetCondition(dtpDateFrom);
                dr["TO_DATE"] = Util.GetCondition(dtpDateTo);

                dr["EQSGID"] = Util.GetCondition(cboEquipmentSegment, "SFU1223");   //라인정보를 선택하세요.
                dr["PROCID"] = Util.GetCondition(cboProcess, "SFU1459");            //공정을 선택하세요.
                dr["EQPTID"] = cboEquipment.SelectedValue.ToString() == string.Empty ? null : cboEquipment.SelectedValue.ToString();


                dtRqst.Rows.Add(dr);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_FAST_TRACK_REG_HIST", "INDATA", "OUTDATA", dtRqst);

                Util.GridSetData(dgNote, dtRslt, FrameOperation, true);

                dgNote.CanUserEditRows = false;

                dgNote.MergingCells -= dgNote_MergingCells;
                dgNote.MergingCells += dgNote_MergingCells;



            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
            finally
            {
                HiddenLoadingIndicator();
            }
        }

        #endregion


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

        private void DoEvents()
        {
            Application.Current.Dispatcher.Invoke(DispatcherPriority.Background, new System.Threading.ThreadStart(delegate { }));
        }

        #endregion

        #region [Process 정보 가져오기]
        private void SetProcess()
        {
            try
            {
                // 동을 선택하세요.
                string sArea = Util.GetCondition(cboArea);
                if (string.IsNullOrWhiteSpace(sArea))
                    return;

                string sEquipmentSegment = Util.GetCondition(cboEquipmentSegment);

                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("AREAID", typeof(string));
                RQSTDT.Columns.Add("EQSGID", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["AREAID"] = sArea;
                dr["EQSGID"] = string.IsNullOrWhiteSpace(sEquipmentSegment) ? null : sEquipmentSegment;
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_PROCESS_BY_AREA_CBO", "RQSTDT", "RSLTDT", RQSTDT);

                cboProcess.DisplayMemberPath = "CBO_NAME";
                cboProcess.SelectedValuePath = "CBO_CODE";

                //DataRow drIns = dtResult.NewRow();
                //drIns["CBO_NAME"] = "-SELECT-";
                //drIns["CBO_CODE"] = "SELECT";
                //dtResult.Rows.InsertAt(drIns, 0);

                cboProcess.ItemsSource = dtResult.Copy().AsDataView();

                if (!LoginInfo.CFG_PROC_ID.Equals(""))
                {
                    cboProcess.SelectedValue = LoginInfo.CFG_PROC_ID;

                    if (cboProcess.SelectedIndex < 0)
                        cboProcess.SelectedIndex = 0;
                }
                else
                {
                    if (cboProcess.Items.Count > 0)
                        cboProcess.SelectedIndex = 0;
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        #endregion

        #region [설비 정보 가져오기]
        private void SetEquipment()
        {
            try
            {
                // 동을 선택하세요.
                string sArea = Util.GetCondition(cboArea);
                if (string.IsNullOrWhiteSpace(sArea))
                    return;

                string sProc = Util.GetCondition(cboProcess);
                if (string.IsNullOrWhiteSpace(sProc))
                {
                    cboEquipment.ItemsSource = null;
                    return;
                }

                string sEquipmentSegment = Util.GetCondition(cboEquipmentSegment);

                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("AREAID", typeof(string));
                RQSTDT.Columns.Add("EQSGID", typeof(string));
                RQSTDT.Columns.Add("PROCID", typeof(string));
                RQSTDT.Columns.Add("COATER_EQPT_TYPE_CODE", typeof(string));
                RQSTDT.Columns.Add("EQPT_RSLT_CODE", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["AREAID"] = sArea;
                dr["EQSGID"] = string.IsNullOrWhiteSpace(sEquipmentSegment) ? null : sEquipmentSegment;
                dr["PROCID"] = cboProcess.SelectedValue.ToString();

                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_EQUIPMENT_AREA_CBO", "RQSTDT", "RSLTDT", RQSTDT);

                cboEquipment.DisplayMemberPath = "CBO_NAME";
                cboEquipment.SelectedValuePath = "CBO_CODE";

                DataRow drIns = dtResult.NewRow();
                drIns["CBO_NAME"] = "-ALL-";
                drIns["CBO_CODE"] = "";
                dtResult.Rows.InsertAt(drIns, 0);

                cboEquipment.ItemsSource = dtResult.Copy().AsDataView();

                if (!LoginInfo.CFG_EQPT_ID.Equals(""))
                {
                    cboEquipment.SelectedValue = LoginInfo.CFG_EQPT_ID;

                    if (cboEquipment.SelectedIndex < 0)
                        cboEquipment.SelectedIndex = 0;
                }
                else
                {
                    cboEquipment.SelectedIndex = 0;
                }

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        #endregion

        #endregion

        private void dgNote_LoadedCellPresenter(object sender, DataGridCellEventArgs e)
        {
            if (sender == null)
                return;

            C1DataGrid dataGrid = sender as C1DataGrid;

            dataGrid?.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (e.Cell.Presenter != null)
                {
                    if (e.Cell.Column.Name == "LOTID" || e.Cell.Column.Name == "LOTID_RP" || e.Cell.Column.Name == "LOTID_SL")
                    {
                        e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Blue);
                    }
                }
            }));
        }

        private void dgNote_UnloadedCellPresenter(object sender, DataGridCellEventArgs e)
        {
            C1DataGrid dataGrid = sender as C1DataGrid;

            if (dataGrid != null)
            {
                if (e.Cell != null && e.Cell.Presenter != null)
                {
                    if (e.Cell.Row.Type == DataGridRowType.Item)
                    {
                        e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Black);
                    }
                }
            }
        }

        private void dgNote_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            try
            {
                // 팝업 중복 실행 방지
                if (iPopupEnableFlag != 0)
                    return;

                C1.WPF.DataGrid.C1DataGrid datagrid = (sender as C1.WPF.DataGrid.C1DataGrid);

                Point pnt = e.GetPosition(null);
                C1.WPF.DataGrid.DataGridCell cell = datagrid.GetCellFromPoint(pnt);

                if (cell == null || datagrid.CurrentRow == null)
                {
                    return;
                }

                if (datagrid.CurrentColumn.Name == "LOTID" || datagrid.CurrentColumn.Name == "LOTID_RP" || datagrid.CurrentColumn.Name == "LOTID_SL")
                {
                    COM001_365_CREATE_HIST popupCreateHist = new COM001_365_CREATE_HIST();
                    popupCreateHist.FrameOperation = FrameOperation;

                    popupCreateHist.Closed += new EventHandler(popupCreateHist_Closed);

                    object[] parameters = new object[9];

                    parameters[0] = datagrid.CurrentCell.Value.ToString();          // LotId
                    parameters[1] = datagrid.CurrentColumn.Name.ToString();         // Eqpt Name

                    if (datagrid.CurrentColumn.Name == "LOTID")
                    {
                        //parameters[2] = datagrid.GetCell(cell.Row.Index, 12).Text.ToString();     // Wrin Flag
                        //parameters[3] = datagrid.GetCell(cell.Row.Index, 17).Text.ToString();     // Wrin Attr01
                        //parameters[4] = datagrid.GetCell(cell.Row.Index, 18).Text.ToString();     // Wrin Attr02
                        //parameters[5] = datagrid.GetCell(cell.Row.Index, 19).Text.ToString();     // Wrin Attr03
                        //parameters[6] = datagrid.GetCell(cell.Row.Index, 16).Text.ToString();     // Note

                        parameters[2] = Util.NVC(DataTableConverter.GetValue(datagrid.CurrentRow.DataItem, "WRIN_FLAG"));       // Wrin Flag
                        parameters[3] = Util.NVC(DataTableConverter.GetValue(datagrid.CurrentRow.DataItem, "CMCODE_CT1"));      // Wrin Attr01
                        parameters[4] = Util.NVC(DataTableConverter.GetValue(datagrid.CurrentRow.DataItem, "CMCODE_CT2"));      // Wrin Attr02
                        parameters[5] = Util.NVC(DataTableConverter.GetValue(datagrid.CurrentRow.DataItem, "CMCODE_CT3"));      // Wrin Attr03
                        parameters[6] = Util.NVC(DataTableConverter.GetValue(datagrid.CurrentRow.DataItem, "NOTE"));            // Note

                        iScrollCol = 16;
                    }
                    else if (datagrid.CurrentColumn.Name == "LOTID_RP")
                    {
                        //parameters[2] = datagrid.GetCell(cell.Row.Index, 27).Text.ToString();     // Delay Flag
                        //parameters[3] = datagrid.GetCell(cell.Row.Index, 28).Text.ToString();     // Delay Note
                        //parameters[4] = datagrid.GetCell(cell.Row.Index, 29).Text.ToString();     // Wrin Flag
                        //parameters[5] = datagrid.GetCell(cell.Row.Index, 34).Text.ToString();     // Wrin Attr01
                        //parameters[6] = datagrid.GetCell(cell.Row.Index, 35).Text.ToString();     // Wrin Attr02
                        //parameters[7] = datagrid.GetCell(cell.Row.Index, 36).Text.ToString();     // Wrin Attr03
                        //parameters[8] = datagrid.GetCell(cell.Row.Index, 33).Text.ToString();     // Note

                        parameters[2] = Util.NVC(DataTableConverter.GetValue(datagrid.CurrentRow.DataItem, "DELAY_FLAG_RP"));  // Delay Flag
                        parameters[3] = Util.NVC(DataTableConverter.GetValue(datagrid.CurrentRow.DataItem, "DELAY_NOTE_RP"));  // Delay Note
                        parameters[4] = Util.NVC(DataTableConverter.GetValue(datagrid.CurrentRow.DataItem, "WRIN_FLAG_RP"));   // Wrin Flag
                        parameters[5] = Util.NVC(DataTableConverter.GetValue(datagrid.CurrentRow.DataItem, "CMCODE_RP1"));     // Wrin Attr01
                        parameters[6] = Util.NVC(DataTableConverter.GetValue(datagrid.CurrentRow.DataItem, "CMCODE_RP2"));     // Wrin Attr02
                        parameters[7] = Util.NVC(DataTableConverter.GetValue(datagrid.CurrentRow.DataItem, "CMCODE_RP3"));     // Wrin Attr03
                        parameters[8] = Util.NVC(DataTableConverter.GetValue(datagrid.CurrentRow.DataItem, "NOTE_RP"));        // Note

                        iScrollCol = 33;
                    }
                    else
                    {
                        //parameters[2] = datagrid.GetCell(cell.Row.Index, 44).Text.ToString();     // Delay Flag
                        //parameters[3] = datagrid.GetCell(cell.Row.Index, 45).Text.ToString();     // Delay Note
                        //parameters[4] = datagrid.GetCell(cell.Row.Index, 46).Text.ToString();     // Note

                        parameters[2] = Util.NVC(DataTableConverter.GetValue(datagrid.CurrentRow.DataItem, "DELAY_FLAG_SL"));    // Delay Flag
                        parameters[3] = Util.NVC(DataTableConverter.GetValue(datagrid.CurrentRow.DataItem, "DELAY_NOTE_SL"));    // Delay Note
                        parameters[4] = Util.NVC(DataTableConverter.GetValue(datagrid.CurrentRow.DataItem, "NOTE_SL"));          // Note

                        iScrollCol = 46;
                    }

                    iPopupEnableFlag = 1;
                    iScrollRow = dgNote.CurrentRow == null ? 0 : dgNote.CurrentRow.Index;

                    C1WindowExtension.SetParameters(popupCreateHist, parameters);
                    grdMain.Children.Add(popupCreateHist);
                    popupCreateHist.BringToFront();
                }
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }

        private void popupCreateHist_Closed(object sender, EventArgs e)
        {
            COM001_365_CREATE_HIST window = sender as COM001_365_CREATE_HIST;
            if (window.DialogResult == MessageBoxResult.OK)
            {
                GetList();

                dgNote.ScrollIntoView(iScrollRow, iScrollCol);
            }

            iPopupEnableFlag = 0;
        }

    }
}