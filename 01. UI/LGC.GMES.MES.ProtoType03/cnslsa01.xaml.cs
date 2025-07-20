/*************************************************************************************
 Created Date : 2016.09.29
      Creator : 
   Decription : 
--------------------------------------------------------------------------------------
 [Change History]
  2016.09.29  DEVELOPER : Initial Created.





 
**************************************************************************************/

using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using C1.WPF;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;

namespace LGC.GMES.MES.ProtoType03
{
    public partial class cnslsa01 : UserControl
    {
        #region Declaration & Constructor 

  
        public cnslsa01()
        {
            InitializeComponent();
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            Initialize();
        }

        #endregion

        #region Initialize

        private void Initialize()
        {
            InitCombo();
        }

        #endregion

        #region Event
        /// <summary>
        /// More 버튼 클릭시
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnExtra_MouseLeave(object sender, MouseEventArgs e)
        {
            btnExtra.IsDropDownOpen = false;
        }

        /// <summary>
        /// 설비 콤보 값 변경시
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void cboEquipment_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {

        }
        #endregion

        #region Mehod      
        /// <summary>
        /// InitCombo() - 설비 콤보 초기화 
        /// </summary>
        private void InitCombo()
        {
            CommonCombo _combo = new CommonCombo();
            // 설비콤보 셋팅     
            //  String[] sFilter = { Process.BACK_WINDER }; //Back Winder          
             _combo.SetCombo(cboEquipment, CommonCombo.ComboStatus.NONE, sFilter:new string[] { LoginInfo.CFG_EQSG_ID, LoginInfo.CFG_PROC_ID}, sCase: "EQUIPMENT");
        }
        #endregion

      
    }
}