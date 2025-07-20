/************************************************************************************
  Created Date : 2021.04.06
       Creator : 정용석
   Description : 모듈 반송 요청
 ------------------------------------------------------------------------------------
  [Change History]
    2021.04.06  정용석 : Initial Created.
    2022.01.19  정용석 : 물류포장유형 컬럼 추가
 ************************************************************************************/
using C1.WPF;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using LGC.GMES.MES.PACK001.Class;
using System;
using System.Data;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace LGC.GMES.MES.PACK001
{
    public partial class PACK003_025_REQUEST : C1Window, IWorkArea
    {
        #region Member Variable Lists...
        private PACK003_025_REQUEST_DataHelper dataHelper = new PACK003_025_REQUEST_DataHelper();

        public IFrameOperation FrameOperation
        {
            get;
            set;
        }
        #endregion

        #region Declaration & Constructor
        public PACK003_025_REQUEST()
        {
            InitializeComponent();
        }
        #endregion

        #region Member Function Lists...
        // 초기화
        private void Initialize()
        {
            try
            {
                Util.pageAuth(this.grdRoot.Children.OfType<Button>().Where(x => !x.Tag.ToString().Contains("SEARCH")).ToList(), FrameOperation.AUTHORITY);
                PackCommon.SetC1ComboBox(this.dataHelper.GetPackEquipmentInfo(), this.cboPackEquipmentID, "SELECT");
                PackCommon.SetC1ComboBox(this.dataHelper.GetPackMixTypeCodeInfo(), this.cboPackMixTypeCode, "ALL");
                PackCommon.SetC1ComboBox(this.dataHelper.GetLogisPackType(), this.cboLogisPackType, "SELECT");

                object[] obj = C1WindowExtension.GetParameters(this);
                if (obj == null || obj.Length <= 0)
                {
                    return;
                }

                // 수동반송요청
                this.txtProdID.Text = obj[0].ToString();
                this.txtPrjtName.Text = obj[1].ToString();
                this.txtAssyAreaList.Tag = obj[2].ToString();
                this.txtElecLineList.Tag = obj[3].ToString();
                this.txtAssyLineList.Tag = obj[4].ToString();
                this.txtPackLineList.Tag = obj[5].ToString();
                this.txtAssyAreaList.Text = obj[6].ToString();
                this.txtElecLineList.Text = obj[7].ToString();
                this.txtAssyLineList.Text = obj[8].ToString();
                this.txtPackLineList.Text = obj[9].ToString();
                this.txtRequestWipTypeCode.Tag = obj[10].ToString();
                this.txtRequestWipTypeCode.Text = obj[11] == null ? string.Empty : obj[11].ToString();
                this.txtRequestQty.Tag = obj[12].ToString();
                this.txtRequestQty.Text = obj[12].ToString();
                this.txtRemark.Text = "Request Manual Transfer";
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        // Validation Check...
        private bool ValidationCheckTransferRequest()
        {
            // 작업자 선택 안하면 Interlock
            if (string.IsNullOrEmpty(this.ucPersonInfoPopUp.UserID))
            {
                Util.MessageValidation("SFU4591");  // 작업자를 입력하세요
                this.ucPersonInfoPopUp.Focus();
                return false;
            }

            // 포장기 선택 안하면 Interlock
            if (this.cboPackEquipmentID.SelectedValue == null || string.IsNullOrEmpty(this.cboPackEquipmentID.SelectedValue.ToString()))
            {
                Util.MessageValidation("9080");  // 설비를 선택하여 주십시오.
                return false;
            }

            // 선택한 컬럼의 수량이 0이면 Interlock
            if (Convert.ToInt32(this.txtRequestQty.Text.ToString()) <= 0)
            {
                Util.MessageValidation("SFU1683");  // 수량은 0보다 커야 합니다.
                return false;
            }

            // 처음에 넘어온 수량보다 크면 Interlock
            if (Convert.ToInt32(this.txtRequestQty.Text.ToString()) > Convert.ToInt32(this.txtRequestQty.Tag.ToString()))
            {
                Util.MessageValidation("SFU8395");  // 요청수량은 42보다 크면 안됩니다.
                return false;
            }

            // 물류포장유형 선택 안하면 Interlock
            if (this.cboLogisPackType.SelectedValue == null || string.IsNullOrEmpty(this.cboLogisPackType.SelectedValue.ToString()))
            {
                Util.MessageValidation("SFU4925", ObjectDic.Instance.GetObjectName("LOGIS_PACK_TYPE_NAME")); // 물류포장유형(을)를 선택하세요.
                this.cboLogisPackType.Focus();
                return false;
            }

            return true;
        }

        // 반송요청정보 저장
        private void SaveTransferRequest()
        {
            // Declarations...
            if (!this.ValidationCheckTransferRequest())
            {
                return;
            }

            try
            {
                Util.MessageConfirm("SFU8018", (result) =>
                {
                    if (result == MessageBoxResult.OK)
                    {
                        if (this.dataHelper.SaveTransferRequest(this.cboPackEquipmentID.SelectedValue.ToString()        // 포장기ID
                                                               , "M"                                                    // 자동수동 구분
                                                               , "C"                                                    // Current / Next 구분
                                                               , this.txtProdID.Text                                    // 제품 ID
                                                               , this.txtAssyAreaList.Tag.ToString()                    // 조립동
                                                               , this.txtAssyLineList.Tag.ToString()                    // 조립설비
                                                               , this.txtElecLineList.Tag.ToString()                    // 전극설비
                                                               , this.txtRequestQty.Text                                // 반송수량
                                                               , "CHK_ALLOW"                                            // Input Mix Check Method Code
                                                               , this.txtRequestWipTypeCode.Tag.ToString()              // 가용재고, Hold, NG
                                                               , this.txtPackLineList.Tag.ToString()                    // 팩라인
                                                               , this.cboPackMixTypeCode.SelectedValue.ToString()       // 포장혼입유형코드
                                                               , this.cboLogisPackType.SelectedValue.ToString()         // 물류포장유형코드
                                                               ))
                        {
                            this.Close();
                        }
                    }
                });
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        #endregion

        #region Event Lists...
        private void C1Window_Initialized(object sender, EventArgs e)
        {
        }

        private void C1Window_Loaded(object sender, RoutedEventArgs e)
        {
            this.Initialize();
            this.Loaded -= new RoutedEventHandler(this.C1Window_Loaded);
        }

        private void btnTransferRequest_Click(object sender, RoutedEventArgs e)
        {
            this.SaveTransferRequest();
        }

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
        #endregion
    }

    public class PACK003_025_REQUEST_DataHelper
    {
        #region Member Variable Lists...
        private DataTable dtPackEquipmentInfo = new DataTable();            // 포장기 설비정보
        private DataTable dtPackMixTypeCodeInfo = new DataTable();          // 포장혼입유형코드정보
        private DataTable dtRequestWipTypeCodeInfo = new DataTable();       // 반송재공상태코드정보
        private DataTable dtLogisPackType = new DataTable();                // 반송재공상태코드정보
        #endregion

        #region Constructor
        public PACK003_025_REQUEST_DataHelper()
        {
            this.GetPackEquipmentInfo(ref this.dtPackEquipmentInfo);
            this.GetCommonCodeInfo(ref this.dtPackMixTypeCodeInfo, "PACK_MIX_TYPE_CODE");
            this.GetCommonCodeInfo(ref this.dtRequestWipTypeCodeInfo, "REQ_WIP_TYPE_CODE");
            this.GetCommonCodeInfo(ref this.dtLogisPackType, "LOGIS_PACK_TYPE");
        }
        #endregion

        #region Member Function Lists...
        // BizRule 호출 - CommonCode
        private void GetCommonCodeInfo(ref DataTable dtReturn, string cmcdType)
        {
            try
            {
                string bizRuleName = "DA_BAS_SEL_COMMONCODE_ATTRIBUTES";
                DataTable dtRQSTDT = new DataTable("RQSTDT");
                DataTable dtRSLTDT = new DataTable("RSLTDT");

                dtRQSTDT.Columns.Add("LANGID", typeof(string));
                dtRQSTDT.Columns.Add("CMCDTYPE", typeof(string));
                dtRQSTDT.Columns.Add("ATTRIBUTE1", typeof(string));
                dtRQSTDT.Columns.Add("ATTRIBUTE2", typeof(string));
                dtRQSTDT.Columns.Add("ATTRIBUTE3", typeof(string));
                dtRQSTDT.Columns.Add("ATTRIBUTE4", typeof(string));
                dtRQSTDT.Columns.Add("ATTRIBUTE5", typeof(string));

                DataRow drRQSTDT = dtRQSTDT.NewRow();
                drRQSTDT["LANGID"] = LoginInfo.LANGID;
                drRQSTDT["CMCDTYPE"] = cmcdType;
                drRQSTDT["ATTRIBUTE1"] = DBNull.Value;
                drRQSTDT["ATTRIBUTE2"] = DBNull.Value;
                drRQSTDT["ATTRIBUTE3"] = DBNull.Value;
                drRQSTDT["ATTRIBUTE4"] = DBNull.Value;
                drRQSTDT["ATTRIBUTE5"] = DBNull.Value;
                dtRQSTDT.Rows.Add(drRQSTDT);

                dtRSLTDT = new ClientProxy().ExecuteServiceSync(bizRuleName, dtRQSTDT.TableName, dtRSLTDT.TableName, dtRQSTDT);
                if (CommonVerify.HasTableRow(dtRSLTDT))
                {
                    dtReturn = dtRSLTDT.Copy();
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        // BizRule 호출 - 포장기 (반송구분이 수동인것만 가져오기)
        private void GetPackEquipmentInfo(ref DataTable dtReturn)
        {
            try
            {
                string bizRuleName = "DA_BAS_SEL_LOGIS_PACK_EQPT_CBO";
                DataTable dtRQSTDT = new DataTable("RQSTDT");
                DataTable dtRSLTDT = new DataTable("RSLTDT");

                dtRQSTDT.Columns.Add("LANGID", typeof(string));
                dtRQSTDT.Columns.Add("AREAID", typeof(string));
                dtRQSTDT.Columns.Add("PACK_CELL_AUTO_LOGIS_FLAG", typeof(string));
                dtRQSTDT.Columns.Add("PACK_MEB_LINE_FLAG", typeof(string));
                dtRQSTDT.Columns.Add("PACK_BOX_LINE_FLAG", typeof(string));
                dtRQSTDT.Columns.Add("AUTO_TRF_FLAG", typeof(string));

                DataRow drRQSTDT = dtRQSTDT.NewRow();
                drRQSTDT["LANGID"] = LoginInfo.LANGID;
                drRQSTDT["AREAID"] = LoginInfo.CFG_AREA_ID;
                drRQSTDT["PACK_CELL_AUTO_LOGIS_FLAG"] = DBNull.Value;
                drRQSTDT["PACK_MEB_LINE_FLAG"] = DBNull.Value;
                drRQSTDT["PACK_BOX_LINE_FLAG"] = "Y";
                drRQSTDT["AUTO_TRF_FLAG"] = "N";
                dtRQSTDT.Rows.Add(drRQSTDT);

                dtRSLTDT = new ClientProxy().ExecuteServiceSync(bizRuleName, dtRQSTDT.TableName, dtRSLTDT.TableName, dtRQSTDT);
                if (CommonVerify.HasTableRow(dtRSLTDT))
                {
                    this.dtPackEquipmentInfo = dtRSLTDT.Copy();
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        // BizRule 호출 - 반송 요청
        public bool SaveTransferRequest(params object[] obj)
        {
            bool returnValue = true;
            string bizRuleName = "BR_PRD_REG_LOGIS_REQUEST";

            DataSet dsINDATA = new DataSet();
            DataSet dsOUTDATA = new DataSet();

            try
            {
                DataTable dtINDATA = new DataTable("INDATA");
                dtINDATA.Columns.Add("PACK_EQPTID", typeof(string));
                dtINDATA.Columns.Add("AUTO_MANUAL", typeof(string));
                dtINDATA.Columns.Add("CURR_NEXT", typeof(string));
                dtINDATA.Columns.Add("PRODID", typeof(string));
                dtINDATA.Columns.Add("AREAID", typeof(string));
                dtINDATA.Columns.Add("PKG_EQPTID", typeof(string));
                dtINDATA.Columns.Add("COATING_EQPTID", typeof(string));
                dtINDATA.Columns.Add("TRF_LOT_QTY", typeof(string));
                dtINDATA.Columns.Add("INPUT_MIX_CHK_MTHD_CODE", typeof(string));
                dtINDATA.Columns.Add("REQ_WIP_TYPE_CODE", typeof(string));
                dtINDATA.Columns.Add("PROD_PACK_EQSGID_LIST", typeof(string));
                dtINDATA.Columns.Add("PACK_MIX_TYPE_CODE", typeof(string));
                dtINDATA.Columns.Add("LOGIS_PACK_TYPE", typeof(string));

                DataRow drINDATA = dtINDATA.NewRow();
                drINDATA["PACK_EQPTID"] = obj[0].ToString();
                drINDATA["AUTO_MANUAL"] = obj[1].ToString();
                drINDATA["CURR_NEXT"] = obj[2].ToString();
                drINDATA["PRODID"] = obj[3].ToString();
                drINDATA["AREAID"] = "ALL";              // 조립동은 Pass
                drINDATA["PKG_EQPTID"] = string.IsNullOrEmpty(obj[5].ToString()) ? "ALL" : obj[5].ToString();
                drINDATA["COATING_EQPTID"] = string.IsNullOrEmpty(obj[6].ToString()) ? "ALL" : obj[6].ToString();
                drINDATA["TRF_LOT_QTY"] = obj[7].ToString();
                drINDATA["INPUT_MIX_CHK_MTHD_CODE"] = obj[8].ToString();      // 수동반송요청의 경우
                drINDATA["REQ_WIP_TYPE_CODE"] = obj[9].ToString();
                drINDATA["PROD_PACK_EQSGID_LIST"] = obj[10].ToString();
                drINDATA["PACK_MIX_TYPE_CODE"] = obj[11].ToString();
                drINDATA["LOGIS_PACK_TYPE"] = obj[12].ToString();

                dtINDATA.Rows.Add(drINDATA);

                dsINDATA.Tables.Add(dtINDATA);

                string inDataTableNameList = string.Join(",", dsINDATA.Tables.OfType<DataTable>().Select(x => x.TableName).ToList());
                dsOUTDATA = new ClientProxy().ExecuteServiceSync_Multi(bizRuleName, inDataTableNameList, null, dsINDATA);
                if (dsOUTDATA != null)
                {
                    Util.MessageInfo("SFU8357");        // 반송 요청이 저장되었습니다.
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                returnValue = false;
            }

            return returnValue;
        }

        // 포장기
        public DataTable GetPackEquipmentInfo()
        {
            if (!CommonVerify.HasTableRow(this.dtPackEquipmentInfo))
            {
                return null;
            }

            var query = this.dtPackEquipmentInfo.AsEnumerable().Select(x => new
            {
                CHK = false,
                PACK_EQPTID = x.Field<string>("CBO_CODE"),
                PACK_EQPTNAME = x.Field<string>("CBO_NAME")
            });

            return PackCommon.queryToDataTable(query.ToList());
        }

        // 포장혼입유형코드
        public DataTable GetPackMixTypeCodeInfo()
        {
            if (!CommonVerify.HasTableRow(this.dtPackMixTypeCodeInfo))
            {
                return null;
            }

            var query = this.dtPackMixTypeCodeInfo.AsEnumerable().Select(x => new
            {
                CHK = false,
                PACK_MIX_TYPE_CODE = x.Field<string>("CBO_CODE"),
                PACK_MIX_TYPE_CODE_NAME = x.Field<string>("CBO_NAME")
            });

            return PackCommon.queryToDataTable(query.ToList());
        }

        // 물류포장유형코드
        public DataTable GetLogisPackType()
        {
            if (!CommonVerify.HasTableRow(this.dtLogisPackType))
            {
                return null;
            }

            var query = this.dtLogisPackType.AsEnumerable().Select(x => new
            {
                CHK = false,
                LOGIS_PACK_TYPE_CODE = x.Field<string>("CBO_CODE"),
                LOGIS_PACK_TYPE_NAME = x.Field<string>("CBO_NAME")
            });

            return PackCommon.queryToDataTable(query.ToList());
        }
        #endregion
    }
}
