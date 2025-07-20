/*************************************************************************************
 Created Date : 2023.11.01
      Creator : 조영대
   Decription : BizActor 관련 - MMD BizActor 관리 파일 복사
                (04. Reference Files/BizActorInfoAgent.dll 참조)
--------------------------------------------------------------------------------------
 [Change History]
  2023.11.01  조영대   : Init. Copy by MMD "BizActorInfo.cs"
**************************************************************************************/

using System;
using System.Data;
using System.Linq;
using BizActorPlatform.Agent;


namespace LGC.GMES.MES.CMM001.Class
{
    public class BizActorInfo
    {
        public bool BIzValidation(string bizIP, string bizIndex, string bizName)
        {
            BizActorInfoAgent BizTestInfo = new BizActorInfoAgent(bizIP, bizIndex);

            DataSet dsSVCList = BizTestInfo.GetServiceInfo("bizactor_svclistex", null);

            DataTable dt = dsSVCList.Tables[0];

            //dsSVCList 구조
            //=======================================================================================================
            // | LAYER | GRP_NAME           | COM_ID       | ID              | STMP                        | IS_SVC |
            //=======================================================================================================
            // | BR    | Activity.Equipment | BR.EIOSTATE  | BR_ACT_INS_EIO  | 2016 - 10 - 18 오후 2:06:21 |  N     |

            int rowCount = dt.Select(string.Format("ID = '{0}'", bizName)).Count();

            if (rowCount > 0)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public bool BizLayer(string sBizType, string bizIP, string bizIndex, string sBizName)
        {
            DataView _dvBiz = new DataView();
            BizActorInfoAgent BizTestInfo = new BizActorInfoAgent(bizIP, bizIndex);

            DataSet dsSVCList = BizTestInfo.GetServiceInfo("bizactor_svclistex", null);

            //dsSVCList 구조
            //=======================================================================================================
            // | LAYER | GRP_NAME           | COM_ID       | ID              | STMP                        | IS_SVC |
            //=======================================================================================================
            // | BR    | Activity.Equipment | BR.EIOSTATE  | BR_ACT_INS_EIO  | 2016 - 10 - 18 오후 2:06:21 |  N     |

            DataTable dt = dsSVCList.Tables[0];

            int rowCount = dt.Select(string.Format("LAYER = '{0}' AND ID = '{1}'", sBizType , sBizName)).Count();

            if (rowCount > 0)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public string GetBizDesc(string bizIP, string bizIndex, string bizName)
        {
            string svcDesc = string.Empty;

            try
            {
                DataSet dsInput = new DataSet();
                DataTable dtInput = new DataTable();
                dtInput.TableName = "RQST";

                dtInput.Columns.Add("SVC_ID", typeof(string));

               DataRow drInput = dtInput.NewRow();
                drInput["SVC_ID"] = bizName;
                dtInput.Rows.Add(drInput);

                dsInput.Tables.Add(dtInput);

                BizActorInfoAgent BizTestInfo = new BizActorInfoAgent(bizIP, bizIndex);
                                
                DataSet dsdsSVCInfo = BizTestInfo.GetServiceInfo("bizactor_svcinfo", dsInput);

                //dsdsSVCInfo 구조
                //==================================================================================================================================================================
                // | SVC_ID                              | SVC_POST | SVC_RSLT            | SVC_NAME                        | SVC_DESC
                //==================================================================================================================================================================
                // | BR_PRD_GET_BOX_INFO_FOR_CANCEL_SHIP | INDATA   | OUTDATA,OUTDATA_RCV | 포장 출고 취소 Pallet 정보 조회 | 포장 출고 취소 Pallet 정보 조회
                //                                                                                                            [누락사항]
                //                                                                                                            1. 라벨발행 여부
                //
                //                                                                                                            [INPUT 사항]
                //                                                                                                            BOXID는 1건만 입력 가능
                //                                                                                                            ===============================================================
                //                                                                                                            개발일자 버젼      처리자 CSR ID 내용 
                //                                                                                                            2016.00.00    0.1                        SI 최초생성
                //                                                                                                            2021.01.22    0.2        김정균     115741        49번 스텝과 46번 스텝 위치 변경 | C20201116-000339
                //                                                                                                            2021.01.28    0.3        염규봉 SI             출고ID Check 시 Shipping된 Pallet 만 Return & Step 8,48,50 ERROR dataset 변경 INDATA ->VAR_PLT_TMP(Step 8),VAR_PLT(Step 48, Step 50)
                //                                                                                                            2021.03.29    0.4        염규봉     164087        BOX_RCV_ISS_STAT_CODE의 END_RECEIVE상태 일 경우 ERROR 발생(STEP 61,62),  버전 0.3 에서 출고ID Check 시 Shipping된 Pallet 만 Return 부분 원복

                DataTable dt = dsdsSVCInfo.Tables["BIZACTOR_SVCINFO"];

                DataRow tdr = dt.Rows[0];
                svcDesc = tdr["SVC_DESC"].ToString();
            }
            catch (Exception ex)
            {
                svcDesc = ex.Message;
            }

            return svcDesc;
        }

        public string GetBizVersion(string strBizDesc)
        {
            string sLasrBizVer = string.Empty;

            string[] lineStrings = strBizDesc.Split(new char[1] { '\n' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (string lineStr in lineStrings)
            {
                string[] tokens = lineStr.Replace("\r", "").Replace("\n", "").Split(new char[1] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                bool isNext = false;
                foreach(string token in tokens)
                {
                    if (isNext && token.Contains("."))
                    {
                        sLasrBizVer = token;
                        isNext = false;
                        continue;
                    }

                    DateTime dtParse = DateTime.Now;
                    if (DateTime.TryParse(token, out dtParse)) isNext = true;
                }
                
            }

            return sLasrBizVer;
        }

        public string BizState(string sBizType, string bizIP, string bizIndex, string sBizName)
        {
            string sBizStat = string.Empty;

            try
            {
                DataSet dsInput = new DataSet();
                DataTable dtInput = new DataTable();
                dtInput.TableName = "RQST";

                dtInput.Columns.Add("LAYER", typeof(string));
                dtInput.Columns.Add("ID", typeof(string));

                DataRow drInput = dtInput.NewRow();
                drInput["LAYER"] = sBizType;
                drInput["ID"] = sBizName;

                BizActorInfoAgent BizTestInfo = new BizActorInfoAgent(bizIP, bizIndex);

                DataSet dsSVCInfo = BizTestInfo.GetServiceInfo("bizactor_svclistex", null);

                //dsSVCList 구조
                //=======================================================================================================
                // | LAYER | GRP_NAME           | COM_ID       | ID              | STMP                        | IS_SVC |
                //=======================================================================================================
                // | BR    | Activity.Equipment | BR.EIOSTATE  | BR_ACT_INS_EIO  | 2016 - 10 - 18 오후 2:06:21 |  N     |

                DataTable dt = dsSVCInfo.Tables[0];
                dt = dt.Select(string.Format("LAYER = '{0}' AND ID = '{1}'", sBizType, sBizName)).CopyToDataTable();
                
                DataRow tdr = dt.Rows[0];
                sBizStat = tdr["IS_SVC"].ToString();
            }
            catch (Exception ex)
            {
                sBizStat = ex.Message;
            }

            return sBizStat;
        }
    }
}
