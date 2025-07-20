using System;
using System.Windows.Controls;
using C1.WPF.DataGrid;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;
using System.Reflection;
using System.Data;
using System.Windows.Input;

namespace LGC.GMES.MES.CMM001.UserControls
{
    /// <summary>
    /// UcFormInput.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class UcFormInputTray
    {
        #region Declaration

        public IFrameOperation FrameOperation
        {
            get;
            set;
        }

        public UserControl UcParentControl;

        public C1DataGrid DgInputTray { get; set; }

        public string ProdLotId { get; set; }

        public string ProcessCode { get; set; }

        public string EquipmentCode { get; set; }

        public TextBox TextBoxTrayID { get; set; }

        public Button ButtonTrayRemainWait { get; set; }  //[C20181121_50523] tray 분할 버튼 추가

        private readonly BizDataSet _bizDataSet = new BizDataSet();

        private readonly Util _util = new Util();

        public UcFormInputTray()
        {
            InitializeComponent();
            SetControl();
        }

        #endregion

        #region Initialize
        private void SetControl()
        {
            DgInputTray = dgInputTray;
            TextBoxTrayID = txtTrayID;
            ButtonTrayRemainWait = btnTrayRemainWait;
        }

        #endregion

        #region Event

        private void txtTrayID_GotFocus(object sender, System.Windows.RoutedEventArgs e)
        {
            InputMethod.Current.ImeConversionMode = ImeConversionModeValues.Alphanumeric;
        }

        private void txtTrayID_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                if (!ValidationTrayChange()) return;

                ChangeStratTray();
            }
        }

        private void btnTrayChange_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            if (!ValidationTrayChange()) return;

            ChangeStratTray();
        }

        #endregion

        #region Mehod

        public void ChangeEquipment(string equipmentCode)
        {
            try
            {
                EquipmentCode = equipmentCode;

                ProdLotId = string.Empty;
                InitializeControls();

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        public void InitializeControls()
        {
            Util.gridClear(dgInputTray);
            txtTrayID.Text = string.Empty;
        }

        public void SelectInputTrayList()
        {
            try
            {
                string bizRuleName = string.Empty;

                if (string.Equals(ProcessCode, Process.CircularGrader) || string.Equals(ProcessCode, Process.CircularCharacteristicGrader))
                    bizRuleName = "DA_PRD_SEL_INPUT_TRAY_GD";
                else if (string.Equals(ProcessCode, Process.SmallGrader))
                    bizRuleName = "DA_PRD_SEL_INPUT_TRAY_FO";

                DataTable inTable = new DataTable();
                inTable.Columns.Add("LOTID", typeof(string));

                DataRow newRow = inTable.NewRow();
                newRow["LOTID"] = ProdLotId;
                inTable.Rows.Add(newRow);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync(bizRuleName, "INDATA", "OUTDATA", inTable);

                Util.GridSetData(dgInputTray, dtResult, null);

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        /// <summary>
        /// 시작 Tray 변경
        /// </summary>
        private void ChangeStratTray()
        {
            try
            {
                string bizRuleName = string.Empty;

                if (string.Equals(ProcessCode, Process.CircularGrader))
                    bizRuleName = "BR_PRD_CHK_INPUT_LOT_GD";
                else if (string.Equals(ProcessCode, Process.SmallGrader))
                    bizRuleName = "BR_PRD_CHK_INPUT_LOT_GS";
                else if (string.Equals(ProcessCode, Process.CircularCharacteristicGrader))
                    bizRuleName = "BR_PRD_CHK_INPUT_LOT_CG";

                DataTable inTable = new DataTable();
                inTable.Columns.Add("SRCTYPE", typeof(string));
                inTable.Columns.Add("IFMODE", typeof(string));
                inTable.Columns.Add("EQPTID", typeof(string));
                inTable.Columns.Add("INPUT_LOTID", typeof(string));
                inTable.Columns.Add("INPUT_TYPE", typeof(string));
                inTable.Columns.Add("USERID", typeof(string));
                inTable.Columns.Add("PROD_LOTID", typeof(string));

                DataRow newRow = inTable.NewRow();
                newRow["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                newRow["IFMODE"] = IFMODE.IFMODE_OFF;
                newRow["EQPTID"] = Util.NVC(EquipmentCode);
                newRow["INPUT_LOTID"] = txtTrayID.Text;
                newRow["INPUT_TYPE"] = "T";
                newRow["USERID"] = LoginInfo.USERID;
                newRow["PROD_LOTID"] = ProdLotId;
                inTable.Rows.Add(newRow);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync(bizRuleName, "INDATA", "OUTDATA", inTable);

                InitializeControls();
                SelectInputTrayList();
                GetProductLot();

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private bool ValidationTrayChange()
        {
            if (string.IsNullOrWhiteSpace(ProdLotId))
            {
                // 생산 Lot 정보가 없습니다.
                Util.MessageValidation("SFU4014");
                return false;
            }

            if (string.IsNullOrWhiteSpace(txtTrayID.Text))
            {
                // 시작 Tray를 입력 하세요.
                Util.MessageValidation("SFU4000");
                return false;
            }

            return true;
        }

        protected virtual void GetProductLot()
        {
            if (UcParentControl == null)
                return;

            try
            {
                Type type = UcParentControl.GetType();
                MethodInfo methodInfo = type.GetMethod("GetProductLot");
                ParameterInfo[] parameters = methodInfo.GetParameters();

                object[] parameterArrys = new object[parameters.Length];

                ////for (int i = 0; i < parameterArrys.Length; i++)
                ////{
                ////    parameterArrys[i] = true;
                ////}

                parameterArrys[0] = true;
                parameterArrys[1] = null;

                methodInfo.Invoke(UcParentControl, parameters.Length == 0 ? null : parameterArrys);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }



        #endregion





    }
}
