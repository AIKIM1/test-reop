/*************************************************************************************
 Created Date : 2018.03.06
      Creator : INS 김동일K
   Decription : 전지 5MEGA-GMES 구축 - 조립 공정진척 화면 - LOT 종료취소(CST 용) 팝업
--------------------------------------------------------------------------------------
 [Change History]
  2018.03.06  INS 김동일K : Initial Created.
  2020.05.27  김동일 : C20200513-000349 재고 및 수율 정합성 향상을 위한 투입Lot 종료 취소에 대한 기능변경
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

namespace LGC.GMES.MES.CMM001
{
    /// <summary>
    /// CMM_ASSY_CANCEL_TERM_CST.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class CMM_ASSY_CANCEL_TERM_CST : C1Window, IWorkArea
    {
        private string _ProcID = string.Empty;


        private string _AUTO_STOP_FLAG = string.Empty;

        private BizDataSet _Biz = new BizDataSet();
        private Util _Util = new Util();

        public CMM_ASSY_CANCEL_TERM_CST()
        {
            InitializeComponent();
        }

        public IFrameOperation FrameOperation
        {
            get;
            set;
        }
        
        private void txtSearchID_KeyUp(object sender, KeyEventArgs e)
        {
            try
            {
                if (e.Key == Key.Enter)
                {
                    GetTermLotInfo();
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void C1Window_Initialized(object sender, System.EventArgs e)
        {

        }

        private void C1Window_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {
            try
            {
                object[] tmps = C1WindowExtension.GetParameters(this);

                if (tmps != null && tmps.Length >= 1)
                {
                    _ProcID = Util.NVC(tmps[0]);
                }
                else
                {
                    _ProcID = "";
                }

                ApplyPermissions();
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (txtSearchID.Text.Trim().Equals(""))
                {
                    Util.MessageValidation("SFU1190");
                    return;
                }

                GetTermLotInfo();
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void btnSave_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            try
            {
                if (!CanCancelTerm())
                    return;

                Util.MessageConfirm("SFU1887", (result) =>// 종료취소 하시겠습니까?
                {
                    if (result == MessageBoxResult.OK)
                    {
                        CancelTermLot();
                    }
                });
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void btnClose_Clicked(object sender, System.Windows.RoutedEventArgs e)
        {
            this.DialogResult = MessageBoxResult.Cancel;
        }

        private void ApplyPermissions()
        {
            List<Button> listAuth = new List<Button>();
            listAuth.Add(btnSave);

            Util.pageAuth(listAuth, FrameOperation.AUTHORITY);
        }

        private bool CanCancelTerm()
        {
            bool bRet = false;

            if (txtLotID.Text.Trim().Equals(""))
            {
                Util.MessageValidation("SFU2953");    // 종료 취소 할 항목이 없습니다.
                return bRet;
            }

            if (txtCstID.Text.Trim().Equals(""))
            {
                Util.MessageValidation("SFU1244");
                return bRet;
            }

            // 자동 Change 모드인 경우 투입취소 불가.
            if (_ProcID.Equals(Process.LAMINATION) && _AUTO_STOP_FLAG.Equals("Y"))
            {
                // 투입취소 불가 : 설비 자동 Change 모드로 투입 완료처리된 LOT은 취소 불가.
                Util.MessageValidation("SFU6037");
                return bRet;
            }

            // 수량 체크.
            double dQty = 0;
            double.TryParse(txtQty.Text, out dQty);
            if (dQty < 1)
            {
                Util.MessageValidation("SFU1683");  // 수량은 0보다 커야 합니다.
                return bRet;
            }

            bRet = true;
            return bRet;
        }

        private void GetTermLotInfo()
        {
            try
            {
                if (txtSearchID.Text.Trim().Equals("")) return;

                loadingIndicator.Visibility = Visibility.Visible;

                DataTable inTable = new DataTable();
                inTable.Columns.Add("PROCID", typeof(string));
                inTable.Columns.Add("LOTID", typeof(string));
                inTable.Columns.Add("SEL_TYPE", typeof(string));

                DataRow newRow = inTable.NewRow();
                newRow["PROCID"] = _ProcID;
                newRow["LOTID"] = txtSearchID.Text.Trim();
                newRow["SEL_TYPE"] = "CST";

                inTable.Rows.Add(newRow);

                new ClientProxy().ExecuteService("DA_PRD_SEL_CANCEL_TERMINATE_CMM", "INDATA", "OUTDATA", inTable, (bizResult, bizException) =>
                {
                    try
                    {
                        if (bizException != null)
                        {
                            Util.MessageException(bizException);
                            return;
                        }

                        if (bizResult != null && bizResult.Rows.Count < 1)
                        {
                            Util.MessageValidation("SFU2885", txtSearchID.Text);    // {0} 은 해당 공정에 투입LOT 중 종료된 정보가 없습니다.

                            txtSearchID.Text = "";

                            txtLotID.Text = "";
                            txtQty.Text = "";
                            txtCstID.Text = "";
                            txtProdID.Text = "";
                            txtShift.Text = "";
                            txtWrkName.Text = "";
                            txtDttm.Text = "";

                            txtWIPQTY_IN.Text = "";
                            txtEQPT_INPUT_END_QTY.Text = "";

                            _AUTO_STOP_FLAG = "";
                        }
                        else
                        {
                            txtSearchID.Text = "";


                            txtQty.KeyUp -= txtQty_KeyUp;

                            double dTmp, dWipQtyIn, dEqptEndQty = 0;
                            double.TryParse(Util.NVC(bizResult.Rows[0]["WIPQTY2_ST"]), out dTmp);
                            double.TryParse(Util.NVC(bizResult.Rows[0]["WIPQTY_IN"]), out dWipQtyIn);
                            double.TryParse(Util.NVC(bizResult.Rows[0]["EQPT_INPUT_END_QTY"]), out dEqptEndQty);

                            txtLotID.Text = Util.NVC(bizResult.Rows[0]["LOTID"]);
                            txtQty.Text = Util.NVC(bizResult.Rows[0]["WIPQTY2_ST"]).Equals("") ? "" : dTmp.ToString();
                            txtCstID.Text = Util.NVC(bizResult.Rows[0]["CSTID"]);
                            txtProdID.Text = Util.NVC(bizResult.Rows[0]["PRODID"]);
                            txtShift.Text = Util.NVC(bizResult.Rows[0]["SHIFT"]);
                            txtWrkName.Text = Util.NVC(bizResult.Rows[0]["WRK_USER_NAME"]);
                            txtDttm.Text = Util.NVC(bizResult.Rows[0]["WIPDTTM_OT"]);
                            
                            txtWIPQTY_IN.Text = Util.NVC(bizResult.Rows[0]["WIPQTY_IN"]).Equals("") ? "0" : dWipQtyIn.ToString();
                            txtEQPT_INPUT_END_QTY.Text = Util.NVC(bizResult.Rows[0]["EQPT_INPUT_END_QTY"]).Equals("") ? "" : dEqptEndQty.ToString();

                            _AUTO_STOP_FLAG = Util.NVC(bizResult.Rows[0]["AUTO_STOP_FLAG"]);
                        }
                    }
                    catch (Exception ex)
                    {
                        Util.MessageException(ex);
                    }
                    finally
                    {
                        txtQty.KeyUp -= txtQty_KeyUp;

                        txtQty.KeyUp += txtQty_KeyUp;

                        loadingIndicator.Visibility = Visibility.Collapsed;
                    }
                });
            }
            catch (Exception ex)
            {                
                Util.MessageException(ex);
                loadingIndicator.Visibility = Visibility.Collapsed;
            }
        }

        private void CancelTermLot()
        {
            try
            {
                loadingIndicator.Visibility = Visibility.Visible;
                
                DataSet indataSet = new DataSet();

                DataTable inTable = indataSet.Tables.Add("INDATA");
                inTable.Columns.Add("SRCTYPE", typeof(string));
                inTable.Columns.Add("USERID", typeof(string));
                
                DataTable in_DATA = indataSet.Tables.Add("INLOT");
                in_DATA.Columns.Add("LOTID", typeof(string));
                in_DATA.Columns.Add("LOTSTAT", typeof(string));
                in_DATA.Columns.Add("WIPQTY", typeof(int));
                in_DATA.Columns.Add("WIPQTY2", typeof(int));
                in_DATA.Columns.Add("CSTID", typeof(string));

                DataRow newRow = inTable.NewRow();
                newRow["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                newRow["USERID"] = LoginInfo.USERID;

                inTable.Rows.Add(newRow);
                
                newRow = null;

                newRow = in_DATA.NewRow();
                newRow["LOTID"] = txtLotID.Text.Trim();
                newRow["LOTSTAT"] = "RELEASED";
                newRow["WIPQTY"] = txtQty.Text.Equals("") ? 0 : double.Parse(txtQty.Text);
                newRow["WIPQTY2"] = txtQty.Text.Equals("") ? 0 : double.Parse(txtQty.Text);
                newRow["CSTID"] = txtCstID.Text.Trim();

                in_DATA.Rows.Add(newRow);

                new ClientProxy().ExecuteService_Multi("BR_PRD_REG_CANCEL_INPUT_TERMINATE_LOT", "INDATA,INLOT", null, (bizResult, bizException) =>
                {
                    try
                    {
                        if (bizException != null)
                        {
                            Util.MessageException(bizException);
                            return;
                        }

                        Util.MessageInfo("SFU1275");      //정상 처리 되었습니다.

                        txtSearchID.Text = "";

                        txtLotID.Text = "";
                        txtQty.Text = "";
                        txtCstID.Text = "";
                        txtProdID.Text = "";
                        txtShift.Text = "";
                        txtWrkName.Text = "";
                        txtDttm.Text = "";
                        txtWIPQTY_IN.Text = "";
                        txtEQPT_INPUT_END_QTY.Text = "";
                    }
                    catch (Exception ex)
                    {
                        Util.MessageException(ex);
                    }
                    finally
                    {
                        loadingIndicator.Visibility = Visibility.Collapsed;
                    }
                }, indataSet);
            }
            catch (Exception ex)
            {
                loadingIndicator.Visibility = Visibility.Collapsed;
                Util.MessageException(ex);
            }
        }

        private void txtQty_KeyUp(object sender, KeyEventArgs e)
        {
            try
            {
                if (!Util.CheckDecimal(txtQty.Text, 0))
                {
                    txtQty.Text = "";
                    return;
                }

                if (!txtWIPQTY_IN.Text.Equals(""))
                {
                    double dMax = 0;
                    double dNow = 0;

                    double.TryParse(txtWIPQTY_IN.Text, out dMax);
                    double.TryParse(txtQty.Text, out dNow);

                    if (dMax >= 0 && dMax < dNow)
                    {
                        Util.MessageValidation("SFU3107");   // 수량이 이전 수량보다 많이 입력 되었습니다.
                        txtQty.Text = dMax == 0 ? "1" : dMax.ToString();
                    }
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void txtSearchID_GotFocus(object sender, RoutedEventArgs e)
        {
            try
            {
                if (txtSearchID == null) return;
                InputMethod.SetPreferredImeConversionMode(txtSearchID, ImeConversionModeValues.Alphanumeric);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void txtCstID_GotFocus(object sender, RoutedEventArgs e)
        {
            try
            {
                if (txtCstID == null) return;
                InputMethod.SetPreferredImeConversionMode(txtCstID, ImeConversionModeValues.Alphanumeric);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
    }
}
