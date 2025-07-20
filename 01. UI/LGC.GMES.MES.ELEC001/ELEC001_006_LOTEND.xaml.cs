/*************************************************************************************
 Created Date :
      Creator :
   Decription : Mixer 작업완료
--------------------------------------------------------------------------------------
 [Change History]
  2016.08.19  : Initial Created.
  
**************************************************************************************/

using C1.WPF;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using System.Data;
using System.Windows;

namespace LGC.GMES.MES.ELEC001
{
    /// <summary>
    /// ELEC001_006_LOTEND.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class ELEC001_006_LOTEND : C1Window, IWorkArea
    {
        #region Declaration & Constructor
        public string _ProcID = string.Empty;
        public string _WorkOrder = string.Empty;
        public string _EqptID = string.Empty;
        public string _LotID = string.Empty;
        public string _UserID = string.Empty;
        public string _BizRule = string.Empty;

        Util _Util = new Util();
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
        public ELEC001_006_LOTEND()
        {
            InitializeComponent();
        }

        private void InitializeControls()
        {
            txtWorkOrder.Text = _WorkOrder;
            txtLotid.Text = _LotID;
        }
        #endregion

        #region Event
        private void C1Window_Loaded(object sender, RoutedEventArgs e)
        {
            object[] tmps = C1WindowExtension.GetParameters(this);

            if (tmps == null)
            {
                this.DialogResult = MessageBoxResult.Cancel;
                this.Close();
            }
            _ProcID = Util.NVC(tmps[0]);
            _WorkOrder = Util.NVC(tmps[1]);
            _EqptID = Util.NVC(tmps[2]);
            _LotID = Util.NVC(tmps[3]);
            _UserID = Util.NVC(tmps[4]);
            InitializeControls();

        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidateEnd())
            {
                return;
            }
            LotEnd();

            this.DialogResult = MessageBoxResult.OK;
            this.Close();
        }

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = MessageBoxResult.Cancel;
        }

        #endregion

        #region Mehod

        private void LotEnd()
        {
            //저장하시겠습니까?
            Util.MessageConfirm("SFU1241", (sresult) =>
            {
                if (sresult == MessageBoxResult.OK)
                {
                    DataTable IndataTable = new DataTable();
                    IndataTable.Columns.Add("SRCTYPE", typeof(string));
                    IndataTable.Columns.Add("IFMODE", typeof(string));
                    IndataTable.Columns.Add("EQPTID", typeof(string));
                    IndataTable.Columns.Add("LOTID", typeof(string));
                    IndataTable.Columns.Add("INPUTQTY", typeof(decimal));
                    IndataTable.Columns.Add("OUTPUTQTY", typeof(decimal));
                    IndataTable.Columns.Add("RESNQTY", typeof(decimal));
                    IndataTable.Columns.Add("USERID", typeof(string));

                    DataRow Indata = IndataTable.NewRow();
                    Indata["SRCTYPE"] = "UI";
                    Indata["IFMODE"] = IFMODE.IFMODE_ON;   // 장비에서만 진행
                    Indata["EQPTID"] = _EqptID;
                    Indata["LOTID"] = txtLotid.Text;
                    Indata["INPUTQTY"] = txtResultqty.Text;
                    Indata["OUTPUTQTY"] = txtResultqty.Text;
                    Indata["RESNQTY"] = 0;
                    Indata["USERID"] = _UserID;
                    IndataTable.Rows.Add(Indata);

                    if (_ProcID == Process.MIXING)
                    {
                        _BizRule = "BR_PRD_REG_EQPT_END_LOT_MX";
                    }
                    else if (_ProcID == Process.SRS_MIXING)
                    {
                        _BizRule = "BR_PRD_REG_EQPT_END_LOT_SX";
                    }

                    new ClientProxy().ExecuteService(_BizRule, "INDATA", null, IndataTable, (result, ex) =>
                    {
                        loadingIndicator.Visibility = Visibility.Collapsed;

                        if (ex != null)
                        {
                            Util.AlertByBiz(_BizRule, ex.Message, ex.ToString());
                            return;
                        }
                        Util.AlertInfo("SFU1275"); //정상처리되었습니다.
                    });
                }
            });
            //this.Close();
        }

        private bool ValidateEnd()
        {
            bool rslt = true;
            if (txtResultqty.Text.Equals(""))
                rslt = false;
            return rslt;
        }

        #endregion


    }
}
