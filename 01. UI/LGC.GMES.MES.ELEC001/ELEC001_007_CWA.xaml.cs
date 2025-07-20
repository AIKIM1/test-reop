/*****************************************
 Created Date : 2016.06.16
      Creator : 
   Decription : 코터 공정진척
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
    public partial class ELEC001_007_CWA : IWorkArea
    {
        #region Declaration & Constructor
        UcBaseElec_CWA _baseForm;
        private Util _Util = new Util();

        public ELEC001_007_CWA()
        {
            InitializeComponent();
            InitInheritance();
        }

        public IFrameOperation FrameOperation { get; set; }

        void InitInheritance()
        {
            _baseForm = new UcBaseElec_CWA();
            grdMain.Children.Add(_baseForm);
            _baseForm.PROCID = Process.COATING;
            _baseForm.STARTBUTTON.Click += btnStart_Click;
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

            dicParam.Add("EQPTID",   Util.NVC(_baseForm.EQUIPMENT_COMBO.SelectedValue));
            dicParam.Add("EQSGID",   Util.NVC(_baseForm.EQUIPMENTSEGMENT_COMBO.SelectedValue));
            dicParam.Add("LARGELOT", Util.NVC(_baseForm.LARGELOTID));
            dicParam.Add("WODETIL",  Util.NVC(_baseForm.WO_DETL_ID));
            dicParam.Add("COATSIDE", "");
            dicParam.Add("SINGL", "");
            if (new Util().GetDataGridCheckFirstRowIndex(_baseForm.PRODLOT_GRID, "CHK") != -1)
            {
                dicParam.Add("LOTID_PR", Util.NVC(DataTableConverter.GetValue(_baseForm.PRODLOT_GRID.Rows[new Util().GetDataGridCheckFirstRowIndex(_baseForm.PRODLOT_GRID, "CHK")].DataItem, "LOTID_PR")));
            }
            else
            {
                dicParam.Add("LOTID_PR", "");
            }
            ELEC001_007_LOTSTART _LotStart = new ELEC001_007_LOTSTART(dicParam);
            _LotStart.FrameOperation = FrameOperation;
            _LotStart.IsSingleCoater = false;
            if (_LotStart != null)
            {
                _LotStart.Closed += new EventHandler(LotStart_Closed);
                this.Dispatcher.BeginInvoke(new Action(() => _LotStart.ShowModal()));
                _LotStart.CenterOnScreen();
            }
        }

        private void LotStart_Closed(object sender, EventArgs e)
        {
            ELEC001_007_LOTSTART window = sender as ELEC001_007_LOTSTART;

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