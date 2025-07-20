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

namespace LGC.GMES.MES.BOX001
{

    public partial class BOX001_027_INBOX_LABEL : C1Window, IWorkArea
    {
        #region Declaration & Constructor 

        private string _PROCID = string.Empty;
        private string _EQPTID = string.Empty;
        private string _USERID = string.Empty;

        public BOX001_027_INBOX_LABEL()
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
           
            _combo.SetCombo(cboEquipment, CommonCombo.ComboStatus.SELECT, sFilter: new string[] { string.Empty, _PROCID }, sCase: "EQUIPMENT_BY_EQSGID_PROCID");     // DA_BAS_SEL_EQUIPMENT_BY_EQSGID_PROCID_CBO       
            _combo.SetCombo(cboType, CommonCombo.ComboStatus.SELECT, sFilter: new string[] { "MOBILE_INBOX_LABEL_TYPE" }, sCase: "COMMCODE_WITHOUT_CODE");
        }

        /// <summary>
        /// 컨트롤 초기값 설정
        /// </summary>
        private void InitControl()
        {
            object[] tmps = C1WindowExtension.GetParameters(this);
            cboEquipment.SelectedValue = Util.NVC(tmps[1]);
            _USERID = Util.NVC(tmps[2]);          
        }
        #endregion

        #region Event

        private void C1Window_Loaded(object sender, RoutedEventArgs e)
        {
            object[] tmps = C1WindowExtension.GetParameters(this);
            _PROCID = Util.NVC(tmps[0]);

            InitCombo();
            InitControl();
        }

        private void btnPrint_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                DataSet indataSet = new DataSet();
                DataTable inDataTable = indataSet.Tables.Add("INDATA");
                inDataTable.Columns.Add("EQPTID");
                inDataTable.Columns.Add("PRINT_QTY");
                inDataTable.Columns.Add("INBOX_LABEL_TYPE");
                inDataTable.Columns.Add("USERID");
              
                DataRow newRow = inDataTable.NewRow();
                newRow["EQPTID"] = Util.NVC(cboEquipment.SelectedValue);
                newRow["PRINT_QTY"] = txtInputQty.Value;
                newRow["INBOX_LABEL_TYPE"] = Util.NVC(cboType.SelectedValue);
                newRow["USERID"] = _USERID;

                inDataTable.Rows.Add(newRow);

                new ClientProxy().ExecuteService_Multi("BR_PRD_ISSUE_INBOX_LABEL_MP", "INDATA", "OUTLABEL,OUTBOX", (bizResult, bizException) =>
                {
                    try
                    {
                        if (bizException != null)
                        {
                            Util.MessageException(bizException);
                            return;
                        }                     

                        Util.MessageInfo("SFU1275");    //정상 처리 되었습니다.

                        this.DialogResult = MessageBoxResult.OK;
                    }
                    catch (Exception ex)
                    {
                        Util.MessageException(ex);
                    }
                }, indataSet);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
            finally
            {
                loadingIndicator.Visibility = Visibility.Collapsed;
            }
        }
        #endregion

        private void btnClose_Clicked(object sender, RoutedEventArgs e)
        {
            this.DialogResult = MessageBoxResult.Cancel;
            this.Close();
        }

        #region Mehod

        #endregion

    }
}
