/*************************************************************************************
 Created Date : 2018.01.11
      Creator : 이진선
   Decription : 예상종료시간등록
--------------------------------------------------------------------------------------
 [Change History]
  2017.01.14  이진선 : Initial Created.
**************************************************************************************/

using System;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using C1.WPF;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;

namespace LGC.GMES.MES.ASSY001
{
    public partial class ASSY001_024_ENDTIME : C1Window, IWorkArea
    {
        #region Declaration & Constructor        


        string EQPT_BTCH_WRK_NO;
        string EQPTID;

        #endregion

        #region Initialize        
        /// <summary>
        /// Frame과 상호작용하기 위한 객체
        /// </summary>
        public IFrameOperation FrameOperation { get; set; }


        public ASSY001_024_ENDTIME()
        {
            InitializeComponent();
        }

        #endregion

        #region[Event]
        private void C1Window_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                object[] tmps = C1WindowExtension.GetParameters(this);

                if (tmps != null && tmps.Length >= 1)
                {
                    EQPTID = Util.NVC(tmps[0]);
                }

                GetEqptList();


            }
            catch (Exception ex)
            {

            }
         }

        private void btnInspEndTime_Click(object sender, RoutedEventArgs e)
        {
            DataSet ds = new DataSet();

            DataTable dt = new DataTable();
            dt.TableName = "IN_EQP";
            dt.Columns.Add("VD_SCHD_END_DTTM", typeof(DateTime));
            dt.Columns.Add("EQPT_BTCH_WRK_NO", typeof(string));
            ds.Tables.Add(dt);


            for (int i = 0; i < dgOperation.GetRowCount(); i++)
            {
                if (Util.NVC(DataTableConverter.GetValue(dgOperation.Rows[i].DataItem, "CHK")).Equals("1") || Util.NVC(DataTableConverter.GetValue(dgOperation.Rows[i].DataItem, "CHK")).Equals("True"))
                {

                    DataRow dr = dt.NewRow();
                    dr["VD_SCHD_END_DTTM"] = Util.NVC(DataTableConverter.GetValue(dgOperation.Rows[i].DataItem, "VD_SCHD_END_DTTM"));
                    dr["EQPT_BTCH_WRK_NO"] = Util.NVC(DataTableConverter.GetValue(dgOperation.Rows[i].DataItem, "EQPT_BTCH_WRK_NO"));
                    dt.Rows.Add(dr);
                }

            }

            new ClientProxy().ExecuteService_Multi("BR_PRD_REG_EXPECT_END_DTTM_VD", "IN_EQP", null, (bizResult, bizException) =>
            {
                try
                {
                    if (bizException != null)
                    {
                        Util.MessageException(bizException);
                        return;
                    }

                    Util.MessageInfo("SFU3369");//확정완료

                    //foreach (ASSY001_024 win in Util.FindVisualChildren<ASSY001_024>(Application.Current.MainWindow))
                    //{
                    //    win.REFRESH = true;
                    //    return;
                    //}

                    this.DialogResult = MessageBoxResult.OK;

                }
                catch (Exception ex)
                {
                    Util.MessageException(ex);
                }
            }, ds);


        }

        private void GetEqptList()
        {
            DataTable dt = new DataTable();
            dt.Columns.Add("EQPTID", typeof(string));
            dt.Columns.Add("PROCID", typeof(string));
            dt.Columns.Add("WIPSTAT", typeof(string));

            DataRow dr = dt.NewRow();
            dr["EQPTID"] = EQPTID;
            dr["PROCID"] = Process.VD_LMN;
            dr["WIPSTAT"] = Wip_State.PROC;
            dt.Rows.Add(dr);

            DataTable result = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_WIP_EQPTID", "INDATA", "OUTDATA", dt);

            Util.GridSetData(dgOperation, result, FrameOperation, false);

        }


     


        #endregion


    }
}