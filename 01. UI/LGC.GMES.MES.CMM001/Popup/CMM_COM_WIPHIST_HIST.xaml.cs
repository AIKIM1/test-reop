/*************************************************************************************
 Created Date : 2020.02.12
      Creator : INS 김동일K
   Decription : 생산실적 변경이력 조회
--------------------------------------------------------------------------------------
 [Change History]
  2020.02.12  INS 김동일K : Initial Created.
  2023.02.07  성민식      : C20230109-000394 와인더 설비 투입 수량 변경 이력 조회 추가
  
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

namespace LGC.GMES.MES.CMM001.Popup
{
    /// <summary>
    /// CMM_COM_WIPHIST_HIST.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class CMM_COM_WIPHIST_HIST : C1Window, IWorkArea
    {
        #region Declaration & Constructor        

        private string _LOTID = string.Empty;
        private string _WIPSEQ = string.Empty;

        private BizDataSet _Biz = new BizDataSet();
        private Util _Util = new Util();

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
        public CMM_COM_WIPHIST_HIST()
        {
            InitializeComponent();
        }
        #endregion

        #region Event
        private void C1Window_Initialized(object sender, EventArgs e)
        {
            try
            {

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void C1Window_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                object[] tmps = C1WindowExtension.GetParameters(this);

                if (tmps?.Length > 0) _LOTID = Util.NVC(tmps[0]);
                if (tmps?.Length > 1) _WIPSEQ = Util.NVC(tmps[1]);
                                
                ApplyPermissions();

                GetHistory();
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        
        private void btnClose_Clicked(object sender, RoutedEventArgs e)
        {
            this.DialogResult = MessageBoxResult.Cancel;
        }        

        #endregion

        #region Mehod

        #region [BizCall]

        public void GetHistory()
        {
            try
            {
                ShowLoadingIndicator();

                DataTable inTable = new DataTable();
                inTable.Columns.Add("LANGID", typeof(string));
                inTable.Columns.Add("LOTID", typeof(string));
                inTable.Columns.Add("WIPSEQ", typeof(string));

                DataRow newRow = inTable.NewRow();
                newRow["LANGID"] = LoginInfo.LANGID;
                newRow["LOTID"] = _LOTID;
                newRow["WIPSEQ"] = _WIPSEQ;

                inTable.Rows.Add(newRow);

                new ClientProxy().ExecuteService("DA_PRD_SEL_WIP_HISTORY_HIST", "INDATA", "OUTDATA", inTable, (searchResult, searchException) =>
                {
                    try
                    {
                        if (searchException != null)
                        {
                            Util.MessageException(searchException);
                            return;
                        }

                        //dgList.Columns.Clear();

                        #region Numeric Format
                        string sFormat = "#,##0";

                        if (searchResult.Columns.Contains("PROCID") &&
                            searchResult.Columns.Contains("PCSGID") &&
                            searchResult.Columns.Contains("UNIT_CODE") &&
                            searchResult.Rows.Count > 0)
                        {
                            if (Util.NVC(searchResult.Rows[0]["PROCID"]).Equals(Process.MIXING))
                            {
                                // 믹서
                                sFormat = "###,##0.###";
                            }
                            else if (Util.NVC(searchResult.Rows[0]["PCSGID"]).Equals("A"))
                            {
                                // 조립
                                sFormat = "###,##0";
                            }
                            else
                            {
                                // 전극
                                if (Util.NVC(searchResult.Rows[0]["UNIT_CODE"]).Equals("EA"))
                                    sFormat = "###,##0.##";
                                else
                                    sFormat = "###,##0.#";
                            }
                        }
                        #endregion

                        for (int iCol = 0; iCol < dgList.Columns.Count; iCol++)
                        {
                            if (dgList.Columns[iCol].GetType() == typeof(C1.WPF.DataGrid.DataGridNumericColumn))
                            {
                                ((C1.WPF.DataGrid.DataGridBoundColumn)(dgList.Columns[iCol])).Format = sFormat;
                            }
                            //if (Util.NVC(searchResult.Columns[iCol].ColumnName).Equals("PROCID") || 
                            //    Util.NVC(searchResult.Columns[iCol].ColumnName).Equals("PCSGID") || 
                            //    Util.NVC(searchResult.Columns[iCol].ColumnName).Equals("UNIT_CODE")) continue;

                            //if (searchResult.Columns[iCol].DataType == typeof(decimal))
                            //    Util.SetGridColumnNumeric(dgList, searchResult.Columns[iCol].ColumnName, null, searchResult.Columns[iCol].ColumnName, true, true, true, true, -1, HorizontalAlignment.Right, Visibility.Visible, sFormat);
                            //else
                            //    Util.SetGridColumnText(dgList, searchResult.Columns[iCol].ColumnName, null, searchResult.Columns[iCol].ColumnName, true, true, true, true, -1, HorizontalAlignment.Center, Visibility.Visible);
                        }

                        #region 공정별 컬럼 설정
                        if (searchResult.Rows.Count > 0 &&
                            (Util.NVC(searchResult.Rows[0]["PROCID"]).Equals(Process.COATING) ||
                             Util.NVC(searchResult.Rows[0]["PROCID"]).Equals(Process.BACK_COATING) ||
                             Util.NVC(searchResult.Rows[0]["PROCID"]).Equals(Process.INS_COATING) ||
                             Util.NVC(searchResult.Rows[0]["PROCID"]).Equals(Process.TOP_COATING)))
                        {
                            dgList.Columns["EQPT_INPUT_M_TOP_QTY"].Visibility = Visibility.Visible;
                            dgList.Columns["EQPT_INPUT_M_TOP_BACK_QTY"].Visibility = Visibility.Visible;
                            dgList.Columns["EQPT_INPUT_M_TOTL_QTY"].Visibility = Visibility.Visible;
                            dgList.Columns["EQPT_INPUT_PTN_TOP_QTY"].Visibility = Visibility.Visible;
                            dgList.Columns["EQPT_INPUT_PTN_TOP_BACK_QTY"].Visibility = Visibility.Visible;
                            dgList.Columns["EQPT_INPUT_PTN_TOTL_QTY"].Visibility = Visibility.Visible;
                        }
                        else
                        {
                            dgList.Columns["EQPT_INPUT_M_TOP_QTY"].Visibility = Visibility.Collapsed;
                            dgList.Columns["EQPT_INPUT_M_TOP_BACK_QTY"].Visibility = Visibility.Collapsed;
                            dgList.Columns["EQPT_INPUT_M_TOTL_QTY"].Visibility = Visibility.Collapsed;
                            dgList.Columns["EQPT_INPUT_PTN_TOP_QTY"].Visibility = Visibility.Collapsed;
                            dgList.Columns["EQPT_INPUT_PTN_TOP_BACK_QTY"].Visibility = Visibility.Collapsed;
                            dgList.Columns["EQPT_INPUT_PTN_TOTL_QTY"].Visibility = Visibility.Collapsed;
                        }


                        if (searchResult.Rows.Count > 0 && (Util.NVC(searchResult.Rows[0]["PCSGID"]).Equals(Area_Type.ELEC) ||
                                                            Util.NVC(searchResult.Rows[0]["PROCID"]).Equals(Process.NOTCHING)))
                        {
                            dgList.Columns["WIPQTY2_IN"].Visibility = Visibility.Collapsed;
                            dgList.Columns["WIPQTY2_ST"].Visibility = Visibility.Collapsed;
                            dgList.Columns["WIPQTY2_ED"].Visibility = Visibility.Visible;
                            dgList.Columns["WIPQTY2_OT"].Visibility = Visibility.Collapsed;
                            dgList.Columns["DFCT_QTY2"].Visibility = Visibility.Visible;
                            dgList.Columns["LOSS_QTY2"].Visibility = Visibility.Visible;
                            dgList.Columns["PRDT_REQ_QTY2"].Visibility = Visibility.Visible;
                            dgList.Columns["LEN_OVER_QTY2"].Visibility = Visibility.Visible;

                            dgList.Columns["WIPQTY_IN"].Header = ObjectDic.Instance.GetObjectName("공정지정수량(Roll)");
                            dgList.Columns["WIPQTY_ST"].Header = ObjectDic.Instance.GetObjectName("작업시작수량(Roll)");
                            dgList.Columns["WIPQTY_ED"].Header = ObjectDic.Instance.GetObjectName("양품량(Roll)");// "작업완료수량(Roll)");
                            dgList.Columns["WIPQTY_OT"].Header = ObjectDic.Instance.GetObjectName("공정이탈수량(Roll)");
                            dgList.Columns["DFCT_QTY"].Header = ObjectDic.Instance.GetObjectName("불량량(Roll)");
                            dgList.Columns["LOSS_QTY"].Header = ObjectDic.Instance.GetObjectName("LOSS량(Roll)");
                            dgList.Columns["PRDT_REQ_QTY"].Header = ObjectDic.Instance.GetObjectName("물품청구(Roll)");
                            dgList.Columns["LEN_OVER_QTY"].Header = ObjectDic.Instance.GetObjectName("길이초과(Roll)");
                        }
                        else
                        {
                            dgList.Columns["WIPQTY2_IN"].Visibility = Visibility.Collapsed;
                            dgList.Columns["WIPQTY2_ST"].Visibility = Visibility.Collapsed;
                            dgList.Columns["WIPQTY2_ED"].Visibility = Visibility.Collapsed;
                            dgList.Columns["WIPQTY2_OT"].Visibility = Visibility.Collapsed;
                            dgList.Columns["DFCT_QTY2"].Visibility = Visibility.Collapsed;
                            dgList.Columns["LOSS_QTY2"].Visibility = Visibility.Collapsed;
                            dgList.Columns["PRDT_REQ_QTY2"].Visibility = Visibility.Collapsed;
                            dgList.Columns["LEN_OVER_QTY2"].Visibility = Visibility.Collapsed;
                        }

                        if (searchResult.Rows.Count > 0 && (LoginInfo.CFG_SHOP_ID == "A010"                                 &&
                                                            Util.NVC(searchResult.Rows[0]["PCSGID"]).Equals(Area_Type.ASSY) &&
                                                            Util.NVC(searchResult.Rows[0]["PROCID"]).Equals(Process.WINDING)))
                        {
                            dgList.Columns["ACTQTY"].Visibility = Visibility.Visible;
                            dgList.Columns["PRE_WIPQTY_ED"].Visibility = Visibility.Visible;
                            dgList.Columns["UPDUSER"].Visibility = Visibility.Visible;
                        }


                        dgList.Columns["WIPQTY_IN"].Visibility = Visibility.Collapsed;
                        dgList.Columns["WIPQTY_ST"].Visibility = Visibility.Collapsed;
                        dgList.Columns["WIPQTY_ED"].Visibility = Visibility.Visible;
                        dgList.Columns["WIPQTY_OT"].Visibility = Visibility.Collapsed;

                        #endregion

                        Util.GridSetData(dgList, searchResult, FrameOperation, true);                        
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
                );
            }
            catch (Exception ex)
            {
                HiddenLoadingIndicator();
                Util.MessageException(ex);
            }
        }
        
        #endregion

        #region [Validation]

        #endregion

        #region [Func]
        private void ApplyPermissions()
        {
            List<Button> listAuth = new List<Button>();

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
        
        #endregion

        #endregion
    }
}
