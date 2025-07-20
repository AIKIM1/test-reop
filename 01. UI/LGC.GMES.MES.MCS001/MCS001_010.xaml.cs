/*************************************************************************************
 Created Date : 2018.10.18
      Creator : 오화백
   Decription : PORT 기준정보관리
--------------------------------------------------------------------------------------
 [Change History]
  2018.10.18  DEVELOPER : Initial Created.


 
**************************************************************************************/

using C1.WPF;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using System;
using System.Data;
using System.Windows;
using System.Windows.Controls;

namespace LGC.GMES.MES.MCS001
{
    public partial class MCS001_010 : UserControl, IWorkArea
    {
        #region Declaration & Constructor 
        DataTable dtMain = new DataTable();
        Util _Util = new Util();


        public MCS001_010()
        {
            InitializeComponent();
            InitCombo();
        }
        public IFrameOperation FrameOperation
        {
            get;
            set;
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {

            if (this.FrameOperation.Parameters != null && this.FrameOperation.Parameters.Length > 0)
            {
                GetResult();
            }
           

            this.Loaded -= UserControl_Loaded;
        }
        #endregion


        #region Initialize
        private void InitCombo()
        {
            try
            {
                CommonCombo _combo = new CommonCombo();

                //PORT 모드
                String[] sFilter1 = { "MCS_PORT_TYPE_CODE" };
                _combo.SetCombo(cboPortMode, CommonCombo.ComboStatus.ALL, sFilter: sFilter1, sCase: "COMMCODE");

                //설비타입
                String[] sFilter2 = { "LOGIS_EQPT_DETL_TYPE" };
                C1ComboBox[] cboAreaChild = { cboEqpName };
                _combo.SetCombo(cboEqpType, CommonCombo.ComboStatus.ALL, cbChild: cboAreaChild, sFilter: sFilter2, sCase: "COMMCODE");

                //설비명
                C1ComboBox[] cboEquipmentTypeParent = { cboEqpType };
                _combo.SetCombo(cboEqpName, CommonCombo.ComboStatus.ALL, cbParent: cboEquipmentTypeParent, sCase: "CWAMCSEQUIPMENT");


                //전극유형
                String[] sFilter3 = { "ELTR_TYPE_CODE" };
                _combo.SetCombo(cboElecType, CommonCombo.ComboStatus.ALL, sFilter: sFilter3, sCase: "COMMCODE");

                //사용여부
                String[] sFilter4 = { "IUSE" };
                _combo.SetCombo(cboUseYN, CommonCombo.ComboStatus.ALL, sFilter: sFilter4, sCase: "COMMCODE");
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        #endregion

        #region Event
     
        #region 조회 : btnSearch()
        /// <summary>
        /// 조회 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnSearch(object sender, RoutedEventArgs e)
        {
            GetResult();
        }
        #endregion

        #region 수정 : btnMod_Click()
        /// <summary>
        ///  수정
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnMod_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!Validation_Modify())
                {
                    return;
                }
                //int idxchk = _Util.GetDataGridCheckFirstRowIndex(dgDefectList, "CHK");
                //CMM_POLYMER_CELL_INPUT popupCell = new CMM_POLYMER_CELL_INPUT();

                //popupCell.CTNR_DEFC_LOT_CHK = "N";  //대차ID/불량그룹 체크
                //popupCell.FrameOperation = this.FrameOperation;

                //object[] parameters = new object[1];
                //parameters[0] = Util.NVC(dgDefectList.GetCell(idxchk, dgDefectList.Columns["LOTID"].Index).Value);
                //C1WindowExtension.SetParameters(popupCell, parameters);
                //popupCell.Closed += new EventHandler(popupCell_Closed);
                //foreach (Grid tmp in Util.FindVisualChildren<Grid>(Application.Current.MainWindow))
                //{
                //    if (tmp.Name == "grdMain")
                //    {
                //        tmp.Children.Add(popupCell);
                //        popupCell.BringToFront();
                //        break;
                //    }
                //}

            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }
        #endregion

        #region 삭제 : btnDel_Click()
        /// <summary>
        /// 삭제
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>

        private void btnDel_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!Validation_Delete())
                {
                    return;
                }
                //int idxchk = _Util.GetDataGridCheckFirstRowIndex(dgDefectList, "CHK");
                //CMM_POLYMER_CELL_INPUT popupCell = new CMM_POLYMER_CELL_INPUT();

                //popupCell.CTNR_DEFC_LOT_CHK = "N";  //대차ID/불량그룹 체크
                //popupCell.FrameOperation = this.FrameOperation;

                //object[] parameters = new object[1];
                //parameters[0] = Util.NVC(dgDefectList.GetCell(idxchk, dgDefectList.Columns["LOTID"].Index).Value);
                //C1WindowExtension.SetParameters(popupCell, parameters);
                //popupCell.Closed += new EventHandler(popupCell_Closed);
                //foreach (Grid tmp in Util.FindVisualChildren<Grid>(Application.Current.MainWindow))
                //{
                //    if (tmp.Name == "grdMain")
                //    {
                //        tmp.Children.Add(popupCell);
                //        popupCell.BringToFront();
                //        break;
                //    }
                //}

            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }
        #endregion

        #endregion

        #region Mehod 

        #region 조회 : GetResult()

        /// <summary>
        /// 조회
        /// </summary>
        private void GetResult()
        {
            string sPortID = "";// cboPortName.SelectedValue.ToString();
            string sPortMode = cboPortMode.SelectedValue.ToString();
            string sEquip = "";// cboEqpName.SelectedValue.ToString();
            string sUseYn = cboUseYN.SelectedValue.ToString();
            string sElecType = cboElecType.SelectedValue.ToString();

            if (sPortID == "")
            {
                sPortID = null;
            }
            if (sPortMode == "")
            {
                sPortMode = null;
            }
            if (sEquip == "")
            {
                sEquip = null;
            }
            if (sUseYn == "")
            {
                sUseYn = null;
            }
            if (sElecType == "")
            {
                sElecType = null;
            }

            DataTable inTable = new DataTable();
            inTable.Columns.Add("PORT_ID", typeof(string));
            inTable.Columns.Add("PORT_MODE", typeof(string));
            inTable.Columns.Add("EQPTID", typeof(string));
            inTable.Columns.Add("USE_FLAG", typeof(string));
            inTable.Columns.Add("ELTR_TYPE_CODE", typeof(string));
            inTable.Columns.Add("LANGID", typeof(string));

            DataRow newRow = inTable.NewRow();

            newRow["PORT_ID"] = sPortID;
            newRow["PORT_MODE"] = sPortMode;
            newRow["EQPTID"] = sEquip;
            newRow["USE_FLAG"] = sUseYn;
            newRow["ELTR_TYPE_CODE"] = sElecType;
            newRow["LANGID"] = LoginInfo.LANGID;
            inTable.Rows.Add(newRow);

            dtMain = new ClientProxy().ExecuteServiceSync("DA_MCS_SEL_PORT_MMD", "INDATA", "OUTDATA", inTable);
            Util.GridSetData(dgLotList, dtMain, FrameOperation, true);

        }

        #endregion

        #region  Validation : Validation_Modify(), Validation_Delete()
        /// <summary>
        /// 수정 Validation
        /// </summary>
        /// <returns></returns>
        private bool Validation_Modify()
        {


            if (dgLotList.Rows.Count == 0)
            {
                Util.MessageValidation("SFU3537"); // 조회된 데이터가 없습니다
                return false;
            }

            DataRow[] drSelect = DataTableConverter.Convert(dgLotList.ItemsSource).Select("CHK = '1'");


            int CheckCount = 0;

            for (int i = 0; i < dgLotList.Rows.Count; i++)
            {
                if (Util.NVC(DataTableConverter.GetValue(dgLotList.Rows[i].DataItem, "CHK")) == "1")
                {
                    CheckCount = CheckCount + 1;
                }
            }

            if (CheckCount == 0)
            {
                Util.MessageValidation("SFU1651");//선택된 항목이 없습니다.
                return false;
            }
            if (CheckCount > 1)
            {
                Util.MessageValidation("SFU4159");//한건만 선택하세요.
                return false;
            }


            return true;
        }

        /// <summary>
        /// 삭제 Validation
        /// </summary>
        /// <returns></returns>
        private bool Validation_Delete()
        {


            if (dgLotList.Rows.Count == 0)
            {
                Util.MessageValidation("SFU3537"); // 조회된 데이터가 없습니다
                return false;
            }

            DataRow[] drSelect = DataTableConverter.Convert(dgLotList.ItemsSource).Select("CHK = '1'");
            int CheckCount = 0;

            for (int i = 0; i < dgLotList.Rows.Count; i++)
            {
                if (Util.NVC(DataTableConverter.GetValue(dgLotList.Rows[i].DataItem, "CHK")) == "1")
                {
                    CheckCount = CheckCount + 1;
                }
            }
            if (CheckCount == 0)
            {
                Util.MessageValidation("SFU1651");//선택된 항목이 없습니다.
                return false;
            }



            return true;
        }

        #endregion

        #endregion




     
    }
}
