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
using System.Windows.Input;

namespace LGC.GMES.MES.ELEC001
{
    public partial class ELEC001_014_LOTEND : C1Window, IWorkArea
    {
        #region Declaration & Constructor 
        private static string PRODID = "";
        private static string WORKDATE = "";
        private static string LOTID = "";
        private static string STATUS = "";
        private static string USERID = "";
        private static string EQPTID = "";

        private static bool result = false;

        private string saveQty = "";
        private string sProcid = "";

        /// <summary>
        ///  Frame과 상호작용하기 위한 객체
        /// </summary>
        public IFrameOperation FrameOperation
        {
            get;
            set;
        }
        public ELEC001_014_LOTEND()
        {
            InitializeComponent();
        }

        #endregion

        #region Initialize

        #endregion

        #region Event
        private void C1Window_Loaded(object sender, RoutedEventArgs e)
        {
            object[] tmps = C1WindowExtension.GetParameters(this);

            DataRowView rowview = tmps[0] as DataRowView;

            if (rowview == null)
            {
                return;
            }

            PRODID = rowview[0].ToString();
            WORKDATE = rowview[1].ToString();
            LOTID = rowview[2].ToString();
            STATUS = rowview[3].ToString();
            EQPTID = rowview[4].ToString();

            dtpDate.SelectedDateTime = System.DateTime.Now;
            TimeEditor.Value = new TimeSpan(DateTime.Now.Hour, DateTime.Now.Minute, DateTime.Now.Second);

            SearchInputLot();
            SearchResultLot(1);

        }
        #endregion

        #region Mehod
        private void SearchInputLot()
        {
            try
            {
                txtWorkOrder.Text = string.Empty;
                txtProdId.Text = string.Empty;
                txtWorkDate.Text = string.Empty;
                txtLotID.Text = string.Empty;
                txtRunQty.Text = string.Empty;
                txtWipState.Text = string.Empty;

                DataTable IndataTable = new DataTable();
                IndataTable.Columns.Add("LOTID", typeof(string));

                DataRow Indata = IndataTable.NewRow();
                Indata["LOTID"] = LOTID;
                IndataTable.Rows.Add(Indata);

                DataTable dtMain = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_SLIT_RUNLOT", "INDATA", "RSLTDT", IndataTable);

                if (dtMain.Rows.Count == 0 )
                {
                    return;
                }
                txtWorkOrder.Text = DataTableConverter.GetValue(dtMain, "WOID").ToString();
                txtProdId.Text = DataTableConverter.GetValue(dtMain, "PRODID").ToString();
                txtWorkDate.Text = DataTableConverter.GetValue(dtMain, "WORKDATE").ToString();
                txtLotID.Text = LOTID;
                txtRunQty.Text = DataTableConverter.GetValue(dtMain, "WIPQTY").ToString();
                saveQty = txtRunQty.Text;
                txtWipState.Text = DataTableConverter.GetValue(dtMain, "WIPSTAT").ToString();

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        private void SearchResultLot(int div)
        {
            try
            {
                DataTable IndataTable = new DataTable();
                IndataTable.Columns.Add("LOTID", typeof(string));

                DataRow Indata = IndataTable.NewRow();
                Indata["LOTID"] = LOTID;
                IndataTable.Rows.Add(Indata);
                DataTable dtMain;
                if (div == 1)
                {
                    dtMain = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_SLIT_ENDLOT", "INDATA", "RSLTDT", IndataTable);
                }
                else
                {
                    dtMain = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_SLIT_CHILD_ENDLOT", "INDATA", "RSLTDT", IndataTable);
                }
                

                dgEndLot.ItemsSource = DataTableConverter.Convert(dtMain);

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }

        }
        private bool ValidData()
        {
            if (!Util.CheckDecimal(txtOutQty.Text, 0)) { return false; }
            
            if (dgEndLot.Rows.Count == 0)
            {
                Util.MessageValidation("SFU1703");  //실적 LOT이 없습니다.
                return false;
            }

            for (int i = 0; i < dgEndLot.Rows.Count; i++)
            {
                if (Util.NVC_Decimal(DataTableConverter.GetValue(dgEndLot.Rows[i].DataItem, "WIPQTY_ED").ToString()) != Util.NVC_Decimal(txtOutQty.Text))
                {
                    Util.MessageValidation("SFU1802");  //입력 수량이 잘못 되었습니다.
                    return false;
               }
            }
            return true;
        }
        #endregion

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!ValidData()) return;

                //string msg = "SFU1241";
                //if (chkFinalCut.IsChecked == true)
                //{
                //    msg = "'Final Cut'이 체크 되었습니다" + msg;
                //}

                #region 기존소스
                //string sCheckLot = GridUtil.GetValue(spdRsltLot, 0, "LOTID").ToString();

                //if (sCheckLot.Substring(0, 3) == "XBC")
                //{
                //    if (sCheckLot.Substring(9, 1) == "1")
                //    {
                //        commMessage.Show("샘플 전극입니다." + "샘플링 진행/마우저 두께 기입 바랍니다.");
                //    }
                //}
                #endregion

                DataTable IndataTable = new DataTable();
                IndataTable.Columns.Add("LOTID", typeof(string));
                IndataTable.Columns.Add("PROCID", typeof(string));
                IndataTable.Columns.Add("EQPTID", typeof(string));
                IndataTable.Columns.Add("INPUTQTY", typeof(decimal));
                IndataTable.Columns.Add("CTRLQTY", typeof(decimal));
                IndataTable.Columns.Add("FINAL_CHECK", typeof(string));
                IndataTable.Columns.Add("REQFORM", typeof(string));
                IndataTable.Columns.Add("USERID", typeof(string));


                DataRow Indata = IndataTable.NewRow();
                Indata["LOTID"] = LOTID;
                Indata["PROCID"] = Process.SLITTING;
                Indata["EQPTID"] = EQPTID;
                Indata["INPUTQTY"] = Util.NVC_Decimal(txtOutQty.Text);
                Indata["CTRLQTY"] = 0;
                Indata["FINAL_CHECK"] = chkFinalCut.IsChecked == true ? "Y" : "N";
                Indata["REQFORM"] = "U";
                Indata["USERID"] = LoginInfo.USERID;

                IndataTable.Rows.Add(Indata);

                DataTable dtMain = new ClientProxy().ExecuteServiceSync("ECOM_PRE_EQPEND_LOT", "INDATA", "RSLTDT", IndataTable);
                SearchInputLot();
                SearchResultLot(2);

                btnSave.IsEnabled = false;
                Util.AlertInfo("SFU1275");  //정상처리되었습니다.
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
        private void txtOutQty_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                txtQtyChange_Process();
            }
        }
        private void txtQtyChange_Process()
        {
            if (!Util.CheckDecimal(txtOutQty.Text, 0)) { return; }

            decimal rsltQty = Util.NVC_Decimal(txtOutQty.Text); 
            decimal inputQty = Util.NVC_Decimal(saveQty);

            txtRunQty.Text = Util.NVC_DecimalStr(inputQty - rsltQty);

            for (int i = 0; i < dgEndLot.Rows.Count; i++)
            {
                DataTableConverter.SetValue(dgEndLot.Rows[i].DataItem, "WIPQTY_ED", Util.NVC_Decimal(txtOutQty.Text));
            }
        }
    }
}
