/*************************************************************************************
 Created Date : 2016.06.16
      Creator : 
   Decription : 
--------------------------------------------------------------------------------------
 [Change History]
  2016.06.16  DEVELOPER : Initial Created.
**************************************************************************************/

using System;
using System.Windows;
using C1.WPF;
using LGC.GMES.MES.Common;

namespace LGC.GMES.MES.ProtoType04
{
    public partial class PGM_GUI_201_WAITING_LOT : C1Window, IWorkArea
    {
        #region Declaration & Constructor 
        public PGM_GUI_201_WAITING_LOT()
        {
            InitializeComponent();
        }

        /// <summary>
        ///  Frame�� ��ȣ�ۿ��ϱ� ���� ��ü
        /// </summary>
        public IFrameOperation FrameOperation
        {
            get;
            set;
        }



        #endregion

        #region Initialize

        #endregion

        #region Event

        #endregion

        #region Mehod

        #endregion

        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            // ��� Lot ��ȸ
            Get_WaitLotList();

            // �ڽ� ������ Lot ��ȸ
            Get_TerminateLotList();
        }

        private void Get_WaitLotList()
        {
            try
            {
                //DA_PRD_SEL_LOT_FOR_BOXING_NISSAN
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(ex.Message, null, "Error", MessageBoxButton.OK, ControlsLibrary.MessageBoxIcon.Warning);
            }
        }

        private void Get_TerminateLotList()
        {
            try
            {
                //DA_PRD_SEL_LOT_FOR_COMPLETE_NISSAN
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(ex.Message, null, "Error", MessageBoxButton.OK, ControlsLibrary.MessageBoxIcon.Warning);
            }
        }
    }
}