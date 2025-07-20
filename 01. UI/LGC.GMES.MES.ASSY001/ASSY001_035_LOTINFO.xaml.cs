/*************************************************************************************
 Created Date : 2018.02.02
      Creator : 이진선
   Decription :  VD검사이력조회
--------------------------------------------------------------------------------------
 [Change History]
  2017.01.14  이진선 : Initial Created.
**************************************************************************************/

using System;
using System.Data;
using System.Windows;
using C1.WPF;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using System.Collections.Generic;
using System.Linq;
using C1.WPF.DataGrid;

namespace LGC.GMES.MES.ASSY001
{
    public partial class ASSY001_035_LOTINFO : C1Window, IWorkArea
    {
        #region Declaration & Constructor        


        private string EQPT_BTCH_WRK_NO;
        private string EQPTID;
        private string LOTID_RT;
        private string VD_QA_INSP_COND_CODE;
        private string QA_TYPE;
        private string REWORK_TYPE;

        #endregion

        #region Initialize        
        /// <summary>
        /// Frame과 상호작용하기 위한 객체
        /// </summary>
        public IFrameOperation FrameOperation { get; set; }


        public ASSY001_035_LOTINFO()
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

                if (tmps != null)
                {
                    LOTID_RT = Util.NVC(tmps[0]);
                    EQPT_BTCH_WRK_NO = Util.NVC(tmps[1]);
                    VD_QA_INSP_COND_CODE = Util.NVC(tmps[2]);
                    EQPTID = Util.NVC(tmps[3]);
                    QA_TYPE = Util.NVC(tmps[4]);
                    REWORK_TYPE = Util.NVC(tmps[5]);
               }

                GetLot();

            }
            catch (Exception ex)
            {

            }
         }
        #endregion

        private void GetLot()
        {
            if (QA_TYPE.Equals("Y"))//자동검사 - 배치단위
            {
                GetLotByEqptBtch();
            }
            else //수동검사
            {
                if (VD_QA_INSP_COND_CODE.Equals("VD_QA_INSP_RULE_01")) //배치단위
                {
                    GetLotByEqptBtch();
                }
                else //대LOT단위
                {
                    if (REWORK_TYPE.Equals("REWORK_QA"))
                    {
                        GetLotByEqptBtch();
                    }
                    else if (REWORK_TYPE.Equals("REWORK_QA_L"))
                    {
                        //챔버내 장기재고
                        GetLotByEqptBtch("L");
                    }
                    else
                    {
                        GetLotByLotidRt();
                    }
                }
            }
        }

        private void GetLotByEqptBtch(string re_vd_type_code=null)
        {
            try
            {
                DataTable dt = new DataTable();
                dt.Columns.Add("LANGID", typeof(string));
                dt.Columns.Add("EQPT_BTCH_WRK_NO", typeof(string));
                dt.Columns.Add("RE_VD_TYPE_CODE", typeof(string));

                DataRow dr = dt.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["EQPT_BTCH_WRK_NO"] = EQPT_BTCH_WRK_NO;
                dr["RE_VD_TYPE_CODE"] = re_vd_type_code;

                dt.Rows.Add(dr);

                DataTable result = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_WIPHISTORYATTR_EQPT_BTCH_WRK_NO", "RQST", "RSLT", dt);

                Util.GridSetData(dgLotInfo, result, FrameOperation, true);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }

            
          }

        private void GetLotByLotidRt()
        {
            try
            {
                DataTable dt = new DataTable();
                dt.Columns.Add("LANGID", typeof(string));
                dt.Columns.Add("LOTID_RT", typeof(string));

                DataRow dr = dt.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["LOTID_RT"] = LOTID_RT;
                dt.Rows.Add(dr);

                DataTable result = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_LOT_VD_QA_HIST", "RQST", "RSLT", dt);

                Util.GridSetData(dgLotInfo, result, FrameOperation, true);

            }
            catch(Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void dgLotInfo_MergingCells(object sender, C1.WPF.DataGrid.DataGridMergingCellsEventArgs e)
        {
            try
            {
                if (dgLotInfo.GetRowCount() == 0) return;

                List<System.Data.DataRow> list = DataTableConverter.Convert(dgLotInfo.ItemsSource).Select().ToList();
                List<int> arr = list.GroupBy(c => c["EQPT_BTCH_WRK_NO"]).Select(group => group.Count()).ToList();


                int p = 0;
                for (int j = 0; j < arr.Count; j++)
                {
                    for (int i = 0; i < dgLotInfo.Columns.Count; i++)
                    {
                        if (dgLotInfo.Columns[i].Name.Equals("m"))
                        {
                            e.Merge(new DataGridCellsRange(dgLotInfo.GetCell(p, i), dgLotInfo.GetCell((p + arr[j] - 1), i)));

                        }
                    }
                    p += arr[j];
                }

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
    }
}