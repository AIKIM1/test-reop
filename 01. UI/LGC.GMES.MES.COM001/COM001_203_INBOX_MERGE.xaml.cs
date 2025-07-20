/*************************************************************************************
 Created Date : 2018.03.06
      Creator : 
   Decription : INBOX 병합
--------------------------------------------------------------------------------------
 [Change History]

**************************************************************************************/

using System;
using System.Windows;
using System.Data;

using System.Windows.Input;

using C1.WPF;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using LGC.GMES.MES.CMM001.Class;
using C1.WPF.DataGrid;
using System.Collections.Generic;
using System.Windows.Controls;
using LGC.GMES.MES.CMM001;
using LGC.GMES.MES.CMM001.Extensions;

using Application = System.Windows.Application;
using DataGridLength = C1.WPF.DataGrid.DataGridLength;
using DataGridRow = C1.WPF.DataGrid.DataGridRow;
using System.Linq;
using System.Windows.Media;


namespace LGC.GMES.MES.COM001
{
    public partial class COM001_203_INBOX_MERGE : C1Window, IWorkArea
    {
        #region Declaration
        Util _Util = new Util();
        BizDataSet _bizRule = new BizDataSet();
     
        private string _CTNR_ID = string.Empty;
        private string _WIP_QLTY_TYPE_CODE = string.Empty;
        private int _INBOX_TYPE_QTY = 0;
        private int _tagPrintCount;
        private bool _load = true;
        private readonly Util _util = new Util();
        private readonly BizDataSet _bizDataSet = new BizDataSet();
        /// <summary>
        ///  Frame과 상호작용하기 위한 객체
        /// </summary>
        public IFrameOperation FrameOperation
        {
            get;
            set;
        }
        #endregion

        #region Initialize

        public COM001_203_INBOX_MERGE()
        {
            InitializeComponent();
        }
        #endregion

        #region Event

        #region [Form Load]
        private void C1Window_Loaded(object sender, RoutedEventArgs e)
        {
            if (_load)
            {
                object[] parameters = C1WindowExtension.GetParameters(this);
                _CTNR_ID = parameters[0] as string; // 대차ID
                _WIP_QLTY_TYPE_CODE = parameters[2] as string; // 품질유형
                if(Util.NVC(_WIP_QLTY_TYPE_CODE) == string.Empty)
                {
                    _WIP_QLTY_TYPE_CODE = "G";
                }
                DefectColumChange(_WIP_QLTY_TYPE_CODE); // 스프레드 컬럼 설정
                DataTable MastInbox = parameters[1] as DataTable; //Inbox 정보
                if (MastInbox == null)
                    return;

                InboxInfo(MastInbox);
                ValidateOpen(MastInbox);
                //Inbox유형에 대한 최대수량
                if (_WIP_QLTY_TYPE_CODE == "G")
                {
                    SetEqptInboxTypeQTY();
                    int sumInboxQty = 0;

                    for (int i=0; i< MastInbox.Rows.Count; i++)
                    {
                        sumInboxQty = sumInboxQty + Convert.ToUInt16(MastInbox.Rows[i]["WIPQTY"].ToString());
                    }
                    if(_INBOX_TYPE_QTY == 0)
                    {
                        Util.MessageValidation("SFU4627");
                        this.DialogResult = MessageBoxResult.No;
                        return;
                    }
                    else if (_INBOX_TYPE_QTY < sumInboxQty)
                    {
                        Util.MessageValidation("SFU4485");
                        this.DialogResult = MessageBoxResult.No;
                        return;
                    }
                }
                _load = false;
            }

        }

        private void InitializeUserControls()
        {
        }
        private void SetControl()
        {
        }

        private void SetControlVisibility()
        {
        }
       
        #endregion

        #region [INBOX 병합]

        private void btnMerge_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidateMerge())
                return;

            // 병합하시겠습니까?
            Util.MessageConfirm("SFU2876", (result) =>
            {
                if (result == MessageBoxResult.OK)
                {
                    MergeInBox();
                }
            });
        }


        #endregion

        #region [닫기]
        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            
            this.DialogResult = MessageBoxResult.Cancel;
        }
        #endregion

        #region [발행]
        private void btnTag_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidationPrint()) return;

            string processName = Util.NVC(DataTableConverter.GetValue(dgInbox_Merge.Rows[0].DataItem, "PROCNAME")).ToString();
            string modelId = Util.NVC(DataTableConverter.GetValue(dgInbox_Merge.Rows[0].DataItem, "MODLID")).ToString();
            string projectName = Util.NVC(DataTableConverter.GetValue(dgInbox_Merge.Rows[0].DataItem, "PRJT_NAME")).ToString();
            string marketTypeName = Util.NVC(DataTableConverter.GetValue(dgInbox_Merge.Rows[0].DataItem, "MKT_TYPE_NAME")).ToString();
            string assyLotId = Util.NVC(DataTableConverter.GetValue(dgInbox_Merge.Rows[0].DataItem, "LOTID_RT")).ToString();
            string calDate = Util.NVC(DataTableConverter.GetValue(dgInbox_Merge.Rows[0].DataItem, "CALDATE")).ToString();
            string shiftName = Util.NVC(DataTableConverter.GetValue(dgInbox_Merge.Rows[0].DataItem, "SHFT_NAME")).ToString();
            string equipmentShortName = Util.NVC(DataTableConverter.GetValue(dgInbox_Merge.Rows[0].DataItem, "EQPTSHORTNAME")).ToString();
            string inspectorId = Util.NVC(DataTableConverter.GetValue(dgInbox_Merge.Rows[0].DataItem, "VISL_INSP_USERNAME")).ToString();



            // 불량 태그(라벨) 항목
            DataTable dtLabelItem = new DataTable();
            dtLabelItem.Columns.Add("LABEL_CODE", typeof(string));     //LABEL CODE
            dtLabelItem.Columns.Add("ITEM001", typeof(string));     //PROCESSNAME
            dtLabelItem.Columns.Add("ITEM002", typeof(string));     //MODEL
            dtLabelItem.Columns.Add("ITEM003", typeof(string));     //PKGLOT
            dtLabelItem.Columns.Add("ITEM004", typeof(string));     //DEFECT
            dtLabelItem.Columns.Add("ITEM005", typeof(string));     //QTY
            dtLabelItem.Columns.Add("ITEM006", typeof(string));     //MKTTYPE
            dtLabelItem.Columns.Add("ITEM007", typeof(string));     //PRODDATE
            dtLabelItem.Columns.Add("ITEM008", typeof(string));     //EQUIPMENT
            dtLabelItem.Columns.Add("ITEM009", typeof(string));     //INSPECTOR
            dtLabelItem.Columns.Add("ITEM010", typeof(string));     //
            dtLabelItem.Columns.Add("ITEM011", typeof(string));     //

            DataTable inTable = _bizDataSet.GetBR_PRD_REG_LABEL_HIST();

            foreach (DataGridRow row in dgInbox_Merge.Rows)
            {

                DataRow dr = dtLabelItem.NewRow();

                if (_WIP_QLTY_TYPE_CODE == "G")
                {
                    dr["LABEL_CODE"] = "LBL0106";
                    dr["ITEM001"] = Util.NVC(DataTableConverter.GetValue(row.DataItem, "CAPA_GRD_CODE"));
                    dr["ITEM002"] = modelId + "(" + projectName + ") ";
                    dr["ITEM003"] = assyLotId;
                    dr["ITEM004"] = Util.NVC_Int(DataTableConverter.GetValue(row.DataItem, "WIPQTY")).GetString();
                    dr["ITEM005"] = equipmentShortName;
                    dr["ITEM006"] = DataTableConverter.GetValue(row.DataItem, "CALDATE").GetString() + "(" + DataTableConverter.GetValue(row.DataItem, "SHFT_NAME").GetString() + ")";
                    dr["ITEM007"] = inspectorId;
                    dr["ITEM008"] = marketTypeName;
                    dr["ITEM009"] = Util.NVC(DataTableConverter.GetValue(row.DataItem, "INBOX_ID"));
                    dr["ITEM010"] = string.Empty;
                    dr["ITEM011"] = string.Empty;

                }
                else
                {
                    dr["LABEL_CODE"] = "LBL0107";
                    dr["ITEM001"] = modelId + "(" + projectName + ") ";
                    dr["ITEM002"] = assyLotId;
                    dr["ITEM003"] = marketTypeName;
                    dr["ITEM004"] = Util.NVC(DataTableConverter.GetValue(row.DataItem, "WIPQTY"));
                    dr["ITEM005"] = equipmentShortName;
                    dr["ITEM006"] = Util.NVC(DataTableConverter.GetValue(row.DataItem, "DFCT_RSN_GR_NAME"));
                    dr["ITEM007"] = calDate + "(" + shiftName + ")";
                    dr["ITEM008"] = inspectorId;
                    dr["ITEM009"] = string.Empty;
                    dr["ITEM010"] = Util.NVC(DataTableConverter.GetValue(row.DataItem, "CAPA_GRD_CODE"));
                    dr["ITEM011"] = Util.NVC(DataTableConverter.GetValue(row.DataItem, "INBOX_ID_DEF")) + "+" +
                                    Util.NVC(DataTableConverter.GetValue(row.DataItem, "RESNGR_ABBR_CODE")) + "+" +
                                    Util.NVC(DataTableConverter.GetValue(row.DataItem, "CAPA_GRD_CODE"));
                }
                dtLabelItem.Rows.Add(dr);

               

            }
            string printType;
            string resolution;
            string issueCount;
            string xposition;
            string yposition;
            string darkness;
            //string portName = dr["PORTNAME"].GetString();
            DataRow drPrintInfo;

            if (!_util.GetConfigPrintInfo(out printType, out resolution, out issueCount, out xposition, out yposition, out darkness, out drPrintInfo))
                return;

            bool isLabelPrintResult = Util.PrintLabelPolymerForm(FrameOperation, loadingIndicator, dtLabelItem, printType, resolution, issueCount, xposition, yposition, darkness, drPrintInfo);

            if (!isLabelPrintResult)
            {
                //라벨 발행중 문제가 발생하였습니다.
                Util.MessageValidation("SFU3243");
            }
            else
            {
                //// 라벨 발행이력 저장
                //new ClientProxy().ExecuteService("BR_PRD_REG_LABEL_PRINT_HIST", "INDATA", null, inTable, (result, searchException) =>
                //{
                //    try
                //    {
                //        if (searchException != null)
                //        {
                //            Util.MessageException(searchException);
                //            return;
                //        }

                //    }
                //    catch (Exception ex)
                //    {
                //        Util.MessageException(ex);
                //    }
                //    finally
                //    {

                //    }
                //});

            }
        }
        #endregion

        #region 스프레드
        //스프레드 라디오 버튼 이벤트
        private void dgChk_Checked(object sender, RoutedEventArgs e)
        {
            try
            {
                //(grdResult.Children[0] as UcAssyResult).dtpCaldate.SelectedDataTimeChanged -= OndtpCaldate_SelectedDataTimeChanged;
                RadioButton rb = sender as RadioButton;
                if (rb?.DataContext == null) return;

                if (rb.IsChecked != null)
                {
                    DataRowView drv = rb.DataContext as DataRowView;
                    if (drv != null && ((bool)rb.IsChecked && drv.Row["CHK"].ToString().Equals("0") || Convert.ToBoolean(drv.Row["CHK"]) == false))
                    {
                        int idx = ((DataGridCellPresenter)rb.Parent).Row.Index;

                        for (int i = 0; i < ((DataGridCellPresenter)rb.Parent).DataGrid.Rows.Count; i++)
                        {
                            DataTableConverter.SetValue(((DataGridCellPresenter)rb.Parent).DataGrid.Rows[i].DataItem, "CHK", idx == i);
                        }

                    }
                }

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        #endregion


        #endregion

        #region User Method

        #region [BizCall]

        /// <summary>
        /// INBOX 병합
        /// </summary>
        private void MergeInBox()
        {
            try
            {
                ShowLoadingIndicator();

                DataSet inData = new DataSet();
                int idx = _Util.GetDataGridFirstRowIndexByCheck(dgInbox_Master, "CHK");
                //마스터 정보
                DataTable inDataTable = inData.Tables.Add("INDATA");
                inDataTable.Columns.Add("SRCTYPE", typeof(string));
                inDataTable.Columns.Add("IFMODE", typeof(string));
                inDataTable.Columns.Add("MERGE_INBOX", typeof(string));
                inDataTable.Columns.Add("USERID", typeof(string));

                DataRow row = null;

                row = inDataTable.NewRow();
                row["SRCTYPE"] = "UI";
                row["IFMODE"] = "OFF";
                if(_WIP_QLTY_TYPE_CODE == "G") // 양품일 경우
                {
                    row["MERGE_INBOX"] = DataTableConverter.GetValue(dgInbox_Master.Rows[idx].DataItem, "INBOX_ID").ToString();
                }
                else // 불량
                {
                    row["MERGE_INBOX"] = DataTableConverter.GetValue(dgInbox_Master.Rows[idx].DataItem, "INBOX_ID_DEF").ToString();
                }
                row["USERID"] = LoginInfo.USERID;

                inDataTable.Rows.Add(row);
                //병합 INBOX
                DataTable inLot = inData.Tables.Add("INLOT");
                inLot.Columns.Add("INBOXID", typeof(string));
                for (int i = 0; i < dgInbox_Master.Rows.Count; i++)
                {
                    if (Util.NVC(DataTableConverter.GetValue(dgInbox_Master.Rows[i].DataItem, "CHK")) != "1")
                    {
                        row = inLot.NewRow();
                        if (_WIP_QLTY_TYPE_CODE == "G") // 양품일 경우
                        {
                            row["INBOXID"] = Util.NVC(DataTableConverter.GetValue(dgInbox_Master.Rows[i].DataItem, "INBOX_ID"));
                        }
                        else // 불량
                        {
                            row["INBOXID"] = Util.NVC(DataTableConverter.GetValue(dgInbox_Master.Rows[i].DataItem, "INBOX_ID_DEF"));
                        }
                        inLot.Rows.Add(row);
                    }
                }
                try
                {
                    //병합처리
                    new ClientProxy().ExecuteService_Multi("BR_PRD_REG_MERGE_LOT_MCP", "INDATA,INLOT", null, (Result, ex) =>
                    {
                        if (ex != null)
                        {
                            Util.MessageException(ex);
                            HiddenLoadingIndicator();
                            return;
                        }

                        //병합 INBOX 정보 조회
                        GetMerge_INBOX();



                        //태그발행 버튼 활성화
                        btnTag.IsEnabled = true;
                        btnMerge.IsEnabled = false;

                    }, inData);



                }
                catch (Exception ex)
                {
                    HiddenLoadingIndicator();
                    Util.AlertByBiz("BR_PRD_REG_SPLIT_PALLET", ex.Message, ex.ToString());

                }
            }
            catch (Exception ex)
            {
                HiddenLoadingIndicator();
                Util.MessageException(ex);
            }
            finally
            {
                HiddenLoadingIndicator();
            }
        }
        /// <summary>
        /// INBOX 조회
        /// </summary>
        private void InboxInfo(DataTable Inbox)
        {
            try
            {
                // 마스터 정보  
                Util.GridSetData(dgInbox_Master, Inbox, FrameOperation);
            }
            catch (Exception ex)
            {
                HiddenLoadingIndicator();
                Util.MessageException(ex);
            }
            finally
            {
                HiddenLoadingIndicator();
            }
        }
        #endregion

        #region[[Validation]
        private bool ValidateMerge()
        {
            if (_CTNR_ID == string.Empty)
            {
                // 대차 정보가 없습니다.
                Util.MessageValidation("SFU4365");
                return false;
            }
            DataRow[] drInfo = Util.gridGetChecked(ref dgInbox_Master, "CHK");
            if (drInfo.Count() <= 0)
            {
                Util.MessageValidation("SFU4232");  //대표Lot을 선택하세요.
                return false;
            }

            return true;
        }
        //발행 Valldation
        private bool ValidationPrint()
        {


            if (LoginInfo.CFG_SERIAL_PRINT.Rows.Count < 1)
            {
                Util.MessageValidation("SFU2003"); //프린트 환경 설정값이 없습니다.
                return false;
            }

            var query = (from t in LoginInfo.CFG_SERIAL_PRINT.AsEnumerable()
                         where t.Field<string>("LABELID") == "LBL0106"
                         select t).ToList();

            if (!query.Any())
            {
                Util.MessageValidation("SFU4339"); //프린터 환경설정에 라벨정보 항목이 없습니다.
                return false;
            }

            if (LoginInfo.CFG_SERIAL_PRINT.Rows.Cast<DataRow>().Any(itemRow => string.IsNullOrEmpty(itemRow[CustomConfig.CONFIGTABLE_SERIALPRINTER_LABELID].GetString())))
            {
                Util.MessageValidation("SFU4339"); //프린터 환경설정에 라벨정보 항목이 없습니다.
                return false;
            }

            return true;
        }

        private void ValidateOpen(DataTable MastInbox)
        {
            string ChkLotId_Rt = MastInbox.Rows[0]["LOTID_RT"].ToString();
            string ChkInboxType = MastInbox.Rows[0]["INBOX_TYPE_CODE"].ToString();
            string ChkDeftChk = MastInbox.Rows[0]["DFCT_RSN_GR_ID"].ToString();
            string ChkGrade = MastInbox.Rows[0]["CAPA_GRD_CODE"].ToString();

            for(int i=0; i< MastInbox.Rows.Count; i++)
            {
                //동일한 조립LOT이 아닙니다.
                if (ChkLotId_Rt != MastInbox.Rows[i]["LOTID_RT"].ToString())
                {
                    Util.MessageValidation("SFU4146"); //동일한 조립LOT이 아닙니다.
                    this.DialogResult = MessageBoxResult.No;
                }
                //동일한 등급이 아닙니다
                if (ChkGrade != MastInbox.Rows[i]["CAPA_GRD_CODE"].ToString())
                {
                    Util.MessageValidation("SFU4182"); //동일한 등급이 아닙니다.
                    this.DialogResult = MessageBoxResult.No;
                }

                if(_WIP_QLTY_TYPE_CODE == "G")
                {
                    //동일한 INBOX 유형이 아닙니다.
                    if (ChkInboxType != MastInbox.Rows[i]["INBOX_TYPE_CODE"].ToString())
                    {
                        Util.MessageValidation("SFU4514"); //동일한 INBOX 유형이 아닙니다.
                        this.DialogResult = MessageBoxResult.No;
                    }

                }
                else
                {
                    //동일한 불량그룹정보가 아닙니다..
                    if (ChkDeftChk != MastInbox.Rows[i]["DFCT_RSN_GR_ID"].ToString())
                    {
                        Util.MessageValidation("SFU4601"); //동일한 불량그룹정보가 아닙니다.
                        this.DialogResult = MessageBoxResult.No;
                    }


                }

            }



        }





        #endregion

        #region [Func]
        private void ShowLoadingIndicator()
        {
            if (loadingIndicator != null)
                loadingIndicator.Visibility = Visibility.Visible;
        }
        private void HiddenLoadingIndicator()
        {
            if (loadingIndicator != null)
                loadingIndicator.Visibility = Visibility.Collapsed;
        }
        //양품 / 불량에 대한 Spread 컬럼 설정
        public void DefectColumChange(string Qlty_Type)
        {

            if (Qlty_Type == "G")
            {

                //병합전 INBOX 스프레드
                dgInbox_Master.Columns["INBOX_ID"].Visibility = Visibility.Visible;           // INBOX ID
                dgInbox_Master.Columns["INBOX_ID_DEF"].Visibility = Visibility.Collapsed;     // 불량그룹 LOT
                dgInbox_Master.Columns["INBOX_TYPE_NAME"].Visibility = Visibility.Visible;    // INBOX 유형
                dgInbox_Master.Columns["DFCT_RSN_GR_NAME"].Visibility = Visibility.Collapsed;     // 불량그룹명

                //병합후 INBOX 스프레드
                dgInbox_Merge.Columns["INBOX_ID"].Visibility = Visibility.Visible;           // INBOX ID
                dgInbox_Merge.Columns["INBOX_ID_DEF"].Visibility = Visibility.Collapsed;     // 불량그룹 LOT
                dgInbox_Merge.Columns["INBOX_TYPE_NAME"].Visibility = Visibility.Visible;    // INBOX 유형
                dgInbox_Merge.Columns["DFCT_RSN_GR_NAME"].Visibility = Visibility.Collapsed;     // 불량그룹명
            }
            else
            {
                //병합전 INBOX 스프레드
                dgInbox_Master.Columns["INBOX_ID"].Visibility = Visibility.Collapsed;       // INBOX ID
                dgInbox_Master.Columns["INBOX_ID_DEF"].Visibility = Visibility.Visible;     // 불량그룹 LOT
                dgInbox_Master.Columns["INBOX_TYPE_NAME"].Visibility = Visibility.Collapsed;// INBOX 유형
                dgInbox_Master.Columns["DFCT_RSN_GR_NAME"].Visibility = Visibility.Visible;     // 불량그룹명
                //병합후 INBOX 스프레드
                dgInbox_Merge.Columns["INBOX_ID"].Visibility = Visibility.Collapsed;       // INBOX ID
                dgInbox_Merge.Columns["INBOX_ID_DEF"].Visibility = Visibility.Visible;     // 불량그룹 LOT
                dgInbox_Merge.Columns["INBOX_TYPE_NAME"].Visibility = Visibility.Collapsed;// INBOX 유형
                dgInbox_Merge.Columns["DFCT_RSN_GR_NAME"].Visibility = Visibility.Visible;     // 불량그룹명
            }




        }

        //양품일 경우 INBOX 유형 수량
        private void SetEqptInboxTypeQTY()
        {
            try
            {
                DataTable inTable = new DataTable();
                inTable.Columns.Add("AREAID", typeof(string));
                inTable.Columns.Add("PROCID", typeof(string));
                inTable.Columns.Add("INBOX_TYPE_CODE", typeof(string));
                DataRow newRow = inTable.NewRow();
                newRow["AREAID"] = LoginInfo.CFG_AREA_ID;
                newRow["PROCID"] = Util.NVC(DataTableConverter.GetValue(dgInbox_Master.Rows[0].DataItem, "PROCID"));
                newRow["INBOX_TYPE_CODE"] = Util.NVC(DataTableConverter.GetValue(dgInbox_Master.Rows[0].DataItem, "INBOX_TYPE_CODE"));

                inTable.Rows.Add(newRow);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_INBOX_TYPE_QTY", "INDATA", "OUTDATA", inTable);

                if (dtResult != null && dtResult.Rows.Count > 0)
                {
                    _INBOX_TYPE_QTY = Convert.ToInt32(dtResult.Rows[0]["INBOX_LOAD_QTY"].ToString());
                }
            
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }

        }
        // 병합후 병합된 INBOX 정보 조회
        public void GetMerge_INBOX()
        {
            try
            {
                DataTable dtRqst = new DataTable();
                dtRqst.Columns.Add("CTNR_ID", typeof(string));
                dtRqst.Columns.Add("LOTID_RT", typeof(string));
                dtRqst.Columns.Add("INBOX_ID", typeof(string));
                dtRqst.Columns.Add("LANGID", typeof(string));
                DataRow dr = dtRqst.NewRow();

                int idx = _util.GetDataGridCheckFirstRowIndex(dgInbox_Master, "CHK");

                dr["CTNR_ID"] = _CTNR_ID;
                dr["LOTID_RT"] = Util.NVC(DataTableConverter.GetValue(dgInbox_Master.Rows[idx].DataItem, "LOTID_RT"));
                dr["INBOX_ID"] = Util.NVC(DataTableConverter.GetValue(dgInbox_Master.Rows[idx].DataItem, "INBOX_ID"));
                dr["LANGID"] = LoginInfo.LANGID;
                dtRqst.Rows.Add(dr);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_POLYMER_CART_INBOX_INFO", "INDATA", "OUTDATA", dtRqst);
                if (dtRslt.Rows.Count > 0)
                {
                    Util.gridClear(dgInbox_Merge);
                    Util.GridSetData(dgInbox_Merge, dtRslt, FrameOperation, false);
                }

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
