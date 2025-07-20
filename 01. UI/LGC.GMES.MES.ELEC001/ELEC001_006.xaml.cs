/*************************************************************************************
 Created Date : 2016.06.16
      Creator : 
   Decription : 믹서 공정진척
--------------------------------------------------------------------------------------
 [Change History]
  2016.06.16  DEVELOPER : Initial Created.

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
    public partial class ELEC001_006 : IWorkArea
    {
        #region Declaration & Constructor
        UcBaseElec _baseForm;
        private Util _Util = new Util();
        public IFrameOperation FrameOperation { get; set; }

        public ELEC001_006()
        {
            InitializeComponent();
            InitInheritance();
        }

        void InitInheritance()
        {
            _baseForm = new UcBaseElec();
            grdMain.Children.Add(_baseForm);
            _baseForm.FrameOperation = FrameOperation;
            _baseForm.PROCID = Process.MIXING;
            _baseForm.STARTBUTTON.Click += btnStart_Click;
            _baseForm.INVOICEBUTTON.Click += INVOICEBUTTON_Click;
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            this.Loaded -= UserControl_Loaded;
            _baseForm.FrameOperation = FrameOperation;
            _baseForm.SetApplyPermissions();
            //ApplyPermissions();
        }
        #endregion

        #region Event
        private void ApplyPermissions()
        {
            _baseForm.FrameOperation = FrameOperation;

            List<Button> listAuth = new List<Button>();
            Util.pageAuth(listAuth, FrameOperation.AUTHORITY);
        }

        private void btnStart_Click(object sender, RoutedEventArgs e)
        {
            if (!WorkOrder_chk())
                return;
            Dictionary<string, string> dicParam = new Dictionary<string, string>();

            dicParam.Add("PROCID", Process.MIXING);
            dicParam.Add("EQPTID", Util.NVC(_baseForm.EQUIPMENT_COMBO.SelectedValue));
            dicParam.Add("ELEC",""); //극성정보

            ELEC001_006_LOTSTART _LotStart = new ELEC001_006_LOTSTART(dicParam);
            _LotStart.FrameOperation = FrameOperation;

            if (_LotStart != null)
            {
                _LotStart.Closed += new EventHandler(LotStart_Closed);
                this.Dispatcher.BeginInvoke(new Action(() => _LotStart.ShowModal()));
                _LotStart.CenterOnScreen();
            }

            //ELEC001_006_LOTSTART _LotStart = new ELEC001_006_LOTSTART();
            //_LotStart.FrameOperation = FrameOperation;

            //if (_LotStart != null)
            //{
            //    object[] Parameters = new object[3];
            //    Parameters[0] = Process.MIXING;
            //    Parameters[1] = Util.NVC(_baseForm.EQUIPMENT_COMBO.SelectedValue);
            //    Parameters[2] = ""; //극성정보
            //    C1WindowExtension.SetParameters(_LotStart, Parameters);

            //    _LotStart.Closed += new EventHandler(LotStart_Closed);
            //    _LotStart.ShowModal();
            //    _LotStart.CenterOnScreen();
            //}
        }

        private void LotStart_Closed(object sender, EventArgs e)
        {
            if ((sender as ELEC001_006_LOTSTART).DialogResult == MessageBoxResult.OK)
                _baseForm.REFRESH = true;
        }

        private void INVOICEBUTTON_Click(object sender, RoutedEventArgs e)
        {
            ELEC001_002 invoice = new ELEC001_002();
            invoice.FrameOperation = FrameOperation;
            Window win = new Window { Content = invoice };
            win.ShowDialog();
        }

        // 선택한 W/O 와 선택되어있는 W/O 다를때 메시지 추가 요청
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