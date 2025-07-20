/*************************************************************************************
 Created Date : 2017.05.31
      Creator : 이슬아D
   Decription : 전지 5MEGA-GMES 구축 - 1차포장구성 화면 - INBOX 라벨 발행 팝업
--------------------------------------------------------------------------------------
 [Change History]
  2017.06.20  이슬아D : 최초생성
  
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
using C1.WPF;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.ControlsLibrary;
using System.Data;
using C1.WPF.DataGrid;
using LGC.GMES.MES.CMM001.Extensions;

namespace LGC.GMES.MES.BOX001
{

    public partial class BOX001_204_CHANGE_SHIPTO : C1Window, IWorkArea
    {
        #region Declaration & Constructor 
        Util _Util = new Util();
        
        string sPrt = string.Empty;
        string sRes = string.Empty;
        string sCopy = string.Empty;
        string sXpos = string.Empty;
        string sYpos = string.Empty;
        string sDark = string.Empty;
        DataRow drPrtInfo = null;
        
        private string _USERID = string.Empty;

        public BOX001_204_CHANGE_SHIPTO()
        {
            InitializeComponent();
        }

        /// <summary>
        ///  Frame과 상호작용하기 위한 객체
        /// </summary>
        public IFrameOperation FrameOperation
        {
            get;
            set;
        }

        #endregion


        #region Initialize


        /// <summary>
        /// 화면내 버튼 권한 처리
        /// </summary>
        private void ApplyPermissions()
        {
            List<Button> listAuth = new List<Button>();
            //listAuth.Add(btnInReplace);
            Util.pageAuth(listAuth, FrameOperation.AUTHORITY);
        }

        /// <summary>
        /// 콤보박스 초기 설정
        /// </summary>
        private void InitCombo()
        {
            CommonCombo _combo = new CommonCombo();
          //_combo.SetCombo(cboShipto, CommonCombo.ComboStatus.SELECT, sFilter: new string[] { LoginInfo.CFG_SHOP_ID, null, null }, sCase: "SHIPTO_CP");
            _combo.SetCombo(cboLabelType, CommonCombo.ComboStatus.SELECT, sFilter: new string[] { "MOBILE_INBOX_LABEL_TYPE" }, sCase: "COMMCODE_WITHOUT_CODE");
        }

        /// <summary>
        /// 컨트롤 초기값 설정
        /// </summary>
        private void InitControl()
        {
            object[] tmps = C1WindowExtension.GetParameters(this);
            txtPalletID.Text = (string)tmps[0];
            SetShipto((string)tmps[1]);
            txtShiptoID.Text = (string)tmps[2];
            _USERID = (string)tmps[3];
        }
        #endregion

        #region Event

        private void C1Window_Loaded(object sender, RoutedEventArgs e)
        {
            //object[] tmps = C1WindowExtension.GetParameters(this);
            //_EQSGID = Util.NVC(tmps[0]);
            //_EQPTID = Util.NVC(tmps[1]);
            //_USERID = Util.NVC(tmps[2]);

            InitCombo();
            InitControl();
        }

        private void btnChangeShipto_Click(object sender, RoutedEventArgs e)
        {
            
            if (string.IsNullOrWhiteSpace((string)cboShipto.SelectedValue) || (string)cboShipto.SelectedValue == "SELECT")
            {
                //SFU4096 	출하처를 선택하세요.
                Util.MessageValidation("SFU4096");
                return;
            }

            if (string.IsNullOrWhiteSpace((string)cboLabelType.SelectedValue) || (string)cboLabelType.SelectedValue == "SELECT")
            {
                //SFU1522 라벨 타입을 선택하세요.
                Util.MessageValidation("SFU1522");
                return;
            }

            //BR_PRD_REG_CHANGE_SHIPTO_NJ
            // SFU2875  변경하시겠습니까?	
            Util.MessageConfirm("SFU2875", (result) =>
            {
                if (result == MessageBoxResult.OK)
                {
                    DataSet indataSet = new DataSet();
                    DataTable inDataTable = indataSet.Tables.Add("INDATA");
                    inDataTable.Columns.Add("SRCTYPE");
                    inDataTable.Columns.Add("BOXID");
                    inDataTable.Columns.Add("TO_SHIPTO_ID");
                    inDataTable.Columns.Add("USERID");
                    inDataTable.Columns.Add("LABEL_ID");

                    DataRow newRow = inDataTable.NewRow();
                    newRow["SRCTYPE"] = "UI";
                    newRow["BOXID"] = txtPalletID.Text;
                    newRow["TO_SHIPTO_ID"] = cboShipto.SelectedValue;
                    newRow["USERID"] = _USERID;
                    newRow["LABEL_ID"] = cboLabelType.SelectedValue.ToString();

                    inDataTable.Rows.Add(newRow);
                    loadingIndicator.Visibility = Visibility.Visible;

                    new ClientProxy().ExecuteService_Multi("BR_PRD_REG_CHANGE_SHIPTO_NJ", "INDATA", null, (bizResult, bizException) =>
                    {
                        try
                        {
                            if (bizException != null)
                            {
                                Util.MessageException(bizException);
                                return;
                            }
                            //SFU1166  변경되었습니다.
                            Util.MessageInfo("SFU1166");
                            this.DialogResult = MessageBoxResult.OK;
                            this.Close();
                        }
                        catch (Exception ex)
                        {
                            Util.MessageException(ex);
                        }
                        finally
                        {
                            loadingIndicator.Visibility = Visibility.Collapsed;
                        }
                    }, indataSet);
                }
            });
        }
        #endregion

        private void btnClose_Clicked(object sender, RoutedEventArgs e)
        {
            this.DialogResult = MessageBoxResult.Cancel;
            this.Close();
        }

        #region Mehod

        private void SetShipto(String sProdID)
        {
            const string bizRuleName = "DA_PRD_SEL_SHIPTO_CBO_NJ";
            string[] arrColumn = { "LANGID", "SHOPID", "PRODID" };
            string[] arrCondition = { LoginInfo.LANGID, LoginInfo.CFG_SHOP_ID, sProdID };
            string selectedValueText = "CBO_CODE";
            string displayMemberText = "CBO_NAME";

            CommonCombo.CommonBaseCombo(bizRuleName, cboShipto, arrColumn, arrCondition, CommonCombo.ComboStatus.SELECT, selectedValueText, displayMemberText, null);
        }

        #endregion

    }
}
