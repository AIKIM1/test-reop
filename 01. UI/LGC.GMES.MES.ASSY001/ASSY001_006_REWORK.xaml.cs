/*************************************************************************************
 Created Date : 2017.06.14
      Creator : INS 김동일K
   Decription : 전지 5MEGA-GMES 구축 - STACKING 공정진척 화면 - 재작업 생성 팝업
--------------------------------------------------------------------------------------
 [Change History]
  2017.06.14  INS 김동일K : Initial Created.
  2017.08.09  CNS 고현영S : STACKING 에서 사용가능하도록 추가
**************************************************************************************/


using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using C1.WPF;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.ControlsLibrary;
using System.Data;

namespace LGC.GMES.MES.ASSY001
{
    /// <summary>
    /// ASSY001_006_REWORK.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class ASSY001_006_REWORK : C1Window, IWorkArea
    {
        #region Declaration & Constructor 
        private string _LineID = string.Empty;
        private string _EqptID = string.Empty;
        private string _ProdLotID = string.Empty;
        private string _OutLotID = string.Empty;
        private decimal _OutQty = 0;

        private string _UNLDR_LOT_IDENT_BAS_CODE = string.Empty;

        private BizDataSet _Biz = new BizDataSet();

        public string OUT_LOT_ID
        {
            get { return _OutLotID; }
        } 

        public decimal OUT_QTY
        {
            get { return _OutQty; }
        }
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
        public ASSY001_006_REWORK()
        {
            InitializeComponent();
        }        
        #endregion

        #region Event

        private void C1Window_Loaded(object sender, RoutedEventArgs e)
        {
            object[] tmps = C1WindowExtension.GetParameters(this);

            if (tmps != null && tmps.Length >= 4)
            {
                _LineID = Util.NVC(tmps[0]);
                _EqptID = Util.NVC(tmps[1]);
                _ProdLotID = Util.NVC(tmps[2]);

                _UNLDR_LOT_IDENT_BAS_CODE = Util.NVC(tmps[3]);
            }
            else
            {
                _LineID = "";
                _EqptID = "";
                _ProdLotID = "";

                _UNLDR_LOT_IDENT_BAS_CODE = "";
            }
            ApplyPermissions();

            if (_UNLDR_LOT_IDENT_BAS_CODE.Equals("RF_ID") || _UNLDR_LOT_IDENT_BAS_CODE.Equals("CST_ID"))
            {
                lblCST.Visibility = Visibility.Visible;
                txtOutCa.Visibility = Visibility.Visible;
            }
            else
            {
                lblCST.Visibility = Visibility.Collapsed;
                txtOutCa.Visibility = Visibility.Collapsed;
            }
        }

        private void C1Window_Initialized(object sender, EventArgs e)
        {

        }
        
        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = MessageBoxResult.Cancel;
        }

        private void btnCreate_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!CanCreateReworkBox())
                {                 
                    return;
                }

                CreateReworkBox(GetNewOutLotid());
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void txtTotQty_KeyUp(object sender, KeyEventArgs e)
        {
            try
            {
                if (!Util.CheckDecimal(txtTotQty.Text, 0))
                {
                    txtTotQty.Text = "";
                    return;
                }

                decimal dTotQty = 0;
                decimal dReworkQty = 0;

                dTotQty = txtTotQty.Text.Equals("") ? 0 : Util.NVC_Decimal(txtTotQty.Text);
                dReworkQty = txtReworkQty.Text.Equals("") ? 0 : Util.NVC_Decimal(txtReworkQty.Text);

                //if (dTotQty - dReworkQty < 0)
                //{
                //    Util.MessageValidation("양품 수량은 음수일 수 없습니다.");
                //    txtTotQty.Text = txtReworkQty.Text;
                //    txtGoodQty.Text = txtReworkQty.Text;
                //    return;
                //}

                txtGoodQty.Text = (dTotQty - dReworkQty).ToString();
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void txtReworkQty_KeyUp(object sender, KeyEventArgs e)
        {
            try
            {
                if (!Util.CheckDecimal(txtReworkQty.Text, 0))
                {
                    txtReworkQty.Text = "";
                    return;
                }

                decimal dTotQty = 0;
                decimal dReworkQty = 0;

                dTotQty = txtTotQty.Text.Equals("") ? 0 : Util.NVC_Decimal(txtTotQty.Text);
                dReworkQty = txtReworkQty.Text.Equals("") ? 0 : Util.NVC_Decimal(txtReworkQty.Text);

                //if (dTotQty - dReworkQty < 0)
                //{
                //    Util.MessageValidation("양품 수량은 음수일 수 없습니다.");
                //    txtReworkQty.Text = txtTotQty.Text;
                //    txtGoodQty.Text = txtTotQty.Text;
                //    return;
                //}

                txtGoodQty.Text = (dTotQty - dReworkQty).ToString();
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        #endregion

        #region Mehod

        #region [BizCall]

        private void CreateReworkBox(string sNewOutLot)
        {
            try
            {
                if (sNewOutLot.Equals(""))
                    return;

                ShowLoadingIndicator();

                DataTable inTable = _Biz.GetBR_PRD_REG_CREATE_BOX_FD();

                DataRow newRow = inTable.NewRow();
                newRow["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                newRow["LANGID"] = LoginInfo.LANGID;
                newRow["IFMODE"] = IFMODE.IFMODE_OFF;
                newRow["EQPTID"] = _EqptID;
                newRow["OUT_LOTID"] = sNewOutLot;
                newRow["PROD_LOTID"] = _ProdLotID;
                newRow["WO_DETL_ID"] = null;
                newRow["INPUTQTY"] = Convert.ToDecimal(txtTotQty.Text);
                newRow["OUTPUTQTY"] = Convert.ToDecimal(txtTotQty.Text);
                newRow["RESNQTY"] = 0;
                newRow["SHIFT"] = null;
                newRow["WIPNOTE"] = "";
                newRow["WRK_USER_NAME"] = "";
                newRow["USERID"] = LoginInfo.USERID;
                newRow["BONUSQTY"] = Convert.ToDecimal(txtReworkQty.Text);

                if (_UNLDR_LOT_IDENT_BAS_CODE.Equals("RF_ID") || _UNLDR_LOT_IDENT_BAS_CODE.Equals("CST_ID"))
                    newRow["CSTID"] = txtOutCa.Text.Trim();

                inTable.Rows.Add(newRow);

                new ClientProxy().ExecuteService("BR_PRD_REG_CREATE_OUT_LOT_FD", "INDATA", null, inTable, (searchResult, searchException) =>
                {
                    try
                    {
                        if (searchException != null)
                        {
                            Util.MessageException(searchException);
                            return;
                        }

                        _OutLotID = sNewOutLot;
                        _OutQty = Convert.ToDecimal(txtTotQty.Text);

                        Util.MessageValidation("SFU1275");  //정상 처리 되었습니다.

                        this.DialogResult = MessageBoxResult.OK;
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

        private string GetNewOutLotid()
        {
            try
            {
                ShowLoadingIndicator();

                DataTable inTable = _Biz.GetBR_PRD_GET_NEW_OUT_LOT_FD();

                DataRow newRow = inTable.NewRow();
                newRow["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                newRow["IFMODE"] = IFMODE.IFMODE_OFF;
                newRow["EQPTID"] = _EqptID;
                newRow["PROD_LOTID"] = _ProdLotID;
                //newRow["WIP_TYPE_CODE"] = INOUT_TYPE.OUT;
                //newRow["CALDATE_YMD"] = "";
                newRow["USERID"] = LoginInfo.USERID;

                inTable.Rows.Add(newRow);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("BR_PRD_GET_NEW_OUT_LOTID_FD", "INDATA", "OUTDATA", inTable);

                string sNewLot = string.Empty;
                if (dtResult != null && dtResult.Rows.Count > 0)
                {
                    sNewLot = Util.NVC(dtResult.Rows[0]["OUT_LOTID"]);
                }
                HiddenLoadingIndicator();
                return sNewLot;
            }
            catch (Exception ex)
            {
                HiddenLoadingIndicator();
                Util.MessageException(ex);
                return "";
            }
        }
        
        #endregion

        #region [Func]
        private void ApplyPermissions()
        {
            List<Button> listAuth = new List<Button>();
            listAuth.Add(btnCreate);

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

        #region [Validation]
        private bool CanCreateReworkBox()
        {
            bool bRet = false;

            if (txtTotQty.Text.Trim().Equals("") || Util.NVC_Decimal(txtTotQty.Text) < 1)
            {
                Util.MessageValidation("SFU3580");   // 전체 수량을 입력 하세요.
                return bRet;
            }

            if (txtReworkQty.Text.Trim().Equals("") || Util.NVC_Decimal(txtReworkQty.Text) < 1)
            {
                Util.MessageValidation("SFU3581");  // 재작업 수량을 입력 하세요.
                return bRet;
            }

            decimal dGoodQty = 0;
            dGoodQty = txtGoodQty.Text.Trim().Equals("") ? 0 : Util.NVC_Decimal(txtGoodQty.Text);
            if (dGoodQty < 0)
            {
                Util.MessageValidation("SFU1721");  // 양품량은 음수가 될 수 없습니다.값을 맞게 변경하세요.
                return bRet;
            }

            if (!CheckedUseCassette())  //Cassette 중복여부 체크.
            {
                txtOutCa.SelectAll();
                return bRet;
            }

            bRet = true;
            return bRet;
        }

        #endregion

        #endregion

        private void txtOutCa_GotFocus(object sender, RoutedEventArgs e)
        {
            try
            {
                if (txtOutCa == null) return;
                InputMethod.SetPreferredImeConversionMode(txtOutCa, ImeConversionModeValues.Alphanumeric);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        /// <summary>
        /// CST 사용 가능 확인
        /// </summary>
        /// <returns></returns>
        private bool CheckedUseCassette()
        {
            try
            {
                DataSet IndataSet = new DataSet();

                DataTable dtIN_EQP = IndataSet.Tables.Add("IN_EQP");
                dtIN_EQP.Columns.Add("SRCTYPE", typeof(string));
                dtIN_EQP.Columns.Add("IFMODE", typeof(string));
                dtIN_EQP.Columns.Add("CSTID", typeof(string));
                dtIN_EQP.Columns.Add("WIP_TYPE_CODE", typeof(string));

                DataRow dr = dtIN_EQP.NewRow();
                dr["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                dr["IFMODE"] = IFMODE.IFMODE_OFF;
                dr["CSTID"] = Util.NVC(txtOutCa.Text.Trim());
                dr["WIP_TYPE_CODE"] = "OUT";
                dtIN_EQP.Rows.Add(dr);


                DataTable dtIN_INPUT = IndataSet.Tables.Add("IN_INPUT");
                dtIN_INPUT.Columns.Add("LANGID", typeof(string));
                dtIN_INPUT.Columns.Add("PROCID", typeof(string));
                dtIN_INPUT.Columns.Add("EQSGID", typeof(string));

                dr = dtIN_INPUT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["PROCID"] = Process.STACKING_FOLDING;
                dr["EQSGID"] = _LineID;
                dtIN_INPUT.Rows.Add(dr);

                new ClientProxy().ExecuteServiceSync_Multi("BR_PRD_CHK_CST_MAPPING_DUP", "IN_EQP,IN_INPUT", null, IndataSet);

                return true;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return false;
            }
        }
    }
}
