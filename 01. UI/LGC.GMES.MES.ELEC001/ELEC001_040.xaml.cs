/*************************************************************************************
 Created Date : 2022.11.02
      Creator : 
   Decription : 작업시작 대기 Lot List 조회 (Laser ablation)
--------------------------------------------------------------------------------------
 [Change History]
 2022.11.02 : DEVELOPER : Initial Created. [C20221107-000542]
**************************************************************************************/

using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.CMM001.UserControls;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;

namespace LGC.GMES.MES.ELEC001
{
    public partial class ELEC001_040 : IWorkArea
    {
        #region Declaration & Constructor 
        UcBaseElec _baseForm;

        public IFrameOperation FrameOperation { get; set; }

        public ELEC001_040()
        {
            InitializeComponent();
            InitInheritance();
        }

        void InitInheritance()
        {
            _baseForm = new UcBaseElec();
            grdMain.Children.Add(_baseForm);
            _baseForm.PROCID = Process.LASER_ABLATION;
            _baseForm.STARTBUTTON.Click += btnStart_Click;
        }
        #endregion

        #region Event
        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            this.Loaded -= UserControl_Loaded;
            _baseForm.FrameOperation = FrameOperation;
            _baseForm.SetApplyPermissions();
            //ApplyPermissions();
        }

        private void ApplyPermissions()
        {
            _baseForm.FrameOperation = FrameOperation;

            List<Button> listAuth = new List<Button>();
            Util.pageAuth(listAuth, FrameOperation.AUTHORITY);
        }

        private void btnStart_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                ELEC001_040_LOTSTART LOTSTART = new ELEC001_040_LOTSTART();

                if (LOTSTART != null)
                {
                    object[] Parameters = new object[4];
                    Parameters[0] = Process.LASER_ABLATION;
                    Parameters[1] = _baseForm.EQUIPMENT_COMBO.SelectedValue.ToString();
                    Parameters[2] = _baseForm.EQUIPMENTSEGMENT_COMBO.SelectedValue.ToString();
                    if (new Util().GetDataGridCheckFirstRowIndex(_baseForm.PRODLOT_GRID, "CHK") != -1)
                    {
                        Parameters[3] = Util.NVC(DataTableConverter.GetValue(_baseForm.PRODLOT_GRID.Rows[new Util().GetDataGridCheckFirstRowIndex(_baseForm.PRODLOT_GRID, "CHK")].DataItem, "LOTID_PR"));
                    }
                    C1WindowExtension.SetParameters(LOTSTART, Parameters);

                    LOTSTART.Closed += new EventHandler(LotStart_Closed);
                    this.Dispatcher.BeginInvoke(new Action(() => LOTSTART.ShowModal()));
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void LotStart_Closed(object sender, EventArgs e)
        {
            ELEC001_040_LOTSTART window = sender as ELEC001_040_LOTSTART;

            if (window.DialogResult == MessageBoxResult.OK)
                _baseForm.REFRESH = true;
        }
        #endregion
    }
}