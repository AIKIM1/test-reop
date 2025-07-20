/*************************************************************************************
 Created Date : 2016.08.22
      Creator : INS 김동일K
   Decription : 전지 5MEGA-GMES 구축 - BizDatSet 공통 클래스
--------------------------------------------------------------------------------------
 [Change History]
  2016.08.22  INS 김동일K : Initial Created.
  2019.12.05      이상준C : CNJ Cell 조립 - ZZS 공정대응 DataTable 추가 
  2024.12.23  이동주 : [MES팀] 모델 버전별 반제품/설비 CP revision으로 설비 및 레시피를 운영하기 위한 MES 개선 요청 건(CatchUp)
**************************************************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;

namespace LGC.GMES.MES.CMM001.Class
{
    public partial class BizDataSet
    {
        public BizDataSet()
        {
        }

        #region 공통
        public DataTable GetDA_PRD_SEL_PROCESS_FP_INFO()
        {
            DataTable inDataTable = new DataTable();
            inDataTable.Columns.Add("PROCID", typeof(string));

            return inDataTable;
        }

        public DataTable GetDA_PRD_SEL_WO()
        {
            DataTable inDataTable = new DataTable();
            inDataTable.Columns.Add("WOID", typeof(string));

            return inDataTable;
        }

        public DataTable GetDA_PRD_SEL_WORKORDER_LIST()
        {
            DataTable inDataTable = new DataTable();
            inDataTable.Columns.Add("LANGID", typeof(string));
            inDataTable.Columns.Add("PROCID", typeof(string));
            inDataTable.Columns.Add("EQSGID", typeof(string));
            inDataTable.Columns.Add("EQPTID", typeof(string));
            inDataTable.Columns.Add("STDT", typeof(string));
            inDataTable.Columns.Add("EDDT", typeof(string));

            return inDataTable;
        }

        public DataTable GetDA_PRD_SEL_WORKORDER_LIST_ELTR_ASSY()
        {
            DataTable inDataTable = new DataTable();
            inDataTable.Columns.Add("LANGID", typeof(string));
            inDataTable.Columns.Add("PROCID", typeof(string));
            inDataTable.Columns.Add("EQSGID", typeof(string));
            inDataTable.Columns.Add("EQPTID", typeof(string));
            inDataTable.Columns.Add("STDT", typeof(string));
            inDataTable.Columns.Add("EDDT", typeof(string));
            inDataTable.Columns.Add("PROC_EQPT_FLAG", typeof(string));

            return inDataTable;
        }

        public DataTable GetDA_PRD_SEL_WORKORDER_LIST_BY_PROCID()
        {
            DataTable inDataTable = new DataTable();
            inDataTable.Columns.Add("LANGID", typeof(string));
            inDataTable.Columns.Add("PROCID", typeof(string));
            inDataTable.Columns.Add("EQSGID", typeof(string));
            inDataTable.Columns.Add("EQPTID", typeof(string));
            inDataTable.Columns.Add("STDT", typeof(string));
            inDataTable.Columns.Add("EDDT", typeof(string));

            return inDataTable;
        }

        public DataTable GetDA_PRD_SEL_WORKORDER_SIDE_LIST()
        {
            DataTable inDataTable = new DataTable();
            inDataTable.Columns.Add("LANGID", typeof(string));
            inDataTable.Columns.Add("PROCID", typeof(string));
            inDataTable.Columns.Add("EQSGID", typeof(string));
            inDataTable.Columns.Add("EQPTID", typeof(string));
            inDataTable.Columns.Add("STDT", typeof(string));
            inDataTable.Columns.Add("EDDT", typeof(string));
            inDataTable.Columns.Add("COAT_SIDE_TYPE", typeof(string));

            return inDataTable;
        }

        public DataTable GetDA_PRD_SEL_WORKORDER_LIST_BY_LINE()
        {
            DataTable inDataTable = new DataTable();
            inDataTable.Columns.Add("LANGID", typeof(string));
            inDataTable.Columns.Add("PROCID", typeof(string));
            inDataTable.Columns.Add("EQSGID", typeof(string));
            inDataTable.Columns.Add("EQPTID", typeof(string));
            inDataTable.Columns.Add("STDT", typeof(string));
            inDataTable.Columns.Add("EDDT", typeof(string));
            inDataTable.Columns.Add("COAT_SIDE_TYPE", typeof(string));
            inDataTable.Columns.Add("OTHER_EQSGID", typeof(string));
            inDataTable.Columns.Add("PROC_EQPT_FLAG", typeof(string));

            return inDataTable;
        }

        public DataTable GetDA_PRD_SEL_WORKORDER_PLAN_DETAIL_BYEQPTID()
        {
            DataTable inDataTable = new DataTable();
            inDataTable.Columns.Add("EQPTID", typeof(string));

            return inDataTable;
        }

        public DataTable GetBR_PRD_REG_LABEL_HIST()
        {
            DataTable inDataTable = new DataTable();
            inDataTable.Columns.Add("LABEL_CODE", typeof(string));
            inDataTable.Columns.Add("LABEL_ZPL_CNTT", typeof(string));
            inDataTable.Columns.Add("LABEL_PRT_COUNT", typeof(string));
            inDataTable.Columns.Add("PRT_ITEM01", typeof(string));
            inDataTable.Columns.Add("PRT_ITEM02", typeof(string));
            inDataTable.Columns.Add("PRT_ITEM03", typeof(string));
            inDataTable.Columns.Add("PRT_ITEM04", typeof(string));
            inDataTable.Columns.Add("PRT_ITEM05", typeof(string));
            inDataTable.Columns.Add("PRT_ITEM06", typeof(string));
            inDataTable.Columns.Add("PRT_ITEM07", typeof(string));
            inDataTable.Columns.Add("PRT_ITEM08", typeof(string));
            inDataTable.Columns.Add("PRT_ITEM09", typeof(string));
            inDataTable.Columns.Add("PRT_ITEM10", typeof(string));
            inDataTable.Columns.Add("PRT_ITEM11", typeof(string));
            inDataTable.Columns.Add("PRT_ITEM12", typeof(string));
            inDataTable.Columns.Add("PRT_ITEM13", typeof(string));
            inDataTable.Columns.Add("PRT_ITEM14", typeof(string));
            inDataTable.Columns.Add("PRT_ITEM15", typeof(string));
            inDataTable.Columns.Add("PRT_ITEM16", typeof(string));
            inDataTable.Columns.Add("PRT_ITEM17", typeof(string));
            inDataTable.Columns.Add("PRT_ITEM18", typeof(string));
            inDataTable.Columns.Add("PRT_ITEM19", typeof(string));
            inDataTable.Columns.Add("PRT_ITEM20", typeof(string));
            inDataTable.Columns.Add("PRT_ITEM21", typeof(string));
            inDataTable.Columns.Add("PRT_ITEM22", typeof(string));
            inDataTable.Columns.Add("PRT_ITEM23", typeof(string));
            inDataTable.Columns.Add("PRT_ITEM24", typeof(string));
            inDataTable.Columns.Add("PRT_ITEM25", typeof(string));
            inDataTable.Columns.Add("PRT_ITEM26", typeof(string));
            inDataTable.Columns.Add("PRT_ITEM27", typeof(string));
            inDataTable.Columns.Add("PRT_ITEM28", typeof(string));
            inDataTable.Columns.Add("PRT_ITEM29", typeof(string));
            inDataTable.Columns.Add("PRT_ITEM30", typeof(string));
            inDataTable.Columns.Add("INSUSER", typeof(string));
            inDataTable.Columns.Add("LOTID", typeof(string));
            inDataTable.Columns.Add("PGM_ID", typeof(string));
            inDataTable.Columns.Add("BZRULE_ID", typeof(string));

            return inDataTable;
        }

        public DataSet GetBR_PRD_REG_END_CPROD_LOT()
        {
            DataSet indataSet = new DataSet();

            DataTable inDataTable1 = indataSet.Tables.Add("INDATA");
            inDataTable1.Columns.Add("USERID", typeof(string));
            inDataTable1.Columns.Add("CPROD_LOTID", typeof(string));
            inDataTable1.Columns.Add("MKT_TYPE_CODE", typeof(string));
            inDataTable1.Columns.Add("RECYC_QTY_FC", typeof(Decimal));

            DataTable inDataTable2 = indataSet.Tables.Add("IN_BC");
            inDataTable2.Columns.Add("PRODID", typeof(string));
            inDataTable2.Columns.Add("RECYC_QTY", typeof(Decimal));
            inDataTable2.Columns.Add("SCRP_QTY", typeof(Decimal));
            inDataTable2.Columns.Add("ADD_INPUT_QTY", typeof(Decimal));

            DataTable inDataTable3 = indataSet.Tables.Add("IN_FC");
            inDataTable3.Columns.Add("PRODID", typeof(string));
            inDataTable3.Columns.Add("RECYC_QTY", typeof(Decimal));
            inDataTable3.Columns.Add("SCRP_QTY", typeof(Decimal));
            inDataTable3.Columns.Add("ADD_INPUT_QTY", typeof(Decimal));

            return indataSet;
        }

        public DataTable GetDA_PRD_SEL_ERP_SEND_INFO()
        {
            DataTable inDataTable = new DataTable();
            inDataTable.Columns.Add("LOTID", typeof(string));
            inDataTable.Columns.Add("WIPSEQ", typeof(string));

            return inDataTable;
        }

        public DataTable GetDA_PRD_SEL_SET_WORKORDER_INFO()
        {
            DataTable inDataTable = new DataTable();
            inDataTable.Columns.Add("EQPTID", typeof(string));

            return inDataTable;
        }

        public DataSet GetBR_PRD_REG_MODIFY_LOT()
        {
            DataSet indataSet = new DataSet();

            DataTable inDataTable = indataSet.Tables.Add("INDATA");
            inDataTable.Columns.Add("AREAID", typeof(string));
            inDataTable.Columns.Add("USERID", typeof(string));

            DataTable out_LOT = indataSet.Tables.Add("IN_INPUT");
            out_LOT.Columns.Add("LOTID", typeof(string));
            out_LOT.Columns.Add("WIPSEQ", typeof(int));
            out_LOT.Columns.Add("WIPQTY_ED", typeof(int));
            out_LOT.Columns.Add("BONUSQTY", typeof(int));

            return indataSet;
        }

        public DataSet GetBR_PRD_REG_OUT_CPROD_LOT()
        {
            DataSet indataSet = new DataSet();

            DataTable inDataTable = indataSet.Tables.Add("INDATA");
            inDataTable.Columns.Add("USERID", typeof(string));

            DataTable in_lotTable = indataSet.Tables.Add("IN_LOT");
            in_lotTable.Columns.Add("CPROD_LOTID", typeof(string));

            return indataSet;
        }

        public DataTable GetDA_EQP_SEL_PROC_EQPT_PRDT_SET_ITEM_INFO()
        {
            DataTable inDataTable = new DataTable();
            inDataTable.Columns.Add("LANGID", typeof(string));
            inDataTable.Columns.Add("PROCID", typeof(string));
            inDataTable.Columns.Add("EQPTID", typeof(string));
            inDataTable.Columns.Add("LOTID", typeof(string));
            inDataTable.Columns.Add("EQSGID", typeof(string));
            inDataTable.Columns.Add("BEFORE_LOTID", typeof(string));

            return inDataTable;
        }

        public DataSet GetBR_QCA_REG_EQPT_DATA_CLCT()
        {
            DataSet indataSet = new DataSet();

            DataTable inDataTable = indataSet.Tables.Add("IN_EQP");
            inDataTable.Columns.Add("SRCTYPE", typeof(string));
            inDataTable.Columns.Add("IFMODE", typeof(string));
            inDataTable.Columns.Add("EQPTID", typeof(string));
            inDataTable.Columns.Add("UNIT_EQPTID", typeof(string));
            inDataTable.Columns.Add("USERID", typeof(string));
            inDataTable.Columns.Add("LOTID", typeof(string));
            //inDataTable.Columns.Add("SUBLOTID", typeof(string));
            inDataTable.Columns.Add("INPUT_SEQ_NO", typeof(int));
            //inDataTable.Columns.Add("EVENT_NAME", typeof(string));

            DataTable in_DATA = indataSet.Tables.Add("IN_DATA");
            in_DATA.Columns.Add("CLCTITEM", typeof(string));
            in_DATA.Columns.Add("CLCTITEM_VALUE01", typeof(string));

            return indataSet;
        }

        public DataTable GetDA_PRD_SEL_EQPT_CLCT_CNT()
        {
            DataTable inDataTable = new DataTable();
            inDataTable.Columns.Add("EQPTID", typeof(string));
            inDataTable.Columns.Add("LOTID", typeof(string));

            return inDataTable;
        }

        public DataTable GetDA_PRD_SEL_CANCEL_TERMINATE()
        {
            DataTable inDataTable = new DataTable();
            inDataTable.Columns.Add("PROCID", typeof(string));
            inDataTable.Columns.Add("LOTID", typeof(string));
            inDataTable.Columns.Add("WIP_TYPE_CODE", typeof(string));

            return inDataTable;
        }

        public DataSet GetBR_PRD_REG_CANCEL_TERMINATE_LOT()
        {
            DataSet indataSet = new DataSet();

            DataTable inDataTable = indataSet.Tables.Add("INDATA");
            inDataTable.Columns.Add("SRCTYPE", typeof(string));
            inDataTable.Columns.Add("USERID", typeof(string));

            DataTable in_DATA = indataSet.Tables.Add("INLOT");
            in_DATA.Columns.Add("LOTID", typeof(string));
            in_DATA.Columns.Add("LOTSTAT", typeof(string));
            in_DATA.Columns.Add("WIPQTY", typeof(int));
            in_DATA.Columns.Add("WIPQTY2", typeof(int));

            return indataSet;
        }

        public DataTable GetBR_EQP_REG_TESTMODE()
        {
            DataTable inDataTable = new DataTable();
            inDataTable.Columns.Add("SRCTYPE", typeof(string));
            inDataTable.Columns.Add("IFMODE", typeof(string));
            inDataTable.Columns.Add("EQPTID", typeof(string));
            inDataTable.Columns.Add("USERID", typeof(string));

            return inDataTable;
        }

        public DataTable GetDA_EQP_SEL_TESTMODE()
        {
            DataTable inDataTable = new DataTable();
            inDataTable.Columns.Add("EQPTID", typeof(string));

            return inDataTable;
        }
        //=====================================================================================================================
        // C20220822-000365 원통형 9,10호 실적 자동 확정 기능 구현
        //=====================================================================================================================
        public DataTable GetDA_EQP_SEL_AUTO_CONFIRMMODE()
        {
            DataTable inDataTable = new DataTable();
            inDataTable.Columns.Add("EQPTID", typeof(string));

            return inDataTable;
        }
        public DataSet GetBR_PRD_REG_IN_CPROD_LOT()
        {
            DataSet indataSet = new DataSet();

            DataTable inDataTable = indataSet.Tables.Add("INDATA");
            inDataTable.Columns.Add("CPROD_WRK_PSTN_ID", typeof(string));
            inDataTable.Columns.Add("USERID", typeof(string));

            DataTable in_DATA = indataSet.Tables.Add("IN_LOT");
            in_DATA.Columns.Add("CPROD_LOTID", typeof(string));
            in_DATA.Columns.Add("DFCT_QTY", typeof(decimal));

            return indataSet;
        }

        #endregion

        #region Cell 조립

        public DataTable GetDA_PRD_SEL_MBOM()
        {
            DataTable inDataTable = new DataTable();
            inDataTable.Columns.Add("WO_DETL_ID", typeof(string));
            inDataTable.Columns.Add("SHOPID", typeof(string));
            //inDataTable.Columns.Add("EQSGID", typeof(string));
            //inDataTable.Columns.Add("INPUT_PROCID", typeof(string));

            return inDataTable;
        }

        public DataTable GetDA_PRD_SEL_THERMAL_PAPER_PRT_INFO()
        {
            DataTable inDataTable = new DataTable();
            inDataTable.Columns.Add("AREAID", typeof(string));
            inDataTable.Columns.Add("LOTID", typeof(string));
            inDataTable.Columns.Add("LANGID", typeof(string));  // 2017.11.28 : 극성 출력을 위해 추가
            return inDataTable;
        }

        public DataTable GetUC_BR_PRD_REG_CURR_INPUT()
        {
            DataTable inDataTable = new DataTable();
            inDataTable.Columns.Add("EQPT_MOUNT_PSTN_ID", typeof(string));
            inDataTable.Columns.Add("EQPT_MOUNT_PSTN_STATE", typeof(string));
            inDataTable.Columns.Add("MTRLID", typeof(string));
            inDataTable.Columns.Add("INPUT_LOTID", typeof(string));
            inDataTable.Columns.Add("ACTQTY", typeof(int));
            inDataTable.Columns.Add("WIPNOTE", typeof(string));
            inDataTable.Columns.Add("INPUT_SEQNO", typeof(Int64));
            inDataTable.Columns.Add("CSTID", typeof(string));
            return inDataTable;
        }

        public DataTable GetBR_PRD_REG_END_INPUT_LOT_WS()
        {
            DataTable inDataTable = new DataTable();
            inDataTable.Columns.Add("SRCTYPE", typeof(string));
            inDataTable.Columns.Add("IFMODE", typeof(string));
            inDataTable.Columns.Add("EQPTID", typeof(string));
            inDataTable.Columns.Add("USERID", typeof(string));
            inDataTable.Columns.Add("EQPT_MOUNT_PSTN_ID", typeof(string));
            inDataTable.Columns.Add("EQPT_MOUNT_PSTN_STATE", typeof(string));
            inDataTable.Columns.Add("INPUT_LOTID", typeof(string));
            inDataTable.Columns.Add("PROD_LOTID", typeof(string));
            inDataTable.Columns.Add("EQPT_LOTID", typeof(string));
            return inDataTable;
        }

        public DataTable GetBR_PRD_REG_EQPT_END_INPUT_LOT_AS()
        {
            DataTable inDataTable = new DataTable();
            inDataTable.Columns.Add("SRCTYPE", typeof(string));
            inDataTable.Columns.Add("IFMODE", typeof(string));
            inDataTable.Columns.Add("EQPTID", typeof(string));
            inDataTable.Columns.Add("USERID", typeof(string));
            inDataTable.Columns.Add("EQPT_MOUNT_PSTN_ID", typeof(string));
            inDataTable.Columns.Add("EQPT_MOUNT_PSTN_STATE", typeof(string));
            inDataTable.Columns.Add("INPUT_LOTID", typeof(string));
            inDataTable.Columns.Add("PROD_LOTID", typeof(string));
            inDataTable.Columns.Add("EQPT_LOTID", typeof(string));
            return inDataTable;
        }

        public DataTable GetUC_DA_PRD_SEL_INPUT_HIST()
        {
            DataTable inDataTable = new DataTable();
            inDataTable.Columns.Add("LANGID", typeof(string));
            inDataTable.Columns.Add("EQPTID", typeof(string));
            inDataTable.Columns.Add("PROD_LOTID", typeof(string));
            inDataTable.Columns.Add("PROD_WIPSEQ", typeof(int));
            inDataTable.Columns.Add("INPUT_LOTID", typeof(string));
            inDataTable.Columns.Add("EQPT_MOUNT_PSTN_ID", typeof(string));

            return inDataTable;
        }

        public DataTable GetDA_BAS_SEL_TB_SFC_EQPT_CURR_MOUNT_MTRL_CBO()
        {
            DataTable inDataTable = new DataTable();

            inDataTable.Columns.Add("LANGID", typeof(string));
            inDataTable.Columns.Add("EQPTID", typeof(string));
            inDataTable.Columns.Add("MTGRID", typeof(string));

            return inDataTable;
        }

        public DataTable GetDA_EQP_REG_EQPT_NOTE()
        {
            DataTable inDataTable = new DataTable();
            inDataTable.Columns.Add("EQPTID", typeof(string));
            inDataTable.Columns.Add("WRK_DATE", typeof(string));
            inDataTable.Columns.Add("SHFT_ID", typeof(string));
            inDataTable.Columns.Add("EQPT_NOTE", typeof(string));
            inDataTable.Columns.Add("REG_USERID", typeof(string));
            inDataTable.Columns.Add("USERID", typeof(string));

            return inDataTable;
        }

        public DataTable GetBR_PRD_GET_CALDATE()
        {
            DataTable inDataTable = new DataTable();
            inDataTable.Columns.Add("EQPTID", typeof(string));

            return inDataTable;
        }

        public DataTable GetDA_EQP_SEL_EQPT_NOTE()
        {
            DataTable inDataTable = new DataTable();
            inDataTable.Columns.Add("EQPTID", typeof(string));
            inDataTable.Columns.Add("WRK_DATE", typeof(string));
            inDataTable.Columns.Add("SHFT_ID", typeof(string));

            return inDataTable;
        }

        public DataTable GetDA_EQP_SEL_EQPT_COMMENT()
        {
            DataTable inDataTable = new DataTable();
            inDataTable.Columns.Add("LANGID", typeof(string));
            inDataTable.Columns.Add("EQPTID", typeof(string));
            inDataTable.Columns.Add("WRK_DATE", typeof(string));
            inDataTable.Columns.Add("AREAID", typeof(string));
            inDataTable.Columns.Add("EQSGID", typeof(string));
            inDataTable.Columns.Add("PROCID", typeof(string));
            inDataTable.Columns.Add("SHOPID", typeof(string));

            return inDataTable;
        }

        public DataSet GetBR_PRD_REG_INPUT_LOT()
        {
            DataSet indataSet = new DataSet();

            DataTable inDataTable = indataSet.Tables.Add("INDATA");
            inDataTable.Columns.Add("SRCTYPE", typeof(string));
            inDataTable.Columns.Add("IFMODE", typeof(string));
            inDataTable.Columns.Add("INPUT_EVENT_CODE", typeof(string));
            inDataTable.Columns.Add("EQPTID", typeof(string));
            inDataTable.Columns.Add("PROCID", typeof(string));
            inDataTable.Columns.Add("PROD_LOTID", typeof(string));
            inDataTable.Columns.Add("OUT_LOTID", typeof(string));
            inDataTable.Columns.Add("WOID", typeof(string));
            inDataTable.Columns.Add("WO_DETL_ID", typeof(string));
            inDataTable.Columns.Add("USERID", typeof(string));

            DataTable inMtrl = indataSet.Tables.Add("IN_INPUT");
            inMtrl.Columns.Add("EQPT_MOUNT_PSTN_ID", typeof(string));
            inMtrl.Columns.Add("EQPT_MOUNT_PSTN_STATE", typeof(string));
            inMtrl.Columns.Add("MTRLID", typeof(string));
            inMtrl.Columns.Add("INPUT_LOTID", typeof(string));
            inMtrl.Columns.Add("ACTQTY", typeof(int));

            return indataSet;
        }

        public DataSet GetBR_PRD_CHK_INPUT_LOT()
        {
            DataSet indataSet = new DataSet();

            DataTable inDataTable = indataSet.Tables.Add("INDATA");
            inDataTable.Columns.Add("SRCTYPE", typeof(string));
            inDataTable.Columns.Add("IFMODE", typeof(string));
            inDataTable.Columns.Add("EQPTID", typeof(string));
            inDataTable.Columns.Add("PROCID", typeof(string));
            inDataTable.Columns.Add("WOID", typeof(string));
            inDataTable.Columns.Add("WO_DETL_ID", typeof(string));
            inDataTable.Columns.Add("USERID", typeof(string));

            DataTable inMtrl = indataSet.Tables.Add("IN_INPUT");
            inMtrl.Columns.Add("EQPT_MOUNT_PSTN_ID", typeof(string));
            inMtrl.Columns.Add("EQPT_MOUNT_PSTN_STATE", typeof(string));
            inMtrl.Columns.Add("MTRLID", typeof(string));
            inMtrl.Columns.Add("INPUT_LOTID", typeof(string));
            inMtrl.Columns.Add("ACTQTY", typeof(int));

            return indataSet;
        }

        public DataTable GetDA_PRD_SEL_WIPINFO_NT()
        {
            DataTable inDataTable = new DataTable();
            inDataTable.Columns.Add("LANGID", typeof(string));
            inDataTable.Columns.Add("PROCID", typeof(string));
            //inDataTable.Columns.Add("WOID", typeof(string));
            inDataTable.Columns.Add("EQSGID", typeof(string));
            inDataTable.Columns.Add("EQPTID", typeof(string));
            inDataTable.Columns.Add("WIPSTAT", typeof(string));

            return inDataTable;
        }

        public DataTable GetDA_PRD_SEL_TERM_LOT_LIST_NT()
        {
            DataTable inDataTable = new DataTable();
            inDataTable.Columns.Add("LANGID", typeof(string));
            inDataTable.Columns.Add("PROCID", typeof(string));
            inDataTable.Columns.Add("EQSGID", typeof(string));
            inDataTable.Columns.Add("EQPTID", typeof(string));
            inDataTable.Columns.Add("LOTID", typeof(string));
            inDataTable.Columns.Add("MODLID", typeof(string));

            return inDataTable;
        }

        public DataTable GetDA_PRD_SEL_MIN_CHILD_GR_SEQ_NT()
        {
            DataTable inDataTable = new DataTable();
            inDataTable.Columns.Add("PROCID", typeof(string));
            inDataTable.Columns.Add("PR_LOTID", typeof(string));

            return inDataTable;
        }

        public DataTable GetDA_PRD_SEL_CHILDINFO_NT()
        {
            DataTable inDataTable = new DataTable();
            inDataTable.Columns.Add("LANGID", typeof(string));
            inDataTable.Columns.Add("PR_LOTID", typeof(string));
            inDataTable.Columns.Add("PROCID", typeof(string));
            inDataTable.Columns.Add("AREAID", typeof(string));
            inDataTable.Columns.Add("CHILD_GR_SEQNO", typeof(int));
            inDataTable.Columns.Add("EQPTID", typeof(string));

            return inDataTable;
        }

        public DataSet GetBR_PRD_REG_RESURRECT()
        {
            DataSet indataSet = new DataSet();

            DataTable inDataTable = indataSet.Tables.Add("INDATA");
            inDataTable.Columns.Add("SRCTYPE", typeof(string));
            inDataTable.Columns.Add("IFMODE", typeof(string));
            inDataTable.Columns.Add("USERID", typeof(string));

            DataTable inMtrl = indataSet.Tables.Add("IN_INPUT");
            inMtrl.Columns.Add("LOTID", typeof(string));
            inMtrl.Columns.Add("WIPQTY", typeof(string));
            inMtrl.Columns.Add("WIPSEQ", typeof(int));

            return indataSet;
        }

        public DataTable GetDA_PRD_SEL_LOT_REMARK()
        {
            DataTable inDataTable = new DataTable();
            inDataTable.Columns.Add("LANGID", typeof(string));
            inDataTable.Columns.Add("LOTID", typeof(string));
            inDataTable.Columns.Add("WIPSEQ", typeof(string));

            return inDataTable;
        }

        public DataTable GetDA_PRD_UPD_LOT_REMARK()
        {
            DataTable inDataTable = new DataTable();
            inDataTable.Columns.Add("LOTID", typeof(string));
            inDataTable.Columns.Add("WIPSEQ", typeof(string));
            inDataTable.Columns.Add("WIP_NOTE", typeof(string));
            inDataTable.Columns.Add("USERID", typeof(string));

            return inDataTable;
        }

        public DataTable GetDA_PRD_SEL_INPUTLOT_INFO_NT()
        {
            DataTable inDataTable = new DataTable();
            inDataTable.Columns.Add("LANGID", typeof(string));
            inDataTable.Columns.Add("LOTID", typeof(string));
            inDataTable.Columns.Add("EQPTID", typeof(string));

            return inDataTable;
        }

        public DataTable GetDA_PRD_SEL_RSLTLOT_INFO_NT()
        {
            DataTable inDataTable = new DataTable();
            inDataTable.Columns.Add("LANGID", typeof(string));
            inDataTable.Columns.Add("PR_LOTID", typeof(string));
            inDataTable.Columns.Add("PROCID", typeof(string));

            return inDataTable;
        }

        public DataTable GetDA_PROD_SEL_ACTVITYREASON_CODE()
        {
            DataTable inDataTable = new DataTable();
            inDataTable.Columns.Add("LANGID", typeof(string));
            inDataTable.Columns.Add("AREAID", typeof(string));
            inDataTable.Columns.Add("PROCID", typeof(string));
            inDataTable.Columns.Add("ACTID", typeof(string));
            inDataTable.Columns.Add("EQPTID", typeof(string));

            return inDataTable;
        }

        public DataTable GetDA_QCA_SEL_WIPRESONCOLLECT()
        {
            DataTable inDataTable = new DataTable();
            inDataTable.Columns.Add("LANGID", typeof(string));
            inDataTable.Columns.Add("AREAID", typeof(string));
            inDataTable.Columns.Add("PROCID", typeof(string));
            inDataTable.Columns.Add("EQPTID", typeof(string));
            inDataTable.Columns.Add("LOTID", typeof(string));
            inDataTable.Columns.Add("WIPSEQ", typeof(string));
            inDataTable.Columns.Add("ACTID", typeof(string));

            return inDataTable;
        }

        public DataTable GetDA_QCA_SEL_QUALITY_PIVOT()
        {
            DataTable inDataTable = new DataTable();
            inDataTable.Columns.Add("LANGID", typeof(string));
            inDataTable.Columns.Add("PROCID", typeof(string));
            inDataTable.Columns.Add("LOTID", typeof(string));
            inDataTable.Columns.Add("WIPSEQ", typeof(string));

            return inDataTable;
        }

        public DataSet GetBR_PRD_REG_EQPEND_NT()
        {
            DataSet indataSet = new DataSet();

            DataTable inDataTable = indataSet.Tables.Add("IN_EQP");
            inDataTable.Columns.Add("SRCTYPE", typeof(string));
            inDataTable.Columns.Add("IFMODE", typeof(string));
            inDataTable.Columns.Add("EQPTID", typeof(string));
            inDataTable.Columns.Add("USERID", typeof(string));
            inDataTable.Columns.Add("END_DTTM", typeof(DateTime));

            DataTable inMtrl = indataSet.Tables.Add("IN_INPUT");
            inMtrl.Columns.Add("EQPT_MOUNT_PSTN_ID", typeof(string));
            inMtrl.Columns.Add("EQPT_MOUNT_PSTN_STATE", typeof(string));
            inMtrl.Columns.Add("INPUT_LOTID", typeof(string));
            inMtrl.Columns.Add("INPUT_QTY", typeof(int));
            inMtrl.Columns.Add("OUTPUT_QTY", typeof(int));

            return indataSet;
        }

        public DataTable GetDA_PRD_WIP_INFO()
        {
            DataTable inDataTable = new DataTable();
            inDataTable.Columns.Add("LANGID", typeof(string));
            inDataTable.Columns.Add("LOTID", typeof(string));

            return inDataTable;
        }

        public DataTable GetBR_PRD_REG_WIPSTATCHG_NT()
        {
            DataTable inDataTable = new DataTable();
            inDataTable.Columns.Add("LANGID", typeof(string));
            inDataTable.Columns.Add("LOTID", typeof(string));
            inDataTable.Columns.Add("OUTQTY", typeof(int));

            return inDataTable;
        }

        public DataTable GetDA_PRD_SEL_LOT_NOTE_HISTORY()
        {
            DataTable inDataTable = new DataTable();
            inDataTable.Columns.Add("LANGID", typeof(string));
            inDataTable.Columns.Add("LOTID", typeof(string));

            return inDataTable;
        }

        public DataTable GetBR_PRD_GET_NEW_LOT_NT()
        {
            DataTable inDataTable = new DataTable();
            inDataTable.Columns.Add("SRCTYPE", typeof(string));
            inDataTable.Columns.Add("EQPTID", typeof(string));
            inDataTable.Columns.Add("LOTID_MO", typeof(string));
            inDataTable.Columns.Add("USERID", typeof(string));

            return inDataTable;
        }

        public DataSet GetBR_PRD_REG_START_LOT_NT()
        {
            DataSet indataSet = new DataSet();

            DataTable inDataTable = indataSet.Tables.Add("IN_EQP");
            inDataTable.Columns.Add("SRCTYPE", typeof(string));
            inDataTable.Columns.Add("IFMODE", typeof(string));
            inDataTable.Columns.Add("EQPTID", typeof(string));
            //inDataTable.Columns.Add("LOTID", typeof(string));
            //inDataTable.Columns.Add("WO_DETL_ID", typeof(string));
            inDataTable.Columns.Add("USERID", typeof(string));

            DataTable inMtrl = indataSet.Tables.Add("IN_INPUT");
            inMtrl.Columns.Add("INPUT_LOTID", typeof(string));
            inMtrl.Columns.Add("EQPT_MOUNT_PSTN_STATE", typeof(string));
            inMtrl.Columns.Add("EQPT_MOUNT_PSTN_ID", typeof(string));
            //inMtrl.Columns.Add("LOT_TYPE_CODE", typeof(string));

            return indataSet;
        }

        public DataTable GetBR_PRD_REG_CNL_START_LOT_NT()
        {
            DataTable inDataTable = new DataTable();
            inDataTable.Columns.Add("SRCTYPE", typeof(string));
            inDataTable.Columns.Add("EQPTID", typeof(string));
            inDataTable.Columns.Add("LOTID", typeof(string));
            inDataTable.Columns.Add("INPUT_LOTID", typeof(string));
            inDataTable.Columns.Add("USERID", typeof(string));
            inDataTable.Columns.Add("IFMODE", typeof(string));

            return inDataTable;
        }

        public DataTable GetDA_PRD_SEL_LOT_CHILD()
        {
            DataTable inDataTable = new DataTable();
            inDataTable.Columns.Add("LANGID", typeof(string));
            inDataTable.Columns.Add("LOTID", typeof(string));

            return inDataTable;
        }

        public DataSet GetBR_PRD_REG_CONFIRM_LOT_NT()
        {
            DataSet indataSet = new DataSet();

            DataTable inDataTable = indataSet.Tables.Add("INDATA");
            inDataTable.Columns.Add("SRCTYPE", typeof(string));
            inDataTable.Columns.Add("IFMODE", typeof(string));
            inDataTable.Columns.Add("EQPTID", typeof(string));
            inDataTable.Columns.Add("PROD_VER_CODE", typeof(string));
            inDataTable.Columns.Add("SHIFT", typeof(string));
            inDataTable.Columns.Add("WIPDTTM_ED", typeof(DateTime));
            inDataTable.Columns.Add("WRK_USERID", typeof(string));
            inDataTable.Columns.Add("WRK_USER_NAME", typeof(string));
            inDataTable.Columns.Add("WIPNOTE", typeof(string));
            inDataTable.Columns.Add("USERID", typeof(string));
            //inDataTable.Columns.Add("CUTYN", typeof(string));
            inDataTable.Columns.Add("REMAINQTY", typeof(int));

            DataTable inPRLot = indataSet.Tables.Add("IN_INPUT");
            inPRLot.Columns.Add("INPUT_LOTID", typeof(string));
            inPRLot.Columns.Add("EQPT_MOUNT_PSTN_ID", typeof(string));
            inPRLot.Columns.Add("EQPT_MOUNT_PSTN_STATE", typeof(string));

            DataTable inLot = indataSet.Tables.Add("IN_LOT");
            inLot.Columns.Add("LOTID", typeof(string));
            inLot.Columns.Add("INPUTQTY", typeof(int));
            inLot.Columns.Add("OUTPUTQTY", typeof(int));
            inLot.Columns.Add("RESNQTY", typeof(int));

            return indataSet;
        }

        public DataTable GetBR_PRD_REG_SEND_MSG_NT()
        {
            DataTable inDataTable = new DataTable();
            inDataTable.Columns.Add("LANGID", typeof(string));
            inDataTable.Columns.Add("LOTID", typeof(string));

            return inDataTable;
        }

        //public DataTable GetBR_PRD_REG_DEFECT_NT()
        //{
        //    DataTable inDataTable = new DataTable();
        //    inDataTable.Columns.Add("LANGID", typeof(string));
        //    inDataTable.Columns.Add("LOTID", typeof(string));

        //    return inDataTable;
        //}

        public DataTable GetDA_EQP_SEL_EQPTDFCT_INFO()
        {
            DataTable inDataTable = new DataTable();
            inDataTable.Columns.Add("LANGID", typeof(string));
            inDataTable.Columns.Add("EQPTID", typeof(string));
            inDataTable.Columns.Add("LOTID", typeof(string));
            inDataTable.Columns.Add("WIPSEQ", typeof(string));

            return inDataTable;
        }

        public DataTable GetDA_PRD_SEL_MAXCHILDGRSEQ()
        {
            DataTable inDataTable = new DataTable();
            inDataTable.Columns.Add("PR_LOTID", typeof(string));

            return inDataTable;
        }

        public DataTable GetBR_PRD_REG_CHGFCUT_NT()
        {
            DataTable inDataTable = new DataTable();
            inDataTable.Columns.Add("LOTID", typeof(string));

            return inDataTable;
        }

        public DataTable GetDA_PRD_SEL_CHILDEQPTQTY_INFO()
        {
            DataTable inDataTable = new DataTable();
            inDataTable.Columns.Add("LOTID", typeof(string));
            inDataTable.Columns.Add("EQPTID", typeof(string));

            return inDataTable;
        }

        public DataTable GetDA_COM_SEL_NOWSHIFT_INFO()
        {
            DataTable inDataTable = new DataTable();
            inDataTable.Columns.Add("SHOPID", typeof(string));
            inDataTable.Columns.Add("AREAID", typeof(string));
            inDataTable.Columns.Add("EQSGID", typeof(string));
            inDataTable.Columns.Add("PROCID", typeof(string));

            return inDataTable;
        }

        public DataTable GetBR_PRD_GET_PROCESS_LOT_LABEL_NT()
        {
            DataTable inDataTable = new DataTable();
            inDataTable.Columns.Add("LANGID", typeof(string));
            inDataTable.Columns.Add("PROCID", typeof(string));
            inDataTable.Columns.Add("EQPTID", typeof(string));
            inDataTable.Columns.Add("LOTID", typeof(string));
            inDataTable.Columns.Add("WIPSEQ", typeof(string));
            inDataTable.Columns.Add("PRMK", typeof(string));
            inDataTable.Columns.Add("RESO", typeof(string));
            inDataTable.Columns.Add("PRCN", typeof(string));
            inDataTable.Columns.Add("MARH", typeof(string));
            inDataTable.Columns.Add("MARV", typeof(string));
            inDataTable.Columns.Add("DARK", typeof(string));
            inDataTable.Columns.Add("LBCD", typeof(string));
            inDataTable.Columns.Add("NT_WAIT_YN", typeof(string));

            return inDataTable;
        }

        public DataTable GetDA_PRD_SEL_IN_LOT_VALID_NT()
        {
            DataTable inDataTable = new DataTable();
            inDataTable.Columns.Add("SRCTYPE", typeof(string));
            inDataTable.Columns.Add("IFMODE", typeof(string));
            inDataTable.Columns.Add("EQPTID", typeof(string));
            inDataTable.Columns.Add("LOTID", typeof(string));
            inDataTable.Columns.Add("USERID", typeof(string));

            return inDataTable;
        }

        public DataTable GetDA_PRD_SEL_DEFECT_DTL()
        {
            DataTable inDataTable = new DataTable();
            inDataTable.Columns.Add("INPUTQTY", typeof(int));
            inDataTable.Columns.Add("ALPHAQTY_P", typeof(int));
            inDataTable.Columns.Add("ALPHAQTY_M", typeof(int));
            inDataTable.Columns.Add("OUTPUTQTY", typeof(int));
            inDataTable.Columns.Add("GOODQTY", typeof(int));
            inDataTable.Columns.Add("DTL_DEFECT", typeof(int));
            inDataTable.Columns.Add("DTL_LOSS", typeof(int));
            inDataTable.Columns.Add("DTL_CHARGEPRD", typeof(int));
            inDataTable.Columns.Add("DEFECTQTY", typeof(int));

            return inDataTable;
        }

        public DataTable GetDA_PRD_SEL_WAIT_LIST_NT()
        {
            DataTable inDataTable = new DataTable();
            inDataTable.Columns.Add("LANGID", typeof(string));
            inDataTable.Columns.Add("PROCID", typeof(string));
            inDataTable.Columns.Add("EQSGID", typeof(string));
            inDataTable.Columns.Add("WOID", typeof(string));
            inDataTable.Columns.Add("PRDT_CLSS_CODE", typeof(string));
            inDataTable.Columns.Add("LOTID", typeof(string));

            return inDataTable;
        }

        public DataTable GetDA_PRD_SEL_LOT_HISTORY_NT()
        {
            DataTable inDataTable = new DataTable();
            inDataTable.Columns.Add("LANGID", typeof(string));
            inDataTable.Columns.Add("PROCID", typeof(string));
            inDataTable.Columns.Add("EQSGID", typeof(string));
            inDataTable.Columns.Add("EQPTID", typeof(string));
            inDataTable.Columns.Add("LOTID", typeof(string));
            inDataTable.Columns.Add("STDT", typeof(string));
            inDataTable.Columns.Add("EDDT", typeof(string));

            return inDataTable;
        }

        public DataTable GetDA_PRD_SEL_RUN_WORKORDER_INFO()
        {
            DataTable inDataTable = new DataTable();
            inDataTable.Columns.Add("LANGID", typeof(string));
            inDataTable.Columns.Add("PROCID", typeof(string));
            inDataTable.Columns.Add("EQPTID", typeof(string));
            inDataTable.Columns.Add("EQSGID", typeof(string));

            return inDataTable;
        }

        public DataTable GetBR_PRD_REG_EIO_WO_DETL_ID()
        {
            DataTable inDataTable = new DataTable();
            inDataTable.Columns.Add("SRCTYPE", typeof(string));
            inDataTable.Columns.Add("EQPTID", typeof(string));
            inDataTable.Columns.Add("WO_DETL_ID", typeof(string));
            inDataTable.Columns.Add("CP_VER_CODE", typeof(string));
            inDataTable.Columns.Add("USERID", typeof(string));

            return inDataTable;
        }

        public DataTable GetDA_PRD_SEL_WIPINFO_LM()
        {
            DataTable inDataTable = new DataTable();
            inDataTable.Columns.Add("LANGID", typeof(string));
            inDataTable.Columns.Add("PROCID", typeof(string));
            inDataTable.Columns.Add("EQPTID", typeof(string));
            inDataTable.Columns.Add("EQSGID", typeof(string));
            inDataTable.Columns.Add("WOID", typeof(string));
            inDataTable.Columns.Add("WIPSTAT", typeof(string));

            return inDataTable;
        }

        public DataTable GetDA_PRD_SEL_RUN_IN_LOT_LM()
        {
            DataTable inDataTable = new DataTable();
            inDataTable.Columns.Add("LANGID", typeof(string));
            inDataTable.Columns.Add("LOTID", typeof(string));
            inDataTable.Columns.Add("WIPSEQ", typeof(string));
            inDataTable.Columns.Add("INPUT_LOT_STAT_CODE", typeof(string));
            inDataTable.Columns.Add("INPUT_LOT_TYPE_CODE", typeof(string));

            return inDataTable;
        }

        public DataTable GetDA_PRD_SEL_CURR_IN_LOT_LM()
        {
            DataTable inDataTable = new DataTable();
            inDataTable.Columns.Add("LANGID", typeof(string));
            inDataTable.Columns.Add("PROCID", typeof(string));
            inDataTable.Columns.Add("EQSGID", typeof(string));
            inDataTable.Columns.Add("EQPTID", typeof(string));
            inDataTable.Columns.Add("PROD_LOTID", typeof(string));
            inDataTable.Columns.Add("PROD_WIPSEQ", typeof(int));

            return inDataTable;
        }

        public DataTable GetDA_PRD_SEL_READY_LOT_LM()
        {
            DataTable inDataTable = new DataTable();
            inDataTable.Columns.Add("LANGID", typeof(string));
            inDataTable.Columns.Add("PROCID", typeof(string));
            inDataTable.Columns.Add("EQSGID", typeof(string));
            inDataTable.Columns.Add("EQPTID", typeof(string));
            inDataTable.Columns.Add("WOID", typeof(string));
            //inDataTable.Columns.Add("PRDT_CLSS_CODE", typeof(string));
            inDataTable.Columns.Add("LOTID", typeof(string));
            inDataTable.Columns.Add("IN_LOTID", typeof(string));
            inDataTable.Columns.Add("INPUT_MTRL_CLSS_CODE", typeof(string));

            return inDataTable;
        }

        public DataTable GetDA_PRD_SEL_OUT_MAGAZINE_LM()
        {
            DataTable inDataTable = new DataTable();
            inDataTable.Columns.Add("LANGID", typeof(string));
            //inDataTable.Columns.Add("PROCID", typeof(string));
            //inDataTable.Columns.Add("EQSGID", typeof(string));
            //inDataTable.Columns.Add("EQPTID", typeof(string));
            inDataTable.Columns.Add("PR_LOTID", typeof(string));

            return inDataTable;
        }

        public DataTable GetDA_PRD_SEL_IN_MTRL_LM()
        {
            DataTable inDataTable = new DataTable();
            inDataTable.Columns.Add("LANGID", typeof(string));
            inDataTable.Columns.Add("LOTID", typeof(string));
            inDataTable.Columns.Add("WIPSEQ", typeof(string));

            return inDataTable;
        }

        public DataTable GetDA_PRD_SEL_READY_LOT_BY_PROD_LM()
        {
            DataTable inDataTable = new DataTable();
            inDataTable.Columns.Add("LANGID", typeof(string));
            inDataTable.Columns.Add("PROCID", typeof(string));
            inDataTable.Columns.Add("EQSGID", typeof(string));
            inDataTable.Columns.Add("EQPTID", typeof(string));
            inDataTable.Columns.Add("WOID", typeof(string));
            inDataTable.Columns.Add("POSITION", typeof(string));
            inDataTable.Columns.Add("LOTID", typeof(string));

            return inDataTable;
        }

        public DataSet GetBR_PRD_SEL_IN_LOT_VALID_LM()
        {
            DataSet indataSet = new DataSet();
            DataTable inDataTable = indataSet.Tables.Add("IN_EQP");
            inDataTable.Columns.Add("SRCTYPE", typeof(string));
            inDataTable.Columns.Add("IFMODE", typeof(string));
            inDataTable.Columns.Add("EQPTID", typeof(string));
            inDataTable.Columns.Add("PROD_LOTID", typeof(string));
            inDataTable.Columns.Add("USERID", typeof(string));

            DataTable inInputTable = indataSet.Tables.Add("IN_INPUT");
            inInputTable.Columns.Add("EQPT_MOUNT_PSTN_ID", typeof(string));
            inInputTable.Columns.Add("EQPT_MOUNT_PSTN_STATE", typeof(string));
            inInputTable.Columns.Add("INPUT_LOTID", typeof(string));

            return indataSet;
        }

        public DataTable GetDA_PRD_SEL_INPUT_MTRL_CLSS_CODE()
        {
            DataTable inDataTable = new DataTable();
            inDataTable.Columns.Add("LANGID", typeof(string));
            inDataTable.Columns.Add("EQPTID", typeof(string));
            inDataTable.Columns.Add("EQPT_MOUNT_PSTN_ID", typeof(string));

            return inDataTable;
        }

        public DataTable GetBR_PRD_REG_INPUT_LM()
        {
            DataTable inDataTable = new DataTable();
            inDataTable.Columns.Add("LANGID", typeof(string));
            inDataTable.Columns.Add("PROCID", typeof(string));
            inDataTable.Columns.Add("EQSGID", typeof(string));
            inDataTable.Columns.Add("EQPTID", typeof(string));
            inDataTable.Columns.Add("LOTID", typeof(string));
            inDataTable.Columns.Add("POSITION", typeof(string));
            inDataTable.Columns.Add("IN_LOTID", typeof(string));

            return inDataTable;
        }

        public DataTable GetBR_PRD_SEL_IN_MTRL_VALID_LM()
        {
            DataTable inDataTable = new DataTable();
            inDataTable.Columns.Add("LANGID", typeof(string));
            inDataTable.Columns.Add("PROCID", typeof(string));
            inDataTable.Columns.Add("EQSGID", typeof(string));
            inDataTable.Columns.Add("EQPTID", typeof(string));
            inDataTable.Columns.Add("LOTID", typeof(string));
            inDataTable.Columns.Add("POSITION", typeof(string));
            inDataTable.Columns.Add("IN_LOTID", typeof(string));

            return inDataTable;
        }

        public DataSet GetBR_PRD_REG_MTRL_INPUT_LM()
        {
            DataSet indataSet = new DataSet();
            DataTable inDataTable = indataSet.Tables.Add("INDATA");
            inDataTable.Columns.Add("SRCTYPE", typeof(string));
            inDataTable.Columns.Add("IFMODE", typeof(string));
            inDataTable.Columns.Add("EQPTID", typeof(string));
            inDataTable.Columns.Add("PROD_LOTID", typeof(string));
            inDataTable.Columns.Add("USERID", typeof(string));

            DataTable inInputTable = indataSet.Tables.Add("IN_INPUT");
            inInputTable.Columns.Add("EQPT_MOUNT_PSTN_ID", typeof(string));
            inInputTable.Columns.Add("EQPT_MOUNT_PSTN_STATE", typeof(string));
            inInputTable.Columns.Add("INPUT_LOTID", typeof(string));

            return indataSet;
        }

        public DataSet GetBR_PRD_REG_MTRL_INPUT_WN()
        {
            DataSet indataSet = new DataSet();
            DataTable inDataTable = indataSet.Tables.Add("INDATA");
            inDataTable.Columns.Add("SRCTYPE", typeof(string));
            inDataTable.Columns.Add("IFMODE", typeof(string));
            inDataTable.Columns.Add("EQPTID", typeof(string));
            inDataTable.Columns.Add("PROD_LOTID", typeof(string));
            inDataTable.Columns.Add("USERID", typeof(string));
            inDataTable.Columns.Add("LANGID", typeof(string));

            DataTable inInputTable = indataSet.Tables.Add("IN_INPUT");
            inInputTable.Columns.Add("EQPT_MOUNT_PSTN_ID", typeof(string));
            inInputTable.Columns.Add("EQPT_MOUNT_PSTN_STATE", typeof(string));
            inInputTable.Columns.Add("INPUT_LOTID", typeof(string));

            return indataSet;
        }

        public DataSet GetBR_PRD_REG_INPUT_LOT_AS()
        {
            DataSet indataSet = new DataSet();
            DataTable inDataTable = indataSet.Tables.Add("IN_EQP");
            inDataTable.Columns.Add("SRCTYPE", typeof(string));
            inDataTable.Columns.Add("IFMODE", typeof(string));
            inDataTable.Columns.Add("EQPTID", typeof(string));
            inDataTable.Columns.Add("USERID", typeof(string));
            inDataTable.Columns.Add("PROD_LOTID", typeof(string));
            inDataTable.Columns.Add("EQPT_LOTID", typeof(string));

            DataTable inInputTable = indataSet.Tables.Add("IN_INPUT");
            inInputTable.Columns.Add("EQPT_MOUNT_PSTN_ID", typeof(string));
            inInputTable.Columns.Add("EQPT_MOUNT_PSTN_STATE", typeof(string));
            inInputTable.Columns.Add("PRODID", typeof(string));
            inInputTable.Columns.Add("WINDING_RUNCARD_ID", typeof(string));
            inInputTable.Columns.Add("INPUT_QTY", typeof(Decimal));

            return indataSet;
        }

        public DataSet GetBR_PRD_REG_CONFIRM_LOT_TAPING_AFTER_FOLDING()
        {
            DataSet indataSet = new DataSet();

            DataTable inDataTable = indataSet.Tables.Add("INDATA");
            inDataTable.Columns.Add("SRCTYPE", typeof(string));
            inDataTable.Columns.Add("IFMODE", typeof(string));
            inDataTable.Columns.Add("EQPTID", typeof(string));
            inDataTable.Columns.Add("PROD_LOTID", typeof(string));
            inDataTable.Columns.Add("INPUTQTY", typeof(int));
            inDataTable.Columns.Add("OUTPUTQTY", typeof(int));
            inDataTable.Columns.Add("RESNQTY", typeof(int));
            inDataTable.Columns.Add("SHIFT", typeof(string));
            inDataTable.Columns.Add("WIPNOTE", typeof(string));
            inDataTable.Columns.Add("WRK_USERID", typeof(string));
            inDataTable.Columns.Add("WRK_USER_NAME", typeof(string));
            inDataTable.Columns.Add("USERID", typeof(string));
            inDataTable.Columns.Add("WIPDTTM_ED", typeof(DateTime));

            DataTable inInputLot = indataSet.Tables.Add("IN_INPUT");
            inInputLot.Columns.Add("EQPT_MOUNT_PSTN_ID", typeof(string));
            inInputLot.Columns.Add("EQPT_MOUNT_PSTN_STATE", typeof(string));
            inInputLot.Columns.Add("INPUT_LOTID", typeof(string));

            return indataSet;
        }

        public DataSet GetBR_PRD_REG_INPUT_LOT_WS()
        {
            DataSet indataSet = new DataSet();
            DataTable inDataTable = indataSet.Tables.Add("IN_EQP");
            inDataTable.Columns.Add("SRCTYPE", typeof(string));
            inDataTable.Columns.Add("IFMODE", typeof(string));
            inDataTable.Columns.Add("EQPTID", typeof(string));
            inDataTable.Columns.Add("USERID", typeof(string));
            inDataTable.Columns.Add("PROD_LOTID", typeof(string));
            inDataTable.Columns.Add("EQPT_LOTID", typeof(string));

            DataTable inInputTable = indataSet.Tables.Add("IN_INPUT");
            inInputTable.Columns.Add("EQPT_MOUNT_PSTN_ID", typeof(string));
            inInputTable.Columns.Add("EQPT_MOUNT_PSTN_STATE", typeof(string));
            inInputTable.Columns.Add("INPUT_LOTID", typeof(string));
            inInputTable.Columns.Add("INPUT_QTY", typeof(decimal));
            return indataSet;
        }

        public DataSet GetBR_PRD_REG_MTRL_INPUT_AS()
        {
            DataSet indataSet = new DataSet();
            DataTable inDataTable = indataSet.Tables.Add("IN_EQP");
            inDataTable.Columns.Add("SRCTYPE", typeof(string));
            inDataTable.Columns.Add("IFMODE", typeof(string));
            inDataTable.Columns.Add("EQPTID", typeof(string));
            inDataTable.Columns.Add("USERID", typeof(string));
            inDataTable.Columns.Add("PROD_LOTID", typeof(string));
            inDataTable.Columns.Add("EQPT_LOTID", typeof(string));
            inDataTable.Columns.Add("EQPT_MOUNT_PSTN_ID", typeof(string));
            inDataTable.Columns.Add("EQPT_MOUNT_PSTN_STATE", typeof(string));
            inDataTable.Columns.Add("CSTID", typeof(string));
            inDataTable.Columns.Add("OUT_LOTID", typeof(string));
            return indataSet;
        }

        public DataSet GetBR_PRD_REG_INPUT_CANCEL_LM()
        {
            DataSet indataSet = new DataSet();
            DataTable inDataTable = indataSet.Tables.Add("INDATA");
            inDataTable.Columns.Add("SRCTYPE", typeof(string));
            inDataTable.Columns.Add("IFMODE", typeof(string));
            inDataTable.Columns.Add("EQPTID", typeof(string));
            inDataTable.Columns.Add("USERID", typeof(string));
            inDataTable.Columns.Add("PROD_LOTID", typeof(string));

            DataTable inInputTable = indataSet.Tables.Add("IN_INPUT");
            inInputTable.Columns.Add("WIPNOTE", typeof(string));
            inInputTable.Columns.Add("INPUT_LOTID", typeof(string));
            inInputTable.Columns.Add("EQPT_MOUNT_PSTN_ID", typeof(string));
            //inInputTable.Columns.Add("ACTQTY", typeof(int));
            inInputTable.Columns.Add("CSTID", typeof(string));
            inInputTable.Columns.Add("INPUT_SEQNO", typeof(Int64));

            return indataSet;
        }

        public DataSet GetBR_PRD_REG_INPUT_COMPLETE_LM()
        {
            DataSet indataSet = new DataSet();
            DataTable inDataTable = indataSet.Tables.Add("INDATA");
            inDataTable.Columns.Add("SRCTYPE", typeof(string));
            inDataTable.Columns.Add("IFMODE", typeof(string));
            inDataTable.Columns.Add("EQPTID", typeof(string));
            inDataTable.Columns.Add("USERID", typeof(string));
            inDataTable.Columns.Add("PROD_LOTID", typeof(string));

            DataTable inInputTable = indataSet.Tables.Add("IN_INPUT");
            inInputTable.Columns.Add("EQPT_MOUNT_PSTN_STATE", typeof(string));
            inInputTable.Columns.Add("INPUT_LOTID", typeof(string));
            inInputTable.Columns.Add("EQPT_MOUNT_PSTN_ID", typeof(string));
            //inInputTable.Columns.Add("ACTQTY", typeof(int));

            return indataSet;
        }

        public DataSet GetBR_PRD_REG_CREATE_MAGAZINE_LM()
        {
            DataSet indataSet = new DataSet();
            DataTable inDataTable = indataSet.Tables.Add("INDATA");
            inDataTable.Columns.Add("SRCTYPE", typeof(string));
            inDataTable.Columns.Add("LANGID", typeof(string));
            inDataTable.Columns.Add("IFMODE", typeof(string));
            inDataTable.Columns.Add("EQPTID", typeof(string));
            inDataTable.Columns.Add("WO_DETL_ID", typeof(string));
            inDataTable.Columns.Add("PROD_LOTID", typeof(string));
            inDataTable.Columns.Add("OUT_LOTID", typeof(string));
            inDataTable.Columns.Add("CSTID", typeof(string));
            inDataTable.Columns.Add("INPUTQTY", typeof(int));
            inDataTable.Columns.Add("OUTPUTQTY", typeof(int));
            inDataTable.Columns.Add("RESNQTY", typeof(int));
            inDataTable.Columns.Add("SHIFT", typeof(string));
            inDataTable.Columns.Add("WIPNOTE", typeof(string));
            inDataTable.Columns.Add("WRK_USER_NAME", typeof(string));
            inDataTable.Columns.Add("USERID", typeof(string));

            DataTable inInputTable = indataSet.Tables.Add("IN_INPUT");
            inInputTable.Columns.Add("EQPT_MOUNT_PSTN_ID", typeof(string));
            inInputTable.Columns.Add("EQPT_MOUNT_PSTN_STATE", typeof(string));
            inInputTable.Columns.Add("INPUT_LOTID", typeof(string));

            return indataSet;
        }

        public DataTable GetBR_PRD_GET_NEW_OUT_LOT_LM()
        {
            DataTable inDataTable = new DataTable();
            inDataTable.Columns.Add("SRCTYPE", typeof(string));
            inDataTable.Columns.Add("EQPTID", typeof(string));
            inDataTable.Columns.Add("PROD_LOTID", typeof(string));
            inDataTable.Columns.Add("IFMODE", typeof(string));
            inDataTable.Columns.Add("USERID", typeof(string));
            inDataTable.Columns.Add("CSTID", typeof(string));

            return inDataTable;
        }

        public DataSet GetBR_PRD_REG_DISPATCH_LOT_LM()
        {
            DataSet indataSet = new DataSet();

            DataTable inDataTable = indataSet.Tables.Add("INDATA");
            inDataTable.Columns.Add("SRCTYPE", typeof(string));
            //inDataTable.Columns.Add("LANGID", typeof(string));
            inDataTable.Columns.Add("IFMODE", typeof(string));
            inDataTable.Columns.Add("EQPTID", typeof(string));
            inDataTable.Columns.Add("EQSGID", typeof(string));
            inDataTable.Columns.Add("REWORK", typeof(string));
            inDataTable.Columns.Add("USERID", typeof(string));

            DataTable input_LOT = indataSet.Tables.Add("INLOT");
            input_LOT.Columns.Add("LOTID", typeof(string));
            input_LOT.Columns.Add("ACTQTY", typeof(int));
            input_LOT.Columns.Add("ACTUQTY", typeof(int));
            input_LOT.Columns.Add("WIPNOTE", typeof(string));

            return indataSet;
        }

        public DataSet GetBR_PRD_REG_DELETE_MAGAZINE_LM()
        {
            DataSet indataSet = new DataSet();

            DataTable inDataTable = indataSet.Tables.Add("INDATA");
            inDataTable.Columns.Add("SRCTYPE", typeof(string));
            inDataTable.Columns.Add("IFMODE", typeof(string));
            inDataTable.Columns.Add("EQPTID", typeof(string));
            inDataTable.Columns.Add("PROD_LOTID", typeof(string));
            inDataTable.Columns.Add("USERID", typeof(string));

            DataTable input_LOT = indataSet.Tables.Add("IN_INPUT");
            input_LOT.Columns.Add("OUT_LOTID", typeof(string));

            return indataSet;
        }

        public DataSet GetBR_PRD_REG_SAVE_MAGAZINE_LM()
        {
            DataSet indataSet = new DataSet();

            DataTable inDataTable = indataSet.Tables.Add("IN_DATA");
            inDataTable.Columns.Add("SRCTYPE", typeof(string));
            inDataTable.Columns.Add("IFMODE", typeof(string));
            inDataTable.Columns.Add("EQPTID", typeof(string));
            inDataTable.Columns.Add("PROD_LOTID", typeof(string));
            inDataTable.Columns.Add("USERID", typeof(string));

            DataTable out_LOT = indataSet.Tables.Add("IN_OUTLOT");
            out_LOT.Columns.Add("OUT_LOTID", typeof(string));
            out_LOT.Columns.Add("OUTPUT_QTY", typeof(int));
            //out_LOT.Columns.Add("CSTID", typeof(string));

            //DataTable input_LOT = indataSet.Tables.Add("IN_INPUT");
            //input_LOT.Columns.Add("EQPT_MOUNT_PSTN_ID", typeof(string));
            //input_LOT.Columns.Add("EQPT_MOUNT_PSTN_STATE", typeof(string));
            //input_LOT.Columns.Add("INPUT_LOTID", typeof(string));

            return indataSet;
        }

        public DataTable GetBR_PRD_GET_PROCESS_LOT_LABEL_LM()
        {
            DataTable inDataTable = new DataTable();
            inDataTable.Columns.Add("LANGID", typeof(string));
            inDataTable.Columns.Add("PROCID", typeof(string));
            inDataTable.Columns.Add("EQPTID", typeof(string));
            inDataTable.Columns.Add("LOTID", typeof(string));
            inDataTable.Columns.Add("WIPSEQ", typeof(string));
            inDataTable.Columns.Add("PRMK", typeof(string));
            inDataTable.Columns.Add("RESO", typeof(string));
            inDataTable.Columns.Add("PRCN", typeof(string));
            inDataTable.Columns.Add("MARH", typeof(string));
            inDataTable.Columns.Add("MARV", typeof(string));

            return inDataTable;
        }

        public DataTable GetDA_PRD_UPD_OUT_LOT_LABEL()
        {
            DataTable inDataTable = new DataTable();
            inDataTable.Columns.Add("LANGID", typeof(string));
            inDataTable.Columns.Add("PROCID", typeof(string));
            inDataTable.Columns.Add("EQPTID", typeof(string));
            inDataTable.Columns.Add("LOTID", typeof(string));

            return inDataTable;
        }

        public DataSet GetBR_PRD_REG_EQPEND_LM()
        {
            DataSet indataSet = new DataSet();

            DataTable inDataTable = indataSet.Tables.Add("IN_EQP");
            inDataTable.Columns.Add("SRCTYPE", typeof(string));
            inDataTable.Columns.Add("IFMODE", typeof(string));
            inDataTable.Columns.Add("EQPTID", typeof(string));
            inDataTable.Columns.Add("USERID", typeof(string));
            inDataTable.Columns.Add("PROD_LOTID", typeof(string));
            inDataTable.Columns.Add("INPUT_QTY", typeof(int));
            inDataTable.Columns.Add("OUTPUT_QTY", typeof(int));
            inDataTable.Columns.Add("END_DTTM", typeof(DateTime));

            DataTable out_LOT = indataSet.Tables.Add("IN_INPUT");
            out_LOT.Columns.Add("EQPT_MOUNT_PSTN_ID", typeof(string));
            out_LOT.Columns.Add("EQPT_MOUNT_PSTN_STATE", typeof(string));
            out_LOT.Columns.Add("INPUT_LOTID", typeof(string));

            DataTable input_LOT = indataSet.Tables.Add("IN_DEFECT");
            input_LOT.Columns.Add("EQPT_DFCT_CODE", typeof(string));
            input_LOT.Columns.Add("DFCT_QTY", typeof(int));

            return indataSet;
        }

        public DataTable GetBR_PRD_REG_EQPEND_CMM()
        {
            DataTable inDataTable = new DataTable();
            inDataTable.Columns.Add("SRCTYPE", typeof(string));
            inDataTable.Columns.Add("IFMODE", typeof(string));
            inDataTable.Columns.Add("EQPTID", typeof(string));
            inDataTable.Columns.Add("USERID", typeof(string));
            inDataTable.Columns.Add("PROD_LOTID", typeof(string));
            inDataTable.Columns.Add("OUTPUT_QTY", typeof(int));
            inDataTable.Columns.Add("END_DTTM", typeof(DateTime));

            return inDataTable;
        }

        public DataSet GetBR_PRD_REG_EQPEND_FD()
        {
            DataSet indataSet = new DataSet();

            DataTable inDataTable = indataSet.Tables.Add("IN_EQP");
            inDataTable.Columns.Add("SRCTYPE", typeof(string));
            inDataTable.Columns.Add("IFMODE", typeof(string));
            inDataTable.Columns.Add("EQPTID", typeof(string));
            inDataTable.Columns.Add("USERID", typeof(string));
            inDataTable.Columns.Add("PROD_LOTID", typeof(string));
            inDataTable.Columns.Add("INPUT_QTY", typeof(int));
            inDataTable.Columns.Add("OUTPUT_QTY", typeof(int));
            inDataTable.Columns.Add("END_DTTM", typeof(DateTime));

            DataTable out_LOT = indataSet.Tables.Add("IN_INPUT");
            out_LOT.Columns.Add("EQPT_MOUNT_PSTN_ID", typeof(string));
            out_LOT.Columns.Add("EQPT_MOUNT_PSTN_STATE", typeof(string));
            out_LOT.Columns.Add("INPUT_LOTID", typeof(string));

            DataTable input_LOT = indataSet.Tables.Add("IN_DEFECT");
            input_LOT.Columns.Add("EQPT_DFCT_CODE", typeof(string));
            input_LOT.Columns.Add("DFCT_QTY", typeof(int));

            return indataSet;
        }

        public DataSet GetBR_PRD_REG_EQPEND_CL()
        {
            DataSet indataSet = new DataSet();

            DataTable inDataTable = indataSet.Tables.Add("IN_EQP");
            inDataTable.Columns.Add("SRCTYPE", typeof(string));
            inDataTable.Columns.Add("IFMODE", typeof(string));
            inDataTable.Columns.Add("EQPTID", typeof(string));
            inDataTable.Columns.Add("PROD_LOTID", typeof(string));
            inDataTable.Columns.Add("INPUT_QTY", typeof(int));
            inDataTable.Columns.Add("OUTPUT_QTY", typeof(int));
            inDataTable.Columns.Add("USERID", typeof(string));
            inDataTable.Columns.Add("END_DTTM", typeof(DateTime));

            DataTable out_LOT = indataSet.Tables.Add("IN_INPUT");
            out_LOT.Columns.Add("INPUT_LOTID", typeof(string));
            out_LOT.Columns.Add("MTRLID", typeof(string));
            out_LOT.Columns.Add("EQPT_MOUNT_PSTN_ID", typeof(string));
            out_LOT.Columns.Add("EQPT_MOUNT_PSTN_STATE", typeof(string));

            return indataSet;
        }

        public DataTable GetDA_PRD_SEL_WAIT_LOT_LIST_LM()
        {
            DataTable inDataTable = new DataTable();
            inDataTable.Columns.Add("LANGID", typeof(string));
            inDataTable.Columns.Add("PROCID", typeof(string));
            inDataTable.Columns.Add("EQPTID", typeof(string));
            inDataTable.Columns.Add("EQSGID", typeof(string));
            inDataTable.Columns.Add("ELECTYPE", typeof(string));
            inDataTable.Columns.Add("LOTID", typeof(string));

            return inDataTable;
        }

        public DataTable GetDA_PRD_SEL_WAIT_LOT_LIST_NT()
        {
            DataTable inDataTable = new DataTable();
            inDataTable.Columns.Add("LANGID", typeof(string));
            inDataTable.Columns.Add("PROCID", typeof(string));
            inDataTable.Columns.Add("EQPTID", typeof(string));
            inDataTable.Columns.Add("EQSGID", typeof(string));
            inDataTable.Columns.Add("ELECTYPE", typeof(string));
            inDataTable.Columns.Add("LOTID", typeof(string));

            return inDataTable;
        }

        public DataTable GetBR_PRD_GET_CONVERT_UNIT_QTY()
        {
            DataTable inDataTable = new DataTable();
            inDataTable.Columns.Add("LOTID", typeof(string));
            //inDataTable.Columns.Add("QTY", typeof(string));

            return inDataTable;
        }

        public DataSet GetBR_PRD_REG_REMAIN_INPUT_IN_LOT()
        {
            DataSet indataSet = new DataSet();

            DataTable inDataTable = indataSet.Tables.Add("INDATA");
            inDataTable.Columns.Add("SRCTYPE", typeof(string));
            inDataTable.Columns.Add("IFMODE", typeof(string));
            inDataTable.Columns.Add("EQPTID", typeof(string));
            inDataTable.Columns.Add("USERID", typeof(string));
            inDataTable.Columns.Add("PROD_LOTID", typeof(string));
            inDataTable.Columns.Add("INPUT_LOT_TYPE", typeof(string));

            DataTable inInputLot = indataSet.Tables.Add("IN_INPUT");
            inInputLot.Columns.Add("EQPT_MOUNT_PSTN_ID", typeof(string));
            inInputLot.Columns.Add("EQPT_MOUNT_PSTN_STATE", typeof(string));
            inInputLot.Columns.Add("INPUT_LOTID", typeof(string));
            inInputLot.Columns.Add("WIPNOTE", typeof(string));
            inInputLot.Columns.Add("ACTQTY", typeof(int));
            inInputLot.Columns.Add("CSTID", typeof(string));

            return indataSet;
        }

        public DataTable GetDA_PRD_SEL_EQUIPMENT_ELTYPE_IFNO()
        {
            DataTable inDataTable = new DataTable();
            inDataTable.Columns.Add("EQPTID", typeof(string));

            return inDataTable;
        }

        public DataSet GetBR_PRD_REG_LOTSTART_LM()
        {
            DataSet indataSet = new DataSet();

            DataTable inDataTable = indataSet.Tables.Add("IN_EQP");
            inDataTable.Columns.Add("SRCTYPE", typeof(string));
            inDataTable.Columns.Add("IFMODE", typeof(string));
            inDataTable.Columns.Add("EQPTID", typeof(string));
            inDataTable.Columns.Add("PROD_LOTID", typeof(string));
            inDataTable.Columns.Add("WO_DETL_ID", typeof(string));
            inDataTable.Columns.Add("USERID", typeof(string));
            inDataTable.Columns.Add("CELL_DETL_CLSS_CODE", typeof(string));

            DataTable inInputLot = indataSet.Tables.Add("IN_INPUT");
            inInputLot.Columns.Add("EQPT_MOUNT_PSTN_ID", typeof(string));
            inInputLot.Columns.Add("EQPT_MOUNT_PSTN_STATE", typeof(string));
            inInputLot.Columns.Add("INPUT_LOTID", typeof(string));

            return indataSet;
        }

        public DataSet GetBR_PRD_GET_NEW_LOT_LM()
        {
            DataSet indataSet = new DataSet();

            DataTable inDataTable = indataSet.Tables.Add("IN_EQP");
            inDataTable.Columns.Add("SRCTYPE", typeof(string));
            inDataTable.Columns.Add("IFMODE", typeof(string));
            inDataTable.Columns.Add("EQPTID", typeof(string));
            inDataTable.Columns.Add("LAMI_CELL_TYPE", typeof(string));
            inDataTable.Columns.Add("USERID", typeof(string));

            DataTable inInputLot = indataSet.Tables.Add("IN_INPUT");
            inInputLot.Columns.Add("EQPT_MOUNT_PSTN_ID", typeof(string));
            inInputLot.Columns.Add("EQPT_MOUNT_PSTN_STATE", typeof(string));
            inInputLot.Columns.Add("INPUT_LOTID", typeof(string));

            return indataSet;
        }

        public DataTable GetDA_PRD_SEL_WIP_INFO()
        {
            DataTable inDataTable = new DataTable();
            inDataTable.Columns.Add("LOTID", typeof(string));
            inDataTable.Columns.Add("LANGID", typeof(string));

            return inDataTable;
        }

        public DataSet GetBR_PRD_REG_DEFECT()
        {
            DataSet indataSet = new DataSet();

            DataTable inDataTable = indataSet.Tables.Add("INDATA");
            inDataTable.Columns.Add("SRCTYPE", typeof(string));
            inDataTable.Columns.Add("IFMODE", typeof(string));
            inDataTable.Columns.Add("EQPTID", typeof(string));
            inDataTable.Columns.Add("USERID", typeof(string));

            DataTable inDEFECT_LOT = indataSet.Tables.Add("INRESN");
            inDEFECT_LOT.Columns.Add("LOTID", typeof(string));
            inDEFECT_LOT.Columns.Add("WIPSEQ", typeof(int));
            inDEFECT_LOT.Columns.Add("ACTID", typeof(string));
            inDEFECT_LOT.Columns.Add("RESNCODE", typeof(string));
            inDEFECT_LOT.Columns.Add("RESNQTY", typeof(double));
            inDEFECT_LOT.Columns.Add("RESNCODE_CAUSE", typeof(string));
            inDEFECT_LOT.Columns.Add("PROCID_CAUSE", typeof(string));
            inDEFECT_LOT.Columns.Add("RESNNOTE", typeof(string));
            inDEFECT_LOT.Columns.Add("DFCT_TAG_QTY", typeof(int));

            //DataTable inLOSS_LOT = indataSet.Tables.Add("INLOSS_LOT");
            //inLOSS_LOT.Columns.Add("ACTID", typeof(string));
            //inLOSS_LOT.Columns.Add("RESNCODE", typeof(string));
            //inLOSS_LOT.Columns.Add("RESNQTY", typeof(double));
            //inLOSS_LOT.Columns.Add("RESNCODE_CAUSE", typeof(string));
            //inLOSS_LOT.Columns.Add("PROCID_CAUSE", typeof(string));
            //inLOSS_LOT.Columns.Add("RESNNOTE", typeof(string));

            //DataTable inREPAIR_LOT = indataSet.Tables.Add("INREPAIR_LOT");
            //inREPAIR_LOT.Columns.Add("ACTID", typeof(string));
            //inREPAIR_LOT.Columns.Add("RESNCODE", typeof(string));
            //inREPAIR_LOT.Columns.Add("RESNQTY", typeof(double));
            //inREPAIR_LOT.Columns.Add("RESNCODE_CAUSE", typeof(string));
            //inREPAIR_LOT.Columns.Add("PROCID_CAUSE", typeof(string));
            //inREPAIR_LOT.Columns.Add("RESNNOTE", typeof(string));

            //DataTable inSCRAP_LOT = indataSet.Tables.Add("INSCRAP_LOT");
            //inSCRAP_LOT.Columns.Add("ACTID", typeof(string));
            //inSCRAP_LOT.Columns.Add("RESNCODE", typeof(string));
            //inSCRAP_LOT.Columns.Add("RESNQTY", typeof(double));
            //inSCRAP_LOT.Columns.Add("RESNCODE_CAUSE", typeof(string));
            //inSCRAP_LOT.Columns.Add("PROCID_CAUSE", typeof(string));
            //inSCRAP_LOT.Columns.Add("RESNNOTE", typeof(string));

            return indataSet;
        }

        public DataSet GetBR_PRD_REG_DEFECT_ALL()
        {
            DataSet indataSet = new DataSet();

            DataTable inDataTable = indataSet.Tables.Add("INDATA");
            inDataTable.Columns.Add("SRCTYPE", typeof(string));
            inDataTable.Columns.Add("IFMODE", typeof(string));
            inDataTable.Columns.Add("EQPTID", typeof(string));
            inDataTable.Columns.Add("USERID", typeof(string));

            DataTable inDEFECT_LOT = indataSet.Tables.Add("INRESN");
            inDEFECT_LOT.Columns.Add("LOTID", typeof(string));
            inDEFECT_LOT.Columns.Add("WIPSEQ", typeof(string));
            inDEFECT_LOT.Columns.Add("ACTID", typeof(string));
            inDEFECT_LOT.Columns.Add("RESNCODE", typeof(string));
            inDEFECT_LOT.Columns.Add("RESNQTY", typeof(double));
            inDEFECT_LOT.Columns.Add("RESNCODE_CAUSE", typeof(string));
            inDEFECT_LOT.Columns.Add("PROCID_CAUSE", typeof(string));
            inDEFECT_LOT.Columns.Add("RESNNOTE", typeof(string));
            inDEFECT_LOT.Columns.Add("DFCT_TAG_QTY", typeof(int));
            inDEFECT_LOT.Columns.Add("LANE_QTY", typeof(int));
            inDEFECT_LOT.Columns.Add("LANE_PTN_QTY", typeof(int));
            inDEFECT_LOT.Columns.Add("COST_CNTR_ID", typeof(string));
            inDEFECT_LOT.Columns.Add("A_TYPE_DFCT_QTY", typeof(int));
            inDEFECT_LOT.Columns.Add("C_TYPE_DFCT_QTY", typeof(int));

            return indataSet;
        }

        public DataSet GetBR_PRD_REG_CONFIRM_LOT_LM()
        {
            DataSet indataSet = new DataSet();

            DataTable inDataTable = indataSet.Tables.Add("INDATA");
            inDataTable.Columns.Add("SRCTYPE", typeof(string));
            inDataTable.Columns.Add("IFMODE", typeof(string));
            inDataTable.Columns.Add("EQPTID", typeof(string));
            inDataTable.Columns.Add("PROD_LOTID", typeof(string));
            inDataTable.Columns.Add("INPUTQTY", typeof(int));
            inDataTable.Columns.Add("OUTPUTQTY", typeof(int));
            inDataTable.Columns.Add("RESNQTY", typeof(int));
            inDataTable.Columns.Add("SHIFT", typeof(string));
            inDataTable.Columns.Add("WIPNOTE", typeof(string));
            inDataTable.Columns.Add("WRK_USERID", typeof(string));
            inDataTable.Columns.Add("WRK_USER_NAME", typeof(string));
            inDataTable.Columns.Add("USERID", typeof(string));
            inDataTable.Columns.Add("WIPDTTM_ED", typeof(DateTime));

            DataTable inInputLot = indataSet.Tables.Add("IN_INPUT");
            inInputLot.Columns.Add("EQPT_MOUNT_PSTN_ID", typeof(string));
            inInputLot.Columns.Add("EQPT_MOUNT_PSTN_STATE", typeof(string));
            inInputLot.Columns.Add("INPUT_LOTID", typeof(string));

            return indataSet;
        }

        public DataTable GetDA_PRD_SEL_WIPINFO_BY_PERIOD_LM()
        {
            DataTable inDataTable = new DataTable();
            inDataTable.Columns.Add("LANGID", typeof(string));
            inDataTable.Columns.Add("EQPTID", typeof(string));
            inDataTable.Columns.Add("STDT", typeof(string));
            inDataTable.Columns.Add("EDDT", typeof(string));

            return inDataTable;
        }

        public DataTable GetDA_PRD_SEL_IN_MTRL_OF_MAZ_LM()
        {
            DataTable inDataTable = new DataTable();
            inDataTable.Columns.Add("LOTID", typeof(string));

            return inDataTable;
        }

        public DataTable GetDA_PRD_SEL_RESV_LOT_LIST_LM()
        {
            DataTable inDataTable = new DataTable();
            inDataTable.Columns.Add("LANGID", typeof(string));
            inDataTable.Columns.Add("PROCID", typeof(string));
            inDataTable.Columns.Add("EQPTID", typeof(string));
            inDataTable.Columns.Add("EQSGID", typeof(string));

            return inDataTable;
        }

        public DataTable GetDA_PRD_SEL_INPUT_POS_INFO()
        {
            DataTable inDataTable = new DataTable();
            inDataTable.Columns.Add("LANGID", typeof(string));
            //inDataTable.Columns.Add("PROCID", typeof(string));
            inDataTable.Columns.Add("EQPTID", typeof(string));
            inDataTable.Columns.Add("MOUNT_MTRL_TYPE_CODE", typeof(string));

            return inDataTable;
        }

        public DataTable GetDA_PRD_SEL_WIPINFO_FD()
        {
            DataTable inDataTable = new DataTable();
            inDataTable.Columns.Add("LANGID", typeof(string));
            inDataTable.Columns.Add("WOID", typeof(string));
            inDataTable.Columns.Add("EQPTID", typeof(string));
            inDataTable.Columns.Add("PROCID", typeof(string));
            inDataTable.Columns.Add("EQSGID", typeof(string));

            return inDataTable;
        }

        public DataSet GetBR_PRD_REG_INPUT_CANCEL_FD()
        {
            DataSet indataSet = new DataSet();
            DataTable inDataTable = indataSet.Tables.Add("INDATA");
            inDataTable.Columns.Add("SRCTYPE", typeof(string));
            inDataTable.Columns.Add("IFMODE", typeof(string));
            inDataTable.Columns.Add("EQPTID", typeof(string));
            inDataTable.Columns.Add("USERID", typeof(string));
            inDataTable.Columns.Add("PROD_LOTID", typeof(string));

            DataTable inInputTable = indataSet.Tables.Add("IN_INPUT");
            inInputTable.Columns.Add("WIPNOTE", typeof(string));
            inInputTable.Columns.Add("INPUT_LOTID", typeof(string));
            inInputTable.Columns.Add("EQPT_MOUNT_PSTN_ID", typeof(string));
            //inInputTable.Columns.Add("ACTQTY", typeof(int));
            inInputTable.Columns.Add("INPUT_SEQNO", typeof(Int64));
            inInputTable.Columns.Add("CSTID", typeof(string));

            return indataSet;
        }

        public DataTable GetDA_PRD_SEL_WAIT_LOT_LIST_FD()
        {
            DataTable inDataTable = new DataTable();
            inDataTable.Columns.Add("LANGID", typeof(string));
            inDataTable.Columns.Add("PROCID", typeof(string));
            inDataTable.Columns.Add("EQSGID", typeof(string));
            inDataTable.Columns.Add("EQPTID", typeof(string));
            inDataTable.Columns.Add("PRODUCT_LEVEL2_CODE", typeof(string));
            inDataTable.Columns.Add("PRODUCT_LEVEL3_CODE", typeof(string));
            inDataTable.Columns.Add("PRODID", typeof(string));
            inDataTable.Columns.Add("WOID", typeof(string));
            inDataTable.Columns.Add("LOTID", typeof(string));
            inDataTable.Columns.Add("CSTID", typeof(string));

            return inDataTable;
        }

        public DataTable GetDA_PRD_SEL_IN_LOT_LIST_FD()
        {
            DataTable inDataTable = new DataTable();
            inDataTable.Columns.Add("LOTID", typeof(string));
            inDataTable.Columns.Add("WIPSEQ", typeof(string));
            inDataTable.Columns.Add("INPUT_LOT_STAT_CODE", typeof(string));
            inDataTable.Columns.Add("INPUT_LOT_TYPE_CODE", typeof(string));

            return inDataTable;
        }

        public DataTable GetDA_PRD_SEL_OUT_BOX_FD()
        {
            DataTable inDataTable = new DataTable();
            inDataTable.Columns.Add("LANGID", typeof(string));
            //inDataTable.Columns.Add("PROCID", typeof(string));
            //inDataTable.Columns.Add("EQSGID", typeof(string));
            //inDataTable.Columns.Add("EQPTID", typeof(string));
            inDataTable.Columns.Add("PR_LOTID", typeof(string));

            return inDataTable;
        }

        public DataSet GetBR_PRD_REG_MTRL_INPUT_FD()
        {
            DataSet indataSet = new DataSet();
            DataTable inDataTable = indataSet.Tables.Add("INDATA");
            inDataTable.Columns.Add("SRCTYPE", typeof(string));
            inDataTable.Columns.Add("IFMODE", typeof(string));
            inDataTable.Columns.Add("EQPTID", typeof(string));
            inDataTable.Columns.Add("PROD_LOTID", typeof(string));
            inDataTable.Columns.Add("USERID", typeof(string));

            DataTable inInputTable = indataSet.Tables.Add("IN_INPUT");
            inInputTable.Columns.Add("EQPT_MOUNT_PSTN_ID", typeof(string));
            inInputTable.Columns.Add("EQPT_MOUNT_PSTN_STATE", typeof(string));
            inInputTable.Columns.Add("INPUT_LOTID", typeof(string));
            inInputTable.Columns.Add("CSTID", typeof(string));

            return indataSet;
        }

        public DataTable GetBR_PRD_GET_NEW_OUT_LOT_FD()
        {
            DataTable inDataTable = new DataTable();
            inDataTable.Columns.Add("SRCTYPE", typeof(string));
            inDataTable.Columns.Add("IFMODE", typeof(string));
            inDataTable.Columns.Add("EQPTID", typeof(string));
            inDataTable.Columns.Add("PROD_LOTID", typeof(string));
            //inDataTable.Columns.Add("WIP_TYPE_CODE", typeof(string));
            //inDataTable.Columns.Add("CALDATE_YMD", typeof(string));
            inDataTable.Columns.Add("USERID", typeof(string));
            inDataTable.Columns.Add("CSTID", typeof(string));

            return inDataTable;
        }

        public DataTable GetBR_PRD_REG_CREATE_BOX_FD()
        {
            DataTable inDataTable = new DataTable();
            inDataTable.Columns.Add("SRCTYPE", typeof(string));
            inDataTable.Columns.Add("LANGID", typeof(string));
            inDataTable.Columns.Add("IFMODE", typeof(string));
            inDataTable.Columns.Add("EQPTID", typeof(string));
            inDataTable.Columns.Add("WO_DETL_ID", typeof(string));
            inDataTable.Columns.Add("PROD_LOTID", typeof(string));
            inDataTable.Columns.Add("OUT_LOTID", typeof(string));
            inDataTable.Columns.Add("CSTID", typeof(string));
            inDataTable.Columns.Add("INPUTQTY", typeof(int));
            inDataTable.Columns.Add("OUTPUTQTY", typeof(int));
            inDataTable.Columns.Add("RESNQTY", typeof(int));
            inDataTable.Columns.Add("SHIFT", typeof(string));
            inDataTable.Columns.Add("WIPNOTE", typeof(string));
            inDataTable.Columns.Add("WRK_USER_NAME", typeof(string));
            inDataTable.Columns.Add("USERID", typeof(string));
            inDataTable.Columns.Add("BONUSQTY", typeof(int));

            return inDataTable;
        }

        public DataTable GetBR_PRD_REG_DELETE_BOX_FD()
        {
            DataTable inDataTable = new DataTable();
            inDataTable.Columns.Add("SRCTYPE", typeof(string));
            inDataTable.Columns.Add("LANGID", typeof(string));
            inDataTable.Columns.Add("IFMODE", typeof(string));
            inDataTable.Columns.Add("EQPTID", typeof(string));
            inDataTable.Columns.Add("WO_DETL_ID", typeof(string));
            inDataTable.Columns.Add("LOTID", typeof(string));
            inDataTable.Columns.Add("LOTID_RT", typeof(string));
            inDataTable.Columns.Add("INPUTQTY", typeof(int));
            inDataTable.Columns.Add("OUTPUTQTY", typeof(int));
            inDataTable.Columns.Add("RESNQTY", typeof(int));
            inDataTable.Columns.Add("SHIFT", typeof(string));
            inDataTable.Columns.Add("WIPNOTE", typeof(string));
            inDataTable.Columns.Add("WRK_USER_NAME", typeof(string));
            inDataTable.Columns.Add("USERID", typeof(string));

            return inDataTable;
        }

        public DataTable GetBR_PRD_REG_SAVE_BOX_FD()
        {
            DataTable inDataTable = new DataTable();
            inDataTable.Columns.Add("SRCTYPE", typeof(string));
            inDataTable.Columns.Add("LANGID", typeof(string));
            inDataTable.Columns.Add("IFMODE", typeof(string));
            inDataTable.Columns.Add("EQPTID", typeof(string));
            inDataTable.Columns.Add("WO_DETL_ID", typeof(string));
            inDataTable.Columns.Add("LOTID", typeof(string));
            inDataTable.Columns.Add("LOTID_RT", typeof(string));
            inDataTable.Columns.Add("INPUTQTY", typeof(int));
            inDataTable.Columns.Add("OUTPUTQTY", typeof(int));
            inDataTable.Columns.Add("RESNQTY", typeof(int));
            inDataTable.Columns.Add("SHIFT", typeof(string));
            inDataTable.Columns.Add("WIPNOTE", typeof(string));
            inDataTable.Columns.Add("WRK_USER_NAME", typeof(string));
            inDataTable.Columns.Add("USERID", typeof(string));

            return inDataTable;
        }

        public DataSet GetBR_PRD_REG_CONFIRM_LOT_FD()
        {
            DataSet indataSet = new DataSet();

            DataTable inDataTable = indataSet.Tables.Add("INDATA");
            inDataTable.Columns.Add("SRCTYPE", typeof(string));
            inDataTable.Columns.Add("IFMODE", typeof(string));
            inDataTable.Columns.Add("EQPTID", typeof(string));
            inDataTable.Columns.Add("PROD_LOTID", typeof(string));
            inDataTable.Columns.Add("INPUTQTY", typeof(int));
            inDataTable.Columns.Add("OUTPUTQTY", typeof(int));
            inDataTable.Columns.Add("RESNQTY", typeof(int));
            inDataTable.Columns.Add("SHIFT", typeof(string));
            inDataTable.Columns.Add("WIPDTTM_ED", typeof(DateTime));
            inDataTable.Columns.Add("WIPNOTE", typeof(string));
            inDataTable.Columns.Add("WRK_USERID", typeof(string));
            inDataTable.Columns.Add("WRK_USER_NAME", typeof(string));
            inDataTable.Columns.Add("USERID", typeof(string));

            DataTable input_LOT = indataSet.Tables.Add("IN_INPUT");
            input_LOT.Columns.Add("EQPT_MOUNT_PSTN_ID", typeof(string));
            input_LOT.Columns.Add("EQPT_MOUNT_PSTN_STATE", typeof(string));
            input_LOT.Columns.Add("INPUT_LOTID", typeof(string));

            return indataSet;
        }

        public DataSet GetBR_PRD_REG_DISPATCH_LOT_FD()
        {
            DataSet indataSet = new DataSet();

            DataTable inDataTable = indataSet.Tables.Add("INDATA");
            inDataTable.Columns.Add("SRCTYPE", typeof(string));
            //inDataTable.Columns.Add("LANGID", typeof(string));
            inDataTable.Columns.Add("IFMODE", typeof(string));
            inDataTable.Columns.Add("EQPTID", typeof(string));
            inDataTable.Columns.Add("EQSGID", typeof(string));
            inDataTable.Columns.Add("REWORK", typeof(string));
            inDataTable.Columns.Add("USERID", typeof(string));

            DataTable input_LOT = indataSet.Tables.Add("INLOT");
            input_LOT.Columns.Add("LOTID", typeof(string));
            input_LOT.Columns.Add("ACTQTY", typeof(int));
            input_LOT.Columns.Add("ACTUQTY", typeof(int));
            input_LOT.Columns.Add("WIPNOTE", typeof(string));

            return indataSet;
        }

        public DataSet GetBR_PRD_GET_NEW_LOT_FD()
        {
            DataSet indataSet = new DataSet();

            DataTable inDataTable = indataSet.Tables.Add("IN_EQP");
            inDataTable.Columns.Add("SRCTYPE", typeof(string));
            //inDataTable.Columns.Add("LANGID", typeof(string));
            inDataTable.Columns.Add("IFMODE", typeof(string));
            inDataTable.Columns.Add("EQPTID", typeof(string));
            //inDataTable.Columns.Add("NEXTDAY", typeof(string));
            inDataTable.Columns.Add("USERID", typeof(string));

            DataTable input_LOT = indataSet.Tables.Add("IN_INPUT");
            input_LOT.Columns.Add("EQPT_MOUNT_PSTN_ID", typeof(string));
            input_LOT.Columns.Add("EQPT_MOUNT_PSTN_STATE", typeof(int));
            input_LOT.Columns.Add("INPUT_LOTID", typeof(string));

            return indataSet;
        }

        public DataTable GetDA_PRD_SEL_NOT_DISPATCH_CNT()
        {
            DataTable inDataTable = new DataTable();
            inDataTable.Columns.Add("PR_LOTID", typeof(string));

            return inDataTable;
        }

        public DataTable GetDA_PRD_SEL_PRINT_YN()
        {
            DataTable inDataTable = new DataTable();
            inDataTable.Columns.Add("LOTID", typeof(string));
            inDataTable.Columns.Add("WIPSEQ", typeof(int));

            return inDataTable;
        }

        public DataTable GetBR_PRD_REG_CREATE_REWORK_MAG_FD()
        {
            DataTable inDataTable = new DataTable();
            inDataTable.Columns.Add("SRCTYPE", typeof(string));
            inDataTable.Columns.Add("IFMODE", typeof(string));
            inDataTable.Columns.Add("EQPTID", typeof(string));
            inDataTable.Columns.Add("PRODID", typeof(string));
            inDataTable.Columns.Add("CSTID", typeof(string));
            inDataTable.Columns.Add("WIPQTY", typeof(int));
            inDataTable.Columns.Add("USERID", typeof(string));
            inDataTable.Columns.Add("MGZN_RECONF_TYPE_CODE", typeof(string));

            return inDataTable;
        }

        public DataTable GetDA_PRD_SEL_INPUT_MAG_PROD_INFO_FD()
        {
            DataTable inDataTable = new DataTable();
            inDataTable.Columns.Add("LANGID", typeof(string));
            inDataTable.Columns.Add("PROCID", typeof(string));
            inDataTable.Columns.Add("EQSGID", typeof(string));
            inDataTable.Columns.Add("EQPTID", typeof(string));
            inDataTable.Columns.Add("FROM_DT", typeof(string));
            inDataTable.Columns.Add("TO_DT", typeof(string));

            return inDataTable;
        }

        public DataSet GetBR_PRD_REG_LOTSTART_FD()
        {
            DataSet indataSet = new DataSet();

            DataTable inDataTable = indataSet.Tables.Add("IN_EQP");
            inDataTable.Columns.Add("SRCTYPE", typeof(string));
            //inDataTable.Columns.Add("LANGID", typeof(string));
            inDataTable.Columns.Add("IFMODE", typeof(string));
            inDataTable.Columns.Add("EQPTID", typeof(string));
            inDataTable.Columns.Add("PROD_LOTID", typeof(string));
            inDataTable.Columns.Add("WO_DETL_ID", typeof(string));
            inDataTable.Columns.Add("USERID", typeof(string));

            DataTable input_LOT = indataSet.Tables.Add("IN_INPUT");
            input_LOT.Columns.Add("EQPT_MOUNT_PSTN_ID", typeof(string));
            input_LOT.Columns.Add("EQPT_MOUNT_PSTN_STATE", typeof(string));
            input_LOT.Columns.Add("CSTID", typeof(string));
            input_LOT.Columns.Add("INPUT_LOTID", typeof(string));

            return indataSet;
        }

        public DataTable GetDA_PRD_SEL_WAIT_LOT_LIST_BY_LINE_FD()
        {
            DataTable inDataTable = new DataTable();
            inDataTable.Columns.Add("LANGID", typeof(string));
            inDataTable.Columns.Add("PROCID", typeof(string));
            inDataTable.Columns.Add("EQPTID", typeof(string));
            inDataTable.Columns.Add("EQSGID", typeof(string));
            inDataTable.Columns.Add("ELECTYPE", typeof(string));
            inDataTable.Columns.Add("LOTID", typeof(string));

            return inDataTable;
        }

        public DataTable GetDA_PRD_SEL_WIPINFO_ST()
        {
            DataTable inDataTable = new DataTable();
            inDataTable.Columns.Add("LANGID", typeof(string));
            inDataTable.Columns.Add("WOID", typeof(string));
            inDataTable.Columns.Add("EQPTID", typeof(string));
            inDataTable.Columns.Add("PROCID", typeof(string));
            inDataTable.Columns.Add("EQSGID", typeof(string));

            return inDataTable;
        }

        public DataTable GetDA_PRD_SEL_WAIT_LOT_LIST_ST()
        {
            DataTable inDataTable = new DataTable();
            inDataTable.Columns.Add("LANGID", typeof(string));
            inDataTable.Columns.Add("PROCID", typeof(string));
            inDataTable.Columns.Add("EQSGID", typeof(string));
            inDataTable.Columns.Add("EQPTID", typeof(string));
            inDataTable.Columns.Add("WOID", typeof(string));
            inDataTable.Columns.Add("PRODID", typeof(string));
            inDataTable.Columns.Add("PRODUCT_LEVEL2_CODE", typeof(string));
            inDataTable.Columns.Add("PRODUCT_LEVEL3_CODE", typeof(string));
            inDataTable.Columns.Add("LOTID", typeof(string));

            return inDataTable;
        }

        public DataSet GetBR_PRD_GET_NEW_LOT_ST()
        {
            DataSet indataSet = new DataSet();

            DataTable inDataTable = indataSet.Tables.Add("INDATA");
            inDataTable.Columns.Add("SRCTYPE", typeof(string));
            //inDataTable.Columns.Add("LANGID", typeof(string));
            inDataTable.Columns.Add("IFMODE", typeof(string));
            inDataTable.Columns.Add("EQPTID", typeof(string));
            inDataTable.Columns.Add("NEXTDAY", typeof(string));
            inDataTable.Columns.Add("USERID", typeof(string));

            DataTable input_LOT = indataSet.Tables.Add("IN_INPUT");
            input_LOT.Columns.Add("EQPT_MOUNT_PSTN_ID", typeof(string));
            input_LOT.Columns.Add("EQPT_MOUNT_PSTN_STATE", typeof(int));
            input_LOT.Columns.Add("INPUT_LOTID", typeof(string));

            return indataSet;
        }

        public DataSet GetBR_PRD_REG_LOTSTART_ST()
        {
            DataSet indataSet = new DataSet();

            DataTable inDataTable = indataSet.Tables.Add("IN_EQP");
            inDataTable.Columns.Add("SRCTYPE", typeof(string));
            //inDataTable.Columns.Add("LANGID", typeof(string));
            inDataTable.Columns.Add("IFMODE", typeof(string));
            inDataTable.Columns.Add("EQPTID", typeof(string));
            inDataTable.Columns.Add("PROD_LOTID", typeof(string));
            inDataTable.Columns.Add("WO_DETL_ID", typeof(string));
            inDataTable.Columns.Add("USERID", typeof(string));

            DataTable input_LOT = indataSet.Tables.Add("IN_INPUT");
            input_LOT.Columns.Add("EQPT_MOUNT_PSTN_ID", typeof(string));
            input_LOT.Columns.Add("EQPT_MOUNT_PSTN_STATE", typeof(string));
            input_LOT.Columns.Add("CSTID", typeof(string));
            input_LOT.Columns.Add("INPUT_LOTID", typeof(string));

            return indataSet;
        }

        public DataTable GetBR_PRD_REG_CREATE_BOX_ST()
        {
            DataTable inDataTable = new DataTable();
            inDataTable.Columns.Add("SRCTYPE", typeof(string));
            inDataTable.Columns.Add("LANGID", typeof(string));
            inDataTable.Columns.Add("IFMODE", typeof(string));
            inDataTable.Columns.Add("EQPTID", typeof(string));
            inDataTable.Columns.Add("WO_DETL_ID", typeof(string));
            inDataTable.Columns.Add("PROD_LOTID", typeof(string));
            inDataTable.Columns.Add("OUT_LOTID", typeof(string));
            inDataTable.Columns.Add("CSTID", typeof(string));
            inDataTable.Columns.Add("INPUTQTY", typeof(int));
            inDataTable.Columns.Add("OUTPUTQTY", typeof(int));
            inDataTable.Columns.Add("RESNQTY", typeof(int));
            inDataTable.Columns.Add("SHIFT", typeof(string));
            inDataTable.Columns.Add("WIPNOTE", typeof(string));
            inDataTable.Columns.Add("WRK_USER_NAME", typeof(string));
            inDataTable.Columns.Add("USERID", typeof(string));

            return inDataTable;
        }

        public DataSet GetBR_PRD_REG_DELETE_BOX_ST()
        {
            DataSet indataSet = new DataSet();

            DataTable inDataTable = indataSet.Tables.Add("INDATA");
            inDataTable.Columns.Add("SRCTYPE", typeof(string));
            inDataTable.Columns.Add("IFMODE", typeof(string));
            inDataTable.Columns.Add("EQPTID", typeof(string));
            inDataTable.Columns.Add("PROD_LOTID", typeof(string));
            inDataTable.Columns.Add("USERID", typeof(string));

            DataTable input_LOT = indataSet.Tables.Add("IN_INPUT");
            input_LOT.Columns.Add("OUT_LOTID", typeof(string));

            return indataSet;
        }

        public DataTable GetDA_PRD_SEL_OUT_BOX_ST()
        {
            DataTable inDataTable = new DataTable();
            inDataTable.Columns.Add("LANGID", typeof(string));
            //inDataTable.Columns.Add("PROCID", typeof(string));
            //inDataTable.Columns.Add("EQSGID", typeof(string));
            //inDataTable.Columns.Add("EQPTID", typeof(string));
            inDataTable.Columns.Add("PR_LOTID", typeof(string));

            return inDataTable;
        }

        public DataTable GetBR_PRD_GET_NEW_OUT_LOT_ST()
        {
            DataTable inDataTable = new DataTable();
            inDataTable.Columns.Add("SRCTYPE", typeof(string));
            inDataTable.Columns.Add("IFMODE", typeof(string));
            inDataTable.Columns.Add("EQPTID", typeof(string));
            inDataTable.Columns.Add("PROD_LOTID", typeof(string));
            //inDataTable.Columns.Add("WIP_TYPE_CODE", typeof(string));
            //inDataTable.Columns.Add("CALDATE_YMD", typeof(string));
            inDataTable.Columns.Add("USERID", typeof(string));
            inDataTable.Columns.Add("CSTID", typeof(string));

            return inDataTable;
        }

        public DataTable GetDA_PRD_SEL_WAIT_LOT_LIST_BY_LINE_ST()
        {
            DataTable inDataTable = new DataTable();
            inDataTable.Columns.Add("LANGID", typeof(string));
            inDataTable.Columns.Add("PROCID", typeof(string));
            inDataTable.Columns.Add("EQPTID", typeof(string));
            inDataTable.Columns.Add("EQSGID", typeof(string));
            inDataTable.Columns.Add("ELECTYPE", typeof(string));
            inDataTable.Columns.Add("LOTID", typeof(string));

            return inDataTable;
        }

        public DataTable GetBR_PRD_GET_TEST_LABEL_ST()
        {
            DataTable inDataTable = new DataTable();
            inDataTable.Columns.Add("LANGID", typeof(string));
            inDataTable.Columns.Add("LOTID", typeof(string));
            inDataTable.Columns.Add("QTY", typeof(string));
            inDataTable.Columns.Add("DATE", typeof(string));
            inDataTable.Columns.Add("PRMK", typeof(string));
            inDataTable.Columns.Add("RESO", typeof(string));
            inDataTable.Columns.Add("PRCN", typeof(string));
            inDataTable.Columns.Add("MARH", typeof(string));
            inDataTable.Columns.Add("MARV", typeof(string));

            return inDataTable;
        }

        public DataTable GetBR_PRD_GET_PROCESS_LOT_LABEL_ST()
        {
            DataTable inDataTable = new DataTable();
            inDataTable.Columns.Add("LANGID", typeof(string));
            inDataTable.Columns.Add("PROCID", typeof(string));
            inDataTable.Columns.Add("EQPTID", typeof(string));
            inDataTable.Columns.Add("LOTID", typeof(string));
            inDataTable.Columns.Add("WIPSEQ", typeof(string));
            inDataTable.Columns.Add("PRMK", typeof(string));
            inDataTable.Columns.Add("RESO", typeof(string));
            inDataTable.Columns.Add("PRCN", typeof(string));
            inDataTable.Columns.Add("MARH", typeof(string));
            inDataTable.Columns.Add("MARV", typeof(string));

            return inDataTable;
        }

        public DataSet GetBR_PRD_REG_CONFIRM_LOT_ST()
        {
            DataSet indataSet = new DataSet();

            DataTable inDataTable = indataSet.Tables.Add("INDATA");
            inDataTable.Columns.Add("SRCTYPE", typeof(string));
            inDataTable.Columns.Add("IFMODE", typeof(string));
            inDataTable.Columns.Add("EQPTID", typeof(string));
            inDataTable.Columns.Add("PROD_LOTID", typeof(string));
            inDataTable.Columns.Add("INPUTQTY", typeof(int));
            inDataTable.Columns.Add("OUTPUTQTY", typeof(int));
            inDataTable.Columns.Add("RESNQTY", typeof(int));
            inDataTable.Columns.Add("SHIFT", typeof(string));
            inDataTable.Columns.Add("WIPDTTM_ED", typeof(DateTime));
            inDataTable.Columns.Add("WIPNOTE", typeof(string));
            inDataTable.Columns.Add("WRK_USERID", typeof(string));
            inDataTable.Columns.Add("WRK_USER_NAME", typeof(string));
            inDataTable.Columns.Add("USERID", typeof(string));

            DataTable input_LOT = indataSet.Tables.Add("IN_INPUT");
            input_LOT.Columns.Add("EQPT_MOUNT_PSTN_ID", typeof(string));
            input_LOT.Columns.Add("EQPT_MOUNT_PSTN_STATE", typeof(string));
            input_LOT.Columns.Add("INPUT_LOTID", typeof(string));

            return indataSet;
        }

        public DataTable GetDA_PRD_SEL_WIPINFO_CL()
        {
            DataTable inDataTable = new DataTable();
            inDataTable.Columns.Add("LANGID", typeof(string));
            inDataTable.Columns.Add("PROCID", typeof(string));
            inDataTable.Columns.Add("WOID", typeof(string));
            inDataTable.Columns.Add("EQPTID", typeof(string));
            inDataTable.Columns.Add("EQSGID", typeof(string));

            return inDataTable;
        }

        public DataTable GetDA_PRD_SEL_NOW_PROD_INFO()
        {
            DataTable inDataTable = new DataTable();
            inDataTable.Columns.Add("PROCID", typeof(string));
            inDataTable.Columns.Add("EQPTID", typeof(string));

            return inDataTable;
        }

        public DataTable GetDA_PRD_SEL_EIOINFO_CL()
        {
            DataTable inDataTable = new DataTable();

            inDataTable.Columns.Add("EQPTID", typeof(string));

            return inDataTable;
        }

        public DataTable GetDA_PRD_SEL_WAIT_LIST_CL()
        {
            DataTable inDataTable = new DataTable();

            inDataTable.Columns.Add("LANGID", typeof(string));
            inDataTable.Columns.Add("PROCID", typeof(string));
            inDataTable.Columns.Add("EQPTID", typeof(string));
            inDataTable.Columns.Add("EQSGID", typeof(string));
            inDataTable.Columns.Add("WO_DETL_ID", typeof(string));
            inDataTable.Columns.Add("LOTID", typeof(string));

            return inDataTable;
        }

        public DataTable GetDA_PRD_SEL_IN_BOX_LIST_CL()
        {
            DataTable inDataTable = new DataTable();

            inDataTable.Columns.Add("LANGID", typeof(string));
            inDataTable.Columns.Add("LOTID", typeof(string));
            inDataTable.Columns.Add("WIPSEQ", typeof(int));

            return inDataTable;
        }

        public DataTable GetDA_PRD_SEL_INPUT_LIST_CL()
        {
            DataTable inDataTable = new DataTable();

            inDataTable.Columns.Add("LANGID", typeof(string));
            inDataTable.Columns.Add("PROCID", typeof(string));
            inDataTable.Columns.Add("EQSGID", typeof(string));
            inDataTable.Columns.Add("EQPTID", typeof(string));
            inDataTable.Columns.Add("PROD_LOTID", typeof(string));
            inDataTable.Columns.Add("PROD_WIPSEQ", typeof(int));

            return inDataTable;
        }

        public DataTable GetDA_PRD_SEL_OUT_LIST_CL()
        {
            DataTable inDataTable = new DataTable();

            inDataTable.Columns.Add("LANGID", typeof(string));
            inDataTable.Columns.Add("AREAID", typeof(string));
            inDataTable.Columns.Add("PR_LOTID", typeof(string));
            inDataTable.Columns.Add("PROCID", typeof(string));
            inDataTable.Columns.Add("EQSGID", typeof(string));
            inDataTable.Columns.Add("EQPTID", typeof(string));
            inDataTable.Columns.Add("TRAYID", typeof(string));
            inDataTable.Columns.Add("CELL_TRACE_FLAG", typeof(string));

            return inDataTable;
        }

        public DataTable GetDA_PRD_SEL_CST_INFO_CL()
        {
            DataTable inDataTable = new DataTable();

            inDataTable.Columns.Add("AREAID", typeof(string));
            inDataTable.Columns.Add("CST_TYPE_CODE", typeof(string));

            return inDataTable;
        }

        public DataSet GetBR_PRD_REG_LOTSTART_CL()
        {
            DataSet indataSet = new DataSet();

            DataTable inDataTable = indataSet.Tables.Add("IN_EQP");
            inDataTable.Columns.Add("SRCTYPE", typeof(string));
            inDataTable.Columns.Add("IFMODE", typeof(string));
            inDataTable.Columns.Add("EQPTID", typeof(string));
            inDataTable.Columns.Add("PROD_LOTID", typeof(string));
            inDataTable.Columns.Add("WO_DETL_ID", typeof(string));
            inDataTable.Columns.Add("USERID", typeof(string));

            DataTable inMtrl = indataSet.Tables.Add("IN_INPUT");
            inMtrl.Columns.Add("INPUT_LOTID", typeof(string));
            inMtrl.Columns.Add("MTRLID", typeof(string));
            inMtrl.Columns.Add("EQPT_MOUNT_PSTN_ID", typeof(string));
            inMtrl.Columns.Add("EQPT_MOUNT_PSTN_STATE", typeof(string));

            return indataSet;
        }

        public DataSet GetBR_PRD_GET_REQLOT_CL()
        {
            DataSet indataSet = new DataSet();

            DataTable inDataTable = indataSet.Tables.Add("IN_EQP");
            inDataTable.Columns.Add("SRCTYPE", typeof(string));
            inDataTable.Columns.Add("IFMODE", typeof(string));
            inDataTable.Columns.Add("EQPTID", typeof(string));
            inDataTable.Columns.Add("NEXT_DAY_WORK", typeof(string));
            inDataTable.Columns.Add("REQTYPE", typeof(string));
            inDataTable.Columns.Add("USERID", typeof(string));

            DataTable inMtrl = indataSet.Tables.Add("IN_INPUT");
            inMtrl.Columns.Add("INPUT_LOTID", typeof(string));
            inMtrl.Columns.Add("MTRLID", typeof(string));
            inMtrl.Columns.Add("EQPT_MOUNT_PSTN_ID", typeof(string));
            inMtrl.Columns.Add("EQPT_MOUNT_PSTN_STATE", typeof(string));

            return indataSet;
        }

        public DataTable GetBR_PRD_REG_CONFIRM_LOT_CL()
        {
            DataTable inDataTable = new DataTable();

            inDataTable.Columns.Add("SRCTYPE", typeof(string));
            inDataTable.Columns.Add("IFMODE", typeof(string));
            inDataTable.Columns.Add("EQPTID", typeof(string));
            inDataTable.Columns.Add("LOTID", typeof(string));
            inDataTable.Columns.Add("INPUTQTY", typeof(int));
            inDataTable.Columns.Add("OUTPUTQTY", typeof(int));
            inDataTable.Columns.Add("RESNQTY", typeof(int));
            inDataTable.Columns.Add("SHIFT", typeof(string));
            inDataTable.Columns.Add("WIPDTTM_ED", typeof(DateTime));
            inDataTable.Columns.Add("WIPNOTE", typeof(string));
            inDataTable.Columns.Add("WRK_USERID", typeof(string));
            inDataTable.Columns.Add("WRK_USER_NAME", typeof(string));
            inDataTable.Columns.Add("USERID", typeof(string));
            inDataTable.Columns.Add("INPUT_DIFF_QTY", typeof(int));

            return inDataTable;
        }

        public DataTable GetDA_PRD_SEL_WAIT_LOT_LIST_BY_LINE_CL()
        {
            DataTable inDataTable = new DataTable();
            inDataTable.Columns.Add("LANGID", typeof(string));
            inDataTable.Columns.Add("PROCID", typeof(string));
            inDataTable.Columns.Add("EQSGID", typeof(string));
            inDataTable.Columns.Add("LOTID", typeof(string));

            return inDataTable;
        }

        public DataTable GetDA_PRD_SEL_OUT_TRAY_CL()
        {
            DataTable inDataTable = new DataTable();

            inDataTable.Columns.Add("LANGID", typeof(string));
            inDataTable.Columns.Add("LOTID", typeof(string));

            return inDataTable;
        }

        public DataTable GetBR_PRD_REG_SPCL_TRAY_SAVE_CL()
        {
            DataTable inDataTable = new DataTable();

            inDataTable.Columns.Add("SRCTYPE", typeof(string));
            inDataTable.Columns.Add("IFMODE", typeof(string));
            inDataTable.Columns.Add("EQPTID", typeof(string));
            inDataTable.Columns.Add("SPCL_LOT_GNRT_FLAG", typeof(string));
            inDataTable.Columns.Add("SPCL_LOT_RSNCODE", typeof(string));
            inDataTable.Columns.Add("SPCL_LOT_NOTE", typeof(string));
            inDataTable.Columns.Add("SPCL_PROD_LOTID", typeof(string));
            inDataTable.Columns.Add("USERID", typeof(string));

            return inDataTable;
        }

        public DataTable GetDA_PRD_SEL_SPCL_TRAY_INFO_CL()
        {
            DataTable inDataTable = new DataTable();

            inDataTable.Columns.Add("EQPTID", typeof(string));

            return inDataTable;
        }

        public DataTable GetBR_PRD_REG_EQP_END_CL()
        {
            DataTable inDataTable = new DataTable();

            inDataTable.Columns.Add("LANGID", typeof(string));
            inDataTable.Columns.Add("LOTID", typeof(string));

            return inDataTable;
        }

        public DataTable GetDA_PRD_SEL_MTRL_CL()
        {
            DataTable inDataTable = new DataTable();

            inDataTable.Columns.Add("LANGID", typeof(string));
            inDataTable.Columns.Add("LOTID", typeof(string));
            inDataTable.Columns.Add("WIPSEQ", typeof(int));

            return inDataTable;
        }

        public DataSet GetBR_PRD_REG_IN_CANCEL_CL()
        {
            DataSet indataSet = new DataSet();

            DataTable inDataTable = indataSet.Tables.Add("INDATA");
            inDataTable.Columns.Add("SRCTYPE", typeof(string));
            inDataTable.Columns.Add("IFMODE", typeof(string));
            inDataTable.Columns.Add("EQPTID", typeof(string));
            inDataTable.Columns.Add("PROD_LOTID", typeof(string));
            inDataTable.Columns.Add("USERID", typeof(string));

            DataTable inMtrl = indataSet.Tables.Add("IN_INPUT");
            inMtrl.Columns.Add("INPUT_LOTID", typeof(string));
            inMtrl.Columns.Add("WIPNOTE", typeof(string));
            inMtrl.Columns.Add("EQPT_MOUNT_PSTN_ID", typeof(string));
            inMtrl.Columns.Add("INPUT_SEQNO", typeof(Int64));

            return indataSet;
        }

        public DataSet GetBR_PRD_REG_IN_COMPLETE_CL()
        {
            DataSet indataSet = new DataSet();

            DataTable inDataTable = indataSet.Tables.Add("INDATA");
            inDataTable.Columns.Add("SRCTYPE", typeof(string));
            inDataTable.Columns.Add("IFMODE", typeof(string));
            inDataTable.Columns.Add("EQPTID", typeof(string));
            inDataTable.Columns.Add("PROCID", typeof(string));
            inDataTable.Columns.Add("PROD_LOTID", typeof(string));
            inDataTable.Columns.Add("OUT_LOTID", typeof(string));
            inDataTable.Columns.Add("USERID", typeof(string));

            DataTable inMtrl = indataSet.Tables.Add("IN_INPUT");
            inMtrl.Columns.Add("INPUT_LOTID", typeof(string));
            inMtrl.Columns.Add("MTRLID", typeof(string));
            inMtrl.Columns.Add("EQPT_MOUNT_PSTN_ID", typeof(string));
            inMtrl.Columns.Add("EQPT_MOUNT_PSTN_STATE", typeof(string));
            inMtrl.Columns.Add("ACTQTY", typeof(int));

            return indataSet;
        }

        public DataSet GetBR_PRD_REG_INPUT_BASKET_CL()
        {
            DataSet indataSet = new DataSet();

            DataTable inDataTable = indataSet.Tables.Add("IN_EQP");
            inDataTable.Columns.Add("SRCTYPE", typeof(string));
            inDataTable.Columns.Add("IFMODE", typeof(string));
            inDataTable.Columns.Add("EQPTID", typeof(string));
            inDataTable.Columns.Add("PROD_LOTID", typeof(string));
            inDataTable.Columns.Add("USERID", typeof(string));

            DataTable inMtrl = indataSet.Tables.Add("IN_INPUT");
            inMtrl.Columns.Add("INPUT_LOTID", typeof(string));
            inMtrl.Columns.Add("MTRLID", typeof(string));
            inMtrl.Columns.Add("EQPT_MOUNT_PSTN_ID", typeof(string));
            inMtrl.Columns.Add("EQPT_MOUNT_PSTN_STATE", typeof(string));
            inMtrl.Columns.Add("ACTQTY", typeof(int));
            inMtrl.Columns.Add("CSTID", typeof(string));

            return indataSet;
        }

        public DataSet GetBR_PRD_REG_INPUT_CANCEL_BASKET_CL()
        {
            DataSet indataSet = new DataSet();

            DataTable inDataTable = indataSet.Tables.Add("INDATA");
            inDataTable.Columns.Add("SRCTYPE", typeof(string));
            inDataTable.Columns.Add("IFMODE", typeof(string));
            inDataTable.Columns.Add("USERID", typeof(string));

            DataTable inMtrl = indataSet.Tables.Add("INLOT");
            inMtrl.Columns.Add("INPUT_SEQNO", typeof(string));
            inMtrl.Columns.Add("LOTID", typeof(string));
            inMtrl.Columns.Add("WIPQTY", typeof(Decimal));
            inMtrl.Columns.Add("WIPQTY2", typeof(Decimal));
            inMtrl.Columns.Add("CSTID", typeof(string));

            return indataSet;
        }

        public DataSet GetBR_PRD_REG_INPUT_CANCEL_BASKET2_CL()
        {
            DataSet indataSet = new DataSet();

            DataTable inDataTable = indataSet.Tables.Add("INDATA");
            inDataTable.Columns.Add("SRCTYPE", typeof(string));
            inDataTable.Columns.Add("IFMODE", typeof(string));
            inDataTable.Columns.Add("USERID", typeof(string));
            inDataTable.Columns.Add("PROD_LOTID", typeof(string));
            inDataTable.Columns.Add("PROCID", typeof(string));

            DataTable inMtrl = indataSet.Tables.Add("INLOT");
            inMtrl.Columns.Add("INPUT_SEQNO", typeof(string));
            inMtrl.Columns.Add("LOTID", typeof(string));
            inMtrl.Columns.Add("WIPQTY", typeof(Decimal));
            inMtrl.Columns.Add("WIPQTY2", typeof(Decimal));
            inMtrl.Columns.Add("CSTID", typeof(string));

            return indataSet;
        }

        public DataSet GetBR_PRD_REG_IN_MATERIAL_CL()
        {
            DataSet indataSet = new DataSet();

            DataTable inDataTable = indataSet.Tables.Add("IN_EQP");
            inDataTable.Columns.Add("SRCTYPE", typeof(string));
            inDataTable.Columns.Add("IFMODE", typeof(string));
            inDataTable.Columns.Add("EQPTID", typeof(string));
            inDataTable.Columns.Add("PROD_LOTID", typeof(string));
            inDataTable.Columns.Add("USERID", typeof(string));

            DataTable inMtrl = indataSet.Tables.Add("IN_INPUT");
            inMtrl.Columns.Add("INPUT_LOTID", typeof(string));
            inMtrl.Columns.Add("MTRLID", typeof(string));
            inMtrl.Columns.Add("EQPT_MOUNT_PSTN_ID", typeof(string));
            inMtrl.Columns.Add("EQPT_MOUNT_PSTN_STATE", typeof(string));
            inMtrl.Columns.Add("ACTQTY", typeof(int));

            return indataSet;
        }

        public DataTable GetBR_PRD_REG_IN_MATERIAL_DEL_CL()
        {
            DataTable inDataTable = new DataTable();

            inDataTable.Columns.Add("LANGID", typeof(string));
            inDataTable.Columns.Add("LOTID", typeof(string));

            return inDataTable;
        }

        public DataTable GetDA_PRD_SEL_TRAY_INFO_CL()
        {
            DataTable inDataTable = new DataTable();

            inDataTable.Columns.Add("LANGID", typeof(string));
            inDataTable.Columns.Add("PROD_LOTID", typeof(string));
            inDataTable.Columns.Add("CSTID", typeof(string));

            return inDataTable;
        }

        public DataTable GetDA_PRD_SEL_CELL_INFO_CL()
        {
            DataTable inDataTable = new DataTable();

            inDataTable.Columns.Add("LANGID", typeof(string));
            inDataTable.Columns.Add("LOTID", typeof(string));
            inDataTable.Columns.Add("TRAYID", typeof(string));

            return inDataTable;
        }

        public DataTable GetDA_PRD_SEL_CELLTRACE_INFO_CL()
        {
            DataTable inDataTable = new DataTable();

            inDataTable.Columns.Add("LANGID", typeof(string));
            inDataTable.Columns.Add("LOTID", typeof(string));
            inDataTable.Columns.Add("TRAYID", typeof(string));

            return inDataTable;
        }

        public DataTable GetBR_PRD_GET_FCS_TRAY_CHK_CL()
        {
            DataTable inDataTable = new DataTable();

            inDataTable.Columns.Add("LANGID", typeof(string));
            inDataTable.Columns.Add("LOTID", typeof(string));
            inDataTable.Columns.Add("TRAYID", typeof(string));

            return inDataTable;
        }

        public DataSet GetBR_PRD_REG_TRAY_CONFIRM_CL()
        {
            DataSet indataSet = new DataSet();

            DataTable inDataTable = indataSet.Tables.Add("IN_EQP");
            inDataTable.Columns.Add("SRCTYPE", typeof(string));
            inDataTable.Columns.Add("IFMODE", typeof(string));
            inDataTable.Columns.Add("EQPTID", typeof(string));
            inDataTable.Columns.Add("PROD_LOTID", typeof(string));
            inDataTable.Columns.Add("USERID", typeof(string));

            DataTable inCst = indataSet.Tables.Add("IN_CST");
            inCst.Columns.Add("OUT_LOTID", typeof(string));
            inCst.Columns.Add("OUTPUT_QTY", typeof(int));
            inCst.Columns.Add("CSTID", typeof(string));
            inCst.Columns.Add("SPCL_CST_GNRT_FLAG", typeof(string));
            inCst.Columns.Add("SPCL_CST_NOTE", typeof(string));
            inCst.Columns.Add("SPCL_CST_RSNCODE", typeof(string));

            return indataSet;
        }

        public DataTable GetBR_PRD_REG_TRAY_DEL_CL()
        {
            DataTable inDataTable = new DataTable();

            inDataTable.Columns.Add("SRCTYPE", typeof(string));
            inDataTable.Columns.Add("IFMODE", typeof(string));
            inDataTable.Columns.Add("EQPTID", typeof(string));
            inDataTable.Columns.Add("PROD_LOTID", typeof(string));
            inDataTable.Columns.Add("OUT_LOTID", typeof(string));
            inDataTable.Columns.Add("TRAYID", typeof(string));
            inDataTable.Columns.Add("WO_DETL_ID", typeof(string));
            inDataTable.Columns.Add("USERID", typeof(string));

            return inDataTable;
        }

        public DataTable GetBR_PRD_REG_BAD_TRAY_CL()
        {
            DataTable inDataTable = new DataTable();

            inDataTable.Columns.Add("LANGID", typeof(string));
            inDataTable.Columns.Add("LOTID", typeof(string));
            inDataTable.Columns.Add("TRAYID", typeof(string));

            return inDataTable;
        }

        public DataSet GetBR_PRD_REG_CREATE_TRAY_CL()
        {
            DataSet indataSet = new DataSet();

            DataTable inDataTable = indataSet.Tables.Add("IN_EQP");
            inDataTable.Columns.Add("SRCTYPE", typeof(string));
            inDataTable.Columns.Add("IFMODE", typeof(string));
            inDataTable.Columns.Add("EQPTID", typeof(string));
            inDataTable.Columns.Add("PROD_LOTID", typeof(string));
            inDataTable.Columns.Add("OUT_LOTID", typeof(string));
            inDataTable.Columns.Add("TRAYID", typeof(string));
            inDataTable.Columns.Add("WO_DETL_ID", typeof(string));
            inDataTable.Columns.Add("USERID", typeof(string));
            inDataTable.Columns.Add("CELL_QTY", typeof(decimal));

            DataTable inMtrl = indataSet.Tables.Add("IN_INPUT");
            inMtrl.Columns.Add("INPUT_LOTID", typeof(string));
            inMtrl.Columns.Add("MTRLID", typeof(string));
            inMtrl.Columns.Add("EQPT_MOUNT_PSTN_ID", typeof(string));
            inMtrl.Columns.Add("EQPT_MOUNT_PSTN_STATE", typeof(string));

            DataTable inSpcl = indataSet.Tables.Add("IN_SPCL");
            inSpcl.Columns.Add("SPCL_CST_GNRT_FLAG", typeof(string));
            inSpcl.Columns.Add("SPCL_CST_NOTE", typeof(string));
            inSpcl.Columns.Add("SPCL_CST_RSNCODE", typeof(string));

            return indataSet;
        }

        public DataTable GetBR_PRD_REG_MOVETRAY_CL()
        {
            DataTable inDataTable = new DataTable();

            inDataTable.Columns.Add("SRCTYPE", typeof(string));
            inDataTable.Columns.Add("IFMODE", typeof(string));
            inDataTable.Columns.Add("EQPTID", typeof(string));
            inDataTable.Columns.Add("PROD_LOTID", typeof(string));
            inDataTable.Columns.Add("OUT_LOTID", typeof(string));
            inDataTable.Columns.Add("CSTID", typeof(string));
            inDataTable.Columns.Add("CSTID_NEW", typeof(string));
            inDataTable.Columns.Add("USERID", typeof(string));

            return inDataTable;
        }

        public DataSet GetBR_PRD_REG_TRAY_CONFIRM_CNL_CL()
        {
            DataSet indataSet = new DataSet();

            DataTable inDataTable = indataSet.Tables.Add("IN_EQP");
            inDataTable.Columns.Add("SRCTYPE", typeof(string));
            inDataTable.Columns.Add("IFMODE", typeof(string));
            inDataTable.Columns.Add("EQPTID", typeof(string));
            inDataTable.Columns.Add("PROD_LOTID", typeof(string));
            inDataTable.Columns.Add("USERID", typeof(string));

            DataTable inSpcl = indataSet.Tables.Add("IN_CST");
            inSpcl.Columns.Add("OUT_LOTID", typeof(string));
            inSpcl.Columns.Add("CSTID", typeof(string));

            return indataSet;
        }

        public DataTable GetBR_PRD_REG_TRAY_SPECIAL_CL()
        {
            DataTable inDataTable = new DataTable();

            inDataTable.Columns.Add("LANGID", typeof(string));
            inDataTable.Columns.Add("LOTID", typeof(string));
            inDataTable.Columns.Add("TRAYID", typeof(string));

            return inDataTable;
        }

        public DataSet GetBR_PRD_REG_TRAY_SAVE_CL()
        {
            DataSet indataSet = new DataSet();

            DataTable inDataTable = indataSet.Tables.Add("IN_EQP");
            inDataTable.Columns.Add("SRCTYPE", typeof(string));
            inDataTable.Columns.Add("IFMODE", typeof(string));
            inDataTable.Columns.Add("EQPTID", typeof(string));
            inDataTable.Columns.Add("PROD_LOTID", typeof(string));
            inDataTable.Columns.Add("USERID", typeof(string));

            DataTable inLot = indataSet.Tables.Add("IN_LOT");
            inLot.Columns.Add("OUT_LOTID", typeof(string));
            inLot.Columns.Add("CSTID", typeof(string));
            inLot.Columns.Add("WIPQTY", typeof(int));

            DataTable inSpcl = indataSet.Tables.Add("IN_SPCL");
            inSpcl.Columns.Add("SPCL_CST_GNRT_FLAG", typeof(string));
            inSpcl.Columns.Add("SPCL_CST_NOTE", typeof(string));
            inSpcl.Columns.Add("SPCL_CST_RSNCODE", typeof(string));

            return indataSet;
        }

        public DataTable GetDA_PRD_SEL_WO_SUMMARY_INFO()
        {
            DataTable inDataTable = new DataTable();

            inDataTable.Columns.Add("AREAID", typeof(string));
            inDataTable.Columns.Add("EQPTID", typeof(string));

            return inDataTable;
        }

        public DataTable GetDA_PRD_SEL_WO_SUMMARY_INFO_BY_PROCID()
        {
            DataTable inDataTable = new DataTable();

            inDataTable.Columns.Add("AREAID", typeof(string));
            inDataTable.Columns.Add("EQSGID", typeof(string));
            inDataTable.Columns.Add("PROCID", typeof(string));
            inDataTable.Columns.Add("EQPTID", typeof(string));

            return inDataTable;
        }

        public DataTable GetDA_PRD_SEL_TRAY_CELL_LIST()
        {
            DataTable inDataTable = new DataTable();

            inDataTable.Columns.Add("PROD_LOTID", typeof(string));
            inDataTable.Columns.Add("OUT_LOTID", typeof(string));
            inDataTable.Columns.Add("EQPTID", typeof(string));
            inDataTable.Columns.Add("TRAYID", typeof(string));

            return inDataTable;
        }

        public DataTable GetDA_PRD_SEL_CELL_INFO()
        {
            DataTable inDataTable = new DataTable();

            inDataTable.Columns.Add("LOTID", typeof(string));
            inDataTable.Columns.Add("OUT_LOTID", typeof(string));
            inDataTable.Columns.Add("TRAYID", typeof(string));
            inDataTable.Columns.Add("CELLID", typeof(string));

            return inDataTable;
        }

        public DataTable GetDA_PRD_SEL_CELL_DUP_LIST()
        {
            DataTable inDataTable = new DataTable();

            inDataTable.Columns.Add("LOTID", typeof(string));
            inDataTable.Columns.Add("OUT_LOTID", typeof(string));
            inDataTable.Columns.Add("TRAYID", typeof(string));
            inDataTable.Columns.Add("CELLID", typeof(string));

            return inDataTable;
        }

        public DataSet GetBR_PRD_REG_PUT_SUBLOT_IN_CST_CL()
        {
            DataSet indataSet = new DataSet();

            DataTable inDataTable = indataSet.Tables.Add("IN_EQP");
            inDataTable.Columns.Add("SRCTYPE", typeof(string));
            inDataTable.Columns.Add("IFMODE", typeof(string));
            inDataTable.Columns.Add("EQPTID", typeof(string));
            inDataTable.Columns.Add("PROD_LOTID", typeof(string));
            inDataTable.Columns.Add("OUT_LOTID", typeof(string));
            inDataTable.Columns.Add("CSTID", typeof(string));
            inDataTable.Columns.Add("USERID", typeof(string));

            DataTable inSub = indataSet.Tables.Add("IN_CST");
            inSub.Columns.Add("SUBLOTID", typeof(string));
            inSub.Columns.Add("CSTSLOT", typeof(string));

            return indataSet;
        }

        public DataSet GetBR_PRD_REG_PUT_SUBLOT_IN_CST_CL_S()
        {
            DataSet indataSet = new DataSet();

            DataTable inDataTable = indataSet.Tables.Add("IN_EQP");
            inDataTable.Columns.Add("SRCTYPE", typeof(string));
            inDataTable.Columns.Add("IFMODE", typeof(string));
            inDataTable.Columns.Add("EQPTID", typeof(string));
            inDataTable.Columns.Add("PROD_LOTID", typeof(string));
            inDataTable.Columns.Add("OUT_LOTID", typeof(string));
            inDataTable.Columns.Add("CSTID", typeof(string));
            inDataTable.Columns.Add("USERID", typeof(string));

            DataTable inSub = indataSet.Tables.Add("IN_CST");
            inSub.Columns.Add("SUBLOTID", typeof(string));
            inSub.Columns.Add("CSTSLOT", typeof(string));

            DataTable inEl = indataSet.Tables.Add("IN_EL");
            inEl.Columns.Add("SUBLOTID", typeof(string));
            inEl.Columns.Add("EL_PRE_WEIGHT", typeof(string));
            inEl.Columns.Add("EL_AFTER_WEIGHT", typeof(string));
            inEl.Columns.Add("EL_WEIGHT", typeof(string));
            inEl.Columns.Add("EL_PSTN", typeof(string));
            inEl.Columns.Add("EL_JUDG_VALUE", typeof(string));
            inEl.Columns.Add("VALUE001", typeof(string));
            inEl.Columns.Add("VALUE002", typeof(string));
            inEl.Columns.Add("VALUE003", typeof(string));
            inEl.Columns.Add("VALUE004", typeof(string));
            inEl.Columns.Add("VALUE005", typeof(string));
            inEl.Columns.Add("VALUE006", typeof(string));
            inEl.Columns.Add("VALUE007", typeof(string));
            inEl.Columns.Add("VALUE008", typeof(string));
            inEl.Columns.Add("VALUE009", typeof(string));
            inEl.Columns.Add("VALUE010", typeof(string));
            inEl.Columns.Add("VALUE011", typeof(string));
            inEl.Columns.Add("VALUE012", typeof(string));
            inEl.Columns.Add("VALUE013", typeof(string));
            inEl.Columns.Add("VALUE014", typeof(string));
            inEl.Columns.Add("VALUE015", typeof(string));
            inEl.Columns.Add("VALUE016", typeof(string));
            inEl.Columns.Add("VALUE017", typeof(string));
            inEl.Columns.Add("VALUE018", typeof(string));
            inEl.Columns.Add("VALUE019", typeof(string));
            inEl.Columns.Add("VALUE020", typeof(string));

            return indataSet;
        }

        public DataSet GetBR_PRD_REG_DELETE_SUBLOT_CL()
        {
            DataSet indataSet = new DataSet();

            DataTable inDataTable = indataSet.Tables.Add("INDATA");
            inDataTable.Columns.Add("SRCTYPE", typeof(string));
            inDataTable.Columns.Add("IFMODE", typeof(string));
            inDataTable.Columns.Add("EQPTID", typeof(string));
            inDataTable.Columns.Add("PROD_LOTID", typeof(string));
            inDataTable.Columns.Add("OUT_LOTID", typeof(string));
            inDataTable.Columns.Add("USERID", typeof(string));

            DataTable inSub = indataSet.Tables.Add("IN_CST");
            inSub.Columns.Add("CSTID", typeof(string));
            inSub.Columns.Add("SUBLOTID", typeof(string));

            return indataSet;
        }

        public DataTable GetBR_PRD_CHK_VALID_TRAY()
        {
            DataTable inDataTable = new DataTable();

            inDataTable.Columns.Add("SRCTYPE", typeof(string));
            inDataTable.Columns.Add("IFMODE", typeof(string));
            inDataTable.Columns.Add("EQPTID", typeof(string));
            inDataTable.Columns.Add("PROD_LOTID", typeof(string));
            inDataTable.Columns.Add("OUT_LOTID", typeof(string));
            inDataTable.Columns.Add("TRAYID", typeof(string));
            inDataTable.Columns.Add("USERID", typeof(string));

            return inDataTable;
        }

        public DataSet GetBR_PRD_CHK_VALID_SUBLOTID()
        {
            DataSet indataSet = new DataSet();

            DataTable inDataTable = indataSet.Tables.Add("INDATA");
            inDataTable.Columns.Add("SRCTYPE", typeof(string));
            inDataTable.Columns.Add("IFMODE", typeof(string));
            inDataTable.Columns.Add("EQPTID", typeof(string));
            inDataTable.Columns.Add("PROD_LOTID", typeof(string));
            inDataTable.Columns.Add("OUT_LOTID", typeof(string));
            inDataTable.Columns.Add("TRAYID", typeof(string));
            inDataTable.Columns.Add("USERID", typeof(string));

            DataTable inSub = indataSet.Tables.Add("IN_SUBLOT");
            inSub.Columns.Add("SUBLOTID", typeof(string));
            inSub.Columns.Add("CSTSLOT", typeof(Int32));

            return indataSet;
        }

        public DataTable GetBR_PRD_SEL_TRAY_LOCATION_CNT()
        {
            DataTable inDataTable = new DataTable();

            inDataTable.Columns.Add("PROD_LOTID", typeof(string));
            inDataTable.Columns.Add("OUT_LOTID", typeof(string));
            inDataTable.Columns.Add("CSTID", typeof(string));
            inDataTable.Columns.Add("CSTSLOT", typeof(string));

            return inDataTable;
        }

        public DataTable GetBR_PRD_SEL_DUP_TRAY_LOCATION_LIST()
        {
            DataTable inDataTable = new DataTable();

            inDataTable.Columns.Add("PROD_LOTID", typeof(string));
            inDataTable.Columns.Add("OUT_LOTID", typeof(string));
            inDataTable.Columns.Add("CSTID", typeof(string));
            inDataTable.Columns.Add("CSTSLOT", typeof(string));

            return inDataTable;
        }

        public DataTable GetDA_PRD_SEL_MDLLOT_INFO_CL()
        {
            DataTable inDataTable = new DataTable();

            inDataTable.Columns.Add("AREAID", typeof(string));
            inDataTable.Columns.Add("WOID", typeof(string));

            return inDataTable;
        }

        public DataTable GetDA_PRD_SEL_WIP_INFO_CL()
        {
            DataTable inDataTable = new DataTable();
            inDataTable.Columns.Add("AREAID", typeof(string));
            inDataTable.Columns.Add("LOTID", typeof(string));

            return inDataTable;
        }

        public DataTable GetDA_PRD_SEL_IN_BOX_TOT_QTY_CL()
        {
            DataTable inDataTable = new DataTable();
            inDataTable.Columns.Add("LANGID", typeof(string));
            inDataTable.Columns.Add("PROCID", typeof(string));
            inDataTable.Columns.Add("EQSGID", typeof(string));
            inDataTable.Columns.Add("EQPTID", typeof(string));
            inDataTable.Columns.Add("PROD_LOTID", typeof(string));
            inDataTable.Columns.Add("WIPSEQ", typeof(int));

            return inDataTable;
        }

        public DataTable GetDA_PRD_SEL_LOT_STOCK() // DSF 대기창고 Lot List
        {
            DataTable inDataTable = new DataTable();
            inDataTable.Columns.Add("LANGID", typeof(string));
            inDataTable.Columns.Add("PROCID", typeof(string));
            inDataTable.Columns.Add("DATE_FROM", typeof(string));
            inDataTable.Columns.Add("DATE_TO", typeof(string));
            inDataTable.Columns.Add("WH_ID", typeof(string));
            inDataTable.Columns.Add("RACK_ID", typeof(string));
            inDataTable.Columns.Add("LINEID", typeof(string));
            inDataTable.Columns.Add("WIPYN", typeof(string));
            inDataTable.Columns.Add("PRODID", typeof(string));

            return inDataTable;
        }

        public DataTable GetDA_PRD_SEL_TRAY_STOCK() // DSF 대기창고 Lot 별 Tray List
        {
            DataTable inDataTable = new DataTable();

            inDataTable.Columns.Add("PROCID", typeof(string));
            inDataTable.Columns.Add("DATE_FROM", typeof(string));
            inDataTable.Columns.Add("DATE_TO", typeof(string));
            inDataTable.Columns.Add("WIPYN", typeof(string));
            inDataTable.Columns.Add("LOTID", typeof(string));
            inDataTable.Columns.Add("PRODID", typeof(string));

            return inDataTable;
        }

        public DataTable GetDA_PRD_SEL_WIP_TRAY_DSF() // DSF 공정진척 Tray 재공
        {
            DataTable inDataTable = new DataTable();

            inDataTable.Columns.Add("PROCID", typeof(string));
            inDataTable.Columns.Add("DATE_FROM", typeof(string));
            inDataTable.Columns.Add("DATE_TO", typeof(string));
            inDataTable.Columns.Add("WIPYN", typeof(string));
            inDataTable.Columns.Add("LOTID", typeof(string));
            inDataTable.Columns.Add("PRODID", typeof(string));

            return inDataTable;
        }

        public DataTable GetDA_PRD_SEL_INOUT_HISTORY_STOCK() // DSF 대기창고 이력조회
        {
            DataTable inDataTable = new DataTable();
            inDataTable.Columns.Add("LANGID", typeof(string));
            inDataTable.Columns.Add("PROCID", typeof(string));
            inDataTable.Columns.Add("DATE_FROM", typeof(string));
            inDataTable.Columns.Add("DATE_TO", typeof(string));
            inDataTable.Columns.Add("LINEID", typeof(string));
            inDataTable.Columns.Add("WH_ID", typeof(string));
            inDataTable.Columns.Add("INOUT", typeof(string));

            return inDataTable;
        }

        public DataTable GetDA_PRD_SEL_PRODUCT_STOCK() // DSF 대기창고 제품별 재공 조회
        {
            DataTable inDataTable = new DataTable();
            inDataTable.Columns.Add("LANGID", typeof(string));
            inDataTable.Columns.Add("PROCID", typeof(string));
            inDataTable.Columns.Add("WH_ID", typeof(string));

            return inDataTable;
        }

        public DataTable GetDA_PRD_SEL_TRAY_RESTOCK() // DSF 대기창고 재입고 조회
        {
            DataTable inDataTable = new DataTable();
            inDataTable.Columns.Add("LANGID", typeof(string));
            inDataTable.Columns.Add("PROCID", typeof(string));
            inDataTable.Columns.Add("CSTID", typeof(string));

            return inDataTable;
        }

        public DataSet GetBR_PRD_REG_TRAY_STOCK_OUT() // Tray 출고 Biz
        {
            DataSet inDataSet = new DataSet();

            DataTable inDataTable = inDataSet.Tables.Add("INDATA");
            inDataTable.Columns.Add("SRCTYPE", typeof(string));
            inDataTable.Columns.Add("USERID", typeof(string));

            DataTable inputLOT = inDataSet.Tables.Add("INLOT");
            inputLOT.Columns.Add("CSTID", typeof(string));

            return inDataSet;
        }

        public DataSet GetBR_PRD_REG_TRAY_RESTOCK() // Tray 재입고 Biz
        {
            DataSet inDataSet = new DataSet();

            DataTable inDataTable = inDataSet.Tables.Add("INDATA");
            inDataTable.Columns.Add("SRCTYPE", typeof(string));
            inDataTable.Columns.Add("USERID", typeof(string));

            DataTable inputLOT = inDataSet.Tables.Add("INLOT");
            inputLOT.Columns.Add("CSTID", typeof(string));

            return inDataSet;
        }

        public DataTable GetDA_PRD_SEL_LABEL_PRINT_TARGET_STOCK() // DSF 대기창고 입고 라벨발행 대상조회
        {
            DataTable inDataTable = new DataTable();
            inDataTable.Columns.Add("LANGID", typeof(string));
            inDataTable.Columns.Add("PROCID", typeof(string));
            inDataTable.Columns.Add("WH_ID", typeof(string));
            inDataTable.Columns.Add("LOTID", typeof(string));

            return inDataTable;
        }

        public DataTable GetBR_PRD_GET_PROCESS_LOT_LABEL_STOCK() // DSF 대기창고 ZPL 코드 생성
        {
            DataTable inDataTable = new DataTable();
            inDataTable.Columns.Add("LANGID", typeof(string));
            inDataTable.Columns.Add("PROCID", typeof(string));
            inDataTable.Columns.Add("LOTID", typeof(string));
            inDataTable.Columns.Add("WIPSEQ", typeof(string));
            inDataTable.Columns.Add("PRMK", typeof(string));
            inDataTable.Columns.Add("RESO", typeof(string));
            inDataTable.Columns.Add("PRCN", typeof(string));
            inDataTable.Columns.Add("MARH", typeof(string));
            inDataTable.Columns.Add("MARV", typeof(string));
            inDataTable.Columns.Add("DARK", typeof(string));
            inDataTable.Columns.Add("LBCD", typeof(string));

            return inDataTable;
        }

        public DataSet GetBR_PRD_REG_RACK_STOCK() // DSF 대기창고 LOT정보(Rack) 갱신
        {
            DataSet inDataSet = new DataSet();

            DataTable inDataTable = inDataSet.Tables.Add("INDATA");
            inDataTable.Columns.Add("SRCTYPE", typeof(string));
            inDataTable.Columns.Add("USERID", typeof(string));

            DataTable inputLOT = inDataSet.Tables.Add("INLOT");
            inputLOT.Columns.Add("LOTID", typeof(string));
            inputLOT.Columns.Add("RACK_ID", typeof(string));

            return inDataSet;
        }

        public DataSet GetBR_PRD_REG_START_PROD_LOT_DSF() // DSF 공정진척 - PROD LOT 생성 및 착공처리에 사용함.
        {
            DataSet indataSet = new DataSet();
            DataTable inDataTable = indataSet.Tables.Add("IN_EQP");
            inDataTable.Columns.Add("SRCTYPE", typeof(string));
            inDataTable.Columns.Add("IFMODE", typeof(string));
            inDataTable.Columns.Add("EQPTID", typeof(string));
            inDataTable.Columns.Add("USERID", typeof(string));

            DataTable inInputTable = indataSet.Tables.Add("IN_INPUT");
            inInputTable.Columns.Add("CSTID", typeof(string));
            inInputTable.Columns.Add("CELLID", typeof(string));

            return indataSet;
        }

        public DataTable GetBR_PRD_REG_EQPT_END_PROD_LOT_DSF() // DSF 공정진척 - 장비완료
        {
            DataTable inDataTable = new DataTable();
            inDataTable.Columns.Add("SRCTYPE", typeof(string));
            inDataTable.Columns.Add("IFMODE", typeof(string));
            inDataTable.Columns.Add("EQPTID", typeof(string));
            inDataTable.Columns.Add("USERID", typeof(string));

            return inDataTable;
        }

        public DataTable GetDA_PRD_SEL_WIP_DSF() // DSF 공정진척 - 작업대상
        {
            DataTable inDataTable = new DataTable();
            inDataTable.Columns.Add("LANGID", typeof(string));
            inDataTable.Columns.Add("EQPTID", typeof(string));
            inDataTable.Columns.Add("WOID", typeof(string));
            inDataTable.Columns.Add("PROCID", typeof(string));
            inDataTable.Columns.Add("EQSGID", typeof(string));

            return inDataTable;
        }

        public DataTable GetDA_PRD_SEL_OUT_LOT_LIST_DSF() // DSF 공정진척 - 실적확인 - Tray 정보
        {
            DataTable inDataTable = new DataTable();

            inDataTable.Columns.Add("LANGID", typeof(string));
            inDataTable.Columns.Add("AREAID", typeof(string));
            inDataTable.Columns.Add("PR_LOTID", typeof(string));
            inDataTable.Columns.Add("PROCID", typeof(string));
            inDataTable.Columns.Add("EQSGID", typeof(string));
            inDataTable.Columns.Add("EQPTID", typeof(string));
            inDataTable.Columns.Add("TRAYID", typeof(string));

            return inDataTable;
        }

        public DataTable GetDA_PRD_SEL_IN_MTRL_LIST_DSF() // DSF 공정진척 - 실적확인 - 투입자재 정보
        {
            DataTable inDataTable = new DataTable();

            inDataTable.Columns.Add("LANGID", typeof(string));
            inDataTable.Columns.Add("LOTID", typeof(string));
            inDataTable.Columns.Add("WIPSEQ", typeof(int));

            return inDataTable;
        }

        public DataTable GetBR_PRD_REG_END_LOT_DSF() // DSF 공정진척 - 완공보고
        {
            DataTable inDataTable = new DataTable();

            inDataTable.Columns.Add("SRCTYPE", typeof(string));
            inDataTable.Columns.Add("IFMODE", typeof(string));
            inDataTable.Columns.Add("EQPTID", typeof(string));
            inDataTable.Columns.Add("LOTID", typeof(string));
            inDataTable.Columns.Add("INPUTQTY", typeof(int));
            inDataTable.Columns.Add("OUTPUTQTY", typeof(int));
            inDataTable.Columns.Add("RESNQTY", typeof(int));
            inDataTable.Columns.Add("SHIFT", typeof(string));
            inDataTable.Columns.Add("WIPDTTM_ED", typeof(DateTime));
            inDataTable.Columns.Add("WIPNOTE", typeof(string));
            inDataTable.Columns.Add("WRK_USERID", typeof(string));
            inDataTable.Columns.Add("WRK_USER_NAME", typeof(string));
            inDataTable.Columns.Add("USERID", typeof(string));
            inDataTable.Columns.Add("INPUT_DIFF_QTY", typeof(int));

            return inDataTable;
        }


        public DataTable GetDA_PRD_SEL_BC_MTRL() // C생산 작업실적 등록/수정 BC조회
        {
            DataTable inDataTable = new DataTable();

            inDataTable.Columns.Add("PRODID", typeof(string));

            return inDataTable;
        }

        public DataTable GetDA_PRD_SEL_FC_PROD() // C생산 작업실적 등록/수정 FC조회
        {
            DataTable inDataTable = new DataTable();

            inDataTable.Columns.Add("PRODID", typeof(string));

            return inDataTable;
        }
        #endregion








        #region 남경 N4 특이공정(SRC, STP, SSC Bi-Cell)

        #region [SRC]

        public DataTable GetDA_PRD_SEL_SET_WORKORDER_INFO_SRC()
        {
            DataTable inDataTable = new DataTable();
            inDataTable.Columns.Add("EQPTID", typeof(string));

            return inDataTable;
        }
        /// <summary>
        /// 작업지시 선택, 취소
        /// </summary>
        /// <returns></returns>
        public DataTable GetBR_PRD_REG_EIO_WO_DETL_ID_SRC()
        {
            DataTable inDataTable = new DataTable();
            inDataTable.Columns.Add("SRCTYPE", typeof(string));
            inDataTable.Columns.Add("EQPTID", typeof(string));
            inDataTable.Columns.Add("WO_DETL_ID", typeof(string));
            inDataTable.Columns.Add("USERID", typeof(string));
            inDataTable.Columns.Add("WO_DETL_ID2", typeof(string));

            return inDataTable;
        }
        /// <summary>
        /// SRC 대기 매거진 정보
        /// </summary>
        /// <returns></returns>
        public DataTable GetGetDA_PRD_SEL_WAIT_MAG_SRC()
        {
            DataTable inDataTable = new DataTable();
            inDataTable.Columns.Add("LANGID", typeof(string));
            inDataTable.Columns.Add("PROCID", typeof(string));
            inDataTable.Columns.Add("EQSGID", typeof(string));
            inDataTable.Columns.Add("EQPTID", typeof(string));
            inDataTable.Columns.Add("WO_DETL_ID", typeof(string));
            inDataTable.Columns.Add("WOID", typeof(string));
            inDataTable.Columns.Add("LOTID", typeof(string));

            return inDataTable;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public DataTable GetDA_PRD_SEL_EQUIPMENT_WO_SRC()
        {
            DataTable inDataTable = new DataTable();
            inDataTable.Columns.Add("EQPTID", typeof(string));

            return inDataTable;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public DataTable GetBR_PRD_GET_NEW_PROD_LOTID_SRC()
        {
            DataTable inDataTable = new DataTable();

            inDataTable.Columns.Add("SRCTYPE", typeof(string));
            inDataTable.Columns.Add("IFMODE", typeof(string));
            inDataTable.Columns.Add("EQPTID", typeof(string));
            inDataTable.Columns.Add("USERID", typeof(string));
            inDataTable.Columns.Add("CELL_DETL_CLSS_CODE", typeof(string));
            inDataTable.Columns.Add("MOUNT_PSTN_GR_CODE", typeof(string));

            return inDataTable;
        }
        /// <summary>
        /// SRC 작업시작
        /// </summary>
        /// <returns></returns>
        public DataSet GetBR_PRD_REG_START_LOT_SRC()
        {
            DataSet indataSet = new DataSet();

            DataTable inDataTable = indataSet.Tables.Add("IN_EQP");
            inDataTable.Columns.Add("SRCTYPE", typeof(string));
            inDataTable.Columns.Add("IFMODE", typeof(string));
            inDataTable.Columns.Add("EQPTID", typeof(string));
            inDataTable.Columns.Add("USERID", typeof(string));
            inDataTable.Columns.Add("PROD_LOTID", typeof(string));
            inDataTable.Columns.Add("MOUNT_PSTN_GR_CODE", typeof(string));
            inDataTable.Columns.Add("CELL_DETL_CLSS_CODE", typeof(string));

            DataTable input_LOT = indataSet.Tables.Add("IN_INPUT");
            input_LOT.Columns.Add("MOUNT_PSTN_GR_CODE", typeof(string));
            input_LOT.Columns.Add("EQPT_MOUNT_PSTN_ID", typeof(string));
            input_LOT.Columns.Add("EQPT_MOUNT_PSTN_STATE", typeof(string));
            input_LOT.Columns.Add("INPUT_LOTID", typeof(string));
            input_LOT.Columns.Add("CSTID", typeof(string));
            input_LOT.Columns.Add("OUT_CSTID", typeof(string));
            //input_LOT.Columns.Add("ACTQTY", typeof(double));

            return indataSet;
        }
        /// <summary>
        /// 작업대상 LOT 조회
        /// </summary>
        /// <returns></returns>
        public DataTable GetDA_PRD_SEL_WIP_SRC()
        {
            DataTable inDataTable = new DataTable();
            inDataTable.Columns.Add("LANGID", typeof(string));
            inDataTable.Columns.Add("EQPTID", typeof(string));
            inDataTable.Columns.Add("WOID", typeof(string));
            inDataTable.Columns.Add("PROCID", typeof(string));
            inDataTable.Columns.Add("EQSGID", typeof(string));

            return inDataTable;
        }

        /// <summary>
        /// SRC 대기매거진
        /// </summary>
        /// <returns></returns>
        public DataTable GetDA_PRD_SEL_WAIT_MAG_SRC()
        {
            DataTable inDataTable = new DataTable();
            inDataTable.Columns.Add("LANGID", typeof(string));
            inDataTable.Columns.Add("PROCID", typeof(string));
            inDataTable.Columns.Add("EQSGID", typeof(string));
            inDataTable.Columns.Add("EQPTID", typeof(string));
            inDataTable.Columns.Add("WO_DETL_ID", typeof(string));
            inDataTable.Columns.Add("LOTID", typeof(string));
            inDataTable.Columns.Add("WOID", typeof(string));

            return inDataTable;
        }

        /// <summary>
        /// SRC 대기매거진 투입
        /// </summary>
        /// <returns></returns>
        public DataSet GetBR_PRD_REG_START_INPUT_LOT_SRC()
        {
            DataSet indataSet = new DataSet();
            DataTable inDataTable = indataSet.Tables.Add("IN_EQP");
            inDataTable.Columns.Add("SRCTYPE", typeof(string));
            inDataTable.Columns.Add("IFMODE", typeof(string));
            inDataTable.Columns.Add("EQPTID", typeof(string));
            inDataTable.Columns.Add("USERID", typeof(string));
            inDataTable.Columns.Add("PROD_LOTID", typeof(string));

            DataTable inInputTable = indataSet.Tables.Add("IN_INPUT");
            inInputTable.Columns.Add("EQPT_MOUNT_PSTN_ID", typeof(string));
            inInputTable.Columns.Add("EQPT_MOUNT_PSTN_STATE", typeof(string));
            inInputTable.Columns.Add("INPUT_LOTID", typeof(string));
            inInputTable.Columns.Add("CSTID", typeof(string));
            inInputTable.Columns.Add("OUT_CSTID", typeof(string));
            //inInputTable.Columns.Add("ACTQTY", typeof(double));

            return indataSet;
        }

        /// <summary>
        /// SRC 투입자재(매거진) 조회
        /// </summary>
        /// <returns></returns>
        public DataTable GetDA_PRD_SEL_CURR_IN_LOT_LIST_SRC()
        {
            DataTable inDataTable = new DataTable();
            inDataTable.Columns.Add("LANGID", typeof(string));
            inDataTable.Columns.Add("EQPTID", typeof(string));
            inDataTable.Columns.Add("MOUNT_PSTN_GR_CODE", typeof(string));

            return inDataTable;
        }

        /// <summary>
        /// SRC 작업시작 투입 정보 조회
        /// </summary>
        /// <returns></returns>
        public DataTable GetDA_PRD_SEL_EQPT_MOUNT_INFO_SRC()
        {
            DataTable inDataTable = new DataTable();
            inDataTable.Columns.Add("LANGID", typeof(string));
            inDataTable.Columns.Add("EQSGID", typeof(string));
            inDataTable.Columns.Add("EQPTID", typeof(string));

            return inDataTable;
        }

        /// <summary>
        /// SRC 작업시작 - 선택 W/O 제품정보 조회
        /// </summary>
        /// <returns></returns>
        public DataTable GetDDA_PRD_SEL_EQUIPMENT_WO_SRC()
        {
            DataTable inDataTable = new DataTable();
            inDataTable.Columns.Add("EQPTID", typeof(string));
            inDataTable.Columns.Add("WO_DETL_ID", typeof(string));

            return inDataTable;
        }

        /// <summary>
        /// SRC 생산 매거진 조회
        /// </summary>
        /// <returns></returns>
        public DataTable GetDA_PRD_SEL_OUT_LOT_LIST_SRC()
        {
            DataTable inDataTable = new DataTable();
            inDataTable.Columns.Add("LANGID", typeof(string));
            inDataTable.Columns.Add("PR_LOTID", typeof(string));

            return inDataTable;
        }

        /// <summary>
        /// 자재투입 투입취소
        /// </summary>
        public DataSet GetBR_PRD_REG_CANCEL_INPUT_LOT_SRC()
        {
            DataSet indataSet = new DataSet();
            DataTable inDataTable = indataSet.Tables.Add("INDATA");
            inDataTable.Columns.Add("SRCTYPE", typeof(string));
            inDataTable.Columns.Add("IFMODE", typeof(string));
            inDataTable.Columns.Add("EQPTID", typeof(string));
            inDataTable.Columns.Add("USERID", typeof(string));
            inDataTable.Columns.Add("PROD_LOTID", typeof(string));

            DataTable inInputTable = indataSet.Tables.Add("IN_INPUT");
            inInputTable.Columns.Add("EQPT_MOUNT_PSTN_ID", typeof(string));
            inInputTable.Columns.Add("INPUT_LOTID", typeof(string));
            inInputTable.Columns.Add("WIPNOTE", typeof(string));

            return indataSet;
        }

        /// <summary>
        /// 생산 매거진 생성
        /// </summary>
        /// <returns></returns>
        public DataTable GetBR_PRD_GET_NEW_OUT_LOTID_SRC()
        {
            DataTable inDataTable = new DataTable();
            inDataTable.Columns.Add("SRCTYPE", typeof(string));
            inDataTable.Columns.Add("EQPTID", typeof(string));
            inDataTable.Columns.Add("PROD_LOTID", typeof(string));
            inDataTable.Columns.Add("IFMODE", typeof(string));
            inDataTable.Columns.Add("USERID", typeof(string));

            return inDataTable;
        }

        /// <summary>
        /// 생산 매거진 삭제
        /// </summary>
        /// <returns></returns>
        public DataSet GetBR_PRD_REG_DELETE_OUT_LOT_SRC()
        {
            DataSet indataSet = new DataSet();

            DataTable inDataTable = indataSet.Tables.Add("INDATA");
            inDataTable.Columns.Add("SRCTYPE", typeof(string));
            inDataTable.Columns.Add("IFMODE", typeof(string));
            inDataTable.Columns.Add("EQPTID", typeof(string));
            inDataTable.Columns.Add("PROD_LOTID", typeof(string));
            inDataTable.Columns.Add("USERID", typeof(string));

            DataTable input_LOT = indataSet.Tables.Add("IN_INPUT");
            input_LOT.Columns.Add("OUT_LOTID", typeof(string));

            return indataSet;
        }

        /// <summary>
        /// 매거진신규구성 저장
        /// </summary>
        /// <returns></returns>
        public DataTable GetBR_PRD_REG_MAGAZINE_SRC()
        {
            DataTable inDataTable = new DataTable();
            inDataTable.Columns.Add("SRCTYPE", typeof(string));
            inDataTable.Columns.Add("IFMODE", typeof(string));
            inDataTable.Columns.Add("EQPTID", typeof(string));
            inDataTable.Columns.Add("PRODID", typeof(string));
            inDataTable.Columns.Add("CSTID", typeof(string));
            inDataTable.Columns.Add("WIPQTY", typeof(int));
            inDataTable.Columns.Add("USERID", typeof(string));
            inDataTable.Columns.Add("MGZN_RECONF_TYPE_CODE", typeof(string));
            inDataTable.Columns.Add("WO_DETL_ID", typeof(string));

            return inDataTable;
        }

        /// <summary>
        /// 자재투입 투입완료
        /// </summary>
        /// <returns></returns>
        public DataSet GetBR_PRD_REG_END_INPUT_LOT_SRC()
        {
            DataSet indataSet = new DataSet();
            DataTable inDataTable = indataSet.Tables.Add("IN_EQP");
            inDataTable.Columns.Add("SRCTYPE", typeof(string));
            inDataTable.Columns.Add("IFMODE", typeof(string));
            inDataTable.Columns.Add("EQPTID", typeof(string));
            inDataTable.Columns.Add("USERID", typeof(string));
            inDataTable.Columns.Add("PROD_LOTID", typeof(string));

            DataTable inInputTable = indataSet.Tables.Add("IN_INPUT");
            inInputTable.Columns.Add("EQPT_MOUNT_PSTN_ID", typeof(string));
            inInputTable.Columns.Add("EQPT_MOUNT_PSTN_STATE", typeof(string));
            inInputTable.Columns.Add("INPUT_LOTID", typeof(string));
            inInputTable.Columns.Add("OUTPUT_QTY", typeof(int));

            return indataSet;
        }

        /// <summary>
        /// 실적 확정
        /// </summary>
        /// <returns></returns>
        public DataSet GetBR_PRD_REG_END_LOT_SRC()
        {
            DataSet indataSet = new DataSet();

            DataTable inDataTable = indataSet.Tables.Add("INDATA");
            inDataTable.Columns.Add("SRCTYPE", typeof(string));
            inDataTable.Columns.Add("IFMODE", typeof(string));
            inDataTable.Columns.Add("EQPTID", typeof(string));
            inDataTable.Columns.Add("PROD_LOTID", typeof(string));
            inDataTable.Columns.Add("INPUTQTY", typeof(int));
            inDataTable.Columns.Add("OUTPUTQTY", typeof(int));
            inDataTable.Columns.Add("RESNQTY", typeof(int));
            inDataTable.Columns.Add("SHIFT", typeof(string));
            inDataTable.Columns.Add("WIPNOTE", typeof(string));
            inDataTable.Columns.Add("WRK_USERID", typeof(string));
            inDataTable.Columns.Add("WRK_USER_NAME", typeof(string));
            inDataTable.Columns.Add("USERID", typeof(string));
            inDataTable.Columns.Add("WIPDTTM_ED", typeof(DateTime));

            DataTable inInputLot = indataSet.Tables.Add("IN_INPUT");
            inInputLot.Columns.Add("EQPT_MOUNT_PSTN_ID", typeof(string));
            inInputLot.Columns.Add("EQPT_MOUNT_PSTN_STATE", typeof(string));
            inInputLot.Columns.Add("INPUT_LOTID", typeof(string));

            return indataSet;
        }

        /// <summary>
        /// SRC 잔량처리
        /// </summary>
        /// <returns></returns>
        public DataSet GetBR_PRD_REG_REMAIN_LOT_SRC()
        {
            DataSet indataSet = new DataSet();

            DataTable inDataTable = indataSet.Tables.Add("INDATA");
            inDataTable.Columns.Add("SRCTYPE", typeof(string));
            inDataTable.Columns.Add("IFMODE", typeof(string));
            inDataTable.Columns.Add("EQPTID", typeof(string));
            inDataTable.Columns.Add("USERID", typeof(string));
            inDataTable.Columns.Add("PROD_LOTID", typeof(string));
            inDataTable.Columns.Add("INPUT_LOT_TYPE", typeof(string));
            inDataTable.Columns.Add("LANGID", typeof(string));

            DataTable inInputLot = indataSet.Tables.Add("IN_INPUT");
            inInputLot.Columns.Add("EQPT_MOUNT_PSTN_ID", typeof(string));
            inInputLot.Columns.Add("EQPT_MOUNT_PSTN_STATE", typeof(string));
            inInputLot.Columns.Add("INPUT_LOTID", typeof(string));
            inInputLot.Columns.Add("WIPNOTE", typeof(string));
            inInputLot.Columns.Add("ACTQTY", typeof(double));
            inInputLot.Columns.Add("OUTPUT_QTY", typeof(double));
            inInputLot.Columns.Add("REMAIN_QTY", typeof(double));
            inInputLot.Columns.Add("CSTID", typeof(string));

            return indataSet;
        }

        #endregion

        #region [STP]
        /// <summary>
        /// 투입 정보 조회 - STP
        /// </summary>
        /// <returns></returns>
        public DataTable GetDA_PRD_SEL_EQPT_MOUNT_INFO_STP()
        {
            DataTable inDataTable = new DataTable();
            inDataTable.Columns.Add("LANGID", typeof(string));
            inDataTable.Columns.Add("EQPTID", typeof(string));
            inDataTable.Columns.Add("MOUNT_MTRL_TYPE_CODE", typeof(string));

            return inDataTable;
        }

        /// <summary>
        /// 투입 매거진 - 투입완료
        /// </summary>
        /// <returns></returns>
        public DataSet GetBR_PRD_REG_END_INPUT_IN_LOT_STP()
        {
            DataSet indataSet = new DataSet();
            DataTable inDataTable = indataSet.Tables.Add("INDATA");
            inDataTable.Columns.Add("SRCTYPE", typeof(string));
            inDataTable.Columns.Add("IFMODE", typeof(string));
            inDataTable.Columns.Add("EQPTID", typeof(string));
            inDataTable.Columns.Add("USERID", typeof(string));
            inDataTable.Columns.Add("PROD_LOTID", typeof(string));

            DataTable inInputTable = indataSet.Tables.Add("IN_INPUT");
            inInputTable.Columns.Add("EQPT_MOUNT_PSTN_ID", typeof(string));
            inInputTable.Columns.Add("EQPT_MOUNT_PSTN_STATE", typeof(string));
            inInputTable.Columns.Add("INPUT_LOTID", typeof(string));
            //inInputTable.Columns.Add("ACTQTY", typeof(int));

            return indataSet;
        }

        /// <summary>
        /// STP 대기 매거진 조회
        /// </summary>
        /// <returns></returns>
        public DataTable GetDA_PRD_SEL_WAIT_MAG_STP()
        {
            DataTable inDataTable = new DataTable();
            inDataTable.Columns.Add("LANGID", typeof(string));
            inDataTable.Columns.Add("PROCID", typeof(string));
            inDataTable.Columns.Add("EQSGID", typeof(string));
            inDataTable.Columns.Add("EQPTID", typeof(string));
            inDataTable.Columns.Add("PRODID", typeof(string));
            inDataTable.Columns.Add("LOTID", typeof(string));
            inDataTable.Columns.Add("BICELL_LEVEL3_CODE", typeof(string));
            inDataTable.Columns.Add("CSTID", typeof(string));
            inDataTable.Columns.Add("PRDT_SIZE", typeof(string));
            inDataTable.Columns.Add("PRDT_DIRCTN", typeof(string));

            return inDataTable;
        }

        public DataSet GetBR_PRD_REG_START_INPUT_LOT_STP()
        {
            DataSet indataSet = new DataSet();
            DataTable inDataTable = indataSet.Tables.Add("IN_EQP");
            inDataTable.Columns.Add("SRCTYPE", typeof(string));
            inDataTable.Columns.Add("IFMODE", typeof(string));
            inDataTable.Columns.Add("EQPTID", typeof(string));
            inDataTable.Columns.Add("PROD_LOTID", typeof(string));
            inDataTable.Columns.Add("USERID", typeof(string));

            DataTable inInputTable = indataSet.Tables.Add("IN_INPUT");
            inInputTable.Columns.Add("EQPT_MOUNT_PSTN_ID", typeof(string));
            inInputTable.Columns.Add("EQPT_MOUNT_PSTN_STATE", typeof(string));
            inInputTable.Columns.Add("INPUT_LOTID", typeof(string));

            return indataSet;
        }

        public DataTable GetDA_PRD_SEL_WIP_STP()
        {
            DataTable inDataTable = new DataTable();
            inDataTable.Columns.Add("LANGID", typeof(string));
            inDataTable.Columns.Add("WOID", typeof(string));
            inDataTable.Columns.Add("EQPTID", typeof(string));
            inDataTable.Columns.Add("PROCID", typeof(string));
            inDataTable.Columns.Add("EQSGID", typeof(string));

            return inDataTable;
        }

        public DataTable GetBR_PRD_REG_CREATE_OUT_LOT_STP()
        {
            DataTable inDataTable = new DataTable();
            inDataTable.Columns.Add("SRCTYPE", typeof(string));
            inDataTable.Columns.Add("LANGID", typeof(string));
            inDataTable.Columns.Add("IFMODE", typeof(string));
            inDataTable.Columns.Add("EQPTID", typeof(string));
            inDataTable.Columns.Add("WO_DETL_ID", typeof(string));
            inDataTable.Columns.Add("PROD_LOTID", typeof(string));
            inDataTable.Columns.Add("OUT_LOTID", typeof(string));
            inDataTable.Columns.Add("INPUTQTY", typeof(int));
            inDataTable.Columns.Add("OUTPUTQTY", typeof(int));
            inDataTable.Columns.Add("RESNQTY", typeof(int));
            inDataTable.Columns.Add("SHIFT", typeof(string));
            inDataTable.Columns.Add("WIPNOTE", typeof(string));
            inDataTable.Columns.Add("WRK_USER_NAME", typeof(string));
            inDataTable.Columns.Add("USERID", typeof(string));
            inDataTable.Columns.Add("CSTID", typeof(string));

            return inDataTable;
        }
        public DataSet GetBR_PRD_REG_DELETE_OUT_LOT_STP()
        {
            DataSet indataSet = new DataSet();

            DataTable inDataTable = indataSet.Tables.Add("INDATA");
            inDataTable.Columns.Add("SRCTYPE", typeof(string));
            inDataTable.Columns.Add("IFMODE", typeof(string));
            inDataTable.Columns.Add("EQPTID", typeof(string));
            inDataTable.Columns.Add("PROD_LOTID", typeof(string));
            inDataTable.Columns.Add("USERID", typeof(string));

            DataTable input_LOT = indataSet.Tables.Add("IN_INPUT");
            input_LOT.Columns.Add("OUT_LOTID", typeof(string));

            return indataSet;
        }
        public DataSet GetBR_PRD_GET_NEW_PROD_LOTID_STP()
        {
            DataSet indataSet = new DataSet();

            DataTable inDataTable = indataSet.Tables.Add("IN_EQP");
            inDataTable.Columns.Add("SRCTYPE", typeof(string));
            //inDataTable.Columns.Add("LANGID", typeof(string));
            inDataTable.Columns.Add("IFMODE", typeof(string));
            inDataTable.Columns.Add("EQPTID", typeof(string));
            //inDataTable.Columns.Add("NEXTDAY", typeof(string));
            inDataTable.Columns.Add("USERID", typeof(string));

            DataTable input_LOT = indataSet.Tables.Add("IN_INPUT");
            input_LOT.Columns.Add("EQPT_MOUNT_PSTN_ID", typeof(string));
            input_LOT.Columns.Add("EQPT_MOUNT_PSTN_STATE", typeof(int));
            input_LOT.Columns.Add("INPUT_LOTID", typeof(string));

            return indataSet;
        }
        public DataSet GetBR_PRD_REG_START_PROD_LOT_STP()
        {
            DataSet indataSet = new DataSet();

            DataTable inDataTable = indataSet.Tables.Add("IN_EQP");
            inDataTable.Columns.Add("SRCTYPE", typeof(string));
            inDataTable.Columns.Add("IFMODE", typeof(string));
            inDataTable.Columns.Add("EQPTID", typeof(string));
            inDataTable.Columns.Add("USERID", typeof(string));
            inDataTable.Columns.Add("PROD_LOTID", typeof(string));
            inDataTable.Columns.Add("LANGID", typeof(string));

            DataTable input_LOT = indataSet.Tables.Add("IN_INPUT");
            input_LOT.Columns.Add("EQPT_MOUNT_PSTN_ID", typeof(string));
            input_LOT.Columns.Add("EQPT_MOUNT_PSTN_STATE", typeof(string));
            input_LOT.Columns.Add("INPUT_LOTID", typeof(string));

            return indataSet;
        }
        public DataSet GetBR_PRD_REG_END_LOT_STP()
        {
            DataSet indataSet = new DataSet();

            DataTable inDataTable = indataSet.Tables.Add("INDATA");
            inDataTable.Columns.Add("SRCTYPE", typeof(string));
            inDataTable.Columns.Add("IFMODE", typeof(string));
            inDataTable.Columns.Add("EQPTID", typeof(string));
            inDataTable.Columns.Add("PROD_LOTID", typeof(string));
            inDataTable.Columns.Add("INPUTQTY", typeof(int));
            inDataTable.Columns.Add("OUTPUTQTY", typeof(int));
            inDataTable.Columns.Add("RESNQTY", typeof(int));
            inDataTable.Columns.Add("SHIFT", typeof(string));
            inDataTable.Columns.Add("WIPDTTM_ED", typeof(DateTime));
            inDataTable.Columns.Add("WIPNOTE", typeof(string));
            inDataTable.Columns.Add("WRK_USERID", typeof(string));
            inDataTable.Columns.Add("WRK_USER_NAME", typeof(string));
            inDataTable.Columns.Add("USERID", typeof(string));

            DataTable input_LOT = indataSet.Tables.Add("IN_INPUT");
            input_LOT.Columns.Add("EQPT_MOUNT_PSTN_ID", typeof(string));
            input_LOT.Columns.Add("EQPT_MOUNT_PSTN_STATE", typeof(string));
            input_LOT.Columns.Add("INPUT_LOTID", typeof(string));

            return indataSet;
        }
        public DataTable GetBR_PRD_REG_MAGAZINE_STP()
        {
            DataTable inDataTable = new DataTable();
            inDataTable.Columns.Add("SRCTYPE", typeof(string));
            inDataTable.Columns.Add("IFMODE", typeof(string));
            inDataTable.Columns.Add("EQPTID", typeof(string));
            inDataTable.Columns.Add("PRODID", typeof(string));
            inDataTable.Columns.Add("CSTID", typeof(string));
            inDataTable.Columns.Add("WIPQTY", typeof(int));
            inDataTable.Columns.Add("USERID", typeof(string));
            inDataTable.Columns.Add("MGZN_RECONF_TYPE_CODE", typeof(string));

            return inDataTable;
        }

        #endregion

        #region [SSC Bi-Cell]
        /// <summary>
        /// 투입 정보 조회 - SSC Bi-Cell
        /// </summary>
        /// <returns></returns>
        public DataTable GetDA_PRD_SEL_EQPT_MOUNT_INFO_SSCBI()
        {
            DataTable inDataTable = new DataTable();
            inDataTable.Columns.Add("LANGID", typeof(string));
            inDataTable.Columns.Add("EQPTID", typeof(string));
            inDataTable.Columns.Add("MOUNT_MTRL_TYPE_CODE", typeof(string));

            return inDataTable;
        }
        /// <summary>
        /// 작업시작 투입 Lot 발번
        /// </summary>
        /// <returns></returns>
        public DataTable GetBR_PRD_GET_NEW_PROD_LOTID_SSCBI()
        {
            DataTable inDataTable = new DataTable();
            inDataTable.Columns.Add("SRCTYPE", typeof(string));
            inDataTable.Columns.Add("IFMODE", typeof(string));
            inDataTable.Columns.Add("EQPTID", typeof(string));
            inDataTable.Columns.Add("USERID", typeof(string));

            return inDataTable;
        }

        /// <summary>
        /// 작업시작 - SSC Bi-Cell
        /// </summary>
        /// <returns></returns>
        public DataSet GetBR_PRD_REG_START_LOT_SSCBI()
        {
            DataSet indataSet = new DataSet();

            DataTable inDataTable = indataSet.Tables.Add("IN_EQP");
            inDataTable.Columns.Add("SRCTYPE", typeof(string));
            inDataTable.Columns.Add("IFMODE", typeof(string));
            inDataTable.Columns.Add("EQPTID", typeof(string));
            inDataTable.Columns.Add("USERID", typeof(string));
            inDataTable.Columns.Add("PROD_LOTID", typeof(string));

            DataTable input_LOT = indataSet.Tables.Add("IN_INPUT");
            input_LOT.Columns.Add("EQPT_MOUNT_PSTN_ID", typeof(string));
            input_LOT.Columns.Add("EQPT_MOUNT_PSTN_STATE", typeof(string));
            input_LOT.Columns.Add("INPUT_LOTID", typeof(string));
            //input_LOT.Columns.Add("ACTQTY", typeof(double));

            return indataSet;
        }
        /// <summary>
        /// SSC Bi-Cell 대기 매거진 조회
        /// </summary>
        /// <returns></returns>
        public DataTable GetDA_PRD_SEL_WAIT_MAG_SSCBI()
        {
            DataTable inDataTable = new DataTable();
            inDataTable.Columns.Add("LANGID", typeof(string));
            inDataTable.Columns.Add("PROCID", typeof(string));
            inDataTable.Columns.Add("EQSGID", typeof(string));
            inDataTable.Columns.Add("EQPTID", typeof(string));
            inDataTable.Columns.Add("WO_DETL_ID", typeof(string));
            inDataTable.Columns.Add("LOTID", typeof(string));

            return inDataTable;
        }
        /// <summary>
        /// 작업대상 LOT 조회
        /// </summary>
        /// <returns></returns>
        public DataTable GetDA_PRD_SEL_WIP_SSCBI()
        {
            DataTable inDataTable = new DataTable();
            inDataTable.Columns.Add("LANGID", typeof(string));
            inDataTable.Columns.Add("EQPTID", typeof(string));
            inDataTable.Columns.Add("WOID", typeof(string));
            inDataTable.Columns.Add("PROCID", typeof(string));
            inDataTable.Columns.Add("EQSGID", typeof(string));

            return inDataTable;
        }
        /// <summary>
        /// SSC Bi-Cell 대기매거진 투입
        /// </summary>
        /// <returns></returns>
        public DataSet GetBR_PRD_REG_START_INPUT_LOT_SSCBI()
        {
            DataSet indataSet = new DataSet();
            DataTable inDataTable = indataSet.Tables.Add("IN_EQP");
            inDataTable.Columns.Add("SRCTYPE", typeof(string));
            inDataTable.Columns.Add("IFMODE", typeof(string));
            inDataTable.Columns.Add("EQPTID", typeof(string));
            inDataTable.Columns.Add("USERID", typeof(string));
            inDataTable.Columns.Add("PROD_LOTID", typeof(string));

            DataTable inInputTable = indataSet.Tables.Add("IN_INPUT");
            inInputTable.Columns.Add("EQPT_MOUNT_PSTN_ID", typeof(string));
            inInputTable.Columns.Add("EQPT_MOUNT_PSTN_STATE", typeof(string));
            inInputTable.Columns.Add("INPUT_LOTID", typeof(string));
            //inInputTable.Columns.Add("ACTQTY", typeof(double));

            return indataSet;
        }
        public DataTable GetBR_PRD_REG_MAGAZINE_SSCBI()
        {
            DataTable inDataTable = new DataTable();
            inDataTable.Columns.Add("SRCTYPE", typeof(string));
            inDataTable.Columns.Add("IFMODE", typeof(string));
            inDataTable.Columns.Add("EQPTID", typeof(string));
            inDataTable.Columns.Add("PRODID", typeof(string));
            inDataTable.Columns.Add("CSTID", typeof(string));
            inDataTable.Columns.Add("WIPQTY", typeof(int));
            inDataTable.Columns.Add("USERID", typeof(string));
            inDataTable.Columns.Add("MGZN_RECONF_TYPE_CODE", typeof(string));

            return inDataTable;
        }
        /// <summary>
        /// 자재투입 투입취소
        /// </summary>
        public DataSet GetBR_PRD_REG_CANCEL_INPUT_LOT_SSCBI()
        {
            DataSet indataSet = new DataSet();
            DataTable inDataTable = indataSet.Tables.Add("INDATA");
            inDataTable.Columns.Add("SRCTYPE", typeof(string));
            inDataTable.Columns.Add("IFMODE", typeof(string));
            inDataTable.Columns.Add("EQPTID", typeof(string));
            inDataTable.Columns.Add("USERID", typeof(string));
            inDataTable.Columns.Add("PROD_LOTID", typeof(string));

            DataTable inInputTable = indataSet.Tables.Add("IN_INPUT");
            inInputTable.Columns.Add("EQPT_MOUNT_PSTN_ID", typeof(string));
            inInputTable.Columns.Add("INPUT_LOTID", typeof(string));
            inInputTable.Columns.Add("WIPNOTE", typeof(string));

            return indataSet;
        }
        /// <summary>
        /// 자재투입 투입완료
        /// </summary>
        /// <returns></returns>
        public DataSet GetBR_PRD_REG_END_INPUT_LOT_SSCBI()
        {
            DataSet indataSet = new DataSet();
            DataTable inDataTable = indataSet.Tables.Add("IN_EQP");
            inDataTable.Columns.Add("SRCTYPE", typeof(string));
            inDataTable.Columns.Add("IFMODE", typeof(string));
            inDataTable.Columns.Add("EQPTID", typeof(string));
            inDataTable.Columns.Add("USERID", typeof(string));
            inDataTable.Columns.Add("PROD_LOTID", typeof(string));
            //inDataTable.Columns.Add("LANGID", typeof(string));

            DataTable inInputTable = indataSet.Tables.Add("IN_INPUT");
            inInputTable.Columns.Add("EQPT_MOUNT_PSTN_ID", typeof(string));
            inInputTable.Columns.Add("EQPT_MOUNT_PSTN_STATE", typeof(string));
            inInputTable.Columns.Add("INPUT_LOTID", typeof(string));
            inInputTable.Columns.Add("OUTPUT_QTY", typeof(int));

            return indataSet;
        }
        /// <summary>
        /// SSC Bi-Cell 잔량처리
        /// </summary>
        /// <returns></returns>
        public DataSet GetBR_PRD_REG_REMAIN_LOT_SSCBI()
        {
            DataSet indataSet = new DataSet();

            DataTable inDataTable = indataSet.Tables.Add("INDATA");
            inDataTable.Columns.Add("SRCTYPE", typeof(string));
            inDataTable.Columns.Add("IFMODE", typeof(string));
            inDataTable.Columns.Add("EQPTID", typeof(string));
            inDataTable.Columns.Add("USERID", typeof(string));
            inDataTable.Columns.Add("PROD_LOTID", typeof(string));
            inDataTable.Columns.Add("INPUT_LOT_TYPE", typeof(string));
            inDataTable.Columns.Add("LANGID", typeof(string));

            DataTable inInputLot = indataSet.Tables.Add("IN_INPUT");
            inInputLot.Columns.Add("EQPT_MOUNT_PSTN_ID", typeof(string));
            inInputLot.Columns.Add("EQPT_MOUNT_PSTN_STATE", typeof(string));
            inInputLot.Columns.Add("INPUT_LOTID", typeof(string));
            inInputLot.Columns.Add("WIPNOTE", typeof(string));
            inInputLot.Columns.Add("ACTQTY", typeof(double));
            inInputLot.Columns.Add("OUTPUT_QTY", typeof(double));
            inInputLot.Columns.Add("REMAIN_QTY", typeof(double));

            return indataSet;
        }
        /// <summary>
        /// SSC Bi-Cell 생산 매거진 조회
        /// </summary>
        /// <returns></returns>
        public DataTable GetDA_PRD_SEL_OUT_LOT_LIST_SSCBI()
        {
            DataTable inDataTable = new DataTable();
            inDataTable.Columns.Add("LANGID", typeof(string));
            inDataTable.Columns.Add("PR_LOTID", typeof(string));

            return inDataTable;
        }
        /// <summary>
        /// 실적 확정
        /// </summary>
        /// <returns></returns>
        public DataSet GetBR_PRD_REG_END_LOT_SSCBI()
        {
            DataSet indataSet = new DataSet();

            DataTable inDataTable = indataSet.Tables.Add("INDATA");
            inDataTable.Columns.Add("SRCTYPE", typeof(string));
            inDataTable.Columns.Add("IFMODE", typeof(string));
            inDataTable.Columns.Add("EQPTID", typeof(string));
            inDataTable.Columns.Add("PROD_LOTID", typeof(string));
            inDataTable.Columns.Add("INPUTQTY", typeof(int));
            inDataTable.Columns.Add("OUTPUTQTY", typeof(int));
            inDataTable.Columns.Add("RESNQTY", typeof(int));
            inDataTable.Columns.Add("SHIFT", typeof(string));
            inDataTable.Columns.Add("WIPNOTE", typeof(string));
            inDataTable.Columns.Add("WRK_USERID", typeof(string));
            inDataTable.Columns.Add("WRK_USER_NAME", typeof(string));
            inDataTable.Columns.Add("USERID", typeof(string));
            inDataTable.Columns.Add("WIPDTTM_ED", typeof(DateTime));

            DataTable inInputLot = indataSet.Tables.Add("IN_INPUT");
            inInputLot.Columns.Add("EQPT_MOUNT_PSTN_ID", typeof(string));
            inInputLot.Columns.Add("EQPT_MOUNT_PSTN_STATE", typeof(string));
            inInputLot.Columns.Add("INPUT_LOTID", typeof(string));

            return indataSet;
        }
        /// <summary>
        /// SRC 투입자재(매거진) 조회
        /// </summary>
        /// <returns></returns>
        public DataTable GetDA_PRD_SEL_CURR_IN_LOT_LIST_SSCBI()
        {
            DataTable inDataTable = new DataTable();
            inDataTable.Columns.Add("LANGID", typeof(string));
            inDataTable.Columns.Add("EQPTID", typeof(string));
            inDataTable.Columns.Add("PROD_LOTID", typeof(string));

            return inDataTable;
        }

        #endregion

        #region [ZZS]
        public DataTable GetDA_PRD_SEL_READY_LOT_ZZS()
        {
            DataTable inDataTable = new DataTable();
            inDataTable.Columns.Add("LANGID", typeof(string));
            inDataTable.Columns.Add("PROCID", typeof(string));
            inDataTable.Columns.Add("EQPTID", typeof(string));
            inDataTable.Columns.Add("IN_LOTID", typeof(string));
            inDataTable.Columns.Add("EQPT_MOUNT_PSTN_ID", typeof(string));

            return inDataTable;
        }
        #endregion


        #endregion

        #region 원각조립

        #region [공통]

        public DataTable GetDA_BAS_SEL_TB_SFC_EQPT_CURR_MOUNT_MTRL_CBO_ASSY()
        {
            DataTable inDataTable = new DataTable();
            inDataTable.Columns.Add("LANGID", typeof(string));
            inDataTable.Columns.Add("EQPTID", typeof(string));
            inDataTable.Columns.Add("MOUNT_MTRL_TYPE_CODE", typeof(string));

            return inDataTable;
        }

        public DataTable GetDA_PRD_SEL_ITEM_LIST()
        {
            DataTable inDataTable = new DataTable();
            inDataTable.Columns.Add("WOID", typeof(string));
            inDataTable.Columns.Add("ITEMCODE", typeof(string));

            return inDataTable;
        }

        /// <summary>
        /// 작업대상 조회
        /// </summary>
        /// <returns></returns>
        public DataTable GetDA_PRD_SEL_PRODUCTLOT_ASSY()
        {
            DataTable inDataTable = new DataTable();
            inDataTable.Columns.Add("LANGID", typeof(string));
            inDataTable.Columns.Add("EQSGID", typeof(string));
            inDataTable.Columns.Add("EQPTID", typeof(string));
            inDataTable.Columns.Add("WIPSTAT", typeof(string));
            inDataTable.Columns.Add("PROCID", typeof(string));
            inDataTable.Columns.Add("WIPTYPECODE", typeof(string));

            return inDataTable;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public DataTable GetDA_PRD_SEL_ASSY_DEFECT_ALL()
        {
            DataTable inDataTable = new DataTable();
            inDataTable.Columns.Add("LANGID", typeof(string));
            inDataTable.Columns.Add("AREAID", typeof(string));
            inDataTable.Columns.Add("PROCID", typeof(string));
            inDataTable.Columns.Add("RESNTYPE", typeof(string));
            inDataTable.Columns.Add("LOTID", typeof(string));

            return inDataTable;
        }

        /// <summary>
        /// 불량, Loss, 물청 저장
        /// </summary>
        /// <returns></returns>
        public DataSet GetBR_QCA_REG_WIPREASONCOLLECT_ALL()
        {
            // 원각형 자재 투입
            DataSet indataSet = new DataSet();

            DataTable inDataTable = indataSet.Tables.Add("INDATA");
            inDataTable.Columns.Add("SRCTYPE", typeof(string));
            inDataTable.Columns.Add("IFMODE", typeof(string));
            inDataTable.Columns.Add("EQPTID", typeof(string));
            inDataTable.Columns.Add("USERID", typeof(string));

            DataTable inDefectLot = indataSet.Tables.Add("INRESN");
            inDefectLot.Columns.Add("LOTID", typeof(string));
            inDefectLot.Columns.Add("ACTID", typeof(string));
            inDefectLot.Columns.Add("RESNCODE", typeof(string));
            inDefectLot.Columns.Add("RESNQTY", typeof(string));
            inDefectLot.Columns.Add("COST_CNTR_ID", typeof(string));

            return indataSet;
        }

        /// <summary>
        /// 투입 자재 조회
        /// </summary>
        /// <returns></returns>
        public DataTable GetDA_PRD_SEL_ASSY_INPUT_MTRL()
        {
            DataTable inDataTable = new DataTable();
            inDataTable.Columns.Add("LANGID", typeof(string));
            inDataTable.Columns.Add("LOTID", typeof(string));
            inDataTable.Columns.Add("MTRLTYPE", typeof(string));

            return inDataTable;
        }

        /// <summary>
        /// 반제품 조회
        /// </summary>
        /// <returns></returns>
        public DataTable GetBR_PRD_REG_END_LOT_HS_UI()
        {
            DataTable inDataTable = new DataTable();
            inDataTable.Columns.Add("SRCTYPE", typeof(string));
            inDataTable.Columns.Add("IFMODE", typeof(string));
            inDataTable.Columns.Add("EQPTID", typeof(string));
            inDataTable.Columns.Add("LOTID", typeof(string));
            inDataTable.Columns.Add("USERID", typeof(string));

            return inDataTable;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public DataTable GetDA_PRD_SEL_INPUT_HALFPROD()
        {
            DataTable inDataTable = new DataTable();
            inDataTable.Columns.Add("LANGID", typeof(string));
            inDataTable.Columns.Add("LOTID", typeof(string));
            inDataTable.Columns.Add("MTRLTYPE", typeof(string));

            return inDataTable;
        }

        /// <summary>
        /// Tray 삭제 - Washing, 초소형 Winding, 초소형 Washing
        /// </summary>
        /// <returns></returns>
        public DataTable GetBR_PRD_REG_DELETE_OUT_LOT_WS()
        {
            DataTable inDataTable = new DataTable();
            inDataTable.Columns.Add("SRCTYPE", typeof(string));
            inDataTable.Columns.Add("IFMODE", typeof(string));
            inDataTable.Columns.Add("EQPTID", typeof(string));
            inDataTable.Columns.Add("PROD_LOTID", typeof(string));
            inDataTable.Columns.Add("OUT_LOTID", typeof(string));
            inDataTable.Columns.Add("TRAYID", typeof(string));
            inDataTable.Columns.Add("WO_DETL_ID", typeof(string));
            inDataTable.Columns.Add("USERID", typeof(string));

            return inDataTable;
        }

        /// <summary>
        /// Tray 확정취소
        /// </summary>
        /// <returns></returns>
        public DataSet GetBR_PRD_REG_CNFM_CANCEL_OUT_LOT_WS()
        {
            // 원각형 자재 투입
            DataSet indataSet = new DataSet();

            DataTable inDataTable = indataSet.Tables.Add("IN_EQP");
            inDataTable.Columns.Add("SRCTYPE", typeof(string));
            inDataTable.Columns.Add("IFMODE", typeof(string));
            inDataTable.Columns.Add("EQPTID", typeof(string));
            inDataTable.Columns.Add("PROD_LOTID", typeof(string));
            inDataTable.Columns.Add("USERID", typeof(string));

            DataTable InCSTTable = indataSet.Tables.Add("IN_CST");
            InCSTTable.Columns.Add("OUT_LOTID", typeof(string));
            InCSTTable.Columns.Add("CSTID", typeof(string));

            return indataSet;
        }

        #endregion

        #region [Winding]

        public DataTable GetDA_PRD_SEL_HALFPROD_DATA_WN()
        {
            DataTable inDataTable = new DataTable();
            inDataTable.Columns.Add("LANGID", typeof(string));
            inDataTable.Columns.Add("PROCID", typeof(string));
            inDataTable.Columns.Add("LOTID", typeof(string));

            return inDataTable;
        }

        public DataTable GetDA_PRD_SEL_HALFPROD_LIST_WN()
        {
            DataTable inDataTable = new DataTable();
            inDataTable.Columns.Add("LANGID", typeof(string));
            inDataTable.Columns.Add("EQSGID", typeof(string));
            inDataTable.Columns.Add("PROCID", typeof(string));
            inDataTable.Columns.Add("PRODID", typeof(string));
            inDataTable.Columns.Add("ELECTRODETYPE", typeof(string));
            inDataTable.Columns.Add("LOTID", typeof(string));
            inDataTable.Columns.Add("STARTDT", typeof(string));
            inDataTable.Columns.Add("ENDDT", typeof(string));

            return inDataTable;
        }

        public DataSet GetBR_PRD_REG_INPUT_MTRL_WN()
        {
            // 원각형 자재 투입
            DataSet indataSet = new DataSet();

            DataTable inDataTable = indataSet.Tables.Add("IN_EQP");
            inDataTable.Columns.Add("SRCTYPE", typeof(string));
            inDataTable.Columns.Add("IFMODE", typeof(string));
            inDataTable.Columns.Add("EQPTID", typeof(string));
            inDataTable.Columns.Add("USERID", typeof(string));
            inDataTable.Columns.Add("PROD_LOTID", typeof(string));

            DataTable inProduct = indataSet.Tables.Add("IN_INPUT");
            inProduct.Columns.Add("EQPT_MOUNT_PSTN_ID", typeof(string));
            inProduct.Columns.Add("EQPT_MOUNT_PSTN_STATE", typeof(string));
            inProduct.Columns.Add("MTRLID", typeof(string));
            inProduct.Columns.Add("INPUT_LOTID", typeof(string));
            inProduct.Columns.Add("INPUT_QTY", typeof(double));

            return indataSet;
        }

        public DataSet GetBR_PRD_DEL_INPUT_ITEM_WN()
        {
            // 원각형 자재 투입
            DataSet indataSet = new DataSet();

            DataTable inDataTable = indataSet.Tables.Add("IN_EQP");
            inDataTable.Columns.Add("SRCTYPE", typeof(string));
            inDataTable.Columns.Add("IFMODE", typeof(string));
            inDataTable.Columns.Add("EQPTID", typeof(string));
            inDataTable.Columns.Add("USERID", typeof(string));
            inDataTable.Columns.Add("PROD_LOTID", typeof(string));

            DataTable inProduct = indataSet.Tables.Add("IN_INPUT");
            inProduct.Columns.Add("INPUT_SEQNO", typeof(Int64));
            inProduct.Columns.Add("INPUT_LOTID", typeof(string));

            return indataSet;
        }

        public DataSet GetBR_PRD_DEL_INPUT_LOT_AS()
        {
            DataSet indataSet = new DataSet();

            DataTable inDataTable = indataSet.Tables.Add("INDATA");
            inDataTable.Columns.Add("SRCTYPE", typeof(string));
            inDataTable.Columns.Add("IFMODE", typeof(string));
            inDataTable.Columns.Add("EQPTID", typeof(string));
            inDataTable.Columns.Add("USERID", typeof(string));
            inDataTable.Columns.Add("PROD_LOTID", typeof(string));

            DataTable inProduct = indataSet.Tables.Add("INLOT");
            inProduct.Columns.Add("INPUT_SEQNO", typeof(string));
            inProduct.Columns.Add("INPUT_LOTID", typeof(string));
            inProduct.Columns.Add("WIPQTY", typeof(decimal));
            inProduct.Columns.Add("EQPT_MOUNT_PSTN_ID", typeof(string));

            return indataSet;
        }

        public DataSet GetBR_PRD_REG_INPUT_LOT_WN()
        {
            // 원각형 반제품 투입 (WINDING)
            DataSet indataSet = new DataSet();

            DataTable inDataTable = indataSet.Tables.Add("IN_EQP");
            inDataTable.Columns.Add("SRCTYPE", typeof(string));
            inDataTable.Columns.Add("IFMODE", typeof(string));
            inDataTable.Columns.Add("EQPTID", typeof(string));
            inDataTable.Columns.Add("USERID", typeof(string));
            inDataTable.Columns.Add("PROD_LOTID", typeof(string));

            DataTable inProduct = indataSet.Tables.Add("IN_INPUT");
            inProduct.Columns.Add("EQPT_MOUNT_PSTN_ID", typeof(string));
            inProduct.Columns.Add("EQPT_MOUNT_PSTN_STATE", typeof(string));
            inProduct.Columns.Add("PRODID", typeof(string));
            inProduct.Columns.Add("INPUT_LOTID", typeof(string));
            inProduct.Columns.Add("INPUT_QTY", typeof(double));

            return indataSet;
        }

        public DataSet GetBR_PRD_REG_END_LOT_WN()
        {
            // 원각형 조립 실적확정 (WINDING)
            DataSet indataSet = new DataSet();

            DataTable inDataTable = indataSet.Tables.Add("INDATA");
            inDataTable.Columns.Add("SRCTYPE", typeof(string));
            inDataTable.Columns.Add("IFMODE", typeof(string));
            inDataTable.Columns.Add("EQPTID", typeof(string));
            inDataTable.Columns.Add("SHIFT", typeof(string));
            inDataTable.Columns.Add("WIPDTTM_ED", typeof(DateTime));
            inDataTable.Columns.Add("WIPNOTE", typeof(string));
            inDataTable.Columns.Add("WRK_USERID", typeof(string));
            inDataTable.Columns.Add("WRK_USER_NAME", typeof(string));
            inDataTable.Columns.Add("USERID", typeof(string));
            inDataTable.Columns.Add("PROD_LOTID", typeof(string));
            inDataTable.Columns.Add("INPUTQTY", typeof(double));
            inDataTable.Columns.Add("OUTPUTQTY", typeof(double));
            inDataTable.Columns.Add("RESNQTY", typeof(double));

            return indataSet;
        }
        //=====================================================================================================================
        // C20220822-000365 원통형 9,10호 실적 자동 확정 기능 구현
        //=====================================================================================================================
        public DataSet GetBR_PRD_REG_WINDING_AUTO_RSLT_CNFM()
        {
            // 원각형 조립 실적확정 (WINDING)
            DataSet indataSet = new DataSet();

            DataTable inDataTable = indataSet.Tables.Add("INDATA");
            inDataTable.Columns.Add("SRCTYPE", typeof(string));
            inDataTable.Columns.Add("IFMODE", typeof(string));
            inDataTable.Columns.Add("EQPTID", typeof(string));
            inDataTable.Columns.Add("PROD_LOTID", typeof(string));
            inDataTable.Columns.Add("USERID", typeof(string));

            return indataSet;
        }

        public DataSet GetBR_PRD_REG_START_LOT_WN()
        {
            // WINDER LOT START
            DataSet indataSet = new DataSet();

            DataTable inDataTable = indataSet.Tables.Add("IN_EQP");
            inDataTable.Columns.Add("SRCTYPE", typeof(string));
            inDataTable.Columns.Add("IFMODE", typeof(string));
            inDataTable.Columns.Add("EQPTID", typeof(string));
            inDataTable.Columns.Add("WO_DETL_ID", typeof(string));
            //inDataTable.Columns.Add("PROD_LOTID", typeof(string));
            inDataTable.Columns.Add("USERID", typeof(string));

            DataTable inProduct = indataSet.Tables.Add("IN_INPUT");
            inProduct.Columns.Add("EQPT_MOUNT_PSTN_ID", typeof(string));
            inProduct.Columns.Add("EQPT_MOUNT_PSTN_STATE", typeof(string));
            inProduct.Columns.Add("PRODID", typeof(string));
            inProduct.Columns.Add("INPUT_LOTID", typeof(string));
            inProduct.Columns.Add("INPUT_QTY", typeof(double));

            DataTable inMaterial = indataSet.Tables.Add("IN_ITEM");
            inMaterial.Columns.Add("EQPT_MOUNT_PSTN_ID", typeof(string));
            inMaterial.Columns.Add("EQPT_MOUNT_PSTN_STATE", typeof(string));
            inMaterial.Columns.Add("MTRLID", typeof(string));
            inMaterial.Columns.Add("INPUT_LOTID", typeof(string));
            inMaterial.Columns.Add("INPUT_QTY", typeof(double));

            return indataSet;
        }

        public DataSet GetBR_PRD_REG_START_PROD_LOT_WN()
        {
            DataSet indataSet = new DataSet();

            DataTable inEqpTable = indataSet.Tables.Add("IN_EQP");
            inEqpTable.Columns.Add("SRCTYPE", typeof(string));
            inEqpTable.Columns.Add("IFMODE", typeof(string));
            inEqpTable.Columns.Add("EQPTID", typeof(string));
            inEqpTable.Columns.Add("USERID", typeof(string));
            inEqpTable.Columns.Add("EQPT_LOTID", typeof(string));
            inEqpTable.Columns.Add("WO_DETL_ID", typeof(string));

            DataTable inProduct = indataSet.Tables.Add("IN_INPUT");
            inProduct.Columns.Add("EQPT_MOUNT_PSTN_ID", typeof(string));
            inProduct.Columns.Add("EQPT_MOUNT_PSTN_STATE", typeof(string));
            inProduct.Columns.Add("INPUT_LOTID", typeof(string));
            inProduct.Columns.Add("INPUT_QTY", typeof(double));

            return indataSet;
        }

        /// <summary>
        /// 이력카드 출력
        /// </summary>
        /// <returns></returns>
        public DataTable GetDA_BAS_SEL_WINDING_RUNCARD_LIST()
        {
            DataTable inDataTable = new DataTable();
            inDataTable.Columns.Add("EQPTID", typeof(string));
            inDataTable.Columns.Add("EQSGID", typeof(string));
            inDataTable.Columns.Add("FRCALDATE", typeof(string));
            inDataTable.Columns.Add("TOCALDATE", typeof(string));
            inDataTable.Columns.Add("LOTID", typeof(string));
            inDataTable.Columns.Add("WINDING_RUNCARD_ID", typeof(string));
            inDataTable.Columns.Add("PROCID", typeof(string));

            return inDataTable;
        }

        public DataSet GetBR_PRD_SEL_WINDING_RUNCARD_WN()
        {
            // 원각형 반제품 투입 (WINDING)
            DataSet indataSet = new DataSet();
            DataTable inDataTable = indataSet.Tables.Add("IN_DATA");
            inDataTable.Columns.Add("LANGID", typeof(string));
            inDataTable.Columns.Add("LOTID", typeof(string));

            return indataSet;
        }
        #endregion

        #region [Assembly]


        public DataSet GetBR_PRD_REG_END_LOT_AS()
        {
            // 원각형 조립 실적확정 (ASSEMBLY)
            DataSet indataSet = new DataSet();

            DataTable inDataTable = indataSet.Tables.Add("INDATA");
            inDataTable.Columns.Add("SRCTYPE", typeof(string));
            inDataTable.Columns.Add("IFMODE", typeof(string));
            inDataTable.Columns.Add("EQPTID", typeof(string));
            inDataTable.Columns.Add("SHIFT", typeof(string));
            inDataTable.Columns.Add("WIPDTTM_ED", typeof(DateTime));
            inDataTable.Columns.Add("WIPNOTE", typeof(string));
            inDataTable.Columns.Add("WRK_USERID", typeof(string));
            inDataTable.Columns.Add("WRK_USER_NAME", typeof(string));
            inDataTable.Columns.Add("USERID", typeof(string));
            inDataTable.Columns.Add("PROD_LOTID", typeof(string));
            inDataTable.Columns.Add("INPUTQTY", typeof(double));
            inDataTable.Columns.Add("OUTPUTQTY", typeof(double));
            inDataTable.Columns.Add("RESNQTY", typeof(double));
            inDataTable.Columns.Add("INPUT_DIFF_QTY", typeof(double));
            return indataSet;
        }

        public DataSet GetBR_PRD_REG_START_LOT_AS()
        {
            // ASSEMBLY LOT START
            DataSet indataSet = new DataSet();

            DataTable inDataTable = indataSet.Tables.Add("IN_EQP");
            inDataTable.Columns.Add("SRCTYPE", typeof(string));
            inDataTable.Columns.Add("IFMODE", typeof(string));
            inDataTable.Columns.Add("EQPTID", typeof(string));
            inDataTable.Columns.Add("WO_DETL_ID", typeof(string));
            inDataTable.Columns.Add("PROD_LOTID", typeof(string));
            inDataTable.Columns.Add("USERID", typeof(string));

            DataTable inProduct = indataSet.Tables.Add("IN_INPUT");
            inProduct.Columns.Add("EQPT_MOUNT_PSTN_ID", typeof(string));
            inProduct.Columns.Add("EQPT_MOUNT_PSTN_STATE", typeof(string));
            inProduct.Columns.Add("PRODID", typeof(string));
            inProduct.Columns.Add("WINDING_RUNCARD_ID", typeof(string));
            inProduct.Columns.Add("INPUT_QTY", typeof(double));

            return indataSet;
        }

        public DataTable GetDA_PRD_SEL_HALFPROD_LIST_AS()
        {
            DataTable inDataTable = new DataTable();
            inDataTable.Columns.Add("LANGID", typeof(string));
            inDataTable.Columns.Add("PROCID", typeof(string));
            inDataTable.Columns.Add("EQSGID", typeof(string));
            inDataTable.Columns.Add("LOTID", typeof(string));

            return inDataTable;
        }

        public DataTable GetBR_PRD_GET_NEW_PROD_LOTID_AS()
        {
            DataTable inDataTable = new DataTable();
            inDataTable.Columns.Add("SRCTYPE", typeof(string));
            inDataTable.Columns.Add("IFMODE", typeof(string));
            inDataTable.Columns.Add("EQPTID", typeof(string));
            inDataTable.Columns.Add("USERID", typeof(string));
            inDataTable.Columns.Add("INPUT_LOTID", typeof(string));

            return inDataTable;
        }

        #endregion

        #region [Washing]

        public DataSet GetBR_PRD_REG_START_LOT_WS()
        {
            // WINDER LOT START
            DataSet indataSet = new DataSet();

            DataTable inDataTable = indataSet.Tables.Add("IN_EQP");
            inDataTable.Columns.Add("SRCTYPE", typeof(string));
            inDataTable.Columns.Add("IFMODE", typeof(string));
            inDataTable.Columns.Add("EQPTID", typeof(string));
            inDataTable.Columns.Add("WO_DETL_ID", typeof(string));
            inDataTable.Columns.Add("PROD_LOTID", typeof(string));
            inDataTable.Columns.Add("USERID", typeof(string));

            return indataSet;
        }

        /// <summary>
        /// tray 이동 - To Lot List
        /// </summary>
        /// <returns></returns>
        public DataTable GetDA_PRD_SEL_MOVE_TO_LOT_LIST_WS()
        {
            DataTable inDataTable = new DataTable();
            inDataTable.Columns.Add("LANGID", typeof(string));
            inDataTable.Columns.Add("PROD_LOTID", typeof(string));

            return inDataTable;
        }

        /// <summary>
        /// Tray 이동 - From Tray List
        /// </summary>
        /// <returns></returns>
        public DataTable GetDA_PRD_SEL_MOVE_FROM_OUT_LOT_LIST_WS()
        {
            DataTable inDataTable = new DataTable();
            inDataTable.Columns.Add("PROD_LOTID", typeof(string));
            inDataTable.Columns.Add("PROCID", typeof(string));
            inDataTable.Columns.Add("EQSGID", typeof(string));
            inDataTable.Columns.Add("EQPTID", typeof(string));

            return inDataTable;
        }

        /// <summary>
        /// Tray 이동 - To Tray List
        /// </summary>
        /// <returns></returns>
        public DataTable GetDA_PRD_SEL_OUT_LOT_LIST_WS()
        {
            DataTable inDataTable = new DataTable();
            inDataTable.Columns.Add("LANGID", typeof(string));
            inDataTable.Columns.Add("AREAID", typeof(string));
            inDataTable.Columns.Add("PR_LOTID", typeof(string));
            inDataTable.Columns.Add("PROCID", typeof(string));
            inDataTable.Columns.Add("EQSGID", typeof(string));
            inDataTable.Columns.Add("EQPTID", typeof(string));
            inDataTable.Columns.Add("TRAYID", typeof(string));

            return inDataTable;
        }

        /// <summary>
        /// Tray 이동 저장
        /// </summary>
        /// <returns></returns>
        public DataSet GetBR_PRD_REG_MOVE_OUT_LOT_WS()
        {
            DataSet indataSet = new DataSet();

            DataTable inDataTable = indataSet.Tables.Add("IN_DATA");
            inDataTable.Columns.Add("SRCTYPE", typeof(string));
            inDataTable.Columns.Add("IFMODE", typeof(string));
            inDataTable.Columns.Add("EQPTID", typeof(string));
            inDataTable.Columns.Add("FROM_PROD_LOTID", typeof(string));
            inDataTable.Columns.Add("TO_PROD_LOTID", typeof(string));
            inDataTable.Columns.Add("USERID", typeof(string));

            DataTable inInputLot = indataSet.Tables.Add("IN_CST");
            inInputLot.Columns.Add("OUT_LOTID", typeof(string));
            inInputLot.Columns.Add("CSTID", typeof(string));

            return indataSet;
        }

        public DataSet GetBR_PRD_REG_END_OUT_LOT_WS()
        {
            DataSet indataSet = new DataSet();

            DataTable inDataTable = indataSet.Tables.Add("IN_EQP");
            inDataTable.Columns.Add("SRCTYPE", typeof(string));
            inDataTable.Columns.Add("IFMODE", typeof(string));
            inDataTable.Columns.Add("EQPTID", typeof(string));
            inDataTable.Columns.Add("PROD_LOTID", typeof(string));
            inDataTable.Columns.Add("USERID", typeof(string));

            DataTable inInputLot = indataSet.Tables.Add("IN_CST");
            inInputLot.Columns.Add("OUT_LOTID", typeof(string));
            inInputLot.Columns.Add("OUTPUT_QTY", typeof(int));
            inInputLot.Columns.Add("CSTID", typeof(string));
            inInputLot.Columns.Add("SPCL_CST_GNRT_FLAG", typeof(string));
            inInputLot.Columns.Add("SPCL_CST_NOTE", typeof(string));
            inInputLot.Columns.Add("SPCL_CST_RSNCODE", typeof(string));

            return indataSet;
        }

        public DataSet GetBR_PRD_REG_TRAY_ALL_OUT_WS()
        {
            DataSet indataSet = new DataSet();

            DataTable inDataTable = indataSet.Tables.Add("IN_EQP");
            inDataTable.Columns.Add("SRCTYPE", typeof(string));
            inDataTable.Columns.Add("IFMODE", typeof(string));
            inDataTable.Columns.Add("EQPTID", typeof(string));
            inDataTable.Columns.Add("USERID", typeof(string));
            inDataTable.Columns.Add("PROD_LOTID", typeof(string));
            inDataTable.Columns.Add("EQPT_LOTID", typeof(string));

            DataTable inInputLot = indataSet.Tables.Add("IN_CST");
            inInputLot.Columns.Add("OUT_LOTID", typeof(string));
            inInputLot.Columns.Add("CSTID", typeof(string));
            inInputLot.Columns.Add("OUTPUT_QTY", typeof(int));

            return indataSet;
        }

        public DataSet GetBR_PRD_REG_UPD_OUT_LOT_WS()
        {
            DataSet indataSet = new DataSet();

            DataTable inDataTable = indataSet.Tables.Add("IN_EQP");
            inDataTable.Columns.Add("SRCTYPE", typeof(string));
            inDataTable.Columns.Add("IFMODE", typeof(string));
            inDataTable.Columns.Add("EQPTID", typeof(string));
            inDataTable.Columns.Add("PROD_LOTID", typeof(string));
            inDataTable.Columns.Add("USERID", typeof(string));

            DataTable inLot = indataSet.Tables.Add("IN_LOT");
            inLot.Columns.Add("OUT_LOTID", typeof(string));
            inLot.Columns.Add("CSTID", typeof(string));
            inLot.Columns.Add("WIPQTY", typeof(int));

            DataTable inSpcl = indataSet.Tables.Add("IN_SPCL");
            inSpcl.Columns.Add("SPCL_CST_GNRT_FLAG", typeof(string));
            inSpcl.Columns.Add("SPCL_CST_NOTE", typeof(string));
            inSpcl.Columns.Add("SPCL_CST_RSNCODE", typeof(string));

            return indataSet;
        }

        public DataSet GetBR_PRD_REG_MODIFY_OUT_LOT_WS()
        {
            DataSet indataSet = new DataSet();

            DataTable inDataTable = indataSet.Tables.Add("IN_EQP");
            inDataTable.Columns.Add("SRCTYPE", typeof(string));
            inDataTable.Columns.Add("IFMODE", typeof(string));
            inDataTable.Columns.Add("EQPTID", typeof(string));
            inDataTable.Columns.Add("PROD_LOTID", typeof(string));
            inDataTable.Columns.Add("OUT_LOTID", typeof(string));
            inDataTable.Columns.Add("TRAYID", typeof(string));
            inDataTable.Columns.Add("CELLQTY", typeof(Int32));
            inDataTable.Columns.Add("WO_DETL_ID", typeof(string));
            inDataTable.Columns.Add("USERID", typeof(string));

            DataTable inLot = indataSet.Tables.Add("IN_CST");
            inLot.Columns.Add("CSTSLOT", typeof(string));
            inLot.Columns.Add("CELL_FLAG", typeof(string));
            inLot.Columns.Add("SUBLOTID", typeof(string));

            return indataSet;
        }

        public DataTable GetDA_PRD_SEL_LOT_LIST_WS()
        {
            DataTable inDataTable = new DataTable();
            inDataTable.Columns.Add("LANGID", typeof(string));
            inDataTable.Columns.Add("PROCID", typeof(string));
            inDataTable.Columns.Add("WOID", typeof(string));

            return inDataTable;
        }

        public DataSet GetBR_PRD_REG_START_OUT_LOT_WS()
        {
            DataSet indataSet = new DataSet();

            DataTable inDataTable = indataSet.Tables.Add("IN_EQP");
            inDataTable.Columns.Add("SRCTYPE", typeof(string));
            inDataTable.Columns.Add("IFMODE", typeof(string));
            inDataTable.Columns.Add("EQPTID", typeof(string));
            inDataTable.Columns.Add("USERID", typeof(string));
            inDataTable.Columns.Add("PROD_LOTID", typeof(string));
            inDataTable.Columns.Add("EQPT_LOTID", typeof(string));
            inDataTable.Columns.Add("CSTID", typeof(string));
            inDataTable.Columns.Add("OUTPUT_QTY", typeof(decimal));

            DataTable inInputCst = indataSet.Tables.Add("IN_CST");
            inInputCst.Columns.Add("CSTSLOT", typeof(string));
            inInputCst.Columns.Add("CSTSLOT_F", typeof(string));
            inInputCst.Columns.Add("SUBLOTID", typeof(string));

            DataTable inInPut = indataSet.Tables.Add("IN_INPUT");
            inInPut.Columns.Add("EQPT_MOUNT_PSTN_ID", typeof(string));
            inInPut.Columns.Add("EQPT_MOUNT_PSTN_STATE", typeof(string));
            inInPut.Columns.Add("INPUT_LOTID", typeof(string));
            inInPut.Columns.Add("MTRLID", typeof(string));

            return indataSet;
        }

        public DataSet GetBR_PRD_GET_TRAY_INFO_WS()
        {
            DataSet indataSet = new DataSet();

            DataTable inDataTable = indataSet.Tables.Add("INDATA");
            inDataTable.Columns.Add("SRCTYPE", typeof(string));
            inDataTable.Columns.Add("PROD_LOTID", typeof(string));
            inDataTable.Columns.Add("TRAYID", typeof(string));
            inDataTable.Columns.Add("OUT_LOTID", typeof(string));

            return indataSet;
        }

        public DataTable GetDA_BAS_SEL_TRAY_LIST_WS()
        {
            DataTable inDataTable = new DataTable();
            inDataTable.Columns.Add("LANGID", typeof(string));
            inDataTable.Columns.Add("PROCID", typeof(string));
            inDataTable.Columns.Add("EQSGID", typeof(string));
            inDataTable.Columns.Add("TRAYID", typeof(string));
            inDataTable.Columns.Add("SPCL_RSNCODE", typeof(string));
            inDataTable.Columns.Add("FROM_DATE", typeof(string));
            inDataTable.Columns.Add("TO_DATE", typeof(string));

            return inDataTable;
        }

        public DataSet GetBR_PRD_REG_END_LOT_WS()
        {
            // 원각형 조립 실적확정 (WASHING)
            DataSet indataSet = new DataSet();

            DataTable inDataTable = indataSet.Tables.Add("INDATA");
            inDataTable.Columns.Add("SRCTYPE", typeof(string));
            inDataTable.Columns.Add("IFMODE", typeof(string));
            inDataTable.Columns.Add("EQPTID", typeof(string));
            inDataTable.Columns.Add("SHIFT", typeof(string));
            inDataTable.Columns.Add("WIPDTTM_ED", typeof(DateTime));
            inDataTable.Columns.Add("WIPNOTE", typeof(string));
            //inDataTable.Columns.Add("WRK_USERID", typeof(string));
            inDataTable.Columns.Add("WRK_USER_NAME", typeof(string));
            inDataTable.Columns.Add("USERID", typeof(string));
            inDataTable.Columns.Add("PROD_LOTID", typeof(string));
            inDataTable.Columns.Add("INPUT_QTY", typeof(double));
            inDataTable.Columns.Add("OUTPUT_QTY", typeof(double));
            inDataTable.Columns.Add("RESNQTY", typeof(double));

            return indataSet;
        }



        #endregion

        #endregion


        #region Common

        //라벨프린터 이력저장
        public DataTable GetBR_COR_REG_LABEL_PUB_HIST_G()
        {
            DataTable inDataTable = new DataTable();
            inDataTable.Columns.Add("BARCODE_NO", typeof(string));
            inDataTable.Columns.Add("INSDTTM", typeof(string));
            inDataTable.Columns.Add("ORG_CODE", typeof(string));
            inDataTable.Columns.Add("WOID", typeof(string));
            inDataTable.Columns.Add("ERP_ORDER_NO", typeof(string));
            inDataTable.Columns.Add("MTRLID", typeof(string));
            inDataTable.Columns.Add("CUSTOMERID", typeof(string));
            inDataTable.Columns.Add("SUPPLIERID", typeof(string));
            inDataTable.Columns.Add("PUB_YMD", typeof(string));
            inDataTable.Columns.Add("LBL_ID", typeof(string));
            inDataTable.Columns.Add("LBL_VER_NO", typeof(string));
            inDataTable.Columns.Add("LBL_WRK_TP_CODE", typeof(string));
            inDataTable.Columns.Add("LBL_SRL_NO", typeof(string));
            inDataTable.Columns.Add("PUB_TP_CODE", typeof(string));
            inDataTable.Columns.Add("WO_QTY", typeof(string));
            inDataTable.Columns.Add("PRT_QTY", typeof(string));
            inDataTable.Columns.Add("DISTRB_TP_CODE", typeof(string));
            inDataTable.Columns.Add("RE_PUB_RSN", typeof(string));
            inDataTable.Columns.Add("BUYER_SRL_NO", typeof(string));
            inDataTable.Columns.Add("BUYER_MDL_NAME", typeof(string));
            inDataTable.Columns.Add("NOTES", typeof(string));
            inDataTable.Columns.Add("LBL_TP_CODE", typeof(string));
            inDataTable.Columns.Add("LBL_MDL_SFFX_NAME", typeof(string));
            inDataTable.Columns.Add("LBL_QR_LAYER_ID", typeof(string));
            inDataTable.Columns.Add("INSUSER", typeof(string));
            inDataTable.Columns.Add("UPDUSER", typeof(string));
            inDataTable.Columns.Add("UPDDTTM", typeof(string));

            return inDataTable;
        }

        #endregion

        #region 자재관리
        /// <summary>
        /// 자재ID 조회
        /// </summary>
        /// <returns></returns>
        public DataTable GetCOR_SEL_ORG_MTRLID_CBO_G()
        {
            DataTable inDataTable = new DataTable();
            inDataTable.Columns.Add("LANGID", typeof(string));
            inDataTable.Columns.Add("SHOPID", typeof(string));
            inDataTable.Columns.Add("ORG_CODE", typeof(string));
            inDataTable.Columns.Add("MULTI_ORG_CODE", typeof(string));
            inDataTable.Columns.Add("MTRLTYPE", typeof(string));
            inDataTable.Columns.Add("MTRLID", typeof(string));
            inDataTable.Columns.Add("LOCAL_MTRL_CLASS_CODE1", typeof(string));
            inDataTable.Columns.Add("LOCAL_MTRL_CLASS_CODE2", typeof(string));
            inDataTable.Columns.Add("LOCAL_MTRL_CLASS_CODE3", typeof(string));

            return inDataTable;
        }

        /// <summary>
        /// MLOT 입고를 위한 ORG MTRLID 조회
        /// </summary>
        /// <returns></returns>
        public DataTable GetBR_COR_SEL_ORG_MTRLID_FOR_STORE_MLOT_G()
        {
            DataTable inDataTable = new DataTable();
            inDataTable.Columns.Add("LANGID", typeof(string));
            inDataTable.Columns.Add("SHOPID", typeof(string));
            inDataTable.Columns.Add("ORG_CODE", typeof(string));
            inDataTable.Columns.Add("MULTI_ORG_CODE", typeof(string));
            inDataTable.Columns.Add("MTRLTYPE", typeof(string));
            inDataTable.Columns.Add("MTRLID", typeof(string));
            inDataTable.Columns.Add("MLOT_TP_CODE", typeof(string));

            return inDataTable;
        }

        /// <summary>
        /// MLOT - 입고분류코드
        /// </summary>
        /// <returns></returns>
        public DataTable GetCOR_SEL_MLOT_RCV_GR_CODE_CBO_G()
        {
            DataTable inDataTable = new DataTable();
            inDataTable.Columns.Add("LANGID", typeof(string));
            inDataTable.Columns.Add("MLOT_RCV_GR_CODE", typeof(string));
            inDataTable.Columns.Add("SEL_STORE_FLAG", typeof(string));

            return inDataTable;
        }

        /// <summary>
        /// MLOT - 자재LOT 유형
        /// </summary>
        /// <returns></returns>
        public DataTable GetCOR_SEL_MLOT_TP_CODE_CBO_G()
        {
            DataTable inDataTable = new DataTable();
            inDataTable.Columns.Add("LANGID", typeof(string));
            inDataTable.Columns.Add("SHOPID", typeof(string));
            inDataTable.Columns.Add("ORG_CODE", typeof(string));
            inDataTable.Columns.Add("MULTI_ORG_CODE", typeof(string));
            inDataTable.Columns.Add("MLOT_TP_CODE", typeof(string));
            inDataTable.Columns.Add("SEL_STORE_FLAG", typeof(string));
            inDataTable.Columns.Add("MLOT_RCV_GR_CODE", typeof(string));
            inDataTable.Columns.Add("SUB_FLAG", typeof(string));

            return inDataTable;
        }

        /// <summary>
        /// 자재 현황 조회 - 입고 대기 
        /// </summary>
        /// <returns></returns>
        public DataTable GetCOR_SEL_SFU_RCV_WT_G()
        {
            DataTable inDataTable = new DataTable();
            inDataTable.Columns.Add("LANGID", typeof(string));
            inDataTable.Columns.Add("SEQ_NO", typeof(decimal));
            inDataTable.Columns.Add("SHOPID", typeof(string));
            inDataTable.Columns.Add("ORG_CODE", typeof(string));
            inDataTable.Columns.Add("MLOTID", typeof(string));
            inDataTable.Columns.Add("SUPPLIER_LOTID", typeof(string));
            inDataTable.Columns.Add("ORIG_SUPPLIER_LOTID", typeof(string));
            inDataTable.Columns.Add("MLOT_TP_CODE", typeof(string));
            inDataTable.Columns.Add("MTRLID", typeof(string));
            inDataTable.Columns.Add("MLOT_RCV_WT_QTY", typeof(decimal));
            inDataTable.Columns.Add("MLOT_RCV_WT_FRDATE", typeof(DateTime));
            inDataTable.Columns.Add("MLOT_RCV_WT_TODATE", typeof(DateTime));
            inDataTable.Columns.Add("MLOT_ST_CODE", typeof(string));
            inDataTable.Columns.Add("SUPPLIERID", typeof(string));
            inDataTable.Columns.Add("MLOT_GRD_CODE", typeof(string));
            inDataTable.Columns.Add("MTRLTYPE", typeof(string));
            inDataTable.Columns.Add("MLOT_RCV_GR_CODE", typeof(string));

            return inDataTable;
        }

        /// <summary>
        /// 자재현황조회 - 입고완료/출고대기/출고완료/정상진행/폐기완료
        /// </summary>
        /// <returns></returns>
        public DataTable GetCOR_SEL_MATERIALLOTLIST_G()
        {
            DataTable inDataTable = new DataTable();
            inDataTable.Columns.Add("LANGID", typeof(string));
            inDataTable.Columns.Add("MLOTID", typeof(string));
            inDataTable.Columns.Add("MTRLID", typeof(string));
            inDataTable.Columns.Add("MLOTSTAT", typeof(string));
            inDataTable.Columns.Add("NORM_MLOTSTAT_FLAG", typeof(string));
            inDataTable.Columns.Add("MLOTHOLD", typeof(string));
            inDataTable.Columns.Add("MLOTSDTTM_FR", typeof(DateTime));
            inDataTable.Columns.Add("MLOTSDTTM_TO", typeof(DateTime));
            inDataTable.Columns.Add("MLOTDTTM_STOCKED_FR", typeof(DateTime));
            inDataTable.Columns.Add("MLOTDTTM_STOCKED_TO", typeof(DateTime));
            inDataTable.Columns.Add("MLOTDTTM_IN_FR", typeof(DateTime));
            inDataTable.Columns.Add("MLOTDTTM_IN_TO", typeof(DateTime));
            inDataTable.Columns.Add("SUPPLIERID", typeof(string));
            inDataTable.Columns.Add("IQCPASS", typeof(string));
            inDataTable.Columns.Add("SHOPID", typeof(string));
            inDataTable.Columns.Add("ORG_CODE", typeof(string));
            inDataTable.Columns.Add("MULTI_ORG_CODE", typeof(string));
            inDataTable.Columns.Add("MLOT_TP_CODE", typeof(string));
            inDataTable.Columns.Add("SUPPLIER_LOTID", typeof(string));
            inDataTable.Columns.Add("ORIG_SUPPLIER_LOTID", typeof(string));
            inDataTable.Columns.Add("MLOT_GRD_CODE", typeof(string));
            inDataTable.Columns.Add("MTRLTYPE", typeof(string));
            inDataTable.Columns.Add("MLOT_RCV_GR_CODE", typeof(string));

            return inDataTable;
        }



        /// <summary>
        /// 자재입고
        /// </summary>
        /// <returns></returns>
        public DataSet GetBR_COR_REG_STORE_MATERIALLOT_G()
        {
            DataSet indataSet = new DataSet();

            DataTable inMLotTable = indataSet.Tables.Add("IN_MATERIALLOT");
            inMLotTable.Columns.Add("MLOTID", typeof(string));
            inMLotTable.Columns.Add("MLOTQTY_STOCKED", typeof(string));
            inMLotTable.Columns.Add("MLOTDTTM_STOCKED", typeof(string));
            inMLotTable.Columns.Add("DTTM_OT_SUPPLIER", typeof(string));
            inMLotTable.Columns.Add("MTRLID", typeof(string));
            inMLotTable.Columns.Add("BOMREV", typeof(string));
            inMLotTable.Columns.Add("SUPPLIERID", typeof(string));
            inMLotTable.Columns.Add("IQCPASS", typeof(string));
            inMLotTable.Columns.Add("LOTUID", typeof(string));
            inMLotTable.Columns.Add("NOTE", typeof(string));
            inMLotTable.Columns.Add("USERID", typeof(string));

            DataTable inMLotAttrTable = indataSet.Tables.Add("IN_MATERIALLOTATTR");
            inMLotAttrTable.Columns.Add("MLOTID", typeof(string));
            inMLotAttrTable.Columns.Add("ORG_CODE", typeof(string));
            inMLotAttrTable.Columns.Add("MLOT_TP_CODE", typeof(string));
            inMLotAttrTable.Columns.Add("MLOT_BARCODE", typeof(string));
            inMLotAttrTable.Columns.Add("VLD_DATE", typeof(string));
            inMLotAttrTable.Columns.Add("MLOT_ISS_QTY", typeof(string));
            inMLotAttrTable.Columns.Add("MLOT_QTY_ST_CODE", typeof(string));
            inMLotAttrTable.Columns.Add("REP_INSP_MLOTID", typeof(string));
            inMLotAttrTable.Columns.Add("IQC_NO", typeof(string));
            inMLotAttrTable.Columns.Add("SUPPLIER_LOTID", typeof(string));
            inMLotAttrTable.Columns.Add("ORIG_SUPPLIER_LOTID", typeof(string));
            inMLotAttrTable.Columns.Add("SUPPLIER_PRDTN_DATE", typeof(string));
            inMLotAttrTable.Columns.Add("SUBINV_ID", typeof(string));
            inMLotAttrTable.Columns.Add("MLOT_GRD_CODE", typeof(string));
            inMLotAttrTable.Columns.Add("MAKER_CODE", typeof(string));
            inMLotAttrTable.Columns.Add("S01", typeof(string));
            inMLotAttrTable.Columns.Add("S02", typeof(string));
            inMLotAttrTable.Columns.Add("S03", typeof(string));
            inMLotAttrTable.Columns.Add("S04", typeof(string));
            inMLotAttrTable.Columns.Add("S05", typeof(string));
            inMLotAttrTable.Columns.Add("S06", typeof(string));
            inMLotAttrTable.Columns.Add("S07", typeof(string));
            inMLotAttrTable.Columns.Add("S08", typeof(string));
            inMLotAttrTable.Columns.Add("S09", typeof(string));
            inMLotAttrTable.Columns.Add("S10", typeof(string));
            inMLotAttrTable.Columns.Add("S11", typeof(string));
            inMLotAttrTable.Columns.Add("S12", typeof(string));
            inMLotAttrTable.Columns.Add("S13", typeof(string));
            inMLotAttrTable.Columns.Add("S14", typeof(string));
            inMLotAttrTable.Columns.Add("S15", typeof(string));
            inMLotAttrTable.Columns.Add("S16", typeof(string));
            inMLotAttrTable.Columns.Add("S17", typeof(string));
            inMLotAttrTable.Columns.Add("S18", typeof(string));
            inMLotAttrTable.Columns.Add("S19", typeof(string));
            inMLotAttrTable.Columns.Add("S20", typeof(string));

            return indataSet;
        }

        /// <summary>
        /// 취소처리 - 입고취소 처리
        /// </summary>
        /// <returns></returns>
        public DataSet GetBR_COR_REG_CANCEL_STORE_MATERIALLOT_G()
        {
            DataSet indataSet = new DataSet();

            DataTable inDataTable = indataSet.Tables.Add("IN_MLOT");
            inDataTable.Columns.Add("ORG_CODE", typeof(string));
            inDataTable.Columns.Add("MLOTID", typeof(string));
            inDataTable.Columns.Add("MLOT_RCV_CNCL_QTY", typeof(Decimal));
            inDataTable.Columns.Add("NOTE", typeof(string));
            inDataTable.Columns.Add("USERID", typeof(string));

            return indataSet;
        }

        // FCCL 투입지시
        public DataSet GetBR_COR_REG_ISSUE_MATERIALLOT_G()
        {
            DataSet indataSet = new DataSet();

            DataTable inDataTable = indataSet.Tables.Add("IN_MLOT");
            inDataTable.Columns.Add("ORG_CODE", typeof(string));
            inDataTable.Columns.Add("MLOTID", typeof(string));
            inDataTable.Columns.Add("NOTE", typeof(string));
            inDataTable.Columns.Add("USERID", typeof(string));

            DataTable inUnique = indataSet.Tables.Add("IN_UNIQUE");
            inUnique.Columns.Add("ORG_CODE", typeof(string));
            inUnique.Columns.Add("PRODUCED_MTRLID", typeof(string));
            inUnique.Columns.Add("FCCL_FLAG", typeof(string));

            return indataSet;
        }

        /// <summary>
        /// 자재출고취소 처리
        /// </summary>
        /// <returns></returns>
        public DataSet GetBR_COR_REG_CANCEL_ISSUE_MATERIALLOT_G()
        {
            DataSet indataSet = new DataSet();

            DataTable inMLotTable = indataSet.Tables.Add("IN_MLOT");
            inMLotTable.Columns.Add("ORG_CODE", typeof(string));
            inMLotTable.Columns.Add("MLOTID", typeof(string));
            inMLotTable.Columns.Add("NOTE", typeof(string));
            inMLotTable.Columns.Add("USERID", typeof(string));

            return indataSet;
        }

        /// <summary>
        /// 자재Hold 처리
        /// </summary>
        /// <returns></returns>
        public DataSet GetBR_COR_REG_HOLD_MATERIALLOT_G()
        {
            DataSet indataSet = new DataSet();

            DataTable inMLotTable = indataSet.Tables.Add("IN_MLOT");
            inMLotTable.Columns.Add("ORG_CODE", typeof(string));
            inMLotTable.Columns.Add("MLOTID", typeof(string));
            inMLotTable.Columns.Add("NOTE", typeof(string));
            inMLotTable.Columns.Add("USERID", typeof(string));

            return indataSet;
        }

        /// <summary>
        /// 자재Release 처리
        /// </summary>
        /// <returns></returns>
        public DataSet GetBR_COR_REG_RELEASE_MATERIALLOT_G()
        {
            DataSet indataSet = new DataSet();

            DataTable inMLotTable = indataSet.Tables.Add("IN_MLOT");
            inMLotTable.Columns.Add("ORG_CODE", typeof(string));
            inMLotTable.Columns.Add("MLOTID", typeof(string));
            inMLotTable.Columns.Add("NOTE", typeof(string));
            inMLotTable.Columns.Add("USERID", typeof(string));

            return indataSet;
        }

        /// <summary>
        /// 자재폐기 처리
        /// </summary>
        /// <returns></returns>
        public DataSet GetBR_COR_REG_SCRAP_MATERIALLOT_G()
        {
            DataSet indataSet = new DataSet();

            DataTable inMLotTable = indataSet.Tables.Add("IN_MLOT");
            inMLotTable.Columns.Add("ORG_CODE", typeof(string));
            inMLotTable.Columns.Add("MLOTID", typeof(string));
            inMLotTable.Columns.Add("NOTE", typeof(string));
            inMLotTable.Columns.Add("USERID", typeof(string));

            return indataSet;
        }

        /// <summary>
        /// 자재폐기취소 처리
        /// </summary>
        /// <returns></returns>
        public DataSet GetBR_COR_REG_CANCEL_SCRAP_MATERIALLOT_G()
        {
            DataSet indataSet = new DataSet();

            DataTable inMLotTable = indataSet.Tables.Add("IN_MLOT");
            inMLotTable.Columns.Add("ORG_CODE", typeof(string));
            inMLotTable.Columns.Add("MLOTID", typeof(string));
            inMLotTable.Columns.Add("NOTE", typeof(string));
            inMLotTable.Columns.Add("USERID", typeof(string));

            return indataSet;
        }

        /// <summary>
        /// 수입검사 조회
        /// </summary>
        /// <returns></returns>
        public DataTable GetCOR_SEL_MATERIALLOT_IQC_INPUTED_G()
        {
            DataTable inDataTable = new DataTable();
            inDataTable.Columns.Add("LANGID", typeof(string));
            inDataTable.Columns.Add("SHOPID", typeof(string));
            inDataTable.Columns.Add("ORG_CODE", typeof(string));
            inDataTable.Columns.Add("MULTI_ORG_CODE", typeof(string));
            inDataTable.Columns.Add("IQCGBN", typeof(string));
            inDataTable.Columns.Add("MLOTID", typeof(string));
            inDataTable.Columns.Add("MTRLID", typeof(string));
            inDataTable.Columns.Add("MLOTSTAT", typeof(string));
            inDataTable.Columns.Add("MLOTDTTM_STOCKED_FR", typeof(DateTime));
            inDataTable.Columns.Add("MLOTDTTM_STOCKED_TO", typeof(DateTime));
            inDataTable.Columns.Add("DTTM_IQC_ACT_FR", typeof(DateTime));
            inDataTable.Columns.Add("DTTM_IQC_ACT_TO", typeof(DateTime));
            inDataTable.Columns.Add("SUPPLIER_PRDTN_DATE_FR", typeof(DateTime));
            inDataTable.Columns.Add("SUPPLIER_PRDTN_DATE_TO", typeof(DateTime));
            inDataTable.Columns.Add("FCCL_LOT_ID", typeof(string));
            inDataTable.Columns.Add("MAKER_CODE", typeof(string));
            inDataTable.Columns.Add("LOCAL_MTRL_CLASS_CODE", typeof(string));
            inDataTable.Columns.Add("MTRL_WIDTH", typeof(string));
            inDataTable.Columns.Add("MLOT_GRD_CODE", typeof(string));
            inDataTable.Columns.Add("STANDARD", typeof(string));
            inDataTable.Columns.Add("IQCPASS", typeof(string));

            return inDataTable;
        }

        /// <summary>
        /// TS - 투입실적 조회
        /// </summary>
        /// <returns></returns>
        public DataTable GetCUS_SEL_MLOT_INPUT_HISTORY_TS1()
        {
            DataTable inDataTable = new DataTable();
            inDataTable.Columns.Add("LANGID", typeof(string));
            inDataTable.Columns.Add("SHOPID", typeof(string));
            inDataTable.Columns.Add("ORG_CODE", typeof(string));
            inDataTable.Columns.Add("MULTI_ORG_CODE", typeof(string));
            inDataTable.Columns.Add("MLOTID", typeof(string));
            inDataTable.Columns.Add("MTRLID", typeof(string));
            inDataTable.Columns.Add("MLOTSTAT", typeof(string));
            inDataTable.Columns.Add("FCCL_LOT_ID", typeof(string));
            inDataTable.Columns.Add("MAKER_CODE", typeof(string));
            inDataTable.Columns.Add("MLOT_GRD_CODE", typeof(string));
            inDataTable.Columns.Add("MLOT_INPUTDTTM_FR", typeof(DateTime));
            inDataTable.Columns.Add("MLOT_INPUTDTTM_TO", typeof(DateTime));
            inDataTable.Columns.Add("LOTTYPE", typeof(string));
            inDataTable.Columns.Add("PRODID", typeof(string));
            inDataTable.Columns.Add("MULTI_PRODID", typeof(string));
            inDataTable.Columns.Add("CUSTOMERID", typeof(string));
            inDataTable.Columns.Add("LOCAL_MTRL_CLASS_CODE", typeof(string));
            inDataTable.Columns.Add("LOTID", typeof(string));

            return inDataTable;
        }

        /// <summary>
        /// TS - 자재재공 조회
        /// </summary>
        /// <returns></returns>
        public DataTable GetCUS_SEL_MLOT_CURRENT_HISTORY_TS1()
        {
            DataTable inDataTable = new DataTable();
            inDataTable.Columns.Add("LANGID", typeof(string));
            inDataTable.Columns.Add("SHOPID", typeof(string));
            inDataTable.Columns.Add("MLOTDTTM_STOCKED_FR", typeof(DateTime));
            inDataTable.Columns.Add("MLOTDTTM_STOCKED_TO", typeof(DateTime));
            inDataTable.Columns.Add("LOCAL_MTRL_CLASS_CODE", typeof(string));
            inDataTable.Columns.Add("MAKER_CODE", typeof(string));
            inDataTable.Columns.Add("MLOTSTAT", typeof(string));
            inDataTable.Columns.Add("MLOTSTAT_MULTI", typeof(string));
            inDataTable.Columns.Add("IQCPASS", typeof(string));
            inDataTable.Columns.Add("MTRLID", typeof(string));

            return inDataTable;
        }

        /// <summary>
        /// 수입검사 결과서
        /// </summary>
        /// <returns></returns>
        public DataSet GetBR_COR_REG_MATERIALDATACOLLECT_TS_G()
        {
            DataSet indataSet = new DataSet();

            DataTable inDataTable = indataSet.Tables.Add("IN_DATA");
            inDataTable.Columns.Add("MLOTID", typeof(string));
            inDataTable.Columns.Add("SEQ", typeof(string));
            inDataTable.Columns.Add("VERSION", typeof(string));
            inDataTable.Columns.Add("MLOT_GRD_CODE", typeof(string));
            inDataTable.Columns.Add("IQCPASS", typeof(string));
            inDataTable.Columns.Add("NOTE", typeof(string));
            inDataTable.Columns.Add("USERID", typeof(string));
            inDataTable.Columns.Add("SUPPLIER_PRDTN_DATE", typeof(DateTime));

            DataTable inLotTable = indataSet.Tables.Add("IN_LOT");
            inLotTable.Columns.Add("MLOTID", typeof(string));

            DataTable inVisInspTable = indataSet.Tables.Add("IN_VIS_INSP");
            inVisInspTable.Columns.Add("CLCTITEM", typeof(string));
            inVisInspTable.Columns.Add("CLCTCNT", typeof(string));
            inVisInspTable.Columns.Add("CLCTVAL01", typeof(string));
            inVisInspTable.Columns.Add("CLCTVAL02", typeof(string));
            inVisInspTable.Columns.Add("JUDGE", typeof(string));

            DataTable inDsDataTable = indataSet.Tables.Add("IN_DS_DATA");
            inDsDataTable.Columns.Add("CLCTITEM", typeof(string));
            inDsDataTable.Columns.Add("CLCTCNT", typeof(string));
            inDsDataTable.Columns.Add("CLCTVAL01", typeof(string));
            inDsDataTable.Columns.Add("CLCTVAL02", typeof(string));
            inDsDataTable.Columns.Add("CLCTVAL03", typeof(string));
            inDsDataTable.Columns.Add("JUDGE", typeof(string));

            DataTable inGrindPinHoleInspTable = indataSet.Tables.Add("IN_GRIND_PIN_HOLE_INSP");
            inGrindPinHoleInspTable.Columns.Add("CLCTITEM", typeof(string));
            inGrindPinHoleInspTable.Columns.Add("CLCTVAL01", typeof(string));
            inGrindPinHoleInspTable.Columns.Add("JUDGE", typeof(string));


            DataTable inDimInspTable = indataSet.Tables.Add("IN_DIM_INSP");
            inDimInspTable.Columns.Add("CLCTITEM", typeof(string));
            inDimInspTable.Columns.Add("CLCTCNT", typeof(string));
            inDimInspTable.Columns.Add("CLCTAVG", typeof(string));
            inDimInspTable.Columns.Add("CLCTVAL01", typeof(string));
            inDimInspTable.Columns.Add("CLCTVAL02", typeof(string));
            inDimInspTable.Columns.Add("CLCTVAL03", typeof(string));
            inDimInspTable.Columns.Add("JUDGE", typeof(string));

            return indataSet;
        }

        /// <summary>
        /// 부자재 수입검사 결과서
        /// </summary>
        /// <returns></returns>
        public DataSet GetBR_COR_REG_STORE_MATERIALDATACOLLECT_TS_G()
        {
            DataSet indataSet = new DataSet();

            DataTable inMLotTable = indataSet.Tables.Add("IN_MATERIALLOT");
            inMLotTable.Columns.Add("MLOTID", typeof(string));
            inMLotTable.Columns.Add("MLOTQTY_STOCKED", typeof(string));
            inMLotTable.Columns.Add("MLOTDTTM_STOCKED", typeof(string));
            inMLotTable.Columns.Add("DTTM_OT_SUPPLIER", typeof(string));
            inMLotTable.Columns.Add("MTRLID", typeof(string));
            inMLotTable.Columns.Add("BOMREV", typeof(string));
            inMLotTable.Columns.Add("SUPPLIERID", typeof(string));
            inMLotTable.Columns.Add("IQCPASS", typeof(string));
            inMLotTable.Columns.Add("LOTUID", typeof(string));
            inMLotTable.Columns.Add("NOTE", typeof(string));
            inMLotTable.Columns.Add("USERID", typeof(string));

            DataTable inMLotAttrTable = indataSet.Tables.Add("IN_MATERIALLOTATTR");
            inMLotAttrTable.Columns.Add("MLOTID", typeof(string));
            inMLotAttrTable.Columns.Add("ORG_CODE", typeof(string));
            inMLotAttrTable.Columns.Add("MLOT_TP_CODE", typeof(string));
            inMLotAttrTable.Columns.Add("MLOT_BARCODE", typeof(string));
            inMLotAttrTable.Columns.Add("VLD_DATE", typeof(string));
            inMLotAttrTable.Columns.Add("MLOT_ISS_QTY", typeof(string));
            inMLotAttrTable.Columns.Add("MLOT_QTY_ST_CODE", typeof(string));
            inMLotAttrTable.Columns.Add("REP_INSP_MLOTID", typeof(string));
            inMLotAttrTable.Columns.Add("IQC_NO", typeof(string));
            inMLotAttrTable.Columns.Add("SUPPLIER_LOTID", typeof(string));
            inMLotAttrTable.Columns.Add("ORIG_SUPPLIER_LOTID", typeof(string));
            inMLotAttrTable.Columns.Add("SUPPLIER_PRDTN_DATE", typeof(string));
            inMLotAttrTable.Columns.Add("SUBINV_ID", typeof(string));
            inMLotAttrTable.Columns.Add("MLOT_GRD_CODE", typeof(string));
            inMLotAttrTable.Columns.Add("MAKER_CODE", typeof(string));
            inMLotAttrTable.Columns.Add("S01", typeof(string));
            inMLotAttrTable.Columns.Add("S02", typeof(string));
            inMLotAttrTable.Columns.Add("S03", typeof(string));
            inMLotAttrTable.Columns.Add("S04", typeof(string));
            inMLotAttrTable.Columns.Add("S05", typeof(string));
            inMLotAttrTable.Columns.Add("S06", typeof(string));
            inMLotAttrTable.Columns.Add("S07", typeof(string));
            inMLotAttrTable.Columns.Add("S08", typeof(string));
            inMLotAttrTable.Columns.Add("S09", typeof(string));
            inMLotAttrTable.Columns.Add("S10", typeof(string));
            inMLotAttrTable.Columns.Add("S11", typeof(string));
            inMLotAttrTable.Columns.Add("S12", typeof(string));
            inMLotAttrTable.Columns.Add("S13", typeof(string));
            inMLotAttrTable.Columns.Add("S14", typeof(string));
            inMLotAttrTable.Columns.Add("S15", typeof(string));
            inMLotAttrTable.Columns.Add("S16", typeof(string));
            inMLotAttrTable.Columns.Add("S17", typeof(string));
            inMLotAttrTable.Columns.Add("S18", typeof(string));
            inMLotAttrTable.Columns.Add("S19", typeof(string));
            inMLotAttrTable.Columns.Add("S20", typeof(string));

            DataTable inDataTable = indataSet.Tables.Add("IN_DATA");
            inDataTable.Columns.Add("MLOTID", typeof(string));
            inDataTable.Columns.Add("SEQ", typeof(string));
            inDataTable.Columns.Add("VERSION", typeof(string));
            inDataTable.Columns.Add("MLOT_GRD_CODE", typeof(string));
            inDataTable.Columns.Add("IQCPASS", typeof(string));
            inDataTable.Columns.Add("NOTE", typeof(string));
            inDataTable.Columns.Add("USERID", typeof(string));

            DataTable inLotTable = indataSet.Tables.Add("IN_LOT");
            inLotTable.Columns.Add("MLOTID", typeof(string));

            DataTable inVisInspTable = indataSet.Tables.Add("IN_VIS_INSP");
            inVisInspTable.Columns.Add("CLCTITEM", typeof(string));
            inVisInspTable.Columns.Add("CLCTCNT", typeof(string));
            inVisInspTable.Columns.Add("CLCTVAL01", typeof(string));
            inVisInspTable.Columns.Add("CLCTVAL02", typeof(string));
            inVisInspTable.Columns.Add("JUDGE", typeof(string));

            DataTable inDsDataTable = indataSet.Tables.Add("IN_DS_DATA");
            inDsDataTable.Columns.Add("CLCTITEM", typeof(string));
            inDsDataTable.Columns.Add("CLCTCNT", typeof(string));
            inDsDataTable.Columns.Add("CLCTVAL01", typeof(string));
            inDsDataTable.Columns.Add("CLCTVAL02", typeof(string));
            inDsDataTable.Columns.Add("CLCTVAL03", typeof(string));
            inDsDataTable.Columns.Add("JUDGE", typeof(string));

            DataTable inGrindPinHoleInspTable = indataSet.Tables.Add("IN_GRIND_PIN_HOLE_INSP");
            inGrindPinHoleInspTable.Columns.Add("CLCTITEM", typeof(string));
            inGrindPinHoleInspTable.Columns.Add("CLCTVAL01", typeof(string));
            inGrindPinHoleInspTable.Columns.Add("JUDGE", typeof(string));

            DataTable inDimInspTable = indataSet.Tables.Add("IN_DIM_INSP");
            inDimInspTable.Columns.Add("CLCTITEM", typeof(string));
            inDimInspTable.Columns.Add("CLCTCNT", typeof(string));
            inDimInspTable.Columns.Add("CLCTAVG", typeof(string));
            inDimInspTable.Columns.Add("CLCTVAL01", typeof(string));
            inDimInspTable.Columns.Add("CLCTVAL02", typeof(string));
            inDimInspTable.Columns.Add("CLCTVAL03", typeof(string));
            inDimInspTable.Columns.Add("CLCTVAL04", typeof(string));
            inDimInspTable.Columns.Add("CLCTVAL05", typeof(string));
            inDimInspTable.Columns.Add("CLCTVAL06", typeof(string));
            inDimInspTable.Columns.Add("CLCTVAL07", typeof(string));
            inDimInspTable.Columns.Add("CLCTVAL08", typeof(string));
            inDimInspTable.Columns.Add("CLCTVAL09", typeof(string));
            inDimInspTable.Columns.Add("CLCTVAL10", typeof(string));
            inDimInspTable.Columns.Add("JUDGE", typeof(string));

            return indataSet;
        }

        /// <summary>
        /// 공급사 조회
        /// </summary>
        /// <returns></returns>
        public DataTable GetCOR_SEL_SHOP_SUPPLIER_CBO_G()
        {
            DataTable inDataTable = new DataTable();
            inDataTable.Columns.Add("LANGID", typeof(string));
            inDataTable.Columns.Add("SHOPID", typeof(string));
            inDataTable.Columns.Add("SUPPLIERID", typeof(string));

            return inDataTable;
        }

        /// <summary>
        /// 제조사 조회
        /// </summary>
        /// <returns></returns>
        public DataTable GetCOR_SEL_SHOP_MAKER_G()
        {
            DataTable inDataTable = new DataTable();
            inDataTable.Columns.Add("LANGID", typeof(string));
            inDataTable.Columns.Add("SHOPID", typeof(string));
            inDataTable.Columns.Add("ORG_CODE", typeof(string));
            inDataTable.Columns.Add("MULTI_ORG_CODE", typeof(string));
            inDataTable.Columns.Add("MAKER_CODE", typeof(string));
            inDataTable.Columns.Add("SUPPLIERID", typeof(string));

            return inDataTable;
        }

        //자재LOT 지정/지정 취소 조회
        public DataTable GetBR_CUS_SEL_PELLICLE_PRE_ASSGN_PM1()
        {
            DataTable inDataTable = new DataTable();
            inDataTable.Columns.Add("LANGID", typeof(string));
            inDataTable.Columns.Add("ORG_CODE", typeof(string));
            inDataTable.Columns.Add("MULTI_ORG_CODE", typeof(string));
            inDataTable.Columns.Add("PROD_LOTID", typeof(string));
            inDataTable.Columns.Add("LOTID", typeof(string));
            inDataTable.Columns.Add("PRODID", typeof(string));
            inDataTable.Columns.Add("CUSTOMERID", typeof(string));
            inDataTable.Columns.Add("LAYER_NAME", typeof(string));
            inDataTable.Columns.Add("LAYER_ID", typeof(string));
            inDataTable.Columns.Add("WIPDTTM_ST_FR", typeof(DateTime));
            inDataTable.Columns.Add("WIPDTTM_ST_TO", typeof(DateTime));
            inDataTable.Columns.Add("CONFM_FLAG", typeof(string));
            inDataTable.Columns.Add("CLE_COMPLT_FLAG", typeof(string));

            return inDataTable;
        }

        //PM1 Pellicle 자재LOT 자동 지정 처리
        public DataSet GetBR_CUS_REG_PELLICLE_PRE_ASSGN_A_PM1()
        {
            DataSet indataSet = new DataSet();

            DataTable inMLotTable = indataSet.Tables.Add("IN_PELLICLE_PRE_ASSGN");
            inMLotTable.Columns.Add("LANGID", typeof(string));
            inMLotTable.Columns.Add("ORG_CODE", typeof(string));
            inMLotTable.Columns.Add("PROD_LOTID", typeof(string));
            inMLotTable.Columns.Add("LOTID", typeof(string));
            inMLotTable.Columns.Add("GD_NAME", typeof(string));
            inMLotTable.Columns.Add("NOTE", typeof(string));
            inMLotTable.Columns.Add("USERID", typeof(string));

            return indataSet;
        }

        //자재LOT 지정/지정 취소 처리
        public DataSet GetBR_COR_REG_MLOT_PRE_ASSGN_G()
        {
            DataSet indataSet = new DataSet();

            DataTable inMLotTable = indataSet.Tables.Add("IN_MLOT_PRE_ASSGN");
            inMLotTable.Columns.Add("ORG_CODE", typeof(string));
            inMLotTable.Columns.Add("PROD_LOTID", typeof(string));
            inMLotTable.Columns.Add("PROCID", typeof(string));
            inMLotTable.Columns.Add("ASSGN_SEQ_NO", typeof(Decimal));
            inMLotTable.Columns.Add("MLOTID", typeof(string));
            inMLotTable.Columns.Add("LOTID", typeof(string));
            inMLotTable.Columns.Add("GD_NAME", typeof(string));
            inMLotTable.Columns.Add("ASSGN_FLAG", typeof(string));
            inMLotTable.Columns.Add("AUTO_ASSGN_FLAG", typeof(string));
            inMLotTable.Columns.Add("NOTE", typeof(string));
            inMLotTable.Columns.Add("USERID", typeof(string));

            return indataSet;
        }

        //자재LOT 확정/확정 취소 처리
        public DataSet GetBR_COR_REG_MLOT_PRE_ASSGN_CONFM_G()
        {
            DataSet indataSet = new DataSet();

            DataTable inMLotTable = indataSet.Tables.Add("IN_MLOT_PRE_ASSGN");
            inMLotTable.Columns.Add("ORG_CODE", typeof(string));
            inMLotTable.Columns.Add("PROD_LOTID", typeof(string));
            inMLotTable.Columns.Add("PROCID", typeof(string));
            inMLotTable.Columns.Add("ASSGN_SEQ_NO", typeof(Decimal));
            inMLotTable.Columns.Add("CONFM_FLAG", typeof(string));
            inMLotTable.Columns.Add("NOTE", typeof(string));
            inMLotTable.Columns.Add("USERID", typeof(string));

            return indataSet;
        }

        //자재LOT 수동지정 대상 조회
        public DataTable GetCUS_SEL_PELLICLE_PRE_ASSGN_WT_PM1()
        {
            DataTable inDataTable = new DataTable();
            inDataTable.Columns.Add("LANGID", typeof(string));
            inDataTable.Columns.Add("ORG_CODE", typeof(string));
            inDataTable.Columns.Add("LOTID", typeof(string));
            inDataTable.Columns.Add("GD_NAME", typeof(string));
            inDataTable.Columns.Add("PRIORITY", typeof(string));

            return inDataTable;
        }

        //자재LOT 투입 이력 조회.
        public DataTable GetCOR_SEL_MLOT_INPUT_HISTORY_G()
        {
            DataTable inDataTable = new DataTable();
            inDataTable.Columns.Add("LANGID", typeof(string));
            inDataTable.Columns.Add("SHOPID", typeof(string));
            inDataTable.Columns.Add("ORG_CODE", typeof(string));
            inDataTable.Columns.Add("MULTI_ORG_CODE", typeof(string));
            inDataTable.Columns.Add("MLOT_INPUTDTTM_FR", typeof(DateTime));
            inDataTable.Columns.Add("MLOT_INPUTDTTM_TO", typeof(DateTime));
            inDataTable.Columns.Add("PROD_LOT_PEGGINGDTTM_FR", typeof(DateTime));
            inDataTable.Columns.Add("PROD_LOT_PEGGINGDTTM_TO", typeof(DateTime));
            inDataTable.Columns.Add("MTRLID", typeof(string));
            inDataTable.Columns.Add("MLOT_TP_CODE", typeof(string));
            inDataTable.Columns.Add("PRODID", typeof(string));
            inDataTable.Columns.Add("PROD_LOTID", typeof(string));
            inDataTable.Columns.Add("LOTID", typeof(string));
            inDataTable.Columns.Add("MLOTID", typeof(string));
            inDataTable.Columns.Add("SUPPLIER_LOTID", typeof(string));

            // 2017.3.8 / 장용창 / 출하일시 기준 검색조건 추가
            inDataTable.Columns.Add("SHIPDTTM_FR", typeof(DateTime));
            inDataTable.Columns.Add("SHIPDTTM_TO", typeof(DateTime));

            return inDataTable;
        }

        /// <summary>
        /// 부자재 입고
        /// </summary>
        /// <returns></returns>
        public DataSet GetBR_CUS_REG_STORE_SUBMLOT_BY_RCV_QTY()
        {
            DataSet indataSet = new DataSet();

            DataTable inMLotTable = indataSet.Tables.Add("IN_DATA");
            inMLotTable.Columns.Add("ORG_CODE", typeof(string));
            inMLotTable.Columns.Add("MTRLID", typeof(string));
            inMLotTable.Columns.Add("CHEM_LOTID_PREFIX_CODE", typeof(string));
            inMLotTable.Columns.Add("ORIG_SUPPLIER_LOTID", typeof(string));
            inMLotTable.Columns.Add("SUBMLOT_RCV_QTY", typeof(Decimal));
            inMLotTable.Columns.Add("SUPPLIERID", typeof(string));
            inMLotTable.Columns.Add("DTTM_OT_SUPPLIER", typeof(DateTime));
            inMLotTable.Columns.Add("SUPPLIER_PRDTN_DATE", typeof(DateTime));
            inMLotTable.Columns.Add("BOMREV", typeof(string));
            inMLotTable.Columns.Add("IQCPASS", typeof(string));
            inMLotTable.Columns.Add("NOTE", typeof(string));
            inMLotTable.Columns.Add("USERID", typeof(string));

            return indataSet;
        }

        /// <summary>
        /// 약품LOT 유형 정보
        /// </summary>
        /// <returns></returns>
        public DataTable GetCUS_SEL_CHEM_LOT_TP_CODE_CBO_PM1()
        {
            DataTable inMLotTable = new DataTable();

            inMLotTable.Columns.Add("LANGID", typeof(string));
            inMLotTable.Columns.Add("SHOPID", typeof(string));
            inMLotTable.Columns.Add("ORG_CODE", typeof(string));
            inMLotTable.Columns.Add("MULTI_ORG_CODE", typeof(string));
            inMLotTable.Columns.Add("CHEM_LOT_TP_CODE", typeof(string));

            return inMLotTable;
        }

        /// <summary>
        /// 자재ID 선택 시 품명(자재 명) 표시
        /// </summary>
        /// <returns></returns>
        public DataTable GetCOR_SEL_ORG_GD_NAME_G()
        {
            DataTable inMLotTable = new DataTable();

            inMLotTable.Columns.Add("LANGID", typeof(string));
            inMLotTable.Columns.Add("SHOPID", typeof(string));
            inMLotTable.Columns.Add("ORG_CODE", typeof(string));
            inMLotTable.Columns.Add("MULTI_ORG_CODE", typeof(string));
            inMLotTable.Columns.Add("MTRLTYPE", typeof(string));
            inMLotTable.Columns.Add("MTRLID", typeof(string));

            return inMLotTable;
        }

        /// <summary>
        /// 원단 상태 FCCL 조회
        /// </summary>
        /// <returns></returns>
        public DataTable GetCUS_SEL_FCCL_ROLL_INFO()
        {
            DataTable inDataTable = new DataTable();

            inDataTable.Columns.Add("LANGID", typeof(string));
            inDataTable.Columns.Add("MAKER_CODE", typeof(string));
            inDataTable.Columns.Add("MTRL_WIDTH", typeof(string));
            inDataTable.Columns.Add("MLOT_GRD_CODE", typeof(string));

            return inDataTable;
        }

        /// <summary>
        /// 원소재  FCCL 조회
        /// </summary>
        /// <returns></returns>
        public DataTable GetCUS_SEL_FCCL_RAW_INFO()
        {
            DataTable inDataTable = new DataTable();

            inDataTable.Columns.Add("LANGID", typeof(string));
            inDataTable.Columns.Add("MAKER_CODE", typeof(string));
            inDataTable.Columns.Add("MTRL_WIDTH", typeof(string));
            inDataTable.Columns.Add("MLOT_GRD_CODE", typeof(string));

            return inDataTable;
        }

        /// <summary>
        /// MLOTID 조회
        /// </summary>
        /// <returns></returns>
        public DataTable GetCUS_SEL_FCCL_MLOTID_INFO()
        {
            DataTable inDataTable = new DataTable();

            inDataTable.Columns.Add("LANGID", typeof(string));
            inDataTable.Columns.Add("MTRLID", typeof(string));

            return inDataTable;
        }

        /// <summary>
        /// ORG 별 ATTRDEFINE 조회
        /// </summary>
        /// <returns></returns>
        public DataTable GetCUS_SEL_ATTRDEFINE_BY_ORG()
        {
            DataTable inDataTable = new DataTable();

            inDataTable.Columns.Add("LANGID", typeof(string));
            inDataTable.Columns.Add("ORG_CODE", typeof(string));
            inDataTable.Columns.Add("TABNAME", typeof(string));

            return inDataTable;
        }

        /// <summary>
        /// 부자재 입고/출고 이력 조회
        /// </summary>
        /// <returns></returns>
        public DataTable GetCOR_SEL_SUBMLOTACTHIST_FOR_MTRL_QTY_G()
        {
            DataTable inDataTable = new DataTable();

            inDataTable.Columns.Add("LANGID", typeof(string));
            inDataTable.Columns.Add("SHOPID", typeof(string));
            inDataTable.Columns.Add("ORG_CODE", typeof(string));
            inDataTable.Columns.Add("MULTI_ORG_CODE", typeof(string));
            inDataTable.Columns.Add("ACTID", typeof(string));
            inDataTable.Columns.Add("ACTDTTM_FR", typeof(DateTime));
            inDataTable.Columns.Add("ACTDTTM_TO", typeof(DateTime));
            inDataTable.Columns.Add("MTRLID", typeof(string));

            return inDataTable;
        }

        /// <summary>
        /// ORG-MTRLID 단위 MLOT 입고 처리
        /// </summary>
        /// <returns></returns>
        public DataSet GetBR_CUS_REG_STORE_SUBMLOT_BY_MTRL_QTY()
        {
            DataSet indataSet = new DataSet();

            DataTable inDataTable = indataSet.Tables.Add("INDATA");
            inDataTable.Columns.Add("ORG_CODE", typeof(string));
            inDataTable.Columns.Add("MTRLID", typeof(string));
            inDataTable.Columns.Add("BOMREV", typeof(string));
            inDataTable.Columns.Add("MLOTQTY_STOCKED", typeof(Decimal));
            inDataTable.Columns.Add("MLOTDTTM_STOCKED", typeof(DateTime));
            inDataTable.Columns.Add("DTTM_OT_SUPPLIER", typeof(DateTime));
            inDataTable.Columns.Add("SUPPLIERID", typeof(string));
            inDataTable.Columns.Add("IQCPASS", typeof(string));
            inDataTable.Columns.Add("NOTE", typeof(string));
            inDataTable.Columns.Add("USERID", typeof(string));
            inDataTable.Columns.Add("MLOT_TP_CODE", typeof(string));
            inDataTable.Columns.Add("MLOT_BARCODE", typeof(string));
            inDataTable.Columns.Add("VLD_DATE", typeof(DateTime));
            inDataTable.Columns.Add("MLOT_ISS_QTY", typeof(Decimal));
            inDataTable.Columns.Add("MLOT_QTY_ST_CODE", typeof(string));
            inDataTable.Columns.Add("REP_INSP_MLOTID", typeof(string));
            inDataTable.Columns.Add("IQC_NO", typeof(string));
            inDataTable.Columns.Add("SUPPLIER_LOTID", typeof(string));
            inDataTable.Columns.Add("ORIG_SUPPLIER_LOTID", typeof(string));
            inDataTable.Columns.Add("SUPPLIER_PRDTN_DATE", typeof(DateTime));
            inDataTable.Columns.Add("SUBINV_ID", typeof(string));
            inDataTable.Columns.Add("MLOT_GRD_CODE", typeof(string));
            inDataTable.Columns.Add("MAKER_CODE", typeof(string));
            inDataTable.Columns.Add("S01", typeof(string));
            inDataTable.Columns.Add("S02", typeof(string));
            inDataTable.Columns.Add("S03", typeof(string));
            inDataTable.Columns.Add("S04", typeof(string));
            inDataTable.Columns.Add("S05", typeof(string));
            inDataTable.Columns.Add("S06", typeof(string));
            inDataTable.Columns.Add("S07", typeof(string));
            inDataTable.Columns.Add("S08", typeof(string));
            inDataTable.Columns.Add("S09", typeof(string));
            inDataTable.Columns.Add("S10", typeof(string));
            inDataTable.Columns.Add("S11", typeof(string));
            inDataTable.Columns.Add("S12", typeof(string));
            inDataTable.Columns.Add("S13", typeof(string));
            inDataTable.Columns.Add("S14", typeof(string));
            inDataTable.Columns.Add("S15", typeof(string));
            inDataTable.Columns.Add("S16", typeof(string));
            inDataTable.Columns.Add("S17", typeof(string));
            inDataTable.Columns.Add("S18", typeof(string));
            inDataTable.Columns.Add("S19", typeof(string));
            inDataTable.Columns.Add("S20", typeof(string));

            return indataSet;
        }

        /// <summary>
        /// ORG-MTRLID 단위 MLOT 출고 처리
        /// </summary>
        /// <returns></returns>
        public DataSet GetBR_CUS_REG_ISSUE_SUBMLOT_BY_MTRL_QTY()
        {
            DataSet indataSet = new DataSet();

            DataTable inDataTable = indataSet.Tables.Add("INDATA");
            inDataTable.Columns.Add("ORG_CODE", typeof(string));
            inDataTable.Columns.Add("MTRLID", typeof(string));
            inDataTable.Columns.Add("MLOT_ISS_QTY", typeof(Decimal));
            inDataTable.Columns.Add("ACTDTTM", typeof(DateTime));
            inDataTable.Columns.Add("NOTE", typeof(string));
            inDataTable.Columns.Add("USERID", typeof(string));

            return indataSet;
        }

        /// <summary>
        /// ORG 공통 코드 조회
        /// </summary>
        /// <returns></returns>
        public DataTable GetCOR_SEL_ORG_COM_CODE_CBO_G()
        {
            DataTable inDataTable = new DataTable();

            inDataTable.Columns.Add("LANGID", typeof(string));
            inDataTable.Columns.Add("SHOPID", typeof(string));
            inDataTable.Columns.Add("ORG_CODE", typeof(string));
            inDataTable.Columns.Add("MULTI_ORG_CODE", typeof(string));
            inDataTable.Columns.Add("CMCDTYPE", typeof(string));
            inDataTable.Columns.Add("CMCODE", typeof(string));
            inDataTable.Columns.Add("ATTRIBUTE1", typeof(string));
            inDataTable.Columns.Add("ATTRIBUTE2", typeof(string));
            inDataTable.Columns.Add("ATTRIBUTE3", typeof(string));
            inDataTable.Columns.Add("ATTRIBUTE4", typeof(string));
            inDataTable.Columns.Add("ATTRIBUTE5", typeof(string));

            return inDataTable;
        }

        // FCCL 설비 Combo조회
        public DataTable GetCUS_SEL_FCCL_EQUIPMENT_G()
        {
            DataTable inDataTable = new DataTable();

            inDataTable.Columns.Add("LANGID", typeof(string));

            return inDataTable;
        }

        // FCCL 착공대기 조회
        public DataTable GetCUS_SEL_MATERIALLOT_ISSUED_FOR_START()
        {
            DataTable inDataTable = new DataTable();

            inDataTable.Columns.Add("LANGID", typeof(string));
            inDataTable.Columns.Add("ORG_CODE", typeof(string));
            inDataTable.Columns.Add("MULTI_ORG_CODE", typeof(string));
            inDataTable.Columns.Add("MLOTSDTTM_FR", typeof(DateTime));
            inDataTable.Columns.Add("MLOTSDTTM_TO", typeof(DateTime));
            inDataTable.Columns.Add("MLOTID", typeof(string));
            inDataTable.Columns.Add("SUPPLIERID", typeof(string));
            inDataTable.Columns.Add("MAKER_CODE", typeof(string));

            return inDataTable;
        }

        // FCCL 착공처리
        public DataSet GetBR_COR_REG_START_MATERIALLOT_G()
        {
            DataSet indataSet = new DataSet();

            DataTable inDataTable = indataSet.Tables.Add("INDATA");
            inDataTable.Columns.Add("SRCTYPE", typeof(string));
            inDataTable.Columns.Add("MLOTID", typeof(string));
            inDataTable.Columns.Add("PROCID", typeof(Decimal));
            inDataTable.Columns.Add("EQPTID", typeof(string));
            inDataTable.Columns.Add("PRODID", typeof(string));
            inDataTable.Columns.Add("NOTE", typeof(string));
            inDataTable.Columns.Add("USERID", typeof(string));

            DataTable inUniqueTable = indataSet.Tables.Add("IN_UNIQUE");
            inUniqueTable.Columns.Add("ORG_CODE", typeof(string));


            return indataSet;
        }

        // FCCL 완공처리
        public DataSet GetBR_CUS_REG_END_MATERIALLOT_CONSUME_G()
        {
            DataSet indataSet = new DataSet();

            DataTable inDataTable = indataSet.Tables.Add("INDATA");
            inDataTable.Columns.Add("SRCTYPE", typeof(string));
            inDataTable.Columns.Add("IFMODE", typeof(string));
            inDataTable.Columns.Add("MLOTID", typeof(string));
            inDataTable.Columns.Add("PROCID", typeof(string));
            inDataTable.Columns.Add("EQPTID", typeof(string));
            inDataTable.Columns.Add("PRODID", typeof(string));
            inDataTable.Columns.Add("USERID", typeof(string));

            DataTable inUniqueTable = indataSet.Tables.Add("IN_UNIQUE");
            inUniqueTable.Columns.Add("ORG_CODE", typeof(string));
            inUniqueTable.Columns.Add("MATERIALUSEFLAG", typeof(string));
            inUniqueTable.Columns.Add("TOOLRESETFLAG", typeof(string));

            DataTable inClotTable = indataSet.Tables.Add("IN_CLOT");
            inClotTable.Columns.Add("MLOTID", typeof(string));
            inClotTable.Columns.Add("MTRLID", typeof(string));
            inClotTable.Columns.Add("ACTQTY", typeof(Decimal));
            inClotTable.Columns.Add("SCRAPFLAG", typeof(string));
            inClotTable.Columns.Add("S01", typeof(string));
            inClotTable.Columns.Add("S02", typeof(string));
            inClotTable.Columns.Add("S03", typeof(string));
            inClotTable.Columns.Add("S04", typeof(string));
            inClotTable.Columns.Add("S05", typeof(string));

            DataTable inConsumedTable = indataSet.Tables.Add("IN_CONSUMED");
            inConsumedTable.Columns.Add("MLOTID", typeof(string));
            inConsumedTable.Columns.Add("CONSUMEDMLOTID", typeof(string));
            inConsumedTable.Columns.Add("CONSUMEDQTY", typeof(Decimal));

            DataTable inMtrlTable = indataSet.Tables.Add("IN_MTRL");
            inMtrlTable.Columns.Add("MLOTID", typeof(string));
            inMtrlTable.Columns.Add("MTRLID", typeof(string));
            inMtrlTable.Columns.Add("CONSUMEDMLOTID", typeof(string));
            inMtrlTable.Columns.Add("CONSUMEDQTY", typeof(Decimal));

            DataTable inToolTable = indataSet.Tables.Add("IN_TOOL");
            inToolTable.Columns.Add("TOOLID", typeof(string));
            inToolTable.Columns.Add("USEQTY", typeof(decimal));

            return indataSet;
        }

        // FCCL Tool Reset
        public DataSet GetBR_COR_REG_TOOL_INFO_G()
        {
            DataSet indataSet = new DataSet();

            DataTable inDataTable = indataSet.Tables.Add("INDATA");
            inDataTable.Columns.Add("TXNTYPE", typeof(string));
            inDataTable.Columns.Add("TOOLID", typeof(string));
            inDataTable.Columns.Add("EQPTID", typeof(string));
            inDataTable.Columns.Add("RESNCODE", typeof(string));
            inDataTable.Columns.Add("USERID", typeof(string));
            inDataTable.Columns.Add("NOTE", typeof(string));
            inDataTable.Columns.Add("LINEID", typeof(string));
            inDataTable.Columns.Add("EQPTTABLEID", typeof(string));
            inDataTable.Columns.Add("EQPTHEADERID", typeof(string));
            inDataTable.Columns.Add("CONFIRMUSER", typeof(string));
            inDataTable.Columns.Add("CHGNOTE1", typeof(string));
            inDataTable.Columns.Add("CHGNOTE2", typeof(string));
            inDataTable.Columns.Add("CHGSTDTTM", typeof(DateTime));
            inDataTable.Columns.Add("CHGEDDTTM", typeof(DateTime));

            return indataSet;
        }

        //자재LOT IQC 결과 정보 불러오기
        public DataTable GetCOR_SEL_MLOT_IQC_RSLT_G()
        {
            DataTable inDataTable = new DataTable();
            inDataTable.Columns.Add("LANGID", typeof(string));
            inDataTable.Columns.Add("SHOPID", typeof(string));
            inDataTable.Columns.Add("MTRL_GR_CODE", typeof(string));
            inDataTable.Columns.Add("MAKER_CODE", typeof(string));
            inDataTable.Columns.Add("MTRL_CLASS_CODE", typeof(string));
            inDataTable.Columns.Add("MTRL_GRD_CODE", typeof(string));
            inDataTable.Columns.Add("INSP_TP_CODE", typeof(string));
            inDataTable.Columns.Add("CLCTITEM", typeof(string));
            inDataTable.Columns.Add("MEASR_VALUE", typeof(double));

            return inDataTable;
        }

        #endregion

        #region 생산관리

        /// <summary>
        /// Org Code
        /// </summary>
        /// <returns></returns>
        public DataTable GetCOR_SEL_SHOP_ORG_G()
        {
            DataTable inDataTable = new DataTable();
            inDataTable.Columns.Add("LANGID", typeof(string));
            inDataTable.Columns.Add("SHOPID", typeof(string));
            inDataTable.Columns.Add("ORG_CODE", typeof(string));
            inDataTable.Columns.Add("MULTI_ORG_CODE", typeof(string));

            return inDataTable;
        }

        /// <summary>
        /// 실적마감상태 코드조회
        /// </summary>
        /// <returns></returns>
        public DataTable GetCOR_SEL_MNTH_CLOSE_ST_CODE_G()
        {
            DataTable inDataTable = new DataTable();
            inDataTable.Columns.Add("LANGID", typeof(string));
            inDataTable.Columns.Add("CLOSE_ST_CODE", typeof(string));
            inDataTable.Columns.Add("CLOSE_ST_TP_CODE", typeof(string));

            return inDataTable;
        }

        /// <summary>
        /// 실적마감통제이력 조회
        /// </summary>
        /// <returns></returns>
        public DataTable GetCOR_SEL_MNTH_CLOSE_HIST_G()
        {
            DataTable inDataTable = new DataTable();
            inDataTable.Columns.Add("LANGID", typeof(string));
            inDataTable.Columns.Add("ORG_CODE", typeof(string));
            inDataTable.Columns.Add("CLOSE_YM_FR", typeof(string));
            inDataTable.Columns.Add("CLOSE_YM_TO", typeof(string));
            inDataTable.Columns.Add("CLOSE_ST_CODE", typeof(string));

            return inDataTable;
        }

        /// <summary>
        /// 실적마감상태 조회
        /// </summary>
        /// <returns></returns>
        public DataTable GetCOR_SEL_MNTH_CLOSE_G()
        {
            DataTable inDataTable = new DataTable();
            inDataTable.Columns.Add("LANGID", typeof(string));
            inDataTable.Columns.Add("ORG_CODE", typeof(string));

            return inDataTable;
        }

        /// <summary>
        /// 실적마감 처리
        /// </summary>
        /// <returns></returns>
        public DataSet GetBR_COR_REG_MNTH_CLOSE_G()
        {
            DataSet indataSet = new DataSet();

            DataTable inDataTable = indataSet.Tables.Add("IN_MNTH_CLOSE");
            inDataTable.Columns.Add("LANGID", typeof(string));
            inDataTable.Columns.Add("ORG_CODE", typeof(string));
            inDataTable.Columns.Add("CLOSE_YM", typeof(string));
            inDataTable.Columns.Add("CLOSE_ST_CODE", typeof(string));
            inDataTable.Columns.Add("NOTE", typeof(string));
            inDataTable.Columns.Add("USERID", typeof(string));
            inDataTable.Columns.Add("USER_IP", typeof(string));
            inDataTable.Columns.Add("USER_PC_NAME", typeof(string));

            return indataSet;
        }

        /// <summary>
        /// 제품ID 조회
        /// </summary>
        /// <returns></returns>
        public DataTable GetCOR_SEL_PRODUCTLIST_G()
        {
            DataTable inDataTable = new DataTable();
            inDataTable.Columns.Add("LANGID", typeof(string));
            inDataTable.Columns.Add("SHOPID", typeof(string));
            inDataTable.Columns.Add("ORG_CODE", typeof(string));
            inDataTable.Columns.Add("MULTI_ORG_CODE", typeof(string));
            inDataTable.Columns.Add("PRODID", typeof(string));
            inDataTable.Columns.Add("MULTI_PRODID", typeof(string));
            inDataTable.Columns.Add("SRCH_PRODID", typeof(string));
            inDataTable.Columns.Add("MODLID", typeof(string));
            inDataTable.Columns.Add("PRODTYPE", typeof(string));
            inDataTable.Columns.Add("MULTI_PRODTYPE", typeof(string));
            inDataTable.Columns.Add("PROD_CLASS1_CODE", typeof(string));
            inDataTable.Columns.Add("PROD_CLASS2_CODE", typeof(string));
            inDataTable.Columns.Add("PROD_CLASS3_CODE", typeof(string));
            inDataTable.Columns.Add("PROD_CLASS4_CODE", typeof(string));
            inDataTable.Columns.Add("PROD_CLASS5_CODE", typeof(string));

            return inDataTable;
        }

        /// <summary>
        /// 고객사 조회
        /// </summary>
        /// <returns></returns>
        public DataTable GetCOR_SEL_SHOP_CUSTOMER_CBO_G()
        {
            DataTable inDataTable = new DataTable();
            inDataTable.Columns.Add("LANGID", typeof(string));
            inDataTable.Columns.Add("SHOPID", typeof(string));
            inDataTable.Columns.Add("CUSTOMERID", typeof(string));

            return inDataTable;
        }

        /// <summary>
        /// 고객사 조회(TS)
        /// </summary>
        /// <returns></returns>
        public DataTable GetCOR_SEL_TS_VENDOR_CBO_G()
        {
            DataTable inDataTable = new DataTable();
            inDataTable.Columns.Add("LANGID", typeof(string));
            inDataTable.Columns.Add("ORG_CODE", typeof(string));
            inDataTable.Columns.Add("PROD_CLASS1_CODE", typeof(string));
            inDataTable.Columns.Add("PROD_CLASS2_CODE", typeof(string));
            inDataTable.Columns.Add("PROD_CLASS3_CODE", typeof(string));
            inDataTable.Columns.Add("PRODID", typeof(string));

            return inDataTable;
        }

        /// <summary>
        /// 모델 조회(TS)
        /// </summary>
        /// <returns></returns>
        public DataTable GetCOR_SEL_TS_MODEL_CBO_G()
        {
            DataTable inDataTable = new DataTable();
            inDataTable.Columns.Add("LANGID", typeof(string));
            inDataTable.Columns.Add("ORG_CODE", typeof(string));
            inDataTable.Columns.Add("PROD_CLASS1_CODE", typeof(string));
            inDataTable.Columns.Add("PROD_CLASS2_CODE", typeof(string));
            inDataTable.Columns.Add("PROD_CLASS3_CODE", typeof(string));
            inDataTable.Columns.Add("CUSTOMERID", typeof(string));

            return inDataTable;
        }

        /// <summary>
        /// 출하성적서 파일 조회(TS)
        /// </summary>
        /// <returns></returns>
        public DataTable GetCOR_SEL_SHIP_RPT_FRMT_CBO()
        {
            DataTable inDataTable = new DataTable();
            inDataTable.Columns.Add("AREAID", typeof(string));
            inDataTable.Columns.Add("PROD_CLASS1_CODE", typeof(string));
            inDataTable.Columns.Add("PROD_CLASS2_CODE", typeof(string));
            inDataTable.Columns.Add("PROD_CLASS3_CODE", typeof(string));
            inDataTable.Columns.Add("PRODID", typeof(string));
            inDataTable.Columns.Add("CUSTOMERID", typeof(string));

            return inDataTable;
        }


        /// <summary>
        /// 주문 상태 코드 조회
        /// </summary>
        /// <returns></returns>
        public DataTable GetCOR_SEL_ORDER_ST_CODE_CBO_G()
        {
            DataTable inDataTable = new DataTable();
            inDataTable.Columns.Add("LANGID", typeof(string));
            inDataTable.Columns.Add("ORDER_ST_CODE", typeof(string));

            return inDataTable;
        }

        //주문 현황 조회
        public DataTable GetCOR_SEL_ERP_ORDER_G()
        {
            DataTable inDataTable = new DataTable();
            inDataTable.Columns.Add("LANGID", typeof(string));
            inDataTable.Columns.Add("SHOPID", typeof(string));
            inDataTable.Columns.Add("ORG_CODE", typeof(string));
            inDataTable.Columns.Add("MULTI_ORG_CODE", typeof(string));
            inDataTable.Columns.Add("ERP_ORDER_NO", typeof(string));
            inDataTable.Columns.Add("ORDER_PRODID", typeof(string));
            inDataTable.Columns.Add("CUSTOMERID", typeof(string));
            inDataTable.Columns.Add("ORDER_ST_CODE", typeof(string));
            inDataTable.Columns.Add("PROD_LOTID", typeof(string));
            inDataTable.Columns.Add("LAYER_ID", typeof(string));
            inDataTable.Columns.Add("DUE_DATE_FR", typeof(DateTime));
            inDataTable.Columns.Add("DUE_DATE_TO", typeof(DateTime));

            return inDataTable;
        }

        //납기일변경 사유코드 조회
        public DataTable GetCOR_SEL_DUE_DATE_CHG_RSN_CODE()
        {
            DataTable inDataTable = new DataTable();
            inDataTable.Columns.Add("LANGID", typeof(string));
            inDataTable.Columns.Add("SHOPID", typeof(string));
            inDataTable.Columns.Add("ORG_CODE", typeof(string));
            inDataTable.Columns.Add("MULTI_ORG_CODE", typeof(string));
            inDataTable.Columns.Add("DUE_DATE_CHG_RSN_CODE", typeof(string));

            return inDataTable;
        }

        // 납기일변경 사유코드 처리
        public DataSet GetBR_COR_REG_CHG_DUE_DATE_G()
        {
            DataSet indataSet = new DataSet();

            DataTable inDataTable = indataSet.Tables.Add("IN_ERP_ORDER");
            inDataTable.Columns.Add("ORG_CODE", typeof(string));
            inDataTable.Columns.Add("ERP_ORDER_NO", typeof(string));
            inDataTable.Columns.Add("CHG_DUE_DATE", typeof(DateTime));
            inDataTable.Columns.Add("DUE_DATE_CHG_RSN_CODE", typeof(string));
            inDataTable.Columns.Add("NOTE", typeof(string));
            inDataTable.Columns.Add("USERID", typeof(string));

            return indataSet;
        }

        //W/O상태 ComboBox 조회
        public DataTable GetCOR_SEL_WOSTAT_CBO_G()
        {
            DataTable inDataTable = new DataTable();
            inDataTable.Columns.Add("LANGID", typeof(string));
            inDataTable.Columns.Add("WOSTAT", typeof(string));

            return inDataTable;
        }

        //W/O 분류ComboBox 조회
        public DataTable GetCOR_SEL_WOTYPE_CBO_G()
        {
            DataTable inDataTable = new DataTable();
            inDataTable.Columns.Add("LANGID", typeof(string));
            inDataTable.Columns.Add("WOTYPE", typeof(string));

            return inDataTable;
        }

        //작업장 ComboBox 조회
        public DataTable GetCOR_SEL_ERP_DEPT_CBO_G()
        {
            DataTable inDataTable = new DataTable();
            inDataTable.Columns.Add("LANGID", typeof(string));
            inDataTable.Columns.Add("ORG_CODE", typeof(string));
            inDataTable.Columns.Add("MULTI_ORG_CODE", typeof(string));
            inDataTable.Columns.Add("ERP_DEPT_CODE", typeof(string));

            return inDataTable;
        }

        //Workorder 수동 생성 사유 조회
        public DataTable GetCOR_SEL_MANUAL_WO_RSN_CODE_CBO_G()
        {
            DataTable inDataTable = new DataTable();
            inDataTable.Columns.Add("LANGID", typeof(string));
            inDataTable.Columns.Add("MANUAL_WO_RSN_CODE", typeof(string));

            return inDataTable;
        }

        //Workorder 현황 조회
        public DataTable GetCOR_SEL_WORKORDERLIST_G()
        {
            DataTable inDataTable = new DataTable();
            inDataTable.Columns.Add("LANGID", typeof(string));
            inDataTable.Columns.Add("SHOPID", typeof(string));
            inDataTable.Columns.Add("ORG_CODE", typeof(string));
            inDataTable.Columns.Add("MULTI_ORG_CODE", typeof(string));
            inDataTable.Columns.Add("WOID", typeof(string));
            inDataTable.Columns.Add("PRODID", typeof(string));
            inDataTable.Columns.Add("MULTI_PRODID", typeof(string));
            inDataTable.Columns.Add("PLANSTDTTM_FR", typeof(DateTime));
            inDataTable.Columns.Add("PLANSTDTTM_TO", typeof(DateTime));
            inDataTable.Columns.Add("WOSTAT", typeof(string));
            inDataTable.Columns.Add("WO_SRC_TP_CODE", typeof(string));
            inDataTable.Columns.Add("LOTID", typeof(string));
            inDataTable.Columns.Add("PROD_LOTID", typeof(string));

            return inDataTable;
        }

        //Workorder 예외 발생 이력 조회
        public DataTable GetCOR_SEL_WORKORDER_EXCEPT_G()
        {
            DataTable inDataTable = new DataTable();
            inDataTable.Columns.Add("LANGID", typeof(string));
            inDataTable.Columns.Add("SHOPID", typeof(string));
            inDataTable.Columns.Add("ORG_CODE", typeof(string));
            inDataTable.Columns.Add("MULTI_ORG_CODE", typeof(string));
            inDataTable.Columns.Add("WOID", typeof(string));
            inDataTable.Columns.Add("PRODID", typeof(string));
            inDataTable.Columns.Add("EXCEPTDTTM_FR", typeof(DateTime));
            inDataTable.Columns.Add("EXCEPTDTTM_TO", typeof(DateTime));
            inDataTable.Columns.Add("LOTID", typeof(string));
            inDataTable.Columns.Add("PROD_LOTID", typeof(string));

            return inDataTable;
        }

        // W/O 수동생성처리
        public DataSet GetBR_COR_REG_MANUAL_WORKORDER_G()
        {
            DataSet indataSet = new DataSet();

            DataTable inDataTable = indataSet.Tables.Add("IN_WORKORDER");
            inDataTable.Columns.Add("ORG_CODE", typeof(string));
            inDataTable.Columns.Add("WOID", typeof(string));
            inDataTable.Columns.Add("PRODID", typeof(string));
            inDataTable.Columns.Add("PLANQTY", typeof(Decimal));
            inDataTable.Columns.Add("PLANSTDTTM", typeof(DateTime));
            inDataTable.Columns.Add("PLANEDDTTM", typeof(DateTime));
            inDataTable.Columns.Add("CUSTOMERID", typeof(string));
            inDataTable.Columns.Add("WOTYPE", typeof(string));
            inDataTable.Columns.Add("DEPT_CODE", typeof(string));
            inDataTable.Columns.Add("ASSBY_CODE", typeof(string));
            inDataTable.Columns.Add("RSN_CODE", typeof(string));
            inDataTable.Columns.Add("NOTE", typeof(string));
            inDataTable.Columns.Add("USERID", typeof(string));

            return indataSet;
        }

        //실적유형 ComboBox 조회
        public DataTable GetCOR_SEL_ERP_TXN_TP_CODE_CBO_G()
        {
            DataTable inDataTable = new DataTable();
            inDataTable.Columns.Add("LANGID", typeof(string));
            inDataTable.Columns.Add("ERP_TXN_ERR_TP_CODE", typeof(string));

            return inDataTable;
        }

        //ERP 실적 처리 현황 조회
        public DataTable GetBR_CUS_SEL_ERP_TXN_LIST_QP1PM1()
        {
            DataTable inDataTable = new DataTable();
            inDataTable.Columns.Add("LANGID", typeof(string));
            inDataTable.Columns.Add("ORG_CODE", typeof(string));
            inDataTable.Columns.Add("ERP_WRK_DATE", typeof(DateTime));
            inDataTable.Columns.Add("PRODID", typeof(string));
            inDataTable.Columns.Add("MULTI_PRODID", typeof(string));
            inDataTable.Columns.Add("ERP_TXN_ERR_TP_CODE", typeof(string));
            inDataTable.Columns.Add("PROD_LOTID", typeof(string));
            inDataTable.Columns.Add("LOTID", typeof(string));

            return inDataTable;
        }

        //ERP 실적 처리 결과 조회
        public DataTable GetCOR_SEL_ERP_TXN_RSLT_G()
        {
            DataTable inDataTable = new DataTable();
            inDataTable.Columns.Add("LANGID", typeof(string));
            inDataTable.Columns.Add("ORG_CODE", typeof(string));
            inDataTable.Columns.Add("ERP_WRK_DATE", typeof(DateTime));
            inDataTable.Columns.Add("PROD_LOTID", typeof(string));
            inDataTable.Columns.Add("LOTID", typeof(string));

            return inDataTable;
        }

        #endregion

        #region 생산실행

        // Serial 발번
        public DataSet GetBR_COR_GET_CREATE_SERIAL_G()
        {
            DataSet indataSet = new DataSet();

            DataTable inDataTable = indataSet.Tables.Add("IN_DATA");
            inDataTable.Columns.Add("SITEID", typeof(string));
            inDataTable.Columns.Add("SHOPID", typeof(string));
            inDataTable.Columns.Add("LOTTYPE", typeof(DateTime));
            inDataTable.Columns.Add("SERIAL_NO_CODE", typeof(string));
            inDataTable.Columns.Add("PRODID", typeof(string));
            inDataTable.Columns.Add("SERIAL_NO", typeof(string));
            inDataTable.Columns.Add("NOTE", typeof(string));
            inDataTable.Columns.Add("USERID", typeof(string));

            DataTable inTs = indataSet.Tables.Add("IN_TS");
            inTs.Columns.Add("AREAID", typeof(string));
            inTs.Columns.Add("VENDERID", typeof(string));

            return indataSet;
        }

        /// <summary>
        /// 공정검사 --  Process Data 저장
        /// </summary>
        /// <returns></returns>
        public DataSet GetBR_EIF_GET_PROCESS_DATA_UI_G()
        {
            DataSet indataSet = new DataSet();
            DataTable inDataTable = indataSet.Tables.Add("IN_COMM");
            inDataTable.Columns.Add("SRCTYPE", typeof(string));
            inDataTable.Columns.Add("SHOPID", typeof(string));
            inDataTable.Columns.Add("EQPTID", typeof(string));
            inDataTable.Columns.Add("EVENTTIME", typeof(string));
            inDataTable.Columns.Add("LOTID", typeof(string));
            inDataTable.Columns.Add("USERID", typeof(string));
            inDataTable.Columns.Add("PROCID", typeof(string));

            DataTable inITEM = indataSet.Tables.Add("IN_ITEM");
            inITEM.Columns.Add("CLCTITEM", typeof(string));
            inITEM.Columns.Add("CLCTCNT", typeof(string));
            inITEM.Columns.Add("CLCTVAL01", typeof(string));
            inITEM.Columns.Add("CLCTVAL02", typeof(string));
            inITEM.Columns.Add("CLCTVAL03", typeof(string));
            inITEM.Columns.Add("CLCTVAL04", typeof(string));
            inITEM.Columns.Add("CLCTVAL05", typeof(string));
            inITEM.Columns.Add("CLCTVAL06", typeof(string));
            inITEM.Columns.Add("CLCTVAL07", typeof(string));
            inITEM.Columns.Add("CLCTVAL08", typeof(string));
            inITEM.Columns.Add("CLCTVAL09", typeof(string));
            inITEM.Columns.Add("CLCTVAL10", typeof(string));
            inITEM.Columns.Add("CLCTVAL11", typeof(string));
            inITEM.Columns.Add("CLCTVAL12", typeof(string));
            inITEM.Columns.Add("CLCTVAL13", typeof(string));
            inITEM.Columns.Add("CLCTVAL14", typeof(string));
            inITEM.Columns.Add("CLCTVAL15", typeof(string));

            DataTable inFILE = indataSet.Tables.Add("IN_FILE");
            inFILE.Columns.Add("FILEKEY", typeof(string));
            inFILE.Columns.Add("FILENAME", typeof(string));
            inFILE.Columns.Add("FILEPATH", typeof(string));
            inFILE.Columns.Add("FILE_CNTT", typeof(Byte[]));

            return indataSet;
        }

        /// <summary>
        /// 공정검사(TS) --  Process Data 저장
        /// </summary>
        /// <returns></returns>
        public DataSet GetBR_CUS_REG_WIPDATACOLLECT_G()
        {
            DataSet indataSet = new DataSet();
            DataTable inDataTable = indataSet.Tables.Add("IN_COMM");
            inDataTable.Columns.Add("SHOPID", typeof(string));
            inDataTable.Columns.Add("PROCID", typeof(string));
            inDataTable.Columns.Add("EQPTID", typeof(string));
            inDataTable.Columns.Add("WIPSEQ", typeof(string));
            //inDataTable.Columns.Add("CLCTSEQ", typeof(string));
            inDataTable.Columns.Add("LOTID", typeof(string));
            inDataTable.Columns.Add("USERID", typeof(string));

            DataTable inITEM = indataSet.Tables.Add("IN_ITEM");
            inITEM.Columns.Add("CLCTITEM", typeof(string));
            inITEM.Columns.Add("CLCTCNT", typeof(string));
            inITEM.Columns.Add("CLCTVAL01", typeof(string));
            inITEM.Columns.Add("CLCTVAL02", typeof(string));
            inITEM.Columns.Add("CLCTVAL03", typeof(string));
            inITEM.Columns.Add("CLCTVAL04", typeof(string));
            inITEM.Columns.Add("CLCTVAL05", typeof(string));
            inITEM.Columns.Add("CLCTVAL06", typeof(string));
            inITEM.Columns.Add("CLCTVAL07", typeof(string));
            inITEM.Columns.Add("CLCTVAL08", typeof(string));
            inITEM.Columns.Add("CLCTVAL09", typeof(string));
            inITEM.Columns.Add("CLCTVAL10", typeof(string));
            inITEM.Columns.Add("CLCTVAL11", typeof(string));
            inITEM.Columns.Add("CLCTVAL12", typeof(string));
            inITEM.Columns.Add("CLCTVAL13", typeof(string));
            inITEM.Columns.Add("CLCTVAL14", typeof(string));
            inITEM.Columns.Add("CLCTVAL15", typeof(string));

            return indataSet;
        }
        //PRODUCT GROUP 정보 조회
        public DataTable GetCOR_SEL_PRODUCTGROUP_CBO()
        {
            DataTable inDataTable = new DataTable();
            inDataTable.Columns.Add("LANGID", typeof(string));

            return inDataTable;
        }

        //제품별 CLASS 조회
        public DataTable GetCOR_SEL_PRODUCT_CLASS_CBO_G()
        {
            DataTable inDataTable = new DataTable();
            inDataTable.Columns.Add("LANGID", typeof(string));
            inDataTable.Columns.Add("ORG_CODE", typeof(string));
            inDataTable.Columns.Add("CMCDTYPE", typeof(string));

            return inDataTable;
        }

        //제품 분류에 따른 제품 조회
        public DataTable GetCOR_SEL_PRODUCT_BY_CLASS_G()
        {
            DataTable inDataTable = new DataTable();
            inDataTable.Columns.Add("LANGID", typeof(string));
            inDataTable.Columns.Add("SHOPID", typeof(string));
            inDataTable.Columns.Add("ORG_CODE", typeof(string));
            inDataTable.Columns.Add("PRODTYPE", typeof(string));
            inDataTable.Columns.Add("PROD_CLASS1_CODE", typeof(string));
            inDataTable.Columns.Add("PROD_CLASS2_CODE", typeof(string));
            inDataTable.Columns.Add("PROD_CLASS3_CODE", typeof(string));
            inDataTable.Columns.Add("PROD_CLASS4_CODE", typeof(string));

            return inDataTable;
        }

        //MODEL 정보 조회
        public DataTable GetCOR_SEL_PRODUCTMODEL_CBO_G()
        {
            DataTable inDataTable = new DataTable();
            inDataTable.Columns.Add("LANGID", typeof(string));
            inDataTable.Columns.Add("PDGRID", typeof(string));

            return inDataTable;
        }

        //PRODUCT 정보 조회
        public DataTable GetCOR_SEL_PRODUCT_CBO_G()
        {
            DataTable inDataTable = new DataTable();
            inDataTable.Columns.Add("LANGID", typeof(string));
            inDataTable.Columns.Add("MODELID", typeof(string));
            inDataTable.Columns.Add("PDGRID", typeof(string));
            inDataTable.Columns.Add("PRODTYPE", typeof(string));

            return inDataTable;
        }

        //PRODUCT + ROUTE 정보 조회
        public DataTable GetCOR_SEL_PRODUCT_ROUTE_CBO_G()
        {
            DataTable inDataTable = new DataTable();
            inDataTable.Columns.Add("LANGID", typeof(string));
            inDataTable.Columns.Add("MODELID", typeof(string));
            inDataTable.Columns.Add("PDGRID", typeof(string));
            inDataTable.Columns.Add("PRODTYPE", typeof(string));

            return inDataTable;
        }

        //ACTIVITYREASONGROUP BY PROCID 정보 조회
        public DataTable GetCOR_SEL_ACTIVITYREASONGROUP_BY_PROCID_CBO_G()
        {
            DataTable inDataTable = new DataTable();
            inDataTable.Columns.Add("LANGID", typeof(string));
            inDataTable.Columns.Add("ACTID", typeof(string));
            inDataTable.Columns.Add("PROCID", typeof(string));

            return inDataTable;
        }

        //ACTIVITYREASON BY PROCID 정보 조회
        public DataTable GetCOR_SEL_ACTIVITYREASON_BY_PROCID_CBO_G()
        {
            DataTable inDataTable = new DataTable();
            inDataTable.Columns.Add("LANGID", typeof(string));
            inDataTable.Columns.Add("ACTID", typeof(string));
            inDataTable.Columns.Add("PROCID", typeof(string));
            inDataTable.Columns.Add("RESNGRID", typeof(string));

            return inDataTable;
        }

        //PROCESS BY PRODUCT 정보 조회
        public DataTable GetCOR_SEL_PROCESS_SHOP_CBO_G()
        {
            DataTable inDataTable = new DataTable();
            inDataTable.Columns.Add("LANGID", typeof(string));
            inDataTable.Columns.Add("SHOPID", typeof(string));
            inDataTable.Columns.Add("PRODID", typeof(string));
            inDataTable.Columns.Add("PCSGID", typeof(string));

            return inDataTable;
        }

        //LAYER_NAME 정보 조회
        public DataTable GetCOR_SEL_LAYER_NAME_G()
        {
            DataTable inDataTable = new DataTable();
            inDataTable.Columns.Add("LANGID", typeof(string));
            inDataTable.Columns.Add("SHOPID", typeof(string));
            inDataTable.Columns.Add("ORG_CODE", typeof(string));
            inDataTable.Columns.Add("MULTI_ORG_CODE", typeof(string));

            return inDataTable;
        }

        //PROCESSEQUIPMENT 정보 조회
        public DataTable GetCOR_SEL_PROCESSEQUIPMENT_G()
        {
            DataTable inDataTable = new DataTable();
            inDataTable.Columns.Add("LANGID", typeof(string));
            inDataTable.Columns.Add("PROCID", typeof(string));
            inDataTable.Columns.Add("EQPTID", typeof(string));

            return inDataTable;
        }

        //LOT 정보조회
        public DataTable GetBR_EIF_GET_REAL_LOTID()
        {
            DataTable inDataTable = new DataTable();
            inDataTable.Columns.Add("SHOPID", typeof(string));
            inDataTable.Columns.Add("INPUT_VAL", typeof(string));
            inDataTable.Columns.Add("MULTI_YN", typeof(string));

            return inDataTable;
        }

        //LOT HOLD/RELEASE 정보조회
        public DataTable GetCOR_SEL_WIP_FOR_HOLD_G()
        {
            DataTable inDataTable = new DataTable();
            inDataTable.Columns.Add("LANGID", typeof(string));
            inDataTable.Columns.Add("SHOPID", typeof(string));
            inDataTable.Columns.Add("ORG_CODE", typeof(string));
            inDataTable.Columns.Add("MULTI_ORG_CODE", typeof(string));
            inDataTable.Columns.Add("PRODID", typeof(string));
            inDataTable.Columns.Add("PROCID", typeof(string));
            inDataTable.Columns.Add("LOTID", typeof(string));
            inDataTable.Columns.Add("MULTI_LOTID", typeof(string));
            inDataTable.Columns.Add("PROD_CLASS1_CODE", typeof(string));
            inDataTable.Columns.Add("PROD_CLASS2_CODE", typeof(string));
            inDataTable.Columns.Add("PROD_CLASS3_CODE", typeof(string));
            inDataTable.Columns.Add("AREAID", typeof(string));

            return inDataTable;
        }

        //LOT HOLD 처리
        public DataSet GetBR_COR_REG_HOLD_LOT_G()
        {
            DataSet indataSet = new DataSet();
            DataTable inDataTable = indataSet.Tables.Add("IN_LOT");
            inDataTable.Columns.Add("SRCTYPE", typeof(string));
            inDataTable.Columns.Add("LOTID", typeof(string));
            inDataTable.Columns.Add("HOLDNOTE", typeof(string));
            inDataTable.Columns.Add("USERID", typeof(string));
            inDataTable.Columns.Add("HOLDCODE", typeof(string));
            inDataTable.Columns.Add("IFMODE", typeof(string));

            return indataSet;
        }

        //LOT RELEASE 처리
        public DataSet GetBR_COR_REG_RELEASE_LOT_G()
        {
            DataSet indataSet = new DataSet();
            DataTable inDataTable = indataSet.Tables.Add("IN_LOT");
            inDataTable.Columns.Add("SRCTYPE", typeof(string));
            inDataTable.Columns.Add("LOTID", typeof(string));
            inDataTable.Columns.Add("UNHOLDNOTE", typeof(string));
            inDataTable.Columns.Add("USERID", typeof(string));
            inDataTable.Columns.Add("UNHOLDCODE", typeof(string));
            inDataTable.Columns.Add("IFMODE", typeof(string));
            inDataTable.Columns.Add("PROCID", typeof(string));

            return indataSet;
        }

        //LOT 불량이력조회
        public DataTable GetCOR_SEL_WIPREASONCOLLECT_HIS01_G()
        {
            DataTable inDataTable = new DataTable();
            inDataTable.Columns.Add("LANGID", typeof(string));
            inDataTable.Columns.Add("ORG_CODE", typeof(string));
            inDataTable.Columns.Add("WIPSDTTM_FR", typeof(DateTime));
            inDataTable.Columns.Add("WIPSDTTM_TO", typeof(DateTime));
            inDataTable.Columns.Add("PROD_CLASS1_CODE", typeof(string));
            inDataTable.Columns.Add("PROD_CLASS2_CODE", typeof(string));
            inDataTable.Columns.Add("PROD_CLASS3_CODE", typeof(string));
            inDataTable.Columns.Add("PROD_CLASS4_CODE", typeof(string));
            inDataTable.Columns.Add("PROD_CLASS5_CODE", typeof(string));
            inDataTable.Columns.Add("PRODID", typeof(string));
            inDataTable.Columns.Add("LOTID", typeof(string));
            inDataTable.Columns.Add("ACTID", typeof(string));
            inDataTable.Columns.Add("AREAID", typeof(string));

            return inDataTable;
        }

        //Stocker 정보 조회
        public DataTable GetCUR_CBO_STRK_ID()
        {
            DataTable inDataTable = new DataTable();
            inDataTable.Columns.Add("LANGID", typeof(string));
            inDataTable.Columns.Add("STKR_TYPE", typeof(string));

            return inDataTable;
        }

        //Slot 정보 조회
        public DataTable GetCUS_CBO_SLT_NO()
        {
            DataTable inDataTable = new DataTable();
            inDataTable.Columns.Add("LANGID", typeof(string));
            inDataTable.Columns.Add("STKR_TYPE", typeof(string));
            inDataTable.Columns.Add("STKR_ID", typeof(string));

            return inDataTable;
        }

        //RouteFlow으로 공정 조회
        public DataTable GetCOR_SEL_ROUTEFLOW_PATH_OPT()
        {
            DataTable inDataTable = new DataTable();
            inDataTable.Columns.Add("LANGID", typeof(string));
            inDataTable.Columns.Add("FLOWNORM", typeof(string));
            inDataTable.Columns.Add("ROUTID", typeof(string));
            inDataTable.Columns.Add("FLOWID", typeof(string));
            inDataTable.Columns.Add("PROCID", typeof(string));
            inDataTable.Columns.Add("FLOWID_TO", typeof(string));
            inDataTable.Columns.Add("PROCID_TO", typeof(string));
            inDataTable.Columns.Add("PATHTYPE", typeof(string));

            return inDataTable;
        }

        //RouteStep으로 공정 조회
        public DataTable GetCOR_SEL_ROUTESTEP_CBO()
        {
            DataTable inDataTable = new DataTable();
            inDataTable.Columns.Add("LANGID", typeof(string));
            inDataTable.Columns.Add("ROUTID", typeof(string));
            inDataTable.Columns.Add("FLOWID", typeof(string));

            return inDataTable;
        }


        //RoutePath에 해당하는 Next Flow/공정 정보
        public DataTable GetCOR_SEL_NEXT_PROC_ROUTEFLOW_G()
        {
            DataTable inDataTable = new DataTable();
            inDataTable.Columns.Add("LANGID", typeof(string));
            inDataTable.Columns.Add("ROUTID", typeof(string));
            inDataTable.Columns.Add("FLOWID", typeof(string));
            inDataTable.Columns.Add("PROCID", typeof(string));
            inDataTable.Columns.Add("NEXT_FLOWTYPE", typeof(string));
            inDataTable.Columns.Add("NEXT_FLOWID", typeof(string));

            return inDataTable;
        }

        //QP1 생산LOT 투입 대기 현황 조회
        public DataTable GetCUS_SEL_PROD_LOT_INPUT_WT_QP1()
        {
            DataTable inDataTable = new DataTable();
            inDataTable.Columns.Add("LANGID", typeof(string));
            inDataTable.Columns.Add("SHOPID", typeof(string));
            inDataTable.Columns.Add("ORG_CODE", typeof(string));
            inDataTable.Columns.Add("MULTI_ORG_CODE", typeof(string));
            inDataTable.Columns.Add("LOTID", typeof(string));
            inDataTable.Columns.Add("PRODID", typeof(string));
            inDataTable.Columns.Add("MLOTDTTM_STOCKED_FR", typeof(DateTime));
            inDataTable.Columns.Add("MLOTDTTM_STOCKED_TO", typeof(DateTime));
            inDataTable.Columns.Add("MLOT_TP_CODE", typeof(string));
            inDataTable.Columns.Add("SUPPLIER_LOTID", typeof(string));

            return inDataTable;
        }

        //LOT 지정 여부에 따른 WORKORDER 조회 (화면 조회용)
        public DataTable GetCOR_SEL_WOLIST_BY_LOT_ASSGN_FLAG_G()
        {
            DataTable inDataTable = new DataTable();
            inDataTable.Columns.Add("LANGID", typeof(string));
            inDataTable.Columns.Add("SHOPID", typeof(string));
            inDataTable.Columns.Add("ORG_CODE", typeof(string));
            inDataTable.Columns.Add("MULTI_ORG_CODE", typeof(string));
            inDataTable.Columns.Add("WOID", typeof(string));
            inDataTable.Columns.Add("PRODID", typeof(string));
            inDataTable.Columns.Add("PLANSTDTTM_FR", typeof(DateTime));
            inDataTable.Columns.Add("PLANSTDTTM_TO", typeof(DateTime));
            inDataTable.Columns.Add("WOSTAT", typeof(string));
            inDataTable.Columns.Add("WO_SRC_TP_CODE", typeof(string));
            inDataTable.Columns.Add("UNASSGN_ACT_FLAG", typeof(string));
            inDataTable.Columns.Add("ASSGN_ACT_FLAG", typeof(string));

            return inDataTable;
        }

        //QP1 생산LOT (전반/후반) 투입 
        public DataSet GetBR_CUS_REG_ASSIGN_PRODLOT_QP1()
        {
            DataSet indataSet = new DataSet();
            DataTable inDataTable = indataSet.Tables.Add("IN_DATA");

            inDataTable.Columns.Add("ORG_CODE", typeof(string));
            inDataTable.Columns.Add("WOID", typeof(string));
            inDataTable.Columns.Add("LOTID", typeof(string));
            inDataTable.Columns.Add("PROCID", typeof(string));
            inDataTable.Columns.Add("PROCID_TO", typeof(string));
            inDataTable.Columns.Add("IQC_PASS_FLAG", typeof(string));
            inDataTable.Columns.Add("SRCTYPE", typeof(string));
            inDataTable.Columns.Add("IFMODE", typeof(string));
            inDataTable.Columns.Add("RECIPEID", typeof(string));
            inDataTable.Columns.Add("CALDATE", typeof(DateTime));
            inDataTable.Columns.Add("SHIFT", typeof(string));
            inDataTable.Columns.Add("NOTE", typeof(string));
            inDataTable.Columns.Add("USERID", typeof(string));

            return indataSet;
        }

        //투입공정 
        public DataTable GetCUS_SEL_PROD_LOT_INPUT_PROCID_CBO_QP1()
        {
            DataTable inDataTable = new DataTable();
            inDataTable.Columns.Add("LANGID", typeof(string));
            inDataTable.Columns.Add("ORG_CODE", typeof(string));
            inDataTable.Columns.Add("LOTID", typeof(string));

            return inDataTable;
        }

        //LABEL TYPE
        public DataTable GetCOR_SEL_SFU_LABEL_TYPE_G()
        {
            DataTable inDataTable = new DataTable();
            inDataTable.Columns.Add("SHOPID", typeof(string));
            inDataTable.Columns.Add("LABEL_TYPE", typeof(string));

            return inDataTable;
        }

        //불량1분류
        public DataTable GetCOR_SEL_ACT_RSN_GR_1_BY_PROCID_CBO_G()
        {
            DataTable inDataTable = new DataTable();
            inDataTable.Columns.Add("LANGID", typeof(string));
            inDataTable.Columns.Add("ACTID", typeof(string));
            inDataTable.Columns.Add("PROCID", typeof(string));

            return inDataTable;
        }

        //불량2분류
        public DataTable GetCOR_SEL_ACT_RSN_GR_2_BY_PROCID_CBO_G()
        {
            DataTable inDataTable = new DataTable();
            inDataTable.Columns.Add("LANGID", typeof(string));
            inDataTable.Columns.Add("ACTID", typeof(string));
            inDataTable.Columns.Add("PROCID", typeof(string));
            inDataTable.Columns.Add("RESNGRID_PV", typeof(string));

            return inDataTable;
        }

        //비가동 사유코드
        public DataTable GetCOR_SEL_NOWORKCODE_G()
        {
            DataTable inDataTable = new DataTable();
            inDataTable.Columns.Add("LANGID", typeof(string));
            inDataTable.Columns.Add("SHOPID", typeof(string));
            inDataTable.Columns.Add("NOWORKID", typeof(string));
            inDataTable.Columns.Add("NOWORKID_PV", typeof(string));

            return inDataTable;
        }

        // 폐기처리분류(PM/PMP)
        public DataTable GetCUS_SEL_SCRAP_PRCS_GR_CODE_CBO_QP1PM1()
        {
            DataTable inDataTable = new DataTable();
            inDataTable.Columns.Add("LANGID", typeof(string));
            inDataTable.Columns.Add("SHOPID", typeof(string));
            inDataTable.Columns.Add("ORG_CODE", typeof(string));
            inDataTable.Columns.Add("MULTI_ORG_CODE", typeof(string));
            inDataTable.Columns.Add("SCRAP_PRCS_GR_CODE", typeof(string));

            return inDataTable;
        }

        // Recipe
        public DataTable GetCUS_SEL_SFU_RECIPE_CBO_G()
        {
            DataTable inDataTable = new DataTable();
            inDataTable.Columns.Add("LANGID", typeof(string));
            inDataTable.Columns.Add("EQPTID", typeof(string));

            return inDataTable;
        }

        // D/S자재
        public DataTable GetCUS_SEL_DS_MTRLID_CBO_QP1()
        {
            DataTable inDataTable = new DataTable();
            inDataTable.Columns.Add("LANGID", typeof(string));
            inDataTable.Columns.Add("ORG_CODE", typeof(string));
            inDataTable.Columns.Add("PRODID", typeof(string));

            return inDataTable;
        }

        // 공정 진행 LOT 현황 조회
        public DataTable GetCOR_SEL_WIP_BY_LOTDTTM_CR_G()
        {
            DataTable inDataTable = new DataTable();
            inDataTable.Columns.Add("LANGID", typeof(string));
            inDataTable.Columns.Add("SHOPID", typeof(string));
            inDataTable.Columns.Add("ORG_CODE", typeof(string));
            inDataTable.Columns.Add("MULTI_ORG_CODE", typeof(string));
            inDataTable.Columns.Add("PROD_LOTID", typeof(string));
            inDataTable.Columns.Add("LOTID", typeof(string));
            inDataTable.Columns.Add("SUPPLIER_LOTID", typeof(string));
            inDataTable.Columns.Add("PRODID", typeof(string));
            inDataTable.Columns.Add("PROCID", typeof(string));
            inDataTable.Columns.Add("LOTDTTM_CR_FR", typeof(DateTime));
            inDataTable.Columns.Add("LOTDTTM_CR_TO", typeof(DateTime));
            inDataTable.Columns.Add("CUSTOMERID", typeof(string));
            inDataTable.Columns.Add("ERP_ORDER_NO", typeof(string));
            inDataTable.Columns.Add("IQCPASS", typeof(string));

            return inDataTable;
        }

        // LOT 반송 IQC NG/OK 처리
        public DataSet GetBR_CUS_REG_IQC_FOR_RTN_MLOT_PM1()
        {
            DataSet indataSet = new DataSet();

            DataTable inDataTable = indataSet.Tables.Add("INDATA");
            //inDataTable.Columns.Add("SRCTYPE", typeof(string));
            inDataTable.Columns.Add("ORG_CODE", typeof(string));
            inDataTable.Columns.Add("LOTID", typeof(string));
            inDataTable.Columns.Add("IQCPASS", typeof(string));
            inDataTable.Columns.Add("RESNCODE", typeof(string));
            inDataTable.Columns.Add("PROCID_CAUSE", typeof(string));
            inDataTable.Columns.Add("FILEUNIQUEID", typeof(string));
            inDataTable.Columns.Add("NOTE", typeof(string));
            inDataTable.Columns.Add("USERID", typeof(string));
            //inDataTable.Columns.Add("IFMODE", typeof(string));

            //DataTable inUniqueTable = indataSet.Tables.Add("IN_UNIQUE");
            //inUniqueTable.Columns.Add("ORG_CODE", typeof(string));

            return indataSet;
        }

        // 반송 처리
        public DataSet GetBR_COR_REG_RETURN_MLOT_G()
        {
            DataSet indataSet = new DataSet();

            DataTable inDataTable = indataSet.Tables.Add("IN_DATA");
            inDataTable.Columns.Add("SRCTYPE", typeof(string));
            inDataTable.Columns.Add("LOTID", typeof(string));
            inDataTable.Columns.Add("RESNCODE", typeof(string));
            inDataTable.Columns.Add("PROCID_CAUSE", typeof(string));
            inDataTable.Columns.Add("NOTE", typeof(string));
            inDataTable.Columns.Add("USERID", typeof(string));
            inDataTable.Columns.Add("IFMODE", typeof(string));

            DataTable inUniqueTable = indataSet.Tables.Add("IN_UNIQUE");
            inUniqueTable.Columns.Add("ORG_CODE", typeof(string));

            return indataSet;
        }

        // 반송 처리(NEW)
        public DataSet GetBR_CUS_REG_RETURN_MLOT_PM1()
        {
            DataSet indataSet = new DataSet();

            DataTable inDataTable = indataSet.Tables.Add("INDATA");
            //inDataTable.Columns.Add("SRCTYPE", typeof(string));
            inDataTable.Columns.Add("ORG_CODE", typeof(string));
            inDataTable.Columns.Add("LOTID", typeof(string));
            inDataTable.Columns.Add("NOTE", typeof(string));
            inDataTable.Columns.Add("USERID", typeof(string));
            //inDataTable.Columns.Add("IFMODE", typeof(string));


            return indataSet;
        }

        // 재 반입 대기 LOT 현황 조회
        public DataTable GetCOR_SEL_WIP_FOR_RE_RCV_G()
        {
            DataTable inDataTable = new DataTable();
            inDataTable.Columns.Add("LANGID", typeof(string));
            inDataTable.Columns.Add("SHOPID", typeof(string));
            inDataTable.Columns.Add("ORG_CODE", typeof(string));
            inDataTable.Columns.Add("MULTI_ORG_CODE", typeof(string));
            inDataTable.Columns.Add("PROD_LOTID", typeof(string));
            inDataTable.Columns.Add("LOTID", typeof(string));
            inDataTable.Columns.Add("SUPPLIER_LOTID", typeof(string));
            inDataTable.Columns.Add("PRODID", typeof(string));
            inDataTable.Columns.Add("PROCID", typeof(string));
            inDataTable.Columns.Add("RETURN_DATE_FR", typeof(DateTime));
            inDataTable.Columns.Add("RETURN_DATE_TO", typeof(DateTime));
            inDataTable.Columns.Add("CUSTOMERID", typeof(string));
            inDataTable.Columns.Add("ERP_ORDER_NO", typeof(string));

            return inDataTable;
        }

        // 재 반입 처리
        public DataSet GetBR_COR_REG_RECEIVE_MLOT_G()
        {
            DataSet indataSet = new DataSet();

            DataTable inDataTable = indataSet.Tables.Add("INDATA");
            inDataTable.Columns.Add("SRCTYPE", typeof(string));
            inDataTable.Columns.Add("LOTID", typeof(string));
            inDataTable.Columns.Add("NOTE", typeof(string));
            inDataTable.Columns.Add("USERID", typeof(string));
            inDataTable.Columns.Add("IFMODE", typeof(string));

            DataTable inUniqueTable = indataSet.Tables.Add("IN_UNIQUE");
            inUniqueTable.Columns.Add("ORG_CODE", typeof(string));

            return indataSet;
        }

        // 반송LOT 투입 대기 현황 조회
        public DataTable GetCUS_SEL_WIP_FOR_RTNLOT_INPUT_QP1()
        {
            DataTable inDataTable = new DataTable();

            inDataTable.Columns.Add("LANGID", typeof(string));
            inDataTable.Columns.Add("SHOPID", typeof(string));
            inDataTable.Columns.Add("ORG_CODE", typeof(string));
            inDataTable.Columns.Add("MULTI_ORG_CODE", typeof(string));
            inDataTable.Columns.Add("PROD_LOTID", typeof(string));
            inDataTable.Columns.Add("LOTID", typeof(string));
            inDataTable.Columns.Add("SUPPLIER_LOTID", typeof(string));
            inDataTable.Columns.Add("PRODID", typeof(string));
            inDataTable.Columns.Add("RETURN_DATE_FR", typeof(DateTime));
            inDataTable.Columns.Add("RETURN_DATE_TO", typeof(DateTime));
            inDataTable.Columns.Add("PM_PROD_LOTID", typeof(string));
            inDataTable.Columns.Add("PM_LOTID", typeof(string));

            return inDataTable;
        }

        // 반송LOT 확정 대기 현황 조회
        public DataTable GetCUS_SEL_WIP_FOR_RTNLOT_CONFM_QP1()
        {
            DataTable inDataTable = new DataTable();

            inDataTable.Columns.Add("LANGID", typeof(string));
            inDataTable.Columns.Add("SHOPID", typeof(string));
            inDataTable.Columns.Add("ORG_CODE", typeof(string));
            inDataTable.Columns.Add("MULTI_ORG_CODE", typeof(string));
            inDataTable.Columns.Add("LOTID", typeof(string));
            inDataTable.Columns.Add("SUPPLIER_LOTID", typeof(string));
            inDataTable.Columns.Add("PRODID", typeof(string));
            inDataTable.Columns.Add("LOTDTTM_CR_FR", typeof(DateTime));
            inDataTable.Columns.Add("LOTDTTM_CR_TO", typeof(DateTime));
            inDataTable.Columns.Add("PM_LOTID", typeof(string));
            inDataTable.Columns.Add("MULTI_CONFM_FLAG", typeof(string));

            return inDataTable;
        }

        // LOT 출하 반품 처리
        public DataSet GetBR_COR_REG_RETURN_SHIP_LOT_G()
        {
            DataSet indataSet = new DataSet();

            DataTable inDataTable = indataSet.Tables.Add("IN_LOT");
            inDataTable.Columns.Add("SRCTYPE", typeof(string));
            inDataTable.Columns.Add("LOTID", typeof(string));
            inDataTable.Columns.Add("RETURN_QTY", typeof(Decimal));
            inDataTable.Columns.Add("WOID", typeof(string));
            inDataTable.Columns.Add("PROCID", typeof(string));
            inDataTable.Columns.Add("WIPNOTE", typeof(string));
            inDataTable.Columns.Add("USERID", typeof(string));
            inDataTable.Columns.Add("IFMODE", typeof(string));

            DataTable inUniqueTable = indataSet.Tables.Add("IN_UNIQUE");
            inUniqueTable.Columns.Add("ORG_CODE", typeof(string));
            inUniqueTable.Columns.Add("RESNCODE", typeof(string));
            inUniqueTable.Columns.Add("MLOT_TP_CODE", typeof(string));


            return indataSet;
        }

        // 반송LOT 확정 처리
        public DataSet GetBR_CUS_REG_RTN_CONFM_LOT_QP1()
        {
            DataSet indataSet = new DataSet();

            DataTable inDataTable = indataSet.Tables.Add("INDATA");
            inDataTable.Columns.Add("ORG_CODE", typeof(string));
            inDataTable.Columns.Add("LOTID", typeof(string));
            inDataTable.Columns.Add("CONFM_FLAG", typeof(string));
            inDataTable.Columns.Add("RESNCODE", typeof(string));
            inDataTable.Columns.Add("FILEUNIQUEID", typeof(string));
            inDataTable.Columns.Add("NOTE", typeof(string));
            inDataTable.Columns.Add("USERID", typeof(string));

            return indataSet;
        }

        //생산샐행 - 재공현황
        public DataTable GetCOR_SEL_WIP_BY_PROC_G()
        {
            DataTable inDataTable = new DataTable();
            inDataTable.Columns.Add("LANGID", typeof(string));
            inDataTable.Columns.Add("ORG_CODE", typeof(string));
            inDataTable.Columns.Add("LOTTYPE", typeof(string));
            inDataTable.Columns.Add("MULTI_MLOT_TP_CODE", typeof(string));
            inDataTable.Columns.Add("MULTI_GD_NAME", typeof(string));
            inDataTable.Columns.Add("MULTI_REP_PRODID", typeof(string));
            inDataTable.Columns.Add("LOTID", typeof(string));
            inDataTable.Columns.Add("PROD_LOTID", typeof(string));
            inDataTable.Columns.Add("SUPPLIER_LOTID", typeof(string));

            return inDataTable;
        }

        //생산샐행 - 재공상세현황
        public DataTable GetCOR_SEL_WIP_BY_PROC_DETAIL_G()
        {
            DataTable inDataTable = new DataTable();
            inDataTable.Columns.Add("LANGID", typeof(string));
            inDataTable.Columns.Add("ORG_CODE", typeof(string));
            inDataTable.Columns.Add("LOTTYPE", typeof(string));
            inDataTable.Columns.Add("PRODID", typeof(string));
            inDataTable.Columns.Add("PROD_LOTID", typeof(string));
            inDataTable.Columns.Add("LOTID", typeof(string));
            inDataTable.Columns.Add("PROCID", typeof(string));

            return inDataTable;
        }

        //생산샐행 - 재공상세현황
        public DataTable GetBR_COR_SEL_WIP_BY_PROC_DETAIL_G()
        {
            DataTable inDataTable = new DataTable();
            inDataTable.Columns.Add("LANGID", typeof(string));
            inDataTable.Columns.Add("ORG_CODE", typeof(string));
            inDataTable.Columns.Add("LOTTYPE", typeof(string));
            inDataTable.Columns.Add("MULTI_MLOT_TP_CODE", typeof(string));
            inDataTable.Columns.Add("MULTI_GD_NAME", typeof(string));
            inDataTable.Columns.Add("SUPPLIER_LOTID", typeof(string));
            inDataTable.Columns.Add("MULTI_REP_PRODID", typeof(string));
            inDataTable.Columns.Add("PROD_LOTID", typeof(string));
            inDataTable.Columns.Add("LOTID", typeof(string));
            inDataTable.Columns.Add("PROCID", typeof(string));

            return inDataTable;
        }

        //생산샐행 - 설계정보 조회
        public DataTable GetCUS_SEL_RPMS_DSGN_INFO_PM()
        {
            DataTable inDataTable = new DataTable();
            inDataTable.Columns.Add("LANGID", typeof(string));
            inDataTable.Columns.Add("ORG_CODE", typeof(string));
            inDataTable.Columns.Add("LOTID", typeof(string));
            inDataTable.Columns.Add("PROD_LOTID", typeof(string));
            inDataTable.Columns.Add("ERP_ORDER_NO", typeof(string));
            inDataTable.Columns.Add("PRODID", typeof(string));
            inDataTable.Columns.Add("FROM_DATE", typeof(DateTime));
            inDataTable.Columns.Add("TO_DATE", typeof(DateTime));

            return inDataTable;
        }

        //생산샐행 - 설계서 특이사항 이력조회
        public DataTable GetCOR_SEL_ORG_REMARKS_HIST_G()
        {
            DataTable inDataTable = new DataTable();
            inDataTable.Columns.Add("ORG_CODE", typeof(string));
            inDataTable.Columns.Add("REMARKS_TP_CODE", typeof(string));
            inDataTable.Columns.Add("LOTID", typeof(string));

            return inDataTable;
        }

        // 조직 코드별 특화 특이사항 등록
        public DataSet GetBR_CUS_REG_ORG_REMARKS_G()
        {
            DataSet indataSet = new DataSet();

            DataTable inDataTable = indataSet.Tables.Add("INDATA");
            inDataTable.Columns.Add("ORG_CODE", typeof(string));
            inDataTable.Columns.Add("REMARKS_TP_CODE", typeof(string));
            inDataTable.Columns.Add("LOTID", typeof(string));
            inDataTable.Columns.Add("REMARKS", typeof(string));
            inDataTable.Columns.Add("USERID", typeof(string));
            inDataTable.Columns.Add("FILEUNIQUEID", typeof(string));

            return indataSet;
        }

        //PM 상세공정 불러오기
        public DataTable GetBR_COR_SEL_TG_PROCID_CBO_G()
        {
            DataTable inDataTable = new DataTable();
            inDataTable.Columns.Add("ORG_CODE", typeof(string));
            inDataTable.Columns.Add("LOTID", typeof(string));
            inDataTable.Columns.Add("TO_PROCID", typeof(string));

            return inDataTable;
        }

        //PRODUCT BY ROUTE 불러오기
        public DataTable GetCOR_SEL_PRODUCTROUTE_CBO_G()
        {
            DataTable inDataTable = new DataTable();
            inDataTable.Columns.Add("LANGID", typeof(string));
            inDataTable.Columns.Add("PRODID", typeof(string));
            inDataTable.Columns.Add("SHOPID", typeof(string));

            return inDataTable;
        }

        //TOOL 불러오기
        public DataTable GetCUS_SEL_TOOL_CBO()
        {
            DataTable inDataTable = new DataTable();
            inDataTable.Columns.Add("LANGID", typeof(string));
            inDataTable.Columns.Add("TOOLITEMID", typeof(string));
            inDataTable.Columns.Add("TOOL_TP_CODE", typeof(string));
            inDataTable.Columns.Add("PROCID", typeof(string));
            inDataTable.Columns.Add("LOTID", typeof(string));

            return inDataTable;
        }

        //대표 제품ID Combobox 정보
        public DataTable GetCOR_SEL_REP_PRODID_CBO_G()
        {
            DataTable inDataTable = new DataTable();
            inDataTable.Columns.Add("LANGID", typeof(string));
            inDataTable.Columns.Add("SHOPID", typeof(string));
            inDataTable.Columns.Add("ORG_CODE", typeof(string));
            inDataTable.Columns.Add("MULTI_ORG_CODE", typeof(string));
            inDataTable.Columns.Add("PRODID", typeof(string));
            inDataTable.Columns.Add("MODLID", typeof(string));
            inDataTable.Columns.Add("PRODTYPE", typeof(string));
            inDataTable.Columns.Add("MULTI_PRODTYPE", typeof(string));
            inDataTable.Columns.Add("PROD_CLASS1_CODE", typeof(string));
            inDataTable.Columns.Add("PROD_CLASS2_CODE", typeof(string));
            inDataTable.Columns.Add("PROD_CLASS3_CODE", typeof(string));
            inDataTable.Columns.Add("PROD_CLASS4_CODE", typeof(string));
            inDataTable.Columns.Add("PROD_CLASS5_CODE", typeof(string));

            return inDataTable;
        }


        //TS - LOT생성
        public DataSet GetBR_CUS_REG_CREATE_LOT_TS_EACH()
        {
            DataSet indataSet = new DataSet();

            DataTable inDataTable = indataSet.Tables.Add("INDATA");
            inDataTable.Columns.Add("SRCTYPE", typeof(string));
            inDataTable.Columns.Add("LOT_INPUT_CNT", typeof(Decimal));
            inDataTable.Columns.Add("SITEID", typeof(string));
            inDataTable.Columns.Add("SHOPID", typeof(string));
            inDataTable.Columns.Add("ORG_CODE", typeof(string));
            inDataTable.Columns.Add("AREAID", typeof(string));
            inDataTable.Columns.Add("PRODID", typeof(string));
            inDataTable.Columns.Add("CUSTOMERID", typeof(string));
            inDataTable.Columns.Add("EQSGID", typeof(string));
            inDataTable.Columns.Add("BOMREV", typeof(string));
            inDataTable.Columns.Add("WOID", typeof(string));
            inDataTable.Columns.Add("WOTYPE", typeof(string));
            inDataTable.Columns.Add("LOTTYPE", typeof(string));
            inDataTable.Columns.Add("PROCID", typeof(string));
            inDataTable.Columns.Add("LOTQTY", typeof(Decimal));
            inDataTable.Columns.Add("UNITQTY", typeof(Decimal));
            inDataTable.Columns.Add("UNITID", typeof(string));
            inDataTable.Columns.Add("USERID", typeof(string));
            inDataTable.Columns.Add("IFMODE", typeof(string));
            inDataTable.Columns.Add("WIPNOTE", typeof(string));
            inDataTable.Columns.Add("INPUT_EXP_DATE", typeof(DateTime));
            inDataTable.Columns.Add("SPECIAL_LOTID_FLAG", typeof(string));

            return indataSet;
        }

        //TS - 생산LOT 투입 대기 현황 조회
        public DataTable GetCUS_SEL_LOT_INPUT_TS1()
        {
            DataTable inDataTable = new DataTable();
            inDataTable.Columns.Add("LANGID", typeof(string));
            inDataTable.Columns.Add("SHOPID", typeof(string));
            inDataTable.Columns.Add("ORG_CODE", typeof(string));
            inDataTable.Columns.Add("AREAID", typeof(string));
            inDataTable.Columns.Add("PROD_CLASS1_CODE", typeof(string));
            inDataTable.Columns.Add("PROD_CLASS2_CODE", typeof(string));
            inDataTable.Columns.Add("PROD_CLASS3_CODE", typeof(string));
            inDataTable.Columns.Add("PRODID", typeof(string));
            inDataTable.Columns.Add("LOTTYPE", typeof(string));
            inDataTable.Columns.Add("CUSTID", typeof(string));
            inDataTable.Columns.Add("INPUT_YN", typeof(string));



            return inDataTable;
        }

        //TS - 생산LOT 투입대기 WorkOrder 연계
        public DataTable GetCUS_SEL_WORKORDER_INPUT_TS1()
        {
            DataTable inDataTable = new DataTable();
            inDataTable.Columns.Add("PRODID", typeof(string));
            inDataTable.Columns.Add("AREAID", typeof(string));
            inDataTable.Columns.Add("LANGID", typeof(string));
            inDataTable.Columns.Add("SHOPID", typeof(string));
            inDataTable.Columns.Add("ORG_CODE", typeof(string));

            return inDataTable;
        }

        //TS - 투입보고
        public DataSet GetBR_COR_REG_INPUT_LOT_G()
        {
            DataSet indataSet = new DataSet();

            DataTable inDataTable = indataSet.Tables.Add("IN_LOT");
            inDataTable.Columns.Add("SRCTYPE", typeof(string));
            inDataTable.Columns.Add("LOTID", typeof(string));
            inDataTable.Columns.Add("LOTID_RT", typeof(string));
            inDataTable.Columns.Add("PRODID", typeof(string));
            inDataTable.Columns.Add("ROUTID", typeof(string));
            inDataTable.Columns.Add("FLOWID", typeof(string));
            inDataTable.Columns.Add("PROCID", typeof(string));
            inDataTable.Columns.Add("EQSGID", typeof(string));
            inDataTable.Columns.Add("HOTFLAG", typeof(string));
            inDataTable.Columns.Add("CSTID", typeof(string));
            inDataTable.Columns.Add("USERID", typeof(string));
            inDataTable.Columns.Add("IFMODE", typeof(string));
            inDataTable.Columns.Add("WOID", typeof(string));

            DataTable inSubDataTable = indataSet.Tables.Add("IN_SUBLOT");
            inSubDataTable.Columns.Add("LOTID", typeof(string));

            return indataSet;
        }

        //TS - 투입취소
        public DataSet GetBR_COR_REG_CANCEL_INPUT_LOT_G()
        {
            DataSet indataSet = new DataSet();

            DataTable inDataTable = indataSet.Tables.Add("IN_LOT");
            inDataTable.Columns.Add("SRCTYPE", typeof(string));
            inDataTable.Columns.Add("LOTID", typeof(string));
            inDataTable.Columns.Add("PROCID", typeof(string));
            inDataTable.Columns.Add("USERID", typeof(string));
            inDataTable.Columns.Add("IFMODE", typeof(string));

            DataTable inCstDataTable = indataSet.Tables.Add("IN_CST");
            inCstDataTable.Columns.Add("LOTID", typeof(string));
            inCstDataTable.Columns.Add("CSTID", typeof(string));

            return indataSet;
        }

        //TS - TS 용으로 LOT 생성을 위한 제품 정보 조회
        public DataTable GetCUS_SEL_PROD_LOT_CREATE_TS1()
        {
            DataTable inDataTable = new DataTable();
            inDataTable.Columns.Add("LANGID", typeof(string));
            inDataTable.Columns.Add("AREAID", typeof(string));
            inDataTable.Columns.Add("SHOPID", typeof(string));
            inDataTable.Columns.Add("ORG_CODE", typeof(string));
            inDataTable.Columns.Add("PRODID", typeof(string));
            inDataTable.Columns.Add("CUSTOMERID", typeof(string));

            return inDataTable;
        }

        //TS - LENGTH 로 입력 하여 UNIT 로 환원하여 보여줌
        public DataTable GetBR_CUS_GET_UNITQTY_TS()
        {
            DataTable inDataTable = new DataTable();
            inDataTable.Columns.Add("AREAID", typeof(string));
            inDataTable.Columns.Add("PRODID", typeof(string));
            inDataTable.Columns.Add("CUSTOMERID", typeof(string));
            inDataTable.Columns.Add("CURVALUE", typeof(decimal));

            return inDataTable;
        }

        //TS - INPUT PLAN 조회
        public DataTable GetCUS_SEL_INPUTPLAN_TS()
        {
            DataTable inDataTable = new DataTable();
            inDataTable.Columns.Add("LANGID", typeof(string));
            inDataTable.Columns.Add("SHOPID", typeof(string));
            inDataTable.Columns.Add("ORG_CODE", typeof(string));
            inDataTable.Columns.Add("AREAID", typeof(string));
            inDataTable.Columns.Add("PLN_TP_CODE", typeof(string));
            inDataTable.Columns.Add("PROD_CLASS1_CODE", typeof(string));
            inDataTable.Columns.Add("PROD_CLASS2_CODE", typeof(string));
            inDataTable.Columns.Add("PROD_CLASS3_CODE", typeof(string));
            inDataTable.Columns.Add("PRODID", typeof(string));
            inDataTable.Columns.Add("LOTTYPE", typeof(string));
            inDataTable.Columns.Add("CUSTOMERID", typeof(string));
            inDataTable.Columns.Add("PLANDATE_FR", typeof(DateTime));
            inDataTable.Columns.Add("PLANDATE_TO", typeof(DateTime));

            return inDataTable;
        }

        //TS - INPUT PLAN 입력
        public DataSet GetCUS_MRG_PRDTN_PLN_TS()
        {
            DataSet indataSet = new DataSet();

            DataTable inDataTable = indataSet.Tables.Add("INDATA");
            inDataTable.Columns.Add("PLN_TP_CODE", typeof(string));
            inDataTable.Columns.Add("SHOPID", typeof(string));
            inDataTable.Columns.Add("AREAID", typeof(string));
            inDataTable.Columns.Add("PRDTN_YMD", typeof(string));
            inDataTable.Columns.Add("PRODID", typeof(string));
            inDataTable.Columns.Add("CUSTOMERID", typeof(string));
            inDataTable.Columns.Add("PRDTN_QTY", typeof(decimal));
            inDataTable.Columns.Add("USERID", typeof(string));

            return indataSet;
        }

        //TS - 투입계획 기반 신규 LOT 조회
        public DataTable GetCUS_SEL_NEW_LOTLIST_TS()
        {
            DataTable inDataTable = new DataTable();
            inDataTable.Columns.Add("LANGID", typeof(string));
            inDataTable.Columns.Add("AREAID", typeof(string));
            inDataTable.Columns.Add("PROD_CLASS2_CODE", typeof(string));
            inDataTable.Columns.Add("PRDTN_YMD", typeof(string));

            return inDataTable;
        }

        //TS - INPUT PLAN LOT Creation   
        public DataSet GetBR_CUS_REG_CREATE_LOT_TS_PLAN_G()
        {
            DataSet indataSet = new DataSet();

            DataTable inDataTable = indataSet.Tables.Add("IN_LOT");
            inDataTable.Columns.Add("SRCTYPE", typeof(string));
            inDataTable.Columns.Add("SITEID", typeof(string));
            inDataTable.Columns.Add("SHOPID", typeof(string));
            inDataTable.Columns.Add("ORG_CODE", typeof(string));
            inDataTable.Columns.Add("AREAID", typeof(string));
            inDataTable.Columns.Add("PRODID", typeof(string));
            inDataTable.Columns.Add("EQSGID", typeof(string));
            inDataTable.Columns.Add("BOMREV", typeof(string));
            inDataTable.Columns.Add("WOID", typeof(string));
            inDataTable.Columns.Add("WOTYPE", typeof(string));
            inDataTable.Columns.Add("LOTTYPE", typeof(string));
            inDataTable.Columns.Add("PROCID", typeof(string));
            inDataTable.Columns.Add("LOTQTY", typeof(decimal));
            inDataTable.Columns.Add("UNITQTY", typeof(decimal));
            inDataTable.Columns.Add("UNITID", typeof(string));
            inDataTable.Columns.Add("USERID", typeof(string));
            inDataTable.Columns.Add("IFMODE", typeof(string));
            inDataTable.Columns.Add("LOTID", typeof(string));
            inDataTable.Columns.Add("CUSTOMERID", typeof(string));
            inDataTable.Columns.Add("WIPNOTE", typeof(string));

            DataTable inUniqueTable = indataSet.Tables.Add("IN_UNIQUE");
            inUniqueTable.Columns.Add("ORG_CODE", typeof(string));

            return indataSet;
        }

        //TS - 입고준수율 조회
        public DataTable GetCUS_SEL_COMPLIANCE_RATE_TS()
        {
            DataTable inDataTable = new DataTable();
            inDataTable.Columns.Add("LANGID", typeof(string));
            inDataTable.Columns.Add("SHOPID", typeof(string));
            inDataTable.Columns.Add("AREAID", typeof(string));
            inDataTable.Columns.Add("PLN_WEEK", typeof(string));
            inDataTable.Columns.Add("BASEDATE_FR", typeof(string));
            inDataTable.Columns.Add("BASEDATE_TO", typeof(string));

            return inDataTable;
        }

        //TS - 입고준수율 입력
        public DataSet GetCUS_MRG_COMPLIANCE_RATE_TS()
        {
            DataSet indataSet = new DataSet();

            DataTable inDataTable = indataSet.Tables.Add("INDATA");
            inDataTable.Columns.Add("PRODID", typeof(string));
            inDataTable.Columns.Add("CUSTOMERID", typeof(string));
            inDataTable.Columns.Add("PLN_YM", typeof(string));
            inDataTable.Columns.Add("PLN_WEEK", typeof(string));
            inDataTable.Columns.Add("REMARKS", typeof(string));
            inDataTable.Columns.Add("USERID", typeof(string));

            return indataSet;
        }

        //PRODID로 LENGTH <---> UNIT 환산 (TS)
        public DataTable GetCUS_SEL_PROD_LENGTHUNIT()
        {
            DataTable inDataTable = new DataTable();
            inDataTable.Columns.Add("PRODID", typeof(string));
            inDataTable.Columns.Add("CUSTID", typeof(string));
            inDataTable.Columns.Add("SLITTING_COUNT", typeof(string));
            inDataTable.Columns.Add("QTY", typeof(string));
            inDataTable.Columns.Add("UNIT", typeof(string));

            return inDataTable;
        }

        //TS - 출하계획 삭제
        public DataSet GetCUS_DEL_SHIPMENTPLAN_TS()
        {
            DataSet indataSet = new DataSet();
            DataTable inDataTable = indataSet.Tables.Add("IN_LOT");
            inDataTable.Columns.Add("PLN_TP_CODE", typeof(string));
            inDataTable.Columns.Add("SHOPID", typeof(string));
            inDataTable.Columns.Add("AREAID", typeof(string));
            inDataTable.Columns.Add("PRDTN_YMD", typeof(string));
            inDataTable.Columns.Add("PRODID", typeof(string));
            inDataTable.Columns.Add("CUSTOMERID", typeof(string));

            return indataSet;
        }

        //설비상태변경
        public DataSet GetBR_COR_REG_EIO_STATE_G()
        {
            DataSet indataSet = new DataSet();

            DataTable inEIOTable = indataSet.Tables.Add("IN_EIO");
            inEIOTable.Columns.Add("SRCTYPE", typeof(string));
            inEIOTable.Columns.Add("ACTID", typeof(string));
            inEIOTable.Columns.Add("EQPTID", typeof(string));
            inEIOTable.Columns.Add("EIOSTAT_TO", typeof(string));
            inEIOTable.Columns.Add("LOTCNT", typeof(Int32));
            inEIOTable.Columns.Add("EIONOTE", typeof(string));
            inEIOTable.Columns.Add("USERID", typeof(string));

            DataTable inNoworkTable = indataSet.Tables.Add("IN_NOWORK");
            inNoworkTable.Columns.Add("EQPTID", typeof(string));
            inNoworkTable.Columns.Add("NOWORKCD1", typeof(string));
            inNoworkTable.Columns.Add("NOWORKCD2", typeof(string));
            inNoworkTable.Columns.Add("NOWORKCD3", typeof(string));
            inNoworkTable.Columns.Add("NOWORK_YMDHMS", typeof(string));

            DataTable inUniqueTable = indataSet.Tables.Add("IN_UNIQUE");
            inUniqueTable.Columns.Add("ORG_CODE", typeof(string));


            return indataSet;
        }

        /// <summary>
        /// 코드조회 조회
        /// </summary>
        /// <returns></returns>
        public DataTable GetCOR_SEL_SHOP_COM_CODE()
        {
            DataTable inDataTable = new DataTable();
            inDataTable.Columns.Add("LANGID", typeof(string));
            inDataTable.Columns.Add("SHOPID", typeof(string));
            inDataTable.Columns.Add("CMCDTYPE", typeof(string));
            inDataTable.Columns.Add("CMCODE", typeof(string));

            return inDataTable;
        }
        #endregion

        #region  이상부적합 관련 Biz DataSet
        /// <summary>
        /// COR_SEL_LOTINFO_BY_LOTID_G - 이상부적합 LOT 체크
        /// </summary>
        /// <returns></returns>
        public DataTable COR_SEL_LOTINFO_BY_LOTID_G()
        {
            DataTable inDataTable = new DataTable();
            inDataTable.Columns.Add("LANGID", typeof(string));
            inDataTable.Columns.Add("LOTID", typeof(string));
            inDataTable.Columns.Add("PROCID", typeof(string));

            return inDataTable;
        }

        /// <summary>
        /// GetBR_CUS_REG_UNFIT_G - 이상부적합 발생등록
        /// </summary>
        /// <returns></returns>
        public DataSet GetBR_CUS_REG_UNFIT_G()
        {
            DataSet indataSet = new DataSet();

            DataTable inDataTable = indataSet.Tables.Add("IN_DATA");
            inDataTable.Columns.Add("UNFIT_ID", typeof(string));
            inDataTable.Columns.Add("CUDTYPE", typeof(string));
            inDataTable.Columns.Add("ORG_CODE", typeof(string));
            inDataTable.Columns.Add("THIS_STEP", typeof(string));
            inDataTable.Columns.Add("NEXT_STEP", typeof(string));
            inDataTable.Columns.Add("UNFIT_TP_CODE", typeof(string));
            inDataTable.Columns.Add("OCCUR_DATE", typeof(DateTime));
            inDataTable.Columns.Add("AREAID", typeof(string));
            inDataTable.Columns.Add("OCCUR_PROCID", typeof(string));
            inDataTable.Columns.Add("LOT_PRCS_VALUE", typeof(string));
            inDataTable.Columns.Add("HOLD_RSV_PROCID", typeof(string));
            inDataTable.Columns.Add("O_LOTID", typeof(string));
            inDataTable.Columns.Add("O_PRODID", typeof(string));
            inDataTable.Columns.Add("END_FLAG", typeof(string));
            inDataTable.Columns.Add("O_QTY", typeof(Decimal));
            inDataTable.Columns.Add("E_LOTID", typeof(string));
            inDataTable.Columns.Add("PRDTN_USERID", typeof(string));
            inDataTable.Columns.Add("QLTY_USERID", typeof(string));
            inDataTable.Columns.Add("O_NOTE", typeof(string));
            inDataTable.Columns.Add("INSUSER", typeof(string));
            inDataTable.Columns.Add("FILE_TP_CODE", typeof(string));       //파일발생구분


            DataTable inMailTable = indataSet.Tables.Add("IN_MAIL");
            inMailTable.Columns.Add("RCV_USERID", typeof(string));

            DataTable inFileTable = indataSet.Tables.Add("IN_FILE");
            inFileTable.Columns.Add("FILESEQ", typeof(decimal));
            inFileTable.Columns.Add("CRUD_FLAG", typeof(string));
            inFileTable.Columns.Add("UNIQUEID", typeof(string));
            inFileTable.Columns.Add("FILENAME", typeof(string));

            DataTable inHoldLotTable = indataSet.Tables.Add("IN_HOLDLOT");
            inHoldLotTable.Columns.Add("SRCTYPE", typeof(string));
            inHoldLotTable.Columns.Add("LOTID", typeof(string));
            inHoldLotTable.Columns.Add("HOLDCODE", typeof(string));
            inHoldLotTable.Columns.Add("IFMODE", typeof(string));

            return indataSet;
        }

        /// <summary>
        /// GetBR_CUS_REG_UNFIT_SCORE_G - 이상부적합 발생검토 및 평가 등록
        /// </summary>
        /// <returns></returns>
        public DataSet GetBR_CUS_REG_UNFIT_SCORE_G()
        {
            DataSet indataSet = new DataSet();

            DataTable inDataTable = indataSet.Tables.Add("INDATA");
            inDataTable.Columns.Add("LANGID", typeof(string));
            inDataTable.Columns.Add("UNFIT_ID", typeof(string));
            inDataTable.Columns.Add("THIS_STEP", typeof(string));
            inDataTable.Columns.Add("NEXT_STEP", typeof(string));
            inDataTable.Columns.Add("PRDTN_NOTE", typeof(string));
            inDataTable.Columns.Add("QLTY_NOTE", typeof(string));
            inDataTable.Columns.Add("TOT_SCORE", typeof(decimal));
            inDataTable.Columns.Add("REPORT_USERID", typeof(string));
            inDataTable.Columns.Add("USERID", typeof(string));

            DataTable inScore = indataSet.Tables.Add("IN_SCORE");
            inScore.Columns.Add("ITEM_NAME", typeof(string));
            inScore.Columns.Add("SEQ_NO", typeof(Int32));
            inScore.Columns.Add("SCORE", typeof(decimal));

            return indataSet;
        }

        /// <summary>
        /// GetBR_CUS_REG_UNFIT_RPT_G - 이상부적합 보고서 등록
        /// </summary>
        /// <returns></returns>
        public DataSet GetBR_CUS_REG_UNFIT_RPT_G()
        {
            DataSet indataSet = new DataSet();

            DataTable inDataTable = indataSet.Tables.Add("INDATA");
            inDataTable.Columns.Add("LANGID", typeof(string));
            inDataTable.Columns.Add("UNFIT_ID", typeof(string));
            inDataTable.Columns.Add("CUDTYPE", typeof(string));
            inDataTable.Columns.Add("THIS_STEP", typeof(string));
            inDataTable.Columns.Add("NEXT_STEP", typeof(string));

            inDataTable.Columns.Add("TEMP_PROD_PRCS_CNTT", typeof(string));
            inDataTable.Columns.Add("TEMP_OCCUR_CNTT", typeof(string));
            inDataTable.Columns.Add("TEMP_OUTFLW_CNTT", typeof(string));
            inDataTable.Columns.Add("BASE_OCCUR_CNTT", typeof(string));
            inDataTable.Columns.Add("BASE_OUTFLW_CNTT", typeof(string));
            inDataTable.Columns.Add("RPT_RCPT_USERID", typeof(string));
            inDataTable.Columns.Add("USERID", typeof(string));
            inDataTable.Columns.Add("FILE_TP_CODE", typeof(string));       //파일발생구분

            DataTable inFileTable = indataSet.Tables.Add("IN_FILE");
            inFileTable.Columns.Add("FILESEQ", typeof(decimal));
            inFileTable.Columns.Add("CRUD_FLAG", typeof(string));
            inFileTable.Columns.Add("UNIQUEID", typeof(string));
            inFileTable.Columns.Add("FILENAME", typeof(string));

            return indataSet;
        }

        /// <summary>
        /// GetBR_CUS_REG_UNFIT_RPT_RCPT_G - 이상부적합 보고서 접수
        /// </summary>
        /// <returns></returns>
        public DataSet GetBR_CUS_REG_UNFIT_RPT_RCPT_G()
        {
            DataSet indataSet = new DataSet();

            DataTable inDataTable = indataSet.Tables.Add("INDATA");
            inDataTable.Columns.Add("LANGID", typeof(string));
            inDataTable.Columns.Add("UNFIT_ID", typeof(string));
            inDataTable.Columns.Add("NEXT_STEP", typeof(string));
            inDataTable.Columns.Add("THIS_STEP", typeof(string));
            inDataTable.Columns.Add("EFF_EVAL_CNTT", typeof(string));
            inDataTable.Columns.Add("TEMP_OCCUR_CNTT", typeof(string));
            inDataTable.Columns.Add("LOT_PRCS_ST_CNTT", typeof(string));
            inDataTable.Columns.Add("REL_FLAG", typeof(string));
            inDataTable.Columns.Add("RCPT_FLAG", typeof(string));
            inDataTable.Columns.Add("REJ_CNTT", typeof(string));
            inDataTable.Columns.Add("USERID", typeof(string));
            inDataTable.Columns.Add("FILE_TP_CODE", typeof(string));       //파일발생구분

            DataTable inApprvTable = indataSet.Tables.Add("UNFIT_APPRV");
            inApprvTable.Columns.Add("CURR_STEP_VALUE", typeof(decimal));
            inApprvTable.Columns.Add("USERID", typeof(string));

            DataTable inFileTable = indataSet.Tables.Add("IN_FILE");
            inFileTable.Columns.Add("FILESEQ", typeof(decimal));
            inFileTable.Columns.Add("CRUD_FLAG", typeof(string));
            inFileTable.Columns.Add("UNIQUEID", typeof(string));
            inFileTable.Columns.Add("FILENAME", typeof(string));

            return indataSet;
        }

        /// <summary>
        /// GetBR_CUS_REG_UNFIT_APPRV_G - 이상부적합 승인
        /// </summary>
        /// <returns></returns>
        public DataSet GetBR_CUS_REG_UNFIT_APPRV_G()
        {
            DataSet indataSet = new DataSet();

            DataTable inDataTable = indataSet.Tables.Add("INDATA");
            inDataTable.Columns.Add("LANGID", typeof(string));
            inDataTable.Columns.Add("UNFIT_ID", typeof(string));
            inDataTable.Columns.Add("THIS_STEP", typeof(string));
            inDataTable.Columns.Add("NEXT_STEP", typeof(string));
            inDataTable.Columns.Add("APPRV_ST_CODE", typeof(string));
            inDataTable.Columns.Add("APPRV_COMMT", typeof(string));
            inDataTable.Columns.Add("INSUSER", typeof(string));

            return indataSet;
        }

        #endregion

        #region 모니터링
        //자재정보
        public DataTable GetCOR_SEL_MATERIALLOT_INFO_PM()
        {
            DataTable inDataTable = new DataTable();
            inDataTable.Columns.Add("LANGID", typeof(string));
            inDataTable.Columns.Add("LOTID", typeof(string));

            return inDataTable;
        }

        //주문정보
        public DataTable GetCOR_SEL_ORDER_DSGN_INFO_PM()
        {
            DataTable inDataTable = new DataTable();
            inDataTable.Columns.Add("LANGID", typeof(string));
            inDataTable.Columns.Add("LOTID", typeof(string));

            return inDataTable;
        }

        //LOT 이력
        public DataTable GetCUS_SEL_WIPACTHISTORY_LOTID()
        {
            DataTable inDataTable = new DataTable();
            inDataTable.Columns.Add("LANGID", typeof(string));
            inDataTable.Columns.Add("LOTID", typeof(string));

            return inDataTable;
        }
        #endregion

        #region ORG 공통 코드 Attribute 정보 불러오기
        public DataTable GetCOR_SEL_ORG_COM_CODE_ATTRIBUTE_CBO()
        {
            DataTable inDataTable = new DataTable();
            inDataTable.Columns.Add("LANGID", typeof(string));
            inDataTable.Columns.Add("SHOPID", typeof(string));
            inDataTable.Columns.Add("ORG_CODE", typeof(string));
            inDataTable.Columns.Add("MULTI_ORG_CODE", typeof(string));
            inDataTable.Columns.Add("CMCDTYPE", typeof(string));
            inDataTable.Columns.Add("CMCODE", typeof(string));
            inDataTable.Columns.Add("ATTRIBUTE1", typeof(string));
            inDataTable.Columns.Add("ATTRIBUTE2", typeof(string));
            inDataTable.Columns.Add("ATTRIBUTE3", typeof(string));
            inDataTable.Columns.Add("ATTRIBUTE4", typeof(string));
            inDataTable.Columns.Add("ATTRIBUTE5", typeof(string));
            inDataTable.Columns.Add("MULTI_ATTRIBUTE1", typeof(string));
            inDataTable.Columns.Add("MULTI_ATTRIBUTE2", typeof(string));
            inDataTable.Columns.Add("MULTI_ATTRIBUTE3", typeof(string));
            inDataTable.Columns.Add("MULTI_ATTRIBUTE4", typeof(string));
            inDataTable.Columns.Add("MULTI_ATTRIBUTE5", typeof(string));
            inDataTable.Columns.Add("J36_ATTRIBUTE1", typeof(string));

            return inDataTable;
        }
        #endregion
    }
}
