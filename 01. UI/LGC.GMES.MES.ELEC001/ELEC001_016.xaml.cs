/*****************************************
 Created Date : 2016.06.16
      Creator : 
   Decription : SRS 믹서 공정진척
------------------------------------------
 [Change History]
  2016.06.16  DEVELOPER : Initial Created.
******************************************/

using C1.WPF;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.CMM001.UserControls;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using System;
using System.Collections.Generic;
using System.Data;
using System.Windows;
using System.Windows.Controls;

namespace LGC.GMES.MES.ELEC001
{
    /// <summary>
    /// ELEC001_016.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class ELEC001_016 : IWorkArea
    {
        #region Declaration & Constructor
        UcBaseElec _baseForm;

        public IFrameOperation FrameOperation { get; set; }
        private Util _Util = new Util();
        public ELEC001_016()
        {
            InitializeComponent();
            InitInheritance();
        }

        void InitInheritance()
        {
            _baseForm = new UcBaseElec();
            grdMain.Children.Add(_baseForm);
            _baseForm.PROCID = Process.SRS_MIXING;
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

            dicParam.Add("PROCID", Process.SRS_MIXING);
            dicParam.Add("EQPTID", Util.NVC(_baseForm.EQUIPMENT_COMBO.SelectedValue));
            dicParam.Add("ELEC", "");//극성정보

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
            //    Parameters[0] = Process.SRS_MIXING;
            //    Parameters[1] = Util.NVC(_baseForm.EQUIPMENT_COMBO.SelectedValue);
            //    Parameters[2] = ""; // 극성
            //    C1WindowExtension.SetParameters(_LotStart, Parameters);

            //    _LotStart.Closed += new EventHandler(LotStart_Closed);
            //    _LotStart.ShowModal();
            //    _LotStart.CenterOnScreen();
            //}
        }

        private void LotStart_Closed(object sender, EventArgs e)
        {
            ELEC001_006_LOTSTART window = sender as ELEC001_006_LOTSTART;

            if (window.DialogResult == MessageBoxResult.OK)
                _baseForm.REFRESH = true;
        }
        #endregion

        #region Mothod
        private void Set_Combo_Eqpt(C1ComboBox cbo)
        {
            DataTable dtRQSTDT = new DataTable();
            dtRQSTDT.TableName = "RQSTDT";
            dtRQSTDT.Columns.Add("LANGID", typeof(string));

            DataRow drnewrow = dtRQSTDT.NewRow();
            drnewrow["LANGID"] = LoginInfo.LANGID;
            dtRQSTDT.Rows.Add(drnewrow);

            new ClientProxy().ExecuteService("DA_BAS_SEL_SRSMIX_EQPT_CBO", "RQSTDT", "RSLTDT", dtRQSTDT, (result, Exception) =>
            {
                if (Exception != null)
                {
                    Util.AlertByBiz("DA_BAS_SEL_SRSMIX_EQPT_CBO", Exception.Message, Exception.ToString());
                    return;
                }

                DataRow dRow = result.NewRow();
                dRow["CBO_NAME"] = "-SELECT-";
                dRow["CBO_CODE"] = "";
                result.Rows.InsertAt(dRow, 0);

                cbo.ItemsSource = DataTableConverter.Convert(result);
                //if ((from DataRow dr in result.Rows where dr["CBO_CODE"].Equals(selectedEquipment) select dr).Count() > 0)
                //{
                //    cbo.SelectedValue = selectedEquipment;
                //}
                //else if (result.Rows.Count > 0)
                //{
                //    cbo.SelectedIndex = 0;
                //}
                //else if (result.Rows.Count == 0)
                //{
                //    cbo.SelectedItem = null;
                //}
            });
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