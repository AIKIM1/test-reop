/*************************************************************************************
 Created Date :
      Creator :
   Decription : SRS TANK
--------------------------------------------------------------------------------------
 [Change History]
  2016.08.19  : Initial Created.
  
**************************************************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Data;

using C1.WPF;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.ControlsLibrary;

namespace LGC.GMES.MES.CMM001.Popup
{
    /// <summary>
    /// ELEC001_006_LOTEND.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class CMM_SRSTANK : C1Window, IWorkArea
    {
        #region Declaration & Constructor
        private string _EQPTID = string.Empty;
        private string _TANK = string.Empty;
        private string _TANKNAME = string.Empty;
        private string _LOTID = string.Empty; // batch lot
        CommonCombo _combo = new CommonCombo();
        Util _Util = new Util();
        public string _ReturnLotID
        {
            get { return _LOTID; }
        }
        public string _ReturnTank
        {
            get { return _TANK; }
        }
        public string _ReturnTankName
        {
            get { return _TANKNAME; }
        }
        #endregion

        #region Initialize
        /// <summary>
        ///  Frame과 상호작용하기 위한 객체
        /// </summary>
        public IFrameOperation FrameOperation
        {
            get;
            set;
        }
        public CMM_SRSTANK()
        {
            InitializeComponent();
            InitCombo();
        }
        private void InitCombo()
        {
            Set_Equpipment(cboEquipment);
        }
        private void InitializeControls()
        {

        }
        #endregion

        #region Event
        private void C1Window_Loaded(object sender, RoutedEventArgs e)
        {
            object[] tmps = C1WindowExtension.GetParameters(this);

            if (tmps == null)
            {
                this.DialogResult = MessageBoxResult.Cancel;
                return;
            }

            _EQPTID = tmps[0].ToString();

            InitializeControls();
            InitCombo();
            GetCurrentMount();
        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            if (txtBatchLot.Text.Trim().Equals(""))
            {
                Util.MessageValidation("SFU1313");  //Batch Lot이 선택되지 않았습니다.
                return;
            }
            _TANK = Util.NVC(cboEquipment.SelectedValue);
            _TANKNAME = Util.NVC(cboEquipment.Text);
            _LOTID = txtBatchLot.Text;
            SetTank();
            this.DialogResult = MessageBoxResult.OK;
        }

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = MessageBoxResult.Cancel;
        }
        private void cboEquipment_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            try
            {
                DataTable IndataTable = new DataTable();
                IndataTable.Columns.Add("EQPTID", typeof(string));

                DataRow Indata = IndataTable.NewRow();
                Indata["EQPTID"] = Util.NVC(cboEquipment.SelectedValue);
                IndataTable.Rows.Add(Indata);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_BATCHID", "INDATA", "RSLTDT", IndataTable);

                if (dtResult.Rows.Count == 0)
                {
                    txtBatchLot.Text = string.Empty;
                }
                else
                {
                    txtBatchLot.Text = dtResult.Rows[0]["INPUT_LOTID"].ToString();
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        #endregion

        #region Mehod
        private void GetCurrentMount()
        {
            try
            {
                DataTable IndataTable = new DataTable();
                IndataTable.Columns.Add("LANGID", typeof(string));
                IndataTable.Columns.Add("EQPTID", typeof(string));

                DataRow Indata = IndataTable.NewRow();
                Indata["LANGID"] = LoginInfo.LANGID;
                Indata["EQPTID"] = _EQPTID;
                IndataTable.Rows.Add(Indata);

                DataTable dtMain = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_EQPT_CURR_MOUNT_MTRL_CT", "INDATA", "RSLTDT", IndataTable);
                if (dtMain.Rows.Count > 0)
                {
                    DataTable Slurrydt = dtMain.Clone();
                    foreach (DataRow _iRow in dtMain.Select("PRDT_CLSS_CODE = 'ASL'"))
                    {
                        Slurrydt.ImportRow(_iRow);
                    }
                    dgSlurry.ItemsSource = DataTableConverter.Convert(Slurrydt);
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        #region  

        private void Set_Equpipment(C1ComboBox cbo)
        {
            DataTable dtRQSTDT = new DataTable();
            dtRQSTDT.TableName = "RQSTDT";
            dtRQSTDT.Columns.Add("LANGID", typeof(string));

            DataRow drnewrow = dtRQSTDT.NewRow();
            drnewrow["LANGID"] = LoginInfo.LANGID;
            dtRQSTDT.Rows.Add(drnewrow);

            new ClientProxy().ExecuteService("DA_BAS_SEL_SRSTANK_CBO", "RQSTDT", "RSLTDT", dtRQSTDT, (result, Exception) =>
            {
                if (Exception != null)
                {
                    Util.AlertByBiz("DA_BAS_SEL_SRSTANK_CBO", Exception.Message, Exception.ToString());
                    return;
                }
                cbo.ItemsSource = DataTableConverter.Convert(result);
                cbo.SelectedIndex = 0;
            }
            );
        }

        private void SetTank()
        {
            try
            {
                DataTable IndataTable = new DataTable();
                IndataTable.Columns.Add("EQPTID", typeof(string));
                IndataTable.Columns.Add("TANK", typeof(string));
                IndataTable.Columns.Add("INPUT_LOTID", typeof(string));

                DataRow Indata = IndataTable.NewRow();
                Indata["EQPTID"] = _EQPTID;
                Indata["TANK"] = _TANK;
                Indata["INPUT_LOTID"] = Util.NVC(txtBatchLot.Text);
                IndataTable.Rows.Add(Indata);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_INS_CURR_MOUNT_BATCH", "INDATA", null, IndataTable);

                
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }





        #endregion

        #endregion

        private void btnAdd_Click(object sender, RoutedEventArgs e)
        {
            if (txtBatchLot.Text.Trim().Equals(""))
            {
                Util.MessageValidation("SFU1633"); //선택된 Slurry가 없습니다.
                return;
            }
            DataTableConverter.SetValue(dgSlurry.Rows[dgSlurry.CurrentRow].DataItem, "INPUT_LOTID", txtBatchLot.Text);
        }
    }
}
