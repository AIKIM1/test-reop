/*************************************************************************************
 Created Date : 2016.06.16
      Creator : 
   Decription : 
--------------------------------------------------------------------------------------
 [Change History]
  2016.06.16  DEVELOPER : Initial Created.
**************************************************************************************/

using System.Windows.Controls;
using C1.WPF;
using LGC.GMES.MES.CMM001.Class;

namespace LGC.GMES.MES.ProtoType04
{
    public partial class PGM_GUI_125 : UserControl
    {
        #region Declaration & Constructor 
        public PGM_GUI_125()
        {
            InitializeComponent();
            InitCombo();

        }




        #endregion

        #region Initialize
        //화면내 combo 셋팅
        private void InitCombo()
        {


            //동,라인,공정,설비 셋팅

            CommonCombo _combo = new CMM001.Class.CommonCombo();

            C1ComboBox[] cboAreaChild = { cboLine, cboProcess };
            _combo.SetCombo(cboArea, CommonCombo.ComboStatus.ALL, cbChild: cboAreaChild);

            C1ComboBox[] cboLineChild = { cboEquipment };
            C1ComboBox[] cboLineParent = { cboArea };
            _combo.SetCombo(cboLine, CommonCombo.ComboStatus.ALL, cbChild: cboLineChild, cbParent: cboLineParent);

            C1ComboBox[] cbProcessChild = { cboEquipment };
            _combo.SetCombo(cboProcess, CommonCombo.ComboStatus.ALL, cbChild: cbProcessChild);

            C1ComboBox[] cbEquipmentParent = { cboLine, cboProcess };
            _combo.SetCombo(cboEquipment, CommonCombo.ComboStatus.ALL, cbParent: cbEquipmentParent);

        }


        #endregion

        #region Event
        #endregion

        #region Mehod
        #endregion
    }
}