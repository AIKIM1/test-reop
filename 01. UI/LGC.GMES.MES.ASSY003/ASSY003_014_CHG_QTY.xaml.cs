/*************************************************************************************
 Created Date : 2017.12.22
      Creator : INS 김동일K
   Decription : 전지 5MEGA-GMES 구축 - 조립 공정진척 화면 - C 생산 재공수량 변경 팝업
--------------------------------------------------------------------------------------
 [Change History]
  2016.11.28  INS 김동일K : Initial Created.
  
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
using C1.WPF.DataGrid;

namespace LGC.GMES.MES.ASSY003
{
    /// <summary>
    /// ASSY003_014_CHG_QTY.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class ASSY003_014_CHG_QTY : C1Window, IWorkArea
    {        
        #region Declaration & Constructor 
        private string _PRJT_NAME = string.Empty;
        private string _PRODID = string.Empty;
        private string _PRODUCT_LEVEL3_CODE = string.Empty;
        private string _PRODUCT_LEVEL3_NAME = string.Empty;
        private string _MKT_TYPE_CODE = string.Empty;
        private string _MKT_TYPE_NAME = string.Empty;
        private string _WIPQTY = string.Empty;
        private string _EqptID = string.Empty;
        private string _LOTID = string.Empty;

        private BizDataSet _Biz = new BizDataSet();
        private Util _Util = new Util();
        #endregion

        #region Initialize 
        public IFrameOperation FrameOperation
        {
            get;
            set;
        }

        public ASSY003_014_CHG_QTY()
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

                if (tmps != null && tmps.Length >= 8)
                {
                    _PRJT_NAME = Util.NVC(tmps[0]);
                    _PRODID = Util.NVC(tmps[1]);
                    _PRODUCT_LEVEL3_CODE = Util.NVC(tmps[2]);
                    _PRODUCT_LEVEL3_NAME = Util.NVC(tmps[3]);
                    _MKT_TYPE_CODE = Util.NVC(tmps[4]);
                    _MKT_TYPE_NAME = Util.NVC(tmps[5]);
                    _WIPQTY = Util.NVC(tmps[6]);
                    _EqptID = Util.NVC(tmps[7]);
                    //_LOTID = Util.NVC(tmps[8]);
                }
                else
                {
                    _PRJT_NAME = "";
                    _PRODID = "";
                    _PRODUCT_LEVEL3_CODE = "";
                    _PRODUCT_LEVEL3_NAME = "";
                    _MKT_TYPE_CODE = "";
                    _MKT_TYPE_NAME = "";
                    _WIPQTY = "";
                    _EqptID = "";
                    //_LOTID = "";
                }

                DataTable dtTmp = new DataTable();
                //dtTmp.Columns.Add("LOTID", typeof(string));
                dtTmp.Columns.Add("PRJT_NAME", typeof(string));
                dtTmp.Columns.Add("PRODID", typeof(string));
                dtTmp.Columns.Add("CELLTYPE", typeof(string));
                dtTmp.Columns.Add("MKT_TYPE_CODE", typeof(string));
                dtTmp.Columns.Add("MKT_TYPE_NAME", typeof(string));
                dtTmp.Columns.Add("WIPQTY", typeof(string));

                DataRow dtRow = dtTmp.NewRow();
                //dtRow["LOTID"] = _LOTID;
                dtRow["PRJT_NAME"] = _PRJT_NAME;
                dtRow["PRODID"] = _PRODID;
                dtRow["CELLTYPE"] = _PRODUCT_LEVEL3_NAME;
                dtRow["MKT_TYPE_CODE"] = _MKT_TYPE_CODE;
                dtRow["MKT_TYPE_NAME"] = _MKT_TYPE_NAME;
                dtRow["WIPQTY"] = _WIPQTY;

                dtTmp.Rows.Add(dtRow);

                Util.GridSetData(dgInfo, dtTmp, FrameOperation, false);

                ApplyPermissions();

                txtDifQty.Text = "0";
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void btnChange_Clicked(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!CanChange())
                    return;

                ChangeQty();
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

        private void txtChgQty_KeyUp(object sender, KeyEventArgs e)
        {
            try
            {
                if (!Util.CheckDecimal(txtChgQty.Text, 0))
                {
                    txtChgQty.Text = "";

                    if (dgInfo.Rows.Count > 0)
                        txtDifQty.Text = Util.NVC(DataTableConverter.GetValue(dgInfo.Rows[0].DataItem, "WIPQTY")).Equals("") ? 0.ToString() : Util.NVC(DataTableConverter.GetValue(dgInfo.Rows[0].DataItem, "WIPQTY"));
                    else
                        txtDifQty.Text = "0";
                }
                else
                {
                    if (dgInfo.Rows.Count > 0)
                    {
                        double dTmp = 0;
                        double.TryParse(Util.NVC(DataTableConverter.GetValue(dgInfo.Rows[0].DataItem, "WIPQTY")), out dTmp);


                        txtDifQty.Text = (dTmp - double.Parse(txtChgQty.Text)).ToString("#,###");
                    }
                    else
                    {
                        txtDifQty.Text = (0 - double.Parse(txtChgQty.Text)).ToString("#,###");
                    }
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
        private void ChangeQty()
        {
            try
            {
                ShowLoadingIndicator();

                DataSet indataSet = new DataSet();

                DataTable inTable = indataSet.Tables.Add("INDATA");
                inTable.Columns.Add("SRCTYPE", typeof(string));
                inTable.Columns.Add("IFMODE", typeof(string));
                inTable.Columns.Add("EQPTID", typeof(string));
                inTable.Columns.Add("CPROD_ACT_CODE", typeof(string));
                //inTable.Columns.Add("CPROD_LOTID", typeof(string));
                inTable.Columns.Add("AUTO_PRCS_FLAG", typeof(string));
                inTable.Columns.Add("NOTE", typeof(string));
                inTable.Columns.Add("USERID", typeof(string));

                DataTable in_lotTable = indataSet.Tables.Add("IN_WRK");
                in_lotTable.Columns.Add("CPROD_LOTID", typeof(string));
                in_lotTable.Columns.Add("PRODID", typeof(string));
                in_lotTable.Columns.Add("MKT_TYPE_CODE", typeof(string));
                in_lotTable.Columns.Add("WIPQTY", typeof(decimal));

                DataRow newRow = inTable.NewRow();
                newRow["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                newRow["IFMODE"] = IFMODE.IFMODE_OFF;
                newRow["EQPTID"] = _EqptID;
                newRow["CPROD_ACT_CODE"] = "QTY_CLOT";                
                newRow["AUTO_PRCS_FLAG"] = "N";
                newRow["NOTE"] = new TextRange(rtxChgReason.Document.ContentStart, rtxChgReason.Document.ContentEnd).Text;
                newRow["USERID"] = LoginInfo.USERID;

                inTable.Rows.Add(newRow);

                newRow = in_lotTable.NewRow();
                newRow["CPROD_LOTID"] = "";
                newRow["PRODID"] = Util.NVC(DataTableConverter.GetValue(dgInfo.Rows[0].DataItem, "PRODID"));
                newRow["MKT_TYPE_CODE"] = Util.NVC(DataTableConverter.GetValue(dgInfo.Rows[0].DataItem, "MKT_TYPE_CODE"));
                newRow["WIPQTY"] = txtChgQty.Text.Equals("") ? 0 : decimal.Parse(txtChgQty.Text);

                in_lotTable.Rows.Add(newRow);

                new ClientProxy().ExecuteService_Multi("BR_PRD_REG_CPROD_CELL_WIP", "INDATA,IN_WRK", null, (searchResult, searchException) =>
                {
                    try
                    {
                        if (searchException != null)
                        {
                            Util.MessageException(searchException);
                            return;
                        }

                        Util.MessageInfo("SFU1275");    //정상 처리 되었습니다.

                        this.DialogResult = MessageBoxResult.OK;
                    }
                    catch (Exception ex)
                    {
                        Util.MessageException(ex);
                        //Logger.Instance.WriteLine(Logger.OPERATION_R + "GetProductLot", ex);
                    }
                    finally
                    {
                        HiddenLoadingIndicator();
                        //Logger.Instance.WriteLine(Logger.OPERATION_R + "GetProductLot", Logger.MESSAGE_OPERATION_END);
                    }
                }, indataSet
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
        private bool CanChange()
        {
            bool bRet = false;

            //if (double.Parse(txtDifQty.Text) < 0)
            //{
            //    Util.MessageValidation("SFU4419");  // 변경수량은 재공수량보다 클 수 없습니다.
            //    return bRet;
            //}

            if ((new TextRange(rtxChgReason.Document.ContentStart, rtxChgReason.Document.ContentEnd).Text).Trim().Equals(""))
            {
                Util.MessageValidation("SFU1594");  // 사유를 입력하세요.
                return bRet;
            }

            bRet = true;
            return bRet;
        }
        #endregion

        #region [Func]
        private void ApplyPermissions()
        {
            List<Button> listAuth = new List<Button>();
            listAuth.Add(btnChange);

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
