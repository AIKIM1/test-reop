/*************************************************************************************
 Created Date : 2016.06.16
      Creator : 
   Decription : 
--------------------------------------------------------------------------------------
 [Change History]
  2016.06.16  DEVELOPER : Initial Created.





 
**************************************************************************************/

using C1.WPF;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using System;
using System.Data;
using System.Windows;
using System.Windows.Controls;


namespace LGC.GMES.MES.BOX001
{
    public partial class BOX001_007_TMP_CELL : C1Window, IWorkArea
    {
        #region Declaration & Constructor 

        private Util _Util = new Util();
        CommonCombo _combo = new CMM001.Class.CommonCombo();

        private string sSHOPID = string.Empty;
        private string sAREAID = string.Empty;
        private string sLINEID = string.Empty;
        private string sMDLLOTID = string.Empty;
        private string sUSERID = string.Empty;

        // 팝업 호출한 폼으로 리턴함.
        private string _RetJOBDATE = string.Empty;
        private string _RetSAVE_SEQ = string.Empty;

        public string retJOBDATE
        {
            get { return _RetJOBDATE; }
        }

        public string retSAVE_SEQ
        {
            get { return _RetSAVE_SEQ; }
        }

        /// <summary>
        ///  Frame과 상호작용하기 위한 객체
        /// </summary>
        public IFrameOperation FrameOperation
        {
            get;
            set;
        }


        public BOX001_007_TMP_CELL()
        {
            InitializeComponent();
            Loaded += BOX001_007_TMP_CELL_Loaded;

        }

        private void BOX001_007_TMP_CELL_Loaded(object sender, RoutedEventArgs e)
        {
            Loaded -= BOX001_007_TMP_CELL_Loaded;

            object[] tmps = C1WindowExtension.GetParameters(this);
            sLINEID = tmps[0] as string;
            sMDLLOTID = tmps[1] as string;
           // sUSERID = tmps[2] as string;
            sSHOPID = tmps[3] as string;
            sAREAID = tmps[4] as string;

            InitSet();
           
        }


        #endregion

        #region Initialize

        private void InitSet()
        {

            String[] sFilter = { sAREAID };    // Area
            //C1ComboBox[] cboEquipmentChild = { cboProduct };
            C1ComboBox[] cboChild = { cboModelLot };
            _combo.SetCombo(cboEquipmentSegment, CommonCombo.ComboStatus.SELECT, cbChild:cboChild, sFilter: sFilter, sCase: "LINE_CP");

            C1ComboBox[] cboParent = { cboEquipmentSegment };
            _combo.SetCombo(cboModelLot, CommonCombo.ComboStatus.SELECT, cbParent: cboParent);

            //작업자 Combo Set.
           // String[] sFilter4 = { sSHOPID, sAREAID, Process.CELL_BOXING };
          //  _combo.SetCombo(cboProcUser, CommonCombo.ComboStatus.ALL, sFilter: sFilter4, sCase: "PROC_USER");

            dtpDateFrom.Text = DateTime.Now.AddDays(-1).ToString("yyyy-MM-dd");
            dtpDateTo.Text = DateTime.Now.ToString("yyyy-MM-dd");

            if (!string.IsNullOrEmpty(sLINEID))
                cboEquipmentSegment.SelectedValue = sLINEID;
            if (!string.IsNullOrEmpty(sMDLLOTID))
                cboModelLot.SelectedValue = sMDLLOTID;

        }

        #endregion

        #region Event


        private void dgListChoice_Checked(object sender, RoutedEventArgs e)
        {
            RadioButton rb = sender as RadioButton;

            if (rb.DataContext == null)
                return;

            if ((bool)rb.IsChecked && (rb.DataContext as DataRowView).Row["CHK"].ToString().Equals("0"))
            {

                //selLotData = ((C1.WPF.DataGrid.DataGridCellPresenter)rb.Parent).Row.DataItem;
                int idx = ((C1.WPF.DataGrid.DataGridCellPresenter)rb.Parent).Row.Index;
                DataRow dtRow = (rb.DataContext as DataRowView).Row;

                C1.WPF.DataGrid.C1DataGrid dg = ((C1.WPF.DataGrid.DataGridCellPresenter)rb.Parent).DataGrid;
                for (int i = 0; i < dg.GetRowCount(); i++)
                {
                    if (idx == i)
                        DataTableConverter.SetValue(dg.Rows[i].DataItem, ((C1.WPF.DataGrid.DataGridCellPresenter)rb.Parent).Column.Name, true);
                    else
                        DataTableConverter.SetValue(dg.Rows[i].DataItem, ((C1.WPF.DataGrid.DataGridCellPresenter)rb.Parent).Column.Name, false);
                }
            }
        }
        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {

            // 기본적인 Validation
            string sLine = Util.NVC(cboEquipmentSegment.SelectedValue);
            if (sLine == string.Empty || sLine == "SELECT")
            {
                //"라인을 선택해주십시오." >> 라인을 선택해 주세요.
                Util.MessageValidation("SFU1223");
               // LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("SFU1223"), null, "Information", MessageBoxButton.OK, MessageBoxIcon.None);
                return;
            }

            string sModel = Util.NVC(cboModelLot.SelectedValue);
            if (sModel == string.Empty || sModel == "SELECT")
            {
                //모델을 선택해주십시오.
                Util.MessageValidation("SFU1257");
                //LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("SFU1257"), null, "Information", MessageBoxButton.OK, MessageBoxIcon.None);
                return;
            }


            string sUser = txtWorker.Text;
            //if (sUser == string.Empty)
            //{
            //    sUser = null;
            //}

            if (string.IsNullOrWhiteSpace(sUser))
            {
                ////작업자를 선택해 주세요
                Util.MessageValidation("SFU1843");
                //LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("SFU1843"), null, "Error", MessageBoxButton.OK, ControlsLibrary.MessageBoxIcon.None);
                //return;
                sUser = null;
            }


            // BizData data = new BizData("QR_TEMP_SAVE_CELL_LIST", "RSLTDT");
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("AREAID", typeof(string));
                RQSTDT.Columns.Add("PACK_TMP_TYPE_CODE", typeof(string));
                RQSTDT.Columns.Add("FROMDATE", typeof(DateTime));
                RQSTDT.Columns.Add("TODATE", typeof(DateTime));
                RQSTDT.Columns.Add("EQSGID", typeof(string));
                RQSTDT.Columns.Add("MDLLOT_ID", typeof(string));
                RQSTDT.Columns.Add("USERID", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["AREAID"] = sAREAID;
                dr["PACK_TMP_TYPE_CODE"] = "PACK_CELL";
                dr["FROMDATE"] = dtpDateFrom.SelectedDateTime.ToString("yyyy-MM-dd") + " 00:00:00"; //Convert.ToDateTime(dtpDateFrom.Text).ToString("yyyy-MM-dd") + " 00:00:00";
                dr["TODATE"] = dtpDateTo.SelectedDateTime.ToString("yyyy-MM-dd") + " 23:59:59"; //Convert.ToDateTime(dtpDateTo.Text).ToString("yyyy-MM-dd") + " 23:59:59";
                dr["EQSGID"] = Util.NVC(cboEquipmentSegment.SelectedValue);
                dr["MDLLOT_ID"] = Util.NVC(cboModelLot.SelectedValue);
                dr["USERID"] = sUser;
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_TB_SFC_CELL_PACK_TMP_SAVE", "RQSTDT", "RSLTDT", RQSTDT);
                ////dgList.ItemsSource = DataTableConverter.Convert(dtResult);
                Util.GridSetData(dgList, dtResult, FrameOperation);
                // 스프레드의 Row만큼 반복하면서 '번호' 입력
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
               // LGC.GMES.MES.ControlsLibrary.MessageBox.Show(ex.Message, null, "Error", MessageBoxButton.OK, ControlsLibrary.MessageBoxIcon.None);
            }

        }

        private void btnSelect_Click(object sender, RoutedEventArgs e)
        {
            int iRow = -1;

            DataTable DT = DataTableConverter.Convert(dgList.ItemsSource);
            for (int i = 0; i < DT.Rows.Count; i++)
            {
                if (Util.NVC(DT.Rows[i]["CHK"]) == "1")
                {
                    iRow = i;
                    break;
                }
            }

            if (iRow == -1)
            {
                Util.MessageValidation("SFU1629"); //"선택 후 작업하세요"
                return;
            }
            else
            {
                _RetJOBDATE = Util.NVC(DT.Rows[iRow]["INSDTTM"]);
                _RetSAVE_SEQ = Util.NVC(DT.Rows[iRow]["SAVE_SEQNO"]);

                this.DialogResult = MessageBoxResult.OK;
                this.Close();
            }
        }


        private void btnTagPring_Click(object sender, RoutedEventArgs e)
        {
            int iRow = -1;
            DataTable DT = DataTableConverter.Convert(dgList.ItemsSource);
            for (int i = 0; i < DT.Rows.Count; i++)
            {
                if (Util.NVC(DT.Rows[i]["CHK"]) == "1")
                {
                    iRow = i;
                    break;
                }
            }

            if (iRow == -1)
            {
                Util.MessageValidation("SFU1629"); //"선택 후 작업하세요"
                return;
            }
            else
            {

                //Tag Printing...
                DataTable dtTempCellTag = setTempCellTag(iRow);

                // 태그 발행 창 화면에 띄움.
                object[] Parameters = new object[2];
                if (LoginInfo.LANGID == "ko-KR")
                    Parameters[0] = "TempCell_Tag"; // "PalletHis_Tag";
                else
                    Parameters[0] = "TempCell_Tag_en-US"; // "PalletHis_Tag";
                Parameters[1] = dtTempCellTag;

                LGC.GMES.MES.BOX001.Report rs = new LGC.GMES.MES.BOX001.Report();
                C1WindowExtension.SetParameters(rs, Parameters);
                rs.Show();
            }
        }

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = MessageBoxResult.Cancel;
            this.Close();
        }
        
        #endregion


        #region Mehod


        private DataTable setTempCellTag(int iRow)
        {
            try
            {
                DataTable dtTempTag = new DataTable();
                dtTempTag.Columns.Add("REG_DATE", typeof(string));
                dtTempTag.Columns.Add("REG_USER", typeof(string));
                dtTempTag.Columns.Add("LINEID", typeof(string));
                dtTempTag.Columns.Add("PRODID", typeof(string));
                dtTempTag.Columns.Add("PROJECTNAME", typeof(string));
                dtTempTag.Columns.Add("PACK_OUTGONAME", typeof(string));
                dtTempTag.Columns.Add("CELLQTY", typeof(string));
                dtTempTag.Columns.Add("LOT_BARCODE", typeof(string));
                dtTempTag.Columns.Add("LOT_TEXT", typeof(string));

                DataRow dr = dtTempTag.NewRow();
                dr["REG_DATE"] = Util.NVC(dgList.GetCell(iRow, dgList.Columns["INSDTTM"].Index).Value);
                dr["REG_USER"] = Util.NVC(dgList.GetCell(iRow, dgList.Columns["USERNAME"].Index).Value);
                dr["LINEID"] = Util.NVC(dgList.GetCell(iRow, dgList.Columns["EQSGNAME"].Index).Value);
                dr["PRODID"] = Util.NVC(dgList.GetCell(iRow, dgList.Columns["PRODID"].Index).Value);
                dr["PROJECTNAME"] = Util.NVC(dgList.GetCell(iRow, dgList.Columns["PROJECTNAME"].Index).Value);
                dr["PACK_OUTGONAME"] = Util.NVC(dgList.GetCell(iRow, dgList.Columns["SHIPTO_NAME"].Index).Value);
                dr["CELLQTY"] = Util.NVC(dgList.GetCell(iRow, dgList.Columns["SCAN_QTY"].Index).Value);

                string sJobDate = Convert.ToDateTime(dgList.GetCell(iRow, dgList.Columns["INSDTTM"].Index).Value).ToString("yyyyMMdd");
                string sLotText = sJobDate + Util.NVC(dgList.GetCell(iRow, dgList.Columns["SAVE_SEQNO"].Index).Value);
                dr["LOT_BARCODE"] = "ZZ" + sLotText;
                dr["LOT_TEXT"] = sLotText;

                dtTempTag.Rows.Add(dr);
                return dtTempTag;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return null;
            }
        }


        #endregion


    }
}
