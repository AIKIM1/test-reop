/*****************************************
 Created Date : 2016.06.16
      Creator : 
   Decription : ������ ������ô
------------------------------------------
 [Change History]
  2016.06.16  DEVELOPER : Initial Created.
******************************************/

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
    public partial class ELEC001_017 : IWorkArea
    {
        #region Declaration & Constructor 
        UcBaseElec _baseForm;
        Util _Util = new Util();

        public IFrameOperation FrameOperation { get; set; }

        public ELEC001_017()
        {
            InitializeComponent();
            InitInheritance();
        }

        void InitInheritance()
        {
            _baseForm = new UcBaseElec();
            grdMain.Children.Add(_baseForm);
            _baseForm.PROCID = Process.TAPING;
            _baseForm.STARTBUTTON.Click += btnStart_Click;
        }
        #endregion

        #region MainWindow
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
            //if (_Util.GetDataGridCheckFirstRowIndex(_baseForm.PRODLOT_GRID, "CHK") == -1)
            //{
            //    Util.Alert("������ LOT�� �����ϴ�.");
            //    return;
            //}
            if (!WorkOrder_chk())
                return;

            Dictionary<string, string> dicParam = new Dictionary<string, string>();

            dicParam.Add("PROCID", Process.TAPING);
            dicParam.Add("EQPTID", _baseForm.EQUIPMENT_COMBO.SelectedValue.ToString());
            dicParam.Add("EQSGID", _baseForm.EQUIPMENTSEGMENT_COMBO.SelectedValue.ToString());
            if (_Util.GetDataGridCheckFirstRowIndex(_baseForm.PRODLOT_GRID, "CHK") != -1)
            {
                dicParam.Add("LOTID", Util.NVC(DataTableConverter.GetValue(_baseForm.PRODLOT_GRID.Rows[new Util().GetDataGridCheckFirstRowIndex(_baseForm.PRODLOT_GRID, "CHK")].DataItem, "LOTID_PR")));
            }
            PGM_GUI_016_LOTSTART _LotStart = new PGM_GUI_016_LOTSTART(dicParam);
            _LotStart.FrameOperation = FrameOperation;

            if (_LotStart != null)
            {
                _LotStart.Closed += new EventHandler(LotStart_Closed);
                this.Dispatcher.BeginInvoke(new Action(() => _LotStart.ShowModal()));
                _LotStart.CenterOnScreen();
            }

            //PGM_GUI_016_LOTSTART LOTSTART = new PGM_GUI_016_LOTSTART();

            //if (LOTSTART != null)
            //{
            //    object[] Parameters = new object[4];
            //    Parameters[0] = Process.TAPING;
            //    Parameters[1] = _baseForm.EQUIPMENT_COMBO.SelectedValue.ToString();
            //    Parameters[2] = _baseForm.EQUIPMENTSEGMENT_COMBO.SelectedValue.ToString();

            //    if (_Util.GetDataGridCheckFirstRowIndex(_baseForm.PRODLOT_GRID, "CHK") != -1)
            //    {
            //        Parameters[3] = Util.NVC(DataTableConverter.GetValue(_baseForm.PRODLOT_GRID.Rows[new Util().GetDataGridCheckFirstRowIndex(_baseForm.PRODLOT_GRID, "CHK")].DataItem, "LOTID_PR"));
            //    }

            //    C1WindowExtension.SetParameters(LOTSTART, Parameters);

            //    LOTSTART.Closed += new EventHandler(LotStart_Closed);
            //    this.Dispatcher.BeginInvoke(new Action(() => LOTSTART.ShowModal()));
            //}
        }

        private void LotStart_Closed(object sender, EventArgs e)
        {
            PGM_GUI_016_LOTSTART window = sender as PGM_GUI_016_LOTSTART;

            if (window.DialogResult == MessageBoxResult.OK)
                _baseForm.REFRESH = true;
        }

        // ������ W/O �� ���õǾ��ִ� W/O �ٸ��� �޽��� �߰� ��û
        public bool WorkOrder_chk()
        {
            bool _Woder = true;
            if (new Util().GetDataGridCheckFirstRowIndex(_baseForm.WORKORDER_GRID, "CHK") != -1)
            {
                int idx = _Util.GetDataGridCheckFirstRowIndex(_baseForm.WORKORDER_GRID, "CHK");

                if (Util.NVC(DataTableConverter.GetValue(_baseForm.WORKORDER_GRID.Rows[0].DataItem, "EIO_WO_SEL_STAT")) == "Y")
                {
                    if (Util.NVC(DataTableConverter.GetValue(_baseForm.WORKORDER_GRID.Rows[0].DataItem, "WOID")) != Util.NVC(DataTableConverter.GetValue(_baseForm.WORKORDER_GRID.Rows[idx].DataItem, "WOID")))
                    {
                        Util.MessageValidation("SFU1436");
                        _Woder = false;
                    }
                    else
                    {
                        _Woder = true;
                    }
                }
            }
            return _Woder;
        }
        #endregion
    }
}