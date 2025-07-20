/*************************************************************************************
 Created Date : 2016.06.16
      Creator : 
   Decription : 
--------------------------------------------------------------------------------------
 [Change History]
  2016.06.16  DEVELOPER : Initial Created.





 
**************************************************************************************/

using C1.WPF;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using System;
using System.Collections.Generic;
using System.Data;
using System.Windows;
using System.Windows.Controls;

namespace LGC.GMES.MES.ASSY001
{
    public partial class ASSY001_002_FAULTY : C1Window, IWorkArea
    {
        #region Declaration & Constructor 

        private string _LineID = string.Empty;
        private string _EqptID = string.Empty;
        private string _LOTID = string.Empty;

        DataTable dtMain = new DataTable();
        Util _Util = new Util();
        /// <summary>
        ///  Frame과 상호작용하기 위한 객체
        /// </summary>

        public IFrameOperation FrameOperation
        {
            get;
            set;
        }

        public ASSY001_002_FAULTY()
        {
            InitializeComponent();
        }


        #endregion
        #region Initialize

        #endregion

        #region Event
        private void btnShift_Click(object sender, RoutedEventArgs e)
        {

            MES.CMM001.Popup.CMM_SHIFT wndPopup = new MES.CMM001.Popup.CMM_SHIFT();
            wndPopup.FrameOperation = FrameOperation;

            if (wndPopup != null)
            {
                object[] Parameters = new object[4];
                Parameters[0] = LoginInfo.CFG_SHOP_ID;
                Parameters[1] = LoginInfo.CFG_AREA_ID;
                Parameters[2] = LoginInfo.CFG_EQSG_ID;
                Parameters[3] = Process.NOTCHING;
                C1WindowExtension.SetParameters(wndPopup, Parameters);

                //wndPopup.Closed += new EventHandler(wndShift_Closed);
                this.Dispatcher.BeginInvoke(new Action(() => wndPopup.ShowModal()));
            }
        }

        private void btnWorker_Click(object sender, RoutedEventArgs e)
        {
        //    if (txtShift.Text.Trim().Equals(""))
        //    {
        //        // 선택된 작업조가 없습니다.
        //        LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("선택된 작업조가 없습니다."), null, "Warning", MessageBoxButton.OK, MessageBoxIcon.None);
        //        return;
        //    }

        //    CMM_SHIFT_USER wndPopup = new CMM_SHIFT_USER();
        //    wndPopup.FrameOperation = FrameOperation;

        //    if (wndPopup != null)
        //    {
        //        object[] Parameters = new object[5];
        //        Parameters[0] = LoginInfo.CFG_SHOP_ID;
        //        Parameters[1] = LoginInfo.CFG_AREA_ID;
        //        Parameters[2] = cboEquipmentSegment.SelectedValue.ToString();
        //        Parameters[3] = Process.NOTCHING;
        //        Parameters[4] = txtShift.Tag;
        //        C1WindowExtension.SetParameters(wndPopup, Parameters);

        //        wndPopup.Closed += new EventHandler(wndShiftUser_Closed);
        //        this.Dispatcher.BeginInvoke(new Action(() => wndPopup.ShowModal()));
        //    }
        }


        private void C1Window_Loaded(object sender, RoutedEventArgs e)
        {
            object[] tmps = C1WindowExtension.GetParameters(this);

            if (tmps != null && tmps.Length >= 3)
            {
                _LineID = Util.NVC(tmps[0]);
                _EqptID = Util.NVC(tmps[1]);
                _LOTID = Util.NVC(tmps[2]);
            }
            else
            {
                _LineID = "";
                _EqptID = "";
                _LOTID = "";
            }

            ApplyPermissions();

            GetLotInfo();
        }
        private void btnCommit_Click(object sender, RoutedEventArgs e)
        {
            SetDefect(_LOTID);
        }


        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }


        #endregion

        #region Mehod

        #endregion
        private void GetLotInfo()
        {

        }

        private void SetDefect(string LotID)
        {


            DataTable inDataTable = new DataTable();
            inDataTable.Columns.Add("LOTID", typeof(string));
            inDataTable.Columns.Add("PROCID", typeof(string));
            inDataTable.Columns.Add("RESNCODE", typeof(string));
            inDataTable.Columns.Add("RESNQTY", typeof(decimal));
            inDataTable.Columns.Add("USERID", typeof(string));

            DataRow inData = null;
            DataRowView rowview = null;

            foreach (C1.WPF.DataGrid.DataGridRow row in dgTrayList.Rows)
            {

                rowview = row.DataItem as DataRowView;

                inData = inDataTable.NewRow();

                inData["LOTID"] = _LOTID;
                inData["PROCID"] = Process.VD_LMN;
                if (!rowview["RESNQTY"].ToString().Equals(""))
                {
                    inData["RESNCODE"] = rowview["RESNCODE"].ToString();
                    inData["RESNQTY"] = Util.NVC_Decimal(rowview["RESNQTY"].ToString());

                }
                //inData["USERID"] = txtWorkPerson.Text;
                inDataTable.Rows.Add(inData);
            }

            new ClientProxy().ExecuteService("DA_PRD_INS_WIPREASONCOLLECT_ELEC", "INDATA", "RSLTDT", inDataTable, (result, ex) =>
            {
                if (ex != null)
                {
                    Util.MessageException(ex);
                    return;
                }

                //LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("저장완료"), null, "Info", MessageBoxButton.OK, MessageBoxIcon.Warning);
                Util.MessageInfo("SFU1270");

            });
        }



        private void ApplyPermissions()
        {
            List<Button> listAuth = new List<Button>();
            listAuth.Add(btnSave);

            Util.pageAuth(listAuth, FrameOperation.AUTHORITY);
        }
    }
}
