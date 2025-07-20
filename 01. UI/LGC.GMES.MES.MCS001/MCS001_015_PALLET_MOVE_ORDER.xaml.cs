/*************************************************************************************
 Created Date : 2018.10.03
      Creator : 신광희 차장
   Decription : CWA물류-전극 Pallet 이동 지시 신규 생성
--------------------------------------------------------------------------------------
  
**************************************************************************************/
using C1.WPF;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using System.Windows;
using LGC.GMES.MES.CMM001.Class;

namespace LGC.GMES.MES.MCS001
{
    /// <summary>
    /// MCS001_015_PALLET_MOVE_ORDER.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class MCS001_015_PALLET_MOVE_ORDER : C1Window, IWorkArea
    {
        #region Declaration & Constructor 
        public IFrameOperation FrameOperation { get; set; }
        private bool _isLoaded;
        private string _fromPortCode;
        #endregion

        public MCS001_015_PALLET_MOVE_ORDER()
        {
            InitializeComponent();
        }

        #region Initialize 
        private void InitializeControls()
        {

        }

        private void InitializeComboBox()
        {
            // 열, 연, 단, Destination 콤보박스 Setting
        }
        #endregion

        #region Event
        private void C1Window_Loaded(object sender, RoutedEventArgs e)
        {
            object[] parameters = C1WindowExtension.GetParameters(this);
            if (parameters != null)
            {

            }

            InitializeControls();
            _isLoaded = true;
        }

        private void btnMove_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidationMove()) return;

        }

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
        #endregion



        #region Mehod

        private bool ValidationMove()
        {
            return true;
        }

        private void SetDestinationCombo(C1ComboBox cbo)
        {
            const string bizRuleName = "DA_MCS_SEL_PORT_RELATION_CBO";
            string[] arrColumn = { "LANGID", "FROM_PORT_ID" };
            string[] arrCondition = { LoginInfo.LANGID, _fromPortCode };
            string selectedValueText = cbo.SelectedValuePath;
            string displayMemberText = cbo.DisplayMemberPath;

            CommonCombo.CommonBaseCombo(bizRuleName, cbo, arrColumn, arrCondition, CommonCombo.ComboStatus.NONE, selectedValueText, displayMemberText, string.Empty);
        }

        #endregion


    }
}
