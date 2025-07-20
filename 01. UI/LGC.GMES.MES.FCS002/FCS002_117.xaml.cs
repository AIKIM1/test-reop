/*************************************************************************************
 Created Date : 2021.08.25
      Creator : 강동희
   Decription : 외관불량 Cell 이력조회
--------------------------------------------------------------------------------------
 [Change History]
  2021.08.25  강동희 : Initial Created.
  2022.10.19  강동희 : 충방전기, 대용량 방전기 추가.
**************************************************************************************/

using C1.WPF.DataGrid;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;
using System;
using System.Data;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Collections.Generic;
using LGC.GMES.MES.CMM001.Extensions;
using LGC.GMES.MES.FCS002.Controls;

namespace LGC.GMES.MES.FCS002
{
    public partial class FCS002_117 : UserControl, IWorkArea
    {
        #region Declaration & Constructor 
        private DataTable _dtHeader;
        private string sRowCount = string.Empty;

        public FCS002_117()
        {
            InitializeComponent();
        }

        public IFrameOperation FrameOperation
        {
            get;
            set;
        }

        #endregion

        #region Initialize

        private void Initialize()
        {
            _dtHeader = new DataTable();

            sRowCount = GetCommonCode("SELECT_ROW_COUNT", "A"); //20221019_충방전기, 대용량 방전기 추가
            txtRowCntCell.Value = Convert.ToInt32(sRowCount); //20221019_충방전기, 대용량 방전기 추가

            InitSpread_JF();
            InitSpread_OCV();
            InitSpread_SELECTOR();
            InitSpread_FORMATION(); //20221019_충방전기, 대용량 방전기 추가
            InitSpread_POWERGRADING(); //20221019_충방전기, 대용량 방전기 추가
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            Initialize();

            this.Loaded -= UserControl_Loaded;
        }

        private void InitSpread_JF()
        {
            Util.gridClear(dgJigFormation); //Grid clear

            int Header_Row_count = 2;

            //칼럼 헤더 행 추가
            for (int i = 0; i < Header_Row_count; i++)
            {
                DataGridColumnHeaderRow HR = new DataGridColumnHeaderRow();
                dgJigFormation.TopRows.Add(HR);
            }

            //FIX
            FixedMultiHeader(dgJigFormation, "CELL_ID|CELL_ID", "SUBLOTID", false, 0);
            FixedMultiHeader(dgJigFormation, "TRAY_LOT_ID|TRAY_LOT_ID", "LOTID", false, 0);

            AddDefectHeader_JF(dgJigFormation, 0);
        }

        private void InitSpread_OCV()
        {
            Util.gridClear(dgOCV); //Grid clear

            int Header_Row_count = 2;

            //칼럼 헤더 행 추가
            for (int i = 0; i < Header_Row_count; i++)
            {
                DataGridColumnHeaderRow HR = new DataGridColumnHeaderRow();
                dgOCV.TopRows.Add(HR);
            }

            //FIX
            FixedMultiHeader(dgOCV, "CELL_ID|CELL_ID", "SUBLOTID", false, 0);
            FixedMultiHeader(dgOCV, "TRAY_LOT_ID|TRAY_LOT_ID", "LOTID", false, 0);

            AddDefectHeader_OCV_SELECTOR(dgOCV, "8", "81", 0);
        }

        private void InitSpread_SELECTOR()
        {
            Util.gridClear(dgSelector); //Grid clear

            int Header_Row_count = 2;

            //칼럼 헤더 행 추가
            for (int i = 0; i < Header_Row_count; i++)
            {
                DataGridColumnHeaderRow HR = new DataGridColumnHeaderRow();
                dgSelector.TopRows.Add(HR);
            }

            //FIX
            FixedMultiHeader(dgSelector, "CELL_ID|CELL_ID", "SUBLOTID", false, 0);
            FixedMultiHeader(dgSelector, "TRAY_LOT_ID|TRAY_LOT_ID", "LOTID", false, 0);

            AddDefectHeader_OCV_SELECTOR(dgSelector, "6", "61", 0);
        }

        //20221019_충방전기, 대용량 방전기 추가 START
        private void InitSpread_FORMATION()
        {
            Util.gridClear(dgFormation); //Grid clear

            int Header_Row_count = 2;

            //칼럼 헤더 행 추가
            for (int i = 0; i < Header_Row_count; i++)
            {
                DataGridColumnHeaderRow HR = new DataGridColumnHeaderRow();
                dgFormation.TopRows.Add(HR);
            }

            //FIX
            FixedMultiHeader(dgFormation, "CELL_ID|CELL_ID", "SUBLOTID", false, 0);
            FixedMultiHeader(dgFormation, "TRAY_LOT_ID|TRAY_LOT_ID", "LOTID", false, 0);

            AddDefectHeader_FORMATION(dgFormation, 0);
        }
        //20221019_충방전기, 대용량 방전기 추가 END

        //20221019_충방전기, 대용량 방전기 추가 START
        private void InitSpread_POWERGRADING()
        {
            Util.gridClear(dgPowerGrading); //Grid clear

            int Header_Row_count = 2;

            //칼럼 헤더 행 추가
            for (int i = 0; i < Header_Row_count; i++)
            {
                DataGridColumnHeaderRow HR = new DataGridColumnHeaderRow();
                dgPowerGrading.TopRows.Add(HR);
            }

            //FIX
            FixedMultiHeader(dgPowerGrading, "CELL_ID|CELL_ID", "SUBLOTID", false, 0);
            FixedMultiHeader(dgPowerGrading, "TRAY_LOT_ID|TRAY_LOT_ID", "LOTID", false, 0);

            AddDefectHeader_POWERGRADING(dgPowerGrading, 0);
        }
        //20221019_충방전기, 대용량 방전기 추가 END

        private void FixedMultiHeader(C1DataGrid dg, string sName, string sBindName, bool bPercent, int iWidth = 75)
        {
            bool bReadOnly = true;
            bool bEditable = false;
            bool bVisible = true;

            string[] sColName = sName.Split('|');

            List<string> Multi_Header = new List<string>();
            Multi_Header = sColName.ToList();

            var column_TEXT = CreateTextColumn(null, Multi_Header, sBindName, sBindName, iWidth, bReadOnly: bReadOnly, bEditable: bEditable, bVisible: bVisible, bPercent: bPercent);
            dg.Columns.Add(column_TEXT);
        }

        private void AddDefectHeader_JF(C1DataGrid dg, int iWidth = 0)
        {
            DataSet dsDirectInfo = new DataSet();
            DataTable dtRqst = new DataTable();
            dtRqst.TableName = "RQSTDT";
            dtRqst.Columns.Add("LANGID", typeof(string));
            dtRqst.Columns.Add("AREAID", typeof(string));
            dtRqst.Columns.Add("USE_FLAG", typeof(string));
            dtRqst.Columns.Add("S26", typeof(string));
            dtRqst.Columns.Add("S27", typeof(string));

            DataRow dr = dtRqst.NewRow();
            dr["LANGID"] = LoginInfo.LANGID;
            dr["AREAID"] = LoginInfo.CFG_AREA_ID;
            dr["USE_FLAG"] = "Y";
            dr["S26"] = "J";
            dr["S27"] = "J7,J1";

            dtRqst.Rows.Add(dr);

            DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_SEL_DEFECT_CELL_HIST_PROCESS_MB", "RQSTDT", "RSLTDT", dtRqst);

            DataRow[] drRslt = dtRslt.Select("S25 = '1'"); // 1차 공정만

            foreach (DataRow d in drRslt)
            {
                bool bReadOnly = true;
                bool bEditable = false;
                bool bVisible = true;

                string sCol = string.Empty;
                string sColHeader = string.Empty;
                string sBinding = string.Empty;

                //칼럼 헤더 행 추가
                if (d["PROC_SEQ"].ToString().Trim().Equals("1") || d["PROC_SEQ"].ToString().Trim().Equals("2"))
                {
                    for (int i = 0; i < 5; i++)
                    {
                        if (i.Equals(0))
                        {
                            // CH
                            sCol = d["PROCID"].ToString().Trim() + "_EQPT_INFO";
                            sColHeader = d["PROC_DESC"].ToString().Trim() + "|설비정보";
                            sBinding = sCol;
                        }
                        else if (i.Equals(1))
                        {
                            // CH
                            sCol = d["PROCID"].ToString().Trim() + "_CSTSLOT";
                            sColHeader = d["PROC_DESC"].ToString().Trim() + "|CHANNEL";
                            sBinding = sCol;
                        }
                        else if (i.Equals(2))
                        {
                            // JIG
                            sCol = d["PROCID"].ToString().Trim() + "_EQPTID";
                            sColHeader = d["PROC_DESC"].ToString().Trim() + "|JIG";
                            sBinding = sCol;
                        }
                        else if (i.Equals(3))
                        {
                            // 시작시간
                            sCol = d["PROCID"].ToString().Trim() + "_WRK_STRT_DTTM";
                            sColHeader = d["PROC_DESC"].ToString().Trim() + "|START_TIME";
                            sBinding = sCol;
                        }
                        else if (i.Equals(4))
                        {
                            // 종료시간
                            sCol = d["PROCID"].ToString().Trim() + "_WRK_END_DTTM";
                            sColHeader = d["PROC_DESC"].ToString().Trim() + "|END_TIME";
                            sBinding = sCol;
                        }
                        string[] sColName = sColHeader.Split('|');

                        //칼럼 헤더 자동 병합을 위해 Header로 사용할 List
                        List<string> Multi_Header = new List<string>();
                        Multi_Header = sColName.ToList();

                        var column_TEXT = CreateTextColumn(null, Multi_Header, sCol, sBinding, iWidth, bReadOnly: bReadOnly, bEditable: bEditable, bVisible: bVisible, bPercent: false);
                        dg.Columns.Add(column_TEXT);
                    }
                }
                else if (d["PROC_SEQ"].ToString().Trim().Equals("5"))
                {
                    for (int i = 0; i < 3; i++)
                    {
                        if (i.Equals(0))
                        {
                            // TRAY
                            sCol = d["PROCID"].ToString().Trim() + "_TRAY";
                            sColHeader = d["PROC_DESC"].ToString().Trim() + "|TRAY_ID";
                            sBinding = sCol;
                        }
                        else if (i.Equals(1))
                        {
                            // CH
                            sCol = d["PROCID"].ToString().Trim() + "_CSTSLOT";
                            sColHeader = d["PROC_DESC"].ToString().Trim() + "|CHANNEL";
                            sBinding = sCol;
                        }
                        else if (i.Equals(2))
                        {
                            // 종료시간
                            sCol = d["PROCID"].ToString().Trim() + "_WRK_END_DTTM";
                            sColHeader = d["PROC_DESC"].ToString().Trim() + "|END_TIME";
                            sBinding = sCol;
                        }
                        string[] sColName = sColHeader.Split('|');

                        //칼럼 헤더 자동 병합을 위해 Header로 사용할 List
                        List<string> Multi_Header = new List<string>();
                        Multi_Header = sColName.ToList();

                        var column_TEXT = CreateTextColumn(null, Multi_Header, sCol, sBinding, iWidth, bReadOnly: bReadOnly, bEditable: bEditable, bVisible: bVisible, bPercent: false);
                        dg.Columns.Add(column_TEXT);
                    }
                }
                else
                {
                    for (int i = 0; i < 3; i++)
                    {
                        if (i.Equals(0))
                        {
                            // JIG
                            sCol = d["PROCID"].ToString().Trim() + "_EQPTID";
                            sColHeader = d["PROC_DESC"].ToString().Trim() + "|JIG";
                            sBinding = sCol;
                        }
                        else if (i.Equals(1))
                        {
                            // 시작시간
                            sCol = d["PROCID"].ToString().Trim() + "_WRK_STRT_DTTM";
                            sColHeader = d["PROC_DESC"].ToString().Trim() + "|START_TIME";
                            sBinding = sCol;
                        }
                        else if (i.Equals(2))
                        {
                            // 종료시간
                            sCol = d["PROCID"].ToString().Trim() + "_WRK_END_DTTM";
                            sColHeader = d["PROC_DESC"].ToString().Trim() + "|END_TIME";
                            sBinding = sCol;
                        }
                        string[] sColName = sColHeader.Split('|');

                        //칼럼 헤더 자동 병합을 위해 Header로 사용할 List
                        List<string> Multi_Header = new List<string>();
                        Multi_Header = sColName.ToList();

                        var column_TEXT = CreateTextColumn(null, Multi_Header, sCol, sBinding, iWidth, bReadOnly: bReadOnly, bEditable: bEditable, bVisible: bVisible, bPercent: false);
                        dg.Columns.Add(column_TEXT);
                    }
                }
            }
        }

        private void AddDefectHeader_OCV_SELECTOR(C1DataGrid dg, string s26, string s27, int iWidth = 0)
        {
            DataSet dsDirectInfo = new DataSet();
            DataTable dtRqst = new DataTable();
            dtRqst.TableName = "RQSTDT";
            dtRqst.Columns.Add("LANGID", typeof(string));
            dtRqst.Columns.Add("AREAID", typeof(string));
            dtRqst.Columns.Add("USE_FLAG", typeof(string));
            dtRqst.Columns.Add("S26", typeof(string));
            dtRqst.Columns.Add("S27", typeof(string));

            DataRow dr = dtRqst.NewRow();
            dr["LANGID"] = LoginInfo.LANGID;
            dr["AREAID"] = LoginInfo.CFG_AREA_ID;
            dr["USE_FLAG"] = "Y";
            dr["S26"] = s26;
            dr["S27"] = s27;
            dtRqst.Rows.Add(dr);

            DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_SEL_DEFECT_CELL_HIST_PROCESS_MB", "RQSTDT", "RSLTDT", dtRqst);

            foreach (DataRow d in dtRslt.Rows)
            {
                bool bReadOnly = true;
                bool bEditable = false;
                bool bVisible = true;

                string sCol = string.Empty;
                string sColHeader = string.Empty;
                string sBinding = string.Empty;

                //칼럼 헤더 행 추가
                if (d["PROC_SEQ"].ToString().Trim().Equals("1"))
                {
                    for (int i = 0; i < 5; i++)
                    {
                        if (i.Equals(0))
                        {
                            // TRAY
                            sCol = d["PROCID"].ToString().Trim() + "_EQPTID";
                            sColHeader = d["PROC_DESC"].ToString().Trim() + "|EQPT_UNIT";
                            sBinding = sCol;
                        }
                        else if (i.Equals(1))
                        {
                            // TRAY
                            sCol = d["PROCID"].ToString().Trim() + "_CSTID";
                            sColHeader = d["PROC_DESC"].ToString().Trim() + "|TRAY_ID";
                            sBinding = sCol;
                        }
                        else if (i.Equals(2))
                        {
                            // CH
                            sCol = d["PROCID"].ToString().Trim() + "_CSTSLOT";
                            sColHeader = d["PROC_DESC"].ToString().Trim() + "|CHANNEL";
                            sBinding = sCol;
                        }
                        else if (i.Equals(3))
                        {
                            // 시작시간
                            sCol = d["PROCID"].ToString().Trim() + "_WRK_STRT_DTTM";
                            sColHeader = d["PROC_DESC"].ToString().Trim() + "|START_TIME";
                            sBinding = sCol;
                        }
                        else if (i.Equals(4))
                        {
                            // 종료시간
                            sCol = d["PROCID"].ToString().Trim() + "_WRK_END_DTTM";
                            sColHeader = d["PROC_DESC"].ToString().Trim() + "|END_TIME";
                            sBinding = sCol;
                        }
                        string[] sColName = sColHeader.Split('|');

                        //칼럼 헤더 자동 병합을 위해 Header로 사용할 List
                        List<string> Multi_Header = new List<string>();
                        Multi_Header = sColName.ToList();

                        var column_TEXT = CreateTextColumn(null, Multi_Header, sCol, sBinding, iWidth, bReadOnly: bReadOnly, bEditable: bEditable, bVisible: bVisible, bPercent: false);
                        dg.Columns.Add(column_TEXT);
                    }
                }
                else
                {
                }
            }
        }

        //20221019_충방전기, 대용량 방전기 추가 START
        private void AddDefectHeader_FORMATION(C1DataGrid dg, int iWidth = 0)
        {
            DataSet dsDirectInfo = new DataSet();
            DataTable dtRqst = new DataTable();
            dtRqst.TableName = "RQSTDT";
            dtRqst.Columns.Add("LANGID", typeof(string));
            dtRqst.Columns.Add("AREAID", typeof(string));
            dtRqst.Columns.Add("USE_FLAG", typeof(string));
            dtRqst.Columns.Add("S26", typeof(string));
            dtRqst.Columns.Add("S27", typeof(string));

            DataRow dr = dtRqst.NewRow();
            dr["LANGID"] = LoginInfo.LANGID;
            dr["AREAID"] = LoginInfo.CFG_AREA_ID;
            dr["USE_FLAG"] = "Y";
            dr["S26"] = "1";
            dr["S27"] = "11,12";

            dtRqst.Rows.Add(dr);

            DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_SEL_DEFECT_CELL_HIST_PROCESS_MB", "RQSTDT", "RSLTDT", dtRqst);

            //DataRow[] drRslt = dtRslt.Select("S25 = '1'"); // 1차 공정만

            foreach (DataRow d in dtRslt.Rows)
            {
                bool bReadOnly = true;
                bool bEditable = false;
                bool bVisible = true;

                string sCol = string.Empty;
                string sColHeader = string.Empty;
                string sBinding = string.Empty;

                //칼럼 헤더 행 추가
                if (d["PROC_SEQ"].ToString().Trim().Equals("1") || d["PROC_SEQ"].ToString().Trim().Equals("2"))
                {
                    for (int i = 0; i < 5; i++)
                    {
                        if (i.Equals(0))
                        {
                            // CH
                            sCol = d["PROCID"].ToString().Trim() + "_EQPT_INFO";
                            sColHeader = d["PROC_DESC"].ToString().Trim() + "|설비정보";
                            sBinding = sCol;
                        }
                        else if (i.Equals(1))
                        {
                            // CH
                            sCol = d["PROCID"].ToString().Trim() + "_CSTSLOT";
                            sColHeader = d["PROC_DESC"].ToString().Trim() + "|CHANNEL";
                            sBinding = sCol;
                        }
                        else if (i.Equals(2))
                        {
                            // JIG
                            sCol = d["PROCID"].ToString().Trim() + "_EQPTID";
                            sColHeader = d["PROC_DESC"].ToString().Trim() + "|BOX";
                            sBinding = sCol;
                        }
                        else if (i.Equals(3))
                        {
                            // 시작시간
                            sCol = d["PROCID"].ToString().Trim() + "_WRK_STRT_DTTM";
                            sColHeader = d["PROC_DESC"].ToString().Trim() + "|START_TIME";
                            sBinding = sCol;
                        }
                        else if (i.Equals(4))
                        {
                            // 종료시간
                            sCol = d["PROCID"].ToString().Trim() + "_WRK_END_DTTM";
                            sColHeader = d["PROC_DESC"].ToString().Trim() + "|END_TIME";
                            sBinding = sCol;
                        }
                        string[] sColName = sColHeader.Split('|');

                        //칼럼 헤더 자동 병합을 위해 Header로 사용할 List
                        List<string> Multi_Header = new List<string>();
                        Multi_Header = sColName.ToList();

                        var column_TEXT = CreateTextColumn(null, Multi_Header, sCol, sBinding, iWidth, bReadOnly: bReadOnly, bEditable: bEditable, bVisible: bVisible, bPercent: false);
                        dg.Columns.Add(column_TEXT);
                    }
                }
                else
                {
                }
            }
        }
        //20221019_충방전기, 대용량 방전기 추가 END

        //20221019_충방전기, 대용량 방전기 추가 START
        private void AddDefectHeader_POWERGRADING(C1DataGrid dg, int iWidth = 0)
        {
            DataSet dsDirectInfo = new DataSet();
            DataTable dtRqst = new DataTable();
            dtRqst.TableName = "RQSTDT";
            dtRqst.Columns.Add("LANGID", typeof(string));
            dtRqst.Columns.Add("AREAID", typeof(string));
            dtRqst.Columns.Add("USE_FLAG", typeof(string));
            dtRqst.Columns.Add("S26", typeof(string));
            dtRqst.Columns.Add("S27", typeof(string));

            DataRow dr = dtRqst.NewRow();
            dr["LANGID"] = LoginInfo.LANGID;
            dr["AREAID"] = LoginInfo.CFG_AREA_ID;
            dr["USE_FLAG"] = "Y";
            dr["S26"] = "1";
            dr["S27"] = "17";

            dtRqst.Rows.Add(dr);

            DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_SEL_DEFECT_CELL_HIST_PROCESS_MB", "RQSTDT", "RSLTDT", dtRqst);

            //DataRow[] drRslt = dtRslt.Select("S25 = '1'"); // 1차 공정만

            foreach (DataRow d in dtRslt.Rows)
            {
                bool bReadOnly = true;
                bool bEditable = false;
                bool bVisible = true;

                string sCol = string.Empty;
                string sColHeader = string.Empty;
                string sBinding = string.Empty;

                //칼럼 헤더 행 추가
                if (d["PROC_SEQ"].ToString().Trim().Equals("1") || d["PROC_SEQ"].ToString().Trim().Equals("2"))
                {
                    for (int i = 0; i < 5; i++)
                    {
                        if (i.Equals(0))
                        {
                            // CH
                            sCol = d["PROCID"].ToString().Trim() + "_EQPT_INFO";
                            sColHeader = d["PROC_DESC"].ToString().Trim() + "|설비정보";
                            sBinding = sCol;
                        }
                        else if (i.Equals(1))
                        {
                            // CH
                            sCol = d["PROCID"].ToString().Trim() + "_CSTSLOT";
                            sColHeader = d["PROC_DESC"].ToString().Trim() + "|CHANNEL";
                            sBinding = sCol;
                        }
                        else if (i.Equals(2))
                        {
                            // JIG
                            sCol = d["PROCID"].ToString().Trim() + "_EQPTID";
                            sColHeader = d["PROC_DESC"].ToString().Trim() + "|BOX";
                            sBinding = sCol;
                        }
                        else if (i.Equals(3))
                        {
                            // 시작시간
                            sCol = d["PROCID"].ToString().Trim() + "_WRK_STRT_DTTM";
                            sColHeader = d["PROC_DESC"].ToString().Trim() + "|START_TIME";
                            sBinding = sCol;
                        }
                        else if (i.Equals(4))
                        {
                            // 종료시간
                            sCol = d["PROCID"].ToString().Trim() + "_WRK_END_DTTM";
                            sColHeader = d["PROC_DESC"].ToString().Trim() + "|END_TIME";
                            sBinding = sCol;
                        }
                        string[] sColName = sColHeader.Split('|');

                        //칼럼 헤더 자동 병합을 위해 Header로 사용할 List
                        List<string> Multi_Header = new List<string>();
                        Multi_Header = sColName.ToList();

                        var column_TEXT = CreateTextColumn(null, Multi_Header, sCol, sBinding, iWidth, bReadOnly: bReadOnly, bEditable: bEditable, bVisible: bVisible, bPercent: false);
                        dg.Columns.Add(column_TEXT);
                    }
                }
                else
                {
                }
            }
        }
        //20221019_충방전기, 대용량 방전기 추가 END



        private C1.WPF.DataGrid.DataGridTextColumn CreateTextColumn(string Single_Header
                                                                         , List<string> Multi_Header
                                                                         , string sName
                                                                         , string sBinding
                                                                         , int iWidth
                                                                         , bool bReadOnly = true
                                                                         , bool bEditable = false
                                                                         , bool bVisible = true
                                                                         , bool bPercent = false
                                                                         , HorizontalAlignment HorizonAlign = HorizontalAlignment.Center
                                                                         , VerticalAlignment VerticalAlign = VerticalAlignment.Center
                                                        )
        {

            C1.WPF.DataGrid.DataGridTextColumn Col = new C1.WPF.DataGrid.DataGridTextColumn();

            Col.Name = sName;
            Col.Binding = new Binding(sBinding);
            Col.IsReadOnly = bReadOnly;
            Col.EditOnSelection = bEditable;
            Col.Visibility = bVisible.Equals(true) ? Visibility.Visible : Visibility.Collapsed;
            Col.HorizontalAlignment = HorizonAlign;
            Col.VerticalAlignment = VerticalAlign;

            if (iWidth == 0)
                Col.Width = new C1.WPF.DataGrid.DataGridLength(1, DataGridUnitType.Auto);
            else
                Col.Width = new C1.WPF.DataGrid.DataGridLength(iWidth, DataGridUnitType.Pixel);

            if (bPercent)
                Col.Format = "P2";

            if (!string.IsNullOrEmpty(Single_Header))
            {
                Col.Header = Single_Header;
            }
            else
            {
                Col.Header = Multi_Header;
            }

            return Col;
        }

        #endregion

        #region Event
        private void btnCellReset_Click(object sender, RoutedEventArgs e)
        {
            Util.gridClear(dgCellList);
            if (Convert.ToInt32(txtRowCntCell.Value) > Convert.ToInt32(sRowCount))
            {
                //20221019_충방전기, 대용량 방전기 추가 START
                //Util.AlertInfo("FM_ME_0458");  //%1개 까지 조회 가능합니다.
                Util.MessageValidation("FM_ME_0458", new string[] { sRowCount });  //%1개 까지 조회 가능합니다.
                //20221019_충방전기, 대용량 방전기 추가 END
                return;
            }
            DataGridRowAdd(dgCellList, Convert.ToInt32(txtRowCntCell.Value));
        }

        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                ClearScreen();

                string sCellList = string.Empty;
                string sCellID = string.Empty;

                for (int i = 0; i < dgCellList.Rows.Count; i++)
                {
                    sCellID = Util.NVC(DataTableConverter.GetValue(dgCellList.Rows[i].DataItem, "SUBLOTID")).ToString();
                    if (!string.IsNullOrEmpty(sCellID))
                    {
                        sCellList = sCellList + sCellID + ",";
                    }
                }

                if (string.IsNullOrEmpty(sCellList))
                {
                    Util.AlertInfo("SFU8200");  //입력된 CELLID가 존재하지 않습니다.
                    return;
                }

                sCellList = sCellList.Remove(sCellList.LastIndexOf(","));

                if (tbcDefectHist.SelectedItem.Equals(tpHPCD))
                {
                    GetHPCD(sCellList);
                }
                else if (tbcDefectHist.SelectedItem.Equals(tpJigFormation))
                {
                    GetJG_FM_PG(dgJigFormation, sCellList, "J", "J7,J1"); //20221019_충방전기, 대용량 방전기 추가
                }
                else if (tbcDefectHist.SelectedItem.Equals(tpDegas))
                {
                    GetDegas(sCellList);
                }
                else if (tbcDefectHist.SelectedItem.Equals(tpEOL))
                {
                    GetEOL(sCellList);
                }
                else if (tbcDefectHist.SelectedItem.Equals(tpOCV))
                {
                    GetOcvSelector(dgOCV, sCellList, "8", "81");
                }
                else if (tbcDefectHist.SelectedItem.Equals(tpSelector))
                {
                    GetOcvSelector(dgSelector, sCellList, "6", "61");
                }
                //20221019_충방전기, 대용량 방전기 추가 START
                else if (tbcDefectHist.SelectedItem.Equals(tpFormation))
                {
                    GetJG_FM_PG(dgFormation, sCellList, "1", "11,12");
                }
                else if (tbcDefectHist.SelectedItem.Equals(tpPowerGrading))
                {
                    GetJG_FM_PG(dgPowerGrading, sCellList, "1", "17");
                }
                //20221019_충방전기, 대용량 방전기 추가 END
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void dgCellList_LoadedCellPresenter(object sender, DataGridCellEventArgs e)
        {
            try
            {
                C1.WPF.DataGrid.C1DataGrid dataGrid = sender as C1.WPF.DataGrid.C1DataGrid;

                dataGrid.Dispatcher.BeginInvoke(new Action(() =>
                {
                    if (e.Cell.Presenter == null)
                    {
                        return;
                    }

                    if (e.Cell.Row.Type == DataGridRowType.Item)
                    {
                        ////////////////////////////////////////////  default 색상 및 Cursor
                        e.Cell.Presenter.Cursor = Cursors.Arrow;

                        e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Black);
                        e.Cell.Presenter.FontSize = 12;
                        e.Cell.Presenter.FontWeight = FontWeights.Normal;
                        e.Cell.Presenter.Background = new SolidColorBrush(Colors.Transparent);
                        ///////////////////////////////////////////////////////////////////////////////////

                        if (e.Cell.Column.Name.ToString().Equals("SUBLOTID"))
                        {
                            e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Blue);
                        }
                    }
                }));
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void dgCellList_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            try
            {
                Point pnt = e.GetPosition(null);
                C1.WPF.DataGrid.DataGridCell cell = dgCellList.GetCellFromPoint(pnt);

                if (cell != null)
                {

                    if (!cell.Column.Name.Equals("SUBLOTID"))
                    {
                        return;
                    }

                    if (cell.Column.Name.Equals("SUBLOTID"))
                    {
                        string sCellID = Util.NVC(DataTableConverter.GetValue(dgCellList.Rows[cell.Row.Index].DataItem, "SUBLOTID")).ToString();
                        if (tbcDefectHist.SelectedItem.Equals(tpHPCD))
                        {
                            GetHPCD(sCellID);
                        }
                        else if (tbcDefectHist.SelectedItem.Equals(tpJigFormation))
                        {
                            GetJG_FM_PG(dgJigFormation, sCellID, "J", "J7,J1"); //20221019_충방전기, 대용량 방전기 추가
                        }
                        else if (tbcDefectHist.SelectedItem.Equals(tpDegas))
                        {
                            GetDegas(sCellID);
                        }
                        else if (tbcDefectHist.SelectedItem.Equals(tpEOL))
                        {
                            GetEOL(sCellID);
                        }
                        else if (tbcDefectHist.SelectedItem.Equals(tpOCV))
                        {
                            GetOcvSelector(dgOCV, sCellID, "8", "81");
                        }
                        else if (tbcDefectHist.SelectedItem.Equals(tpSelector))
                        {
                            GetOcvSelector(dgSelector, sCellID, "6", "61");
                        }
                        //20221019_충방전기, 대용량 방전기 추가 START
                        else if (tbcDefectHist.SelectedItem.Equals(tpFormation))
                        {
                            GetJG_FM_PG(dgFormation, sCellID, "1", "11,12");
                        }
                        else if (tbcDefectHist.SelectedItem.Equals(tpPowerGrading))
                        {
                            GetJG_FM_PG(dgPowerGrading, sCellID, "1", "17");
                        }
                        //20221019_충방전기, 대용량 방전기 추가 END
                    }
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return;
            }
            finally
            {
                loadingIndicator.Visibility = Visibility.Collapsed;
            }
        }

        //20221019_충방전기, 대용량 방전기 추가 START
        private void dgdgCellList_LoadedRowHeaderPresenter(object sender, C1.WPF.DataGrid.DataGridRowEventArgs e)
        {
            if (e.Row.HeaderPresenter == null)
            {
                return;
            }

            e.Row.HeaderPresenter.Content = null;
            C1DataGrid dataGrid = e.Row.DataGrid as C1DataGrid;
            TextBlock tb = new TextBlock();

            if (e.Row.Index + 1 - dataGrid.TopRows.Count > 0)
            {
                tb.Text = (e.Row.Index + 1 - dataGrid.TopRows.Count).ToString();
                tb.VerticalAlignment = VerticalAlignment.Center;
                tb.HorizontalAlignment = HorizontalAlignment.Center;
                e.Row.HeaderPresenter.Content = tb;
            }
        }
        //20221019_충방전기, 대용량 방전기 추가 END

        #endregion

        #region Mehod
        private void ClearScreen()
        {
            try
            {
                Util.gridClear(dgHPCD);
                Util.gridClear(dgDegas);
                Util.gridClear(dgJigFormation);
                Util.gridClear(dgEOL);
                Util.gridClear(dgOCV);
                Util.gridClear(dgSelector);
                Util.gridClear(dgFormation); //20221019_충방전기, 대용량 방전기 추가
                Util.gridClear(dgPowerGrading); //20221019_충방전기, 대용량 방전기 추가
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void GetHPCD(string sCellID)
        {
            try
            {
                Util.gridClear(dgHPCD);

                DataTable dtRqst = new DataTable();
                dtRqst.TableName = "RQSTDT";
                dtRqst.Columns.Add("SUBLOTID_LIST", typeof(string));
                dtRqst.Columns.Add("LANGID", typeof(string));
                dtRqst.Columns.Add("S26", typeof(string));
                dtRqst.Columns.Add("S27", typeof(string));

                DataRow dr = dtRqst.NewRow();
                dr["SUBLOTID_LIST"] = Util.NVC(sCellID);
                if (string.IsNullOrEmpty(dr["SUBLOTID_LIST"].ToString()))
                {
                    return;
                }
                dr["LANGID"] = LoginInfo.LANGID;
                dr["S26"] = "U";
                dr["S27"] = "U1";
                dtRqst.Rows.Add(dr);

                DataSet dsRqst = new DataSet();
                dsRqst.Tables.Add(dtRqst);

                ShowLoadingIndicator();
                new ClientProxy().ExecuteService("DA_SEL_DEFECT_CELL_HIST_MB", "RQSTDT", "RSLTDT", dtRqst, (bizResult, bizException) =>
                {
                    if (bizException != null)
                    {
                        HiddenLoadingIndicator();
                        Util.MessageException(bizException);
                        return;
                    }

                    if (bizResult.Rows.Count > 0)
                    {
                        Util.GridSetData(dgHPCD, bizResult, FrameOperation, true);
                    }
                    HiddenLoadingIndicator();
                });
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                HiddenLoadingIndicator();
            }
        }

        //20221019_충방전기, 대용량 방전기 추가 START
        private void GetJG_FM_PG(C1DataGrid dg, string sCellID, string s26, string s27)
        {
            try
            {
                Util.gridClear(dgJigFormation);

                DataTable dtRqst = new DataTable();
                dtRqst.TableName = "RQSTDT";
                dtRqst.Columns.Add("SUBLOTID_LIST", typeof(string));
                dtRqst.Columns.Add("LANGID", typeof(string));
                dtRqst.Columns.Add("S26", typeof(string));
                dtRqst.Columns.Add("S27", typeof(string));

                DataRow dr = dtRqst.NewRow();
                dr["SUBLOTID_LIST"] = Util.NVC(sCellID);
                if (string.IsNullOrEmpty(dr["SUBLOTID_LIST"].ToString()))
                {
                    return;
                }
                dr["LANGID"] = LoginInfo.LANGID;
                dr["S26"] = s26;
                dr["S27"] = s27;
                dtRqst.Rows.Add(dr);

                DataSet dsRqst = new DataSet();
                dsRqst.Tables.Add(dtRqst);

                ShowLoadingIndicator();
                new ClientProxy().ExecuteService("DA_SEL_DEFECT_CELL_HIST_MB", "RQSTDT", "RSLTDT", dtRqst, (bizResult, bizException) =>
                {
                    if (bizException != null)
                    {
                        HiddenLoadingIndicator();
                        Util.MessageException(bizException);
                        return;
                    }

                    if (bizResult.Rows.Count > 0)
                    {
                        //pivot 처리
                        //pivot 의 row 만들기용 distinct
                        DataTable dtPivot = new DataView(bizResult).ToTable(true, new string[] { "SUBLOTID", "LOTID" });

                        for (int i = dg.Columns["LOTID"].Index + 1; i < dg.Columns.Count; i++)
                        {
                            dtPivot.Columns.Add(dg.Columns[i].Name.ToString(), typeof(string));
                        }
                        //pk 지정
                        dtPivot.PrimaryKey = new DataColumn[] { dtPivot.Columns["SUBLOTID"], dtPivot.Columns["LOTID"] };

                        object[] oKeyVal = new object[2];

                        DataView dvPivotData = new DataView(bizResult);
                        dvPivotData.RowFilter = "SUBLOTID IS NOT NULL";

                        DataTable dtPivotData = dvPivotData.ToTable();

                        string sColName = string.Empty;
                        for (int i = 0; i < dtPivotData.Rows.Count; i++)
                        {
                            // Set the values of the keys to find.
                            oKeyVal[0] = dtPivotData.Rows[i]["SUBLOTID"].ToString();
                            oKeyVal[1] = dtPivotData.Rows[i]["LOTID"].ToString();

                            DataRow drPivot = dtPivot.Rows.Find(oKeyVal);

                            sColName = dtPivotData.Rows[i]["PROCID"].ToString() + "_EQPT_INFO";
                            if (dtPivot.Columns.Contains(sColName))
                            {
                                drPivot[sColName] = dtPivotData.Rows[i]["EQPT_INFO"];
                            }

                            sColName = dtPivotData.Rows[i]["PROCID"].ToString() + "_CSTSLOT";
                            if (dtPivot.Columns.Contains(sColName))
                            {
                                drPivot[sColName] = dtPivotData.Rows[i]["CSTSLOT"];
                            }

                            sColName = dtPivotData.Rows[i]["PROCID"].ToString() + "_EQPTID";
                            if (dtPivot.Columns.Contains(sColName))
                            {
                                drPivot[sColName] = dtPivotData.Rows[i]["EQPTID"];
                            }

                            sColName = dtPivotData.Rows[i]["PROCID"].ToString() + "_WRK_STRT_DTTM";
                            if (dtPivot.Columns.Contains(sColName))
                            {
                                drPivot[sColName] = dtPivotData.Rows[i]["WRK_STRT_DTTM"];
                            }

                            sColName = dtPivotData.Rows[i]["PROCID"].ToString() + "_WRK_END_DTTM";
                            if (dtPivot.Columns.Contains(sColName))
                            {
                                drPivot[sColName] = dtPivotData.Rows[i]["WRK_END_DTTM"];
                            }
                        }
                        dtPivot.Columns["SUBLOTID"].AllowDBNull = true;
                        dtPivot.Columns["LOTID"].AllowDBNull = true;

                        dtPivot.PrimaryKey = null;
                        dtPivot.Constraints.Clear();

                        Util.GridSetData(dg, dtPivot, FrameOperation, false);
                    }
                    HiddenLoadingIndicator();
                });
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                HiddenLoadingIndicator();
            }
        }
        //20221019_충방전기, 대용량 방전기 추가 END

        private void GetDegas(string sCellID)
        {
            try
            {
                Util.gridClear(dgDegas);

                DataTable dtRqst = new DataTable();
                dtRqst.TableName = "RQSTDT";
                dtRqst.Columns.Add("SUBLOTID_LIST", typeof(string));

                DataRow dr = dtRqst.NewRow();
                dr["SUBLOTID_LIST"] = Util.NVC(sCellID);
                if (string.IsNullOrEmpty(dr["SUBLOTID_LIST"].ToString()))
                {
                    return;
                }
                dtRqst.Rows.Add(dr);

                DataSet dsRqst = new DataSet();
                dsRqst.Tables.Add(dtRqst);

                ShowLoadingIndicator();
                new ClientProxy().ExecuteService("DA_SEL_DEFECT_CELL_DEGAS_HIST_MB", "RQSTDT", "RSLTDT", dtRqst, (bizResult, bizException) =>
                {
                    if (bizException != null)
                    {
                        HiddenLoadingIndicator();
                        Util.MessageException(bizException);
                        return;
                    }

                    if (bizResult.Rows.Count > 0)
                    {
                        Util.GridSetData(dgDegas, bizResult, FrameOperation, true);
                    }
                    HiddenLoadingIndicator();
                });
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                HiddenLoadingIndicator();
            }
        }

        private void GetEOL(string sCellID)
        {
            try
            {
                Util.gridClear(dgEOL);

                DataTable dtRqst = new DataTable();
                dtRqst.TableName = "RQSTDT";
                dtRqst.Columns.Add("SUBLOTID_LIST", typeof(string));

                DataRow dr = dtRqst.NewRow();
                dr["SUBLOTID_LIST"] = Util.NVC(sCellID);
                if (string.IsNullOrEmpty(dr["SUBLOTID_LIST"].ToString()))
                {
                    return;
                }
                dtRqst.Rows.Add(dr);

                DataSet dsRqst = new DataSet();
                dsRqst.Tables.Add(dtRqst);

                ShowLoadingIndicator();
                new ClientProxy().ExecuteService("DA_SEL_DEFECT_CELL_EOL_HIST_MB", "RQSTDT", "RSLTDT", dtRqst, (bizResult, bizException) =>
                {
                    if (bizException != null)
                    {
                        HiddenLoadingIndicator();
                        Util.MessageException(bizException);
                        return;
                    }

                    if (bizResult.Rows.Count > 0)
                    {
                        Util.GridSetData(dgEOL, bizResult, FrameOperation, true);
                    }
                    HiddenLoadingIndicator();
                });
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                HiddenLoadingIndicator();
            }
        }

        private void GetOcvSelector(C1DataGrid dg, string sCellID, string s26, string s27)
        {
            try
            {
                if (tbcDefectHist.SelectedItem.Equals(tpOCV))
                {
                    Util.gridClear(dgOCV);
                }
                else if (tbcDefectHist.SelectedItem.Equals(tpSelector))
                {
                    Util.gridClear(dgSelector);
                }

                DataTable dtRqst = new DataTable();
                dtRqst.TableName = "RQSTDT";
                dtRqst.Columns.Add("SUBLOTID_LIST", typeof(string));
                dtRqst.Columns.Add("LANGID", typeof(string));
                dtRqst.Columns.Add("S26", typeof(string));
                dtRqst.Columns.Add("S27", typeof(string));

                DataRow dr = dtRqst.NewRow();
                dr["SUBLOTID_LIST"] = Util.NVC(sCellID);
                if (string.IsNullOrEmpty(dr["SUBLOTID_LIST"].ToString()))
                {
                    return;
                }
                dr["LANGID"] = LoginInfo.LANGID;
                dr["S26"] = s26;
                dr["S27"] = s27;
                dtRqst.Rows.Add(dr);

                DataSet dsRqst = new DataSet();
                dsRqst.Tables.Add(dtRqst);

                ShowLoadingIndicator();
                new ClientProxy().ExecuteService("DA_SEL_DEFECT_CELL_HIST_MB", "RQSTDT", "RSLTDT", dtRqst, (bizResult, bizException) =>
                {
                    if (bizException != null)
                    {
                        HiddenLoadingIndicator();
                        Util.MessageException(bizException);
                        return;
                    }

                    if (bizResult.Rows.Count > 0)
                    {
                        //pivot 처리
                        //pivot 의 row 만들기용 distinct
                        DataTable dtPivot = new DataView(bizResult).ToTable(true, new string[] { "SUBLOTID", "LOTID" });

                        for (int i = dg.Columns["LOTID"].Index + 1; i < dg.Columns.Count; i++)
                        {
                            dtPivot.Columns.Add(dg.Columns[i].Name.ToString(), typeof(string));
                        }
                        //pk 지정
                        dtPivot.PrimaryKey = new DataColumn[] { dtPivot.Columns["SUBLOTID"], dtPivot.Columns["LOTID"] };

                        object[] oKeyVal = new object[2];

                        DataView dvPivotData = new DataView(bizResult);
                        dvPivotData.RowFilter = "SUBLOTID IS NOT NULL";

                        DataTable dtPivotData = dvPivotData.ToTable();

                        string sColName = string.Empty;
                        for (int i = 0; i < dtPivotData.Rows.Count; i++)
                        {
                            // Set the values of the keys to find.
                            oKeyVal[0] = dtPivotData.Rows[i]["SUBLOTID"].ToString();
                            oKeyVal[1] = dtPivotData.Rows[i]["LOTID"].ToString();

                            DataRow drPivot = dtPivot.Rows.Find(oKeyVal);

                            sColName = dtPivotData.Rows[i]["PROCID"].ToString() + "_EQPTID";
                            if (dtPivot.Columns.Contains(sColName))
                            {
                                drPivot[sColName] = dtPivotData.Rows[i]["EQPTID"];
                            }

                            sColName = dtPivotData.Rows[i]["PROCID"].ToString() + "_CSTID";
                            if (dtPivot.Columns.Contains(sColName))
                            {
                                drPivot[sColName] = dtPivotData.Rows[i]["CSTID"];
                            }

                            sColName = dtPivotData.Rows[i]["PROCID"].ToString() + "_CSTSLOT";
                            if (dtPivot.Columns.Contains(sColName))
                            {
                                drPivot[sColName] = dtPivotData.Rows[i]["CSTSLOT"];
                            }

                            sColName = dtPivotData.Rows[i]["PROCID"].ToString() + "_WRK_STRT_DTTM";
                            if (dtPivot.Columns.Contains(sColName))
                            {
                                drPivot[sColName] = dtPivotData.Rows[i]["WRK_STRT_DTTM"];
                            }

                            sColName = dtPivotData.Rows[i]["PROCID"].ToString() + "_WRK_END_DTTM";
                            if (dtPivot.Columns.Contains(sColName))
                            {
                                drPivot[sColName] = dtPivotData.Rows[i]["WRK_END_DTTM"];
                            }
                        }
                        dtPivot.Columns["SUBLOTID"].AllowDBNull = true;
                        dtPivot.Columns["LOTID"].AllowDBNull = true;

                        dtPivot.PrimaryKey = null;
                        dtPivot.Constraints.Clear();

                        Util.GridSetData(dg, dtPivot, FrameOperation, false);
                    }
                    HiddenLoadingIndicator();
                });
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                HiddenLoadingIndicator();
            }
        }

        private void GetCellList(string LotID)
        {
            try
            {
                Util.gridClear(dgCellList);

                const string bizRuleName = "DA_BAS_SEL_SUBLOT_BY_LOTID";
                DataTable inDataTable = new DataTable("RQSTDT");
                inDataTable.Columns.Add("LOTID", typeof(string));

                DataRow dr = inDataTable.NewRow();
                dr["LOTID"] = Util.NVC(LotID);
                inDataTable.Rows.Add(dr);

                ShowLoadingIndicator();

                new ClientProxy().ExecuteService(bizRuleName, "RQSTDT", "RSLTDT", inDataTable, (bizResult, bizException) =>
                {
                    HiddenLoadingIndicator();
                    if (bizException != null)
                    {
                        Util.MessageException(bizException);
                        return;
                    }
                    if (bizResult.Rows.Count > 0)
                    {
                        Util.GridSetData(dgCellList, bizResult, FrameOperation, false);
                    }
                    else
                    {
                        Util.GridSetData(dgCellList, bizResult, FrameOperation, false);
                    }

                    HiddenLoadingIndicator();
                });

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

        private void DataGridRowAdd(C1.WPF.DataGrid.C1DataGrid dg, int iRowCount)
        {
            try
            {
                DataTable dt = new DataTable();

                dt = DataTableConverter.Convert(dg.ItemsSource);

                if (dg.ItemsSource != null)
                {
                    for (int i = 0; i < iRowCount; i++)
                    {
                        DataRow dr2 = dt.NewRow();
                        dt.Rows.Add(dr2);
                        dg.BeginEdit();
                        dg.EndEdit();
                    }
                    dg.ItemsSource = DataTableConverter.Convert(dt);
                }
                else
                {
                    foreach (C1.WPF.DataGrid.DataGridColumn col in dg.Columns)
                    {
                        dt.Columns.Add(Convert.ToString(col.Name));
                    }

                    for (int i = 0; i < iRowCount; i++)
                    {
                        DataRow dr = dt.NewRow();
                        dt.Rows.Add(dr);
                        dg.BeginEdit();
                        dg.EndEdit();
                    }
                    dg.ItemsSource = DataTableConverter.Convert(dt);
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

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

        //20221019_충방전기, 대용량 방전기 추가 START
        private string GetCommonCode(string sComTypeCode, string sComCode)
        {
            string sValue = string.Empty;

            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("CMCDTYPE", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["CMCDTYPE"] = sComTypeCode;
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_COMMCODE_ALL_CBO", "RQSTDT", "RSLTDT", RQSTDT);

                foreach (DataRow row in dtResult.Rows)
                {
                    if (row["CBO_CODE"].ToString().Equals(sComCode))
                    {
                        sValue = row["ATTR1"].ToString();
                    }
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }

            return sValue;
        }
        //20221019_충방전기, 대용량 방전기 추가 END

        #endregion

        private void GetTestData(ref DataTable dt)
        {
            dt.TableName = "RQSTDT";
            dt.Columns.Add("EQPT_ID", typeof(string));
            dt.Columns.Add("PORT_ID", typeof(string));
            dt.Columns.Add("PORT_NAME", typeof(string));
            dt.Columns.Add("TRAY_ID", typeof(string));
            dt.Columns.Add("LOT_ID", typeof(string));
            dt.Columns.Add("WIP_QTY", typeof(string));
            dt.Columns.Add("ROW_VALUE", typeof(int));
            dt.Columns.Add("COLUMN_VALUE", typeof(int));
            dt.Columns.Add("H_VALUE", typeof(int));
            dt.Columns.Add("W_VALUE", typeof(int));
            dt.Columns.Add("L_VALUE", typeof(int));
            dt.Columns.Add("T_VALUE", typeof(int));
            dt.Columns.Add("R_VALUE", typeof(int));
            dt.Columns.Add("B_VALUE", typeof(int));


            #region ROW ADD
            DataRow row1 = dt.NewRow();
            row1["EQPT_ID"] = "EQPTID_1";
            row1["PORT_ID"] = "PORT ID 1";
            row1["PORT_NAME"] = "PORT DESC 1";
            row1["TRAY_ID"] = "TRAY ID 1";
            row1["LOT_ID"] = "LOT ID 1";
            row1["WIP_QTY"] = "5";
            row1["ROW_VALUE"] = "1";
            row1["COLUMN_VALUE"] = "1";
            row1["H_VALUE"] = "300";
            row1["W_VALUE"] = "500";
            row1["L_VALUE"] = "0";
            row1["T_VALUE"] = "0";
            row1["R_VALUE"] = "0";
            row1["B_VALUE"] = "0";
            dt.Rows.Add(row1);
            DataRow row2 = dt.NewRow();
            row2["EQPT_ID"] = "EQPTID_2";
            row2["PORT_ID"] = "PORT ID 2";
            row2["PORT_NAME"] = "PORT DESC 2";
            row2["TRAY_ID"] = "TRAY ID 2";
            row2["LOT_ID"] = "LOT ID 2";
            row2["WIP_QTY"] = "15";
            row2["ROW_VALUE"] = "1";
            row2["COLUMN_VALUE"] = "2";
            row2["H_VALUE"] = "300";
            row2["W_VALUE"] = "500";
            row2["L_VALUE"] = "0";
            row2["T_VALUE"] = "0";
            row2["R_VALUE"] = "0";
            row2["B_VALUE"] = "0";
            dt.Rows.Add(row2);
            DataRow row3 = dt.NewRow();
            row3["EQPT_ID"] = "EQPTID_3";
            row3["PORT_ID"] = "PORT ID 3";
            row3["PORT_NAME"] = "PORT DESC 3";
            row3["TRAY_ID"] = "TRAY ID 3";
            row3["LOT_ID"] = "LOT ID 3";
            row3["WIP_QTY"] = "20";
            row3["ROW_VALUE"] = "1";
            row3["COLUMN_VALUE"] = "3";
            row3["H_VALUE"] = "300";
            row3["W_VALUE"] = "500";
            row3["L_VALUE"] = "0";
            row3["T_VALUE"] = "0";
            row3["R_VALUE"] = "0";
            row3["B_VALUE"] = "0";
            dt.Rows.Add(row3);
            DataRow row4 = dt.NewRow();
            row4["EQPT_ID"] = "EQP";
            row4["PORT_ID"] = "";
            row4["PORT_NAME"] = "";
            row4["TRAY_ID"] = "";
            row4["LOT_ID"] = "LOT ID 4";
            row4["WIP_QTY"] = "25";
            row4["ROW_VALUE"] = "2";
            row4["COLUMN_VALUE"] = "1";
            row4["H_VALUE"] = "100";
            row4["W_VALUE"] = "1500";
            row4["L_VALUE"] = "0";
            row4["T_VALUE"] = "0";
            row4["R_VALUE"] = "0";
            row4["B_VALUE"] = "0";
            dt.Rows.Add(row4);
            DataRow row5 = dt.NewRow();
            row5["EQPT_ID"] = "EQPTID_4";
            row5["PORT_ID"] = "PORT ID 4";
            row5["PORT_NAME"] = "PORT DESC 4";
            row5["TRAY_ID"] = "TRAY ID 4";
            row5["LOT_ID"] = "LOT ID 5";
            row5["WIP_QTY"] = "30";
            row5["ROW_VALUE"] = "3";
            row5["COLUMN_VALUE"] = "1";
            row5["H_VALUE"] = "300";
            row5["W_VALUE"] = "500";
            row5["L_VALUE"] = "0";
            row5["T_VALUE"] = "0";
            row5["R_VALUE"] = "0";
            row5["B_VALUE"] = "0";
            dt.Rows.Add(row5);
            DataRow row6 = dt.NewRow();
            row6["EQPT_ID"] = "EQPTID_5";
            row6["PORT_ID"] = "PORT ID 5";
            row6["PORT_NAME"] = "PORT DESC 5";
            row6["TRAY_ID"] = "TRAY ID 5";
            row6["LOT_ID"] = "LOT ID 5";
            row6["WIP_QTY"] = "30";
            row6["ROW_VALUE"] = "3";
            row6["COLUMN_VALUE"] = "2";
            row6["H_VALUE"] = "300";
            row6["W_VALUE"] = "500";
            row6["L_VALUE"] = "0";
            row6["T_VALUE"] = "0";
            row6["R_VALUE"] = "0";
            row6["B_VALUE"] = "0";
            dt.Rows.Add(row6);
            DataRow row7 = dt.NewRow();
            row7["EQPT_ID"] = "EQPTID_6";
            row7["PORT_ID"] = "PORT ID 6";
            row7["PORT_NAME"] = "PORT DESC 6";
            row7["TRAY_ID"] = "TRAY ID 6";
            row7["LOT_ID"] = "LOT ID 6";
            row7["WIP_QTY"] = "35";
            row7["ROW_VALUE"] = "3";
            row7["COLUMN_VALUE"] = "3";
            row7["H_VALUE"] = "300";
            row7["W_VALUE"] = "500";
            row7["L_VALUE"] = "0";
            row7["T_VALUE"] = "0";
            row7["R_VALUE"] = "0";
            row7["B_VALUE"] = "0";
            dt.Rows.Add(row7);

            #endregion

        }

    }
}