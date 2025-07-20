/*****************************************
 Created Date : 2016.06.16
      Creator : 
   Decription : BackWinder 공정진척
------------------------------------------
 [Change History]
  2016.06.16  DEVELOPER : Initial Created.
******************************************/

using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.CMM001.UserControls;
using LGC.GMES.MES.Common;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;

namespace LGC.GMES.MES.ELEC001
{
    public partial class ELEC001_013 : IWorkArea
    {
        #region Variable -- Declaration & Constructor
        UcBaseElec _baseForm;

        public ELEC001_013()
        {
            InitializeComponent();
            InitInheritance();
        }

        /// <summary>
        /// Frame과 상호작용하기 위한 객체
        /// </summary>
        public IFrameOperation FrameOperation { get; set; }

        void InitInheritance()
        {
            _baseForm = new UcBaseElec();
            grdMain.Children.Add(_baseForm);
            _baseForm.PROCID = Process.BACK_WINDER;
            _baseForm.STARTBUTTON.Click += btnStart_Click;
        }
        #endregion

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            this.Loaded -= UserControl_Loaded;
            _baseForm.FrameOperation = FrameOperation;
            _baseForm.SetApplyPermissions();
            //ApplyPermissions();
        }

        private void btnStart_Click(object sender, RoutedEventArgs e)
        {
                StartRun();
        }

        private void StartRun()
        {
            try
            {
                string strPROCID = Process.BACK_WINDER;
                string strEQSGID = _baseForm.EQUIPMENTSEGMENT_COMBO.SelectedValue.ToString();
                string strEQPTID = _baseForm.EQUIPMENT_COMBO.SelectedValue.ToString();
                string strLOTID = string.Empty;
                if (new Util().GetDataGridCheckFirstRowIndex(_baseForm.PRODLOT_GRID, "CHK") != -1)
                {
                    strLOTID = Util.NVC(DataTableConverter.GetValue(_baseForm.PRODLOT_GRID.Rows[new Util().GetDataGridCheckFirstRowIndex(_baseForm.PRODLOT_GRID, "CHK")].DataItem, "LOTID_PR"));
                }

                ELEC001_013_LOTSTART LOTSTART = new ELEC001_013_LOTSTART(strPROCID, strEQSGID, strEQPTID, strLOTID);

                LOTSTART.Closed += new EventHandler(LotStart_Closed);
                this.Dispatcher.BeginInvoke(new Action(() => LOTSTART.ShowModal()));
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void LotStart_Closed(object sender, EventArgs e)
        {
            ELEC001_013_LOTSTART window = sender as ELEC001_013_LOTSTART;

            if (window.DialogResult == MessageBoxResult.OK)
                _baseForm.REFRESH = true;
        }

        private void ApplyPermissions()
        {
            _baseForm.FrameOperation = FrameOperation;

            List<Button> listAuth = new List<Button>();
            Util.pageAuth(listAuth, FrameOperation.AUTHORITY);
        }
    }
}