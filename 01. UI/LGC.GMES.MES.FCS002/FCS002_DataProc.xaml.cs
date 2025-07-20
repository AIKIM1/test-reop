/*************************************************************************************
 Created Date : 2021.07.15
      Creator : PSM
   Decription : DataProc
--------------------------------------------------------------------------------------
 [Change History]
  2021.07.15  NAME : Initial Created
**************************************************************************************/
using C1.WPF;
using C1.WPF.DataGrid;
using C1.WPF.DataGrid.Summaries;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using System;
using System.Collections.Generic;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using LGC.GMES.MES.CMM001;
using LGC.GMES.MES.CMM001.Extensions;
using System.Threading.Tasks;

namespace LGC.GMES.MES.FCS002
{
    public partial class FCS002_DataProc : UserControl, IWorkArea
    {
        #region [Declaration & Constructor]
        private DataTable dtTrayList = null;
        private bool bContinue = true;
        private bool bIng = false;
        private string sPGMID = string.Empty;

        #endregion

        #region [Initialize]
        public FCS002_DataProc()
        {
            InitializeComponent();
        }

        public IFrameOperation FrameOperation
        {
            get;
            set;
        }
        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            this.Loaded -= UserControl_Loaded; //2021.04.13 화면간 이동 시 초기화 현상 제거

            object[] parameters = this.FrameOperation.Parameters;
            if (parameters != null && parameters.Length >= 1)
            {
                dtTrayList = parameters[0] as DataTable;
                sPGMID = parameters[1].ToString();
            }
            SetData();
        }
        private void InitControl()
        {

        }
        #endregion

        #region [Method]

        public void SetData()
        {
            dtTrayList.Columns.Add("EXEC_DESC", typeof(string));
            Util.GridSetData(dgList, dtTrayList, this.FrameOperation);
            for (int i = 0; i < dtTrayList.Rows.Count; i++)
            {
                DataTableConverter.SetValue(dgList.Rows[i].DataItem, "EXEC_YN", "N");
            }
        }

        private async void Execute()
        {
            bIng = true;
            if (dtTrayList == null || dtTrayList.Rows.Count == 0)
            {
                Util.Alert("FM_ME_0240"); //처리할 데이터가 없습니다.
                btnStart.IsEnabled = true;
                return;
            }

            //PROGRESSBAR처리

            var task1 = Task.Run(() =>
                {
                    string sMsg = string.Empty;
                    string sResult = "Y";

                    DataTable dtRqst = new DataTable();
                    dtRqst = dtTrayList.Clone();

                    for (int i = 0; i < dtTrayList.Rows.Count; i++)
                    {
                        try
                        {
                            DataSet indataSet = new DataSet();
                            DataTable dtIndata = indataSet.Tables.Add("INDATA");
                            dtIndata.Columns.Add("USERID", typeof(string));
                            dtIndata.Columns.Add("EQPTID", typeof(string));

                            DataTable dtInCst = indataSet.Tables.Add("IN_CST");
                            dtInCst.Columns.Add("CSTID", typeof(string));
                            dtInCst.Columns.Add("PRE_ROUTID", typeof(string));
                            dtInCst.Columns.Add("AFTER_ROUTID", typeof(string));
                            dtInCst.Columns.Add("AFTER_PROCID", typeof(string));
                            DataRow InRow = dtIndata.NewRow();
                            DataRow RowCell = dtInCst.NewRow();

                            InRow["USERID"] = dtTrayList.Rows[i]["USERID"];
                            InRow["EQPTID"] = DBNull.Value;
                            dtIndata.Rows.Add(InRow);

                            RowCell["CSTID"] = dtTrayList.Rows[i]["CSTID"];
                            RowCell["PRE_ROUTID"] = dtTrayList.Rows[i]["PRE_ROUTID"];
                            RowCell["AFTER_ROUTID"] = dtTrayList.Rows[i]["AFTER_ROUTID"];
                            RowCell["AFTER_PROCID"] = dtTrayList.Rows[i]["AFTER_PROCID"];

                            dtInCst.Rows.Add(RowCell);

                            DataSet dsRslt = new ClientProxy().ExecuteServiceSync_Multi("BR_SET_TRAY_ROUTE_CHANGE_MB", "INDATA,IN_CST", "OUTDATA", indataSet);
                            sMsg = "OK";
                            sResult = "Y";
                        }
                        catch (Exception e)
                        {
                            sMsg = e.ToString();
                            sResult = "Y";
                        }
                        finally
                        {
                            SetResult(dtTrayList.Rows[i]["CSTID"].ToString(), sResult, sMsg, i);
                        }
                        DataTableConverter.SetValue(dgList.Rows[i].DataItem, "EXEC_YN", "Y");

                        if (!bContinue) //강제종료 시 현재 TASK 빠져나가기
                        {
                            return;
                        }
                    }
                });
            await task1;
            Clear();
            Util.MessageInfo("FM_ME_0239"); // 처리 완료되었습니다.
        }

        private void SetResult(string sTrayId, string sResult, string sMsg, int i)
        {
            if (sTrayId.Equals(Util.NVC(DataTableConverter.GetValue(dgList.Rows[i].DataItem, "CSTID"))))
            {
                DataTableConverter.SetValue(dgList.Rows[i].DataItem, "EXEC_YN", sResult);
                DataTableConverter.SetValue(dgList.Rows[i].DataItem, "EXEC_DESC", sMsg);
                //progressbar value 
            }
        }

        private void Clear()
        {
            bContinue = true;
            btnStart.IsEnabled = true;
            dtTrayList = null;
            bIng = false;
        }
        #endregion

        #region [Event]
        private void btnStart_Click(object sender, RoutedEventArgs e)
        {
            btnStart.IsEnabled = false;

            if (sPGMID.Equals("FCS002_005_ROUTE_CHG")) Execute();
        }

        // btnAbort
        #endregion
    }
}
