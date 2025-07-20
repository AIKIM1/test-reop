/*************************************************************************************
 Created Date : 2021.12.25
      Creator : INS 오화백K
   Decription : STK 재작업 - H/F Cell 입고처리 팝업
--------------------------------------------------------------------------------------
 [Change History]
  2021.12.25  INS 오화백K : Initial Created.
  
**************************************************************************************/


using C1.WPF;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using System;
using System.Collections.Generic;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using C1.WPF.Excel;
using LGC.GMES.MES.CMM001.Extensions;
using Microsoft.Win32;
using System.Configuration;
using System.IO;
using System.Linq;
using C1.WPF.DataGrid;


namespace LGC.GMES.MES.ASSY004
{
    /// <summary>
    /// ASSY004_061_INPUT_HF_CELL.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class ASSY004_061_INPUT_HF_CELL : C1Window, IWorkArea
    {        
        #region Declaration & Constructor 
        private string _LineID = string.Empty;
        private string _EqptID = string.Empty;
        private BizDataSet _Biz = new BizDataSet();
        private Util _Util = new Util();

        private DataTable resultTable;
        private DataRow tmpDataRow;
        #endregion

        #region Initialize 
        /// <summary>
        ///  Frame과 상호작용하기 위한 객체
        /// </summary>
        public IFrameOperation FrameOperation
        {
            get;
            set;
        }
        public ASSY004_061_INPUT_HF_CELL()
        {
            InitializeComponent();
        }

        private void InitializeResultTable()
        {
            if (resultTable == null)
            {
                resultTable = new DataTable();
                resultTable.Columns.Add("CHK", typeof(bool));
                resultTable.Columns.Add("LOTID", typeof(string));
                resultTable.Columns.Add("CSTID", typeof(string));
                resultTable.Columns.Add("LOTYNAME", typeof(string));
                resultTable.Columns.Add("LOTID_RT", typeof(string));
                resultTable.Columns.Add("PRJT_NAME", typeof(string));
                resultTable.Columns.Add("PRODID", typeof(string));
                resultTable.Columns.Add("PRODNAME", typeof(string));
                resultTable.Columns.Add("PRDT_CLSS_CODE", typeof(string));
                resultTable.Columns.Add("WIPSNAME", typeof(string));
                resultTable.Columns.Add("WIPQTY", typeof(string));
                resultTable.Columns.Add("UNIT_CODE", typeof(string));
                resultTable.Columns.Add("WIPHOLD", typeof(string));
                resultTable.Columns.Add("LOTDTTM_CR", typeof(string));
                resultTable.Columns.Add("WIPSEQ", typeof(string));
            }
        }
        #endregion

        #region Event

        private void C1Window_Loaded(object sender, RoutedEventArgs e)
        {
            object[] tmps = C1WindowExtension.GetParameters(this);

            if (tmps != null && tmps.Length >= 2)
            {
                _LineID = Util.NVC(tmps[0]);
                _EqptID = Util.NVC(tmps[1]);
            }
            else
            {
                _LineID = "";
                _EqptID = "";
            }
            ApplyPermissions();
            InitializeResultTable();

        }

        private void C1Window_Initialized(object sender, EventArgs e)
        {
            
        }
  

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = MessageBoxResult.Cancel;
        }

   

        private void btnMove_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidationProcessMove()) return;
            // 입고처리 하시겠습니까?
            Util.MessageConfirm("SFU4589", (result) =>
            {
                if (result == MessageBoxResult.OK)
                {
                    try
                    {
                        ShowLoadingIndicator();

                        DataSet inDataSet = new DataSet();

                        DataTable inDataTable = inDataSet.Tables.Add("INDATA");
                        inDataTable.Columns.Add("SRCTYPE", typeof(string));
                        inDataTable.Columns.Add("IFMODE", typeof(string));
                        inDataTable.Columns.Add("PROCID", typeof(string));
                        inDataTable.Columns.Add("EQPTID", typeof(string));
                        inDataTable.Columns.Add("USERID", typeof(string));
                        inDataTable.Columns.Add("PROCID_TO", typeof(string));
                        inDataTable.Columns.Add("EQSGID_TO", typeof(string));
                        inDataTable.Columns.Add("FLOWNORM", typeof(string));

                        DataTable inLot = inDataSet.Tables.Add("INLOT");
                        inLot.Columns.Add("LOTID", typeof(string));
                        inLot.Columns.Add("WIPNOTE", typeof(string));

                        DataRow inRow = inDataTable.NewRow();
                        inRow["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                        inRow["IFMODE"] = IFMODE.IFMODE_OFF;
                        inRow["PROCID"] = Process.STACKING_FOLDING;
                        inRow["EQPTID"] = _EqptID;
                        inRow["USERID"] = LoginInfo.USERID;

                        inRow["PROCID_TO"] = Process.RWK_LNS;
                        inRow["EQSGID_TO"] = _LineID;
                        inRow["FLOWNORM"] = "N";

                        inDataTable.Rows.Add(inRow);
                        inRow = null;

                        for (int i = 0; i < dgWaitLot.Rows.Count; i++)
                        {
                            inRow = inLot.NewRow();
                            inRow["LOTID"] = Util.NVC(DataTableConverter.GetValue(dgWaitLot.Rows[i].DataItem, "LOTID"));
                            inRow["WIPNOTE"] = string.Empty;

                            inLot.Rows.Add(inRow);
                        }

                        if (inLot.Rows.Count < 1)
                            return;

                        new ClientProxy().ExecuteService_Multi("BR_PRD_RWK_HC_INPUT", "INDATA,INLOT", null, (bizResult, searchException) =>
                        {
                            try
                            {
                                if (searchException != null)
                                {
                                    Util.MessageException(searchException);
                                    return;
                                }

                                //Util.AlertInfo("정상 처리 되었습니다.");
                                Util.MessageInfo("SFU1275");

                                InitializeResultTable();
                                tmpDataRow = null;
                                resultTable.Clear();

                                Util.gridClear(dgWaitLot);

                            }
                            catch (Exception ex)
                            {
                                Util.MessageException(ex);
                            }
                            finally
                            {
                                HiddenLoadingIndicator();
                            }
                        }, inDataSet);
                    }
                    catch (Exception ex)
                    {
                        Util.MessageException(ex);
                    }
                    finally
                    {
                        HiddenLoadingIndicator();
                    }
                }
            });
        }


        private void txtWaitPancakeLot_KeyDown(object sender, KeyEventArgs e)
        {
            try
            {
                if (e.Key == Key.Enter)
                {
                    if (txtWaitPancakeLot.Text == string.Empty)
                        return;

                    if (!CanAddToRow(resultTable, txtWaitPancakeLot, "LOTID"))
                        return;

                    if (!CanAddToRow_MG(resultTable, txtWaitPancakeLot, "CSTID"))
                        return;
                    GetWaitLot();
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        #endregion

        #region Mehod

        #region [BizCall]
        private void GetWaitLot()
        {
            try
            {
                ShowLoadingIndicator();

                DataTable inTable = new DataTable();
                inTable.Columns.Add("LANGID", typeof(string));
                inTable.Columns.Add("PROCID", typeof(string));
                inTable.Columns.Add("LOTID", typeof(string));
                inTable.Columns.Add("CSTID", typeof(string));

                DataRow newRow = inTable.NewRow();
                newRow["LANGID"] = LoginInfo.LANGID;
                newRow["PROCID"] = Process.STACKING_FOLDING;
                newRow["LOTID"] = txtWaitPancakeLot == null ? "" : txtWaitPancakeLot.Text;
                newRow["CSTID"] = txtWaitPancakeLot == null ? "" : txtWaitPancakeLot.Text;
                inTable.Rows.Add(newRow);


                new ClientProxy().ExecuteService("DA_PRD_SEL_WAIT_LOT_LIST_BY_LINE_STKREWORK", "INDATA", "OUTDATA", inTable, (searchResult, searchException) =>
                {
                    try
                    {
                        if (searchException != null)
                        {
                            Util.MessageException(searchException);
                            return;
                        }

                        //데이터 없는 경우
                        if (searchResult.Rows.Count == 0)
                        {
                            object[] parameters = new object[1];
                            parameters[0] = txtWaitPancakeLot.Text.Trim();
                            MessageValidationWithCallBack("SFU7000", (result) =>
                            {
                                txtWaitPancakeLot.Focus();
                            }, parameters); //LOTID[%1]에 해당하는 LOT이 없습니다.

                            return;
                        }
                        tmpDataRow = null;
                        tmpDataRow = resultTable.NewRow();
                        tmpDataRow["CHK"] = false;
                        tmpDataRow["LOTID"] = Util.NVC(searchResult.Rows[0]["LOTID"]);
                        tmpDataRow["CSTID"] = Util.NVC(searchResult.Rows[0]["CSTID"]);
                        tmpDataRow["LOTYNAME"] = Util.NVC(searchResult.Rows[0]["LOTYNAME"]);
                        tmpDataRow["LOTID_RT"] = Util.NVC(searchResult.Rows[0]["LOTID_RT"]);
                        tmpDataRow["PRJT_NAME"] = Util.NVC(searchResult.Rows[0]["PRJT_NAME"]);
                        tmpDataRow["PRODID"] = Util.NVC(searchResult.Rows[0]["PRODID"]);
                        tmpDataRow["PRODNAME"] = Util.NVC(searchResult.Rows[0]["PRODNAME"]);
                        tmpDataRow["PRDT_CLSS_CODE"] = Util.NVC(searchResult.Rows[0]["PRDT_CLSS_CODE"]);
                        tmpDataRow["WIPSNAME"] = Util.NVC(searchResult.Rows[0]["WIPSNAME"]);
                        tmpDataRow["WIPQTY"] = Util.NVC(searchResult.Rows[0]["WIPQTY"]);
                        tmpDataRow["UNIT_CODE"] = Util.NVC(searchResult.Rows[0]["UNIT_CODE"]);
                        tmpDataRow["WIPHOLD"] = Util.NVC(searchResult.Rows[0]["WIPHOLD"]);
                        tmpDataRow["LOTDTTM_CR"] = Util.NVC(searchResult.Rows[0]["LOTDTTM_CR"]);
                        tmpDataRow["WIPSEQ"] = Util.NVC(searchResult.Rows[0]["WIPSEQ"]);
                        resultTable.Rows.Add(tmpDataRow);

                        txtWaitPancakeLot.Clear();
                       Util.GridSetData(dgWaitLot, resultTable, this.FrameOperation);
                    }
                    catch (Exception ex)
                    {
                        Util.MessageException(ex);
                    }
                    finally
                    {
                        HiddenLoadingIndicator();
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

        #region [Func]
        private void ApplyPermissions()
        {
            List<Button> listAuth = new List<Button>();
            //listAuth.Add(btn);

            Util.pageAuth(listAuth, FrameOperation.AUTHORITY);
        }

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
        private bool ValidationProcessMove()
        {
            if (dgWaitLot.Rows.Count == 0)
            {
                Util.MessageValidation("SFU3537"); // 조회된 데이터가 없습니다
                return false;
            }
       
          
            return true;
        }


        private bool CanAddToRow(DataTable dt, TextBox txt, string value)
        {
            bool canAdd = true;

            foreach (DataRowView drv in dt.DefaultView)
            {
                if (txt.Text.Trim().Equals(Util.NVC(DataTableConverter.GetValue(drv, value))))
                {
                    canAdd = false;

                    object[] parameters = new object[1];
                    if (value.Equals("LOTID"))
                        parameters[0] = ObjectDic.Instance.GetObjectName("LOTID") + "[" + Util.NVC(txt.Text) + "]";

                

                    MessageValidationWithCallBack("SFU3471", (result) =>
                    {
                        txt.Clear();
                        txt.Focus();
                    }, parameters);

                    break;
                }
            }

            return canAdd;
        }

        private bool CanAddToRow_MG(DataTable dt, TextBox txt, string value)
        {
            bool canAdd = true;

            foreach (DataRowView drv in dt.DefaultView)
            {
                if (txt.Text.Trim().Equals(Util.NVC(DataTableConverter.GetValue(drv, value))))
                {
                    canAdd = false;

                    object[] parameters = new object[1];
                    if (value.Equals("CSTID"))
                        parameters[0] = ObjectDic.Instance.GetObjectName("CSTID") + "[" + Util.NVC(txt.Text) + "]";

                   
                    MessageValidationWithCallBack("SFU3471", (result) =>
                    {
                        txt.Clear();
                        txt.Focus();
                    }, parameters);

                    break;
                }
            }

            return canAdd;
        }
        public static void MessageValidationWithCallBack(string messageId, Action<MessageBoxResult> callback, params object[] parameters)
        {
            string message = MessageDic.Instance.GetMessage(messageId);
            message = message.Replace("\\r\\n", Environment.NewLine).Replace("\\n", Environment.NewLine);
            if (message.IndexOf("%1", StringComparison.Ordinal) > -1)
            {
                for (int i = 0; i < parameters.Length; i++)
                {
                    message = message.Replace("%" + (i + 1), parameters[i].ToString());
                }
            }
            else
            {
                message = MessageDic.Instance.GetMessage(messageId, parameters);
            }
            ControlsLibrary.MessageBox.Show(message, "", "Caution", MessageBoxButton.OK, MessageBoxIcon.None, callback);
        }

        private void btnDelRow_Click(object sender, RoutedEventArgs e)
        {
            DataGridCellPresenter presenter = (sender as Button).Parent as DataGridCellPresenter;

            if (presenter == null)
                return;

            int clickedIndex = presenter.Row.Index;
            resultTable.Rows.RemoveAt(clickedIndex);
            Util.GridSetData(presenter.DataGrid, resultTable, this.FrameOperation);
        }

        #endregion

        #endregion











    }
}
