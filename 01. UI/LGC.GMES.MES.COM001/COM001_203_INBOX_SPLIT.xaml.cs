/*************************************************************************************
 Created Date : 2018.01.12
      Creator : 
   Decription : INBOX 분할
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
    public partial class COM001_203_INBOX_SPLIT : C1Window, IWorkArea
    {
        #region Declaration
        Util _Util = new Util();
        BizDataSet _bizRule = new BizDataSet();
     
        private string _CTNR_ID = string.Empty;
        private string _INBOX_ID = string.Empty;
        private string _WIP_QLTY_TYPE_CODE = string.Empty;
        private int _tagPrintCount;

        private DataTable CHANGE_INBOX_DATA = null;

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

        public COM001_203_INBOX_SPLIT()
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
                _INBOX_ID = parameters[1] as string; // INBOX_ID
                _WIP_QLTY_TYPE_CODE = parameters[2] as string; // 품질유형
                if (Util.NVC(_WIP_QLTY_TYPE_CODE) == string.Empty)
                {
                    _WIP_QLTY_TYPE_CODE = "G";
                }
                DefectColumChange(_WIP_QLTY_TYPE_CODE); // 스프레드 컬럼 설정
                InboxInfo();
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

        #region [INBOX 분할]
        private void btnSplit_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidateCancel())
                return;

            // 분할하시겠습니까?
            Util.MessageConfirm("SFU4469", (result) =>
            {
                if (result == MessageBoxResult.OK)
                {
                    SplitInBox();
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


        #endregion

        #region User Method

        #region [BizCall]

        /// <summary>
        /// INBOX 분할
        /// </summary>
        private void SplitInBox()
        {
            try
            {
                ShowLoadingIndicator();

                DataSet inData = new DataSet();

                //마스터 정보
                DataTable inDataTable = inData.Tables.Add("INDATA");
                inDataTable.Columns.Add("SRCTYPE", typeof(string));
                inDataTable.Columns.Add("IFMODE", typeof(string));
                inDataTable.Columns.Add("INBOXID", typeof(string));
                inDataTable.Columns.Add("FROM_INBOX_QTY", typeof(decimal));
                inDataTable.Columns.Add("USERID", typeof(string));

                DataRow row = null;

                row = inDataTable.NewRow();
                row["SRCTYPE"] = "UI";
                row["IFMODE"] = "OFF";
                row["INBOXID"] = DataTableConverter.GetValue(dgInbox_Master.Rows[0].DataItem, "INBOX_ID").ToString();
                row["USERID"] = LoginInfo.USERID;

                inDataTable.Rows.Add(row);
                //분할 INBOX
                DataTable inLot = inData.Tables.Add("INLOT");
                inLot.Columns.Add("ACTQTY", typeof(decimal));
                //inLot.Columns.Add("INBOX_QTY", typeof(decimal));


                for (int i = 0; i < dgInbox_Split.Rows.Count; i++)
                {
                    row = inLot.NewRow();
                    row["ACTQTY"] = Convert.ToDecimal(Util.NVC(DataTableConverter.GetValue(dgInbox_Split.Rows[i].DataItem, "NEW_CELL_QTY")));
                    //row["INBOX_QTY"] = "1";
                    inLot.Rows.Add(row);
                }
                try
                {
                    //Pallet선별처리
                    new ClientProxy().ExecuteService_Multi("BR_PRD_REG_SPLIT_LOT_MCP", "INDATA,INLOT", "OUTDATA", (Result, ex) =>
                    {
                        if (ex != null)
                        {
                            Util.MessageException(ex);
                            HiddenLoadingIndicator();
                            return;
                        }
                        //CHANGE_INBOX_DATA.Rows[0]["NEW_INBOX_ID"] = Result.Tables[1].Rows[0]["SPLIT_LOTID"].ToString();
                        //CHANGE_INBOX_DATA.Rows[0]["NEW_INBOX_ID_DEF"] = Result.Tables[1].Rows[0]["SPLIT_LOTID"].ToString();
                        //분할 대상 INBOX
                        //Util.GridSetData(dgInbox_Master, CHANGE_INBOX_DATA, FrameOperation);
                        //신규 INBOX 정보
                        //Util.GridSetData(dgInbox_Split, CHANGE_INBOX_DATA, FrameOperation);
                        NewInboxInfo(Result.Tables["OUTDATA"]);

                       //txtSplitQty.IsEnabled = false;

                        //태그발행 버튼 활성화
                        btnPrint_Master.IsEnabled = true;
                        btnPrint_Split.IsEnabled = true;

                        //분할 버튼 비활성화
                        btnSplit.IsEnabled = false;

                        btnAdd_Split.IsEnabled = false;
                        btnDel_Split.IsEnabled = false;

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
        private void InboxInfo()
        {
            try
            {
                ShowLoadingIndicator();

                DataTable dtRqst = new DataTable();
                dtRqst.Columns.Add("CTNR_ID", typeof(string));
                dtRqst.Columns.Add("INBOX_ID", typeof(string));
                dtRqst.Columns.Add("LANGID", typeof(string));
                DataRow dr = dtRqst.NewRow();
                dr["CTNR_ID"] = _CTNR_ID;
                dr["INBOX_ID"] = _INBOX_ID;
                dr["LANGID"] = LoginInfo.LANGID;
                dtRqst.Rows.Add(dr);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_POLYMER_CART_INBOX_INFO", "INDATA", "OUTDATA", dtRqst);
                if (dtRslt.Rows.Count > 0)
                {
                    //분할 대상 INBOX
                    Util.GridSetData(dgInbox_Master, dtRslt, FrameOperation);
                    //신규 INBOX 정보
                    //Util.GridSetData(dgInbox_Split, dtRslt, FrameOperation);
                    //변경될 정보를 담기위해서
                    CHANGE_INBOX_DATA = new DataTable();
                    CHANGE_INBOX_DATA = dtRslt.Copy();

                    //txtSplitQty.Focus();
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



        #endregion

        #region[[Validation]
        private bool ValidateCancel()
        {
            if (_CTNR_ID == string.Empty)
            {
                // 대차 정보가 없습니다.
                Util.MessageValidation("SFU4365");
                return false;
            }

            if (dgInbox_Master.Rows.Count == 0)
            {
            
                Util.MessageValidation("SFU4467");
                return false;
            }

            if(Util.NVC(DataTableConverter.GetValue(dgInbox_Split.Rows[0].DataItem, "NEW_CELL_QTY")) == string.Empty)
            {
                
                Util.MessageValidation("SFU4471");
                return false;
            }

            if (Util.NVC(DataTableConverter.GetValue(dgInbox_Split.Rows[0].DataItem, "NEW_CELL_QTY")) == "0")
            {
                
                Util.MessageValidation("SFU4471");
                return false;
            }

            if (Util.NVC(DataTableConverter.GetValue(dgInbox_Split.Rows[0].DataItem, "NEW_CELL_QTY")) == "0")
            {

                Util.MessageValidation("SFU4471");
                return false;
            }
            decimal SumNewCellQty = 0;
            decimal BeforQty = Convert.ToDecimal(DataTableConverter.GetValue(dgInbox_Master.Rows[0].DataItem, "WIPQTY").ToString().Replace(",", ""));
            if (dgInbox_Split.Rows.Count > 0)
            {
                for (int i = 0; i < dgInbox_Split.Rows.Count; i++)
                {
                    SumNewCellQty = SumNewCellQty + Convert.ToDecimal(DataTableConverter.GetValue(dgInbox_Split.Rows[i].DataItem, "NEW_CELL_QTY").ToString() == string.Empty ? "0" : DataTableConverter.GetValue(dgInbox_Split.Rows[i].DataItem, "NEW_CELL_QTY").ToString());
                }
            }
            if (BeforQty < SumNewCellQty)
            {
                //입력Cell 수량의 합이 기존 Cell 수량보다 큽니다
                Util.MessageValidation("SFU4230");
                return false;
            }
            return true;
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
                dgInbox_Split.Columns["NEW_INBOX_ID"].Visibility = Visibility.Visible;           // INBOX ID
                dgInbox_Split.Columns["NEW_INBOX_ID_DEF"].Visibility = Visibility.Collapsed;     // 불량그룹 LOT
                dgInbox_Split.Columns["INBOX_TYPE_NAME"].Visibility = Visibility.Visible;    // INBOX 유형
                dgInbox_Split.Columns["DFCT_RSN_GR_NAME"].Visibility = Visibility.Collapsed;     // 불량그룹명
            }
            else
            {
                //병합전 INBOX 스프레드
                dgInbox_Master.Columns["INBOX_ID"].Visibility = Visibility.Collapsed;       // INBOX ID
                dgInbox_Master.Columns["INBOX_ID_DEF"].Visibility = Visibility.Visible;     // 불량그룹 LOT
                dgInbox_Master.Columns["INBOX_TYPE_NAME"].Visibility = Visibility.Collapsed;// INBOX 유형
                dgInbox_Master.Columns["DFCT_RSN_GR_NAME"].Visibility = Visibility.Visible;     // 불량그룹명
                //병합후 INBOX 스프레드
                dgInbox_Split.Columns["NEW_INBOX_ID"].Visibility = Visibility.Collapsed;       // INBOX ID
                dgInbox_Split.Columns["NEW_INBOX_ID_DEF"].Visibility = Visibility.Visible;     // 불량그룹 LOT
                dgInbox_Split.Columns["INBOX_TYPE_NAME"].Visibility = Visibility.Collapsed;// INBOX 유형
                dgInbox_Split.Columns["DFCT_RSN_GR_NAME"].Visibility = Visibility.Visible;     // 불량그룹명
            }
        }

        #endregion

        #endregion

        private void btnPrint_Master_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidationPrint()) return;
          
            string processName = Util.NVC(DataTableConverter.GetValue(dgInbox_Master.Rows[0].DataItem, "PROCNAME")).ToString();
            string modelId = Util.NVC(DataTableConverter.GetValue(dgInbox_Master.Rows[0].DataItem, "MODLID")).ToString();
            string projectName = Util.NVC(DataTableConverter.GetValue(dgInbox_Master.Rows[0].DataItem, "PRJT_NAME")).ToString();
            string marketTypeName = Util.NVC(DataTableConverter.GetValue(dgInbox_Master.Rows[0].DataItem, "MKT_TYPE_NAME")).ToString();
            string assyLotId = Util.NVC(DataTableConverter.GetValue(dgInbox_Master.Rows[0].DataItem, "LOTID_RT")).ToString();
            string calDate = Util.NVC(DataTableConverter.GetValue(dgInbox_Master.Rows[0].DataItem, "CALDATE")).ToString();
            string shiftName = Util.NVC(DataTableConverter.GetValue(dgInbox_Master.Rows[0].DataItem, "SHFT_NAME")).ToString();
            string equipmentShortName = Util.NVC(DataTableConverter.GetValue(dgInbox_Master.Rows[0].DataItem, "EQPTSHORTNAME")).ToString();
            string inspectorId = Util.NVC(DataTableConverter.GetValue(dgInbox_Master.Rows[0].DataItem, "VISL_INSP_USERNAME")).ToString();



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

            foreach (DataGridRow row in dgInbox_Master.Rows)
            {
              
                    DataRow dr = dtLabelItem.NewRow();
                    if (_WIP_QLTY_TYPE_CODE == "G")
                    {
                        dr["LABEL_CODE"] = "LBL0106";
                        dr["ITEM001"] = Util.NVC(DataTableConverter.GetValue(row.DataItem, "CAPA_GRD_CODE"));
                        dr["ITEM002"] = modelId + "(" + projectName + ") ";
                        dr["ITEM003"] = assyLotId;
                        dr["ITEM004"] = Util.NVC_Int(DataTableConverter.GetValue(row.DataItem, "AFTER_CELL_QTY")).GetString();
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

                    /*
                    dr["LABEL_CODE"] = "LBL0106";
                    dr["ITEM001"] = processName;
                    dr["ITEM002"] = modelId + "(" + projectName + ") ";
                    dr["ITEM003"] = assyLotId;
                    dr["ITEM004"] = Util.NVC(DataTableConverter.GetValue(row.DataItem, "CELL_QTY"));
                    dr["ITEM005"] = marketTypeName;
                    dr["ITEM006"] = calDate + "(" + shiftName + ")";
                    dr["ITEM007"] = equipmentShortName;
                    dr["ITEM008"] = inspectorId;
                    dr["ITEM009"] = Util.NVC(DataTableConverter.GetValue(row.DataItem, "INBOX_ID"));
                    dr["ITEM010"] = Util.NVC(DataTableConverter.GetValue(row.DataItem, "INBOX_ID"));
                    dr["ITEM011"] = Util.NVC(DataTableConverter.GetValue(row.DataItem, "CAPA_GRD_CODE"));
                    */

                    // 라벨 발행 이력 저장
                    //DataRow newRow = inTable.NewRow();
                    //newRow["LABEL_PRT_COUNT"] = 1;                                                             // 발행 수량
                    //newRow["PRT_ITEM01"] = Util.NVC(DataTableConverter.GetValue(row.DataItem, "INBOX_ID"));    // Cell ID
                    //newRow["PRT_ITEM02"] = Util.NVC(DataTableConverter.GetValue(row.DataItem, "WIPSEQ"));
                    //newRow["PRT_ITEM04"] = Util.NVC(DataTableConverter.GetValue(row.DataItem, "PRINT_YN"));                                                // 재발행 여부
                    //newRow["INSUSER"] = LoginInfo.USERID;
                    //newRow["LOTID"] = Util.NVC(DataTableConverter.GetValue(row.DataItem, "INBOX_ID"));
                    //inTable.Rows.Add(newRow);

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
      
        private void btnPrint_Split_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidationPrint()) return;

            string processName = Util.NVC(DataTableConverter.GetValue(dgInbox_Split.Rows[0].DataItem, "PROCNAME")).ToString();
            string modelId = Util.NVC(DataTableConverter.GetValue(dgInbox_Split.Rows[0].DataItem, "MODLID")).ToString();
            string projectName = Util.NVC(DataTableConverter.GetValue(dgInbox_Split.Rows[0].DataItem, "PRJT_NAME")).ToString();
            string marketTypeName = Util.NVC(DataTableConverter.GetValue(dgInbox_Split.Rows[0].DataItem, "MKT_TYPE_NAME")).ToString();
            string assyLotId = Util.NVC(DataTableConverter.GetValue(dgInbox_Split.Rows[0].DataItem, "LOTID_RT")).ToString();
            string calDate = Util.NVC(DataTableConverter.GetValue(dgInbox_Split.Rows[0].DataItem, "CALDATE")).ToString();
            string shiftName = Util.NVC(DataTableConverter.GetValue(dgInbox_Split.Rows[0].DataItem, "SHFT_NAME")).ToString();
            string equipmentShortName = Util.NVC(DataTableConverter.GetValue(dgInbox_Split.Rows[0].DataItem, "EQPTSHORTNAME")).ToString();
            string inspectorId = Util.NVC(DataTableConverter.GetValue(dgInbox_Split.Rows[0].DataItem, "VISL_INSP_USERNAME")).ToString();



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

            foreach (DataGridRow row in dgInbox_Split.Rows)
            {

                DataRow dr = dtLabelItem.NewRow();

                if (_WIP_QLTY_TYPE_CODE == "G")
                {
                    dr["LABEL_CODE"] = "LBL0106";
                    dr["ITEM001"] = Util.NVC(DataTableConverter.GetValue(row.DataItem, "CAPA_GRD_CODE"));
                    dr["ITEM002"] = modelId + "(" + projectName + ") ";
                    dr["ITEM003"] = assyLotId;
                    dr["ITEM004"] = Util.NVC_Int(DataTableConverter.GetValue(row.DataItem, "NEW_CELL_QTY")).GetString();
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
                    dr["ITEM004"] = Util.NVC(DataTableConverter.GetValue(row.DataItem, "NEW_CELL_QTY"));
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

                /*
                dr["LABEL_CODE"] = "LBL0106";
                dr["ITEM001"] = processName;
                dr["ITEM002"] = modelId + "(" + projectName + ") ";
                dr["ITEM003"] = assyLotId;
                dr["ITEM004"] = Util.NVC(DataTableConverter.GetValue(row.DataItem, "CELL_QTY"));
                dr["ITEM005"] = marketTypeName;
                dr["ITEM006"] = calDate + "(" + shiftName + ")";
                dr["ITEM007"] = equipmentShortName;
                dr["ITEM008"] = inspectorId;
                dr["ITEM009"] = Util.NVC(DataTableConverter.GetValue(row.DataItem, "INBOX_ID"));
                dr["ITEM010"] = Util.NVC(DataTableConverter.GetValue(row.DataItem, "INBOX_ID"));
                dr["ITEM011"] = Util.NVC(DataTableConverter.GetValue(row.DataItem, "CAPA_GRD_CODE"));
                */

                // 라벨 발행 이력 저장
                //DataRow newRow = inTable.NewRow();
                //newRow["LABEL_PRT_COUNT"] = 1;                                                             // 발행 수량
                //newRow["PRT_ITEM01"] = Util.NVC(DataTableConverter.GetValue(row.DataItem, "INBOX_ID"));    // Cell ID
                //newRow["PRT_ITEM02"] = Util.NVC(DataTableConverter.GetValue(row.DataItem, "WIPSEQ"));
                //newRow["PRT_ITEM04"] = Util.NVC(DataTableConverter.GetValue(row.DataItem, "PRINT_YN"));                                                // 재발행 여부
                //newRow["INSUSER"] = LoginInfo.USERID;
                //newRow["LOTID"] = Util.NVC(DataTableConverter.GetValue(row.DataItem, "INBOX_ID"));
                //inTable.Rows.Add(newRow);

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

        private void btnAdd_Split_Click(object sender, RoutedEventArgs e)
        {
            DataGridRowAdd(dgInbox_Split);
        }

        private void DataGridRowAdd(C1.WPF.DataGrid.C1DataGrid dg)
        {
            try
            {
                DataTable dt = new DataTable();
             
                if (dg.ItemsSource == null || dg.Rows.Count < 0)
                {
                    dt.Columns.Add("NEW_INBOX_ID", typeof(string));
                    dt.Columns.Add("NEW_INBOX_ID_DEF", typeof(string));
                    dt.Columns.Add("INBOX_TYPE_NAME", typeof(string));
                    dt.Columns.Add("DFCT_RSN_GR_NAME", typeof(string));
                    dt.Columns.Add("CAPA_GRD_CODE", typeof(string));
                    dt.Columns.Add("NEW_CELL_QTY", typeof(string));
                    dt.Columns.Add("MODLID", typeof(string));
                    dt.Columns.Add("MKT_TYPE_NAME", typeof(string));
                    dt.Columns.Add("CALDATE", typeof(string));
                    dt.Columns.Add("SHFT_NAME", typeof(string));
                    dt.Columns.Add("EQPTSHORTNAME", typeof(string));
                    dt.Columns.Add("VISL_INSP_USERNAME", typeof(string));
                    dt.Columns.Add("PRJT_NAME", typeof(string));
                    dt.Columns.Add("LOTID_RT", typeof(string));
                    dt.Columns.Add("PROCNAME", typeof(string));
                    dt.Columns.Add("DFCT_RSN_GR_ID", typeof(string));
                    dt.Columns.Add("RESNGR_ABBR_CODE", typeof(string));
                }
                else
                {
                    //foreach (C1.WPF.DataGrid.DataGridColumn col in dg.Columns)
                    //{
                    //    dt.Columns.Add(Convert.ToString(col.Name));
                    //}
                    dt = DataTableConverter.Convert(dg.ItemsSource);
                }
             

                DataRow dr2 = dt.NewRow();
                dr2["NEW_INBOX_ID"] = "NEW";
                dr2["NEW_INBOX_ID_DEF"] ="NEW";
                dr2["INBOX_TYPE_NAME"] = Util.NVC(DataTableConverter.GetValue(dgInbox_Master.Rows[0].DataItem, "INBOX_TYPE_NAME"));
                dr2["DFCT_RSN_GR_NAME"] = Util.NVC(DataTableConverter.GetValue(dgInbox_Master.Rows[0].DataItem, "DFCT_RSN_GR_NAME"));
                dr2["CAPA_GRD_CODE"] = Util.NVC(DataTableConverter.GetValue(dgInbox_Master.Rows[0].DataItem, "CAPA_GRD_CODE"));
                dr2["NEW_CELL_QTY"] = string.Empty;
                dr2["MODLID"] = Util.NVC(DataTableConverter.GetValue(dgInbox_Master.Rows[0].DataItem, "MODLID"));
                dr2["MKT_TYPE_NAME"] = Util.NVC(DataTableConverter.GetValue(dgInbox_Master.Rows[0].DataItem, "MKT_TYPE_NAME"));
                dr2["CALDATE"] = Util.NVC(DataTableConverter.GetValue(dgInbox_Master.Rows[0].DataItem, "CALDATE"));
                dr2["SHFT_NAME"] = Util.NVC(DataTableConverter.GetValue(dgInbox_Master.Rows[0].DataItem, "SHFT_NAME"));
                dr2["EQPTSHORTNAME"] = Util.NVC(DataTableConverter.GetValue(dgInbox_Master.Rows[0].DataItem, "EQPTSHORTNAME"));
                dr2["VISL_INSP_USERNAME"] = Util.NVC(DataTableConverter.GetValue(dgInbox_Master.Rows[0].DataItem, "VISL_INSP_USERNAME"));
                dr2["PRJT_NAME"] = Util.NVC(DataTableConverter.GetValue(dgInbox_Master.Rows[0].DataItem, "PRJT_NAME"));
                dr2["LOTID_RT"] = Util.NVC(DataTableConverter.GetValue(dgInbox_Master.Rows[0].DataItem, "LOTID_RT"));
                dr2["PROCNAME"] = Util.NVC(DataTableConverter.GetValue(dgInbox_Master.Rows[0].DataItem, "PROCNAME"));
                dr2["DFCT_RSN_GR_ID"] =  Util.NVC(DataTableConverter.GetValue(dgInbox_Master.Rows[0].DataItem, "DFCT_RSN_GR_ID"));
                dr2["RESNGR_ABBR_CODE"] = Util.NVC(DataTableConverter.GetValue(dgInbox_Master.Rows[0].DataItem, "RESNGR_ABBR_CODE"));

                dt.Rows.Add(dr2);
                dt.AcceptChanges();
                //dg.ItemsSource = DataTableConverter.Convert(dt);
                Util.GridSetData(dg, dt, FrameOperation, false);


                // 스프레드 스크롤 하단으로 이동
                dg.ScrollIntoView(dg.GetRowCount() - 1, 0);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return;
            }
        }

        private void btnDel_Split_Click(object sender, RoutedEventArgs e)
        {
            DataGridRowRemove(dgInbox_Split);
        }
        private void DataGridRowRemove(C1.WPF.DataGrid.C1DataGrid dg)
        {
            try
            {
                // 기존 저장자료는 제외
                if (dg.SelectedIndex > -1)
                {
                    DataTable dt = DataTableConverter.Convert(dg.ItemsSource);
                    dt.Rows[dg.SelectedIndex].Delete();
                    dt.AcceptChanges();
                    //dg.ItemsSource = DataTableConverter.Convert(dt);
                    Util.GridSetData(dg, dt, FrameOperation, false);
                    if (dg.Rows.Count == 0)
                    {
                        DataTableConverter.SetValue(dgInbox_Master.Rows[0].DataItem, "AFTER_CELL_QTY", 0);
                        return;
                    }

                    decimal AllWipQty = 0;
                    decimal BeforQty = Convert.ToDecimal(DataTableConverter.GetValue(dgInbox_Master.Rows[0].DataItem, "WIPQTY").ToString().Replace(",", ""));
                    if (dg.Rows.Count > 0)
                    {
                        for (int i = 0; i < dg.Rows.Count; i++)
                        {
                            AllWipQty = AllWipQty + Convert.ToDecimal(DataTableConverter.GetValue(dg.Rows[i].DataItem, "NEW_CELL_QTY").ToString() == string.Empty ? "0" : DataTableConverter.GetValue(dg.Rows[i].DataItem, "NEW_CELL_QTY").ToString());
                       
                        }
                    }
                    if (AllWipQty != 0)
                    {
                        DataTableConverter.SetValue(dgInbox_Master.Rows[0].DataItem, "AFTER_CELL_QTY", BeforQty - AllWipQty);
                    }

                  
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return;
            }
        }
      

        private void dgInbox_Split_KeyDown(object sender, KeyEventArgs e)
        {
            C1.WPF.DataGrid.C1DataGrid grd = (sender as C1.WPF.DataGrid.C1DataGrid);
            int index = 0;
            if (e.Key == Key.Enter)
            {

                if (btnPrint_Split.IsEnabled == true) return;
                if (grd != null &&
                        grd.CurrentCell != null &&
                        grd.CurrentCell.Column != null && grd.CurrentCell.Column.Name.Equals("NEW_CELL_QTY"))
                {
                    //INBOX(LOT) 셋팅
                    DataRow[] drDefectList = DataTableConverter.Convert(dgInbox_Split.ItemsSource).Select();
                    //index 설정해놓음..
                    index = grd.CurrentCell.Row.Index;
                    //분할전 수량
                    decimal BeforQty = Convert.ToDecimal(DataTableConverter.GetValue(dgInbox_Master.Rows[0].DataItem, "WIPQTY").ToString().Replace(",", ""));
                    decimal AfterQty = Convert.ToDecimal(DataTableConverter.GetValue(dgInbox_Master.Rows[0].DataItem, "AFTER_CELL_QTY").ToString().Replace(",", ""));


                    decimal SumNewCellQty = 0;
                    if(dgInbox_Split.Rows.Count > 0)
                    {
                        for(int i=0; i< dgInbox_Split.Rows.Count; i++)
                        {
                            SumNewCellQty = SumNewCellQty + Convert.ToDecimal(DataTableConverter.GetValue(dgInbox_Split.Rows[i].DataItem, "NEW_CELL_QTY").ToString() == string.Empty ? "0" : DataTableConverter.GetValue(dgInbox_Split.Rows[i].DataItem, "NEW_CELL_QTY").ToString());
                        }
                    }

                    if (BeforQty < SumNewCellQty)
                    {
                        //입력Cell 수량의 합이 기존 Cell 수량보다 큽니다
                       
                        LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("SFU4230"), null, "Information", MessageBoxButton.OK, MessageBoxIcon.None, (result) =>
                        {
                            if (result == MessageBoxResult.OK)
                            {
                                DataTableConverter.SetValue(dgInbox_Split.Rows[index].DataItem, "NEW_CELL_QTY", 0);
                            }
                        });
                        return;
                    }
                   
                    DataTableConverter.SetValue(dgInbox_Master.Rows[0].DataItem, "AFTER_CELL_QTY", BeforQty - SumNewCellQty);

                }
            }
        }

        private void dgInbox_Split_LoadedCellPresenter(object sender, DataGridCellEventArgs e)
        {
            if (sender == null)
                return;

            C1DataGrid dg = sender as C1DataGrid;

            dg?.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (e.Cell.Presenter == null)
                {
                    return;
                }

                if (e.Cell.Row.Type == DataGridRowType.Item)
                {
                    if (e.Cell.Column.Name.Equals("NEW_CELL_QTY"))
                    {
                        e.Cell.Presenter.Background = new SolidColorBrush(Colors.Yellow);
                    }
                   
                }
            }));
        }
        

        private void NewInboxInfo(DataTable dt)
        {
            try
            {
                ShowLoadingIndicator();

                DataTable dtRqst = new DataTable();
                dtRqst.Columns.Add("CTNR_ID", typeof(string));
                dtRqst.Columns.Add("INBOX_ID", typeof(string));
                dtRqst.Columns.Add("LANGID", typeof(string));

                DataRow dr = null;
               
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    dr = dtRqst.NewRow();
                    dr["CTNR_ID"] = _CTNR_ID;
                    dr["INBOX_ID"] = dt.Rows[i]["SPLIT_LOTID"].ToString();
                    dr["LANGID"] = LoginInfo.LANGID;
                    dtRqst.Rows.Add(dr);
                }
               

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_POLYMER_CART_SPLIT_NEW_INBOX_INFO", "INDATA", "OUTDATA", dtRqst);
                if (dtRslt.Rows.Count > 0)
                {
                    //신규 INBOX 정보
                    Util.GridSetData(dgInbox_Split, dtRslt, FrameOperation);
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

        private void dgInbox_Split_BeginningEdit(object sender, C1.WPF.DataGrid.DataGridBeginningEditEventArgs e)
        {
            if (e.Column.Name == "NEW_CELL_QTY")
            {
                if (btnPrint_Master.IsEnabled == true)
                {
                    e.Cancel = true;
                }
                else
                {
                    e.Cancel = false;
                }
            }
        }
    }
}
