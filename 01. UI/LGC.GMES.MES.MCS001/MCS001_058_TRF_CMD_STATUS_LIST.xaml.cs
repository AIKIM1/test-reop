/*************************************************************************************
 Created Date : 2024.02.06
      Creator : 안유수
   Decription : 반송지시현황 조회 List
--------------------------------------------------------------------------------------
  2025.04.30  이민형 : 조회 조건 인자값 추가 (db_conn, systemid)
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

namespace LGC.GMES.MES.MCS001
{
    public partial class MCS001_058_TRF_CMD_STATUS_LIST : C1Window, IWorkArea
    {

        #region Declaration & Constructor 

        private string _portid = string.Empty;
        private string _dbconn = string.Empty;
        private string _systemid = string.Empty;

        private string WH_ID;

        public MCS001_058_TRF_CMD_STATUS_LIST()
        {
            InitializeComponent();
        }
        public IFrameOperation FrameOperation
        {
            get;
            set;
        }
        private void C1Window_Loaded(object sender, RoutedEventArgs e)
        {
            object[] tmps = C1WindowExtension.GetParameters(this);

            if (tmps != null && tmps.Length >= 1)
            {
                _portid = Util.NVC(tmps[0]);
                _dbconn = Util.NVC(tmps[1]);
                _systemid = Util.NVC(tmps[2]);

                SeachData();
            }
        }
        #endregion

        #region Event

        #endregion

        #region Mehod
        private void SeachData()
        {
            DataTable RQSTDT = new DataTable("RQSTDT");
            RQSTDT.Columns.Add("PORT_ID", typeof(string));
            RQSTDT.Columns.Add("CONN_DB", typeof(string));
            RQSTDT.Columns.Add("SYSTEM_TYPE_CODE", typeof(string));
            
            DataRow dr = RQSTDT.NewRow();
            dr["PORT_ID"] = _portid;
            dr["CONN_DB"] = _dbconn;
            dr["SYSTEM_TYPE_CODE"] = _systemid;

            RQSTDT.Rows.Add(dr);

            DataTable dtRslt = new ClientProxy().ExecuteServiceSync("BR_MHS_SEL_TRF_CMD_BY_TO_PORT", "INDATA", "OUTDATA", RQSTDT);

            if (dtRslt.Rows.Count != 0)
            {
                Util.GridSetData(dgList, dtRslt, FrameOperation);
            }
        }
        #endregion

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = MessageBoxResult.Cancel;
        }
    }
}