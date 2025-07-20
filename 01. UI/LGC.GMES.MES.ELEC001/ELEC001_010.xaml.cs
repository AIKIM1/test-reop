/*****************************************
 Created Date : 2016.06.16
      Creator : 
   Decription : 롤프레스 공정진척
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
    /// <summary>
    /// PGM_GUI_015.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class ELEC001_010 : IWorkArea
    {
        #region Declaration & Constructor
        UcBaseElec _baseForm;
        private Util _Util = new Util();
        public IFrameOperation FrameOperation { get; set; }

        public ELEC001_010()
        {
            InitializeComponent();
            InitInheritance();
        }

        void InitInheritance()
        {
            _baseForm = new UcBaseElec();
            grdMain.Children.Add(_baseForm);
            _baseForm.PROCID = Process.ROLL_PRESSING;
            _baseForm.STARTBUTTON.Click += btnStart_Click;
        }
        #endregion

        #region Initialize
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
            try
            {
                if (!WorkOrder_chk())
                    return;

                Dictionary<string, string> dicParam = new Dictionary<string, string>();

                dicParam.Add("PROCID", Process.ROLL_PRESSING);
                dicParam.Add("EQPTID", _baseForm.EQUIPMENT_COMBO.SelectedValue.ToString());
                dicParam.Add("EQSGID", _baseForm.EQUIPMENTSEGMENT_COMBO.SelectedValue.ToString());
                if (new Util().GetDataGridCheckFirstRowIndex(_baseForm.PRODLOT_GRID, "CHK") != -1)
                {
                    dicParam.Add("LOTID", Util.NVC(DataTableConverter.GetValue(_baseForm.PRODLOT_GRID.Rows[new Util().GetDataGridCheckFirstRowIndex(_baseForm.PRODLOT_GRID, "CHK")].DataItem, "LOTID_PR")));
                }
                ELEC001_010_LOTSTART _LotStart = new ELEC001_010_LOTSTART(dicParam);
                _LotStart.FrameOperation = FrameOperation;

                if (_LotStart != null)
                {
                    _LotStart.Closed += new EventHandler(LotStart_Closed);
                    this.Dispatcher.BeginInvoke(new Action(() => _LotStart.ShowModal()));
                    _LotStart.CenterOnScreen();
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void LotStart_Closed(object sender, EventArgs e)
        {
            ELEC001_010_LOTSTART window = sender as ELEC001_010_LOTSTART;

            if (window.DialogResult == MessageBoxResult.OK)
                _baseForm.REFRESH = true;
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