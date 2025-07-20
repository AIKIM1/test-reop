/*************************************************************************************
 Created Date : 2019.11.08
      Creator : 
   Decription : 계획조정 계획추가 Popup
--------------------------------------------------------------------------------------
 [Change History]
  2019.11.08  : Initial Created. 
**************************************************************************************/
using C1.WPF;
using C1.WPF.DataGrid;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using System;
using System.Collections;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace LGC.GMES.MES.COM001
{
    public partial class COM001_276_ADD : C1Window, IWorkArea
    {
        #region Declaration & Constructor 
        private string sPJT = string.Empty;
        private string sITEMID = string.Empty;
        private string sWO = string.Empty;
        private string sRESOURCE = string.Empty;
        private string sFACTORYID = string.Empty;
        private string sPROCID = string.Empty;
        private decimal dPLANQTY = 0;
        private decimal dORDERSEQ = 0;
        private decimal dPRE_ORDERSEQ = 0;

        private Util util = new Util();
        private string _PJT = string.Empty;
        private string _ITEMID = string.Empty;
        private string _WO = string.Empty;
        private string _OPERATION = string.Empty;
        private string _RESOURCE = string.Empty;
        private decimal _PLANQTY = 0;
        private decimal _ORDER = 0;
        private decimal _PREORDER = 0;

        public string _GetPjt
        {
            get { return _PJT; }
        }

        public string _GetITEMID
        {
            get { return _ITEMID; }
        }
        public string _GetWO
        {
            get { return _WO; }
        }
        public string _GetOPERATION
        {
            get { return sPROCID; }
        }
        public string _GetRESOURCE
        {
            get { return _RESOURCE; }
        }
        public decimal _GetPLANQTY
        {
            get { return _PLANQTY; }
        }
        public decimal _GetORDER
        {
            get { return _ORDER; }
        }
        public decimal _GetPREORDER
        {
            get { return _PREORDER; }
        }
        public COM001_276_ADD()
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

        }

        #endregion

        #region Event
        private void C1Window_Loaded(object sender, RoutedEventArgs e)
        {
            object[] tmps = C1WindowExtension.GetParameters(this);

            if (tmps != null)
            {
                sPJT = Util.NVC(tmps[0]);
                sITEMID = Util.NVC(tmps[1]);
                sWO = Util.NVC(tmps[2]);
                dPLANQTY = Util.NVC_Decimal(tmps[3]);
                sRESOURCE = Util.NVC(tmps[4]);
                dORDERSEQ = Util.NVC_Decimal(tmps[5]);
                dPRE_ORDERSEQ = Util.NVC_Decimal(tmps[5]);
                sFACTORYID = Util.NVC(tmps[6]);
                sPROCID = Util.NVC(tmps[7]);

                cboEquipment.ItemsSource = DataTableConverter.Convert(setCboAssignEquipment(sITEMID));
                cboEquipment.SelectedValue = sRESOURCE;
                txtPRJ.Text = sPJT;
                txtITEM.Text = sITEMID;
                txtWO_ENG.Text = sWO;
                txtPlanQty.Value = (double)dPLANQTY;
                txtOrder.Value = (double)dORDERSEQ;
            }
        }

        private void btnAdd_Click(object sender, RoutedEventArgs e)
        {
            if (!CanSave())
                return;

            // 추가하시겠습니까?
            Util.MessageConfirm("SFU2965", (result) =>
            {
                try
                {
                    if (result == MessageBoxResult.OK)
                    {
                        // Result
                        _PJT = Util.NVC(txtPRJ.Text);
                        _ITEMID = Util.NVC(txtITEM.Text);
                        _WO = Util.NVC(txtWO_ENG.Text);
                        _RESOURCE = Util.NVC(cboEquipment.SelectedValue);
                        _PLANQTY = (decimal)txtPlanQty.Value;
                        _ORDER = (decimal)txtOrder.Value;
                        _PREORDER = dPRE_ORDERSEQ;

                        this.DialogResult = MessageBoxResult.OK;
                    }
                }
                catch (Exception ex)
                {
                    Util.MessageException(ex);
                }
            });
        }

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = MessageBoxResult.Cancel;
        }

        #endregion

        #region Mehod
        #region # Combo
        private DataTable setCboAssignEquipment(string sITEMID)
        {
            try
            {
                DataTable dtRQSTDT = new DataTable();
                dtRQSTDT.TableName = "RQSTDT";
                dtRQSTDT.Columns.Add("LANGID", typeof(string));
                dtRQSTDT.Columns.Add("SHOPID", typeof(string));
                dtRQSTDT.Columns.Add("AREAID", typeof(string));
                dtRQSTDT.Columns.Add("PROCID", typeof(string));
                dtRQSTDT.Columns.Add("ITEM_ID", typeof(string));

                DataRow drnewrow = dtRQSTDT.NewRow();
                drnewrow["LANGID"] = LoginInfo.LANGID;
                drnewrow["SHOPID"] = LoginInfo.CFG_SHOP_ID;
                drnewrow["AREAID"] = sFACTORYID;
                drnewrow["PROCID"] = sPROCID;
                drnewrow["ITEM_ID"] = sITEMID;
                dtRQSTDT.Rows.Add(drnewrow);

                DataTable result = new ClientProxy().ExecuteServiceSync("DA_RTS_SEL_ASSIGN_EQUIPMENT_CBO", "RQSTDT", "RSLTDT", dtRQSTDT);
                if (result != null && result.Rows.Count > 0)
                    return result;
                else
                    return null;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return null;
            }
        }

        #endregion

        #region Validation
        private bool CanSave()
        {
            try
            {
                bool bRet = false;

                if (txtPlanQty.Value <= 0 || string.Equals(Util.NVC(txtPlanQty.Value), Double.NaN.ToString()))
                {
                    Util.MessageValidation("SFU1684");  //수량을 입력하세요
                    return bRet;
                }
                if (txtOrder.Value <= 0 || string.Equals(Util.NVC(txtOrder.Value), Double.NaN.ToString()))
                {
                    Util.MessageValidation("SFU8122");  //우선순위를 입력하세요
                    return bRet;
                }

                bRet = true;
                return bRet;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return false;
            }
        }
        #endregion

        #endregion

    }
}
