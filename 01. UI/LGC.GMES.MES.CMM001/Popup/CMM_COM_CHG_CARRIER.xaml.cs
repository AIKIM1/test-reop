/*************************************************************************************
 Created Date : 2019.09.02
      Creator : INS 김동일K
   Decription : Carrier 교체 팝업 (Stacking and folding 공정만 사용)
--------------------------------------------------------------------------------------
 [Change History]
  2019.09.02  INS 김동일K : Initial Created.
  
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

namespace LGC.GMES.MES.CMM001.Popup
{
    /// <summary>
    /// CMM_COM_CHG_CARRIER.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class CMM_COM_CHG_CARRIER : C1Window, IWorkArea
    {
        private string _StkYN = string.Empty;
        private string _Procid = string.Empty;

        public CMM_COM_CHG_CARRIER()
        {
            InitializeComponent();
        }
        public IFrameOperation FrameOperation
        {
            get;
            set;
        }

        private void C1Window_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                object[] tmps = C1WindowExtension.GetParameters(this);

                _Procid = Util.NVC(tmps[0]);
                _StkYN = Util.NVC(tmps[1]);

                ApplyPermissions();

                InitControls("ALL");

                txtFromCstId.Focus();                
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void C1Window_Initialized(object sender, EventArgs e)
        {

        }

        private void txtFromCstId_KeyUp(object sender, KeyEventArgs e)
        {
            try
            {
                InitControls("FROM");

                if (e.Key == Key.Enter)
                {
                    if (txtFromCstId.Text.Equals("")) return;

                    DataTable dtRslt = GetCstInfo(txtFromCstId.Text);

                    if (dtRslt?.Rows?.Count > 0)
                    {
                        txtFromCstIdHidden.Text = Util.NVC(dtRslt.Rows[0]["CSTID"]);
                        txtFromCstProd.Text = Util.NVC(dtRslt.Rows[0]["CSTPROD"]);
                        txtFromCstProdName.Text = Util.NVC(dtRslt.Rows[0]["CSTPROD_NAME"]);
                        txtFromCstTypeName.Text = Util.NVC(dtRslt.Rows[0]["CSTTYPE_NAME"]);
                        txtFromCstType.Text = Util.NVC(dtRslt.Rows[0]["CSTTYPE"]);
                        txtFromLotId.Text = Util.NVC(dtRslt.Rows[0]["CURR_LOTID"]);
                        txtFromQty.Text = Util.NVC(dtRslt.Rows[0]["WIPQTY"]).Equals("") ? "0" : double.Parse(Util.NVC(dtRslt.Rows[0]["WIPQTY"])).ToString();
                        txtFromCstStat.Text = Util.NVC(dtRslt.Rows[0]["CSTSTAT"]);
                        txtFromWipStat.Text = Util.NVC(dtRslt.Rows[0]["WIPSTAT"]);
                        txtFromWipStatName.Text = Util.NVC(dtRslt.Rows[0]["WIPSTAT_NAME"]);

                        txtToCstId.Focus();
                    }
                    else
                    {
                        // 캐리어 정보가 없습니다.
                        Util.MessageValidation("SFU4564", (result) =>
                        {
                            txtFromCstId.Text = "";
                            txtFromCstId.Focus();
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void txtToCstId_KeyUp(object sender, KeyEventArgs e)
        {
            try
            {
                InitControls("TO");

                if (e.Key == Key.Enter)
                {
                    if (txtToCstId.Text.Equals("")) return;

                    DataTable dtRslt = GetCstInfo(txtToCstId.Text);

                    if (dtRslt?.Rows?.Count > 0)
                    {
                        if (Util.NVC(dtRslt.Rows[0]["CSTSTAT"]).Equals("U"))
                        {
                            // 작업오류 : 교체 Carrier [%1]의 상태가 'Empty'가 아닙니다. (Carrier 상태 확인)
                            Util.MessageValidation("SFU3738", (result) =>
                            {
                                txtToCstId.Focus();
                                txtToCstId.SelectAll();
                            }, txtToCstId.Text);

                            return;
                        }

                        txtToCstIdHidden.Text = Util.NVC(dtRslt.Rows[0]["CSTID"]);
                        txtToCstProd.Text = Util.NVC(dtRslt.Rows[0]["CSTPROD"]);
                        txtToCstProdName.Text = Util.NVC(dtRslt.Rows[0]["CSTPROD_NAME"]);
                        txtToCstStatName.Text = Util.NVC(dtRslt.Rows[0]["CSTSTAT_NAME"]);
                        txtToCstStat.Text = Util.NVC(dtRslt.Rows[0]["CSTSTAT"]);
                        txtToCstTypeName.Text = Util.NVC(dtRslt.Rows[0]["CSTTYPE_NAME"]);
                        txtToCstType.Text = Util.NVC(dtRslt.Rows[0]["CSTTYPE"]);
                        txtToLotId.Text = txtFromLotId.Text; //Util.NVC(dtRslt.Rows[0]["CURR_LOTID"]);                        
                    }
                    else
                    {
                        // 캐리어 정보가 없습니다.
                        Util.MessageValidation("SFU4564", (result) =>
                        {
                            txtToCstId.Text = "";
                            txtToCstId.Focus();
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!CanSave())
                    return;

                Util.MessageConfirm("SFU1241", (result) =>// 저장 하시겠습니까?
                {
                    if (result == MessageBoxResult.OK)
                    {
                        ChgCarrier();
                    }
                });
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                this.DialogResult = MessageBoxResult.Cancel;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void ApplyPermissions()
        {
            List<Button> listAuth = new List<Button>();

            listAuth.Add(btnSave);
            Util.pageAuth(listAuth, FrameOperation.AUTHORITY);
        }

        private void InitControls(string sType)
        {
            switch (sType.ToUpper())
            {
                case "ALL":
                    txtFromCstId.Text = "";
                    txtFromCstIdHidden.Text = "";
                    txtFromCstProd.Text = "";
                    txtFromCstProdName.Text = "";
                    txtFromCstType.Text = "";
                    txtFromCstTypeName.Text = "";
                    txtFromLotId.Text = "";
                    txtFromQty.Text = "";
                    txtFromCstStat.Text = "";
                    txtFromWipStat.Text = "";
                    txtFromWipStatName.Text = "";

                    txtToCstId.Text = "";
                    txtToCstIdHidden.Text = "";
                    txtToCstProd.Text = "";
                    txtToCstProdName.Text = "";
                    txtToCstStat.Text = "";
                    txtToCstStatName.Text = "";
                    txtToCstType.Text = "";
                    txtToCstTypeName.Text = "";
                    txtToLotId.Text = "";
                    break;
                case "FROM":
                    //txtFromCstId.Text = "";
                    txtFromCstIdHidden.Text = "";
                    txtFromCstProd.Text = "";
                    txtFromCstProdName.Text = "";
                    txtFromCstType.Text = "";
                    txtFromCstTypeName.Text = "";
                    txtFromLotId.Text = "";
                    txtFromQty.Text = "";
                    txtFromCstStat.Text = "";
                    txtFromWipStat.Text = "";
                    txtFromWipStatName.Text = "";
                    break;
                case "TO":
                    //txtToCstId.Text = "";
                    txtToCstIdHidden.Text = "";
                    txtToCstProd.Text = "";
                    txtToCstProdName.Text = "";
                    txtToCstStat.Text = "";
                    txtToCstStatName.Text = "";
                    txtToCstType.Text = "";
                    txtToCstTypeName.Text = "";
                    txtToLotId.Text = "";
                    break;
                default:
                    txtFromCstId.Text = "";
                    txtFromCstIdHidden.Text = "";
                    txtFromCstProd.Text = "";
                    txtFromCstProdName.Text = "";
                    txtFromCstType.Text = "";
                    txtFromCstTypeName.Text = "";
                    txtFromLotId.Text = "";
                    txtFromQty.Text = "";
                    txtFromCstStat.Text = "";
                    txtFromWipStat.Text = "";
                    txtFromWipStatName.Text = "";

                    txtToCstId.Text = "";
                    txtToCstIdHidden.Text = "";
                    txtToCstProd.Text = "";
                    txtToCstProdName.Text = "";
                    txtToCstStat.Text = "";
                    txtToCstStatName.Text = "";
                    txtToCstType.Text = "";
                    txtToCstTypeName.Text = "";
                    txtToLotId.Text = "";
                    break;
            }
        }

        private DataTable GetCstInfo(string sCstID)
        {
            try
            {
                loadingIndicator.Visibility = Visibility.Visible;

                DataTable inTable = null;
                
                DataTable inDataTable = new DataTable();
                inDataTable.Columns.Add("CSTID", typeof(string));
                inDataTable.Columns.Add("LANGID", typeof(string));
                
                inTable = inDataTable;

                DataRow newRow = inTable.NewRow();
                newRow["LANGID"] = LoginInfo.LANGID;
                newRow["CSTID"] = sCstID;
               
                inTable.Rows.Add(newRow);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_CARRIER_WITH_WIP", "INDATA", "OUTDATA", inTable);

                loadingIndicator.Visibility = Visibility.Collapsed;

                return dtRslt;
            }
            catch (Exception ex)
            {
                loadingIndicator.Visibility = Visibility.Collapsed;
                Util.MessageException(ex);
                return null;
            }
        }

        private bool CanSave()
        {
            bool bRet = false;

            if (txtFromCstIdHidden.Text.Equals(""))
            {
                // 입력오류 : 이전 Carrier 정보를 입력하세요.
                Util.MessageValidation("SFU3740", (result) =>
                {
                    txtFromCstId.Focus();
                });
                return bRet;
            }

            if (txtToCstIdHidden.Text.Equals(""))
            {
                // 입력오류 : 교체 Carrier 정보를 입력하세요.
                Util.MessageValidation("SFU3741", (result) =>
                {
                    txtToCstId.Focus();
                });
                return bRet;
            }

            if (!(txtFromCstType.Text.Equals("BK") && txtToCstType.Text.Equals("BK")))
            {
                // 작업오류 : Carrier 교체는 'BOX' 타입의 Carrier만 교체 가능 합니다. (Carrier Type 확인)
                Util.MessageValidation("SFU3734");
                return bRet;
            }

            if (!string.Equals(txtFromCstProd.Text, txtToCstProd.Text))
            {
                // 작업오류 : 동일한 Carrier 사용자재만 교체 가능 합니다. (동일 Carrier 사용자재 여부 확인)
                Util.MessageValidation("SFU3735");
                return bRet;
            }

            if (txtFromLotId.Text.Trim().Equals(""))
            {
                // 작업오류 : 이전 Carrier [%1]에 매핑된 Lot ID가 없습니다.
                Util.MessageValidation("SFU3736", (result) =>
                {
                    txtFromCstId.Focus();
                    txtFromCstId.SelectAll();
                }, txtFromCstId.Text);
                return bRet;
            }

            if (!txtFromCstStat.Text.Equals("U"))
            {
                // 작업오류 : 이전 Carrier [%1]의 상태가 'Using'이 아닙니다. (Carrier 상태 확인)
                Util.MessageValidation("SFU3737", (result) =>
                {
                    txtFromCstId.Focus();
                    txtFromCstId.SelectAll();
                }, txtFromCstId.Text);
                return bRet;
            }

            if (txtToCstStat.Text.Equals("U"))
            {
                // 작업오류 : 교체 Carrier [%1]의 상태가 'Empty'가 아닙니다. (Carrier 상태 확인)
                Util.MessageValidation("SFU3738", (result) =>
                {
                    txtToCstId.Focus();
                    txtToCstId.SelectAll();
                }, txtToCstId.Text);
                return bRet;
            }

            if (!txtFromWipStat.Text.Equals("WAIT"))
            {
                // 작업오류 : 이전 Carrier [%1]에 매핑된 Lot [%2]이 대기 상태가 아닙니다. (Lot 상태 확인)
                Util.MessageValidation("SFU3739", (result) =>
                {
                    txtFromCstId.Focus();
                    txtFromCstId.SelectAll();
                }, txtFromCstId.Text, txtFromLotId.Text);
                return bRet;
            }

            bRet = true;

            return bRet;
        }

        private void ChgCarrier()
        {
            try
            {
                loadingIndicator.Visibility = Visibility.Visible;

                DataTable inDataTable = new DataTable();
                inDataTable.Columns.Add("SRCTYPE", typeof(string));
                inDataTable.Columns.Add("IFMODE", typeof(string));
                inDataTable.Columns.Add("PROCID", typeof(string));
                inDataTable.Columns.Add("FROM_CSTID", typeof(string));
                inDataTable.Columns.Add("TO_LOTID", typeof(string));
                inDataTable.Columns.Add("TO_CSTID", typeof(string));
                inDataTable.Columns.Add("USERID", typeof(string));

                DataRow param = inDataTable.NewRow();
                param["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                param["IFMODE"] = IFMODE.IFMODE_OFF;
                param["PROCID"] = _Procid;
                param["FROM_CSTID"] = txtFromCstIdHidden.Text;
                param["TO_LOTID"] = txtToLotId.Text;
                param["TO_CSTID"] = txtToCstIdHidden.Text;
                param["USERID"] = LoginInfo.USERID;

                inDataTable.Rows.Add(param);

                new ClientProxy().ExecuteService("BR_PRD_REG_CHANGE_CARRIER", "INDATA", null, inDataTable, (bizResult, bizException) =>
                {
                    try
                    {
                        if (bizException != null)
                        {
                            Util.MessageException(bizException);
                            return;
                        }

                        Util.MessageInfo("SFU1275");      //정상 처리 되었습니다.

                        InitControls("ALL");
                    }
                    catch (Exception ex)
                    {
                        Util.MessageException(ex);
                    }
                    finally
                    {
                        loadingIndicator.Visibility = Visibility.Collapsed;
                    }
                });
            }
            catch (Exception ex)
            {
                loadingIndicator.Visibility = Visibility.Collapsed;
                Util.MessageException(ex);
            }
        }
    }
}
