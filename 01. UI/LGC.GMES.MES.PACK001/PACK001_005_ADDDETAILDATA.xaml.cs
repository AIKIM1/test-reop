/*************************************************************************************
 Created Date : 2016.08.01
      Creator : Jeong Hyeon Sik
  Description : Pack Lot이력 - 상세데이터 입력 팝업
--------------------------------------------------------------------------------------
 [Change History]
  2016.08.01  Jeong Hyeon Sik : Initial Created.
  2019.12.11  손우석          : SM CMI Pack 메시지 다국어 처리 요청
  2020.01.20  손우석          : CSR ID 6935 Inspection Data history tap 상 Operator column 추가 요청 건 [요청번호 : C20191121-000303]
  2020.02.28  손우석          : SM 추가 버튼 클릭시 AREAID 파라미터 추가
  2024.07.22  정용석          : [E20240531-001870] 상단 설비콤보에 EQPTLEVEL이 UNIT인것도 나오도록 추가
**************************************************************************************/
using C1.WPF;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using LGC.GMES.MES.PACK001.Class;
using System;
using System.Collections.Generic;
using System.Data;
using System.Windows;
using System.Windows.Controls;

namespace LGC.GMES.MES.PACK001
{
    public partial class PACK001_005_ADDDETAILDATA : C1Window, IWorkArea
    {
        #region Member Variable List
        private string equipmentSegmentID = string.Empty;
        #endregion

        #region Property
        public IFrameOperation FrameOperation
        {
            get;
            set;
        }
        #endregion

        #region Constructor
        public PACK001_005_ADDDETAILDATA()
        {
            InitializeComponent();
        }
        #endregion

        #region Member Funtion Lists...
        // Initiallize
        private void Initialize()
        {
            try
            {
                List<Button> listAuth = new List<Button>();
                listAuth.Add(this.btnOK);
                Util.pageAuth(listAuth, FrameOperation.AUTHORITY);

                this.equipmentSegmentID = LoginInfo.CFG_EQSG_ID;
                object[] arrObject = C1WindowExtension.GetParameters(this);
                if (arrObject != null)
                {
                    DataTable dt = arrObject[0] as DataTable;

                    if (dt.Rows.Count > 0)
                    {
                        this.txtLOTID.Text = Util.NVC(dt.Rows[0]["LOTID"]);
                        this.txtProductID.Text = Util.NVC(dt.Rows[0]["PRODID"]);
                        this.equipmentSegmentID = Util.NVC(dt.Rows[0]["EQSGID"]);
                    }
                }

                PackCommon.SetC1ComboBox(this.GetProcessInfo(this.equipmentSegmentID), this.cboProcess, true);
                if (!string.IsNullOrEmpty(LoginInfo.CFG_PROC_ID))
                {
                    this.cboProcess.SelectedValue = LoginInfo.CFG_PROC_ID;
                    if (this.cboProcess.SelectedIndex < 0)
                    {
                        this.cboProcess.SelectedIndex = 0;
                    }
                }
                else
                {
                    this.cboProcess.SelectedIndex = 0;
                }

                GetWIPDataCollectInfo();
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        // 초기화버튼 눌렀을 때
        private void ClearQualityItemValue()
        {
            if (this.dgQualityItem.Rows.Count > 0)
            {
                for (int i = 0; i < this.dgQualityItem.Rows.Count; i++)
                {
                    DataTableConverter.SetValue(this.dgQualityItem.Rows[i].DataItem, "CLCTVAL01", "");
                }
            }
        }

        // 저장버튼 눌렀을 때
        private void SaveProcess()
        {
            try
            {
                bool validationCheck = false;
                for (int i = 0; i < this.dgQualityItem.Rows.Count; i++)
                {
                    if ((Util.NVC(DataTableConverter.GetValue(this.dgQualityItem.Rows[i].DataItem, "CLCTVAL01")).Length > 0))
                    {
                        if (!(Util.NVC(DataTableConverter.GetValue(this.dgQualityItem.Rows[i].DataItem, "CLCTVAL01")).Equals("")))
                        {
                            validationCheck = true;
                            break;
                        }
                    }
                }

                if (validationCheck)
                {
                    LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("검사 이력을 추가 합니다."), null, "Info", MessageBoxButton.OK, MessageBoxIcon.None, (result) =>
                    {
                        if (result == MessageBoxResult.OK)
                        {
                            this.SaveWipDataCollect();
                            this.GetWIPDataCollectInfo();
                        }
                    });
                }
                else
                {
                    Util.Alert("SFU2052");
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        // 순서도 호출 : 공정정보
        private DataTable GetProcessInfo(string equipmentSegmentID)
        {
            string bizRuleName = "DA_BAS_SEL_PROCESS_CBO";
            DataTable dtRQSTDT = new DataTable("RQSTDT");
            DataTable dtRSLTDT = new DataTable("RSLTDT");

            try
            {
                dtRQSTDT.Columns.Add("LANGID", typeof(string));
                dtRQSTDT.Columns.Add("EQSGID", typeof(string));

                DataRow drRQSTDT = dtRQSTDT.NewRow();
                drRQSTDT["LANGID"] = LoginInfo.LANGID;
                drRQSTDT["EQSGID"] = equipmentSegmentID;
                dtRQSTDT.Rows.Add(drRQSTDT);
                dtRSLTDT = new ClientProxy().ExecuteServiceSync(bizRuleName, dtRQSTDT.TableName, dtRSLTDT.TableName, dtRQSTDT);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }

            return dtRSLTDT;
        }

        // 순서도 호출 : 설비정보
        private DataTable GetEquipmentInfo(string equipmentSegmentID, string processID)
        {
            string bizRuleName = "DA_BAS_SEL_EQUIPMENT_CBO";
            DataTable dtRQSTDT = new DataTable("RQSTDT");
            DataTable dtRSLTDT = new DataTable("RSLTDT");

            try
            {
                dtRQSTDT.Columns.Add("LANGID", typeof(string));
                dtRQSTDT.Columns.Add("EQSGID", typeof(string));
                dtRQSTDT.Columns.Add("PROCID", typeof(string));
                dtRQSTDT.Columns.Add("COATER_EQPT_TYPE_CODE", typeof(string));
                dtRQSTDT.Columns.Add("EQPT_RSLT_CODE", typeof(string));
                dtRQSTDT.Columns.Add("EQGRID", typeof(string));
                dtRQSTDT.Columns.Add("CONTAIN_UNIT_FLAG", typeof(string));

                DataRow drRQSTDT = dtRQSTDT.NewRow();
                drRQSTDT["LANGID"] = LoginInfo.LANGID;
                drRQSTDT["EQSGID"] = equipmentSegmentID;
                drRQSTDT["PROCID"] = processID;
                drRQSTDT["COATER_EQPT_TYPE_CODE"] = null;
                drRQSTDT["EQPT_RSLT_CODE"] = null;
                drRQSTDT["EQGRID"] = null;
                drRQSTDT["CONTAIN_UNIT_FLAG"] = "Y";        // [E20240531-001870] 상단 설비콤보에 EQPTLEVEL이 UNIT인것도 나오도록 추가
                dtRQSTDT.Rows.Add(drRQSTDT);
                dtRSLTDT = new ClientProxy().ExecuteServiceSync(bizRuleName, dtRQSTDT.TableName, dtRSLTDT.TableName, dtRQSTDT);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }

            return dtRSLTDT;
        }

        // 순서도 호출 : 설비데이터 이력 조회
        private void GetWIPDataCollectInfo()
        {
            if (string.IsNullOrEmpty(this.txtLOTID.Text.Trim()))
            {
                return;
            }
            try
            {
                if (txtLOTID.Text.Length > 0)
                {
                    string bizRuleName = "DA_PRD_SEL_WIPDATACOLLECT_CLCTTYPE_E";
                    DataTable dtRQSTDT = new DataTable("RQSTDT");
                    DataTable dtRSLTDT = new DataTable("RSLTDT");
                    dtRQSTDT.Columns.Add("LANGID", typeof(string));
                    dtRQSTDT.Columns.Add("LOTID", typeof(string));

                    DataRow drRQSTDT = dtRQSTDT.NewRow();
                    drRQSTDT["LANGID"] = LoginInfo.LANGID;
                    drRQSTDT["LOTID"] = this.txtLOTID.Text;
                    dtRQSTDT.Rows.Add(drRQSTDT);
                    dtRSLTDT = new ClientProxy().ExecuteServiceSync(bizRuleName, dtRQSTDT.TableName, dtRSLTDT.TableName, dtRQSTDT);
                    this.dgWIPDataCollect.ItemsSource = DataTableConverter.Convert(dtRSLTDT);
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        // 순서도 호출 : 설비데이터입력을 위한 수집항목 목록 조회
        private void GetQualityItemForWIPDataCollectInput()
        {
            try
            {
                string bizRuleName = "DA_PRD_SEL_QUALITEM_BY_CLCTTYPE";
                DataTable dtRQSTDT = new DataTable("RQSTDT");
                DataTable dtRSLTDT = new DataTable("RSLTDT");
                dtRQSTDT.Columns.Add("LANGID", typeof(string));
                dtRQSTDT.Columns.Add("PRODID", typeof(string));
                dtRQSTDT.Columns.Add("PROCID", typeof(string));
                dtRQSTDT.Columns.Add("CLCTTYPE", typeof(string));
                dtRQSTDT.Columns.Add("AREAID", typeof(string));

                DataRow drRQSTDT = dtRQSTDT.NewRow();
                drRQSTDT["LANGID"] = LoginInfo.LANGID;
                drRQSTDT["PRODID"] = txtProductID.Text;
                drRQSTDT["PROCID"] = Util.NVC(cboProcess.SelectedValue);
                drRQSTDT["CLCTTYPE"] = "E";
                drRQSTDT["AREAID"] = LoginInfo.CFG_AREA_ID;
                dtRQSTDT.Rows.Add(drRQSTDT);

                dtRSLTDT = new ClientProxy().ExecuteServiceSync(bizRuleName, dtRQSTDT.TableName, dtRSLTDT.TableName, dtRQSTDT);
                this.dgQualityItem.ItemsSource = DataTableConverter.Convert(dtRSLTDT);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        // 순서도 호출 : 입력한 수집항목명 저장
        private void SaveWipDataCollect()
        {
            try
            {
                //BR_PRD_REG_DETAILDATA
                DataSet dsINDATA = new DataSet();
                DataTable dtINDATA = new DataTable("INDATA");
                dtINDATA.Columns.Add("SRCTYPE", typeof(string));
                dtINDATA.Columns.Add("LANGID", typeof(string));
                dtINDATA.Columns.Add("LOTID", typeof(string));
                dtINDATA.Columns.Add("PROCID", typeof(string));
                dtINDATA.Columns.Add("EQPTID", typeof(string));
                dtINDATA.Columns.Add("USERID", typeof(string));

                DataRow drINDATA = dtINDATA.NewRow();
                drINDATA["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                drINDATA["LANGID"] = LoginInfo.LANGID;
                drINDATA["LOTID"] = txtLOTID.Text;
                drINDATA["PROCID"] = cboProcess.SelectedValue;
                drINDATA["EQPTID"] = cboEquipment.SelectedValue;
                drINDATA["USERID"] = LoginInfo.USERID;
                dtINDATA.Rows.Add(drINDATA);
                dsINDATA.Tables.Add(dtINDATA);

                DataTable dtIN_CLCTITEM = new DataTable("IN_CLCTDITEM");
                dtIN_CLCTITEM.Columns.Add("CLCTDITEM", typeof(string));
                dtIN_CLCTITEM.Columns.Add("CLCTDVAL", typeof(string));

                DataRow drIN_CLCTITEM = null;
                if (this.dgQualityItem.Rows.Count > 0)
                {
                    for (int i = 0; i < this.dgQualityItem.Rows.Count; i++)
                    {
                        // 손홍구S 요청 : 데이터 값이 있을 경우만 저장
                        if ((Util.NVC(DataTableConverter.GetValue(this.dgQualityItem.Rows[i].DataItem, "CLCTVAL01")).Length > 0))
                        {
                            drIN_CLCTITEM = dtIN_CLCTITEM.NewRow();
                            drIN_CLCTITEM["CLCTDITEM"] = Util.NVC(DataTableConverter.GetValue(this.dgQualityItem.Rows[i].DataItem, "CLCTITEM"));
                            drIN_CLCTITEM["CLCTDVAL"] = Util.NVC(DataTableConverter.GetValue(this.dgQualityItem.Rows[i].DataItem, "CLCTVAL01"));
                            dtIN_CLCTITEM.Rows.Add(drIN_CLCTITEM);
                        }
                    }
                }
                else
                {
                    drIN_CLCTITEM = dtIN_CLCTITEM.NewRow();
                    dtIN_CLCTITEM.Rows.Add(drIN_CLCTITEM);
                }

                dsINDATA.Tables.Add(dtIN_CLCTITEM);
                new ClientProxy().ExecuteServiceSync_Multi("BR_PRD_REG_DETAILDATA_PACK", "INDATA,IN_CLCTDITEM", "", dsINDATA, null);

                ClearQualityItemValue();
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        #endregion

        #region Event Lists...
        private void C1Window_Loaded(object sender, RoutedEventArgs e)
        {
            this.Initialize();
        }

        private void btnAdd_Click(object sender, RoutedEventArgs e)
        {
            this.GetQualityItemForWIPDataCollectInput();
        }

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = MessageBoxResult.OK;
            this.Close();
        }

        private void btnOK_Click(object sender, RoutedEventArgs e)
        {
            this.SaveProcess();
        }

        private void btnReset_Click(object sender, RoutedEventArgs e)
        {
            this.ClearQualityItemValue();
        }

        private void cboProcess_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            string processID = e.NewValue.ToString();
            DataTable dt = this.GetEquipmentInfo(this.equipmentSegmentID, processID);
            PackCommon.SetC1ComboBox(dt, this.cboEquipment, true);
            if (!string.IsNullOrEmpty(LoginInfo.CFG_EQPT_ID))
            {
                this.cboEquipment.SelectedValue = LoginInfo.CFG_PROC_ID;
                if (this.cboEquipment.SelectedIndex < 0)
                {
                    this.cboEquipment.SelectedIndex = 0;
                }
            }
            else
            {
                this.cboEquipment.SelectedIndex = 0;
            }
        }
        #endregion
    }
}
