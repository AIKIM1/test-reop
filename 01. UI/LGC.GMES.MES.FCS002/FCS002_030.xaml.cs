/*************************************************************************************
 Created Date : 2020.11.24
      Creator : 
   Decription : 수동 판정
--------------------------------------------------------------------------------------
 [Change History]
  2020.11.24  DEVELOPER : Initial Created.
  2021.04.02     PSM    : Lot별 예측 dOCV 수동판정 추가
  2022.08.24    김진섭  : C20220816-000503 -W등급 기준정보 등록 & 삭제시 발생하는 오류 수정 및 수정기능 추가
**************************************************************************************/
using C1.WPF;
using C1.WPF.DataGrid;
using C1.WPF.DataGrid.Summaries;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using System;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Linq;
using C1.WPF.Excel;
using System.Configuration;
using Microsoft.Win32;
using System.IO;
using LGC.GMES.MES.CMM001.Extensions;
using System.Collections.Generic;


namespace LGC.GMES.MES.FCS002
{
    public partial class FCS002_030 : UserControl, IWorkArea
    {
        #region Declaration & Constructor 
        private readonly Util _util = new Util();
        private int EVENT_PROC_CNT = 0;
        private string[] JUDGlist = null;
        int judgeTotalCount = 0;
        int judgeTotalCountMulti = 0;

        private int dRouteidx = 0;
        private int dProcidx = 0;
        public FCS002_030()
        {
            InitializeComponent();
        }
        #endregion

        #region Initialize
        private void InitCombo()
        {
            CommonCombo_Form_MB _combo = new CommonCombo_Form_MB();

            C1ComboBox[] cboLineChild = { cboModel };
            _combo.SetCombo(cboLine, CommonCombo_Form_MB.ComboStatus.ALL, sCase: "LINE", cbChild: cboLineChild);

            C1ComboBox[] cboModelChild = { cboRoute };
            C1ComboBox[] cboModelParent = { cboLine };
            _combo.SetCombo(cboModel, CommonCombo_Form_MB.ComboStatus.ALL, sCase: "LINEMODEL", cbChild: cboModelChild, cbParent: cboModelParent);

            C1ComboBox[] cboRouteParent = { cboLine, cboModel };
            C1ComboBox[] cboRouteChild = { cboJudgOp, cboOp };
            _combo.SetCombo(cboRoute, CommonCombo_Form_MB.ComboStatus.ALL, sCase: "ROUTE", cbChild: cboRouteChild, cbParent: cboRouteParent);
            
            C1ComboBox[] cboOperParent = { cboRoute };
            _combo.SetCombo(cboOp, CommonCombo_Form_MB.ComboStatus.ALL, sCase: "ROUTE_OP", cbParent: cboOperParent);

            _combo.SetCombo(cboJudgOp, CommonCombo_Form_MB.ComboStatus.NONE, sCase: "JUDGE_OP", cbParent: cboOperParent);

            string[] sFilter1 = { "COMBO_WIPSTATE" };
            _combo.SetCombo(cboState, CommonCombo_Form_MB.ComboStatus.ALL, sCase: "COMMCODE", sFilter: sFilter1);

            string[] sFilter2 = { "FLAG_YN" };

            _combo.SetCombo(cboAbnorm, CommonCombo_Form_MB.ComboStatus.ALL, sCase: "COMMCODE", sFilter: sFilter2);
            _combo.SetCombo(cboISS, CommonCombo_Form_MB.ComboStatus.ALL, sCase: "COMMCODE", sFilter: sFilter2);

            //Lot별 예측 dOCV 수동판정
            _combo.SetCombo(cboModel2, CommonCombo_Form_MB.ComboStatus.ALL, sCase: "MODEL");

            // FITTED 판정 숨김처리
            chkDCIR.Visibility = Visibility.Collapsed;



            C1ComboBox[] cboLineMultiChild = { cboModelMulti };
            _combo.SetCombo(cboLineMulti, CommonCombo_Form_MB.ComboStatus.ALL, sCase: "LINE", cbChild: cboLineMultiChild);

            C1ComboBox[] cboModelMultiChild = { cboRouteMulti };
            C1ComboBox[] cboModelMultiParent = { cboLineMulti };
            _combo.SetCombo(cboModelMulti, CommonCombo_Form_MB.ComboStatus.ALL, sCase: "LINEMODEL", cbChild: cboModelMultiChild, cbParent: cboModelMultiParent);

            C1ComboBox[] cboRouteMultiParent = { cboLineMulti, cboModelMulti };
            //C1ComboBox[] cboRouteMultiChild = { cboJudgOpMulti, cboOpMulti };
            _combo.SetCombo(cboRouteMulti, CommonCombo_Form_MB.ComboStatus.ALL, sCase: "ROUTE", cbParent: cboRouteMultiParent);

            C1ComboBox[] cboOperMultiParent = { cboRouteMulti };
            _combo.SetCombo(cboOpMulti, CommonCombo_Form_MB.ComboStatus.ALL, sCase: "ROUTE_OP", cbParent: cboOperMultiParent);

            //_combo.SetCombo(cboJudgOpMulti, CommonCombo_Form_MB.ComboStatus.NONE, sCase: "JUDGE_OP", cbParent: cboOperMultiParent);
           
            string[] sFilterMulti1 = { "COMBO_WIPSTATE" };
            _combo.SetCombo(cboStateMulti, CommonCombo_Form_MB.ComboStatus.ALL, sCase: "COMMCODE", sFilter: sFilter1);

            string[] sFilterMulti2 = { "FLAG_YN" };

            _combo.SetCombo(cboAbnormMulti, CommonCombo_Form_MB.ComboStatus.ALL, sCase: "COMMCODE", sFilter: sFilter2);
            _combo.SetCombo(cboISSMulti, CommonCombo_Form_MB.ComboStatus.ALL, sCase: "COMMCODE", sFilter: sFilter2);
            
            // FITTED 판정 숨김처리
            chkDCIRMulti.Visibility = Visibility.Collapsed;


            // dOCV 백분율 판정
            GetdOCVRoute();

        }
        /// <summary>
        /// Frame과 상호작용하기 위한 객체
        /// </summary>
        public IFrameOperation FrameOperation
        {
            get;
            set;
        }

        private void GetAreaCommonCode(string sComCode)
        {
            try
            {
                EVENT_PROC_CNT = 0;

                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("AREAID", typeof(string));
                RQSTDT.Columns.Add("COM_TYPE_CODE", typeof(string));
                RQSTDT.Columns.Add("COM_CODE", typeof(string));
                RQSTDT.Columns.Add("USE_FLAG", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["AREAID"] = LoginInfo.CFG_AREA_ID;
                dr["COM_TYPE_CODE"] = "EVENT_PROCESSING_COUNT";
                dr["COM_CODE"] = sComCode;
                dr["USE_FLAG"] = "Y";
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_AREA_COM_CODE_USE", "RQSTDT", "RSLTDT", RQSTDT);

                foreach (DataRow row in dtResult.Rows)
                {
                    EVENT_PROC_CNT = int.Parse(row["ATTR1"].ToString());
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }


        private void GetdOCVRoute()
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("AREAID", typeof(string));
                RQSTDT.Columns.Add("ITEM_CODE", typeof(string));
                RQSTDT.Columns.Add("ITEM_GR_CODE", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["AREAID"] = LoginInfo.CFG_AREA_ID;
                dr["ITEM_GR_CODE"] = "R_GRP08";
                dr["ITEM_CODE"] = "DOCV_MV_PCT_USE_FLAG";
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_DOCV_ROUTE_CBO_MB", "RQSTDT", "RSLTDT", RQSTDT);

                cboDRoute.DisplayMemberPath = "CBO_NAME";
                cboDRoute.SelectedValuePath = "CBO_CODE";

                DataRow drIns = dtResult.NewRow();
                drIns["CBO_NAME"] = "-ALL-";
                drIns["CBO_CODE"] = "";
                dtResult.Rows.InsertAt(drIns, 0);
                cboDRoute.ItemsSource = dtResult.Copy().AsDataView();


                cboDRoute.SelectedIndex = 0;
            }


            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
}

        #endregion

        #region Method

        #region [수동판정 tab]
        private void GetList()
        {
            try
            {
                //xProgress.Value = 0;
                //xTextBlock.Text = "0/0";
                chkHeaderAll.IsChecked = false;

                DataTable dtRqst = new DataTable();
                dtRqst.TableName = "RQSTDT";
                dtRqst.Columns.Add("LANGID", typeof(string));
                dtRqst.Columns.Add("PROCID", typeof(string));
                dtRqst.Columns.Add("EQSGID", typeof(string));
                dtRqst.Columns.Add("MDLLOT_ID", typeof(string));
                dtRqst.Columns.Add("ROUTID", typeof(string));
                dtRqst.Columns.Add("WIPSTAT", typeof(string));
                dtRqst.Columns.Add("ABNORM_FLAG", typeof(string));
                dtRqst.Columns.Add("ISS_RSV_FLAG", typeof(string));
                dtRqst.Columns.Add("PROD_LOTID", typeof(string));
                dtRqst.Columns.Add("CSTID", typeof(string));

                DataRow dr = dtRqst.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["PROCID"] = Util.GetCondition(cboOp, bAllNull: true);
                dr["EQSGID"] = Util.GetCondition(cboLine, bAllNull: true);
                dr["MDLLOT_ID"] = Util.GetCondition(cboModel, bAllNull: true);
                dr["ROUTID"] = Util.GetCondition(cboRoute, bAllNull: true);
                dr["WIPSTAT"] = Util.GetCondition(cboState, bAllNull: true);
                dr["ABNORM_FLAG"] = Util.GetCondition(cboAbnorm, bAllNull: true);
                dr["ISS_RSV_FLAG"] = Util.GetCondition(cboISS, bAllNull: true);
                if (dr["ISS_RSV_FLAG"].Equals("N")) dr["ISS_RSV_FLAG"] = null;
                if (!string.IsNullOrEmpty(txtLotID.Text)) dr["PROD_LOTID"] = Util.GetCondition(txtLotID, bAllNull: true);
                if (!string.IsNullOrEmpty(txtTrayID.Text)) dr["CSTID"] = Util.GetCondition(txtTrayID, bAllNull: true);
                dtRqst.Rows.Add(dr);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_SEL_TRAY_LIST_JDA_FOR_JUDGE_MB", "RQSTDT", "RSLTDT", dtRqst);
                dtRslt.Columns.Add("CHK", typeof(bool));
                dtRslt.Columns.Add("REMARK", typeof(string));
                Util.GridSetData(dgMaintJudg, dtRslt, FrameOperation);
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }

        private void GetListOne()
        {
            try
            {
                //xProgressMulti.Value = 0;
                //xTextBlockMulti.Text = "0/0";
                chkAllJUDG.IsChecked = false;
                chkHeaderAllMulti.IsChecked = false;
                //lbJUDG.Items.Clear();

                DataTable dtRqst = new DataTable();
                dtRqst.TableName = "RQSTDT";
                dtRqst.Columns.Add("LANGID", typeof(string));
                dtRqst.Columns.Add("PROCID", typeof(string));
                dtRqst.Columns.Add("EQSGID", typeof(string));
                dtRqst.Columns.Add("MDLLOT_ID", typeof(string));
                dtRqst.Columns.Add("ROUTID", typeof(string));
                dtRqst.Columns.Add("WIPSTAT", typeof(string));
                dtRqst.Columns.Add("ABNORM_FLAG", typeof(string));
                dtRqst.Columns.Add("ISS_RSV_FLAG", typeof(string));
                dtRqst.Columns.Add("CSTID", typeof(string));

                DataRow dr = dtRqst.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["PROCID"] = Util.GetCondition(cboOpMulti, bAllNull: true);
                dr["EQSGID"] = Util.GetCondition(cboLineMulti, bAllNull: true);
                dr["MDLLOT_ID"] = Util.GetCondition(cboModelMulti, bAllNull: true);
                dr["ROUTID"] = Util.GetCondition(cboRouteMulti, bAllNull: true);
                dr["WIPSTAT"] = Util.GetCondition(cboStateMulti, bAllNull: true);
                dr["ABNORM_FLAG"] = Util.GetCondition(cboAbnormMulti, bAllNull: true);
                dr["ISS_RSV_FLAG"] = Util.GetCondition(cboISSMulti, bAllNull: true);
                if (dr["ISS_RSV_FLAG"].Equals("N")) dr["ISS_RSV_FLAG"] = null;
                if (string.IsNullOrEmpty(txtTrayIDMulti.Text))
                    return;
                dr["CSTID"] = Util.GetCondition(txtTrayIDMulti, bAllNull: true);
                dtRqst.Rows.Add(dr);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_SEL_TRAY_LIST_JDA_FOR_JUDGE_TRAY_MB", "RQSTDT", "RSLTDT", dtRqst);

                if(dtRslt.Rows.Count == 0)
                {
                    Util.Alert("FM_ME_0260");
                }
                dtRslt.Columns.Add("CHK", typeof(bool));
                dtRslt.Columns.Add("REMARK", typeof(string));
                Util.GridSetData(dgMaintJudgMulti, dtRslt, FrameOperation);
                string sRoutID = string.Empty;
                if (dtRslt.Rows.Count>0)
                     sRoutID = dtRslt.Rows[0]["ROUTID"].ToString();

                //InitControl(sRoutID);
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }

        private void InitControl(string sRoutID)
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("ROUTID", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["ROUTID"] = sRoutID;
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_COMBO_ROUTE_JUDGE_OP", "RQSTDT", "RSLTDT", RQSTDT);

                foreach (DataRow drResult in dtResult.Rows)
                {
                    CheckBox cbChk = new CheckBox();
                    cbChk.Tag = drResult["CBO_CODE"];
                    cbChk.Content = drResult["CBO_NAME"];
                    //lbJUDG.Items.Add(cbChk);
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }

        }

        private string PrevJudgOpFittedCheck(string sTrayNo, string sRouteId, string sOp, int iFitted)
        {
            string sRtn = string.Empty;

            try
            {
                DataTable dtRqst = new DataTable();
                dtRqst.TableName = "RQSTDT";
                dtRqst.Columns.Add("LOTID", typeof(string));
                dtRqst.Columns.Add("ROUTID", typeof(string));
                dtRqst.Columns.Add("JUDG_PROCID", typeof(string));
                dtRqst.Columns.Add("FITTED_MODE", typeof(string));

                DataRow dr = dtRqst.NewRow();
                dr["LOTID"] = sTrayNo;
                dr["ROUTID"] = sRouteId;
                dr["JUDG_PROCID"] = sOp;
                dr["FITTED_MODE"] = iFitted.ToString();
                dtRqst.Rows.Add(dr);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("BR_GET_JUDG_FITTED_OP_MB", "RQSTDT", "RSLTDT", dtRqst);
                sRtn = dtRslt.Rows[0]["RETVAL"].ToString();
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
            return sRtn;
        }
        private void progress(int i)
        {
            //  await System.Threading.Tasks.Task.Run(() =>
            //   {
            const string bizRuleName = "BR_SET_CELL_JUDGMENT_MANUAL_MB";

            //  Application.Current.Dispatcher.Invoke(new Action(delegate
            //  {
            DataTable dtRqst = new DataTable();
            dtRqst.Columns.Add("SRCTYPE", typeof(string));
            dtRqst.Columns.Add("IFMODE", typeof(string));
            dtRqst.Columns.Add("LOTID", typeof(string));           //TRAY_NO
            dtRqst.Columns.Add("JUDG_PROCID", typeof(string));     //JUDG_OP_ID
            dtRqst.Columns.Add("USERID", typeof(string));          //MDF_ID
            dtRqst.Columns.Add("FITTED_MODE", typeof(string));     //FITTED_MODE
            dtRqst.Columns.Add("NOT_INIT_GRADE_FLAG", typeof(string)); //NOT_INIT_GRADE

            DataRow dr = dtRqst.NewRow();
            dr["SRCTYPE"] = "UI";
            dr["IFMODE"] = "OFF";
            dr["LOTID"] = Util.NVC(DataTableConverter.GetValue(dgMaintJudg.Rows[i].DataItem, "LOTID"));
            dr["JUDG_PROCID"] = Util.GetCondition(cboJudgOp, "FM_ME_0248");
            dr["USERID"] = LoginInfo.USERID;

            //chkCapa, chkCell 자동차는 사용하지 않음. 화면에서 숨김
            int FittedMode = 0;
            if ((bool)chkDCIR.IsChecked) FittedMode = FittedMode + 1;
            if ((bool)chkCapa.IsChecked) FittedMode = FittedMode + 2;
            dr["FITTED_MODE"] = FittedMode;
            dr["NOT_INIT_GRADE_FLAG"] = chkCell.IsChecked == true ? "Y" : "N";

            dtRqst.Rows.Add(dr);


            new ClientProxy().ExecuteService(bizRuleName, "RQSTDT", "RSLTDT", dtRqst, (bizResult, bizException) =>
            {
                if (bizException != null)
                {
                    loadingIndicator.Visibility = Visibility.Collapsed;
                    DataTableConverter.SetValue(dgMaintJudg.Rows[i].DataItem, "REMARK", "FAIL : " + bizException.Message);
                    UpdateProgressBar(i);
                    return;
                }
                if (bizResult.Rows[0]["RETVAL"].ToString().Equals("0"))
                {
                    DataTableConverter.SetValue(dgMaintJudg.Rows[i].DataItem, "REMARK", "SUCCESS");
                }

                else
                {
                    DataTableConverter.SetValue(dgMaintJudg.Rows[i].DataItem, "REMARK", "FAIL");
                }
                UpdateProgressBar(i);


            });

            //    }),
            //        System.Windows.Threading.DispatcherPriority.Input
            //    );
            //   });
            //  loadingIndicator.Visibility = Visibility.Collapsed;
        }

        private async void progressMulti(int i)
        {
            await System.Threading.Tasks.Task.Run(() =>
            {
                const string bizRuleName = "BR_SET_CELL_JUDGMENT_MANUAL_MB";

                Application.Current.Dispatcher.Invoke(new Action(delegate
                {
                    DataTable dtRqst = new DataTable();
                    dtRqst.Columns.Add("SRCTYPE", typeof(string));
                    dtRqst.Columns.Add("IFMODE", typeof(string));
                    dtRqst.Columns.Add("LOTID", typeof(string));           //TRAY_NO
                    dtRqst.Columns.Add("JUDG_PROCID", typeof(string));     //JUDG_OP_ID
                    dtRqst.Columns.Add("USERID", typeof(string));          //MDF_ID
                    dtRqst.Columns.Add("FITTED_MODE", typeof(string));     //FITTED_MODE
                    dtRqst.Columns.Add("NOT_INIT_GRADE_FLAG", typeof(string)); //NOT_INIT_GRADE

                    DataRow dr = dtRqst.NewRow();
                    dr["SRCTYPE"] = "UI";
                    dr["IFMODE"] = "OFF";
                    dr["LOTID"] = Util.NVC(DataTableConverter.GetValue(dgMaintJudgMulti.Rows[0].DataItem, "LOTID"));
                    dr["JUDG_PROCID"] = JUDGlist[i];
                    dr["USERID"] = LoginInfo.USERID;

                    //chkCapa, chkCell 자동차는 사용하지 않음. 화면에서 숨김
                    int FittedMode = 0;
                    if ((bool)chkDCIRMulti.IsChecked) FittedMode = FittedMode + 1;
                    if ((bool)chkCapaMulti.IsChecked) FittedMode = FittedMode + 2;
                    dr["FITTED_MODE"] = FittedMode;
                    dr["NOT_INIT_GRADE_FLAG"] = chkCellMulti.IsChecked == true ? "Y" : "N";

                    dtRqst.Rows.Add(dr);


                    new ClientProxy().ExecuteService(bizRuleName, "RQSTDT", "RSLTDT", dtRqst, (bizResult, bizException) =>
                    {
                        if (bizException != null)
                        {
                            loadingIndicator.Visibility = Visibility.Collapsed;
                            DataTableConverter.SetValue(dgMaintJudgMulti.Rows[0].DataItem, "REMARK", "FAIL : " + bizException.Message);
                            UpdateProgressBarMulti(i);
                            return;
                        }
                        if (bizResult.Rows[0]["RETVAL"].ToString().Equals("0"))
                        {
                            DataTableConverter.SetValue(dgMaintJudgMulti.Rows[0].DataItem, "REMARK", "SUCCESS");
                        }

                        else
                        {
                            DataTableConverter.SetValue(dgMaintJudg.Rows[0].DataItem, "REMARK", "FAIL");
                        }
                        UpdateProgressBarMulti(i);



                    });

                }),
                    System.Windows.Threading.DispatcherPriority.Input
                );
            });
            loadingIndicator.Visibility = Visibility.Collapsed;
        }
        private void UpdateProgressBar(int value)
        {
            //Application.Current.Dispatcher.Invoke(
            //    new Action(delegate
            //    {
            //        xProgress.v += 1;
            //        int maxVal =  _util.GetDataGridRowCountByCheck(dgMaintJudg, "CHK", true);
            //        //xTextBlock.Text = xProgress.Value + "/" + _util.GetDataGridRowCountByCheck(dgMaintJudg, "CHK", true);
            //        xTextBlock.Text = xProgress.Value + "/" + maxVal;

            //        if ((int)xProgress.Value == maxVal)
            //            btnJudge.IsEnabled = true;
            //    }),
            //    System.Windows.Threading.DispatcherPriority.Input

            
            

            //);
        }

        private void UpdateProgressBarMulti(int value)
        {
            //Application.Current.Dispatcher.Invoke(
            //    new Action(delegate
            //    {
            //        xProgressMulti.Value += 1;
            //        xTextBlockMulti.Text = xProgressMulti.Value + "/" + JUDGlist.LongLength;
            //    }),
            //    System.Windows.Threading.DispatcherPriority.Input
            //);
        }



        private void LoadExcel()
        {


            DataTable dataTable = new DataTable();

            OpenFileDialog fd = new OpenFileDialog();

            if (ConfigurationManager.AppSettings["APP_CONFIG_SBC_MODE"] == "SBC")
            {
                fd.InitialDirectory = @"\\Client\C$";
            }

            fd.Filter = "Excel Files (.xlsx)|*.xlsx";


            if (fd.ShowDialog() == true)
            {
                using (Stream stream = fd.OpenFile())
                {
                    C1XLBook book = new C1XLBook();
                    book.Load(stream, FileFormat.OpenXml);
                    XLSheet sheet = book.Sheets[0];
                    dataTable.Columns.Add("CSTID", typeof(string));

                    for (int rowInx = 0; rowInx < sheet.Rows.Count; rowInx++)
                    {
                        // CELL ID;
                        if (sheet.GetCell(rowInx, 0) == null)
                            return;

                        string CSTID = Util.NVC(sheet.GetCell(rowInx, 0).Text);
                        DataRow dataRow = dataTable.NewRow();
                        dataRow["CSTID"] = CSTID;
                        dataTable.Rows.Add(dataRow);


                    }



                }
            }

            // Excel Load 이상 없으면 목록에서 체크

            if (dataTable.Rows.Count > 0)
            {
                for (int i = 0; i < dataTable.Rows.Count; i++)
                {
                    string CSTID = dataTable.Rows[i]["CSTID"].ToString();

                    for (int dgRow = 0; dgRow < dgMaintJudg.Rows.Count; dgRow++)
                    {
                        if (CSTID.Equals(Util.NVC(DataTableConverter.GetValue(dgMaintJudg.Rows[dgRow].DataItem, "CSTID"))))
                            DataTableConverter.SetValue(dgMaintJudg.Rows[dgRow].DataItem, "CHK", true);
                    }
                }
            }
        }
        #endregion

        #region [Lot별 예측 dOCV 수동판정 tab]
        private void GetListWJudg()
        {
            xProgressW.Value = 0;
            xTextBlockW.Text = "0/0";

            Util.gridClear(dgTrayList); //Lot List 초기화

            DataTable dtRqst = new DataTable();
            dtRqst.TableName = "RQSTDT";
            dtRqst.Columns.Add("LANGID", typeof(string));
            dtRqst.Columns.Add("PROD_LOTID", typeof(string));
            dtRqst.Columns.Add("MDLLOT_ID", typeof(string));
            dtRqst.Columns.Add("W_JUDG_TYPE_CODE", typeof(string));
            dtRqst.Columns.Add("PREDCT_DAY", typeof(string));

            DataRow dr = dtRqst.NewRow();
            dr["LANGID"] = LoginInfo.LANGID;
            if (!string.IsNullOrEmpty(txtLotID2.Text)) dr["PROD_LOTID"] = txtLotID2.Text;
            dr["MDLLOT_ID"] = Util.GetCondition(cboModel2, bAllNull: true);
            dr["W_JUDG_TYPE_CODE"] = "G";
            dtRqst.Rows.Add(dr);

            string sBiz = "DA_SEL_TM_LOW_VOLTAGE_JUDG_LOT_MB";
            //  if ((bool)chkLLOT.IsChecked) sBiz = "DA_SEL_TM_LOW_VOLTAGE_JUDG_LOT_ASSY";

            DataTable dtRslt = new ClientProxy().ExecuteServiceSync(sBiz, "RQSTDT", "RSLTDT", dtRqst);
            dtRslt.Columns.Add("CHK", typeof(bool));
            dtRslt.Columns.Add("FLAG", typeof(string));
            dtRslt.Columns.Add("NEWFLAG", typeof(string));
            for (int i = 0; i < dtRslt.Rows.Count; i++)
            {
                dtRslt.Rows[i]["FLAG"] = "N";
                dtRslt.Rows[i]["NEWFLAG"] = "O";
            }
            Util.GridSetData(dgPredDocv, dtRslt, this.FrameOperation, true);
        }
        private async void JudgLotW(int i)
        {
            await System.Threading.Tasks.Task.Run(() =>
            {
                const string bizRuleName = "BR_SET_W_MANUAL_LOT_MB";

                Application.Current.Dispatcher.Invoke(new Action(delegate
                {
                    DataTable dtRqst = new DataTable();
                    dtRqst.Columns.Add("SRCTYPE", typeof(string));
                    dtRqst.Columns.Add("IFMODE", typeof(string));
                    dtRqst.Columns.Add("LOTID", typeof(string));           //TRAY_NO
                    dtRqst.Columns.Add("MANUAL_YN", typeof(string));
                    dtRqst.Columns.Add("USERID", typeof(string));          //MDF_ID

                    DataRow dr = dtRqst.NewRow();
                    dr["SRCTYPE"] = "UI";
                    dr["IFMODE"] = "OFF";
                    dr["LOTID"] = Util.NVC(DataTableConverter.GetValue(dgTrayList.Rows[i].DataItem, "LOTID"));
                    dr["MANUAL_YN"] = (bool)rdoManual.IsChecked ? "Y" : "N";
                    dr["USERID"] = LoginInfo.USERID;

                    dtRqst.Rows.Add(dr);


                    new ClientProxy().ExecuteService(bizRuleName, "RQSTDT", "RSLTDT", dtRqst, (bizResult, bizException) =>
                    {
                        if (bizException != null)
                        {
                            loadingIndicator.Visibility = Visibility.Collapsed;
                            DataTableConverter.SetValue(dgTrayList.Rows[i].DataItem, "REMARK", "FAIL : " + bizException.Message);
                            UpdateProgressBarW();
                            return;
                        }
                        if (bizResult.Rows[0]["RETVAL"].ToString().Equals("1"))
                        {
                            DataTableConverter.SetValue(dgTrayList.Rows[i].DataItem, "REMARK", "SUCCESS");
                        }

                        else
                        {
                            DataTableConverter.SetValue(dgTrayList.Rows[i].DataItem, "REMARK", "FAIL");
                        }
                        UpdateProgressBarW();
                    });

                }),
                    System.Windows.Threading.DispatcherPriority.Input
                );
            });
            loadingIndicator.Visibility = Visibility.Collapsed;
        }
        private void UpdateProgressBarW()
        {
            Application.Current.Dispatcher.Invoke(
                new Action(delegate
                {
                    xProgressW.Value += 1;
                    xTextBlockW.Text = xProgressW.Value + "/" + dgTrayList.Rows.Count;
                }),
                System.Windows.Threading.DispatcherPriority.Input
            );
        }



        private void dgPredDocv_CommittingEdit(object sender, DataGridEndingEditEventArgs e)
        {
            C1.WPF.DataGrid.C1DataGrid datagrid = (sender as C1.WPF.DataGrid.C1DataGrid);

            if (e.Column.Name.Equals("PROD_LOTID"))
            {
                string prodlotid = Util.NVC(DataTableConverter.GetValue(datagrid.Rows[e.Row.Index].DataItem, "PROD_LOTID"));

                if (string.IsNullOrEmpty(prodlotid)) return;

                for (int i = 0; i < datagrid.Rows.Count; i++)
                {
                    if (i == e.Row.Index) continue;

                    if (prodlotid.Equals(Util.NVC(DataTableConverter.GetValue(datagrid.Rows[i].DataItem, "PROD_LOTID"))))
                    {
                        DataTableConverter.SetValue(dgPredDocv.Rows[e.Row.Index].DataItem, "PROD_LOTID", "");

                        Util.Alert("SFU6005");

                        return;
                    }
                }
            }
        }

        #endregion

        #region [다중수동판정 tab]


        //private string[] GetCheckdJUDG()
        //{
        //    //int idx = 0;
        //    //string[] sCheck = new string[lbJUDG.Items.Count];
        //    //foreach (CheckBox itemChecked in lbJUDG.Items)
        //    //{
        //    //    if (itemChecked.IsChecked == true)
        //    //    {

        //    //        sCheck[idx] = itemChecked.Tag.ToString();
        //    //        idx++;
        //    //    }
        //    //}

        //    //sCheck = sCheck.Where(x => !string.IsNullOrEmpty(x)).ToArray();

        //    //return sCheck;
        //}


        #endregion

        #region [dOCV mv tab]


        private void GetListDOCV()
        {
            try
            {

                DataTable dtRqst = new DataTable();
                dtRqst.TableName = "RQSTDT";
                dtRqst.Columns.Add("ROUTID", typeof(string));
                dtRqst.Columns.Add("PROD_LOTID", typeof(string));

                DataRow dr = dtRqst.NewRow();
                dr["ROUTID"] = Util.GetCondition(cboDRoute, bAllNull: true);
                if (!string.IsNullOrEmpty(txtDLotID.Text))
                    dr["PROD_LOTID"] = Util.GetCondition(txtDLotID, bAllNull: true);
                dtRqst.Rows.Add(dr);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_SEL_DOCV_MV_PCT_LOT_MB", "RQSTDT", "RSLTDT", dtRqst);
                //dtRslt.Columns.Add("CHK", typeof(bool));
                Util.GridSetData(dgDocvSpec, dtRslt, FrameOperation);
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }
        private void GetDOCVProc(string sRoutID, string sProd_LotID)
        {
            try
            {

                DataTable dtRqst = new DataTable();
                dtRqst.TableName = "RQSTDT";
                dtRqst.Columns.Add("ROUTID", typeof(string));
                dtRqst.Columns.Add("LANGID", typeof(string));

                DataRow dr = dtRqst.NewRow();
                dr["ROUTID"] = sRoutID;
                dr["LANGID"] = LoginInfo.LANGID;
                dtRqst.Rows.Add(dr);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_SEL_DOCV_MV_PCT_PROC_MB", "RQSTDT", "RSLTDT", dtRqst);

                dtRslt.Columns.Add("DAY_GR_LOTID", typeof(string));

                for (int i = 0; i < dtRslt.Rows.Count; i++)
                {
                    dtRslt.Rows[i]["DAY_GR_LOTID"] = sProd_LotID;
                }


                Util.GridSetData(dgDocvProc, dtRslt, FrameOperation);
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }


        #endregion

        #endregion

        #region Event
        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            InitCombo();
            
            this.Loaded -= UserControl_Loaded;
        }

        #region [수동판정 tab]
        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            GetList();
            GetAreaCommonCode("100");
        }

        private void btnJudge_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!dgMaintJudg.IsCheckedRow("CHK"))
                {
                    Util.Alert("SFU1645"); //선택된 작업대상이 없습니다.
                    return;
                }             

                int iFitted = 0;

                if ((bool)chkDCIR.IsChecked)
                    iFitted = iFitted + 1;

                if ((bool)chkCapa.IsChecked)
                    iFitted = iFitted + 2;


                string sOp = Util.GetCondition(cboJudgOp);

                if (string.IsNullOrEmpty(sOp))
                {
                    Util.Alert("FM_ME_0248");  //판정공정을 선택해주세요.
                    return;
                }

                //if (result != MessageBoxResult.OK) return;

                List<DataRow> chkRows = dgMaintJudg.GetCheckedDataRow("CHK");

                object[] argument = new object[4] { chkRows, iFitted, sOp, chkCell.IsChecked == true ? "Y" : "N" };

                xProgress.Percent = 0;
                judgeTotalCount = _util.GetDataGridRowCountByCheck(dgMaintJudg, "CHK", true);
                xProgress.ProgressText = "0 / " + judgeTotalCount.Nvc();
                xProgress.Visibility = Visibility.Visible;

                Util.MessageConfirm("FM_ME_0177", (result) => //수동판정을 등록하시겠습니까?
                {
                    if (result != MessageBoxResult.OK) return;

                    btnSearch.IsEnabled = false;
                    btnJudge.IsEnabled = false;

                    xProgress.RunWorker(argument);
                });
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }

            //2024-09-11 DeadLock 오류 발생으로 주석처리
            //try
            //{
            //    Util.MessageConfirm("FM_ME_0177", (result) => //수동판정을 등록하시겠습니까?
            //    {

            //        if (result != MessageBoxResult.OK) return;

            //        btnJudge.IsEnabled = false;

            //        int iFitted = 0;

            //        if ((bool)chkDCIR.IsChecked)
            //            iFitted = iFitted + 1;

            //        if ((bool)chkCapa.IsChecked)
            //            iFitted = iFitted + 2;

            //        string sNotJudgeCell = string.Empty;

            //        if ((bool)chkCell.IsChecked)
            //            sNotJudgeCell = "Y";
            //        else
            //            sNotJudgeCell = "N";

            //        string sOp = Util.GetCondition(cboJudgOp);

            //        if (string.IsNullOrEmpty(sOp))
            //        {
            //            Util.Alert("FM_ME_0248");  //판정공정을 선택해주세요.
            //            return;
            //        }

            //        int chkCnt = 0;
            //        for (int i = 0; i < dgMaintJudg.Rows.Count; i++)
            //        {
            //            if (Util.NVC(DataTableConverter.GetValue(dgMaintJudg.Rows[i].DataItem, "CHK")).Equals("True") ||
            //                Util.NVC(DataTableConverter.GetValue(dgMaintJudg.Rows[i].DataItem, "CHK")).Equals("1"))
            //            {
            //                chkCnt++;
            //            }
            //        }

            //        if (chkCnt > EVENT_PROC_CNT)
            //        {
            //            // 최대 ROW수는 %1입니다.
            //            Util.MessageValidation("SFU5015", EVENT_PROC_CNT);
            //            return;
            //        }

            //        if (result != MessageBoxResult.OK) return;
            //        xProgress.Visibility = Visibility.Visible;
            //        xProgress.Maximum = _util.GetDataGridRowCountByCheck(dgMaintJudg, "CHK", true);
            //        xProgress.Minimum = 0;
            //        xProgress.Value = 0;
            //        xTextBlock.Text = "0/0";
            //        //개별 처리로 변경

            //        for (int i = 0; i < dgMaintJudg.Rows.Count; i++)
            //        {
            //            if (Util.NVC(DataTableConverter.GetValue(dgMaintJudg.Rows[i].DataItem, "CHK")).Equals("True") ||
            //        Util.NVC(DataTableConverter.GetValue(dgMaintJudg.Rows[i].DataItem, "CHK")).Equals("1"))
            //            {

            //                if (iFitted > 0)
            //                {

            //                    switch (PrevJudgOpFittedCheck(DataTableConverter.GetValue(dgMaintJudg.Rows[i].DataItem, "LOTID").ToString(),
            //                        DataTableConverter.GetValue(dgMaintJudg.Rows[i].DataItem, "ROUTID").ToString(), sOp, iFitted))
            //                    {
            //                        case "0":
            //                            break;
            //                        case "1":
            //                            Util.Alert("FM_ME_0005", new string[] { DataTableConverter.GetValue(dgMaintJudg.Rows[i].DataItem, "CSTID").ToString() });  //[Tray ID : {0}] 이전 판정에 Fitted 공정이 포함되어 있습니다.\r\n이전 판정 공정을 확인하세요.
            //                            return;
            //                        case "2":
            //                            Util.Alert("FM_ME_0007", new string[] { DataTableConverter.GetValue(dgMaintJudg.Rows[i].DataItem, "CSTID").ToString() });  //[Tray ID : {0}] 현재 이후 판정 공정 조건에 Fitted 공정이 포함되어 있지 않습니다. 판정 공정을 확인하세요.
            //                            return;
            //                        default:
            //                            Util.Alert("FM_ME_0006", new string[] { DataTableConverter.GetValue(dgMaintJudg.Rows[i].DataItem, "CSTID").ToString() });  //[Tray ID : {0}] 정상 처리되지 않았습니다. 판정 공정을 확인하세요.
            //                            return;
            //                    }
            //                }
            //                progress(i);
            //            }
            //        }

            //        //  btnJudge.IsEnabled = true;
            //        ////대용량 데이터 처리부분 - 대용량 처리용 화면 자체가 개발되어 있지 않으므로 위 개별 처리 로직으로 처리
            //    });


            //}
            //catch (Exception ex)
            //{
            //    LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            //}
        }

        private object xProgress_WorkProcess(object sender, CMM001.Controls.UcProgress.WorkProcessEventArgs e)
        {
            try
            {
                object[] arguments = e.Arguments as object[];

                List<DataRow> checkedRows = arguments[0] as List<DataRow>;

                int iFitted = arguments[1].NvcInt();

                string sOp = arguments[2].Nvc();

                string isCheckCell = arguments[3].Nvc();

                int workCount = 0;

                foreach (DataRow checkRow in checkedRows)
                {
                    workCount++;

                    if (iFitted > 0)
                    {
                        switch (PrevJudgOpFittedCheck(checkRow["LOTID"].Nvc(), checkRow["ROUTID"].Nvc(), sOp, iFitted))
                        {
                            case "0":
                                break;
                            case "1":
                                //[Tray ID : {0}] 이전 판정에 Fitted 공정이 포함되어 있습니다.\r\n이전 판정 공정을 확인하세요.
                                return MessageDic.Instance.GetMessage("FM_ME_0005", new string[] { checkRow["CSTID"].Nvc() });
                            case "2":
                                //[Tray ID : {0}] 현재 이후 판정 공정 조건에 Fitted 공정이 포함되어 있지 않습니다. 판정 공정을 확인하세요.
                                return MessageDic.Instance.GetMessage("FM_ME_0007", new string[] { checkRow["CSTID"].Nvc() });
                            default:
                                //[Tray ID : {0}] 정상 처리되지 않았습니다. 판정 공정을 확인하세요.
                                return MessageDic.Instance.GetMessage("FM_ME_0006", new string[] { checkRow["CSTID"].Nvc() });
                        }
                    }

                    string updateResultText = string.Empty;

                    try
                    {
                        const string bizRuleName = "BR_SET_CELL_JUDGMENT_MANUAL_MB";

                        DataTable dtRqst = new DataTable();
                        dtRqst.Columns.Add("SRCTYPE", typeof(string));
                        dtRqst.Columns.Add("IFMODE", typeof(string));
                        dtRqst.Columns.Add("LOTID", typeof(string));           //TRAY_NO
                        dtRqst.Columns.Add("JUDG_PROCID", typeof(string));     //JUDG_OP_ID
                        dtRqst.Columns.Add("USERID", typeof(string));          //MDF_ID
                        dtRqst.Columns.Add("FITTED_MODE", typeof(string));     //FITTED_MODE
                        dtRqst.Columns.Add("NOT_INIT_GRADE_FLAG", typeof(string)); //NOT_INIT_GRADE
                        dtRqst.Columns.Add("MENUID", typeof(string));
                        dtRqst.Columns.Add("USER_PC_IP", typeof(string));
                        dtRqst.Columns.Add("PC_NAME", typeof(string));

                        DataRow dr = dtRqst.NewRow();
                        dr["SRCTYPE"] = "UI";
                        dr["IFMODE"] = "OFF";
                        dr["LOTID"] = checkRow["LOTID"].Nvc();
                        dr["JUDG_PROCID"] = sOp;
                        dr["USERID"] = LoginInfo.USERID;
                        dr["FITTED_MODE"] = iFitted;
                        dr["NOT_INIT_GRADE_FLAG"] = isCheckCell;
                        dr["MENUID"] = LoginInfo.CFG_MENUID;
                        dr["USER_PC_IP"] = LoginInfo.USER_IP;
                        dr["PC_NAME"] = LoginInfo.PC_NAME;
                        dtRqst.Rows.Add(dr);

                        DataTable bizResult = new ClientProxy().ExecuteServiceSync(bizRuleName, "RQSTDT", "RSLTDT", dtRqst);
                        if (bizResult != null && bizResult.Rows.Count > 0 && bizResult.Rows[0]["RETVAL"].ToString().Equals("0"))
                        {
                            updateResultText = "SUCCESS";
                        }
                        else
                        {
                            updateResultText = "FAIL";
                        }
                    }
                    catch (Exception ex)
                    {
                        updateResultText = "FAIL : " + ex.Message;
                    }

                    object[] progressArgument = new object[2] { workCount.Nvc() + " / " + judgeTotalCount.Nvc(), updateResultText };

                    e.Worker.ReportProgress(Convert.ToInt16((double)workCount / (double)judgeTotalCount * 100), progressArgument);

                    checkRow["REMARK"] = updateResultText;
                }
                return "SUCCESS";
            }
            catch (Exception ex)
            {
                return ex;
            }
        }

        private void xProgress_WorkProcessChanged(object sender, int percent, CMM001.Controls.UcProgress.WorkProcessEventArgs e)
        {
            try
            {
                object[] progressArguments = e.Arguments as object[];

                string progressText = progressArguments[0].Nvc();
                string updateText = progressArguments[1].Nvc();

                xProgress.Percent = percent;
                xProgress.ProgressText = progressText;
                xProgress.InvalidateVisual();
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void xProgress_WorkProcessCompleted(object sender, CMM001.Controls.UcProgress.WorkProcessEventArgs e)
        {
            try
            {
                object[] arguments = e.Arguments as object[];

                if (e.Result != null && e.Result is string)
                {
                    if (e.Result.Nvc().Equals("SUCCESS"))
                    {
                        Util.AlertInfo("SFU1889");
                    }
                    else
                    {
                        Util.AlertInfo("[*]" + e.Result.Nvc());
                    }
                }
                else if (e.Result != null && e.Result is Exception)
                {
                    Util.MessageException(e.Result as Exception);
                }
                else
                {
                    string msg = MessageDic.Instance.GetMessage("FM_ME_0202");
                    Util.AlertInfo(msg);  //작업중 오류가 발생하였습니다.
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
            finally
            {
                xProgress.Visibility = Visibility.Collapsed;
                btnSearch.IsEnabled = true;
                btnJudge.IsEnabled = true;
            }
        }

        private void dgMaintJudg_LoadedRowHeaderPresenter(object sender, C1.WPF.DataGrid.DataGridRowEventArgs e)
        {
            if (e.Row.HeaderPresenter == null)
            {
                return;
            }

            e.Row.HeaderPresenter.Content = null;

            TextBlock tb = new TextBlock();

            tb.Text = (e.Row.Index + 1 - dgMaintJudg.TopRows.Count).ToString();
            tb.VerticalAlignment = VerticalAlignment.Center;
            tb.HorizontalAlignment = HorizontalAlignment.Center;
            e.Row.HeaderPresenter.Content = tb;
        }

        private void chkHeaderAll_Checked(object sender, RoutedEventArgs e)
        {
            Util.DataGridCheckAllChecked(dgMaintJudg);
        }

        private void chkHeaderAll_Unchecked(object sender, RoutedEventArgs e)
        {
            Util.DataGridCheckAllUnChecked(dgMaintJudg);
        }

        private void btnExcel_Click(object sender, RoutedEventArgs e)
        {
            GetList();

            LoadExcel();
        }

        #endregion

        #region [Lot별 예측 dOCV 수동판정 tab]


        private void DataGridRowAdd(C1.WPF.DataGrid.C1DataGrid dg, int iRowcount)
        {
            try
            {
                DataTable dt = new DataTable();

                foreach (C1.WPF.DataGrid.DataGridColumn col in dg.Columns)
                {
                    dt.Columns.Add(Convert.ToString(col.Name));
                }

                if (dg.ItemsSource != null)
                {
                    for (int i = 0; i < iRowcount; i++)
                    {
                        dt = DataTableConverter.Convert(dg.ItemsSource);
                        DataRow dr2 = dt.NewRow();
                        dt.Rows.Add(dr2);
                        // dg.BeginEdit();
                        dg.ItemsSource = DataTableConverter.Convert(dt);
                        //  dg.EndEdit();
                    }
                }
                else
                {
                    for (int i = 0; i < iRowcount; i++)
                    {
                        DataRow dr = dt.NewRow();
                        dt.Rows.Add(dr);
                        // dg.BeginEdit();
                        dg.ItemsSource = DataTableConverter.Convert(dt);
                        // dg.EndEdit();
                    }
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        private void DataGridRowRemove(C1.WPF.DataGrid.C1DataGrid dg)
        {
            try
            {
                if (dg.GetRowCount() > 0)
                {
                    DataTable dt = DataTableConverter.Convert(dg.ItemsSource);
                    dt.Rows[dt.Rows.Count - 1].Delete();
                    dt.AcceptChanges();
                    dg.ItemsSource = DataTableConverter.Convert(dt);
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return;
            }
        }
       

        private void btnSearch2_Click(object sender, RoutedEventArgs e)
        {
            GetListWJudg();
        }

        private void btnPlus_Click(object sender, RoutedEventArgs e)
        {
            DataGridRowAdd(dgPredDocv, 1);
            DataTableConverter.SetValue(dgPredDocv.Rows[dgPredDocv.Rows.Count - 1].DataItem, "FLAG", "Y");
            DataTableConverter.SetValue(dgPredDocv.Rows[dgPredDocv.Rows.Count - 1].DataItem, "NEWFLAG", "N");
        }

        private void btnMinus_Click(object sender, RoutedEventArgs e)
        {
            //string flag = Util.NVC(DataTableConverter.GetValue(dgPredDocv.Rows[dgPredDocv.Rows.Count - 1].DataItem, "FLAG"));
            //if (flag.Equals("Y")) DataGridRowRemove(dgPredDocv);

            string flag = Util.NVC(DataTableConverter.GetValue(dgPredDocv.Rows[dgPredDocv.Rows.Count - 1].DataItem, "NEWFLAG"));
            if (flag.Equals("N")) DataGridRowRemove(dgPredDocv);
        }

        private void btnLotW_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Util.MessageConfirm("FM_ME_0211", (result) =>  //재판정하시겠습니까?
                {
                    if (result != MessageBoxResult.OK)
                    {
                        return;
                    }

                    xProgressW.Maximum = dgTrayList.Rows.Count;
                    xProgressW.Minimum = 0;
                    xTextBlockW.Text = "0/0";
                    for (int i = 0; i < dgTrayList.Rows.Count; i++)
                    {
                        JudgLotW(i);
                    }
                });
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Util.MessageConfirm("FM_ME_0214", (result) => //저장하시겠습니까?
                {
                    if (result != MessageBoxResult.OK)
                    {
                        return;
                    }

                    DataTable dtRqst = new DataTable();
                    dtRqst.Columns.Add("PROD_LOTID", typeof(string));
                    dtRqst.Columns.Add("PREDCT_DAY", typeof(string));
                    dtRqst.Columns.Add("L_THRES", typeof(double));
                    dtRqst.Columns.Add("U_THRES", typeof(double));
                    dtRqst.Columns.Add("W_JUDG_TYPE_CODE", typeof(string));
                    dtRqst.Columns.Add("PASS_YN", typeof(string));
                    dtRqst.Columns.Add("USERID", typeof(string));

                    for (int i = 0; i < dgPredDocv.Rows.Count; i++)
                    {
                        if (Util.NVC(DataTableConverter.GetValue(dgPredDocv.Rows[i].DataItem, "FLAG")).Equals("Y"))  //수정 된 Row만
                        {
                            string lot = Util.NVC(DataTableConverter.GetValue(dgPredDocv.Rows[i].DataItem, "PROD_LOTID"));
                            string predct = Util.NVC(DataTableConverter.GetValue(dgPredDocv.Rows[i].DataItem, "PREDCT_DAY"));
                            string lthres = Util.NVC(DataTableConverter.GetValue(dgPredDocv.Rows[i].DataItem, "L_THRES"));
                            string uthres = Util.NVC(DataTableConverter.GetValue(dgPredDocv.Rows[i].DataItem, "U_THRES"));

                            if (string.IsNullOrEmpty(lot) || string.IsNullOrEmpty(predct)) //PROD_LOTID, PREDCT_DAY는 필수조건
                            {
                                continue;
                            }
                            DataRow dr = dtRqst.NewRow();
                            dr["PROD_LOTID"] = lot;
                            dr["PREDCT_DAY"] = predct;
                            if (!string.IsNullOrEmpty(lthres)) dr["L_THRES"] = Convert.ToDouble(lthres);
                            if (!string.IsNullOrEmpty(uthres)) dr["U_THRES"] = Convert.ToDouble(uthres);
                            dr["W_JUDG_TYPE_CODE"] = "G";
                            dr["PASS_YN"] = "N";
                            dr["USERID"] = LoginInfo.USERID;

                            
                            dtRqst.Rows.Add(dr);

                            DataTable dtRslt = new ClientProxy().ExecuteServiceSync("BR_SET_LOW_VOLTAGE_JUDG_LOT_MB", "RQSTDT", "RSLTDT", dtRqst);
                            if (int.Parse(dtRslt.Rows[0]["RETVAL"].ToString()) > 0)
                                Util.Alert("FM_ME_0215");  //저장하였습니다.
                            else
                                Util.Alert("FM_ME_0213");  //저장실패하였습니다.

                            GetListWJudg();
                        }
                    }
                });
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }

        private void dgPredDocv_BeginningEdit(object sender, C1.WPF.DataGrid.DataGridBeginningEditEventArgs e)
        {
            C1.WPF.DataGrid.C1DataGrid dataGrid = sender as C1.WPF.DataGrid.C1DataGrid;

            if (e.Column.Name.Equals("PROD_LOTID"))
            {
                //if (Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Row.Index].DataItem, "FLAG")).Equals("N"))
                if(Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Row.Index].DataItem, "NEWFLAG")).Equals("O"))
                {
                    e.Cancel = true;
                }
            }
            dataGrid.Rows[e.Row.Index].Presenter.Background = new SolidColorBrush(Colors.LightYellow);

            //C1.WPF.DataGrid.C1DataGrid datagrid = (sender as C1.WPF.DataGrid.C1DataGrid);
            int row = e.Row.Index;
            DataTableConverter.SetValue(dataGrid.Rows[row].DataItem, "FLAG", "Y");
        }

        private void dgPredDocv_LoadedCellPresenter(object sender, DataGridCellEventArgs e)
        {
        }

        private void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (dgPredDocv.Rows.Count == 0) return;
                Util.MessageConfirm("FM_ME_0167", (result) => //선택한 데이터를 삭제하시겠습니까?
                {
                    if (result != MessageBoxResult.OK)
                    {
                        return;
                    }

                    DataTable dtRqst = new DataTable();
                    dtRqst.Columns.Add("PROD_LOTID", typeof(string));
                    dtRqst.Columns.Add("PREDCT_DAY", typeof(string));
                    dtRqst.Columns.Add("L_THRES", typeof(string));
                    dtRqst.Columns.Add("U_THRES", typeof(string));
                    dtRqst.Columns.Add("USERID", typeof(string));
                    dtRqst.Columns.Add("W_JUDG_TYPE_CODE", typeof(string));
                    dtRqst.Columns.Add("PASS_YN", typeof(string));

                    for (int i = 0; i < dgPredDocv.Rows.Count; i++)
                    {
                        if (Util.NVC(DataTableConverter.GetValue(dgPredDocv.Rows[i].DataItem, "CHK")).ToUpper().Equals("TRUE"))
                        {
                            DataRow dr = dtRqst.NewRow();
                            dr["PROD_LOTID"] = Util.NVC(DataTableConverter.GetValue(dgPredDocv.Rows[i].DataItem, "PROD_LOTID"));
                            dr["PREDCT_DAY"] = Util.NVC(DataTableConverter.GetValue(dgPredDocv.Rows[i].DataItem, "PREDCT_DAY"));

                            string lthres = Util.NVC(DataTableConverter.GetValue(dgPredDocv.Rows[i].DataItem, "L_THRES"));
                            string uthres = Util.NVC(DataTableConverter.GetValue(dgPredDocv.Rows[i].DataItem, "U_THRES"));

                            if (!string.IsNullOrEmpty(lthres)) dr["L_THRES"] = lthres;
                            if (!string.IsNullOrEmpty(uthres)) dr["U_THRES"] = uthres;

                            dr["USERID"] = Util.NVC(DataTableConverter.GetValue(dgPredDocv.Rows[i].DataItem, "UPDUSER"));
                            dr["W_JUDG_TYPE_CODE"] = "G";
                            dr["PASS_YN"] = "N";

                            // 김진섭
                            dtRqst.Rows.Add(dr);
                        }
                    }
                    DataTable dtRslt = new ClientProxy().ExecuteServiceSync("BR_SET_LOW_VOLTAGE_JUDG_DEL_LOT_MB", "RQSTDT", "RSLTDT", dtRqst);
                    Util.Alert("FM_ME_0154"); //삭제완료하였습니다.
                    GetListWJudg();
                });
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }

        private void dgPredDocv_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            try
            {
                Point pnt = e.GetPosition(null);
                C1.WPF.DataGrid.DataGridCell cell = dgPredDocv.GetCellFromPoint(pnt);

                if (cell != null)
                {
                    if (!cell.Column.Name.Equals("CHK"))
                    {
                        return;
                    }

                    if (Util.NVC(cell.Value).ToUpper().Equals("TRUE")) RemoveTrayList(cell.Row.Index);
                    else SearchTrayList(cell.Row.Index);
                }
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);

            }
        }
        private void SearchTrayList(int rowIdx)
        {
            DataTable dtRqst = new DataTable();
            dtRqst.TableName = "RQSTDT";
            dtRqst.Columns.Add("PROD_LOTID", typeof(string));

            DataRow dr = dtRqst.NewRow();
            dr["PROD_LOTID"] = Util.NVC(DataTableConverter.GetValue(dgPredDocv.Rows[rowIdx].DataItem, "PROD_LOTID"));
            dtRqst.Rows.Add(dr);

            DataTable dtRslt = new ClientProxy().ExecuteServiceSync("BR_GET_TRAY_LIST_W_REJUDGE_MB", "RQSTDT", "RSLTDT", dtRqst);

            dtRslt.Columns.Add("REMARK", typeof(string));
            DataTable temp = DataTableConverter.Convert(dgTrayList.ItemsSource);

            temp.Merge(dtRslt);

            if (temp.Rows.Count == 0)
            {
                Util.Alert("FM_ME_0210");  //재판정대상이 없습니다.
                return;
            }

            Util.GridSetData(dgTrayList, temp, this.FrameOperation);

        }
        private void RemoveTrayList(int rowIdx)
        {
            string pkgLot = Util.NVC(DataTableConverter.GetValue(dgPredDocv.Rows[rowIdx].DataItem, "PROD_LOTID"));

            DataTable temp = DataTableConverter.Convert(dgTrayList.ItemsSource);

            for (int i = 0; i < temp.Rows.Count; i++)
            {
                if (temp.Rows[i]["PROD_LOTID"].Equals(pkgLot))
                {
                    temp.Rows.RemoveAt(i);
                    i--;
                }
            }
            if (temp.Rows.Count == 0) Util.gridClear(dgTrayList);
            else Util.GridSetData(dgTrayList, temp, this.FrameOperation);
        }
        private void dgTrayList_LoadedRowHeaderPresenter(object sender, C1.WPF.DataGrid.DataGridRowEventArgs e)
        {
            if (e.Row.HeaderPresenter == null)
            {
                return;
            }

            e.Row.HeaderPresenter.Content = null;

            TextBlock tb = new TextBlock();

            tb.Text = (e.Row.Index + 1 - dgTrayList.TopRows.Count).ToString();
            tb.VerticalAlignment = VerticalAlignment.Center;
            tb.HorizontalAlignment = HorizontalAlignment.Center;
            e.Row.HeaderPresenter.Content = tb;
        }

        private void dgPredDocv_LoadedRowHeaderPresenter(object sender, C1.WPF.DataGrid.DataGridRowEventArgs e)
        {
            if (e.Row.HeaderPresenter == null)
            {
                return;
            }

            e.Row.HeaderPresenter.Content = null;

            TextBlock tb = new TextBlock();

            tb.Text = (e.Row.Index + 1 - dgPredDocv.TopRows.Count).ToString();
            tb.VerticalAlignment = VerticalAlignment.Center;
            tb.HorizontalAlignment = HorizontalAlignment.Center;
            e.Row.HeaderPresenter.Content = tb;
        }
        #endregion

        #region [다중수동판정 tab]


        private void btnSearchMulti_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(txtTrayIDMulti.Text))
            {
                Util.Alert("FM_ME_0070");
            }
            GetListOne();
        }


        private void chkAllJUDG_Checked(object sender, RoutedEventArgs e)
        {
            //foreach (CheckBox itemChecked in lbJUDG.Items)
            //{
            //    itemChecked.IsChecked = true;
            //}
        }

        private void chkAllJUDG_Unchecked(object sender, RoutedEventArgs e)
        {
            //foreach (CheckBox itemChecked in lbJUDG.Items)
            //{
            //    itemChecked.IsChecked = false;
            //}
        }

        //private void btnJudgeMulti_Click(object sender, RoutedEventArgs e)
        //{
        //    try
        //    {
        //        Util.MessageConfirm("FM_ME_0177", (result) => //수동판정을 등록하시겠습니까?
        //        {
        //            if (result != MessageBoxResult.OK) return;

        //            int iFitted = 0;

        //            if ((bool)chkDCIRMulti.IsChecked)
        //                iFitted = iFitted + 1;

        //            if ((bool)chkCapaMulti.IsChecked)
        //                iFitted = iFitted + 2;

        //            string sNotJudgeCell = string.Empty;

        //            if ((bool)chkCellMulti.IsChecked)
        //                sNotJudgeCell = "Y";
        //            else
        //                sNotJudgeCell = "N";



        //            JUDGlist = GetCheckdJUDG();
        //            if (!string.IsNullOrEmpty(GetCheckdJUDG()))
        //                return;
        //            dr["GRADE_CD"] = GetCheckdJUDG();
        //            if (JUDGlist.Length == 0)
        //            {
        //                Util.Alert("FM_ME_0248");  //판정공정을 선택해주세요.
        //                return;
        //            }

        //            return;

        //            xProgressMulti.Visibility = Visibility.Visible;
        //            xProgressMulti.Maximum = JUDGlist.Length;
        //            xProgressMulti.Minimum = 0;
        //            xTextBlockMulti.Text = "0/0";

        //            개별 처리로 변경
        //            for (int i = 0; i < JUDGlist.Length; i++)
        //            {

        //                if (iFitted > 0)
        //                {

        //                    switch (PrevJudgOpFittedCheck(DataTableConverter.GetValue(dgMaintJudgMulti.Rows[0].DataItem, "LOTID").ToString(),
        //                        DataTableConverter.GetValue(dgMaintJudgMulti.Rows[0].DataItem, "ROUTID").ToString(), JUDGlist[i], iFitted))
        //                    {
        //                        case "0":
        //                            break;
        //                        case "1":
        //                            Util.Alert("FM_ME_0005", new string[] { DataTableConverter.GetValue(dgMaintJudgMulti.Rows[0].DataItem, "CSTID").ToString() });  //[Tray ID : {0}] 이전 판정에 Fitted 공정이 포함되어 있습니다.\r\n이전 판정 공정을 확인하세요.
        //                            return;
        //                        case "2":
        //                            Util.Alert("FM_ME_0007", new string[] { DataTableConverter.GetValue(dgMaintJudgMulti.Rows[0].DataItem, "CSTID").ToString() });  //[Tray ID : {0}] 현재 이후 판정 공정 조건에 Fitted 공정이 포함되어 있지 않습니다. 판정 공정을 확인하세요.
        //                            return;
        //                        default:
        //                            Util.Alert("FM_ME_0006", new string[] { DataTableConverter.GetValue(dgMaintJudgMulti.Rows[0].DataItem, "CSTID").ToString() });  //[Tray ID : {0}] 정상 처리되지 않았습니다. 판정 공정을 확인하세요.
        //                            return;
        //                    }
        //                }

        //                progressMulti(i);


        //            }
        //            //대용량 데이터 처리부분 - 대용량 처리용 화면 자체가 개발되어 있지 않으므로 위 개별 처리 로직으로 처리
        //        });
        //    }
        //    catch (Exception ex)
        //    {
        //        LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
        //    }

        //}


        private void txtTrayIDMulti_KeyDown(object sender, KeyEventArgs e)
        {
            if ((!string.IsNullOrEmpty(txtTrayIDMulti.Text)) && (e.Key == Key.Enter))
            {
                btnSearchMulti_Click(null, null);
            }
        }

        private void btnJudgeMulti_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!dgMaintJudgMulti.IsCheckedRow("CHK"))
                {
                    Util.Alert("SFU1645"); //선택된 작업대상이 없습니다.
                    return;
                }

                int iFitted = 0;

                if ((bool)chkDCIRMulti.IsChecked)
                    iFitted = iFitted + 1;

                if ((bool)chkCapaMulti.IsChecked)
                    iFitted = iFitted + 2;

                //JUDGlist = GetCheckdJUDG();
              
                //if (JUDGlist.Length == 0)
                //{
                //    Util.Alert("FM_ME_0248");  //판정공정을 선택해주세요.
                //    return;
                //}
             

                //if (result != MessageBoxResult.OK) return;

                List<DataRow> chkRows = dgMaintJudgMulti.GetCheckedDataRow("CHK");

                object[] argument = new object[3] { chkRows, iFitted, chkCell.IsChecked == true ? "Y" : "N" };

                xProgressMulti.Percent = 0;
                judgeTotalCountMulti = _util.GetDataGridRowCountByCheck(dgMaintJudgMulti, "CHK", true);
                xProgressMulti.ProgressText = "0 / " + judgeTotalCountMulti.Nvc();
                xProgressMulti.Visibility = Visibility.Visible;

                Util.MessageConfirm("FM_ME_0177", (result) => //수동판정을 등록하시겠습니까?
                {
                    if (result != MessageBoxResult.OK) return;

                    btnSearchMulti.IsEnabled = false;
                    btnJudgeMulti.IsEnabled = false;

                    xProgressMulti.RunWorker(argument);
                });
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }

         
        }

        private object xProgressMulti_WorkProcess(object sender, CMM001.Controls.UcProgress.WorkProcessEventArgs e)
        {
            try
            {
                object[] arguments = e.Arguments as object[];

                List<DataRow> checkedRows = arguments[0] as List<DataRow>;

                int iFitted = arguments[1].NvcInt();
                
                string isCheckCell = arguments[2].Nvc();

                int workCount = 0;

                foreach (DataRow checkRow in checkedRows)
                {

                    //for (int i = 0; i < JUDGlist.Length; i++)
                    //{
                        workCount++;

                        string sOp = checkRow["JUDGID"].Nvc();

                        if (iFitted > 0)
                        {
                            switch (PrevJudgOpFittedCheck(checkRow["LOTID"].Nvc(), checkRow["ROUTID"].Nvc(), sOp, iFitted))
                            {
                                case "0":
                                    break;
                                case "1":
                                    //[Tray ID : {0}] 이전 판정에 Fitted 공정이 포함되어 있습니다.\r\n이전 판정 공정을 확인하세요.
                                    return MessageDic.Instance.GetMessage("FM_ME_0005", new string[] { checkRow["CSTID"].Nvc() });
                                case "2":
                                    //[Tray ID : {0}] 현재 이후 판정 공정 조건에 Fitted 공정이 포함되어 있지 않습니다. 판정 공정을 확인하세요.
                                    return MessageDic.Instance.GetMessage("FM_ME_0007", new string[] { checkRow["CSTID"].Nvc() });
                                default:
                                    //[Tray ID : {0}] 정상 처리되지 않았습니다. 판정 공정을 확인하세요.
                                    return MessageDic.Instance.GetMessage("FM_ME_0006", new string[] { checkRow["CSTID"].Nvc() });
                            }
                        }

                        string updateResultText = string.Empty;

                        try
                        {
                            const string bizRuleName = "BR_SET_CELL_JUDGMENT_MANUAL_MB";

                            DataTable dtRqst = new DataTable();
                            dtRqst.Columns.Add("SRCTYPE", typeof(string));
                            dtRqst.Columns.Add("IFMODE", typeof(string));
                            dtRqst.Columns.Add("LOTID", typeof(string));           //TRAY_NO
                            dtRqst.Columns.Add("JUDG_PROCID", typeof(string));     //JUDG_OP_ID
                            dtRqst.Columns.Add("USERID", typeof(string));          //MDF_ID
                            dtRqst.Columns.Add("FITTED_MODE", typeof(string));     //FITTED_MODE
                            dtRqst.Columns.Add("NOT_INIT_GRADE_FLAG", typeof(string)); //NOT_INIT_GRADE
                            dtRqst.Columns.Add("MENUID", typeof(string));
                            dtRqst.Columns.Add("USER_PC_IP", typeof(string));
                            dtRqst.Columns.Add("PC_NAME", typeof(string));

                            DataRow dr = dtRqst.NewRow();
                            dr["SRCTYPE"] = "UI";
                            dr["IFMODE"] = "OFF";
                            dr["LOTID"] = checkRow["LOTID"].Nvc();
                            dr["JUDG_PROCID"] = sOp;
                            dr["USERID"] = LoginInfo.USERID;
                            dr["FITTED_MODE"] = iFitted;
                            dr["NOT_INIT_GRADE_FLAG"] = isCheckCell;
                            dr["MENUID"] = LoginInfo.CFG_MENUID;
                            dr["USER_PC_IP"] = LoginInfo.USER_IP;
                            dr["PC_NAME"] = LoginInfo.PC_NAME;
                            dtRqst.Rows.Add(dr);

                            DataTable bizResult = new ClientProxy().ExecuteServiceSync(bizRuleName, "RQSTDT", "RSLTDT", dtRqst);
                            if (bizResult != null && bizResult.Rows.Count > 0 && bizResult.Rows[0]["RETVAL"].ToString().Equals("0"))
                            {
                                updateResultText = "SUCCESS";
                            }
                            else
                            {
                                updateResultText = "FAIL";
                            }
                        }
                        catch (Exception ex)
                        {
                            updateResultText = "FAIL : " + ex.Message;
                        }
                    object[] progressArgument = new object[2] { workCount.Nvc() + " / " + judgeTotalCountMulti.Nvc(), updateResultText };

                    e.Worker.ReportProgress(Convert.ToInt16((double)workCount / (double)judgeTotalCountMulti * 100), progressArgument);

                    checkRow["REMARK"] = updateResultText;

                    //}
                }
                return "SUCCESS";
            }
            catch (Exception ex)
            {
                return ex;
            }
        }

        private void xProgressMulti_WorkProcessChanged(object sender, int percent, CMM001.Controls.UcProgress.WorkProcessEventArgs e)
        {
            try
            {
                object[] progressArguments = e.Arguments as object[];

                string progressText = progressArguments[0].Nvc();
                string updateText = progressArguments[1].Nvc();

                xProgressMulti.Percent = percent;
                xProgressMulti.ProgressText = progressText;
                xProgressMulti.InvalidateVisual();
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void xProgressMulti_WorkProcessCompleted(object sender, CMM001.Controls.UcProgress.WorkProcessEventArgs e)
        {
            try
            {
                object[] arguments = e.Arguments as object[];

                if (e.Result != null && e.Result is string)
                {
                    if (e.Result.Nvc().Equals("SUCCESS"))
                    {
                        Util.AlertInfo("SFU1889");
                    }
                    else
                    {
                        Util.AlertInfo("[*]" + e.Result.Nvc());
                    }
                }
                else if (e.Result != null && e.Result is Exception)
                {
                    Util.MessageException(e.Result as Exception);
                }
                else
                {
                    string msg = MessageDic.Instance.GetMessage("FM_ME_0202");
                    Util.AlertInfo(msg);  //작업중 오류가 발생하였습니다.
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
            finally
            {
                xProgressMulti.Visibility = Visibility.Collapsed;
                btnSearchMulti.IsEnabled = true;
                btnJudgeMulti.IsEnabled = true;
            }
        }

        private void chkHeaderAllMulti_Checked(object sender, RoutedEventArgs e)
        {
            Util.DataGridCheckAllChecked(dgMaintJudgMulti);
        }

        private void chkHeaderAllMulti_Unchecked(object sender, RoutedEventArgs e)
        {
            Util.DataGridCheckAllUnChecked(dgMaintJudgMulti);
        }

        #endregion

        #region [dOCV mv tab]


        private void btnDSearch2_Click(object sender, RoutedEventArgs e)
        {
            Util.gridClear(dgDocvSpec);
            Util.gridClear(dgDocvProc);

            GetListDOCV();

        }


        private void btnDocv_Click(object sender, RoutedEventArgs e)
        {
            // 수동 계산
            //BR_SET_DOCV_MV_PCT_CALC_MB

            int chkCnt = 0;

            string RoutID = string.Empty;
            string DayGRLotID = string.Empty;
            string ProcID = string.Empty;

            for (int i = 0; i < dgDocvProc.Rows.Count; i++)
            {
                if (DataTableConverter.GetValue(dgDocvProc.Rows[i].DataItem, "CHK").ToString().ToUpper().Equals("TRUE"))
                {
                    RoutID = DataTableConverter.GetValue(dgDocvProc.Rows[i].DataItem, "ROUTID").ToString();
                    DayGRLotID = DataTableConverter.GetValue(dgDocvProc.Rows[i].DataItem, "DAY_GR_LOTID").ToString();
                    ProcID = DataTableConverter.GetValue(dgDocvProc.Rows[i].DataItem, "PROCID").ToString();
                    chkCnt++;
                }
            }

            if (chkCnt > 1)
            {
                // 하나만 선택해주세요
                Util.MessageValidation("FM_ME_0539");
                return;
            }
            if (chkCnt == 0)
            {
                // 판정공정을 선택해주세요
                Util.MessageValidation("FM_ME_0248");
                return;
            }

            try
            {
                DataTable dtRqst = new DataTable();
                dtRqst.TableName = "RQSTDT";
                dtRqst.Columns.Add("SRCTYPE", typeof(string));
                dtRqst.Columns.Add("IFMODE", typeof(string));
                dtRqst.Columns.Add("ROUTID", typeof(string));
                dtRqst.Columns.Add("DAY_GR_LOTID", typeof(string));
                dtRqst.Columns.Add("PROCID", typeof(string));
                dtRqst.Columns.Add("USERID", typeof(string));

                DataRow dr = dtRqst.NewRow();
                dr["SRCTYPE"] = "UI";
                dr["IFMODE"] = "OFF";
                dr["ROUTID"] = RoutID;
                dr["DAY_GR_LOTID"] = DayGRLotID;
                dr["PROCID"] = ProcID;
                dr["USERID"] = LoginInfo.USERID;
                dtRqst.Rows.Add(dr);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("BR_SET_DOCV_MV_PCT_CALC_MB", "RQSTDT", "RSLTDT", dtRqst);

                switch (dtRslt.Rows[0]["RETVAL"].ToString())
                {
                    case "1": // 대상 route 아님
                        Util.MessageValidation("FM_ME_0541");
                        break;
                    case "2": // 사용 공정아님
                        Util.MessageValidation("FM_ME_0542");
                        break;
                    default:// 처리되었습니다
                        Util.MessageValidation("FM_ME_0239");
                        break;

                }
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }

        }

        private void btnDJUDG_Click(object sender, RoutedEventArgs e)
        {
            // 수동 판정
            //BR_SET_DOCV_MV_PCT_JUDGMENT_MANL_MB

            int chkCnt = 0;
            string RoutID = string.Empty;
            string DayGRLotID = string.Empty;

            for (int i = 0; i < dgDocvSpec.Rows.Count; i++)
            {
                if (DataTableConverter.GetValue(dgDocvSpec.Rows[i].DataItem, "CHK").ToString().ToUpper().Equals("TRUE"))
                {
                    RoutID = DataTableConverter.GetValue(dgDocvSpec.Rows[i].DataItem, "ROUTID").ToString();
                    DayGRLotID = DataTableConverter.GetValue(dgDocvSpec.Rows[i].DataItem, "DAY_GR_LOTID").ToString();
                    chkCnt++;
                }
            }

            if (chkCnt > 1)
            {
                // 하나만 선택해주세요
                Util.MessageValidation("FM_ME_0539");
                return;
            }
            if (chkCnt == 0)
            {
                // 선택된 LOT이 없습니다.
                Util.MessageValidation("FM_ME_0162");
                return;
            }
            try
            {
                DataTable dtRqst = new DataTable();
                dtRqst.TableName = "RQSTDT";
                dtRqst.Columns.Add("SRCTYPE", typeof(string));
                dtRqst.Columns.Add("IFMODE", typeof(string));
                dtRqst.Columns.Add("ROUTID", typeof(string));
                dtRqst.Columns.Add("DAY_GR_LOTID", typeof(string));
                dtRqst.Columns.Add("USERID", typeof(string));

                DataRow dr = dtRqst.NewRow();
                dr["SRCTYPE"] = "UI";
                dr["IFMODE"] = "OFF";
                dr["ROUTID"] = RoutID;
                dr["DAY_GR_LOTID"] = DayGRLotID;
                dr["USERID"] = LoginInfo.USERID;
                dtRqst.Rows.Add(dr);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("BR_SET_DOCV_MV_PCT_JUDGMENT_MANL_MB", "RQSTDT", "RSLTDT", dtRqst);

                switch (dtRslt.Rows[0]["RETVAL"].ToString())
                {
                    case "1": // 판정 기준정보 없음
                        Util.MessageValidation("FM_ME_0543");
                        break;
                    case "2": // 측정정보 없음
                        Util.MessageValidation("FM_ME_0544");
                        break;
                    case "3": // 판정대상정보 없음
                        Util.MessageValidation("FM_ME_0545");
                        break;
                    default:// 처리되었습니다
                        Util.MessageValidation("FM_ME_0239");
                        break;

                }

            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }

        }


        private void dgDocvSpec_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            try
            {

                Point pnt = e.GetPosition(null);
                C1.WPF.DataGrid.DataGridCell cell = dgDocvSpec.GetCellFromPoint(pnt);

                if (cell == null)
                {
                    return;
                }
                if (cell != null)
                {
                    if (!cell.Column.Name.Equals("CHK"))
                    {
                        return;
                    }
                }

                DataTableConverter.SetValue(dgDocvSpec.Rows[dRouteidx].DataItem, "CHK", false);

                int idx = cell.Row.Index;

                string RoutID = DataTableConverter.GetValue(dgDocvSpec.Rows[idx].DataItem, "ROUTID").ToString();
                string Prod_LotID = DataTableConverter.GetValue(dgDocvSpec.Rows[idx].DataItem, "DAY_GR_LOTID").ToString();

                GetDOCVProc(RoutID, Prod_LotID);

                dRouteidx = cell.Row.Index;
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);

            }
        }


        private void dgDocvProc_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            try
            {
                Point pnt = e.GetPosition(null);
                C1.WPF.DataGrid.DataGridCell cell = dgDocvProc.GetCellFromPoint(pnt);

                if (cell == null)
                {
                    return;
                }
                if (cell != null)
                {
                    if (!cell.Column.Name.Equals("CHK"))
                    {
                        return;
                    }
                }

                DataTableConverter.SetValue(dgDocvProc.Rows[dProcidx].DataItem, "CHK", false);

                dProcidx = cell.Row.Index;
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);

            }
        }












        #endregion

        #endregion

    
    }
}
