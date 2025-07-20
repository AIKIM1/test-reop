/*************************************************************************************
 Created Date : 2016.06.16
      Creator : 
   Decription : 
--------------------------------------------------------------------------------------
 [Change History]
  2016.06.16  DEVELOPER : Initial Created.
**************************************************************************************/

using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using C1.WPF;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;

namespace LGC.GMES.MES.ProtoType04
{
    public partial class PGM_GUI_134 : UserControl, IWorkArea
    {
        #region Declaration & Constructor 
        public PGM_GUI_134()
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
        //화면내 combo 셋팅
        private void InitCombo()
        {

            //동,라인,공정,설비 셋팅
            CommonCombo _combo = new CommonCombo();


            //동
            C1ComboBox[] cboAreaChild = { cboEquipmentSegmant, cboShift };
            _combo.SetCombo(cboArea, CommonCombo.ComboStatus.ALL, cbChild: cboAreaChild);

            //라인
            C1ComboBox[] cboEquipmentSegmantParent = { cboArea };
            C1ComboBox[] cboEquipmentSegmantChild = { cboShift, cboProcess, cboEquipment };
            _combo.SetCombo(cboEquipmentSegmant, CommonCombo.ComboStatus.ALL, cbChild: cboEquipmentSegmantChild, cbParent: cboEquipmentSegmantParent);

            //공정
            C1ComboBox[] cbProcessParent = { cboShift, cboEquipmentSegmant };
            C1ComboBox[] cbProcessChild = { cboEquipment };
            _combo.SetCombo(cboProcess, CommonCombo.ComboStatus.ALL, cbChild: cbProcessChild, cbParent: cbProcessParent);

            //설비
            C1ComboBox[] cbEquipmentParent = { cboEquipmentSegmant, cboProcess };
            _combo.SetCombo(cboEquipment, CommonCombo.ComboStatus.ALL, cbParent: cbEquipmentParent);

            //작업조
            C1ComboBox[] cboShiftParent = { cboArea, cboEquipmentSegmant, cboProcess };
            _combo.SetCombo(cboShift, CommonCombo.ComboStatus.ALL, cbParent: cboShiftParent);

            //loss 코드
            C1ComboBox[] cboLossChild = { cboAction };
            _combo.SetCombo(cboLoss, CommonCombo.ComboStatus.NONE, cbChild: cboLossChild);

            //부동코드
            C1ComboBox[] cboActionParent = { cboLoss, cboEquipment };
            _combo.SetCombo(cboAction, CommonCombo.ComboStatus.NONE, cbParent: cboActionParent);

        }
        #endregion

        #region Event
        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            //사용자 권한별로 버튼 숨기기
            List<Button> listAuth = new List<Button>();
            Util.pageAuth(listAuth, FrameOperation.AUTHORITY);
            //여기까지 사용자 권한별로 버튼 숨기기

        }
        #endregion

        #region Mehod
        #endregion
    }
}