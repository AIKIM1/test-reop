/*************************************************************************************
 Created Date : 2018.10.17
      Creator : 오화백
   Decription : 반송 우선순위 관리
--------------------------------------------------------------------------------------
 [Change History]
  2018.10.17  DEVELOPER : Initial Created.


 
**************************************************************************************/

using C1.WPF;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using System;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using C1.WPF.DataGrid;



namespace LGC.GMES.MES.MCS001
{
    public partial class MCS001_009 : UserControl, IWorkArea
    {
        #region Declaration & Constructor 
        DataTable dtMain = new DataTable();
        Util _Util = new Util();


        public MCS001_009()
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
        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {

            if (this.FrameOperation.Parameters != null && this.FrameOperation.Parameters.Length > 0)
            {
                Array ary = FrameOperation.Parameters;
                txtLot.Text = ary.GetValue(0).ToString();
                GetResult();
            }


            this.Loaded -= UserControl_Loaded;
        }
        #endregion

        #region Event

        #region 저장 : btnSave_Click()
        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            dgReturnList.EndEdit();
            dgReturnList.EndEditRow(true);

            if (!ValidationReturn()) return;

            // {0}를 수정 하시겠습니까?
            object[] parameters = new object[1];
            parameters[0] = ObjectDic.Instance.GetObjectName("반송우선순위");
            Util.MessageConfirm("SFU4331", result =>
            {
                if (result == MessageBoxResult.OK)
                {
                    ModifyInbox_Out();
                }
            }, parameters);
        }
        #endregion

        #region 콤보박스 초기화 : InitCombo()
        private void InitCombo()
        {
            try
            {
                CommonCombo _combo = new CommonCombo();

                //설비타입
                String[] sFilter1 = { "LOGIS_EQPT_DETL_TYPE" };
                C1ComboBox[] cboAreaChild = { cboEquipment };
                _combo.SetCombo(cboEquipmentType, CommonCombo.ComboStatus.ALL, cbChild: cboAreaChild, sFilter: sFilter1, sCase: "COMMCODE");

                //설비명
                C1ComboBox[] cboEquipmentTypeParent = { cboEquipmentType };
                _combo.SetCombo(cboEquipment, CommonCombo.ComboStatus.ALL, cbParent: cboEquipmentTypeParent, sCase: "CWAMCSEQUIPMENT");

            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        #endregion

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

        #region Cell 변경 : dgReturnList_CurrentCellChanged()
        /// <summary>
        /// Cell 변경
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void dgReturnList_CurrentCellChanged(object sender, C1.WPF.DataGrid.DataGridCellEventArgs e)
        {
            if (sender == null || e.Cell == null)
                return;

            if (e.Cell.Row.Type == DataGridRowType.Item)
            {
                if (e.Cell.Column.Name.Equals("LOGIS_CMD_PRIORITY_NO") && e.Cell.IsEditable == true)
                {
                    DataTableConverter.SetValue(e.Cell.Row.DataItem, "CHK", 1);
                }
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
            if (Convert.ToDecimal(Convert.ToDateTime(dtpFrom.SelectedDateTime).ToString("yyyyMMdd")) > Convert.ToDecimal(Convert.ToDateTime(dtpTo.SelectedDateTime).ToString("yyyyMMdd")))
            {
                Util.Alert("SFU1913");  //종료일자가 시작일자보다 빠릅니다.
                return;
            }
            string sCmdID = txtcommand.Text.ToString();
            string sEquip = string.Empty;//cboEquipment.SelectedValue.ToString();
            string sLotId = txtLot.Text.ToString();

            if (sCmdID == "")
            {
                sCmdID = null;
            }
            if (sEquip == "")
            {
                sEquip = null;
            }
            if (sLotId == "")
            {
                sLotId = null;
            }

            DataTable inTable = new DataTable();
            inTable.Columns.Add("FROM_DATE", typeof(string));
            inTable.Columns.Add("TO_DATE", typeof(string));
            inTable.Columns.Add("LOGIS_CMD_ID", typeof(string));
            inTable.Columns.Add("LOTID", typeof(string));
            inTable.Columns.Add("EQPTID", typeof(string));
            inTable.Columns.Add("LANGID", typeof(string));

            DataRow newRow = inTable.NewRow();
            if (sCmdID != null || sLotId != null)
            {
                newRow["FROM_DATE"] = null;
                newRow["TO_DATE"] = null;
            }
            else
            {
                newRow["FROM_DATE"] = dtpFrom.SelectedDateTime.ToString("yyyyMMdd");
                newRow["TO_DATE"] = dtpTo.SelectedDateTime.ToString("yyyyMMdd");
            }

            newRow["LOGIS_CMD_ID"] = sCmdID;
            newRow["LOTID"] = sLotId;
            newRow["EQPTID"] = sEquip;
            newRow["LANGID"] = LoginInfo.LANGID;

            inTable.Rows.Add(newRow);

            dtMain = new ClientProxy().ExecuteServiceSync("DA_MCS_SEL_RETURN_PRIORITY", "INDATA", "OUTDATA", inTable);

            dgReturnList.CurrentCellChanged -= dgReturnList_CurrentCellChanged;
            Util.GridSetData(dgReturnList, dtMain, FrameOperation, true);
            dgReturnList.CurrentCellChanged += dgReturnList_CurrentCellChanged;

        }
        #endregion

        #region 저장 : ModifyInbox_Out()
        private void ModifyInbox_Out()
        {
            try
            {
                ShowLoadingIndicator();

                // DATA Table
                DataSet inDataSet = new DataSet();
                DataTable inTable = inDataSet.Tables.Add("INDATA");
                inTable.Columns.Add("LOGIS_CMD_ID", typeof(string));
                inTable.Columns.Add("LOGIS_CMD_PRIORITY_NO", typeof(string));
                inTable.Columns.Add("UPDUSER", typeof(string));

                DataRow[] dr = DataTableConverter.Convert(dgReturnList.ItemsSource).Select("CHK = 1");
                DataRow newRow = null;
                foreach (DataRow drDel in dr)
                {
                    newRow = inTable.NewRow();
                    newRow["LOGIS_CMD_ID"] = drDel["LOGIS_CMD_ID"].ToString();
                    newRow["LOGIS_CMD_PRIORITY_NO"] = Util.NVC_Int(drDel["LOGIS_CMD_PRIORITY_NO"]);
                    newRow["UPDUSER"] = LoginInfo.USERID;

                    inTable.Rows.Add(newRow);
                }


                new ClientProxy().ExecuteService_Multi("BR_MCS_REG_LOGIS_CMD_PRIORITY", "INDATA", null, (bizResult, bizException) =>
                {
                    try
                    {
                        HiddenLoadingIndicator();

                        if (bizException != null)
                        {
                            Util.MessageException(bizException);
                            return;
                        }

                        GetResult();



                    }
                    catch (Exception ex)
                    {
                        HiddenLoadingIndicator();
                        Util.MessageException(ex);
                    }
                }, inDataSet);
            }
            catch (Exception ex)
            {
                HiddenLoadingIndicator();
                Util.MessageException(ex);
            }

        }
        #endregion

        #region Validation : ValidationReturn()
        private bool ValidationReturn()
        {

            if (dgReturnList.Rows.Count == 0)
            {
                //Util.Alert("조회된 데이터가 없습니다.");
                Util.MessageValidation("SFU3537");
                return false;
            }

            DataRow[] dr = DataTableConverter.Convert(dgReturnList.ItemsSource).Select("CHK = 1");

            if (dr.Length == 0)
            {
                //Util.Alert("선택된 항목이 없습니다.");
                Util.MessageValidation("SFU1651");
                return false;
            }


            return true;
        }

        #endregion

        #region loadingIndicator : ShowLoadingIndicator(), HiddenLoadingIndicator()

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
        #endregion
    
        #endregion

    }
}
