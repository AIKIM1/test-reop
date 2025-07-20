/*************************************************************************************
 Created Date : 2019.11.20
      Creator : 신광희C
   Decription : InsulationMixing 공정진척
--------------------------------------------------------------------------------------
 [Change History]
  2019.11.20  신광희 차장 : Initial Created.    
**************************************************************************************/

using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.CMM001.UserControls;
using LGC.GMES.MES.Common;
using System;
using System.Collections.Generic;
using System.Windows;

namespace LGC.GMES.MES.ELEC001
{
    public partial class ELEC001_035 : IWorkArea
    {
        #region Declaration & Constructor

        public UcBaseElec_CWA UcBaseElectrode;
        private readonly Util _util = new Util();

        public IFrameOperation FrameOperation { get; set; }

        public ELEC001_035()
        {
            InitializeComponent();
            InitInheritance();
        }

        void InitInheritance()
        {
            UcBaseElectrode = new UcBaseElec_CWA();
            grdMain.Children.Add(UcBaseElectrode);
            UcBaseElectrode.FrameOperation = FrameOperation;
            UcBaseElectrode.PROCID = Process.InsulationMixing;
            UcBaseElectrode.STARTBUTTON.Click += btnStart_Click;
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            Loaded -= UserControl_Loaded;
            UcBaseElectrode.FrameOperation = FrameOperation;
            UcBaseElectrode.SetApplyPermissions();            
        }

        #endregion


        #region Event

        private void btnStart_Click(object sender, RoutedEventArgs e)
        {
            if (!WorkOrder_chk())
                return;

            Dictionary<string, string> dicparameter = new Dictionary<string, string>();

            dicparameter.Add("PROCID", Process.InsulationMixing);
            dicparameter.Add("EQPTID", Util.NVC(UcBaseElectrode.EQUIPMENT_COMBO.SelectedValue));
            dicparameter.Add("ELEC", "");//극성정보

            ELEC001_006_LOTSTART popLotStart = new ELEC001_006_LOTSTART(dicparameter);
            popLotStart.FrameOperation = FrameOperation;

            popLotStart.Closed += new EventHandler(LotStart_Closed);
            Dispatcher.BeginInvoke(new Action(() => popLotStart.ShowModal()));
            popLotStart.CenterOnScreen();

        }

        private void LotStart_Closed(object sender, EventArgs e)
        {
            ELEC001_006_LOTSTART popup = sender as ELEC001_006_LOTSTART;

            if (popup != null && popup.DialogResult == MessageBoxResult.OK)
            {
                UcBaseElectrode.REFRESH = true;
            }
        }

        // 선택한 W/O 와 선택되어있는 W/O 다를때 메시지 추가 요청
        public bool WorkOrder_chk()
        {
            //bool _Woder = true;
            //if (new Util().GetDataGridCheckFirstRowIndex(UcBaseElectrode.WORKORDER_GRID, "CHK") != -1)
            //{
            //    int idx = _util.GetDataGridCheckFirstRowIndex(UcBaseElectrode.WORKORDER_GRID, "CHK");

            //    if (Util.NVC(DataTableConverter.GetValue(UcBaseElectrode.WORKORDER_GRID.Rows[0].DataItem, "EIO_WO_SEL_STAT")) == "Y")
            //    {
            //        if (Util.NVC(DataTableConverter.GetValue(UcBaseElectrode.WORKORDER_GRID.Rows[0].DataItem, "WOID")) != Util.NVC(DataTableConverter.GetValue(UcBaseElectrode.WORKORDER_GRID.Rows[idx].DataItem, "WOID")))
            //        {
            //            Util.MessageValidation("SFU1436");
            //            _Woder = false;
            //        }
            //        else
            //        {
            //            _Woder = true;
            //        }
            //    }
            //}
            //return _Woder;


            if (_util.GetWorkOrderGridSelectedRow(UcBaseElectrode.WORKORDER_GRID, "EIO_WO_SEL_STAT", "Y") == null)
            {
                Util.MessageValidation("SFU1436");
                return false;
            }

            return true;
        }

        #endregion


        #region Method

        #endregion Method
    }
}