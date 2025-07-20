/*************************************************************************************
 Created Date : 2019.07.17
      Creator : 정문교
   Decription : 원자재관리 - PET 자재 라벨 발행 
--------------------------------------------------------------------------------------
 [Change History]


 
**************************************************************************************/

using C1.WPF;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;
using System;
using System.Collections.Generic;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace LGC.GMES.MES.MTRL001
{
    public partial class MTRL001_008 : UserControl, IWorkArea
    {
        #region Declaration & Constructor 

        private Util _Util = new Util();

        public IFrameOperation FrameOperation
        {
            get;
            set;
        }

        public MTRL001_008()
        {
            InitializeComponent();
        }

        #endregion

        #region Initialize

        private void InitializeControls()
        {
            cboMtrlID.SelectedIndex = 0;
            cboSupplier.SelectedIndex = 0;
            txtMtrlQty.Value = 0;
        }

        private void InitializeGrid()
        {
            Util.gridClear(dgList);
        }

        /// <summary>
        /// 화면내 combo 셋팅
        /// </summary>
        private void InitCombo()
        {
            SetAreaCommonCombo(cboMtrlID, "REQUEST_MTRLID");
            SetAreaCommonCombo(cboSupplier, "MTRL_SUPPLIER");
            SetAreaCommonCombo(cboMtrlIDHis, "REQUEST_MTRLID");
        }

        private void SetControl()
        {
        }

        #endregion

        #region Event

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            //사용자 권한별로 버튼 숨기기
            List<Button> listAuth = new List<Button>();
            listAuth.Add(btnPrint);
            Util.pageAuth(listAuth, FrameOperation.AUTHORITY);

            InitializeControls();
            InitializeGrid();
            InitCombo();
            SetControl();

            this.Loaded -= UserControl_Loaded;
        }

        /// <summary>
        /// 라벨 발행
        /// </summary>
        private void btnPrint_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidationPrint())
                return;

            LableSetting();
        }

        /// <summary>
        /// 발행이력 체크 Click
        /// </summary>
        private void dgList_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            Point pnt = e.GetPosition(null);
            C1.WPF.DataGrid.DataGridCell cell = dgList.GetCellFromPoint(pnt);

            if (cell != null)
            {
                if (!cell.Column.Name.Equals("CHK"))
                {
                    return;
                }

                int row = cell.Row.Index;
                DataTable dt = DataTableConverter.Convert(dgList.ItemsSource);

                if (dt.Rows[row]["CHK"].Equals(1))
                    dt.Rows[row]["CHK"] = 0;
                else
                    dt.Rows[row]["CHK"] = 1;

                dt.AcceptChanges();
                Util.GridSetData(dgList, dt, null, true);
            }

        }

        /// <summary>
        /// 라벨 재발행
        /// </summary>
        private void btnPrintHis_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidationRePrint())
                return;

            LableSetting(true);
        }

        /// <summary>
        /// 라벨 이력조회
        /// </summary>
        private void btnSearchHis_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidationSearch())
                return;

            SearchProcess();

        }

        #endregion

        #region Mehod

        #region [BizCall]

        /// <summary>
        /// 자재코드, Vender 콤보
        /// </summary>
        private void SetAreaCommonCombo(C1ComboBox cbo, string ComTypeCode)
        {
            const string bizRuleName = "DA_PRD_SEL_TB_MMD_AREA_COM_CODE";
            string[] arrColumn = { "LANGID", "AREAID", "COM_TYPE_CODE", "COM_CODE" };
            string[] arrCondition = { LoginInfo.LANGID, LoginInfo.CFG_AREA_ID, ComTypeCode, null };
            string selectedValueText = cbo.SelectedValuePath;
            string displayMemberText = cbo.DisplayMemberPath;

            CommonCombo.CommonBaseCombo(bizRuleName, cbo, arrColumn, arrCondition, CommonCombo.ComboStatus.SELECT, selectedValueText, displayMemberText);
            cbo.SelectedIndex = 0;
        }

        /// <summary>
        /// 라벨 발행, 재발행
        /// </summary>
        private void LableSetting(bool bReprint = false)
        {
            // 라벨 발행 저장
            DataTable inTable = new DataTable();
            inTable.Columns.Add("LABEL_PRT_SEQNO", typeof(string));
            inTable.Columns.Add("LABEL_CODE", typeof(string));
            inTable.Columns.Add("PRT_ITEM01", typeof(string));
            inTable.Columns.Add("PRT_ITEM02", typeof(string));
            inTable.Columns.Add("PRT_ITEM03", typeof(string));
            inTable.Columns.Add("PRT_ITEM06", typeof(string));
            inTable.Columns.Add("LABEL_PRT_COUNT", typeof(Int16));
            inTable.Columns.Add("USERID", typeof(string));

            // 태그(라벨) 항목
            DataTable dtLabelItem = new DataTable();
            dtLabelItem.Columns.Add("LABEL_CODE", typeof(string));  // LABEL CODE
            dtLabelItem.Columns.Add("ITEM001", typeof(string));     // 자재LOT
            dtLabelItem.Columns.Add("ITEM002", typeof(string));     // 자재ID
            dtLabelItem.Columns.Add("ITEM003", typeof(string));     // Vendor Code
            dtLabelItem.Columns.Add("ITEM004", typeof(string));     // QTY(수량)
            dtLabelItem.Columns.Add("ITEM005", typeof(string));     // 
            dtLabelItem.Columns.Add("ITEM006", typeof(string));     // 
            dtLabelItem.Columns.Add("ITEM007", typeof(string));     // 
            dtLabelItem.Columns.Add("ITEM008", typeof(string));     // 
            dtLabelItem.Columns.Add("ITEM009", typeof(string));     // 
            dtLabelItem.Columns.Add("ITEM010", typeof(string));     //
            dtLabelItem.Columns.Add("ITEM011", typeof(string));     // 

            if (bReprint)
            {
                DataRow[] drRePrint = DataTableConverter.Convert(dgList.ItemsSource).Select("CHK = 1");
                foreach (DataRow row in drRePrint)
                {
                    DataRow dr = dtLabelItem.NewRow();
                    dr["LABEL_CODE"] = "LBL0188";
                    dr["ITEM001"] = row["LOTID"].ToString();
                    dr["ITEM002"] = row["MTRLDESC"].ToString();
                    dr["ITEM003"] = row["VENDORNAME"].ToString();
                    dr["ITEM004"] = row["MTRL_QTY"].ToString();
                    dr["ITEM005"] = null;
                    dr["ITEM006"] = null;
                    dr["ITEM007"] = null;
                    dr["ITEM008"] = null;
                    dr["ITEM009"] = null;
                    dr["ITEM010"] = null;
                    dr["ITEM011"] = null;
                    dtLabelItem.Rows.Add(dr);

                    DataRow newRow = inTable.NewRow();
                    newRow["LABEL_PRT_SEQNO"] = row["LABEL_PRT_SEQNO"].ToString();
                    newRow["LABEL_PRT_COUNT"] = string.IsNullOrWhiteSpace(row["LABEL_PRT_COUNT"].ToString()) ? 0: int.Parse(row["LABEL_PRT_COUNT"].ToString());
                    newRow["USERID"] = LoginInfo.USERID;
                    inTable.Rows.Add(newRow);
                }

                ShowLoadingIndicator();

                // 라벨 출력
                PrintLable(dtLabelItem);

                // 라벨 재발행 이력 저장
                new ClientProxy().ExecuteService("BR_MTR_REG_MLOT_LABEL", "INDATA", "OUTDATA", inTable, (bizResult, bizException) =>
                {
                    try
                    {
                        HiddenLoadingIndicator();

                        if (bizException != null)
                        {
                            Util.MessageException(bizException);
                            return;
                        }

                        // 재조회
                        SearchProcess();

                    }
                    catch (Exception ex)
                    {
                        HiddenLoadingIndicator();
                        Util.MessageException(ex);
                    }
                });
            }
            else
            {
                // 라벨 발행 이력에 저장 후 라벨 발행

                // INDATA SET
                DataRow newRow = inTable.NewRow();
                newRow["LABEL_CODE"] = "LBL0188";
                newRow["PRT_ITEM01"] = cboMtrlID.SelectedValue.ToString();
                newRow["PRT_ITEM02"] = cboSupplier.SelectedValue.ToString().Equals("SELECT") ? "" : cboSupplier.SelectedValue.ToString();
                newRow["PRT_ITEM03"] = "";
                newRow["PRT_ITEM06"] = txtMtrlQty.Value;
                newRow["USERID"] = LoginInfo.USERID;
                inTable.Rows.Add(newRow);

                ShowLoadingIndicator();

                new ClientProxy().ExecuteService("BR_MTR_REG_MLOT_LABEL", "INDATA", "OUTDATA", inTable, (bizResult, bizException) =>
                {
                    try
                    {
                        HiddenLoadingIndicator();

                        if (bizException != null)
                        {
                            Util.MessageException(bizException);
                            return;
                        }

                        if (bizResult.Rows.Count == 0)
                        {
                            //라벨 발행중 문제가 발생하였습니다.
                            Util.MessageValidation("SFU3243");
                            return;
                        }

                        DataRow dr = dtLabelItem.NewRow();
                        dr["LABEL_CODE"] = "LBL0188";
                        dr["ITEM001"] = bizResult.Rows[0]["MLOTID"].ToString();
                        dr["ITEM002"] = cboMtrlID.Text;
                        dr["ITEM003"] = cboSupplier.SelectedValue.ToString().Equals("SELECT") ? "" : cboSupplier.Text;
                        dr["ITEM004"] = txtMtrlQty.Value;
                        dr["ITEM005"] = null;
                        dr["ITEM006"] = null;
                        dr["ITEM007"] = null;
                        dr["ITEM008"] = null;
                        dr["ITEM009"] = null;
                        dr["ITEM010"] = null;
                        dr["ITEM011"] = null;
                        dtLabelItem.Rows.Add(dr);

                        // 라벨 출력
                        PrintLable(dtLabelItem);

                        // 정상 처리 되었습니다
                        Util.MessageInfo("SFU1889");

                        InitializeControls();
                    }
                    catch (Exception ex)
                    {
                        HiddenLoadingIndicator();
                        Util.MessageException(ex);
                    }
                });
            }

        }

        /// <summary>
        /// 발행이력 조회
        /// </summary>
        private void SearchProcess()
        {
            try
            {
                // Clear
                InitializeGrid();

                DataTable inTable = new DataTable();
                inTable.Columns.Add("LANGID", typeof(string));
                inTable.Columns.Add("AREAID", typeof(string));
                inTable.Columns.Add("FROM_DATE", typeof(string));
                inTable.Columns.Add("TO_DATE", typeof(string));
                inTable.Columns.Add("MTRLID", typeof(string));

                // INDATA SET
                DataRow newRow = inTable.NewRow();
                newRow["LANGID"] = LoginInfo.LANGID;
                newRow["AREAID"] = LoginInfo.CFG_AREA_ID;
                newRow["FROM_DATE"] = Util.GetCondition(dtpDateFromHis);
                newRow["TO_DATE"] = Util.GetCondition(dtpDateToHis);
                newRow["MTRLID"] = cboMtrlIDHis.SelectedValue.ToString();
                inTable.Rows.Add(newRow);

                ShowLoadingIndicator();

                new ClientProxy().ExecuteService("DA_MTR_SEL_MLOT_LABEL_HIST", "INDATA", "OUTDATA", inTable, (bizResult, bizException) =>
                {
                    try
                    {
                        HiddenLoadingIndicator();

                        if (bizException != null)
                        {
                            Util.MessageException(bizException);
                            return;
                        }

                        Util.GridSetData(dgList, bizResult, FrameOperation, true);
                    }
                    catch (Exception ex)
                    {
                        HiddenLoadingIndicator();
                        Util.MessageException(ex);
                    }
                });
            }
            catch (Exception ex)
            {
                HiddenLoadingIndicator();
                Util.MessageException(ex);
            }
        }

        #endregion

        #region Function

        #region [Validation]
        private bool ValidationPrint()
        {
            if (cboMtrlID.SelectedValue == null || cboMtrlID.SelectedValue.ToString().Equals("SELECT"))
            {
                // %1(을)를 선택하세요.
                Util.MessageValidation("SFU4925", ObjectDic.Instance.GetObjectName("자재"));
                return false;
            }

            if (txtMtrlQty.Value.ToString() == "NaN" || txtMtrlQty.Value == 0)
            {
                // % 1이 입력되지 않았습니다.
                Util.MessageValidation("SFU1299", ObjectDic.Instance.GetObjectName("자재수량"));
                return false;
            }

            return true;
        }

        private bool ValidationRePrint()
        {
            int rowIndex = _Util.GetDataGridFirstRowIndexWithTopRow(dgList, "CHK");

            if (rowIndex < 0)
            {
                // 선택된 대상이 없습니다.
                Util.MessageValidation("SFU1636");
                return false;
            }

            return true;
        }


        private bool ValidationSearch()
        {
            if (cboMtrlIDHis.SelectedValue == null || cboMtrlIDHis.SelectedValue.ToString().Equals("SELECT"))
            {
                // %1(을)를 선택하세요.
                Util.MessageValidation("SFU4925", ObjectDic.Instance.GetObjectName("자재"));
                return false;
            }

            if ((dtpDateToHis.SelectedDateTime - dtpDateFromHis.SelectedDateTime).TotalDays > 31)
            {
                // 기간은 {0}일 이내 입니다.
                Util.MessageValidation("SFU2042", "31");
                return false;
            }

            return true;
        }

        #endregion

        #region [팝업]
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

        private void PrintLable(DataTable dt)
        {
            string printType;
            string resolution;
            string issueCount;
            string xposition;
            string yposition;
            string darkness;
            DataRow drPrintInfo;

            if (!_Util.GetConfigPrintInfo(out printType, out resolution, out issueCount, out xposition, out yposition, out darkness, out drPrintInfo))
                return;

            bool isLabelPrintResult = Util.PrintLabelMLOT(FrameOperation, loadingIndicator, dt, printType, resolution, issueCount, xposition, yposition, darkness, drPrintInfo);
            if (!isLabelPrintResult)
            {
                //라벨 발행중 문제가 발생하였습니다.
                Util.MessageValidation("SFU3243");
                return;
            }

        }

        #endregion

        #endregion

        #endregion

    }
}
